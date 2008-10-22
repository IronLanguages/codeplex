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
    
from iptest.assert_util import *


add_clr_assemblies("loadorder_3")

# namespace First {
#     public class Generic1<K, V> {
#         public static string Flag = typeof(Generic1<,>).FullName;
#     }
# }

import First

add_clr_assemblies("loadorder_3c")

# namespace First {
#     public class Generic1 {
#         public static string Flag = typeof(Generic1).FullName;
#     }
# }

AreEqual(First.Generic1.Flag, "First.Generic1")
AreEqual(First.Generic1[int, int].Flag, "First.Generic1`2")

from First import Generic1

AreEqual(Generic1.Flag, "First.Generic1")
AreEqual(Generic1[str, str].Flag, "First.Generic1`2")

