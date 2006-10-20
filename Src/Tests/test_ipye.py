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

##
## Testing IronPython Engine
##

from lib.assert_util import *
import sys
import IronPython
pe = IronPython.Hosting.PythonEngine()

def test_trivial():
    Assert(IronPython.Hosting.PythonEngine.Version != "")


def test_version():
    """ test that the assembly versions are the same as 1.0 release for compatibility."""
    import clr
    import System

    for asm in [
            clr.GetClrType(IronPython.Hosting.PythonEngine).Assembly,
            (1L).GetType().Assembly
        ]:
        av = asm.GetName().Version
        Assert(av != None)
        AreEqual(av.Major, 1)
        AreEqual(av.Minor, 0)
        AreEqual(av.Build, 60816)
        AreEqual(av.Revision, 1877)

def test_coverage():
    # 1. fasteval 
    save = IronPython.Compiler.Options.FastEvaluation
    IronPython.Compiler.Options.FastEvaluation = True
    AreEqual(eval("None"), None)
    AreEqual(eval("str(2)"), "2")
    IronPython.Compiler.Options.FastEvaluation = save
    
    # 2. ...

# verify no interference between PythonEngine used in IronPython console and user created one
def test_no_interference(): 
    # 1. path
    oldpath = sys.path
    pe = IronPython.Hosting.PythonEngine()
    AreEqual(sys.path, oldpath)
    
    # 2. how about other states...

# now verify CLR loaded modules don't interfere 
def test_no_module_interference():
    pe.Execute("""import clr
clr.AddReferenceByPartialName('System.Security')
import System.Security.Cryptography
if not hasattr(System.Security.Cryptography, 'CryptographicAttributeObject'):
    raise AssertionError("CryptographicAttributeObject not found")
""")

    # verify our engine doesn't see it
    import System.Security
    Assert(not hasattr(System.Security.Cryptography, 'CryptographicAttributeObject'))

# tests wrote in C# EngineTest.cs
def test_engine():
    load_iron_python_test()
    import IronPythonTest
    et = IronPythonTest.EngineTest()
    for s in dir(et):
        if s.startswith("Scenario"):
            getattr(et, s)()

def test_CreateMethod():
    """Test cases specific to PythonEngine.CreateMethod<DelegateType>"""

    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    from IronPython.Hosting import EngineModule
    from clr import Reference
    
    load_iron_python_test()
    
    from IronPythonTest import IntArgDelegate, StringArgDelegate, RefReturnDelegate, OutReturnDelegate
    
    AreEqual(pe.CreateMethod[IntArgDelegate]('arg1 = "abc"\narg2="def"\nreturn arg1')(2, 3), 'abc')
    AreEqual(pe.CreateMethod[IntArgDelegate]('return (arg1+1,arg2+1)')(5,10), (6,11))
    AreEqual(pe.CreateMethod[StringArgDelegate]('arg1 = 2\narg2=3\nreturn arg1')('abc', 'def'), 2)
    y = Reference[object]()
    AreEqual(pe.CreateMethod[RefReturnDelegate]('sender = 2\nres.Value = 3\nreturn sender')('abc', y), 2)
    AreEqual(y.Value, 3)
    y = Reference[object]('abc')
    AreEqual(pe.CreateMethod[OutReturnDelegate]('res.Value = sender\nsender = "def"\nreturn sender')('abc', y), 'def')
    AreEqual(y.Value, 'abc')

def test_CreateLambdaAndMethod():
    """Common Test cases for PythonEngine.CreateLambda<DelegateType>(...) and PythonEngine.CreateMethod<DelegateType>"""
    
    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    from IronPython.Hosting import EngineModule
    
    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegate, SimpleReturnDelegateArg1, SimpleReturnDelegateArg2
    
    for funcInfo in [(pe.CreateLambda,''), (pe.CreateMethod,'return ')]:         
        func = funcInfo[0]
        prepend = funcInfo[1]
        
        # simple parameterless eval works
        AreEqual(func[SimpleReturnDelegate](prepend+'123')(), 123)
        
        # eval w/ different module scopes works
        engineModule1 = pe.CreateModule('test', False)
        engineModule1.Globals['abc'] = 'xyz'
        
        engineModule2 = pe.CreateModule('test', False)
        engineModule2.Globals['abc'] = 'def'
    
        AreEqual(func[SimpleReturnDelegate](prepend+'abc', engineModule1)(), 'xyz')
        AreEqual(func[SimpleReturnDelegate](prepend+'abc', engineModule2)(), 'def')
            
        # scoped w/ 1 arg, none remapped    
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + arg1', engineModule1)('qrt'), 'xyzqrt')
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + arg1', engineModule2)('qrt'), 'defqrt')
        
        # scoped w/ 2 args, none remapped    
        AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + arg1 + arg2', engineModule1)('qrt','asd'), 'xyzqrtasd')
        AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + arg1 + arg2', engineModule2)('qrt','asd'), 'defqrtasd')
    
        # scoped w/ 1 arg, remapped
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + xyz', ('xyz',), engineModule1)('qrt'), 'xyzqrt')
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + xyz', ('xyz',),engineModule2)('qrt'), 'defqrt')
        
        # unscoped w/ 1 arg remapped
        AreEqual(func[SimpleReturnDelegateArg1](prepend+'xyz', ('xyz',))('qrt'), 'qrt')
        
        # scoped w/ 2 args, remapped
        AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + xyz + qrt', ('xyz','qrt'), engineModule1)('qrt','asd'), 'xyzqrtasd')
        AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + xyz + qrt', ('xyz','qrt'), engineModule2)('qrt','asd'), 'defqrtasd')

    for funcInfo in [(pe.CreateLambdaUnscoped,''), (pe.CreateMethodUnscoped,'return ')]:
        func = funcInfo[0]
        prepend = funcInfo[1]
        
        # unscoped lambda
        x = func[SimpleReturnDelegate](prepend+'abc')
        
        AreEqual(x(engineModule1)(), 'xyz')
        AreEqual(x(engineModule2)(), 'def')
        
    
    
