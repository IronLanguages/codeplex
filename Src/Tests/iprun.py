#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

'''
IronPython Test Driver

  -O option (to specify output detail)
    -O:min : try to keep the output in one screen; 
             '.' for pass, 'x' for fail (with test name after)
    -O:med : show 'PASS' and 'FAIL' for each test
    -O:max : beside showing 'PASS' and 'FAIL' for each test, 
             print the exception message at the end

  -T option (to specify time related)
    -T:min : 
    -T:med :
    -T:max :
  
  other arguments without leading '-' will be taken as categories
  Category- can be used to exclude an entire category (e.g. Compat-)
'''

import sys
import nt
import time
import categories

from lib.assert_util import *
from lib.file_util import *
import clr

def my_format_exc():
    if directory_exists(testpath.lib_testdir):
        perserve_syspath()
        sys.path.append(testpath.lib_testdir)
        from traceback import format_exc
        restore_syspath()
        return format_exc()
    else: 
        return None

def getNextResultLog():
    import _random
    r = _random.Random()

    for x in xrange(1, 100):
        for y in xrange(10):
            fn = 'result_%i_%i.log' % (x, int(r.random() * 100))
            try:
                return fn, file(fn, 'w+')
            except:
                pass    # try next name

    raise AssertionError, 'cannot create log file'

## To control the output stream
#
#   - NullStream eats all, 
#   - MyFileStream flushes to file immediately (More consideration)
#

class NullStream:
    softspace = False
    def __init__(self): pass
    def __repr__(self): return ''
    def close(self):    pass
    def flush(self):    pass
    def fileno(self):   return 1
    def read(self):     return ""
    def readline(self): return "\n"
    def write(self, s): pass


class MyFileStream(NullStream):
    def __init__(self, sw): self.sw = sw
    def close(self):    self.sw.close()
    def write(self, s):
        self.sw.write(s)
        self.sw.flush()        

logname, logfile = getNextResultLog()
logstream = MyFileStream(logfile)
perflogname = "perf_%s.log" % time.strftime('%y%m%d_%H%M%S', time.localtime())
perflogfile = file(perflogname, 'w+')
print 'perf log saved as %s' % fullpath(perflogname)
perfstream =  MyFileStream(perflogfile)

assertOccurred = False

if is_cli: 
    from System.Diagnostics import Debug, TraceListener
    
    class MyTraceListener(TraceListener):
        def __init__(self, stream):
            self.stream = stream
            self.banner = '\n!!!' + 'X' * 70 + '!!!\n'
        def Write(self, message):
            self.stream.write(self.banner) 
            self.stream.write(message)
            self.stream.write(self.banner) 
            global assertOccurred
            assertOccurred = True
        def WriteLine(self, message):
            self.Write(message + r'\n')
        def Flush(self):
            self.stream.flush()
            
    Debug.Listeners.Clear()
    myListener = MyTraceListener(logstream)
    Debug.Listeners.Add(myListener)

