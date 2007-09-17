/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;

namespace IronPython.Runtime.Calls {
    [Flags]
    public enum CompileFlags {
        CO_NESTED = 0x0010,              //  nested_scopes
        CO_GENERATOR_ALLOWED = 0x1000,   //  generators
        CO_FUTURE_DIVISION = 0x2000,   //  division
    }
}
