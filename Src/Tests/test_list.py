
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

def test_extend_self():
    l=['a','b','c']
    l.extend(l)
    Assert(l==['a','b','c','a','b','c'])

# verify repr and print have the same result for a recursive list
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

if is_cli: 
    import clr
    x = [1,2,3]
    y = []
    xenum = iter(x)
    while xenum.MoveNext():
        y.append(xenum.Current)
    AreEqual(x, y)

# Disallow assignment to empty list
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
    # BUG: 1028
    #del a, b, c

    [[a, b], c] = (listOfSize2, listOfSize2)
    AreEqual(a, 1)
    AreEqual(b, 2)
    AreEqual(c, listOfSize2)
    #del a, b, c

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

if is_cli:
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
               
        # test ListWrapperForArrayList
        pl = [check_content, 1, type, "string"]
        cl = System.Collections.ArrayList()
        for x in pl: result = cl.Add(x)

        do_something(IronPythonTest.UsePythonListAsArrayList, pl, cl, check_content)
        
        # test DictWrapperForIDict 
        pl = {"redmond" : 10, "seattle" : 20}
        cl = System.Collections.Generic.Dictionary[str, int]()
        for x, y in pl.iteritems(): cl.Add(x, y)
        
        def check_content():
            for x, y in zip(cl, pl.iteritems()): 
                AreEqual(x.Key, y[0])
                AreEqual(x.Value, y[1])
      
        do_something(IronPythonTest.UsePythonDictAsDictionary, pl, cl, check_content)

        # test DictWrapperForHashtable 
        pl = {"redmond" : 10, "seattle" : 20}
        cl = System.Collections.Hashtable()
        for x, y in pl.iteritems(): cl.Add(x, y)

        def check_content():
            AreEqual(len(pl), len(cl))
            for x in cl.Keys: 
                AreEqual(cl[x], pl[x])
       
        do_something(IronPythonTest.UsePythonDictAsHashtable, cl, pl, check_content)

run_test(__name__)