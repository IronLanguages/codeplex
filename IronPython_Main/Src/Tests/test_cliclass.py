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

"""Test cases for class-related features specific to CLI"""
from __future__ import with_statement

from iptest.assert_util import *
skiptest("win32")
import clr
import System

load_iron_python_test()

from IronPythonTest import *

def test_inheritance():
    import System
    class MyList(System.Collections.Generic.List[int]):
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
    array = System.Array[int](range(10))
    
    System.Array.Sort(array, 0, 10, MyComparer())
    System.Array.Sort(array, 0, 10, MyDerivedComparer())
    System.Array.Sort(array, 0, 10, MyFurtherDerivedComparer())
    
def test_inheritance_generic_method():
    #
    # Verify we can inherit from an interface containing a generic method
    #

    class MyGenericMethods(IGenericMethods):
        def Factory0(self, TParam = None):
            self.type = clr.GetClrType(TParam).FullName
            return TParam("123")
        def Factory1(self, x, T):
            self.type = clr.GetClrType(T).FullName
            return T("456") + x
        def OutParam(self, x, T):
            x.Value = T("2")
            return True
        def RefParam(self, x, T):
            x.Value = x.Value + T("10")
        def Wild(self, *args, **kwargs):
            self.args = args
            self.kwargs = kwargs
            self.args[2].Value = kwargs['T2']('1.5')
            return self.args[3][0]
    
    c = MyGenericMethods()
    AreEqual(GenericMethodTester.TestIntFactory0(c), 123)
    AreEqual(c.type, 'System.Int32')
    
    AreEqual(GenericMethodTester.TestStringFactory1(c, "789"), "456789")
    AreEqual(c.type, 'System.String')
    
    AreEqual(GenericMethodTester.TestIntFactory1(c, 321), 777)
    AreEqual(c.type, 'System.Int32')
    
    AreEqual(GenericMethodTester.TestStringFactory0(c), '123')
    AreEqual(c.type, 'System.String')
    
    AreEqual(GenericMethodTester.TestOutParamString(c), '2')
    AreEqual(GenericMethodTester.TestOutParamInt(c), 2)
    
    AreEqual(GenericMethodTester.TestRefParamString(c, '10'), '1010')
    AreEqual(GenericMethodTester.TestRefParamInt(c, 10), 20)
    
    x = System.Collections.Generic.List[int]((2, 3, 4))
    r = GenericMethodTester.GoWild(c, True, 'second', x)
    AreEqual(r.Length, 2)
    AreEqual(r[0], 1.5)

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
    if not is_silverlight:
        AssertErrorWithMatch(TypeError, "__bases__ assignment: 'MyExceptionComparer' object layout differs from 'IronPython.NewTypes.System.Exception#IComparer#IDisposable_*",
                             setattr, MyExceptionComparer, "__bases__", MyIncompatibleExceptionComparer.__bases__)
        AssertErrorWithMatch(TypeError, "__class__ assignment: 'MyExceptionComparer' object layout differs from 'IronPython.NewTypes.System.Exception#IComparer#IDisposable_*",
                             setattr, MyExceptionComparer(), "__class__", MyIncompatibleExceptionComparer().__class__)
    else:
        try:
            setattr(MyExceptionComparer, "__bases__", MyIncompatibleExceptionComparer.__bases__)
        except TypeError, e:
            Assert(e.args[0].startswith("__bases__ assignment: 'MyExceptionComparer' object layout differs from 'IronPython.NewTypes.System.Exception#IComparer#IDisposable_"))
        
        try:
            setattr(MyExceptionComparer(), "__class__", MyIncompatibleExceptionComparer().__class__)
        except TypeError, e:
            Assert(e.args[0].startswith("__class__ assignment: 'MyExceptionComparer' object layout differs from 'IronPython.NewTypes.System.Exception#IComparer#IDisposable_"))


def test_open_generic():    
    # Inherting from an open generic instantiation should fail with a good error message
    try:
        class Foo(System.Collections.Generic.IEnumerable): pass
    except TypeError:
        (exc_type, exc_value, exc_traceback) = sys.exc_info()
        Assert(exc_value.message.__contains__("cannot inhert from open generic instantiation"))

def test_interface_slots():
    import System
    
    # slots & interfaces
    class foo(object):
        __slots__ = ['abc']
    
    class bar(foo, System.IComparable):
        def CompareTo(self, other):
                return 23
    
    class baz(bar): pass

def test_op_Implicit_inheritance():
    """should inherit op_Implicit from base classes"""
    a = NewClass()
    AreEqual(int(a), 1002)
    AreEqual(NewClass.op_Implicit(a), 1002)

def test_symbol_dict():
    """tests to verify that Symbol dictionaries do the right thing in dynamic scenarios
    same as the tests in test_class, but we run this in a module that has imported clr"""
    
    def CheckDictionary(C): 
        # add a new attribute to the type...
        C.newClassAttr = 'xyz'
        AreEqual(C.newClassAttr, 'xyz')
        
        # add non-string index into the class and instance dictionary        
        a = C()
        try:
            a.__dict__[1] = '1'
            C.__dict__[2] = '2'
            AreEqual(a.__dict__.has_key(1), True)
            AreEqual(C.__dict__.has_key(2), True)
            AreEqual(dir(a).__contains__(1), True)
            AreEqual(dir(a).__contains__(2), True)
            AreEqual(dir(C).__contains__(2), True)
            AreEqual(repr(a.__dict__), "{1: '1'}")
            AreEqual(repr(C.__dict__).__contains__("2: '2'"), True)
        except TypeError:
            # new-style classes have dict-proxy, can't do the assignment
            pass 
        
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
                AssertUnreachable()
            except AttributeError:
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

