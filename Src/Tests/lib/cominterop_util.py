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

if is_cli:
    import clr

    from System import Type
    from System import Activator
    from System import Exception as System_dot_Exception

    remove_ironpython_dlls(testpath.public_testdir)
    load_iron_python_dll()
    import IronPython

    load_iron_python_test()
    import IronPythonTest
    
    #--For asserts in IP/DLR assemblies----------------------------------------
    from System.Diagnostics import Debug, DefaultTraceListener
    
    class MyTraceListener(DefaultTraceListener):
        def Fail(self, msg, detailMsg=''):
            print "ASSERT FAILED:", msg
            if detailMsg!='':
                print "              ", detailMsg
            sys.exit(1)
            
    if is_snap:
        Debug.Listeners.Clear()
        Debug.Listeners.Add(MyTraceListener())
    
    
is_pywin32 = False
if sys.platform=="win32":
    try:
        import win32com.client
        is_pywin32 = True
        if sys.prefix not in nt.environ["Path"]:
            nt.environ["Path"] += ";" + sys.prefix
    except:
        pass
    
    

#------------------------------------------------------------------------------
#--GLOBALS
    
windir = get_environ_variable("windir")
if is_cli:
    preferComDispatch = IronPythonTest.TestHelpers.GetContext().Options.PreferComDispatchOverTypeInfo
else:
    preferComDispatch = False

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
def CreateAgentServer():
    if preferComDispatch:
        from System import Type, Activator
        agentServerType = Type.GetTypeFromProgID("Agent.Server")
        return Activator.CreateInstance(agentServerType)
    else:
        if not file_exists(agentsvr_path):
            from sys import exit
            print "Cannot test AgentServerObjects.dll when it doesn't exist."
            exit(0)
        
        if not file_exists_in_path("tlbimp.exe"):
            from sys import exit
            print "tlbimp.exe is not in the path!"
            exit(1)
        else:
            import clr
            run_tlbimp(agentsvr_path)
            Assert(file_exists("AgentServerObjects.dll"))
            clr.AddReference("AgentServerObjects.dll")
            from AgentServerObjects import AgentServerClass
            return AgentServerClass()

#------------------------------------------------------------------------------
def CreateDlrComServer():
    com_type_name = "DlrComLibrary.DlrComServer"
    
    if is_cli:
        com_obj = getRCWFromProgID(com_type_name)
    else:
        com_obj = win32com.client.Dispatch(com_type_name)
        
    return com_obj    

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
def AssertResults(expectedResultWithIDispatch, expectedResultWithInteropAssembly, func, *args, **kwargs):
    if preferComDispatch: expectedResult = expectedResultWithIDispatch
    else: expectedResult = expectedResultWithInteropAssembly
    
    exceptionExpected = False
    try:
        exceptionExpected = (issubclass(expectedResult, Exception) or 
                             issubclass(expectedResult, System_dot_Exception))
    except TypeError: pass
    
    if exceptionExpected:
        try:        result = func(*args, **kwargs)
        except expectedResult: return
        else :      Fail("Expected %r but got no exception (result=%r)" % (expectedResult, result))
    else:
        AreEqual(func(*args, **kwargs), expectedResult)

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
    }
    
    dlrcomlib_list = [  "dlrcomserver", "paramsinretval", "method", "obj", "prop",  ]
    for mod_name in dlrcomlib_list: module_dll_dict[mod_name] = [ testpath.rowan_root + "\\Test\\DlrComLibrary\\Debug\\DlrComLibrary.dll" ]
    
    
    if not file_exists_in_path("tlbimp.exe"):
        print "ERROR: tlbimp.exe is not in the path!"
        sys.exit(1)
    
    try:
        if not module_dll_dict.has_key(mod_name):
            print "WARNING: cannot determine which interop assemblies to install!"
            print "         This may affect peverify runs adversely."
            print
            return
            
        else:
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
    if not preferComDispatch and sys.platform!="win32":
        print "Re-running under '-X:PreferComDispatch' mode."
        AreEqual(launch_ironpython_changing_extensions(file, add=["-X:PreferComDispatch"]), 0)
    
    genPeverifyInteropAsm(file)
    