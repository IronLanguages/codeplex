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

'''
OVERVIEW:
This script outputs a log file showing standard CPython modules which cannot be 
imported from IronPython.

USAGE:
    ipy stdmodules.py C:\Python25
'''

from   sys import argv, path
import nt
import sys


#--GLOBALS---------------------------------------------------------------------
CPY_DIR = argv[1]  #E.g., C:\Python25
CPY_LIB_DIR = CPY_DIR + "\\Lib"  #E.g., C:\Python25\Lib
LOG_FILE_BUSTED = open("STD_MODULES_BROKEN_UNDER_IP.log", "w")
LOG_FILE_OK     = open("STD_MODULES_OK_UNDER_IP.log", "w")
STDOUT_FAKE = open(__file__ + ".stdout.log", "w")
STDOUT_BAK = sys.stdout
FILE_LIST = [LOG_FILE_BUSTED, LOG_FILE_OK, STDOUT_FAKE]


#--FUNCTIONS-------------------------------------------------------------------
def import_helper(mod_name):
    '''
    Helper function used to temporarily override stdout before importing
    a module.
    '''
    try:
        sys.stdout = STDOUT_FAKE
        __import__(mod_name)
    finally:
        sys.stdout = STDOUT_BAK
             
                
def is_package(dir_name):
    '''
    Returns True if dir_name is actually a Python package in the current
    working directory.
    '''
    #*.py, *.pyd, etc
    if "." in dir_name:
        return False        
    
    #Make sure it exists
    try:
        if not nt.stat(dir_name): return False
    except:
        return False
    
    #Make sure it has an __init__.py
    try:
        if "__init__.py" not in nt.listdir(nt.getcwd() + "\\" + dir_name):
            return False
    except:
        return False
        
    return True


def log_broken(name, e):
    print name, "FAILED"
    print >> LOG_FILE_BUSTED, "----------------------------------------------------------------"
    print >> LOG_FILE_BUSTED, "--", name
    if hasattr(e, "clsException"):
        print >> LOG_FILE_BUSTED, e.clsException
    else:
        print >> LOG_FILE_BUSTED, e      


def log_ok(name):
    print name, "PASSED"    
    print >> LOG_FILE_OK, name


def check_package(package_name):
    '''
    Checks all subpackages and modules in the package_name package.
    '''
    cwd = nt.getcwd()
    
    if cwd==CPY_LIB_DIR:
        root_name = package_name
    else:
        root_name = cwd.split(CPY_DIR + "\\Lib\\")[1].replace("\\", ".") + "." + package_name
       
    #First check that the root package can be imported 
    try:
        import_helper(package_name)    
        log_ok(root_name)
        
    except (Exception, SystemExit), e:
        log_broken(root_name, e)
            
    #Next examine subpackages and modules
    nt.chdir(cwd + "\\" + package_name)        
    
    for x in nt.listdir("."):
        if x.endswith(".py") and x!="__init__.py":
            x = x.split(".py", 1)[0]
            mod_name = nt.getcwd().split(CPY_DIR + "\\Lib\\")[1] + "\\" + x
            mod_name = mod_name.replace("\\", ".")
        
            try:
                import_helper(x)   
                log_ok(mod_name) 
        
            except (Exception, SystemExit), e:
                log_broken(mod_name, e)
        
        elif is_package(x):
            check_package(x)

    nt.chdir(cwd)

#--MAIN------------------------------------------------------------------------
path.append(CPY_LIB_DIR)
path.insert(0, ".")


print "------------------------------------------------------------------------"
print "--PACKAGES"
cwd = nt.getcwd()
nt.chdir(CPY_LIB_DIR)

for pack_name in nt.listdir(CPY_LIB_DIR):
    if not is_package(pack_name):
        continue
    #Importing all tests takes too long
    elif pack_name in ["test"]:
        continue
        
    check_package(pack_name)
print

nt.chdir(cwd)

print "------------------------------------------------------------------------"
print "--MODULES"

for x in nt.listdir(CPY_LIB_DIR):
    if x.endswith(".py"):
        mod_name = x.split(".py", 1)[0]
    else:
        continue
        
    try:
        import_helper(mod_name)   
        log_ok(mod_name) 
        
    except Exception, e:
        log_broken(mod_name, e)

        
#Cleanup        
for f in FILE_LIST:
    f.close()
