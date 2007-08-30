#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices.ComTypes;
using VarEnum = System.Runtime.InteropServices.VarEnum;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace IronPython.Runtime.Types.ComDispatch {
    class ComMethodDesc {

        # region private fields
        private readonly int _memid;  // this is the member id extracted from FUNCDESC.memid
        private readonly string _name;
        private readonly bool _isPropertyGet;
        private readonly bool _isPropertyPut;
        //private readonly int _mandatoryParamsCount;
        private readonly ComParamDesc[] _parameters;
        # endregion

        # region ctor
        /// <summary>
        /// ctor for defining functions.
        /// </summary>
        /// <param name="name"></param>
        public ComMethodDesc(string name) {
            // no ITypeInfo constructor
            _name = name;
        }

        public ComMethodDesc(ITypeInfo typeInfo, FUNCDESC funcDesc) {
            _memid = funcDesc.memid;

            int cNames;
            string[] rgNames = new string[1];
            typeInfo.GetNames(_memid, rgNames , 1, out cNames);
            _name = rgNames[0];

            _isPropertyGet = (funcDesc.invkind & INVOKEKIND.INVOKE_PROPERTYGET) != 0;
            _isPropertyPut = (funcDesc.invkind & INVOKEKIND.INVOKE_PROPERTYPUT) != 0;
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
        public string Name {
            get { return _name; }
        }

        public bool IsPropertyGet {
            get { return _isPropertyGet; }
        }

        public bool IsPropertyPut {
            get { return _isPropertyPut; }
        }

        internal ComParamDesc[] Parameters {
            get { return _parameters; }
        }
        # endregion 

    }
}
#endif
