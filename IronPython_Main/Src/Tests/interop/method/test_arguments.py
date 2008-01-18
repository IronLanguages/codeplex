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
skiptest("silverlight")

if is_cli:
    add_clr_assemblies("methodargs", "typesamples")

    from Merlin.Testing import *
    from Merlin.Testing.Call import *
    from Merlin.Testing.TypeSample import *
    o = VariousParameters()
    
else:
    def M100(): pass
    def M200(arg): pass
    def M201(arg=20): pass
    def M202(*arg): pass
    
    def M300(x, y): pass
    def M350(x, *y): pass

def test_0_1_args():

    # public void M100() { Flag.Reset(); Flag.Set(10); }
    f = is_cli and o.M100 or M100
    f()
    AssertErrorWithMessage(TypeError, 'M100() takes no arguments (1 given)', lambda: f(1))
    f(*())
    AssertErrorWithMessage(TypeError, 'M100() takes no arguments (2 given)', lambda: f(*(1,2)))
    AssertErrorWithMessage(TypeError, 'M100() takes no arguments (1 given)', lambda: f(x = 10))
    AssertErrorWithMessage(TypeError, 'M100() takes no arguments (2 given)',lambda: f(x = 10, y = 20))
    f(**{})
    AssertErrorWithMessage(TypeError, 'M100() takes no arguments (1 given)', lambda: f(**{'x':10}))
    f(*(), **{})
    
    # public void M200(int arg) { Flag.Reset(); Flag.Set(arg); }
    f = is_cli and o.M200 or M200
    AssertErrorWithMessage(TypeError, "M200() takes exactly 1 argument (0 given)", lambda: f())
    f(1)
    AssertErrorWithMessage(TypeError, "M200() takes exactly 1 argument (2 given)", lambda: f(1, 2))
    f(*(1,))
    f(1, *())
    AssertErrorWithMessage(TypeError, "M200() takes exactly 1 argument (2 given)", lambda: f(1, *(2,)))
    f(arg = 1); AssertError(NameError, lambda: arg)
    f(arg = 1, *())
    f(arg = 1, **{})
    f(**{"arg" : 1})
    f(*(), **{"arg" : 1})
    AssertErrorWithMessages(TypeError, "M200() takes exactly 1 argument (2 given)", "M200() got multiple values for keyword argument 'arg'", lambda: f(1, arg = 1))
    AssertErrorWithMessages(TypeError, "M200() takes exactly 1 argument (2 given)", "M200() got multiple values for keyword argument 'arg'", lambda: f(arg = 1, *(1,)))
    AssertErrorWithMessage(TypeError, "M200() got an unexpected keyword argument 'other'", lambda: f(other = 1))
    AssertErrorWithMessages(TypeError, "M200() takes exactly 1 argument (2 given)", "M200() got an unexpected keyword argument 'other'", lambda: f(1, other = 1))
    AssertErrorWithMessages(TypeError, "M200() takes exactly 1 argument (2 given)", "M200() got an unexpected keyword argument 'other'", lambda: f(other = 1, arg = 2))
    AssertErrorWithMessages(TypeError, "M200() takes exactly 1 argument (2 given)", "M200() got an unexpected keyword argument 'other'", lambda: f(arg = 1, other = 2))
    AssertErrorWithMessages(TypeError, "M200() takes exactly 1 argument (2 given)", "M200() got multiple values for keyword argument 'arg'", lambda: f(arg = 1, **{'arg' : 2})) # msg

    # public void M201([DefaultParameterValue(20)] int arg) { Flag.Reset(); Flag.Set(arg); }
    f = is_cli and o.M201 or M201
    f()
    f(1)
    AssertErrorWithMessage(TypeError, 'M201() takes at most 1 argument (2 given)', lambda: f(1, 2))# msg
    f(*())
    f(1, *())
    f(*(1,))
    AssertErrorWithMessage(TypeError, 'M201() takes at most 1 argument (3 given)', lambda: f(1, *(2, 3)))# msg
    AssertErrorWithMessage(TypeError, 'M201() takes at most 1 argument (2 given)', lambda: f(*(1, 2)))# msg
    f(arg = 1)
    f(arg = 1, *())
    f(arg = 1, **{})
    f(**{"arg" : 1})
    f(*(), **{"arg" : 1})
    AssertErrorWithMessages(TypeError, "M201() takes at most 1 argument (2 given)", "M201() got multiple values for keyword argument 'arg'", lambda: f(1, arg = 1))# msg
    AssertErrorWithMessages(TypeError, "M201() takes at most 1 argument (2 given)", "M201() got multiple values for keyword argument 'arg'", lambda: f(arg = 1, *(1,)))# msg
    AssertErrorWithMessage(TypeError, "M201() got an unexpected keyword argument 'other'", lambda: f(other = 1))
    AssertErrorWithMessages(TypeError, "M201() takes at most 1 argument (2 given)", "M201() got an unexpected keyword argument 'other'", lambda: f(1, other = 1))
    AssertErrorWithMessages(TypeError, "M201() takes at most 1 argument (2 given)", "M201() got an unexpected keyword argument 'other'", lambda: f(**{ "other" : 1, "arg" : 2}))
    AssertErrorWithMessages(TypeError, "M201() takes at most 1 argument (2 given)", "M201() got an unexpected keyword argument 'other'", lambda: f(arg = 1, other = 2))
    AssertErrorWithMessages(TypeError, "M201() takes at most 1 argument (2 given)", "M201() got an unexpected keyword argument 'arg1'", lambda: f(arg1 = 1, other = 2))
    AssertErrorWithMessage(TypeError, "M201() got an unexpected keyword argument 'arg1'", lambda: f(**{ "arg1" : 1}))

    # public void M202(params int[] arg) { Flag.Reset(); Flag.Set(arg.Length); }
    f = is_cli and o.M202 or M202
    f()
    f(1)
    f(1,2)
    f(*())
    f(1, *(), **{})
    f(1, *(2, 3))
    f(*(1, 2, 3, 4))
    AssertErrorWithMessage(TypeError, "M202() got an unexpected keyword argument 'arg'", lambda: f(arg = 1))# msg
    AssertErrorWithMessages(TypeError, "M202() takes at least 1 argument (2 given)", "M202() got an unexpected keyword argument 'arg'", lambda: f(1, arg = 2))# msg
    AssertErrorWithMessage(TypeError, "M202() got an unexpected keyword argument 'arg'", lambda: f(**{'arg': 3}))# msg
    AssertErrorWithMessage(TypeError, "M202() got an unexpected keyword argument 'other'", lambda: f(**{'other': 4}))

