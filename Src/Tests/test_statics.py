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
# test static members
#

from lib.assert_util import *

load_iron_python_test()
from IronPythonTest.StaticTest import *

allTypes = [Base, OverrideNothing, OverrideAll]

def test_field():
    # read on class
    AreEqual(Base.Field, 'Base.Field')
    AreEqual(OverrideNothing.Field, 'Base.Field')
    AreEqual(OverrideAll.Field, 'OverrideAll.Field')
    
    # write and read back
    Base.Field = 'FirstString'    
    AreEqual(Base.Field, 'FirstString')
    AreEqual(OverrideNothing.Field, 'FirstString')
    AreEqual(OverrideAll.Field, 'OverrideAll.Field')
    
    def f(): OverrideNothing.Field = 'SecondString'
    AssertErrorWithMessage(AttributeError, "attribute 'Field' of 'OverrideNothing' object is read-only", f)
   
    AreEqual(Base.Field, 'FirstString')
    AreEqual(OverrideNothing.Field, 'FirstString')
    AreEqual(OverrideAll.Field, 'OverrideAll.Field')
    
    OverrideAll.Field = 'ThirdString'
    AreEqual(Base.Field, 'FirstString')
    AreEqual(OverrideNothing.Field, 'FirstString')
    AreEqual(OverrideAll.Field, 'ThirdString')

    # reset back 
    Base.Field = 'Base.Field'
    OverrideAll.Field = 'OverrideAll.Field'

    # read / write on instance
    b, o1, o2 = Base(), OverrideNothing(), OverrideAll()
    
    AreEqual(b.Field, 'Base.Field')
    AreEqual(o1.Field, 'Base.Field')
    AreEqual(o2.Field, 'OverrideAll.Field')
    
    b.Field = 'FirstString'
    AreEqual(b.Field, 'FirstString')
    AreEqual(o1.Field, 'FirstString')
    AreEqual(o2.Field, 'OverrideAll.Field')
    
    def f(): o1.Field = 'SecondString'
    AssertErrorWithMessage(AttributeError, "'OverrideNothing' object has no attribute 'Field'", f)

    o2.Field = 'ThirdString'
    AreEqual(b.Field, 'FirstString')
    AreEqual(o1.Field, 'FirstString')
    AreEqual(o2.Field, 'ThirdString')
    
    # del 
    def f(target): del target.Field
    AssertErrorWithMessage(AttributeError, "cannot delete attribute 'Field' of builtin type 'Base'", f, Base)
    AssertErrorWithMessage(AttributeError, "No attribute Field.", f, OverrideNothing)
    AssertErrorWithMessage(AttributeError, "cannot delete attribute 'Field' of builtin type 'OverrideAll'", f, OverrideAll)
    
    AssertErrorWithMessage(AttributeError, "cannot delete attribute 'Field' of builtin type 'Base'", f, b)
    AssertErrorWithMessage(AttributeError, "'OverrideNothing' object has no attribute 'Field'", f, o1)
    AssertErrorWithMessage(AttributeError, "cannot delete attribute 'Field' of builtin type 'OverrideAll'", f, o2)
    
def test_property():
    # read on class
    AreEqual(Base.Property, 'Base.Property')
    AreEqual(OverrideNothing.Property, 'Base.Property')
    AreEqual(OverrideAll.Property, 'OverrideAll.Property')
    
    # write and read back
    Base.Property = 'FirstString'    
    AreEqual(Base.Property, 'FirstString')
    AreEqual(OverrideNothing.Property, 'FirstString')
    AreEqual(OverrideAll.Property, 'OverrideAll.Property')
   
    def f(): OverrideNothing.Property = 'SecondString'
    AssertErrorWithMessage(AttributeError, "attribute 'Property' of 'OverrideNothing' object is read-only", f)
 
    AreEqual(Base.Property, 'FirstString')
    AreEqual(OverrideNothing.Property, 'FirstString')
    AreEqual(OverrideAll.Property, 'OverrideAll.Property')
    
    OverrideAll.Property = 'ThirdString'
    AreEqual(Base.Property, 'FirstString')
    AreEqual(OverrideNothing.Property, 'FirstString')
    AreEqual(OverrideAll.Property, 'ThirdString')
    
    # reset back 
    Base.Property = 'Base.Property'
    OverrideAll.Property = 'OverrideAll.Property'

    # read / write on instance
    b, o1, o2 = Base(), OverrideNothing(), OverrideAll()

    def f_read(target): print target.Property
    for x in [b, o1, o2]:
        AssertErrorWithMessage(TypeError, "get_Property() takes exactly 0 arguments (0 given)", f_read, x)

    def f_write(target): target.Property = 'Anything'  
    for x in [b, o2]:
        AssertErrorWithMessage(TypeError, "set_Property() takes exactly 0 arguments (1 given)", f_write, x)      
    AssertErrorWithMessage(AttributeError, "'OverrideNothing' object has no attribute 'Property'", f_write, o1)                
      
    # del 
    def f(target): del target.Property
    AssertErrorWithMessage(AttributeError, "attribute 'Property' of 'Base' object is read-only", f, Base)
    AssertErrorWithMessage(AttributeError, "No attribute Property.", f, OverrideNothing)
    AssertErrorWithMessage(AttributeError, "attribute 'Property' of 'OverrideAll' object is read-only", f, OverrideAll)
    
    AssertErrorWithMessage(AttributeError, "attribute 'Property' of 'Base' object is read-only", f, b)
    AssertErrorWithMessage(AttributeError, "'OverrideNothing' object has no attribute 'Property'", f, o1)
    AssertErrorWithMessage(AttributeError, "attribute 'Property' of 'OverrideAll' object is read-only", f, o2)

