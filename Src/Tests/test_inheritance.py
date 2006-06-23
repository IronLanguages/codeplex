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

import sys
import System
load_iron_python_test()
from IronPythonTest import *

class InheritedClass(BaseClass):
    def ReturnHeight(self):
        return self.Height
    def ReturnWidth(self):
        return self.Width
    def ReturnSize(self):
        return self.Size

i = InheritedClass()

Assert(i.Width == 0)
Assert(i.Height == 0)
Assert(i.Size.width == 0)
Assert(i.Size.height == 0)
Assert(i.ReturnHeight() == 0)
Assert(i.ReturnWidth() == 0)
Assert(i.ReturnSize().width == 0)
Assert(i.ReturnSize().height == 0)

i.Width = 1
i.Height = 2

Assert(i.Width == 1)
Assert(i.Height == 2)
Assert(i.Size.width == 1)
Assert(i.Size.height == 2)
Assert(i.ReturnHeight() == 2)
Assert(i.ReturnWidth() == 1)
Assert(i.ReturnSize().width == 1)
Assert(i.ReturnSize().height == 2)

s = MySize(3, 4)

i.Size = s

Assert(i.Width == 3)
Assert(i.Height == 4)
Assert(i.Size.width == 3)
Assert(i.Size.height == 4)
Assert(i.ReturnHeight() == 4)
Assert(i.ReturnWidth() == 3)
Assert(i.ReturnSize().width == 3)
Assert(i.ReturnSize().height == 4)


class InheritFromMarshalByRefObject(System.MarshalByRefObject):
    pass


class StaticConstructorInherit(BaseClassStaticConstructor):
    pass

sci = StaticConstructorInherit()

Assert(sci.Value == 10)

class PythonDerived(Overriding):
    def TemplateMethod(self):
        return "From Python"

    def BigTemplateMethod(self, *args):
        return ":".join(str(arg) for arg in args)

    def AbstractTemplateMethod(self):
        return "Overriden"

o = PythonDerived()
AreEqual(o.TopMethod(), "From Python - and Top")

del PythonDerived.TemplateMethod
AreEqual(o.TopMethod(), "From Base - and Top")

def NewTemplateMethod(self):
    return "From Function"

PythonDerived.TemplateMethod = NewTemplateMethod
AreEqual(o.TopMethod(), "From Function - and Top")

AreEqual(o.BigTopMethod(), "0:1:2:3:4:5:6:7:8:9 - and Top")

del PythonDerived.BigTemplateMethod
AreEqual(o.BigTopMethod(), "BaseBigTemplate - and Top")


AreEqual(o.AbstractTopMethod(), "Overriden - and Top")
del PythonDerived.AbstractTemplateMethod

AssertError(AttributeError, o.AbstractTopMethod)

class PythonDerived2(PythonDerived):
    def TemplateMethod(self):
        return "Python2"

o = PythonDerived2()
AreEqual(o.TopMethod(), "Python2 - and Top")


del PythonDerived2.TemplateMethod
##!!! TODO
##AreEqual(o.TopMethod(), "From Function - and Top")

del PythonDerived.TemplateMethod
AreEqual(o.TopMethod(), "From Base - and Top")


AreEqual(o.BigTopMethod(), "BaseBigTemplate - and Top")
#########################################################

class CTest(Inherited):
    def TopMethod(self):
        return "CTest"

o = CTest()

#########################################################
class C(ITestIt1, ITestIt2):
    def Method(self, x=None):
        if x: return "Python"+`x`
        else: return "Python"

o = C()
AreEqual(TestIt.DoIt1(o), "Python")
AreEqual(TestIt.DoIt2(o), "Python42")

# inheritance from a single interface should be verifable

class SingleInherit(ITestIt1): pass

import System
class Point(System.IFormattable):
    def __init__(self, x, y):
        self.x, self.y = x, y

    def ToString(self, format=None, fp=None):
        if format == 'x': return `(self.x, self.y)`
        return "Point(%r, %r)" % (self.x, self.y)

p = Point(1,2)
AreEqual(p.ToString(), "Point(1, 2)")
AreEqual(p.ToString('x', None), "(1, 2)")
#System.Console.WriteLine("{0}", p)

