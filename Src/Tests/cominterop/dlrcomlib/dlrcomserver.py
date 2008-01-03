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
from System.Runtime.InteropServices import COMException
from System import InvalidOperationException

com_type_name = "DlrComLibrary.DlrComServer"

#------------------------------------------------------------------------------
# Create a COM object
com_obj = getRCWFromProgID(com_type_name)

def test_perfScenarios():
    AreEqual(com_obj.SimpleMethod(), None)
    AreEqual(com_obj.IntArguments(1, 2), None)
    AreEqual(com_obj.StringArguments("hello", "there"), None)
    AreEqual(com_obj.ObjectArguments(com_obj, com_obj), None)

def test_errorInfo():
    try:
        com_obj.TestErrorInfo()
    except COMException, e:
        AreEqual("Test error message" in str(e), True)

def test_documentation():
    import IronPython
    ops = IronPython.Hosting.PythonEngine.CurrentEngine.Operations
    AssertResults("void IntArguments(Int32 arg1, Int32 arg2)",
                  InvalidOperationException, # Not implemented yet
                  ops.GetDocumentation, com_obj.IntArguments)

#------------------------------------------------------------------------------
run_com_test(__name__, __file__)