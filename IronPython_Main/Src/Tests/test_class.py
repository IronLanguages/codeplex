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

from lib.assert_util import *
from lib.type_util import *

############################################################
def test_common_attributes():
    builtin_type_instances = [None, object(), 1, "Hello", [0,1], {"a":0}]
    builtin_hashable_type_instances = [None, object(), 1, "Hello"]
    builtin_types = [type(None), object, int, str, list, dict]

    for i in builtin_type_instances:
        # Read-only attribute
        AssertError(AttributeError, i.__delattr__, "__doc__")
        # Non-existent attribute
        AssertError(AttributeError, i.__delattr__, "foo")
    # Modifying __class__ causes a TypeError
        AssertError(TypeError, i.__delattr__, "__class__")
    
        # Read-only attribute
        AssertError(TypeError, i.__setattr__, "__doc__")
        # Non-existent attribute
        AssertError(AttributeError, i.__setattr__, "foo", "foovalue")
        # Modifying __class__ causes a TypeError
        AssertError(TypeError, i.__setattr__, "__class__")
        
        AreEqual(type(i), i.__getattribute__("__class__"))
        # Non-existent attribute
        AssertError(AttributeError, i.__getattribute__, "foo")
        
        if (is_cli or is_silverlight) and i == None: # !!! Need to expose __reduce__ on all types
            AssertError(TypeError, i.__reduce__)
            AssertError(TypeError, i.__reduce_ex__)
    
    for i in builtin_hashable_type_instances:
        AreEqual(hash(i), i.__hash__())
            
    for i in builtin_types:
          if (is_cli or is_silverlight) and i == type(None):
              continue
          # __init__ and __new__ are implemented by IronPython.Runtime.Operations.InstanceOps
          # We do repr to ensure that we can map back the functions properly
          repr(getattr(i, "__init__"))
          repr(getattr(i, "__new__"))
            
############################################################
def test_set_dict():
    class C: pass
    setdict = C.__dict__
    C.__dict__ = setdict
    
    o1 = C()

    class C:
        def m(self):
            return 42
    
    o2 = C()
    Assert(42 == o2.m())
    
    Assert(o2.__class__ is C)
    Assert(o2.__class__ is not o1.__class__)


############################################################
def test_attrs():
    class C:pass
    
    C.v = 10
    
    Assert(C.v == 10)
    
    success = 0
    try:
        x = C.x
    except AttributeError:
        success = 1
    Assert(success == 1)


############################################################
def test_type_in():
	AreEqual(type in (None, True, False, 1, {}, [], (), 1.0, 1L, (1+0j)), False)

############################################################
def test_init_defaults():
    class A:
        def __init__(self, height=20, width=30):
            self.area = height * width
    
    a = A()
    Assert(a.area == 600)
    a = A(2,3)
    Assert(a.area == 6)
    a = A(2)
    Assert(a.area == 60)
    a = A(width = 2)
    Assert(a.area == 40)

############################################################
def test_getattr():
    class C:
        def __init__(self, name, flag):
            self.f = file(name, flag)
        def __getattr__(self, name):
            return getattr(self.f, name)
    
    tmpfile = "tmpfile.txt"
    
    if not is_silverlight:
        c=C(tmpfile, "w")
        c.write("Hello\n")
        c.close()
        c=C(tmpfile, "r")
        Assert(c.readline() == "Hello\n")
        c.close()

    try:
        import nt
        nt.unlink(tmpfile)
    except:
        pass
            
    # new-style
    class C(object):
        def __getattr__(self, name):
            raise AttributeError(name)
    
    # old-style
    class D:
        def __getattr__(self, name):
            raise AttributeError(name)

    # new-style __getattribute__
    class E(object):
        def __getattribute__(self, name):
            if name == 'xyz':
                raise AttributeError(name)
            return object.__getattribute__(self, name)

    # exception shouldn't propagate out
    for cls in [C, D, E]:  
        AreEqual(getattr(cls(), 'xyz', 'DNE'), 'DNE')
        AreEqual(hasattr(cls(), 'xyz'), False)
    
    
    # removing & adding back on __getattribute__ should work
    class foo(object):
        def __getattribute__(self, name): return 42

    x = foo.__getattribute__
    del foo.__getattribute__
    AssertError(AttributeError, getattr, foo(), 'x')
    foo.__getattribute__ = x
    AreEqual(foo().x, 42)
    del foo.__getattribute__
    AssertError(AttributeError, getattr, foo(), 'x')

    
def count_elem(d,n):
    count = 0
    for e in d:
        if e == n:
            count += 1
    return count

############################################################
def test_newstyle_oldstyle_dict():
    """Dictionary and new style classes"""
    
    class class_n(object):
        val1 = "Value"
        def __init__(self):
            self.val2 = self.val1
    
    inst_n = class_n()
    Assert(inst_n.val2 == "Value")
    Assert(not 'val2' in dir(class_n))
    Assert('val1' in dir(class_n))
    Assert('val2' in dir(inst_n))
    Assert('val1' in dir(inst_n))
    Assert('val2' in inst_n.__dict__)
    Assert(inst_n.__dict__['val2'] == "Value")
    Assert(count_elem(dir(inst_n), "val1") == 1)
    inst_n.val1 = 20
    Assert(count_elem(dir(inst_n), "val1") == 1)

    # old style classes:
    
    class class_o:
        val1 = "Value"
        def __init__(self):
            self.val2 = self.val1
    
    inst_o = class_o()
    Assert('val1' in dir(class_o))
    Assert(not 'val2' in dir(class_o))
    Assert('val1' in dir(inst_o))
    Assert('val2' in dir(inst_o))
    Assert('val2' in inst_o.__dict__)
    Assert(inst_o.__dict__['val2'] == "Value")
    Assert(count_elem(dir(inst_o), "val1") == 1)
    inst_n.val1 = 20
    Assert(count_elem(dir(inst_o), "val1") == 1)
    Assert(isinstance(class_o, object))
    Assert(isinstance(inst_o, object))
    Assert(isinstance(None, object))


############################################################
def test_misc():
    class C:
        def x(self):
            return 'C.x'
        def y(self):
            return 'C.y'
    
    class D:
        def z(self):
            return 'D.z'
    
    c = C()
    AreEqual(c.x(), "C.x")
    AreEqual(c.y(), "C.y")
    
    # verify repr and str on old-style class objects have the right format:
    
    # bug# 795
    AreEqual(str(C), __name__+'.C')
    AreEqual(repr(C).index('<class '+__name__+'.C at 0x'), 0)
    
    success=0
    try:
        c.z()
    except AttributeError:
        success=1
    Assert(success==1)
    
    C.__bases__+=(D,)
    
    AreEqual(c.z(), "D.z")

    import sys
    class C:
        def m(self):
            return "IronPython"
        def n(self, parm):
            return parm
    
    c = C()
    
    y = c.m
    y = c.n
    y = C.m
    y = C.n

    Assert('__dict__' not in str.__dict__)

