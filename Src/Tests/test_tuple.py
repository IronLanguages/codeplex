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

def test_add_mul():
    AreEqual((1,2,3) + (4,5,6),  (1,2,3,4,5,6))
    AreEqual((1,2,3) * 2, (1,2,3,1,2,3))
    AreEqual(2 * (1,2,3), (1,2,3,1,2,3))

    class mylong(long): pass
    AreEqual((1,2) * mylong(2L), (1, 2, 1, 2))
    AreEqual((3, 4).__mul__(mylong(2L)), (3, 4, 3, 4))
    AreEqual((5, 6).__rmul__(mylong(2L)), (5, 6, 5, 6))
    AreEqual(mylong(2L) * (7,8) , (7, 8, 7, 8))

def test_tuple_hash():
    class myhashable(object):
        def __init__(self):
            self.hashcalls = 0
        def __hash__(self):
            self.hashcalls += 1
            return 42
        def __eq__(self, other):
            return type(self) == type(other)
    
    
    test = (myhashable(), myhashable(), myhashable())
    
    hash(test)
    
    AreEqual(test[0].hashcalls, 1)
    AreEqual(test[1].hashcalls, 1)
    AreEqual(test[2].hashcalls, 1)
    
def test_tuple_cli_interactions():
	if is_cli:
		# verify you can call ToString on a tuple after importing clr
		import clr
		a = (0,)
		
		AreEqual(str(a), a.ToString())
    

def test_sequence_assign():
    try:
        a, b = None
        AssertUnreachable()
    except TypeError, e:
        Assert(str(e).find('unpack non-sequence') != -1)

run_test(__name__)

