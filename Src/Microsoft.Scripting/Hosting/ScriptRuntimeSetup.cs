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
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")] // TODO: fix
    [Serializable]
    public struct LanguageProviderSetup {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")] // TODO: fix
        public readonly string[] Names;
        public readonly string Assembly;
        public readonly string Type;

        public LanguageProviderSetup(string type, string assembly, params string[] names) {
            this.Names = names;
            this.Assembly = assembly;
            this.Type = type;
        }
    }

    [Serializable]
    public sealed class ScriptRuntimeSetup {

        private LanguageProviderSetup[]/*!*/ _languageProviders;
        private Type/*!*/ _hostType;
        private object[]/*!*/ _hostArguments;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: fix
        public LanguageProviderSetup[]/*!*/ LanguageProviders {
            get { 
                return _languageProviders; 
            }
            set {
                Contract.RequiresNotNull(value, "value");
                _languageProviders = value; 
            }
        }

        public Type/*!*/ HostType {
            get { 
                return _hostType; 
            }
            set {
                Contract.RequiresNotNull(value, "value");
                if (!value.IsSubclassOf(typeof(ScriptHost))) throw new ArgumentException("Invalid type", "value");
                _hostType = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: fix
        public object[]/*!*/ HostArguments {
            get {
                return _hostArguments;
            }
            set {
                Contract.RequiresNotNull(value, "value");
                _hostArguments = value;
            }
        }

        private static string/*!*/ AppDomainDataKey {
            get {
                return typeof(ScriptRuntimeSetup).FullName;
            }
        }

#if !SILVERLIGHT
        public void AssociateWithAppDomain(AppDomain/*!*/ domain) {
            Contract.RequiresNotNull(domain, "domain");
            domain.SetData(AppDomainDataKey, this);
        }

        public static ScriptRuntimeSetup GetAppDomainAssociated(AppDomain/*!*/ domain) {
            Contract.RequiresNotNull(domain, "domain");
            return domain.GetData(AppDomainDataKey) as ScriptRuntimeSetup;
        }
#endif

        public ScriptRuntimeSetup() 
            : this(false) {
        }

        public ScriptRuntimeSetup(bool addWellKnownLanguages) {
            if (addWellKnownLanguages) {
                _languageProviders = new LanguageProviderSetup[] {
#if SIGNED
                    new LanguageProviderSetup("IronPython.Runtime.PythonContext", "IronPython, Version=2.0.0.1000, Culture=neutral, PublicKeyToken=31bf3856ad364e35", ".py", "py", "python", "ironpython"),
                    new LanguageProviderSetup("Microsoft.JScript.Runtime.JSContext", "Microsoft.JScript.Runtime, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", ".jsx", ".js", "managedjscript", "js", "jscript"),
                    new LanguageProviderSetup("Ruby.Runtime.RubyContext", "IronRuby, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", ".rb", "rb", "ruby", "ironruby"),
                    new LanguageProviderSetup("Microsoft.VisualBasic.Scripting.Runtime.VisualBasicLanguageContext", "Microsoft.VisualBasic.Scripting, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", ".vbx", "vbx"),
                    new LanguageProviderSetup("ToyScript.ToyLanguageContext", "ToyScript, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", ".ts", "ts", "toyscript", "toyscript"),
#else
                    new LanguageProviderSetup("IronPython.Runtime.PythonContext", "IronPython", ".py", "py", "python", "ironpython"),
                    new LanguageProviderSetup("Microsoft.JScript.Runtime.JSContext", "Microsoft.JScript.Runtime", ".jsx", ".js", "managedjscript", "js", "jscript"),
                    new LanguageProviderSetup("Microsoft.VisualBasic.Scripting.Runtime.VisualBasicLanguageContext", "Microsoft.VisualBasic.Scripting", ".vbx", "vbx"),
                    new LanguageProviderSetup("Ruby.Runtime.RubyContext", "IronRuby", ".rb", "rb", "ruby", "ironruby"),
                    new LanguageProviderSetup("ToyScript.ToyLanguageContext", "ToyScript", ".ts", "ts", "toyscript", "toyscript"),
#endif
                };
            } else {
                 _languageProviders = new LanguageProviderSetup[0];
            }

            _hostType = typeof(ScriptHost);
            _hostArguments = ArrayUtils.EmptyObjects;
        }

        internal void RegisterLanguages(ScriptDomainManager/*!*/ manager) {
            Debug.Assert(manager != null);

            foreach (LanguageProviderSetup provider in _languageProviders) {
                manager.RegisterLanguageContext(provider.Assembly, provider.Type, provider.Names);
            }
        }
    }
}
