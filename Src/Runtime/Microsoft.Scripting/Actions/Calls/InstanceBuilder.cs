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
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Ast = Microsoft.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    public class InstanceBuilder {
        // Index of actual argument expression or -1 if the instance is null.
        private readonly int _index;

        public InstanceBuilder(int index) {
            Debug.Assert(index >= -1);
            _index = index;
        }

        public bool HasValue {
            get { return _index != -1; }
        }

        internal protected virtual Expression ToExpression(ref MethodInfo method, OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed) {
            if (_index == -1) {
                return AstUtils.Constant(null);
            }

            Debug.Assert(hasBeenUsed.Length == args.Length);
            Debug.Assert(_index < args.Length);
            Debug.Assert(!hasBeenUsed[_index]);
            hasBeenUsed[_index] = true;

            GetCallableMethod(args, ref method);
            return resolver.Convert(args.GetObject(_index), args.GetType(_index), null, method.DeclaringType);
        }

        internal protected virtual Func<object[], object> ToDelegate(ref MethodInfo method, OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed) {
            if (_index == -1) {
                return (_) => null;
            }

            GetCallableMethod(args, ref method);

            Func<object[], object> conv = resolver.GetConvertor(_index + 1, args.GetObject(_index), null, method.DeclaringType);
            if (conv != null) {
                return conv;
            }

            return (Func<object[], object>)Delegate.CreateDelegate(
                typeof(Func<object[], object>),
                _index + 1,
                typeof(ArgBuilder).GetMethod("ArgumentRead"));
        }

        private void GetCallableMethod(RestrictedArguments args, ref MethodInfo method) {
            // If we have a non-visible method see if we can find a better method which
            // will call the same thing but is visible. If this fails we still bind anyway - it's
            // the callers responsibility to filter out non-visible methods.
            //
            // We use limit type of the meta instance so that we can access methods inherited to that type 
            // as accessible via an interface implemented by the type. The type might be internal and the methods 
            // might not be accessible otherwise.
            method = CompilerHelpers.TryGetCallableMethod(args.GetObject(_index).LimitType, method);
        }
    }
}