############################################################
def test_dir_in_init():
    # both of these shouldn't throw
    
    class DirInInit(object):
        def __init__(self):
            dir(self)
    
    a = DirInInit()


############################################################
def test_priv_class():
    class _PrivClass(object):
        def __Mangled(self):
                pass
        def __init__(self):
                a = self.__Mangled
    
    a = _PrivClass()

############################################################
def test_inheritance_attrs_dir():
    class foo:
        def foofunc(self):
            return "foofunc"
    
    class bar(foo):
        def barfunc(self):
            return "barfunc"
    
    class baz(foo, bar):
        def bazfunc(self):
            return "bazfunc"
    
    Assert('foofunc' in dir(foo))
    Assert(dir(foo).count('__doc__') == 1)
    Assert(dir(foo).count('__module__') == 1)
    Assert(len(dir(foo)) == 3)
    Assert('foofunc' in dir(bar))
    Assert('barfunc' in dir(bar))
    Assert(dir(bar).count('__doc__') == 1)
    Assert(dir(bar).count('__module__') == 1)
    Assert(len(dir(bar)) == 4)
    Assert('foofunc' in dir(baz))
    Assert('barfunc' in dir(baz))
    Assert('bazfunc' in dir(baz))
    Assert(dir(baz).count('__doc__') == 1)
    Assert(dir(baz).count('__module__') == 1)
    Assert(len(dir(baz)) == 5)
    
    bz = baz()
    Assert('foofunc' in dir(bz))
    Assert('barfunc' in dir(bz))
    Assert('bazfunc' in dir(bz))
    Assert(dir(bz).count('__doc__') == 1)
    Assert(dir(bz).count('__module__') == 1)
    Assert(len(dir(bz)) == 5)
    
    bz.__module__ = "MODULE"
    Assert(bz.__module__ == "MODULE")
    bz.__module__ = "SOMEOTHERMODULE"
    Assert(bz.__module__ == "SOMEOTHERMODULE")
    bz.__module__ = 33
    Assert(bz.__module__ == 33)
    bz.__module__ = [2, 3, 4]
    Assert(bz.__module__ == [2, 3 , 4])


############################################################
def test_oldstyle_setattr():
    global called
    class C:
        def __setattr__(self, name, value):
            global called
            called = (self, name, value)
            
    a = C()
    a.abc = 'def'
    AreEqual(called, (a, 'abc', 'def'))
    
    del C.__setattr__
    
    a.qrt = 'abc'
    
    AreEqual(called, (a, 'abc', 'def'))
    
    def setattr(self, name, value): 
        global called
        called = (self, name, value)        
    
    C.__setattr__ = setattr
    
    a.qrt = 'abc'
    
    AreEqual(called, (a, 'qrt', 'abc'))
    
############################################################
def test_oldstyle_getattr():
    """verify we don't access __getattr__ while creating an old
    style class."""
    
    class C:
        def __getattr__(self,name):
            return globals()[name]
    
    a = C()

def test_oldstyle_eq():
    """old style __eq__ shouldn't call __cmp__"""
    class x: pass
    
    inst = type(x())
    
    global cmpCalled
    cmpCalled = False
    class C:
        def __init__(self, value):
            self.value = value
        def __cmp__(self, other):
            global cmpCalled
            cmpCalled = True
            return self.value - other.value
    
    class D:
        def __init__(self, value):
            self.value = value
        def __cmp__(self, other):
            global cmpCalled
            cmpCalled = True
            return self.value - other.value
    
    
    class C2(C): pass
    
    class D2(D): pass
    
    
    AreEqual(inst.__eq__(C(3.0), C(4.5)), NotImplemented)
    AreEqual(inst.__eq__(C(3.0), C2(4.5)), NotImplemented)
    AreEqual(inst.__eq__(C(3.0), D(4.5)), NotImplemented)
    AreEqual(cmpCalled, False)

############################################################
def test_raise_attrerror():
    """raising AttributeError from __getattr__ should be ok,
    and shouldn't be seen by the user"""
    
    class A:
        def __getattr__(self, name):
            raise AttributeError, 'get outta here'        
        def __repr__(self):
            return 'foo'
    
    class B:
        def __getattr__(self, name):
            raise AttributeError, 'get outta here'        
        def __str__(self):
            return 'foo'
    
    AreEqual(str(A()), 'foo')
    AreEqual(repr(A()), 'foo')
    AreEqual(str(B()), 'foo')
    Assert(repr(B()).find('B instance') != -1)

############################################################

# use exec to define methods on classes:

def test_exec_namespace():
    class oldclasswithexec:
        exec "def oldexecmethod(self): return 'result of oldexecmethod'"
    
    Assert('oldexecmethod' in dir(oldclasswithexec))
    AreEqual(oldclasswithexec().oldexecmethod(), 'result of oldexecmethod')
    
    class newclasswithexec(object):
        exec "def newexecmethod(self): return 'result of newexecmethod'"
    
    Assert('newexecmethod' in dir(newclasswithexec))
    AreEqual(newclasswithexec().newexecmethod(), 'result of newexecmethod')


############################################################
def test_module_name():
    global __name__
    import sys
    
    mod = sys.modules[__name__]
    name = __name__
    
    mod.__name__ = None
    
    class C: pass
    
    AreEqual(C.__module__, None)
    
    def func1():
        __name__ = "wrong"
        class C: pass
        return C()
    
    def func2():
        class C: pass
        return C()
    
    def func3():
        global __name__ 
        __name__ = "right"
        class C: pass
        return C()
        
        
    AreEqual(func1().__module__, func2().__module__)
    
    __name__ = "fake"
    AreEqual(func1().__module__, "fake")
    
    AreEqual(func3().__module__, "right")
    mod.__name__ = name
    

############################################################
def test_check_dictionary():
    """tests to verify that Symbol dictionaries do the right thing in dynamic scenarios"""
    def CheckDictionary(C): 
        # add a new attribute to the type...
        C.newClassAttr = 'xyz'
        AreEqual(C.newClassAttr, 'xyz')
        
        # add non-string index into the class and instance dictionary
        a = C()
        a.__dict__[1] = '1'
        if object in C.__bases__:
            try:
                C.__dict__[2] = '2'
                AssertUnreachable()
            except TypeError: pass
            AreEqual(C.__dict__.has_key(2), False)

        AreEqual(a.__dict__.has_key(1), True)
        AreEqual(dir(a).__contains__(1), True)

        AreEqual(repr(a.__dict__), "{1: '1'}")
        
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
            except TypeError:
                print "CodePlex Work Item #10577"
        
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


############################################################

