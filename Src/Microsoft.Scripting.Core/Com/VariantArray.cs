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
#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Linq.Expressions;
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.ComInterop {

    // TODO: make these types internal

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [CLSCompliant(false)]
    [StructLayout(LayoutKind.Sequential)]
    public struct VariantArray1 {
        [Obsolete("do not use", true)]
        public Variant Element0;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [CLSCompliant(false)]
    [StructLayout(LayoutKind.Sequential)]
    public struct VariantArray2 {
        [Obsolete("do not use", true)]
        public Variant Element0;
        [Obsolete("do not use", true)]
        public Variant Element1;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [CLSCompliant(false)]
    [StructLayout(LayoutKind.Sequential)]
    public struct VariantArray4 {
        [Obsolete("do not use", true)]
        public Variant Element0;
        [Obsolete("do not use", true)]
        public Variant Element1;
        [Obsolete("do not use", true)]
        public Variant Element2;
        [Obsolete("do not use", true)]
        public Variant Element3;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [CLSCompliant(false)]
    [StructLayout(LayoutKind.Sequential)]
    public struct VariantArray8 {
        [Obsolete("do not use", true)]
        public Variant Element0;
        [Obsolete("do not use", true)]
        public Variant Element1;
        [Obsolete("do not use", true)]
        public Variant Element2;
        [Obsolete("do not use", true)]
        public Variant Element3;
        [Obsolete("do not use", true)]
        public Variant Element4;
        [Obsolete("do not use", true)]
        public Variant Element5;
        [Obsolete("do not use", true)]
        public Variant Element6;
        [Obsolete("do not use", true)]
        public Variant Element7;
    }

    //
    // Helper for getting the right VariantArray struct for a given number of
    // arguments. Will generate a struct if needed.
    //
    // We use this because we don't have stackalloc or pinning in Expression
    // Trees, so we can't create an array of Variants directly.
    //
    internal static class VariantArray {
        // Don't need a dictionary for this, it will have very few elements
        // (guarenteed less than 28, in practice 0-2)
        private static readonly List<Type> _generatedTypes = new List<Type>(0);

        internal static MemberExpression GetStructField(ParameterExpression variantArray, int field) {
            return Expression.Field(variantArray, "Element" + field);
        }

        internal static Type GetStructType(int args) {
            Debug.Assert(args >= 0);
            if (args <= 1) return typeof(VariantArray1);
            if (args <= 2) return typeof(VariantArray2);
            if (args <= 4) return typeof(VariantArray4);
            if (args <= 8) return typeof(VariantArray8);

            int size = 1;
            while (args > size) {
                size *= 2;
            }

            lock (_generatedTypes) {
                // See if we can find an existing type
                foreach (Type t in _generatedTypes) {
                    int arity = int.Parse(t.Name.Substring("VariantArray".Length), CultureInfo.InvariantCulture);
                    if (size == arity) {
                        return t;
                    }
                }

                // Else generate a new type
                Type type = CreateCustomType(size);
                _generatedTypes.Add(type);
                return type;
            }
        }

        // TODO: generate internal type with internal members
        private static Type CreateCustomType(int size) {
            var attrs = TypeAttributes.Public | TypeAttributes.SequentialLayout;
            AssemblyGen asm = Snippets.Shared.GetAssembly(false, false);
            TypeBuilder type = asm.DefineType("VariantArray" + size, typeof(ValueType), attrs, true);
            for (int i = 0; i < size; i++) {
                type.DefineField("Element" + i, typeof(Variant), FieldAttributes.Public);
            }
            return type.CreateType();
        }
    }
}

#endif