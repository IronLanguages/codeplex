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
using System.Text;

namespace Microsoft.Linq.Expressions {
    public enum GotoExpressionKind {
        Goto,
        Return,
        Break,
        Continue,
    }

    public sealed class GotoExpression : Expression {
        private readonly GotoExpressionKind _kind;
        private readonly Expression _value;
        private readonly LabelTarget _target;

        internal GotoExpression(GotoExpressionKind kind, LabelTarget target, Expression value, Annotations annotations)
            : base(annotations) {
            _kind = kind;
            _value = value;
            _target = target;
        }

        protected override Type GetExpressionType() {
            return typeof(void);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Goto;
        }

        internal override Expression.NodeFlags GetFlags() {
            return NodeFlags.CanRead;
        }

        /// <summary>
        /// The value passed to the target, or null if the target is of type
        /// System.Void
        /// </summary>
        public Expression Value {
            get { return _value; }
        }

        /// <summary>
        /// The target label where this node jumps to
        /// </summary>
        public LabelTarget Target {
            get { return _target; }
        }

        /// <summary>
        /// The kind of the goto. For information purposes only.
        /// </summary>
        public GotoExpressionKind Kind {
            get { return _kind; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitGoto(this);
        }
    }

    /// <summary>
    /// Factory methods
    /// </summary>
    public partial class Expression {
        public static GotoExpression Break(LabelTarget target) {
            return MakeGoto(GotoExpressionKind.Break, target, null, null);
        }

        public static GotoExpression Break(LabelTarget target, Expression value) {
            return MakeGoto(GotoExpressionKind.Break, target, value, null);
        }

        public static GotoExpression Break(LabelTarget target, Expression value, Annotations annotations) {
            return MakeGoto(GotoExpressionKind.Break, target, value, annotations);
        }

        public static GotoExpression Continue(LabelTarget target) {
            return MakeGoto(GotoExpressionKind.Continue, target, null, null);
        }

        public static GotoExpression Continue(LabelTarget target, Annotations annotations) {
            return MakeGoto(GotoExpressionKind.Continue, target, null, annotations);
        }

        public static GotoExpression Return(LabelTarget target) {
            return MakeGoto(GotoExpressionKind.Return, target, null, null);
        }

        public static GotoExpression Return(LabelTarget target, Expression value) {
            return MakeGoto(GotoExpressionKind.Return, target, value, null);
        }

        public static GotoExpression Return(LabelTarget target, Expression value, Annotations annotations) {
            return MakeGoto(GotoExpressionKind.Return, target, value, annotations);
        }

        public static GotoExpression Goto(LabelTarget target) {
            return MakeGoto(GotoExpressionKind.Goto, target, null, null);
        }

        public static GotoExpression Goto(LabelTarget target, Expression value) {
            return MakeGoto(GotoExpressionKind.Goto, target, value, null);
        }

        public static GotoExpression Goto(LabelTarget target, Expression value, Annotations annotations) {
            return MakeGoto(GotoExpressionKind.Goto, target, value, annotations);
        }

        public static GotoExpression MakeGoto(GotoExpressionKind kind, LabelTarget target, Expression value, Annotations annotations) {
            ValidateGoto(target, ref value, "target", "value");
            return new GotoExpression(kind, target, value, annotations);
        }

        private static void ValidateGoto(LabelTarget target, ref Expression value, string targetParameter, string valueParameter) {
            ContractUtils.RequiresNotNull(target, targetParameter);
            if (value == null) {
                ContractUtils.Requires(target.Type == typeof(void), Strings.LabelMustBeVoidOrHaveExpression);
            } else {
                ValidateGotoType(target.Type, ref value, valueParameter);
            }
        }

        // Standard argument validation, taken from ValidateArgumentTypes
        private static void ValidateGotoType(Type expectedType, ref Expression value, string paramName) {
            RequiresCanRead(value, paramName);
            if (!TypeUtils.AreReferenceAssignable(expectedType, value.Type)) {
                // C# autoquotes return values, so we'll do that here
                if (TypeUtils.IsSameOrSubclass(typeof(Expression), expectedType) &&
                    TypeUtils.AreAssignable(expectedType, value.GetType())) {
                    value = Expression.Quote(value);
                }
                throw Error.ExpressionTypeDoesNotMatchLabel(value.Type, expectedType);
            }
        }

    }
}
