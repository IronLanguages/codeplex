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
            value = GetAttribute(context, instance, owner);
            return true;
        }

        #region IDescriptor Members
        [PythonName("__get__")]
        public object GetAttribute(CodeContext context, object instance) { return GetAttribute(context, instance, null); }

        [PythonName("__get__")]
        public object GetAttribute(CodeContext context, object instance, object owner) {
            owner = CheckGetArgs(context, instance, owner);
            return new Method(func, owner, DynamicHelpers.GetDynamicType(owner));
        }

        private object CheckGetArgs(CodeContext context, object instance, object owner) {
            if (owner == null) {
                if (instance == null) throw PythonOps.TypeError("__get__(None, None) is invalid");
                owner = DynamicHelpers.GetDynamicType(instance);
            } else {
                DynamicType dt = owner as DynamicType;
                if (dt == null) {
                    throw PythonOps.TypeError("descriptor {0} for type {1} needs a type, not a {2}",
                        PythonOps.StringRepr(func.Name),
                        PythonOps.StringRepr(func.DeclaringType.Name),
                        PythonOps.StringRepr(DynamicTypeOps.GetName(owner)));
                }
                if (!dt.IsSubclassOf(TypeCache.Dict)) {
                    throw PythonOps.TypeError("descriptor {0} for type {1} doesn't apply to type {2}",
                        PythonOps.StringRepr(func.Name),
                        PythonOps.StringRepr(func.DeclaringType.Name),
                        PythonOps.StringRepr(DynamicTypeOps.GetName(dt)));
                }
            }
            if (instance != null)
                BuiltinMethodDescriptor.CheckSelfWorker(context, instance, func);

            return owner;
        }
        #endregion

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            BuiltinFunction bf = func as BuiltinFunction;
            if (bf != null) {
                return String.Format("<method {0} of {1} objects>",
                    PythonOps.StringRepr(bf.Name),
                    PythonOps.StringRepr(bf.DeclaringType));
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
