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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System.Text;

namespace Microsoft.Linq.Expressions.Compiler {

    /// <summary>
    /// LambdaCompiler is responsible for compiling individual lambda (LambdaExpression). The complete tree may
    /// contain multiple lambdas, the Compiler class is reponsible for compiling the whole tree, individual
    /// lambdas are then compiled by the LambdaCompiler.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")] // TODO: fix
    internal sealed partial class LambdaCompiler {

        private delegate void WriteBack();

        // Indicates that the method should logically be treated as a
        // DynamicMethod. We need this because in debuggable code we have to
        // emit into a MethodBuilder, but we still want to pretend it's a
        // DynamicMethod
        private readonly bool _dynamicMethod;

        private readonly ILGen _ilg;

        // The TypeBuilder backing this method, if any
        private readonly TypeBuilder _typeBuilder;

        private readonly MethodInfo _method;

        // Currently active LabelTargets and their mapping to IL labels
        private LabelBlockInfo _labelBlock = new LabelBlockInfo(null, LabelBlockKind.Block);
        // Mapping of labels used for "long" jumps (jumping out and into blocks)
        private readonly Dictionary<LabelTarget, LabelInfo> _labelInfo = new Dictionary<LabelTarget, LabelInfo>();

        // Synthetic label info for doing returns
        // TODO: remove when merging ReturnStatement and GotoExpression
        private LabelInfo _returnBlock;

        // The currently active variable scope
        private CompilerScope _scope;

        // The lambda we are compiling
        private readonly LambdaExpression _lambda;

        /// <summary>
        /// Argument types
        /// 
        /// This list contains _all_ arguments on the underlying method builder (except for the
        /// "this"). There are two views on the list. First provides the raw view (shows all
        /// arguments), the second view provides view of the arguments which are in the original
        /// lambda (so first argument, which may be closure argument, is skipped in that case)
        /// </summary>
        private readonly ReadOnlyCollection<Type> _paramTypes;

        // True if we want to emitting debug symbols
        private readonly bool _emitDebugSymbols;
        // TODO: can this be readonly?
        private ISymbolDocumentWriter _debugSymbolWriter;

        // Runtime constants bound to the delegate
        private readonly List<object> _boundConstants;
        private readonly Dictionary<object, int> _constantCache;


        private LambdaCompiler(
            CompilerScope scope,
            TypeBuilder typeBuilder,
            MethodInfo method,
            ILGenerator ilg,
            IList<Type> paramTypes,
            bool dynamicMethod,
            bool emitDebugSymbols) {

            ContractUtils.Requires(dynamicMethod || method.IsStatic, "dynamicMethod");
            Debug.Assert(scope != null);
            _lambda = (LambdaExpression)scope.Expression;
            _typeBuilder = typeBuilder;
            _method = method;
            _paramTypes = new ReadOnlyCollection<Type>(paramTypes);
            _dynamicMethod = dynamicMethod;
            _scope = scope;

            // Create the ILGen instance, debug or not
            if (DebugOptions.DumpIL || DebugOptions.ShowIL) {
                _ilg = CreateDebugILGen(ilg, method, paramTypes);
            } else {
                _ilg = new ILGen(ilg);
            }

            // Initialize constant pool
            if (_dynamicMethod) {
                _boundConstants = new List<object>();
                _constantCache = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Instance);
            }

            Debug.Assert(!emitDebugSymbols || _typeBuilder != null, "emitting debug symbols requires a TypeBuilder");
            _emitDebugSymbols = emitDebugSymbols;

            // See if we can find a return label, so we can emit better IL
            AddReturnLabel(_lambda.Body);
        }

        public override string ToString() {
            return _method.ToString();
        }

        #region Properties

        internal ILGen IL {
            get { return _ilg; }
        }

        private bool IsDynamicMethod {
            get {
                return _method is DynamicMethod;
            }
        }

        internal ReadOnlyCollection<ParameterExpression> Parameters {
            get { return _lambda.Parameters; }
        }

        private bool HasClosure {
            get { return _paramTypes[0] == typeof(Closure); }
        }

        #endregion

        #region Compiler entry point

