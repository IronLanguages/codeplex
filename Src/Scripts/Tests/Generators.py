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

def ifilter(iterable):
    def predicate(x):
        return x % 3
    for x in iterable:
        if predicate(x):
            yield x
def ifilterfalse(iterable):
    def predicate(x):
        return x % 3
    for x in iterable:
        if not predicate(x):
            yield x

ll = [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20]
x = ifilter(ll)
l = []
for i in x: l.append(i)
x = ifilterfalse(ll)
Assert(l == [1,2,4,5,7,8,10,11,13,14,16,17,19,20])
l = []
for i in x: l.append(i)
Assert(l == [3,6,9,12,15,18])

#  Generator expressions

AreEqual(sum(i+i for i in range(100) if i < 50), 2450)
AreEqual(list((i,j) for i in xrange(2) for j in xrange(3)), [(0, 0), (0, 1), (0, 2), (1, 0), (1, 1), (1, 2)])
AreEqual(list((i,j) for i in xrange(2) for j in xrange(i+1)), [(0, 0), (1, 0), (1, 1)])

i = 10
AreEqual(sum(i+i for i in range(1000) if i < 50), 2450)
AreEqual(i, 10)

g = (i+i for i in range(10))
AreEqual(list(g), [0, 2, 4, 6, 8, 10, 12, 14, 16, 18])

g = (i+i for i in range(3))
AreEqual(g.next(), 0)
AreEqual(g.next(), 2)
AreEqual(g.next(), 4)
AssertError(StopIteration, g.next)
AssertError(StopIteration, g.next)
AssertError(StopIteration, g.next)
AreEqual(list(g), [])

def f(n):
    return (i+i for i in range(n) if i < 50)

AreEqual(sum(f(100)), 2450)
AreEqual(list(f(10)), [0, 2, 4, 6, 8, 10, 12, 14, 16, 18])
AreEqual(sum(f(10)), 90)

def f(n):
    return ((i,j) for i in xrange(n) for j in xrange(i))

AreEqual(list(f(3)), [(1, 0), (2, 0), (2, 1)])


# Nested generators

def outergen():
    def innergen():
        yield i
        for j in range(i):
            yield j
    for i in range(10):
        yield (i, innergen())

for a,b in outergen():
    AreEqual(a, b.next())
    AreEqual(range(a), list(b))


def f():
    import sys
    yield "Import inside generator"

AreEqual(f().next(), "Import inside generator")


def xgen():
    try:
        yield 1
    except:
        pass
    else:
        yield 2

AreEqual([ i for i in xgen()], [1,2])


def xgen2(x):
    yield "first"
    try:
        yield "try"
        if x > 3:
            raise AssertionError("x > 10")
        100 / x
        yield "try 2"
    except AssertionError:
        yield "assert"
        yield "assert 2"
    except:
        yield "exc"
        yield "exc 2"
    else:
        yield "else"
        yield "else 2"
    yield "last"

def testxgen2(x, r):
    AreEqual(list(xgen2(x)), r)

testxgen2(0, ['first', 'try', 'exc', 'exc 2', 'last'])
testxgen2(1, ['first', 'try', 'try 2', 'else', 'else 2', 'last'])
testxgen2(2, ['first', 'try', 'try 2', 'else', 'else 2', 'last'])
testxgen2(3, ['first', 'try', 'try 2', 'else', 'else 2', 'last'])
testxgen2(4, ['first', 'try', 'assert', 'assert 2', 'last'])


def xgen3():
    yield "first"
    try:
        pass
    finally:
        yield "fin"
        yield "fin 2"
    yield "last"

AreEqual(list(xgen3()), ['first', 'fin', 'fin 2', 'last'])

AreEqual(type(xgen), type(xgen2))
AreEqual(type(ifilter), type(xgen3))

def f():
    def g():
        def xx():
            return x

        def yy():
            return y

        def zz():
            return z

        def ii():
            return i


        yield xx()
        yield yy()
        yield zz()
        for i in [11, 12, 13]:
            yield ii()
    x = 1
    y = 2
    z = 3

    return g()

AreEqual(list(f()), [1, 2, 3, 11, 12, 13])