# result related classes
class TestResultSet:
    startTime = time.ctime()
    endTime = None
    results = []
    TotalCnt = None
    FailCnt  = None

    @staticmethod
    def Finish():
        TestResultSet.endTime = time.ctime()
        TestResultSet.TotalCnt = len(TestResultSet.results)
        TestResultSet.FailCnt  = len([x for x in TestResultSet.results if not x.succeed])

    @staticmethod
    def Add(result): TestResultSet.results.append(result)

    @staticmethod
    def _tabOuput(s):
        s = str(s).strip()
        if s: 
            print 
            for x in s.split('\n'):
                print formatter.Space4, x

    @staticmethod
    def PrintDetail():
        if TestResultSet.FailCnt: print formatter.SeparatorEqual
        for x in TestResultSet.results:
            if not x.succeed: 
                print x.testname.ljust(formatter.TestNameLen), '|', x.exception[0]
                TestResultSet._tabOuput(x.stdout)
                TestResultSet._tabOuput(x.stderr)
                if x.exception[1]: 
                    print x.exception[1]
                print formatter.SeparatorMinus
                
    @staticmethod
    def PrintSummary(toprint = True):
        s = ''
        if TestResultSet.FailCnt: 
            s += (formatter.SeparatorStar +'\n')
            s += (("FAIL (%r out of %r)" % (TestResultSet.FailCnt, TestResultSet.TotalCnt)).center(formatter.Number) + '\n')
            s += (formatter.SeparatorStar +'\n')
            for x in TestResultSet.results:
                if not x.succeed:
                    s += ("%-30s (%s)\n" % (x.testname, x.testconfig.name))
        else: 
            s += (formatter.SeparatorStar +'\n')
            s += (("PASS (Total: %r)" % TestResultSet.TotalCnt).center(formatter.Number) + '\n')
            s += (formatter.SeparatorStar +'\n')
            
        if toprint: 
            print
            print s
        else:
            return s            
              
    @staticmethod
    def PrintReproString():
        return 
        print "To repro, try"
        print sys.executable, sys.argv[0], 
        for x in TestResultSet.results:
            if not x.succeed:
                print x.testname, 
                
    @staticmethod
    def SaveSummaryToFile():
        f = logstream
        f.write(formatter.SeparatorStar +'\n')
        f.write('Started  @ %s\n' % TestResultSet.startTime)
        f.write('Finished @ %s\n\n' % TestResultSet.endTime)
        f.write(TestResultSet.PrintSummary(False))
        
class TestResult:
    def __init__(self, name, testcfg): 
        self.testname = name
        self.testconfig = testcfg
        self.stdout = self.stderr = None
        
    def startTest(self):
        self.startTime = time.ctime()
        
    def _stopTest(self):
        self.stopTime = time.ctime()
        TestResultSet.Add(self)

    def setFailure(self, exc):
        self.succeed = False
        self.exception = exc
        self._stopTest()

    def setSuccess(self):
        self.succeed = True
        self.exception = None
        self._stopTest()
    
    def setAssertOccur(self):
        self.succeed = False
        self.exception = None
        self._stopTest()    
##
## How each file gets run
##

##  1. simply import
def ImportRunStep(file, timeLevel='med'): 
    __import__(file)
    del sys.modules[file]

##  2. expecting test_main, invoke it
def TestMainRunStep(file, timeLevel='med'):
    module = __import__(file)
    if hasattr(module, 'test_main'):
        getattr(module, 'test_main')(timeLevel)
    del sys.modules[file]

##  3. expecting test_main, but different pkg way
def RegressRunStep(file, timeLevel='med'):
    package = __import__(file)
    module = getattr(package, file.split('.')[-1])
    if hasattr(module, 'test_main'):
        getattr(module, 'test_main')()
    del sys.modules[file]

##  4. import the python module and compare any output with the expected behavior
##      - succeed or fail but with the expected exception message : pass
def LibraryRunStep(file, timeLevel='long'):
    errMsg = None
    try:
        package = __import__(file)
    except Exception, e:
        errMsg = str(e)
    except object, o:
        errMsg = str(o)
    
    if categories.LibraryExpectedFailures.has_key(file):  ## expected to fail
        expected = categories.LibraryExpectedFailures[file]
        if errMsg != expected: 
            raise AssertionError("different errMsg, expecting %s, actually %s"
                 % (expected, errMsg))
    else:  # expected to pass
        if errMsg: 
            raise AssertionError("expecting no exception, but got %s" 
                % errMsg)            

##  5. currently no difference from ImportRunStep, 
##     but i expect not to only show progress per file

def CompatRunStep(file, timelevel='med'):
    package = __import__(file)
    del sys.modules[file]


## Runners
## not used, but leave it for debugging purpose
class TestRunner:
    def __init__(self, tc):
        self.tc = tc
        
    def runTests(self):
        for x in sorted(self.tc.testList):
            self.runOneTest(x)
            
        if sys.modules.get('__future__'):
            del sys.modules['__future__']

    def runOneTest(self, test):
        try:
            sys.stdout.write(test.ljust(formatter.TestNameLen))
            self.testResult = TestResult(test, self.tc)
            self.testResult.startTest()
            self.runstep(test, self.timeLevel)
            print ' PASS '
            self.testResult.setSuccess()
        except Exception, e:  
            print '*FAIL*'
            self.testResult.setFailure((str(e.args), my_format_exc()))

