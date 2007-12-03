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

def test_extend_self():
    l=['a','b','c']
    l.extend(l)
    Assert(l==['a','b','c','a','b','c'])

# verify repr and print have the same result for a recursive list
@skip('silverlight')
def test_append_self():
    a = list('abc')
    a.append(a)
    AreEqual(str(a), "['a', 'b', 'c', [...]]")

    ## file
    from lib.file_util import path_combine
    fn = path_combine(testpath.temporary_dir, "testfile.txt")

    fo = open(fn, "wb")
    a = list('abc')
    a.append(a)
    print >>fo, a,
    fo.close()

    fo = open(fn, "rb")
    Assert(fo.read() == repr(a))
    fo.close()

if is_cli or is_silverlight: 
    import clr
    x = [1,2,3]
    y = []
    xenum = iter(x)
    while xenum.MoveNext():
        y.append(xenum.Current)
    AreEqual(x, y)

# Disallow assignment to empty list
#When #10643 gets fixed, it will be necessary to remove the AssertError calls
#below and replace them with the actual assignments which are allowed in 2.5.
@skip("win32", "CodePlex Work Item 10643")
def test_assign_to_empty():
    y = []
    AssertError(SyntaxError, compile, "[] = y", "Error", "exec")
    AssertError(SyntaxError, compile, "[], t = y, 0", "Error", "exec")
    AssertError(SyntaxError, compile, "[[[]]]=[[y]]", "Error", "exec")
    del y

def test_unpack():
    listOfSize2 = [1, 2]

    # Disallow unequal unpacking assignment
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
    del a, b, c

def test_sort():
    # named params passed to sort
    LExpected = ['A', 'b', 'c', 'D']
    L = ['D', 'c', 'b', 'A']
    L.sort(key=lambda x: x.lower())
    Assert(L == LExpected)

    l = [1, 2, 3]
    l2 = l[:]
    l.sort(lambda x, y: x > y)
    AreEqual(l, l2)
    l.sort(lambda x, y: x > y)
    AreEqual(l, l2)

def test_list_in_list():
    aList = [['a']]
    anItem = ['a']
    AreEqual( aList.index(anItem), 0 )
    Assert(anItem in aList)

def test_pop():
    x = [1,2,3,4,5,6,7,8,9,0]
    Assert(x.pop() == 0)
    Assert(x.pop(3) == 4)
    Assert(x.pop(-5) == 5)
    Assert(x.pop(0) == 1)
    Assert(x.pop() == 9)
    Assert(x.pop(2) == 6)
    Assert(x.pop(3) == 8)
    Assert(x.pop(-1) == 7)
    Assert(x.pop(-2) == 2)
    Assert(x.pop() == 3)

def test_add_mul():
    x = [1,2,3]
    x += [4,5,6]
    Assert(x == [1,2,3,4,5,6])
    
    x = [1,2,3]
    AreEqual(x * 2, [1,2,3,1,2,3])
    AreEqual(2 * x, [1,2,3,1,2,3])

    class mylong(long): pass
    AreEqual([1, 2] * mylong(2L), [1, 2, 1, 2])
    AreEqual([3, 4].__mul__(mylong(2L)), [3, 4, 3, 4])
    AreEqual([5, 6].__rmul__(mylong(2L)), [5, 6, 5, 6])
    AreEqual(mylong(2L) * [7,8] , [7, 8, 7, 8])
    AssertError(TypeError, lambda: [1,2] * [3,4])
    AssertError(OverflowError, lambda: [1,2] * mylong(203958720984752098475023957209L))

def test_reverse():
    x = ["begin",1,2,3,4,5,6,7,8,9,0,"end"]
    del x[6:]
    x.reverse()
    Assert(x == [5, 4, 3, 2, 1, "begin"])

    x = list("iron python")
    x.reverse()
    Assert(x == ['n','o','h','t','y','p',' ','n','o','r','i'])


def test_equal():
    AreEqual([2,3] == '', False)
    AreEqual(list.__eq__([], None), NotImplemented)
    
    class MyEquality(object):
        def __eq__(self, other):
            return 'abc'
    
    class MyOldEquality(object):
        def __eq__(self, other):
            return 'def'            
            
    AreEqual([] == MyEquality(), 'abc')        
    AreEqual([] == MyOldEquality(), 'def')
    
    AreEqual([2,3] == (2,3), False)

    class MyIterable(object):
        def __iter__(self): return MyIterable()
        def next(self):
            yield 'a'
            yield 'b'
            
    AreEqual(['a', 'b'] == MyIterable(), False)

