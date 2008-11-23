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


#if !SILVERLIGHT

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Com {
    internal sealed class InvokeBinder {
        private readonly ComMethodDesc _methodDesc;
        private readonly Expression _method;        // ComMethodDesc to be called
        private readonly Expression _dispatch;      // IDispatchObject

        private readonly IList<Argument> _arguments;
        private readonly MetaObject[] _args;
        private readonly Expression _instance;

        private Restrictions _restrictions;

        private VarEnumSelector _varEnumSelector;
        private string[] _keywordArgNames;
        private int _totalExplicitArgs; // Includes the individial elements of ArgumentKind.Dictionary (if any)

        private ParameterExpression _dispatchObject;
        private ParameterExpression _dispatchPointer;
        private ParameterExpression _dispId;
        private ParameterExpression _dispParams;
        private ParameterExpression _paramVariants;
        private ParameterExpression _invokeResult;
        private ParameterExpression _returnValue;
        private ParameterExpression _dispIdsOfKeywordArgsPinned;
        private ParameterExpression _propertyPutDispId;

        internal InvokeBinder(IList<Argument> arguments, MetaObject[] args, Restrictions restrictions, Expression method, Expression dispatch, ComMethodDesc methodDesc) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(dispatch, "dispatch");
            ContractUtils.Requires(TypeUtils.AreReferenceAssignable(typeof(ComMethodDesc), method.Type), "method");
            ContractUtils.Requires(TypeUtils.AreReferenceAssignable(typeof(IDispatchObject), dispatch.Type), "method");

            _method = method;
            _dispatch = dispatch;
            _methodDesc = methodDesc;

            _arguments = arguments;
            _args = args;
            _restrictions = restrictions;

            // Set Instance to some value so that CallBinderHelper has the right number of parameters to work with
            _instance = dispatch;
        }

        private ParameterExpression DispatchObjectVariable {
            get { return EnsureVariable(ref _dispatchObject, typeof(IDispatchObject), "dispatchObject"); }
        }

        private ParameterExpression DispatchPointerVariable {
            get { return EnsureVariable(ref _dispatchPointer, typeof(IntPtr), "dispatchPointer"); }
        }

        private ParameterExpression DispIdVariable {
            get { return EnsureVariable(ref _dispId, typeof(int), "dispId"); }
        }

        private ParameterExpression DispParamsVariable {
            get { return EnsureVariable(ref _dispParams, typeof(ComTypes.DISPPARAMS), "dispParams"); }
        }

        private ParameterExpression ParamVariantsVariable {
            get { return EnsureVariable(ref _paramVariants, typeof(VariantArray), "paramVariants"); }
        }

        private ParameterExpression InvokeResultVariable {
            get { return EnsureVariable(ref _invokeResult, typeof(Variant), "invokeResult"); }
        }

        private ParameterExpression ReturnValueVariable {
            get { return EnsureVariable(ref _returnValue, typeof(object), "returnValue"); }
        }

        private ParameterExpression DispIdsOfKeywordArgsPinnedVariable {
            get { return EnsureVariable(ref _dispIdsOfKeywordArgsPinned, typeof(GCHandle), "dispIdsOfKeywordArgsPinned"); }
        }

        private ParameterExpression PropertyPutDispIdVariable {
            get { return EnsureVariable(ref _propertyPutDispId, typeof(int), "propertyPutDispId"); }
        }

        private static ParameterExpression EnsureVariable(ref ParameterExpression var, Type type, string name) {
            if (var != null) {
                return var;
            }
            return var = Expression.Variable(type, name);
        }

        internal MetaObject Invoke() {
            _keywordArgNames = GetArgumentNames();
            // will not include implicit instance argument (if any)
            Type[] explicitArgTypes = _args.Map(a => a.LimitType);
            Type[] marshalArgTypes = _args.Map(a => a.IsByRef ? a.LimitType.MakeByRefType() : a.LimitType);

            Expression[] explicitArgExprs = _args.Map(a => a.Expression);
            _totalExplicitArgs = explicitArgTypes.Length;

            bool hasAmbiguousMatch = false;
            try {
                _varEnumSelector = new VarEnumSelector(typeof(object), marshalArgTypes);
            } catch (AmbiguousMatchException) {
                hasAmbiguousMatch = true;
            }

            // We already tested the instance, so no need to test it again
            for (int i = 0; i < explicitArgTypes.Length; i++) {
                _restrictions = _restrictions.Merge(Restrictions.TypeRestriction(explicitArgExprs[i], explicitArgTypes[i]));
            }

            if (explicitArgTypes.Length > VariantArray.NumberOfElements ||
                hasAmbiguousMatch ||
                !_varEnumSelector.IsSupportedByFastPath) {
                return new MetaObject(
                    CreateScope(
                        MakeUnoptimizedInvokeTarget()
                    ),
                    Restrictions.Combine(_args).Merge(_restrictions)
                );
            }

            return new MetaObject(
                CreateScope(
                    MakeIDispatchInvokeTarget()
                ),
                Restrictions.Combine(_args).Merge(_restrictions)
            );
        }

        private static void AddNotNull(List<ParameterExpression> list, ParameterExpression var) {
            if (var != null) list.Add(var);
        }

        private Expression CreateScope(Expression expression) {
            List<ParameterExpression> vars = new List<ParameterExpression>();
            AddNotNull(vars, _dispatchObject);
            AddNotNull(vars, _dispatchPointer);
            AddNotNull(vars, _dispId);
            AddNotNull(vars, _dispParams);
            AddNotNull(vars, _paramVariants);
            AddNotNull(vars, _invokeResult);
            AddNotNull(vars, _returnValue);
            AddNotNull(vars, _dispIdsOfKeywordArgsPinned);
            AddNotNull(vars, _propertyPutDispId);
            return vars.Count > 0 ? Expression.Scope(expression, vars) : expression;
        }

        private Expression GenerateTryBlock() {
            //
            // Declare variables
            //
            ParameterExpression excepInfo = Expression.Variable(typeof(ExcepInfo), "excepInfo");
            ParameterExpression argErr = Expression.Variable(typeof(uint), "argErr");
            ParameterExpression hresult = Expression.Variable(typeof(int), "hresult");

            List<Expression> tryStatements = new List<Expression>();
            Expression expr;

            if (_keywordArgNames.Length > 0) {
                string[] names = _keywordArgNames.AddFirst(_methodDesc.Name);

                expr = Expression.AssignField(
                    DispParamsVariable,
                    typeof(ComTypes.DISPPARAMS).GetField("rgdispidNamedArgs"),
                    Expression.Call(typeof(UnsafeMethods).GetMethod("GetIdsOfNamedParameters"),
                        DispatchObjectVariable,
                        Expression.Constant(names),
                        DispIdVariable,
                        DispIdsOfKeywordArgsPinnedVariable
                    )
                );
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
                    ParamVariantsVariable,
                    variantIndex,
                    parameters[i + 1]);
                tryStatements.AddRange(marshalStatements);
            }

            //
            // Call Invoke
            //
            MethodCallExpression invoke = Expression.Call(
                typeof(UnsafeMethods).GetMethod("IDispatchInvoke"),
                DispatchPointerVariable,
                DispIdVariable,
                Expression.Constant(
                    _methodDesc.IsPropertyPut ?
                        ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT :
                        ComTypes.INVOKEKIND.INVOKE_FUNC | ComTypes.INVOKEKIND.INVOKE_PROPERTYGET
                ), // INVOKE_PROPERTYGET should only be needed for COM objects without typeinfo, where we might have to treat properties as methods
                DispParamsVariable,
                InvokeResultVariable,
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
                Expression.Constant(_methodDesc.Name, typeof(string))
            );
            tryStatements.Add(expr);

            //
            // _returnValue = (ReturnType)_invokeResult.ToObject();
            //
            Expression invokeResultObject =
                Expression.Call(
                    InvokeResultVariable,
                    typeof(Variant).GetMethod("ToObject"));

            ArgBuilder[] argBuilders = _varEnumSelector.GetArgBuilders();

            Expression[] parametersForUpdates = MakeArgumentExpressions();
            Expression returnValues = _varEnumSelector.ReturnBuilder.ToExpression(invokeResultObject);
            expr = Expression.Assign(ReturnValueVariable, returnValues);
            tryStatements.Add(expr);

            for (int i = 0, n = argBuilders.Length; i < n; i++) {
                Expression updateFromReturn = argBuilders[i].UpdateFromReturn(parametersForUpdates[i + 1]);
                if (updateFromReturn != null) {
                    tryStatements.Add(updateFromReturn);
                }
            }

            return Expression.Scope(
                Expression.Block(tryStatements),
                excepInfo, argErr, hresult
            );
        }

        private Expression GenerateFinallyBlock() {
            List<Expression> finallyStatements = new List<Expression>();
            Expression expr;

            //
            // _dispatchObject.ReleaseDispatchPointer(_dispatchPointer);
            //
            expr = Expression.Call(
                DispatchObjectVariable,
                typeof(IDispatchObject).GetMethod("ReleaseDispatchPointer"),
                DispatchPointerVariable
            );
            finallyStatements.Add(expr);

            //
            // Clear memory allocated for marshalling
            //
            foreach (VariantBuilder variantBuilder in _varEnumSelector.VariantBuilders) {
                List<Expression> clearingStatements = variantBuilder.Clear(ParamVariantsVariable);
                finallyStatements.AddRange(clearingStatements);
            }

            //
            // _invokeResult.Clear()
            //

            expr = Expression.Call(
                InvokeResultVariable,
                typeof(Variant).GetMethod("Clear")
            );
            finallyStatements.Add(expr);

            //
            // _dispIdsOfKeywordArgsPinned.Free()
            //
            if (_dispIdsOfKeywordArgsPinned != null) {
                expr = Expression.Call(
                    DispIdsOfKeywordArgsPinnedVariable,
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

            //
            // _dispId = ((DispCallable)this).ComMethodDesc.DispId;
            //
            Expression expr = Expression.Assign(
                DispIdVariable,
                Expression.Property(
                    _method,
                    typeof(ComMethodDesc).GetProperty("DispId")));
            exprs.Add(expr);

            //
            // _dispParams.rgvararg = RuntimeHelpers.UnsafeMethods.ConvertVariantByrefToPtr(ref _paramVariants._element0)
            //
            if (_totalExplicitArgs != 0) {
                MethodCallExpression addrOfParamVariants = Expression.Call(
                    typeof(UnsafeMethods).GetMethod("ConvertVariantByrefToPtr"),
                    Expression.Field(
                        ParamVariantsVariable,
                        VariantArray.GetField(0)));
                expr = Expression.AssignField(
                    DispParamsVariable,
                    typeof(ComTypes.DISPPARAMS).GetField("rgvarg"),
                    addrOfParamVariants);

                exprs.Add(expr);
            }

            //
            // _dispParams.cArgs = <number_of_params>;
            //
            expr = Expression.AssignField(
                DispParamsVariable,
                typeof(ComTypes.DISPPARAMS).GetField("cArgs"),
                Expression.Constant(_totalExplicitArgs));
            exprs.Add(expr);

            if (_methodDesc.IsPropertyPut) {
                //
                // dispParams.cNamedArgs = 1;
                // dispParams.rgdispidNamedArgs = RuntimeHelpers.UnsafeMethods.GetNamedArgsForPropertyPut()
                //
                expr = Expression.AssignField(
                    DispParamsVariable,
                    typeof(ComTypes.DISPPARAMS).GetField("cNamedArgs"),
                    Expression.Constant(1));
                exprs.Add(expr);

                expr = Expression.Assign(
                    PropertyPutDispIdVariable,
                    Expression.Constant(ComDispIds.DISPID_PROPERTYPUT));
                exprs.Add(expr);

                MethodCallExpression rgdispidNamedArgs = Expression.Call(
                    typeof(UnsafeMethods).GetMethod("ConvertInt32ByrefToPtr"),
                    PropertyPutDispIdVariable
                );

                expr = Expression.AssignField(
                    DispParamsVariable,
                    typeof(ComTypes.DISPPARAMS).GetField("rgdispidNamedArgs"),
                    rgdispidNamedArgs);
                exprs.Add(expr);
            } else {
                //
                // _dispParams.cNamedArgs = N;
                //
                expr = Expression.AssignField(
                    DispParamsVariable,
                    typeof(ComTypes.DISPPARAMS).GetField("cNamedArgs"),
                    Expression.Constant(_keywordArgNames.Length));
                exprs.Add(expr);
            }

            //
            // _dispatchObject = ((DispCallable)this).DispatchObject
            // _dispatchPointer = dispatchObject.GetDispatchPointerInCurrentApartment();
            //

            expr = Expression.Assign(DispatchObjectVariable, _dispatch);
            exprs.Add(expr);

            expr = Expression.Assign(
                DispatchPointerVariable,
                Expression.Call(
                    DispatchObjectVariable,
                    typeof(IDispatchObject).GetMethod("GetDispatchPointerInCurrentApartment")));
            exprs.Add(expr);

            Expression tryStatements = GenerateTryBlock();

            Expression finallyStatements = GenerateFinallyBlock();

            expr = Expression.TryFinally(tryStatements, finallyStatements);
            exprs.Add(expr);

            exprs.Add(ReturnValueVariable);
            List<ParameterExpression> vars = new List<ParameterExpression>();
            foreach (ArgBuilder ab in _varEnumSelector.GetArgBuilders()) {
                vars.AddRange(ab.TemporaryVariables);
            }
            return Expression.Scope(Expression.Comma(exprs), vars);
        }

        private Expression MakeUnoptimizedInvokeTarget() {
            Expression[] args = new Expression[_args.Length];
            for (int i = 0; i < args.Length; i++) {
                args[i] = Expression.ConvertHelper(_args[i].Expression, typeof(object));
            }

            // UnoptimizedInvoke(ComMethodDesc method, IDispatchObject dispatch, string[] keywordArgNames, object[] explicitArgs)
            return Expression.Call(
                typeof(ComRuntimeHelpers).GetMethod("UnoptimizedInvoke"),
                _method,
                _dispatch,
                Expression.Constant(_keywordArgNames),
                Expression.NewArrayInit(typeof(object), args)
            );
        }

        /// <summary>
        /// Gets expressions to access all the arguments. This includes the instance argument.
        /// </summary>
        private Expression[] MakeArgumentExpressions() {
            if (_instance == null) {
                return MetaObject.GetExpressions(_args);
            }

            Expression[] res = new Expression[_args.Length + 1];
            res[0] = _instance;
            for (int i = 0; i < _args.Length; i++) {
                res[i + 1] = _args[i].Expression;
            }
            return res;
        }

        /// <summary>
        /// Gets all of the argument names. The instance argument is not included
        /// </summary>
        private string[] GetArgumentNames() {
            // Get names of named arguments
            if (_arguments.Count == 0) {
                return new string[0];
            } else {
                var result = new List<string>();
                foreach (Argument arg in _arguments) {
                    if (arg.ArgumentType == ArgumentType.Named) {
                        result.Add(((NamedArgument)arg).Name);
                    }
                }
                return result.ToArray();
            }
        }
    }
}

#endif
