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

from lib.assert_util import *
skiptest("silverlight")
import clr
import sys
import re
from System.Diagnostics import Process, ProcessWindowStyle
from time import sleep

doRun = True

sys.path.append(testpath.rowan_root + '\\Languages\\IronPython\\External\\Maui')

try:
    clr.AddReference('Maui.Core.dll')
except:
    print "test_superconsole.py failed: cannot load Maui.Core assembly"
    doRun = False

if doRun:
    from Maui.Core import App
    proc = Process()
    proc.StartInfo.FileName = sys.executable
    proc.StartInfo.WorkingDirectory = testpath.rowan_root + '\\Languages\\IronPython\\Tests'
    proc.StartInfo.Arguments = '-X:TabCompletion -X:AutoIndent -X:ColorfulConsole'
    proc.StartInfo.UseShellExecute = True	
    proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal
    proc.StartInfo.CreateNoWindow = False
    started = proc.Start()

    try:
        superConsole = App(proc.Id)
    except:
        print "test_superconsole.py failed: cannot initialize App object (probably running as service, or in minimized remote window"
        proc.Kill()
        doRun = False
        #sys.exit(1)

if doRun:
# (Test scaffolding is loaded into the SuperConsole by pretest.py)
    superConsole.SendKeys('from pretest import *{ENTER}')

# Store test regexp for the baseline
    testRegex = ""

# Test Case #1: ensure that an attribute with a prefix unique to the dictionary is properly completed.
######################################################################################################
    print "CodePlex Work Item 10928"
    #superConsole.SendKeys('print z{TAB}{ENTER}')
    #testRegex += 'zoltar'
    #superConsole.SendKeys('print yo{TAB}{ENTER}')
    #testRegex += 'yorick'

# Test Case #2: ensure that tabbing on a non-unique prefix cycles through the available options
######################################################################################################
    print "CodePlex Work Item 10928"
    #superConsole.SendKeys('print y{TAB}{ENTER}')
    #superConsole.SendKeys('print y{TAB}{TAB}{ENTER}')
    #testRegex += '(yorickyak|yakyorick)'


# Test Case #3: ensure that tabbing after 'ident.' cycles through the available options
######################################################################################################

# 3.1: identifier is valid, we can get dict
    superConsole.SendKeys('print c.{TAB}{ENTER}')

# it is *either* __doc__ ('Cdoc') or __module__ ('pretest')
    testRegex += '(Cdoc|pretest)'

# 3.2: identifier is not valid
    superConsole.SendKeys('try:{ENTER}')

# autoindent
    superConsole.SendKeys('print f.{TAB}x{ENTER}')

# backup from autoindent
    superConsole.SendKeys('{BACKSPACE}except:{ENTER}')
    superConsole.SendKeys('print "EXC"{ENTER}{ENTER}{ENTER}')
    testRegex += 'EXC'

# Test Case #4: auto-indent
######################################################################################################
    superConsole.SendKeys("def f{(}{)}:{ENTER}print 'f!'{ENTER}{ENTER}")
    superConsole.SendKeys('f{(}{)}{ENTER}')
    testRegex += 'f!'

# Test Case #5: backspace and delete
######################################################################################################
    superConsole.SendKeys("print 'IQ{BACKSPACE}P'{ENTER}")
    testRegex += "IP"

    superConsole.SendKeys("print 'FW'{LEFT}{LEFT}{DELETE}X{ENTER}")
    testRegex += "FX"

# 5.3: backspace over auto-indentation
#   a: all white space
#   b: some non-whitespace characters

# Test Case #6: cursor keys
######################################################################################################
    superConsole.SendKeys("print 'up'{ENTER}")
    testRegex += 'up'
    superConsole.SendKeys("print 'down'{ENTER}")
    testRegex += 'down'
    superConsole.SendKeys("{UP}{UP}{ENTER}") 
    testRegex += 'up'
    superConsole.SendKeys("{DOWN}{ENTER}")
    testRegex += 'down'

    superConsole.SendKeys("print 'up'{ENTER}{UP}{ENTER}")
    testRegex += 'upup'
    superConsole.SendKeys("print 'awy{LEFT}{LEFT}{RIGHT}a{RIGHT}'{ENTER}")
    testRegex += 'away'
    superConsole.SendKeys("print 'bad'{ESC}print 'good'{ENTER}")
    testRegex += 'good'
    superConsole.SendKeys("rint 'hom'{HOME}p{END}{LEFT}e{ENTER}")
    testRegex += 'home'

