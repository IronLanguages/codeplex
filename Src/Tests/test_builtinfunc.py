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

def test_callable():
    class C: x=1

    Assert(callable(min))
    Assert(not callable("a"))
    Assert(callable(callable))
    Assert(callable(lambda x, y: x + y))
    Assert(callable(C))
    Assert(not callable(C.x))

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

def test_abs():
    AssertError(TypeError,abs,None)

    #	long integer passed to abs
    AreEqual(22L, abs(22L)) 
    AreEqual(22L, abs(-22L))

    #	bool passed to abs
    AreEqual(1, abs(True)) 
    AreEqual(0, abs(False))

    #	__abs__ defined on user type
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
   
def test_dir():
    local_var = 10
    AreEqual(dir(), ['local_var'])
    
def test_ord():
    # ord of extensible string
    class foo(str): pass
    
    AreEqual(ord(foo('A')), 65)
    

Assert("__name__" in dir())
Assert("__builtins__" in dir())


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

def test_len():
    # old-style classes throw AttributeError, new-style classes throw
    # TypeError
    AssertError(TypeError, len, 2)
    class foo: pass
    
    AssertError(AttributeError, len, foo())
    
    class foo(object): pass
    
    AssertError(TypeError, len, foo())
    
run_test(__name__)

