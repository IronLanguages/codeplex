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
This script outputs a log file showing missing modules, module members, etc 
from:
- builtin CPython modules 
- *.pyd files
- modules which IronPython recreates

Also, it creates another log file showing extra methods IP implements which
it should not.

USAGE:
    ipy cmodule.py C:\Python25

NOTES:
- the BUILTIN_MODULES list needs to be updated using the info found at 
  pydoc.org (e.g., http://pydoc.org/2.5.1/) for every new IronPython
  release corresponding to a new major CPython release
'''

from sys import argv
import gc
import nt
from clr_helpers import Process

CPY_DIR = argv[1]  #E.g., C:\Python26

#--GLOBALS---------------------------------------------------------------------

#CPython builtin modules
BUILTIN_MODULES =  [
                    "__builtin__",
                    "_ast", #CodePlex 21088
                    "_bisect", #CodePlex 21392
                    "_bytesio", #CodePlex 21393
                    "_codecs",
                    "_codecs_cn", #CodePlex 15507
                    "_codecs_hk", #CodePlex 15507
                    "_codecs_iso2022", #CodePlex 21394
                    "_codecs_jp", #CodePlex 15507
                    "_codecs_kr", #CodePlex 15507
                    "_codecs_tw", #CodePlex 15507
                    "_collections", 
                    "_csv", #CodePlex 21395
                    "_fileio", #CodePlex 19545
                    "_functools",
                    "_heapq", #CodePlex 21396
                    "_hotshot", #CodePlex 21397
                    "_json", #CodePlex 19581
                    "_locale",
                    "_lsprof", #CodePlex 21398
                    "_md5",
                    "_multibytecodec", #CodePlex 21399
                    "_random",
                    "_sha",
                    "_sha256",
                    "_sha512",
                    "_sre",
                    "_struct",
                    "_symtable", #IronPython incompat
                    "_subprocess", #CodePlex 15512
                    #"_types",  Can't import this in CPython 2.6 either...
                    "_warnings", 
                    "_weakref", 
                    "_winreg", 
                    "array", 
                    "aidoop", #CodePlex 21400
                    "binascii", 
                    "cPickle", 
                    "cStringIO", 
                    "cmath", 
                    "datetime", 
                    "errno",
                    "exceptions",
                    "future_builtins", #CodePlex 19580
                    "gc",
                    "imageop", #Deprecated in CPy 2.6.  Removed in Cpy 3.0
                    "imp",
                    "itertools", 
                    "marshal",
                    "math", 
                    "mmap", #CodePlex 21401
                    "msvcrt", #CodePlex 21402
                    "nt", 
                    "operator", 
                    "parser", #CodePlex 1347 - Won't fix
                    "signal", #CodePlex 16414
                    "strop", #CodePlex 21403
                    "sys",
                    "thread",
                    "time", 
                    "xxsubtype",
                    "zipimport", #CodePlex 391
                    "zlib", #CodePlex 2590
                    ]
 
#Most of these are standard *.py modules which IronPython overrides for one 
#reason or another
OVERRIDDEN_MODULES =  [ 
            "copy_reg",
            "socket",
        ]

MODULES = BUILTIN_MODULES + OVERRIDDEN_MODULES
           
#Add any extension modules found in DLLs or Lib
for x in nt.listdir(CPY_DIR + "\\DLLs"):
    if x.endswith(".pyd"):
        MODULES.append(x.split(".pyd", 1)[0])
        
for x in nt.listdir(CPY_DIR + "\\Lib"):
    if x.endswith(".pyd"):
        MODULES.append(x.split(".pyd", 1)[0])

#Modules we don't implement found in DLLs or Lib and
#the reason why:
# bz2 - TODO?
# pyexpat - CodePlex 20023
# unicodedata - CodePlex 21404
# winsound - CodePlex 21405
# _bsddb - CodePlex 362
# _ctypes - CodePlex 374
# _ctypes_test - dependent upon CodePlex 374
# _elementtree - CodePlex 21407
# _hashlib - CodePlex 21408
# _msi - CodePlex 21409
# _multiprocessing - CodePlex 19542
# _socket - N/A.  We already implement socket.py directly in C#
# _sqlite3 - CodePlex 21410
# _ssl - CodePlex 21411
# _testcapi N/A. This tests the C API for CPython
# _tkinter - TODO?


#Let the user override modules from the command-line
if len(argv)==3:
    MODULES = [argv[2]]

#Log containing all modules and their attributes which IP should implement, but does not
IPY_SHOULD_IMPL = open("IPY_NEEDS_TO_IMPLEMENT.log", "w")
#Log containing all module attributes that IP should not be implementing
IPY_SHOULD_NOT_IMPL = open("IPY_SHOULD_NOT_IMPLEMENT.log", "w")

#TODO: each of these members attached to string objects include MANY more
#      members IP does not implement
str_functions = [   'capitalize', 'center', 'count', 'decode',
                    'encode', 'endswith', 'expandtabs', 'find', 'index', 
                    'isalnum', 'isalpha', 'isdigit', 'islower', 'isspace', 
                    'istitle', 'isupper', 'join', 'ljust', 'lower', 'lstrip', 
                    'partition', 'replace', 'rfind', 'rindex', 'rjust', 
                    'rpartition', 'rsplit', 'rstrip', 'split', 'splitlines', 
                    'startswith', 'strip', 'swapcase', 'title', 'translate', 
                    'upper', 'zfill']

#The maximum recursion depth used when examining the attributes of any 
#CPython module.
MAX_DEPTH = 10


IGNORE_LIST = [ "__builtin__.print",
                ]

#--FUNCTIONS-------------------------------------------------------------------
def ip_missing(mod_attr):
    '''
    Logs a module or module attribute IP is missing.
    '''
    IPY_SHOULD_IMPL.write(mod_attr + "\n")
    IPY_SHOULD_IMPL.flush() 
    #print mod_attr
    
def ip_extra(mod_attr):
    '''
    Logs a module attribute IP provides, but should not.
    '''
    IPY_SHOULD_NOT_IMPL.write(mod_attr + "\n")
    IPY_SHOULD_NOT_IMPL.flush() 
    #print mod_attr    


def get_cpython_results(name, level=0, temp_mod=None):
    '''
    Recursively gets all attributes of a CPython module up to a depth of
    MAX_DEPTH.
    '''
    #from the name determine the module
    if "." in name:
        mod_name, rest_of_name = name.split(".", 1)
        rest_of_name += "."
    else:
        #we're looking at the module for the first time...import it
        mod_name, rest_of_name = name, ""
        
        try:
            temp_mod = __import__(mod_name)
        except ImportError, e:
            ip_missing(mod_name)
            return

    #Get the results of:
    #   python.exe -c 'import abc;print dir(abc.xyz)'
    proc = Process()
    proc.StartInfo.FileName = CPY_DIR + "\\python.exe"
    proc.StartInfo.Arguments = "-c \"import " + mod_name + ";print dir(" + name + ")\""
    proc.StartInfo.UseShellExecute = False
    proc.StartInfo.RedirectStandardOutput = True
    if (not proc.Start()):
        raise "CPython process failed to start"
    else:
        cpymod_dir = proc.StandardOutput.ReadToEnd()
        
    #Convert "['a', 'b']" to a real (sorted) list
    cpymod_dir = eval(cpymod_dir)
    cpymod_dir.sort()
    
    #Determine what IronPython implements
    if level==0:
        ipymod_dir_str = "dir(temp_mod)"
    else:
        ipymod_dir_str = "dir(temp_mod." + name.split(".", 1)[1] + ")"

    
    try:
        ipymod_dir = eval(ipymod_dir_str)
        #Ensure this is also present CPython
        for x in [y for y in ipymod_dir if cpymod_dir.count(y)==0]:
            ip_extra(name + "." + x)
    except TypeError, e:
        #CodePlex 15715
        if not ipymod_dir_str.endswith(".fromkeys)"):
            print "ERROR:", ipymod_dir_str
            raise e
    
    #Look through all attributes the CPython version of the 
    #module implements
    for x in cpymod_dir:
        if name + "." + x in IGNORE_LIST:
            print "Will not reflect on", name + "." + x
            return
    
        #Check if IronPython is missing the CPython attribute
        try:    
            temp = eval("temp_mod." + rest_of_name + x)
        except AttributeError, e:
            ip_missing(name + "." + x)
            continue
        
        #Skip these as they will recurse forever
        if x.startswith("__") and x.endswith("__"):
            continue
        #Skip these as they overload the log files
        elif x in ["_Printer__setup", "im_class", "_Printer__name", "func_code", "func_dict", "func_globals"]:
            continue 
        #Each of these str functions has many __*__ methods
        elif x in str_functions and level > 2:
            continue
        #Skip these as they recurse forever
        elif name.startswith("datetime") and x in ["min", "max", "resolution"]:
            continue
        elif level>=MAX_DEPTH:
            print "Recursion too deep:", name, x
            continue
        get_cpython_results(name + "." + x, level+1, temp_mod)
    
    return
    
#--MAIN------------------------------------------------------------------------
for mod_name in MODULES:
    get_cpython_results(mod_name)
    
IPY_SHOULD_IMPL.close()
IPY_SHOULD_NOT_IMPL.close()
