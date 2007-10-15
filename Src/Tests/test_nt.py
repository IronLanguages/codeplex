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

from lib.assert_util import *
skiptest("silverlight")
from lib.file_util import *
import _random
from exceptions import IOError

import nt


AreEqual(nt.environ.has_key('COMPUTERNAME') or nt.environ.has_key('computername'), True)

# mkdir,listdir,rmdir,getcwd
def test_mkdir():
    nt.mkdir('dir_create_test')
    AreEqual(nt.listdir(nt.getcwd()).count('dir_create_test'), 1)
    
    nt.rmdir('dir_create_test')
    AreEqual(nt.listdir(nt.getcwd()).count('dir_create_test'), 0)

@disabled("CodePlex Work Item 1216")    
def test_mkdir_negative():    
    nt.mkdir("dir_create_test")
    AssertError(WindowsError, nt.mkdir, "dir_create_test")
    #if it fails once...it should fail again
    AssertError(WindowsError, nt.mkdir, "dir_create_test")
    nt.rmdir('dir_create_test')
    nt.mkdir("dir_create_test")
    AssertError(WindowsError, nt.mkdir, "dir_create_test")
    nt.rmdir('dir_create_test')
    
# stat,lstat
def test_stat():
    # stat
    AssertError(nt.error, nt.stat, 'doesnotexist.txt')
        
    #lstat
    AssertError(nt.error, nt.lstat, 'doesnotexist.txt')    
 
    
# getcwdu test
def test_getcwdu():
    AreEqual(nt.getcwd(),nt.getcwdu())
    
    nt.mkdir('dir_create_test')
    AreEqual(nt.listdir(nt.getcwdu()).count('dir_create_test'), 1)
    nt.rmdir('dir_create_test')


# getpid test
def test_getpid():
    result = None
    result = nt.getpid()
    Assert(result>=0,
          "processPID should not be less than zero")
    
    result2 = nt.getpid()
    Assert(result2 == result,
           "The processPID in one process should be same")
 
 
# environ test      
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
 
    
# startfile
def test_startfile():
    AssertError(OSError, nt.startfile, "not_exist_file.txt")


# chdir tests
def test_chdir():
    currdir = nt.getcwd()
    nt.mkdir('tsd')
    nt.chdir('tsd')
    AreEqual(currdir+'\\tsd', nt.getcwd())
    nt.chdir(currdir)
    AreEqual(currdir, nt.getcwd())
    nt.rmdir('tsd')
    
    # the directory is empty or does not exist
    AssertError(OSError, lambda:nt.chdir(''))
    AssertError(OSError, lambda:nt.chdir('tsd'))
    

# fdopen tests
def test_fdopen():
    # fd = 0 
    # IronPython does not implement the nt.dup function
    if not is_cli:
        result = None
        result = nt.fdopen(nt.dup(0),"r",1024)
        Assert(result!=None,"1,The file object was not returned correctly") 
        
        result = None
        result = nt.fdopen(nt.dup(0),"w",2048)
        Assert(result!=None,"2,The file object was not returned correctly") 
        
        result = None
        result = nt.fdopen(nt.dup(0),"a",512)
        Assert(result!=None,"3,The file object was not returned correctly") 
        
        # fd = 1
        result = None
        result = nt.fdopen(nt.dup(1),"a",1024)
        Assert(result!=None,"4,The file object was not returned correctly") 
        
        result = None
        result = nt.fdopen(nt.dup(1),"r",2048)
        Assert(result!=None,"5,The file object was not returned correctly") 
        
        result = None
        result = nt.fdopen(nt.dup(1),"w",512)
        Assert(result!=None,"6,The file object was not returned correctly") 
        
        # fd = 2
        result = None
        result = nt.fdopen(nt.dup(2),"r",1024)
        Assert(result!=None,"7,The file object was not returned correctly") 
        
        result = None
        result = nt.fdopen(nt.dup(2),"a",2048)
        Assert(result!=None,"8,The file object was not returned correctly") 
        
        result = None
        result = nt.fdopen(nt.dup(2),"w",512)
        Assert(result!=None,"9,The file object was not returned correctly") 
    
        result.close()
         
    # The file descriptor is not valid  
    AssertError(OSError,nt.fdopen,3)
    AssertError(OSError,nt.fdopen,-1)
    AssertError(OSError,nt.fdopen,3, "w")
    AssertError(OSError,nt.fdopen,3, "w", 1024)
    
        
    # The file mode does not exist
    #CodePlex Work Item #8617
    # AssertError(ValueError,nt.fdopen,0,"p")
 
   
