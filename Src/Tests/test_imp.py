#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

from lib.assert_util import *
if is_silverlight==False:
    from lib.file_util import *

import sys
import imp
import operator

def test_imp_new_module():
    x = imp.new_module('abc')
    sys.modules['abc'] = x
    x.foo = 'bar'
    import abc
    AreEqual(abc.foo, 'bar')

@skip("silverlight")
def test_imp_in_exec():
    _imfp    = 'impmodfrmpkg'
    _f_imfp_init = path_combine(testpath.public_testdir, _imfp, "__init__.py")
    _f_imfp_mod  = path_combine(testpath.public_testdir, _imfp, "mod.py")
    _f_imfp_start = path_combine(testpath.public_testdir, "imfpstart.tpy")
    
    write_to_file(_f_imfp_init, "")
    write_to_file(_f_imfp_mod, "")
    write_to_file(_f_imfp_start, """
try:
    from impmodfrmpkg.mod import mod
except ImportError, e:
    pass
else:
    raise AssertionError("Import of mod from pkg.mod unexpectedly succeeded")
    """)

    # import a package
    import impmodfrmpkg
    
    # create a dictionary like that package
    glb = {'__name__' : impmodfrmpkg.__name__, '__path__' : impmodfrmpkg.__path__}
    loc = {}
    
    exec 'import mod' in glb, loc
    Assert('mod' in loc)
    
    glb = {'__name__' : impmodfrmpkg.__name__, '__path__' : impmodfrmpkg.__path__}
    loc = {}
    exec 'from mod import *' in glb, loc
    #Assert('value' in loc)         # TODO: Fix me
    
    if is_cli or is_silverlight:
        loc = {}
        exec 'from System import *' in globals(), loc
        
        Assert('Int32' in loc)
        Assert('Int32' not in globals())
        
        if is_cli or is_silverlight:
            exec 'from System import *'
            Assert('Int32' in dir())

def test_imp_basic():
    magic = imp.get_magic()
    suffixes = imp.get_suffixes()
    Assert(isinstance(suffixes, list))
    for suffix in suffixes:
        Assert(isinstance(suffix, tuple))
        AreEqual(len(suffix), 3)
    Assert((".py", "U", 1) in suffixes)

if is_silverlight==False:
    _testdir = "ImpTest"
    _imptestdir = path_combine(testpath.public_testdir, _testdir)
    _f_init = path_combine(_imptestdir, "__init__.py")

@skip('silverlight')
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

if is_silverlight==False:
    _f_module = path_combine(_imptestdir, "imptestmod.py")

@skip('silverlight')
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
        #AreEqual(x.__dict__, None)
        
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
        
        x.__name__ = 'module_does_not_exist_in_sys_dot_modules'
        AssertError(ImportError, reload, x)
   
def test_redefine_import():
    # redefining global __import__ shouldn't change import semantics
    global __import__
    global called
    called = False
    def __import__(*args):
        global called
        called = True
    import sys      
    AreEqual(called, False)
    del __import__
    called = False
    import sys
    AreEqual(called, False)
   
def test_module_dict():
    currentModule = sys.modules[__name__]
    AreEqual(operator.isMappingType(currentModule.__dict__), True)
run_test(__name__)
if is_silverlight==False:
    delete_all_f(__name__)
