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
using System; using Microsoft;


#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using Microsoft.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Binders;
using System.Globalization;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.ComInterop {

    internal sealed class ComTypeLibDesc {

        // typically typelibs contain very small number of coclasses
        // so we will just use the linked list as it performs better
        // on small number of entities
        LinkedList<ComTypeClassDesc> _classes;
        Dictionary<string, ComTypeEnumDesc> _enums;
        string _typeLibName;
        ComTypes.TYPELIBATTR _typeLibAttributes;

        private static Dictionary<Guid, ComTypeLibDesc> _CachedTypeLibDesc = new Dictionary<Guid, ComTypeLibDesc>();

        private ComTypeLibDesc() {
            _enums = new Dictionary<string, ComTypeEnumDesc>();
            _classes = new LinkedList<ComTypeClassDesc>();
        }

        public override string ToString() {
            return String.Format(CultureInfo.CurrentCulture, "<type library {0}>", _typeLibName);
        }

        internal static ComTypeLibDesc GetFromTypeLib(ComTypes.ITypeLib typeLib) {
            // check whether we have already loaded this type library
            ComTypes.TYPELIBATTR typeLibAttr = ComRuntimeHelpers.GetTypeAttrForTypeLib(typeLib);
            ComTypeLibDesc typeLibDesc;
            lock (_CachedTypeLibDesc) {
                if (_CachedTypeLibDesc.TryGetValue(typeLibAttr.guid, out typeLibDesc)) {
                    return typeLibDesc;
                }
            }

            typeLibDesc = new ComTypeLibDesc();

            typeLibDesc._typeLibName = ComRuntimeHelpers.GetNameOfLib(typeLib);
            typeLibDesc._typeLibAttributes = typeLibAttr;

            int countTypes = typeLib.GetTypeInfoCount();
            for (int i = 0; i < countTypes; i++) {
                ComTypes.TYPEKIND typeKind;
                typeLib.GetTypeInfoType(i, out typeKind);

                ComTypes.ITypeInfo typeInfo;
                if (typeKind == ComTypes.TYPEKIND.TKIND_COCLASS) {
                    typeLib.GetTypeInfo(i, out typeInfo);
                    ComTypeClassDesc classDesc = new ComTypeClassDesc(typeInfo, typeLibDesc);
                    typeLibDesc._classes.AddLast(classDesc);
                } else if (typeKind == ComTypes.TYPEKIND.TKIND_ENUM) {
                    typeLib.GetTypeInfo(i, out typeInfo);
                    ComTypeEnumDesc enumDesc = new ComTypeEnumDesc(typeInfo, typeLibDesc);
                    typeLibDesc._enums.Add(enumDesc.TypeName, enumDesc);
                }
            }

            // cached the typelib using the guid as the dictionary key
            lock (_CachedTypeLibDesc) {
                _CachedTypeLibDesc.Add(typeLibAttr.guid, typeLibDesc);
            }

            return typeLibDesc;
        }

        internal object GetTypeLibObjectDesc(string member) {
            foreach (ComTypeClassDesc coclass in _classes) {
                if (member == coclass.TypeName) {
                    return coclass;
                }
            }

            ComTypeEnumDesc enumDesc;
            if (_enums != null && _enums.TryGetValue(member, out enumDesc) == true)
                return enumDesc;

            return null;
        }

        internal string[] GetMemberNames() {
            string[] retval = new string[_enums.Count + _classes.Count];
            int i = 0;

            foreach (ComTypeClassDesc coclass in _classes) {
                retval[i++] = coclass.TypeName;
            }

            foreach (KeyValuePair<string, ComTypeEnumDesc> enumDesc in _enums) {
                retval[i++] = enumDesc.Key;
            }

            return retval;
        }

        internal bool HasMember(string member) {
            foreach (ComTypeClassDesc coclass in _classes) {
                if (member == coclass.TypeName) {
                    return true;
                }
            }

            if (_enums.ContainsKey(member) == true)
                return true;

            return false;
        }

        public Guid Guid {
            get { return _typeLibAttributes.guid; }
        }

        public short VersionMajor {
            get { return _typeLibAttributes.wMajorVerNum; }
        }

        public short VersionMinor {
            get { return _typeLibAttributes.wMinorVerNum; }
        }

        public string Name {
            get { return _typeLibName; }
        }

        internal ComTypeClassDesc GetCoClassForInterface(string itfName) {
            foreach (ComTypeClassDesc coclass in _classes) {
                if (coclass.Implements(itfName, false)) {
                    return coclass;
                }
            }

            return null;
        }
    }
}

#endif
