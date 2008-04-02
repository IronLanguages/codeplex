#####################################################################################
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public
# License. A  copy of the license can be found in the License.html file at the
# root of this distribution. If  you cannot locate the  Microsoft Public
# License, please send an email to  dlr@microsoft.com. By using this source
# code in any fashion, you are agreeing to be bound by the terms of the 
# Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#####################################################################################

##
## Test builtin-method of str
##

from lib.assert_util import *
import sys
 
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
    
    AreEqual('abc'.rfind('', 0, 0), 0)
    AreEqual('abc'.rfind('', 0, 1), 1)
    AreEqual('abc'.rfind('', 0, 2), 2)
    AreEqual('abc'.rfind('', 0, 3), 3)
    AreEqual('abc'.rfind('', 0, 4), 3)
    
    AreEqual('x'.rfind('x', 0, 0), -1)
    
    AreEqual('x'.rfind('x', 3, 0), -1)
    AreEqual('x'.rfind('', 3, 0), -1)
    

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


def test_codecs():
    #all the encodings that should be supported
    #encodings = [ 'cp1252','ascii', 'utf-8', 'utf-16', 'latin-1', 'iso-8859-1', 'utf-16-le', 'utf-16-be', 'unicode-escape', 'raw-unicode-escape']
    #what actually is supported
    encodings = [ 'cp1252','ascii', 'utf-8', 'latin-1', 'iso-8859-1', 'utf-16-le', 'raw-unicode-escape']
    for encoding in encodings: Assert('abc'.encode(encoding).decode(encoding)=='abc')
    
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

def test_string_escape():
    for i in range(0x7f):
        if chr(i) == "'":
            AreEqual(chr(i).encode('string-escape'), "\\" + repr(chr(i))[1:-1])
        else:
            AreEqual(chr(i).encode('string-escape'), repr(chr(i))[1:-1])

def test_encode_decode():
    #AssertError(TypeError, 'abc'.encode, None) #INCOMPAT
    #AssertError(TypeError, 'abc'.decode, None)
    AreEqual('abc'.encode(), 'abc')
    AreEqual('abc'.decode(), 'abc')
    
def test_string_escape_trailing_slash():
    ok = False
    try:
        "\\".decode("string-escape")
    except ValueError:
        ok = True
    Assert(ok, "string that ends in trailing slash should fail string decode")

def test_str_subclass():
    import binascii
    class customstring(str):
        def __str__(self): return self.swapcase()
        def __repr__(self): return '<' + self + '>'
        def __hash__(self): return 42
        def __mul__(self, count): return 'multiplied'
        def __add__(self, other): return 23
        def __len__(self): return 2300
        def __contains__(self, value): return False
    
    o = customstring('abc')
    AreEqual(str(o), 'ABC')
    AreEqual(repr(o), '<abc>')
    AreEqual(hash(o), 42)
    AreEqual(o * 3, 'multiplied')
    AreEqual(o + 'abc', 23)
    AreEqual(len(o), 2300)
    AreEqual('a' in o, False)

if is_cli:
    def test_str_char_hash():
        import System 
        a = System.Char.Parse('a')

        for x in [{'a':'b'}, set(['a']), 'abc', ['a'], ('a',)]:
            AreEqual(a in x, True)
    
        AreEqual(hash(a), hash('a'))
        
        AreEqual('a' in a, True)

def test_str_equals():
    x = 'abc' == 'abc'
    y = 'def' == 'def'
    AreEqual(id(x), id(y))
    AreEqual(id(x), id(True))
    
    x = 'abc' != 'abc'
    y = 'def' != 'def'
    AreEqual(id(x), id(y))
    AreEqual(id(x), id(False))
    
    x = 'abcx' == 'abc'
    y = 'defx' == 'def'
    AreEqual(id(x), id(y))
    AreEqual(id(x), id(False))
    
    x = 'abcx' != 'abc'
    y = 'defx' != 'def'
    AreEqual(id(x), id(y))
    AreEqual(id(x), id(True))

run_test(__name__)
