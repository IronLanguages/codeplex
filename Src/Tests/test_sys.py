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

from lib.assert_util import *
from lib.process_util import launch_ironpython_changing_extensions

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

def test_negative():
	# api_version
	AreEqual(sys.api_version, 'IronPython does not support the C APIs, the api_version is not supported')
	try:
		sys.api_version = 2
	except NotImplementedError:
		pass
	else:
		Assert(False, "Setting sys.api_version did not throw NotImplementedError---ERROR!")
	# displayhook
	AreEqual(sys.displayhook, 'IronPython does not support sys.displayhook')
	try:
		sys.displayhook = 2
	except NotImplementedError:
		pass
	else:
		Assert(False, "Setting sys.displayhook did not throw NotImplementedError---ERROR!")
	# settrace
	AssertError(NotImplementedError, sys.settrace, None)
	# getrefcount
	AssertError(NotImplementedError, sys.getrefcount, None)


if is_cli:
    testDelGetFrame = "Test_Del_GetFrame" in sys.argv
    if testDelGetFrame:
        test_del_getframe()
    else:
        run_test(__name__)
        # this is a destructive test, run it in separate instance of IronPython
        launch_ironpython_changing_extensions(__file__, [], [], ("Test_Del_GetFrame",))