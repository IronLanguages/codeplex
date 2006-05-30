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

from lib.assert_util import *
from lib.file_util import *
from lib.process_util import *

try:
    import this_module_does_not_exist
except ImportError: pass
else:  Fail("should already thrown")

# generate test files on the fly
_testdir    = 'ImportTestDir'
_f_init     = path_combine(testpath.public_testdir, _testdir, '__init__.py')
_f_error    = path_combine(testpath.public_testdir, _testdir, 'Error.py')
_f_gen      = path_combine(testpath.public_testdir, _testdir, 'Gen.py') 
_f_module   = path_combine(testpath.public_testdir, _testdir, 'Module.py') 

write_to_file(_f_init)
write_to_file(_f_error, 'raise AssertionError()')
write_to_file(_f_gen, '''
def gen():
    try:
        yield "yield inside try"
    except:
        pass
''')

unique_line = "This is the module to test 'from ImportTestDir import Module'"

write_to_file(_f_module, '''
value = %r

a = 1
b = 2
c = 3
d = 4
e = 5
f = 6
g = 7
h = 8
i = 9
j = 10
''' % unique_line)

import sys
result = reload(sys)
Assert(not sys.modules.__contains__("Error"))

try:   
    import ImportTestDir.Error
except AssertionError: pass
else:  Fail("Should have thrown AssertionError from Error.py")

Assert(not sys.modules.__contains__("Error"))

from ImportTestDir import Module

filename = Module.__file__.lower()
Assert(filename.endswith("module.py") or filename.endswith("module.pyc"))    
AreEqual(Module.__name__.lower(), "importtestdir.module")
AreEqual(Module.value, unique_line)

from ImportTestDir.Module import (a, b,
c
,
d, e,
    f, g, h
, i, j)

for x in range(ord('a'), ord('j')+1):
    Assert(chr(x) in dir()) 

# testing double import of generators with yield inside try

from ImportTestDir import Gen
result = sys.modules
#u = sys.modules.pop("lib")
g = sys.modules.pop("ImportTestDir.Gen")
from ImportTestDir import Gen
AreEqual(Gen.gen().next(), "yield inside try")

#########################################################################################
# using import in nested blocks

def f():
    import time
    now = time.time()

f()

try:
    print time
except NameError: pass
else: Fail("time should be undefined")

def f():
    import time as t
    now = t.time()
    try:
        now = time
    except NameError: pass
    else: Fail("time should be undefined")

f()

try:
    print time
except NameError:  pass
else: Fail("time should be undefined")


def f():
    from time import clock
    now = clock()
    try:
        now = time
    except NameError: pass
    else: Fail("time should be undefined")
f()

try:
    print time
except NameError:  pass
else: Fail("time should be undefined")

try:
    print clock
except NameError:  pass
else: Fail("clock should be undefined")

def f():
    from time import clock as c
    now = c()
    try:
        now = time
    except NameError:  pass
    else: Fail("time should be undefined")
    try:
        now = clock
    except NameError:  pass
    else: Fail("clock should be undefined")

f()

try:
    print time
except NameError:  pass
else: Fail("time should be undefined")

try:
    print clock
except NameError:  pass
else: Fail("clock should be undefined")

# with closures

def f():
    def g(): now = clock_in_closure()
    from time import clock as clock_in_closure
    g()

f()


#########################################################################################

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

#########################################################################################

_testdir        = 'ImportTestDir'
_f_init2         = path_combine(testpath.public_testdir, _testdir, '__init__.py')
_f_longpath     = path_combine(testpath.public_testdir, _testdir, 'longpath.py')
_f_recursive    = path_combine(testpath.public_testdir, _testdir, 'recursive.py')
_f_usebuiltin   = path_combine(testpath.public_testdir, _testdir, 'usebuiltin.py')

write_to_file(_f_init2, '''
import recursive
import longpath
''')

