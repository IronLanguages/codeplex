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

# COM Interop tests for IronPython
from lib.assert_util import skiptest
skiptest("silverlight")
from System import DateTime
from clr import StrongBox
from lib.cominterop_util import *

com_type_name = "DlrComLibrary.Properties"
com_obj = getRCWFromProgID(com_type_name)

test_sanity_types_data = [
    ("pBstr", "abcd"),
    ("pVariant", 42),
    ("pVariant", "42"),
    ("pLong", 12345),
    ]    
#Verify that types are marshalled properly for properties. This is just a sanity check
#since parmainretval covers marshalling extensively.
def test_sanity_types():
    for propName, val in test_sanity_types_data:
        setattr(com_obj, propName, val)
        AreEqual(getattr(com_obj, propName), val)
    now = DateTime.Now
    com_obj.pDate = now
    AreEqual(str(com_obj.pDate), str(now))        

#Verify properties with propputref act as expected.
@skip("multiple_execute")
def test_ref_properties():    
    com_obj.RefProperty = com_obj
    AreEqual(com_obj.RefProperty, com_obj)
    AreEqual(com_obj.RefProperty, com_obj)
    AreEqual(com_obj.RefProperty, com_obj)
    #Merlin 380775 - this is a weird bug. It manifests only if the RefProperty is accessed multiple times
    #and corrupts com_obj rendering it useless after that. Hence the following line is commented.
    #AreEqual(com_obj.RefProperty, com_obj)
    
    if preferComDispatch: #Merlin XXXXXX
        com_obj.PutAndPutRefProperty = 2.0
        AreEqual(com_obj.PutAndPutRefProperty, 4.0) #The set_ multiples the value by 2 but the let_ does not in the com object.
    if not preferComDispatch: #Merlin 380784
        com_obj.let_PutAndPutRefProperty(2.0)
        AreEqual(com_obj.PutAndPutRefProperty, 2.0)

#Verify that readonly and writeonly properties work as expected.
def test_restricted_properties():
    c = com_obj.ReadOnlyProperty
    AssertError(AttributeError, setattr, com_obj, "ReadOnlyProperty", "a", skip=preferComDispatch, bugid="380806")
    AssertError(StandardError, setattr, com_obj, "ReadOnlyProperty", "a", runonly=preferComDispatch, bugid="380806")
	
    com_obj.WriteOnlyProperty = DateTime.Now
    AssertError(AttributeError, getattr, com_obj, "WriteOnlyProperty", skip=preferComDispatch, bugid="380813")

#Validate behaviour of properties which take in parameters.
@skip_comdispatch("Merlin 380822")
def test_properties_param():
    com_obj.PropertyWithParam[20] = 42
    AreEqual(com_obj.PropertyWithParam[0], 62)
    AreEqual(com_obj.PropertyWithParam[20], 42)
    
    strongVar = StrongBox[str]("a")
    com_obj.PropertyWithOutParam[strongVar] = "abcd"
    AreEqual(com_obj.PropertyWithOutParam[strongVar], "abcd")
    AreEqual(strongVar.Value, "abcd")
    
    com_obj.PropertyWithTwoParams[2, 2] = 2
    AreEqual(com_obj.PropertyWithTwoParams[0, 0], 6)
    AreEqual(com_obj.PropertyWithTwoParams[2,2], 2)
  
@disabled("Merlin 381252")
#Validate that one is able to call default properties with indexers.
def test_default_property():
    com_obj[23] = True
    AreEqual(com_obj[23], True)

#Call the get_ and set_ methods of the properties.
@skip_comdispatch("Merlin 381591")
def test_propeties_as_methods():
    for propName, val in test_sanity_types_data:
        setterFunc = getattr(com_obj, "set_" + propName)
        getterFunc = getattr(com_obj, "get_" + propName)
        setterFunc(val)
        AreEqual(getterFunc(), val)
    
    AssertError(AttributeError, getattr, com_obj, "set_ReadOnlyProperty")
    AssertError(AttributeError, getattr, com_obj, "get_WriteOnlyProperty")
    
    com_obj.set_PropertyWithParam(20, 42)
    AreEqual(com_obj.get_PropertyWithParam(0), 62)
        
#------------------------------------------------------------------------------------
run_com_test(__name__, __file__)
#------------------------------------------------------------------------------------