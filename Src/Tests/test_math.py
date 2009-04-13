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

from iptest.assert_util import *
if is_cli or is_silverlight:
    from System import Int64, Byte, Int16
import math

def test_nonnumeric_multiply():
    Assert("pypypypypy" == 5 * "py")
    Assert("pypypypypy" == "py" * 5)
    Assert("pypypypy" == 2 * "py" * 2)
    
    Assert(['py', 'py', 'py'] == ['py'] * 3)
    Assert(['py', 'py', 'py'] == 3 * ['py'])
    
    Assert(['py', 'py', 'py', 'py', 'py', 'py', 'py', 'py', 'py'] == 3 * ['py'] * 3)


def test_misc():
    Assert(3782452410 > 0)

def test_complex():
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

    if is_cli or is_silverlight: AreEqual((1j) + Int64(), 1j)

    AssertError(TypeError, (lambda:(1+1j)+[]))
    AssertError(TypeError, lambda : type(2j).__dict__['real'].__set__, 2j, 0)

def test_floor_divide():
    AreEqual( (12j//5j), (2+0j))
    AreEqual( (12.0+0j) // (5.0+0j), (2+0j) )
    AreEqual( (12+0j) // (0+3j), 0j)
    AreEqual( (0+12j) // (3+0j), 0j)
    AssertError(TypeError, (lambda:2j // "astring"))
    AssertError(ZeroDivisionError, (lambda:3.0 // (0j)))
    AreEqual ( 3.0 // (2j), 0j)
    AreEqual( 12 // (3j), 0j)
    AreEqual( 25 % (5+3j), (10-9j))

def test_more_complex():
    AreEqual((12+3j)/3L, (4+1j))
    AreEqual(3j - 5L, -5+3j)
    if is_cli or is_silverlight: AreEqual(3j - Int64(), 3j)
    AssertError(TypeError, (lambda:3j-[]))
    if is_cli or is_silverlight: AreEqual(pow(5j, Int64()), (1+0j))
    AreEqual(pow(5j, 0L), (1+0j))
    AssertError(TypeError, (lambda:pow(5j, [])))
    if is_cli or is_silverlight: AreEqual(5j * Int64(), 0)
    AreEqual(5j * 3L, 15j)
    AssertError(TypeError, (lambda:(5j*[])))

def test_pow():
    AreEqual(pow(2, 1000000000, 2147483647 + 10), 511677409)
    AreEqual(pow(2, 2147483647*2147483647, 2147483647*2147483647), 297528129805479806)
    nums = [3, 2.3, (2+1j), (2-1j), 1j, (-1j), 1]
    for x in nums:
        for y in nums:
            z = x ** y

def test_mod_pow():
    for i in range(-100, 100, 7):
        l = long(i)
        Assert(type(i) == int)
        Assert(type(l) == long)
        for exp in [1, 17, 2863, 234857, 1435435, 234636554, 2147483647]:
            lexp = long(exp)
            Assert(type(exp) == int)
            Assert(type(lexp) == long)
            for mod in [-7, -5293, -2147483647, 7, 5293, 23745, 232474276, 534634665, 2147483647]:
                lmod = long(mod)
                Assert(type(mod) == int)
                Assert(type(lmod) == long)
    
                ir = pow(i, exp, mod)
                lr = pow(l, lexp, lmod)
    
                AreEqual(ir, lr)
                
                for zero in [0, 0L]:
                    ir = pow(i, zero, mod)
                    lr = pow(l, zero, lmod)
                    
                    if mod > 0:
                        AreEqual(ir, 1)
                        AreEqual(lr, 1)
                    else:
                        AreEqual(ir, mod+1)
                        AreEqual(lr, mod+1)
            AssertError(ValueError, pow, i, exp, 0)
            AssertError(ValueError, pow, l, lexp, 0L)
    
        
        for exp in [0, 0L]:
            for mod in [-1,1,-1L,1L]:
                ir = pow(i, exp, mod)
                lr = pow(l, exp, mod)
                AreEqual(ir, 0)
                AreEqual(lr, 0)

def test_user_ops():
    class powtest:
        def __pow__(self, exp, mod = None):
            return ("powtest.__pow__", exp, mod)
        def __rpow__(self, exp):
            return ("powtest.__rpow__", exp)
    
    AreEqual(pow(powtest(), 1, 2), ("powtest.__pow__", 1, 2))
    AreEqual(pow(powtest(), 3), ("powtest.__pow__", 3, None))
    AreEqual(powtest() ** 4, ("powtest.__pow__", 4, None))
    AreEqual(5 ** powtest(), ("powtest.__rpow__", 5))
    AreEqual(pow(7, powtest()), ("powtest.__rpow__", 7))
    AssertError(TypeError, pow, 1, powtest(), 7)
    
    # Extensible Float tests
    class XFloat(float): pass
    
    AreEqual(XFloat(3.14), 3.14)
    Assert(XFloat(3.14) < 4.0)
    Assert(XFloat(3.14) > 3.0)
    Assert(XFloat(3.14) < XFloat(4.0))
    Assert(XFloat(3.14) > XFloat(3.0))
    
    Assert(0xabcdef01 + (0xabcdef01<<32)+(0xabcdef01<<64) == 0xabcdef01abcdef01abcdef01)

def test_rounding():
    Assert(round(-5.5489) == (-6.0))
    Assert(round(5.5519) == (6.0))
    Assert(round(-5.5) == (-6.0))
    Assert(round(-5.0) == (-5.0))
    
    Assert(round(-4.5) == (-5.0))
    Assert(round(-2.5) == (-3.0))
    Assert(round(-0.5) == (-1.0))
    Assert(round(0.5) == (1.0))
    Assert(round(2.5) == (3.0))
    Assert(round(4.5) == (5.0))
        
    Assert(round(-4.0) == (-4.0))
    Assert(round(-3.5) == (-4.0))
    Assert(round(-3.0) == (-3.0))
    Assert(round(-2.0) == (-2.0))
    Assert(round(-1.5) == (-2.0))
    Assert(round(-1.0) == (-1.0))
    Assert(round(0.0) == (0.0))
    Assert(round(1.0) == (1.0))
    Assert(round(1.5) == (2.0))
    Assert(round(2.0) == (2.0))
    Assert(round(3.0) == (3.0))
    Assert(round(3.5) == (4.0))
    Assert(round(4.0) == (4.0))
    Assert(round(5.0) == (5.0))
    
    # two parameter round overload
    Assert(round(-4.0, 0) == (-4.0))
    Assert(round(-3.5, 0) == (-4.0))
    Assert(round(-3.0, 0) == (-3.0))
    Assert(round(-2.0, 0) == (-2.0))
    Assert(round(-1.5, 0) == (-2.0))
    Assert(round(-1.0, 0) == (-1.0))
    Assert(round(0.0, 0) == (0.0))
    Assert(round(1.0, 0) == (1.0))
    Assert(round(1.5, 0) == (2.0))
    Assert(round(2.0, 0) == (2.0))
    Assert(round(3.0, 0) == (3.0))
    Assert(round(3.5, 0) == (4.0))
    Assert(round(4.0, 0) == (4.0))
    Assert(round(5.0, 0) == (5.0))
    Assert(round(123.41526375, 1) == 123.4)
    Assert(round(123.41526375, 2) == 123.42)
    Assert(round(123.41526375, 3) == 123.415)
    Assert(round(123.41526375, 4) == 123.4153)
    Assert(round(123.41526375, 5) == 123.41526)
    Assert(round(123.41526375, 6) == 123.415264)
    Assert(round(123.41526375, 7) == 123.4152638)
    Assert(round(-123.41526375, 1) == -123.4)
    Assert(round(-123.41526375, 2) == -123.42)
    Assert(round(-123.41526375, 3) == -123.415)
    Assert(round(-123.41526375, 4) == -123.4153)
    Assert(round(-123.41526375, 5) == -123.41526)
    Assert(round(-123.41526375, 6) == -123.415264)
    Assert(round(-123.41526375, 7) == -123.4152638)
    for i in xrange(8, 307):
        # Note: We can't do exact equality here due to the inexact nature of IEEE
        # double precision floats when multiplied and later divided by huge powers of 10.
        # Neither CPython nor IronPython mantain exact equality for precisions >= 17
        if i < 17:
            Assert(round(123.41526375, i) == 123.41526375)
            Assert(round(-123.41526375, i) == -123.41526375)
        else:
            Assert(abs(round(123.41526375, i) - 123.41526375) < 0.0000000001)
            Assert(abs(round(-123.41526375, i) - -123.41526375) < 0.0000000001)
            
    Assert(round(7182930456.0, -1) == 7182930460.0)
    Assert(round(7182930456.0, -2) == 7182930500.0)
    Assert(round(7182930456.0, -3) == 7182930000.0)
    Assert(round(7182930456.0, -4) == 7182930000.0)
    Assert(round(7182930456.0, -5) == 7182900000.0)
    Assert(round(7182930456.0, -6) == 7183000000.0)
    Assert(round(7182930456.0, -7) == 7180000000.0)
    Assert(round(7182930456.0, -8) == 7200000000.0)
    Assert(round(7182930456.0, -9) == 7000000000.0)
    Assert(round(7182930456.0, -10) == 10000000000.0)
    Assert(round(7182930456.0, -11) == 0.0)
    Assert(round(-7182930456.0, -1) == -7182930460.0)
    Assert(round(-7182930456.0, -2) == -7182930500.0)
    Assert(round(-7182930456.0, -3) == -7182930000.0)
    Assert(round(-7182930456.0, -4) == -7182930000.0)
    Assert(round(-7182930456.0, -5) == -7182900000.0)
    Assert(round(-7182930456.0, -6) == -7183000000.0)
    Assert(round(-7182930456.0, -7) == -7180000000.0)
    Assert(round(-7182930456.0, -8) == -7200000000.0)
    Assert(round(-7182930456.0, -9) == -7000000000.0)
    Assert(round(-7182930456.0, -10) == -10000000000.0)
    Assert(round(-7182930456.0, -11) == 0.0)
    for i in xrange(-12, -309, -1):
        Assert(round(7182930456.0, i) == 0.0)
        Assert(round(-7182930456.0, i) == 0.0)

def test_other():
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
    if is_cli or is_silverlight:
        AssertError(ValueError, (lambda: 10 ** 735293857239475))

    Assert(2 ** 3.0 == 8.0)
    Assert(2.0 ** 3 == 8.0)
    Assert(4 ** 0.5 == 2.0)

def test_divmod():
    AreEqual(7.1//2.1, 3.0)
    AreEqual(divmod(5.0, 2), (2.0,1.0))
    AreEqual(divmod(5,2), (2,1))
    AreEqual(divmod(-5,2), (-3,1))

def test_boolean():
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

@skip("win32")
def test_byte():
    a = Byte()
    AreEqual(type(a), Byte)
    AreEqual(a, 0)

    b = a + Byte(1)
    AreEqual(b, 1)
    AreEqual(type(b), Byte)

    bprime = b * Byte(10)
    AreEqual(type(bprime), Byte)

    d = a + Byte(255)
    AreEqual(type(d), Byte)

    c = b + Byte(255)
    AreEqual(c, 256)
    AreEqual(type(c), Int16)

def test_negated_comparisons():
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

def test_zero_division():
    AssertError(ZeroDivisionError, (lambda: (0 ** -1)))
    AssertError(ZeroDivisionError, (lambda: (0.0 ** -1)))
    AssertError(ZeroDivisionError, (lambda: (0 ** -1.0)))
    AssertError(ZeroDivisionError, (lambda: (0.0 ** -1.0)))
    AssertError(ZeroDivisionError, (lambda: (False ** -1)))
    AssertError(ZeroDivisionError, (lambda: (0L ** -(2 ** 65))))
    AssertError(ZeroDivisionError, (lambda: (0j ** -1)))
    AssertError(ZeroDivisionError, (lambda: (0j ** 1j)))

def test_extensible_math():
    operators = ['__add__', '__sub__', '__pow__', '__mul__', '__div__', '__floordiv__', '__truediv__', '__mod__']
    opSymbol  = ['+',       '-',       '**',      '*',       '/',       '//',           '/',           '%']
    
    types = []
    for baseType in [(int, (100,2)), (long, (100L, 2L)), (float, (100.0, 2.0))]:
    # (complex, (100+0j, 2+0j)) - cpython doesn't call reverse ops for complex ?
        class prototype(baseType[0]):
            for op in operators:
                exec '''def %s(self, other):
    global opCalled
    opCalled.append('%s')
    return super(self.__class__, self).%s(other)

def %s(self, other):
    global opCalled
    opCalled.append('%s')
    return super(self.__class__, self).%s(other)''' % (op, op, op, op[:2] + 'r' + op[2:], op[:2] + 'r' + op[2:], op[:2] + 'r' + op[2:])
        
        types.append( (prototype, baseType[1]) )
    
    global opCalled
    opCalled = []
    for op in opSymbol:
            for typeInfo in types:
                ex = typeInfo[0](typeInfo[1][0])
                ey = typeInfo[0](typeInfo[1][1])
                nx = typeInfo[0].__bases__[0](typeInfo[1][0])
                ny = typeInfo[0].__bases__[0](typeInfo[1][1])
                                
                #print 'nx %s ey' % op, type(nx), type(ey)
                res1 = eval('nx %s ey' % op)
                res2 = eval('nx %s ny' % op)
                AreEqual(res1, res2)
                AreEqual(len(opCalled), 1)
                opCalled = []
                
                #print 'ex %s ny' % op, type(ex), type(ny)
                res1 = eval('ex %s ny' % op)
                res2 = eval('nx %s ny' % op)
                AreEqual(res1, res2)
                AreEqual(len(opCalled), 1)
                opCalled = []
                

def test_nan():
    x  = 1e66666
    Assert(x==x)
    Assert(x<=x)
    Assert(x>=x)
    Assert(not x!=x)
    Assert(not x<x)
    Assert(not x>x)
    AreEqual(cmp(x, x), 0)
    
    y = x/x
    AreEqual(y == y, False)
    AreEqual(y >= y, False)
    AreEqual(y <= y, False)
    AreEqual(y != y, True)
    AreEqual(y > y, False)
    AreEqual(y < y, False)
    AreEqual(cmp(y, y), 0)
    
    Assert(not x==y)
    Assert(x!=y)
    Assert(not x>=y)
    Assert(not x<=y)
    Assert(not x<y)
    Assert(not x>y)
    #CodePlex 17517
    #AreEqual(cmp(x, y), 1)

def test_long_log():
    """logon big ints should work"""
    AreEqual(round(math.log10(10 ** 1000), 5), 1000.0)
    AreEqual(round(math.log(10 ** 1000), 5), 2302.58509)
    
    AreEqual(round(math.log10(18446744073709551615), 5),  19.26592)
    AreEqual(round(math.log(18446744073709551615), 5), 44.36142)

    AreEqual(round(math.log10(18446744073709551616), 5),  19.26592)
    AreEqual(round(math.log(18446744073709551616), 5), 44.36142)

    AreEqual(round(math.log10(18446744073709551614), 5),  19.26592)
    AreEqual(round(math.log(18446744073709551614), 5), 44.36142)
    
    # log in a new base
    AreEqual(round(math.log(2 ** 1000, 2), 5), 1000.0)
    
    AssertError(ValueError, math.log, 0L)
    AssertError(ValueError, math.log, -1L)
    AreEqual(math.log(2L, 1e666), 0.0)
    AssertError(ValueError, math.log, 2L, -1e666)
    AssertError(ValueError, math.log, 1L, 0.0)
    AssertError(ValueError, math.log, 2L, 0.0)
    AssertError(ZeroDivisionError, math.log, 2L, 1.0)

    #Make sure that an object is converted to float before being passed into log funcs
    class N(object):
        def __float__(self):
            return 10.0
        def __long__(self):
		    return 100
		    
    AreEqual(round(math.log10(N()), 5),1.0)
    AreEqual(round(math.log(N()), 5),2.30259)
    
def test_float_26():
    from_hex_tests = [('1.fffffffffffff7', 1.9999999999999998),
                      ('1.fffffffffffff8', 2.0),
                      ('-1.1fffffffffffffffffffffffffffffffff', -1.125),
                      ('-1.ffffffffffffffffffffffffffffffffff', -2),
                      ('10.4', 16.25),
                      ('1.fffffffffffffp1023', 1.7976931348623157e+308),
                      ('1p1', 2.0),
                      ('-1.0p1', -2.0),
                      ('+1.0p1', 2.0),
                      ('-0x1.0p1', -2.0),
                      ('1.0p1023', 8.9884656743115795e+307),
                      ('1.0p-1023', 1.1125369292536007e-308),
                      ('1.1234p1', 2.1422119140625),
                      ('1.' + 'f'*1000+ 'p1', 4.0),
                      ('1.ap1', 3.25),
                      ('1.Ap1', 3.25),
                      ('1.0p-1', 0.5),
                      ('1.0p-1', 0.5),
                      ('1.0p-1074', 4.9406564584124654e-324),
                      ('2.0p-1075', 4.9406564584124654e-324),
                      ('1.0p-1075', 0),
                      ('10.0p-1075', 3.9525251667299724e-323),
                      ('0xaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.a', 1.634661910256948e+55),
                      ('0x1.Ap1', 3.25),
                      ('0x1.Ap2', 6.5),
                      ('0x1.Ap3', 13),
                      ('  0x1.Ap1   ', 3.25),
                      ]
                      
    for string, value in from_hex_tests:
        #print string, value
        AreEqual(float.fromhex(string), value)
    
    from_hex_errors = [(OverflowError, '1.0p1024'), 
                       (OverflowError, '1.0p1025'), 
                       (OverflowError, '10.0p1023'),
                       (OverflowError, '1.ffffffffffffffp1023'),
                       (ValueError, 'xxxx'), 
                       (OverflowError, '1.0p99999999999999999999999')]
    for excep, error in from_hex_errors:
        AssertError(excep, float.fromhex, error)
    

def test_integer_ratio():
    int_ratio_tests = [ (2.5, (5, 2)), (1.3, (5854679515581645L, 4503599627370496L))]
    
    for flt, res in int_ratio_tests:
        AreEqual(flt.as_integer_ratio(), res)

run_test(__name__)
