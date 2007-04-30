/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;

namespace IronPython.CodeDom {
    public class PythonProvider : CodeDomProvider, IMergableProvider {
        List<string> references = new List<string>();

        [Obsolete]
        public override ICodeCompiler CreateCompiler() {
            return new PythonGenerator();
        }

        [Obsolete]
        public override ICodeGenerator CreateGenerator() {
            return new PythonGenerator();
        }

        [Obsolete]
        public override ICodeParser CreateParser() {
            return new PythonParser(references);
        }

        public void AddReference(string assemblyName) {
            references.Add(assemblyName);
        }

        public override string FileExtension {
            get {
                return "py";
            }
        }

        #region IMergableProvider Members

        public CodeCompileUnit ParseMergable(string text, string filename, IMergeDestination mergeDestination) {
            return new PythonParser(references).ParseMergeable(text, filename, mergeDestination);
        }

        public CodeCompileUnit ParseMergable(System.IO.StreamReader sw, IMergeDestination mergeDestination) {
            return new PythonParser(references).ParseMergeable(sw, mergeDestination);
        }

        public void MergeCodeFromCompileUnit(CodeCompileUnit compileUnit) {
            new PythonGenerator().InternalGenerateCompileUnit(compileUnit);
        }

        #endregion
    }

}
