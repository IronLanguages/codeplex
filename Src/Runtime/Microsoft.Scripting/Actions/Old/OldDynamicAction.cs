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
using System; using Microsoft;


using Microsoft.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Contracts;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    public enum DynamicActionKind {
        DoOperation,
        ConvertTo,
        GetMember,
        SetMember,
        DeleteMember,
        Call,
        CreateInstance
    }

    public abstract class OldDynamicAction : CallSiteBinder {
        protected OldDynamicAction() {
        }

        public abstract DynamicActionKind Kind { get; }

        [Confined]
        public override string ToString() {
            return Kind.ToString();
        }

        protected static Expression CreateActionBinderReadExpression() {
            return Expression.Property(
                Expression.Property(
                        AstUtils.CodeContext(),
                        typeof(CodeContext),
                        "LanguageContext"
                ),
                typeof(LanguageContext),
                "Binder"
            );
        }
    }
}
