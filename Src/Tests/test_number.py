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
from System import *
import clr

Assert(Single.IsInfinity(Single.PositiveInfinity))
Assert(not Single.IsInfinity(1.0))

x = ['a','c','d','b',1,3,2, 2.5, -2]
x.sort()
Assert(x == [-2, 1, 2, 2.5, 3, 'a', 'b', 'c', 'd'])

x = [333, 1234.5, 1, 333, -1, 66.6]
x.sort()
Assert(x == [-1, 1, 66.6, 333, 333, 1234.5])

Assert(10 < 76927465928764592743659287465928764598274369258736489327465298374695287346592837496)
Assert(76927465928764592743659287465928764598274369258736489327465298374695287346592837496 > 10)


x = 3e1000
Assert(Double.IsInfinity(x))
Assert(Double.IsPositiveInfinity(x))
x = -3e1000
Assert(Double.IsInfinity(x))
Assert(Double.IsNegativeInfinity(x))
x = 3e1000 - 3e1000
Assert(Double.IsNaN(x))

f_x = "4.75"
f_y = "3.25"
i_x = "4"
i_y = "3"

def Parse(type, value):
    return type.Parse(value, Globalization.CultureInfo.InvariantCulture.NumberFormat)

def VerifyTypes(v):
    AreEqual(v.x.GetType().ToString(), v.n)
    AreEqual(v.y.GetType().ToString(), v.n)

class float:
    def __init__(self, type, name):
        self.x = Parse(type, f_x)
        self.y = Parse(type, f_y)
        self.n = name

class fixed:
    def __init__(self, type, name):
        self.x = type.Parse(i_x)
        self.y = type.Parse(i_y)
        self.n = name

s = float(Single, "System.Single")
d = float(Double, "System.Double")
sb = fixed(SByte, "System.SByte")
sh = fixed(Int16, "System.Int16")
i = fixed(Int32, "System.Int32")
l = fixed(Int64, "System.Int64")
ub = fixed(Byte, "System.Byte")
ui = fixed(UInt32, "System.UInt32")
ul = fixed(UInt64, "System.UInt64")

def float_test(x,y):
    Assert(x + y == y + x)
    Assert(x * y == y * x)
    Assert(x / y == x / y)
    Assert(x % y == x % y)
    Assert(x - y == -(y - x))
    Assert(x ** y == x ** y)
    Assert(x // y == x // y)
    z = x
    z /= y
    Assert(z == x / y)
    z = x
    z *= y
    Assert(z == x * y)
    z = x
    z %= y
    Assert(z == x % y)
    z = x
    z += y
    Assert(z == x + y)
    z = x
    z -= y
    Assert(z == x - y)
    z = x
    z **= y
    Assert(z == x ** y)
    z = x
    z //= y
    Assert(z == x // y)
    Assert((x < y) == (not (x >= y)))
    Assert((x <= y) == (not (x > y)))
    Assert((x > y) == (not (x <= y)))
    Assert((x >= y) == (not (x < y)))
    Assert((x != y) == (not (x == y)))
    AreEqual((x == y), (y == x))
    Assert((x == y) == (y == x))
    Assert((x == y) == (not (x != y)))

def type_test(tx, ty):
    x = tx.x
    y = ty.y
    float_test(x,x)
    float_test(x,y)
    float_test(y,y)
    float_test(y,x)

test_types = [s,d,i,l]
# BUG 10 : Add support for unsigned integer types (and other missing data types)
#test_types = [s,d,i,l,sb,sh,ub,ui,ul]
# /BUG

for a in test_types:
    VerifyTypes(a)
    for b in test_types:
        VerifyTypes(b)
        type_test(a, b)
        type_test(b, a)


from lib.assert_util import *
load_iron_python_test()
from IronPythonTest import *

# implicit conversions (conversion defined on Derived)

a = ConversionStorage()
b = Base(5)
d = Derived(23)


a.Base = d
AreEqual(a.Base.value, d.value)
a.Derived = d
AreEqual(a.Derived.value, d.value)

a.Base = b
AreEqual(a.Base.value, b.value)


def assignBaseToDerived(storage, base):
    storage.Derived = base
    
AssertError(TypeError, assignBaseToDerived, a, b)


# implicit conversions (conversion defined on base)


a = ConversionStorage()
b = Base2(5)
d = Derived2(23)


a.Base2 = d
AreEqual(a.Base2.value, d.value)
a.Derived2 = d
AreEqual(a.Derived2.value, d.value)

a.Base2 = b
AreEqual(a.Base2.value, b.value)


def assignBaseToDerived(storage, base):
    storage.Derived2 = base
    
AssertError(TypeError, assignBaseToDerived, a, b)


class myFakeInt:
    def __int__(self):
        return 23

class myFakeLong:
    def __long__(self):
        return 23L

class myFakeComplex:
    def __complex__(self):
        return 0j + 23

class myFakeFloat:
    def __float__(self):
        return 23.0

class myNegative:
    def __pos__(self):
        return 23

AreEqual(int(myFakeInt()), 23)
AreEqual(long(myFakeLong()), 23L)
AreEqual(complex(myFakeComplex()), 0j + 23)
AreEqual(__builtins__.float(myFakeFloat()), 23.0)   # we redefined float above, go directly to the real float...
AreEqual(+myNegative(), 23)


# True/False and None...  They shouldn't convert to each other, but
# a truth test against none should always be false.

AreEqual(False == None, False)
AreEqual(True == None, False)
AreEqual(None == False, False)
AreEqual(None == True, False)

if None: AreEqual(False, True)

a = None
if a: AreEqual(False, True)

# Enum conversions

class EnumRec:
    def __init__(self, code, min, max, enum, test):
        self.code = code
        self.min = min
        self.max = max
        self.enum = enum
        self.test = test

enum_types = [
    EnumRec("SByte", -128, 127, EnumSByte, EnumTest.TestEnumSByte),
    EnumRec("Byte", 0, 255, EnumByte, EnumTest.TestEnumByte),
    EnumRec("Short", -32768, 32767, EnumShort, EnumTest.TestEnumShort),
    EnumRec("UShort", 0, 65535, EnumUShort, EnumTest.TestEnumUShort),
    EnumRec("Int", -2147483648, 2147483647, EnumInt, EnumTest.TestEnumInt),
    EnumRec("UInt", 0, 4294967295, EnumUInt, EnumTest.TestEnumUInt),
    EnumRec("Long", -9223372036854775808, 9223372036854775807, EnumLong, EnumTest.TestEnumLong),
    EnumRec("ULong", 0, 18446744073709551615, EnumULong, EnumTest.TestEnumULong),
]

value_names = ["Zero"]
value_values = {"Zero" : 0}
for e in enum_types:
    value_names.append("Min" + e.code)
    value_names.append("Max" + e.code)
    value_values["Min" + e.code] = e.min
    value_values["Max" + e.code] = e.max

for enum in enum_types:
    for name in value_names:
        val = value_values[name]
        if hasattr(enum.enum, name):
            for test in enum_types:
                func = test.test
                ev = getattr(enum.enum, name)
                if test.min <= val and val <= test.max:
                    func(ev)
                else:
                    try:
                        func(ev)
                    except:
                        pass
                    else:
                        Assert(False)
                EnumTest.TestEnumBoolean(ev)

#***** Above code are from 'conversions' *****

#***** Copying from 'Arithmetics' *****

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

#***** Copying from 'comparison' *****

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

def test_scenarios(templates, cmps):
    values = [3.5, 4.5, 4, 0, -200L, 12345678901234567890]
    for l in values:
        for r in values:
            for t in templates:
                for c in cmps:
                    easy = "%s %s %s" % (l, c, r)
                    inst = t % (l, c, r)
                    #print inst, eval(easy), eval(inst)
                    Assert(eval(easy) == eval(inst))
                   

templates1 = [ "C(%s) %s C(%s)", "C2(%s) %s C2(%s)",
               "C(%s) %s D(%s)", "D(%s) %s C(%s)", 
               "C2(%s) %s D(%s)", "D(%s) %s C2(%s)", 
               "C(%s) %s D2(%s)", "D2(%s) %s C(%s)", 
               "C2(%s) %s D2(%s)", "D2(%s) %s C2(%s)"]
templates2 = [x for x in templates1 if x.startswith('C')]

# OldClass: both C and D define __lt__
class C:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class D:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">"])

# OldClass: C defines __lt__, D does not
class C:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class D:
    def __init__(self, value):
        self.value = value
class C2(C): pass
class D2(D): pass
test_scenarios(templates2, ["<"])

# UserType: both C and D define __lt__
class C(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class D(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">"])

# UserType: C defines __lt__, D does not
class C(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class D(object):
    def __init__(self, value):
        self.value = value
class C2(C): pass
class D2(D): pass
test_scenarios(templates2, ["<"])

# Mixed: both C and D define __lt__
class C:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value

class D(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">"])

# Mixed, with all cmpop
class C(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
    def __gt__(self, other):
        return self.value > other.value
    def __le__(self, other):
        return self.value <= other.value
    def __ge__(self, other):
        return self.value >= other.value

class D:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
    def __gt__(self, other):
        return self.value > other.value
    def __le__(self, other):
        return self.value <= other.value
    def __ge__(self, other):
        return self.value >= other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">", "<=", ">="])

# verify two instances of class compare differently

