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

##
## Test built-in types: set/frozenset
##

from lib.assert_util import *
from lib.type_util import myset, myfrozenset

s1 = [2, 4, 5]
s2 = [4, 7, 9, 10]
s3 = [2, 4, 5, 6]

def test_equality(): 
    for x in (set, frozenset, myset, myfrozenset):
        for y in (set, frozenset, myset, myfrozenset):
            AreEqual(x(s1), y(s1))

def test_sanity():
    for x in (set, frozenset, myset, myfrozenset):
        # creating as default
        y = x()
        AreEqual(len(y), 0)
        # creating with 2 args
        AssertError(TypeError, x, range(3), 3)
        #!!!AssertError(TypeError, x.__new__, str)
        #!!!AssertError(TypeError, x.__new__, str, 'abc')

        xs1, xs2, xs3 = x(s1), x(s2), x(s3)
        
        # membership
        AreEqual(4 in xs1, True)
        AreEqual(6 in xs1, False)    
        
        # relation with another of the same type
        AreEqual(xs1.issubset(xs2), False)
        AreEqual(xs1.issubset(xs3), True)
        AreEqual(xs3.issuperset(xs1), True)
        AreEqual(xs3.issuperset(xs2), False)

        # equivalent op
        AreEqual(xs1 <= xs2, False)
        AreEqual(xs1 <= xs3, True)
        AreEqual(xs3 >= xs1, True)
        AreEqual(xs3 >= xs2, False)
        
        AreEqual(xs1.union(xs2), x([2, 4, 5, 7, 9, 10]))
        AreEqual(xs1.intersection(xs2), x([4]))
        AreEqual(xs1.difference(xs2), x([2, 5]))
        AreEqual(xs2.difference(xs1), x([7, 9, 10]))
        AreEqual(xs2.symmetric_difference(xs1), x([2, 5, 7, 9, 10]))
        AreEqual(xs3.symmetric_difference(xs1), x([6]))

        # equivalent op
        AreEqual(xs1 | xs2, x([2, 4, 5, 7, 9, 10]))
        AreEqual(xs1 & xs2, x([4]))
        AreEqual(xs1 - xs2, x([2, 5]))
        AreEqual(xs2 - xs1, x([7, 9, 10]))
        AreEqual(xs2 ^ xs1, x([2, 5, 7, 9, 10]))
        AreEqual(xs3 ^ xs1, x([6]))

        # repeat with list
        AreEqual(xs1.issubset(s2), False)
        AreEqual(xs1.issubset(s3), True)
        AreEqual(xs3.issuperset(s1), True)
        AreEqual(xs3.issuperset(s2), False)
        
        AreEqual(xs1.union(s2), x([2, 4, 5, 7, 9, 10]))
        AreEqual(xs1.intersection(s2), x([4]))
        AreEqual(xs1.difference(s2), x([2, 5]))
        AreEqual(xs2.difference(s1), x([7, 9, 10]))
        AreEqual(xs2.symmetric_difference(s1), x([2, 5, 7, 9, 10]))
        AreEqual(xs3.symmetric_difference(s1), x([6]))

def test_ops():
    s1, s2, s3 = 'abcd', 'be', 'bdefgh'
    for t1 in (set, frozenset, myset, myfrozenset):
        for t2 in (set, frozenset, myset, myfrozenset):
            # set/frozenset creation
            AreEqual(t1(t2(s1)), t1(s1))
            
            # ops
            for (op, exp1, exp2) in [('&', 'b', 'bd'), ('|', 'abcde', 'abcdefgh'), ('-', 'acd', 'ac'), ('^', 'acde', 'acefgh')]:
                
                x1 = t1(s1)
                exec "x1   %s= t2(s2)" % op
                AreEqual(x1, t1(exp1))

                x1 = t1(s1)
                exec "x1   %s= t2(s3)" % op
                AreEqual(x1, t1(exp2))
                
                x1 = t1(s1)            
                exec "y = x1 %s t2(s2)" % op
                # BUG: 1029
                #AreEqual(y, t1(exp1))

                x1 = t1(s1)            
                exec "y = x1 %s t2(s3)" % op
                # BUG: 1029
                #AreEqual(y, t1(exp2))

def test_none():
    x, y = set([None, 'd']), set(['a', 'b', 'c', None])
    AreEqual(x | y, set([None, 'a', 'c', 'b', 'd']))
    AreEqual(y | x, set([None, 'a', 'c', 'b', 'd']))
    AreEqual(x & y, set([None]))
    AreEqual(y & x, set([None]))
    AreEqual(x - y, set('d'))
    AreEqual(y - x, set('abc'))
    
run_test(__name__)


