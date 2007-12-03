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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices.ComTypes;
using VarEnum = System.Runtime.InteropServices.VarEnum;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Microsoft.Scripting.Actions.ComDispatch {
    public class ComMethodDesc {
        # region private fields

        private readonly bool _hasTypeInfo;
        private readonly int _memid;  // this is the member id extracted from FUNCDESC.memid
        private readonly string _name;
        private readonly string _dispidOrName;
        private readonly bool _isPropertyGet;
        private readonly bool _isPropertyPut;
        //private readonly int _mandatoryParamsCount;
        private readonly ComParamDesc[] _parameters;

        # endregion

        # region ctor

        private ComMethodDesc(int dispId) {
            _memid = dispId;
            _dispidOrName = String.Format("[DISPID={0}]", dispId);
        }

        public ComMethodDesc(string name, int dispId)
            : this(dispId) {
            // no ITypeInfo constructor
            _hasTypeInfo = false;
            _name = name;
        }

        public ComMethodDesc(ITypeInfo typeInfo, FUNCDESC funcDesc)
            : this(funcDesc.memid) {

            _hasTypeInfo = true;

            int cNames;
            string[] rgNames = new string[1];
            typeInfo.GetNames(_memid, rgNames, 1, out cNames);
            _name = rgNames[0];

            _isPropertyGet = (funcDesc.invkind & INVOKEKIND.INVOKE_PROPERTYGET) != 0;
            _isPropertyPut = (funcDesc.invkind & (INVOKEKIND.INVOKE_PROPERTYPUT | INVOKEKIND.INVOKE_PROPERTYPUTREF)) != 0;
            _parameters = new ComParamDesc[funcDesc.cParams];

            int offset = 0;
            for (int i = 0; i < funcDesc.cParams; i++) {
                ELEMDESC elemDesc = (ELEMDESC)Marshal.PtrToStructure(
                    new IntPtr(funcDesc.lprgelemdescParam.ToInt64() + offset),
                    typeof(ELEMDESC));

                _parameters[i] = new ComParamDesc(elemDesc);

                offset += Marshal.SizeOf(typeof(ELEMDESC));
            }
        }

        # endregion

        # region properties

        internal bool HasTypeInfo {
            get {
                return _hasTypeInfo;
            }
        }

        public string DispIdOrName {
            get {
                Debug.Assert(_dispidOrName != null);
                return _dispidOrName; 
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
            get { return _isPropertyGet; }
        }

        public bool IsPropertyPut {
            get { return _isPropertyPut; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: Remove this when ComMethodDesc is made internal
        public ComParamDesc[] Parameters {
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

        # endregion
    }
}

#endif
