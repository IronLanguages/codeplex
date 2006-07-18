#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################

# COM Interop tests for IronPython

import nt
import clr
import sys

from lib.assert_util import *
from lib.file_util import *
from lib.process_util import *

from System import Type, Activator, Environment, IntPtr, Array, Object, Int32
from System.Reflection import BindingFlags

windir = nt.environ["windir"] or ""
scriptpw_path = path_combine(windir, r"system32\scriptpw.dll")
agentsvr_path = path_combine(windir, r"msagent\agentsvr.exe")

# TEST MATRIX:
#
#                    (PIA)                  (No PIA)
# (Registered)
# (Not Registered)   excel/merlin

def _test_common(o):
    for x in ['GetHashCode', 'GetPassword', '__repr__', 'ToString']:
        Assert(x in dir(o))

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

def test__1_registered_nopia():
    # Check to see that namespace 'spwLib' isn't accessible
    Assert('spwLib' not in dir(), "spwLib is already registered")

    run_register_com_component(scriptpw_path)
    
    pwcType = Type.GetTypeFromProgID('ScriptPW.Password.1')
    pwcInst = Activator.CreateInstance(pwcType)
    
    Assert('__ComObject' in repr(pwcInst))
    AreEqual('System.__ComObject', pwcInst.ToString())
    
    try: del pwcInst.GetPassword
    except AttributeError: pass
    else: Fail("'__ComObject' object has no attribute 'GetPassword'")
    
    _test_common(pwcInst)

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
    
    _test_common(pc)
    
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

def test_merlin():
    run_tlbimp(agentsvr_path)
    Assert(file_exists("AgentServerObjects.dll"))
    
    import clr
    clr.AddReference("AgentServerObjects.dll")

    from AgentServerObjects import * 
    a = AgentServerClass()    
    Assert('Equals' in dir(a))
    cid = a.Load('Merlin.acs')[0]
    
    c = a.GetCharacter(cid)
    #c.Show(0)
    c.Think('IronPython...')
    c.Play('Read')
    c.GestureAt(True, False)
    c.GestureAt(100, 200)
    AssertError(TypeError, c.GestureAt, 11.34, 32) # Cannot convert float(11.34) to Int16
    
    c.Speak('hello world', None)
    
    c.StopAll(0)
    c.Hide(0)
    
    delete_files("AgentServerObjects.dll")

from Microsoft.Win32 import Registry

if Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Office\\11.0\\Excel"):
    def test_excel():
        import clr
        clr.AddReferenceByName('Microsoft.Office.Interop.Excel, Version=11.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c')

        try: import Microsoft.Office.Interop.Excel as Excel
        except ImportError: 
            print "Warning: VSTO is not installed"
            return
        
        saved = number_of_process('excel.exe')
        ex = None
        try: 
            ex = Excel.ApplicationClass() 
            ex.DisplayAlerts = False 
            #ex.Visible = True
            nb = ex.Workbooks.Add()
            ws = nb.Worksheets[1]
            
            AreEqual('Sheet1', ws.Name)
            AssertError(EnvironmentError, lambda: ws.Rows[0])
            
            for i in range(1, 10):
                for j in range(1, 10):
                    ws.Cells[i, j] = i * j
            
            #add more here related to get_Range, chart after fix
            #rng = ws.get_Range('A1', '')
        finally:    
            if ex: ex.Quit()
            else: print "ex is %s" % ex

if Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Office\\11.0\\PowerPoint"):
    def test_powerpoint():
        pp = None   
        try:
            ppt = Type.GetTypeFromProgID("PowerPoint.Application.11")
            pp = Activator.CreateInstance(ppt)
            # test that late-binding call to Name works the same as the one from the typeinfo
            ppName = ppt.InvokeMember("Name", BindingFlags.GetProperty, None, pp, None)
            Assert (ppName == "Microsoft PowerPoint", "Late-bound Name property should be 'Microsoft PowerPoint'")
            Assert (ppName == pp.Name)
            # test that we can change the caption from IronPython
            if pp.Caption == "PowerPoint Controlled By Iron Python":
                Fail("Kill PowerPnt.exe and try again")
            Assert (pp.Caption == "Microsoft PowerPoint")
            pp.Caption = "PowerPoint Controlled By Iron Python"
            Assert (pp.Caption == "PowerPoint Controlled By Iron Python", "Setting caption should work")
            # make it visible, just for kicks
            args = Array.CreateInstance(Object, 1)
            args[0] = Int32(-1) # literal value of msoTrue in MsoTriState enum, gacked from Office PIA by ILDASM
            ppt.InvokeMember("Visible", BindingFlags.SetProperty, None, pp, args)
            Assert (pp.Visible)
            try:
                for i in pp: pass
                Assert(False, "PPT instance should not be enumerable")
            except TypeError:
                pass
            for i in pp.Windows: pass
            
            Assert('ToString' in dir(pp))
            Assert('ActiveWindow' in dir(pp))
        finally:
            if pp: pp.Quit()
            else: print "ex is %s" % ex

if is_cli32:
    run_test(__name__)

if is_cli64: 
    print "Warning: Skipping Interop tests on 64-bit machines"