def test_generic_TypeGroup():
    # TypeGroup is used to expose "System.IComparable" and "System.IComparable`1" as "System.IComparable"
    
    # repr
    AreEqual(repr(System.IComparable), "<types 'IComparable', 'IComparable[T]'>")

    # Test member access
    AreEqual(System.IComparable.CompareTo(1,1), 0)
    AreEqual(System.IComparable.CompareTo(1,2), -1)
    AreEqual(System.IComparable[int].CompareTo(1,1), 0)
    AreEqual(System.IComparable[int].CompareTo(1,2), -1)
    Assert(dir(System.IComparable).__contains__("CompareTo"))
    Assert(vars(System.IComparable).keys().__contains__("CompareTo"))

    import IronPythonTest
    genericTypes = IronPythonTest.NestedClass.InnerGenericClass
    
    # IsAssignableFrom is SecurityCritical and thus cannot be called via reflection in silverlight,
    # so disable this in interpreted mode.
    if not (is_silverlight and is_interpreted()):
        # converstion to Type
        Assert(System.Type.IsAssignableFrom(System.IComparable, int))
        AssertError(TypeError, System.Type.IsAssignableFrom, object, genericTypes)

    # Test illegal type instantiation
    try:
        System.IComparable[int, int]
    except ValueError: pass
    else: AssertUnreachable()

    try:
        System.EventHandler(None)
    except TypeError: pass
    else: AssertUnreachable()
    
    def handler():
        pass

    try:
        System.EventHandler(handler)("sender", None)
    except TypeError: pass
    else: AssertUnreachable()    
        
    def handler(s,a):
        pass

    # Test constructor
    if not is_silverlight:
        # GetType is SecurityCritical; can't call via reflection on silverlight
        if not is_interpreted():
            AreEqual(System.EventHandler(handler).GetType(), System.Type.GetType("System.EventHandler"))
        
        # GetGenericTypeDefinition is SecuritySafe, can't call on Silverlight.
        AreEqual(System.EventHandler[System.EventArgs](handler).GetType().GetGenericTypeDefinition(), System.Type.GetType("System.EventHandler`1"))
    
    # Test inheritance
    class MyComparable(System.IComparable):
        def CompareTo(self, other):
            return self.Equals(other)
    myComparable = MyComparable()
    Assert(myComparable.CompareTo(myComparable))
    
    try:
        class MyDerivedClass(genericTypes): pass
    except TypeError: pass
    else: AssertUnreachable()
    
    # Use a TypeGroup to index a TypeGroup
    t = genericTypes[System.IComparable]
    t = genericTypes[System.IComparable, int]
    try:
        System.IComparable[genericTypes]
    except TypeError: pass
    else: AssertUnreachable()

def test_generic_only_TypeGroup():
    try:
        BinderTest.GenericOnlyConflict()
    except System.TypeLoadException, e:
        Assert(str(e).find('requires a non-generic type') != -1)
        Assert(str(e).find('GenericOnlyConflict') != -1)

def test_autodoc():
    from System.Threading import Thread, ThreadStart
    
    Assert(Thread.__doc__.find('Thread(ThreadStart start)') != -1)
    
    #Assert(Thread.__new__.__doc__.find('__new__(cls, ThreadStart start)') != -1)
    
    #AreEqual(Thread.__new__.Overloads[ThreadStart].__doc__, '__new__(cls, ThreadStart start)' + newline)
    

#IronPythonTest.TypeDescTests is not available for silverlight
@skip("silverlight")    
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
    
    Assert(test.TestProperties(b, [], []))
    bar.foobar = property(lambda x: 42)
    Assert(test.TestProperties(b, ['foobar'], []))
    bar.baz = property(lambda x:42)
    Assert(test.TestProperties(b, ['foobar', 'baz'], []))
    delattr(bar, 'baz')
    Assert(test.TestProperties(b, ['foobar'], ['baz']))
    # Check that adding a non-string entry in the dictionary does not cause any grief.
    b.__dict__[1] = 1;
    Assert(test.TestProperties(b, ['foobar'], ['baz']))
    
    #Assert(test.TestProperties(test, ['GetConverter', 'GetEditor', 'GetEvents', 'GetHashCode'] , []))
    
    
    # old style tests
    
    class foo: pass
    
    a = foo()
    
    Assert(test.TestProperties(a, [], []))
    
    
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
    
    x = test.GetProperties(a, (System.ComponentModel.BrowsableAttribute(True), ))
    Assert(x.Count == 0)
    
    foo.bar = property(lambda x:'hello')
    
    Assert(test.TestProperties(a, ['bar'], []))
    delattr(foo, 'bar')
    Assert(test.TestProperties(a, [], ['bar']))
    
    a = a.__class__
    
    Assert(test.TestProperties(a, [], []))
    
    foo.bar = property(lambda x:'hello')
    
    Assert(test.TestProperties(a, [], []))
    delattr(a, 'bar')
    Assert(test.TestProperties(a, [], ['bar']))
    
    x = test.GetClassName(a)
    AreEqual(x, 'classobj')
    
    x = test.CallCanConvertToForInt(a)
    AreEqual(x, False)
    
    x = test.GetDefaultEvent(a)
    AreEqual(x, None)
    
    x = test.GetDefaultProperty(a)
    AreEqual(x, None)
    
    x = test.GetEditor(a, object)
    AreEqual(x, None)
    
    x = test.GetEvents(a)
    AreEqual(x.Count, 0)
    
    x = test.GetEvents(a, None)
    AreEqual(x.Count, 0)
    
    x = test.GetProperties(a)
    Assert(x.Count > 0)

