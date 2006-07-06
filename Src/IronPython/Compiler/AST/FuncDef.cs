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
using System.Reflection.Emit;
using System.Diagnostics;
using System.Text;
using System.CodeDom;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Compiler.Generation;
using IronPython.CodeDom;

namespace IronPython.Compiler.Ast {
    [FlagsAttribute]
    public enum FunctionAttributes { None = 0, ArgumentList, KeywordDictionary }

    /// <summary>
    /// Summary description for FuncDef.
    /// </summary>
    public partial class FunctionDefinition : ScopeStatement {
        private static int counter = 0;

        const string tupleArgHeader = "tupleArg#";

        private Location header;
        private readonly SymbolId name;
        private readonly Expression[] parameters;
        private readonly Expression[] defaults;        
        private readonly FunctionAttributes flags;        
        private Expression decorators;        
        private string filename;        
        private int yieldCount = 0;        

        public FunctionDefinition(SymbolId name, Expression[] parameters, Expression[] defaults, FunctionAttributes flags, string sourceFile)
            : this(name, parameters, defaults, flags, null, sourceFile) {
        }

        public FunctionDefinition(SymbolId name, Expression[] parameters, Expression[] defaults, FunctionAttributes flags, Statement body, string sourceFile)
            : base(body) {
            this.name = name;
            this.parameters = parameters;
            this.defaults = defaults;
            this.flags = flags;
            this.decorators = null;
            this.filename = sourceFile;
        }

        public Location Header {
            get { return header; }
            set { header = value; }
        }

        public SymbolId Name {
            get { return name; }
        }

        public IList<Expression> Parameters {
            get { return parameters; }
        }

        public IList<Expression> Defaults {
            get { return defaults; }
        }

        public FunctionAttributes Flags {
            get { return flags; }
        }

        public Expression Decorators {
            get { return decorators; }
            set { decorators = value; }
        }

        public string FileName {
            get { return filename; }
            set { filename = value; }
        }

        public int YieldCount {
            get { return yieldCount; }
            set { yieldCount = value; }
        }

        public object MakeFunction(NameEnvironment environment) {
            string[] names = SymbolTable.IdsToStrings(makeNames(parameters));
            object[] defaults = Expression.Evaluate(this.defaults, environment);
            return new InterpFunction(names, defaults, Body, environment.Globals);
        }

        public override object Execute(NameEnvironment environment) {
            environment.Set(name.GetString(), MakeFunction(environment));
            return NextStatement;
        }

        public override void Walk(IAstWalker walker) {
            if (walker.Walk(this)) {
                foreach (Expression e in parameters) e.Walk(walker);
                foreach (Expression e in defaults) e.Walk(walker);
                Body.Walk(walker);
            }
            walker.PostWalk(this);
        }

