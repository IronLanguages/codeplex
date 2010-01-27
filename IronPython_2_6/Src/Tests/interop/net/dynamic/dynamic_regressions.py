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

"""
This module consists of regression tests for CodePlex and Dev10 IronPython bugs on
.NET 4.0's dynamic feature added primarily by IP developers that need to be 
folded into other test modules and packages.

Any test case added to this file should be of the form:
    def test_cp1234(): ...
where 'cp' refers to the fact that the test case is for a regression on CodePlex
(use 'dev10' for Dev10 bugs).  '1234' should refer to the CodePlex or Dev10
Work Item number.
"""

#------------------------------------------------------------------------------
#--Imports
from iptest.assert_util import *
skiptest("win32")
skiptest("silverlight")
if not is_net40:
    print "This test module should only be run from .NET 4.0!"
    sys.exit(0)

import sys
import clr
clr.AddReference("IronPythonTest")
import IronPythonTest.DynamicRegressions as DR

#------------------------------------------------------------------------------
#--Globals

#------------------------------------------------------------------------------
#--Test cases
def test_cp24117():
    if False: #Expectation
        AreEqual(DR.cp24117(xrange),    "<type 'xrange'>")
        AreEqual(DR.cp24117(xrange(3)), "xrange(3)")
    else: #Actual
        AreEqual(DR.cp24117(xrange),    "IronPython.Runtime.Types.PythonType")
        AreEqual(DR.cp24117(xrange(3)), "IronPython.Runtime.XRange")

#------------------------------------------------------------------------------
#--Main
run_test(__name__)
