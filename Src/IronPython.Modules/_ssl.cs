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

#if !SILVERLIGHT // System.NET

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;


[assembly: PythonModule("_ssl", typeof(IronPython.Modules.PythonSsl))]
namespace IronPython.Modules {
    public static class PythonSsl {
        public const string __doc__ = "Implementation module for SSL socket operations.";

        [SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, PythonDictionary/*!*/ dict) {            
            var socket = context.GetBuiltinModule("socket");
            var socketError = PythonSocket.GetSocketError(context, socket.__dict__);
            
            context.EnsureModuleException("SSLError", socketError, dict, "SSLError", "ssl");

        }
        #region Stubs for RAND functions

        // The RAND_ functions are effectively no-ops, as the BCL draws on system sources
        // for cryptographically-strong randomness and doesn't need (or accept) user input

        public static void RAND_add(string buf, double entropy) {
        }

        public static void RAND_egd(string source) {
        }

        public static int RAND_status() {
            return 1; // always ready
        }

        #endregion

        public static PythonType SSLType = DynamicHelpers.GetPythonTypeFromType(typeof(PythonSocket.ssl));
        
        public static PythonSocket.ssl sslwrap(
            CodeContext context,
            PythonSocket.socket socket, 
            bool server_side, 
            [DefaultParameterValue(null)] string keyfile, 
            [DefaultParameterValue(null)] string certfile,
            [DefaultParameterValue(PythonSsl.CERT_NONE)]int certs_mode,
            [DefaultParameterValue(-1)]int protocol,
            [DefaultParameterValue(null)]string cacertsfile) {
            return new PythonSocket.ssl(
                context,
                socket,
                server_side,
                keyfile,
                certfile,
                certs_mode,
                protocol,
                cacertsfile
            );
        }

        internal static PythonType SSLError(CodeContext/*!*/ context) {
            return (PythonType)PythonContext.GetContext(context).GetModuleState("SSLError");
        }

        public static PythonDictionary _test_decode_cert(CodeContext context, string filename, [DefaultParameterValue(false)]bool complete) {
            var cert = ReadCertificate(context, filename);

            return CertificateToPython(context, cert, complete);
        }

        internal static PythonDictionary CertificateToPython(CodeContext context, X509Certificate cert, bool complete) {
            var dict = new CommonDictionaryStorage();

            dict.AddNoLock("notAfter", ToPythonDateFormat(cert.GetExpirationDateString()));
            dict.AddNoLock("subject", IssuerToPython(context, cert.Subject));
            if (complete) {
                dict.AddNoLock("notBefore", ToPythonDateFormat(cert.GetEffectiveDateString()));
                dict.AddNoLock("serialNumber", SerialNumberToPython(cert));
                dict.AddNoLock("version", cert.GetCertHashString());
                dict.AddNoLock("issuer", IssuerToPython(context, cert.Issuer));
            }

            return new PythonDictionary(dict);
        }

        private static string ToPythonDateFormat(string date) {
            return DateTime.Parse(date).ToUniversalTime().ToString("MMM d HH:mm:ss yyyy") + " GMT";
        }

        private static string SerialNumberToPython(X509Certificate cert) {
            var res = cert.GetSerialNumberString();
            for (int i = 0; i < res.Length; i++) {
                if (res[i] != '0') {
                    return res.Substring(i);
                }
            }
            return res;
        }

        private static PythonTuple IssuerToPython(CodeContext context, string issuer) {
            var split = issuer.Split(',');
            object[] res = new object[split.Length];
            for(int i = 0; i<split.Length; i++) {
                res[i] = PythonTuple.MakeTuple(
                    IssuerFieldToPython(context, split[i].Trim())
                );
                
            }
            return PythonTuple.MakeTuple(res);
        }

