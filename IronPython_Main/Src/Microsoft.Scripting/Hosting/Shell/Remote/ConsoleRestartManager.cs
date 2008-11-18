/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SILVERLIGHT // Remoting

using System; using Microsoft;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Scripting.Hosting.Shell.Remote {
    /// <summary>
    /// Supports detecting the remote runtime being killed, and starting up a new one
    /// </summary>
    public abstract class ConsoleRestartManager {
        private RemoteConsoleHost _remoteConsole;
        private Thread _consoleThread;
        private bool _normalExit;

        #region Private methods

        private void RunConsole() {
            _remoteConsole.Run(new string[0]);
            _normalExit = true;
        }

        private void InitializeConsole() {
            _remoteConsole = CreateRemoteConsoleHost();
            _normalExit = false;
            _remoteConsole.RemoteRuntimeExited += OnRemoteRuntimeExited;
            ThreadStart threadStart = new ThreadStart(RunConsole);
            _consoleThread = new Thread(threadStart);
            _consoleThread.Start();
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")] // TODO: review
        public virtual void OnRemoteRuntimeExited(object sender, EventArgs eventArgs) {
            Debug.Assert(!_normalExit);
            // Abort the console thread. Note that the ThreadAbortException will be deferred if the thread
            // is in native code. 
            _consoleThread.Abort();
        }

        public abstract RemoteConsoleHost CreateRemoteConsoleHost();

        public void Run(bool exitOnNormalExit) {
            do {
                InitializeConsole();
                _consoleThread.Join();
                _remoteConsole.Dispose();
            } while (exitOnNormalExit && !_normalExit);
        }
    }
}

#endif
