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

"""Test cases for class-related features specific to CLI"""

from lib.assert_util import *
import clr
import System

load_iron_python_test()

from IronPythonTest import *


def test_inheritance():
    import System
    class MyList(System.Collections.ArrayList):
        def get0(self):
            return self[0]

    l = MyList()
    index = l.Add(22)
    Assert(l.get0() == 22)

def test_interface_inheritance():
    #
    # Verify we can inherit from a class that inherits from an interface
    #

    class MyComparer(System.Collections.IComparer):
        def Compare(self, x, y): return 0
    class MyDerivedComparer(MyComparer): pass
    class MyFurtherDerivedComparer(MyDerivedComparer): pass

    # Check that MyDerivedComparer and MyFurtherDerivedComparer can be used as an IComparer
    s = System.Collections.SortedList(MyComparer())
    s = System.Collections.SortedList(MyDerivedComparer())
    s = System.Collections.SortedList(MyFurtherDerivedComparer())
    
def test_bases():    
    #
    # Verify that changing __bases__ works
    #
    
    class MyExceptionComparer(System.Exception, System.Collections.IComparer):
        def Compare(self, x, y): return 0
    class MyDerivedExceptionComparer(MyExceptionComparer): pass
    
    e = MyExceptionComparer()
   
    MyDerivedExceptionComparer.__bases__ = (System.Exception, System.Collections.IComparer)
    MyDerivedExceptionComparer.__bases__ = (MyExceptionComparer,)
    
    class OldType:
        def OldTypeMethod(self): return "OldTypeMethod"
    class NewType:
        def NewTypeMethod(self): return "NewTypeMethod"
    class MyOtherExceptionComparer(System.Exception, System.Collections.IComparer, OldType, NewType):
        def Compare(self, x, y): return 0
    MyExceptionComparer.__bases__ = MyOtherExceptionComparer.__bases__
    AreEqual(e.OldTypeMethod(), "OldTypeMethod")
    AreEqual(e.NewTypeMethod(), "NewTypeMethod")
    Assert(isinstance(e, System.Exception))
    Assert(isinstance(e, System.Collections.IComparer))
    Assert(isinstance(e, MyExceptionComparer))
    
    class MyIncompatibleExceptionComparer(System.Exception, System.Collections.IComparer, System.IDisposable):
        def Compare(self, x, y): return 0
        def Displose(self): pass
    AssertErrorWithMatch(TypeError, "__bases__ assignment: 'MyExceptionComparer' object layout differs from 'IronPython.NewTypes.System.Exception#IComparer#IDisposable_*",
                         setattr, MyExceptionComparer, "__bases__", MyIncompatibleExceptionComparer.__bases__)
    AssertErrorWithMatch(TypeError, "__class__ assignment: 'MyExceptionComparer' object layout differs from 'IronPython.NewTypes.System.Exception#IComparer#IDisposable_*",
                         setattr, MyExceptionComparer(), "__class__", MyIncompatibleExceptionComparer().__class__)

def test_open_generic():    
    # Inherting from an open generic instantiation should fail with a good error message
    try:
        class Foo(System.Collections.Generic.IEnumerable): pass
    except TypeError:
        (exc_type, exc_value, exc_traceback) = sys.exc_info()
        Assert(exc_value.msg.__contains__("cannot inhert from open generic instantiation"))

def test_interface_slots():
    import System
    
    # slots & interfaces
    class foo(object):
        __slots__ = ['abc']
    
    class bar(foo, System.IComparable):
        def CompareTo(self, other):
                return 23
    
    class baz(bar): pass


def test_symbol_dict():
    """tests to verify that Symbol dictionaries do the right thing in dynamic scenarios
    same as the tests in test_class, but we run this in a module that has imported clr"""
    
    def CheckDictionary(C): 
        # add a new attribute to the type...
        C.newClassAttr = 'xyz'
        AreEqual(C.newClassAttr, 'xyz')
        
        # add non-string index into the class and instance dictionary
        a = C()
        a.__dict__[1] = '1'
        C.__dict__[2] = '2'
        AreEqual(a.__dict__.has_key(1), True)
        AreEqual(C.__dict__.has_key(2), True)
        AreEqual(dir(a).__contains__(1), True)
        AreEqual(dir(a).__contains__(2), True)
        AreEqual(dir(C).__contains__(2), True)
        AreEqual(repr(a.__dict__), "{1: '1'}")
        AreEqual(repr(C.__dict__).__contains__("2: '2'"), True)
        
        # replace a class dictionary (containing non-string keys) w/ a normal dictionary
        C.newTypeAttr = 1
        AreEqual(hasattr(C, 'newTypeAttr'), True)
        
        class OldClass: pass
        
        if isinstance(C, type(OldClass)):
            C.__dict__ = dict(C.__dict__)  
            AreEqual(hasattr(C, 'newTypeAttr'), True)
        else:
            try:
                C.__dict__ = {}
                AreEqual(True, False)
            except TypeError:
                pass
        
        # replace an instance dictionary (containing non-string keys) w/ a new one.
        a.newInstanceAttr = 1
        AreEqual(hasattr(a, 'newInstanceAttr'), True)
        a.__dict__  = dict(a.__dict__)
        AreEqual(hasattr(a, 'newInstanceAttr'), True)
    
        a.abc = 'xyz'  
        AreEqual(hasattr(a, 'abc'), True)
        AreEqual(getattr(a, 'abc'), 'xyz')
        
    
    class OldClass: 
        def __init__(self):  pass
    
    class NewClass(object): 
        def __init__(self):  pass
    
    CheckDictionary(OldClass)
    CheckDictionary(NewClass)

