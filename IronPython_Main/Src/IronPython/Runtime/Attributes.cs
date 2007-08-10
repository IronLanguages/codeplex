/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal class PythonHiddenFieldAttribute : Attribute {
    }

    /// <summary>
    /// PythonNameAttribute is used to decorate methods in the engine with the
    /// names of the Python function that they implement
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class PythonNameAttribute : ScriptNameAttribute {
        
        /// <summary>
        /// Creates a new PythonNameAttribute that can rename a method
        /// </summary>
        public PythonNameAttribute(string name) : base(name) {
        }

        public override ContextId Context {
            get {
                return PythonContext.Id;
            }
        }
     
    }

    /// <summary>
    /// Marks that the return value of a function might include NotImplemented.
    /// 
    /// This is added to an operator method to ensure that all necessary methods are called
    /// if one cannot guarantee that it can perform the comparison.
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue)]
    public class MaybeNotImplementedAttribute : Attribute {
    }

    /// <summary>
    /// PythonVersionAttribute is used to decorate methods in the engine with the
    /// Python version starting with which they are present
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PythonVersionAttribute : Attribute {

        private readonly Version version;

        /// <summary>
        /// Creates a new PythonVersionAttribute that specifies the minimum Python version required for
        /// a feature to be exposed.
        /// </summary>
        public PythonVersionAttribute(int major, int minor) {
            this.version = new Version(major, minor);
        }

        /// <summary>
        /// Gets the minimum version for which this feature can be applied
        /// </summary>
        public Version Version {
            get { return version; }
        }

        public int Major {
            get {
                return version.Major;
            }
        }

        public int Minor {
            get {
                return version.Minor;
            }
        }

        public static bool HasVersion25(ICustomAttributeProvider provider) {
            Debug.Assert(provider != null);
            
            object[] attribute = provider.GetCustomAttributes(typeof(PythonVersionAttribute), false);
            return attribute.Length == 1 && ((PythonVersionAttribute)attribute[0]).Version == new Version(2, 5);
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
    public sealed class PythonTypeAttribute : ScriptTypeAttribute {
        /// <summary>
        /// Provides the Python name for a type.  
        /// </summary>
        /// <param name="name"></param>
        public PythonTypeAttribute(string name) : base(name) {
        }

        /// <summary>
        /// Marks a type as impersonating another type.  The type's implementation will come from
        /// the type this is added on, but this type will logically appear to be a subclass of the
        /// impersonated type and will have its repr.
        /// </summary>
        public PythonTypeAttribute(Type impersonateType) : base(DynamicTypeOps.GetName(DynamicHelpers.GetDynamicTypeFromType(impersonateType)), impersonateType) {            
        }

        public override ExtensionNameTransformer GetTransformer(DynamicType type) {
            return new PythonExtensionTypeAttribute(type).PythonNameTransformer;
        }
    }

    /// <summary>
    /// This assembly-level attribute specifies which types in the engine represent built-in Python modules.
    /// 
    /// Members of a built-in module type should all be static as an instance is never created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class PythonModuleAttribute : Attribute {
        private readonly string name; 
        private readonly Type type;
        public static ConstructorInfo CtorInfo = typeof(PythonModuleAttribute).GetConstructor(new Type[] { typeof(string), typeof(Type) });

        /// <summary>
        /// Creates a new PythonModuleAttribute that can be used to specify a built-in module that exists
        /// within an assembly.
        /// </summary>
        /// <param name="name">The built-in module name</param>
        /// <param name="type">The type that implements the built-in module.</param>
        public PythonModuleAttribute(string name, Type type) {
            this.name = name;
            this.type = type;
        }

        /// <summary>
        /// The built-in module name
        /// </summary>
        public string Name {
            get { return name; }
        }

        /// <summary>
        /// The type that implements the built-in module
        /// </summary>
        public Type Type {
            get { return type; }
        }

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class PythonClassMethodAttribute : PythonNameAttribute {
        public PythonClassMethodAttribute(string name)
            : base(name) {
        }
    }
    
    /// <summary>
    /// This is to specify the doc string for builtin types and methods.
    /// Many common methods dont need a doc string since ReflectionUtil.GetDefaultDocumentation
    /// has many of the common doc strings.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DocumentationAttribute : Attribute {
        private readonly string doc;

        public DocumentationAttribute(string documentation) {
            doc = documentation;
        }

        public string Documentation {
            get { return doc; }
        }
    }
}
