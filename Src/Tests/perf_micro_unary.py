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

import time
import sys

loops = 1000000

def minus_maker(arg):
    for x in xrange(loops):
        a = -arg
        a = -arg        
        a = -arg
        a = -arg        
        a = -arg
        a = -arg        
        a = -arg
        a = -arg         
        a = -arg
        a = -arg        
        a = -arg
        a = -arg        
        a = -arg
        a = -arg        
        a = -arg
        a = -arg        
        a = -arg
        a = -arg        
        a = -arg
        a = -arg        
        a = -arg
        a = -arg        
        a = -arg
        a = -arg                        

def plus_maker(arg):        
    for x in xrange(loops):
        a = +arg
        a = +arg        
        a = +arg
        a = +arg        
        a = +arg
        a = +arg        
        a = +arg
        a = +arg         
        a = +arg
        a = +arg        
        a = +arg
        a = +arg        
        a = +arg
        a = +arg        
        a = +arg
        a = +arg        
        a = +arg
        a = +arg        
        a = +arg
        a = +arg        
        a = +arg
        a = +arg        
        a = +arg
        a = +arg                        

def invert_maker(arg):
    for x in xrange(loops):
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg         
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg        
        a = ~arg
        a = ~arg                        

def not_maker(arg):
    for x in xrange(loops):
        a = not arg
        a = not arg        
        a = not arg
        a = not arg        
        a = not arg
        a = not arg        
        a = not arg
        a = not arg         
        a = not arg
        a = not arg        
        a = not arg
        a = not arg        
        a = not arg
        a = not arg        
        a = not arg
        a = not arg        
        a = not arg
        a = not arg        
        a = not arg
        a = not arg        
        a = not arg
        a = not arg        
        a = not arg
        a = not arg                        

def test_minus_int():
    minus_maker(1)

def test_plus_int():
    plus_maker(1)

def test_invert_int():
    invert_maker(1)

def test_minus_long():
    minus_maker(1L)

def test_plus_long():
    plus_maker(1L)

def test_invert_long():
    invert_maker(1L)

def test_minus_float():
    minus_maker(1.0)

def test_plus_float():
    plus_maker(1.0)

def test_minus_complex():
    minus_maker(1 + 1j)

def test_plus_complex():
    plus_maker(1 + 1j)

def test_not_none():
    not_maker(True)

def test_not_int():
    not_maker(0)

def test_not_float():
    not_maker(1.0)

def test_not_none():
    not_maker(None)

def test_not_empty_str():
    not_maker("")

def test_not_nonempty_str():
    not_maker("abc")

def test_not_empty_list():
    not_maker([])

def test_not_nonempty_list():
    not_maker([1,2,3])

def test_not_empty_tuple():
    not_maker(())

def test_not_nonempty_tuple():
    not_maker((1,2,3))

def test_not_object():
    not_maker(object())

def test_not_oc_user_object():
    class X:
        def __nonzero__(self): return True
    not_maker(X())

def test_not_oc_user_object_2():
    class X:
        def __len__(self): return 1
    not_maker(X())

def test_not_ns_user_object():
    class X(object):
        def __nonzero__(self): return True
    not_maker(X())

def test_not_ns_user_object_2():
    class X(object):
        def __len__(self): return 1
    not_maker(X())

def run_all_tests():
    times = []
    names = []
    
    tests = [(testname, test) for testname, test in sys.modules[__name__].__dict__.iteritems() if isinstance(testname, str) and testname.startswith('test_')]
    tests.sort(lambda x,y: cmp(x[0], y[0]))
    start = prev = time.clock()
    for testname, test in tests:
        if not isinstance(testname, str): continue
        if not testname.startswith('test_'): continue
        
        test()

        times += time.clock(),
        names += testname,
    
    for thetime, name in zip(times, names):
        print name, thetime-prev, 'seconds'
        prev = thetime
        
    print 'total', prev-start

if __name__ == "__main__":
    run_all_tests()