        internal override void Emit(CodeGen cg) {
            cg.EmitPosition(Start, header);
            SignatureInfo sigInfo = GetSignature(cg);

            FlowChecker.Check(this);

            string mname = name.GetString() + "$f" + counter++;

            // create the new method & setup it's locals
            CodeGen impl = cg.DefineMethod(mname, typeof(object), sigInfo.ParamTypes, sigInfo.ParamNames);
            impl.Names = CodeGen.CreateLocalNamespace(impl);
            impl.Context = cg.Context;

            for (int arg = sigInfo.HasContext ? 1 : 0; arg < sigInfo.ParamNames.Length; arg++) {
                impl.Names.SetSlot(sigInfo.ParamNames[arg], impl.GetArgumentSlot(arg));
            }

            if (sigInfo.HasContext) {
                if (IsClosure) {
                    impl.StaticLinkSlot = impl.GetArgumentSlot(0);
                }
                impl.ContextSlot = impl.GetArgumentSlot(0);
                impl.ModuleSlot = new PropertySlot(impl.ContextSlot, typeof(ICallerContext).GetProperty("Module"));
            }

            // then generate the actual method
            EmitFunctionImplementation(impl, cg);
            if (NeedsWrapperMethod()) impl = MakeWrapperMethodN(cg, impl.MethodInfo, sigInfo.HasContext);

            //  Create instance of the Function? object
            Type funcType, targetType;
            using (impl) {
                GetFunctionType(out funcType, out targetType);
                cg.EmitModuleInstance();
                cg.EmitString(name.GetString());

                cg.EmitDelegate(impl, targetType, sigInfo.ContextSlot);
            }

            int first = sigInfo.HasContext ? 1 : 0;
            //  Emit string array (minus the first environment argument)
            cg.EmitInt(sigInfo.ParamNames.Length - first);
            cg.Emit(OpCodes.Newarr, typeof(string));
            for (int i = first; i < sigInfo.ParamNames.Length; i++) {
                cg.Emit(OpCodes.Dup);
                cg.EmitInt(i - first);
                cg.EmitStringOrNull(sigInfo.ParamNames[i].GetString());
                cg.Emit(OpCodes.Stelem_Ref);
            }
            cg.EmitObjectArray(defaults);

            if (flags == FunctionAttributes.None) {
                cg.Emit(OpCodes.Newobj, funcType.GetConstructor(
                    new Type[] { typeof(PythonModule), typeof(string), targetType, typeof(string[]), typeof(object[]) }));
            } else {
                cg.EmitInt((int)flags);
                cg.Emit(OpCodes.Newobj, funcType.GetConstructor(
                    new Type[] { typeof(PythonModule), typeof(string), targetType, typeof(string[]), typeof(object[]), typeof(FunctionAttributes) }));
            }

            string doc = Body.Documentation;
            if (doc != null) {
                cg.Emit(OpCodes.Dup);
                cg.EmitString(doc);
                cg.EmitCall(typeof(PythonFunction).GetProperty("Documentation").GetSetMethod());
            }

            // update func_code w/ appropriate state.
            cg.Emit(OpCodes.Dup);

            Slot functionCode = cg.GetLocalTmp(typeof(FunctionCode));

            cg.EmitCall(typeof(PythonFunction).GetProperty("FunctionCode").GetGetMethod());
            cg.Emit(OpCodes.Castclass, typeof(FunctionCode));
            cg.Emit(OpCodes.Dup);
            functionCode.EmitSet(cg);
            cg.EmitInt(this.Start.Line);
            cg.EmitCall(typeof(FunctionCode), "SetLineNumber");

            functionCode.EmitGet(cg);
            cg.EmitString(this.filename);
            cg.EmitCall(typeof(FunctionCode), "SetFilename");

            // Only codegen the call into SetFlags if there are flags to set.
            FunctionCode.FuncCodeFlags codeFlags = 0;
            if (cg.Context.TrueDivision) codeFlags |= FunctionCode.FuncCodeFlags.FutureDivision;
            if (this.yieldCount > 0)  codeFlags |= FunctionCode.FuncCodeFlags.Generator;
            if (codeFlags != 0) {
                functionCode.EmitGet(cg);
                cg.EmitInt((int)codeFlags);
                cg.EmitCall(typeof(FunctionCode), "SetFlags");
            }

            cg.FreeLocalTmp(functionCode);

            cg.EmitSet(name);

            if (decorators != null) {
                decorators.Emit(cg);
                cg.EmitSet(name);
            }
        }

        private Slot GetContextSlot(CodeGen cg) {
            if (IsClosure) return cg.EnvironmentSlot;

            return cg.ContextSlot;
        }

