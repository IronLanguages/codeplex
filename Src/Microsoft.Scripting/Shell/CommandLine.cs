/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Shell {

    public class CommandLine {
        private IConsole _console;
        private IScriptEngine _engine;
        private ConsoleOptions _options;
        private ScriptModule _module;

        protected IConsole Console { get { return _console; } }
        protected IScriptEngine Engine { get { return _engine; } }
        protected ConsoleOptions Options { get { return _options; } }
        protected internal ScriptModule Module { get { return _module; } set { _module = value; } }

        protected virtual string Prompt { get { return Resources.ConsolePrompt; } }
        protected virtual string PromptContinuation { get { return Resources.ConsoleContinuePrompt; } } 
        protected virtual string Logo { get { return null; } }

        public CommandLine() {
        }

        protected virtual void Initialize() {
        }

        /// <summary>
        /// Executes the comand line - depending upon the options provided we will
        /// either run a single file, a single command, or enter the interactive loop.
        /// </summary>
        public int Run(IScriptEngine engine, IConsole console, ConsoleOptions options) {
            if (engine == null) throw new ArgumentNullException("engine");
            if (console == null) throw new ArgumentNullException("console");
            if (options == null) throw new ArgumentNullException("options");

            _engine = engine;
            _options = options;
            _console = console;

            Initialize();
            
            try {

                int result;

                if (options.Command != null) {
                    result = RunCommand(options.Command);
                }
                
#if !SILVERLIGHT
                else if (options.FileName != null) {
                    result = RunFile(options.FileName);
                }
#endif
                else {
                    return RunInteractive();
                }

                if (options.Introspection) {
                    return RunInteractiveLoop();
                }

                return result;

#if !SILVERLIGHT // ThreadAbortException.ExceptionState
            } catch (System.Threading.ThreadAbortException tae) {
                if (tae.ExceptionState is KeyboardInterruptException) {
                    Thread.ResetAbort();
                }
                return -1;
#endif
            } finally {
                try {
                    engine.Shutdown();
                } catch (Exception e) {
                    _console.WriteLine("", Style.Error);
                    _console.WriteLine("Error in sys.exitfunc:", Style.Error);
                    _console.Write(engine.FormatException(e), Style.Error);
                }
            }
        }
        
        #region Console

        private static IConsole CreateConsole(CommandLine commandLine, ScriptEngine engine, bool isSuper, bool isColorful) {
            Debug.Assert(engine != null);

            if (isSuper) {
                return CreateSuperConsole(commandLine, engine, isColorful);
            } else {
                return new BasicConsole(engine, isColorful);
            }
        }

        // The advanced console functions are in a special non-inlined function so that 
        // dependencies are pulled in only if necessary.
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static IConsole CreateSuperConsole(CommandLine commandLine, ScriptEngine engine, bool isColorful) {
            Debug.Assert(engine != null);
            return new SuperConsole(commandLine, engine, isColorful);
        }
        
        #endregion

#if !SILVERLIGHT
        /// <summary>
        /// Runs the specified filename
        ///
        /// TODO minimize code duplication in overriding classes
        /// </summary>
        protected virtual int RunFile(string filename) {
            int result = 1;
            if (Options.HandleExceptions) {
                try {
                    Engine.ExecuteFile(filename);
                    result = 0;
                } catch (Exception e) {
                    Console.Write(Engine.FormatException(e), Style.Error);
                }
            } else {
                Engine.ExecuteFile(filename);
                result = 0;
            }

            return result;
        }        
#endif

        /// <summary>
        /// Runs a single line of text as the only input to the language.  The console exits afterwards.
        /// 
        /// TODO minimize code duplication in overriding classes
        /// </summary>
        protected virtual int RunCommand(string command) {
            int result = 1;

            if (Options.HandleExceptions) {
                try {
                    Engine.ExecuteCommand(command);
                    result = 0;
                } catch (Exception e) {
                    Console.Write(Engine.FormatException(e), Style.Error);
                }
            } else {
                Engine.ExecuteCommand(command);
                result = 0;
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
            if (_module == null) {
                _module = ScriptDomainManager.CurrentManager.CreateModule("<stdin>");
            }
            
            int? res = null;
            do {
                if (Options.HandleExceptions) {
                    try {
                        res = TryInteractiveAction();
#if SILVERLIGHT 
                    } catch (Utils.Environment.ExitProcessException e) {
                        res = e.ExitCode;
#endif
                    } catch (Exception e) {
                        // There should be no unhandled exceptions in the interactive session
                        // We catch all exceptions here, and just display it,
                        // and keep on going
                        _console.WriteLine(_engine.FormatException(e), Style.Error);
                    }
                } else {
                    res = TryInteractiveAction();
                }

            } while (res == null);

            return res.Value;
        }        

        /// <summary>
        /// Attempts to run a single interaction and handle any language-specific
        /// exceptions.  Base classes can override this and call the base implementation
        /// surrounded with their own exception handling.
        /// 
        /// Returns null if successful and execution should continue, or an exit code.
        /// </summary>
        public virtual int? TryInteractiveAction() {
            int? result = null;

            try {
                result = RunOneInteraction();
#if SILVERLIGHT // ThreadAbortException.ExceptionState
            } catch (ThreadAbortException) {
#else
            } catch (ThreadAbortException tae) {
                KeyboardInterruptException pki = tae.ExceptionState as KeyboardInterruptException;
                if (pki != null) {
                    _console.WriteLine(_engine.FormatException(tae), Style.Error);

                    Thread.ResetAbort();

                    /*!!!
                    bool endOfMscorlib = false;
                    string ex = engine.FormatException(tae, ExceptionConverter.ToPython(pki), delegate(StackFrame sf) {
                        // filter out mscorlib methods that show up on the stack initially, 
                        // for example ReadLine / ReadBuffer etc...
                        if (!endOfMscorlib &&
                            sf.GetMethod().DeclaringType != null &&
                            sf.GetMethod().DeclaringType.Assembly == typeof(string).Assembly) {
                            return false;
                        }
                        endOfMscorlib = true;
                        return true;
                    });
                     *
                    _console.Write(ex, Style.Error);*/
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

            _engine.ExecuteCommand(s, _module);

            return null;
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
        public string ReadStatement(out bool continueInteraction) {
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

                InteractiveCodeProperties props = _engine.GetInteractiveCodeProperties(code);

                if (InteractiveCodePropertiesEnum.IsValidAndComplete(props, allowIncompleteStatement))
                    return code;

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
        
        #endregion
    }
    
    
}