# fstat,unlink tests
def test_fstat():
    #CodePlex Work Item #8618
    #result = nt.fstat(1)
    #Assert(result!=0,"0,The file stat object was not returned correctly") 
    
    result = None
    tmpfile = "tmpfile1.tmp"
    f = open(tmpfile, "w")
    result = nt.fstat(f.fileno())
    Assert(result!=None,"0,The file stat object was not returned correctly") 
    f.close()
    nt.unlink(tmpfile)
   
    
    # invalid file descriptor
    AssertError(OSError,nt.fstat,3)
    AssertError(OSError,nt.fstat,-1)

# chmod tests:
# BUG 828,830
#nt.mkdir('tmp2')
#nt.chmod('tmp2', 256) # NOTE: change to flag when stat is implemented
#AssertError(IOError, lambda:nt.rmdir('tmp2'))
#nt.chmod('tmp2', 128)
#nt.rmdir('tmp2')
# /BUG


################################################################################################
# popen/popen2/popen3/unlink tests

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

    # write to a pipe
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

 
# utime tests   
def test_utime():
    f = file('temp_file_does_not_exist.txt', 'w')
    f.close()
    import nt
    x = nt.stat('.')
    nt.utime('temp_file_does_not_exist.txt', (x[7], x[8]))
    y = nt.stat('temp_file_does_not_exist.txt')
    AreEqual(x[7], y[7])
    AreEqual(x[8], y[8])
    nt.unlink('temp_file_does_not_exist.txt')
    

#Merlin Work Item 153306
def xtest_tempnam():
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

# BUG 8777,Should IronPython throw a warning when tmpnam is called ?
# tmpnam test
def test_tmpnam():
    str = nt.tmpnam()
    AreEqual(isinstance(str,type("string")),True)
    if is_cli:
        Assert(str.find(colon)!=-1,
               "1,the returned path is invalid")
        Assert(str.find(separator)!=-1,
               "2,the returned path is invalid")       


# times test        
def test_times():
    '''
    '''
    #simple sanity check
    utime, stime, zero1, zero2, zero3 = nt.times()
    Assert(utime>=0)
    Assert(stime>=0)
    AreEqual(zero1, 0)
    AreEqual(zero2, 0)
    #BUG - according to the specs this should be 0 for Windows
    #AreEqual(zero3, 0)
    

# putenv tests    
def test_putenv():
    '''
    '''
    #simple sanity check
    nt.putenv("IPY_TEST_ENV_VAR", "xyz")
       
    #ensure it really does what it claims to do
    Assert(not nt.environ.has_key("IPY_TEST_ENV_VAR"))
    
    #negative cases
    AssertError(TypeError, nt.putenv, None, "xyz")
    #BUG
    #AssertError(TypeError, nt.putenv, "ABC", None)
    AssertError(TypeError, nt.putenv, 1, "xyz")
    AssertError(TypeError, nt.putenv, "ABC", 1)
  

# unsetenv tests
def test_unsetenv():
    #CPython nt has no unsetenv function
    #simple sanity check
    if is_cli:
        nt.putenv("ipy_test_env_var", "xyz")
        nt.unsetenv("ipy_test_env_var_unset") 
        Assert(not nt.environ.has_key("ipy_test_env_var_unset"))
     