write_to_file(_f_longpath, '''
from lib.assert_util import *
import pkg_q.pkg_r.pkg_s.mod_s
Assert(pkg_q.pkg_r.pkg_s.mod_s.result == "Success")
''')

write_to_file(_f_recursive, '''
from lib.assert_util import *
import pkg_a.mod_a
Assert(pkg_a.mod_a.pkg_b.mod_b.pkg_c.mod_c.pkg_d.mod_d.result == "Success")
''')

write_to_file(_f_usebuiltin, '''
x = max(3,5)
x = min(3,5)
min = x

x = cmp(min, x)

cmp = 17
del(cmp)

dir = 'abc'
del(dir)
''')

_f_pkga_init    = path_combine(testpath.public_testdir, _testdir, 'pkg_a', '__init__.py')
_f_pkga_moda    = path_combine(testpath.public_testdir, _testdir, 'pkg_a', 'mod_a.py')
write_to_file(_f_pkga_init, '''
import __builtin__

def new_import(a,b,c,d):
    print "* pkg_a.py import"
    print a, d
    return old_import(a,b,c,d)

old_import = __builtin__.__import__
#__builtin__.__import__ = new_import
''')
write_to_file(_f_pkga_moda, '''
import __builtin__

def new_import(a,b,c,d):
    print "* mod_a.py import"
    print a, d
    return old_import(a,b,c,d)

old_import = __builtin__.__import__
#__builtin__.__import__ = new_import

import pkg_b.mod_b
''')

_f_pkgb_init    = path_combine(testpath.public_testdir, _testdir, 'pkg_a', 'pkg_b','__init__.py')
_f_pkgb_modb    = path_combine(testpath.public_testdir, _testdir, 'pkg_a', 'pkg_b','mod_b.py')
write_to_file(_f_pkgb_init)
write_to_file(_f_pkgb_modb, 'import pkg_c.mod_c')

_f_pkgc_init    = path_combine(testpath.public_testdir, _testdir, 'pkg_a', 'pkg_b', 'pkg_c', '__init__.py')
_f_pkgc_modc    = path_combine(testpath.public_testdir, _testdir, 'pkg_a', 'pkg_b', 'pkg_c', 'mod_c.py')
write_to_file(_f_pkgc_init)
write_to_file(_f_pkgc_modc, '''
import __builtin__

def new_import(a,b,c,d):
    print "* mod_c.py import"
    print a, d
    return old_import(a,b,c,d)

old_import = __builtin__.__import__
#__builtin__.__import__ = new_import

import pkg_d.mod_d
''')

_f_pkgd_init    = path_combine(testpath.public_testdir, _testdir, 'pkg_a', 'pkg_b', 'pkg_c', 'pkg_d', '__init__.py')
_f_pkgd_modd    = path_combine(testpath.public_testdir, _testdir, 'pkg_a', 'pkg_b', 'pkg_c', 'pkg_d', 'mod_d.py')
write_to_file(_f_pkgd_init)
write_to_file(_f_pkgd_modd, '''result="Success"''')

_f_pkgm_init    = path_combine(testpath.public_testdir, _testdir, 'pkg_m', '__init__.py')
_f_pkgm_moda    = path_combine(testpath.public_testdir, _testdir, 'pkg_m', 'mod_a.py')
_f_pkgm_modb    = path_combine(testpath.public_testdir, _testdir, 'pkg_m', 'mod_b.py')
write_to_file(_f_pkgm_init, 'from ImportTestDir.pkg_m.mod_b import value_b')
write_to_file(_f_pkgm_moda, 'from ImportTestDir.pkg_m.mod_b import value_b')
write_to_file(_f_pkgm_modb, 'value_b = "ImportTestDir.pkg_m.mod_b.value_b"')

