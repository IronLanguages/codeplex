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

import rulediff
from common import *

def run_cpython(test):
    return launch(cpython_executable, test)

def run_ipython(test):
    return launch(ipython_executable, test)

def get_sbs_tests():
    not_run_tests = [ 
        "sbs_typeop.py",
    ]
    import nt
    return [x[:-3] for x in nt.listdir(compat_test_path) 
                   if x.startswith("sbs_") and x.endswith(".py") and (x.lower() not in not_run_tests)]

success = failure = compfail = 0
   
def run_sbs_test(l):
    global success, failure
    create_new_file(get_summary_file())
    
    exceptions = []
    for test in l:
        try:
            print "Running", test, " ..."
            __import__(test)
            success += 1
        except Exception, e:
            print "   FAIL"
            print e
            failure += 1
            exceptions.append(e)
            
    print "----------------------------------------"
    if failure > 0 or len(exceptions) > 0:
        print " Simply Run:   !!! FAILED !!!"
    else:
        print " Simply Run:   !!! SUCCESS !!!"
    print "----------------------------------------"
    print " Tests ran: " + str(success + failure), " Success: " + str(success)  + " Failure:   " + str(failure)
    print "----------------------------------------"
    if len(exceptions) > 0:
        print "Exceptions:"
        for exception in exceptions:
            print exception
            print "----------------------------------------"

def compare_sbs_test_log():
    win_metafile = get_summary_file("win")
    cli_metafile = get_summary_file("cli")
    
    f = file(win_metafile, "r")
    win_logfiles = f.readlines()
    f.close()
    
    f = file(cli_metafile, "r")
    cli_logfiles = f.readlines()
    f.close()
    
    win_dict = {}
    for x in win_logfiles:
        win_dict[x[4:-5]] = x[:-1]
    
    cli_dict = {}
    for x in cli_logfiles:
        cli_dict[x[4:-5]] = x[:-1]
    
    missings = []
    global compfail
    
    for k in win_dict.keys():
        if cli_dict.has_key(k) == False: 
            missings.append(k)
            continue
    
        cli_log = cli_dict[k]
        win_log = win_dict[k]
        diff_logname = "diff_" + k + ".log"
        
        diff_logfile = open(fullpath(diff_logname),"w+")

        print "Comparing", k, "...", 
        
        (val, summary) = rulediff.compare(fullpath(win_log), fullpath(cli_log), diff_logfile)
        if val == result_pass:
            print "   PASS "
            #print "       windiff %s %s" % (win_log, cli_log)
        else: 
            compfail += 1
            print "   FAIL [%s]" % summary
            print "         windiff %s %s" % (win_log, cli_log)
            print "         notepad %s " % diff_logname
            
        diff_logfile.close()

    print "----------------------------------------"
    if compfail > 0 or len(missings) > 0:
        print " Comparison:   !!! FAILED !!!"
    else:
        print " Comparison:   !!! SUCCESS !!!"
    print "----------------------------------------"
    print " Failure:   " + str(compfail)
    print "----------------------------------------"
    if len(missings) > 0:
        print "Missing:"
        for miss in missings:
            print miss
        print "----------------------------------------"            

def run(type="long", tests = "full", compare=True):
    if type in ["short", "medium"]:
        return 1

    ensure_future_present()
    
    print "*** generated result logs/scripts @", compat_test_path
    if tests == "full": 
        tests = get_sbs_tests()
    
    run_sbs_test(tests)

    if compare:
        compare_sbs_test_log()

    if failure == 0 and compfail == 0: 
        return 1
    else :
        return 0
  
if __name__ == "__main__":
    args = sys.argv
    
    bCompare = sys.platform == "cli"
    if len(args) == 1 :
        run(compare = bCompare)
    else:
        run(tests = [ x[:-3] for x in args[1:] ], compare = bCompare)
        
