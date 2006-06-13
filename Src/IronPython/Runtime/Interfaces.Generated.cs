/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace IronPython.Runtime {

    public interface INumber {
        #region Generated INumber Methods

        // *** BEGIN GENERATED CODE ***

        object Add(object other);
        object ReverseAdd(object other);

        object Subtract(object other);
        object ReverseSubtract(object other);

        object Power(object other);
        object ReversePower(object other);

        object Multiply(object other);
        object ReverseMultiply(object other);

        object Divide(object other);
        object ReverseDivide(object other);

        object FloorDivide(object other);
        object ReverseFloorDivide(object other);

        object TrueDivide(object other);
        object ReverseTrueDivide(object other);

        object Mod(object other);
        object ReverseMod(object other);


        // *** END GENERATED CODE ***

        #endregion
    }
}
