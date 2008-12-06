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

##
## Test builtin-method of str
##
from iptest.assert_util import *
from iptest.misc_util import ip_supported_encodings
import sys

types = [bytearray, bytes]

def test_capitalize():
    tests = [(b'foo', b'Foo'), 
             (b' foo', b' foo')]
    
    for testType in types:
        for data, result in tests:
            AreEqual(testType(data).capitalize(), result)
            
    y = b''
    x = y.capitalize()
    AreEqual(id(x), id(y))
    
    y = bytearray(b'')
    x = y.capitalize()
    Assert(id(x) != id(y), "bytearray.capitalize returned self")

def test_center():
    for testType in types:
        AreEqual(testType(b'aa').center(4), b' aa ')
        AreEqual(testType(b'aa').center(4, b'*'), b'*aa*')
        AreEqual(testType(b'aa').center(4, '*'), b'*aa*')
        AreEqual(testType(b'aa').center(2), b'aa')
        AreEqual(testType(b'aa').center(2, '*'), b'aa')
        AreEqual(testType(b'aa').center(2, b'*'), b'aa')
    
    x = b'aa'
    AreEqual(id(x.center(2, '*')), id(x))
    AreEqual(id(x.center(2, b'*')), id(x))
    
    x = bytearray(b'aa')
    Assert(id(x.center(2, '*')) != id(x))
    Assert(id(x.center(2, b'*')) != id(x))

def test_count():
    for testType in types:
        AreEqual(testType(b"adadad").count(b"d"), 3)
        AreEqual(testType(b"adbaddads").count(b"ad"), 3)
        AreEqual(testType(b"adbaddads").count(b"ad", 1, 8), 2)
        AreEqual(testType(b"adbaddads").count(b"ad", -1, -1), 0)
        AreEqual(testType(b"adbaddads").count(b"ad", 0, -1), 3)
        AreEqual(testType(b"adbaddads").count(b"", 0, -1), 9)
        AreEqual(testType(b"adbaddads").count(b"", 27), 0)

def test_decode():
    for testType in types:
        AreEqual(testType(b'\xff\xfea\x00b\x00c\x00').decode('utf-16'), 'abc')

def test_endswith():
    for testType in types:
        AssertError(TypeError, testType(b'abcdef').endswith, ([], ))
        AreEqual(testType(b'abcdef').endswith(b'def'), True)
        AreEqual(testType(b'abcdef').endswith(b'def', -1, -2), False)
        AreEqual(testType(b'abcdef').endswith(b'def', 0, 42), True)
        AreEqual(testType(b'abcdef').endswith(b'def', 0, -7), False)
        AreEqual(testType(b'abcdef').endswith(b'def', 42, -7), False)
        AreEqual(testType(b'abcdef').endswith(b'def', 42), False)
        AreEqual(testType(b'abcdef').endswith(b'bar'), False)
        AreEqual(testType(b'abcdef').endswith((b'def', )), True)
        AreEqual(testType(b'abcdef').endswith((b'baz', )), False)
        AreEqual(testType(b'abcdef').endswith((b'baz', ), 0, 42), False)
        AreEqual(testType(b'abcdef').endswith((b'baz', ), 0, -42), False)

                
        for x in (0, 1, 2, 3, -10, -3, -4):
            AreEqual(testType(b"abcdef").endswith(b"def", x), True)
            AreEqual(testType(b"abcdef").endswith(b"de", x, 5), True)
            AreEqual(testType(b"abcdef").endswith(b"de", x, -1), True)
            AreEqual(testType(b"abcdef").endswith((b"def", ), x), True)
            AreEqual(testType(b"abcdef").endswith((b"de", ), x, 5), True)
            AreEqual(testType(b"abcdef").endswith((b"de", ), x, -1), True)
    
        for x in (4, 5, 6, 10, -1, -2):
            AreEqual(testType(b"abcdef").endswith((b"def", ), x), False)
            AreEqual(testType(b"abcdef").endswith((b"de", ), x, 5), False)
            AreEqual(testType(b"abcdef").endswith((b"de", ), x, -1), False)

def test_expandtabs():
    for testType in types:
        Assert(testType(b"\ttext\t").expandtabs(0) == b"text")
        Assert(testType(b"\ttext\t").expandtabs(-10) == b"text")
        AreEqual(testType(b"\r\ntext\t").expandtabs(-10), b"\r\ntext")
        
        AreEqual(len(testType(b"aaa\taaa\taaa").expandtabs()), 19)
        AreEqual(testType(b"aaa\taaa\taaa").expandtabs(), b"aaa     aaa     aaa")

def test_extend():
    b = bytearray(b'abc')
    b.extend(b'def')
    AreEqual(b, b'abcdef')
    b.extend(bytearray(b'ghi'))
    AreEqual(b, b'abcdefghi')
    
def test_find():
    for testType in types:
        AreEqual(testType(b"abcdbcda").find(b"cd", 1), 2)
        AreEqual(testType(b"abcdbcda").find(b"cd", 3), 5)
        AreEqual(testType(b"abcdbcda").find(b"cd", 7), -1)
        AreEqual(testType(b'abc').find(b'abc', -1, 1), -1)
        AreEqual(testType(b'abc').find(b'abc', 25), -1)
        AreEqual(testType(b'abc').find(b'add', 0, 3), -1)
        
        AreEqual(testType(b'abc').find(b'', 0, 0), 0)
        AreEqual(testType(b'abc').find(b'', 0, 1), 0)
        AreEqual(testType(b'abc').find(b'', 0, 2), 0)
        AreEqual(testType(b'abc').find(b'', 0, 3), 0)
        AreEqual(testType(b'abc').find(b'', 0, 4), 0)
        AreEqual(testType(b'').find(b'', 0, 4), 0)
        
        AreEqual(testType(b'x').find(b'x', 0, 0), -1)
        
        AreEqual(testType(b'x').find(b'x', 3, 0), -1)
        AreEqual(testType(b'x').find(b'', 3, 0), -1)
        
def test_fromhex():
    for testType in types:
        if testType != str:
            AssertError(ValueError, testType.fromhex, u'0')
            AssertError(ValueError, testType.fromhex, u'A')
            AssertError(ValueError, testType.fromhex, u'a')
            AssertError(ValueError, testType.fromhex, u'aG')
            AssertError(ValueError, testType.fromhex, u'Ga')
            
            AreEqual(testType.fromhex(u'00'), b'\x00')
            AreEqual(testType.fromhex(u'00 '), b'\x00')
            AreEqual(testType.fromhex(u'00  '), b'\x00')
            AreEqual(testType.fromhex(u'00  01'), b'\x00\x01')
            AreEqual(testType.fromhex(u'00  01 0a'), b'\x00\x01\x0a')
            AreEqual(testType.fromhex(u'00  01 0a 0B'), b'\x00\x01\x0a\x0B')
            AreEqual(testType.fromhex(u'00  a1 Aa 0B'), b'\x00\xA1\xAa\x0B')

