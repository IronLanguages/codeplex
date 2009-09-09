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

#-----------------------------------------------------------------------------------
#These have to be run first: importing iptest.assert_util masked a bug. Just make sure
#these do not throw
for stuff in [bool, True, False]:
    temp = dir(stuff)

items = globals().items() #4716

import sys
def cp946():
    builtins = __builtins__
    if type(builtins) is type(sys):
        builtins = builtins.__dict__
    
    if "hasattr" not in builtins:
        raise "hasattr should be in __builtins__"
    if "HasAttr" in builtins:
        raise "HasAttr should not be in __builtins__"

cp946()
if sys.platform=="cli":
    import clr
cp946()


#-----------------------------------------------------------------------------------
from iptest.assert_util import *

AssertError(NameError, lambda: __new__)


def test_callable():
    class C: x=1

    Assert(callable(min))
    Assert(not callable("a"))
    Assert(callable(callable))
    Assert(callable(lambda x, y: x + y))
    Assert(callable(C))
    Assert(not callable(C.x))
    Assert(not callable(__builtins__))

def test_callable_oldclass():
    # for class instances, callable related to whether the __call__ attribute is defined.
    # This can be mutated at runtime.
    class Dold:
        pass
    d=Dold()
    #
    AreEqual(callable(d), False)
    #
    d.__call__ = None # This defines the attr, even though it's None
    AreEqual(callable(d), True) # True for oldinstance, False for new classes.
    #
    del (d.__call__) # now remove the attr, no longer callable
    AreEqual(callable(d), False)

def test_callable_newclass():
    class D(object):
      pass
    AreEqual(callable(D), True)
    d=D()
    AreEqual(callable(d), False)
    #
    # New class with a __call__ defined is callable()
    class D2(object):
      def __call__(self): pass
    d2=D2()
    AreEqual(callable(d2), True)
    # Inherit callable
    class D3(D2):
      pass
    d3=D3()
    AreEqual(callable(d3), True)

def test_cmp():
    x = {}
    x['foo'] = x
    y = {}
    y['foo'] = y

    AssertError(RuntimeError, cmp, x, y)

def test_reversed():
    class ToReverse:
        a = [1,2,3,4,5,6,7,8,9,0]
        def __len__(self): return len(self.a)
        def __getitem__(self, i): return self.a[i]

    x = []
    for i in reversed(ToReverse()):
        x.append(i)

    Assert(x == [0,9,8,7,6,5,4,3,2,1])

    # more tests for 'reversed'
    AssertError(TypeError, reversed, 1) # arg to reversed must be a sequence
    AssertError(TypeError, reversed, None)
    AssertError(TypeError, reversed, ToReverse)
    
    # no __len__ on class, reversed should throw
    class x(object):
        def __getitem__(self, index): return 2
    
    def __len__(): return 42

    a = x()
    a.__len__ = __len__
    AssertError(TypeError, reversed, a)

    # no __len__ on class, reversed should throw
    class x(object):
        def __len__(self): return 42

    def __getitem__(index): return 2    
    a = x()
    a.__getitem__ = __getitem__
    AssertError(TypeError, reversed, a)


def test_reduce():
    def add(x,y): return x+y;
    Assert(reduce(add, [2,3,4]) == 9)
    Assert(reduce(add, [2,3,4], 1) == 10)

    AssertError(TypeError, reduce, None, [1,2,3]) # arg 1 must be callable
    AssertError(TypeError, reduce, None, [2,3,4], 1)
    AssertError(TypeError, reduce, add, None) # arg 2 must support iteration
    AssertError(TypeError, reduce, add, None, 1)
    AssertError(TypeError, reduce, add, []) # arg 2 must not be empty sequence with no initial value
    AssertError(TypeError, reduce, add, "") # empty string sequence
    AssertError(TypeError, reduce, add, ()) # empty tuple sequence
    
    Assert(reduce(add, [], 1), 1) # arg 2 has initial value through arg 3 so no TypeError for this
    Assert(reduce(add, [], None) == None)
    AssertError(TypeError, reduce, add, [])
    AssertError(TypeError, reduce, add, "")
    AssertError(TypeError, reduce, add, ())

def test_apply():
    def foo(): return 42
    
    AreEqual(apply(foo), 42)
    
def test_map():
    def cat(x,y):
        ret = ""
        if x != None: ret += x
        if y != None: ret += y
        return ret

    Assert(map(cat, ["a","b"],["c","d", "e"]) == ["ac", "bd", "e"])
    Assert(map(lambda x,y: x+y, [1,1],[2,2]) == [3,3])
    Assert(map(None, [1,2,3]) == [1,2,3])
    Assert(map(None, [1,2,3], [4,5,6]) == [(1,4),(2,5),(3,6)])
    AreEqual(map(lambda x:'a' + x + 'c', 'b'), ['abc'])
    
