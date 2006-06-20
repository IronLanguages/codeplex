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

def make_params1(nargs, prefix=("ICallerContext context",)):
    params = ["object arg%d" % i for i in range(nargs)]
    return ", ".join(list(prefix) + params)

def make_args1(nargs, prefix, start=0):
    args = ["arg%d" % i for i in range(start, nargs)]
    return ", ".join(list(prefix) + args)

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

    cw.write("")
    for nparams in range(MAX_ARGS+1):
        cw.write("public delegate object CallTargetWithContext%d(%s);" %
                 (nparams, make_params(nparams, "ICallerContext context")))
    cw.write("")

CodeGenerator("CallTargets", calltargets).doit()

def get_call_type(postfix):
    if postfix == "": return "CallType.None"
    else: return "CallType.ImplicitInstance"


def make_call_to_target(cw, index, postfix, extraArg):
        cw.enter_block("public override object Call%(postfix)s(%(params)s)", postfix=postfix,
                       params=make_params1(index))
        cw.write("if (target%(index)d != null) return target%(index)d(%(args)s);", index=index,
                 args = make_args1(index, extraArg))
        cw.write("throw BadArgumentError(%(callType)s, %(nargs)d);", callType=get_call_type(postfix), nargs=index)
        cw.exit_block()

def make_call_to_targetX(cw, index, postfix, extraArg):
        cw.enter_block("public override object Call%(postfix)s(%(params)s)", postfix=postfix,
                       params=make_params1(index))
        cw.write("return target%(index)d(%(args)s);", index=index, args = make_args1(index, extraArg))
        cw.exit_block()

def make_error_calls(cw, index):
        cw.enter_block("public override object Call(%(params)s)", params=make_params1(index))
        cw.write("throw BadArgumentError(CallType.None, %(nargs)d);", nargs=index)
        cw.exit_block()

        if index > 0:
            cw.enter_block("public override object CallInstance(%(params)s)", params=make_params1(index))
            cw.write("throw BadArgumentError(CallType.ImplicitInstance, %(nargs)d);", nargs=index)
            cw.exit_block()
    
    

def gen_fastcallable_any(cw, postfix="WithContext", extraParam=("ICallerContext context",), extraArg=("context",)):
    cw.enter_block("public class FastCallable%(postfix)sAny : FastCallable", postfix=postfix)
    for i in xrange(MAX_ARGS + 1):
        cw.write("public CallTarget%(postfix)s%(index)d target%(index)d;", index=i, postfix=postfix)
    cw.write("public CallTarget%(postfix)sN targetN;", postfix=postfix)


    cw.write("private string name;")    
    cw.write("private int minArgs, maxArgs;")
    cw.enter_block("public FastCallable%(postfix)sAny(string name, int minArgs, int maxArgs)", postfix=postfix)
    cw.write("this.name = name;")
    cw.write("this.minArgs = minArgs;")
    cw.write("this.maxArgs = maxArgs;")
    cw.exit_block()

    for i in xrange(MAX_ARGS + 1):
        make_call_to_target(cw, i, "", extraArg)

    for i in xrange(1, MAX_ARGS + 1):
        make_call_to_target(cw, i, "Instance", extraArg)


    cw.enter_block("public override object Call(%(params)s)", params=", ".join(extraParam+("params object[] args",)))
    cw.enter_block("switch(args.Length)")
    for i in xrange(MAX_ARGS+1):
        args = ", ".join(["context"]+["args[%d]" % ai for ai in xrange(i)])
        cw.write("case %(index)s: return Call(%(args)s);", index=i, args=args)
    cw.exit_block()
    cw.write("if (targetN != null) return targetN(%(args)s);", args = ", ".join(extraArg+("args",)))
    cw.write("throw BadArgumentError(CallType.None, args.Length);")
    cw.exit_block()

    
    cw.enter_block("public override object CallInstance(%(params)s)", params=", ".join(extraParam+("object instance", "params object[] args",)))
    cw.enter_block("switch(args.Length)")
    for i in xrange(MAX_ARGS):
        args = ", ".join(["context", "instance",]+["args[%d]" % ai for ai in xrange(i)])
        cw.write("case %(index)s: return CallInstance(%(args)s);", index=i, args=args)
    cw.exit_block()
    
    cw.write("if (targetN != null) return targetN(%(args)s);", args = ", ".join(extraArg+("PrependInstance(instance, args)",)))
    cw.write("throw BadArgumentError(CallType.None, args.Length+1);")
    cw.exit_block()
    
    cw.enter_block("private Exception BadArgumentError(CallType callType, int nargs)")
    cw.write("return BadArgumentError(name, minArgs, maxArgs, callType, nargs);")
    cw.exit_block()
        
    cw.exit_block()

