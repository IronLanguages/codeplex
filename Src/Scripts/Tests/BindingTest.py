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

import clr
from Util.Debug import *

load_iron_python_test()
from IronPythonTest import *

x = BindingTestClass.Bind("Hello")
Assert(x == "Hello")
x = BindingTestClass.Bind(10)
Assert(x == 10)
x = BindingTestClass.Bind(False)
Assert(x == False)

b = InheritedBindingSub()
Assert(b.Bind(True) == "Subclass bool")
Assert(b.Bind("Hi") == "Subclass string")
Assert(b.Bind(10) == "Subclass int")

x = "this is a string"
y = x.Split(' ')
Assert(y[0] == "this")
Assert(y[1] == "is")
Assert(y[2] == "a")
Assert(y[3] == "string")

def verify_complex(x, xx):
    Assert(x.Real == xx.real)
    Assert(x.Imag == xx.imag)

i  = Cmplx(3, 4)
ii = (3 + 4j)
j  = Cmplx(2, 1)
jj = (2 + 1j)

verify_complex(i, ii)
verify_complex(j, jj)

verify_complex(i + j, ii + jj)
verify_complex(i - j, ii - jj)
verify_complex(i * j, ii * jj)
verify_complex(i / j, ii / jj)

verify_complex(i + 2.5, ii + 2.5)
verify_complex(i - 2.5, ii - 2.5)
verify_complex(i * 2.5, ii * 2.5)
verify_complex(i / 2.5, ii / 2.5)

verify_complex(2.5 + j, 2.5 + jj)
verify_complex(2.5 - j, 2.5 - jj)
verify_complex(2.5 * j, 2.5 * jj)
verify_complex(2.5 / j, 2.5 / jj)

verify_complex(i + 2, ii + 2)
verify_complex(i - 2, ii - 2)
verify_complex(i * 2, ii * 2)
verify_complex(i / 2, ii / 2)

verify_complex(2 + j, 2 + jj)
verify_complex(2 - j, 2 - jj)
verify_complex(2 * j, 2 * jj)
verify_complex(2 / j, 2 / jj)

verify_complex(-i, -ii)
verify_complex(-j, -jj)

i *= j
ii *= jj
verify_complex(i, ii)

i /= j
ii /= jj
verify_complex(i, ii)

i += j
ii += jj
verify_complex(i, ii)

i -= j
ii -= jj
verify_complex(i, ii)

i -= 2
ii -= 2
verify_complex(i, ii)

i += 2
ii += 2
verify_complex(i, ii)

i *= 2
ii *= 2
verify_complex(i, ii)

i /= 2
ii /= 2
verify_complex(i, ii)

class D(Infinite):
    pass

class E(D):
    def __cmp__(self, other):
        return super(E, self).__cmp__(other)

e = E()
result = E.__cmp__(e, 20)
retuls = e.__cmp__(20)

class F(Infinite):
    def __cmp__(self, other):
        return super(F, self).__cmp__(other)

f = F()
result = F.__cmp__(f, 20)
result = f.__cmp__(20)

import System

clr.AddReferenceByPartialName("System.Drawing")
from System.Drawing import Rectangle
r = Rectangle(0, 0, 3, 7)
s = Rectangle(3, 0, 8, 14)
i = Rectangle.Intersect(r, s)
AreEqual(i, Rectangle(3, 0, 0, 7))
AreEqual(r, Rectangle(0, 0, 3, 7))
AreEqual(s, Rectangle(3, 0, 8, 14))
i = r.Intersect(s)
AreEqual(i, None)
AreEqual(r, Rectangle(3, 0, 0, 7))
AreEqual(s, Rectangle(3, 0, 8, 14))


s = System.IO.MemoryStream()
a = System.Array.CreateInstance(System.Byte, 10)
b = System.Array.CreateInstance(System.Byte, a.Length)
for i in range(a.Length):
    a[i] = a.Length - i
s.Write(a, 0, a.Length)
result = s.Seek(0, System.IO.SeekOrigin.Begin)
r = s.Read(b, 0, b.Length)
Assert(r == b.Length)
for i in range(a.Length):
    AreEqual(a[i], b[i])

BoolType    = (System.Boolean, BindTest.BoolValue,    BindResult.Bool)
ByteType    = (System.Byte,    BindTest.ByteValue,    BindResult.Byte)
CharType    = (System.Char,    BindTest.CharValue,    BindResult.Char)
DecimalType = (System.Decimal, BindTest.DecimalValue, BindResult.Decimal)
DoubleType  = (System.Double,  BindTest.DoubleValue,  BindResult.Double)
FloatType   = (System.Single,  BindTest.FloatValue,   BindResult.Float)
IntType     = (System.Int32,   BindTest.IntValue,     BindResult.Int)
LongType    = (System.Int64,   BindTest.LongValue,    BindResult.Long)
ObjectType  = (System.Object,  BindTest.ObjectValue,  BindResult.Object)
SByteType   = (System.SByte,   BindTest.SByteValue,   BindResult.SByte)
ShortType   = (System.Int16,   BindTest.ShortValue,   BindResult.Short)
StringType  = (System.String,  BindTest.StringValue,  BindResult.String)
UIntType    = (System.UInt32,  BindTest.UIntValue,    BindResult.UInt)
ULongType   = (System.UInt64,  BindTest.ULongValue,   BindResult.ULong)
UShortType  = (System.UInt16,  BindTest.UShortValue,  BindResult.UShort)

saveType = type

