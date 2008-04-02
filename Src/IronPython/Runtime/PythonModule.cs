/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using System.Diagnostics;

using IronPython.Compiler;
using IronPython.Modules;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;

using System.Threading;

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
    public class PythonModule : ICustomAttributes, IModuleEnvironment, ICodeFormattable {
        internal IAttributesDictionary __dict__;

        private ICustomAttributes innerMod;
        private bool packageImported;
        private CallerContextAttributes contextFlags;
        private bool trueDivision;
        private InitializeModule initialize;
        private SystemState systemState;

        #region Python constructors

        [PythonName("__new__")]
        public static PythonModule MakeModule(ICallerContext context, DynamicType cls, params object[] args\u03c4) {
            if (cls.IsSubclassOf(TypeCache.Module)) {
                return new PythonModule(context);
            }
            throw Ops.TypeError("{0} is not a subtype of module", cls.__name__);
        }

        [PythonName("__new__")]
        public static PythonModule MakeModule(ICallerContext context, DynamicType cls, [ParamDict] Dict kwDict\u03c4, params object[] args\u03c4) {
            return MakeModule(context, cls, args\u03c4);
        }
        
        [PythonName("__init__")]
        public void Initialize(ICallerContext context, string name) {
            Initialize(context, name, null);
        }

        [PythonName("__init__")]
        public void Initialize(ICallerContext context, string name, string documentation) {
            EnsureDict();

            ModuleName = name;

            if (documentation != null) {
                __dict__[SymbolTable.Doc] = documentation;
            }
        }

        #endregion

        #region Internal constructors

        internal PythonModule(ICallerContext context) {
            contextFlags = context.ContextFlags;
            systemState = context.SystemState;
        }

        internal PythonModule(string name, IAttributesDictionary dict, SystemState state)
            : this(name, dict, state, null, CallerContextAttributes.None) {
        }

        internal PythonModule(string name, CompiledModule compiledModule, SystemState state, InitializeModule init)
            : this(name, compiledModule, state, init, CallerContextAttributes.None) {
        }

        internal PythonModule(string name, IAttributesDictionary dict, SystemState state, InitializeModule init, CallerContextAttributes callerContextFlags) {
            Debug.Assert(state != null);

            __dict__ = dict;
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
            get {
                object res;
                if (__dict__ != null && __dict__.TryGetValue(SymbolTable.Name, out res))
                    return res as string;

                return "?";
            }
            [PythonName("__name__")]
            set { EnsureDict(); __dict__[SymbolTable.Name] = value; }
        }
        public string Filename {
            [PythonName("__file__")]
            get {
                if (__dict__ != null) {

                    object file;
                    if (__dict__.TryGetValue(SymbolTable.File, out file)) {
                        string sfile = file as string;
                        if (sfile != null) return sfile;
                    }
                }
                return null;
            }
            [PythonName("__file__")]
            set { EnsureDict();  __dict__[SymbolTable.File] = value; }
        }

        public void Initialize() {            
            Debug.Assert(__dict__ != null, "Generated modules should always get a __dict__");

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

        IAttributesDictionary ICallerContext.Globals {
            get { return __dict__; }
        }

        object ICallerContext.GetStaticData(int index) {
            throw new InvalidOperationException("not supported on standard modules");
        }

        CallerContextAttributes ICallerContext.ContextFlags {
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
            if((((ICallerContext)this).ContextFlags & CallerContextAttributes.AllowWithStatement)!=0) {
                context.AllowWithStatement = true;
            }
            return context;
        }

        #endregion

        #region IModuleEnvironment Members

        public object GetGlobal(SymbolId symbol) {
            throw new InvalidOperationException("not supported on standard modules");
        }

        public bool TryGetGlobal(SymbolId symbol, out object value) {
            throw new InvalidOperationException("not supported on standard modules");
        }

        public void SetGlobal(SymbolId symbol, object value) {
            throw new InvalidOperationException("not supported on standard modules");
        }

        public void DelGlobal(SymbolId symbol) {
            throw new InvalidOperationException("not supported on standard modules");
        }

        #endregion

        #region Object Overrides

        [PythonName("__str__")]
        public override string ToString() {
            if (Filename == null) {
                if (innerMod != null) {
                    ReflectedPackage rp = innerMod as ReflectedPackage;
                    if (rp != null) {
                        if (rp.packageAssemblies.Count != 1) {
                            return String.Format("<module '{0}' (CLS module, {1} assemblies loaded)>", ModuleName, rp.packageAssemblies.Count);
                        } else {
                            return String.Format("<module '{0}' (CLS module from {1})>", ModuleName, rp.packageAssemblies[0].FullName);
                        }
                    } else {
                        return String.Format("<module '{0}' (CLS module)>", ModuleName);
                    }
                } else
                    return String.Format("<module '{0}' (built-in)>", ModuleName);
            } else
                return String.Format("<module '{0}' from '{1}'>", ModuleName, Filename);
        }

        #endregion

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            if (__dict__ != null && __dict__.TryGetValue(name, out value)) {
                if (value == Uninitialized.instance) return false;

                IContextAwareMember icaa = value as IContextAwareMember;
                if (icaa == null || icaa.IsVisible(context)) {
                    return true;
                }
                value = null;
            }

            if (name == SymbolTable.Dict) {
                if (packageImported) value = innerMod.GetAttrDict(context);
                else value = __dict__;

                return true;
            }

            if (packageImported) return innerMod.TryGetAttr(context, name, out value);
            if (TypeCache.Module.TryGetAttr(context, this, name, out value)) return true;

            return false;
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            if (name == SymbolTable.Dict) throw Ops.TypeError("readonly attribute");

            EnsureDict();
            TypeCache.Module.SetAttrWithCustomDict(context, this, __dict__, name, value);
        }

        private void EnsureDict() {
            if (__dict__ == null) {
                Interlocked.CompareExchange<IAttributesDictionary>(ref __dict__, new FieldIdDict(), null);
            }
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            if (name == SymbolTable.Dict) throw Ops.TypeError("can't set attributes of built-in/extension type 'module'");

            if (__dict__ != null && TypeCache.Module.DeleteAttrWithCustomDict(context, this, __dict__, name)) {
                object dummy;
                if (packageImported && innerMod.TryGetAttr(context, name, out dummy))
                    innerMod.DeleteAttr(context, name);
                return;
            }

            if (packageImported) {
                innerMod.DeleteAttr(context, name);
            } else
                throw Ops.AttributeErrorForMissingAttribute(ModuleName, name);
        }

        public List GetAttrNames(ICallerContext context) {
            if (__dict__ == null) throw Ops.TypeError("module.__dict__ is not a dictionary");

            List ret;
            if ((context.ContextFlags & CallerContextAttributes.ShowCls) == 0) {
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
            return TypeCache.Module.GetAttrDictWithCustomDict(context, this, __dict__ == null ? new Dict(1) : __dict__);
        }
        #endregion

        internal void SetImportedAttr(ICallerContext context, SymbolId name, object value) {
            EnsureDict();
            __dict__[name] = value;
        }

        #region ICodeFormattable Members

        public string ToCodeString() {
            return ToString();
        }

        #endregion
    }

    [Flags]
    public enum CallerContextAttributes {
        None = 0,
        ShowCls = 0x01,
        AllowWithStatement = 0x02,
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
            currentValue = Uninitialized.instance;
            this.name = name;
        }


        public object CurrentValue {
            get {
                if (currentValue == Uninitialized.instance) return originalValue;
                return currentValue;
            }
            set {
                if (value == Uninitialized.instance && currentValue == Uninitialized.instance) throw Ops.NameError("name '{0}' is not defined", name);
                currentValue = value;
            }
        }
    }

}
