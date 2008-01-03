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


namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// Indicates the specific type of failure, if any, from binding to a method.
    /// </summary>
    public enum BindingResult {
        /// <summary>
        /// The binding succeeded
        /// </summary>
        Success,
        /// <summary>
        /// More than one method was applicable for the provided parameters and no method was considered the best.
        /// </summary>
        AmbigiousMatch,
        /// <summary>
        /// There are no overloads that match the number of parameters required for the call
        /// </summary>
        IncorrectArgumentCount,
        /// <summary>
        /// One or more of the arguments cannot be converted
        /// </summary>
        ConversionFailure,
    }
}
