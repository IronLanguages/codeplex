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

namespace IronPython.Runtime {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class EnvironmentIndexAttribute : PythonHiddenFieldAttribute {
        public readonly int index;
        public EnvironmentIndexAttribute(int index) {
            this.index = index;
        }
    }
    
    [PythonType(typeof(Dict))]
    public abstract class FunctionEnvironmentDictionary : CustomSymbolDict, IFrameEnvironment {
        [PythonHiddenField] public FunctionEnvironmentDictionary parent;
        [PythonHiddenField] public IFrameEnvironment context;
        [PythonHiddenField] protected SymbolId[] names;

        protected FunctionEnvironmentDictionary(FunctionEnvironmentDictionary parent, IFrameEnvironment context, SymbolId[] names) {
            this.parent = parent;
            this.context = context;
            this.names = names;
        }

        public override SymbolId[] GetExtraKeys() {
            return names;
        }
        
        protected static Exception OutOfRange(int index) {
            string msg = string.Format("FunctionEnvironment - index out of range: {0}", index);
            Debug.Fail(msg);
            throw new IndexOutOfRangeException(msg);
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

        public object Globals {
            get { return context.Globals; }
        }

        public object GetStaticData(int index) {
            return context.GetStaticData(index);
        }

        public CallerContextFlags ContextFlags {
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

        #region IGlobalEnvironment Members

        public object GetGlobal(string name) {
            return context.GetGlobal(name);
        }

        public void SetGlobal(string name, object value) {
            context.SetGlobal(name, value);
        }

        public void DelGlobal(string name) {
            context.DelGlobal(name);
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
        [PythonHiddenField]public object[] environmentValues;

        public FunctionEnvironmentNDictionary(int size, FunctionEnvironmentDictionary parent, IFrameEnvironment context, SymbolId[] names)
            : base(parent, context, names) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Temporary, "FuncEnv " + size.ToString());
            Debug.Assert(names.Length == size);
            this.environmentValues = new object[size];
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
            value = null;
            return false;
        }
    }
}
