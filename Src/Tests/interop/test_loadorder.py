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
import sys, nt

def environ_var(key):
    return [nt.environ[x] for x in nt.environ.keys() if x.lower() == key.lower()][0]

merlin_root = environ_var("MERLIN_ROOT")
sys.path.append(merlin_root + r"\Languages\IronPython\Tests")

from lib.assert_util import *
from lib.process_util import *

skiptest("silverlight")

saved = environ_var('IRONPYTHONPATH')
nt.environ['IRONPYTHONPATH'] = merlin_root + r"\Test\ClrAssembly;" + merlin_root + r"\Languages\IronPython\Tests"

try:
    directory = testpath.public_testdir + r"\interop\loadorder"
    count = 0
    for x in nt.listdir(directory):
        if not x.startswith("t") or not x.endswith(".py"):
            continue
        
        # skip list
        if x in [ 't1a.py', 't1b.py', 't1c.py', 't1d.py', 't6.py' ]:
            continue
        
        # running ipy with parent's switches
        result = launch_ironpython_changing_extensions(directory + "\\" + x)
        
        if result == 0: 
            print "%s: pass" % x
        else:
            count += 1 
            print "%s: fail" % x

    if count != 0:
        sys.exit(1)
finally: 
    nt.environ['IRONPYTHONPATH'] = saved