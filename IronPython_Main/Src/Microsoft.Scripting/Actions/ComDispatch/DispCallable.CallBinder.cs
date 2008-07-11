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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Actions.ComDispatch {

    public partial class DispCallable {

        /// <summary>
        /// Creates a rule for calling a COM method using IDispatch::Invoke
        /// </summary>
        /// <typeparam name="T">Type of the DynamicSite</typeparam>
        private sealed class CallBinder<T> : ComCallBinderHelper<T, OldCallAction> where T : class {

            private VarEnumSelector _varEnumSelector;
            private MethodBinderContext _methodBinderContext;
            private SymbolId[] _keywordArgNames;
            private int _totalExplicitArgs; // Includes the individial elements of ArgumentKind.Dictionary (if any)

            private VariableExpression _dispatchObject;
            private VariableExpression _dispatchPointer;
            private VariableExpression _dispId;
            private VariableExpression _dispParams;
            private VariableExpression _paramVariants;
            private VariableExpression _invokeResult;
            private VariableExpression _returnValue;
            private VariableExpression _dispIdsOfKeywordArgsPinned;

            internal CallBinder(CodeContext context, OldCallAction action, object[] args)
                : base(context, action, args) {

                ContractUtils.RequiresNotNull(args, "args");
                if (args.Length < 1) {
                    throw new ArgumentException("Must receive at least one argument, the target to call", "args");
                }

                // Set Instance to some value so that CallBinderHelper has the right number of parameters to work with
                Instance = GetIDispatchObject();

                _methodBinderContext = new MethodBinderContext(Binder, Rule);
            }

            private Expression ThisParameter {
                get { return Rule.Parameters[0]; }
            }

            private Expression GetDispCallable() {
                return Expression.ConvertHelper(ThisParameter, typeof(DispCallable));
            }

            private Expression GetIDispatchObject() {
                Expression dispCallable = GetDispCallable();
                return Expression.Property(dispCallable, typeof(DispCallable).GetProperty("DispatchObject"));
            }

            private Expression GetDispId() {
                Expression dispCallable = Expression.ConvertHelper(ThisParameter, typeof(DispCallable));
                Expression comMethodDesc = Expression.Property(dispCallable, typeof(DispCallable).GetProperty("ComMethodDesc"));
                return Expression.Property(comMethodDesc, typeof(ComMethodDesc).GetProperty("DispId"));
            }

            private Expression GenerateTryBlock() {
                //
                // Declare variables
                //
                VariableExpression excepInfo = Rule.GetTemporary(typeof(ExcepInfo), "excepInfo");
                VariableExpression argErr = Rule.GetTemporary(typeof(uint), "argErr");
                VariableExpression hresult = Rule.GetTemporary(typeof(int), "hresult");

                List<Expression> tryStatements = new List<Expression>();
                Expression expr;

                if (_keywordArgNames.Length > 0) {

                    // _dispParams.rgdispidNamedArgs = ComRuntimeHelpers.UnsafeMethods.GetIdsOfNamedParameters(out _dispIdsOfKeywordArgsPinned)

                    _dispIdsOfKeywordArgsPinned = Rule.GetTemporary(typeof(GCHandle), "dispIdsOfKeywordArgsPinned");

                    string[] names = SymbolTable.IdsToStrings(_keywordArgNames);
                    names = ArrayUtils.Insert(((DispCallable)Callable).ComMethodDesc.Name, names);

                    expr = Expression.AssignField(
                        _dispParams,
                        typeof(ComTypes.DISPPARAMS).GetField("rgdispidNamedArgs"),
                        Expression.Call(typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("GetIdsOfNamedParameters"),
                            _dispatchObject,
                            Expression.Constant(names),
                            _dispId,
                            _dispIdsOfKeywordArgsPinned));
                    tryStatements.Add(expr);
                }

                //
                // Marshal the arguments to Variants
                //
                // For a call like this:
                //   comObj.Foo(100, 101, 102, x=123, z=125)
                // DISPPARAMS needs to be setup like this:
                //   cArgs:             5
                //   cNamedArgs:        2
                //   rgArgs:            123, 125, 102, 101, 100
                //   rgdispidNamedArgs: dx, dz (the dispids of x and z respectively)

                Expression[] parameters = MakeArgumentExpressions();
                int reverseIndex = _varEnumSelector.VariantBuilders.Length - 1;
                int positionalArgs = _varEnumSelector.VariantBuilders.Length - _keywordArgNames.Length; // args passed by position order and not by name
                for (int i = 0; i < _varEnumSelector.VariantBuilders.Length; i++, reverseIndex--) {
                    int variantIndex;
                    if (i >= positionalArgs) {
                        // Named arguments are in order at the start of rgArgs
                        variantIndex = i - positionalArgs;
                    } else {
                        // Positial arguments are in reverse order at the tail of rgArgs
                        variantIndex = reverseIndex;
                    }
                    VariantBuilder variantBuilder = _varEnumSelector.VariantBuilders[i];
                    List<Expression> marshalStatements = variantBuilder.WriteArgumentVariant(
                        _methodBinderContext,
                        _paramVariants,
                        variantIndex,
                        parameters);
                    tryStatements.AddRange(marshalStatements);
                }

                //
                // Call Invoke
                //
                MethodCallExpression invoke = Expression.Call(
                    typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("IDispatchInvoke"),
                    _dispatchPointer,
                    _dispId,
                    Expression.Constant(
                        Callable is DispPropertyPut ?
                            ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT :
                            ComTypes.INVOKEKIND.INVOKE_FUNC | ComTypes.INVOKEKIND.INVOKE_PROPERTYGET
                                 ), // INVOKE_PROPERTYGET should only be needed for COM objects without typeinfo, where we might have to treat properties as methods
                    _dispParams,
                    _invokeResult,
                    excepInfo,
                    argErr);
                expr = Expression.Assign(hresult, invoke);
                tryStatements.Add(expr);

                //
                // ComRuntimeHelpers.CheckThrowException(hresult, excepInfo, argErr, ThisParameter);
                //
                expr = Expression.Call(
                    typeof(ComRuntimeHelpers).GetMethod("CheckThrowException"),
                    hresult,
                    excepInfo,
                    argErr,
                    GetDispCallable()
                );
                tryStatements.Add(expr);

                //
                // _returnValue = (ReturnType)_invokeResult.ToObject();
                //
                Expression invokeResultObject = Binder.ConvertExpression(
                    Expression.Call(
                        _invokeResult,
                        typeof(Variant).GetMethod("ToObject")),
                    Rule.ReturnType,
                    ConversionResultKind.ExplicitCast,
                    Rule.Context);

                ArgBuilder[] argBuilders = _varEnumSelector.GetArgBuilders();

                Expression[] parametersForUpdates = MakeArgumentExpressions();
                Expression returnValues = _varEnumSelector.ReturnBuilder.ToExpression(invokeResultObject);
                expr = Expression.Assign(_returnValue, returnValues);
                tryStatements.Add(expr);

                foreach (ArgBuilder argBuilder in argBuilders) {
                    Expression updateFromReturn = argBuilder.UpdateFromReturn(parametersForUpdates);
                    if (updateFromReturn != null) {
                        tryStatements.Add(updateFromReturn);
                    }
                }

                return Expression.Block(tryStatements);
            }

            private Expression GenerateFinallyBlock() {
                List<Expression> finallyStatements = new List<Expression>();
                Expression expr;

                //
                // _dispatchObject.ReleaseDispatchPointer(_dispatchPointer);
                //
                expr = Expression.Call(
                    _dispatchObject,
                    typeof(IDispatchObject).GetMethod("ReleaseDispatchPointer"),
                    _dispatchPointer
                );
                finallyStatements.Add(expr);

                //
                // Clear memory allocated for marshalling
                //
                foreach (VariantBuilder variantBuilder in _varEnumSelector.VariantBuilders) {
                    List<Expression> clearingStatements = variantBuilder.Clear(_paramVariants);
                    finallyStatements.AddRange(clearingStatements);
                }

                //
                // _invokeResult.Clear()
                //

                expr = Expression.Call(
                    _invokeResult,
                    typeof(Variant).GetMethod("Clear")
                );
                finallyStatements.Add(expr);

                //
                // _dispIdsOfKeywordArgsPinned.Free()
                //
                if (_dispIdsOfKeywordArgsPinned != null) {
                    expr = Expression.Call(
                        _dispIdsOfKeywordArgsPinned,
                        typeof(GCHandle).GetMethod("Free")
                    );
                    finallyStatements.Add(expr);
                }

                return Expression.Block(finallyStatements);
            }

            /// <summary>
            /// Create a stub for the target of the optimized lopop.
            /// </summary>
            /// <returns></returns>
            private Expression MakeIDispatchInvokeTarget() {
                Debug.Assert(_varEnumSelector.VariantBuilders.Length == _totalExplicitArgs);

                List<Expression> exprs = new List<Expression>();
                Expression expr;

                //
                // Create variables, and initialize them to trivial values
                //
                _dispatchObject = Rule.GetTemporary(typeof(IDispatchObject), "dispatchObject");
                _dispatchPointer = Rule.GetTemporary(typeof(IntPtr), "dispatchPointer");
                _dispId = Rule.GetTemporary(typeof(int), "dispId");
                _dispParams = Rule.GetTemporary(typeof(ComTypes.DISPPARAMS), "dispParams");
                _paramVariants = Rule.GetTemporary(typeof(VariantArray), "paramVariants");
                _invokeResult = Rule.GetTemporary(typeof(Variant), "invokeResult");
                _returnValue = Rule.GetTemporary(Rule.ReturnType, "returnValue");
                _dispIdsOfKeywordArgsPinned = null;

                //
                // _dispId = ((DispCallable)this).ComMethodDesc.DispId;
                //
                expr = Expression.Assign(
                    _dispId,
                    Expression.Property(
                        Expression.Property(
                            Expression.ConvertHelper(ThisParameter, typeof(DispCallable)),
                            typeof(DispCallable).GetProperty("ComMethodDesc")),
                        typeof(ComMethodDesc).GetProperty("DispId")));
                exprs.Add(expr);

                //
                // _dispParams.rgvararg = RuntimeHelpers.UnsafeMethods.ConvertVariantByrefToPtr(ref _paramVariants._element0)
                //
                if (_totalExplicitArgs != 0) {
                    MethodCallExpression addrOfParamVariants = Expression.Call(
                        typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("ConvertVariantByrefToPtr"),
                        Expression.Field(
                            _paramVariants,
                            VariantArray.GetField(0)));
                    expr = Expression.AssignField(
                        _dispParams,
                        typeof(ComTypes.DISPPARAMS).GetField("rgvarg"),
                        addrOfParamVariants);

                    exprs.Add(expr);
                }

                //
                // _dispParams.cArgs = <number_of_params>;
                //
                expr = Expression.AssignField(
                    _dispParams,
                    typeof(ComTypes.DISPPARAMS).GetField("cArgs"),
                    Expression.Constant(_totalExplicitArgs));
                exprs.Add(expr);

                if (Callable is DispPropertyPut) {
                    //
                    // dispParams.cNamedArgs = 1;
                    // dispParams.rgdispidNamedArgs = RuntimeHelpers.UnsafeMethods.GetNamedArgsForPropertyPut()
                    //
                    expr = Expression.AssignField(
                        _dispParams,
                        typeof(ComTypes.DISPPARAMS).GetField("cNamedArgs"),
                        Expression.Constant(1));
                    exprs.Add(expr);

                    VariableExpression _propertyPutDispId = Rule.GetTemporary(typeof(int), "propertyPutDispId");

                    expr = Expression.Assign(
                        _propertyPutDispId,
                        Expression.Constant(ComDispIds.DISPID_PROPERTYPUT));

                    MethodCallExpression rgdispidNamedArgs = Expression.Call(
                        typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("ConvertInt32ByrefToPtr"),
                        _propertyPutDispId);

                    expr = Expression.AssignField(
                        _dispParams,
                        typeof(ComTypes.DISPPARAMS).GetField("rgdispidNamedArgs"),
                        rgdispidNamedArgs);
                    exprs.Add(expr);
                } else {
                    //
                    // _dispParams.cNamedArgs = N;
                    //
                    expr = Expression.AssignField(
                        _dispParams,
                        typeof(ComTypes.DISPPARAMS).GetField("cNamedArgs"),
                        Expression.Constant(_keywordArgNames.Length));
                    exprs.Add(expr);
                }

                //
                // _dispatchObject = ((DispCallable)this).DispatchObject
                // _dispatchPointer = dispatchObject.GetDispatchPointerInCurrentApartment();
                //

                expr = Expression.Assign(
                    _dispatchObject,
                    GetIDispatchObject());
                exprs.Add(expr);

                expr = Expression.Assign(
                    _dispatchPointer,
                    Expression.Call(
                        _dispatchObject,
                        typeof(IDispatchObject).GetMethod("GetDispatchPointerInCurrentApartment")));
                exprs.Add(expr);

                Expression tryStatements = GenerateTryBlock();

                Expression finallyStatements = GenerateFinallyBlock();

                expr = Expression.TryFinally(tryStatements, finallyStatements);
                exprs.Add(expr);

                expr = Expression.Return(
                    _returnValue);
                exprs.Add(expr);

                return Expression.Block(exprs);
            }

            # region Unoptimized or error cases

            private Expression MakeUnoptimizedInvokeTarget() {
                Expression dispCallable = GetDispCallable();
                Expression[] arguments = MakeArgumentExpressions();
                arguments = ArrayUtils.RemoveFirst(arguments); // Remove the instance argument

                arguments = ArrayUtils.Insert<Expression>(Rule.Context, Expression.Constant(_keywordArgNames), arguments);
                MethodInfo unoptimizedInvoke = typeof(DispCallable).GetMethod("UnoptimizedInvoke");
                Expression target = Expression.ComplexCallHelper(dispCallable, unoptimizedInvoke, arguments);
                return Expression.Return(target);
            }

            private RuleBuilder<T> MakeUnoptimizedInvokeRule() {
                Rule.Target = MakeUnoptimizedInvokeTarget();
                return Rule;
            }

            #endregion

            internal RuleBuilder<T> MakeRule() {
                DispCallable dispCallable = (DispCallable)Callable;

                Type[] explicitArgTypes; // will not include implicit instance argument (if any)            
                GetArgumentNamesAndTypes(out _keywordArgNames, out explicitArgTypes);
                _totalExplicitArgs = explicitArgTypes.Length;

                bool hasAmbiguousMatch = false;
                try {
                    _varEnumSelector = new VarEnumSelector(Binder, Rule.ReturnType, explicitArgTypes);
                } catch (AmbiguousMatchException) {
                    hasAmbiguousMatch = true;
                }

                Type[] testTypes = ArrayUtils.Insert(typeof(DispCallable), explicitArgTypes); // Add a dummy instance argument - it will not really be used
                FinishTestForCandidate(testTypes, explicitArgTypes);

                if ((explicitArgTypes.Length > VariantArray.NumberOfElements) ||
                    (hasAmbiguousMatch) ||
                    (!_varEnumSelector.IsSupportedByFastPath) ||
                    (Context.LanguageContext.Options.InterpretedMode)) { // The rule we generate cannot be interpreted

                    return MakeUnoptimizedInvokeRule();
                }

                Expression target = MakeIDispatchInvokeTarget();
                Rule.Target = target;

                return Rule;
            }

            // This can produce a IsCallable rule that returns the immutable constant isCallable.
            // Beware that objects can have a mutable callable property. Eg, in Python, assign or delete the __call__ attribute.
            internal static RuleBuilder<T> MakeIsCallableRule(CodeContext context, object self, bool isCallable) {
                RuleBuilder<T> rule = new RuleBuilder<T>();
                rule.MakeTest(CompilerHelpers.GetType(self));
                rule.Target =
                    rule.MakeReturn(
                        context.LanguageContext.Binder,
                        Expression.Constant(isCallable)
                    );

                return rule;
            }
        }
    }
}

#endif
