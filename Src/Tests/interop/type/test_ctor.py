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
skiptest("silverlight")

add_clr_assemblies("methodargs", "typesamples")

from Merlin.Testing import *
from Merlin.Testing.Call import *
from Merlin.Testing.TypeSample import *

def test_1_byref_arg():
    obj = ByRefParameters()

    #public void M100(ref int arg) { arg = 1; }
    f = obj.M100
    
    AreEqual(f(2), 1)
    AreEqual(f(arg = 3), 1)
    AreEqual(f(*(4,)), 1)
    AreEqual(f(**{'arg': 5}), 1)
    
    x = clr.Reference[int](6); AreEqual(f(x), None); AreEqual(x.Value, 1)
    x = clr.Reference[int](7); f(arg = x); AreEqual(x.Value, 1)
    x = clr.Reference[int](8); f(*(x,)); AreEqual(x.Value, 1)
    x = clr.Reference[int](9); f(**{'arg':x}); AreEqual(x.Value, 1)
    
    #public void M120(out int arg) { arg = 2; }
    f = obj.M120
    AreEqual(f(), 2)
    #AssertError(TypeError, lambda: f(1))  # bug 311218
    
    x = clr.Reference[int](); AreEqual(f(x), None); AreEqual(x.Value, 2)
    x = clr.Reference[int](7); f(arg = x); AreEqual(x.Value, 2)
    x = clr.Reference[int](8); f(*(x,)); AreEqual(x.Value, 2)
    x = clr.Reference[int](9); f(**{'arg':x}); AreEqual(x.Value, 2)

def test_2_byref_args():
    obj = ByRefParameters()

    #public void M200(int arg1, ref int arg2) { Flag.Reset(); Flag.Value1 = arg1 * 10 + arg2; arg2 = 10; }
    f = obj.M200
    AreEqual(f(1, 2), 10); Flag.Check(12)
    AreEqual(f(3, arg2 = 4), 10); Flag.Check(34)
    AreEqual(f(arg2 = 6, arg1 = 5), 10); Flag.Check(56)
    AreEqual(f(*(7, 8)), 10); Flag.Check(78)
    AreEqual(f(9, *(1,)), 10); Flag.Check(91)
    
    x = clr.Reference[int](5); AreEqual(f(1, x), None); AreEqual(x.Value, 10); Flag.Check(15)
    x = clr.Reference[int](6); f(2, x); AreEqual(x.Value, 10); Flag.Check(26)
    x = clr.Reference[int](7); f(3, *(x,)); AreEqual(x.Value, 10); Flag.Check(37)
    x = clr.Reference[int](8); f(**{'arg1': 4, 'arg2' : x}); AreEqual(x.Value, 10); Flag.Check(48)
    
    #public void M201(ref int arg1, int arg2) { Flag.Reset(); Flag.Value1 = arg1 * 10 + arg2; arg1 = 20; }
    f = obj.M201
    AreEqual(f(1, 2), 20)
    x = clr.Reference[int](2); f(x, *(2,)); AreEqual(x.Value, 20); Flag.Check(22)
    
    #public void M202(ref int arg1, ref int arg2) { Flag.Reset(); Flag.Value1 = arg1 * 10 + arg2; arg1 = 30; arg2 = 40; }
    f = obj.M202
    AreEqual(f(1, 2), (30, 40))
    AreEqual(f(arg2 = 1, arg1 = 2), (30, 40)); Flag.Check(21)
    
    AssertErrorWithMessage(TypeError, "expected int, got StrongBox[int]", lambda: f(clr.Reference[int](3), 4))  # bug 311239
    x = clr.Reference[int](3)
    y = clr.Reference[int](4)
    #f(arg2 = y, *(x,)); Flag.Check(34) # bug 311169
    AssertErrorWithMessage(TypeError, "M202() got multiple values for keyword argument 'arg1'", lambda: f(arg1 = x, *(y,))) # msg
    
    # just curious
    x = y = clr.Reference[int](5)
    f(x, y); AreEqual(x.Value, 40); AreEqual(y.Value, 40); Flag.Check(55)

