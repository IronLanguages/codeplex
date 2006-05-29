#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################

import generate
import sys
reload(generate)
from generate import CodeGenerator, CodeWriter

MAX_ARGS = 5

def make_params(nargs, *prefix):
    params = ["object arg%d" % i for i in range(nargs)]
    return ", ".join(list(prefix) + params)

def gen_args_comma(nparams, comma):
    args = ""
    for i in xrange(nparams):
        args = args + comma + ("object arg%d" % i)
        comma = ", "
    return args

def gen_args(nparams):
    return gen_args_comma(nparams, "")

def gen_args_call(nparams):
    args = ""
    comma = ""
    for i in xrange(nparams):
        args = args + comma +("arg%d" % i)
        comma = ", "
    return args

def gen_callargs(nparams):
    args = ""
    comma = ""
    for i in xrange(nparams):
        args = args + comma + ("callArgs[%d]" % i)
        comma = ","
    return args

def gen_args_paramscall(nparams):
    args = ""
    comma = ""
    for i in xrange(nparams):
        args = args + comma + ("args[%d]" % i)
        comma = ","
    return args



def calltargets(cw):
    cw.write("")
    for nparams in range(MAX_ARGS+1):
        cw.write("public delegate object CallTarget%d(%s);" %
                 (nparams, make_params(nparams)))
    cw.write("")

CodeGenerator("CallTargets", calltargets).doit()

def gen_call(nargs, nparams, cw):
    #params = ["object arg%d" % i for i in range(nargs)]
    args = ["arg%d" % i for i in range(nargs)]
    cw.enter_block("public override object Call(%s)" % make_params(nargs))
    
    # first emit error checking...
    ndefaults = nparams-nargs
    if nargs != nparams:    
        cw.write("if (defaults.Length < %d) throw BadArgumentError(%d);" % (ndefaults,nargs))
    
    # emit the common case of no recursion check
    if (nargs == nparams):
        cw.write("if(!EnforceRecursion) return target(%s);" % ", ".join(args))
    else:        
        dargs = args + ["defaults[defaults.Length - %d]" % i for i in range(ndefaults, 0, -1)]
        cw.write("if(!EnforceRecursion) return target(%s);" % ", ".join(dargs))
    
    # emit non-common case of recursion check
    cw.write("PushFrame();")
    cw.enter_block("try")

    # make function body
    if (nargs == nparams):
        cw.write("return target(%s);" % ", ".join(args))
    else:        
        dargs = args + ["defaults[defaults.Length - %d]" % i for i in range(ndefaults, 0, -1)]
        cw.write("return target(%s);" % ", ".join(dargs))
    
    cw.finally_block()
    cw.write("PopFrame();")
    cw.exit_block()
        
    cw.exit_block()

def gen_params_callN(cw, any):
    cw.enter_block("public override object Call(ICallerContext context, params object[] args)")
    cw.write("if (!IsContextAware) return Call(args);")
    cw.write("")
    cw.enter_block("if (Instance == null)")
    cw.write("object[] newArgs = new object[args.Length + 1];")
    cw.write("newArgs[0] = context;")
    cw.write("Array.Copy(args, 0, newArgs, 1, args.Length);")
    cw.write("return Call(newArgs);")
    cw.else_block()
    
    # need to call w/ Context, Instance, *args
    
    if any:
        cw.enter_block("switch (args.Length)")
        for i in xrange(MAX_ARGS-1):
                if i == 0:
                    cw.write(("case %d: if(target2 != null) return target2(context, Instance); break;") % (i))
                else:
                    cw.write(("case %d: if(target%d != null) return target%d(context, Instance, " + gen_args_paramscall(i) + "); break;") % (i, i+2, i+2))
        cw.exit_block()
        cw.enter_block("if (targetN != null)")
        cw.write("object [] newArgs = new object[args.Length+2];")
        cw.write("newArgs[0] = context;")
        cw.write("newArgs[1] = Instance;")
        cw.write("Array.Copy(args, 0, newArgs, 2, args.Length);")
        cw.write("return targetN(newArgs);")
        cw.exit_block()
        cw.write("throw BadArgumentError(args.Length);")
        cw.exit_block()
    else:
        cw.write("object [] newArgs = new object[args.Length+2];")
        cw.write("newArgs[0] = context;")
        cw.write("newArgs[1] = Instance;")
        cw.write("Array.Copy(args, 0, newArgs, 2, args.Length);")
        cw.write("return target(newArgs);")
        cw.exit_block()
        
    cw.exit_block()
    cw.write("")
        

