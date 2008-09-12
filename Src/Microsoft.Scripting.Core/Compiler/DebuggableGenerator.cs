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
using System.Collections;
using System.Collections.Generic;
using Microsoft.Scripting;

namespace Microsoft.Runtime.CompilerServices {

    public interface IDebuggableGenerator {
        int YieldMarkerLocation { get; set; }
    }

    public sealed class DebuggableGenerator : Generator, IDebuggableGenerator {

        private readonly int[] _yieldMarkers;

        public DebuggableGenerator(GeneratorNext next, int[] yieldMarkers)
            : base(next) {
            _yieldMarkers = yieldMarkers;
        }

        #region IDebuggableGenerator Members

        public int YieldMarkerLocation {
            get {
                if (Location < _yieldMarkers.Length)
                    return _yieldMarkers[Location];

                throw new System.InvalidOperationException();
            }
            set {
                for (int i = 0; i < _yieldMarkers.Length; i++) {
                    if (_yieldMarkers[i] == value) {
                        Location = i;
                        return;
                    }
                }

                throw new System.InvalidOperationException();
            }
        }

        #endregion
    }
}
