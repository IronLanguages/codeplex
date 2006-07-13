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
items = 0

d = {'key1': 'value1', 'key2': 'value2'}
for key, value in d.iteritems():
     items += 1
     Assert((key, value) == ('key1', 'value1') or (key,value) == ('key2', 'value2'))

Assert(items == 2)

Assert(d["key1"] == "value1")
Assert(d["key2"] == "value2")

def getitem(d,k):
    d[k]

AssertError(KeyError, getitem, d, "key3")

x = d.get("key3")
Assert(x == None)
Assert(d["key1"] == d.get("key1"))
Assert(d["key2"] == d.get("key2"))
Assert(d.get("key3", "value3") == "value3")

AssertError(KeyError, getitem, d, "key3")
Assert(d.setdefault("key3") == None)
Assert(d.setdefault("key4", "value4") == "value4")
Assert(d["key3"] == None)
Assert(d["key4"] == "value4")


d2= dict(key1 = 'value1', key2 = 'value2')
Assert(d2['key1'] == 'value1')


## inherit from a dictionary
class MyDict(dict):
    def __setitem__(self, *args):
            super(MyDict, self).__setitem__(*args)

a = MyDict()
a[0] = 'abc'


class MyDict(dict):
    def __setitem__(self, *args):
        dict.__setitem__(self, *args)

a = MyDict()
a[0] = 'abc'

#########################################
# verify function environments, FieldIdDict,
# custom old class dict, and module environments
# all local identical to normal dictionaries

x = {}

class C: pass

AreEqual(dir(x), dir(C.__dict__))

class C: 
    xx = 'abc'
    yy = 'def'
    pass

AreEqual(dir(x), dir(C.__dict__))

class C: 
    x0 = 'abc'
    x1 = 'def'
    x2 = 'aaa'
    x3 = 'aaa'
    pass

AreEqual(dir(x), dir(C.__dict__))

class C: 
    x0 = 'abc'
    x1 = 'def'
    x2 = 'aaa'
    x3 = 'aaa'
    x4 = 'abc'
    x5 = 'def'
    x6 = 'aaa'
    x7 = 'aaa'
    x0 = 'abc'
    pass

AreEqual(dir(x), dir(C.__dict__))

class C: 
    x0 = 'abc'
    x1 = 'def'
    x2 = 'aaa'
    x3 = 'aaa'
    x4 = 'abc'
    x5 = 'def'
    x6 = 'aaa'
    x7 = 'aaa'
    x0 = 'abc'
    x10 = 'abc'
    x11 = 'def'
    x12 = 'aaa'
    x13 = 'aaa'
    x14 = 'abc'
    x15 = 'def'
    x16 = 'aaa'
    x17 = 'aaa'
    x10 = 'abc'
    pass

AreEqual(dir(x), dir(C.__dict__))


class C: 
    x0 = 'abc'
    x1 = 'def'
    x2 = 'aaa'
    x3 = 'aaa'
    x4 = 'abc'
    x5 = 'def'
    x6 = 'aaa'
    x7 = 'aaa'
    x0 = 'abc'
    x10 = 'abc'
    x11 = 'def'
    x12 = 'aaa'
    x13 = 'aaa'
    x14 = 'abc'
    x15 = 'def'
    x16 = 'aaa'
    x17 = 'aaa'
    x10 = 'abc'
    x20 = 'abc'
    x21 = 'def'
    x22 = 'aaa'
    x23 = 'aaa'
    x24 = 'abc'
    x25 = 'def'
    x26 = 'aaa'
    x27 = 'aaa'
    x20 = 'abc'
    x110 = 'abc'
    x111 = 'def'
    x112 = 'aaa'
    x113 = 'aaa'
    x114 = 'abc'
    x115 = 'def'
    x116 = 'aaa'
    x117 = 'aaa'
    x110 = 'abc'
    pass

AreEqual(dir(x), dir(C.__dict__))


a = C()
AreEqual(dir(x), dir(a.__dict__))

a = C()
a.abc = 'def'
a.ghi = 'def'
AreEqual(dir(x), dir(a.__dict__))