# remove tests
def test_remove():
    # remove an existing file
    handler = open("create_test_file.txt","w")
    handler.close()
    path1 = nt.getcwd()
    nt.remove(path1+'\\create_test_file.txt')
    AreEqual(nt.listdir(nt.getcwd()).count('create_test_file.txt'), 0)
    
    # BUG 8780, IP does not throw 
    #AssertError(OSError, nt.remove, path1+'\\create_test_file2.txt')
    
    # the path is a type other than string
    AssertError(TypeError, nt.remove, 1)
    AssertError(TypeError, nt.remove, True) 
    AssertError(TypeError, nt.remove, None)
  
# rename tests
def test_rename():
    # normal test
    handler = open("oldnamefile.txt","w")
    handler.close()
    str_old = "oldnamefile.txt"
    dst = "newnamefile.txt"
    nt.rename(str_old,dst)
    AreEqual(nt.listdir(nt.getcwd()).count(dst), 1)
    AreEqual(nt.listdir(nt.getcwd()).count(str_old), 0)
    nt.remove(dst)
    
    # the destination name is a directory
    handler = open("oldnamefile.txt","w")
    handler.close()
    str_old = "oldnamefile.txt"
    dst = "newnamefile.txt"
    nt.mkdir(dst)
    AssertError(OSError, nt.rename,str_old,dst)
    nt.rmdir(dst)
    nt.remove(str_old)
    
    # the dst already exists
    handler1 = open("oldnamefile.txt","w")
    handler1.close()
    handler2 = open("newnamefile.txt","w")
    handler2.close()
    str_old = "oldnamefile.txt"
    dst = "newnamefile.txt"
    AssertError(OSError, nt.rename,str_old,dst)
    nt.remove(str_old)
    nt.remove(dst)
    
    # the source file specified does not exist
    str_old = "oldnamefile.txt"
    dst = "newnamefile.txt"
    AssertError(OSError, nt.rename,str_old,dst)


# spawnle tests    
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


# spawnl tests
def test_spawnl():
    if is_cli == False:
        return
    
    #sanity check
    #CPython nt has no spawnl function 
    pint_cmd = ping_cmd = nt.environ["windir"] + "\system32\ping.exe" 
    nt.spawnl(nt.P_WAIT, ping_cmd , "ping","127.0.0.1")   
    nt.spawnl(nt.P_WAIT, ping_cmd , "ping","/?")     
    nt.spawnl(nt.P_WAIT, ping_cmd , "ping")     
    
    # negative case
    cmd = pint_cmd+"oo"
    AssertError(OSError,nt.spawnl,nt.P_WAIT,cmd,"ping","/?")


# spawnve tests
def test_spawnv():
    #sanity check
    ping_cmd = nt.environ["windir"] + "\system32\ping" 
    nt.spawnv(nt.P_WAIT, ping_cmd , ["ping"])  
    nt.spawnv(nt.P_WAIT, ping_cmd , ["ping","127.0.0.1"])  
    nt.spawnv(nt.P_WAIT, ping_cmd, ["ping", "-n", "5", "-w", "5000", "127.0.0.1"])
    
        
# spawnve tests    
def test_spawnve():
    '''
    '''
    ping_cmd = nt.environ["windir"] + "\system32\ping" 
    
    #simple sanity checks
    nt.spawnve(nt.P_WAIT, ping_cmd, ["ping", "/?"], {})
    nt.spawnve(nt.P_WAIT, ping_cmd, ["ping", "127.0.0.1"], {})
    nt.spawnve(nt.P_WAIT, ping_cmd, ["ping", "-n", "6", "-w", "1000", "127.0.0.1"], {})
    
    #negative cases
    AssertError(TypeError, nt.spawnve, nt.P_WAIT, ping_cmd , ["ping", "/?"], None)
    AssertError(TypeError, nt.spawnve, nt.P_WAIT, ping_cmd , ["ping", "/?"], {1: "xyz"})
    AssertError(TypeError, nt.spawnve, nt.P_WAIT, ping_cmd , ["ping", "/?"], {"abc": 1})
    
    
# tmpfile tests    
#for some strange reason this fails on some Vista machines with an OSError related
#to permissions problems
@skip("win32")  
def test_tmpfile():
    '''
    '''
    #sanity check
    joe = nt.tmpfile()
    AreEqual(type(joe), file)
    joe.close()


