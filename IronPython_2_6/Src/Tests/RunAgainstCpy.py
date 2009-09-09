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
- 

Assumptions:
- we start out in the "Tests" directory
- all tests we wish to run are in "Tests"
- the tests are named using the pattern test_*.py
'''

#------------------------------------------------------------------------------
#--IMPORTS

import subprocess
import sys
import os

#Find the location of the 'iptest' package
mroot = [os.environ[x] for x in os.environ.keys() if x.lower() == "merlin_root"]
if len(mroot)==0:
    rowan_bin = os.getcwd() + r"\..\..\bin\debug\lib"
else:
    rowan_bin = [os.environ[x] for x in os.environ.keys() if x.lower() == "rowan_bin"]
    if len(rowan_bin)==0:
        rowan_bin = mroot[0] + r"\bin\debug\lib"
    else:
        rowan_bin = rowan_bin[0] + r"\lib"
os.environ.update({ "pythonpath" : sys.exec_prefix + r"\lib;" + rowan_bin})


#------------------------------------------------------------------------------
#--GLOBALS

#tests we do not wish to run. These should be in the "Tests" directory
EXCLUDE_LIST = ["test_fuzz_parser.py"]

#For some reason test_math is taking extraordinary amounts of time to run
#under CPython 2.5.  For now, this is just disabled.
EXCLUDE_LIST.append("test_math.py")

#List of extra tests in "Tests" which do not follow the "test_*.py" pattern.
#These WILL be run.
EXTRA_INCLUDE_LIST = ["regressions.py"]

#Debugging...
DEBUG = False

EXCLUDE_LIST = [x.lower() for x in EXCLUDE_LIST]
EXTRA_INCLUDE_LIST = [x.lower() for x in EXTRA_INCLUDE_LIST]


#Test Packages
PKG_LIST = [ "modules"]

#------------------------------------------------------------------------------

#get a list of all test_*.py files in the CWD
test_list = [ x.lower() for x in os.listdir(os.getcwd()) if x.startswith("test_") and x.endswith(".py") ]
if DEBUG:
    print "test_list:", test_list
    print

#strip out all IP-only tests
test_list = [ x for x in test_list if EXCLUDE_LIST.count(x)==0 ]

#add the extra tests
#ensure no duplicates...
EXTRA_INCLUDE_LIST = [ x for x in EXTRA_INCLUDE_LIST if test_list.count(x)==0 ]
test_list = EXTRA_INCLUDE_LIST + test_list
if len(test_list)<50:
    print "Should have been more than 50 tests:", test_list
    raise Exception("Something's wrong with RunAgainstCpy.py")

#------------------------------------------------------------------------------
failed_tests = []
for test_name in test_list:
    print "-------------------------------------------------------------------"
    print "-- " + test_name
    #run the test
    ec = subprocess.call(sys.executable + " " + test_name,
                         env=os.environ,
                         shell=True)
    
    #if it fails, add it to the list
    if ec!=0:
        failed_tests.append(test_name + "; Exit Code=" + str(ec))
    print

#------------------------------------------------------------------------------
sys.path.append(os.getcwd())
for test_name in PKG_LIST:
    print "-------------------------------------------------------------------"
    print "-- " + test_name
    #run the test
    ec = subprocess.call(sys.executable + " harness.py " + test_name,
                         env=os.environ)
    
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
    sys.exit(0)
else:
    print "The following tests failed:"
    for test_name in failed_tests: print test_name
    sys.exit(1)
    