def test_index():
    for testType in types:
        AssertError(TypeError, testType(b'abc').index, 257)
        AreEqual(testType(b'abc').index(b'a'), 0)
        AreEqual(testType(b'abc').index(b'a', 0, -1), 0)
        
        AssertError(ValueError, testType(b'abc').index, b'c', 0, -1)
        AssertError(ValueError, testType(b'abc').index, b'a', -1)

def test_insert():
    b = bytearray(b'abc')
    b.insert(0, ord('d'))
    AreEqual(b, b'dabc')

    b.insert(1000, ord('d'))
    AreEqual(b, b'dabcd')

    b.insert(-1, ord('d'))
    AreEqual(b, b'dabcdd')
    
    AssertError(ValueError, b.insert, 0, 256)

def check_is_method(methodName, result):
    for testType in types:
        AreEqual(getattr(testType(b''), methodName)(), False)
        for i in xrange(256):
            data = bytearray()
            data.append(i)
               
            Assert(getattr(testType(data), methodName)() == result(i), chr(i) + " (" + str(i) + ") should be " + str(result(i)))
    
def test_isalnum():
    check_is_method('isalnum', lambda i : i >= ord('a') and i <= ord('z') or i >= ord('A') and i <= ord('Z') or i >= ord('0') and i <= ord('9'))
    
def test_isalpha():
    check_is_method('isalpha', lambda i : i >= ord('a') and i <= ord('z') or i >= ord('A') and i <= ord('Z'))

def test_isdigit():
    check_is_method('isdigit', lambda i : (i >= ord('0') and i <= ord('9')))

def test_islower():
    check_is_method('islower', lambda i : i >= ord('a') and i <= ord('z'))
    
def test_isspace():
    check_is_method('isspace', lambda i : i in [ord(' '), ord('\t'), ord('\f'), ord('\n'), ord('\r'), 11])

def test_istitle():
    for testType in types:
        AreEqual(testType(b'').istitle(), False)
        AreEqual(testType(b'Foo').istitle(), True)
        AreEqual(testType(b'Foo Bar').istitle(), True)
        AreEqual(testType(b'FooBar').istitle(), False)
        AreEqual(testType(b'foo').istitle(), False)

def test_isupper():
    check_is_method('isupper', lambda i : i >= ord('A') and i <= ord('Z'))

def test_join():
    x = b''
    AreEqual(id(x.join(b'')), id(x))

    x = bytearray(x)
    Assert(id(x.join(b'')) != id(x))

    x = b'abc'
    AreEqual(id(b'foo'.join([x])), id(x))

    AssertError(TypeError, b'foo'.join, [42])
    
    x = bytearray(b'foo')
    Assert(id(bytearray(b'foo').join([x])) != id(x), "got back same object on single arg join w/ bytearray")
    
    for testType in types:
        AreEqual(testType(b'x').join([b'd', b'e', b'f']), b'dxexf')
        AreEqual(testType(b'x').join([b'd', b'e', b'f']), b'dxexf')
        AreEqual(type(testType(b'x').join([b'd', b'e', b'f'])), testType)
        if str != bytes:
            # works in Py3k/Ipy, not in Py2.6
            AreEqual(b'x'.join([testType(b'd'), testType(b'e'), testType(b'f')]), b'dxexf')
        AreEqual(bytearray(b'x').join([testType(b'd'), testType(b'e'), testType(b'f')]), b'dxexf')
        AreEqual(testType(b'').join([]), b'')
        AreEqual(testType(b'').join((b'abc', )), b'abc')
        AreEqual(testType(b'').join((b'abc', b'def')), b'abcdef')
        AssertError(TypeError, testType(b'').join, (42, ))

def test_ljust():
    for testType in types:
        AssertError(TypeError, testType(b'').ljust, 42, '  ')
        AssertError(TypeError, testType(b'').ljust, 42, b'  ')
        AssertError(TypeError, testType(b'').ljust, 42, u'\u0100')
        AreEqual(testType(b'abc').ljust(4), b'abc ')
        AreEqual(testType(b'abc').ljust(4, b'x'), b'abcx')
        AreEqual(testType(b'abc').ljust(4, 'x'), b'abcx')
    
    x = b'abc'
    AreEqual(id(x.ljust(2)), id(x))
    
    x = bytearray(x)
    Assert(id(x.ljust(2)) != id(x))

def test_lower():
    expected = b'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f'  \
    b'\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%' \
    b'&\'()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[\\]^_`'          \
    b'abcdefghijklmnopqrstuvwxyz{|}~\x7f\x80\x81\x82\x83\x84\x85\x86\x87\x88' \
    b'\x89\x8a\x8b\x8c\x8d\x8e\x8f\x90\x91\x92\x93\x94\x95\x96\x97\x98\x99'   \
    b'\x9a\x9b\x9c\x9d\x9e\x9f\xa0\xa1\xa2\xa3\xa4\xa5\xa6\xa7\xa8\xa9\xaa'   \
    b'\xab\xac\xad\xae\xaf\xb0\xb1\xb2\xb3\xb4\xb5\xb6\xb7\xb8\xb9\xba\xbb'   \
    b'\xbc\xbd\xbe\xbf\xc0\xc1\xc2\xc3\xc4\xc5\xc6\xc7\xc8\xc9\xca\xcb\xcc'   \
    b'\xcd\xce\xcf\xd0\xd1\xd2\xd3\xd4\xd5\xd6\xd7\xd8\xd9\xda\xdb\xdc\xdd'   \
    b'\xde\xdf\xe0\xe1\xe2\xe3\xe4\xe5\xe6\xe7\xe8\xe9\xea\xeb\xec\xed\xee'   \
    b'\xef\xf0\xf1\xf2\xf3\xf4\xf5\xf6\xf7\xf8\xf9\xfa\xfb\xfc\xfd\xfe\xff'
    
    data = bytearray()
    for i in xrange(256):
        data.append(i)
    
    for testType in types:
        AreEqual(testType(data).lower(), expected)
    
def test_lstrip():
    for testType in types:
        AreEqual(testType(b' abc').lstrip(), b'abc')
        AreEqual(testType(b' abc ').lstrip(), b'abc ')
        AreEqual(testType(b' ').lstrip(), b'')

    x = b'abc'
    AreEqual(id(x.lstrip()), id(x))

    x = bytearray(x)
    Assert(id(x.lstrip()) != id(x))

