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
skiptest("silverlight")

add_clr_assemblies("typesamples")

from Merlin.Testing import *
from Merlin.Testing.TypeSample import *

from System import Array 

def test_creation():
    Array(int, [1,2])    
    pass
    
run_test(__name__)
