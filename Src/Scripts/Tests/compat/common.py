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

import nt
import sys
import time

is_cli = (sys.platform == "cli")

result_pass = 0
result_fail = 1

def file_exists(file):
    try:
        nt.stat(file)
        return True
    except:
        return False

def get_all_paths():
    if sys.platform == "cli":
        ipython_executable = sys.executable
        compat_test_path   = sys.prefix + "/Src/Scripts/Tests/Compat/"
        cpython_executable = sys.prefix + "/../External/Regress/Python24/Python.exe"
        cpython_lib_path   = sys.prefix + "/../External/Regress/Python24/Lib"
        
    elif sys.platform == "win32":
        cpython_executable = sys.executable
        cpython_lib_path   = sys.prefix + "/Lib"
        ipython_executable = sys.prefix + "/../../../Public/IronPythonConsole.exe"
        compat_test_path   = sys.prefix + "/../../../Public/Src/Scripts/Tests/Compat/"
  
        # second try    
        if file_exists(ipython_executable) == False:
            ipython_executable = nt.environ['IP_ROOT'] + "/Public/IronPythonConsole.exe"
            compat_test_path   = nt.environ['IP_ROOT'] + "/Public/Src/Scripts/Tests/Compat/"
    
    else:
        raise AssertionError
    
    assert file_exists(cpython_executable)
    assert file_exists(cpython_lib_path)
    assert file_exists(ipython_executable)
    assert file_exists(compat_test_path)
    
    return cpython_executable, cpython_lib_path, ipython_executable, compat_test_path

cpython_executable, cpython_lib_path, ipython_executable, compat_test_path = get_all_paths()
 
def delete_files(files):
    for f in files: 
        nt.remove(f)

def launch(executable, test):
    if sys.platform == "cli":
        return nt.spawnl(0, executable, *(test,))
    else:
        return nt.spawnv(0, executable, (executable, test))
        
class my_stdout:
    def __init__(self, o):
        self.stdout = o
    def write(self, s):
        self.stdout.write(s)

def printwith(head, *arg): 
    print "%s##" %head, 
    for x in arg: print x,
    print 

def printwithtype(arg):
    t = type(arg)
    if t == float:
        print "float## %.4f" % arg
    elif t == complex:
        print "complex## %.4f | %.4f" % (arg.real, arg.imag)
    else:
        print "same##", arg
    
def fullpath(file):
    return compat_test_path + file

def run_single_test(test, filename = None):
    if filename == None: 
        ret = test()
    else :
        filename = fullpath(filename)
        file = open(filename, "w+")
        saved = sys.stdout
        sys.stdout = my_stdout(file)
        ret = result_fail
        try:
            ret = test()
        finally:
            sys.stdout = saved
            file.close()
    return ret
        
def get_platform_string(current = None):
    ''' return my customized platform string
        if no param, it returns the string based on current python runtime
        if current is provide, this function is a mapping. 
    '''
    if current == None:
        import sys
        current = sys.platform
        
    if current.startswith("win"): return "win"
    if current.startswith("cli"): return "cli"
    return "non"

def get_summary_file(platform = None):
    return compat_test_path + "/" + get_platform_string(platform) + "_summary.log"

def create_new_file(filename):
    f = file(filename, "w")
    f.close()

def append_string_to_file(filename, *lines):
    f = file(filename, "a")
    for x in lines:
        f.writelines(x + "\n")
    f.close()

def ensure_future_present():
    import nt 
    future = compat_test_path + "/__future__.py"
    try:
        nt.stat(future)
    except:
        append_string_to_file(future, "division = 1")
    else:
        pass

def get_class_name(type):
    typename = str(type)
    return typename[typename.rfind('.')+1:-2]

def get_file_name(type, test, platform = None):
    ''' return the log file for the specified test
    '''
    return "_".join((get_platform_string(platform), type.__module__, get_class_name(type), test)) + ".log"
    
def runtests(type):
    import sys
    obj = type()
    summary_file = get_summary_file()
    
    for x in dir(type):
        if x.startswith("test_") == False: 
            continue
        test = getattr(obj, x)
        if callable(test) == False:
            continue
        
        typename = get_class_name(type)
        print "  %s\\%s" % (typename, x)
        log_filename = get_file_name(type, x)
        append_string_to_file(summary_file, log_filename)
        run_single_test(test, log_filename)

class MyException: pass
