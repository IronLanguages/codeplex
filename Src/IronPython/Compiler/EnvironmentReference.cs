/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Reflection;
using System.Diagnostics;
using IronPython.Runtime;

namespace IronPython.Compiler {
    public abstract class EnvironmentReference {
        protected Type type;

        protected EnvironmentReference(Type type) {
            this.type = type;
        }

        public Type ReferenceType {
            get { return type; }
        }

        public abstract int Index { get; }

        public abstract Slot CreateSlot(Slot instance);
    }

    public sealed class IndexEnvironmentReference : EnvironmentReference {
        private int index;

        public IndexEnvironmentReference(int index, Type type)
            : base(type) {
            this.index = index;
        }

        public override int Index {
            get { return index; }
        }

        public override Slot CreateSlot(Slot instance) {
            return new IndexSlot(new FieldSlot(instance, typeof(FunctionEnvironmentNDictionary).GetField("environmentValues")), index);
        }
    }

    public sealed class FieldEnvironmentReference : EnvironmentReference {
        private FieldInfo field;

        public FieldEnvironmentReference(FieldInfo field, Type type)
            : base(type) {
            this.field = field;
        }

        public override int Index {
            get {
                object[] attrs = field.GetCustomAttributes(typeof(EnvironmentIndexAttribute), false);
                Debug.Assert(attrs.Length > 0, "No EnvironmentIndexAttribute found");
                EnvironmentIndexAttribute eia = (EnvironmentIndexAttribute)attrs[0];
                return eia.index;
            }
        }

        public override Slot CreateSlot(Slot instance) {
            return new FieldSlot(instance, field);
        }
    }

    public sealed class NamedEnvironmentReference : EnvironmentReference {
        public readonly SymbolId name;

        public NamedEnvironmentReference(SymbolId name, Type type)
            : base(type) {
            this.name = name;
        }

        public override int Index {
            get { throw new NotSupportedException("The method or operation is not supported."); }
        }

        public override Slot CreateSlot(Slot instance) {
            return new NamedFrameSlot(instance, name);
        }
    }
}
