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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Lifetime;
using System.Threading;

namespace Microsoft.Scripting.Hosting.Shell.Remote {
    /// <summary>
    /// The remote runtime server uses this class to publish an initialized ScriptEngine and ScriptRuntime 
    /// over a remoting channel.
    /// </summary>
    public static class RemoteRuntimeServer {
        internal const string RuntimeUriName = "RuntimeUri";
        internal const string CommandDispatcherUri = "CommandDispatcherUri";
        internal const string RemoteRuntimeArg = "-X:RemoteRuntimeChannel";

        private static TimeSpan GetSevenDays() {
            return new TimeSpan(7, 0, 0, 0); // days,hours,mins,secs 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods")] // TODO: Microsoft.Scripting does not need to be APTCA
        internal static IpcChannel CreateChannel(string channelName, string portName) {
            // The Hosting API classes require TypeFilterLevel.Full to be remoted
            BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            System.Collections.IDictionary properties = new System.Collections.Hashtable();
            properties["name"] = channelName;
            properties["portName"] = portName;
            properties["exclusiveAddressUse"] = true;

            // Create the channel.  
            IpcChannel channel = new IpcChannel(properties, null, serverProv);
            return channel;
        }

        /// <summary>
        /// Publish the ScriptEngine so that the host can use it, and then block indefinitely (until the input stream is open)
        /// </summary>
        /// <param name="remoteRuntimeChannelName">The IPC channel that the remote console expects to use to communicate with the ScriptEngine</param>
        /// <param name="engine">A intialized ScriptEngine that is ready to start processing script commands</param>
        internal static void StartServer(string remoteRuntimeChannelName, ScriptEngine engine) {
            Debug.Assert(ChannelServices.GetChannel(remoteRuntimeChannelName) == null);

            IpcChannel channel = CreateChannel("ipc", remoteRuntimeChannelName);

            LifetimeServices.LeaseTime = GetSevenDays();
            LifetimeServices.LeaseManagerPollTime = GetSevenDays();
            LifetimeServices.RenewOnCallTime = GetSevenDays();
            LifetimeServices.SponsorshipTimeout = GetSevenDays();

            ChannelServices.RegisterChannel(channel, false);

            try {
                RemotingServices.Marshal(engine, RuntimeUriName);

                RemoteCommandDispatcher remoteCommandDispatcher = new RemoteCommandDispatcher();
                RemotingServices.Marshal(remoteCommandDispatcher, CommandDispatcherUri);

                // Let the remote console know that the server is ready by using a system-wide synchronization event
                EventWaitHandle serverInitializedEvent = new EventWaitHandle(false, EventResetMode.AutoReset, remoteRuntimeChannelName);
                serverInitializedEvent.Set();

                // Block on Console.In. This is used to determine when the host process exits, since ReadLine will return null then.
                string input = System.Console.ReadLine();
                Debug.Assert(input == null);
            } finally {
                ChannelServices.UnregisterChannel(channel);
            }
        }
    }
}

#endif