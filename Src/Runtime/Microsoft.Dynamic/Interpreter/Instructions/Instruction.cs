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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    public interface IInstructionProvider {
        void AddInstructions(LightCompiler compiler);
    }

    public abstract partial class Instruction {
        public virtual int ConsumedStack { get { return 0; } }
        public virtual int ProducedStack { get { return 0; } }

        public int StackBalance {
            get { return ProducedStack - ConsumedStack; }
        }

        public abstract int Run(InterpretedFrame frame);

        public virtual string InstructionName {
            get { return GetType().Name.Replace("Instruction", ""); }
        }

        public override string ToString() {
            return InstructionName + "()";
        }

        public virtual string ToDebugString(object cookie, IList<object> objects) {
            return ToString();
        }

        public virtual object GetDebugCookie(LightCompiler compiler) {
            return null;
        }
    }

    internal sealed class NotInstruction : Instruction {
        public static readonly Instruction Instance = new NotInstruction();

        private NotInstruction() { }
        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            frame.Push((bool)frame.Pop() ? ScriptingRuntimeHelpers.False : ScriptingRuntimeHelpers.True);
            return +1;
        }
    }
}
