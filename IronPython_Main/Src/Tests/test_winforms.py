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

"""Test cases for features that require WinForms and may display UI"""

from lib.assert_util import *
skiptest("win32")
skiptest("silverlight")

import clr
import System

load_iron_python_test()

from IronPythonTest import *
clr.AddReference('System.Windows.Forms')
import System.Windows.Forms as SWF

########################################################################################################
# data binding helper classes

class AgeQualifier(object):
	def __get__(self, instance, ctx):
		if instance.Age < 13: return 'young'
		if instance.Age < 20: return 'teen'
		if instance.Age < 30: return 'twenties'
		if instance.Age < 40: return 'thirties'
		if instance.Age < 50: return 'forties'
		return 'old'  

SAMPLE_DATA = [('Joe', 23, 'twenties'),  ('Bob', 8, 'young'),  ('Thomas', 32, 'thirties'),  ('Patrick', 41, 'forties'),  ('Kathy', 19, 'teen'),  ('Sue', 77, 'old'),]

class Person(System.Object):
    def __init__(self, name, age):
        self._name = name
        self._age = age
    def get_name(self):
        return self._name
    def set_name(self, value):
        self._name = value
    Name = property(get_name, set_name)
    def get_age(self):
        return self._age
    def set_age(self, value):
        self._age = value
    Age = property(get_age, set_age)
    AgeDescription = AgeQualifier()

# test cases

def test_databinding_auto():
    class Form(SWF.Form):
        def __init__(self):
            SWF.Form.__init__(self)
            self._people = people = []
            for name, age, ignored in SAMPLE_DATA:
                people.append(Person(name, age))
            grid = SWF.DataGridView()
            grid.AutoGenerateColumns = True
            grid.DataSource = people
            grid.Dock = SWF.DockStyle.Fill
            self.grid = grid
            self.Controls.Add(grid)
    
    form = Form()
    def close_form():
        while not form.Visible:
            System.Threading.Thread.Sleep(100)
        System.Threading.Thread.Sleep(1000)
        
        for i in xrange(len(SAMPLE_DATA)):
            row = form.grid.Rows[i]
            AreEqual(row.Cells[0].FormattedValue, SAMPLE_DATA[i][0])
            AreEqual(int(row.Cells[1].FormattedValue), SAMPLE_DATA[i][1])
            AreEqual(row.Cells[2].FormattedValue, SAMPLE_DATA[i][2])

        form.Close()
    th = System.Threading.Thread(System.Threading.ThreadStart(close_form))
    th.Start()
    SWF.Application.Run(form) 
    

def test_databinding_manual():
    class Form(SWF.Form):
        def __init__(self):
            SWF.Form.__init__(self)
            self._people = people = []
            for name, age, ignored in SAMPLE_DATA:
                people.append(Person(name, age))
            grid = SWF.DataGridView()
            grid.AutoGenerateColumns = False
            grid.Columns.Add('Name', 'Name')
            grid.Columns[0].DataPropertyName = 'Name'
            grid.Columns.Add('Age', 'Age')
            grid.Columns[1].DataPropertyName = 'Age'
            grid.Columns.Add('AgeDescription', 'AgeDescription')
            grid.Columns[2].DataPropertyName = 'AgeDescription'
            grid.DataSource = people
            grid.Dock = SWF.DockStyle.Fill
            self.grid = grid
            self.Controls.Add(grid)
    
    form = Form()
    def close_form():
        while not form.Visible:
            System.Threading.Thread.Sleep(100)
        System.Threading.Thread.Sleep(1000)
            
        for i in xrange(len(SAMPLE_DATA)):
            row = form.grid.Rows[i]
            AreEqual(row.Cells[0].FormattedValue, SAMPLE_DATA[i][0])
            AreEqual(int(row.Cells[1].FormattedValue), SAMPLE_DATA[i][1])
            AreEqual(row.Cells[2].FormattedValue, SAMPLE_DATA[i][2])
        form.Close()
    th = System.Threading.Thread(System.Threading.ThreadStart(close_form))
    th.Start()
    SWF.Application.Run(form) 

def test_class_name():
    '''
    This failed under IP 2.0A4.
    '''
    AreEqual(SWF.TextBox.__name__, "TextBox")
    AreEqual(SWF.TextBox().__class__.__name__, "TextBox")


run_test(__name__)