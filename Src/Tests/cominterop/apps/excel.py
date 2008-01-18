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

# Excel COM Interop tests

from lib.assert_util import *
skiptest("win32", "silverlight", "cli64")
from lib.cominterop_util import *

if not IsExcelInstalled():
    from sys import exit
    print "Excel is not installed.  Cannot run this test!"
    exit(1)
else:
    TryLoadExcelInteropAssembly()
    from Microsoft.Office.Interop import Excel

def CreateApplication():
    if preferComDispatch:
        from System import Type, Activator
        applicationType = Type.GetTypeFromProgID("Excel.Application")
        return Activator.CreateInstance(applicationType)
    else:
        return Excel.ApplicationClass()

#------------------------------------------------------------------------------
#--HELPERS
selection_counter = 0

def selection_change_eventhandler(range):
    global selection_counter
    selection_counter = selection_counter + 1
    #print "selected range - " + range.Address[0]
        
def add_worksheet_event(ws):
    if preferComDispatch:
        ws.Event_SelectionChange += selection_change_eventhandler
    else:
        ws.SelectionChange += selection_change_eventhandler

def remove_worksheet_event(ws):
    if preferComDispatch:
        ws.Event_SelectionChange -= selection_change_eventhandler
    else:
        ws.SelectionChange -= selection_change_eventhandler

#-----------------------------------------------------------------------------
#--TESTS
def _test_excel():
    ex = None
    
    try: 
        ex = CreateApplication() 
        ex.DisplayAlerts = False
        AreEqual(ex.DisplayAlerts, False)
        #ex.Visible = True
        nb = ex.Workbooks.Add()
        ws = nb.Worksheets[1]

        AreEqual('Sheet1', ws.Name)

        if not preferComDispatch: # Bug 325464
            # COM has 1-based arrays
            AssertError(EnvironmentError, lambda: ws.Rows[0])

        for i in range(1, 10):
            for j in range(1, 10):
                ws.Cells[i, j] = i * j

        rng = ws.Range['A1', 'B3']
        AreEqual(6, rng.Count)

        co = ws.ChartObjects()
        graph = co.Add(100, 100, 200, 200)
        graph.Chart.ChartWizard(rng, Excel.XlChartType.xl3DColumn)                        
    
    finally:            
        # clean up outstanding RCWs 
        ws = None
        nb = None
        rng = None
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        if ex: ex.Quit()
        else: print "ex is %s" % ex


def test_excelevents():
    ex = None
    try: 
        ex = CreateApplication() 
        ex.DisplayAlerts = False 
        #ex.Visible = True
        wb = ex.Workbooks.Add()
                
        ws = ex.ActiveSheet
        
        global selection_counter
        selection_counter = 0

        # test single event is firing
        add_worksheet_event(ws)
        ex.ActiveCell.Offset[1, 0].Activate()
        AreEqual(selection_counter, 1)

        # test events chaining is working
        add_worksheet_event(ws)
        ex.ActiveCell.Offset[1, 0].Activate()
        AreEqual(selection_counter, 3)

        # test removing event from a chain
        remove_worksheet_event(ws)
        ex.ActiveCell.Offset[1, 0].Activate()
        AreEqual(selection_counter, 4)

        # test removing event alltogether
        remove_worksheet_event(ws)
        ex.ActiveCell.Offset[1, 0].Activate()
        AreEqual(selection_counter, 4)

        if "-X:Interpret" in System.Environment.CommandLine:
            print "Rowan Work Item 312901"
            ws = None
            System.GC.Collect()
            System.GC.WaitForPendingFinalizers()
            return
                
        add_worksheet_event(ws)
        ex.ActiveCell.Offset[1, 0].Activate()
        AreEqual(selection_counter, 5)

        # test GCing RCW detaches the event handler

        # NOTICE: You might wonder why we call "add_worksheet_event"
        # NOTICE: and "remove_worksheet_event" instead of just inlining
        # NOTICE: ws.Event_SelectionChange += selection_change_eventhandler
        # NOTICE: The reason is that IPy emits code with local vars
        # NOTICE: for temporary variables. These vars are scoped to the current
        # NOTICE: frame. As a result GC.Collect would not collect the object 
        # NOTICE: returned by ws.Event_SelectionChange which holds a references to Worksheet.
        # NOTICE: which will prevent the event handler from unadvising.

        ws = None
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()

        ex.ActiveCell.Offset[1, 0].Activate()
        AreEqual(selection_counter, 5)

    finally:
        # clean up outstanding RCWs 
        ws = None
        wb = None
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()
                
        if ex: ex.Quit()
        else: print "ex is %s" % ex

def test_new(): # regression test for 148579
    AssertErrorWithMessage(TypeError, 
        "Cannot create instances of Range", 
        lambda: Excel.Range(1,2))
    
#------------------------------------------------------------------------------
run_com_test(__name__, __file__)
