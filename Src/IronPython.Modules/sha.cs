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

#if !SILVERLIGHT    // System.Cryptography.SHA1CryptoServiceProvider
[assembly: PythonModule("sha", typeof(IronPython.Modules.PythonSha))]
namespace IronPython.Modules {
    [Documentation("SHA1 hash algorithm")]
    [PythonType("sha")]
    public static class PythonSha {
        private static readonly SHA1CryptoServiceProvider hasher = new SHA1CryptoServiceProvider();
        private static readonly int digestSize = hasher.HashSize / 8;
        private const int blockSize = 1;

        public static int DigestSize {
            [Documentation("Size of the resulting digest in bytes (constant)")]
            [PythonName("digest_size")]
            get { return digestSize; }
        }

        public static int BlockSize {
            [Documentation("Block size")]
            [PythonName("blocksize")]
            get { return blockSize; }
        }

        [Documentation("new([data]) -> object (object used to calculate hash)")]
        [PythonName("new")]
        public static ShaObject Make(object data) {
            return new ShaObject(data);
        }

        [Documentation("new([data]) -> object (object used to calculate hash)")]
        [PythonName("new")]
        public static ShaObject Make() {
            return new ShaObject();
        }

        [Documentation("new([data]) -> object (object used to calculate hash)")]
        [PythonType("sha")]
        public class ShaObject : ICloneable {
            byte[] bytes;
            byte[] hash;

            public ShaObject() : this(new byte[0]) { }

            public ShaObject(object initialData) {
                bytes = new byte[0];
                Update(initialData);
            }

            private ShaObject(byte[] initialBytes) {
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

            [Documentation("copy() -> object (copy of this object)")]
            [PythonName("copy")]
            public object Clone() {
                return new ShaObject(bytes);
            }

        }
    }
}
#endif
