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
using System.Reflection;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Generation {
    using Compiler = Microsoft.Scripting.Ast.Compiler;

    /// <summary>
    /// Creates sub-types of new-types.  Sub-types of new types are created when
    /// the new-type is created with slots, and therefore has a concrete object
    /// layout which the subtype also inherits.
    /// </summary>
    class NewSubtypeMaker : NewTypeMaker {
        public NewSubtypeMaker(PythonTuple bases, NewTypeInfo ti)
            : base(bases, ti) {
        }

        protected override string GetName() {
            return base.GetName().Substring(TypePrefix.Length);
        }

        protected override void ImplementInterfaces() {
            // only implement interfaces defined in our newly derived type
            IList<Type> baseInterfaces = _baseType.GetInterfaces();
            foreach (Type interfaceType in _interfaceTypes) {
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
                Compiler cg = _tg.DefineMethodOverride(typeof(IPythonObject).GetMethod("get_Dict"));
                _dictField.EmitGet(cg);
                cg.EmitReturn();
                cg.Finish();

                cg = _tg.DefineMethodOverride(typeof(IPythonObject).GetMethod("get_HasDictionary"));
                cg.EmitBoolean(true);
                cg.EmitReturn();
                cg.Finish();

                cg = _tg.DefineMethodOverride(typeof(IPythonObject).GetMethod("ReplaceDict"));
                cg.EmitArgGet(0);
                _dictField.EmitSet(cg);
                cg.EmitBoolean(true);
                cg.EmitReturn();
                cg.Finish();

                cg = _tg.DefineMethodOverride(typeof(IPythonObject).GetMethod("SetDict"));
                _dictField.EmitGetAddr(cg);
                cg.EmitArgGet(0);
                cg.EmitCall(typeof(UserTypeOps), "SetDictHelper");
                cg.EmitReturn();
                cg.Finish();
            }
        }

        private bool NeedsNewWeakRef() {
            foreach (PythonType dt in _baseClasses) {
                PythonTypeSlot dts;
                if (dt.TryLookupSlot(DefaultContext.Default, Symbols.WeakRef, out dts))
                    return false;
            }
            return true;
        }

        protected override void ImplementWeakReference() {
            if (NeedsNewWeakRef()
                && (_slots == null || _slots.Contains("__weakref__"))) {
                // base type didn't have slots, but it's there now...
                base.ImplementWeakReference();
            }
        }
    }
}