def test_call_type_call():
    for stuff in [object, int, str, bool, float]:
        AreEqual(type(type.__call__(stuff)), stuff)
	
    AreEqual(type.__call__(int, 5), 5)
    AreEqual(type.__call__(int), 0)
    
    AreEqual(type.__call__(bool, True), True)
    AreEqual(type.__call__(bool), False)

def test_cp8246():
    
    #...
    class K(object):
        def __call__(self):
            return ((), {})
    AreEqual(K()(), ((), {}))
    
    #...
    class K(object):
        def __call__(self, **kwargs):
            return ((), kwargs)
    AreEqual(K()(), ((), {}))
    AreEqual(K()(**{}), ((), {}))
    AreEqual(K()(**{'a':None}), ((), {'a':None}))
    
    #...
    class K(object):
        def __call__(self, *args):
            return (args, {})
    AreEqual(K()(), ((), {}))
    AreEqual(K()(*()), ((), {}))
    AreEqual(K()(*(None,)), ((None,), {}))
    
    #...
    class K(object):
        def __call__(self, *args, **kwargs):
            return (args, kwargs)
    AreEqual(K()(), ((), {}))
    AreEqual(K()(*()), ((), {}))
    AreEqual(K()(**{}), ((), {}))
    AreEqual(K()(*(), **{}), ((), {}))
    AreEqual(K()(*(None,)), ((None,), {}))
    AreEqual(K()(**{'a':None}), ((), {'a':None}))
    AreEqual(K()(*(None,), **{'a':None}), ((None,), {'a':None}))
    
    
############################################################
def text_mixed_inheritance():
    """inheritance from both old & new style classes..."""
    class foo: pass
    
    class bar(object): pass
    
    class baz1(foo, bar): pass
    
    class baz2(bar, foo): pass
    
    AreEqual(baz1.__bases__, (foo, bar))
    AreEqual(baz2.__bases__, (bar, foo))


############################################################
def test_newstyle_unbound_inheritance():
    """verify calling unbound method w/ new-style class on subclass which 
    new-style also inherits from works."""
    class foo:
        def func(self): return self
    
    class bar(object, foo):
        def barfunc(self):
                return foo.func(self)
    
    a = bar()
    AreEqual(a.barfunc(), a)

############################################################
def test_mro():
    """mro (method resolution order) support"""
    class A(object): pass
    
    AreEqual(A.__mro__, (A, object))
    
    class B(object): pass
    
    AreEqual(B.__mro__, (B, object))
    
    class C(B): pass
    
    AreEqual(C.__mro__, (C, B, object))
    
    class N(C,B,A): pass
    
    AreEqual(N.__mro__, (N, C, B, A, object))
    
    try:
        class N(A, B,C): pass
        AssertUnreachable("impossible MRO created") 
    except TypeError:
        pass
    
    try:
        class N(A, A): pass
        AssertUnreachable("can't dervie from the same base type twice") 
    except TypeError:
        pass

############################################################
def test_mro_bases():
    """verify replacing base classes also updates MRO"""
    class C(object): 
        def __getattribute__(self, name): 
            if(name == 'xyz'): return 'C'
            return super(C, self).__getattribute__(name)
    
    class C1(C): 
        def __getattribute__(self, name): 
            if(name == 'xyz'):  return 'C1'
            return super(C1, self).__getattribute__(name)
    
    class A(object): pass
    
    class B(object):
        def __getattribute__(self, name):
            if(name == 'xyz'): return 'B'
            return super(B, self).__getattribute__(name)
    
    a = C1()
    AreEqual(a.xyz, 'C1')
    
    C1.__bases__ = (A,B)
    AreEqual(a.xyz, 'C1')
    
    del(C1.__getattribute__)
    AreEqual(a.xyz, 'B')

############################################################
def test_builtin_mro():
    """int mro shouldn't include ValueType"""
    AreEqual(int.__mro__, (int, object))

############################################################
def test_mixed_inheritance_mro():
    """mixed inheritance from old-style & new-style classes"""
    
    # we should use old-style MRO when inheriting w/ a single old-style class
    class A: pass
    
    class B(A): pass
    
    class C(A): pass
    
    class D(B, C):pass
    
    class E(D, object): pass
    
    # old-style MRO of D is D, B, A, C, which should
    # be present  in E's mro
    AreEqual(E.__mro__, (E, D, B, A, C, object))
    
    class F(B, C, object): pass
    
    # but when inheriting from multiple old-style classes we switch
    # to new-style MRO, and respect local ordering of classes in the MRO
    AreEqual(F.__mro__, (F, B, C, A, object))
    
    class G(B, object, C): pass
    
    AreEqual(G.__mro__, (G, B, object, C, A))
    
    class H(E): pass
    
    AreEqual(H.__mro__, (H, E, D, B, A, C, object))
    
    try:
        class H(A,B,E): pass
        AssertUnreachable()
    except TypeError: 
        pass
    
    
    class H(E,B,A): pass
    
    AreEqual(H.__mro__, (H, E, D, B, A, C, object))

############################################################
def test_depth_first_mro_mixed():
    """Verify given two large, independent class hierarchies
    that we favor them in the order listed.
    
    w/ old-style
    """
    
    class A: pass
    
    class B(A): pass
    
    class C(A): pass
    
    class D(B,C): pass
    
    class E(D, object): pass
    
    class G: pass
    
    class H(G): pass
    
    class I(G): pass
    
    class K(H,I, object): pass
    
    class L(K,E): pass
    
    AreEqual(L.__mro__, (L, K, H, I, G, E, D, B, A, C, object))

############################################################
def test_depth_first_mro():
    """w/o old-style"""
    
    class A(object): pass
    
    class B(A): pass
    
    class C(A): pass
    
    class D(B,C): pass
    
    class E(D, object): pass
    
    class G(object): pass
    
    class H(G): pass
    
    class I(G): pass
    
    class K(H,I, object): pass
    
    class L(K,E): pass
    
    AreEqual(L.__mro__, (L, K, H, I, G, E, D, B, C, A, object))

    
