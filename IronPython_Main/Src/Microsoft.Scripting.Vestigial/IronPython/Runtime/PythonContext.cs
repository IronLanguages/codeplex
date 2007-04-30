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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Compiler;
using IronPython.Runtime.Exceptions;
using IronPython.Hosting;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Hosting;

namespace IronPython.Runtime {
    public sealed class PythonContext : LanguageContext {
        private readonly bool _trueDivision;
        private PythonModuleContext _moduleContext;
        [ThreadStatic]
        private Exception RawException;
       
        /// <summary>
        /// Creates a new PythonContext
        /// </summary>
        public PythonContext(ScriptEngine engine, PythonModuleContext moduleContext)
             : this(engine, moduleContext, false) {
        }

        public PythonContext(ScriptEngine engine, PythonModuleContext moduleContext, PythonCompilerOptions options)
            : this(engine, moduleContext, options.TrueDivision) {
        }

        public PythonContext(ScriptEngine engine, PythonModuleContext moduleContext, bool trueDivision) 
            : base(engine) {
            // TODO: if (engine == null) throw new ArgumentNullException("engine");
            if (moduleContext == null) throw new ArgumentNullException("moduleContext");

            _moduleContext = moduleContext;
            _trueDivision = trueDivision;
        }

        /// <summary>
        /// True division is encoded into a compiled code that this context is associated with. Cannot be changed.
        /// TODO: Shoulnd't this code property be stored on ScriptCode?
        /// </summary>
        public bool TrueDivision {
            get { return _trueDivision; }
        }

        public override ScriptEngine Engine {
            get {
                if (base.Engine == null) {
                    SetEngine(SystemState.Instance.Engine);
                }

                return base.Engine;
            }
        }

        public override bool ShowCls {
            get {
                return _moduleContext.ShowCls;
            }
            set {
                _moduleContext.ShowCls = value;
            }
        }

        public PythonModuleContext ModuleContext {
            get { return _moduleContext; }
            set {
                if (value == null) throw new ArgumentNullException("value");
                _moduleContext = value;
            }
        }

        public override ContextId ContextId {
            get {
                return DefaultContext.PythonContext;
            }
        }

        public override LanguageContext GetLanguageContextForModule(ScriptModule module) {
            Debug.Assert(module != null);

            // TODO: remove cast
            PythonModuleContext existing_context = (PythonModuleContext)Engine.TryGetModuleContext(module);

            if (existing_context == null) {
                existing_context = new PythonModuleContext();
                Engine.SetModuleContext(module, existing_context);
            }
            
            // code executed in the scope of module cannot disable TrueDivision:
            if (existing_context.TrueDivision && !this._trueDivision) {
                throw new InvalidOperationException("Code cannot be executed in this module (TrueDivision cannot be disabled).");
            }

            // flow the code's true division into the module if we have it set
            existing_context.TrueDivision |= this._trueDivision;            

            return new PythonContext(Engine, existing_context, TrueDivision);
        }

        /// <summary>
        /// Python's global scope includes looking at built-ins.  First check built-ins, and if
        /// not there then fallback to any DLR globals.
        /// </summary>
        public override bool TryLookupGlobal(Scope scope, SymbolId name, out object value) {
            object builtins;
            if (!scope.GlobalScope.TryGetName(this, Symbols.Builtins, out builtins)) {
                value = null;
                return false;
            }

            ScriptModule sm = builtins as ScriptModule;
            if (sm == null) {
                value = null;
                return false;
            }

            if (Ops.TryGetBoundAttr(builtins, name, out value)) return true;

            return base.TryLookupGlobal(scope, name, out value);
        }

        protected override Exception MissingName(SymbolId name) {
            throw Ops.NameError(name);
        }

        public override ScriptCode Reload(ScriptCode original, ScriptModule module) {
            PythonModuleOps.CheckReloadable(module);

            SystemState state = SystemState.Instance;

            string filename = PythonModuleOps.GetReloadFilename(module, original.SourceUnit as SourceFileUnit);
            if (filename == null) {
                state.Importer.ReloadBuiltin(module);
                return original;
            }

            SourceFileUnit su = new SourceFileUnit(state.Engine, 
                filename,            
                PythonModuleOps.GetName(module) as string,
                state.DefaultEncoding);

            return ScriptCode.FromCompiledCode(su.Compile(module.GetCompilerOptions(state.Engine)));
        }

        public override CompilerOptions GetCompilerOptions() {
            PythonCompilerOptions options = new PythonCompilerOptions();
            options.TrueDivision = TrueDivision;
            return options;
        }

        public override Microsoft.Scripting.Actions.ActionBinder Binder {
            get {
                return PythonBinder.Default;
            }
        }

