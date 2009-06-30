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

'''
This module consists of test cases which utilize the __clrtype__ method of 
Python callables set as the __metaclass__ member of Python classes.  Doing
this enables one to manually expose Python methods/attributes/classes directly
to the CLR.  For example, a Python class member might be exposed as a static
field on a CLR class.

Most tests found in this module deal with various nuances of __clrtype__
implementations and ensuring instances of Python classes using __clrtype__
via the __metaclass__ member behave the same as normal Python objects.

There are also a few sanity tests used to ensure the primary purpose
of __clrtype__ is actually met.  Namely, providing IronPython users the ability
to code entirely in Python without having to write Csharp code to do things 
like decorate their classes with custom attributes.  It should not be necessary 
to exhaustively test every possible use of __clrtype__ as we already get much 
of this coverage through our .NET interop inheritance tests.
'''


#--PRE-CLR IMPORT TESTS--------------------------------------------------------
if hasattr(type, "__clrtype__"):
    exc_msg = "type.__clrtype__ should not exist until the 'clr/System' module has been imported"
    print exc_msg
    #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=23251
    #raise Exception(exc_msg)
else:
    raise Exception("http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=23251 has been fixed. Please update test")


#--IMPORTS---------------------------------------------------------------------
from iptest.assert_util import *
skiptest("win32")

import System
import nt, os

#--GLOBALS---------------------------------------------------------------------


#--HELPERS---------------------------------------------------------------------


#--TEST CASES------------------------------------------------------------------

###SANITY TEST CASES####################
def test_sanity___clrtype___gets_called():
    '''
    Simple case.  Just make sure the __clrtype__ method gets called immediately 
    after the __metaclass__ is set and we can call type.__clrtype__() from our
    __clrtype__ implementation.
    '''
    global called
    called = False
    
    class MyType(type):
        def __clrtype__(self):
            global called
            called = True
            return super(MyType, self).__clrtype__()
    
    class X(object):
        __metaclass__ = MyType

    AreEqual(called, True)
    
    
def test_sanity_override_constructors():
    '''
    Create a new CLR Type and override all of its constructors.
    '''
    AddReferenceToDlrCore()
    
    from System import Reflection
    from Microsoft.Scripting.Generation import AssemblyGen
    from System.Reflection import Emit, FieldAttributes
    from System.Reflection.Emit import OpCodes    
    gen = AssemblyGen(Reflection.AssemblyName('test'), None, '.dll', False)
    
    try:
        class MyType(type):
            def __clrtype__(self):
                baseType = super(MyType, self).__clrtype__()
                t = gen.DefinePublicType(self.__name__, baseType, True)
                
                ctors = baseType.GetConstructors()
                for ctor in ctors:            
                    builder = t.DefineConstructor(
                        Reflection.MethodAttributes.Public, 
                        Reflection.CallingConventions.Standard, 
                        tuple([p.ParameterType for p in ctor.GetParameters()])
                    )
                    ilgen = builder.GetILGenerator()
                    ilgen.Emit(OpCodes.Ldarg, 0)
                    for index in range(len(ctor.GetParameters())):
                        ilgen.Emit(OpCodes.Ldarg, index + 1)
                    ilgen.Emit(OpCodes.Call, ctor)
                    ilgen.Emit(OpCodes.Ret)
                
                newType = t.CreateType()
                return newType
        
        class X(object):
            __metaclass__ = MyType
            def __init__(self):
                self.abc = 3
              
        a = X()
        AreEqual(a.abc, 3)
        
    finally:
        #gen.SaveAssembly()
        pass    