# Test Case #7: control-character rendering
###########################################

# Ctrl-D
    superConsole.SendKeys('print "^(d)^(d){LEFT}{DELETE}"{ENTER}')
    testRegex += chr(4)

# check that Ctrl-C breaks an infinite loop (the test is that subsequent things actually appear)
    superConsole.SendKeys('while True: pass{ENTER}{ENTER}')
    superConsole.SendKeys('^(c)')

# check that Ctrl-C breaks an infinite loop (the test is that subsequent things actually appear)
    superConsole.SendKeys('def foo{(}{)}:{ENTER}try:{ENTER}while True: pass{ENTER}{BACKSPACE}{BACKSPACE}except KeyboardInterrupt:{ENTER}print "caught"{ENTER}{BACKSPACE}{ENTER}print "after"{ENTER}{BACKSPACE}{ENTER}foo{(}{)}{ENTER}')    
    sleep(2)
    superConsole.SendKeys('^(c)')
    testRegex += 'caughtafter'

# Test Case #8: tab insertion
###########################################
    superConsole.SendKeys('print "x{TAB}{TAB}y"{ENTER}')
    testRegex += 'x    y'
	
# Test Case #9: make sure that home, delete, backspace, etc. at start have no effect
###########################################
    superConsole.SendKeys('{BACKSPACE}{DELETE}{HOME}{LEFT}print "start"{ENTER}')
    testRegex += 'start'

# Test Case #10: tab-completion is case-insensitive (wrt input)
###########################################
    superConsole.SendKeys('import System{ENTER}')
    superConsole.SendKeys('print System.r{TAB}{ENTER}')
    testRegex += "<type 'Random'>"

# Test Case #11: history
###########################################
    superConsole.SendKeys('print "first"{ENTER}')
    testRegex += 'first'
    superConsole.SendKeys('print "second"{ENTER}')
    testRegex += 'second'
    superConsole.SendKeys('print "third"{ENTER}')
    testRegex += 'third'
    superConsole.SendKeys('print "fourth"{ENTER}')
    testRegex += 'fourth'
    superConsole.SendKeys('print "fifth"{ENTER}')
    testRegex += 'fifth'
    superConsole.SendKeys('{UP}{UP}{UP}{ENTER}')
    testRegex += 'third'
    superConsole.SendKeys('{UP}{ENTER}')
    testRegex += 'third'
    superConsole.SendKeys('{UP}{UP}{UP}{DOWN}{ENTER}')
    testRegex += 'second'
    superConsole.SendKeys('{UP}{ENTER}')
    testRegex += 'second'
    superConsole.SendKeys('{DOWN}{ENTER}')
    testRegex += 'third'
    superConsole.SendKeys('{DOWN}{ENTER}')
    testRegex += 'fourth'
    superConsole.SendKeys('{DOWN}{ENTER}')
    testRegex += 'fifth'
    superConsole.SendKeys('{UP}{UP}{ESC}print "sixth"{ENTER}')
    testRegex += 'sixth'
    superConsole.SendKeys('{UP}{ENTER}')
    testRegex += 'sixth'
    superConsole.SendKeys('{UP}{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}{ENTER}')
    testRegex += 'sixth'

# and finally test that F6 shuts it down
    superConsole.SendKeys('{F6}')
    superConsole.SendKeys('{ENTER}')
    sleep(5)
    Assert(not superConsole.IsRunning)

# now verify the log file against the test regexp

    f = open(testpath.rowan_root + '\\Languages\\IronPython\\Tests\\ip_session.log','r')
    
    chopped = ''.join([line[:-1] for line in f.readlines() if not line.startswith(">>>") and not line.startswith("...")])
    f.close()

    Assert(re.match(testRegex, chopped))
    print "Passed."
        

