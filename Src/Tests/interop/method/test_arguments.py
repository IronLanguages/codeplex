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
    
import sys, nt

def environ_var(key): return [nt.environ[x] for x in nt.environ.keys() if x.lower() == key.lower()][0]

merlin_root = environ_var("MERLIN_ROOT")
sys.path.insert(0, merlin_root + r"\Languages\IronPython\Tests")
sys.path.insert(0, merlin_root + r"\Test\ClrAssembly\bin")

from lib.assert_util import *
skiptest("silverlight")

if is_cli:
    import clr
    clr.AddReference("methodargs", "typesamples")

    #from lib.file_util import *
    #peverify_dependency = [merlin_root + r"\Test\ClrAssembly\bin\methodargs.dll", merlin_root + r"\Test\ClrAssembly\bin\fieldtests.dll"]
    #copy_dlls_for_peverify(peverify_dependency)

    from Merlin.Testing import *
    from Merlin.Testing.Call import *
    from Merlin.Testing.TypeSample import *
    o = VariousParameters()
    flag = o.Flag
    
else:
    def M100(): pass
    def M200(arg): pass
    def M201(arg=20): pass
    def M202(*arg): pass
    
    def M300(x, y): pass
    def M350(x, *y): pass

def test_0_1_args():

    # public void M100() { }
    f = is_cli and o.M100 or M100
    f()
    AssertErrorWithMessages(TypeError, "M100() takes exactly 0 arguments (1 given)", 'M100() takes no arguments (1 given)', lambda: f(1))
    f(*())
    AssertErrorWithMessages(TypeError, "M100() takes exactly 0 arguments (2 given)", 'M100() takes no arguments (2 given)', lambda: f(*(1,2)))
    AssertErrorWithMessages(TypeError, "M100() got an unexpected keyword argument 'x'", 'M100() takes no arguments (1 given)', lambda: f(x = 10))
    AssertErrorWithMessages(TypeError, "M100() got an unexpected keyword argument 'x'", 'M100() takes no arguments (2 given)',lambda: f(x = 10, y = 20))
    f(**{})
    AssertErrorWithMessages(TypeError, "M100() got an unexpected keyword argument 'x'", 'M100() takes no arguments (1 given)', lambda: f(**{'x':10}))
    f(*(), **{})
    
    #public void M200(int arg) { }
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
    AssertErrorWithMessages(TypeError, "M200() takes exactly 1 non-keyword argument (2 given)", "M200() got multiple values for keyword argument 'arg'", lambda: f(1, arg = 1))
    AssertErrorWithMessages(TypeError, "M200() takes exactly 1 non-keyword argument (2 given)", "M200() got multiple values for keyword argument 'arg'", lambda: f(arg = 1, *(1,)))
    AssertErrorWithMessage(TypeError, "M200() got an unexpected keyword argument 'other'", lambda: f(other = 1))
    AssertErrorWithMessage(TypeError, "M200() got an unexpected keyword argument 'other'", lambda: f(1, other = 1))
    AssertErrorWithMessage(TypeError, "M200() got an unexpected keyword argument 'other'", lambda: f(other = 1, arg = 2))
    AssertErrorWithMessage(TypeError, "M200() got an unexpected keyword argument 'other'", lambda: f(arg = 1, other = 2))
    AssertErrorWithMessages(TypeError, "M200() takes exactly 1 non-keyword argument (1 given)", "M200() got multiple values for keyword argument 'arg'", lambda: f(arg = 1, **{'arg' : 2})) # msg

    #public void M201([DefaultParameterValue(20)] int arg) { }
    f = is_cli and o.M201 or M201
    f()
    f(1)
    AssertErrorWithMessages(TypeError, "M201() takes at most 0 arguments (2 given)", 'M201() takes at most 1 argument (2 given)', lambda: f(1, 2))# msg
    f(*())
    f(1, *())
    f(*(1,))
    AssertErrorWithMessages(TypeError, "M201() takes at most 0 arguments (3 given)", 'M201() takes at most 1 argument (3 given)', lambda: f(1, *(2, 3)))# msg
    AssertErrorWithMessages(TypeError, "M201() takes at most 0 arguments (2 given)", 'M201() takes at most 1 argument (2 given)', lambda: f(*(1, 2)))# msg
    f(arg = 1)
    f(arg = 1, *())
    f(arg = 1, **{})
    f(**{"arg" : 1})
    f(*(), **{"arg" : 1})
    AssertErrorWithMessages(TypeError, "M201() takes at most 0 non-keyword arguments (2 given)", "M201() got multiple values for keyword argument 'arg'", lambda: f(1, arg = 1))# msg
    AssertErrorWithMessages(TypeError, "M201() takes at most 0 non-keyword arguments (2 given)", "M201() got multiple values for keyword argument 'arg'", lambda: f(arg = 1, *(1,)))# msg
    AssertErrorWithMessage(TypeError, "M201() got an unexpected keyword argument 'other'", lambda: f(other = 1))
    AssertErrorWithMessage(TypeError, "M201() got an unexpected keyword argument 'other'", lambda: f(1, other = 1))
    AssertErrorWithMessage(TypeError, "M201() got an unexpected keyword argument 'other'", lambda: f(**{ "other" : 1, "arg" : 2}))
    AssertErrorWithMessage(TypeError, "M201() got an unexpected keyword argument 'other'", lambda: f(arg = 1, other = 2))
    AssertErrorWithMessage(TypeError, "M201() got an unexpected keyword argument 'arg1'", lambda: f(arg1 = 1, other = 2))
    AssertErrorWithMessage(TypeError, "M201() got an unexpected keyword argument 'arg1'", lambda: f(**{ "arg1" : 1}))

    #public void M202(params int[] arg) { }
    f = is_cli and o.M202 or M202
    f()
    f(1)
    f(1,2)
    f(*())
    f(1, *(), **{})
    f(1, *(2, 3))
    f(*(1, 2, 3, 4))
    AssertErrorWithMessages(TypeError, "M202() takes at most 0 non-keyword arguments (1 given)", "M202() got an unexpected keyword argument 'arg'", lambda: f(arg = 1))# msg
    AssertErrorWithMessages(TypeError, "M202() takes at most 0 non-keyword arguments (2 given)", "M202() got an unexpected keyword argument 'arg'", lambda: f(1, arg = 2))# msg
    AssertErrorWithMessages(TypeError, "M202() takes at most 0 arguments (0 given)", "M202() got an unexpected keyword argument 'arg'", lambda: f(**{'arg': 3}))# msg
    AssertErrorWithMessage(TypeError, "M202() got an unexpected keyword argument 'other'", lambda: f(**{'other': 4}))

