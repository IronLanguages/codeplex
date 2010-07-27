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

using System.Collections.Generic;
using System.Text;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.PyAnalysis.Interpreter;
using System;

namespace Microsoft.PyAnalysis.Values {
    internal class FunctionInfo : UserDefinedInfo {
        private readonly ProjectEntry _entry;
        private Dictionary<Namespace, ISet<Namespace>> _methods;
        private Dictionary<string, VariableDef> _functionAttrs;
        private GeneratorInfo _generator;
        private TypedStorageLocation _returnValue;
        public bool IsStatic;
        public bool IsClassMethod;
        public bool IsProperty;
        [ThreadStatic] private static List<Namespace> _descriptionStack;

        internal FunctionInfo(AnalysisUnit unit, ProjectEntry entry)
            : base(unit) {
            _entry = entry;
            _returnValue = new TypedStorageLocation();
            // TODO: pass NoneInfo if we can't determine the function always returns
        }

        public ProjectEntry ProjectEntry {
            get {
                return _entry;
            }
        }

        public override ISet<Namespace> Call(Node node, AnalysisUnit unit, ISet<Namespace>[] args, string[] keywordArgNames) {            
            if (unit != null) {
                AddCall(node, keywordArgNames, unit, args);
            }

            if (_generator != null) {
                return _generator.SelfSet;
            }

            return ReturnValue.Types.ToSet();
        }

        private void AddCall(Node node, string[] keywordArgNames, AnalysisUnit unit, ISet<Namespace>[] args) {
            ReturnValue.AddDependency(unit);

            if (ParameterTypes != null) {
                bool added = false;

                // TODO: Warn when a keyword argument is provided and it maps to
                // something which is also a positional argument:
                // def f(a, b, c):
                //    print a, b, c
                //
                // f(2, 3, a=42)

                if (PropagateCall(node, keywordArgNames, unit, args, added)) {
                    // new inputs to the function, it needs to be analyzed.
                    _analysisUnit.Enqueue();
                }
            }
        }

        private bool PropagateCall(Node node, string[] keywordArgNames, AnalysisUnit unit, ISet<Namespace>[] args, bool added) {
            for (int i = 0; i < args.Length; i++) {
                int kwIndex = i - (args.Length - keywordArgNames.Length);
                if (kwIndex >= 0) {
                    string curArg = keywordArgNames[kwIndex];
                    switch (curArg) {
                        case "*":
                            int lastPos = args.Length - keywordArgNames.Length;
                            foreach (var type in args[i]) {
                                int? argLen = type.GetLength();
                                if (argLen != null) {
                                    for (int j = 0; j < argLen.Value; j++) {
                                        var indexType = type.GetIndex(node, unit, _analysisUnit.ProjectState.GetConstant(j));

                                        int paramIndex = lastPos + j;
                                        if (paramIndex >= ParameterTypes.Length) {
                                            break;
                                        } else if (ParameterTypes[lastPos + j].AddTypes(FunctionDefinition.Parameters[lastPos + j], unit, indexType, addReference: false)) {
                                            added = true;
                                        }
                                    }
                                }
                            }
                            break;
                        case "**":
                            // TODO: Handle keyword argument splatting
                            break;
                        default:
                            for (int j = 0; j < ParameterTypes.Length; j++) {
                                string paramName = GetParameterName(j);
                                if (paramName == curArg) {
                                    if (ParameterTypes[j].AddTypes(FunctionDefinition.Parameters[j], unit, args[i], addReference: false)) {
                                        added = true;
                                        break;
                                    }
                                }
                            }
                            // TODO: Report a warning if we don't find the keyword argument and we don't 
                            // have a ** parameter.
                            break;
                    }
                } else if (i < ParameterTypes.Length) {
                    // positional argument
                    if (ParameterTypes[i].AddTypes(FunctionDefinition.Parameters[i], unit, args[i], addReference: false)) {
                        added = true;
                    }

                } // else we should warn too many arguments
            }
            return added;
        }

        public override string Description {
            get {
                StringBuilder result = new StringBuilder("def ");
                result.Append(FunctionDefinition.Name);
                result.Append("(...)"); // TOOD: Include parameter information?
                if (!String.IsNullOrEmpty(Documentation)) {
                    result.AppendLine();
                    result.Append(Documentation);
                }
                return result.ToString();
            }
        }

        public string FunctionDescription {
            get {
                StringBuilder result = new StringBuilder();
                bool first = true;
                foreach (var ns in ReturnValue.Types) {
                    if (ns == null || ns.Description == null) {
                        continue;
                    }

                    if (first) {
                        result.Append(" -> ");
                        first = false;
                    } else {
                        result.Append(", ");
                    }
                    AppendDescription(result, ns);
                }
                //result.Append(GetDependencyDisplay());
                return result.ToString();
            }
        }

        private static void AppendDescription(StringBuilder result, Namespace key) {
            if (DescriptionStack.Contains(key)) {
                result.Append("...");
            } else {
                DescriptionStack.Add(key);
                try {
                    result.Append(key.Description);
                } finally {
                    DescriptionStack.Pop();
                }
            }
        }

        private static List<Namespace> DescriptionStack {
            get {
                if (_descriptionStack == null) {
                    _descriptionStack = new List<Namespace>();
                }
                return _descriptionStack;
            }
        }

