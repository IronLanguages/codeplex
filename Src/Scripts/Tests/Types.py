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

for (x, y) in [("2", int), (2, int), ("string", str), (None, int), (str, None), (int, 3), (int, (6, 7))]:
    AssertError(TypeError, lambda: issubclass(x, y))
    
import clr

Assert(issubclass(int, int))
Assert(not issubclass(str, int))
Assert(not issubclass(int, (str, str)))
Assert(issubclass(int, (str, int)))

Assert(str(None) == "None")
Assert(issubclass(type(None),type(None)))
Assert(str(type(None)) == "<type 'NoneType'>")
Assert(str(type(None)) == type(None).ToString())
Assert(str(1) == "1")
Assert('__str__' in dir(None))

def tryAssignToNoneAttr():
	None.__doc__ = "Nothing!"

def tryAssignToNoneNotAttr():
	None.notanattribute = "";

AssertError(AttributeError, tryAssignToNoneAttr)
AssertError(AttributeError, tryAssignToNoneNotAttr)
v = None.__doc__
v = None.__new__
v = None.__hash__
AssertError(TypeError, type(None))

import sys
AreEqual(str(sys), "<module 'sys' (built-in)>")

import time
import toimport

m = [type(sys), type(time), type(toimport)]
for i in m:
    for j in m:
        Assert(issubclass(i,j))

AssertError(TypeError, type, None, None, None) # arg 1 must be string
AssertError(TypeError, type, "NewType", None, None) # arg 2 must be tuple
AssertError(TypeError, type, "NewType", (), None) # arg 3 must be dict


def splitTest():
    "string".split('')
AssertError(ValueError, splitTest)

#####################################################################################
# IronPython does not allow extending System.Int64 and System.Boolean. So we have
# some directed tests for this.

import System

def InheritFromType(t):
    class InheritedType(t): pass
    return InheritedType

AssertError(TypeError, InheritFromType, System.Int64)
AssertError(TypeError, InheritFromType, System.Boolean)

# isinstance

Assert(isinstance(System.Int64(), System.Int64) == True)
Assert(isinstance(System.Boolean(), System.Boolean) == True)

Assert(isinstance(1, System.Int64) == False)
Assert(isinstance(1, System.Boolean) == False)

class userClass(object): pass
Assert(isinstance(userClass(), System.Int64) == False)
Assert(isinstance(userClass(), System.Boolean) == False)

# issubclass

Assert(issubclass(System.Int64, System.Int64) == True)
Assert(issubclass(System.Boolean, System.Boolean) == True)

Assert(issubclass(type(1), System.Int64) == False)
Assert(issubclass(type(1), System.Boolean) == False)

Assert(issubclass(userClass, System.Int64) == False)
Assert(issubclass(userClass, System.Boolean) == False)

#####################################################################################

import System
arrayMapping = {'u': System.Char, 'c': System.Char, 'b': System.SByte, 'h': System.Int16, 'H': System.UInt16, 
                'i': System.Int32, 'I': System.UInt32, 'l': System.Int64, 'L': System.UInt64, 'f': System.Single,
                'd': System.Double }
                
def tryConstructValues(validate, *args):
    for x in arrayMapping.keys():
        # construct from DynamicType
        y = System.Array[arrayMapping[x]](*args)
        AreEqual(y.GetType().GetElementType(), arrayMapping[x]().GetType())
        validate(y, *args)

        # construct from CLR type
        y = System.Array[y.GetType().GetElementType()](*args)
        AreEqual(y.GetType().GetElementType(), arrayMapping[x]().GetType())
        validate(y, *args)
            

def tryConstructSize(validate, *args):
    for x in arrayMapping.keys():
        # construct from DynamicType
        y = System.Array.CreateInstance(arrayMapping[x], *args)
        AreEqual(y.GetType().GetElementType(), arrayMapping[x]().GetType())
        validate(y, *args)
    
        # construct from CLR type
        y = System.Array.CreateInstance(y.GetType().GetElementType(), *args)
        AreEqual(y.GetType().GetElementType(), arrayMapping[x]().GetType())
        validate(y, *args)
            

