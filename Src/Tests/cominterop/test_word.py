#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

# COM Interop tests for IronPython
from lib.assert_util import skiptest
skiptest("win32", "silverlight", "cli64")
from lib.cominterop_util import *

from Microsoft.Win32 import Registry
from System.IO import File

def IsWordInstalled():
    word  = None
    
    #Office 11 or 12 are both OK for this test. Office 12 is preferred.
    word = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office\\12.0\\Word\\InstallRoot")
    if word==None:
        word= Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office\\11.0\\Word\\InstallRoot")
    
    #sanity check
    if word==None:
        return False
    
    #make sure it's really installed on disk
    word_path = word.GetValue("Path") + "winword.exe"
    return File.Exists(word_path)

if not IsWordInstalled():
    from sys import exit
    print "Word is not installed.  Cannot run this test!"
    exit(1)

#------------------------------------------------------------------------------
#--HELPERS

def IsWordPIAInstalled():
    word_pia_registry  = None

    wordapp_pia_registry = Registry.ClassesRoot.OpenSubKey("CLSID\\{000209FF-0000-0000-C000-000000000046}\\InprocServer32")
    #worddoc_pia_registry = Registry.ClassesRoot.OpenSubKey("CLSID\\{00020906-0000-0000-C000-000000000046}\\InprocServer32")

    return wordapp_pia_registry != None

isPiaInstalled = IsWordPIAInstalled();

selection_counter = 0

def wd_selection_change_eventhandler(range):
    global selection_counter
    selection_counter = selection_counter + 1
    #print "selected range - ", range.Start, range.End

def add_wordapp_event(wdapp):
    if isPiaInstalled : 
        wdapp.WindowSelectionChange += wd_selection_change_eventhandler
    else: 
        wdapp.Event_WindowSelectionChange += wd_selection_change_eventhandler

def remove_wordapp_event(wdapp):
    if isPiaInstalled : 
        wdapp.WindowSelectionChange -= wd_selection_change_eventhandler
    else: 
        wdapp.Event_WindowSelectionChange -= wd_selection_change_eventhandler
    
def get_range(doc, start, end):
    if isPiaInstalled : 
        return doc.Range(start, end)[0]
    else: 
        return doc.Range(start, end)
    
def quit_word(wd):
    if isPiaInstalled : 
        wd.Quit(clr.Reference[System.Object](0))
    else: 
        wd.Quit(0)

def test_wordevents():
    if isPiaInstalled:
        print "Found PIAs for Word"
    else:
        print "No PIAs for Word were Found!!!!" 

    # running "tlbimp" for Word is VERY expensive and usually fails
    # let's disable the scenarios when Ipy attempts to generate
    # an interop assembly on the fly i.e. when there is no PIA
    # and no -X:PreferComDispatch
    if not isPiaInstalled and not preferComDispatch:
        print "Prefer COM dispatch is required when Word PIA is not installed!!!"
        return

    type = System.Type.GetTypeFromProgID("Word.Application")
    wd = None
    doc = None
    try:
        wd = System.Activator.CreateInstance(type)

        #wd.Visible = True
        doc = wd.Documents.Add()
        doc.Range().Text = "test"

        global selection_counter 
        selection_counter = 0

        add_wordapp_event(wd)
        get_range(doc, 1, 1).Select()
        AreEqual(selection_counter, 1)

        add_wordapp_event(wd)
        get_range(doc, 1, 2).Select()
        AreEqual(selection_counter, 3)

        remove_wordapp_event(wd)
        get_range(doc, 2, 2).Select()
        AreEqual(selection_counter, 4)

        remove_wordapp_event(wd)
        get_range(doc, 2, 3).Select()
        AreEqual(selection_counter, 4)

    finally:
        
        # clean up outstanding RCWs 
        doc = None
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()
        
        if wd: quit_word(wd)
        else: print "wd is %s" % wd

#------------------------------------------------------------------------------
run_com_test(__name__, __file__)
