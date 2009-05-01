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
using Microsoft.Linq.Expressions;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;

namespace IronPython.Compiler {
    interface IPythonVariableExpression  {
        Expression/*!*/ Assign(Expression/*!*/ value);
        Expression/*!*/ Delete();
    }

    interface IPythonGlobalExpression : IPythonVariableExpression {
        Expression/*!*/ RawValue();
    }
    /// <summary>
    /// Small reducable node which just fetches the value from a PythonGlobal
    /// object.  The compiler recognizes these on sets and turns them into
    /// assignments on the python global object.
    /// </summary>
    class PythonGlobalVariableExpression : Expression, IInstructionProvider, IPythonGlobalExpression {
        private readonly Expression/*!*/ _variable;
        private readonly PythonGlobal _global;

        public PythonGlobalVariableExpression(Expression/*!*/ globalExpr, PythonGlobal global) {
            Assert.NotNull(globalExpr);

            _variable = globalExpr;
            _global = global;
        }

        public Expression/*!*/ Target {
            get {
                return _variable;
            }
        }

        public PythonGlobal Global {
            get {
                return _global;
            }
        }

        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Extension;
        }

        protected override Type/*!*/ TypeImpl() {
            return typeof(object);
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        public override Expression/*!*/ Reduce() {
            return Expression.Property(
                _variable,
                PythonGlobal.CurrentValueProperty
            );
        }

        public Expression/*!*/ RawValue() {
            return Expression.Property(
                _variable,
                PythonGlobal.RawValueProperty
            );
        }

        public Expression/*!*/ Assign(Expression/*!*/ value) {
            return new PythonSetGlobalVariableExpression(this, value);
        }

        public Expression/*!*/ Delete() {
            return Expression.Assign(
                Expression.Property(
                    Target,
                    typeof(PythonGlobal).GetProperty("CurrentValue")
                ),
                Expression.Field(null, typeof(Uninitialized).GetField("Instance"))
            );
        }

        protected override Expression VisitChildren(Func<Expression, Expression> visitor) {
            Expression v = visitor(_variable);
            if (v == _variable) {
                return this;
            }
            return new PythonGlobalVariableExpression(v, _global);
        }

        #region IInstructionProvider Members

        public void AddInstructions(LightCompiler compiler) {
            compiler.AddInstruction(new PythonGlobalInstruction(_global));
        }

        #endregion
    }

    class PythonGlobalInstruction : Instruction {
        private readonly PythonGlobal _global;
        public PythonGlobalInstruction(PythonGlobal global) {
            _global = global;
        }

        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            frame.Push(_global.CurrentValue);
            return +1;
        }

        public override string ToString() {
            return "GetGlobal(" + _global + ")";
        }
    }

    class PythonSetGlobalVariableExpression : Expression, IInstructionProvider {
        private readonly PythonGlobalVariableExpression/*!*/ _global;
        private readonly Expression/*!*/ _value;

        public PythonSetGlobalVariableExpression(PythonGlobalVariableExpression/*!*/ global, Expression/*!*/ value) {
            _global = global;
            _value = value;
        }

        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Extension;
        }

        protected override Type/*!*/ TypeImpl() {
            return typeof(object);
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        public override Expression Reduce() {
            return Expression.Assign(
                Expression.Property(
                    _global.Target,
                    typeof(PythonGlobal).GetProperty("CurrentValue")
                ),
                Utils.Convert(_value, typeof(object))
            );
        }


        protected override Expression VisitChildren(Func<Expression, Expression> visitor) {
            var g = (PythonGlobalVariableExpression)visitor(_global);
            var v = visitor(_value);
            if (g == _global && v == _value) {
                return this;
            }
            return new PythonSetGlobalVariableExpression(g, v);
        }

        #region IInstructionProvider Members

        public void AddInstructions(LightCompiler compiler) {
            compiler.Compile(_value);
            compiler.AddInstruction(new PythonSetGlobalInstruction(_global.Global));
        }

        #endregion
    }

    class PythonSetGlobalInstruction : Instruction {
        private readonly PythonGlobal _global;
        public PythonSetGlobalInstruction(PythonGlobal global) {
            _global = global;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            _global.CurrentValue = frame.Peek();
            return +1;
        }

        public override string ToString() {
            return "SetGlobal(" + _global + ")";
        }
    }

    class LookupGlobalVariable : Expression, IInstructionProvider, IPythonGlobalExpression {
        private readonly string/*!*/ _name;
        private readonly bool/*!*/ _isLocal;
        private readonly Expression/*!*/ _scope;

        public LookupGlobalVariable(Expression/*!*/ scopeExpr, string/*!*/  name, bool isLocal) {
            Assert.NotNull(name);

            _name = name;
            _isLocal = isLocal;
            _scope = scopeExpr;
        }

        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Extension;
        }

        protected override Type/*!*/ TypeImpl() {
            return typeof(object);
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        public Expression/*!*/ RawValue() {
            return Expression.Call(
                typeof(PythonOps).GetMethod(_isLocal ? "RawGetLocal" : "RawGetGlobal"),
                _scope,
                Utils.Constant(SymbolTable.StringToId(_name))
            );
        }

        public override Expression/*!*/ Reduce() {
            return Expression.Call(
                typeof(PythonOps).GetMethod(_isLocal ? "GetLocal" : "GetGlobal"),
                _scope,
                Utils.Constant(SymbolTable.StringToId(_name))
            );
        }

        public Expression/*!*/ Assign(Expression/*!*/ value) {
            return Expression.Call(
                typeof(PythonOps).GetMethod(_isLocal ? "SetLocal" : "SetGlobal"),
                _scope,
                Utils.Constant(SymbolTable.StringToId(_name)),
                value
            );
        }

        public Expression/*!*/ Delete() {
            return Expression.Call(
                typeof(PythonOps).GetMethod(_isLocal ? "DeleteLocal" : "DeleteGlobal"),
                _scope,
                Utils.Constant(SymbolTable.StringToId(_name))
            );
        }

        #region IInstructionProvider Members

        void IInstructionProvider.AddInstructions(LightCompiler compiler) {
            compiler.Compile(_scope);
            compiler.AddInstruction(new LookupGlobalInstruction(_name, _isLocal));
        }

        #endregion
    }

    class LookupGlobalInstruction : Instruction {
        private readonly SymbolId _name;
        private readonly bool _isLocal;
        public LookupGlobalInstruction(string name, bool isLocal) {
            _name = SymbolTable.StringToId(name);
            _isLocal = isLocal;
        }
        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            if (_isLocal) {
                frame.Push(PythonOps.GetLocal((Scope)frame.Pop(), _name));
            } else {
                frame.Push(PythonOps.GetGlobal((Scope)frame.Pop(), _name));
            }
            return +1;
        }

        public override string ToString() {
            return "LookupGlobal(" + _name + ", isLocal=" + _isLocal + ")";
        }
    }
}
