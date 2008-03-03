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
from lib.cominterop_util import *

if is_cli:
    from System import DateTime, TimeSpan, Reflection, Int32
    from System.Runtime.InteropServices import COMException

###############################################################################
##GLOBALS######################################################################
###############################################################################

com_type_name = "DlrComLibrary.ParamsInRetval"
com_obj = getRCWFromProgID(com_type_name)

STRING_VALUES = [   "", "a", "ab", "abc", "aa",
                    "a" * 100000,
                    "1", "1.0", "1L", "object", "str", "object()",
                    " ", "_", "abc ", " abc", " abc ", "ab c", "ab  c",
                    "\ta", "a\t", "\n", "\t", "\na", "a\n"]
STRING_VALUES = [unicode(x) for x in STRING_VALUES] + STRING_VALUES

def aFunc(): pass

class KNew(object): pass

class KOld: pass

NON_NUMBER_VALUES = [   object, 
                        KNew, KOld, 
                        Exception,
                        #object(), KNew(), KOld(),  #Merlin 324223
                        aFunc, str, eval, type,
                        [], [3.14], ["abc"],
                        (), (3,), (u"xyz",),
                        xrange(5), 
                        {}, {'a':1},
                        #None,  #Merlin 323996
                        __builtins__,
                     ] #+ STRING_VALUES #Merlin 324223

FPN_VALUES = [   -1.23, -1.0, -0.123, -0.0, 0.123, 1.0, 1.23, 
                0.0000001, 3.14159265, 1E10, 1.0E10 ]
UINT_VALUES = [ 0, 1, 2, 7, 10, 32]
INT_VALUES = [ -x for x in UINT_VALUES ] + UINT_VALUES
LONG_VALUES = [long(x) for x in INT_VALUES]
COMPLEX_VALUES = [ 3j]
MERLIN_324223_VALUES = STRING_VALUES + [ object(), KNew(), KOld()]
MERLIN_323996_VALUES = [None]


#--Subclasses of Python/.NET types
class Py_Str(str): pass  

class Py_System_String(System.String): pass

class Py_Float(float): pass  

class Py_Double(float): pass  

class Py_System_Double(System.Double): pass

class Py_UShort(int): pass

class Py_ULong(long): pass

class Py_ULongLong(long): pass

class Py_Short(int): pass

class Py_Long(int): pass

class Py_System_Int32(System.Int32): pass

class Py_LongLong(long): pass


###############################################################################
##HELPERS######################################################################
###############################################################################

def shallow_copy(in_list):
    '''
    We do not necessarily have access to the copy module.
    '''
    return [x for x in in_list]

def pos_num_helper(clr_type):
    return [
            clr_type.MinValue,
            clr_type.MinValue + 1,
            clr_type.MinValue + 2,
            clr_type.MinValue + 10,
            clr_type.MaxValue/2,
            clr_type.MaxValue - 10,
            clr_type.MaxValue - 2,
            clr_type.MaxValue - 1,
            clr_type.MaxValue,
            ]
            
def overflow_num_helper(clr_type):
    return [
            clr_type.MinValue - 1,
            clr_type.MinValue - 2,
            clr_type.MinValue - 3,
            clr_type.MinValue - 10,
            clr_type.MaxValue + 10,
            clr_type.MaxValue + 3,
            clr_type.MaxValue + 2,
            clr_type.MaxValue + 1,
            ]          