@skip("win32")    
def test_optional():
    #public void M231([Optional] int arg) { Flag.Reset(); Flag.Value1 = arg; }
    #public void M232([Optional] bool arg) { Flag.Reset(); Flag.Value2 = arg; }
    #public void M233([Optional] object arg) { Flag.Reset(); Flag.Value4 = arg; }
    #public void M234([Optional] string arg) { Flag.Reset(); Flag.Value3 = arg; }
    #public void M235([Optional] EnumInt32 arg) { Flag.Reset(); Flag.Value5 = arg; }
    #public void M236([Optional] SimpleClass arg) { Flag.Reset(); Flag.Value6 = arg; }
    #public void M237([Optional] SimpleStruct arg) { Flag.Reset(); Flag.Value7 = arg; }
    
    ## testing the passed in value, and the default values
    o.M231(12); AreEqual(flag.Value1, 12)
    o.M231(); AreEqual(flag.Value1, 0)
    
    o.M232(True); AreEqual(flag.Value2, True)
    o.M232(); AreEqual(flag.Value2, False)
    
    def t(): pass
    o.M233(t); AreEqual(flag.Value4, t)
    o.M233(); AreEqual(flag.Value4, System.Type.Missing.Value)
    
    o.M234("ironpython"); AreEqual(flag.Value3, "ironpython")
    o.M234(); AreEqual(flag.Value3, None)
    
    o.M235(EnumInt32.B); AreEqual(flag.Value5, EnumInt32.B)
    o.M235(); AreEqual(flag.Value5, EnumInt32.A)
    
    x = SimpleClass(23)
    o.M236(x); AreEqual(flag.Value6, x)
    o.M236(); AreEqual(flag.Value6, None)
    
    x = SimpleStruct(24)
    o.M237(x); AreEqual(flag.Value7, x)
    o.M237(); AreEqual(flag.Value7.Flag, 0) 
    
    ## testing the argument style
    f = is_cli and o.M231 or M231
    
    f(*()); AreEqual(flag.Value1, 0)
    f(*(2, )); AreEqual(flag.Value1, 2)
    f(arg = 3); AreEqual(flag.Value1, 3)
    f(**{}); AreEqual(flag.Value1, 0)
    f(*(), **{'arg':4}); AreEqual(flag.Value1, 4)
    
    AssertErrorWithMessage(TypeError, "M231() takes at most 0 arguments (2 given)", lambda: f(1, 2))  # msg
    AssertErrorWithMessage(TypeError, "M231() takes at most 0 arguments (1 given)", lambda: f(1, **{'arg': 2}))  # msg
    AssertErrorWithMessage(TypeError, "M231() takes at most 0 non-keyword arguments (1 given)", lambda: f(arg = 3, **{'arg': 4}))  # msg
    AssertErrorWithMessage(TypeError, "M231() got an unexpected keyword argument 'other'", lambda: f(arg = 3, **{'other': 4}))  # msg
    
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
    
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 non-keyword arguments (1 given)", 'M300() takes exactly 2 non-keyword arguments (0 given)', lambda: f(y = 1))
    f(y = 2, x = 1)
    AssertErrorWithMessage(TypeError, "M300() got an unexpected keyword argument 'x2'", lambda: f(y = 1, x2 = 2))
    AssertErrorWithMessage(TypeError, "M300() got an unexpected keyword argument 'z'", lambda: f(x = 1, y = 1, z = 3))
    #AssertError(SyntaxError, eval, "f(x=1, y=2, y=3)")
    
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (0 given)", 'M300() takes exactly 2 non-keyword arguments (1 given)', lambda: f(**{"x":1}))  # msg
    f(**{"x":1, "y":2})
    
    # ...
    
    # mixed
    # positional/keyword
    f(1, y = 2)
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 non-keyword arguments (2 given)", "M300() got multiple values for keyword argument 'x'", lambda: f(2, x = 1))    # msg    
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 non-keyword arguments (3 given)", "M300() got multiple values for keyword argument 'x'", lambda: f(1, y = 1, x = 2)) # msg
    
    # positional / **
    f(1, **{'y': 2})
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (1 given)", "M300() got multiple values for keyword argument 'x'", lambda: f(2, ** {'x':1}))
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 arguments (1 given)", "M300() got multiple values for keyword argument 'x'", lambda: f(1, ** {'y':1, 'x':2})) 
    
    # keyword / *
    f(y = 2, *(1,))
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 non-keyword arguments (3 given)", "M300() got multiple values for keyword argument 'y'", lambda: f(y = 2, *(1,2)))
    AssertErrorWithMessages(TypeError, "M300() takes exactly 2 non-keyword arguments (3 given)", "M300() got multiple values for keyword argument 'x'", lambda: f(y = 2, x = 1, *(3,)))
    
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
    AssertErrorWithMessages(TypeError, "M350() takes at most 1 non-keyword argument (2 given)", "M350() got an unexpected keyword argument 'y'", lambda: f(x = 1, y = 2))
    
    f(**{'x' : 1})
    AssertErrorWithMessages(TypeError, "M350() takes at least 1 argument (0 given)", "M350() got an unexpected keyword argument 'y'", lambda: f(**{'x' : 1, 'y' : 2}))
    AssertErrorWithMessages(TypeError, "M350() takes at most 1 non-keyword argument (4 given)", "M350() got multiple values for keyword argument 'x'", lambda: f(2, 3, 4, x = 1))
    

run_test(__name__)

#delete_dlls_for_peverify(peverify_dependency)
