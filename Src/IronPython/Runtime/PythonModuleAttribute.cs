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
using System.Text;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime {
    /// <summary>
    /// This assembly-level attribute specifies which types in the engine represent built-in Python modules.
    /// 
    /// Members of a built-in module type should all be static as an instance is never created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class PythonModuleAttribute : Attribute {
        private readonly string/*!*/ _name;
        private readonly Type/*!*/ _type;

        /// <summary>
        /// Creates a new PythonModuleAttribute that can be used to specify a built-in module that exists
        /// within an assembly.
        /// </summary>
        /// <param name="name">The built-in module name</param>
        /// <param name="type">The type that implements the built-in module.</param>
        public PythonModuleAttribute(string/*!*/ name, Type/*!*/ type) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(type, "type");

            _name = name;
            _type = type;
        }

        /// <summary>
        /// The built-in module name
        /// </summary>
        public string/*!*/ Name {
            get { return _name; }
        }

        /// <summary>
        /// The type that implements the built-in module
        /// </summary>
        public Type/*!*/ Type {
            get { return _type; }
        }
    }
}
