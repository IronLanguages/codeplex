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


from Util.Debug import *
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
