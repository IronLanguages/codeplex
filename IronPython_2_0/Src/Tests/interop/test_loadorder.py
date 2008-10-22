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
from iptest.process_util import *

skiptest("silverlight")

directory = testpath.public_testdir + r"\interop\loadorder"

count = 0
for x in nt.listdir(directory):
    if not x.startswith("t") or not x.endswith(".py"):
        continue
    
    # skip list
    if x in [ 't6.py' ]:
        continue
    
    # running ipy with parent's switches
    result = launch_ironpython_changing_extensions(directory + "\\" + x)
    
    if result == 0: 
        print "%s: pass" % x
    else:
        count += 1 
        print "%s: fail" % x

if count != 0:
    Fail("there are %s failures" % count)