def test_sanity_static_dot_net_type():
    '''
    Create a new static CLR Type.
    '''
    import clr
    AddReferenceToDlrCore()
    
    from System import Reflection
    from Microsoft.Scripting.Generation import AssemblyGen
    from System.Reflection import Emit, FieldAttributes
    from System.Reflection.Emit import OpCodes      
    gen = AssemblyGen(Reflection.AssemblyName('test'), None, '.dll', False)
    
    try:
        class MyType(type):
            def __clrtype__(self):
                baseType = super(MyType, self).__clrtype__()
                t = gen.DefinePublicType(self.__name__, baseType, True)
                
                ctors = baseType.GetConstructors()
                for ctor in ctors:            
                    baseParams = ctor.GetParameters()
                    newParams = baseParams[1:]
                    
                    builder = t.DefineConstructor(
                        Reflection.MethodAttributes.Public, 
                        Reflection.CallingConventions.Standard, 
                        tuple([p.ParameterType for p in newParams])
                    )
                    fldAttrs = FieldAttributes.Static | FieldAttributes.Public
                    fld = t.DefineField('$$type', type, fldAttrs)
                    
                    ilgen = builder.GetILGenerator()
                    ilgen.Emit(OpCodes.Ldarg, 0)
                    ilgen.Emit(OpCodes.Ldsfld, fld)
                    for index in range(len(ctor.GetParameters())):
                        ilgen.Emit(OpCodes.Ldarg, index + 1)
                    ilgen.Emit(OpCodes.Call, ctor)
                    ilgen.Emit(OpCodes.Ret)

                    # keep a ctor which takes Python types as well so we 
                    # can be called from Python still.
                    builder = t.DefineConstructor(
                        Reflection.MethodAttributes.Public, 
                        Reflection.CallingConventions.Standard, 
                        tuple([p.ParameterType for p in ctor.GetParameters()])
                    )
                    ilgen = builder.GetILGenerator()
                    ilgen.Emit(OpCodes.Ldarg, 0)
                    for index in range(len(ctor.GetParameters())):
                        ilgen.Emit(OpCodes.Ldarg, index + 1)
                    ilgen.Emit(OpCodes.Call, ctor)
                    ilgen.Emit(OpCodes.Ret)

                newType = t.CreateType()
                newType.GetField('$$type').SetValue(None, self)
                return newType
        
        class MyCreatableDotNetType(object):
            __metaclass__ = MyType
            def __init__(self):
                self.abc = 3

        # TODO: Test Type.GetType (requires the base class to be non-transient)
    finally:
        #gen.SaveAssembly()
        pass    


###__CLRTYPE__#########################
def test_type___clrtype__():
    '''
    Tests out type.__clrtype__ directly.
    '''
    #Make sure it exists
    Assert(hasattr(type, "__clrtype__"))
    
    #Make sure the documentation is useful
    #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=23252
    AreEqual(type.__clrtype__.__doc__.replace("\r\n", "\n"),
             '''Type __clrtype__(self)
Type __clrtype__(self)''')
    
    AreEqual(type.__clrtype__(type),  type)
    AreEqual(type.__clrtype__(float), float)
    AreEqual(type.__clrtype__(System.Double), float)
    AreEqual(type.__clrtype__(float), System.Double)
    Assert(not type.__clrtype__(float)==type)
    Assert(type.__clrtype__(float)!=type)



def test_clrtype_returns_existing_python_types():
    '''
    TODO.
    Our implementation of __clrtype__ returns existing pure-Python types
    instead of subclassing from type or System.Type.
    '''
    global called
    
    for x in [
                float,
                #TODO
                ]:
        called = False
        
        class MyType(type):
            def __clrtype__(self):
                global called
                called = True
                return x
        
        class X(object):
            __metaclass__ = MyType
        
        AreEqual(called, True)


def test_clrtype_returns_existing_clr_types():
    '''
    TODO.
    Our implementation of __clrtype__ returns existing .NET types
    instead of subclassing from type or System.Type.
    '''
    global called
    
    for x in [
                #TODO
                ]:
        called = False
        
        class MyType(type):
            def __clrtype__(self):
                global called
                called = True
                return x
        
        class X(object):
            __metaclass__ = MyType
        
        AreEqual(called, True)


###TYPE IMPLEMENTATIONS################
def test_type_constructor_args():
    '''
    TODO:
    - only stipulation is the first parameter must be PythonType
    - subclass of PythonType?
    - overloads
    - generics?
    '''
    pass


def test_type_inheritance():
    '''
    TODO:
    - type must implement IPythonObject
    - type implements IPythonObject in Csharp
    - type implements IPythonObject in IronPython
    - type implements IPO, but members are private?
    - type implements IPO and other interfaces
    - type is a subclass of a class implementing IPO: subclass of "type", "float", etc
    - type is a generic and implements IPO
    '''
    pass


###CRITICAL SCENARIOS##################
def test_critical_custom_attributes():
    '''
    TODO.
    Add custom attributes to a Python class:
    - parameterless
    - positional attribute parameters
    '''
    pass


def test_critical_wpf_databinding():
    '''
    TODO.
    Can WPF APIs (e.g., ListBox.ItemTemplate) automatically detect Python properties?
    '''
    pass
    
    
def test_critical_clr_reflection():
    '''
    TODO.
    Can we use CLR reflection over a Python type?
    '''
    pass
    
    
def test_critical_parameterless_constructor():
    '''
    TODO.
    '''
    pass    


