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

from iptest.assert_util import *

# adding some negative test case coverage for the sys module; we currently don't implement
# some methods---there is a CodePlex work item 1042 to track the real implementation of
# these methods

import sys

def test_getframe():
    # This test requires -X:FullFrames, run it in separate instance of IronPython.
    global testDelGetFrame
    if not testDelGetFrame:
        return

    _getframe = sys._getframe

    for val in [None, 0, 1L, str, False]:
        sys._getframe = val
        AreEqual(sys._getframe, val)

    del sys._getframe
    
    try:
        # try access again
        x = sys._getframe
    except AttributeError: pass
    else: raise AssertionError("Deletion of sys._getframe didn't take effect")
    
    try:
        # try deletion again
        del sys._getframe
    except AttributeError: pass
    else: raise AssertionError("Deletion of sys._getframe didn't take effect")
    
    # restore it back
    sys._getframe = _getframe

    def g():
        y = 42
        def f():
            x = sys._getframe(1)
            return x

        AreEqual(f().f_locals['y'], 42)
        Assert(f().f_builtins is f().f_globals['__builtins__'].__dict__)
        AreEqual(f().f_locals['y'], 42)
        AreEqual(f().f_exc_traceback, None)
        AreEqual(f().f_exc_type, None)
        AreEqual(f().f_restricted, False)
        AreEqual(f().f_trace, None)

        # replace __builtins__, func code should have the non-replaced value
        global __builtins__
        oldbuiltin = __builtins__
        try:
            __builtins__ = dict(__builtins__.__dict__)
            def f():
                x = sys._getframe(1)
                return x
            Assert(f().f_builtins is oldbuiltin.__dict__)
        finally:
            __builtins__ = oldbuiltin

        def f():
            x = sys._getframe()
            return x

        AreEqual(f().f_back.f_locals['y'], 42)

        def f():
            yield sys._getframe()
           
        frame = list(f())[0]
        if is_cli:
            # incompat, this works for us, but not CPython, not sure why.
            AreEqual(frame.f_back.f_locals['y'], 42)
        
        # call through a built-in function
        global gfres
        class x(object):
            def __cmp__(self, other):
                global gfres
                gfres = sys._getframe(1)
                return -1
                
        cmp(x(), x())
        AreEqual(gfres.f_locals['y'], 42)
    g()
    
    def f():
        x = 42
        def g():
                import sys
                AreEqual(sys._getframe(1).f_locals['x'], 42)
        g()
        yield 42
    
    list(f())
    
    class x:
        abc = sys._getframe(0)
        
    AreEqual(x.abc.f_locals['abc'], x.abc)
    
    class x:
        class y:
            abc = sys._getframe(1)
        abc = y.abc
    
    AreEqual(x.abc.f_locals['abc'], x.abc)

@skip("win32")
def test_api_version():
    # api_version
    AreEqual(sys.api_version, 0)

@skip("win32")
def test_settrace():
    """TODO: now that sys.settrace has been implemented this test case needs to be fully revisited"""
    # settrace
    Assert(hasattr(sys, 'settrace'))

@skip("win32")
def test_getrefcount():
    # getrefcount
    Assert(not hasattr(sys, 'getrefcount'))

@skip("win32 silverlight")
def test_version():
    import re
    #E.g., 2.5.0 (IronPython 2.0 Alpha (2.0.0.800) on .NET 2.0.50727.1433)
    regex = "^\d\.\d\.\d \(IronPython \d\.\d(\.\d)? ((Alpha \d+ )|(Beta \d+ )|())((DEBUG )|()|(\d?))\(\d\.\d\.\d\.\d{1,8}\) on \.NET \d(\.\d{1,5}){3}\)$"
    Assert(re.match(regex, sys.version) != None)

def test_winver():
    import re
    #E.g., "2.5"
    Assert(re.match("^\d\.\d$", sys.winver) != None)

@disabled("CodePlex 16497")
def test_ps1():
    Assert(not hasattr(sys, "ps1"))

@disabled("CodePlex 16497")
def test_ps2():
    Assert(not hasattr(sys, "ps2"))    
    

testDelGetFrame = "Test_GetFrame" in sys.argv
if testDelGetFrame:
    test_getframe()
else:
    run_test(__name__)
    # this is a destructive test, run it in separate instance of IronPython
    if not is_silverlight and sys.platform!="win32":
        from iptest.process_util import launch_ironpython_changing_extensions
        AreEqual(0, launch_ironpython_changing_extensions(__file__, ["-X:FullFrames"], [], ("Test_GetFrame",)))
    elif sys.platform == "win32":
        print 'running test_getframe on cpython'
        testDelGetFrame = True
        test_getframe()
