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
from lib.assert_util import *
from System import Type, Activator, Environment, IntPtr, Array, Object, Int32
from System.Runtime.InteropServices import RuntimeEnvironment as RTEnv
from System.Reflection import BindingFlags
import nt
import clr
import sys

windir = Environment.GetEnvironmentVariable("windir") or ""
runtimeDir = RTEnv.GetRuntimeDirectory()
sys.path.append(nt.getcwd())

# find path to tlb
tlbImpHome = ''
try:
    if 'TlbImp.exe' in nt.listdir(Environment.GetEnvironmentVariable("ProgramFiles")+"\\Microsoft.NET\\SDK\\v2.0\\bin"):
        tlbImpHome = Environment.GetEnvironmentVariable("ProgramFiles")+"\\Microsoft.NET\\SDK\\v2.0\\bin"
except IOError:
    pass

if 'TlbImp.exe' in nt.listdir(runtimeDir):
   tlbImpHome= runtimeDir

for direc in Environment.GetEnvironmentVariable("PATH").split(';'):
    if 'TlbImp.exe' in nt.listdir(direc):
        tlbImpHome = direc
    
# convert a typelib/dll to a .NET assembly
def call_tlbimp(pathToTypeLib, outputName):
    from System.Diagnostics import Process, ProcessStartInfo
    psi = ProcessStartInfo(tlbImpHome+"\\tlbimp.exe", pathToTypeLib+" /out:"+outputName)
    psi.UseShellExecute = False;
    psi.RedirectStandardOutput = True;
    p = Process.Start(psi)
    p.WaitForExit()
    return p.ExitCode

def register_com_component(pathToDll):
    from System.Diagnostics import Process, ProcessStartInfo
    psi = ProcessStartInfo("regsvr32.exe",  "/s "+pathToDll)
    psi.UseShellExecute = False
    psi.RedirectStandardOutput = True
    p = Process.Start(psi)
    p.WaitForExit()
    return p.ExitCode

def unregister_com_component(pathToDll):
    from System.Diagnostics import Process, ProcessStartInfo
    psi = ProcessStartInfo("regsvr32.exe", "/s /u "+pathToDll)
    psi.UseShellExecute = False
    psi.RedirectStandardOutput = True
    p = Process.Start(psi)
    p.WaitForExit()
    return p.ExitCode


# Test matrix: X means 'not covered'
#
#                    (PIA)           (No PIA)
# (Registered)
# (Not Registered)     X                

def EnsureSpwLibIsNotImported():
    try:
        from spwLib import *
        raise "spwLib is already registered"
    except ImportError:
        pass

def registered_nopia():
    # Check to see that namespace 'spwLib' isn't accessible
    print "running register_nopia"
    EnsureSpwLibIsNotImported()
    register_com_component(windir+"\\system32\\scriptpw.dll")
    pwcType = Type.GetTypeFromProgID('ScriptPW.Password.1')
    pwcInst = Activator.CreateInstance(pwcType)
    Assert('__ComObject' in repr(pwcInst))
    Assert('ToString' in dir(pwcInst))
    Assert('GetPassword' in dir(pwcInst))
    try:
        pwcInst[3] = "something"
        Assert(False)
    except AttributeError:
        pass
    try:
        something = pwcInst[3]
        Assert(False)
    except AttributeError:
        pass
    try:
        del pwcInst.GetPassword
        Assert(False)
    except Exception:
        pass                      
    # should be TypeError, is SystemError
    AssertError(TypeError, (lambda:pwcInst+3))
    AssertError(TypeError, (lambda:pwcInst-3))
    AssertError(TypeError, (lambda:pwcInst*3))
    AssertError(TypeError, (lambda:pwcInst/3))
    AssertError(TypeError, (lambda:pwcInst >> 3))
    AssertError(TypeError, (lambda:pwcInst << 3))

def unregistered_nopia():
    print "running unregistered_nopia"
    # Check to see that namespace 'spwLib' isn't accessible
    try:
        from spwLib import *
        return None
    except ImportError:
        pass
    unregister_com_component(windir+"\\system32\\scriptpw.dll")
    pwcType = Type.GetTypeFromProgID('ScriptPW.Password.1')
    AreEqual(pwcType, None)
    
    # Registration-free COM activation
    load_iron_python_test()
    import IronPythonTest
    password = IronPythonTest.ScriptPW.CreatePassword()
    AreEqual('System.__ComObject', password.ToString())

def with_pia():
    print "running with_pia"
    call_tlbimp(windir+"\\system32\\scriptpw.dll", "spwLib")
    register_com_component(windir+"\\system32\\scriptpw.dll")
    clr.AddReference("spwLib.dll")
    from spwLib import PasswordClass as PC
    password = PC()
    Assert('ToString' in str(password.ToString))
    Assert('GetPassword' in str(password.GetPassword))

def ppt_test():
    from Microsoft.Win32 import Registry, RegistryKey
    hkeyCurrentUser = Registry.CurrentUser
    if hkeyCurrentUser.OpenSubKey("Software\\Microsoft\\Office\\11.0\\PowerPoint"):
        print "running ppt_test"
        powerPtType = Type.GetTypeFromProgID("PowerPoint.Application.11")
        powerPtInst = Activator.CreateInstance(powerPtType)
        # test that late-binding call to Name works the same as the one from the typeinfo
        ppName = powerPtType.InvokeMember("Name", BindingFlags.GetProperty, None, powerPtInst, None)
        Assert (ppName == "Microsoft PowerPoint", "Late-bound Name property should be 'Microsoft PowerPoint'")
        Assert (ppName == powerPtInst.Name)
        # test that we can change the caption from IronPython
        if powerPtInst.Caption == "PowerPoint Controlled By Iron Python":
            Fail("Kill PowerPnt.exe and try again")
        Assert (powerPtInst.Caption == "Microsoft PowerPoint")
        powerPtInst.Caption = "PowerPoint Controlled By Iron Python"
        Assert (powerPtInst.Caption == "PowerPoint Controlled By Iron Python", "Setting caption should work")
        # make it visible, just for kicks
        args = Array.CreateInstance(Object, 1)
        args[0] = Int32(-1) # literal value of msoTrue in MsoTriState enum, gacked from Office PIA by ILDASM
        powerPtType.InvokeMember("Visible", BindingFlags.SetProperty, None, powerPtInst, args)
        Assert (powerPtInst.Visible)
        try:
            for i in powerPtInst: pass
            Assert(False, "PPT instance should not be enumerable")
        except TypeError:
            pass
        for i in powerPtInst.Windows:
            pass
        Assert('ToString' in dir(powerPtInst))
        Assert('ActiveWindow' in dir(powerPtInst))
        powerPtInst.Quit()
#        powerPtType.InvokeMember("Quit", BindingFlags.InvokeMethod, None, powerPtInst, None)
    else:
        print "Warning: ppt_test() vacuously true, PowerPoint not installed.. please run on a machine with Office 11"

# !! do not run on 64-bit
if IntPtr.Size < 8:
    # Check to see that scriptpw.dll is in %windir%\System32
    if 'scriptpw.dll' in nt.listdir(windir+'\\system32'):
        # Run before registered_nopia() to ensure that the COM types were not previously loaded
        unregistered_nopia()
        
        registered_nopia()

        if tlbImpHome != '':
            with_pia()
        else:
            print "Warning: Skipping Interop tests since tlbimp.exe was not found"
    else:
        print "Warning: Skipping Interop tests since scriptpw.dll was not found"

    ppt_test()
else:
    print "Warning: Skipping Interop tests on 64-bit machines"
