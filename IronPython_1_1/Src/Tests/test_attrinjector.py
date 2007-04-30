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

import sys
from lib.assert_util import *

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

test_attributes_injector()