def test_partition():
    for testType in types:
        AssertError(TypeError, testType(b'').partition, None)
        AssertError(TypeError, testType(b'').partition, None)
        AssertError(ValueError, testType(b'').partition, b'')
        AssertError(ValueError, testType(b'').partition, b'')
        
        AreEqual(testType(b'abc').partition(b'b'), (b'a', b'b', b'c'))
        AreEqual(testType(b'abc').partition(b'd'), (b'abc', b'', b''))
        
        x = testType(b'abc')
        one, two, three = x.partition(b'd')
        AreEqual(id(one), id(x))
    
    one, two, three = b''.partition(b'abc')
    AreEqual(id(one), id(two))
    AreEqual(id(two), id(three))

    one, two, three = bytearray().partition(b'abc')
    Assert(id(one) != id(two))
    Assert(id(two) != id(three))
    Assert(id(three) != id(one))

def test_pop():
    b = bytearray()
    AssertError(OverflowError, b.pop)
    AssertError(OverflowError, b.pop, 0)
    
    b = bytearray(b'abc')
    AreEqual(b.pop(), ord('c'))
    AreEqual(b, b'ab')
    
    b = bytearray(b'abc')
    b.pop(1)
    AreEqual(b, b'ac')

    b = bytearray(b'abc')
    b.pop(-1)
    AreEqual(b, b'ab')

def test_replace():
    for testType in types:
        AssertError(TypeError, testType(b'abc').replace, None, b'abc')
        AssertError(TypeError, testType(b'abc').replace, b'abc', None)
        
        AreEqual(testType(b'abc').replace(b'b', b'foo'), b'afooc')
        AreEqual(testType(b'abc').replace(b'b', b''), b'ac')
        AreEqual(testType(b'abcb').replace(b'b', b'foo', 1), b'afoocb')
        AreEqual(testType(b'abcb').replace(b'b', b'foo', 2), b'afoocfoo')
        AreEqual(testType(b'abcb').replace(b'b', b'foo', 3), b'afoocfoo')
        AreEqual(testType(b'abcb').replace(b'b', b'foo', -1), b'afoocfoo')
        AreEqual(testType(b'abcb').replace(b'', b'foo', 100), b'fooafoobfoocfoobfoo')
        AreEqual(testType(b'abcb').replace(b'', b'foo', 0), b'abcb')
        AreEqual(testType(b'abcb').replace(b'', b'foo', 1), b'fooabcb')
    
    x = b'abc'
    AreEqual(id(x.replace(b'foo', b'bar', 0)), id(x))
    
    if is_cli:
        # CPython bug in 2.6 - http://bugs.python.org/issue4348
        x = bytearray(b'abc')
        Assert(id(x.replace(b'foo', b'bar', 0)) != id(x))

def test_remove():
    b = bytearray(b'abc')
    b.remove(ord('a'))
    AreEqual(b, b'bc')
    AssertError(ValueError, b.remove, ord('x'))

def test_reverse():
    b = bytearray(b'abc')
    b.reverse()
    AreEqual(b, b'cba')    
    
@skip("silverlight") # CoreCLR bug xxxx found in build 30324 from silverlight_w2
def test_rfind():
    for testType in types:
        AreEqual(testType(b"abcdbcda").rfind(b"cd", 1), 5)
        AreEqual(testType(b"abcdbcda").rfind(b"cd", 3), 5)
        AreEqual(testType(b"abcdbcda").rfind(b"cd", 7), -1)
        AreEqual(testType(b"abcdbcda").rfind(b"cd", -1, -2), -1)
        AreEqual(testType(b"abc").rfind(b"add", 3, 0), -1)
        AreEqual(testType(b'abc').rfind(b'bd'), -1)
        
        AreEqual(testType(b'abc').rfind(b'', 0, 0), 0)
        AreEqual(testType(b'abc').rfind(b'', 0, 1), 1)
        AreEqual(testType(b'abc').rfind(b'', 0, 2), 2)
        AreEqual(testType(b'abc').rfind(b'', 0, 3), 3)
        AreEqual(testType(b'abc').rfind(b'', 0, 4), 3)
        
        AreEqual(testType(b'x').rfind(b'x', 0, 0), -1)
        
        AreEqual(testType(b'x').rfind(b'x', 3, 0), -1)
        AreEqual(testType(b'x').rfind(b'', 3, 0), -1)    

def test_rindex():
    for testType in types:
        AssertError(TypeError, testType(b'abc').rindex, 257)
        AreEqual(testType(b'abc').rindex(b'a'), 0)
        AreEqual(testType(b'abc').rindex(b'a', 0, -1), 0)
        
        AssertError(ValueError, testType(b'abc').rindex, b'c', 0, -1)
        AssertError(ValueError, testType(b'abc').rindex, b'a', -1)

def test_rjust():
    for testType in types:
        AssertError(TypeError, testType(b'').rjust, 42, '  ')
        AssertError(TypeError, testType(b'').rjust, 42, b'  ')
        AssertError(TypeError, testType(b'').rjust, 42, u'\u0100')
        AreEqual(testType(b'abc').rjust(4), b' abc')
        AreEqual(testType(b'abc').rjust(4, b'x'), b'xabc')
        AreEqual(testType(b'abc').rjust(4, 'x'), b'xabc')
    
    x = b'abc'
    AreEqual(id(x.rjust(2)), id(x))
    
    x = bytearray(x)
    Assert(id(x.rjust(2)) != id(x))

def test_rpartition():
    for testType in types:
        AssertError(TypeError, testType(b'').rpartition, None)
        AssertError(TypeError, testType(b'').rpartition, None)
        AssertError(ValueError, testType(b'').rpartition, b'')
        AssertError(ValueError, testType(b'').rpartition, b'')
        
        AreEqual(testType(b'abc').rpartition(b'b'), (b'a', b'b', b'c'))
        AreEqual(testType(b'abc').rpartition(b'd'), (b'', b'', b'abc'))
        
        x = testType(b'abc')
        one, two, three = x.rpartition(b'd')        
        AreEqual(id(three), id(x))
    
    one, two, three = b''.rpartition(b'abc')
    AreEqual(id(one), id(two))
    AreEqual(id(two), id(three))

    one, two, three = bytearray().rpartition(b'abc')
    Assert(id(one) != id(two))
    Assert(id(two) != id(three))
    Assert(id(three) != id(one))

