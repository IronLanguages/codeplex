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

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Ast;
using System.Reflection.Emit;
using System.IO;

namespace Microsoft.Scripting.Generation {

    public sealed class Snippets {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Snippets/*!*/ Shared = new Snippets();

        private int _methodNameIndex;

        private AssemblyGen _assembly;
        private AssemblyGen _unsafeAssembly;
        private AssemblyGen _debugAssembly;
        private AssemblyGen _unsafeDebugAssembly;

        // once an option is used no changes to the options are allowed any more:
        private bool _optionsFrozen;

        // TODO: options should be internal
        private string _snippetsDirectory;
        private string _snippetsFileName;
        private bool _saveSnippets;
        
        /// <summary>
        /// Directory where snippet assembly will be saved if SaveSnippets is set.
        /// </summary>
        public string SnippetsDirectory {
            get { return _snippetsDirectory; }
            set {
                Contract.Requires(!_optionsFrozen);
                _snippetsDirectory = value;
            }
        }

        /// <summary>
        /// Name of the snippet assembly (w/o extension).
        /// </summary>
        public string SnippetsFileName {
            get { return _snippetsFileName; }
            set {
                Contract.Requires(!_optionsFrozen);
                _snippetsFileName = value;
            }
        }

        /// <summary>
        /// Save snippets to an assembly (see also SnippetsDirectory, SnippetsFileName).
        /// </summary>
        public bool SaveSnippets {
            get { return _saveSnippets; }
            set {
                Contract.Requires(!_optionsFrozen);
                _saveSnippets = value; 
            }
        }

        private Snippets() {
        }

        private AssemblyGen/*!*/ GetAssembly(bool emitSymbols, bool isUnsafe) {
            _optionsFrozen = true;

            // If snippetst are not to be saved, we can merge unsafe and safe IL.
            
            if (isUnsafe && _saveSnippets) {
                return (emitSymbols) ? 
                    GetOrCreateAssembly(emitSymbols, isUnsafe, ref _unsafeDebugAssembly) :
                    GetOrCreateAssembly(emitSymbols, isUnsafe, ref _unsafeAssembly);
            } else {
                return (emitSymbols) ? 
                    GetOrCreateAssembly(emitSymbols, isUnsafe, ref _debugAssembly) :
                    GetOrCreateAssembly(emitSymbols, isUnsafe, ref _assembly);
            }
        }

        private AssemblyGen/*!*/ GetOrCreateAssembly(bool emitSymbols, bool isUnsafe, ref AssemblyGen assembly) {
            if (assembly == null) {
                string suffix = (emitSymbols) ? ".debug" : "" + (isUnsafe ? ".unsafe" : "");
                Interlocked.CompareExchange(ref assembly, CreateNewAssembly(suffix, emitSymbols), null);
            }
            return assembly;
        }

        private AssemblyGen CreateNewAssembly(string/*!*/ nameSuffix, bool emitSymbols) {
            string dir;

            if (_saveSnippets) {
                dir = _snippetsDirectory ?? Directory.GetCurrentDirectory();
            } else {
                dir = null;
            }

            string name = (_snippetsFileName ?? "Snippets") + nameSuffix;

            return new AssemblyGen(new AssemblyName(name), dir, ".dll", emitSymbols);
        }

        public void Dump() {
            if (!_saveSnippets) {
                return;
            }

            // first save all assemblies to disk:
            if (_assembly != null) {
                _assembly.Dump();
            }

            if (_debugAssembly != null) {
                _debugAssembly.Dump();
            }

            if (_unsafeAssembly != null) {
                _unsafeAssembly.Dump();
            }

            if (_unsafeDebugAssembly != null) {
                _unsafeDebugAssembly.Dump();
            }

            // then verify the verifiable ones:
            if (_assembly != null) {
                _assembly.Verify();
                _assembly = null;
            }

            if (_debugAssembly != null) {
                _debugAssembly.Verify();
                _debugAssembly = null;
            }
        }

        public DynamicILGen/*!*/ CreateDynamicMethod(string/*!*/ methodName, Type/*!*/ returnType, Type/*!*/[]/*!*/ parameterTypes,
            bool isDebuggable) {

            Contract.RequiresNotEmpty(methodName, "methodName");
            Contract.RequiresNotNull(returnType, "returnType");
            Contract.RequiresNotNullItems(parameterTypes, "parameterTypes");

            AssemblyGen assembly = GetAssembly(isDebuggable, false);

            if (_saveSnippets) {
                TypeGen tg = assembly.DefinePublicType(methodName, typeof(object), false);
                TypeBuilder tb = tg.TypeBuilder;
                MethodBuilder mb = tb.DefineMethod(methodName, CompilerHelpers.PublicStatic, returnType, parameterTypes);
                return new DynamicILGenType(tb, mb, mb.GetILGenerator());
            } else {
                DynamicMethod dm = ReflectionUtils.CreateDynamicMethod(methodName, returnType, parameterTypes, assembly.ModuleBuilder);
                return new DynamicILGenMethod(dm, dm.GetILGenerator());
            }
        }

