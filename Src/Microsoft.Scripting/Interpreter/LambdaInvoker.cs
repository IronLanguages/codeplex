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

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Interpreter {
    /// <summary>
    /// Helper class used by the interpreter to package lambda as a delegate,
    /// allow it being called, and then resume interpretation.
    /// </summary>
    public class LambdaInvoker {
        private readonly LambdaExpression _lambda;
        private readonly CodeContext _context;

        internal LambdaInvoker(LambdaExpression lambda, CodeContext context) {
            _lambda = lambda;
            _context = context;
        }

        /// <summary>
        /// Triggers interpretation of the Lambda
        /// </summary>
        /// <param name="args">All arguments, except for the parameter array</param>
        /// <param name="array">Parameter array, if it was from the caller</param>
        public object Invoke(object[] args, object[] array) {
            return Interpreter.InterpretLambda(_context, _lambda, args, array);
        }
    }
}
