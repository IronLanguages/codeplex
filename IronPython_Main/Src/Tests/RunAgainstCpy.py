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
Simple script which executes tests written for IP (and relevant for CPython) under 
CPython to ensure compatibility.

Parameters: 
- first parameter is the directory containing test lists
- directories containing generic test files which should *not*
be run (e.g., they're only relevant for .NET). This is entirely optional

Assumptions:
- we start out in the "Tests" directory
- all tests we wish to run are in "Tests"
- the tests are named using the pattern test_*.py
'''

#------------------------------------------------------------------------------
#--IMPORTS

from sys import exit
from sys import executable
from sys import argv

from os  import system
from os  import listdir
from os  import getcwd
from os  import environ

#------------------------------------------------------------------------------
#--GLOBALS

#tests we do not wish to run. These should be in the "Tests" directory
EXCLUDE_LIST = ['test_winforms.py']

#For some reason test_math is taking extraordinary amounts of time to run
#under CPython 2.5.  For now, this is just disabled.
EXCLUDE_LIST.append("test_math.py")

#List of extra tests in "Tests" which do not follow the "test_*.py" pattern.
#These WILL be run.
EXTRA_INCLUDE_LIST = []

#Directory containing VSTS test lists
TEST_LIST_DIR = argv[1]

#Debugging...
DEBUG = False

EXCLUDE_LIST = [x.lower() for x in EXCLUDE_LIST]
EXTRA_INCLUDE_LIST = [x.lower() for x in EXTRA_INCLUDE_LIST]

#------------------------------------------------------------------------------

#get a list of all test_*.py files in the CWD
test_list = [ x.lower() for x in listdir(getcwd()) if x.startswith("test_") and x.endswith(".py") ]
if DEBUG:
    print "test_list:", test_list
    print

#figure out which tests we should ignore based on test list directories provided
#from the command line
for exclude_dir in argv[2:]:
    t_list = []
    for x in listdir(TEST_LIST_DIR + "\\" + exclude_dir):
        x = x.lower()
        x = x.replace("_1x.generictest", ".py")
        x = x.replace("_2x.generictest", ".py")
        x = x.replace("_20.generictest", ".py")
        
        if x.endswith(".py"):
            t_list.append(x)
        
    if DEBUG:
        print "exclude_dir:", exclude_dir, ", t_list:", t_list
        print 
    
    #strip out IP-only tests
    test_list = [ x for x in test_list if t_list.count(x)==0 ]   

#strip out all IP-only tests
test_list = [ x for x in test_list if EXCLUDE_LIST.count(x)==0 ]

#add the extra tests
#ensure no duplicates...
EXTRA_INCLUDE_LIST = [ x for x in EXTRA_INCLUDE_LIST if test_list.count(x)==0 ]
test_list = EXTRA_INCLUDE_LIST + test_list

#------------------------------------------------------------------------------
failed_tests = []
for test_name in test_list:
    print "-------------------------------------------------------------------"
    print "-- " + test_name
    #run the test
    ec = system(executable + " " + test_name)
    
    #if it fails, add it to the list
    if ec!=0:
        failed_tests.append(test_name + "; Exit Code=" + str(ec))
    print    
    
#------------------------------------------------------------------------------
print
print
print "#######################################################################"
if  len(failed_tests)==0:
    print "Everything passed!"
    exit(0)
else:
    print "The following tests failed:"
    for test_name in failed_tests: print test_name    
    exit(1)
    