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

#
# PART 2. how IronPython choose the overload methods
#

from lib.assert_util import *
from lib.type_util import *

load_iron_python_test()
from IronPythonTest.BinderTest import *

class PT_I(I): pass
class PT_C1(C1): pass
class PT_I_int(I): 
    def __int__(self): return 100

class PT_int_old: 
    def __int__(self): return 200
class PT_int_new(object): 
    def __int__(self): return 300
    
UInt32Min = System.UInt32.MaxValue
Byte10   = System.Byte.Parse('10')
SBytem10 = System.SByte.Parse('-10')
Int1610  = System.Int16.Parse('10')
Int16m20 = System.Int16.Parse('-20')
UInt163  = System.UInt16.Parse('3')

pt_i = PT_I()
pt_c1 = PT_C1()
pt_i_int = PT_I_int()
pt_int_old = PT_int_old()
pt_int_new = PT_int_new()

arrayInt = array_int((10, 20))
tupleInt = ((10, 20), )
listInt  = ([10, 20], )
tupleLong1, tupleLong2  = ((10L, 20L), ), ((System.Int64.MaxValue, System.Int32.MaxValue * 2),)    
arrayByte = array_byte((10, 20))
arrayObj = array_object(['str', 10])

def _self_defined_method(name): return len(name) == 4 and name[0] == "M"

def _result_pair(s, offset=0):
    fn = s.split()
    val = [int(x[1:]) + offset for x in fn]
    return dict(zip(fn, val))

def _first(s): return _result_pair(s, 0)
def _second(s): return _result_pair(s, 100)

def _merge(*args):
    ret = {}
    for arg in args:
        for (k, v) in arg.iteritems(): ret[k] = v
    return ret

def _my_call(func, arg):
    if isinstance(arg, tuple):
        l = len(arg)
        if l == 0: func()
        elif l == 1: func(arg[0])
        elif l == 2: func(arg[0], arg[1])
        elif l == 3: func(arg[0], arg[1], arg[2])
        elif l == 4: func(arg[0], arg[1], arg[2], arg[3])
        elif l == 5: func(arg[0], arg[1], arg[2], arg[3], arg[4])
        elif l == 6: func(arg[0], arg[1], arg[2], arg[3], arg[4], arg[5])
        else: func(*arg)
    else:
        func(arg)
    
def _try_arg(target, arg, mapping, funcOverflowError, funcValueError):
    '''try the pass-in argument 'arg' on all methods 'target' has.
       mapping specifies (method-name, flag-value)
       funcOverflowError contains method-name, which will cause OverflowError when passing in 'arg'
    '''
    print arg, 
    for funcname in dir(target):
        if not _self_defined_method(funcname) : continue
        
        print funcname, 
        func = getattr(target, funcname)

        if funcname in mapping.keys():
            _my_call(func, arg)
            left, right = Flag.Value, mapping[funcname]
            if left != right: 
                Fail("left %s != right %s when func %s on arg %s" % (left, right, funcname, arg))
            Flag.Value = -99           # reset 
        else:
            if funcname in funcOverflowError: expectError = OverflowError
            elif funcname in funcValueError:  expectError = ValueError
            else: expectError = TypeError
            
            try: _my_call(func, arg)
            except expectError: pass
            else: Fail("expect %s, but got no exception (flag %s) when func %s with arg %s" % (expectError, Flag.Value, funcname, arg))
    print 
    
def test_other_concerns():
    target = COtherOverloadConcern()
    
    # the one asking for Int32 is private
    target.M100(100)
    AreEqual(Flag.Value, 200); Flag.Value = 99
    
    # static / instance
    target.M110(target, 100)
    AreEqual(Flag.Value, 110); Flag.Value = 99
    COtherOverloadConcern.M110(100)
    AreEqual(Flag.Value, 210); Flag.Value = 99
    
    AssertError(TypeError, COtherOverloadConcern.M110, target, 100)
    
    # statics
    target.M120(target, 100)
    AreEqual(Flag.Value, 120); Flag.Value = 99
    target.M120(100)
    AreEqual(Flag.Value, 220); Flag.Value = 99    

    COtherOverloadConcern.M120(target, 100)
    AreEqual(Flag.Value, 120); Flag.Value = 99
    COtherOverloadConcern.M120(100)
    AreEqual(Flag.Value, 220); Flag.Value = 99    
    
    # generic
    for x in [100, 100.1234]: 
        target.M130(x)
        AreEqual(Flag.Value, 130); Flag.Value = 99

    AssertError(TypeError, target.M130, C1())

    for x in [100, 100.1234]: 
        target.M130[int](x)
        AreEqual(Flag.Value, 230); Flag.Value = 99
    

