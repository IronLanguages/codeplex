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
using IronPython.Runtime.Calls;

namespace IronPython.Compiler.Generation {
    abstract class EnvironmentFactory {
        public EnvironmentReference MakeEnvironmentReference(SymbolId name) {
            return MakeEnvironmentReference(name, typeof(object));
        }
        public Slot MakeParentSlot(Slot instance) {
            return new FieldSlot(instance, typeof(FunctionEnvironmentDictionary).GetField("parent"));
        }
        public abstract Type EnvironmentType { get; }
        public abstract EnvironmentReference MakeEnvironmentReference(SymbolId name, Type type);
    }

    class IndexEnvironmentFactory : EnvironmentFactory {
        private int size;
        private int index;

        public IndexEnvironmentFactory(int size) {
            this.size = size;
        }

        public override Type EnvironmentType {
            get {
                return typeof(FunctionEnvironmentNDictionary);
            }
        }

        public override EnvironmentReference MakeEnvironmentReference(SymbolId name, Type type) {
            if (index < size) {
                return new IndexEnvironmentReference(index++, type);
            } else {
                throw new InvalidOperationException("not enough environment references available");
            }
        }
    }

    class FieldEnvironmentFactory : EnvironmentFactory {
        Type type;
        FieldInfo[] fields;
        private int index;

        public FieldEnvironmentFactory(Type type) {
            Debug.Assert(type.BaseType == typeof(FunctionEnvironmentDictionary));

            this.type = type;
            fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

#if DEBUG
            foreach (FieldInfo field in fields) {
                Debug.Assert(field.FieldType == typeof(object), "supporting only object fields");
            }
#endif
        }

        public override Type EnvironmentType {
            get { return type; }
        }

        public override EnvironmentReference MakeEnvironmentReference(SymbolId name, Type type) {
            Debug.Assert(index < fields.Length);
            return new FieldEnvironmentReference(fields[index++], type);
        }
    }


    class GlobalEnvironmentFactory : EnvironmentFactory {
        public GlobalEnvironmentFactory() {
        }

        public override Type EnvironmentType {
            get {
                return typeof(ModuleScope);
            }
        }

        public override EnvironmentReference MakeEnvironmentReference(SymbolId name, Type type) {
            return new NamedEnvironmentReference(name, type);
        }
    }
}
