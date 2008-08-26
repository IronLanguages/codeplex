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
using System.IO;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Threading;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting.Providers;

namespace Microsoft.Scripting.Hosting.Shell {

    /// <summary>
    /// Command line hosting service.
    /// </summary>
    public class CommandLine {
        private LanguageContext _language;
        private IConsole _console;
        private ConsoleOptions _options;
        private Scope _scope;
        private ScriptEngine _engine;

        protected IConsole Console { get { return _console; } }
        protected LanguageContext Language { get { return _language; } }
        protected ConsoleOptions Options { get { return _options; } }
        protected Scope Scope { get { return _scope; } set { _scope = value; } }

        protected virtual string Prompt { get { return ">>> "; } }
        protected virtual string PromptContinuation { get { return "... "; } }
        protected virtual string Logo { get { return null; } }

        public CommandLine() {
        }

        protected virtual void Initialize() {
        }

        protected virtual Scope CreateScope() {
            return new Scope();
        }

        /// <summary>
        /// Executes the comand line - depending upon the options provided we will
        /// either run a single file, a single command, or enter the interactive loop.
        /// </summary>
        public int Run(ScriptEngine engine, IConsole console, ConsoleOptions options) {
            ContractUtils.RequiresNotNull(engine, "engine");
            ContractUtils.RequiresNotNull(console, "console");
            ContractUtils.RequiresNotNull(options, "options");

            _language = HostingHelpers.GetLanguageContext(_engine = engine);
            _options = options;
            _console = console;

            Initialize();

            try {
                return Run();

#if !SILVERLIGHT // ThreadAbortException.ExceptionState
            } catch (System.Threading.ThreadAbortException tae) {
                if (tae.ExceptionState is KeyboardInterruptException) {
                    Thread.ResetAbort();
                }
                return -1;
#endif
            } finally {
                Shutdown();
                _language = null;
                _options = null;
                _console = null;
            }
        }

