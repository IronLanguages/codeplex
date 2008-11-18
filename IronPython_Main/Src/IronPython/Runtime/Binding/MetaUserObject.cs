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
using System.Collections;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Binders;
using Microsoft.Scripting.Runtime;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    partial class MetaUserObject : MetaPythonObject, IPythonInvokable {
        private readonly MetaObject _baseMetaObject;            // if we're a subtype of MetaObject this is the base class MO

        public MetaUserObject(Expression/*!*/ expression, Restrictions/*!*/ restrictions, MetaObject baseMetaObject, IPythonObject value)
            : base(expression, restrictions, value) {
            _baseMetaObject = baseMetaObject;
        }

        #region IPythonInvokable Members

        public MetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region MetaObject Overrides

        public override MetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            CodeContext context = BinderState.GetBinderState(action).Context;
            IPythonObject sdo = (IPythonObject)args[0].Value;
            PythonTypeSlot foundSlot;

            if (TryGetGetAttribute(context, sdo.PythonType, out foundSlot)) {
                // we'll always fetch the value, go ahead and invoke afterwards.
                return BindingHelpers.GenericCall(action, this, args);
            }

            bool isOldStyle;
            bool systemTypeResolution;
            foundSlot = FindSlot(context, action.Name, sdo, out isOldStyle, out systemTypeResolution);
            if (foundSlot != null && !systemTypeResolution) {
                // we found the member in the type dictionary, not a .NET type, go ahead and
                // do the get & invoke.
                return BindingHelpers.GenericCall(action, this, args);
            }

            // it's a normal .NET member, let the calling language handle it how it usually does
            return action.FallbackInvokeMember(this, args);
        }

        public override MetaObject/*!*/ BindConvert(ConvertBinder/*!*/ conversion) {
            Type type = conversion.Type;
            ValidationInfo typeTest = BindingHelpers.GetValidationInfo(Expression, Value.PythonType);

            return BindingHelpers.AddDynamicTestAndDefer(
                conversion,
                TryPythonConversion(conversion, type) ?? base.BindConvert(conversion),
                new MetaObject[] { this },
                typeTest
            );
        }

        [Obsolete]
        public override MetaObject/*!*/ BindOperation(OperationBinder/*!*/ operation, params MetaObject/*!*/[]/*!*/ args) {
            return PythonProtocol.Operation(operation, ArrayUtils.Insert(this, args));
        }

        public override MetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
            Expression context = Ast.Call(
                typeof(PythonOps).GetMethod("GetPythonTypeContext"),
                Ast.Property(
                    Ast.Convert(Expression, typeof(IPythonObject)),
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

        private MetaObject/*!*/ InvokeWorker(MetaObjectBinder/*!*/ action, Expression/*!*/ codeContext, MetaObject/*!*/[] args) {
            ValidationInfo typeTest = BindingHelpers.GetValidationInfo(Expression, Value.PythonType);

            return BindingHelpers.AddDynamicTestAndDefer(
                action,
                PythonProtocol.Call(action, this, args) ?? BindingHelpers.InvokeFallback(action, codeContext, this, args),
                args,
                typeTest
            );
        }

        #endregion

        #region Conversions

        private MetaObject TryPythonConversion(ConvertBinder conversion, Type type) {
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Object:
                        if (type == typeof(Complex64)) {
                            // TODO: Fallback to Float
                            return MakeConvertRuleForCall(conversion, this, Symbols.ConvertToComplex, "ConvertToComplex");
                        } else if (type == typeof(BigInteger)) {
                            return MakeConvertRuleForCall(conversion, this, Symbols.ConvertToLong, "ConvertToLong");
                        } else if (type == typeof(IEnumerable)) {
                            return PythonProtocol.ConvertToIEnumerable(conversion, this);
                        } else if (type == typeof(IEnumerator)) {
                            return PythonProtocol.ConvertToIEnumerator(conversion, this);
                        }
                        break;
                    case TypeCode.Int32:
                        return MakeConvertRuleForCall(conversion, this, Symbols.ConvertToInt, "ConvertToInt");
                    case TypeCode.Double:
                        return MakeConvertRuleForCall(conversion, this, Symbols.ConvertToFloat, "ConvertToFloat");
                    case TypeCode.Boolean:
                        return PythonProtocol.ConvertToBool(
                            conversion,
                            this
                        );
                }
            }

            return null;
        }

        private MetaObject/*!*/ MakeConvertRuleForCall(ConvertBinder/*!*/ convertToAction, MetaObject/*!*/ self, SymbolId symbolId, string returner) {
            PythonType pt = ((IPythonObject)self.Value).PythonType;
            PythonTypeSlot pts;
            CodeContext context = BinderState.GetBinderState(convertToAction).Context;

            if (pt.TryResolveSlot(context, symbolId, out pts) && !IsBuiltinConversion(context, pts, symbolId, pt)) {
                ParameterExpression tmp = Ast.Variable(typeof(object), "func");

                Expression callExpr = Ast.Call(
                    PythonOps.GetConversionHelper(returner, GetResultKind(convertToAction)),
                    Ast.Dynamic(
                        new PythonInvokeBinder(
                            BinderState.GetBinderState(convertToAction),
                            new CallSignature(0)
                        ),
                        typeof(object),
                        BinderState.GetCodeContext(convertToAction),
                        tmp
                    )
                );

                if (typeof(Extensible<>).MakeGenericType(convertToAction.Type).IsAssignableFrom(self.LimitType)) {
                    // if we're doing a conversion to the underlying type and we're an 
                    // Extensible<T> of that type:

                    // if an extensible type returns it's self in a conversion, then we need 
                    // to actually return the underlying value.  If an extensible just keeps 
                    // returning more instances  of it's self a stack overflow occurs - both 
                    // behaviors match CPython.
                    callExpr = AstUtils.Convert(AddExtensibleSelfCheck(convertToAction, self, callExpr), typeof(object));
                }

                return new MetaObject(
                    Ast.Block(
                        new ParameterExpression[] { tmp },
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
                                AstUtils.Convert(
                                    ConversionFallback(convertToAction),
                                    typeof(object)
                                )
                            ),
                            convertToAction.Defer(this).Expression
                        )
                    ),
                    self.Restrict(self.RuntimeType).Restrictions
                );
            }

            return convertToAction.FallbackConvert(this);
        }

        private static Expression/*!*/ AddExtensibleSelfCheck(ConvertBinder/*!*/ convertToAction, MetaObject/*!*/ self, Expression/*!*/ callExpr) {
            ParameterExpression tmp = Ast.Variable(callExpr.Type, "tmp");
            callExpr = Ast.Block(
                new ParameterExpression[] { tmp },
                Ast.Block(
                    Ast.Assign(tmp, callExpr),
                    Ast.Condition(
                        Ast.Equal(tmp, self.Expression),
                        Ast.Property(
                            Ast.Convert(self.Expression, self.LimitType),
                            self.LimitType.GetProperty("Value")
                        ),
                        Binders.Convert(
                            BinderState.GetBinderState(convertToAction),
                            convertToAction.Type,
                            ConversionResultKind.ExplicitCast,
                            tmp
                        )
                    )
                )
            );
            return callExpr;
        }

        private ConversionResultKind GetResultKind(ConvertBinder convertToAction) {
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

        private Expression ConversionFallback(ConvertBinder/*!*/ convertToAction) {
            ConversionBinder cb = convertToAction as ConversionBinder;
            if (cb != null) {
                return GetConversionFailedReturnValue(cb, this);
            }

            return convertToAction.Defer(this).Expression;
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
                    return DefaultBinder.GetTryConvertReturnValue(convertToAction.Type);
                case ConversionResultKind.ExplicitCast:
                case ConversionResultKind.ImplicitCast:
                    DefaultBinder db = BinderState.GetBinderState(convertToAction).Binder;
                    return DefaultBinder.MakeError(
                        db.MakeConversionError(
                            convertToAction.Type,
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
        private MetaObject/*!*/ Fallback(MetaObjectBinder/*!*/ action, Expression codeContext) {
            if (_baseMetaObject != null) {
                IPythonGetable ipyget = _baseMetaObject as IPythonGetable;
                if (ipyget != null) {
                    PythonGetMemberBinder gmb = action as PythonGetMemberBinder;
                    if (gmb != null) {
                        return ipyget.GetMember(gmb, codeContext);
                    }
                }

                GetMemberBinder gma = action as GetMemberBinder;
                if (gma != null) {
                    return _baseMetaObject.BindGetMember(gma);
                }

                return _baseMetaObject.BindGetMember(
                    new CompatibilityGetMember(
                        BinderState.GetBinderState(action),
                        GetGetMemberName(action)
                    )
                );
            }

            return GetMemberFallback(action, codeContext);
        }

        /// <summary>
        /// Helper for falling back - if we have a base object fallback to it first (which can
        /// then fallback to the calling site), otherwise fallback to the calling site.
        /// </summary>
        private MetaObject/*!*/ Fallback(SetMemberBinder/*!*/ action, MetaObject/*!*/ value) {
            if (_baseMetaObject != null) {
                return _baseMetaObject.BindSetMember(action, value);
            }

            return action.FallbackSetMember(this, value);
        }

        #endregion

        public new IPythonObject Value {
            get {
                return (IPythonObject)base.Value;
            }
        }
    }
}