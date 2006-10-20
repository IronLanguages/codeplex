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
sys.path.append(sys.path[0] + '\\..')
from Util.Debug import *



import sys
load_iron_python_test()

import IronPythonTest
id1 = id(IronPythonTest)
AreEqual(dir(IronPythonTest).count('BindResult'), 1)
AreEqual(dir(IronPythonTest).count('PythonFunc'), 0)

sys.path.append(sys.path[0] + '\\ModPath')
AreEqual(dir(IronPythonTest).count('PythonFunc'), 0)
AreEqual(dir(IronPythonTest).count('BindResult'), 1)

import IronPythonTest

AreEqual(dir(IronPythonTest).count('PythonFunc'), 1)
AreEqual(dir(IronPythonTest).count('BindResult'), 1)

id2 = id(IronPythonTest)

id3 = id(sys.modules['IronPythonTest'])

AreEqual(id1, id2)
AreEqual(id2, id3)