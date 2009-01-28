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

def test_tuple_index():
    t = (1,4,3,0,3)
    
    AssertError(TypeError, t.index)
    AssertError(TypeError, t.index, 3, 'a')
    AssertError(TypeError, t.index, 3, 2, 'a')
    AssertError(TypeError, t.index, 3, 2, 5, 1)
    
    AssertError(ValueError, t.index, 5)
    AssertError(ValueError, t.index, 'a')
    
    AreEqual(t.index(1), 0)
    AreEqual(t.index(3), 2)
    AreEqual(t.index(3, 2), 2)
    AreEqual(t.index(3, 3), 4)
    AssertError(ValueError, t.index, 3, 3, 4)
    AreEqual(t.index(3, 3, 5), 4)
    AreEqual(t.index(3, 3, 100), 4)
    
    AreEqual(t.index(3, -1), 4)
    AreEqual(t.index(3, -3, -1), 2)
    AreEqual(t.index(3, 2, -2), 2)
    AssertError(ValueError, t.index, 3, 3, -1)
    AssertError(ValueError, t.index, 3, -2, 0)

def test_tuple_count():
    t = ('1','2','3',1,3,3,2,3,1,'1','1',3,'3')
    
    AssertError(TypeError, t.count)
    AssertError(TypeError, t.count, 1, 2)
    
    AreEqual(t.count('1'), 3)
    AreEqual(t.count('2'), 1)
    AreEqual(t.count('3'), 2)
    AreEqual(t.count(1), 2)
    AreEqual(t.count(2), 1)
    AreEqual(t.count(3), 4)

@skip("multiple_execute")
def test_warnings():
    import sys
    from collections import deque
    from _warnings import (filters, default_action, once_registry, warn, warn_explicit)
    
    stderr_filename = "test__warnings_stderr.txt"
    
    # clean up after possible incomplete test runs
    def cleanup():
        try:
            import os
            if os.path.exists(stderr_filename):
                os.remove(stderr_filename)
        except ImportError:
            # os module not available in IronPython; use .NET classes
            from System.IO import File
            if File.Exists(stderr_filename):
                File.Delete(stderr_filename)
    cleanup()
    
    # helper for test output
    expected = deque() # expected output (ignoring filename, lineno, and line)
    def expect(warn_type, message):
        for filter in filters:
            if filter[0] == "ignore" and issubclass(warn_type, filter[2]):
                return
        expected.append(": " + warn_type.__name__ + ": " + message + "\n")
    
    # redirect stderr
    stderr_old = sys.stderr
    stderr_new = open(stderr_filename,'w')
    sys.stderr = stderr_new
    
    # generate test output
    warn_types = [Warning, UserWarning, DeprecationWarning, PendingDeprecationWarning, SyntaxWarning, RuntimeWarning, FutureWarning, ImportWarning, UnicodeWarning, BytesWarning]
    warn("Warning Message!")
    expect(UserWarning, "Warning Message!")
    for warn_type in warn_types:
        warn(warn_type("Type-overriding message!"), UnicodeWarning)
        expect(warn_type, "Type-overriding message!")
        warn("Another Warning Message!", warn_type)
        expect(warn_type, "Another Warning Message!")
        warn_explicit("Explicit Warning!", warn_type, "nonexistent_file.py", 12)
        expect(warn_type, "Explicit Warning!")
        warn_explicit("Explicit Warning!", warn_type, "test_python26.py", 34)
        expect(warn_type, "Explicit Warning!")
        warn_explicit("Explicit Warning!", warn_type, "nonexistent_file.py", 56, "module.py")
        expect(warn_type, "Explicit Warning!")
        warn_explicit("Explicit Warning!", warn_type, "test_python26.py", 78, "module.py")
        expect(warn_type, "Explicit Warning!")
    
    # reset stdout,stderr
    sys.stderr = stderr_old
    stderr_new.close()
    
    # check test output
    stderr_file = open(stderr_filename, 'r')
    # count lines
    nlines = 0
    for line in stderr_file:
        if not line.startswith("  "):
            nlines += 1
    AreEqual(nlines, expected.__len__())
    stderr_file.seek(0)
    # match lines
    for line in stderr_file:
        if line.startswith("  "):
            continue
        Assert(line.endswith(expected.popleft()))
    # clean up
    stderr_file.close()
    
    # remove generated files
    cleanup()