class MetaClass(type):
    def __new__(metacls, name, bases, vars):
        cls = type.__new__(metacls, name, bases, vars)
        return cls

MC = MetaClass('Foo', (), {})
AreEqual(dir(MC), ['__class__', '__doc__', '__module__'])

# metaclass such as defined in string.py
class MetaClass2(type):
	def __init__(metacls, name, bases, vars):
		super(MetaClass2, name).__init__
#!!! more meta testing todo


#########################################################
class OverrideParamTesting(MoreOverridding):
    def Test1(self, args):
        return "xx" + args[0] + args[1]
    def Test2(self, x, args):
        return "xx" + x + args[0] + args[1]
    def Test3(self, xr):
        xr.Value += "xx"
        return "Test3"
    def Test4(self, x, yr):
        yr.Value += x
        return "Test4"
    def Test5(self, sc):
        return "xx" + repr(sc)
    def Test6(self, x, sc):
        return "xx" + x + repr(sc)

a = OverrideParamTesting()
x = a.CallTest1()
Assert(x == 'xxaabb')
x = a.CallTest2()
Assert(x == 'xxaabbcc')
AreEqual(a.CallTest3("@"), ("Test3", "@xx"))
AreEqual(a.CallTest4("@"), ("Test4", "@aa"))
x = a.CallTest5()
Assert(x == 'xxOrdinal')
x = a.CallTest6()
Assert(x == 'xxaaOrdinal')

try:
    a.CallTest3()
    Assert(False)
except TypeError:
    pass

try:
    a.CallTest4()
    Assert(False)
except TypeError:
    pass



##############################################################

class _ToMangle(object):
    def getPriv(self):
        return self.__value
    def setPriv(self, val):
        self.__value = val

class AccessMangled(_ToMangle):
    def inheritGetPriv(self):
        return self._ToMangle__value

a = AccessMangled()
a.setPriv('def')
Assert(a.inheritGetPriv() == 'def')

##############################################################

class Test(BaseClass):
        def __init__(self):
            BaseClass.__init__(self, Width = 20, Height = 30)

a = Test()
Assert(a.Width == 20)
Assert(a.Height == 30)

class Test(MoreOverridding):
    def Test1(self, *p):
        return "Override!"

class OuterTest(Test): pass

a = OuterTest()
Assert(a.Test1() == "Override!")

class OuterTest(Test):
    def Test1(self, *p):
        return "Override Outer!"

a = OuterTest()
Assert(a.Test1() == "Override Outer!")


# BUG 463
class PythonBaseClass:
    def Func(self):
        print "PBC::Func"

class PythonDerivedClass(PythonBaseClass): pass

Assert('Func' in dir(PythonDerivedClass))
Assert('Func' in dir(PythonDerivedClass()))

class PythonDerivedFromCLR(System.Collections.ArrayList): pass
#
# now check that methods of Uri appear
Assert('ReadOnly' in dir(PythonDerivedFromCLR))
Assert('GetRange' in dir(PythonDerivedFromCLR()))
# 


##############################################################

class Flag:  pass
f = Flag()

##
## Subclassing once as python class, use it; and pass into CLI again
##
class PythonClass(CliInterface):
    def M1(self):
        f.v = 100
    def M2(self, x):
        f.v = 200 + x

p = PythonClass()
p.M1()
AreEqual(f.v, 100)

p.M2(1)
AreEqual(f.v, 201)

c = UseCliClass()
c.AsParam10(p)
AreEqual(f.v, 100)

#Bug 562
#c.AsParam11(p, 2)
#AreEqual(f.v, 202)

p.InstanceAttr = 200
p2 = c.AsRetVal10(p)
AreEqual(p, p2)
AreEqual(p2.InstanceAttr, 200)

# normal scenario
class PythonClass(CliAbstractClass):
    def MV(self, x):
        f.v = 300 + x
    def MA(self, x):
        f.v = 400 + x

p = PythonClass()
p.MS(1)
AreEqual(p.helperF, -2 * 1)

p.MI(2)
AreEqual(p.helperF, -3 * 2)

p.MV(3)
AreEqual(f.v, 303)

