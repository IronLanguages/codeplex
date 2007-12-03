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
using System.Text;
using Microsoft.Scripting;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Actions.ComDispatch {
    public class ComTypeDesc {
        private string _typeName;
        private Guid _guid;
        private Dictionary<SymbolId, ComDispatch.ComMethodDesc> _funcs;
        private Dictionary<SymbolId, ComEventDesc> _events;

        public ComTypeDesc(ComTypes.ITypeInfo typeInfo) {
            this._typeName = GetNameOfType(typeInfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // TODO: remove this when COM support is completely moved to the DLR
        public Dictionary<SymbolId, ComDispatch.ComMethodDesc> Funcs {
            get { return _funcs; }
            set { _funcs = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // TODO: remove this when COM support is completely moved to the DLR
        public Dictionary<SymbolId, ComEventDesc> Events {
            get { return _events; }
            set { _events = value; }
        }

        public string TypeName {
            get { return _typeName; }
        }

        public Guid Guid {
            get { return _guid; }
            set { _guid = value; }
        }

        public static string GetNameOfType(ComTypes.ITypeInfo typeInfo) {
            string name;
            string strDocString;
            int dwHelpContext;
            string strHelpFile;
            typeInfo.GetDocumentation(-1, out name, out strDocString, out dwHelpContext, out strHelpFile);
            return name;
        }
    }
}

#endif