def test_2_out_args():
    obj = ByRefParameters()
    
    #public void M203(int arg1, out int arg2) { Flag.Reset(); Flag.Value1 = arg1 * 10; arg2 = 50; }
    f = obj.M203
    AreEqual(f(1), 50)
    AreEqual(f(*(2,)), 50)
    #AssertError(TypeError, lambda: f(1, 2))  # bug 311218
    
    x = clr.Reference[int](4)
    f(1, x); AreEqual(x.Value, 50)
    
    #public void M204(out int arg1, int arg2) { Flag.Reset(); Flag.Value1 = arg2; arg1 = 60; }
    # TODO
    
    #public void M205(out int arg1, out int arg2) { arg1 = 70; arg2 = 80; }
    f = obj.M205
    AreEqual(f(), (70, 80))
    AssertErrorWithMessage(TypeError, "M205() takes at most 2 arguments (1 given)", lambda: f(1))
    #AssertErrorWithMessage(TypeError, "M205() ??)", lambda: f(1, 2))
    
    AssertErrorWithMessage(TypeError, "M205() takes at most 2 arguments (1 given)", lambda: f(arg2 = clr.Reference[int](2)))
    AssertErrorWithMessage(TypeError, "M205() takes at most 2 arguments (1 given)", lambda: f(arg1 = clr.Reference[int](2)))
    
    for l in [
        lambda: f(*(x, y)),
        lambda: f(x, y, *()),
        lambda: f(arg2 = y, arg1 = x, *()),
        lambda: f(x, arg2 = y, ),
        lambda: f(x, **{"arg2":y})
             ]:
        x, y = clr.Reference[int](1), clr.Reference[int](2)
        #print l
        l()
        AreEqual(x.Value, 70)
        AreEqual(y.Value, 80)
    
    
    #public void M206(ref int arg1, out int arg2) { Flag.Reset(); Flag.Value1 = arg1 * 10; arg1 = 10; arg2 = 20; }
    f = obj.M206
    AreEqual(f(1), (10, 20))
    AreEqual(f(arg1 = 2), (10, 20))
    AreEqual(f(*(3,)), (10, 20))
    AssertError(TypeError, lambda: f(clr.Reference[int](5)))
   
    x, y = clr.Reference[int](4), clr.Reference[int](5)
    f(x, y); AreEqual(x.Value, 10); AreEqual(y.Value, 20); 
    
    #public void M207(out int arg1, ref int arg2) { Flag.Reset(); Flag.Value1 = arg2; arg1 = 30; arg2 = 40; }
    
    f = obj.M207
    AreEqual(f(1), (30, 40))
    AreEqual(f(arg2 = 2), (30, 40)); Flag.Check(2)
    #AssertError(TypeError, lambda: f(1, 2))
    AssertError(TypeError, lambda: f(arg2 = 1, arg1 = 2))
    
    for l in [ 
            lambda: f(x, y), 
            lambda: f(arg2 = y, arg1 = x),
            lambda: f(x, *(y,)),
            lambda: f(*(x, y,)),
            #lambda: f(arg1 = x, *(y,)),
            lambda: f(arg1 = x, **{'arg2': y}),
             ]:
        x, y = clr.Reference[int](1), clr.Reference[int](2)
        #print l
        l()
        AreEqual(x.Value, 30)
        AreEqual(y.Value, 40)
        Flag.Check(2)

    #// 1 argument 
    #public class Ctor100 {
        #public Ctor100(int arg) { }
    #}
    #public class Ctor101 {
        #public Ctor101([DefaultParameterValue(10)]int arg) { }
    #}
    #public class Ctor102 {
        #public Ctor102([Optional]int arg) { }
    #}
    
