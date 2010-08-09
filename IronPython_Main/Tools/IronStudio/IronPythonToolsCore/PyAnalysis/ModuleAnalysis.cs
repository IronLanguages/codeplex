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
    /// Encapsulates all of the information about a single module which has been analyzed.  
    /// 
    /// Can be queried for various information about the resulting analysis.
    /// </summary>
    public sealed class ModuleAnalysis {
        private readonly AnalysisUnit _unit;
        private readonly InterpreterScope[] _scopes;
        private readonly Stack<ScopePositionInfo> _scopeTree;

        internal ModuleAnalysis(AnalysisUnit unit, Stack<ScopePositionInfo> tree) {
            _unit = unit;
            _scopes = unit.Scopes;
            _scopeTree = tree;
        }

        #region Public API

        /// <summary>
        /// Evaluates the given expression in at the provided line number and returns the values
        /// that the expression can evaluate to.
        /// </summary>
        /// <param name="exprText">The expression to determine the result of.</param>
        /// <param name="lineNumber">The line number to evaluate at within the module.</param>
        public IEnumerable<IAnalysisValue> GetValues(string exprText, int lineNumber) {
            var expr = GetExpressionFromText(exprText);
            var scopes = FindScopes(lineNumber);
            var eval = new ExpressionEvaluator(_unit.CopyForEval(), scopes.ToArray());

            var res = eval.Evaluate(expr);
            foreach (var v in res) {
                yield return v;
            }
        }

        private IEnumerable<IAnalysisVariable> ToVariables(IReferenceable referenceable) {
            LocatedVariableDef locatedDef = referenceable as LocatedVariableDef;
            if (locatedDef != null) {
                yield return new AnalysisVariable(VariableType.Definition, new LocationInfo(locatedDef.Entry, locatedDef.Node.Start.Line, locatedDef.Node.Start.Column, locatedDef.Node.Span.Length));
            }

            foreach (var reference in referenceable.Definitions) {
                yield return new AnalysisVariable(VariableType.Definition, new LocationInfo(reference.Key, reference.Value.Line, reference.Value.Column, reference.Value.Length));
            }

            foreach (var reference in referenceable.References) {
                yield return new AnalysisVariable(VariableType.Reference, new LocationInfo(reference.Key, reference.Value.Line, reference.Value.Column, reference.Value.Length));
            }
        }

        /// <summary>
        /// Gets the variables the given expression evaluates to.  Variables include parameters, locals, and fields assigned on classes, modules and instances.
        /// 
        /// Variables are classified as either definitions or references.  Only parameters have unique definition points - all other types of variables
        /// have only one or more references.
        /// </summary>
        public IEnumerable<IAnalysisVariable> GetVariables(string exprText, int lineNumber) {
            var expr = GetExpressionFromText(exprText);
            var scopes = FindScopes(lineNumber);
            var eval = new ExpressionEvaluator(_unit.CopyForEval(), FindScopes(lineNumber).ToArray());
            NameExpression name = expr as NameExpression;
            if (name != null) {
                for (int i = scopes.Count - 1; i >= 0; i--) {
                    VariableDef def;
                    if (IncludeScope(scopes, i, lineNumber) && scopes[i].Variables.TryGetValue(name.Name, out def)) {
                        foreach (var res in ToVariables(def)) {
                            yield return res;
                        }

                        if (scopes[i] is FunctionScope) {
                            // if this is a parameter or a local indicate any values which we know are assigned to it.
                            foreach (var type in def.Types) {
                                if (type.Location != null) {
                                    yield return new AnalysisVariable(VariableType.Value, type.Location);
                                }
                            }
                        } else if (scopes[i] is ModuleScope) {
                            foreach (var type in def.Types) {
                                if (type.Location != null) {
                                    yield return new AnalysisVariable(VariableType.Definition, type.Location);
                                }

                                foreach (var reference in type.References) {
                                    yield return new AnalysisVariable(VariableType.Reference, reference);
                                }
                            }
                        }

                    }
                }

                var variables = _unit.ProjectState.BuiltinModule.GetDefinitions(name.Name);
                foreach (var referenceable in variables) {
                    foreach (var res in ToVariables(referenceable)) {
                        yield return res;
                    }
                }
            }

            MemberExpression member = expr as MemberExpression;
            if (member != null) {
                var objects = eval.Evaluate(member.Target);

                foreach (var v in objects) {
                    var container = v as IReferenceableContainer;
                    if (container != null) {
                        var defs = container.GetDefinitions(member.Name);

                        foreach (var def in defs) {
                            foreach (var reference in def.Definitions) {
                                yield return new AnalysisVariable(VariableType.Definition, new LocationInfo(reference.Key, reference.Value.Line, reference.Value.Column, reference.Value.Length));
                            }

                            foreach (var reference in def.References) {
                                yield return new AnalysisVariable(VariableType.Reference, new LocationInfo(reference.Key, reference.Value.Line, reference.Value.Column, reference.Value.Length));
                            }
                        }
                    }
                }
            }
        }

        private static bool IncludeScope(List<InterpreterScope> scopes, int i, int lineNo) {
            if(scopes[i].VisibleToChildren || i == scopes.Count - 1) {
                return true;
            }

            // if we're on the 1st line of a function include our class def as well
            if (i == scopes.Count - 2 && scopes[scopes.Count - 1] is FunctionScope) {
                var funcScope = (FunctionScope)scopes[scopes.Count - 1];
                if (lineNo == funcScope.Function.FunctionDefinition.Start.Line) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Evaluates a given expression and returns a list of members which exist in the expression.
        /// </summary>
        public IEnumerable<MemberResult> GetMembers(string exprText, int lineNumber, bool intersectMultipleResults = true) {
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


        /// <summary>
        /// Gets information about the available signatures for the given expression.
        /// </summary>
        /// <param name="exprText">The expression to get signatures for.</param>
        /// <param name="lineNumber">The line number to use for the context of looking up members.</param>
        public IEnumerable<IOverloadResult> GetSignatures(string exprText, int lineNumber) {
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
        /// Gets the available names at the given location.  This includes built-in variables, global variables, and locals.
        /// </summary>
        /// <param name="lineNumber">The line number where the available mebmers should be looked up.</param>
        public IEnumerable<MemberResult> GetAllAvailableMembers(int lineNumber) {
            var result = new Dictionary<string, List<Namespace>>();
            
            // collect builtins
            foreach (var variable in ProjectState.BuiltinModule.VariableDict) {
                result[variable.Key] = new List<Namespace>(variable.Value);
            }

            // collect variables from user defined scopes
            foreach (var scope in FindScopes(lineNumber)) {
                foreach (var kvp in scope.Variables) {
                    result[kvp.Key] = new List<Namespace>(kvp.Value.Types);
                }
            }
            return MemberDictToResultList(result);
        }

        #endregion

        /// <summary>
        /// TODO: This should go away, it's only used for tests.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        internal IEnumerable<PythonType> GetTypesFromName(string name, int lineNumber) {
            var chain = FindScopes(lineNumber);
            var result = new HashSet<PythonType>();
            foreach (var scope in chain) {
                if (scope.VisibleToChildren || scope == chain[chain.Count - 1]) {
                    VariableDef v;
                    if (scope.Variables.TryGetValue(name, out v)) {
                        foreach (var ns in v.Types) {
                            // add the clr type
                            // TODO: handle null?
                            if (ns != null && ns.PythonType != null) {
                                result.Add(ns.PythonType);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a list of valid names available at the given position in the analyzed source code minus the builtin variables.
        /// 
        /// TODO: This should go away, it's only used for tests.
        /// </summary>
        /// <param name="lineNumber">The line number where the available mebmers should be looked up.</param>
        /// <returns></returns>
        internal IEnumerable<string> GetVariablesNoBuiltins(int lineNumber) {
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
        /// TODO: This method should go away, it's only being used for tests, and the tests should be using GetMembersFromExpression
        /// which may need to be cleaned up.
        /// </summary>
        internal IEnumerable<string> GetMembersFromName(string name, int lineNumber) {
            var lookup = GetVariablesForExpression(GetExpressionFromText(name), lineNumber);
            return GetMemberResults(lookup).Select(m => m.Name);
        }

        /// <summary>
        /// Gets the top-level scope for the module.
        /// </summary>
        internal ModuleInfo GlobalScope {
            get {
                var result = (Scopes[0] as ModuleScope);
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
            var chain = new List<InterpreterScope> { Scopes[0] };

            while (curScope != prevScope) {
                prevScope = curScope;

                // TODO: Binary search?
                // We currently search backwards because the end positions are sometimes unreliable
                // and go onto the next line overlapping w/ the previous definition.  Therefore searching backwards always 
                // hits the valid method first matching on Start.  For example:
                // def f():  # Starts on 1, ends on 3
                //     pass
                // def g():  # starts on 3, ends on 4
                //     pass
                for (int i = curScope.Children.Count - 1; i >= 0; i--) {
                    var scope = curScope.Children[i];
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
