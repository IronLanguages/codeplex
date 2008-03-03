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
#--Override a couple of definitions from assert_util
from lib import assert_util
DEBUG = 1

def assert_helper(in_dict):    
    #add the keys if they're not there
    if not in_dict.has_key("runonly"): in_dict["runonly"] = True
    if not in_dict.has_key("skip"): in_dict["skip"] = False
    
    #determine whether this test will be run or not
    run = in_dict["runonly"] and not in_dict["skip"]
    
    #strip out the keys
    for x in ["runonly", "skip"]: in_dict.pop(x)
    
    if not run:
        if in_dict.has_key("bugid"):
            print "...skipped an assert due to bug", str(in_dict["bugid"])
            
        elif DEBUG:
            print "...skipped an assert on", sys.platform
    
    if in_dict.has_key("bugid"): in_dict.pop("bugid")
    return run

def Assert(*args, **kwargs):
    if assert_helper(kwargs): assert_util.Assert(*args, **kwargs)
    
def AreEqual(*args, **kwargs):
    if assert_helper(kwargs): assert_util.AreEqual(*args, **kwargs)

def AssertError(*args, **kwargs):
    try:
        if assert_helper(kwargs): assert_util.AssertError(*args, **kwargs)
    except Exception, e:
        print "AssertError(" + str(args) + ", " + str(kwargs) + ") failed!"
        raise e

def AssertErrorWithMessage(*args, **kwargs):
    try:
        if assert_helper(kwargs): assert_util.AssertErrorWithMessage(*args, **kwargs)
    except Exception, e:
        print "AssertErrorWithMessage(" + str(args) + ", " + str(kwargs) + ") failed!"
        raise e

def AssertErrorWithPartialMessage(*args, **kwargs):
    try:
        if assert_helper(kwargs): assert_util.AssertErrorWithPartialMessage(*args, **kwargs)
    except Exception, e:
        print "AssertErrorWithPartialMessage(" + str(args) + ", " + str(kwargs) + ") failed!"
        raise e

def AlmostEqual(*args, **kwargs):
    if assert_helper(kwargs): assert_util.AlmostEqual(*args, **kwargs)


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
    if is_cli:
        return Activator.CreateInstance(getTypeFromProgID(prog_id))
    else:
        return win32com.client.Dispatch(prog_id)

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
#--Fake parts of System for compat tests
if sys.platform=="win32":
    class System:
        class Byte(int):
            MinValue = 0
            MaxValue = 255
        class SByte(int):
            MinValue = -128
            MaxValue = 127
        class Int16(int):
            MinValue = -32768
            MaxValue = 32767
        class UInt16(int):
            MinValue = 0
            MaxValue = 65535
        class Int32(int):
            MinValue = -2147483648
            MaxValue =  2147483647
        class UInt32(long):
            MinValue = 0
            MaxValue = 4294967295
        class Int64(long):
            MinValue = -9223372036854775808L
            MaxValue =  9223372036854775807L
        class UInt64(long):
            MinValue = 0L 
            MaxValue = 18446744073709551615
        class Single(float):
            MinValue = -3.40282e+038
            MaxValue =  3.40282e+038
        class Double(float):
            MinValue = -1.79769313486e+308
            MaxValue =  1.79769313486e+308
        class String(str):
            pass
        class Boolean(int):
            pass
            
#------------------------------------------------------------------------------
def run_pkg_helper(filename, exclude_list = []):
    from exceptions import SystemExit

    #Determine the package structure
    import nt
    cwd = nt.getcwd()
    
    common_end = 0
    for i in xrange(len(cwd)):
        if cwd[i]!=filename[i]: break
        common_end+=1
    
    common_end+=1
    temp_package = filename[common_end:filename.rfind("\\")+1]
    temp_package.replace("\\", ".")
    
    #get the module names in the package
    mod_names = [temp_package + x for x in get_mod_names(filename) if x not in exclude_list]
    
    for test_module in mod_names:
        print "--------------------------------------------------------------------"
        print "Importing", test_module, "..."
        try:
            __import__(test_module)
        except SystemExit, e:
            if e.code!=0: 
                raise Exception("Importing '%s' caused an unexpected exit code: %s" % (test_module, str(e.code)))
        print ""

#------------------------------------------------------------------------------
RERUN_UNDER_PREFERCOMDISPATCH = ["cominterop\\apps\\msagent.py"]

def run_com_test(name, file):
    run_test(name)
    
    #Run this test with PreferComDispatch as well
    if not preferComDispatch and sys.platform!="win32" and file.lower() in RERUN_UNDER_PREFERCOMDISPATCH:
        print
        print "#" * 80
        print "Re-running %s under '-X:PreferComDispatch' mode." % (file)
        AreEqual(launch_ironpython_changing_extensions(file, add=["-X:PreferComDispatch"]), 0)
    
    genPeverifyInteropAsm(file)
    