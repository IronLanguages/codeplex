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

using System.Collections.Generic;

using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Internal.Generation {
    public abstract class ArgBuilder {
        public virtual bool CanGenerate {
            get { return true; }
        }

        public abstract int Priority {
            get;
        }

        public virtual bool NeedsContext {
            get { return false; }
        }

        public abstract Expression ToExpression(ActionBinder binder, Expression[] parameters);

        public abstract object Build(CodeContext context, object[] args);
        public virtual void UpdateFromReturn(object callArg, object[] args) { }

        public abstract void Generate(CodeGen cg, IList<Slot> argSlots);
    }
}
