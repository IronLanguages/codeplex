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
        private readonly List<LanguageSetup> _languageSetups;

        // DLR options:
        private bool _debugMode;
        private bool _privateBinding;

        // common language options:
        private Dictionary<string, object> _options;

        public ScriptRuntimeSetup() {
            _languageSetups = new List<LanguageSetup>();
            _options = new Dictionary<string, object>();
            _hostType = typeof(ScriptHost);
            _hostArguments = ArrayUtils.EmptyObjects;
        }

        public IList<LanguageSetup> LanguageSetups {
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
        public Dictionary<string, object> Options {
            get { return _options; }
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

        internal DlrConfiguration ToConfiguration() {
            var config = new DlrConfiguration(_debugMode, _privateBinding, _options);

            foreach (var language in _languageSetups) {
                config.AddLanguage(
                    language.TypeName,
                    language.DisplayName,
                    language.Names,
                    language.FileExtensions,
                    language.Options
                );
            }

            return config;
        }

        /// <summary>
        /// Reads setup from .NET configuration system (.config files).
        /// If there is no configuration available returns an empty setup.
        /// </summary>
        public static ScriptRuntimeSetup ReadConfiguration() {
#if SILVERLIGHT
            return new ScriptRuntimeSetup();
#else
            var setup = new ScriptRuntimeSetup();
            Configuration.Section.LoadRuntimeSetup(setup, null);
            return setup;
#endif
        }

#if !SILVERLIGHT
        /// <summary>
        /// Reads setup from a specified XML stream.
        /// </summary>
        public static ScriptRuntimeSetup ReadConfiguration(Stream configFileStream) {
            ContractUtils.RequiresNotNull(configFileStream, "configFileStream");
            var setup = new ScriptRuntimeSetup();
            Configuration.Section.LoadRuntimeSetup(setup, configFileStream);
            return setup;
        }

        /// <summary>
        /// Reads setup from a specified XML file.
        /// </summary>
        public static ScriptRuntimeSetup ReadConfiguration(string configFilePath) {
            ContractUtils.RequiresNotNull(configFilePath, "configFilePath");

            using (var stream = File.OpenRead(configFilePath)) {
                return ReadConfiguration(stream);
            }
        }
#endif
    }
}
