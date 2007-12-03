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

from lib.assert_util import *

import struct

def test_sanity():
    mapping = { 
        'c': 'a',
        'b': ord('b'),
        'B': ord('c'),
        'h': -123,
        'H': 123,
        'i': -12345,
        'l': -123456789, 
        'I': 12345,
        'L': 123456789,
        'q': -1000000000,
        'Q': 1000000000,
        'f': 3.14,
        'd': -0.3439, 
        '6s': 'string',
        '15p': 'another string'
        }
    
    for (k, v) in mapping.iteritems():
        s = struct.pack(k, v)
        v2 = struct.unpack(k, s)
        
        if isinstance(v, float):
            AlmostEqual(v, v2[0])
        else:
            AreEqual(v, v2[0])

    AreEqual(struct.pack(' c\t', 'a'), 'a') 
    
def test_padding_len():
    AreEqual(struct.unpack('4xi','\x00\x01\x02\x03\x01\x00\x00\x00'), (1,))

#CodePlex Work Item 3092
def test_3092():
    #TODO: need far more testing on this!

    for format in [ "i", "I", "l", "L"]:    
        mem = "\x01\x00\x00\x00" * 8
        AreEqual(len(mem), 32)
    
        fmt = "<8" + format
        AreEqual(struct.calcsize(fmt), 32)
        AreEqual(struct.unpack(fmt, mem), (1,)*8)
    
        fmt = "<2" + format + "4x5" + format
        AreEqual(struct.calcsize(fmt), 32)
        AreEqual(struct.unpack(fmt, mem), (1,)*7)
        
        fmt = "<" + format + format + "4x5" + format
        AreEqual(struct.calcsize(fmt), 32)
        AreEqual(struct.unpack(fmt, mem), (1,)*7)

        fmt = "<32x"
        AreEqual(struct.calcsize(fmt), 32)
        AreEqual(struct.unpack(fmt, mem), ())
    
        fmt = "<28x" + format
        AreEqual(struct.calcsize(fmt), 32)
        AreEqual(struct.unpack(fmt, mem), (1,))
    


def test_negative():
    AssertError(struct.error, struct.pack, 'x', 1)
    AssertError(struct.error, struct.unpack, 'hh', struct.pack('h', 1))
    
    AssertError(struct.error, struct.pack, 'a', 1)
    
    # BUG: 1033
    # such chars should be in the leading position only
    
    for x in '=@<>!':
        AssertError(struct.error, struct.pack, 'h'+x+'h', 1, 2)   

    AssertError(struct.error, struct.pack, 'c', 300) 
    
run_test(__name__)