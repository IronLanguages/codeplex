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
    load_iron_python_test()
    from IronPythonTest import *

if is_cli:
    import clr
    clr.AddReferenceByPartialName("System.Drawing")
    from System.Drawing import Point, Size, PointF, SizeF, Rectangle, RectangleF

    x = Point()
    Assert(x == Point(0,0))
    x = Size()
    Assert(x == Size(0,0))
    x = PointF()
    Assert(x == PointF(0,0))
    x = SizeF()
    Assert(x == SizeF(0,0))
    x = Rectangle()
    Assert(x == Rectangle(0,0,0,0))
    x = RectangleF()
    Assert(x == RectangleF(0,0,0,0))

    p = Point(3,4)
    s = Size(2,9)

    q = p + s
    Assert(q == Point(5,13))
    Assert(q != Point(13,5))
    q = p - s
    Assert(q == Point(1,-5))
    Assert(q != Point(0,4))
    q += s
    Assert(q == Point(3,4))
    Assert(q != Point(2,4))
    q -= Size(1,2)
    Assert(q == Point(2,2))
    Assert(q != Point(1))

    t = s
    Assert(t == s)
    Assert(t != s - Size(1,0))
    t += Size(3,1)
    Assert(t == Size(5,10))
    Assert(t != Size(5,0))
    t -= Size(2,8)
    Assert(t == Size(3,2))
    Assert(t != Size(0,2))
    t = s + Size(-1,-2)
    Assert(t == Size(1,7))
    Assert(t != Size(1,5))
    t = s - Size(1,2)
    Assert(t == Size(1,7))
    Assert(t != Size(1,3))

    def weekdays(enum):
        return enum.Mon|enum.Tue|enum.Wed|enum.Thu|enum.Fri

    def weekend(enum):
        return enum.Sat|enum.Sun

    def TestEnum(enum):
        days = [enum.Mon,enum.Tue,enum.Wed,enum.Thu,enum.Fri,enum.Sat,enum.Sun]
        x = enum.Mon|enum.Tue|enum.Wed|enum.Thu|enum.Fri|enum.Sat|enum.Sun
        y = enum.Mon
        for day in days:
            y |= day
        Assert(x == y)
        Assert((x <> y) == False)
        if x == y:  # EqualRetBool
            b = True
        else :
            b = False
        Assert(b)
        
        Assert(x == weekdays(enum)|weekend(enum))
        Assert(x == (weekdays(enum)^weekend(enum)))
        Assert((weekdays(enum)&weekend(enum)) == enum.None)
        Assert(weekdays(enum) == enum.Weekdays)
        Assert(weekend(enum) == enum.Weekend)
        Assert(weekdays(enum) != enum.Weekend)
        Assert(weekdays(enum) != weekend(enum))
        
    for e in [DaysInt, DaysShort, DaysLong, DaysSByte, DaysByte, DaysUShort, DaysUInt, DaysULong]:
        TestEnum(e)

    for e in [DaysInt, DaysShort, DaysLong, DaysSByte]:
        import operator
        z = operator.inv(e.Mon)
        AreEqual(type(z), e)
        AreEqual(z.ToString(), "-2")

    for (e, v) in [ (DaysByte,254), (DaysUShort,65534), (DaysUInt,4294967294), (DaysULong,18446744073709551614) ]:
        import operator
        z = operator.inv(e.Mon)
        AreEqual(type(z), e)
        AreEqual(z.ToString(), str(v))

    AssertError(ValueError, lambda: DaysInt.Mon == DaysShort.Mon)
    AssertError(ValueError, lambda: DaysInt.Mon & DaysShort.Mon)
    AssertError(ValueError, lambda: DaysInt.Mon | DaysShort.Mon)
    AssertError(ValueError, lambda: DaysInt.Mon ^ DaysShort.Mon)
    AssertError(ValueError, lambda: DaysInt.Mon == 1)
    AssertError(ValueError, lambda: DaysInt.Mon & 1)
    AssertError(ValueError, lambda: DaysInt.Mon | 1)
    AssertError(ValueError, lambda: DaysInt.Mon ^ 1)

    def f():
        if DaysInt.Mon == DaysShort.Mon: return True
    AssertError(ValueError, f)
    
    Assert(not DaysInt.Mon == None)
    Assert(DaysInt.Mon != None)
    