############################################################
def test_newstyle_lookup():
    """new-style classes should only lookup methods from the class, not from the instance"""
    class Strange(object):
        def uselessMethod(self): pass
    
    global obj
    obj = Strange()
    obj.__nonzero__ = lambda: False
    AreEqual(bool(obj), True)
    
    def twoargs(self, other): 
        global twoArgsCalled 
        twoArgsCalled = True
        return self
    
    def onearg(self): 
        return self
        
    def onearg_str(self):
        return 'abc'
    
    # create methods that we can then stick into Strange
    twoargs = type(Strange.uselessMethod)(twoargs, None, Strange)
    onearg = type(Strange.uselessMethod)(onearg, None, Strange)


    class ForwardAndReverseTests:
        testCases = [
            #forward versions
            ('__add__', 'obj + obj'), 
            ('__sub__', 'obj - obj'),
            ('__mul__', 'obj * obj'),
            ('__floordiv__', 'obj // obj'),
            ('__mod__', 'obj % obj'),
            ('__divmod__', 'divmod(obj,obj)'), 
            ('__pow__', 'pow(obj, obj)'),
            ('__lshift__', 'obj << obj'),
            ('__rshift__', 'obj >> obj'),
            ('__and__', 'obj & obj'),
            ('__xor__', 'obj ^ obj'),
            ('__or__', 'obj | obj'),
            
            # reverse versions
            ('__radd__', '1 + obj'),
            ('__rsub__', '1 - obj'),
            ('__rmul__', '1 * obj'),
            ('__rfloordiv__', '1 // obj'),
            ('__rmod__', '1 % obj'),
            #('__rdivmod__', '1 % obj'), #bug 975
            ('__rpow__', 'pow(1, obj)'),
            ('__rlshift__', '1 << obj'),
            ('__rrshift__', '1 >> obj'),
            ('__rand__', '1  & obj'),
            ('__rxor__', '1 ^ obj'),
            ('__ror__', '1 | obj'),      
            ]
        
        @staticmethod
        def NegativeTest(method, testCase):
            setattr(obj, method, twoargs)
            
            try:
                eval(testCase)    
                AssertUnreachable()
            except TypeError, e:
                pass
            
            delattr(obj, method)
        
        @staticmethod
        def PositiveTest(method, testCase):
            setattr(Strange, method, twoargs)
            
            AreEqual(eval(testCase), obj)
            
            delattr(Strange, method)
    
    
    class InPlaceTests:
        # in-place versions require exec instead of eval
        testCases = [
            # inplace versions
            ('__iadd__', 'obj += obj'),
            ('__isub__', 'obj -= obj'),
            ('__imul__', 'obj *= obj'),
            ('__ifloordiv__', 'obj //= obj'),
            ('__imod__', 'obj %= obj'),
            ('__ipow__', 'obj **= obj'),
            ('__ilshift__', 'obj <<= obj'),
            ('__irshift__', 'obj >>= obj'),
            ('__iand__', 'obj &= obj'),
            ('__ixor__', 'obj ^= obj'),
            ('__ior__', 'obj |= obj'),      
        ]    
        
        @staticmethod
        def NegativeTest(method, testCase):
            setattr(obj, method, twoargs)
            
            try:
                exec testCase in globals(), locals()
                AssertUnreachable()
            except TypeError:
                pass
            
            delattr(obj, method)
        
        @staticmethod
        def PositiveTest(method, testCase):
            setattr(Strange, method, twoargs)
            
            global twoArgsCalled
            twoArgsCalled = False
            exec testCase in globals(), locals()
            AreEqual(twoArgsCalled, True)        
            
            delattr(Strange, method)
    
    
    class SingleArgTests:    
        testCases = [
            # one-argument versions
            ('__neg__', '-obj'), 
            ('__pos__', '+obj'),
            ('__abs__', 'abs(obj)'),
            ('__invert__', '~obj'),     
            ]        
        
        @staticmethod
        def NegativeTest(method, testCase):
            setattr(obj, method, onearg)
        
            try:
                eval(testCase)
                AssertUnreachable()
            except TypeError:
                pass
            
            delattr(obj, method)
        
        @staticmethod
        def PositiveTest(method, testCase):
            setattr(Strange, method, onearg)

            try:
                AreEqual(eval(testCase), obj)                               
            except TypeError:
                Assert(method == '__oct__' or method == '__hex__')
            
            delattr(Strange, method)
    
    class HexOctTests:
        testCases = [
            ('__oct__', 'oct(obj)'),
            ('__hex__', 'hex(obj)'),
            ]

        @staticmethod
        def NegativeTest(method, testCase):
            setattr(obj, method, onearg)
        
            try:
                eval(testCase)
                AssertUnreachable()
            except TypeError:
                pass
            
            delattr(obj, method)
        
        @staticmethod
        def PositiveTest(method, testCase):
            setattr(Strange, method, onearg_str)

            AreEqual(eval(testCase), 'abc')                               
            
            delattr(Strange, method)

    class ConversionTests:
        testCases = [
            (('__complex__', 2+0j), 'complex(obj)'),
            (('__int__', 1), 'int(obj)'),
            (('__long__', 1L), 'long(obj)'),
            (('__float__', 1.0), 'float(obj)'),
          ]
          
        @staticmethod
        def NegativeTest(method, testCase):
            setattr(obj, method[0], onearg)
        
            try:
                eval(testCase)
                AssertUnreachable()
            except (TypeError, ValueError), e:
                AreEqual(e.args[0].find('returned') == -1, True)    # shouldn't have returned '__complex__ returned ...'

            delattr(obj, method[0])
            
        @staticmethod
        def PositiveTest(method, testCase):
            def testMethod(self):
                return method[1]
                
            testMethod = type(Strange.uselessMethod)(testMethod, None, Strange)
            setattr(Strange, method[0], testMethod)
    
            AreEqual(eval(testCase), method[1])
            
            delattr(Strange, method[0])
        
    allTests = [ForwardAndReverseTests, InPlaceTests, SingleArgTests, ConversionTests, HexOctTests]
    
    for test in allTests:
        for method,testCase in test.testCases: 
            test.NegativeTest(method, testCase)
        for method,testCase in test.testCases: 
            test.PositiveTest(method, testCase)


def test_bad_repr():
    # overriding a classes __repr__ and returning a
    # non-string should throw
    
    class C:
        def __repr__(self):
            return None
    
    AssertError(TypeError, repr, C())
    
    class C(object):
        def __repr__(self):
            return None
    
    AssertError(TypeError, repr, C())


############################################################
def test_name():
    """setting __name__ on a class should work"""
    
    class C(object): pass
    
    C.__name__ = 'abc'
    AreEqual(C.__name__, 'abc')

############################################################
def test_mro_super():
    """super for multiple inheritance we should follow the MRO as we go up the super chain"""
    class F: 
        def meth(self):
            return 'F' 
    
    class G: pass
    
    def gmeth(self): return 'G'
    
    
    class A(object):
        def meth(self):
            if hasattr(super(A, self), 'meth'):
                return 'A' + super(A, self).meth()
            else:
                return "A" 
    
    class B(A):
        def __init__(self):
            self.__super = super(B, self)
            super(B, self).__init__()
        def meth(self):
            return "B" + self.__super.meth()
    
    class C(A):
        def __init__(self):
            self.__super = super(C, self)
            super(C, self).__init__()
        def meth(self):
            return "C" + self.__super.meth()
    
    class D(C, B):
        def meth(self):
            return "D" + super(D, self).meth()
    
    AreEqual(D().meth(), 'DCBA')
    
    class D(C, F, B):
        def meth(self):
            return "D" + super(D, self).meth()
    
    AreEqual(D.__mro__, (D,C,F,B,A,object))
    AreEqual(D().meth(), 'DCF')
    
    class D(C, B, F):
        def meth(self):
            return "D" + super(D, self).meth()
    
    AreEqual(D.__mro__, (D,C,B,A,object,F))
    AreEqual(D().meth(), 'DCBAF')
    
    
    class D(C, B, G):
        def meth(self):
            return "D" + super(D, self).meth()
    
    d = D()
    d.meth = type(F.meth)(gmeth, d, G)
    AreEqual(d.meth(), 'G')

