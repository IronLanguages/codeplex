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
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    class PythonTypeBasesSlot : PythonTypeSlot {
        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            PythonType dt = instance as PythonType ?? owner as PythonType;

            if (dt == null) throw new ArgumentException("expected PythonType", "instance");
            
            object[] res = new object[dt.BaseTypes.Count];            
            IList<PythonType> bases = dt.BaseTypes;
            for(int i = 0; i<bases.Count; i++) {
                PythonType baseType = bases[i];

                if (Mro.IsOldStyle(baseType)) {
                    PythonTypeSlot dts;
                    bool success = baseType.TryLookupSlot(context, Symbols.Class, out dts);
                    Debug.Assert(success);

                    success = dts.TryGetValue(context, null, baseType, out res[i]);
                    Debug.Assert(success);
                } else {
                    res[i] = baseType;
                }
            }

            value = new PythonTuple(false, res);
            return true;
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            if (instance == null) return false;

            PythonType dt = instance as PythonType;

            Debug.Assert(dt != null);

            // validate we got a tuple...           
            PythonTuple t = value as PythonTuple;
            if (t == null) throw PythonOps.TypeError("expected tuple of types or old-classes, got {0}", PythonOps.StringRepr(PythonTypeOps.GetName(value)));

            List<PythonType> ldt = new List<PythonType>();
            PythonTypeBuilder dtb = PythonTypeBuilder.GetBuilder(dt);            

            foreach(object o in t) {
                // gather all the type objects...
                PythonType adt = o as PythonType;
                if (adt == null) {
                    OldClass oc = o as OldClass;
                    if (oc == null) {
                        throw PythonOps.TypeError("expected tuple of types, got {0}", PythonOps.StringRepr(PythonTypeOps.GetName(o)));                        
                    }

                    adt = oc.TypeObject;
                }

                ldt.Add(adt);
            }

            // Ensure that we are not switching the CLI type
            Type newType = Compiler.Generation.NewTypeMaker.GetNewType(dt.Name, t, dt.GetMemberDictionary(DefaultContext.Default));
            if (dt.UnderlyingSystemType != newType)
                throw PythonOps.TypeErrorForIncompatibleObjectLayout("__bases__ assignment", dt, newType);

            // set bases & the new resolution order
            IList<PythonType> mro = Mro.Calculate(dt, ldt);

            dtb.SetBases(ldt);

            PropagateAttributeCustomization(context, dt, dtb, mro);

            dtb.SetResolutionOrder(mro);

            PythonTypeSlot dummy;
            if (!dt.TryLookupSlot(context, Symbols.GetAttribute, out dummy)) {
                dtb.SetHasGetAttribute(false);
                foreach (PythonType dm in mro) {
                    if (dm.HasGetAttribute) {
                        dtb.SetHasGetAttribute(true);
                        break;
                    }
                }
            }

            dtb.ReleaseBuilder();

            return true;
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return true;
        }

        private static void PropagateAttributeCustomization(CodeContext context, PythonType dt, PythonTypeBuilder dtb, IList<PythonType> mro) {
            if (dt.CustomBoundGetter != null) {
                // we already have a __getattribute__, figure out if it's inherited
                // our declared on the type and propagate it or leave it alone
                PythonTypeSlot dts;
                if (dt.TryLookupSlot(context, Symbols.GetAttribute, out dts)) {
                    PythonTypeGetAttributeSlot getAttr = dts as PythonTypeGetAttributeSlot;
                    if (getAttr != null && getAttr.Inherited) {
                        PropagateGetAttributeFromMro(dtb, mro, Symbols.GetAttribute);
                    }
                }
            } else {
                // propagate __getattribute__ if necessary
                PropagateGetAttributeFromMro(dtb, mro, Symbols.GetAttribute);
            }

            if (dt.CustomSetter != null) {
                PythonTypeSlot dts;
                if (dt.TryLookupSlot(context, Symbols.SetAttr, out dts)) {
                    PythonTypeGetAttributeSlot setAttr = dts as PythonTypeGetAttributeSlot;
                    if (setAttr != null && setAttr.Inherited) {
                        PropagateGetAttributeFromMro(dtb, mro, Symbols.SetAttr);
                    }
                }
            } else {
                PropagateGetAttributeFromMro(dtb, mro, Symbols.SetAttr);
            }

            if (dt.CustomDeleter != null) {
                PythonTypeSlot dts;
                if (dt.TryLookupSlot(context, Symbols.DelAttr, out dts)) {
                    PythonTypeGetAttributeSlot delAttr = dts as PythonTypeGetAttributeSlot;
                    if (delAttr != null && delAttr.Inherited) {
                        PropagateGetAttributeFromMro(dtb, mro, Symbols.DelAttr);
                    }
                }
            } else {
                PropagateGetAttributeFromMro(dtb, mro, Symbols.DelAttr);
            }

        }

        internal static void PropagateGetAttributeFromMro(PythonTypeBuilder dtb, IList<PythonType> mro, SymbolId attrHook) {
            for (int i = 1; i < mro.Count; i++) {
                if (attrHook == Symbols.GetAttribute && mro[i].CustomBoundGetter != null) {
                    dtb.SetCustomBoundGetter(mro[i].CustomBoundGetter);
                    PythonTypeGetAttributeSlot dts = new PythonTypeGetAttributeSlot(dtb.UnfinishedType, null, Symbols.GetAttribute);
                    dts.Inherited = true;
                    dtb.AddSlot(Symbols.GetAttribute, dts);
                    dtb.SetHasGetAttribute(true);
                    break;
                }

                if (attrHook == Symbols.SetAttr && mro[i].CustomSetter != null) {
                    dtb.SetCustomSetter(mro[i].CustomSetter);
                    PythonTypeGetAttributeSlot dts = new PythonTypeGetAttributeSlot(dtb.UnfinishedType, null, Symbols.SetAttr);
                    dts.Inherited = true;
                    dtb.AddSlot(Symbols.SetAttr, dts);
                    break;
                }

                if (attrHook == Symbols.DelAttr && mro[i].CustomDeleter != null) {
                    dtb.SetCustomDeleter(mro[i].CustomDeleter);
                    PythonTypeGetAttributeSlot dts = new PythonTypeGetAttributeSlot(dtb.UnfinishedType, null, Symbols.DelAttr);
                    dts.Inherited = true;
                    dtb.AddSlot(Symbols.DelAttr, dts);
                    break;
                }
            }
        }

    }
}
