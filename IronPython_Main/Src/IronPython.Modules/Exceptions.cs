/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronPython.Runtime;

// 
// To add new builtin exceptions:
//
// 1) Add a new entry to ExceptionConverter.ExceptionMapping
// 2) Add a static field to PythonExceptions module in the generated region.
// 3) build ipy
// 4) Run the generator script using the ipy you just built
//   ipyd generate_exceptions.py
// That will read the exception types from the currently running ipy, detect the new exception, and emit new 
// source files (rewriting what you did in step #2).
[assembly: PythonModule("exceptions", typeof(IronPython.Modules.PythonExceptions))]
namespace IronPython.Modules {
    [PythonType("exceptions")]
    public static partial class PythonExceptions {
    }
}
