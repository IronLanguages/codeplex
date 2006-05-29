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


Assert("__name__" in dir())
Assert("__builtins__" in dir())

import sys
import clr
clr.AddReferenceToFileAndPath(sys.prefix + "\\IronPython.dll")
import IronPython
version = IronPython.Hosting.PythonEngine.Version
Assert(version != "")

save = IronPython.Compiler.Options.FastEval
IronPython.Compiler.Options.FastEval = True
AreEqual(eval("None"), None)
AreEqual(eval("str(2)"), "2")
IronPython.Compiler.Options.FastEval = save

# verify no interferece between PythonEngine used in IronPythonConsole and user created one
oldpath = sys.path
pe = IronPython.Hosting.PythonEngine()
AreEqual(sys.path, oldpath)


# now verify CLR loaded modules don't interfere 

result = pe.Execute("""import clr

clr.AddReferenceByPartialName('System.Security')
import System.Security.Cryptography
assert hasattr(System.Security.Cryptography, 'CryptographicAttributeObject')
""")

# verify our engine doesn't see it
import System.Security
Assert(not hasattr(System.Security.Cryptography, 'CryptographicAttributeObject'))
