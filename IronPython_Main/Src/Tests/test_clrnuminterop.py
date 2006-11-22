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

'''
This module is in place to test the interoperatbility between CPython and CLR numerical 
types.

TODO:
- at the moment the test cases in place are simple sanity checks to ensure the 
  appropriate operator overloads have been implemented.  This needs to be extended
  quite a bit (e.g., see what happens with OverFlow cases).
- a few special cases aren't covered yet - comlex numbers, unary ops, System.Char,

'''

from lib.assert_util import *

if is_cli==False:
    from sys import exit
    exit(0)
    
import System
import clr
      
#Test Python/CLR number interop.
clr_integer_types = [ "System.Byte",
                      "System.SByte",
                      "System.Int16",
                      "System.UInt16",
                      "System.Int32",
                      "System.UInt32",
                      "System.Int64",
                      "System.UInt64"]

clr_float_types = [ "System.Single",
                    "System.Double",
                    "System.Decimal",
                  ]

#TODO - char???
clr_types = clr_integer_types + clr_float_types


py_integer_types = ["int", "long"]
py_float_types   = ["float"]

#TODO - special case complex???
py_types = py_integer_types + py_float_types

#------------------------------------------------------------------------------
def num_ok_for_type(number, proposed_type):
    '''
    Helper function returns true if the number param is within the range of 
    valid values for the proposed type
    '''
    #handle special cases first
    if proposed_type=="long":
        #arbitrary precision
        return True
    elif proposed_type=="float":
        #arbitrary precision
        return True
    
    if number >= eval(proposed_type + ".MinValue") and number <= eval(proposed_type + ".MaxValue"):
        return True
    return False
            
            
#------------------------------------------------------------------------------
def _test_interop_set(clr_types, py_types, test_cases):
    '''
    Helper function which permutes Python/CLR types onto test cases
    '''
    #each test case
    for leftop, op, rightop, expected_value in test_cases:
        
        #get the left operand as a Python type
        py_left = eval(leftop)
        
        #------------------------------------------------------------------
        #create a list of values where each element is the lefthand operand
        #converted to a CLR type
        leftop_clr_types = [x for x in clr_types if num_ok_for_type(py_left, x)]
        leftop_clr_values = [eval(x + "(" + leftop + ")") for x in leftop_clr_types]
                    
        #------------------------------------------------------------------
        #create a list of values where each element is the lefthand operand
        #converted to a Python type
        leftop_py_types = [x for x in py_types if num_ok_for_type(py_left, x)]
        leftop_py_values = [eval(x + "(" + leftop + ")") for x in leftop_py_types]
        
        #------------------------------------------------------------------
        #get the right operand as a Python type
        py_right = eval(rightop)       
        rightop_clr_types = [x for x in clr_types if num_ok_for_type(py_right, x)]
        rightop_clr_values = [eval(x + "(" + rightop + ")") for x in rightop_clr_types]
                    
        #------------------------------------------------------------------
        #create a list of values where each element is the lefthand operand
        #converted to a Python type
        rightop_py_types = [x for x in py_types if num_ok_for_type(py_right, x)]
        rightop_py_values = [eval(x + "(" + rightop + ")") for x in rightop_py_types]
                
        #------------------------------------------------------------------
        #Comparisons between CPython/CLR types
        def assertionHelper(left_type, left_op, op, right_type, right_op, expected):
            '''
            Helper function used to figure out which test cases fail
            without blowing up the rest of the test.
            '''
            expression_str = left_type + "("+ left_op +") " + str(op) + " " + right_type + "("+ right_op +")"
            try:
                expression = eval(expression_str)
            except TypeError, e:
                print "TYPE BUG:", expression_str
                return

            #CodePlex Work Item 5682
            #TODO: once 5682 gets closed, remove the try/except clause here.                
            try:
                AreEqual(expression, expected)
            except AssertionError, e:
                print "ASSERTION BUG:", expression_str, ";  EXPECTED:", str(expected)
                return
            
        #CLR-CLR           
        for x in leftop_clr_types:
            for y in rightop_clr_types:
                assertionHelper(x, leftop, op, y, rightop, expected_value)             
                             
        #CLR-PY
        for x in leftop_clr_types:
            for y in rightop_py_types:
                assertionHelper(x, leftop, op, y, rightop, expected_value)
                             
        #PY-CLR
        for x in leftop_py_types:
            for y in rightop_clr_types:
                assertionHelper(x, leftop, op, y, rightop, expected_value)
            
        #PY-PY
        for x in leftop_py_types:
            for y in rightop_py_types:
                assertionHelper(x, leftop, op, y, rightop, expected_value)

