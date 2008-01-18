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
from System import *
from System.Runtime.InteropServices import COMException, DispatchWrapper
from clr import StrongBox

com_type_name = "DlrComLibrary.OutParams"

#------------------------------------------------------------------------------
# Create a COM object
com_obj = getRCWFromProgID(com_type_name)

#------------------------------------------------------------------------------

simple_primitives_data = [
     ("mByte",       Byte,   Byte.MaxValue),
     ("mChar",       SByte,  SByte.MaxValue),
     ("mUShort",     UInt16, UInt16.MaxValue),
     ("mShort",      Int16,  Int16.MaxValue),
     ("mUlong",      UInt32, UInt32.MaxValue),
     ("mLong",       int,    Int32.MaxValue),
     ("mULongLong",  UInt64, UInt64.MaxValue),
     ("mLongLong",   Int64,  Int64.MaxValue),
     ("mFloat",      Single, Single.MaxValue),
     ("mDouble",     float,  Double.MaxValue)]

def test_primitive_types():
    for test_data in simple_primitives_data:
        testFunctionName = test_data[0]
        testType = test_data[1]
        testValue = test_data[2]

        testFunction = getattr(com_obj, testFunctionName)
        strongBox = StrongBox[testType]()
        testFunction(testValue, strongBox)
        AreEqual(strongBox.Value, testValue)
        
        testFunction(1, strongBox)
        AreEqual(strongBox.Value, 1)
        
        testFunction(0, strongBox)
        AreEqual(strongBox.Value, 0)
    
    strongBoxString = StrongBox[str]()
    com_obj.mBstr("Hello", strongBoxString)
    AreEqual(strongBoxString.Value, "Hello")
    
    strongBoxDispatch = StrongBox[object](DispatchWrapper(None))
    com_obj.mIDispatch(com_obj, strongBoxDispatch)
    AreEqual(strongBoxDispatch.Value, com_obj)


#------------------------------------------------------------------------------
run_com_test(__name__, __file__)
