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
## Test array support by IronPython (System.Array)
##

from iptest.assert_util import *

if not sys.platform=="win32":
    import System

import array

@skip("win32")
def test_sanity():
    # 1-dimension array
    array1 = System.Array.CreateInstance(int, 2)
    for i in range(2): array1[i] = i * 10
    
    AssertError(IndexError, lambda: array1[2])
        
    array2 = System.Array.CreateInstance(int, 4)
    for i in range(2, 6): array2[i - 2] = i * 10

    array3 = System.Array.CreateInstance(float, 3)
    array3[0] = 2.1
    array3[1] = 3.14
    array3[2] = 0.11

    ## __setitem__/__getitem__
    System.Array.__setitem__(array3, 2, 0.14)
    AreEqual(System.Array.__getitem__(array3, 1), 3.14)
    AreEqual([x for x in System.Array.__getitem__(array3, slice(2))], [2.1, 3.14])

    ## __repr__

    # 2-dimension array
    array4 = System.Array.CreateInstance(int, 2, 2)
    array4[0, 1] = 1
    Assert(repr(array4).startswith("<2 dimensional Array[int] at"), "bad repr for 2-dimensional array")

    # 3-dimension array
    array5 = System.Array.CreateInstance(object, 2, 2, 2)
    array5[0, 1, 1] = int
    Assert(repr(array5).startswith("<3 dimensional Array[object] at "), "bad repr for 3-dimensional array")

    ## index access
    AssertError(TypeError, lambda : array5['s'])
    def f1(): array5[0, 1] = 0
    AssertError(ValueError, f1)
    def f2(): array5['s'] = 0
    AssertError(TypeError, f2)

    ## __add__/__mul__
    for f in (
        lambda a, b : System.Array.__add__(a, b),
        lambda a, b : a + b
        ) :
        
        temp = System.Array.__add__(array1, array2)
        result = f(array1, array2)
        
        for i in range(6): AreEqual(i * 10, result[i])
        AreEqual(repr(result), "Array[int]((0, 10, 20, 30, 40, 50))")
        
        result = f(array1, array3)
        AreEqual(len(result), 2 + 3)
        AreEqual([x for x in result], [0, 10, 2.1, 3.14, 0.14])
        
        AssertError(NotImplementedError, f, array1, array4)
        
    for f in [
        lambda a, x: System.Array.__mul__(a, x),
        lambda a, x: array1 * x
        ]:

        AreEqual([x for x in f(array1, 4)], [0, 10, 0, 10, 0, 10, 0, 10])
        AreEqual([x for x in f(array1, 5)], [0, 10, 0, 10, 0, 10, 0, 10, 0, 10])
        AreEqual([x for x in f(array1, 0)], [])
        AreEqual([x for x in f(array1, -10)], [])

@skip("win32")
def test_slice():
    array1 = System.Array.CreateInstance(int, 20)
    for i in range(20): array1[i] = i * i
    
    # positive
    array1[::2] = [x * 2 for x in range(10)]

    for i in range(0, 20, 2):
        AreEqual(array1[i], i)
    for i in range(1, 20, 2):
        AreEqual(array1[i], i * i)

    # negative: not-same-length
    def f(): array1[::2] = [x * 2 for x in range(11)]
    AssertError(ValueError, f)

@skip("win32")
def test_creation():
    t = System.Array
    ti = type(System.Array.CreateInstance(int, 1))

    AssertError(TypeError, t, [1, 2])
    for x in (ti([1,2]), t[int]([1, 2]), ti([1.5, 2.3])):
        AreEqual([i for i in x], [1, 2])
        t.Reverse(x)
        AreEqual([i for i in x], [2, 1])


def _ArrayEqual(a,b):
    AreEqual(a.Length, b.Length)
    for x in xrange(a.Length):
        AreEqual(a[x], b[x])
    
## public static Array CreateInstance (
##    Type elementType,
##    int[] lengths,
##    int[] lowerBounds
##)

