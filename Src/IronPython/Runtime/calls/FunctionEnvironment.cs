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
using System.Diagnostics;

namespace IronPython.Runtime.Calls {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal sealed class EnvironmentIndexAttribute : PythonHiddenFieldAttribute {
        public readonly int index;
        public EnvironmentIndexAttribute(int index) {
            this.index = index;
        }
    }

    [PythonType(typeof(Dict))]
    public abstract class FunctionEnvironmentDictionary : CustomSymbolDict, IModuleEnvironment {
        [PythonHiddenField]
        public FunctionEnvironmentDictionary parent;
        [PythonHiddenField]
        public IModuleEnvironment context;
        [PythonHiddenField]
        protected SymbolId[] names;
        private SymbolId[] outer;       // outer scopes
        private SymbolId[] extra;       // extra keys

        public FunctionEnvironmentDictionary() {
            extra = new SymbolId[0];
            names = new SymbolId[0];
        }

        protected FunctionEnvironmentDictionary(FunctionEnvironmentDictionary parent, IModuleEnvironment context, SymbolId[] names, SymbolId[] outer) {
            this.parent = parent;
            this.context = context;
            this.names = names;
            this.outer = outer;

            if (names != null) {
                if (outer != null) {
                    extra = new SymbolId[names.Length + outer.Length];
                    Array.Copy(names, extra, names.Length);
                    Array.Copy(outer, 0, extra, names.Length, outer.Length);
                } else extra = names;
            } else extra = outer;
        }

        public override SymbolId[] GetExtraKeys() {
            return extra;
        }

        protected static Exception OutOfRange(int index) {
            string msg = string.Format("FunctionEnvironment - index out of range: {0}", index);
            Debug.Fail(msg);
            throw new IndexOutOfRangeException(msg);
        }

        protected bool TryGetOuterValue(SymbolId key, out object value) {
            if (outer != null) {
                // Does the key belong to any of the outer scopes?
                for (int index = 0; index < outer.Length; index++) {
                    if (outer[index] == key) {
                        FunctionEnvironmentDictionary current = parent;

                        while (current != null) {
                            if (current.TryGetExtraValueRaw(key, out value)) return true;
                            current = current.parent;
                        }
                    }
                }
            }
            value = null;
            return false;
        }

        protected abstract object GetValueAtIndex(int index);

        private bool TryGetExtraValueRaw(SymbolId key, out object value) {
            for (int index = 0; index < names.Length; index++) {
                if (names[index] == key) {
                    value = GetValueAtIndex(index);
                    return true;
                }
            }
            value = null;
            return false;
        }

        #region ICallerContext Members

        public PythonModule Module {
            get { return context.Module; }
        }

        public SystemState SystemState {
            get {
                return context.SystemState;
            }
        }

        public object Locals {
            get { return this; }
        }

        public IAttributesDictionary Globals {
            get { return context.Globals; }
        }

        public object GetStaticData(int index) {
            return context.GetStaticData(index);
        }

        public CallerContextAttributes ContextFlags {
            get { return context.ContextFlags; }
            set { context.ContextFlags = value; }
        }

        public bool TrueDivision {
            get { return context.TrueDivision; }
            set { context.TrueDivision = value; }
        }

        public IronPython.Compiler.CompilerContext CreateCompilerContext() {
            return context.CreateCompilerContext();
        }

        #endregion

        #region IModuleEnvironment Members

        public object GetGlobal(SymbolId symbol) {
            return context.GetGlobal(symbol);
        }

        public bool TryGetGlobal(SymbolId symbol, out object value) {
            return context.TryGetGlobal(symbol, out value);
        }

        public void SetGlobal(SymbolId symbol, object value) {
            context.SetGlobal(symbol, value);
        }

        public void DelGlobal(SymbolId symbol) {
            context.DelGlobal(symbol);
        }

        #endregion
    }


    /// <summary>
    /// The environment for closures. The environment provides access to the variables
    /// defined in the enclosing lexical scopes.
    /// </summary>
    [PythonType(typeof(Dict))]
    public sealed class FunctionEnvironmentNDictionary : FunctionEnvironmentDictionary, ICloneable {
        // Array of the variables in the environment
        [PythonHiddenField]
        public object[] environmentValues;

        public FunctionEnvironmentNDictionary() {
        }

        public FunctionEnvironmentNDictionary(int size, FunctionEnvironmentDictionary parent, IModuleEnvironment context, SymbolId[] names, SymbolId[] outer)
            : base(parent, context, names, outer) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Temporary, "FuncEnv " + size.ToString());
            Debug.Assert(names.Length == size);
            this.environmentValues = new object[size];
        }

        protected override object GetValueAtIndex(int index) {
            return GetAtIndex(index);
        }

        private object GetAtIndex(int index) {
            return environmentValues[index];
        }
        private void SetAtIndex(int index, object value) {
            environmentValues[index] = value;
        }

        public override bool TrySetExtraValue(SymbolId key, object value) {
            for (int i = 0; i < names.Length; i++) {
                if (names[i] == key) {
                    SetAtIndex(i, value);
                    return true;
                }
            }
            return false;
        }

        public override bool TryGetExtraValue(SymbolId key, out object value) {
            for (int index = 0; index < names.Length; index++) {
                if (names[index] == key) {
                    value = GetAtIndex(index);
                    return true;
                }
            }
            return TryGetOuterValue(key, out value);
        }
    }
}