p.MA(4)
AreEqual(f.v, 404)

c.AsParam20(p, 3)
AreEqual(f.v, 303)

c.AsParam21(p, 4)
AreEqual(f.v, 404)

c.AsParam22(p, 2)
AreEqual(p.helperF,  - 2*2)

c.AsParam23(p, 1)
AreEqual(p.helperF, - 3*1)

# virtual function is not overriden in the python class: call locally, and pass back to clr and call.
class PythonClass(CliAbstractClass): pass
p = PythonClass()
p.MV(5)
AreEqual(p.helperF, -4 * 5)

c.AsParam20(p, 6)
AreEqual(p.helperF, -4 * 6)

# "override" a  non-virtual method, and pass the object back to clr. This method should not be called
class PythonClass(CliAbstractClass):
    def MI(self, x):
            f.v = x * 300
p = PythonClass()
f.v = 0
c.AsParam23(p, 2)
AreEqual(f.v, 0)
AreEqual(p.helperF, -3 * 2)

##
## Subclassing twice
##
class PythonClass(CliInterface): pass
class PythonClass2(PythonClass): pass

class PythonClass(CliAbstractClass): pass
class PythonClass2(PythonClass): pass

##
## Negative cases: struct, enum, delegate
##
try:
    class PythonClass(MySize): pass
    Fail("should thrown")
except TypeError:    pass

try:
    class PythonClass(DaysInt): pass
    Fail("should thrown")
except TypeError:    pass

try:
    class PythonClass(VoidDelegate): pass
    Fail("should thrown")
except TypeError:    pass


##
## All Virtual stuff can be overriden? and run
##
class PythonClass(CliVirtualStuff): pass
class PythonClass2(PythonClass):
    def VirtualMethod(self, x):
        return 20 * x
    def VirtualPropertyGetter(self):
        return 20 * self.InstanceAttr
    def VirtualPropertySetter(self, x):
        self.InstanceAttr = x
    VirtualProperty = property(VirtualPropertyGetter, VirtualPropertySetter)

    def VirtualProtectedMethod(self): return 2000
    VirtualProtectedProperty = property(VirtualPropertyGetter, VirtualPropertySetter)


p = PythonClass()
AreEqual(p.VirtualMethod(1), 10)
p.VirtualProperty = 99
AreEqual(p.VirtualProperty, 99)

p2 = PythonClass2()
AreEqual(p2.VirtualMethod(1), 20)
p2.VirtualProperty = -1
AreEqual(p2.VirtualProperty, -20)

Assert(p2.PublicStuffCheckHelper(-1 * 20,20 * 10))

p2.VirtualProtectedProperty = 999
Assert(p2.ProtectedStuffCheckHelper(999 * 20,2000))


##############################################################

result = type('a', (list,), dict()) ((1,2))
result = type('a', (str,), dict()) ('abc')
result = type('a', (tuple,), dict()) ('abc')
result = type('a', (dict,), dict()) ()
result = type('a', (int,), dict()) (0)



class foo(object):
    def __str__(self):
        return 'abc'
        
        
a = foo()
AreEqual(str(a), 'abc')

##############################################################
# set virtual override on an instance method

class Foo(Overriding):
    pass
    
def MyTemplate(self):
    return "I'm Derived"
    
a = Foo()
a.TemplateMethod = type(a.TemplateMethod)(MyTemplate, a)

AreEqual(a.TopMethod(), "I'm Derived - and Top")




##############################################################
# new / init combos w/ inheritance from CLR class


# CtorTest has no __init__, so the parameters passed directly
# to Foo in these tests will always go to the ctor.

# 3 ints
class Foo(CtorTest):
    def __init__(self, a, b, c):
            AreEqual(self.CtorRan, 0)
            super(CtorTest, self).__init__(a, b, c)

a = Foo(2,3,4)


# 3 strings
class Foo(CtorTest):
    def __init__(self, a, b, c):
            AreEqual(self.CtorRan, 1)
            super(CtorTest, self).__init__(a, b, c)

a = Foo("2","3","4")


# single int, init adds extra args
class Foo(CtorTest):
    def __init__(self, a):
        AreEqual(self.CtorRan, 3)
        super(CtorTest, self).__init__(a, 2, 3)