class RedirectTestRunner(TestRunner):
    def _save_original(self):       self.saved_stdout, self.saved_stderr = sys.stdout, sys.stderr
    def _restore_original(self):    sys.stdout, sys.stderr = self.saved_stdout, self.saved_stderr
        
    def _redirect_output(self):
        sys.stdout = self.testResult.stdout = logstream
        sys.stderr = self.testResult.stderr = logstream

    def runOneTest(self, test):
        self.testResult = TestResult(test, self.tc)
        self._save_original()
        self._redirect_output()
        if self.detailLevel <> 'min':
            self.saved_stdout.write(test.ljust(formatter.TestNameLen))
        logstream.write('>>>> ' + test +'\n')
        perfstream.write('%s,' % test)
        perfStartTime = time.clock()
        try:
            self.testResult.startTest()
            self.runstep(test, self.timeLevel)
            
            logstream.write('\n') 
            global assertOccurred
            if assertOccurred: 
                self.testResult.setAssertOccur()
                assertOccurred = False

                if self.detailLevel == 'min':
                    self.saved_stdout.write("A(%s)" % test)
                else:
                    self.saved_stdout.write("!ASSERT!\n")
                perfstream.write('%s, assert\n' % (time.clock() - perfStartTime))
            else:
                self.testResult.setSuccess()
                if self.detailLevel == 'min':
                    self.saved_stdout.write(".")
                else:
                    self.saved_stdout.write(" PASS \n")
                perfstream.write('%s, pass\n' % (time.clock() - perfStartTime))
        except Exception, e: 
            logstream.write('\t\t*FAIL*\n')
            perfstream.write('%s, fail\n' % (time.clock() - perfStartTime))
            self.testResult.setFailure((str(e.args), my_format_exc()))
            print 'exception:', str(e.args)
            print 'traceback:', my_format_exc()
            print e
            print e.clsException
            if self.detailLevel == 'min':
                self.saved_stdout.write("x(%s)" % test)
            else:
                self.saved_stdout.write("*FAIL*\n")
        logstream.write(formatter.SeparatorMinus + '\n')
        self._restore_original()
    
# test configuration
class TestConfig: 
    def __init__(self):
        self.notRunList = [ 
        ]        
        self.name       = 'IronPythonTests'
        self.shortcut   = 'ip iron ironpython'
        self.directory  = testpath.public_testdir
        self.runstep    = ImportRunStep
        self.runner     = RedirectTestRunner
        self.categories = categories.IronPythonTests

    # some utility functions to rebuild itself as different things
    def rebuildString(self):
        space = "            "
        for k in sorted(self.categories.keys()): 
            print "%s'%s':" % (space, k)
            print "%s    '''" % space
            for x in sorted(self.categories[k].split()):
                print "%s    %s" % (space, x)
            print "%s    '''," % space
            
    def rebuildFiles(self):
        for v in self.categories.itervalues():
            for x in v.split():
                fp = path_combine(self.directory, x +".py")
                if not file_exists(fp): 
                    f = file(fp, "w")
                    f.close()
                    
    def rebuildSolution(self, dte):
        ''' To run this, open IronPython VS Console, and type
>>> import sys
>>> sys.path.append(this_file_path)
>>> import iprun
>>> tc = iprun.TestConfig()
>>> tc.rebuildSolution(dte)
        '''
        testProj = [x for x in dte.Solution.Projects if x.Name == "Tests"]
        if len(testProj) <> 1: 
            print "found", testProj
            return 
    
        tp = testProj[0]
        testProj = [pi for pi in tp.ProjectItems if pi.Name.lower() == "category" ]
        if len(testProj) <> 1:
            print "found", testProj
            return 

        tp = testProj[0]
        for (k, v) in self.categories.iteritems():        
            pis = [pi for pi in tp.SubProject.ProjectItems if pi.Name.lower() == k.lower()]
            if len(pis) == 1:
                folder = pis[0].SubProject                
            else: 
                folder = tp.ProjectItems.AddFolder(k)
            for x in v.split():
                folder.ProjectItems.AddFromFile(path_combine(self.directory, x + '.py'))
                                
    def str2list(self, s): 
        if isinstance(s, str):
            return s.split()
        else : 
            return s
        
    def _applicable(self, s):
        sl = s.lower()
        if sl[:-3] in self.notRunList: return False
        return sl.startswith("test_") and sl.endswith(".py")
        
    def getAllTests(self):
        l = []
        for filename in nt.listdir(self.directory):
            if not self._applicable(filename): continue
            l.append(filename[:-3])
        return l
        
    def getTests(self, *cats):
        tests = set()
        for x in cats:
            s = self.categories.get(x, '')
            for y in self.str2list(s):
                tests.add(y)
        tests = tests - set(self.notRunList)
        return list(tests)
        
    def getTestsShownInCategories(self): 
        tests = set()
        for x in self.categories:
            for y in self.str2list(self.categories[x]):
                if not y in self.notRunList:
                    tests.add(y)
        return list(tests)        

    def applicableTests(self, reqs=None):
        if not hasattr(self, 'testList'): 
            if not reqs: 
                self.testList = self.getAllTests()
            else:
                self.testList = [] 
                for x in reqs:
                    if not x.startswith("test_"):  self.testList.extend(self.getTests(x))
                self.testList.extend([x for x in reqs if x.startswith("test_")])
        return self.testList

