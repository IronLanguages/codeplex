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

import nt

nt.mkdir('dir_create_test')
AreEqual(nt.listdir(nt.getcwd()).count('dir_create_test'), 1)

nt.rmdir('dir_create_test')
AreEqual(nt.listdir(nt.getcwd()).count('dir_create_test'), 0)

AreEqual(nt.environ['COMPUTERNAME'] != None, True)

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
   
test_environ()

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

# open a pipe just for reading...
x = nt.popen('ping 127.0.0.1', 'r')
text = x.read()
Assert(text.lower().index('pinging') != -1)
AreEqual(x.close(), None)

# bug 1146
#x = nt.popen('sort', 'w')
#x.write('hello\nabc\n')
#AreEqual(x.close(), None)


# once w/ default mode
# once w/ no mode specified

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
    


# verify that nt.stat reports times in seconds, not ticks...

import time
tmpfile = 'tmpfile.tmp'
f = open(tmpfile, 'w')
f.close()
t = time.time()

mt = nt.stat(tmpfile).st_mtime

nt.unlink(tmpfile)

Assert(abs(t-mt) < 60)


import nt
nt.chmod('tmpfile.tmp', 256)
nt.chmod('tmpfile.tmp', 128)
nt.unlink('tmpfile.tmp')


