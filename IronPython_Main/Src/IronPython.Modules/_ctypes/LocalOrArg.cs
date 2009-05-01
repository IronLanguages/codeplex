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
using System.Reflection.Emit;

#if !SILVERLIGHT
namespace IronPython.Modules {
    /// <summary>
    /// Wrapper class for emitting locals/variables during marshalling code gen.
    /// </summary>
    internal abstract class LocalOrArg {
        public abstract void Emit(ILGenerator ilgen);
        public abstract Type Type {
            get;
        }
    }

    class Local : LocalOrArg {
        private readonly LocalBuilder _local;

        public Local(LocalBuilder local) {
            _local = local;
        }

        public override void Emit(ILGenerator ilgen) {
            ilgen.Emit(OpCodes.Ldloc, _local);
        }

        public override Type Type {
            get { return _local.LocalType; }
        }
    }

    class Arg : LocalOrArg {
        private readonly int _index;
        private readonly Type _type;

        public Arg(int index, Type type) {
            _index = index;
            _type = type;
        }

        public override void Emit(ILGenerator ilgen) {
            ilgen.Emit(OpCodes.Ldarg, _index);
        }

        public override Type Type {
            get { return _type; }
        }
    }
}
#endif
