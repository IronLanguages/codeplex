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
import nt

test_script_path = sys.prefix + "/Src/Scripts/Test"
test_temporary_path = None

def print_usage():
    print """
    runtests [parrot|parrotbench] [ip|iron] [pystone|py] [regr|regress] [lib|library]
             [compat] [all] [mini] [short|long|(medium|med)] [quiet|verbose]
             [slowpaths(+)] [snippets(+)] [fasteval(+)] [saveasm(+)] [frames(+)] [maxrecursion(+)] [mixed]

     parrot,parrotbench - run Parrotbench
     ip,iron            - run IronPython regression tests
     pystone            - run Pystone
     regr,regress       - run supported subset of CPython's regression suite
     lib, library       - run the Python library baseline test
     compat             - run CPython compatability test
     all                - run all of the above
     mini               - run parrot, pystone and ip
     short              - run short version of the selected test suites
     medium,med         - run medium version of the selected test suites
     long               - run long version of the selected test suites
     full               - run full version of the selected test suites
     cgcheck            - verify code gen is not out of sync
                        - full version runs all tests at one iteration each

     Extension styles:

     slowpaths(+):      - hit all the slow paths (equivalent to snippets & maxrecursion)
     snippets(+):       - only import modules as snippets (console input emulation)
     saveasm(+):        - save assembly
     fasteval(+):       - run with fast eval enabled
     frames(+):         - run code generating custom frames
     maxrecursion(+):   - enable maximum recursion checks
     mixed:             - run all combinations
                        - When "+" is present after a switch, one test run with that switch enabled happens; 
                        - If not, two test runs with the switch enabled/disabled will occur
                        - When multiple switchs present, run with a combination of such switch settings.
                    
                        - Default extension: saveasm+ slowpaths
                        - which means one run with -X:SaveAssemblies -X:GenerateAsSnippets -X:MaxRecursion 1001, 
                          and another one with -X:SaveAssemblies only.

    Tracing:

    verbose             - print verbose traces (default)
    quiet               - supress much of the traces

    Parrotbench:
        long:          4 rounds of all tests
        medium:        2 rounds of all tests
        short/full:    1 round of all tests
    Pystone:
        long:         1000000 loops
        medium:        250000 loops
        short/full:     50000 loops
    IronPython regression suite:
        long/full:     run all
        medium:        run all except Arithmetics and IndiceTest
        short:         run all except Arithmetics, IndiceTest and Delegates
    CPython regression suite:
        long/full:     run all
        medium:        run all
        short:         skip
    Library suite:
        long/full:     run all
        medium:        skip
        short:         skip
     
    """

def launch(file, parm, style, quiet):
    try:
        params = ("-O", "-D")
        params += tuple(style)
        params += (file, parm)
        if quiet:
            params += ("quiet",)
        print params
        return nt.spawnl(0, sys.executable, *params)
    except:
        return 1

def exec_test(test, parm, style=[], quiet=False):
    testfile = test_script_path + test + ".py"
    print testfile
    result = launch(testfile, parm, style, quiet)
    return ("%8s" % test) + str([x for x in style if x in ["-X:SaveAssemblies", "-X:GenerateAsSnippets"]]), result

def execute_tests_with_style(tests, parm, style, quiet):
    result = []
    for x in tests:
        result.append(exec_test(x, parm, style, quiet))
    return result

def execute_tests(tests, parm, styles, quiet):
    result = []
    for style in styles:
        print "\n********** START RUNNING IRONPYTHON UNDER", str(style), "**********"
        result.extend(execute_tests_with_style(tests, parm, style, quiet))
    return result
    
def find_peverify():
    if sys.platform != 'cli':
        return None
    import System
    path = System.Environment.GetEnvironmentVariable("PATH")
    dirs = path.split(';')
    for dir in dirs:
        file = System.IO.Path.Combine(dir, "peverify.exe")
        if System.IO.File.Exists(file):
            return file

    print """
#################################################
#     peverify.exe not found. Test will fail.   #
#################################################
"""
    return None

def set_tempdir():
    import nt
    
    root = nt.environ.get("TEMP")
    if root == None: root = nt.environ.get("TMP")
    if (root == None) or (' ' in root) : root = r"C:\temp"

    global test_temporary_path
    test_temporary_path = root + "/IronPython"
    
    try: nt.mkdir(test_temporary_path) 
    except: pass

def clean_tempdir():
    import nt
    for f in nt.listdir(test_temporary_path):
        try :   nt.remove(test_temporary_path + "/" + f)
        except: pass
    
