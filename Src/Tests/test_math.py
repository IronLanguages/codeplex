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
                

                        
test_extensible_math()

def mystr(x):
    if isinstance(x, tuple):
        return "(" + ", ".join(mystr(e) for e in x) + ")"
    else:
        s = str(x)
        if s.endswith("L"): return s[:-1]
        else: return s
    
def test_dot_net_types():
    from System import Byte, UInt16, UInt32, UInt64, SByte, Int16
    from operator import add, sub, mul, div, mod, and_, or_, xor, floordiv, truediv, lshift, rshift, neg, pos, abs, invert

    list = [-2, -3, -5, 2, 3, 5]

    utypes = [Byte, UInt16, UInt32, UInt64]
    stypes = [SByte, Int16]

    ops = [
        ("add", add),
        ("sub", sub),
        ("mul", mul),
        ("div", div),
        ("floordiv", floordiv),
        ("truediv", truediv),
        ("mod", mod),
        ("and", and_),
        ("or", or_),
        ("xor", xor),
        ("pow", pow),
        ("lshift", lshift),
        ("rshift", rshift),
        ("divmod", divmod),
        ]
        
    unops = [
        ("neg", neg),
        ("pos", pos),
        ("abs", abs),
          ]

    total = 0

    def get_message(ta, tb, a, b, op, e_s, e_v, g_s, g_v):
        return """
        Math test failed, operation: %(op)s
        Left:
            type:  %(ta)s
            value: %(a)s
        Right:
            type:  %(tb)s
            value: %(b)s
        Expected:
            (%(e_s)s, %(e_v)s)
        Got:
            (%(g_s)s, %(g_v)s)
        """ % {
            'ta'  : str(ta),
            'tb'  : str(tb),
            'a'   : str(a),
            'b'   : str(b),
            'op'  : str(op),
            'e_s' : str(e_s),
            'e_v' : str(e_v),
            'g_s' : str(g_s),
            'g_v' : str(g_v)
        }

    def get_messageun(ta, a, op, e_s, e_v, g_s, g_v):
        return """
        Math test failed, operation: %(op)s
        Operant:
            type:  %(ta)s
            value: %(a)s
        Expected:
            (%(e_s)s, %(e_v)s)
        Got:
            (%(g_s)s, %(g_v)s)
        """ % {
            'ta'  : str(ta),
            'a'   : str(a),
            'op'  : str(op),
            'e_s' : str(e_s),
            'e_v' : str(e_v),
            'g_s' : str(g_s),
            'g_v' : str(g_v)
        }

    def test(ta, tb, a, b, op, (e_s, e_v), (g_s, g_v)):
        Assert(e_s == g_s, get_message(ta, tb, a, b, op, e_s, e_v, g_s, g_v))

        if e_s:
            # same value
            Assert(mystr(e_v) == mystr(g_v), get_message(ta, tb, a, b, op, e_s, e_v, g_s, g_v))
        else:
            # same exception
            Assert(type(e_v) == type(g_v), get_message(ta, tb, a, b, op, e_s, e_v, g_s, g_v))

    def testun(ta, a, name, (e_s, e_v), (g_s, g_v)):
        Assert(e_s == g_s, get_messageun(ta, a, op, e_s, e_v, g_s, g_v))
        if e_s:
            # same value
            Assert(mystr(e_v) == mystr(g_v), get_messageun(ta, a, op, e_s, e_v, g_s, g_v))
        else:
            # unary operator should never fail
            Fail(get_messageun(ta, a, op, e_s, e_v, g_s, g_v))

    def calc2(op, a, b):
        try:
            return True, op(a,b)
        except Exception, e:
            return False, e

    def calc1(op, a):
        try:
            return True, op(a)
        except Exception, e:
            return False, e
            
    def calc0(op):
        try:
            return True, op()
        except Exception, e:
            return False, e

    # binaries
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
                        
                        # print a, b, xa, xb, name, 
                        
                        expect = calc2(op, a, b)        # expect
                        got    = calc2(op, xa, xb)      # got
                        test(ta, tb, a, b, name, expect, got)

                        # print expect, got,

                        op = getattr(xa, "__" + name + "__")
                        got = calc1(op, xb)
                        test(ta, tb, a, b, name, expect, got)

                        # print got,

                        op = getattr(xb, "__r" + name + "__")
                        got = calc1(op, xa)
                        test(ta, tb, a, b, name, expect, got)

                        # print got
                        
    # unaries
    for a in list:
        if a < 0: atypes = stypes
        else: atypes = stypes + utypes
        
        for ta in atypes:
            xa = ta.Parse(str(a))
            
            for name, op in unops:
                total += 1

                expect = calc1(op, a)
                got    = calc1(op, xa)
                testun(ta, a, name, expect, got)

                op = getattr(xa, "__" + name + "__")
                got = calc0(op)
                testun(ta, a, name, expect, got)

    # invert is special 
    for a in list:
        if a < 0: atypes = stypes
        else: atypes = stypes + utypes
        
        for ta in atypes:
            xa = ta.Parse(str(a))
            name, op = "invert", invert
            total += 1
            
            ia = op(xa)
            iia = op(ia)

            # compare xa with invert(invert(xa))
            testun(ta, a, name, (True, xa), (True, iia))
            
            # compare xa and invert(xa) with 0
            got = and_(xa, ia)
            testun(ta, a, name, (True, 0), (True, got))

    print total, "tests ran"

if is_cli:
    test_dot_net_types()
