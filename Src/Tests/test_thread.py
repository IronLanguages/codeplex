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

from iptest.assert_util import *
skiptest("win32")

from System import *
from System.Threading import *

def test_thread():
    
    class Sync:
        hit = 0
    
    def ThreadProcParm(parm):
        parm.hit = 1
    
    def ThreadProcNoParm():
        pass
    
    def Main():
        if not is_silverlight:
            sync = Sync()
            t = Thread(ParameterizedThreadStart(ThreadProcParm))
            t.Start(sync)
            t.Join()
            Assert(sync.hit == 1)
    
        t = Thread(ThreadStart(ThreadProcNoParm))
        t.Start()
        t.Join()
    
    Main()
    
    
    def import_sys():
        import sys
        Assert(sys != None)
    
    t = Thread(ThreadStart(import_sys))
    t.Start()
    
    t.Join()
    
    so = sys.stdout
    se = sys.stderr
    class myStdOut:
        def write(self, text): pass
    
    
    sys.stdout = myStdOut()
    sys.stderr = myStdOut()
    
    import thread
    
    def raises(*p):
        raise Exception
    
    id = thread.start_new_thread(raises, ())
    Thread.Sleep(1000)  # wait a bit and make sure we don't get ripped.
    
    sys.stdout = so
    sys.stderr = se

def test_stack_size():
    import sys
    if is_cli or (sys.version_info[0] == 2 and sys.version_info[1] > 4) or sys.version_info[0] > 2:
        import thread
        
        size = thread.stack_size()
        Assert(size==0 or size>=32768)

        bad_size_list = [ 1, -1, -32768, -32769, -32767, -40000, 32767, 32766]
        for bad_size in bad_size_list:
            AssertError(ValueError, thread.stack_size, bad_size)
            
        good_size_list = [4096*10, 4096*100, 4096*1000, 4096*10000]
        for good_size in good_size_list:
            #CodePlex Work Item 7827
            if (is_cli or is_silverlight) and good_size<=50000: print "Ignoring", good_size, "for CLI"; continue
            temp = thread.stack_size(good_size)
            Assert(temp>=32768 or temp==0)
        
        def temp(): pass
        thread.start_new_thread(temp, ())
        temp = thread.stack_size(1024*1024)
        Assert(temp>=32768 or temp==0)

def test_new_thread_is_background():
    """verify new threads created during Python are background threads"""
    import thread
    global done
    done = None
    def f():
        global done
        done = Thread.CurrentThread.IsBackground
    thread.start_new_thread(f, ())
    while done == None:
        Thread.Sleep(1000)
    Assert(done)

def test_thread_local():
    import thread
    x = thread._local()
    x.foo = 42
    AreEqual(x.foo, 42)
    
    global found
    found = None
    def f():
        global found
        found = hasattr(x, 'foo')
        
    thread.start_new_thread(f, ())

    while found == None:
        Thread.Sleep(1000)

    Assert(not found)

run_test(__name__)
