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
using System.IO;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.Contracts;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Diagnostics;

namespace Microsoft.Scripting {
    /// <summary>
    /// ScriptCode is an instance of compiled code that is bound to a specific LanguageContext
    /// but not a specific ScriptScope. The code can be re-executed multiple times in different
    /// scopes. Hosting API counterpart for this class is <c>CompiledCode</c>.
    /// </summary>
    public abstract class ScriptCode {
        private readonly SourceUnit _sourceUnit;

        public ScriptCode(SourceUnit sourceUnit) {
            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");

            _sourceUnit = sourceUnit;
        }

        public LanguageContext LanguageContext {
            get { return _sourceUnit.LanguageContext; }
        }

        public SourceUnit SourceUnit {
            get { return _sourceUnit; }
        }

        public virtual Scope CreateScope() {
            return new Scope();
        }

        public abstract object Run(Scope scope);
        public abstract object Run();

        class CodeInfo {
            public readonly MethodBuilder Builder;
            public readonly ScriptCode Code;
            public readonly Type DelegateType;

            public CodeInfo(MethodBuilder builder, ScriptCode code, Type delegateType) {
                Builder = builder;
                Code = code;
                DelegateType = delegateType;
            }
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
            var symbolDict = new Dictionary<SymbolId, FieldBuilder>();
            // then compile all of the code

            Dictionary<Type, List<CodeInfo>> langCtxBuilders = new Dictionary<Type, List<CodeInfo>>();
            foreach (ScriptCode sc in codes) {
                List<CodeInfo> builders;
                if (!langCtxBuilders.TryGetValue(sc.LanguageContext.GetType(), out builders)) {
                    langCtxBuilders[sc.LanguageContext.GetType()] = builders = new List<CodeInfo>();
                }

                KeyValuePair<MethodBuilder, Type> compInfo = sc.CompileForSave(tg, symbolDict);

                builders.Add(new CodeInfo(compInfo.Key, sc, compInfo.Value));
            }

            MethodBuilder mb = tb.DefineMethod(
                "GetScriptCodeInfo",
                MethodAttributes.SpecialName | MethodAttributes.Public | MethodAttributes.Static,
                typeof(MutableTuple<Type[], Delegate[][], string[][], object>),
                Type.EmptyTypes);

            ILGen ilgen = new ILGen(mb.GetILGenerator());

            var langsWithBuilders = langCtxBuilders.ToArray();

            // lang ctx array
            ilgen.EmitArray(typeof(Type), langsWithBuilders.Length, (index) => {
                ilgen.Emit(OpCodes.Ldtoken, langsWithBuilders[index].Key);
                ilgen.EmitCall(typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
            });

            // builders array of array
            ilgen.EmitArray(typeof(Delegate[]), langsWithBuilders.Length, (index) => {
                List<CodeInfo> builders = langsWithBuilders[index].Value;

                ilgen.EmitArray(typeof(Delegate), builders.Count, (innerIndex) => {
                    ilgen.EmitNull();
                    ilgen.Emit(OpCodes.Ldftn, builders[innerIndex].Builder);
                    ilgen.EmitNew(
                        builders[innerIndex].DelegateType,
                        new[] { typeof(object), typeof(IntPtr) }
                    );
                });
            });

            // paths array of array
            ilgen.EmitArray(typeof(string[]), langsWithBuilders.Length, (index) => {
                List<CodeInfo> builders = langsWithBuilders[index].Value;

                ilgen.EmitArray(typeof(string), builders.Count, (innerIndex) => {
                    ilgen.EmitString(builders[innerIndex].Code._sourceUnit.Path);
                });
            });

            // 4th element in tuple - always null...
            ilgen.EmitNull();

            ilgen.EmitNew(
                typeof(MutableTuple<Type[], Delegate[][], string[][], object>),
                new[] { typeof(Type[]), typeof(Delegate[][]), typeof(string[][]), typeof(object) }
            );
            ilgen.Emit(OpCodes.Ret);

            mb.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(DlrCachedCodeAttribute).GetConstructor(Type.EmptyTypes),
                ArrayUtils.EmptyObjects
            ));

            tg.FinishType();
            ag.SaveAssembly();
        }

        /// <summary>
        /// This will take an assembly object which the user has loaded and return a new set of ScriptCode�s which have 
        /// been loaded into the provided ScriptDomainManager.  
        /// 
        /// If the language associated with the ScriptCode�s has not already been loaded the DLR will load the 
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

            MethodInfo mi = t.GetMethod("GetScriptCodeInfo");
            if (mi.IsSpecialName && mi.IsDefined(typeof(DlrCachedCodeAttribute), false)) {
                var infos = (MutableTuple<Type[], Delegate[][], string[][], object>)mi.Invoke(null, ArrayUtils.EmptyObjects);

                for (int i = 0; i < infos.Item000.Length; i++) {
                    Type curType = infos.Item000[i];
                    LanguageContext lc = runtime.GetLanguage(curType);

                    Debug.Assert(infos.Item001[i].Length == infos.Item002[i].Length);

                    Delegate[] methods = infos.Item001[i];
                    string[] names = infos.Item002[i];

                    for (int j = 0; j < methods.Length; j++) {
                        codes.Add(lc.LoadCompiledCode(methods[j], names[j]));
                    }
                }
            }

            return codes.ToArray();
        }

        protected LambdaExpression RewriteForSave(TypeGen typeGen, LambdaExpression code) {
            var diskRewriter = new ToDiskRewriter(typeGen);
            return diskRewriter.RewriteLambda(code);
        }

        protected virtual KeyValuePair<MethodBuilder, Type> CompileForSave(TypeGen typeGen, Dictionary<SymbolId, FieldBuilder> symbolDict) {
            throw new NotSupportedException();
        }

        [Confined]
        public override string ToString() {
            return String.Format("ScriptCode '{0}' from {1}", SourceUnit.Path, LanguageContext.GetType().Name);
        }
    }
}