def test_self_init():
    a = [1, 2, 3]
    list.__init__(a, a)
    AreEqual(a, [])

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
AreEqual(hitCount, 1)       # should stop after the first equality check

a = [appends(), appends(), appends()]
hitCount = 0
AssertError(ValueError, a.index, 2)
AreEqual(hitCount, 3)       # should have only checked existing items

@runonly('cli')
def test_pass_pythonlist_to_clr():
    ##
    ## test passing pythonlist to clr where IList or ArrayList is requested
    ## also borrow this place to test passing python dict to clr where 
    ##      IDictionary or Hashtable is requested
    ##
    
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
    import IronPythonTest 
        
    # test ListWrapperForIList 
    pl = range(40)
    cl = System.Collections.Generic.List[int]()
    for x in pl: cl.Add(x)
        
    def check_content():
        for x, y in zip(cl, pl): AreEqual(x, y)
            
    do_something(IronPythonTest.UsePythonListAsList, pl, cl, check_content)
        
    # test DictWrapperForIDict 
    pl = {"redmond" : 10, "seattle" : 20}
    cl = System.Collections.Generic.Dictionary[str, int]()
    for x, y in pl.iteritems(): cl.Add(x, y)
        
    def check_content():
        for x, y in zip(cl, pl.iteritems()): 
            AreEqual(x.Key, y[0])
            AreEqual(x.Value, y[1])
      
    do_something(IronPythonTest.UsePythonDictAsDictionary, pl, cl, check_content)

def test_inplace_addition():
    x = [2,3,4]
    x += x
    AreEqual(x, [2,3,4,2,3,4])
    
    test_cases = [ ([],     [],     []),
                   ([1],    [],     [1]),
                   ([],     [1],    [1]),
                   ([1],    [1],    [1, 1]),
                   ([1],    [2],    [1, 2]),
                   ([2],    [1],    [2, 1]),
                   ([1, 2], [],     [1, 2]),
                   ([],     [1, 2], [1, 2]),
                   ([1, 2], [3],    [1, 2, 3]),
                   ([3],    [1, 2], [3, 1, 2]),
                   ([1, 2], [3, 4], [1, 2, 3, 4]),
                   ([3, 4], [1, 2], [3, 4, 1, 2]),
                   ([None], [],     [None]),
                   ([None], [2],    [None, 2]),
                   ([""],   [],     [""]),
                   ]
                   
    for left_operand, right_operand, result in test_cases:
    
        #(No access to copy.deepcopy in IP) 
        #  Create new list to verify no side effects to the RHS list
        orig_right = [x for x in right_operand]
            
        left_operand += right_operand
        
        AreEqual(left_operand, result)
        
        #Side effects...
        AreEqual(orig_right, right_operand)
        
    #interesting cases
    x = [None]
    x += xrange(3)
    AreEqual(x, [None, 0, 1, 2])
    
    x = [None]
    x += (0, 1, 2)
    AreEqual(x, [None, 0, 1, 2])
    
    x = [None]
    x += "012"
    AreEqual(x, [None, "0", "1", "2"])
    
    x = [None]
    x += Exception()
    AreEqual(x, [None])
    
    #negative cases
    neg_cases = [   ([],    None),
                    ([],    1),
                    ([],    1L),
                    ([],    3.14),
                    ([],    object),
                    ([],    object()),
                 ]
    for left_operand, right_operand in neg_cases:
        try:
            left_operand += right_operand
            AssertUnreachable()
        except TypeError, e:
            pass
            
def test_indexing():
    l = [2,3,4]
    def set(x, i, v): x[i] = v
    AssertError(TypeError, lambda : l[2.0])
    AssertError(TypeError, lambda : set(l, 2.0, 1))
    
    class mylist(list):
        def __getitem__(self, index):
            return list.__getitem__(self, int(index))
        def __setitem__(self, index, value):
            return list.__setitem__(self, int(index), value)

    l = mylist(l)
    AreEqual(l[2.0], 4)
    l[2.0] = 1
    AreEqual(l[2], 1)

run_test(__name__)