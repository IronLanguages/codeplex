#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

from lib.assert_util import *

class It:
    x = 0
    a = ()
    def __init__(self, a):
        self.x = 0
        self.a = a
    def next(self):
        if self.x <= 9:
            self.x = self.x+1
            return self.a[self.x-1]
        else:
            raise StopIteration
    def __iter__(self):
        return self


class Iterator:
    x = 0
    a = (1,2,3,4,5,6,7,8,9,0)
    def __iter__(self):
        return It(self.a)

class Indexer:
    a = (1,2,3,4,5,6,7,8,9,0)
    def __getitem__(self, i):
        if i < len(self.a):
            return self.a[i]
        else:
            raise IndexError

i = Iterator()
for j in i:
    Assert(j in i)

Assert(1 in i)
Assert(2 in i)
Assert(not (10 in i))

i = Indexer()
for j in i:
    Assert(j in i)

Assert(1 in i)
Assert(2 in i)
Assert(not (10 in i))


# Testing the iter(o,s) function

class Iter:
    x = [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20]
    index = -1

it = Iter()

def f():
    it.index += 1
    return it.x[it.index]


y = []

for i in iter(f, 14):
    y.append(i)

Assert(y == [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13])

y = ['1']
y += Iterator()
Assert(y == ['1', 1, 2, 3, 4, 5, 6, 7, 8, 9, 0])
y = ['1']
y += Indexer()
Assert(y == ['1', 1, 2, 3, 4, 5, 6, 7, 8, 9, 0])

AssertErrorWithMessages(TypeError, "iter() takes at least 1 argument (0 given)", 
                                   "iter expected at least 1 arguments, got 0", iter)


