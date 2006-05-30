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

from lib.assert_util import *

# Disallow assignment to empty tuple
def test_assign_to_empty():
    y = ()
    AssertError(SyntaxError, compile, "() = y", "Error", "exec")
    AssertError(SyntaxError, compile, "(), t = y, 0", "Error", "exec")
    AssertError(SyntaxError, compile, "((()))=((y))", "Error", "exec")
    del y

# Disallow unequal unpacking assignment
def test_unpack():
    tupleOfSize2 = (1, 2)

    def f1(): (a, b, c) = tupleOfSize2
    def f2(): del a

    AssertError(ValueError, f1)
    AssertError(NameError, f2)

    (a) = tupleOfSize2
    AreEqual(a, tupleOfSize2)
    del a

    (a, (b, c)) = (tupleOfSize2, tupleOfSize2)
    AreEqual(a, tupleOfSize2)
    AreEqual(b, 1)
    AreEqual(c, 2)
    del a, b, c

    ((a, b), c) = (tupleOfSize2, tupleOfSize2)
    AreEqual(a, 1)
    AreEqual(b, 2)
    AreEqual(c, tupleOfSize2)
    del a, b, c



run_test(__name__)