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
clr.AddReference("loadorder_2")

# namespace First {
#     public class Nongeneric1 {
#         public static string Flag = typeof(Nongeneric1).FullName;
#     }
# }

clr.AddReference("loadorder_2f")

# // generic type, which has different namespace, different name from First.Nongeneric1
# namespace Second {
#     public class Nongeneric2<T> {
#         public static string Flag = typeof(Nongeneric2<>).FullName;
#     }
# }


from First import *
from Second import *

print Nongeneric1.Flag
print Nongeneric2[int].Flag

import First, Second

print First.Nongeneric1.Flag
print Second.Nongeneric2[str].Flag