def test_rsplit():
    for testType in types:
        x=testType(b"Hello Worllds")
        AreEqual(x.rsplit(), [b'Hello', b'Worllds'])
        s = x.rsplit(b"ll")
        Assert(s[0] == b"He")
        Assert(s[1] == b"o Wor")
        Assert(s[2] == b"ds")
    
        Assert(testType(b"1--2--3--4--5--6--7--8--9--0").rsplit(b"--", 2) == [b'1--2--3--4--5--6--7--8', b'9', b'0'])
    
        for temp_string in [b"", b"  ", b"   ", b"\t", b" \t", b"\t ", b"\t\t", b"\n", b"\n\n", b"\n \n"]:
            AreEqual(temp_string.rsplit(None), [])
        
        AreEqual(testType(b"ab").rsplit(None), [b"ab"])
        AreEqual(testType(b"a b").rsplit(None), [b"a", b"b"])

def test_rstrip():
    for testType in types:
        AreEqual(testType(b'abc ').rstrip(), b'abc')
        AreEqual(testType(b' abc ').rstrip(), b' abc')
        AreEqual(testType(b' ').rstrip(), b'')

    x = b'abc'
    AreEqual(id(x.rstrip()), id(x))

    x = bytearray(x)
    Assert(id(x.rstrip()) != id(x))

def test_split():
    for testType in types:
        
        x=testType(b"Hello Worllds")
        AssertError(ValueError, x.split, b'')
        AreEqual(x.split(None, 0), [b'Hello Worllds'])
        AreEqual(x.split(None, -1), [b'Hello', b'Worllds'])
        AreEqual(x.split(None, 2), [b'Hello', b'Worllds'])
        AreEqual(x.split(), [b'Hello', b'Worllds'])
        AreEqual(testType(b'abc').split(b'c'), [b'ab', b''])
        AreEqual(testType(b'abcd').split(b'c'), [b'ab', b'd'])
        AreEqual(testType(b'abccdef').split(b'c'), [b'ab', b'', b'def'])
        s = x.split(b"ll")
        Assert(s[0] == b"He")
        Assert(s[1] == b"o Wor")
        Assert(s[2] == b"ds")
    
        Assert(testType(b"1,2,3,4,5,6,7,8,9,0").split(b",") == [b'1',b'2',b'3',b'4',b'5',b'6',b'7',b'8',b'9',b'0'])
        Assert(testType(b"1,2,3,4,5,6,7,8,9,0").split(b",", -1) == [b'1',b'2',b'3',b'4',b'5',b'6',b'7',b'8',b'9',b'0'])
        Assert(testType(b"1,2,3,4,5,6,7,8,9,0").split(b",", 2) == [b'1',b'2',b'3,4,5,6,7,8,9,0'])
        Assert(testType(b"1--2--3--4--5--6--7--8--9--0").split(b"--") == [b'1',b'2',b'3',b'4',b'5',b'6',b'7',b'8',b'9',b'0'])
        Assert(testType(b"1--2--3--4--5--6--7--8--9--0").split(b"--", -1) == [b'1',b'2',b'3',b'4',b'5',b'6',b'7',b'8',b'9',b'0'])
        Assert(testType(b"1--2--3--4--5--6--7--8--9--0").split(b"--", 2) == [b'1', b'2', b'3--4--5--6--7--8--9--0'])
    
        AreEqual(testType(b"").split(None), [])
        AreEqual(testType(b"ab").split(None), [b"ab"])
        AreEqual(testType(b"a b").split(None), [b"a", b"b"])

def test_splitlines():
    for testType in types:
        AreEqual(testType(b'foo\nbar\n').splitlines(), [b'foo', b'bar'])
        AreEqual(testType(b'foo\nbar\n').splitlines(True), [b'foo\n', b'bar\n'])
        AreEqual(testType(b'foo\r\nbar\r\n').splitlines(True), [b'foo\r\n', b'bar\r\n'])
        AreEqual(testType(b'foo\r\nbar\r\n').splitlines(), [b'foo', b'bar'])
        AreEqual(testType(b'foo\rbar\r').splitlines(True), [b'foo\r', b'bar\r'])
        AreEqual(testType(b'foo\nbar\nbaz').splitlines(), [b'foo', b'bar', b'baz'])
        AreEqual(testType(b'foo\nbar\nbaz').splitlines(True), [b'foo\n', b'bar\n', b'baz'])
        AreEqual(testType(b'foo\r\nbar\r\nbaz').splitlines(True), [b'foo\r\n', b'bar\r\n', b'baz'])
        AreEqual(testType(b'foo\rbar\rbaz').splitlines(True), [b'foo\r', b'bar\r', b'baz'])
    
def test_startswith():
    for testType in types:
        AreEqual(testType(b"abcde").startswith(b'c', 2, 6), True)
        AreEqual(testType(b"abc").startswith(b'c', 4, 6), False)
        AreEqual(testType(b"abcde").startswith(b'cde', 2, 9), True)
        AreEqual(testType(b'abc').startswith(b'abcd', 4), False)
        AreEqual(testType(b'abc').startswith(b'abc', -3), True)
        AreEqual(testType(b'abc').startswith(b'abc', -10), True)
        AreEqual(testType(b'abc').startswith(b'abc', -3, 0), False)
        AreEqual(testType(b'abc').startswith(b'abc', -10, 0), False)
        AreEqual(testType(b'abc').startswith(b'abc', -10, -10), False)
        AreEqual(testType(b'abc').startswith(b'ab', 0, -1), True)
        AreEqual(testType(b'abc').startswith((b'abc', ), -10), True)
        AreEqual(testType(b'abc').startswith((b'abc', ), 10), False)
        AreEqual(testType(b'abc').startswith((b'abc', ), -10, 0), False)
        AreEqual(testType(b'abc').startswith((b'abc', ), 10, 0), False)
        AreEqual(testType(b'abc').startswith((b'abc', ), 1, -10), False)
        AreEqual(testType(b'abc').startswith((b'abc', ), 1, -1), False)
        AreEqual(testType(b'abc').startswith((b'abc', ), -1, -2), False)

        AreEqual(testType(b'abc').startswith((b'abc', b'def')), True)
        AreEqual(testType(b'abc').startswith((b'qrt', b'def')), False)
        AreEqual(testType(b'abc').startswith((b'abc', b'def'), -3), True)
        AreEqual(testType(b'abc').startswith((b'qrt', b'def'), -3), False)
        AreEqual(testType(b'abc').startswith((b'abc', b'def'), 0), True)
        AreEqual(testType(b'abc').startswith((b'qrt', b'def'), 0), False)
        AreEqual(testType(b'abc').startswith((b'abc', b'def'), -3, 3), True)
        AreEqual(testType(b'abc').startswith((b'qrt', b'def'), -3, 3), False)
        AreEqual(testType(b'abc').startswith((b'abc', b'def'), 0, 3), True)
        AreEqual(testType(b'abc').startswith((b'qrt', b'def'), 0, 3), False)
        
        hw = testType(b"hello world")
        Assert(hw.startswith(b"hello"))
        Assert(not hw.startswith(b"heloo"))
        Assert(hw.startswith(b"llo", 2))
        Assert(not hw.startswith(b"lno", 2))
        Assert(hw.startswith(b"wor", 6, 9))
        Assert(not hw.startswith(b"wor", 6, 7))
        Assert(not hw.startswith(b"wox", 6, 10))
        Assert(not hw.startswith(b"wor", 6, 2))

