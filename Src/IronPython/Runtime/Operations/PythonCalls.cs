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

using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using DefaultContext = IronPython.Runtime.DefaultContext;

namespace IronPython.Runtime.Operations {
    public static partial class PythonCalls {
        private static readonly DynamicSite<object, object[], object> _splatSite =
            new DynamicSite<object, object[], object>(MakeSplatAction());

        private static readonly DynamicSite<object, object[], IAttributesCollection, object> _dictSplatSite =
            new DynamicSite<object, object[], IAttributesCollection, object>(MakeDictSplatAction());

        public static object Call(object func, params object[] args) {
            return _splatSite.Invoke(DefaultContext.Default, func, args);
        }

        public static object CallWithKeywordArgs(object func, object[] args, string[] names) {
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

        public static object CallWithKeywordArgs(CodeContext context, object func, object[] args, IAttributesCollection dict) {
            return _dictSplatSite.Invoke(context, func, args, dict);
        }

        internal static OldDynamicAction MakeSplatAction() {
            return OldCallAction.Make(DefaultContext.DefaultPythonBinder, new CallSignature(new ArgumentInfo(ArgumentKind.List)));
        }

        internal static OldDynamicAction MakeDictSplatAction() {
            return OldCallAction.Make(
                DefaultContext.DefaultPythonBinder,
                new CallSignature(
                    new ArgumentInfo(ArgumentKind.List),
                    new ArgumentInfo(ArgumentKind.Dictionary)
                )
            );
        }
    }
}
