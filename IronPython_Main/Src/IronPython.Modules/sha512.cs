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
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

//!!! This is pretty inefficient. We should probably use hasher.TransformBlock instead of
//!!! hanging onto all of the bytes.
//!!! Also, we could probably make a generic version of this that could then be specialized
//!!! for both md5 and sha.

#if !SILVERLIGHT    // System.Cryptography.SHA1CryptoServiceProvider
[assembly: PythonModule("_sha512", typeof(IronPython.Modules.PythonSha512))]
namespace IronPython.Modules {
    [Documentation("SHA512 hash algorithm")]
    public static class PythonSha512 {
        private static readonly SHA512 hasher512 = SHA512Managed.Create();
        private static readonly SHA384 hasher384 = SHA384Managed.Create();
        private static readonly int digestSize = hasher512.HashSize / 8;
        private const int blockSize = 1;

        [PythonName("sha512")]
        public static Sha512Object Sha512(object data) {
            return new Sha512Object(hasher512, data);
        }

        [PythonName("sha512")]
        public static Sha512Object Sha512() {
            return new Sha512Object(hasher512);
        }

        [PythonName("sha384")]
        public static Sha384Object Sha384(object data) {
            return new Sha384Object(hasher384, data);
        }

        [PythonName("sha384")]
        public static Sha384Object Sha384() {
            return new Sha384Object(hasher384);
        }

        public sealed class Sha384Object : HashBase, ICloneable {
            internal Sha384Object(HashAlgorithm hasher) : this(hasher, new byte[0]) { }

            internal Sha384Object(HashAlgorithm hasher, object initialData)
                : base(hasher) {
                _bytes = new byte[0];
                Update(initialData);
            }

            private Sha384Object(HashAlgorithm hasher, byte[] initialBytes)
                : base(hasher) {
                _bytes = new byte[0];
                Update(initialBytes);
            }

            [Documentation("copy() -> object (copy of this object)")]
            [PythonName("copy")]
            public object Clone() {
                return new Sha384Object(_hasher, _bytes);
            }
        }

        public sealed class Sha512Object : HashBase, ICloneable {
            internal Sha512Object(HashAlgorithm hasher) : this(hasher, new byte[0]) { }

            internal Sha512Object(HashAlgorithm hasher, object initialData)
                : base(hasher) {
                _bytes = new byte[0];
                Update(initialData);
            }

            private Sha512Object(HashAlgorithm hasher, byte[] initialBytes)
                : base(hasher) {
                _bytes = new byte[0];
                Update(initialBytes);
            }

            [Documentation("copy() -> object (copy of this object)")]
            [PythonName("copy")]
            public object Clone() {
                return new Sha512Object(_hasher, _bytes);
            }
        }        
    }

    public class HashBase {
        internal HashAlgorithm _hasher;
        internal byte[] _bytes;
        private byte[] _hash;

        internal HashBase(HashAlgorithm hasher) {
            _hasher = hasher;
        }

        internal void Update(byte[] newBytes) {
            byte[] updatedBytes = new byte[_bytes.Length + newBytes.Length];
            Array.Copy(_bytes, updatedBytes, _bytes.Length);
            Array.Copy(newBytes, 0, updatedBytes, _bytes.Length, newBytes.Length);
            _bytes = updatedBytes;
            _hash = _hasher.ComputeHash(_bytes);
        }

        [Documentation("update(string) -> None (update digest with string data)")]
        [PythonName("update")]
        public void Update(object newData) {
            Update(StringOps.ToByteArray(Converter.ConvertToString(newData)));
        }


        [Documentation("digest() -> int (current digest value)")]
        [PythonName("digest")]
        public string Digest() {
            return StringOps.FromByteArray(_hash);
        }

        [Documentation("hexdigest() -> string (current digest as hex digits)")]
        [PythonName("hexdigest")]
        public string HexDigest() {
            StringBuilder result = new StringBuilder(2 * _hash.Length);
            for (int i = 0; i < _hash.Length; i++) {
                result.Append(_hash[i].ToString("x2"));
            }
            return result.ToString();
        }
    }

}
#endif
