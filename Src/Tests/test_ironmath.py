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

#
# test Microsoft.Scripting.Math
#


from iptest.assert_util import *
skiptest("win32")


from System import *
import clr
#silverlight already has this
if is_cli:
    math_assembly = (1L).GetType().Assembly
    clr.AddReference(math_assembly)
load_iron_python_test()
import IronPythonTest
    
from Microsoft.Scripting.Math import BigInteger
from Microsoft.Scripting.Math import Complex64


class myFormatProvider(IFormatProvider):
    def ToString():pass
    
p = myFormatProvider()


def test_bigint():
    AreEqual(BigInteger.Add(1L,99999999999999999999999999999999999999999999999999999999999L) ,BigInteger.Subtract(100000000000000000000000000000000000000000000000000000000001L,1L))
    AreEqual(BigInteger.Multiply(400L,500L) , BigInteger.Divide(1000000L,5L))
    AreEqual(BigInteger.Multiply(400L,8L) , BigInteger.LeftShift(400L,3L))
    AreEqual(BigInteger.Divide(400L,8L) , BigInteger.RightShift(400L,3L))
    AreEqual(BigInteger.LeftShift(BigInteger.RightShift(400L,-100L),-100L) , 400L)
    AreEqual(BigInteger.LeftShift(BigInteger.RightShift(-12345678987654321L,-100L),-100L) , -12345678987654321L)
    AreEqual(BigInteger(-123456781234567812345678123456781234567812345678123456781234567812345678L).OnesComplement().OnesComplement() , -123456781234567812345678123456781234567812345678123456781234567812345678L)
    AreEqual(BigInteger(-1234567812345678123456781234567812345678123456781234567812345678123456781234567812345678L).OnesComplement() , -(-1234567812345678123456781234567812345678123456781234567812345678123456781234567812345678L + 1L ))
    Assert(BigInteger.Xor(-1234567812345678123456781234567812345678123456781234567812345678123456781234567812345678L,BigInteger(-1234567812345678123456781234567812345678123456781234567812345678123456781234567812345678L).OnesComplement()) , -1L)
    AreEqual(BigInteger.BitwiseAnd(0xff00ff00,BigInteger.BitwiseOr(0x00ff00ff,0xaabbaabb)) , BigInteger(0xaa00aa00))
    AreEqual(BigInteger.Mod(BigInteger(-9999999999999999999999999999999999999999),1000000000000000000) , -BigInteger.Mod(9999999999999999999999999999999999999999,BigInteger(-1000000000000000000)))


    AreEqual(BigInteger.ToInt64(0x7fffffffffffffff) , 9223372036854775807L)

    AssertError(OverflowError, BigInteger.ToInt64, 0x8000000000000000)



    if not is_net40: #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=25894
        AreEqual(BigInteger(-0).ToBoolean(p) , False )
        AreEqual(BigInteger(-1212321.3213).ToBoolean(p) , True )
        AreEqual(BigInteger(1212321384892342394723947L).ToBoolean(p) , True )
    
        AreEqual(BigInteger(0L).ToChar(p) , Char.MinValue)
        AreEqual(BigInteger(65L).ToChar(p) , IConvertible.ToChar('A', p))
        AreEqual(BigInteger(0xffff).ToChar(p) , Char.MaxValue)
        AssertError(OverflowError, BigInteger(-1).ToChar, p)
    
        AreEqual(BigInteger(100).ToDouble(p) , 100.0)
        AreEqual(BigInteger(BigInteger(100).ToDouble(p)).ToSingle(p) , BigInteger(100.1213123).ToFloat())
    
    Assert(BigInteger(100) != 100.32)
    AreEqual(BigInteger(100) , 100.0)
    
    Assert( 100.32 != BigInteger(100))
    AreEqual(100.0 , BigInteger(100) )

def test_big_1():
    if is_net40:
        print "http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=25894"
        return
    for (a, m, t,x) in [
                        (7, "ToSByte",  SByte,2),
                        (8, "ToByte",   Byte, 0),
                        (15, "ToInt16", Int16,2),
                        (16, "ToUInt16", UInt16,0),
                        (31, "ToInt32", Int32,2),
                        (32, "ToUInt32", UInt32,0),
                        (63, "ToInt64", Int64,2),
                        (64, "ToUInt64", UInt64,0)
                       ]:
    
        b = BigInteger(-x ** a )
        left = getattr(b, m)(p)
        right = t.MinValue
        AreEqual(left, right)
        
        b = BigInteger(2 ** a -1)
        left = getattr(b, m)(p)
        right = t.MaxValue
        AreEqual(left, right)
    
        b = BigInteger(0L)
        left = getattr(b, m)(p)
        right = t.MaxValue - t.MaxValue
        AreEqual(left, 0)

        AssertError(OverflowError,getattr(BigInteger(2 ** a ), m),p)
        AssertError(OverflowError,getattr(BigInteger(-1 - x ** a ), m),p)