        /// <summary>
        /// Compiler entry point
        /// </summary>
        /// <param name="lambda">LambdaExpression to compile.</param>
        /// <param name="method">Product of compilation</param>
        /// <param name="delegateType">Type of the delegate to create</param>
        /// <param name="emitDebugSymbols">True to emit debug symbols, false otherwise.</param>
        /// <param name="forceDynamic">Force dynamic method regardless of save assemblies.</param>
        /// <returns>The compiled delegate.</returns>
        internal static Delegate CompileLambda(LambdaExpression lambda, Type delegateType, bool emitDebugSymbols, bool forceDynamic, out MethodInfo method) {
            // 1. Create signature
            List<Type> types;
            List<string> names;
            string name;
            Type returnType;
            ComputeSignature(lambda, out types, out names, out name, out returnType);

            // 2. Create lambda compiler
            LambdaCompiler c = CreateDynamicLambdaCompiler(
                AnalyzeLambda(lambda),
                name,
                returnType,
                types,
                null, // parameter names
                false, // closure
                emitDebugSymbols,
                forceDynamic
            );

            // 3. Emit
            c.EmitBody();

            c.Finish();

            // 4. Return the delegate.
            return c.CreateDelegate(delegateType, out method);
        }

        internal static T CompileLambda<T>(LambdaExpression lambda, bool forceDynamic, out MethodInfo method) {
            return (T)(object)CompileLambda(lambda, typeof(T), false, forceDynamic, out method);
        }

        internal static T CompileLambda<T>(LambdaExpression lambda, bool emitDebugSymbols) {
            MethodInfo method;
            return (T)(object)CompileLambda(lambda, typeof(T), emitDebugSymbols, false, out method);
        }

        internal static Delegate CompileLambda(LambdaExpression lambda, bool emitDebugSymbols) {
            MethodInfo method;
            return CompileLambda(lambda, lambda.Type, emitDebugSymbols, false, out method);
        }

        /// <summary>
        /// Creates and returns a MethodBuilder
        /// </summary>
        internal static MethodBuilder CompileLambda(LambdaExpression lambda, TypeBuilder type, MethodAttributes attributes, bool emitDebugSymbols) {
            // 1. Create signature
            List<Type> types;
            List<string> names;
            string lambdaName;
            Type returnType;
            ComputeSignature(lambda, out types, out names, out lambdaName, out returnType);

            // don't use generated name
            lambdaName = lambda.Name ?? "lambda_method";

            // 2. Create lambda compiler
            LambdaCompiler lc = CreateStaticLambdaCompiler(
                AnalyzeLambda(lambda),
                type,
                lambdaName,
                attributes,
                returnType,
                types,
                names,
                false, // dynamicMethod
                false, // closure
                emitDebugSymbols
            );

            // 3. Emit
            lc.EmitBody();
            lc.Finish();

            return (MethodBuilder)lc._method;
        }

        #endregion

        private static CompilerScope AnalyzeLambda(LambdaExpression lambda) {
            DumpLambda(lambda);

            // Spill the stack for any exception handling blocks or other
            // constructs which require entering with an empty stack
            lambda = StackSpiller.AnalyzeLambda(lambda);

            DumpLambda(lambda);

            // Bind any variable references in this lambda
            return VariableBinder.Bind(null, lambda);
        }

        [Conditional("DEBUG")]
        private static void DumpLambda(LambdaExpression lambda) {
            ExpressionWriter.Dump(lambda, lambda.Name);
        }

        private void EmitSequencePointNone() {
            EmitPosition(SourceLocation.None, SourceLocation.None);
        }

        private void EmitPosition(SourceLocation start, SourceLocation end) {
            Debug.Assert(_emitDebugSymbols);

            if (!start.IsValid || !end.IsValid) {
                start = SourceLocation.None;
                end = SourceLocation.None;
            }

            if (_debugSymbolWriter != null) {
                _ilg.MarkSequencePoint(_debugSymbolWriter, start.Line, start.Column, end.Line, end.Column);
            }

            _ilg.Emit(OpCodes.Nop);
        }