if is_cli:
    # cpython does not have __dict__ at the module level?
    AreEqual(dir(x), dir(__dict__))

#####################################################################
## coverage for CustomFieldIdDict

def contains(d, *attrs):
    for attr in attrs:
        Assert(attr in d)
        Assert(d.__contains__(attr))

def repeat_on_class(C):
    c = C()
    d = C.__dict__
    contains(d, '__doc__', 'x1', 'f1')

    ## recursive entries & repr
    d['abc'] = d
    x = repr(d) # shouldn't stack overflow
    Assert(x.find("'abc'") != -1)
    Assert(x.find("{...}") != -1)
    del d['abc']
    
    keys, values = d.keys(), d.values()
    AreEqual(len(keys), len(values))
    contains(keys, '__doc__', 'x1', 'f1')

    ## initial length
    l = len(d)
    Assert(l > 3)

    # add more attributes
    def f2(self): return 22
    def f3(self): return 33

    d['f2'] = f2
    d['x2'] = 20

    AreEqual(len(d), l + 2)
    AreEqual(d.__len__(), l + 2)

    contains(d, '__doc__', 'x1', 'x2', 'f1', 'f2')
    contains(d.keys(), '__doc__', 'x1', 'x2', 'f1', 'f2')

    AreEqual(d['x1'], 10)
    AreEqual(d['x2'], 20)
    AreEqual(d['f1'](c), 11)
    AreEqual(d['f2'](c), 22)
    AssertError(KeyError, lambda : d['x3'])
    AssertError(KeyError, lambda : d['f3'])

    ## get
    AreEqual(d.get('x1'), 10)
    AreEqual(d.get('x2'), 20)
    AreEqual(d.get('f1')(c), 11)
    AreEqual(d.get('f2')(c), 22)

    AreEqual(d.get('x3'), None)
    AreEqual(d.get('x3', 30), 30)
    AreEqual(d.get('f3'), None)
    AreEqual(d.get('f3', f3)(c), 33)
    

    ## setdefault
    AreEqual(d.setdefault('x1'), 10)
    AreEqual(d.setdefault('x1', 30), 10)
    AreEqual(d.setdefault('f1')(c), 11)
    AreEqual(d.setdefault('f1', f3)(c), 11)
    AreEqual(d.setdefault('x2'), 20)
    AreEqual(d.setdefault('x2', 30), 20)
    AreEqual(d.setdefault('f2')(c), 22)
    AreEqual(d.setdefault('f2', f3)(c), 22)
    AreEqual(d.setdefault('x3', 30), 30)
    AreEqual(d.setdefault('f3', f3)(c), 33)

    ## pop
    l1 = len(d); AreEqual(d.pop('x1', 30), 10)
    AreEqual(len(d), l1-1)
    l1 = len(d); AreEqual(d.pop('x2', 30), 20)
    AreEqual(len(d), l1-1)
    l1 = len(d); AreEqual(d.pop("xx", 70), 70)
    AreEqual(len(d), l1)

    ## has_key
    Assert(d.has_key('f1'))
    Assert(d.has_key('f2'))
    Assert(d.has_key('f3'))
    Assert(d.has_key('fx') == False)
    
    # subclassing, overriding __getitem__, and passing to 
    # eval    
    dictType = type(d)
    
    try:
        class newDict(dictType):
            def __getitem__(self, key):
                if self.key == 'abc':
                    return 'def'
                return super(self, dictType).__getitem__(key)
    except TypeError, ex:
        Assert(ex.msg.find('cannot derive from sealed or value types') != -1)
    else:           
        try:
            nd = newDict()
        except TypeError:
            # can't construct an instance of this dictionary
            pass
        else:
            AreEqual(eval('abc', {}, nd), 'def')

    ############### IN THIS POINT, d LOOKS LIKE ###############
    ##  {'f1': f1, 'f2': f2, 'f3': f3, 'x3': 30, '__doc__': 'This is comment', '__module__': '??'}

    ## iteritems
    lk = []
    for (k, v) in d.iteritems():
        lk.append(k)
        exp = None
        if k == 'f1': exp = 11
        elif k == 'f2': exp == 22
        elif k == 'f3': exp == 33
        
        if exp <> None:
            AreEqual(v(c), exp)

    contains(lk, 'f1', 'f2', 'f3', 'x3', '__doc__')

    # iterkeys
    lk = []
    for k in d.iterkeys():
        lk.append(k)

    contains(lk, 'f1', 'f2', 'f3', 'x3', '__doc__')

    # itervalues
    for v in d.itervalues():
        if callable(v):
            exp = v(c)
            Assert(exp in [11, 22, 33])
        elif v is str: 
            Assert(v == 'This is comment')
        elif v is int:
            Assert(v == 30)
            
    ## something fun before destorying it
    l1 = len(d); d[dict] = 3    # object as key
    AreEqual(len(d), l1+1)
   
    l1 = len(d); d[int] = 4     # object as key
    AreEqual(len(d), l1+1)

    l1 = len(d); del d[int]
    AreEqual(len(d), l1-1)

    l1 = len(d); del d[dict]
    AreEqual(len(d), l1-1)
   
    l1 = len(d); del d['x3']
    AreEqual(len(d), l1-1)

    l1 = len(d); d.popitem()
    AreEqual(len(d), l1-1)

    ## object as key
    d[int] = int
    d[str] = "str"

    AreEqual(d[int], int)
    AreEqual(d[str], "str")

    d.clear()
    AreEqual(len(d), 0)
    AreEqual(d.__len__(), 0)