def test_unbound_generic_repr():
    AreEqual(repr(System.IComparable), "<types 'IComparable', 'IComparable[T]'>")

def test_type_descs():
    test = TypeDescTests()
    
    # new style tests
    
    class bar(int): pass
    b = bar(2)
    
    class foo(object): pass
    c = foo()
    
    
    #test.TestProperties(...)
    
    res = test.GetClassName(test)
    Assert(res == 'IronPythonTest.TypeDescTests')
    
    #res = test.GetClassName(a)
    #Assert(res == 'list')
    
    
    res = test.GetClassName(c)
    Assert(res == 'foo')
    
    res = test.GetClassName(b)
    Assert(res == 'bar')
    
    res = test.GetConverter(b)
    x = res.ConvertTo(None, None, b, int)
    Assert(x == 2)
    Assert(type(x) == int)
    
    x = test.GetDefaultEvent(b)
    Assert(x == None)
    
    x = test.GetDefaultProperty(b)
    Assert(x == None)
    
    x = test.GetEditor(b, object)
    Assert(x == None)
    
    x = test.GetEvents(b)
    Assert(x.Count == 0)
    
    x = test.GetEvents(b, None)
    Assert(x.Count == 0)
    
    x = test.GetProperties(b)
    Assert(x.Count > 0)
    
    Assert(test.TestProperties(b, ['__doc__'], []))
    b.foobar = 'hello'
    Assert(test.TestProperties(b, ['__doc__','foobar'], []))
    b.baz = 'goodbye'
    Assert(test.TestProperties(b, ['__doc__','foobar', 'baz'], []))
    delattr(b, 'baz')
    Assert(test.TestProperties(b, ['__doc__','foobar'], ['baz']))
    # Check that adding a non-string entry in the dictionary does not cause any grief.
    b.__dict__[1] = 1;
    Assert(test.TestProperties(b, ['__doc__','foobar'], ['baz']))
    
    #Assert(test.TestProperties(test, ['GetConverter', 'GetEditor', 'GetEvents', 'GetHashCode'] , []))
    
    
    # old style tests
    
    class foo: pass
    
    a = foo()
    
    Assert(test.TestProperties(a, ['__doc__', '__module__'], []))
    
    
    res = test.GetClassName(a)
    Assert(res == 'foo')
    
    
    x = test.CallCanConvertToForInt(a)
    Assert(x == False)
    
    x = test.GetDefaultEvent(a)
    Assert(x == None)
    
    x = test.GetDefaultProperty(a)
    Assert(x == None)
    
    x = test.GetEditor(a, object)
    Assert(x == None)
    
    x = test.GetEvents(a)
    Assert(x.Count == 0)
    
    x = test.GetEvents(a, None)
    Assert(x.Count == 0)
    
    x = test.GetProperties(a)
    Assert(x.Count > 0)
    
    a.bar = 'hello'
    
    Assert(test.TestProperties(a, ['__doc__', '__module__', 'bar'], []))
    delattr(a, 'bar')
    Assert(test.TestProperties(a, ['__doc__', '__module__'], ['bar']))
    
    a = a.__class__
    
    Assert(test.TestProperties(a, ['__doc__', '__module__'], []))
    
    a.bar = 'hello'
    
    Assert(test.TestProperties(a, ['__doc__', '__module__','bar'], []))
    delattr(a, 'bar')
    Assert(test.TestProperties(a, ['__doc__', '__module__'], ['bar']))
    
    x = test.GetClassName(a)
    Assert(x == 'IronPython.Runtime.Types.OldClass')
    
    x = test.CallCanConvertToForInt(a)
    Assert(x == False)
    
    x = test.GetDefaultEvent(a)
    Assert(x == None)
    
    x = test.GetDefaultProperty(a)
    Assert(x == None)
    
    x = test.GetEditor(a, object)
    Assert(x == None)
    
    x = test.GetEvents(a)
    Assert(x.Count == 0)
    
    x = test.GetEvents(a, None)
    Assert(x.Count == 0)
    
    x = test.GetProperties(a)
    Assert(x.Count > 0)

if is_cli: 
    run_test(__name__)
