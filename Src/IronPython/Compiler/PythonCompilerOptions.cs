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

using System; using Microsoft;
using IronPython.Runtime;
using Microsoft.Scripting;

namespace IronPython.Compiler {
    [Flags]
    public enum PythonLanguageFeatures {
        Default = 0,
        /// <summary>
        /// Enable usage of the with statement
        /// </summary>
        AllowWithStatement = 0x001,
        /// <summary>
        /// Enable true division (1/2 == .5)
        /// </summary>
        TrueDivision       = 0x002,
        /// <summary>
        /// Enable absolute imports
        /// </summary>
        AbsoluteImports    = 0x004,
        /// <summary>
        /// Enable usage of print as a function
        /// </summary>
        PrintFunction      = 0x008,
        /// <summary>
        /// Enable access to features new in Python 2.6
        /// </summary>
        Python26           = 0x010,
        /// <summary>
        /// Include comments in the parse tree
        /// </summary>
        Verbatim           = 0x020,
    }

    [Serializable]
    public sealed class PythonCompilerOptions : CompilerOptions {
        private PythonLanguageFeatures _languageFeatures;
        private ModuleOptions _module = ModuleOptions.Optimized;
        private bool _skipFirstLine, _dontImplyIndent;
        private string _moduleName;

        /// <summary>
        /// Creates a new PythonCompilerOptions with the default langauge features enabled.
        /// </summary>
        public PythonCompilerOptions()
            : this(PythonLanguageFeatures.Default) {
        }

        /// <summary>
        /// Creates a new PythonCompilerOptions with the specified langauge features enabled.
        /// </summary>
        public PythonCompilerOptions(PythonLanguageFeatures features) {
            _languageFeatures = features;
        }

        /// <summary>
        /// Creates a new PythonCompilerOptions and enables or disables true division.
        /// 
        /// This overload is obsolete, instead you should use the overload which takes a
        /// PythonLanguageFeatures.
        /// </summary>
        [Obsolete("Use the overload that takes PythonLanguageFeatures instead")]
        public PythonCompilerOptions(bool trueDivision) {
            TrueDivision = trueDivision;
        }

        /// <summary>
        /// Gets or sets the language features which will be enabled when compiling.
        /// </summary>
        public PythonLanguageFeatures LanguageFeatures {
            get { return _languageFeatures; }
            set { _languageFeatures = value; }
        }

        public bool DontImplyDedent {
            get { return _dontImplyIndent; }
            set { _dontImplyIndent = value; }
        }

        public bool TrueDivision {
            get {
                return (_languageFeatures & PythonLanguageFeatures.TrueDivision) != 0;
            }
            set {
                if (value) _languageFeatures |= PythonLanguageFeatures.TrueDivision;
                else _languageFeatures &= ~PythonLanguageFeatures.TrueDivision;
            }
        }

        public bool AllowWithStatement {
            get {
                return (_languageFeatures & PythonLanguageFeatures.AllowWithStatement) != 0;
            }
            set {
                if (value) _languageFeatures |= PythonLanguageFeatures.AllowWithStatement;
                else _languageFeatures &= ~PythonLanguageFeatures.AllowWithStatement;
            }
        }

        public bool AbsoluteImports {
            get {
                return (_languageFeatures & PythonLanguageFeatures.AbsoluteImports) != 0;
            }
            set {
                if (value) _languageFeatures |= PythonLanguageFeatures.AbsoluteImports;
                else _languageFeatures &= ~PythonLanguageFeatures.AbsoluteImports;
            }
        }

        public bool Verbatim {
            get {
                return (_languageFeatures & PythonLanguageFeatures.Verbatim) != 0;
            }
            set {
                if (value) _languageFeatures |= PythonLanguageFeatures.Verbatim;
                else _languageFeatures &= ~PythonLanguageFeatures.Verbatim;
            }
        }

        public bool PrintFunction {
            get {
                return (_languageFeatures & PythonLanguageFeatures.PrintFunction) != 0;
            }
            set {
                if (value) _languageFeatures |= PythonLanguageFeatures.PrintFunction;
                else _languageFeatures &= ~PythonLanguageFeatures.PrintFunction;
            }
        }

        public bool Python26 {
            get {
                return (_languageFeatures & PythonLanguageFeatures.Python26) != 0;
            }
            set {
                if (value) _languageFeatures |= PythonLanguageFeatures.Python26;
                else _languageFeatures &= ~PythonLanguageFeatures.Python26;
            }
        }

        public ModuleOptions Module {
            get {
                return _module;
            }
            set {
                _module = value;
            }
        }

        public string ModuleName {
            get {
                return _moduleName;
            }
            set {
                _moduleName = value;
            }
        }

        public bool SkipFirstLine {
            get { return _skipFirstLine; }
            set { _skipFirstLine = value; }
        }

        #region ICloneable Members

        public override object Clone() {
            return MemberwiseClone();
        }

        #endregion
    }
}
