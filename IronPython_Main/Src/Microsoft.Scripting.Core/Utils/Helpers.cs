
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
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Scripting.Utils {
    // Miscellaneous helpers that don't belong anywhere else
    internal static class Helpers {

        internal static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes) {
#if SILVERLIGHT // Module-hosted DynamicMethod is not available in SILVERLIGHT
            return new DynamicMethod(name, returnType, parameterTypes);
#else
            //
            // WARNING: we set restrictedSkipVisibility == true  (last parameter)
            //          setting this bit will allow accessing nonpublic members
            //          for more information see http://msdn.microsoft.com/en-us/library/bb348332.aspx
            //
            return new DynamicMethod(name, returnType, parameterTypes, true);
#endif
        }

        /// <summary>
        /// Creates an array of size count with each element initialized to item
        /// </summary>
        internal static T[] RepeatedArray<T>(T item, int count) {
            T[] ret = new T[count];
            for (int i = 0; i < count; i++) ret[i] = item;
            return ret;
        }

        internal static string ToValidPath(string path) {
            return ToValidPath(path, false, true);
        }

        internal static string ToValidPath(string path, bool isMask) {
            return ToValidPath(path, isMask, true);
        }

        internal static string ToValidFileName(string path) {
            return ToValidPath(path, false, false);
        }

        private static string ToValidPath(string path, bool isMask, bool isPath) {
            Debug.Assert(!isMask || isPath);

            if (String.IsNullOrEmpty(path)) {
                return "_";
            }

            StringBuilder sb = new StringBuilder(path);

            if (isPath) {
                foreach (char c in Path.GetInvalidPathChars()) {
                    sb.Replace(c, '_');
                }
            } else {
#if SILVERLIGHT
                foreach (char c in Path.GetInvalidPathChars()) {
                    sb.Replace(c, '_');
                }
                sb.Replace(':', '_').Replace('*', '_').Replace('?', '_').Replace('\\', '_').Replace('/', '_');
#else
                foreach (char c in Path.GetInvalidFileNameChars()) {
                    sb.Replace(c, '_');
                }
#endif
            }

            if (!isMask) {
                sb.Replace('*', '_').Replace('?', '_');
            }

            return sb.ToString();
        }
    }
}
