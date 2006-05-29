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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using System.Diagnostics;

using IronPython.Compiler;
using IronPython.Modules;

namespace IronPython.Runtime {

    /// <summary>
    /// The delegate that PythonModule calls to initialize the compiled
    /// module (run its body)
    /// </summary>
    public delegate void InitializeModule();

    /// <summary>
    /// Summary description for module.
    /// </summary>
    [PythonType("module")]
    public class PythonModule : ICustomAttributes, IFrameEnvironment {
        internal IAttributesDictionary __dict__;

        internal const string ModuleFieldName = "myModule__py";

        private ICustomAttributes innerMod;
        private bool packageImported;
        private CallerContextFlags contextFlags;
        private bool trueDivision;
        private InitializeModule initialize;
        private SystemState systemState;

        #region Public constructors

        public PythonModule(string name, IDictionary<object, object> dict, SystemState state)
            : this(name, dict, state, null, CallerContextFlags.None) {
        }

        public PythonModule(string name, IDictionary<object, object> dict, SystemState state, InitializeModule init)
            : this(name, dict, state, init, CallerContextFlags.None) {
        }

        public PythonModule(string name, IDictionary<object, object> dict, SystemState state, InitializeModule init, CallerContextFlags callerContextFlags) {
            Debug.Assert(state != null);

            if (dict is IAttributesDictionary)
                __dict__ = (IAttributesDictionary)dict;
            else
                __dict__ = new FieldIdDict(dict);
            ModuleName = name;
            __dict__[SymbolTable.Builtins] = TypeCache.Builtin;

            initialize = init;

            contextFlags = callerContextFlags;
            systemState = state;
        }

        #endregion

        #region Public API Surface
        public string ModuleName {
            [PythonName("__name__")]
            get { return (string)__dict__[SymbolTable.Name]; }
            [PythonName("__name__")]
            set { __dict__[SymbolTable.Name] = value; }
        }
        public string Filename {
            [PythonName("__file__")]
            get {
                object file;
                if (__dict__.TryGetValue(SymbolTable.File, out file)) {
                    string sfile = file as string;
                    if (sfile != null) return sfile;
                }
                return null;
            }
            [PythonName("__file__")]
            set { __dict__[SymbolTable.File] = value; }
        }

        //!!! This needs to go
        public void InitializeBuiltins() {
            FieldInfo ti = __dict__.GetType().GetField(ModuleFieldName);
            if (ti != null) ti.SetValue(__dict__, this);

            foreach (FieldInfo fi in __dict__.GetType().GetFields()) {
                if (fi.FieldType != typeof(object) && fi.FieldType != typeof(BuiltinWrapper)) continue;
                if (!fi.IsStatic) continue;

                if (fi.Name == "__debug__") {
                    fi.SetValue(null, new BuiltinWrapper(IronPython.Hosting.PythonEngine.options.DebugMode, "__debug__"));
                    continue;
                }

                //!!! GetAttr instead of GetSlot
                
                object bi;
                if (TypeCache.Builtin.TryGetSlot(DefaultContext.Default, SymbolTable.StringToId(fi.Name), out bi)) {
                    Debug.Assert(fi.FieldType == typeof(BuiltinWrapper));

                    ReflectedMethod rm = bi as ReflectedMethod;
                    BuiltinFunction bf;
                    if (rm != null) {
                        // update the dictionary w/ an optimized version, if one's available.
                        bf = ReflectOptimizer.MakeFunction(rm);
                        if (bf != null) bi = bf;
                    }

                    fi.SetValue(null, new BuiltinWrapper(Ops.GetDescriptor(bi, null, TypeCache.Builtin), fi.Name));
                    continue;
                }

                if (fi.GetValue(null) == null) {
                    Debug.Assert(fi.FieldType == typeof(object));

                    fi.SetValue(null, new Uninitialized(fi.Name));
                }
            }
        }

        public void Initialize() {
            if (initialize != null) {
                initialize();
            }
        }

        public void UpdateForReload(PythonModule reloaded) {
            this.__dict__ = reloaded.__dict__;
            this.initialize = reloaded.initialize;

            Initialize();
        }
        #endregion

        #region Internal API surface
        internal ICustomAttributes InnerModule {
            get {
                return innerMod;
            }
            set {
                innerMod = value;
            }
        }

