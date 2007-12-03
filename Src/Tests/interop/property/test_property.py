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
add_clr_assemblies("propertydefinitions", "typesamples")

from Merlin.Testing import *
from Merlin.Testing.Property import *
from Merlin.Testing.TypeSample import *

def test_explicitly_implemented_property():
    for t in [ClassExplicitlyImplement, StructExplicitlyImplement]:
        x = t()

        AssertError(AttributeError, lambda: x.Number)
        
        d = IData.Number
        d.SetValue(x, 20)
        AreEqual(d.GetValue(x), 20)
        
        d.__set__(x, 30)
        AreEqual(d.__get__(x), 30)
        
    x = ClassExplicitlyReadOnly()
    d = IReadOnlyData.Number
    AssertErrorWithMessage(SystemError, "cannot set property", lambda: d.SetValue(x, "abc"))
    AreEqual(d.GetValue(x), "python")
    #AssertErrorWithMessage(AttributeError, "ddd", lambda: d.__set__(x, "abc"))
    AreEqual(d.__get__(x), "python")
    
    x = StructExplicitlyWriteOnly()
    d = IWriteOnlyData.Number
    d.SetValue(x, SimpleStruct(3)); Flag.Check(10)
    AssertErrorWithMessage(AttributeError, "unreadable property", lambda: d.GetValue(x))
    d.__set__(x, SimpleStruct(30)); Flag.Check(10)
    AssertErrorWithMessage(AttributeError, "unreadable property", lambda: d.__get__(x))
    
    
def test_readonly():
    x = ClassWithReadOnly()
    
    AreEqual(x.InstanceProperty, 9)
    def f(): x.InstanceProperty = 10
    AssertErrorWithMessage(AttributeError, "'ClassWithReadOnly' object has no attribute 'InstanceProperty'", f)
    
    AreEqual(ClassWithReadOnly.StaticProperty, "dlr")
    def f(): ClassWithReadOnly.StaticProperty = 'abc'
    AssertErrorWithMessage(AttributeError, "'ClassWithReadOnly' object has no attribute 'StaticProperty'", f)

def test_writeonly():
    x = ClassWithWriteOnly()
    
    AssertErrorWithMessage(AttributeError, "InstanceProperty", lambda: x.InstanceProperty)  # msg
    x.InstanceProperty = 1; Flag.Check(11)
    
    #print ClassWithWriteOnly.StaticProperty
    
def test_readonly_writeonly_derivation():
    x = WriteOnlyDerived()
    
    x.Number = 100; Flag.Check(100)
    AssertErrorWithMessage(AttributeError, "Number", lambda: x.Number)
    
    AreEqual(x.get_Number(), 21)
    x.set_Number(101); Flag.Check(101)
    
run_test(__name__)