def gen_builtin_function(nparams, cw):
    suffix = str(nparams)
    if(nparams == sys.maxint): suffix = 'N'

    cw.write("""[PythonType("builtin_function_or_method")]""")
    cw.enter_block("public class OptimizedFunction%s : BuiltinFunction" % suffix)
    cw.write("CallTarget%s target = null;" % suffix)
    cw.write("")

    cw.enter_block("public OptimizedFunction%s(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType)" % suffix)
    cw.write("target = IronPython.Compiler.CodeGen.CreateDelegate(info, typeof(CallTarget%s)) as CallTarget%s;" % (suffix, suffix))    
    cw.exit_block()
    
    cw.enter_block("public OptimizedFunction%s(OptimizedFunction%s from) : base(from)" % (suffix,suffix))
    cw.write("target = from.target;")
    cw.exit_block()
    
    if nparams != sys.maxint:
        # normal calls
        
        # instanceless target
        cw.enter_block("public override object Call(" + gen_args(nparams) + ")")
        cw.write("if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(%d" % (nparams) +");")
        cw.write("return target(" + gen_args_call(nparams) + ");")
        cw.exit_block()
        
        # target w/ instance (takes 1 less parameter)
        if nparams != 0:
            cw.enter_block("public override object Call(" + gen_args(nparams-1) + ")")
            cw.write("if(HasInstance)")
            if nparams != 1:
                cw.write("    return target(Instance, " + gen_args_call(nparams-1) + ");")
            else:
                cw.write("    return target(Instance);")
            cw.write("else")
            cw.write("    throw BadArgumentError(%d);" % (nparams-1))
            cw.exit_block()
        
        # calls w/ context
        
        # instanceless
        if nparams != 0: 
            cw.enter_block("public override object Call(ICallerContext context, " + gen_args(nparams) + ")")
        else:
            cw.enter_block("public override object Call(ICallerContext context)")
		
        cw.write("if((this.FunctionType & FunctionType.Function)==0 && HasInstance) throw BadArgumentError(%d" % (nparams) +");")
        cw.write("return target(" + gen_args_call(nparams) + ");")
        cw.exit_block()

        if nparams != 0:
            # target w/ instance (or context), 1 less parameter
            if nparams != 1:
                cw.enter_block("public override object Call(ICallerContext context, " + gen_args(nparams-1) + ")")
                cw.write("if(HasInstance) return target(Instance, " + gen_args_call(nparams-1) + ");")
                cw.write("if(IsContextAware) return target(context, " + gen_args_call(nparams-1) + ");")
            else:
                cw.enter_block("public override object Call(ICallerContext context)")            
                cw.write("if(HasInstance) return target(Instance);")
                cw.write("if(IsContextAware) return target((object)context);")
            cw.write("")
            cw.write("throw BadArgumentError(%d);" % (nparams-1))
            cw.exit_block()
		
		# target w/ instance & context
        if nparams > 1:
            if nparams == 2:
                cw.enter_block("public override object Call(ICallerContext context)")            
                cw.write("if(!HasInstance || !IsContextAware) throw BadArgumentError(%d);" %(nparams-2))
                cw.write("return target(context, Instance);")
            else:
                cw.enter_block("public override object Call(ICallerContext context, " + gen_args(nparams-2) + ")")
                cw.write("if(!HasInstance || !IsContextAware) throw BadArgumentError(%d);" %(nparams-2))
                cw.write("return target(context, Instance, " + gen_args_call(nparams-2) + ");")
            cw.exit_block()
            
    else:
        for i in xrange(MAX_ARGS+1):
            cw.enter_block("public override object Call(" + gen_args(i) + ")")            
            cw.write("if(HasInstance) return target(new object[]{Instance, " + gen_args_call(i) + "});")
            cw.write("return target( new object[]{" + gen_args_call(i) + "});")
            cw.exit_block()
            cw.write("")
            if i != 0:
                cw.enter_block("public override object Call(ICallerContext context, " + gen_args(i) + ")")            
            else:
                cw.enter_block("public override object Call(ICallerContext context)")            
            cw.write("if(HasInstance) return target(new object[]{Instance, " + gen_args_call(i) + "});")
            cw.write("return target( new object[]{" + gen_args_call(i) + "});")
            cw.exit_block()

        cw.write("")
        cw.enter_block("public override object Call(params object[] args)")
        cw.enter_block("if(HasInstance)")
        cw.write("object [] newArgs = new object[args.Length+1];")
        cw.write("newArgs[0] = Instance;")
        cw.write("Array.Copy(args, 0, newArgs, 1, args.Length);")
        cw.write("return target(newArgs);")
        cw.exit_block()
        cw.write("return target(args);")
        cw.exit_block()
        
        gen_params_callN(cw, False)
    
    cw.enter_block("protected override Delegate[] OptimizedTargets")
    cw.enter_block("get")
    cw.write("return new Delegate[] { target };")
    cw.exit_block()
    cw.exit_block()

    cw.enter_block("public override int GetMaximumArguments()")
    if(nparams != sys.maxint):
        cw.write("return %d;" % nparams)
    else:
        cw.write("return int.MaxValue;")
              
    cw.exit_block()
    
    cw.enter_block("public override BuiltinFunction Clone()")
    cw.write("return new OptimizedFunction%s(this);" % (suffix))
    cw.exit_block()

    
    cw.exit_block()
    
    