#silverlight does not support System.Char.Parse
@skip("silverlight")
def test_char():
    import System
    for x in range(256):
        c = System.Char.Parse(chr(x))
        AreEqual(c, chr(x))
        AreEqual(chr(x), c)
        
        if c == chr(x): pass
        else: Assert(False)
        
        if not c == chr(x): Assert(False)
        
        if chr(x) == c: pass
        else: Assert(False)
        
        if not chr(x) == c: Assert(False)

def test_repr():
    import clr
    if not is_silverlight:
        clr.AddReference('System.Drawing')
    
        from System.Drawing import Point
    
        AreEqual(repr(Point(1,2)).startswith('<System.Drawing.Point object'), True)
        AreEqual(repr(Point(1,2)).endswith('[{X=1,Y=2}]>'),True)
    
    # these 3 classes define the same repr w/ different \r, \r\n, \n versions
    a = UnaryClass(3)
    b = BaseClass()
    c = BaseClassStaticConstructor()
    
    ra = repr(a)
    rb = repr(b)
    rc = repr(c)
    
    sa = ra.find('HelloWorld')
    sb = rb.find('HelloWorld')
    sc = rc.find('HelloWorld')
    
    AreEqual(ra[sa:sa+13], rb[sb:sb+13])
    AreEqual(rb[sb:sb+13], rc[sc:sc+13])
    AreEqual(ra[sa:sa+13], 'HelloWorld...') # \r\n should be removed, replaced with ...

def test_explicit_interfaces():
    otdc = OverrideTestDerivedClass()
    AreEqual(otdc.MethodOverridden(), "OverrideTestDerivedClass.MethodOverridden() invoked")
    AreEqual(IOverrideTestInterface.MethodOverridden(otdc), 'IOverrideTestInterface.MethodOverridden() invoked')

    AreEqual(IOverrideTestInterface.x.GetValue(otdc), 'IOverrideTestInterface.x invoked')
    AreEqual(IOverrideTestInterface.y.GetValue(otdc), 'IOverrideTestInterface.y invoked')
    IOverrideTestInterface.x.SetValue(otdc, 'abc')
    AreEqual(OverrideTestDerivedClass.Value, 'abcx')
    IOverrideTestInterface.y.SetValue(otdc, 'abc')
    AreEqual(OverrideTestDerivedClass.Value, 'abcy')
    
    AreEqual(otdc.y, 'OverrideTestDerivedClass.y invoked')

    AreEqual(IOverrideTestInterface.Method(otdc), "IOverrideTestInterface.method() invoked")
    
    AreEqual(hasattr(otdc, 'IronPythonTest_IOverrideTestInterface_x'), False)
    
    # we can also do this the ugly way:
    
    AreEqual(IOverrideTestInterface.x.__get__(otdc, OverrideTestDerivedClass), 'IOverrideTestInterface.x invoked')
    AreEqual(IOverrideTestInterface.y.__get__(otdc, OverrideTestDerivedClass), 'IOverrideTestInterface.y invoked')

    AreEqual(IOverrideTestInterface.__getitem__(otdc, 2), 'abc')
    AreEqual(IOverrideTestInterface.__getitem__(otdc, 2), 'abc')
    AssertError(NotImplementedError, IOverrideTestInterface.__setitem__, otdc, 2, 3)
    try:
        IOverrideTestInterface.__setitem__(otdc, 2, 3)
    except NotImplementedError: pass
    else: AssertUnreachable()

def test_field_helpers():
    otdc = OverrideTestDerivedClass()
    OverrideTestDerivedClass.z.SetValue(otdc, 'abc')
    AreEqual(otdc.z, 'abc')
    AreEqual(OverrideTestDerivedClass.z.GetValue(otdc), 'abc')

def test_field_descriptor():
    AreEqual(MySize.width.__get__(MySize()), 0)
    AreEqual(MySize.width.__get__(MySize(), MySize), 0)

def test_field_const_write():
    try:
        MySize.MaxSize = 23
    except AttributeError, e:
        Assert(str(e).find('MaxSize') != -1)
        Assert(str(e).find('MySize') != -1)

    try:
        ClassWithLiteral.Literal = 23
    except AttributeError, e:
        Assert(str(e).find('Literal') != -1)
        Assert(str(e).find('ClassWithLiteral') != -1)

    try:
        ClassWithLiteral.__dict__['Literal'].__set__(None, 23)
    except AttributeError, e:
        Assert(str(e).find('Literal') != -1)
        Assert(str(e).find('ClassWithLiteral') != -1)

    try:
        ClassWithLiteral.__dict__['Literal'].__set__(ClassWithLiteral(), 23)
    except AttributeError, e:
        Assert(str(e).find('Literal') != -1)
        Assert(str(e).find('ClassWithLiteral') != -1)

    try:
        MySize().MaxSize = 23
    except AttributeError, e:
        Assert(str(e).find('MaxSize') != -1)
        Assert(str(e).find('MySize') != -1)

    try:
        ClassWithLiteral().Literal = 23
    except AttributeError, e:
        Assert(str(e).find('Literal') != -1)
        Assert(str(e).find('ClassWithLiteral') != -1)

def test_field_const_access():
    AreEqual(MySize().MaxSize, System.Int32.MaxValue)
    AreEqual(MySize.MaxSize, System.Int32.MaxValue)
    AreEqual(ClassWithLiteral.Literal, 5)
    AreEqual(ClassWithLiteral().Literal, 5)

