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

et = IronPythonTest.EngineTest()
multipleexecskips = [ ]
for s in dir(et):
    if s.startswith("Scenario"):
        if s in multipleexecskips:
            exec '@skip("multiple_execute") \ndef test_Engine_%s(): getattr(et, "%s")()' % (s, s)
        else :
            exec 'def test_Engine_%s(): getattr(et, "%s")()' % (s, s)

#Rowan Work Item 312902
@disabled("The ProfileDrivenCompilation feature is removed from DLR")
def test_deferred_compilation():
    save1 = IronPythonTest.TestHelpers.GetContext().Options.InterpretedMode
    save2 = IronPythonTest.TestHelpers.GetContext().Options.ProfileDrivenCompilation
    modules = sys.modules.copy()
    IronPythonTest.TestHelpers.GetContext().Options.ProfileDrivenCompilation = True # this will enable interpreted mode
    Assert(IronPythonTest.TestHelpers.GetContext().Options.InterpretedMode)
    try:
        # Just import some modules to make sure we can switch to compilation without blowing up
        import test_namebinding
        import test_function
        import test_tcf
    finally:
        IronPythonTest.TestHelpers.GetContext().Options.InterpretedMode = save1
        IronPythonTest.TestHelpers.GetContext().Options.ProfileDrivenCompilation = save2
        sys.modules = modules

def CreateOptions():
    import sys
    import clr
    
    o = IronPython.PythonEngineOptions()
    if sys.argv.count('-X:ExceptionDetail') > 0: o.ExceptionDetail = True
    return o

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


#Rowan Work Item 312902
@skip("silverlight", "multiple_execute")
def test_formatexception():
    try:
        import Microsoft.Scripting
        se = Microsoft.Scripting.Hosting.ScriptRuntime.Create()
        pe = se.GetEngine('py')
        
        AssertError(TypeError, pe.FormatException, None)
    
        exc_string = pe.FormatException(System.Exception("first",
                                                        System.Exception("second",
                                                                         System.Exception())))
        AreEqual(exc_string, 'Traceback (most recent call last):\r\nException: first')
        exc_string = pe.FormatException(c())
        AreEqual(exc_string.count(" File "), 4)
        AreEqual(exc_string.count(" line "), 4)
    finally:
        pass

#Rowan Work Item 31290
@skip("silverlight")
def test_formatexception_showclrexceptions():
    try:
        import Microsoft.Scripting
        se = Microsoft.Scripting.Hosting.ScriptRuntime.Create()
        pe = se.GetEngine('py')

        pe.Options.ShowClrExceptions = True
    
        exc_string = pe.FormatException(System.Exception("first",
                                                        System.Exception("second",
                                                                         System.Exception())))
        AreEqual(exc_string, "Traceback (most recent call last):\r\nException: first\r\nCLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n")
        exc_string = pe.FormatException(c())

        # Uncomment when bug #371095 is fixed: Helper stubs shown in Python stack traces
        #AreEqual(exc_string.count(" File "), 4)
        #AreEqual(exc_string.count(" line "), 4)
        Assert(exc_string.endswith("CLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n"))
    
    finally:
        pe.Options.ShowClrExceptions = False


@disabled("CodePlex 6710")
def test_formatexception_exceptiondetail():
    '''
    Expected return values need to be updated before this test can be re-enabled.
    '''
    try:
        import Microsoft.Scripting
        se = Microsoft.Scripting.Hosting.ScriptRuntime.Create()
        pe = se.GetEngine('py')

        IronPythonTest.TestHelpers.GetContext().Options.ExceptionDetail = True
    
        #CodePlex Work Item 6710
        exc_string = pe.FormatException(System.Exception("first", System.Exception("second", System.Exception())))
        AreEqual(exc_string, "Traceback (most recent call last):\r\nException: first\r\nCLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n")
        exc_string = pe.FormatException(c())
        AreEqual(exc_string.count(" File "), 6)
        AreEqual(exc_string.count(" line "), 4)
        Assert(exc_string.endswith("CLR Exception: \r\n    Exception\r\n: \r\nfirst\r\n    Exception\r\n: \r\nsecond\r\n    Exception\r\n: \r\nException of type 'System.Exception' was thrown.\r\n"))
    
    finally:
        IronPythonTest.TestHelpers.GetContext().Options.ExceptionDetail = False
    

run_test(__name__)


#Make sure this runs last
#test_dispose()
