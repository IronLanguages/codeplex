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
using System.Text;
using System.Diagnostics;

namespace Microsoft.Scripting {
    /// <summary>
    /// These are some generally useful helper methods for handling exceptions.
    /// </summary>
    public static class ExceptionHelpers {
        private const string prevStackTraces = "PreviousStackTraces";

        /// <summary>
        /// Updates an exception before it's getting re-thrown so
        /// we can present a reasonable stack trace to the user.
        /// </summary>
        public static Exception UpdateForRethrow(Exception rethrow) {
#if !SILVERLIGHT
            List<StackTrace> prev;

            StackTrace st = new StackTrace(rethrow, true);

            if (!TryGetAssociatedStackTraces(rethrow, out prev)) {
                prev = new List<StackTrace>();
                AssociateStackTraces(rethrow, prev);
            }

            prev.Add(st);

#endif
            return rethrow;
        }

        private static void AssociateStackTraces(Exception e, List<StackTrace> traces) {
            Utils.GetDataDictionary(e)[prevStackTraces] = traces;
        }

        private static bool TryGetAssociatedStackTraces(Exception e, out List<StackTrace> traces) {
            traces = Utils.GetDataDictionary(e)[prevStackTraces] as List<StackTrace>;
            return traces != null;
        }

        /// <summary>
        /// Returns all the stack traces associates with an exception
        /// </summary>
        public static IList<StackTrace> GetExceptionStackTraces(Exception rethrow) {
            List<StackTrace> result;
            return TryGetAssociatedStackTraces(rethrow, out result) ? result : null;
        }
    }
}
