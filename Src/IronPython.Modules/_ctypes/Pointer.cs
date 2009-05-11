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
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if !SILVERLIGHT
namespace IronPython.Modules {
    /// <summary>
    /// Provides support for interop with native code from Python code.
    /// </summary>
    public static partial class CTypes {
        [PythonType("_Pointer")]
        public abstract class Pointer : CData {
            private CData _object;

            public Pointer() {
                _memHolder = new MemoryHolder(IntPtr.Size);
            }

            public Pointer(CData value) {
                _object = value; // Keep alive the object, more to do here.
                _memHolder = new MemoryHolder(IntPtr.Size);
                _memHolder.WriteIntPtr(0, value._memHolder);
            }

            public object contents {
                get {
                    PythonType elementType = (PythonType)((PointerType)NativeType)._type;

                    CData res = (CData)elementType.CreateInstance(elementType.Context.SharedContext);
                    res._memHolder = _memHolder.ReadMemoryHolder(0);
                    if(res._memHolder.UnsafeAddress == IntPtr.Zero) {
                        throw PythonOps.ValueError("NULL value access");
                    }
                    return res;
                }
                set {
                }
            }

            public object this[int index] {
                get {
                    INativeType type = ((PointerType)NativeType)._type;
                    MemoryHolder address = _memHolder.ReadMemoryHolder(0);

                    return type.GetValue(address, checked(type.Size * index), false);
                }
                set {
                    MemoryHolder address = _memHolder.ReadMemoryHolder(0);

                    INativeType type = ((PointerType)NativeType)._type;
                    type.SetValue(address, checked(type.Size * index), value);
                }
            }

            public bool __nonzero__() {
                return _memHolder.ReadIntPtr(0) != IntPtr.Zero;
            }

            public object this[Slice index] {
                get {
                    int start = index.start != null ? (int)index.start : 0;
                    int stop = index.stop != null ? (int)index.stop : 0;
                    int step = index.step != null ? (int)index.step : 1;

                    if (start < 0) {
                        start = 0;
                    }
                    INativeType type = ((PointerType)NativeType)._type;
                    SimpleType elemType = type as SimpleType;

                    if ((stop < start && step > 0) || (start < stop && step < 0)) {
                        if (elemType != null && (elemType._type == SimpleTypeKind.WChar || elemType._type == SimpleTypeKind.Char)) {
                            return String.Empty;
                        }
                        return new List();
                    }

                    MemoryHolder address = _memHolder.ReadMemoryHolder(0);
                    if (elemType != null && (elemType._type == SimpleTypeKind.WChar || elemType._type == SimpleTypeKind.Char)) {
                        int elmSize = ((INativeType)elemType).Size;
                        StringBuilder res = new StringBuilder();
                        for (int i = start; i < stop; i += step) {
                            res.Append(
                                elemType.ReadChar(address, checked(i * elmSize))
                            );
                        }

                        return res.ToString();
                    } else {
                        List res = new List(stop - start);
                        for (int i = start; i < stop; i+= step) {
                            res.AddNoLock(
                                type.GetValue(address, checked(type.Size * i), false)
                            );
                        }
                        return res;
                    }
                }
            }
        }
    }
}
#endif
