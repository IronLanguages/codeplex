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
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = Microsoft.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    partial class MetaUserObject : MetaPythonObject, IPythonInvokable {
        private readonly DynamicMetaObject _baseMetaObject;            // if we're a subtype of MetaObject this is the base class MO

        public MetaUserObject(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, DynamicMetaObject baseMetaObject, IPythonObject value)
            : base(expression, restrictions, value) {
            _baseMetaObject = baseMetaObject;
        }

        #region IPythonInvokable Members

        public DynamicMetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region MetaObject Overrides

        public override DynamicMetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ action, DynamicMetaObject/*!*/[]/*!*/ args) {
            return new InvokeBinderHelper(this, action, args, BinderState.GetCodeContext(action)).Bind();
        }

        public override DynamicMetaObject/*!*/ BindConvert(ConvertBinder/*!*/ conversion) {
            Type type = conversion.Type;
            ValidationInfo typeTest = BindingHelpers.GetValidationInfo(this, Value.PythonType);

            return BindingHelpers.AddDynamicTestAndDefer(
                conversion,
                TryPythonConversion(conversion, type) ?? base.BindConvert(conversion),
                new DynamicMetaObject[] { this },
                typeTest,
                type
            );
        }

        public override DynamicMetaObject/*!*/ BindBinaryOperation(BinaryOperationBinder/*!*/ binder, DynamicMetaObject/*!*/ arg) {
            return PythonProtocol.Operation(binder, this, arg, null);
        }

        public override DynamicMetaObject/*!*/ BindUnaryOperation(UnaryOperationBinder/*!*/ binder) {
            return PythonProtocol.Operation(binder, this);
        }

        public override DynamicMetaObject/*!*/ BindGetIndex(GetIndexBinder/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ indexes) {
            return PythonProtocol.Index(binder, PythonIndexType.GetItem, ArrayUtils.Insert(this, indexes));
        }

        public override DynamicMetaObject/*!*/ BindSetIndex(SetIndexBinder/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ indexes, DynamicMetaObject/*!*/ value) {
            return PythonProtocol.Index(binder, PythonIndexType.SetItem, ArrayUtils.Insert(this, ArrayUtils.Append(indexes, value)));
        }

        public override DynamicMetaObject/*!*/ BindDeleteIndex(DeleteIndexBinder/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ indexes) {
            return PythonProtocol.Index(binder, PythonIndexType.DeleteItem, ArrayUtils.Insert(this, indexes));
        }        

        public override DynamicMetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ action, DynamicMetaObject/*!*/[]/*!*/ args) {
            Expression context = Ast.Call(
                typeof(PythonOps).GetMethod("GetPythonTypeContext"),
                Ast.Property(
                    AstUtils.Convert(Expression, typeof(IPythonObject)),
                    "PythonType"
                )
            );

            return InvokeWorker(
                action, 
                context, 
                args
            );
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames() {
            foreach (object o in Value.PythonType.GetMemberNames(Value.PythonType.PythonContext.DefaultBinderState.Context, Value)) {
                if (o is string) {
                    yield return (string)o;
                }
            }
        }

        #endregion

        #region Invoke Implementation

        private DynamicMetaObject/*!*/ InvokeWorker(DynamicMetaObjectBinder/*!*/ action, Expression/*!*/ codeContext, DynamicMetaObject/*!*/[] args) {
            ValidationInfo typeTest = BindingHelpers.GetValidationInfo(this, Value.PythonType);

            return BindingHelpers.AddDynamicTestAndDefer(
                action,
                PythonProtocol.Call(action, this, args) ?? BindingHelpers.InvokeFallback(action, codeContext, this, args),
                args,
                typeTest
            );
        }

        #endregion

        #region Conversions

        private DynamicMetaObject TryPythonConversion(ConvertBinder conversion, Type type) {
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
                        } else if (type == typeof(IEnumerator)){
                            return PythonProtocol.ConvertToIEnumerator(conversion, this);
                        } else if (conversion.Type.IsSubclassOf(typeof(Delegate))) {
                            return MakeDelegateTarget(conversion, conversion.Type, Restrict(Value.GetType()));
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
                    case TypeCode.String:
                        if (!typeof(Extensible<string>).IsAssignableFrom(this.LimitType)) {
                            return MakeConvertRuleForCall(conversion, this, Symbols.String, "ConvertToString");
                        }
                        break;
                }
            }

            return null;
        }

        private DynamicMetaObject/*!*/ MakeConvertRuleForCall(ConvertBinder/*!*/ convertToAction, DynamicMetaObject/*!*/ self, SymbolId symbolId, string returner) {
            PythonType pt = ((IPythonObject)self.Value).PythonType;
            PythonTypeSlot pts;
            CodeContext context = BinderState.GetBinderState(convertToAction).Context;
            ValidationInfo valInfo = BindingHelpers.GetValidationInfo(this, pt);

            if (pt.TryResolveSlot(context, symbolId, out pts) && !IsBuiltinConversion(context, pts, symbolId, pt)) {
                ParameterExpression tmp = Ast.Variable(typeof(object), "func");

                Expression callExpr = Ast.Call(
                    PythonOps.GetConversionHelper(returner, GetResultKind(convertToAction)),
                    Ast.Dynamic(
                        BinderState.GetBinderState(convertToAction).InvokeNone,
                        typeof(object),
                        BinderState.GetCodeContext(convertToAction),
                        tmp
                    )
                );

                if (typeof(Extensible<>).MakeGenericType(convertToAction.Type).IsAssignableFrom(self.GetLimitType())) {
                    // if we're doing a conversion to the underlying type and we're an 
                    // Extensible<T> of that type:

                    // if an extensible type returns it's self in a conversion, then we need 
                    // to actually return the underlying value.  If an extensible just keeps 
                    // returning more instances  of it's self a stack overflow occurs - both 
                    // behaviors match CPython.
                    callExpr = AstUtils.Convert(AddExtensibleSelfCheck(convertToAction, self, callExpr), typeof(object));
                }

                return BindingHelpers.AddDynamicTestAndDefer(
                    convertToAction,
                    new DynamicMetaObject(
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
                        self.Restrict(self.GetRuntimeType()).Restrictions
                    ),
                    new DynamicMetaObject[] { this },
                    valInfo,
                    tmp
                );
            }

            return convertToAction.FallbackConvert(this);
        }

        private static Expression/*!*/ AddExtensibleSelfCheck(ConvertBinder/*!*/ convertToAction, DynamicMetaObject/*!*/ self, Expression/*!*/ callExpr) {
            ParameterExpression tmp = Ast.Variable(callExpr.Type, "tmp");
            callExpr = Ast.Block(
                new ParameterExpression[] { tmp },
                Ast.Block(
                    Ast.Assign(tmp, callExpr),
                    Ast.Condition(
                        Ast.Equal(tmp, self.Expression),
                        Ast.Property(
                            AstUtils.Convert(self.Expression, self.GetLimitType()),
                            self.GetLimitType().GetProperty("Value")
                        ),
                        Ast.Dynamic(
                            new ConversionBinder(
                                BinderState.GetBinderState(convertToAction),
                                convertToAction.Type,
                                ConversionResultKind.ExplicitCast
                            ),
                            convertToAction.Type,
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

            return convertToAction.GetUpdateExpression(typeof(object));
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
        private Expression/*!*/ GetConversionFailedReturnValue(ConversionBinder/*!*/ convertToAction, DynamicMetaObject/*!*/ self) {
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
        private DynamicMetaObject/*!*/ Fallback(DynamicMetaObjectBinder/*!*/ action, Expression codeContext) {
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
                    BinderState.GetBinderState(action).CompatGetMember(
                        GetGetMemberName(action)
                    )
                );
            }

            return GetMemberFallback(this, action, codeContext);
        }

        /// <summary>
        /// Helper for falling back - if we have a base object fallback to it first (which can
        /// then fallback to the calling site), otherwise fallback to the calling site.
        /// </summary>
        private DynamicMetaObject/*!*/ Fallback(SetMemberBinder/*!*/ action, DynamicMetaObject/*!*/ value) {
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
