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

Assert(range(10) == [0, 1, 2, 3, 4, 5, 6, 7, 8, 9])
Assert(range(0) == [])
Assert(range(-10) == [])

Assert(range(3,10) == [3, 4, 5, 6, 7, 8, 9])
Assert(range(10,3) == [])
Assert(range(-3,-10) == [])
Assert(range(-10,-3) == [-10, -9, -8, -7, -6, -5, -4])

Assert(range(3,20,2) == [3, 5, 7, 9, 11, 13, 15, 17, 19])
Assert(range(3,20,-2) == [])
Assert(range(20,3,2) == [])
Assert(range(20,3,-2) == [20, 18, 16, 14, 12, 10, 8, 6, 4])
Assert(range(-3,-20,2) == [])
Assert(range(-3,-20,-2) == [-3, -5, -7, -9, -11, -13, -15, -17, -19])
Assert(range(-20,-3, 2) == [-20, -18, -16, -14, -12, -10, -8, -6, -4])
Assert(range(-20,-3,-2) == [])


def testranges(r, o):
    Assert(len(r) == len(o))
    for i in range(len(r)):
        Assert(r[i]==o[i])
        Assert(r[1-i] == o[1-i])

def tr_1(n):
    testranges(xrange(n), range(n))

def tr_2(m, n):
    testranges(xrange(m, n), range(m, n))

def tr_3(m, n, s):
    testranges(xrange(m, n, s), range(m, n, s))

tr_1(10)
tr_1(0)
tr_1(-10)

tr_2(3,10)
tr_2(10,3)
tr_2(-3,-10)
tr_2(-10,-3)
tr_3(3,20,2)
tr_3(3,20,-2)
tr_3(20,3,2)
tr_3(20,3,-2)
tr_3(-3,-20,2)
tr_3(-3,-20,-2)
tr_3(-20,-3, 2)
tr_3(-20,-3,-2)

tr_3(7,-20, 4)
tr_3(7,-20, -4)
tr_3(7,-21, 4)
tr_3(7,-21, -4)
tr_3(7,-22, 4)
tr_3(7,-22, -4)
tr_3(7,-23, 4)
tr_3(7,-23, -4)

tr_3(-7,20, 4)
tr_3(-7,20, -4)
tr_3(-7,21, 4)
tr_3(-7,21, -4)
tr_3(-7,22, 4)
tr_3(-7,22, -4)
tr_3(-7,23, 4)
tr_3(-7,23, -4)

import sys
x = xrange(0, sys.maxint, sys.maxint-1)
AreEqual(x[0], 0)
AreEqual(x[1], sys.maxint-1)

# coverage

AreEqual(str(xrange(0, 3, 1)), "xrange(3)")
AreEqual(str(xrange(1, 3, 1)), "xrange(1, 3)")
AreEqual(str(xrange(0, 5, 2)), "xrange(0, 6, 2)")

AreEqual([x for x in xrange(5L)], range(5))
AreEqual([x for x in xrange(10L, 15L)], range(10, 15))
AreEqual([x for x in xrange(10L, 15L, 2)], range(10, 15,2 ))

AssertError(TypeError, lambda: xrange(4) + 4)
AssertError(TypeError, lambda: xrange(4) * 4)
AssertError(TypeError, lambda: xrange(4)[:2])