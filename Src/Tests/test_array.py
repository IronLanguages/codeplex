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

if is_cli:
    import System

    array1 = System.Array.CreateInstance(int, 2)
    for i in range(2): array1[i] = i * 10
        
    array2 = System.Array.CreateInstance(int, 4)
    for i in range(2, 6): array2[i - 2] = i * 10

    array3 = System.Array.CreateInstance(float, 3)
    array3[0] = 2.1
    array3[1] = 3.14
    array3[2] = 0.11
    System.Array.__setitem__(array3, 2, 0.14)
    AreEqual(System.Array.__getitem__(array3, 1), 3.14)
    AreEqual([x for x in System.Array.__getitem__(array3, slice(2))], [2.1, 3.14])

    array4 = System.Array.CreateInstance(int, 2, 2)
    array4[0, 1] = 1
    AreEqual(repr(array4), "System.Int32[,](\n0, 1\n0, 0)")

    array5 = System.Array.CreateInstance(object, 2, 2, 2)
    array5[0, 1, 1] = int
    AreEqual(repr(array5), "System.Object[,,]( Multi-dimensional array )")

    AssertError(TypeError, lambda : array5['s'])
    def f1(): array5[0, 1] = 0
    AssertError(ValueError, f1)
    def f2(): array5['s'] = 0
    AssertError(TypeError, f2)

    for f in (
        lambda a, b : System.Array.__add__(a, b), 
        lambda a, b : a + b
        ) : 
        
        temp = System.Array.__add__(array1, array2)
        result = f(array1, array2)
        
        for i in range(6): AreEqual(i * 10, result[i])
        AreEqual(repr(result), "System.Int32[](0, 10, 20, 30, 40, 50)")
        
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

    ## slice fun
    array1 = System.Array.CreateInstance(int, 20)
    for i in range(20): array1[i] = i * i
    array1[::2] = [x * 2 for x in range(10)]

    for i in range(0, 20, 2):
        AreEqual(array1[i], i) 
    for i in range(1, 20, 2):
        AreEqual(array1[i], i * i) 

    def f(): array1[::2] = [x * 2 for x in range(11)]
    AssertError(ValueError, f)    
    
    ## creation
    t = System.Array
    ti = type(System.Array.CreateInstance(int, 1))

    AssertError(TypeError, t, [1, 2])
    for x in (ti([1,2]), t[int]([1,2]), ti([1.5, 2.3])):
        AreEqual([i for i in x], [1,2])
        t.Reverse(x)
        AreEqual([i for i in x], [2, 1])    
    
#################################################################    
# check arrays w/ non-zero lower bounds

#single dimension

def ArrayEqual(a,b):
    AreEqual(a.Length, b.Length)
    for x in xrange(a.Length):
        AreEqual(a[x], b[x])
        
a = System.Array.CreateInstance(int, (5,), (5,))
for i in xrange(5): a[i] = i

ArrayEqual(a[:2], System.Array[int]((0,1)))
ArrayEqual(a[2:], System.Array[int]( (2,3,4)))
ArrayEqual(a[2:4], System.Array[int]((2,3)))
AreEqual(a[-1], 4)

x = repr(a)
AreEqual(x, 'System.Int32[*](0, 1, 2, 3, 4)')

a = System.Array.CreateInstance(int, (5,), (5,))
b = System.Array.CreateInstance(int, (5,), (5,))

ArrayEqual(a,b)


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
    