for binding in [BoolType, ByteType, CharType, DecimalType, DoubleType,
                FloatType, IntType, LongType, ObjectType, SByteType,
                ShortType, StringType, UIntType, ULongType, UShortType]:
    type   = binding[0]
    value  = binding[1]
    expect = binding[2]

    # Select using System.Type object
    select = BindTest.Bind.__overloads__[type]
    result = select(value)
    AreEqual(expect, result)

    # Select using ReflectedType object
    select = BindTest.Bind.__overloads__[type]
    result = select(value)
    AreEqual(expect, result)

    # Make simple call
    result = BindTest.Bind(value)
    if not binding is CharType:
        AreEqual(expect, result)

    result, output = BindTest.BindRef(value)
    if not binding is CharType:
        AreEqual(expect | BindResult.Ref, result)

    # Select using Array type
    arrtype = System.Type.MakeArrayType(type)
    select = BindTest.Bind.__overloads__[arrtype]
    array  = System.Array.CreateInstance(type, 1)
    array[0] = value
    result = select(array)
    AreEqual(expect | BindResult.Array, result)

    # Select using ByRef type
    reftype = System.Type.MakeByRefType(type)
    select = BindTest.Bind.__overloads__[reftype]
    result, output = select()
    AreEqual(expect | BindResult.Out, result)

    select = BindTest.BindRef.__overloads__[reftype]
    result, output = select(value)
    AreEqual(expect | BindResult.Ref, result)

type = saveType

select = BindTest.Bind.__overloads__[()]
result = select()
AreEqual(BindResult.None, result)

class MyEnumTest(EnumTest):
    def TestDaysInt(self):
        return DaysInt.Weekdays

    def TestDaysShort(self):
        return DaysShort.Weekdays

    def TestDaysLong(self):
        return DaysLong.Weekdays

    def TestDaysSByte(self):
        return DaysSByte.Weekdays

    def TestDaysByte(self):
        return DaysByte.Weekdays

    def TestDaysUShort(self):
        return DaysUShort.Weekdays

    def TestDaysUInt(self):
        return DaysUInt.Weekdays

    def TestDaysULong(self):
        return DaysULong.Weekdays

et = MyEnumTest()

AreEqual(et.TestDaysInt(), DaysInt.Weekdays)
AreEqual(et.TestDaysShort(), DaysShort.Weekdays)
AreEqual(et.TestDaysLong(), DaysLong.Weekdays)
AreEqual(et.TestDaysSByte(), DaysSByte.Weekdays)
AreEqual(et.TestDaysByte(), DaysByte.Weekdays)
AreEqual(et.TestDaysUShort(), DaysUShort.Weekdays)
AreEqual(et.TestDaysUInt(), DaysUInt.Weekdays)
AreEqual(et.TestDaysULong(), DaysULong.Weekdays)

for l in range(10):
    a = System.Array.CreateInstance(str, l)
    r = []
    for i in range(l):
        a[i] = "ip" * i
        r.append("IP" * i)
    m = map(str.upper, a)
    AreEqual(m, r)

methods = [
    MyEnumTest.TestEnumInt,
    MyEnumTest.TestEnumShort,
    MyEnumTest.TestEnumLong,
    MyEnumTest.TestEnumSByte,
    MyEnumTest.TestEnumUInt,
    MyEnumTest.TestEnumUShort,
    MyEnumTest.TestEnumULong,
    MyEnumTest.TestEnumByte,
    MyEnumTest.TestEnumBoolean,
]

parameters = [
    DaysInt.Weekdays,
    DaysShort.Weekdays,
    DaysLong.Weekdays,
    DaysSByte.Weekdays,
    DaysByte.Weekdays,
    DaysUShort.Weekdays,
    DaysUInt.Weekdays,
    DaysULong.Weekdays,
]

for p in parameters:
    for m in methods:
        x = m(p)
    x = int(p)
    x = bool(p)

######################################################################################

import IronPythonTest

def Check(flagValue, func, *args):
    IronPythonTest.Dispatch.Flag = 0
    func(*args)
    Assert(IronPythonTest.Dispatch.Flag == flagValue)

d = IronPythonTest.Dispatch()

#======================================================================
#        public void M1(int arg) { Flag = 101; }
#        public void M1(DispatchHelpers.Color arg) { Flag = 201; }
#======================================================================

Check(101, d.M1, 1)
Check(201, d.M1, IronPythonTest.DispatchHelpers.Color.Red)
AssertError(TypeError, d.M1, None)

#======================================================================
#        public void M2(int arg) { Flag = 102; }
#        public void M2(int arg, params int[] arg2) { Flag = 202; }
#======================================================================

Check(102, d.M2, 1)
Check(202, d.M2, 1, 1)
Check(202, d.M2, 1, 1, 1)
Check(202, d.M2, 1, None)
AssertError(TypeError, d.M2, 1, 1, "string", 1)
AssertError(TypeError, d.M2, None, None)
AssertError(TypeError, d.M2, None)

#======================================================================
#        public void M3(int arg) { Flag = 103; }
#        public void M3(int arg, int arg2) { Flag = 203; }
#======================================================================

Check(103, d.M3, IronPythonTest.DispatchHelpers.Color.Red)
Check(103, d.M3, 1)
Check(203, d.M3, 1, 1)
AssertError(TypeError, d.M3, None, None)
AssertError(TypeError, d.M4, None)

#======================================================================
#        public void M4(int arg) { Flag = 104; }
#        public void M4(int arg, __arglist) { Flag = 204; }
#======================================================================

Check(104, d.M4, IronPythonTest.DispatchHelpers.Color.Red)
Check(104, d.M4, 1)
#!!! Apparently IP choose the first one, HOWEVER should we care since
# Reflection MethodInfo.Invoke on vararg method is not supported?
AssertError(TypeError, d.M4, 1, 1)
AssertError(TypeError, d.M4, None, None)
AssertError(TypeError, d.M4, None)

