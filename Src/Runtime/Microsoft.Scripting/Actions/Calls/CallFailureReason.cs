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


namespace Microsoft.Scripting.Actions.Calls {
    public enum CallFailureReason {
        /// <summary>
        /// Default value, their was no CallFailure.
        /// </summary>
        None,
        /// <summary>
        /// One of more parameters failed to be converted
        /// </summary>
        ConversionFailure,
        /// <summary>
        /// One or more keyword arguments could not be successfully assigned to a positional argument
        /// </summary>
        UnassignableKeyword,
        /// <summary>
        /// One or more keyword arguments were duplicated or would have taken the spot of a 
        /// provided positional argument.
        /// </summary>
        DuplicateKeyword
    }
}
