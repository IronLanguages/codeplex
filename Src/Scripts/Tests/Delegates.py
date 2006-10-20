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

import clr
clr.AddReferenceByPartialName("System.Drawing")
clr.AddReferenceByPartialName("System.Windows.Forms")

from System.Windows.Forms import *
from System.Drawing import *
import System

superForm = Form(Text="Test")
superTimer = Timer()

def MyOnPaint(data, event):
    f = data
    g = f.CreateGraphics()
    #g.GetHdc()
    br = SolidBrush(SystemColors.WindowText)
    g.DrawString("Hello", f.Font, br, System.Single.Epsilon,System.Single.Epsilon)

def MyTick(data, event):
    superForm.Close()
    superTimer.Stop();

def MyLoad(data, event):
    superTimer.Interval = 1000
    superTimer.Tick += MyTick
    superTimer.Start()

superForm.Paint += MyOnPaint
superForm.Load += MyLoad
superForm.ShowDialog()


from Util.Debug import *
load_iron_python_test()

def identity(x): return x

import IronPythonTest
r = IronPythonTest.ReturnTypes()
r.floatEvent += identity

AreEqual(r.RunFloat(1.4), 1.4)

################################################################################################
# verify bound / unbound methods go to the write delegates...
# ParameterizedThreadStart vs ThreadStart is a good example of this, we have a delegate
# that takes a parameter, and one that doesn't, and we need to correctly disambiguiate

class foo(object):
    def bar(self):
        global called, globalSelf
        called = True
        globalSelf = self
    def baz(self, arg):
        global called, globalSelf, globalArg
        called = True
        globalSelf = self
        globalArg = arg

from System.Threading import Thread

# try parameterized thread

a = foo()
t = Thread(foo.bar)
t.Start(a)
t.Join()

AreEqual(called, True)
AreEqual(globalSelf, a)

# try non-parameterized
a = foo()
called = False

t = Thread(a.bar)
t.Start()
t.Join()

AreEqual(called, True)
AreEqual(globalSelf, a)


# parameterized w/ self
a = foo()
called = False

t = Thread(a.baz)
t.Start('hello')
t.Join()

AreEqual(called, True)
AreEqual(globalSelf, a)
AreEqual(globalArg, 'hello')


# parameterized w/ self & extra arg, should throw

try:
    t = Thread(foo.baz)
    AreEqual(True, False)
except TypeError: pass


