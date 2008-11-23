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

"""
This module consists of regression tests for CodePlex and Dev10 IronPython bugs
added primarily by IP developers that need to be folded into other test modules
and packages.

Any test case added to this file should be of the form:
    def test_cp1234(): ...
where 'cp' refers to the fact that the test case is for a regression on CodePlex
(use 'dev10' for Dev10 bugs).  '1234' should refer to the CodePlex or Dev10
Work Item number.
"""

#------------------------------------------------------------------------------
#--Imports
from iptest.assert_util import *
from iptest.process_util import launch

#------------------------------------------------------------------------------
#--Globals

#------------------------------------------------------------------------------
#--Test cases
@skip("win32", "silverlight")
def test_cp18345():
    import System
    import time
    class x(object):
        def f(self):
            global z
            z = 100
            
    System.AppDomain.CurrentDomain.DoCallBack(x().f)
    time.sleep(10)
    AreEqual(z, 100)

#------------------------------------------------------------------------------
@skip("silverlight")
def test_cp17420():
    #Create a temporary Python file
    test_file_name = path_combine(testpath.temporary_dir, "cp17420.py")
    test_log_name  = path_combine(testpath.temporary_dir, "cp17420.log")
    try:
        nt.remove(test_log_name)
    except:
        pass
    
    test_file = '''
output = []
for i in xrange(0, 100):
    output.append(str(i) + "\\n")

file("%s", "w").writelines(output)''' % (test_log_name)
    
    write_to_file(test_file_name, test_file)

    #Execute the file from a separate process
    AreEqual(launch(sys.executable, test_file_name), 0)
    
    #Verify contents of file
    temp_file = open(test_log_name, "r")
    lines = temp_file.readlines()
    temp_file.close()
    AreEqual(len(lines), 100)
    
#------------------------------------------------------------------------------
def test_cp17274():
    class KOld:
        def __init__(self):
            self.__doc__ = "KOld doc"
            
    class KNew(object):
        def __init__(self):
            self.__doc__ = "KNew doc"
            
    class KNewDerived(KNew, KOld):
        def method(self):
            self.__doc__ = "KNewDerived doc"
            
    class KNewDerivedSpecial(int):
        def __init__(self):
            self.__doc__ = "KNewDerivedSpecial doc"

    AreEqual(KOld().__doc__, "KOld doc")
    AreEqual(KNew().__doc__, "KNew doc")
    k = KNewDerived()
    AreEqual(k.__doc__, "KNew doc")
    k.method()
    AreEqual(k.__doc__, "KNewDerived doc")
    AreEqual(KNewDerivedSpecial().__doc__, "KNewDerivedSpecial doc")

#------------------------------------------------------------------------------
@skip("win32", "silverlight")
def test_cp16831():
    import clr
    clr.AddReference("IronPythonTest")
    import IronPythonTest
    temp = IronPythonTest.NullableTest()
    
    temp.BProperty = True
    for i in xrange(2):
        if not temp.BProperty:
            Fail("Nullable Boolean was set to True")
    for i in xrange(2):
        if not temp.BProperty==True:
            Fail("Nullable Boolean was set to True")
            
    temp.BProperty = False
    for i in xrange(2):
        if temp.BProperty:
            Fail("Nullable Boolean was set to False")
    for i in xrange(2):
        if not temp.BProperty==False:
            Fail("Nullable Boolean was set to False")
            
    temp.BProperty = None
    for i in xrange(2):
        if temp.BProperty:
            Fail("Nullable Boolean was set to None")
    for i in xrange(2):
        if not temp.BProperty==None:
            Fail("Nullable Boolean was set to None")           

def test_cp19675():
    class MyFloatType(float):
        def __new__(cls):
            return float.__new__(cls, 0.0)
        def __repr__(self):
            return "MyFloat"
        __str__ = __repr__
    
    AreEqual(str(MyFloatType()), 'MyFloat')
    AreEqual(repr(MyFloatType()), 'MyFloat')

def test_aaa_cp19656():
    # needs to run before import System
    import sys
    self = sys.modules[__name__]
    global Keys
    Keys = 'abc'
    AreEqual(self.Keys, 'abc')

@skip("win32")
def test_bbb_cp19656():
    # needs to run before import System
    import operator
    AreEqual(operator.isSequenceType(type), False)
    import System
    AreEqual(operator.isSequenceType(type), True)
    
def test_cp19678():
    global iterCalled, getItemCalled
    iterCalled = False
    getItemCalled = False
    class o(object):
        def __iter__(self):
            global iterCalled
            iterCalled = True
            return iter([1, 2, 3])
        def __getitem__(self, index):
            global getItemCalled
            getItemCalled = True
            return [1, 2, 3][index]
        def __len__(self):
            return 3

    AreEqual(1 in o(), True)
    AreEqual(iterCalled, True)
    AreEqual(getItemCalled, False)

#------------------------------------------------------------------------------
#--Main
run_test(__name__)
