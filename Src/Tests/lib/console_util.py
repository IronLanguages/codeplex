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

"""
This module contains a single class, IronPythonInstance, which
encapsulates a separately-spawned, interactive Iron Python process.
Its purpose is to enable testing behaviour of the top-level console,
when that differs from behaviour while importing a module and executing
its statements.
"""
from System.Diagnostics import Process, ProcessStartInfo
from System.IO import StreamReader, StreamWriter

class IronPythonInstance:
    """
    Class to hold a single instance of the Iron Python interactive
console for testing purposes, and direct input to and from the instance.

    Example usage:
    from sys import exec_prefix
    ip = IronPythonInstance(sys.executable, exec_prefix)
    AreEqual(ip.Start(), True)
    if ip.ExecuteLine("1+1") != "2":
        raise "Bad console!"
    else:
        print "Console passes sanity check."
    ip.End()

    """
    def __init__(self, pathToBin, wkDir, *parms):
        self.proc = Process()
        self.proc.StartInfo.FileName = pathToBin
        self.proc.StartInfo.WorkingDirectory = wkDir
        self.proc.StartInfo.Arguments = " ".join(parms)
        self.proc.StartInfo.UseShellExecute = False
        self.proc.StartInfo.RedirectStandardOutput = True
        self.proc.StartInfo.RedirectStandardInput = True
        self.proc.StartInfo.RedirectStandardError = True

    def Start(self):
        if (not self.proc.Start()):
            return False
        else:
            self.reader = self.proc.StandardOutput
            self.reader2 = self.proc.StandardError
            self.writer = self.proc.StandardInput
            self.EatToPrompt()
            return True

    def StartAndRunToCompletion(self):
        if (not self.proc.Start()):
            return (False, None, None)
        else:
            self.reader = self.proc.StandardOutput
            self.reader2 = self.proc.StandardError
            self.writer = self.proc.StandardInput
            # This will hang if the output exceeds the buffer size
            output = self.reader.ReadToEnd()
            output2 = self.reader2.ReadToEnd()
            return (True, output, output2, self.proc.ExitCode)

    def EnsureInteractive(self):
        if "4" <> self.ExecuteLine("2 + 2"): 
            raise AssertionError, 'EnsureInteractive failed'

    # This implements a state machine which reads all lines until it gets to
    # ">>> ". Note that this text could occur in the middle of other output.
    # However, it is important to read all the output from the child process
    # to avoid deadlocks. Hence, we assume that ">>> " will only occur
    # as the prompt.
    handlers = [ (lambda ch: (ch == '>'  and (1,   '')) or (0, ch)),
                 (lambda ch: (ch == '>'  and (2,   '')) or (0, ch)),
                 (lambda ch: (ch == '>'  and (3,   '')) or (0, ch)),
                 (lambda ch: (ch == ' '  and (4,   '')) or (0, ch))]

    def EatToPrompt(self, readError=False):
        slurped = ""
        state = 0
        while state < 4:
            c = self.reader.Read()
            if c == -1: raise ValueError("unexpected end of input")
            (state, nextChar) = self.handlers[state](chr(c))
            slurped += nextChar
            if slurped == '...': raise ValueError("found ... instead of >>>")
            
        if readError: 
            class RE:
                def __init__(self, eStream):
                    self.errString = ""
                    self.errStream = eStream
                def reading_error(self):
                    while True:
                        nextChar = self.errStream.Read()
                        if (nextChar == -1): break
                        self.errString += chr(nextChar)
                        
            from System.Threading import Thread, ThreadStart
            from time import sleep
            
            re = RE(self.reader2)
            th = Thread(ThreadStart(re.reading_error))
            th.Start()
            prev = re.errString
            sleep(1)
            while prev != re.errString:
                sleep(1)
                prev = re.errString
                #print prev, re.errString
            th.Abort()
            
            #print re.errString + slurped
            return re.errString + slurped
        else: 
            return slurped
        

    # Execute a single-line command, and return the output
    def ExecuteLine(self, line, readError=False):
        self.writer.Write(line+"\n")
        return self.EatToPrompt(readError)[0:-2]

    # Submit one line of a multi-line command to the console. There can be 
    # multiple calls to ExecutePartialLine before a final call to ExecuteLine
    def ExecutePartialLine(self, line):
        self.writer.Write(line+"\n")
        ch = self.reader.Read()
        if ch == -1 or chr(ch) <> '.' : raise AssertionError, 'missing the first dot'
        ch = self.reader.Read()
        if ch == -1 or chr(ch) <> '.' : raise AssertionError, 'missing the second dot'
        ch = self.reader.Read()
        if ch == -1 or chr(ch) <> '.' : raise AssertionError, 'missing the third dot'
        ch = self.reader.Read()
        if ch == -1 or chr(ch) <> ' ' : raise AssertionError, 'missing the last space char'

    def End(self):
        if 'writer' in dir(self) and 'Close' in dir(self.writer):
            self.writer.Close()
