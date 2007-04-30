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
using System.Collections.Generic;

namespace Microsoft.Scripting.Internal.Generation {
    public class ReturnBuilder {
        private Type _returnType;

        /// <summary>
        /// Creates a ReturnBuilder
        /// </summary>
        /// <param name="returnType">the type the ReturnBuilder will leave on the stack</param>
        public ReturnBuilder(Type returnType) { this._returnType = returnType; }

        public virtual object Build(CodeContext context, object[] args, object ret) {
            return ret;
        }

        public virtual int CountOutParams {
            get { return 0; }
        }

        public virtual bool CanGenerate {
            get { return true; }
        }

        public virtual void Generate(CodeGen cg, IList<Slot> argSlots) {
        }

        public Type ReturnType {
            get {
                return _returnType;
            }
        }
    }
}
