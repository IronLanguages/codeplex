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

#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.ComDispatch {
    /// <summary>
    /// SimpleArgBuilder produces the value produced by the user as the argument value.  It
    /// also tracks information about the original parameter and is used to create extended
    /// methods for params arrays and param dictionary functions.
    /// </summary>
    internal class SimpleArgBuilder : ArgBuilder {
        private int _index;
        private Type _parameterType;

        internal SimpleArgBuilder(int index, Type parameterType) {
            _index = index;
            _parameterType = parameterType;
        }

        internal override object Build(CodeContext context, object[] args) {
            return context.LanguageContext.Binder.Convert(args[_index], _parameterType);
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters) {
            Debug.Assert(_index < parameters.Count);
            Debug.Assert(parameters[_index] != null);
            return context.ConvertExpression(parameters[_index], _parameterType);
        }

        internal int Index {
            get {
                return _index;
            }
        }

        protected override Type Type {
            get {
                return _parameterType;
            }
        }
    }
}

#endif