        internal LocalBuilder GetNamedLocal(Type type, string name) {
            Assert.NotNull(type);

            LocalBuilder lb = _ilg.DeclareLocal(type);
            if (_emitDebugSymbols && name != null) {
                lb.SetLocalSymInfo(name);
            }
            return lb;
        }

        private void Finish() {
            Debug.Assert(_labelBlock.Parent == null && _labelBlock.Kind == LabelBlockKind.Block);

            if (_returnBlock != null) {
                _ilg.MarkLabel(_returnBlock.Label);
                if (_returnBlock.Value != null) {
                    _ilg.Emit(OpCodes.Ldloc, _returnBlock.Value);
                }
                _ilg.Emit(OpCodes.Ret);
            }

            // Validate labels
            foreach (LabelInfo label in _labelInfo.Values) {
                label.ValidateFinish();
            }

            if (_dynamicMethod) {
                CreateDelegateMethodInfo();
            }
        }

        /// <summary>
        /// Gets the argument slot corresponding to the parameter at the given
        /// index. Assumes that the method takes a certain number of prefix
        /// arguments, followed by the real parameters stored in Parameters
        /// </summary>
        internal int GetLambdaArgument(int index) {
            return index + (HasClosure ? 1 : 0) + (_method.IsStatic ? 0 : 1);
        }

        internal Type GetLambdaArgumentType(int index) {
            return _paramTypes[index + (HasClosure ? 1 : 0)];
        }

        /// <summary>
        /// Returns the index-th argument. This method provides access to the actual arguments
        /// defined on the lambda itself, and excludes the possible 0-th closure argument.
        /// </summary>
        internal void EmitLambdaArgument(int index) {
            _ilg.EmitLoadArg(GetLambdaArgument(index));
        }

        internal void EmitClosureArgument() {
            Debug.Assert(HasClosure, "must have a Closure argument");

            // TODO: this is for emitting into instance methods, but it's not
            // used anymore (we always emit into static MethodBuilders or
            // DynamicMethods, which are both static)
            _ilg.EmitLoadArg(_method.IsStatic ? 0 : 1);
        }

        internal MethodInfo CreateDelegateMethodInfo() {
            if (_method is DynamicMethod) {
                return (MethodInfo)_method;
            } else if (_method is MethodBuilder) {
                MethodBuilder mb = _method as MethodBuilder;
                Type methodType = _typeBuilder.CreateType();
                return methodType.GetMethod(mb.Name);
            } else {
                throw new InvalidOperationException();
            }
        }

        internal Delegate CreateDelegate(Type delegateType) {
            MethodInfo method;
            return CreateDelegate(delegateType, out method);
        }

        internal Delegate CreateDelegate(Type delegateType, out MethodInfo method) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            method = CreateDelegateMethodInfo();

