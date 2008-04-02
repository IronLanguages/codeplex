#####################################################################################
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public
# License. A  copy of the license can be found in the License.html file at the
# root of this distribution. If  you cannot locate the  Microsoft Public
# License, please send an email to  dlr@microsoft.com. By using this source
# code in any fashion, you are agreeing to be bound by the terms of the 
# Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#####################################################################################

import sys
from lib.assert_util import *

"""Test cases for CLR types that don't involve actually loading CLR into the module
using the CLR types"""

if is_cli:
    sys.path.append(testpath.test_inputs_dir)
    import UseCLI

    UseCLI.Form().Controls.Add(UseCLI.Control())

    nc = UseCLI.NestedClass()
    
    ic = UseCLI.NestedClass.InnerClass()
    
    tc = UseCLI.NestedClass.InnerClass.TripleNested()
    
    # access methods, fields, and properties on the class w/ nesteds,
    # the nested class, and the triple nested class
    for x in ((nc, ''), (ic, 'Inner'), (tc, 'Triple')):
        obj, name = x[0], x[1]
        
        AreEqual(getattr(obj, 'CallMe' + name)(), name + ' Hello World')
        
        AreEqual(getattr(obj, name+'Field'), None)
        
        AreEqual(getattr(obj, name+'Property'), None)
        
        setattr(obj, name+'Property', name)
        
        AreEqual(getattr(obj, name+'Field'), name)
        
        AreEqual(getattr(obj, name+'Property'), name)
        
        