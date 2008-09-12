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
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;

namespace Microsoft.Scripting {
    /// <summary>
    /// YieldAnnotation can be used to annotate yield statements.  YieldAnnotation can
    /// used with IDebuggableGenerator to change the state of the generator.
    /// </summary>
    [Serializable]
    public struct YieldAnnotation {
        private readonly int _yieldMarker;

        public YieldAnnotation(int yieldMarker) {
            _yieldMarker = yieldMarker;
        }

        public int YieldMarker {
            get { return _yieldMarker; }
        }

        public static bool operator ==(YieldAnnotation left, YieldAnnotation right) {
            return left._yieldMarker == right._yieldMarker;
        }

        public static bool operator !=(YieldAnnotation left, YieldAnnotation right) {
            return left._yieldMarker != right._yieldMarker;
        }

        public override bool Equals(object obj) {
            if (!(obj is YieldAnnotation)) return false;

            YieldAnnotation other = (YieldAnnotation)obj;
            return _yieldMarker == other._yieldMarker;
        }

        public override int GetHashCode() {
            return _yieldMarker;
        }
    }
}
