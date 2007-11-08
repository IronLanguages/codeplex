/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;

namespace ToyScript.Runtime {
    class ToyBinder : ActionBinder {
        public ToyBinder(CodeContext context)
            : base(context)  {
        }

        #region ActionBinder overrides

        public override bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel level) {
            return toType.IsAssignableFrom(fromType);
        }

        public override bool PreferConvert(Type t1, Type t2) {
            throw new NotImplementedException();
        }

        public override Expression ConvertExpression(Expression expr, Type toType) {
            return Ast.ConvertHelper(expr, toType);
        }

        public override Expression CheckExpression(Expression expr, Type toType) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