import operator

x = ['a','b','c','d']
g = operator.itemgetter(2)
AreEqual(g(x), 'c')

class C:
    a = 10
g = operator.attrgetter("a")
AreEqual(g(C), 10)
AreEqual(g(C()), 10)

a = { 'k' : 'v' }
g = operator.itemgetter('x')
AssertError(KeyError, g, a)

x = True
AreEqual(x, True)
AreEqual(not x, False)
x = False
AreEqual(x, False)
AreEqual(not x, True)


class C:
    def func(self):
           pass

a = C.func
b = C.func
AreEqual(a, b)

c = C()
a = c.func
b = c.func
AreEqual(a, b)


########################
# string multiplication

class foo(int): pass

fooInst = foo(3)

AreEqual('aaa', 'a' * 3)
AreEqual('aaa', 'a' * 3L)
AreEqual('aaa', 'a' * fooInst)

AreEqual('', 'a' * False)
AreEqual('a', 'a' * True)


###############################
# equals overloading semantics

class CustomEqual:
    def __eq__(self, other):
        return 7
 
AreEqual((CustomEqual() == 1), 7)


# Test binary operators for all numeric types and types inherited from them

class myint(int): pass
class mylong(long): pass
class myfloat(float): pass
class mycomplex(complex): pass

l = [2, 10L, (1+2j), 3.4, myint(7), mylong(5), myfloat(2.32), mycomplex(3, 2), True]

if is_cli:
    import System
    l.append(System.Int64.Parse("5"))

def add(a, b): return a + b
def sub(a, b): return a - b
def mul(a, b): return a * b
def div(a, b): return a / b
def mod(a, b): return a % b
def truediv(a,b): return a / b
def floordiv(a,b): return a // b
def pow(a,b): return a ** b

op = [
 ('+', add, True),
 ('-', sub, True),
 ('*', mul, True),
 ('/', div, True),
 ('%', mod, False),
 ('//', floordiv, False),
 ('**', pow, True)
]

for a in l:
    for b in l:
        for sym, fnc, cmp in op:
            if cmp or (not isinstance(a, complex) and not isinstance(b, complex)):
                try:
                    r = fnc(a,b)
                except:
                    (exc_type, exc_value, exc_traceback) = sys.exc_info()
                    Fail("Binary operator failed: %s, %s: %s %s %s (Message=%s)" % (type(a).__name__, type(b).__name__, str(a), sym, str(b), str(exc_value)))


threes = [ 3, 3L, 3.0 ]
zeroes = [ 0, 0L, 0.0 ]

if is_cli:
    import System
    threes.append(System.Int64.Parse("3"))
    zeroes.append(System.Int64.Parse("0"))

for i in threes:
    for j in zeroes:
        for fnc in [div, mod, truediv, floordiv]:
            try:
                r = fnc(i, j)
            except ZeroDivisionError:
                pass
            else:
                (exc_type, exc_value, exc_traceback) = sys.exc_info()
                Fail("Didn't get ZeroDivisionError %s, %s, %s, %s, %s (Message=%s)" % (str(func), type(i).__name__, type(j).__name__, str(i), str(j), str(exc_value)))

if is_cli: 
    unary = UnaryClass(9)
    AreEqual(-(unary.value), (-unary).value)
    AreEqual(~(unary.value), (~unary).value)

# testing customized unary op 
class C1:
    def __pos__(self):
        return -10
    def __neg__(self):
        return 10
    def __invert__(self): 
        return 20
    def __abs__(self): 
        return 30        

class C2(object): 
    def __pos__(self):
        return -10
    def __neg__(self):
        return 10
    def __invert__(self): 
        return 20
    def __abs__(self): 
        return 30        

for x in C1(), C2(): 
    AreEqual(+x, -10)
    AreEqual(-x, 10)
    AreEqual(~x, 20)
    AreEqual(abs(x), 30)


    
#***** Above code are from 'Operators' *****

# object identity of booleans - __ne__ should return "True" or "False", not a new boxed bool

AreEqual(id(complex.__ne__(1+1j, 1+1j)), id(False))
AreEqual(id(complex.__ne__(1+1j, 1+2j)), id(True))


