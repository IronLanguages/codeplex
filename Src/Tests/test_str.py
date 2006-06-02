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

##
## Test builtin-method of str
##

from lib.assert_util import *

def test_none():
    AssertError(TypeError, "abc".translate, None)
    AssertError(TypeError, "abc".translate, None, 'h')

    AssertError(TypeError, "abc".replace, "new")
    AssertError(TypeError, "abc".replace, "new", 2)

    for fn in ['find', 'index', 'rfind', 'count', 'startswith', 'endswith']:
        f = getattr("abc", fn)
        AssertError(TypeError, f, None)
        AssertError(TypeError, f, None, 0)
        AssertError(TypeError, f, None, 0, 2)

    AssertError(TypeError, 'abc'.replace, None, 'ef')
    AssertError(TypeError, 'abc'.replace, None, 'ef', 1)

def test_add_mul(): 
    AssertError(TypeError, lambda: "a" + 3)
    AssertError(TypeError, lambda: 3 + "a")

    import sys
    AssertError(TypeError, lambda: "a" * "3")
    AssertError(OverflowError, lambda: "a" * (sys.maxint + 1))
    AssertError(OverflowError, lambda: (sys.maxint + 1) * "a")

    class mylong(long): pass

    # multiply
    AreEqual("aaaa", "a" * 4L)
    AreEqual("aaaa", "a" * mylong(4L))
    AreEqual("aaa", "a" * 3)
    AreEqual("a", "a" * True)
    AreEqual("", "a" * False)

    AreEqual("aaaa", 4L * "a")
    AreEqual("aaaa", mylong(4L) * "a")
    AreEqual("aaa", 3 * "a")
    AreEqual("a", True * "a")
    AreEqual("", False * "a" )

def test_startswith():
    AreEqual("abcde".startswith('c', 2, 6), True) 
    AreEqual("abc".startswith('c', 4, 6), False) 
    AreEqual("abcde".startswith('cde', 2, 9), True) 
    
    hw = "hello world"
    Assert(hw.startswith("hello"))
    Assert(not hw.startswith("heloo"))
    Assert(hw.startswith("llo", 2))
    Assert(not hw.startswith("lno", 2))
    Assert(hw.startswith("wor", 6, 9))
    Assert(not hw.startswith("wor", 6, 7))
    Assert(not hw.startswith("wox", 6, 10))
    Assert(not hw.startswith("wor", 6, 2))


def test_endswith():
    for x in (0, 1, 2, 3, -10, -3, -4):
        AreEqual("abcdef".endswith("def", x), True)
        AreEqual("abcdef".endswith("de", x, 5), True)
        AreEqual("abcdef".endswith("de", x, -1), True)

    for x in (4, 5, 6, 10, -1, -2):
        AreEqual("abcdef".endswith("def", x), False)
        AreEqual("abcdef".endswith("de", x, 5), False)
        AreEqual("abcdef".endswith("de", x, -1), False)

def test_rfind():
    AreEqual("abcdbcda".rfind("cd", 1), 5)
    AreEqual("abcdbcda".rfind("cd", 3), 5)
    AreEqual("abcdbcda".rfind("cd", 7), -1)

def test_split():
    x="Hello Worllds"
    s = x.split("ll")
    Assert(s[0] == "He")
    Assert(s[1] == "o Wor")
    Assert(s[2] == "ds")

    Assert("1,2,3,4,5,6,7,8,9,0".split(",") == ['1','2','3','4','5','6','7','8','9','0'])
    Assert("1,2,3,4,5,6,7,8,9,0".split(",", -1) == ['1','2','3','4','5','6','7','8','9','0'])
    Assert("1,2,3,4,5,6,7,8,9,0".split(",", 2) == ['1','2','3,4,5,6,7,8,9,0'])
    Assert("1--2--3--4--5--6--7--8--9--0".split("--") == ['1','2','3','4','5','6','7','8','9','0'])
    Assert("1--2--3--4--5--6--7--8--9--0".split("--", -1) == ['1','2','3','4','5','6','7','8','9','0'])
    Assert("1--2--3--4--5--6--7--8--9--0".split("--", 2) == ['1', '2', '3--4--5--6--7--8--9--0'])

def test_count():
    Assert("adadad".count("d") == 3)
    Assert("adbaddads".count("ad") == 3)

def test_expandtabs():
    Assert("\ttext\t".expandtabs(0) == "text")
    Assert("\ttext\t".expandtabs(-10) == "text")

# zero-length string
def test_empty_string():
    AreEqual(''.title(), '')
    AreEqual(''.capitalize(), '')
    AreEqual(''.count('a'), 0)
    table = '10' * 128
    AreEqual(''.translate(table), '')
    AreEqual(''.replace('a', 'ef'), '')
    AreEqual(''.replace('bc', 'ef'), '')
    AreEqual(''.split(), [])
    AreEqual(''.split(' '), [''])
    AreEqual(''.split('a'), [''])

run_test(__name__)
