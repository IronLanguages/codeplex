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
clr.AddReference("loadorder_3")

# namespace First {
#     public class Generic1<K, V> {
#         public static string Flag = typeof(Generic1<,>).FullName;
#     }
# }

import First

clr.AddReference("loadorder_3e")

# namespace Second {
#     public class Generic1<K, V> {
#         public static string Flag = typeof(Generic1<,>).FullName;
#     }
# }

import Second

AreEqual(First.Generic1[int, int].Flag, "First.Generic1`2")
AreEqual(Second.Generic1[int, int].Flag, "Second.Generic1`2")

from First import *

AreEqual(Generic1[int, int].Flag, "First.Generic1`2")

from Second import *

AreEqual(Generic1[int, int].Flag, "Second.Generic1`2")
