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
        private bool _ilDebug;
        
        /// <summary>
        /// Directory where snippet assembly will be saved if SaveSnippets is set.
        /// </summary>
        public string SnippetsDirectory {
            get { return _snippetsDirectory; }
            set {
                ContractUtils.Requires(!_optionsFrozen);
                _snippetsDirectory = value;
            }
        }

        /// <summary>
        /// Name of the snippet assembly (w/o extension).
        /// </summary>
        public string SnippetsFileName {
            get { return _snippetsFileName; }
            set {
                ContractUtils.Requires(!_optionsFrozen);
                _snippetsFileName = value;
            }
        }

        /// <summary>
        /// Save snippets to an assembly (see also SnippetsDirectory, SnippetsFileName).
        /// </summary>
        public bool SaveSnippets {
            get { return _saveSnippets; }
            set {
                ContractUtils.Requires(!_optionsFrozen);
                _saveSnippets = value; 
            }
        }

        /// <summary>
        /// Write IL to a text file as it is generated.
        /// This flag can be changed any time.
        /// </summary>
        public bool ILDebug {
            get { return _ilDebug; }
            set { _ilDebug = value; }
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

        internal string/*!*/ GetMethodILDumpFile(MethodBase/*!*/ method) {
            string fullName = ((method.DeclaringType != null) ? method.DeclaringType.Name + "." : "") + method.Name;

            if (fullName.Length > 100) {
                fullName = fullName.Substring(0, 100);
            }

            string filename = String.Format("{0}_{1}.il", IOUtils.ToValidFileName(fullName), Interlocked.Increment(ref _methodNameIndex));

            string dir = _snippetsDirectory ?? Path.Combine(Path.GetTempPath(), "__DLRIL");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, filename);
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

            _unsafeDebugAssembly = null;
            _optionsFrozen = false;
        }

        public DynamicILGen/*!*/ CreateDynamicMethod(string/*!*/ methodName, Type/*!*/ returnType, Type/*!*/[]/*!*/ parameterTypes,
            bool isDebuggable) {

            ContractUtils.RequiresNotEmpty(methodName, "methodName");
            ContractUtils.RequiresNotNull(returnType, "returnType");
            ContractUtils.RequiresNotNullItems(parameterTypes, "parameterTypes");

            AssemblyGen assembly = GetAssembly(isDebuggable, false);

            if (_saveSnippets) {
                TypeBuilder tb = assembly.DefinePublicType(methodName, typeof(object), false);
                MethodBuilder mb = tb.DefineMethod(methodName, CompilerHelpers.PublicStatic, returnType, parameterTypes);
                return new DynamicILGenType(tb, mb, mb.GetILGenerator());
            } else {
                DynamicMethod dm = ReflectionUtils.CreateDynamicMethod(methodName, returnType, parameterTypes, assembly.ModuleBuilder);
                return new DynamicILGenMethod(dm, dm.GetILGenerator());
            }
        }

        public TypeBuilder/*!*/ DefinePublicType(string  name, Type/*!*/ parent) {
            return GetAssembly(false, false).DefinePublicType(name, parent, false);
        }

        internal TypeGen/*!*/ DefineUnsafeType(string/*!*/ name, Type/*!*/ parent) {
            return DefineType(name, parent, false, null, true);
        }

        internal TypeGen/*!*/ DefineType(string/*!*/ name, Type/*!*/ parent, bool preserveName, SourceUnit source, bool isUnsafe) {
            bool debugMode = source != null && source.LanguageContext.DomainManager.GlobalOptions.DebugMode;
            bool emitSymbols = debugMode && source.HasPath;

            AssemblyGen ag = GetAssembly(emitSymbols, isUnsafe);
            TypeBuilder tb = ag.DefinePublicType(name, parent, preserveName);
            return new TypeGen(ag, tb);
        }

        internal DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes) {
            // We don't care which assembly we get, all we need is to tag the dynamic method on it
            // and even that only sometimes ...
            AssemblyGen assembly = GetAssembly(false, false);
            string uniqueName = name + "##" + Interlocked.Increment(ref _methodNameIndex);

            return ReflectionUtils.CreateDynamicMethod(uniqueName, returnType, parameterTypes, assembly.ModuleBuilder);
        }
    }
}
