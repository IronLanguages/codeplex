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

#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Scripting;
using System.Scripting.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using ComObject = Microsoft.Scripting.Actions.ComDispatch.ComObject;

namespace IronPython.Runtime.Operations {
    public static class ComOps {
        public static string __str__(object self) {
            if (self is ComObject) {
                return __str__inner((ComObject)self);
            }

            if (ComObject.IsGenericComObject(self)) {
                return __str__inner(ComObject.ObjectToComObject(self));
            }

            return __str__inner(self);
        }

        public static string __repr__(object self) {
            if (self is ComObject) {
                return __repr__inner((ComObject)self);
            }

            if (ComObject.IsGenericComObject(self)) {
                return __repr__inner(ComObject.ObjectToComObject(self));
            }

            return __repr__inner(self);
        }

        private static string __str__inner(object self) {
            return self.ToString();
        }

        private static string __repr__inner(object self) {
            return String.Format("<{0} object at {1}>",
                self.ToString(),
                PythonOps.HexId(self));
        }

        public static IList<object> GetAttrNames(CodeContext context, object self) {
            return (IList<object>)GetMemberNames(context, self);
        }

        public static IList<object> GetMemberNames(CodeContext context, object self) {
            // resolve statically known names from .NET objects...
            Dictionary<string, string> names = new Dictionary<string, string>();
            PythonBinder.GetBinder(context).ResolveMemberNames(context, DynamicHelpers.GetPythonType(self), DynamicHelpers.GetPythonType(self), names);

            // then pcik up any names from the COM object...
            ComObject co = self as ComObject;
            if (self == null && ComObject.IsGenericComObject(self)) {
                co = ComObject.ObjectToComObject(self);
            }

            if (co != null) {
                foreach (string o in GetMemberNames_inner(context, co)) {
                    names[o] = o;
                }
            }

            List<object> res = new List<object>();
            foreach (string name in names.Keys) {
                res.Add(name);
            }
            return res;
        }

        private static IList<string> GetMemberNames_inner(CodeContext context, ComObject self) {
            List<string> res = new List<string>();
            foreach (SymbolId si in self.GetMemberNames(context)) {
                res.Add(SymbolTable.IdToString(si));
            }
            return res;
        }
    }
}

#endif