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
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis.Interpreter {
    internal class DDG : PythonWalker {
        private AnalysisUnit _unit;
        private ExpressionEvaluator _eval;

        public void Analyze(Queue<AnalysisUnit>queue) {
            while (queue.Count > 0) {
                _unit = queue.Dequeue();
                _unit.IsInQueue = false;

                 _eval = new ExpressionEvaluator(_unit);
                _unit.Ast.Walk(this);
            }
        }

        public InterpreterScope[] Scopes {
            get { return _unit.Scopes; }
        }

        public ModuleInfo GlobalScope {
            get { return _unit.DeclaringModule; }
        }

        public ProjectState ProjectState {
            get { return _unit.ProjectState; }
        }

        /// <summary>
        /// Gets the function which we are processing code for currently or
        /// null if we are not inside of a function body.
        /// </summary>
        public FunctionScope CurrentFunction {
            get { return CurrentContainer<FunctionScope>(); }
        }

        public ClassScope CurrentClass {
            get { return CurrentContainer<ClassScope>(); }
        }

        private T CurrentContainer<T>() where T : InterpreterScope {
            for (int i = Scopes.Length - 1; i >= 0; i--) {
                T result = Scopes[i] as T;
                if (result != null) {
                    return result;
                }
            }
            return null;
        }

        public T LookupDefinition<T>(string name) where T : Namespace {
            var defined = _eval.LookupNamespaceByName(name);
            if (defined != null) {
                foreach (var definition in defined) {
                    T result = definition as T;
                    if (result != null) {
                        return result;
                    }
                }
            }
            return null;
        }

        private void AssignTo(Node assignStmt, Expression left, ISet<Namespace> values) {
            if (values.Count != 0) {
                if (left is NameExpression) {
                    var l = (NameExpression)left;
                    var vars = _eval.LookupVariablesByName(l.Name, assignStmt);
                    if (vars == null) {
                        Scopes[Scopes.Length - 1].SetVariable(l.Name, values, _unit);
                    } else {
                        vars.AddTypes(values, _unit);
                    }
                } else if (left is MemberExpression) {
                    var l = (MemberExpression)left;
                    foreach (var obj in _eval.Evaluate(l.Target)) {
                        obj.SetMember(assignStmt, _unit, l.Name, values);
                    }
                } else if (left is IndexExpression) {
                    var l = (IndexExpression)left;
                    var indexObj = _eval.Evaluate(l.Index);
                    foreach (var obj in _eval.Evaluate(l.Target)) {
                        obj.SetIndex(assignStmt, _unit, indexObj, values);
                    }
                } else if (left is SequenceExpression) {
                    // list/tuple
                    var l = (SequenceExpression)left;
                    foreach (var value in values.ToArray()) {
                        for (int i = 0; i < l.Items.Count; i++) {
                            AssignTo(assignStmt, l.Items[i], value.GetIndex(assignStmt, _unit, ProjectState.GetConstant(i)));
                        }
                    }
                }
            }
        }

        public override bool Walk(AssignmentStatement node) {
            var valueType = _eval.Evaluate(node.Right);
            foreach (var left in node.Left) {
                AssignTo(node, left, valueType);
            }
            return false;
        }

        public override bool Walk(AugmentedAssignStatement node) {
            foreach (var x in _eval.Evaluate(node.Left)) {
                x.AugmentAssign(node, _unit, _eval.Evaluate(node.Right));
            }
            return false;
        }

        // TODO: Walk BinaryExpression

        public override bool Walk(ClassDefinition node) {
            if (node.Body == null || node.Name == null) {
                // invalid class body
                return false;
            }

            var newScope = LookupDefinition<ClassInfo>(node.Name);
            if (newScope == null) {
                // We failed to find the value, this occurs when there are multiple
                // definitions by the same name.  For example:
                // class f:
                //   def g(): pass
                // def f(): pass
                //
                // the 2nd f() will replace the first and we can't find g.
                return false;
            }
            
            newScope.Bases.Clear();
            // Process base classes
            foreach (var baseClass in node.Bases) {
                baseClass.Walk(this);
                var bases = _eval.Evaluate(baseClass);
                foreach (var ns in bases) {
                    ns.AddReference(node, _unit);
                }
                newScope.Bases.Add(bases);
            }

            WalkBody(node.Body, newScope._analysisUnit);

            return false;
        }

        public AnalysisUnit PushScope(AnalysisUnit unit) {
            var oldUnit = _unit;
            _unit = unit;
            return oldUnit;
        }

        public void PopScope(AnalysisUnit unit) {
            _unit = unit;
        }

        public override bool Walk(ExpressionStatement node) {
            _eval.Evaluate(node.Expression);
            return false;
        }

        public override bool Walk(ForStatement node) {
            foreach (var listType in _eval.Evaluate(node.List).ToArray()) {
                AssignTo(node, node.Left, listType.GetEnumeratorTypes(node, _unit));
            }
            return true;
        }

        private void WalkFromImportWorker(FromImportStatement node, Namespace userMod, object[] mods, string impName, string newName) {
            var saveName = (newName == null) ? impName : newName;
            GlobalScope.Imports[node].Types.Add(new[] { impName, newName });

            // look for builtin / user-defined modules first
            ModuleInfo module = userMod as ModuleInfo;
            if (module != null) { 
                var modVal = module.Scope.CreateVariable(impName, _unit);
                modVal.AddDependency(_unit);

                Scopes[Scopes.Length - 1].SetVariable(saveName, modVal.Types, _unit);
                return;
            }

            BuiltinModule builtinModule = userMod as BuiltinModule;
            if (builtinModule != null) {
                var modVal = builtinModule.GetMember(node, _unit, impName);

                Scopes[Scopes.Length - 1].CreateVariable(saveName, _unit).AddTypes(modVal, _unit);
                return;
            }

            // then look for .NET reflected namespace
            if (mods == null || mods.Length == 0) {
                return;
            }

            var mems = new List<object>(mods.Length);
            foreach (var mod in mods) {
                object val;
                if (ProjectState.TryGetMember(mod, impName, out val) && val != null) {
                    mems.Add(val);
                }
            }

            if (mems.Count > 0) {
                GlobalScope.ShowClr = true;
                var ns = ProjectState.GetNamespaceFromObjects(mems);
                ns.AddReference(node, _unit);
                Scopes[Scopes.Length - 1].SetVariable(saveName, ns.SelfSet, _unit);
            }
        }

        public override bool Walk(FromImportStatement node) {
            var mods = ProjectState.GetReflectedNamespaces(node.Root.Names, true);
            ModuleReference moduleRef;
            Namespace userMod = null;
            var modName = node.Root.MakeString();
            if (ProjectState.Modules.TryGetValue(modName, out moduleRef)) {
                userMod = moduleRef.Module;
                if (userMod == null) {
                    moduleRef.References.Add(_unit);
                }
            } else {
                moduleRef = ProjectState.Modules[modName] = new ModuleReference();
                if (moduleRef.References == null) {
                    moduleRef.References = new HashSet<AnalysisUnit>();
                }
                moduleRef.References.Add(_unit);
            }

            var asNames = node.AsNames ?? node.Names;
            var impInfo = new ImportInfo(node.Root.MakeString(), node.Span);
            GlobalScope.Imports[node] = impInfo;

            int len = Math.Min(node.Names.Count, asNames.Count);
            for (int i = 0; i < len; i++) {
                var impName = node.Names[i];
                var newName = asNames[i];

                if (impName == null) {
                    // incomplete import statement
                    continue;
                } else if (impName == "*") {
                    // Handle "import *"
                    if (userMod != null) {
                        foreach (var varName in GetModuleKeys(userMod)) {
                            WalkFromImportWorker(node, userMod, mods, varName, null);
                        }
                    }
                    if (mods != null) {
                        foreach (var mod in mods) {
                            foreach (var name in Utils.DirHelper(mod, true)) {
                                WalkFromImportWorker(node, null, mods, name, null);
                            }
                        }
                    }
                } else {
                    if (userMod != null) {
                        WalkFromImportWorker(node, userMod, mods, impName, newName);
                    }

                    if (mods != null) {
                        foreach (var mod in mods) {
                            WalkFromImportWorker(node, userMod, mods, impName, newName);
                        }
                    }
                }
            }

            return true;
        }

        private ICollection<string> GetModuleKeys(Namespace userMod) {
            ModuleInfo mi = userMod as ModuleInfo;
            if (mi != null) {
                return mi.Scope.Variables.Keys;
            }

            BuiltinModule bmi = userMod as BuiltinModule;
            if (bmi != null) {
                return bmi.VariableDict.Keys;
            }

            return new string[0];
        }

        private List<Namespace> LookupBaseMethods(string name, IEnumerable<ISet<Namespace>> bases, Node node, AnalysisUnit unit) {
            var result = new List<Namespace>();
            foreach (var b in bases) {
                foreach (var curType in b) {
                    BuiltinClassInfo klass = curType as BuiltinClassInfo;
                    if (klass != null) {
                        var value = klass.GetMember(node, unit, "name"); // curType.GetVariable(name);
                        if (value != null) {
                            result.AddRange(value);
                        }
                    }
                }
            }
            return result;
        }

        private void PropagateBaseParams(Namespace newScope, Namespace method) {
            foreach (var overload in method.Overloads) {
                var p = overload.Parameters;
                if (p.Length == newScope.ParameterTypes.Length) {
                    for (int i = 1; i < p.Length; i++) {
                        var baseParam = p[i];
                        var newParam = newScope.ParameterTypes[i];
                        // TODO: baseParam.Type isn't right, it's a string, not a type object
                        var baseType = ProjectState.GetNamespaceFromObjects(baseParam.Type);
                        if (baseType != null) {
                            newParam.Types.Add(baseType);
                        }
                    }
                }
            }
        }

        private void ProcessFunctionDecorators(FunctionDefinition funcdef, FunctionInfo newScope) {
            if (funcdef.Decorators != null) {
                foreach (var d in funcdef.Decorators) {
                    var decorator = _eval.Evaluate(d);

                    if (decorator.Contains(ProjectState._propertyObj)) {
                        newScope.IsProperty = true;
                    } else if (decorator.Contains(ProjectState._staticmethodObj)) {
                        newScope.IsStatic = true;
                    } else if (decorator.Contains(ProjectState._classmethodObj)) {
                        newScope.IsClassMethod = true;
                    }
                }
            }
            
            if (newScope.IsClassMethod) {
                if (newScope.ParameterTypes.Length > 0) {
                    newScope.ParameterTypes[0].AddTypes(ProjectState._typeObj.SelfSet, _unit);
                }
            } else if (!newScope.IsStatic) {
                // self is always an instance of the class
                // TODO: Check for __new__ (auto static) and
                // @staticmethod and @classmethod and @property
                InstanceInfo selfInst = null;
                for (int i = Scopes.Length - 1; i >= 0; i--) {
                    if (Scopes[i] is ClassScope) {
                        selfInst = ((ClassScope)Scopes[i]).Class.InstanceInfo;
                        break; 
                    }
                }
                if (selfInst != null && newScope.ParameterTypes.Length > 0) {
                    newScope.ParameterTypes[0].AddTypes(selfInst.SelfSet, _unit);
                }
            }
        }

        public override bool Walk(FunctionDefinition node) {
            if (node.Body == null) {
                // invalid function body
                return false;
            }

            var newScope = this._unit.DeclaringModule.NodeVariables[node].First() as FunctionInfo;
            Debug.Assert(newScope != null);

            // TODO: __new__ in class should assign returnValue

            var curClass = CurrentClass;
            if (curClass != null) {
                // wire up information about the class
                // TODO: Should follow MRO
                var bases = LookupBaseMethods(node.Name, curClass.Class.Bases, node, _unit);
                foreach (var ns in bases) {
                    if (ns is BuiltinMethodInfo) {
                        PropagateBaseParams(newScope, ns);
                    }
                }
            }
            ProcessFunctionDecorators(node, newScope);
            
            // process parameters
            int len = Math.Min(node.Parameters.Count, newScope.ParameterTypes.Length);
            for (int i = 0; i < len; i++) {
                var p = node.Parameters[i];
                var v = newScope.ParameterTypes[i];
                if (p.DefaultValue != null) {
                    var val = _eval.Evaluate(p.DefaultValue);
                    if (val != null) {
                        v.AddTypes(val, _unit);
                    }
                }
            }
            
            // process body
            WalkBody(node.Body, newScope._analysisUnit);

            return false;
        }

        private void WalkBody(Node node, AnalysisUnit unit) {
            var oldUnit = _unit;
            var eval = _eval;
            _unit = unit;
            _eval = new ExpressionEvaluator(unit);
            try {
                node.Walk(this);
            } finally {
                _unit = oldUnit;
                _eval = eval;
            }
        }


        public override bool Walk(IfStatement node) {
            foreach (var test in node.Tests) {
                _eval.Evaluate(test.Test);
                test.Body.Walk(this);
            }
            if (node.ElseStatement != null) {
                node.ElseStatement.Walk(this);
            }
            return true;
        }

        public override bool Walk(ImportStatement node) {
            var iinfo = new ImportInfo("", node.Span);
            GlobalScope.Imports[node] = iinfo;
            int len = Math.Min(node.Names.Count, node.AsNames.Count);
            for (int i = 0; i < len; i++) {
                var impName = node.Names[i];
                var newName = node.AsNames[i];
                var strImpName = impName.MakeString();
                iinfo.Types.Add(new[] { strImpName, newName });

                if (strImpName == "clr") {
                    GlobalScope.ShowClr = true;
                }

                var saveName = (String.IsNullOrEmpty(newName)) ? strImpName : newName;
                ModuleReference modRef;

                if (ProjectState.Modules.TryGetValue(strImpName, out modRef)) {
                    if (modRef.Module != null) {
                        ModuleInfo mi = modRef.Module as ModuleInfo;
                        if (mi != null) {
                            mi.ModuleDefinition.AddDependency(_unit);
                        }
                        Scopes[Scopes.Length - 1].SetVariable(saveName, modRef.Module.SelfSet, _unit);
                        continue;
                    } else {
                        modRef.References.Add(_unit);
                    }
                } else {
                    ProjectState.Modules[strImpName] = modRef = new ModuleReference();
                    if (modRef.References == null) {
                        modRef.References = new HashSet<AnalysisUnit>();
                    }
                    modRef.References.Add(_unit);
                }

                var builtinRefs = ProjectState.GetReflectedNamespaces(impName.Names, impName.Names.Count > 1 && !String.IsNullOrEmpty(newName));
                if (builtinRefs != null && builtinRefs.Length > 0) {
                    GlobalScope.ShowClr = true;
                    var ns = ProjectState.GetNamespaceFromObjects(builtinRefs);

                    // TODO: Should we pony up a fake module for the module we failed to resolve?
                    if (ns != null) {
                        Scopes[Scopes.Length - 1].SetVariable(saveName, ns.SelfSet, _unit);
                    }
                } else {

                }
            }
            return true;
        }

        public override bool Walk(ReturnStatement node) {
            var curFunc = CurrentFunction;
            if (node.Expression != null && curFunc != null) {
                var lookupRes = _eval.Evaluate(node.Expression);

                curFunc.Function.ReturnValue.AddTypes(lookupRes, _unit);
            }
            return true;
        }

        public override bool Walk(WithStatement node) {
            var ctxMgr = _eval.Evaluate(node.ContextManager);
            if (node.Variable != null) {
                AssignTo(node, node.Variable, ctxMgr);
            }
            return true;
        }
    }
}
