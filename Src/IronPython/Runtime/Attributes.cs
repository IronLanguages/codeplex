/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;

namespace IronPython.Runtime {

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal class PythonHiddenFieldAttribute : Attribute {
    }

    /// <summary>
    /// PythonNameAttribute is used to decorate methods in the engine with the
    /// names of the Python function that they implement
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PythonNameAttribute : Attribute {
        public readonly string name;
        public PythonNameAttribute(string name) {
            this.name = name;
        }
    }

    /// <summary>
    /// PythonVersionAttribute is used to decorate methods in the engine with the
    /// Python version starting with which they are present
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PythonVersionAttribute : Attribute {
        public readonly Version version;
        public PythonVersionAttribute(int major, int minor) {
            this.version = new Version(major, minor);
        }
    }


    /// <summary>
    /// PythonTypeAttribute is used for two purposes:
    /// 1. It can be used to specify the Python name of types in the engine which
    ///    implement built-in types. For eg. The type IronPython.Runtime.Set in the engine
    ///    represents the Python built-in "set". IronPython.Runtime.Function represents
    ///    the Python built-in "function".
    /// 2. There might be multiple types in the engine which represent the same
    ///    built-in type in Python. In such cases, the multiple types in the engine point
    ///    to a single type in the engine which represents a built-in Python type.
    ///    For eg, Function0, Function1, etc all point to IronPython.Runtime.Function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    public sealed class PythonTypeAttribute : Attribute {

        // The Python name of the type
        public readonly string name;

        // The type in the engine which implements the Python type
        public readonly Type impersonateType;

        public PythonTypeAttribute(string name) {
            this.name = name;
        }

        public PythonTypeAttribute(Type impersonateType) {
            // The impersonating type should have a name for the Python type it represents.
            System.Diagnostics.Debug.Assert(((PythonTypeAttribute)
                Attribute.GetCustomAttribute(impersonateType, typeof(PythonTypeAttribute))).name != null);

            this.impersonateType = impersonateType;
            this.name = ((PythonTypeAttribute)
                Attribute.GetCustomAttribute(impersonateType, typeof(PythonTypeAttribute))).name;
        }
    }

    /// <summary>
    /// This assembly-level attribute specifies which types in the engine represent built-in Python modules.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class PythonModuleAttribute : Attribute {
        public readonly string name; // The built-in module name
        public readonly Type type;
        public PythonModuleAttribute(string name, Type type) {
            this.name = name;
            this.type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class PythonClassMethodAttribute : PythonNameAttribute {
        public PythonClassMethodAttribute(string name)
            : base(name) {
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class StaticOpsMethodAttribute : PythonNameAttribute {
        public StaticOpsMethodAttribute(string name)
            : base(name) {
        }
    }



    /// <summary>
    /// This attribute is used to mark the parameter which is dictionary indexed by the names
    /// For eg. in this Python method,
    ///     def foo(**paramDict): print paramDict
    ///     foo(a=1, b=2)
    /// paramDict will be {"a":1, "b":2}
    /// This attribute is related to System.ParamArrayAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class ParamDictAttribute : Attribute {
        public ParamDictAttribute() {
        }
    }

    /// <summary>
    /// This is to specify the doc string for builtin types and methods.
    /// Many common methods dont need a doc string since ReflectionUtil.GetDefaultDocumentation
    /// has many of the common doc strings.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DocumentationAttribute : Attribute {
        public readonly string Value;

        public DocumentationAttribute(string documentation) {
            Value = documentation;
        }
    }
}
