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

# !!! DO NOT MOVE OR CHANGE THE FOLLOWING LINES
def _raise_exception():
    raise Exception()

_retb = (18, 0, 'test_traceback.py', '_raise_exception')

from lib.assert_util import *
from lib.file_util import *
if not is_cli: import os
def _raise_exception_with_finally():
    try:
        raise Exception()
    finally:
        pass

_rewftb= (27, 0, 'test_traceback.py', '_raise_exception_with_finally')

def assert_traceback(expected):
    import sys
    tb = sys.exc_info()[2]
    
    if expected is None: 
        AreEqual(expected, None)
    else:
        tb_list = []
        while tb is not None :
            f = tb.tb_frame
            co = f.f_code
            filename = co.co_filename
            name = co.co_name
            tb_list.append((tb.tb_lineno, tb.tb_lasti, filename, name))
            tb = tb.tb_next
        
        #print tb_list
        
        AreEqual(len(expected), len(tb_list))
        
        for x in range(len(expected)):
            AreEqual(expected[x][0], tb_list[x][0])
            AreEqual(expected[x][2:], tb_list[x][2:])
            

def test_no_traceback():
    #assert_traceback(None)
    try:    
        _raise_exception()
    except: 
        pass
    assert_traceback(None)

FILE="test_traceback.py"

LINE100 = 69
def test_catch_others_exception():
    try:
        _raise_exception()
    except: 
        assert_traceback([(LINE100 + 2, 0, FILE, 'test_catch_others_exception'), _retb])

LINE110 = 76
def test_catch_its_own_exception():
    try:
        raise Exception()
    except: 
        assert_traceback([(LINE110 + 2, 0, FILE, 'test_catch_its_own_exception')])

LINE120 = 83
def test_catch_others_exception_with_finally():
    try:
        _raise_exception_with_finally()
    except: 
        assert_traceback([(LINE120 + 2, 0, FILE, 'test_catch_others_exception_with_finally'), _rewftb])

LINE130 = 90        
def test_nested_caught_outside():
    try:
        x = 2
        try: 
            _raise_exception()
        except NameError:
            Assert(False, "unhittable")
        y = 2
    except:
        assert_traceback([(LINE130 + 4, 0, FILE, 'test_nested_caught_outside'), _retb])

LINE140 = 102
def test_nested_caught_inside():
    try:
        x = 2
        try: 
            _raise_exception()
        except:
            assert_traceback([(LINE140 + 4, 0, FILE, 'test_nested_caught_inside'), _retb])
        y = 2
    except:
        assert_traceback(None)

LINE150 = 114
def xtest_throw_in_except():
    try: 
        _raise_exception()
    except:
        assert_traceback([(LINE150+2, 0, FILE, 'test_throw_in_except'), _retb])
        try: 
            assert_traceback([(LINE150+2, 0, FILE, 'test_throw_in_except'), _retb])
            _raise_exception()
        except: 
            assert_traceback([(LINE150+7, 0, FILE, 'test_throw_in_except'), _retb])
    assert_traceback([(LINE150+7, 0, FILE, 'test_throw_in_except'), _retb])

LINE160 = 127    
class C1:
    def M(self): 
        try: 
            _raise_exception()
        except:
            assert_traceback([(LINE160 + 3, 0, FILE, 'M'), _retb])

def test_throw_in_method():
    c = C1()
    c.M()

LINE170 = 139    
def test_throw_when_defining_class():
    class C2(object):
        try: 
            _raise_exception()
        except:
            assert_traceback([(LINE170 + 3, 0, FILE, 'C2'), _retb])

def throw_when_defining_class_directly():
    class C3(C1):
        _raise_exception()

LINE180 = 151
def test_throw_when_defining_class_directly():
    try: 
        throw_when_defining_class_directly()
    except:
        assert_traceback([(LINE180 + 2, 0, FILE, 'test_throw_when_defining_class_directly'), 
        (LINE180 - 4, 0, FILE, 'throw_when_defining_class_directly'), 
        (LINE180 - 3, 0, FILE, 'C3'), _retb])
LINE200 = 160   
@skip("win32") #CodePlex Work Item #8291
def test_compiled_code():
    try:
        codeobj = compile('\nraise Exception()', '<mycode>', 'exec')
        exec(codeobj, {})
    except:
        assert_traceback([(LINE200+3, 0, FILE, 'test_compiled_code'), (2, 0, '<mycode>', '<module>')])

def generator_throw_before_yield():
    _raise_exception()
    yield 1
    
LINE210 = 172
def xtest_throw_before_yield():
    try: 
        for x in generator_throw_before_yield():
            pass
    except:
        assert_traceback([])

def generator_throw_after_yield():
    yield 1
    _raise_exception()

LINE220 = 184
def xtest_throw_while_yield():
    try: 
        for x in generator_throw_while_yield():
            pass
    except:
        assert_traceback([])

def generator_yield_inside_try():
    try: 
        yield 1
        yield 2  
        _raise_exception()    
    except NameError: 
        pass

LINE230 = 200
def xtest_yield_inside_try():
    try: 
        for x in generator_yield_inside_try():
            pass
    except:
        assert_traceback([])

LINE240 = 208
def test_throw_and_throw():
    try:
        _raise_exception()
    except:
        assert_traceback([(LINE240 + 2, 0, FILE, 'test_throw_and_throw'), _retb])
    try:
        _raise_exception()
    except:
        assert_traceback([(LINE240 + 6, 0, FILE, 'test_throw_and_throw'), _retb])
LINE250 = 219 
def test_throw_in_another_file():
    if is_cli: _f_file = path_combine(testpath.public_testdir, 'foo.py')
    else: _f_file = os.getcwd() + '\\foo.py'
    write_to_file(_f_file, '''
def another_raise():
    raise Exception()
''');
    try:
        import foo
        foo.another_raise()
    except:
        assert_traceback([(LINE250 + 8, 0, FILE, 'test_throw_in_another_file'), (3, 0, _f_file, 'another_raise')])
    finally:
        nt.remove(_f_file)

class MyException(Exception): pass

Line260 = 236
def catch_MyException():
    try:
        _raise_exception()
    except MyException:
        assert_traceback([])  # UNREACABLE. THIS TRICK SIMPLIFIES THE CHECK

def test_catch_MyException():
    try:
        catch_MyException()
    except:
        assert_traceback([(Line260+8, 0, FILE, 'test_catch_MyException'), (Line260+2, 0, FILE, 'catch_MyException'), _retb])

run_test(__name__)
