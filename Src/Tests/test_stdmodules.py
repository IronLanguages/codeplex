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
This module consists of test cases where IronPython was broken under standard CPython
Python-based modules.
'''

import sys
from lib.assert_util import *
skiptest("silverlight")

if directory_exists(testpath.lib_testdir):
    sys.path.append(testpath.lib_testdir)
else:
    print "Need access to CPython's libraries to run this test"
    sys.exit(0)

##GLOBALS######################################################################


##TEST CASES###################################################################
def test_cp8678():
    from itertools import izip
    x = iter(range(4))
    expected = ([0, 1], [2, 3])
    actual = []
    
    for i, j in izip(x, x): 
        actual.append([i, j])

    AreEqual(len(expected), len(actual))
    for i in xrange(len(expected)):
        AreEqual(expected[i], actual[i])

@skip("win32", "multiple_execute") #No _socket    
def test_cp10825():
    import urllib
    temp_url = urllib.urlopen("http://www.microsoft.com")
    try:
        AreEqual(temp_url.url, "http://www.microsoft.com/en/us/default.aspx")
    finally:
        temp_url.close()

def test_cp5566():
    import base64
    AreEqual(base64.decodestring('w/=='), '\xc3')
    test_str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~!@#$%^&*()_+-=[]\{}|;':,.//<>?\""
    test_str+= "/a/b/c/d/e/f/g/h/i/j/k/l/m/n/o/p/q/r/s/t/u/v/w/x/y/z/A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z/0/1/2/3/4/5/6/7/8/9/~/!/@/#/$/%/^/&/*/(/)/_/+/-/=/[/]/\/{/}/|/;/'/:/,/.///</>/?\""
    
    for str_function in [str, unicode]:
        AreEqual(base64.decodestring(str_function(test_str)),
                'i\xb7\x1dy\xf8!\x8a9%\x9az)\xaa\xbb-\xba\xfc1\xcb0\x01\x081\x05\x18r\t(\xb3\r8\xf4\x11I5\x15Yv\x19\xd3]\xb7\xe3\x9e\xbb\xf3\xdf')

@skip("win32")
def test_cp13618():
    import os
    from System.IO.Path import PathSeparator
    AreEqual(os.pathsep, PathSeparator)

def test_cp12907():
    #from codeop import compile_command, PyCF_DONT_IMPLY_DEDENT
    from nt import unlink
    
    f_name = "fake_stdout.txt"
    test_list = [
                    ("print 1", 
                        "single", ["1\n"]),
                    ("print 1", 
                        "exec", ["1\n"]),
                    ("1", 
                        "single", ["1\n"]),
                    ("1", 
                        "exec", []),
                    #CodePlex 12907
                    ("def f(n):\n    return n*n\nprint f(3)", 
                        "exec", ["9\n"]),
                    ("if 1:\n    print 1\n", 
                        "single", ["1\n"]),
                    ("if 1:\n    print 1\n", 
                        "exec", ["1\n"]),
                ]
                
    for test_case, kind, expected in test_list:
        
        c = compile(test_case, "", kind, 0x200, 1)
        try:
            orig_stdout = sys.stdout
            
            sys.stdout = open(f_name, "w")
            exec c
            sys.stdout.close()
            
            t_file = open(f_name, "r")
            lines = t_file.readlines()
            t_file.close()
            
            AreEqual(lines, expected)
            
        finally:
            sys.stdout = orig_stdout
    nt.unlink(f_name)

    #negative cases
    bad_test_list = [
                    ("def f(n):\n    return n*n\n\nf(3)\n", "single"),
                    ("def f(n):\n    return n*n\n\nf(3)",   "single"),
                    ("def f(n):\n    return n*n\n\nf(3)\n", "single"),
                    ("if 1:\n    print 1",                  "single"),
                    ("if 1:\n    print 1",                  "exec"),
                ]
                
    for test_case, kind in bad_test_list:
        print test_case, kind
        AssertError(SyntaxError, compile, test_case, "", kind, 0x200, 1)


##MAIN#########################################################################    
run_test(__name__)
