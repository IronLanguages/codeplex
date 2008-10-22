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

def gen_args_call(nparams):
    args = ""
    comma = ""
    for i in xrange(nparams):
        args = args + comma +("arg%d" % i)
        comma = ", "
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


def call_targets(cw):
    for nparams in range(MAX_ARGS+1):
        cw.write("public delegate object CallTarget%d(%s);" %
                 (nparams, make_params(nparams)))

def generator_targets(cw):
    for nparams in range(MAX_ARGS+1):
        cw.write("public delegate IEnumerator GeneratorTarget%d(%s);" %
                 (nparams, make_params(nparams, "PythonGenerator generator")))

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
        cw.write("case %d: return typeof(CallTarget%d);" % (nparams, nparams))

def gen_python_generator_switch(cw):
   for nparams in range(MAX_ARGS+1):
        cw.write("case %d: return typeof(GeneratorTarget%d);" % (nparams, nparams))

def main():
    return generate(
        ("Python Call Targets", call_targets),
        ("Python Generator Targets", generator_targets),
        ("Python Call Target Switch", gen_python_switch),
        ("Python Generator Target Switch", gen_python_generator_switch),
    )

if __name__ == "__main__":
    main()
