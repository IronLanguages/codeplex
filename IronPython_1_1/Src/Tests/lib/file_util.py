#####################################################################################
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public
# License. A  copy of the license can be found in the License.html file at the
# root of this distribution. If  you cannot locate the  Microsoft Public
# License, please send an email to  dlr@microsoft.com. By using this source
# code in any fashion, you are agreeing to be bound by the terms of the 
# Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#####################################################################################

## BE PLATFORM NETURAL

import nt
import sys

colon = ':'
separator = '\\'

def create_new_file(filename):
    f = file(filename, "w")
    f.close()

def append_string_to_file(filename, *lines):
    f = file(filename, "a")
    for x in lines:
        f.writelines(x + "\n")
    f.close()

def directory_exists(path):
    try:    
        nt.stat(path)
        return True
    except: 
        return False

def file_exists(file):
    try:    
        nt.stat(file)
        return True
    except: 
        return False
        
def file_exists_in_path(file):
    full_path = [nt.environ[x] for x in nt.environ.keys() if x.lower() == "path"]
    if len(full_path)==0:
        return False
    else:
        full_path = full_path[0]
    
    for path in [nt.getcwd()] + full_path.split(";"):
        path = path.lstrip().rstrip()
        if file_exists(path + "\\" + file) == True:
            return True
    
    return False
        

# need consider .. and . later
def fullpath(path):
    if colon not in path:
        return nt.getcwd() + separator + path
    else: 
        return path

def path_combine(*paths):
    l = len(paths)
    p = ''
    for x in paths[:-1]:
        if x[-1] == separator:
            p += x 
        else: 
            p += x + separator
    return p + paths[-1]
            
def ensure_directory_present(path): 
    path = fullpath(path)
    p = ''
    for x in path.split(separator):
        p += x + separator
        if not directory_exists(p):
            nt.mkdir(p)
        
def write_to_file(filename, content=''):
    filename = fullpath(filename)
    pos = filename.rfind(separator)
    try:
        ensure_directory_present(filename[:pos])
        f = file(filename, 'w')
        f.write(content)
        f.close()
    except: 
        raise AssertionError, 'unable to write to file'
    
def delete_files(*files):
    for f in files: 
        try:    nt.remove(f)
        except: pass
        
def get_parent_directory(path):
    pos = path[:-1].rfind(separator)
    return path[:pos]

def samefile(file1, file2):
    return fullpath(file1).lower() == fullpath(file2).lower()
    
def filecopy(oldpath, newpath):
    if samefile(oldpath, newpath):
        raise AssertionError, "%s and %s are same" % (oldpath, newpath)
        
    of, nf = None, None
    try: 
        of = file(oldpath, 'rb')
        nf = file(newpath, 'wb')
        while True:
            b = of.read(1024 * 16)
            if not b: 
                break
            nf.write(b)
    finally:
        if of: of.close()
        if nf: nf.close()

def ensure_future_present(path):
    futureFile = path_combine(path, "__future__.py")
    write_to_file(futureFile, 
    '''division=1
with_statement=1''')
        
def clean_directory(path):
    for f in nt.listdir(path):
        try: 
            nt.unlink(path_combine(path, f))
        except: 
            pass

def get_directory_name(file):
    file = fullpath(file)
    pos = file.rfind(separator)
    return file[:pos]
    
def find_peverify():
    if sys.platform <> 'cli': return None
    
    import System
    for d in System.Environment.GetEnvironmentVariable("PATH").split(';'):
        file = path_combine(d, "peverify.exe")
        if file_exists(file):
            return file

    print """
#################################################
#     peverify.exe not found. Test will fail.   #
#################################################
"""
    return None  


def delete_all_f(module_name):
    module = sys.modules[module_name]
    for x in dir(module):
        if x.startswith('_f_'):
            fn = getattr(module, x)
            if isinstance(fn, str):
                try:    nt.unlink(fn)
                except: pass