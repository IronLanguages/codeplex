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
add_clr_assemblies("indexerdefinitionsvb", "typesamples")

from Merlin.Testing import *
from Merlin.Testing.Indexer import *
from Merlin.Testing.TypeSample import *

import System
array = System.Array[object]

def test_basic():
    for t in [
                ClassWithIndexer, 
                #StructWithIndexer,
             ]:
        x = t()
        x.Init()

        AssertError(TypeError, lambda: x[2])
        
        x.PropertyName[2] = 9
        AreEqual(x.PropertyName[2], 9)
        
        i = x.PropertyName
        i.SetValue(array([3]), 10)
        AreEqual(i.GetValue(array([3])), 10)
        
        i = t.PropertyName                         # bug 363422
        #i.SetValue(x, array([4]), 11)
        #AreEqual(i.GetValue(x, array([4])), 11)
    
def test_signature():
    x = ClassWithSignature()
    i = x.PropertyName
    i[3] = 10
    AreEqual(10, i[3])
    AreEqual(10, i[1, 4])
    
def test_only_optional():
    x = ClassWithOnlyOptional()
    i = x.PropertyName
    #i[()]  # bug 363440
    
def test_only_paramarray():
    x = ClassWithOnlyParamArray()
    i = x.PropertyName
    
    AreEqual(i[()], -99)
    i[()] = 10
    AreEqual(i[()], 10)
    
    i[1] = 4
    AreEqual(i[1], 4)
    AreEqual(i[1, 4, 5, 7], 4)
    
def test_static_indexer():
    t = ClassWithStaticIndexer
    x = t()
    
    i = ClassWithStaticIndexer.PropertyName
    i[1] = 10
    AreEqual(i[100], 111)
    
    i.SetValue(array([2]), 20)
    AreEqual(i.GetValue(array([200])), 222)


run_test(__name__)