@skip('silverlight', 'win32')
def test_nonzero_lowerbound():
    a = System.Array.CreateInstance(int, (5,), (5,))
    for i in xrange(5): a[i] = i
    
    _ArrayEqual(a[:2], System.Array[int]((0,1)))
    _ArrayEqual(a[2:], System.Array[int]((2,3,4)))
    _ArrayEqual(a[2:4], System.Array[int]((2,3)))
    AreEqual(a[-1], 4)

    AreEqual(repr(a), 'Array[int]((0, 1, 2, 3, 4))')

    a = System.Array.CreateInstance(int, (5,), (15,))
    b = System.Array.CreateInstance(int, (5,), (20,))
    _ArrayEqual(a,b)

    ## 5-dimension
    a = System.Array.CreateInstance(int, (2,2,2,2,2), (1,2,3,4,5))
    AreEqual(a[0,0,0,0,0], 0)

    for i in range(5):
        index = [0,0,0,0,0]
        index[i] = 1
        
        a[index[0], index[1], index[2], index[3], index[4]] = i
        AreEqual(a[index[0], index[1], index[2], index[3], index[4]], i)
        
    for i in range(5):
        index = [0,0,0,0,0]
        index[i] = 0
        
        a[index[0], index[1], index[2], index[3], index[4]] = i
        AreEqual(a[index[0], index[1], index[2], index[3], index[4]], i)

    def sliceArray(arr, index):
        arr[:index]

    def sliceArrayAssign(arr, index, val):
        arr[:index] = val

    AssertError(NotImplementedError, sliceArray, a, 1)
    AssertError(NotImplementedError, sliceArray, a, 200)
    AssertError(NotImplementedError, sliceArray, a, -200)
    AssertError(NotImplementedError, sliceArrayAssign, a, -200, 1)
    AssertError(NotImplementedError, sliceArrayAssign, a, 1, 1)

@skip("win32")
def test_array_type():
    
    def type_helper(array_type, instance):
        #create the array type
        AT = System.Array[array_type]
        
        a0 = AT([])
        a1 = AT([instance])
        a2 = AT([instance, instance])
                
        a_normal = System.Array.CreateInstance(array_type, 3)
        Assert(str(AT)==str(type(a_normal)))
        for i in xrange(3):
            a_normal[i] = instance
            Assert(str(AT)==str(type(a_normal)))
   
        a_multi  = System.Array.CreateInstance(array_type, 2, 3)
        Assert(str(AT)==str(type(a_multi)))
        for i in xrange(2):
            for j in xrange(3):
                Assert(str(AT)==str(type(a_multi)))
                a_multi[i, j]=instance
                
        Assert(str(AT)==str(type(a0)))
        Assert(str(AT)==str(type(a0[0:])))
        Assert(str(AT)==str(type(a0[:0])))
        Assert(str(AT)==str(type(a1)))
        Assert(str(AT)==str(type(a1[1:])))
        Assert(str(AT)==str(type(a1[:0])))
        Assert(str(AT)==str(type(a_normal)))
        Assert(str(AT)==str(type(a_normal[:0])))
        Assert(str(AT)==str(type(a_normal[3:])))
        Assert(str(AT)==str(type(a_normal[4:])))
        Assert(str(AT)==str(type(a_normal[1:])))
        Assert(str(AT)==str(type(a_normal[1:1:50])))
        Assert(str(AT)==str(type(a_multi)))
        def silly(): a_multi[0:][1:0]
        AssertError(NotImplementedError, silly)
        Assert(str(AT)==str(type((a0+a1)[:0])))
            
    type_helper(int, 0)
    type_helper(int, 1)
    type_helper(int, 100)
    type_helper(bool, False)
    type_helper(bool, True)
    #type_helper(bool, 1)
    type_helper(long, 0L)
    type_helper(long, 1L)
    type_helper(long, 100L)
    type_helper(float, 0.0)
    type_helper(float, 1.0)
    type_helper(float, 3.14)
    type_helper(str, "")
    type_helper(str, " ")
    type_helper(str, "abc")

def test_array_array_I():
    for x in [  0, 1, 2,
                (2**8)-2, (2**8)-1, (2**8), (2**8)+1, (2**8)+2,
                (2**16)-2, (2**16)-1, (2**16), (2**16)+1, (2**16)+2,
                (2**32)-2, (2**32)-1,
                ]:
                
        temp_array1 = array.array('I', [x])
        AreEqual(temp_array1[0], x)
        
        temp_array1 = array.array('I', [x, x])
        AreEqual(temp_array1[0], x)
        AreEqual(temp_array1[1], x)
        
    for x in [  (2**32), (2**32)+1, (2**32)+2 ]:
        AssertError(OverflowError, array.array, 'I', [x])