class C:
    '''This is comment'''
    x1 = 10
    def f1(self): return 11
repeat_on_class(C)

class C(object):
    '''This is comment'''
    x1 = 10
    def f1(self): return 11
repeat_on_class(C)

## fromkeys
def repeat_on_class(C):
    d1 = C.__dict__
    l1 = len(d1)
    d2 = dict.fromkeys(d1)
    l2 = len(d2)
    AreEqual(l1, l2)
    AreEqual(d2['x'], None)
    AreEqual(d2['f'], None)

    d2 = dict.fromkeys(d1, 10)
    l2 = len(d2)
    AreEqual(l1, l2)
    AreEqual(d2['x'], 10)
    AreEqual(d2['f'], 10)
    
class C: 
    x = 10
    def f(self): pass
repeat_on_class(C)

class C(object): 
    x = 10
    def f(self): pass
repeat_on_class(C)

## compare 
def repeat_on_class(C1, C2):
    d1 = C1.__dict__
    d2 = C2.__dict__
        
    # object as key
    d1[int] = int
    d2[int] = int
    Assert(d1 <> d2)

    d2['f'] = d1['f']
    Assert([x for x in d1] == [x for x in d2])

    Assert(d1.fromkeys([x for x in d1]) >= d2.fromkeys([x for x in d2]))
    Assert(d1.fromkeys([x for x in d1]) <= d2.fromkeys([x for x in d2]))

    d1['y'] = 20
    d1[int] = int

    Assert(d1.fromkeys([x for x in d1]) > d2.fromkeys([x for x in d2]))
    Assert(d1.fromkeys([x for x in d1]) >= d2.fromkeys([x for x in d2]))
    Assert(d2.fromkeys([x for x in d2]) < d1.fromkeys([x for x in d1]))
    Assert(d2.fromkeys([x for x in d2]) <= d1.fromkeys([x for x in d1]))

class C1: 
    x = 10
    def f(self): pass
class C2:
    x = 10
    def f(self): pass

repeat_on_class(C1, C2) 

class C1(object): 
    x = 10
    def f(self): pass
class C2(object):
    x = 10
    def f(self): pass   
    
repeat_on_class(C1, C2) 


#####################################################################
## coverage for FieldIdDict

def func(): pass

d = func.__dict__

d['x1'] = 10
d['f1'] = lambda : 11
d[int]  = "int"
d[dict] = {2:20}

keys, values = d.keys(), d.values()
AreEqual(len(keys), len(values))
contains(keys, 'x1', 'f1', int, dict)

## initial length
l = len(d)
Assert(l == 4)

