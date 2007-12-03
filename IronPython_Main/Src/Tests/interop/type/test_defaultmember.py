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
    
from lib.assert_util import *
skiptest("silverlight")

add_clr_assemblies("defaultmembersvb", "defaultmemberscs", "typesamples")

from Merlin.Testing import *
from Merlin.Testing.TypeSample import *
from Merlin.Testing.DefaultMemberSample import *

def test_indexer_not_named_as_item():
    x = ClassWithOverloadDefaultIndexer()
   
    for i in range(3):  
        #x[i] = 2 * i
        #AreEqual(x[i], 2 * i)
    
        x.MyProperty[i] = 3 * i
        AreEqual(x.MyProperty[i], 3 * i)
    
    for i in range(2, 4):
        for j in range(6, 9):
            a = i + j

            #x[i, j] = a
            #AreEqual(a, x[i, j])
            
            #x.MyProperty[i, j] = a * 2
            #AreEqual(x.MyProperty[i, j], a * 2)
            
    x = StructWithDefaultIndexer()
    x.Init()
    #x[1] = 1
    #print x.MyProperty[0]
    #x.MyProperty[1] = 1

def test_indexer_not_existing():
    x = ClassWithNotExisting()
    # x[1] 
    # x[2] = 2
    
def test_special_item():
    x = ClassWithItem()
    AssertError(TypeError, lambda: x[1])
    x.Item = 2
    AreEqual(x.Item, 2)

    x = ClassWithset_Item()
    x[10] = 20
    
run_test(__name__)

