#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################
    
import sys, nt

def environ_var(key): return [nt.environ[x] for x in nt.environ.keys() if x.lower() == key.lower()][0]

merlin_root = environ_var("MERLIN_ROOT")
sys.path.extend([merlin_root + r"\Languages\IronPython\Tests", merlin_root + r"\Test\ClrAssembly\bin"])

from lib.assert_util import *
skiptest("silverlight")

import clr
clr.AddReference("fieldtests", "typesamples")

from Merlin.Testing.FieldTest import *
from Merlin.Testing.TypeSample import *

def _test_get(this_type, field_source):
    ## accessing from object instance
    o = this_type()
    
    # get
    # - literal
    # bug# 299501
    #AreEqual(o.LiteralIntField, 10)
    #AreEqual(o.LiteralReferenceTypeField, None)

    # - initonly    
    AreEqual(o.InitOnlyIntField, 20)
    AreEqual(o.InitOnlyDateTimeField, System.DateTime(200))
    AreEqual(o.InitOnlyClassField.Number, 30)

    # - normal static 
    #   - value type
    AreEqual(o.IntStaticField, 0)
    AreEqual(o.EnumStaticField, EnumInt32.A)
    AreEqual(o.StructStaticField.Number, 0)
    AreEqual(o.GenericStructStaticField.Number, 0)
    
    #   - reference type
    AreEqual(o.ClassStaticField, None)
    AreEqual(o.DerivedClassStaticField, None)
    AreEqual(o.GenericClassStaticField, None)
    AreEqual(o.DerivedGenericClassStaticField, None)
    AreEqual(o.DerivedNonGenericClassStaticField, None)
    
    # - normal instance 
    #   - value type
    AreEqual(o.IntInstanceField, 0)
    AreEqual(o.EnumInstanceField, EnumInt32.A)
    AreEqual(o.StructInstanceField.Number, 0)
    AreEqual(o.GenericStructInstanceField.Number, 0)
    
    #   - reference type
    AreEqual(o.ClassInstanceField, None)
    AreEqual(o.DerivedClassInstanceField, None)
    AreEqual(o.GenericClassInstanceField, None)
    AreEqual(o.DerivedGenericClassInstanceField, None)
    AreEqual(o.DerivedNonGenericClassInstanceField, None)

    ## accessing from type
    
    # get
    # - literal
    AreEqual(this_type.LiteralIntField, 10)
    AreEqual(this_type.LiteralReferenceTypeField, None)

    # - initonly    
    AreEqual(this_type.InitOnlyIntField, 20)
    AreEqual(this_type.InitOnlyDateTimeField, System.DateTime(200))
    AreEqual(this_type.InitOnlyClassField.Number, 30)

    # - normal static 
    #   - value type
    AreEqual(this_type.IntStaticField, 0)
    AreEqual(this_type.EnumStaticField, EnumInt32.A)
    AreEqual(this_type.StructStaticField.Number, 0)
    AreEqual(this_type.GenericStructStaticField.Number, 0)
    
    #   - reference type
    AreEqual(this_type.ClassStaticField, None)
    AreEqual(this_type.DerivedClassStaticField, None)
    AreEqual(this_type.GenericClassStaticField, None)
    AreEqual(this_type.DerivedGenericClassStaticField, None)
    AreEqual(this_type.DerivedNonGenericClassStaticField, None)
    
    # ReflectedFields   
    AreEqual(str(this_type.IntInstanceField), "<field# IntInstanceField on %s>" % field_source)
    AssertError(TypeError, lambda: this_type.IntInstanceField())
    AssertError(TypeError, lambda: this_type.IntInstanceField(o))
    
    AreEqual(this_type.IntInstanceField.__get__(o, System.Object), 0) # works even with System.Object
    
    AreEqual(this_type.IntInstanceField.__get__(o, this_type), 0)
    AreEqual(this_type.EnumInstanceField.__get__(o, this_type), EnumInt32.A)
    AreEqual(this_type.StructInstanceField.__get__(o, this_type).Number, 0)
    AreEqual(this_type.GenericStructInstanceField.__get__(o, this_type).Number, 0)

    AreEqual(this_type.ClassInstanceField.__get__(o, this_type), None)
    AreEqual(this_type.DerivedClassInstanceField.__get__(o, this_type), None)
    AreEqual(this_type.GenericClassInstanceField.__get__(o, this_type), None)
    AreEqual(this_type.DerivedGenericClassInstanceField.__get__(o, this_type), None)
    AreEqual(this_type.DerivedNonGenericClassInstanceField.__get__(o, this_type), None)
    
    # - normal instance 
    # bug# 299522: Support GetValue
    #   - value type
    #AreEqual(this_type.IntInstanceField.GetValue(o), 0)
    #AreEqual(this_type.EnumInstanceField.GetValue(o), EnumInt32.A)
    #AreEqual(this_type.StructInstanceField.GetValue(o).Number, 0)
    #AreEqual(this_type.GenericStructInstanceField.GetValue(o).Number, 0)

    #   - reference type
    #AreEqual(this_type.ClassInstanceField.GetValue(o), None)
    #AreEqual(this_type.DerivedClassInstanceField.GetValue(o), None)
    #AreEqual(this_type.GenericClassInstanceField.GetValue(o), None)
    #AreEqual(this_type.DerivedGenericClassInstanceField.GetValue(o), None)
    #AreEqual(this_type.DerivedNonGenericClassInstanceField.GetValue(o), None)    

