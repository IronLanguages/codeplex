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
if is_cli:
    from System import Int64, Byte
    
Assert("pypypypypy" == 5 * "py")
Assert("pypypypypy" == "py" * 5)
Assert("pypypypy" == 2 * "py" * 2)

Assert(['py', 'py', 'py'] == ['py'] * 3)
Assert(['py', 'py', 'py'] == 3 * ['py'])

Assert(['py', 'py', 'py', 'py', 'py', 'py', 'py', 'py', 'py'] == 3 * ['py'] * 3)

Assert(3782452410 > 0)

# Complex tests
Assert((2+4j)/(1+1j) == (3+1j))
Assert((2+10j)/4.0 == (0.5+2.5j))
AreEqual(1j ** 2, (-1.0+0j))
AreEqual(pow(1j, 2), (-1.0+0j))
AreEqual(1+0j, 1)
AreEqual(1+0j, 1.0)
AreEqual(1+0j, 1L)
AreEqual((1+1j)/1L, (1+1j))
AreEqual((1j) + 1L, (1+1j))
if is_cli: AreEqual((1j) + Int64(), 1j)
AssertError(TypeError, (lambda:(1+1j)+[]))

AreEqual( (12j//5j), (2+0j))
AreEqual( (12.0+0j) // (5.0+0j), (2+0j) )
AreEqual( (12+0j) // (0+3j), 0j)
AreEqual( (0+12j) // (3+0j), 0j)
AssertError(TypeError, (lambda:2j // "astring"))
AssertError(ZeroDivisionError, (lambda:3.0 // (0j)))
AreEqual ( 3.0 // (2j), 0j)
AreEqual( 12 // (3j), 0j)
AreEqual( 25 % (5+3j), (10-9j))

AreEqual((12+3j)/3L, (4+1j))
AreEqual(3j - 5L, -5+3j)
if is_cli: AreEqual(3j - Int64(), 3j)
AssertError(TypeError, (lambda:3j-[]))
if is_cli: AreEqual(pow(5j, Int64()), (1+0j))
AreEqual(pow(5j, 0L), (1+0j))
AssertError(TypeError, (lambda:pow(5j, [])))
if is_cli: AreEqual(5j * Int64(), 0)
AreEqual(5j * 3L, 15j)
AssertError(TypeError, (lambda:(5j*[])))

# Extensible Float tests
class XFloat(float): pass

AreEqual(XFloat(3.14), 3.14)
Assert(XFloat(3.14) < 4.0)
Assert(XFloat(3.14) > 3.0)
Assert(XFloat(3.14) < XFloat(4.0))
Assert(XFloat(3.14) > XFloat(3.0))

Assert(0xabcdef01 + (0xabcdef01<<32)+(0xabcdef01<<64) == 0xabcdef01abcdef01abcdef01)

Assert(round(-5.5) == (-6.0))
Assert(round(-5.0) == (-5.0))
Assert(round(-4.5) == (-5.0))
Assert(round(-4.0) == (-4.0))
Assert(round(-3.5) == (-4.0))
Assert(round(-3.0) == (-3.0))
Assert(round(-2.5) == (-3.0))
Assert(round(-2.0) == (-2.0))
Assert(round(-1.5) == (-2.0))
Assert(round(-1.0) == (-1.0))
Assert(round(-0.5) == (-1.0))
Assert(round(0.0) == (0.0))
Assert(round(0.5) == (1.0))
Assert(round(1.0) == (1.0))
Assert(round(1.5) == (2.0))
Assert(round(2.0) == (2.0))
Assert(round(2.5) == (3.0))
Assert(round(3.0) == (3.0))
Assert(round(3.5) == (4.0))
Assert(round(4.0) == (4.0))
Assert(round(4.5) == (5.0))
Assert(round(5.0) == (5.0))

x = ('a', 'b', 'c')
y = x
y *= 3
z = x
z += x
z += x
Assert(y == z)

Assert(1 << 32 == 4294967296L)
Assert(2 << 32 == (1 << 32) << 1)
Assert(((1 << 16) << 16) << 16 == 1 << 48)
Assert(((1 << 16) << 16) << 16 == 281474976710656L)

for i in [1, 10, 42, 1000000000, 34141235135135135, 13523525234523452345235235234523, 100000000000000000000000000000000000000]:
    Assert(~i == -i - 1)

Assert(7 ** 5 == 7*7*7*7*7)
Assert(7L ** 5L == 7L*7L*7L*7L*7L)
Assert(7 ** 5L == 7*7*7*7*7)
Assert(7L ** 5 == 7L*7L*7L*7L*7L)
Assert(1 ** 735293857239475 == 1)
Assert(0 ** 735293857239475 == 0)

# cpython tries to compute this, takes a long time to finish
if is_cli:
    AssertError(ValueError, (lambda: 10 ** 735293857239475))

Assert(2 ** 3.0 == 8.0)
Assert(2.0 ** 3 == 8.0)
Assert(4 ** 0.5 == 2.0)

nums = [3, 2.3, (2+1j), (2-1j), 1j, (-1j), 1]
for x in nums:
    for y in nums:
        z = x ** y

AreEqual(7.1//2.1, 3.0)
AreEqual(divmod(5.0, 2), (2.0,1.0))
AreEqual(divmod(5,2), (2,1))
AreEqual(divmod(-5,2), (-3,1))

AreEqual(True | False, True)
AreEqual(True | 4, 5)
AreEqual(True & 3, 1)
AreEqual(True + 3, 4)
AreEqual(True - 10, -9)
AreEqual(True * 8, 8)
AreEqual(True / 3, 0)
AreEqual(True ** 5, 1)
AreEqual(True % 2, 1)
AreEqual(True << 4, 1 << 4)
AreEqual(True >> 2, 0)
AreEqual(True ^ 3, 2)

if is_cli: 
    a = Byte()
    AreEqual(type(a), Byte)
    AreEqual(a, 0)

    b = a + 1
    AreEqual(b, 1)
    AreEqual(type(b), Byte)

    bprime = b * 10
    AreEqual(type(b), Byte)

    d = a + 255
    AreEqual(type(b), Byte)

    c = b + 255
    AreEqual(c, 256)
    AreEqual(type(c), int)

Assert(not (20 == False))
Assert(not (20 == None))
Assert(not (False == 20))
Assert(not (None == 20))
Assert(not (20 == 'a'))
Assert(not ('a' == 20))
Assert(not (2.5 == None))
Assert(not (20 == (2,3)))

AreEqual(long(1234793454934), 1234793454934)
AreEqual(4 ** -2, 0.0625)
AreEqual(4L ** -2, 0.0625)

AssertError(ZeroDivisionError, (lambda: (0 ** -1)))
AssertError(ZeroDivisionError, (lambda: (0.0 ** -1)))
AssertError(ZeroDivisionError, (lambda: (0 ** -1.0)))
AssertError(ZeroDivisionError, (lambda: (0.0 ** -1.0)))
AssertError(ZeroDivisionError, (lambda: (False ** -1)))
AssertError(ZeroDivisionError, (lambda: (0L ** -(2 ** 65))))
AssertError(ZeroDivisionError, (lambda: (0j ** -1)))
AssertError(ZeroDivisionError, (lambda: (0j ** 1j)))

def test_dot_net_types():
    from System import Byte, UInt16, UInt32, UInt64, SByte, Int16
    from operator import add, sub, mul, div, mod, and_, or_, xor, floordiv

    list = [-2, -3, -5, 2, 3, 5]

    utypes = [Byte, UInt16, UInt32, UInt64]
    stypes = [SByte, Int16]

    ops = [
        ("add", add),
        ("sub", sub),
        ("mul", mul),
        ("div", div),
        ("floordiv", floordiv),
        ("mod", mod),
        ("and", and_),
        ("or", or_),
        ("xor", xor)
          ]

    total = 0


    def test(ta, tb, a, b, op, expect, got):
        if str(expect) != str(got):
            Fail(" ".join(map(str, [ta, tb, a, b, op, expect, got])))

    for a in list:
        for b in list:
            if a < 0:
                atypes = stypes
            else:
                atypes = stypes + utypes
            if b < 0:
                btypes = stypes
            else:
                btypes = stypes + utypes

            for ta in atypes:
                for tb in btypes:
                    xa = ta.Parse(str(a))
                    xb = tb.Parse(str(b))

                    for name, op in ops:
                        total += 1
                        expect = op(a,b)
                        got = op(xa, xb)

                        test(ta, tb, a, b, name, expect, got)

                        op = getattr(xa, "__" + name + "__")
                        got = op(xb)
                        
                        test(ta, tb, a, b, name, expect, got)

                        op = getattr(xb, "__r" + name + "__")
                        got = op(xa)
                        test(ta, tb, a, b, name, expect, got)

if is_cli:
    test_dot_net_types()