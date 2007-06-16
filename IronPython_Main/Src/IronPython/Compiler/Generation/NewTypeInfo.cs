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
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime;

namespace IronPython.Compiler.Generation {
    /// <summary>
    /// TypeInfo captures the minimal CLI information required by NewTypeMaker for a Python object
    /// that inherits from a CLI type.
    /// </summary>
    internal class NewTypeInfo {
        // The CLI base-type.
        private Type baseType;

        private IList<Type> interfaceTypes;
        private IList<string> slots;
        private Nullable<int> hash;

        public NewTypeInfo(Type baseType, IList<Type> interfaceTypes, IList<string> slots) {
            this.baseType = baseType;
            this.interfaceTypes = interfaceTypes;
            this.slots = slots;
        }

        public Type BaseType {
            get { return baseType; }
        }

        public IList<string> Slots {
            get {
                return slots;
            }
        }

        public IEnumerable<Type> InterfaceTypes {
            get { return interfaceTypes; }
        }

        public override int GetHashCode() {
            if (hash == null) {
                int hashCode = baseType.GetHashCode();
                for (int i = 0; i < interfaceTypes.Count; i++) {
                    hashCode ^= interfaceTypes[i].GetHashCode();
                }

                if (slots != null) {
                    for (int i = 0; i < slots.Count; i++) {
                        hashCode ^= slots[i].GetHashCode();
                    }
                }

                hash = hashCode;
            }

            return hash.Value;
        }

        public override bool Equals(object obj) {
            NewTypeInfo other = obj as NewTypeInfo;
            if (other == null) return false;


            if (baseType.Equals(other.baseType) &&
                interfaceTypes.Count == other.interfaceTypes.Count &&
                ((slots == null && other.slots == null) ||
                (slots != null && other.slots != null && slots.Count == other.slots.Count))) {

                for (int i = 0; i < interfaceTypes.Count; i++) {
                    if (!interfaceTypes[i].Equals(other.interfaceTypes[i])) return false;
                }

                if (slots != null) {
                    for (int i = 0; i < slots.Count; i++) {
                        if (slots[i] != other.slots[i]) return false;
                    }
                }

                return true;
            }
            return false;
        }
    }


}
