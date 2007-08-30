#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;

namespace IronPython.Runtime.Types.ComDispatch {
    class DispMethod : DispCallable, ICallableWithCodeContext {
        internal DispMethod(IDispatch dispatch, ComDispatch.ComMethodDesc methodDesc) 
            : base(dispatch, methodDesc) {
        }

        public object Call(CodeContext context, object[] args) {
            return base.Call(args);
        }
    }
}
#endif