a = Foo(2)

# single string, init adds extra args

class Foo(CtorTest):
    def __init__(self, a):
        AreEqual(self.CtorRan, 2)
        super(CtorTest, self).__init__(a, "2", "3")

a = Foo("2")


# single string (shoudl go to string overload)
class Foo(CtorTest):
    def __init__(self):
        AreEqual(self.CtorRan, -1)
        super(CtorTest, self).__init__("2")

a = Foo()

# single int (should go to object overload)
class Foo(CtorTest):
    def __init__(self):
        AreEqual(self.CtorRan, -1)
        super(CtorTest, self).__init__(2)

a = Foo()


# init adds int, we call w/ no args, should go to object
class Foo(CtorTest):
    def __init__(self):
        AreEqual(self.CtorRan, -1)
        super(CtorTest, self).__init__(2)


a = Foo()

class Foo(CtorTest):
    def __init__(self):
        AreEqual(self.CtorRan, -1)
        super(CtorTest, self).__init__(2,3,4)


a = Foo()


########################################################
# verify we can't call it w/ bad args...

class Foo(CtorTest):
    def __init__(self):
            super(CtorTest, self).__init__()

def BadFoo():
	a = Foo(2,3,4,5)

AssertError(TypeError, BadFoo)


########################################################

# now run the __new__ tests.  Overriding __new__ should
# allow us to change the parameters that can be passed
# to create the function


class Foo(CtorTest):
    def __new__(cls, a, b, c):
            ret = CtorTest.__new__(CtorTest, a, b, c)            
            return ret

a = Foo(2,3,4)
AreEqual(a.CtorRan, 0)


a = Foo("2","3","4")
AreEqual(a.CtorRan, 1)

# use var-args to invoke arbitrary overloads...

class Foo(CtorTest):
    def __new__(cls, *args):
            ret = CtorTest.__new__(CtorTest, *args)            
            return ret


a = Foo(2,3,4)
AreEqual(a.CtorRan, 0)

a = Foo("2","3","4")
AreEqual(a.CtorRan, 1)

a = Foo("abc")
AreEqual(a.CtorRan, 2)

a = Foo([])
AreEqual(a.CtorRan, 3)

########################################################
# new/init combo tests...


class Foo(CtorTest):
    def __new__(cls, *args):
        ret = CtorTest.__new__(CtorTest, *args)            
        return ret
    def __init__(self, *args):  pass
      
                
# empty init, we should be able to create any of them...

a = Foo(2,3,4)
AreEqual(a.CtorRan, 0)

a = Foo("2","3","4")
AreEqual(a.CtorRan, 1)

a = Foo("abc")
AreEqual(a.CtorRan, 2)

a = Foo([])
AreEqual(a.CtorRan, 3)



class Foo(CtorTest):
    def __new__(cls, *args):
        ret = CtorTest.__new__(Foo, *args)            
        return ret
    def __init__(self): 
        super(CtorTest, self).__init__(self)

#ok, we have a compatbile init...
a = Foo()
AreEqual(a.CtorRan, -1)

#should all fail due to incompatible init.
AssertError(TypeError, Foo, 2,3,4)
AssertError(TypeError, Foo, "2","3","4")
AssertError(TypeError, Foo, "abc")
AssertError(TypeError, Foo, [])


class Foo(CtorTest):
    def __new__(cls, *args):
        ret = CtorTest.__new__(Foo, *args)            
        return ret
    def __init__(self, x): 
        super(CtorTest, self).__init__(self, x)


a = Foo("abc")
AreEqual(a.CtorRan, 2)
a = Foo([])
AreEqual(a.CtorRan, 3)

AssertError(TypeError, Foo)
AssertError(TypeError, Foo, 2, 3, 4)
AssertError(TypeError, Foo, "2", "3", "4")
AssertError(TypeError, Foo, "2", "3", "4", "5")

##################
# verify tuple is ok after deriving from it.

class T(tuple):
    pass

result = 'a' in ('c','d','e')



##################
# inheriting from string should allow us to create extensible strings w/ no params

class MyString(str): pass

