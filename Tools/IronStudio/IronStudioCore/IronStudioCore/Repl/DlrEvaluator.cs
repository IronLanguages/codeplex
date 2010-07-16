/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Threading;
using Microsoft.IronStudio.Core.Repl;
using Microsoft.IronStudio.Repl;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

namespace Microsoft.IronStudio.Library.Repl {
    public abstract class DlrEvaluator : ReplEvaluator, IDlrEvaluator {
        protected ScriptEngine _engine;
        protected ScriptScope _currentScope;
        protected Thread _thread, _targetThread;
        protected Dispatcher _currentDispatcher;
        private int _varCounter;

        // Concrete subclasses constructed via reflection when deserialized from the registry.
        protected DlrEvaluator(string/*!*/ language)
            : base(language) {
        }

        public ScriptEngine Engine {
            get {
                return _engine;
            }
        }

        public override void Start() {
            Action<string> writer = _output.Write;
            _engine = MakeEngine(new OutputStream(writer), new Writer(writer), new Reader(ReadInput));
        }

        public override void Reset() {
            InitScope(Engine.CreateScope());
        }

        protected virtual ScriptEngine MakeEngine(System.IO.Stream stream, System.IO.TextWriter writer, System.IO.TextReader reader) {
            throw new NotImplementedException();
        }

        protected virtual void InitScope(ScriptScope scope) {
            _currentScope = scope ?? _engine.CreateScope();
        }

        protected void InitThread() {
            _thread = new Thread(Execute);
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Priority = ThreadPriority.Normal;
            _thread.Name = String.Format("Execution Engine ({0})", Language);
            _thread.IsBackground = true;
            _thread.Start();
        }

        // [ReplCommand("to_gui")
        public void SwitchToTargetThread() {
            if (_targetThread == null) {
                WriteException(new ObjectHandle(new Exception("Not attached to running program")));
                return;
            }
            var dispatcher = Dispatcher.FromThread(_targetThread);
            if (dispatcher == null) {
                WriteException(new ObjectHandle(new Exception("Running program is not a GUI application")));
                return;
            }
            _currentDispatcher = dispatcher;
        }

        // [ReplCommand("to_repl")
        public void SwitchToOurThread() {
            _currentDispatcher = Dispatcher.FromThread(_thread);
        }

        protected void OutputResult(object result, ObjectHandle exception) {
            if (exception != null) {
                WriteException(exception);
            } else if (result != null) {
                ScopeForLastResult.SetVariable("_", result);
                WriteObject(result, _engine.Operations.Format(result));
            }
        }

        public override string FormatException(ObjectHandle exception) {
            // TODO: ???
            return _engine.GetService<ExceptionOperations>().FormatException(exception);
        }

        protected virtual ScriptScope ScopeForLastResult {
            get { return _currentScope; }
        }

        protected CompilerOptions CompilerOptions {
            get { return _engine.GetCompilerOptions(_currentScope); }
        }

        protected virtual SourceCodeKind SourceCodeKind {
            get { return SourceCodeKind.Statements; }
        }

        protected Dispatcher Dispatcher {
            get {
                if (_currentDispatcher == null) {
                    // First try the target thread and use its dispatcher if possible
                    _currentDispatcher = Dispatcher.FromThread(_targetThread);
                    if (_currentDispatcher == null) {
                        // Fall back to our own dispatcher
                        _currentDispatcher = Dispatcher.FromThread(_thread);
                    }
                }
                return _currentDispatcher;
            }
        }

        public override bool CanExecuteText(string/*!*/ text) {
            var source = _engine.CreateScriptSourceFromString(text, SourceCodeKind);
            var result = source.GetCodeProperties(CompilerOptions);
            return (result == ScriptCodeParseResult.Empty ||
                result == ScriptCodeParseResult.Complete ||
                result == ScriptCodeParseResult.Invalid);
        }

        private void FinishExecution(ObjectHandle result, ObjectHandle exception, Action<bool, ObjectHandle> completionFunction) {
            _output.Flush();
            if (exception != null) {
                OutputResult(null, exception);
            }

            if (completionFunction != null) {
                completionFunction(exception == null, exception);
            }
        }

        public override bool ExecuteText(string text, Action<bool, ObjectHandle> completionFunction) {
            return ExecuteTextInScope(text, _currentScope, completionFunction);
        }

