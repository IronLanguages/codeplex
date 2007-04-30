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
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class CallExpression : Expression {
        private readonly Expression _target;
        private readonly Arg[] _args;
        private bool _hasArgsTuple, _hasKeywordDict;
        private int _keywordCount, _extraArgs;

        public CallExpression(Expression target, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs)
            : this(target, args, hasArgsTuple, hasKeywordDictionary, keywordCount, extraArgs, SourceSpan.None) {
        }

        public CallExpression(Expression target, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs, SourceSpan span)
            : base(span) {
            this._target = target;
            this._args = args;
            this._hasArgsTuple = hasArgsTuple;
            this._hasKeywordDict = hasKeywordDictionary;
            this._keywordCount = keywordCount;
            this._extraArgs = extraArgs;
        }

        public IList<Arg> Args {
            get { return _args; }
        }

        public Expression Target {
            get { return _target; }
        }

        public override object Evaluate(CodeContext context) {
            object callee = _target.Evaluate(context);

            object[] cargs = new object[_args.Length];
            int index = 0;
            foreach (Arg arg in _args) {
                if (arg.Name != SymbolId.Empty) throw new NotImplementedException("keywords");
                cargs[index++] = arg.Expression.Evaluate(context);
            }

            switch (cargs.Length) {
                case 0: return RuntimeHelpers.CallWithContext(context, callee);
                default: return RuntimeHelpers.CallWithContext(context, callee, cargs);
            }
        }

        public override void Emit(CodeGen cg) {
            Expression[] exprs = new Expression[_args.Length - _extraArgs];
            Expression argsTuple = null, keywordDict = null;
            string[] keywordNames = new string[_keywordCount];
            int index = 0, keywordIndex = 0;
            foreach (Arg arg in _args) {
                switch (arg.Kind) {
                    case Arg.ArgumentKind.List:
                        argsTuple = arg.Expression;
                        continue;
                    case Arg.ArgumentKind.Dictionary:
                        keywordDict = arg.Expression;
                        continue;
                    case Arg.ArgumentKind.Simple:
                        break;
                    case Arg.ArgumentKind.Named:
                        Debug.Assert(arg.Name != SymbolId.Empty);
                        keywordNames[keywordIndex++] = SymbolTable.IdToString(arg.Name);
                        break;
                    
                }
                exprs[index++] = arg.Expression;
            }

            if (_hasKeywordDict || (_hasArgsTuple && _keywordCount > 0)) {
                cg.EmitCodeContext();
                _target.Emit(cg);
                cg.EmitArrayFromExpressions(typeof(object), exprs);
                cg.EmitArray(keywordNames);
                cg.EmitExprOrNull(argsTuple);
                cg.EmitExprOrNull(keywordDict);
                cg.EmitCall(typeof(RuntimeHelpers), "CallWithArgsKeywordsTupleDict",
                    new Type[] { typeof(CodeContext), typeof(object), typeof(object[]), typeof(string[]),
							   typeof(object), typeof(object)});
            } else if (_hasArgsTuple) {
                cg.EmitCodeContext();
                _target.Emit(cg);
                cg.EmitArrayFromExpressions(typeof(object), exprs);
                cg.EmitExprOrNull(argsTuple);
                cg.EmitCall(typeof(RuntimeHelpers), "CallWithArgsTuple",
                    new Type[] { typeof(CodeContext), typeof(object), typeof(object[]), typeof(object) });
            } else if (_keywordCount > 0) {
                cg.EmitCodeContext();
                _target.Emit(cg);
                cg.EmitArrayFromExpressions(typeof(object), exprs);
                cg.EmitArray(keywordNames);
                cg.EmitCall(typeof(RuntimeHelpers), "CallWithKeywordArgs",
                    new Type[] { typeof(CodeContext), typeof(object), typeof(object[]), typeof(string[]) });
            } else {
                cg.EmitCodeContext();
                _target.Emit(cg);
                if (_args.Length <= CallTargets.MaximumCallArgs) {
                    Type[] argTypes = new Type[_args.Length + 2];
                    int i = 0;
                    argTypes[i++] = typeof(CodeContext);
                    argTypes[i++] = typeof(object);
                    foreach (Expression e in exprs) {
                        e.EmitAs(cg, typeof(object));
                        argTypes[i++] = typeof(object);
                    }
                    cg.EmitCall(typeof(RuntimeHelpers), "CallWithContext", argTypes);
                } else {
                    cg.EmitArrayFromExpressions(typeof(object), exprs);
                    cg.EmitCall(typeof(RuntimeHelpers), "CallWithContext",
                        new Type[] { typeof(CodeContext), typeof(object), typeof(object[]) });
                }
            }
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _target.Walk(walker);
                foreach (Arg arg in _args) arg.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