def test_strip():
    for testType in types:
        AreEqual(testType(b'abc ').strip(), b'abc')
        AreEqual(testType(b' abc').strip(), b'abc')
        AreEqual(testType(b' abc ').strip(), b'abc')
        AreEqual(testType(b' ').strip(), b'')

    x = b'abc'
    AreEqual(id(x.strip()), id(x))

    x = bytearray(x)
    Assert(id(x.strip()) != id(x))

def test_swapcase():
    expected = b'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f'  \
    b'\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%' \
    b'&\'()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[\\]^_`'          \
    b'ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~\x7f\x80\x81\x82\x83\x84\x85\x86\x87\x88' \
    b'\x89\x8a\x8b\x8c\x8d\x8e\x8f\x90\x91\x92\x93\x94\x95\x96\x97\x98\x99'   \
    b'\x9a\x9b\x9c\x9d\x9e\x9f\xa0\xa1\xa2\xa3\xa4\xa5\xa6\xa7\xa8\xa9\xaa'   \
    b'\xab\xac\xad\xae\xaf\xb0\xb1\xb2\xb3\xb4\xb5\xb6\xb7\xb8\xb9\xba\xbb'   \
    b'\xbc\xbd\xbe\xbf\xc0\xc1\xc2\xc3\xc4\xc5\xc6\xc7\xc8\xc9\xca\xcb\xcc'   \
    b'\xcd\xce\xcf\xd0\xd1\xd2\xd3\xd4\xd5\xd6\xd7\xd8\xd9\xda\xdb\xdc\xdd'   \
    b'\xde\xdf\xe0\xe1\xe2\xe3\xe4\xe5\xe6\xe7\xe8\xe9\xea\xeb\xec\xed\xee'   \
    b'\xef\xf0\xf1\xf2\xf3\xf4\xf5\xf6\xf7\xf8\xf9\xfa\xfb\xfc\xfd\xfe\xff'
    
    data = bytearray()
    for i in xrange(256):
        data.append(i)

    for testType in types:
        AreEqual(testType(b'123').swapcase(), b'123')       
        b = testType(b'123')
        Assert(id(b.swapcase()) != id(b))
        
        AreEqual(testType(b'abc').swapcase(), b'ABC')
        AreEqual(testType(b'ABC').swapcase(), b'abc')
        AreEqual(testType(b'ABc').swapcase(), b'abC')
        
        x = testType(data).swapcase()
        AreEqual(testType(data).swapcase(), expected)
    
def test_title():
    for testType in types:
        AreEqual(testType(b'').title(), b'')
        AreEqual(testType(b'foo').title(), b'Foo')
        AreEqual(testType(b'Foo').title(), b'Foo')
        AreEqual(testType(b'foo bar baz').title(), b'Foo Bar Baz')
        
        for i in xrange(256):
            b = bytearray()
            b.append(i)
            
            if (b >= b'a' and b <= b'z') or (b >= b'A' and b <= 'Z'):
                continue
            
            inp = testType(b.join([b'foo', b'bar', b'baz']))
            exp = b.join([b'Foo', b'Bar', b'Baz'])
            AreEqual(inp.title(), exp)
            
    x = b''
    AreEqual(id(x.title()), id(x))
    
    x = bytearray(b'')
    Assert(id(x.title()) != id(x))

def test_translate():
    identTable = bytearray()
    for i in xrange(256):
        identTable.append(i)

    repAtable = bytearray(identTable)
    repAtable[ord('A')] = ord('B')
    
    for testType in types:
        AssertError(TypeError, testType(b'').translate, {})
        AssertError(ValueError, testType(b'foo').translate, b'')
        AssertError(ValueError, testType(b'').translate, b'')        
        AreEqual(testType(b'AAA').translate(repAtable), b'BBB')
        AreEqual(testType(b'AAA').translate(repAtable, b'A'), b'')
        AssertError(TypeError, b''.translate, identTable, None)
    
    AreEqual(b'AAA'.translate(None, b'A'), b'')
    AreEqual(b'AAABBB'.translate(None, b'A'), b'BBB')
    AreEqual(b'AAA'.translate(None), b'AAA')
    AssertError(TypeError, bytearray(b'AAA').translate, None, b'A')
    AssertError(TypeError, bytearray(b'AAA').translate, None)
    
    b = b'abc'    
    AreEqual(id(b.translate(None)), id(b))    
    
    b = b''
    AreEqual(id(b.translate(identTable)), id(b))

    b = b''
    AreEqual(id(b.translate(identTable, b'')), id(b))

    b = b''
    AreEqual(id(b.translate(identTable, b'')), id(b))
    
    if is_cli:
        # CPython bug 4348 - http://bugs.python.org/issue4348
        b = bytearray(b'')
        Assert(id(b.translate(identTable)) != id(b))

def test_upper():
    expected = b'\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f'  \
    b'\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f !"#$%' \
    b'&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`'          \
    b'ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~\x7f\x80\x81\x82\x83\x84\x85\x86\x87\x88' \
    b'\x89\x8a\x8b\x8c\x8d\x8e\x8f\x90\x91\x92\x93\x94\x95\x96\x97\x98\x99'   \
    b'\x9a\x9b\x9c\x9d\x9e\x9f\xa0\xa1\xa2\xa3\xa4\xa5\xa6\xa7\xa8\xa9\xaa'   \
    b'\xab\xac\xad\xae\xaf\xb0\xb1\xb2\xb3\xb4\xb5\xb6\xb7\xb8\xb9\xba\xbb'   \
    b'\xbc\xbd\xbe\xbf\xc0\xc1\xc2\xc3\xc4\xc5\xc6\xc7\xc8\xc9\xca\xcb\xcc'   \
    b'\xcd\xce\xcf\xd0\xd1\xd2\xd3\xd4\xd5\xd6\xd7\xd8\xd9\xda\xdb\xdc\xdd'   \
    b'\xde\xdf\xe0\xe1\xe2\xe3\xe4\xe5\xe6\xe7\xe8\xe9\xea\xeb\xec\xed\xee'   \
    b'\xef\xf0\xf1\xf2\xf3\xf4\xf5\xf6\xf7\xf8\xf9\xfa\xfb\xfc\xfd\xfe\xff'
    
    data = bytearray()
    for i in xrange(256):
        data.append(i)

    for testType in types:
        AreEqual(testType(data).upper(), expected)

