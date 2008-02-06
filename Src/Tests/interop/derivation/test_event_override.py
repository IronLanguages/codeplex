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

add_clr_assemblies("baseclasscs", "typesamples")

from Merlin.Testing import *
from Merlin.Testing.TypeSample import *
from Merlin.Testing.BaseClass import *

# no way to continue???

def xtest_simple():
    class C(IEvent10):
        def __init__(self):
            self.act = None
        def add_Act(self, value):
            self.act = System.Delegate.Combine(self.act, value);
        def remove_Act(self, value):
            self.act = System.Delegate.Remove(self.act, value);

        def call(self):
            self.act(1)
    
    x = C()
    def f(x, y): print x, y
    x.add_Act(f)
    x.call()

run_test(__name__)
