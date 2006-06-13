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
using System.Reflection;

using IronPython.Runtime;

namespace IronPython.Compiler {
    /// <summary>
    /// Creates sub-types of new-types.  Sub-types of new types are created when
    /// the new-type is created with slots, and therefore has a concrete object
    /// layout which the subtype also inherits.
    /// </summary>
    class NewSubtypeMaker : NewTypeMaker {
        public NewSubtypeMaker(Tuple bases, string typeName, NewTypeInfo ti)
            : base(bases, typeName, ti) {
        }

        protected override string GetName() {
            return base.GetName().Substring(TypePrefix.Length);            
        }

        protected override void ImplementInterfaces(){
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
            return mi.DeclaringType != baseType;
        }

        protected override void ImplementPythonObject() {
            if (slots == null) {
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
            foreach(DynamicType dt in baseClasses){
                UserType ut = dt as UserType;
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
