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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Runtime;


using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;

    class GetMemberBinder : GetMemberAction, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;
        private readonly bool _isNoThrow;

        public GetMemberBinder(BinderState/*!*/ binder, string/*!*/ name)
            : base(name, false) {
            _state = binder;
        }

        public GetMemberBinder(BinderState/*!*/ binder, string/*!*/ name, bool isNoThrow)
            : base(name, false) {
            _state = binder;
            _isNoThrow = isNoThrow;
        }

        public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args, MetaObject onBindingError) {
            // Python always provides an extra arg to GetMember to flow the context.
            Debug.Assert(args.Length == 2 && args[1].Expression.Type == typeof(CodeContext));

            return FallbackWorker(args, args[1].Expression, Name, _isNoThrow, this);
        }

        internal static MetaObject FallbackWorker(MetaObject/*!*/[] args, Expression/*!*/ codeContext, string name, bool isNoThrow, MetaAction action) {
            if (args[0].NeedsDeferral()) {
                return action.Defer(args);
            }

            Type limitType = args[0].LimitType;

            if (limitType == typeof(None) || PythonBinder.IsPythonType(limitType)) {
                // look up in the PythonType so that we can 
                // get our custom method names (e.g. string.startswith)            
                PythonType argType = DynamicHelpers.GetPythonTypeFromType(limitType);

                // if the name is defined in the CLS context but not the normal context then
                // we will hide it.                
                if (argType.IsHiddenMember(name)) {
                    MetaObject baseRes = BinderState.GetBinderState(action).Binder.GetMember(
                        name,
                        args[0],
                        args[1].Expression,
                        isNoThrow
                    );
                    Expression failure = GetFailureExpression(limitType, name, isNoThrow, action);

                    return BindingHelpers.FilterShowCls(codeContext, action, baseRes, failure);
                }
            }

            if (args[0].LimitType == typeof(OldInstance)) {
                if (isNoThrow) {
                    return new MetaObject(
                        Ast.Field(
                            null,
                            typeof(OperationFailed).GetField("Value")
                        ),
                        args[0].Restrictions.Merge(Restrictions.TypeRestriction(args[0].Expression, typeof(OldInstance)))
                    );
                } else {
                    return new MetaObject(
                        Ast.Throw(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("AttributeError"),
                                Ast.Constant("{0} instance has no attribute '{1}'"),
                                Ast.NewArrayInit(
                                    typeof(object),
                                    Ast.Constant(((OldInstance)args[0].Value)._class._name),
                                    Ast.Constant(name)
                                )
                            )
                        ),
                        args[0].Restrictions.Merge(Restrictions.TypeRestriction(args[0].Expression, typeof(OldInstance)))
                    );
                }
            }

            return BinderState.GetBinderState(action).Binder.GetMember(name, args[0], codeContext, isNoThrow);
        }

        private static Expression/*!*/ GetFailureExpression(Type/*!*/ limitType, string name, bool isNoThrow, MetaAction action) {
            return isNoThrow ?
                Ast.Field(null, typeof(OperationFailed).GetField("Value")) :
                DefaultBinder.MakeError(
                    BinderState.GetBinderState(action).Binder.MakeMissingMemberError(
                        limitType,
                        name
                    )                    
                );
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        public bool IsNoThrow {
            get {
                return _isNoThrow;
            }
        }

        public override object HashCookie {
            get { return this; }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode() ^ (_isNoThrow ? 1 : 0);
        }

        public override bool Equals(object obj) {
            GetMemberBinder ob = obj as GetMemberBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && 
                ob._isNoThrow == _isNoThrow &&
                base.Equals(obj);
        }

        public override string ToString() {
            return String.Format("Python GetMember {0} IsNoThrow: {1}", Name, _isNoThrow);
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeGetAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Ast.Constant(Name),
                Ast.Constant(IsNoThrow)
            );
        }

        #endregion
    }

    class CompatibilityGetMember : GetMemberAction, IPythonSite {
        private readonly BinderState/*!*/ _state;

        public CompatibilityGetMember(BinderState/*!*/ binder, string/*!*/ name)
            : this(binder, name, false) {
        }

        public CompatibilityGetMember(BinderState/*!*/ binder, string/*!*/ name, bool ignoreCase)
            : base(name, ignoreCase) {
            _state = binder;
        }

        public override MetaObject/*!*/ Fallback(MetaObject/*!*/[]/*!*/ args, MetaObject onBindingError) {
            return GetMemberBinder.FallbackWorker(args, BinderState.GetCodeContext(this), Name, false, this);
        }

        #region IPythonSite Members

        public BinderState Binder {
            get { return _state; }
        }
        
        #endregion

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            CompatibilityGetMember ob = obj as CompatibilityGetMember;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder &&
                base.Equals(obj);
        }

        public override object HashCookie {
            get { return this; }
        }

    }
}