# waitpid tests    
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


# stat_result test
def test_stat_result():
    #sanity check
    statResult = [0,1,2,3,4,5,6,7,8,9]
    object = None
    object = nt.stat_result(statResult)
    Assert(object != None,
           "The class did not return an object instance")
    AreEqual(object.st_uid,4)
    AreEqual(object.st_gid,5)
    AreEqual(object.st_nlink,3)
    AreEqual(object.st_dev,2)
    AreEqual(object.st_ino,1)
    AreEqual(object.st_mode,0)
    AreEqual(object.st_atime,7)
    AreEqual(object.st_mtime,8)
    AreEqual(object.st_ctime,9)
    
    #negative tests
    statResult = [0,1,2,3,4,5,6,7,8,]
    AssertError(TypeError,nt.stat_result,statResult)
    
    # BUG 8755,the length of the sequence is more than 10
    # statResult = ["a","b","c","y","r","a","a","b","d","r","fu"]
    # AssertError(TypeError,nt.stat_result,statResult)


# urandom tests
def test_urandom():
    # argument n is a random int
    rand = _random.Random()
    n = rand.getrandbits(16)
    str = nt.urandom(n)
    result = len(str)
    AreEqual(isinstance(str,type("string")),True)
    AreEqual(n,result)


# write/read tests    
def test_write():
    # write the file
    tempfilename = "temp.txt"
    file = open(tempfilename,"w")
    nt.write(file.fileno(),"Hello,here is the value of test string")
    file.close()
    
    # read from the file
    file =   open(tempfilename,"r") 
    str = nt.read(file.fileno(),100)
    AreEqual(str,"Hello,here is the value of test string")
    file.close()
    nt.unlink(tempfilename)
    
    # BUG 8783 the argument buffersize in nt.read(fd, buffersize) is less than zero
    # the string written to the file is empty string
    #tempfilename = "temp.txt"
    #file = open(tempfilename,"w")
    #nt.write(file.fileno(),"bug test")
    #file.close()
    #file = open(tempfilename,"r")
    #AssertError(OSError,nt.read,file.fileno(),-10)
    #file.close()
    #nt.unlink(tempfilename)

# open test   
def test_open():
    # BUG 8784
    # sanity test
    #tempfilename = "temp.txt"
    #fd = nt.open(tempfilename,256,1)    
    pass     

def test_system_minimal():
    if sys.platform=="win32":
        Assert(hasattr(nt, "system"))
    else:
        print "CodePlex Work Item 2982"
        Assert(not hasattr(nt, "system"), "Please modify test_system_minimal now that nt.system has been implemented")

# flags test
def test_flags():
    AreEqual(nt.P_WAIT,0)
    AreEqual(nt.P_NOWAIT,1)
    AreEqual(nt.P_NOWAITO,3)
    AreEqual(nt.O_APPEND,8)
    AreEqual(nt.O_CREAT,256)
    AreEqual(nt.O_TRUNC,512)
    AreEqual(nt.O_EXCL,1024)
    AreEqual(nt.O_NOINHERIT,128)
    AreEqual(nt.O_RANDOM,16)
    AreEqual(nt.O_SEQUENTIAL,32)
    AreEqual(nt.O_SHORT_LIVED,4096)
    AreEqual(nt.O_TEMPORARY,64)
    AreEqual(nt.O_WRONLY,1)
    AreEqual(nt.O_RDONLY,0)
    AreEqual(nt.O_RDWR,2)
    AreEqual(nt.O_BINARY,32768) 
    AreEqual(nt.O_TEXT,16384) 
              
try:
    run_test(__name__)

finally:
    #test cleanup - the test functions create the following directories and if any of them
    #fail, the directories may not necessarily be removed.  for this reason we try to remove
    #them again
    for temp_dir in ['dir_create_test', 'tsd', 'tmp2', 'newnamefile.txt']:
        try:
            nt.rmdir(temp_dir)
        except:
            pass
