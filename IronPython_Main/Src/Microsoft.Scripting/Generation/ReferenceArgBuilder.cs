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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    public class ReferenceArgBuilder : SimpleArgBuilder {
        private Type _elementType;
        public ReferenceArgBuilder(int index, Type parameterType)
            : base(index, parameterType) {
            _elementType = parameterType.GetGenericArguments()[0];
        }

        public override int Priority {
            get { return 5; }
        }

        //TODO tighten up typing rules when adding generation support
        public override object Build(CodeContext context, object[] args) {
            object arg = args[Index];

            if (arg == null) {
                throw RuntimeHelpers.SimpleTypeError("expected Reference, but found null");
            }
            Type argType = arg.GetType();
            if (!argType.IsGenericType || argType.GetGenericTypeDefinition() != typeof(Reference<>)) {
                throw RuntimeHelpers.SimpleTypeError("expected Reference<>");
            }

            object value = ((IReference)arg).Value;

            if (value == null) return null;
            return context.LanguageContext.Binder.Convert(value, _elementType);
        }

        //    IReference r = (IReference)context.Binder.Convert(args[index], typeof(IReference)); // base.Build(context, args);
        //    if (r == null) throw  //clr.Reference", arg);
        //    return r.Value;
        //}

        public override void UpdateFromReturn(object callArg, object[] args) {
            ((IReference)args[Index]).Value = callArg;
        }
    }
}