#======================================================================
#        public void M5(float arg) { Flag = 105; }
#        public void M5(double arg) { Flag = 205; }
#======================================================================

#!!! easy way to get M5(float) invoked
Check(105, d.M5, System.Single.Parse("3.14"))
Check(205, d.M5, 3.14)
AssertError(TypeError, d.M5, None)

#======================================================================
#        public void M6(char arg) { Flag = 106; }
#        public void M6(string arg) { Flag = 206; }
#======================================================================

#!!! no way to invoke M6(char)
Check(206, d.M6, 'a')
Check(206, d.M6, 'hello')
Check(206, d.M6, 'hello'[0])
Check(206, d.M6, None)

#======================================================================
#        public void M7(int arg) { Flag = 107; }
#        public void M7(params int[] args) { Flag = 207; }
#======================================================================
Check(207, d.M7)
Check(107, d.M7, 1)
Check(207, d.M7, 1, 1)
Check(207, d.M7, None)

#======================================================================
#        public void M8(int arg) { Flag = 108; }
#        public void M8(ref int arg) { Flag = 208; arg = 999; }
#        public void M10(ref int arg) { Flag = 210; arg = 999; }
#======================================================================
Check(108, d.M8, 1);

Assert(d.M10(1) == 999)
Check(210, d.M10, 1);
AssertError(TypeError, d.M10, None)

#======================================================================
#        public void M11(int arg, int arg2) { Flag = 111; }
#        public void M11(DispatchHelpers.Color arg, int arg2) { Flag = 211; }
#======================================================================

Check(111, d.M11, 1, 1)
Check(111, d.M11, 1, IronPythonTest.DispatchHelpers.Color.Red)
Check(211, d.M11, IronPythonTest.DispatchHelpers.Color.Red, 1)
Check(211, d.M11, IronPythonTest.DispatchHelpers.Color.Red, IronPythonTest.DispatchHelpers.Color.Red)

#======================================================================
#        public void M12(int arg, DispatchHelpers.Color arg2) { Flag = 112; }
#        public void M12(DispatchHelpers.Color arg, int arg2) { Flag = 212; }
#======================================================================

AssertError(TypeError, d.M12, 1, 1)
Check(112, d.M12, 1, IronPythonTest.DispatchHelpers.Color.Red)
Check(212, d.M12, IronPythonTest.DispatchHelpers.Color.Red, 1)
# !!!
#Check(112, d.M12, IronPythonTest.DispatchHelpers.Color.Red, IronPythonTest.DispatchHelpers.Color.Red)

#======================================================================
#        public void M20(DispatchHelpers.B arg) { Flag = 120; }
#======================================================================

Check(120, d.M20, None)

#======================================================================
#        public void M22(DispatchHelpers.B arg) { Flag = 122; }
#        public void M22(DispatchHelpers.D arg) { Flag = 222; }
#======================================================================

# Bug 716: AssertError(TypeError, d.M22, None)
Check(122, d.M22, IronPythonTest.DispatchHelpers.B())
Check(222, d.M22, IronPythonTest.DispatchHelpers.D())

#======================================================================
#        public void M23(DispatchHelpers.I arg) { Flag = 123; }
#        public void M23(DispatchHelpers.C2 arg) { Flag = 223; }
#======================================================================

Check(123, d.M23, IronPythonTest.DispatchHelpers.C1())
Check(223, d.M23, IronPythonTest.DispatchHelpers.C2())

#======================================================================
# Bug 20 - public void M50(params DispatchHelpers.B[] args) { Flag = 150; }
#======================================================================

Check(150, d.M50, IronPythonTest.DispatchHelpers.B())
Check(150, d.M50, IronPythonTest.DispatchHelpers.D())
Check(150, d.M50, IronPythonTest.DispatchHelpers.B(), IronPythonTest.DispatchHelpers.B())
Check(150, d.M50, IronPythonTest.DispatchHelpers.B(), IronPythonTest.DispatchHelpers.D())
Check(150, d.M50, IronPythonTest.DispatchHelpers.D(), IronPythonTest.DispatchHelpers.D())

#======================================================================
#        public void M51(params DispatchHelpers.B[] args) { Flag = 151; }
#        public void M51(params DispatchHelpers.D[] args) { Flag = 251; }
#======================================================================

Check(151, d.M51, IronPythonTest.DispatchHelpers.B())
Check(251, d.M51, IronPythonTest.DispatchHelpers.D())
Check(151, d.M51, IronPythonTest.DispatchHelpers.B(), IronPythonTest.DispatchHelpers.B())
Check(151, d.M51, IronPythonTest.DispatchHelpers.B(), IronPythonTest.DispatchHelpers.D())
Check(251, d.M51, IronPythonTest.DispatchHelpers.D(), IronPythonTest.DispatchHelpers.D())

#======================================================================
#        public void M60(int? arg) { Flag = 160; }
#======================================================================
Check(160, d.M60, 1)
Check(160, d.M60, None)

#======================================================================
#        public void M70(Dispatch arg) { Flag = 170; }
#======================================================================
Check(170, d.M70, d)
AssertError(TypeError, IronPythonTest.Dispatch.M70, d)
AssertError(TypeError, d.M70, d, d)
Check(170, IronPythonTest.Dispatch.M70, d, d)

#======================================================================
#        public static void M71(Dispatch arg) { Flag = 171; }
#======================================================================
Check(171, d.M71, d)
Check(171, IronPythonTest.Dispatch.M71, d)
AssertError(TypeError, d.M71, d, d)
AssertError(TypeError, IronPythonTest.Dispatch.M71, d, d)

