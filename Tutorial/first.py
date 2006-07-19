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

def add(a, b):
    "add(a, b) -> returns a + b"
    return a + b

def factorial(n):
    "factorial(n) -> returns factorial of n"
    if n <= 1: return 1
    return n * factorial(n-1)

hi = "Hello from IronPython!"
