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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.CodeDom;
using System.Text.RegularExpressions;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.CodeDom;
using IronPython.Compiler.Generation;

namespace IronPython.Compiler.AST {
    /// <summary>
    /// Summary description for ClassDef.
    /// </summary>
    public class ClassDef : ScopeStatement {
        private Location header;
        private SymbolId name;
        private Expr[] bases;
        private static int index = 0;

        public ClassDef(SymbolId name, Expr[] bases, Stmt body)
            : base(body) {
            this.name = name;
            this.bases = bases;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }
        
        public SymbolId Name {
            get { return name; }
            set { name = value; }
        }

        public IList<Expr> Bases {
            get { return bases; }
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, header);

            CodeGen icg = CreateClassMaker(cg);
            try {
                // emit call to MakeClass(ICallerContext mod, string modName, string name, Tuple bases, IDictionary<object, object> vars) 

                // ICallerContext mod
                cg.EmitModuleInstance();

                // string modName (can't pull from context, could be changed)
                cg.EmitGetGlobal(SymbolTable.Name);
                cg.Emit(OpCodes.Castclass, typeof(string));

                // class name
                cg.EmitString(name.GetString());

                // bases array
                cg.EmitObjectArray(bases);
                cg.EmitCall(typeof(Ops), "MakeTuple");

                // vars
                cg.EmitEnvironmentOrNull();
                cg.EmitContextOrNull();
                cg.EmitCall(icg.MethodInfo);

                cg.EmitCall(typeof(Ops), "MakeClass");

                // store result to class name
                cg.EmitSet(name);
            } finally {
                icg.Dispose();
            }
        }

        private CodeGen CreateClassMaker(CodeGen cg) {
            Type[] signature = new Type[2];
            signature[0] = IsClosure ? cg.EnvironmentSlot.Type : typeof(FunctionEnvironmentDictionary);
            signature[1] = cg.ContextSlot != null ? cg.ContextSlot.Type : typeof(ICallerContext);
            CodeGen icg = cg.DefineUserHiddenMethod(name.GetString() + "$maker" + System.Threading.Interlocked.Increment(ref index),
                typeof(IDictionary<object, object>), signature);

            if (IsClosure) icg.StaticLinkSlot = icg.GetArgumentSlot(0);
            if (cg.ContextSlot != null) icg.ContextSlot = icg.GetArgumentSlot(1);

            if (icg.ContextSlot != null) {
                icg.ModuleSlot = new PropertySlot(icg.ContextSlot, typeof(ICallerContext).GetProperty("Module"));
            } else if (icg.StaticLinkSlot != null) {
                icg.ModuleSlot = new PropertySlot(icg.ContextSlot, typeof(ICallerContext).GetProperty("Module"));
            }

            icg.Names = CodeGen.CreateLocalNamespace(icg);
            icg.Context = cg.Context;

            // emit class initialization

            EmitClassInitialization(cg, icg);
            icg.EnvironmentSlot.EmitGet(icg);
            icg.Emit(OpCodes.Ret);
            return icg;
        }

        private void EmitClassInitialization(CodeGen cg, CodeGen icg) {
            // Populate the namespace with slots
            PrepareForEmit(cg, icg);

            body.Emit(icg);
        }

        internal void PrepareForEmit(CodeGen cg, CodeGen icg) {
            Debug.Assert(cg != null, "null codegen");
            Debug.Assert(icg != null, "null init codegen");

            PromoteLocalsToEnvironment();
            icg.ContextSlot = icg.EnvironmentSlot = CreateEnvironment(icg);
            CreateGlobalSlots(icg, cg);
            CreateClosureSlots(icg);

            // Detect class locals that may be used uninitialized
            // and create global-backed slots for them
            FlowChecker.Check(this);
            CreateBackedSlots(icg);

            string doc = body.GetDocString();
            if (doc != null) {
                icg.EmitString(doc);
                icg.EmitSet(SymbolTable.Doc);
            }
        }

        private void CreateBackedSlots(CodeGen icg) {
            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                if (kv.Value.IsLocal && kv.Value.Uninitialized) {
                    Slot global = icg.Names.Globals.GetOrMakeSlot(kv.Key);
                    Slot attribute = icg.Names.Slots[kv.Key];
                    icg.Names.Slots[kv.Key] = new GlobalBackedSlot(attribute, global);
                }
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (Expr e in bases) e.Walk(w);
                body.Walk(w);
            }
            w.PostWalk(this);
        }
    }
}
