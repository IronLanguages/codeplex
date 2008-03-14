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

using System;
using System.Collections.Generic;

using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Actions.ComDispatch {
    internal class ComTypeDesc {
        private string _typeName;
        private string _documentation;
        private Guid _guid;
        private Dictionary<SymbolId, ComDispatch.ComMethodDesc> _funcs;
        private Dictionary<SymbolId, ComEventDesc> _events;
        private ComMethodDesc _getItem;
        private ComMethodDesc _setItem;

        private static readonly Dictionary<SymbolId, ComEventDesc> _EmptyEventsDict = new Dictionary<SymbolId, ComEventDesc>();

        private ComTypeDesc() {
            _funcs = new Dictionary<SymbolId, ComMethodDesc>();
            _events = _EmptyEventsDict;
        }

        internal ComTypeDesc(ComTypes.ITypeInfo typeInfo) {
            GetInfoFromType(typeInfo, out _typeName, out _documentation);
        }

        internal static ComTypeDesc CreateEmptyTypeDesc() {
            return new ComTypeDesc();
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

        internal string TypeName {
            get { return _typeName; }
        }

        internal string Documentation {
            get { return _documentation; }
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

        internal static void GetInfoFromType(ComTypes.ITypeInfo typeInfo, out string name, out string documentation) {
            int dwHelpContext;
            string strHelpFile;
            typeInfo.GetDocumentation(-1, out name, out documentation, out dwHelpContext, out strHelpFile);
        }

        internal static string GetNameOfType(ComTypes.ITypeInfo typeInfo) {
            string name;
            string documentation;
            GetInfoFromType(typeInfo, out name, out documentation);
            return name;
        }
    }
}

#endif