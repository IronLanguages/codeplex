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

using Microsoft.Scripting;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

// This generated code is updated by the generate_exceptions.py script.
namespace IronPython.Runtime.Exceptions {
    public static partial class PythonExceptions {

        #region Generated Python New-Style Exceptions

        // *** BEGIN GENERATED CODE ***

        internal static PythonType _SystemExit = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(BaseException)), typeof(SystemExit), DefaultExceptionModule, "");

        [PythonSystemType("SystemExit"), Serializable]
        public partial class SystemExit : BaseException {
            private object _code;

            public SystemExit() : base(DynamicHelpers.GetPythonTypeFromType(typeof(SystemExit))) { }
            public SystemExit(PythonType type) : base(type) { }

            [StaticExtensionMethod("__new__")]
            public new static object __new__(PythonType cls, params object[] args) {
                return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
            }
            public object code {
                get { return _code; }
                set { _code = value; }
            }

        }

        public static readonly PythonType KeyboardInterrupt = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(BaseException)), "KeyboardInterrupt", "");
        public static readonly PythonType Exception = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(BaseException)), "Exception", "");
        public static readonly PythonType GeneratorExit = CreateSubType(Exception, "GeneratorExit", "");
        public static readonly PythonType StopIteration = CreateSubType(Exception, "StopIteration", "");
        public static readonly PythonType StandardError = CreateSubType(Exception, "StandardError", "");
        public static readonly PythonType ArithmeticError = CreateSubType(StandardError, "ArithmeticError", "");
        public static readonly PythonType FloatingPointError = CreateSubType(ArithmeticError, "FloatingPointError", "");
        public static readonly PythonType OverflowError = CreateSubType(ArithmeticError, "OverflowError", "");
        public static readonly PythonType ZeroDivisionError = CreateSubType(ArithmeticError, "ZeroDivisionError", "");
        public static readonly PythonType AssertionError = CreateSubType(StandardError, "AssertionError", "");
        public static readonly PythonType AttributeError = CreateSubType(StandardError, "AttributeError", "");
        internal static PythonType _EnvironmentError = CreateSubType(StandardError, typeof(EnvironmentError), DefaultExceptionModule, "");

        [PythonSystemType("EnvironmentError"), Serializable]
        public partial class EnvironmentError : BaseException {
            private object _errno;
            private object _strerror;
            private object _filename;

            public EnvironmentError() : base(DynamicHelpers.GetPythonTypeFromType(typeof(EnvironmentError))) { }
            public EnvironmentError(PythonType type) : base(type) { }

            [StaticExtensionMethod("__new__")]
            public new static object __new__(PythonType cls, params object[] args) {
                return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
            }
            public object errno {
                get { return _errno; }
                set { _errno = value; }
            }

            public object strerror {
                get { return _strerror; }
                set { _strerror = value; }
            }

            public object filename {
                get { return _filename; }
                set { _filename = value; }
            }

        }

        public static readonly PythonType IOError = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(EnvironmentError)), "IOError", "");
        public static readonly PythonType OSError = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(EnvironmentError)), "OSError", "");

        #if !SILVERLIGHT
        internal static PythonType _WindowsError = CreateSubType(OSError, typeof(WindowsError), DefaultExceptionModule, "");

        [PythonSystemType("WindowsError"), Serializable]
        public partial class WindowsError : EnvironmentError {
            private object _winerror;

            public WindowsError() : base(DynamicHelpers.GetPythonTypeFromType(typeof(WindowsError))) { }
            public WindowsError(PythonType type) : base(type) { }

            [StaticExtensionMethod("__new__")]
            public new static object __new__(PythonType cls, params object[] args) {
                return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
            }
            public object winerror {
                get { return _winerror; }
                set { _winerror = value; }
            }

        }

        #endif // !SILVERLIGHT

        public static readonly PythonType VMSError = CreateSubType(OSError, "VMSError", "");
        public static readonly PythonType EOFError = CreateSubType(StandardError, "EOFError", "");
        public static readonly PythonType ImportError = CreateSubType(StandardError, "ImportError", "");
        public static readonly PythonType LookupError = CreateSubType(StandardError, "LookupError", "");
        public static readonly PythonType IndexError = CreateSubType(LookupError, "IndexError", "");
        public static readonly PythonType KeyError = CreateSubType(LookupError, "KeyError", "");
        public static readonly PythonType MemoryError = CreateSubType(StandardError, "MemoryError", "");
        public static readonly PythonType NameError = CreateSubType(StandardError, "NameError", "");
        public static readonly PythonType UnboundLocalError = CreateSubType(NameError, "UnboundLocalError", "");
        public static readonly PythonType ReferenceError = CreateSubType(StandardError, "ReferenceError", "");
        public static readonly PythonType RuntimeError = CreateSubType(StandardError, "RuntimeError", "");
        public static readonly PythonType NotImplementedError = CreateSubType(RuntimeError, "NotImplementedError", "");
        internal static PythonType _SyntaxError = CreateSubType(StandardError, typeof(SyntaxError), DefaultExceptionModule, "");

        [PythonSystemType("SyntaxError"), Serializable]
        public partial class SyntaxError : BaseException {
            private object _text;
            private object _print_file_and_line;
            private object _filename;
            private object _lineno;
            private object _offset;
            private object _msg;

            public SyntaxError() : base(DynamicHelpers.GetPythonTypeFromType(typeof(SyntaxError))) { }
            public SyntaxError(PythonType type) : base(type) { }

            [StaticExtensionMethod("__new__")]
            public new static object __new__(PythonType cls, params object[] args) {
                return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
            }
            public object text {
                get { return _text; }
                set { _text = value; }
            }

            public object print_file_and_line {
                get { return _print_file_and_line; }
                set { _print_file_and_line = value; }
            }

            public object filename {
                get { return _filename; }
                set { _filename = value; }
            }

            public object lineno {
                get { return _lineno; }
                set { _lineno = value; }
            }

            public object offset {
                get { return _offset; }
                set { _offset = value; }
            }

            public object msg {
                get { return _msg; }
                set { _msg = value; }
            }

        }

        public static readonly PythonType IndentationError = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(SyntaxError)), "IndentationError", "");
        public static readonly PythonType TabError = CreateSubType(IndentationError, "TabError", "");
        public static readonly PythonType SystemError = CreateSubType(StandardError, "SystemError", "");
        public static readonly PythonType TypeError = CreateSubType(StandardError, "TypeError", "");
        public static readonly PythonType ValueError = CreateSubType(StandardError, "ValueError", "");
        public static readonly PythonType UnicodeError = CreateSubType(ValueError, "UnicodeError", "");

        #if !SILVERLIGHT
        internal static PythonType _UnicodeDecodeError = CreateSubType(UnicodeError, typeof(UnicodeDecodeError), DefaultExceptionModule, "");

        [PythonSystemType("UnicodeDecodeError"), Serializable]
        public partial class UnicodeDecodeError : BaseException {
            private object _start;
            private object _reason;
            private object _object;
            private object _end;
            private object _encoding;

            public UnicodeDecodeError() : base(DynamicHelpers.GetPythonTypeFromType(typeof(UnicodeDecodeError))) { }
            public UnicodeDecodeError(PythonType type) : base(type) { }

            [StaticExtensionMethod("__new__")]
            public new static object __new__(PythonType cls, params object[] args) {
                return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
            }
            public void __init__(object encoding, object @object, object start, object end, object reason) {
                _encoding = encoding;
                _object = @object;
                _start = start;
                _end = end;
                _reason = reason;
                args = PythonTuple.MakeTuple(encoding, @object, start, end, reason);
            }

            public override void __init__(params object[] args) {
                if (args == null || args.Length != 5) {
                    throw PythonOps.TypeError("__init__ takes exactly 5 arguments ({0} given)", args.Length);
                }
                __init__(encoding, @object, start, end, reason);
            }

            public object start {
                get { return _start; }
                set { _start = value; }
            }

            public object reason {
                get { return _reason; }
                set { _reason = value; }
            }

            public object @object {
                get { return _object; }
                set { _object = value; }
            }

            public object end {
                get { return _end; }
                set { _end = value; }
            }

            public object encoding {
                get { return _encoding; }
                set { _encoding = value; }
            }

        }

        #endif // !SILVERLIGHT


        #if !SILVERLIGHT
        internal static PythonType _UnicodeEncodeError = CreateSubType(UnicodeError, typeof(UnicodeEncodeError), DefaultExceptionModule, "");

        [PythonSystemType("UnicodeEncodeError"), Serializable]
        public partial class UnicodeEncodeError : BaseException {
            private object _start;
            private object _reason;
            private object _object;
            private object _end;
            private object _encoding;

            public UnicodeEncodeError() : base(DynamicHelpers.GetPythonTypeFromType(typeof(UnicodeEncodeError))) { }
            public UnicodeEncodeError(PythonType type) : base(type) { }

            [StaticExtensionMethod("__new__")]
            public new static object __new__(PythonType cls, params object[] args) {
                return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
            }
            public void __init__(object encoding, object @object, object start, object end, object reason) {
                _encoding = encoding;
                _object = @object;
                _start = start;
                _end = end;
                _reason = reason;
                args = PythonTuple.MakeTuple(encoding, @object, start, end, reason);
            }

            public override void __init__(params object[] args) {
                if (args == null || args.Length != 5) {
                    throw PythonOps.TypeError("__init__ takes exactly 5 arguments ({0} given)", args.Length);
                }
                __init__(encoding, @object, start, end, reason);
            }

            public object start {
                get { return _start; }
                set { _start = value; }
            }

            public object reason {
                get { return _reason; }
                set { _reason = value; }
            }

            public object @object {
                get { return _object; }
                set { _object = value; }
            }

            public object end {
                get { return _end; }
                set { _end = value; }
            }

            public object encoding {
                get { return _encoding; }
                set { _encoding = value; }
            }

        }

        #endif // !SILVERLIGHT

        internal static PythonType _UnicodeTranslateError = CreateSubType(UnicodeError, typeof(UnicodeTranslateError), DefaultExceptionModule, "");

        [PythonSystemType("UnicodeTranslateError"), Serializable]
        public partial class UnicodeTranslateError : BaseException {
            private object _start;
            private object _reason;
            private object _object;
            private object _end;
            private object _encoding;

            public UnicodeTranslateError() : base(DynamicHelpers.GetPythonTypeFromType(typeof(UnicodeTranslateError))) { }
            public UnicodeTranslateError(PythonType type) : base(type) { }

            [StaticExtensionMethod("__new__")]
            public new static object __new__(PythonType cls, params object[] args) {
                return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
            }
            public object start {
                get { return _start; }
                set { _start = value; }
            }

            public object reason {
                get { return _reason; }
                set { _reason = value; }
            }

            public object @object {
                get { return _object; }
                set { _object = value; }
            }

            public object end {
                get { return _end; }
                set { _end = value; }
            }

            public object encoding {
                get { return _encoding; }
                set { _encoding = value; }
            }

        }

        public static readonly PythonType Warning = CreateSubType(Exception, "Warning", "");
        public static readonly PythonType DeprecationWarning = CreateSubType(Warning, "DeprecationWarning", "");
        public static readonly PythonType PendingDeprecationWarning = CreateSubType(Warning, "PendingDeprecationWarning", "");
        public static readonly PythonType RuntimeWarning = CreateSubType(Warning, "RuntimeWarning", "");
        public static readonly PythonType SyntaxWarning = CreateSubType(Warning, "SyntaxWarning", "");
        public static readonly PythonType UserWarning = CreateSubType(Warning, "UserWarning", "");
        public static readonly PythonType FutureWarning = CreateSubType(Warning, "FutureWarning", "");
        public static readonly PythonType ImportWarning = CreateSubType(Warning, "ImportWarning", "");
        public static readonly PythonType UnicodeWarning = CreateSubType(Warning, "UnicodeWarning", "");
        public static readonly PythonType OverflowWarning = CreateSubType(Warning, "OverflowWarning", "");

        // *** END GENERATED CODE ***

        #endregion

        #region Generated ToPython Exception Helper

        // *** BEGIN GENERATED CODE ***

        private static BaseException/*!*/ ToPythonHelper(System.Exception clrException) {
            #if !SILVERLIGHT
            if (clrException is System.Text.DecoderFallbackException) return new UnicodeDecodeError();
            #endif
            if (clrException is System.DivideByZeroException) return new BaseException(ZeroDivisionError);
            #if !SILVERLIGHT
            if (clrException is System.Text.EncoderFallbackException) return new UnicodeEncodeError();
            #endif
            if (clrException is System.IO.EndOfStreamException) return new BaseException(EOFError);
            if (clrException is System.MissingMemberException) return new BaseException(AttributeError);
            if (clrException is System.OverflowException) return new BaseException(OverflowError);
            if (clrException is IronPython.Runtime.Exceptions.StopIterationException) return new BaseException(StopIteration);
            if (clrException is IronPython.Runtime.Exceptions.TabException) return new SyntaxError(TabError);
            #if !SILVERLIGHT
            if (clrException is System.ComponentModel.Win32Exception) return new WindowsError();
            #endif
            if (clrException is System.ArgumentException) return new BaseException(ValueError);
            if (clrException is System.ArithmeticException) return new BaseException(ArithmeticError);
            if (clrException is IronPython.Runtime.Exceptions.DeprecationWarningException) return new BaseException(DeprecationWarning);
            if (clrException is System.Runtime.InteropServices.ExternalException) return new EnvironmentError();
            if (clrException is IronPython.Runtime.Exceptions.FutureWarningException) return new BaseException(FutureWarning);
            if (clrException is System.IO.IOException) return new EnvironmentError(IOError);
            if (clrException is IronPython.Runtime.Exceptions.ImportWarningException) return new BaseException(ImportWarning);
            if (clrException is IronPython.Runtime.Exceptions.IndentationException) return new SyntaxError(IndentationError);
            if (clrException is System.IndexOutOfRangeException) return new BaseException(IndexError);
            if (clrException is System.Collections.Generic.KeyNotFoundException) return new BaseException(KeyError);
            if (clrException is System.NotImplementedException) return new BaseException(NotImplementedError);
            if (clrException is IronPython.Runtime.Exceptions.OSException) return new EnvironmentError(OSError);
            if (clrException is System.OutOfMemoryException) return new BaseException(MemoryError);
            if (clrException is IronPython.Runtime.Exceptions.PendingDeprecationWarningException) return new BaseException(PendingDeprecationWarning);
            if (clrException is IronPython.Runtime.Exceptions.RuntimeWarningException) return new BaseException(RuntimeWarning);
            if (clrException is IronPython.Runtime.Exceptions.SyntaxWarningException) return new BaseException(SyntaxWarning);
            if (clrException is Microsoft.Scripting.UnboundLocalException) return new BaseException(UnboundLocalError);
            if (clrException is IronPython.Runtime.Exceptions.UnicodeTranslateException) return new UnicodeTranslateError();
            if (clrException is IronPython.Runtime.Exceptions.UserWarningException) return new BaseException(UserWarning);
            if (clrException is System.ComponentModel.WarningException) return new BaseException(Warning);
            if (clrException is System.ApplicationException) return new BaseException(StandardError);
            if (clrException is Microsoft.Scripting.ArgumentTypeException) return new BaseException(TypeError);
            if (clrException is IronPython.Runtime.Exceptions.AssertionException) return new BaseException(AssertionError);
            if (clrException is IronPython.Runtime.Exceptions.FloatingPointException) return new BaseException(FloatingPointError);
            if (clrException is IronPython.Runtime.Exceptions.GeneratorExitException) return new BaseException(GeneratorExit);
            if (clrException is IronPython.Runtime.Exceptions.ImportException) return new BaseException(ImportError);
            if (clrException is Microsoft.Scripting.Shell.KeyboardInterruptException) return new BaseException(KeyboardInterrupt);
            if (clrException is IronPython.Runtime.Exceptions.LookupException) return new BaseException(LookupError);
            if (clrException is IronPython.Runtime.Exceptions.ReferenceException) return new BaseException(ReferenceError);
            if (clrException is IronPython.Runtime.Exceptions.RuntimeException) return new BaseException(RuntimeError);
            if (clrException is Microsoft.Scripting.SyntaxErrorException) return new SyntaxError();
            if (clrException is System.SystemException) return new BaseException(SystemError);
            if (clrException is IronPython.Runtime.Exceptions.SystemExitException) return new SystemExit();
            if (clrException is Microsoft.Scripting.UnboundNameException) return new BaseException(NameError);
            if (clrException is IronPython.Runtime.Exceptions.UnicodeException) return new BaseException(UnicodeError);
            if (clrException is IronPython.Runtime.Exceptions.UnicodeWarningException) return new BaseException(UnicodeWarning);
            return new BaseException(Exception);
        }
        private static System.Exception/*!*/ ToClrHelper(PythonType/*!*/ type, string message) {
            #if !SILVERLIGHT
            if (type == DynamicHelpers.GetPythonTypeFromType(typeof(UnicodeDecodeError))) return new System.Text.DecoderFallbackException(message);
            #endif
            if (type == ZeroDivisionError) return new System.DivideByZeroException(message);
            #if !SILVERLIGHT
            if (type == DynamicHelpers.GetPythonTypeFromType(typeof(UnicodeEncodeError))) return new System.Text.EncoderFallbackException(message);
            #endif
            if (type == EOFError) return new System.IO.EndOfStreamException(message);
            if (type == AttributeError) return new System.MissingMemberException(message);
            if (type == OverflowError) return new System.OverflowException(message);
            if (type == StopIteration) return new IronPython.Runtime.Exceptions.StopIterationException(message);
            if (type == TabError) return new IronPython.Runtime.Exceptions.TabException(message);
            #if !SILVERLIGHT
            if (type == DynamicHelpers.GetPythonTypeFromType(typeof(WindowsError))) return new System.ComponentModel.Win32Exception(message);
            #endif
            if (type == ValueError) return new System.ArgumentException(message);
            if (type == ArithmeticError) return new System.ArithmeticException(message);
            if (type == DeprecationWarning) return new IronPython.Runtime.Exceptions.DeprecationWarningException(message);
            if (type == DynamicHelpers.GetPythonTypeFromType(typeof(EnvironmentError))) return new System.Runtime.InteropServices.ExternalException(message);
            if (type == FutureWarning) return new IronPython.Runtime.Exceptions.FutureWarningException(message);
            if (type == IOError) return new System.IO.IOException(message);
            if (type == ImportWarning) return new IronPython.Runtime.Exceptions.ImportWarningException(message);
            if (type == IndentationError) return new IronPython.Runtime.Exceptions.IndentationException(message);
            if (type == IndexError) return new System.IndexOutOfRangeException(message);
            if (type == KeyError) return new System.Collections.Generic.KeyNotFoundException(message);
            if (type == NotImplementedError) return new System.NotImplementedException(message);
            if (type == OSError) return new IronPython.Runtime.Exceptions.OSException(message);
            if (type == MemoryError) return new System.OutOfMemoryException(message);
            if (type == PendingDeprecationWarning) return new IronPython.Runtime.Exceptions.PendingDeprecationWarningException(message);
            if (type == RuntimeWarning) return new IronPython.Runtime.Exceptions.RuntimeWarningException(message);
            if (type == SyntaxWarning) return new IronPython.Runtime.Exceptions.SyntaxWarningException(message);
            if (type == UnboundLocalError) return new Microsoft.Scripting.UnboundLocalException(message);
            if (type == DynamicHelpers.GetPythonTypeFromType(typeof(UnicodeTranslateError))) return new IronPython.Runtime.Exceptions.UnicodeTranslateException(message);
            if (type == UserWarning) return new IronPython.Runtime.Exceptions.UserWarningException(message);
            if (type == Warning) return new System.ComponentModel.WarningException(message);
            if (type == StandardError) return new System.ApplicationException(message);
            if (type == TypeError) return new Microsoft.Scripting.ArgumentTypeException(message);
            if (type == AssertionError) return new IronPython.Runtime.Exceptions.AssertionException(message);
            if (type == FloatingPointError) return new IronPython.Runtime.Exceptions.FloatingPointException(message);
            if (type == GeneratorExit) return new IronPython.Runtime.Exceptions.GeneratorExitException(message);
            if (type == ImportError) return new IronPython.Runtime.Exceptions.ImportException(message);
            if (type == KeyboardInterrupt) return new Microsoft.Scripting.Shell.KeyboardInterruptException(message);
            if (type == LookupError) return new IronPython.Runtime.Exceptions.LookupException(message);
            if (type == ReferenceError) return new IronPython.Runtime.Exceptions.ReferenceException(message);
            if (type == RuntimeError) return new IronPython.Runtime.Exceptions.RuntimeException(message);
            if (type == DynamicHelpers.GetPythonTypeFromType(typeof(SyntaxError))) return new Microsoft.Scripting.SyntaxErrorException(message);
            if (type == SystemError) return new System.SystemException(message);
            if (type == DynamicHelpers.GetPythonTypeFromType(typeof(SystemExit))) return new IronPython.Runtime.Exceptions.SystemExitException(message);
            if (type == NameError) return new Microsoft.Scripting.UnboundNameException(message);
            if (type == UnicodeError) return new IronPython.Runtime.Exceptions.UnicodeException(message);
            if (type == UnicodeWarning) return new IronPython.Runtime.Exceptions.UnicodeWarningException(message);
            if (type == Exception) return new System.Exception(message);
            return new Exception(message);
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
