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
using System.Reflection.Emit;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    public abstract class DynamicILGen : ILGen {
        protected DynamicILGen(ILGenerator il)
            : base(il) {
        }

        public T CreateDelegate<T>() {
            MethodInfo mi;
            return CreateDelegate<T>(out mi);
        }

        // TODO: We shouldn't need the MethodInfo result...
        public abstract T CreateDelegate<T>(out MethodInfo mi);
    }

    class DynamicILGenMethod : DynamicILGen {
        private readonly DynamicMethod _dm;

        internal DynamicILGenMethod(DynamicMethod dm, ILGenerator il)
            : base(il) {
            _dm = dm;
        }

        public override T CreateDelegate<T>(out MethodInfo mi) {
            Contract.Requires(typeof(T).IsSubclassOf(typeof(Delegate)), "T");
            mi = _dm;
            return (T)(object)_dm.CreateDelegate(typeof(T), null);
        }
    }

    class DynamicILGenType : DynamicILGen {
        private readonly TypeBuilder _tb;
        private readonly MethodBuilder _mb;

        internal DynamicILGenType(TypeBuilder tb, MethodBuilder mb, ILGenerator il)
            : base(il) {
            _tb = tb;
            _mb = mb;
        }

        public override T CreateDelegate<T>(out MethodInfo mi) {
            Contract.Requires(typeof(T).IsSubclassOf(typeof(Delegate)), "T");

            Type t = _tb.CreateType();
            mi = t.GetMethod(_mb.Name);
            return (T)(object)Delegate.CreateDelegate(typeof(T), mi);
        }
    }
}
