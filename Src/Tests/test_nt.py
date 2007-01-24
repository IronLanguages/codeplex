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

from lib.assert_util import *
from lib.file_util import *

import nt

nt.mkdir('dir_create_test')
AreEqual(nt.listdir(nt.getcwd()).count('dir_create_test'), 1)

nt.rmdir('dir_create_test')
AreEqual(nt.listdir(nt.getcwd()).count('dir_create_test'), 0)

AreEqual(nt.environ.has_key('COMPUTERNAME') or nt.environ.has_key('computername'), True)

AssertError(nt.error, nt.stat, 'doesnotexist.txt')

def test_environ():
    non_exist_key      = "_NOT_EXIST_"
    iron_python_string = "Iron_pythoN"

    try:
        nt.environ[non_exist_key]
        raise AssertionError
    except KeyError:
        pass

    # set
    nt.environ[non_exist_key] = iron_python_string
    AreEqual(nt.environ[non_exist_key], iron_python_string)
    
    import sys
    if is_cli:
        import System
        AreEqual(System.Environment.GetEnvironmentVariable(non_exist_key), iron_python_string)
    
    # update again    
    swapped = iron_python_string.swapcase()
    nt.environ[non_exist_key] = swapped
    AreEqual(nt.environ[non_exist_key], swapped)
    if is_cli:
        AreEqual(System.Environment.GetEnvironmentVariable(non_exist_key), swapped)
        
    # remove 
    del nt.environ[non_exist_key]
    if is_cli :
        AreEqual(System.Environment.GetEnvironmentVariable(non_exist_key), None)
    
    AreEqual(type(nt.environ), type({}))
    

AssertError(WindowsError, nt.startfile, "not_exist_file.txt")

currdir = nt.getcwd()
nt.mkdir('tsd')
nt.chdir('tsd')
AreEqual(currdir+'\\tsd', nt.getcwd())
nt.chdir(currdir)
AreEqual(currdir, nt.getcwd())
nt.rmdir('tsd')
AssertError(OSError, lambda:nt.chdir(''))
AssertError(OSError, lambda:nt.chdir('tsd'))

# chmod tests:
# BUG 828,830
#nt.mkdir('tmp2')
#nt.chmod('tmp2', 256) # NOTE: change to flag when stat is implemented
#AssertError(IOError, lambda:nt.rmdir('tmp2'))
#nt.chmod('tmp2', 128)
#nt.rmdir('tmp2')
# /BUG


################################################################################################
# popen/popen2/popen3 tests

def test_popen():
    # open a pipe just for reading...
    pipe_modes = [["ping 127.0.0.1", "r"],
                  ["ping 127.0.0.1"]]
    if is_cli:
        pipe_modes.append(["ping 127.0.0.1", ""])
        
    for args in pipe_modes:
        x = nt.popen(*args)
        text = x.read()
        Assert(text.lower().index('pinging') != -1)
        AreEqual(x.close(), None)

    #write to a pipe
    x = nt.popen('sort', 'w')
    x.write('hello\nabc\n')
    x.close()

    # bug 1146
    #x = nt.popen('sort', 'w')
    #x.write('hello\nabc\n')
    #AreEqual(x.close(), None)

    # once w/ default mode
    AssertError(ValueError, nt.popen, "ping 127.0.0.1", "a")


# once w/ no mode
stdin, stdout = nt.popen2('sort')
stdin.write('hello\nabc\n')
AreEqual(stdin.close(), None)
AreEqual(stdout.read(), 'abc\nhello\n')
AreEqual(stdout.close(), None)

# bug 1146
# and once w/ each mode
#for mode in ['b', 't']:
#    stdin, stdout = nt.popen2('sort', mode)
#    stdin.write('hello\nabc\n')
#    AreEqual(stdin.close(), None)
#    AreEqual(stdout.read(), 'abc\nhello\n')
#    AreEqual(stdout.close(), None)
    

# popen3: once w/ no mode
stdin, stdout, stderr = nt.popen3('sort')
stdin.write('hello\nabc\n')
AreEqual(stdin.close(), None)
AreEqual(stdout.read(), 'abc\nhello\n')
AreEqual(stdout.close(), None)
AreEqual(stderr.read(), '')
AreEqual(stderr.close(), None)

