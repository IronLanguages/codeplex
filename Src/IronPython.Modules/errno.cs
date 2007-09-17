/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime;

[assembly: PythonModule("errno", typeof(IronPython.Modules.PythonErrorNumber))]
namespace IronPython.Modules {
    [PythonType("errno")]
    public static class PythonErrorNumber {
        static PythonErrorNumber() {
            errorcode = new PythonDictionary();

            errorcode["E2BIG"] = E2BIG;
            errorcode["EACCES"] = EACCES;
            errorcode["EADDRINUSE"] = EADDRINUSE;
            errorcode["EADDRNOTAVAIL"] = EADDRNOTAVAIL;
            errorcode["EAFNOSUPPORT"] = EAFNOSUPPORT;
            errorcode["EAGAIN"] = EAGAIN;
            errorcode["EALREADY"] = EALREADY;
            errorcode["EBADF"] = EBADF;
            errorcode["EBUSY"] = EBUSY;
            errorcode["ECHILD"] = ECHILD;
            errorcode["ECONNABORTED"] = ECONNABORTED;
            errorcode["ECONNREFUSED"] = ECONNREFUSED;
            errorcode["ECONNRESET"] = ECONNRESET;
            errorcode["EDEADLK"] = EDEADLK;
            errorcode["EDEADLOCK"] = EDEADLOCK;
            errorcode["EDESTADDRREQ"] = EDESTADDRREQ;
            errorcode["EDOM"] = EDOM;
            errorcode["EDQUOT"] = EDQUOT;
            errorcode["EEXIST"] = EEXIST;
            errorcode["EFAULT"] = EFAULT;
            errorcode["EFBIG"] = EFBIG;
            errorcode["EHOSTDOWN"] = EHOSTDOWN;
            errorcode["EHOSTUNREACH"] = EHOSTUNREACH;
            errorcode["EILSEQ"] = EILSEQ;
            errorcode["EINPROGRESS"] = EINPROGRESS;
            errorcode["EINTR"] = EINTR;
            errorcode["EINVAL"] = EINVAL;
            errorcode["EIO"] = EIO;
            errorcode["EISCONN"] = EISCONN;
            errorcode["EISDIR"] = EISDIR;
            errorcode["ELOOP"] = ELOOP;
            errorcode["EMFILE"] = EMFILE;
            errorcode["EMLINK"] = EMLINK;
            errorcode["EMSGSIZE"] = EMSGSIZE;
            errorcode["ENAMETOOLONG"] = ENAMETOOLONG;
            errorcode["ENETDOWN"] = ENETDOWN;
            errorcode["ENETRESET"] = ENETRESET;
            errorcode["ENETUNREACH"] = ENETUNREACH;
            errorcode["ENFILE"] = ENFILE;
            errorcode["ENOBUFS"] = ENOBUFS;
            errorcode["ENODEV"] = ENODEV;
            errorcode["ENOENT"] = ENOENT;
            errorcode["ENOEXEC"] = ENOEXEC;
            errorcode["ENOLCK"] = ENOLCK;
            errorcode["ENOMEM"] = ENOMEM;
            errorcode["ENOPROTOOPT"] = ENOPROTOOPT;
            errorcode["ENOSPC"] = ENOSPC;
            errorcode["ENOSYS"] = ENOSYS;
            errorcode["ENOTCONN"] = ENOTCONN;
            errorcode["ENOTDIR"] = ENOTDIR;
            errorcode["ENOTEMPTY"] = ENOTEMPTY;
            errorcode["ENOTSOCK"] = ENOTSOCK;
            errorcode["ENOTTY"] = ENOTTY;
            errorcode["ENXIO"] = ENXIO;
            errorcode["EOPNOTSUPP"] = EOPNOTSUPP;
            errorcode["EPERM"] = EPERM;
            errorcode["EPFNOSUPPORT"] = EPFNOSUPPORT;
            errorcode["EPIPE"] = EPIPE;
            errorcode["EPROTONOSUPPORT"] = EPROTONOSUPPORT;
            errorcode["EPROTOTYPE"] = EPROTOTYPE;
            errorcode["ERANGE"] = ERANGE;
            errorcode["EREMOTE"] = EREMOTE;
            errorcode["EROFS"] = EROFS;
            errorcode["ESHUTDOWN"] = ESHUTDOWN;
            errorcode["ESOCKTNOSUPPORT"] = ESOCKTNOSUPPORT;
            errorcode["ESPIPE"] = ESPIPE;
            errorcode["ESRCH"] = ESRCH;
            errorcode["ESTALE"] = ESTALE;
            errorcode["ETIMEDOUT"] = ETIMEDOUT;
            errorcode["ETOOMANYREFS"] = ETOOMANYREFS;
            errorcode["EUSERS"] = EUSERS;
            errorcode["EWOULDBLOCK"] = EWOULDBLOCK;
            errorcode["EXDEV"] = EXDEV;
            errorcode["WSABASEERR"] = WSABASEERR;
            errorcode["WSAEACCES"] = WSAEACCES;
            errorcode["WSAEADDRINUSE"] = WSAEADDRINUSE;
            errorcode["WSAEADDRNOTAVAIL"] = WSAEADDRNOTAVAIL;
            errorcode["WSAEAFNOSUPPORT"] = WSAEAFNOSUPPORT;
            errorcode["WSAEALREADY"] = WSAEALREADY;
            errorcode["WSAEBADF"] = WSAEBADF;
            errorcode["WSAECONNABORTED"] = WSAECONNABORTED;
            errorcode["WSAECONNREFUSED"] = WSAECONNREFUSED;
            errorcode["WSAECONNRESET"] = WSAECONNRESET;
            errorcode["WSAEDESTADDRREQ"] = WSAEDESTADDRREQ;
            errorcode["WSAEDISCON"] = WSAEDISCON;
            errorcode["WSAEDQUOT"] = WSAEDQUOT;
            errorcode["WSAEFAULT"] = WSAEFAULT;
            errorcode["WSAEHOSTDOWN"] = WSAEHOSTDOWN;
            errorcode["WSAEHOSTUNREACH"] = WSAEHOSTUNREACH;
            errorcode["WSAEINPROGRESS"] = WSAEINPROGRESS;
            errorcode["WSAEINTR"] = WSAEINTR;
            errorcode["WSAEINVAL"] = WSAEINVAL;
            errorcode["WSAEISCONN"] = WSAEISCONN;
            errorcode["WSAELOOP"] = WSAELOOP;
            errorcode["WSAEMFILE"] = WSAEMFILE;
            errorcode["WSAEMSGSIZE"] = WSAEMSGSIZE;
            errorcode["WSAENAMETOOLONG"] = WSAENAMETOOLONG;
            errorcode["WSAENETDOWN"] = WSAENETDOWN;
            errorcode["WSAENETRESET"] = WSAENETRESET;
            errorcode["WSAENETUNREACH"] = WSAENETUNREACH;
            errorcode["WSAENOBUFS"] = WSAENOBUFS;
            errorcode["WSAENOPROTOOPT"] = WSAENOPROTOOPT;
            errorcode["WSAENOTCONN"] = WSAENOTCONN;
            errorcode["WSAENOTEMPTY"] = WSAENOTEMPTY;
            errorcode["WSAENOTSOCK"] = WSAENOTSOCK;
            errorcode["WSAEOPNOTSUPP"] = WSAEOPNOTSUPP;
            errorcode["WSAEPFNOSUPPORT"] = WSAEPFNOSUPPORT;
            errorcode["WSAEPROCLIM"] = WSAEPROCLIM;
            errorcode["WSAEPROTONOSUPPORT"] = WSAEPROTONOSUPPORT;
            errorcode["WSAEPROTOTYPE"] = WSAEPROTOTYPE;
            errorcode["WSAEREMOTE"] = WSAEREMOTE;
            errorcode["WSAESHUTDOWN"] = WSAESHUTDOWN;
            errorcode["WSAESOCKTNOSUPPORT"] = WSAESOCKTNOSUPPORT;
            errorcode["WSAESTALE"] = WSAESTALE;
            errorcode["WSAETIMEDOUT"] = WSAETIMEDOUT;
            errorcode["WSAETOOMANYREFS"] = WSAETOOMANYREFS;
            errorcode["WSAEUSERS"] = WSAEUSERS;
            errorcode["WSAEWOULDBLOCK"] = WSAEWOULDBLOCK;
            errorcode["WSANOTINITIALISED"] = WSANOTINITIALISED;
            errorcode["WSASYSNOTREADY"] = WSASYSNOTREADY;
            errorcode["WSAVERNOTSUPPORTED"] = WSAVERNOTSUPPORTED;
        }

