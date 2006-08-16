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
## Test "-X:PrivateBinding"
##

from lib.assert_util import *
from lib.file_util import *
from lib.process_util import *

import System
privateBinding = "-X:PrivateBinding" in System.Environment.GetCommandLineArgs()

load_iron_python_test()
import IronPythonTest
from IronPythonTest import *

clsPart = ClsPart()

def Negate(i): return -i

def test_Common():
    # !!! AreEqual("InternalClsPart" in dir(IronPythonTest), privateBinding)
    # !!! AreEqual("InternalClsPart" in dir(), privateBinding)
    AreEqual("_ClsPart__privateField" in dir(ClsPart), privateBinding)
    AreEqual("_ClsPart__privateProperty" in dir(ClsPart), privateBinding)
    AreEqual("_ClsPart__privateEvent" in dir(ClsPart), privateBinding)
    AreEqual("_ClsPart__privateMethod" in dir(ClsPart), privateBinding)
    pass

if not privateBinding: 
    def test_NormalBinding():    
        try:
            # no public types in namespace, shouldn't be able to get namespace
            from IronPython.Compiler import Generation
        except ImportError:
            pass

        # mixed namespace
        import IronPython.Runtime
        AssertError(AttributeError, lambda: IronPython.Runtime.TopReflectedPackage)
        
else: 
    def test_PrivateBinding():
        # entirely internal namespace
        from IronPython.Compiler import Generation
        x = Generation.Namespace(None)
        
        # mixed namespace
        import IronPython.Runtime
        x = IronPython.Runtime.TopReflectedPackage
        
        clsPart._ClsPart__privateField = 1
        AreEqual(clsPart._ClsPart__privateField, 1)
        clsPart._ClsPart__privateProperty = 1
        AreEqual(clsPart._ClsPart__privateProperty, 1)
        # !!! clsPart._ClswPart__privateEvent += Negate
        clsPart._ClsPart__privateEvent = Negate
        AreEqual(clsPart._ClsPart__privateMethod(1), -1)
        
        # !!! internalClsPart = InternalClsPart()
        internalClsPart = IronPythonTest.InternalClsPart()
        internalClsPart._InternalClsPart__Field = 1
        AreEqual(internalClsPart._InternalClsPart__Field, 1)
        internalClsPart._InternalClsPart__Property = 1
        AreEqual(internalClsPart._InternalClsPart__Property, 1)
        # !!! internalClsPart._InternalClsPart__Event += Negate
        internalClsPart._InternalClsPart__Event = Negate
        AreEqual(internalClsPart._InternalClsPart__Method(1), -1)
        
        # !!! AreEqual("_InternalClsPart__privateField" in dir(IronPythonTest.InternalClsPart), True)
        # !!! AreEqual("_InternalClsPart__privateProperty" in dir(InternalClsPart), True)
        # !!! AreEqual("_InternalClsPart__privateEvent" in dir(InternalClsPart), True)
        # !!! AreEqual("_InternalClsPart__privateMethod" in dir(InternalClsPart), True)

# use this when running standalone
#run_test(__name__)

run_test(__name__, noOutputPlease=True)

if not privateBinding:
    launch_ironpython_changing_extensions(__file__, add=["-X:PrivateBinding"])