def gen_fastcallable_x(cw, index, postfix="WithContext", extraArg=("context",)):
    cw.enter_block("public class FastCallable%(postfix)s%(index)d : FastCallable", postfix=postfix, index=index)
    cw.write("public CallTarget%(postfix)s%(index)d target%(index)d;", index=index, postfix=postfix)
    cw.write("private string name;")
    cw.enter_block("public FastCallable%(postfix)s%(index)d(string name, CallTarget%(postfix)s%(index)d target)",
             index=index, postfix=postfix)
    cw.write("this.target%(index)d = target;", index=index)
    cw.write("this.name = name;")
    cw.exit_block()

    for i in xrange(MAX_ARGS+1):
        if i == index:
            make_call_to_targetX(cw, index, "", extraArg)
            if index > 0:
                make_call_to_targetX(cw, index, "Instance", extraArg)
        else:
            make_error_calls(cw, i)

    cw.enter_block("public override object Call(ICallerContext context, params object[] args)")
    cw.write("if (args.Length == %(index)d) return Call(%(args)s);", index=index,
             args=", ".join(["context"]+["args[%d]" % i for i in xrange(index)]))
    cw.write("throw BadArgumentError(CallType.None, args.Length);")
    cw.exit_block()

    cw.enter_block("public override object CallInstance(ICallerContext context, object instance, params object[] args)")
    cw.write("if (args.Length == %(index)d) return CallInstance(%(args)s);", index=index-1,
             args=", ".join(["context", "instance"] + ["args[%d]" % i for i in xrange(index-1)]))
    cw.write("throw BadArgumentError(CallType.ImplicitInstance, args.Length);")
    cw.exit_block()
    
    cw.enter_block("private Exception BadArgumentError(CallType callType, int nargs)")
    cw.write("return BadArgumentError(name, %(index)s, %(index)s, callType, nargs);", index=index)
    cw.exit_block()

    cw.exit_block()

def gen_fastcallables(cw):
    gen_fastcallable_any(cw, postfix="", extraParam=("ICallerContext context",), extraArg=())
    gen_fastcallable_any(cw, postfix="WithContext", extraParam=("ICallerContext context",), extraArg=("context",))

    for i in xrange(MAX_ARGS+1):
        gen_fastcallable_x(cw, i, postfix="", extraArg=())
        gen_fastcallable_x(cw, i, postfix="WithContext", extraArg=("context",))

CodeGenerator("ConcreteFastCallables", gen_fastcallables).doit()


def gen_fastmakers(cw, postfix):
    cw.enter_block("switch (nargs)")
    for i in xrange(MAX_ARGS+1):
        cw.write("case %(index)d: return new FastCallable%(postfix)s%(index)d(name, (CallTarget%(postfix)s%(index)d)target);",
                 index = i, postfix=postfix)
    cw.exit_block()

def gen_fastmaker(cw):
    cw.enter_block("public static FastCallable Make(string name, bool needsContext, int nargs, Delegate target)")
    cw.enter_block("if (needsContext)")
    gen_fastmakers(cw, "WithContext")
    cw.exit_block()
    cw.enter_block("else")
    gen_fastmakers(cw, "")
    cw.exit_block()
    cw.write("throw new NotImplementedException();")
    cw.exit_block()

def gen_fastmethods(cw):
    for i in xrange(MAX_ARGS+1):
        cw.write("public abstract object Call(%(params)s);", params=make_params1(i))
        if i > 0:
            cw.write("public abstract object CallInstance(%(params)s);", params=make_params1(i))
    cw.enter_block("public object CallInstance(%(params)s)", params=make_params1(MAX_ARGS+1))
    cw.write("return CallInstance(context, arg0, new object[] { %(args)s });",
             args=", ".join(["arg%d" % (i+1) for i in xrange(MAX_ARGS)]))
    cw.exit_block()