        internal SignatureInfo GetSignature(CodeGen cg) {
            int first = 0;
            Type[] paramTypes;
            SymbolId[] paramNames;
            Slot contextSlot = GetContextSlot(cg);

            if (contextSlot != null) {
                first = 1;          // Skip the first argument 
                paramTypes = new Type[parameters.Length + 1];
                paramNames = new SymbolId[parameters.Length + 1];

                paramTypes[0] = contextSlot.Type;
                paramNames[0] = SymbolTable.EnvironmentParmName;

                for (int i = 1; i < paramTypes.Length; i++) {
                    paramTypes[i] = typeof(object);
                    paramNames[i] = makeName(parameters[i - 1]);
                }
            } else {
                paramTypes = CompilerHelpers.MakeRepeatedArray(typeof(object), parameters.Length);
                paramNames = makeNames(parameters);
            }

            return new SignatureInfo(paramTypes, paramNames, first > 0, contextSlot);
        }

        /// <summary>
        /// Emits the raw function implementation into a method body.  Requires both a
        /// code gen for the method and for any initialization that might be required
        /// before the method is run (a module init or type cctor).  True if the method
        /// requires context, false if it doesn't.
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="icg"></param>
        /// <param name="context"></param>
        internal void EmitFunctionImplementation(CodeGen methodCodeGen, CodeGen initCodeGen) {
            if (EmitLocalDictionary) {
                PromoteLocalsToEnvironment();
            }

            methodCodeGen.EmitTraceBackTryBlockStart();

            // emit the actual body
            if (yieldCount > 0) {
                EmitGeneratorBody(methodCodeGen, initCodeGen);
            } else {
                EmitFunctionBody(methodCodeGen, initCodeGen);
            }

            methodCodeGen.EmitTraceBackFaultBlock(name.GetString(), filename);

            methodCodeGen.Finish();
        }

        private bool NeedsWrapperMethod() {
            return parameters.Length > Ops.MaximumCallArgs || flags != FunctionAttributes.None;
        }

        private CodeGen MakeWrapperMethodN(CodeGen cg, MethodInfo impl, bool context) {
            CodeGen icg;
            int index = 0;
            if (context) {
                Type environmentType = IsClosure ? cg.EnvironmentSlot.Type : cg.ContextSlot.Type;
                icg = cg.DefineUserHiddenMethod(impl.Name, typeof(object), new Type[] { environmentType, typeof(object[]) });
                Slot env = icg.GetArgumentSlot(index++);
                env.EmitGet(icg);
            } else {
                icg = cg.DefineUserHiddenMethod(impl.Name, typeof(object), new Type[] { typeof(object[]) });
            }

            Slot arg = icg.GetArgumentSlot(index);

            for (int pi = 0; pi < parameters.Length; pi++) {
                arg.EmitGet(icg);
                icg.EmitInt(pi);
                icg.Emit(OpCodes.Ldelem_Ref);
            }
            icg.EmitCall(impl);
            icg.Emit(OpCodes.Ret);
            return icg;
        }

        private void EmitTupleParams(CodeGen cg) {
            for (int i = 0; i < parameters.Length; i++) {
                Expression p = parameters[i];
                if (p is NameExpression) continue;

                cg.Names[EncodeTupleParamName(p as TupleExpression)].EmitGet(cg);

                p.EmitSet(cg);
            }
        }

        private void EmitFunctionBody(CodeGen cg, CodeGen ocg) {
            if (HasEnvironment) {
                cg.ContextSlot = cg.EnvironmentSlot = CreateEnvironment(cg);
            }
            if (cg.ContextSlot == null && IsClosure) {
                cg.ContextSlot = cg.StaticLinkSlot;
            }

            // Populate the environment with slots
            CreateGlobalSlots(cg, ocg);
            CreateClosureSlots(cg);
            CreateLocalSlots(cg);
            EmitTupleParams(cg);
            Body.Emit(cg);
            cg.EmitReturn(null);
        }

