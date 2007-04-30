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
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using System.Diagnostics;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting;

namespace Microsoft.Scripting.Internal.Ast {
    public abstract class Statement : Node {
        public static readonly object NextStatement = new object();

        protected Statement() {
        }

        protected Statement(SourceSpan span)
            : base(span) {
        }

        public virtual object Execute(CodeContext context) {
            throw new NotImplementedException(String.Format(CultureInfo.CurrentCulture, Resources.NotImplemented_Execute, this));
        }

        public abstract void Emit(CodeGen cg);
    }
}
