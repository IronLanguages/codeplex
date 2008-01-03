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
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions.ComDispatch {
    using Ast = Microsoft.Scripting.Ast.Ast;

    /// <summary>
    /// Creates a rule for calling a COM method using IDispatch::Invoke
    /// </summary>
    /// <typeparam name="T">Type of the DynamicSite</typeparam>
    public class IDispatchCallBinderHelper<T> :  CallBinderHelper<T, CallAction> {
        private object _currentThis;                                // the implicit first argument
        private object[] _currentExplicitArgs;                      // the explicit arguments the binder is binding to (does not include the "this" argument)
        VarEnumSelector _varEnumSelector;

        public IDispatchCallBinderHelper(CodeContext context, CallAction action, object[] args) : base(context, action, args) {
            Contract.RequiresNotNull(args, "args");
            if (args.Length < 1) {
                throw new ArgumentException("Must receive at least one argument, the target to call", "args");
            }

            _currentThis = args[0];
            _currentExplicitArgs = ArrayUtils.RemoveFirst(args);
        }

        private Expression ThisParameter { get { return Rule.Parameters[0]; } }

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

        /// <summary>
        /// Do the work of Marshal.GetNativeVariantForObject.
        /// </summary>
        /// <param name="explicitArgIndex">This does not include the implicit "this" argument. It only includes 
        /// the explicit arguments passed by the user</param>
        /// <param name="targetComType"></param>
        /// <param name="paramVariants"></param>
        private List<Statement> MarshalArgumentToVariant(int explicitArgIndex, VarEnum targetComType, Variable paramVariants) {
            List<Statement> stmts = new List<Statement>();
            Statement stmt;

            FieldInfo variantArrayField = VariantArray.GetField(explicitArgIndex);

            // Convert the argument into a form that can be expressed as a Variant
            Expression comCompatibleObject = Rule.Parameters[1 + explicitArgIndex];
            comCompatibleObject = Binder.ConvertExpression(comCompatibleObject, VarEnumSelector.GetManagedMarshalType(targetComType));

            if (Variant.IsPrimitiveType(targetComType) ||
                (targetComType == VarEnum.VT_UNKNOWN) ||
                (targetComType == VarEnum.VT_DISPATCH)) {
                // paramVariants._elementN.AsType = (cast)argN
                stmt = Ast.Statement(
                    Ast.AssignProperty(
                        Ast.ReadField(
                            Ast.ReadDefined(paramVariants),
                            variantArrayField),
                        Variant.GetAccessor(targetComType),
                        comCompatibleObject));
                stmts.Add(stmt);
                return stmts;
            }

            switch(targetComType) {
                case VarEnum.VT_EMPTY:
                    return stmts;

                case VarEnum.VT_NULL:
                    // paramVariants._elementN.SetAsNULL();

                    stmt = Ast.Statement(
                        Ast.Call(
                            Ast.ReadField(
                                Ast.ReadDefined(paramVariants),
                                variantArrayField),
                            typeof(Variant).GetMethod("SetAsNULL")));
                    stmts.Add(stmt);
                    return stmts;

                default:
                    Debug.Assert(false, "Unexpected VarEnum");
                    return stmts;
            }
        }

        private List<Statement> ClearArgumentVariant(int explicitArgIndex, VarEnum targetComType, Variable paramVariants) {
            List<Statement> stmts = new List<Statement>();
            Statement stmt;

            FieldInfo variantArrayField = VariantArray.GetField(explicitArgIndex);

            switch (targetComType) {
                case VarEnum.VT_EMPTY:
                case VarEnum.VT_NULL:
                    return stmts;

                case VarEnum.VT_BSTR:
                case VarEnum.VT_UNKNOWN:
                case VarEnum.VT_DISPATCH:
                    // paramVariants._elementN.Clear()
                    stmt = Ast.Statement(
                        Ast.Call(
                            Ast.ReadField(
                                Ast.ReadDefined(paramVariants),
                                variantArrayField),
                            typeof(Variant).GetMethod("Clear")));
                    stmts.Add(stmt);
                    return stmts;

                default:
                    if (Variant.IsPrimitiveType(targetComType)) {
                        return stmts;
                    }
                    Debug.Assert(false, "Unexpected VarEnum");
                    return stmts;
            }
        }

        private Statement GenerateTryBlock(
            Variable dispatchPointer, 
            Variable dispId, 
            Variable dispParams, 
            Variable paramVariants,
            Variable invokeResult,
            Variable returnValue) {

            //
            // Declare variables
            //
            Variable excepInfo = Rule.GetTemporary(typeof(ExcepInfo), "excepInfo");
            Variable argErr = Rule.GetTemporary(typeof(uint), "argErr");
            Variable hresult = Rule.GetTemporary(typeof(int), "hresult");

            List<Statement> tryStatements = new List<Statement>();
            Statement stmt;

            //
            // Marshal the arguments to Variants
            //
            for (int i = 0; i < Rule.Parameters.Length - 1; i++) {
                VarEnumSelector.DispatchArgumentInfo argInfo = _varEnumSelector.DispatchArguments[i];
                List<Statement> marshalStatements = MarshalArgumentToVariant(i, argInfo.VariantType, paramVariants);
                tryStatements.AddRange(marshalStatements);
            }

            //
            // Call Invoke
            //
            MethodCallExpression invoke = Ast.Call(
                typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("IDispatchInvoke"),
                Ast.ReadDefined(dispatchPointer),
                Ast.ReadDefined(dispId),
                Ast.Constant(ComTypes.INVOKEKIND.INVOKE_FUNC|
                             ComTypes.INVOKEKIND.INVOKE_PROPERTYGET), // INVOKE_PROPERTYGET should only be needed for COM objects without typeinfo, where we might have to treat properties as methods
                Ast.ReadDefined(dispParams),
                Ast.ReadDefined(invokeResult),
                Ast.ReadDefined(excepInfo),
                Ast.ReadDefined(argErr));
            stmt = Ast.Write(hresult, invoke);
            tryStatements.Add(stmt);

            //
            // ComRuntimeHelpers.CheckThrowException(hresult, excepInfo, argErr, ThisParameter);
            //
            stmt = Ast.Statement(
                Ast.Call(
                    typeof(ComRuntimeHelpers).GetMethod("CheckThrowException"),
                    Ast.ReadDefined(hresult),
                    Ast.ReadDefined(excepInfo),
                    Ast.ReadDefined(argErr),
                    GetDispCallable()));
            tryStatements.Add(stmt);

            //
            // returnValue = (ReturnType)invokeResult.ToObject();
            //
            stmt = Ast.Write(
                returnValue,
                Binder.ConvertExpression(
                    Ast.Call(
                        Ast.ReadDefined(invokeResult),
                        typeof(Variant).GetMethod("ToObject")),
                    Rule.ReturnType));
            tryStatements.Add(stmt);

            return Ast.Block(tryStatements);
        }

        private Statement GenerateFinallyBlock(
            Variable dispatchObject,
            Variable dispatchPointer,
            Variable paramVariants,
            Variable result) {
            List<Statement> finallyStatements = new List<Statement>();
            Statement stmt;

            //
            // dispatchObject.ReleaseDispatchPointer(dispatchPointer);
            //
            stmt = Ast.Statement(
                Ast.Call(
                    Ast.ReadDefined(dispatchObject),
                    typeof(IDispatchObject).GetMethod("ReleaseDispatchPointer"),
                    Ast.ReadDefined(dispatchPointer)));
            finallyStatements.Add(stmt);

            //
            // Clear memory allocated for marshalling
            //
            for (int i = 0; i < Rule.Parameters.Length - 1; i++) {
                VarEnumSelector.DispatchArgumentInfo argInfo = _varEnumSelector.DispatchArguments[i];
                List<Statement> clearingStatements = ClearArgumentVariant(i, argInfo.VariantType, paramVariants);
                finallyStatements.AddRange(clearingStatements);
            }

            //
            // result.Clear()
            //

            stmt = Ast.Statement(
                Ast.Call(
                    Ast.ReadDefined(result),
                    typeof(Variant).GetMethod("Clear")));
            finallyStatements.Add(stmt);

            return Ast.Block(finallyStatements);
        }

        /// <summary>
        /// Create a stub for the target of the optimized lopop.
        /// </summary>
        /// <returns></returns>
        private Statement MakeIDispatchInvokeTarget() {
            Debug.Assert(_varEnumSelector.DispatchArguments.Length == (Rule.Parameters.Length - 1));

            // Since we are taking the address of local variables, this *has* to be compiled.
            Rule.CanInterpretTarget = false;

            List<Statement> stmts = new List<Statement>();
            Statement stmt;

            //
            // Declare variables, and initialize them to trivial values
            //
            Variable dispatchObject = Rule.GetTemporary(typeof(IDispatchObject), "dispatchObject");
            Variable dispatchPointer = Rule.GetTemporary(typeof(IntPtr), "dispatchPointer");
            Variable dispId = Rule.GetTemporary(typeof(int), "dispId");
            Variable dispParams = Rule.GetTemporary(typeof(ComTypes.DISPPARAMS), "dispParams");
            Variable paramVariants = Rule.GetTemporary(typeof(VariantArray), "paramVariants");
            Variable invokeResult = Rule.GetTemporary(typeof(Variant), "invokeResult");
            Variable returnValue = Rule.GetTemporary(Rule.ReturnType, "returnValue");

            //
            // dispId = ((DispCallable)this).ComMethodDesc.DispId;
            //
            stmt = Ast.Write(
                dispId,
                Ast.ReadProperty(
                    Ast.ReadProperty(
                        Ast.ConvertHelper(ThisParameter, typeof(DispCallable)),
                        typeof(DispCallable).GetProperty("ComMethodDesc")),
                    typeof(ComMethodDesc).GetProperty("DispId")));
            stmts.Add(stmt);

            //
            // dispParams.rgvararg = RuntimeHelpers.UnsafeMethods.ConvertByrefToPtr(paramVariants)
            //
            if (Rule.Parameters.Length != 0) {
                MethodCallExpression addrOfParamVariants = Ast.Call(
                    typeof(ComRuntimeHelpers.UnsafeMethods).GetMethod("ConvertByrefToPtr"),
                    Ast.ReadField(
                        Ast.ReadDefined(paramVariants),
                        VariantArray.GetField(0)));
                stmt = Ast.Write(
                    dispParams,
                    typeof(ComTypes.DISPPARAMS).GetField("rgvarg"),
                    addrOfParamVariants);

                stmts.Add(stmt);
            }

            //
            // dispParams.cArgs = <number_of_params>;
            // dispParams.cNamedArgs = 0;
            //
            stmt = Ast.Write(
                dispParams,
                typeof(ComTypes.DISPPARAMS).GetField("cArgs"),
                Ast.Constant(Rule.Parameters.Length - 1));
            stmts.Add(stmt);

            stmt = Ast.Write(
                dispParams,
                typeof(ComTypes.DISPPARAMS).GetField("cNamedArgs"),
                Ast.Constant(0));
            stmts.Add(stmt);

            //
            // dispatchObject = ((DispCallable)this).DispatchObject
            // dispatchPointer = dispatchObject.GetDispatchPointerInCurrentApartment();
            //

            stmt = Ast.Write(
                dispatchObject,
                GetIDispatchObject());
            stmts.Add(stmt);

            stmt = Ast.Write(
                dispatchPointer,
                Ast.Call(
                    Ast.ReadDefined(dispatchObject),
                    typeof(IDispatchObject).GetMethod("GetDispatchPointerInCurrentApartment")));
            stmts.Add(stmt);

            Statement tryStatements = GenerateTryBlock(
                dispatchPointer, 
                dispId, 
                dispParams, 
                paramVariants,
                invokeResult,
                returnValue);

            Statement finallyStatements = GenerateFinallyBlock(
                dispatchObject, 
                dispatchPointer,
                paramVariants,
                invokeResult);

            stmt = Ast.TryFinally(tryStatements, finallyStatements);
            stmts.Add(stmt);

            stmt = Ast.Return(
                Ast.ReadDefined(returnValue));
            stmts.Add(stmt);

            return Ast.Block(stmts);
        }

        # region Unoptimized or error cases

        private Statement MakeUnoptimizedInvokeTarget() {
            Expression dispCallable = Ast.ConvertHelper(ThisParameter, typeof(DispCallable));
            Expression[] arguments = MakeArgumentExpressions();
            arguments = ArrayUtils.Insert<Expression>(Ast.CodeContext(), arguments);
            MethodInfo unoptimizedInvoke = typeof(DispCallable).GetMethod("UnoptimizedInvoke");
            Expression target = Ast.ComplexCallHelper(dispCallable, unoptimizedInvoke, arguments);
            return Ast.Return(target);
        }

        private StandardRule<T> MakeUnoptimizedInvokeRule() {
            Rule.SetTest(Test);
            Rule.SetTarget(MakeUnoptimizedInvokeTarget());
            return Rule;
        }

        private StandardRule<T> MakeListCallRule() {
            MakeSplatTests();
            Rule.SetTest(Test);
            // We should be able to generate an optimized target. For now, we will keep things simple.
            Rule.SetTarget(MakeUnoptimizedInvokeTarget());
            return Rule;
        }

        private StandardRule<T> MakeRuleForByrefParams() {
            Test = Ast.AndAlso(
                Test,
                Ast.Equal(
                    Ast.ReadProperty(
                        Ast.ReadProperty(Ast.ConvertHelper(ThisParameter, typeof(DispCallable)),
                                         typeof(DispCallable).GetProperty("ComMethodDesc")),
                        typeof(ComMethodDesc).GetProperty("HasByrefOrOutParameters")),
                    Ast.Constant(true)));
            Rule.SetTest(Test);

            Rule.SetTarget(MakeUnoptimizedInvokeTarget());
            return Rule;
        }

        #endregion

        public override StandardRule<T> MakeRule() {
            if (Action.Signature.HasKeywordArgument()) {
                throw new NotImplementedException("Fancy call types not supported with IDispatch");
            }

            if (Action.Signature.HasListArgument()) {
                return MakeListCallRule();
            }

            DispCallable currentDispCallable = (DispCallable)_currentThis;

            // For byref or out parameters, fall back to the default helper
            if (currentDispCallable.ComMethodDesc.HasByrefOrOutParameters) {
                return MakeRuleForByrefParams();
            }

            bool hasAmbiguousMatch = false;
            try {
                _varEnumSelector = new VarEnumSelector(Binder, _currentExplicitArgs);
            } catch (AmbiguousMatchException) {
                hasAmbiguousMatch = true;
            }

            if ((_currentExplicitArgs.Length > VariantArray.NumberOfElements) ||
                (hasAmbiguousMatch) ||
                (!_varEnumSelector.IsSupportedByFastPath) ||
                (Context.LanguageContext.Options.InterpretedMode)) { // The rule we generate cannot be interpreted

                return MakeUnoptimizedInvokeRule();
            }

            // Check that all the parameters are by-value
            Test = Ast.AndAlso(
                Test,
                Ast.Equal(
                    Ast.ReadProperty(
                        Ast.ReadProperty(Ast.ConvertHelper(ThisParameter, typeof(DispCallable)),
                                         typeof(DispCallable).GetProperty("ComMethodDesc")),
                        typeof(ComMethodDesc).GetProperty("HasByrefOrOutParameters")),
                    Ast.Constant(false)));

            for (int i = 0; i < _currentExplicitArgs.Length; i++) {
                Test = Ast.AndAlso(
                    Test,
                    Rule.MakeTypeTest(CompilerHelpers.GetType(_currentExplicitArgs[i]), i + 1));
            }

            Rule.SetTest(Test);

            Statement target = MakeIDispatchInvokeTarget();
            Rule.SetTarget(target);

            return Rule;
        }
    }
}

#endif
