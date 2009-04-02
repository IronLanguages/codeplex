#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

import sys
from generate import generate

MAX_ARGS = 16

def make_params(nargs, *prefix):
    params = ["object arg%d" % i for i in range(nargs)]
    return ", ".join(list(prefix) + params)

def make_params1(nargs, prefix=("CodeContext context",)):
    params = ["object arg%d" % i for i in range(nargs)]
    return ", ".join(list(prefix) + params)

def make_args(nargs, *prefix):
    params = ["arg%d" % i for i in range(nargs)]
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

def gen_args_call(nparams, *prefix):
    args = ""
    comma = ""
    for i in xrange(nparams):
        args = args + comma +("arg%d" % i)
        comma = ", "
    if prefix:
        if args:
            args = prefix[0] + ', ' + args
        else:
            args = prefix[0]
    return args

def gen_args_array(nparams):
    args = gen_args_call(nparams)
    if args: return "{ " + args + " }"
    else: return "{ }"

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

builtin_function_switch_template = """case %(argCount)d:
    if (IsUnbound) {
        return typeof(BuiltinFunctionCaller<%(typeParams)s>);
    }
    return typeof(BuiltinMethodCaller<%(typeParams)s>);"""

def builtin_function_callers_switch(cw):
    for nparams in range(MAX_ARGS-2):        
        cw.write(builtin_function_switch_template % {
                  'argCount' : nparams,
                  'typeParams' : ',' * nparams,
                  'dlgParams' : ',' * (nparams + 3),
                 })

builtin_function_caller_template = """class BuiltinFunctionCaller<TFuncType, %(typeParams)s> where TFuncType : class {
    private readonly CallSite<Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object>> _site;
    private readonly OptimizingInfo _info;
    public readonly Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object> MyDelegate;
    private readonly BuiltinFunction _func;
%(typeVars)s

    public BuiltinFunctionCaller(CallSite<Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object>> site, OptimizingInfo info, BuiltinFunction func, %(typeCheckParams)s) {
        _func = func;
        _info = info;
        _site = site;
        MyDelegate = new Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object>(Call%(argCount)d);
%(argsAssign)s
    }

    public object Call%(argCount)d(CallSite site, CodeContext context, TFuncType func, %(callParams)s) {
        if (_info.OptimizedDelegate == null &&
            func == _func && 
%(typeCheck)s
           ) {
            bool shouldOptimize;
            object res = _info.Caller(new object[] { context, %(callArgs)s }, out shouldOptimize);

            if (shouldOptimize) {
                _info.OptimizedDelegate = _info.GetInvokeBinder(context).Optimize(_site, new object[] { context, func, %(callArgs)s });
            }

            return res;
        }

        return ((CallSite<Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object>>)site).Update(site, context, func, %(callArgs)s);
    }
}

class BuiltinMethodCaller<TFuncType, %(typeParams)s> where TFuncType : class {
    private readonly CallSite<Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object>> _site;
    private readonly OptimizingInfo _info;
    public readonly Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object> MyDelegate;
    private readonly Type _selfType;
    private readonly BuiltinFunctionData _data;
%(typeVars)s

    public BuiltinMethodCaller(CallSite<Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object>> site, OptimizingInfo info, BuiltinFunction func, Type selfType, %(typeCheckParams)s) {
        _selfType = selfType;
        _data = func._data;
        _info = info;
        _site = site;
        MyDelegate = new Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object>(Call%(argCount)d);
%(argsAssign)s
    }

    public object Call%(argCount)d(CallSite site, CodeContext context, TFuncType func, %(callParams)s) {
        BuiltinFunction bf = func as BuiltinFunction;
        if (_info.OptimizedDelegate == null &&
            bf != null && !bf.IsUnbound && bf._data == _data &&
            (_selfType == null || CompilerHelpers.GetType(bf.__self__) == _selfType) &&
%(typeCheck)s
            ) {
            bool shouldOptimize;
            object res = _info.Caller(new object[] { context, bf.__self__, %(callArgs)s }, out shouldOptimize);

            if (shouldOptimize) {
                _info.OptimizedDelegate = _info.GetInvokeBinder(context).Optimize(_site, new object[] { context, func, %(callArgs)s });
            }

            return res;
        }

        return ((CallSite<Func<CallSite, CodeContext, TFuncType, %(typeParams)s, object>>)site).Update(site, context, func, %(callArgs)s);
    }
}
"""

