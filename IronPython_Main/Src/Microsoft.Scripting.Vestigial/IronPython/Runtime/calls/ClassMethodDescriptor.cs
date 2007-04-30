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

using Microsoft.Scripting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Calls {
    public class ClassMethodDescriptor : DynamicTypeSlot, ICodeFormattable {
        internal BuiltinFunction func;

        internal ClassMethodDescriptor(BuiltinFunction func) {
            this.func = func;
        }

        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            value = GetAttribute(instance, owner);
            return true;
        }

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(object instance) { return GetAttribute(instance, null); }

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            owner = CheckGetArgs(instance, owner);
            return new Method(func, owner, Ops.GetDynamicType(owner));
        }

        private object CheckGetArgs(object instance, object owner) {
            if (owner == null) {
                if (instance == null) throw Ops.TypeError("__get__(None, None) is invalid");
                owner = Ops.GetDynamicType(instance);
            } else {
                DynamicType dt = owner as DynamicType;
                if (dt == null) {
                    throw Ops.TypeError("descriptor {0} for type {1} needs a type, not a {2}",
                        Ops.StringRepr(func.Name),
                        Ops.StringRepr(func.DeclaringType.Name),
                        Ops.StringRepr(Ops.GetDynamicType(owner)));
                }
                if (!dt.IsSubclassOf(TypeCache.Dict)) {
                    throw Ops.TypeError("descriptor {0} for type {1} doesn't apply to type {2}",
                        Ops.StringRepr(func.Name),
                        Ops.StringRepr(func.DeclaringType.Name),
                        Ops.StringRepr(dt.Name));
                }
            }
            if (instance != null)
                BuiltinMethodDescriptor.CheckSelfWorker(instance, func);

            return owner;
        }
        #endregion

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            BuiltinFunction bf = func as BuiltinFunction;
            if (bf != null) {
                return String.Format("<method {0} of {1} objects>",
                    Ops.StringRepr(bf.Name),
                    Ops.StringRepr(bf.DeclaringType));
            }

            return String.Format("<classmethod object at {0}>",
                IdDispenser.GetId(this));
        }

        #endregion

        public override bool Equals(object obj) {
            ClassMethodDescriptor cmd = obj as ClassMethodDescriptor;
            if (cmd == null) return false;

            return cmd.func == func;
        }

        public override int GetHashCode() {
            return ~func.GetHashCode();
        }
    } 
}
