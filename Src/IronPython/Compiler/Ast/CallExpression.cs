/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using MSAst = Microsoft.Scripting.Ast;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    public class CallExpression : Expression {
        private readonly Expression _target;
        private readonly Arg[] _args;
        private bool _hasArgsTuple, _hasKeywordDict;
        private int _keywordCount, _extraArgs;
        private static bool _callAction = true;

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
            } else {
                if (nameExpr.Name == Symbols.Eval) return true;
                if (nameExpr.Name == Symbols.ExecFile) return true;
            }
            return false;
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            MSAst.Arg[] args = ag.Transform(_args);
            bool callOk = true;
            if (_callAction && args.Length < 6) {
                MSAst.Expression[] argVals = new MSAst.Expression[args.Length + 1];
                int i = 1;
                argVals[0] = ag.Transform(_target);
                foreach (MSAst.Arg arg in args) {
                    if (arg.Kind == MSAst.Arg.ArgumentKind.Named) {
                        callOk = false;
                        break;
                    }
                    argVals[i] = args[i-1].Expression;
                    i++;
                }

                if (callOk) {
                    return MSAst.ActionExpression.Call(
                        Span,
                        CallAction.Make(args),
                        type,
                        argVals
                    );
                }
            }

            return new MSAst.CallExpression(
                ag.Transform(_target),
                args,
                _hasArgsTuple,
                _hasKeywordDict,
                _keywordCount,
                _extraArgs,
                Span
                );
            
        }

        internal override MSAst.Statement TransformDelete(AstGenerator ag) {
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