def test_event():
    lambda1 = lambda : 'FirstString'
    lambda2 = lambda : 'SecondString'
    lambda3 = lambda : 'ThirdString'
    
    AreEqual(Base.TryEvent(), 'Still None')
    AreEqual(OverrideNothing.TryEvent(), 'Still None')
    AreEqual(OverrideAll.TryEvent(), 'Still None here')

    Base.Event += lambda1
    AreEqual(Base.TryEvent(), 'FirstString')
    AreEqual(OverrideNothing.TryEvent(), 'FirstString')
    AreEqual(OverrideAll.TryEvent(), 'Still None here')

    Base.Event -= lambda1
    AreEqual(Base.TryEvent(), 'Still None')
        
    def f(): OverrideNothing.Event += lambda2
    AssertErrorWithMessage(AttributeError, "attribute 'Event' of 'OverrideNothing' object is read-only", f)
    
    # ISSUE
    Base.Event -= lambda2
    
    AreEqual(Base.TryEvent(), 'Still None')
    AreEqual(OverrideNothing.TryEvent(), 'Still None')
    AreEqual(OverrideAll.TryEvent(), 'Still None here')
    
    OverrideAll.Event += lambda3
    AreEqual(Base.TryEvent(), 'Still None')
    AreEqual(OverrideNothing.TryEvent(), 'Still None')
    AreEqual(OverrideAll.TryEvent(), 'ThirdString')
    
    OverrideAll.Event -= lambda3
    AreEqual(OverrideAll.TryEvent(), 'Still None here')

    # Play on instance
    b, o1, o2 = Base(), OverrideNothing(), OverrideAll()
    
    b.Event += lambda1 
    AreEqual(Base.TryEvent(), 'FirstString')
    AreEqual(OverrideNothing.TryEvent(), 'FirstString')
    AreEqual(OverrideAll.TryEvent(), 'Still None here')
    b.Event -= lambda1
    
    def f(): o1.Event += lambda2
    AssertErrorWithMessage(AttributeError, "'OverrideNothing' object has no attribute 'Event'", f)
   
    # ISSUE
    try:    o1.Event -= lambda2
    except: pass
    
    AreEqual(Base.TryEvent(), 'Still None')
    AreEqual(OverrideNothing.TryEvent(), 'Still None')
    AreEqual(OverrideAll.TryEvent(), 'Still None here')
    
    o2.Event += lambda3
    AreEqual(Base.TryEvent(), 'Still None')
    AreEqual(OverrideNothing.TryEvent(), 'Still None')
    AreEqual(OverrideAll.TryEvent(), 'ThirdString')

    # del
    def f(target): del target.Event
    AssertErrorWithMessage(AttributeError, "attribute 'Event' of 'Base' object is read-only", f, Base)
    AssertErrorWithMessage(AttributeError, "No attribute Event.", f, OverrideNothing)
    AssertErrorWithMessage(AttributeError, "attribute 'Event' of 'OverrideAll' object is read-only", f, OverrideAll)
    
    AssertErrorWithMessage(AttributeError, "attribute 'Event' of 'Base' object is read-only", f, b)
    AssertErrorWithMessage(AttributeError, "'OverrideNothing' object has no attribute 'Event'", f, o1)
    AssertErrorWithMessage(AttributeError, "attribute 'Event' of 'OverrideAll' object is read-only", f, o2)

