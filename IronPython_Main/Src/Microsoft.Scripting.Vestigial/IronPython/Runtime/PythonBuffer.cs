/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Text;

using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Internal;

namespace IronPython.Runtime {
    [PythonType("buffer")]
    public class PythonBuffer : ICodeFormattable {
        private object _object;
        private int _offset;
        private int _size;

        private bool _isbuffer;      /*buffer of buffer*/
        private bool _isstring;
        private bool _isarray;

        public PythonBuffer(object @object)
            : this(@object, 0) {
        }

        public PythonBuffer(object @object, int offset)
            : this(@object, offset, -1) {
        }

        public PythonBuffer(object @object, int offset, int size) {
            if (!InitBufferObject(@object, offset, size)) {
                throw Ops.TypeError("expected buffer object");
            }
        }

        private bool InitBufferObject(object o, int offset, int size) {
            //  we currently support only buffers, strings and arrays
            //  of primitives and strings
            if (o == null || (!(_isbuffer = o is PythonBuffer) && !(_isstring = o is string) && !(_isarray = o is Array))) {
                return false;
            }
            if (offset < 0) {
                throw Ops.ValueError("offset must be zero or positive");
            }
            //  -1 is the way to ask for the default size so we allow -1 as a size
            if (size < -1) {
                throw Ops.ValueError("size must be zero or positive");
            }
            if (_isbuffer) {
                PythonBuffer py = (PythonBuffer)o;
                o = py._object; // grab the internal object
                offset = py._offset + offset; // reset the offset based on the given buffer's original offset
                // reset the size based on the given buffer's original size
                if (size >= py._size - offset || size == -1) {
                    this._size = py._size - offset;
                } else {
                    this._size = size;
                }
            } else if (_isstring) {
                string strobj = ((string)o);
                if (size >= strobj.Length || size == -1) {
                    this._size = strobj.Length;
                } else {
                    this._size = size;
                }
            } else if (_isarray) { // has to be an array at this point
                Array arr = (Array)o;
                Type t = arr.GetType().GetElementType();
                if (!t.IsPrimitive && t != typeof(string)) {
                    return false;
                }
                if (size >= arr.Length || size == -1) {
                    this._size = arr.Length;
                } else {
                    this._size = size;
                }
            }
            this._object = o;
            this._offset = offset;

            return true;
        }

        public override string ToString() {
            return Ops.GetIndex(_object, GetSlice()).ToString();
        }

        private object GetSlice() {
            object end = null;
            if (_size >= 0) {
                end = _offset + _size;
            }
            return new Slice(_offset, end);
        }

        public object this[object s] {
            [OperatorMethod]
            get {
                return Ops.GetIndex(Ops.GetIndex(_object, GetSlice()), s);
            }
            [OperatorMethod]
            set {
                throw Ops.TypeError("buffer is read-only");
            }
        }

        public static object operator +(PythonBuffer a, PythonBuffer b) {
            return PythonSites.Add(Ops.GetIndex(a._object, a.GetSlice()), Ops.GetIndex(b._object, b.GetSlice()));
        }

        public static object operator *(PythonBuffer b, int n) {
            return PythonSites.Multiply(Ops.GetIndex(b._object, b.GetSlice()), n);
        }

        public static object operator *(int n, PythonBuffer b) {
            return PythonSites.Multiply(Ops.GetIndex(b._object, b.GetSlice()), n);
        }

        public static bool operator ==(PythonBuffer a, PythonBuffer b) {
            if (Object.ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;

            return a._object.Equals(b._object) &&
                a._offset == b._offset &&
                a._size == b._size;
        }

        public static bool operator !=(PythonBuffer a, PythonBuffer b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            PythonBuffer b = obj as PythonBuffer;
            if (b == null) return false;

            return this == b;
        }

        public override int GetHashCode() {
            return _object.GetHashCode() ^ _offset ^ (_size<<16 | (_size>>16));
        }

        [OperatorMethod, PythonName("__len__")]
        public int GetLength() {
            return _size;
        }

        public int Size {
            get {
                return _size;
            }
        }

        #region ICodeFormattable Members

        [OperatorMethod, PythonName("__repr__")]
        public string ToCodeString(CodeContext context) {
            return string.Format("<read-only buffer for 0x{0:X16}, size {1}, offset {2} at 0x{3:X16}>",
                Ops.Id(_object), _size, _offset, Ops.Id(this));
        }

        #endregion
    }
}