def test_warnings_showwarning():
    try:
        from _warnings import showwarning
        from System.IO import StringWriter
        
        class string_file(file):
            def __init__(self):
                self.buf = None
            def write(self, s):
                Assert(issubclass(type(s), str))
                self.buf = s
            def __repr__(self):
                return self.buf if self.buf else self.buf.__repr__()

        sw = StringWriter()
        sf = string_file()
        
        showwarning("testwarning", RuntimeWarning, "some_file.py", 666, sw, "# this is a line of code")
        showwarning("testwarning", SyntaxWarning, "other_file.py", 42, sf, "# another line of code")
        
        AreEqual(sw.ToString(), "some_file.py:666: RuntimeWarning: testwarning\n  # this is a line of code\n")
        AreEqual(sf.__repr__(), "other_file.py:42: SyntaxWarning: testwarning\n  # another line of code\n")
        
        sw.Close()
        sf.close()

    except ImportError:
        # _warnings.showwarning and/or .NET classes unavailable in CPython - skip test
        pass

def test_builtin_next():
    from collections import deque
    
    values = [1,2,3,4]
    
    for iterable in [list, tuple, set, deque]:
        i = iter(iterable(values))
        AreEqual(next(i), 1)
        AreEqual(next(i), 2)
        AreEqual(next(i), 3)
        AreEqual(next(i), 4)
        AssertError(StopIteration, next, i)
        
        i = iter(iterable(values))
        AreEqual(next(i, False), 1)
        AreEqual(next(i, False), 2)
        AreEqual(next(i, False), 3)
        AreEqual(next(i, False), 4)
        AreEqual(next(i, False), False)
        AreEqual(next(i, False), False)
    
    i = iter('abcdE')
    AreEqual(next(i), 'a')
    AreEqual(next(i), 'b')
    AreEqual(next(i), 'c')
    AreEqual(next(i), 'd')
    AreEqual(next(i), 'E')
    AssertError(StopIteration, next, i)
    
    i = iter('edcbA')
    AreEqual(next(i, False), 'e')
    AreEqual(next(i, False), 'd')
    AreEqual(next(i, False), 'c')
    AreEqual(next(i, False), 'b')
    AreEqual(next(i, False), 'A')
    AreEqual(next(i, False), False)
    AreEqual(next(i, False), False)