def test_1_value_type_get(): _test_get(ValueType, "ValueType")
def test_1_generic_value__type_of_value_type_get(): _test_get(GenericValueType[int], "GenericValueType`1")
def test_1_generic_value__type_of_reference_type_get(): _test_get(GenericValueType[str], "GenericValueType`1")

def test_1_reference_type_get(): _test_get(ReferenceType, "ReferenceType")
def test_1_derived_reference_type_get(): _test_get(DerviedReferenceType, "ReferenceType")

def test_1_generic_reference_type_of_value_type_get(): _test_get(GenericReferenceType[long], "GenericReferenceType`1")
def test_1_generic_reference_type_of_reference_type_get(): _test_get(GenericReferenceType[System.Object], "GenericReferenceType`1")

def test_1_derived_generic_reference_type_of_value_type_get(): _test_get(DerivedGenericReferenceType[long], "GenericReferenceType`1")
def test_1_derived_generic_reference_type_of_reference_type_get(): _test_get(DerivedGenericReferenceType[System.Object], "GenericReferenceType`1")

def test_1_derived_generic_reference_type_of_value_type_get2(): _test_get(DerivedGenericReferenceTypeOfValueType, "GenericReferenceType`1")
def test_1_derived_generic_reference_type_of_reference_type_get2(): _test_get(DerivedGenericReferenceTypeOfReferenceType, "GenericReferenceType`1")

def _calling_get_on_instance_fields(this_type):
    o = this_type()
    
    ### instance fields
    expected_values = { 
        'IntInstanceField' : 0, 
        'EnumInstanceField' : EnumInt32.A, 

        'ClassInstanceField' : None, 
        'DerivedClassInstanceField' : None, 
        'GenericClassInstanceField' : None, 
        'DerivedGenericClassInstanceField' : None,
        'DerivedNonGenericClassInstanceField' : None,
    }
    
    for (k, v) in expected_values.iteritems():
        desc = this_type.__dict__[k]
        AreEqual(desc.__get__(o, this_type), v)
        AreEqual(desc.__get__(None, this_type), desc)
        
    expected_values = { 
        'StructInstanceField' : 0, 
        'GenericStructInstanceField' : 0,
    }
    
    for (k, v) in expected_values.iteritems():
        desc = this_type.__dict__[k]
        AreEqual(desc.__get__(o, this_type).Number, v)
        AreEqual(desc.__get__(None, this_type), desc)

