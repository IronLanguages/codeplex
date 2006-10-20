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

def operator_add(a, b) :
    return a + b

def test_add(a,b,c):
    Assert(c == b + a)
    Assert(a + b == c)
    Assert(c - a == b)
    Assert(c - b == a)

def operator_sub(a, b) :
    return a - b

def test_sub(a,b,c):
    Assert(c == -(b - a))
    Assert(c == a - b)
    Assert(a == b + c)
    Assert(b == a - c)

def operator_mul(a, b) :
    return a * b

def test_mul(a,b,c):
    Assert(c == a * b)
    Assert(c == b * a)
    if a != 0:
        Assert(b == c / a)
    if b != 0:
        Assert(a == c / b)

def operator_div(a, b) :
    if b != 0:
        return a / b

def test_div(a,b,c):
    if b != 0:
        Assert(a / b == c)
        Assert(((c * b) + (a % b)) == a)

def operator_mod(a, b) :
    if b != 0:
        return a % b

def test_mod(a,b,c):
    if b != 0:
        Assert(a % b == c)
        Assert((a / b) * b + c == a)
        Assert((a - c) % b == 0)

def operator_and(a, b) :
    return a & b

def test_and(a,b,c):
    Assert(a & b == c)
    Assert(b & a == c)

def operator_or(a, b) :
    return a | b

def test_or(a,b,c):
    Assert(a | b == c)
    Assert(b | a == c)

def operator_xor(a, b) :
    return a ^ b

def test_xor(a,b,c):
    Assert(a ^ b == c)
    Assert(b ^ a == c)

pats = [0L, 1L, 42L, 0x7fffffffL, 0x80000000L, 0xabcdef01L, 0xffffffffL]
nums = []
for p0 in pats:
    for p1 in pats:
        #for p2 in pats:
            n = p0+(p1<<32)
            nums.append(n)
            nums.append(-n)

bignums = []
for p0 in pats:
    for p1 in pats:
        for p2 in pats:
            n = p0+(p1<<32)+(p2<<64)
            bignums.append(n)
            bignums.append(-n)

ops = [
    ('/', operator_div, test_div),
    ('+', operator_add, test_add),
    ('-', operator_sub, test_sub),
    ('*', operator_mul, test_mul),
    ('%', operator_mod, test_mod),
    ('&', operator_and, test_and),
    ('|', operator_or,  test_or),
    ('^', operator_xor, test_xor),
]

def test_it_all(nums):
    for sym, op, test in ops:
        for x in nums:
            for y in nums:
                z = op(x, y)
                try:
                    test(x,y,z)
                except:
                    print x, " ", sym, " ", y, " ", z, "Failed"
                    raise

test_it_all(bignums)
test_it_all(nums)
