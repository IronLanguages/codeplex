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
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace Microsoft.PyAnalysis.Values {
    internal class BuiltinFieldInfo : BuiltinNamespace {
        private readonly ReflectedField _value;
        private string _doc;

        public BuiltinFieldInfo(ReflectedField value, ProjectState projectState)
            : base(new LazyDotNetDict(ClrModule.GetPythonType(value.FieldType), projectState, true)) {
            _value = value;
            _doc = null;
            _type = ClrModule.GetPythonType(value.FieldType);
        }

        public override string Description {
            get {
                return "field of type " + PythonType.Get__name__(_value.FieldType);
            }
        }

        public override bool IsBuiltin {
            get {
                return true;
            }
        }

        public override ObjectType NamespaceType {
            get {
                return ObjectType.Field;
            }
        }

        public override string Documentation {
            get {
                if (_doc == null) {
                    _doc = Utils.StripDocumentation(_value.__doc__);
                }
                return _doc;
            }
        }
    }
}
