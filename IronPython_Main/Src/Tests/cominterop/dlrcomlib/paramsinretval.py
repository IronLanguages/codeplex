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
from iptest.assert_util import skiptest
skiptest("silverlight")
from iptest.cominterop_util import *

if is_cli:
    from System import DateTime, TimeSpan, Reflection, Int32
    from System.Runtime.InteropServices import COMException

###############################################################################
##GLOBALS######################################################################
###############################################################################

com_type_name = "DlrComLibrary.ParamsInRetval"
com_obj = getRCWFromProgID(com_type_name)


###############################################################################
##HELPERS######################################################################
###############################################################################

def testhelper(function, values, equality_func=AreEqual):
    for i in xrange(len(values)):
        try:
            t_val = function(values[i])
        except Exception, e:
            print "FAILED trying to pass", values[i], "of type", type(values[i]) ,"to", function#.__name__
            raise e
        
        for j in xrange(i, len(values)):
            equality_func(values[i], values[j]) #Make sure no test issues first
        
            try:
                t_val2 = function(values[j])
            except Exception, e:
                print "FAILED trying to pass", values[j], "of type", type(values[j]) ,"to", function#.__name__
                raise e
            
            equality_func(t_val, t_val2)


###############################################################################
##SANITY CHECKS################################################################
###############################################################################
def test_sanity():
    #Integer types
    AreEqual(com_obj.mVariantBool(True), True)
    AreEqual(com_obj.mByte(System.Byte.MinValue), System.Byte.MinValue)
    AreEqual(com_obj.mChar(System.SByte.MinValue), System.SByte.MinValue)
    AreEqual(com_obj.mShort(System.Int16.MinValue), System.Int16.MinValue)
    AreEqual(com_obj.mUShort(System.UInt16.MinValue), System.UInt16.MinValue)
    AreEqual(com_obj.mLong(System.Int32.MinValue), System.Int32.MinValue)
    AreEqual(com_obj.mUlong(System.UInt32.MinValue), System.UInt32.MinValue)
    AreEqual(com_obj.mLongLong(System.Int64.MinValue), System.Int64.MinValue)
    AreEqual(com_obj.mULongLong(System.UInt64.MinValue), System.UInt64.MinValue)
    
    #Float types
    AreEqual(com_obj.mDouble(System.Double(3.14)), 3.14)
    AreEqual(com_obj.mFloat(System.Single(2.0)), 2.0)     
    
    #Complex types
    AreEqual(com_obj.mCy(System.Decimal(3)), 3)
    tempDate = System.DateTime.Now
    AreEqual(str(com_obj.mDate(tempDate)), str(tempDate))
    AreEqual(com_obj.mVariant(System.Single(4.0)), 4.0)   

    #Ole types
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

    
@skip_comdispatch("Merlin 324238")
def test_sanity_int_types_broken():
    AreEqual(com_obj.mScode(System.Int32.MinValue), System.Int32.MinValue)
    
        
###############################################################################
##SIMPLE TYPES#################################################################
###############################################################################
'''
X    [id(32), helpstring("method mVariantBool")] HRESULT mVariantBool([in] VARIANT_BOOL a, [out,retval] VARIANT_BOOL* b);
X    [id(1), helpstring("method mBstr")] HRESULT mBstr([in] BSTR a, [out,retval] BSTR* b);
X    [id(2), helpstring("method mByte")] HRESULT mByte([in] BYTE a, [out,retval] BYTE* b);
X    [id(3), helpstring("method mChar")] HRESULT mChar([in] CHAR a, [out,retval] CHAR* b);
X    [id(6), helpstring("method mDouble")] HRESULT mDouble([in] DOUBLE a, [out,retval] DOUBLE* b);
X    [id(7), helpstring("method mFloat")] HRESULT mFloat([in] FLOAT a, [out,retval] FLOAT* b);

X    [id(30), helpstring("method mUShort")] HRESULT mUShort([in] USHORT a, [out,retval] USHORT* b);
X    [id(28), helpstring("method mUlong")] HRESULT mUlong([in] ULONG a, [out,retval] ULONG* b);
X    [id(29), helpstring("method mULongLong")] HRESULT mULongLong([in] ULONGLONG a, [out,retval] ULONGLONG* b);

X    [id(27), helpstring("method mShort")] HRESULT mShort([in] SHORT a, [out,retval] SHORT* b);
X    [id(12), helpstring("method mLong")] HRESULT mLong([in] LONG a, [out,retval] LONG* b);
X    [id(13), helpstring("method mLongLong")] HRESULT mLongLong([in] LONGLONG a, [out,retval] LONGLONG* b);   
'''

