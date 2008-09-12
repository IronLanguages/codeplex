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
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

namespace Microsoft.Linq.Expressions.Compiler {

    /// <summary>
    /// Dynamic Language Runtime Compiler.
    /// This part compiles lambdas.
    /// </summary>
    partial class LambdaCompiler {
        private static int _Counter;
        private static int _GeneratorCounter;

        private static readonly string[] _GeneratorSigNames = new string[] { "$gen", "$ret" };

        internal void EmitConstantArray<T>(T[] array) {
            // Emit as runtime constant if possible
            // if not, emit into IL
            if (_boundConstants != null) {
                EmitConstant(array, typeof(T[]));
            } else {
                _ilg.EmitArray(array);
            }
        }

        private void EmitClosureCreation(LambdaCompiler inner, bool closure) {
            bool boundConstants = inner._boundConstants != null;
            if (!closure && !boundConstants && !(inner._method is DynamicMethod)) {
                _ilg.EmitNull();
                return;
            }

            // new Closure(constantPool, currentHoistedLocals)
            if (boundConstants) {
                EmitConstant(inner._boundConstants.ToArray(), typeof(object[]));
            } else {
                _ilg.EmitNull();
            }
            if (_scope.NearestHoistedLocals != null) {
                _scope.EmitGet(_scope.NearestHoistedLocals.SelfVariable);
            } else {
                _ilg.EmitNull();
            }
            _ilg.EmitNew(typeof(Closure).GetConstructor(new Type[] { typeof(object[]), typeof(object[]) }));
        }

