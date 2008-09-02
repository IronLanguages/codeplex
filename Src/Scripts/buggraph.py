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

#Builds a graph using Excel automation
import clr, sys
import System, System.IO
clr.AddReferenceByName('Microsoft.Office.Interop.Excel, Version=11.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c')
import Microsoft.Office.Interop.Excel as Excel

def setcellvalue(sheet, cell, value):
    sheet.Range(cell, cell).Value = value

def buildgraph(data):
    excel = Excel.ApplicationClass()
    workbook = excel.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet)
    worksheet = workbook.Worksheets[1]
    setcellvalue(worksheet, "A2", "P1 bugs")
    setcellvalue(worksheet, "A3", "P2 bugs")
    setcellvalue(worksheet, "A4", "Active bugs")
    setcellvalue(worksheet, "A5", "Resolved bugs")
    weeknumber = 0
    letters = 'BCDEFGHIJKLMNOPQRSTUVWXYZ'
    for weekdata in data:
        setcellvalue(worksheet, letters[weeknumber] + '1', weekdata[1])
        weeknumber += 1
    weeknumber = 0
    for weekdata in data:
        for i in xrange(0, 4):
            setcellvalue(worksheet, letters[weeknumber] + str(i+2), weekdata[0][i])
        weeknumber += 1
    range = worksheet.Range("A1", letters[len(data)-1] + '5');
    chartobjects = worksheet.ChartObjects()
    chart = chartobjects.Add(0, 100, 500, 350);
    chart.Chart.ChartWizard(range, Excel.XlChartType.xlLineMarkers,\
        4, Excel.XlRowCol.xlRows, 0, 0, True, "Bug trends last %d weeks" % len(data),\
        "Date", "Number of bugs")
    chart.Chart.Export(r"c:\temp\__graph.gif", "GIF", False)
    workbook.Saved = True
    excel.Quit()
