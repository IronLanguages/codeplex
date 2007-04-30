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

using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Internal.Generation {
    public class SimpleArgBuilder : ArgBuilder {
        private int _index;
        private Type _parameterType;
        public SimpleArgBuilder(int index, Type parameterType) {
            this._index = index;
            this._parameterType = parameterType;
        }

        public override int Priority {
            get { return 0; }
        }

        public override object Build(CodeContext context, object[] args) {
            return context.LanguageContext.Binder.Convert(args[_index], _parameterType);
        }

        public override void Generate(CodeGen cg, IList<Slot> argSlots) {
            argSlots[_index].EmitGetAs(cg, _parameterType);
        }

        public override Expression ToExpression(ActionBinder binder, Expression[] parameters) {
            return binder.ConvertExpression(parameters[_index], _parameterType);
        }

        public int Index {
            get {
                return _index;
            }
        }

        public Type Type {
            get {
                return _parameterType;
            }
        }
    }
}
