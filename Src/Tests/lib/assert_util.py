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

### make this file platform neutral as much as possible

import nt
import sys
from file_util import *
from type_util import types

is_cli = sys.platform == 'cli'

def usage(code, msg=''):
    print sys.modules['__main__'].__doc__ or 'No doc provided'
    if msg: print 'Error message: "%s"' % msg
    sys.exit(code)

def get_environ_variable(key):
    l = [nt.environ[x] for x in nt.environ.keys() if x.lower() == key.lower()]
    if l: return l[0]
    else: return None

def get_temp_dir():
    temp = get_environ_variable("TEMP")
    if temp == None: temp = get_environ_variable("TMP")
    if (temp == None) or (' ' in temp) : 
        temp = r"C:\temp"
    return temp
    
class testpath:
    # find the ironpython root directory
    ip_root             = get_parent_directory(sys.prefix)

    # get some directories and files
    external_dir        = path_combine(ip_root, r'External')
    public_testdir      = path_combine(sys.prefix, r'Src\Tests')
    compat_testdir      = path_combine(sys.prefix, r'Src\Tests\compat')
    script_testdir      = path_combine(sys.prefix, r'Src\Scripts')

    parrot_testdir      = path_combine(external_dir, r'parrotbench')
    lib_testdir         = path_combine(external_dir, r'Regress\Python24\Lib')
    private_testdir     = path_combine(external_dir, r'Regress\Python24\Lib\test')

    temporary_dir       = path_combine(get_temp_dir(), "IronPython")
    
    iron_python_test_dll        = 'IronPythonTest.dll'
    iron_python_test_src_dir    = path_combine(sys.prefix, r'Src\IronPythonTest')
    iron_python_test_dll_final  = path_combine(sys.prefix, iron_python_test_dll)
    
    if is_cli: 
        ipython_executable  = sys.executable
        cpython_executable  = path_combine(external_dir, r'Regress\Python24\python.exe')
    else: 
        ipython_executable  = path_combine(sys.prefix, r'ironpythonconsole.exe')
        cpython_executable  = sys.executable
    
    team_dir            = path_combine(ip_root, r'Team')
    team_profile        = path_combine(team_dir, r'settings.py')
    
    my_name             = nt.environ.get(r'USERNAME', None)
    my_dir              = my_name and path_combine(team_dir, my_name) or None
    my_profile          = my_dir and path_combine(my_dir, r'settings.py') or None

ensure_directory_present(testpath.temporary_dir)

class formatter:
    Number         = 60
    TestNameLen    = 40
    SeparatorEqual = '=' * Number
    Separator1     = '#' * Number
    SeparatorMinus = '-' * Number
    SeparatorStar  = '*' * Number
    SeparatorPlus  = '+' * Number
    Space4         = ' ' * 4
    Greater4       = '>' * 4

# helper functions for sys.path
_saved_syspath = []
def perserve_syspath(): 
    _saved_syspath[:] = list(set(sys.path))
    
def restore_syspath():  
    sys.path = _saved_syspath[:]

# test support 
def Fail(m):  raise AssertionError(m)

def Assert(c, m = "Assertion failed"):
    if not c: raise AssertionError(m)

def AreEqual(a, b):
    Assert(a == b, "expected %r, but found %r" % (b, a))

def AlmostEqual(a, b):
    Assert(round(a-b, 6) == 0, "expected %r and %r almost same" % (a, b))    
    
def AssertError(exc, func, *args):
    try:        func(*args)
    except exc: return
    else :      Fail("Expected %r but got no exception" % exc)

# Check that the exception is raised with the provided message

def AssertErrorWithMessage(exc, expectedMessage, func, *args):
    Assert(expectedMessage, "expectedMessage cannot be null")
    try:   func(*args)
    except exc, inst:
        Assert(expectedMessage == inst.__str__(), \
               "Exception %r message (%r) does not contain %r" % (type(inst), inst.__str__(), expectedMessage))
    else:  Assert(False, "Expected %r but got no exception" % exc)

# Check that the exception is raised with the provided message, where the message
# differs on IronPython and CPython

def AssertErrorWithMessages(exc, ironPythonMessage, cpythonMessage, func, *args):
    expectedMessage = is_cli and ironPythonMessage or cpythonMessage

    Assert(expectedMessage, "expectedMessage cannot be null")
    try:   func(*args)
    except exc, inst:
        Assert(expectedMessage == inst.__str__(), \
               "Exception %r message (%r) does not contain %r" % (type(inst), inst.__str__(), expectedMessage))
    else:  Assert(False, "Expected %r but got no exception" % exc)

# Check that the exception is raised with the provided message, where the message
# is matches using a regular-expression match

def AssertErrorWithMatch(exc, expectedMessage, func, *args):
    import re
    Assert(expectedMessage, "expectedMessage cannot be null")
    try:   func(*args)
    except exc, inst:
        Assert(re.compile(expectedMessage).match(inst.__str__()), \
               "Exception %r message (%r) does not contain %r" % (type(inst), inst.__str__(), expectedMessage))
    else:  Assert(False, "Expected %r but got no exception" % exc)


testdll_copied = False
def copy_iron_python_test():
    global testdll_copied
    if testdll_copied: return 
    
    for flavor in ['debug', 'release']:
        src_path = path_combine(testpath.iron_python_test_src_dir, 'bin', flavor, testpath.iron_python_test_dll)
        try:
            filecopy(src_path, testpath.iron_python_test_dll_final)
            testdll_copied = True
            return 
        except: pass

def load_iron_python_test(*args):
    copy_iron_python_test()
    import clr
    if args: 
        return clr.LoadAssemblyFromFileWithPath(testpath.iron_python_test_dll_final)
    else: 
        clr.AddReferenceToFileAndPath(testpath.iron_python_test_dll_final)

def GetTotalMemory():
    import System
    # 3 collect calls to ensure collection
    for x in range(3):
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()
    return System.GC.GetTotalMemory(True)

def run_csc(args=""):
    return run_tool("csc.exe", args)

def run_tool(cmd, args=""):
    import System
    process = System.Diagnostics.Process()
    process.StartInfo.FileName = cmd
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
    try:   run_csc("/?")
    except WindowsError: return False
    else:  return True

def run_test(mod_name, verbose=False):
    import sys
    module = sys.modules[mod_name]
    for name in dir(module): 
        obj = getattr(module, name)
        if isinstance(obj, types.functionType) and name.startswith("test_"): 
            if verbose or mod_name == '__main__': print "Testing %s" % name
            obj()

def run_class(mod_name, verbose=False): 
    pass
    