        internal Compiler/*!*/ DefineMethod(string/*!*/ methodName, Type/*!*/ returnType, IList<Type/*!*/>/*!*/ paramTypes, ConstantPool constantPool) {
            return DefineMethod(methodName, returnType, paramTypes, null, constantPool, null, false);
        }

        internal Compiler/*!*/ DefineUnsafeMethod(string/*!*/ methodName, Type/*!*/ returnType, IList<Type/*!*/>/*!*/ paramTypes,
            IList<string> paramNames, ConstantPool constantPool) {
            return DefineMethod(methodName, returnType, paramTypes, paramNames, constantPool, null, true);
        }

        internal Compiler/*!*/ DefineMethod(string/*!*/ methodName, Type/*!*/ returnType, IList<Type/*!*/>/*!*/ paramTypes,
            IList<string> paramNames, ConstantPool constantPool, SourceUnit source) {
            return DefineMethod(methodName, returnType, paramTypes, paramNames, constantPool, source, false);
        }

        private Compiler/*!*/ DefineMethod(string/*!*/ methodName, Type/*!*/ returnType, IList<Type/*!*/>/*!*/ paramTypes,
            IList<string> paramNames, ConstantPool constantPool, SourceUnit source, bool isUnsafe) {

            Assert.NotEmpty(methodName);
            Assert.NotNull(returnType);
            Assert.NotNullItems(paramTypes);
            // TODO: Debug.Assert(methodName.IndexOf("##") == -1);

            Compiler result;
            bool debugMode = source != null && source.LanguageContext.DomainManager.GlobalOptions.DebugMode;
            bool emitSymbols = debugMode && source.HasPath;
            AssemblyGen assembly = GetAssembly(emitSymbols, isUnsafe);

            //
            // Generate a static method if either
            // 1) we want to dump all geneated IL to an assembly on disk (SaveSnippets on)
            // 2) the method is debuggable, i.e. DebugMode is on and a source unit is associated with the method
            //
            if (_saveSnippets || emitSymbols) {
                TypeGen typeGen = assembly.DefinePublicType(methodName, typeof(object), false);

                result = typeGen.DefineMethod(methodName, returnType, paramTypes, paramNames, constantPool);
                
                // emit symbols iff we have a source unit (and are in debug mode, see GenerateStaticMethod):
                result.SetDebugSymbols(source, emitSymbols);
            } else {
                string uniqueName = methodName + "##" + Interlocked.Increment(ref _methodNameIndex);

                Type[] parameterTypes = CompilerHelpers.MakeParamTypeArray(paramTypes, constantPool);
                DynamicMethod target = ReflectionUtils.CreateDynamicMethod(uniqueName, returnType, parameterTypes, assembly.ModuleBuilder);
                result = new Compiler(null, assembly, target, target.GetILGenerator(), parameterTypes, constantPool);
                
                // emits line number setting instructions if source unit available:
                if (debugMode) {
                    result.SetDebugSymbols(source, false);
                    result.EmitLineInfo = true; // TODO: ??
                }
            }

            // do not allocate constants to static fields:
            result.CacheConstants = false;

            // this is a dynamic method:
            result.DynamicMethod = true;

            return result;
        }

        public TypeGen/*!*/ DefineUnsafeType(string/*!*/ name, Type/*!*/ parent) {
            return DefineType(name, parent, false, null, true);
        }

        public TypeGen/*!*/ DefineType(string/*!*/ name, Type/*!*/ parent) {
            return DefineType(name, parent, false, null, false);
        }

        public TypeGen/*!*/ DefineType(string/*!*/ name, Type/*!*/ parent, bool preserveName, SourceUnit source, bool isUnsafe) {
            bool debugMode = source != null && source.LanguageContext.DomainManager.GlobalOptions.DebugMode;
            bool emitSymbols = debugMode && source.HasPath;

            return GetAssembly(emitSymbols, isUnsafe).DefinePublicType(name, parent, preserveName);
        }

    }
}
