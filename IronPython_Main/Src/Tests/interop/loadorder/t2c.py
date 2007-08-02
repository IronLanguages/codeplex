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


import First
from First import *

print First.Nongeneric1.Flag
print Nongeneric1.Flag

clr.AddReference("loadorder_2c")

# // non-generic type, which has same namespace, same name from First.Nongeneric1
# namespace First {
#     public class Nongeneric1 {
#         public static string Flag = typeof(Nongeneric1).FullName + "_Same";
#     }
# }


print First.Nongeneric1.Flag
print Nongeneric1.Flag      # !!

import First
from First import *

print First.Nongeneric1.Flag
print Nongeneric1.Flag


