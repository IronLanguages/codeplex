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

namespace Microsoft.Scripting.Actions {

    public class ConvertToAction : Action {
        private Type _type;

        public static ConvertToAction Make(string type) {
            return Make(Type.GetType(type));
        }

        public static ConvertToAction Make(Type type) {
            return new ConvertToAction(type);
        }

        private ConvertToAction(Type type) { this._type = type; }

        public Type ToType { get { return _type; } }
        public override ActionKind Kind { get { return ActionKind.ConvertTo; } }
        public override string ParameterString { get { return _type.AssemblyQualifiedName; } }

        public override bool Equals(object obj) {
            ConvertToAction other = obj as ConvertToAction;
            if (other == null) return false;
            return _type == other._type;
        }

        public override int GetHashCode() {
            return (int)Kind << 28 ^ _type.GetHashCode();
        }
    }

}
