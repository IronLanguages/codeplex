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

from Util.Debug import *

def nop():
    pass

class gc:
    pass

try:
    import System
    from System import GC

    gc.collect = GC.Collect
    gc.WaitForPendingFinalizers = GC.WaitForPendingFinalizers
except ImportError:
    import gc
    gc.collect = gc.collect
    gc.WaitForPendingFinalizers = nop


def FullCollect():
    gc.collect()
    gc.WaitForPendingFinalizers()

def Hello():
    global res
    res = 'Hello finalizer'

#########################################
# class implements finalizer

class Foo:
    def __del__(self):
            global res
            res = 'Foo finalizer'

#########################################
# class doesn't implement finalizer

class Bar:
    pass

######################
# simple case

def SimpleTest():
    global res

    res = ''
    f = Foo()
    del(f)
    FullCollect()

    Assert(res == 'Foo finalizer')


######################
# Try to delete a builtin name. This should fail since "del" should not
# lookup the builtin namespace

def DoDelBuiltin():
    global pow
    del(pow)

def DelBuiltin():
    # Check that "pow" is defined
    global pow
    p = pow

    AssertError(NameError, DoDelBuiltin)
    AssertError(NameError, DoDelBuiltin)

######################
# Try to delete a builtin name. This should fail since "del" should not
# lookup the builtin namespace

def DoDelGlobal():
    global glb
    del(glb)
    return True

def DelUndefinedGlobal():
    AssertError(NameError, DoDelGlobal)
    AssertError(NameError, DoDelGlobal)

def DelDefinedGlobal():
    # Check that "glb" is defined
    global glb
    l = glb

    Assert(DoDelGlobal() == True)
    AssertError(NameError, DoDelGlobal)
    AssertError(NameError, DoDelGlobal)

######################
# Try to delete a name from an enclosing function. This should fail since "del" should not
# lookup the enclosing namespace

def EnclosingFunction():
    val = 1
    def DelEnclosingName():
        del val
    DelEnclosingName()

######################
# per-instance override
def PerInstOverride():

    global res
    res = ''
    f = Foo()

    f.__del__ = Hello

    Assert(hasattr(Foo, '__del__'))

    Assert(hasattr(f, '__del__'))

    del(f)
    FullCollect()

    Assert(res == 'Hello finalizer')

##################################
# per-instance override & remove

def PerInstOverrideAndRemove():
    global res
    global Hello

    res = ''
    f = Foo()
    f.__del__ = Hello
    Assert(hasattr(Foo, '__del__'))

    Assert(hasattr(f, '__del__'))

    del(f.__del__)
    Assert(hasattr(Foo, '__del__'))
    Assert(hasattr(f, '__del__'))

    del(f)
    FullCollect()

    Assert(res == 'Foo finalizer')


##################################
# per-instance override & remove both

def PerInstOverrideAndRemoveBoth():

    res = ''
    f = Foo()
    Assert(hasattr(Foo, '__del__'))
    Assert(hasattr(f, '__del__'))

    f.__del__ = Hello

    Assert(hasattr(Foo, '__del__'))
    Assert(hasattr(f, '__del__'))

    FullCollect()
    FullCollect()

    del(Foo.__del__)
    Assert(hasattr(Foo, '__del__') == False)
    Assert(hasattr(f, '__del__'))

    del(f.__del__)
    dir(f)

    Assert(hasattr(Foo, '__del__') == False)
    Assert(hasattr(f, '__del__') == False)

    FullCollect()
    FullCollect()

    del(f)
    FullCollect()

    Assert(res == '')


##################################
# define finalizer after instance creation
def NoFinAddToInstance():

    global res
    res = ''
    b = Bar()
    Assert(hasattr(Bar, '__del__') == False)

    Assert(hasattr(b, '__del__') == False)

    b.__del__ = Hello
    Assert(hasattr(Bar, '__del__') == False)
    Assert(hasattr(b, '__del__'))

    del(b)
    FullCollect()

    Assert(res == 'Hello finalizer')


##################################
# define & remove finalizer after instance creation
def NoFinAddToInstanceAndRemove():
    global res
    res = ''
    b = Bar()
    Assert(hasattr(Bar, '__del__') == False)

    Assert(hasattr(b, '__del__') == False)

    b.__del__ = Hello
    Assert(hasattr(Bar, '__del__') == False)
    Assert(hasattr(b, '__del__'))

    del(b.__del__)
    Assert(hasattr(Bar, '__del__') == False)
    Assert(hasattr(b, '__del__') == False)

    del(b)
    FullCollect()

    Assert(res == '')


SimpleTest()
# Bug 156
# DelBuiltin()
# EnclosingFunction()
DelUndefinedGlobal()
glb = 100
DelDefinedGlobal()
PerInstOverride()
PerInstOverrideAndRemove()
PerInstOverrideAndRemoveBoth()
NoFinAddToInstance()
NoFinAddToInstanceAndRemove()
