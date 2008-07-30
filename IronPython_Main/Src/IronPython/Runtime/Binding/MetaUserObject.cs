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
using System.Collections;
using System.Linq.Expressions;
using Microsoft.Scripting;
using System.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    partial class MetaUserObject : MetaPythonObject, IPythonInvokable {
        private readonly MetaObject _baseMetaObject;            // if we're a subtype of MetaObject this is the base class MO

        public MetaUserObject(Expression/*!*/ expression, Restrictions/*!*/ restrictions, MetaObject baseMetaObject, IPythonObject value)
            : base(expression, restrictions, value) {
            _baseMetaObject = baseMetaObject;
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(InvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ Call(CallAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, args);
        }

        public override MetaObject/*!*/ Convert(ConvertAction/*!*/ conversion, MetaObject/*!*/[]/*!*/ args) {
            Type type = conversion.ToType;
            ValidationInfo typeTest = BindingHelpers.GetValidationInfo(Expression, Value.PythonType);

            return BindingHelpers.AddDynamicTestAndDefer(
                conversion,
                TryPythonConversion(conversion, type, args) ?? base.Convert(conversion, args),
                args,
                typeTest
            );
        }

        public override MetaObject/*!*/ Operation(OperationAction/*!*/ operation, params MetaObject/*!*/[]/*!*/ args) {
            return PythonProtocol.Operation(operation, args);
        }

        public override MetaObject/*!*/ Invoke(InvokeAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            Expression context = Ast.Call(
                typeof(PythonOps).GetMethod("GetPythonTypeContext"),
                Ast.Property(
                    Ast.Convert(args[0].Expression, typeof(IPythonObject)),
                    "PythonType"
                )
            );

            return InvokeWorker(
                action, 
                context, 
                args
            );
        }

        #endregion

        #region Invoke Implementation

        private MetaObject/*!*/ InvokeWorker(MetaAction/*!*/ action, Expression/*!*/ codeContext, MetaObject/*!*/[] args) {
            ValidationInfo typeTest = BindingHelpers.GetValidationInfo(Expression, Value.PythonType);

            return BindingHelpers.AddDynamicTestAndDefer(
                action,
                PythonProtocol.Call(action, args) ?? BindingHelpers.InvokeFallback(action, codeContext, args),
                args,
                typeTest
            );
        }

        #endregion

        #region Conversions

        private MetaObject TryPythonConversion(ConvertAction conversion, Type type, MetaObject/*!*/[]/*!*/ args) {
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Object:
                        if (type == typeof(Complex64)) {
                            // TODO: Fallback to Float
                            return MakeConvertRuleForCall(conversion, this, args, Symbols.ConvertToComplex, "ConvertToComplex");
                        } else if (type == typeof(BigInteger)) {
                            return MakeConvertRuleForCall(conversion, this, args, Symbols.ConvertToLong, "ConvertToLong");
                        } else if (type == typeof(IEnumerable)) {
                            return PythonProtocol.ConvertToIEnumerable(conversion, this);
                        } else if (type == typeof(IEnumerator)) {
                            return PythonProtocol.ConvertToIEnumerator(conversion, this);
                        }
                        break;
                    case TypeCode.Int32:
                        return MakeConvertRuleForCall(conversion, this, args, Symbols.ConvertToInt, "ConvertToInt");
                    case TypeCode.Double:
                        return MakeConvertRuleForCall(conversion, this, args, Symbols.ConvertToFloat, "ConvertToFloat");
                    case TypeCode.Boolean:
                        return PythonProtocol.ConvertToBool(
                            conversion,
                            this
                        );
                }
            }

            return null;
        }

        private MetaObject/*!*/ MakeConvertRuleForCall(ConvertAction/*!*/ convertToAction, MetaObject/*!*/ self, MetaObject/*!*/[]/*!*/ args, SymbolId symbolId, string returner) {
            PythonType pt = ((IPythonObject)self.Value).PythonType;
            PythonTypeSlot pts;
            CodeContext context = BinderState.GetBinderState(convertToAction).Context;

            if (pt.TryResolveSlot(context, symbolId, out pts) && !IsBuiltinConversion(context, pts, symbolId, pt)) {
                VariableExpression tmp = Ast.Variable(typeof(object), "func");

                Expression callExpr = Ast.Call(
                    PythonOps.GetConversionHelper(returner, GetResultKind(convertToAction)),
                    Ast.ActionExpression(
                        new InvokeBinder(
                            BinderState.GetBinderState(convertToAction),
                            new CallSignature(0)
                        ),
                        typeof(object),
                        BinderState.GetCodeContext(convertToAction),
                        tmp
                    )
                );

                if (typeof(Extensible<>).MakeGenericType(convertToAction.ToType).IsAssignableFrom(self.LimitType)) {
                    // if we're doing a conversion to the underlying type and we're an 
                    // Extensible<T> of that type:

                    // if an extensible type returns it's self in a conversion, then we need 
                    // to actually return the underlying value.  If an extensible just keeps 
                    // returning more instances  of it's self a stack overflow occurs - both 
                    // behaviors match CPython.
                    callExpr = Ast.ConvertHelper(AddExtensibleSelfCheck(convertToAction, self, callExpr), typeof(object));
                }

                return new MetaObject(
                    Ast.Scope(
                        Ast.Condition(
                            BindingHelpers.CheckTypeVersion(
                                self.Expression,
                                pt.Version
                            ),
                            Ast.Condition(
                                MakeTryGetTypeMember(
                                    BinderState.GetBinderState(convertToAction),
                                    pts,
                                    self.Expression,
                                    tmp
                                ),
                                callExpr,
                                Ast.ConvertHelper(
                                    ConversionFallback(convertToAction, args),
                                    typeof(object)
                                )
                            ),
                            convertToAction.Defer(args).Expression
                        ),
                        tmp
                    ),
                    self.Restrict(self.RuntimeType).Restrictions
                );
            }

            return convertToAction.Fallback(args);
        }

        private static Expression/*!*/ AddExtensibleSelfCheck(ConvertAction/*!*/ convertToAction, MetaObject/*!*/ self, Expression/*!*/ callExpr) {
            VariableExpression tmp = Ast.Variable(callExpr.Type, "tmp");
            callExpr = Ast.Scope(
                Ast.Comma(
                    Ast.Assign(tmp, callExpr),
                    Ast.Condition(
                        Ast.Equal(tmp, self.Expression),
                        Ast.Property(
                            Ast.Convert(self.Expression, self.LimitType),
                            self.LimitType.GetProperty("Value")
                        ),
                        Binders.Convert(
                            BinderState.GetBinderState(convertToAction),
                            convertToAction.ToType,
                            ConversionResultKind.ExplicitCast,
                            tmp
                        )
                    )
                ),
                tmp
            );
            return callExpr;
        }

        private ConversionResultKind GetResultKind(ConvertAction convertToAction) {
            ConversionBinder cb = convertToAction as ConversionBinder;
            if (cb != null) {
                return cb.ResultKind;
            }

            if (convertToAction.Explicit) {
                return ConversionResultKind.ExplicitCast;
            } else {
                return ConversionResultKind.ImplicitCast;
            }
        }

        private Expression ConversionFallback(ConvertAction/*!*/ convertToAction, MetaObject/*!*/[]/*!*/ args) {
            ConversionBinder cb = convertToAction as ConversionBinder;
            if (cb != null) {
                return GetConversionFailedReturnValue(cb, args[0]);
            }

            return convertToAction.Defer(args).Expression;
        }

        private static bool IsBuiltinConversion(CodeContext/*!*/ context, PythonTypeSlot/*!*/ pts, SymbolId name, PythonType/*!*/ selfType) {
            Type baseType = selfType.UnderlyingSystemType.BaseType;
            Type tmpType = baseType;
            do {
                if (tmpType.IsGenericType && tmpType.GetGenericTypeDefinition() == typeof(Extensible<>)) {
                    baseType = tmpType.GetGenericArguments()[0];
                    break;
                }
                tmpType = tmpType.BaseType;
            } while (tmpType != null);

            PythonType ptBase = DynamicHelpers.GetPythonTypeFromType(baseType);
            PythonTypeSlot baseSlot;
            if (ptBase.TryResolveSlot(context, name, out baseSlot) && pts == baseSlot) {
                return true;
            }

            return false;
        }

        /// <summary>
        ///  Various helpers related to calling Python __*__ conversion methods 
        /// </summary>
        private Expression/*!*/ GetConversionFailedReturnValue(ConversionBinder/*!*/ convertToAction, MetaObject/*!*/ self) {
            switch (convertToAction.ResultKind) {
                case ConversionResultKind.ImplicitTry:
                case ConversionResultKind.ExplicitTry:
                    return DefaultBinder.GetTryConvertReturnValue(convertToAction.ToType);
                case ConversionResultKind.ExplicitCast:
                case ConversionResultKind.ImplicitCast:
                    DefaultBinder db = BinderState.GetBinderState(convertToAction).Binder;
                    return DefaultBinder.MakeError(
                        db.MakeConversionError(
                            convertToAction.ToType,
                            self.Expression
                        )
                    );
                default:
                    throw new InvalidOperationException(convertToAction.ResultKind.ToString());
            }
        }

        /// <summary>
        /// Helper for falling back - if we have a base object fallback to it first (which can
        /// then fallback to the calling site), otherwise fallback to the calling site.
        /// </summary>
        private MetaObject/*!*/ Fallback(DeleteMemberAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            if (_baseMetaObject != null) {
                return _baseMetaObject.DeleteMember(action, args);
            }

            return action.Fallback(args);
        }

        /// <summary>
        /// Helper for falling back - if we have a base object fallback to it first (which can
        /// then fallback to the calling site), otherwise fallback to the calling site.
        /// </summary>
        private MetaObject/*!*/ Fallback(GetMemberAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            if (_baseMetaObject != null) {
                return _baseMetaObject.GetMember(action, args);
            }

            return action.Fallback(args);
        }

        /// <summary>
        /// Helper for falling back - if we have a base object fallback to it first (which can
        /// then fallback to the calling site), otherwise fallback to the calling site.
        /// </summary>
        private MetaObject/*!*/ Fallback(SetMemberAction/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            if (_baseMetaObject != null) {
                return _baseMetaObject.SetMember(action, args);
            }

            return action.Fallback(args);
        }

        #endregion

        public new IPythonObject Value {
            get {
                return (IPythonObject)base.Value;
            }
        }
    }
}