        public FunctionDefinition FunctionDefinition {
            get {
                return (_analysisUnit.Ast as FunctionDefinition);
            }
        }

        public override ISet<Namespace> GetDescriptor(Namespace instance, AnalysisUnit unit) {
            if (instance == null || IsStatic) {
                return SelfSet;
            }
            if (IsProperty) {
                return ReturnValue.Types.ToSet();
            }

            if (_methods == null) {
                _methods = new Dictionary<Namespace, ISet<Namespace>>();
            }

            ISet<Namespace> result;
            if (!_methods.TryGetValue(instance, out result) || result == null) {
                _methods[instance] = result = new MethodInfo(this, instance).SelfSet;
            }
            return result;
        }

        public override string Documentation {
            get {
                if (FunctionDefinition.Body != null) {
                    return FunctionDefinition.Body.Documentation;
                }
                return "";
            }
        }

        public override ResultType ResultType {
            get {
                return IsProperty ? ResultType.Property : ResultType.Function;
            }
        }

        public override string ToString() {
            return "FunctionInfo" /* + hex(id(this)) */ + " " + FunctionDefinition.Name;
        }

        public override LocationInfo Location {
            get {
                return new LocationInfo(
                    _entry,
                    FunctionDefinition.Start.Line, 
                    FunctionDefinition.Start.Column,
                    FunctionDefinition.Header.Index - FunctionDefinition.Start.Index);
            }
        }

        public override ICollection<OverloadResult> Overloads {
            get {
                var parameters = new ParameterResult[FunctionDefinition.Parameters.Count];
                for (int i = 0; i < FunctionDefinition.Parameters.Count; i++) {
                    var curParam = FunctionDefinition.Parameters[i];
                    var newParam = MakeParameterResult(_entry.ProjectState, curParam);
                    parameters[i] = newParam;
                }

                return new OverloadResult[] {
                    new SimpleOverloadResult(parameters, FunctionDefinition.Name, Documentation)
                };
            }
        }

        internal static ParameterResult MakeParameterResult(ProjectState state, Parameter curParam) {
            string name = curParam.Name;
            if (curParam.IsDictionary) {
                name = "**" + name;
            } else if (curParam.IsList) {
                name = "*" + curParam.Name;
            }

            if (curParam.DefaultValue != null) {
                // TODO: Support all possible expressions for default values, we should
                // probably have a PythonAst walker for expressions or we should add ToCodeString()
                // onto Python ASTs so they can round trip
                ConstantExpression defaultValue = curParam.DefaultValue as ConstantExpression;
                if (defaultValue != null) {
                    name = name + " = " + PythonOps.Repr(state.CodeContext, defaultValue.Value);
                }

                NameExpression nameExpr = curParam.DefaultValue as NameExpression;
                if (nameExpr != null) {
                    name = name + " = " + nameExpr.Name;
                }

                DictionaryExpression dict = curParam.DefaultValue as DictionaryExpression;
                if (dict != null) {
                    if (dict.Items.Count == 0) {
                        name = name + " = {}";
                    } else {
                        name = name + " = {...}";
                    }
                }

                ListExpression list = curParam.DefaultValue as ListExpression;
                if (list != null) {
                    if (list.Items.Count == 0) {
                        name = name + " = []";
                    } else {
                        name = name + " = [...]";
                    }
                }

                TupleExpression tuple = curParam.DefaultValue as TupleExpression;
                if (tuple != null) {
                    if (tuple.Items.Count == 0) {
                        name = name + " = ()";
                    } else {
                        name = name + " = (...)";
                    }
                }
            }

            var newParam = new ParameterResult(name);
            return newParam;
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, ISet<Namespace> value) {
            if (_functionAttrs == null) {
                _functionAttrs = new Dictionary<string, VariableDef>();
            }

            VariableDef varRef;
            if (!_functionAttrs.TryGetValue(name, out varRef)) {
                _functionAttrs[name] = varRef = new VariableDef();
            }

            varRef.AddTypes(node, unit, value);
        }

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            VariableDef tmp;
            if (_functionAttrs != null && _functionAttrs.TryGetValue(name, out tmp)) {
                tmp.AddDependency(unit);
                return tmp.Types;
            }
            // TODO: Create one and add a dependency

            return _entry.ProjectState._functionType.GetMember(node, unit, name);
        }

        public override IDictionary<string, ISet<Namespace>> GetAllMembers(bool showClr) {
            if (_functionAttrs == null || _functionAttrs.Count == 0) {
                return _entry.ProjectState._functionType.GetAllMembers(showClr);
            }

            var res = new Dictionary<string, ISet<Namespace>>(_entry.ProjectState._functionType.GetAllMembers(showClr));
            foreach (var variable in _functionAttrs) {
                ISet<Namespace> existing;
                if (!res.TryGetValue(variable.Key, out existing)) {
                    res[variable.Key] = existing = new HashSet<Namespace>();
                }
                existing.UnionWith(variable.Value.Types);
            }
            return res;
        }

        private string GetParameterName(int index) {
            return FunctionDefinition.Parameters[index].Name;
        }

        public GeneratorInfo Generator {
            get {
                if (_generator == null) {
                    _generator = new GeneratorInfo(this);
                }
                return _generator;
            }
        }

        public TypedStorageLocation ReturnValue {
            get { return _returnValue; }
        }

        public ProjectState ProjectState { get { return _entry.ProjectState; } }
    }
}