############################################################
def test_slots():
    """slots tests"""
    
    # simple slots, assign, delete, etc...
    
    class foo(object):
        __slots__ = ['abc']
        
    class bar(object):
        __slots__ = 'abc'
    
    class baz(object):
        __slots__ = ('abc', )
    
    for slotType in [foo, bar, baz]:
        a = slotType()
        AssertError(AttributeError, lambda: a.abc)
        AreEqual(hasattr(a, 'abc'), False)
        
        a.abc = 'xyz'
        AreEqual(a.abc, 'xyz')
        AreEqual(hasattr(a, 'abc'), True)

        del(a.abc)
        AreEqual(hasattr(a, 'abc'), False)
        AssertError(AttributeError, lambda: a.abc)
        
        # slot classes don't have __dict__
        AreEqual(hasattr(a, '__dict__'), False)
        AssertError(AttributeError, lambda: a.__dict__)
    
    # sub-class of slots class, has no slots, has a __dict__    
    class foo(object):
        __slots__ = 'abc'
        def __init__(self):
            self.abc = 23
            
    class bar(foo): 
        def __init__(self):
            super(bar, self).__init__()
        
    a = bar()
    AreEqual(a.abc, 23)
    
    del(a.abc)
    AreEqual(hasattr(a, 'abc'), False)
    a.abc = 42
    AreEqual(a.abc, 42)
    
    x = a.__dict__
    AreEqual(x.has_key('abc'), False)
    a.xyz = 'abc'
    AreEqual(a.xyz, 'abc')
    
    # subclass of not-slots class defining slots:
    
    class A(object): pass
    class B(A): __slots__ = 'c'
    
    AreEqual(hasattr(B(), '__dict__'), True)
    AreEqual(hasattr(B, 'c'), True)
    
    # slots & metaclass
    if is_cli or is_silverlight:          # INCOMPATBILE: __slots__ not supported for subtype of type
        class foo(type): 
            __slots__ = ['abc']
    
        class bar(object):
            __metaclass__ = foo
    
    # complex slots
    
    class foo(object):
        __slots__ = ['abc']
        def __new__(cls, *args, **kw):
            self = object.__new__(cls)
            dict = object.__getattribute__(self, '__dict__')
            return self
    
    class bar(foo): pass
    
    
    a = bar()
    
    AssertError(AttributeError, foo)
    
    # slots & name-mangling
    
    class foo(object):
        __slots__ = '__bar'
        
    AreEqual(hasattr(foo, '_foo__bar'), True)
    
    # invalid __slots__ values
    for x in ['', None, '3.5']:
        try:
            class C(object):
                __slots__ = x
            AssertUnreachable()
        except TypeError:
            pass
    
    # including __dict__ in slots allows accessing __dict__
    class A(object): __slots__ = '__dict__'
    
    AreEqual(hasattr(A(),"__dict__"), True)
    a = A()
    a.abc = 'xyz'
    AreEqual(a.abc, 'xyz')
    
    class B(A): pass
    AreEqual(hasattr(B(),"__dict__"), True)
    b = A()
    b.abc = 'xyz'
    AreEqual(b.abc, 'xyz')
    
    # including __weakref__ explicitly
    class A(object):
        __slots__ = ["__weakref__"]
    
    hasattr(A(), "__weakref__")
    
    class B(A): pass
    
    hasattr(B(), "__weakref__")
    
    # weird case, including __weakref__ and __dict__ and we allow
    # a subtype to inherit from both

    if is_cli or is_silverlight: types = [object, dict, tuple]    # INCOMPATBILE: __slots__ not supported for tuple
    else: types = [object,dict]
    
    for x in types:
        class A(x):
            __slots__ = ["__dict__"]
        
        class B(x):
            __slots__ = ["__weakref__"]
        
        class C(A,B):
            __slots__ = []
            
        a = C()
        AreEqual(hasattr(a, '__dict__'), True)
        AreEqual(hasattr(a, '__weakref__'), True)
    
        class C(A,B):
            __slots__ = ['xyz']
            
        a = C()
        AreEqual(hasattr(a, '__dict__'), True)
        AreEqual(hasattr(a, '__weakref__'), True)
        AreEqual(hasattr(C, 'xyz'), True)
    
    # calling w/ keyword args
    
    class foo(object):
        __slots__ = ['a', 'b']
        def __new__(cls, one='a', two='b'):
            self = object.__new__(cls)
            self.a = one
            self.b = two
            return self
    
    a = foo('x', two='y')
    AreEqual(a.a, 'x')
    AreEqual(a.b, 'y')    
        
    # assign to __dict__
    
    class C(object): pass
    
    a = C()
    a.__dict__ = {'b':1}
    AreEqual(a.b, 1)
    
    
    # base, child define slots, grand-child doesn't
    
    class foo(object): __slots__ = ['fooSlot']
    
    class bar(foo): __slots__ = ['barSlot']
    
    class baz(bar): pass   # shouldn't throw
    
    a = baz()
    a.barSlot = 'xyz'
    a.fooSlot = 'bar'
    a.dictEntry = 'foo'
    
    AreEqual(a.barSlot, 'xyz')
    AreEqual(a.fooSlot, 'bar')
    AreEqual(a.dictEntry, 'foo')
    
    # large number of slots works (nested tuple slots)
    class foo(object):
        __slots__ = [ 'a' + str(x) for x in range(256) ]
        
    a = foo()
    
    for x in range(256):
        setattr(a, 'a' + str(x), x)
        
    for x in range(256):
        AreEqual(x, getattr(a, 'a' + str(x)))


    # new-style / old-style mixed with slots, slots take precedence
    class foo:
        abc = 3

    class bar(object):
        __slots__ = ['abc']
        def __init__(self): self.abc = 5

    class bar2(object):
        abc = 5

    class baz(foo, bar): pass
    
    class baz2(foo, bar2): pass

    AreEqual(baz().abc, 5)
    
    # but if it's just a class member we respect MRO
    AreEqual(baz2().abc, 3)
    
    
def test_slots11457():
    class COld:
        __slots__ = ['a']

    class CNew(object):
        __slots__ = ['a']
        
    for C in [COld, CNew]:
        for i in xrange(2):
            setattr(C, 'a', 5)
            AreEqual(C().a, 5)
            
            setattr(C, 'a', 7)
            AreEqual(C().a, 7)        
    