def test_array():
    import System
    arr = System.Array[int]([0])
    AreEqual(repr(arr), str(arr))
    AreEqual(repr(System.Array[int]([0, 1])), 'Array[int]((0, 1))')


def test_strange_inheritance():
    """verify that overriding strange methods (such as those that take caller context) doesn't
       flow caller context through"""
    class m(StrangeOverrides):
        def SomeMethodWithContext(self, arg):
            AreEqual(arg, 'abc')
        def ParamsMethodWithContext(self, *arg):
            AreEqual(arg, ('abc', 'def'))
        def ParamsIntMethodWithContext(self, *arg):
            AreEqual(arg, (2,3))

    a = m()
    a.CallWithContext('abc')
    a.CallParamsWithContext('abc', 'def')
    a.CallIntParamsWithContext(2, 3)   

#lib.process_util, file, etc are not available in silverlight
@skip("silverlight")
def test_nondefault_indexers():
    from iptest.process_util import *

    if not has_vbc(): return
    import nt
    import _random
    
    r = _random.Random()
    r.seed()
    f = file('vbproptest1.vb', 'w')
    try:
        f.write("""
Public Class VbPropertyTest
private Indexes(23) as Integer
private IndexesTwo(23,23) as Integer
private shared SharedIndexes(5,5) as Integer

Public Property IndexOne(ByVal index as Integer) As Integer
    Get
        return Indexes(index)
    End Get
    Set
        Indexes(index) = Value
    End Set
End Property

Public Property IndexTwo(ByVal index as Integer, ByVal index2 as Integer) As Integer
    Get
        return IndexesTwo(index, index2)
    End Get
    Set
        IndexesTwo(index, index2) = Value
    End Set
End Property

Public Shared Property SharedIndex(ByVal index as Integer, ByVal index2 as Integer) As Integer
    Get
        return SharedIndexes(index, index2)
    End Get
    Set
        SharedIndexes(index, index2) = Value
    End Set
End Property
End Class        
    """)
        f.close()
        
        name = 'vbproptest%f.dll' % (r.random())
        x = run_vbc('/target:library vbproptest1.vb "/out:%s"' % name)        
        AreEqual(x, 0)
        import clr
        clr.AddReferenceToFileAndPath(name)
        import VbPropertyTest
        
        x = VbPropertyTest()
        AreEqual(x.IndexOne[0], 0)
        x.IndexOne[1] = 23
        AreEqual(x.IndexOne[1], 23)
        
        AreEqual(x.IndexTwo[0,0], 0)
        x.IndexTwo[1,2] = 5
        AreEqual(x.IndexTwo[1,2], 5)
        
        AreEqual(VbPropertyTest.SharedIndex[0,0], 0)
        VbPropertyTest.SharedIndex[3,4] = 42
        AreEqual(VbPropertyTest.SharedIndex[3,4], 42)
    finally:
        if not f.closed: f.close()
              
        nt.unlink('vbproptest1.vb')

def test_interface_abstract_events():
    # inherit from an interface or abstract event, and define the event
    for baseType in [IEventInterface, AbstractEvent]:
        class foo(baseType):
            def __init__(self):
                self._events = []            
            def add_MyEvent(self, value):
                AreEqual(type(value), SimpleDelegate)
                self._events.append(value)
            def remove_MyEvent(self, value):
                AreEqual(type(value), SimpleDelegate)
                self._events.remove(value)
            def MyRaise(self):
                for x in self._events: x()
    
        global called
        called = False
        def bar(*args): 
            global called
            called = True
    
        a = foo()
        
        a.MyEvent += bar
        a.MyRaise()
        AreEqual(called, True)
        
        a.MyEvent -= bar        
        called = False        
        a.MyRaise()        
        AreEqual(called, False)
        
        # hook the event from the CLI side, and make sure that raising
        # it causes the CLI side to see the event being fired.
        UseEvent.Hook(a)
        a.MyRaise()
        AreEqual(UseEvent.Called, True)
        UseEvent.Called = False
        UseEvent.Unhook(a)
        a.MyRaise()
        AreEqual(UseEvent.Called, False)

@disabled("Merlin 177188: Fail in Orcas")
def test_dynamic_assembly_ref():
    # verify we can add a reference to a dynamic assembly, and
    # then create an instance of that type
    class foo(object): pass
    import clr
    clr.AddReference(foo().GetType().Assembly)
    import IronPython.NewTypes.System
    for x in dir(IronPython.NewTypes.System):
        if x.startswith('Object_'):
            t = getattr(IronPython.NewTypes.System, x)            
            x = t(foo)
            break
    else:
        # we should have found our type
        AssertUnreachable()

def test_nonzero():
    from System import Single, Byte, SByte, Int16, UInt16, Int64, UInt64
    for t in [Single, Byte, SByte, Int16, UInt16, Int64, UInt64]:
        Assert(hasattr(t, '__nonzero__'))
        if t(0): AssertUnreachable()
        if not t(1): AssertUnreachable()

