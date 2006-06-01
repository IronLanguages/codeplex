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
from IronPythonTest import *

# verify generic .NET method binding

# Create an instance of the generic method provider class.
gm = GenMeth()

# Check that the documentation strings for all the instance methods (they all have the same name) is as expected.
expected_inst_methods = 'str InstMeth[T]()\r\nstr InstMeth[(T, U)]()\r\nstr InstMeth[T](int arg1)\r\nstr InstMeth[T](str arg1)\r\nstr InstMeth[(T, U)](int arg1)\r\nstr InstMeth[T](T arg1)\r\nstr InstMeth[(T, U)](T arg1, U arg2)\r\nstr InstMeth()\r\nstr InstMeth(int arg1)\r\nstr InstMeth(str arg1)\r\n';
Assert(gm.InstMeth.__doc__ == expected_inst_methods)

# And the same for the static methods.
expected_static_methods = 'static str StaticMeth[T]()\r\nstatic str StaticMeth[(T, U)]()\r\nstatic str StaticMeth[T](int arg1)\r\nstatic str StaticMeth[T](str arg1)\r\nstatic str StaticMeth[(T, U)](int arg1)\r\nstatic str StaticMeth[T](T arg1)\r\nstatic str StaticMeth[(T, U)](T arg1, U arg2)\r\nstatic str StaticMeth()\r\nstatic str StaticMeth(int arg1)\r\nstatic str StaticMeth(str arg1)\r\n'
Assert(GenMeth.StaticMeth.__doc__ == expected_static_methods)

# Check that we bind to the correct method based on type and call arguments for each of our instance methods. We can validate this
# because each target method returns a unique string we can compare.
AreEqual(gm.InstMeth(), "InstMeth()")
AreEqual(gm.InstMeth[str](), "InstMeth<String>()")
AreEqual(gm.InstMeth[(int, str)](), "InstMeth<Int32, String>()")
AreEqual(gm.InstMeth(1), "InstMeth(Int32)")
AreEqual(gm.InstMeth(""), "InstMeth(String)")
AreEqual(gm.InstMeth[int](1), "InstMeth<Int32>(Int32)")
AreEqual(gm.InstMeth[str](""), "InstMeth<String>(String)")
AreEqual(gm.InstMeth[(str, int)](1), "InstMeth<String, Int32>(Int32)")
AreEqual(gm.InstMeth[GenMeth](gm), "InstMeth<GenMeth>(GenMeth)")
AreEqual(gm.InstMeth[(str, int)]("", 1), "InstMeth<String, Int32>(String, Int32)")

# And the same for the static methods.
AreEqual(GenMeth.StaticMeth(), "StaticMeth()")
AreEqual(GenMeth.StaticMeth[str](), "StaticMeth<String>()")
AreEqual(GenMeth.StaticMeth[(int, str)](), "StaticMeth<Int32, String>()")
AreEqual(GenMeth.StaticMeth(1), "StaticMeth(Int32)")
AreEqual(GenMeth.StaticMeth(""), "StaticMeth(String)")
AreEqual(GenMeth.StaticMeth[int](1), "StaticMeth<Int32>(Int32)")
AreEqual(GenMeth.StaticMeth[str](""), "StaticMeth<String>(String)")
AreEqual(GenMeth.StaticMeth[(str, int)](1), "StaticMeth<String, Int32>(Int32)")
AreEqual(GenMeth.StaticMeth[GenMeth](gm), "StaticMeth<GenMeth>(GenMeth)")
AreEqual(GenMeth.StaticMeth[(str, int)]("", 1), "StaticMeth<String, Int32>(String, Int32)")
