/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections;
using System.Text;

namespace IronPython.Runtime {
    [PythonType("buffer")]
    public class PythonBuffer {
        private object @object;
        private int offset;
        private int size;

        private bool isbuffer = false /*buffer of buffer*/, isstring = false, isarray = false;

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
            if (o == null || (!(isbuffer = o is PythonBuffer) && !(isstring = o is string) && !(isarray = o is Array))) {
                return false;
            }
            if (offset < 0) {
                throw Ops.ValueError("offset must be zero or positive");
            }
            //  -1 is the way to ask for the default size so we allow -1 as a size
            if (size < -1) {
                throw Ops.ValueError("size must be zero or positive");
            }
            if (isbuffer) {
                PythonBuffer py = (PythonBuffer)o;
                o = py.@object; // grab the internal object
                offset = py.offset + offset; // reset the offset based on the given buffer's original offset
                // reset the size based on the given buffer's original size
                if (size >= py.size - offset || size == -1) {
                    this.size = py.size - offset;
                } else {
                    this.size = size;
                }
            }
            else if (isstring) {
                string strobj = ((string)o);
                if (size >= strobj.Length || size == -1) {
                    this.size = strobj.Length;
                } else {
                    this.size = size;
                }
            } else { // has to be an array at this point
                Array arr = (Array)o;
                Type t = arr.GetType().GetElementType();
                if (!t.IsPrimitive && t != typeof(string)) {
                    return false;
                }
                if (size >= arr.Length || size == -1) {
                    this.size = arr.Length;
                } else {
                    this.size = size;
                }
            }
            this.@object = o;
            this.offset = offset;
            
            return true;
        }

        [PythonName("__str__")]
        public override string ToString() {
            return Ops.GetIndex(@object, GetSlice()).ToString();
        }

        private object GetSlice() {
            object end = null;
            if (size >= 0) {
                end = offset + size;
            }
            return new Slice(offset, end);
        }

        public object this[object s] {
            get {
                return Ops.GetIndex(Ops.GetIndex(@object, GetSlice()), s);
            }
            set {
                throw Ops.TypeError("buffer is read-only");
            }
        }

        [PythonName("__repr__")]
        public string Repr() {
            return string.Format("<read-only buffer for 0x{0:X16}, size {1}, offset {2} at 0x{3:X16}>",
                Ops.Id(@object), size, offset, Ops.Id(this));
        }

        public static object operator +(PythonBuffer a, PythonBuffer b) {
            return Ops.Add(Ops.GetIndex(a.@object, a.GetSlice()), Ops.GetIndex(b.@object, b.GetSlice()));
        }

        public static object operator *(PythonBuffer b, int n) {
            return Ops.Multiply(Ops.GetIndex(b.@object, b.GetSlice()), n);
        }

        public static object operator *(int n, PythonBuffer b) {
            return Ops.Multiply(Ops.GetIndex(b.@object, b.GetSlice()), n);
        }

        [PythonName("__len__")]
        public int GetLength() {
            return size;
        }

        public int Size {
            get {
                return size;
            }
        }
    }
}
