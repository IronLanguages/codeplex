/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using Microsoft.PyAnalysis.Values;
using Microsoft.Scripting.Utils;

namespace Microsoft.PyAnalysis.Interpreter {
    internal class ExpressionEvaluator {
        private readonly AnalysisUnit _unit;
        private readonly InterpreterScope[] _currentScopes;

        /// <summary>
        /// Creates a new ExpressionEvaluator that will evaluate in the context of the top-level module.
        /// </summary>
        public ExpressionEvaluator(AnalysisUnit unit) {
            _unit = unit;
            _currentScopes = unit.Scopes;
        }

        public ExpressionEvaluator(AnalysisUnit unit, InterpreterScope[] scopes) {
            _unit = unit;
            _currentScopes = scopes;
        }

        #region Public APIs

        /// <summary>
        /// Returns possible variable refs associated with the expr in the expression evaluators scope.
        /// </summary>
        public ISet<Namespace> Evaluate(Expression node) {
            var res = EvaluateWorker(node);
            Debug.Assert(res != null);
            return res;
        }

        public ISet<Namespace> EvaluateMaybeNull(Expression node) {
            if (node == null) {
                return null;
            }

            return Evaluate(node);
        }

        /// <summary>
        /// Returns a sequence of possible types associated with the name in the expression evaluators scope.
        /// </summary>
        public ISet<Namespace> LookupNamespaceByName(Node node, string name, bool addRef = true) {
            for (int i = Scopes.Length - 1; i >= 0; i--) {
                if (i == Scopes.Length - 1 || Scopes[i].VisibleToChildren) {
                    var refs = Scopes[i].GetVariable(node, _unit, name, addRef);
                    if (refs != null) {
                        return refs.Types;
                    }
                }
            }

            return ProjectState.BuiltinModule.GetMember(node, _unit, name);
        }

