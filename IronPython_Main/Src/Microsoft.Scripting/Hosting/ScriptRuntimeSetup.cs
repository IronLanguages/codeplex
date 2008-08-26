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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Reflection;
using System.IO;

namespace Microsoft.Scripting.Hosting {
    [Serializable]
    public sealed class ScriptRuntimeSetup {
        // host specification:
        private Type _hostType;
        private object[] _hostArguments;

        // languages available in the runtime: 
        private readonly Dictionary<string, LanguageSetup> _languageSetups;

        // DLR options:
        private bool _debugMode;
        private bool _privateBinding;

        // common language options:
        private IDictionary<string, object> _options;

        public ScriptRuntimeSetup() {
            _languageSetups = new Dictionary<string, LanguageSetup>();
            _hostType = typeof(ScriptHost);
            _hostArguments = ArrayUtils.EmptyObjects;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "addWellKnownLanguages")]
        [Obsolete(@"Use ScriptRuntimeSetup() overload and fill it in manually or ScriptRuntimeSetup.ReadConfiguration() factory load setup from .config files.", true)]
        public ScriptRuntimeSetup(bool addWellKnownLanguages) {
        }

        public Dictionary<string, LanguageSetup> LanguageSetups {
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

        /// <remarks>
        /// Option names are case-sensitive.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IDictionary<string, object> Options {
            get {
                if (_options == null) {
                    _options = new Dictionary<string, object>();
                }
                return _options;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _options = value;
            }
        }

        public bool HasOptions {
            get { return _options != null; }
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

        public bool TryGetLanguageProviderByFileExtension(string fileExtension, out string providerAssemblyQualifiedTypeName) {
            if (!fileExtension.StartsWith(".")) {
                fileExtension = "." + fileExtension;
            }

            foreach (var entry in _languageSetups) {
                if (IndexOf(entry.Value.FileExtensions, fileExtension, DlrConfiguration.FileExtensionComparer) != -1) {
                    providerAssemblyQualifiedTypeName = entry.Key;
                    return true;
                }
            }

            providerAssemblyQualifiedTypeName = null;
            return false;
        }

        public string GetLanguageProviderByFileExtension(string fileExtension) {
            string provider;
            if (!TryGetLanguageProviderByFileExtension(fileExtension, out provider)) {
                throw new ArgumentException(String.Format("Unknown language extension: '{0}'", fileExtension));
            }
            return provider;
        }

        public bool TryGetLanguageProviderByName(string languageName, out string providerAssemblyQualifiedTypeName) {
            foreach (var entry in _languageSetups) {
                if (IndexOf(entry.Value.Names, languageName, DlrConfiguration.LanguageNameComparer) != -1) {
                    providerAssemblyQualifiedTypeName = entry.Key;
                    return true;
                }
            }
            providerAssemblyQualifiedTypeName = null;
            return false;
        }

        public string GetLanguageProviderByName(string languageName) {
            string provider;
            if (!TryGetLanguageProviderByName(languageName, out provider)) {
                throw new ArgumentException(String.Format("Unknown language name: '{0}'", languageName));
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

                Dictionary<string, object> options;
                if (HasOptions || entry.Value.HasOptions) {
                    // add global language options first, they can be rewritten by language specific ones:
                    if (HasOptions) {
                        options = new Dictionary<string, object>(_options);
                    } else {
                        options = new Dictionary<string, object>();
                    }

                    // add only those global options that are not yet set in language options:
                    if (entry.Value.HasOptions) {
                        foreach (var option in entry.Value.Options) {
                            options[option.Key] = option.Value;
                        }
                    }
                } else {
                    options = null;
                }

                config.AddLanguage(
                    entry.Key, 
                    entry.Value.Names, 
                    entry.Value.FileExtensions, 
                    options
                );
            }

            return config;
        }

        /// <summary>
        /// Loads setup from .NET configuration (.config files).
        /// If there is no configuration available returns an empty setup.
        /// </summary>
        public static ScriptRuntimeSetup ReadConfiguration() {
#if SILVERLIGHT
            return new ScriptRuntimeSetup();
#else
            return new ScriptRuntimeSetup().LoadConfiguration();
#endif
        }

#if !SILVERLIGHT
        /// <summary>
        /// Loads settings from .NET configuration (.config files) and adds them to the setup object.
        /// </summary>
        /// <returns>This instance.</returns>
        public ScriptRuntimeSetup LoadConfiguration() {
            Configuration.Section.LoadRuntimeSetup(this, null);
            return this;
        }

        /// <summary>
        /// Loads settings from a specified XML stream and adds them to the specified setup object
        /// Returns <paramref name="baseSetup"/>.
        /// </summary>
        /// <returns>This instance.</returns>
        public ScriptRuntimeSetup LoadConfiguration(Stream configFileStream) {
            ContractUtils.RequiresNotNull(configFileStream, "configFileStream");
            Configuration.Section.LoadRuntimeSetup(this, configFileStream);
            return this;
        }

        /// <summary>
        /// Loads settings from a specified XML file and adds them to the specified setup object
        /// Returns <paramref name="baseSetup"/>.
        /// </summary>
        /// <returns>This instance.</returns>
        public ScriptRuntimeSetup LoadConfiguration(string configFilePath) {
            ContractUtils.RequiresNotNull(configFilePath, "configFilePath");

            using (var stream = File.OpenRead(configFilePath)) {
                Configuration.Section.LoadRuntimeSetup(this, stream);
            }

            return this;
        }
#endif

        public ScriptRuntimeSetup LoadFromAssemblies(IEnumerable<Assembly> assemblies) {
            ContractUtils.RequiresNotNullItems(assemblies, "assemblies");

            foreach (var assembly in assemblies) {
                foreach (DynamicLanguageProviderAttribute attribute in assembly.GetCustomAttributes(typeof(DynamicLanguageProviderAttribute), false)) {
                    var languageSetup = new LanguageSetup(attribute.DisplayName, attribute.Names, attribute.FileExtensions);
                    _languageSetups[attribute.LanguageContextType.AssemblyQualifiedName] = languageSetup;
                }
            }

            return this;
        }
    }
}
