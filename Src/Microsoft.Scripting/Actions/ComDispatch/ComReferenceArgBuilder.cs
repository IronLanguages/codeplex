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

#if !SILVERLIGHT // ComObject

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {
    using Ast = Microsoft.Scripting.Ast.Ast;

    /// <summary>
    /// This allows passing a COM type by reference, given a StrongBox of the managed type.
    /// ReferenceArgBuilder can be used when COM and the CLR agree on the data layout of the data type,
    /// like for integral types. However, strings for eg are represented as BSTR and System.String respectively
    /// in the two worlds. ReferenceArgBuilder cannot be used as is in such cases
    /// </summary>
    internal class ComReferenceArgBuilder : ReferenceArgBuilder {
        private Variable _unmanagedTemp;

        internal ComReferenceArgBuilder(int index, Type parameterType)
            : base(index, parameterType) {
            Debug.Assert(ElementType == typeof(string));
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters) {
            if (_unmanagedTemp == null) {
                _unmanagedTemp = context.GetTemporary(typeof(IntPtr), "unmanagedOutParam");
            }

            // (_unmanagedTemp = Marshal.StringToBSTR(<string from underlying builder>)), &_unmanagedTemp

            return Ast.Comma(
                Ast.Assign(
                    _unmanagedTemp,
                    Ast.Call(
                        typeof(Marshal).GetMethod("StringToBSTR"),
                        base.ToExpression(context, parameters))),
                Ast.Read(_unmanagedTemp));
        }

        protected override Expression UpdatedValue() {
            // Marhsal.PtrToStringBSTR(_unmanagedTemp)
            return Ast.Call(
                typeof(Marshal).GetMethod("PtrToStringBSTR"),
                Ast.Read(_unmanagedTemp));
        }

        internal List<Expression> Clear() {
            List<Expression> exprs = new List<Expression>();
            Expression expr;

            // Marhsal.FreeBSTR(_unmanagedTemp)
            expr = Ast.Statement(
                Ast.Call(
                    typeof(Marshal).GetMethod("FreeBSTR"),
                    Ast.Read(_unmanagedTemp)));
            exprs.Add(expr);
            return exprs;
        }

        public override object Build(CodeContext context, object[] args) {
            object arg = base.Build(context, args);
            // If the argument is null, Type.InvokeMember will not know to marshal it as a VT_BSTR. Hence, wrap it up in a BStrWrapper
            return new BStrWrapper((string)arg);
        }
    }
}

#endif