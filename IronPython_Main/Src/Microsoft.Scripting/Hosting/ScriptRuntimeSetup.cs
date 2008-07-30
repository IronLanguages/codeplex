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
using System.Diagnostics;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    [Serializable]
    public sealed class ScriptRuntimeSetup {
        // host specification:
        private Type _hostType;
        private object[] _hostArguments;

        // languages available in the runtime: 
        private readonly Dictionary<AssemblyQualifiedTypeName, LanguageSetup> _languageSetups;

        // DLR options:
        private bool _debugMode;
        private bool _privateBinding;

        public Dictionary<AssemblyQualifiedTypeName, LanguageSetup> LanguageSetups {
            get { return _languageSetups; }
        }

        public bool DebugMode {
            get { return _debugMode; }
            set { _debugMode = value; }
        }

        public bool PrivateBinding {
            get { return _privateBinding; }
            set { _privateBinding = value; }
        }

        public Type HostType {
            get { return _hostType; }
            set { _hostType = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public object[] HostArguments {
            get {
                return _hostArguments;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _hostArguments = value;
            }
        }

        public ScriptRuntimeSetup() 
            : this(false) {
        }

#if SIGNED
        private const string IronPythonAssembly = "IronPython, Version=2.0.0.3000, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        private const string JScriptAssembly = "Microsoft.JScript.Runtime, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        private const string IronRubyAssembly = "IronRuby, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        private const string VisualBasicAssembly = "Microsoft.VisualBasic.Scripting, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        private const string ToyScriptAssembly = "ToyScript, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
#else
        private const string IronPythonAssembly = "IronPython";
        private const string JScriptAssembly = "Microsoft.JScript.Runtime";
        private const string IronRubyAssembly = "IronRuby";
        private const string VisualBasicAssembly = "Microsoft.VisualBasic.Scripting";
        private const string ToyScriptAssembly = "ToyScript";
#endif

        public ScriptRuntimeSetup(bool addWellKnownLanguages) {
            _languageSetups = new Dictionary<AssemblyQualifiedTypeName, LanguageSetup>();

            if (addWellKnownLanguages) {
                AddLanguage("IronPython.Runtime.PythonContext", IronPythonAssembly, 
                    "IronPython",
                    new[] { "py", "python", "ironpython" },
                    new[] { ".py" } 
                );

                AddLanguage("Microsoft.JScript.Runtime.JSContext", JScriptAssembly,
                    "Managed JScript",
                    new[] { "managedjscript", "js", "jscript" },
                    new[] { ".jsx", ".js" }
                );

                AddLanguage("Ruby.Runtime.RubyContext", IronRubyAssembly, 
                    "IronRuby",
                    new[] { "rb", "ruby", "ironruby" },
                    new[] { ".rb" }
                );

                AddLanguage("Microsoft.VisualBasic.Scripting.Runtime.VisualBasicLanguageContext", VisualBasicAssembly, 
                    "Visual Basic",
                    new[] { "vbx" },
                    new[] { ".vbx" } 
                );

                AddLanguage("ToyScript.ToyLanguageContext", ToyScriptAssembly, 
                    "ToyScript",
                    new[] { "ts", "toyscript" },
                    new[] { ".ts" } 
                );
            }

            _hostType = typeof(ScriptHost);
            _hostArguments = ArrayUtils.EmptyObjects;
        }

        private void AddLanguage(string typeName, string assemblyName, string displayName, string[] ids, string[] extensions) {
            _languageSetups.Add(new AssemblyQualifiedTypeName(typeName, assemblyName), new LanguageSetup(displayName, ids, extensions));
        }

        public LanguageSetup GetLanguageSetup<TLanguage>() {
            LanguageSetup setup;
            if (!_languageSetups.TryGetValue(new AssemblyQualifiedTypeName(typeof(TLanguage)), out setup)) {
                throw new ArgumentException("Language not registered");
            }
            return setup;
        }

        public bool TryGetLanguageProviderByExtension(string extension, out AssemblyQualifiedTypeName provider) {
            if (!extension.StartsWith(".")) {
                extension = "." + extension;
            }

            foreach (var entry in _languageSetups) {
                if (IndexOf(entry.Value.FileExtensions, extension, StringComparer.OrdinalIgnoreCase) != -1) {
                    provider = entry.Key;
                    return true;
                }
            }

            provider = default(AssemblyQualifiedTypeName);
            return false;
        }

        public bool TryGetLanguageProviderById(string id, out AssemblyQualifiedTypeName provider) {
            foreach (var entry in _languageSetups) {
                if (IndexOf(entry.Value.Ids, id, StringComparer.OrdinalIgnoreCase) != -1) {
                    provider = entry.Key;
                    return true;
                }
            }
            provider = default(AssemblyQualifiedTypeName);
            return false;
        }

        public AssemblyQualifiedTypeName GetLanguageProviderById(string id) {
            AssemblyQualifiedTypeName provider;
            if (!TryGetLanguageProviderById(id, out provider)) {
                throw new ArgumentException("Unknown language identifier");
            }
            return provider;
        }

        public AssemblyQualifiedTypeName GetLanguageProviderByExtension(string extension) {
            AssemblyQualifiedTypeName provider;
            if (!TryGetLanguageProviderByExtension(extension, out provider)) {
                throw new ArgumentException("Unknown language extension");
            }
            return provider;
        }

        private static int IndexOf(IList<string> list, string value, StringComparer comparer) {
            for (int i = 0; i < list.Count; i++) {
                if (comparer.Compare(list[i], value) == 0) {
                    return i;
                }
            }
            return -1;
        }

        internal DlrConfiguration ToConfiguration() {
            var config = new DlrConfiguration(
                _debugMode,
                _privateBinding
            );

            foreach (var entry in _languageSetups) {
                config.AddLanguage(
                    entry.Key, 
                    entry.Value.Ids, 
                    entry.Value.FileExtensions, 
                    entry.Value.HasOptions ? entry.Value.Options : null
                );
            }

            return config;
        }
    }
}
