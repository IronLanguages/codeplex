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
    /// <summary>
    /// Stores information needed to setup a ScriptRuntime
    /// </summary>
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

        /// <summary>
        /// The list of language setup information for languages to load into
        /// the runtime
        /// </summary>
        public IList<LanguageSetup> LanguageSetups {
            get { return _languageSetups; }
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
            set { _debugMode = value; }
        }

        /// <summary>
        /// Ignore CLR visibility checks
        /// </summary>
        public bool PrivateBinding {
            get { return _privateBinding; }
            set { _privateBinding = value; }
        }

        /// <summary>
        /// Can be any derived class of ScriptHost. When set, it allows the
        /// host to override certain methods to control behavior of the runtime
        /// </summary>
        public Type HostType {
            get { return _hostType; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                ContractUtils.Requires(typeof(ScriptHost).IsAssignableFrom(value), "value", "Must be ScriptHost or a derived type of ScriptHost");
                _hostType = value;
            }
        }

        /// <remarks>
        /// Option names are case-sensitive.
        /// </remarks>
        public Dictionary<string, object> Options {
            get { return _options; }
        }

        /// <summary>
        /// Arguments passed to the host type when it is constructed
        /// </summary>
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

        internal DlrConfiguration ToConfiguration(string paramName) {
            ContractUtils.Requires(_languageSetups.Count > 0, paramName, "ScriptRuntimeSetup must have at least one LanguageSetup");

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
