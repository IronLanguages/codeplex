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
import sys

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

file(r"%s", "w").writelines(output)''' % (test_log_name)
    
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

@skip("win32")
def test_protected_ctor_inheritance_cp20021():
    load_iron_python_test()
    from IronPythonTest import (
        ProtectedCtorTest, ProtectedCtorTest1, ProtectedCtorTest2, 
        ProtectedCtorTest3, ProtectedCtorTest4,
        ProtectedInternalCtorTest, ProtectedInternalCtorTest1, 
        ProtectedInternalCtorTest2, ProtectedInternalCtorTest3, 
        ProtectedInternalCtorTest4
        
    )
    
    # no number: 
    protected = [ProtectedCtorTest, ProtectedCtorTest1, ProtectedCtorTest2, 
                 ProtectedCtorTest3, ProtectedCtorTest4, ]
    protected_internal = [ProtectedInternalCtorTest, ProtectedInternalCtorTest1,
                          ProtectedInternalCtorTest2, ProtectedInternalCtorTest3, 
                          ProtectedInternalCtorTest4, ]
    
    for zero, one, two, three, four in (protected, protected_internal):      
        # calling protected ctors shouldn't work
        AssertError(TypeError, zero)
        AssertError(TypeError, zero.__new__)
        
        AssertError(TypeError, one, object())
        AssertError(TypeError, one.__new__, object())
        
        AssertError(TypeError, two, object())
        AssertError(TypeError, two.__new__, two, object())
        AssertError(TypeError, two, object(), object())
        AssertError(TypeError, two.__new__, two, object(), object())
        
        AssertError(TypeError, three)
        AssertError(TypeError, three.__new__, three)
        
        three(object())
        three.__new__(ProtectedCtorTest3, object())
        
        AssertError(TypeError, four, object())
        AssertError(TypeError, four.__new__, four, object())
        
        four()
        four.__new__(four)
        
        class myzero(zero):
            def __new__(cls): return zero.__new__(cls)
        class myone(one):
            def __new__(cls): return one.__new__(cls, object())
        class mytwo1(two):
            def __new__(cls): return two.__new__(cls, object())
        class mytwo2(two):
            def __new__(cls): return two.__new__(cls, object(), object())
        class mythree1(three):
            def __new__(cls): return three.__new__(cls)
        class mythree2(three):
            def __new__(cls): return three.__new__(cls, object())
        class myfour1(four):
            def __new__(cls): return four.__new__(cls)
        class myfour2(four):
            def __new__(cls): return four.__new__(cls, object())

        for cls in [myzero, myone, mytwo1, mytwo2, mythree1, mythree2, myfour1, myfour2]:
            cls()

def test_re_paren_in_char_list_cp20191():
    import re
    format_re = re.compile(r'(?P<order1>[<>|=]?)(?P<repeats> *[(]?[ ,0-9]*[)]? *)(?P<order2>[<>|=]?)(?P<dtype>[A-Za-z0-9.]*)')
    
    AreEqual(format_re.match('a3').groups(), ('', '', '', 'a3'))

@skip("silverlight")
def test_struct_uint_bad_value_cp20039():
    class x(object):
        def __init__(self, value):
            self.value = value
        def __and__(self, other):
            global andCalled
            andCalled = True
            return self.value
        def __int__(self):
            raise Exception('foo')

    import _struct
    global andCalled
    AreEqual(_struct.Struct('L').pack(4294967296), '\x00\x00\x00\x00')
    AreEqual(_struct.Struct('L').pack(-1), '\xff\xff\xff\xff')
    AreEqual(_struct.Struct('L').pack(x(0)), '\x00\x00\x00\x00')
    AreEqual(andCalled, True)
    AreEqual(_struct.Struct('I').pack(4294967296), '\x00\x00\x00\x00')
    AreEqual(_struct.Struct('I').pack(-1), '\xff\xff\xff\xff')
    andCalled = False
    AreEqual(_struct.Struct('I').pack(x(0)), '\x00\x00\x00\x00')
    AreEqual(andCalled, True)
    AssertError(OverflowError, _struct.Struct('I').pack, x(-1))
    AssertError(OverflowError, _struct.Struct('L').pack, x(-1))

def test_reraise_backtrace_cp20051():
    '''
    TODO: this test needs far better verification.
    '''
    import sys
    def foo():
        some_exception_raising_code()
    
    try:
        try:
            foo()
        except:
            excinfo1 = sys.exc_info()[2]
            exc1_list = []
            while excinfo1:
                exc1_list.append((excinfo1.tb_frame.f_code.co_filename, 
                                  excinfo1.tb_frame.f_code.co_name,
                                  excinfo1.tb_frame.f_lineno))
                excinfo1 = excinfo1.tb_next
            raise
    except Exception, e:
        excinfo2 = sys.exc_info()[2]
        exc2_list = []
        while excinfo2:
            exc2_list.append((excinfo2.tb_frame.f_code.co_filename, 
                              excinfo2.tb_frame.f_code.co_name,
                              excinfo2.tb_frame.f_lineno))
            excinfo2 = excinfo2.tb_next

        # CPython reports 2 frames, IroPython includes the re-raise and reports 3
        Assert(len(exc2_list) >= 2)

@skip("silverlight")
def test_winreg_error_cp17050():
    import _winreg
    AreEqual(_winreg.error, WindowsError)


@skip("win32", "silverlight")
def test_indexing_value_types_cp20370():
    clr.AddReference("System.Drawing")
    from System.Drawing import Point
    
    p = Point(1,2)
    l = [None]
    l[0] = p
    AreEqual(id(l[0]), id(p))
    AreEqual(id(l[0]), id(p))
    
    x = {}
    x[p] = p
    AreEqual(id(list(x.iterkeys())[0]), id(p))
    AreEqual(id(list(x.itervalues())[0]), id(p))
    
    load_iron_python_test()
    
    from IronPythonTest import StructIndexable
    a = StructIndexable()
    a[0] = 1
    AreEqual(a[0], 1)

def test_enumerate_index_increment_cp20016():
    def f(item):
        return item[0] in [0, 1]
    
    AreEqual(filter(f, enumerate(['a', 'b'])), [(0, 'a'), (1, 'b')])
    AreEqual(filter( lambda (j, _): j in [0, 1], enumerate([10.0, 27.0])),
             [(0, 10.0), (1, 27.0)])

@skip("silverlight")
def test_invalid_args_cp20616():
    test_cases = {
        lambda: ''.join() : "join() takes exactly one argument (0 given)",
        lambda: ''.join("", "") : "join() takes exactly one argument (2 given)",
        lambda: ''.join("", "", "") : "join() takes exactly one argument (3 given)",
        lambda: ''.replace("", "", "", "") : "replace() takes at most 3 arguments (4 given)",
    }
    if is_cli:
        test_cases.update({
                            lambda: System.String("").PadRight() : "PadRight() takes at least 1 argument (0 given)",
                            lambda: System.String("").PadRight(1, "a", "") : "PadRight() takes at most 2 arguments (3 given)",
                          })
    #CodePlex 21063
    if is_cli:
        for key in test_cases:
            test_cases[key] = test_cases[key].replace("one", "1")
    
    
    for key in test_cases:
        temp_lambda = key
        expected_err_msg = test_cases[key]
        AssertErrorWithMessage(TypeError, expected_err_msg, temp_lambda)

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


def test_exception_multiple_inheritance_cp20208():
    class FTPError(Exception): pass
    class FTPOSError(FTPError, OSError): pass
    
    AreEqual(FTPOSError, type(FTPOSError()))

def test_conversions_cp19675():
    class MyFloatType(float):
        def __int__(self):
            return 42    
        def __str__(self):
            return 'hello'
            
    MyFloat = MyFloatType()
    AreEqual(int(MyFloat), 42)
    AreEqual(str(MyFloat), 'hello')

    class MyFloatType(float): pass
    MyFloat = MyFloatType()
    AreEqual(int(MyFloat), 0)
    AreEqual(str(MyFloat), '0.0')

    class MyFloatType(float):
        def __new__(cls):
            return float.__new__(cls, 3.14)
    
    MyFloat = MyFloatType()
    AreEqual(MyFloat, 3.14)
    AreEqual(int(MyFloat), 3)
    

@skip("win32")
def test_type_delegate_conversion():
    import clr
    if is_net40:
      from System import Func    
    else:
      clr.AddReference('Microsoft.Scripting.Core')
      from Microsoft.Scripting.Utils import Func    
      
    class x(object): pass
    ctor = Func[object](x)
    AreEqual(type(ctor()), x)


def test_module_alias_cp19656():
    stuff_mod = path_combine(testpath.public_testdir, "stuff.py")
    check_mod = path_combine(testpath.public_testdir, "check.py")
    
    try:
        write_to_file(stuff_mod, "Keys = 3")
        write_to_file(check_mod, "def check(module):\n    return module.Keys")
        import stuff
        from check import check
        AreEqual(check(stuff), 3)
    finally:
        import nt
        nt.unlink(stuff_mod)
        nt.unlink(check_mod)

def test_cp24691():
    import os
    pwd = os.getcwd()
    AreEqual(os.path.abspath("bad:"),
             os.getcwd() + "\\bad:")

def test_cp24690():
    import errno
    AreEqual(errno.errorcode[2],
             "ENOENT")

def test_cp24692():
    import errno, nt, stat
    dir_name = "cp24692_testdir"
    try:
        nt.mkdir(dir_name)
        nt.chmod(dir_name, stat.S_IREAD)
        try:
            nt.rmdir(dir_name)
        except WindowsError, e:
            pass
        AreEqual(e.errno, errno.EACCES)
    finally:
        nt.chmod(dir_name, stat.S_IWRITE)
        nt.rmdir(dir_name)

# TODO: this test needs to run against Dev10 builds as well
@skip("win32")
def test_cp22735():
    import System
    if System.Environment.Version.Major < 4:
        clr.AddReference("System.Core")
    from System import Func

#------------------------------------------------------------------------------
#--General coverage.  These need to be extended.
def test_xxsubtype_bench():
    import xxsubtype
    AreEqual(type(xxsubtype.bench(xxsubtype, "bench")),
             float)

def test_str_ljust_cp21483():
    AreEqual('abc'.ljust(-2147483648), 'abc')
    AreEqual('abc'.ljust(-2147483647), 'abc')
    AssertError(OverflowError, #"long int too large to convert to int",
                'abc'.ljust, -2147483649L)


@skip("win32")
def test_help_dir_cp11833():
    import System
    Assert(dir(System).count('Action') == 1)
    from cStringIO import StringIO
    oldstdout, sys.stdout = sys.stdout, StringIO()
    try:
        help(System.Action)
    finally:
        sys.stdout = oldstdout
    Assert(dir(System).count('Action') == 1)


def test_not___len___cp_24129():
    class C(object):
        def __len__(self):
            return 3
    
    c = C()
    print bool(c)
    AreEqual(not c, False)

@skip("win32")
def test_cp18912():
    import __future__
    feature = __future__.__dict__['with_statement']
    x = compile('x=1', 'ignored', 'exec', feature.compiler_flag)

def test_cp19789():
    class A:
        a = 1
    
    class B(object):
        b = 2
    
    class C(A, B):
        pass
    
    AreEqual(dir(A),
             ['__doc__', '__module__', 'a'])
    Assert('b' in dir(B))
    Assert('a' in dir(C) and 'b' in dir(C))

def test_cp24573():
    def f(a=None):
        pass
        
    AssertErrorWithMessage(TypeError, "f() got multiple values for keyword argument 'a'",
                           lambda: f(1, a=3))

@skip("win32")
def test_cp24802():
    import clr
    clr.AddReference('System.Drawing')
    import System
    p = System.Drawing.Pen(System.Drawing.Color.Blue)
    p.Width = System.Single(3.14)
    AreEqual(p.Width, System.Single(3.14))
    p.Width = 4.0
    AreEqual(p.Width, 4.0)

#------------------------------------------------------------------------------
# This is not a regression, but need to find the right place to move this test to

class MyException(IOError):
    def __str__(self):
        return "MyException is a user sub-type of IOError"

@skip("win32")
def test_clr_exception_has_non_trivial_exception_message():
    import System
    try:
        raise MyException
    except System.Exception as e:
        pass
    AreEqual(e.Message, "Python Exception: MyException")

def test_cp23822():
    from copy import deepcopy
    def F():
        a = 4
        class C:
            field=7
            def G(self):
                print a
                b = 4
                return deepcopy(locals().keys())
        
        c = C()
        return c.G()
    
    temp_list = F()
    temp_list.sort()
    AreEqual(temp_list, ['a', 'b', 'deepcopy', 'self'])
    
def test_cp23823():
    from copy import deepcopy
    def f():
        a = 10
        def g1():
            print a
            return deepcopy(locals().keys())
        def g2():
            return deepcopy(locals().keys())
        return (g1(), g2())
    
    AreEqual(f(), (['a', 'deepcopy'], ['deepcopy']))


#------------------------------------------------------------------------------
#--Main
run_test(__name__)