        /// <summary>
        /// Returns the variable definition for the given name.
        /// </summary>
        public VariableDef LookupVariableByName(string name, Node node, bool addReference = true) {
            for (int i = Scopes.Length - 1; i >= 0; i--) {
                if (i == Scopes.Length - 1 || Scopes[i].VisibleToChildren) {
                    var value = Scopes[i].GetVariable(node, _unit, name, addReference);
                    if (value != null) {
                        return value;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Implementation Details

        private ModuleInfo GlobalScope {
            get { return _unit.DeclaringModule; }
        }

        private ProjectState ProjectState {
            get { return _unit.ProjectState; }
        }        
        
        /// <summary>
        /// Gets the list of scopes which define the current context.
        /// </summary>
        private InterpreterScope[] Scopes {
            get { return _currentScopes; }
        }

        private ISet<Namespace>[] Evaluate(IList<Arg> nodes) {
            var result = new ISet<Namespace>[nodes.Count];
            for (int i = 0; i < nodes.Count; i++) {
                result[i] = Evaluate(nodes[i].Expression);
            }
            return result;
        }

        private ISet<Namespace> EvaluateWorker(Node node) {
            EvalDelegate eval;
            if (_evaluators.TryGetValue(node.GetType(), out eval)) {
                return eval(this, node);
            }

            return EmptySet<Namespace>.Instance;
        }

        delegate ISet<Namespace> EvalDelegate(ExpressionEvaluator ee, Node node);

        private static Dictionary<Type, EvalDelegate> _evaluators = new Dictionary<Type, EvalDelegate> {
            { typeof(AndExpression),  ExpressionEvaluator.EvaluateAnd }, 
            { typeof(BackQuoteExpression),  ExpressionEvaluator.EvaluateBackQuote },
            { typeof(BinaryExpression),  ExpressionEvaluator.EvaluateBinary },
            { typeof(CallExpression),  ExpressionEvaluator.EvaluateCall},
            { typeof(ConditionalExpression),  ExpressionEvaluator.EvaluateConditional},
            { typeof(ConstantExpression),  ExpressionEvaluator.EvaluateConstant},
            { typeof(DictionaryExpression),  ExpressionEvaluator.EvaluateDictionary},
            { typeof(SetExpression),  ExpressionEvaluator.EvaluateSet},
            { typeof(GeneratorExpression),  ExpressionEvaluator.EvaluateGenerator},
            { typeof(IndexExpression),  ExpressionEvaluator.EvaluateIndex},
            { typeof(LambdaExpression),  ExpressionEvaluator.EvaluateLambda},
            { typeof(ListComprehension),  ExpressionEvaluator.EvaluateListComprehension},
            { typeof(MemberExpression),  ExpressionEvaluator.EvaluateMember},
            { typeof(NameExpression),  ExpressionEvaluator.EvaluateName},
            { typeof(OrExpression),  ExpressionEvaluator.EvaluateOr},
            { typeof(ParenthesisExpression),  ExpressionEvaluator.EvaluateParenthesis},
            { typeof(UnaryExpression),  ExpressionEvaluator.EvaluateUnary },
            { typeof(YieldExpression),  ExpressionEvaluator.EvaluateYield},
            { typeof(TupleExpression),  ExpressionEvaluator.EvaluateSequence},
            { typeof(ListExpression),  ExpressionEvaluator.EvaluateSequence},            
            { typeof(SliceExpression),  ExpressionEvaluator.EvaluateSlice},            
        };

        private static ISet<Namespace> EvaluateSequence(ExpressionEvaluator ee, Node node) {
            // Covers both ListExpression and TupleExpression
            return ee.GlobalScope.GetOrMakeNodeVariable(node, (n) => ee.MakeSequence(ee, n));
        }

        private static ISet<Namespace> EvaluateParenthesis(ExpressionEvaluator ee, Node node) {
            var n = (ParenthesisExpression)node;
            return ee.Evaluate(n.Expression);
        }

        private static ISet<Namespace> EvaluateOr(ExpressionEvaluator ee, Node node) {
            // TODO: Warn if lhs is always false
            var n = (OrExpression)node;
            var result = ee.Evaluate(n.Left);
            return result.Union(ee.Evaluate(n.Right));
        }

        private static ISet<Namespace> EvaluateName(ExpressionEvaluator ee, Node node) {
            var n = (NameExpression)node;
            var res = ee.LookupNamespaceByName(node, n.Name);
            foreach (var value in res) {
                value.AddReference(node, ee._unit);
            }
            return res;
        }

        private static ISet<Namespace> EvaluateMember(ExpressionEvaluator ee, Node node) {
            var n = (MemberExpression)node;
            return ee.Evaluate(n.Target).GetMember(node, ee._unit, n.Name);
        }

        private static ISet<Namespace> EvaluateIndex(ExpressionEvaluator ee, Node node) {
            var n = (IndexExpression)node;

            return ee.Evaluate(n.Target).GetIndex(n, ee._unit, ee.Evaluate(n.Index));
        }

        private static ISet<Namespace> EvaluateSet(ExpressionEvaluator ee, Node node) {
            var n = (SetExpression)node;
            ISet<Namespace> result;
            if (!ee.GlobalScope.NodeVariables.TryGetValue(node, out result)) {
                var values = new HashSet<Namespace>();
                foreach (var x in n.Items) {
                    values.Union(ee.Evaluate(x));
                }

                result = new DictionaryInfo(values, values, ee.ProjectState, ee.GlobalScope.ShowClr).SelfSet;
                ee.GlobalScope.NodeVariables[node] = result;
            }
            return result;
        }

        private static ISet<Namespace> EvaluateDictionary(ExpressionEvaluator ee, Node node) {
            var n = (DictionaryExpression)node;
            ISet<Namespace> result;
            if (!ee.GlobalScope.NodeVariables.TryGetValue(node, out result)) {
                var keys = new HashSet<Namespace>();
                var values = new HashSet<Namespace>();
                foreach (var x in n.Items) {
                    foreach (var keyVal in ee.Evaluate(x.SliceStart)) {
                        keys.Add(keyVal);
                    }
                    foreach (var itemVal in ee.Evaluate(x.SliceStop)) {
                        values.Add(itemVal);
                    }
                }

                result = new DictionaryInfo(keys, values, ee.ProjectState, ee.GlobalScope.ShowClr).SelfSet;
                ee.GlobalScope.NodeVariables[node] = result;
            }
            return result;
        }

        private static ISet<Namespace> EvaluateConstant(ExpressionEvaluator ee, Node node) {
            var n = (ConstantExpression)node;
            return ee.ProjectState.GetConstant(n.Value);
        }

        private static ISet<Namespace> EvaluateConditional(ExpressionEvaluator ee, Node node) {
            var n = (ConditionalExpression)node;
            ee.Evaluate(n.Test);
            var result = ee.Evaluate(n.TrueExpression);
            return result.Union(ee.Evaluate(n.FalseExpression));
        }

        private static ISet<Namespace> EvaluateBackQuote(ExpressionEvaluator ee, Node node) {
            var strType = ee.ProjectState.GetNamespaceFromObjects(typeof(string));
            return strType.SelfSet;
        }

        private static ISet<Namespace> EvaluateAnd(ExpressionEvaluator ee, Node node) {
            var n = (AndExpression)node;
            var result = ee.Evaluate(n.Left);
            return result.Union(ee.Evaluate(n.Right));
        }

        private static ISet<Namespace> EvaluateCall(ExpressionEvaluator ee, Node node) {
            // TODO: Splatting, keyword args

            // Get the argument types that we're providing at this call site
            var n = (CallExpression)node;
            var argTypes = ee.Evaluate(n.Args);

            // Then lookup the possible methods we're calling
            var targetRefs = ee.Evaluate(n.Target);

            ISet<Namespace> res = EmptySet<Namespace>.Instance;
            bool madeSet = false;
            foreach (var target in targetRefs) {
                res = res.Union(target.Call(node, ee._unit, argTypes, GetNamedArguments(n.Args)), ref madeSet);
            }

            return res;
        }


        private static string[] GetNamedArguments(IList<Arg> args) {
            string[] res = null;
            for (int i = 0; i < args.Count; i++) {
                if (args[i].Name != null) {
                    if (res == null) {
                        res = new string[args.Count - i];
                    }

                    res[i - (args.Count - res.Length)] = args[i].Name;
                }
            }
            return res ?? ArrayUtils.EmptyStrings;
        }

        private static ISet<Namespace> EvaluateUnary(ExpressionEvaluator ee, Node node) {            
            var n = (UnaryExpression)node;
            return ee.Evaluate(n.Expression).UnaryOperation(node, ee._unit, n.Op); ;
        }

        private static ISet<Namespace> EvaluateBinary(ExpressionEvaluator ee, Node node) {
            var n = (BinaryExpression)node;

            return ee.Evaluate(n.Left).BinaryOperation(node, ee._unit, n.Operator, ee.Evaluate(n.Right));
        }

        private static ISet<Namespace> EvaluateYield(ExpressionEvaluator ee, Node node) {
            var yield = (YieldExpression)node;
            var funcDef = ee._currentScopes[ee._currentScopes.Length - 1].Namespace as FunctionInfo;
            if (funcDef != null) {
                var gen = funcDef.Generator;

                gen.AddYield(ee.Evaluate(yield.Expression));

                return gen.Sends.Types;
            }

            return EmptySet<Namespace>.Instance;
        }

        private static ISet<Namespace> EvaluateListComprehension(ExpressionEvaluator ee, Node node) {
            ListComprehension listComp = (ListComprehension)node;
                       
            for(int i = 0; i<listComp.Iterators.Count;i++) {
                
                ComprehensionFor compFor = listComp.Iterators[i] as ComprehensionFor;
                if (compFor != null) {
                    foreach (var listType in ee.Evaluate(compFor.List)) {                        
                        //ee.AssignTo(node, node.Left, listType.GetEnumeratorTypes(node, _unit));
                    }
                }
            }

/*            return ee.GlobalScope.GetOrMakeNodeVariable(
                node, 
                (x) => new ListInfo(new[] { ee.Evaluate(listComp.Item) }, ee._unit.ProjectState._listType).SelfSet);*/

            return ee.GlobalScope.GetOrMakeNodeVariable(
                node,
                (x) => new ListInfo(new ISet<Namespace>[0], ee._unit.ProjectState._listType).SelfSet);
        }

        private static ISet<Namespace> EvaluateLambda(ExpressionEvaluator ee, Node node) {
            var lambda = (LambdaExpression)node;

            return ee.GlobalScope.GetOrMakeNodeVariable(node, n => MakeLambdaFunction(lambda, ee));
        }

        private static ISet<Namespace> MakeLambdaFunction(LambdaExpression node, ExpressionEvaluator ee) {
            return ee.GlobalScope.NodeVariables[node.Function];
        }

        private static ISet<Namespace> EvaluateGenerator(ExpressionEvaluator ee, Node node) {
            GeneratorExpression gen = (GeneratorExpression)node;

            ee.Evaluate(gen.Iterable);

            // TODO: Implement
            return EmptySet<Namespace>.Instance;
        }

        private static ISet<Namespace> EvaluateSlice(ExpressionEvaluator ee, Node node) {
            SliceExpression se = node as SliceExpression;

            return ee.GlobalScope.GetOrMakeNodeVariable(
                node, 
                (n) => new SliceInfo(
                    ee.EvaluateMaybeNull(se.SliceStart),
                    ee.EvaluateMaybeNull(se.SliceStop),
                    se.StepProvided ? ee.EvaluateMaybeNull(se.SliceStep) : null
                )
            );
        }

        private ISet<Namespace> MakeSequence(ExpressionEvaluator ee, Node node) {
            ISet<Namespace> result;
            if (!ee.GlobalScope.NodeVariables.TryGetValue(node, out result)) {
                var seqItems = ((SequenceExpression)node).Items;
                var indexValues = new ISet<Namespace>[seqItems.Count];

                for (int i = 0; i < seqItems.Count; i++) {
                    indexValues[i] = Evaluate(seqItems[i]);
                }

                ISet<Namespace> sequence;
                if (node is ListExpression) {
                    sequence = new ListInfo(indexValues, _unit.ProjectState._listType).SelfSet;
                } else {
                    Debug.Assert(node is TupleExpression);
                    sequence = new SequenceInfo(indexValues, _unit.ProjectState._tupleType).SelfSet;
                }

                ee.GlobalScope.NodeVariables[node] = result = sequence;
            }

            return result;
        }

        #endregion
    }
}
