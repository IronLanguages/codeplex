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

using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Compiler;
using IronPython.Modules;

namespace IronPython.Runtime {
    /// <summary>
    /// PythonModule normally runs code using compiled modules. However, CompiledCode snippet code can also 
    /// be run in the context of a PythonModule. In such a case, ModuleScope holds the context and scope information
    /// of the PythonModule. There can be multiple ModuleScopes corresponding to a single PythonModule.
    /// </summary>

    public class ModuleScope : IModuleEnvironment, ICloneable {
        // This is usually the same as __module__.__dict__.
        // It differs for code which does:
        //     eval("someGlobal==someLocal", { "someGlobal":1 }, { "someLocal":1 })
        private IAttributesDictionary f_globals;
        private object f_locals; // can be any dictionary or IMapping.

        private bool trueDivision;

        private ReflectedType __builtin__;
        public PythonModule __module__;

        public List<object> staticData;

        internal ModuleScope(PythonModule mod) : this(mod, mod.__dict__, mod.__dict__) {
        }

        internal ModuleScope(PythonModule mod, IAttributesDictionary globals, object locals) {
            __module__ = mod;
            f_globals = globals;
            f_locals = locals;
            __builtin__ = TypeCache.Builtin;
        }

        internal ModuleScope(PythonModule mod, IAttributesDictionary globals, object locals, ICallerContext context) 
            : this(mod, globals, locals) {
            trueDivision = context.TrueDivision;
        }

        public override string ToString() {
            return Module.ToString();
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
                    return ad.TryGetValue(symbol, out ret);
                }

                string name = symbol.ToString();

                // always check for IMapping first, it does the right thing
                // w.r.t. overriding __getitem__ & TryGetValue.
                IMapping imap = f_locals as IMapping;
                if (imap != null) {
                    return imap.TryGetValue(name, out ret);
                }

                IDictionary<object, object> dict = f_locals as IDictionary<object, object>;
                if (dict != null) {
                    return dict.TryGetValue(name, out ret);
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

        #region IModuleEnvironment Members

        public virtual object GetGlobal(SymbolId symbol) {
            object ret;

            if (TryGetGlobal(symbol, out ret)) return ret;

            // In theory, we need to check if "__builtins__" has been set by the user
            // to some custom module. However, we do not do that for perf reasons.
            if (Ops.TryGetAttr(__module__.SystemState.modules["__builtin__"],
                symbol,
                out ret))
                return ret;

            throw Ops.NameError("name '{0}' not defined", symbol);
        }

        public virtual bool TryGetGlobal(SymbolId symbol, out object value) {
            return f_globals.TryGetValue(symbol, out value);
        }

        public virtual void SetGlobal(SymbolId symbol, object value) {
            Ops.SetIndexId(f_globals, symbol, value);
        }

        public virtual void DelGlobal(SymbolId symbol) {
            if (!f_globals.Remove(symbol)) {
                throw Ops.NameError("name {0} not defined", symbol);
            }
        }

        #endregion

        #region ICallerContext Members

        public PythonModule Module {
            get { return __module__; }
        }

        public SystemState SystemState {
            get {
                return ((ICallerContext)__module__).SystemState;
            }
        }

        public object Locals {
            get { return f_locals; }
        }

        public IAttributesDictionary Globals {
            get { return f_globals; }
        }

        public object GetStaticData(int index) {
            return staticData[index];
        }

        public CallerContextAttributes ContextFlags {
            get {
                return ((ICallerContext)this.__module__).ContextFlags;
            }
            set {
                ((ICallerContext)this.__module__).ContextFlags = value;
            }
        }

        public bool TrueDivision {
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
}