# bug 1146
# popen3: and once w/ each mode
#for mode in ['b', 't']:
#    stdin, stdout, stderr = nt.popen3('sort', mode)
#    stdin.write('hello\nabc\n')
#    AreEqual(stdin.close(), None)
#    AreEqual(stdout.read(), 'abc\nhello\n')
#    AreEqual(stdout.close(), None)
#    AreEqual(stderr.read(), '')
#    AreEqual(stderr.close(), None)
    
tmpfile = 'tmpfile.tmp'
f = open(tmpfile, 'w')
f.close()
nt.unlink(tmpfile)
try:
    nt.chmod('tmpfile.tmp', 256)
except Exception:
    pass #should throw when trying to access file deleted by unlink
else:
    Assert(False,"Error! Trying to access file deleted by unlink should have thrown.")

try:
    tmpfile = "tmpfile2.tmp"
    f = open(tmpfile, "w")
    f.write("testing chmod")
    f.close()
    nt.chmod(tmpfile, 256)
    AssertError(OSError, nt.unlink, tmpfile)
    nt.chmod(tmpfile, 128)
    nt.unlink(tmpfile)
    AssertError(IOError, file, tmpfile)
finally:
    try:
        nt.chmod(tmpfile, 128)
        nt.unlink(tmpfile)
    except:
        print "exc"

# verify that nt.stat reports times in seconds, not ticks...

import time
tmpfile = 'tmpfile.tmp'
f = open(tmpfile, 'w')
f.close()
t = time.time()
mt = nt.stat(tmpfile).st_mtime
nt.unlink(tmpfile) # this deletes the file
Assert(abs(t-mt) < 60)

tmpfile = 'tmpfile.tmp' # need to open it again since we deleted it with 'unlink'
f = open(tmpfile, 'w')
f.close()
nt.chmod('tmpfile.tmp', 256)
nt.chmod('tmpfile.tmp', 128)
nt.unlink('tmpfile.tmp')


def test_utime():
    f = file('temp_file_does_not_exist.txt', 'w')
    f.close()
    import nt
    x = nt.stat('.')
    nt.utime('temp_file_does_not_exist.txt', (x[7], x[8]))
    y = nt.stat('temp_file_does_not_exist.txt')
    AreEqual(x[7], y[7])
    AreEqual(x[8], y[8])

def test_tempnam():
    '''
    '''
    #sanity checks
    AreEqual(type(nt.tempnam()), str)
    AreEqual(type(nt.tempnam("garbage name should still work")), str)
    #BUG - "" does not work
    #AreEqual(type(nt.tempnam("", "pre")), str)
    
    #Very basic case
    joe = nt.tempnam()
    last_dir = joe.rfind("\\")
    temp_dir = joe[:last_dir+1]
    Assert(directory_exists(temp_dir))
    Assert(not file_exists(joe))
    
    #Basic case where we give it an existing directory and ensure
    #it uses that directory
    joe = nt.tempnam(get_temp_dir())
    last_dir = joe.rfind("\\")
    temp_dir = joe[:last_dir+1]
    Assert(directory_exists(temp_dir))
    Assert(not file_exists(joe))
    # The next line is not guaranteed to be true in some scenarios. 
    #AreEqual(nt.stat(temp_dir.strip("\\")), nt.stat(get_temp_dir()))
    
    #few random prefixes
    prefix_names = ["", "a", "1", "_", ".", "sillyprefix", 
                    "                                ", 
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                    ]
    #test a few directory names that shouldn't really work
    dir_names = ["b", "2", "_", ".", "anotherprefix", 
                 "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
                 None]
    
    for dir_name in dir_names:
        #just try the directory name on it's own
        joe = nt.tempnam(dir_name)
        last_dir = joe.rfind("\\")
        temp_dir = joe[:last_dir+1]
        Assert(directory_exists(temp_dir))
        Assert(not file_exists(joe))
        Assert(temp_dir != dir_name)
            
        #now try every prefix
        for prefix_name in prefix_names:
            joe = nt.tempnam(dir_name, prefix_name)
            last_dir = joe.rfind("\\")
            temp_dir = joe[:last_dir+1]
            file_name = joe[last_dir+1:]
            Assert(directory_exists(temp_dir))
            Assert(not file_exists(joe))
            Assert(temp_dir != dir_name)
            Assert(file_name.startswith(prefix_name))
        