        // Called by debugger to do an Eval at a breakpoint. 
        // This should be called on the Eval thread.
        // This is a blocking call and pumps our own thread to execute until the eval finishes.
        // So this will invoke completionFunction before it returns.
        // In a debug-context, all other threads may be suspended by the debugger, so 
        // be careful of cross-thread calls.
        public bool ExecuteTextInScopeAndBlock(string text, Scope scope, Action<bool, ObjectHandle> completionFunction) {
            Debug.Assert(scope != null);

            // Setup a nested dispatcher pump.
            DispatcherFrame frame = new DispatcherFrame();


            var d = this.Dispatcher;

            // This should be called on the current thread
            Debug.Assert(Dispatcher.CurrentDispatcher == d);
            Debug.Assert(_thread == Thread.CurrentThread);


            var ss = Microsoft.Scripting.Hosting.Providers.HostingHelpers.CreateScriptScope(_engine, scope);
            var result = ExecuteTextInScope(text, ss, completionFunction);

            // Queue exit message after we queue the real work.
            d.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrameHelper), frame);

            Dispatcher.PushFrame(frame); // blocks for exit message.


            return result;
        }

        // Helper to get our nested Dispatched to exit.
        private object ExitFrameHelper(object f) {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }

        public bool ExecuteTextInScope(string text, ScriptScope scope, Action<bool, ObjectHandle> completionFunction) {
            return ExecuteTextInScopeWorker(text, scope, SourceCodeKind, (r, e) => FinishExecution(r, e, completionFunction));
        }

        private bool ExecuteTextInScopeWorker(string text, ScriptScope scope, SourceCodeKind kind, Action<ObjectHandle, ObjectHandle> completionFunction) {
            var source = _engine.CreateScriptSourceFromString(text, kind);
            var errors = new DlrErrorListener();
            var command = source.Compile(CompilerOptions, errors);
            if (command == null) {
                if (errors.Errors.Count > 0) {
                    WriteException(new ObjectHandle(errors.Errors[0]));
                }
                return false;
            }
            // Allow re-entrant execution.

            if (Dispatcher.HasShutdownStarted) {
                SwitchToOurThread();
                if (Dispatcher.HasShutdownStarted) {
                    WriteException(new ObjectHandle(new Exception("This dispatcher is no longer running")));
                    return false;
                }
                WriteLine("(switching back to REPL dispatcher)");
            }

            Dispatcher.BeginInvoke(new Action(() => {
                ObjectHandle result = null;
                ObjectHandle exception = null;
                try {
                    result = command.ExecuteAndWrap(scope, out exception);
                } catch (ThreadAbortException e) {
                    if (e.ExceptionState != null) {
                        exception = new ObjectHandle(e.ExceptionState);
                    } else {
                        exception = new ObjectHandle(e);
                    }
                    if ((Thread.CurrentThread.ThreadState & System.Threading.ThreadState.AbortRequested) != 0) {
                        Thread.ResetAbort();
                    }
                } catch (RemotingException) {
                    WriteLine("Communication with the remote process has been disconnected.");
                } catch (Exception e) {
                    exception = new ObjectHandle(e);
                }
                if (completionFunction != null) {
                    completionFunction(result, exception);
                }
            }));

            return true;
        }

        public virtual ICollection<MemberDoc> GetMemberNames(string expression) {
            var source = _engine.CreateScriptSourceFromString(expression, SourceCodeKind.Expression);
            var errors = new DlrErrorListener();

            if (ShouldEvaluateForCompletion(expression)) {
                var command = source.Compile(CompilerOptions, errors);
                if (command == null) {
                    return new MemberDoc[0];
                }

                ObjectHandle exception, obj = command.ExecuteAndWrap(_currentScope, out exception);
                if (exception == null) {
                    var docOps = _engine.GetService<DocumentationOperations>();
                    if (docOps != null) {
                        return docOps.GetMembers(obj);
                    }

                    return new List<MemberDoc>(
                        _engine.Operations.GetMemberNames(obj).Select(
                            x => new MemberDoc(x, MemberKind.None)
                        )
                    );
                }
            }

            return new MemberDoc[0];
        }

        protected virtual bool ShouldEvaluateForCompletion(string source) {
            return true;
        }

        public virtual ICollection<OverloadDoc> GetSignatureDocumentation(string expression) {
            var source = _engine.CreateScriptSourceFromString(expression, SourceCodeKind.Expression);
            if (ShouldEvaluateForCompletion(expression)) {
                var errors = new DlrErrorListener();
                var command = source.Compile(CompilerOptions, errors);
                if (command == null) {
                    return new OverloadDoc[0];
                }

                ObjectHandle exception, obj = command.ExecuteAndWrap(_currentScope, out exception);
                if (exception == null) {
                    var docOps = _engine.GetService<DocumentationOperations>();
                    if (docOps != null) {
                        return docOps.GetOverloads(obj);
                    }

                    return new[] { 
                        new OverloadDoc(
                            "",
                            _engine.Operations.GetDocumentation(obj),
                            new ParameterDoc[0]
                        )
                    };
                }
            }

            return new OverloadDoc[0];
        }

        public override bool CheckSyntax(string text, SourceCodeKind kind) {
            var source = _engine.CreateScriptSourceFromString(text, SourceCodeKind);
            var errors = new DlrErrorListener();
            var command = source.Compile(CompilerOptions, errors);
            if (command == null) {
                if (errors.Errors.Count > 0) {
                    WriteException(new ObjectHandle(errors.Errors[0]));
                }
                return false;
            }
            return true;
        }

        public override void AbortCommand() {
            // TODO: Support for in-proc REPLs
        }

        public new void Dispose() {
            base.Dispose();
            var dispatcher = Dispatcher.FromThread(_thread);
            if (dispatcher != null) {
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            }
            var otherDispatcher = Dispatcher.FromThread(_targetThread);
            if (otherDispatcher != null && otherDispatcher != dispatcher) {
                otherDispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            }
        }

        private void Execute() {
            while (!Dispatcher.CurrentDispatcher.HasShutdownStarted) {
                try {
                    Dispatcher.Run();
                } catch (Exception exception) {
                    try {
                        OutputResult(null, new ObjectHandle(exception));
                    } catch (Exception) {
                        Restart();
                    }
                }
            }
            _engine.Runtime.Shutdown();
        }

        public virtual void Restart() {
        }

        public override string InsertData(object data, string prefix) {
            if (prefix == null || prefix.Length == 0) {
                prefix = "__data";
            }

            while (true) {
                var varname = prefix + _varCounter.ToString();
                if (!_currentScope.ContainsVariable(varname)) {
                    // TODO: Race condition
                    _currentScope.SetVariable(varname, data);
                    return varname;
                }
                _varCounter++;
            }
        }

        protected virtual string[] FilterNames(IList<string> names, string startsWith) {
            // TODO: LINQify once targeting CLR4?
            var n = new List<string>(names);
            if (startsWith != null && startsWith.Length != 0) {
                n.RemoveAll((s) => !s.StartsWith(startsWith));
            }
            n.Sort((a, b) => {
                int cmp1 = a.ToLowerInvariant().CompareTo(b.ToLowerInvariant());
                if (cmp1 != 0) {
                    return cmp1;
                }
                return a.CompareTo(b);
            });
            return n.ToArray();
        }

        protected override object GetRootObject() {
            return _currentScope;
        }

        internal override object GetObjectMember(object obj, string name) {
            var dobj = (obj as DispatcherObject);
            if (dobj != null && dobj.CheckAccess()) {
                var dlg = (Func<object, string, object>)GetObjectMember;
                return dobj.Dispatcher.Invoke(dlg, obj, name);
            }

            object result = null;
            try {
                // TODO: case-sensitivity
                _engine.Operations.TryGetMember(obj, name, false, out result);
            } catch {
                // TODO: Log error?
            }
            return result;
        }

        protected override string[] GetObjectMemberNames(ObjectHandle obj, string startsWith) {
            try {
                return FilterNames(_engine.Operations.GetMemberNames(obj), startsWith);
            } catch {
                // TODO: Log error?
                return new string[0];
            }
        }

        public virtual bool EnableMultipleScopes {
            get {
                return true;
            }
        }

        class DlrErrorListener : ErrorListener {
            private readonly List<Exception> _errors = new List<Exception>();

            internal List<Exception> Errors {
                get { return _errors; }
            }

            public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity) {
                _errors.Add(new SyntaxErrorException(
                    message,
                    source.Path,
                    source.GetCode(),
                    source.GetCodeLine(span.Start.Line),
                    span,
                    errorCode,
                    severity));
            }
        }
#if FALSE
        public override WorkspaceVariable[] GetVariablesInGlobalScope() {
            var result = new List<WorkspaceVariable>();
            foreach (var v in _scope.GetItems())
            {
                var obj = v.Value;
                result.Add(new WorkspaceVariable {
                    Name = v.Key,
                    Value = (obj == null) ? "null" : obj.ToString(),
                    Type = (obj == null) ? "none" : obj.GetType().Name
                });
            }
            return result.ToArray();
        }
#endif
    }
}
    
