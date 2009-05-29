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
'''
#------------------------------------------------------------------------------
from iptest.assert_util import *

import clr
from System.Reflection import Assembly
from System.Reflection.Emit import AssemblyBuilder
    
def test_assembly_instance():
    mscorlib = clr.LoadAssemblyByName("mscorlib")
        
    #GetMemberNames
    Assert(len(dir(mscorlib)), 64)
    for x in ["System", "Microsoft"]:
        Assert( x in dir(mscorlib), "dir(mscorlib) does not have %s" % x)
    
    #GetBoundMember
    AreEqual(mscorlib.System.Int32(42), 42)    
    AssertError(AttributeError, lambda: mscorlib.NonExistentNamespace)

def test_assemblybuilder_instance():    
    name = System.Reflection.AssemblyName()
    name.Name = 'Test'
    assemblyBuilder = System.AppDomain.CurrentDomain.DefineDynamicAssembly(name, System.Reflection.Emit.AssemblyBuilderAccess.Run)    
    
    asm_builder_dir = dir(assemblyBuilder)
    AreEqual(len(asm_builder_dir), 77)
    Assert("AddResourceFile" in asm_builder_dir)
    Assert("CreateInstance" in asm_builder_dir)
    
def test_type():
    mscorlib = Assembly.Load("mscorlib")
    Assert("Assembly" in repr(mscorlib))  

    AreEqual(len(dir(Assembly)), 64)
    AreEqual(len(dir(AssemblyBuilder)), 77)   
    
#####################################################################################
run_test(__name__)
#####################################################################################