def validateLen(res, *args):
    AreEqual(len(res), *args)

def validateVals(res, *args): 
    len(res) == len(args)
    for x in range(len(args[0])):
        try:
            lhs = int(res[x])
            rhs = int(args[0][x])
        except:
            lhs = float(res[x])
            rhs = float(args[0][x])
        AreEqual(lhs, rhs)

def validateValsIter(res, *args): 
    len(res) == len(args)    
    for x in range(len(args)):
        print int(res[x]), args[0][x]
        AreEqual(int(res[x]), int(args[0][x]))
    
    
class MyList(object):
    def __len__(self): 
        return 4
    def __iter__(self):
        yield 3
        yield 4
        yield 5
        yield 6

def validateValsIter(res, *args): 
    compList = MyList()
    len(res) == len(args)    
    index = 0
    for x in compList:
        try:
            lhs = int(res[index])
            rhs = int(x)
        except Exception, e:
            lhs = float(res[index])
            rhs = float(x)
        
        AreEqual(lhs, rhs)
        index += 1
    
    
        
tryConstructSize(validateLen, 0)
tryConstructSize(validateLen, 1)
tryConstructSize(validateLen, 20)

tryConstructValues(validateVals, (3,4,5,6))
tryConstructValues(validateVals, [3,4,5,6])

tryConstructValues(validateValsIter, MyList())


#############################################
# metaclass tests

# normal meta class construction & initialization
metaInit = False
instInit = False
class MetaType(type):
    def someFunction(cls): 
        return "called someFunction"
    def __init__(cls, name, bases, dct):
        global metaInit
        metaInit = True
        super(MetaType, cls).__init__(name, bases,dct)
        cls.xyz = 'abc'
        
class MetaInstance(object):
    __metaclass__ = MetaType
    def __init__(self):
        global instInit
        instInit = True
        
a = MetaInstance()
AreEqual(metaInit, True)
AreEqual(instInit, True)

AreEqual(MetaInstance.xyz, 'abc')
AreEqual(MetaInstance.someFunction(), "called someFunction")


class MetaInstance(object):
    __metaclass__ = MetaType
    def __init__(self, xyz):
        global instInit
        instInit = True
        self.val = xyz
        
metaInit = False
instInit = False

a = MetaInstance('def')
AreEqual(instInit, True)
AreEqual(MetaInstance.xyz, 'abc')
AreEqual(a.val, 'def')
AreEqual(MetaInstance.someFunction(), "called someFunction")

# initialization by calling the metaclass type.

Foo = MetaType('foo', (), {})

AreEqual(type(Foo), MetaType)

instInit = False
def newInit(self):
    global instInit
    instInit = True
    
Foo = MetaType('foo', (), {'__init__':newInit})
a = Foo()
AreEqual(instInit, True)
    
#################################################################    
# check arrays w/ non-zero lower bounds

#single dimension

def ArrayEqual(a,b):
    AreEqual(a.Length, b.Length)
    for x in xrange(a.Length):
        AreEqual(a[x], b[x])
        
a = System.Array.CreateInstance(int, (5,), (5,))
for i in xrange(5): a[i] = i

ArrayEqual(a[:2], System.Array[int]((0,1)))
ArrayEqual(a[2:], System.Array[int]( (2,3,4)))
ArrayEqual(a[2:4], System.Array[int]((2,3)))
AreEqual(a[-1], 4)

x = repr(a)
AreEqual(x, 'System.Int32[*](0, 1, 2, 3, 4)')

a = System.Array.CreateInstance(int, (5,), (5,))
b = System.Array.CreateInstance(int, (5,), (5,))

ArrayEqual(a,b)


a = System.Array.CreateInstance(int, (2,2,2,2,2), (1,2,3,4,5))
AreEqual(a[0,0,0,0,0], 0)

