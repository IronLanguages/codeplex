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
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System.Threading;

namespace Microsoft.Linq.Expressions.Compiler {

    /// <summary>
    /// Dynamic Language Runtime Compiler.
    /// This part compiles lambdas.
    /// </summary>
    partial class LambdaCompiler {
        private static int _Counter;

        internal void EmitConstantArray<T>(T[] array) {
            // Emit as runtime constant if possible
            // if not, emit into IL
            if (_dynamicMethod) {
                EmitConstant(array, typeof(T[]));
            } else {
                _ilg.EmitArray(array);
            }
        }

        private void EmitClosureCreation(LambdaCompiler inner) {
            bool closure = inner._scope.NeedsClosure;
            bool boundConstants = inner._boundConstants.Count > 0;

            if (!closure && !boundConstants) {
                _ilg.EmitNull();
                return;
            }

            // new Closure(constantPool, currentHoistedLocals)
            if (boundConstants) {
                _boundConstants.EmitConstant(this, inner._boundConstants.ToArray(), typeof(object[]));
            } else {
                _ilg.EmitNull();
            }
            if (closure) {
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
        /// cannot be used with virtual/instance methods (inner must be static method)
        /// </summary>
        private void EmitDelegateConstruction(LambdaCompiler inner, Type delegateType) {
            DynamicMethod dynamicMethod = inner._method as DynamicMethod;
            if (dynamicMethod != null) {
                // dynamicMethod.CreateDelegate(delegateType, closure)
                _boundConstants.EmitConstant(this, dynamicMethod, typeof(DynamicMethod));
                _ilg.EmitType(delegateType);
                EmitClosureCreation(inner);
                _ilg.EmitCall(typeof(DynamicMethod).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object) }));
                _ilg.Emit(OpCodes.Castclass, delegateType);
            } else {
                // new DelegateType(closure)
                EmitClosureCreation(inner);
                _ilg.Emit(OpCodes.Ldftn, (MethodInfo)inner._method);
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
            // 1. create the signature
            List<Type> paramTypes;
            List<string> paramNames;
            string implName;
            Type returnType;
            ComputeSignature(lambda, out paramTypes, out paramNames, out implName, out returnType);

            // 2. create the new compiler
            LambdaCompiler impl;
            if (_dynamicMethod) {
                impl = CreateDynamicCompiler(_tree, lambda, implName, returnType, paramTypes, paramNames, _emitDebugSymbols, _method is DynamicMethod);
            } else {
                impl = CreateStaticCompiler(_tree, lambda, _typeBuilder, implName, TypeUtils.PublicStatic, returnType, paramTypes, paramNames, _dynamicMethod, _emitDebugSymbols);
            }

            // TODO: better way to flow this in?
            impl._debugSymbolWriter = _debugSymbolWriter;
            impl._symbolWriters = _symbolWriters;

            // 3. emit the lambda
            impl.EmitLambdaBody(_scope);

            // 4. emit the delegate creation in the outer lambda
            EmitDelegateConstruction(impl, delegateType);
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

        private void EmitLambdaBody(CompilerScope parent) {
            _scope.Enter(this, parent);
            if (_emitDebugSymbols && _symbolWriters == null) {
                _symbolWriters = new Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter>();
            }

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

                    _ilg.EmitDefault(returnType);
                }
            }
            //must be the last instruction in the body
            EmitReturn();
            _scope.Exit();
            Finish();
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
                BlockExpression body = lambda.Body as BlockExpression;
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
    }
}
