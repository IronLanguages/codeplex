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


using Microsoft.Scripting;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using DefaultContext = IronPython.Runtime.DefaultContext;
using IronPython.Runtime.Binding;

namespace IronPython.Runtime.Operations {
    public static partial class PythonCalls {
        public static object Call(object func, params object[] args) {
            return DefaultContext.DefaultPythonContext.Call(func, args);
        }

        public static object Call(CodeContext/*!*/ context, object func, params object[] args) {
            return PythonContext.GetContext(context).Call(func, args);
        }

        public static object CallWithKeywordArgs(CodeContext/*!*/ context, object func, object[] args, string[] names) {
            PythonDictionary dict = new PythonDictionary();
            for (int i = 0; i < names.Length; i++) {
                dict[names[i]] = args[args.Length - names.Length + i];
            }
            object[] newargs = new object[args.Length - names.Length];
            for (int i = 0; i < newargs.Length; i++) {
                newargs[i] = args[i];
            }

            return CallWithKeywordArgs(context, func, newargs, dict);
        }

        public static object CallWithKeywordArgs(CodeContext context, object func, object[] args, IAttributesCollection dict) {
            return PythonContext.GetContext(context).CallWithKeywords(func, args, dict);
        }        
    }
}
