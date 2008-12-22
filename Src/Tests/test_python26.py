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

from __future__ import print_function
from __future__ import unicode_literals

from iptest.assert_util import *


def test_class_decorators():
    global called
    called = False
    def f(x): 
        global called
        called = True
        return x

    @f
    class x(object): pass
    
    AreEqual(called, True)
    AreEqual(type(x), type)

    called = False
    
    @f
    def abc(): pass
    
    AreEqual(called, True)
    
    def f(*args):
        def g(x):
            return x
        global called
        called = args
        return g
    
    
    @f('foo', 'bar')
    class x(object): pass
    AreEqual(called, ('foo', 'bar'))
    AreEqual(type(x), type)

    @f('foo', 'bar')
    def x(): pass
    AreEqual(called, ('foo', 'bar'))
    
    def f():
        exec """@f\nif True: pass\n"""
    AssertError(SyntaxError, f)

def test_binary_numbers():
    AreEqual(0b01, 1)
    AreEqual(0b10, 2)
    AreEqual(0b100000000000000000000000000000000, 4294967296)

    AreEqual(0b01L, 1)
    AreEqual(0b10L, 2)
    AreEqual(0b100000000000000000000000000000000L, 4294967296)

    AreEqual(type(0b01), int)
    AreEqual(type(0b10), int)
    AreEqual(type(0b01111111111111111111111111111111), int)
    AreEqual(type(0b10000000000000000000000000000000), long)
    AreEqual(type(-0b01111111111111111111111111111111), int)
    AreEqual(type(-0b10000000000000000000000000000000), int)
    AreEqual(type(-0b10000000000000000000000000000001), long)
    AreEqual(type(0b00000000000000000000000000000000000001), int)

    AreEqual(type(0b01L), long)
    AreEqual(type(0b10L), long)
    AreEqual(type(0b01l), long)
    AreEqual(type(0b10l), long)
    AreEqual(type(0b100000000000000000000000000000000L), long)
    AreEqual(0b01, 0B01)

def test_print_function():
    AssertError(TypeError, print, sep = 42)
    AssertError(TypeError, print, end = 42)
    AssertError(TypeError, print, abc = 42)
    AssertError(AttributeError, print, file = 42)
    
    import sys
    oldout = sys.stdout
    sys.stdout = file('abc.txt', 'w')
    try:
        print('foo')
        print('foo', end = 'abc')
        print()
        print('foo', 'bar', sep = 'abc')        
        
        sys.stdout.close()        
        f = file('abc.txt')
        AreEqual(f.readlines(), ['foo\n', 'fooabc\n', 'fooabcbar\n'])
    finally:
        sys.stdout = oldout

    class myfile(object):
        def __init__(self):
            self.text = ''
        def write(self, text):
            self.text += text
    
    sys.stdout = myfile()
    try:
        print('foo')
        print('foo', end = 'abc')
        print()
        print('foo', 'bar', sep = 'abc')        

        AreEqual(sys.stdout.text, 'foo\n' + 'fooabc\n' + 'fooabcbar\n')
    finally:
        sys.stdout = oldout


def test_user_mappings():
    # **args should support arbitrary mapping types
    class ms(str): pass
    
    class x(object):
        def __getitem__(self, key):
            #print('gi', key, type(key))
            return str('abc')
        def keys(self): 
            return [str('a'), str('b'), str('c'), ms('foo')]
    
    def f(**args): return args
    f(**x())
    l = [(k,v, type(k), type(v)) for k,v in f(**x()).iteritems()]

    def mycmp(a, b):
        r = cmp(str(a[0]), str(b[0]))
        return r
    l.sort(cmp = mycmp)
    
    AreEqual(l, [(str('a'), str('abc'), str, str), (str('b'), str('abc'), str, str), (str('c'), str('abc'), str, str), (str('foo'), str('abc'), ms, str), ])
    
    class y(object):
        def __getitem__(self, key):
                return lambda x:x
        def keys(self): 
            return ['key']
    
    max([2,3,4], **y())
    
    class y(object):
        def __getitem__(self, key):
                return lambda x:x
        def keys(self): 
            return [ms('key')]
    
    
    max([2,3,4], **y())

def test_type_subclasscheck():
    global called
    called = []
    class metatype(type):
        def __subclasscheck__(self, sub):
            called.append((self, sub))
            return True
            
    class myclass(object): __metaclass__ = metatype
    
    AreEqual(issubclass(int, myclass), True)
    AreEqual(called, [(myclass, int)])
    called = []
    
    AreEqual(isinstance(myclass(), int), False)
    AreEqual(called, [])
    
def test_type_instancecheck():
    global called
    called = []
    class metatype(type):
        def __instancecheck__(self, inst):
            called.append((self, inst))
            return True
            
    class myclass(object): __metaclass__ = metatype

    AreEqual(isinstance(4, myclass), True)
    AreEqual(called, [(myclass, 4)])

def test_complex():
    strs = ["5", "2.61e-5", "3e+010", "1.40e09"]
    vals = [5, 2.61e-5, 3e10, 1.4e9]
    
    strings = ["j", "+j", "-j"] + strs + map(lambda x: x + "j", strs)
    values = [1j, 1j, -1j] + vals + map(lambda x: x * 1j, vals)
    
    for s0,v0 in zip(strs, vals):
        for s1,v1 in zip(strs, vals):
            for sign,mult in [("+",1), ("-",-1)]:
                newstrs = [s0+sign+s1+"j", s1+sign+s0+"j", s0+"j"+sign+s1, s1+"j"+sign+s0]
                newvals = [complex(v0,v1*mult), complex(v1,v0*mult), complex(v1*mult,v0), complex(v0*mult,v1)]
                strings += newstrs
                strings += map(lambda x: "(" + x + ")", newstrs)
                values += newvals * 2
    
    for s,v in zip(strings, values):
        AreEqual(complex(s), v)
        AreEqual(v, complex(v.__repr__()))