        private void EmitGeneratorBody(CodeGen cg, CodeGen ocg) {
            // Create the GenerateNext function
            CodeGen ncg = cg.DefineMethod(name.GetString() + "$g" + counter++, typeof(bool),
                new Type[] { typeof(Generator), typeof(object).MakeByRefType() },
                new String[] { "$gen", "$ret" });
            ncg.Context = cg.Context;

            PromoteLocalsToEnvironment();

            // Namespace without er factory - all locals must exist ahead of time
            ncg.Names = new Namespace(null);
            Slot generator = ncg.GetArgumentSlot(0);
            ncg.StaticLinkSlot = new FieldSlot(generator, typeof(Generator).GetField("staticLink"));
            if (HasEnvironment) {
                cg.EnvironmentSlot = CreateEnvironment(cg);
                EnvironmentFactory ef = this.environmentFactory;
                Slot envSlotCast = new CastSlot(
                    new FieldSlot(generator, typeof(Generator).GetField("environment")),
                    ef.EnvironmentType
                    );
                Slot envSlot = ncg.GetLocalTmp(ef.EnvironmentType);
                // setup the environment and static link slots
                ncg.EnvironmentSlot = envSlot;
                ncg.ContextSlot = envSlot;
                // pull the environment into typed local variable
                envSlot.EmitSet(ncg, envSlotCast);
                InheritEnvironment(ncg);
                CreateGeneratorTemps(ef, ncg);
            } else {
                ncg.ContextSlot = ncg.StaticLinkSlot;
            }
            ncg.ModuleSlot = new PropertySlot(ncg.ContextSlot, typeof(ICallerContext).GetProperty("Module"));

            CreateClosureSlots(ncg);
            CreateGlobalSlots(ncg, ocg);

            // Emit the generator body using the typed er
            EmitGenerator(ncg);

            // Initialize the generator
            EmitTupleParams(cg);

            // Create instance of the generator
            cg.EmitStaticLinkOrNull();
            cg.EmitEnvironmentOrNull();
            cg.EmitDelegate(ncg, typeof(Generator.NextTarget), null);
            cg.EmitNew(typeof(Generator), new Type[] { typeof(FunctionEnvironmentDictionary), typeof(FunctionEnvironmentDictionary), typeof(Generator.NextTarget) });
            cg.EmitReturn();
        }

        private void CreateGeneratorTemps(EnvironmentFactory ef, CodeGen cg) {
            for (int i = 0; i < TempsCount; i++) {
                cg.Names.AddTempSlot(ef.MakeEnvironmentReference(SymbolTable.StringToId("temp$" + i)).CreateSlot(cg.EnvironmentSlot));
            }
        }

        private void InheritEnvironment(CodeGen cg) {
            if (environment == null) return;
            foreach (KeyValuePair<SymbolId, EnvironmentReference> kv in environment) {
                Slot slot = kv.Value.CreateSlot(cg.EnvironmentSlot);
                cg.Names[kv.Key] = slot;
            }
        }

        private void EmitGenerator(CodeGen ncg) {
            YieldTarget[] targets = YieldLabelBuilder.BuildYieldTargets(this, ncg);

            Label[] jumpTable = new Label[yieldCount];
            for (int i = 0; i < yieldCount; i++) jumpTable[i] = targets[i].TopBranchTarget;
            ncg.yieldLabels = jumpTable;

            ncg.PushTryBlock();
            ncg.BeginExceptionBlock();

            ncg.Emit(OpCodes.Ldarg_0);
            ncg.EmitFieldGet(typeof(Generator), "location");
            ncg.Emit(OpCodes.Switch, jumpTable);

            // fall-through on first pass
            // yield statements will insert the needed labels after their returns
            Body.Emit(ncg);

            // fall-through is almost always possible in generators, so this
            // is almost always needed
            ncg.EmitReturnInGenerator(null);

            // special handling for StopIteration thrown in body
            ncg.BeginCatchBlock(typeof(StopIterationException));
            ncg.EndExceptionBlock();
            ncg.EmitReturnInGenerator(null);
            ncg.PopTargets();

            ncg.Finish();
        }

        public override bool TryGetBinding(SymbolId name, out Binding binding) {
            if (Names.TryGetValue(name, out binding)) {
                return binding.IsBound;
            } else return false;
        }

