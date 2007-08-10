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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.IO;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Compiler;
using IronPython.Runtime.Exceptions;
using IronPython.Hosting;


namespace IronPython.Runtime {
    public sealed class PythonContext : LanguageContext {
        /// <summary> Standard Python context.  This is the ContextId we use to indicate calls from the Python context. </summary>
        public static ContextId Id = ContextId.RegisterContext(typeof(PythonContext));

        [ThreadStatic]
        private static Exception RawException;
        private readonly PythonEngine _engine;
        private readonly bool _trueDivision;

        /// <summary>
        /// Creates a new PythonContext
        /// </summary>
        public PythonContext(PythonEngine engine)
             : this(engine, false) {
        }

        public PythonContext(PythonEngine engine, PythonCompilerOptions options)
            : this(engine, options.TrueDivision) {
        }

        public PythonContext(PythonEngine engine, bool trueDivision) {
            _engine = engine;
            _trueDivision = trueDivision;
        }

        /// <summary>
        /// True division is encoded into a compiled code that this context is associated with. Cannot be changed.
        /// </summary>
        public bool TrueDivision {
            get { return _trueDivision; }
        }

        public override ScriptEngine Engine {
            get {
                return PythonScriptEngine;
            }
        }

        private PythonEngine PythonScriptEngine {
            get {
                if (_engine == null) {
                    return PythonEngine.CurrentEngine;
                }

                return _engine;
            }
        }

        public override ContextId ContextId {
            get {
                return PythonContext.Id;
            }
        }

        public override ModuleContext CreateModuleContext(ScriptModule module) {
            if (module == null) throw new ArgumentNullException("module");
            InitializeModuleScope(module);
            return new PythonModuleContext(module);
        }

        private void InitializeModuleScope(ScriptModule module) {
            // TODO: following should be context sensitive variables:

            // adds __builtin__ variable if this is not a __builtin__ module itself:             
            if (PythonScriptEngine.SystemState.modules.ContainsKey("__builtin__")) {
                module.Scope.SetName(Symbols.Builtins, PythonScriptEngine.SystemState.modules["__builtin__"]);
            } else {
                Debug.Assert(module.ModuleName == "__builtin__");
            }

            // do not set names if null to make attribute getter pas thru:
            if (module.ModuleName != null) {
                PythonModuleOps.SetName(module, module.ModuleName);
            }

            if (module.FileName != null) {
                PythonModuleOps.SetFileName(module, module.FileName);
            }

            // If the filename is __init__.py then this is the initialization code
            // for a package and we need to set the __path__ variable appropriately
            if (module.FileName != null && Path.GetFileName(module.FileName) == "__init__.py") {
                string dirname = Path.GetDirectoryName(module.FileName);
                string dir_path = ScriptDomainManager.CurrentManager.Host.NormalizePath(dirname);
                module.Scope.SetName(Symbols.Path, List.MakeList(dir_path));
            }
        }


        public override void ModuleContextEntering(ModuleContext newContext) {
            if (newContext == null) return;

            PythonModuleContext newPythonContext = (PythonModuleContext)newContext;
            
            // code executed in the scope of module cannot disable TrueDivision:

            // TODO: doesn't work for evals (test_future.py) 
            //if (newPythonContext.TrueDivision && !_trueDivision) {
            //    throw new InvalidOperationException("Code cannot be executed in this module (TrueDivision cannot be disabled).");
            //}

            // flow the code's true division into the module if we have it set
            newPythonContext.TrueDivision |= _trueDivision;            
        }

        /// <summary>
        /// Python's global scope includes looking at built-ins.  First check built-ins, and if
        /// not there then fallback to any DLR globals.
        /// </summary>
        public override bool TryLookupGlobal(CodeContext context, SymbolId name, out object value) {
            object builtins;
            if (!context.Scope.ModuleScope.TryGetName(this, Symbols.Builtins, out builtins)) {
                value = null;
                return false;
            }

            ScriptModule sm = builtins as ScriptModule;
            if (sm == null) {
                value = null;
                return false;
            }

            if (sm.Scope.TryGetName(context.LanguageContext, name, out value)) return true;            

            return base.TryLookupGlobal(context, name, out value);
        }

        protected override Exception MissingName(SymbolId name) {
            throw PythonOps.NameError(name);
        }

        public override ScriptCode Reload(ScriptCode original, ScriptModule module) {
            PythonModuleOps.CheckReloadable(module);

            PythonEngine engine = PythonEngine.CurrentEngine;

            string filename = PythonModuleOps.GetReloadFilename(module, original.SourceUnit as SourceFileUnit);
            if (filename == null) {
                engine.Importer.ReloadBuiltin(module);
                return original;
            }

            SourceFileUnit su = new SourceFileUnit(engine, 
                filename,            
                PythonModuleOps.GetName(module) as string,
                engine.SystemState.DefaultEncoding);

            return ScriptCode.FromCompiledCode(su.Compile(module.GetCompilerOptions(engine)));
        }

