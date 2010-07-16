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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.Scripting.Utils;

namespace Microsoft.PyAnalysis.Values {
    /// <summary>
    /// A VariableDef represents a collection of type information and dependencies
    /// upon that type information.  
    /// 
    /// The collection of type information is represented by a HashSet of Namespace
    /// objects.  This set includes all of the types that are known to have been
    /// seen for this variable.
    /// 
    /// Dependency data is added when an one value is assigned to a variable.  
    /// For example for the statement:
    /// 
    /// foo = value
    /// 
    /// There will be a variable refs for the name "foo", and "value" will evaluate
    /// to a collection of namespaces.  When value is assigned to
    /// foo the types in value will be propagated to foo's VariableDef by a call
    /// to AddDependentTypes.  If value adds any new type information to foo
    /// then the caller will then the caller needs to re-analyze anyone who
    /// is dependent upon foo's values.  If "value" was a VariableDef as well, rather
    /// than some arbitrary expression, then reading "value" would have made the
    /// code being analyzed dependent upon "value".  After a call to
    /// AddDependentTypes the caller needs to check the return value and if
    /// new types were added needs to re-enque it's scope.
    /// 
    /// Dependecies are stored in a dictionary keyed off of the IProjectEntry object.
    /// This is a consistent object which always represents the same module even
    /// across multiple analysis.  The object is versioned so that when we encounter
    /// a new version all the old dependencies will be thrown away when a variable ref 
    /// is updated with new dependencies.
    /// 
    /// TODO: We should store built-in types not keyed off of the ModuleInfo.
    /// </summary>
    class VariableDef {
        private Dictionary<IProjectEntry, DependencyInfo> _dependencies;

        public VariableDef() {
        }

        public bool AddTypes(IEnumerable<Namespace> newTypes, AnalysisUnit unit) {
            if (TryMigration(newTypes, unit)) {
                return false;
            }

            var dependencies = GetDependencyInfo(unit);
            var types = dependencies.Types;
#if EXTRA_DEBUG
            if (types.Count > 50) {
                Dictionary<Type, int> dist = new Dictionary<Type, int>();
                foreach (var type in types) {
                    int count;
                    if (!dist.TryGetValue(type.GetType(), out count)) {
                        dist[type.GetType()] = 1;
                    } else {
                        dist[type.GetType()] = count + 1;
                    }
                }

                List<KeyValuePair<Type, int>> list = new List<KeyValuePair<Type, int>>(dist);
                list.Sort((x, y) => x.Value - y.Value);
            }
#endif

            bool added = false;
            foreach (var ns in newTypes) {
                if (dependencies.Types.Add(ns, unit)) {
                    added = true;                    
                }
            }

            if (added) {
                EnqueueDependents();
            }
            return added;
        }

        public void ClearOldValues(IProjectEntry fromModule) {
            DependencyInfo deps;
            if (_dependencies != null) {
                if (_dependencies.TryGetValue(fromModule, out deps)) {
                    if (deps.Version != fromModule.Version) {
                        _dependencies.Remove(fromModule);
                    }
                }
            }
        }

        private Dictionary<IProjectEntry, DependencyInfo> Dependencies {
            get {
                if (_dependencies == null) {
                    _dependencies = new Dictionary<IProjectEntry, DependencyInfo>();
                }
                return _dependencies;
            }
        }

        private DependencyInfo GetDependentItems(AnalysisUnit unit) {
            var module = unit.DeclaringModule.ProjectEntry;
            DependencyInfo result;
            if (!Dependencies.TryGetValue(module, out result) || result.Version != module.Version) {
                Dependencies[module] = result = new DependencyInfo(module.Version);
            }
            return result;
        }
        
        private DependencyInfo GetDependencyInfo(AnalysisUnit unit) {
            return GetDependentItems(unit);
        }

        private bool TryMigration(IEnumerable<Namespace> newTypes, AnalysisUnit unit) {
            if (_dependencies == null) {
                return false;
            }

            var module = unit.DeclaringModule.ProjectEntry;
            DependencyInfo existing;
            if (!_dependencies.TryGetValue(module, out existing) || existing == null) {
                return false;
            }

            bool hasUserDefined = false;
            foreach (var t in existing.Types) {
                if (!t.IsBuiltin) {
                    hasUserDefined = true;
                    break;
                }
            }

            if (hasUserDefined) {
                return false;
            }

            bool allPresent = true;
            foreach (var ns in newTypes) {
                if (!existing.Types.Contains(ns)) {
                    allPresent = false;
                    break;
                }                
            }

            var dependencies = GetDependencyInfo(unit);
            dependencies.Types = existing.Types;
            return allPresent;
        }

        /// <summary>
        /// Enqueues any nodes which depend upon this type into the provided analysis queue for
        /// further analysis.
        /// </summary>
        public void EnqueueDependents() {
            if (_dependencies != null) {
                foreach (var val in _dependencies.Values) {
                    foreach (var analysisUnit in val.DependentUnits) {
                        analysisUnit.Enqueue();
                    }
                }
            }
        }

        public ISet<Namespace> Types {
            get {
                if (_dependencies != null) {
                    HashSet<Namespace> res = new HashSet<Namespace>();
                    foreach (var mod in _dependencies.Values) {
                        res.UnionWith(mod.Types);
                    }
                    return res;
                }
                return EmptySet<Namespace>.Instance;
            }
        }

        public void AddDependency(AnalysisUnit unit) {
            if (!unit.ForEval) {
                GetDependencyInfo(unit).DependentUnits.Add(unit);
            }
        }
    }
}
