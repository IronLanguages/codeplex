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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Operations {
    public class PythonSites {
        #region Generated Python Sites

        // *** BEGIN GENERATED CODE ***


        private static FastDynamicSite<object, object, object> AddSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Add));

        public static object Add(object x, object y) {
            return AddSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> SubtractSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Subtract));

        public static object Subtract(object x, object y) {
            return SubtractSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> PowerSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Power));

        public static object Power(object x, object y) {
            return PowerSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> MultiplySharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Multiply));

        public static object Multiply(object x, object y) {
            return MultiplySharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> FloorDivideSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.FloorDivide));

        public static object FloorDivide(object x, object y) {
            return FloorDivideSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> DivideSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Divide));

        public static object Divide(object x, object y) {
            return DivideSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> TrueDivideSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.TrueDivide));

        public static object TrueDivide(object x, object y) {
            return TrueDivideSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> ModSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Mod));

        public static object Mod(object x, object y) {
            return ModSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> LeftShiftSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.LeftShift));

        public static object LeftShift(object x, object y) {
            return LeftShiftSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> RightShiftSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.RightShift));

        public static object RightShift(object x, object y) {
            return RightShiftSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> BitwiseAndSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.BitwiseAnd));

        public static object BitwiseAnd(object x, object y) {
            return BitwiseAndSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> BitwiseOrSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.BitwiseOr));

        public static object BitwiseOr(object x, object y) {
            return BitwiseOrSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> XorSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Xor));

        public static object Xor(object x, object y) {
            return XorSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> LessThanSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.LessThan));

        public static object LessThan(object x, object y) {
            return LessThanSharedSite.Invoke(x, y);
        }
        private static FastDynamicSite<object, object, bool> LessThanBooleanSharedSite =
            FastDynamicSite<object, object, bool>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.LessThan));

        public static bool LessThanRetBool(object x, object y) {
            return LessThanBooleanSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> GreaterThanSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.GreaterThan));

        public static object GreaterThan(object x, object y) {
            return GreaterThanSharedSite.Invoke(x, y);
        }
        private static FastDynamicSite<object, object, bool> GreaterThanBooleanSharedSite =
            FastDynamicSite<object, object, bool>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.GreaterThan));

        public static bool GreaterThanRetBool(object x, object y) {
            return GreaterThanBooleanSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> LessThanOrEqualSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.LessThanOrEqual));

        public static object LessThanOrEqual(object x, object y) {
            return LessThanOrEqualSharedSite.Invoke(x, y);
        }
        private static FastDynamicSite<object, object, bool> LessThanOrEqualBooleanSharedSite =
            FastDynamicSite<object, object, bool>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.LessThanOrEqual));

        public static bool LessThanOrEqualRetBool(object x, object y) {
            return LessThanOrEqualBooleanSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> GreaterThanOrEqualSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.GreaterThanOrEqual));

        public static object GreaterThanOrEqual(object x, object y) {
            return GreaterThanOrEqualSharedSite.Invoke(x, y);
        }
        private static FastDynamicSite<object, object, bool> GreaterThanOrEqualBooleanSharedSite =
            FastDynamicSite<object, object, bool>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.GreaterThanOrEqual));

        public static bool GreaterThanOrEqualRetBool(object x, object y) {
            return GreaterThanOrEqualBooleanSharedSite.Invoke(x, y);
        }

        private static FastDynamicSite<object, object, object> NotEqualsSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.NotEquals));

        public static object NotEquals(object x, object y) {
            return NotEqualsSharedSite.Invoke(x, y);
        }
        private static FastDynamicSite<object, object, bool> NotEqualsBooleanSharedSite =
            FastDynamicSite<object, object, bool>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.NotEquals));

        public static bool NotEqualsRetBool(object x, object y) {
            return NotEqualsBooleanSharedSite.Invoke(x, y);
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
