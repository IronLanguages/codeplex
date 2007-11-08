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

import clr
clr.AddReference("loadorder_1a")

# namespace NamespaceOrType {
#     public class C {
#         public static string Flag = typeof(C).FullName;
#     }
# }

import NamespaceOrType

clr.AddReference("loadorder_1b")

# public class NamespaceOrType {
#     public static string Flag = typeof(NamespaceOrType).FullName;
# }

AreEqual(NamespaceOrType.C.Flag, "NamespaceOrType.C")

import NamespaceOrType

AssertError(AttributeError, lambda: NamespaceOrType.C)

AreEqual(NamespaceOrType.Flag, "NamespaceOrType")
