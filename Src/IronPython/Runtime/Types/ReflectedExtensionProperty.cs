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

using System; using Microsoft;
using System.Reflection;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Represents a ReflectedProperty created for an extension method.  Logically the property is an
    /// instance property but the method implementing it is static.
    /// </summary>
    public class ReflectedExtensionProperty : ReflectedGetterSetter {
        private readonly MethodInfo _deleter;
        private readonly ExtensionPropertyInfo/*!*/ _extInfo;

        public ReflectedExtensionProperty(ExtensionPropertyInfo info, NameType nt)
            : base(new MethodInfo[] { info.Getter }, new MethodInfo[] { info.Setter }, nt) {
            _extInfo = info;
            _deleter = info.Deleter;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            if (Getter.Length == 0 || instance == null) {
                value = null;
                return false;
            }

            value = CallGetter(context, null, instance, ArrayUtils.EmptyObjects);
            return true;
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            if (Setter.Length == 0 || instance == null) return false;

            return CallSetter(context, null, instance, ArrayUtils.EmptyObjects, value);
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            if (_deleter == null || instance == null) {
                return base.TryDeleteValue(context, instance, owner);
            }

            CallTarget(context, null, new MethodInfo[] { _deleter }, instance);
            return true;
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return true;
        }

        public void __set__(CodeContext context, object instance, object value) {
            if (!TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value)) {
                throw PythonOps.TypeError("readonly attribute");
            }
        }

        internal override Type DeclaringType {
            get {
                return _extInfo.DeclaringType;
            }
        }

        internal ExtensionPropertyInfo ExtInfo {
            get {
                return _extInfo;
            }
        }

        public override string __name__ {
            get {
                return _extInfo.Name;
            }
        }

        public string __doc__ {
            get {
                return DocBuilder.DocOneInfo(ExtInfo);
            }
        }
    }
}
