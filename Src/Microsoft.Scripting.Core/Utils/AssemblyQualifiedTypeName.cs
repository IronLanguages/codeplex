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

namespace System.Scripting.Utils {
    [Serializable]
    public struct AssemblyQualifiedTypeName : IEquatable<AssemblyQualifiedTypeName> {
        // not-null
        public readonly string TypeName;

        // not-null
        public readonly string AssemblyName;

        public AssemblyQualifiedTypeName(string typeName, string assemblyName) {
            ContractUtils.RequiresNotNull(typeName, "typeName");
            ContractUtils.RequiresNotNull(assemblyName, "assemblyName");

            TypeName = typeName;
            AssemblyName = assemblyName;
        }

        public AssemblyQualifiedTypeName(Type type) {
            TypeName = type.FullName;
            AssemblyName = type.Assembly.FullName;
        }

        public bool Equals(AssemblyQualifiedTypeName other) {
            return TypeName == other.TypeName && AssemblyName == other.AssemblyName;
        }

        public override bool Equals(object obj) {
            return obj is AssemblyQualifiedTypeName && Equals((AssemblyQualifiedTypeName)obj);
        }

        public override int GetHashCode() {
            return TypeName.GetHashCode() ^ AssemblyName.GetHashCode();
        }

        public override string ToString() {
            return TypeName + ", " + AssemblyName;
        }

        public static bool operator ==(AssemblyQualifiedTypeName name, AssemblyQualifiedTypeName other) {
            return name.Equals(other);
        }

        public static bool operator !=(AssemblyQualifiedTypeName name, AssemblyQualifiedTypeName other) {
            return !name.Equals(other);
        }
    }
}
