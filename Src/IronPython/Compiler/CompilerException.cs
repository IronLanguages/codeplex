/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.IO;

using Microsoft.Scripting.Ast;

namespace IronPython.Compiler {

    /// <summary>
    /// Used to prevent further processing after an error while compiling for CodeDom
    /// 
    /// This is the only exception we catch - all other exceptions propagate to our caller.
    /// </summary>
    [Serializable]
    public class CompilerException : Exception {
        private readonly Node _node;
        private readonly string _filename;

        public CompilerException() {
        }

        public CompilerException(string message, Node node, string filename)
            : base(message) {
            _node = node;
            _filename = filename;
        }

        public CompilerException(string message)
            : base(message) {
        }

        public CompilerException(string message, Exception innerException)
            : base(message, innerException) {
        }

        public Node Node {
            get { return _node; }
        }

        public string Filename {
            get { return _filename; }
        } 

#if !SILVERLIGHT // SerializationInfo
        protected CompilerException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) {
        }

        public override void GetObjectData(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) {
            base.GetObjectData(info, context);
        }
#endif
    }
}
