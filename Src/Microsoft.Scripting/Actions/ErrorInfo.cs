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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Encapsulates information about the result that should be produced when 
    /// a DynamicAction cannot be performed.  The ErrorInfo can hold one of:
    ///     an expression which creates an Exception to be thrown 
    ///     an expression which produces a value which should be returned 
    ///         directly to the user and represents an error has occured (for
    ///         example undefined in JavaScript)
    ///     an expression which produces a value which should be returned
    ///         directly to the user but does not actually represent an error.
    /// 
    /// ErrorInfo's are produced by an ActionBinder in response to a failed
    /// binding.  
    /// </summary>
    public sealed class ErrorInfo {
        private readonly Expression/*!*/ _value;
        private readonly ErrorInfoKind _kind;       

        /// <summary>
        /// Private constructor - consumers must use static From* factories
        /// to create ErrorInfo objects.
        /// </summary>
        private ErrorInfo(Expression/*!*/ value, ErrorInfoKind kind) {
            Debug.Assert(value != null);

            _value = value;
            _kind = kind;
        }

        /// <summary>
        /// Creates a new ErrorInfo which represents an exception that should
        /// be thrown.
        /// </summary>
        public static ErrorInfo FromException(Expression/*!*/ exceptionValue) {
            Contract.RequiresNotNull(exceptionValue, "exceptionValue");
            Contract.Requires(typeof(Exception).IsAssignableFrom(exceptionValue.Type), "exceptionValue", "must by an Exception instance");

            return new ErrorInfo(exceptionValue, ErrorInfoKind.Exception);
        }

        /// <summary>
        /// Creates a new ErrorInfo which represents a value which should be
        /// returned to the user.
        /// </summary>
        public static ErrorInfo FromValue(Expression/*!*/ resultValue) {
            Contract.RequiresNotNull(resultValue, "resultValue");

            return new ErrorInfo(resultValue, ErrorInfoKind.Error);
        }

        /// <summary>
        /// Crates a new ErrorInfo which represents a value which should be returned
        /// to the user but does not represent an error.
        /// </summary>
        /// <param name="resultValue"></param>
        /// <returns></returns>
        public static ErrorInfo FromValueNoError(Expression/*!*/ resultValue) {
            Contract.RequiresNotNull(resultValue, "resultValue");

            return new ErrorInfo(resultValue, ErrorInfoKind.Success);
        }

        /// <summary>
        /// Internal helper to produce the actual expression used for the error when emitting
        /// the error into a rule.
        /// </summary>
        public Expression MakeErrorForRule(StandardRule rule, ActionBinder binder) {
            switch(_kind) {
                case ErrorInfoKind.Error:
                    rule.IsError = true;
                    return rule.MakeReturn(binder, _value);
                case ErrorInfoKind.Success:
                    return rule.MakeReturn(binder, _value);
                case ErrorInfoKind.Exception:
                    return rule.MakeError(_value);
                default: throw new InvalidOperationException();
            }
        }

        public object GetValue(CodeContext context) {
            switch(_kind) {
                case ErrorInfoKind.Error:
                case ErrorInfoKind.Success:
                    return Interpreter.Interpreter.Evaluate(context, _value);                    
                case ErrorInfoKind.Exception:
                    throw (Exception)Interpreter.Interpreter.Evaluate(context, _value);
                default:
                    throw new InvalidOperationException();
            }
        }

        enum ErrorInfoKind {
            /// <summary>
            /// The ErrorInfo expression produces an exception
            /// </summary>
            Exception,
            /// <summary>
            /// The ErrorInfo expression produces a value which represents the error (e.g. undefined)
            /// </summary>
            Error,
            /// <summary>
            /// The ErrorInfo expression produces a value which is not an error
            /// </summary>
            Success
        }
    }
}
