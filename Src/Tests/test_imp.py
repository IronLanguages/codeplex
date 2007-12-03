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



temp_name = ["nt",
             "nt.P_WAIT",
             "nt.chmod",
             "sys.path",
             "xxxx"
            ]

@skip('silverlight')
def test_imp_package():
    write_to_file(_f_init, "my_name = 'imp package test'")
    pf, pp, (px, pm, pt) = imp.find_module(_testdir, [testpath.public_testdir])
    AreEqual(pt, imp.PKG_DIRECTORY)
    AreEqual(pf, None)
    AreEqual(px, "")
    AreEqual(pm, "")
    module = imp.load_module(_testdir, pf, pp, (px, pm, pt))
    Assert(_testdir in sys.modules)
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
        
    # nt module disabled in Silverlight
    if not is_silverlight:
        AreEqual(imp.is_builtin("nt"),1)
        
    AreEqual(imp.is_builtin("thread"),1)
    
    
    # there are a several differences between ironpython and cpython
    if is_cli or is_silverlight:
        AreEqual(imp.is_builtin("copy_reg"),1)
        AreEqual(imp.is_builtin("sys"),0)
        AreEqual(imp.is_builtin("__builtin__"),1)
    else:
        AreEqual(imp.is_builtin("copy_reg"),0)
        AreEqual(imp.is_builtin("sys"),-1)
        AreEqual(imp.is_builtin("__builtin__"),-1)
        

@skip("win32")
def test_sys_path_none_builtins():
    prevPath = sys.path

    #import some builtin modules not previously imported
    try:
        sys.path = [None] + prevPath
        Assert('datetime' not in sys.modules.keys())
        import datetime
        Assert('datetime' in sys.modules.keys())
        
        sys.path = prevPath + [None]
        Assert('copy_reg' not in sys.modules.keys())
        import datetime
        import copy_reg
        Assert('datetime' in sys.modules.keys())
        Assert('copy_reg' in sys.modules.keys())
        
        sys.path = [None]
        Assert('cStringIO' not in sys.modules.keys())
        import datetime
        import copy_reg
        import cStringIO
        Assert('datetime' in sys.modules.keys())
        Assert('copy_reg' in sys.modules.keys())
        Assert('cStringIO' in sys.modules.keys())
        
    finally:
        sys.path = prevPath


@skip("silverlight")        
def test_sys_path_none_userpy():
    prevPath = sys.path

    #import a *.py file
    temp_syspath_none = path_combine(testpath.public_testdir, "temp_syspath_none.py")
    write_to_file(temp_syspath_none, "stuff = 3.14")
    
    try:
        sys.path = [None] + prevPath
        import temp_syspath_none
        AreEqual(temp_syspath_none.stuff, 3.14)
        
    finally:
        sys.path = prevPath


def test_sys_path_none_negative():
    prevPath = sys.path
    test_paths = [  [None] + prevPath,
                    prevPath + [None],
                    [None],
                 ]
                 
    try:
        for temp_path in test_paths:
            
            sys.path = temp_path
            try:
                import does_not_exist
                AssertUnerachable()
            except ImportError:
                pass       
    finally:
        sys.path = prevPath


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
    
    
def test_user_defined_modules():
    """test the importer using user-defined module types"""
    class MockModule(object):
        def __init__(self, name): self.__name__ = name
        def __repr__(self): return 'MockModule("' + self.__name__ + '")'

    TopModule = MockModule("TopModule")
    sys.modules["TopModule"] = TopModule
    
    SubModule = MockModule("SubModule")
    theObj = object()
    SubModule.Object = theObj
    TopModule.SubModule = SubModule
    sys.modules["TopModule.SubModule"] = SubModule
    
    # clear the existing names from our namespace...
    x, y = TopModule, SubModule
    del TopModule, SubModule
    
    # verify we can import TopModule w/ TopModule.SubModule name
    import TopModule.SubModule
    AreEqual(TopModule, x)
    Assert('SubModule' not in dir())
        
    # verify we can import Object from TopModule.SubModule
    from TopModule.SubModule import Object
    AreEqual(Object, theObj)
    
    # verify we short-circuit the lookup in TopModule if 
    # we have a sys.modules entry...
    SubModule2 = MockModule("SubModule2")    
    SubModule2.Object2 = theObj    
    sys.modules["TopModule.SubModule"] = SubModule2
    from TopModule.SubModule import Object2    
    AreEqual(Object2, theObj)
    
    del sys.modules['TopModule']
    del sys.modules['TopModule.SubModule']
    
def test_constructed_module():    
    """verify that we don't load arbitrary modules from modules, only truly nested modules"""
    ModuleType = type(sys)

    TopModule = ModuleType("TopModule")
    sys.modules["TopModule"] = TopModule

    SubModule = ModuleType("SubModule")
    SubModule.Object = object()
    TopModule.SubModule = SubModule

    try:
        import TopModule.SubModule
        AssertUnreachable()
    except ImportError:
        pass

    del sys.modules['TopModule']

def test_import_from_custom():
    import __builtin__
    try:
        class foo(object):
            b = 'abc'
        def __import__(name, globals, locals, fromlist):
            global received
            received = name, fromlist
            return foo()
    
        saved = __builtin__.__import__ 
        __builtin__.__import__ = __import__
    
        from a import b
        AreEqual(received, ('a', ('b', )))
    finally:
        __builtin__.__import__ = saved
        
run_test(__name__)
if is_silverlight==False:
    delete_all_f(__name__)
