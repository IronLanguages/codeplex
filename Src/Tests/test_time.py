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

x = time.strftime('%x %X', time.localtime())

Assert(len(x) > 3)

x1 = time.strftime('%x', time.localtime())
x2 = time.strftime('%X', time.localtime())

Assert(len(x1) > 1)
Assert(len(x2) > 1)

AreEqual(x, x1 + ' ' + x2)


x = time.clock()
time.sleep(1)
y = time.clock()
Assert(y-x > .95 and y-x < 1.10)  # make sure we're close...

