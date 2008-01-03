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

from lib.assert_util import *

import time

def test_strftime():
    t = time.localtime()
    x = time.strftime('%x %X', t)
    
    Assert(len(x) > 3)
    
    x1 = time.strftime('%x', t)
    x2 = time.strftime('%X', t)
    
    Assert(len(x1) > 1)
    Assert(len(x2) > 1)
    
    AreEqual(x, x1 + ' ' + x2)
    
    t = time.gmtime()
    AreEqual(time.strftime('%c', t), time.strftime('%x %X', t))

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
    
    #CodePlex Work Item 2557
    AreEqual((2006, 7, 3, 7, 24, 0, 0, 184, -1), time.strptime("%07/03/06 07:24:00", "%%%c"))
    if not is_cli and not is_silverlight:
        AreEqual((1900, 6, 1, 0, 0, 0, 4, 152, -1), time.strptime("%6", "%%%m"))
        
    
    # CPY & IPY differ on daylight savings time for this parse
        
    AssertError(ValueError, time.strptime, "July 3, 2006 At 0724 GMT", "%B %x, %Y At %H%M GMT")
    
#Skip under silverlight because we cannot determine whether the AMD processor bug applies
#here or not.
@skip("silverlight")
def test_sleep():
    #The QueryPerformanceCounter() system  call is broken in XP and 2K3 (see 
    #http://channel9.msdn.com/ShowPost.aspx?PostID=156175) for 
    #certain AMD64 multi-proc machines.
    #This means that randomly y can end up being less than x.
    if get_environ_variable("PROCESSOR_REVISION")=="0508" and get_environ_variable("PROCESSOR_LEVEL")=="15":
        print "Bailing test_sleep for certain AMD64 machines!"
        return
        
    sleep_time = 5
    safe_deviation = 0.20

    x = time.clock()
    time.sleep(sleep_time)
    y = time.clock()
    
    print
    print "x is", x
    print "y is", y
    
    if y>x:
        Assert(y-x > sleep_time*(1-(safe_deviation/2)))
        Assert(y-x < sleep_time*(1+safe_deviation))  # make sure we're close...

@disabled("CodePlex Work Item 11733")
def test_dst():
    AreEqual(time.altzone, time.timezone+[3600,-3600][time.daylight])
    t = time.time()
    AreEqual(time.mktime(time.gmtime(t))-time.mktime(time.localtime(t)), time.timezone)

def test_tzname():
    AreEqual(type(time.tzname), tuple)
    AreEqual(len(time.tzname), 2)

run_test(__name__)