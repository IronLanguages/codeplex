using System;
using System.Collections.Generic;
using System.Text;

namespace IronPython.Runtime.Exceptions {
    public interface IPythonException {
        object ToPythonException();
    }
}