def test_zfill():
    for testType in types:
        AreEqual(testType(b'abc').zfill(0), b'abc')
        AreEqual(testType(b'abc').zfill(4), b'0abc')
        AreEqual(testType(b'+abc').zfill(5), b'+0abc')
        AreEqual(testType(b'-abc').zfill(5), b'-0abc')
        AreEqual(testType(b'').zfill(2), b'00')
        AreEqual(testType(b'+').zfill(2), b'+0')
        AreEqual(testType(b'-').zfill(2), b'-0')

    b = b'abc'
    AreEqual(id(b.zfill(0)), id(b))
    
    b = bytearray(b)
    Assert(id(b.zfill(0)) != id(b))

def test_none():
    for testType in types:        
        AssertError(TypeError, testType(b'abc').replace, b"new")
        AssertError(TypeError, testType(b'abc').replace, b"new", 2)
        AssertError(TypeError, testType(b'abc').center, 0, None)
        if str != bytes:
            AssertError(TypeError, testType(b'abc').fromhex, None)
        AssertError(TypeError, testType(b'abc').decode, 'ascii', None)
    
        for fn in ['find', 'index', 'rfind', 'count', 'startswith', 'endswith']:
            f = getattr(testType(b'abc'), fn)
            AssertError(TypeError, f, None)
            AssertError(TypeError, f, None, 0)
            AssertError(TypeError, f, None, 0, 2)
    
        AssertError(TypeError, testType(b'abc').replace, None, b'ef')
        AssertError(TypeError, testType(b'abc').replace, None, b'ef', 1)
        AssertError(TypeError, testType(b'abc').replace, b'abc', None)
        AssertError(TypeError, testType(b'abc').replace, b'abc', None, 1)

def test_add_mul():
    for testType in types:
        AssertError(TypeError, lambda: testType(b"a") + 3)
        AssertError(TypeError, lambda: 3 + testType(b"a"))
    
        AssertError(TypeError, lambda: "a" * "3")
        AssertError(OverflowError, lambda: "a" * (sys.maxint + 1))
        AssertError(OverflowError, lambda: (sys.maxint + 1) * "a")
    
        class mylong(long): pass
        
        if is_cli:
            from System.IO import Path
            AreEqual("foo\\", "foo" + Path.DirectorySeparatorChar)
            AreEqual("\\\\", Path.DirectorySeparatorChar + '\\')
    
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

# zero-length string
def test_empty_bytes():
    for testType in types:
        AreEqual(testType(b'').title(), b'')
        AreEqual(testType(b'').capitalize(), b'')
        AreEqual(testType(b'').count(b'a'), 0)
        table = testType(b'10') * 128
        AreEqual(testType(b'').translate(table), b'')
        AreEqual(testType(b'').replace(b'a', b'ef'), b'')
        AreEqual(testType(b'').replace(b'bc', b'ef'), b'')
        AreEqual(testType(b'').split(), [])
        AreEqual(testType(b'').split(b' '), [b''])
        AreEqual(testType(b'').split(b'a'), [b''])

def test_encode_decode():
    for testType in types:
        AreEqual(testType(b'abc').decode(), u'abc')

def test_encode_decode():
    for testType in types:
        AssertError(TypeError, testType(b'abc').decode, None)
          
def test_bytes_subclass():
    for testType in types:
        class customstring(testType):
            def __str__(self):  return 'xyz'
            def __repr__(self): return 'foo'
            def __hash__(self): return 42
            def __mul__(self, count): return b'multiplied'
            def __add__(self, other): return 23
            def __len__(self): return 2300
            def __contains__(self, value): return False
        
        o = customstring(b'abc')
        AreEqual(str(o), "xyz")
        AreEqual(repr(o), "foo")
        AreEqual(hash(o), 42)
        AreEqual(o * 3, b'multiplied')
        AreEqual(o + b'abc', 23)
        AreEqual(len(o), 2300)
        AreEqual(b'a' in o, False)

def test_bytes_equals():
    for testType in types:
        x = testType(b'abc') == testType(b'abc')
        y = testType(b'def') == testType(b'def')
        AreEqual(id(x), id(y))
        AreEqual(id(x), id(True))
        
        x = testType(b'abc') != testType(b'abc')
        y = testType(b'def') != testType(b'def')
        AreEqual(id(x), id(y))
        AreEqual(id(x), id(False))
        
        x = testType(b'abcx') == testType(b'abc')
        y = testType(b'defx') == testType(b'def')
        AreEqual(id(x), id(y))
        AreEqual(id(x), id(False))
        
        x = testType(b'abcx') != testType(b'abc')
        y = testType(b'defx') != testType(b'def')
        AreEqual(id(x), id(y))
        AreEqual(id(x), id(True))

def test_bytes_dict():
    Assert('__init__' not in bytes.__dict__.keys())
    Assert('__init__' in bytearray.__dict__.keys())

    for testType in types:
        extra_str_dict_keys = [ "__cmp__", "isdecimal", "isnumeric", "isunicode"]  # "__radd__", 
        
        #It's OK that __getattribute__ does not show up in the __dict__.  It is
        #implemented.
        Assert(hasattr(testType, "__getattribute__"), str(testType) + " has no __getattribute__ method")
        
        for temp_key in extra_str_dict_keys:
            Assert(not temp_key in testType.__dict__.keys())

def test_bytes_to_numeric():
    for testType in types:
        class substring(testType):
            def __int__(self): return 1
            def __complex__(self): return 1j
            def __float__(self): return 1.0
            def __long__(self): return 1L
        
        class myfloat(float): pass
        class mylong(long): pass
        class myint(int): pass
        class mycomplex(complex): pass
        
        v = substring(b"123")        
        
        AreEqual(long(v), 1L)
        
        # str in 2.6 still supports this, but not in 3.0, we have the 3.0 behavior.
        if not is_cli and testType == bytes:
            AreEqual(complex(v), 123 + 0j)
            AreEqual(float(v), 123.0)
            
            AreEqual(mycomplex(v), 123 + 0j)
            AreEqual(myfloat(v), 123.0)
        else:
            AreEqual(complex(v), 1j)
            AreEqual(float(v), 1.0)

            AreEqual(mycomplex(v), 1j)
            AreEqual(myfloat(v), 1.0)
            AreEqual(type(myfloat(v)), myfloat)
        
        class substring(testType): pass
        
        v = substring(b"123")
        
        AreEqual(long(v), 123L)
        AreEqual(int(v), 123)
        AreEqual(float(v), 123.0)
        
        AreEqual(mylong(v), 123L)
        AreEqual(type(mylong(v)), mylong)
        AreEqual(myint(v), 123)
        AreEqual(type(myint(v)), myint)

        if testType == str:
            # 2.6 allows this, 3.0 disallows this.
            AreEqual(complex(v), 123+0j)
            AreEqual(mycomplex(v), 123+0j)
        else:
            AssertError(TypeError, complex, v)
            AssertError(TypeError, mycomplex, v)

