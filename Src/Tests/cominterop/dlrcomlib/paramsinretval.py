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
skiptest("win32", "silverlight", "cli64")
from lib.cominterop_util import *
from System import DateTime, TimeSpan, Reflection, Int32
from System.Runtime.InteropServices import COMException

com_type_name = "DlrComLibrary.ParamsInRetval"

#------------------------------------------------------------------------------
# Create a COM object
com_obj = getRCWFromProgID(com_type_name)

#------------------------------------------------------------------------------

#**kwargs is broken under -X:PreferComDispatch
print "Merlin Work Item 324009"
def AssertError(exc, func, *args):
    try:        func(*args)
    except exc: return
    else :      Fail("Expected %r but got no exception" % exc)

def pythonToCOM(in_type):
    '''
    Given a COM type (in string format), this helper function returns a list of
    lists where each sublists contains 1-N elements.  Each of these elements in
    turn are of different types (compatible with in_type), but equivalent to 
    one another.
    '''
    ############################################################
    temp_funcs = [int, long, bool, System.Boolean]
    temp_values = [ 0, 1, True, False]
    VARIANT_BOOL =  [ [y(x) for y in temp_funcs] for x in temp_values]
                     
    ############################################################              
    temp_funcs = [System.Byte]
    temp_values = [ System.Byte.MinValue,
                    System.Byte.MinValue + 1,
                    System.Byte.MinValue + 2,
                    System.Byte.MaxValue/2,
                    System.Byte.MaxValue - 2,
                    System.Byte.MaxValue - 1,
                    System.Byte.MaxValue,
                    ]
                
    BYTE =  [ [y(x) for y in temp_funcs] for x in temp_values]

    ############################################################
    class Py_Str(str): pass  
    class Py_System_String(System.String): pass      
    
    temp_funcs = [str, System.String, Py_Str, Py_System_String]
    temp_values = ["", " ", "_", "a", "ab", "abc", "abc ", " abc", " abc ", "ab c", "ab  c"]
    
    BSTR = [ [y(x) for y in temp_funcs] for x in temp_values]
  
    ############################################################  
    temp_funcs = [System.SByte]
    temp_values = [ System.SByte.MinValue,
                    System.SByte.MinValue + 1,
                    System.SByte.MinValue + 2,
                    System.SByte.MaxValue/2,
                    System.SByte.MaxValue - 2,
                    System.SByte.MaxValue - 1,
                    System.SByte.MaxValue,
                    ]
                
    CHAR =  [ [y(x) for y in temp_funcs] for x in temp_values]
    
    ############################################################
    interesting_values = {
        "VARIANT_BOOL" : VARIANT_BOOL,
        "BYTE" : BYTE,
        "BSTR" : BSTR,
        "CHAR" : CHAR,
    }

    return interesting_values[in_type]


#------------------------------------------------------------------------------
def testhelper(function, values):
    for i in xrange(len(values)):
        try:
            t_val = function(values[i])
        except Exception, e:
            print "FAILED trying to pass", values[i], "of type", type(values[i]) ,"to", function.__name__
            raise e
        
        for j in xrange(i, len(values)):
            AreEqual(values[i], values[j]) #Make sure no test issues first
        
            try:
                t_val2 = function(values[j])
            except Exception, e:
                print "FAILED trying to pass", values[j], "of type", type(values[j]) ,"to", function.__name__
                raise e
            
            AreEqual(t_val, t_val2)



#------------------------------------------------------------------------------
#--SANITY TESTS----------------------------------------------------------------
def test_sanity_int_types():
    AreEqual(com_obj.mVariantBool(True), True)
    AreEqual(com_obj.mByte(System.Byte.MinValue), System.Byte.MinValue)
    AreEqual(com_obj.mChar(System.SByte.MinValue), System.SByte.MinValue)
    AreEqual(com_obj.mShort(System.Int16.MinValue), System.Int16.MinValue)
    AreEqual(com_obj.mUShort(System.UInt16.MinValue), System.UInt16.MinValue)
    AreEqual(com_obj.mLong(System.Int32.MinValue), System.Int32.MinValue)
    AreEqual(com_obj.mUlong(System.UInt32.MinValue), System.UInt32.MinValue)
    AreEqual(com_obj.mLongLong(System.Int64.MinValue), System.Int64.MinValue)
    AreEqual(com_obj.mULongLong(System.UInt64.MinValue), System.UInt64.MinValue)
    
    
@skip_comdispatch("Merlin 324238")
def test_sanity_int_types_broken():
    AreEqual(com_obj.mScode(System.Int32.MinValue), System.Int32.MinValue)
    
    
def test_sanity_float_types():
    AreEqual(com_obj.mDouble(System.Double(3.14)), 3.14)
    AreEqual(com_obj.mFloat(System.Single(2.0)), 2.0)     

def test_sanity_complex_types():
    #AreEqual(com_obj.mCy(System.Double(3.0)), 3.0) #TODO: BUG in test
    tempDate = System.DateTime.Now
    AreEqual(str(com_obj.mDate(tempDate)), str(tempDate))
    AreEqual(com_obj.mVariant(System.Single(4.0)), 4.0)   
    