def gen_builtin_function_any(cw):
    cw.write("""[PythonType("builtin_function_or_method")]""")
    cw.enter_block("public class OptimizedFunctionAny : BuiltinFunction")
    cw.write("CallTargetN targetN;")
    for i in xrange(MAX_ARGS+1):
        cw.write("CallTarget%d target%d;" %(i, i))

    cw.enter_block("public OptimizedFunctionAny(string name, MethodInfo[] infos, MethodBase[] targets, FunctionType functionType) : base(name, targets, functionType)")
    cw.enter_block("for (int i = 0; i < infos.Length; i++)")
    cw.write("Debug.Assert(infos[i].IsStatic);")
    cw.write("")
    cw.enter_block("switch(infos[i].GetParameters().Length)")
    for i in xrange(MAX_ARGS+1):
        if(i != 1):
            cw.write("case %(i)d: target%(i)d = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTarget%(i)d)) as CallTarget%(i)d; break;" %{'i': i})
        else:
            cw.write("case 1:")
            cw.write("if (!infos[i].GetParameters()[0].ParameterType.HasElementType) ")
            cw.write("    target1 = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTarget1)) as CallTarget1;")
            cw.write("else")
            cw.write("    targetN = IronPython.Compiler.CodeGen.CreateDelegate(infos[i], typeof(CallTargetN)) as CallTargetN;")
            cw.write("break;")
    cw.exit_block()
    cw.exit_block()
    cw.exit_block()

    cw.enter_block("public OptimizedFunctionAny(OptimizedFunctionAny from) : base(from)")
    for i in xrange(MAX_ARGS+1):
        cw.write("target%d = from.target%d;" % (i, i))
    cw.write("targetN = from.targetN;")
    cw.exit_block()

    for i in xrange(MAX_ARGS+1):
        cw.enter_block("public override object Call(" + gen_args(i) + ")")
        cw.enter_block("if(HasInstance)")
        if i == 0:
            cw.write("if(target1 != null) return target1(Instance);")
        elif i == MAX_ARGS:
            cw.write( ("if(targetN != null) return targetN(new object[]{ Instance, " +  gen_args_call(i) + "});") )
        else:
            cw.write( ("if(target%d != null) return target%d(Instance, " + gen_args_call(i) + ");") % (i+1, i+1) )
        cw.write("if (targetN != null) return targetN(new object[] {Instance, " + gen_args_call(i) + " });")
        cw.write("throw BadArgumentError(%d);" % (i))
        cw.exit_block()
            
        cw.write( ("if (target%d != null) return target%d(" + gen_args_call(i) + ");") % (i,i) )
        cw.write("else if (targetN != null) return targetN(new object[] { " + gen_args_call(i) + " });")
        cw.write("else throw BadArgumentError(%d);" % i)
        cw.exit_block()
    
    cw.enter_block("public override object Call(params object[] args)")
    cw.enter_block("if(Instance == null)")
    
    cw.enter_block("switch (args.Length)")
    for i in xrange(MAX_ARGS+1):
            cw.write(("case %d: return Call(" + gen_args_paramscall(i) + ");") % i)
    cw.exit_block()
    cw.write("if (targetN != null) return targetN(args);")
    
    cw.else_block("")
    
    cw.enter_block("switch (args.Length)")
    for i in xrange(MAX_ARGS+1):
            if i == 0:
                cw.write(("case %d: if(target1 != null) return target1(Instance); break;") % (i))
            elif i == MAX_ARGS:
                cw.write(("case %d: if(targetN != null) return targetN(new object[]{ Instance, " + gen_args_paramscall(i) + "}); break;") % i)
            else:
                cw.write(("case %d: if(target%d != null) return target%d(Instance, " + gen_args_paramscall(i) + "); break;") % (i, i+1, i+1))
    cw.exit_block()
    cw.enter_block("if (targetN != null)")
    cw.write("object [] newArgs = new object[args.Length+1];")
    cw.write("newArgs[0] = Instance;")
    cw.write("Array.Copy(args, 0, newArgs, 1, args.Length);")
    cw.write("return targetN(newArgs);")
    cw.exit_block()

    cw.exit_block()
    cw.write("throw BadArgumentError(args.Length);")
    cw.exit_block()

    cw.enter_block("public override object Call(ICallerContext context)")
    cw.write("if(!IsContextAware) return Call();")
    
    cw.enter_block("if(HasInstance)")
    cw.write("if(target2 != null) return target2(context, Instance);")
    cw.write("else if(targetN != null) return targetN(new object[]{context, Instance});")
    cw.write("throw BadArgumentError(0);")    
    cw.exit_block()
    
    cw.write("return Call((object)context);")
    cw.exit_block()
    cw.write("")
    
    for i in xrange(MAX_ARGS-1):
        cw.enter_block("public override object Call(ICallerContext context, " + gen_args(i+1) +")")
        cw.write("if(!IsContextAware) return Call(" + gen_args_call(i+1) + ");")
        cw.write("if(!HasInstance) return Call((object)context, " + gen_args_call(i+1) + ");")
        # instance & context
        
        if i+3 < MAX_ARGS:
            cw.write( ("if(target%d != null) return target%d(context, Instance, " + gen_args_call(i+1) + ");") % (i+3, i+3) )
            
        cw.write("if (targetN != null) return targetN(new object[] {context, Instance, " + gen_args_call(i+1) + " });")
        cw.write("throw BadArgumentError(%d);" % (i))

        cw.exit_block()
        cw.write("")
        
    cw.enter_block("public override object Call(ICallerContext context, " + gen_args(MAX_ARGS) +")")
    cw.write("if(IsContextAware) return Call(new object[]{context, " + gen_args_call(MAX_ARGS) + "});")
    cw.write("return Call(" + gen_args_call(MAX_ARGS) + ");")
    cw.exit_block()
    cw.write("")
    
    gen_params_callN(cw, True)
    
    cw.enter_block("protected override Delegate[] OptimizedTargets")
    cw.enter_block("get")
    cw.enter_block("Delegate[] delegates = new Delegate[]")
    for i in xrange(MAX_ARGS+1):
        cw.write("target%d," % i)
    cw.write("targetN")
    cw.exit_block()
    cw.write(";")
    cw.write("return delegates;")
    cw.exit_block()
    cw.exit_block()

    cw.enter_block("public override int GetMaximumArguments()")
    cw.write("if (targetN != null) return int.MaxValue;")
    for i in reversed(xrange(MAX_ARGS+1)):
        if i != 0: cw.write("if (target%d != null) return %d;" % (i,i))
    cw.write("return 0;")
    cw.exit_block()
    
    cw.enter_block("public override BuiltinFunction Clone()")
    cw.write("return new OptimizedFunctionAny(this);")
    cw.exit_block()

    cw.exit_block()


