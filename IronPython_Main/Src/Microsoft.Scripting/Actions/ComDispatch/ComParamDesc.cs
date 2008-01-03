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
using System.Runtime.InteropServices.ComTypes;
using VarEnum = System.Runtime.InteropServices.VarEnum;
using Marshal = System.Runtime.InteropServices.Marshal;
using System.Diagnostics;

namespace Microsoft.Scripting.Actions.ComDispatch {
    public class ComParamDesc {
        # region private fields

        private readonly bool _isOut; // is an output parameter?
        private readonly bool _isOpt; // is an optional parameter?
        private readonly bool _byRef; // is a reference or pointer parameter?
        private readonly bool _isArray;
        private readonly VarEnum _vt;
        private readonly string _name;
        private readonly Type _type;

        # endregion

        # region ctor

        /// <summary>
        /// Creates a representation for the paramter or return value of a COM method
        /// </summary>
        /// <param name="elemDesc"></param>
        /// <param name="name">This can be String.Empty for return values</param>
        public ComParamDesc(ELEMDESC elemDesc, string name) {
            _name = name;
            this._isOut = (elemDesc.desc.paramdesc.wParamFlags & PARAMFLAG.PARAMFLAG_FOUT) != 0;
            this._isOpt = (elemDesc.desc.paramdesc.wParamFlags & PARAMFLAG.PARAMFLAG_FOPT) != 0;

            _vt = (VarEnum)elemDesc.tdesc.vt;
            TYPEDESC typeDesc = elemDesc.tdesc;
            while (true) {
                if (_vt == VarEnum.VT_PTR) {
                    this._byRef = true;
                } else if (_vt == VarEnum.VT_ARRAY) {
                    this._isArray = true;
                } else {
                    break;
                }

                TYPEDESC childTypeDesc = (TYPEDESC)Marshal.PtrToStructure(typeDesc.lpValue, typeof(TYPEDESC));
                _vt = (VarEnum)childTypeDesc.vt;
                typeDesc = childTypeDesc;
            }

            VarEnum vtWithoutByref = _vt;
            if ((_vt & VarEnum.VT_BYREF) != 0) {
                vtWithoutByref = (_vt & ~VarEnum.VT_BYREF);
                _byRef = true;
            }

            _type = GetTypeForVarEnum(vtWithoutByref);
        }

        private Type GetTypeForVarEnum(VarEnum vt) {
            Type type;

            switch (vt) {
                // VarEnums which can be used in VARIANTs, but which cannot occur in a TYPEDESC
                case VarEnum.VT_EMPTY:
                case VarEnum.VT_NULL:
                case VarEnum.VT_RECORD:
                    string message = String.Format("Unexpected VarEnum {0} in ELEMDESC", vt);
                    throw new InvalidOperationException(message);

                // VarEnums which are not used in VARIANTs, but which can occur in a TYPEDESC
                case VarEnum.VT_VOID:
                    Debug.Assert(_name == String.Empty); // Only return values can have this
                    type = null;
                    break;

#if DISABLE // TODO: WTypes.h indicates that these cannot be used in VARIANTs, but Type.InvokeMember seems to allow it
                case VarEnum.VT_I8:
                case VarEnum.UI8:
#endif
                case VarEnum.VT_HRESULT:
                    type = typeof(int);
                    break;

                case ((VarEnum)37): // VT_INT_PTR:
                case VarEnum.VT_PTR:
                    type = typeof(IntPtr);
                    break;

                case ((VarEnum)38): // VT_UINT_PTR:
                    type = typeof(UIntPtr);
                    break;

                case VarEnum.VT_SAFEARRAY:
                case VarEnum.VT_CARRAY:
                    type = typeof(Array);
                    break;

                case VarEnum.VT_LPSTR:
                case VarEnum.VT_LPWSTR:
                    type = typeof(string);
                    break;

                case VarEnum.VT_USERDEFINED:
                    type = typeof(object);
                    break;

                // For VarEnums that can be used in VARIANTs and well as TYPEDESCs, just use VarEnumSelector
                default:
                    type = VarEnumSelector.GetManagedMarshalType(vt);
                    break;
            }

            return type;
        }

        public override string ToString() {
            StringBuilder result = new StringBuilder();
            if (_isOpt) {
                result.Append("[Optional] ");
            }

            if (_isOut) {
                result.Append("[out]");
            }

            result.Append(_type.Name);

            if (_isArray) {
                result.Append("[]");
            }

            if (_byRef) {
                result.Append("&");
            }

            result.Append(" ");
            result.Append(_name);

            return result.ToString();
        }

        # endregion

        # region properties

        public bool IsOut {
            get { return _isOut; }
        }

        public bool IsOptional {
            get { return _isOpt; }
        }

        public bool ByReference {
            get { return _byRef; }
        }

        public bool IsArray {
            get { return _isArray; }
        }

        public Type ParameterType {
            get {
                return _type;
            }
        }

        # endregion
    }
}

#endif
