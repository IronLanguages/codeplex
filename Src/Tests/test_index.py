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

import sys
import System
import System.Collections
from lib.assert_util import *
load_iron_python_test()
from IronPythonTest import *

x = System.Array.CreateInstance(System.String, 2)
x[0]="Hello"
x[1]="Python"
Assert(x[0] == "Hello")
Assert(x[1] == "Python")

x = System.Collections.Hashtable()
x["Hi"] = "Hello"
x[1] = "Python"
x[10,] = "Tuple Int"
x["String",] = "Tuple String"
x[2.4,] = "Tuple Double"

Assert(x["Hi"] == "Hello")
Assert(x[1] == "Python")
Assert(x[(10,)] == "Tuple Int")
Assert(x[("String",)] == "Tuple String")
Assert(x[(2.4,)] == "Tuple Double")

success=False
try:
    x[1,2] = 10
except TypeError, e:
    success=True
Assert(success)

x[(1,2)] = "Tuple key in hashtable"
Assert(x[1,2,] == "Tuple key in hashtable")

md = System.Array.CreateInstance(System.Int32, 2, 2, 2)

for i in range(2):
    for j in range(2):
        for k in range(2):
            md[i,j,k] = i+j+k

for i in range(2):
    for j in range(2):
        for k in range(2):
            Assert(md[i,j,k] == i+j+k)


d = dict()
d[1,2,3,4,5] = 12345
Assert(d[1,2,3,4,5] == d[(1,2,3,4,5)])
Assert(d[1,2,3,4,5] == 12345)
Assert(d[(1,2,3,4,5)] == 12345)


i = Indexable()

i[10] = "Hello Integer"
i["String"] = "Hello String"
i[2.4] = "Hello Double"

Assert(i[10] == "Hello Integer")
Assert(i["String"] == "Hello String")
Assert(i[2.4] == "Hello Double")

indexes = (10, "String", 2.4)
for a in indexes:
    for b in indexes:
        complicated = "Complicated " + str(a) + " " + str(b)
        i[a,b] = complicated
        Assert(i[a,b] == complicated)

x = PropertyAccessClass()
for i in range(3):
    Assert(x[i] == i)
    for j in range(3):
        x[i, j] = i + j
        Assert(x[i, j] == i + j)
        for k in range(3):
            x[i, j, k] = i + j + k
            Assert(x[i, j, k] == i + j + k)

x = MultipleIndexes()

def get_value(*i):
    value = ""
    append = False
    for v in i:
        if append:
            value = value + " : "
        value = value + str(v)
        append = True
    return value

def get_tuple_value(*i):
    return get_value("Indexing as tuple", *i)

def get_none(*i):
    return None

def verify_values(mi, gv, gtv):
    for i in i_idx:
        Assert(x[i] == gv(i))
        Assert(x[i,] == gtv(i))
        for j in j_idx:
            Assert(x[i,j] == gv(i,j))
            Assert(x[i,j,] == gtv(i,j))
            for k in k_idx:
                Assert(x[i,j,k] == gv(i,j,k))
                Assert(x[i,j,k,] == gtv(i,j,k))
                for l in l_idx:
                    Assert(x[i,j,k,l] == gv(i,j,k,l))
                    Assert(x[i,j,k,l,] == gtv(i,j,k,l))
                    for m in m_idx:
                        Assert(x[i,j,k,l,m] == gv(i,j,k,l,m))
                        Assert(x[i,j,k,l,m,] == gtv(i,j,k,l,m))

i_idx = ("Hi", 2.5, 34)
j_idx = (0, "*", "@")
k_idx = range(3)
l_idx = ("Sun", "Moon", "Star")
m_idx = ((9,8,7), (6,5,4,3,2), (4,))

for i in i_idx:
    x[i] = get_value(i)
    for j in j_idx:
        x[i,j] = get_value(i,j)
        for k in k_idx:
            x[i,j,k] = get_value(i,j,k)
            for l in l_idx:
                x[i,j,k,l] = get_value(i,j,k,l)
                for m in m_idx:
                    x[i,j,k,l,m] = get_value(i,j,k,l,m)

verify_values(x, get_value, get_none)

for i in i_idx:
    x[i,] = get_tuple_value(i)
    for j in j_idx:
        x[i,j,] = get_tuple_value(i,j)
        for k in k_idx:
            x[i,j,k,] = get_tuple_value(i,j,k)
            for l in l_idx:
                x[i,j,k,l,] = get_tuple_value(i,j,k,l)
                for m in m_idx:
                    x[i,j,k,l,m,] = get_tuple_value(i,j,k,l,m)

verify_values(x, get_value, get_tuple_value)


a = IndexableList()
for i in range(5): result = a.Add(i)

for i in range(5):
    AreEqual(a[str(i)], i)
    

#***** Above code are from 'Index' *****
