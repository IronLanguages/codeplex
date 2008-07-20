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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Actions.ComDispatch {
    /// <summary>
    /// Creates a rule for calling a COM method using IDispatch::Invoke
    /// </summary>
    /// <typeparam name="T">Type of the DynamicSite</typeparam>
    internal sealed class CallBinder<T> where T : class {
        private readonly CodeContext _context;
        private readonly ComMethodDesc _methodDesc;
        private readonly object[] _args;
        private readonly IList<Expression> _parameters;
        private readonly Expression _contextExpression;
        private readonly Expression _instance;
        private readonly OldCallAction _action;

        private MethodBinderContext _methodBinderContext;

        private VarEnumSelector _varEnumSelector;
        private string[] _keywordArgNames;
        private int _totalExplicitArgs; // Includes the individial elements of ArgumentKind.Dictionary (if any)

        private VariableExpression _dispatchObject;
        private VariableExpression _dispatchPointer;
        private VariableExpression _dispId;
        private VariableExpression _dispParams;
        private VariableExpression _paramVariants;
        private VariableExpression _invokeResult;
        private VariableExpression _returnValue;
        private VariableExpression _dispIdsOfKeywordArgsPinned;
        private VariableExpression _propertyPutDispId;

        private Expression _test;

        internal CallBinder(CodeContext context, OldCallAction action, object[] args, ComMethodDesc methodDesc, Expression contextExpression, IList<Expression> parameters) {
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.Requires(args.Length > 0, "args", "Must receive at least one argument, the target to call");

            _contextExpression = contextExpression;
            _parameters = parameters;
            _test = MakeTypeTest(CompilerHelpers.GetType(args[0]), parameters[0]);
            _context = context;
            _action = action;
            _args = args;
            _methodDesc = methodDesc;

            // Set Instance to some value so that CallBinderHelper has the right number of parameters to work with
            _instance = GetIDispatchObject();

            _methodBinderContext = new MethodBinderContext(Binder, ContextExpression);
        }

        private VariableExpression DispatchObjectVariable {
            get { return EnsureVariable(ref _dispatchObject, typeof(IDispatchObject), "dispatchObject"); }
        }

        private VariableExpression DispatchPointerVariable {
            get { return EnsureVariable(ref _dispatchPointer, typeof(IntPtr), "dispatchPointer"); }
        }

        private VariableExpression DispIdVariable {
            get { return EnsureVariable(ref _dispId, typeof(int), "dispId"); }
        }

        private VariableExpression DispParamsVariable {
            get { return EnsureVariable(ref _dispParams, typeof(ComTypes.DISPPARAMS), "dispParams"); }
        }

        private VariableExpression ParamVariantsVariable {
            get { return EnsureVariable(ref _paramVariants, typeof(VariantArray), "paramVariants"); }
        }

        private VariableExpression InvokeResultVariable {
            get { return EnsureVariable(ref _invokeResult, typeof(Variant), "invokeResult"); }
        }

        private VariableExpression ReturnValueVariable {
            get { return EnsureVariable(ref _returnValue, ReturnType, "returnValue"); }
        }

        private VariableExpression DispIdsOfKeywordArgsPinnedVariable {
            get { return EnsureVariable(ref _dispIdsOfKeywordArgsPinned, typeof(GCHandle), "dispIdsOfKeywordArgsPinned"); }
        }

        private VariableExpression PropertyPutDispIdVariable {
            get { return EnsureVariable(ref _propertyPutDispId, typeof(int), "propertyPutDispId"); }
        }

        private static VariableExpression EnsureVariable(ref VariableExpression var, Type type, string name) {
            if (var != null) {
                return var;
            }
            return var = Expression.Variable(type, name);
        }

        private ActionBinder Binder {
            get { return _context.LanguageContext.Binder; }
        }

        private Type ReturnType {
            get {
                return typeof(T).GetMethod("Invoke").ReturnType;
            }
        }

        private Expression ContextExpression {
            get {
                return _contextExpression;
            }
        }

        private IList<Expression> Parameters {
            get { return _parameters; }
        }

        private Expression GetDispCallable() {
            return Expression.ConvertHelper(Parameters[0], typeof(DispCallable));
        }

        private static void AddNotNull(List<VariableExpression> list, VariableExpression var) {
            if (var != null) list.Add(var);
        }

        private Expression CreateScope(Expression expression) {
            List<VariableExpression> vars = new List<VariableExpression>();
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

        private Expression GetIDispatchObject() {
            return Expression.Property(
                GetDispCallable(),
                typeof(DispCallable).GetProperty("DispatchObject")
            );
        }

        private Expression GenerateTryBlock() {
            //
            // Declare variables
            //
            VariableExpression excepInfo = Expression.Variable(typeof(ExcepInfo), "excepInfo");
            VariableExpression argErr = Expression.Variable(typeof(uint), "argErr");
            VariableExpression hresult = Expression.Variable(typeof(int), "hresult");

            List<Expression> tryStatements = new List<Expression>();
            Expression expr;

            if (_keywordArgNames.Length > 0) {
                string[] names = ArrayUtils.Insert(_methodDesc.Name, _keywordArgNames);

                expr = Expression.AssignField(
                    DispParamsVariable,
                    typeof(ComTypes.DISPPARAMS).GetField("rgdispidNamedArgs"),
                    Expression.Call(typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("GetIdsOfNamedParameters"),
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
                    _methodBinderContext,
                    ParamVariantsVariable,
                    variantIndex,
                    parameters);
                tryStatements.AddRange(marshalStatements);
            }

            //
            // Call Invoke
            //
            MethodCallExpression invoke = Expression.Call(
                typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("IDispatchInvoke"),
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
            Expression invokeResultObject = Binder.ConvertExpression(
                Expression.Call(
                    InvokeResultVariable,
                    typeof(Variant).GetMethod("ToObject")),
                ReturnType,
                ConversionResultKind.ExplicitCast,
                ContextExpression);

            ArgBuilder[] argBuilders = _varEnumSelector.GetArgBuilders();

            Expression[] parametersForUpdates = MakeArgumentExpressions();
            Expression returnValues = _varEnumSelector.ReturnBuilder.ToExpression(invokeResultObject);
            expr = Expression.Assign(ReturnValueVariable, returnValues);
            tryStatements.Add(expr);

            foreach (ArgBuilder argBuilder in argBuilders) {
                Expression updateFromReturn = argBuilder.UpdateFromReturn(parametersForUpdates);
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
                    Expression.Property(
                        GetDispCallable(),
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
                    typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("ConvertInt32ByrefToPtr"),
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

            expr = Expression.Assign(
                DispatchObjectVariable,
                GetIDispatchObject());
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

            expr = Expression.Return(
                ReturnValueVariable);
            exprs.Add(expr);

            List<VariableExpression> vars = new List<VariableExpression>();
            foreach (ArgBuilder ab in _varEnumSelector.GetArgBuilders()) {
                vars.AddRange(ab.TemporaryVariables);
            }
            return Expression.Scope(Expression.Block(exprs), vars);
        }

        # region Unoptimized or error cases

        private Expression MakeUnoptimizedInvokeTarget() {
            Expression[] arguments = MakeArgumentExpressions();
            arguments = ArrayUtils.RemoveFirst(arguments); // Remove the instance argument

            arguments = ArrayUtils.Insert<Expression>(ContextExpression, Expression.Constant(_keywordArgNames), arguments);
            Expression target = Expression.ComplexCallHelper(
                GetDispCallable(),
                typeof(DispCallable).GetMethod("UnoptimizedInvoke"),
                arguments
            );
            return Expression.Return(target);
        }

        #endregion

        internal void Bind(out Expression test, out Expression body) {
            Type[] explicitArgTypes; // will not include implicit instance argument (if any)            
            GetArgumentNamesAndTypes(out _keywordArgNames, out explicitArgTypes);
            _totalExplicitArgs = explicitArgTypes.Length;

            bool hasAmbiguousMatch = false;
            try {
                _varEnumSelector = new VarEnumSelector(Binder, ReturnType, explicitArgTypes);
            } catch (AmbiguousMatchException) {
                hasAmbiguousMatch = true;
            }

            Type[] testTypes = ArrayUtils.Insert(typeof(DispCallable), explicitArgTypes); // Add a dummy instance argument - it will not really be used
            FinishTestForCandidate(testTypes, explicitArgTypes);

            if ((explicitArgTypes.Length > VariantArray.NumberOfElements) ||
                (hasAmbiguousMatch) ||
                (!_varEnumSelector.IsSupportedByFastPath) ||
                (_context.LanguageContext.Options.InterpretedMode)) { // The rule we generate cannot be interpreted

                body = CreateScope(MakeUnoptimizedInvokeTarget());
            } else {
                body = CreateScope(MakeIDispatchInvokeTarget());
            }

            test = _test;
        }

        private Expression[] FinishTestForCandidate(IList<Type> testTypes, Type[] explicitArgTypes) {
            Expression[] exprArgs = MakeArgumentExpressions();
            Debug.Assert(exprArgs.Length == (explicitArgTypes.Length + ((_instance == null) ? 0 : 1)));
            Debug.Assert(testTypes == null || exprArgs.Length == testTypes.Count);

            MakeSplatTests();

            if (explicitArgTypes.Length > 0 && testTypes != null) {
                // We've already tested the instance, no need to test it again. So remove it before adding 
                // rules for the arguments
                Expression[] exprArgsWithoutInstance = exprArgs;
                List<Type> testTypesWithoutInstance = new List<Type>(testTypes);
                for (int i = 0; i < exprArgs.Length; i++) {
                    if (exprArgs[i] == _instance) {
                        // We found the instance, so remove it
                        exprArgsWithoutInstance = ArrayUtils.RemoveAt(exprArgs, i);
                        testTypesWithoutInstance.RemoveAt(i);
                        break;
                    }
                }

                _test = Expression.AndAlso(_test, MakeNecessaryTests(testTypesWithoutInstance.ToArray(), exprArgsWithoutInstance));
            }

            return exprArgs;
        }

        /// <summary>
        /// Gets expressions to access all the arguments. This includes the instance argument. Splat arguments are
        /// unpacked in the output. The resulting array is similar to Rule.Parameters (but also different in some ways)
        /// </summary>
        private Expression[] MakeArgumentExpressions() {
            List<Expression> exprargs = new List<Expression>();
            if (_instance != null) {
                exprargs.Add(_instance);
            }

            for (int i = 0; i < _action.Signature.ArgumentCount; i++) { // ArgumentCount(Action, Rule)
                switch (_action.Signature.GetArgumentKind(i)) {
                    case ArgumentKind.Simple:
                    case ArgumentKind.Named:
                        exprargs.Add(Parameters[i + 1]);
                        break;

                    case ArgumentKind.List:
                        IList<object> list = (IList<object>)_args[i + 1];
                        for (int j = 0; j < list.Count; j++) {
                            exprargs.Add(
                                Expression.Call(
                                    Expression.Convert(
                                        Parameters[i + 1],
                                        typeof(IList<object>)
                                    ),
                                    typeof(IList<object>).GetMethod("get_Item"),
                                    Expression.Constant(j)
                                )
                            );
                        }
                        break;

                    case ArgumentKind.Dictionary:
                        IDictionary dict = (IDictionary)_args[i + 1];

                        IDictionaryEnumerator dictEnum = dict.GetEnumerator();
                        while (dictEnum.MoveNext()) {
                            DictionaryEntry de = dictEnum.Entry;

                            string strKey = de.Key as string;
                            if (strKey == null) continue;

                            Expression dictExpr = Parameters[Parameters.Count - 1];
                            exprargs.Add(
                                Expression.Call(
                                    Expression.ConvertHelper(dictExpr, typeof(IDictionary)),
                                    typeof(IDictionary).GetMethod("get_Item"),
                                    Expression.Constant(strKey)
                                )
                            );
                        }
                        break;
                }
            }
            return exprargs.ToArray();
        }

        #region Test support

        private static Expression MakeTypeTest(Type type, Expression tested) {
            if (type == null || type == typeof(None)) {
                return Expression.Equal(tested, Expression.Null());
            }

            return RuleBuilder.MakeTypeTestExpression(type, tested);
        }

        private Expression MakeNecessaryTests(Type[] testTypes, IList<Expression> arguments) {
            Expression typeTest = Expression.Constant(true);

            if (testTypes != null) {
                for (int i = 0; i < testTypes.Length; i++) {
                    if (testTypes[i] != null) {
                        Debug.Assert(i < arguments.Count);
                        typeTest = Expression.AndAlso(typeTest, MakeTypeTest(testTypes[i], arguments[i]));
                    }
                }
            }

            return typeTest;
        }

        /// <summary>
        /// Makes test for param arrays and param dictionary parameters.
        /// </summary>
        private void MakeSplatTests() {
            if (_action.Signature.HasListArgument()) {
                MakeParamsArrayTest();
            }

            if (_action.Signature.HasDictionaryArgument()) {
                MakeParamsDictionaryTest();
            }
        }

        private void MakeParamsArrayTest() {
            int listIndex = _action.Signature.IndexOf(ArgumentKind.List);
            Debug.Assert(listIndex != -1);
            _test = Expression.AndAlso(_test, MakeParamsTest(_args[listIndex + 1], Parameters[listIndex + 1]));
        }

        private static Expression MakeParamsTest(object paramArg, Expression listArg) {
            return Expression.AndAlso(
                Expression.TypeIs(listArg, typeof(ICollection<object>)),
                Expression.Equal(
                    Expression.Property(
                        Expression.Convert(listArg, typeof(ICollection<object>)),
                        typeof(ICollection<object>).GetProperty("Count")
                    ),
                    Expression.Constant(((IList<object>)paramArg).Count)
                )
            );
        }

        private void MakeParamsDictionaryTest() {
            IDictionary dict = (IDictionary)_args[_args.Length - 1];
            IDictionaryEnumerator dictEnum = dict.GetEnumerator();

            // verify the dictionary has the same count and arguments.

            string[] names = new string[dict.Count];
            int index = 0;
            while (dictEnum.MoveNext()) {
                string name = dictEnum.Entry.Key as string;
                if (name == null) {
                    throw RuntimeHelpers.SimpleTypeError(String.Format("expected string for dictionary argument got {0}", dictEnum.Entry.Key));
                }
                names[index++] = name;
            }

            _test = Expression.AndAlso(
                _test,
                Expression.AndAlso(
                    Expression.TypeIs(Parameters[Parameters.Count - 1], typeof(IDictionary)),
                    Expression.Call(
                        typeof(RuntimeHelpers).GetMethod("CheckDictionaryMembers"),
                        Expression.Convert(Parameters[Parameters.Count - 1], typeof(IDictionary)),
                        Expression.Constant(names)
                    )
                )
            );
        }

        #endregion


        /// <summary>
        /// Gets all of the argument names and types. The instance argument is not included
        /// </summary>
        /// <param name="argNames">The names correspond to the end of argTypes.
        /// ArgumentKind.Dictionary is unpacked in the return value.
        /// This is set to an array of size 0 if there are no keyword arguments</param>
        /// <param name="argTypes">Non named arguments are returned at the beginning.
        /// ArgumentKind.List is unpacked in the return value. </param>
        private void GetArgumentNamesAndTypes(out string[] argNames, out Type[] argTypes) {
            // Get names of named arguments
            argNames = SymbolTable.IdsToStrings(_action.Signature.GetArgumentNames());
            argTypes = GetArgumentTypes();

            if (_action.Signature.HasDictionaryArgument()) {
                // need to get names from dictionary argument...
                GetDictionaryNamesAndTypes(ref argNames, ref argTypes);
            }
        }

        private Type[] GetArgumentTypes() {
            List<Type> res = new List<Type>();
            for (int i = 1; i < _args.Length; i++) {
                switch (_action.Signature.GetArgumentKind(i - 1)) {
                    case ArgumentKind.Simple:
                    case ArgumentKind.Instance:
                    case ArgumentKind.Named:
                        res.Add(CompilerHelpers.GetType(_args[i]));
                        continue;

                    case ArgumentKind.List:
                        IList<object> list = _args[i] as IList<object>;
                        if (list == null) return null;

                        for (int j = 0; j < list.Count; j++) {
                            res.Add(CompilerHelpers.GetType(list[j]));
                        }
                        break;

                    case ArgumentKind.Dictionary:
                        // caller needs to process these...
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            return res.ToArray();
        }

        private void GetDictionaryNamesAndTypes(ref string[] argNames, ref Type[] argTypes) {
            Debug.Assert(_action.Signature.GetArgumentKind(_action.Signature.ArgumentCount - 1) == ArgumentKind.Dictionary);

            List<string> names = new List<string>(argNames);
            List<Type> types = new List<Type>(argTypes);

            IDictionary dict = (IDictionary)_args[_args.Length - 1];
            IDictionaryEnumerator dictEnum = dict.GetEnumerator();
            while (dictEnum.MoveNext()) {
                DictionaryEntry de = dictEnum.Entry;

                if (de.Key is string) {
                    names.Add((string)de.Key);
                    types.Add(CompilerHelpers.GetType(de.Value));
                }
            }

            argNames = names.ToArray();
            argTypes = types.ToArray();
        }
    }
}

#endif
