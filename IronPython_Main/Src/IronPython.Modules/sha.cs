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
using Microsoft.Scripting.Runtime;

//!!! This is pretty inefficient. We should probably use hasher.TransformBlock instead of
//!!! hanging onto all of the bytes.
//!!! Also, we could probably make a generic version of this that could then be specialized
//!!! for both md5 and sha.

#if !SILVERLIGHT    // System.Cryptography.SHA1CryptoServiceProvider
[assembly: PythonModule("_sha", typeof(IronPython.Modules.PythonSha))]
namespace IronPython.Modules {
    [Documentation("SHA1 hash algorithm")]
    public static class PythonSha {
        private static readonly SHA1CryptoServiceProvider hasher = new SHA1CryptoServiceProvider();        
        private static readonly int digestSize = hasher.HashSize / 8;
        private const int blockSize = 1;

        public static int digest_size {
            [Documentation("Size of the resulting digest in bytes (constant)")]
            get { return digestSize; }
        }

        public static int blocksize {
            [Documentation("Block size")]
            get { return blockSize; }
        }

        [Documentation("new([data]) -> object (object used to calculate hash)")]
        public static sha @new(object data) {
            return new sha(data);
        }

        [Documentation("new([data]) -> object (object used to calculate hash)")]
        public static sha @new() {
            return new sha();
        }

        [Documentation("new([data]) -> object (object used to calculate hash)")]
        public class sha : ICloneable {
            byte[] _bytes;
            byte[] _hash;

            public sha() : this(new byte[0]) { }

            public sha(object initialData) {
                _bytes = new byte[0];
                update(initialData);
            }

            private sha(byte[] initialBytes) {
                _bytes = new byte[0];
                update(initialBytes);
            }

            [Documentation("update(string) -> None (update digest with string data)")]
            public void update(object newData) {
                update(StringOps.ToByteArray(Converter.ConvertToString(newData)));
            }

            private void update(byte[] newBytes) {
                byte[] updatedBytes = new byte[_bytes.Length + newBytes.Length];
                Array.Copy(_bytes, updatedBytes, _bytes.Length);
                Array.Copy(newBytes, 0, updatedBytes, _bytes.Length, newBytes.Length);
                _bytes = updatedBytes;
                _hash = hasher.ComputeHash(_bytes);
            }

            [Documentation("digest() -> int (current digest value)")]
            public string digest() {
                return StringOps.FromByteArray(_hash);
            }

            [Documentation("hexdigest() -> string (current digest as hex digits)")]
            public string hexdigest() {
                StringBuilder result = new StringBuilder(2 * _hash.Length);
                for (int i = 0; i < _hash.Length; i++) {
                    result.Append(_hash[i].ToString("x2"));
                }
                return result.ToString();
            }

            [Documentation("copy() -> object (copy of this object)")]
            public sha copy() {
                return new sha(_bytes);
            }

            object ICloneable.Clone() {
                return copy();
            }
        }
    }
}
#endif