s = MyString()

AreEqual(s, '')

#################
# inheritance from an interface w/ a property

class foo(ITestIt3):
    def get_Foo(self): return 'abc'
    Name = property(fget=get_Foo)


a = foo()
AreEqual(a.Name, 'abc')


#######################################################################
#### test converter logics and EmitCastFromObject

#############################################
## no inherited stuffs, expecting those default value defined in CReturnTypes
def add(x, y): return x + y

class DReturnTypes(CReturnTypes): pass

used = UseCReturnTypes(DReturnTypes())
used.Use_void()
AreEqual(used.Use_Char(), System.Char.MaxValue)
AreEqual(used.Use_Int32(), System.Int32.MaxValue)
AreEqual(used.Use_String(), "string")
AreEqual(used.Use_Int64(), System.Int64.MaxValue)
AreEqual(used.Use_Double(), System.Double.MaxValue)
AreEqual(used.Use_Boolean(), True)
AreEqual(used.Use_Single(), System.Single.MaxValue)
AreEqual(used.Use_Byte(), System.Byte.MaxValue)
AreEqual(used.Use_SByte(), System.SByte.MaxValue)
AreEqual(used.Use_Int16(), System.Int16.MaxValue)
AreEqual(used.Use_UInt32(), System.UInt32.MaxValue)
AreEqual(used.Use_UInt64(), System.UInt64.MaxValue)
AreEqual(used.Use_UInt16(), System.UInt16.MaxValue)
AreEqual(used.Use_Type(), System.Type.GetType("System.Int32"))
AreEqual(used.Use_RtEnum(), RtEnum.A)
AreEqual(used.Use_RtDelegate().Invoke(30), 30 * 2)
AreEqual(used.Use_RtStruct().F, 1)
AreEqual(used.Use_RtClass().F, 1)
AreEqual(reduce(add, used.Use_IEnumerator()),  60)


#############################################
## inherited all, but with correct return types
## expect values defined here

def func(arg): return arg

flag = 10
class DReturnTypes(CReturnTypes):
    def M_void(self): global flag; flag = 20
    def M_Char(self): return System.Char.MinValue
    def M_Int32(self): return System.Int32.MinValue
    def M_String(self): return "hello"
    def M_Int64(self): return System.Int64.MinValue
    def M_Double(self): return System.Double.MinValue
    def M_Boolean(self): return False
    def M_Single(self): return System.Single.MinValue
    def M_Byte(self): return System.Byte.MinValue
    def M_SByte(self): return System.SByte.MinValue
    def M_Int16(self): return System.Int16.MinValue
    def M_UInt32(self): return System.UInt32.MinValue
    def M_UInt64(self): return System.UInt64.MinValue
    def M_UInt16(self): return System.UInt16.MinValue
    def M_Type(self): return System.Type.GetType("System.Int64")
    def M_RtEnum(self): return RtEnum.B
    def M_RtDelegate(self): return func
    def M_RtStruct(self): return RtStruct(20)
    def M_RtClass(self): return RtClass(30)
    def M_IEnumerator(self): return [1, 2, 3, 4, 5]

used = UseCReturnTypes(DReturnTypes())
used.Use_void()
AreEqual(flag, 20)
AreEqual(used.Use_Char(), System.Char.MinValue)
AreEqual(used.Use_Int32(), System.Int32.MinValue)
AreEqual(used.Use_String(), "hello")
AreEqual(used.Use_Int64(), System.Int64.MinValue)
AreEqual(used.Use_Double(), System.Double.MinValue)
AreEqual(used.Use_Boolean(), False)
AreEqual(used.Use_Single(), System.Single.MinValue)
AreEqual(used.Use_Byte(), System.Byte.MinValue)
AreEqual(used.Use_SByte(), System.SByte.MinValue)
AreEqual(used.Use_Int16(), System.Int16.MinValue)
AreEqual(used.Use_UInt32(), System.UInt32.MinValue)
AreEqual(used.Use_UInt64(), System.UInt64.MinValue)
AreEqual(used.Use_UInt16(), System.UInt16.MinValue)
AreEqual(used.Use_Type(), System.Type.GetType("System.Int64"))
AreEqual(used.Use_RtEnum(), RtEnum.B)
AreEqual(used.Use_RtDelegate().Invoke(100), 100)
AreEqual(used.Use_RtStruct().F, 20)
AreEqual(used.Use_RtClass().F, 30)
AreEqual(reduce(add, used.Use_IEnumerator()), 15)

