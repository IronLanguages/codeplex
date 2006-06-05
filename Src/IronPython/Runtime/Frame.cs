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
        private IDictionary<object, object> f_globals;

        private object f_locals; // can be any dictionary or IMapping.

        private bool trueDivision;

        private ReflectedType __builtin__;
        public readonly PythonModule __module__;

        public List<object> staticData;

        public static Frame MakeFrameForFunction(PythonModule context) {
            return new Frame(context, ((IDictionary<object,object>)context.__dict__), new FieldIdDict());
        }

        public Frame(PythonModule mod) {
            __module__ = mod;
            f_globals = ((IDictionary<object,object>)mod.__dict__);
            f_locals = mod.__dict__;
            __builtin__ = TypeCache.Builtin;
        }

        public Frame(PythonModule mod, IDictionary<object, object> globals, object locals) {
            __module__ = mod;
            f_globals = globals;
            f_locals = locals;
            __builtin__ = TypeCache.Builtin;
        }

        public object GetLocal(string name) {
            object ret;
            if (TryGetLocal(name, out ret)) { return ret; }
            return GetGlobal(name);
        }

        private bool TryGetLocal(string name, out object ret) {
            // couple of exception-free fast paths...
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
            ret = null;
            return false;
        }

        public void DelLocal(string name) {
            try {
                Ops.DelIndex(f_locals, name);
            } catch(KeyNotFoundException) {                
                throw Ops.NameError("name {0} is not defined", name);
            }
        }

        public void SetLocal(string name, object value) {
            Ops.SetIndex(f_locals, name, value);
        }

        public object GetGlobal(string name) {
            object ret;

            if (f_globals.TryGetValue(name, out ret)) return ret;

            // In theory, we need to check if "__builtins__" has been set by the user
            // to some custom module. However, we do not do that for perf reasons.
            if (__builtin__.TryGetAttr(this, SymbolTable.StringToId(name), out ret)) return ret;

            throw Ops.NameError("name '{0}' not defined", name);
        }

        public void SetGlobal(string name, object value) {
            Ops.SetIndex(f_globals, name, value);
        }

        public void DelGlobal(string name) {
            if (!f_globals.Remove(name)) {
                throw Ops.NameError("name {0} not defined", name);
            }
        }

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

        object ICallerContext.Globals {
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
