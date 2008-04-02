#####################################################################################
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public
# License. A  copy of the license can be found in the License.html file at the
# root of this distribution. If  you cannot locate the  Microsoft Public
# License, please send an email to  dlr@microsoft.com. By using this source
# code in any fashion, you are agreeing to be bound by the terms of the 
# Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#####################################################################################

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

temp_name = ["nt",
             "nt.P_WAIT",
             "nt.chmod",
             "sys.path",
             "xxxx"
            ]

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

#test release_lock,lock_held,acquire_lock
def test_lock():
    i=0
    while i<5:
        i+=1
        if not imp.lock_held():
            AssertError(RuntimeError,imp.release_lock)
            imp.acquire_lock()
        else:
            imp.release_lock()
       

# test is_frozen 
def test_is_frozen():
    for name in temp_name:
        f = imp.is_frozen(name)
        if f:
            Fail("result should be False")
            
# test init_frozen
def test_init_frozen():
    for name in temp_name:
        f = imp.init_frozen(name)
        if f != None :
            Fail("return object should be None!")
    
# is_builtin
def test_is_builtin():
   
    AreEqual(imp.is_builtin("xxx"),0)
    AreEqual(imp.is_builtin("12324"),0)
    AreEqual(imp.is_builtin("&*^^"),0)
    
    AreEqual(imp.is_builtin("dir"),0)
    AreEqual(imp.is_builtin("__doc__"),0)
    AreEqual(imp.is_builtin("__name__"),0)
    
    AreEqual(imp.is_builtin("_locle"),0)
    
    AreEqual(imp.is_builtin("cPickle"),1)
    AreEqual(imp.is_builtin("_random"),1)
    AreEqual(imp.is_builtin("nt"),1)
    AreEqual(imp.is_builtin("thread"),1)
    
    
    # there are a several differences between ironpython and cpython
    if is_cli:
        AreEqual(imp.is_builtin("copy_reg"),1)
        AreEqual(imp.is_builtin("sys"),0)
        AreEqual(imp.is_builtin("__builtin__"),1)
    else:
        AreEqual(imp.is_builtin("copy_reg"),0)
        AreEqual(imp.is_builtin("sys"),-1)
        AreEqual(imp.is_builtin("__builtin__"),-1)
        
    
 
#init_builtin           
def test_init_builtin():
    r  = imp.init_builtin("c_Pickle")
    AreEqual(r,None)
    
    r  = imp.init_builtin("2345")
    AreEqual(r,None)
    r  = imp.init_builtin("xxxx")
    AreEqual(r,None)
    r  = imp.init_builtin("^$%$#@")
    AreEqual(r,None)
    
    r  = imp.init_builtin("_locale")
    Assert(r!=None)
    
#test SEARCH_ERROR, PY_SOURCE,PY_COMPILED,C_EXTENSION,PY_RESOURCE,PKG_DIRECTORY,C_BUILTIN,PY_FROZEN,PY_CODERESOURCE
def test_flags():
    AreEqual(imp.SEARCH_ERROR,0)
    AreEqual(imp.PY_SOURCE,1)
    AreEqual(imp.PY_COMPILED,2)
    AreEqual(imp.C_EXTENSION,3)
    AreEqual(imp.PY_RESOURCE,4)
    AreEqual(imp.PKG_DIRECTORY,5)
    AreEqual(imp.C_BUILTIN,6)
    AreEqual(imp.PY_FROZEN,7)
    AreEqual(imp.PY_CODERESOURCE,8)
    
    
def test_imp_sys_path_none():
    import sys
    x = list(sys.path)
    try:
        sys.path = [None]
        try:
            import does_not_exist
        except ImportError:
            pass        
    finally:
        sys.path = x
run_test(__name__)
delete_all_f(__name__)
