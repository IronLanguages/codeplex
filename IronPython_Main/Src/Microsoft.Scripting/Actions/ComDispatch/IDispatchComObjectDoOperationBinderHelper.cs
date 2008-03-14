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
using System.Reflection;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {
    using Ast = Microsoft.Scripting.Ast.Ast;

    class IDispatchComObjectDoOperationBinderHelper<T> : BinderHelper<T, DoOperationAction> {
        private StandardRule<T> _rule = new StandardRule<T>();

        internal IDispatchComObjectDoOperationBinderHelper(CodeContext context, DoOperationAction action)
            : base(context, action) {
        }

        internal StandardRule<T> MakeNewRule() {
            // Since the only way to get this rule in the first place is to pass through the statically defined pre-binders,
            // the test here essentially redundant.  We need one though, so we'll use the basic type test.
            _rule.MakeTest(typeof(IDispatchComObject));
            _rule.Target = MakeDoOperationTarget();

            return _rule;
        }

        private Expression MakeDoOperationTarget() {
            switch(Action.Operation){
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

            return Ast.Block(
                _rule.MakeReturn(
                    Binder,
                    Ast.SimpleCallHelper(
                        _rule.Parameters[0],
                        _getMemberNamesMethod,
                        Ast.CodeContext())));
        }

        private Expression MakeEqualsOperationTarget() {
            MethodInfo _equalsMethod = typeof(ComObject).GetMethod("Equals");

            return Ast.Block(
                _rule.MakeReturn(
                    Binder,
                    Ast.SimpleCallHelper(
                        _rule.Parameters[0],
                        _equalsMethod,
                        _rule.Parameters[1])));
        }

        private Expression MakeDocumentationOperationTarget() {

            MethodInfo _documentationMethod = typeof(ComObject).GetProperty("Documentation").GetGetMethod();

            return Ast.Block(
                _rule.MakeReturn(
                    Binder, 
                    Ast.SimpleCallHelper(
                        _rule.Parameters[0],
                        _documentationMethod)));
        }

        private Expression MakeIndexOperationTarget() {

            List<Expression> _expressions = new List<Expression>();
            Variable _dispIndexer = _rule.GetTemporary(typeof(object), "dispCallable");

            MethodInfo _indexMethod =
                Action.Operation == Operators.GetItem ?
                    typeof(IDispatchComObject).GetMethod("TryGetGetItem") :
                    typeof(IDispatchComObject).GetMethod("TryGetSetItem");

            _expressions.Add(
                Ast.If(
                    Ast.SimpleCallHelper(
                        _rule.Parameters[0],
                        _indexMethod,
                        Ast.Read(_dispIndexer)),
                    _rule.MakeReturn(
                        Binder,
                        Ast.Action.Call(
                            _rule.ReturnType,
                            ArrayUtils.InsertAt<Expression>(
                                (Expression[])ArrayUtils.RemoveFirst(_rule.Parameters), 
                                0, 
                                Ast.Read(_dispIndexer))))));
            
            _expressions.Add(
                GetFailureStatement(
                    ComObject.ComObjectType, 
                    Action.Operation.ToString()));

            return Ast.Block(_expressions);
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

#endif