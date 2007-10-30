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

# COM interop utility module

from lib.assert_util  import *
from lib.file_util    import *
from lib.process_util import *

import clr

remove_ironpython_dlls(testpath.public_testdir)
load_iron_python_dll()
import IronPython

load_iron_python_test()
import IronPythonTest
    
windir = get_environ_variable("windir")    
pe = IronPython.Hosting.PythonEngine.CurrentEngine
preferComDispatch = pe.Options.PreferComDispatchOverTypeInfo


#------------------------------------------------------------------------------
#--HELPERS

def TryLoadExcelInteropAssembly():
    try:
        clr.AddReferenceByName('Microsoft.Office.Interop.Excel, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c')
    except:
        try:
            clr.AddReferenceByName('Microsoft.Office.Interop.Excel, Version=11.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c')
        except:
            pass


def IsExcelInstalled():
    from Microsoft.Win32 import Registry
    from System.IO import File

    excel = None
    
    #Office 11 or 12 are both OK for this test. Office 12 is preferred.
    excel = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office\\12.0\\Excel\\InstallRoot")
    if excel==None:
        excel = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office\\11.0\\Excel\\InstallRoot")
    
    #sanity check
    if excel==None:
        return False
    
    #make sure it's really installed on disk
    excel_path = excel.GetValue("Path") + "excel.exe"
    return File.Exists(excel_path)
    
    
def run_com_test(name, file):
    run_test(name)
    
    #Run this test with PreferComDispatch as well
    if not preferComDispatch:
        print "Re-running under '-X:PreferComDispatch' mode."
        AreEqual(launch_ironpython_changing_extensions(file, add=["-X:PreferComDispatch"]), 0)