############################################################
def test_inheritance_cycle():
    """test for inheritance cycle"""
    class CycleA: pass
    class CycleB: pass
    
    try:
      CycleA.__bases__ = (CycleA,)
      AssertUnreachable()
    except TypeError: pass
    
    try:
      CycleA.__bases__ = (CycleB,)
      CycleB.__bases__ = (CycleA,)
      AssertUnreachable()
    except TypeError: pass

############################################################
def test_hexoct():
    """returning non-string from hex & oct should throw"""
    
    class foo(object):
        def __hex__(self): return self
        def __oct__(self): return self
        
    class bar:
        def __hex__(self): return self
        def __oct__(self): return self
        
    AssertError(TypeError, hex, foo())
    AssertError(TypeError, oct, foo())
    AssertError(TypeError, hex, bar())
    AssertError(TypeError, oct, bar())

@disabled("CodePlex Work Item 766")
def test_no_clr_attributes():
    """verify types have no CLR attributes"""
    for stuff in [int, float, bool, str, None]:
        for dir_stuff in dir(stuff):
            if dir_stuff[:1].isalpha():
                Assert(dir_stuff[:1].islower(), 
                       "%s should not be an attribute of %s" % (dir_stuff, str(stuff)))

def test_no_clr_attributes_sanity():    
    AreEqual(hasattr(int, 'MaxValue'), False)
    AreEqual(hasattr(int, 'MinValue'), False)
    AreEqual(hasattr(int, 'Abs'), False)
    AreEqual(hasattr(int, 'BitwiseOr'), False)
    AreEqual(hasattr(int, 'Equals'), False)
    
    AreEqual(hasattr(str, 'Empty'), False)
    AreEqual(hasattr(str, 'Compare'), False)
    AreEqual(hasattr(str, 'Equals'), False)
    AreEqual(hasattr(str, 'IndexOf'), False)

############################################################
def test_outer_scope():
    """do not automatically include outer scopes in closure scenarios"""
    def outer_scope_test():
        class Referenced:
            pass
        class C:
            if Referenced: pass
        Assert("Referenced" not in C.__dict__.keys())
    
    outer_scope_test()
    
    
    for x in [None, 'abc', 3]:
        class foo(object): pass
        a = foo()
        try:
            a.__dict__ = x
            AssertUnreachable()
        except TypeError: pass

def test_default_new_init():
    """test cases to verify we do the right thing for the default new & init
    methods"""

    anyInitList = [object,      # classes that take any set of args to __init__
                   int,
                   long,
                   float,
                   complex,
                   tuple,
                  ]
    anyNewList  = [list,        # classes that take any set of args to __new__
                   set,
                   ]        
            
    for x in anyInitList:
        x().__init__(1,2,3)
            
        AssertError(TypeError, x.__new__, x, 1, 2, 3)
        AreEqual(isinstance(x.__new__(x), x), True)
    
    for x in anyNewList:
        AreEqual(len(x.__new__(x, 1, 2, 3)), 0)
        AssertError(TypeError, x.__new__(x).__init__, 1, 2, 3)    


    
    class foo(object): pass
    
    AssertError(TypeError, foo, 1)
    
    class foo(list): pass
    AreEqual(list.__new__(list, sequence='abc'), [])
    
    x = list.__new__(foo, 1, 2, 3)
    AreEqual(len(x), 0)
    AreEqual(type(x), foo)
    
    
    # define only __init__.  __new__ should be the same object
    # for both types, and calling it w/ different types should have
    # different responses.
    class foo(object):
        def __init__(self): pass
                
    AreEqual(id(foo.__new__), id(object.__new__))
    
    AssertError(TypeError, object.__new__, object, 1,2,3)
    AreEqual(type(object.__new__(foo, 1, 2, 3)), foo)
    
def test_hash():
    for x in [tuple, str, unicode, object, frozenset]:
        inst = x()
        AreEqual(inst.__hash__(), hash(inst))
        

def test_NoneSelf():
    try:
        set.add(None)
        AssertUnreachable()
    except TypeError:
        pass

def test_builtin_classmethod():
    descr = dict.__dict__["fromkeys"]
    AssertError(TypeError, descr.__get__, 42)
    AssertError(TypeError, descr.__get__, None, 42)
    AssertError(TypeError, descr.__get__, None, int)

def test_classmethod():
    AssertError(TypeError, classmethod, 1)
    def foo(): pass
        
    cm = classmethod(foo)
    AssertError(TypeError, cm.__get__, None)
    AssertError(TypeError, cm.__get__, None, None)
        

def test_EmptyTypes():
    for x in [None, Ellipsis, NotImplemented]:
        Assert(type(x) != str)
        
    AreEqual(repr(Ellipsis), 'Ellipsis')
    AreEqual(repr(NotImplemented), 'NotImplemented')
    AreEqual(repr(type(Ellipsis)), "<type 'ellipsis'>")
    AreEqual(repr(type(NotImplemented)), "<type 'NotImplementedType'>")
    
def test_property():
    prop = property()
    try: prop.fget = test_classmethod
    except TypeError: pass
    else: AssertUnreachable()
    
    try: prop.fdel = test_classmethod
    except TypeError: pass
    else: AssertUnreachable()
    
    try: prop.__doc__ = 'abc'
    except TypeError: pass
    else: AssertUnreachable()
    
    try: prop.fset = test_classmethod
    except TypeError: pass
    else: AssertUnreachable()
    
@skip("win32")
def test_override_mro():
    try:
        class C(object):
            def __mro__(self): pass
    except NotImplementedError: pass
    else: Fail("Expected NotImplementedError, got none")
        
    class C(object):
        def mro(self): pass
    
    try:
        class C(type):
            def mro(self): pass
    except NotImplementedError: pass
    else: Fail("Expected NotImplementedError, got none")
    
    class D(type): pass
        
    try:
        class E(D):
            def mro(self): pass
    except NotImplementedError: pass
    else: Fail("Expected NotImplementedError, got none")

@skip("win32")    
def test_type_mro():
    AssertError(NotImplementedError, type.mro, int)

def test_derived_tuple_eq():
    # verify overriding __eq__ on tuple still allows us to call the super version
    class bazbar(tuple):    
        def __eq__(self,other):
            other = bazbar(other)
            return super(bazbar,self).__eq__(other)
    AreEqual(bazbar('abc'), 'abc')
    
def test_new_old_slots():
    class N(object): pass
    class O: pass
    class C(N, O):
        __slots__ = ['a','b']


def test_slots_counter():
    import gc
    class Counter(object):
        c = 0
        def __init__(self):
            Counter.c += 1
        def __del__(self):
            Counter.c -= 1
    def testit():
        class C(object):
            __slots__ = ['a', 'b', 'c']
    
        x = C()
        x.a = Counter()
        x.b = Counter()
        x.c = Counter()
    
        AreEqual(Counter.c, 3)
        del x
    
    testit()
    #collect not defined for silverlight
    if not is_silverlight:
        gc.collect()    
        AreEqual(Counter.c, 0)

