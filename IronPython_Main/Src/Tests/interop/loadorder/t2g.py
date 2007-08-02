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

clr.AddReference("loadorder_2g")

# // generic type, which has same namespace, same name from First.Nongeneric1
# namespace First {
#     public class Nongeneric1<T> {
#         public static string Flag = typeof(Nongeneric1<>).FullName;
#     }
# }


print First.Nongeneric1.Flag
print First.Nongeneric1[int].Flag  # no need to import First again

print First.Nongeneric1.Flag
print Nongeneric1.Flag

print First.Nongeneric1[str].Flag

# SystemError: Nongeneric1 is not generic type yet
# Nongeneric1[float]

from First import *
print Nongeneric1[float].Flag