#======================================================================
#        public static void M81(Dispatch arg, int arg2) { Flag = 181; }
#        public void M81(int arg) { Flag = 281; }
#======================================================================
Check(181, d.M81, d, 1)
Check(181, IronPythonTest.Dispatch.M81, d, 1)
Check(281, d.M81, 1)
AssertError(TypeError, IronPythonTest.Dispatch.M81, 1)

#======================================================================
#        public static void M82(bool arg) { Flag = 182; }
#        public static void M82(string arg) { Flag = 282; }
#======================================================================
Check(182, d.M82, True)
Check(282, d.M82, "True")
Check(182, IronPythonTest.Dispatch.M82, True)
Check(282, IronPythonTest.Dispatch.M82, "True")

#======================================================================
#        public void M83(bool arg) { Flag = 183; }
#        public void M83(string arg) { Flag = 283; }
#======================================================================
Check(183, d.M83, True)
Check(283, d.M83, "True")
AssertError(TypeError, IronPythonTest.Dispatch.M83, True)
AssertError(TypeError, IronPythonTest.Dispatch.M83, "True")
AssertError(TypeError, d.M83, d, True)
AssertError(TypeError, d.M83, d, "True")
Check(183, IronPythonTest.Dispatch.M83, d, True)
Check(283, IronPythonTest.Dispatch.M83, d, "True")


#======================================================================
#        public void M90<T>(int arg) { Flag = 190; }
#======================================================================
## Bug 474: AssertError(TypeError, d.M90, 1)
## Check(191, d.M91, 1) : not sure this?

#======================================================================
#======================================================================

d = IronPythonTest.DispatchDerived()
Check(201, d.M1, 1)

Check(102, d.M2, 1)
Check(202, d.M2, IronPythonTest.DispatchHelpers.Color.Red)

Check(103, d.M3, 1)
Check(203, d.M3, "hello")

Check(104, d.M4, 100)
Check(204, d.M4, "python")

Check(205, d.M5, 1)
Check(106, d.M6, 1)

#======================================================================
# ConversionDispatch - Test binding List / Tuple to array/enum/IList/ArrayList/etc...
#======================================================================

cd = IronPythonTest.ConversionDispatch()

###########################################
# checker functions - verify the result of the test

def Check(res, orig):
    if hasattr(res, "__len__"):
        AreEqual(len(res), len(orig))
    i = 0
    for a in res:
        AreEqual(a, orig[i])
        i = i+1
    AreEqual(i, len(orig))

def CheckModify(res, orig):
    Check(res, orig)

    index = res.Count
    res.Add(orig[0])
    Check(res, orig)

    res.RemoveAt(index)
    Check(res, orig)

    x = res[0]
    res.Remove(orig[0])
    Check(res, orig)

    res.Insert(0, x)
    Check(res, orig)

    if(hasattr(res, "Sort")):
        res.Sort()
        Check(res, orig)

    res.Clear()
    Check(res, orig)

def CheckDict(res, orig):
    if hasattr(res, "__len__"):
        AreEqual(len(res), len(orig))
    i = 0
    for a in res.Keys:
        AreEqual(res[a], orig[a])
        i = i+1
    AreEqual(i, len(orig))


###################################
# test data sets used for all the checks


# list/tuple data
inttuple = (2,3,4,5)
strtuple = ('a', 'b', 'c', 'd')
othertuple = (['a', 2], ['c', 'd', 3], 5)


intlist = [2,3,4,5]
strlist = ['a', 'b', 'c', 'd']
otherlist = [('a', 2), ('c', 'd', 3), 5]

intdict = {2:5, 7:8, 9:10}
strdict = {'abc': 'def', 'xyz':'abc', 'mno':'prq'}
objdict = { (2,3) : (4,5), (1,2):(3,4), (8,9):(1,4)}
mixeddict = {'abc': 2, 'def': 9, 'qrs': 8}

objFunctions = [cd.Array,cd.ObjIList, cd.ObjList, cd.ArrayList, cd.Enumerator]
objData = [inttuple, strtuple, othertuple]

intFunctions = [cd.IntArray, cd.IntEnumerator, cd.IntIList]
intData = [inttuple, intlist]

intTupleFunctions = [cd.IntList]
intTupleData = [inttuple]

strFunctions = [cd.StringArray, cd.StringEnumerator, cd.StringIList]
strData = [strtuple, strlist]

strTupleFunctions = [cd.StringList]
strTupleData = [strtuple]

# dictionary data

objDictFunctions = [cd.DictTest, cd.HashtableTest]
objDictData = [intdict, strdict, objdict, mixeddict]

intDictFunctions = [cd.IntDictTest]
intDictData = [intdict]

strDictFunctions = [cd.StringDictTest]
strDictData = [strdict]

mixedDictFunctions = [cd.MixedDictTest]
mixedDictData = [mixeddict]

modCases = [ (cd.ObjIList, (intlist, strlist, otherlist)), \
             ( cd.IntIList, (intlist,) ),  \
             ( cd.StringIList, (strlist,) ),   \
             ( cd.ArrayList, (intlist, strlist, otherlist) ) ]

testCases = [ [objFunctions, objData], \
              [intFunctions, intData], \
              [strFunctions, strData], \
              [intTupleFunctions, intTupleData], \
              [strTupleFunctions, strTupleData] ]

dictTestCases = ( (objDictFunctions, objDictData ), \
                  (intDictFunctions, intDictData ), \
                  (strDictFunctions, strDictData),  \
                  (mixedDictFunctions, mixedDictData) )

############################################3
# run the test cases:

