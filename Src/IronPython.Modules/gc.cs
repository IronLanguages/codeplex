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
using IronPython.Runtime;
using System.Text;
using System.Runtime.InteropServices;

using IronPython.Runtime.Operations;
using Microsoft.Scripting;

[assembly: PythonModule("gc", typeof(IronPython.Modules.PythonGC))]
namespace IronPython.Modules {
    [PythonType("gc")]
    public static class PythonGC {
        public static object gc = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonGC));
        public const int DEBUG_STATS = 1;
        public const int DEBUG_COLLECTABLE = 2;
        public const int DEBUG_UNCOLLECTABLE = 4;
        public const int DEBUG_INSTANCES = 8;
        public const int DEBUG_OBJECTS = 16;
        public const int DEBUG_SAVEALL = 32;
        public const int DEBUG_LEAK = (DEBUG_COLLECTABLE | DEBUG_UNCOLLECTABLE | DEBUG_INSTANCES | DEBUG_OBJECTS | DEBUG_SAVEALL);

        static Tuple thresholds = Tuple.MakeTuple(64 * 1024, 256 * 1024, 1024 * 1024);

        [PythonName("enable")]
        public static void Enable() {
        }

        [PythonName("disable")]
        public static void Disable() {
            throw PythonOps.NotImplementedError("gc.disable isn't implemented");
        }

        [PythonName("isenabled")]
        public static object IsEnabled() {
            return RuntimeHelpers.True;
        }

#if !SILVERLIGHT // GC.Collect
        [PythonName("collect")]
        public static int Collect() {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            return 0;
        }
#endif

        [PythonName("set_debug")]
        public static void SetDebug(object o) {
            throw PythonOps.NotImplementedError("gc.set_debug isn't implemented");
        }

        [PythonName("get_debug")]
        public static object GetDebug() {
            return null;
        }

        [PythonName("get_objects")]
        public static object[] GetObjects() {
            throw PythonOps.NotImplementedError("gc.get_objects isn't implemented");
        }

        [PythonName("set_threshold")]
        public static void SetThreshold(params object[] args) {
            thresholds = Tuple.MakeTuple(args);
        }

        [PythonName("get_threshold")]
        public static Tuple GetThreshold() {
            return thresholds;
        }

        [PythonName("get_referrers")]
        public static object[] get_referrers(params object[] objs) {
            throw PythonOps.NotImplementedError("gc.get_referrers isn't implemented");
        }

        [PythonName("get_referents")]
        public static object[] GetReferents(params object[] objs) {
            throw PythonOps.NotImplementedError("gc.get_referents isn't implemented");
        }


        public static List Garbage {
            [PythonName("garbage")]
            get {
                return new List();
            }
        }

    }
}