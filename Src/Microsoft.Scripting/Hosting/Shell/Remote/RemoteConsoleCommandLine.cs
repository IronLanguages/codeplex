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
using System.Threading;

namespace Microsoft.Scripting.Hosting.Shell.Remote {
    /// <summary>
    /// Customize the CommandLine for remote scenarios
    /// </summary>
    public class RemoteConsoleCommandLine : CommandLine {
        private RemoteConsoleCommandDispatcher _remoteConsoleCommandDispatcher;

        public RemoteConsoleCommandLine(RemoteCommandDispatcher remoteCommandDispatcher, AutoResetEvent remoteOutputReceived) {
            _remoteConsoleCommandDispatcher = new RemoteConsoleCommandDispatcher(remoteCommandDispatcher, remoteOutputReceived);
        }

        protected override ICommandDispatcher CreateCommandDispatcher() {
            return _remoteConsoleCommandDispatcher;
        }

        protected override void UnhandledException(Exception e) {
            string message;
            try {
                ExceptionOperations exceptionOperations = Engine.GetService<ExceptionOperations>();
                message = exceptionOperations.FormatException(e);
            } catch (System.Runtime.Remoting.RemotingException) {
                // The remote server may have shutdown. So just do something simple
                message = e.ToString();
            }
            Console.WriteLine(message, Style.Error);
        }

        /// <summary>
        /// CommandDispatcher to ensure synchronize output from the remote runtime
        /// </summary>
        class RemoteConsoleCommandDispatcher : ICommandDispatcher {
            private RemoteCommandDispatcher _remoteCommandDispatcher;
            private AutoResetEvent _remoteOutputReceived;

            internal RemoteConsoleCommandDispatcher(RemoteCommandDispatcher remoteCommandDispatcher, AutoResetEvent remoteOutputReceived) {
                _remoteCommandDispatcher = remoteCommandDispatcher;
                _remoteOutputReceived = remoteOutputReceived;
            }

            public object Execute(CompiledCode compiledCode, ScriptScope scope) {
                // Delegate the operation to the RemoteCommandDispatcher which will execute the code in the remote runtime
                object result = _remoteCommandDispatcher.Execute(compiledCode, scope);

                // Output is received async, and so we need explicit synchronization in the remote console
                _remoteOutputReceived.WaitOne();

                return result;
            }
        }
    }
}

#endif