# verify all conversions succeed properly

for cases in testCases:
    for func in cases[0]:
        for data in cases[1]:
            Check(func(data), data)


# verify that modifications show up as appropriate.

for case in modCases:
    for data in case[1]:
        newData = list(data)
        CheckModify(case[0](newData), newData)


# verify dictionary test cases

for case in dictTestCases:
    for data in case[1]:
        for func in case[0]:
            newData = dict(data)
            CheckDict(func(newData), newData)


x = FieldTest()
y = System.Collections.Generic.List[System.Type]()
x.Field = y

# verify we can bind w/ add & radd
AreEqual(x.Field, y)

a = Cmplx(2, 3)
b = Cmplx2(3, 4)

x = a + b
y = b + a


#############################################################
# Verify combinaions of instance / no instance

a = MixedDispatch("one")
b = MixedDispatch("two")
c = MixedDispatch("three")
d = MixedDispatch("four")



x= a.Combine(b)
y = MixedDispatch.Combine(a,b)

AreEqual(x.called, "instance")
AreEqual(y.called, "static")

x= a.Combine2(b)
y = MixedDispatch.Combine2(a,b)
z = MixedDispatch.Combine2(a,b,c,d)
v = a.Combine2(b,c,d)

AreEqual(x.called, "instance")
AreEqual(y.called, "static")
AreEqual(z.called, "instance_three")
AreEqual(v.called, "instance_three")


###########################################################
# verify non-instance built-in's don't get bound

class C:
    mycmp = cmp
    
a = C()
AreEqual(a.mycmp(0,0), 0)

###########################################################
# verify generic .NET method binding

# Create an instance of the generic method provider class.
gm = GenMeth()

# Check that the documentation strings for all the instance methods (they all have the same name) is as expected.
expected_inst_methods = 'str InstMeth[T]()\r\nstr InstMeth[(T, U)]()\r\nstr InstMeth[T](int arg1)\r\nstr InstMeth[T](str arg1)\r\nstr InstMeth[(T, U)](int arg1)\r\nstr InstMeth[T](T arg1)\r\nstr InstMeth[(T, U)](T arg1, U arg2)\r\nstr InstMeth()\r\nstr InstMeth(int arg1)\r\nstr InstMeth(str arg1)\r\n';
Assert(gm.InstMeth.__doc__ == expected_inst_methods)

# And the same for the static methods.
expected_static_methods = 'static str StaticMeth[T]()\r\nstatic str StaticMeth[(T, U)]()\r\nstatic str StaticMeth[T](int arg1)\r\nstatic str StaticMeth[T](str arg1)\r\nstatic str StaticMeth[(T, U)](int arg1)\r\nstatic str StaticMeth[T](T arg1)\r\nstatic str StaticMeth[(T, U)](T arg1, U arg2)\r\nstatic str StaticMeth()\r\nstatic str StaticMeth(int arg1)\r\nstatic str StaticMeth(str arg1)\r\n'
Assert(GenMeth.StaticMeth.__doc__ == expected_static_methods)

# Check that we bind to the correct method based on type and call arguments for each of our instance methods. We can validate this
# because each target method returns a unique string we can compare.
AreEqual(gm.InstMeth(), "InstMeth()")
AreEqual(gm.InstMeth[str](), "InstMeth<String>()")
AreEqual(gm.InstMeth[(int, str)](), "InstMeth<Int32, String>()")
AreEqual(gm.InstMeth(1), "InstMeth(Int32)")
AreEqual(gm.InstMeth(""), "InstMeth(String)")
AreEqual(gm.InstMeth[int](1), "InstMeth<Int32>(Int32)")
AreEqual(gm.InstMeth[str](""), "InstMeth<String>(String)")
AreEqual(gm.InstMeth[(str, int)](1), "InstMeth<String, Int32>(Int32)")
AreEqual(gm.InstMeth[GenMeth](gm), "InstMeth<GenMeth>(GenMeth)")
AreEqual(gm.InstMeth[(str, int)]("", 1), "InstMeth<String, Int32>(String, Int32)")

# And the same for the static methods.
AreEqual(GenMeth.StaticMeth(), "StaticMeth()")
AreEqual(GenMeth.StaticMeth[str](), "StaticMeth<String>()")
AreEqual(GenMeth.StaticMeth[(int, str)](), "StaticMeth<Int32, String>()")
AreEqual(GenMeth.StaticMeth(1), "StaticMeth(Int32)")
AreEqual(GenMeth.StaticMeth(""), "StaticMeth(String)")
AreEqual(GenMeth.StaticMeth[int](1), "StaticMeth<Int32>(Int32)")
AreEqual(GenMeth.StaticMeth[str](""), "StaticMeth<String>(String)")
AreEqual(GenMeth.StaticMeth[(str, int)](1), "StaticMeth<String, Int32>(Int32)")
AreEqual(GenMeth.StaticMeth[GenMeth](gm), "StaticMeth<GenMeth>(GenMeth)")
AreEqual(GenMeth.StaticMeth[(str, int)]("", 1), "StaticMeth<String, Int32>(String, Int32)")



####################################################################################
# Default parameter value tests

tst = DefaultValueTest()


AreEqual(tst.Test_Enum(), BindResult.Bool)
AreEqual(tst.Test_BigEnum(), BigEnum.BigValue)
AreEqual(tst.Test_String(), 'Hello World')
AreEqual(tst.Test_Int(), 5)
AreEqual(tst.Test_UInt(), 4294967295)
AreEqual(tst.Test_Bool(), True)
AreEqual(str(tst.Test_Char()), 'A')
AreEqual(tst.Test_Byte(), 2)
AreEqual(tst.Test_SByte(), 2)
AreEqual(tst.Test_Short(), 2)
AreEqual(tst.Test_UShort(), 2)
AreEqual(tst.Test_Long(), 9223372036854775807)
AreEqual(tst.Test_ULong(), 18446744073709551615)



