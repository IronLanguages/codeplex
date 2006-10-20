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

##
## Test the datetime module
## 

from lib.assert_util import *
import datetime

def test_date():
    #--------------------------------------------------------------------------
    #basic sanity checks
    x = datetime.date(2005,3,22)
    AreEqual(x.year, 2005)
    AreEqual(x.month, 3)
    AreEqual(x.day, 22)

    AreEqual(x.strftime("%y-%a-%b"), "05-Tue-Mar")
    AreEqual(x.strftime("%Y-%A-%B"), "2005-Tuesday-March")
    AreEqual(x.strftime("%Y%m%d"), '20050322')
    
    datetime.date(1,1,1)
    datetime.date(9999, 12, 31)
    datetime.date(2004, 2, 29)
    
    AssertError(ValueError, datetime.date, 2005, 4,31)
    AssertError(ValueError, datetime.date, 2005, 3,32)
    AssertError(ValueError, datetime.datetime, 2006, 2, 29)
    AssertError(ValueError, datetime.datetime, 2006, 9, 31)
    
    AssertError(ValueError, datetime.date, 0, 1, 1)
    AssertError(ValueError, datetime.date, 1, 0, 1)
    AssertError(ValueError, datetime.date, 1, 1, 0)
    AssertError(ValueError, datetime.date, 0, 0, 0)
    AssertError(ValueError, datetime.date, -1, 1, 1)
    AssertError(ValueError, datetime.date, 1, -1, 1)
    AssertError(ValueError, datetime.date, 1, 1, -1)
    AssertError(ValueError, datetime.date, -1, -1, -1)
    AssertError(ValueError, datetime.date, -10, -10, -10)
    AssertError(ValueError, datetime.date, 10000, 12, 31)
    AssertError(ValueError, datetime.date, 9999, 13, 31)
    AssertError(ValueError, datetime.date, 9999, 12, 32)
    AssertError(ValueError, datetime.date, 10000, 13, 32)
    AssertError(ValueError, datetime.date, 100000, 130, 320)
    
