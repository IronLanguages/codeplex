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

import nt
import sys
from Util.Debug import *

medium_tests = [
    ]

long_tests = [
    "arithmetics",
    "indicetest",
    "evalleaks",
    "compiler",
    "codedom",
    ]

do_not_run_list = [
    "delegates",
    "irontestsuite",
    "runall",
    "util",
    "pretest",
    "__future__",
    "temp_future", # in case it was left behind
    "custombuiltins",
    "interop", #BUG 465
    ]


def run_the_test(type, test):
    test = test.lower()
    if test in do_not_run_list: return False
    if type == "short":
        if test in medium_tests: return False
        if test in long_tests: return False
    elif type == "medium":
        if test in long_tests: return False
    elif type == "full": return True
    return True


def get_test_list(type):
    path = iron_python_root + iron_python_tests
    files = nt.listdir(path)
    tests = []
    for test in files:
        if test[-3:] == ".py":
            test = test[:-3]
            if run_the_test(type, test):
                tests.append(test)
        elif test[-4:] in [".exe", ".dll", ".pdb", ".pyc", ".cs"]:
            continue
        else:
            try:
                if "__init__.py" in nt.listdir(get_subdir(path,test)) and run_the_test(type, test):
                    tests.append(test)
            except:
                pass
    return tests

def run(type="short"):
    if not type in ["short", "medium", "long", "full"]:
        raise AssertionError("Unknown test type %s" % type)

    success = 0
    failure = 0
    exceptions = list()
    
    text_to_file("division = 1", iron_python_root + iron_python_tests + "/__future__.py")

    for test in get_test_list(type):
        try:
            print "Running " + test + " ...",
            __import__(test)
            print "   PASS"
            success += 1
        except Exception, e:
            print "   FAIL"
            print e
            failure += 1
            exceptions.append(e)

    print "----------------------------------------"
    if failure > 0 or len(exceptions) > 0:
        print " Test summary:   !!! FAILED !!!"
    else:
        print " Test summary:   !!! SUCCESS !!!"
    print "----------------------------------------"
    print " Tests ran: " + str(success + failure)
    print " Success:   " + str(success)
    print " Failure:   " + str(failure)
    print "----------------------------------------"
    if len(exceptions) > 0:
        print "Exceptions:"
        for exception in exceptions:
            print exception
            print "----------------------------------------"

    if failure == 0 and len(exceptions) == 0:
        return 1
    else:
        return 0

def main():
    result = run()
    if not result:
        sys.exit("!!! FAILED !!!")

if __name__ == "__main__":
    main()