def test_override_container_contains():
    for x in (dict, list, tuple):
        class C(x):
            def __contains__(self, other): 
                return other == "abc"
                
        AreEqual('abc' in C(), True)

def test_override_container_len():
    for x in (dict, list, tuple):
        class C(x):
            def __len__(self): return 42
            
        AreEqual(len(C()), 42)
        
def test_dictproxy_access():
    def f():
        int.__dict__[0] = 0
        
    AssertError(TypeError, f)

# tests w/ special requirements that can't be run in methods..
#Testing the class attributes backed by globals
    
x = 10

class C:
    x = x
    del x
    x = x
    
AreEqual(C.x, 10)
AreEqual(x, 10)

try:
    class C:
        x = x
        del x
        del x
except NameError:
    pass
else:
    Assert("Expecting name error")

AreEqual(x, 10)

class C:
    x = 10
    del x
    b = x
    AreEqual(x, 10)

AreEqual(C.b, 10)
AreEqual(x, 10)

def test_getattribute_getattr():
    # verify object.__getattribute__(name) will call __getattr__
    class Base(object):
        def __getattribute__(self, name):
            return object.__getattribute__(self, name)
    
    class Derived(Base):
        def __getattr__(self, name):
            if name == "bar": return 23
            raise AttributeError(name)
        def __getattribute__(self, name):
            return Base.__getattribute__(self, name)
    
    a = Derived()
    AreEqual(a.bar, 23)

def test_setattr():
    # verify defining __setattr__ works
    global setCalled
    setCalled = False
    class TestClass(object): 
        def __setattr__(self, name, value):
            global setCalled
            setCalled = True
            object.__setattr__(self, name, value)
    
    x = TestClass()
    x.abc = 23
    AreEqual(x.abc, 23)
    AreEqual(setCalled, True)

def test_dynamic_getattribute():
    # verify adding / removing __getattribute__ succeeds
    
    # remove
    class foo(object):
        def __getattribute__(self, name):
            raise Exception()
    
    class bar(foo):
        def __getattribute__(self, name):
            return super(bar, self).__getattribute__(name)
    
    del foo.__getattribute__
    a = bar()
    a.xyz = 'abc'
    AreEqual(a.xyz, 'abc')
    
    # add
    class foo(object): pass

    def getattr(self, name): 
        if name == 'abc': return 42
    
    foo.__getattribute__ = getattr
    
    AreEqual(foo().abc, 42)

def test_nonstring_name():
    global __name__
    
    name = __name__
    try:
        __name__ = 3
        class C: pass
        
        AreEqual(C.__module__, 3)

        class C(object): pass
        
        AreEqual(C.__module__, 3)
        
        class C(type): pass
        
        AreEqual(C.__module__, 3)
        
        class D(object):
            __metaclass__ = C
            
        AreEqual(D.__module__, 3)    
    finally:
        __name__ = name
    
def test_dictproxy_descrs():
    # verify that we get the correct descriptors when we access the dictionary proxy directly
    class foo(object):
        xyz = 'abc'

    AreEqual(foo.__dict__['xyz'], 'abc')
    
    class foo(object):
        def __get__(self, instance, context):
            return 'abc'

    class bar(object):
        xyz = foo()

    AreEqual(bar.__dict__['xyz'].__class__, foo)

## __int__

def test_fastnew_int():
    class C1:
        def __int__(self): return 100
    class C2: 
        def __int__(self): return myint(100)
    class C3:
        def __int__(self): return 100L
    class C4: 
        def __int__(self): return mylong(100L)
    class C5:
        def __int__(self): return -123456789012345678910
    class C6:
        def __int__(self): return C6()
    class C7: 
        def __int__(self): return "100"
    
    for x in [C1, C2, C3, C4]:   AreEqual(int(x()), 100)
    AreEqual(int(C5()), -123456789012345678910)
    for x in [C6, C7]:      AssertError(TypeError, int, x())       
        
    class C1(object):
        def __int__(self): return 100
    class C2(object): 
        def __int__(self): return myint(100)
    class C3(object):
        def __int__(self): return 100L
    class C4(object): 
        def __int__(self): return mylong(100L)
    class C5(object):
        def __int__(self): return -123456789012345678910
    class C6(object):
        def __int__(self): return C6()
    class C7(object): 
        def __int__(self): return "100"
    
    for x in [C1, C2, C3, C4]:   AreEqual(int(x()), 100)
    AreEqual(int(C5()), -123456789012345678910)
    for x in [C6, C7]:      AssertError(TypeError, int, x())       


def test_type_type_is_type():
    class OS: pass    
    class NS(object): pass
    
    true_values = [type, NS, int, float, tuple, str]
    if is_cli:
        import System
        true_values += [System.Boolean, System.Int32, System.Version, System.Exception]
    
    for x in true_values:
        Assert(type(x) is type)
        
    false_values = [OS]
    if is_cli:
        false_values += [ System.Boolean(1), System.Int32(3), System.Version(), System.Exception() ]
        
    for x in false_values:    
        Assert(type(x) is not type)
    

def test_hash_return_values():
    # new-style classes do conversion to int
    for retval in [1, 1.0, 1.1, 1L, 1<<30]:
        for type in [object, int, str, float, long]:
            class foo(object):
                def __hash__(self): return retval
            
            AreEqual(hash(foo()), int(retval))
    
    # old-style classes require int or long return value
    for retval in [1.0, 1.1]:
        class foo:
            def __hash__(self): return retval
        
        AssertError(TypeError, hash, foo())

    tests = {   1L:1,
                2L:2,
                1<<32: 1,
                (1<<32)+1: 2,
                (1<<32)-1: -2,
                1<<34: 4,
                1<<31: -2147483648,
            }
    #CodePlex Work Item #10578
    if sys.platform!="win32":
        print "CodePlex Work Item #10578"
        tests = {}
    for retval in tests.keys():
        class foo:
            def __hash__(self): return retval
        
        AreEqual(hash(foo()), tests[retval])    

       