        /// <summary>
        /// Emits code which creates new instance of the delegateType delegate.
        /// 
        /// Since the delegate is getting closed over the "Closure" argument, this
        /// cannot be used with virtual/instance methods (delegateFunction must be static method)
        /// </summary>
        private void EmitDelegateConstruction(LambdaCompiler delegateFunction, Type delegateType, bool closure) {
            Assert.NotNull(delegateFunction, delegateType);

            DynamicMethod dynamicMethod = delegateFunction._method as DynamicMethod;
            if (dynamicMethod != null) {
                // dynamicMethod.CreateDelegate(delegateType, closure)
                EmitConstant(dynamicMethod, typeof(DynamicMethod));
                _ilg.EmitType(delegateType);
                EmitClosureCreation(delegateFunction, closure);
                _ilg.EmitCall(typeof(DynamicMethod).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object) }));
                _ilg.Emit(OpCodes.Castclass, delegateType);
            } else {
                // new DelegateType(closure)

                EmitClosureCreation(delegateFunction, closure);
                _ilg.Emit(OpCodes.Ldftn, (MethodInfo)delegateFunction._method);
                _ilg.Emit(OpCodes.Newobj, (ConstructorInfo)(delegateType.GetMember(".ctor")[0]));
            }
        }

        /// <summary>
        /// Emits a delegate to the method generated for the LambdaExpression.
        /// May end up creating a wrapper to match the requested delegate type.
        /// </summary>
        /// <param name="lambda">Lambda for which to generate a delegate</param>
        /// <param name="delegateType">Type of the delegate.</param>
        private void EmitDelegateConstruction(LambdaExpression lambda, Type delegateType) {
            // 1. create the scope
            CompilerScope scope = VariableBinder.Bind(_scope, lambda);

            // 2. create the signature
            List<Type> paramTypes;
            List<string> paramNames;
            string implName;
            Type returnType;
            ComputeSignature(scope.Lambda, out paramTypes, out paramNames, out implName, out returnType);

            // 3. create the new compiler
            LambdaCompiler impl = CreateLambdaCompiler(
                scope,
                implName,
                returnType,
                paramTypes,
                paramNames.ToArray(),
                scope.IsClosure
            );

            // 4. emit the lambda
            impl.EmitBody();
            impl.Finish();

            // 5. emit the delegate creation in the outer lambda
            EmitDelegateConstruction(impl, delegateType, scope.IsClosure);
        }

        /// <summary>
        /// Creates the signature for the actual CLR method to create. The base types come from the
        /// lambda/LambdaExpression (or its wrapper method), this method may pre-pend an argument to hold
        /// closure information (for closing over constant pool or the lexical closure)
        /// </summary>
        private static Type[] MakeParameterTypeArray(IList<Type> baseTypes, bool dynamicMethod, bool closure) {
            Assert.NotNullItems(baseTypes);

            Type[] signature;

            if (dynamicMethod || closure) {
                signature = new Type[baseTypes.Count + 1];
                baseTypes.CopyTo(signature, 1);

                // Add the closure argument
                signature[0] = typeof(Closure);
            } else {
                signature = new Type[baseTypes.Count];
                baseTypes.CopyTo(signature, 0);
            }

            return signature;
        }

        /// <summary>
        /// Creates the signature for the lambda as list of types and list of names separately
        /// </summary>
        private static void ComputeSignature(
            LambdaExpression lambda,
            out List<Type> paramTypes,
            out List<string> paramNames,
            out string implName,
            out Type returnType) {

            paramTypes = new List<Type>();
            paramNames = new List<string>();

            foreach (ParameterExpression p in lambda.Parameters) {
                paramTypes.Add(p.IsByRef ? p.Type.MakeByRefType() : p.Type);
                paramNames.Add(p.Name);
            }

            implName = GetGeneratedName(lambda.Name);
            returnType = lambda.ReturnType;
        }

        private static string GetGeneratedName(string prefix) {
            return prefix + "$" + Interlocked.Increment(ref _Counter);
        }

        // Used by Compiler
        internal void EmitBody() {
            if (_generatorInfo != null) {
                EmitGeneratorLambdaBody();
            } else {
                EmitLambdaBody();
            }
        }

        private void EmitLambdaBody() {
            _scope.EnterLambda(this);

            EmitLambdaStart(_lambda);

            Type returnType = _method.GetReturnType();
            if (returnType == typeof(void)) {
                EmitExpressionAsVoid(_lambda.Body);
                EmitLambdaEnd(_lambda);
            } else {
                if (_lambda.Body.Type != typeof(void)) {
                    Expression body = _lambda.Body;
                    Debug.Assert(TypeUtils.AreReferenceAssignable(returnType, body.Type));
                    EmitExpression(body);
                    EmitLambdaEnd(_lambda);
                } else {
                    EmitExpressionAsVoid(_lambda.Body);
                    EmitLambdaEnd(_lambda);

                    if (TypeUtils.CanAssign(typeof(object), returnType)) {
                        _ilg.EmitNull();
                    } else {
                        _ilg.EmitMissingValue(returnType);
                    }
                }
            }
            //must be the last instruction in the body
            EmitReturn();
        }

        #region DebugMarkers

        private void EmitLambdaStart(LambdaExpression lambda) {
            if (!_emitDebugSymbols) {
                return;
            }

            // get the source file & language id, if present, otherwise use the
            // information from the parent lambda
            SourceFileInformation fileInfo;
            if (lambda.Annotations.TryGet<SourceFileInformation>(out fileInfo)) {
                var module = (ModuleBuilder)_typeBuilder.Module;
                _debugSymbolWriter = module.DefineDocument(
                    fileInfo.FileName,
                    fileInfo.LanguageGuid,
                    fileInfo.VendorGuid,
                    SymbolGuids.DocumentType_Text
                );
            }

            // ensure a break point exists at the top
            // of the file if there isn't a statement
            // flush with the start of the file.
            SourceSpan lambdaSpan;
            if (!lambda.Annotations.TryGet<SourceSpan>(out lambdaSpan)) {
                return;
            }

            SourceSpan bodySpan = lambda.Body.Annotations.Get<SourceSpan>();
            if (bodySpan.Start.IsValid) {
                if (bodySpan.Start != lambdaSpan.Start) {
                    EmitPosition(lambdaSpan.Start, lambdaSpan.Start);
                }
            } else {
                Block body = lambda.Body as Block;
                if (body != null) {
                    for (int i = 0; i < body.Expressions.Count; i++) {
                        bodySpan = body.Expressions[i].Annotations.Get<SourceSpan>();
                        if (bodySpan.Start.IsValid) {
                            if (bodySpan.Start != lambdaSpan.Start) {
                                EmitPosition(lambdaSpan.Start, lambdaSpan.Start);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void EmitLambdaEnd(LambdaExpression lambda) {
            if (!_emitDebugSymbols) {
                return;
            }

            // ensure we emit a sequence point at the end
            // so the user can inspect any info before exiting
            // the function.  Also make sure additional code
            // isn't associated with this function.           
            SourceSpan span;
            if (lambda.Annotations.TryGet<SourceSpan>(out span)) {
                EmitPosition(span.End, span.End);
            }
            EmitSequencePointNone();
        }

        #endregion


        #region GeneratorLambdaExpression

        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        /// <returns></returns>
        private LambdaCompiler CreateGeneratorLambdaCompiler() {
            // Create the GenerateNext function
            LambdaCompiler ncg = CreateLambdaCompiler(
                _scope,
                GetGeneratorMethodName(_scope.Lambda.Name),     // Method Name
                typeof(bool),                           // Return Type
                new Type[] {                            // signature
                    typeof(Generator),
                    typeof(object).MakeByRefType()
                },
                _GeneratorSigNames,                     // param names
                _scope.HasHoistedLocals || _scope.IsClosure
            );

            // We are emitting generator, mark the Compiler
            ncg.IsGeneratorBody = true;

            return ncg;
        }

        /// <summary>
        /// Emits the body of the function that creates a Generator object.  Also creates another
        /// Compiler for the inner method which implements the user code defined in the generator.
        /// </summary>
        private void EmitGeneratorLambdaBody() {
            // 1. Create a nested code gen and emit the generator body
            LambdaCompiler ncg = CreateGeneratorLambdaCompiler();
            ncg.EmitGenerator();

            // 2. Emit the generator construction
            _scope.EnterGeneratorOuter(this);
            bool needsClosure = _scope.IsClosure || _scope.HasHoistedLocals;
            EmitDelegateConstruction(ncg, typeof(GeneratorNext), needsClosure);

            // The presence of yieldmarkers map indicates that the generator needs to be debugabble
            if (_generatorInfo.YieldMarkers != null) {
                // Emit the debug-cookie map
                EmitDebugCookieMap(_generatorInfo.YieldMarkers);

                _ilg.EmitNew(typeof(DebuggableGenerator), new Type[] { typeof(GeneratorNext), typeof(int[]) });

            } else {
                _ilg.EmitNew(typeof(Generator), new Type[] { typeof(GeneratorNext) });
            }
            EmitReturn();
        }

        private void EmitDebugCookieMap(IList<int> yieldMarkers) {
            if (_boundConstants != null) {
                // Emit as a constant
                EmitConstant(yieldMarkers.ToArray(), typeof(int[]));
            } else {
                // Emit creation of the array
                _ilg.EmitArray<int>(yieldMarkers);
            }
        }

        private static string GetGeneratorMethodName(string name) {
            return name + "$g" + _GeneratorCounter++;
        }

        private void EmitGenerator() {
            _scope.EnterGeneratorInner(this);

            IList<YieldTarget> topTargets = _generatorInfo.TopTargets;
            Debug.Assert(topTargets != null);

            Label[] jumpTable = new Label[topTargets.Count];
            for (int i = 0; i < jumpTable.Length; i++) {
                jumpTable[i] = topTargets[i].EnsureLabel(this);
            }

            EmitLambdaArgument(0);
            _ilg.EmitCall(typeof(Generator).GetProperty("Location").GetGetMethod());
            _ilg.Emit(OpCodes.Dup);
            GotoRouter = _ilg.GetLocal(typeof(int));
            _ilg.Emit(OpCodes.Stloc, GotoRouter);
            _ilg.Emit(OpCodes.Switch, jumpTable);

            // fall-through on first pass
            // yield statements will insert the needed labels after their returns

            EmitExpression(_lambda.Body);

            // fall-through is almost always possible in generators, so this
            // is almost always needed
            EmitReturnInGenerator(null);
            Finish();

            _ilg.FreeLocal(GotoRouter);
            GotoRouter = null;
        }

        #endregion
    }
}