def gen_function(nparams, cw):
    cw.write("""[PythonType(typeof(PythonFunction))]""")
    cw.enter_block("public class Function%d : PythonFunction" % nparams)
    cw.write("private CallTarget%d target;" % nparams)
    cw.write("public Function%(nparams)d(PythonModule globals, string name, CallTarget%(nparams)s target, string[] argNames, object[] defaults)", nparams=nparams)
    cw.enter_block("    : base(globals, name, argNames, defaults)")
    cw.write("this.target = target;")
    cw.exit_block()

#    cw.write("public override MethodInfo GetMethod() { return target.Method; }")

    for nargs in range(nparams+1):
        gen_call(nargs, nparams, cw)

    cw.enter_block("public override object Call(params object[] args)")
    cw.enter_block("switch (args.Length)")
    for nargs in range(nparams+1):
        args = ["args[%d]" % i for i in range(nargs)]
        cw.write("case %d: return Call(%s);" % (nargs, ", ".join(args)))
    cw.write("default: throw BadArgumentError(args.Length);")
    cw.exit_block()
    cw.exit_block()

    cw.enter_block("public override bool Equals(object obj)")
    cw.write("Function%(nparams)d other = obj as Function%(nparams)d;" % {'nparams':nparams})
    cw.write("if (other == null) return false;")
    cw.write("")
    cw.write("return target == other.target;")
    cw.exit_block()

    cw.enter_block("public override int GetHashCode()")
    cw.write("    return target.GetHashCode();")
    cw.exit_block()

    cw.enter_block("public override object Clone()")
    cw.write("    return new Function%d(Module, Name, target, argNames, defaults);" % nparams)
    cw.exit_block()

    cw.exit_block()

