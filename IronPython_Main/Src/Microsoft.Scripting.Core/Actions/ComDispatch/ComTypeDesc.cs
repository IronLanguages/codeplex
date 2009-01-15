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

#if !SILVERLIGHT // ComObject

using System; using Microsoft;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Actions.ComDispatch {

    public class ComTypeDesc : ComTypeLibMemberDesc {

        private string _typeName;
        private string _documentation;
        private Guid _guid;
        private Dictionary<SymbolId, ComDispatch.ComMethodDesc> _funcs;
        private Dictionary<SymbolId, ComEventDesc> _events;
        private ComMethodDesc _getItem;
        private ComMethodDesc _setItem;
        private readonly ComTypeLibDesc _typeLibDesc;
        private static readonly Dictionary<SymbolId, ComEventDesc> _EmptyEventsDict = new Dictionary<SymbolId, ComEventDesc>();

        protected ComTypeDesc(ITypeInfo typeInfo, ComType memberType, ComTypeLibDesc typeLibDesc) : base(memberType) {
            if (typeInfo != null) {
                ComRuntimeHelpers.GetInfoFromType(typeInfo, out _typeName, out _documentation);
            }
            _typeLibDesc = typeLibDesc;
        }

        internal static ComTypeDesc FromITypeInfo(ComTypes.ITypeInfo typeInfo, ComTypeLibDesc typeLibDesc) {
            ComTypes.TYPEATTR typeAttr;
            typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);
            if (typeAttr.typekind == ComTypes.TYPEKIND.TKIND_COCLASS) {
                return new ComTypeClassDesc(typeInfo, typeLibDesc);
            } else if (typeAttr.typekind == ComTypes.TYPEKIND.TKIND_ENUM) {
                return new ComTypeEnumDesc(typeInfo, typeLibDesc);
            } else if ((typeAttr.typekind == ComTypes.TYPEKIND.TKIND_DISPATCH) ||
                  (typeAttr.typekind == ComTypes.TYPEKIND.TKIND_INTERFACE)) {
                ComTypeDesc typeDesc = new ComTypeDesc(typeInfo, ComType.Interface, typeLibDesc);
                return typeDesc;
            } else {
                throw new InvalidOperationException("Attempting to wrapp an unsupported enum type");
            }
        }

        internal static ComTypeDesc CreateEmptyTypeDesc() {
            ComTypeDesc typeDesc = new ComTypeDesc(null, ComType.Interface, null);
            typeDesc._funcs = new Dictionary<SymbolId, ComMethodDesc>();
            typeDesc._events = _EmptyEventsDict;

            return typeDesc;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // TODO: remove this when COM support is completely moved to the DLR
        internal static Dictionary<SymbolId, ComEventDesc> EmptyEvents {
            get { return _EmptyEventsDict; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // TODO: remove this when COM support is completely moved to the DLR
        internal Dictionary<SymbolId, ComDispatch.ComMethodDesc> Funcs {
            get { return _funcs; }
            set { _funcs = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // TODO: remove this when COM support is completely moved to the DLR
        internal Dictionary<SymbolId, ComEventDesc> Events {
            get { return _events; }
            set { _events = value; }
        }

        // this property is public - accessed by an AST
        public string TypeName {
            get { return _typeName; }
        }

        internal string Documentation {
            get { return _documentation; }
        }

        // this property is public - accessed by an AST
        public ComTypeLibDesc TypeLib {
            get { return _typeLibDesc; }
        }

        internal Guid Guid {
            get { return _guid; }
            set { _guid = value; }
        }

        internal ComMethodDesc GetItem {
            get { return _getItem; }
            set { _getItem = value; }
        }

        internal ComMethodDesc SetItem {
            get { return _setItem; }
            set { _setItem = value; }
        }
    }
}

#endif
