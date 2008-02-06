/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;


#if !SILVERLIGHT // System.NET
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;


[assembly: PythonModule("socket", typeof(IronPython.Modules.PythonSocket))]
namespace IronPython.Modules {
    public static class PythonSocket {
        public static string __doc__ = "Implementation module for socket operations.\n\n"
            + "This module is a loose wrapper around the .NET System.Net.Sockets API, so you\n"
            + "may find the corresponding MSDN documentation helpful in decoding error\n"
            + "messages and understanding corner cases.\n"
            + "\n"
            + "This implementation of socket differs slightly from the standard CPython\n"
            + "socket module. Many of these differences are due to the implementation of the\n"
            + ".NET socket libraries. These differences are summarized below. For full\n"
            + "details, check the docstrings of the functions mentioned.\n"
            + " - s.accept(), s.connect(), and s.connect_ex() do not support timeouts.\n"
            + " - Timeouts in s.sendall() don't work correctly.\n"
            + " - makefile() and s.dup() are not implemented.\n"
            + " - getservbyname() and getservbyport() are not implemented.\n"
            + " - SSL support is not implemented."
            + "\n"
            + "An Extra IronPython-specific function is exposed only if the clr module is\n"
            + "imported:\n"
            + " - s.HandleToSocket() returns the System.Net.Sockets.Socket object associated\n"
            + "   with a particular \"file descriptor number\" (as returned by s.fileno()).\n"
            ;

        #region Socket object

        public static PythonType socket = DynamicHelpers.GetPythonTypeFromType(typeof(SocketObj));
        public static PythonType SocketType = socket;

        [PythonType("socket")]
        public class SocketObj : IWeakReferenceable {
            public static string __doc__ = "socket([family[, type[, proto]]]) -> socket object\n\n"
                + "Create a socket (a network connection endpoint) of the given family, type,\n"
                + "and protocol. socket() accepts keyword arguments.\n"
                + " - family (address family) defaults to AF_INET\n"
                + " - type (socket type) defaults to SOCK_STREAM\n"
                + " - proto (protocol type) defaults to 0, which specifies the default protocol\n"
                + "\n"
                + "This module supports only IP sockets. It does not support raw or Unix sockets.\n"
                + "Both IPv4 and IPv6 are supported.";

            #region Fields

            /// <summary>
            /// handleToSocket allows us to translate from Python's idea of a socket resource (file
            /// descriptor numbers) to .NET's idea of a socket resource (System.Net.Socket objects).
            /// In particular, this allows the select module to convert file numbers (as returned by
            /// fileno()) and convert them to Socket objects so that it can do something useful with them.
            /// </summary>
            private static Dictionary<IntPtr, List<Socket>> handleToSocket = new Dictionary<IntPtr, List<Socket>>();

            private const int DefaultAddressFamily = (int)AddressFamily.InterNetwork;
            private const int DefaultSocketType = (int)System.Net.Sockets.SocketType.Stream;
            private const int DefaultProtocolType = (int)ProtocolType.Unspecified;

            internal Socket socket;
            private WeakRefTracker weakRefTracker = null;

            #endregion

            #region Public API

            public SocketObj([DefaultParameterValue(DefaultAddressFamily)] int addressFamily,
                [DefaultParameterValue(DefaultSocketType)] int socketType,
                [DefaultParameterValue(DefaultProtocolType)] int protocolType) {

                System.Net.Sockets.SocketType type = (System.Net.Sockets.SocketType)Enum.ToObject(typeof(System.Net.Sockets.SocketType), socketType);
                if (!Enum.IsDefined(typeof(System.Net.Sockets.SocketType), type)) {
                    throw MakeException(new SocketException((int)SocketError.SocketNotSupported));
                }
                AddressFamily family = (AddressFamily)Enum.ToObject(typeof(AddressFamily), addressFamily);
                if (!Enum.IsDefined(typeof(AddressFamily), family)) {
                    throw MakeException(new SocketException((int)SocketError.AddressFamilyNotSupported));
                }
                ProtocolType proto = (ProtocolType)Enum.ToObject(typeof(ProtocolType), protocolType);
                if (!Enum.IsDefined(typeof(ProtocolType), proto)) {
                    throw MakeException(new SocketException((int)SocketError.ProtocolNotSupported));
                }

                Socket newSocket;
                try {
                    newSocket = new Socket(family, type, proto);
                } catch (SocketException e) {
                    throw MakeException(e);
                }
                Initialize(newSocket);
            }

            [Documentation("accept() -> (conn, address)\n\n"
                + "Accept a connection. The socket must be bound and listening before calling\n"
                + "accept(). conn is a new socket object connected to the remote host, and\n"
                + "address is the remote host's address (e.g. a (host, port) tuple for IPv4).\n"
                + "\n"
                + "Difference from CPython: accept() does not support timeouts in blocking mode.\n"
                + "If a timeout is set and the socket is in blocking mode, accept() will block\n"
                + "indefinitely until a connection is ready."
                )]
            [PythonName("accept")]
            public PythonTuple Accept() {
                SocketObj wrappedRemoteSocket;
                Socket realRemoteSocket;
                try {
                    realRemoteSocket = socket.Accept();
                } catch (Exception e) {
                    throw MakeException(e);
                }
                wrappedRemoteSocket = new SocketObj(realRemoteSocket);
                return PythonTuple.MakeTuple(wrappedRemoteSocket, wrappedRemoteSocket.GetRemoteAddress());
            }