class MathTestConfig(TestConfig):
    def __init__(self):
        self.notRunList = ['nztest.testFactor', ]
        self.name       = "Math"
        self.shortcut   = 'math nzmath'
        self.directory  = [testpath.math_testdir, testpath.lib_testdir,]
        self.runner     = RedirectTestRunner
        self.runstep    = RegressRunStep        
        self.categories = categories.MathTests
        self.Mode       = 1
        
    def getAllTests(self):
        return self.getTestsShownInCategories()

class MiscTestConfig(TestConfig):
    def __init__(self):
        self.notRunList = []
        self.name       = "Parrot/Pystone/CgCheck"
        self.shortcut   = 'misc'
        self.directory  = [testpath.lib_testdir, testpath.script_testdir, testpath.parrot_testdir, testpath.private_testdir]
        self.runner     = RedirectTestRunner
        self.runstep    = TestMainRunStep        
        self.categories = categories.MiscTests
        
    def getAllTests(self):
        return self.getTestsShownInCategories()

class LibraryTestConfig(TestConfig):
    def __init__(self):
        self.notRunList = [ ]
        self.name       = "Library"
        self.shortcut   = 'lib library'
        self.directory  = [testpath.lib_testdir]
        self.runner     = RedirectTestRunner
        self.runstep    = LibraryRunStep
        self.categories = categories.LibraryTests

    def getAllTests(self):
        return self.getTestsShownInCategories()
    
class RegressionTestConfig(TestConfig):
    def __init__(self):
        self.notRunList = []
        self.name       = "Regression"
        self.shortcut   = 'regr regress regression'
        self.directory  = testpath.lib_testdir
        self.runner     = RedirectTestRunner
        self.runstep    = RegressRunStep
        self.categories = categories.RegressionTests
        if is_cli64:
            # traceback support disabled on 64-bit
            self.notRunList += ['test.test_traceback']
        
    def getAllTests(self):
        return self.getTestsShownInCategories()

class CompatTestConfig(TestConfig):
    def __init__(self):
        self.notRunList = ['sbs_typeop.py']
        if is_cli64: 
           self.notRunList.append('sbs_exceptions.py')
        self.name       = "Compatability"
        self.shortcut   = 'compat sbs'
        self.directory  = testpath.compat_testdir
        self.runner     = RedirectTestRunner
        self.runstep    = CompatRunStep
        self.categories = categories.CompatTests

    def _applicable(self, s):
        sl = s.lower()
        if sl in self.notRunList: return False
        return sl.startswith("sbs_") and sl.endswith(".py")
            
       