#############################################
## inherited all, but returns with a python old class, or new class, 
##                    or with explicit ops

## return a class whose derived methods returns the same specified object
def create_class(retObj):
    class NewC(CReturnTypes):
        def M_void(self): return retObj
        def M_Char(self): return retObj
        def M_Int32(self): return retObj
        def M_String(self): return retObj
        def M_Int64(self): return retObj
        def M_Double(self): return retObj
        def M_Boolean(self): return retObj
        def M_Single(self): return retObj
        def M_Byte(self): return retObj
        def M_SByte(self): return retObj
        def M_Int16(self): return retObj
        def M_UInt32(self): return retObj
        def M_UInt64(self): return retObj
        def M_UInt16(self): return retObj
        def M_Type(self): return retObj
        def M_RtEnum(self): return retObj
        def M_RtDelegate(self): return retObj
        def M_RtStruct(self): return retObj
        def M_RtClass(self): return retObj
        def M_IEnumerator(self): return retObj
    return NewC

## all return None
DReturnTypes = create_class(None)

used = UseCReturnTypes(DReturnTypes())
AreEqual(used.Use_Type(), None)
AreEqual(used.Use_String(), None)
AreEqual(used.Use_RtDelegate(), None)
AreEqual(used.Use_RtClass(), None)
AreEqual(used.Use_Boolean(), False)
AreEqual(used.Use_IEnumerator(), None)

for f in [used.Use_Char, used.Use_Int32, used.Use_Int64, 
    used.Use_Double, used.Use_Single, used.Use_Byte, used.Use_SByte, used.Use_RtEnum,
    used.Use_Int16, used.Use_UInt32, used.Use_UInt64, used.Use_UInt16, used.Use_RtStruct, ]:
    AssertError(TypeError, f)

## return old class instance / user type instance
class python_old_class: pass
class python_new_class(object): pass

def check_behavior(expected_obj):
    DReturnTypes = create_class(expected_obj)
    used = UseCReturnTypes(DReturnTypes())
    
    AreEqual(used.Use_void(), None)
    AreEqual(used.Use_Boolean(), True)
    
    for f in [used.Use_Char, used.Use_Int32, used.Use_String, used.Use_Int64, 
        used.Use_Double, used.Use_Single, used.Use_Byte, used.Use_SByte, 
        used.Use_Int16, used.Use_UInt32, used.Use_UInt64, used.Use_UInt16, used.Use_Type, 
        used.Use_RtEnum, used.Use_RtStruct, used.Use_RtClass, used.Use_IEnumerator,
        #used.Use_RtDelegate,
        ]:
        AssertError(TypeError, f)
    
check_behavior(python_old_class())
check_behavior(python_new_class())  

## extensible int
class python_my_int(int): pass

def check_behavior(expected_obj):
    DReturnTypes = create_class(expected_obj)
    used = UseCReturnTypes(DReturnTypes())
    
    AreEqual(used.Use_void(), None)
    AreEqual(used.Use_Boolean(), True)
 
    AreEqual(used.Use_Int32(), System.Int32.Parse("10"))
    AreEqual(used.Use_Int64(), System.Int64.Parse("10"))
    AreEqual(used.Use_Double(), System.Double.Parse("10"))
    AreEqual(used.Use_Single(), System.Single.Parse("10"))
    AreEqual(used.Use_UInt32(), System.UInt32.Parse("10"))
    AreEqual(used.Use_UInt64(), System.UInt64.Parse("10"))

    for f in [used.Use_Char, used.Use_String, used.Use_Byte, used.Use_SByte, 
        used.Use_Int16, used.Use_UInt16, used.Use_Type, 
        used.Use_RtEnum, used.Use_RtStruct, used.Use_RtClass, used.Use_IEnumerator
        # used.Use_RtDelegate,
        ]:
        AssertError(TypeError, f)
        
