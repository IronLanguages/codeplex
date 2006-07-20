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

def test_sanity():
    x = datetime.date(2005,3,22)
    AreEqual(x.year, 2005)
    AreEqual(x.month, 3)
    AreEqual(x.day, 22)

    AreEqual(x.strftime("%y-%a-%b"), "05-Tue-Mar")
    AreEqual(x.strftime("%Y-%A-%B"), "2005-Tuesday-March")

    AssertError(ValueError, datetime.date, 2005, 4,31)
    AssertError(ValueError, datetime.date, 2005, 3,32)
    
    x = datetime.datetime(2006,4,11,2,28,3,99,datetime.tzinfo())
    AreEqual(x.year, 2006)
    AreEqual(x.month, 4)
    AreEqual(x.day, 11)
    AreEqual(x.hour, 2)
    AreEqual(x.minute, 28)
    AreEqual(x.second, 3)
    AreEqual(x.microsecond, 99)
    

run_test(__name__)