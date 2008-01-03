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

add_clr_assemblies("fieldtests", "typesamples")

from Merlin.Testing.FieldTest import *
from Merlin.Testing.TypeSample import *

def test_accessibility():
    o = Misc()
    o.Set()
    AreEqual(o.PublicField, 100)
    AreEqual(o.ProtectedField, 200)
    AssertErrorWithMatch(AttributeError, "'Misc' object has no attribute 'PrivateField'", lambda: o.PrivateField)
    AreEqual(o.InterfaceField.Flag, 500)
    
    o = DerivedMisc()
    o.Set()
    AreEqual(o.PublicField, 400)
    AreEqual(o.ProtectedField, 200)

run_test(__name__)