####################################################################################
# coverage
def test_function(c, o):
    ############ OptimizedFunctionX ############
    line = ""
    for i in range(6): 
        args = ",".join(['1'] * i)
        line += 'AreEqual(o.IM%d(%s), "IM%d")\n' % (i, args, i)
        line += 'AreEqual(c.IM%d(o,%s), "IM%d")\n' % (i, args, i)
        if i > 0: 
            line += 'try: o.IM%d(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (i, ",".join(['1'] * (i-1)))
            line += 'try: c.IM%d(o, %s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (i, ",".join(['1'] * (i-1)))
        line += 'try: o.IM%d(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (i, ",".join(['1'] * (i+1)))
        line += 'try: c.IM%d(o, %s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (i, ",".join(['1'] * (i+1)))
            
        line += 'AreEqual(o.SM%d(%s), "SM%d")\n' % (i, args, i)
        line += 'AreEqual(c.SM%d(%s), "SM%d")\n' % (i, args, i)
        
        if i > 0: 
            line += 'try: o.SM%d(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (i, ",".join(['1'] * (i-1)))
            line += 'try: c.SM%d(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (i, ",".join(['1'] * (i-1)))
        line += 'try: o.SM%d(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (i, ",".join(['1'] * (i+1)))
        line += 'try: c.SM%d(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (i, ",".join(['1'] * (i+1)))
    
    #print line
    exec line

    ############ OptimizedFunctionAny ############
    ## 1    
    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in [0, 3, 4]:
            line += 'AreEqual(o.IDM0(%s), "IDM0-%d")\n' % (args, i)
            line += 'AreEqual(c.IDM0(o,%s), "IDM0-%d")\n' % (args, i)
        else:
            line += 'try: o.IDM0(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)
            line += 'try: c.IDM0(o, %s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)
            
    #print line
    exec line    
    
    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in [0, 3]:
            line += 'AreEqual(o.SDM0(%s), "SDM0-%d")\n' % (args, i)
            line += 'AreEqual(c.SDM0(%s), "SDM0-%d")\n' % (args, i)
        else:
            line += 'try: o.SDM0(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)
            line += 'try: c.SDM0(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)
            
    #print line
    exec line    

    ## 2
    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in [1]:
            line += 'AreEqual(o.IDM1(%s), "IDM1-%d")\n' % (args, i)
            line += 'AreEqual(c.IDM1(o,%s), "IDM1-%d")\n' % (args, i)
            line += 'AreEqual(o.SDM1(%s), "SDM1-%d")\n' % (args, i)
            line += 'AreEqual(c.SDM1(%s), "SDM1-%d")\n' % (args, i)
        else:
            line += 'AreEqual(o.IDM1(%s), "IDM1-x")\n' % (args)
            line += 'AreEqual(c.IDM1(o,%s), "IDM1-x")\n' % (args)
            line += 'AreEqual(o.SDM1(%s), "SDM1-x")\n' % (args)
            line += 'AreEqual(c.SDM1(%s), "SDM1-x")\n' % (args)
            
    #print line
    exec line    

    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in [2]:
            line += 'AreEqual(o.IDM4(%s), "IDM4-%d")\n' % (args, i)
            line += 'AreEqual(c.IDM4(o,%s), "IDM4-%d")\n' % (args, i)
            line += 'AreEqual(o.SDM4(%s), "SDM4-%d")\n' % (args, i)
            line += 'AreEqual(c.SDM4(%s), "SDM4-%d")\n' % (args, i)
        else:
            line += 'AreEqual(o.IDM4(%s), "IDM4-x")\n' % (args)
            line += 'AreEqual(c.IDM4(o,%s), "IDM4-x")\n' % (args)
            line += 'AreEqual(o.SDM4(%s), "SDM4-x")\n' % (args)
            line += 'AreEqual(c.SDM4(%s), "SDM4-x")\n' % (args)
            
    #print line
    exec line    

    ## 3
    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in range(5):
            line += 'AreEqual(o.IDM2(%s), "IDM2-%d")\n' % (args, i)
            line += 'AreEqual(c.IDM2(o,%s), "IDM2-%d")\n' % (args, i)
        else:
            line += 'try: o.IDM2(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)
            line += 'try: c.IDM2(o, %s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)
    #print line
    exec line    

    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in range(6):
            line += 'AreEqual(o.SDM2(%s), "SDM2-%d")\n' % (args, i)
            line += 'AreEqual(c.SDM2(%s), "SDM2-%d")\n' % (args, i)
        else:
            line += 'try: o.SDM2(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)
            line += 'try: c.SDM2(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)

    #print line
    exec line    
    
    ## 4
    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in [0, 5]:
            line += 'AreEqual(o.IDM5(%s), "IDM5-%d")\n' % (args, i)
            line += 'AreEqual(c.IDM5(o,%s), "IDM5-%d")\n' % (args, i)
        else:
            line += 'try: o.IDM5(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)
            line += 'try: c.IDM5(o, %s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)

    #print line
    exec line    

    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in [0, 6]:
            line += 'AreEqual(o.SDM5(%s), "SDM5-%d")\n' % (args, i)
            line += 'AreEqual(c.SDM5(%s), "SDM5-%d")\n' % (args, i)
        else:
            line += 'try: o.SDM5(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)
            line += 'try: c.SDM5(%s) \nexcept TypeError: pass \nelse: raise AssertionError\n' % (args)

    #print line
    exec line    

    ## 5
    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in range(5):
            line += 'AreEqual(o.IDM3(%s), "IDM3-%d")\n' % (args, i)
            line += 'AreEqual(c.IDM3(o,%s), "IDM3-%d")\n' % (args, i)
        else:
            line += 'AreEqual(o.IDM3(%s), "IDM3-x")\n' % (args)
            line += 'AreEqual(c.IDM3(o,%s), "IDM3-x")\n' % (args)
            
    #print line
    exec line    
    
    line = ""
    for i in range(7):
        args = ",".join(['1'] * i)
        if i in range(6):
            line += 'AreEqual(o.SDM3(%s), "SDM3-%d")\n' % (args, i)
            line += 'AreEqual(c.SDM3(%s), "SDM3-%d")\n' % (args, i)
        else:
            line += 'AreEqual(o.SDM3(%s), "SDM3-x")\n' % (args)
            line += 'AreEqual(c.SDM3(%s), "SDM3-x")\n' % (args)
            
    #print line
    exec line   

    ############ OptimizedFunctionN ############
    line = ""
    for i in range(6): 
        args = ",".join(['1'] * i)
        line +=  'AreEqual(o.IPM0(%s), "IPM0-%d")\n' % (args, i)
        line +=  'AreEqual(o.SPM0(%s), "SPM0-%d")\n' % (args, i)
        line +=  'AreEqual(c.IPM0(o,%s), "IPM0-%d")\n' % (args, i)
        line +=  'AreEqual(c.SPM0(%s), "SPM0-%d")\n' % (args, i)
        
        line +=  'AreEqual(o.SPM1(0,%s), "SPM1-%d")\n' % (args, i)
        line +=  'AreEqual(o.IPM1(0,%s), "IPM1-%d")\n' % (args, i)
        line +=  'AreEqual(c.IPM1(o, 0,%s), "IPM1-%d")\n' % (args, i)
        line +=  'AreEqual(c.SPM1(0,%s), "SPM1-%d")\n' % (args, i)

    #print line
    exec line   
    
