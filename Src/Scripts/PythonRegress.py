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

#regression tests enabled for IronPython

regression_tests = [
    "test.test_atexit",
    "test.test_augassign",
    "test.test_binop",
    "test.test_bufio",
    "test.test_call",
    "test.test_calendar",
    "test.test_colorsys",
    "test.test_contains",
    "test.test_compare",
    "test.test_dict",
    "test.test_dircache",
    "test.test_dummy_thread",
    "test.test_dummy_threading",
    "test.test_enumerate",
    "test.test_errno",
    "test.test_filecmp",
    "test.test_fileinput",
    "test.test_fnmatch",
    "test.test_fpformat",
    "test.test_format",
    "test.test_grammar",
    "test.test_hexoct",
    "test.test_htmllib",
    "test.test_list",
    "test.test_macpath",
    "test.test_math",
    "test.test_ntpath",
    "test.test_operations",
    "test.test_operator",
    "test.test_opcodes",
    "test.test_pep263",
    "test.test_pkg",
    "test.test_pkgimport",
    "test.test_popen",
    "test.test_popen2",
    "test.test_queue",
    "test.test_rfc822",
    "test.test_urlparse",
    "test.test_sgmllib",
    "test.test_shlex",
    "test.test_slice",
    "test.test_string",
    "test.test_struct",
    "test.test_textwrap",
    "test.test_thread",
    "test.test_threading",
    "test.test_time",
    "test.test_types",
    "test.test_unary",
    "test.test_univnewlines",
    "test.test_userdict",
    "test.test_userstring",
    "test.test_warnings",
    "test.test_xrange",  
    
    #"test.test_site",

    # tests with changes beyond implementation details
    "test.test_bisect",     # doctest support
    "test.test_bool",       # 4 scenarios disabled due to pickle
    "test.test_codecs",     # Pyunycode, Nameprep, and idna not implemented, need to manually import encodings
    "test.test_complex",    # BUG# 980
    "test.test_decimal",    # Bugs 972, 975, 973; another 2 cases disabled due to pickle
    "test.test_decorators", # BUG# 976
    "test.test_deque",      # weakref, pickle, itertools not implemented
    "test.test_eof",        # tests for the whole exception string verbatim, changed to test for substring
    "test.test_exceptions", # warnings module
    "test.test_iter",       # BUG# 908
    "test.test_itertools",
    "test.test_long",       # subclass long, test_float_overflow(), test_logs(), test_mixed_compares()
    "test.test_marshal",    # code not implemented, file() operations need to be explicitly closed
    "test.test_repr",       # repr for array module commentted out
    "test.test_richcmp",    # VectorTest disabled (due to __cast?), Also "False is False" == False(rarely)
    "test.test_pow",        # BUG# 884
    "test.test_scope",      # Bugs 961, 962
    "test.test_set",        # weakref, itertools, and pickling not supported
    "test.test_sort",       # finalizer (__del__)
    "test.test_str",        # formatting disabled in string_tests, need to import encodings manually
    "test.test_stringio",   # IP doesn't support buffer, iter(StringIO()) is wrapped IEnumerator
    "test.test_syntax",     # BUG# 971
    "test.test_traceback",  # generates files aren't collected, need to close manually
    "test.test_weakref",    # various tests disabled due to collection not being eager enough, additional gc.collect calls
    "test.test_builtin",    # various tests disabled - locals(), dir(), unicode strings
]

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

def write(s,t):
    try:
        s.write(t)
    except:
        pass

def run_one_test(test, summary):
    summary.write("======================================================================\n")
    summary.write("  Running %s\n" % test)
    package = __import__(test)
    module = getattr(package, test.split('.')[-1])
    if hasattr(module, 'test_main'):
        summary.write("     test_main()\n")
        getattr(module, 'test_main')()
    summary.write("  DONE\n")

def run_regression_tests(summary):
    success = 0
    failure = 0
    exceptions = []
    results = []

    for test in regression_tests:
        try:
            run_one_test(test, summary)
            success += 1
            results.append(" PASS  " + test + "\n")
        except Exception, e:
            write(summary, str(e) + "\n")
            exceptions.append(e)
            failure += 1
            results.append("*FAIL  " + test + "\n")
        except object, o:
            write(summary, str(o) + "\n")
            exceptions.append(AssertionError("test failed %s" % test))
            failure += 1
            results.append("+ERROR " + test + "\n")

    for r in results:
        write(summary, r)

    if failure:
        raise AssertionError("Python Regression Test failed")

def regress(test="long", quiet=False):
    if not test in ["medium", "long", "full"]:
        return

    old_stdout = sys.stdout
    old_stdout_ = sys.__stdout__
    old_stderr = sys.stderr
    old_stderr_ = sys.__stderr__

    if quiet:
        print "Redirecting output"
        sys.stdout = sys.stderr = sys.__stdout__ = sys.__stderr__ = nullstream()

    try:
        run_regression_tests(old_stdout)
    finally:
        sys.stdout = old_stdout
        sys.__stdout__ = old_stdout_
        sys.stderr = old_stderr
        sys.__stderr__ = old_stderr_

def main(test="long"):
    if test in ["medium", "long", "full"]:
        run_regression_tests()

if __name__ == "__main__":
    main()
