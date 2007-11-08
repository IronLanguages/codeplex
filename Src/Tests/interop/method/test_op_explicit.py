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
clr.AddReference("userdefinedconversions", "typesamples")

from lib.file_util import *
peverify_dependency = [
    merlin_root + r"\Test\ClrAssembly\bin\userdefinedconversions.dll", 
    merlin_root + r"\Test\ClrAssembly\bin\typesamples.dll"
]
copy_dlls_for_peverify(peverify_dependency)

import System
from Merlin.Testing import *
from Merlin.Testing.Call import *
from Merlin.Testing.TypeSample import *

        
run_test(__name__)

delete_dlls_for_peverify(peverify_dependency)