def test_virtual_event():
    # inherit from a class w/ a virtual event and a
    # virtual event that's been overridden.  Check both
    # overriding it and not overriding it.
    for baseType in [VirtualEvent, OverrideVirtualEvent]:
        for override in [True, False]:
            class foo(baseType):
                def __init__(self):
                    self._events = []            
                if override:
                    def add_MyEvent(self, value):
                        AreEqual(type(value), SimpleDelegate)
                        self._events.append(value)
                    def remove_MyEvent(self, value):
                        AreEqual(type(value), SimpleDelegate)
                        self._events.remove(value)
                    def add_MyCustomEvent(self, value): pass
                    def remove_MyCustomEvent(self, value): pass
                    def MyRaise(self):
                        for x in self._events: x()
                else:
                    def MyRaise(self):
                        self.FireEvent()                    

            # normal event
            global called
            called = False
            def bar(*args): 
                global called
                called = True
                        
            a = foo()
            a.MyEvent += bar
            a.MyRaise()
                
            AreEqual(called, True)
            
            a.MyEvent -= bar
            
            called = False            
            a.MyRaise()            
            AreEqual(called, False)
        
            # custom event
            a.LastCall = None
            a = foo()
            a.MyCustomEvent += bar
            if override: AreEqual(a.LastCall, None)
            else: Assert(a.LastCall.endswith('Add'))
            
            a.Lastcall = None            
            a.MyCustomEvent -= bar
            if override: AreEqual(a.LastCall, None)
            else: Assert(a.LastCall.endswith('Remove'))


            # hook the event from the CLI side, and make sure that raising
            # it causes the CLI side to see the event being fired.
            UseEvent.Hook(a)
            a.MyRaise()
            AreEqual(UseEvent.Called, True)
            UseEvent.Called = False
            UseEvent.Unhook(a)
            a.MyRaise()
            AreEqual(UseEvent.Called, False)

@skip("silverlight")
def test_property_get_set():
    clr.AddReference("System.Drawing")
    from System.Drawing import Size
    
    temp = Size()
    AreEqual(temp.Width, 0)
    temp.Width = 5
    AreEqual(temp.Width, 5)
        
    for i in xrange(5):
        temp.Width = i
        AreEqual(temp.Width, i)    

def test_write_only_property_set():
    from IronPythonTest import WriteOnly
    obj = WriteOnly()
    
    AssertError(AttributeError, getattr, obj, 'Writer')

def test_isinstance_interface():
    import System
    Assert(isinstance('abc', System.Collections.IEnumerable))

def test_constructor_function():
    '''
    Test to hit IronPython.Runtime.Operations.ConstructionFunctionOps.
    '''
    
    AreEqual(System.DateTime.__new__.__name__, '__new__')
    Assert(System.DateTime.__new__.__doc__.find('__new__(cls, int year, int month, int day)') != -1)
                
    if not is_silverlight:
        Assert(System.AssemblyLoadEventArgs.__new__.__doc__.find('__new__(cls, Assembly loadedAssembly)') != -1)

def test_class_property():
    """__class__ should work on standard .NET types and should return the type object associated with that class"""
    import System
    AreEqual(System.Environment.Version.__class__, System.Version)

def test_null_str():
    """if a .NET type has a bad ToString() implementation that returns null always return String.Empty in Python"""
    AreEqual(str(RudeObjectOverride()), '')
    AreEqual(RudeObjectOverride().__str__(), '')
    AreEqual(RudeObjectOverride().ToString(), None)
    Assert(repr(RudeObjectOverride()).startswith('<IronPythonTest.RudeObjectOverride object at '))

def test_keyword_construction_readonly():
    import System
    # Build is read-only property
    AssertError(AttributeError, System.Version, 1, 0, Build=100)  
    AssertError(AttributeError, ClassWithLiteral, Literal=3)

@skip("silverlight") # no FileSystemWatcher in Silverlight
def test_kw_construction_types():
    import System
    for val in [True, False]:
        x = System.IO.FileSystemWatcher('.', EnableRaisingEvents = val)
        AreEqual(x.EnableRaisingEvents, val)

def test_as_bool():
    """verify using expressions in if statements works correctly.  This generates an
    site whose return type is bool so it's important this works for various ways we can
    generate the body of the site, and use the site both w/ the initial & generated targets"""
    import clr
    clr.AddReference('System') # ensure test passes in ipt
    import System
    
    # instance property
    x = System.Uri('http://foo')
    for i in xrange(2):
        if x.AbsolutePath: pass
        else: AssertUnreachable()
    
    # instance property on type
    for i in xrange(2):
        if System.Uri.AbsolutePath: pass
        else: AssertUnreachable()
    
    # static property
    for i in xrange(2):
        if System.Threading.Thread.CurrentThread: pass
        else: AssertUnreachable()
    
    # static field
    for i in xrange(2):
        if System.String.Empty: AssertUnreachable()
    
    # instance field
    x = NestedClass()
    for i in xrange(2):
        if x.Field: AssertUnreachable()        
    
    # instance field on type
    for i in xrange(2):
        if NestedClass.Field: pass
        else: AssertUnreachable()
    
    # math
    for i in xrange(2):
        if System.Int64(1) + System.Int64(1): pass
        else: AssertUnreachable()

    for i in xrange(2):
        if System.Int64(1) + System.Int64(1): pass
        else: AssertUnreachable()
    
    # GetItem
    x = System.Collections.Generic.List[str]()
    x.Add('abc')
    for i in xrange(2):
        if x[0]: pass
        else: AssertUnreachable()
        
    
    
@skip("silverlight") # no Stack on Silverlight
def test_generic_getitem():
    # calling __getitem__ is the same as indexing
    import System
    AreEqual(System.Collections.Generic.Stack.__getitem__(int), System.Collections.Generic.Stack[int])
    
    # but __getitem__ on a type takes precedence
    AssertError(TypeError, System.Collections.Generic.List.__getitem__, int)
    x = System.Collections.Generic.List[int]()
    x.Add(0)
    AreEqual(System.Collections.Generic.List[int].__getitem__(x, 0), 0)
    
    # but we can call type.__getitem__ with the instance    
    AreEqual(type.__getitem__(System.Collections.Generic.List, int), System.Collections.Generic.List[int])
    

