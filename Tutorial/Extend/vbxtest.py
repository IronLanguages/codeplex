#####################################################################################
#  
#  Copyright (c) Microsoft Corporation. All rights reserved.
# 
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
# 
#  You must not remove this notice, or any other, from this software.
# 
######################################################################################

# Task 1

import clr
clr.AddReferenceToFile("vbextend.dll")
import Simple
dir(Simple)
s = Simple(10)
print s


# Task 2

import clr
clr.AddReferenceToFile("vbextend.dll")
import Simple
dir(Simple)
s = Simple(10)
for i in s: print i

# Task 3

import clr
clr.AddReferenceToFile("vbextend.dll")
import Simple
dir(Simple)
a = Simple(10)
b = Simple(20)
a + b

# Task 4

import clr
clr.AddReferenceToFile("vbextend.dll")
import Simple
a = Simple(10)
def X(i):
    return i + 100

a.Transform(X)
