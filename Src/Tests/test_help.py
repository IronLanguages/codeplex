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
import sys

class stdout_reader:
    def __init__(self):
        self.text = ''
    def write(self, text):
        self.text += text
        

if is_cli:
    def test_z_cli_tests():    # runs last to prevent tainting the module w/ CLR names
        import clr
        import System        
        load_iron_python_test()        
        from IronPythonTest import WriteOnly

        sys.stdout = stdout_reader()
        
        help(WriteOnly)
        
        sys.stdout = sys.__stdout__

def test_module():
    import time
    sys.stdout = stdout_reader()
    
    help(time)
    
    x = sys.stdout.text
    sys.stdout = sys.__stdout__    
    
    Assert(x.find('clock(...)') != -1)      # should have help for our stuff
    Assert(x.find('unichr(...)') == -1)     # shouldn't have bulit-in help
    Assert(x.find('AscTime') == -1)         # shouldn't display CLI names
    Assert(x.find('static') == -1)          # methods shouldn't be displayed as static
    
def test_userfunction():
    def foo():
        """my help is useful"""
        
    sys.stdout = stdout_reader()
    help(foo)
    x = sys.stdout.text
    sys.stdout = sys.__stdout__
    
    Assert(x.find('my help is useful') != -1)
    
    
def test_builtinfunction():
    sys.stdout = stdout_reader()
    help(abs)
    x = sys.stdout.text
    sys.stdout = sys.__stdout__
    
    Assert(x.find('Return the absolute value of the argument') != -1)
    
def test_user_class():
    class foo(object):
        """this documentation is going to make the world a better place"""
                        
    sys.stdout = stdout_reader()
    help(foo)
    x = sys.stdout.text
    sys.stdout = sys.__stdout__
        
    Assert(x.find('this documentation is going to make the world a better place') != -1)
    

def test_methoddescriptor():
    sys.stdout = stdout_reader()
    help(list.append)
    x = sys.stdout.text
    sys.stdout = sys.__stdout__
    
    Assert(x.find('append(...)') != -1)

def test_oldstyle_class():    
    class foo:
        """the slow lazy fox jumped over the quick brown dog..."""
                        
    sys.stdout = stdout_reader()
    help(foo)
    x = sys.stdout.text
    sys.stdout = sys.__stdout__
        
    Assert(x.find('the slow lazy fox jumped over the quick brown dog...') != -1)

def test_nodoc():
    sys.stdout = stdout_reader()
    class foo(object): pass
    
    help(foo)
    
    sys.stdout = sys.__stdout__

    sys.stdout = stdout_reader()
    class foo: pass
    
    help(foo)
    
    sys.stdout = sys.__stdout__

    sys.stdout = stdout_reader()
    def foo(): pass
    
    help(foo)
    
    sys.stdout = sys.__stdout__
    
def test_str():
    sys.stdout = stdout_reader()
    help('abs')
    x = sys.stdout.text
    sys.stdout = sys.__stdout__
    
    Assert(x.find('Return the absolute value of the argument.') != -1)
    
run_test(__name__)