@skip("silverlight") # no WinForms on Silverlight
def test_multiple_inheritance():
    """multiple inheritance from two types in the same hierarchy should work, this is similar to class foo(int, object)"""
    clr.AddReference("System.Windows.Forms")
    import System
    class foo(System.Windows.Forms.Form, System.Windows.Forms.Control): pass
    
def test_struct_no_ctor_kw_args():
    for x in range(2):
        s = Structure(a=3)
        AreEqual(s.a, 3)

def test_nullable_new():
    from System import Nullable
    AreEqual(clr.GetClrType(Nullable[()]).IsGenericType, False)

def test_ctor_keyword_args_newslot():
    """ctor keyword arg assignment contruction w/ new slot properties"""
    x = BinderTest.KeywordDerived(SomeProperty = 'abc')
    AreEqual(x.SomeProperty, 'abc')

    x = BinderTest.KeywordDerived(SomeField = 'abc')
    AreEqual(x.SomeField, 'abc')

def test_enum_truth():
    # zero enums are false, non-zero enums are true
    import System
    Assert(not System.StringSplitOptions.None)
    Assert(System.StringSplitOptions.RemoveEmptyEntries)
    AreEqual(System.StringSplitOptions.None.__nonzero__(), False)
    AreEqual(System.StringSplitOptions.RemoveEmptyEntries.__nonzero__(), True)

def test_bad_inheritance():
    """verify a bad inheritance reports the type name you're inheriting from"""
    import System
    def f(): 
        class x(System.Single): pass
    def g(): 
        class x(System.Version): pass
    
    AssertErrorWithPartialMessage(TypeError, 'System.Single', f)
    AssertErrorWithPartialMessage(TypeError, 'System.Version', g)

def test_disposable():
    """classes implementing IDisposable should automatically support the with statement"""
    x = DisposableTest()
    
    with x:
        pass
        
    AreEqual(x.Called, True)
    
    Assert(hasattr(x, '__enter__'))
    Assert(hasattr(x, '__exit__'))

    x = DisposableTest()
    x.__enter__()
    try:
        pass
    finally:
        AreEqual(x.__exit__(None, None, None), None)
    
    AreEqual(x.Called, True)
    
    Assert('__enter__' in dir(x))
    Assert('__exit__' in dir(x))
    Assert('__enter__' in dir(DisposableTest))
    Assert('__exit__' in dir(DisposableTest))

def test_dbnull():
    """DBNull should not be true"""
    
    if System.DBNull.Value:
        AssertUnreachable()


def test_special_repr():
    list = System.Collections.Generic.List[object]()
    AreEqual(repr(list), 'List[object]()')
    
    list.Add('abc')    
    AreEqual(repr(list), "List[object](['abc'])")
    
    list.Add(2)
    AreEqual(repr(list), "List[object](['abc', 2])")
    
    list.Add(list)
    AreEqual(repr(list), "List[object](['abc', 2, [...]])")
    
    dict = System.Collections.Generic.Dictionary[object, object]()
    AreEqual(repr(dict), "Dictionary[object, object]()")
    
    dict["abc"] = "def"
    AreEqual(repr(dict), "Dictionary[object, object]({'abc' : 'def'})")
    
    dict["two"] = "def"
    Assert(repr(dict) == "Dictionary[object, object]({'abc' : 'def', 'two' : 'def'})" or
           repr(dict) == "Dictionary[object, object]({'two' : 'def', 'def' : 'def'})")
           
    dict = System.Collections.Generic.Dictionary[object, object]()
    dict['abc'] = dict
    AreEqual(repr(dict), "Dictionary[object, object]({'abc' : {...}})")

    dict = System.Collections.Generic.Dictionary[object, object]()
    dict[dict] = 'abc'
    
    AreEqual(repr(dict), "Dictionary[object, object]({{...} : 'abc'})")

def test_issubclass():    
    Assert(issubclass(int, clr.GetClrType(int)))

def test_explicit_interface_impl():
    noConflict = ExplicitTestNoConflict()
    oneConflict = ExplicitTestOneConflict()
    
    AreEqual(noConflict.A(), "A")
    AreEqual(noConflict.B(), "B")
    Assert(hasattr(noConflict, "A"))
    Assert(hasattr(noConflict, "B"))
    
    AssertError(AttributeError, lambda : oneConflict.A())
    AreEqual(oneConflict.B(), "B")
    Assert(not hasattr(oneConflict, "A"))
    Assert(hasattr(oneConflict, "B"))
    
@skip("silverlight") # no ArrayList on Silverlight
def test_interface_isinstance():
    l = System.Collections.ArrayList()
    AreEqual(isinstance(l, System.Collections.IList), True)