        /// <summary>
        /// True if the package has been imported into this module, otherwise false.
        /// </summary>
        internal bool PackageImported {
            get {
                return packageImported;
            }
            set {
                packageImported = true;
            }
        }
        #endregion

        #region ICallerContext Members

        PythonModule ICallerContext.Module {
            get { return this; }
        }

        public SystemState SystemState {
            get {
                Debug.Assert(systemState != null);
                return systemState; 
            }
        }

        object ICallerContext.Locals {
            get { return __dict__; }
        }

        object ICallerContext.Globals {
            get { return __dict__; }
        }

        object ICallerContext.GetStaticData(int index) {
            throw new InvalidOperationException("not supported on standard modules");
        }

        CallerContextFlags ICallerContext.ContextFlags {
            get { return contextFlags; }
            set { contextFlags = value; }
        }

        bool ICallerContext.TrueDivision {
            get {
                return trueDivision;
            }
            set {
                trueDivision = value;
            }
        }

        CompilerContext ICallerContext.CreateCompilerContext() {
            CompilerContext context = new CompilerContext();
            context.TrueDivision = ((ICallerContext)this).TrueDivision;
            return context;
        }

        #endregion

        #region IFrameEnvironment Members

        public object GetGlobal(string name) {
            throw new InvalidOperationException("not supported on standard modules");
        }

        public void SetGlobal(string name, object value) {
            throw new InvalidOperationException("not supported on standard modules");
        }

        public void DelGlobal(string name) {
            throw new InvalidOperationException("not supported on standard modules");
        }

        #endregion

        #region Object Overrides

        [PythonName("__str__")]
        public override string ToString() {
            if (Filename == null) {
                if (innerMod != null) 
                    return String.Format("<module '{0}' (CLS module)>", ModuleName);
                else 
                    return String.Format("<module '{0}' (built-in)>", ModuleName);
            } else 
                return String.Format("<module '{0}' from '{1}'>", ModuleName, Filename);
        }

        #endregion

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            if (__dict__.TryGetValue(name, out value)) {
                IContextAwareMember icaa = value as IContextAwareMember;
                if (icaa == null || icaa.IsVisible(context)) {
                    return true;
                }
                value = null;
            }
            if (name == SymbolTable.Dict) { value = __dict__; return true; }
            if (packageImported) return innerMod.TryGetAttr(context, name, out value);
            return false;
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            __dict__[name] = value;
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            __dict__.Remove(name);
        }

        public List GetAttrNames(ICallerContext context) {
            List ret;
            if ((context.ContextFlags & CallerContextFlags.ShowCls) == 0) {
                ret = new List();
                foreach (KeyValuePair<object, object> kvp in __dict__) {
                    IContextAwareMember icaa = kvp.Value as IContextAwareMember;
                    if (icaa == null || icaa.IsVisible(context)) {
                        ret.AddNoLock(kvp.Key);
                    }
                }
            } else {
                ret = List.Make(__dict__.Keys);
            }

            ret.AddNoLock("__dict__");
            if (packageImported) {
                foreach (object o in innerMod.GetAttrNames(context)) {
                    if (o is string && (string)o == "__dict__") continue;
                    if (!((IDictionary<object, object>)__dict__).ContainsKey(o)) ret.AddNoLock(o);
                }
            }
            return ret;
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            ///!!! reflected package?
            return ((IDictionary<object, object>)__dict__);
        }
        #endregion
    }

    [Flags]
    public enum CallerContextFlags {
        None = 0,
        ShowCls = 0x01,
    }

    /// <summary>
    /// Wraps values for Builtins in a module.  Not a descriptor because
    /// modules attribute lookups don't do descriptor lookups.
    /// </summary>
    public sealed class BuiltinWrapper {
        private object originalValue;
        public object currentValue;
        private string name;

        public BuiltinWrapper(object value, string name) {
            originalValue = value;
            currentValue = new Uninitialized(name);
            this.name = name;
        }


        public object CurrentValue {
            get {
                if (currentValue is Uninitialized) return originalValue;
                return currentValue;
            }
            set {
                if (value is Uninitialized && currentValue is Uninitialized) throw Ops.NameError("name '{0}' is not defined", name);
                currentValue = value;
            }
        }
    }

}