def _test_get_via_descriptor(this_type):
    ### static fields
    o = this_type()

    expected_values = { 
        'LiteralIntField' : 10, 'LiteralReferenceTypeField' : None, 
        'InitOnlyIntField' : 20, 'InitOnlyDateTimeField' : System.DateTime(200), 
        'IntStaticField' : 0, 'EnumStaticField' : EnumInt32.A, 
        
        # field type is reference type
        'ClassStaticField' : None, 
        'DerivedClassStaticField' : None, 
        'GenericClassStaticField' : None, 
        'DerivedGenericClassStaticField' : None,
        'DerivedNonGenericClassStaticField' : None,
    }
    
    for (k, v) in expected_values.iteritems():
        AreEqual(this_type.__dict__[k].__get__(o, this_type), v)
        AreEqual(this_type.__dict__[k].__get__(None, this_type), v)
        
    # field type is value type (which has the Number property)
    expected_values = { 
        'InitOnlyClassField' : 30, 
        'StructStaticField' : 0, 
        'GenericStructStaticField' : 0,
    }
    
    for (k, v) in expected_values.iteritems():
        AreEqual(this_type.__dict__[k].__get__(o, this_type).Number, v)
        AreEqual(this_type.__dict__[k].__get__(None, this_type).Number, v)

    ### instance fields
    _calling_get_on_instance_fields(this_type)
            
def _test_get_via_descriptor_from_derived_type(this_type):
    ### static fields
    for k in [
        'LiteralIntField', 'LiteralReferenceTypeField', 
        'InitOnlyIntField', 'InitOnlyDateTimeField', 'InitOnlyClassField', 
        'IntStaticField', 'EnumStaticField', 'StructStaticField', 'GenericStructStaticField', 
        'ClassStaticField', 'DerivedClassStaticField', 
        'GenericClassStaticField', 'DerivedGenericClassStaticField', 'DerivedNonGenericClassStaticField', 
    ]:
        Assert(k not in this_type.__dict__)

    ### instance fields
    _calling_get_on_instance_fields(this_type)
      
        
def test_1_value_type_get_via_descriptor(): _test_get_via_descriptor(ValueType)
def test_1_generic_value__type_of_value_type_get_via_descriptor(): _test_get_via_descriptor(GenericValueType[long])
def test_1_generic_value__type_of_reference_type_get_via_descriptor(): _test_get_via_descriptor(GenericValueType[str])

def test_1_reference_type_get_via_descriptor(): _test_get_via_descriptor(ReferenceType)
def test_1_derived_reference_type_get_via_descriptor(): _test_get_via_descriptor_from_derived_type(DerviedReferenceType)

def test_1_generic_reference_type_of_value_type_get_via_descriptor(): _test_get_via_descriptor(GenericReferenceType[int])
def test_1_generic_reference_type_of_reference_type_get_via_descriptor(): _test_get_via_descriptor(GenericReferenceType[System.Type])

def test_1_derived_generic_reference_type_of_value_type_get_via_descriptor(): _test_get_via_descriptor_from_derived_type(DerivedGenericReferenceType[float])
def test_1_derived_generic_reference_type_of_reference_type_get_via_descriptor(): _test_get_via_descriptor_from_derived_type(DerivedGenericReferenceType[System.Type])
def test_1_derived_generic_reference_type_of_value_type_get2_via_descriptor(): _test_get_via_descriptor_from_derived_type(DerivedGenericReferenceTypeOfValueType)
def test_1_derived_generic_reference_type_of_reference_type_get2_via_descriptor(): _test_get_via_descriptor_from_derived_type(DerivedGenericReferenceTypeOfReferenceType)

def test_descriptor_negative():
    AssertErrorWithMatch(TypeError, "expected ValueType, got type", lambda: ValueType.__dict__['ClassStaticField'].__get__(ValueType, ValueType))
    
    #ReferenceType.__dict__['ClassStaticField'].__get__(DerviedReferenceType(), DerviedReferenceType)
    

