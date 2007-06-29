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

#
# Test what works with -X:FastEval
#
# This is not a direct test; it is a helper module invoked by test_ipye.py.

Global1 = 5
Global2 = 6

def do_fasteval_test():
    '''Test the sorts of statements that work under FastEval mode'''
    l = []
    for i in range(10):
        l.append(i)
    # we can't use AreEqual &co. until more stuff works.
    assert l == range(10), 'for loops/function calls'
    assert l == list(tuple(range(10))), 'lists/tuples'
    x = 0
    while x < 20:
        x += 1
        if x >= 10:
            break
        else:
            continue
        assert False
    assert x == 10, 'while loops/basic arithmetic'

    def two_plus_two():
        return 2+2
    assert two_plus_two() == 4, 'function definition/execution'

    class C:
        def __init__(self, a):
            self.a = a
    c=C(5)
    assert c.a == 5

    class C2(object):
        def __init__(self, a):
            self.a = a
    c2=C2(4)
    assert c2.a == 4

    def add(a,b):
        return a+b
    assert add(3,4) == 7

    assert Global1 == 5 and Global2 == 6
    def reset_globals():
        global Global1, Global2
        Global1 = 1
        Global2 = 2
    reset_globals()
    assert Global1 == 1 and Global2 == 2

    x = False; y = False
    try:
        assert False, 'exception handling'
    except AttributeError:
        assert False, 'exception handling'
    except AssertionError:
        y = True
    else:
        assert False, 'else clause in try/catch'
    finally:
        x = True
    assert x and y, 'exception handling'

if __name__=='__main__':
    do_fasteval_test()
