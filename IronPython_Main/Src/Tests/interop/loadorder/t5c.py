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
clr.AddReference("loadorder_5")

# namespace NS {
#     public class Target<T> {
#         public static string Flag = typeof(Target<>).FullName;
#     }
#     public class Target<T1, T2> {
#         public static string Flag = typeof(Target<,>).FullName;
#     }
# }


import NS
print dir(NS)
#print NS.Target.Flag
#print NS.Target[int].Flag

clr.AddReference("loadorder_5c")

# namespace NS {
#     public class Target<T1> { // different generic parameter name
#         public static string Flag = typeof(Target<>).FullName + "_T1";
#     }
# }

print dir(NS)

#print NS.Target.Flag

print dir(NS)
print NS.__dict__
print NS.Target[int].Flag
print NS.Target[int, int].Flag


