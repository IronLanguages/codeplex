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
using IronPython.Runtime;
using System.Diagnostics;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

[assembly: PythonModule("xxsubtype", typeof(IronPython.Modules.xxsubtype))]
namespace IronPython.Modules {
    /// <summary>
    /// Samples on how to subtype built-in types from C#
    /// </summary>
    public static class xxsubtype {
        [PythonType("spamlist")]
        public class SpamList : List {
            public SpamList()
                : base() {
            }

            public SpamList(object sequence)
                : base(sequence) {
            }

            /// <summary>
            /// an int variable for demonstration purposes
            /// </summary>
            public int state;

            [PythonName("getstate")]
            public int getstate() {
                return state;
            }

            [PythonName("setstate")]
            public void setstate(int value) {
                state = value;
            }
        }

        [PythonType("spamdict")]
        public class SpamDict : PythonDictionary {
            /// <summary>
            /// an int variable for demonstration purposes
            /// </summary>
            public int state;

            [PythonName("getstate")]
            public int getstate() {
                return state;
            }

            [PythonName("setstate")]
            public void setstate(int value) {
                state = value;
            }
        }
#if !SILVERLIGHT
        public static double bench(CodeContext context, object x, string name) {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 1001; i++) {
                PythonOps.GetBoundAttr(context, x, SymbolTable.StringToId(name));
            }

            sw.Stop();
            return ((double)sw.ElapsedMilliseconds)/1000.0;
        }
#endif
    }
}
