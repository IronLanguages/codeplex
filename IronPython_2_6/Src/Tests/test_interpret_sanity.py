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

from iptest.assert_util import *
skiptest("win32")
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
    preserve_syspath()
    try:
        # Just try some important tests.
        # The full test suite should pass using -X:Interpret; this is just a lightweight check for "run 0".

        import test_delegate
        restore_syspath()
        import test_function
        restore_syspath()
        import test_closure
        restore_syspath()
        import test_generator
        restore_syspath()
        import test_tcf
        restore_syspath()
        import test_methoddispatch
        restore_syspath()
        import test_operator
        restore_syspath()
        import test_exec
        restore_syspath()
        import test_list
        restore_syspath()
        import test_cliclass
        restore_syspath()
        import test_exceptions
        restore_syspath()
        # These two pass, but take forever to run
        #import test_numtypes
        #import test_number
        import test_str
        restore_syspath()
        import test_math
        restore_syspath()
        #Due to http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22345,
        #test_namebinding must be run after test_math.
        import test_namebinding
        restore_syspath()
        import test_statics
        restore_syspath()
        import test_property
        restore_syspath()
        if not is_silverlight:
            import test_weakref
            restore_syspath()
        import test_specialcontext
        restore_syspath()
        import test_thread
        restore_syspath()
        import test_dict
        restore_syspath()
        import test_set
        restore_syspath()
        import test_tuple
        restore_syspath()
        import test_class
        restore_syspath()
        import test_syntax
        restore_syspath()
    finally:
        IronPythonTest.TestHelpers.GetContext().Options.InterpretedMode = save
        # "Un-import" these modules so that they get re-imported in emit mode
        sys.modules = modules
        restore_syspath()

run_test(__name__)