# add more attributes
d['x2'] = 20
d['f2'] = lambda x: 22

AreEqual(len(d), l + 2)
AreEqual(d.__len__(), l + 2)

contains(d, 'x1', 'x2', 'f1', 'f2', int, dict)
contains(d.keys(), 'x1', 'x2', 'f1', 'f2', int, dict)

AreEqual(d['x1'], 10)
AreEqual(d['x2'], 20)
AreEqual(d['f1'](), 11)
AreEqual(d['f2'](9), 22)
AssertError(KeyError, lambda : d['x3'])
AssertError(KeyError, lambda : d['f3'])

## get
AreEqual(d.get('x1'), 10)
AreEqual(d.get('x2'), 20)
AreEqual(d.get('f1')(), 11)
AreEqual(d.get('f2')(1), 22)

def f3(): return 33

AreEqual(d.get('x3'), None)
AreEqual(d.get('x3', 30), 30)
AreEqual(d.get('f3'), None)
AreEqual(d.get('f3', f3)(), 33)

## setdefault
AreEqual(d.setdefault('x1'), 10)
AreEqual(d.setdefault('x1', 30), 10)
AreEqual(d.setdefault('f1')(), 11)
AreEqual(d.setdefault('f1', f3)(), 11)
AreEqual(d.setdefault('x2'), 20)
AreEqual(d.setdefault('x2', 30), 20)
AreEqual(d.setdefault('f2')(1), 22)
AreEqual(d.setdefault('f2', f3)(1), 22)
AreEqual(d.setdefault('x3', 30), 30)
AreEqual(d.setdefault('f3', f3)(), 33)

## pop
l1 = len(d); AreEqual(d.pop('x1', 30), 10)
AreEqual(len(d), l1-1)
l1 = len(d); AreEqual(d.pop('x2', 30), 20)
AreEqual(len(d), l1-1)
l1 = len(d); AreEqual(d.pop(int, 70), "int")
AreEqual(len(d), l1-1)
l1 = len(d); AreEqual(d.pop("xx", 70), 70)
AreEqual(len(d), l1)

## has_key
Assert(d.has_key('f1'))
Assert(d.has_key('f2'))
Assert(d.has_key('f3'))
Assert(d.has_key(dict))
Assert(d.has_key('fx') == False)

############### IN THIS POINT, d LOOKS LIKE ###############
# f1, f2, f3, x3, dict as keys

## iteritems
lk = []
for (k, v) in d.iteritems():
    lk.append(k)
    if k == 'f1': AreEqual(v(), 11)
    elif k == 'f2': AreEqual(v(1), 22)
    elif k == 'f3': AreEqual(v(), 33)
    elif k == 'x3': AreEqual(v, 30)
    elif k == dict: AreEqual(v, {2:20})

contains(lk, 'f1', 'f2', 'f3', 'x3', dict)

# iterkeys
lk = []
for k in d.iterkeys():
    lk.append(k)

contains(lk, 'f1', 'f2', 'f3', 'x3', dict)

# itervalues
for v in d.itervalues():
    if callable(v):
        try: exp = v(1)
        except: pass
        try: exp = v()
        except: pass
        Assert(exp in [11, 22, 33])
    elif v is dict: 
        Assert(v == {2:20})
    elif v is int:
        Assert(v == 30)
        
## something fun before destorying it
l1 = len(d); d[int] = 4     # object as key
AreEqual(len(d), l1+1)

l1 = len(d); del d[int]
AreEqual(len(d), l1-1)

l1 = len(d); del d[dict]
AreEqual(len(d), l1-1)

l1 = len(d); del d['x3']
AreEqual(len(d), l1-1)

l1 = len(d); popped_item = d.popitem()
AreEqual(len(d), l1-1)

## object as key
d[int] = int
d[str] = "str"

AreEqual(d[int], int)
AreEqual(d[str], "str")

d.clear()
AreEqual(len(d), 0)
AreEqual(d.__len__(), 0)

d[int] = int
AreEqual(len(d), 1)


## comparison
def func1(): pass
def func2(): pass