def test_range():
    AreEqual(range(2, 5.0), [2,3,4])
    AreEqual(range(3, 10, 2.0), [3, 5, 7, 9])
    if is_cpython:
        AssertErrorWithMessage(TypeError, "range() integer end argument expected, got float.", 
                               range, float(-2<<32))
        AssertErrorWithMessage(TypeError, "range() integer end argument expected, got float.", 
                               range, 0, float(-2<<32))
        AssertErrorWithMessage(TypeError, "range() integer start argument expected, got float.", 
                               range, float(-2<<32), 100)
        AssertErrorWithMessage(TypeError, "range() integer step argument expected, got float.", 
                               range, 0, 100, float(-2<<32))
        AssertErrorWithMessage(TypeError, "range() integer start argument expected, got float.", 
                               range, float(-2<<32), float(-2<<32), float(-2<<32))
    else: #CodePlex 24249
        AssertErrorWithPartialMessage(TypeError, " end ", range, float(-2<<32))
        AssertErrorWithPartialMessage(TypeError, " end ", range, 0, float(-2<<32))
        AssertErrorWithPartialMessage(TypeError, " start ", range, float(-2<<32), 100)
        AssertErrorWithPartialMessage(TypeError, " step ", range, 0, 100, float(-2<<32))


def test_sorted():
    a = [6,9,4,5,3,1,2,7,8]
    Assert(sorted(a) == [1,2,3,4,5,6,7,8,9])
    Assert(a == [6,9,4,5,3,1,2,7,8])
    Assert(sorted(a, None, None, True) == [9,8,7,6,5,4,3,2,1])

    def invcmp(a,b): return -cmp(a,b)

    Assert(sorted(range(10), None, None, True) == range(10)[::-1])
    Assert(sorted(range(9,-1,-1), None, None, False) == range(10))
    Assert(sorted(range(10), invcmp, None, True) == sorted(range(9,-1,-1), None, None, False))
    Assert(sorted(range(9,-1,-1),invcmp, None, True) == sorted(range(9,-1,-1), None, None, False))

    class P:
        def __init__(self, n, s):
            self.n = n
            self.s = s

    def equal_p(a,b):      return a.n == b.n and a.s == b.s

    def key_p(a):          return a.n.lower()

    def cmp_s(a,b):        return cmp(a.s, b.s)

    def cmp_n(a,b):        return cmp(a.n, b.n)

    a = [P("John",6),P("Jack",9),P("Gary",4),P("Carl",5),P("David",3),P("Joe",1),P("Tom",2),P("Tim",7),P("Todd",8)]
    x = sorted(a, cmp_s)
    y = [P("Joe",1),P("Tom",2),P("David",3),P("Gary",4),P("Carl",5),P("John",6),P("Tim",7),P("Todd",8),P("Jack",9)]

    for i,j in zip(x,y): Assert(equal_p(i,j))

    # case sensitive compariso is the default one

    a = [P("John",6),P("jack",9),P("gary",4),P("carl",5),P("David",3),P("Joe",1),P("Tom",2),P("Tim",7),P("todd",8)]
    x = sorted(a, cmp_n)
    y = [P("David",3),P("Joe",1),P("John",6),P("Tim",7),P("Tom",2),P("carl",5),P("gary",4),P("jack",9),P("todd",8)]

    for i,j in zip(x,y): Assert(equal_p(i,j))

    # now compare using keys - case insensitive

    x = sorted(a,None,key_p)
    y = [P("carl",5),P("David",3),P("gary",4),P("jack",9),P("Joe",1),P("John",6),P("Tim",7),P("todd",8),P("Tom",2)]
    
    for i,j in zip(x,y): Assert(equal_p(i,j))

    d = {'John': 6, 'Jack': 9, 'Gary': 4, 'Carl': 5, 'David': 3, 'Joe': 1, 'Tom': 2, 'Tim': 7, 'Todd': 8}
    x = sorted([(v,k) for k,v in d.items()])
    Assert(x == [(1, 'Joe'), (2, 'Tom'), (3, 'David'), (4, 'Gary'), (5, 'Carl'), (6, 'John'), (7, 'Tim'), (8, 'Todd'), (9, 'Jack')])

def test_unichr():

    #Added the following to resolve Codeplex WorkItem #3220.
    max_uni = sys.maxunicode
    Assert(max_uni==0xFFFF or max_uni==0x10FFFF)
    max_uni_plus_one = max_uni + 1
    
    huger_than_max = 100000
    max_ok_value = u'\uffff'
    
    #special case for WorkItem #3220
    if max_uni==0x10FFFF:
        huger_than_max = 10000000
        max_ok_value = u'\u0010FFFF' #OK representation for UCS4???
        
    AssertError(ValueError, unichr, -1) # arg must be in the range [0...65535] or [0...1114111] inclusive
    AssertError(ValueError, unichr, max_uni_plus_one)
    AssertError(ValueError, unichr, huger_than_max)
    Assert(unichr(0) == '\x00')
    Assert(unichr(max_uni) == max_ok_value)