def test_times():
    '''
    '''
    #simple sanity check
    utime, stime, zero1, zero2, zero3 = nt.times()
    Assert(utime>0)
    Assert(stime>0)
    AreEqual(zero1, 0)
    AreEqual(zero2, 0)
    #BUG - according to the specs this should be 0 for Windows
    #AreEqual(zero3, 0)
    
    
def test_putenv():
    '''
    '''
    #simple sanity check
    nt.putenv("IPY_TEST_ENV_VAR", "xyz")
       
    #ensure it really does what it claims to do
    #TODO
    
    #negative cases
    AssertError(TypeError, nt.putenv, None, "xyz")
    #BUG
    #AssertError(TypeError, nt.putenv, "ABC", None)
    AssertError(TypeError, nt.putenv, 1, "xyz")
    AssertError(TypeError, nt.putenv, "ABC", 1)
    
def test_spawnle():
    '''
    '''
    #BUG?
    #CPython nt has no spawnle function
    if is_cli == False:
        return
    
    ping_cmd = nt.environ["windir"] + "\system32\ping" 
    
    #simple sanity check
    nt.spawnle(nt.P_WAIT, ping_cmd , "ping", "/?", {})
    #BUG - the first parameter of spawnle should be "ping"
    #nt.spawnle(nt.P_WAIT, ping_cmd , "ping", "127.0.0.1", {})
    #BUG - even taking "ping" out, multiple args do not work
    #pid = nt.spawnle(nt.P_NOWAIT, ping_cmd ,  "-n", "15", "-w", "1000", "127.0.0.1", {})
    
    #negative cases
    AssertError(TypeError, nt.spawnle, nt.P_WAIT, ping_cmd , "ping", "/?", None)
    AssertError(TypeError, nt.spawnle, nt.P_WAIT, ping_cmd , "ping", "/?", {1: "xyz"})
    AssertError(TypeError, nt.spawnle, nt.P_WAIT, ping_cmd , "ping", "/?", {"abc": 1})
    
def test_tmpfile():
    '''
    '''
    #sanity check
    joe = nt.tmpfile()
    AreEqual(type(joe), file)
    joe.close()
    
def test_waitpid():
    '''
    '''
    #sanity check    
    #the usage of spawnle is a bug in this case that should be fixed in IP, 
    #but since this test is for waitpid it's basically OK
    #ping_cmd = nt.environ["windir"] + "\system32\ping" 
    #pid = nt.spawnle(nt.P_NOWAIT, ping_cmd ,  "-n", "5", "-w", "1000", "127.0.0.1", {})
    #new_pid, exit_stat = nt.waitpid(pid, 0)
    
    #negative cases
    #BUG - should be an OSError instead of a ValueError
    #AssertError(OSError, nt.waitpid, -1234, 0)
    AssertError(TypeError, nt.waitpid, "", 0)
    
def test_spawnve():
    '''
    '''
    ping_cmd = nt.environ["windir"] + "\system32\ping" 
    
    #simple sanity checks
    nt.spawnve(nt.P_WAIT, ping_cmd, ["ping", "/?"], {})
    nt.spawnve(nt.P_WAIT, ping_cmd, ["ping", "127.0.0.1"], {})
    nt.spawnve(nt.P_WAIT, ping_cmd, ["ping", "-n", "15", "-w", "1000", "127.0.0.1"], {})
    
    #negative cases
    AssertError(TypeError, nt.spawnve, nt.P_WAIT, ping_cmd , ["ping", "/?"], None)
    AssertError(TypeError, nt.spawnve, nt.P_WAIT, ping_cmd , ["ping", "/?"], {1: "xyz"})
    AssertError(TypeError, nt.spawnve, nt.P_WAIT, ping_cmd , ["ping", "/?"], {"abc": 1})
    
run_test(__name__)