def test_deque():
    from collections import deque

    # make sure __init__ clears existing contents
    x = deque([6,7,8,9])
    x.__init__(deque([1,2,3]))
    AreEqual(x, deque([1,2,3]))
    x.__init__()
    AreEqual(x, deque())

    # test functionality with maxlen
    x = deque(maxlen=5)
    for i in xrange(5):
        x.append(i)
        AreEqual(x, deque(range(i+1)))
    x.append(5)
    AreEqual(x, deque([1,2,3,4,5]))
    x.appendleft(100)
    AreEqual(x, deque([100,1,2,3,4]))
    x.extend(range(10))
    AreEqual(x, deque([5,6,7,8,9]))
    x.extendleft(range(10,20))
    AreEqual(x, deque([19,18,17,16,15]))
    x.remove(19)
    AreEqual(x, deque([18,17,16,15]))
    x.rotate()
    AreEqual(x, deque([15,18,17,16]))
    x.rotate(-8)
    AreEqual(x, deque([15,18,17,16]))
    x.pop()
    AreEqual(x, deque([15,18,17]))
    x.rotate(-1)
    AreEqual(x, deque([18,17,15]))
    x.popleft()
    AreEqual(x, deque([17,15]))
    x.extendleft(range(4))
    AreEqual(x, deque([3,2,1,0,17]))
    x.rotate(3)
    AreEqual(x, deque([1,0,17,3,2]))
    x.extend(range(3))
    AreEqual(x, deque([3,2,0,1,2]))
    y = x.__copy__()
    AreEqual(x, y)
    x.extend(range(4))
    y.extend(range(4))
    AreEqual(x, y)

def test_set_multiarg():
    from iptest.type_util import myset, myfrozenset
    
    s1 = [2, 4, 5]
    s2 = [4, 7, 9, 10]
    s3 = [2, 4, 5, 6]
    
    for A in (set, myset):
        for B in (set, frozenset, myset, myfrozenset):
            as1, as2, as3 = A(s1), A(s2), A(s3)
            bs1, bs2, bs3 = B(s1), B(s2), B(s3)
            
            AreEqual(as1.union(as2, as3), A([2, 4, 5, 6, 7, 9, 10]))
            AreEqual(as1.intersection(as2, as3), A([4]))
            AreEqual(as2.difference(as3, A([2, 7, 8])), A([9, 10]))
            
            AreEqual(bs1.union(as2, as3), A([2, 4, 5, 6, 7, 9, 10]))
            AreEqual(bs1.intersection(as2, as3), A([4]))
            AreEqual(bs2.difference(as3, A([2, 7, 8])), A([9, 10]))
            
            AreEqual(as1.union(bs2, as3), A([2, 4, 5, 6, 7, 9, 10]))
            AreEqual(as1.intersection(as2, bs3), A([4]))
            AreEqual(as2.difference(as3, B([2, 7, 8])), A([9, 10]))
            
            as1.update(as2, bs3)
            AreEqual(as1, B([2, 4, 5, 6, 7, 9, 10]))
            as2.difference_update(bs3, A([2, 7, 8]))
            AreEqual(as2, A([9, 10]))
            as3.intersection_update(bs2, bs1)
            AreEqual(as3, B([4]))

def test_attrgetter():
    import operator
    
    tests = ['abc', 3, ['d','e','f'], (1,4,9)]
    
    get0 = operator.attrgetter('__class__.__name__')
    get1 = operator.attrgetter('__class__..')
    get2 = operator.attrgetter('__class__..__name__')
    get3 = operator.attrgetter('__class__.__name__.__class__.__name__')
    
    for x in tests:
        AreEqual(x.__class__.__name__, get0(x))
        AssertError(AttributeError, get1, x)
        AssertError(AttributeError, get2, x)
        AreEqual('str', get3(x))

def test_im_aliases():
    def func(): pass
    class foo(object):
        def f(self): pass
    class bar(foo):
        def g(self): pass
    class yak(foo):
        def f(self): pass
    
    a = foo()
    b = foo()
    c = bar()
    d = bar()
    e = yak()
    f = yak()
    
    fs = [foo.f, a.f, b.f, c.f, d.f]
    gs = [bar.g, c.g, d.g]
    f2s = [yak.f, e.f, f.f]
    all = fs + gs + f2s
    
    for x in all:
        AreEqual(x.im_self, x.__self__)
        AreEqual(x.im_func, x.__func__)
    
    for r in [fs, f2s]:
        for s in [fs, f2s]:
            if r == s:
                for x in r:
                    if x.__func__ != None:
                        for y in r:
                            AreEqual(x.__func__, y.__func__)
            else:
                for x in r:
                    if x.__func__ != None and x.__self__ != None:
                        for y in s:
                            if x != y:
                                AreNotEqual(x.__self__, y.__self__)
                                AreNotEqual(x.__func__, y.__func__)
    
    
    AreEqual(a.f.__func__, c.f.__func__)
    AreEqual(b.f.__func__, d.f.__func__)
    AreNotEqual(a.f.__self__, b.f.__self__)
    AreNotEqual(b.f.__self__, d.f.__self__)

run_test(__name__)
