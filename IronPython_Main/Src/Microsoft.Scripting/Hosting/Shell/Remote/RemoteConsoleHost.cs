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
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;

namespace Microsoft.Scripting.Hosting.Shell.Remote {
    /// <summary>
    /// ConsoleHost where the ScriptRuntime is hosted in a separate process (referred to as the remote runtime server)
    /// 
    /// The RemoteConsoleHost spawns the remote runtime server and specifies an IPC channel name to use to communicate
    /// with each other. The remote runtime server creates and initializes a ScriptRuntime and a ScriptEngine, and publishes
    /// it over the specified IPC channel at a well-known URI. Note that the RemoteConsoleHost cannot easily participate
    /// in the initialization of the ScriptEngine as classes like LanguageContext are not remotable.
    /// 
    /// The RemoteConsoleHost then starts the interactive loop and executes commands on the ScriptEngine over the remoting channel.
    /// The RemoteConsoleHost listens to stdout of the remote runtime server and echos it locally to the user.
    /// </summary>
    public abstract class RemoteConsoleHost : ConsoleHost, IDisposable {
        Process _remoteRuntimeProcess;
        internal RemoteCommandDispatcher _remoteCommandDispatcher;
        private string _channelName = RemoteConsoleHost.GetChannelName();
        private IpcChannel _clientChannel;
        private AutoResetEvent _remoteOutputReceived = new AutoResetEvent(false);
        private EventWaitHandle _serverInitializedEvent;

        ~RemoteConsoleHost() {
            Dispose(false);
        }

        #region Private methods

        private static string GetChannelName() {
            return "RemoteRuntime-" + System.DateTime.Now.Ticks;
        }

        private ProcessStartInfo GetProcessStartInfo() {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Arguments = RemoteRuntimeServer.RemoteRuntimeArg + " " + _channelName;
            processInfo.CreateNoWindow = true;

            // Set UseShellExecute to false to enable redirection.
            processInfo.UseShellExecute = false;

            // Redirect the standard streams. The output streams will be read asynchronously using an event handler.
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            // The input stream can be ignored as the remote server process does not need to read any input
            processInfo.RedirectStandardInput = true;

            CustomizeRemoteRuntimeStartInfo(processInfo);
            Debug.Assert(processInfo.FileName != null);
            return processInfo;
        }

        private void StartRemoteRuntimeProcess() {
            Process process = new Process();

            process.StartInfo = GetProcessStartInfo();

            process.OutputDataReceived += new DataReceivedEventHandler(OnOutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(OnErrorDataReceived);

            process.Exited += new EventHandler(OnRemoteRuntimeExited);
            process.Start();

            // Start the asynchronous read of the output streams.
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // wire up exited 
            process.EnableRaisingEvents = true;

            _remoteRuntimeProcess = process;

            _serverInitializedEvent = new EventWaitHandle(false, EventResetMode.AutoReset, _channelName);
            _serverInitializedEvent.WaitOne();
        }

        private T GetRemoteObject<T>(string uri) {
            T result = (T)Activator.GetObject(typeof(T), "ipc://" + _channelName + "/" + uri);

            // Ensure that the remote object is responsive by calling a virtual method (which will be executed remotely)
            Debug.Assert(result.ToString() != null);

            return result;
        }

        private void InitializeRemoteScriptEngine() {
            StartRemoteRuntimeProcess();

            Engine = GetRemoteObject<ScriptEngine>(RemoteRuntimeServer.RuntimeUriName);

            _remoteCommandDispatcher = GetRemoteObject<RemoteCommandDispatcher>(RemoteRuntimeServer.CommandDispatcherUri);

            // Register a channel for the reverse direction, when the remote runtime process wants to fire events
            // or throw an exception
            string clientChannelName = _channelName.Replace("RemoteRuntime", "RemoteConsole");
            _clientChannel = RemoteRuntimeServer.CreateChannel(clientChannelName, clientChannelName);
            ChannelServices.RegisterChannel(_clientChannel, false);
        }

        private void OnRemoteRuntimeExited(object sender, EventArgs args) {
            Debug.Assert(_remoteRuntimeProcess.HasExited);
            RemoteRuntimeExited(sender, args);
        }

        #endregion

        protected override CommandLine CreateCommandLine() {
            return new RemoteConsoleCommandLine(_remoteCommandDispatcher, _remoteOutputReceived);
        }

        protected override void UnhandledException(ScriptEngine engine, Exception e) {
            try {
                base.UnhandledException(engine, e);
            } catch (System.Runtime.Remoting.RemotingException) {
                // The remote server may have shutdown. So just do something simple
                Console.Error.Write(e.ToString());
            }
        }
        /// <summary>
        /// Called if the remote runtime process exits by itself. ie. without the remote console killing it.
        /// </summary>
        internal event EventHandler RemoteRuntimeExited;

        /// <summary>
        /// Allows the console to customize the environment variables, working directory, etc.
        /// </summary>
        /// <param name="processInfo">At the least, processInfo.FileName should be initialized</param>
        public abstract void CustomizeRemoteRuntimeStartInfo(ProcessStartInfo processInfo);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")] // TODO: review
        protected virtual void OnOutputDataReceived(object sender, DataReceivedEventArgs eventArgs) {
            if (String.IsNullOrEmpty(eventArgs.Data)) {
                return;
            }

            string output = eventArgs.Data as string;

            if (output.Contains(RemoteCommandDispatcher.OutputCompleteMarker)) {
                Debug.Assert(output == RemoteCommandDispatcher.OutputCompleteMarker);
                _remoteOutputReceived.Set();
            } else {
                Console.WriteLine(output);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")] // TODO: review
        protected virtual void OnErrorDataReceived(object sender, DataReceivedEventArgs eventArgs) {
            if (!String.IsNullOrEmpty(eventArgs.Data)) {
                Console.Error.WriteLine(eventArgs.Data);
            }
        }

        /// <summary>
        /// Aborts the current active call to Execute by doing Thread.Abort
        /// </summary>
        /// <returns>true if a Thread.Abort was actually called. false if there is no active call to Execute</returns>
        public bool AbortCommand() {
            return _remoteCommandDispatcher.AbortCommand();
        }

        public override int Run(string[] args) {
            var runtimeSetup = CreateRuntimeSetup();
            var options = new ConsoleHostOptions();
            ConsoleHostOptionsParser = new ConsoleHostOptionsParser(options, runtimeSetup);

            try {
                ParseHostOptions(args);
            } catch (InvalidOptionException e) {
                Console.Error.WriteLine("Invalid argument: " + e.Message);
                return ExitCode = 1;
            }

            _languageOptionsParser = CreateOptionsParser();

            InitializeRemoteScriptEngine();
            Runtime = Engine.Runtime;

            ExecuteInternal();

            _remoteRuntimeProcess.Exited -= OnRemoteRuntimeExited;
            return ExitCode;
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // dispose managed resources
                _remoteOutputReceived.Close();

                if (_clientChannel != null) {
                    ChannelServices.UnregisterChannel(_clientChannel);
                }

                if (_remoteRuntimeProcess != null && !_remoteRuntimeProcess.HasExited) {
                    _remoteRuntimeProcess.Kill();
                    _remoteRuntimeProcess.WaitForExit();
                }

                if (_serverInitializedEvent != null) {
                    _serverInitializedEvent.Close();
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

#endif