def test_compares():
    a = b'A'
    b = b'B'
    bb = b'BB'
    aa = b'AA'
    ab = b'AB'
    ba = b'BA'
    
    for testType in types:
        for otherType in types:
            AreEqual(testType(a) > otherType(b), False)
            AreEqual(testType(a) < otherType(b), True)
            AreEqual(testType(a) <= otherType(b), True)
            AreEqual(testType(a) >= otherType(b), False)
            AreEqual(testType(a) == otherType(b), False)
            AreEqual(testType(a) != otherType(b), True)
            
            AreEqual(testType(b) > otherType(a), True)
            AreEqual(testType(b) < otherType(a), False)
            AreEqual(testType(b) <= otherType(a), False)
            AreEqual(testType(b) >= otherType(a), True)
            AreEqual(testType(b) == otherType(a), False)
            AreEqual(testType(b) != otherType(a), True)

            AreEqual(testType(a) > otherType(a), False)
            AreEqual(testType(a) < otherType(a), False)
            AreEqual(testType(a) <= otherType(a), True)
            AreEqual(testType(a) >= otherType(a), True)
            AreEqual(testType(a) == otherType(a), True)
            AreEqual(testType(a) != otherType(a), False)
            
            AreEqual(testType(aa) > otherType(b), False)
            AreEqual(testType(aa) < otherType(b), True)
            AreEqual(testType(aa) <= otherType(b), True)
            AreEqual(testType(aa) >= otherType(b), False)
            AreEqual(testType(aa) == otherType(b), False)
            AreEqual(testType(aa) != otherType(b), True)
            
            AreEqual(testType(bb) > otherType(a), True)
            AreEqual(testType(bb) < otherType(a), False)
            AreEqual(testType(bb) <= otherType(a), False)
            AreEqual(testType(bb) >= otherType(a), True)
            AreEqual(testType(bb) == otherType(a), False)
            AreEqual(testType(bb) != otherType(a), True)

            AreEqual(testType(ba) > otherType(b), True)
            AreEqual(testType(ba) < otherType(b), False)
            AreEqual(testType(ba) <= otherType(b), False)
            AreEqual(testType(ba) >= otherType(b), True)
            AreEqual(testType(ba) == otherType(b), False)
            AreEqual(testType(ba) != otherType(b), True)
            
            AreEqual(testType(ab) > otherType(a), True)
            AreEqual(testType(ab) < otherType(a), False)
            AreEqual(testType(ab) <= otherType(a), False)
            AreEqual(testType(ab) >= otherType(a), True)
            AreEqual(testType(ab) == otherType(a), False)
            AreEqual(testType(ab) != otherType(a), True)
            
            AreEqual(testType(ab) == [], False)
            
            AreEqual(testType(a) > None, True)
            AreEqual(testType(a) < None, False)
            AreEqual(testType(a) <= None, False)
            AreEqual(testType(a) >= None, True)
            AreEqual(None > testType(a), False)
            AreEqual(None < testType(a), True)
            AreEqual(None <= testType(a), True)
            AreEqual(None >= testType(a), False)

            
def test_bytearray():
    AssertError(TypeError, hash, bytearray(b'abc'))
    AssertError(TypeError, bytearray(b'').__setitem__, None, b'abc')
    AssertError(TypeError, bytearray(b'').__delitem__, None)
    
    
    def f():
        x = bytearray(b'abc')
        x[0:2] = [1j]
    AssertError(TypeError, f)
    
    x = bytearray(b'abc')
    x[0:1] = [ord('d')]
    AreEqual(x, b'dbc')
    
    x = bytearray(b'abc')
    x[0:3] = x
    AreEqual(x, b'abc')
    
    x = bytearray(b'abc')
    
    del x[0]
    AreEqual(x, b'bc')
    
    x = bytearray(b'abc')
    x += b'foo'
    AreEqual(x, b'abcfoo')
    
    x = bytearray(b'abc')
    x += bytearray(b'foo')
    AreEqual(x, b'abcfoo')

    x = bytearray(b'abc')
    x *= 2
    AreEqual(x, b'abcabc')
    
    tests = ( ((0, 3, None), b'abc', b''), 
              ((0, 2, None), b'abc', b'c'), 
              ((4, 0, 2),    b'abc', b'abc'), 
              ((3, 0, 2),    b'abc', b'abc'), 
              ((3, 0, -2),   b'abc', b'ab'), 
              ((0, 3, 1),    b'abc', b''), 
              ((0, 2, 1),    b'abc', b'c'), 
              ((0, 3, 2),    b'abc', b'b'), 
              ((0, 2, 2),    b'abc', b'bc'), 
              ((0, 3, -1),   b'abc', b'abc'), 
              ((0, 2, -1),   b'abc', b'abc'), 
              ((3, 0, -1),   b'abc', b'a'), 
              ((2, 0, -1),   b'abc', b'a'), 
              ((4, 2, -1),   b'abcdef', b'abcf'),
            )

    for indexes, input, result in tests:
        x = bytearray(input)
        if indexes[2] == None:
            del x[indexes[0] : indexes[1]]
            AreEqual(x, result)
        else:
            del x[indexes[0] : indexes[1] : indexes[2]]
            AreEqual(x, result)     
    
    for setval in [b'bar', bytearray(b'bar'), [b'b', b'a', b'r'], (b'b', b'a', b'r')]:
        x = bytearray(b'abc')
        x[0:3] = setval
        AreEqual(x, b'bar')
        
        x = bytearray(b'abc')
        x[1:4] = setval
        AreEqual(x, b'abar')

        x = bytearray(b'abc')
        x[0:2] = setval
        AreEqual(x, b'barc')
        
        x = bytearray(b'abc')
        x[4:0:2] = setval[-1:-1]
        AreEqual(x, b'abc')
        
        x = bytearray(b'abc')
        x[3:0:2] = setval[-1:-1]
        AreEqual(x, b'abc')
        
        x = bytearray(b'abc')
        x[3:0:-2] = setval[-1:-1]
        AreEqual(x, b'ab')
        
        x = bytearray(b'abc')
        x[3:0:-2] = setval[0:-2]
        AreEqual(x, b'abb')
        
        x = bytearray(b'abc')
        x[0:3:1] = setval
        AreEqual(x, b'bar')
        
        x = bytearray(b'abc')
        x[0:2:1] = setval
        AreEqual(x, b'barc')
        
        x = bytearray(b'abc')
        x[0:3:2] = setval[0:-1]
        AreEqual(x, b'bba')
        
        x = bytearray(b'abc')
        x[0:2:2] = setval[0:-2]
        AreEqual(x, b'bbc')
        
        x = bytearray(b'abc')
        x[0:3:-1] = setval[-1:-1]
        AreEqual(x, b'abc')
        
        x = bytearray(b'abc')
        x[0:2:-1] = setval[-1:-1]
        AreEqual(x, b'abc')
        
        x = bytearray(b'abc')
        x[3:0:-1] = setval[0:-1]
        AreEqual(x, b'aab')
        
        x = bytearray(b'abc')
        x[2:0:-1] = setval[0:-1]
        AreEqual(x, b'aab')
        
        x = bytearray(b'abcdef')
        def f():x[0:6:2] = b'a'
        AssertError(ValueError, f)

