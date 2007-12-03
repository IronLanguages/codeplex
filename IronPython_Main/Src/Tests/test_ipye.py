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

##
## Testing IronPython Engine
##

from lib.assert_util import *
import sys

if not is_silverlight:
    remove_ironpython_dlls(testpath.public_testdir)
    load_iron_python_dll()

# setup Scenario tests in module from EngineTest.cs
# this enables us to see the individual tests that pass / fail
load_iron_python_test()

import IronPython
import IronPythonTest

pe = IronPython.Hosting.PythonEngine.CurrentEngine
et = IronPythonTest.EngineTest()
for s in dir(et):
    if s.startswith("Scenario"):
        exec 'def test_Engine_%s(): getattr(et, "%s")()' % (s, s)

#Rowan Work Item 312902
@skip("interpreted")
def test_trivial():
    Assert(IronPython.Hosting.PythonEngine.Version != "")

#Rowan Work Item 312902
@skip("interpreted")
def test_interpreted():
    global pe

    # pe.Options.InterpretedMode is tested at compile time:
    # it will take effect not immediately, but in modules we import
    save = pe.Options.InterpretedMode
    pe.Options.InterpretedMode = True
    modules = sys.modules.copy()
    try:
        # Just try some important tests.
        # The full test suite should pass using -X:Interpret; this is just a lightweight check for "run 0".

        import test_delegate
        import test_function
        import test_closure
        import test_namebinding
        import test_generator
        import test_tcf
        import test_methoddispatch
        import test_operator
        import test_exec
        import test_list
        import test_cliclass
        import test_exceptions
        # These two pass, but take forever to run
        #import test_numtypes
        #import test_number
        import test_str
        import test_math
        import test_statics
        import test_property
        import test_weakref
        import test_specialcontext
        import test_thread
        import test_dict
        import test_set
        import test_tuple
        import test_class
        if not is_silverlight:
            #This particular test corrupts the run - CodePlex Work Item 11830
            import test_syntax
    finally:
        pe.Options.InterpretedMode = save
        # "Un-import" these modules so that they get re-imported in emit mode
        sys.modules = modules

#Rowan Work Item 312902
@skip("interpreted")
def test_deferred_compilation():
    global pe
    
    save1 = pe.Options.InterpretedMode
    save2 = pe.Options.ProfileDrivenCompilation
    modules = sys.modules.copy()
    pe.Options.ProfileDrivenCompilation = True # this will enable interpreted mode
    Assert(pe.Options.InterpretedMode)
    try:
        # Just import some modules to make sure we can switch to compilation without blowing up
        import test_namebinding
        import test_function
        import test_tcf
    finally:
        pe.Options.InterpretedMode = save1
        pe.Options.ProfileDrivenCompilation = save2
        sys.modules = modules

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


#Rowan Work Item 312902
@skip("interpreted")
def test_get_exception_message():
    ex = System.Exception("BAD")
    tName, tType = pe.GetExceptionMessage(ex)
    AreEqual(tName, "Exception: BAD")
    AreEqual(tType, "Exception")

def test_publishmodule():
    AssertError(TypeError, pe.PublishModule, None)



def a():
    raise System.Exception()

def b():
    try:
        a()
    except System.Exception, e:
        raise System.Exception("second", e)

def c():
    try:
        b()
    except System.Exception, e:
        x = System.Exception("first", e)
    return x

@skip("silverlight")
def test_formatexception():
    try:
        AssertError(TypeError, pe.FormatException, None)
    
        exc_string = pe.FormatException(System.Exception("first", 
                                                        System.Exception("second", 
                                                                         System.Exception())))
        AreEqual(exc_string, 'Traceback (most recent call last):\r\nException: first\r\n')
        exc_string = pe.FormatException(c())
        AreEqual(exc_string.count(" File "), 4)
        AreEqual(exc_string.count(" line "), 4)
    finally:
        pass

@skip("silverlight")
def test_formatexception_showclrexceptions():
    try:
        pe.Options.ShowClrExceptions = True
    
        exc_string = pe.FormatException(System.Exception("first", 
                                                        System.Exception("second", 
                                                                         System.Exception())))
        AreEqual(exc_string, "Traceback (most recent call last):\r\nException: first\r\nCLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n")
        exc_string = pe.FormatException(c())
        AreEqual(exc_string.count(" File "), 6)
        AreEqual(exc_string.count(" line "), 6)
        Assert(exc_string.endswith("CLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n"))
    
    finally:
        pe.Options.ShowClrExceptions = False

@disabled("CodePlex 6710")
def test_formatexception_exceptiondetail():       
    '''
    Expected return values need to be updated before this test can be re-enabled.
    ''' 
    try:
        pe.Options.ExceptionDetail = True
    
        #CodePlex Work Item 6710
        exc_string = pe.FormatException(System.Exception("first", System.Exception("second", System.Exception())))
        AreEqual(exc_string, "Traceback (most recent call last):\r\nException: first\r\nCLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n")
        exc_string = pe.FormatException(c())
        AreEqual(exc_string.count(" File "), 6)
        AreEqual(exc_string.count(" line "), 4)
        Assert(exc_string.endswith("CLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n"))
    
    finally:
        pe.Options.ExceptionDetail = False
    

run_test(__name__)


#Make sure this runs last
#test_dispose()
if not is_silverlight and __name__ == "__main__":
    pe.Dispose()