def test_max_min():
    Assert(max([1,2,3]) == 3)
    Assert(max((1,2,3)) == 3)
    Assert(max(1,2,3) == 3)
    
    Assert(min([1,2,3]) == 1)
    Assert(min((1,2,3)) == 1)
    Assert(min(1,2,3) == 1)
    
    AreEqual(max((1,2), None), (1, 2))
    AreEqual(min((1,2), None), None)

def test_abs():
    AssertError(TypeError,abs,None)

    #long integer passed to abs
    AreEqual(22L, abs(22L))
    AreEqual(22L, abs(-22L))

    #bool passed to abs
    AreEqual(1, abs(True))
    AreEqual(0, abs(False))

    #__abs__ defined on user type
    class myclass:
        def __abs__(self):
            return "myabs"
    c = myclass()
    AreEqual("myabs", abs(c))
    
def test_coerce():
    AreEqual(coerce(None, None), (None, None))
    AssertError(TypeError, coerce, None, 1)
    AssertError(TypeError, coerce, 1, None)
   
def test_zip():
    def foo(): yield 2

    def bar():
        yield 2
        yield 3

    AreEqual(zip(foo()), [(2,)])
    AreEqual(zip(foo(), foo()), [(2,2)])
    AreEqual(zip(foo(), foo(), foo()), [(2,2,2)])

    AreEqual(zip(bar(), foo()), [(2,2)])
    AreEqual(zip(foo(), bar()), [(2,2)])
    
    # test passing the same object for multiple iterables
    AreEqual(zip(*[iter([])]), [])
    AreEqual(zip(*[iter([])] * 2), [])
    AreEqual(zip(*[xrange(3)] * 2), [(0, 0), (1, 1), (2, 2)])
    AreEqual(zip(*[iter(["0", "1"])] * 2), [('0', '1')])
    AreEqual(zip(*[iter(["0", "1", "2"])] * 3), [('0', '1', '2')])
    AreEqual(zip(*'abc'), [('a', 'b', 'c')])

def test_dir():
    local_var = 10
    AreEqual(dir(), ['local_var'])
    
    def f():
        local_var = 10
        AreEqual(dir(*()), ['local_var'])
    f()
    
    def f():
        local_var = 10
        AreEqual(dir(**{}), ['local_var'])
    f()

    def f():
        local_var = 10
        AreEqual(dir(*(), **{}), ['local_var'])
    f()

    class A(object):
        def __dir__(self):
                return ['foo']
        def __init__(self):
                self.abc = 3
    
    AreEqual(dir(A()), ['foo'])

    class C:
        a = 1
    
    class D(object, C):
        b = 2
    
    Assert('a' in dir(D()))
    Assert('b' in dir(D()))
    Assert('__hash__' in dir(D()))
    
    if is_cli:
        import clr
        try:
            clr.AddReference("Microsoft.Scripting.Core")
        except Exception, e:
            if is_net40:
                clr.AddReference("System.Core")
            else:
                raise e
        
        from System.Dynamic import ExpandoObject
        eo = ExpandoObject()
        eo.bill = 5
        Assert('bill' in dir(eo))

    
def test_ord():
    # ord of extensible string
    class foo(str): pass
    
    AreEqual(ord(foo('A')), 65)
    
@skip("silverlight", "Merlin bug #404247: this test doesn't work when the file is executed from non-Python host (thost)" )
def test_top_level_dir():
    Assert("__name__" in top_level_dir)
    Assert("__builtins__" in top_level_dir)

top_level_dir = dir()

x = 10
y = 20

def test_eval():
    d1 = { 'y' : 3 }
    d2 = { 'x' : 4 }

    AreEqual(eval("x + y", None, d1), 13)
    AreEqual(eval("x + y", None, None), 30)
    AreEqual(eval("x + y", None), 30)
    AreEqual(eval("x + y", None, d2), 24)

    AssertError(NameError, eval, "x + y", d1)
    AssertError(NameError, eval, "x + y", d1, None)

    try:
        eval('1+')
        AssertUnreachable()
    except Exception, exp:
        pass
    else:
        AssertUnreachable()

def test_len():
    # old-style classes throw AttributeError, new-style classes throw
    # TypeError
    AssertError(TypeError, len, 2)
    class foo: pass
    
    AssertError(AttributeError, len, foo())
    
    class foo(object): pass
    
    AssertError(TypeError, len, foo())

