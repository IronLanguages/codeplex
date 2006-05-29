/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

#region Using directives

using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

#endregion

namespace IronPython.Hosting {
    public interface IConsole {
        // Read a single line of interactive input
        // autoIndentSize is the indentation level to be used for the current suite of a compound statement.
        // The console can ignore this argument if it does not want to support auto-indentation
        string ReadLine(int autoIndentSize);

        void Write(string text, Style style);
        void WriteLine(string text, Style style);
    }

    public enum Style {
        Prompt, Out, Error
    }
    
    public class BasicConsole : IConsole {
        private TextWriter Out;
        private TextReader In;
        private AutoResetEvent ctrlCEvent;
        private Thread MainEngineThread = Thread.CurrentThread;

        public BasicConsole()
            : this(Console.Out, Console.In) {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            ctrlCEvent = new AutoResetEvent(false);
        }

        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC) {
                e.Cancel = true;
                ctrlCEvent.Set();
                MainEngineThread.Abort(new IronPython.Runtime.PythonKeyboardInterrupt(""));
            }
        }

        public BasicConsole(TextWriter _out, TextReader _in) {
            this.Out = _out;
            this.In = _in;
        }

        #region IConsole Members

        public string ReadLine(int autoIndentSize) {         
            string res= In.ReadLine();
            if (res == null) {
                // we have a race - the Ctrl-C event is delivered
                // after ReadLine returns.  We need to wait for a little
                // bit to see which one we got.  This will cause a slight
                // delay when shutting down the process via ctrl-z, but it's
                // not really perceptible.  In the ctrl-C case we will return
                // as soon as the event is signaled.
                if (ctrlCEvent != null && ctrlCEvent.WaitOne(100,false)) {
                    // received ctrl-C
                    return "";
                } else {
                    // received ctrl-Z
                    return null;
                }
            }
            return res;
        }

        public void Write(string text, Style style) {
            Out.Write(text);
            Out.Flush();
        }

        public void WriteLine(string text, Style style) {
            Out.WriteLine(text);
        }

        #endregion
    }
}