def main():
    arguments = sys.argv
    thescript = arguments.pop(0)
    exitcode = 0
    
    set_tempdir()
    
    run_set = set()
    run_parm = ""
    
    ext_map = { 
        "snippets"   : "-X:GenerateAsSnippets", 
        "slowpaths"  : ["-X:GenerateAsSnippets", "-X:NoOptimize", "-X:MaxRecursion",  "1001"],
        "saveasm"    : ["-X:SaveAssemblies", "-X:AssembliesDir", test_temporary_path], 
        "fasteval"   : "-X:FastEval",
        "frames"     : "-X:Frames",
    }
    ext_keys = ext_map.keys();
    run_ext = {}
    run_mixed = 0
    run_quiet = False
    
    for arg in arguments:
        arg = arg.lower()

        if arg in ["ip", "iron", "ironpython"]:     run_set.add("iron");
        elif arg in ["parrot", "parrotbench"]:      run_set.add("parrot")
        elif arg in ["pystone", "py"]:              run_set.add("pystone")
        elif arg in ["regr", "regress"]:            run_set.add("regress")
        elif arg in ["lib", "library"]:             run_set.add("library")
        elif arg == "cgcheck":                      run_set.add("cgcheck")
        elif arg == "compat":                       run_set.add("compat")
        elif arg == "all":                          run_set = set(["iron", "parrot", "pystone", "regress","library", "cgcheck", "compat"])
        elif arg == "mini":                         run_set = set(["iron", "parrot", "pystone"])
        elif arg == "long":                         run_parm = "long"
        elif arg in ["med", "medium"]:              run_parm = "medium"
        elif arg == "short":                        run_parm = "short"
        elif arg == "full":                         run_parm = "full"
            
        elif arg in [k + "-" for k in ext_keys]:    run_ext[arg[0:-1]] = 0
        elif arg in [k + "+" for k in ext_keys]:    run_ext[arg[0:-1]] = 1
        elif arg in ext_keys:                       run_ext[arg] = 2
        
        elif arg == "mixed":                        run_mixed = 1
        
        elif arg == "verbose" :                     run_quiet = False
        elif arg == "quiet":                        run_quiet = True
        
        elif arg in ["-?", "?", "/?", "-h", "--help"]:
            return print_usage()
        else:
            raise ValueError("Invalid argument: %s" % arg)

    if len(run_set) == 0:       run_set = set(["iron", "parrot", "pystone", "regress","library", "cgcheck", "compat"])
    if run_parm == "":          run_parm = "full"

    # Default run scenarios (1 applies to all scenarios, 2 creates a new style based upon things that apply to all)
    if len(run_ext) == 0:       run_ext = {"saveasm" : 1, "slowpaths" : 2}
    if run_mixed == 1:          run_ext = {"saveasm" : 2, "slowpaths" : 2, "fasteval":2 }
   
    run_styles = [[]]
    for key, ext in run_ext.iteritems():
        adding = ext_map[key]
        if ext == 1:
            for style in run_styles:                
                if not isinstance(adding, list): style.append(adding)
                else: style.extend(adding)

        elif ext == 2:
            run_styles_clone = []
            for style in run_styles:
                run_styles_clone.append(style[:])
        
            for style in run_styles:
                if not isinstance(adding, list): style.append(adding)
                else: style.extend(adding)

            run_styles.extend(run_styles_clone)
    
    # verify presence of peverify.exe
    peverify_exe = find_peverify()

    # first verify if we were to fail we'd get a good exit code back.
    exitRes = exec_test('ExitCode', run_parm, quiet = run_quiet)
    if exitRes[1] != 1:
        print 'ExitCode test failed, cannot run tests'
        sys.exit(1)      # apparently we won't propagate the exit code, but at least hopefully someone will notice tests aren't running!
        
    # run compat 
    if "compat" in run_set:
        from TestAll import cpythonrun
        cpythonrun(run_parm, True)

    # normal test runs
    result = execute_tests(run_set, run_parm, run_styles, run_quiet)

    print "**********************************"

    if not peverify_exe:
        print "peverify.exe not found: FAIL"
        exitcode = 1

    for res in result:
        print res[0],
        if res[1]:
            print "!!! FAIL !!!"
            exitcode = 1
        else:
            print "... PASS ..."
    print "**********************************"
    print "FINAL RESULT: ",
    if exitcode == 0:
        print "PASS"
    else:
        print "FAIL"
    print "**********************************"
    
    clean_tempdir()
    
    sys.exit(exitcode)

if __name__=="__main__":
    main()
