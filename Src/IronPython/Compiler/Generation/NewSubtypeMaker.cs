/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace IronPython.Compiler.Generation {
    /// <summary>
    /// Creates sub-types of new-types.  Sub-types of new types are created when
    /// the new-type is created with slots, and therefore has a concrete object
    /// layout which the subtype also inherits.
    /// </summary>
    class NewSubtypeMaker : NewTypeMaker {
        public NewSubtypeMaker(Tuple bases, NewTypeInfo ti)
            : base(bases, ti) {
        }

        protected override string GetName() {
            return base.GetName().Substring(TypePrefix.Length);
        }

        protected override void ImplementInterfaces() {
            // only implement interfaces defined in our newly derived type
            IList<Type> baseInterfaces = baseType.GetInterfaces();
            foreach (Type interfaceType in interfaceTypes) {
                if (!baseInterfaces.Contains(interfaceType)) {
                    ImplementInterface(interfaceType);
                }
            }
        }

        protected override ParameterInfo[] GetOverrideCtorSignature(ParameterInfo[] original) {
            return original;
        }

        protected override bool ShouldOverrideVirtual(MethodInfo mi) {
            return !IsInstanceType(mi.DeclaringType);
        }

        protected override void ImplementPythonObject() {
            if (NeedsDictionary) {
                // override our bases slots implementation w/ one that
                // can use dicts
                CodeGen cg = tg.DefineMethodOverride(baseType.GetMethod("GetDict"));
                dictField.EmitGet(cg);
                cg.EmitReturn();
                cg.Finish();

                cg = tg.DefineMethodOverride(baseType.GetMethod("SetDict"));
                cg.EmitArgGet(0);
                dictField.EmitSet(cg);
                cg.EmitRawConstant(true);
                cg.EmitReturn();
                cg.Finish();
            }
        }

        private bool NeedsNewWeakRef() {
            foreach (IPythonType pt in baseClasses) {
                UserType ut = pt as UserType;
                if (ut == null) continue;

                if (ut.HasWeakRef) return false;
            }
            return true;
        }

        protected override void ImplementWeakReference() {
            if (NeedsNewWeakRef()
                && (slots == null || slots.Contains("__weakref__"))) {
                // base type didn't have slots, but it's there now...
                base.ImplementWeakReference();
            }
        }
    }
}