def test_big_2():
    for (a, m, t,x) in [
                        (31, "ToInt32",Int32,2),
                        (32, "ToUInt32",UInt32,0),
                        (63, "ToInt64",Int64,2),
                        (64, "ToUInt64",UInt64,0)
                       ]:
    
        b = BigInteger(-x ** a )
        left = getattr(b, m)()
        right = t.MinValue
        AreEqual(left, right)

        b = BigInteger(2 ** a -1)
        left = getattr(b, m)()
        right = t.MaxValue
        AreEqual(left, right)
    
        b = BigInteger(0L)
        left = getattr(b, m)()
        right = t.MaxValue - t.MaxValue
        AreEqual(left, right)

        AssertError(OverflowError,getattr(BigInteger(2 ** a ), m))
        AssertError(OverflowError,getattr(BigInteger(-1 - x ** a ), m))


#complex
def test_complex():
    AreEqual(Complex64.Add(Complex64(BigInteger(9999L),-1234),Complex64(9999,-1234).Conjugate()),Complex64.Multiply(Complex64(BigInteger(9999L)),2))
    AreEqual(Complex64.Add(Complex64(99999.99e-200,12345.88e+100),Complex64.Negate(Complex64(99999.99e-200,12345.88e+100))),Complex64.Subtract(Complex64(99999.99e-200,12345.88e+100),Complex64(99999.99e-200,12345.88e+100)))
    AreEqual (Complex64.Divide(4+2j,2) , (2 + 1j) )
    Assert(not hasattr(Complex64, "Mod"))  #IP 1.x had limited support for modulo which has been removed

def test_bool_misc():
    AreEqual(BigInteger(-1234).Sign, -1)
    AreEqual(BigInteger(-1234).IsZero(), False)
    AreEqual(BigInteger(-1234).IsNegative(), True)
    AreEqual(BigInteger(-1234).IsPositive(), False)
    
    AreEqual(BigInteger(0).Sign, 0)
    AreEqual(BigInteger(0).IsZero(), True)
    AreEqual(BigInteger(0).IsNegative(), False)
    AreEqual(BigInteger(0).IsPositive(), False)

    AreEqual(BigInteger(1234).Sign, 1)
    AreEqual(BigInteger(1234).IsZero(), False)
    AreEqual(BigInteger(1234).IsNegative(), False)
    AreEqual(BigInteger(1234).IsPositive(), True)


def test_byte_conversions():

    def CheckByteConversions(bigint, bytes):
        SequencesAreEqual(bigint.ToByteArray(), bytes)
        AreEqual(BigInteger.Create(Array[Byte](bytes)), bigint)

    CheckByteConversions(BigInteger(0x00), [0x00])

    CheckByteConversions(BigInteger(-0x01), [0xff])
    CheckByteConversions(BigInteger(-0x81), [0x7f, 0xff])
    CheckByteConversions(BigInteger(-0x100), [0x00, 0xff])
    CheckByteConversions(BigInteger(-0x1000), [0x00, 0xf0])
    CheckByteConversions(BigInteger(-0x10000), [0x00, 0x00, 0xff])
    CheckByteConversions(BigInteger(-0x100000), [0x00, 0x00, 0xf0])
    CheckByteConversions(BigInteger(-0x10000000), [0x00, 0x00, 0x00, 0xf0])
    CheckByteConversions(BigInteger(-0x100000000), [0x00, 0x00, 0x00, 0x00, 0xff])

    CheckByteConversions(BigInteger(0x7f), [0x7f])
    CheckByteConversions(BigInteger(0xff), [0xff, 0x00])
    CheckByteConversions(BigInteger(0x0201), [0x01, 0x02])
    CheckByteConversions(BigInteger(0xf2f1), [0xf1, 0xf2, 0x00])
    CheckByteConversions(BigInteger(0x03020100), [0x00, 0x01, 0x02, 0x03])
    CheckByteConversions(BigInteger(0x0403020100), [0x00, 0x01, 0x02, 0x03, 0x04])
    CheckByteConversions(BigInteger(0x0706050403020100), [0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07])
    CheckByteConversions(BigInteger(0x080706050403020100), [0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08])

def test_dword_conversions():
    if is_net40:
        print "http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=25899"
        return
    def CheckDwordConversions(bigint, dwords):
        SequencesAreEqual(bigint.GetWords(), dwords)
        if bigint == BigInteger.Zero:
            AreEqual(
                IronPythonTest.System_Scripting_Math.CreateBigInteger(
                    0,
                    Array[UInt32](dwords),),
                bigint)
        else:
            AreEqual(
                IronPythonTest.System_Scripting_Math.CreateBigInteger(
                    1,
                    Array[UInt32](dwords)),
                bigint)
            AreEqual(
                IronPythonTest.System_Scripting_Math.CreateBigInteger(
                    -1,
                    Array[UInt32](dwords)),
                BigInteger.Negate(bigint))
    
    CheckDwordConversions(BigInteger(0), [])
    CheckDwordConversions(BigInteger(1), [0x00000001])
    CheckDwordConversions(BigInteger((1<<31)), [0x80000000])
    CheckDwordConversions(BigInteger(((1<<31) + 9)), [0x80000009])
    CheckDwordConversions(BigInteger((1<<32)), [0x00000000, 0x00000001])

def test_misc():
    AssertError(ArgumentException, IronPythonTest.System_Scripting_Math.CreateBigInteger, 0, (1, 2, 3))
    AssertError(ArgumentNullException, IronPythonTest.System_Scripting_Math.CreateBigInteger, 0, None)

    AreEqual(BigInteger(1).CompareTo(None), 1)
    AssertError(ArgumentException, BigInteger(1).CompareTo, True)

run_test(__name__)
