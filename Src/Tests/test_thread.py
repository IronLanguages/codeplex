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

from lib.assert_util import *
if is_cli:
    from System import *
    from System.Threading import *
    
    class Sync:
        hit = 0
    
    def ThreadProcParm(parm):
        parm.hit = 1
    
    def ThreadProcNoParm():
        pass
    
    def Main():
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
            if is_cli and good_size<=50000: print "Ignoring", good_size, "for CLI"; continue
            temp = thread.stack_size(good_size)
            Assert(temp>=32768 or temp==0)
        
        def temp(): pass
        thread.start_new_thread(temp, ())
        temp = thread.stack_size(1024*1024)
        Assert(temp>=32768 or temp==0)

run_test(__name__)
