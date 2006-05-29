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

# test results + tracking

test_success = 1
test_failure = 0
test_skipped = -1

class testresult:
    def __init__(self, name):
        self.name = name
        self.result = test_failure

# relative paths into the public subtree

bin_path = "/Bin"
lib_path = "/Bin/Lib"
tests_path = "/Src/Scripts/Tests"
compat_path = "/Src/Scripts/Tests/compat"
script_path = "/Public/Src/Scripts"

# relative paths into the private subtree

iron_python_public_root = sys.prefix
private_parrot_path = "/External/parrotbench"
private_regress_test_path = "/External/Regress/Python24/Lib/test"
private_regress_lib_path = "/External/Regress/Python24/Lib"

iron_paths = [tests_path]
parrot_paths  = [script_path, private_parrot_path]
regress_paths = [private_regress_lib_path, script_path]
pystone_paths = [private_regress_test_path]

def testit(c, m="Test Failed"):
    if not c:
        raise AssertionError(m)

def initialize_test(root, path):
    newpath = [root + bin_path, root + lib_path]

    for d in path:
        newpath.append(root + d)

    sys.path[:] = newpath

def iron_python_private_root():
    if sys.prefix.lower().endswith("\\public") or sys.prefix.lower().endswith("/public"):
        return sys.prefix[:-7]
    else:
        raise AssertionError("invalid root " + sys.prefix)

def initialize_public_test(path):
    initialize_test(iron_python_public_root, path)

def initialize_private_test(path):
    try:
        initialize_test(iron_python_private_root(), path)
        return test_success
    except AssertionError:
        return test_skipped

def get_testparams():
    testtype = None
    quiet = False
    for arg in sys.argv[1:]:
        arg = arg.lower()
        if arg in ["long", "medium", "full", "short"]:
            testtype = arg
        elif arg == "quiet":
            quiet = True
        elif arg == "verbose":
            quiet = False
    if testtype == None or len(testtype) == 0:
        testtype = "long"
    return testtype, quiet

def get_testmode():
    import System
    lastConsumesNext = False
    switches = []
    for x in System.Environment.GetCommandLineArgs():
        if x.startswith("-"):
            switches.append(x)
            if x == "-X:Optimize" or x == "-W" or x == "-c" or x == "-X:MaxRecursion":
                lastConsumesNext = True
        else:
            if lastConsumesNext:
                 switches.append(x)   
            lastConsumesNext = False
    return str(switches)

def cpythonrun(test, quiet):
    if test in ["short", "medium"]:
        return 1

    initialize_public_test([tests_path, compat_path])
    print "********** START RUNNING CPYTHON ON APPLICABLE TESTS **********"
    import runsbs
    # 1) Scripts\Tests 
    #runsbs.run_cpython(compat_test_path + "runiron.py")
    
    # 2) Scripts\Tests\Compat
    print runsbs.compat_test_path + "runsbs.py"
    runsbs.run_cpython(runsbs.compat_test_path + "runsbs.py")

# public test suites
def ironpython(test, quiet):
    initialize_public_test(iron_paths)
    import irontestsuite
    return irontestsuite.run(test)

def ironpythoncompat(test, quiet):
    initialize_public_test([compat_path])
    import runsbs
    return runsbs.run(test)

# private test suites
def parrotbench(test, quiet):
    res = initialize_private_test(parrot_paths)
    if res != test_success: return res
    import parrotrun
    parrotrun.main(test)
    return test_success

def pystone(test, quiet):
    res = initialize_private_test(pystone_paths)
    if res != test_success: return res
    import pystone
    loops = { "full": 50000, "short" : 50000, "medium" : 250000, "long" : 1000000 }[test]
    benchtime, stones = pystone.pystones(loops)
    print "Pystone(%s) time for %d passes = %g" % \
        (pystone.__version__, loops, benchtime)
    print "This machine benchmarks at %g pystones/second" % stones
    return test_success

def pythonregress(test, quiet):
    res = initialize_private_test(regress_paths)
    if res != test_success: return res
    import PythonRegress
    PythonRegress.regress(test, quiet)
    return test_success

def pythonlibrary(test, quiet):
    res = initialize_private_test(regress_paths)
    if res != test_success: return res
    import PythonStdLib
    PythonStdLib.regress(test)
    return test_success

def exectest(test, testtype, testmode, quiet):
    try:
        return test(testtype, quiet)
    except Exception, e:
        print e
        print e.clsException.StackTrace
        return test_failure
#    except:
#        print "Unhandled exception"
#        return test_failure
    return test_failure

def runtest(test, msg):
    testtype, quiet = get_testparams()
    testmode = get_testmode()
    print "======================================================="
    print "Testing " + msg + " suite " + testtype + " in mode "+ testmode

    res = exectest(test, testtype, testmode, quiet)

    print "======================================================="
    if res == test_success:
        print msg + " !!! PASS !!!"
    elif res == test_failure:
        print msg + " !!! FAIL !!!"
    elif res == test_skipped:
        print msg + " !!! SKIP !!!"
    print "======================================================="
    return res

def runtestexit(test, msg):
    res = runtest(test,msg)
    if res == test_failure:
        sys.exit(1)

def runtesttrack(results, test, msg):
    result = testresult(msg)
    result.result = runtest(test, msg)
    results.append(result)

def main():
    results = []
    runtesttrack(results, parrotbench, "ParrotBench")
    runtesttrack(results, pystone, "Pystone")
    runtesttrack(results, ironpython, "IronPython")
    runtesttrack(results, pythonregress, "Python Regression")
    runtesttrack(results, pythonlibrary, "Python Library")

    print "*******************************************************"

    success = True
    for result in results:
        print result.name,
        if result.result == test_success:
            print " PASS"
        elif result.result == test_failure:
            print " FAIL"
            success = False
        elif result.result == test_skipped:
            print " SKIP"
        else:
            print result.result

    print "*******************************************************"
    if success:
        print "ALL TESTS PASS"
    else:
        print "TEST FAILED"
    print "*******************************************************"
    if not success:
        sys.exit(1)

if __name__ == "__main__":
    main()
