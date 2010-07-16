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
using System.Linq;
using IronPython.Compiler.Ast;
using IronPython.Runtime.Types;
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.PyAnalysis.Values;
using Microsoft.Scripting;
using Microsoft.Scripting.Library;

namespace Microsoft.PyAnalysis {
    /// <summary>
    /// Encapsulates all of the information about a module which has been analyzed.  
    /// 
    /// Can be queried for various information about the resulting analysis.
    /// </summary>
    public class ModuleAnalysis {
        private readonly AnalysisUnit _unit;
        private readonly InterpreterScope[] _scopes;
        private readonly Stack<ScopePositionInfo> _scopeTree;

        internal ModuleAnalysis(AnalysisUnit unit, Stack<ScopePositionInfo> tree) {
            _unit = unit;
            _scopes = unit.Scopes;
            _scopeTree = tree;
        }

        public IEnumerable<VariableResult> GetVariablesFromExpression(string exprText, int lineNumber) {
            var expr = GetExpressionFromText(exprText);
            foreach (var v in GetVariablesForExpression(expr, lineNumber)) {
                yield return new VariableResult(v);
            }
        }

        public IEnumerable<MemberResult> GetMembersFromExpression(string exprText, int lineNumber, bool intersectMultipleResults = true) {
            if (exprText.EndsWith(".")) {
                exprText = exprText.Substring(0, exprText.Length - 1);
                if (exprText.Length == 0) {
                    // don't return all available members on empty dot.
                    return new MemberResult[0];
                }
            } else {
                int cut = exprText.LastIndexOfAny(new[] { '.', ']', ')' });
                if (cut != -1) {
                    exprText = exprText.Substring(0, cut);
                } else {
                    exprText = String.Empty;
                }
            }

            // try {
            if (exprText.Length == 0) {
                return GetAllAvailableMembers(lineNumber);
            } else {
                var expr = GetExpressionFromText(exprText);
                if (expr is ConstantExpression && ((ConstantExpression)expr).Value is int) {
                    // no completions on integer ., the user is typing a float
                    return new MemberResult[0];
                }
                var lookup = GetVariablesForExpression(expr, lineNumber);
                return GetMemberResults(lookup, intersectMultipleResults);
            }
        }

        public IEnumerable<MemberResult> GetAllAvailableMembers(int lineNumber) {
            var result = new Dictionary<string, List<Namespace>>();
            foreach (var scope in FindScopes(lineNumber)) {
                foreach (var kvp in scope.Variables) {
                    result[kvp.Key] = new List<Namespace>(kvp.Value.Types);
                }
            }
            return MemberDictToResultList(result);
        }

        /// <summary>
        /// TODO: This method should go away, it's only being used for tests, and the tests should be using GetMembersFromExpression
        /// which may need to be cleaned up.
        /// </summary>
        public IEnumerable<string> GetMembersFromName(string name, int lineNumber) {
            var lookup = GetVariablesForExpression(GetExpressionFromText(name), lineNumber);
            return GetMemberResults(lookup).Select(m => m.Name);
        }