def builtin_function_callers(cw):
    for nparams in range(1, MAX_ARGS-2):       
        assignTemplate = "        _type%d = type%d;"
        typeCheckTemplate = "            (_type%d == null || CompilerHelpers.GetType(arg%d) == _type%d)"
        typeVarTemplate = "    private readonly Type " + ', '.join(('_type%d' % i for i in xrange(nparams))) + ';'
        cw.write(builtin_function_caller_template % {
                  'argCount' : nparams,
                  'ctorArgs' : ',' * nparams,
                  'typeParams' : ', '.join(('T%d' % d for d in xrange(nparams))),
                  'callArgs': ', '.join(('arg%d' % d for d in xrange(nparams))),
                  'callParams': ', '.join(('T%d arg%d' % (d,d) for d in xrange(nparams))),
                  'typeCheckParams': ', '.join(('Type type%d' % (d,) for d in xrange(nparams))),
                  'argsAssign' : '\n'.join((assignTemplate % (d,d) for d in xrange(nparams))),
                  'typeCheck' : ' &&\n'.join((typeCheckTemplate % (d,d,d) for d in xrange(nparams))),
                  'dlgParams' : ',' * (nparams + 3),
                  'typeVars' : typeVarTemplate,
                 })

function_caller_template = """
class FunctionCaller<%(typeParams)s> : FunctionCaller {
    public FunctionCaller(int compat) : base(compat) { }
    
    public object Call%(argCount)d(CallSite site, CodeContext context, object func, %(callParams)s) {
        PythonFunction pyfunc = func as PythonFunction;
        if (pyfunc != null && !EnforceRecursion && pyfunc._compat == _compat) {
            var callTarget = pyfunc.Target as CallTarget%(argCount)d;
            if (callTarget != null) {
                return callTarget(pyfunc, %(callArgs)s);
            }
        }

        return ((CallSite<Func<CallSite, CodeContext, object, %(typeParams)s, object>>)site).Update(site, context, func, %(callArgs)s);
    }"""
    
defaults_template = """
    public object Default%(defaultCount)dCall%(argCount)d(CallSite site, CodeContext context, object func, %(callParams)s) {
        PythonFunction pyfunc = func as PythonFunction;
        if (pyfunc != null && !EnforceRecursion && pyfunc._compat == _compat) {
            var callTarget = pyfunc.Target as CallTarget%(totalParamCount)d;
            if (callTarget != null) {            
                int defaultIndex = pyfunc.Defaults.Length - pyfunc.NormalArgumentCount + %(argCount)d;
                return callTarget(pyfunc, %(callArgs)s, %(defaultArgs)s);
            }
        }

        return ((CallSite<Func<CallSite, CodeContext, object, %(typeParams)s, object>>)site).Update(site, context, func, %(callArgs)s);
    }"""

defaults_template_0 = """
public object Default%(argCount)dCall0(CallSite site, CodeContext context, object func) {
    PythonFunction pyfunc = func as PythonFunction;
    if (pyfunc != null && !EnforceRecursion && pyfunc._compat == _compat) {
        var callTarget = pyfunc.Target as CallTarget%(argCount)d;
        if (callTarget != null) {
            int defaultIndex = pyfunc.Defaults.Length - pyfunc.NormalArgumentCount;
            return callTarget(pyfunc, %(defaultArgs)s);
        }
    }

    return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
}"""

def function_callers(cw):
    for nparams in range(1, MAX_ARGS-2):        
        cw.write(function_caller_template % {
                  'typeParams' : ', '.join(('T%d' % d for d in xrange(nparams))),
                  'callParams': ', '.join(('T%d arg%d' % (d,d) for d in xrange(nparams))),
                  'argCount' : nparams,
                  'callArgs': ', '.join(('arg%d' % d for d in xrange(nparams))),
                 })                    
                 
        for i in xrange(nparams + 1, MAX_ARGS - 2):
            cw.write(defaults_template % {
                      'typeParams' : ', '.join(('T%d' % d for d in xrange(nparams))),
                      'callParams': ', '.join(('T%d arg%d' % (d,d) for d in xrange(nparams))),
                      'argCount' : nparams,
                      'totalParamCount' : i,
                      'callArgs': ', '.join(('arg%d' % d for d in xrange(nparams))),
                      'defaultCount' : i - nparams,
                      'defaultArgs' : ', '.join(('pyfunc.Defaults[defaultIndex + %d]' % curDefault for curDefault in xrange(i - nparams))),
                     })                 
        cw.write('}')

