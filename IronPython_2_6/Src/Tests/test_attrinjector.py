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

import sys
from iptest.assert_util import *

#System.Xml is unavailable in silverlight
skiptest("silverlight")
skiptest("win32")

load_iron_python_test()
from IronPythonTest import *

def test_attributes_injector():
    # load XML Dom
    x = AttrInjectorTest.LoadXml('<root><foo>foo text</foo><bar><baz>baz text</baz></bar></root>')

    # access injected attributes

    AreEqual(x.GetType().Name, 'XmlElement')
    AreEqual(x.foo.GetType().Name, 'String')
    AreEqual(x.foo, 'foo text')

    AreEqual(x.GetType().Name, 'XmlElement')
    AreEqual(x.bar.GetType().Name, 'XmlElement')
    AreEqual(x.bar.baz.GetType().Name, 'String')
    AreEqual(x.bar.baz, 'baz text')

def operator_test(a):
    AreEqual(a.doesnotexist, 42)
    
    # GetBoundMember shouldn't be called for pre-existing attributes, and we verify the name we got before.
    AreEqual(a.Name, 'doesnotexist')
    
    # SetMember should be called for sets
    a.somethingelse = 123
    AreEqual(a.Name, 'somethingelse')
    AreEqual(a.Value, 123)

def test_get_set_extended(): operator_test(ExtendedClass())

def test_get_set_instance(): operator_test(OperatorTest())

run_test(__name__)
