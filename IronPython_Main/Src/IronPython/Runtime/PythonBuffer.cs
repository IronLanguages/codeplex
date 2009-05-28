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
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Binding;
using System.Collections.Generic;

namespace IronPython.Runtime {
    [PythonType("buffer")]
    public sealed class PythonBuffer : ICodeFormattable {
        internal object _object;
        private int _offset;
        private int _size;

        private readonly CodeContext/*!*/ _context;

        public PythonBuffer(CodeContext/*!*/ context, object @object)
            : this(context, @object, 0) {
        }

        public PythonBuffer(CodeContext/*!*/ context, object @object, int offset)
            : this(context, @object, offset, -1) {
        }

        public PythonBuffer(CodeContext/*!*/ context, object @object, int offset, int size) {
            if (!InitBufferObject(@object, offset, size)) {
                throw PythonOps.TypeError("expected buffer object");
            }
            _context = context;
        }

        private bool InitBufferObject(object o, int offset, int size) {
            if (offset < 0) {
                throw PythonOps.ValueError("offset must be zero or positive");
            } else if (size < -1) {
                //  -1 is the way to ask for the default size so we allow -1 as a size
                throw PythonOps.ValueError("size must be zero or positive");
            }

            //  we currently support only buffers, strings and arrays
            //  of primitives, strings, bytes, and bytearray objects.
            int length;
            if (o is PythonBuffer) {
                PythonBuffer py = (PythonBuffer)o;
                o = py._object; // grab the internal object
                length = py._size;
            } else if (o is string) {
                string strobj = (string)o;
                length = strobj.Length;
            } else if (o is Bytes) {
                length = ((Bytes)o).Count;
            } else if (o is ByteArray) {
                length = ((ByteArray)o).Count;
            } else if (o is Array || o is IPythonArray) {
                Array arr = o as Array;
                if (arr != null) {
                    Type t = arr.GetType().GetElementType();
                    if (!t.IsPrimitive && t != typeof(string)) {
                        return false;
                    }
                    length = arr.Length;
                } else {
                    IPythonArray pa = (IPythonArray)o;
                    length = pa.__len__();
                }
            } else if (o is IPythonBufferable) {
                length = ((IPythonBufferable)o).Size;
                _object = o;
            } else {
                return false;
            }

            // reset the size based on the given buffer's original size
            if (size >= (length - offset) || size == -1) {
                _size = length - offset;
            } else {
                _size = size;
            }
            
            _object = o;
            _offset = offset;

            return true;
        }

        public override string ToString() {
            object res = GetSelectedRange();
            if (res is Bytes) {
                return PythonOps.MakeString((Bytes)res);
            } else if (res is ByteArray) {
                return PythonOps.MakeString((ByteArray)res);
            } else if (res is IPythonBufferable) {
                return PythonOps.MakeString((IList<byte>)GetSelectedRange());
            }

            return res.ToString();
        }

        public override bool Equals(object obj) {
            PythonBuffer b = obj as PythonBuffer;
            if (b == null) return false;

            return this == b;
        }

        public override int GetHashCode() {
            return _object.GetHashCode() ^ _offset ^ (_size << 16 | (_size >> 16));
        }

        private Slice GetSlice() {
            object end = null;
            if (_size >= 0) {
                end = _offset + _size;
            }
            return new Slice(_offset, end);
        }

        public object __getslice__(object start, object stop) {
            return this[new Slice(start, stop)];
        }

        private Exception ReadOnlyError() {
            return PythonOps.TypeError("buffer is read-only");
        }

        public object __setslice__(object start, object stop, object value) {
            throw ReadOnlyError();
        }

        public void __delitem__(int index) {
            throw ReadOnlyError();
        }

        public void __delslice__(object start, object stop) {
           throw ReadOnlyError();
        }

        public object this[object s] {
            [SpecialName]
            get {
                return PythonOps.GetIndex(_context, GetSelectedRange(), s);
            }
            [SpecialName]
            set {
                throw ReadOnlyError();
            }
        }

        private object GetSelectedRange() {
            IPythonArray arr = _object as IPythonArray;
            if(arr != null) {
                return arr.tostring();
            }

            ByteArray bytearr = _object as ByteArray;
            if (bytearr != null) {
                return new Bytes((IList<byte>)bytearr[GetSlice()]);
            }

            IPythonBufferable pyBuf = _object as IPythonBufferable;
            if (pyBuf != null) {
                return new Bytes(pyBuf.GetBytes(_offset, _size));
            }

            return PythonOps.GetIndex(_context, _object, GetSlice());
        }

        public static object operator +(PythonBuffer a, PythonBuffer b) {
            PythonContext context = PythonContext.GetContext(a._context);

            return context.Operation(
                PythonOperationKind.Add,
                PythonOps.GetIndex(a._context, a._object, a.GetSlice()), 
                PythonOps.GetIndex(a._context, b._object, b.GetSlice())
            );
        }

        public static object operator +(PythonBuffer a, string b) {
            return a.ToString() + b;
        }

        public static object operator *(PythonBuffer b, int n) {
            PythonContext context = PythonContext.GetContext(b._context);

            return context.Operation(
                PythonOperationKind.Multiply,
                PythonOps.GetIndex(b._context, b._object, b.GetSlice()),
                n
            );
        }

        public static object operator *(int n, PythonBuffer b) {
            PythonContext context = PythonContext.GetContext(b._context);

            return context.Operation(
                PythonOperationKind.Multiply,
                PythonOps.GetIndex(b._context, b._object, b.GetSlice()),
                n
            );                
        }

        public static bool operator ==(PythonBuffer a, PythonBuffer b) {
            if (Object.ReferenceEquals(a, b)) return true;
            if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null)) return false;

            return a._object.Equals(b._object) &&
                a._offset == b._offset &&
                a._size == b._size;
        }

        public static bool operator !=(PythonBuffer a, PythonBuffer b) {
            return !(a == b);
        }

        public int __len__() {
            return Math.Max(_size, 0);
        }

        internal int Size {
            get {
                return _size;
            }
        }

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<read-only buffer for 0x{0:X16}, size {1}, offset {2} at 0x{3:X16}>",
                PythonOps.Id(_object), _size, _offset, PythonOps.Id(this));
        }

        #endregion
    }

    /// <summary>
    /// A marker interface so we can recognize and access sequence members on our array objects.
    /// </summary>
    internal interface IPythonArray : ISequence {
        string tostring();
    }

    internal interface IPythonBufferable {
        IntPtr UnsafeAddress {
            get;
        }

        int Size {
            get;
        }

        byte[] GetBytes(int offset, int length);
    }
}
