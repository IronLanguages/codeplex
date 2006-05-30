
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

l=['a','b','c']
l.extend(l)
Assert(l==['a','b','c','a','b','c'])

if is_cli: 
    import clr
    x = [1,2,3]
    y = []
    xenum = iter(x)
    while xenum.MoveNext():
        y.append(xenum.Current)
    AreEqual(x, y)

#####################################################################################
# Disallow assignment to empty list
y = []
AssertError(SyntaxError, compile, "[] = y", "Error", "exec")
AssertError(SyntaxError, compile, "[], t = y, 0", "Error", "exec")
AssertError(SyntaxError, compile, "[[[]]]=[[y]]", "Error", "exec")
del y

#####################################################################################
# Disallow unequal unpacking assignment

listOfSize2 = [1, 2]

def f1(): [a, b, c] = listOfSize2
def f2(): del a
def f3(): [a] = listOfSize2

AssertError(ValueError, f1)
AssertError(NameError, f2)
AssertError(ValueError, f3)
AssertError(NameError, f2)

[a, [b, c]] = [listOfSize2, listOfSize2]
AreEqual(a, listOfSize2)
AreEqual(b, 1)
AreEqual(c, 2)
del a, b, c

[[a, b], c] = (listOfSize2, listOfSize2)
AreEqual(a, 1)
AreEqual(b, 2)
AreEqual(c, listOfSize2)
del a, b, c, listOfSize2

#####################################################################################
# Disallow assignment to empty tuple
y = ()
AssertError(SyntaxError, compile, "() = y", "Error", "exec")
AssertError(SyntaxError, compile, "(), t = y, 0", "Error", "exec")
AssertError(SyntaxError, compile, "((()))=((y))", "Error", "exec")
del y

#####################################################################################
# Disallow unequal unpacking assignment

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
del a, b, c, tupleOfSize2

#####################################################################################
#verify repr and print have the same result for a
# recursive list

fo = open("testfile.txt", "wb")
a = list('abc')
a.append(a)
print >>fo, a,
fo.close()

fo = open("testfile.txt", "rb")
Assert(fo.read() == repr(a))

try:
    import nt
    nt.unlink("testfile.txt")
except:
    pass

# named params passed to sort
LExpected = ['A', 'b', 'c', 'D']
L = ['D', 'c', 'b', 'A']
L.sort(key=lambda x: x.lower())
Assert(L == LExpected)

aList = [['a']]
anItem = ['a']

AreEqual( aList.index(anItem), 0 )

Assert(anItem in aList)

l = [1, 2, 3]
l2 = l[:]
l.sort(lambda x, y: x > y)
AreEqual(l, l2)
l.sort(lambda x, y: x > y)
AreEqual(l, l2)

    
if is_cli:
    def contains_all_1s(x):
        '''check the return value are 11111 or similar'''
        if type(x) == tuple: 
            x = x[0]
        s = str(x)
        AreEqual(s.count("1"), len(s))
        
    def do_something(thetype, pl, cl, check_func):
        pt = thetype(pl)
        pt.AddRemove()
        
        ct = thetype(cl)
        ct.AddRemove()
        
        check_func()
        
        x = pt.Inspect()
        y = ct.Inspect()
        contains_all_1s(x)
        contains_all_1s(y)
        AreEqual(x, y)
        
        AreEqual(pt.Loop(), ct.Loop())
        check_func()
        
    load_iron_python_test()
    import System
    from IronPythonTest import *
    
    # test ListWrapperForIList 
    pl = range(40)
    cl = System.Collections.Generic.List[int]()
    for x in pl: cl.Add(x)
    
    def check_content():
        for x, y in zip(cl, pl): AreEqual(x, y)
        
    do_something(UsePythonListAsList, pl, cl, check_content)
           
    # test ListWrapperForArrayList
    pl = [check_content, 1, type, "string"]
    cl = System.Collections.ArrayList()
    for x in pl: result = cl.Add(x)

    do_something(UsePythonListAsArrayList, pl, cl, check_content)
    
    # test DictWrapperForIDict 
    pl = {"redmond" : 10, "seattle" : 20}
    cl = System.Collections.Generic.Dictionary[str, int]()
    for x, y in pl.iteritems(): cl.Add(x, y)
    
    def check_content():
        for x, y in zip(cl, pl.iteritems()): 
            AreEqual(x.Key, y[0])
            AreEqual(x.Value, y[1])
  
    do_something(UsePythonDictAsDictionary, pl, cl, check_content)

    # test DictWrapperForHashtable 
    pl = {"redmond" : 10, "seattle" : 20}
    cl = System.Collections.Hashtable()
    for x, y in pl.iteritems(): cl.Add(x, y)

    def check_content():
        AreEqual(len(pl), len(cl))
        for x in cl.Keys: 
            AreEqual(cl[x], pl[x])
   
    do_something(UsePythonDictAsHashtable, cl, pl, check_content)


