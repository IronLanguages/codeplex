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
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    interface ILazySlotFactory<T> {
        Slot GetConcreteSlot(LambdaCompiler cg, T data);
    }


    internal class LazySlot<T> : Slot {
        private ILazySlotFactory<T> _factory;
        private T _data;
        private Type _type;

        public LazySlot(ILazySlotFactory<T> factory, Type type, T data) {
            _factory = factory;
            _data = data;
            _type = type;
        }

        public override void EmitGet(LambdaCompiler cg) {
            _factory.GetConcreteSlot(cg, _data).EmitGet(cg);
        }

        public override void EmitGetAddr(LambdaCompiler cg) {
            _factory.GetConcreteSlot(cg, _data).EmitGetAddr(cg);
        }

        public override void EmitSet(LambdaCompiler cg) {
            _factory.GetConcreteSlot(cg, _data).EmitSet(cg);
        }

        public override void EmitSet(LambdaCompiler cg, Slot val) {
            _factory.GetConcreteSlot(cg, _data).EmitSet(cg, val);
        }

        public override Type Type {
            get { return _type; }
        }
    }
}
