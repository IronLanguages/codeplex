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

using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class SwitchStatement : Expression, ISpan {
        private readonly SourceLocation _header;
        private readonly Expression _testValue;
        private readonly ReadOnlyCollection<SwitchCase> _cases;

        private readonly SourceLocation _start;
        private readonly SourceLocation _end;

        internal SwitchStatement(SourceLocation start, SourceLocation end, SourceLocation header, Expression/*!*/ testValue, ReadOnlyCollection<SwitchCase>/*!*/ cases)
            : base(AstNodeType.SwitchStatement, typeof(void)) {
            Assert.NotNullItems(cases);

            _start = start;
            _end = end;
            _testValue = testValue;
            _cases = cases;
            _header = header;
        }

        public Expression TestValue {
            get { return _testValue; }
        }

        public ReadOnlyCollection<SwitchCase> Cases {
            get { return _cases; }
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation End {
            get { return _end; }
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Ast {
        public static SwitchStatement Switch(SourceSpan span, SourceLocation header, Expression value, params SwitchCase[] cases) {
            Contract.RequiresNotNull(value, "value");
            Contract.Requires(value.Type == typeof(int), "value", "Value must be int");
            Contract.RequiresNotEmpty(cases, "cases");
            Contract.RequiresNotNullItems(cases, "cases");

            bool @default = false;
            int max = Int32.MinValue;
            int min = Int32.MaxValue;
            foreach (SwitchCase sc in cases) {
                if (sc.IsDefault) {
                    Contract.Requires(@default == false, "cases", "Only one default clause allowed");
                    @default = true;
                } else {
                    int val = sc.Value;
                    if (val > max) max = val;
                    if (val < min) min = val;
                }
            }

            Contract.Requires(UniqueCaseValues(cases, min, max), "cases", "Case values must be unique");

            return new SwitchStatement(span.Start, span.End, header, value, CollectionUtils.ToReadOnlyCollection(cases));
        }

        // Below his threshold we'll use brute force N^2 algorithm
        private const int N2Threshold = 10;

        // If values are in a small range, we'll use bit array
        private const long BitArrayThreshold = 1024;

        private static bool UniqueCaseValues(SwitchCase[] cases, int min, int max) {
            int length = cases.Length;

            // If we have small number of cases, use straightforward N2 algorithm
            // which doesn't allocate memory
            if (length < N2Threshold) {
                for (int i = 0; i < length; i++) {
                    SwitchCase sci = cases[i];
                    if (sci.IsDefault) {
                        continue;
                    }
                    for (int j = i + 1; j < length; j++) {
                        SwitchCase scj = cases[j];
                        if (scj.IsDefault) {
                            continue;
                        }

                        if (sci.Value == scj.Value) {
                            // Duplicate value found
                            return false;
                        }
                    }
                }

                return true;
            }

            // We have at least N2Threshold items so the min and max values
            // are set to actual values and not the Int32.MaxValue and Int32.MaxValue
            Debug.Assert(min <= max);
            long delta = (long)max - (long)min;
            if (delta < BitArrayThreshold) {
                BitArray ba = new BitArray((int)delta + 1, false);

                for (int i = 0; i < length; i++) {
                    SwitchCase sc = cases[i];
                    if (sc.IsDefault) {
                        continue;
                    }
                    // normalize to 0 .. (max - min)
                    int val = sc.Value - min;
                    if (ba.Get(val)) {
                        // Duplicate value found
                        return false;
                    }
                    ba.Set(val, true);
                }

                return true;
            }

            // Too many values that are too spread around. Use dictionary
            // Using Dictionary<int, object> as it is used elsewhere to
            // minimize the impact of generic instantiation
            Dictionary<int, object> dict = new Dictionary<int, object>(length);
            for (int i = 0; i < length; i++) {
                SwitchCase sc = cases[i];
                if (sc.IsDefault) {
                    continue;
                }
                int val = sc.Value;
                if (dict.ContainsKey(val)) {
                    // Duplicate value found
                    return false;
                }
                dict[val] = null;
            }

            return true;
        }
    }
}
