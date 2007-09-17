#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################
    
import sys, nt

def environ_var(key): return [nt.environ[x] for x in nt.environ.keys() if x.lower() == key.lower()][0]

merlin_root = environ_var("MERLIN_ROOT")
sys.path.extend([merlin_root + r"\Languages\IronPython\Tests", merlin_root + r"\Test\ClrAssembly\bin"])

from lib.assert_util import *
skiptest("silverlight")

import clr
clr.AddReference("fieldtests", "typesamples")

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