#------------------------------------------------------------------------------
#--BOOLEAN
bool_test_cases = [ 
                    #x==x
                    ("0","==","0", True),
                    ("0.0","==","0", True),
                    ("1","==","1", True),
                    ("3","==","3", True),
                    ("-1","==","-1", True),
                    
                    #! x==x
                    ("10","==","0", False),
                    ("10.0","==","0", False),
                    ("11","==","1", False),
                    ("31","==","3", False),
                    ("-11","==","-1", False),
                    
                    #x!=x
                    ("10","!=","0", True),
                    ("10.0","!=","0", True),
                    ("11","!=","1", True),
                    ("31","!=","3", True),
                    ("-11","!=","-1", True),
                    
                    #! x!=x
                    ("0","!=","0", False),
                    ("0.0","!=","0", False),
                    ("1","!=","1", False),
                    ("3","!=","3", False),
                    ("-1","!=","-1", False),
                    
                    #x<=x
                    ("0","<=","0", True),
                    ("0.0","<=","0", True),
                    ("1","<=","1", True),
                    ("3","<=","3", True),
                    ("-1","<=","-1", True),
                        
                    #! x<=x
                    ("10","<=","0", False),
                    ("10.0","<=","0", False),
                    ("11","<=","1", False),
                    ("13","<=","3", False),
                    ("10","<=","-1", False),
                    
                    #x>=x
                    ("0",">=","0", True),
                    ("0.0",">=","0", True),
                    ("1",">=","1", True),
                    ("3",">=","3", True),
                    ("-1",">=","-1", True),
                   
                    #! x>=x
                    ("0",">=","10", False),
                    ("0.0",">=","10", False),
                    ("1",">=","11", False),
                    ("3",">=","13", False),
                    ("-1",">=","11", False),
                    
                    #x<=/<y
                    ("0", "<=", "1", True),
                    ("0", "<", "1", True),
                    ("3.14", "<=", "19", True),
                    ("3.14", "<", "19", True),
                    
                    #!x<=/<y
                    ("10", "<=", "1", False),
                    ("10", "<", "1", False),
                    ("31.14", "<=", "19", False),
                    ("31.14", "<", "19", False),
                         
                    #x>=/.y
                    ("10", ">=", "1", True),
                    ("10", ">", "1", True),
                    ("31.14", ">=", "19", True),
                    ("31.14", ">", "19", True),
                    
                    #! x>=/.y
                    ("0", ">=", "1", False),
                    ("0", ">", "1", False),
                    ("3.14", ">=", "19", False),
                    ("3.14", ">", "19", False),
                   ]
              
def test_boolean():
    '''
    Test boolean operations involving a left and right operand
    '''
    _test_interop_set(clr_types, py_types, bool_test_cases)
    
#------------------------------------------------------------------------------
#--ARITHMETIC
#TODO - unary minus, unary plus
arith_test_cases = [
                    #add
                    ("0", "+", "0", 0),
                    ("0", "+", "1", 1),
                    ("1", "+", "-1", 0),
                    ("2", "+", "-1", 1),
                    
                    #sub
                    ("0", "-", "0", 0),
                    ("0", "-", "1", -1),
                    ("1", "-", "-1", 2),
                    ("2", "-", "-1", 3),
                    
                    #mult
                    ("0", "*", "0", 0),
                    ("0", "*", "1", 0),
                    ("2", "*", "1", 2),
                    ("1", "*", "-1", -1),
                    ("2", "*", "-1", -2),
                    
                    #div
                    ("0", "/", "1", 0),
                    ("4", "/", "2", 2),
                    ("2", "/", "1", 2),
                    ("1", "/", "-1", -1),
                    ("2", "/", "-1", -2),
                    
                    #trun div
                    ("0", "//", "1", 0),
                    ("4", "//", "2", 2),
                    ("2", "//", "1", 2),
                    ("1", "//", "-1", -1),
                    ("2", "//", "-1", -2),
                    ("3", "//", "2", 1),
                    
                    #power
                    ("0", "**", "1", 0),
                    ("4", "**", "2", 16),
                    ("2", "**", "1", 2),
                    ("1", "**", "-1", 1),
                    
                    #mod
                    ("0", "%", "1", 0),
                    ("5", "%", "2", 1),
                    ("2", "%", "1", 0),
                    ("1", "%", "-1", 0),
                    ("2", "%", "-1", 0),
                   ]

def test_arithmetic():
    '''
    Test general arithmetic operations.
    '''
    _test_interop_set(clr_types, py_types, arith_test_cases) 
    
    
#------------------------------------------------------------------------------
#BITWISE and SHIFT
#TODO: bitwise negation
bitwise_test_cases = [
                        #left shift
                        ("0", "<<", "1", 0),
                        ("3", "<<", "1", 6),
                        ("-3", "<<", "1", -6),
                        
                        #right shift
                        ("0", ">>", "1", 0),
                        ("6", ">>", "1", 3),
                        ("-3", ">>", "1", -2),
                        
                        #bitwise AND
                        ("0", "&", "1", 0),
                        ("1", "&", "1", 1),  
                        ("7", "&", "2", 2),
                        ("-1", "&", "1", 1),    
                        
                        #bitwise OR
                        ("0", "|", "1", 1),
                        ("1", "|", "1", 1),  
                        ("4", "|", "2", 6),
                        ("-1", "|", "1", -1),    
                        
                        #bitwise XOR
                        ("0", "^", "1", 1),
                        ("1", "^", "1", 0),  
                        ("7", "^", "2", 5),
                        ("-1", "^", "1", -2), 
                     ]

def test_bitwiseshift():
    '''
    Test bitwise and shifting operations.
    '''
    _test_interop_set(clr_integer_types, py_integer_types, bitwise_test_cases) 


run_test(__name__)