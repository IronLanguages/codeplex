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

using Microsoft.Scripting.Runtime;

using IronPython.Runtime;

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
        private const int blockSize = 128;

        public const string __doc__ = "SHA512 hash algorithm";

        public static Sha512Object sha512(object data) {
            return new Sha512Object(hasher512, data);
        }

        public static Sha512Object sha512() {
            return new Sha512Object(hasher512);
        }

        public static Sha384Object sha384(object data) {
            return new Sha384Object(hasher384, data);
        }

        public static Sha384Object sha384() {
            return new Sha384Object(hasher384);
        }

        [PythonHidden]
        public sealed class Sha384Object : HashBase, ICloneable {
            internal Sha384Object(HashAlgorithm hasher) : this(hasher, new byte[0]) { }

            internal Sha384Object(HashAlgorithm hasher, object initialData)
                : base(hasher) {
                _bytes = new byte[0];
                update(initialData);
            }

            private Sha384Object(HashAlgorithm hasher, byte[] initialBytes)
                : base(hasher) {
                _bytes = new byte[0];
                update(initialBytes);
            }

            [Documentation("copy() -> object (copy of this object)")]
            public Sha384Object copy() {
                return new Sha384Object(_hasher, _bytes);
            }

            object ICloneable.Clone() {
                return copy();
            }

            public const int block_size = 128;
            public const int digest_size = 48;
            public const int digestsize = 48;
            public const string name = "SHA384";
        }

        [PythonHidden]
        public sealed class Sha512Object : HashBase, ICloneable {
            internal Sha512Object(HashAlgorithm hasher) : this(hasher, new byte[0]) { }

            internal Sha512Object(HashAlgorithm hasher, object initialData)
                : base(hasher) {
                _bytes = new byte[0];
                update(initialData);
            }

            private Sha512Object(HashAlgorithm hasher, byte[] initialBytes)
                : base(hasher) {
                _bytes = new byte[0];
                update(initialBytes);
            }

            [Documentation("copy() -> object (copy of this object)")]
            public Sha512Object copy() {
                return new Sha512Object(_hasher, _bytes);
            }

            object ICloneable.Clone() {
                return copy();
            }

            public const int block_size = 128;
            public const int digest_size = 64;
            public const int digestsize = 64;
            public const string name = "SHA512";
        }        
    }   
}
#endif