def test_method():
    AreEqual(Base.Method_None(), 'Base.Method_None')
    AreEqual(OverrideNothing.Method_None(), 'Base.Method_None')
    AreEqual(OverrideAll.Method_None(), 'OverrideAll.Method_None')
    
    for type in allTypes:
        AssertError(TypeError, type.Method_None, None)
        AssertError(TypeError, type.Method_None, 1)

    AreEqual(Base.Method_OneArg(1), 'Base.Method_OneArg')
    AreEqual(OverrideNothing.Method_OneArg(1), 'Base.Method_OneArg')
    AreEqual(OverrideAll.Method_OneArg(1), 'OverrideAll.Method_OneArg')
    
    for type in allTypes:
        AssertError(TypeError, type.Method_OneArg)
        AssertError(TypeError, type.Method_OneArg, None)

    #==============================================================
    
    b, d1, d2 = Base(), OverrideNothing(), OverrideAll()
    for x in [b, d1, d2]:
        AreEqual(Base.Method_Base(x), 'Base.Method_Base')
        AreEqual(OverrideNothing.Method_Base(x), 'Base.Method_Base')        
    
    AssertErrorWithMessage(TypeError, 'expected OverrideAll, got Base', OverrideAll.Method_Base, b)
    AssertErrorWithMessage(TypeError, 'expected OverrideAll, got OverrideNothing', OverrideAll.Method_Base, d1)
    AreEqual(OverrideAll.Method_Base(d2), 'OverrideAll.Method_Base')        

    #==============================================================

    b, d = B(), D()
    
    AreEqual(Base.Method_Inheritance1(b), 'Base.Method_Inheritance1')
    AreEqual(OverrideNothing.Method_Inheritance1(b), 'Base.Method_Inheritance1')
    AssertErrorWithMessage(TypeError, 'expected D, got B', OverrideAll.Method_Inheritance1, b)

    AreEqual(Base.Method_Inheritance1(d), 'Base.Method_Inheritance1')
    AreEqual(OverrideNothing.Method_Inheritance1(d), 'Base.Method_Inheritance1')
    AreEqual(OverrideAll.Method_Inheritance1(d), 'OverrideAll.Method_Inheritance1')

    AssertErrorWithMessage(TypeError, 'expected D, got B', Base.Method_Inheritance2, b)
    AssertErrorWithMessage(TypeError, 'expected D, got B', OverrideNothing.Method_Inheritance2, b)
    AreEqual(OverrideAll.Method_Inheritance2(b), 'OverrideAll.Method_Inheritance2')

    AreEqual(Base.Method_Inheritance2(d), 'Base.Method_Inheritance2')
    AreEqual(OverrideNothing.Method_Inheritance2(d), 'Base.Method_Inheritance2')
    AreEqual(OverrideAll.Method_Inheritance2(d), 'OverrideAll.Method_Inheritance2')

    # play with instance
    b, o1, o2 = Base(), OverrideNothing(), OverrideAll()
    AreEqual(b.Method_None(), 'Base.Method_None')
    AreEqual(o1.Method_None(), 'Base.Method_None')
    AreEqual(o2.Method_None(), 'OverrideAll.Method_None')
    
    AreEqual(b.Method_Base(b), 'Base.Method_Base')
    AreEqual(o1.Method_Base(b), 'Base.Method_Base')
    AssertErrorWithMessage(TypeError, 'expected OverrideAll, got Base', o2.Method_Base, b)

    AreEqual(b.Method_Base(o1), 'Base.Method_Base')
    AreEqual(o1.Method_Base(o1), 'Base.Method_Base')
    AssertErrorWithMessage(TypeError, 'expected OverrideAll, got OverrideNothing', o2.Method_Base, o1)
    
    AreEqual(b.Method_Base(o2), 'Base.Method_Base')
    AreEqual(o1.Method_Base(o2), 'Base.Method_Base')
    AreEqual(o2.Method_Base(o2), 'OverrideAll.Method_Base')
    
    # del 
    def f(target): del target.Method_None

    AssertErrorWithMessage(TypeError, "can't delete 'Method_None' from dictproxy", f, Base)
    AssertErrorWithMessage(AttributeError, "No attribute Method_None.", f, OverrideNothing)
    AssertErrorWithMessage(TypeError, "can't delete 'Method_None' from dictproxy", f, OverrideAll)
    
    AssertErrorWithMessage(AttributeError, "attribute 'Method_None' of 'Base' object is read-only", f, b)
    AssertErrorWithMessage(AttributeError, "'OverrideNothing' object has no attribute 'Method_None'", f, o1)
    AssertErrorWithMessage(AttributeError, "attribute 'Method_None' of 'OverrideAll' object is read-only", f, o2)

run_test(__name__)
