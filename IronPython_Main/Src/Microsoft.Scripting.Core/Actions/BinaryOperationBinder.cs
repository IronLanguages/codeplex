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


using Microsoft.Scripting.Utils;
using Microsoft.Contracts;
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting {
    public abstract class BinaryOperationBinder : MetaObjectBinder {
        private ExpressionType _operation;

        protected BinaryOperationBinder(ExpressionType operation) {
            ContractUtils.Requires(OperationIsValid(operation), "operation");
            _operation = operation;
        }

        public ExpressionType Operation {
            get {
                return _operation;
            }
        }

        public MetaObject FallbackBinaryOperation(MetaObject target, MetaObject arg) {
            return FallbackBinaryOperation(target, arg, null);
        }

        public abstract MetaObject FallbackBinaryOperation(MetaObject target, MetaObject arg, MetaObject errorSuggestion);

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.Requires(args.Length == 1);

            return target.BindBinaryOperation(this, args[0]);
        }

        [Confined]
        public override bool Equals(object obj) {
            BinaryOperationBinder oa = obj as BinaryOperationBinder;
            return oa != null && oa._operation == _operation;
        }

        [Confined]
        public override int GetHashCode() {
            return BinaryOperationBinderHash ^ (int)_operation;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool OperationIsValid(ExpressionType operation) {
            switch (operation) {
                #region Generated Binary Operation Binder Validator

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_binop_validator from: generate_tree.py

                case ExpressionType.Add:
                case ExpressionType.And:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:

                // *** END GENERATED CODE ***

                #endregion

                case ExpressionType.Extension:
                    return true;

                default:
                    return false;
            }
        }
    }
}