for i in range(5):    
    index = [0,0,0,0,0]
    index[i] = 1    
    
    a[index[0], index[1], index[2], index[3], index[4]] = i
    AreEqual(a[index[0], index[1], index[2], index[3], index[4]], i)
    
    
for i in range(5):    
    index = [0,0,0,0,0]
    index[i] = 0
    
    a[index[0], index[1], index[2], index[3], index[4]] = i
    AreEqual(a[index[0], index[1], index[2], index[3], index[4]], i)


def sliceArray(arr, index):
    arr[:index]


def sliceArrayAssign(arr, index, val):
    arr[:index] = val

AssertError(NotImplementedError, sliceArray, a, 1)
AssertError(NotImplementedError, sliceArray, a, 200)
AssertError(NotImplementedError, sliceArray, a, -200)
AssertError(NotImplementedError, sliceArrayAssign, a, -200, 1)
AssertError(NotImplementedError, sliceArrayAssign, a, 1, 1)


# verify two instances of an old class compare differently

class C: pass

a = C()
b = C()

AreEqual(cmp(a,b) == 0, False)

#################################################################    
# check that unhashable types cannot be hashed by Python
# However, they should be hashable using System.Object.GetHashCode

class OldUserClass:
    def foo(): pass
import _weakref
import collections

AssertError(TypeError, hash, slice(None))
hashcode = System.Object.GetHashCode(slice(None))

# weakproxy
AssertError(TypeError, hash, _weakref.proxy(OldUserClass()))
hashcode = System.Object.GetHashCode(_weakref.proxy(OldUserClass()))

# weakcallableproxy
AssertError(TypeError, hash, _weakref.proxy(OldUserClass().foo))
hashcode = System.Object.GetHashCode(_weakref.proxy(OldUserClass().foo))

AssertError(TypeError, hash, collections.deque())
hashcode = System.Object.GetHashCode(collections.deque())

AssertError(TypeError, hash, dict())
hashcode = System.Object.GetHashCode(dict())

AssertError(TypeError, hash, list())
hashcode = System.Object.GetHashCode(list())

AssertError(TypeError, hash, set())
hashcode = System.Object.GetHashCode(set())

#################################################################    
# Check that attributes of built-in types cannot be deleted

def AssignMethodOfBuiltin():
    def mylen(): pass
    l = list()
    l.len = mylen
AssertError(AttributeError, AssignMethodOfBuiltin)

def DeleteMethodOfBuiltin():
    l = list()
    del l.len
AssertError(AttributeError, DeleteMethodOfBuiltin)

def SetAttrOfBuiltin():
    l = list()
    l.attr = 1
AssertError(AttributeError, SetAttrOfBuiltin)

def SetDictElementOfBuiltin():
    l = list()
    l.__dict__["attr"] = 1
AssertError(AttributeError, SetDictElementOfBuiltin)

def SetAttrOfCLIType():
    d = System.DateTime()
    d.attr = 1
AssertError(AttributeError, SetAttrOfCLIType)

def SetDictElementOfCLIType():
    d = System.DateTime()
    d.__dict__["attr"] = 1
AssertError(AttributeError, SetDictElementOfCLIType)

AssertErrorWithMessage(TypeError, "vars() argument must have __dict__ attribute", vars, list())
AssertErrorWithMessage(TypeError, "vars() argument must have __dict__ attribute", vars, System.DateTime())

#################################################################    
# verify a class w/ explicit interface implementation gets
# it's interfaces shown


import System
AreEqual('32'.ToDouble(None), 32.0)


#################################################################    
# Value types are now immutable (at least through attribute sets)

load_iron_python_test()
from IronPythonTest import *

direct_vt = MySize(1, 2)
embedded_vt = BaseClass()
embedded_vt.Width = 3
embedded_vt.Height = 4

# Read access should still succeed.
AreEqual(direct_vt.width, 1)
AreEqual(embedded_vt.size.width, 3)
AreEqual(embedded_vt.Size.width, 3)

