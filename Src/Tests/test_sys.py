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

def test_del_getframe():
    # This is a destructive test, run it in separate instance of IronPython.
    # Currently, there is no way to restore sys back to its original state.
    global testDelGetFrame
    if not testDelGetFrame:
        return

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

def test_assign_getframe():
    for val in [None, 0, 1L, str, False]:
        sys._getframe = val
        AreEqual(sys._getframe, val)

@skip("win32")
def test_api_version():
    # api_version
    AreEqual(sys.api_version, 0)

@skip("win32")
def test_settrace():
    # settrace
    AssertError(NotImplementedError, sys.settrace, None)

@skip("win32")
def test_getrefcount():
    # getrefcount
    Assert(not hasattr(sys, 'getrefcount'))

@skip("win32 silverlight")
def test_version():
    import re
    #E.g., 2.5.0 (IronPython 2.0 Alpha (2.0.0.800) on .NET 2.0.50727.1433)
    regex = "^\d\.\d\.\d \(IronPython \d\.\d(\.\d)? ((Alpha )|(Beta )|())\(\d\.\d\.\d\.\d{1,8}\) on \.NET \d(\.\d{1,5}){3}\)$"
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
    

testDelGetFrame = "Test_Del_GetFrame" in sys.argv
if testDelGetFrame:
    test_del_getframe()
else:
    run_test(__name__)
    # this is a destructive test, run it in separate instance of IronPython
    if not is_silverlight and sys.platform!="win32":
        from iptest.process_util import launch_ironpython_changing_extensions
        AreEqual(0, launch_ironpython_changing_extensions(__file__, [], [], ("Test_Del_GetFrame",)))
