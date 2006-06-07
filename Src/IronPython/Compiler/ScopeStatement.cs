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
using System.Reflection;
using System.Diagnostics;
using IronPython.Runtime;

using System.Reflection.Emit;

namespace IronPython.Compiler {
    public abstract partial class ScopeStatement : Stmt {
        public class Binding {
            [Flags]
            private enum Kind {
                Free = 0,
                Bound = 0x1,        // Defined in the scope
                Global = 0x2,       // Global name
                Environment = 0x4,  // Part of the scope's environment (referenced from enlosed scopes)

                ValidityMask = Bound | Global | Environment,

                Deleted = 0x8,      // del was detected for this name in the scope
                Assigned = 0x10,    // assignment (definition) was detected for the name in the scope
                Parameter = 0x20,   // the variable is a parameter
            }
            private Kind kind;
            private int index;          // Flow analysis - index into the bit vector
            private bool unassigned;    // Name ever referenced without being bound
            private bool uninitialized; // Name ever used either uninitialized or after deletion

            public override string ToString() {
                return string.Format("Binding [({0}), idx {1}, unb {2}]", kind, index, unassigned);
            }

            public bool IsFree {
                get { return kind == Kind.Free; }
            }

            public bool IsBound {
                get { return (kind & Kind.Bound) != 0; }
            }

            public bool IsEnvironment {
                get { return (kind & Kind.Environment) != 0; }
            }

            public bool IsLocal {
                get { return (kind & Kind.Bound) != 0 && (kind & Kind.Global) == 0; }
            }

            public bool IsGlobal {
                get { return (kind & Kind.Global) != 0; }
            }

            public bool IsDeleted {
                get { return (kind & Kind.Deleted) != 0; }
            }

            public bool IsAssigned {
                get { return (kind & Kind.Assigned) != 0; }
            }

            public bool IsParameter {
                get { return (kind & Kind.Parameter) != 0; }
            }

            public void Bind() {
                kind |= Kind.Bound | Kind.Assigned;
            }

            public void BindParameter() {
                kind |= Kind.Bound | Kind.Assigned | Kind.Parameter;
            }

            public void BindGlobal() {
                kind |= Kind.Bound | Kind.Global;
            }

            public void BindDeleted() {
                kind |= Kind.Bound | Kind.Deleted;
            }

            public void Reference() {
            }

            public void MakeEnvironment() {
                kind |= Kind.Environment;
            }

            public void MakeUnboundGlobal() {
                kind |= Kind.Global;
            }

            internal int Index {
                get { return index; }
                set { index = value; }
            }

            internal bool Unassigned {
                get { return unassigned; }
            }

            internal void UnassignedUse() {
                unassigned = true;
            }

            internal bool Uninitialized {
                get { return uninitialized; }
            }

            internal void UninitializedUse() {
                uninitialized = true;
            }

            [Conditional("DEBUG")]
            public void Validate() {
                switch (kind & Kind.ValidityMask) {
                    case Kind.Free:                             // free variable
                    case Kind.Global:                           // free bound to global
                    case Kind.Bound:                            // bound local
                    case Kind.Bound | Kind.Global:              // bound global
                    case Kind.Bound | Kind.Environment:         // bound local promoted to environment
                        break;

                    // invalid cases
                    case Kind.Environment:
                    case Kind.Environment | Kind.Global:
                    case Kind.Bound | Kind.Environment | Kind.Global:
                        Debug.Fail("invalid binding");
                        break;
                }
            }
        }

        [Flags]
        public enum ScopeFlags {
            ContainsImportStar = 0x01,              // from module import *
            ContainsUnqualifiedExec = 0x02,         // exec "code"
            ContainsNestedFreeVariables = 0x04,     // nested function with free variable
            ContainsFreeVariables = 0x08,

            IsClosure = 0x10,                       // Is a closure (receives environment as parameter)
            HasEnvironment = 0x20,                  // Defines its own environment for lexically nested scopes
                                                    // (or passes environment through for access across scopes)
        }

        public ScopeStatement parent;
        public Stmt body;

        protected ScopeFlags scopeInfo;

