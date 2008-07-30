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
using IronPython.Runtime;
using Microsoft.Scripting;

namespace IronPython.Compiler {
    [Flags]
    public enum PythonLanguageFeatures {
        Default = 0,
        AllowWithStatement = 1,
        TrueDivision = 2,
        AbsoluteImports = 4,
    }

    [Serializable]
    public sealed class PythonCompilerOptions : CompilerOptions {
        private PythonLanguageFeatures _languageFeatures;
        private ModuleOptions _module = ModuleOptions.Optimized;
        private bool _skipFirstLine, _dontImplyIndent;
        private string _moduleName;

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

        public PythonCompilerOptions()
            : this(false) {
        }

        public PythonCompilerOptions(bool trueDivision) {
            TrueDivision = trueDivision;
        }

        #region ICloneable Members

        public override object Clone() {
            return MemberwiseClone();
        }

        #endregion
    }
}
