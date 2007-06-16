#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

##
## Testing IronPython Engine
##

from lib.assert_util import *
import sys

if not is_silverlight:
    remove_ironpython_dlls(testpath.public_testdir)
    load_iron_python_dll()
    
import IronPython
pe = IronPython.Hosting.PythonEngine.CurrentEngine

# setup Scenario tests in module from EngineTest.cs
# this enables us to see the individual tests that pass / fail
load_iron_python_test()
import IronPythonTest
et = IronPythonTest.EngineTest()
for s in dir(et):
    if s.startswith("Scenario"):
        exec 'def test_Engine_%s(): getattr(et, "%s")()' % (s, s)

def test_trivial():
    Assert(IronPython.Hosting.PythonEngine.Version != "")

def test_fasteval():
    global pe;

    # pe.Options.FastEvaluation is tested at compile time, so we need
    # to import another module here for the option to take effect.
    save = pe.Options.FastEvaluation
    pe.Options.FastEvaluation = True
    import fasteval
    fasteval.do_fasteval_test()
    pe.Options.FastEvaluation = save

def skip_test_CreateMethod():
    """Test cases specific to PythonEngine.CreateMethod<DelegateType>"""

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

def skip_test_CreateLambdaAndMethod():
    """Common Test cases for PythonEngine.CreateLambda<DelegateType>(...) and PythonEngine.CreateMethod<DelegateType>"""
    
    
    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegate, SimpleReturnDelegateArg1, SimpleReturnDelegateArg2

    try:
        for funcInfo in [(pe.CreateLambda,''), (pe.CreateMethod,'return ')]:         
            func = funcInfo[0]
            prepend = funcInfo[1]
            
            # simple parameterless eval works
            AreEqual(func[SimpleReturnDelegate](prepend+'123')(), 123)
            
            # eval w/ different module scopes works
            module1 = pe.CreateModule('test')
            module1.Scope.SetName('abc', 'xyz')
            
            module2 = pe.CreateModule('test')
            module2.Scope.SetName('abc', 'def')
        
            AreEqual(func[SimpleReturnDelegate](prepend+'abc', module1)(), 'xyz')
            AreEqual(func[SimpleReturnDelegate](prepend+'abc', module2)(), 'def')
                
            # scoped w/ 1 arg, none remapped    
            AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + arg1', module1)('qrt'), 'xyzqrt')
            AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + arg1', module2)('qrt'), 'defqrt')
            
            # scoped w/ 2 args, none remapped    
            AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + arg1 + arg2', module1)('qrt','asd'), 'xyzqrtasd')
            AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + arg1 + arg2', module2)('qrt','asd'), 'defqrtasd')
            
            # scoped w/ 1 arg, remapped
            AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + xyz', ('xyz',), module1)('qrt'), 'xyzqrt')
            AreEqual(func[SimpleReturnDelegateArg1](prepend+'abc + xyz', ('xyz',), module2)('qrt'), 'defqrt')
            
            # unscoped w/ 1 arg remapped
            AreEqual(func[SimpleReturnDelegateArg1](prepend+'xyz', ('xyz',))('qrt'), 'qrt')
            
            # scoped w/ 2 args, remapped
            AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + xyz + qrt', ('xyz','qrt'), module1)('qrt','asd'), 'xyzqrtasd')
            AreEqual(func[SimpleReturnDelegateArg2](prepend+'abc + xyz + qrt', ('xyz','qrt'), module2)('qrt','asd'), 'defqrtasd')
    
        for funcInfo in [(pe.CreateLambdaUnscoped,''), (pe.CreateMethodUnscoped,'return ')]:
            func = funcInfo[0]
            prepend = funcInfo[1]
            
            # unscoped lambda
            x = func[SimpleReturnDelegate](prepend+'abc')
            
            AreEqual(x(module1)(), 'xyz')
            AreEqual(x(module2)(), 'def')
    except Exception, e: print e, e.clsException
    
    
def skip_test_CreateMethod_Negative():
    """Negative test cases for PythonEngine.CreateMethod<DelegateType>"""
    
    from System import ArgumentException, Delegate, MulticastDelegate

    load_iron_python_test()
    from IronPythonTest import SimpleDelegate, SimpleReturnDelegateArg1, SimpleReturnDelegateArg2
    module = pe.CreateModule()

    # specifying too many arguments should raise a ArgumentException
    try:
        pe.CreateMethod[SimpleDelegate]('abc', ('abc',), module)
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
        pe.CreateMethod[SimpleReturnDelegateArg2]('abc + xyz + arg2', ('xyz',), module)('qrt','asd')
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
    import clr
    
    o = IronPython.PythonEngineOptions()
    if sys.argv.count('-X:ExceptionDetail') > 0: o.ExceptionDetail = True
    return o

def skip_test_CreateLambda_Negative():
    """Negative test cases for PythonEngine.CreateLambda<DelegateType>"""

    from System import ArgumentException, Delegate, MulticastDelegate

    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegate, SimpleReturnDelegateArg2
    module = pe.CreateModule()
    module.Scope.SetName('abc', 'abc')

    # specifying too many arguments should raise a ArgumentException
    try:
        pe.CreateLambda[SimpleReturnDelegate]('abc', ('abc',), module)
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
        pe.CreateLambda[SimpleReturnDelegateArg2]('abc + xyz + arg2', ('xyz',), module)('qrt','asd')
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


def skip_test_CreateMethod_ImportSys():
    import sys
    script = "import sys\nreturn sys.version"
    
    v = sys.version.split(" ", 1)

    pe.InitializeModules("", "", v[1][1:-1])
    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegate	
    
    method = pe.CreateMethod[SimpleReturnDelegate](script)
    AreEqual(method(), sys.version)


def skip_test_CreateLambda_Division():
    load_iron_python_test()
    from IronPythonTest import SimpleReturnDelegateArg1	
    
    ex = pe.CreateLambda[SimpleReturnDelegateArg1]("1.0 / arg1")
    result = ex(100000)
    Assert(result < 1)


def test_error_expression():
    '''
    Only purpose of this test is to hit IronPython.Compiler.ErrorExpression.
    This is not possible without creating our own error sink which does not throw
    any exceptions from the Add method which in turn is invoked by a Parser
    object.
    '''
    global pe
    import Microsoft
    
    if sys.executable.lower().find("\\debug\\") != -1:
        #The debug binaries contain an assert which kills ipy.exe on the "[." 
        #evaluation
        return
    
    class TestErrorSink(Microsoft.Scripting.Hosting.ErrorSink):
        def Add(self, su, msg, span, errorCode, severity):
            pass

    copts = pe.GetDefaultCompilerOptions()
    scu = Microsoft.Scripting.SourceCodeUnit(pe, "[.")
    sink = TestErrorSink()
    cc = Microsoft.Scripting.CompilerContext(scu, copts, sink)
    AssertError(TypeError, pe.Compiler.ParseExpressionCode, cc)

def test_get_exception_message():
    ex = System.Exception("BAD")
    tName, tType = pe.GetExceptionMessage(ex)
    AreEqual(tName, "Exception: BAD")
    AreEqual(tType, "Exception")

def test_script_compiler():
    Assert(pe.ScriptCompiler!=None)  

def test_publishmodule():
    AssertError(TypeError, pe.PublishModule, None)

  
       
run_test(__name__)


#Make sure this runs last
#test_dispose()
if not is_silverlight and __name__ == "__main__":
    pe.Dispose()
