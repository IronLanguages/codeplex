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

log_relative_path = "/Public/Src/Scripts"
test_relative_path = "/External/Regress/Python24/Lib"

log_name = "PythonStdLib.log"
base_name = "PythonStdLibBase.log"
exc_name = "PythonStdLibExceptions.log"

donotrun=["reconvert", "regsub"]

def testit(c, m="Test Failed"):
    if not c:
        raise AssertionError(m)

def get_root():
    root = sys.prefix
    for slash in ["/", "\\"]:
        if root.lower().endswith(slash + "public"):
            return root[:-len("public") - 1]
    raise AssertionError("Invalid root sys.prefix: %s" % sys.prefix)

def get_path(rel):
    return get_root() + rel

class tr:
    def __init__(self, m):
        self.m = m
        self.r = None
        self.e = None

def writeboth(a,b,s):
    a.write(s)
    b.write(s)

def error_message(e):
    message = str(e)
    if message == None:
        return ""
    lines = message.split("\n")
    return lines[0]

def runtests(details, summary):
    import os

    success = 0
    failure = 0
    exceptions = list()

    logpath = get_path(log_relative_path)
    location = get_path(test_relative_path)
    all_tests = os.listdir(location)
    print all_tests
    for test in all_tests:
        if test[-3:] == ".py":
            testmodule = test[:-3]
            if not (testmodule.lower() in donotrun):
                result = tr(testmodule)
                try:
                    details.write("Running " + test + " ...")
                    mod = __import__(testmodule)
                    details.write("   PASS\n")
                    summary.write(".")
                    success += 1
                    result.r = True
                except Exception, e:
                    details.write("   FAIL\n")
                    failure += 1
                    result.r = False
                    result.e = e
                    summary.write("x")
                except object, o:
                    failure += 1
                    details.write("   FAIL\n")
                    summary.write("x")
                    result.r = False
                    result.e = o
                exceptions.append(result)

    if len(exceptions) > 0:
        e = file(logpath + "\\" + log_name, "w")
        e.write("Exceptions:")
        for r in exceptions:
            e.write("\n----------------------------------------\n")
            e.write(r.m)
            e.write(": ")
            if r.r:
                e.write("PASS")
            else:
                e.write("FAIL: ")
                e.write(error_message(r.e))
        e.close()

        e = file(logpath + "\\" + exc_name, "w")
        e.write("Exceptions:")
        for r in exceptions:
            if not r.r:
                e.write("\n----------------------------------------\n")
                e.write(r.m)
                e.write(": ")
                e.write("FAIL: \n")
                e.write(str(r.e))
                e.write("\n")
        e.close()

    base = file(logpath + "\\" + base_name)
    this = file(logpath + "\\" + log_name)

    baseline = True
    curLine = 1
    while 1:
        base_l = base.readline()
        this_l = this.readline()
        if base_l != this_l:
            baseline = False
            summary.write("\nDiffers on line " + str(curLine) + "\n")
            summary.write("Old: " + base_l)
            summary.write("New: " + this_l)

        if base_l == "" or this_l == "":
            break

        curLine = curLine + 1

    base.close()
    this.close()

    summary.write("\n")
    writeboth(details, summary, "----------------------------------------\n")
    if not baseline:
        writeboth(details, summary, " Test summary:   !!! FAILED !!!\n")
    else:
        writeboth(details, summary, " Test summary:   !!! SUCCESS !!!\n")
    writeboth(details, summary, "----------------------------------------\n")
    details.write(" Tests ran: " + str(success + failure) + "\n")
    details.write(" Success:   " + str(success) + "\n")
    details.write(" Failure:   " + str(failure) + "\n")
    details.write("----------------------------------------\n")

    return baseline


class nullstream:
    softspace = False
    def close(self):
        pass
    def flush(self):
        pass
    def fileno(self):
        return 1
    def isatty(self):
        return False
    def read(self):
        return ""
    def readline(self):
        return "\n"
    def write(self, s):
        pass

def regress(test="long"):
    if not test in ["long", "full"]:
        return
    old_stdout = sys.stdout
    sys.stdout = nullstream()
    old_stderr = sys.stderr
    sys.stderr = nullstream()
    result = False
    try:
        result = runtests(nullstream(), old_stdout)
    finally:
        sys.stdout = old_stdout
        sys.stderr = old_stderr
    testit(result)

def main():
    old_stderr = sys.stderr
    sys.stderr = nullstream()
    try:
        runtests(sys.stdout, nullstream())
    finally:
        sys.stderr = old_stderr

if __name__ == "__main__":
    sys.path.append(get_path(test_relative_path))
    main()
