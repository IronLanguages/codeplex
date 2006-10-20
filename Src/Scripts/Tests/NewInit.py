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
from collections import *

global init

def Assert(val):
    if val == False:
        raise TypeError, "assertion failed"

def runTest(testCase):
    global typeMatch
    global init

    class foo(testCase.subtype):
        def __new__(cls, param):
            ret = testCase.subtype.__new__(cls, param)
            Assert(ret == testCase.newEq)
            Assert((ret != testCase.newEq) != True)
            return ret
        def __init__(self, param):
            testCase.subtype.__init__(self, param)
            Assert(self == testCase.initEq)
            Assert((self != testCase.initEq) != True)

    a = foo(testCase.param)
    Assert((type(a) == foo) == testCase.match)

class TestCase(object):
    __slots__ = ['subtype', 'newEq', 'initEq', 'match', 'param']
    def __init__(self, subtype, newEq, initEq, match, param):
        self.match = match
        self.subtype = subtype
        self.newEq = newEq
        self.initEq = initEq
        self.param = param


cases = [TestCase(int, 2, 2, True, 2),
         TestCase(list, [], [2,3,4], True, (2,3,4)),
         TestCase(deque, deque(), deque((2,3,4)), True, (2,3,4)),
         TestCase(set, set(), set((2,3,4)), True, (2,3,4)),
         TestCase(frozenset, frozenset((2,3,4)), frozenset((2,3,4)), True, (2,3,4)),
         TestCase(tuple, (2,3,4), (2,3,4), True, (2,3,4)),
         TestCase(str, 'abc', 'abc', True, 'abc'),
         TestCase(float, 2.3, 2.3, True, 2.3),
         TestCase(type, type(object), type(object), False, object),
         TestCase(long, 10000000000L, 10000000000L, True, 10000000000L),
         #TestCase(complex, complex(2.0, 0), complex(2.0, 0), True, 2.0),        # complex is currently a struct w/ no extensibel, we fail here
        # TestCase(file, 'abc', True),      # ???
        ]


for case in cases:
    runTest(case)

# verify we can call the base init directly

if is_cli:
    import clr
    clr.AddReferenceByPartialName('System.Windows.Forms')
    from System.Windows.Forms import *

    class MyForm(Form):
        def __init__(self, title):
            Form.__init__(self)
            self.Text = title

    a = MyForm('abc')
    AreEqual(a.Text, 'abc')

#TestCase(bool, True, True),                    # not an acceptable base type

