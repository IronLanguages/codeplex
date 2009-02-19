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

from Util.Debug import *

def always_true():
    exec "assert 1 / 2 == 0"
    exec "from __future__ import division; assert 1/2 == 0.5"
    AreEqual(1/2, 0)
    AreEqual(eval("1/2"), 0)

tempfile = "temp_future.py"

code1  = '''
exec "assert 1/2 == 0"
exec "from __future__ import division; assert 1/2 == 0.5"
assert 1/2 == 0
assert eval('1/2') == 0
'''

code2 = '''
from __future__ import division
exec "assert 1/2 == 0.5"
exec "from __future__ import division; assert 1/2 == 0.5"
assert 1/2 == 0.5
assert eval('1/2') == 0.5
'''

def f1(): execfile(tempfile)
def f2(): exec(compile(code, tempfile, "exec"))
def f3(): exec(code)
def f4():
    if is_cli:
        import IronPython
        #pe = IronPython.Hosting.PythonEngine()
        #issue around py hosting py again.
        
always_true()
try: 
    import sys
    save = sys.path[:]
    sys.path.append(".")
    
    for code in (code1, code2):        
        always_true()
        text_to_file(code, tempfile)

        for f in (f1, f2, f3, f4):
            f()
            always_true()

        # test after importing
        import temp_future
        always_true()
        reloaded = reload(temp_future)
        always_true()
        
finally:    
    sys.path = save
    delete_files(tempfile)  