def typeErrorTrigger(in_type):
    ret_val = {}
    
    ############################################################
    #Is there anything in Python not being able to evaluate to a bool?
    ret_val["VARIANT_BOOL"] =  [ ]
                     
    ############################################################              
    ret_val["BYTE"] = shallow_copy(NON_NUMBER_VALUES)
    
    if not preferComDispatch:
        ret_val["BYTE"] += MERLIN_324223_VALUES + MERLIN_323996_VALUES
        ret_val["BYTE"] += COMPLEX_VALUES #Merlin 374285
    
    if sys.platform=="win32":
        ret_val["BYTE"] += FPN_VALUES  #Merlin 323751
        ret_val["BYTE"] = [x for x in ret_val["BYTE"] if type(x) not in [unicode, str]] #INCOMPAT BUG - should be ValueError
        ret_val["BYTE"] = [x for x in ret_val["BYTE"] if not isinstance(x, KOld)] #INCOMPAT BUG - should be AttributeError
      
        
    ############################################################
    ret_val["BSTR"] = shallow_copy(NON_NUMBER_VALUES)
    
    if not preferComDispatch:
        ret_val["BSTR"] += MERLIN_324223_VALUES
        ret_val["BSTR"] += FPN_VALUES + UINT_VALUES + INT_VALUES #Merlin 324232
        ret_val["BSTR"] += COMPLEX_VALUES #Merlin 374285
    
    if sys.platform=="win32":
        ret_val["BSTR"] = [] #INCOMPAT BUG
    
    #strip out string values
    ret_val["BSTR"] = [x for x in ret_val["BSTR"] if type(x) is not str]
  
    ############################################################  
    ret_val["CHAR"] =  shallow_copy(NON_NUMBER_VALUES)
    if not preferComDispatch:
        ret_val["CHAR"] += MERLIN_324223_VALUES
        ret_val["CHAR"] += COMPLEX_VALUES #Merlin 374285
        ret_val["CHAR"] += MERLIN_323996_VALUES
        ret_val["CHAR"] += STRING_VALUES #Merlin 324223
    if sys.platform=="win32":
        ret_val["CHAR"] += FPN_VALUES #Merlin 323751
    
    ############################################################
    ret_val["FLOAT"] = shallow_copy(NON_NUMBER_VALUES)
    
    if not preferComDispatch:
        ret_val["FLOAT"] += STRING_VALUES #Merlin 324223
        ret_val["FLOAT"] += MERLIN_324223_VALUES
        ret_val["FLOAT"] += COMPLEX_VALUES #Merlin 374285
        ret_val["FLOAT"] += MERLIN_323996_VALUES
        
    if sys.platform=="win32":
            ret_val["FLOAT"] += UINT_VALUES + INT_VALUES #COMPAT BUG
    
    ############################################################
    ret_val["DOUBLE"] = shallow_copy(ret_val["FLOAT"])
    
    ############################################################            
    ret_val["USHORT"] =  shallow_copy(NON_NUMBER_VALUES)
    
    if not preferComDispatch:
        ret_val["USHORT"] += STRING_VALUES #Merlin 324223
        ret_val["USHORT"] += MERLIN_324223_VALUES
        ret_val["USHORT"] += COMPLEX_VALUES #Merlin 374285
        ret_val["USHORT"] += MERLIN_323996_VALUES
        
    if sys.platform=="win32":
            ret_val["USHORT"] += FPN_VALUES #Merlin 323751
    
    ############################################################  
    ret_val["ULONG"] = shallow_copy(ret_val["USHORT"])
    
    ############################################################           
    ret_val["ULONGLONG"] =  shallow_copy(ret_val["ULONG"])
    
    ############################################################  
    ret_val["SHORT"] =  shallow_copy(NON_NUMBER_VALUES)
    
    if not preferComDispatch:
        ret_val["SHORT"] += STRING_VALUES #Merlin 324223
        ret_val["SHORT"] += MERLIN_324223_VALUES
        ret_val["SHORT"] += COMPLEX_VALUES #Merlin 374285
        ret_val["SHORT"] += MERLIN_323996_VALUES
        
    if sys.platform=="win32":
            ret_val["SHORT"] += FPN_VALUES  #Merlin 323751
    
    ############################################################  
    ret_val["LONG"] =  shallow_copy(ret_val["SHORT"])
    
    ############################################################             
    ret_val["LONGLONG"] =  shallow_copy(ret_val["LONG"])
    
    ############################################################
    return ret_val[in_type]
    
    
def overflowErrorTrigger(in_type):
    ret_val = {}
    
    ############################################################
    ret_val["VARIANT_BOOL"] =  []
                     
    ############################################################              
    ret_val["BYTE"] = []
    ret_val["BYTE"] += overflow_num_helper(System.Byte)
        
    ############################################################
    #Doesn't seem possible to create a value (w/o 1st overflowing
    #in Python) to pass to the COM method which will overflow.
    ret_val["BSTR"] = [] #["0123456789" * 1234567890]
    
    ############################################################ 
    ret_val["CHAR"] = []
    ret_val["CHAR"] +=  overflow_num_helper(System.SByte)
    
    ############################################################
    ret_val["FLOAT"] = []  #Merlin 374289
    
    #Shouldn't be possible to overflow a double.
    ret_val["DOUBLE"] =  []
    
    
    ############################################################            
    ret_val["USHORT"] =  []
    ret_val["USHORT"] += overflow_num_helper(System.UInt16)
      
    ret_val["ULONG"] =  []
    ret_val["ULONG"] +=  overflow_num_helper(System.UInt32)
               
    ret_val["ULONGLONG"] =  []
    ret_val["ULONGLONG"] +=  overflow_num_helper(System.UInt64)
      
    ret_val["SHORT"] =  []
    ret_val["SHORT"] += overflow_num_helper(System.Int16)
      
    ret_val["LONG"] =  []
    ret_val["LONG"] += overflow_num_helper(System.Int32)
                
    ret_val["LONGLONG"] =  []
    ret_val["LONGLONG"] += overflow_num_helper(System.Int64)
    
    ############################################################
    return ret_val[in_type]    
    

