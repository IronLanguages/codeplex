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

from Util.Debug import *
result = "Failed"

a = 2
b = a
del a


try:
    b = a
except NameError:
    result = "Success"

Assert(result == "Success")


class C:
    pass

c = C()
c.a = 10
Assert("a" in dir(c))
del c.a
Assert(not "a" in dir(c))
C.a = 10
Assert("a" in dir(C))
del C.a
Assert(not "a" in dir(C))

for m in dir([]):
    c = callable(m)
    a = getattr([], m)

success = False
try:
    del this_name_is_undefined
except NameError:
    success = True
Assert(success)
