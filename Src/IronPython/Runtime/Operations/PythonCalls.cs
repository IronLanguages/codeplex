/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using Microsoft.Scripting;

using DefaultContext = IronPython.Runtime.Calls.DefaultContext;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Operations {
    public static partial class PythonCalls {
        private static DynamicSite<object, object[], object> _splatSite = MakeSplatSite();
        private static DynamicSite<object, object[], IAttributesCollection, object> _dictSplatSite = MakeDictSplatSite();

        public static object Call(object func, params object[] args) {
            ICallableWithCodeContext icc = func as ICallableWithCodeContext;
            if (icc != null) return icc.Call(DefaultContext.Default, args);

            return _splatSite.Invoke(DefaultContext.Default, func, args);
        }

        public static object CallWithKeywordArgs(object func, object[] args, string[] names) {
            IFancyCallable ic = func as IFancyCallable;
            if (ic != null) return ic.Call(DefaultContext.Default, args, names);

            PythonDictionary dict = new PythonDictionary();
            for (int i = 0; i < names.Length; i++) {
                dict[names[i]] = args[args.Length - names.Length + i];
            }
            object[] newargs = new object[args.Length - names.Length];
            for (int i = 0; i < newargs.Length; i++) {
                newargs[i] = args[i];
            }

            return CallWithKeywordArgs(func, newargs, dict);
        }

        public static object CallWithKeywordArgs(object func, object[] args, IAttributesCollection dict) {
            return _dictSplatSite.Invoke(DefaultContext.Default, func, args, dict);
        }

        private static DynamicSite<object, object[], object> MakeSplatSite() {
            return DynamicSite<object, object[], object>.Create(CallAction.Make(new CallSignature(new ArgumentInfo(ArgumentKind.List))));
        }

        private static DynamicSite<object, object[], IAttributesCollection, object> MakeDictSplatSite() {
            return DynamicSite<object, object[], IAttributesCollection, object>.Create(
                CallAction.Make(
                    new CallSignature(
                        new ArgumentInfo(ArgumentKind.List),
                        new ArgumentInfo(ArgumentKind.Dictionary)
                    )
                )
            );
        }
    }
}
