#####################################################################################
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public
# License. A  copy of the license can be found in the License.html file at the
# root of this distribution. If  you cannot locate the  Microsoft Public
# License, please send an email to  dlr@microsoft.com. By using this source
# code in any fashion, you are agreeing to be bound by the terms of the 
# Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#####################################################################################

from lib.assert_util import *
if is_cli:
    from System import Int64, Byte, Int16
    
    
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

AreEqual(pow(2, 1000000000, 2147483647 + 10), 511677409)
AreEqual(pow(2, 2147483647*2147483647, 2147483647*2147483647), 297528129805479806)

for i in range(1, 100, 7):
    l = long(i)
    Assert(type(i) == int)
    Assert(type(l) == long)
    for exp in [17, 2863, 234857, 1435435, 234636554, 2147483647]:
        lexp = long(exp)
        Assert(type(exp) == int)
        Assert(type(lexp) == long)
        for mod in [7, 5293, 23745, 232474276, 534634665, 2147483647]:
            lmod = long(mod)
            Assert(type(mod) == int)
            Assert(type(lmod) == long)

            ir = pow(i, exp, mod)
            lr = pow(l, lexp, lmod)

            AreEqual(ir, lr)
            
            ir = pow(i, 0, mod)
            lr = pow(l, 0L, lmod)

            AreEqual(ir, 1)
            AreEqual(lr, 1)

        AssertError(ValueError, pow, i, exp, 0)
        AssertError(ValueError, pow, l, lexp, 0L)

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
                

def test_conversions():
    AreEqual((3.0).__float__(), 3.0)
    class Foo(float): pass
    
    AreEqual(Foo(3.0).__float__(), 3.0)
    AreEqual(type(Foo(3.0).__float__()), Foo)
       
    AreEqual((3L).__long__(), 3L)
    class Foo(long): pass
    
    AreEqual(Foo(3L).__long__(), 3L)
    AreEqual(type(Foo(3L).__long__()), Foo)                        

    AreEqual((3).__int__(), 3)
    class Foo(int): pass
    
    AreEqual(Foo(3).__int__(), 3)
    AreEqual(type(Foo(3).__int__()), Foo)                        

def test_float_equality():
    class myfloat(float):
        def __eq__(self, other):
            return not float.__eq__(self, other)
            
    AreEqual(myfloat(2) == myfloat(2), False)

test_extensible_math()
test_conversions()
test_float_equality()