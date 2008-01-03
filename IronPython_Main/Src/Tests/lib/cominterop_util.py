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

# COM interop utility module

import sys
import nt

from lib.assert_util  import *
from lib.file_util    import *
from lib.process_util import *

import clr

from System import Type
from System import Activator
from System import Exception as System_dot_Exception

remove_ironpython_dlls(testpath.public_testdir)
load_iron_python_dll()
import IronPython

load_iron_python_test()
import IronPythonTest

#------------------------------------------------------------------------------
#--GLOBALS
    
windir = get_environ_variable("windir")    
preferComDispatch = IronPythonTest.TestHelpers.GetContext().Options.PreferComDispatchOverTypeInfo

agentsvr_path = path_combine(windir, r"msagent\agentsvr.exe")
scriptpw_path = path_combine(windir, r"system32\scriptpw.dll")

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

#------------------------------------------------------------------------------
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

#------------------------------------------------------------------------------    
def getTypeFromProgID(prog_id):
    '''
    Returns the Type object for prog_id.
    '''    
    return Type.GetTypeFromProgID(prog_id)

#------------------------------------------------------------------------------    
def getRCWFromProgID(prog_id):
    '''
    Returns an instance of prog_id.
    '''
    return Activator.CreateInstance(getTypeFromProgID(prog_id))

#------------------------------------------------------------------------------    
def AssertResults(expectedResultWithIDispatch, expectedResultWithInteropAssembly, func, *args):
    if preferComDispatch: expectedResult = expectedResultWithIDispatch
    else: expectedResult = expectedResultWithInteropAssembly
    
    exceptionExpected = False
    try:
        exceptionExpected = (issubclass(expectedResult, Exception) or 
                             issubclass(expectedResult, System_dot_Exception))
    except TypeError: pass
    
    if exceptionExpected:
        try:        result = func(*args)
        except expectedResult: return
        else :      Fail("Expected %r but got no exception (result=%r)" % (expectedResult, result))
    else:
        AreEqual(func(*args), expectedResult)

#------------------------------------------------------------------------------
def genPeverifyInteropAsm(file):
    #if this isn't a test run that will invoke peverify there's no point in
    #continuing
    if not is_peverify_run: 
        return
    else:
        mod_name = file.rsplit("\\", 1)[1].split(".py")[0]
        print "Generating interop assemblies for the", mod_name, "test module which are needed in %TEMP% by peverify..."
        from System.IO import Path
        tempDir = Path.GetTempPath()
        cwd = nt.getcwd()
    
    #maps COM interop test module names to a list of DLLs
    module_dll_dict = {
        "excel" :          [],
        "msagent" :        [agentsvr_path],
        "scriptpw" :       [scriptpw_path],
        "word" :           [],
        "dlrcomserver" :   [testpath.rowan_root + "\\Test\\DlrComLibrary\\Debug\\DlrComLibrary.dll"],
        "paramsinretval" : [testpath.rowan_root + "\\Test\\DlrComLibrary\\Debug\\DlrComLibrary.dll"],
    }
    
    
    if not module_dll_dict.has_key(mod_name):
        print "ERROR: cannot determine which interop assemblies to install!"
        sys.exit(1)
    
    if not file_exists_in_path("tlbimp.exe"):
        print "ERROR: tlbimp.exe is not in the path!"
        sys.exit(1)
    
    try:
        nt.chdir(tempDir)
    
        for com_dll in module_dll_dict[mod_name]:
            if not file_exists(com_dll):
                print "\tERROR: %s does not exist!" % (com_dll)
                continue
    
            print "\trunning tlbimp on", com_dll
            run_tlbimp(com_dll)
        
    finally:
        nt.chdir(cwd)   
        
        
#------------------------------------------------------------------------------
class skip_comdispatch: 
    def __init__(self, msg):
        self.msg = msg
        
    def __call__(self, f):
        if not preferComDispatch:
            return f
        else: 
            from lib.assert_util import _do_nothing
            return _do_nothing('... Decorated with @skip_comdispatch(%s), Skipping %s ...' % (self.msg, f.func_name))
            
#------------------------------------------------------------------------------
def run_com_test(name, file):
    run_test(name)
    
    #Run this test with PreferComDispatch as well
    if not preferComDispatch:
        print "Re-running under '-X:PreferComDispatch' mode."
        AreEqual(launch_ironpython_changing_extensions(file, add=["-X:PreferComDispatch"]), 0)
    
    genPeverifyInteropAsm(file)
    