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


###############################
# test array

if is_cli:
    import System

    array1 = System.Array.CreateInstance(int, 2)
    for i in range(2): array1[i] = i * 10
        
    array2 = System.Array.CreateInstance(int, 4)
    for i in range(2, 6): array2[i - 2] = i * 10

    array3 = System.Array.CreateInstance(float, 3)
    array3[0] = 2.1
    array3[1] = 3.14
    array3[2] = 0.11
    System.Array.__setitem__(array3, 2, 0.14)
    AreEqual(System.Array.__getitem__(array3, 1), 3.14)
    AreEqual([x for x in System.Array.__getitem__(array3, slice(2))], [2.1, 3.14])

    array4 = System.Array.CreateInstance(int, 2, 2)
    array4[0, 1] = 1
    AreEqual(repr(array4), "System.Int32[,](\n0, 1\n0, 0)")

    array5 = System.Array.CreateInstance(object, 2, 2, 2)
    array5[0, 1, 1] = int
    AreEqual(repr(array5), "System.Object[,,]( Multi-dimensional array )")

    AssertError(TypeError, lambda : array5['s'])
    def f1(): array5[0, 1] = 0
    AssertError(ValueError, f1)
    def f2(): array5['s'] = 0
    AssertError(TypeError, f2)

    for f in (
        lambda a, b : System.Array.__add__(a, b), 
        lambda a, b : a + b
        ) : 
        
        temp = System.Array.__add__(array1, array2)
        result = f(array1, array2)
        
        for i in range(6): AreEqual(i * 10, result[i])
        AreEqual(repr(result), "System.Int32[](0, 10, 20, 30, 40, 50)")
        
        result = f(array1, array3)
        AreEqual(len(result), 2 + 3)
        AreEqual([x for x in result], [0, 10, 2.1, 3.14, 0.14])
        
        AssertError(NotImplementedError, f, array1, array4)
        
    for f in [
        lambda a, x: System.Array.__mul__(a, x), 
        lambda a, x: array1 * x
        ]:

        AreEqual([x for x in f(array1, 4)], [0, 10, 0, 10, 0, 10, 0, 10])
        AreEqual([x for x in f(array1, 5)], [0, 10, 0, 10, 0, 10, 0, 10, 0, 10])
        AreEqual([x for x in f(array1, 0)], [])
        AreEqual([x for x in f(array1, -10)], [])

    ## slice fun
    array1 = System.Array.CreateInstance(int, 20)
    for i in range(20): array1[i] = i * i
    array1[::2] = [x * 2 for x in range(10)]

    for i in range(0, 20, 2):
        AreEqual(array1[i], i) 
    for i in range(1, 20, 2):
        AreEqual(array1[i], i * i) 

    def f(): array1[::2] = [x * 2 for x in range(11)]
    AssertError(ValueError, f)    
    
    ## creation
    t = System.Array
    ti = type(System.Array.CreateInstance(int, 1))

    AssertError(TypeError, t, [1, 2])
    for x in (ti([1,2]), t[int]([1,2]), ti([1.5, 2.3])):
        AreEqual([i for i in x], [1,2])
        t.Reverse(x)
        AreEqual([i for i in x], [2, 1])

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