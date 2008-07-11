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

using System;
using System.Scripting.Actions;
using IronPython.Runtime;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class CallExpression : Expression {
        private readonly Expression _target;
        private readonly Arg[] _args;
        private bool _hasArgsTuple, _hasKeywordDict;
        private int _keywordCount, _extraArgs;

        public CallExpression(Expression target, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs) {
            _target = target;
            _args = args;
            _hasArgsTuple = hasArgsTuple;
            _hasKeywordDict = hasKeywordDictionary;
            _keywordCount = keywordCount;
            _extraArgs = extraArgs;
        }

        public Expression Target {
            get { return _target; }
        }

        public Arg[] Args {
            get { return _args; }
        } 

        public bool NeedsLocalsDictionary() {
            NameExpression nameExpr = _target as NameExpression;
            if (nameExpr == null) return false;

            if (_args.Length == 0) {
                if (nameExpr.Name == Symbols.Locals) return true;
                if (nameExpr.Name == Symbols.Vars) return true;
                if (nameExpr.Name == Symbols.Dir) return true;
                return false;
            } else if (_args.Length == 1 && (nameExpr.Name == Symbols.Dir || nameExpr.Name == Symbols.Vars)) {
                if (_args[0].Name == Symbols.Star || _args[0].Name == Symbols.StarStar) {
                    // could be splatting empty list or dict resulting in 0-param call which needs context
                    return true;
                }
            } else if (_args.Length == 2 && (nameExpr.Name == Symbols.Dir || nameExpr.Name == Symbols.Vars)) {
                if (_args[0].Name == Symbols.Star && _args[1].Name == Symbols.StarStar) {
                    // could be splatting empty list and dict resulting in 0-param call which needs context
                    return true;
                }
            } else {
                if (nameExpr.Name == Symbols.Eval) return true;
                if (nameExpr.Name == Symbols.ExecFile) return true;
            }
            return false;
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            MSAst.Expression[] values = new MSAst.Expression[_args.Length + 2];
            ArgumentInfo[] kinds = new ArgumentInfo[_args.Length];

            values[0] = Ast.CodeContext();
            values[1] = ag.Transform(_target);

            for (int i = 0; i < _args.Length; i++) {
                kinds[i] = _args[i].Transform(ag, out values[i + 2]);
            }

            return AstUtils.Call(
                OldCallAction.Make(ag.Binder, new CallSignature(kinds)),
                type,
                values
            );
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            ag.AddError("can't delete function call", Span);
            return null;
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