        private static PythonTuple IssuerFieldToPython(CodeContext context, string p) {
            if (String.Compare(p, 0, "CN=", 0, 3) == 0) {
                return PythonTuple.MakeTuple("commonName", p.Substring(3));
            } else if (String.Compare(p, 0, "OU=", 0, 3) == 0) {
                return PythonTuple.MakeTuple("organizationalUnitName", p.Substring(3));
            } else if (String.Compare(p, 0, "O=", 0, 2) == 0) {
                return PythonTuple.MakeTuple("organizationName", p.Substring(2));
            } else if (String.Compare(p, 0, "L=", 0, 2) == 0) {
                return PythonTuple.MakeTuple("localityName", p.Substring(2));
            } else if (String.Compare(p, 0, "S=", 0, 2) == 0) {
                return PythonTuple.MakeTuple("stateOrProvinceName", p.Substring(2));
            } else if (String.Compare(p, 0, "C=", 0, 2) == 0) {
                return PythonTuple.MakeTuple("countryName", p.Substring(2));
            } else if (String.Compare(p, 0, "E=", 0, 2) == 0) {
                return PythonTuple.MakeTuple("email", p.Substring(2));
            }

            throw PythonExceptions.CreateThrowable(SSLError(context), "Unknown field: ", p);
        }


        internal static X509Certificate2 ReadCertificate(CodeContext context, string filename) {

            string[] lines;
            try {
                lines = File.ReadAllLines(filename);
            } catch (IOException) {
                throw PythonExceptions.CreateThrowable(SSLError(context), "Can't open file ", filename);
            }

            X509Certificate2 cert = null;
            RSACryptoServiceProvider key = null;
            try {
                for (int i = 0; i < lines.Length; i++) {
                    if (lines[i] == "-----BEGIN CERTIFICATE-----") {
                        var certStr = ReadToEnd(lines, ref i, "-----END CERTIFICATE-----");

                        try {
                            cert = new X509Certificate2(Convert.FromBase64String(certStr.ToString()));
                        } catch (Exception e) {
                            throw ErrorDecoding(context, filename, e);
                        }
                    } else if (lines[i] == "-----BEGIN RSA PRIVATE KEY-----") {
                        var keyStr = ReadToEnd(lines, ref i, "-----END RSA PRIVATE KEY-----");

                        try {
                            var keyBytes = Convert.FromBase64String(keyStr.ToString());

                            key = ParsePkcs1DerEncodedPrivateKey(context, filename, keyBytes);
                        } catch (Exception e) {
                            throw ErrorDecoding(context, filename, e);
                        }
                    }
                }
            } catch (InvalidOperationException e) {
                throw ErrorDecoding(context, filename, e.Message);
            }

            if (cert != null) {
                if (key != null) {
                    try {
                        cert.PrivateKey = key;
                    } catch(CryptographicException e) {
                        throw ErrorDecoding(context, filename, "cert and private key are incompatible", e);
                    }
                }
                return cert;
            }
            throw ErrorDecoding(context, filename, "certificate not found");
        }

        #region Private Key Parsing

        const int ClassOffset = 6;
        const int ClassMask = 0xc0;
        const int ClassUniversal = 0x00 << ClassOffset;
        const int ClassApplication = 0x01 << ClassOffset;
        const int ClassContextSpecific = 0x02 << ClassOffset;
        const int ClassPrivate = 0x03 << ClassOffset;

        const int NumberMask = 0x1f;

        const int UnivesalSequence = 0x10;
        const int UniversalInteger = 0x02;
        const int UniversalOctetString = 0x04;

        private static RSACryptoServiceProvider ParsePkcs1DerEncodedPrivateKey(CodeContext context, string filename, byte[] x) {
            // http://tools.ietf.org/html/rfc3447#appendix-A.1.2
            // RSAPrivateKey ::= SEQUENCE {
            //   version           Version,
            //   modulus           INTEGER,  -- n
            //   publicExponent    INTEGER,  -- e
            //   privateExponent   INTEGER,  -- d
            //   prime1            INTEGER,  -- p
            //   prime2            INTEGER,  -- q
            //   exponent1         INTEGER,  -- d mod (p-1)
            //   exponent2         INTEGER,  -- d mod (q-1)
            //   coefficient       INTEGER,  -- (inverse of q) mod p
            //   otherPrimeInfos   OtherPrimeInfos OPTIONAL
            // }

            // read header for sequence
            if ((x[0] & ClassMask) != ClassUniversal) {
                throw ErrorDecoding(context, filename, "failed to find universal class");
            } else if ((x[0] & NumberMask) != UnivesalSequence) {
                throw ErrorDecoding(context, filename, "failed to read sequence header");
            }

            // read length of sequence
            int offset = 1;
            int len = ReadLength(x, ref offset);

            // read version
            int version = ReadUnivesalInt(x, ref offset);
            if (version != 0) {
                // unsupported version
                throw new InvalidOperationException(String.Format("bad vesion: {0}", version));
            }

            // read in parameters and initialize provider
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            RSAParameters parameters = new RSAParameters();

            parameters.Modulus = ReadUnivesalIntAsBytes(x, ref offset);
            parameters.Exponent = ReadUnivesalIntAsBytes(x, ref offset);
            parameters.D = ReadUnivesalIntAsBytes(x, ref offset);
            parameters.P = ReadUnivesalIntAsBytes(x, ref offset);
            parameters.Q = ReadUnivesalIntAsBytes(x, ref offset);
            parameters.DP = ReadUnivesalIntAsBytes(x, ref offset);
            parameters.DQ = ReadUnivesalIntAsBytes(x, ref offset);
            parameters.InverseQ = ReadUnivesalIntAsBytes(x, ref offset);
            
            provider.ImportParameters(parameters);            
            return provider;
        }

