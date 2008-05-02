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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Provides access to non-default .NET indexers (aka properties w/ parameters).
    /// 
    /// C# doesn't support these, but both COM and VB.NET do.  The types dictionary
    /// gets populated w/a ReflectedGetterSetter indexer which is a descriptor.  Getting
    /// the descriptor returns a bound indexer.  The bound indexer supports indexing.
    /// We support multiple indexer parameters via expandable tuples.
    /// </summary>
    [PythonSystemType("indexer#")]
    public sealed class ReflectedIndexer : ReflectedGetterSetter {
        private readonly object _instance;
        private readonly PropertyInfo/*!*/ _info;

        public ReflectedIndexer(PropertyInfo/*!*/ info, NameType nt)
            : base(new MethodInfo[] { info.GetGetMethod() }, new MethodInfo[] { info.GetSetMethod() }, nt) {
            Debug.Assert(info != null);

            _info = info;
        }

        public ReflectedIndexer(ReflectedIndexer from, object instance)
            : base(from) {
            _instance = instance;
            _info = from._info;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = new ReflectedIndexer(this, instance);
            return true;
        }

        public override Type DeclaringType {
            get { return _info.DeclaringType; }
        }

        public override string Name {
            get { return _info.Name; }
        }

        #region Public APIs

        public bool SetValue(CodeContext context, object [] keys, object value) {
            return CallSetter(context, _instance, keys, value);
        }

        public object GetValue(CodeContext context, object[] keys) {
            return CallGetter(context, _instance, keys);
        }
        
        public object __get__(object instance, object owner) {
            object val;
            TryGetValue(DefaultContext.Default, instance, owner as PythonType, out val);
            return val;
        }

        public object this[params object[] key] {
            get {
                return GetValue(DefaultContext.Default, key);
            }
            set {
                if (!SetValue(DefaultContext.Default, key, value)) {
                    throw PythonOps.AttributeErrorForReadonlyAttribute(DeclaringType.Name, SymbolTable.StringToId(Name));
                }
            }
        }

        #endregion
    }

}
