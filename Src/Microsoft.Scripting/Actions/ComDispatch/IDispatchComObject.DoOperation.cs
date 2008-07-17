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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {

    public partial class IDispatchComObject {

        private sealed class DoOperationBinder<T> : ComBinderHelper<T, OldDoOperationAction> where T : class {

            private readonly RuleBuilder<T> _rule = new RuleBuilder<T>();
            private readonly ComTypeDesc _wrapperType;

            internal DoOperationBinder(CodeContext context, ComTypeDesc wrapperType, OldDoOperationAction action)
                : base(context, action) {

                _wrapperType = wrapperType;
            }

            internal RuleBuilder<T> MakeNewRule() {

                _rule.Test = ComObject.MakeComObjectTest(typeof(IDispatchComObject), typeof(IDispatchComObject).GetProperty("ComTypeDesc"), _wrapperType, _rule);
                _rule.Target = MakeDoOperationTarget();

                return _rule;
            }

            private Expression MakeDoOperationTarget() {

                switch (Action.Operation) {
                    case Operators.GetItem:
                    case Operators.SetItem:
                        return MakeIndexOperationTarget();

                    case Operators.Documentation:
                        return MakeDocumentationOperationTarget();

                    case Operators.Equals:
                        return MakeEqualsOperationTarget();

                    case Operators.GetMemberNames:
                        return MakeGetMemberNamesTarget();
                }

                return null;
            }

            private Expression MakeGetMemberNamesTarget() {

                MethodInfo _getMemberNamesMethod = typeof(ComObject).GetMethod("GetMemberNames");

                return Expression.Block(
                    _rule.MakeReturn(
                        Binder,
                        Expression.SimpleCallHelper(
                            _rule.Parameters[0],
                            _getMemberNamesMethod,
                            _rule.Context)));
            }

            private Expression MakeEqualsOperationTarget() {

                MethodInfo _equalsMethod = typeof(ComObject).GetMethod("Equals");

                return Expression.Block(
                    _rule.MakeReturn(
                        Binder,
                        Expression.SimpleCallHelper(
                            _rule.Parameters[0],
                            _equalsMethod,
                            _rule.Parameters[1])));
            }

            private Expression MakeDocumentationOperationTarget() {

                MethodInfo _documentationMethod = typeof(ComObject).GetProperty("Documentation").GetGetMethod();

                return Expression.Block(
                    _rule.MakeReturn(
                        Binder,
                        Expression.SimpleCallHelper(
                            _rule.Parameters[0],
                            _documentationMethod)));
            }

            private Expression MakeIndexOperationTarget() {

                List<Expression> _expressions = new List<Expression>();
                VariableExpression _dispIndexer = _rule.GetTemporary(typeof(DispCallable), "dispCallable");

                MethodInfo _indexMethod =
                    Action.Operation == Operators.GetItem ?
                        typeof(IDispatchComObject).GetMethod("TryGetGetItem") :
                        typeof(IDispatchComObject).GetMethod("TryGetSetItem");


                // Remove first arg, insert codecontext, dispIndexer
                Expression[] args = ArrayUtils.Insert(_rule.Context, _rule.Parameters);
                args[1] = _dispIndexer;

                _expressions.Add(
                    Expression.If(
                        Expression.SimpleCallHelper(
                            _rule.Parameters[0],
                            _indexMethod,
                            _dispIndexer),
                        _rule.MakeReturn(
                            Binder,
                            Expression.ActionExpression(
                                // TODO: too much magic number
                                // (it's stripping codecontext, dispIndexer)
                                OldCallAction.Make(Binder, new CallSignature(args.Length - 2)),
                                _rule.ReturnType,
                                args
                            )
                        )
                    )
                );

                _expressions.Add(
                    GetFailureStatement(
                        ComObject.ComObjectType,
                        Action.Operation.ToString()));

                return Expression.Block(_expressions);
            }

            /// <summary>
            /// Generate the failure Statement
            ///     if Action.IsNoThrow = True
            ///         OperationFailed.Value 
            ///     else
            ///         MakeMissingMemberError -> returns Undefined.Value
            /// </summary>
            /// <returns></returns>
            private Expression GetFailureStatement(Type type, string memberName) {

                return Binder.MakeMissingMemberError(
                            type,
                            memberName
                       ).MakeErrorForRule(_rule, Binder);
            }
        }
    }
}

#endif
