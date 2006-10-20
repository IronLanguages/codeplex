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
from lib.file_util import *

import sys
import imp


def test_imp_new_module():
    x = imp.new_module('abc')
    sys.modules['abc'] = x
    x.foo = 'bar'
    import abc
    AreEqual(abc.foo, 'bar')

def test_imp_basic():
    magic = imp.get_magic()
    suffixes = imp.get_suffixes()
    Assert(isinstance(suffixes, list))
    for suffix in suffixes:
        Assert(isinstance(suffix, tuple))
        AreEqual(len(suffix), 3)
    Assert((".py", "U", 1) in suffixes)

_testdir = "ImpTest"
_imptestdir = path_combine(testpath.public_testdir, _testdir)
_f_init = path_combine(_imptestdir, "__init__.py")

def test_imp_package():
    write_to_file(_f_init, "my_name = 'imp package test'")
    pf, pp, (px, pm, pt) = imp.find_module(_testdir, [testpath.public_testdir])
    AreEqual(pt, imp.PKG_DIRECTORY)
    AreEqual(pf, None)
    AreEqual(px, "")
    AreEqual(pm, "")
    module = imp.load_module(_testdir, pf, pp, (px, pm, pt))
    AreEqual(module.my_name, 'imp package test')

    save_sys_path = sys.path
    try:
        sys.path = list(sys.path)
        sys.path.append(testpath.public_testdir)
        fm = imp.find_module(_testdir)
    finally:
        sys.path = save_sys_path
    # unpack the result obtained above
    pf, pp, (px, pm, pt) = fm
    AreEqual(pt, imp.PKG_DIRECTORY)
    AreEqual(pf, None)
    AreEqual(px, "")
    AreEqual(pm, "")
    module = imp.load_module(_testdir, pf, pp, (px, pm, pt))
    AreEqual(module.my_name, 'imp package test')

_f_module = path_combine(_imptestdir, "imptestmod.py")

def test_imp_module():
    write_to_file(_f_module, "value = 'imp test module'")
    pf, pp, (px, pm, pt) = imp.find_module("imptestmod", [_imptestdir])
    AreEqual(pt, imp.PY_SOURCE)
    Assert(pf != None)
    Assert(isinstance(pf, file))
    module = imp.load_module("imptestmod", pf, pp, (px, pm, pt))
    AreEqual(module.value, 'imp test module')
    pf.close()

    save_sys_path = sys.path
    try:
        sys.path = list(sys.path)
        sys.path.append(_imptestdir)
        fm = imp.find_module("imptestmod")
    finally:
        sys.path = save_sys_path
    # unpack the result obtained above
    pf, pp, (px, pm, pt) = fm
    AreEqual(pt, imp.PY_SOURCE)
    Assert(pf != None)
    Assert(isinstance(pf, file))
    AreEqual(px, ".py")
    AreEqual(pm, "U")
    module = imp.load_module("imptestmod", pf, pp, (px, pm, pt))
    AreEqual(module.value, 'imp test module')
    pf.close()

def test_direct_module_creation():
	import math
	import sys
	
	for baseMod in math, sys:
		module = type(baseMod)
		
		x = module.__new__(module)
		AreEqual(repr(x), "<module '?' (built-in)>")
		AreEqual(x.__dict__, None)
		
		x.__init__('abc', 'def')
		AreEqual(repr(x), "<module 'abc' (built-in)>")
		AreEqual(x.__doc__, 'def')
		
		x.__init__('aaa', 'zzz')
		AreEqual(repr(x), "<module 'aaa' (built-in)>")
		AreEqual(x.__doc__, 'zzz')
				
		# can't assign to module __dict__	 
		try:
			x.__dict__ = {}
		except TypeError: pass
		else: AssertUnreachable()
		
		# can't delete __dict__
		try:
			del(x.__dict__)
		except TypeError: pass
		else: AssertUnreachable()

		# init doesn't clobber dict, it just re-initializes values
		
		x.__dict__['foo'] = 'xyz'
		x.__init__('xyz', 'nnn')
		
		AreEqual(x.foo, 'xyz')
		
		# dict is lazily created on set
		x = module.__new__(module)
		x.foo = 23
		AreEqual(x.__dict__, {'foo':23})
		
		AreEqual(repr(x), "<module '?' (built-in)>")
		
		# can't pass wrong sub-type to new
		try:
			module.__new__(str)
		except TypeError: pass
		else: AssertUnreachable()

		# dir on non-initialized module raises TypeError
		x = module.__new__(module)
		
		AssertError(TypeError, dir, x)
		AssertError(SystemError, reload, x)
		x.__name__ = 'module_does_not_exist_in_sys_dot_modules'
		AssertError(ImportError, reload, x)

def test_module_dict():
    currentModule = sys.modules[__name__]
    AreEqual(type({}), type(currentModule.__dict__))
    AreEqual(isinstance(currentModule.__dict__, dict), True)

run_test(__name__)
delete_all_f(__name__)