def gen_fastmembers(cw):
    gen_fastmaker(cw)
    gen_fastmethods(cw)

CodeGenerator("FastCallableMembers", gen_fastmembers).doit()

def gen_rfastmethods(cw):
    cw.enter_block("public override object Call(" + make_params1(0) + ")")
    cw.write("return target.Call(context);")
    cw.exit_block()

    for i in xrange(1, MAX_ARGS+1):
        args = ["arg%d" % argi for argi in xrange(i)]
        if len(args) > 1:
            args[0], args[1] = args[1], args[0]
        args = ", ".join(["context"]+args)
		
        cw.enter_block("public override object Call(" + make_params1(i) + ")")
        cw.write("return target.Call(%(args)s);", args=args)
        cw.exit_block()
        
        cw.enter_block("public override object CallInstance(" + make_params1(i) + ")")
        cw.write("return target.CallInstance(%(args)s);", args=args)
        cw.exit_block()

CodeGenerator("ReversedFastCallableWrapper Members", gen_rfastmethods).doit()


def gen_call(nargs, nparams, cw):
    args = ["arg%d" % i for i in range(nargs)]
    cw.enter_block("public override object Call(%s)" % make_params1(nargs))
    
    # first emit error checking...
    ndefaults = nparams-nargs
    if nargs != nparams:    
        cw.write("if (Defaults.Length < %d) throw BadArgumentError(%d);" % (ndefaults,nargs))
    
    # emit the common case of no recursion check
    if (nargs == nparams):
        cw.write("if(!EnforceRecursion) return target(%s);" % ", ".join(args))
    else:        
        dargs = args + ["Defaults[Defaults.Length - %d]" % i for i in range(ndefaults, 0, -1)]
        cw.write("if(!EnforceRecursion) return target(%s);" % ", ".join(dargs))
    
    # emit non-common case of recursion check
    cw.write("PushFrame();")
    cw.enter_block("try")

    # make function body
    if (nargs == nparams):
        cw.write("return target(%s);" % ", ".join(args))
    else:        
        dargs = args + ["Defaults[Defaults.Length - %d]" % i for i in range(ndefaults, 0, -1)]
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

    cw.enter_block("public override object Call(ICallerContext context, params object[] args)")
    cw.enter_block("switch (args.Length)")
    for nargs in range(nparams+1):
        args = ["args[%d]" % i for i in range(nargs)]
        cw.write("case %d: return Call(%s);" % (nargs, ", ".join(["context"]+args)))
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
    cw.write("    return new Function%d(Module, Name, target, ArgNames, Defaults);" % nparams)
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


CODE = """
public static object Call(%(params)s) {
    FastCallable fc = func as FastCallable;
    if (fc != null) return fc.Call(%(args)s);

    return Ops.Call(func, %(argsArray)s);
}"""


def gen_call_meth(nargs, cw):
    args = ["DefaultContext.Default"]+["arg%d" % i for i in range(nargs)]
    argsArray = "EMPTY"
    if nargs > 0:
        argsArray = "new object[] { %s }" % ", ".join(args[1:])

    cw.write(CODE, params=make_params(nargs, 'object func'), args=", ".join(args),
             argsArray = argsArray)

def gen_callcontext_meth(nparams, cw):
    cw.enter_block("public static object CallWithContext(ICallerContext context, object func" + gen_args_comma(nparams, ", ") + ")")
    cw.write("FastCallable fc = func as FastCallable;")
    cw.write("if (fc != null) return fc.Call("+ make_args1(nparams, ["context"]) +");")
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



def gen_function_fastcall(cw):
    for i in xrange(MAX_ARGS+1):
        cw.enter_block("public override object Call(" + make_params1(i) + ")")
        cw.write("throw BadArgumentError(%d);" % i)
        cw.exit_block()
    for i in xrange(1, MAX_ARGS+1):
        cw.enter_block("public override object CallInstance(" + make_params1(i) + ")")
        cw.write("return Call(%(args)s);", args=make_args1(i, ["context"]))
        cw.exit_block()

