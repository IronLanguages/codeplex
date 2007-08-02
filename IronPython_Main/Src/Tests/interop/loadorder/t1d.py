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

import clr
clr.AddReference("loadorder_1c")

# public class NamespaceOrType<T> {
#     public static string Flag = typeof(NamespaceOrType<>).FullName;
# }

import NamespaceOrType

clr.AddReference("loadorder_1a")

# namespace NamespaceOrType {
#     public class C {
#         public static string Flag = typeof(C).FullName;
#     }
# }

# check
import NamespaceOrType
# check
