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
    
import sys, nt

def environ_var(key): return [nt.environ[x] for x in nt.environ.keys() if x.lower() == key.lower()][0]

merlin_root = environ_var("MERLIN_ROOT")
sys.path.insert(0, merlin_root + r"\Languages\IronPython\Tests")
sys.path.insert(0, merlin_root + r"\Test\ClrAssembly\bin")

from lib.assert_util import *
skiptest("silverlight")

import clr
clr.AddReference("fieldtests", "typesamples")

from lib.file_util import *
peverify_dependency = [merlin_root + r"\Test\ClrAssembly\bin\typesamples.dll", merlin_root + r"\Test\ClrAssembly\bin\fieldtests.dll"]
copy_dlls_for_peverify(peverify_dependency)

from Merlin.Testing.TypeSample import *

def test_get_set():
    o = EnumInt32()
    AreEqual(o.A, EnumInt32.A)
   
    desc = EnumInt32.__dict__['B']
    AreEqual(EnumInt32.B, desc.__get__(o, EnumInt32))
    AreEqual(EnumInt32.B, desc.__get__(None, EnumInt32))
    
    def f(): o.A = 10
    AssertErrorWithMatch(AttributeError, "attribute 'A' of 'EnumInt32' object is read-only", f)
    
    def f(): EnumInt32.B = 10
    AssertErrorWithMatch(AttributeError, "Cannot set field B on type EnumInt32", f)

    def f(): EnumInt32.B = EnumInt32.A
    AssertErrorWithMatch(AttributeError, "Cannot set field B on type EnumInt32", f)
    
    def f(): desc.__set__(o, 12)
    AssertErrorWithMatch(AttributeError, "attribute 'B' of 'EnumInt32' object is read-only", f)

    def f(): desc.__set__(EnumInt32, 12)
    AssertErrorWithMatch(AttributeError, "attribute 'B' of 'EnumInt32' object is read-only", f)

    def f(): desc.__set__(None, EnumInt32.B) 
    AssertErrorWithMatch(AttributeError, "attribute 'B' of 'EnumInt32' object is read-only", f)
    
run_test(__name__)

delete_dlls_for_peverify(peverify_dependency)
