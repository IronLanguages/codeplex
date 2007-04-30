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
using Microsoft.Scripting;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace IronPython.Runtime {
    public static partial class Symbols {
        private static Dictionary<SymbolId, FieldInfo> predefined;

        public const int ObjectKeysId = -2;
        public static readonly SymbolId ObjectKeys = new SymbolId(ObjectKeysId);

        internal static FieldInfo GetFieldInfo(SymbolId id) {
            FieldInfo fi;

            lock (predefined) {
                // Field is not predefined
                if (!predefined.TryGetValue(id, out fi)) {
                    return null;
                }
            }

            if (fi == null) {
                fi = GetField(id);
                Debug.Assert(fi != null);
                lock (predefined) {
                    predefined[id] = fi;
                }
            }
            return fi;
        }

        private static FieldInfo GetField(string name) {
            FieldInfo fi = typeof(Symbols).GetField(name);
            Debug.Assert(fi != null);
            return fi;
        }

        //
        // This can be only called from the static constructor - no synchornization here
        //
        private static SymbolId MakeSymbolId(string name) {
            if (predefined == null) {
                predefined = new Dictionary<SymbolId, FieldInfo>();
            }

            SymbolId id = SymbolTable.StringToId(name);
            predefined[id] = null;
            return id;
        }
    }
}