def test_bytes():
    AreEqual(hash(b'abc'), hash(b'abc'))
    AreEqual(b'abc', B'abc')

def test_operators():
    for testType in types:
        AssertError(TypeError, lambda : testType(b'abc') * None)
        AssertError(TypeError, lambda : testType(b'abc') + None)
        AssertError(TypeError, lambda : None * testType(b'abc'))
        AssertError(TypeError, lambda : None + testType(b'abc'))
        AreEqual(testType(b'abc') * 2, b'abcabc')
        
        if is_cli or testType == bytearray:
            # we match the 3.0 behavior for bytes.__getitem__
            AreEqual(testType(b'abc')[0], ord('a'))
        
        for otherType in types:
            
            AreEqual(testType(b'abc') + otherType(b'def'), b'abcdef')
            resType = type(testType(b'abc') + otherType(b'def'))
            if testType == bytearray or otherType == bytearray:
                AreEqual(resType, bytearray)
            else:
                AreEqual(resType, bytes)
    
    x = b'abc'
    AreEqual(x * 1, x)
    AreEqual(1 * x, x)
    AreEqual(id(x), id(x * 1))    
    AreEqual(id(x), id(1 * x))    

    x = bytearray(b'abc')
    AreEqual(x * 1, x)
    AreEqual(1 * x, x)
    Assert(id(x) != id(x * 1))    
    Assert(id(x) != id(1 * x))    

def test_init():
    for testType in types:
        if testType != str:  # skip on Cpy 2.6 for str type
            AssertError(TypeError, testType, None, 'ascii')
            AssertError(TypeError, testType, u'abc', None)
            AreEqual(testType(u'abc', 'ascii'), b'abc')

def test_slicing():
    for testType in types:
        AreEqual(testType(b'abc')[0:3], b'abc')
        AreEqual(testType(b'abc')[0:2], b'ab')
        AreEqual(testType(b'abc')[3:0:2], b'')
        AreEqual(testType(b'abc')[3:0:2], b'')
        AreEqual(testType(b'abc')[3:0:-2], b'c')
        AreEqual(testType(b'abc')[3:0:-2], b'c')
        AreEqual(testType(b'abc')[0:3:1], b'abc')
        AreEqual(testType(b'abc')[0:2:1], b'ab')
        AreEqual(testType(b'abc')[0:3:2], b'ac')
        AreEqual(testType(b'abc')[0:2:2], b'a')
        AreEqual(testType(b'abc')[0:3:-1], b'')
        AreEqual(testType(b'abc')[0:2:-1], b'')
        AreEqual(testType(b'abc')[3:0:-1], b'cb')
        AreEqual(testType(b'abc')[2:0:-1], b'cb')
        
        AssertError(TypeError, testType(b'abc').__getitem__, None)

@skip("win32")
def test_zzz_cli_features():
    import System
    import clr
    clr.AddReference('Microsoft.Scripting')
    import Microsoft
    
    for testType in types:
        AreEqual(testType(b'abc').Count, 3)
        AreEqual(bytearray(b'abc').Contains(ord('a')), True)
        AreEqual(list(System.Collections.IEnumerable.GetEnumerator(bytearray(b'abc'))), [ord('a'), ord('b'), ord('c')])
        AreEqual(testType(b'abc').IndexOf(ord('a')), 0)
        AreEqual(testType(b'abc').IndexOf(ord('d')), -1)
        
        myList = System.Collections.Generic.List[System.Byte]()
        myList.Add(ord('a'))
        myList.Add(ord('b'))
        myList.Add(ord('c'))
        
        AreEqual(testType(b'').join([myList]), b'abc')

    # bytearray
    AreEqual(Microsoft.Scripting.IValueEquality.ValueEquals(bytearray(b'abc'), 'abc'), False)
        
    AreEqual(b'abc'.IsReadOnly, True)
    AreEqual(bytearray(b'abc').IsReadOnly, False)
        
    AreEqual(bytearray(b'abc').Remove(ord('a')), True)
    AreEqual(bytearray(b'abc').Remove(ord('d')), False)
    
    x = bytearray(b'abc')
    x.Clear()
    AreEqual(x, b'')
    
    x.Add(ord('a'))
    AreEqual(x, b'a')
    
    AreEqual(x.IndexOf(ord('a')), 0)
    AreEqual(x.IndexOf(ord('b')), -1)
    
    x.Insert(0, ord('b'))
    AreEqual(x, b'ba')
    
    x.RemoveAt(0)
    AreEqual(x, b'a')
    
    System.Collections.Generic.IList[System.Byte].__setitem__(x, 0, ord('b'))
    AreEqual(x, b'b')

    # bytes    
    AssertError(System.InvalidOperationException, b'abc'.Remove, ord('a'))
    AssertError(System.InvalidOperationException, b'abc'.Remove, ord('d'))    
    AssertError(System.InvalidOperationException, b'abc'.Clear)    
    AssertError(System.InvalidOperationException, b'abc'.Add, ord('a'))    
    AssertError(System.InvalidOperationException, b'abc'.Insert, 0, ord('b'))    
    AssertError(System.InvalidOperationException, b'abc'.RemoveAt, 0)    
    AssertError(System.InvalidOperationException, System.Collections.Generic.IList[System.Byte].__setitem__, b'abc', 0, ord('b'))

run_test(__name__)
