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

from exceptions import SystemExit
from lib.assert_util import testpath

if testpath.basePyDir.lower()=='src':
    import sys
    print "Skipping DLR COM Lib tests..."
    sys.exit(0)

for test_module in ["dlrcomserver", "paramsinretval"]:
    print "--------------------------------------------------------------------"
    print "Importing", test_module, "..."
    try:
        __import__(test_module)
    except SystemExit, e:
        if e.code!=0: 
            raise Exception("Importing '%s' caused an unexpected exit code: %s" % (test_module, str(e.code)))
    print ""