@skip("silverlight") # no serialization support in Silverlight
def test_serialization():
    """
    TODO:
    - this should become a test module in and of itself
    - way more to test here..
    """
    
    import cPickle
    import clr
    
    # test the primitive data types...    
    data = [1, 1.0, 2j, 2L, System.Int64(1), System.UInt64(1), 
            System.UInt32(1), System.Int16(1), System.UInt16(1), 
            System.Byte(1), System.SByte(1), System.Decimal(1),
            System.Char.MaxValue, System.DBNull.Value, System.Single(1.0),
            System.DateTime.Now, None, {}, (), [], {'a': 2}, (42, ), [42, ],
            System.StringSplitOptions.RemoveEmptyEntries,
            ]
    
    data.append(list(data))     # list of all the data..
    data.append(tuple(data))    # tuple of all the data...
    
    class X:
        def __init__(self):
            self.abc = 3
    
    class Y(object):
        def __init__(self):
            self.abc = 3

    # instance dictionaries...
    data.append(X().__dict__)
    data.append(Y().__dict__)

    # recursive list
    l = []
    l.append(l)
    data.append(l)
    
    # dict of all the data
    d = {}
    cnt = 100
    for x in data:
        d[cnt] = x
        cnt += 1
        
    data.append(d)
    
    # recursive dict...
    d1 = {}
    d2 = {}
    
    d1['abc'] = d2
    d1['foo'] = 'baz'
    d2['abc'] = d1
    
    data.append(d1)
    data.append(d2)
    
    for value in data:
        # use cPickle & clr.Serialize/Deserialize directly
        for newVal in (cPickle.loads(cPickle.dumps(value)), clr.Deserialize(*clr.Serialize(value))):
            AreEqual(type(newVal), type(value))
            try:
                AreEqual(newVal, value)
            except RuntimeError, e:
                # we hit one of our recursive structures...
                AreEqual(e.message, "maximum recursion depth exceeded in cmp")
                Assert(type(newVal) is list or type(newVal) is dict)
    
    # passing an unknown format raises...
    AssertError(ValueError, clr.Deserialize, "unknown", "foo")
    
    al = System.Collections.ArrayList()
    al.Add(2)
    
    gl = System.Collections.Generic.List[int]()
    gl.Add(2)
    
    # lists...
    for value in (al, gl):
        for newX in (cPickle.loads(cPickle.dumps(value)), clr.Deserialize(*clr.Serialize(value))):
            AreEqual(value.Count, newX.Count)
            for i in xrange(value.Count):
                AreEqual(value[i], newX[i])
    
    ht = System.Collections.Hashtable()
    ht['foo'] = 'bar'
    
    gd = System.Collections.Generic.Dictionary[str, str]()
    gd['foo'] = 'bar'

    # dictionaries
    for value in (ht, gd):
        for newX in (cPickle.loads(cPickle.dumps(value)), clr.Deserialize(*clr.Serialize(value))):
            AreEqual(value.Count, newX.Count)
            for key in value.Keys:
                AreEqual(value[key], newX[key])
                
    # interesting cases
    for tempX in [System.Exception("some message")]:
        for newX in (cPickle.loads(cPickle.dumps(tempX)), clr.Deserialize(*clr.Serialize(tempX))):
            AreEqual(newX.Message, tempX.Message)

    try:
        exec " print 1"
    except Exception, tempX:
        pass
    newX = cPickle.loads(cPickle.dumps(tempX))
    for attr in ['args', 'filename', 'text', 'lineno', 'msg', 'offset', 'print_file_and_line',
                 'message',
                 ]:
        AreEqual(eval("newX.%s" % attr), 
                 eval("tempX.%s" % attr))
    

    class K(System.Exception):
        other = "something else"
    tempX = K()
    #CodePlex 16415
    #for newX in (cPickle.loads(cPickle.dumps(tempX)), clr.Deserialize(*clr.Serialize(tempX))):
    #    AreEqual(newX.Message, tempX.Message)
    #    AreEqual(newX.other, tempX.other)
    
    #CodePlex 16415
    tempX = System.Exception
    #for newX in (cPickle.loads(cPickle.dumps(System.Exception)), clr.Deserialize(*clr.Serialize(System.Exception))):
    #    temp_except = newX("another message")
    #    AreEqual(temp_except.Message, "another message")

def test_generic_method_error():
    import clr
    clr.AddReference('System.Core')
    from System.Linq import Queryable
    AssertErrorWithMessage(TypeError, "Queryable.First is a generic method and must be indexed with types before calling", Queryable.First, [])

def test_collection_length():
    a = GenericCollection()
    AreEqual(len(a), 0)
    a.Add(1)
    AreEqual(len(a), 1)
    
    Assert(hasattr(a, '__len__'))
    
def test_dict_copy():
    Assert(int.__dict__.copy().has_key('MaxValue'))

def test_decimal_bool():
    AreEqual(bool(System.Decimal(0)), False)
    AreEqual(bool(System.Decimal(1)), True)

@skip("silverlight") # no Char.Parse
def test_add_str_char():
    AreEqual('bc' + System.Char.Parse('a'), 'bca')
    AreEqual(System.Char.Parse('a') + 'bc', 'abc')

def test_import_star_enum():
    from System.AttributeTargets import *
    Assert('ReturnValue' in dir())

@skip("silverlight")
def test_cp11971():
    old_syspath = [x for x in sys.path]
    try:
        sys.path.append(testpath.temporary_dir)
        
        #Module using System
        write_to_file(path_combine(testpath.temporary_dir, "cp11971_module.py"), 
                      """def a():
    from System import Array
    return Array.CreateInstance(int, 2, 2)""")

        #Module which doesn't use System directly
        write_to_file(path_combine(testpath.temporary_dir, "cp11971_caller.py"), 
                      """import cp11971_module
A = cp11971_module.a()
if not hasattr(A, 'Rank'):
    raise 'CodePlex 11971'
    """)
    
        #Validations
        import cp11971_caller
        Assert(hasattr(cp11971_caller.A, 'Rank'))
        Assert(hasattr(cp11971_caller.cp11971_module.a(), 'Rank'))
    
    finally:
        sys.path = old_syspath