        /// <summary>
        /// Runs the command line.  Languages can override this to provide custom behavior other than:
        ///     1. Running a single command
        ///     2. Running a file
        ///     3. Entering the interactive console loop.
        /// </summary>
        /// <returns></returns>
        protected virtual int Run() {
            int result;

            if (_options.Command != null) {
                result = RunCommand(_options.Command);
            } else if (_options.FileName != null) {
                result = RunFile(_options.FileName);
            } else {
                return RunInteractive();
            }

            if (_options.Introspection) {
                return RunInteractiveLoop();
            }

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void Shutdown() {
            try {
                _language.Shutdown();
            } catch (Exception e) {
                UnhandledException(e);
            }
        }

        protected virtual int RunFile(string fileName) {
            return RunFile(_language.CreateFileUnit(fileName));
        }

        protected virtual int RunCommand(string command) {
            return RunFile(_language.CreateSnippet(command));
        }

        /// <summary>
        /// Runs the specified filename
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual int RunFile(SourceUnit source) {
            int result = 1;

            if (Options.HandleExceptions) {
                try {
                    result = source.ExecuteProgram();
                } catch (Exception e) {
                    UnhandledException(e);
                }
            } else {
                result = source.ExecuteProgram();
            }

            return result;
        }

        protected void PrintLogo() {
            _console.Write(Logo, Style.Out);
        }

        #region Interactivity

        /// <summary>
        /// Starts the interactive loop.  Performs any initialization necessary before
        /// starting the loop and then calls RunInteractiveLoop to start the loop.
        /// 
        /// Returns the exit code when the interactive loop is completed.
        /// </summary>
        protected virtual int RunInteractive() {
            PrintLogo();
            return RunInteractiveLoop();
        }

        /// <summary>
        /// Runs the interactive loop.  Repeatedly parse and run interactive actions
        /// until an exit code is received.  If any exceptions are unhandled displays
        /// them to the console
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected int RunInteractiveLoop() {
            if (_scope == null) {
                _scope = CreateScope();
            }

            int? res = null;
            do {
                if (Options.HandleExceptions) {
                    try {
                        res = TryInteractiveAction();
#if SILVERLIGHT 
                    } catch (ExitProcessException e) {
                        res = e.ExitCode;
#endif
                    } catch (Exception e) {
                        // There should be no unhandled exceptions in the interactive session
                        // We catch all exceptions here, and just display it,
                        // and keep on going
                        UnhandledException(e);
                    }
                } else {
                    res = TryInteractiveAction();
                }

            } while (res == null);

            return res.Value;
        }

        protected virtual void UnhandledException(Exception e) {
            _console.WriteLine(_language.FormatException(e), Style.Error);
        }

        /// <summary>
        /// Attempts to run a single interaction and handle any language-specific
        /// exceptions.  Base classes can override this and call the base implementation
        /// surrounded with their own exception handling.
        /// 
        /// Returns null if successful and execution should continue, or an exit code.
        /// </summary>
        protected virtual int? TryInteractiveAction() {
            int? result = null;

            try {
                result = RunOneInteraction();
#if SILVERLIGHT // ThreadAbortException.ExceptionState
            } catch (ThreadAbortException) {
#else
            } catch (ThreadAbortException tae) {
                KeyboardInterruptException pki = tae.ExceptionState as KeyboardInterruptException;
                if (pki != null) {
                    UnhandledException(tae);
                    Thread.ResetAbort();
                }
#endif
            }

            return result;
        }

        /// <summary>
        /// Parses a single interactive command and executes it.  
        /// 
        /// Returns null if successful and execution should continue, or the appropiate exit code.
        /// </summary>
        private int? RunOneInteraction() {
            bool continueInteraction;
            string s = ReadStatement(out continueInteraction);

            if (continueInteraction == false)
                return 0;

            if (String.IsNullOrEmpty(s)) {
                // Is it an empty line?
                _console.Write(String.Empty, Style.Out);
                return null;
            }

            ExecuteCommand(_language.CreateSnippet(s, SourceCodeKind.InteractiveCode));
            return null;
        }

        protected object ExecuteCommand(SourceUnit source) {
            return source.Compile(_language.GetCompilerOptions(_scope), ErrorSink).Run(_scope);
        }

        protected virtual ErrorSink ErrorSink {
            get { return ErrorSink.Default; }
        }

        /// <summary>
        /// Private helper function to see if we should treat the current input as a blank link.
        /// 
        /// We do this if we only have auto-indent text.
        /// </summary>
        private static bool TreatAsBlankLine(string line, int autoIndentSize) {
            if (line.Length == 0) return true;
            if (autoIndentSize != 0 && line.Trim().Length == 0 && line.Length == autoIndentSize) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Read a statement, which can potentially be a multiple-line statement suite (like a class declaration).
        /// </summary>
        /// <param name="continueInteraction">Should the console session continue, or did the user indicate 
        /// that it should be terminated?</param>
        /// <returns>Expression to evaluate. null for empty input</returns>
        protected string ReadStatement(out bool continueInteraction) {
            StringBuilder b = new StringBuilder();
            int autoIndentSize = 0;

            _console.Write(Prompt, Style.Prompt);

            while (true) {
                string line = ReadLine(autoIndentSize);
                continueInteraction = true;

                if (line == null) {
                    continueInteraction = false;
                    return null;
                }

                bool allowIncompleteStatement = TreatAsBlankLine(line, autoIndentSize);
                b.Append(line);
                b.Append("\n");

                string code = b.ToString();

                SourceUnit command = _language.CreateSnippet(code, SourceCodeKind.InteractiveCode);
                ScriptCodeParseResult props = command.GetCodeProperties(_language.GetCompilerOptions(_scope));

                if (SourceCodePropertiesUtils.IsCompleteOrInvalid(props, allowIncompleteStatement)) {
                    return props != ScriptCodeParseResult.Empty ? code : null;
                }

                if (_options.AutoIndent && _options.AutoIndentSize != 0) {
                    autoIndentSize = GetNextAutoIndentSize(code);
                }

                // Keep on reading input
                _console.Write(PromptContinuation, Style.Prompt);
            }
        }

        /// <summary>
        /// Gets the next level for auto-indentation
        /// </summary>
        protected virtual int GetNextAutoIndentSize(string text) {
            return 0;
        }

        protected virtual string ReadLine(int autoIndentSize) {
            return _console.ReadLine(autoIndentSize);
        }

        internal protected virtual TextWriter GetOutputWriter(bool isErrorOutput) {
            return isErrorOutput ? System.Console.Error : System.Console.Out;
        }

        //private static DynamicSite<object, IList<string>>  _memberCompletionSite =
        //    new DynamicSite<object, IList<string>>(OldDoOperationAction.Make(Operators.GetMemberNames));

        public IList<string> GetMemberNames(string code) {
            object value = _language.CreateSnippet(code, SourceCodeKind.Expression).Execute(_scope);
            return _engine.Operations.GetMemberNames(value);
            // TODO: why doesn't this work ???
            //return _memberCompletionSite.Invoke(new CodeContext(_scope, _language), value);
        }

        public virtual IList<string> GetGlobals(string name) {
            List<string> res = new List<string>();
            foreach (SymbolId scopeName in _scope.Keys) {
                string strName = SymbolTable.IdToString(scopeName);
                if (strName.StartsWith(name)) {
                    res.Add(strName);
                }
            }

            return res;
        }

        #endregion
    }


}