#------------------------------------------------------------------------------
def test_variant_bool():
    for test_list in pythonToCOM("VARIANT_BOOL"):
        testhelper(com_obj.mVariantBool, test_list)

def test_variant_bool_typeerrror():
    for val in typeErrorTrigger("VARIANT_BOOL"):
        AssertError(TypeError, com_obj.mVariantBool, val)
        
def test_variant_bool_overflowerror():
    for val in overflowErrorTrigger("VARIANT_BOOL"):
        AssertError(OverflowError, com_obj.mVariantBool, val)
        
#------------------------------------------------------------------------------
def test_byte():
    for test_list in pythonToCOM("BYTE"):
        testhelper(com_obj.mByte, test_list)

def test_byte_typeerrror():
    for val in typeErrorTrigger("BYTE"):
        AssertError(TypeError, com_obj.mByte, val)
        
def test_byte_overflowerror():
    for val in overflowErrorTrigger("BYTE"):
        AssertError(OverflowError, com_obj.mByte, val)

#------------------------------------------------------------------------------
def test_bstr():
    for test_list in pythonToCOM("BSTR"):
        testhelper(com_obj.mBstr, test_list)

def test_bstr_typeerrror():
    for val in typeErrorTrigger("BSTR"):
        AssertError(TypeError, com_obj.mBstr, val)

def test_bstr_overflowerror():
    for val in overflowErrorTrigger("BSTR"):
        AssertError(OverflowError, com_obj.mBstr, val)

#------------------------------------------------------------------------------
def test_char():
    for test_list in pythonToCOM("CHAR"):
        testhelper(com_obj.mChar, test_list)

def test_char_typeerrror():
    for val in typeErrorTrigger("CHAR"):
        AssertError(TypeError, com_obj.mChar, val)

def test_char_overflowerror():
    for val in overflowErrorTrigger("CHAR"):
        AssertError(OverflowError, com_obj.mChar, val)

#------------------------------------------------------------------------------
def test_float():
    for test_list in pythonToCOM("FLOAT"):
        testhelper(com_obj.mFloat, test_list, equality_func=AlmostEqual)

    #Min/Max float values
    if not preferComDispatch:
        Assert(str(com_obj.mFloat(-3.402823e+039)), "-1.#INF") 
        Assert(str(com_obj.mFloat(3.402823e+039)), "1.#INF")
    AssertError(OverflowError, com_obj.mFloat, 3.402823e+039, runonly=preferComDispatch, bugid="373662")


def test_float_typeerrror():
    for val in typeErrorTrigger("FLOAT"):
        AssertError(TypeError, com_obj.mFloat, val)

def test_float_overflowerror():
    for val in overflowErrorTrigger("FLOAT"):
        AssertError(OverflowError, com_obj.mFloat, val)

#------------------------------------------------------------------------------
def test_double():
    for test_list in pythonToCOM("DOUBLE"):
        testhelper(com_obj.mDouble, test_list, equality_func=AlmostEqual)

    #Min/Max double values
    Assert(str(com_obj.mDouble(-1.797693134864e+309)), "-1.#INF") 
    Assert(str(com_obj.mDouble(1.797693134862313e309)), "1.#INF")

def test_double_typeerrror():
    for val in typeErrorTrigger("DOUBLE"):
        AssertError(TypeError, com_obj.mDouble, val)

def test_double_overflowerror():
    for val in overflowErrorTrigger("DOUBLE"):
        AssertError(OverflowError, com_obj.mDouble, val)

