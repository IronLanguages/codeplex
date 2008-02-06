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
using System.Threading;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

[assembly: PythonExtensionType(typeof(None), typeof(NoneTypeOps))]
[assembly: PythonExtensionType(typeof(Ellipsis), typeof(EllipsisTypeOps))]
[assembly: PythonExtensionType(typeof(NotImplementedType), typeof(NotImplementedTypeOps))]
namespace IronPython.Runtime.Types {

    [PythonType("ellipsis")]
    [Documentation(null)]
    public class Ellipsis : ICodeFormattable {

        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return "Ellipsis";
        }

        #endregion
    }

    [Documentation(null)]
    public class NotImplementedType : ICodeFormattable {
        #region ICodeFormattable Members

        public string ToCodeString(CodeContext context) {
            return "NotImplemented";
        }

        #endregion
    }

    public class NoneTypeOps : EmptyTypeOps<object> {
        internal static void InitInstance() {
            InitOps(0x1e1a2e40,
                null,
                "None");
        }

        [SpecialName, PythonName("__repr__")]
        public static string ToCodeString(None self) {
            return "None";
        }
    }

    public class EllipsisTypeOps : EmptyTypeOps<Ellipsis> {

        internal static Ellipsis CreateInstance() {
            InitOps(0x1e1a6208,
                new Ellipsis(),
                "Ellipsis");

            return Instance;
        }
    }

    public class NotImplementedTypeOps : EmptyTypeOps<NotImplementedType> {

        internal static NotImplementedType CreateInstance() {
            InitOps(0x1e1a1e98,
                new NotImplementedType(),
                "NotImplemented");

            return Instance;
        }
    }

    /// <summary>
    /// Provides default functionality for empty classes.  Empty classes consist of types
    /// such as None, Ellipsis, and NotImplemented.  These types all have the same members
    /// but differ by their names, hash codes, and singleton instances.  We use a single generic
    /// type that gets instantiated against the derived ops type (FooOps : EmptyTypeOps&lt;FooOps&gt;
    /// 
    /// The derived type provided the information such as name, concrete instance type, singleton
    /// instance value, etc...  All of this gets stored in the generic type's static fields which
    /// are bound per-type instantiation, leaving us with one set of the methods that need to be
    /// declared to ensure that all the types appear essentially the same.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EmptyTypeOps<T> {
        private static string name;
        private static T instance;
        private static int hash;

        protected static void InitOps(int hashCode,
            T singletonInstance,
            string instanceName) {
            name = instanceName;
            instance = singletonInstance;
            hash = hashCode;
        }
        /*
        [StaticOpsMethod("__init__")]
        public static void InitMethod(params object[] prms) {
            // nop
        }*/

        internal static string Name {
            get {
                return name;
            }
        }

        internal static T Instance {
            get {
                return instance;
            }
        }

        internal static PythonType TypeInstance {
            get {
                return DynamicHelpers.GetPythonType(instance);
            }
        }

        internal static int HashCode {
            get {
                return hash;
            }
        }

        [SpecialName, PythonName("__hash__")]
        public static int HashMethod(object self) {
            return hash;
        }

        [SpecialName, PythonName("__repr__")]
        public static string ReprMethod(object self) {
            return Name;
        }

        [SpecialName, PythonName("__str__")]
        public static string ToString(object self) {
            return Name;
        }


        [StaticExtensionMethod("__new__")]
        public static object NewMethod(CodeContext context, object type, params object[] prms) {
            if (type == TypeInstance) {
                throw PythonOps.TypeError("cannot create instances of '{0}'", Name);
            }
            // someone is using  None.__new__ or type(None).__new__ to create
            // a new instance.  Call the type they want to create the instance for.
            return PythonOps.CallWithContext(context, type, prms);
        }        
    }
}
