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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {

    // TODO: This should be a static class
    // TODO: simplify initialization logic & state
    public sealed class Snippets {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Snippets Shared = new Snippets();

        private int _methodNameIndex;

        private AssemblyGen _assembly;
        private AssemblyGen _debugAssembly;

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
            get { return _optionsFrozen ? _snippetsDirectory : DebugOptions.SnippetsDirectory; }
        }

        /// <summary>
        /// Name of the snippet assembly (w/o extension).
        /// </summary>
        public string SnippetsFileName {
            get { return _optionsFrozen ? _snippetsFileName : DebugOptions.SnippetsFileName; }
        }

        /// <summary>
        /// Save snippets to an assembly (see also SnippetsDirectory, SnippetsFileName).
        /// </summary>
        public bool SaveSnippets {
            get { return _optionsFrozen ? _saveSnippets : DebugOptions.SaveSnippets; }
        }

        private AssemblyGen GetAssembly(bool emitSymbols) {
            // reload options, the may have changed
            if (!_optionsFrozen) {
                _saveSnippets = DebugOptions.SaveSnippets;
                _snippetsDirectory = DebugOptions.SnippetsDirectory;
                _snippetsFileName = DebugOptions.SnippetsFileName;
                _optionsFrozen = true;
            }

            return (emitSymbols) ?
                GetOrCreateAssembly(emitSymbols,  ref _debugAssembly) :
                GetOrCreateAssembly(emitSymbols, ref _assembly);
        }

        private AssemblyGen GetOrCreateAssembly(bool emitSymbols, ref AssemblyGen assembly) {
            if (assembly == null) {
                string suffix = (emitSymbols) ? ".debug" : "";
                suffix += ".scripting";
                Interlocked.CompareExchange(ref assembly, CreateNewAssembly(suffix, emitSymbols), null);
            }
            return assembly;
        }

        private AssemblyGen CreateNewAssembly(string nameSuffix, bool emitSymbols) {
            string dir;

            if (_saveSnippets) {
                dir = _snippetsDirectory ?? Directory.GetCurrentDirectory();
            } else {
                dir = null;
            }

            string name = (_snippetsFileName ?? "Snippets") + nameSuffix;

            return new AssemblyGen(new AssemblyName(name), dir, ".dll", emitSymbols);
        }

        internal string GetMethodILDumpFile(MethodBase method) {
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

            // Invoke the core Snippets.Dump via reflection
            // Do this before verifying because our assemblies will depend on
            // the core ones
            Assembly core = typeof(System.Linq.Expressions.Expression).Assembly;
            Type snippets = core.GetType("System.Linq.Expressions.Compiler.Snippets");
            MethodInfo dump = snippets.GetMethod("Dump", BindingFlags.NonPublic | BindingFlags.Static);
            dump.Invoke(null, null);

            // then verify the verifiable ones:
            if (_assembly != null) {
                _assembly.Verify();
                _assembly = null;
            }

            if (_debugAssembly != null) {
                _debugAssembly.Verify();
                _debugAssembly = null;
            }

            _optionsFrozen = false;
        }

        public DynamicILGen CreateDynamicMethod(string methodName, Type returnType, Type[] parameterTypes, bool isDebuggable) {

            ContractUtils.RequiresNotEmpty(methodName, "methodName");
            ContractUtils.RequiresNotNull(returnType, "returnType");
            ContractUtils.RequiresNotNullItems(parameterTypes, "parameterTypes");

            if (DebugOptions.SaveSnippets) {
                AssemblyGen assembly = GetAssembly(isDebuggable);
                TypeBuilder tb = assembly.DefinePublicType(methodName, typeof(object), false);
                MethodBuilder mb = tb.DefineMethod(methodName, CompilerHelpers.PublicStatic, returnType, parameterTypes);
                return new DynamicILGenType(tb, mb, mb.GetILGenerator());
            } else {
                DynamicMethod dm = RawCreateDynamicMethod(methodName, returnType, parameterTypes);
                return new DynamicILGenMethod(dm, dm.GetILGenerator());
            }
        }

        public TypeBuilder DefinePublicType(string name, Type parent) {
            return GetAssembly(false).DefinePublicType(name, parent, false);
        }

        public TypeGen DefineType(string name, Type parent, bool preserveName, bool emitDebugSymbols) {
            AssemblyGen ag = GetAssembly(emitDebugSymbols);
            TypeBuilder tb = ag.DefinePublicType(name, parent, preserveName);
            return new TypeGen(ag, tb);
        }

        internal DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes) {
            string uniqueName = name + "##" + Interlocked.Increment(ref _methodNameIndex);
            return RawCreateDynamicMethod(uniqueName, returnType, parameterTypes);
        }

        internal TypeBuilder DefineDelegateType(string name) {
            AssemblyGen assembly = GetAssembly(false);
            return assembly.DefineType(
                name,
                typeof(MulticastDelegate),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                false
            );
        }

        private static DynamicMethod RawCreateDynamicMethod(string name, Type returnType, Type[] parameterTypes) {
#if SILVERLIGHT // Module-hosted DynamicMethod is not available in SILVERLIGHT
            return new DynamicMethod(name, returnType, parameterTypes);
#else
            //
            // WARNING: we set restrictedSkipVisibility == true  (last parameter)
            //          setting this bit will allow accessing nonpublic members
            //          for more information see http://msdn.microsoft.com/en-us/library/bb348332.aspx
            //
            return new DynamicMethod(name, returnType, parameterTypes, true);
#endif
        }
    }
}
