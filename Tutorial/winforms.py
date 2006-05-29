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
clr.AddReferenceByPartialName("System.Windows.Forms")
clr.AddReferenceByPartialName("System.Drawing")
clr.AddReferenceByPartialName("IronPython")

from System.Drawing import Size
from System.Windows.Forms import Form, Application
from System.Threading import Thread
from IronPython.Runtime import CallTarget0
from System.Threading import AutoResetEvent
import IronPython


are = AutoResetEvent(False)

def thread_proc():
    try:
        global dispatcher
        global are
        dispatcher = Form(Size = Size(0,0))
        dispatcher.Show()
        dispatcher.Hide()
        are.Set()
        Application.Run()
    finally:
        IronPython.Hosting.PythonEngine.ExecWrapper = None

def callback(code):
    if code:
        dispatcher.Invoke(CallTarget0(code))
    else:
        Application.Exit()

t = Thread(thread_proc)
t.Start()
are.WaitOne()
IronPython.Hosting.PythonEngine.ExecWrapper = callback
