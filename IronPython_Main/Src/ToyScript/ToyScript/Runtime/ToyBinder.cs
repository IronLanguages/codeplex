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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;

namespace ToyScript.Runtime {
    class ToyBinder : ActionBinder {
        public ToyBinder(CodeContext context)
            : base(context)  {
        }

        protected override StandardRule<T> MakeRule<T>(CodeContext callerContext, DynamicAction action, object[] args) {
            StandardRule<T> rule = null; 
            if (action.Kind == DynamicActionKind.DoOperation) {
                rule = MakeDoRule<T>((DoOperationAction)action, args);
            }
            return rule ?? base.MakeRule<T>(callerContext, action, args);            
        }

        private StandardRule<T> MakeDoRule<T>(DoOperationAction action, object[] args) {
            if (action.Operation == Operators.Add &&
                args[0] is string &&
                args[1] is string) {

                StandardRule<T> rule = new StandardRule<T>();

                // (arg0 is string && args1 is string)
                rule.Test = Ast.AndAlso(
                    Ast.TypeIs(
                        rule.Parameters[0],
                        typeof(string)
                    ),
                    Ast.TypeIs(
                        rule.Parameters[1],
                        typeof(string)
                    )
                );

                // string.Concat(string str0, string str1);
                rule.Target =
                    rule.MakeReturn(this,
                        Ast.Call(
                            typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }),
                            Ast.Convert(rule.Parameters[0], typeof(string)),
                            Ast.Convert(rule.Parameters[1], typeof(string))
                        )
                    );

                return rule;
            }

            return null;
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

        protected override IList<Type> GetExtensionTypes(Type t) {
            if (t == typeof(string)) {
                return new Type[] { typeof(StringExtensions) };
            }
            return Type.EmptyTypes;
        }

        #endregion
    }
}
