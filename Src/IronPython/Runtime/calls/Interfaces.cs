/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

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