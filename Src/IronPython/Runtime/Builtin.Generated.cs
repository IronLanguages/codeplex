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

        public static PythonType SystemExit = PythonExceptions._SystemExit;
        public static PythonType KeyboardInterrupt = PythonExceptions.KeyboardInterrupt;
        public static PythonType Exception = PythonExceptions.Exception;
        public static PythonType GeneratorExit = PythonExceptions.GeneratorExit;
        public static PythonType StopIteration = PythonExceptions.StopIteration;
        public static PythonType StandardError = PythonExceptions.StandardError;
        public static PythonType ArithmeticError = PythonExceptions.ArithmeticError;
        public static PythonType FloatingPointError = PythonExceptions.FloatingPointError;
        public static PythonType OverflowError = PythonExceptions.OverflowError;
        public static PythonType ZeroDivisionError = PythonExceptions.ZeroDivisionError;
        public static PythonType AssertionError = PythonExceptions.AssertionError;
        public static PythonType AttributeError = PythonExceptions.AttributeError;
        public static PythonType EnvironmentError = PythonExceptions._EnvironmentError;
        public static PythonType IOError = PythonExceptions.IOError;
        public static PythonType OSError = PythonExceptions.OSError;

        #if !SILVERLIGHT
        public static PythonType WindowsError = PythonExceptions._WindowsError;
        #endif // !SILVERLIGHT

        public static PythonType VMSError = PythonExceptions.VMSError;
        public static PythonType EOFError = PythonExceptions.EOFError;
        public static PythonType ImportError = PythonExceptions.ImportError;
        public static PythonType LookupError = PythonExceptions.LookupError;
        public static PythonType IndexError = PythonExceptions.IndexError;
        public static PythonType KeyError = PythonExceptions.KeyError;
        public static PythonType MemoryError = PythonExceptions.MemoryError;
        public static PythonType NameError = PythonExceptions.NameError;
        public static PythonType UnboundLocalError = PythonExceptions.UnboundLocalError;
        public static PythonType ReferenceError = PythonExceptions.ReferenceError;
        public static PythonType RuntimeError = PythonExceptions.RuntimeError;
        public static PythonType NotImplementedError = PythonExceptions.NotImplementedError;
        public static PythonType SyntaxError = PythonExceptions._SyntaxError;
        public static PythonType IndentationError = PythonExceptions.IndentationError;
        public static PythonType TabError = PythonExceptions.TabError;
        public static PythonType SystemError = PythonExceptions.SystemError;
        public static PythonType TypeError = PythonExceptions.TypeError;
        public static PythonType ValueError = PythonExceptions.ValueError;
        public static PythonType UnicodeError = PythonExceptions.UnicodeError;

        #if !SILVERLIGHT
        public static PythonType UnicodeDecodeError = PythonExceptions._UnicodeDecodeError;
        #endif // !SILVERLIGHT


        #if !SILVERLIGHT
        public static PythonType UnicodeEncodeError = PythonExceptions._UnicodeEncodeError;
        #endif // !SILVERLIGHT

        public static PythonType UnicodeTranslateError = PythonExceptions._UnicodeTranslateError;
        public static PythonType Warning = PythonExceptions.Warning;
        public static PythonType DeprecationWarning = PythonExceptions.DeprecationWarning;
        public static PythonType PendingDeprecationWarning = PythonExceptions.PendingDeprecationWarning;
        public static PythonType RuntimeWarning = PythonExceptions.RuntimeWarning;
        public static PythonType SyntaxWarning = PythonExceptions.SyntaxWarning;
        public static PythonType UserWarning = PythonExceptions.UserWarning;
        public static PythonType FutureWarning = PythonExceptions.FutureWarning;
        public static PythonType ImportWarning = PythonExceptions.ImportWarning;
        public static PythonType UnicodeWarning = PythonExceptions.UnicodeWarning;
        public static PythonType OverflowWarning = PythonExceptions.OverflowWarning;

        // *** END GENERATED CODE ***

        #endregion

    }
}
