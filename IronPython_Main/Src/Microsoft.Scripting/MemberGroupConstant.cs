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
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Microsoft.Scripting {
    /// <summary>
    /// Represents a member group which is a collection of members from a type.
    /// 
    /// Currently member groups are limited only to methods.
    /// </summary>
    public sealed class MemberGroupConstant : CompilerConstant {
        private BuiltinFunction _group;

        public MemberGroupConstant(BuiltinFunction group) {
            _group = group;
        }

        public override Type Type {
            get { return typeof(BuiltinFunction); }
        }

        public override void EmitCreation(Microsoft.Scripting.Generation.CodeGen cg) {
            throw new NotImplementedException();
        }

        public override object Create() {
            return _group;
        }
    }
}