        public override bool IsTrue(object obj) {
            return Ops.IsTrue(obj);
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

            if (type is Exception) throwable = type as Exception;
            else if (Ops.IsInstance(type, Ops.ExceptionType)) throwable = ExceptionConverter.ToClr(type);
            else if (type is string) throwable = new StringException(type.ToString(), value);
            else if (type is OldClass) {
                if (value == null)
                    throwable = ExceptionConverter.CreateThrowable(type);
                else
                    throwable = ExceptionConverter.CreateThrowable(type, value);
            } else if (type is OldInstance) throwable = ExceptionConverter.ToClr(type);
            else throwable = Ops.TypeError("exceptions must be classes, instances, or strings (deprecated), not {0}", Ops.GetDynamicType(type)); 

            if (traceback != null) {
                TraceBack tb = traceback as TraceBack;
                if (tb == null) throw Ops.TypeError("traceback argument must be a traceback object");

                Utils.GetDataDictionary(throwable)[typeof(TraceBack)] = tb;
            } else if (Utils.GetDataDictionary(throwable).Contains(typeof(TraceBack))) {
                Utils.GetDataDictionary(throwable).Remove(typeof(TraceBack));
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
                if (Ops.IsInstance(exception, test)) {
                    // catching a Python type.
                    return exception;
                }
            } else if (test is DynamicType) {
                if (Ops.IsSubClass(test as DynamicType, Ops.GetDynamicTypeFromType(typeof(Exception)))) {
                    // catching a CLR exception type explicitly.
                    Exception clrEx = ExceptionConverter.ToClr(exception);
                    if (Ops.IsInstance(clrEx, test)) return clrEx;
                }
            }

            return null;
        }

        private TraceBack CreateTraceBack(Exception e) {
            if (Utils.GetDataDictionary(e).Contains(typeof(TraceBack))) {
                // user provided trace back
                return (TraceBack)Utils.GetDataDictionary(e)[typeof(TraceBack)];
            }

            DynamicStackFrame [] frames = DynamicHelpers.GetDynamicStackFrames(e);
            TraceBack tb = null;
            for (int i = frames.Length - 1; i >= 0; i--) {
                DynamicStackFrame frame = frames[i];

                PythonFunction fx = new Function0(frame.CodeContext, frame.GetMethodName(), null, new string[0], Ops.EmptyObjectArray);

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
                object excType = Ops.GetBoundAttr(DefaultContext.Default, pyExcep, Symbols.Class);
                SystemState.Instance.ExceptionType = excType;
                SystemState.Instance.ExceptionValue = pyExcep;

                return Ops.MakeTuple(
                    excType,
                    pyExcep,
                    tb);
            }

            // string exceptions are special...  there tuple looks
            // like string, argument, traceback instead of
            //      type,   instance, traceback
            SystemState.Instance.ExceptionType = pyExcep;
            SystemState.Instance.ExceptionValue = se.Value;

            return Ops.MakeTuple(
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
                    throw Ops.AttributeErrorForMissingAttribute(obj, name);
                }
                return null;
            }

            if (!Ops.GetDynamicType(obj).TryDeleteMember(context, obj, name)) {
                throw Ops.AttributeErrorForMissingOrReadonly(context, Ops.GetDynamicType(obj), name);
            }
            return null;
        }

        // Python's delete operator doesn't have a return value. Return null
        public override object DeleteIndex(CodeContext context, object obj, object index) {
            Ops.DelIndex(obj, index);
            return null;
        }

        public override object GetIndex(CodeContext context, object target, object index) {
            return Ops.GetIndex(target, index);
        }

        public override void SetIndex(CodeContext context, object target, object index, object value) {
            Ops.SetIndex(target, index, value);
        }

        public override object GetMember(CodeContext context, object target, SymbolId name) {
            return Ops.GetAttr(context, target, name);
        }

        public override object GetBoundMember(CodeContext context, object target, SymbolId name) {
            return Ops.GetBoundAttr(context, target, name);
        }

        public override void SetMember(CodeContext context, object target, SymbolId name, object value) {
            Ops.SetAttr(context, target, name, value);
        }

        public override object Call(CodeContext context, object function, object[] args) {
            return Ops.CallWithContext(context, function, args);
        }

        public override object CallWithThis(CodeContext context, object function, object instance, object[] args) {
            return Ops.CallWithContextAndThis(context, function, instance, args);
        }

        public override object CallWithArgsKeywordsTupleDict(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            return Ops.CallWithArgsTupleAndKeywordDictAndContext(context, func, args, names, argsTuple, kwDict);
        }

        public override object CallWithArgsTuple(CodeContext context, object func, object[] args, object argsTuple) {
            return Ops.CallWithArgsTupleAndContext(context, func, args, argsTuple);
        }

        public override object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names) {
            return Ops.CallWithKeywordArgs(context, func, args, names);
        }

        public override bool EqualReturnBool(CodeContext context, object x, object y) {
            return Ops.EqualRetBool(x, y);
        }
    }   
}