        private static SymbolId EncodeTupleParamName(TupleExpression param) {
            // we encode a tuple parameter so we can extract the compound
            // members back out of it's name.
            StringBuilder sb = new StringBuilder(tupleArgHeader);
            AppendTupleParamNames(sb, param);

            return SymbolTable.StringToId(sb.ToString());
        }

        private static void AppendTupleParamNames(StringBuilder sb, TupleExpression param) {
            for (int i = 0; i < param.Items.Count; i++) {
                NameExpression ne = param.Items[i] as NameExpression;
                if (ne != null) {
                    sb.Append('!');
                    sb.Append(ne.Name.GetString());
                } else {
                    // nested tuple
                    AppendTupleParamNames(sb, param.Items[i] as TupleExpression);
                }
            }
        }

        /// <summary>
        /// Returns a tuple of argument names.  Nested tuple args are
        /// represented as Tuple's nested in the array.
        /// </summary>
        internal static Tuple DecodeTupleParamName(string name) {
            // encoding is: tupleArg#!argName[!argName ...]
            // nestings can occur in which case we get:
            // tupleArg#!argName!tupleArg#!(encoding arg names)#

            Debug.Assert(String.Compare(name, 0, tupleArgHeader, 0, tupleArgHeader.Length) == 0);

            int curIndex = name.IndexOf('!');
            List<string> names = new List<string>();
            while (curIndex != -1) {
                Debug.Assert(curIndex != (name.Length - 1));

                int nextindex = name.IndexOf('!', curIndex + 1);
                if (nextindex == -1) {
                    names.Add(name.Substring(curIndex + 1));
                    break;
                }
                names.Add(name.Substring(curIndex + 1, nextindex - (curIndex + 1)));

                curIndex = nextindex;
            }
            return new Tuple(names);
        }

        private static SymbolId makeName(Expression param) {
            NameExpression ne = param as NameExpression;
            if (ne == null) {
                return EncodeTupleParamName((TupleExpression)param);
            } else {
                return ne.Name;
            }
        }

        private static SymbolId[] makeNames(Expression[] parameters) {
            SymbolId[] ret = new SymbolId[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                ret[i] = makeName(parameters[i]);
            }
            return ret;
        }
    }

    class SignatureInfo {
        public SignatureInfo(Type[] paramTypes, SymbolId[] paramNames, bool hasContext, Slot contextSlot) {
            ParamTypes = paramTypes;
            ParamNames = paramNames;
            HasContext = hasContext;
            ContextSlot = contextSlot;
        }

        public readonly Type[] ParamTypes;
        public readonly SymbolId[] ParamNames;
        public readonly bool HasContext;
        public readonly Slot ContextSlot;
    }

    public struct YieldTarget {
        private Label topBranchTarget;
        private Label tryBranchTarget;
        private bool finallyBranch;

        public YieldTarget(Label topBranchTarget) {
            this.topBranchTarget = topBranchTarget;
            tryBranchTarget = new Label();
            finallyBranch = false;
        }

        public Label TopBranchTarget {
            get { return topBranchTarget; }
            set { topBranchTarget = value; }
        }
        
        public Label TryBranchTarget {
            get { return tryBranchTarget; }
            set { tryBranchTarget = value; }
        }
        
        public bool FinallyBranch {
            get { return finallyBranch; }
            set { finallyBranch = value; }
        }

        internal YieldTarget FixForTry(CodeGen cg) {
            tryBranchTarget = cg.DefineLabel();
            return this;
        }

        internal YieldTarget FixForFinally(CodeGen cg) {
            tryBranchTarget = cg.DefineLabel();
            finallyBranch = true;
            return this;
        }
    }

    class YieldLabelBuilder : AstWalker {
        public abstract class ExceptionBlock {
            public enum State {
                Try,
                Handler,
                Finally
            };
            public State state;