CodeGenerator("Function FastCallable Members", gen_function_fastcall).doit()


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
        cw.enter_block("public override object Call(" + make_params1(i) + ")")
        cw.write("return Call(new object[] { " + gen_args_call(i) + "});")
        cw.exit_block()
        cw.write("")

CodeGenerator("FunctionN FastCallable Members", gen_functionN_fastcall).doit()

def gen_method_fastcall(cw):
    for i in xrange(MAX_ARGS+1):
        cw.enter_block("public override object Call(" + make_params1(i) + ")")
        cw.write("FastCallable fc = func as FastCallable;")
        cw.enter_block("if (fc != null)")
        cw.write("if (inst != null) return fc.CallInstance(%(args)s);", args=make_args1(i, ["context", "inst"]))
        if i > 0:
            cw.write("else return fc.Call(%(args)s);", args=make_args1(i, ["context", "CheckSelf(arg0)"], start=1))
        else:
            cw.write("else return fc.Call(%(args)s);", args=make_args1(i, ["context"]))
        cw.else_block("")
        cw.write("if (inst != null) return Ops.CallWithContext(%(args)s);", args=make_args1(i, ["context", "func", "inst"]))
        if i > 0:
            cw.write("return Ops.CallWithContext(%(args)s);", args=make_args1(i, ["context", "func", "CheckSelf(arg0)"], start=1))    
        else:
            cw.write("throw BadSelf(null);")
                
        cw.exit_block()
        cw.exit_block()
        cw.write("")

CodeGenerator("Method FastCallable Members", gen_method_fastcall).doit()


def gen_reflectedmethod_contextcall(cw):
    cw.enter_block("public override object Call(ICallerContext context)")
    cw.write("if (HasInstance) return OptimizedTarget.CallInstance(context, Instance);")
    cw.write("else return OptimizedTarget.Call(context);")
    cw.exit_block()
    
    for i in xrange(MAX_ARGS):
        param = i+1
        cw.enter_block("public override object Call(ICallerContext context, " + gen_args(param) + ")")
        cw.write("if (HasInstance) return OptimizedTarget.CallInstance(context, Instance, %s);" % gen_args_call(param))
        cw.write("else return OptimizedTarget.Call(context, %s);" % gen_args_call(param))
        cw.exit_block()
        cw.enter_block("public override object CallInstance(ICallerContext context, " + gen_args(param) + ")")
        cw.write("if (HasInstance) return OptimizedTarget.CallInstance(context, Instance, %s);" % gen_args_call(param))
        cw.write("else return OptimizedTarget.CallInstance(context, %s);" % gen_args_call(param))
        cw.exit_block()

def gen_builtin_targets(cw):
    for i in xrange(MAX_ARGS+1):
        cw.enter_block("public override object Call(%(params)s)", params=make_params1(i))
        cw.write("return OptimizedTarget.Call(%(args)s);", args=make_args1(i, ["context"]))
        cw.exit_block()

        if i == 0: continue
        cw.enter_block("public override object CallInstance(%(params)s)", params=make_params1(i))
        cw.write("return OptimizedTarget.CallInstance(%(args)s);", args=make_args1(i, ["context"]))
        cw.exit_block()
        
def gen_boundbuiltin_targets(cw):
    for i in xrange(MAX_ARGS+1):
        cw.enter_block("public override object Call(%(params)s)", params=make_params1(i))
        cw.write("return target.OptimizedTarget.CallInstance(%(args)s);", args=make_args1(i, ["context", "instance"]))
        cw.exit_block()

        if i == 0: continue
        cw.enter_block("public override object CallInstance(%(params)s)", params=make_params1(i))
        cw.write("return target.OptimizedTarget.CallInstance(%(args)s);", args=make_args1(i, ["context", "instance"]))
        cw.exit_block()

            

CodeGenerator("BuiltinFunction targets", gen_builtin_targets).doit()

CodeGenerator("BoundBuiltinFunction targets", gen_boundbuiltin_targets).doit()