def getAllConfigs(exclude=[]):
    _module = sys.modules[__name__]
    return [ getattr(_module, x)() for x in dir(_module) if x.endswith("TestConfig") and not exclude.count(x[:-10])]

## not used. leave it for future                    
def getAllConfigs2():       
    l = [ TestConfig() ] 
    
    def getExtraConfig(path):
        perserve_syspath()    
        sys.path.append(path)
        package = __import__("settings")
        for x in dir(package):
            if x.endswith("TestConfig"): 
                tc = getattr(package, x)
                if tc.__module__ == 'settings':
                    l.append(tc())
        restore_syspath() 
        del sys.modules['settings']
    
    getExtraConfig(testpath.team_dir)
    if my_dir: 
        getExtraConfig(testpath.my_dir)

    return l

def main(args):
    unknown = [x for x in args if x.startswith('-') and not x.startswith('-T:') and not x.startswith('-O:')]
    if unknown: usage(1, 'unknown options: %s' % unknown)

    # make sure we have a simple __future__
    ensure_future_present(testpath.compat_testdir)
    ensure_future_present(testpath.public_testdir)

    # set output level
    detailLevel = 'min'
    for x in [x[3:].lower() for x in args if x.startswith('-O:')]:
        if x in ('min', 'minimal'): detailLevel = 'min'
        elif x in ('med', 'mid', 'medium', 'middle'): detailLevel = 'med'
        elif x in ('max', 'verbose'): detailLevel = 'max'
    
    # set time level
    timeLevel = 'full'
    for x in [x[3:].lower() for x in args if x.startswith('-T:')]:
        if x in ('min', 'minimal', 'short', 'sanity'): timeLevel = 'short'
        elif x in ('med', 'mid', 'medium', 'middle'): timeLevel = 'medium'
        elif x in ('max', 'long', ): timeLevel = 'long'
        elif x in ('full', ): timeLevel = 'full'
    
    # decide the set of tests
    tests = [x.lower() for x in args if not x.startswith('-') and not x.endswith('-')]

    # -- excludes an entire test config, e.g. Compat--    
    allTcs = getAllConfigs([x[:-1] for x in args if x.endswith('-')])
    shortcuts = {}
    for x in allTcs: shortcuts[x.shortcut] = x

    processed = []
    for x in tests: 
        for y in shortcuts.keys():
            if x in y.split():  
                # setting tc.testList here
                shortcuts[y].applicableTests()
                processed.append(x)
                
    for x in processed: tests.remove(x)
    
    # hack: if we only have shortcut passed in, tests becomes empty; which will cause all tests to run
    if processed and not tests: tests = ['notexist']
    
    # now 'tests' is really category
    print 'tests: ', tests
    
    # To figure out which config has the desire test categories first
    filteredTcs = [ tc for tc in allTcs if len(tc.applicableTests(tests)) > 0 ]

    # go through each test config
    for tc in filteredTcs:
        if detailLevel <> 'min':
            print formatter.SeparatorEqual
            print tc.name.center(formatter.Number)
            print formatter.SeparatorMinus
        else:
            print 
            print formatter.Greater4, tc.name
        
        runner = tc.runner(tc)
        runner.detailLevel = detailLevel
        runner.timeLevel = timeLevel

        perserve_syspath()
        
        if isinstance(tc.directory, str): sys.path.insert(0, tc.directory)
        else: sys.path[0:0] = tc.directory

        runner.runstep = tc.runstep
        runner.runTests()
        
        restore_syspath()

    TestResultSet.Finish()
    
    if detailLevel == 'max':
        TestResultSet.PrintDetail()
        TestResultSet.PrintReproString()

    TestResultSet.PrintSummary()
    TestResultSet.SaveSummaryToFile()

    if TestResultSet.FailCnt:
        import nt
        nt.chmod(logname, 256)

    logstream.close()
    
    print '>>>> Log saved as', fullpath(logname)    
    sys.exit(TestResultSet.FailCnt)

if __name__ == "__main__": 
    main(sys.argv[1:])
