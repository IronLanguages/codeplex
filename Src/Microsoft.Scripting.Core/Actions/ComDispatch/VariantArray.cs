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

#if !SILVERLIGHT // ComObject

using System; using Microsoft;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.Actions.ComDispatch {

    [CLSCompliant(false)]
    [StructLayout(LayoutKind.Sequential)]
    public struct VariantArray {
        public Variant _element0;
        public Variant _element1;
        public Variant _element2;
        public Variant _element3;
        public Variant _element4;
        public Variant _element5;
        public Variant _element6;
        public Variant _element7;

        internal const int NumberOfElements = 8;

        # region FxCop-required APIs

        public override bool Equals(object obj) {
            if ((obj == null) || (!(obj is VariantArray))) {
                return false;
            }

            VariantArray other = (VariantArray)obj;
            return _element0 == other._element0 &&
                _element1 == other._element1 &&
                _element2 == other._element2 &&
                _element3 == other._element3 &&
                _element4 == other._element4 &&
                _element5 == other._element5 &&
                _element6 == other._element6 &&
                _element7 == other._element7;
        }

        public override int GetHashCode() {
            return _element0.GetHashCode() ^ _element1.GetHashCode() ^ _element2.GetHashCode() ^ _element3.GetHashCode() ^
                   _element4.GetHashCode() ^ _element5.GetHashCode() ^ _element6.GetHashCode() ^ _element7.GetHashCode();
        }

        public static bool operator ==(VariantArray a, VariantArray b) {
            return a.Equals(b);
        }
        public static bool operator !=(VariantArray a, VariantArray b) {
            return !a.Equals(b);
        }

        #endregion

        internal static System.Reflection.FieldInfo GetField(int index) {
            Debug.Assert(index < NumberOfElements);
            return typeof(VariantArray).GetField("_element" + index);
        }
    }
}

#endif