@skip("win32")    
def test_optional():
    #public void M231([Optional] int arg) { Flag.Set(arg); }  // not reset any
    #public void M232([Optional] bool arg) { Flag<bool>.Set(arg); }
    #public void M233([Optional] object arg) { Flag<object>.Set(arg); }
    #public void M234([Optional] string arg) { Flag<string>.Set(arg); }
    #public void M235([Optional] EnumInt32 arg) { Flag<EnumInt32>.Set(arg); }
    #public void M236([Optional] SimpleClass arg) { Flag<SimpleClass>.Set(arg); }
    #public void M237([Optional] SimpleStruct arg) { Flag<SimpleStruct>.Set(arg); }

    ## testing the passed in value, and the default values
    o.M231(12); Flag.Check(12)
    o.M231(); Flag.Check(0)
    
    o.M232(True); Flag[bool].Check(True) 
    o.M232(); Flag[bool].Check(False)
        
    def t(): pass
    o.M233(t); Flag[object].Check(t)
    o.M233(); Flag[object].Check(System.Type.Missing.Value)
    
    o.M234("ironpython"); Flag[str].Check("ironpython")
    o.M234(); Flag[str].Check(None)
    
    o.M235(EnumInt32.B); Flag[EnumInt32].Check(EnumInt32.B)
    o.M235(); Flag[EnumInt32].Check(EnumInt32.A)
    
    x = SimpleClass(23)
    o.M236(x); Flag[SimpleClass].Check(x)
    o.M236(); Flag[SimpleClass].Check(None)
    
    x = SimpleStruct(24)
    o.M237(x); Flag[SimpleStruct].Check(x)
    o.M237(); AreEqual(Flag[SimpleStruct].Value1.Flag, 0) 
    
    ## testing the argument style
    f = is_cli and o.M231 or M231
    
    f(*()); Flag.Check(0)
    f(*(2, )); Flag.Check(2)
    f(arg = 3); Flag.Check(3)
    f(**{}); Flag.Check(0)
    f(*(), **{'arg':4}); Flag.Check(4)
    
    AssertErrorWithMessage(TypeError, "M231() takes at most 1 argument (2 given)", lambda: f(1, 2))  # msg
    AssertErrorWithMessage(TypeError, "M231() takes at most 1 argument (2 given)", lambda: f(1, **{'arg': 2}))  # msg
    AssertErrorWithMessage(TypeError, "M231() takes at most 1 argument (2 given)", lambda: f(arg = 3, **{'arg': 4}))  # msg
    AssertErrorWithMessage(TypeError, "M231() takes at most 1 argument (2 given)", lambda: f(arg = 3, **{'other': 4}))  # msg
    