def test_ctor_1_arg():
    Ctor101()
    
    #public class Ctor103 {
    #   public Ctor103(params int[] arg) { }
    #}
    Ctor103()
    Ctor103(1)
    Ctor103(1, 2, 3)
    
    #public class Ctor110 {
    #   public Ctor110(ref int arg) { arg = 10; }
    #}
    
    # bug: 313995
    #Ctor110(2)

    #x = clr.Reference[int]()
    #Ctor110(x)
    #AreEqual(x.Value, 10)  # bug 313045
    

    #public class Ctor111 {
    #   public Ctor111(out int arg) { arg = 10; }
    #}

    #Ctor111() # bug 312981

    #x = clr.Reference[int]()
    #Ctor111(x)
    #AreEqual(x.Value, 10)   # bug 313045
    
def test_object_array_as_ctor_args():
    from System import Array
    Ctor104(Array[object]([1,2]))
    
def test_ctor_keyword():
    def check(o):
        Flag[int, int, int].Check(1, 2, 3)
        AreEqual(o.Arg4, 4)
        Flag[int, int, int].Reset()
        
    x = 4
    o = Ctor610(1, arg2 = 2, Arg3 = 3, Arg4 = x); check(o)
    o = Ctor610(Arg3 = 3, Arg4 = x, arg1 = 1, arg2 = 2); check(o)
    #o = Ctor610(Arg3 = 3, Arg4 = x, *(1, 2)); check(o)

# parameter name is same as property
def test_ctor_keyword2():
    Ctor620(arg1 = 1)
    f = Flag[int, int, int, str]
    o = Ctor620(arg1 = 1, arg2 = 2); f.Check(1, 2, 0, None); f.Reset()
    o = Ctor620(arg1 = 1, arg2 = "hello"); f.Check(1, 0, 0, "hello"); f.Reset()
    #Ctor620(arg1 = 1, arg2 = 2, **{ 'arg1' : 3})
    pass

def test_ctor_bad_property_field():
    AssertErrorWithMessage(AttributeError, "Property ReadOnlyProperty is read-only", lambda: Ctor700(1, ReadOnlyProperty = 1))
    AssertErrorWithMessage(AttributeError, "Field ReadOnlyField is read-only", lambda: Ctor720(ReadOnlyField = 2))
    AssertErrorWithMessage(AttributeError, "Field LiteralField is read-only", lambda: Ctor730(LiteralField = 3))
    #AssertErrorWithMessage(AttributeError, "xxx", lambda: Ctor710(StaticField = 10))
    #AssertErrorWithMessage(AttributeError, "xxx", lambda: Ctor750(StaticProperty = 10))
    AssertErrorWithMessage(TypeError, "Ctor760() takes no arguments (1 given)", lambda: Ctor760(InstanceMethod = 1))
    AssertErrorWithMessage(TypeError, "expected EventHandler, got int", lambda: Ctor760(MyEvent = 1))

def test_set_field_for_value_type_in_ctor():
    # with all fields set
    x = Struct(IntField = 2, StringField = "abc", ObjectField = 4)
    AreEqual(x.IntField, 2)
    AreEqual(x.StringField, "abc")
    AreEqual(x.ObjectField, 4)

    # with partial field set
    x = Struct(StringField = "def")
    AreEqual(x.IntField, 0)
    AreEqual(x.StringField, "def")
    AreEqual(x.ObjectField, None)
    
    # with not-existing field as keyword
    # http://vstfdevdiv:8080/WorkItemTracking/WorkItem.aspx?artifactMoniker=361389
    AssertErrorWithMessage(TypeError, 
        "CreateInstance() takes no arguments (2 given)", 
        lambda: Struct(IntField = 2, FloatField = 3.4))
    
    # set with value of "wrong" type
    # http://vstfdevdiv:8080/WorkItemTracking/WorkItem.aspx?artifactMoniker=361389
    AssertErrorWithMessage(TypeError, 
        "expected str, got int", 
        lambda: Struct(StringField = 2))

run_test(__name__)

