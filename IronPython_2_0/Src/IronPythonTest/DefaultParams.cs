/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using System.Runtime.InteropServices;

namespace IronPythonTest {
    public class DefaultParams {
        public static int FuncWithDefaults([DefaultParameterValue(1)] int x,
           [DefaultParameterValue(2)] int y,
           [DefaultParameterValue(3)] int z) {
            return x + y + z;
        }
    }
}