_f_pkgq_init    = path_combine(testpath.public_testdir, _testdir, 'pkg_q', '__init__.py')
_f_pkgr_init    = path_combine(testpath.public_testdir, _testdir, 'pkg_q', 'pkg_r', '__init__.py')
_f_pkgs_init    = path_combine(testpath.public_testdir, _testdir, 'pkg_q', 'pkg_r', 'pkg_s', '__init__.py')
_f_pkgs_mods    = path_combine(testpath.public_testdir, _testdir, 'pkg_q', 'pkg_r', 'pkg_s', 'mod_s.py')
write_to_file(_f_pkgq_init)
write_to_file(_f_pkgr_init)
write_to_file(_f_pkgs_init)
write_to_file(_f_pkgs_mods, 'result="Success"')

from ImportTestDir.pkg_m import mod_a
AreEqual(mod_a.value_b, "ImportTestDir.pkg_m.mod_b.value_b")

import ImportTestDir.usebuiltin as test
AreEqual(dir(test).count('x'), 1)       # defined variable, not a builtin
AreEqual(dir(test).count('min'), 1)     # defined name that overwrites a builtin
AreEqual(dir(test).count('max'), 0)     # used builtin, never assigned to
AreEqual(dir(test).count('cmp'), 0)     # used, assigned to, deleted, shouldn't be visibled
AreEqual(dir(test).count('del'), 0)     # assigned to, deleted, never used

#***** Above code are from 'Import' *****
#########################################################################################
#***** Copying from 'ImportAs' *****

import clr
clr.AddReferenceByPartialName("System.Windows.Forms")
import System.Windows.Forms as TestWinForms
form = TestWinForms.Form()
form.Text = "Hello"
Assert(form.Text == "Hello")

#***** Copying from 'Packages' *****

_f_pkg1 = path_combine(testpath.public_testdir, 'StandAlone\\Packages1.py')
_f_pkg2 = path_combine(testpath.public_testdir, 'StandAlone\\Packages2.py')
_f_mod  = path_combine(testpath.public_testdir, 'StandAlone\\ModPath\\IronPythonTest.py')
write_to_file(_f_pkg1, '''
import sys
sys.path.append(sys.path[0] + '\\..')

from lib.assert_util import *

import sys
sys.path.append(sys.path[0] +'\\ModPath')
import IronPythonTest

id1 = id(IronPythonTest)
AreEqual(dir(IronPythonTest).count('PythonFunc'), 1)

load_iron_python_test()

AreEqual(dir(IronPythonTest).count('PythonFunc'), 1)
AreEqual(dir(IronPythonTest).count('BindResult'), 0)

import IronPythonTest
id2 = id(IronPythonTest)

AreEqual(dir(IronPythonTest).count('PythonFunc'), 1)
AreEqual(dir(IronPythonTest).count('BindResult'), 1)

id3 = id(sys.modules['IronPythonTest'])

AreEqual(id1, id2)
AreEqual(id2, id3)
''')
write_to_file(_f_pkg2, '''
import sys
sys.path.append(sys.path[0] + '\\..')
from lib.assert_util import *

import sys
load_iron_python_test()

import IronPythonTest
id1 = id(IronPythonTest)
AreEqual(dir(IronPythonTest).count('BindResult'), 1)
AreEqual(dir(IronPythonTest).count('PythonFunc'), 0)

sys.path.append(sys.path[0] + '\\ModPath')
AreEqual(dir(IronPythonTest).count('PythonFunc'), 0)
AreEqual(dir(IronPythonTest).count('BindResult'), 1)

import IronPythonTest

AreEqual(dir(IronPythonTest).count('PythonFunc'), 1)
AreEqual(dir(IronPythonTest).count('BindResult'), 1)

id2 = id(IronPythonTest)

id3 = id(sys.modules['IronPythonTest'])

AreEqual(id1, id2)
AreEqual(id2, id3)
''')

write_to_file(_f_mod, 'def PythonFunc(): pass')

AreEqual(launch_ironpython_changing_extensions(_f_pkg1), 0)
AreEqual(launch_ironpython_changing_extensions(_f_pkg2), 0)


# remove all test files

delete_all_f(__name__)