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

if is_cli:
    from System.Collections import ArrayList
    l = ArrayList()
    index = l.Add(22)
    Assert(l[0] == 22)
    l[0] = 33
    Assert(l[0] == 33)

import sys
Assert('<stdin>' in str(sys.stdin))
#Assert('<stdout>' in str(sys.stdout))
#Assert('<stderr>' in str(sys.stderr))


# setting an instance property on a built-in type should
# throw that you can't set on built-in types
if is_cli:
    def setCount():
        ArrayList.Count = 23

    AssertError(AttributeError, setCount)
    
# getting a property from a class should return the property,
# getting it from an instance should do the descriptor check

class foo(object):
    def myset(self, value): pass
    def myget(self): return "hello"
    prop = property(fget=myget,fset=myset)

AreEqual(type(foo.prop), property)

a = foo()

AreEqual(a.prop, 'hello')

# a class w/ a metaclass that has a property
# defined should hit the descriptor when getting
# it on the class.

class MyType(type):
    def myget(self): return 'hello'
    aaa = property(fget=myget)

class foo(object):
    __metaclass__ = MyType

AreEqual(foo.aaa, 'hello')

# ReflectedProperty tests
if is_cli: 
    alist = ArrayList()
    AreEqual(ArrayList.Count.__set__(None, 5), False)
    AssertError(TypeError, ArrayList.Count, alist, 5)
    AreEqual(alist.Count, 0)
    AreEqual(str(ArrayList.__dict__['Count']), '<property# Count on ArrayList>')
    
    def tryDelReflectedProp():
	    del ArrayList.Count

    AssertError(TypeError, tryDelReflectedProp)

# define a property w/ only the doc

x = property(None, None, doc = 'Holliday')
AreEqual(x.fget, None)
AreEqual(x.fset, None)
AreEqual(x.fdel, None)
AreEqual(x.__doc__, 'Holliday')
 