class DispatchAgain2(DispatchAgain): pass

test_function(DispatchAgain, DispatchAgain())
test_function(DispatchAgain2, DispatchAgain2())

AreEqual(type(BindTest.ReturnTest('char')), System.Char)
AreEqual(type(BindTest.ReturnTest('null')), type(None))
AreEqual(type(BindTest.ReturnTest('object')), object)
Assert(repr(BindTest.ReturnTest("com")).startswith('<System.__ComObject'))

#####################################################################
## testing multicall generator

c = MultiCall()

def AllEqual(exp, meth, passins):
    for arg in passins: 
        #print meth, arg
        AreEqual(meth(*arg), exp)

def AllAssert(type, meth, passins):
    for arg in passins: 
        #print meth, arg
        AssertError(type, meth, arg)    

import sys
maxint = sys.maxint
import System
maxlong1 = System.Int64.MaxValue
maxlong2 = long(str(maxlong1))

#############################################################################################
#        public int M0(int arg) { return 1; }
#        public int M0(long arg) { return 2; }

func = c.M0
AllEqual(1, func, [(0,), (1,), (maxint,), (10L,), (-1234.0,)])
AllEqual(2, func, [(maxint + 1,), (-maxint-10,)])
AllAssert(TypeError, func, [ 
    #(-10.2,),
    (1+2j,), 
    ("10",), 
    (System.Byte.Parse("2"),)
    ])
    
#############################################################################################
#        public int M1(int arg) { return 1; }
#        public int M1(long arg) { return 2; }
#        public int M1(object arg) { return 3; }

func = c.M1
AllEqual(1, func, [
    (0,), 
    (1,), 
    (maxint,), 
    #(10L,), 
    #(-1234.0,),
    ])
AllEqual(2, func, [
    #(maxint + 1,), 
    #(-maxint-10,),
    ])
AllEqual(3, func, [(-10.2,), (1+2j,), ("10",), (System.Byte.Parse("2"),)])

#############################################################################################
#        public int M2(int arg1, int arg2) { return 1; }
#        public int M2(long arg1, int arg2) { return 2; }
#        public int M2(int arg1, long arg2) { return 3; }
#        public int M2(long arg1, long arg2) { return 4; }
#        public int M2(object arg1, object arg2) { return 5; }

func = c.M2
AllEqual(1, func, [
    (0, 0), (1, maxint), (maxint, 1), (maxint, maxint), 
    #(10L, 0),
    ])

AllEqual(2, func, [
    #(maxint+1, 0), 
    #(maxint+10, 10),
    #(maxint+10, 10L),
    (maxlong1, 0),
    #(maxlong2, 0),
    ])

AllEqual(3, func, [
    (0, maxint+1), 
    (10, maxint+10),
    #(10L, maxint+10),
    ])

AllEqual(4, func, [
    #(maxint+10, maxint+1), 
    #(-maxint-10, maxint+10),
    #(-maxint-10L, maxint+100),
    (maxlong1, maxlong1),
    #(maxlong2, maxlong1),
    ])  
    
AllEqual(5, func, [
    (maxlong1 + 1, 1),
    (maxlong2 + 1, 1),
    (maxint, maxlong1 + 10), 
    (maxint, maxlong2 + 10), 
    (1, "100L"),
    (10.2, 1),
    ])       
    
#############################################################################################    
#        public int M4(int arg1, int arg2, int arg3, int arg4) { return 1; }
#        public int M4(object arg1, object arg2, object arg3, object arg4) { return 2; }