def test_array_array_c():
    a = array.array('c', "stuff")
    a[1:0] = a
    b = array.array('c', "stuff"[:1] + "stuff" + "stuff"[1:])
    AreEqual(a, b)

def test_array_array_L():
    a = array.array('L', "\x12\x34\x45\x67")
    AreEqual(1, len(a))
    AreEqual(1732588562, a[0])

def test_array_array_B():
    a = array.array('B', [0]) * 2L
    AreEqual(2, len(a))
    AreEqual("array('B', [0, 0])", str(a))
    
    AreEqual(array.array('b', 'foo'), array.array('b', [102, 111, 111]))


def test_cp9348():
    test_cases = {  ('c', "a") : "array('c', 'a')",
                    ('b', "a") : "array('b', [97])",
                    ('B', "a") : "array('B', [97])",
                    #('u', u"a") : "array('u', u'a')", #CodePlex 19215
                    ('h', "\x12\x34") : "array('h', [13330])",
                    ('H', "\x12\x34") : "array('H', [13330])",
                    ('i', "\x12\x34\x45\x67") : "array('i', [1732588562])",
                    #('I', "\x12\x34\x45\x67") : "array('I', [1732588562L])", #CodePlex 19216
                    #('l', "\x12\x34\x45\x67") : "array('l', [1732588562])", #CodePlex 19217
                    ('L', "\x12\x34\x45\x67") : "array('L', [1732588562L])",
                    ('f', "\x12\x34\x45\x67") : "array('f', [9.3126672485384569e+23])",
                    ('d', "\x12\x34\x45\x67\x12\x34\x45\x67") : "array('d', [2.9522485325887698e+189])",
                }
    for key in test_cases.keys():
        type_code, param = key
        temp_val = array.array(type_code, param)
        AreEqual(str(temp_val), test_cases[key]) 

def test_cp8736():
    a = array.array('b')
    for i in [-1, -2, -3, -(2**8), -1000, -(2**16)+1, -(2**16), -(2**16)-1, -(2**64)]:
        a[:i] = a
        AreEqual(str(a), "array('b')")

    a2 = array.array('b', 'a')
    a2[:-1] = a2
    AreEqual(str(a2), "array('b', [97, 97])")
    a2[:-(2**64)-1] = a2
    if is_cli or is_silverlight:
        print "CodePlex 8736"
    else:        
        AreEqual(str(a2), "array('b', [97, 97, 97, 97])")  
    
def test_array_typecode():
    x = array.array('i')
    AreEqual(type(x.typecode), str)

def test_reduce():
    x = array.array('i', [1,2,3])
    AreEqual(repr(x.__reduce_ex__(1)), "(<type 'array.array'>, ('i', '\\x01\\x00\\x00\\x00\\x02\\x00\\x00\\x00\\x03\\x00\\x00\\x00'), None)")
    AreEqual(repr(x.__reduce_ex__()), "(<type 'array.array'>, ('i', '\\x01\\x00\\x00\\x00\\x02\\x00\\x00\\x00\\x03\\x00\\x00\\x00'), None)")
    AreEqual(repr(x.__reduce__()), "(<type 'array.array'>, ('i', '\\x01\\x00\\x00\\x00\\x02\\x00\\x00\\x00\\x03\\x00\\x00\\x00'), None)")

def test_copy():
    x = array.array('i', [1,2,3])
    y = x.__copy__()
    Assert(id(x) != id(y), "copy should copy")
    
    if is_cli or is_silverlight:
        #CodePlex 19200
        y = x.__deepcopy__()
    else:
        y = x.__deepcopy__(x)
    Assert(id(x) != id(y), "copy should copy")

def test_cp9350():
    for i in [1, 1L]:
        a = array.array('B', [0]) * i
        AreEqual(a, array.array('B', [0]))

    for i in [2, 2L]:
        a = array.array('B', [0]) * i
        AreEqual(a, array.array('B', [0, 0]))
    
    for i in [2**8, long(2**8)]:
        a = array.array('B', [1]) * i
        AreEqual(a, array.array('B', [1]*2**8))
    



run_test(__name__)