######################################################################
# Verify behavior of index when the list changes...


class clears(object):
    def __eq__(self, other):
            global hitCount
            hitCount = hitCount + 1
            del a[:]
            return False

class appends(object):
    def __eq__(self, other):
            global hitCount
            hitCount = hitCount + 1
            a.append(self)
            return False
            
a = [clears(), clears(),clears(),clears(),clears()]
hitCount = 0
AssertError(ValueError, a.index, 23)
    
AreEqual(hitCount, 1)       # shoudl stop after the first equality check


a = [appends(), appends(), appends()]
hitCount = 0
AssertError(ValueError, a.index, 2)
AreEqual(hitCount, 3)       # should have only checked existing items



# verify __getslice__ is used for sequence types

class C(list):
    def __getslice__(self, i, j):
        return (i,j)

a = C()
AreEqual(a[32:197], (32,197))


# positive values work w/o len defined
class C(object):
    def __getslice__(self, i, j):
        return 'Ok'
    def __setslice__(self, i, j, value):
        self.lastCall = 'set'
    def __delslice__(self, i,j):
        self.lastCall = 'delete'
        
a = C()
AreEqual(a[5:10], 'Ok')

a.lastCall = ''

a[5:10] = 'abc'
AreEqual(a.lastCall, 'set')

a.lastCall = ''

del(a[5:10])
AreEqual(a.lastCall, 'delete')


# all values work w/ length defined,but don't call len if it's positive
class C(object):
    def __init__(self):
        self.calls = []
    def __getslice__(self, i, j):
        self.calls.append('get')
        return 'Ok'
    def __setslice__(self, i, j, value):
        self.calls.append('set')
    def __delslice__(self, i, j):
        self.calls.append('delete')
    def __len__(self):
        self.calls.append('len')
        return 5
        
a = C()
AreEqual(a[3:5], 'Ok')
AreEqual(a.calls, ['get'])

a = C()
a[3:5] = 'abc'
AreEqual(a.calls, ['set'])

a = C()
del(a[3:5])
AreEqual(a.calls, ['delete'])

# but call length if it's negative (and we should only call length once)


a = C()
AreEqual(a[-1:5], 'Ok')
AreEqual(a.calls, ['len', 'get'])

a = C()
AreEqual(a[-1:5], 'Ok')
AreEqual(a.calls, ['len', 'get'])

a = C()
AreEqual(a[-1:5], 'Ok')
AreEqual(a.calls, ['len', 'get'])


a = C()
a[-1:5] = 'abc'
AreEqual(a.calls, ['len', 'set'])

a = C()
a[1:-5] = 'abc'
AreEqual(a.calls, ['len', 'set'])

a = C()
a[-1:-5] = 'abc'
AreEqual(a.calls, ['len', 'set'])



a = C()
del(a[-1:5])
AreEqual(a.calls, ['len', 'delete'])

a = C()
del(a[1:-5])
AreEqual(a.calls, ['len', 'delete'])

a = C()
del(a[-1:-5])
AreEqual(a.calls, ['len', 'delete'])


#***** Above code are from 'Lists' *****