        public static PythonDictionary errorcode;

        public static object E2BIG = 7;
        public static object EACCES = 13;
        public static object EADDRINUSE = 10048;
        public static object EADDRNOTAVAIL = 10049;
        public static object EAFNOSUPPORT = 10047;
        public static object EAGAIN = 11;
        public static object EALREADY = 10037;
        public static object EBADF = 9;
        public static object EBUSY = 16;
        public static object ECHILD = 10;
        public static object ECONNABORTED = 10053;
        public static object ECONNREFUSED = 10061;
        public static object ECONNRESET = 10054;
        public static object EDEADLK = 36;
        public static object EDEADLOCK = 36;
        public static object EDESTADDRREQ = 10039;
        public static object EDOM = 33;
        public static object EDQUOT = 10069;
        public static object EEXIST = 17;
        public static object EFAULT = 14;
        public static object EFBIG = 27;
        public static object EHOSTDOWN = 10064;
        public static object EHOSTUNREACH = 10065;
        public static object EILSEQ = 42;
        public static object EINPROGRESS = 10036;
        public static object EINTR = 4;
        public static object EINVAL = 22;
        public static object EIO = 5;
        public static object EISCONN = 10056;
        public static object EISDIR = 21;
        public static object ELOOP = 10062;
        public static object EMFILE = 24;
        public static object EMLINK = 31;
        public static object EMSGSIZE = 10040;
        public static object ENAMETOOLONG = 38;
        public static object ENETDOWN = 10050;
        public static object ENETRESET = 10052;
        public static object ENETUNREACH = 10051;
        public static object ENFILE = 23;
        public static object ENOBUFS = 10055;
        public static object ENODEV = 19;
        public static object ENOENT = 2;
        public static object ENOEXEC = 8;
        public static object ENOLCK = 39;
        public static object ENOMEM = 12;
        public static object ENOPROTOOPT = 10042;
        public static object ENOSPC = 28;
        public static object ENOSYS = 40;
        public static object ENOTCONN = 10057;
        public static object ENOTDIR = 20;
        public static object ENOTEMPTY = 41;
        public static object ENOTSOCK = 10038;
        public static object ENOTTY = 25;
        public static object ENXIO = 6;
        public static object EOPNOTSUPP = 10045;
        public static object EPERM = 1;
        public static object EPFNOSUPPORT = 10046;
        public static object EPIPE = 32;
        public static object EPROTONOSUPPORT = 10043;
        public static object EPROTOTYPE = 10041;
        public static object ERANGE = 34;
        public static object EREMOTE = 10071;
        public static object EROFS = 30;
        public static object ESHUTDOWN = 10058;
        public static object ESOCKTNOSUPPORT = 10044;
        public static object ESPIPE = 29;
        public static object ESRCH = 3;
        public static object ESTALE = 10070;
        public static object ETIMEDOUT = 10060;
        public static object ETOOMANYREFS = 10059;
        public static object EUSERS = 10068;
        public static object EWOULDBLOCK = 10035;
        public static object EXDEV = 18;
        public static object WSABASEERR = 10000;
        public static object WSAEACCES = 10013;
        public static object WSAEADDRINUSE = 10048;
        public static object WSAEADDRNOTAVAIL = 10049;
        public static object WSAEAFNOSUPPORT = 10047;
        public static object WSAEALREADY = 10037;
        public static object WSAEBADF = 10009;
        public static object WSAECONNABORTED = 10053;
        public static object WSAECONNREFUSED = 10061;
        public static object WSAECONNRESET = 10054;
        public static object WSAEDESTADDRREQ = 10039;
        public static object WSAEDISCON = 10101;
        public static object WSAEDQUOT = 10069;
        public static object WSAEFAULT = 10014;
        public static object WSAEHOSTDOWN = 10064;
        public static object WSAEHOSTUNREACH = 10065;
        public static object WSAEINPROGRESS = 10036;
        public static object WSAEINTR = 10004;
        public static object WSAEINVAL = 10022;
        public static object WSAEISCONN = 10056;
        public static object WSAELOOP = 10062;
        public static object WSAEMFILE = 10024;
        public static object WSAEMSGSIZE = 10040;
        public static object WSAENAMETOOLONG = 10063;
        public static object WSAENETDOWN = 10050;
        public static object WSAENETRESET = 10052;
        public static object WSAENETUNREACH = 10051;
        public static object WSAENOBUFS = 10055;
        public static object WSAENOPROTOOPT = 10042;
        public static object WSAENOTCONN = 10057;
        public static object WSAENOTEMPTY = 10066;
        public static object WSAENOTSOCK = 10038;
        public static object WSAEOPNOTSUPP = 10045;
        public static object WSAEPFNOSUPPORT = 10046;
        public static object WSAEPROCLIM = 10067;
        public static object WSAEPROTONOSUPPORT = 10043;
        public static object WSAEPROTOTYPE = 10041;
        public static object WSAEREMOTE = 10071;
        public static object WSAESHUTDOWN = 10058;
        public static object WSAESOCKTNOSUPPORT = 10044;
        public static object WSAESTALE = 10070;
        public static object WSAETIMEDOUT = 10060;
        public static object WSAETOOMANYREFS = 10059;
        public static object WSAEUSERS = 10068;
        public static object WSAEWOULDBLOCK = 10035;
        public static object WSANOTINITIALISED = 10093;
        public static object WSASYSNOTREADY = 10091;
        public static object WSAVERNOTSUPPORTED = 10092;

    }
}
