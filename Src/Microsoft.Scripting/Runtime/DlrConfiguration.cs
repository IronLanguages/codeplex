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
using System.Threading;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Singleton for each language.
    /// </summary>
    internal sealed class LanguageConfiguration {
        private readonly AssemblyQualifiedTypeName _providerName;
        private readonly IDictionary<string, object> _options;
        private LanguageContext _context;
        
        public LanguageContext LanguageContext {
            get { return _context; }
        }

        public LanguageConfiguration(AssemblyQualifiedTypeName providerName, IDictionary<string, object> options) {
            _providerName = providerName;
            _options = options;
        }

        /// <summary>
        /// Must not be called under a lock as it can potentially call a user code.
        /// </summary>
        /// <exception cref="MissingTypeException"></exception>
        /// <exception cref="System.Scripting.InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
        internal LanguageContext LoadLanguageContext(ScriptDomainManager domainManager) {
            if (_context == null) {
                Type type;

                try {
                    type = domainManager.Platform.LoadAssembly(_providerName.AssemblyName).GetType(_providerName.TypeName, true);
                } catch (Exception e) {
                    throw new MissingTypeException(_providerName.ToString(), e);
                }

                Interlocked.CompareExchange(ref _context, ReflectionUtils.CreateInstance<LanguageContext>(type, domainManager, _options), null);
            }

            return _context;
        }
    }

    public sealed class DlrConfiguration {
        private bool _frozen;

        private readonly bool _debugMode;
        private readonly bool _privateBinding;

        private readonly Dictionary<string, LanguageConfiguration> _languageIds;
        private readonly Dictionary<string, LanguageConfiguration> _languageExtensions;
        private readonly Dictionary<AssemblyQualifiedTypeName, LanguageConfiguration> _langaugeConfigurations;

        public DlrConfiguration(bool debugMode, bool privateBinding) {
            _debugMode = debugMode;
            _privateBinding = privateBinding;

            _languageIds = new Dictionary<string, LanguageConfiguration>(StringComparer.OrdinalIgnoreCase);
            _languageExtensions = new Dictionary<string, LanguageConfiguration>(StringComparer.OrdinalIgnoreCase);
            _langaugeConfigurations = new Dictionary<AssemblyQualifiedTypeName, LanguageConfiguration>();
        }

        /// <summary>
        /// Whether the application is in debug mode.
        /// This means:
        /// 
        /// 1) Symbols are emitted for debuggable methods (methods associated with SourceUnit).
        /// 2) Debuggable methods are emitted to non-collectable types (this is due to CLR limitations on dynamic method debugging).
        /// 3) JIT optimization is disabled for all methods
        /// 4) Languages may disable optimizations based on this value.
        /// </summary>
        public bool DebugMode {
            get { return _debugMode; }
        }

        /// <summary>
        /// Ignore CLR visibility checks.
        /// </summary>
        public bool PrivateBinding {
            get { return _privateBinding; }
        }

        public void AddLanguage(AssemblyQualifiedTypeName provider, IList<string> identifiers, IList<string> fileExtensions, 
            IDictionary<string, object> options) {
            ContractUtils.Requires(!_frozen, "Configuration cannot be modified once the runtime is initialized");
            ContractUtils.Requires(!_langaugeConfigurations.ContainsKey(provider), "Language already added");
            ContractUtils.Requires(CollectionUtils.TrueForAll(identifiers, (id) => !String.IsNullOrEmpty(id) && !_languageIds.ContainsKey(id)), "Language identifier null, empty or already defined");
            ContractUtils.Requires(CollectionUtils.TrueForAll(fileExtensions, (ext) => !String.IsNullOrEmpty(ext) && !_languageExtensions.ContainsKey(ext)), "Extension null, empty or already defined");

            var config = new LanguageConfiguration(provider, options);
            
            _langaugeConfigurations.Add(provider, config);

            // allow duplicate ids in identifiers and extensions lists:
            foreach (var id in identifiers) {
                _languageIds[id] = config;
            }

            foreach (var ext in fileExtensions) {
                _languageExtensions[NormalizeExtension(ext)] = config;
            }
        }

        internal static string NormalizeExtension(string extension) {
            return extension[0] == '.' ? extension : "." + extension;
        }

        internal void Freeze() {
            Debug.Assert(!_frozen);
            _frozen = true;
        }

        internal bool TryLoadLanguage(ScriptDomainManager manager, AssemblyQualifiedTypeName providerName, out LanguageContext language) {
            Assert.NotNull(manager);
            LanguageConfiguration config;

            if (_langaugeConfigurations.TryGetValue(providerName, out config)) {
                language = config.LoadLanguageContext(manager);
                return true;
            }

            language = null;
            return false;
        }

        internal bool TryLoadLanguage(ScriptDomainManager manager, string str, bool isExtension, out LanguageContext language) {
            Assert.NotNull(manager, str);

            var dict = (isExtension) ? _languageExtensions : _languageIds;

            LanguageConfiguration config;
            if (dict.TryGetValue(str, out config)) {
                language = config.LoadLanguageContext(manager);
                return true;
            }

            language = null;
            return false;
        }

        public string[] GetLanguageIdentifiers(LanguageContext context) {
            ContractUtils.RequiresNotNull(context, "context");

            List<string> result = new List<string>();
            
            foreach (var entry in _languageIds) {
                if (entry.Value.LanguageContext == context) {
                    result.Add(entry.Key);
                }
            }

            return result.ToArray();
        }

        public string[] GetLanguageIdentifiers() {
            return ArrayUtils.MakeArray<string>(_languageIds.Keys);
        }

        public string[] GetFileExtensions(LanguageContext context) {
            ContractUtils.RequiresNotNull(context, "context");

            List<string> result = new List<string>();
            foreach (var entry in _languageExtensions) {
                if (entry.Value.LanguageContext == context) {
                    result.Add(entry.Key);
                }
            }

            return result.ToArray();
        }

        public string[] GetFileExtensions() {
            return ArrayUtils.MakeArray<string>(_languageExtensions.Keys);
        }
    }
}