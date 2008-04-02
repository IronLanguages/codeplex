/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License. A  copy of the license can be found in the License.html file at the
 * root of this distribution. If  you cannot locate the  Microsoft Public
 * License, please send an email to  dlr@microsoft.com. By using this source
 * code in any fashion, you are agreeing to be bound by the terms of the 
 * Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using System.Diagnostics;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime {
    class PerfTrack {
        public enum Categories {
            /// <summary>
            /// temporary categories for quick investigation, use a custom key if you
            /// need to track multiple items, and if you want to keep it then create 
            /// a new Categories entry & rename all your temporary entries.
            /// </summary>
            Temporary,
            Exceptions,     // exceptions thrown
            Properties,     // properties got or set
            Fields,         // fields got or set
            Methods,        // methods called through MethodBase.Invoke()...
            Compiler,       // Methods compiled via the ReflectOptimizer
            DelegateCreate, // we've created a new method for delegates
            DictInvoke      // Dictionary accesses
        }

        private static int totalEvents = 0;
        private static Dictionary<string, int> events = new Dictionary<string, int>();
        private static Dictionary<Categories, int> summaryStats = new Dictionary<Categories, int>();

        public static void PerfTest(BuiltinFunction o, int count) {
            Stopwatch s = new Stopwatch();
            s.Start();
            for (int i = 0; i < count; i++) {
                o.Call();
            }
            s.Stop();
            Console.WriteLine(s.Elapsed);
        }

        public static void DumpStats() {
            if (totalEvents == 0) return;

            // numbers from AMD Opteron 244 1.8 Ghz, 2.00GB of ram,
            // running on IronPython 1.0 Beta 4 against Whidbey RTM.
            const double CALL_TIME = 0.0000051442355;
            const double THROW_TIME = 0.000025365656;
            const double FIELD_TIME = 0.0000018080093;

            Console.WriteLine();
            Console.WriteLine("---- Performance Details ----");
            Console.WriteLine();

            foreach (KeyValuePair<string, int> kvp in events) {
                Console.WriteLine("{0} {1}", kvp.Key, kvp.Value);
            }

            Console.WriteLine();
            Console.WriteLine("---- Performance Summary ----");
            Console.WriteLine();
            double knownTimes = 0;
            foreach (KeyValuePair<Categories, int> kvp in summaryStats) {
                switch (kvp.Key) {
                    case Categories.Exceptions:
                        Console.WriteLine("Total Exception ({0}) = {1}  (throwtime = ~{2} secs)", kvp.Key, kvp.Value, kvp.Value * THROW_TIME);
                        knownTimes += kvp.Value * THROW_TIME;
                        break;
                    case Categories.Fields:
                        Console.WriteLine("Total field = {0} (time = ~{1} secs)", kvp.Value, kvp.Value * FIELD_TIME);
                        knownTimes += kvp.Value * FIELD_TIME;
                        break;
                    case Categories.Methods:
                        Console.WriteLine("Total calls = {0} (calltime = ~{1} secs)", kvp.Value, kvp.Value * CALL_TIME);
                        knownTimes += kvp.Value * CALL_TIME;
                        break;
                    //case Categories.Properties:
                    default:
                        Console.WriteLine("Total {1} = {0}", kvp.Value, kvp.Key);
                        break;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Total Known Times: {0}", knownTimes);
        }

        [Conditional("DEBUG")]
        public static void NoteEvent(Categories category, object key) {
            if (!IronPython.Compiler.Options.TrackPerformance) return;

            totalEvents++;
            lock (events) {
                string name = key.ToString();
                Exception ex = key as Exception;
                if (ex != null) name = ex.GetType().ToString();
                int v;
                if (!events.TryGetValue(name, out v)) events[name] = 1;
                else events[name] = v + 1;

                if (!summaryStats.TryGetValue(category, out v)) summaryStats[category] = 1;
                else summaryStats[category] = v + 1;
            }
        }
    }
}