        // Names referenced in the scope with and their binding kind
        protected Dictionary<SymbolId, Binding> names = new Dictionary<SymbolId, Binding>();

        // Names that need to be promoted to the environment and their assigned index in the environment
        protected Dictionary<SymbolId, EnvironmentReference> environment;
        protected EnvironmentFactory environmentFactory;
        public int tempsCount;

        protected ScopeStatement(Stmt body) {
            this.body = body;
        }

        internal ScopeFlags ScopeInfo {
            get { return scopeInfo; }
        }
        internal bool IsClosure {
            get { return (scopeInfo & ScopeFlags.IsClosure) != 0; }
            set { if (value) scopeInfo |= ScopeFlags.IsClosure; else scopeInfo &= ~ScopeFlags.IsClosure; }
        }
        internal bool HasEnvironment {
            get { return (scopeInfo & ScopeFlags.HasEnvironment) != 0; }
            set { if (value) scopeInfo |= ScopeFlags.HasEnvironment; else scopeInfo &= ~ScopeFlags.HasEnvironment; }
        }
        internal bool ContainsImportStar {
            get { return (scopeInfo & ScopeFlags.ContainsImportStar) != 0; }
            set { if (value) scopeInfo |= ScopeFlags.ContainsImportStar; else scopeInfo &= ~ScopeFlags.ContainsImportStar; }
        }
        internal bool ContainsUnqualifiedExec {
            get { return (scopeInfo & ScopeFlags.ContainsUnqualifiedExec) != 0; }
            set { if (value) scopeInfo |= ScopeFlags.ContainsUnqualifiedExec; else scopeInfo &= ~ScopeFlags.ContainsUnqualifiedExec; }
        }
        internal bool ContainsNestedFreeVariables {
            get { return (scopeInfo & ScopeFlags.ContainsNestedFreeVariables) != 0; }
            set { if (value) scopeInfo |= ScopeFlags.ContainsNestedFreeVariables; else scopeInfo &= ~ScopeFlags.ContainsNestedFreeVariables; }
        }

        public Dictionary<SymbolId, Binding> Bindings {
            get { return names; }
        }

        protected Binding Get(SymbolId name) {
            Binding binding;
            if (!names.TryGetValue(name, out binding)) {
                binding = new Binding();
                names[name] = binding;
            }
            return binding;
        }

        public void Bind(SymbolId name) {
            Get(name).Bind();
        }

        public void BindParameter(SymbolId name) {
            Get(name).BindParameter();
        }

        public void Reference(SymbolId name) {
            Get(name).Reference();
        }

        public void BindGlobal(SymbolId name) {
            Get(name).BindGlobal();
        }

        public void BindDeleted(SymbolId name) {
            Get(name).BindDeleted();
        }

        private void MakeEnvironment(SymbolId name) {
            Get(name).MakeEnvironment();
        }

        public virtual bool TryGetBinding(SymbolId name, out Binding binding) {
            binding = null;
            return false;
        }

        public bool BindInParent(SymbolId name, Binder binder) {
            Binding binding;
            ContainsNestedFreeVariables = true;
            if (TryGetBinding(name, out binding)) {
                if (binding.IsGlobal) {
                    return false;
                } else {
                    if (binding.IsDeleted) {
                        binder.ReportSyntaxError(string.Format("can not delete variable '{0}' referenced in nested scope", name.GetString()), this);
                    }
                    MakeEnvironment(name);
                    HasEnvironment = true;
                    return true;
                }
            }
            if (parent != null) {
                if (parent.BindInParent(name, binder)) {
                    IsClosure = true;
                    HasEnvironment = true;
                    return true;
                }
            }
            return false;
        }

        public void BindNames(GlobalSuite globals, Binder binder) {
            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                Binding b = kv.Value;
                SymbolId n = kv.Key;

                // Global binding
                if (b.IsGlobal) {
                    globals.Bind(n);
                    continue;
                }

                // Free variable binding
                if (b.IsFree) {
                    // Bind in the parent
                    if (parent != null) {
                        if (parent.BindInParent(kv.Key, binder)) {
                            IsClosure = true;
                            continue;
                        }
                    }

                    // Free, but not defined in parent scopes ==> global
                    globals.Bind(n);
                    b.MakeUnboundGlobal();
                    continue;
                }

                // Defined in this scope
                if (b.IsBound) {
                    continue;
                }

                Debug.Fail("Unbound name", n.GetString());
            }
        }

