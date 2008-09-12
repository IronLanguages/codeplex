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
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// This class is used to look for matching rules in the caches
    /// by executing individual rules against the site whose fallback
    /// code delegates here.
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    internal partial class Matchmaker {
        #region Generated Matchmaker

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_matchmaker from: generate_dynsites.py

        //
        // Mismatch routines for dynamic sites
        //

        // Mismatch detection - arity 0
        internal static TRet Mismatch0<TRet>(StrongBox<bool> box, CallSite site) {
            box.Value = false;
            return default(TRet);
        }

        // Mismatch detection - arity 1
        internal static TRet Mismatch1<T0, TRet>(StrongBox<bool> box, CallSite site, T0 arg0) {
            box.Value = false;
            return default(TRet);
        }

        // Mismatch detection - arity 2
        internal static TRet Mismatch2<T0, T1, TRet>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1) {
            box.Value = false;
            return default(TRet);
        }

        // Mismatch detection - arity 3
        internal static TRet Mismatch3<T0, T1, T2, TRet>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2) {
            box.Value = false;
            return default(TRet);
        }

        // Mismatch detection - arity 4
        internal static TRet Mismatch4<T0, T1, T2, T3, TRet>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            box.Value = false;
            return default(TRet);
        }

        // Mismatch detection - arity 5
        internal static TRet Mismatch5<T0, T1, T2, T3, T4, TRet>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            box.Value = false;
            return default(TRet);
        }

        // Mismatch detection - arity 6
        internal static TRet Mismatch6<T0, T1, T2, T3, T4, T5, TRet>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            box.Value = false;
            return default(TRet);
        }

        // Mismatch detection - arity 7
        internal static TRet Mismatch7<T0, T1, T2, T3, T4, T5, T6, TRet>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            box.Value = false;
            return default(TRet);
        }

        // Mismatch detection - arity 8
        internal static TRet Mismatch8<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            box.Value = false;
            return default(TRet);
        }

        // Mismatch detection - arity 9
        internal static TRet Mismatch9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
            box.Value = false;
            return default(TRet);
        }


        // *** END GENERATED CODE ***

        #endregion

        #region Generated Void Matchmaker

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_void_matchmaker from: generate_dynsites.py

        //
        // Mismatch routines for dynamic sites with void return type
        //

        // Mismatch detection - arity 1
        internal static void MismatchVoid1<T0>(StrongBox<bool> box, CallSite site, T0 arg0) {
            box.Value = false;
        }

        // Mismatch detection - arity 2
        internal static void MismatchVoid2<T0, T1>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1) {
            box.Value = false;
        }

        // Mismatch detection - arity 3
        internal static void MismatchVoid3<T0, T1, T2>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2) {
            box.Value = false;
        }

        // Mismatch detection - arity 4
        internal static void MismatchVoid4<T0, T1, T2, T3>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            box.Value = false;
        }

        // Mismatch detection - arity 5
        internal static void MismatchVoid5<T0, T1, T2, T3, T4>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            box.Value = false;
        }

        // Mismatch detection - arity 6
        internal static void MismatchVoid6<T0, T1, T2, T3, T4, T5>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            box.Value = false;
        }

        // Mismatch detection - arity 7
        internal static void MismatchVoid7<T0, T1, T2, T3, T4, T5, T6>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            box.Value = false;
        }

        // Mismatch detection - arity 8
        internal static void MismatchVoid8<T0, T1, T2, T3, T4, T5, T6, T7>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            box.Value = false;
        }

        // Mismatch detection - arity 9
        internal static void MismatchVoid9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(StrongBox<bool> box, CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
            box.Value = false;
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