        private static byte[] ReadUnivesalIntAsBytes(byte[] x, ref int offset) {
            ReadIntType(x, ref offset);

            int bytes = ReadLength(x, ref offset);

            // we need to remove any leading 0 bytes which aren't part of a number.  Including
            // them causes our parsing to differ from certification parsing.
            while (x[offset] == 0) {
                bytes--;
                offset++;
            }

            byte[] res = new byte[bytes];
            for (int i = 0; i < res.Length; i++) {
                res[i] = x[offset++];
            }
            
            return res;

        }

        private static void ReadIntType(byte[] x, ref int offset) {
            int versionType = x[offset++];
            if (versionType != UniversalInteger) {
                throw new InvalidOperationException(String.Format("expected version, fonud {0}", versionType));
            }            
        }
        private static int ReadUnivesalInt(byte[] x, ref int offset) {
            ReadIntType(x, ref offset);

            return ReadInt(x, ref offset);
        }

        private static int ReadLength(byte[] x, ref int offset) {
            int bytes = x[offset++];
            if ((bytes & 0x80) == 0) {
                return bytes;
            }

            return ReadInt(x, ref offset, bytes & ~0x80);

        }

        private static int ReadInt(byte[] x, ref int offset, int bytes) {
            if (bytes + offset > x.Length) {
                throw new InvalidOperationException();
            }

            int res = 0;
            for (int i = 0; i < bytes; i++) {
                res = res << 8 | x[offset++];
            }
            return res;
        }
        /// <summary>
        /// BER encoding of an integer value is the number of bytes
        /// required to represent the integer followed by the bytes
        /// </summary>
        private static int ReadInt(byte[] x, ref int offset) {            
            int bytes = x[offset++];
            
            return ReadInt(x, ref offset, bytes);
        }

        private static string ReadToEnd(string[] lines, ref int start, string end) {
            StringBuilder key = new StringBuilder();
            for (start++; start < lines.Length; start++) {
                if (lines[start] == end) {                    
                    return key.ToString();
                }
                key.Append(lines[start]);
            }
            return null;
        }

        #endregion

        private static Exception ErrorDecoding(CodeContext context, params object[] args) {
            return PythonExceptions.CreateThrowable(SSLError(context), ArrayUtils.Insert("Error decoding PEM-encoded file ", args));
        }

        public const int CERT_NONE = 0;
        public const int CERT_OPTIONAL = 1;
        public const int CERT_REQUIRED = 2;

        public const int PROTOCOL_SSLv2 = 0;
        public const int PROTOCOL_SSLv3 = 1;
        public const int PROTOCOL_SSLv23 = 2;
        public const int PROTOCOL_TLSv1 = 3;

        #region Exported constants

        public const int SSL_ERROR_SSL = 1;
        public const int SSL_ERROR_WANT_READ = 2;
        public const int SSL_ERROR_WANT_WRITE = 3;
        public const int SSL_ERROR_WANT_X509_LOOKUP = 4;
        public const int SSL_ERROR_SYSCALL = 5;
        public const int SSL_ERROR_ZERO_RETURN = 6;
        public const int SSL_ERROR_WANT_CONNECT = 7;
        public const int SSL_ERROR_EOF = 8;
        public const int SSL_ERROR_INVALID_ERROR_CODE = 9;

        #endregion
    }
}
#endif
