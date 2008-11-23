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


namespace Microsoft.Scripting.Actions {
    internal static partial class UpdateDelegates {

        // Disable the 'obsolete' warning.
#pragma warning disable 618

        #region Generated Predefined Update Targets

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_update_targets from: generate_dynsites.py

        /// <summary>
        /// Site update code - arity 1
        /// </summary>
        internal static TRet Update1<T, T0, TRet>(CallSite site, T0 arg0) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0 });
        }

        /// <summary>
        /// Site update code - arity 2
        /// </summary>
        internal static TRet Update2<T, T0, T1, TRet>(CallSite site, T0 arg0, T1 arg1) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1 });
        }

        /// <summary>
        /// Site update code - arity 3
        /// </summary>
        internal static TRet Update3<T, T0, T1, T2, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2 });
        }

        /// <summary>
        /// Site update code - arity 4
        /// </summary>
        internal static TRet Update4<T, T0, T1, T2, T3, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3 });
        }

        /// <summary>
        /// Site update code - arity 5
        /// </summary>
        internal static TRet Update5<T, T0, T1, T2, T3, T4, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4 });
        }

        /// <summary>
        /// Site update code - arity 6
        /// </summary>
        internal static TRet Update6<T, T0, T1, T2, T3, T4, T5, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5 });
        }

        /// <summary>
        /// Site update code - arity 7
        /// </summary>
        internal static TRet Update7<T, T0, T1, T2, T3, T4, T5, T6, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6 });
        }

        /// <summary>
        /// Site update code - arity 8
        /// </summary>
        internal static TRet Update8<T, T0, T1, T2, T3, T4, T5, T6, T7, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
        }

        /// <summary>
        /// Site update code - arity 9
        /// </summary>
        internal static TRet Update9<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
        }

        /// <summary>
        /// Site update code - arity 10
        /// </summary>
        internal static TRet Update10<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
        }

        /// <summary>
        /// Site update code - arity 11
        /// </summary>
        internal static TRet Update11<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 });
        }

        /// <summary>
        /// Site update code - arity 12
        /// </summary>
        internal static TRet Update12<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11 });
        }

        /// <summary>
        /// Site update code - arity 13
        /// </summary>
        internal static TRet Update13<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12 });
        }

        /// <summary>
        /// Site update code - arity 14
        /// </summary>
        internal static TRet Update14<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13 });
        }

        /// <summary>
        /// Site update code - arity 15
        /// </summary>
        internal static TRet Update15<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) where T : class {
            return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14 });
        }


        // *** END GENERATED CODE ***

        #endregion

        #region Generated Predefined Void Update Targets

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_void_update_targets from: generate_dynsites.py

        /// <summary>
        /// Site update code - arity 1
        /// </summary>
        internal static void UpdateVoid1<T, T0>(CallSite site, T0 arg0) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0 });
        }

        /// <summary>
        /// Site update code - arity 2
        /// </summary>
        internal static void UpdateVoid2<T, T0, T1>(CallSite site, T0 arg0, T1 arg1) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1 });
        }

        /// <summary>
        /// Site update code - arity 3
        /// </summary>
        internal static void UpdateVoid3<T, T0, T1, T2>(CallSite site, T0 arg0, T1 arg1, T2 arg2) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2 });
        }

        /// <summary>
        /// Site update code - arity 4
        /// </summary>
        internal static void UpdateVoid4<T, T0, T1, T2, T3>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3 });
        }

        /// <summary>
        /// Site update code - arity 5
        /// </summary>
        internal static void UpdateVoid5<T, T0, T1, T2, T3, T4>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4 });
        }

        /// <summary>
        /// Site update code - arity 6
        /// </summary>
        internal static void UpdateVoid6<T, T0, T1, T2, T3, T4, T5>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5 });
        }

        /// <summary>
        /// Site update code - arity 7
        /// </summary>
        internal static void UpdateVoid7<T, T0, T1, T2, T3, T4, T5, T6>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6 });
        }

        /// <summary>
        /// Site update code - arity 8
        /// </summary>
        internal static void UpdateVoid8<T, T0, T1, T2, T3, T4, T5, T6, T7>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
        }

        /// <summary>
        /// Site update code - arity 9
        /// </summary>
        internal static void UpdateVoid9<T, T0, T1, T2, T3, T4, T5, T6, T7, T8>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
        }

        /// <summary>
        /// Site update code - arity 10
        /// </summary>
        internal static void UpdateVoid10<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
        }

        /// <summary>
        /// Site update code - arity 11
        /// </summary>
        internal static void UpdateVoid11<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 });
        }

        /// <summary>
        /// Site update code - arity 12
        /// </summary>
        internal static void UpdateVoid12<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11 });
        }

        /// <summary>
        /// Site update code - arity 13
        /// </summary>
        internal static void UpdateVoid13<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12 });
        }

        /// <summary>
        /// Site update code - arity 14
        /// </summary>
        internal static void UpdateVoid14<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13 });
        }

        /// <summary>
        /// Site update code - arity 15
        /// </summary>
        internal static void UpdateVoid15<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) where T : class {
            ((CallSite<T>)site).UpdateAndExecute(new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14 });
        }


        // *** END GENERATED CODE ***

        #endregion
    }

#pragma warning restore 618

}