check_behavior(python_my_int(10))

## customized __int__. __float__
class python_old_class:
    def __int__(self): return 100
    def __float__(self): return 12345.6
class python_new_class(object):    
    def __int__(self): return 100
    def __float__(self): return 12345.6
    
def check_behavior(expected_obj):
    DReturnTypes = create_class(expected_obj)
    used = UseCReturnTypes(DReturnTypes())
    
    AreEqual(used.Use_void(), None)
    AreEqual(used.Use_Boolean(), True)
    AreEqual(used.Use_Int32(), System.Int32.Parse("100"))
    AreEqual(used.Use_Double(), System.Double.Parse("12345.6"))

    for f in [used.Use_Char, used.Use_String, used.Use_Int64, 
        used.Use_Single, used.Use_Byte, used.Use_SByte, 
        used.Use_Int16, used.Use_UInt32, used.Use_UInt64, used.Use_UInt16, used.Use_Type, 
        used.Use_RtEnum, used.Use_RtStruct, used.Use_RtClass, used.Use_IEnumerator
        #used.Use_RtDelegate,
        ]:
        AssertError(TypeError, f)

check_behavior(python_old_class())
check_behavior(python_new_class())

#############################################
## inherited all, but with more interesting return types

class DReturnTypes(CReturnTypes): pass
drt = DReturnTypes()
used = UseCReturnTypes(drt)

def func(self): global flag; flag = 60; return 70
DReturnTypes.M_void = func
AreEqual(used.Use_void(), None)
AreEqual(flag, 60)

## Char 
DReturnTypes.M_Char = lambda self: ord('z')
AssertError(TypeError, used.Use_Char)

DReturnTypes.M_Char = lambda self: 'y'
AreEqual(used.Use_Char(), System.Char.Parse('y'))

DReturnTypes.M_Char = lambda self: ''
AssertError(TypeError, used.Use_Char)

DReturnTypes.M_Char = lambda self: 'abc'
AssertError(TypeError, used.Use_Char)

## String
DReturnTypes.M_String = lambda self: 'z'
AreEqual(used.Use_String(), 'z')

DReturnTypes.M_String = lambda self: ''
AreEqual(used.Use_String(), System.String.Empty)

DReturnTypes.M_String = lambda self: ord('z')
AssertError(TypeError, used.Use_String)

## Int32
DReturnTypes.M_Int32 = lambda self: System.Char.Parse('z')
AssertError(TypeError, used.Use_Int32())

DReturnTypes.M_Int32 = lambda self: System.SByte.Parse('-123')
AreEqual(used.Use_Int32(), -123)

DReturnTypes.M_Int32 = lambda self: 12345678901234
AssertError(OverflowError, used.Use_Int32)

## RtClass
class MyRtClass(RtClass): 
    def __init__(self, value):
        super(MyRtClass, self).__init__(value)
    
DReturnTypes.M_RtClass = lambda self: MyRtClass(500)
AreEqual(used.Use_RtClass().F, 500)

## IEnumerator
DReturnTypes.M_IEnumerator = lambda self: (2, 20, 200, 2000)
AreEqual(reduce(add, used.Use_IEnumerator()), 2222)

DReturnTypes.M_IEnumerator = lambda self: { 1 : "one", 10: "two", 100: "three"}
AreEqual(reduce(add, used.Use_IEnumerator()), 111)

DReturnTypes.M_IEnumerator = lambda self: System.Array[int](range(10))
AreEqual(reduce(add, used.Use_IEnumerator()), 45)

## RtDelegate
def func2(arg1, arg2): return arg1 * arg2 

DReturnTypes.M_RtDelegate = lambda self : func2
AssertError(TypeError, used.Use_RtDelegate)

#############################################
## Redefine non-virtual method:

class DReturnTypes(CReturnTypes): pass

used = UseCReturnTypes(DReturnTypes())
AreEqual(used.Use_NonVirtual(), 100)

class DReturnTypes(CReturnTypes): 
    def M_NonVirtual(self): return 200

used = UseCReturnTypes(DReturnTypes())
AreEqual(used.Use_NonVirtual(), 100)