Assert( (cmp(C(3), C(3)) == 0) == False)        
Assert( (cmp(D(3), D(3)) == 0) == False)        
Assert( (cmp(C2(3), C2(3)) == 0) == False)        
Assert( (cmp(D2(3), D2(3)) == 0) == False)        
      
Assert( (cmp(D(5), C(5)) == 0) == False)        
Assert( (cmp(C(3), C(5)) == -1) == True)        
Assert( (cmp(D2(5), C(3)) == 1) == True)        
Assert( (cmp(D(5), C2(8)) == -1) == True)  

# define __cmp__; do not move this before those above cmp testing
class C:
    def __init__(self, value):
        self.value = value    
    def __cmp__(self, other):
        return self.value - other.value

class D:
    def __init__(self, value):
        self.value = value    
    def __cmp__(self, other):
        return self.value - other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">", ">=", "<="])

Assert( (cmp(C(3), C(3)) == 0) == True)        
Assert( (cmp(C2(3), D(3)) == 0) == True)        
Assert( (cmp(C(3.0), D2(4.6)) > 0) == False)        
Assert( (cmp(D(3), C(4.9)) < 0) == True)        
Assert( (cmp(D2(3), D2(1234567890)) > 0) == False)        

class C(object):
    def __init__(self, value):
        self.value = value    
    def __cmp__(self, other):
        return self.value - other.value

class D(object):
    def __init__(self, value):
        self.value = value    
    def __cmp__(self, other):
        return self.value - other.value
class C2(C): pass
class D2(D): pass
test_scenarios(templates1, ["<", ">", ">=", "<="])

Assert( (cmp(C(3), C(3)) == 0) == True)        
Assert( (cmp(C2(3.4), D(3.4)) == 0) == True)        
Assert( (cmp(C(3.3), D2(4.9232)) > 0) == False)        
Assert( (cmp(D(3L), C(4000000000)) < 0) == True)        
Assert( (cmp(D2(3), D2(4.9)) < 0) == True)        


from lib.assert_util import *
load_iron_python_test()
from IronPythonTest import ComparisonTest

def test_comparisons(typeObj):
    class Callback:
        called = False
        def __call__(self, value):
            #print value, expected
            AreEqual(value, expected)
            self.called = True
        def check(self):
            Assert(self.called)
            self.called = False

    cb = Callback()
    ComparisonTest.report = cb
    
    values = [3.5, 4.5, 4, 0]

    for l in values:
        for r in values:
            ctl = typeObj(l)
            ctr = typeObj(r)

            AreEqual(str(ctl), "ct<%s>" % str(l))
            AreEqual(str(ctr), "ct<%s>" % str(r))

            expected = "< on [ct<%s>, ct<%s>]" % (l, r)
            AreEqual(ctl < ctr, l < r)
            cb.check()
            expected = "> on [ct<%s>, ct<%s>]" % (l, r)
            AreEqual(ctl > ctr, l > r)
            cb.check()
            expected = "<= on [ct<%s>, ct<%s>]" % (l, r)
            AreEqual(ctl <= ctr, l <= r)
            cb.check()
            expected = ">= on [ct<%s>, ct<%s>]" % (l, r)
            AreEqual(ctl >= ctr, l >= r)
            cb.check()
            
class ComparisonTest2(ComparisonTest): pass
    
test_comparisons(ComparisonTest)
test_comparisons(ComparisonTest2)

class C:
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
    def __gt__(self, other):
        return self.value > other.value     
class C2(C): pass        
D = ComparisonTest
D2 = ComparisonTest2  
test_scenarios(templates1, ["<", ">"])

class C(object):
    def __init__(self, value):
        self.value = value
    def __lt__(self, other):
        return self.value < other.value
    def __gt__(self, other):
        return self.value > other.value     
class C2(C): pass     

# test_scenarios(templates1, ["<", ">"])

ComparisonTest.report = None
Assert( (cmp(ComparisonTest(5), ComparisonTest(5)) == 0) == False)        
Assert( (cmp(ComparisonTest(5), ComparisonTest(8)) == -1) == True)        
Assert( (cmp(ComparisonTest2(50), ComparisonTest(8)) == 1) == True)        


Assert( (None < None) == False)
Assert( (None > None) == False)
Assert( (None <= None) == True)
Assert( (None >= None) == True)
Assert( (None == "") == False)
Assert( (None != "") == True)
Assert( (None < "") == True)
Assert( (None > "") == False)
Assert( (None <= "") == True)
Assert( (None >= "") == False)

def check(c):
    Assert( (c < None) == False)
    Assert( (c > None) == True)
    Assert( (c <= None) == False)
    Assert( (c >= None) == True)
    Assert( (None < c) == True)
    Assert( (None > c) == False)
    Assert( (None <= c) == True)
    Assert( (None >= c) == False)

class C1: pass
class C2(object): pass
class C3(C2): pass

for x in [C1, C2, C3]:
    check(x())


ignore = '''    
############ Let us get some strange ones ############ 
# both C and D claims bigger
class C:
    def __lt__(self, other):
        return False
class D:
    def __lt__(self, other):
        return False

Assert( (C() < D()) == False )
Assert( (C() > D()) == False )
Assert( (D() < C()) == False )
Assert( (D() > C()) == False )

# C is always larger
class C(object):
    def __lt__(self, other):
        return False
        
        
class D: pass

Assert( (C() < D()) == False )
Assert( (C() > D()) == True )
Assert( (D() < C()) == True )
Assert( (D() > C()) == False )
'''

#***** Copying from 'Integers' *****

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

import sys
from lib.assert_util import *
load_iron_python_test()
from IronPythonTest import IntegerTest as it
import System

