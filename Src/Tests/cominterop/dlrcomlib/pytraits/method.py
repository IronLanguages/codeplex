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


from lib.assert_util import skiptest
skiptest("win32", "silverlight")
from lib.cominterop_util import *
    
#------------------------------------------------------------------------------
#--GLOBALS
com_obj = CreateDlrComServer()
m0 = com_obj.SimpleMethod     #[id(1), helpstring("method SimpleMethod")] HRESULT SimpleMethod(void);
m2 = com_obj.StringArguments  #[id(3), helpstring("method StringArguments")] HRESULT StringArguments([in] BSTR arg1, [in] BSTR arg2);

STD_METHOD_ATTRIBUTES = ['__call__', '__class__', '__cmp__', '__delattr__', 
                        '__doc__', '__get__', '__getattribute__', '__hash__', 
                        '__init__', '__new__', '__reduce__', '__reduce_ex__', 
                        '__repr__', '__setattr__', '__str__', 'im_class', 
                        'im_func', 'im_self']

                        
#Merlin 370042
BROKEN_STD_METHOD_ATTRIBUTES = ['__cmp__', 'im_class', 'im_func', 'im_self']
#Merlin 370043
BROKEN_ID_STD_METHOD_ATTRIBUTES = ['__call__', '__cmp__', '__get__', 'im_class', 'im_func', 'im_self']


#------------------------------------------------------------------------------
#--HIGH-PRIORITY TESTS
def test_sanity():

    #--Just make sure all newstyle class method attributes are there
    m0_attributes = dir(m0)
    
    for std_attr in STD_METHOD_ATTRIBUTES:
        try:
            Assert(std_attr in m0_attributes, 
                   std_attr + " is not a method attribute of " + str(m0))
        except Exception, e:
            #Ignore known failures
            if is_pywin32: raise e
            
            if not preferComDispatch:
                if std_attr not in BROKEN_STD_METHOD_ATTRIBUTES: raise e
            else:
                if std_attr not in BROKEN_ID_STD_METHOD_ATTRIBUTES: raise e

#--------------------------------------    
def test_star_args():
    AreEqual(m0(*[]), None)
    AreEqual(m2(*["a", "b"]), None)
    
def test_star_args_neg():    
    #Merlin 369650
    #AssertErrorWithMessage(TypeError, 
    #                       "SimpleMethod() takes exactly 1 argument (2 given)",
    #                       lambda: m0(*[1]))
    
    #Merlin 369650
    #AssertErrorWithMessage(TypeError, 
    #                       "SimpleMethod() takes exactly 3 arguments (2 given)",
    #                       lambda: m2(*[1]))
    pass                         
                           
    
def test_keyword_args():
    AreEqual(m0(**{}), None)
    AreEqual(m2("a", "b", **{}), None)

def test_keyword_args_neg():
    pass
    
    
def test_star_args_keyword_args():
    AreEqual(m0(*[], **{}), None)
    AreEqual(m2(*["x", "y"], **{}), None)

def test_star_args_keyword_args_neg():
    pass

#--------------------------------------

def test_as_callable_obj():
    '''
    m = com_obj.m
    m()
    '''
    pass    

def test_method_del():
    pass    

def test_name_binding():    
    pass

def test_method_globals():
    pass
    
def test_method_locals():
    pass    

#--------------------------------------
def test_invoke_from_exec():
    pass
    
def test_invoke_from_eval():
    pass

def test_from_cmdline():
    pass    
    
#--------------------------------------
def test_help():
    pass

#--------------------------------------

def test_is():
    pass
    
def test_type():
    pass
    
def test_isinstance():
    pass    
    
#------------------------------------------------------------------------------
#--MEDIUM PRIORITY TESTS

#--Method attributes
@skip_comdispatch("Merlin 370043")
def test__call__():
    pass

def test__class__():
    pass
    
@skip("cli") #Merlin 370042    
def test__cmp__():
    pass
    
def test__delattr__():
    pass
    
def test__doc__():
    pass    
    
@skip_comdispatch("Merlin 370043")
def test__get__():
    pass
    
def test__getattribute__():
    pass
    
def test__hash__():
    pass
    
def test__init__():
    pass
    
def test__new__():
    pass
    
def test__reduce__():
    pass
    
def test__reduce_ex__():
    pass
    
def test__repr__():
    pass

def test__setattr__():
    pass
    
def test__str__():
    pass    


#------------------------------------------------------------------------------
def test_as_obj():
    pass

#--------------------------------------
def test_attr_access():
    pass
    
def test_attr_setting():    
    pass

#--------------------------------------
def test_method_return():
    pass
    
def test_method_yield():
    pass
    
def test_method_raise():
    pass

#--------------------------------------
@skip("cli") #Merlin 370042 
def test_im_func():
    '''
    ['__call__', '__class__', '__delattr__', '__dict__', '__doc__', '__get__', 
    '__getattribute__', '__hash__', '__init__', '__module__', '__name__', 
    '__new__', '__reduce__', '__reduce_ex__', '__repr__', '__setattr__', 
    '__str__', 'func_closure', 'func_code', 'func_defaults', 'func_dict', 
    'func_doc', 'func_globals', 'func_name']
    '''
    pass

#--------------------------------------  
@skip("cli") #Merlin 370042 
def test_im_self():
    '''
    ['__class__', '__delattr__', '__dict__', '__doc__', '__getattribute__', 
    '__hash__', '__init__', '__module__', '__new__', '__reduce__', '__reduce_ex__', 
    '__repr__', '__setattr__', '__str__', '__weakref__', 'm', 'm0']
    '''
    pass
    
#--------------------------------------
@skip("cli") #Merlin 370042 
def test_im_class():
    '''
    ['__class__', '__delattr__', '__dict__', '__doc__', '__getattribute__', 
    '__hash__', '__init__', '__module__', '__new__', '__reduce__', '__reduce_ex__', 
    '__repr__', '__setattr__', '__str__', '__weakref__', 'm', 'm0']
    '''
    pass    


#---------------------------------------
def test_dot_op():
    pass
    
def test_str_op():
    pass 
    

#--Internal types for code objects    
def test_im_func_func_code():
    '''
    ['__class__', '__cmp__', '__delattr__', '__doc__', '__getattribute__', 
    '__hash__', '__init__', '__new__', '__reduce__', '__reduce_ex__', 
    '__repr__', '__setattr__', '__str__', 'co_argcount', 'co_cellvars', 
    'co_code', 'co_consts', 'co_filename', 'co_firstlineno', 'co_flags', 
    'co_freevars', 'co_lnotab', 'co_name', 'co_names', 'co_nlocals', 
    'co_stacksize', 'co_varnames']
    '''
    pass    
    
#------------------------------------------------------------------------------
#--LOWER PRIORITY TESTS
    
def test_method_simple_ops():
    '''
    com_obj.m0 + 3
    not com_obj.m0
    '''
    pass
        
    

    
def test_misc_conversions():
    '''
    conv_methods = [ int, long, float, complex,
                    str, repr, eval, tuple, list, set, dict,
                    frozenset, chr, unichr, ord, hex, oct
                    ]
    '''
    pass
    

def test_invoke_with_apply():
    pass
    
def test_compile():
    pass    

#------------------------------------------------------------------------------
run_com_test(__name__, __file__)
