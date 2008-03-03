/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace Microsoft.Scripting.Ast {

    public enum VariableKind {

        /// <summary>
        /// Local variable.
        /// 
        /// Local variables can be referenced from nested CodeBlocks
        /// </summary>
        Local,

        /// <summary>
        /// Parameter to a CodeBlock
        /// 
        /// Like locals, they can be referenced from nested CodeBlocks
        /// </summary>
        Parameter,

        /// <summary>
        /// Temporary variable
        /// 
        /// Temporaries must be contained within the code block
        /// (cannot be referenced from nested code blocks)
        /// </summary>
        Temporary,

        /// <summary>
        /// Global variable
        /// 
        /// Should only appear in global (top level) code block.
        /// TODO: Pythonism, globals should go away and be handled on Python side only
        /// </summary>
        Global
    };

}