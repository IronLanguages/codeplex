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

import toimport

from Util.Debug import *
import sys
result = reload(sys)
Assert(not sys.modules.__contains__("Error"))
success = False
try:
    import Util.Error
except AssertionError:
    success = True
Assert(success)
Assert(not sys.modules.__contains__("Error"))

from Util import Module

filename = Module.__file__.lower()
if is_cli:
    Assert(filename.endswith("module.py"))
else :
    Assert(filename.endswith("module.py") or filename.endswith("module.pyc"))    
AreEqual(Module.__name__.lower(), "util.module")
AreEqual(Module.value, "This is the module to test from Util import Module")

from Util.Module import (a, b,
c
,
d, e,
    f, g, h
, i, j)

Assert('a' in dir())
Assert('b' in dir())
Assert('c' in dir())
Assert('d' in dir())
Assert('e' in dir())
Assert('f' in dir())
Assert('g' in dir())
Assert('h' in dir())
Assert('i' in dir())
Assert('j' in dir())

# testing double import of generators with yield inside try

from Util import Gen
result = sys.modules
u = sys.modules.pop("Util")
g = sys.modules.pop("Util.Gen")
from Util import Gen
AreEqual(Gen.gen().next(), "yield inside try")

# using import in nested blocks

def f():
    import time
    now = time.time()

f()

try:
    print time
except NameError:
    pass
else:
    Assert(False, "time should be undefined")

def f():
    import time as t
    now = t.time()
    try:
        now = time
    except NameError:
        pass
    else:
        Assert(False, "time should be undefined")

f()

try:
    print time
except NameError:
    pass
else:
    Assert(False, "time should be undefined")


def f():
    from time import clock
    now = clock()
    try:
        now = time
    except NameError:
        pass
    else:
        Assert(False, "time should be undefined")

f()

try:
    print time
except NameError:
    pass
else:
    Assert(False, "time should be undefined")

try:
    print clock
except NameError:
    pass
else:
    Assert(False, "clock should be undefined")

def f():
    from time import clock as c
    now = c()
    try:
        now = time
    except NameError:
        pass
    else:
        Assert(False, "time should be undefined")
    try:
        now = clock
    except NameError:
        pass
    else:
        Assert(False, "clock should be undefined")

f()

try:
    print time
except NameError:
    pass
else:
    Assert(False, "time should be undefined")

try:
    print clock
except NameError:
    pass
else:
    Assert(False, "clock should be undefined")

# with closures

def f():
    def g():
        now = clock_in_closure()
    from time import clock as clock_in_closure
    g()

f()


def get_local_filename(base):
    if __file__.count('\\'):
        return __file__.rsplit("\\", 1)[0] + '\\'+ base
    else:
        return base



def compileAndRef(name, filename, *args):
    import clr
    sys.path.append(sys.exec_prefix)
    AreEqual(run_csc("/nologo /t:library " + ' '.join(args) + " /out:\"" + sys.exec_prefix + "\"\\" + name +".dll \"" + filename + "\""), 0)
    clr.AddReference(name)


def test_c1cs():
    """verify re-loading an assembly causes the new type to show up"""
    if not has_csc(): 
        return
    
    c1cs = get_local_filename('c1.cs')    
    outp = sys.exec_prefix
    
    compileAndRef('c1', c1cs, '/d:BAR1')
    
    import Foo
    class c1Child(Foo.Bar): pass
    o = c1Child()
    AreEqual(o.Method(), "In bar1")
    
    
    compileAndRef('c1_b', c1cs)
    import Foo
    class c2Child(Foo.Bar): pass
    o = c2Child()
    AreEqual(o.Method(), "In bar2")
    # ideally we would delete c1.dll, c2.dll here so as to keep them from cluttering up
    # /Public; however, they need to be present for the peverify pass.


def test_c2cs():
    """verify generic types & non-generic types mixed in the same namespace can 
    successfully be used"""
    if not has_csc(): 
        return
    
    c2cs = get_local_filename('c2.cs')    
    outp = sys.exec_prefix

    # first let's load Foo<T>
    compileAndRef('c2_a', c2cs, '/d:TEST1')    
    import ImportTestNS
    
    x = ImportTestNS.Foo[int]()
    AreEqual(x.Test(), 'Foo<T>')
    
    compileAndRef('c2_b', c2cs, '/d:TEST2')
    
    # ok, now let's get a Foo<T,Y> going on...
    import ImportTestNS
    x = ImportTestNS.Foo[int,int]()
    AreEqual(x.Test(), 'Foo<T,Y>')
    
    # that worked, let's make sure Foo<T> is still available...
    x = ImportTestNS.Foo[int]()
    AreEqual(x.Test(), 'Foo<T>')
    
    compileAndRef('c2_c', c2cs, '/d:TEST3')
    
    import ImportTestNS
    x = ImportTestNS.Foo[int,int,int]()
    AreEqual(x.Test(), 'Foo<T,Y,Z>')
    
    # make sure the other two are still available
    x = ImportTestNS.Foo[int,int]()
    AreEqual(x.Test(), 'Foo<T,Y>')    
    x = ImportTestNS.Foo[int]()
    AreEqual(x.Test(), 'Foo<T>')
    
    # ok, now let's get plain Foo in the picture...
    compileAndRef('c2_d', c2cs, '/d:TEST4')
    import ImportTestNS
    
    x = ImportTestNS.Foo()
    AreEqual(x.Test(), 'Foo')
    
    # check the generics still work
    x = ImportTestNS.Foo[int,int,int]()
    AreEqual(x.Test(), 'Foo<T,Y,Z>')
    x = ImportTestNS.Foo[int,int]()
    AreEqual(x.Test(), 'Foo<T,Y>')    
    x = ImportTestNS.Foo[int]()
    AreEqual(x.Test(), 'Foo<T>')
    
    # now let's try replacing the non-generic Foo
    compileAndRef('c2_e', c2cs, '/d:TEST5')
    import ImportTestNS
    
    x = ImportTestNS.Foo()
    AreEqual(x.Test(), 'Foo2')
    
    # and make sure all the generics still work
    x = ImportTestNS.Foo[int,int,int]()
    AreEqual(x.Test(), 'Foo<T,Y,Z>')
    x = ImportTestNS.Foo[int,int]()
    AreEqual(x.Test(), 'Foo<T,Y>')    
    x = ImportTestNS.Foo[int]()
    AreEqual(x.Test(), 'Foo<T>')
    
    # finally, let's now replace one of the 
    # generic overloads...
    compileAndRef('c2_f', c2cs, '/d:TEST6')
    import ImportTestNS

    x = ImportTestNS.Foo[int]()
    AreEqual(x.Test(), 'Foo<T>2')

    # and make sure the old ones are still there too..
    x = ImportTestNS.Foo()
    AreEqual(x.Test(), 'Foo2')
    x = ImportTestNS.Foo[int,int,int]()
    AreEqual(x.Test(), 'Foo<T,Y,Z>')
    x = ImportTestNS.Foo[int,int]()
    AreEqual(x.Test(), 'Foo<T,Y>')    
    
if is_cli:    
    test_c1cs()
    test_c2cs()

Assert(sys.modules.has_key("__main__"))

from Imp.pkg_m import mod_a
AreEqual(mod_a.value_b, "Imp.pkg_m.mod_b.value_b")


import imp

x = imp.new_module('abc')
sys.modules['abc'] = x
x.foo = 'bar'
import abc

AreEqual(abc.foo, 'bar')