@skip("silverlight") # no Stack on Silverlight
def test_ienumerable__getiter__():
    
    #--empty list
    called = 0
    x = System.Collections.Generic.List[int]()
    Assert(hasattr(x, "__iter__"))
    for stuff in x:
        called +=1 
    AreEqual(called, 0)
    
    #--add one element to the list
    called = 0
    x.Add(1)
    for stuff in x:
        AreEqual(stuff, 1)
        called +=1
    AreEqual(called, 1)
    
    #--one element list before __iter__ is called
    called = 0
    x = System.Collections.Generic.List[int]()
    x.Add(1)
    for stuff in x:
        AreEqual(stuff, 1)
        called +=1
    AreEqual(called, 1)
    
    #--two elements in the list
    called = 0
    x.Add(2)
    for stuff in x:
        AreEqual(stuff-1, called)
        called +=1
    AreEqual(called, 2)

def test_overload_functions():
    for x in min.Overloads.Functions:
        Assert(x.__doc__.startswith('object min('))
        Assert(x.__doc__.find('CodeContext') == -1)
    # multiple accesses should return the same object
    AreEqual(
        id(min.Overloads[object, object]), 
        id(min.Overloads[object, object])
    )

def test_clr_dir():
    Assert('IndexOf' not in clr.Dir('abc'))
    Assert('IndexOf' in clr.DirClr('abc'))

def test_array_contains():
    AssertError(KeyError, lambda : System.Array[str].__dict__['__contains__'])

def test_underlying_type():
    # simple case, just make sure it's called and we can call super
    global called
    called = None
    class MyType(type):
        def __clrtype__(self):
            global called
            called = True
            return super(MyType, self).__clrtype__()
    
    class x(object):
        __metaclass__ = MyType

    AreEqual(called, True)
    
    import clr
    AddReferenceToDlrCore()
    
    from System import Reflection
    from Microsoft.Scripting.Generation import AssemblyGen
    from System.Reflection import Emit, FieldAttributes
    from System.Reflection.Emit import OpCodes      
    gen = AssemblyGen(Reflection.AssemblyName('test'), None, '.dll', False)
    
    try:
        # more complex case, actually create a new type and override the
        # ctors
        class MyType(type):
            def __clrtype__(self):
                baseType = super(MyType, self).__clrtype__()
                t = gen.DefinePublicType(self.__name__, baseType, True)
                
                ctors = baseType.GetConstructors()
                for ctor in ctors:            
                    builder = t.DefineConstructor(
                        Reflection.MethodAttributes.Public, 
                        Reflection.CallingConventions.Standard, 
                        tuple([p.ParameterType for p in ctor.GetParameters()])
                    )
                    ilgen = builder.GetILGenerator()
                    ilgen.Emit(OpCodes.Ldarg, 0)
                    for index in range(len(ctor.GetParameters())):
                        ilgen.Emit(OpCodes.Ldarg, index + 1)
                    ilgen.Emit(OpCodes.Call, ctor)
                    ilgen.Emit(OpCodes.Ret)
                
                newType = t.CreateType()
                return newType
        
        class x(object):
            __metaclass__ = MyType
            def __init__(self):
                self.abc = 3
              
        a = x()
        AreEqual(a.abc, 3)
        
        # more complex case, make a static .NET type which can be created
        class MyType(type):
            def __clrtype__(self):
                baseType = super(MyType, self).__clrtype__()
                t = gen.DefinePublicType(self.__name__, baseType, True)
                
                ctors = baseType.GetConstructors()
                for ctor in ctors:            
                    baseParams = ctor.GetParameters()
                    newParams = baseParams[1:]
                    
                    builder = t.DefineConstructor(
                        Reflection.MethodAttributes.Public, 
                        Reflection.CallingConventions.Standard, 
                        tuple([p.ParameterType for p in newParams])
                    )
                    fldAttrs = FieldAttributes.Static | FieldAttributes.Public
                    fld = t.DefineField('$$type', type, fldAttrs)
                    
                    ilgen = builder.GetILGenerator()
                    ilgen.Emit(OpCodes.Ldarg, 0)
                    ilgen.Emit(OpCodes.Ldsfld, fld)
                    for index in range(len(ctor.GetParameters())):
                        ilgen.Emit(OpCodes.Ldarg, index + 1)
                    ilgen.Emit(OpCodes.Call, ctor)
                    ilgen.Emit(OpCodes.Ret)

                    # keep a ctor which takes Python types as well so we 
                    # can be called from Python still.
                    builder = t.DefineConstructor(
                        Reflection.MethodAttributes.Public, 
                        Reflection.CallingConventions.Standard, 
                        tuple([p.ParameterType for p in ctor.GetParameters()])
                    )
                    ilgen = builder.GetILGenerator()
                    ilgen.Emit(OpCodes.Ldarg, 0)
                    for index in range(len(ctor.GetParameters())):
                        ilgen.Emit(OpCodes.Ldarg, index + 1)
                    ilgen.Emit(OpCodes.Call, ctor)
                    ilgen.Emit(OpCodes.Ret)

                newType = t.CreateType()
                newType.GetField('$$type').SetValue(None, self)
                return newType
        
        class MyCreatableDotNetType(object):
            __metaclass__ = MyType
            def __init__(self):
                self.abc = 3

        # TODO: Test Type.GetType (requires the base class to be non-transient)
    finally:
        #gen.SaveAssembly()
        pass

def test_a_override_patching():
    clr.AddReference('Microsoft.Scripting.Core')
    # derive from object
    class x(object):
        pass
    
    # force creation of GetHashCode built-in function
    TestHelpers.HashObject(x())

    # derive from a type which overrides GetHashCode
    from Microsoft.Scripting import InvokeBinder
    from System.Linq.Expressions import Expression
    
    class y(InvokeBinder):
        def GetHashCode(self): return super(InvokeBinder, self).GetHashCode()
    
    # now the super call should work & should include the InvokeBinder new type
    TestHelpers.HashObject(y(Expression[()].CallInfo(0)))
    
run_test(__name__)

