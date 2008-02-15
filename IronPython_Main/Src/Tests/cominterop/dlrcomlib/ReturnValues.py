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
skiptest("win32", "silverlight")
from lib.cominterop_util import *
from System import NullReferenceException
from System.Runtime.InteropServices import COMException

com_type_name = "DlrComLibrary.ReturnValues"

#------------------------------------------------------------------------------
# Create a COM object
com_obj = getRCWFromProgID(com_type_name)

#------------------------------------------------------------------------------

#Test making calls to COM methods which return non-HRESULT values
def test_nonHRESULT_retvals():
    com_obj.mNoRetVal()  #void
    AreEqual(com_obj.mIntRetVal(), 42) #int
    #The method with two return values is a signature that tlbimp cant handle so it skips it.
    try:
        AreEqual(com_obj.mTwoRetVals(), 42) #Todo: What should be the expected behaviour for the IDispatch mode - 42 or [3,42]
    except AttributeError:
        pass    
    
#Test making calls to COM methods which return error values of HRESULT.
@skip_comdispatch("Merlin 378174")
def test_HRESULT_Error():    
    AssertError(NullReferenceException, com_obj.mNullRefException)    
    AssertError(COMException, com_obj.mGenericCOMException)
    
#------------------------------------------------------------------------------
run_com_test(__name__, __file__)
