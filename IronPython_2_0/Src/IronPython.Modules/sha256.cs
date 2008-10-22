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

using System; using Microsoft;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

//!!! This is pretty inefficient. We should probably use hasher.TransformBlock instead of
//!!! hanging onto all of the bytes.
//!!! Also, we could probably make a generic version of this that could then be specialized
//!!! for both md5 and sha.

[assembly: PythonModule("_sha256", typeof(IronPython.Modules.PythonSha256))]
namespace IronPython.Modules {
    [Documentation("SHA256 hash algorithm")]
    public static class PythonSha256 {
        private static readonly SHA256 hasher256 = new SHA256Managed();
        private const int blockSize = 64;

        public const string __doc__ = "SHA256 hash algorithm";

        public static Sha256Object sha256(object data) {
            return new Sha256Object(hasher256, data);
        }

        public static Sha256Object sha256() {
            return new Sha256Object(hasher256);
        }

        public static Sha256Object sha224(object data) {
            throw new NotImplementedException();
        }

        public static Sha256Object sha224() {
            throw new NotImplementedException();
        }

        [PythonHidden]
        public sealed class Sha256Object : HashBase
#if !SILVERLIGHT
            , ICloneable 
#endif
        {
            internal Sha256Object(HashAlgorithm hasher) : this(hasher, new byte[0]) { }

            internal Sha256Object(HashAlgorithm hasher, object initialData)
                : base(hasher) {
                _bytes = new byte[0];
                update(initialData);
            }

            private Sha256Object(HashAlgorithm hasher, byte[] initialBytes)
                : base(hasher) {
                _bytes = new byte[0];
                update(initialBytes);
            }

            [Documentation("copy() -> object (copy of this object)")]
            public Sha256Object copy() {
                return new Sha256Object(_hasher, _bytes);
            }
#if !SILVERLIGHT
            object ICloneable.Clone() {
                return copy();
            }
#endif

            public const int block_size = 64;
            public const int digest_size = 32;
            public const int digestsize = 32;
            public const string name = "SHA256";
        }
    }

    public class HashBase {
        internal HashAlgorithm _hasher;
        internal byte[] _bytes;
        private byte[] _hash;

        internal HashBase(HashAlgorithm hasher) {
            _hasher = hasher;
        }

        internal void update(byte[] newBytes) {
            byte[] updatedBytes = new byte[_bytes.Length + newBytes.Length];
            Array.Copy(_bytes, updatedBytes, _bytes.Length);
            Array.Copy(newBytes, 0, updatedBytes, _bytes.Length, newBytes.Length);
            _bytes = updatedBytes;
            _hash = _hasher.ComputeHash(_bytes);
        }

        [Documentation("update(string) -> None (update digest with string data)")]
        public void update(object newData) {
            update(StringOps.ToByteArray(Converter.ConvertToString(newData)));
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
    }
}
