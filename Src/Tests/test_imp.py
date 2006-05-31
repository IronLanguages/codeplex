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

run_test(__name__)
delete_all_f(__name__)
