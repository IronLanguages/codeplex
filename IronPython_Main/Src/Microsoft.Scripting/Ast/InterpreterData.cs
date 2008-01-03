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

using System;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// This class stores information used by the interpreter to execute code blocks.
    /// </summary>
    internal class InterpreterData {
        // Interpreted mode: Cache for emitted delegate so that we only generate code once.
        public Delegate Delegate;

        // Profile-driven compilation support
        public int CallCount = 0;
        public CompilerContext DeclaringContext;
        public bool ForceWrapperMethod;

        public const int MaxInterpretedCalls = 2;
    }
}