def test_two_args():
    #public void M300(int x, int y) { }
    f = is_cli and o.M300 or M300
    AssertErrorWithMessage(TypeError, "M300() takes exactly 2 arguments (0 given)", lambda: f())
    AssertErrorWithMessage(TypeError, "M300() takes exactly 2 arguments (1 given)", lambda: f(1))
    f(1, 2)
    AssertErrorWithMessage(TypeError, "M300() takes exactly 2 arguments (3 given)", lambda: f(1, 2, 3))
    
    AssertErrorWithMessage(TypeError, "M300() takes exactly 2 arguments (0 given)", lambda: f(*()))
    AssertErrorWithMessage(TypeError, "M300() takes exactly 2 arguments (1 given)", lambda: f(*(1,)))
    f(1, *(2,))
    f(*(3, 4))
    AssertErrorWithMessage(TypeError, "M300() takes exactly 2 arguments (3 given)", lambda: f(1, *(2, 3)))
    
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (1 given)", 'M300() takes exactly 2 non-keyword arguments (0 given)', lambda: f(y = 1))
    f(y = 2, x = 1)
    AssertErrorWithMessage(TypeError, "M300() got an unexpected keyword argument 'x2'", lambda: f(y = 1, x2 = 2))
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (3 given)", "M300() got an unexpected keyword argument 'z'", lambda: f(x = 1, y = 1, z = 3))
    #AssertError(SyntaxError, eval, "f(x=1, y=2, y=3)")
    
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (1 given)", 'M300() takes exactly 2 non-keyword arguments (1 given)', lambda: f(**{"x":1}))  # msg
    f(**{"x":1, "y":2})
    
    # ...
    
    # mixed
    # positional/keyword
    f(1, y = 2)
    AssertErrorWithMessage(TypeError, "M300() got multiple values for keyword argument 'x'", lambda: f(2, x = 1))    # msg    
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (3 given)", "M300() got multiple values for keyword argument 'x'", lambda: f(1, y = 1, x = 2)) # msg
    
    # positional / **
    f(1, **{'y': 2})
    AssertErrorWithMessage(TypeError, "M300() got multiple values for keyword argument 'x'", lambda: f(2, ** {'x':1}))
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (3 given)", "M300() got multiple values for keyword argument 'x'", lambda: f(1, ** {'y':1, 'x':2})) 
    
    # keyword / *
    f(y = 2, *(1,))
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (3 given)", "M300() got multiple values for keyword argument 'y'", lambda: f(y = 2, *(1,2)))
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (3 given)", "M300() got multiple values for keyword argument 'x'", lambda: f(y = 2, x = 1, *(3,)))
    
    # keyword / **
    f(y = 2, **{'x' : 1})
    
    #public void M350(int x, params int[] y) { }
    
    f = is_cli and o.M350 or M350
    AssertErrorWithMessage(TypeError, "M350() takes at least 1 argument (0 given)", lambda: f())
    f(1)
    f(1, 2)
    f(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
    
    AssertErrorWithMessage(TypeError, "M350() takes at least 1 argument (0 given)", lambda: f(*()))
    f(*(1,))
    f(1, 2, *(3, 4))
    f(1, 2, *())
    f(1, 2, 3, *(4, 5, 6, 7, 8, 9, 10))
    
    f(x = 1)
    AssertErrorWithMessage(TypeError, "M350() got an unexpected keyword argument 'y'", lambda: f(x = 1, y = 2))
    
    f(**{'x' : 1})
    AssertErrorWithMessage(TypeError, "M350() got an unexpected keyword argument 'y'", lambda: f(**{'x' : 1, 'y' : 2}))
    AssertErrorWithMessage(TypeError, "M350() got multiple values for keyword argument 'x'", lambda: f(2, 3, 4, x = 1))
    
    # TODO: mixed 
    f(x = 1)  # check the value

def test_default_values_2():
    # public void M310(int x, [DefaultParameterValue(30)]int y) { Flag.Reset(); Flag.Set(x + y); }
    f = o.M310
    AssertErrorWithMessage(TypeError, "M310() takes at least 1 argument (0 given)", f) 
    f(1); Flag.Check(31)
    f(1, 2); Flag.Check(3)
    AssertErrorWithMessage(TypeError, "M310() takes at most 2 arguments (3 given)", lambda : f(1, 2, 3))
    
    f(x = 2); Flag.Check(32)
    f(4, y = 5); Flag.Check(9)
    f(y = 7, x = 10); Flag.Check(17)
    f(*(8,)); Flag.Check(38)
    f(*(9, 10)); Flag.Check(19)
    
    f(1, **{'y':2}); Flag.Check(3)
    
    # public void M320([DefaultParameterValue(40)] int y, int x) { Flag.Reset(); Flag.Set(x + y); }
    f = o.M320
    AssertErrorWithMessage(TypeError, "M320() takes at least 1 argument (0 given)", f) 
    f(1); Flag.Check(41)  # !!!
    f(2, 3); Flag.Check(5)
    AssertErrorWithMessage(TypeError, "M320() takes at most 2 arguments (3 given)", lambda : f(1, 2, 3))
    
    f(x = 2); Flag.Check(42)
    f(x = 2, y = 3); Flag.Check(5)
    f(*(1,)); Flag.Check(41)
    f(*(1, 2)); Flag.Check(3)
    
    AssertErrorWithMessage(TypeError, "M320() got multiple values for keyword argument 'y'", lambda : f(5, y = 6)) # !!!
    f(6, x = 7); Flag.Check(13)
    
    # public void M330([DefaultParameterValue(50)] int x, [DefaultParameterValue(60)] int y) { Flag.Reset(); Flag.Set(x + y); }
    f = o.M330
    f(); Flag.Check(110)
    f(1); Flag.Check(61)
    f(1, 2); Flag.Check(3)
    
    f(x = 1); Flag.Check(61)
    f(y = 2); Flag.Check(52)
    f(y = 3, x = 4); Flag.Check(7)
    
    f(*(5,)); Flag.Check(65)
    f(**{'y' : 6}); Flag.Check(56)

def test_3_args():
    # public void M500(int x, int y, int z) { Flag.Reset(); Flag.Set(x * 100 + y * 10 + z); }
    f = o.M500
    f(1, 2, 3); Flag.Check(123)
    f(y = 1, z = 2, x = 3); Flag.Check(312)
    f(3, *(2, 1)); Flag.Check(321)
    f(1, z = 2, **{'y':3}); Flag.Check(132)
    f(z = 1, **{'x':2, 'y':3}); Flag.Check(231)
    f(1, z = 2, *(3,)); #Flag.Check(132)

    # public void M510(int x, int y, [DefaultParameterValue(70)] int z) { Flag.Reset(); Flag.Set(x * 100 + y * 10 + z); }
    f = o.M510
    f(1, 2); Flag.Check(120 + 70)
    f(2, y = 1); Flag.Check(210 + 70)

    f(1, 2, 3); Flag.Check(123)
    
    # public void M520(int x, [DefaultParameterValue(80)]int y, int z) { Flag.Reset(); Flag.Set(x * 100 + y * 10 + z); }
    f = o.M520
    f(1, 2); Flag.Check(102 + 800)
    f(2, z = 1); Flag.Check(201 + 800)
    f(z=1, **{'x': 2}); Flag.Check(201 + 800)
    f(2, *(1,)); Flag.Check(201 + 800)
    
    f(1, z = 2, y = 3); Flag.Check(132)
    f(1, 2, 3); Flag.Check(123)
    
    # public void M530([DefaultParameterValue(90)]int x, int y, int z) { Flag.Reset(); Flag.Set(x * 100 + y * 10 + z); }
    f = o.M530
    f(1, 2); Flag.Check(12 + 9000)
    f(3, z = 4); Flag.Check(34 + 9000)
    f(*(5,), **{'z':6}); Flag.Check(56 + 9000)
    AssertErrorWithMessage(TypeError, "M530() got multiple values for keyword argument 'y'", lambda: f(2, y = 2)) # msg
    
    f(1, 2, 3); Flag.Check(123)
    
    # public void M550(int x, int y, params int[] z) { Flag.Reset(); Flag.Set(x * 100 + y * 10 + z.Length); }
    f = o.M550
    f(1, 2); Flag.Check(120)
    f(1, 2, 3); Flag.Check(121)
    f(1, 2, 3, 4, 5, 6, 7, 8, 9, 10); Flag.Check(128)
    
    f(1, 2, *()); Flag.Check(120)
    f(1, 2, *(3,)); Flag.Check(121)
    
    # bug 311155
    ##def  f(x, y, *z): print x, y, z
    #f(1, y = 2); Flag.Check(120)
    #f(x = 2, y = 3); Flag.Check(230)
    #f(1, y = 2, *()); Flag.Check(120)

    #f(1, y = 2, *(3, )); Flag.Check(121)
    #f(y = 2, x = 3; *(3, 4)); Flag.Check(322)
    #f(1, *(2, 3), **{'y': 4}); Flag.Check(142)
    #f(*(4, 5, 6), **{'y':7}); Flag.Check(472)
    #f(*(1, 2, 0, 1), **{'y':3, 'x':4}); Flag.Check(434)

def test_many_args():
    #public void M650(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10) { }
    f = o.M650
    expect = "1 2 3 4 5 6 7 8 9 10"
    
    f(1, 2, 3, 4, 5, 6, 7, 8, 9, 10); Flag[str].Check(expect)
    
    #def f(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10): print arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10
    f(arg2 = 2, arg3 = 3, arg4 = 4, arg5 = 5, arg6 = 6, arg7 = 7, arg8 = 8, arg9 = 9, arg10 = 10, arg1 = 1); Flag[str].Check(expect) 
    f(1, 2, arg6 = 6, arg7 = 7, arg8 = 8, arg3 = 3, arg4 = 4, arg5 = 5, arg9 = 9, arg10 = 10); Flag[str].Check(expect) 

    #AssertErrorWithMessage(TypeError, "M650() got multiple values for keyword argument 'arg5'", lambda: f(1, 2, 3, arg5 = 5, *(4, 6, 7, 8, 9, 10))) 
    #AssertErrorWithMessage(TypeError, "M650() got multiple values for keyword argument 'arg1'", lambda: f(arg3 = 3, arg2 = 2, arg1 = 1, *(4, 5, 6, 7, 9, 10), **{'arg8': 8})) 
    #AssertErrorWithMessage(TypeError, "M650() got multiple values for keyword argument 'arg3'", lambda: f(1, 2, 4, 5, 6, 7, 8, 9, 10, **{'arg3' : 3})) 
    
    f(1, 2, 3, arg9 = 9, arg10 = 10, *(4, 5, 6, 7, 8)); # Flag[str].Check(expect)  # bug 311195
    f(1, 2, 3, arg10 = 10, *(4, 5, 6, 7, 8), ** {'arg9': 9}); # Flag[str].Check(expect) # bug 311195
    
    AssertErrorWithMessage(TypeError, "M650() got multiple values for keyword argument 'arg5'", lambda: f(2, 3, arg5 = 5, arg10 = 10, *(4, 6, 7, 9), **{'arg8': 8, 'arg1': 1})) # msg (should be 6 given)
    
    #public void M700(int arg1, string arg2, bool arg3, object arg4, EnumInt16 arg5, SimpleClass arg6, SimpleStruct arg7) { }
    
    f = o.M700

def test_special_name():
    #// keyword argument name, or **dict style
    #public void M800(int True) { }
    #public void M801(int def) { }
    
    f = o.M800
    f(True=9); Flag.Check(9)
    AreEqual(str(True), "True")
    
    f(**{"True": 19}); Flag.Check(19)

    f = o.M801
    AssertError(SyntaxError, eval, "f(def = 3)")
    f(**{"def": 8}); Flag.Check(8)
    
run_test(__name__)

