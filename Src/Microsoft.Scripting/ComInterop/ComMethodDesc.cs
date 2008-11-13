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
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Globalization;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Microsoft.Scripting.ComInterop {

    public class ComMethodDesc {

        private readonly bool _hasTypeInfo;
        private readonly int _memid;  // this is the member id extracted from FUNCDESC.memid
        private readonly string _name;
        internal readonly INVOKEKIND InvokeKind;
        private readonly ComParamDesc _returnValue;
        private readonly ComParamDesc[] _parameters;

        private ComMethodDesc(int dispId) {
            _memid = dispId;
        }

        internal ComMethodDesc(string name, int dispId)
            : this(dispId) {
            // no ITypeInfo constructor
            _name = name;
        }

        internal ComMethodDesc(string name, int dispId, INVOKEKIND invkind)
            : this(name, dispId) {
            InvokeKind = invkind;
        }

        internal ComMethodDesc(ITypeInfo typeInfo, FUNCDESC funcDesc)
            : this(funcDesc.memid) {

            _hasTypeInfo = true;
            InvokeKind = funcDesc.invkind;

            ELEMDESC returnValue = funcDesc.elemdescFunc;
            _returnValue = new ComParamDesc(ref returnValue);

            int cNames;
            string[] rgNames = new string[1 + funcDesc.cParams];
            typeInfo.GetNames(_memid, rgNames, rgNames.Length, out cNames);
            if (IsPropertyPut) {
                rgNames[rgNames.Length - 1] = "value";
                cNames++;
            }
            Debug.Assert(cNames == rgNames.Length);
            _name = rgNames[0];

            _parameters = new ComParamDesc[funcDesc.cParams];

            int offset = 0;
            for (int i = 0; i < funcDesc.cParams; i++) {
                ELEMDESC elemDesc = (ELEMDESC)Marshal.PtrToStructure(
                    new IntPtr(funcDesc.lprgelemdescParam.ToInt64() + offset),
                    typeof(ELEMDESC));

                _parameters[i] = new ComParamDesc(ref elemDesc, rgNames[1 + i]);

                offset += Marshal.SizeOf(typeof(ELEMDESC));
            }
        }

        internal bool HasTypeInfo {
            get {
                return _hasTypeInfo;
            }
        }

        internal string DispIdString {
            get {
                return String.Format(CultureInfo.InvariantCulture, "[DISPID={0}]", _memid);
            }
        }

        public string Name {
            get {
                Debug.Assert(_name != null);
                return _name;
            }
        }

        public int DispId {
            get { return _memid; }
        }

        public bool IsPropertyGet {
            get {
                return (InvokeKind & INVOKEKIND.INVOKE_PROPERTYGET) != 0;
            }
        }

        public bool IsPropertyPut {
            get {
                return (InvokeKind & (INVOKEKIND.INVOKE_PROPERTYPUT | INVOKEKIND.INVOKE_PROPERTYPUTREF)) != 0;
            }
        }

        public bool IsPropertyPutRef {
            get {
                return (InvokeKind & INVOKEKIND.INVOKE_PROPERTYPUTREF) != 0;
            }
        }

        internal ComParamDesc[] Parameters {
            get {
                Debug.Assert((_parameters != null) == _hasTypeInfo);
                return _parameters;  
            }
        }

        public bool HasByrefOrOutParameters {
            get {
                if (!_hasTypeInfo) {
                    // We have just a dispId and not ITypeInfo to get the list of parameters.
                    // We have to assume that all parameters are In parameters. The user will explicitly have 
                    // to pass StrongBox objects to represent ref or out parameters.
                    return false;
                }

                for (int i = 0; i < _parameters.Length; i++) {
                    if (_parameters[i].ByReference || _parameters[i].IsOut) {
                        return true;
                    }
                }

                return false;
            }
        }

        public string Signature {
            get {
                if (!_hasTypeInfo) {
                    return _name + "(...)";
                }

                StringBuilder result = new StringBuilder();
                if (_returnValue.ParameterType == null) {
                    result.Append("void");
                } else {
                    result.Append(_returnValue.ToString());
                }
                result.Append(" ");
                result.Append(_name);
                result.Append("(");
                for (int i = 0; i < _parameters.Length; i++) {
                    result.Append(_parameters[i].ToString());
                    if (i < (_parameters.Length - 1)) {
                        result.Append(", ");
                    }
                }
                result.Append(")");
                return result.ToString();
            }
        }

        public override string ToString() {
            return Signature;
        }
    }
}

#endif
