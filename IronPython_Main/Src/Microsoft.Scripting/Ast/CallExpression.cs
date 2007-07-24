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
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class CallExpression : Expression {
        private readonly Expression _target;
        private readonly Arg[] _args;
        private bool _hasArgsTuple, _hasKeywordDict;
        private int _keywordCount, _extraArgs;

        internal CallExpression(SourceSpan span, Expression target, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs)
            : base(span) {
            _target = target;
            _args = args;
            _hasArgsTuple = hasArgsTuple;
            _hasKeywordDict = hasKeywordDictionary;
            _keywordCount = keywordCount;
            _extraArgs = extraArgs;
        }

        public IList<Arg> Args {
            get { return _args; }
        }

        public Expression Target {
            get { return _target; }
        }

        public override object Evaluate(CodeContext context) {
            object callee = _target.Evaluate(context);
            object[] cargs = new object[_args.Length - _extraArgs];
            object argsTuple = null, keywordDict = null;
            string[] keywordNames = new string[_keywordCount];
            int index = 0, keywordIndex = 0;

            foreach (Arg arg in _args) {
                switch (arg.Kind) {
                    case ArgumentKind.List:
                        argsTuple = arg.Expression.Evaluate(context);
                        continue;

                    case ArgumentKind.Dictionary:
                        keywordDict = arg.Expression.Evaluate(context);
                        continue;

                    case ArgumentKind.Simple:
                        break;

                    case ArgumentKind.Named:
                        Debug.Assert(arg.Name != SymbolId.Empty);
                        keywordNames[keywordIndex++] = SymbolTable.IdToString(arg.Name);
                        break;

                }
                cargs[index++] = arg.Expression.Evaluate(context);
            }
            if (_hasKeywordDict || (_hasArgsTuple && _keywordCount > 0)) {
                return RuntimeHelpers.CallWithArgsKeywordsTupleDict(context, callee, cargs, keywordNames, argsTuple, keywordDict);
            } else if (_hasArgsTuple) {
                return RuntimeHelpers.CallWithArgsTuple(context, callee, cargs, argsTuple);
            } else if (_keywordCount > 0) {
                return RuntimeHelpers.CallWithKeywordArgs(context, callee, cargs, keywordNames);
            } else {
                return RuntimeHelpers.CallWithContext(context, callee, cargs);
            }
        }

        public override void Emit(CodeGen cg) {
            Expression[] exprs = new Expression[_args.Length - _extraArgs];
            Expression argsTuple = null, keywordDict = null;
            string[] keywordNames = new string[_keywordCount];
            int index = 0, keywordIndex = 0;
            foreach (Arg arg in _args) {
                switch (arg.Kind) {
                    case ArgumentKind.List:
                        argsTuple = arg.Expression;
                        continue;
                    case ArgumentKind.Dictionary:
                        keywordDict = arg.Expression;
                        continue;
                    case ArgumentKind.Simple:
                        break;
                    case ArgumentKind.Named:
                        Debug.Assert(arg.Name != SymbolId.Empty);
                        keywordNames[keywordIndex++] = SymbolTable.IdToString(arg.Name);
                        break;
                    
                }
                exprs[index++] = arg.Expression;
            }

            if (_hasKeywordDict || (_hasArgsTuple && _keywordCount > 0)) {
                cg.EmitCodeContext();
                _target.EmitAsObject(cg);
                cg.EmitArrayFromExpressions(typeof(object), exprs);
                cg.EmitArray(keywordNames);
                cg.EmitExprAsObjectOrNull(argsTuple);
                cg.EmitExprAsObjectOrNull(keywordDict);
                cg.EmitCall(typeof(RuntimeHelpers), "CallWithArgsKeywordsTupleDict",
                    new Type[] { typeof(CodeContext), typeof(object), typeof(object[]), typeof(string[]),
							   typeof(object), typeof(object)});
            } else if (_hasArgsTuple) {
                cg.EmitCodeContext();
                _target.EmitAsObject(cg);
                cg.EmitArrayFromExpressions(typeof(object), exprs);
                cg.EmitExprAsObjectOrNull(argsTuple);
                cg.EmitCall(typeof(RuntimeHelpers), "CallWithArgsTuple",
                    new Type[] { typeof(CodeContext), typeof(object), typeof(object[]), typeof(object) });
            } else if (_keywordCount > 0) {
                cg.EmitCodeContext();
                _target.EmitAsObject(cg);
                cg.EmitArrayFromExpressions(typeof(object), exprs);
                cg.EmitArray(keywordNames);
                cg.EmitCall(typeof(RuntimeHelpers), "CallWithKeywordArgs",
                    new Type[] { typeof(CodeContext), typeof(object), typeof(object[]), typeof(string[]) });
            } else {
                cg.EmitCodeContext();
                _target.EmitAsObject(cg);
                if (_args.Length <= CallTargets.MaximumCallArgs) {
                    Type[] argTypes = new Type[_args.Length + 2];
                    int i = 0;
                    argTypes[i++] = typeof(CodeContext);
                    argTypes[i++] = typeof(object);
                    foreach (Expression e in exprs) {
                        e.EmitAsObject(cg);
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

    public static partial class Ast {
        public static CallExpression DynamicCall(Expression target, Arg[] args) {
            return DynamicCall(SourceSpan.None, target, args, false, false, 0, 0);
        }
        public static CallExpression DynamicCall(SourceSpan span, Expression target, Arg[] args) {
            return DynamicCall(span, target, args, false, false, 0, 0);
        }
        public static CallExpression DynamicCall(Expression target, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs) {
            return DynamicCall(SourceSpan.None, target, args, hasArgsTuple, hasKeywordDictionary, keywordCount, extraArgs);
        }
        public static CallExpression DynamicCall(SourceSpan span, Expression target, Arg[] args, bool hasArgsTuple, bool hasKeywordDictionary, int keywordCount, int extraArgs) {
            return new CallExpression(span, target, args, hasArgsTuple, hasKeywordDictionary, keywordCount, extraArgs);
        }
    }
}