#------------------------------------------------------------------------------
@skip_comdispatch("Merlin 374246")
def test_ushort():
    for test_list in pythonToCOM("USHORT"):
        testhelper(com_obj.mUShort, test_list)

def test_ushort_typeerrror():
    for val in typeErrorTrigger("USHORT"):
        AssertError(TypeError, com_obj.mUShort, val)

def test_ushort_overflowerror():
    for val in overflowErrorTrigger("USHORT"):
        AssertError(OverflowError, com_obj.mUShort, val)

#------------------------------------------------------------------------------
@skip_comdispatch("Merlin 374246")
def test_ulong():
    for test_list in pythonToCOM("ULONG"):
        testhelper(com_obj.mUlong, test_list)

def test_ulong_typeerrror():
    for val in typeErrorTrigger("ULONG"):
        AssertError(TypeError, com_obj.mUlong, val)

def test_ulong_overflowerror():
    for val in overflowErrorTrigger("ULONG"):
        AssertError(OverflowError, com_obj.mUlong, val)

#------------------------------------------------------------------------------
@skip_comdispatch("Merlin 374272")
def test_ulonglong():
    for test_list in pythonToCOM("ULONGLONG"):
        testhelper(com_obj.mULongLong, test_list)

def test_ulonglong_typeerrror():
    for val in typeErrorTrigger("ULONGLONG"):
        AssertError(TypeError, com_obj.mULongLong, val)

def test_ulonglong_overflowerror():
    for val in overflowErrorTrigger("ULONGLONG"):
        AssertError(OverflowError, com_obj.mULongLong, val)

#------------------------------------------------------------------------------
@skip_comdispatch("Merlin 374246")
def test_short():
    for test_list in pythonToCOM("SHORT"):
        testhelper(com_obj.mShort, test_list)

def test_short_typeerrror():
    for val in typeErrorTrigger("SHORT"):
        AssertError(TypeError, com_obj.mShort, val)

def test_short_overflowerror():
    for val in overflowErrorTrigger("SHORT"):
        AssertError(OverflowError, com_obj.mShort, val)

#------------------------------------------------------------------------------
@skip_comdispatch("Merlin 374246")
def test_long():
    for test_list in pythonToCOM("LONG"):
        testhelper(com_obj.mLong, test_list)

def test_long_typeerrror():
    for val in typeErrorTrigger("LONG"):
        AssertError(TypeError, com_obj.mLong, val)

def test_long_overflowerror():
    for val in overflowErrorTrigger("LONG"):
        AssertError(OverflowError, com_obj.mLong, val)

#------------------------------------------------------------------------------
@skip_comdispatch("Merlin 374257")
def test_longlong():
    for test_list in pythonToCOM("LONGLONG"):
        testhelper(com_obj.mLongLong, test_list)

def test_longlong_typeerrror():
    for val in typeErrorTrigger("LONGLONG"):
        AssertError(TypeError, com_obj.mLongLong, val)

def test_longlong_overflowerror():
    for val in overflowErrorTrigger("LONGLONG"):
        AssertError(OverflowError, com_obj.mLongLong, val)


###############################################################################
##INTERFACE TYPES##############################################################
###############################################################################
'''
    [id(8), helpstring("method mIDispatch")] HRESULT mIDispatch([in] IDispatch* a, [out,retval] IDispatch** b);
    [id(9), helpstring("method mIFontDisp")] HRESULT mIFontDisp([in] IFontDisp* a, [out,retval] IDispatch** b);
    [id(10), helpstring("method mIPictureDisp")] HRESULT mIPictureDisp([in] IPictureDisp* a, [out,retval] IDispatch** b);
    [id(11), helpstring("method mIUnknown")] HRESULT mIUnknown([in] IUnknown* a, [out,retval] IUnknown** b);
'''
    
