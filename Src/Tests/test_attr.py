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

def CheckDictionary(mod): 
    # add a new attribute to the type...
    mod.newModuleAttr = 'xyz'
    AreEqual(mod.newModuleAttr, 'xyz')
    
    # add non-string index into the class and instance dictionary
    mod.__dict__[1] = '1'
    CheckObjectKeys(mod)
    
    # replace a module dictionary (containing non-string keys) w/ a normal dictionary
    AreEqual(hasattr(mod, 'newModuleAttr'), True)
    mod.__dict__ = dict(mod.__dict__)  
    AreEqual(hasattr(mod, 'newModuleAttr'), True)

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

#***** Above code are from 'Attrs' *****