######### generated python code below #########

def test_arg_NoArgNecessary():
    target = COverloads_NoArgNecessary()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), _merge(_first('M100 M101 M102 M103 M104 M105 '), _second('M106 ')), [], [], ),
(         100, _merge(_first('M105 M106 '), _second('M101 M102 M103 M104 ')), [], [], ),
(  (100, 200), _second('M102 M104 M105 M106 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_OneArg_NormalArg():
    target = COverloads_OneArg_NormalArg()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), dict(), [], [], ),
(         100, _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 '), [], [], ),
(  (100, 200), _second('M102 M107 M108 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_OneArg_RefArg():
    target = COverloads_OneArg_RefArg()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), dict(), [], [], ),
(         100, _first('M100 M101 M103 M105 M106 M107 M108 '), [], [], ),
(  (100, 200), _second('M101 M106 M107 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_OneArg_NullableArg():
    target = COverloads_OneArg_NullableArg()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), dict(), [], [], ),
(         100, _merge(_first('M100 M107 '), _second('M101 M102 M103 M104 M105 M106 ')), [], [], ),
(  (100, 200), _second('M100 M105 M106 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_OneArg_TwoArgs():
    target = COverloads_OneArg_TwoArgs()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), dict(), [], [], ),
(         100, _second('M100 M101 M102 M103 M104 '), [], [], ),
(  (100, 200), _first('M100 M101 M102 M103 M104 M105 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_OneArg_NormalOut():
    target = COverloads_OneArg_NormalOut()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), dict(), [], [], ),
(         100, _first('M100 M102 M103 M104 M105 '), [], [], ),
(  (100, 200), _second('M103 M104 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_OneArg_RefOut():
    target = COverloads_OneArg_RefOut()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), dict(), [], [], ),
(         100, _merge(_first('M101 M102 M103 '), _second('M100 ')), [], [], ),
(  (100, 200), _second('M101 M102 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_OneArg_OutNormal():
    target = COverloads_OneArg_OutNormal()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), dict(), [], [], ),
(         100, _first('M100 M101 M102 M103 '), [], [], ),
(  (100, 200), _second('M101 M102 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_OneArg_OutRef():
    target = COverloads_OneArg_OutRef()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), dict(), [], [], ),
(         100, _first('M100 M101 M102 '), [], [], ),
(  (100, 200), _second('M100 M101 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_OneArg_NormalDefault():
    target = COverloads_OneArg_NormalDefault()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(     tuple(), dict(), [], [], ),
(         100, _first('M100 M101 '), [], [], ),
(  (100, 200), _first('M100 M101 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_String():
    target = COverloads_String()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(         'a', _merge(_first('M100 M101 '), _second('M102 ')), [], [], ),
(       'abc', _merge(_first('M100 M101 '), _second('M102 ')), [], [], ),
(  mystr('a'), _merge(_first('M100 M101 '), _second('M102 ')), [], [], ),
(mystr('abc'), _merge(_first('M100 M101 '), _second('M102 ')), [], [], ),
(           1, _first('M101 M102 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_Enum():
    target = COverloads_Enum()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(        E1.A, _first('M100 '), [], [], ),
(        E2.A, _first('M101 '), [], [], ),
(           1, _second('M100 M101 '), [], [], ),
(     UInt163, _second('M101 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_UserDefined():
    target = COverloads_UserDefined()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(        C1(), _merge(_first('M101 M102 M103 M104 '), _second('M100 ')), [], [], ),
(        C2(), _merge(_first('M102 M103 '), _second('M100 M101 M104 ')), [], [], ),
(        C3(), _second('M103 '), [], [], ),
(        S1(), _first('M100 M101 M102 M103 '), [], [], ),
(        C6(), _second('M103 M105 '), [], [], ),
(        pt_i, _first('M100 M101 M102 M103 '), [], [], ),
(       pt_c1, _merge(_first('M101 M102 M103 M104 '), _second('M100 ')), [], [], ),
(    pt_i_int, _first('M100 M101 M102 M103 '), [], [], ),
(  pt_int_old, _second('M102 M103 '), [], [], ),
(  pt_int_new, _second('M102 M103 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_Derived_Number():
    target = COverloads_Derived_Number()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(        None, _merge(_first('M106 '), _second('M102 M103 ')), [], [], ),
(        True, _merge(_first('M100 M101 M103 M106 '), _second('M102 M104 M105 ')), [], [], ),
(        -100, _merge(_first('M100 '), _second('M104 M105 M106 ')), [], [], ),
(        200L, _merge(_first('M103 M106 '), _second('M102 ')), [], [], ),
(      Byte10, _merge(_first('M103 '), _second('M100 M105 M106 ')), [], [], ),
(       12.34, _merge(_first('M103 M105 M106 '), _second('M101 M102 ')), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_Collections():
    target = COverloads_Collections()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(    arrayInt, _merge(_first('M100 '), _second('M101 M102 M103 M104 ')), [], [], ),
(    tupleInt, _merge(_first('M102 '), _second('M100 M101 M103 M104 ')), [], [], ),
(     listInt, _merge(_first('M102 '), _second('M100 M103 ')), [], [], ),
(  tupleLong1, _merge(_first('M102 '), _second('M100 M103 ')), [], [], ),
(  tupleLong2, _merge(_first('M102 '), _second('M100 M103 ')), [], [], ),
(   arrayByte, _first('M101 '), [], [], ),
(    arrayObj, _merge(_first('M101 M102 '), _second('M100 M103 ')), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_Boolean():
    target = COverloads_Boolean()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(        None, _second('M112 '), [], [], ),
(        True, _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(       False, _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(         100, _second('M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(  myint(100), _merge(_first('M100 '), _second('M106 M108 M110 M111 M112 ')), [], [], ),
(        -100, _second('M102 M104 M106 M108 M109 M110 M111 M112 '), [], [], ),
(   UInt32Min, _second('M105 M107 M108 M109 M110 M111 M112 '), [], [], ),
(        200L, _second('M101 M103 M104 M105 M106 M107 M108 M110 M111 M112 '), [], [], ),
(       -200L, _second('M104 M106 M108 M110 M111 M112 '), [], [], ),
(      Byte10, _second('M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(    SBytem10, _second('M102 M104 M106 M108 M109 M110 M111 M112 '), [], [], ),
(     Int1610, _second('M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(    Int16m20, _second('M102 M104 M106 M108 M109 M110 M111 M112 '), [], [], ),
(       12.34, _second('M106 M110 M111 M112 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_Byte():
    target = COverloads_Byte()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(        None, _second('M112 '), [], [], ),
(        True, _second('M100 M106 M112 '), [], [], ),
(       False, _second('M100 M106 M112 '), [], [], ),
(         100, _merge(_first('M100 M101 '), _second('M106 M108 M109 M110 M111 M112 ')), [], [], ),
(  myint(100), _second('M106 M108 M110 M111 M112 '), [], [], ),
(        -100, _second('M106 M108 M109 M110 M111 M112 '), [], [], ),
(   UInt32Min, _second('M105 M107 M108 M109 M110 M111 M112 '), [], [], ),
(        200L, _merge(_first('M100 M101 '), _second('M108 M112 ')), [], [], ),
(       -200L, _second('M108 M112 '), [], [], ),
(      Byte10, _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(    SBytem10, _second('M102 M104 M106 M108 M109 M110 M111 M112 '), [], [], ),
(     Int1610, _merge(_first('M100 M101 '), _second('M104 M106 M108 M109 M110 M111 M112 ')), [], [], ),
(    Int16m20, _second('M104 M106 M108 M109 M110 M111 M112 '), [], [], ),
(       12.34, _second('M111 M112 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_Int16():
    target = COverloads_Int16()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(        None, _second('M112 '), [], [], ),
(        True, _second('M100 M106 M112 '), [], [], ),
(       False, _second('M100 M106 M112 '), [], [], ),
(         100, _merge(_first('M100 M101 '), _second('M106 M108 M109 M110 M111 M112 ')), [], [], ),
(  myint(100), _second('M106 M108 M110 M111 M112 '), [], [], ),
(        -100, _merge(_first('M100 M101 '), _second('M106 M108 M109 M110 M111 M112 ')), [], [], ),
(   UInt32Min, _second('M105 M107 M108 M109 M110 M111 M112 '), [], [], ),
(        200L, _merge(_first('M100 M101 '), _second('M108 M112 ')), [], [], ),
(       -200L, _merge(_first('M100 M101 '), _second('M108 M112 ')), [], [], ),
(      Byte10, _merge(_first('M100 M101 M103 M106 M108 M109 M110 M111 M112 '), _second('M102 ')), [], [], ),
(    SBytem10, _merge(_first('M100 M101 M102 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), _second('M103 ')), [], [], ),
(     Int1610, _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(    Int16m20, _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(       12.34, _second('M111 M112 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_Int32():
    target = COverloads_Int32()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(        None, _second('M112 '), [], [], ),
(        True, _merge(_first('M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 '), _second('M100 M112 ')), [], [], ),
(       False, _merge(_first('M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 '), _second('M100 M112 ')), [], [], ),
(         100, _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(  myint(100), _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(        -100, _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(   UInt32Min, _second('M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
(        200L, _merge(_first('M100 M101 '), _second('M108 M112 ')), [], [], ),
(       -200L, _merge(_first('M100 M101 '), _second('M108 M112 ')), [], [], ),
(      Byte10, _merge(_first('M100 M101 M103 M108 M109 M110 M111 M112 '), _second('M102 M104 M105 ')), [], [], ),
(    SBytem10, _merge(_first('M100 M101 M102 M104 M106 M107 M108 M109 M110 M111 M112 '), _second('M103 M105 ')), [], [], ),
(     Int1610, _merge(_first('M100 M101 M102 M103 M104 M106 M107 M108 M109 M110 M111 M112 '), _second('M105 ')), [], [], ),
(    Int16m20, _merge(_first('M100 M101 M102 M103 M104 M106 M107 M108 M109 M110 M111 M112 '), _second('M105 ')), [], [], ),
(       12.34, _merge(_first('M100 M101 '), _second('M111 M112 ')), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

def test_arg_Double():
    target = COverloads_Double()
    for (arg, mapping, funcOverflowError, funcValueError) in [
(        None, _second('M112 '), [], [], ),
(        True, _second('M100 M107 M112 '), [], [], ),
(       False, _second('M100 M107 M112 '), [], [], ),
(         100, _merge(_first('M100 M101 M102 M103 M104 M105 M106 M108 M112 '), _second('M107 M109 M111 ')), [], [], ),
(  myint(100), _merge(_first('M100 M101 M102 M103 M104 M105 M106 M108 M112 '), _second('M107 M109 M111 ')), [], [], ),
(        -100, _merge(_first('M100 M101 M102 M103 M104 M105 M106 M108 M112 '), _second('M107 M109 M111 ')), [], [], ),
(   UInt32Min, _merge(_first('M100 M101 M102 M103 M104 M105 M107 M112 '), _second('M106 M108 M109 M111 ')), [], [], ),
(        200L, _merge(_first('M100 M101 '), _second('M109 M112 ')), [], [], ),
(       -200L, _merge(_first('M100 M101 '), _second('M109 M112 ')), [], [], ),
(      Byte10, _merge(_first('M100 M101 M103 M112 '), _second('M102 M104 M105 M106 M107 M108 M109 M111 ')), [], [], ),
(    SBytem10, _merge(_first('M100 M101 M102 M104 M106 M108 M112 '), _second('M103 M105 M107 M109 M111 ')), [], [], ),
(     Int1610, _merge(_first('M100 M101 M102 M103 M104 M106 M108 M112 '), _second('M105 M107 M109 M111 ')), [], [], ),
(    Int16m20, _merge(_first('M100 M101 M102 M103 M104 M106 M108 M112 '), _second('M105 M107 M109 M111 ')), [], [], ),
(       12.34, _first('M100 M101 M102 M103 M104 M105 M106 M107 M108 M109 M110 M111 M112 '), [], [], ),
    ]:
        _try_arg(target, arg, mapping, funcOverflowError, funcValueError)

run_test(__name__)