one = [t.Parse("5") for t in [System.Byte, System.SByte, System.UInt16, System.Int16, System.UInt32, System.Int32, 
             System.UInt32, System.Int32, System.UInt64, System.Int64, 
             System.Char, System.Decimal, System.Single, System.Double] ]
one.extend([True, False, 5L, DispatchHelpers.Color.Red ])

two = [t.Parse("5.5") for t in [ System.Decimal, System.Single, System.Double] ]
two.extend([None, "5", "5.5", maxint * 2, ])

together = [] 
together.extend(one)
together.extend(two)

ignore = '''
for a1 in together:
    for a2 in together:
        for a3 in together:
            for a4 in together:
                # print a1, a2, a3, a4, type(a1), type(a1), type(a2), type(a3), type(a4)
                if a1 in two or a2 is two or a3 in two or a4 in two:
                    AreEqual(c.M4(a1, a2, a3, a4), 2)
                else :
                    AreEqual(c.M4(a1, a2, a3, a4), 1)
'''

#############################################################################################    
#        public int M5(DispatchHelpers.B arg1, DispatchHelpers.B args) { return 1; }
#        public int M5(DispatchHelpers.D arg1, DispatchHelpers.B args) { return 2; }
#        public int M5(object arg1, object args) { return 3; }
b = DispatchHelpers.B()
d = DispatchHelpers.D()

func = c.M5

AllEqual(1, func, [(b, b), (b, d)])
AllEqual(2, func, [(d, b), (d, d)])
AllEqual(3, func, [(1, 2)])

#############################################################################################    
#        public int M6(DispatchHelpers.B arg1, DispatchHelpers.B args) { return 1; }
#        public int M6(DispatchHelpers.B arg1, DispatchHelpers.D args) { return 2; }
#        public int M6(object arg1, DispatchHelpers.D args) { return 3; }

func = c.M6

AllEqual(1, func, [(b, b), (d, b)])
AllEqual(2, func, [(b, d), (d, d)])
AllEqual(3, func, [(1, d), (6L, d)])
AllAssert(TypeError, func, [(1,1), (None, None), (None, d), (3, b)])

#############################################################################################    
#        public int M7(DispatchHelpers.B arg1, DispatchHelpers.B args) { return 1; }
#        public int M7(DispatchHelpers.B arg1, DispatchHelpers.D args) { return 2; }
#        public int M7(DispatchHelpers.D arg1, DispatchHelpers.B args) { return 3; }
#        public int M7(DispatchHelpers.D arg1, DispatchHelpers.D args) { return 4; }

func = c.M7
AllEqual(1, func, [(b, b)])
AllEqual(2, func, [(b, d)])
AllEqual(3, func, [(d, b)])
AllEqual(4, func, [(d, d)])
AllAssert(TypeError, func, [(1,1), (None, None), (None, d)])

#############################################################################################    
#        public int M8(int arg1, int arg2) { return 1;}
#        public int M8(DispatchHelpers.B arg1, DispatchHelpers.B args) { return 2; }
#        public int M8(object arg1, object arg2) { return 3; }

func = c.M8
AllEqual(1, func, [(1, 2), (maxint, 2L)])
AllEqual(2, func, [(b, b), (b, d), (d, b), (d, d)])
AllEqual(3, func, [(5.1, b), (1, d), (d, 1), (d, maxlong2), (maxlong1, d), (None, 3), (3, None)])

#############################################################################################    
# public static int M92(out int i, out int j, out int k, bool boolIn)
AreEqual(Dispatch.M92(True), (4, 1,2,3))
AreEqual(Dispatch.Flag, 192)

#############################################################################################    
# public int M93(out int i, out int j, out int k, bool boolIn)
AreEqual(Dispatch().M93(True), (4, 1,2,3))
AreEqual(Dispatch.Flag, 193)

#############################################################################################    
# public int M94(out int i, out int j, bool boolIn, out int k)
AreEqual(Dispatch().M94(True), (4, 1,2,3))
AreEqual(Dispatch.Flag, 194)

#############################################################################################    
# public static int M95(out int i, out int j, bool boolIn, out int k)
AreEqual(Dispatch.M95(True), (4, 1,2,3))
AreEqual(Dispatch.Flag, 195)

#############################################################################################    
# public static int M96(out int x, out int j, params int[] extras) 
AreEqual(Dispatch.M96(), (0, 1,2))
AreEqual(Dispatch.Flag, 196)
AreEqual(Dispatch.M96(1,2), (3, 1,2))
AreEqual(Dispatch.Flag, 196)
AreEqual(Dispatch.M96(1,2,3), (6, 1,2))
AreEqual(Dispatch.Flag, 196)

#############################################################################################    
# public int M97(out int x, out int j, params int[] extras)
AreEqual(Dispatch().M97(), (0, 1,2))
AreEqual(Dispatch.Flag, 197)
AreEqual(Dispatch().M97(1,2), (3, 1,2))
AreEqual(Dispatch.Flag, 197)
AreEqual(Dispatch().M97(1,2,3), (6, 1,2))
AreEqual(Dispatch.Flag, 197)


#############################################################################################    
# public void M98(string a, string b, string c, string d, out int x, ref Dispatch di)
a = Dispatch()
x = a.M98('1', '2', '3', '4', a)
AreEqual(x[0], 10)
AreEqual(x[1], a)
# doc for this method should have the out & ref params as return values
AreEqual(a.M98.__doc__, '(int, Dispatch) M98(str a, str b, str c, str d, Dispatch di)\r\n')

# call type.InvokeMember on String.ToString - all methods have more arguments than max args.
res = clr.GetClrType(str).InvokeMember('ToString', System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.InvokeMethod, None, 'abc', [])
AreEqual(res, 'abc')
