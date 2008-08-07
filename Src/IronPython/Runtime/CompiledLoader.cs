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
using Microsoft.Scripting;
using System.IO;

namespace IronPython.Runtime {
    public class CompiledLoader {
        private List<ScriptCode> _codes = new List<ScriptCode>();

        internal void AddScriptCode(ScriptCode code) {
            _codes.Add(code);
        }

        public ModuleLoader find_module(string fullname, List path) {
            foreach (ScriptCode sc in _codes) {

                if (Path.GetFileNameWithoutExtension(sc.SourceUnit.Path) == fullname) {
                    // normal .py file
                    return new ModuleLoader(sc);
                } else if (fullname.IndexOf('.') > 0) {
                    string packagePath = Path.Combine(Path.GetDirectoryName(sc.SourceUnit.Path), Path.GetFileNameWithoutExtension(sc.SourceUnit.Path)).Replace(Path.DirectorySeparatorChar, '.');
                    if (packagePath == fullname) {
                        return new ModuleLoader(sc);
                    }
                }

                if (sc.SourceUnit.Path.EndsWith("__init__.py") && sc.SourceUnit.Path.EndsWith(fullname + Path.DirectorySeparatorChar + "__init__.py")) {
                    return new ModuleLoader(sc);
                }
            }

            return null;
        }
    }
}
