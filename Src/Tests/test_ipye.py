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

##
## Testing IronPython Engine
##

from lib.assert_util import *
import sys
import IronPython
pe = IronPython.Hosting.PythonEngine()

def test_trival():
    Assert(IronPython.Hosting.PythonEngine.Version != "")

def test_coverage():
    # 1. fasteval 
    save = IronPython.Compiler.Options.FastEvaluation
    IronPython.Compiler.Options.FastEvaluation = True
    AreEqual(eval("None"), None)
    AreEqual(eval("str(2)"), "2")
    IronPython.Compiler.Options.FastEvaluation = save
    
    # 2. ...

# verify no interferece between PythonEngine used in IronPython console and user created one
def test_no_interference(): 
    # 1. path
    oldpath = sys.path
    pe = IronPython.Hosting.PythonEngine()
    AreEqual(sys.path, oldpath)
    
    # 2. how about other states...

# now verify CLR loaded modules don't interfere 
def test_no_module_interference():
    pe.Execute("""import clr
clr.AddReferenceByPartialName('System.Security')
import System.Security.Cryptography
if not hasattr(System.Security.Cryptography, 'CryptographicAttributeObject'):
    raise AssertionError("CryptographicAttributeObject not found")
""")

    # verify our engine doesn't see it
    import System.Security
    Assert(not hasattr(System.Security.Cryptography, 'CryptographicAttributeObject'))

# tests wrote in C# EngineTest.cs
def test_engine():
    load_iron_python_test()
    import IronPythonTest
    et = IronPythonTest.EngineTest()
    for s in dir(et):
        if s.startswith("Scenario"):
            getattr(et, s)()

run_test(__name__)
