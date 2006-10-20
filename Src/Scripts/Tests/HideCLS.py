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

from Util.Debug import *

# our built in types shouldn't show CLS methods

AreEqual(hasattr(object, 'ToString'), False)
AreEqual(dir(object).count('ToString'), 0)

AreEqual(hasattr('abc', 'ToString'), False)
AreEqual(dir('abc').count('ToString'), 0)

AreEqual(hasattr([], 'ToString'), False)
AreEqual(dir([]).count('ToString'), 0)

import System

# but CLS types w/o the attribute should....
AreEqual(hasattr(System.Environment, 'ToString'), True)
AreEqual(dir(System.Environment).count('ToString'), 1)


# and importing clr should show them all...
import clr

AreEqual(hasattr(object, 'ToString'), True)
AreEqual(dir(object).count('ToString'), 1)

AreEqual(hasattr('abc', 'ToString'), True)
AreEqual(dir('abc').count('ToString'), 1)

AreEqual(hasattr([], 'ToString'), True)
AreEqual(dir([]).count('ToString'), 1)


# and they should still show up on system.
AreEqual(hasattr(System.Environment, 'ToString'), True)
AreEqual(dir(System.Environment).count('ToString'), 1)

# eval should flow it's context
a = "hello world"
c = compile("x = a.Split(' ')", "<string>", "single")
eval(c)
AreEqual(x[0], "hello")
AreEqual(x[1], "world")

y = eval("a.Split(' ')")
AreEqual(y[0], "hello")
AreEqual(y[1], "world")