###PYTHON OBJECT CHARACTERISTIC########
def test_clrtype_metaclass_characteristics():
    '''
    TODO.
    Make sure clrtype is a properly behaved Python metaclass
    '''
    pass

###NEGATIVE TEST CASES#################
def test_neg_type___clrtype__():
    '''
    Tests out negative type.__clrtype__ cases.
    '''
    #Number of params
    AssertErrorWithMessage(TypeError, "__clrtype__() takes exactly 1 argument (0 given)", 
                           type.__clrtype__)
    AssertErrorWithMessage(TypeError, "__clrtype__() takes exactly 1 argument (2 given)", 
                           type.__clrtype__, None, None)
    AssertErrorWithMessage(TypeError, "__clrtype__() takes exactly 1 argument (3 given)", 
                           type.__clrtype__, None, None, None)
    
    #Wrong param type                           
    AssertErrorWithPartialMessage(TypeError, ", got NoneType", 
                                  type.__clrtype__, None)
    AssertErrorWithPartialMessage(TypeError, ", got float", 
                                  type.__clrtype__, 3.14)
                           
    for x in [None, [], (None,), Exception("message"), 3.14, 3L, 0, 5j, "string", u"string",
              True, System, nt, os, exit, lambda: 3.14]:
        AssertError(TypeError, 
                    type.__clrtype__, x)

    #Shouldn't be able to set __clrtype__ to something else
    AssertErrorWithMessage(AttributeError, "attribute '__clrtype__' of 'type' object is read-only",
                           setattr, type, "__clrtype__", None)


def test_neg_clrtype_wrong_case():
    '''
    Define the __clrtype__ function using the wrong case and see what happens.
    '''
    global called
    called = False
    
    class MyType(type):
        def __clrType__(self):
            global called
            called = True
            return super(MyType, self).__clrtype__()
    
    class X(object):
        __metaclass__ = MyType

    AreEqual(called, False)


def test_neg_clrtype_returns_nonsense_values():
    '''
    The __clrtype__ implementation returns invalid values.
    '''
    global called
    
    for x, expected_msg in [[[], "expected Type, got list"], 
                            [(None,), "expected Type, got tuple"], 
                            [True, "expected Type, got bool"], 
                            [False, "expected Type, got bool"], 
                            [3.14, "expected Type, got float"], 
                            ["a string", "expected Type, got str"],
                            [System.UInt16(32), "expected Type, got UInt16"],
                            [1L, "expected Type, got long"],
                ]:
        called = False
        
        class MyType(type):
            def __clrtype__(self):
                global called
                called = True
                return x

        try:
            class X(object):
                __metaclass__ = MyType
            Fail("Arbitrary return values of __clrtype__ should not be allowed: " + str(x))
        except TypeError, e:
            AreEqual(e.message,
                     expected_msg)
        finally:    
            AreEqual(called, True)
        
        
    #http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=23244
    called = False
    
    class MyType(type):
        def __clrtype__(self):
            global called
            called = True
            return None
    
    try:
        class X(object):
            __metaclass__ = MyType
        Fail("Arbitrary return values of __clrtype__ are not allowed: ", + str(x))
    except SystemError, e:
        AreEqual(e.message,
                 "Object reference not set to an instance of an object.")
    finally:
        AreEqual(called, True)
    

def test_neg_clrtype_raises_exceptions():
    '''
    TODO.
    What happens when the __clrtype__ implementation raises exceptions?
    - IOError
    - BaseException, Exception
    - KeyboardInterrupt
    '''
    pass
    
    
def test_neg_type_constructor_args():
    '''
    TODO:
    - all cases where the first parameter of the System.Type subclass is not a PythonType
    '''
    pass
    

def test_neg_type_inheritance():
    '''
    TODO:
    - type implements IPO, but members are private
    - type implements IPO, but IPO methods return bogus values
    - type implements IPO, but IPO methods throw
    '''
    pass

        
###RANDOM STUFF########################
def test_misc():
    '''
    TODO:
    - gen.DefinePublicType(type.__clrtype__())
    - gen.DefinePublicType(subclass of type.__clrtype__())
    - methods defined in the type that are not also in the dictproxy instance
    - clr.getClrType(...) returns the right value
    - do instances of the type work in 'normal' methods (e.g., str())
    - change __class__ of instances
    '''
    pass


def test_stress():
    '''
    TODO.
    - __clrtype__ implementation takes a long time to return?
    '''
    pass
    

#--MAIN------------------------------------------------------------------------    
run_test(__name__)
