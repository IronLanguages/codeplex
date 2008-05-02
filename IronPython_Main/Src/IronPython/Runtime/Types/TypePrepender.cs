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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Ast;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Types {
    public class TypePrepender {
        private PythonType _type;
        private PrependerState _state;

        public class PrependerState {
            public PrependerState(BuiltinFunction ctor) {
                Ctor = ctor;
            }

            public BuiltinFunction Ctor;
            public DynamicSite<BuiltinFunction, PythonType, object[], object> Site;

            internal void EnsureSite() {
                if (!Site.IsInitialized) {
                    Site.EnsureInitialized(
                        CallAction.Make(
                            DefaultContext.DefaultPythonBinder,
                            new CallSignature(
                                new ArgumentInfo(ArgumentKind.Simple),
                                new ArgumentInfo(ArgumentKind.List)
                            )
                        )
                    );
                }
            }
        }

        public TypePrepender(PythonType dt, PrependerState state) {
            _type = dt;
            _state = state;
        }

        [SpecialName]
        public object Call(CodeContext context, params object[] args) {
            // we've already boxed the args so we'll just call through a splat-site
            // which will do the unsplat for us.

            _state.EnsureSite();
            return _state.Site.Invoke(DefaultContext.Default, _state.Ctor, _type, args);
        }

        [SpecialName]
        public object Call(CodeContext context, [ParamDictionary] IAttributesCollection dict, params object[] args) {
            return PythonCalls.CallWithKeywordArgs(context, _state.Ctor, ArrayUtils.Insert((object)_type, args), dict);
        }
    }
}
