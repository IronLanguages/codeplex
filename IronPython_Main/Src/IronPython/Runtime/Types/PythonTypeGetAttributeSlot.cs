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

using Microsoft.Scripting;

namespace IronPython.Runtime.Types {
    class PythonTypeGetAttributeSlot : PythonTypeSlot {
        private PythonType _dt;
        private UserTypeBuilder.CustomAttributeInfo _info;
        private bool _inherited;
        private SymbolId _attrHook;

        public PythonTypeGetAttributeSlot(PythonType dt, UserTypeBuilder.CustomAttributeInfo info, SymbolId attrHook) {
            _dt = dt;
            _info = info;
            _attrHook = attrHook;
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            if (instance == null && _info != null) {
                PythonTypeBuilder dtb = PythonTypeBuilder.GetBuilder(_dt);

                if (_attrHook == Symbols.GetAttribute) {
                    dtb.SetHasGetAttribute(false);
                    dtb.SetCustomBoundGetter(null);
                } else if (_attrHook == Symbols.SetAttr) {
                    dtb.SetCustomSetter(null);
                } else if (_attrHook == Symbols.DelAttr) {
                    dtb.SetCustomDeleter(null);
                }

                dtb.RemoveSlot(context.LanguageContext.ContextId, _attrHook);

                PythonTypeBasesSlot.PropagateGetAttributeFromMro(dtb, _dt.ResolutionOrder, _attrHook);
                return true;
            }

            return false;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            if (_info != null) {
                PythonTypeSlot idts = _info.Function as PythonTypeSlot;
                if(idts != null) return idts.TryGetValue(context, instance, owner, out value);

                value = _info.Function;
                return true;
            }

            bool foundSelf = false;
            foreach (PythonType mroMember in owner.ResolutionOrder) {
                if (mroMember == _dt) {
                    foundSelf = true;
                } else if (foundSelf) {
                    PythonTypeSlot dts;
                    if (mroMember.TryLookupSlot(context, Symbols.GetAttribute, out dts) && dts.TryGetValue(context, instance, owner, out value)) {
                        return true;
                    }
                }
            }
            return base.TryGetValue(context, instance, owner, out value);
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            if (instance == null) {
                EnsureInfo(value);
                _info.Function = value;
                Inherited = false;
                
                return true;
            }

            return false;
        }

        private void EnsureInfo(object value) {
            if (_info == null) {
                PythonTypeBuilder dtb = PythonTypeBuilder.GetBuilder(_dt);
                _info = new UserTypeBuilder.CustomAttributeInfo(value);

                if (_attrHook == Symbols.GetAttribute) {
                    dtb.SetHasGetAttribute(true);
                    dtb.SetCustomBoundGetter(_info.HookedGetAttribute);
                } else if (_attrHook == Symbols.SetAttr) {
                    dtb.SetCustomSetter(_info.HookedSetAttribute);
                } else if (_attrHook == Symbols.DelAttr) {
                    dtb.SetCustomDeleter(_info.HookedDeleteAttribute);
                }
            }
        }

        public bool Inherited {
            get {
                return _inherited;
            }
            set {
                _inherited = value;
            }
        }
        
    }
}
