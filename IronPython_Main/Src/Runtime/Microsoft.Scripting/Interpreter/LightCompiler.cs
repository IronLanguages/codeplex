/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System; using Microsoft;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Interpreter {

    public sealed class ExceptionHandler {
        public readonly Type ExceptionType;
        public readonly int StartIndex;
        public readonly int EndIndex;
        public readonly int StartHandlerIndex;
        public readonly int EndHandlerIndex;
        public readonly int HandlerStackDepth;
        public readonly bool PushException;

        public bool IsFinallyOrFault { get { return ExceptionType == null; } }

        public ExceptionHandler(int start, int end, int handlerStackDepth, int handlerStart, int handlerEnd)
            : this(start, end, handlerStackDepth, handlerStart, handlerEnd, null, false) {
        }

        public ExceptionHandler(int start, int end, int handlerStackDepth, int handlerStart, int handlerEnd, Type exceptionType, bool pushException) {
            StartIndex = start;
            EndIndex = end;
            StartHandlerIndex = handlerStart;
            HandlerStackDepth = handlerStackDepth;
            ExceptionType = exceptionType;
            PushException = pushException;
            EndHandlerIndex = handlerEnd;
        }

        public bool Matches(Type exceptionType, int index) {
            if (index >= StartIndex && index < EndIndex) {
                if (ExceptionType == null || ExceptionType.IsAssignableFrom(exceptionType)) {
                    return true;
                }
            }
            return false;
        }

        public bool IsBetterThan(ExceptionHandler other) {
            if (other == null) return true;

            if (StartIndex == other.StartIndex && EndIndex == other.EndIndex) {
                return StartHandlerIndex < other.StartHandlerIndex;
            }

            if (StartIndex > other.StartIndex) {
                Debug.Assert(EndIndex <= other.EndIndex);
                return true;
            } else if (EndIndex < other.EndIndex) {
                Debug.Assert(StartIndex == other.StartIndex);
                return true;
            } else {
                return false;
            }
        }

        internal bool IsInside(int index) {
            return index >= StartIndex && index < EndIndex;
        }

        public override string ToString() {
            return String.Format("{0} [{1}-{2}] [{3}-{4}]",
                (IsFinallyOrFault ? "finally/fault" : "catch(" + ExceptionType.Name + ")"),
                StartIndex, EndIndex, 
                StartHandlerIndex, EndHandlerIndex
            );
        }
    }

    public class DebugInfo {
        public int StartLine, EndLine;
        public int Index;
        public string FileName;
        public bool IsClear;
        private static readonly DebugInfoComparer _debugComparer = new DebugInfoComparer();

        private class DebugInfoComparer : IComparer<DebugInfo> {
            //We allow comparison between int and DebugInfo here
            int IComparer<DebugInfo>.Compare(DebugInfo d1, DebugInfo d2) {
                if (d1.Index > d2.Index) return 1;
                else if (d1.Index == d2.Index) return 0;
                else return -1;
            }
        }
        
        public static DebugInfo GetMatchingDebugInfo(DebugInfo[] debugInfos, int index) {
            //Create a faked DebugInfo to do the search
            DebugInfo d = new DebugInfo { Index = index };

            //to find the closest debug info before the current index

            int i = Array.BinarySearch<DebugInfo>(debugInfos, d, _debugComparer);
            if (i < 0) {
                //~i is the index for the first bigger element
                //if there is no bigger element, ~i is the length of the array
                i = ~i;
                if (i == 0) {
                    return null;
                }
                //return the last one that is smaller
                i = i - 1;
            }

            return debugInfos[i];
        }
    }

#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(Instructions.DebugView))]
#endif
    public class Instructions : List<Instruction> {
        #region Debug View
#if !SILVERLIGHT
        internal sealed class DebugView {
            private readonly Instructions _instructions;

            public DebugView(Instructions instructions) {
                Assert.NotNull(instructions);
                _instructions = instructions;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public InstructionView[]/*!*/ A0 {
                get {
                    var result = new List<InstructionView>();
                    int index = 0;
                    int stackDepth = 0;
                    foreach (var instruction in _instructions) {
                        result.Add(new InstructionView(index, stackDepth, instruction));
                        index++;
                        stackDepth += instruction.ProducedStack - instruction.ConsumedStack;
                    }
                    return result.ToArray();
                }
            }

            [DebuggerDisplay("{GetValue(),nq}", Name = "{GetName(),nq}")]
            internal struct InstructionView {
                private readonly int _index;
                private readonly int _stackDepth;
                private readonly Instruction _instruction;

                internal string GetName() {
                    return _index.ToString() + (_stackDepth == 0 ? "" : " D(" + _stackDepth.ToString() + ")");
                }

                internal string GetValue() {
                    return _instruction.ToString() + " " + (_instruction.ProducedStack - _instruction.ConsumedStack).ToString();

                }

                public InstructionView(int index, int stackDepth, Instruction instruction) {
                    _index = index;
                    _stackDepth = stackDepth;
                    _instruction = instruction;
                }
            }
        }
#endif
        #endregion
    }
    
    public class LightCompiler {
        private static readonly MethodInfo _RunMethod = typeof(Interpreter).GetMethod("Run");
        private static readonly MethodInfo _GetCurrentMethod = typeof(MethodBase).GetMethod("GetCurrentMethod");

#if DEBUG
        static LightCompiler() {
            Debug.Assert(_GetCurrentMethod != null && _RunMethod != null);
        }
#endif

        private Instructions _instructions = new Instructions();
        private int _maxStackDepth = 0;
        private int _currentStackDepth = 0;

        private List<ParameterExpression> _locals = new List<ParameterExpression>();
        private List<bool> _localIsBoxed = new List<bool>();
        private List<ParameterExpression> _closureVariables = new List<ParameterExpression>();

        private List<ExceptionHandler> _handlers = new List<ExceptionHandler>();
        
        // Goto instructions that need to be backpatched by the current try expression to handle jumps from it.
        // Each try expression with a finally clause sets this up and each goto instruction adds itself into the list if it is not null.
        private List<GotoInstruction> _currentTryFinallyGotoFixups;
        
        private List<DebugInfo> _debugInfos = new List<DebugInfo>();
        private List<UpdateStackTraceInstruction> _stackTraceUpdates = new List<UpdateStackTraceInstruction>();

        private Dictionary<LabelTarget, Label> _labels = new Dictionary<LabelTarget, Label>();

        private Stack<ParameterExpression> _exceptionForRethrowStack = new Stack<ParameterExpression>();
        
        private LightCompiler _parent;

        internal LightCompiler() {}

        private LightCompiler(LightCompiler parent) : this() {
            this._parent = parent;
        }

        internal Interpreter CompileTop(LambdaExpression node) {
            foreach (var p in node.Parameters) {
                this.AddVariable(p);
            }
            
            this.Compile(node.Body);
            Debug.Assert(_currentStackDepth == (node.ReturnType != typeof(void) ? 1 : 0));
            return this.MakeInterpreter(node);
        }

        private Interpreter MakeInterpreter(LambdaExpression lambda) {
            var handlers = _handlers.ToArray();
            var debugInfos = _debugInfos.ToArray();
            foreach (var stackTraceUpdate in _stackTraceUpdates) {
                stackTraceUpdate._debugInfos = debugInfos;
            }
            return new Interpreter(lambda, _localIsBoxed.ToArray(), _maxStackDepth, _instructions.ToArray(), handlers, debugInfos);
        }

        private sealed class Label {
            internal const int UnknownIndex = Int32.MinValue;
            internal const int UnknownSize = Int32.MinValue;

            private readonly LightCompiler _compiler;
            
            internal int _index = UnknownIndex;
            internal int _expectedStackSize = UnknownSize;
            private List<OffsetInstruction> _forwardBranchFixups;
            
            public Label(LightCompiler compiler) {
                _compiler = compiler;
            }

            public void Mark() {
                Debug.Assert(_index == UnknownIndex && _expectedStackSize == UnknownSize);

                _expectedStackSize = _compiler._currentStackDepth;
                _index = _compiler._instructions.Count;

                if (_forwardBranchFixups != null) {
                    foreach (var branch in _forwardBranchFixups) {
                        FixupBranch(branch);
                    }
                    _forwardBranchFixups = null;
                }
            }

            public void AddBranch(OffsetInstruction instruction) {
                Debug.Assert((_index == UnknownIndex) == (_expectedStackSize == UnknownSize));

                if (_index == UnknownIndex) {
                    if (_forwardBranchFixups == null) {
                        _forwardBranchFixups = new List<OffsetInstruction>();
                    }
                    _forwardBranchFixups.Add(instruction);
                } else {
                    int index = _compiler._instructions.IndexOf(instruction);
                    int offset = _index - index;
                    instruction.Fixup(offset, _expectedStackSize);
                }
            }

            public void FixupBranch(OffsetInstruction instruction) {
                Debug.Assert(_index != UnknownIndex);
                int index = _compiler._instructions.IndexOf(instruction);
                int offset = _index - index;
                instruction.Fixup(offset, _expectedStackSize);
            }
        }

        private Label MakeLabel() {
            return new Label(this);
        }

        private Label ReferenceLabel(LabelTarget target) {
            Label ret;
            if (!_labels.TryGetValue(target, out ret)) {
                ret = MakeLabel();
                _labels[target] = ret;
            }
            return ret;
        }

        private void AddBranch(OffsetInstruction instruction, Label label) {
            AddInstruction(instruction);
            label.AddBranch(instruction);
        }

        private void AddBranch(Label label, bool hasResult, bool hasValue) {
            AddBranch(new BranchInstruction(hasResult, hasValue), label);
        }

        public void AddInstruction(Instruction instruction) {
            _instructions.Add(instruction);
            _currentStackDepth -= instruction.ConsumedStack;
            Debug.Assert(_currentStackDepth >= 0); // checks that there's enough room to pop
            _currentStackDepth += instruction.ProducedStack;
            if (_currentStackDepth > _maxStackDepth) {
                _maxStackDepth = _currentStackDepth;
            }
        }

        public void PushConstant(object value) {
            AddInstruction(new PushInstruction(value));
        }

        private void CompileConstantExpression(Expression expr) {
            var node = (ConstantExpression)expr;

            PushConstant(node.Value);
        }

        private void CompileDefaultExpression(Expression expr) {
            var node = (DefaultExpression)expr;
            if (node.Type != typeof(void)) {
                object value;
                if (node.Type.IsValueType) {
                    value = Activator.CreateInstance(node.Type);
                } else {
                    value = null;
                }
                PushConstant(value);
            }
        }

        private bool IsBoxed(int index) {
            return _localIsBoxed[index];
        }

        private void SwitchToBoxed(int index) {
            for (int i = 0; i < _instructions.Count; i++) {
                var instruction = _instructions[i] as IBoxableInstruction;

                if (instruction != null) {
                    var newInstruction = instruction.BoxIfIndexMatches(index);
                    if (newInstruction != null) {
                        _instructions[i] = newInstruction;
                    }
                }
            }
        }

        public int GetVariableIndex(ParameterExpression variable) {
            return _locals.IndexOf(variable);
        }

        private void EnsureAvailableForClosure(ParameterExpression expr) {
            int index = GetVariableIndex(expr);
            if (index != -1) {
                if (!_localIsBoxed[index]) {
                    _localIsBoxed[index] = true;
                    SwitchToBoxed(index);
                }
                return;
            }

            if (!_closureVariables.Contains(expr)) {
                Debug.Assert(_parent != null);

                _parent.EnsureAvailableForClosure(expr);
                _closureVariables.Add(expr);
            }
        }

        public Instruction GetVariable(ParameterExpression variable) {
            LocalAccessInstruction local;

            int index = GetVariableIndex(variable);
            if (index != -1) {
                if (_localIsBoxed[index]) {
                    local = new GetBoxedLocalInstruction(index);
                } else {
                    local = new GetLocalInstruction(index);
                }
            } else {
                EnsureAvailableForClosure(variable);

                index = _closureVariables.IndexOf(variable);
                Debug.Assert(index != -1);
                local = new GetClosureInstruction(index);
            }
            local.SetName(variable.Name);
            return local;
        }

        public Instruction GetBoxedVariable(ParameterExpression variable) {
            LocalAccessInstruction local;

            int index = GetVariableIndex(variable);
            if (index != -1) {
                Debug.Assert(_localIsBoxed[index]);
                local = new GetLocalInstruction(index);
            } else {

                EnsureAvailableForClosure(variable);

                index = _closureVariables.IndexOf(variable);
                Debug.Assert(index != -1);
                local = new GetBoxedClosureInstruction(index);
            }
            local.SetName(variable.Name);
            return local;
        }

        public void CompileSetVariable(ParameterExpression variable, bool isVoid) {
            LocalAccessInstruction local;

            int index = GetVariableIndex(variable);
            if (index != -1) {
                if (_localIsBoxed[index]) {
                    if (isVoid) {
                        local = new SetBoxedLocalVoidInstruction(index);
                    } else {
                        local = new SetBoxedLocalInstruction(index);
                    }
                } else {
                    if (isVoid) {
                        local = new SetLocalVoidInstruction(index);
                    } else {
                        local = new SetLocalInstruction(index);
                    }
                }
                AddInstruction(local);
            } else {
                EnsureAvailableForClosure(variable);

                index = _closureVariables.IndexOf(variable);
                Debug.Assert(index != -1);
                AddInstruction(local = new SetClosureInstruction(index));
                if (isVoid) {
                    AddInstruction(PopInstruction.Instance);
                }
            }

            local.SetName(variable.Name);
        }


        private int AddVariable(ParameterExpression expr) {
            int index = _locals.Count;
            _locals.Add(expr);
            _localIsBoxed.Add(false);
            return index;
        }

        private void CompileParameterExpression(Expression expr) {
            var node = (ParameterExpression)expr;
            AddInstruction(GetVariable(node));
        }


        private void CompileBlockExpression(Expression expr, bool asVoid) {
            var node = (BlockExpression)expr;

            // TODO: pop these off a stack when exiting
            // TODO: basic flow analysis so we don't have to initialize all
            // variables.
            foreach (var local in node.Variables) {
                AddInstruction(InitializeLocalInstruction.Create(AddVariable(local), local));
            }

            for (int i = 0; i < node.Expressions.Count - 1; i++) {
                CompileAsVoid(node.Expressions[i]);
            }

            var lastExpression = node.Expressions[node.Expressions.Count - 1];
            if (asVoid) {
                CompileAsVoid(lastExpression);
            } else {
                Compile(lastExpression, asVoid);
            }
        }

        private void CompileIndexAssignment(BinaryExpression node, bool asVoid) {
            var index = (IndexExpression)node.Left;

            if (index.Indexer != null) {
                throw new NotImplementedException();
            }

            if (index.Arguments.Count > 1) {
                throw new NotImplementedException();
            }

            if (!asVoid) {
                throw new NotImplementedException();
            }

            // TODO:
            //Compile(node.Right);
            //Compile(index.Object);

            //for (int i = 0; i < index.Arguments.Count - 1; i++) {
            //    Compile(index.Arguments[i]);
            //}

            CompileSetArrayItem(index.Object, index.Arguments[0], node.Right);            
        }

        private void CompileMemberAssignment(BinaryExpression node, bool asVoid) {
            var member = (MemberExpression)node.Left;

            PropertyInfo pi = member.Member as PropertyInfo;
            if (pi != null) {
                var method = pi.GetSetMethod();
                this.Compile(member.Expression);
                this.Compile(node.Right);

                int index = 0;
                if (!asVoid) {
                    index = AddVariable(Expression.Parameter(node.Right.Type, null));
                    AddInstruction(new SetLocalInstruction(index));
                    // TODO: free the variable when it goes out of scope
                }

                AddInstruction(new CallInstruction(method));

                if (!asVoid) {
                    AddInstruction(new GetLocalInstruction(index));
                }
                return;
            }

            FieldInfo fi = member.Member as FieldInfo;
            if (fi != null) {
                this.Compile(member.Expression);
                this.Compile(node.Right);

                int index = 0;
                if (!asVoid) {
                    index = AddVariable(Expression.Parameter(node.Right.Type, null));
                    AddInstruction(new SetLocalInstruction(index));
                    // TODO: free the variable when it goes out of scope
                }

                AddInstruction(new FieldAssignInstruction(fi));

                if (!asVoid) {
                    AddInstruction(new GetLocalInstruction(index));
                }
                return;
            }

            throw new NotImplementedException();
        }

        private void CompileVariableAssignment(BinaryExpression node, bool asVoid) {
            this.Compile(node.Right);

            var target = (ParameterExpression)node.Left;
            CompileSetVariable(target, asVoid);
        }

        private void CompileAssignBinaryExpression(Expression expr, bool asVoid) {
            var node = (BinaryExpression)expr;

            switch (node.Left.NodeType) {
                case ExpressionType.Index:
                    CompileIndexAssignment(node, asVoid); 
                    break;

                case ExpressionType.MemberAccess:
                    CompileMemberAssignment(node, asVoid); 
                    break;

                case ExpressionType.Parameter:
                case ExpressionType.Extension:
                    CompileVariableAssignment(node, asVoid); 
                    break;

                default:
                    throw new InvalidOperationException("Invalid lvalue for assignment: " + node.Left.NodeType);
            }
        }

        private void CompileBinaryExpression(Expression expr) {
            var node = (BinaryExpression)expr;

            if (node.Method != null) {
                Compile(node.Left);
                Compile(node.Right);
                AddInstruction(new CallInstruction(node.Method));
            } else {
                switch (node.NodeType) {
                    case ExpressionType.ArrayIndex:
                        CompileArrayIndex(node.Left, node.Right);
                        return;

                    case ExpressionType.Equal:
                        CompileEqual(node.Left, node.Right);
                        return;

                    case ExpressionType.Add:
                        CompileAdd(node.Left, node.Right);
                        return;

                    case ExpressionType.NotEqual:
                        CompileNotEqual(node.Left, node.Right);
                        return;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void CompileEqual(Expression left, Expression right) {
            Debug.Assert(left.Type == right.Type || !left.Type.IsValueType && !right.Type.IsValueType);
            Compile(left);
            Compile(right);
            AddInstruction(EqualInstruction.Create(left.Type));
        }

        private void CompileNotEqual(Expression left, Expression right) {
            Debug.Assert(left.Type == right.Type || !left.Type.IsValueType && !right.Type.IsValueType);
            Compile(left);
            Compile(right);
            AddInstruction(NotEqualInstruction.Instance(left.Type));
        }

        private void CompileAdd(Expression left, Expression right) {
            if (left.Type == typeof(int) && right.Type == typeof(int)) {
                Compile(left);
                Compile(right);
                AddInstruction(AddIntInstruction.Instance);
                return;
            }
            throw new NotImplementedException();
        }

        private void CompileArrayIndex(Expression array, Expression index) {
            Type elemType = array.Type.GetElementType();
            if ((elemType.IsClass || elemType.IsInterface) && index.Type == typeof(int)) {
                Compile(array);
                Compile(index);
                AddInstruction(GetArrayItemInstruction<object>.Instance);
            } else {
                throw new NotImplementedException();
            }
        }

        private void CompileSetArrayItem(Expression array, Expression index, Expression value) {
            Type elemType = array.Type.GetElementType();
            if ((elemType.IsClass || elemType.IsInterface) && index.Type == typeof(int)) {
                Compile(value);
                Compile(array);
                Compile(index);
                AddInstruction(SetArrayItemInstruction<object>.Instance);
            } else {
                throw new NotImplementedException();
            }
        }


        private void CompileIndexExpression(Expression expr) {
            var node = (IndexExpression)expr;

            if (node.Object.Type.IsArray && node.Arguments.Count == 1) {
                CompileArrayIndex(node.Object, node.Arguments[0]);
                return;
            }

            throw new System.NotImplementedException();
        }


        private void CompileConvertUnaryExpression(Expression expr) {
            var node = (UnaryExpression)expr;

            // TODO: check the logic on this, but we think we can ignore conversions in this boxed world
            Compile(node.Operand);

            if (node.Method != null) {
                // We should be able to ignore Int32ToObject
                if (node.Method != Runtime.ScriptingRuntimeHelpers.Int32ToObjectMethod) {
                    AddInstruction(new CallInstruction(node.Method));
                }
            }
        }

        private void CompileNotExpression(UnaryExpression node) {
            if (node.Operand.Type == typeof(bool)) {
                this.Compile(node.Operand);
                AddInstruction(NotInstruction.Instance);
            } else {
                throw new NotImplementedException();
            }
        }

        private void CompileUnaryExpression(Expression expr) {
            var node = (UnaryExpression)expr;
            
            if (node.Method != null) {
                this.Compile(node.Operand);
                AddInstruction(new CallInstruction(node.Method));
            } else {
                switch (node.NodeType) {
                    case ExpressionType.Not:
                        CompileNotExpression(node);
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }
        }


        private void CompileAndAlsoBinaryExpression(Expression expr) {
            CompileLogicalBindaryExpression(expr, true);
        }

        private void CompileOrElseBinaryExpression(Expression expr) {
            CompileLogicalBindaryExpression(expr, false);
        }

        private void CompileLogicalBindaryExpression(Expression expr, bool andAlso) {
            var node = (BinaryExpression)expr;
            if (node.Method != null) {
                throw new NotImplementedException();
            }

            Debug.Assert(node.Left.Type == node.Right.Type);

            if (node.Left.Type == typeof(bool)) {
                var elseLabel = MakeLabel();
                var endLabel = MakeLabel();
                Compile(node.Left);
                AddBranch(andAlso ? (OffsetInstruction)new BranchFalseInstruction() : new BranchTrueInstruction(), elseLabel);
                Compile(node.Right);
                AddBranch(endLabel, false, true);
                elseLabel.Mark();
                PushConstant(!andAlso);
                endLabel.Mark();
                return;
            }

            Debug.Assert(node.Left.Type == typeof(bool?));
            throw new NotImplementedException();
        }

        private void CompileConditionalExpression(Expression expr, bool asVoid) {
            var node = (ConditionalExpression)expr;
            this.Compile(node.Test);

            if (node.IfTrue == AstUtils.Empty()) {
                var endOfFalse = MakeLabel();
                AddBranch(new BranchTrueInstruction(), endOfFalse);
                this.Compile(node.IfFalse, asVoid);
                endOfFalse.Mark();
            } else {
                var endOfTrue = MakeLabel();
                AddBranch(new BranchFalseInstruction(), endOfTrue);
                this.Compile(node.IfTrue, asVoid);

                if (node.IfFalse != AstUtils.Empty()) {
                    var endOfFalse = MakeLabel();
                    AddBranch(new BranchInstruction(false, !asVoid), endOfFalse);
                    endOfTrue.Mark();
                    this.Compile(node.IfFalse, asVoid);
                    endOfFalse.Mark();
                } else {
                    endOfTrue.Mark();
                }
            }
        }

        private void CompileLoopExpression(Expression expr) {
            var node = (LoopExpression)expr;

            var continueLabel = node.ContinueLabel == null ? 
                MakeLabel() : ReferenceLabel(node.ContinueLabel);

            continueLabel.Mark();
            this.CompileAsVoid(node.Body);
            AddBranch(new BranchInstruction(), continueLabel);

            if (node.BreakLabel != null) {
                ReferenceLabel(node.BreakLabel).Mark();
            }
        }

        private void CompileSwitchExpression(Expression expr) {
            var node = (SwitchExpression)expr;

            // Currently only supports int test values, with no method
            if (node.SwitchValue.Type != typeof(int) || node.Comparison != null) {
                throw new NotImplementedException();
            }

            // Test values must be constant
            if (!node.Cases.All(c => c.TestValues.All(t => t is ConstantExpression))) {
                throw new NotImplementedException();
            }

            this.Compile(node.SwitchValue);
            int start = _instructions.Count;
            var switchInstruction = new SwitchInstruction();
            AddInstruction(switchInstruction);
            var end = MakeLabel();
            int switchStack = _currentStackDepth;
            for (int i = 0, n = node.Cases.Count; i < n; i++) {
                var clause = node.Cases[i];
                _currentStackDepth = switchStack;
                int offset = _instructions.Count - start;
                foreach (ConstantExpression testValue in clause.TestValues) {
                    switchInstruction.AddCase((int)testValue.Value, offset);
                }
                this.Compile(clause.Body);
                // Last case doesn't need branch
                if (node.DefaultBody != null || i < n - 1) {
                    AddBranch(new BranchInstruction(), end);
                }
                Debug.Assert(_currentStackDepth == switchStack);
            }
            switchInstruction.AddDefault(_instructions.Count - start);
            if (node.DefaultBody != null) {
                _currentStackDepth = switchStack;
                this.Compile(node.DefaultBody);
            }
            if (node.Type != typeof(void)) {
                Debug.Assert(_currentStackDepth == switchStack + 1);
                _currentStackDepth = switchStack + 1;
            } else {
                Debug.Assert(_currentStackDepth == switchStack);
                _currentStackDepth = switchStack;
            }
            end.Mark();
        }

        private void CompileLabelExpression(Expression expr) {
            var node = (LabelExpression)expr;

            if (node.DefaultValue != null) {
                this.Compile(node.DefaultValue);
            }

            ReferenceLabel(node.Target).Mark();
        }

        private void CompileGotoExpression(Expression expr) {
            var node = (GotoExpression)expr;

            if (node.Value != null) {
                this.Compile(node.Value);
            }

            var label = ReferenceLabel(node.Target);

            var gt = new GotoInstruction(_instructions.Count, node.Type != typeof(void), node.Value != null);
            AddBranch(gt, label);

            if (_currentTryFinallyGotoFixups != null) {
                _currentTryFinallyGotoFixups.Add(gt);
            }
        }

        private void CompileThrowUnaryExpression(Expression expr, bool asVoid) {
            var node = (UnaryExpression)expr;

            if (node.Operand == null) {
                AddInstruction(GetVariable(_exceptionForRethrowStack.Peek()));
                AddInstruction(asVoid ? ThrowInstruction.VoidRethrow : ThrowInstruction.Rethrow);
            } else {
                this.Compile(node.Operand);
                AddInstruction(asVoid ? ThrowInstruction.VoidThrow : ThrowInstruction.Throw);
            }
        }

        // TODO: remove (replace by true fault support)
        private bool EndsWithRethrow(Expression expr) {
            if (expr.NodeType == ExpressionType.Throw) {
                var node = (UnaryExpression)expr;
                return node.Operand == null;
            }

            BlockExpression block = expr as BlockExpression;
            if (block != null) {
                return EndsWithRethrow(block.Expressions[block.Expressions.Count - 1]);
            }
            return false;
        }


        // TODO: remove (replace by true fault support)
        private void CompileAsVoidRemoveRethrow(Expression expr) {
            int stackDepth = _currentStackDepth;

            if (expr.NodeType == ExpressionType.Throw) {
                Debug.Assert(((UnaryExpression)expr).Operand == null);
                return;
            }

            BlockExpression node = (BlockExpression)expr;
            foreach (var local in node.Variables) {
                AddVariable(local);
            }


            for (int i = 0; i < node.Expressions.Count - 1; i++) {
                CompileAsVoid(node.Expressions[i]);
            }

            CompileAsVoidRemoveRethrow(node.Expressions[node.Expressions.Count - 1]);

            Debug.Assert(stackDepth == _currentStackDepth);
        }

        private void CompileTryExpression(Expression expr) {
            var node = (TryExpression)expr;

            Label startOfFinally = MakeLabel();

            List<GotoInstruction> gotos = null;
            List<GotoInstruction> parentGotos = _currentTryFinallyGotoFixups;
            if (node.Finally != null) {
                _currentTryFinallyGotoFixups = gotos = new List<GotoInstruction>();
            }

            int tryStackDepth = _currentStackDepth;
            int tryStart = _instructions.Count;
            Compile(node.Body);
            int tryEnd = _instructions.Count;

            bool hasValue = node.Body.Type != typeof(void);

            // keep the result on the stack:
            AddBranch(startOfFinally, hasValue, hasValue);

            // TODO: emulates faults (replace by true fault support)
            if (node.Finally == null && node.Handlers.Count == 1) {
                var handler = node.Handlers[0];
                if (handler.Filter == null && handler.Test == typeof(Exception) && handler.Variable == null) {
                    if (EndsWithRethrow(handler.Body)) {
                        int handlerStart = _instructions.Count;
                        CompileAsVoidRemoveRethrow(handler.Body);
                        startOfFinally.Mark();
                        int handlerEnd = _instructions.Count;

                        _handlers.Add(new ExceptionHandler(tryStart, tryEnd, tryStackDepth, handlerStart, handlerEnd));
                        return;
                    }
                }
            }

            foreach (var handler in node.Handlers) {
                if (handler.Filter != null) throw new NotImplementedException();
                var parameter = handler.Variable;

                // TODO we should only create one of these if needed for a rethrow
                if (parameter == null) {
                    parameter = Expression.Parameter(handler.Test, "currentException");
                }
                // TODO: free the variable when it goes out of scope
                AddVariable(parameter);
                _exceptionForRethrowStack.Push(parameter);

                int handlerStart = _instructions.Count;
                // TODO: we can reuse _currentTryFinallyGotoFixups if allocated. If not we still need a different list.
                
                // add a stack balancing nop instruction (exception handling pushes the current exception):
                AddInstruction(hasValue ? EnterExceptionHandlerInstruction.NonVoid : EnterExceptionHandlerInstruction.Void);
                CompileSetVariable(parameter, true);
                Compile(handler.Body);

                int handlerEnd = _instructions.Count;

                //TODO pop this scoped variable that we no longer need
                //PopVariable(parameter);
                _exceptionForRethrowStack.Pop();

                // keep the value of the body on the stack:
                Debug.Assert(hasValue == (handler.Body.Type != typeof(void)));
                AddBranch(new LeaveExceptionHandlerInstruction(hasValue), startOfFinally);

                _handlers.Add(new ExceptionHandler(tryStart, tryEnd, tryStackDepth, handlerStart, handlerEnd, handler.Test, true));                
            }

            if (node.Fault != null) {
                throw new NotImplementedException();
            }

            startOfFinally.Mark();

            if (node.Finally != null) {
                _currentTryFinallyGotoFixups = parentGotos;
                int finallyStart = _instructions.Count;
                CompileAsVoid(node.Finally);
                int finallyEnd = _instructions.Count;

                // registeres this finally block for execution to all goto instructions that jump out:
                foreach (var gt in gotos) {
                    if (gt.AddFinally(tryStart, tryStackDepth, finallyStart, finallyEnd)) {
                        if (parentGotos != null) {
                            // we might need to execute parent finally as well:
                            parentGotos.Add(gt);
                        }
                    }
                }

                // finally handler spans over try body and all catch handlers:
                _handlers.Add(new ExceptionHandler(tryStart, finallyStart, tryStackDepth, finallyStart, finallyEnd));
            }
        }

        private void CompileDynamicExpression(Expression expr) {
            var node = (DynamicExpression)expr;

            foreach (var arg in node.Arguments) {
                this.Compile(arg);
            }

            AddInstruction(DynamicInstructions.MakeInstruction(node.DelegateType, node.Binder));
        }

        private void CompileMethodCallExpression(Expression expr) {
            var node = (MethodCallExpression)expr;
            
            if (node.Method == _GetCurrentMethod && node.Object == null && node.Arguments.Count == 0) {
                // If we call GetCurrentMethod, it will expose details of the
                // interpreter's CallInstruction. Instead, we use
                // Interpreter.Run, which logically represents the running
                // method, and will appear in the stack trace of an exception.
                AddInstruction(new PushInstruction(_RunMethod));
                return;
            }

            //TODO support pass by reference and lots of other fancy stuff

            if (!node.Method.IsStatic) {
                this.Compile(node.Object);
            }

            foreach (var arg in node.Arguments) {
                this.Compile(arg);
            }

            AddInstruction(new CallInstruction(node.Method));
        }

        private void CompileNewExpression(Expression expr) {
            var node = (NewExpression)expr;

            foreach (var arg in node.Arguments) {
                this.Compile(arg);
            }
            AddInstruction(new NewInstruction(node.Constructor));

        }

        private void CompileMemberExpression(Expression expr) {
            var node = (MemberExpression)expr;

            var member = node.Member;
            FieldInfo fi = member as FieldInfo;
            if (fi != null) {
                if (fi.IsLiteral) {
                    PushConstant(fi.GetRawConstantValue());
                } else if (fi.IsStatic) {
                    if (fi.IsInitOnly) {
                        object value = fi.GetValue(null);
                        PushConstant(value);
                    } else {
                        AddInstruction(new StaticFieldAccessInstruction(fi));
                    }
                } else {
                    Compile(node.Expression);
                    AddInstruction(new FieldAccessInstruction(fi));
                }
                return;
            }

            PropertyInfo pi = member as PropertyInfo;
            if (pi != null) {
                var method = pi.GetGetMethod();
                if (node.Expression != null) {
                    this.Compile(node.Expression);
                }
                AddInstruction(new CallInstruction(method));
                return;
            }


            throw new System.NotImplementedException();
        }

        private void CompileNewArrayExpression(Expression expr) {
            var node = (NewArrayExpression)expr;

            foreach (var arg in node.Expressions) {
                this.Compile(arg);
            }

            Type elementType = node.Type.GetElementType();
            int count = node.Expressions.Count;

            if (node.NodeType == ExpressionType.NewArrayInit) {
                AddInstruction(new NewArrayInitInstruction(elementType, count));
            } else if (node.NodeType == ExpressionType.NewArrayBounds) {
                if (count == 1) {
                    AddInstruction(new NewArrayBoundsInstruction1(elementType));
                } else {
                    AddInstruction(new NewArrayBoundsInstructionN(elementType, count));
                }
            } else {
                throw new System.NotImplementedException();
            }
        }

        class ParameterVisitor : ExpressionVisitor {
            private readonly LightCompiler _compiler;

            public ParameterVisitor(LightCompiler compiler) {
                _compiler = compiler;
            }

            protected override Expression VisitParameter(ParameterExpression node) {
                _compiler.GetVariable(node);
                return node;
            }

            protected override Expression VisitLambda<T>(Expression<T> node) {
                return node;
            }
        }

        private void CompileExtensionExpression(Expression expr) {
            var instructionProvider = expr as IInstructionProvider;
            if (instructionProvider != null) {
                instructionProvider.AddInstructions(this);
                
                // we need to walk the reduced expression in case it has any closure 
                // variables that we'd need to track when we actually turn around and 
                // compile it
                if (expr.CanReduce) {
                    new ParameterVisitor(this).Visit(expr.Reduce());
                }
                return;
            }

            var skip = expr as Ast.SkipInterpretExpression;
            if (skip != null) {
                new ParameterVisitor(this).Visit(skip);
                return;
            }

            var node = expr as Microsoft.Scripting.Ast.SymbolConstantExpression;
            if (node != null) {
                PushConstant(node.Value);
                return;
            }

            var updateStack = expr as LastFaultingLineExpression;
            if (updateStack != null) {
                var updateStackInstr = new UpdateStackTraceInstruction();
                AddInstruction(updateStackInstr);
                _stackTraceUpdates.Add(updateStackInstr);
                return;
            }

            if (expr.CanReduce) {
                Compile(expr.Reduce());
            } else {
                throw new System.NotImplementedException();
            }
        }


        private void CompileDebugInfoExpression(Expression expr) {
            var node = (DebugInfoExpression)expr;
            int start = _instructions.Count;
            var info = new DebugInfo()
            {
                Index = start,
                FileName = node.Document.FileName,
                StartLine = node.StartLine,
                EndLine = node.EndLine,
                IsClear = node.IsClear
            };
            _debugInfos.Add(info);
        }

        private void CompileRuntimeVariablesExpression(Expression expr) {
            // Generates IRuntimeVariables for all requested variables
            var node = (RuntimeVariablesExpression)expr;
            foreach (var variable in node.Variables) {
                EnsureAvailableForClosure(variable);
                AddInstruction(GetBoxedVariable(variable));
            }

            AddInstruction(new RuntimeVariablesInstruction(node.Variables.Count));
        }


        private void CompileLambdaExpression(Expression expr) {
            var node = (LambdaExpression)expr;
            var compiler = new LightCompiler(this);
            var interpreter = compiler.CompileTop(node);

            foreach (ParameterExpression variable in compiler._closureVariables) {
                AddInstruction(GetBoxedVariable(variable));
            }
            AddInstruction(new CreateDelegateInstruction(new LightDelegateCreator(interpreter, node, compiler._closureVariables)));
        }

        private void CompileCoalesceBinaryExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        private void CompileInvocationExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        private void CompileListInitExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        private void CompileMemberInitExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        private void CompileQuoteUnaryExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        private void CompileUnboxUnaryExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        private void CompileTypeBinaryExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        private void CompileReducibleExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        internal void Compile(Expression expr, bool asVoid) {
            if (asVoid) {
                CompileAsVoid(expr);
            } else {
                Compile(expr);
            }
        }

        internal void CompileAsVoid(Expression expr) {
            int startingStackDepth = _currentStackDepth;
            switch (expr.NodeType) {
                case ExpressionType.Assign:
                    CompileAssignBinaryExpression(expr, true);
                    break;

                case ExpressionType.Block:
                    CompileBlockExpression(expr, true);
                    break;

                case ExpressionType.Throw:
                    CompileThrowUnaryExpression(expr, true);
                    break;

                case ExpressionType.Constant:
                case ExpressionType.Default:
                case ExpressionType.Parameter:
                    // no-op
                    break;

                default:
                    Compile(expr);
                    if (expr.Type != typeof(void)) {
                        AddInstruction(PopInstruction.Instance);
                    }
                    break;
            }
            Debug.Assert(_currentStackDepth == startingStackDepth);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Compile(Expression expr) {
            int startingStackDepth = _currentStackDepth;
            switch (expr.NodeType) {
                case ExpressionType.Add: CompileBinaryExpression(expr); break;
                case ExpressionType.AddChecked: CompileBinaryExpression(expr); break;
                case ExpressionType.And: CompileBinaryExpression(expr); break;
                case ExpressionType.AndAlso: CompileAndAlsoBinaryExpression(expr); break;
                case ExpressionType.ArrayLength: CompileUnaryExpression(expr); break;
                case ExpressionType.ArrayIndex: CompileBinaryExpression(expr); break;
                case ExpressionType.Call: CompileMethodCallExpression(expr); break;
                case ExpressionType.Coalesce: CompileCoalesceBinaryExpression(expr); break;
                case ExpressionType.Conditional: CompileConditionalExpression(expr, expr.Type == typeof(void)); break;
                case ExpressionType.Constant: CompileConstantExpression(expr); break;
                case ExpressionType.Convert: CompileConvertUnaryExpression(expr); break;
                case ExpressionType.ConvertChecked: CompileConvertUnaryExpression(expr); break;
                case ExpressionType.Divide: CompileBinaryExpression(expr); break;
                case ExpressionType.Equal: CompileBinaryExpression(expr); break;
                case ExpressionType.ExclusiveOr: CompileBinaryExpression(expr); break;
                case ExpressionType.GreaterThan: CompileBinaryExpression(expr); break;
                case ExpressionType.GreaterThanOrEqual: CompileBinaryExpression(expr); break;
                case ExpressionType.Invoke: CompileInvocationExpression(expr); break;
                case ExpressionType.Lambda: CompileLambdaExpression(expr); break;
                case ExpressionType.LeftShift: CompileBinaryExpression(expr); break;
                case ExpressionType.LessThan: CompileBinaryExpression(expr); break;
                case ExpressionType.LessThanOrEqual: CompileBinaryExpression(expr); break;
                case ExpressionType.ListInit: CompileListInitExpression(expr); break;
                case ExpressionType.MemberAccess: CompileMemberExpression(expr); break;
                case ExpressionType.MemberInit: CompileMemberInitExpression(expr); break;
                case ExpressionType.Modulo: CompileBinaryExpression(expr); break;
                case ExpressionType.Multiply: CompileBinaryExpression(expr); break;
                case ExpressionType.MultiplyChecked: CompileBinaryExpression(expr); break;
                case ExpressionType.Negate: CompileUnaryExpression(expr); break;
                case ExpressionType.UnaryPlus: CompileUnaryExpression(expr); break;
                case ExpressionType.NegateChecked: CompileUnaryExpression(expr); break;
                case ExpressionType.New: CompileNewExpression(expr); break;
                case ExpressionType.NewArrayInit: CompileNewArrayExpression(expr); break;
                case ExpressionType.NewArrayBounds: CompileNewArrayExpression(expr); break;
                case ExpressionType.Not: CompileUnaryExpression(expr); break;
                case ExpressionType.NotEqual: CompileBinaryExpression(expr); break;
                case ExpressionType.Or: CompileBinaryExpression(expr); break;
                case ExpressionType.OrElse: CompileOrElseBinaryExpression(expr); break;
                case ExpressionType.Parameter: CompileParameterExpression(expr); break;
                case ExpressionType.Power: CompileBinaryExpression(expr); break;
                case ExpressionType.Quote: CompileQuoteUnaryExpression(expr); break;
                case ExpressionType.RightShift: CompileBinaryExpression(expr); break;
                case ExpressionType.Subtract: CompileBinaryExpression(expr); break;
                case ExpressionType.SubtractChecked: CompileBinaryExpression(expr); break;
                case ExpressionType.TypeAs: CompileUnaryExpression(expr); break;
                case ExpressionType.TypeIs: CompileTypeBinaryExpression(expr); break;
                case ExpressionType.Assign: CompileAssignBinaryExpression(expr, expr.Type == typeof(void)); break;
                case ExpressionType.Block: CompileBlockExpression(expr, expr.Type == typeof(void)); break;
                case ExpressionType.DebugInfo: CompileDebugInfoExpression(expr); break;
                case ExpressionType.Decrement: CompileUnaryExpression(expr); break;
                case ExpressionType.Dynamic: CompileDynamicExpression(expr); break;
                case ExpressionType.Default: CompileDefaultExpression(expr); break;
                case ExpressionType.Extension: CompileExtensionExpression(expr); break;
                case ExpressionType.Goto: CompileGotoExpression(expr); break;
                case ExpressionType.Increment: CompileUnaryExpression(expr); break;
                case ExpressionType.Index: CompileIndexExpression(expr); break;
                case ExpressionType.Label: CompileLabelExpression(expr); break;
                case ExpressionType.RuntimeVariables: CompileRuntimeVariablesExpression(expr); break;
                case ExpressionType.Loop: CompileLoopExpression(expr); break;
                case ExpressionType.Switch: CompileSwitchExpression(expr); break;
                case ExpressionType.Throw: CompileThrowUnaryExpression(expr, expr.Type == typeof(void)); break;
                case ExpressionType.Try: CompileTryExpression(expr); break;
                case ExpressionType.Unbox: CompileUnboxUnaryExpression(expr); break;
                case ExpressionType.TypeEqual: CompileTypeBinaryExpression(expr); break;
                case ExpressionType.OnesComplement: CompileUnaryExpression(expr); break;
                case ExpressionType.IsTrue: CompileUnaryExpression(expr); break;
                case ExpressionType.IsFalse: CompileUnaryExpression(expr); break;
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                    CompileReducibleExpression(expr); break;
                default: throw Assert.Unreachable;
            };
            Debug.Assert(_currentStackDepth == startingStackDepth + (expr.Type == typeof(void) ? 0 : 1));
        }
    }
}
