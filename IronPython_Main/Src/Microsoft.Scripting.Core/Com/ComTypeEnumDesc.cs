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

using Microsoft.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Actions;
using System.Globalization;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Com {
    // TODO: Can it be made internal?
    public sealed class ComTypeEnumDesc : ComTypeDesc, IDynamicObject {
        readonly string[] _memberNames;
        readonly object[] _memberValues;

        public override string ToString() {
            return String.Format(CultureInfo.CurrentCulture, "<enum '{0}'>", TypeName);
        }

        internal ComTypeEnumDesc(ComTypes.ITypeInfo typeInfo, ComTypeLibDesc typeLibDesc) :
            base(typeInfo, ComType.Enum, typeLibDesc) {
            ComTypes.TYPEATTR typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);
            string[] memberNames = new string[typeAttr.cVars];
            object[] memberValues = new object[typeAttr.cVars];

            IntPtr p = IntPtr.Zero;

            // For each enum member get name and value.
            for (int i = 0; i < typeAttr.cVars; i++) {
                typeInfo.GetVarDesc(i, out p);

                // Get the enum member value (as object).
                ComTypes.VARDESC varDesc;

                try {
                    varDesc = (ComTypes.VARDESC)Marshal.PtrToStructure(p, typeof(ComTypes.VARDESC));

                    if (varDesc.varkind == ComTypes.VARKIND.VAR_CONST) {
                        memberValues[i] = Marshal.GetObjectForNativeVariant(varDesc.desc.lpvarValue);
                    }
                } finally {
                    typeInfo.ReleaseVarDesc(p);
                }

                // Get the enum member name
                memberNames[i] = ComRuntimeHelpers.GetNameOfMethod(typeInfo, varDesc.memid);
            }

            _memberNames = memberNames;
            _memberValues = memberValues;
        }


        #region IDynamicObject Members

        MetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            return new TypeEnumMetaObject(this, parameter);
        }

        #endregion

        public object GetValue(string enumValueName) {
            for (int i = 0; i < _memberNames.Length; i++) {
                if (_memberNames[i] == enumValueName)
                    return _memberValues[i];
            }

            throw new MissingMemberException(enumValueName);
        }

        internal bool HasMember(string name) {
            for (int i = 0; i < _memberNames.Length; i++) {
                if (_memberNames[i] == name)
                    return true;
            }

            return false;
        }

        public string[] GetMemberNames() {
            return (string[])this._memberNames.Clone();
        }
    }
}

#endif
