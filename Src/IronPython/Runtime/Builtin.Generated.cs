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
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    public static partial class Builtin {
        #region Generated builtin exceptions

        // *** BEGIN GENERATED CODE ***

        public static object SystemExit = PythonExceptions._SystemExit;
        public static object KeyboardInterrupt = PythonExceptions.KeyboardInterrupt;
        public static object Exception = PythonExceptions.Exception;
        public static object GeneratorExit = PythonExceptions.GeneratorExit;
        public static object StopIteration = PythonExceptions.StopIteration;
        public static object StandardError = PythonExceptions.StandardError;
        public static object ArithmeticError = PythonExceptions.ArithmeticError;
        public static object FloatingPointError = PythonExceptions.FloatingPointError;
        public static object OverflowError = PythonExceptions.OverflowError;
        public static object ZeroDivisionError = PythonExceptions.ZeroDivisionError;
        public static object AssertionError = PythonExceptions.AssertionError;
        public static object AttributeError = PythonExceptions.AttributeError;
        public static object EnvironmentError = PythonExceptions._EnvironmentError;
        public static object IOError = PythonExceptions.IOError;
        public static object OSError = PythonExceptions.OSError;

        #if !SILVERLIGHT
        public static object WindowsError = PythonExceptions._WindowsError;
        #endif // !SILVERLIGHT

        public static object VMSError = PythonExceptions.VMSError;
        public static object EOFError = PythonExceptions.EOFError;
        public static object ImportError = PythonExceptions.ImportError;
        public static object LookupError = PythonExceptions.LookupError;
        public static object IndexError = PythonExceptions.IndexError;
        public static object KeyError = PythonExceptions.KeyError;
        public static object MemoryError = PythonExceptions.MemoryError;
        public static object NameError = PythonExceptions.NameError;
        public static object UnboundLocalError = PythonExceptions.UnboundLocalError;
        public static object ReferenceError = PythonExceptions.ReferenceError;
        public static object RuntimeError = PythonExceptions.RuntimeError;
        public static object NotImplementedError = PythonExceptions.NotImplementedError;
        public static object SyntaxError = PythonExceptions._SyntaxError;
        public static object IndentationError = PythonExceptions.IndentationError;
        public static object TabError = PythonExceptions.TabError;
        public static object SystemError = PythonExceptions.SystemError;
        public static object TypeError = PythonExceptions.TypeError;
        public static object ValueError = PythonExceptions.ValueError;
        public static object UnicodeError = PythonExceptions.UnicodeError;

        #if !SILVERLIGHT
        public static object UnicodeDecodeError = PythonExceptions._UnicodeDecodeError;
        #endif // !SILVERLIGHT


        #if !SILVERLIGHT
        public static object UnicodeEncodeError = PythonExceptions._UnicodeEncodeError;
        #endif // !SILVERLIGHT

        public static object UnicodeTranslateError = PythonExceptions._UnicodeTranslateError;
        public static object Warning = PythonExceptions.Warning;
        public static object DeprecationWarning = PythonExceptions.DeprecationWarning;
        public static object PendingDeprecationWarning = PythonExceptions.PendingDeprecationWarning;
        public static object RuntimeWarning = PythonExceptions.RuntimeWarning;
        public static object SyntaxWarning = PythonExceptions.SyntaxWarning;
        public static object UserWarning = PythonExceptions.UserWarning;
        public static object FutureWarning = PythonExceptions.FutureWarning;
        public static object ImportWarning = PythonExceptions.ImportWarning;
        public static object UnicodeWarning = PythonExceptions.UnicodeWarning;
        public static object OverflowWarning = PythonExceptions.OverflowWarning;

        // *** END GENERATED CODE ***

        #endregion

    }
}
