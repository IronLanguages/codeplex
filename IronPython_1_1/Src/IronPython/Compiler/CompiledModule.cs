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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using IronPython.Runtime;
using IronPython.Hosting;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Compiler {
    /// <summary>
    /// A Python module gets compiled into a CLI assembly which contains a type that inherits from CompiledModule. 
    /// CompiledModule is comparable with CompiledCode since it contains the code for a PythonModule, but not the state 
    /// associated with the module.
    /// Note that a CompiledModule does not directly relate to pre-compiled script code, though it is used
    /// in that context too.
    /// </summary>
    [PythonType(typeof(Dict))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class CompiledModule : CustomSymbolDict {

        internal static PythonModule Load(string moduleName, Type compiledModuleType, SystemState state) {
            CompiledModule compiledModule = (CompiledModule)compiledModuleType.GetConstructor(Type.EmptyTypes).Invoke(Ops.EMPTY);
            return compiledModule.Load(moduleName, new InitializeModule(compiledModule.Initialize), state);
        }

        internal PythonModule Load(string moduleName, InitializeModule init, SystemState state) {
            InitializeBuiltins();

            PythonModule pmod = new PythonModule(moduleName, this, state, init);
            this.Module = pmod;
            return pmod;
        }

        // The generated type has a static field to access the PythonModule. This allows quick access from
        // the generated IL.
        internal const string ModuleFieldName = "myModule__py";

        private PythonModule Module {
            get {
                FieldInfo ti = this.GetType().GetField(ModuleFieldName);
                return (PythonModule)ti.GetValue(this);
            }

            set {
                FieldInfo ti = this.GetType().GetField(ModuleFieldName);
                if (ti != null) ti.SetValue(this, value);
            }
        }

        /// <summary>
        /// This is the entry-point which corresponds to the global code of the module.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// This initializes generated static fields representing builtins. Use of static fields enables optimized
        /// access to builtins. 
        /// Also, see StaticFieldBuiltinSlot for how we deal with name clashes between builtins and globals.
        /// </summary>
        internal void InitializeBuiltins() {

            foreach (FieldInfo fi in this.GetType().GetFields()) {
                if (fi.FieldType != typeof(object) && fi.FieldType != typeof(BuiltinWrapper)) continue;
                if (!fi.IsStatic) continue;

                if (fi.Name == "__debug__") {
                    fi.SetValue(null, new BuiltinWrapper(Options.DebugMode, "__debug__"));
                    continue;
                }

                object bi;
                if (TypeCache.Builtin.TryGetSlot(DefaultContext.Default, SymbolTable.StringToId(fi.Name), out bi)) {
                    Debug.Assert(fi.FieldType == typeof(BuiltinWrapper));

                    fi.SetValue(null, new BuiltinWrapper(Ops.GetDescriptor(bi, null, TypeCache.Builtin), fi.Name));
                    continue;
                }

                if (fi.GetValue(null) == null) {
                    Debug.Assert(fi.FieldType == typeof(object));

                    fi.SetValue(null, Uninitialized.instance);
                }
            }
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(DynamicType cls, object seq) {
            return Dict.FromKeys(cls, seq, null);
        }

        [PythonClassMethod("fromkeys")]
        public static object fromkeys(DynamicType cls, object seq, object value) {
            return Dict.FromKeys(cls, seq, value);
        }


    }
}