def test_CreateMethod_Negative():
    """Negative test cases for PythonEngine.CreateMethod<DelegateType>"""
    
    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    from System import ArgumentException, Delegate, MulticastDelegate
    from IronPython.Hosting import EngineModule    

    load_iron_python_test()
    from IronPythonTest import SimpleDelegate, SimpleReturnDelegateArg1, SimpleReturnDelegateArg2
    engineModule = pe.CreateModule()

    # specifying too many arguments should raise a ArgumentException
    try:
        pe.CreateMethod[SimpleDelegate]('abc', ('abc',), engineModule)
        AreEqual(True, False)
    except ArgumentException:
        pass
        
    # specifying too many arguments should raise a ArgumentException (w/o module scope)
    try:
        pe.CreateMethod[SimpleDelegate]('abc', ('abc',))
        AreEqual(True, False)
    except ArgumentException:
        pass

    # scoped w/ 2 args, only 1st arg is remapped, so we throw due to bad args
    try:
        pe.CreateMethod[SimpleReturnDelegateArg2]('abc + xyz + arg2', ('xyz',), engineModule)('qrt','asd')
        AssertUnreachable()
    except ValueError:
        pass
        
    # failing to have a return statements doesn't yield an expression value, it returns None
    AreEqual(pe.CreateMethod[SimpleReturnDelegateArg1]('arg1')('abc'), None)

    # bad generic types used to create lambda
    try:        
        pe.CreateMethod[Delegate]('abc')
        Assert(False)
    except ArgumentException: 
        pass
    
    try:        
        pe.CreateMethod[MulticastDelegate]('abc')
        Assert(False)
    except ArgumentException: 
        pass


    try:        
        pe.CreateMethod[object]('abc')
        Assert(False)
    except ArgumentException: 
        pass

def CreateOptions():
    import sys
    
    from IronPython.Hosting import EngineOptions
    o = EngineOptions()
    if sys.argv.count('-X:ExceptionDetail') > 0: o.ExceptionDetail = True
    return o

def test_CreateLambda_Negative():
    """Negative test cases for PythonEngine.CreateLambda<DelegateType>"""
    pe = IronPython.Hosting.PythonEngine(CreateOptions())

    from System import ArgumentException, Delegate, MulticastDelegate
    from IronPython.Hosting import EngineModule

    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegate, SimpleReturnDelegateArg2
    engineModule = pe.CreateModule()
    engineModule.Globals['abc'] = 'abc'

    # specifying too many arguments should raise a ArgumentException
    try:
        pe.CreateLambda[SimpleReturnDelegate]('abc', ('abc',), engineModule)
        Assert(False)
    except ArgumentException:
        pass
        
    # specifying too many arguments should raise a ArgumentException (w/o module scope)
    try:
        pe.CreateLambda[SimpleReturnDelegate]('abc', ('abc',))
        Assert(False)
    except ArgumentException:
        pass        

    # verify statements don't compile for lambda's
    try:
        pe.CreateLambda[SimpleReturnDelegate]('print abc')()
        Assert(False)
    except SyntaxError:
        pass
        
    # scoped w/ 2 args, only 1st arg is remapped, so we throw due to not providing enough args
    try:
        pe.CreateLambda[SimpleReturnDelegateArg2]('abc + xyz + arg2', ('xyz',), engineModule)('qrt','asd')
        AssertUnreachable()
    except ValueError:
        pass

    # bad generic types used to create lambda
    try:        
        pe.CreateLambda[Delegate]('abc')
        Assert(False)
    except ArgumentException: 
        pass
    
    try:        
        pe.CreateLambda[MulticastDelegate]('abc')
        Assert(False)
    except ArgumentException: 
        pass


    try:        
        pe.CreateLambda[object]('abc')
        Assert(False)
    except ArgumentException: 
        pass


def test_CreateMethod_ImportSys():
    script = "import sys\nreturn sys.version"
    
    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    
    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegate	
    
    method = pe.CreateMethod[SimpleReturnDelegate](script)
    import sys
    AreEqual(method(), sys.version)


def test_CreateLambda_Division():
    pe = IronPython.Hosting.PythonEngine(CreateOptions())
    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegateArg1	
    
    ex = pe.CreateLambda[SimpleReturnDelegateArg1]("1.0 / arg1")
    result = ex(100000)
    Assert(result < 1)

    
def test_interactive_input():
    x = pe.ParseInteractiveInput("""x = "abc\\
""", True)
    AreEqual(x, False)

run_test(__name__)
