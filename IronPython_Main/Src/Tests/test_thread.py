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
    if (sys.version_info[0] == 2 and sys.version_info[1] > 4) or sys.version_info[0] > 2:
        import thread
        AreEqual(thread.stack_size(512*1024), 512*1024)
        def temp(): pass
        thread.start_new_thread(temp, ())
        AreEqual(thread.stack_size(1024*1024), 1024*1024)

run_test(__name__)
