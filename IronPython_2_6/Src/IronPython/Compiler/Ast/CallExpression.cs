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

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Interpreter;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {

    public class CallExpression : Expression, IInstructionProvider {
        private readonly Expression _target;
        private readonly Arg[] _args;

        public CallExpression(Expression target, Arg[] args) {
            _target = target;
            _args = args;
        }

        public Expression Target {
            get { return _target; }
        }

        public IList<Arg> Args {
            get { return _args; }
        } 

        public bool NeedsLocalsDictionary() {
            NameExpression nameExpr = _target as NameExpression;
            if (nameExpr == null) return false;

            if (_args.Length == 0) {
                if (nameExpr.Name == "locals") return true;
                if (nameExpr.Name == "vars") return true;
                if (nameExpr.Name == "dir") return true;
                return false;
            } else if (_args.Length == 1 && (nameExpr.Name == "dir" || nameExpr.Name == "vars")) {
                if (_args[0].Name == "*" || _args[0].Name == "**") {
                    // could be splatting empty list or dict resulting in 0-param call which needs context
                    return true;
                }
            } else if (_args.Length == 2 && (nameExpr.Name == "dir" || nameExpr.Name == "vars")) {
                if (_args[0].Name == "*" && _args[1].Name == "**") {
                    // could be splatting empty list and dict resulting in 0-param call which needs context
                    return true;
                }
            } else {
                if (nameExpr.Name == "eval") return true;
                if (nameExpr.Name == "execfile") return true;
            }
            return false;
        }

        public override MSAst.Expression Reduce() {
            return UnicodeCall() ?? NormalCall(_target);
        }

        private MSAst.Expression NormalCall(MSAst.Expression target) {
            MSAst.Expression[] values = new MSAst.Expression[_args.Length + 2];
            Argument[] kinds = new Argument[_args.Length];

            values[0] = Parent.LocalContext;
            values[1] = target;

            for (int i = 0; i < _args.Length; i++) {
                kinds[i] = _args[i].GetArgumentInfo();
                values[i + 2] = _args[i].Expression;
            }

            return Parent.Invoke(
                new CallSignature(kinds),
                values
            );
        }

        private static MSAst.MethodCallExpression _GetUnicode = Expression.Call(AstMethods.GetUnicodeFunction);

        private MSAst.Expression UnicodeCall() {
            if (_target is NameExpression && ((NameExpression)_target).Name == "unicode") {
                // NameExpressions are always typed to object
                Debug.Assert(_target.Type == typeof(object));

                var tmpVar = Expression.Variable(typeof(object));
                return Expression.Block(
                    new[] { tmpVar },
                    Expression.Assign(tmpVar, _target),
                    Expression.Condition(
                        Expression.Call(
                            AstMethods.IsUnicode,
                            tmpVar
                        ),
                        NormalCall(_GetUnicode),
                        NormalCall(tmpVar)
                    )
                );            
            }
            return null;
        }

        #region IInstructionProvider Members

        void IInstructionProvider.AddInstructions(LightCompiler compiler) {
            if (_target is NameExpression && ((NameExpression)_target).Name == "unicode") {
                compiler.Compile(Reduce());
                return;
            }

            for (int i = 0; i < _args.Length; i++) {
                if (!_args[i].GetArgumentInfo().IsSimple) {
                    compiler.Compile(Reduce());
                    return;
                }
            }

            switch (_args.Length) {
                #region Generated Python Call Expression Instruction Switch

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_call_expression_instruction_switch from: generate_calls.py

                case 0:
                    compiler.Compile(Parent.LocalContext);
                    compiler.Compile(_target);
                    compiler.Instructions.Emit(new Invoke0Instruction(Parent.PyContext));
                    return;
                case 1:
                    compiler.Compile(Parent.LocalContext);
                    compiler.Compile(_target);
                    compiler.Compile(_args[0].Expression);
                    compiler.Instructions.Emit(new Invoke1Instruction(Parent.PyContext));
                    return;
                case 2:
                    compiler.Compile(Parent.LocalContext);
                    compiler.Compile(_target);
                    compiler.Compile(_args[0].Expression);
                    compiler.Compile(_args[1].Expression);
                    compiler.Instructions.Emit(new Invoke2Instruction(Parent.PyContext));
                    return;
                case 3:
                    compiler.Compile(Parent.LocalContext);
                    compiler.Compile(_target);
                    compiler.Compile(_args[0].Expression);
                    compiler.Compile(_args[1].Expression);
                    compiler.Compile(_args[2].Expression);
                    compiler.Instructions.Emit(new Invoke3Instruction(Parent.PyContext));
                    return;
                case 4:
                    compiler.Compile(Parent.LocalContext);
                    compiler.Compile(_target);
                    compiler.Compile(_args[0].Expression);
                    compiler.Compile(_args[1].Expression);
                    compiler.Compile(_args[2].Expression);
                    compiler.Compile(_args[3].Expression);
                    compiler.Instructions.Emit(new Invoke4Instruction(Parent.PyContext));
                    return;
                case 5:
                    compiler.Compile(Parent.LocalContext);
                    compiler.Compile(_target);
                    compiler.Compile(_args[0].Expression);
                    compiler.Compile(_args[1].Expression);
                    compiler.Compile(_args[2].Expression);
                    compiler.Compile(_args[3].Expression);
                    compiler.Compile(_args[4].Expression);
                    compiler.Instructions.Emit(new Invoke5Instruction(Parent.PyContext));
                    return;
                case 6:
                    compiler.Compile(Parent.LocalContext);
                    compiler.Compile(_target);
                    compiler.Compile(_args[0].Expression);
                    compiler.Compile(_args[1].Expression);
                    compiler.Compile(_args[2].Expression);
                    compiler.Compile(_args[3].Expression);
                    compiler.Compile(_args[4].Expression);
                    compiler.Compile(_args[5].Expression);
                    compiler.Instructions.Emit(new Invoke6Instruction(Parent.PyContext));
                    return;

                // *** END GENERATED CODE ***

                #endregion
            }
            compiler.Compile(Reduce());
        }

        #endregion

        abstract class InvokeInstruction : Instruction {
            public override int ProducedStack {
                get {
                    return 1;
                }
            }

            public override string InstructionName {
                get {
                    return "Python Invoke" + (ConsumedStack - 1);
                }
            }
        }

        #region Generated Python Call Expression Instructions

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_call_expression_instructions from: generate_calls.py


        class Invoke0Instruction : InvokeInstruction {
            private readonly CallSite<Func<CallSite, CodeContext, object, object>> _site;

            public Invoke0Instruction(PythonContext context) {
                _site = context.CallSite0;
            }

            public override int ConsumedStack {
                get {
                    return 2;
                }
            }

            public override int Run(InterpretedFrame frame) {

                var target = frame.Pop();
                frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), target));
                return +1;
            }
        }

        class Invoke1Instruction : InvokeInstruction {
            private readonly CallSite<Func<CallSite, CodeContext, object, object, object>> _site;

            public Invoke1Instruction(PythonContext context) {
                _site = context.CallSite1;
            }

            public override int ConsumedStack {
                get {
                    return 3;
                }
            }

            public override int Run(InterpretedFrame frame) {
                var arg0 = frame.Pop();
                var target = frame.Pop();
                frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), target, arg0));
                return +1;
            }
        }

        class Invoke2Instruction : InvokeInstruction {
            private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object>> _site;

            public Invoke2Instruction(PythonContext context) {
                _site = context.CallSite2;
            }

            public override int ConsumedStack {
                get {
                    return 4;
                }
            }

            public override int Run(InterpretedFrame frame) {
                var arg1 = frame.Pop();
                var arg0 = frame.Pop();
                var target = frame.Pop();
                frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), target, arg0, arg1));
                return +1;
            }
        }

        class Invoke3Instruction : InvokeInstruction {
            private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object, object>> _site;

            public Invoke3Instruction(PythonContext context) {
                _site = context.CallSite3;
            }

            public override int ConsumedStack {
                get {
                    return 5;
                }
            }

            public override int Run(InterpretedFrame frame) {
                var arg2 = frame.Pop();
                var arg1 = frame.Pop();
                var arg0 = frame.Pop();
                var target = frame.Pop();
                frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), target, arg0, arg1, arg2));
                return +1;
            }
        }

        class Invoke4Instruction : InvokeInstruction {
            private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object>> _site;

            public Invoke4Instruction(PythonContext context) {
                _site = context.CallSite4;
            }

            public override int ConsumedStack {
                get {
                    return 6;
                }
            }

            public override int Run(InterpretedFrame frame) {
                var arg3 = frame.Pop();
                var arg2 = frame.Pop();
                var arg1 = frame.Pop();
                var arg0 = frame.Pop();
                var target = frame.Pop();
                frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), target, arg0, arg1, arg2, arg3));
                return +1;
            }
        }

        class Invoke5Instruction : InvokeInstruction {
            private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object>> _site;

            public Invoke5Instruction(PythonContext context) {
                _site = context.CallSite5;
            }

            public override int ConsumedStack {
                get {
                    return 7;
                }
            }

            public override int Run(InterpretedFrame frame) {
                var arg4 = frame.Pop();
                var arg3 = frame.Pop();
                var arg2 = frame.Pop();
                var arg1 = frame.Pop();
                var arg0 = frame.Pop();
                var target = frame.Pop();
                frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), target, arg0, arg1, arg2, arg3, arg4));
                return +1;
            }
        }

        class Invoke6Instruction : InvokeInstruction {
            private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object, object>> _site;

            public Invoke6Instruction(PythonContext context) {
                _site = context.CallSite6;
            }

            public override int ConsumedStack {
                get {
                    return 8;
                }
            }

            public override int Run(InterpretedFrame frame) {
                var arg5 = frame.Pop();
                var arg4 = frame.Pop();
                var arg3 = frame.Pop();
                var arg2 = frame.Pop();
                var arg1 = frame.Pop();
                var arg0 = frame.Pop();
                var target = frame.Pop();
                frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), target, arg0, arg1, arg2, arg3, arg4, arg5));
                return +1;
            }
        }

        // *** END GENERATED CODE ***

        #endregion

        internal override string CheckAssign() {
            return "can't assign to function call";
        }

        internal override string CheckDelete() {
            return "can't delete function call";
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_target != null) {
                    _target.Walk(walker);
                }
                if (_args != null) {
                    foreach (Arg arg in _args) {
                        arg.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }

        
    }
}
