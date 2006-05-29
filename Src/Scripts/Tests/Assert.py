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

test = "Failure"

try:
    Assert(test == "Success", "Failed message")
except AssertionError, e:
    Assert(e.args[0] == "Failed message")
    test = "Success"

Assert(test == "Success")

test = "Failure"

try:
    Assert(test == "Success", "Failed message 2")
except AssertionError, e:
    Assert(e.args[0] == "Failed message 2")
    test = "Success"

Assert(test == "Success")