            [Documentation("bind(address) -> None\n\n"
                + "Bind to an address. If the socket is already bound, socket.error is raised.\n"
                + "For IP sockets, address is a (host, port) tuple. Raw sockets are not\n"
                + "supported.\n"
                + "\n"
                + "If you do not care which local address is assigned, set host to INADDR_ANY and\n"
                + "the system will assign the most appropriate network address. Similarly, if you\n"
                + "set port to 0, the system will assign an available port number between 1024\n"
                + "and 5000."
                )]
            [PythonName("bind")]
            public void Bind(PythonTuple address) {
                IPEndPoint localEP = TupleToEndPoint(address, socket.AddressFamily);
                try {
                    socket.Bind(localEP);
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("close() -> None\n\nClose the socket. It cannot be used after being closed.")]
            [PythonName("close")]
            public void Close() {
                lock (handleToSocket) {
                    List<Socket> sockets;
                    if (handleToSocket.TryGetValue((IntPtr)socket.Handle, out sockets)) {
                        if (sockets.Contains(socket)) {
                            sockets.Remove(socket);
                        }
                        if (sockets.Count == 0) {
                            handleToSocket.Remove(socket.Handle);
                        }
                    }
                }
                try {
                    socket.Close();
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("connect(address) -> None\n\n"
                + "Connect to a remote socket at the given address. IP addresses are expressed\n"
                + "as (host, port).\n"
                + "\n"
                + "Raises socket.error if the socket has been closed, the socket is listening, or\n"
                + "another connection error occurred."
                + "\n"
                + "Difference from CPython: connect() does not support timeouts in blocking mode.\n"
                + "If a timeout is set and the socket is in blocking mode, connect() will block\n"
                + "indefinitely until a connection is made or an error occurs."
                )]
            [PythonName("connect")]
            public void Connect(PythonTuple address) {
                IPEndPoint remoteEP = TupleToEndPoint(address, socket.AddressFamily);
                try {
                    socket.Connect(remoteEP);
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("connect_ex(address) -> error_code\n\n"
                + "Like connect(), but return an error code insted of raising an exception for\n"
                + "socket exceptions raised by the underlying system Connect() call. Note that\n"
                + "exceptions other than SocketException generated by the system Connect() call\n"
                + "will still be raised.\n"
                + "\n"
                + "A return value of 0 indicates that the connect call was successful."
                + "\n"
                + "Difference from CPython: connect_ex() does not support timeouts in blocking\n"
                + "mode. If a timeout is set and the socket is in blocking mode, connect_ex() will\n"
                + "block indefinitely until a connection is made or an error occurs."
                )]
            [PythonName("connect_ex")]
            public int ConnectEx(PythonTuple address) {
                IPEndPoint remoteEP = TupleToEndPoint(address, socket.AddressFamily);
                try {
                    socket.Connect(remoteEP);
                } catch (SocketException e) {
                    return e.ErrorCode;
                }
                return (int)SocketError.Success;
            }

            [Documentation("fileno() -> file_handle\n\n"
                + "Return the underlying system handle for this socket (a 64-bit integer)."
                )]
            [PythonName("fileno")]
            public Int64 GetHandle() {
                try {
                    return socket.Handle.ToInt64();
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("getpeername() -> address\n\n"
                + "Return the address of the remote end of this socket. The address format is\n"
                + "family-dependent (e.g. a (host, port) tuple for IPv4)."
                )]
            [PythonName("getpeername")]
            public PythonTuple GetRemoteAddress() {
                try {
                    IPEndPoint remoteEP = socket.RemoteEndPoint as IPEndPoint;
                    if (remoteEP == null) {
                        throw MakeException(new SocketException((int)SocketError.AddressFamilyNotSupported));
                    }
                    return EndPointToTuple(remoteEP);
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("getsockname() -> address\n\n"
                + "Return the address of the local end of this socket. The address format is\n"
                + "family-dependent (e.g. a (host, port) tuple for IPv4)."
                )]
            [PythonName("getsockname")]
            public PythonTuple GetLocalAddress() {
                try {
                    IPEndPoint localEP = socket.LocalEndPoint as IPEndPoint;
                    if (localEP == null) {
                        throw MakeException(new SocketException((int)SocketError.InvalidArgument));
                    }
                    return EndPointToTuple(localEP);
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("getsockopt(level, optname[, buflen]) -> value\n\n"
                + "Return the value of a socket option. level is one of the SOL_* constants\n"
                + "defined in this module, and optname is one of the SO_* constants. If buflen is\n"
                + "omitted or zero, an integer value is returned. If it is present, a byte string\n"
                + "whose maximum length is buflen bytes) is returned. The caller must the decode\n"
                + "the resulting byte string."
                )]
            [PythonName("getsockopt")]
            public object GetSocketOption(int optionLevel, int optionName, [DefaultParameterValue(0)] int optionLength) {
                SocketOptionLevel level = (SocketOptionLevel)Enum.ToObject(typeof(SocketOptionLevel), optionLevel);
                if (!Enum.IsDefined(typeof(SocketOptionLevel), level)) {
                    throw MakeException(new SocketException((int)SocketError.InvalidArgument));
                }
                SocketOptionName name = (SocketOptionName)Enum.ToObject(typeof(SocketOptionName), optionName);
                if (!Enum.IsDefined(typeof(SocketOptionName), name)) {
                    throw MakeException(new SocketException((int)SocketError.ProtocolOption));
                }

                try {
                    if (optionLength == 0) {
                        // Integer return value
                        return (int)socket.GetSocketOption(level, name);
                    } else {
                        // Byte string return value
                        return StringOps.FromByteArray(socket.GetSocketOption(level, name, optionLength));
                    }
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("listen(backlog) -> None\n\n"
                + "Listen for connections on the socket. Backlog is the maximum length of the\n"
                + "pending connections queue. The maximum value is system-dependent."
                )]
            [PythonName("listen")]
            public void Listen(int backlog) {
                try {
                    socket.Listen(backlog);
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }
            
            [Documentation("makefile([mode[, bufsize]]) -> file object\n\n"
                + "Return a regular file object corresponding to the socket.  The mode\n"
                + "and bufsize arguments are as for the built-in open() function.")]
            [PythonName("makefile")]
            public PythonFile MakeFile([DefaultParameterValue("r")]string mode, [DefaultParameterValue(8192)]int bufSize) {
                return new FileObject(this, mode, bufSize);
            }

            [Documentation("recv(bufsize[, flags]) -> string\n\n"
                + "Receive data from the socket, up to bufsize bytes. For connection-oriented\n"
                + "protocols (e.g. SOCK_STREAM), you must first call either connect() or\n"
                + "accept(). Connectionless protocols (e.g. SOCK_DGRAM) may also use recvfrom().\n"
                + "\n"
                + "recv() blocks until data is available, unless a timeout was set using\n"
                + "settimeout(). If the timeout was exceeded, socket.timeout is raised."
                + "recv() returns immediately with zero bytes when the connection is closed."
                )]
            [PythonName("recv")]
            public string Receive(int maxBytes, [DefaultParameterValue(0)] int flags) {
                int bytesRead;
                byte[] buffer = new byte[maxBytes];
                try {
                    bytesRead = socket.Receive(buffer, (SocketFlags)flags);
                } catch (Exception e) {
                    throw MakeException(e);
                }
                return StringOps.FromByteArray(buffer, bytesRead);
            }

            [Documentation("recvfrom(bufsize[, flags]) -> (string, address)\n\n"
                + "Receive data from the socket, up to bufsize bytes. string is the data\n"
                + "received, and address (whose format is protocol-dependent) is the address of\n"
                + "the socket from which the data was received."
                )]
            [PythonName("recvfrom")]
            public PythonTuple ReceiveFrom(int maxBytes, [DefaultParameterValue(0)] int flags) {
                int bytesRead;
                byte[] buffer = new byte[maxBytes];
                IPEndPoint remoteIPEP = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remoteEP = remoteIPEP;
                try {
                    bytesRead = socket.ReceiveFrom(buffer, (SocketFlags)flags, ref remoteEP);
                } catch (Exception e) {
                    throw MakeException(e);
                }
                string data = StringOps.FromByteArray(buffer, bytesRead);
                PythonTuple remoteAddress = EndPointToTuple((IPEndPoint)remoteEP);
                return PythonTuple.MakeTuple(data, remoteAddress);
            }

            [Documentation("send(string[, flags]) -> bytes_sent\n\n"
                + "Send data to the remote socket. The socket must be connected to a remote\n"
                + "socket (by calling either connect() or accept(). Returns the number of bytes\n"
                + "sent to the remote socket.\n"
                + "\n"
                + "Note that the successful completion of a send() call does not mean that all of\n"
                + "the data was sent. The caller must keep track of the number of bytes sent and\n"
                + "retry the operation until all of the data has been sent.\n"
                + "\n"
                + "Also note that there is no guarantee that the data you send will appear on the\n"
                + "network immediately. To increase network efficiency, the underlying system may\n"
                + "delay transmission until a significant amount of outgoing data is collected. A\n"
                + "successful completion of the Send method means that the underlying system has\n"
                + "had room to buffer your data for a network send"
                )]
            [PythonName("send")]
            public int Send(string data, [DefaultParameterValue(0)] int flags) {
                byte[] buffer = StringOps.ToByteArray(data);
                try {
                    return socket.Send(buffer, (SocketFlags)flags);
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("sendall(string[, flags]) -> None\n\n"
                + "Send data to the remote socket. The socket must be connected to a remote\n"
                + "socket (by calling either connect() or accept().\n"
                + "\n"
                + "Unlike send(), sendall() blocks until all of the data has been sent or until a\n"
                + "timeout or an error occurs. None is returned on success. If an error occurs,\n"
                + "there is no way to tell how much data, if any, was sent.\n"
                + "\n"
                + "Difference from CPython: timeouts do not function as you would expect. The\n"
                + "function is implemented using multiple calls to send(), so the timeout timer\n"
                + "is reset after each of those calls. That means that the upper bound on the\n"
                + "time that it will take for sendall() to return is the number of bytes in\n"
                + "string times the timeout interval.\n"
                + "\n"
                + "Also note that there is no guarantee that the data you send will appear on the\n"
                + "network immediately. To increase network efficiency, the underlying system may\n"
                + "delay transmission until a significant amount of outgoing data is collected. A\n"
                + "successful completion of the Send method means that the underlying system has\n"
                + "had room to buffer your data for a network send"
                )]
            [PythonName("sendall")]
            public void SendAll(string data, [DefaultParameterValue(0)] int flags) {
                byte[] buffer = StringOps.ToByteArray(data);
                try {
                    int bytesTotal = buffer.Length;
                    int bytesRemaining = bytesTotal;
                    while (bytesRemaining > 0) {
                        bytesRemaining -= socket.Send(buffer, bytesTotal - bytesRemaining, bytesRemaining, (SocketFlags)flags);
                    }
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("sendto(string[, flags], address) -> bytes_sent\n\n"
                + "Send data to the remote socket. The socket does not need to be connected to a\n"
                + "remote socket since the address is specified in the call to sendto(). Returns\n"
                + "the number of bytes sent to the remote socket.\n"
                + "\n"
                + "Blocking sockets will block until the all of the bytes in the buffer are sent.\n"
                + "Since a nonblocking Socket completes immediately, it might not send all of the\n"
                + "bytes in the buffer. It is your application's responsibility to keep track of\n"
                + "the number of bytes sent and to retry the operation until the application sends\n"
                + "all of the bytes in the buffer.\n"
                + "\n"
                + "Note that there is no guarantee that the data you send will appear on the\n"
                + "network immediately. To increase network efficiency, the underlying system may\n"
                + "delay transmission until a significant amount of outgoing data is collected. A\n"
                + "successful completion of the Send method means that the underlying system has\n"
                + "had room to buffer your data for a network send"
                )]
            [PythonName("sendto")]
            public int SendTo(string data, int flags, PythonTuple address) {
                byte[] buffer = StringOps.ToByteArray(data);
                EndPoint remoteEP = TupleToEndPoint(address, socket.AddressFamily);
                try {
                    return socket.SendTo(buffer, (SocketFlags)flags, remoteEP);
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("")]
            [PythonName("sendto")]
            public int SendTo(string data, PythonTuple address) {
                return SendTo(data, 0, address);
            }

            [Documentation("setblocking(flag) -> None\n\n"
                + "Set the blocking mode of the socket. If flag is 0, the socket will be set to\n"
                + "non-blocking mode; otherwise, it will be set to blocking mode. If the socket is\n"
                + "in blocking mode, and a method is called (such as send() or recv() which does\n"
                + "not complete immediately, the caller will block execution until the requested\n"
                + "operation completes. In non-blocking mode, a socket.timeout exception would\n"
                + "would be raised in this case.\n"
                + "\n"
                + "Note that changing blocking mode also affects the timeout setting:\n"
                + "setblocking(0) is equivalent to settimeout(0), and setblocking(1) is equivalent\n"
                + "to settimeout(None)."
                )]
            [PythonName("setblocking")]
            public void SetBlocking(int shouldBlock) {
                if (shouldBlock == 0) {
                    SetTimeout(0);
                } else {
                    SetTimeout(null);
                }
            }

            [Documentation("settimeout(value) -> None\n\n"
                + "Set a timeout on blocking socket methods. value may be either None or a\n"
                + "non-negative float, with one of the following meanings:\n"
                + " - None: disable timeouts and block indefinitely"
                + " - 0.0: don't block at all (return immediately if the operation can be\n"
                + "   completed; raise socket.error otherwise)\n"
                + " - float > 0.0: block for up to the specified number of seconds; raise\n"
                + "   socket.timeout if the operation cannot be completed in time\n"
                + "\n"
                + "settimeout(None) is equivalent to setblocking(1), and settimeout(0.0) is\n"
                + "equivalent to setblocking(0)."
                + "\n"
                + "If the timeout is non-zero and is less than 0.5, it will be set to 0.5. This\n"
                + "limitation is specific to IronPython.\n"
                )]
            [PythonName("settimeout")]
            public void SetTimeout(object timeout) {
                try {
                    if (timeout == null) {
                        socket.Blocking = true;
                        socket.SendTimeout = 0;
                    } else {
                        double seconds;
                        seconds = Converter.ConvertToDouble(timeout);
                        if (seconds < 0) {
                            throw PythonOps.TypeError("a non-negative float is required");
                        }
                        socket.Blocking = seconds > 0; // 0 timeout means non-blocking mode
                        socket.SendTimeout = (int)(seconds * MillisecondsPerSecond);
                    }
                } finally {
                    socket.ReceiveTimeout = socket.SendTimeout;
                }
            }

            [Documentation("gettimeout() -> value\n\n"
                + "Return the timeout duration in seconds for this socket as a float. If no\n"
                + "timeout is set, return None. For more details on timeouts and blocking, see the\n"
                + "Python socket module documentation."
                )]
            [PythonName("gettimeout")]
            public object GetTimeout() {
                try {
                    if (socket.Blocking && socket.SendTimeout == 0) {
                        return null;
                    } else {
                        return (double)socket.SendTimeout / MillisecondsPerSecond;
                    }
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [Documentation("setsockopt(level, optname[, value]) -> None\n\n"
                + "Set the value of a socket option. level is one of the SOL_* constants defined\n"
                + "in this module, and optname is one of the SO_* constants. value may be either\n"
                + "an integer or a string containing a binary structure. The caller is responsible\n"
                + "for properly encoding the byte string."
                )]
            [PythonName("setsockopt")]
            public void SetSocketOption(int optionLevel, int optionName, object value) {
                SocketOptionLevel level = (SocketOptionLevel)Enum.ToObject(typeof(SocketOptionLevel), optionLevel);
                if (!Enum.IsDefined(typeof(SocketOptionLevel), level)) {
                    throw MakeException(new SocketException((int)SocketError.InvalidArgument));
                }
                SocketOptionName name = (SocketOptionName)Enum.ToObject(typeof(SocketOptionName), optionName);
                if (!Enum.IsDefined(typeof(SocketOptionName), name)) {
                    throw MakeException(new SocketException((int)SocketError.ProtocolOption));
                }

                try {
                    int intValue;
                    if (Converter.TryConvertToInt32(value, out intValue)) {
                        socket.SetSocketOption(level, name, intValue);
                        return;
                    }

                    string strValue;
                    if (Converter.TryConvertToString(value, out strValue)) {
                        socket.SetSocketOption(level, name, StringOps.ToByteArray(strValue));
                        return;
                    }
                } catch (Exception e) {
                    throw MakeException(e);
                }

                throw PythonOps.TypeError("setsockopt() argument 3 must be int or string");
            }

            [Documentation("shutdown() -> None\n\n"
                + "Return the timeout duration in seconds for this socket as a float. If no\n"
                + "timeout is set, return None. For more details on timeouts and blocking, see the\n"
                + "Python socket module documentation."
                )]
            [PythonName("shutdown")]
            public void Shutdown(int how) {
                SocketShutdown howValue = (SocketShutdown)Enum.ToObject(typeof(SocketShutdown), how);
                if (!Enum.IsDefined(typeof(SocketShutdown), howValue)) {
                    throw MakeException(new SocketException((int)SocketError.InvalidArgument));
                }
                try {
                    socket.Shutdown(howValue);
                } catch (Exception e) {
                    throw MakeException(e);
                }
            }

            [PythonName("__repr__")]
            public override string ToString() {
                try {
                    return "<socket object, fd=" + GetHandle().ToString()
                        + ", family=" + ((int)socket.AddressFamily).ToString()
                        + ", type=" + ((int)socket.SocketType).ToString()
                        + ", protocol=" + ((int)socket.ProtocolType).ToString()
                        + ">"
                    ;
                } catch {
                    return "<socket object, fd=?, family=?, type=, protocol=>";
                }
            }

            /// <summary>
            /// Return the internal System.Net.Sockets.Socket socket object associated with the given
            /// handle (as returned by GetHandle()), or null if no corresponding socket exists. This is
            /// primarily intended to be used by other modules (such as select) that implement
            /// networking primitives. User code should not normally need to call this function.
            /// </summary>
            public static Socket HandleToSocket(Int64 handle) {
                List<Socket> sockets;
                lock (handleToSocket) {
                    if (handleToSocket.TryGetValue((IntPtr)handle, out sockets)) {
                        if (sockets.Count > 0) {
                            return sockets[0];
                        }
                    }
                }
                return null;
            }

            #endregion

            #region IWeakReferenceable Implementation

            public WeakRefTracker GetWeakRef() {
                return weakRefTracker;
            }

            public bool SetWeakRef(WeakRefTracker value) {
                weakRefTracker = value;
                return true;
            }

            public void SetFinalizer(WeakRefTracker value) {
                weakRefTracker = value;
            }

            #endregion

            #region Private Implementation

            /// <summary>
            /// Create a Python socket object from an existing .NET socket object
            /// (like one returned from Socket.Accept())
            /// </summary>
            private SocketObj(Socket socket) {
                Initialize(socket);
            }

            /// <summary>
            /// Perform initialization common to all constructors
            /// </summary>
            private void Initialize(Socket socket) {
                this.socket = socket;
                if (DefaultTimeout == null) {
                    SetTimeout(null);
                } else {
                    SetTimeout((double)DefaultTimeout / MillisecondsPerSecond);
                }
                lock (handleToSocket) {
                    if (!handleToSocket.ContainsKey(socket.Handle)) {
                        handleToSocket[socket.Handle] = new List<Socket>(1);
                    }
                    if (!handleToSocket[socket.Handle].Contains(socket)) {
                        handleToSocket[socket.Handle].Add(socket);
                    }
                }
            }

            #endregion

        }

        #endregion

        #region Fields

        public static PythonType error = PythonExceptions.CreateSubType(PythonExceptions.Exception, "error", "socket", "");
        public static PythonType herror = PythonExceptions.CreateSubType(error, "herror", "socket", "");
        public static PythonType gaierror = PythonExceptions.CreateSubType(error, "gaierror", "socket", "");
        public static PythonType timeout = PythonExceptions.CreateSubType(error, "timeout", "socket", "");       

        private static int? DefaultTimeout = null; // in milliseconds

        private const string AnyAddrToken = "";
        private const string BroadcastAddrToken = "<broadcast>";
        private const string LocalhostAddrToken = "";
        private const int IPv4AddrBytes = 4;
        private const int IPv6AddrBytes = 16;
        private const double MillisecondsPerSecond = 1000.0;

        #endregion

        #region Public API

        [Documentation("")]
        [PythonName("getaddrinfo")]
        public static List GetAddrInfo(
            string host,
            object port,
            [DefaultParameterValue((int)AddressFamily.Unspecified)] int family,
            [DefaultParameterValue(0)] int socktype,
            [DefaultParameterValue((int)ProtocolType.IP)] int proto,
            [DefaultParameterValue((int)SocketFlags.None)] int flags
        ) {
            int numericPort;
            
            if (port == null) {
                numericPort = 0;
            } else if (port is int) {
                numericPort = (int)port;
            } else if (port is ExtensibleInt) {
                numericPort = ((ExtensibleInt)port).Value;
            } else if (port is string) {
                if (!Int32.TryParse((string)port, out numericPort)) {
                    // TODO: also should consult GetServiceByName                    
                    throw PythonExceptions.CreateThrowable(gaierror, "getaddrinfo failed");
                }
            } else if (port is ExtensibleString) {
                if (!Int32.TryParse(((ExtensibleString)port).Value, out numericPort)) {
                    // TODO: also should consult GetServiceByName                    
                    throw PythonExceptions.CreateThrowable(gaierror, "getaddrinfo failed");
                }
            } else {
                throw PythonExceptions.CreateThrowable(gaierror, "getaddrinfo failed");
            }

            if (socktype != 0) {
                // we just use this to validate; socketType isn't actually used
                System.Net.Sockets.SocketType socketType = (System.Net.Sockets.SocketType)Enum.ToObject(typeof(System.Net.Sockets.SocketType), socktype);
                if (socketType == System.Net.Sockets.SocketType.Unknown || !Enum.IsDefined(typeof(System.Net.Sockets.SocketType), socketType)) {
                    throw PythonExceptions.CreateThrowable(gaierror, PythonTuple.MakeTuple((int)SocketError.SocketNotSupported, "getaddrinfo failed"));
                }
            }

            AddressFamily addressFamily = (AddressFamily)Enum.ToObject(typeof(AddressFamily), family);
            if (!Enum.IsDefined(typeof(AddressFamily), addressFamily)) {
                throw PythonExceptions.CreateThrowable(gaierror, PythonTuple.MakeTuple((int)SocketError.AddressFamilyNotSupported, "getaddrinfo failed"));
            }

            // Again, we just validate, but don't actually use protocolType
            ProtocolType protocolType = (ProtocolType)Enum.ToObject(typeof(ProtocolType), proto);

            IPAddress[] ips = HostToAddresses(host, addressFamily);

            List results = new List();

            foreach (IPAddress ip in ips) {
                results.Add(PythonTuple.MakeTuple(
                    (int)ip.AddressFamily,
                    socktype,
                    proto,
                    "",
                    EndPointToTuple(new IPEndPoint(ip, numericPort))
                ));
            }

            return results;
        }

        [Documentation("getfqdn([hostname_or_ip]) -> hostname\n\n"
            + "Return the fully-qualified domain name for the specified hostname or IP\n"
            + "address. An unspecified or empty name is interpreted as the local host. If the\n"
            + "name lookup fails, the passed-in name is returned as-is."
            )]
        [PythonName("getfqdn")]
        public static string GetFQDN(string host) {
            host = host.Trim();
            if (host == BroadcastAddrToken) {
                return host;
            }
            try {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);
                if (hostEntry.HostName.Contains(".")) {
                    return hostEntry.HostName;
                } else {
                    foreach (string addr in hostEntry.Aliases) {
                        if (addr.Contains(".")) {
                            return addr;
                        }
                    }
                }
            } catch (SocketException) {
                // ignore and return host below
            }
            // seems to match CPython behavior, although docs say gethostname() should be returned
            return host;
        }

        [Documentation("")]
        [PythonName("getfqdn")]
        public static string GetFQDN() {
            return GetFQDN(LocalhostAddrToken);
        }

        [Documentation("gethostbyname(hostname) -> ip address\n\n"
            + "Return the string IPv4 address associated with the given hostname (e.g.\n"
            + "'10.10.0.1'). The hostname is returned as-is if it an IPv4 address. The empty\n"
            + "string is treated as the local host.\n"
            + "\n"
            + "gethostbyname() doesn't support IPv6; for IPv4/IPv6 support, use getaddrinfo()."
            )]
        [PythonName("gethostbyname")]
        public static string GetHostByName(string host) {
            return HostToAddress(host, AddressFamily.InterNetwork).ToString();
        }

        [Documentation("gethostbyname_ex(hostname) -> (hostname, aliases, ip_addresses)\n\n"
            + "Return the real host name, a list of aliases, and a list of IP addresses\n"
            + "associated with the given hostname. If the hostname is an IPv4 address, the\n"
            + "tuple ([hostname, [], [hostname]) is returned without doing a DNS lookup.\n"
            + "\n"
            + "gethostbyname_ex() doesn't support IPv6; for IPv4/IPv6 support, use\n"
            + "getaddrinfo()."
            )]
        [PythonName("gethostbyname_ex")]
        public static PythonTuple GetHostByNameEx(string host) {
            string hostname;
            List aliases;
            List ips = List.Make();

            IPAddress addr;
            if (IPAddress.TryParse(host, out addr)) {
                if (AddressFamily.InterNetwork == addr.AddressFamily) {
                    hostname = host;
                    aliases = List.MakeEmptyList(0);
                    ips.Append(host);
                } else {
                    throw PythonExceptions.CreateThrowable(gaierror, (int)SocketError.HostNotFound, "no IPv4 addresses associated with host");
                }
            } else {
                IPHostEntry hostEntry;
                try {
                    hostEntry = Dns.GetHostEntry(host);
                } catch (SocketException e) {
                    throw PythonExceptions.CreateThrowable(gaierror, e.ErrorCode, "no IPv4 addresses associated with host");
                }
                hostname = hostEntry.HostName;
                aliases = List.Make(hostEntry.Aliases);
                foreach (IPAddress ip in hostEntry.AddressList) {
                    if (AddressFamily.InterNetwork == ip.AddressFamily) {
                        ips.Append(ip.ToString());
                    }
                }
            }

            return PythonTuple.MakeTuple(hostname, aliases, ips);
        }

        [Documentation("gethostname() -> hostname\nReturn this machine's hostname")]
        [PythonName("gethostname")]
        public static string GetHostName() {
            return Dns.GetHostName();
        }

        [Documentation("gethostbyaddr(host) -> (hostname, aliases, ipaddrs)\n\n"
            + "Return a tuple of (primary hostname, alias hostnames, ip addresses). host may\n"
            + "be either a hostname or an IP address."
            )]
        [PythonName("gethostbyaddr")]
        public static object GetHostByAddr(string host) {
            if (host == "") {
                host = GetHostName();
            }
            // This conversion seems to match CPython behavior
            host = GetHostByName(host);

            IPAddress[] ips = null;
            IPHostEntry hostEntry = null;
            try {
                ips = Dns.GetHostAddresses(host);
                hostEntry = Dns.GetHostEntry(host);
            } catch (Exception e) {
                throw MakeException(e);
            }

            List ipStrings = List.Make();
            foreach (IPAddress ip in ips) {
                ipStrings.Append(ip.ToString());
            }

            return PythonTuple.MakeTuple(hostEntry.HostName, List.Make(hostEntry.Aliases), ipStrings);
        }

        [Documentation("getnameinfo(socketaddr, flags) -> (host, port)\n"
            + "Given a socket address, the return a tuple of the corresponding hostname and\n"
            + "port. Available flags:\n"
            + " - NI_NOFQDN: Return only the hostname part of the domain name for hosts on the\n"
            + "   same domain as the executing machine.\n"
            + " - NI_NUMERICHOST: return the numeric form of the host (e.g. '127.0.0.1' or\n"
            + "   '::1' rather than 'localhost').\n"
            + " - NI_NAMEREQD: Raise an error if the hostname cannot be looked up.\n"
            + " - NI_NUMERICSERV: Return string containing the numeric form of the port (e.g.\n"
            + "   '80' rather than 'http'). This flag is required (see below).\n"
            + " - NI_DGRAM: Silently ignored (see below).\n"
            + "\n"
            + "Difference from CPython: the following flag behavior differs from CPython\n"
            + "because the .NET framework libraries offer no name-to-port conversion APIs:\n"
            + " - NI_NUMERICSERV: This flag is required because the .NET framework libraries\n"
            + "   offer no port-to-name mapping APIs. If it is omitted, getnameinfo() will\n"
            + "   raise a NotImplementedError.\n"
            + " - The NI_DGRAM flag is ignored because it only applies when NI_NUMERICSERV is\n"
            + "   omitted. It it were supported, it would return the UDP-based port name\n"
            + "   rather than the TCP-based port name.\n"
            )]
        [PythonName("getnameinfo")]
        public static object GetNameInfo(PythonTuple socketAddr, int flags) {
            if (socketAddr.GetLength() < 2 || socketAddr.GetLength() > 4) {
                throw PythonOps.TypeError("socket address must be a 2-tuple (IPv4 or IPv6) or 4-tuple (IPv6)");
            }

            if ((flags & (int)NI_NUMERICSERV) == 0) {
                throw PythonOps.NotImplementedError("getnameinfo() required the NI_NUMERICSERV flag (see docstring)");
            }

            string host = Converter.ConvertToString(socketAddr[0]);
            if (host == null) throw PythonOps.TypeError("argument 1 must be string");
            int port = 0;
            try {
                port = (int)socketAddr[1];
            } catch (InvalidCastException) {
                throw PythonOps.TypeError("an integer is required");
            }

            string resultHost = null;
            string resultPort = null;

            // Host
            IPHostEntry hostEntry = null;
            try {
                // Do double lookup to force reverse DNS lookup to match CPython behavior
                hostEntry = Dns.GetHostEntry(host);
                if (hostEntry.AddressList.Length < 1) {
                    throw PythonExceptions.CreateThrowable(error, "sockaddr resolved to zero addresses");
                }
                hostEntry = Dns.GetHostEntry(hostEntry.AddressList[0]);
            } catch (SocketException e) {
                throw PythonExceptions.CreateThrowable(gaierror, e.ErrorCode, e.Message);
            } catch (IndexOutOfRangeException) {
                throw PythonExceptions.CreateThrowable(gaierror, "sockaddr resolved to zero addresses");
            }

            if (hostEntry.AddressList.Length > 1) {
                throw PythonExceptions.CreateThrowable(error, "sockaddr resolved to multiple addresses");
            } else if (hostEntry.AddressList.Length < 1) {
                throw PythonExceptions.CreateThrowable(error, "sockaddr resolved to zero addresses");
            }

            if ((flags & (int)NI_NUMERICHOST) != 0) {
                resultHost = hostEntry.AddressList[0].ToString();
            } else if ((flags & (int)NI_NOFQDN) != 0) {
                resultHost = RemoveLocalDomain(hostEntry.HostName);
            } else {
                resultHost = hostEntry.HostName;
            }

            // Port
            // We don't branch on NI_NUMERICSERV here since we throw above if it's not set
            resultPort = port.ToString();

            return PythonTuple.MakeTuple(resultHost, resultPort);
        }

        [Documentation("getprotobyname(protoname) -> integer proto\n\n"
            + "Given a string protocol name (e.g. \"udp\"), return the associated integer\n"
            + "protocol number, suitable for passing to socket(). The name is case\n"
            + "insensitive.\n"
            + "\n"
            + "Raises socket.error if no protocol number can be found."
            )]
        [PythonName("getprotobyname")]
        public static object GetProtoByName(string protocolName) {
            switch (protocolName.ToLower()) {
                case "ah": return IPPROTO_AH;
                case "esp": return IPPROTO_ESP;
                case "dstopts": return IPPROTO_DSTOPTS;
                case "fragment": return IPPROTO_FRAGMENT;
                case "ggp": return IPPROTO_GGP;
                case "icmp": return IPPROTO_ICMP;
                case "icmpv6": return IPPROTO_ICMPV6;
                case "ip": return IPPROTO_IP;
                case "ipv4": return IPPROTO_IPV4;
                case "ipv6": return IPPROTO_IPV6;
                case "nd": return IPPROTO_ND;
                case "none": return IPPROTO_NONE;
                case "pup": return IPPROTO_PUP;
                case "raw": return IPPROTO_RAW;
                case "routing": return IPPROTO_ROUTING;
                case "tcp": return IPPROTO_TCP;
                case "udp": return IPPROTO_UDP;
                default:
                    throw PythonExceptions.CreateThrowable(error, "protocol not found");
            }
        }

        [Documentation("getservbyname(service_name[, protocol_name]) -> port\n\n"
            + "Not implemented."
            //+ "Given a service name (e.g. 'domain') return the associated protocol number (e.g.\n"
            //+ "53). The protocol name (if specified) must be either 'tcp' or 'udp'."
            )]
        [PythonName("getservbyname")]
        public static int GetServiceByName(string serviceName, [DefaultParameterValue(null)] string protocolName) {
            // !!! .NET networking libraries don't support this, so we don't either
            throw PythonOps.NotImplementedError("name to service conversion not supported");
        }

        [Documentation("getservbyport(port[, protocol_name]) -> service_name\n\n"
            + "Not implemented."
            //+ "Given a port number (e.g. 53), return the associated protocol name (e.g.\n"
            //+ "'domain'). The protocol name (if specified) must be either 'tcp' or 'udp'."
            )]
        [PythonName("getservbyport")]
        public static string GetServiceByPort(int port, [DefaultParameterValue(null)] string protocolName) {
            // !!! .NET networking libraries don't support this, so we don't either
            throw PythonOps.NotImplementedError("service to name conversion not supported");
        }

        [Documentation("ntohl(x) -> integer\n\nConvert a 32-bit integer from network byte order to host byte order.")]
        [PythonName("ntohl")]
        public static int NetworkToHostOrder32(object x) {
            return IPAddress.NetworkToHostOrder(SignInsenstitiveToInt32(x));
        }

        [Documentation("ntohs(x) -> integer\n\nConvert a 16-bit integer from network byte order to host byte order.")]
        [PythonName("ntohs")]
        public static short NetworkToHostOrder16(object x) {
            return IPAddress.NetworkToHostOrder(SignInsenstitiveToInt16(x));
        }

        [Documentation("htonl(x) -> integer\n\nConvert a 32bit integer from host byte order to network byte order.")]
        [PythonName("htonl")]
        public static int HostToNetworkOrder32(object x) {
            return IPAddress.HostToNetworkOrder(SignInsenstitiveToInt32(x));
        }

        /// <summary>
        /// Convert an object to a 32-bit integer. This adds two features to Converter.ToInt32:
        ///   1. Sign is ignored. For example, 0xffff0000 converts to 4294901760, where Convert.ToInt32
        ///      would throw because 0xffff000 is less than zero.
        ///   2. Overflow exceptions are thrown. Converter.ToInt32 throws TypeError if x is
        ///      an integer, but is bigger than 32 bits. Instead, we throw OverflowException.
        /// </summary>
        private static int SignInsenstitiveToInt32(object x) {
            BigInteger bigValue = Converter.ConvertToBigInteger(x);
            try {
                return bigValue.ToInt32(null);
            } catch (OverflowException) {
                return (int)bigValue.ToUInt32(null);
            }
        }

        /// <summary>
        /// Convert an object to a 16-bit integer. This adds two features to Converter.ToInt16:
        ///   1. Sign is ignored. For example, 0xff00 converts to 65280, where Convert.ToInt16
        ///      would throw because signed 0xff00 is -256.
        ///   2. Overflow exceptions are thrown. Converter.ToInt16 throws TypeError if x is
        ///      an integer, but is bigger than 16 bits. Instead, we throw OverflowException.
        /// </summary>
        private static short SignInsenstitiveToInt16(object x) {
            BigInteger bigValue = Converter.ConvertToBigInteger(x);
            try {
                return bigValue.ToInt16(null);
            } catch (OverflowException) {
                return (short)bigValue.ToUInt16(null);
            }
        }

        [Documentation("htons(x) -> integer\n\nConvert a 16-bit integer from host byte order to network byte order.")]
        [PythonName("htons")]
        public static short HostToNetworkOrder16(object x) {
            return IPAddress.HostToNetworkOrder(SignInsenstitiveToInt16(x));
        }

        [Documentation("inet_pton(addr_family, ip_string) -> packed_ip\n\n"
            + "Convert an IP address (in string format, e.g. '127.0.0.1' or '::1') to a 32-bit\n"
            + "packed binary format, as 4-byte (IPv4) or 16-byte (IPv6) string. The return\n"
            + "format matches the format of the standard C library's in_addr or in6_addr\n"
            + "struct.\n"
            + "\n"
            + "If the address format is invalid, socket.error will be raised. Validity is\n"
            + "determined by the .NET System.Net.IPAddress.Parse() method.\n"
            + "\n"
            + "inet_pton() supports IPv4 and IPv6."
            )]
        [PythonName("inet_pton")]
        public static string IPStringToPackedBytes(int addressFamily, string ipString) {
            if (addressFamily != (int)AddressFamily.InterNetwork && addressFamily != (int)AddressFamily.InterNetworkV6) {
                throw MakeException(new SocketException((int)SocketError.AddressFamilyNotSupported));
            }

            IPAddress ip;
            try {
                ip = IPAddress.Parse(ipString);
                if (addressFamily != (int)ip.AddressFamily) {
                    throw MakeException(new SocketException((int)SocketError.AddressFamilyNotSupported));
                }
            } catch (FormatException) {
                throw PythonExceptions.CreateThrowable(error, "illegal IP address passed to inet_pton");
            }
            return StringOps.FromByteArray(ip.GetAddressBytes());
        }

        [Documentation("inet_ntop(address_family, packed_ip) -> ip_string\n\n"
            + "Convert a packed IP address (a 4-byte [IPv4] or 16-byte [IPv6] string) to a\n"
            + "string IP address (e.g. '127.0.0.1' or '::1').\n"
            + "\n"
            + "The input format matches the format of the standard C library's in_addr or\n"
            + "in6_addr struct. If the input string is not exactly 4 bytes or 16 bytes,\n"
            + "socket.error will be raised.\n"
            + "\n"
            + "inet_ntop() supports IPv4 and IPv6."
            )]
        [PythonName("inet_ntop")]
        public static string IPPackedBytesToString(int addressFamily, string packedIP) {
            if (!(
                (packedIP.Length == IPv4AddrBytes && addressFamily == (int)AddressFamily.InterNetwork)
                || (packedIP.Length == IPv6AddrBytes && addressFamily == (int)AddressFamily.InterNetworkV6)
            )) {
                throw PythonExceptions.CreateThrowable(error, "invalid length of packed IP address string");
            }
            byte[] ipBytes = StringOps.ToByteArray(packedIP);
            if (addressFamily == (int)AddressFamily.InterNetworkV6) {
                return IPv6BytesToColonHex(ipBytes);
            }
            return (new IPAddress(ipBytes)).ToString();
        }

        [Documentation("inet_aton(ip_string) -> packed_ip\n"
            + "Convert an IP address (in string dotted quad format, e.g. '127.0.0.1') to a\n"
            + "32-bit packed binary format, as four-character string. The return format\n"
            + "matches the format of the standard C library's in_addr struct.\n"
            + "\n"
            + "If the address format is invalid, socket.error will be raised. Validity is\n"
            + "determined by the .NET System.Net.IPAddress.Parse() method.\n"
            + "\n"
            + "inet_aton() supports only IPv4."
            )]
        [PythonName("inet_aton")]
        public static string IPStringToPackedBytes(string ipString) {
            return IPStringToPackedBytes((int)AddressFamily.InterNetwork, ipString);
        }

        [Documentation("inet_ntoa(packed_ip) -> ip_string\n\n"
            + "Convert a packed IP address (a 4-byte string) to a string IP address (in dotted\n"
            + "quad format, e.g. '127.0.0.1'). The input format matches the format of the\n"
            + "standard C library's in_addr struct.\n"
            + "\n"
            + "If the input string is not exactly 4 bytes, socket.error will be raised.\n"
            + "\n"
            + "inet_ntoa() supports only IPv4."
            )]
        [PythonName("inet_ntoa")]
        public static string IPPackedBytesToString(string packedIP) {
            return IPPackedBytesToString((int)AddressFamily.InterNetwork, packedIP);
        }

        [Documentation("getdefaulttimeout() -> timeout\n\n"
            + "Return the default timeout for new socket objects in seconds as a float. A\n"
            + "value of None means that sockets have no timeout and begin in blocking mode.\n"
            + "The default value when the module is imported is None."
            )]
        [PythonName("getdefaulttimeout")]
        public static object GetDefaultTimeout() {
            if (DefaultTimeout == null) {
                return null;
            } else {
                return (double)(DefaultTimeout.Value) / MillisecondsPerSecond;
            }
        }

        [Documentation("setdefaulttimeout(timeout) -> None\n\n"
            + "Set the default timeout for new socket objects. timeout must be either None,\n"
            + "meaning that sockets have no timeout and start in blocking mode, or a\n"
            + "non-negative float that specifies the default timeout in seconds."
            )]
        [PythonName("setdefaulttimeout")]
        public static void SetDefaultTimeout(object timeout) {
            if (timeout == null) {
                DefaultTimeout = null;
            } else {
                double seconds;
                seconds = Converter.ConvertToDouble(timeout);
                if (seconds < 0) {
                    throw PythonOps.ValueError("a non-negative float is required");
                }
                DefaultTimeout = (int)(seconds * MillisecondsPerSecond);
            }
        }

        #endregion

        #region Exported constants

        public static object AF_APPLETALK = (int)AddressFamily.AppleTalk;
        public static object AF_DECnet = (int)AddressFamily.DecNet;
        public static object AF_INET = (int)AddressFamily.InterNetwork;
        public static object AF_INET6 = (int)AddressFamily.InterNetworkV6;
        public static object AF_IPX = (int)AddressFamily.Ipx;
        public static object AF_IRDA = (int)AddressFamily.Irda;
        public static object AF_SNA = (int)AddressFamily.Sna;
        public static object AF_UNSPEC = (int)AddressFamily.Unspecified;
        public static object AI_CANONNAME = (int)0x2;
        public static object AI_NUMERICHOST = (int)0x4;
        public static object AI_PASSIVE = (int)0x1;
        public static object EAI_AGAIN = (int)SocketError.TryAgain;
        public static object EAI_BADFLAGS = (int)SocketError.InvalidArgument;
        public static object EAI_FAIL = (int)SocketError.NoRecovery;
        public static object EAI_FAMILY = (int)SocketError.AddressFamilyNotSupported;
        public static object EAI_MEMORY = (int)SocketError.NoBufferSpaceAvailable;
        public static object EAI_NODATA = (int)SocketError.HostNotFound; // not SocketError.NoData, like you would think
        public static object EAI_NONAME = (int)SocketError.HostNotFound;
        public static object EAI_SERVICE = (int)SocketError.TypeNotFound;
        public static object EAI_SOCKTYPE = (int)SocketError.SocketNotSupported;
        public static object EAI_SYSTEM = (int)SocketError.SocketError;
        public static object EBADF = (int)0x9;
        public static object INADDR_ALLHOSTS_GROUP = unchecked((int)0xe0000001);
        public static object INADDR_ANY = (int)0x00000000;
        public static object INADDR_BROADCAST = unchecked((int)0xFFFFFFFF);
        public static object INADDR_LOOPBACK = unchecked((int)0x7F000001);
        public static object INADDR_MAX_LOCAL_GROUP = unchecked((int)0xe00000FF);
        public static object INADDR_NONE = unchecked((int)0xFFFFFFFF);
        public static object INADDR_UNSPEC_GROUP = unchecked((int)0xE0000000);
        public static object IPPORT_RESERVED = 1024;
        public static object IPPORT_USERRESERVED = 5000;
        public static object IPPROTO_AH = (int)ProtocolType.IPSecAuthenticationHeader;
        public static object IPPROTO_DSTOPTS = (int)ProtocolType.IPv6DestinationOptions;
        public static object IPPROTO_ESP = (int)ProtocolType.IPSecEncapsulatingSecurityPayload;
        public static object IPPROTO_FRAGMENT = (int)ProtocolType.IPv6FragmentHeader;
        public static object IPPROTO_GGP = (int)ProtocolType.Ggp;
        public static object IPPROTO_HOPOPTS = (int)ProtocolType.IPv6HopByHopOptions;
        public static object IPPROTO_ICMP = (int)ProtocolType.Icmp;
        public static object IPPROTO_ICMPV6 = (int)ProtocolType.IcmpV6;
        public static object IPPROTO_IDP = (int)ProtocolType.Idp;
        public static object IPPROTO_IGMP = (int)ProtocolType.Igmp;
        public static object IPPROTO_IP = (int)ProtocolType.IP;
        public static object IPPROTO_IPV4 = (int)ProtocolType.IPv4;
        public static object IPPROTO_IPV6 = (int)ProtocolType.IPv6;
        public static object IPPROTO_MAX = 256;
        public static object IPPROTO_ND = (int)ProtocolType.ND;
        public static object IPPROTO_NONE = (int)ProtocolType.IPv6NoNextHeader;
        public static object IPPROTO_PUP = (int)ProtocolType.Pup;
        public static object IPPROTO_RAW = (int)ProtocolType.Raw;
        public static object IPPROTO_ROUTING = (int)ProtocolType.IPv6RoutingHeader;
        public static object IPPROTO_TCP = (int)ProtocolType.Tcp;
        public static object IPPROTO_UDP = (int)ProtocolType.Udp;
        public static object IPV6_HOPLIMIT = (int)SocketOptionName.HopLimit;
        public static object IPV6_JOIN_GROUP = (int)SocketOptionName.AddMembership;
        public static object IPV6_LEAVE_GROUP = (int)SocketOptionName.DropMembership;
        public static object IPV6_MULTICAST_HOPS = (int)SocketOptionName.MulticastTimeToLive;
        public static object IPV6_MULTICAST_IF = (int)SocketOptionName.MulticastInterface;
        public static object IPV6_MULTICAST_LOOP = (int)SocketOptionName.MulticastLoopback;
        public static object IPV6_PKTINFO = (int)SocketOptionName.PacketInformation;
        public static object IPV6_UNICAST_HOPS = (int)SocketOptionName.IpTimeToLive;
        public static object IP_ADD_MEMBERSHIP = (int)SocketOptionName.AddMembership;
        public static object IP_DROP_MEMBERSHIP = (int)SocketOptionName.DropMembership;
        public static object IP_HDRINCL = (int)SocketOptionName.HeaderIncluded;
        public static object IP_MULTICAST_IF = (int)SocketOptionName.MulticastInterface;
        public static object IP_MULTICAST_LOOP = (int)SocketOptionName.MulticastLoopback;
        public static object IP_MULTICAST_TTL = (int)SocketOptionName.MulticastTimeToLive;
        public static object IP_OPTIONS = (int)SocketOptionName.IPOptions;
        public static object IP_TOS = (int)SocketOptionName.TypeOfService;
        public static object IP_TTL = (int)SocketOptionName.IpTimeToLive;
        public static object MSG_DONTROUTE = (int)SocketFlags.DontRoute;
        public static object MSG_OOB = (int)SocketFlags.OutOfBand;
        public static object MSG_PEEK = (int)SocketFlags.Peek;
        public static object NI_DGRAM = 0x0010;
        public static object NI_MAXHOST = 1025;
        public static object NI_MAXSERV = 32;
        public static object NI_NAMEREQD = 0x0004;
        public static object NI_NOFQDN = 0x0001;
        public static object NI_NUMERICHOST = 0x0002;
        public static object NI_NUMERICSERV = 0x0008;
        public static object SHUT_RD = (int)SocketShutdown.Receive;
        public static object SHUT_RDWR = (int)SocketShutdown.Both;
        public static object SHUT_WR = (int)SocketShutdown.Send;
        public static object SOCK_DGRAM = (int)System.Net.Sockets.SocketType.Dgram;
        public static object SOCK_RAW = (int)System.Net.Sockets.SocketType.Raw;
        public static object SOCK_RDM = (int)System.Net.Sockets.SocketType.Rdm;
        public static object SOCK_SEQPACKET = (int)System.Net.Sockets.SocketType.Seqpacket;
        public static object SOCK_STREAM = (int)System.Net.Sockets.SocketType.Stream;
        public static object SOL_IP = (int)SocketOptionLevel.IP;
        public static object SOL_IPV6 = (int)SocketOptionLevel.IPv6;
        public static object SOL_SOCKET = (int)SocketOptionLevel.Socket;
        public static object SOL_TCP = (int)SocketOptionLevel.Tcp;
        public static object SOL_UDP = (int)SocketOptionLevel.Udp;
        public static object SOMAXCONN = (int)SocketOptionName.MaxConnections;
        public static object SO_ACCEPTCONN = (int)SocketOptionName.AcceptConnection;
        public static object SO_BROADCAST = (int)SocketOptionName.Broadcast;
        public static object SO_DEBUG = (int)SocketOptionName.Debug;
        public static object SO_DONTROUTE = (int)SocketOptionName.DontRoute;
        public static object SO_ERROR = (int)SocketOptionName.Error;
        public static object SO_EXCLUSIVEADDRUSE = (int)SocketOptionName.ExclusiveAddressUse;
        public static object SO_KEEPALIVE = (int)SocketOptionName.KeepAlive;
        public static object SO_LINGER = (int)SocketOptionName.Linger;
        public static object SO_OOBINLINE = (int)SocketOptionName.OutOfBandInline;
        public static object SO_RCVBUF = (int)SocketOptionName.ReceiveBuffer;
        public static object SO_RCVLOWAT = (int)SocketOptionName.ReceiveLowWater;
        public static object SO_RCVTIMEO = (int)SocketOptionName.ReceiveTimeout;
        public static object SO_REUSEADDR = (int)SocketOptionName.ReuseAddress;
        public static object SO_SNDBUF = (int)SocketOptionName.SendBuffer;
        public static object SO_SNDLOWAT = (int)SocketOptionName.SendLowWater;
        public static object SO_SNDTIMEO = (int)SocketOptionName.SendTimeout;
        public static object SO_TYPE = (int)SocketOptionName.Type;
        public static object SO_USELOOPBACK = (int)SocketOptionName.UseLoopback;
        public static object TCP_NODELAY = (int)SocketOptionName.NoDelay;

        public static object has_ipv6 = (int)1;

        #endregion

        #region Private implementation

        /// <summary>
        /// Return a standard socket exception (socket.error) whose message and error code come from a SocketException
        /// This will eventually be enhanced to generate the correct error type (error, herror, gaierror) based on the error code.
        /// </summary>
        private static Exception MakeException(Exception exception) {
            // !!! this shouldn't just blindly set the type to error (see summary)
            if (exception is SocketException) {
                SocketException se = (SocketException)exception;
                switch (se.SocketErrorCode) {
                    case SocketError.NotConnected:  // CPython times out when the socket isn't connected.
                    case SocketError.TimedOut:
                        return PythonExceptions.CreateThrowable(timeout, se.ErrorCode, se.Message);
                    default:
                        return PythonExceptions.CreateThrowable(error, se.ErrorCode, se.Message);
                }
            } else if (exception is ObjectDisposedException) {
                return PythonExceptions.CreateThrowable(error, (int)EBADF, "the socket is closed");
            } else if (exception is InvalidOperationException) {
                return MakeException(new SocketException((int)SocketError.InvalidArgument));
            } else {
                return exception;
            }
        }

        /// <summary>
        /// Convert an IPv6 address byte array to a string in standard colon-hex notation.
        /// The .NET IPAddress.ToString() method uses dotted-quad for the last 32 bits,
        /// which differs from the normal Python implementation (but is allowed by the IETF);
        /// this method returns the standard (no dotted-quad) colon-hex form.
        /// </summary>
        private static string IPv6BytesToColonHex(byte[] ipBytes) {
            Debug.Assert(ipBytes.Length == IPv6AddrBytes);

            const int bytesPerWord = 2; // in bytes
            const int bitsPerByte = 8;
            int[] words = new int[IPv6AddrBytes / bytesPerWord];

            // Convert to array of 16-bit words
            for (int i = 0; i < words.Length; i++) {
                for (int j = 0; j < bytesPerWord; j++) {
                    words[i] <<= bitsPerByte;
                    words[i] += ipBytes[i * bytesPerWord + j];
                }
            }

            // Find longest series of 0-valued words (to collapse to ::)
            int longestStart = 0;
            int longestLen = 0;

            for (int i = 0; i < words.Length; i++) {
                if (words[i] == 0) {
                    for (int j = i; j < words.Length; j++) {
                        if (words[j] != 0) {
                            i += longestLen;
                            break;
                        }
                        if (j - i + 1 > longestLen) {
                            longestStart = i;
                            longestLen = j - i + 1;
                        }
                    }
                }
            }

            // Build colon-hex string
            StringBuilder result = new StringBuilder(IPv6AddrBytes * 3);
            for (int i = 0; i < words.Length; i++) {
                if (i != 0) result.Append(':');
                if (longestLen > 0 && i == longestStart) {
                    if (longestStart == 0) result.Append(':');
                    if (longestStart + longestLen == words.Length) result.Append(':');
                    i += longestLen - 1;
                    continue;
                } else {
                    result.Append(words[i].ToString("x"));
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Handle conversion of "" to INADDR_ANY and "&lt;broadcast&gt;" to INADDR_BROADCAST.
        /// Otherwise returns host unchanged.
        /// </summary>
        private static string ConvertSpecialAddresses(string host) {
            switch (host) {
                case AnyAddrToken:
                    return IPAddress.Any.ToString();
                case BroadcastAddrToken:
                    return IPAddress.Broadcast.ToString();
                default:
                    return host;
            }
        }

        /// <summary>
        /// Return the IP address associated with host, with optional address family checking.
        /// host may be either a name or an IP address (in string form).
        /// 
        /// If family is non-null, a gaierror will be thrown if the host's address family is
        /// not the same as the specified family. gaierror is also raised if the hostname cannot be
        /// converted to an IP address (e.g. through a name lookup failure).
        /// </summary>
        private static IPAddress HostToAddress(string host, AddressFamily family) {
            return HostToAddresses(host, family)[0];
        }

        /// <summary>
        /// Return the IP address associated with host, with optional address family checking.
        /// host may be either a name or an IP address (in string form).
        /// 
        /// If family is non-null, a gaierror will be thrown if the host's address family is
        /// not the same as the specified family. gaierror is also raised if the hostname cannot be
        /// converted to an IP address (e.g. through a name lookup failure).
        /// </summary>
        private static IPAddress[] HostToAddresses(string host, AddressFamily family) {
            host = ConvertSpecialAddresses(host);
            try {
                IPAddress addr;

                bool numeric = true;
                int dotCount = 0;
                foreach (char c in host) {
                    if (!Char.IsNumber(c) && c != '.') {
                        numeric = false;
                    } else if (c == '.') {
                        dotCount++;
                    }
                }
                if (numeric) {
                    if (dotCount == 3 && IPAddress.TryParse(host, out addr)) {
                        if (family == AddressFamily.Unspecified || family == addr.AddressFamily) {
                            return new IPAddress[] { addr };
                        }
                    }
                    // Incorrect family will raise exception below
                } else {                    
                    IPHostEntry hostEntry = Dns.GetHostEntry(host);
                    List<IPAddress> addrs = new List<IPAddress>();
                    foreach (IPAddress ip in hostEntry.AddressList) {
                        if (family == AddressFamily.Unspecified || family == ip.AddressFamily) {
                            addrs.Add(ip);
                        }
                    }
                    if (addrs.Count > 0) return addrs.ToArray();
                }
                throw new SocketException((int)SocketError.HostNotFound);
            } catch (SocketException e) {
                throw PythonExceptions.CreateThrowable(gaierror, e.ErrorCode, "no addresses of the specified family associated with host");
            }
        }

        /// <summary>
        /// Return fqdn, but with its domain removed if it's on the same domain as the local machine.
        /// </summary>
        private static string RemoveLocalDomain(string fqdn) {
            char[] DNS_SEP = new char[] { '.' };
            string[] myName = GetFQDN().Split(DNS_SEP, 2);
            string[] otherName = fqdn.Split(DNS_SEP, 2);

            if (myName.Length < 2 || otherName.Length < 2) return fqdn;

            if (myName[1] == otherName[1]) {
                return otherName[0];
            } else {
                return fqdn;
            }
        }

        /// <summary>
        /// Convert a (host, port) tuple [IPv4] (host, port, flowinfo, scopeid) tuple [IPv6]
        /// to its corresponding IPEndPoint.
        /// 
        /// Throws gaierror if host is not a valid address.
        /// Throws ArgumentTypeException if any of the following are true:
        ///  - address does not have exactly two elements
        ///  - address[0] is not a string
        ///  - address[1] is not an int
        /// </summary>
        private static IPEndPoint TupleToEndPoint(PythonTuple address, AddressFamily family) {
            if (address.Count != 2 && address.Count != 4) {
                throw PythonOps.TypeError("address tuple must have exactly 2 (IPv4) or exactly 4 (IPv6) elements");
            }

            string host;
            try {
                host = Converter.ConvertToString(address[0]);
            } catch (ArgumentTypeException) {
                throw PythonOps.TypeError("host must be string");
            }

            int port;
            try {
                port = Converter.ConvertToInt32(address[1]);
            } catch (ArgumentTypeException) {
                throw PythonOps.TypeError("port must be integer");
            }

            IPAddress ip = HostToAddress(host, family);

            if (address.Count == 2) {
                return new IPEndPoint(ip, port);
            } else {
                long flowInfo;
                try {
                    flowInfo = Converter.ConvertToInt64(address[2]);
                } catch (ArgumentTypeException) {
                    throw PythonOps.TypeError("flowinfo must be integer");
                }
                // We don't actually do anything with flowinfo right now, but we validate it
                // in case we want to do something in the future.

                long scopeId;
                try {
                    scopeId = Converter.ConvertToInt64(address[3]);
                } catch (ArgumentTypeException) {
                    throw PythonOps.TypeError("scopeid must be integer");
                }

                IPEndPoint endPoint = new IPEndPoint(ip, port);
                endPoint.Address.ScopeId = scopeId;
                return endPoint;
            }
        }

        /// <summary>
        /// Convert an IPEndPoint to its corresponding (host, port) [IPv4] or (host, port, flowinfo, scopeid) [IPv6] tuple.
        /// Throws SocketException if the address family is other than IPv4 or IPv6.
        /// </summary>
        private static PythonTuple EndPointToTuple(IPEndPoint endPoint) {
            string ip = endPoint.Address.ToString();
            int port = endPoint.Port;
            switch (endPoint.Address.AddressFamily) {
                case AddressFamily.InterNetwork:
                    return PythonTuple.MakeTuple(ip, port);
                case AddressFamily.InterNetworkV6:
                    long flowInfo = 0; // RFC 3493 p. 7 
                    long scopeId = endPoint.Address.ScopeId;
                    return PythonTuple.MakeTuple(ip, port, flowInfo, scopeId);
                default:
                    throw new SocketException((int)SocketError.AddressFamilyNotSupported);
            }
        }

        class PythonUserSocketStream : Stream {
            private object _userSocket;
            private List<string> _data = new List<string>();
            private int _dataSize, _bufSize;
            private static FastDynamicSite<object, object, object> _sendAllSite = RuntimeHelpers.CreateSimpleCallSite<object, object, object>(DefaultContext.Default);
            private static FastDynamicSite<object, object, string> _recvSite = RuntimeHelpers.CreateSimpleCallSite<object, object, string>(DefaultContext.Default);

            public PythonUserSocketStream(object userSocket, int bufferSize) {
                _userSocket = userSocket;
                _bufSize = bufferSize;
            }

            public override bool CanRead {
                get { return true; }
            }

            public override bool CanSeek {
                get { return false; }
            }

            public override bool CanWrite {
                get { return true; }
            }

            public override void Flush() {
                if (_data.Count > 0) {
                    StringBuilder res = new StringBuilder();
                    foreach (string s in _data) {
                        res.Append(s);
                    }
                    _sendAllSite.Invoke(PythonOps.GetBoundAttr(DefaultContext.Default, _userSocket, SymbolTable.StringToId("sendall")), res.ToString());
                    _data.Clear();
                }
            }

            public override long Length {
                get { throw new NotImplementedException(); }
            }

            public override long Position {
                get {
                    throw new NotImplementedException();
                }
                set {
                    throw new NotImplementedException();
                }
            }

            public override int Read(byte[] buffer, int offset, int count) {
                int size = count;
                string data = _recvSite.Invoke(PythonOps.GetBoundAttr(DefaultContext.Default, _userSocket, SymbolTable.StringToId("recv")), count);

                return PythonAsciiEncoding.Instance.GetBytes(data, 0, data.Length, buffer, offset);
            }

            public override long Seek(long offset, SeekOrigin origin) {
                throw new NotImplementedException();
            }

            public override void SetLength(long value) {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count) {
                string strData = new string(PythonAsciiEncoding.Instance.GetChars(buffer, offset, count));
                _data.Add(strData);
                _dataSize += strData.Length;
                if (_dataSize > _bufSize) {
                    Flush();
                }                               
            }

            public object Socket {
                [PythonName("_sock")]
                get {
                    return _userSocket;
                }
            }
        }

        [PythonType("_fileobject")]
        public class FileObject : PythonFile {
            private const int DefaultBufferSize = 8192;
            public static object default_bufsize = DefaultBufferSize;
            public static object name = "<socket>";

            public FileObject(SocketObj socket)
                : this(socket, "rb", -1) {
            }

            public FileObject(SocketObj socket, string mode)
                : this(socket, mode, -1) {
            }

            public FileObject(SocketObj socket, string mode, int bufsize) {
                Initialize(new NetworkStream(socket.socket), System.Text.Encoding.Default, mode);
                socket.socket.SendBufferSize = socket.socket.ReceiveBufferSize = GetBufferSize(bufsize);
            }

            public FileObject(object socket)
                : this(socket, "rb", -1) {
            }

            public FileObject(object socket, string mode)
                : this(socket, mode, -1) {
            }

            public FileObject(object socket, string mode, int bufsize) {
                Initialize(new PythonUserSocketStream(socket, GetBufferSize(bufsize)), System.Text.Encoding.Default, mode);
            }

            private static int GetBufferSize(int size) {
                if (size == -1) return DefaultBufferSize;
                return size;
            }
        }
        #endregion

    }
}
#endif
