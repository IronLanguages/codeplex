using System;
using System.Collections.Generic;
using System.Text;

namespace ShapeScript {
    internal static class Extensions {
        internal static bool IsNullOrEmpty(this string s) {
            return (s == null) || (s.Length == 0);
        }

        internal static bool IsWhiteSpace(this string s) {
            if (s.Trim().Trim('\n').Length == 0)
                return true;

            return false;
        }
        internal static bool IsNullOrEmptyOrWhiteSpace(this string s) {
            return s.IsNullOrEmpty() || s.IsWhiteSpace();
                
        }
    }
}
