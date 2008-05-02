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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Actions.ComDispatch {
    
    using Ast = Microsoft.Scripting.Ast.Expression;

    /// <summary>
    /// Creates a rule for calling a COM method using IDispatch::Invoke
    /// </summary>
    /// <typeparam name="T">Type of the DynamicSite</typeparam>
    public class IDispatchCallBinderHelper<T> :  CallBinderHelper<T, CallAction> {

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

        public IDispatchCallBinderHelper(CodeContext context, CallAction action, object[] args)
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
            return Ast.ConvertHelper(ThisParameter, typeof(DispCallable));
        }

        private Expression GetIDispatchObject() {
            Expression dispCallable = GetDispCallable();
            return Ast.ReadProperty(dispCallable, typeof(DispCallable).GetProperty("DispatchObject"));
        }

        private Expression GetDispId() {
            Expression dispCallable = Ast.ConvertHelper(ThisParameter, typeof(DispCallable));
            Expression comMethodDesc = Ast.ReadProperty(dispCallable, typeof(DispCallable).GetProperty("ComMethodDesc"));
            return Ast.ReadProperty(comMethodDesc, typeof(ComMethodDesc).GetProperty("DispId"));
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

                expr = Ast.Write(
                    _dispParams,
                    typeof(ComTypes.DISPPARAMS).GetField("rgdispidNamedArgs"),
                    Ast.Call(typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("GetIdsOfNamedParameters"),
                        Ast.Read(_dispatchObject),
                        Ast.Constant(names),
                        Ast.Read(_dispId),
                        Ast.Read(_dispIdsOfKeywordArgsPinned)));
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
            MethodCallExpression invoke = Ast.Call(
                typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("IDispatchInvoke"),
                Ast.ReadDefined(_dispatchPointer),
                Ast.ReadDefined(_dispId),
                Ast.Constant(
                    Callable is DispPropertyPut ?
                        ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT :
                        ComTypes.INVOKEKIND.INVOKE_FUNC | ComTypes.INVOKEKIND.INVOKE_PROPERTYGET
                             ), // INVOKE_PROPERTYGET should only be needed for COM objects without typeinfo, where we might have to treat properties as methods
                Ast.ReadDefined(_dispParams),
                Ast.ReadDefined(_invokeResult),
                Ast.ReadDefined(excepInfo),
                Ast.ReadDefined(argErr));
            expr = Ast.Write(hresult, invoke);
            tryStatements.Add(expr);

            //
            // ComRuntimeHelpers.CheckThrowException(hresult, excepInfo, argErr, ThisParameter);
            //
            expr = Ast.Call(
                typeof(ComRuntimeHelpers).GetMethod("CheckThrowException"),
                Ast.ReadDefined(hresult),
                Ast.ReadDefined(excepInfo),
                Ast.ReadDefined(argErr),
                GetDispCallable()
            );
            tryStatements.Add(expr);

            //
            // _returnValue = (ReturnType)_invokeResult.ToObject();
            //
            Expression invokeResultObject = Binder.ConvertExpression(
                Ast.Call(
                    Ast.ReadDefined(_invokeResult),
                    typeof(Variant).GetMethod("ToObject")),
                Rule.ReturnType);

            ArgBuilder[] argBuilders = _varEnumSelector.GetArgBuilders();

            Expression[] parametersForUpdates = MakeArgumentExpressions();
            Expression returnValues = _varEnumSelector.ReturnBuilder.ToExpression(
                _methodBinderContext, 
                argBuilders, 
                parametersForUpdates, 
                invokeResultObject);
            expr = Ast.Write(_returnValue, returnValues);
            tryStatements.Add(expr);

            foreach (ArgBuilder argBuilder in argBuilders) {
                Expression updateFromReturn = argBuilder.UpdateFromReturn(_methodBinderContext, parametersForUpdates);
                if (updateFromReturn != null) {
                    tryStatements.Add(updateFromReturn);
                }
            }

            return Ast.Block(tryStatements);
        }

        private Expression GenerateFinallyBlock() {
            List<Expression> finallyStatements = new List<Expression>();
            Expression expr;

            //
            // _dispatchObject.ReleaseDispatchPointer(_dispatchPointer);
            //
            expr = Ast.Call(
                Ast.ReadDefined(_dispatchObject),
                typeof(IDispatchObject).GetMethod("ReleaseDispatchPointer"),
                Ast.ReadDefined(_dispatchPointer)
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

            expr = Ast.Call(
                Ast.ReadDefined(_invokeResult),
                typeof(Variant).GetMethod("Clear")
            );
            finallyStatements.Add(expr);

            //
            // _dispIdsOfKeywordArgsPinned.Free()
            //
            if (_dispIdsOfKeywordArgsPinned != null) {
                expr = Ast.Call(
                    Ast.ReadDefined(_dispIdsOfKeywordArgsPinned),
                    typeof(GCHandle).GetMethod("Free")
                );
                finallyStatements.Add(expr);
            }

            return Ast.Block(finallyStatements);
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
            expr = Ast.Write(
                _dispId,
                Ast.ReadProperty(
                    Ast.ReadProperty(
                        Ast.ConvertHelper(ThisParameter, typeof(DispCallable)),
                        typeof(DispCallable).GetProperty("ComMethodDesc")),
                    typeof(ComMethodDesc).GetProperty("DispId")));
            exprs.Add(expr);

            //
            // _dispParams.rgvararg = RuntimeHelpers.UnsafeMethods.ConvertVariantByrefToPtr(ref _paramVariants._element0)
            //
            if (_totalExplicitArgs != 0) {
                MethodCallExpression addrOfParamVariants = Ast.Call(
                    typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("ConvertVariantByrefToPtr"),
                    Ast.ReadField(
                        Ast.ReadDefined(_paramVariants),
                        VariantArray.GetField(0)));
                expr = Ast.Write(
                    _dispParams,
                    typeof(ComTypes.DISPPARAMS).GetField("rgvarg"),
                    addrOfParamVariants);

                exprs.Add(expr);
            }

            //
            // _dispParams.cArgs = <number_of_params>;
            //
            expr = Ast.Write(
                _dispParams,
                typeof(ComTypes.DISPPARAMS).GetField("cArgs"),
                Ast.Constant(_totalExplicitArgs));
            exprs.Add(expr);

            if (Callable is DispPropertyPut) {
                //
                // dispParams.cNamedArgs = 1;
                // dispParams.rgdispidNamedArgs = RuntimeHelpers.UnsafeMethods.GetNamedArgsForPropertyPut()
                //
                expr = Ast.Write(
                    _dispParams,
                    typeof(ComTypes.DISPPARAMS).GetField("cNamedArgs"),
                    Ast.Constant(1));
                exprs.Add(expr);

                VariableExpression _propertyPutDispId = Rule.GetTemporary(typeof(int), "propertyPutDispId");

                expr = Ast.Write(
                    _propertyPutDispId,
                    Ast.Constant(ComDispIds.DISPID_PROPERTYPUT));

                MethodCallExpression rgdispidNamedArgs = Ast.Call(
                    typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("ConvertInt32ByrefToPtr"),
                    Ast.Read(_propertyPutDispId));

                expr = Ast.Write(
                    _dispParams,
                    typeof(ComTypes.DISPPARAMS).GetField("rgdispidNamedArgs"),
                    rgdispidNamedArgs);
                exprs.Add(expr);
            } else {
                //
                // _dispParams.cNamedArgs = N;
                //
                expr = Ast.Write(
                    _dispParams,
                    typeof(ComTypes.DISPPARAMS).GetField("cNamedArgs"),
                    Ast.Constant(_keywordArgNames.Length));
                exprs.Add(expr);
            }

            //
            // _dispatchObject = ((DispCallable)this).DispatchObject
            // _dispatchPointer = dispatchObject.GetDispatchPointerInCurrentApartment();
            //

            expr = Ast.Write(
                _dispatchObject,
                GetIDispatchObject());
            exprs.Add(expr);

            expr = Ast.Write(
                _dispatchPointer,
                Ast.Call(
                    Ast.ReadDefined(_dispatchObject),
                    typeof(IDispatchObject).GetMethod("GetDispatchPointerInCurrentApartment")));
            exprs.Add(expr);

            Expression tryStatements = GenerateTryBlock();

            Expression finallyStatements = GenerateFinallyBlock();

            expr = Ast.TryFinally(tryStatements, finallyStatements);
            exprs.Add(expr);

            expr = Ast.Return(
                Ast.ReadDefined(_returnValue));
            exprs.Add(expr);

            return Ast.Block(exprs);
        }

        # region Unoptimized or error cases

        private Expression MakeUnoptimizedInvokeTarget() {
            Expression dispCallable = GetDispCallable();
            Expression[] arguments = MakeArgumentExpressions();
            arguments = ArrayUtils.RemoveFirst(arguments); // Remove the instance argument

            arguments = ArrayUtils.Insert<Expression>(Ast.CodeContext(), Ast.Constant(_keywordArgNames), arguments);
            MethodInfo unoptimizedInvoke = typeof(DispCallable).GetMethod("UnoptimizedInvoke");
            Expression target = Ast.ComplexCallHelper(dispCallable, unoptimizedInvoke, arguments);
            return Ast.Return(target);
        }

        private RuleBuilder<T> MakeUnoptimizedInvokeRule() {
            Rule.Test = Test;
            Rule.Target = MakeUnoptimizedInvokeTarget();
            return Rule;
        }

        #endregion

        public override RuleBuilder<T> MakeRule() {
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

            Rule.Test = Test;

            Expression target = MakeIDispatchInvokeTarget();
            Rule.Target = target;

            return Rule;
        }
    }
}

#endif
