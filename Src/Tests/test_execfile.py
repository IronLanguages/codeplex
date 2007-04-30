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

from lib.assert_util import *

if is_cli:
    def test_sanity():
        root = testpath.public_testdir
    
        execfile(root + "/Inc/toexec.py")
        execfile(root + "/Inc/toexec.py")
        #execfile(root + "/doc.py")
        execfile(root + "/Inc/toexec.py")

def test_negative():
    AssertError(TypeError, execfile, None) # arg must be string
    AssertError(TypeError, execfile, [])
    AssertError(TypeError, execfile, 1)
    AssertError(TypeError, execfile, "somefile", "")

def test_scope():
    root = testpath.public_testdir
    z = 10
    execfile(root + "/Inc/execfile_scope.py")
    

run_test(__name__)

