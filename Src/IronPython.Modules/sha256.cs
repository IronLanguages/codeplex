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
[assembly: PythonModule("_sha256", typeof(IronPython.Modules.PythonSha256))]
namespace IronPython.Modules {
    [Documentation("SHA256 hash algorithm")]
    public static class PythonSha256 {
        private static readonly SHA256 hasher256 = SHA256Managed.Create();
        private static readonly int digestSize = hasher256.HashSize / 8;
        private const int blockSize = 1;

        [PythonName("sha256")]
        public static Sha256Object Sha256(object data) {
            return new Sha256Object(hasher256, data);
        }

        [PythonName("sha256")]
        public static Sha256Object Sha256() {
            return new Sha256Object(hasher256);
        }

        [PythonName("sha224")]
        public static Sha256Object Sha224(object data) {
            throw new NotImplementedException();
        }

        [PythonName("sha224")]
        public static Sha256Object Sha224() {
            throw new NotImplementedException();
        }

        public sealed class Sha256Object : HashBase, ICloneable {
            internal Sha256Object(HashAlgorithm hasher) : this(hasher, new byte[0]) { }

            internal Sha256Object(HashAlgorithm hasher, object initialData)
                : base(hasher) {
                _bytes = new byte[0];
                Update(initialData);
            }

            private Sha256Object(HashAlgorithm hasher, byte[] initialBytes)
                : base(hasher) {
                _bytes = new byte[0];
                Update(initialBytes);
            }

            [Documentation("copy() -> object (copy of this object)")]
            [PythonName("copy")]
            public object Clone() {
                return new Sha256Object(_hasher, _bytes);
            }
        }
    }
}

#endif