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

import sys
import nt

def Fail(m):
    raise AssertionError(m)

def Assert(c, m = "Assertion failed"):
    if not c:
        raise AssertionError(m)

def AreEqual(a, b):
    Assert(a == b, "expected %r, but found %r" % (b, a))

def AssertError(exc, func, *args):
    try:
        func(*args)
    except exc:
        return
    Assert(False, "Expected %r but got no exception" % exc)

# Check that the exception is raised with the provided message

def AssertErrorWithMessage(exc, expectedMessage, func, *args):
    Assert(expectedMessage, "expectedMessage cannot be null")
    try:
        func(*args)
    except exc, inst:
        Assert(expectedMessage == inst.__str__(), \
               "Exception %r message (%r) does not contain %r" % (type(inst), inst.__str__(), expectedMessage))
        return
    Assert(False, "Expected %r but got no exception" % exc)

# Check that the exception is raised with the provided message, where the message
# differs on IronPython and CPython

def AssertErrorWithMessages(exc, ironPythonMessage, cpythonMessage, func, *args):
    if (sys.platform == "cli"):
        expectedMessage = ironPythonMessage
    else:
        expectedMessage = cpythonMessage
    Assert(expectedMessage, "expectedMessage cannot be null")
    try:
        func(*args)
    except exc, inst:
        Assert(expectedMessage == inst.__str__(), \
               "Exception %r message (%r) does not contain %r" % (type(inst), inst.__str__(), expectedMessage))
        return
    Assert(False, "Expected %r but got no exception" % exc)

# Check that the exception is raised with the provided message, where the message
# is matches using a regular-expression match

def AssertErrorWithMatch(exc, expectedMessage, func, *args):
    import re
    Assert(expectedMessage, "expectedMessage cannot be null")
    try:
        func(*args)
    except exc, inst:
        Assert(re.compile(expectedMessage).match(inst.__str__()), \
               "Exception %r message (%r) does not contain %r" % (type(inst), inst.__str__(), expectedMessage))
        return
    Assert(False, "Expected %r but got no exception" % exc)

path_separator              = "/"
iron_python_root            = sys.prefix
iron_python_tests           = "/Src/Scripts/Tests"
iron_python_test_dll        = "IronPythonTest.dll"
iron_python_test_dll_dbg    = "Src/IronPythonTest/bin/Debug/" + iron_python_test_dll
iron_python_test_dll_rel    = "Src/IronPythonTest/bin/Release/" + iron_python_test_dll
is_cli                      = (sys.platform == "cli")

def get_subdir(path, subdir):
    return path + path_separator + subdir

def copy_iron_python_test(test_assembly):
    import System
    for source in [iron_python_test_dll_dbg, iron_python_test_dll_rel]:
        src_path = System.IO.Path.Combine(sys.prefix, source)
        try:
            System.IO.File.Copy(src_path, test_assembly, True)
            break
        except:
            pass

def load_iron_python_test(*args):
    test_assembly = sys.prefix + path_separator + iron_python_test_dll
    copy_iron_python_test(test_assembly)
    import clr
    if args:
        return clr.LoadAssemblyFromFileWithPath(test_assembly)
    else:
        clr.AddReferenceToFileAndPath(test_assembly)

def get_testmode():
    import System
    lastConsumesNext = False
    switches = []
    for x in System.Environment.GetCommandLineArgs():
        if x.startswith("-"):
            switches.append(x)
            if x == "-X:Optimize" or x == "-W" or x == "-c" or x == "-X:MaxRecursion" or x == "-X:AssembliesDir":
                lastConsumesNext = True
        else:
            if lastConsumesNext:
                 switches.append(x)   
            lastConsumesNext = False
    return switches

def run_standalone(test, add=[], remove=[]):
    import System
    final = get_testmode()
    for x in add:
        if x not in final: final.append(x)
        
    for x in remove:
        if x in final:
            pos = final.index(x)
            if pos <> -1:
                if x == "-X:Optimize" or x == "-W" or x == "-c" or x == "-X:MaxRecursion" or x == "-X:AssembliesDir":
                    del final[pos:pos+2]
                else :
                    del final[pos]
        
    params = tuple(final)
    params += (test,)
    
    return nt.spawnl(0, sys.executable, *params)

def GetTotalMemory():
    from System import GC
    # 3 collect calls to ensure collection
    GC.Collect()
    GC.WaitForPendingFinalizers()
    GC.Collect()
    GC.WaitForPendingFinalizers()
    GC.Collect()
    GC.WaitForPendingFinalizers()
    return GC.GetTotalMemory(True)

def run_csc(args=""):
    import System
    process = System.Diagnostics.Process()
    process.StartInfo.FileName = "csc.exe"
    process.StartInfo.Arguments = args
    process.StartInfo.CreateNoWindow = True
    process.StartInfo.UseShellExecute = False
    process.StartInfo.RedirectStandardInput = True
    process.StartInfo.RedirectStandardOutput = True
    process.StartInfo.RedirectStandardError = True
    process.Start()
    output = process.StandardOutput.ReadToEnd()
    output = process.StandardError.ReadToEnd()
    process.WaitForExit()
    return process.ExitCode

def has_csc():
    try:
        run_csc("/?")
    except WindowsError:
        return False
    return True

def text_to_file(s, file):
    f = open(file, "w")
    f.write(s)
    f.flush()
    f.close()
    
def delete_files(*files):
    import nt
    for f in files: nt.remove(f)