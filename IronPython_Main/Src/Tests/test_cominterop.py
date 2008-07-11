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

# COM Interop tests for IronPython

#For the time being just delegate this to the cominterop package...
import sys
from lib.assert_util import skiptest
skiptest("win32", "silverlight")

failed = 0
try:
    import cominterop
except:
    failed = 1

#------------------------------------------------------------------------------
#Re-run everything under -X:PreferComDispatch
from lib.cominterop_util import preferComDispatch, is_pywin32, AreEqual
from lib.process_util    import launch_ironpython_changing_extensions
if not preferComDispatch and not is_pywin32:
    print
    print "#" * 80
    print "Re-running %s under '-X:PreferComDispatch' mode." % (__file__)
    AreEqual(launch_ironpython_changing_extensions(__file__, add=["-X:PreferComDispatch"]), 0)
    
sys.exit(failed)