def f():
    Assert(it.AreEqual(it.UInt32Int32MaxValue,it.uintT(it.Int32Int32MaxValue)))
    Assert(it.AreEqual(it.UInt64Int32MaxValue,it.ulongT(it.Int32Int32MaxValue)))
    Assert(it.AreEqual(it.Int32Int32MaxValue,it.intT(it.Int32Int32MaxValue)))
    Assert(it.AreEqual(it.Int64Int32MaxValue,it.longT(it.Int32Int32MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Int32MaxValue)))
    Assert(it.AreEqual(it.Int32Int32MinValue,it.intT(it.Int32Int32MinValue)))
    Assert(it.AreEqual(it.Int64Int32MinValue,it.longT(it.Int32Int32MinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Int32MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int32UInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Int16MaxValue,it.uintT(it.Int32Int16MaxValue)))
    Assert(it.AreEqual(it.UInt16Int16MaxValue,it.ushortT(it.Int32Int16MaxValue)))
    Assert(it.AreEqual(it.UInt64Int16MaxValue,it.ulongT(it.Int32Int16MaxValue)))
    Assert(it.AreEqual(it.Int32Int16MaxValue,it.intT(it.Int32Int16MaxValue)))
    Assert(it.AreEqual(it.Int16Int16MaxValue,it.shortT(it.Int32Int16MaxValue)))
    Assert(it.AreEqual(it.Int64Int16MaxValue,it.longT(it.Int32Int16MaxValue)))
    Assert(it.AreEqual(it.CharInt16MaxValue,it.charT(it.Int32Int16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Int16MaxValue)))
    Assert(it.AreEqual(it.Int32Int16MinValue,it.intT(it.Int32Int16MinValue)))
    Assert(it.AreEqual(it.Int16Int16MinValue,it.shortT(it.Int32Int16MinValue)))
    Assert(it.AreEqual(it.Int64Int16MinValue,it.longT(it.Int32Int16MinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Int16MinValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.Int32UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.Int32UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.Int32UInt16MaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.Int32UInt16MaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.Int32UInt16MaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.Int32UInt16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int32UInt16MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int32UInt64MinValue)))
    Assert(it.AreEqual(it.UInt32ByteMaxValue,it.uintT(it.Int32ByteMaxValue)))
    Assert(it.AreEqual(it.UInt16ByteMaxValue,it.ushortT(it.Int32ByteMaxValue)))
    Assert(it.AreEqual(it.UInt64ByteMaxValue,it.ulongT(it.Int32ByteMaxValue)))
    Assert(it.AreEqual(it.Int32ByteMaxValue,it.intT(it.Int32ByteMaxValue)))
    Assert(it.AreEqual(it.Int16ByteMaxValue,it.shortT(it.Int32ByteMaxValue)))
    Assert(it.AreEqual(it.Int64ByteMaxValue,it.longT(it.Int32ByteMaxValue)))
    Assert(it.AreEqual(it.ByteByteMaxValue,it.byteT(it.Int32ByteMaxValue)))
    Assert(it.AreEqual(it.CharByteMaxValue,it.charT(it.Int32ByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32ByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int32ByteMinValue)))
    Assert(it.AreEqual(it.UInt32SByteMaxValue,it.uintT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.UInt16SByteMaxValue,it.ushortT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.UInt64SByteMaxValue,it.ulongT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMaxValue,it.intT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.Int16SByteMaxValue,it.shortT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.Int64SByteMaxValue,it.longT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.ByteSByteMaxValue,it.byteT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.SByteSByteMaxValue,it.sbyteT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.CharSByteMaxValue,it.charT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32SByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMinValue,it.intT(it.Int32SByteMinValue)))
    Assert(it.AreEqual(it.Int16SByteMinValue,it.shortT(it.Int32SByteMinValue)))
    Assert(it.AreEqual(it.Int64SByteMinValue,it.longT(it.Int32SByteMinValue)))
    Assert(it.AreEqual(it.SByteSByteMinValue,it.sbyteT(it.Int32SByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32SByteMinValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.Int32CharMaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.Int32CharMaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.Int32CharMaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.Int32CharMaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.Int32CharMaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.Int32CharMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32CharMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int32CharMinValue)))
    Assert(it.AreEqual(it.UInt32Val0,it.uintT(it.Int32Val0)))
    Assert(it.AreEqual(it.UInt16Val0,it.ushortT(it.Int32Val0)))
    Assert(it.AreEqual(it.UInt64Val0,it.ulongT(it.Int32Val0)))
    Assert(it.AreEqual(it.Int32Val0,it.intT(it.Int32Val0)))
    Assert(it.AreEqual(it.Int16Val0,it.shortT(it.Int32Val0)))
    Assert(it.AreEqual(it.Int64Val0,it.longT(it.Int32Val0)))
    Assert(it.AreEqual(it.ByteVal0,it.byteT(it.Int32Val0)))
    Assert(it.AreEqual(it.SByteVal0,it.sbyteT(it.Int32Val0)))
    Assert(it.AreEqual(it.CharVal0,it.charT(it.Int32Val0)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Val0)))
    Assert(it.AreEqual(it.UInt32Val1,it.uintT(it.Int32Val1)))
    Assert(it.AreEqual(it.UInt16Val1,it.ushortT(it.Int32Val1)))
    Assert(it.AreEqual(it.UInt64Val1,it.ulongT(it.Int32Val1)))
    Assert(it.AreEqual(it.Int32Val1,it.intT(it.Int32Val1)))
    Assert(it.AreEqual(it.Int16Val1,it.shortT(it.Int32Val1)))
    Assert(it.AreEqual(it.Int64Val1,it.longT(it.Int32Val1)))
    Assert(it.AreEqual(it.ByteVal1,it.byteT(it.Int32Val1)))
    Assert(it.AreEqual(it.SByteVal1,it.sbyteT(it.Int32Val1)))
    Assert(it.AreEqual(it.CharVal1,it.charT(it.Int32Val1)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Val1)))
    Assert(it.AreEqual(it.UInt32Val2,it.uintT(it.Int32Val2)))
    Assert(it.AreEqual(it.UInt16Val2,it.ushortT(it.Int32Val2)))
    Assert(it.AreEqual(it.UInt64Val2,it.ulongT(it.Int32Val2)))
    Assert(it.AreEqual(it.Int32Val2,it.intT(it.Int32Val2)))
    Assert(it.AreEqual(it.Int16Val2,it.shortT(it.Int32Val2)))
    Assert(it.AreEqual(it.Int64Val2,it.longT(it.Int32Val2)))
    Assert(it.AreEqual(it.ByteVal2,it.byteT(it.Int32Val2)))
    Assert(it.AreEqual(it.SByteVal2,it.sbyteT(it.Int32Val2)))
    Assert(it.AreEqual(it.CharVal2,it.charT(it.Int32Val2)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Val2)))
    Assert(it.AreEqual(it.Int32Val3,it.intT(it.Int32Val3)))
    Assert(it.AreEqual(it.Int16Val3,it.shortT(it.Int32Val3)))
    Assert(it.AreEqual(it.Int64Val3,it.longT(it.Int32Val3)))
    Assert(it.AreEqual(it.SByteVal3,it.sbyteT(it.Int32Val3)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Val3)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int32Val6)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int32Val6)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int32Val6)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int32Val6)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int32Val6)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int32Val6)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int32Val6)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int32Val6)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int32Val6)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int32Val6)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.Int32Val7)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.Int32Val7)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.Int32Val7)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.Int32Val7)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.Int32Val7)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.Int32Val7)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.Int32Val7)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.Int32Val7)))
    Assert(it.AreEqual(it.CharVal7,it.charT(it.Int32Val7)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Val7)))
    Assert(it.AreEqual(it.Int32Val8,it.intT(it.Int32Val8)))
    Assert(it.AreEqual(it.Int16Val8,it.shortT(it.Int32Val8)))
    Assert(it.AreEqual(it.Int64Val8,it.longT(it.Int32Val8)))
    Assert(it.AreEqual(it.SByteVal8,it.sbyteT(it.Int32Val8)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int32Val8)))
    Assert(it.AreEqual(it.UInt32Int32MaxValue,it.uintT(it.UInt32Int32MaxValue)))
    Assert(it.AreEqual(it.UInt64Int32MaxValue,it.ulongT(it.UInt32Int32MaxValue)))
    Assert(it.AreEqual(it.Int32Int32MaxValue,it.intT(it.UInt32Int32MaxValue)))
    Assert(it.AreEqual(it.Int64Int32MaxValue,it.longT(it.UInt32Int32MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32Int32MaxValue)))
    Assert(it.AreEqual(it.UInt32UInt32MaxValue,it.uintT(it.UInt32UInt32MaxValue)))
    Assert(it.AreEqual(it.UInt64UInt32MaxValue,it.ulongT(it.UInt32UInt32MaxValue)))
    Assert(it.AreEqual(it.Int64UInt32MaxValue,it.longT(it.UInt32UInt32MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32UInt32MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt32UInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Int16MaxValue,it.uintT(it.UInt32Int16MaxValue)))
    Assert(it.AreEqual(it.UInt16Int16MaxValue,it.ushortT(it.UInt32Int16MaxValue)))
    Assert(it.AreEqual(it.UInt64Int16MaxValue,it.ulongT(it.UInt32Int16MaxValue)))
    Assert(it.AreEqual(it.Int32Int16MaxValue,it.intT(it.UInt32Int16MaxValue)))
    Assert(it.AreEqual(it.Int16Int16MaxValue,it.shortT(it.UInt32Int16MaxValue)))
    Assert(it.AreEqual(it.Int64Int16MaxValue,it.longT(it.UInt32Int16MaxValue)))
    Assert(it.AreEqual(it.CharInt16MaxValue,it.charT(it.UInt32Int16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32Int16MaxValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.UInt32UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.UInt32UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.UInt32UInt16MaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.UInt32UInt16MaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.UInt32UInt16MaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.UInt32UInt16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt32UInt16MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt32UInt64MinValue)))
    Assert(it.AreEqual(it.UInt32ByteMaxValue,it.uintT(it.UInt32ByteMaxValue)))
    Assert(it.AreEqual(it.UInt16ByteMaxValue,it.ushortT(it.UInt32ByteMaxValue)))
    Assert(it.AreEqual(it.UInt64ByteMaxValue,it.ulongT(it.UInt32ByteMaxValue)))
    Assert(it.AreEqual(it.Int32ByteMaxValue,it.intT(it.UInt32ByteMaxValue)))
    Assert(it.AreEqual(it.Int16ByteMaxValue,it.shortT(it.UInt32ByteMaxValue)))
    Assert(it.AreEqual(it.Int64ByteMaxValue,it.longT(it.UInt32ByteMaxValue)))
    Assert(it.AreEqual(it.ByteByteMaxValue,it.byteT(it.UInt32ByteMaxValue)))
    Assert(it.AreEqual(it.CharByteMaxValue,it.charT(it.UInt32ByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32ByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt32ByteMinValue)))
    Assert(it.AreEqual(it.UInt32SByteMaxValue,it.uintT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.UInt16SByteMaxValue,it.ushortT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.UInt64SByteMaxValue,it.ulongT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMaxValue,it.intT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.Int16SByteMaxValue,it.shortT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.Int64SByteMaxValue,it.longT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.ByteSByteMaxValue,it.byteT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.SByteSByteMaxValue,it.sbyteT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.CharSByteMaxValue,it.charT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32SByteMaxValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.UInt32CharMaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.UInt32CharMaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.UInt32CharMaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.UInt32CharMaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.UInt32CharMaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.UInt32CharMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32CharMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt32CharMinValue)))
    Assert(it.AreEqual(it.UInt32Val0,it.uintT(it.UInt32Val0)))
    Assert(it.AreEqual(it.UInt16Val0,it.ushortT(it.UInt32Val0)))
    Assert(it.AreEqual(it.UInt64Val0,it.ulongT(it.UInt32Val0)))
    Assert(it.AreEqual(it.Int32Val0,it.intT(it.UInt32Val0)))
    Assert(it.AreEqual(it.Int16Val0,it.shortT(it.UInt32Val0)))
    Assert(it.AreEqual(it.Int64Val0,it.longT(it.UInt32Val0)))
    Assert(it.AreEqual(it.ByteVal0,it.byteT(it.UInt32Val0)))
    Assert(it.AreEqual(it.SByteVal0,it.sbyteT(it.UInt32Val0)))
    Assert(it.AreEqual(it.CharVal0,it.charT(it.UInt32Val0)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32Val0)))
    Assert(it.AreEqual(it.UInt32Val1,it.uintT(it.UInt32Val1)))
    Assert(it.AreEqual(it.UInt16Val1,it.ushortT(it.UInt32Val1)))
    Assert(it.AreEqual(it.UInt64Val1,it.ulongT(it.UInt32Val1)))
    Assert(it.AreEqual(it.Int32Val1,it.intT(it.UInt32Val1)))
    Assert(it.AreEqual(it.Int16Val1,it.shortT(it.UInt32Val1)))
    Assert(it.AreEqual(it.Int64Val1,it.longT(it.UInt32Val1)))
    Assert(it.AreEqual(it.ByteVal1,it.byteT(it.UInt32Val1)))
    Assert(it.AreEqual(it.SByteVal1,it.sbyteT(it.UInt32Val1)))
    Assert(it.AreEqual(it.CharVal1,it.charT(it.UInt32Val1)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32Val1)))
    Assert(it.AreEqual(it.UInt32Val2,it.uintT(it.UInt32Val2)))
    Assert(it.AreEqual(it.UInt16Val2,it.ushortT(it.UInt32Val2)))
    Assert(it.AreEqual(it.UInt64Val2,it.ulongT(it.UInt32Val2)))
    Assert(it.AreEqual(it.Int32Val2,it.intT(it.UInt32Val2)))
    Assert(it.AreEqual(it.Int16Val2,it.shortT(it.UInt32Val2)))
    Assert(it.AreEqual(it.Int64Val2,it.longT(it.UInt32Val2)))
    Assert(it.AreEqual(it.ByteVal2,it.byteT(it.UInt32Val2)))
    Assert(it.AreEqual(it.SByteVal2,it.sbyteT(it.UInt32Val2)))
    Assert(it.AreEqual(it.CharVal2,it.charT(it.UInt32Val2)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32Val2)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt32Val6)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt32Val6)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt32Val6)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt32Val6)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt32Val6)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt32Val6)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt32Val6)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt32Val6)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt32Val6)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt32Val6)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.UInt32Val7)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.UInt32Val7)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.UInt32Val7)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.UInt32Val7)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.UInt32Val7)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.UInt32Val7)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.UInt32Val7)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.UInt32Val7)))
    Assert(it.AreEqual(it.CharVal7,it.charT(it.UInt32Val7)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt32Val7)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int16UInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Int16MaxValue,it.uintT(it.Int16Int16MaxValue)))
    Assert(it.AreEqual(it.UInt16Int16MaxValue,it.ushortT(it.Int16Int16MaxValue)))
    Assert(it.AreEqual(it.UInt64Int16MaxValue,it.ulongT(it.Int16Int16MaxValue)))
    Assert(it.AreEqual(it.Int32Int16MaxValue,it.intT(it.Int16Int16MaxValue)))
    Assert(it.AreEqual(it.Int16Int16MaxValue,it.shortT(it.Int16Int16MaxValue)))
    Assert(it.AreEqual(it.Int64Int16MaxValue,it.longT(it.Int16Int16MaxValue)))
    Assert(it.AreEqual(it.CharInt16MaxValue,it.charT(it.Int16Int16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16Int16MaxValue)))
    Assert(it.AreEqual(it.Int32Int16MinValue,it.intT(it.Int16Int16MinValue)))
    Assert(it.AreEqual(it.Int16Int16MinValue,it.shortT(it.Int16Int16MinValue)))
    Assert(it.AreEqual(it.Int64Int16MinValue,it.longT(it.Int16Int16MinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16Int16MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int16UInt16MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int16UInt64MinValue)))
    Assert(it.AreEqual(it.UInt32ByteMaxValue,it.uintT(it.Int16ByteMaxValue)))
    Assert(it.AreEqual(it.UInt16ByteMaxValue,it.ushortT(it.Int16ByteMaxValue)))
    Assert(it.AreEqual(it.UInt64ByteMaxValue,it.ulongT(it.Int16ByteMaxValue)))
    Assert(it.AreEqual(it.Int32ByteMaxValue,it.intT(it.Int16ByteMaxValue)))
    Assert(it.AreEqual(it.Int16ByteMaxValue,it.shortT(it.Int16ByteMaxValue)))
    Assert(it.AreEqual(it.Int64ByteMaxValue,it.longT(it.Int16ByteMaxValue)))
    Assert(it.AreEqual(it.ByteByteMaxValue,it.byteT(it.Int16ByteMaxValue)))
    Assert(it.AreEqual(it.CharByteMaxValue,it.charT(it.Int16ByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16ByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int16ByteMinValue)))
    Assert(it.AreEqual(it.UInt32SByteMaxValue,it.uintT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.UInt16SByteMaxValue,it.ushortT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.UInt64SByteMaxValue,it.ulongT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMaxValue,it.intT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.Int16SByteMaxValue,it.shortT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.Int64SByteMaxValue,it.longT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.ByteSByteMaxValue,it.byteT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.SByteSByteMaxValue,it.sbyteT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.CharSByteMaxValue,it.charT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16SByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMinValue,it.intT(it.Int16SByteMinValue)))
    Assert(it.AreEqual(it.Int16SByteMinValue,it.shortT(it.Int16SByteMinValue)))
    Assert(it.AreEqual(it.Int64SByteMinValue,it.longT(it.Int16SByteMinValue)))
    Assert(it.AreEqual(it.SByteSByteMinValue,it.sbyteT(it.Int16SByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16SByteMinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int16CharMinValue)))
    Assert(it.AreEqual(it.UInt32Val0,it.uintT(it.Int16Val0)))
    Assert(it.AreEqual(it.UInt16Val0,it.ushortT(it.Int16Val0)))
    Assert(it.AreEqual(it.UInt64Val0,it.ulongT(it.Int16Val0)))
    Assert(it.AreEqual(it.Int32Val0,it.intT(it.Int16Val0)))
    Assert(it.AreEqual(it.Int16Val0,it.shortT(it.Int16Val0)))
    Assert(it.AreEqual(it.Int64Val0,it.longT(it.Int16Val0)))
    Assert(it.AreEqual(it.ByteVal0,it.byteT(it.Int16Val0)))
    Assert(it.AreEqual(it.SByteVal0,it.sbyteT(it.Int16Val0)))
    Assert(it.AreEqual(it.CharVal0,it.charT(it.Int16Val0)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16Val0)))
    Assert(it.AreEqual(it.UInt32Val1,it.uintT(it.Int16Val1)))
    Assert(it.AreEqual(it.UInt16Val1,it.ushortT(it.Int16Val1)))
    Assert(it.AreEqual(it.UInt64Val1,it.ulongT(it.Int16Val1)))
    Assert(it.AreEqual(it.Int32Val1,it.intT(it.Int16Val1)))
    Assert(it.AreEqual(it.Int16Val1,it.shortT(it.Int16Val1)))
    Assert(it.AreEqual(it.Int64Val1,it.longT(it.Int16Val1)))
    Assert(it.AreEqual(it.ByteVal1,it.byteT(it.Int16Val1)))
    Assert(it.AreEqual(it.SByteVal1,it.sbyteT(it.Int16Val1)))
    Assert(it.AreEqual(it.CharVal1,it.charT(it.Int16Val1)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16Val1)))
    Assert(it.AreEqual(it.UInt32Val2,it.uintT(it.Int16Val2)))
    Assert(it.AreEqual(it.UInt16Val2,it.ushortT(it.Int16Val2)))
    Assert(it.AreEqual(it.UInt64Val2,it.ulongT(it.Int16Val2)))
    Assert(it.AreEqual(it.Int32Val2,it.intT(it.Int16Val2)))
    Assert(it.AreEqual(it.Int16Val2,it.shortT(it.Int16Val2)))
    Assert(it.AreEqual(it.Int64Val2,it.longT(it.Int16Val2)))
    Assert(it.AreEqual(it.ByteVal2,it.byteT(it.Int16Val2)))
    Assert(it.AreEqual(it.SByteVal2,it.sbyteT(it.Int16Val2)))
    Assert(it.AreEqual(it.CharVal2,it.charT(it.Int16Val2)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16Val2)))
    Assert(it.AreEqual(it.Int32Val3,it.intT(it.Int16Val3)))
    Assert(it.AreEqual(it.Int16Val3,it.shortT(it.Int16Val3)))
    Assert(it.AreEqual(it.Int64Val3,it.longT(it.Int16Val3)))
    Assert(it.AreEqual(it.SByteVal3,it.sbyteT(it.Int16Val3)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16Val3)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int16Val6)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int16Val6)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int16Val6)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int16Val6)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int16Val6)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int16Val6)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int16Val6)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int16Val6)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int16Val6)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int16Val6)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.Int16Val7)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.Int16Val7)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.Int16Val7)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.Int16Val7)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.Int16Val7)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.Int16Val7)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.Int16Val7)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.Int16Val7)))
    Assert(it.AreEqual(it.CharVal7,it.charT(it.Int16Val7)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16Val7)))
    Assert(it.AreEqual(it.Int32Val8,it.intT(it.Int16Val8)))
    Assert(it.AreEqual(it.Int16Val8,it.shortT(it.Int16Val8)))
    Assert(it.AreEqual(it.Int64Val8,it.longT(it.Int16Val8)))
    Assert(it.AreEqual(it.SByteVal8,it.sbyteT(it.Int16Val8)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int16Val8)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt16UInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Int16MaxValue,it.uintT(it.UInt16Int16MaxValue)))
    Assert(it.AreEqual(it.UInt16Int16MaxValue,it.ushortT(it.UInt16Int16MaxValue)))
    Assert(it.AreEqual(it.UInt64Int16MaxValue,it.ulongT(it.UInt16Int16MaxValue)))
    Assert(it.AreEqual(it.Int32Int16MaxValue,it.intT(it.UInt16Int16MaxValue)))
    Assert(it.AreEqual(it.Int16Int16MaxValue,it.shortT(it.UInt16Int16MaxValue)))
    Assert(it.AreEqual(it.Int64Int16MaxValue,it.longT(it.UInt16Int16MaxValue)))
    Assert(it.AreEqual(it.CharInt16MaxValue,it.charT(it.UInt16Int16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt16Int16MaxValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.UInt16UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.UInt16UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.UInt16UInt16MaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.UInt16UInt16MaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.UInt16UInt16MaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.UInt16UInt16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt16UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt16UInt16MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt16UInt64MinValue)))
    Assert(it.AreEqual(it.UInt32ByteMaxValue,it.uintT(it.UInt16ByteMaxValue)))
    Assert(it.AreEqual(it.UInt16ByteMaxValue,it.ushortT(it.UInt16ByteMaxValue)))
    Assert(it.AreEqual(it.UInt64ByteMaxValue,it.ulongT(it.UInt16ByteMaxValue)))
    Assert(it.AreEqual(it.Int32ByteMaxValue,it.intT(it.UInt16ByteMaxValue)))
    Assert(it.AreEqual(it.Int16ByteMaxValue,it.shortT(it.UInt16ByteMaxValue)))
    Assert(it.AreEqual(it.Int64ByteMaxValue,it.longT(it.UInt16ByteMaxValue)))
    Assert(it.AreEqual(it.ByteByteMaxValue,it.byteT(it.UInt16ByteMaxValue)))
    Assert(it.AreEqual(it.CharByteMaxValue,it.charT(it.UInt16ByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt16ByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt16ByteMinValue)))
    Assert(it.AreEqual(it.UInt32SByteMaxValue,it.uintT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.UInt16SByteMaxValue,it.ushortT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.UInt64SByteMaxValue,it.ulongT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMaxValue,it.intT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.Int16SByteMaxValue,it.shortT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.Int64SByteMaxValue,it.longT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.ByteSByteMaxValue,it.byteT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.SByteSByteMaxValue,it.sbyteT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.CharSByteMaxValue,it.charT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt16SByteMaxValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.UInt16CharMaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.UInt16CharMaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.UInt16CharMaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.UInt16CharMaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.UInt16CharMaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.UInt16CharMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt16CharMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt16CharMinValue)))
    Assert(it.AreEqual(it.UInt32Val0,it.uintT(it.UInt16Val0)))
    Assert(it.AreEqual(it.UInt16Val0,it.ushortT(it.UInt16Val0)))
    Assert(it.AreEqual(it.UInt64Val0,it.ulongT(it.UInt16Val0)))
    Assert(it.AreEqual(it.Int32Val0,it.intT(it.UInt16Val0)))
    Assert(it.AreEqual(it.Int16Val0,it.shortT(it.UInt16Val0)))
    Assert(it.AreEqual(it.Int64Val0,it.longT(it.UInt16Val0)))
    Assert(it.AreEqual(it.ByteVal0,it.byteT(it.UInt16Val0)))
    Assert(it.AreEqual(it.SByteVal0,it.sbyteT(it.UInt16Val0)))
    Assert(it.AreEqual(it.CharVal0,it.charT(it.UInt16Val0)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt16Val0)))
    Assert(it.AreEqual(it.UInt32Val1,it.uintT(it.UInt16Val1)))
    Assert(it.AreEqual(it.UInt16Val1,it.ushortT(it.UInt16Val1)))
    Assert(it.AreEqual(it.UInt64Val1,it.ulongT(it.UInt16Val1)))
    Assert(it.AreEqual(it.Int32Val1,it.intT(it.UInt16Val1)))
    Assert(it.AreEqual(it.Int16Val1,it.shortT(it.UInt16Val1)))
    Assert(it.AreEqual(it.Int64Val1,it.longT(it.UInt16Val1)))
    Assert(it.AreEqual(it.ByteVal1,it.byteT(it.UInt16Val1)))
    Assert(it.AreEqual(it.SByteVal1,it.sbyteT(it.UInt16Val1)))
    Assert(it.AreEqual(it.CharVal1,it.charT(it.UInt16Val1)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt16Val1)))
    Assert(it.AreEqual(it.UInt32Val2,it.uintT(it.UInt16Val2)))
    Assert(it.AreEqual(it.UInt16Val2,it.ushortT(it.UInt16Val2)))
    Assert(it.AreEqual(it.UInt64Val2,it.ulongT(it.UInt16Val2)))
    Assert(it.AreEqual(it.Int32Val2,it.intT(it.UInt16Val2)))
    Assert(it.AreEqual(it.Int16Val2,it.shortT(it.UInt16Val2)))
    Assert(it.AreEqual(it.Int64Val2,it.longT(it.UInt16Val2)))
    Assert(it.AreEqual(it.ByteVal2,it.byteT(it.UInt16Val2)))
    Assert(it.AreEqual(it.SByteVal2,it.sbyteT(it.UInt16Val2)))
    Assert(it.AreEqual(it.CharVal2,it.charT(it.UInt16Val2)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt16Val2)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt16Val6)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt16Val6)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt16Val6)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt16Val6)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt16Val6)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt16Val6)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt16Val6)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt16Val6)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt16Val6)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt16Val6)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.UInt16Val7)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.UInt16Val7)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.UInt16Val7)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.UInt16Val7)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.UInt16Val7)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.UInt16Val7)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.UInt16Val7)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.UInt16Val7)))
    Assert(it.AreEqual(it.CharVal7,it.charT(it.UInt16Val7)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt16Val7)))
    Assert(it.AreEqual(it.UInt32Int32MaxValue,it.uintT(it.Int64Int32MaxValue)))
    Assert(it.AreEqual(it.UInt64Int32MaxValue,it.ulongT(it.Int64Int32MaxValue)))
    Assert(it.AreEqual(it.Int32Int32MaxValue,it.intT(it.Int64Int32MaxValue)))
    Assert(it.AreEqual(it.Int64Int32MaxValue,it.longT(it.Int64Int32MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Int32MaxValue)))
    Assert(it.AreEqual(it.Int32Int32MinValue,it.intT(it.Int64Int32MinValue)))
    Assert(it.AreEqual(it.Int64Int32MinValue,it.longT(it.Int64Int32MinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Int32MinValue)))
    Assert(it.AreEqual(it.UInt32UInt32MaxValue,it.uintT(it.Int64UInt32MaxValue)))
    Assert(it.AreEqual(it.UInt64UInt32MaxValue,it.ulongT(it.Int64UInt32MaxValue)))
    Assert(it.AreEqual(it.Int64UInt32MaxValue,it.longT(it.Int64UInt32MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64UInt32MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int64UInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Int16MaxValue,it.uintT(it.Int64Int16MaxValue)))
    Assert(it.AreEqual(it.UInt16Int16MaxValue,it.ushortT(it.Int64Int16MaxValue)))
    Assert(it.AreEqual(it.UInt64Int16MaxValue,it.ulongT(it.Int64Int16MaxValue)))
    Assert(it.AreEqual(it.Int32Int16MaxValue,it.intT(it.Int64Int16MaxValue)))
    Assert(it.AreEqual(it.Int16Int16MaxValue,it.shortT(it.Int64Int16MaxValue)))
    Assert(it.AreEqual(it.Int64Int16MaxValue,it.longT(it.Int64Int16MaxValue)))
    Assert(it.AreEqual(it.CharInt16MaxValue,it.charT(it.Int64Int16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Int16MaxValue)))
    Assert(it.AreEqual(it.Int32Int16MinValue,it.intT(it.Int64Int16MinValue)))
    Assert(it.AreEqual(it.Int16Int16MinValue,it.shortT(it.Int64Int16MinValue)))
    Assert(it.AreEqual(it.Int64Int16MinValue,it.longT(it.Int64Int16MinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Int16MinValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.Int64UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.Int64UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.Int64UInt16MaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.Int64UInt16MaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.Int64UInt16MaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.Int64UInt16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int64UInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Int64MaxValue,it.ulongT(it.Int64Int64MaxValue)))
    Assert(it.AreEqual(it.Int64Int64MaxValue,it.longT(it.Int64Int64MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Int64MaxValue)))
    Assert(it.AreEqual(it.Int64Int64MinValue,it.longT(it.Int64Int64MinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Int64MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int64UInt64MinValue)))
    Assert(it.AreEqual(it.UInt32ByteMaxValue,it.uintT(it.Int64ByteMaxValue)))
    Assert(it.AreEqual(it.UInt16ByteMaxValue,it.ushortT(it.Int64ByteMaxValue)))
    Assert(it.AreEqual(it.UInt64ByteMaxValue,it.ulongT(it.Int64ByteMaxValue)))
    Assert(it.AreEqual(it.Int32ByteMaxValue,it.intT(it.Int64ByteMaxValue)))
    Assert(it.AreEqual(it.Int16ByteMaxValue,it.shortT(it.Int64ByteMaxValue)))
    Assert(it.AreEqual(it.Int64ByteMaxValue,it.longT(it.Int64ByteMaxValue)))
    Assert(it.AreEqual(it.ByteByteMaxValue,it.byteT(it.Int64ByteMaxValue)))
    Assert(it.AreEqual(it.CharByteMaxValue,it.charT(it.Int64ByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64ByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int64ByteMinValue)))
    Assert(it.AreEqual(it.UInt32SByteMaxValue,it.uintT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.UInt16SByteMaxValue,it.ushortT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.UInt64SByteMaxValue,it.ulongT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMaxValue,it.intT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.Int16SByteMaxValue,it.shortT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.Int64SByteMaxValue,it.longT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.ByteSByteMaxValue,it.byteT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.SByteSByteMaxValue,it.sbyteT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.CharSByteMaxValue,it.charT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64SByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMinValue,it.intT(it.Int64SByteMinValue)))
    Assert(it.AreEqual(it.Int16SByteMinValue,it.shortT(it.Int64SByteMinValue)))
    Assert(it.AreEqual(it.Int64SByteMinValue,it.longT(it.Int64SByteMinValue)))
    Assert(it.AreEqual(it.SByteSByteMinValue,it.sbyteT(it.Int64SByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64SByteMinValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.Int64CharMaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.Int64CharMaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.Int64CharMaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.Int64CharMaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.Int64CharMaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.Int64CharMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64CharMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int64CharMinValue)))
    Assert(it.AreEqual(it.UInt32Val0,it.uintT(it.Int64Val0)))
    Assert(it.AreEqual(it.UInt16Val0,it.ushortT(it.Int64Val0)))
    Assert(it.AreEqual(it.UInt64Val0,it.ulongT(it.Int64Val0)))
    Assert(it.AreEqual(it.Int32Val0,it.intT(it.Int64Val0)))
    Assert(it.AreEqual(it.Int16Val0,it.shortT(it.Int64Val0)))
    Assert(it.AreEqual(it.Int64Val0,it.longT(it.Int64Val0)))
    Assert(it.AreEqual(it.ByteVal0,it.byteT(it.Int64Val0)))
    Assert(it.AreEqual(it.SByteVal0,it.sbyteT(it.Int64Val0)))
    Assert(it.AreEqual(it.CharVal0,it.charT(it.Int64Val0)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Val0)))
    Assert(it.AreEqual(it.UInt32Val1,it.uintT(it.Int64Val1)))
    Assert(it.AreEqual(it.UInt16Val1,it.ushortT(it.Int64Val1)))
    Assert(it.AreEqual(it.UInt64Val1,it.ulongT(it.Int64Val1)))
    Assert(it.AreEqual(it.Int32Val1,it.intT(it.Int64Val1)))
    Assert(it.AreEqual(it.Int16Val1,it.shortT(it.Int64Val1)))
    Assert(it.AreEqual(it.Int64Val1,it.longT(it.Int64Val1)))
    Assert(it.AreEqual(it.ByteVal1,it.byteT(it.Int64Val1)))
    Assert(it.AreEqual(it.SByteVal1,it.sbyteT(it.Int64Val1)))
    Assert(it.AreEqual(it.CharVal1,it.charT(it.Int64Val1)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Val1)))
    Assert(it.AreEqual(it.UInt32Val2,it.uintT(it.Int64Val2)))
    Assert(it.AreEqual(it.UInt16Val2,it.ushortT(it.Int64Val2)))
    Assert(it.AreEqual(it.UInt64Val2,it.ulongT(it.Int64Val2)))
    Assert(it.AreEqual(it.Int32Val2,it.intT(it.Int64Val2)))
    Assert(it.AreEqual(it.Int16Val2,it.shortT(it.Int64Val2)))
    Assert(it.AreEqual(it.Int64Val2,it.longT(it.Int64Val2)))
    Assert(it.AreEqual(it.ByteVal2,it.byteT(it.Int64Val2)))
    Assert(it.AreEqual(it.SByteVal2,it.sbyteT(it.Int64Val2)))
    Assert(it.AreEqual(it.CharVal2,it.charT(it.Int64Val2)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Val2)))
    Assert(it.AreEqual(it.Int32Val3,it.intT(it.Int64Val3)))
    Assert(it.AreEqual(it.Int16Val3,it.shortT(it.Int64Val3)))
    Assert(it.AreEqual(it.Int64Val3,it.longT(it.Int64Val3)))
    Assert(it.AreEqual(it.SByteVal3,it.sbyteT(it.Int64Val3)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Val3)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.Int64Val6)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.Int64Val6)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.Int64Val6)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.Int64Val6)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.Int64Val6)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.Int64Val6)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.Int64Val6)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.Int64Val6)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.Int64Val6)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.Int64Val6)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.Int64Val7)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.Int64Val7)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.Int64Val7)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.Int64Val7)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.Int64Val7)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.Int64Val7)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.Int64Val7)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.Int64Val7)))
    Assert(it.AreEqual(it.CharVal7,it.charT(it.Int64Val7)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Val7)))
    Assert(it.AreEqual(it.Int32Val8,it.intT(it.Int64Val8)))
    Assert(it.AreEqual(it.Int16Val8,it.shortT(it.Int64Val8)))
    Assert(it.AreEqual(it.Int64Val8,it.longT(it.Int64Val8)))
    Assert(it.AreEqual(it.SByteVal8,it.sbyteT(it.Int64Val8)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.Int64Val8)))
    Assert(it.AreEqual(it.UInt32Int32MaxValue,it.uintT(it.UInt64Int32MaxValue)))
    Assert(it.AreEqual(it.UInt64Int32MaxValue,it.ulongT(it.UInt64Int32MaxValue)))
    Assert(it.AreEqual(it.Int32Int32MaxValue,it.intT(it.UInt64Int32MaxValue)))
    Assert(it.AreEqual(it.Int64Int32MaxValue,it.longT(it.UInt64Int32MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64Int32MaxValue)))
    Assert(it.AreEqual(it.UInt32UInt32MaxValue,it.uintT(it.UInt64UInt32MaxValue)))
    Assert(it.AreEqual(it.UInt64UInt32MaxValue,it.ulongT(it.UInt64UInt32MaxValue)))
    Assert(it.AreEqual(it.Int64UInt32MaxValue,it.longT(it.UInt64UInt32MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64UInt32MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt64UInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Int16MaxValue,it.uintT(it.UInt64Int16MaxValue)))
    Assert(it.AreEqual(it.UInt16Int16MaxValue,it.ushortT(it.UInt64Int16MaxValue)))
    Assert(it.AreEqual(it.UInt64Int16MaxValue,it.ulongT(it.UInt64Int16MaxValue)))
    Assert(it.AreEqual(it.Int32Int16MaxValue,it.intT(it.UInt64Int16MaxValue)))
    Assert(it.AreEqual(it.Int16Int16MaxValue,it.shortT(it.UInt64Int16MaxValue)))
    Assert(it.AreEqual(it.Int64Int16MaxValue,it.longT(it.UInt64Int16MaxValue)))
    Assert(it.AreEqual(it.CharInt16MaxValue,it.charT(it.UInt64Int16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64Int16MaxValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.UInt64UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.UInt64UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.UInt64UInt16MaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.UInt64UInt16MaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.UInt64UInt16MaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.UInt64UInt16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64UInt16MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt64UInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Int64MaxValue,it.ulongT(it.UInt64Int64MaxValue)))
    Assert(it.AreEqual(it.Int64Int64MaxValue,it.longT(it.UInt64Int64MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64Int64MaxValue)))
    Assert(it.AreEqual(it.UInt64UInt64MaxValue,it.ulongT(it.UInt64UInt64MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64UInt64MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt64UInt64MinValue)))
    Assert(it.AreEqual(it.UInt32ByteMaxValue,it.uintT(it.UInt64ByteMaxValue)))
    Assert(it.AreEqual(it.UInt16ByteMaxValue,it.ushortT(it.UInt64ByteMaxValue)))
    Assert(it.AreEqual(it.UInt64ByteMaxValue,it.ulongT(it.UInt64ByteMaxValue)))
    Assert(it.AreEqual(it.Int32ByteMaxValue,it.intT(it.UInt64ByteMaxValue)))
    Assert(it.AreEqual(it.Int16ByteMaxValue,it.shortT(it.UInt64ByteMaxValue)))
    Assert(it.AreEqual(it.Int64ByteMaxValue,it.longT(it.UInt64ByteMaxValue)))
    Assert(it.AreEqual(it.ByteByteMaxValue,it.byteT(it.UInt64ByteMaxValue)))
    Assert(it.AreEqual(it.CharByteMaxValue,it.charT(it.UInt64ByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64ByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt64ByteMinValue)))
    Assert(it.AreEqual(it.UInt32SByteMaxValue,it.uintT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.UInt16SByteMaxValue,it.ushortT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.UInt64SByteMaxValue,it.ulongT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMaxValue,it.intT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.Int16SByteMaxValue,it.shortT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.Int64SByteMaxValue,it.longT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.ByteSByteMaxValue,it.byteT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.SByteSByteMaxValue,it.sbyteT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.CharSByteMaxValue,it.charT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64SByteMaxValue)))
    Assert(it.AreEqual(it.UInt32CharMaxValue,it.uintT(it.UInt64CharMaxValue)))
    Assert(it.AreEqual(it.UInt16CharMaxValue,it.ushortT(it.UInt64CharMaxValue)))
    Assert(it.AreEqual(it.UInt64CharMaxValue,it.ulongT(it.UInt64CharMaxValue)))
    Assert(it.AreEqual(it.Int32CharMaxValue,it.intT(it.UInt64CharMaxValue)))
    Assert(it.AreEqual(it.Int64CharMaxValue,it.longT(it.UInt64CharMaxValue)))
    Assert(it.AreEqual(it.CharCharMaxValue,it.charT(it.UInt64CharMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64CharMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt64CharMinValue)))
    Assert(it.AreEqual(it.UInt32Val0,it.uintT(it.UInt64Val0)))
    Assert(it.AreEqual(it.UInt16Val0,it.ushortT(it.UInt64Val0)))
    Assert(it.AreEqual(it.UInt64Val0,it.ulongT(it.UInt64Val0)))
    Assert(it.AreEqual(it.Int32Val0,it.intT(it.UInt64Val0)))
    Assert(it.AreEqual(it.Int16Val0,it.shortT(it.UInt64Val0)))
    Assert(it.AreEqual(it.Int64Val0,it.longT(it.UInt64Val0)))
    Assert(it.AreEqual(it.ByteVal0,it.byteT(it.UInt64Val0)))
    Assert(it.AreEqual(it.SByteVal0,it.sbyteT(it.UInt64Val0)))
    Assert(it.AreEqual(it.CharVal0,it.charT(it.UInt64Val0)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64Val0)))
    Assert(it.AreEqual(it.UInt32Val1,it.uintT(it.UInt64Val1)))
    Assert(it.AreEqual(it.UInt16Val1,it.ushortT(it.UInt64Val1)))
    Assert(it.AreEqual(it.UInt64Val1,it.ulongT(it.UInt64Val1)))
    Assert(it.AreEqual(it.Int32Val1,it.intT(it.UInt64Val1)))
    Assert(it.AreEqual(it.Int16Val1,it.shortT(it.UInt64Val1)))
    Assert(it.AreEqual(it.Int64Val1,it.longT(it.UInt64Val1)))
    Assert(it.AreEqual(it.ByteVal1,it.byteT(it.UInt64Val1)))
    Assert(it.AreEqual(it.SByteVal1,it.sbyteT(it.UInt64Val1)))
    Assert(it.AreEqual(it.CharVal1,it.charT(it.UInt64Val1)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64Val1)))
    Assert(it.AreEqual(it.UInt32Val2,it.uintT(it.UInt64Val2)))
    Assert(it.AreEqual(it.UInt16Val2,it.ushortT(it.UInt64Val2)))
    Assert(it.AreEqual(it.UInt64Val2,it.ulongT(it.UInt64Val2)))
    Assert(it.AreEqual(it.Int32Val2,it.intT(it.UInt64Val2)))
    Assert(it.AreEqual(it.Int16Val2,it.shortT(it.UInt64Val2)))
    Assert(it.AreEqual(it.Int64Val2,it.longT(it.UInt64Val2)))
    Assert(it.AreEqual(it.ByteVal2,it.byteT(it.UInt64Val2)))
    Assert(it.AreEqual(it.SByteVal2,it.sbyteT(it.UInt64Val2)))
    Assert(it.AreEqual(it.CharVal2,it.charT(it.UInt64Val2)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64Val2)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.UInt64Val6)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.UInt64Val6)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.UInt64Val6)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.UInt64Val6)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.UInt64Val6)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.UInt64Val6)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.UInt64Val6)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.UInt64Val6)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.UInt64Val6)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.UInt64Val6)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.UInt64Val7)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.UInt64Val7)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.UInt64Val7)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.UInt64Val7)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.UInt64Val7)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.UInt64Val7)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.UInt64Val7)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.UInt64Val7)))
    Assert(it.AreEqual(it.CharVal7,it.charT(it.UInt64Val7)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.UInt64Val7)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.ByteUInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.ByteUInt16MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.ByteUInt64MinValue)))
    Assert(it.AreEqual(it.UInt32ByteMaxValue,it.uintT(it.ByteByteMaxValue)))
    Assert(it.AreEqual(it.UInt16ByteMaxValue,it.ushortT(it.ByteByteMaxValue)))
    Assert(it.AreEqual(it.UInt64ByteMaxValue,it.ulongT(it.ByteByteMaxValue)))
    Assert(it.AreEqual(it.Int32ByteMaxValue,it.intT(it.ByteByteMaxValue)))
    Assert(it.AreEqual(it.Int16ByteMaxValue,it.shortT(it.ByteByteMaxValue)))
    Assert(it.AreEqual(it.Int64ByteMaxValue,it.longT(it.ByteByteMaxValue)))
    Assert(it.AreEqual(it.ByteByteMaxValue,it.byteT(it.ByteByteMaxValue)))
    Assert(it.AreEqual(it.CharByteMaxValue,it.charT(it.ByteByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.ByteByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.ByteByteMinValue)))
    Assert(it.AreEqual(it.UInt32SByteMaxValue,it.uintT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.UInt16SByteMaxValue,it.ushortT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.UInt64SByteMaxValue,it.ulongT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMaxValue,it.intT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.Int16SByteMaxValue,it.shortT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.Int64SByteMaxValue,it.longT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.ByteSByteMaxValue,it.byteT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.SByteSByteMaxValue,it.sbyteT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.CharSByteMaxValue,it.charT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.ByteSByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.ByteCharMinValue)))
    Assert(it.AreEqual(it.UInt32Val0,it.uintT(it.ByteVal0)))
    Assert(it.AreEqual(it.UInt16Val0,it.ushortT(it.ByteVal0)))
    Assert(it.AreEqual(it.UInt64Val0,it.ulongT(it.ByteVal0)))
    Assert(it.AreEqual(it.Int32Val0,it.intT(it.ByteVal0)))
    Assert(it.AreEqual(it.Int16Val0,it.shortT(it.ByteVal0)))
    Assert(it.AreEqual(it.Int64Val0,it.longT(it.ByteVal0)))
    Assert(it.AreEqual(it.ByteVal0,it.byteT(it.ByteVal0)))
    Assert(it.AreEqual(it.SByteVal0,it.sbyteT(it.ByteVal0)))
    Assert(it.AreEqual(it.CharVal0,it.charT(it.ByteVal0)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.ByteVal0)))
    Assert(it.AreEqual(it.UInt32Val1,it.uintT(it.ByteVal1)))
    Assert(it.AreEqual(it.UInt16Val1,it.ushortT(it.ByteVal1)))
    Assert(it.AreEqual(it.UInt64Val1,it.ulongT(it.ByteVal1)))
    Assert(it.AreEqual(it.Int32Val1,it.intT(it.ByteVal1)))
    Assert(it.AreEqual(it.Int16Val1,it.shortT(it.ByteVal1)))
    Assert(it.AreEqual(it.Int64Val1,it.longT(it.ByteVal1)))
    Assert(it.AreEqual(it.ByteVal1,it.byteT(it.ByteVal1)))
    Assert(it.AreEqual(it.SByteVal1,it.sbyteT(it.ByteVal1)))
    Assert(it.AreEqual(it.CharVal1,it.charT(it.ByteVal1)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.ByteVal1)))
    Assert(it.AreEqual(it.UInt32Val2,it.uintT(it.ByteVal2)))
    Assert(it.AreEqual(it.UInt16Val2,it.ushortT(it.ByteVal2)))
    Assert(it.AreEqual(it.UInt64Val2,it.ulongT(it.ByteVal2)))
    Assert(it.AreEqual(it.Int32Val2,it.intT(it.ByteVal2)))
    Assert(it.AreEqual(it.Int16Val2,it.shortT(it.ByteVal2)))
    Assert(it.AreEqual(it.Int64Val2,it.longT(it.ByteVal2)))
    Assert(it.AreEqual(it.ByteVal2,it.byteT(it.ByteVal2)))
    Assert(it.AreEqual(it.SByteVal2,it.sbyteT(it.ByteVal2)))
    Assert(it.AreEqual(it.CharVal2,it.charT(it.ByteVal2)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.ByteVal2)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.ByteVal6)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.ByteVal6)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.ByteVal6)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.ByteVal6)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.ByteVal6)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.ByteVal6)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.ByteVal6)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.ByteVal6)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.ByteVal6)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.ByteVal6)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.ByteVal7)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.ByteVal7)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.ByteVal7)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.ByteVal7)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.ByteVal7)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.ByteVal7)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.ByteVal7)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.ByteVal7)))
    Assert(it.AreEqual(it.CharVal7,it.charT(it.ByteVal7)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.ByteVal7)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.SByteUInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.SByteUInt16MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.SByteUInt64MinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.SByteByteMinValue)))
    Assert(it.AreEqual(it.UInt32SByteMaxValue,it.uintT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.UInt16SByteMaxValue,it.ushortT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.UInt64SByteMaxValue,it.ulongT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMaxValue,it.intT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.Int16SByteMaxValue,it.shortT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.Int64SByteMaxValue,it.longT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.ByteSByteMaxValue,it.byteT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.SByteSByteMaxValue,it.sbyteT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.CharSByteMaxValue,it.charT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.SByteSByteMaxValue)))
    Assert(it.AreEqual(it.Int32SByteMinValue,it.intT(it.SByteSByteMinValue)))
    Assert(it.AreEqual(it.Int16SByteMinValue,it.shortT(it.SByteSByteMinValue)))
    Assert(it.AreEqual(it.Int64SByteMinValue,it.longT(it.SByteSByteMinValue)))
    Assert(it.AreEqual(it.SByteSByteMinValue,it.sbyteT(it.SByteSByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.SByteSByteMinValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.SByteCharMinValue)))
    Assert(it.AreEqual(it.UInt32Val0,it.uintT(it.SByteVal0)))
    Assert(it.AreEqual(it.UInt16Val0,it.ushortT(it.SByteVal0)))
    Assert(it.AreEqual(it.UInt64Val0,it.ulongT(it.SByteVal0)))
    Assert(it.AreEqual(it.Int32Val0,it.intT(it.SByteVal0)))
    Assert(it.AreEqual(it.Int16Val0,it.shortT(it.SByteVal0)))
    Assert(it.AreEqual(it.Int64Val0,it.longT(it.SByteVal0)))
    Assert(it.AreEqual(it.ByteVal0,it.byteT(it.SByteVal0)))
    Assert(it.AreEqual(it.SByteVal0,it.sbyteT(it.SByteVal0)))
    Assert(it.AreEqual(it.CharVal0,it.charT(it.SByteVal0)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.SByteVal0)))
    Assert(it.AreEqual(it.UInt32Val1,it.uintT(it.SByteVal1)))
    Assert(it.AreEqual(it.UInt16Val1,it.ushortT(it.SByteVal1)))
    Assert(it.AreEqual(it.UInt64Val1,it.ulongT(it.SByteVal1)))
    Assert(it.AreEqual(it.Int32Val1,it.intT(it.SByteVal1)))
    Assert(it.AreEqual(it.Int16Val1,it.shortT(it.SByteVal1)))
    Assert(it.AreEqual(it.Int64Val1,it.longT(it.SByteVal1)))
    Assert(it.AreEqual(it.ByteVal1,it.byteT(it.SByteVal1)))
    Assert(it.AreEqual(it.SByteVal1,it.sbyteT(it.SByteVal1)))
    Assert(it.AreEqual(it.CharVal1,it.charT(it.SByteVal1)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.SByteVal1)))
    Assert(it.AreEqual(it.UInt32Val2,it.uintT(it.SByteVal2)))
    Assert(it.AreEqual(it.UInt16Val2,it.ushortT(it.SByteVal2)))
    Assert(it.AreEqual(it.UInt64Val2,it.ulongT(it.SByteVal2)))
    Assert(it.AreEqual(it.Int32Val2,it.intT(it.SByteVal2)))
    Assert(it.AreEqual(it.Int16Val2,it.shortT(it.SByteVal2)))
    Assert(it.AreEqual(it.Int64Val2,it.longT(it.SByteVal2)))
    Assert(it.AreEqual(it.ByteVal2,it.byteT(it.SByteVal2)))
    Assert(it.AreEqual(it.SByteVal2,it.sbyteT(it.SByteVal2)))
    Assert(it.AreEqual(it.CharVal2,it.charT(it.SByteVal2)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.SByteVal2)))
    Assert(it.AreEqual(it.Int32Val3,it.intT(it.SByteVal3)))
    Assert(it.AreEqual(it.Int16Val3,it.shortT(it.SByteVal3)))
    Assert(it.AreEqual(it.Int64Val3,it.longT(it.SByteVal3)))
    Assert(it.AreEqual(it.SByteVal3,it.sbyteT(it.SByteVal3)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.SByteVal3)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.SByteVal6)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.SByteVal6)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.SByteVal6)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.SByteVal6)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.SByteVal6)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.SByteVal6)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.SByteVal6)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.SByteVal6)))
    Assert(it.AreEqual(it.CharVal6,it.charT(it.SByteVal6)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.SByteVal6)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.SByteVal7)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.SByteVal7)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.SByteVal7)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.SByteVal7)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.SByteVal7)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.SByteVal7)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.SByteVal7)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.SByteVal7)))
    Assert(it.AreEqual(it.CharVal7,it.charT(it.SByteVal7)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.SByteVal7)))
    Assert(it.AreEqual(it.Int32Val8,it.intT(it.SByteVal8)))
    Assert(it.AreEqual(it.Int16Val8,it.shortT(it.SByteVal8)))
    Assert(it.AreEqual(it.Int64Val8,it.longT(it.SByteVal8)))
    Assert(it.AreEqual(it.SByteVal8,it.sbyteT(it.SByteVal8)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.SByteVal8)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanInt32MaxValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanInt32MaxValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanInt32MaxValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanInt32MaxValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanInt32MaxValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanInt32MaxValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanInt32MaxValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanInt32MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanInt32MaxValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanUInt32MaxValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanUInt32MaxValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanUInt32MaxValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanUInt32MaxValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanUInt32MaxValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanUInt32MaxValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanUInt32MaxValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanUInt32MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanUInt32MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.BooleanUInt32MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.BooleanUInt32MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.BooleanUInt32MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.BooleanUInt32MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.BooleanUInt32MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.BooleanUInt32MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.BooleanUInt32MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.BooleanUInt32MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.BooleanUInt32MinValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanInt16MaxValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanInt16MaxValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanInt16MaxValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanInt16MaxValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanInt16MaxValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanInt16MaxValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanInt16MaxValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanInt16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanInt16MaxValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanInt16MinValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanUInt16MaxValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanUInt16MaxValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanUInt16MaxValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanUInt16MaxValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanUInt16MaxValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanUInt16MaxValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanUInt16MaxValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanUInt16MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanUInt16MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.BooleanUInt16MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.BooleanUInt16MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.BooleanUInt16MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.BooleanUInt16MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.BooleanUInt16MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.BooleanUInt16MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.BooleanUInt16MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.BooleanUInt16MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.BooleanUInt16MinValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanInt64MaxValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanInt64MaxValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanInt64MaxValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanInt64MaxValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanInt64MaxValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanInt64MaxValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanInt64MaxValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanInt64MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanInt64MaxValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanInt64MinValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanUInt64MaxValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanUInt64MaxValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanUInt64MaxValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanUInt64MaxValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanUInt64MaxValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanUInt64MaxValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanUInt64MaxValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanUInt64MaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanUInt64MaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.BooleanUInt64MinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.BooleanUInt64MinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.BooleanUInt64MinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.BooleanUInt64MinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.BooleanUInt64MinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.BooleanUInt64MinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.BooleanUInt64MinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.BooleanUInt64MinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.BooleanUInt64MinValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanByteMaxValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanByteMaxValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanByteMaxValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanByteMaxValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanByteMaxValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanByteMaxValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanByteMaxValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.BooleanByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.BooleanByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.BooleanByteMinValue)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.BooleanByteMinValue)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.BooleanByteMinValue)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.BooleanByteMinValue)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.BooleanByteMinValue)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.BooleanByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.BooleanByteMinValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanSByteMaxValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanSByteMaxValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanSByteMaxValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanSByteMaxValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanSByteMaxValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanSByteMaxValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanSByteMaxValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanSByteMaxValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanSByteMaxValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanSByteMinValue)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanSByteMinValue)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanSByteMinValue)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanSByteMinValue)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanSByteMinValue)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanSByteMinValue)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanSByteMinValue)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanSByteMinValue)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanSByteMinValue)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanVal1)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanVal1)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanVal1)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanVal1)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanVal1)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanVal1)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanVal1)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanVal1)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanVal1)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanVal2)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanVal2)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanVal2)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanVal2)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanVal2)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanVal2)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanVal2)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanVal2)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanVal2)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanVal3)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanVal3)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanVal3)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanVal3)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanVal3)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanVal3)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanVal3)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanVal3)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanVal3)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanVal4)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanVal4)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanVal4)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanVal4)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanVal4)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanVal4)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanVal4)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanVal4)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanVal4)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.BooleanVal5)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.BooleanVal5)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.BooleanVal5)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.BooleanVal5)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.BooleanVal5)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.BooleanVal5)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.BooleanVal5)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.BooleanVal5)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.BooleanVal5)))
    Assert(it.AreEqual(it.UInt32Val6,it.uintT(it.BooleanVal6)))
    Assert(it.AreEqual(it.UInt16Val6,it.ushortT(it.BooleanVal6)))
    Assert(it.AreEqual(it.UInt64Val6,it.ulongT(it.BooleanVal6)))
    Assert(it.AreEqual(it.Int32Val6,it.intT(it.BooleanVal6)))
    Assert(it.AreEqual(it.Int16Val6,it.shortT(it.BooleanVal6)))
    Assert(it.AreEqual(it.Int64Val6,it.longT(it.BooleanVal6)))
    Assert(it.AreEqual(it.ByteVal6,it.byteT(it.BooleanVal6)))
    Assert(it.AreEqual(it.SByteVal6,it.sbyteT(it.BooleanVal6)))
    Assert(it.AreEqual(it.BooleanVal6,it.boolT(it.BooleanVal6)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanVal7)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanVal7)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanVal7)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanVal7)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanVal7)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanVal7)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanVal7)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanVal7)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanVal7)))
    Assert(it.AreEqual(it.UInt32Val7,it.uintT(it.BooleanVal8)))
    Assert(it.AreEqual(it.UInt16Val7,it.ushortT(it.BooleanVal8)))
    Assert(it.AreEqual(it.UInt64Val7,it.ulongT(it.BooleanVal8)))
    Assert(it.AreEqual(it.Int32Val7,it.intT(it.BooleanVal8)))
    Assert(it.AreEqual(it.Int16Val7,it.shortT(it.BooleanVal8)))
    Assert(it.AreEqual(it.Int64Val7,it.longT(it.BooleanVal8)))
    Assert(it.AreEqual(it.ByteVal7,it.byteT(it.BooleanVal8)))
    Assert(it.AreEqual(it.SByteVal7,it.sbyteT(it.BooleanVal8)))
    Assert(it.AreEqual(it.BooleanVal8,it.boolT(it.BooleanVal8)))

f()