        protected void PromoteLocalsToEnvironment() {
            HasEnvironment = true;
            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                if (kv.Value.IsLocal) {
                    kv.Value.MakeEnvironment();
                }
            }
        }

        private int CalculateEnvironmentSize() {
            int environmentSize = tempsCount;
            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                if (kv.Value.IsEnvironment) {
                    environmentSize++;
                }
            }
            return environmentSize;
        }

        private void EmitOuterLocalIDs(CodeGen cg) {
            if (EmitLocalDictionary) {
                int size = 0;
                foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                    if (kv.Value.IsFree) size++;
                }
                // Emit null if no outer symbol IDs
                if (size == 0) {
                    cg.EmitExprOrNone(null);
                } else {
                    cg.EmitInt(size);
                    cg.Emit(OpCodes.Newarr, typeof(SymbolId));
                    int index = 0;
                    foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                        if (kv.Value.IsFree) {
                            cg.Emit(OpCodes.Dup);
                            cg.EmitInt(index++);
                            cg.Emit(OpCodes.Ldelema, typeof(SymbolId));
                            cg.EmitSymbolIdId(kv.Key);
                            cg.Emit(OpCodes.Call, typeof(SymbolId).GetConstructor(new Type[] { typeof(int) }));
                        }
                    }
                }
            } else {
                cg.EmitExprOrNone(null);
            }
        }

        private void EmitEnvironmentIDs(CodeGen cg) {
            int size = 0;
            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                if (kv.Value.IsEnvironment) size++;
            }
            // Create the array for the names
            cg.EmitInt(size);
            cg.Emit(OpCodes.Newarr, typeof(SymbolId));

            int index = 0;
            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                if (kv.Value.IsEnvironment) {
                    cg.Emit(OpCodes.Dup);
                    cg.EmitInt(index++);
                    cg.Emit(OpCodes.Ldelema, typeof(SymbolId));
                    cg.EmitSymbolIdId(kv.Key);
                    cg.Emit(OpCodes.Call, typeof(SymbolId).GetConstructor(new Type[] { typeof(int) }));
                }
            }
        }

        protected Slot CreateEnvironment(CodeGen cg) {
            // Get the environment size
            int size = CalculateEnvironmentSize();

            // Find the right environment type
            ConstructorInfo ctor;
            EnvironmentFactory esf;
            Type envType = GetEnvironmentType(size, cg, out ctor, out esf);

            // Emit the static link for the environment constructor
            cg.EmitStaticLinkOrNull();
            cg.EmitCallerContext();
            // Emit the names array for the environment constructor
            EmitEnvironmentIDs(cg);
            EmitOuterLocalIDs(cg);
            cg.EmitNew(ctor);

            // Store the environment reference in the local
            Slot environmentSlot = cg.GetFrameSlot(envType);
            environmentSlot.EmitSet(cg);

            // Remember the environment factory for parent access
            environmentFactory = esf;

            // Create environment references
            environment = new Dictionary<SymbolId, EnvironmentReference>();

            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                if (!kv.Value.IsEnvironment) continue;
                SymbolId name = kv.Key;
                EnvironmentReference er = esf.MakeEnvironmentReference(name);
                Slot slot = er.CreateSlot(environmentSlot);
                Slot current;
                if (cg.Names.Slots.TryGetValue(name, out current)) {
                    slot.EmitSet(cg, current);
                } else {
                    slot.EmitSetUninitialized(cg, name);
                }

                // The full slot goes to the codegen's namespace
                cg.Names.SetSlot(name, slot);
                // Environment reference goes to the environment
                environment[name] = er;
            }

            return environmentSlot;
        }

        protected void CreateLocalSlots(CodeGen cg) {
            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                if (kv.Value.IsLocal) {
                    Slot local = cg.Names.EnsureLocalSlot(kv.Key);
                    local.Local = true;
                    if (kv.Value.Unassigned) {
                        local.EmitSetUninitialized(cg, kv.Key);
                    }
                }
            }
        }
        protected void CreateGlobalSlots(CodeGen to, CodeGen from) {
            to.Names.Globals = from.Names.Globals.Relocate(to.ContextSlot);

            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                if (kv.Value.IsGlobal) {
                    Slot global = to.Names.Globals.GetSlot(kv.Key);
                    // if unbound global (free variable not defined in outer scope),
                    // create backed environment slot
                    if (!kv.Value.IsBound && this.ContainsUnqualifiedExec) {
                        global = new EnvironmentBackedSlot(global, to.EnvironmentSlot, kv.Key);
                    }
                    to.Names.SetSlot(kv.Key, global);
                }
            }
        }

        private bool TryGetEnvironmentReference(SymbolId name, out EnvironmentReference er) {
            Binding binding;
            if (TryGetBinding(name, out binding)) {
                Debug.Assert(binding.IsBound);
                Debug.Assert(environment.ContainsKey(name));
                er = environment[name];
                return true;
            }
            er = null;
            return false;
        }

        protected void CreateClosureSlots(CodeGen cg) {
            CreateClosureSlots(cg.StaticLinkSlot, cg.Names);
        }

        private void CreateClosureSlots(Slot staticLink, Namespace nspace) {
            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                if (!kv.Value.IsFree) continue;

                // Find the slot
                Slot instance = staticLink;
                ScopeStatement current = parent;

                for (; ; ) {
                    instance = new CastSlot(instance, current.environmentFactory.EnvironmentType);
                    Debug.Assert(current != null, "cannot resolve closure", kv.Key.GetString());
                    if (current.environment != null) {
                        EnvironmentReference er;

                        // Is the slot in this environment?
                        if (current.TryGetEnvironmentReference(kv.Key, out er)) {
                            nspace.SetSlot(kv.Key, er.CreateSlot(instance));
                            break;
                        }
                    }
                    instance = current.environmentFactory.MakeParentSlot(instance);
                    current = current.parent;
                }
            }
        }

        private bool? emitLocalDictionary = null;

        protected bool EmitLocalDictionary {
            get {
                if (Options.Frames) {
                    return true;
                }

                if (emitLocalDictionary == null) {
                    emitLocalDictionary = MightNeedLocalsWalker.CheckMightNeedLocalsDictionary(body);
                }
                return (bool)emitLocalDictionary;
            }
        }

        public override string GetDocString() {
            return body.GetDocString();
        }
    }

    internal class MightNeedLocalsWalker : AstWalker {
        public static bool CheckMightNeedLocalsDictionary(Stmt s) {
            MightNeedLocalsWalker w = new MightNeedLocalsWalker();
            s.Walk(w);
            return w.MightNeedLocals;
        }

        private bool MightNeedLocals = false;

        private MightNeedLocalsWalker() { }

        #region AstWalker Method Overrides

        public override void PostWalk(CallExpr node) {
            MightNeedLocals |= node.MightNeedLocalsDictionary();
        }

        public override void PostWalk(ExecStmt node) {
            MightNeedLocals |= node.NeedsLocalsDictionary();
        }

        public override bool Walk(ClassDef node) {
            // Do not recurse into nested classes
            return false;
        }

        public override bool Walk(FuncDef node) {
            // Do not recurse into nested functions
            return false;
        }

        #endregion
    }

    public class GlobalSuite : ScopeStatement {
        public GlobalSuite(Stmt body)
            : base(body) {
        }

        public override void Emit(CodeGen cg) {
            CreateGlobalSlots(cg);
            body.Emit(cg);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                body.Walk(w);
            }
            w.PostWalk(this);
        }

        public void CreateGlobalSlots(CodeGen cg) {
            foreach (KeyValuePair<SymbolId, Binding> kv in names) {
                Slot slot = cg.Names.Globals.GetOrMakeSlot(kv.Key);
                if (kv.Value.IsGlobal) {
                    cg.Names.SetSlot(kv.Key, slot);
                } else {
                    cg.Names.EnsureLocalSlot(kv.Key);
                }
            }
        }
    }
}
