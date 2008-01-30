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

using System;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime;

[assembly: PythonModule("errno", typeof(IronPython.Modules.PythonErrorNumber))]
namespace IronPython.Modules {
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

        public static int E2BIG = 7;
        public static int EACCES = 13;
        public static int EADDRINUSE = 10048;
        public static int EADDRNOTAVAIL = 10049;
        public static int EAFNOSUPPORT = 10047;
        public static int EAGAIN = 11;
        public static int EALREADY = 10037;
        public static int EBADF = 9;
        public static int EBUSY = 16;
        public static int ECHILD = 10;
        public static int ECONNABORTED = 10053;
        public static int ECONNREFUSED = 10061;
        public static int ECONNRESET = 10054;
        public static int EDEADLK = 36;
        public static int EDEADLOCK = 36;
        public static int EDESTADDRREQ = 10039;
        public static int EDOM = 33;
        public static int EDQUOT = 10069;
        public static int EEXIST = 17;
        public static int EFAULT = 14;
        public static int EFBIG = 27;
        public static int EHOSTDOWN = 10064;
        public static int EHOSTUNREACH = 10065;
        public static int EILSEQ = 42;
        public static int EINPROGRESS = 10036;
        public static int EINTR = 4;
        public static int EINVAL = 22;
        public static int EIO = 5;
        public static int EISCONN = 10056;
        public static int EISDIR = 21;
        public static int ELOOP = 10062;
        public static int EMFILE = 24;
        public static int EMLINK = 31;
        public static int EMSGSIZE = 10040;
        public static int ENAMETOOLONG = 38;
        public static int ENETDOWN = 10050;
        public static int ENETRESET = 10052;
        public static int ENETUNREACH = 10051;
        public static int ENFILE = 23;
        public static int ENOBUFS = 10055;
        public static int ENODEV = 19;
        public static int ENOENT = 2;
        public static int ENOEXEC = 8;
        public static int ENOLCK = 39;
        public static int ENOMEM = 12;
        public static int ENOPROTOOPT = 10042;
        public static int ENOSPC = 28;
        public static int ENOSYS = 40;
        public static int ENOTCONN = 10057;
        public static int ENOTDIR = 20;
        public static int ENOTEMPTY = 41;
        public static int ENOTSOCK = 10038;
        public static int ENOTTY = 25;
        public static int ENXIO = 6;
        public static int EOPNOTSUPP = 10045;
        public static int EPERM = 1;
        public static int EPFNOSUPPORT = 10046;
        public static int EPIPE = 32;
        public static int EPROTONOSUPPORT = 10043;
        public static int EPROTOTYPE = 10041;
        public static int ERANGE = 34;
        public static int EREMOTE = 10071;
        public static int EROFS = 30;
        public static int ESHUTDOWN = 10058;
        public static int ESOCKTNOSUPPORT = 10044;
        public static int ESPIPE = 29;
        public static int ESRCH = 3;
        public static int ESTALE = 10070;
        public static int ETIMEDOUT = 10060;
        public static int ETOOMANYREFS = 10059;
        public static int EUSERS = 10068;
        public static int EWOULDBLOCK = 10035;
        public static int EXDEV = 18;
        public static int WSABASEERR = 10000;
        public static int WSAEACCES = 10013;
        public static int WSAEADDRINUSE = 10048;
        public static int WSAEADDRNOTAVAIL = 10049;
        public static int WSAEAFNOSUPPORT = 10047;
        public static int WSAEALREADY = 10037;
        public static int WSAEBADF = 10009;
        public static int WSAECONNABORTED = 10053;
        public static int WSAECONNREFUSED = 10061;
        public static int WSAECONNRESET = 10054;
        public static int WSAEDESTADDRREQ = 10039;
        public static int WSAEDISCON = 10101;
        public static int WSAEDQUOT = 10069;
        public static int WSAEFAULT = 10014;
        public static int WSAEHOSTDOWN = 10064;
        public static int WSAEHOSTUNREACH = 10065;
        public static int WSAEINPROGRESS = 10036;
        public static int WSAEINTR = 10004;
        public static int WSAEINVAL = 10022;
        public static int WSAEISCONN = 10056;
        public static int WSAELOOP = 10062;
        public static int WSAEMFILE = 10024;
        public static int WSAEMSGSIZE = 10040;
        public static int WSAENAMETOOLONG = 10063;
        public static int WSAENETDOWN = 10050;
        public static int WSAENETRESET = 10052;
        public static int WSAENETUNREACH = 10051;
        public static int WSAENOBUFS = 10055;
        public static int WSAENOPROTOOPT = 10042;
        public static int WSAENOTCONN = 10057;
        public static int WSAENOTEMPTY = 10066;
        public static int WSAENOTSOCK = 10038;
        public static int WSAEOPNOTSUPP = 10045;
        public static int WSAEPFNOSUPPORT = 10046;
        public static int WSAEPROCLIM = 10067;
        public static int WSAEPROTONOSUPPORT = 10043;
        public static int WSAEPROTOTYPE = 10041;
        public static int WSAEREMOTE = 10071;
        public static int WSAESHUTDOWN = 10058;
        public static int WSAESOCKTNOSUPPORT = 10044;
        public static int WSAESTALE = 10070;
        public static int WSAETIMEDOUT = 10060;
        public static int WSAETOOMANYREFS = 10059;
        public static int WSAEUSERS = 10068;
        public static int WSAEWOULDBLOCK = 10035;
        public static int WSANOTINITIALISED = 10093;
        public static int WSASYSNOTREADY = 10091;
        public static int WSAVERNOTSUPPORTED = 10092;

    }
}
