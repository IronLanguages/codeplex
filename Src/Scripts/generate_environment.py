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

def gen_one_env(cw, i):
    cw.writeline("[PythonType(typeof(Dict))]")
    cw.enter_block("public sealed class FunctionEnvironment%iDictionary : FunctionEnvironmentDictionary" % i)
    for j in range(i):
        cw.writeline("[EnvironmentIndex(%i)] public object value%i;" % (j, j))
    cw.writeline()
    cw.writeline("public FunctionEnvironment%iDictionary(FunctionEnvironmentDictionary parent, IFrameEnvironment frame, SymbolId[] names, SymbolId[] outer)" % i)
    cw.enter_block("    : base(parent, frame, names, outer)")
    cw.exit_block()
    cw.enter_block("public override bool TrySetExtraValue(SymbolId key, object value)")
    if i > 4:
        cw.enter_block("for (int i = 0; i < names.Length; i++)")
        cw.enter_block("if (names[i]== key)")
        cw.writeline("SetAtIndex(i, value);")
        cw.writeline("return true;")
        cw.exit_block()
        cw.exit_block()
    else:
        for j in range(i):
            cw.enter_block("if(names.Length >= %d)" % (j+1))
            cw.enter_block("if(names[%d]==key)" % j)
            cw.writeline("value%d = value;" % j)
            cw.writeline("return true;")
            cw.exit_block()
        for j in range(i):
            cw.exit_block()

    cw.writeline("return false;")            
    cw.exit_block()

    cw.enter_block("public override bool TryGetExtraValue(SymbolId key, out object value)")
    if i > 4:
        cw.enter_block("for (int index = 0; index < names.Length; index++)")
        cw.enter_block("if (names[index] == key)")   
        cw.writeline("value = GetAtIndex(index);")
        cw.writeline("return true;")
        cw.exit_block()
        cw.exit_block()
    else:
        for j in range(i):
            cw.enter_block("if(names.Length >= %d)" % (j+1))
            cw.enter_block("if(names[%d] == key)" % j)
            cw.writeline("value = value%d;" % j)
            cw.writeline("return true;")
            cw.exit_block()
        for j in range(i):
            cw.exit_block()

    cw.writeline("return TryGetOuterValue(key, out value);")
    cw.exit_block()

    cw.enter_block("protected override object GetValueAtIndex(int index)")
    if i > 4:
        cw.writeline("return GetAtIndex(index);")
    else:
        cw.enter_block("switch (index)")
        for j in range(i):
            cw.writeline("case %i: return value%i;" % (j, j))
        cw.writeline("default: throw OutOfRange(index);")
        cw.exit_block()
    
    cw.exit_block()

    if i > 4:
        cw.enter_block("private object GetAtIndex(int index)")
        cw.enter_block("switch (index)")
        for j in range(i):
            cw.writeline("case %i: return value%i;" % (j, j))
        cw.writeline("default: throw OutOfRange(index);")
        cw.exit_block()
        cw.exit_block()
        cw.enter_block("private void SetAtIndex(int index, object value)")
        cw.enter_block("switch (index)")
        for j in range(i):
            cw.writeline("case %i: value%i = value; break;" % (j, j))
        cw.writeline("default: throw OutOfRange(index);")
        cw.exit_block()
        cw.exit_block()
    cw.exit_block()

sizes = [2, 4, 8, 16, 32]

def gen_runner(cw, fnc):
    nl = False
    for i in sizes:
        if nl:
            cw.writeline()
        fnc(cw, i)
        nl = True

def gen_env(cw):
    gen_runner(cw, gen_one_env)

def gen_one_pf(cw, i, first, last=False):
    if first: cw.enter_block("if (size <= %i)" %  i)
    elif not last: cw.else_block("if (size <= %i)" %  i)
    cw.writeline("envType = typeof(FunctionEnvironment%iDictionary);" % i)

def gen_pf(cw):
    ssizes = sorted(sizes)

    first = True
    cw.enter_block("if (size <= %i && Options.OptimizeEnvironments)" % ssizes[-1])
    for i in ssizes[:-1]:
        gen_one_pf(cw, i, first)
        first = False
    cw.else_block()
    gen_one_pf(cw, ssizes[-1], False, True)

    cw.exit_block()
    cw.writeline("ctor = envType.GetConstructor(new Type[] { typeof(FunctionEnvironmentDictionary), typeof(IFrameEnvironment), typeof(SymbolId[]), typeof(SymbolId[]) });")
    cw.writeline("ef = new FieldEnvironmentFactory(envType);")
    cw.else_block()
    cw.writeline("cg.EmitInt(size);");
    cw.writeline("envType = typeof(FunctionEnvironmentNDictionary);")
    cw.writeline("ctor = envType.GetConstructor(new Type[] { typeof(int), typeof(FunctionEnvironmentDictionary), typeof(IFrameEnvironment), typeof(SymbolId[]), typeof(SymbolId[]) });")
    cw.writeline("ef = new IndexEnvironmentFactory(size);")
    cw.exit_block()

generate.CodeGenerator("environment types", gen_env).doit()
generate.CodeGenerator("partial factories", gen_pf).doit()
