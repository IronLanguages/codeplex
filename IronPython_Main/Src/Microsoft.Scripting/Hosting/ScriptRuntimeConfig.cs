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
using System.Security.Permissions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Configuration for the ScriptRuntime
    /// </summary>
    [Serializable]
    public sealed class ScriptRuntimeConfig {
        private readonly ReadOnlyCollection<LanguageConfig> _languages;
        private readonly bool _debugMode;
        private readonly bool _privateBinding;
        private readonly ReadOnlyDictionary<string, object> _options;

        internal ScriptRuntimeConfig(DlrConfiguration config) {            
            var langs = new LanguageConfig[config.Languages.Count];
            int i = 0;
            foreach (var langConfigs in config.Languages) {
                langs[i++] = new LanguageConfig(
                    langConfigs.Key.ToString(),
                    langConfigs.Value.DisplayName,
                    config.GetLanguageNames(langConfigs.Value),
                    config.GetFileExtensions(langConfigs.Value)
                );
            }

            _languages = new ReadOnlyCollection<LanguageConfig>(langs);
            _debugMode = config.DebugMode;
            _privateBinding = config.PrivateBinding;
            _options = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(config.Options));
        }

        /// <summary>
        /// The list of languages configured in this runtime
        /// (note: they are not necessarily loaded)
        /// </summary>
        public IList<LanguageConfig> Languages {
            get { return _languages; }
        }

        /// <summary>
        /// Indicates that the script runtime is in debug mode.
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
        /// Ignore CLR visibility checks
        /// </summary>
        public bool PrivateBinding {
            get { return _privateBinding; }
        }

        /// <summary>
        /// Global options that were set for all languages
        /// </summary>
        public IDictionary<string, object> Options {
            get { return _options; }
        }
    }
}
