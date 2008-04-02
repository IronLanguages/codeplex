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

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;

namespace IronPython.Runtime.Types {
    public static class DynamicHelpers {
        public static PythonType GetPythonTypeFromType(Type type) {
            Contract.RequiresNotNull(type, "type");

            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "TypeLookup " + type.FullName);

            PythonType ret = PythonType.GetPythonType(type);
            if (ret != null) return ret;

            ret = ReflectedTypeBuilder.Build(type);

            return PythonType.SetPythonType(type, ret);
        }

        public static PythonType GetPythonType(object o) {
            IPythonObject dt = o as IPythonObject;
            if (dt != null) return dt.PythonType;
            
            return GetPythonTypeFromType(CompilerHelpers.GetType(o));
        }

        public static ReflectedEvent.BoundEvent MakeBoundEvent(ReflectedEvent eventObj, object instance, Type type) {
            return new ReflectedEvent.BoundEvent(eventObj, instance, DynamicHelpers.GetPythonTypeFromType(type));
        }
    }
}