        public override CompilerOptions GetCompilerOptions() {
            return new PythonCompilerOptions(TrueDivision);
        }

        public override Microsoft.Scripting.Actions.ActionBinder Binder {
            get {
                return _engine.DefaultBinder;
            }
        }

        public override bool IsTrue(object obj) {
            return PythonOps.IsTrue(obj);
        }

        public override void PopExceptionHandler() {
            // only clear after the last exception is out, we
            // leave the last thrown exception as our current
            // exception
            if (CurrentExceptions != null && CurrentExceptions.Count == 1) {
                RawException = null;
            }
        }

        /// <summary>
        /// helper function for non-re-raise exceptions.
        /// 
        /// type is the type of exception to throwo or an instance.  If it 
        /// is an instance then value should be null.  
        /// 
        /// If type is a type then value can either be an instance of type,
        /// a Tuple, or a single value.  This case is handled by EC.CreateThrowable.
        /// </summary>
        public Exception MakeException(object type, object value, object traceback) {
            Exception throwable;

            if (type == null && value == null && traceback == null) {
                // rethrow
                Tuple t = GetExceptionInfo();
                type = t[0];
                value = t[1];
                traceback = t[2];
            }

            if (type is Exception) { 
                throwable = type as Exception;
            } else if (PythonOps.IsInstance(type, PythonEngine.CurrentEngine._exceptionType)) {
                throwable = ExceptionConverter.ToClr(type);
            } else if (type is string) {
                throwable = new StringException(type.ToString(), value);
            } else if (type is OldClass) {
                if (value == null) {
                    throwable = ExceptionConverter.CreateThrowable(type);
                } else {
                    throwable = ExceptionConverter.CreateThrowable(type, value);
                }
            } else if (type is OldInstance) {
                throwable = ExceptionConverter.ToClr(type);
            } else {
                throwable = PythonOps.TypeError("exceptions must be classes, instances, or strings (deprecated), not {0}", DynamicHelpers.GetDynamicType(type));
            }

            IDictionary dict = ExceptionUtils.GetDataDictionary(throwable);

            if (traceback != null) {
                TraceBack tb = traceback as TraceBack;
                if (tb == null) throw PythonOps.TypeError("traceback argument must be a traceback object");

                dict[typeof(TraceBack)] = tb;
            } else if (dict.Contains(typeof(TraceBack))) {
                dict.Remove(typeof(TraceBack));
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Exceptions, throwable);

            return throwable;
        }

        public override object PushExceptionHandler(CodeContext context, Exception exception) {
            RawException = exception;

            GetExceptionInfo(); // force update of non-thread static exception info...

            return ExceptionConverter.ToPython(exception);
        }

        public override object CheckException(object exception, object test) {
            System.Diagnostics.Debug.Assert(exception != null);

            StringException strex;
            if (test is Tuple) {
                // we handle multiple exceptions, we'll check them one at a time.
                Tuple tt = test as Tuple;
                for (int i = 0; i < tt.Count; i++) {
                    object res = CheckException(exception, tt[i]);
                    if (res != null) return res;
                }
            } else if ((strex = exception as StringException) != null) {
                // catching a string
                if (test.GetType() != typeof(string)) return null;
                if (strex.Message == (string)test) {
                    if (strex.Value == null) return strex.Message;
                    return strex.Value;
                }
                return null;
            } else if (test is OldClass) {
                if (PythonOps.IsInstance(exception, test)) {
                    // catching a Python type.
                    return exception;
                }
            } else if (test is DynamicType) {
                if (PythonOps.IsSubClass(test as DynamicType, DynamicHelpers.GetDynamicTypeFromType(typeof(Exception)))) {
                    // catching a CLR exception type explicitly.
                    Exception clrEx = ExceptionConverter.ToClr(exception);
                    if (PythonOps.IsInstance(clrEx, test)) return clrEx;
                }
            }

            return null;
        }

        private TraceBack CreateTraceBack(Exception e) {
            // user provided trace back
            object result;
            if (ExceptionUtils.TryGetData(e, typeof(TraceBack), out result)) {
                return (TraceBack)result;
            }

            DynamicStackFrame[] frames = DynamicHelpers.GetDynamicStackFrames(e);
            TraceBack tb = null;
            for (int i = frames.Length - 1; i >= 0; i--) {
                DynamicStackFrame frame = frames[i];

                PythonFunction fx = new Function0(frame.CodeContext, frame.GetMethodName(), null, ArrayUtils.EmptyStrings, RuntimeHelpers.EmptyObjectArray);

                TraceBackFrame tbf = new TraceBackFrame(
                    new GlobalsDictionary(frame.CodeContext.Scope),
                    new LocalsDictionary(frame.CodeContext.Scope),
                    fx.FunctionCode);

                fx.FunctionCode.SetFilename(frame.GetFileName());
                fx.FunctionCode.SetLineNumber(frame.GetFileLineNumber());
                tb = new TraceBack(tb, tbf);
                tb.SetLine(frame.GetFileLineNumber());
            }

            return tb;
      }