def pythonToCOM(in_type):
    '''
    Given a COM type (in string format), this helper function returns a list of
    lists where each sublists contains 1-N elements.  Each of these elements in
    turn are of different types (compatible with in_type), but equivalent to 
    one another.
    '''
    ret_val = {}
    
    ############################################################
    temp_funcs = [int, long, bool, System.Boolean]
    temp_values = [ 0, 1, True, False]
    
    ret_val["VARIANT_BOOL"] =  [ [y(x) for y in temp_funcs] for x in temp_values]
                     
    ############################################################              
    temp_funcs = [System.Byte]
    temp_values = pos_num_helper(System.Byte)
    
    ret_val["BYTE"] =  [ [y(x) for y in temp_funcs] for x in temp_values]

    ############################################################
    temp_funcs = [  str, Py_Str, unicode, Py_System_String, 
                    System.String ]
    temp_values = shallow_copy(STRING_VALUES)
    
    ret_val["BSTR"] = [ [y(x) for y in temp_funcs] for x in temp_values]
  
    ############################################################  
    temp_funcs = [System.SByte]
    temp_values = pos_num_helper(System.SByte)            
    
    ret_val["CHAR"] =  [ [y(x) for y in temp_funcs] for x in temp_values]

    ############################################################
    temp_funcs = [  float, Py_Float, 
                    System.Single]
    ret_val["FLOAT"] = [ [y(x) for y in temp_funcs] for x in FPN_VALUES]
    
    ############################################################
    temp_funcs = [  float, Py_Double, Py_System_Double, System.Double]
    temp_values = [-1.0e+308,  1.0e308] + FPN_VALUES

    ret_val["DOUBLE"] = [ [y(x) for y in temp_funcs] for x in temp_values]
    ret_val["DOUBLE"] += ret_val["FLOAT"]
    
    ############################################################  
    temp_funcs = [int, Py_UShort, System.UInt16]
    temp_values = pos_num_helper(System.UInt16)
    
    ret_val["USHORT"] =  [ [y(x) for y in temp_funcs] for x in temp_values]
    
    ############################################################  
    temp_funcs = [int, Py_ULong, System.UInt32]
    temp_values = pos_num_helper(System.UInt32) + pos_num_helper(System.UInt16)
        
    ret_val["ULONG"] =  [ [y(x) for y in temp_funcs] for x in temp_values]
    ret_val["ULONG"] += ret_val["USHORT"]
    
    ############################################################  
    temp_funcs = [int, long, Py_ULongLong, System.UInt64]
    temp_values = pos_num_helper(System.UInt64) + pos_num_helper(System.UInt32) + pos_num_helper(System.UInt16)
                
    ret_val["ULONGLONG"] =  [ [y(x) for y in temp_funcs] for x in temp_values]
    ret_val["ULONGLONG"] += ret_val["ULONG"]
    
    ############################################################  
    temp_funcs = [int, Py_Short, System.Int16]
    temp_values = pos_num_helper(System.Int16)
                
    ret_val["SHORT"] =  [ [y(x) for y in temp_funcs] for x in temp_values]
    
    ############################################################  
    temp_funcs = [int, Py_Long, System.Int32]
    temp_values = pos_num_helper(System.Int32) + pos_num_helper(System.Int16)
    
    ret_val["LONG"] =  [ [y(x) for y in temp_funcs] for x in temp_values]
    ret_val["LONG"] += ret_val["SHORT"]
    
    
    ############################################################  
    temp_funcs = [int, long, Py_LongLong, System.Int64]
    temp_values = pos_num_helper(System.Int64) + pos_num_helper(System.Int32) + pos_num_helper(System.Int16)
                
    ret_val["LONGLONG"] =  [ [y(x) for y in temp_funcs] for x in temp_values]
    ret_val["LONGLONG"] += ret_val["LONG"]
    
    ############################################################
    return ret_val[in_type]


#------------------------------------------------------------------------------
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
    #AreEqual(com_obj.mCy(System.Double(3.0)), 3.0) #TODO: BUG in test
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
    AssertError(TypeError, com_obj.mIUnknown, None, runonly=preferComDispatch, bugid="323996") # DISP_E_TYPEMISMATCH when using VT_EMPTY
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
        AssertError(TypeError, com_obj.mIUnknown, val, skip=not preferComDispatch,  bugid="374282")


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
    # AreEqual(com_obj.mCy(100), 100)
    now = DateTime.Now
    AreEqual(com_obj.mDate(now).ToOADate(), now.ToOADate())
    
    
    


###############################################################################
##MAIN#########################################################################
###############################################################################
run_com_test(__name__, __file__)