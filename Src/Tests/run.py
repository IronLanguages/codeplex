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

'''
This is mainly a wrapper to run iprun.py and IronPython console with
different extension switchs. Two set of switches are run by default.
You can specify the extension mode as shown below:

  -M option (Extension mode)
    -M:1 : launch ip with "-O -D -X:GenerateAsSnippets -X:NoOptimize -X:MaxRecursion 1001"
    -M:2 : launch ip with "-O -D -X:SaveAssemblies"

    -M:A : launch ip with different combinations (tbi)
    -M:D : launch ip twice with -M:1 and -M:2 
           this is default choice if -M: not specified

    -M:"-O -D -X:SaveAssemblies" (use DOUBLE QUOTE)

The following arguments are passed to iprun.py directly:

  -O option (to specify output detail)
    -O:min : try to keep the output in one screen; 
             '.' for pass, 'x' for fail (followed by test name)
    -O:med : show 'PASS' and 'FAIL' for each test
    -O:max : beside showing 'PASS' and 'FAIL' for each test, 
             print the exception message at the end

  -T option (to specify time related options)
    -T:min : 
    -T:med : this is default
    -T:max : 
    
  -h or -? : to show this help message

Other arguments without leading '-' will be considered as categories, 
Use categories.py to explore more category info.

To leverage your dual-proc machine, you may open two IP windows, and type
    run a1 (which run the set of iron, misc, lib, regress test)
    run a2 (which run the set of compat test)
  when you need run all tests, or
 
    run b1 (which run the set of lib, regress test)
    run b2 (which run the set of iron, misc test)
  when you need run all tests except the compat test
        
'''

import nt
import sys
import re

from lib.assert_util import * 
from lib.file_util import *
from lib.process_util import *

class ModeInfo:
    def __init__(self, ipArgs, testArgs):
        self.ipArgs = ipArgs
        self.testArgs = testArgs
    def __str__(self):
        return self.ipArgs
        
mode_mapping = { 
    '1': ModeInfo('-O -D -X:GenerateAsSnippets -X:NoOptimize -X:MaxRecursion 1001', ['Compat-']),
    '2': ModeInfo('-O -D -X:SaveAssemblies -X:AssembliesDir %s' % testpath.temporary_dir, []),
}

def get_mode_list(modes):
    default_modes = [ mode_mapping.get('1'), mode_mapping.get('2') ]
    if modes:
        if len(modes) <> 1 :
            usage(1, 'found more than 1 -M options')
        if 'D' in modes: return default_modes
        if 'A' in modes: 
            # all combinations
            raise AssertionError("Not implemented")
        if mode_mapping.__contains__(modes[0]):
            return [ mode_mapping.get(modes[0]) ]
        else:
            return [ modes[0] ]
    else:
        return default_modes

# used to run tests in dual-proc machine
shortcuts = {
    'a1': 'iron misc regress library',
    'a2': 'compat',
    'b1': 'iron ipcp',
    'b2': 'regress library misc',
}

def test_exit_code():
    # first verify if we were to fail we'd get a good exit code back.
    exitcode_py = 'exitcode.py'
    write_to_file(exitcode_py, 'import sys\nsys.exit(99)\n')
    exitRes = launch_ironpython(exitcode_py)
    if exitRes != 99:
        print '!!! sys.exit test failed, cannot run tests'
        # apparently we won't propagate the exit code, but at least hopefully someone will notice tests aren't running!
        sys.exit(1)    

def clean_log_files():
    # clean log files if i am the only one running ipy.exe
    try:
        ipys = [x for x in nt.popen('tasklist.exe').readlines() if x.lower().startswith('ipy.exe')]
        if len(ipys) == 1:
            class matcher:
                def __init__(self):
                    self.pattern = re.compile("result.*\.log")
                def __call__(self, value):
                    result = re.match(self.pattern, value)
                    return result and result.endpos == len(value)
            f = nt.listdir(".")
            f = filter(matcher(), f)
            delete_files(*f)

    except: pass

def main(args):
    # run options check
    if [x for x in args if x.lower() == '-h' or x == '-?']: 
        usage(1)
    
    for x in args:
        x2 = x.upper()
        if x2.startswith('-'): 
            if not x2.startswith('-M:') and not x2.startswith('-O:') and not x2.startswith('-T:'):
                usage(1, 'unknown option: %s' % x)

    test_exit_code()
    
    clean_log_files()
    
    # shortcuts
    tests = [x.lower() for x in args if not x.startswith('-')]
    tests2 = None
    for x in shortcuts.keys():
        if x in tests: 
            tests2 = shortcuts[x].split()
            break
    if tests2: tests = tests2
        
    # run compat test under cpython
    if (not tests) or ('compat' in tests):
        print "\nRUNNING CPYTHON ON COMPATABILITY TESTS"
        launch_cpython(path_combine(testpath.compat_testdir, 'runsbs.py'))
        
    # find iprun.py
    iprunfile = path_combine(get_directory_name(fullpath(sys.argv[0])), 'iprun.py')
    
    # run ironpython tests with cpython (we don't run the netinterop or hosting categories)
    # command: cpy iprun.py -O:max builtinfuncs builtintypes standard modules stress
    results = []
    sumretval = 0
    if (not tests) or ('ipcp' in tests):
        print "\nRUNNING CPYTHON ON IRONPYTHON TESTS"
        sumretval += launch_cpython(iprunfile, '-O:min', 'builtinfuncs', 'builtintypes', 'standard', 'modules', 'stress')
    
    # other switches will be carried on to iprun.py
    carryon = [x for x in args if not x.startswith('-M:') and x.lower() not in shortcuts.keys() ]
    for x in tests: 
        if x not in carryon : carryon.append(x)

    # launch iprun.py with different modes
    rawModes = [ x[3:] for x in args if x.startswith('-M:') ]
    for style in get_mode_list(rawModes):
        sstyle = str(style)
        print "\nRUNNING IRONPYTHON UNDER", sstyle
        if style.testArgs: print 'With test options ', style.testArgs
        retval = launch_ironpython_with_extensions(iprunfile, sstyle.split(), carryon + style.testArgs)
        sumretval += retval
        results.append((sstyle, retval))
    
    # clean generated .exe/.pdb in SaveAssemblies extension mode
    clean_directory(testpath.temporary_dir)
    
    # print results
    print formatter.SeparatorStar
    for (x, y) in results:
        print (y and "*FAIL*" or " PASS "), '[', x, ']'
    print formatter.SeparatorStar

    # peverify is used inside IronPython. It skips the peverify check on generated assemblies 
    # if peverify.exe could not be found, but as testing, we need make sure it in path.
    if not find_peverify(): sys.exit(1) 
 
    # exit
    sys.exit(sumretval)    

if __name__ == '__main__':
    main(sys.argv[1:])