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

        public IList<LanguageConfig> Languages {
            get { return _languages; }
        }

        public bool DebugMode {
            get { return _debugMode; }
        }

        public bool PrivateBinding {
            get { return _privateBinding; }
        }

        public IDictionary<string, object> Options {
            get { return _options; }
        }

        internal LanguageConfig GetLanguageConfig(LanguageContext context) {
            if (context is InvariantContext) {
                return null;
            }

            var aqtn = new AssemblyQualifiedTypeName(context.GetType());
            foreach (var language in _languages) {
                if (aqtn == new AssemblyQualifiedTypeName(language.TypeName)) {
                    return language;
                }
            }

            throw Assert.Unreachable;
        }
    }
}
