/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime {

    /// <summary>
    /// Marks a member as being hidden from Python code.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    internal class PythonHiddenAttribute : Attribute {
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
                return DefaultContext.Id;
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
    /// New version of PythonTypeAttribute.  Marks a type as being a PythonType for purposes of member lookup,
    /// creating instances, etc...  
    /// 
    /// Currently PythonSystemType's will only be treated differently for the purposes of creating instances.  These
    /// types will go through the normal __new__ / __init__ protocol instead of the .NET call the ctor protocol.
    ///
    /// They can also be used to specify a name on a type.
    /// 
    /// In the future this will also hide standard .NET methods such as Equals, GetHashCode, etc...
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, Inherited = false)]
    public sealed class PythonSystemTypeAttribute : Attribute {
        private readonly string _name;

        public PythonSystemTypeAttribute() {
        }

        public PythonSystemTypeAttribute(string name) {
            _name = name;
        }

        public string Name {
            get {
                return _name;
            }
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
        public PythonTypeAttribute(Type impersonateType) : base(PythonTypeOps.GetName(DynamicHelpers.GetPythonTypeFromType(impersonateType)), impersonateType) {            
        }

        internal override ExtensionNameTransformer GetTransformer(PythonType type) {
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
        private readonly string/*!*/ _name; 
        private readonly Type/*!*/ _type;

        /// <summary>
        /// Creates a new PythonModuleAttribute that can be used to specify a built-in module that exists
        /// within an assembly.
        /// </summary>
        /// <param name="name">The built-in module name</param>
        /// <param name="type">The type that implements the built-in module.</param>
        public PythonModuleAttribute(string/*!*/ name, Type/*!*/ type) {
            Contract.RequiresNotNull(name, "name");
            Contract.RequiresNotNull(type, "type");

            this._name = name;
            this._type = type;
        }

        /// <summary>
        /// The built-in module name
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// The type that implements the built-in module
        /// </summary>
        public Type Type {
            get { return _type; }
        }

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class PythonClassMethodAttribute : PythonNameAttribute {
        public PythonClassMethodAttribute(string name)
            : base(name) {
        }
    }
    
}