def functions(cw):
    cw.write("")
    for nparams in range(MAX_ARGS+1):
        gen_function(nparams, cw)
        cw.write("")

def builtin_functions(cw):
    cw.write("")
    for nparams in range(MAX_ARGS+1):
        gen_builtin_function(nparams, cw)
        cw.write("")
    gen_builtin_function(sys.maxint, cw)
    gen_builtin_function_any(cw)


CodeGenerator("FunctionNs", functions).doit()
CodeGenerator("Builtin Functions", builtin_functions).doit()

##def gen_invoke_meth(nargs, cw):
##    params = ["PyObject arg%d" % i for i in range(nargs)]
##    args = ["arg%d" % i for i in range(nargs)]
##    params.insert(0, "PyString name")
##    cw.enter_block("public virtual PyObject invoke(%s)" %
##                   ", ".join(params))
##    cw.write("return __getattr__(name).__call__(%s);" %
##             ", ".join(args))
##    cw.exit_block()

def gen_methods(cw):
    for nparams in range(MAX_ARGS+1):
        cw.write("object Call(%s);" % make_params(nparams))

CodeGenerator("FastCallable methods", gen_methods).doit()

CODE = """
public static object Call(%(params)s) {
    PythonFunction f = func as PythonFunction;
    if (f != null) return f.Call(%(args)s);

    IFastCallable ifc = func as IFastCallable;
    if (ifc != null) return ifc.Call(%(args)s);

    ICallable ic = func as ICallable;
    if (ic != null) return ic.Call(%(argsArray)s);

    return Ops.Call(func, %(argsArray)s);
}"""


