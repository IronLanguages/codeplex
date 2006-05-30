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
from lib.file_util import *

import marshal

tfn = path_combine(testpath.temporary_dir, 'tempfile.bin')

# a couple of lines are disabled due to 1032

def test_functionality():
    objects = [ None, 
                True, False, 
                '', 'a', 'abc', 
                -3, 0, 10, 
                254, -255, 256, 257, 
                65534, 65535, -65536,                
                3.1415926,
                
                #0L, 
                #-1234567890123456789,
                #[], 
                ['abc'], [1, 2],
                #tuple(), 
                (1,), (1,2,3),
                #{}, 
                {1:2}, {1:2, 4:'4', 5:None},
                0+1j, 2-3.23j,
              ]
    if is_cli:
        import System
        objects.extend(
            [ 
            #System.Single.Parse('-3.1415'),
            System.Int64.Parse('2345678'), 
            
            ])

    # dumps / loads
    for x in objects: 
        s = marshal.dumps(x)
        x2 = marshal.loads(s)        
        AreEqual(x, x2)
        
        s2 = marshal.dumps(x2)
        AreEqual(s, s2)  

    # dump / load
    for x in objects:
        f = file(tfn, 'wb')
        marshal.dump(x, f)
        f.close()
        
        f = file(tfn, 'rb')
        x2 = marshal.load(f)
        f.close()        
        AreEqual(x, x2)
    
def test_buffer():
    for s in ['', ' ', 'abc ', 'abcdef']:
        x = marshal.dumps(buffer(s))
        AreEqual(marshal.loads(x), s)

    for s in ['', ' ', 'abc ', 'abcdef']:
        f = file(tfn, 'wb')
        marshal.dump(buffer(s), f)
        f.close()
        
        f = file(tfn, 'rb')
        x2 = marshal.load(f)
        f.close()
        AreEqual(s, x2)

def test_negative():
    AssertError(TypeError, marshal.dump, 2, None)
    AssertError(TypeError, marshal.load, '-1', None)
    
    l = [1, 2]
    l.append(l)
    AssertError(ValueError, marshal.dumps, l) ## infinite loop
    
    class my: pass
    AssertError(ValueError, marshal.dumps, my())  ## unmarshallable object
    
run_test(__name__)    