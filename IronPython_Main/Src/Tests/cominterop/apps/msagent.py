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

# MSAgent COM Interop tests

from lib.assert_util import skiptest
skiptest("win32", "silverlight", "cli64")
from lib.cominterop_util import *

if not file_exists(agentsvr_path):
    from sys import exit
    print "Cannot test AgentServerObjects.dll when it doesn't exist."
    exit(0)

if not file_exists_in_path("tlbimp.exe"):
    from sys import exit
    print "tlbimp.exe is not in the path!"
    exit(1)
else:
    import clr
    run_tlbimp(agentsvr_path)
    Assert(file_exists("AgentServerObjects.dll"))
    clr.AddReference("AgentServerObjects.dll")
    from AgentServerObjects import * 
#------------------------------------------------------------------------------
#--GLOBALS
com_obj = AgentServerClass()

#------------------------------------------------------------------------------
#--TESTS
def test_merlin():
    from time import sleep
    
    a = AgentServerClass()    
    Assert('Equals' in dir(a))
    cid = com_obj.Load('Merlin.acs')[0]

    c = com_obj.GetCharacter(cid)
    sleep(1)
    
    if is_snap or testpath.basePyDir.lower()=='src':
        c.Show(0)
        sleep(1)
        while c.GetVisible()==0:
            sleep(1)
            
    c.Think('IronPython...')
    c.Play('Read')
    c.GestureAt(True, False)
    c.GestureAt(100, 200)
    if not preferComDispatch:
        AssertError(OverflowError, c.GestureAt, 65537.34, 32) # Cannot convert float(11.34) to Int16

    c.Speak('hello world', None)

    c.StopAll(0)
    c.Hide(0)
    sleep(1)
    com_obj.Unload(cid)
        
    delete_files("AgentServerObjects.dll")
    
#------------------------------------------------------------------------------
run_com_test(__name__, __file__)