def test_sys_flags():
    import sys
    AssertContains(dir(sys), 'flags')
    
    # Assertion helpers
    def IsInt(x):
        Assert(type(x) == int)
    def IsFlagInt(x):
        Assert(x in [0,1,2])
    
    # Check repr
    AreEqual(repr(type(sys.flags)), "<type 'sys.flags'>")
    Assert(repr(sys.flags).startswith("sys.flags(debug="))
    Assert(repr(sys.flags).endswith(")"))
    
    # Check attributes
    attrs = set(dir(sys.flags))
    structseq_attrs = set(["n_fields", "n_sequence_fields", "n_unnamed_fields"])
    flag_attrs = set(['bytes_warning', 'debug', 'division_new', 'division_warning', 'dont_write_bytecode', 'ignore_environment', 'inspect', 'interactive', 'no_site', 'no_user_site', 'optimize', 'py3k_warning', 'tabcheck', 'unicode', 'verbose'])
    expected_attrs = structseq_attrs.union(flag_attrs, dir(object), dir(tuple))
    expected_attrs -= set(["index", "count", "__iter__", "__getnewargs__"]) # tuple fields that don't appear in sys.flags
    
    AreEqual(attrs, set(dir(type(sys.flags))))
    attrs -= set(['__iter__']) # ignore '__iter__', which appears in IPy but not CPy
    AreEqual(expected_attrs - attrs, set()) # check for missing attrs
    AreEqual(attrs - expected_attrs, set()) # check for too many attrs
    
    for attr in structseq_attrs.union(flag_attrs):
        IsInt(getattr(sys.flags, attr))
    for attr in flag_attrs:
        IsFlagInt(getattr(sys.flags, attr))
    AreEqual(sys.flags.n_sequence_fields, len(flag_attrs))
    AreEqual(sys.flags.n_fields, sys.flags.n_sequence_fields)
    AreEqual(sys.flags.n_unnamed_fields, 0)
    
    # Test tuple-like functionality
    
    # __add__
    x = sys.flags + ()
    y = sys.flags + (7,)
    z = sys.flags + (6,5,4,3,2)
    
    # __len__
    AreEqual(len(sys.flags), len(flag_attrs))
    AreEqual(len(sys.flags), len(x))
    AreEqual(len(sys.flags), len(y) - 1)
    AreEqual(len(sys.flags), len(z) - 5)
    
    # __eq__
    AreEqual(sys.flags, x)
    Assert(sys.flags == x)
    Assert(x == sys.flags)
    Assert(sys.flags == sys.flags)
    # __ne__
    AssertFalse(sys.flags != sys.flags)
    Assert(sys.flags != z)
    Assert(z != sys.flags)
    # __ge__
    Assert(sys.flags >= sys.flags)
    Assert(sys.flags >= x)
    Assert(x >= sys.flags)
    AssertFalse(sys.flags >= y)
    Assert(z >= sys.flags)
    # __le__
    Assert(sys.flags <= sys.flags)
    Assert(sys.flags <= x)
    Assert(x <= sys.flags)
    Assert(sys.flags <= y)
    AssertFalse(y <= sys.flags)
    # __gt__
    AssertFalse(sys.flags > sys.flags)
    AssertFalse(sys.flags > x)
    AssertFalse(x > sys.flags)
    AssertFalse(sys.flags > y)
    Assert(z > sys.flags)
    # __lt__
    AssertFalse(sys.flags < sys.flags)
    AssertFalse(sys.flags < x)
    AssertFalse(x < sys.flags)
    Assert(sys.flags < y)
    AssertFalse(y < sys.flags)
    
    # __mul__
    AreEqual(sys.flags * 2, x * 2)
    AreEqual(sys.flags * 5, x * 5)
    # __rmul__
    AreEqual(5 * sys.flags, x * 5)
    
    # __contains__
    AreEqual(0 in sys.flags, 0 in x)
    AreEqual(1 in sys.flags, 1 in x)
    AreEqual(2 in sys.flags, 2 in x)
    AssertFalse(3 in sys.flags)
    
    # __getitem__
    for i in range(len(sys.flags)):
        AreEqual(sys.flags[i], x[i])
    # __getslice__
    AreEqual(sys.flags[:], x[:])
    AreEqual(sys.flags[2:], x[2:])
    AreEqual(sys.flags[3:6], x[3:6])
    AreEqual(sys.flags[1:-1], x[1:-1])
    AreEqual(sys.flags[-7:11], x[-7:11])
    AreEqual(sys.flags[-10:-5], x[-10:-5])
    
    # other sequence ops
    AreEqual(set(sys.flags), set(x))
    AreEqual(list(sys.flags), list(x))
    count = 0
    for f in sys.flags:
        count += 1
        IsFlagInt(f)
    AreEqual(count, len(sys.flags))
    
    # sanity check
    if (sys.dont_write_bytecode):
        AreEqual(sys.flags.dont_write_bytecode, 1)
    else:
        AreEqual(sys.flags.dont_write_bytecode, 0)
    if (sys.py3kwarning):
        AreEqual(sys.flags.py3k_warning, 1)
    else:
        AreEqual(sys.flags.py3k_warning, 0)

def test_functools_reduce():
    import _functools
    
    words = ["I", "am", "the", "walrus"]
    combine = lambda s,t: s + " " + t
    
    Assert(hasattr(_functools, "reduce"))
    
    AreEqual(_functools.reduce(combine, words), "I am the walrus")
    AreEqual(_functools.reduce(combine, words), reduce(combine, words))

def test_log():
    import math
    
    zeros = [-1, -1.0, -1L, 0, 0.0, 0L]
    nonzeros = [2, 2.0, 2L]
    ones = [1, 1.0, 1L]
    
    AreNotEqual(type(zeros[0]), type(zeros[2]))
    
    for z0 in zeros:
        AssertError(ValueError, math.log, z0)
        AssertError(ValueError, math.log10, z0)
        for z in zeros:
            AssertError(ValueError, math.log, z0, z)
        for n in nonzeros + ones:
            AssertError(ValueError, math.log, z0, n)
            AssertError(ValueError, math.log, n, z0)
    
    for one in ones:
        for n in nonzeros:
            AssertError(ZeroDivisionError, math.log, n, 1)

run_test(__name__)