            protected ExceptionBlock(State state) {
                this.state = state;
            }

            public abstract void AddYieldTarget(YieldStatement ys, YieldTarget yt, CodeGen cg);
        }

        public sealed class TryBlock : ExceptionBlock {
            private TryStatement stmt;

            public TryBlock(TryStatement stmt)
                : this(stmt, State.Try) {
            }
            public TryBlock(TryStatement stmt, State state)
                : base(state) {
                this.stmt = stmt;
            }

            public override void AddYieldTarget(YieldStatement ys, YieldTarget yt, CodeGen cg) {
                switch (state) {
                    case State.Try:
                        stmt.AddYieldTarget(yt.FixForTry(cg));
                        ys.Label = yt.TryBranchTarget;
                        break;
                    case State.Handler:
                        stmt.YieldInExcept = true;
                        ys.Label = yt.TopBranchTarget;
                        break;
                }
            }
        }

        public sealed class TryFinallyBlock : ExceptionBlock {
            private TryFinallyStatement stmt;

            public TryFinallyBlock(TryFinallyStatement stmt)
                : this(stmt, State.Try) {
            }
            public TryFinallyBlock(TryFinallyStatement stmt, State state)
                : base(state) {
                this.stmt = stmt;
            }

            public override void AddYieldTarget(YieldStatement ys, YieldTarget yt, CodeGen cg) {
                switch (state) {
                    case State.Try:
                        cg.Context.AddError("cannot yield from try block with finally", ys);
                        break;
                    case State.Finally:
                        stmt.AddYieldTarget(yt.FixForFinally(cg));
                        ys.Label = yt.TryBranchTarget;
                        break;
                }
            }
        }

        Stack<ExceptionBlock> tryBlocks = new Stack<ExceptionBlock>();
        YieldTarget[] topYields;
        FunctionDefinition func;
        CodeGen cg;

        private YieldLabelBuilder(FunctionDefinition func, CodeGen cg) {
            this.func = func;
            this.cg = cg;
            this.topYields = new YieldTarget[func.YieldCount];
        }

        public static YieldTarget[] BuildYieldTargets(FunctionDefinition func, CodeGen cg) {
            YieldLabelBuilder b = new YieldLabelBuilder(func, cg);
            func.Walk(b);
            return b.topYields;
        }

        #region AstWalker method overloads

        public override bool Walk(FunctionDefinition node) {
            // Do not recurse into nested functions
            return node == func;
        }

        public override bool Walk(TryFinallyStatement node) {
            TryFinallyBlock tfb = new TryFinallyBlock(node);
            tryBlocks.Push(tfb);
            node.Body.Walk(this);
            tfb.state = ExceptionBlock.State.Finally;
            node.FinallyStmt.Walk(this);
            ExceptionBlock eb = tryBlocks.Pop();
            Debug.Assert((object)eb == (object)tfb);
            return false;
        }


        public override bool Walk(TryStatement node) {
            TryBlock tb = new TryBlock(node);
            tryBlocks.Push(tb);
            node.Body.Walk(this);

            tb.state = TryBlock.State.Handler;
            foreach (TryStatementHandler handler in node.Handlers) {
                handler.Walk(this);
            }

            ExceptionBlock eb = tryBlocks.Pop();
            Debug.Assert((object)tb == (object)eb);

            if (node.ElseStatement != null) {
                node.ElseStatement.Walk(this);
            }
            return false;
        }

        public override void PostWalk(YieldStatement node) {
            topYields[node.Index] = new YieldTarget(cg.DefineLabel());

            if (tryBlocks.Count == 0) {
                node.Label = topYields[node.Index].TopBranchTarget;
            } else if (tryBlocks.Count == 1) {
                ExceptionBlock eb = tryBlocks.Peek();
                eb.AddYieldTarget(node, topYields[node.Index], cg);
            } else {
                cg.Context.AddError("yield in more than one try block", node);
            }
        }

        #endregion
    }

}
