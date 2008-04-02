/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

using IronPython.Runtime.Types;

namespace IronPython.Runtime.Calls {
    public interface ICallable {
        object Call(params object[] args);
    }

    public interface IFancyCallable {
        object Call(ICallerContext context, object[] args, string[] names);
    }

    public interface ICallableWithCallerContext {
        object Call(ICallerContext context, object[] args);
    }
    public interface IContextAwareMember {
        bool IsVisible(ICallerContext context);
    }

    public interface ICallerContext {
        PythonModule Module { get; }
        SystemState SystemState { get;}
        object Locals { get; }
        IAttributesDictionary Globals { get; }
        object GetStaticData(int index);
        bool TrueDivision { get; set; }
        CallerContextAttributes ContextFlags { get; set; }
        IronPython.Compiler.CompilerContext CreateCompilerContext();
    }
}