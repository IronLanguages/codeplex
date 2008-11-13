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

import clr
clr.AddReference("IronPython")
from IronPython.Runtime import PythonContext
clr.AddReference("Microsoft.Scripting")
from Microsoft.Scripting.Hosting.Shell.Remote import RemoteConsoleHost, ConsoleRestartManager
from System.Reflection import Assembly

# Sending Ctrl-C is hard to automate in testing. This class tests the AbortCommand functionality
# without relying on Ctrl-C
class AutoAbortableConsoleHost(RemoteConsoleHost):
    def get_Provider(self):
        return PythonContext
    
    def CustomizeRemoteRuntimeStartInfo(self, processInfo):
        processInfo.FileName = Assembly.GetEntryAssembly().Location

    def OnOutputDataReceived(self, sender, eventArgs):
        super(AutoAbortableConsoleHost, self).OnOutputDataReceived(sender, eventArgs);
        if eventArgs.Data == None:
            return
        if "ABORT ME!!!" in eventArgs.Data:
            self.AbortCommand()

class TestConsoleRestartManager(ConsoleRestartManager):
    def CreateRemoteConsoleHost(self):
        return AutoAbortableConsoleHost()
    
    def OnRemoteRuntimeExited(self, sender, eventArgs):
        print "Remote runtime exited. Raising ThreadAbort to abort console thread. Press Enter to nudge the old thread out of Console.Readline..."
        super(TestConsoleRestartManager, self).OnRemoteRuntimeExited(sender, eventArgs)

if __name__ == "__main__":
    console = TestConsoleRestartManager()
    console.Run(True)
