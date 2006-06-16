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

from lib.assert_util import *
load_iron_python_test()
import IronPythonTest

a = IronPythonTest.Events()

called = False
def MyEventHandler():
    global called
    called = True

a.InstanceTest += IronPythonTest.EventTestDelegate(MyEventHandler)

a.CallInstance()
AreEqual(called, True)
called = False
IronPythonTest.Events.StaticTest += IronPythonTest.EventTestDelegate(MyEventHandler)
IronPythonTest.Events.CallStatic()

AreEqual(called, True)

import System

called = False
def myhandler(*args): 
    global called
    called = True
    AreEqual(args[0], 'abc')
    AreEqual(args[1], System.EventArgs.Empty)
    
IronPythonTest.Events.OtherStaticTest += myhandler

IronPythonTest.Events.CallOtherStatic('abc', System.EventArgs.Empty)

AreEqual(called, True)

try:
    del(a.InstanceTest)
    AreEqual(True, False)
except AttributeError:
    pass
    
try:
    a.InstanceTest = 'abc'
    AreEqual(True, False)
except TypeError:
    pass
    
try:
    IronPythonTest.Events.StaticTest = 'abc'
    AreEqual(True, False)
except TypeError:
    pass
    

