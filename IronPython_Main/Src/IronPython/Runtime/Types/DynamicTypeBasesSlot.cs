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

using System.Diagnostics;

using Microsoft.Scripting;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    class DynamicTypeBasesSlot : DynamicTypeSlot {
        public override bool TryGetValue(CodeContext context, object instance, DynamicMixin owner, out object value) {
            DynamicType dt = instance as DynamicType ?? owner as DynamicType;

            if (dt == null) throw new ArgumentException("expected DynamicType", "instance");
            
            object[] res = new object[dt.BaseTypes.Count];            
            IList<DynamicType> bases = dt.BaseTypes;
            for(int i = 0; i<bases.Count; i++) {
                DynamicType baseType = bases[i];

                if (Mro.IsOldStyle(baseType)) {
                    DynamicTypeSlot dts;
                    bool success = baseType.TryLookupSlot(context, Symbols.Class, out dts);
                    Debug.Assert(success);

                    success = dts.TryGetValue(context, null, baseType, out res[i]);
                    Debug.Assert(success);
                } else {
                    res[i] = baseType;
                }
            }

            value = new Tuple(false, res);
            return true;
        }

        public override bool TrySetValue(CodeContext context, object instance, DynamicMixin owner, object value) {
            if (instance == null) return false;

            DynamicType dt = instance as DynamicType;

            Debug.Assert(dt != null);

            // validate we got a tuple...           
            Tuple t = value as Tuple;
            if (t == null) throw PythonOps.TypeError("expected tuple of types or old-classes, got {0}", PythonOps.StringRepr(DynamicTypeOps.GetName(value)));

            List<DynamicType> ldt = new List<DynamicType>();
            DynamicTypeBuilder dtb = DynamicTypeBuilder.GetBuilder(dt);            

            foreach(object o in t) {
                // gather all the type objects...
                DynamicType adt = o as DynamicType;
                if (adt == null) {
                    OldClass oc = o as OldClass;
                    if (oc == null) {
                        throw PythonOps.TypeError("expected tuple of types, got {0}", PythonOps.StringRepr(DynamicTypeOps.GetName(o)));                        
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
            IList<DynamicMixin> mro = Mro.Calculate(dt, ldt);

            dtb.SetBases(ldt);

            PropagateAttributeCustomization(context, dt, dtb, mro);

            dtb.SetResolutionOrder(mro);

            dtb.ReleaseBuilder();

            return true;
        }

        private static void PropagateAttributeCustomization(CodeContext context, DynamicType dt, DynamicTypeBuilder dtb, IList<DynamicMixin> mro) {
            if (dt.CustomBoundGetter != null) {
                // we already have a __getattribute__, figure out if it's inherited
                // our declared on the type and propagate it or leave it alone
                DynamicTypeSlot dts;
                if (dt.TryLookupSlot(context, Symbols.GetAttribute, out dts)) {
                    DynamicTypeGetAttributeSlot getAttr = dts as DynamicTypeGetAttributeSlot;
                    if (getAttr != null && getAttr.Inherited) {
                        PropagateGetAttributeFromMro(dtb, mro, Symbols.GetAttribute);
                    }
                }
            } else {
                // propagate __getattribute__ if necessary
                PropagateGetAttributeFromMro(dtb, mro, Symbols.GetAttribute);
            }

            if (dt.CustomSetter != null) {
                DynamicTypeSlot dts;
                if (dt.TryLookupSlot(context, Symbols.SetAttr, out dts)) {
                    DynamicTypeGetAttributeSlot setAttr = dts as DynamicTypeGetAttributeSlot;
                    if (setAttr != null && setAttr.Inherited) {
                        PropagateGetAttributeFromMro(dtb, mro, Symbols.SetAttr);
                    }
                }
            } else {
                PropagateGetAttributeFromMro(dtb, mro, Symbols.SetAttr);
            }

            if (dt.CustomDeleter != null) {
                DynamicTypeSlot dts;
                if (dt.TryLookupSlot(context, Symbols.DelAttr, out dts)) {
                    DynamicTypeGetAttributeSlot delAttr = dts as DynamicTypeGetAttributeSlot;
                    if (delAttr != null && delAttr.Inherited) {
                        PropagateGetAttributeFromMro(dtb, mro, Symbols.DelAttr);
                    }
                }
            } else {
                PropagateGetAttributeFromMro(dtb, mro, Symbols.DelAttr);
            }

        }

        internal static void PropagateGetAttributeFromMro(DynamicTypeBuilder dtb, IList<DynamicMixin> mro, SymbolId attrHook) {
            for (int i = 1; i < mro.Count; i++) {
                if (attrHook == Symbols.GetAttribute && mro[i].CustomBoundGetter != null) {
                    dtb.SetCustomBoundGetter(mro[i].CustomBoundGetter);
                    DynamicTypeGetAttributeSlot dts = new DynamicTypeGetAttributeSlot(dtb.UnfinishedType, null, Symbols.GetAttribute);
                    dts.Inherited = true;
                    dtb.AddSlot(Symbols.GetAttribute, dts);
                    break;
                }

                if (attrHook == Symbols.SetAttr && mro[i].CustomSetter != null) {
                    dtb.SetCustomSetter(mro[i].CustomSetter);
                    DynamicTypeGetAttributeSlot dts = new DynamicTypeGetAttributeSlot(dtb.UnfinishedType, null, Symbols.SetAttr);
                    dts.Inherited = true;
                    dtb.AddSlot(Symbols.SetAttr, dts);
                    break;
                }

                if (attrHook == Symbols.DelAttr && mro[i].CustomDeleter != null) {
                    dtb.SetCustomDeleter(mro[i].CustomDeleter);
                    DynamicTypeGetAttributeSlot dts = new DynamicTypeGetAttributeSlot(dtb.UnfinishedType, null, Symbols.DelAttr);
                    dts.Inherited = true;
                    dtb.AddSlot(Symbols.DelAttr, dts);
                    break;
                }
            }
        }

    }
}
