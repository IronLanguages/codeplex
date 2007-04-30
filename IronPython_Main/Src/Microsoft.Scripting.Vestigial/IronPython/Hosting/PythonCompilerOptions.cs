/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using IronPython.Hosting;
using Microsoft.Scripting.Internal.Ast;

using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;

namespace IronPython.Compiler {
    [Flags]
    public enum PythonLanguageFeatures {
        Default = 0,
        AllowWithStatement = 1,
        TrueDivision = 2,
        SyntaxV25 = 4
    }

    [Serializable]
    public sealed class PythonCompilerOptions : CompilerOptions {
        private PythonLanguageFeatures _languageFeatures;
        private bool _skipFirstLine;

        public PythonLanguageFeatures LanguageFeatures {
            get { return _languageFeatures; }
            set { _languageFeatures = value; }
        }

        // TODO: Python specific; remove when TrueDivision gets to AST statements:
        public override bool PythonTrueDivision {
            get {
                return TrueDivision;
            }
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

        public bool SyntaxV25 {
            get {
                return (_languageFeatures & PythonLanguageFeatures.SyntaxV25) != 0;
            }
            set {
                if (value) _languageFeatures |= PythonLanguageFeatures.SyntaxV25;
                else _languageFeatures &= ~PythonLanguageFeatures.SyntaxV25;
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

        public bool SkipFirstLine {
            get { return _skipFirstLine; }
            set { _skipFirstLine = value; }
        }

        public PythonCompilerOptions() {
            TrueDivision = (ScriptDomainManager.Options.Division == DivisionOption.New);
            SyntaxV25 = ScriptDomainManager.Options.Python25;
        }

        #region ICloneable Members

        public override object Clone() {
            return MemberwiseClone();
        }

        #endregion
    }
}
