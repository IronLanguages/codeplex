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

import _weakref
import gc

class NonCallableClass(object): pass

class CallableClass(object):
    def __call__(self, *args):
        return 42

def test_proxy_dir():
    # dir on a deletex proxy should return an empty list,
    # not throw.
    for cls in [NonCallableClass, CallableClass]:
        def run_test():
            a = cls()        
            b = _weakref.proxy(a)
            
            AreEqual(dir(a), dir(b))
            
            del(a)
            
            return b
            
        prxy = run_test()    
        gc.collect()
        
        AreEqual(dir(prxy), [])

def test_special_methods():
    for cls in [NonCallableClass, CallableClass]:
        # calling repr should give us weakproxy's repr,
        # calling __repr__ should give us the underlying objects
        # repr
        a = cls()    
        b = _weakref.proxy(a)
        
        Assert(repr(b).startswith('<weakproxy at'))
        
        AreEqual(repr(a), b.__repr__())
        
    # calling a special method should work
    class strable(object):
            def __str__(self): return 'abc'

    a = strable()
    b = _weakref.proxy(a)
    AreEqual(str(b), 'abc')


def test_type_call():
    def get_dead_weakref():
        class C: pass
        
        a = C()        
        x = _weakref.proxy(a)
        del(a)
        return x
        
    wr = get_dead_weakref()
    type(wr).__add__.__get__(wr, None) # no exception
    
    try:
        type(wr).__add__.__get__(wr, None)() # object is dead, should throw
    except: pass
    else: AssertUnreachable()
    
        
    # kwarg call
    class C: 
        def __add__(self, other):
            return "abc" + other
        
    a = C()        
    x = _weakref.proxy(a)
    
    if is_cli:      # cli accepts kw-args everywhere
        res = type(x).__add__.__get__(x, None)(other = 'xyz')
        AreEqual(res, "abcxyz")
    
    # calling non-existent method should raise attribute error
    try:
        type(x).__sub__.__get(x, None)('abc')
    except AttributeError: pass
    else: AssertUnreachable()

    if is_cli:      # cli accepts kw-args everywhere
        # calling non-existent method should raise attribute error (kw-arg version)
        try:
            type(x).__sub__.__get(x, None)(other='abc')
        except AttributeError: pass
        else: AssertUnreachable()

def test_slot_repr():
    class C: pass

    a = C()
    x = _weakref.proxy(a)
    AreEqual(repr(type(x).__add__), "<slot wrapper '__add__' of 'weakproxy' objects>")

run_test(__name__)