def test_sanity_ole():
    '''
    A few of these are enums and others are likely typedefs...
    '''
    #mOleTristate
    #mOleColor
    #mOleXposHimetric
    #mOleYposHimetric
    #mOleXsizeHimetric
    #mOleYsizeHimetric
    #mOleXposPixels
    #mOleYposPixels
    #mOleXsizePixels
    #mOleYsizePixels
    #mOleHandle
    #mOleOptExclusive
    pass
    
    
def test_sanity_interface_types():   
    '''
    TODO:
    - mIFontDisp 
    - mIPictureDisp
    ''' 
    #Multiple calls to this breaks ipy.exe
    AreEqual(com_obj.mIDispatch(com_obj), com_obj)
    AreEqual(com_obj.mIUnknown(com_obj), com_obj)
    
    
#------------------------------------------------------------------------------
#--INDIVIDUAL PARAMSINRETVAL METHODS-------------------------------------------
def test_variant_bool():
    for test_list in pythonToCOM("VARIANT_BOOL"):
        testhelper(com_obj.mVariantBool, test_list)


def test_variant_bool_neg():
    '''
    TODO:    
    Are there really any Python types, objects, etc that do not evaluate
    to True or False?
    '''
    test_list = []
    for bad in test_list:
        AssertError(COMException, com_obj.mVariantBool, bad)

#------------------------------------------------------------------------------
def test_byte():
    for test_list in pythonToCOM("BYTE"):
        testhelper(com_obj.mByte, test_list)
        
        
@disabled("Merlin 323751")        
def test_byte_neg_general():
    #Should not try to convert a float to a byte...
    AssertError(TypeError, com_obj.mByte, 3.14)     
        

@skip_comdispatch("Merlin 324216")
def test_byte_neg_overflow():
    test_list = [   System.Byte.MinValue - 1,
                    System.Byte.MinValue - 2,
                    System.Byte.MinValue - 3,
                    System.Byte.MaxValue + 1,
                    System.Byte.MaxValue + 2,
                    System.Byte.MaxValue + 3,
                    1000,
                    1000L,
                ]
    for bad in test_list:
        AssertError(OverflowError, com_obj.mByte, bad) 
        

@skip_comdispatch("Merlin 324223")        
def test_byte_neg_typeerror():
    test_list = [   "some string",
                    u"some unicode string",
                    None,
                    object,
                    str,
                ]
    for bad in test_list:
        AssertError(TypeError, com_obj.mByte, bad) 
    
    
#------------------------------------------------------------------------------
def test_bstr():
    for test_list in pythonToCOM("BSTR"):
        testhelper(com_obj.mBstr, test_list)

    
@skip_comdispatch("Merlin 323996")
def test_bstr_none():
    AreEqual(com_obj.mBstr(None), None) 


@skip_comdispatch("Merlin 324232")
def test_bstr_neg_typeerror(): 
    test_list = [   0, 0L, 0.0,
                    1, 1L, 1.1,
                    object,
                    str,
                ]
    for bad in test_list:
        AssertError(TypeError, com_obj.mBstr, bad) 


#------------------------------------------------------------------------------
def test_char():
    for test_list in pythonToCOM("CHAR"):
        testhelper(com_obj.mChar, test_list)


@disabled("Merlin 323751")  
def test_char_neg_general():
    #Should not try to convert a float to a byte...
    AssertError(TypeError, com_obj.mChar, 3.14)     


@skip_comdispatch("Merlin 324216")        
def test_char_neg_overflow():
    test_list = [   System.SByte.MinValue - 1,
                    System.SByte.MinValue - 2,
                    System.SByte.MinValue - 3,
                    System.SByte.MaxValue + 1,
                    System.SByte.MaxValue + 2,
                    System.SByte.MaxValue + 3,
                    1000,
                    1000L,
                ]
    for bad in test_list:
        AssertError(OverflowError, com_obj.mChar, bad) 


@skip_comdispatch("Merlin 324223")        
def test_char_neg_typeerror(): 
    test_list = [   "some string",
                    u"some unicode string",
                    None,
                    object,
                    str,
                ]
    for bad in test_list:
        AssertError(TypeError, com_obj.mChar, bad) 


#------------------------------------------------------------------------------
#------------------------------------------------------------------------------
def test_primitiveTypes():
    AreEqual(com_obj.mBstr("Hello"), "Hello")
    
    AreEqual(com_obj.mByte(100), 100)
    AssertResults(COMException, OverflowError, com_obj.mByte, Int32.MaxValue) # DISP_E_OVERFLOW with IDispatch
    
    AreEqual(com_obj.mChar(100), 100)
    # AreEqual(com_obj.mCy(100), 100)
    now = DateTime.Now
    AreEqual(com_obj.mDate(now).ToOADate(), now.ToOADate())
    AreEqual(com_obj.mLong(Int32.MaxValue), Int32.MaxValue)
    AreEqual(com_obj.mIUnknown(com_obj), com_obj)
    AssertResults(COMException, None, com_obj.mIUnknown, None) # DISP_E_TYPEMISMATCH when using VT_EMPTY

#------------------------------------------------------------------------------
class MyInt(int): pass

def test_implicitConversionsToPrimitiveTypes():
    AreEqual(com_obj.mLong(1L), 1)
    # IronPython has conversions from ExtensibleInt to Int64, Double, and Decimal
    AssertResults(Reflection.AmbiguousMatchException, 1, com_obj.mLong, MyInt(1))

#------------------------------------------------------------------------------
run_com_test(__name__, __file__)