#############################################
## Similar but smaller set of test for interface/abstract Type
##
def test_returntype(basetype, usetype):
    class derived(basetype): pass
    
    used = usetype(derived()) 

    for f in [ used.Use_void,used.Use_Char,used.Use_Int32,used.Use_String,used.Use_Int64,used.Use_Double,used.Use_Boolean,
        used.Use_Single,used.Use_Byte,used.Use_SByte,used.Use_Int16,used.Use_UInt32,used.Use_UInt64,used.Use_UInt16,
        used.Use_Type,used.Use_RtEnum,used.Use_RtDelegate,used.Use_RtStruct,used.Use_RtClass,used.Use_IEnumerator,
        ]:
        AssertError(AttributeError, f)
    
    class derived(basetype):
        def M_void(self): global flag; flag = 20
        def M_Char(self): return System.Char.MinValue
        def M_Int32(self): return System.Int32.MinValue
        def M_String(self): return "hello"
        def M_Int64(self): return System.Int64.MinValue
        def M_Double(self): return System.Double.MinValue
        def M_Boolean(self): return False
        def M_Single(self): return System.Single.MinValue
        def M_Byte(self): return System.Byte.MinValue
        def M_SByte(self): return System.SByte.MinValue
        def M_Int16(self): return System.Int16.MinValue
        def M_UInt32(self): return System.UInt32.MinValue
        def M_UInt64(self): return System.UInt64.MinValue
        def M_UInt16(self): return System.UInt16.MinValue
        def M_Type(self): return System.Type.GetType("System.Int64")
        def M_RtEnum(self): return RtEnum.B
        def M_RtDelegate(self): return lambda arg: arg * 5
        def M_RtStruct(self): return RtStruct(20)
        def M_RtClass(self): return RtClass(30)
        def M_IEnumerator(self): return [1, 2, 3, 4, 5]

    used = usetype(derived())
    used.Use_void()
    AreEqual(flag, 20)
    AreEqual(used.Use_Char(), System.Char.MinValue)
    AreEqual(used.Use_Int32(), System.Int32.MinValue)
    AreEqual(used.Use_String(), "hello")
    AreEqual(used.Use_Int64(), System.Int64.MinValue)
    AreEqual(used.Use_Double(), System.Double.MinValue)
    AreEqual(used.Use_Boolean(), False)
    AreEqual(used.Use_Single(), System.Single.MinValue)
    AreEqual(used.Use_Byte(), System.Byte.MinValue)
    AreEqual(used.Use_SByte(), System.SByte.MinValue)
    AreEqual(used.Use_Int16(), System.Int16.MinValue)
    AreEqual(used.Use_UInt32(), System.UInt32.MinValue)
    AreEqual(used.Use_UInt64(), System.UInt64.MinValue)
    AreEqual(used.Use_UInt16(), System.UInt16.MinValue)
    AreEqual(used.Use_Type(), System.Type.GetType("System.Int64"))
    AreEqual(used.Use_RtEnum(), RtEnum.B)
    AreEqual(used.Use_RtDelegate().Invoke(100), 100 * 5)
    AreEqual(used.Use_RtStruct().F, 20)
    AreEqual(used.Use_RtClass().F, 30)
    AreEqual(reduce(add, used.Use_IEnumerator()), 15)

test_returntype(IReturnTypes, UseIReturnTypes)
test_returntype(AReturnTypes, UseAReturnTypes)


# verify that classes w/ large vtables we get the
# correct method dispatch when overriding bases
class BigVirtualDerived(BigVirtualClass):
    def __init__(self):
        self.funcCalled = False
    for x in range(50):
        exec 'def M%d(self):\n    self.funcCalled = True\n    return super(type(self), self).M%d()' % (x, x)

a = BigVirtualDerived()
for x in range(50): 
    # call from Python
    AreEqual(a.funcCalled, False)
    AreEqual(x, getattr(a, 'M'+str(x))())
    AreEqual(a.funcCalled, True)
    a.funcCalled = False
    # call non-virtual method that calls from C#
    AreEqual(x, getattr(a, 'CallM'+str(x))())
    AreEqual(a.funcCalled, True)
    a.funcCalled = False
    