def gen_call_meth(nargs, cw):
    args = ["arg%d" % i for i in range(nargs)]
    argsArray = "EMPTY"
    if nargs > 0:
        argsArray = "new object[] { %s }" % ", ".join(args)

    cw.write(CODE, params=make_params(nargs, 'object func'), args=", ".join(args),
             argsArray = argsArray)

def gen_callcontext_meth(nparams, cw):
    cw.enter_block("public static object CallWithContext(ICallerContext context, object func" + gen_args_comma(nparams, ", ") + ")")
    cw.write("BuiltinFunction bf = func as BuiltinFunction;")
    if nparams != 0:
        cw.write("if (bf != null) return bf.Call(context, "+ gen_args_call(nparams) +");")
    else: 
        cw.write("if (bf != null) return bf.Call(context);")
    cw.write("")
    cw.write("PythonFunction f = func as PythonFunction;")
    cw.write("if (f != null) return f.Call(" + gen_args_call(nparams) +");")
    cw.write("")
    cw.write("IFastCallable ifc = func as IFastCallable;")
    cw.write("if (ifc != null) return ifc.Call(" + gen_args_call(nparams) + ");")
    cw.write("")
    if nparams == 0:
        cw.write("return Ops.CallWithContext(context, func, EMPTY);")
    else:
        cw.write("return Ops.CallWithContext(context, func, new object[]{" + gen_args_call(nparams) + "});")
    cw.exit_block()
    cw.write("")

def gen_methods(cw):
    cw.write("public const int MaximumCallArgs = %s;" % MAX_ARGS)

    for nparams in range(MAX_ARGS+1):
        gen_call_meth(nparams, cw)

    cw.write("")

    for nparams in range(MAX_ARGS+1):
        gen_callcontext_meth(nparams, cw)

CodeGenerator("Call Ops", gen_methods).doit()




def gen_targets(cw):
    for nargs in range(MAX_ARGS+1):
        cw.write("public virtual object Call("+ gen_args(nargs) + ") { throw BadArgumentError(%d); }" % nargs)

    cw.enter_block("public virtual object Call(params object[] args)")
    cw.enter_block("switch(args.Length)")
    for nargs in range(MAX_ARGS+1):
        cw.write("case %d: return Call(%s);" %
                 (nargs, ", ".join(["args[%d]" % i for i in range(nargs)])))
    cw.exit_block()
    cw.write("throw BadArgumentError(args.Length);")
    cw.exit_block()

    cw.enter_block("private static BuiltinFunction MakeBuiltinFunction(string name, MethodInfo info, MethodBase[] targets, FunctionType functionType)")
    cw.enter_block("switch (info.GetParameters().Length)")
    for nargs in range(MAX_ARGS+1):
        cw.write("case %d: return new OptimizedFunction%d(name, info, targets, functionType);" % (nargs,nargs))
    cw.exit_block()
    cw.write("throw new InvalidOperationException(\"too many args\");")
    cw.exit_block()
    
def gen_context_targets(cw):
    for nargs in range(MAX_ARGS+1):
        if nargs != 0:
            cw.write("public virtual object Call(ICallerContext context, "+ gen_args(nargs) + ") { throw BadArgumentError(%d); }" % nargs)
        else:
            cw.write("public virtual object Call(ICallerContext context) { throw BadArgumentError(%d); }" % nargs)
    
    cw.enter_block("public virtual object Call(ICallerContext context, params object[] args)")
    cw.enter_block("switch(args.Length)")
    for nargs in range(MAX_ARGS+1):
        if nargs != 0:
            cw.write("case %d: return Call(context, %s);" %
                 (nargs, ", ".join(["args[%d]" % i for i in range(nargs)])))
        else:
            cw.write("case 0: return Call(context);")
            
    cw.exit_block()
    cw.write("throw BadArgumentError(args.Length);")
    cw.exit_block()

