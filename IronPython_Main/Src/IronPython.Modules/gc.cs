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
using System.Runtime.InteropServices;

using Microsoft.Scripting;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

[assembly: PythonModule("gc", typeof(IronPython.Modules.PythonGC))]
namespace IronPython.Modules {
    public static class PythonGC {
        public static object gc = DynamicHelpers.GetPythonTypeFromType(typeof(PythonGC));
        public const int DEBUG_STATS = 1;
        public const int DEBUG_COLLECTABLE = 2;
        public const int DEBUG_UNCOLLECTABLE = 4;
        public const int DEBUG_INSTANCES = 8;
        public const int DEBUG_OBJECTS = 16;
        public const int DEBUG_SAVEALL = 32;
        public const int DEBUG_LEAK = (DEBUG_COLLECTABLE | DEBUG_UNCOLLECTABLE | DEBUG_INSTANCES | DEBUG_OBJECTS | DEBUG_SAVEALL);

        static PythonTuple thresholds = PythonTuple.MakeTuple(64 * 1024, 256 * 1024, 1024 * 1024);

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
        public static int Collect(int generation) {
            if (generation > GC.MaxGeneration || generation < 0) throw PythonOps.ValueError("invalid generation {0}", generation);

            long start = GC.GetTotalMemory(false);
            GC.Collect(generation);
            GC.WaitForPendingFinalizers();
            
            return (int)Math.Max(start - GC.GetTotalMemory(false), 0);
        }

        [PythonName("collect")]
        public static int Collect() {
            long start = GC.GetTotalMemory(false);
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            return (int)Math.Max(start - GC.GetTotalMemory(false), 0);
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
            thresholds = PythonTuple.MakeTuple(args);
        }

        [PythonName("get_threshold")]
        public static PythonTuple GetThreshold() {
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