def _test_set(this_type):
    o = this_type()

    def f(): o.LiteralIntField = -10
    #AssertErrorWithMatch(ValueError, "specify_here", f)
    def f(): o.LiteralReferenceTypeField = None
    #AssertErrorWithMatch(ValueError, "specify_here", f)
    def f(): o.InitOnlyIntField = -20
    #AssertErrorWithMatch(AttributeError, "Cannot set field InitOnlyIntField on type ", f)
    
    def f(): o.IntStaticField = 10
    #AssertErrorWithMatch(ValueError, "specify_here", f)
    def f(): o.ClassStaticField = ClassWithOneField(20)
    #AssertErrorWithMatch(ValueError, "specify_here", f)
    
    # instance fields
    def f(): o.IntInstanceField = 1
    #AssertErrorWithMatch(ValueError, "Attempt to update field 'IntInstanceField' on value type", f)
    def f(): o.DerivedClassInstanceField = None
    #AssertErrorWithMatch(ValueError, "Attempt to update field 'DerivedClassInstanceField' on value type", f)
    
    def f(): this_type.LiteralIntField = -10
    AssertErrorWithMatch(AttributeError, "Cannot set field LiteralIntField on type", f)
    def f(): this_type.InitOnlyIntField = -20
    AssertErrorWithMatch(AttributeError, "Cannot set field InitOnlyIntField on type ", f)
    def f(): this_type.InitOnlyDateTimeField = System.DateTime()
    AssertErrorWithMatch(AttributeError, "Cannot set field InitOnlyDateTimeField on type ", f)
    def f(): this_type.InitOnlyClassField = None
    AssertErrorWithMatch(AttributeError, "Cannot set field InitOnlyClassField on type ", f)

    this_type.IntStaticField = 20
    AreEqual(o.IntStaticField, 20)
    AreEqual(this_type.IntStaticField, 20)

    this_type.IntStaticField = 33.33 
    AreEqual(o.IntStaticField, 33)
    
    this_type.EnumStaticField = EnumInt32.C
    AreEqual(this_type.EnumStaticField, EnumInt32.C)
    AreEqual(o.EnumStaticField, EnumInt32.C)
    
    def f(): this_type.EnumStaticField = 2
    AssertErrorWithMatch(TypeError, "expected EnumInt32, got int", f)
    
    x = GenericStructWithOneField[int]()
    x.Number = 3000
    this_type.GenericStructStaticField = x
    AreEqual(this_type.GenericStructStaticField, x)
    AreEqual(o.GenericStructStaticField.Number, 3000)
    
    x = DerivedClassWithOneField(499)
    this_type.DerivedClassStaticField = x
    AreEqual(o.DerivedClassStaticField, x)
    AreEqual(this_type.DerivedClassStaticField.Number, 499)
    
    def f(): this_type.IntInstanceField.__set__(o, 100)
    AssertErrorWithMatch(ValueError, "Attempt to update field 'IntInstanceField' on value type", f)

    def f(): this_type.EnumInstanceField.__set__(o, EnumInt32.B)
    AssertErrorWithMatch(ValueError, "Attempt to update field 'EnumInstanceField' on value type", f)

def test_2_value_type_set(): _test_set(ValueType)
def test_2_generic_value__type_of_value_type_set(): _test_set(GenericValueType[int])
def test_2_generic_value__type_of_reference_type_set(): _test_set(GenericValueType[str])
def test_2_reference_type_set(): _test_set(ReferenceType)


def test_enum_type_get_set():
    o = EnumInt32()
    #AreEqual(o.A, EnumInt32.A)
   
    desc = EnumInt32.__dict__['B']
    AreEqual(EnumInt32.B, desc.__get__(o, EnumInt32))
    AreEqual(EnumInt32.B, desc.__get__(None, EnumInt32))
    
    def f(): o.A = 10
    #AssertErrorWithMatch(ValueError, "specify_here", f)
    
    def f(): EnumInt32.B = 10
    AssertErrorWithMatch(AttributeError, "Cannot set field B on type EnumInt32", f)

    def f(): EnumInt32.B = EnumInt32.A
    AssertErrorWithMatch(AttributeError, "Cannot set field B on type EnumInt32", f)
    
    def f(): desc.__set__(o, 12)
    AssertErrorWithMatch(AttributeError, "cannot set", f)

    def f(): desc.__set__(EnumInt32, 12)
    AssertErrorWithMatch(AttributeError, "cannot set", f)

    def f(): desc.__set__(None, EnumInt32.B)
    AssertErrorWithMatch(AttributeError, "attribute 'B' of 'EnumInt32' object is read-only", f)
    
run_test(__name__)    
