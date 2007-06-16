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

##
## Test "-X:PrivateBinding"
##

from lib.assert_util import *
import System

if not is_silverlight:
    privateBinding = "-X:PrivateBinding" in System.Environment.GetCommandLineArgs()
else:
    privateBinding = False

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
        AssertError(AttributeError, lambda: IronPython.Runtime.SetHelpers)
        
else: 
    def test_PrivateBinding():
        # entirely internal namespace
        ### need new entirely internal namespace !!!
        #from IronPython.Compiler import Generation
        #x = Generation.Namespace(None)
        
        # mixed namespace
        import Microsoft.Scripting
        x = Microsoft.Scripting.TopReflectedPackage
        
        clsPart._ClsPart__privateField = 1
        AreEqual(clsPart._ClsPart__privateField, 1)
        clsPart._ClsPart__privateProperty = 1
        AreEqual(clsPart._ClsPart__privateProperty, 1)
        def bad_assign():
            clsPart._ClsPart__privateEvent = Negate
        AssertError(AttributeError, bad_assign)
        clsPart._ClsPart__privateEvent += Negate
        clsPart._ClsPart__privateEvent -= Negate
        AreEqual(clsPart._ClsPart__privateMethod(1), -1)
        
        # !!! internalClsPart = InternalClsPart()
        internalClsPart = IronPythonTest.InternalClsPart()
        internalClsPart._InternalClsPart__Field = 1
        AreEqual(internalClsPart._InternalClsPart__Field, 1)
        internalClsPart._InternalClsPart__Property = 1
        AreEqual(internalClsPart._InternalClsPart__Property, 1)
        def bad_assign():
            internalClsPart._InternalClsPart__Event = Negate
        AssertError(AttributeError, bad_assign)
        internalClsPart._InternalClsPart__Event += Negate
        internalClsPart._InternalClsPart__Event -= Negate
        AreEqual(internalClsPart._InternalClsPart__Method(1), -1)
        
        # !!! AreEqual("_InternalClsPart__privateField" in dir(IronPythonTest.InternalClsPart), True)
        # !!! AreEqual("_InternalClsPart__privateProperty" in dir(InternalClsPart), True)
        # !!! AreEqual("_InternalClsPart__privateEvent" in dir(InternalClsPart), True)
        # !!! AreEqual("_InternalClsPart__privateMethod" in dir(InternalClsPart), True)

# use this when running standalone
#run_test(__name__)

run_test(__name__, noOutputPlease=True)

if not privateBinding and not is_silverlight:
    from lib.process_util import launch_ironpython_changing_extensions
    AreEqual(launch_ironpython_changing_extensions(__file__, add=["-X:PrivateBinding"]), 0)