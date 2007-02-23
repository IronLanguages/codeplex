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

import gc

#CodePlex Work Item# 8202
#if gc.get_debug()!=0:
#    raise "Failed - get_debug should return 0 if set_debug has not been used"

from lib.assert_util import *

def test_garbage():
    Assert(type(gc.garbage)==list)

#get_objects
def test_get_objects():
    if is_cli:
        AssertError(NotImplementedError, gc.get_objects)
    else:
        gc.get_objects()

#get_threshold, set_threshold
def test_set_threshold():
    #the method has three arguments
    gc.set_threshold(3,-2,2)
    result = gc.get_threshold()
    AreEqual(result[0],3)
    AreEqual(result[1],-2)
    AreEqual(result[2],2)
    
    #the method has two argument
    gc.set_threshold(0,-100)
    result = gc.get_threshold()
    AreEqual(result[0],0)
    AreEqual(result[1],-100)
    
    #the method has only one argument
    gc.set_threshold(-10009)
    result= gc.get_threshold()
    AreEqual(result[0],-10009)


#get_referrers
def test_get_referrers():
    if is_cli:
        AssertError(NotImplementedError, gc.get_referrers,1,"hello",True)
        AssertError(NotImplementedError, gc.get_referrers)
    else:
        gc.get_referrers(1,"hello",True)
        gc.get_referrers() 
        
        class TempClass: pass
        tc = TempClass()
        AreEqual(gc.get_referrers(TempClass).count(tc), 1)
    
    
#get_referents
def test_get_referents():
    if is_cli:
        AssertError(NotImplementedError, gc.get_referents,1,"hello",True)
        AssertError(NotImplementedError, gc.get_referents)
    else:
        gc.get_referents(1,"hello",True)
        gc.get_referents()  
        
        class TempClass: pass
        AreEqual(gc.get_referents(TempClass).count('TempClass'), 1)
    
def test_enable():
    gc.enable()
    result = gc.isenabled()
    Assert(result,"enable Method can't set gc.isenabled to true.")

def test_disable():
    if is_cli:
        AssertError(NotImplementedError, gc.disable)
    else:
        gc.disable()
        result = gc.isenabled()
        Assert(result == False,"enable Method can't set gc.isenabled to false.")
    
def test_isenabled():
    gc.enable()
    result = gc.isenabled()
    Assert(result,"enable Method can't set gc.isenabled as true.")
    
    if not is_cli:
        gc.disable()
        result = gc.isenabled()
        Assert(result == False,"enable Method can't set gc.isenabled as false.")
    
def test_collect():
    i = gc.collect()
    AreEqual(i,0)
    
    
def test_set_debug():
    if is_cli:
        AssertError(NotImplementedError, gc.set_debug,gc.DEBUG_STATS)
        AssertError(NotImplementedError, gc.set_debug,gc.DEBUG_SAVEALL)
        AssertError(NotImplementedError, gc.set_debug,gc.DEBUG_INSTANCES)
    else:
        gc.set_debug(gc.DEBUG_STATS)
        gc.set_debug(gc.DEBUG_SAVEALL)
        gc.set_debug(gc.DEBUG_INSTANCES) 
        
@skip("cli")
def test_get_debug():
    state = [0,gc.DEBUG_STATS,gc.DEBUG_COLLECTABLE,gc.DEBUG_UNCOLLECTABLE,gc.DEBUG_INSTANCES,gc.DEBUG_OBJECTS,gc.DEBUG_SAVEALL,gc.DEBUG_LEAK]
    result = gc.get_debug()
    if result not in state:
        Fail("Returned value of getdebug method is not invalid value")  
    
run_test(__name__)