        /// <summary>
        /// Gets the list of valid names available at the given position in the
        /// analyzed source code.
        /// </summary>
        private IEnumerable<string> GetVariableNames(int lineNumber) {
            var chain = FindScopes(lineNumber);
            foreach (var scope in chain) {
                if (scope.VisibleToChildren || scope == chain[chain.Count - 1]) {
                    foreach (var varName in scope.Variables) {
                        yield return varName.Key;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of valid names available at the given position in
        /// the analyzed source code minus the builtin variables.
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public IEnumerable<string> GetVariablesNoBuiltins(int lineNumber) {
            HashSet<string> v1 = new HashSet<string>(GetVariableNames(lineNumber));
            v1.ExceptWith(Scopes[0].Variables.Keys);
            return v1;
        }

        public IEnumerable<PythonType> GetTypesFromName(string name, int lineNumber) {
            var chain = FindScopes(lineNumber);
            var result = new HashSet<PythonType>();
            foreach (var scope in chain) {
                if (scope.VisibleToChildren || scope == chain[chain.Count - 1]) {
                    var v = scope.GetVariable(name, _unit);
                    if (v == null) {
                        continue;
                    }
                    foreach (var ns in v.Types) {
                        // add the clr type
                        // TODO: handle null?
                        if (ns != null && ns.ClrType != null) {
                            result.Add(ns.ClrType);
                        }
                    }
                }
            }
            return result;
        }

        public OverloadResult[] GetSignaturesFromExpression(string exprText, int lineNumber) {
            try {
                var eval = new ExpressionEvaluator(_unit.CopyForEval(), FindScopes(lineNumber).ToArray());
                var sourceUnit = ProjectState.GetSourceUnitForExpression(exprText);
                using (var parser = Utils.CreateParser(sourceUnit, new CollectingErrorSink())) {
                    var expr = GetExpression(parser.ParseTopExpression().Body);
                    if (expr is ListExpression ||
                        expr is TupleExpression ||
                        expr is DictionaryExpression) {
                        return new OverloadResult[0];
                    }
                    var lookup = eval.Evaluate(expr);

                    var result = new List<OverloadResult>();

                    // TODO: Include relevant type info on the parameter...
                    foreach (var ns in lookup) {                        
                        result.AddRange(ns.Overloads);                        
                    }

                    return result.ToArray();
                }
            } catch (Exception) {
                // TODO: log exception
                return new[] { new SimpleOverloadResult(new ParameterResult[0], "Unknown", "IntellisenseError_Sigs") };
            }
        }

        /// <summary>
        /// Gets the top-level scope for the module.
        /// </summary>
        internal ModuleInfo GlobalScope {
            get {
                var result = (Scopes[1] as ModuleScope);
                Debug.Assert(result != null);
                return result.Module;
            }
        }

        /// <summary>
        /// Gets the tree of all scopes for the module.
        /// </summary>
        internal Stack<ScopePositionInfo> ScopeTree {
            get { return _scopeTree; }
        }

        internal ProjectState ProjectState {
            get { return GlobalScope.ProjectEntry.ProjectState; }
        }

        internal InterpreterScope[] Scopes {
            get { return _scopes; }
        }

        internal IEnumerable<MemberResult> GetMemberResults(IEnumerable<Namespace> vars, bool intersectMultipleResults = true) {
            IList<Namespace> namespaces = new List<Namespace>();
            foreach (var ns in vars) {
                if (ns != null) {
                    namespaces.Add(ns);
                }
            }

            if (namespaces.Count == 1) {
                // optimize for the common case of only a single namespace
                var newMembers = namespaces[0].GetAllMembers(GlobalScope.ShowClr);
                if (newMembers == null || newMembers.Count == 0) {
                    return new MemberResult[0];
                }

                return SingleMemberResult(newMembers);
            }

            Dictionary<string, List<Namespace>> memberDict = null;
            HashSet<string> memberSet = null;
            foreach (Namespace ns in namespaces) {
                if (ProjectState._noneInst   == ns) {
                    continue;
                }

                var newMembers = ns.GetAllMembers(GlobalScope.ShowClr);
                // IntersectMembers(members, memberSet, memberDict);
                if (newMembers == null || newMembers.Count == 0) {
                    continue;
                }

                if (memberSet == null) {
                    // first namespace, add everything
                    memberSet = new HashSet<string>(newMembers.Keys);
                    memberDict = new Dictionary<string, List<Namespace>>();
                    foreach (var kvp in newMembers) {
                        var tmp = new List<Namespace>(kvp.Value);
                        memberDict[kvp.Key] = tmp;
                    }
                } else {
                    // 2nd or nth namespace, union or intersect
                    HashSet<string> toRemove;
                    IEnumerable<string> adding;

                    if (intersectMultipleResults) {
                        adding = new HashSet<string>(newMembers.Keys);
                        // Find the things only in memberSet that we need to remove from memberDict
                        // toRemove = (memberSet ^ adding) & memberSet

                        toRemove = new HashSet<string>(memberSet);
                        toRemove.SymmetricExceptWith(adding);
                        toRemove.IntersectWith(memberSet);

                        // intersect memberSet with what we're adding
                        memberSet.IntersectWith(adding);

                        // we're only adding things they both had
                        adding = memberSet;
                    } else {
                        // we're adding all of newMembers keys
                        adding = newMembers.Keys;
                        toRemove = null;
                    }

                    // update memberDict
                    foreach (var name in adding) {
                        List<Namespace> values;
                        if (!memberDict.TryGetValue(name, out values)) {
                            memberDict[name] = values = new List<Namespace>();
                        }
                        values.AddRange(newMembers[name]);
                    }

                    if (toRemove != null) {
                        foreach (var name in toRemove) {
                            memberDict.Remove(name);
                        }
                    }
                }
            }
            
            if (memberDict == null) {
                return new MemberResult[0];
            }
            return MemberDictToResultList(memberDict);
        }

        private Expression GetExpressionFromText(string exprText) {
            SourceUnit sourceUnit = ProjectState.GetSourceUnitForExpression(exprText);
            using (var parser = Utils.CreateParser(sourceUnit, new CollectingErrorSink())) {
                return GetExpression(parser.ParseTopExpression().Body);
            }
        }

        private ISet<Namespace> GetVariablesForExpression(Expression expr, int lineNumber) {
            return new ExpressionEvaluator(_unit.CopyForEval(), FindScopes(lineNumber).ToArray()).Evaluate(expr);
        }

        private static IEnumerable<MemberResult> SingleMemberResult(IDictionary<string, ISet<Namespace>> memberDict) {
            foreach (var kvp in memberDict) {
                yield return new MemberResult(kvp.Key, kvp.Value);
            }
        }

        private Expression GetExpression(Statement statement) {
            if (statement is ExpressionStatement) {
                return ((ExpressionStatement)statement).Expression;
            } else if (statement is ReturnStatement) {
                return ((ReturnStatement)statement).Expression;
            } else {
                return null;
            }
        }        

        /// <summary>
        /// Gets the chain of scopes which are associated with the given position in the code.
        /// </summary>
        private List<InterpreterScope> FindScopes(int lineNumber) {
            ScopePositionInfo curScope = ScopeTree.First();
            ScopePositionInfo prevScope = null;
            var chain = new List<InterpreterScope> { Scopes[0], Scopes[1] };

            while (curScope != prevScope) {
                prevScope = curScope;

                // TODO: Binary search?
                foreach (var scope in curScope.Children) {
                    if (scope.Start <= lineNumber && scope.Stop >= lineNumber) {
                        curScope = scope;                        
                        chain.Add(curScope.Scope);
                        break;
                    }
                }
            }
            return chain;
        }

        private static IEnumerable<MemberResult> MemberDictToResultList(Dictionary<string, List<Namespace>> memberDict) {
            foreach (var kvp in memberDict) {
                yield return new MemberResult(kvp.Key, kvp.Value);
            }
        }
    }
}