# But writes to value type fields should fail with ValueError.
success = 0
try:
    direct_vt.width = 5
except ValueError:
    success = 1
Assert(success == 1 and direct_vt.width == 1)

success = 0
try:
    embedded_vt.size.width = 5
except ValueError:
    success = 1
Assert(success == 1 and embedded_vt.size.width == 3)

success = 0
try:
    embedded_vt.Size.width = 5
except ValueError:
    success = 1
Assert(success == 1 and embedded_vt.Size.width == 3)

if is_cli:
    import clr
    # ensure .GetType() and calling the helper w/ the type work
    AreEqual(clr.GetClrType(str), ''.GetType())
    # and ensure we're not just auto-converting back on both of them
    Assert(clr.GetClrType(str) != str)


# types are always true.
types = [str, int, long, float, bool]
for x in types:
    if not x: AreEqual(True, False)

# verify we can't create *Ops classes    
from IronPython.Runtime import FloatOps
AssertError(TypeError, FloatOps)
    

# setting mro to an invalid value should result in
# bases still being correct
class foo(object): pass

class bar(foo): pass

class baz(foo): pass

def changeBazBase():
    baz.__bases__ = (foo, bar)  # illegal MRO

AssertError(TypeError, changeBazBase)

AreEqual(baz.__bases__, (foo, ))
AreEqual(baz.__mro__, (baz, foo, object))

d = {}
d[None, 1] = 2
AreEqual(d, {(None, 1): 2})

#######################################################
# Test for type of System.Int32.MinValue
AreEqual(type(-2147483648), int)
AreEqual(type(-(2147483648)), long)
AreEqual(type(-2147483648L), long)
AreEqual(type(-0x80000000), long)

AreEqual(type(int('-2147483648')), int)
AreEqual(type(int('-80000000', 16)), int)
AreEqual(type(int('-2147483649')), long)
AreEqual(type(int('-80000001', 16)), long)



import clr
import System

# verify our str.split doesn't replace CLR's String.Split

res = 'a b  c'.Split([' '], System.StringSplitOptions.RemoveEmptyEntries)
AreEqual(res[0], 'a')
AreEqual(res[1], 'b')
AreEqual(res[2], 'c')


#######################################################
# MRO Tests

# valid
class C(object): pass

class D(object): pass

class E(D): pass

class F(C, E): pass

AreEqual(F.__mro__, (F,C,E,D,object))

# valid
class A(object): pass

class B(object): pass

class C(A,B): pass

class D(A,B): pass

class E(C,D): pass

AreEqual(E.__mro__, (E,C,D,A,B,object))

# invalid
class A(object): pass

class B(object): pass

class C(A,B): pass

class D(B,A): pass

try:
    class E(C,D): pass
    AreEqual(True, False)
except TypeError:
    pass
    

#######################################################
# calling a type w/ kw-args
AreEqual(complex(real=2), (2+0j))


#######################################################
try:
    2.0 + "2.0"
    AreEqual(True, False)
except TypeError: pass

#### (sometype).__class__ should be defined and correct

class foo(object): pass

AreEqual(foo.__class__, type)

class foo(type): pass

class bar(object):
    __metaclass__ = foo
    
AreEqual(bar.__class__, foo)


#### metaclass order:

metaCalled = []
class BaseMeta(type):
    def __new__(cls, name, bases, dict):
        global metaCalled
        metaCalled.append(cls)
        return type.__new__(cls, name, bases, dict)
        
class DerivedMeta(BaseMeta): pass

class A:
    __metaclass__ = BaseMeta
    
AreEqual(metaCalled, [BaseMeta])

metaCalled = []
    
class B: 
    __metaclass__ = DerivedMeta
    
AreEqual(metaCalled, [DerivedMeta])    

metaCalled = []
class C(A,B): pass

AreEqual(metaCalled, [BaseMeta, DerivedMeta])
AreEqual(type(C).__name__, 'DerivedMeta')


    