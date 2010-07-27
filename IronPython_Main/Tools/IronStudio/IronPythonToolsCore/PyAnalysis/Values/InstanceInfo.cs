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
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.Scripting;

namespace Microsoft.PyAnalysis.Values {
    /// <summary>
    /// Represents a class implemented in Python
    /// </summary>
    internal class InstanceInfo : Namespace, IVariableDefContainer {
        private readonly ClassInfo _classInfo;
        private Dictionary<string, VariableDef> _instanceAttrs;

        public InstanceInfo(ClassInfo classInfo) {
            _classInfo = classInfo;
        }

        public override IDictionary<string, ISet<Namespace>> GetAllMembers(bool showClr) {
            var res = new Dictionary<string, ISet<Namespace>>();
            if (_instanceAttrs != null) {
                foreach (var kvp in _instanceAttrs) {
                    var types = kvp.Value.Types;
                    var key = kvp.Key;

                    if (types.Count > 0) {
                        MergeTypes(res, key, types);
                    }
                }
            }

            foreach (var classMem in _classInfo.GetAllMembers(showClr)) {
                MergeTypes(res, classMem.Key, classMem.Value);
            }
            return res;
        }

        private static void MergeTypes(Dictionary<string, ISet<Namespace>> res, string key, IEnumerable<Namespace> types) {
            ISet<Namespace> set;
            if (!res.TryGetValue(key, out set)) {
                res[key] = set = new HashSet<Namespace>();
            }

            set.UnionWith(types);
        }

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            if (_instanceAttrs == null) {
                _instanceAttrs = new Dictionary<string, VariableDef>();
            }
            VariableDef def;
            if (!_instanceAttrs.TryGetValue(name, out def)) {
                _instanceAttrs[name] = def = new VariableDef();
            }
            def.AddReference(node, unit);

            ISet<Namespace> getattrRes = EmptySet<Namespace>.Instance;
            var getAttribute = _classInfo.GetMemberNoReference(node, unit.CopyForEval(), "__getattribute__");
            if (getAttribute.Count > 0) {
                foreach (var getAttrFunc in getAttribute) {
                    var func = getAttrFunc as BuiltinMethodInfo;
                    if (func != null && func.BuiltinFunctions.Length == 1 && func.BuiltinFunctions[0].DeclaringType == typeof(object)) {
                        continue;
                    }
                    // TODO: We should really do a get descriptor / call here
                    getattrRes = getattrRes.Union(getAttrFunc.Call(node, unit, new[] { SelfSet, _classInfo._analysisUnit.ProjectState._stringType.Instance.SelfSet }, Microsoft.Scripting.Utils.ArrayUtils.EmptyStrings));
                }
                if (getattrRes.Count > 0) {
                    return getattrRes;
                }
            }
            
            def.AddDependency(unit);

            var res = def.Types.Union(_classInfo.GetMember(node, unit, name).GetDescriptor(this, unit));
            if (res.Count == 0) {
                var getAttr = _classInfo.GetMember(node, unit, "__getattr__");
                if (getAttr.Count > 0) {
                    foreach (var getAttrFunc in getAttr) {
                        // TODO: We should really do a get descriptor / call here
                        getattrRes = getattrRes.Union(getAttrFunc.Call(node, unit, new[] { SelfSet, _classInfo._analysisUnit.ProjectState._stringType.Instance.SelfSet }, Microsoft.Scripting.Utils.ArrayUtils.EmptyStrings));
                    }
                }
                return getattrRes;
            }
            return res;
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, ISet<Namespace> value) {
            if (_instanceAttrs == null) {
                _instanceAttrs = new Dictionary<string, VariableDef>();
            }

            VariableDef instMember;
            if (!_instanceAttrs.TryGetValue(name, out instMember) || instMember == null) {
                _instanceAttrs[name] = instMember = new VariableDef();
            }
            instMember.AddAssignment(node, unit);
            instMember.AddTypes(node, unit, value, false);
        }

        public override void DeleteMember(Node node, AnalysisUnit unit, string name) {
            if (_instanceAttrs == null) {
                _instanceAttrs = new Dictionary<string, VariableDef>();
            }
            
            VariableDef instMember;
            if (!_instanceAttrs.TryGetValue(name, out instMember) || instMember == null) {
                _instanceAttrs[name] = instMember = new VariableDef();
            }

            instMember.AddReference(node, unit);
        }

        public override string Description {
            get {
                return ClassInfo.ClassDefinition.Name + " instance";
            }
        }

        public override string Documentation {
            get {
                return ClassInfo.Documentation;
            }
        }

        public override ResultType ResultType {
            get {
                return ResultType.Instance;
            }
        }

        public ClassInfo ClassInfo {
            get { return _classInfo; }
        }

        #region IVariableDefContainer Members

        public IEnumerable<VariableDef> GetDefinitions(string name) {
            VariableDef def;
            if (_instanceAttrs.TryGetValue(name, out def)) {
                yield return def;
            }
        }

        #endregion
    }
}
