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

# COM Interop tests for IronPython



from lib.assert_util import *
skiptest("silverlight")
from lib.file_util import *
from lib.process_util import *

import nt
import clr
import sys

from System import Type, Activator, Environment, IntPtr, Array, Object, Int32
from System.Reflection import BindingFlags

if not is_silverlight:
    remove_ironpython_dlls(testpath.public_testdir)
    load_iron_python_dll()

import IronPython
pe = IronPython.Hosting.PythonEngine.CurrentEngine
preferComDispatch = pe.Options.PreferComDispatchOverTypeInfo

windir = get_environ_variable("windir")
scriptpw_path = path_combine(windir, r"system32\scriptpw.dll")
agentsvr_path = path_combine(windir, r"msagent\agentsvr.exe")



# TEST MATRIX:
#
#                    (PIA)                  (No PIA)
# (Registered)
# (Not Registered)   excel/merlin

def _test_common_on_object(o):
    for x in ['GetHashCode', 'GetPassword', '__repr__', 'ToString']:
        Assert(hasattr(o, x), x + " not in dir(o)")

    for x in ['__doc__', '__init__', '__module__']:
        AreEqual(dir(o).count(x), 1)
    
    Assert(o.GetHashCode()) # not zero
    try: del o.GetHashCode
    except AttributeError: pass
    else: Fail("attribute 'GetHashCode' of 'xxx' object is read-only")

    try: o[3] = "something"
    except AttributeError: pass
    else: Fail("__setitem__")
    try: something = o[3]
    except AttributeError: pass
    else: Fail("__getitem__")

    AssertError(TypeError, (lambda:o+3))
    AssertError(TypeError, (lambda:o-3))
    AssertError(TypeError, (lambda:o*3))
    AssertError(TypeError, (lambda:o/3))
    AssertError(TypeError, (lambda:o >> 3))
    AssertError(TypeError, (lambda:o << 3))

if file_exists(scriptpw_path) and file_exists_in_path("tlbimp.exe"):
    #BUG: http://tkbgitvstfat01:8080/WorkItemTracking/WorkItem.aspx?artifactMoniker=177188
    @skip("orcas")
    def test__1_registered_nopia():
        # Check to see that namespace 'spwLib' isn't accessible
        Assert('spwLib' not in dir(), "spwLib is already registered")
    
        run_register_com_component(scriptpw_path)
        
        pwcType = Type.GetTypeFromProgID('ScriptPW.Password.1')
        
        pwcInst = Activator.CreateInstance(pwcType)
    
        AreEqual('System.__ComObject', pwcInst.ToString())
        
        try: del pwcInst.GetPassword
        except AttributeError: pass
        else: Fail("'__ComObject' object has no attribute 'GetPassword'")
        
        _test_common_on_object(pwcInst)
        
        if preferComDispatch:
            # looks like: "<System.__ComObject  (TypeInfo : IPassword)>"
            types = ['__ComObject', 'IPassword']
        else:
            # looks like: "<System.__ComObject  (TypeInfo : Password)>"
            types = ['__ComObject', 'Password']

        for x in types:
            Assert(x in repr(pwcInst), x + " not in repr(pwcInst)")
    
    #Merlin Work Item #203712
    @skip("cli")
    def test__3_registered_with_pia():
        run_tlbimp(scriptpw_path, "spwLib")
        run_register_com_component(scriptpw_path)
        clr.AddReference("spwLib.dll")
    
        from spwLib import PasswordClass    
        pc = PasswordClass()
        
        Assert('PasswordClass' in repr(pc))
        Assert('spwLib.PasswordClass' in pc.ToString())
        AreEqual(pc.__class__, PasswordClass)
        
        try: del pc.GetPassword
        except AttributeError: pass
        else: Fail("attribute 'GetPassword' of 'PasswordClass' object is read-only")
        
        _test_common_on_object(pc)
        
    def test__2_unregistered_nopia():
        # Check to see that namespace 'spwLib' isn't accessible
        Assert('spwLib' not in dir(), "spwLib is already registered")
    
        run_unregister_com_component(scriptpw_path)
        pwcType = Type.GetTypeFromProgID('ScriptPW.Password.1')
        AreEqual(pwcType, None)
        
        # Registration-free COM activation
        load_iron_python_test()
        import IronPythonTest
        password = IronPythonTest.ScriptPW.CreatePassword()
        AreEqual('System.__ComObject', password.ToString())
else: 
    print "warning:", scriptpw_path, "or tlbimp not found"
    
if file_exists(agentsvr_path) and file_exists_in_path("tlbimp.exe"):
    def test_merlin():
        run_tlbimp(agentsvr_path)
        Assert(file_exists("AgentServerObjects.dll"))
        
        import clr
        from time import sleep
        clr.AddReference("AgentServerObjects.dll")

        from AgentServerObjects import * 
        a = AgentServerClass()    
        Assert('Equals' in dir(a))
        cid = a.Load('Merlin.acs')[0]

        c = a.GetCharacter(cid)
        sleep(1)
        c.Show(0)
        sleep(1)
        while c.GetVisible()==0:
            sleep(1)
        c.Think('IronPython...')
        c.Play('Read')
        c.GestureAt(True, False)
        c.GestureAt(100, 200)
        if not preferComDispatch:
            AssertError(TypeError, c.GestureAt, 11.34, 32) # Cannot convert float(11.34) to Int16

        c.Speak('hello world', None)

        c.StopAll(0)
        c.Hide(0)
        sleep(1)
        a.Unload(cid)
        
        delete_files("AgentServerObjects.dll")
else: 
    print "warning:", agentsvr_path, "or tlbimp not found"
    
from Microsoft.Win32 import Registry
from System.IO import File

def IsExcelInstalled():
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
    
def TryGetTypeFromProgId(): 
    return Type.GetTypeFromProgID("Excel.Application.12") or Type.GetTypeFromProgID("Excel.Application.11")

def TryLoadInteropAssembly():
    try:    clr.AddReferenceByName('Microsoft.Office.Interop.Excel, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c')
    except: 
        try: clr.AddReferenceByName('Microsoft.Office.Interop.Excel, Version=11.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c')
        except: pass
    
if IsExcelInstalled():
    def test_excel():
        TryLoadInteropAssembly()

        try: import Microsoft.Office.Interop.Excel as Excel
        except ImportError: 
            print "Skip: VSTO/Excel is not installed"
            return
        
        ex = None
        try: 
            ex = Excel.ApplicationClass() 
            ex.DisplayAlerts = False 
            #ex.Visible = True
            nb = ex.Workbooks.Add()
            ws = nb.Worksheets[1]

            AreEqual('Sheet1', ws.Name)

            if not preferComDispatch:
                AssertError(EnvironmentError, lambda: ws.Rows[0])

            for i in range(1, 10):
                for j in range(1, 10):
                    ws.Cells[i, j] = i * j

            rng = ws.Range['A1', 'B3']
            AreEqual(6, rng.Count)

            co = ws.ChartObjects()
            graph = co.Add(100, 100, 200, 200)
            graph.Chart.ChartWizard(rng, Excel.XlChartType.xl3DColumn)                        
        finally:    
            if ex: ex.Quit()
            else: print "ex is %s" % ex

if is_cli32:
    run_test(__name__)

if is_cli64: 
    print "Warning: Skipping Interop tests on 64-bit machines"