def test_cmp_notimplemented(): 
    class foo(object):
        def __eq__(self, other):
            ran.append('foo.eq')
            return NotImplemented
        def __ne__(self, other):
            ran.append('foo.ne')        
            return NotImplemented
        def __le__(self, other):
            ran.append('foo.le')
            return NotImplemented
        def __lt__(self, other):
            ran.append('foo.lt')
            return NotImplemented
        def __gt__(self, other):
            ran.append('foo.gt')
            return NotImplemented
        def __ge__(self, other):
            ran.append('foo.ge')
            return NotImplemented
        def __cmp__(self, other):
            ran.append('foo.cmp')
            return NotImplemented
    
    
    class bar:
        def __eq__(self, other):
            ran.append('bar.eq')
            return NotImplemented
        def __ne__(self, other):
            ran.append('bar.ne')
            return NotImplemented
        def __le__(self, other):
            ran.append('bar.le')
            return NotImplemented
        def __lt__(self, other):
            ran.append('bar.lt')
            return NotImplemented
        def __gt__(self, other):
            ran.append('bar.gt')
            return NotImplemented
        def __ge__(self, other):
            ran.append('bar.ge')
            return NotImplemented
        def __cmp__(self, other):
            ran.append('bar.cmp')
            return NotImplemented

    ran = []
    cmp(foo(), bar())
    #AreEqual(ran, ['foo.eq', 'bar.eq', 'foo.lt', 'bar.gt', 'foo.gt', 'bar.lt', 'bar.cmp'])
    
    ran = []
    cmp(foo(), foo())
    #AreEqual(ran, ['foo.cmp', 'foo.cmp'])
    
    ran = []
    cmp(bar(), bar())
    #AreEqual(ran, ['bar.cmp', 'bar.cmp', 'bar.eq', 'bar.eq', 'bar.eq', 'bar.eq', 'bar.lt', 'bat.gt', 'bar.gt', 'bar.lt', 'bar.gt', 'bar.lt', 'bar.lt', 'bar.gt', 'bar.cmp', 'bar.cmp'])
    
    ran = []
    cmp(foo(), 1)
    #AreEqual(ran, ['foo.eq', 'foo.lt', 'foo.gt', 'foo.cmp'])


#CodePlex Work Item #11487
@skip("cli silverlight")
def test_override_repr():
    class KOld:
        def __repr__(self):
            return "old"
            
    class KNew(object):
        def __repr__(self):
            return "new"
            
    AreEqual(repr(KOld()), "old")
    AreEqual(str(KOld()), "old")
    AreEqual(repr(KNew()), "new")
    #IP breaks here because __str__ in new style classes does not call __repr__
    AreEqual(str(KNew()), "new")


def test_mutate_base():
        class basetype(object): 
            xyz = 3
        
        class subtype(basetype): pass
        
        AreEqual(subtype.xyz, 3)
        AreEqual(subtype().xyz, 3)
        
        basetype.xyz = 7
        
        AreEqual(subtype.xyz, 7)
        AreEqual(subtype().xyz, 7)

def test_mixed_newstyle_oldstyle_init():
    """mixed new-style & old-style class should run init if its defined in the old-style class"""
    class foo:
        def __init__(self):
            self.x = 3
    
    class bar(foo):
        def __init__(self):
            self.x = 4
    
    class baz(foo): pass
    
    class ns(object): pass
    
    class full(bar, baz, ns): pass
    
    a = full()
    AreEqual(a.x, 4)
    
    class full(bar, baz, ns): 
        def __init__(self):
            self.x = 5
    
    a = full()
    AreEqual(a.x, 5)

    class ns(object): 
        def __init__(self):
            self.x = 6
    
    class full(bar, baz, ns): pass
    a = full()
    AreEqual(a.x, 4)

def test_getattr_exceptions():
    """verify the original exception propagates out"""
    class AttributeTest(object):
        def __getattr__(self, name): raise AttributeError('catch me')
        
    x = AttributeTest()
    try:
        y = x.throws
    except AttributeError, ex:
        AreEqual(ex.args, ('catch me',))
    else: Fail("should have thrown")

def test_descriptor_meta_magic():
    class valueDescriptor(object):
        def __init__(self,x=None): self.value = x
        def __get__(self,ob,cls):   return self.value
        def __set__(self,ob,x):     self.value = x
    
    class Ameta(type):
        def createShared( cls, nm, initValue=None ):
            o = valueDescriptor(initValue)
            setattr( cls,nm, o )
            setattr( cls.__class__,nm, o )    
    
    class A:
        __metaclass__ = Ameta
    
    class B( A ):
        A.createShared("cls2",1)
    
    def test(value):        
        AreEqual(o.cls2, value)
        AreEqual(o2.cls2, value)
        AreEqual(A.cls2, value)
        AreEqual(B.cls2, value)
        
    o = A()
    o2 = B()
    test(1)
        
    B.cls2 = 2
    test(2)
        
    A.cls2 = 3
    test(3)
    
    o.cls2 = 4
    test(4)
    
    o2.cls2 = 5
    test(5)

def test_missing_attr():
    class foo(object): pass
    
    a = foo()
    def f(): a.dne
    AssertErrorWithMessage(AttributeError, "'foo' object has no attribute 'dne'", f)

def test_method():
    class tst_oc:
        def root(): return 2

    class tst_nc:
        def root(): return 2

    AssertError(TypeError, tst_oc.root)
    AssertError(TypeError, tst_nc.root)

def test_descriptors_custom_attrs():
    """verifies the interaction between descriptors and custom attribute access works properly"""
    class mydesc(object):
        def __get__(self, instance, ctx): 
            raise AttributeError
    
    class f(object):
        x = mydesc()
        def __getattr__(self, name): return 42
    
    AreEqual(f().x, 42)

@runonly("win32")
def test_cp5801():
    class Foo(object):
        __slots__ = ['bar']
        def __getattr__(self, n):
            return n.upper()
        
    foo = Foo()
    AreEqual(foo.bar, "BAR")


def test_property_always_set_descriptor():
    """verifies that set descriptors take precedence over dictionary entries and 
       properties are always treated as set descriptors, even if they have no 
       setter function"""
    
    class C(object):
        x = property(lambda self: self._x)
        def __init__(self):
            self._x = 42
    
    
    c = C()
    c.__dict__['x'] = 43
    AreEqual(c.x, 42)

    # now check a user get descriptor
    class MyDescriptor(object):
        def __get__(self, *args): return 42
        
    class C(object):
        x = MyDescriptor()
        
    c = C()
    c.__dict__['x'] = 43
    AreEqual(c.x, 43)
    
    # now check a user get/set descriptor
    class MyDescriptor(object):
        def __get__(self, *args): return 42
        def __set__(self, *args): pass
        
    class C(object):
        x = MyDescriptor()
        
    c = C()
    c.__dict__['x'] = 43
    AreEqual(c.x, 42)

def test_object_as_condition():
    class C(object):
        def __mod__(self, other): return 1
    o = C()
    flag = 0
    if o % o: flag = 1   # the bug was causing cast error before
    AreEqual(flag, 1)

def test_unbound_class_method():
    class C(object):
        def f(): return 1
    
    x = C()
    AssertErrorWithPartialMessage(TypeError, "unbound method f() must be called with", lambda: C.f())
    AssertErrorWithPartialMessage(TypeError, "unbound method f() must be called with", lambda: C.f(C))
    AssertErrorWithPartialMessage(TypeError, "arguments (1 given)", lambda: C.f(x))
    AssertErrorWithPartialMessage(TypeError, "arguments (1 given)", lambda: x.f())
        
run_test(__name__)
