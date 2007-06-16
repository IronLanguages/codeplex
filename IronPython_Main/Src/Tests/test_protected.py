#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

import sys
from lib.assert_util import *

load_iron_python_test()
from IronPythonTest import *

# properties w/ differening access
@skip("silverlight", "Rowan #245494")
def test_base():
    a = BaseClass()
    AreEqual(a.Area, 0)
    a.Area = 16
    AreEqual(a.Area, 16)


@skip("silverlight", "Rowan #245494")
def test_derived():
    class MyBaseClass(BaseClass):
        def MySetArea(self, size):
            self.Area = size


    a = MyBaseClass()
    AreEqual(a.Area, 0)

    a.MySetArea(16)
    AreEqual(a.Area, 16)

    a.Area = 36
    AreEqual(a.Area, 36)

    # protected fields
    AreEqual(a.foo, 0)
    a.foo = 7
    AreEqual(a.foo, 7)


@skip("silverlight", "Rowan #245494")
def test_override():
    # overriding methods
    a = Inherited()
    AreEqual(a.ProtectedMethod(), 'Inherited.ProtectedMethod')
    AreEqual(a.ProtectedProperty, 'Inherited.Protected')

    class MyInherited(Inherited):
        def ProtectedMethod(self):
            return "MyInherited"
        def ProtectedMethod(self):
            return "MyInherited Override"
        def ProtectedPropertyGetter(self):
            return "MyInherited.Protected"
        ProtectedProperty = property(ProtectedPropertyGetter)

    a = MyInherited()
    
    AreEqual(a.ProtectedMethod(), 'MyInherited Override')
    AreEqual(a.CallProtected(), 'MyInherited Override')
    AreEqual(a.ProtectedProperty, "MyInherited.Protected")
    AreEqual(a.CallProtectedProp(), "MyInherited.Protected")
    

run_test(__name__)