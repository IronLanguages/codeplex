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
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.PyAnalysis.Interpreter;

namespace Microsoft.PyAnalysis.Values {
    internal class SetInfo : BuiltinInstanceInfo {
        private readonly ISet<Namespace> _valueTypes;

        public SetInfo(HashSet<Namespace> valueTypes, ProjectState projectState, bool showClr)
            : base(projectState._setType) {
            _valueTypes = valueTypes;
        }

        public override string ShortDescription {
            get {
                return "set";
            }
        }

        public override string Description {
            get {
                // set({k})
                Namespace valueType = _valueTypes.GetUnionType();
                string valueName = valueType == null ? null : valueType.ShortDescription;

                if (valueName != null) {
                    return "{" +
                        (valueName ?? "unknown") +
                        "}";
                }

                return "set";
            }
        }

        public override bool IsBuiltin {
            get {
                return false;
            }
        }

        public override bool UnionEquals(Namespace ns) {
            return ns is DictionaryInfo;
        }

        public override int UnionHashCode() {
            return 2;
        }

        public override ResultType ResultType {
            get {
                return ResultType.Field;
            }
        }
    }
}
