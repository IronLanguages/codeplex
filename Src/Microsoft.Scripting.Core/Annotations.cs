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
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {
    [Serializable]
    public sealed class Annotations : IEnumerable<object>, IEnumerable {
        // Internal storage as low level as possible 
        private readonly object[] _annotations;

        internal Annotations(IEnumerable<Object> annotations) {
            ContractUtils.RequiresNotNull(annotations, "annotations");
            List<object> clone = new List<object>();

            foreach (object annotation in annotations) {
                ContractUtils.RequiresNotNull(annotation, "annotations");
                clone.Add(annotation);
            }
            this._annotations = clone.ToArray();
        }

        private Annotations(object[] annotations) {
            _annotations = annotations;
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
            for (int i = 0; i < _annotations.Length; i++) {
                if (_annotations[i].GetType() == typeof(T)) {
                    annotation = (T)_annotations[i];
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

            object[] newAnnotations = _annotations;
            Array.Resize(ref newAnnotations, _annotations.Length + 1);
            newAnnotations[_annotations.Length] = annotation;
            return new Annotations(newAnnotations);
        }

        /// <summary>
        /// Returns clone with annotation(s) of type T removed
        /// </summary>
         public Annotations Remove<T>() {
            int count = 0;
            object[] filtered = new object[_annotations.Length];
            for (int i = 0; i < _annotations.Length; i++) {
                if (_annotations[i].GetType() != typeof(T)) {
                    filtered[count] = _annotations[i];
                    count++;
                }
            }
            Array.Resize(ref filtered, count);
            return new Annotations(filtered);
        }

        public bool Contains<T>() {
            for (int i = 0; i < _annotations.Length; i++) {
                if (_annotations[i].GetType() == typeof(T)) {
                    return true;
                }
            }
            return false;
        }

        //
        // IEnumerable
        //

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return _annotations.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            for (int i = 0; i < _annotations.Length; ++i) {
                yield return _annotations[i];
            }
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Annotations Empty = new Annotations(new object[0]);
    }
}

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        public static Annotations Annotate(params Object[] items) {
            return new Annotations(items);
        }
    }
}