def test_interface_types():   
    '''
    TODO:
    - mIFontDisp 
    - mIPictureDisp
    '''
    AreEqual(com_obj.mIDispatch(com_obj), com_obj)
    AreEqual(com_obj.mIUnknown(com_obj), com_obj)
    
    #Merlin 323996
    AssertError(ValueError, com_obj.mIUnknown, None, runonly=preferComDispatch, bugid="323996") # DISP_E_TYPEMISMATCH when using VT_EMPTY
    if not preferComDispatch:
        AreEqual(None, com_obj.mIUnknown(None)) # DISP_E_TYPEMISMATCH when using VT_EMPTY

def test_interface_types_typerror():
    '''
    TODO:
    - mIFontDisp 
    - mIPictureDisp
    '''
    test_cases = shallow_copy(NON_NUMBER_VALUES)
    
    for val in test_cases:
        AssertError(TypeError, com_obj.mIDispatch, val)
        AssertError(TypeError, com_obj.mIUnknown, val)


###############################################################################
##COMPLEX TYPES################################################################
###############################################################################
'''
    [id(31), helpstring("method mVariant")] HRESULT mVariant([in] VARIANT a, [out,retval] VARIANT* b);
    [id(4), helpstring("method mCy")] HRESULT mCy([in] CY a, CY* b);
    [id(5), helpstring("method mDate")] HRESULT mDate([in] DATE a, [out,retval] DATE* b);
    [id(26), helpstring("method mScode")] HRESULT mScode([in] SCODE a, [out,retval] SCODE* b);
'''    


###############################################################################
##OLE TYPES####################################################################
###############################################################################
'''
    [id(14), helpstring("method mOleColor")] HRESULT mOleColor([in] OLE_COLOR a, [out,retval] OLE_COLOR* b);
    [id(15), helpstring("method mOleXposHimetric")] HRESULT mOleXposHimetric([in] OLE_XPOS_HIMETRIC a, [out,retval] OLE_XPOS_HIMETRIC* b);
    [id(16), helpstring("method mOleYposHimetric")] HRESULT mOleYposHimetric([in] OLE_YPOS_HIMETRIC a, [out,retval] OLE_YPOS_HIMETRIC* b);
    [id(17), helpstring("method mOleXsizeHimetric")] HRESULT mOleXsizeHimetric([in] OLE_XSIZE_HIMETRIC a, [out,retval] OLE_XSIZE_HIMETRIC* b);
    [id(18), helpstring("method mOleYsizeHimetric")] HRESULT mOleYsizeHimetric([in] OLE_YSIZE_HIMETRIC a, [out,retval] OLE_YSIZE_HIMETRIC* b);
    [id(19), helpstring("method mOleXposPixels")] HRESULT mOleXposPixels([in] OLE_XPOS_PIXELS a, [out,retval] OLE_XPOS_PIXELS* b);
    [id(20), helpstring("method mOleYposPixels")] HRESULT mOleYposPixels([in] OLE_YPOS_PIXELS a, [out,retval] OLE_YPOS_PIXELS* b);
    [id(21), helpstring("method mOleXsizePixels")] HRESULT mOleXsizePixels([in] OLE_XSIZE_PIXELS a, [out,retval] OLE_XSIZE_PIXELS* b);
    [id(22), helpstring("method mOleYsizePixels")] HRESULT mOleYsizePixels([in] OLE_YSIZE_PIXELS a, [out,retval] OLE_YSIZE_PIXELS* b);
    [id(23), helpstring("method mOleHandle")] HRESULT mOleHandle([in] OLE_HANDLE a, [out,retval] OLE_HANDLE* b);
    [id(24), helpstring("method mOleOptExclusive")] HRESULT mOleOptExclusive([in] OLE_OPTEXCLUSIVE a, [out,retval] OLE_OPTEXCLUSIVE* b);
    [id(25), helpstring("method mOleTristate")] HRESULT mOleTristate([in] enum OLE_TRISTATE a, [out,retval] enum OLE_TRISTATE* b);
'''    


###############################################################################
##MISC#########################################################################
###############################################################################
def test_misc():
    AreEqual(com_obj.mCy(100), 100)
    now = DateTime.Now
    AreEqual(com_obj.mDate(now).ToOADate(), now.ToOADate())
    
    
    


###############################################################################
##MAIN#########################################################################
###############################################################################
run_com_test(__name__, __file__)
