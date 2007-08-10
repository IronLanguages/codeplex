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

import clr
clr.AddReference("loadorder_4")

# namespace NS {
#     public class Target {
#         public static string Flag = typeof(Target).FullName;
#     }
#     public class Target<T> {
#         public static string Flag = typeof(Target<>).FullName;
#     }
# }

import NS
AreEqual(dir(NS), ['Target'])

clr.AddReference("loadorder_4a")

# namespace NS {
#     public class Target {
#         public static string Flag = typeof(Target).FullName + "_Same";
#     }
# }

AreEqual(dir(NS), ['Target'])

AreEqual(NS.Target.Flag, "NS.Target_Same")
AreEqual(NS.Target[int].Flag, "NS.Target`1")

AreEqual(dir(NS), ['Target'])

AreEqual(NS.Target[int].Flag, "NS.Target`1")