            if (_boundConstants != null) {
                return method.CreateDelegate(delegateType, new Closure(_boundConstants.ToArray(), null));
            } else {
                return method.CreateDelegate(delegateType);
            }
        }

        /// <summary>
        /// Creates a compiler that shares the same characteristic as "this". If compiling into
        /// DynamicMethod (both fake or real), it will create compiler backed by dynamic method
        /// (also fake or real), if compiling into a type, it will create compiler linked to
        /// a new (static) method on the same type.
        /// </summary>
        private LambdaCompiler CreateLambdaCompiler(
            CompilerScope scope,
            string name,
            Type retType,
            IList<Type> paramTypes,
            string[] paramNames,
            bool closure) {

            LambdaCompiler lc;
            if (_dynamicMethod) {
                lc = CreateDynamicLambdaCompiler(scope, name, retType, paramTypes, paramNames, closure, _emitDebugSymbols, false);
            } else {
                lc = CreateStaticLambdaCompiler(scope, _typeBuilder, name, TypeUtils.PublicStatic, retType, paramTypes, paramNames, _dynamicMethod, closure, _emitDebugSymbols);
            }

            // TODO: better way to flow this in?
            lc._debugSymbolWriter = _debugSymbolWriter;

            return lc;
        }

        #region IL Debugging Support

        private static DebugILGen CreateDebugILGen(ILGenerator il, MethodBase method, IList<Type> paramTypes) {
            TextWriter txt = Console.Out;
#if !SILVERLIGHT
            if (DebugOptions.DumpIL) {
                txt = new StreamWriter(Snippets.Shared.GetMethodILDumpFile(method));
            }
#endif
            DebugILGen dig = new DebugILGen(il, txt);

            StringBuilder sb = new StringBuilder("\n\n");
            sb.Append(method.GetReturnType().FormatTypeName());
            sb.AppendFormat(" {0} {1} (\n", method.Name, method.Attributes);
            foreach (Type type in paramTypes) {
                sb.Append("\t");
                sb.Append(type.FormatTypeName());
                sb.Append("\n");
            }
            sb.Append(")\n");
            dig.WriteLine(sb.ToString());
            return dig;
        }

        #endregion

        #region Factory methods

        /// <summary>
        /// Creates a compiler backed by dynamic method. Sometimes (when debugging is required) the dynamic
        /// method is actually a 'fake' dynamic method and is backed by static type created specifically for
        /// the one method
        /// </summary>
        private static LambdaCompiler CreateDynamicLambdaCompiler(
            CompilerScope scope,
            string methodName,
            Type returnType,
            IList<Type> paramTypes,
            IList<string> paramNames,
            bool closure,
            bool emitDebugSymbols,
            bool forceDynamic) {

            Assert.NotEmpty(methodName);
            Assert.NotNull(returnType);
            Assert.NotNullItems(paramTypes);

            LambdaCompiler lc;

            //
            // Generate a static method if either
            // 1) we want to dump all geneated IL to an assembly on disk (SaveSnippets on)
            // 2) the method is debuggable, i.e. DebugMode is on and a source unit is associated with the method
            //
            if ((DebugOptions.SaveSnippets || emitDebugSymbols) && !forceDynamic) {
                var typeBuilder = Snippets.Shared.DefineType(methodName, typeof(object), false, false, emitDebugSymbols);
                lc = CreateStaticLambdaCompiler(
                    scope,
                    typeBuilder,
                    methodName,
                    TypeUtils.PublicStatic,
                    returnType,
                    paramTypes,
                    paramNames,
                    true, // dynamicMethod
                    closure,
                    emitDebugSymbols
                );
            } else {
                Type[] parameterTypes = MakeParameterTypeArray(paramTypes, true /*dynamicMethod*/, closure);
                DynamicMethod target = Snippets.Shared.CreateDynamicMethod(methodName, returnType, parameterTypes);
                lc = new LambdaCompiler(
                    scope,
                    null, // typeGen
                    target,
                    target.GetILGenerator(),
                    parameterTypes,
                    true, // dynamicMethod
                    false // emitDebugSymbols
                );
            }

            return lc;
        }

        /// <summary>
        /// Creates a LambdaCompiler backed by a method on a static type
        /// </summary>
        private static LambdaCompiler CreateStaticLambdaCompiler(
            CompilerScope scope,
            TypeBuilder typeBuilder,
            string name,
            MethodAttributes attributes,
            Type retType,
            IList<Type> paramTypes,
            IList<string> paramNames,
            bool dynamicMethod,
            bool closure,
            bool emitDebugSymbols) {

            Assert.NotNull(name, retType);

            Type[] parameterTypes = MakeParameterTypeArray(paramTypes, dynamicMethod, closure);

            MethodBuilder mb = typeBuilder.DefineMethod(name, attributes, retType, parameterTypes);
            LambdaCompiler lc = new LambdaCompiler(
                scope,
                typeBuilder,
                mb,
                mb.GetILGenerator(),
                parameterTypes,
                dynamicMethod,
                emitDebugSymbols
            );

            if (paramNames != null) {
                // parameters are index from 1, with closure argument we need to skip the first arg
                int startIndex = (dynamicMethod || closure) ? 2 : 1;
                for (int i = 0; i < paramNames.Count; i++) {
                    mb.DefineParameter(i + startIndex, ParameterAttributes.None, paramNames[i]);
                }
            }
            return lc;
        }

        #endregion
    }
}
