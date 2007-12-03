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
using System.Security.Cryptography;
using System.Text;

using Microsoft.Scripting;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

//!!! This is pretty inefficient. We should probably use hasher.TransformBlock instead of
//!!! hanging onto all of the bytes.
//!!! Also, we could probably make a generic version of this that could then be specialized
//!!! for both md5 and sha.

#if !SILVERLIGHT // MD5

[assembly: PythonModule("md5", typeof(IronPython.Modules.PythonMD5))]
namespace IronPython.Modules {
    [Documentation("MD5 hash algorithm")]
    [PythonType("md5")]
    public static class PythonMD5 {
        private static readonly MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider();
        private static readonly int digestSize = hasher.HashSize / 8;

        public static int DigestSize {
            [Documentation("Size of the resulting digest in bytes (constant)")]
            [PythonName("digest_size")]
            get { return digestSize; }
        }

        [Documentation("new([data]) -> object (new md5 object)")]
        [PythonName("new")]
        public static MD5Object Make(object data) {
            return new MD5Object(data);
        }

        [Documentation("new([data]) -> object (new md5 object)")]
        [PythonName("new")]
        public static MD5Object Make() {
            return new MD5Object();
        }

        [Documentation("new([data]) -> object (object used to calculate MD5 hash)")]
        [PythonType("md5")]
        public class MD5Object : ICloneable {
            byte[] bytes;
            byte[] hash;

            public MD5Object() : this(new byte[0]) { }

            public MD5Object(object initialData) {
                bytes = new byte[0];
                Update(initialData);
            }

            private MD5Object(byte[] initialBytes) {
                bytes = new byte[0];
                Update(initialBytes);
            }

            [Documentation("update(string) -> None (update digest with string data)")]
            [PythonName("update")]
            public void Update(object newData) {
                Update(StringOps.ToByteArray(Converter.ConvertToString(newData)));
            }

            private void Update(byte[] newBytes) {
                byte[] updatedBytes = new byte[bytes.Length + newBytes.Length];
                Array.Copy(bytes, updatedBytes, bytes.Length);
                Array.Copy(newBytes, 0, updatedBytes, bytes.Length, newBytes.Length);
                bytes = updatedBytes;
                hash = hasher.ComputeHash(bytes);
            }

            [Documentation("digest() -> int (current digest value)")]
            [PythonName("digest")]
            public string Digest() {
                return StringOps.FromByteArray(hash);
            }

            [Documentation("hexdigest() -> string (current digest as hex digits)")]
            [PythonName("hexdigest")]
            public string HexDigest() {
                StringBuilder result = new StringBuilder(2 * hash.Length);
                for (int i = 0; i < hash.Length; i++) {
                    result.Append(hash[i].ToString("x2"));
                }
                return result.ToString();
            }

            [Documentation("copy() -> object (copy of this md5 object)")]
            [PythonName("copy")]
            public object Clone() {
                return new MD5Object(bytes);
            }

        }
    }
}

#endif