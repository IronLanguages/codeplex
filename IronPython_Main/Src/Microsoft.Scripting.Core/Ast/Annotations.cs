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
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {

    // TODO: should be ICollection<object> at least
    [Serializable]
    public abstract class Annotations : IEnumerable<object>, IEnumerable {
        // Internal storage as low level as possible 
        internal Annotations() {
        }

        public int Count {
            get { return CountImpl; }
        }

        //
        // Simple type based accessors
        //

        public T Get<T>() {
            T result;
            TryGet(out result);
            return result;
        }

        public bool TryGet<T>(out T annotation) {
            return TryGetImpl<T>(out annotation);
        }

        internal virtual bool TryGetImpl<T>(out T annotation) {
            for (int i = 0; i < Count; i++) {
                if (this[i].GetType() == typeof(T)) {
                    annotation = (T)this[i];
                    return true;
                }
            }
            annotation = default(T);
            return false;
        }

        /// <summary>
        /// Creates a clone of annotations and adds the value into the clone
        /// </summary>
        public Annotations Add<T>(T annotation) {
            ContractUtils.RequiresNotNull(annotation, "annotation");

            if (Count == 0) {
                return new AnnotationsSingle<T>(annotation);
            } else {
                object[] newAnnotations = new object[Count + 1];
                for (int i = 0; i < Count; i++) {
                    newAnnotations[i] = this[i];
                }
                newAnnotations[Count] = annotation;
                return new AnnotationsArray(newAnnotations);
            }
        }

        /// <summary>
        /// Returns clone with annotation(s) of type T removed
        /// </summary>
        public Annotations Remove<T>() {
            int count = 0;
            object[] filtered = new object[Count];
            for (int i = 0; i < Count; i++) {
                if (this[i].GetType() != typeof(T)) {
                    filtered[count] = this[i];
                    count++;
                }
            }
            Array.Resize(ref filtered, count);
            return new AnnotationsArray(filtered);
        }

        public bool Contains<T>() {
            for (int i = 0; i < Count; i++) {
                if (this[i].GetType() == typeof(T)) {
                    return true;
                }
            }
            return false;
        }

        //
        // IEnumerable
        //

        #region IEnumerable Members
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Microsoft.Linq.Expressions.Annotations.#System.Collections.IEnumerable.GetEnumerator()")]
        IEnumerator IEnumerable.GetEnumerator() {
            for (int i = 0; i < Count; ++i) {
                yield return this[i];
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Microsoft.Linq.Expressions.Annotations.#System.Collections.Generic.IEnumerable`1<System.Object>.GetEnumerator()")]
        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            for (int i = 0; i < Count; ++i) {
                yield return this[i];
            }
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Annotations Empty = new AnnotationsArray(new object[0]);

        internal abstract int CountImpl {
            get;
        }

        internal abstract object this[int index] {
            get;
        }
    }

    internal class AnnotationsArray : Annotations {
        private readonly object[] _annotations;

        internal AnnotationsArray(object[] data) {
            _annotations = data;
        }

        internal override object this[int index] {
            get { return _annotations[index]; }
        }

        internal override int CountImpl {
            get {
                return _annotations.Length;
            }
        }
    }

    internal class AnnotationsSingle<T> : Annotations {
        private readonly T _annotation;

        internal AnnotationsSingle(T data) {
            _annotation = data;
        }

        internal override object this[int index] {
            get {
                Debug.Assert(index == 0);
                return _annotation;
            }
        }

        internal override int CountImpl {
            get {
                return 1;
            }
        }

        internal override bool TryGetImpl<TAnnoationType>(out TAnnoationType annotation) {
            if (typeof(T) == typeof(TAnnoationType)) {
                annotation = (TAnnoationType)(object)_annotation;
                return true;
            }
            annotation = default(TAnnoationType);
            return false;
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        [Obsolete("use Expression.DebugInfo for debug information")]
        public static Annotations Annotate() {
            return Annotations.Empty;
        }

        [Obsolete("use Expression.DebugInfo for debug information")]
        public static Annotations Annotate<T>(T item0) {
            return new AnnotationsSingle<T>(item0);
        }

        [Obsolete("use Expression.DebugInfo for debug information")]
        public static Annotations Annotate(object item0, object item1) {
            return new AnnotationsArray(new[] { item0, item1 });
        }

        [Obsolete("use Expression.DebugInfo for debug information")]
        public static Annotations Annotate(params object[] items) {
            if (items == null) {
                return Annotations.Empty;
            }
            if (items.Length == 0) {
                return Annotations.Empty;
            }

            return new AnnotationsArray((object[])items.Clone());
        }
    }
}