CodeGenerator("BuiltinFunction targets", gen_targets).doit()

CodeGenerator("BuiltinFunction context targets", gen_context_targets).doit()

def gen_function_fastcall(cw):
    for i in xrange(MAX_ARGS+1):
        cw.enter_block("public virtual object Call(" + gen_args(i) + ")")
        cw.write("throw BadArgumentError(%d);" % i)
        cw.exit_block()

CodeGenerator("Function IFastCallable Members", gen_function_fastcall).doit()


def gen_funcdef(cw):
    cw.enter_block("private void GetFunctionType(out Type ft, out Type tt)")
    cw.enter_block("if (flags == FuncDefType.None)")
    cw.enter_block("if (parameters.Length <= Ops.MaximumCallArgs)")
    cw.enter_block("switch (parameters.Length)")
    for i in xrange(MAX_ARGS+1):
        cw.write("case %d: ft = typeof(Function%d); tt = typeof(CallTarget%d); break;" % (i, i, i))
    cw.write("default: ft = typeof(FunctionN); tt = typeof(CallTargetN); break;")
    cw.exit_block()
    cw.else_block()
    cw.write("ft = typeof(FunctionN); tt = typeof(CallTargetN);")
    cw.exit_block()
    cw.else_block()
    cw.write("ft = typeof(FunctionX); tt = typeof(CallTargetN);")
    cw.exit_block()
    cw.exit_block()

CodeGenerator("FuncDef Code", gen_funcdef).doit()


def gen_functionN_fastcall(cw):
    for i in xrange(MAX_ARGS+1):
        cw.enter_block("public override object Call(" + gen_args(i) + ")")
        cw.write("return Call(new object[] { " + gen_args_call(i) + "});")
        cw.exit_block()
        cw.write("")

CodeGenerator("FunctionN IFastCallable Members", gen_functionN_fastcall).doit()

def gen_method_fastcall(cw):
    for i in xrange(MAX_ARGS+1):
        cw.enter_block("public object Call(" + gen_args(i) + ")")
        if(i != 0):
            cw.write("PythonFunction f = func as PythonFunction;")
            cw.enter_block("if(inst != null)")
            cw.write("if (f != null) return f.Call(inst, "+ gen_args_call(i) + ");")
            cw.write("return Ops.Call(func, inst, " + gen_args_call(i) +");")
            cw.else_block("")
            cw.write("if (!Modules.Builtin.IsInstance(arg0, DeclaringClass)) throw BadSelf(arg0);")
            cw.write("if (f != null) return f.Call("+ gen_args_call(i) + ");")
            cw.write("return Ops.Call(func, " + gen_args_call(i) +");")            
            cw.exit_block()
        else:
            cw.write("if (inst != null) return Ops.Call(func, inst);")
            cw.write("throw BadSelf(null);")
            
        cw.exit_block()
        cw.write("")

CodeGenerator("Method IFastCallable Members", gen_method_fastcall).doit()


def gen_reflectedmethod_contextcall(cw):
    cw.enter_block("public override object Call(ICallerContext context)")
    cw.write("return Call(context, new object[0]);")
    cw.exit_block()
    
    for i in xrange(MAX_ARGS):
        param = i+1
        cw.enter_block("public override object Call(ICallerContext context, " + gen_args(param) + ")")
        cw.write("return Call(context, new object[]{ " + gen_args_call(param) + "});")
        cw.exit_block()

def gen_reflectedmethod(cw):    
    for i in xrange(MAX_ARGS+1):
        cw.enter_block("public override object Call(" + gen_args(i) + ")")
        cw.write("return Call(new object[]{ " + gen_args_call(i) + "});")
        cw.exit_block()
            

CodeGenerator("ReflectedMethod context targets", gen_reflectedmethod_contextcall).doit()

CodeGenerator("ReflectedMethod IFastCallable", gen_reflectedmethod).doit()