def test_int_ctor():
    AreEqual(int('0x10', 16), 16)
    AreEqual(int('0X10', 16), 16)
    AreEqual(long('0x10', 16), 16L)
    AreEqual(long('0X10', 16), 16L)
   
def test_type():
    AreEqual(len(type.__bases__), 1)
    AreEqual(type.__bases__[0], object)
    
def test_globals():
    Assert(not globals().has_key("_"))
    AreEqual(globals().keys().count("_"), 0)

def test_vars():
    """vars should look for user defined __dict__ value and directly return the provided value"""
    
    class foo(object):
        def getDict(self):
                return {'a':2}
        __dict__ = property(fget=getDict)
    
    AreEqual(vars(foo()), {'a':2})
    
    class foo(object):
        def __getattribute__(self, name):
            if name == "__dict__":
                    return {'a':2}
            return object.__getattribute__(self, name)

    AreEqual(vars(foo()), {'a':2})
    
    class foo(object):
        def getDict(self):
                return 'abc'
        __dict__ = property(fget=getDict)
    
    AreEqual(vars(foo()), 'abc')
    
    class foo(object):
        def __getattribute__(self, name):
            if name == "__dict__":
                    return 'abc'
            return object.__getattribute__(self, name)

    AreEqual(vars(foo()), 'abc')

    def f():
        local_var = 10
        AreEqual(vars(*()), {'local_var' : 10})
    f()

    def f():
        local_var = 10
        AreEqual(vars(**{}), {'local_var' : 10})
    f()

    def f():
        local_var = 10
        AreEqual(vars(*(), **{}), {'local_var' : 10})
    f()

def test_compile():
    for x in ['exec', 'eval', 'single']:
        c = compile('2', 'foo', x)
        AreEqual(c.co_filename, 'foo')
        
    class mystdout(object):
        def __init__(self):
            self.data = []
        def write(self, data):
            self.data.append(data)

    import sys
    out = mystdout()
    sys.stdout = out
    try:
        c = compile('2', 'test', 'single')
        exec c
        AreEqual(out.data, ['2', '\n'])
    finally:
        sys.stdout = sys.__stdout__
        
    for code in ["abc" + chr(0) + "def", chr(0) + "def", "def" + chr(0)]:
        AssertError(TypeError, compile, code, 'f', 'exec')

def test_str_none():
    class foo(object):
        def __str__(self):
                return None
    
    AreEqual(foo().__str__(), None)
    AssertError(TypeError, str, foo())

def test_not_in_globals():
    AssertError(NameError, lambda: __dict__)
    AssertError(NameError, lambda: __module__)
    AssertError(NameError, lambda: __class__)
    AssertError(NameError, lambda: __init__)

# Regress bug 319126: __int__ on long should return long, not overflow
def test_long_int():
  l=long(1.23e300)
  i = l.__int__()
  Assert(type(l) == type(i))
  Assert(i == l)
    
def test_round():
    AreEqual(round(number=3.4), 3.0)
    AreEqual(round(number=3.125, ndigits=3), 3.125)
    AreEqual(round(number=3.125, ndigits=0), 3)

def test_cp16000():
    class K(object):
        FOO = 39
        def fooFunc():
            return K.FOO
        def memberFunc(self):
            return K.FOO * 3.14


    temp_list = [   None, str, int, long, K,
                    "", "abc", u"abc", 34, 1111111111111L, 3.14, K(), K.FOO,
                    id, hex, K.fooFunc, K.memberFunc, K().memberFunc,
                ]

    if is_cli:
        import System
        temp_list += [  System.Exception, System.InvalidOperationException(),
                        System.Single, System.UInt16(5), System.Version()]

    for x in temp_list:
        Assert(type(id(x)) in [int, long], 
               str(type(id(x))))

#------------------------------------------------------------------------------
def test_locals_contains():
    global locals_globals
    locals_globals = 2
    def func():
        Assert(not 'locals_globals' in locals())
    func()

def in_main():
    Assert(not 'locals_globals' in locals())
    Assert(not 'locals_globals' in globals())
    
    def in_main_sub1():
        Assert(not 'locals_globals' in locals())
        Assert(not 'locals_globals' in globals())
    
    def in_main_sub2():
        global local_globals
        Assert(not 'locals_globals' in locals())
        Assert('locals_globals' in globals())
        
        def in_main_sub3():
            local_globals = 42
            Assert(not 'locals_globals' in locals())
            Assert('locals_globals' in globals())
        
        in_main_sub3()
    
    in_main_sub1()
    return in_main_sub2


def test_error_messages():
    AssertErrorWithMessages(TypeError, "join() takes exactly 1 argument (2 given)", "join() takes exactly one argument (2 given)", "".join, ["a", "b"], "c")
    
    
temp_func = in_main()
locals_globals = 7
temp_func()

#------------------------------------------------------------------------------
run_test(__name__)

