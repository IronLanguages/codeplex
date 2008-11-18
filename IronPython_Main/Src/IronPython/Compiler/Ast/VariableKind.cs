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
using System; using Microsoft;


namespace IronPython.Compiler.Ast {

    internal enum VariableKind {

        /// <summary>
        /// Local variable.
        /// 
        /// Local variables can be referenced from nested lambdas
        /// </summary>
        Local,

        /// <summary>
        /// Variables that weren't assigned within the using scope and were hoisted to the global scope.
        /// </summary>
        GlobalLocal,

        /// <summary>
        /// Variables that weren't assigned within the scope that contains unqualified exec, eval, locals().
        /// </summary>
        HiddenLocal,

        /// <summary>
        /// Parameter to a LambdaExpression
        /// 
        /// Like locals, they can be referenced from nested lambdas
        /// </summary>
        Parameter,

        /// <summary>
        /// Temporary variable
        /// 
        /// Temporaries must be contained within the lambda
        /// (cannot be referenced from nested lambdas)
        /// </summary>
        Temporary,

        /// <summary>
        /// Global variable
        /// 
        /// Should only appear in global (top level) lambda.
        /// </summary>
        Global
    };

}
