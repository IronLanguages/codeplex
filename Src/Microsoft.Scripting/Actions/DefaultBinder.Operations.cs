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
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using System.Text;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions.Calls;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Linq.Expressions.Expression;

    public partial class DefaultBinder : ActionBinder {
        public DynamicMetaObject DoOperation(string operation, params DynamicMetaObject[] args) {
            return DoOperation(operation, Ast.Constant(null, typeof(CodeContext)), args);
        }

        public DynamicMetaObject DoOperation(string operation, Expression codeContext, params DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(operation, "operation");
            ContractUtils.RequiresNotNull(codeContext, "codeContext");
            ContractUtils.RequiresNotNullItems(args, "args");

            return MakeGeneralOperatorRule(operation, codeContext, args);   // Then try comparison / other ExpressionType
        }

        private enum IndexType {
            Get,
            Set
        }

        /// <summary>
        /// Creates the MetaObject for indexing directly into arrays or indexing into objects which have
        /// default members.  Returns null if we're not an indexing operation.
        /// </summary>
        public DynamicMetaObject GetIndex(DynamicMetaObject[] args) {
            if (args[0].GetLimitType().IsArray) {
                return MakeArrayIndexRule(IndexType.Get, args);
            }

            return MakeMethodIndexRule(IndexType.Get, args);
        }

        /// <summary>
        /// Creates the MetaObject for indexing directly into arrays or indexing into objects which have
        /// default members.  Returns null if we're not an indexing operation.
        /// </summary>
        public DynamicMetaObject SetIndex(DynamicMetaObject[] args) {
            if (args[0].LimitType.IsArray) {
                return MakeArrayIndexRule(IndexType.Set, args);
            }

            return MakeMethodIndexRule(IndexType.Set, args);
        }

        public DynamicMetaObject GetDocumentation(DynamicMetaObject target) {
            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);

            object[] attrs = target.LimitType.GetCustomAttributes(typeof(DocumentationAttribute), true);
            string documentation = String.Empty;

            if (attrs.Length > 0) {
                documentation = ((DocumentationAttribute)attrs[0]).Documentation;
            }

            return new DynamicMetaObject(
                Ast.Constant(documentation),
                restrictions
            );
        }

        public DynamicMetaObject GetMemberNames(DynamicMetaObject target, Expression codeContext) {
            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);

            if (typeof(IMembersList).IsAssignableFrom(target.LimitType)) {
                return MakeIMembersListRule(codeContext, target);
            }

            MemberInfo[] members = target.LimitType.GetMembers();
            Dictionary<string, string> mems = new Dictionary<string, string>();
            foreach (MemberInfo mi in members) {
                mems[mi.Name] = mi.Name;
            }

            string[] res = new string[mems.Count];
            mems.Keys.CopyTo(res, 0);

            return new DynamicMetaObject(
                Ast.Constant(res),
                restrictions
            );
        }

        public DynamicMetaObject GetCallSignatures(DynamicMetaObject target) {
            return MakeCallSignatureResult(CompilerHelpers.GetMethodTargets(target.LimitType), target);
        }

        public DynamicMetaObject GetIsCallable(DynamicMetaObject target) {
            // IsCallable() is tightly tied to Call actions. So in general, we need the call-action providers to also
            // provide IsCallable() status. 
            // This is just a rough fallback. We could also attempt to simulate the default CallBinder logic to see
            // if there are any applicable calls targets, but that would be complex (the callbinder wants the argument list, 
            // which we don't have here), and still not correct. 
            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);

            bool callable = false;
            if (typeof(Delegate).IsAssignableFrom(target.LimitType) ||
                typeof(MethodGroup).IsAssignableFrom(target.LimitType)) {
                callable = true;
            }

            return new DynamicMetaObject(
                Ast.Constant(callable),
                restrictions
            );
        }

        /// <summary>
        /// Creates the meta object for the rest of the operations: comparisons and all other
        /// ExpressionType.  If the operation cannot be completed a MetaObject which indicates an
        /// error will be returned.
        /// </summary>
        private DynamicMetaObject MakeGeneralOperatorRule(string operation, Expression codeContext, DynamicMetaObject[] args) {
            OperatorInfo info = OperatorInfo.GetOperatorInfo(operation);
            DynamicMetaObject res;

            if (CompilerHelpers.IsComparisonOperator(info.Operator)) {
                res = MakeComparisonRule(info, codeContext, args);
            } else {
                res = MakeOperatorRule(info, codeContext, args);
            }

            return res;
        }

        #region Comparison operator

        private DynamicMetaObject MakeComparisonRule(OperatorInfo info, Expression codeContext, DynamicMetaObject[] args) {
            return
                TryComparisonMethod(info, codeContext, args[0], args) ??   // check the first type if it has an applicable method
                TryComparisonMethod(info, codeContext, args[0], args) ??   // then check the second type
                TryNumericComparison(info, args) ??           // try Compare: cmp(x,y) (>, <, >=, <=, ==, !=) 0
                TryInvertedComparison(info, args[0], args) ?? // try inverting the operator & result (e.g. if looking for Equals try NotEquals, LessThan for GreaterThan)...
                TryInvertedComparison(info, args[0], args) ?? // inverted binding on the 2nd type
                TryNullComparisonRule(args) ??                // see if we're comparing to null w/ an object ref or a Nullable<T>
                TryPrimitiveCompare(info, args) ??            // see if this is a primitive type where we're comparing the two values.
                MakeOperatorError(info, args);                // no comparisons are possible            
        }

        private DynamicMetaObject TryComparisonMethod(OperatorInfo info, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args) {
            MethodInfo[] targets = GetApplicableMembers(target.GetLimitType(), info);
            if (targets.Length > 0) {
                return TryMakeBindingTarget(targets, args, codeContext, BindingRestrictions.Empty);
            }

            return null;
        }

        private static DynamicMetaObject MakeOperatorError(OperatorInfo info, DynamicMetaObject[] args) {
            return new DynamicMetaObject(
                Ast.Throw(
                    AstUtils.ComplexCallHelper(
                        typeof(BinderOps).GetMethod("BadArgumentsForOperation"),
                        ArrayUtils.Insert((Expression)Ast.Constant(info.Operator), DynamicUtils.GetExpressions(args))
                    )
                ),
                BindingRestrictions.Combine(args)
            );
        }

        private DynamicMetaObject TryNumericComparison(OperatorInfo info, DynamicMetaObject[] args) {
            MethodInfo[] targets = FilterNonMethods(
                args[0].GetLimitType(),
                GetMember(OldDoOperationAction.Make(this, OperatorInfo.ExpressionTypeToOperator(info.Operator)),
                args[0].GetLimitType(),
                "Compare")
            );

            if (targets.Length > 0) {
                MethodBinder mb = MethodBinder.MakeBinder(this, targets[0].Name, targets);
                BindingTarget target = mb.MakeBindingTarget(CallTypes.None, args);
                if (target.Success) {
                    Expression call = AstUtils.Convert(target.MakeExpression(), typeof(int));
                    switch (info.Operator) {
                        case ExpressionType.GreaterThan: call = Ast.GreaterThan(call, Ast.Constant(0)); break;
                        case ExpressionType.LessThan: call = Ast.LessThan(call, Ast.Constant(0)); break;
                        case ExpressionType.GreaterThanOrEqual: call = Ast.GreaterThanOrEqual(call, Ast.Constant(0)); break;
                        case ExpressionType.LessThanOrEqual: call = Ast.LessThanOrEqual(call, Ast.Constant(0)); break;
                        case ExpressionType.Equal: call = Ast.Equal(call, Ast.Constant(0)); break;
                        case ExpressionType.NotEqual: call = Ast.NotEqual(call, Ast.Constant(0)); break;
                    }

                    return new DynamicMetaObject(
                        call,
                        BindingRestrictions.Combine(target.RestrictedArguments)
                    );
                }
            }

            return null;
        }

        private DynamicMetaObject TryInvertedComparison(OperatorInfo info, DynamicMetaObject target, DynamicMetaObject[] args) {
            ExpressionType revOp = GetInvertedOperator(info.Operator);
            OperatorInfo revInfo = OperatorInfo.GetOperatorInfo(revOp);
            Debug.Assert(revInfo != null);

            // try the 1st type's opposite function result negated 
            MethodBase[] targets = GetApplicableMembers(target.GetLimitType(), revInfo);
            if (targets.Length > 0) {
                return TryMakeInvertedBindingTarget(targets, args);
            }

            return null;
        }

        /// <summary>
        /// Produces a rule for comparing a value to null - supports comparing object references and nullable types.
        /// </summary>
        private static DynamicMetaObject TryNullComparisonRule(DynamicMetaObject[] args) {
            Type otherType = args[1].GetLimitType();

            BindingRestrictions restrictions = BindingRestrictionsHelpers.GetRuntimeTypeRestriction(args[0].Expression, args[0].GetLimitType()).Merge(BindingRestrictions.Combine(args));

            if (args[0].GetLimitType() == typeof(DynamicNull)) {
                if (!otherType.IsValueType) {
                    return new DynamicMetaObject(
                        Ast.Equal(args[0].Expression, Ast.Constant(null)),
                        restrictions
                    );
                } else if (otherType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return new DynamicMetaObject(
                            Ast.Property(args[0].Expression, otherType.GetProperty("HasValue")),
                        restrictions
                    );
                }
            } else if (otherType == typeof(DynamicNull)) {
                if (!args[0].GetLimitType().IsValueType) {
                    return new DynamicMetaObject(
                        Ast.Equal(args[0].Expression, Ast.Constant(null)),
                        restrictions
                    );
                } else if (args[0].GetLimitType().GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return new DynamicMetaObject(
                        Ast.Property(args[0].Expression, otherType.GetProperty("HasValue")),
                        restrictions
                    );
                }
            }
            return null;
        }

        private static DynamicMetaObject TryPrimitiveCompare(OperatorInfo info, DynamicMetaObject[] args) {
            if (TypeUtils.GetNonNullableType(args[0].GetLimitType()) == TypeUtils.GetNonNullableType(args[1].GetLimitType()) &&
                TypeUtils.IsNumeric(args[0].GetLimitType())) {
                Expression arg0 = args[0].Expression;
                Expression arg1 = args[1].Expression;

                // TODO: Nullable<PrimitveType> Support
                Expression expr;
                switch (info.Operator) {
                    case ExpressionType.Equal: expr = Ast.Equal(arg0, arg1); break;
                    case ExpressionType.NotEqual: expr = Ast.NotEqual(arg0, arg1); break;
                    case ExpressionType.GreaterThan: expr = Ast.GreaterThan(arg0, arg1); break;
                    case ExpressionType.LessThan: expr = Ast.LessThan(arg0, arg1); break;
                    case ExpressionType.GreaterThanOrEqual: expr = Ast.GreaterThanOrEqual(arg0, arg1); break;
                    case ExpressionType.LessThanOrEqual: expr = Ast.LessThanOrEqual(arg0, arg1); break;
                    default: throw new InvalidOperationException();
                }

                return new DynamicMetaObject(
                    expr,
                    BindingRestrictionsHelpers.GetRuntimeTypeRestriction(arg0, args[0].GetLimitType()).Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(arg1, args[0].GetLimitType())).Merge(BindingRestrictions.Combine(args))
                );
            }

            return null;
        }

        #endregion

        #region Operator Rule

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")] // TODO: fix
        private DynamicMetaObject MakeOperatorRule(OperatorInfo info, Expression codeContext, DynamicMetaObject[] args) {
            return
                TryForwardOperator(info, codeContext, args) ??
                TryReverseOperator(info, codeContext, args) ??
                TryPrimitiveOperator(info, args) ??
                TryMakeDefaultUnaryRule(info, codeContext, args) ??
                MakeOperatorError(info, args);
        }

        private static DynamicMetaObject TryPrimitiveOperator(OperatorInfo info, DynamicMetaObject[] args) {
            if (args.Length == 2 &&
                TypeUtils.GetNonNullableType(args[0].GetLimitType()) == TypeUtils.GetNonNullableType(args[1].GetLimitType()) &&
                TypeUtils.IsArithmetic(args[0].GetLimitType())) {
                // TODO: Nullable<PrimitveType> Support
                Expression expr;
                DynamicMetaObject self = args[0].Restrict(args[0].GetLimitType());
                DynamicMetaObject arg0 = args[1].Restrict(args[0].GetLimitType());

                switch (info.Operator) {
                    case ExpressionType.Add: expr = Ast.Add(self.Expression, arg0.Expression); break;
                    case ExpressionType.Subtract: expr = Ast.Subtract(self.Expression, arg0.Expression); break;
                    case ExpressionType.Divide: expr = Ast.Divide(self.Expression, arg0.Expression); break;
                    case ExpressionType.Modulo: expr = Ast.Modulo(self.Expression, arg0.Expression); break;
                    case ExpressionType.Multiply: expr = Ast.Multiply(self.Expression, arg0.Expression); break;
                    case ExpressionType.LeftShift: expr = Ast.LeftShift(self.Expression, arg0.Expression); break;
                    case ExpressionType.RightShift: expr = Ast.RightShift(self.Expression, arg0.Expression); break;
                    case ExpressionType.And: expr = Ast.And(self.Expression, arg0.Expression); break;
                    case ExpressionType.Or: expr = Ast.Or(self.Expression, arg0.Expression); break;
                    case ExpressionType.ExclusiveOr: expr = Ast.ExclusiveOr(self.Expression, arg0.Expression); break;
                    default: throw new InvalidOperationException();
                }

                return new DynamicMetaObject(
                    expr,
                    self.Restrictions.Merge(arg0.Restrictions)
                );
            }

            return null;
        }

        private DynamicMetaObject TryForwardOperator(OperatorInfo info, Expression codeContext, DynamicMetaObject[] args) {
            MethodInfo[] targets = GetApplicableMembers(args[0].GetLimitType(), info);
            BindingRestrictions restrictions = BindingRestrictions.Empty;

            if (targets.Length > 0) {
                return TryMakeBindingTarget(targets, args, codeContext, restrictions);
            }

            return null;
        }

        private DynamicMetaObject TryReverseOperator(OperatorInfo info, Expression codeContext, DynamicMetaObject[] args) {
            // we need a special conversion for the return type on MemberNames
            if (args.Length > 0) {
                MethodInfo[] targets = GetApplicableMembers(args[0].LimitType, info);
                if (targets.Length > 0) {
                    return TryMakeBindingTarget(targets, args, codeContext, BindingRestrictions.Empty);
                }
            }

            return null;
        }

        private static DynamicMetaObject TryMakeDefaultUnaryRule(OperatorInfo info, Expression codeContext, DynamicMetaObject[] args) {
            if (args.Length == 1) {
                BindingRestrictions restrictions = BindingRestrictionsHelpers.GetRuntimeTypeRestriction(args[0].Expression, args[0].GetLimitType()).Merge(BindingRestrictions.Combine(args));
                switch (info.Operator) {
                    case ExpressionType.IsTrue:
                        if (args[0].GetLimitType() == typeof(bool)) {
                            return args[0];
                        }
                        break;
                    case ExpressionType.Negate:
                        if (TypeUtils.IsArithmetic(args[0].GetLimitType())) {
                            return new DynamicMetaObject(
                                Ast.Negate(args[0].Expression),
                                restrictions
                            );
                        }
                        break;
                    case ExpressionType.Not:
                        if (TypeUtils.IsIntegerOrBool(args[0].GetLimitType())) {
                            return new DynamicMetaObject(
                                Ast.Not(args[0].Expression),
                                restrictions
                            );
                        }
                        break;
                }
            }
            return null;
        }

        private static DynamicMetaObject MakeIMembersListRule(Expression codeContext, DynamicMetaObject target) {
            return new DynamicMetaObject(
                Ast.Call(
                    typeof(BinderOps).GetMethod("GetStringMembers"),
                    Ast.Call(
                        AstUtils.Convert(target.Expression, typeof(IMembersList)),
                        typeof(IMembersList).GetMethod("GetMemberNames"),
                        codeContext
                    )
                ),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target.Expression, target.GetLimitType()).Merge(target.Restrictions)
            );
        }

        private static DynamicMetaObject MakeCallSignatureResult(MethodBase[] methods, DynamicMetaObject target) {
            List<string> arrres = new List<string>();

            if (methods != null) {
                foreach (MethodBase mb in methods) {
                    StringBuilder res = new StringBuilder();
                    string comma = "";
                    foreach (ParameterInfo param in mb.GetParameters()) {
                        if (param.ParameterType == typeof(CodeContext)) continue;

                        res.Append(comma);
                        res.Append(param.ParameterType.Name);
                        res.Append(" ");
                        res.Append(param.Name);
                        comma = ", ";
                    }
                    arrres.Add(res.ToString());
                }
            }

            return new DynamicMetaObject(
                Ast.Constant(arrres.ToArray()),
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target.Expression, target.GetLimitType()).Merge(target.Restrictions)
            );
        }

        #endregion

        #region Indexer Rule

        private static Type GetArgType(DynamicMetaObject[] args, int index) {
            return args[index].HasValue ? args[index].GetLimitType() : args[index].Expression.Type;
        }

        private DynamicMetaObject MakeMethodIndexRule(IndexType oper, DynamicMetaObject[] args) {
            MethodInfo[] defaults = GetMethodsFromDefaults(args[0].GetLimitType().GetDefaultMembers(), oper);
            if (defaults.Length != 0) {
                MethodBinder binder = MethodBinder.MakeBinder(
                    this,
                    oper == IndexType.Get ? "get_Item" : "set_Item",
                    defaults);

                DynamicMetaObject[] selfWithArgs = args;
                ParameterExpression arg2 = null;

                if (oper == IndexType.Set) {
                    Debug.Assert(args.Length >= 2);

                    // need to save arg2 in a temp because it's also our result
                    arg2 = Ast.Variable(args[2].Expression.Type, "arg2Temp");

                    args[2] = new DynamicMetaObject(
                        Ast.Assign(arg2, args[2].Expression),
                        args[2].Restrictions
                    );
                }

                BindingTarget target = binder.MakeBindingTarget(CallTypes.ImplicitInstance, selfWithArgs);

                BindingRestrictions restrictions = BindingRestrictions.Combine(args);

                if (target.Success) {
                    if (oper == IndexType.Get) {
                        return new DynamicMetaObject(
                            target.MakeExpression(),
                            restrictions.Merge(BindingRestrictions.Combine(target.RestrictedArguments))
                        );
                    } else {
                        return new DynamicMetaObject(
                            Ast.Block(
                                new ParameterExpression[] { arg2 },
                                target.MakeExpression(),
                                arg2
                            ),
                            restrictions.Merge(BindingRestrictions.Combine(target.RestrictedArguments))
                        );
                    }
                }

                return MakeError(
                    MakeInvalidParametersError(target),
                    restrictions
                );
            }

            return null;
        }

        private DynamicMetaObject MakeArrayIndexRule(IndexType oper, DynamicMetaObject[] args) {
            if (CanConvertFrom(GetArgType(args, 1), typeof(int), false, NarrowingLevel.All)) {
                BindingRestrictions restrictions = BindingRestrictionsHelpers.GetRuntimeTypeRestriction(args[0].Expression, args[0].GetLimitType()).Merge(BindingRestrictions.Combine(args));

                if (oper == IndexType.Get) {
                    return new DynamicMetaObject(
                        Ast.ArrayAccess(
                            args[0].Expression,
                            ConvertIfNeeded(args[1].Expression, typeof(int))
                        ),
                        restrictions
                    );
                } else {
                    return new DynamicMetaObject(
                        Ast.Assign(
                            Ast.ArrayAccess(
                                args[0].Expression,
                                ConvertIfNeeded(args[1].Expression, typeof(int))
                            ),
                            ConvertIfNeeded(args[2].Expression, args[0].GetLimitType().GetElementType())
                        ),
                        restrictions.Merge(args[1].Restrictions)
                    );
                }
            }

            return null;
        }

        private MethodInfo[] GetMethodsFromDefaults(MemberInfo[] defaults, IndexType op) {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (MemberInfo mi in defaults) {
                PropertyInfo pi = mi as PropertyInfo;

                if (pi != null) {
                    if (op == IndexType.Get) {
                        MethodInfo method = pi.GetGetMethod(PrivateBinding); 
                        if (method != null) methods.Add(method);
                    } else if (op == IndexType.Set) {
                        MethodInfo method = pi.GetSetMethod(PrivateBinding);
                        if (method != null) methods.Add(method);
                    }
                }
            }

            // if we received methods from both declaring type & base types we need to filter them
            Dictionary<MethodSignatureInfo, MethodInfo> dict = new Dictionary<MethodSignatureInfo, MethodInfo>();
            foreach (MethodInfo mb in methods) {
                MethodSignatureInfo args = new MethodSignatureInfo(mb.IsStatic, mb.GetParameters());
                MethodInfo other;

                if (dict.TryGetValue(args, out other)) {
                    if (other.DeclaringType.IsAssignableFrom(mb.DeclaringType)) {
                        // derived type replaces...
                        dict[args] = mb;
                    }
                } else {
                    dict[args] = mb;
                }
            }

            return new List<MethodInfo>(dict.Values).ToArray();
        }

        #endregion

        #region Common helpers

        private DynamicMetaObject TryMakeBindingTarget(MethodInfo[] targets, DynamicMetaObject[] args, Expression codeContext, BindingRestrictions restrictions) {
            MethodBinder mb = MethodBinder.MakeBinder(this, targets[0].Name, targets);

            BindingTarget target = mb.MakeBindingTarget(CallTypes.None, args);
            if (target.Success) {
                return new DynamicMetaObject(
                    target.MakeExpression(new ParameterBinderWithCodeContext(this, codeContext)),
                    restrictions.Merge(BindingRestrictions.Combine(target.RestrictedArguments))
                );
            }

            return null;
        }

        private DynamicMetaObject TryMakeInvertedBindingTarget(MethodBase[] targets, DynamicMetaObject[] args) {
            MethodBinder mb = MethodBinder.MakeBinder(this, targets[0].Name, targets);
            DynamicMetaObject[] selfArgs = args;
            BindingTarget target = mb.MakeBindingTarget(CallTypes.None, selfArgs);

            if (target.Success) {
                return new DynamicMetaObject(
                    Ast.Not(target.MakeExpression()),
                    BindingRestrictions.Combine(target.RestrictedArguments)
                );
            }

            return null;
        }

        private static ExpressionType GetInvertedOperator(ExpressionType op) {
            switch (op) {
                case ExpressionType.LessThan: return ExpressionType.GreaterThanOrEqual;
                case ExpressionType.LessThanOrEqual: return ExpressionType.GreaterThan;
                case ExpressionType.GreaterThan: return ExpressionType.LessThanOrEqual;
                case ExpressionType.GreaterThanOrEqual: return ExpressionType.LessThan;
                case ExpressionType.Equal: return ExpressionType.NotEqual;
                case ExpressionType.NotEqual: return ExpressionType.Equal;
                default: throw new InvalidOperationException();
            }
        }

        private Expression ConvertIfNeeded(Expression expression, Type type) {
            Assert.NotNull(expression, type);

            if (expression.Type != type) {
                return ConvertExpression(expression, type, ConversionResultKind.ExplicitCast, Ast.Constant(null, typeof(CodeContext)));
            }
            return expression;
        }

        private MethodInfo[] GetApplicableMembers(Type t, OperatorInfo info) {
            Assert.NotNull(t, info);

            OldDoOperationAction act = OldDoOperationAction.Make(this, OperatorInfo.ExpressionTypeToOperator(info.Operator));

            MemberGroup members = GetMember(act, t, info.Name);
            if (members.Count == 0 && info.AlternateName != null) {
                members = GetMember(act, t, info.AlternateName);
            }

            // filter down to just methods
            return FilterNonMethods(t, members);
        }
        
        private static BindingRestrictions GetFallbackRestrictions(Type t, EventTracker et, DynamicMetaObject self) {
            if (t == typeof(EventTracker)) {
                //
                // Test Generated:
                //   BinderOps.GetEventHandlerType(((EventTracker)args[0]).Event) == et.Event.EventHandlerType
                //
                return BindingRestrictions.GetExpressionRestriction(
                    Ast.Equal(
                        Ast.Call(
                            typeof(BinderOps).GetMethod("GetEventHandlerType"),
                            Ast.Property(
                                Ast.Convert(
                                    self.Expression,
                                    typeof(EventTracker)
                                ),
                                typeof(EventTracker).GetProperty("Event")
                            )
                        ),
                        Ast.Constant(et.Event.EventHandlerType)
                    )
                );
            } else if (t == typeof(BoundMemberTracker)) {
                //
                // Test Generated:
                //   BinderOps.GetEventHandlerType(((EventTracker)((BoundMemberTracker)args[0]).BountTo).Event) == et.Event.EventHandlerType
                //
                return BindingRestrictions.GetExpressionRestriction(
                    Ast.Equal(
                        Ast.Call(
                            typeof(BinderOps).GetMethod("GetEventHandlerType"),
                            Ast.Property(
                                Ast.Convert(
                                    Ast.Property(
                                        Ast.Convert(
                                            self.Expression,
                                            typeof(BoundMemberTracker)
                                        ),
                                        typeof(BoundMemberTracker).GetProperty("BoundTo")
                                    ),
                                    typeof(EventTracker)
                                ),
                                typeof(EventTracker).GetProperty("Event")
                            )
                        ),
                        Ast.Constant(et.Event.EventHandlerType)
                    )
                );
            }

            return BindingRestrictions.Empty;
        }

        private static MethodInfo[] FilterNonMethods(Type t, MemberGroup members) {
            Assert.NotNull(t, members);

            List<MethodInfo> methods = new List<MethodInfo>(members.Count);
            foreach (MemberTracker mi in members) {
                if (mi.MemberType == TrackerTypes.Method) {
                    MethodInfo method = ((MethodTracker)mi).Method;

                    // don't call object methods for DynamicNull type, but if someone added
                    // methods to null we'd call those.
                    if (method.DeclaringType != typeof(object) || t != typeof(DynamicNull)) {
                        methods.Add(method);
                    }
                }
            }

            return methods.ToArray();
        }

        #endregion
    }
}