d1 = func1.__dict__
d2 = func2.__dict__

d1['x'] = 10
d2['x'] = 30
d1[int] = int
d2[int] = int

# object as key
Assert(d1 <> d2)

d2['x'] = 10
Assert(d1 == d2)

Assert(d1 >= d2)
Assert(d1 <= d2)

d1['y'] = 20
d1[dict] = "int"

Assert(d1 > d2)
Assert(d1 >= d2)
Assert(d2 < d1)
Assert(d2 <= d1)

#####################################################################

# subclassing dict, overriding __init__

class foo(dict):
    def __init__(self, abc):
        self.abc = abc
        
a = foo('abc')
AreEqual(a.abc, 'abc')

# make sure dict.__init__ works

a = {}
a.__init__({'abc':'def'})
AreEqual(a, {'abc':'def'})
a.__init__({'abcd':'defg'})
AreEqual(a, {'abc':'def', 'abcd':'defg'})

# keyword arg contruction

# single kw-arg, should go into dict
a = dict(b=2)
AreEqual(a, {'b':2})

# dict value to init, Plus kw-arg
a = dict({'a':3}, b=2)
AreEqual(a, {'a':3, 'b':2})

# more than one
a = dict({'a':3}, b=2, c=5)
AreEqual(a, {'a':3, 'b':2, 'c':5})

try:
    dict({'a':3}, {'b':2}, c=5)
    AssertUnreachable()
except TypeError: pass

#####################################################################

def test_DictionaryUnionEnumerator():
    if is_cli == False:
        return

    class C(object): pass
    c = C()
    d = c.__dict__
    import System

    # Check empty enumerator
    e = System.Collections.IDictionary.GetEnumerator(d)
    AssertError(SystemError, getattr, e, "Key")
    AreEqual(e.MoveNext(), False)
    AssertError(SystemError, getattr, e, "Key")
    
    # Add non-string attribute
    d[1] = 100
    e = System.Collections.IDictionary.GetEnumerator(d)
    # This returns an instance of DictionaryUnionEnumerator
    AreEqual(e.GetType().Name.__contains__("DictionaryUnionEnumerator"), True)
    AssertError(SystemError, getattr, e, "Key")
    AreEqual(e.MoveNext(), True)
    AreEqual(e.Key, 1)
    AreEqual(e.MoveNext(), False)
    AssertError(SystemError, getattr, e, "Key")
    
    # Add string attribute
    c.attr = 100
    e = System.Collections.IDictionary.GetEnumerator(d)
    AssertError(SystemError, getattr, e, "Key")
    AreEqual(e.MoveNext(), True)
    key1 = e.Key
    AreEqual(e.MoveNext(), True)
    key2 = e.Key
    AreEqual((key1, key2) == (1, "attr") or (key1, key2) == ("attr", 1), True)
    AreEqual(e.MoveNext(), False)
    AssertError(SystemError, getattr, e, "Key")
    
    # Remove non-string attribute
    del d[1]
    e = System.Collections.IDictionary.GetEnumerator(d)
    AssertError(SystemError, getattr, e, "Key")
    AreEqual(e.MoveNext(), True)
    AreEqual(e.Key, "attr")
    AreEqual(e.MoveNext(), False)
    AssertError(SystemError, getattr, e, "Key")
    
    # Remove string attribute and check empty enumerator
    del c.attr
    e = System.Collections.IDictionary.GetEnumerator(d)
    AssertError(SystemError, getattr, e, "Key")
    AreEqual(e.MoveNext(), False)
    AssertError(SystemError, getattr, e, "Key")
    
def test_same_but_different():
    """Test case checks that when two values who are logically different but share hash code & equality
    result in only a single entry"""
    
    AreEqual({-10:0, -10L:1}, {-10:1})


def test_eval_locals_simple():
    class Locals(dict):
        def __getitem__(self, key):
            try:
                return dict.__getitem__(self, key)
            except KeyError, e:
                return 'abc'
    
    locs = Locals()
    AreEqual(eval("unknownvariable", globals(), locs), 'abc')

run_test(__name__)