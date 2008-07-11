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

using System.Reflection.Emit;
using System.Scripting.Generation;
using CompilerServices = System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
    internal sealed partial class CompilerScope {

        private abstract class Storage {
            internal abstract ILGen IL { get; }
            internal abstract void EmitLoad();
            internal abstract void EmitAddress();
            internal abstract void EmitStore();
            internal virtual void EmitStore(Storage value) {
                value.EmitLoad();
                EmitStore();
            }
        }

        private sealed class LocalStorage : Storage {
            private readonly LocalBuilder _local;
            private readonly ILGen _gen;

            internal LocalStorage(LambdaCompiler lc, VariableExpression variable) {
                _gen = lc.IL;
                _local = lc.GetNamedLocal(variable.Type, variable.Name);
            }

            internal LocalStorage(ILGen gen, Type type) {
                _gen = gen;
                _local = gen.DeclareLocal(type);
            }

            internal override ILGen IL {
                get { return _gen; }
            }

            internal override void EmitLoad() {
                _gen.Emit(OpCodes.Ldloc, _local);
            }

            internal override void EmitStore() {
                _gen.Emit(OpCodes.Stloc, _local);
            }

            internal override void EmitAddress() {
                _gen.Emit(OpCodes.Ldloca, _local);
            }
        }

        private sealed class ArgumentStorage : Storage {
            private readonly int _argument;
            private readonly ILGen _gen;

            internal ArgumentStorage(LambdaCompiler lc, ParameterExpression p) {
                _gen = lc.IL;
                _argument = lc.GetLambdaArgument(lc.Parameters.IndexOf(p));
            }

            internal ArgumentStorage(ILGen gen, int argument) {
                _gen = gen;
                _argument = argument;
            }

            internal override ILGen IL {
                get { return _gen; }
            }

            internal override void EmitLoad() {
                _gen.EmitLoadArg(_argument);
            }

            internal override void EmitStore() {
                _gen.EmitStoreArg(_argument);
            }

            internal override void EmitAddress() {
                _gen.EmitLoadArgAddress(_argument);
            }
        }

        // TODO: Track references, emit the StrongBox(T)'s of referenced 
        // variables into IL locals at the top of the frame. This saves an
        // array index *and* a cast, for really good performance. We can also
        // do this for runtime constants.
        private sealed class ElementStorage : Storage {
            private readonly int _index;
            private readonly Type _boxType;
            private readonly Type _type;
            private readonly ILGen _gen;
            private readonly Storage _array;

            internal ElementStorage(HoistedLocals hoistedLocals, Storage array, Expression variable) {
                _gen = array.IL;
                _array = array;
                _index = hoistedLocals.Indexes[variable];
                _type = hoistedLocals.Variables[_index].Type;
                _boxType = typeof(CompilerServices.StrongBox<>).MakeGenericType(_type);
            }

            internal override ILGen IL {
                get { return _gen; }
            }

            internal override void EmitLoad() {
                _array.EmitLoad();
                _gen.EmitInt(_index);
                _gen.Emit(OpCodes.Ldelem_Ref);
                _gen.Emit(OpCodes.Castclass, _boxType);
                _gen.Emit(OpCodes.Ldfld, _boxType.GetField("Value"));
            }

            internal override void EmitStore() {
                LocalBuilder value = _gen.GetLocal(_type);
                _gen.Emit(OpCodes.Stloc, value);
                _array.EmitLoad();
                _gen.EmitInt(_index);
                _gen.Emit(OpCodes.Ldelem_Ref);
                _gen.Emit(OpCodes.Castclass, _boxType);
                _gen.Emit(OpCodes.Ldloc, value);
                _gen.FreeLocal(value);
                _gen.Emit(OpCodes.Stfld, _boxType.GetField("Value"));
            }

            internal override void EmitStore(Storage value) {
                _array.EmitLoad();
                _gen.EmitInt(_index);
                _gen.Emit(OpCodes.Ldelem_Ref);
                _gen.Emit(OpCodes.Castclass, _boxType);
                value.EmitLoad();
                _gen.Emit(OpCodes.Stfld, _boxType.GetField("Value"));
            }

            internal override void EmitAddress() {
                _array.EmitLoad();
                _gen.EmitInt(_index);
                _gen.Emit(OpCodes.Ldelem_Ref);
                _gen.Emit(OpCodes.Castclass, _boxType);
                _gen.Emit(OpCodes.Ldflda, _boxType.GetField("Value"));
            }
        }
    }
}