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

clr.AddReference("loadorder_2d")

# // non-generic type, which has same namespace, different name from First.Nongeneric1
# namespace First {
#     public class Nongeneric2 {
#         public static string Flag = typeof(Nongeneric2).FullName;
#     }
# }


print First.Nongeneric2.Flag  # no need to import First again
print First.Nongeneric1.Flag
print Nongeneric1.Flag

# NameError
# print Nongeneric2.Flag

from First import *

print First.Nongeneric1.Flag
print First.Nongeneric2.Flag
print Nongeneric1.Flag
print Nongeneric2.Flag

