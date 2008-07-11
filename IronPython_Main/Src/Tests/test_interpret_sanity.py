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

'''
Smokescreen test for the -X:Interpret ipy.exe mode.
'''

from lib.assert_util import *
import sys

load_iron_python_test()
import IronPythonTest


#Rowan Work Item 312902
@skip("interpreted")
def test_interpreted():
    # IronPythonTest.TestHelpers.GetContext().Options.InterpretedMode is tested at compile time:
    # it will take effect not immediately, but in modules we import
    save = IronPythonTest.TestHelpers.GetContext().Options.InterpretedMode
    IronPythonTest.TestHelpers.GetContext().Options.InterpretedMode = True
    modules = sys.modules.copy()
    try:
        # Just try some important tests.
        # The full test suite should pass using -X:Interpret; this is just a lightweight check for "run 0".

        import test_delegate
        import test_function
        import test_closure
        import test_namebinding
        import test_generator
        import test_tcf
        import test_methoddispatch
        import test_operator
        import test_exec
        import test_list
        import test_cliclass
        import test_exceptions
        # These two pass, but take forever to run
        #import test_numtypes
        #import test_number
        import test_str
        import test_math
        import test_statics
        import test_property
        import test_weakref
        import test_specialcontext
        import test_thread
        import test_dict
        import test_set
        import test_tuple
        import test_class
        if not is_silverlight:
            #This particular test corrupts the run - CodePlex Work Item 11830
            import test_syntax
    finally:
        IronPythonTest.TestHelpers.GetContext().Options.InterpretedMode = save
        # "Un-import" these modules so that they get re-imported in emit mode
        sys.modules = modules

run_test(__name__)
