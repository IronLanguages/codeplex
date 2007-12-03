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
        private readonly short _vt;
        Type type;

        # endregion

        # region ctor

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")] // TODO: fix
        public ComParamDesc(ELEMDESC elemDesc) {
            this._isOut = (elemDesc.desc.paramdesc.wParamFlags & PARAMFLAG.PARAMFLAG_FOUT) != 0;
            this._isOpt = (elemDesc.desc.paramdesc.wParamFlags & PARAMFLAG.PARAMFLAG_FOPT) != 0;

            _vt = elemDesc.tdesc.vt;
            TYPEDESC typeDesc = elemDesc.tdesc;
            while (true) {
                if (_vt == (short)VarEnum.VT_PTR) {
                    this._byRef = true;
                } else if (_vt == (short)VarEnum.VT_ARRAY) {
                    this._isArray = true;
                } else {
                    break;
                }

                TYPEDESC childTypeDesc = (TYPEDESC)Marshal.PtrToStructure(typeDesc.lpValue, typeof(TYPEDESC));
                _vt = childTypeDesc.vt;
                typeDesc = childTypeDesc;
            }

            // sometimes we will need to go and pass instances of the output parameters.
            // These instances will not be provided by the users, but we will need to create
            // those ourselves. To be able to do this we need to find out the type that we will
            // need to instantiate.
            // This logic should usually only apply to output params, since input data
            // is provided by the user.
            if (this._isOut == false) {
                return;
            }

            switch (_vt) {
                case (short)VarEnum.VT_EMPTY:
                    break;
                case (short)VarEnum.VT_I4:
                case (short)VarEnum.VT_INT:
                    this.type = typeof(Int32);
                    break;
                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_I4:
                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_INT:
                    this.type = typeof(Int32);
                    this._byRef = true;
                    break;
                case (short)VarEnum.VT_UI4:
                case (short)VarEnum.VT_UINT:
                    this.type = typeof(UInt32);
                    break;
                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_UI4:
                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_UINT:
                    this.type = typeof(UInt32);
                    this._byRef = true;
                    break;
                case (short)VarEnum.VT_I2:
                    this.type = typeof(Int16);
                    break;
                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_I2:
                    this.type = typeof(Int16);
                    this._byRef = true;
                    break;
                case (short)VarEnum.VT_UI2:
                    this.type = typeof(UInt16);
                    break;
                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_UI2:
                    this.type = typeof(UInt16);
                    this._byRef = true;
                    break;
                case (short)VarEnum.VT_I1:
                case (short)VarEnum.VT_UI1:
                    this.type = typeof(Byte);
                    break;
                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_I1:
                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_UI1:
                    this.type = typeof(Byte);
                    this._byRef = true;
                    break;
                case (short)VarEnum.VT_R4:
                    this.type = typeof(Single);
                    break;

                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_R4:
                    this.type = typeof(Single);
                    this._byRef = true;
                    break;

                case (short)VarEnum.VT_R8:
                    this.type = typeof(Double);
                    break;

                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_R8:
                    this.type = typeof(Double);
                    this._byRef = true;
                    break;

                case (short)VarEnum.VT_BOOL:
                    this.type = typeof(Boolean);
                    this._byRef = true;
                    break;

                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_BOOL:
                    this.type = typeof(Boolean);
                    this._byRef = true;
                    break;

                case (short)VarEnum.VT_BSTR:
                    this.type = typeof(String);
                    break;

                case (short)VarEnum.VT_BYREF | (short)VarEnum.VT_BSTR:
                    this.type = typeof(String);
                    this._byRef = true;
                    break;
                case (short)VarEnum.VT_UNKNOWN:
                case (short)VarEnum.VT_DISPATCH:
                case (short)VarEnum.VT_VARIANT:
                case (short)VarEnum.VT_USERDEFINED:
                    this.type = typeof(object);
                    break;
                default:
                    Debug.Assert(false, String.Format("{0} - VT is not handled", _vt));
                    break;
            }

            /*
            IntPtr varNative = IntPtr.Zero;
            try {
                Marshal.StructureToPtr(var, varNative, false);
                argsForCall[i] = Marshal.GetObjectForNativeVariant(varNative);
            } finally {
                if (varNative != IntPtr.Zero) {
                    Marshal.FreeHGlobal(varNative);
                }
            }
             */
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
                return type;
            }
        }

        # endregion
    }
}

#endif