def function_callers_0(cw):
    for i in xrange(1, MAX_ARGS - 2):
        cw.write(defaults_template_0 % {
                  'argCount' : i,
                  'defaultArgs' : ', '.join(('pyfunc.Defaults[defaultIndex + %d]' % curDefault for curDefault in xrange(i))),
                 })                 

function_caller_switch_template = """case %(argCount)d:                        
    callerType = typeof(FunctionCaller<%(arity)s>).MakeGenericType(typeParams);
    mi = callerType.GetMethod(baseName + "Call%(argCount)d");
    Debug.Assert(mi != null);
    fc = GetFunctionCaller(callerType, funcCompat);
    funcType = typeof(Func<,,,,%(arity)s>).MakeGenericType(allParams);

    return new Binding.FastBindResult<T>((T)(object)Delegate.CreateDelegate(funcType, fc, mi), true);"""
    

def function_caller_switch(cw):
    for nparams in range(1, MAX_ARGS-2):
        cw.write(function_caller_switch_template % {
                  'arity' : ',' * (nparams - 1),
                  'argCount' : nparams,
                 })   

def gen_lazy_call_targets(cw):
    for nparams in range(MAX_ARGS+1):
        cw.enter_block("public static object OriginalCallTarget%d(%s)" % (nparams, make_params(nparams, "PythonFunction function")))
        cw.write("function.Target = function.func_code.GetCompiledCode();")
        cw.write("return ((CallTarget%d)function.Target)(%s);" % (nparams, gen_args_call(nparams, 'function')))
        cw.exit_block()
        cw.write('')

def call_targets(cw):
    for nparams in range(MAX_ARGS+1):
        cw.write("public delegate object CallTarget%d(%s);" %
                 (nparams, make_params(nparams, "PythonFunction function")))

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

def gen_call(nargs, nparams, cw, extra=[]):
    args = extra + ["arg%d" % i for i in range(nargs)]
    cw.enter_block("public override object Call(%s)" % make_params1(nargs))
    
    # first emit error checking...
    ndefaults = nparams-nargs
    if nargs != nparams:    
        cw.write("if (Defaults.Length < %d) throw BadArgumentError(%d);" % (ndefaults,nargs))
    
    # emit the common case of no recursion check
    if (nargs == nparams):
        cw.write("if (!EnforceRecursion) return target(%s);" % ", ".join(args))
    else:        
        dargs = args + ["Defaults[Defaults.Length - %d]" % i for i in range(ndefaults, 0, -1)]
        cw.write("if (!EnforceRecursion) return target(%s);" % ", ".join(dargs))
    
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
    cw.enter_block("public override object Call(CodeContext context, params object[] args)")
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

CODE = """
public static object Call(%(params)s) {
    FastCallable fc = func as FastCallable;
    if (fc != null) return fc.Call(%(args)s);

    return PythonCalls.Call(func, %(argsArray)s);
}"""

def gen_python_switch(cw):
    for nparams in range(MAX_ARGS+1):
        cw.write("""case %d: 
    originalTarget = (CallTarget%d)OriginalCallTarget%d;
    return typeof(CallTarget%d);""" % (nparams, nparams, nparams, nparams))

def main():
    return generate(
        ("Python Lazy Call Targets", gen_lazy_call_targets),
        ("Python Builtin Function Optimizable Callers", builtin_function_callers),
        ("Python Builtin Function Optimizable Switch", builtin_function_callers_switch),
        ("Python Zero Arg Function Callers", function_callers_0),
        ("Python Function Callers", function_callers),
        ("Python Function Caller Switch", function_caller_switch),
        ("Python Call Targets", call_targets),
        ("Python Call Target Switch", gen_python_switch),
    )

if __name__ == "__main__":
    main()
