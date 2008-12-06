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

from __future__ import print_function
from __future__ import unicode_literals

from iptest.assert_util import *


def test_class_decorators():
    global called
    called = False
    def f(x): 
        global called
        called = True
        return x

    @f
    class x(object): pass
    
    AreEqual(called, True)
    AreEqual(type(x), type)

    called = False
    
    @f
    def abc(): pass
    
    AreEqual(called, True)
    
    def f(*args):
        def g(x):
            return x
        global called
        called = args
        return g
    
    
    @f('foo', 'bar')
    class x(object): pass
    AreEqual(called, ('foo', 'bar'))
    AreEqual(type(x), type)

    @f('foo', 'bar')
    def x(): pass
    AreEqual(called, ('foo', 'bar'))
    
    def f():
        exec """@f\nif True: pass\n"""
    AssertError(SyntaxError, f)

def test_binary_numbers():
    AreEqual(0b01, 1)
    AreEqual(0b10, 2)
    AreEqual(0b100000000000000000000000000000000, 4294967296)

    AreEqual(0b01L, 1)
    AreEqual(0b10L, 2)
    AreEqual(0b100000000000000000000000000000000L, 4294967296)

    AreEqual(type(0b01), int)
    AreEqual(type(0b10), int)
    AreEqual(type(0b01111111111111111111111111111111), int)
    AreEqual(type(0b10000000000000000000000000000000), long)
    AreEqual(type(-0b01111111111111111111111111111111), int)
    AreEqual(type(-0b10000000000000000000000000000000), int)
    AreEqual(type(-0b10000000000000000000000000000001), long)
    AreEqual(type(0b00000000000000000000000000000000000001), int)

    AreEqual(type(0b01L), long)
    AreEqual(type(0b10L), long)
    AreEqual(type(0b01l), long)
    AreEqual(type(0b10l), long)
    AreEqual(type(0b100000000000000000000000000000000L), long)
    AreEqual(0b01, 0B01)

def test_print_function():
    AssertError(TypeError, print, sep = 42)
    AssertError(TypeError, print, end = 42)
    AssertError(TypeError, print, abc = 42)
    AssertError(AttributeError, print, file = 42)
    
    import sys
    oldout = sys.stdout
    sys.stdout = file('abc.txt', 'w')
    try:
        print('foo')
        print('foo', end = 'abc')
        print()
        print('foo', 'bar', sep = 'abc')        
        
        sys.stdout.close()        
        f = file('abc.txt')
        AreEqual(f.readlines(), ['foo\n', 'fooabc\n', 'fooabcbar\n'])
    finally:
        sys.stdout = oldout

    class myfile(object):
        def __init__(self):
            self.text = ''
        def write(self, text):
            self.text += text
    
    sys.stdout = myfile()
    try:
        print('foo')
        print('foo', end = 'abc')
        print()
        print('foo', 'bar', sep = 'abc')        

        AreEqual(sys.stdout.text, 'foo\n' + 'fooabc\n' + 'fooabcbar\n')
    finally:
        sys.stdout = oldout

run_test(__name__)
