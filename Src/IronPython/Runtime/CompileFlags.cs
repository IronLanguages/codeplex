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

using System; using Microsoft;

namespace IronPython.Runtime {
    [Flags]
    public enum CompileFlags {
        CO_NESTED = 0x0010,              //  nested_scopes
        CO_DONT_IMPLY_DEDENT = 0x0200,   // report errors if statement isn't dedented.
        CO_GENERATOR_ALLOWED = 0x1000,   //  generators
        CO_FUTURE_DIVISION = 0x2000,   //  division
        CO_FUTURE_ABSOLUTE_IMPORT = 0x4000, // absolute imports by default
        CO_FUTURE_WITH_STATEMENT = 0x8000,  // with statement
    }
}
