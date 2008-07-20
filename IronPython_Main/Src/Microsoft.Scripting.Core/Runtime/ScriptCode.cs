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

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using System.Threading;
using Microsoft.Contracts;

namespace System.Scripting {
    /// <summary>
    /// ScriptCode is an instance of compiled code that is bound to a specific LanguageContext
    /// but not a specific ScriptScope. The code can be re-executed multiple times in different
    /// scopes. Hosting API counterpart for this class is <c>CompiledCode</c>.
    /// </summary>
    public class ScriptCode {
        private readonly LambdaExpression _code;
        private readonly SourceUnit _sourceUnit;
        private DlrMainCallTarget _target;

        public ScriptCode(LambdaExpression code, SourceUnit sourceUnit)
            : this(code, null, sourceUnit) {
        }

        public ScriptCode(LambdaExpression code, DlrMainCallTarget target, SourceUnit sourceUnit) {
            if (code == null && target == null) {
                throw Error.MustHaveCodeOrTarget();
            }

            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");

            _code = code;
            _sourceUnit = sourceUnit;
            _target = target;
        }

        public LanguageContext LanguageContext {
            get { return _sourceUnit.LanguageContext; }
        }

        public DlrMainCallTarget Target {
            get { return _target; }
        }

        public SourceUnit SourceUnit {
            get { return _sourceUnit; }
        }

        public LambdaExpression Code {
            get { return _code; }
        }

        public virtual Scope CreateScope() {
            return new Scope();
        }

        public virtual void EnsureCompiled() {
            EnsureTarget(_code);
        }

        public object Run(Scope scope) {
            return InvokeTarget(_code, scope);
        }

        public object Run() {
            return Run(CreateScope());
        }

        protected virtual object InvokeTarget(LambdaExpression code, Scope scope) {
            return EnsureTarget(code)(scope, LanguageContext);
        }

        private DlrMainCallTarget EnsureTarget(LambdaExpression code) {
            if (_target == null) {
                Interlocked.CompareExchange(ref _target, Compile<DlrMainCallTarget>(code, SourceUnit.EmitDebugSymbols), null);
            }
            return _target;
        }

        internal void CompileToDisk(TypeGen typeGen) {
            if (_code == null) {
                throw Error.NoCodeToCompile();
            }

            MethodBuilder mb = typeGen.TypeBuilder.DefineMethod(
                SourceUnit.Path,
                CompilerHelpers.PublicStatic | MethodAttributes.SpecialName,
                typeof(object),
                new Type[] { typeof(Scope), typeof(LanguageContext) }
            );

            CustomAttributeBuilder cab = new CustomAttributeBuilder(
                typeof(DlrCachedCodeAttribute).GetConstructor(new Type[] { typeof(Type) }),
                new object[] { LanguageContext.GetType() }
            );
            mb.SetCustomAttribute(cab);

            LambdaExpression lambda = PrepareCodeForSave(mb);
            LambdaCompiler.CompileLambda(lambda, typeGen, mb, _sourceUnit.EmitDebugSymbols);
        }

        public static ScriptCode Load(MethodInfo method, LanguageContext language) {
            SourceUnit su = new SourceUnit(language, NullTextContentProvider.Null, method.Name, SourceCodeKind.File);
            return new ScriptCode(null, (DlrMainCallTarget)Delegate.CreateDelegate(typeof(DlrMainCallTarget), method), su);
        }

        protected virtual LambdaExpression PrepareCodeForSave(MethodBuilder builder) {
            return new ToDiskRewriter().RewriteLambda(_code);
        }

        /// <summary>
        /// This takes an assembly name including extension and saves the provided ScriptCode objects into the assembly.  
        /// 
        /// The provided script codes can constitute code from multiple languages.  The assemblyName can be either a fully qualified 
        /// or a relative path.  The DLR will simply save the assembly to the desired location.  The assembly is created by the DLR and 
        /// if a file already exists than an exception is raised.  
        /// 
        /// The DLR determines the internal format of the ScriptCode and the DLR can feel free to rev this as appropriate.  
        /// </summary>
        public static void SaveToAssembly(string assemblyName, params ScriptCode[] codes) {
            ContractUtils.RequiresNotNull(assemblyName, "assemblyName");
            ContractUtils.RequiresNotNullItems(codes, "codes");

            // break the assemblyName into it's dir/name/extension
            string dir = Path.GetDirectoryName(assemblyName);
            if (String.IsNullOrEmpty(dir)) {
                dir = Environment.CurrentDirectory;
            }

            string name = Path.GetFileNameWithoutExtension(assemblyName);
            string ext = Path.GetExtension(assemblyName);

            // build the assembly & type gen that all the script codes will live in...
            AssemblyGen ag = new AssemblyGen(new AssemblyName(name), dir, ext, /*emitSymbols*/false);
            TypeBuilder tb = ag.DefinePublicType("DLRCachedCode", typeof(object), true);
            TypeGen tg = new TypeGen(ag, tb);

            // then compile all of the code
            foreach (ScriptCode sc in codes) {
                sc.CompileToDisk(tg);
            }

            tg.FinishType();
            ag.Dump();
        }

        /// <summary>
        /// This will take an assembly object which the user has loaded and return a new set of ScriptCode’s which have 
        /// been loaded into the provided ScriptDomainManager.  
        /// 
        /// If the language associated with the ScriptCode’s has not already been loaded the DLR will load the 
        /// LanguageContext into the ScriptDomainManager based upon the saved LanguageContext type.  
        /// 
        /// If the LanguageContext or the version of the DLR the language was compiled against is unavailable a 
        /// TypeLoadException will be raised unless policy has been applied by the administrator to redirect bindings.
        /// </summary>
        public static ScriptCode[] LoadFromAssembly(ScriptDomainManager runtime, Assembly assembly) {
            ContractUtils.RequiresNotNull(runtime, "runtime");
            ContractUtils.RequiresNotNull(assembly, "assembly");

            // get the type which has our cached code...
            Type t = assembly.GetType("DLRCachedCode");
            if (t == null) {
                return new ScriptCode[0];
            }

            List<ScriptCode> codes = new List<ScriptCode>();

            // look for methods which are associated with a saved ScriptCode...
            foreach (MethodInfo mi in t.GetMethods()) {
                // we mark the methods as special name when we generate them because the 
                // method name implies the filename.
                if (!mi.IsSpecialName) {
                    continue;
                }

                // we also put an attribute which contains additional information
                object[] attrs = mi.GetCustomAttributes(typeof(DlrCachedCodeAttribute), false);
                if (attrs.Length != 1) {
                    continue;
                }

                DlrCachedCodeAttribute code = (DlrCachedCodeAttribute)attrs[0];

                LanguageContext lc = runtime.GetLanguage(code.LanguageContextType);
                ScriptCode sc = lc.LoadCompiledCode(mi);
                codes.Add(sc);
            }

            return codes.ToArray();
        }

        [Confined]
        public override string ToString() {
            return String.Format("ScriptCode '{0}' from {1}", SourceUnit.Path, LanguageContext.DisplayName);
        }

        public static T Compile<T>(LambdaExpression code, bool emitDebugSymbols) {
            ContractUtils.RequiresNotNull(code, "code");
            return LambdaCompiler.CompileLambda<T>(code, emitDebugSymbols);
        }
    }
}
