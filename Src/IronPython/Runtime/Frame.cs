/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Reflection;

using IronPython.Compiler;
using IronPython.Modules;

namespace IronPython.Runtime {
    /// <summary>
    /// PythonModule normally runs code using compiled modules. However, FrameCode snippet code can also 
    /// be run in the context of a PythonModule. In such a case, Frame holds the context and scope information
    /// of the PythonModule..
    /// </summary>

    public class Frame : IFrameEnvironment, ICloneable {
        // This is usually the same as __module__.__dict__.
        // It differs for code which does:
        //     eval("someGlobal==someLocal", { "someGlobal":1 }, { "someLocal":1 })
        private IAttributesDictionary f_globals;
        private object f_locals; // can be any dictionary or IMapping.

        private bool trueDivision;

        private ReflectedType __builtin__;
        public PythonModule __module__;

        public List<object> staticData;

        private string moduleName;

        public static Frame MakeFrameForFunction(PythonModule context) {
            return new Frame(context, context.__dict__, new FieldIdDict());
        }

        /// <summary>
        /// These overloads of the constructor allows delayed creating of the PythonModule. The Frame cannot
        /// be used until EnsureInitialized has been called.
        /// </summary>
        public Frame() : this(String.Empty) {
        }

        public Frame(string modName) {
            if (modName == null)
                throw new ArgumentException("moduleName");

            moduleName = modName;
            // Populate the dictionary so that SetGlobal will work
            f_globals = new FieldIdDict();
        }

        internal Frame(PythonModule mod) : this(mod, mod.__dict__, mod.__dict__) {
        }

        internal Frame(PythonModule mod, IAttributesDictionary globals, object locals) {
            Initialize(mod, globals, locals);
        }

        internal void EnsureInitialized(SystemState state) {
            Debug.Assert(state != null);

            if (__module__ == null) {
                lock (this) {
                    if (__module__ == null) {
                        PythonModule mod = new PythonModule(moduleName, f_globals, state);
                        Initialize(mod, mod.__dict__, mod.__dict__);

                        if (moduleName != String.Empty)
                            mod.SystemState.modules[moduleName] = mod;
                    }
                }
            }

            if (__module__.SystemState != state)
                throw new ArgumentException("A ModuleScope can only be used with its associated PythonEngine", "moduleScope");
        }

        private void Initialize(PythonModule mod, IAttributesDictionary globals, object locals) {
            __module__ = mod;
            f_globals = globals;
            f_locals = locals;
            __builtin__ = TypeCache.Builtin;
            moduleName = mod.ModuleName;
        }

        public override string ToString() {
            return moduleName;
        }

        public object GetLocal(SymbolId symbol) {
            object ret;
            if (TryGetLocal(symbol, out ret)) { return ret; }
            return GetGlobal(symbol);
        }

        private bool TryGetLocal(SymbolId symbol, out object ret) {
            if (f_locals != null) {
                // couple of exception-free fast paths...
                IAttributesDictionary ad = f_locals as IAttributesDictionary;
                if (ad != null) {
                    if (ad.TryGetValue(symbol, out ret)) return true;
                }

                string name = symbol.ToString();

                IDictionary<object, object> dict = f_locals as IDictionary<object, object>;
                if (dict != null) {
                    if (dict.TryGetValue(name, out ret)) return true;
                }

                IMapping imap = f_locals as IMapping;
                if (imap != null) {
                    return imap.TryGetValue(name, out ret);
                }

                // uh-oh, we may end up throwing...
                try {
                    ret = Ops.GetIndex(f_locals, name);
                    return true;
                } catch (KeyNotFoundException) {
                    // return false
                }
            }
            ret = null;
            return false;
        }

        public void DelLocal(SymbolId symbol) {
            try {
                Ops.DelIndex(f_locals, symbol.ToString());
            } catch (KeyNotFoundException) {
                throw Ops.NameError("name {0} is not defined", symbol);
            }
        }

        public void SetLocal(SymbolId symbol, object value) {
            Ops.SetIndexId(f_locals, symbol, value);
        }

        #region IFrameEnvironment Members

        public object GetGlobal(SymbolId symbol) {
            object ret;

            if (f_globals.TryGetValue(symbol, out ret)) return ret;

            // In theory, we need to check if "__builtins__" has been set by the user
            // to some custom module. However, we do not do that for perf reasons.
            if (__builtin__.TryGetAttr(this, symbol, out ret)) return ret;

            throw Ops.NameError("name '{0}' not defined", symbol);
        }

        public void SetGlobal(SymbolId symbol, object value) {
            Ops.SetIndexId(f_globals, symbol, value);
        }

        public void DelGlobal(SymbolId symbol) {
            if (!f_globals.Remove(symbol)) {
                throw Ops.NameError("name {0} not defined", symbol);
            }
        }

        #endregion

        #region ICallerContext Members

        public PythonModule Module {
            get { return __module__; }
        }

        SystemState ICallerContext.SystemState {
            get {
                return ((ICallerContext)__module__).SystemState;
            }
        }

        object ICallerContext.Locals {
            get { return f_locals; }
        }

        IAttributesDictionary ICallerContext.Globals {
            get { return f_globals; }
        }

        object ICallerContext.GetStaticData(int index) {
            return staticData[index];
        }

        CallerContextFlags ICallerContext.ContextFlags {
            get {
                return ((ICallerContext)this.__module__).ContextFlags;
            }
            set {
                ((ICallerContext)this.__module__).ContextFlags = value;
            }
        }

        bool ICallerContext.TrueDivision {
            get { return trueDivision; }
            set { trueDivision = value; }
        }

        public CompilerContext CreateCompilerContext() {
            CompilerContext context = new CompilerContext();
            context.TrueDivision = ((ICallerContext)this).TrueDivision;
            return context;
        }

        #endregion

        #region ICloneable Members

        public object Clone() {
            return MemberwiseClone();
        }

        #endregion
    }


    // FrameCode represents code that executes in the context of a Frame. This is typically
    // code executed from the interactive console, code compiled with the "eval" keyword, etc.
    public delegate object FrameCodeDelegate(Frame frame);

    public class FrameCode {
        private FrameCodeDelegate code;
        private string name;
        private List<object> staticData;

        public FrameCode(string name, FrameCodeDelegate code, List<object> staticData) {
            this.name = name;
            this.code = code;
            this.staticData = staticData;
        }

        public object Run(Frame frame) {
            frame = (Frame)frame.Clone();
            frame.staticData = staticData;
            return code(frame);
        }

        public override string ToString() {
            return string.Format("<code {0}>", name);
        }
    }
}
