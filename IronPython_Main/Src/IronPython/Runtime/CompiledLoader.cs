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
using System.Collections.Generic;
using System.IO;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    public class CompiledLoader {
        private List<ScriptCode> _codes = new List<ScriptCode>();

        internal void AddScriptCode(ScriptCode code) {
            _codes.Add(code);
        }

        public ModuleLoader find_module(CodeContext/*!*/ context, string fullname, List path) {
            string nameOnDisk = fullname.Replace('.', Path.DirectorySeparatorChar);
            
            foreach (ScriptCode sc in _codes) {
                if (nameOnDisk == sc.SourceUnit.Path) {
                    return new ModuleLoader(sc);
                } else if (sc.SourceUnit.Path.EndsWith("__init__.py") &&
                    sc.SourceUnit.Path == Path.Combine(nameOnDisk, "__init__.py")) {
                    return new ModuleLoader(sc);
                }
            }

            return null;
        }
    }
}
