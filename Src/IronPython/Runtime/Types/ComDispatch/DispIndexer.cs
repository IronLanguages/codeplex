#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;

namespace IronPython.Runtime.Types.ComDispatch {
    public class DispIndexer : DispCallable, ICallableWithCodeContext {
        internal DispIndexer(IDispatch dispatch, ComDispatch.ComMethodDesc methodDesc)
            : base(dispatch, methodDesc) {
        }

        public object this[params object[] args] {
            get { return base.Call(args); }
        }

        #region ICallableWithCodeContext Members

        public object Call(CodeContext context, object[] args) {
            return base.Call(args);
        }

        #endregion
    }
}
#endif