def test_datetime():
    x = datetime.datetime(2006,4,11,2,28,3,99,datetime.tzinfo())
    AreEqual(x.year, 2006)
    AreEqual(x.month, 4)
    AreEqual(x.day, 11)
    AreEqual(x.hour, 2)
    AreEqual(x.minute, 28)
    AreEqual(x.second, 3)
    AreEqual(x.microsecond, 99)
    
    datetime.datetime(1, 1, 1, 0, 0, 0, 0) #min
    datetime.datetime(9999, 12, 31, 23, 59, 59, 999999) #max
    datetime.datetime(2004, 2, 29, 16, 20, 22, 262000) #leapyear
    
    AssertError(ValueError, datetime.datetime, 2006, 2, 29, 16, 20, 22, 262000) #bad leapyear
    AssertError(ValueError, datetime.datetime, 2006, 9, 31, 16, 20, 22, 262000) #bad number of days
    
    AssertError(ValueError, datetime.datetime, 0, 1, 1, 0, 0, 0, 0)
    AssertError(ValueError, datetime.datetime, 1, 0, 1, 0, 0, 0, 0)
    AssertError(ValueError, datetime.datetime, 1, 1, 0, 0, 0, 0, 0)
    
    AssertError(ValueError, datetime.datetime, -1, 1, 1, 0, 0, 0, 0)
    AssertError(ValueError, datetime.datetime, 1, -1, 1, 0, 0, 0, 0)
    AssertError(ValueError, datetime.datetime, 1, 1, -1, 0, 0, 0, 0)
    AssertError(ValueError, datetime.datetime, 1, 1, 1, -1, 0, 0, 0)
    AssertError(ValueError, datetime.datetime, 1, 1, 1, 0, -1, 0, 0)
    AssertError(ValueError, datetime.datetime, 1, 1, 1, 0, 0, -1, 0)
    #Codeplex WorkItem #3850
    #AssertError(ValueError, datetime.datetime, 1, 1, 1, 0, 0, 0, -1)
    AssertError(ValueError, datetime.datetime, -10, -10, -10, -10, -10, -10, -10)

    AssertError(ValueError, datetime.datetime, 10000, 12, 31, 23, 59, 59, 999999)
    AssertError(ValueError, datetime.datetime, 9999, 13, 31, 23, 59, 59, 999999)
    AssertError(ValueError, datetime.datetime, 9999, 12, 32, 23, 59, 59, 999999)
    AssertError(ValueError, datetime.datetime, 9999, 12, 31, 24, 59, 59, 999999)
    AssertError(ValueError, datetime.datetime, 9999, 12, 31, 23, 60, 59, 999999)
    AssertError(ValueError, datetime.datetime, 9999, 12, 31, 23, 59, 60, 999999)
    AssertError(ValueError, datetime.datetime, 9999, 12, 31, 23, 59, 59, 1000000)
    AssertError(ValueError, datetime.datetime, 10000, 13, 32, 24, 60, 60, 1000000)
    AssertError(ValueError, datetime.datetime, 100000, 130, 320, 240, 600, 600, 10000000)
    
    #--------------------------------------------------------------------------
    #--Test subtraction
    test_data = { ((2006, 9, 29, 15, 37, 28, 686000), (2006, 9, 29, 15, 37, 28, 686000)) : ((0, 0, 0),(0, 0, 0)),
                  ((2006, 9, 29, 15, 37, 28, 686000), (2007, 9, 29, 15, 37, 28, 686000)) : ((365, 0, 0),(-365, 0, 0)),
                  ((2006, 9, 29, 15, 37, 28, 686000), (2006,10, 29, 15, 37, 28, 686000)) : ((30, 0, 0),(-30, 0, 0)),
                  ((2006, 9, 29, 15, 37, 28, 686000), (2006, 9, 30, 15, 37, 28, 686000)) : ((1, 0, 0),(-1, 0, 0)),
                  #Codeplex WorkItem 3990
                  #((2006, 9, 29, 15, 37, 28, 686000), (2006, 9, 29, 16, 37, 28, 686000)) : ((0, 3600, 0),(-1, 82800, 0)),
                  #((2006, 9, 29, 15, 37, 28, 686000), (2006, 9, 29, 15, 38, 28, 686000)) : ((0, 60, 0),(-1, 86340, 0)),
                  #((2006, 9, 29, 15, 37, 28, 686000), (2006, 9, 29, 15, 37, 29, 686000)) : ((0, 1, 0),(-1, 86399, 0)),
                  #((2006, 9, 29, 15, 37, 28, 686000), (2006, 9, 29, 15, 37, 28, 686001)) : ((0, 0, 1),(-1, 86399, 999999)),
                  ((1, 1, 1, 0, 0, 0, 0), (1, 1, 1, 0, 0, 0, 0)) : ((0, 0, 0),(0, 0, 0)),
                  ((9999, 12, 31, 23, 59, 59, 999999), (9999, 12, 31, 23, 59, 59, 999999)) : ((0, 0, 0),(0, 0, 0))
                  }
    for key, (value0, value1) in test_data.iteritems():
        dt1 = datetime.datetime(*key[1])
        dt0 = datetime.datetime(*key[0])
        
        x = dt1 - dt0
        AreEqual(x.days, value0[0])
        AreEqual(x.seconds, value0[1])
        AreEqual(x.microseconds, value0[2])
        
        y = dt0 - dt1
        AreEqual(y.days, value1[0])
        AreEqual(y.seconds, value1[1])
        AreEqual(y.microseconds, value1[2])
        
            
    #--------------------------------------------------------------------------
    #--Test addition
    #TODO
    
    #--------------------------------------------------------------------------
    #--Test <, >, etc
    #TODO
        
def test_timedelta():
    #TODO
    pass
    
def test_time():
    #TODO
    pass
    
def test_tzinfo():
    #TODO
    pass
    
    
run_test(__name__)