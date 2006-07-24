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

import time

def test_strftime():
    x = time.strftime('%x %X', time.localtime())
    
    Assert(len(x) > 3)
    
    x1 = time.strftime('%x', time.localtime())
    x2 = time.strftime('%X', time.localtime())
    
    Assert(len(x1) > 1)
    Assert(len(x2) > 1)
    
    AreEqual(x, x1 + ' ' + x2)

def test_strptime():
    import time
    d = time.strptime("July 3, 2006 At 0724 GMT", "%B %d, %Y At %H%M GMT") 
    AreEqual(d[0], 2006)
    AreEqual(d[1], 7)
    AreEqual(d[2], 3)
    AreEqual(d[3], 7)
    AreEqual(d[4], 24)
    AreEqual(d[5], 0)
    AreEqual(d[6], 0)
    AreEqual(d[7], 184)
    # CPY & IPY differ on daylight savings time for this parse
        
    AssertError(ValueError, time.strptime, "July 3, 2006 At 0724 GMT", "%B %x, %Y At %H%M GMT")
    
def test_sleep():
    x = time.clock()
    time.sleep(1)
    y = time.clock()
    Assert(y-x > .95 and y-x < 1.30)  # make sure we're close...


run_test(__name__)