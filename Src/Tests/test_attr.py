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

class C(object):
    pass

y = C.__delattr__
y = C().__delattr__
y = object.__delattr__
C.a = 10
Assert(C.a == 10)
del C.a
success=0
try:
    y = C.a
except AttributeError:
    success = 1
Assert(success == 1)

######################################################################################

# Non-string attributes on a module

def CheckObjectKeys(mod):
    AreEqual(mod.__dict__.has_key(1), True)
    AreEqual(dir(mod).__contains__(1), True)
    AreEqual(repr(mod.__dict__).__contains__("1: '1'"), True)

def SetDictionary(mod, dict):
    mod.__dict__ = dict

def CheckDictionary(mod): 
    # add a new attribute to the type...
    mod.newModuleAttr = 'xyz'
    AreEqual(mod.newModuleAttr, 'xyz')
    
    # add non-string index into the class and instance dictionary
    mod.__dict__[1] = '1'
    CheckObjectKeys(mod)
    
    # Try to replace __dict__
    if is_cli: # CPython does not consistently use TypeError/AttributeError for read-only attributes
        AssertErrorWithMessage(AttributeError, "attribute '__dict__' of 'module' object is read-only", SetDictionary, mod, dict(mod.__dict__))
    else:
        AssertErrorWithMessage(TypeError, "readonly attribute", SetDictionary, mod, dict(mod.__dict__))

import sys
me = sys.modules[__name__]
CheckDictionary(me)
# This is disabled since it causes recursion. We should define another test module to reload
# reload(me)
# CheckObjectKeys(me)

##########################################################################
# Decorators starting with Bug #993
def f(x='default'): return x

cm = classmethod(f)
sm = staticmethod(f)
p = property(f)
AreEqual(f.__get__(1)(), 1)
AreEqual(str(f.__get__(2)), "<bound method ?.f of 2>")
AreEqual(str(f.__get__(2, list)), "<bound method list.f of 2>")

AreEqual(cm.__get__(1)(), int)
AreEqual(str(cm.__get__(2)), "<bound method type.f of <type 'int'>>")

AreEqual(sm.__get__(1)(), 'default')
AreEqual(p.__get__(1), 1)

######################################################################################
# __getattribute__, __setattr__, __delattr__ on builtins

if is_cli:
	import System
	dateTime = System.DateTime()

	AreEqual(dateTime.ToString, dateTime.__getattribute__("ToString"))
	AssertErrorWithMessage(AttributeError, "attribute 'ToString' of 'DateTime' object is read-only", dateTime.__setattr__, "ToString", "foo")
	AssertErrorWithMessage(AttributeError, "attribute 'ToString' of 'DateTime' object is read-only", dateTime.__delattr__, "ToString")

	arrayList = System.Collections.ArrayList()
	arrayList.__setattr__("Capacity", 123)
	AreEqual(arrayList.Capacity, 123)

AreEqual(me.__file__, me.__getattribute__("__file__"))
me.__setattr__("__file__", "foo")
AreEqual(me.__file__, "foo")
me.__delattr__("__file__")

class C(object):
    def foo(self): pass

# C.foo is "unbound method" on IronPython but "function" on CPython
if is_cli:
    AreEqual(C.foo, C.__getattribute__(C, "foo"))
else:
    AreEqual(C.foo.im_func, C.__getattribute__(C, "foo"))
AreEqual(C.__doc__, C.__getattribute__(C, "__doc__"))
# IronPython incorrectly allows this because of MethodWrappers
if is_cli == False:
    AssertErrorWithMessage(TypeError, "can't apply this __setattr__ to type object", C.__setattr__, C, "__str__", "foo")
    AssertErrorWithMessage(TypeError, "can't apply this __delattr__ to type object", C.__delattr__, C, "__str__")

s = "hello"
AreEqual(s.center, s.__getattribute__("center"))
AssertErrorWithMessages(AttributeError, "attribute 'center' of 'str' object is read-only", 
                                        "'str' object attribute 'center' is read-only", s.__setattr__, "center", "foo")
AssertErrorWithMessages(AttributeError, "attribute 'center' of 'str' object is read-only", 
                                        "'str' object attribute 'center' is read-only", s.__delattr__, "center")

AssertError(TypeError, getattr, object(), None)