        internal Tuple GetExceptionInfo() {
            if (RawException == null) return Tuple.MakeTuple(null, null, null);
            object pyExcep = ExceptionConverter.ToPython(RawException);

            TraceBack tb = CreateTraceBack(RawException);
            SystemState.Instance.ExceptionTraceBack = tb;

            StringException se = pyExcep as StringException;
            if (se == null) {
                object excType = PythonOps.GetBoundAttr(DefaultContext.Default, pyExcep, Symbols.Class);
                SystemState.Instance.ExceptionType = excType;
                SystemState.Instance.ExceptionValue = pyExcep;

                return Tuple.MakeTuple(
                    excType,
                    pyExcep,
                    tb);
            }

            // string exceptions are special...  there tuple looks
            // like string, argument, traceback instead of
            //      type,   instance, traceback
            SystemState.Instance.ExceptionType = pyExcep;
            SystemState.Instance.ExceptionValue = se.Value;

            return Tuple.MakeTuple(
                pyExcep,
                se.Value,
                tb);
        }

        protected override ModuleGlobalCache GetModuleCache(SymbolId name) {
            ModuleGlobalCache res;
            if (!SystemState.Instance.TryGetModuleGlobalCache(name, out res)) {
                res = base.GetModuleCache(name);
            }

            return res;
        }

        // Python's delete operator doesn't have a return value. Return null
        public override object DeleteMember(CodeContext context, object obj, SymbolId name) {
            ICustomMembers ifca = obj as ICustomMembers;
            if (ifca != null) {
                try {
                    ifca.DeleteCustomMember(context, name);
                } catch (InvalidOperationException) {
                    throw PythonOps.AttributeErrorForMissingAttribute(obj, name);
                }
                return null;
            }

            if (!DynamicHelpers.GetDynamicType(obj).TryDeleteMember(context, obj, name)) {
                throw PythonOps.AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetDynamicType(obj), name);
            }
            return null;
        }

        public override object GetMember(CodeContext context, object target, SymbolId name) {
            return PythonOps.GetBoundAttr(context, target, name);
        }

        public override object GetBoundMember(CodeContext context, object target, SymbolId name) {
            return PythonOps.GetBoundAttr(context, target, name);
        }

        public override void SetMember(CodeContext context, object target, SymbolId name, object value) {
            PythonOps.SetAttr(context, target, name, value);
        }

        public override object Call(CodeContext context, object function, object[] args) {
            return PythonOps.CallWithContext(context, function, args);
        }

        public override object CallWithThis(CodeContext context, object function, object instance, object[] args) {
            return PythonOps.CallWithContextAndThis(context, function, instance, args);
        }

        public override object CallWithArgsKeywordsTupleDict(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            return PythonOps.CallWithArgsTupleAndKeywordDictAndContext(context, func, args, names, argsTuple, kwDict);
        }

        public override object CallWithArgsTuple(CodeContext context, object func, object[] args, object argsTuple) {
            return PythonOps.CallWithArgsTupleAndContext(context, func, args, argsTuple);
        }

        public override object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names) {
            return PythonOps.CallWithKeywordArgs(context, func, args, names);
        }

        public override bool EqualReturnBool(CodeContext context, object x, object y) {
            return PythonOps.EqualRetBool(x, y);
        }

        public override object GetNotImplemented(params MethodCandidate[] candidates) {
            return PythonOps.NotImplemented;
        }

        public override bool IsCallable(object obj, int argumentCount, out int min, out int max) {
            return PythonOps.IsCallable(obj, argumentCount, out min, out max);
        }

        public override Assembly LoadAssemblyFromFile(string file) {
#if !SILVERLIGHT
            // check all files in the path...
            IEnumerator ie = PythonOps.GetEnumerator(SystemState.Instance.path);
            while (ie.MoveNext()) {
                string str;
                if (Converter.TryConvertToString(ie.Current, out str)) {
                    string fullName = Path.Combine(str, file);
                    Assembly res;

                    if (TryLoadAssemblyFromFileWithPath(fullName, out res)) return res;
                    if (TryLoadAssemblyFromFileWithPath(fullName + ".EXE", out res)) return res;
                    if (TryLoadAssemblyFromFileWithPath(fullName + ".DLL", out res)) return res;
                }
            }
#endif
            return null;
        }

#if !SILVERLIGHT // AssemblyResolve, files, path
        private bool TryLoadAssemblyFromFileWithPath(string path, out Assembly res) {
            if (File.Exists(path)) {
                try {
                    res = Assembly.LoadFile(path);
                    if (res != null) return true;
                } catch { }
            }
            res = null;
            return false;
        }

        internal static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            AssemblyName an = new AssemblyName(args.Name);
            return DefaultContext.Default.LanguageContext.LoadAssemblyFromFile(an.Name);
        }
#endif
    }   
}
