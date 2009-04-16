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
using Microsoft.Scripting;
using System.IO;
using Microsoft.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Text;
using System.Threading;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations {

    /// <summary>
    /// Contains functions that are called directly from
    /// generated code to perform low-level runtime functionality.
    /// </summary>
    public static partial class PythonOps {
        #region Shared static data

        [ThreadStatic]
        private static List<object> InfiniteRepr;

        // The "current" exception on this thread that will be returned via sys.exc_info()
        [ThreadStatic]
        internal static Exception RawException;

        public static readonly PythonTuple EmptyTuple = PythonTuple.EMPTY;

        #endregion

        public static BigInteger MakeIntegerFromHex(string s) {
            return LiteralParser.ParseBigInteger(s, 16);
        }

        public static PythonDictionary MakeDict(int size) {
            return new PythonDictionary(size);
        }

        /// <summary>
        /// Creates a new dictionary extracting the keys and values from the
        /// provided data array.  Keys/values are adjacent in the array with
        /// the value coming first.
        /// </summary>
        public static PythonDictionary MakeDictFromItems(object[] data) {
            return new PythonDictionary(new CommonDictionaryStorage(data, false));
        }

        /// <summary>
        /// Creates a new dictionary extracting the keys and values from the
        /// provided data array.  Keys/values are adjacent in the array with
        /// the value coming first.
        /// </summary>
        public static PythonDictionary MakeHomogeneousDictFromItems(object[] data) {
            return new PythonDictionary(new CommonDictionaryStorage(data, true));
        }

        public static bool IsCallable(CodeContext/*!*/ context, object o) {
            // This tells if an object can be called, but does not make a claim about the parameter list.
            // In 1.x, we could check for certain interfaces like ICallable*, but those interfaces were deprecated
            // in favor of dynamic sites. 
            // This is difficult to infer because we'd need to simulate the entire callbinder, which can include
            // looking for [SpecialName] call methods and checking for a rule from IDynamicMetaObjectProvider. But even that wouldn't
            // be complete since sites require the argument list of the call, and we only have the instance here. 
            // Thus check a dedicated IsCallable operator. This lets each object describe if it's callable.


            // Invoke Operator.IsCallable on the object. 
            return PythonContext.GetContext(context).IsCallable(o);
        }

        public static bool IsTrue(object o) {
            return Converter.ConvertToBoolean(o);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<object> GetReprInfinite() {
            if (InfiniteRepr == null) {
                InfiniteRepr = new List<object>();
            }
            return InfiniteRepr;
        }

#if !SILVERLIGHT
        
        internal static object LookupEncodingError(CodeContext/*!*/ context, string name) {
            Dictionary<string, object> errorHandlers = PythonContext.GetContext(context).ErrorHandlers;
            lock (errorHandlers) {
                if (errorHandlers.ContainsKey(name))
                    return errorHandlers[name];
                else
                    throw PythonOps.LookupError("unknown error handler name '{0}'", name);
            }
        }

        internal static void RegisterEncodingError(CodeContext/*!*/ context, string name, object handler) {
            Dictionary<string, object> errorHandlers = PythonContext.GetContext(context).ErrorHandlers;

            lock (errorHandlers) {
                if (!PythonOps.IsCallable(context, handler))
                    throw PythonOps.TypeError("handler must be callable");

                errorHandlers[name] = handler;
            }
        }

#endif
        
        internal static PythonTuple LookupEncoding(CodeContext/*!*/ context, string encoding) {
            List<object> searchFunctions = PythonContext.GetContext(context).SearchFunctions;
            lock (searchFunctions) {
                for (int i = 0; i < searchFunctions.Count; i++) {
                    object res = PythonCalls.Call(context, searchFunctions[i], encoding);
                    if (res != null) return (PythonTuple)res;
                }
            }
            
            throw PythonOps.LookupError("unknown encoding: {0}", encoding);
        }

        internal static void RegisterEncoding(CodeContext/*!*/ context, object search_function) {
            if(!PythonOps.IsCallable(context, search_function))
                throw PythonOps.TypeError("search_function must be callable");

            List<object> searchFunctions = PythonContext.GetContext(context).SearchFunctions;

            lock (searchFunctions) {
                searchFunctions.Add(search_function);
            }
        }

        internal static string GetPythonTypeName(object obj) {
            OldInstance oi = obj as OldInstance;
            if (oi != null) return oi._class._name.ToString();
            else return DynamicHelpers.GetPythonType(obj).Name;
        }

        public static string Repr(CodeContext/*!*/ context, object o) {
            if (o == null) return "None";

            string s;
            if ((s = o as string) != null) return StringOps.__repr__(s);
            if (o is int) return Int32Ops.__repr__((int)o);
            if (o is long) return ((long)o).ToString() + "L";

            // could be a container object, we need to detect recursion, but only
            // for our own built-in types that we're aware of.  The user can setup
            // infinite recursion in their own class if they want.
            ICodeFormattable f = o as ICodeFormattable;
            if (f != null) {
                return f.__repr__(context);
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Temporary, "Repr " + o.GetType().FullName);

            return PythonContext.InvokeUnaryOperator(context, UnaryOperators.Repr, o) as string;
        }

        public static List<object> GetAndCheckInfinite(object o) {
            List<object> infinite = GetReprInfinite();
            foreach (object o2 in infinite) {
                if (o == o2) {
                    return null;
                }
            }
            return infinite;
        }

        public static string ToString(object o) {
            return ToString(DefaultContext.Default, o);
        }

        public static string ToString(CodeContext/*!*/ context, object o) {
            string x = o as string;
            PythonType dt;
            OldClass oc;
            if (x != null) return x;
            if (o == null) return "None";
            if (o is double) return DoubleOps.__str__(context, (double)o);
            if ((dt = o as PythonType) != null) return dt.__repr__(DefaultContext.Default);
            if ((oc = o as OldClass) != null) return oc.ToString();

            object value = PythonContext.InvokeUnaryOperator(context, UnaryOperators.String, o);
            string ret = value as string;
            if (ret == null) {
                Extensible<string> es = value as Extensible<string>;
                if (es == null) {
                    throw PythonOps.TypeError("expected str, got {0} from __str__", DynamicHelpers.GetPythonType(value).Name);
                }

                ret = es.Value;
            }
            return ret;
        }

        public static object Plus(object o) {
            object ret;

            if (o is int) return o;
            else if (o is double) return o;
            else if (o is BigInteger) return o;
            else if (o is Complex64) return o;
            else if (o is long) return o;
            else if (o is float) return o;
            else if (o is bool) return ScriptingRuntimeHelpers.Int32ToObject((bool)o ? 1 : 0);

            if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, Symbols.Positive, out ret) && 
                ret != NotImplementedType.Value) {
                return ret;
            }

            throw PythonOps.TypeError("bad operand type for unary +");
        }

        public static object Negate(object o) {
            if (o is int) return Int32Ops.Negate((int)o);
            else if (o is double) return DoubleOps.Negate((double)o);
            else if (o is long) return Int64Ops.Negate((long)o);
            else if (o is BigInteger) return BigIntegerOps.Negate((BigInteger)o);
            else if (o is Complex64) return -(Complex64)o;
            else if (o is float) return DoubleOps.Negate((float)o);
            else if (o is bool) return ScriptingRuntimeHelpers.Int32ToObject((bool)o ? -1 : 0);

            object ret;
            if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, Symbols.OperatorNegate, out ret) &&
                ret != NotImplementedType.Value) {
                return ret;
            }

            throw PythonOps.TypeError("bad operand type for unary -");
        }

        public static bool IsSubClass(PythonType/*!*/ c, PythonType/*!*/ typeinfo) {
            Assert.NotNull(c, typeinfo);

            if (c.OldClass != null) {
                return typeinfo.__subclasscheck__(c.OldClass);
            }

            return typeinfo.__subclasscheck__(c);
        }

        public static bool IsSubClass(PythonType c, object typeinfo) {
            if (c == null) throw PythonOps.TypeError("issubclass: arg 1 must be a class");
            if (typeinfo == null) throw PythonOps.TypeError("issubclass: arg 2 must be a class");

            PythonTuple pt = typeinfo as PythonTuple;
            if (pt != null) {
                // Recursively inspect nested tuple(s)
                foreach (object o in pt) {
                    try {
                        FunctionPushFrame();
                        if (IsSubClass(c, o)) {
                            return true;
                        }
                    } finally {
                        FunctionPopFrame();
                    }
                }
                return false;
            }

            OldClass oc = typeinfo as OldClass;
            if (oc != null) {
                return c.IsSubclassOf(oc.TypeObject);
            }

            Type t = typeinfo as Type;
            if (t != null) {
                typeinfo = DynamicHelpers.GetPythonTypeFromType(t);
            }

            object bases;
            PythonType dt = typeinfo as PythonType;
            if (dt == null) {
                if (!PythonOps.TryGetBoundAttr(typeinfo, Symbols.Bases, out bases)) {
                    //!!! deal with classes w/ just __bases__ defined.
                    throw PythonOps.TypeErrorForBadInstance("issubclass(): {0} is not a class nor a tuple of classes", typeinfo);
                }

                IEnumerator ie = PythonOps.GetEnumerator(bases);
                while (ie.MoveNext()) {
                    PythonType baseType = ie.Current as PythonType;

                    if (baseType == null) {
                        OldClass ocType = ie.Current as OldClass;
                        if (ocType == null) throw PythonOps.TypeError("expected type, got {0}", DynamicHelpers.GetPythonType(ie.Current));

                        baseType = ocType.TypeObject;
                    }

                    if (c.IsSubclassOf(baseType)) return true;
                }
                return false;
            }

            return IsSubClass(c, dt);
        }

        public static bool IsInstance(object o, PythonType typeinfo) {
            if (typeinfo.__instancecheck__(o)) {
                return true;
            }

            return IsInstanceDynamic(o, typeinfo, DynamicHelpers.GetPythonType(o));
        }

        public static bool IsInstance(object o, PythonTuple typeinfo) {
            foreach (object type in typeinfo) {
                try {
                    PythonOps.FunctionPushFrame();
                    if (type is PythonType) {
                        if (IsInstance(o, (PythonType)type)) {
                            return true;
                        }
                    } else if (type is PythonTuple) {
                        if (IsInstance(o, (PythonTuple)type)) {
                            return true;
                        }
                    } else if (IsInstance(o, type)) {
                        return true;
                    }
                } finally {
                    PythonOps.FunctionPopFrame();
                }
            }
            return false;
        }

        public static bool IsInstance(object o, object typeinfo) {
            if (typeinfo == null) throw PythonOps.TypeError("isinstance: arg 2 must be a class, type, or tuple of classes and types");

            PythonTuple tt = typeinfo as PythonTuple;
            if (tt != null) {
                return IsInstance(o, tt);
            }

            if (typeinfo is OldClass) {
                // old instances are strange - they all share a common type
                // of instance but they can "be subclasses" of other
                // OldClass's.  To check their types we need the actual
                // instance.
                OldInstance oi = o as OldInstance;
                if (oi != null) return oi._class.IsSubclassOf(typeinfo);
            }

            PythonType odt = DynamicHelpers.GetPythonType(o);
            if (IsSubClass(odt, typeinfo)) {
                return true;
            }

            return IsInstanceDynamic(o, typeinfo);
        }

        private static bool IsInstanceDynamic(object o, object typeinfo) {
            return IsInstanceDynamic(o, typeinfo, DynamicHelpers.GetPythonType(o));
        }

        private static bool IsInstanceDynamic(object o, object typeinfo, PythonType odt) {
            if (o is IPythonObject || o is OldInstance) {
                object cls;
                if (PythonOps.TryGetBoundAttr(o, Symbols.Class, out cls) &&
                    (!object.ReferenceEquals(odt, cls))) {
                    return IsSubclassSlow(cls, typeinfo);
                }
            }
            return false;
        }

        private static bool IsSubclassSlow(object cls, object typeinfo) {
            Debug.Assert(typeinfo != null);
            if (cls == null) return false;

            // Same type
            if (cls.Equals(typeinfo)) {
                return true;
            }

            // Get bases
            object bases;
            if (!PythonOps.TryGetBoundAttr(cls, Symbols.Bases, out bases)) {
                return false;   // no bases, cannot be subclass
            }
            PythonTuple tbases = bases as PythonTuple;
            if (tbases == null) {
                return false;   // not a tuple, cannot be subclass
            }

            foreach (object baseclass in tbases) {
                if (IsSubclassSlow(baseclass, typeinfo)) return true;
            }

            return false;
        }
        
        public static object OnesComplement(object o) {
            if (o is int) return ~(int)o;
            if (o is long) return ~(long)o;
            if (o is BigInteger) return ~((BigInteger)o);
            if (o is bool) return ScriptingRuntimeHelpers.Int32ToObject((bool)o ? -2 : -1);

            object ret;
            if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, Symbols.OperatorOnesComplement, out ret) &&
                ret != NotImplementedType.Value)
                return ret;


            throw PythonOps.TypeError("bad operand type for unary ~");
        }

        public static object Not(object o) {
            return IsTrue(o) ? ScriptingRuntimeHelpers.False : ScriptingRuntimeHelpers.True;
        }

        public static object Is(object x, object y) {
            return x == y ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
        }

        public static bool IsRetBool(object x, object y) {
            return x == y;
        }

        public static object IsNot(object x, object y) {
            return x != y ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
        }

        public static bool IsNotRetBool(object x, object y) {
            return x != y;
        }

        internal delegate T MultiplySequenceWorker<T>(T self, int count);

        /// <summary>
        /// Wraps up all the semantics of multiplying sequences so that all of our sequences
        /// don't duplicate the same logic.  When multiplying sequences we need to deal with
        /// only multiplying by valid sequence types (ints, not floats), support coercion
        /// to integers if the type supports it, not multiplying by None, and getting the
        /// right semantics for multiplying by negative numbers and 1 (w/ and w/o subclasses).
        /// 
        /// This function assumes that it is only called for case where count is not implicitly
        /// coercible to int so that check is skipped.
        /// </summary>
        internal static object MultiplySequence<T>(MultiplySequenceWorker<T> multiplier, T sequence, Index count, bool isForward) {
            if (isForward && count != null) {
                object ret;
                if (PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default, count.Value, sequence, Symbols.OperatorReverseMultiply, out ret)) {
                    if (ret != NotImplementedType.Value) return ret;
                }
            }

            int icount = GetSequenceMultiplier(sequence, count.Value);
            
            if (icount < 0) icount = 0;
            return multiplier(sequence, icount);
        }

        internal static int GetSequenceMultiplier(object sequence, object count) {
            int icount;
            if (!Converter.TryConvertToIndex(count, out icount)) {
                PythonTuple pt = null;
                if (count is OldInstance || !DynamicHelpers.GetPythonType(count).IsSystemType) {
                    pt = Builtin.TryCoerce(DefaultContext.Default, count, sequence) as PythonTuple;
                }

                if (pt == null || !Converter.TryConvertToIndex(pt[0], out icount)) {
                    throw TypeError("can't multiply sequence by non-int of type '{0}'", DynamicHelpers.GetPythonType(count).Name);
                }
            }
            return icount;
        }
        
        public static object Equal(CodeContext/*!*/ context, object x, object y) {
            PythonContext pc = PythonContext.GetContext(context);
            return pc.EqualSite.Target(pc.EqualSite, x, y);
        }

        public static bool EqualRetBool(object x, object y) {
            //TODO just can't seem to shake these fast paths
            if (x is int && y is int) { return ((int)x) == ((int)y); }
            if (x is string && y is string) { return ((string)x).Equals((string)y); }

            return DynamicHelpers.GetPythonType(x).EqualRetBool(x, y);
        }

        public static bool EqualRetBool(CodeContext/*!*/ context, object x, object y) {
            // TODO: use context

            //TODO just can't seem to shake these fast paths
            if (x is int && y is int) { return ((int)x) == ((int)y); }
            if (x is string && y is string) { return ((string)x).Equals((string)y); }

            return DynamicHelpers.GetPythonType(x).EqualRetBool(x, y); 
        }

        public static int Compare(object x, object y) {
            return Compare(DefaultContext.Default, x, y);
        }

        public static int Compare(CodeContext/*!*/ context, object x, object y) {
            if (x == y) return 0;

            return DynamicHelpers.GetPythonType(x).Compare(x, y);
        }

        public static object CompareEqual(int res) {
            return res == 0 ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
        }

        public static object CompareNotEqual(int res) {
            return res == 0 ? ScriptingRuntimeHelpers.False : ScriptingRuntimeHelpers.True;
        }

        public static object CompareGreaterThan(int res) {
            return res > 0 ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
        }

        public static object CompareGreaterThanOrEqual(int res) {
            return res >= 0 ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
        }

        public static object CompareLessThan(int res) {
            return res < 0 ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
        }

        public static object CompareLessThanOrEqual(int res) {
            return res <= 0 ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
        }

        public static bool CompareTypesEqual(CodeContext/*!*/ context, object x, object y) {
            if (x == null && y == null) return true;
            if (x == null) return false;
            if (y == null) return false;

            if (DynamicHelpers.GetPythonType(x) == DynamicHelpers.GetPythonType(y)) {
                // avoid going to the ID dispenser if we have the same types...
                return x == y;
            }

            return PythonOps.CompareTypesWorker(context, false, x, y) == 0;
        }

        public static bool CompareTypesNotEqual(CodeContext/*!*/ context, object x, object y) {
            return PythonOps.CompareTypesWorker(context, false, x, y) != 0;
        }

        public static bool CompareTypesGreaterThan(CodeContext/*!*/ context, object x, object y) {
            return PythonOps.CompareTypes(context, x, y) > 0;
        }

        public static bool CompareTypesLessThan(CodeContext/*!*/ context, object x, object y) {
            return PythonOps.CompareTypes(context, x, y) < 0;
        }

        public static bool CompareTypesGreaterThanOrEqual(CodeContext/*!*/ context, object x, object y) {
            return PythonOps.CompareTypes(context, x, y) >= 0;
        }

        public static bool CompareTypesLessThanOrEqual(CodeContext/*!*/ context, object x, object y) {
            return PythonOps.CompareTypes(context, x, y) <= 0;
        }

        public static int CompareTypesWorker(CodeContext/*!*/ context, bool shouldWarn, object x, object y) {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            string name1, name2;
            int diff;

            if (DynamicHelpers.GetPythonType(x) != DynamicHelpers.GetPythonType(y)) {
                if (shouldWarn && PythonContext.GetContext(context).PythonOptions.WarnPy3k) {
                    PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "comparing unequal types not supported in 3.x");
                }

                if (x.GetType() == typeof(OldInstance)) {
                    name1 = ((OldInstance)x)._class.__name__;
                    if (y.GetType() == typeof(OldInstance)) {
                        name2 = ((OldInstance)y)._class.__name__;
                    } else {
                        // old instances are always less than new-style classes
                        return -1;
                    }
                } else if (y.GetType() == typeof(OldInstance)) {
                    // old instances are always less than new-style classes
                    return 1;
                } else {
                    name1 = PythonTypeOps.GetName(x);
                    name2 = PythonTypeOps.GetName(y);
                }
                diff = String.CompareOrdinal(name1, name2);
                if (diff == 0) {
                    // if the types are different but have the same name compare based upon their types. 
                    diff = (int)(IdDispenser.GetId(DynamicHelpers.GetPythonType(x)) - IdDispenser.GetId(DynamicHelpers.GetPythonType(y)));
                }
            } else {
                diff = (int)(IdDispenser.GetId(x) - IdDispenser.GetId(y));
            }

            if (diff < 0) return -1;
            if (diff == 0) return 0;
            return 1;
        }

        public static int CompareTypes(CodeContext/*!*/ context, object x, object y) {
            return CompareTypesWorker(context, true, x, y);
        }

        public static object GreaterThanHelper(CodeContext/*!*/ context, object self, object other) {
            return InternalCompare(context, PythonOperationKind.GreaterThan, self, other);
        }

        public static object LessThanHelper(CodeContext/*!*/ context, object self, object other) {
            return InternalCompare(context, PythonOperationKind.LessThan, self, other);
        }

        public static object GreaterThanOrEqualHelper(CodeContext/*!*/ context, object self, object other) {
            return InternalCompare(context, PythonOperationKind.GreaterThanOrEqual, self, other);
        }

        public static object LessThanOrEqualHelper(CodeContext/*!*/ context, object self, object other) {
            return InternalCompare(context, PythonOperationKind.LessThanOrEqual, self, other);
        }

        internal static object InternalCompare(CodeContext/*!*/ context, PythonOperationKind op, object self, object other) {
            object ret;
            if (PythonTypeOps.TryInvokeBinaryOperator(context, self, other, Symbols.OperatorToSymbol(op), out ret))
                return ret;

            return NotImplementedType.Value;
        }

        public static int CompareToZero(object value) {
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                if (val > 0) return 1;
                if (val < 0) return -1;
                return 0;
            }
            throw PythonOps.TypeErrorForBadInstance("an integer is required (got {0})", value);
        }

        public static int CompareArrays(object[] data0, int size0, object[] data1, int size1) {
            int size = Math.Min(size0, size1);
            for (int i = 0; i < size; i++) {
                int c = PythonOps.Compare(data0[i], data1[i]);
                if (c != 0) return c;
            }
            if (size0 == size1) return 0;
            return size0 > size1 ? +1 : -1;
        }

        public static bool ArraysEqual(object[] data0, int size0, object[] data1, int size1) {
            if (size0 != size1) {
                return false;
            }
            for (int i = 0; i < size0; i++) {
                if (data0[i] != null) {
                    if (!EqualRetBool(data0[i], data1[i])) {
                        return false;
                    }
                } else if (data1[i] != null) {
                    return false;
                }
            }
            return true;
        }

        public static object PowerMod(CodeContext/*!*/ context, object x, object y, object z) {
            object ret;
            if (z == null) {
                return PythonContext.GetContext(context).Operation(PythonOperationKind.Power, x, y);
            }
            if (x is int && y is int && z is int) {
                ret = Int32Ops.Power((int)x, (int)y, (int)z);
                if (ret != NotImplementedType.Value) return ret;
            } else if (x is BigInteger) {
                ret = BigIntegerOps.Power((BigInteger)x, y, z);
                if (ret != NotImplementedType.Value) return ret;
            }

            if (x is Complex64 || y is Complex64 || z is Complex64) {
                throw PythonOps.ValueError("complex modulo");
            }

            if (PythonTypeOps.TryInvokeTernaryOperator(context, x, y, z, Symbols.OperatorPower, out ret)) {
                if(ret != NotImplementedType.Value) {
                    return ret;
                } else if (!IsNumericObject(y) || !IsNumericObject(z)) {
                    // special error message in this case...
                    throw TypeError("pow() 3rd argument not allowed unless all arguments are integers");
                }
            }

            throw PythonOps.TypeErrorForBinaryOp("power with modulus", x, y);
        }

        public static ICollection GetCollection(object o) {
            ICollection ret = o as ICollection;
            if (ret != null) return ret;

            List<object> al = new List<object>();
            IEnumerator e = GetEnumerator(o);
            while (e.MoveNext()) al.Add(e.Current);
            return al;
        }

        public static IEnumerator GetEnumerator(object o) {
            IEnumerator ie;
            if (!TryGetEnumerator(DefaultContext.Default, o, out ie)) {
                throw PythonOps.TypeError("{0} is not enumerable", PythonTypeOps.GetName(o));
            }
            return ie;
        }

        public static IEnumerator GetEnumeratorForUnpack(CodeContext/*!*/ context, object enumerable) {
            IEnumerator enumerator;
            if (!TryGetEnumerator(context, enumerable, out enumerator)) {
                throw PythonOps.TypeError("'{0}' object is not iterable", PythonTypeOps.GetName(enumerable));
            }

            return enumerator;
        }

        public static long Id(object o) {
            return IdDispenser.GetId(o);
        }

        public static string HexId(object o) {
            return string.Format("0x{0:X16}", Id(o));
        }
        
        // For hash operators, it's essential that:
        //  Cmp(x,y)==0 implies hash(x) == hash(y)
        //
        // Equality is a language semantic determined by the Python's numerical Compare() ops 
        // in IronPython.Runtime.Operations namespaces.
        // For example, the CLR compares float(1.0) and int32(1) as different, but Python
        // compares them as equal. So Hash(1.0f) and Hash(1) must be equal.
        //
        // Python allows an equality relationship between int, double, BigInteger, and complex.
        // So each of these hash functions must be aware of their possible equality relationships
        // and hash appropriately.
        //
        // Types which differ in hashing from .NET have __hash__ functions defined in their
        //  ops classes which do the appropriate hashing.        
        public static int Hash(CodeContext/*!*/ context, object o) {
            return PythonContext.GetContext(context).Hash(o);
        }

        public static object Hex(object o) {
            if (o is int) return Int32Ops.__hex__((int)o);
            else if (o is BigInteger) return BigIntegerOps.__hex__((BigInteger)o);

            object hex;
            if(PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default,
                o,
                Symbols.ConvertToHex,
                out hex)) {            
                if (!(hex is string) && !(hex is ExtensibleString))
                    throw PythonOps.TypeError("hex expected string type as return, got '{0}'", PythonTypeOps.GetName(hex));

                return hex;
            }
            throw TypeError("hex() argument cannot be converted to hex");
        }

        public static object Oct(object o) {
            if (o is int) {
                return Int32Ops.__oct__((int)o);
            } else if (o is BigInteger) {
                return BigIntegerOps.__oct__((BigInteger)o);
            }

            object octal;

            if(PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default,
                o,
                Symbols.ConvertToOctal,
                out octal)) {            
                if (!(octal is string) && !(octal is ExtensibleString))
                    throw PythonOps.TypeError("hex expected string type as return, got '{0}'", PythonTypeOps.GetName(octal));

                return octal;
            }
            throw TypeError("oct() argument cannot be converted to octal");
        }

        public static int Length(object o) {
            string s = o as String;
            if (s != null) return s.Length;

            ICollection ic = o as ICollection;
            if (ic != null) return ic.Count;

            object len = PythonContext.InvokeUnaryOperator(DefaultContext.Default, UnaryOperators.Length, o, "len() of unsized object");
            
            int res;
            if (len is int) {
                res = (int)len;
            } else {
                res = Converter.ConvertToInt32(len);
            }

            if (res < 0) {
                throw PythonOps.ValueError("__len__ should return >= 0, got {0}", res);
            }
            return res;
        }

        public static object CallWithContext(CodeContext/*!*/ context, object func, params object[] args) {
            return PythonCalls.Call(context, func, args);
        }

        /// <summary>
        /// Supports calling of functions that require an explicit 'this'
        /// Currently, we check if the function object implements the interface 
        /// that supports calling with 'this'. If not, the 'this' object is dropped
        /// and a normal call is made.
        /// </summary>
        public static object CallWithContextAndThis(CodeContext/*!*/ context, object func, object instance, params object[] args) {
            // drop the 'this' and make the call
            return CallWithContext(context, func, args);            
        }

        public static object ToPythonType(PythonType dt) {
            if (dt != null) {
                return ((object)dt.OldClass) ?? ((object)dt);
            }
            return null;
        }

        public static object CallWithArgsTupleAndContext(CodeContext/*!*/ context, object func, object[] args, object argsTuple) {
            PythonTuple tp = argsTuple as PythonTuple;
            if (tp != null) {
                object[] nargs = new object[args.Length + tp.__len__()];
                for (int i = 0; i < args.Length; i++) nargs[i] = args[i];
                for (int i = 0; i < tp.__len__(); i++) nargs[i + args.Length] = tp[i];
                return CallWithContext(context, func, nargs);
            }

            List allArgs = PythonOps.MakeEmptyList(args.Length + 10);
            allArgs.AddRange(args);
            IEnumerator e = PythonOps.GetEnumerator(argsTuple);
            while (e.MoveNext()) allArgs.AddNoLock(e.Current);

            return CallWithContext(context, func, allArgs.GetObjectArray());
        }
       
        public static object CallWithArgsTupleAndKeywordDictAndContext(CodeContext/*!*/ context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            IDictionary kws = kwDict as IDictionary;
            if (kws == null && kwDict != null) throw PythonOps.TypeError("argument after ** must be a dictionary");

            if ((kws == null || kws.Count == 0) && names.Length == 0) {
                List<object> largs = new List<object>(args);
                if (argsTuple != null) {
                    foreach(object arg in PythonOps.GetCollection(argsTuple))
                        largs.Add(arg);
                }
                return CallWithContext(context, func, largs.ToArray());
            } else {
                List<object> largs;

                if (argsTuple != null && args.Length == names.Length) {
                    PythonTuple tuple = argsTuple as PythonTuple;
                    if (tuple == null) tuple = new PythonTuple(argsTuple);

                    largs = new List<object>(tuple);
                    largs.AddRange(args);
                } else {
                    largs = new List<object>(args);
                    if (argsTuple != null) {
                        largs.InsertRange(args.Length - names.Length, PythonTuple.Make(argsTuple));
                    }
                }

                List<string> lnames = new List<string>(names);

                if (kws != null) {
                    IDictionaryEnumerator ide = kws.GetEnumerator();
                    while (ide.MoveNext()) {
                        lnames.Add((string)ide.Key);
                        largs.Add(ide.Value);
                    }
                }

                return PythonCalls.CallWithKeywordArgs(context, func, largs.ToArray(), lnames.ToArray());
            }
        }

        public static object CallWithKeywordArgs(CodeContext/*!*/ context, object func, object[] args, string[] names) {
            return PythonCalls.CallWithKeywordArgs(context, func, args, names);
        }

        public static object CallWithArgsTuple(object func, object[] args, object argsTuple) {
            PythonTuple tp = argsTuple as PythonTuple;
            if (tp != null) {
                object[] nargs = new object[args.Length + tp.__len__()];
                for (int i = 0; i < args.Length; i++) nargs[i] = args[i];
                for (int i = 0; i < tp.__len__(); i++) nargs[i + args.Length] = tp[i];
                return PythonCalls.Call(func, nargs);
            }

            List allArgs = PythonOps.MakeEmptyList(args.Length + 10);
            allArgs.AddRange(args);
            IEnumerator e = PythonOps.GetEnumerator(argsTuple);
            while (e.MoveNext()) allArgs.AddNoLock(e.Current);

            return PythonCalls.Call(func, allArgs.GetObjectArray());
        }
        
        public static object GetIndex(CodeContext/*!*/ context, object o, object index) {
            PythonContext pc = PythonContext.GetContext(context);
            return pc.GetIndexSite.Target(pc.GetIndexSite, o, index);
        }
        
        public static bool TryGetBoundAttr(object o, SymbolId name, out object ret) {
            return TryGetBoundAttr(DefaultContext.Default, o, name, out ret);
        }

        class AttrKey : IEquatable<AttrKey> {
            private Type _type;
            private SymbolId _name;

            public AttrKey(Type type, SymbolId name) {
                _type = type;
                _name = name;
            }

            #region IEquatable<AttrKey> Members

            public bool Equals(AttrKey other) {
                if (other == null) return false;

                return _type == other._type && _name == other._name;
            }

            #endregion

            public override bool Equals(object obj) {
                return Equals(obj as AttrKey);
            }

            public override int GetHashCode() {
                return _type.GetHashCode() ^ _name.GetHashCode();
            }
        }

        public static void SetAttr(CodeContext/*!*/ context, object o, SymbolId name, object value) {
            PythonContext.GetContext(context).SetAttr(context, o, name, value);
        }

        public static bool TryGetBoundAttr(CodeContext/*!*/ context, object o, SymbolId name, out object ret) {
            return DynamicHelpers.GetPythonType(o).TryGetBoundAttr(context, o, name, out ret);
        }

        public static void DeleteAttr(CodeContext/*!*/ context, object o, SymbolId name) {
            PythonContext.GetContext(context).DeleteAttr(context, o, name);
        }

        public static bool HasAttr(CodeContext/*!*/ context, object o, SymbolId name) {
            object dummy;
            try {
                return TryGetBoundAttr(context, o, name, out dummy);
            } catch (SystemExitException) {
                throw;
            } catch (KeyboardInterruptException) {
                // we don't catch ThreadAbortException because it will
                // automatically re-throw on it's own.
                throw;
            } catch {
                return false;
            }
        }
        
        public static object GetBoundAttr(CodeContext/*!*/ context, object o, SymbolId name) {
            object ret;
            if (!DynamicHelpers.GetPythonType(o).TryGetBoundAttr(context, o, name, out ret)) {
                if (o is OldClass) {
                    throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'",
                        ((OldClass)o).__name__, SymbolTable.IdToString(name));
                } else {
                    throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", DynamicHelpers.GetPythonType(o).Name, SymbolTable.IdToString(name));
                }
            }

            return ret;
        }

        public static void ObjectSetAttribute(CodeContext/*!*/ context, object o, SymbolId name, object value) {
            if (!DynamicHelpers.GetPythonType(o).TrySetNonCustomMember(context, o, name, value))
                throw AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetPythonType(o), name);
        }

        public static void ObjectDeleteAttribute(CodeContext/*!*/ context, object o, SymbolId name) {
            object dummy;
            if (!PythonTypeOps.TryInvokeBinaryOperator(context, o, name, Symbols.DeleteDescriptor, out dummy)) {                
                throw AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetPythonType(o), name);
            }
        }

        public static object ObjectGetAttribute(CodeContext/*!*/ context, object o, SymbolId name) {
            OldClass oc = o as OldClass;
            if (oc != null) {
                return oc.GetMember(context, name);                
            }

            object value;
            if (DynamicHelpers.GetPythonType(o).TryGetNonCustomMember(context, o, name, out value)) {
                return value;
            }            

            throw PythonOps.AttributeErrorForMissingAttribute(DynamicHelpers.GetPythonType(o).Name, name);
        }

        public static IList<object> GetAttrNames(CodeContext/*!*/ context, object o) {

            IMembersList memList = o as IMembersList;
            if (memList != null) {
                return memList.GetMemberNames(context);
            }

            List res;
            if (o is IDynamicMetaObjectProvider) {
                res = new List();

                PythonContext pc = PythonContext.GetContext(context);
                foreach (object x in pc.MemberNamesSite.Target.Invoke(pc.MemberNamesSite, context, o)) {
                    res.AddNoLock(x);
                }
            } else {
                res = DynamicHelpers.GetPythonType(o).GetMemberNames(context, o);

#if !SILVERLIGHT
                if (o != null && ComOps.IsComObject(o)) {
                    foreach (string name in Microsoft.Scripting.ComBinder.GetDynamicMemberNames(o)) {
                        if (!res.Contains(name)) {
                            res.AddNoLock(name);
                        }
                    }
                }
#endif
            }

            //!!! ugly, we need to check for non-SymbolID keys
            IPythonObject dyno = o as IPythonObject;
            if (dyno != null) {
                IAttributesCollection iac = dyno.Dict;
                if (iac != null) {
                    foreach (object id in iac.Keys) {
                        if (!res.__contains__(id)) res.append(id);
                    }
                }
            }

            return res;
        }

        public static IDictionary<object, object> GetAttrDict(CodeContext/*!*/ context, object o) {
            IAttributesCollection iac = DynamicHelpers.GetPythonType(o).GetMemberDictionary(context, o);
            if (iac != null) {
                return iac.AsObjectKeyedDictionary();
            }
            throw PythonOps.AttributeErrorForMissingAttribute(PythonTypeOps.GetName(o), Symbols.Dict);
        }

        /// <summary>
        /// Called from generated code emitted by NewTypeMaker.
        /// </summary>
        public static void CheckInitializedAttribute(object o, object self, string name) {
            if (o == Uninitialized.Instance) {
                throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'",
                    DynamicHelpers.GetPythonType(self),
                    name);
            }
        }               

        /// <summary>
        /// Handles the descriptor protocol for user-defined objects that may implement __get__
        /// </summary>
        public static object GetUserDescriptor(object o, object instance, object context) {
            if (o != null && o.GetType() == typeof(OldInstance)) return o;   // only new-style classes can have descriptors
            if (o is IPythonObject) {
                // slow, but only encountred for user defined descriptors.
                PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "__get__");
                object ret;
                if (PythonContext.TryInvokeTernaryOperator(DefaultContext.Default,
                    TernaryOperators.GetDescriptor,
                    o,
                    instance,
                    context,
                    out ret)) {
                    return ret;
                }
            }

            return o;
        }

        /// <summary>
        /// Handles the descriptor protocol for user-defined objects that may implement __set__
        /// </summary>
        public static bool TrySetUserDescriptor(object o, object instance, object value) {
            if (o != null && o.GetType() == typeof(OldInstance)) return false;   // only new-style classes have descriptors

            // slow, but only encountred for user defined descriptors.
            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "__set__");
            
            object dummy;
            return PythonContext.TryInvokeTernaryOperator(DefaultContext.Default, 
                TernaryOperators.SetDescriptor, 
                o, 
                instance, 
                value, 
                out dummy);
        }

        /// <summary>
        /// Handles the descriptor protocol for user-defined objects that may implement __delete__
        /// </summary>
        public static bool TryDeleteUserDescriptor(object o, object instance) {
            if (o != null && o.GetType() == typeof(OldInstance)) return false;   // only new-style classes can have descriptors

            // slow, but only encountred for user defined descriptors.
            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "__delete__");

            object dummy;
            return PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default,
                o,
                instance,
                Symbols.DeleteDescriptor,
                out dummy);
        }

        public static object Invoke(CodeContext/*!*/ context, object target, SymbolId name, params object[] args) {
            return PythonCalls.Call(context, PythonOps.GetBoundAttr(context, target, name), args);
        }

        public static Delegate CreateDynamicDelegate(DynamicMethod meth, Type delegateType, object target) {
            // Always close delegate around its own instance of the frame
            return meth.CreateDelegate(delegateType, target);
        }

        public static double CheckMath(double v) {
            if (double.IsInfinity(v)) {
                throw PythonOps.OverflowError("math range error");
            } else if (double.IsNaN(v)) {
                throw PythonOps.ValueError("math domain error");
            } else {
                return v;
            }
        }

        public static object IsMappingType(CodeContext/*!*/ context, object o) {
            if (o is IDictionary || o is PythonDictionary || o is IDictionary<object, object> || o is IAttributesCollection) {
                return ScriptingRuntimeHelpers.True;
            }
            object getitem;
            if ((o is IPythonObject || o is OldInstance) && PythonOps.TryGetBoundAttr(context, o, Symbols.GetItem, out getitem)) {
                if (!PythonOps.IsClsVisible(context)) {
                    // in standard Python methods aren't mapping types, therefore
                    // if the user hasn't broken out of that box yet don't treat 
                    // them as mapping types.
                    if (o is BuiltinFunction) return ScriptingRuntimeHelpers.False;
                }
                return ScriptingRuntimeHelpers.True;
            }
            return ScriptingRuntimeHelpers.False;
        }

        public static int FixSliceIndex(int v, int len) {
            if (v < 0) v = len + v;
            if (v < 0) return 0;
            if (v > len) return len;
            return v;
        }

        public static void FixSlice(int length, object start, object stop, object step,
                                    out int ostart, out int ostop, out int ostep, out int ocount) {
            if (step == null) {
                ostep = 1;
            } else {
                ostep = Converter.ConvertToIndex(step);
                if (ostep == 0) {
                    throw PythonOps.ValueError("step cannot be zero");
                }
            }

            if (start == null) {
                ostart = ostep > 0 ? 0 : length - 1;
            } else {
                ostart = Converter.ConvertToIndex(start);
                if (ostart < 0) {
                    ostart += length;
                    if (ostart < 0) {
                        ostart = ostep > 0 ? Math.Min(length, 0) : Math.Min(length - 1, -1);
                    }
                } else if (ostart >= length) {
                    ostart = ostep > 0 ? length : length - 1;
                }
            }

            if (stop == null) {
                ostop = ostep > 0 ? length : -1;
            } else {
                ostop = Converter.ConvertToIndex(stop);
                if (ostop < 0) {
                    ostop += length;
                    if (ostop < 0) {
                        ostop = ostep > 0 ? Math.Min(length, 0) : Math.Min(length - 1, -1);
                    }
                } else if (ostop >= length) {
                    ostop = ostep > 0 ? length : length - 1;
                }
            }

            ocount = ostep > 0 ? (ostop - ostart + ostep - 1) / ostep
                               : (ostop - ostart + ostep + 1) / ostep;
        }

        public static int FixIndex(int v, int len) {
            if (v < 0) {
                v += len;
                if (v < 0) {
                    throw PythonOps.IndexError("index out of range: {0}", v - len);
                }
            } else if (v >= len) {
                throw PythonOps.IndexError("index out of range: {0}", v);
            }
            return v;
        }

        public static void InitializeForFinalization(CodeContext/*!*/ context, object newObject) {
            IWeakReferenceable iwr = newObject as IWeakReferenceable;
            Debug.Assert(iwr != null);

            InstanceFinalizer nif = new InstanceFinalizer(context, newObject);
            iwr.SetFinalizer(new WeakRefTracker(nif, nif));
        }

        private static object FindMetaclass(CodeContext/*!*/ context, PythonTuple bases, IAttributesCollection dict) {
            // If dict['__metaclass__'] exists, it is used. 
            object ret;
            if (dict.TryGetValue(Symbols.MetaClass, out ret) && ret != null) return ret;

            // Otherwise, if there is at least one base class, its metaclass is used
            for (int i = 0; i < bases.__len__(); i++) {
                if (!(bases[i] is OldClass)) return DynamicHelpers.GetPythonType(bases[i]);
            }           

            // Otherwise, if there's a global variable named __metaclass__, it is used.
            if (context.GlobalScope.TryLookupName(Symbols.MetaClass, out ret) && ret != null) {
                return ret;
            }

            //Otherwise, the classic metaclass (types.ClassType) is used.
            return TypeCache.OldInstance;
        }

        public static object MakeClass(object body, CodeContext/*!*/ parentContext, string name, object[] bases, string selfNames) {
            Func<CodeContext, CodeContext> func = body as Func<CodeContext, CodeContext>;
            if (func == null) {
                func = ((Compiler.LazyCode<Func<CodeContext, CodeContext>>)body).EnsureDelegate();
            }
            return MakeClass(parentContext, name, bases, selfNames, func(parentContext).Scope.Dict);
        }

        internal static object MakeClass(CodeContext context, string name, object[] bases, string selfNames, IAttributesCollection vars) {
            foreach (object dt in bases) {
                if (dt is TypeGroup) {
                    object[] newBases = new object[bases.Length];
                    for (int i = 0; i < bases.Length; i++) {
                        TypeGroup tc = bases[i] as TypeGroup;
                        if (tc != null) {
                            Type nonGenericType;
                            if (!tc.TryGetNonGenericType(out nonGenericType)) {
                                throw PythonOps.TypeError("cannot derive from open generic types " + Builtin.repr(context, tc).ToString());
                            }
                            newBases[i] = DynamicHelpers.GetPythonTypeFromType(nonGenericType);
                        } else {
                            newBases[i] = bases[i];
                        }
                    }
                    bases = newBases;
                    break;
                }
            }
            PythonTuple tupleBases = PythonTuple.MakeTuple(bases);

            object metaclass = FindMetaclass(context, tupleBases, vars);
            if (metaclass == TypeCache.OldInstance) {
                return new OldClass(name, tupleBases, vars, selfNames);
            } else if (metaclass == TypeCache.PythonType) {
                return PythonType.__new__(context, TypeCache.PythonType, name, tupleBases, vars);
            }

            // eg:
            // def foo(*args): print args            
            // __metaclass__ = foo
            // class bar: pass
            // calls our function...
            PythonContext pc = PythonContext.GetContext(context);

            return pc.MetaClassCallSite.Target(
                pc.MetaClassCallSite,
                context,
                metaclass,
                name,
                tupleBases,
                vars
            );
        }

        /// <summary>
        /// Python runtime helper for raising assertions. Used by AssertStatement.
        /// </summary>
        /// <param name="msg">Object representing the assertion message</param>
        public static void RaiseAssertionError(object msg) {
            if (msg == null) {
                throw PythonOps.AssertionError(String.Empty, ArrayUtils.EmptyObjects);
            } else {
                string message = PythonOps.ToString(msg);
                throw PythonOps.AssertionError("{0}", new object[] { message });
            }
                
        }

        /// <summary>
        /// Python runtime helper to create instance of Python List object.
        /// </summary>
        /// <returns>New instance of List</returns>
        public static List MakeList() {
            return new List();
        }

        /// <summary>
        /// Python runtime helper to create a populated instance of Python List object.
        /// </summary>
        public static List MakeList(params object[] items) {
            return new List(items);
        }

        /// <summary>
        /// Python runtime helper to create a populated instance of Python List object w/o
        /// copying the array contents.
        /// </summary>
        [NoSideEffects]
        public static List MakeListNoCopy(params object[] items) {
            return List.FromArrayNoCopy(items);
        }
        
        /// <summary>
        /// Python runtime helper to create a populated instance of Python List object.
        /// 
        /// List is populated by arbitrary user defined object.
        /// </summary>
        public static List MakeListFromSequence(object sequence) {
            return new List(sequence);
        }

        /// <summary>
        /// Python runtime helper to create an instance of Python List object.
        /// 
        /// List has the initial provided capacity.
        /// </summary>
        [NoSideEffects]
        public static List MakeEmptyList(int capacity) {
            return new List(capacity);
        }

        [NoSideEffects]
        public static List MakeEmptyListFromCode() {
            return List.FromArrayNoCopy(ArrayUtils.EmptyObjects);
        }

        /// <summary>
        /// Python runtime helper to create an instance of Tuple
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        [NoSideEffects]
        public static PythonTuple MakeTuple(params object[] items) {
            return PythonTuple.MakeTuple(items);
        }

        /// <summary>
        /// Python runtime helper to create an instance of Tuple
        /// </summary>
        /// <param name="items"></param>
        [NoSideEffects]
        public static PythonTuple MakeTupleFromSequence(object items) {
            return PythonTuple.Make(items);
        }

        /// <summary>
        /// Python Runtime Helper for enumerator unpacking (tuple assignments, ...)
        /// Creates enumerator from the input parameter e, and then extracts 
        /// expected number of values, returning them as array
        /// </summary>
        /// <param name="context">The code context of the AST getting enumerator values.</param>
        /// <param name="e">object to enumerate</param>
        /// <param name="expected">expected number of objects to extract from the enumerator</param>
        /// <returns>
        /// array of objects (.Lengh == expected) if exactly expected objects are in the enumerator.
        /// Otherwise throws exception
        /// </returns>
        public static object[] GetEnumeratorValues(CodeContext/*!*/ context, object e, int expected) {
            IEnumerator ie = PythonOps.GetEnumeratorForUnpack(context, e);

            int count = 0;
            object[] values = new object[expected];

            while (count < expected) {
                if (!ie.MoveNext()) {
                    throw PythonOps.ValueErrorForUnpackMismatch(expected, count);
                }
                values[count] = ie.Current;
                count++;
            }

            if (ie.MoveNext()) {
                throw PythonOps.ValueErrorForUnpackMismatch(expected, count + 1);
            }

            return values;
        }

        /// <summary>
        /// Python runtime helper to create instance of Slice object
        /// </summary>
        /// <param name="start">Start of the slice.</param>
        /// <param name="stop">End of the slice.</param>
        /// <param name="step">Step of the slice.</param>
        /// <returns>Slice</returns>
        public static Slice MakeSlice(object start, object stop, object step) {
            return new Slice(start, stop, step);
        }

        #region Standard I/O support

        public static void Write(CodeContext/*!*/ context, object f, string text) {
            PythonContext pc = PythonContext.GetContext(context);

            if (f == null) {
                f = pc.SystemStandardOut;
            }
            if (f == null || f == Uninitialized.Instance) {
                throw PythonOps.RuntimeError("lost sys.std_out");
            }

            PythonFile pf = f as PythonFile;
            if (pf != null) {
                // avoid spinning up a site in the normal case
                pf.write(text);
                return;
            }

            pc.WriteCallSite.Target(
                pc.WriteCallSite,
                context,
                GetBoundAttr(context, f, Symbols.ConsoleWrite), 
                text
            );
        }

        private static object ReadLine(CodeContext/*!*/ context, object f) {
            if (f == null || f == Uninitialized.Instance) throw PythonOps.RuntimeError("lost sys.std_in");
            return PythonOps.Invoke(context, f, Symbols.ConsoleReadLine);
        }

        public static void WriteSoftspace(CodeContext/*!*/ context, object f) {
            if (CheckSoftspace(f)) {
                SetSoftspace(f, ScriptingRuntimeHelpers.False);
                Write(context, f, " ");
            }
        }

        public static void SetSoftspace(object f, object value) {
            PythonOps.SetAttr(DefaultContext.Default, f, Symbols.Softspace, value);
        }

        public static bool CheckSoftspace(object f) {
            PythonFile pf = f as PythonFile;
            if (pf != null) {
                // avoid spinning up a site in the common case
                return pf.softspace;
            }

            object result;
            if (PythonOps.TryGetBoundAttr(f, Symbols.Softspace, out result)) {
                return PythonOps.IsTrue(result);
            }

            return false;
        }

        // Must stay here for now because libs depend on it.
        public static void Print(CodeContext/*!*/ context, object o) {
            PrintWithDest(context, PythonContext.GetContext(context).SystemStandardOut, o);
        }

        public static void PrintNoNewline(CodeContext/*!*/ context, object o) {
            PrintWithDestNoNewline(context, PythonContext.GetContext(context).SystemStandardOut, o);
        }

        public static void PrintWithDest(CodeContext/*!*/ context, object dest, object o) {
            PrintWithDestNoNewline(context, dest, o);
            Write(context, dest, "\n");
        }

        public static void PrintWithDestNoNewline(CodeContext/*!*/ context, object dest, object o) {
            WriteSoftspace(context, dest);
            Write(context, dest, o == null ? "None" : ToString(o));
        }

        public static object ReadLineFromSrc(CodeContext/*!*/ context, object src) {
            return ReadLine(context, src);
        }

        /// <summary>
        /// Prints newline into default standard output
        /// </summary>
        public static void PrintNewline(CodeContext/*!*/ context) {
            PrintNewlineWithDest(context, PythonContext.GetContext(context).SystemStandardOut);
        }

        /// <summary>
        /// Prints newline into specified destination. Sets softspace property to false.
        /// </summary>
        public static void PrintNewlineWithDest(CodeContext/*!*/ context, object dest) {
            PythonOps.Write(context, dest, "\n");
            PythonOps.SetSoftspace(dest, ScriptingRuntimeHelpers.False);
        }

        /// <summary>
        /// Prints value into default standard output with Python comma semantics.
        /// </summary>
        public static void PrintComma(CodeContext/*!*/ context, object o) {
            PrintCommaWithDest(context, PythonContext.GetContext(context).SystemStandardOut, o);
        }

        /// <summary>
        /// Prints value into specified destination with Python comma semantics.
        /// </summary>
        public static void PrintCommaWithDest(CodeContext/*!*/ context, object dest, object o) {
            PythonOps.WriteSoftspace(context, dest);
            string s = o == null ? "None" : PythonOps.ToString(o);

            PythonOps.Write(context, dest, s);
            PythonOps.SetSoftspace(dest, !s.EndsWith("\n"));
        }        

        /// <summary>
        /// Called from generated code when we are supposed to print an expression value
        /// </summary>
        public static void PrintExpressionValue(CodeContext/*!*/ context, object value) {
            PythonContext pc = PythonContext.GetContext(context);
            object dispHook = pc.GetSystemStateValue("displayhook");
            pc.CallWithContext(context, dispHook, value);
        }

#if !SILVERLIGHT
        public static void PrintException(CodeContext/*!*/ context, Exception/*!*/ exception, IConsole console) {
            PythonContext pc = PythonContext.GetContext(context);
            PythonTuple exInfo = GetExceptionInfoLocal(context, exception);
            pc.SetSystemStateValue("last_type", exInfo[0]);
            pc.SetSystemStateValue("last_value", exInfo[1]);
            pc.SetSystemStateValue("last_traceback", exInfo[2]);

            object exceptHook = pc.GetSystemStateValue("excepthook");
            BuiltinFunction bf = exceptHook as BuiltinFunction;
            if (console != null && bf != null && bf.DeclaringType == typeof(SysModule) && bf.Name == "excepthook") {
                // builtin except hook, display it to the console which may do nice coloring
                console.WriteLine(pc.FormatException(exception), Style.Error);
            } else {
                // user defined except hook or no console
                try {
                    PythonCalls.Call(context, exceptHook, exInfo[0], exInfo[1], exInfo[2]);
                } catch (Exception e) {
                    PrintWithDest(context, pc.SystemStandardError, "Error in sys.excepthook:");
                    PrintWithDest(context, pc.SystemStandardError, pc.FormatException(e));
                    PrintNewlineWithDest(context, pc.SystemStandardError);

                    PrintWithDest(context, pc.SystemStandardError, "Original exception was:");
                    PrintWithDest(context, pc.SystemStandardError, pc.FormatException(exception));
                }
            }
        }
#endif

        #endregion

        #region Import support

        /// <summary>
        /// Called from generated code for:
        /// 
        /// import spam.eggs
        /// </summary>
        [ProfilerTreatsAsExternal]
        public static object ImportTop(CodeContext/*!*/ context, string fullName, int level) {
            return Importer.Import(context, fullName, null, level);
        }

        /// <summary>
        /// Python helper method called from generated code for:
        /// 
        /// import spam.eggs as ham
        /// </summary>
        [ProfilerTreatsAsExternal]
        public static object ImportBottom(CodeContext/*!*/ context, string fullName, int level) {
            object module = Importer.Import(context, fullName, null, level);

            if (fullName.IndexOf('.') >= 0) {
                // Extract bottom from the imported module chain
                string[] parts = fullName.Split('.');

                for (int i = 1; i < parts.Length; i++) {
                    module = PythonOps.GetBoundAttr(context, module, SymbolTable.StringToId(parts[i]));
                }
            }
            return module;
        }

        /// <summary>
        /// Called from generated code for:
        /// 
        /// from spam import eggs1, eggs2 
        /// </summary>
        [ProfilerTreatsAsExternal]
        public static object ImportWithNames(CodeContext/*!*/ context, string fullName, string[] names, int level) {
            return Importer.Import(context, fullName, PythonTuple.MakeTuple(names), level);
        }


        /// <summary>
        /// Imports one element from the module in the context of:
        /// 
        /// from module import a, b, c, d
        /// 
        /// Called repeatedly for all elements being imported (a, b, c, d above)
        /// </summary>
        public static object ImportFrom(CodeContext/*!*/ context, object module, string name) {
            return Importer.ImportFrom(context, module, name);
        }

        /// <summary>
        /// Called from generated code for:
        /// 
        /// from spam import *
        /// </summary>
        [ProfilerTreatsAsExternal]
        public static void ImportStar(CodeContext/*!*/ context, string fullName, int level) {
            object newmod = Importer.Import(context, fullName, PythonTuple.MakeTuple("*"), level);

            Scope scope = newmod as Scope;
            NamespaceTracker nt = newmod as NamespaceTracker;
            PythonType pt = newmod as PythonType;

            if (pt != null &&
                !pt.UnderlyingSystemType.IsEnum &&
                (!pt.UnderlyingSystemType.IsAbstract || !pt.UnderlyingSystemType.IsSealed)) {
                // from type import * only allowed on static classes (and enums)
                throw PythonOps.ImportError("no module named {0}", pt.Name);
            }

            IEnumerator exports;
            object all;
            bool filterPrivates = false;

            // look for __all__, if it's defined then use that to get the attribute names,
            // otherwise get all the names and filter out members starting w/ _'s.
            if (PythonOps.TryGetBoundAttr(context, newmod, Symbols.All, out all)) {
                exports = PythonOps.GetEnumerator(all);
            } else {
                exports = PythonOps.GetAttrNames(context, newmod).GetEnumerator();
                filterPrivates = true;
            }

            // iterate through the names and populate the scope with the values.
            while (exports.MoveNext()) {
                string name = exports.Current as string;
                if (name == null) {
                    throw PythonOps.TypeErrorForNonStringAttribute();
                } else if (filterPrivates && name.Length > 0 && name[0] == '_') {
                    continue;
                }

                SymbolId fieldId = SymbolTable.StringToId(name);

                // we special case several types to avoid one-off code gen of dynamic sites                
                if (scope != null) {
                    context.Scope.SetName(fieldId, scope.Dict[fieldId]);
                } else if (nt != null) {
                    object value = ReflectedPackageOps.GetCustomMember(context, nt, name);
                    if (value != OperationFailed.Value) {
                        context.Scope.SetName(fieldId, value);
                    }
                } else if (pt != null) {
                    PythonTypeSlot pts;
                    object value;
                    if (pt.TryResolveSlot(context, fieldId, out pts) &&
                        pts.TryGetValue(context, null, pt, out value)) {
                        context.Scope.SetName(fieldId, value);
                    }
                } else {
                    // not a known type, we'll do use a site to do the get...
                    context.Scope.SetName(fieldId, PythonOps.GetBoundAttr(context, newmod, fieldId));
                }
            }
        }        

        #endregion

        #region Exec

        /// <summary>
        /// Unqualified exec statement support.
        /// A Python helper which will be called for the statement:
        /// 
        /// exec code
        /// </summary>
        [ProfilerTreatsAsExternal]
        public static void UnqualifiedExec(CodeContext/*!*/ context, object code) {
            IAttributesCollection locals = null;
            IAttributesCollection globals = null;

            // if the user passes us a tuple we'll extract the 3 values out of it            
            PythonTuple codeTuple = code as PythonTuple;
            if (codeTuple != null && codeTuple.__len__() > 0 && codeTuple.__len__() <= 3) {
                code = codeTuple[0];

                if (codeTuple.__len__() > 1 && codeTuple[1] != null) {
                    globals = codeTuple[1] as IAttributesCollection;
                    if (globals == null) throw PythonOps.TypeError("globals must be dictionary or none");
                }

                if (codeTuple.__len__() > 2 && codeTuple[2] != null) {
                    locals = codeTuple[2] as IAttributesCollection;
                    if (locals == null) throw PythonOps.TypeError("locals must be dictionary or none");
                } else {
                    locals = globals;
                }
            }

            QualifiedExec(context, code, globals, locals);
        }

        /// <summary>
        /// Qualified exec statement support,
        /// Python helper which will be called for the statement:
        /// 
        /// exec code in globals [, locals ]
        /// </summary>
        [ProfilerTreatsAsExternal]
        public static void QualifiedExec(CodeContext/*!*/ context, object code, IAttributesCollection globals, object locals) {
            PythonFile pf;
            Stream cs;

            var pythonContext = PythonContext.GetContext(context);

            bool noLineFeed = true;

            // TODO: use ContentProvider?
            if ((pf = code as PythonFile) != null) {
                List lines = pf.readlines();

                StringBuilder fullCode = new StringBuilder();
                for (int i = 0; i < lines.__len__(); i++) {
                    fullCode.Append(lines[i]);
                }

                code = fullCode.ToString();
            } else if ((cs = code as Stream) != null) {

                using (StreamReader reader = new StreamReader(cs)) { // TODO: encoding? 
                    code = reader.ReadToEnd();
                }

                noLineFeed = false;
            }

            string strCode = code as string;

            if (strCode != null) {
                SourceUnit source;

                if (noLineFeed) {
                    source = pythonContext.CreateSourceUnit(new NoLineFeedSourceContentProvider(strCode), null, SourceCodeKind.Statements);
                } else {
                    source = pythonContext.CreateSnippet(strCode, SourceCodeKind.Statements);
                }

                PythonCompilerOptions compilerOptions = Builtin.GetRuntimeGeneratedCodeCompilerOptions(context, true, 0);

                // do interpretation only on strings -- not on files, streams, or code objects
                code = new FunctionCode(pythonContext.CompilePythonCode(Compiler.CompilationMode.Lookup, source, compilerOptions, ThrowingErrorSink.Default));
            }

            FunctionCode fc = code as FunctionCode;
            if (fc == null) {
                throw PythonOps.TypeError("arg 1 must be a string, file, Stream, or code object");
            }

            if (locals == null) locals = globals;
            if (globals == null) globals = new PythonDictionary(new GlobalScopeDictionaryStorage(context.Scope));

            if (locals != null && PythonOps.IsMappingType(context, locals) != ScriptingRuntimeHelpers.True) {
                throw PythonOps.TypeError("exec: arg 3 must be mapping or None");
            }

            Scope execScope = Builtin.GetExecEvalScope(context, globals, Builtin.GetAttrLocals(context, locals), true, false);
            fc.Call(execScope);
        }

        #endregion        

        public static IEnumerator GetEnumeratorForIteration(CodeContext/*!*/ context, object enumerable) {
            IEnumerator enumerator;
            if (!TryGetEnumerator(context, enumerable, out enumerator)) {
                return ThrowTypeErrorForBadIteration(context, enumerable);
            }

            return enumerator;
        }

        public static IEnumerator ThrowTypeErrorForBadIteration(CodeContext context, object enumerable) {
            throw PythonOps.TypeError(
                "iteration over non-sequence of type {0}",
                PythonOps.Repr(context, DynamicHelpers.GetPythonType(enumerable))
            );
        }

        internal static bool TryGetEnumerator(CodeContext/*!*/ context, object enumerable, out IEnumerator enumerator) {
            string str = enumerable as string;
            if (str != null) {
                enumerator = StringEnumerator(str);
                return true;
            }

            IEnumerable enumer = enumerable as IEnumerable;
            if (enumer != null) {
                enumerator = enumer.GetEnumerator();
                return true;
            }

            enumerator = enumerable as IEnumerator;
            if (enumerator != null) {
                return true;
            }

            IEnumerable ie;
            if (!PythonContext.GetContext(context).TryConvertToIEnumerable(enumerable, out ie)) {
                return false;
            }

            enumerator = ie.GetEnumerator();
            return true;
        }

        public static IEnumerator<string> StringEnumerator(string str) {
            return StringOps.StringEnumerator(str);
        }

        #region Exception handling

        // The semantics here are:
        // 1. Each thread has a "current exception", which is returned as a tuple by sys.exc_info().
        // 2. The current exception is set on encountering an except block, even if the except block doesn't
        //    match the exception.
        // 3. Each function on exit (either via exception, return, or yield) will restore the "current exception" 
        //    to the value it had on function-entry. 
        //
        // So common codegen would be:
        // 
        // function() {
        //   $save = SaveCurrentException();
        //   try { 
        //      def foo():
        //              try:
        //              except:
        //                  SetCurrentException($exception)
        //                  <except body>
        //   
        //   finally {
        //      RestoreCurrentException($save)
        //   }

        // Called at the start of the except handlers to set the current exception. 
        public static object SetCurrentException(CodeContext/*!*/ context, Exception/*!*/ clrException) {
            Assert.NotNull(clrException);

            // we need to extract before we check because ThreadAbort.ExceptionState is cleared after
            // we reset the abort.
            object res = PythonExceptions.ToPython(clrException);

#if !SILVERLIGHT
            // Check for thread abort exceptions.
            // This is necessary to be able to catch python's KeyboardInterrupt exceptions.
            // CLR restrictions require that this must be called from within a catch block.  This gets
            // called even if we aren't going to handle the exception - we'll just reset the abort 
            ThreadAbortException tae = clrException as ThreadAbortException;
            if (tae != null && tae.ExceptionState is KeyboardInterruptException) {
                Thread.ResetAbort();
            }
#endif
            
            RawException = clrException;
            return res;
        }

        public static void BuildExceptionInfo(CodeContext/*!*/ context, Exception clrException) {
            ExceptionHelpers.AssociateDynamicStackFrames(clrException);
            GetExceptionInfo(context);
            ClearDynamicStackFrames();
        }

        // Clear the current exception. Most callers should restore the exception.
        // This is mainly for sys.exc_clear()        
        public static void ClearCurrentException() {
            RestoreCurrentException(null);
        }

        // Called by code-gen to save it. Codegen just needs to pass this back to RestoreCurrentException.
        public static Exception SaveCurrentException() {
            return RawException;
        }

        // Called at function exit (like popping). Pass value from SaveCurrentException.
        public static void RestoreCurrentException(Exception clrException) {
            RawException = clrException;
        } 

        public static object CheckException(object exception, object test) {
            Debug.Assert(exception != null);

            StringException strex;
            ObjectException objex;

            if (test is PythonTuple) {
                // we handle multiple exceptions, we'll check them one at a time.
                PythonTuple tt = test as PythonTuple;
                for (int i = 0; i < tt.__len__(); i++) {
                    object res = CheckException(exception, tt[i]);
                    if (res != null) return res;
                }
            } else if ((strex = exception as StringException) != null) {
                // catching a string
                if (test.GetType() != typeof(string)) return null;
                if (strex.Message == (string)test) {
                    if (strex.Value == null) return strex.Message;
                    return strex.Value;
                }
                return null;
            } else if ((objex = exception as ObjectException) != null) {
                if (PythonOps.IsSubClass(objex.Type, test)) {
                    return objex.Instance;
                }
                return null;
            } else if (test is OldClass) {
                if (PythonOps.IsInstance(exception, test)) {
                    // catching a Python type.
                    return exception;
                }
            } else if (test is PythonType) {                
                if (PythonOps.IsSubClass(test as PythonType, TypeCache.BaseException)) {
                    // catching a Python exception type explicitly.
                    if (PythonOps.IsInstance(exception, test)) return exception;
                } else if (PythonOps.IsSubClass(test as PythonType, DynamicHelpers.GetPythonTypeFromType(typeof(Exception)))) {
                    // catching a CLR exception type explicitly.
                    Exception clrEx = PythonExceptions.ToClr(exception);
                    if (PythonOps.IsInstance(clrEx, test)) return clrEx;
                }
            }

            return null;
        }

        private static TraceBack CreateTraceBack(Exception e) {
            // user provided trace back
            if (e.Data.Contains(typeof(TraceBack))) {
                return (TraceBack)e.Data[typeof(TraceBack)];
            }

            DynamicStackFrame[] frames = ScriptingRuntimeHelpers.GetDynamicStackFrames(e, false);
            TraceBack tb = null;
            for (int i = 0; i < frames.Length; i++) {
                DynamicStackFrame frame = frames[i];

                string name = frame.GetMethodName();
                if (name.IndexOf('#') > 0) {
                    // dynamic method, strip the trailing id...
                    name = name.Substring(0, name.IndexOf('#'));
                }
                CodeContext context = frame.CodeContext;
                if (context == null) {
                    context = DefaultContext.Default;
                }
                PythonFunction fx = new PythonFunction(context, null, new FunctionInfo(name, null, ArrayUtils.EmptyStrings, FunctionAttributes.None, 0, String.Empty, null, false), "", ArrayUtils.EmptyObjects, null);

                TraceBackFrame tbf = new TraceBackFrame(
                    new PythonDictionary(new GlobalScopeDictionaryStorage(context.Scope)),
                    context.Scope.Dict,
                    fx.func_code);

                fx.func_code.SetFilename(frame.GetFileName());
                fx.func_code.SetLineNumber(frame.GetFileLineNumber());
                tb = new TraceBack(tb, tbf);
                tb.SetLine(frame.GetFileLineNumber());
            }

            e.Data[typeof(TraceBack)] = tb;
            return tb;
        }

        /// <summary>
        /// Get an exception tuple for the "current" exception. This is used for sys.exc_info()
        /// </summary>
        public static PythonTuple GetExceptionInfo(CodeContext/*!*/ context) {
            return GetExceptionInfoLocal(context, RawException);
        }

        /// <summary>
        /// Get an exception tuple for a given exception. This is like the inverse of MakeException.
        /// </summary>
        /// <param name="context">the code context</param>
        /// <param name="ex">the exception to create a tuple for.</param>
        /// <returns>a tuple of (type, value, traceback)</returns>
        /// <remarks>This is called directly by the With statement so that it can get an exception tuple
        /// in its own private except handler without disturbing the thread-wide sys.exc_info(). </remarks>
        public static PythonTuple/*!*/ GetExceptionInfoLocal(CodeContext/*!*/ context, Exception ex) {
            if (ex == null) {
                return PythonTuple.MakeTuple(null, null, null);
            }

            object pyExcep = PythonExceptions.ToPython(ex);

            TraceBack tb = CreateTraceBack(ex);
            PythonContext pc = PythonContext.GetContext(context);
            pc.SystemExceptionTraceBack = tb;

            StringException se = pyExcep as StringException;
            if (se == null) {
                object excType = PythonOps.GetBoundAttr(context, pyExcep, Symbols.Class);
                pc.SystemExceptionType = excType;
                pc.SystemExceptionValue = pyExcep;

                return PythonTuple.MakeTuple(
                    excType,
                    pyExcep,
                    tb);
            }

            // string exceptions are special...  there tuple looks
            // like string, argument, traceback instead of
            //      type,   instance, traceback
            pc.SystemExceptionType = pyExcep;
            pc.SystemExceptionValue = se.Value;

            return PythonTuple.MakeTuple(
                pyExcep,
                se.Value,
                tb);
        }

        /// <summary>
        /// helper function for re-raised exceptions.
        /// </summary>
        public static Exception MakeRethrownException(CodeContext/*!*/ context) {
            PythonTuple t = GetExceptionInfo(context);
            
            Exception e = MakeExceptionWorker(context, t[0], t[1], t[2], true);
            e.Data.Remove(typeof(TraceBack));
            ExceptionHelpers.UpdateForRethrow(e);
            return e;
        }

        /// <summary>
        /// helper function for non-re-raise exceptions.
        /// 
        /// type is the type of exception to throw or an instance.  If it 
        /// is an instance then value should be null.  
        /// 
        /// If type is a type then value can either be an instance of type,
        /// a Tuple, or a single value.  This case is handled by EC.CreateThrowable.
        /// </summary>
        public static Exception MakeException(CodeContext/*!*/ context, object type, object value, object traceback) {
            return MakeExceptionWorker(context, type, value, traceback, false);
        }

        private static Exception MakeExceptionWorker(CodeContext context, object type, object value, object traceback, bool forRethrow) {
            Exception throwable;
            PythonType pt;

            if (type is Exception) {
                throwable = type as Exception;
            } else if (type is PythonExceptions.BaseException) {
                throwable = PythonExceptions.ToClr(type);
            } else if ((pt = type as PythonType) != null && typeof(PythonExceptions.BaseException).IsAssignableFrom(pt.UnderlyingSystemType)) {
                throwable = PythonExceptions.CreateThrowableForRaise(context, pt, value);
            } else if (type is string) {
                PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "raising a string exception is deprecated");
                throwable = new StringException(type.ToString(), value);
            } else if (type is OldClass) {
                if (value == null) {
                    throwable = new OldInstanceException((OldInstance)PythonCalls.Call(context, type));
                } else {
                    throwable = PythonExceptions.CreateThrowableForRaise(context, (OldClass)type, value);
                }
            } else if (type is OldInstance) {
                throwable = new OldInstanceException((OldInstance)type);
            } else {
                throwable = MakeExceptionTypeError(type);
            }

            IDictionary dict = throwable.Data;

            if (traceback != null) {
                if (!forRethrow) {
                    TraceBack tb = traceback as TraceBack;
                    if (tb == null) throw PythonOps.TypeError("traceback argument must be a traceback object");

                    dict[typeof(TraceBack)] = tb;
                }
            } else if (dict.Contains(typeof(TraceBack))) {
                dict.Remove(typeof(TraceBack));
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Exceptions, throwable);

            return throwable;
        }

        public static Exception CreateThrowable(PythonType type, params object[] args) {
            return PythonExceptions.CreateThrowable(type, args);
        }

        public static void ClearDynamicStackFrames() {
            ExceptionHelpers.DynamicStackFrames = null;
        }

        public static List<DynamicStackFrame> GetAndClearDynamicStackFrames() {
            List<DynamicStackFrame> res = ExceptionHelpers.DynamicStackFrames;
            ClearDynamicStackFrames();
            return res;
        }

        public static void SetDynamicStackFrames(List<DynamicStackFrame> frames) {
            ExceptionHelpers.DynamicStackFrames = frames;
        }

        #endregion

        public static string[] GetFunctionSignature(PythonFunction function) {
            return new string[] { function.GetSignatureString() };
        }

        public static PythonDictionary CopyAndVerifyDictionary(PythonFunction function, IDictionary dict) {            
            foreach (object o in dict.Keys) {
                if (!(o is string)) {
                    throw TypeError("{0}() keywords most be strings", function.__name__);
                }
            }
            return new PythonDictionary(dict);
        }

        public static PythonDictionary/*!*/ CopyAndVerifyUserMapping(PythonFunction/*!*/ function, object dict) {
            return UserMappingToPythonDictionary(function.Context, dict, function.func_name);
        }

        public static PythonDictionary UserMappingToPythonDictionary(CodeContext context, object dict, string funcName) {
            // call dict.keys()
            object keys;
            if (!PythonTypeOps.TryInvokeUnaryOperator(context, dict, Symbols.Keys, out keys)) {
                throw PythonOps.TypeError("{0}() argument after ** must be a mapping, not {1}",
                    funcName,
                    DynamicHelpers.GetPythonType(dict).Name);
            }

            PythonDictionary res = new PythonDictionary();

            // enumerate the keys getting their values
            IEnumerator enumerator = GetEnumerator(keys);
            while (enumerator.MoveNext()) {
                object o = enumerator.Current;
                string s = o as string;
                if (s == null) {
                    Extensible<string> es = o as Extensible<string>;
                    if (es == null) {
                        throw PythonOps.TypeError("{0}() keywords most be strings, not {0}",
                            funcName,
                            DynamicHelpers.GetPythonType(dict).Name);
                    }

                    s = es.Value;
                }

                res[o] = PythonOps.GetIndex(context, dict, o);
            }

            return res;
        }

        public static PythonDictionary CopyAndVerifyPythonDictionary(PythonFunction function, PythonDictionary dict) {
            if (dict._storage.HasNonStringAttributes()) {
                throw TypeError("{0}() keywords most be strings", function.__name__);
            }

            return new PythonDictionary(dict);
        }

        public static object ExtractDictionaryArgument(PythonFunction function, string name, int argCnt, IAttributesCollection dict) {
            object val;
            if (dict.TryGetObjectValue(name, out val)) {
                dict.RemoveObjectKey(name);
                return val;
            }

            throw PythonOps.TypeError("{0}() takes exactly {1} non-keyword arguments ({2} given)", 
                function.__name__, 
                function.NormalArgumentCount,
                argCnt);
        }

        public static void AddDictionaryArgument(PythonFunction function, string name, object value, IAttributesCollection dict) {
            if (dict.ContainsObjectKey(name)) {
                throw MultipleKeywordArgumentError(function, name);
            }

            dict.AddObjectKey(name, value);
        }

        public static void VerifyUnduplicatedByPosition(PythonFunction function, string name, int position, int listlen) {
            if (listlen > 0 && listlen > position) {
                throw MultipleKeywordArgumentError(function, name);
            }
        }

        public static List CopyAndVerifyParamsList(PythonFunction function, object list) {
            return new List(list);
        }

        public static PythonTuple GetOrCopyParamsTuple(object input) {
            if (input.GetType() == typeof(PythonTuple)) {
                return (PythonTuple)input;
            }

            return PythonTuple.Make(input);
        }

        public static object ExtractParamsArgument(PythonFunction function, int argCnt, List list) {
            if (list.__len__() != 0) {
                return list.pop(0);
            }

            throw function.BadArgumentError(argCnt);
        }

        public static void AddParamsArguments(List list, params object[] args) {
            for (int i = 0; i < args.Length; i++) {
                list.insert(i, args[i]);
            }
        }

        /// <summary>
        /// Extracts an argument from either the dictionary or params
        /// </summary>
        public static object ExtractAnyArgument(PythonFunction function, string name, int argCnt, List list, IDictionary dict) {
            object val;
            if (dict.Contains(name)) {
                if (list.__len__() != 0) {
                    throw MultipleKeywordArgumentError(function, name);
                }
                val = dict[name];
                dict.Remove(name);
                return val;
            }

            if (list.__len__() != 0) {
                return list.pop(0);
            }

            if (function.ExpandDictPosition == -1 && dict.Count > 0) {   
                // python raises an error for extra splatted kw keys before missing arguments.
                // therefore we check for this in the error case here.
                foreach (string x in dict.Keys) {
                    bool found = false;
                    foreach (string y in function.ArgNames) {
                        if (x == y) {
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        throw UnexpectedKeywordArgumentError(function, x);
                    }
                }
            }

            throw BinderOps.TypeErrorForIncorrectArgumentCount(
                function.__name__,
                function.NormalArgumentCount,
                function.Defaults.Length,
                argCnt,
                function.ExpandListPosition != -1,
                dict.Count > 0);
        }

        public static object GetParamsValueOrDefault(PythonFunction function, int index, List extraArgs) {
            if (extraArgs.__len__() > 0) {
                return extraArgs.pop(0);
            }

            return function.Defaults[index];
        }

        public static object GetFunctionParameterValue(PythonFunction function, int index, string name, List extraArgs, PythonDictionary dict) {
            if (extraArgs != null && extraArgs.__len__() > 0) {
                return extraArgs.pop(0);
            }

            object val;
            if (dict != null && dict.TryRemoveValue(name, out val)) {
                return val;
            }

            return function.Defaults[index];
        }

        public static void CheckParamsZero(PythonFunction function, List extraArgs) {
            if (extraArgs.__len__() != 0) {
                throw function.BadArgumentError(extraArgs.__len__() + function.NormalArgumentCount);
            }
        }

        public static void CheckUserParamsZero(PythonFunction function, object sequence) {
            int len = PythonOps.Length(sequence); 
            if(len != 0) {
                throw function.BadArgumentError(len + function.NormalArgumentCount);
            }
        }

        public static void CheckDictionaryZero(PythonFunction function, IDictionary dict) {
            if (dict.Count != 0) {
                IDictionaryEnumerator ie = dict.GetEnumerator();                
                ie.MoveNext();

                throw UnexpectedKeywordArgumentError(function, (string)ie.Key);
            }
        }

        public static bool CheckDictionaryMembers(IAttributesCollection dict, string[] names) {
            if (dict.Count != names.Length) {
                return false;
            }

            foreach (string name in names) {
                if (!dict.ContainsKey(SymbolTable.StringToId(name))) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a new array the values set to Uninitialized.Instance.  The array
        /// is large enough to hold for all of the slots allocated for the type and
        /// its sub types.
        /// </summary>
        public static object[] InitializeUserTypeSlots(PythonType/*!*/ type) {
            if (type.SlotCount == 0) {
                // if we later set the weak reference obj we'll create the array
                return null;
            }

            // weak reference is stored at end of slots
            object[] res = new object[type.SlotCount + 1];  
            for (int i = 0; i < res.Length - 1; i++) {
                res[i] = Uninitialized.Instance;
            }
            return res;
        }

        public static bool IsClsVisible(CodeContext/*!*/ context) {            
            PythonModule module = PythonContext.GetModule(context);
            return module == null || module.ShowCls;
        }

        public static object GetInitMember(CodeContext/*!*/ context, PythonType type, object instance) {
            object value;
            bool res = type.TryGetNonCustomBoundMember(context, instance, Symbols.Init, out value);
            Debug.Assert(res);

            return value;
        }

        public static object GetInitSlotMember(CodeContext/*!*/ context, PythonType type, PythonTypeSlot slot, object instance) {
            object value;
            if (!slot.TryGetValue(context, instance, type, out value)) {
                throw PythonOps.TypeError("bad __init__");
            }

            return value;
        }

        public static object GetMixedMember(CodeContext/*!*/ context, PythonType type, object instance, SymbolId name) {
            foreach (PythonType t in type.ResolutionOrder) {
                if (t.IsOldClass) {
                    OldClass oc = (OldClass)ToPythonType(t);
                    object ret;
                    if (oc.__dict__._storage.TryGetValue(name, out ret)) {
                        if (instance != null) return oc.GetOldStyleDescriptor(context, ret, instance, oc);
                        return ret;
                    }
                } else {
                    PythonTypeSlot dts;
                    if (t.TryLookupSlot(context, name, out dts)) {
                        object ret;
                        if (dts.TryGetValue(context, instance, type, out ret)) {
                            return ret;
                        }
                        return dts;
                    }
                }
            }

            throw AttributeErrorForMissingAttribute(type, name);
        }

        #region Slicing support

        /// <summary>
        /// Helper to determine if the value is a simple numeric type (int or big int or bool) - used for OldInstance
        /// deprecated form of slicing.
        /// </summary>
        public static bool IsNumericObject(object value) {
            return value is int || value is Extensible<int> || value is BigInteger || value is Extensible<BigInteger> || value is bool;
        }

        /// <summary>
        /// Helper to determine if the type is a simple numeric type (int or big int or bool) - used for OldInstance
        /// deprecated form of slicing.
        /// </summary>
        internal static bool IsNumericType(Type t) {
            return IsNonExtensibleNumericType(t) ||
                t.IsSubclassOf(typeof(Extensible<int>)) ||
                t.IsSubclassOf(typeof(Extensible<BigInteger>));
        }

        /// <summary>
        /// Helper to determine if the type is a simple numeric type (int or big int or bool) but not a subclass
        /// </summary>
        internal static bool IsNonExtensibleNumericType(Type t) {
            return t == typeof(int) ||
                t == typeof(bool) ||
                t == typeof(BigInteger);
        }

        /// <summary>
        /// For slicing.  Fixes up a BigInteger and returns an integer w/ the length of the
        /// object added if the value is negative.
        /// </summary>
        public static int NormalizeBigInteger(object self, BigInteger bi, ref Nullable<int> length) {
            int val;
            if (bi < BigInteger.Zero) {
                GetLengthOnce(self, ref length);

                if (bi.AsInt32(out val)) {
                    Debug.Assert(length.HasValue);
                    return val + length.Value;
                } else {
                    return -1;
                }
            } else if (bi.AsInt32(out val)) {
                return val;
            }

            return Int32.MaxValue;
        }
        
        /// <summary>
        /// For slicing.  Gets the length of the object, used to only get the length once.
        /// </summary>
        public static int GetLengthOnce(object self, ref Nullable<int> length) {
            if (length != null) return length.Value;

            length = PythonOps.Length(self);
            return length.Value;
        }

        #endregion
        
        public static ReflectedEvent.BoundEvent MakeBoundEvent(ReflectedEvent eventObj, object instance, Type type) {
            return new ReflectedEvent.BoundEvent(eventObj, instance, DynamicHelpers.GetPythonTypeFromType(type));
        }

        /// <summary>
        /// Helper method for DynamicSite rules that check the version of their dynamic object
        /// TODO - Remove this method for more direct field accesses
        /// </summary>
        /// <param name="o"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool CheckTypeVersion(object o, int version) {
            IPythonObject po = o as IPythonObject;
            if (po == null) return false;

            return po.PythonType.Version == version;
        }

        public static bool CheckSpecificTypeVersion(PythonType type, int version) {
            return type.Version == version;
        }

        #region Conversion helpers 
        
        internal static MethodInfo GetConversionHelper(string name, ConversionResultKind resultKind) {
            MethodInfo res;
            switch (resultKind) {
                case ConversionResultKind.ExplicitCast:
                case ConversionResultKind.ImplicitCast:
                    res = typeof(PythonOps).GetMethod("Throwing" + name); 
                    break;
                case ConversionResultKind.ImplicitTry:
                case ConversionResultKind.ExplicitTry:
                    res = typeof(PythonOps).GetMethod("NonThrowing" + name); 
                    break;
                default: throw new InvalidOperationException();
            }
            Debug.Assert(res != null);
            return res;
        }

        public static IEnumerable OldInstanceConvertToIEnumerableNonThrowing(CodeContext/*!*/ context, OldInstance/*!*/ self) {
            object callable;
            if (self.TryGetBoundCustomMember(context, Symbols.Iterator, out callable)) {
                return CreatePythonEnumerable(self);
            } else if (self.TryGetBoundCustomMember(context, Symbols.GetItem, out callable)) {
                return CreateItemEnumerable(callable, PythonContext.GetContext(context).GetItemCallSite);
            }

            return null;
        }

        public static IEnumerable/*!*/ OldInstanceConvertToIEnumerableThrowing(CodeContext/*!*/ context, OldInstance/*!*/ self) {
            IEnumerable res = OldInstanceConvertToIEnumerableNonThrowing(context, self);
            if (res == null) {
                throw TypeErrorForTypeMismatch("IEnumerable", self);               
            }

            return res;
        }

        public static IEnumerable<T> OldInstanceConvertToIEnumerableOfTNonThrowing<T>(CodeContext/*!*/ context, OldInstance/*!*/ self) {
            object callable;
            if (self.TryGetBoundCustomMember(context, Symbols.Iterator, out callable)) {
                return new IEnumerableOfTWrapper<T>(CreatePythonEnumerable(self));
            } else if (self.TryGetBoundCustomMember(context, Symbols.GetItem, out callable)) {
                return new IEnumerableOfTWrapper<T>(CreateItemEnumerable(callable, PythonContext.GetContext(context).GetItemCallSite));
            }

            return null;
        }

        public static IEnumerable<T>/*!*/ OldInstanceConvertToIEnumerableOfTThrowing<T>(CodeContext/*!*/ context, OldInstance/*!*/ self) {
            IEnumerable<T> res = OldInstanceConvertToIEnumerableOfTNonThrowing<T>(context, self);
            if (res == null) {
                throw TypeErrorForTypeMismatch("IEnumerable[T]", self);
            }

            return res;
        }

        public static IEnumerator OldInstanceConvertToIEnumeratorNonThrowing(CodeContext/*!*/ context, OldInstance/*!*/ self) {
            object callable;
            if (self.TryGetBoundCustomMember(context, Symbols.Iterator, out callable)) {
                return CreatePythonEnumerator(self);
            } else if (self.TryGetBoundCustomMember(context, Symbols.GetItem, out callable)) {
                return CreateItemEnumerator(callable, PythonContext.GetContext(context).GetItemCallSite);
            }

            return null;
        }

        public static IEnumerator/*!*/ OldInstanceConvertToIEnumeratorThrowing(CodeContext/*!*/ context, OldInstance/*!*/ self) {
            IEnumerator res = OldInstanceConvertToIEnumeratorNonThrowing(context, self);
            if (res == null) {
                throw TypeErrorForTypeMismatch("IEnumerator", self);
            }

            return res;
        }

        public static bool? OldInstanceConvertToBoolNonThrowing(CodeContext/*!*/ context, OldInstance/*!*/ oi) {
            object value;
            if (oi.TryGetBoundCustomMember(context, Symbols.NonZero, out value)) {
                object res = NonThrowingConvertToNonZero(PythonCalls.Call(context, value));
                if (res is int) {
                    return ((int)res) != 0;
                } else if (res is bool) {
                    return (bool)res;
                }
            } else if (oi.TryGetBoundCustomMember(context, Symbols.Length, out value)) {
                int res;
                if (Converter.TryConvertToInt32(PythonCalls.Call(context, value), out res)) {
                    return res != 0;
                }
            }

            return null;
        }

        public static object OldInstanceConvertToBoolThrowing(CodeContext/*!*/ context, OldInstance/*!*/ oi) {
            object value;
            if (oi.TryGetBoundCustomMember(context, Symbols.NonZero, out value)) {
                return ThrowingConvertToNonZero(PythonCalls.Call(context, value));
            } else if (oi.TryGetBoundCustomMember(context, Symbols.Length, out value)) {
                return PythonContext.GetContext(context).ConvertToInt32(PythonCalls.Call(context, value)) != 0;
            }

            return null;
        }

        public static object OldInstanceConvertNonThrowing(CodeContext/*!*/ context, OldInstance/*!*/ oi, SymbolId conversion) {
            object value;
            if (oi.TryGetBoundCustomMember(context, conversion, out value)) {
                if (conversion == Symbols.ConvertToInt) {
                    return NonThrowingConvertToInt(PythonCalls.Call(context, value));
                } else if (conversion == Symbols.ConvertToLong) {
                    return NonThrowingConvertToLong(PythonCalls.Call(context, value));
                } else if (conversion == Symbols.ConvertToFloat) {
                    return NonThrowingConvertToFloat(PythonCalls.Call(context, value));
                } else if (conversion == Symbols.ConvertToComplex) {
                    return NonThrowingConvertToComplex(PythonCalls.Call(context, value));
                } else if (conversion == Symbols.String) {
                    return NonThrowingConvertToString(PythonCalls.Call(context, value));
                } else {
                    Debug.Assert(false);
                }
            } else if (conversion == Symbols.ConvertToComplex) {
                object res = OldInstanceConvertNonThrowing(context, oi, Symbols.ConvertToFloat);
                if (res == null) {
                    return null;
                }

                return Converter.ConvertToComplex64(res);
            }

            return null;
        }

        public static object OldInstanceConvertThrowing(CodeContext/*!*/ context, OldInstance/*!*/ oi, SymbolId conversion) {
            object value;
            if (oi.TryGetBoundCustomMember(context, conversion, out value)) {
                if (conversion == Symbols.ConvertToInt) {
                    return ThrowingConvertToInt(PythonCalls.Call(context, value));
                } else if (conversion == Symbols.ConvertToLong) {
                    return ThrowingConvertToLong(PythonCalls.Call(context, value));
                } else if (conversion == Symbols.ConvertToFloat) {
                    return ThrowingConvertToFloat(PythonCalls.Call(context, value));
                } else if (conversion == Symbols.ConvertToComplex) {
                    return ThrowingConvertToComplex(PythonCalls.Call(context, value));
                } else if (conversion == Symbols.String) {
                    return ThrowingConvertToString(PythonCalls.Call(context, value));
                } else {
                    Debug.Assert(false);
                }
            } else if (conversion == Symbols.ConvertToComplex) {
                return OldInstanceConvertThrowing(context, oi, Symbols.ConvertToFloat);
            }

            return null;
        }

        public static object ConvertFloatToComplex(object value) {
            if (value == null) {
                return null;
            }

            double d = (double)value;
            return new Complex64(d, 0.0);
        }

        internal static bool CheckingConvertToInt(object value) {
            return value is int || value is BigInteger || value is Extensible<int> || value is Extensible<BigInteger>;
        }

        internal static bool CheckingConvertToLong(object value) {
            return CheckingConvertToInt(value);
        }

        internal static bool CheckingConvertToFloat(object value) {
            return value is double || (value != null && value is Extensible<double>);
        }

        internal static bool CheckingConvertToComplex(object value) {
            return value is Complex64 || value is Extensible<Complex64> || CheckingConvertToInt(value) || CheckingConvertToFloat(value);
        }

        internal static bool CheckingConvertToString(object value) {
            return value is string || value is Extensible<string>;
        }

        public static bool CheckingConvertToNonZero(object value) {
            return value is bool || value is int;
        }

        public static object NonThrowingConvertToInt(object value) {
            if (!CheckingConvertToInt(value)) return null;
            return value;
        }

        public static object NonThrowingConvertToLong(object value) {
            if (!CheckingConvertToInt(value)) return null;
            return value;
        }

        public static object NonThrowingConvertToFloat(object value) {
            if (!CheckingConvertToFloat(value)) return null;
            return value;
        }

        public static object NonThrowingConvertToComplex(object value) {
            if (!CheckingConvertToComplex(value)) return null;
            return value;                            
        }

        public static object NonThrowingConvertToString(object value) {
            if (!CheckingConvertToString(value)) return null;
            return value;
        }

        public static object NonThrowingConvertToNonZero(object value) {
            if (!CheckingConvertToNonZero(value)) return null;
            return value;
        }

        public static object ThrowingConvertToInt(object value) {
            if (!CheckingConvertToInt(value)) throw TypeError(" __int__ returned non-int (type {0})", PythonTypeOps.GetName(value));
            return value;
        }

        public static object ThrowingConvertToFloat(object value) {
            if (!CheckingConvertToFloat(value)) throw TypeError(" __float__ returned non-float (type {0})", PythonTypeOps.GetName(value));
            return value;
        }

        public static object ThrowingConvertToComplex(object value) {
            if (!CheckingConvertToComplex(value)) throw TypeError(" __complex__ returned non-complex (type {0})", PythonTypeOps.GetName(value));
            return value;
        }

        public static object ThrowingConvertToLong(object value) {
            if (!CheckingConvertToComplex(value)) throw TypeError(" __long__ returned non-long (type {0})", PythonTypeOps.GetName(value));
            return value;
        }

        public static object ThrowingConvertToString(object value) {
            if (!CheckingConvertToString(value)) throw TypeError(" __str__ returned non-str (type {0})", PythonTypeOps.GetName(value));
            return value;
        }

        public static bool ThrowingConvertToNonZero(object value) {
            if (!CheckingConvertToNonZero(value)) throw TypeError("__nonzero__ should return bool or int, returned {0}", PythonTypeOps.GetName(value));
            if (value is bool) {
                return (bool)value;
            }

            return ((int)value) != 0;
        }
                
        #endregion

        public static bool SlotTryGetBoundValue(CodeContext/*!*/ context, PythonTypeSlot/*!*/ slot, object instance, PythonType owner, out object value) {
            return slot.TryGetValue(context, instance, owner, out value);
        }

        public static bool SlotTryGetValue(CodeContext/*!*/ context, PythonTypeSlot/*!*/ slot, object instance, PythonType owner, out object value) {
            return slot.TryGetValue(context, instance, owner, out value);
        }

        public static object SlotGetValue(CodeContext/*!*/ context, PythonTypeSlot/*!*/ slot, object instance, PythonType owner) {
            object value;
            if (!slot.TryGetValue(context, instance, owner, out value)) {
                throw new InvalidOperationException();
            }

            return value;
        }

        public static bool SlotTrySetValue(CodeContext/*!*/ context, PythonTypeSlot/*!*/ slot, object instance, PythonType owner, object value) {
            return slot.TrySetValue(context, instance, owner, value);
        }

        public static object SlotSetValue(CodeContext/*!*/ context, PythonTypeSlot/*!*/ slot, object instance, PythonType owner, object value) {
            if (!slot.TrySetValue(context, instance, owner, value)) {
                throw new InvalidOperationException();
            }

            return value;
        }

        public static bool SlotTryDeleteValue(CodeContext/*!*/ context, PythonTypeSlot/*!*/ slot, object instance, PythonType owner) {
            return slot.TryDeleteValue(context, instance, owner);
        }

        public static BuiltinFunction/*!*/ MakeBoundBuiltinFunction(BuiltinFunction/*!*/ function, object/*!*/ target) {
            return function.BindToInstance(target);
        }

        /// <summary>
        /// Called from generated code.  Gets a builtin function and the BuiltinFunctionData associated
        /// with the object.  Tests to see if the function is bound and has the same data for the generated
        /// rule.
        /// </summary>
        public static bool TestBoundBuiltinFunction(BuiltinFunction/*!*/ function, object data) {
            if (function.IsUnbound) {
                // not bound
                return false;
            }

            return function.TestData(data);
        }

        public static BuiltinFunction/*!*/ GetBuiltinMethodDescriptorTemplate(BuiltinMethodDescriptor/*!*/ descriptor) {
            return descriptor.Template;
        }

        public static int GetTypeVersion(PythonType type) {
            return type.Version;
        }

        public static bool TryResolveTypeSlot(CodeContext/*!*/ context, PythonType type, SymbolId name, out PythonTypeSlot slot) {
            return type.TryResolveSlot(context, name, out slot);
        }

        public static T[] ConvertTupleToArray<T>(PythonTuple tuple) {
            T[] res = new T[tuple.__len__()];
            for (int i = 0; i < tuple.__len__(); i++) {
                try {
                    res[i] = (T)tuple[i];
                } catch (InvalidCastException) {
                    res[i] = Converter.Convert<T>(tuple[i]);
                }
            }
            return res;
        }

        #region Function helpers

        public static PythonGenerator MakeGenerator(PythonFunction function, Microsoft.Scripting.Tuple data, object generatorCode) {
            PythonGeneratorNext next = generatorCode as PythonGeneratorNext;
            if (next == null) {
                next = ((LazyCode<PythonGeneratorNext>)generatorCode).EnsureDelegate();
            }

            return new PythonGenerator(function, next, data);
        }

        public static FunctionInfo MakeFunctionInfo(string name, object documentation, string[] argNames, FunctionAttributes flags, int lineNumber, string path) {
            return new FunctionInfo(name, documentation, argNames, flags, lineNumber, path, null, false);
        }

        [NoSideEffects]
        public static object MakeFunction(CodeContext/*!*/ context, Delegate target, FunctionInfo funcInfo, object modName, object[] defaults) {
            return new PythonFunction(context, target, funcInfo, modName, defaults, null);
        }

        public static CodeContext FunctionGetContext(PythonFunction func) {
            return func.Context;
        }

        public static object FunctionGetDefaultValue(PythonFunction func, int index) {
            return func.Defaults[index];
        }

        public static int FunctionGetCompatibility(PythonFunction func) {
            return func.FunctionCompatibility;
        }

        public static int FunctionGetID(PythonFunction func) {
            return func.FunctionID;
        }

        public static Delegate FunctionGetTarget(PythonFunction func) {
            return func.Target;
        }

        public static void FunctionPushFrame() {
            if (++PythonFunction.Depth > PythonFunction._MaximumDepth)
                throw PythonOps.RuntimeError("maximum recursion depth exceeded");
        }

        public static void FunctionPopFrame() {
            --PythonFunction.Depth;
        }

        public static bool ShouldEnforceRecursion() {
            return PythonFunction.EnforceRecursion;
        }

        #endregion

        public static object ReturnConversionResult(object value) {
            PythonTuple pt = value as PythonTuple;
            if (pt != null) {
                return pt[0];
            }
            return NotImplementedType.Value;
        }

        /// <summary>
        /// Convert object to a given type. This code is equivalent to NewTypeMaker.EmitConvertFromObject
        /// except that it happens at runtime instead of compile time.
        /// </summary>
        public static T ConvertFromObject<T>(object obj) {
            Type toType = typeof(T);
            object result;
            MethodInfo fastConvertMethod = PythonBinder.GetFastConvertMethod(toType);
            if (fastConvertMethod != null) {
                result = fastConvertMethod.Invoke(null, new object[] { obj });
            } else if (typeof(Delegate).IsAssignableFrom(toType)) {
                result = Converter.ConvertToDelegate(obj, toType);
            } else {
                result = obj;
            }
            return (T)obj;
        }

        public static DynamicMetaObjectBinder MakeComplexCallAction(int count, bool list, string[] keywords) {
            Argument[] infos = CompilerHelpers.MakeRepeatedArray(Argument.Simple, count + keywords.Length);
            if (list) {
                infos[count - 1] = new Argument(ArgumentType.List);
            }
            for (int i = 0; i < keywords.Length; i++) {
                infos[count + i] = new Argument(keywords[i]);
            }

            return DefaultContext.DefaultPythonContext.DefaultBinderState.Invoke(
                new CallSignature(infos)
            );
        }

        public static DynamicMetaObjectBinder MakeSimpleCallAction(int count) {
            return DefaultContext.DefaultPythonContext.DefaultBinderState.Invoke(
                new CallSignature(CompilerHelpers.MakeRepeatedArray(Argument.Simple, count))
            );
        }

        public static PythonTuple ValidateCoerceResult(object coerceResult) {
            if (coerceResult == null || coerceResult == NotImplementedType.Value) {
                return null;
            }

            PythonTuple pt = coerceResult as PythonTuple;
            if (pt == null) throw PythonOps.TypeError("coercion should return None, NotImplemented, or 2-tuple, got {0}", PythonTypeOps.GetName(coerceResult));
            return pt;
        }

        public static object GetCoerceResultOne(PythonTuple coerceResult) {
            return coerceResult._data[0];
        }

        public static object GetCoerceResultTwo(PythonTuple coerceResult) {
            return coerceResult._data[1];
        }

        public static object MethodCheckSelf(Method method, object self) {
            return method.CheckSelf(self);
        }

        public static object GeneratorCheckThrowableAndReturnSendValue(object self) {
            return ((PythonGenerator)self).CheckThrowableAndReturnSendValue();
        }

        public static ItemEnumerable CreateItemEnumerable(object callable, CallSite<Func<CallSite, CodeContext, object, int, object>> site) {
            return new ItemEnumerable(callable, site);
        }

        public static IEnumerator MakePythonEnumerator(object iterator) {
            IEnumerator enumerator;
            IEnumerable enumerale = iterator as IEnumerable;
            if (enumerale != null) {
                enumerator = enumerale.GetEnumerator();
            } else {
                enumerator = new PythonEnumerator(iterator);
            }
            return enumerator;
        }

        public static DictionaryKeyEnumerator MakeDictionaryKeyEnumerator(PythonDictionary dict) {
            return new DictionaryKeyEnumerator(dict._storage);
        }

        public static IEnumerable CreatePythonEnumerable(object baseObject) {
            return PythonEnumerable.Create(baseObject);
        }

        public static IEnumerator CreateItemEnumerator(object callable, CallSite<Func<CallSite, CodeContext, object, int, object>> site) {
            return new ItemEnumerator(callable, site);
        }

        public static IEnumerator CreatePythonEnumerator(object baseObject) {
            return PythonEnumerator.Create(baseObject);
        }

        public static bool ContainsFromEnumerable(CodeContext/*!*/ context, IEnumerator ie, object value) {
            while (ie.MoveNext()) {
                if (PythonOps.EqualRetBool(context, ie.Current, value)) {
                    return true;
                }
            }
            return false;
        }

        public static object PythonTypeGetMember(CodeContext/*!*/ context, PythonType type, object instance, SymbolId name) {
            return type.GetMember(context, instance, name);
        }

        [NoSideEffects]
        public static object CheckUninitialized(object value, SymbolId name) {
            if (value == Uninitialized.Instance) {
                ScriptingRuntimeHelpers.ThrowUnboundLocalError(name);
            }
            return value;
        }

        #region OldClass/OldInstance public helpers

        public static bool OldInstanceIsCallable(CodeContext/*!*/ context, OldInstance/*!*/ self) {
            object dummy;
            return self.TryGetBoundCustomMember(context, Symbols.Call, out dummy);
        }

        public static object OldClassCheckCallError(OldClass/*!*/ self, object dictionary, object list) {
            if ((dictionary != null && PythonOps.Length(dictionary) != 0) ||
                (list != null && PythonOps.Length(list) != 0)) {
                return self.MakeCallError();
            }

            return null;
        }

        public static void OldClassSetBases(OldClass oc, object value) {
            oc.SetBases(value);
        }
        
        public static void OldClassSetName(OldClass oc, object value) {
            oc.SetName(value);
        }

        public static void OldClassSetDictionary(OldClass oc, object value) {
            oc.SetDictionary(value);
        }

        public static void OldClassSetNameHelper(OldClass oc, SymbolId name, object value) {
            oc.SetNameHelper(name, value);
        }

        public static bool OldClassTryLookupInit(OldClass oc, object inst, out object ret) {
            return oc.TryLookupInit(inst, out ret);
        }

        public static object OldClassMakeCallError(OldClass oc) {
            return oc.MakeCallError();
        }

        public static PythonTuple OldClassGetBaseClasses(OldClass oc) {
            return PythonTuple.MakeTuple(oc.BaseClasses.ToArray());
        }

        public static void OldClassDictionaryIsPublic(OldClass oc) {
            oc.DictionaryIsPublic();
        }

        public static bool OldClassTryLookupValue(CodeContext context, OldClass oc, SymbolId name, out object value) {
            return oc.TryLookupValue(context, name, out value);
        }

        public static object OldClassLookupValue(CodeContext context, OldClass oc, SymbolId name) {
            return oc.LookupValue(context, name);
        }

        public static object OldInstanceGetOptimizedDictionary(OldInstance instance, int keyVersion) {
            CustomOldClassDictionaryStorage storage = instance.Dictionary._storage as CustomOldClassDictionaryStorage;
            if (storage == null || instance._class.HasSetAttr || storage.KeyVersion != keyVersion) {
                return null;
            }

            return storage;
        }

        public static object OldInstanceDictionaryGetValueHelper(object dict, int index, object oldInstance) {
            return ((CustomOldClassDictionaryStorage)dict).GetValueHelper(index, oldInstance);
        }

        public static bool TryOldInstanceDictionaryGetValueHelper(object dict, int index, object oldInstance, out object res) {
            return ((CustomOldClassDictionaryStorage)dict).TryGetValueHelper(index, oldInstance, out res);
        }
        
        public static object OldInstanceGetBoundMember(CodeContext context, OldInstance instance, SymbolId name) {
            return instance.GetBoundMember(context, name);
        }
        
        public static void OldInstanceDictionarySetExtraValue(object dict, int index, object value) {
            ((CustomOldClassDictionaryStorage)dict).SetExtraValue(index, value);
        }

        public static bool OldClassDeleteMember(CodeContext context, OldClass self, SymbolId name) {
            return self.DeleteCustomMember(context, name);
        }

        public static bool OldClassTryLookupOneSlot(OldClass self, SymbolId name, out object value) {
            return self.TryLookupOneSlot(name, out value);
        }

        public static bool OldInstanceTryGetBoundCustomMember(CodeContext context, OldInstance self, SymbolId name, out object value) {
            return self.TryGetBoundCustomMember(context, name, out value);
        }

        public static void OldInstanceSetCustomMember(CodeContext context, OldInstance self, SymbolId name, object value) {
            self.SetCustomMember(context, name, value);
        }

        public static bool OldInstanceDeleteCustomMember(CodeContext context, OldInstance self, SymbolId name) {
            return self.DeleteCustomMember(context, name);
        }

        #endregion

        public static void PythonTypeSetCustomMember(CodeContext context, PythonType self, SymbolId name, object value) {
            self.SetCustomMember(context, name, value);
        }

        public static bool PythonTypeDeleteCustomMember(CodeContext context, PythonType self, SymbolId name) {
            return self.DeleteCustomMember(context, name);
        }

        public static bool IsPythonType(PythonType type) {
            return type.IsPythonType;
        }

        public static object PublishModule(CodeContext/*!*/ context, string name) {
            object original = null; 
            PythonContext.GetContext(context).SystemStateModules.TryGetValue(name, out original);
            PythonContext.GetContext(context).SystemStateModules[name] = context.Scope;
            return original;
        }

        public static void RemoveModule(CodeContext/*!*/ context, string name, object oldValue) {
            if (oldValue != null) {
                PythonContext.GetContext(context).SystemStateModules[name] = oldValue;
            } else {
                PythonContext.GetContext(context).SystemStateModules.Remove(name);
            }
        }

        public static Ellipsis Ellipsis {
            get {
                return Ellipsis.Value;
            }
        }

        public static NotImplementedType NotImplemented {
            get {
                return NotImplementedType.Value;
            }
        }

        public static void ListAddForComprehension(List l, object o) {
            l.AddNoLock(o);
        }

        public static object GetUserDescriptorValue(object instance, PythonTypeSlot slot) {
            return GetUserDescriptor(((PythonTypeUserDescriptorSlot)slot).Value, instance, ((IPythonObject)instance).PythonType);
        }

        public static void ModuleStarted(CodeContext/*!*/ context, object binderState, PythonLanguageFeatures features) {
            PythonModule scopeExtension = (PythonModule)context.LanguageContext.EnsureScopeExtension(context.Scope.ModuleScope);
            scopeExtension.LanguageFeatures |= features;

            BinderState state = binderState as BinderState;
            if (state != null) {
                state.Context = context;
            }
        }

        public static void Warn(CodeContext/*!*/ context, PythonType category, string message, params object[] args) {
            PythonContext pc = PythonContext.GetContext(context);
            object warnings = null, warn = null;

            try {
                if (!pc._importWarningThrows) {
                    warnings = Importer.ImportModule(context, new PythonDictionary(), "warnings", false, -1);
                }
            } catch {
                // don't repeatedly import after it fails
                pc._importWarningThrows = true;
            }

            if (warnings != null) {
                warn = PythonOps.GetBoundAttr(context, warnings, SymbolTable.StringToId("warn"));
            }

            for (int i = 0; i < args.Length; i++) {
                args[i] = PythonOps.ToString(args[i]);
            }

            message = String.Format(message, args);

            if (warn == null) {
                PythonOps.PrintWithDest(context, pc.SystemStandardError, "warning: " + category.Name + ": " + message);
            } else {
                PythonOps.CallWithContext(context, warn, message, category);
            }
        }

        private static bool IsPrimitiveNumber(object o) {
            return IsNumericObject(o) || 
                o is Complex64 ||
                o is double ||
                o is Extensible<Complex64> || 
                o is Extensible<double>;
        }

        public static void WarnDivision(CodeContext/*!*/ context, PythonDivisionOptions options, object self, object other) {            
            if (options == PythonDivisionOptions.WarnAll) {
                if (IsPrimitiveNumber(self) && IsPrimitiveNumber(other)) {
                    if (self is Complex64 || other is Complex64 || self is Extensible<Complex64> || other is Extensible<Complex64>) {
                        Warn(context, PythonExceptions.DeprecationWarning, "classic complex division");
                        return;
                    } else if (self is double || other is double || self is Extensible<double> || other is Extensible<double>) {
                        Warn(context, PythonExceptions.DeprecationWarning, "classic float division");
                        return;
                    } else {
                        WarnDivisionInts(context, self, other);
                    }
                }
            } else if (IsNumericObject(self) && IsNumericObject(other)) {
                WarnDivisionInts(context, self, other);
            }
        }

        private static void WarnDivisionInts(CodeContext/*!*/ context, object self, object other) {
            if (self is BigInteger || other is BigInteger || self is Extensible<BigInteger> || other is Extensible<BigInteger>) {
                Warn(context, PythonExceptions.DeprecationWarning, "classic long division");
            } else {
                Warn(context, PythonExceptions.DeprecationWarning, "classic int division");
            }
        }

        public static object GetInitialBinderState(CodeContext/*!*/ context) {
            return GetBinderState(context);
        }

        internal static BinderState GetBinderState(CodeContext/*!*/ context) {
            PythonModule pm = PythonContext.GetContext(context).GetPythonModule(context.GlobalScope);

            Debug.Assert(pm.BinderState != null);
            return pm.BinderState;
        }

        public static DynamicMetaObjectBinder MakeComboAction(CodeContext/*!*/ context, DynamicMetaObjectBinder opBinder, DynamicMetaObjectBinder convBinder) {
            return GetBinderState(context).BinaryOperationRetType((PythonBinaryOperationBinder)opBinder, (ConversionBinder)convBinder);
        }

        public static DynamicMetaObjectBinder MakeInvokeAction(CodeContext/*!*/ context, CallSignature signature) {
            return GetBinderState(context).Invoke(signature);
        }

        public static DynamicMetaObjectBinder MakeGetAction(CodeContext/*!*/ context, string name, bool isNoThrow) {
            return GetBinderState(context).GetMember(name);
        }

        public static DynamicMetaObjectBinder MakeSetAction(CodeContext/*!*/ context, string name) {
            return GetBinderState(context).SetMember(name);
        }

        public static DynamicMetaObjectBinder MakeDeleteAction(CodeContext/*!*/ context, string name) {
            return GetBinderState(context).DeleteMember(name);
        }

        public static DynamicMetaObjectBinder MakeConversionAction(CodeContext/*!*/ context, Type type, ConversionResultKind kind) {
            return GetBinderState(context).Convert(type, kind);
        }

        public static DynamicMetaObjectBinder MakeOperationAction(CodeContext/*!*/ context, int operationName) {
            return GetBinderState(context).Operation((PythonOperationKind)operationName);
        }

        public static DynamicMetaObjectBinder MakeUnaryOperationAction(CodeContext/*!*/ context, ExpressionType expressionType) {
            return GetBinderState(context).UnaryOperation(expressionType);
        }

        public static DynamicMetaObjectBinder MakeBinaryOperationAction(CodeContext/*!*/ context, ExpressionType expressionType) {
            return GetBinderState(context).BinaryOperation(expressionType);
        }

        public static DynamicMetaObjectBinder MakeGetIndexAction(CodeContext/*!*/ context, int argCount) {
            return GetBinderState(context).GetIndex(argCount);
        }

        public static DynamicMetaObjectBinder MakeSetIndexAction(CodeContext/*!*/ context, int argCount) {
            return GetBinderState(context).SetIndex(argCount);
        }

        public static DynamicMetaObjectBinder MakeDeleteIndexAction(CodeContext/*!*/ context, int argCount) {
            return GetBinderState(context).DeleteIndex(argCount);
        }

        public static DynamicMetaObjectBinder MakeGetSliceBinder(CodeContext/*!*/ context) {
            return GetBinderState(context).GetSlice;
        }

        public static DynamicMetaObjectBinder MakeSetSliceBinder(CodeContext/*!*/ context) {
            return GetBinderState(context).SetSlice;
        }

        public static DynamicMetaObjectBinder MakeDeleteSliceBinder(CodeContext/*!*/ context) {
            return GetBinderState(context).DeleteSlice;
        }

        /// <summary>
        /// Provides access to AppDomain.DefineDynamicAssembly which cannot be called from a DynamicMethod
        /// </summary>
        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access) {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(name, access);
        }

        /// <summary>
        /// Provides the entry point for a compiled module.  The stub exe calls into InitializeModule which
        /// does the actual work of adding references and importing the main module.  Upon completion it returns
        /// the exit code that the program reported via SystemExit or 0.
        /// </summary>
        public static int InitializeModule(Assembly/*!*/ precompiled, string/*!*/ main, string[] references) {
            ContractUtils.RequiresNotNull(precompiled, "precompiled");
            ContractUtils.RequiresNotNull(main, "main");

            var pythonEngine = Python.CreateEngine();
            
            var pythonContext = (PythonContext)HostingHelpers.GetLanguageContext(pythonEngine);

            foreach (var scriptCode in ScriptCode.LoadFromAssembly(pythonContext.DomainManager, precompiled)) {
                pythonContext.GetCompiledLoader().AddScriptCode(scriptCode);
            }

            if (references != null) {
                foreach (string referenceName in references) {
                    pythonContext.DomainManager.LoadAssembly(Assembly.Load(referenceName));
                }
            }

            // import __main__
            try {
                Importer.Import(new CodeContext(new Scope(), pythonContext), main, PythonTuple.EMPTY, 0);
            } catch (SystemExitException ex) {
                object dummy;
                return ex.GetExitCode(out dummy);
            }

            return 0;
        }

        public static CodeContext GetPythonTypeContext(PythonType pt) {
            return pt.PythonContext.DefaultBinderState.Context;
        }

        public static Delegate GetDelegate(CodeContext context, object target, Type type) {
            return context.LanguageContext.GetDelegate(target, type);
        }

        public static int CompareLists(List self, List other) {
            return self.CompareTo(other);
        }

        public static int CompareTuples(PythonTuple self, PythonTuple other) {
            return self.CompareTo(other);
        }

        public static int CompareFloats(double self, double other) {
            return DoubleOps.Compare(self, other);
        }

        public static Bytes MakeBytes(byte[] bytes) {
            return new Bytes(bytes);
        }

        public static byte[] MakeByteArray(this string s) {
            byte[] ret = new byte[s.Length];
            for (int i = 0; i < s.Length; i++) {
                if (s[i] < 0x100) ret[i] = (byte)s[i];
                else throw PythonOps.UnicodeDecodeError("'ascii' codec can't decode byte {0:X} in position {1}: ordinal not in range", (int)ret[i], i);
            }
            return ret;
        }

        public static string MakeString(this IList<byte> bytes) {
            return MakeString(bytes, bytes.Count);
        }

        internal static string MakeString(this byte[] preamble, IList<byte> bytes) {
            char[] chars = new char[preamble.Length + bytes.Count];
            for (int i = 0; i < preamble.Length; i++) {
                chars[i] = (char)preamble[i];
            }
            for (int i = 0; i < bytes.Count; i++) {
                chars[i + preamble.Length] = (char)bytes[i];
            }
            return new String(chars);
        }

        internal static string MakeString(this IList<byte> bytes, int maxBytes) {
            int bytesToCopy = Math.Min(bytes.Count, maxBytes);
            StringBuilder b = new StringBuilder(bytesToCopy);
            for (int i = 0; i < bytesToCopy; i++) {
                b.Append((char)bytes[i]);
            }
            return b.ToString();
        }

        /// <summary>
        /// Called from generated code, helper to remove a name
        /// </summary>
        public static object RemoveName(CodeContext context, SymbolId name) {
            return context.LanguageContext.RemoveName(context.Scope, name);
        }

        /// <summary>
        /// Called from generated code, helper to do name lookup
        /// </summary>
        public static object LookupName(CodeContext context, SymbolId name) {
            return context.LanguageContext.LookupName(context.Scope, name);
        }

        /// <summary>
        /// Called from generated code, helper to do name assignment
        /// </summary>
        public static object SetName(CodeContext context, SymbolId name, object value) {
            context.LanguageContext.SetName(context.Scope, name, value);
            return value;
        }

        #region Global Access

        public static CodeContext/*!*/ CreateLocalContext(CodeContext/*!*/ outerContext, Microsoft.Scripting.Tuple boxes, SymbolId[] args, bool isVisible) {
            return new CodeContext(
                new Scope(
                    outerContext.Scope,
                    new PythonDictionary(
                        new RuntimeVariablesDictionaryStorage(boxes, args)
                    ),
                    isVisible
                ), 
                outerContext.LanguageContext, 
                outerContext
            );            
        }

        public static CodeContext/*!*/ GetGlobalContext(CodeContext/*!*/ context) {
            while (context.Parent != null) {
                context = context.Parent;
            }

            return context;
        }

        public static ClosureCell/*!*/ MakeClosureCell() {
            return new ClosureCell(Uninitialized.Instance);
        }

        public static ClosureCell/*!*/ MakeClosureCellWithValue(object initialValue) {
            return new ClosureCell(initialValue);
        }

        public static Microsoft.Scripting.Tuple/*!*/ GetClosureTupleFromFunction(PythonFunction/*!*/ function) {
            return GetClosureTupleFromContext(function.Context);
        }

        public static Microsoft.Scripting.Tuple/*!*/ GetClosureTupleFromGenerator(PythonGenerator/*!*/ generator) {
            return GetClosureTupleFromContext(generator.Context);
        }

        public static Microsoft.Scripting.Tuple/*!*/ GetClosureTupleFromContext(CodeContext/*!*/ context) {
            return ((context.Scope.Dict as PythonDictionary)._storage as RuntimeVariablesDictionaryStorage).Tuple;
        }

        public static CodeContext/*!*/ GetParentContextFromFunction(PythonFunction/*!*/ function) {
            return function.Context;
        }

        public static CodeContext/*!*/ GetParentContextFromGenerator(PythonGenerator/*!*/ generator) {
            return generator.Context;
        }

        public static object GetGlobal(Scope scope, SymbolId name) {
            return GetLocal(scope.ModuleScope, name);
        }

        public static object GetLocal(Scope scope, SymbolId name) {
            object res;
            if (scope.TryLookupName(name, out res)) {
                return res;            
            }

            object builtins;
            if (scope.ModuleScope.TryGetName(Symbols.Builtins, out builtins)) {
                Scope builtinsScope = builtins as Scope;
                if (builtinsScope != null && builtinsScope.TryGetName(name, out res)) {
                    return res;
                }

                IAttributesCollection dict = builtins as IAttributesCollection;
                if (dict != null && dict.TryGetValue(name, out res)) {
                    return res;
                }
            }

            throw NameError(name);
            
        }

        public static object RawGetGlobal(Scope scope, SymbolId name) {
            return RawGetLocal(scope.ModuleScope, name);
        }

        public static object RawGetLocal(Scope scope, SymbolId name) {
            object res;
            if (scope.TryLookupName(name, out res)) {
                return res;
            }

            return Uninitialized.Instance;
        }

        public static void SetGlobal(Scope scope, SymbolId name, object value) {
            scope.ModuleScope.Dict[name] = value;
        }

        public static void SetLocal(Scope scope, SymbolId name, object value) {
            scope.Dict[name] = value;
        }

        public static void DeleteGlobal(Scope scope, SymbolId name) {
            if (scope.ModuleScope.Dict.Remove(name)) {
                return;
            }

            throw NameError(name);

        }

        public static void DeleteLocal(Scope scope, SymbolId name) {
            if (scope.Dict.Remove(name)) {
                return;
            }

            throw NameError(name);

        }
        public static CodeContext/*!*/ CreateTopLevelCodeContext(Scope/*!*/ scope, LanguageContext/*!*/ context) {
            context.EnsureScopeExtension(scope.ModuleScope);
            return new CodeContext(scope, context);
        }

        public static PythonGlobal/*!*/[]/*!*/ GetGlobalArray(Scope/*!*/ scope) {
            return ((GlobalDictionaryStorage)((PythonDictionary)scope.Dict)._storage).Data;
        }

        public static PythonGlobal/*!*/[]/*!*/ GetGlobalArrayFromContext(CodeContext/*!*/ context) {
            Debug.Assert(context != null);
            PythonGlobal[] res = GetGlobalArray(GetGlobalContext(context).Scope);
            Debug.Assert(res != null);
            return res;
        }

        #endregion

        #region Exception Factories

        private static Exception MultipleKeywordArgumentError(PythonFunction function, string name) {
            return TypeError("{0}() got multiple values for keyword argument '{1}'", function.__name__, name);
        }

        public static Exception UnexpectedKeywordArgumentError(PythonFunction function, string name) {
            return TypeError("{0}() got an unexpected keyword argument '{1}'", function.__name__, name);
        }

        public static Exception StaticAssignmentFromInstanceError(PropertyTracker tracker, bool isAssignment) {
            return new MissingMemberException(string.Format(isAssignment ? Resources.StaticAssignmentFromInstanceError : Resources.StaticAccessFromInstanceError, tracker.Name, tracker.DeclaringType.Name));
        }

        public static Exception FunctionBadArgumentError(PythonFunction func, int count) {
            return func.BadArgumentError(count);
        }

        public static Exception BadKeywordArgumentError(PythonFunction func, int count) {
            return func.BadKeywordArgumentError(count);
        }

        public static Exception AttributeErrorForMissingOrReadonly(CodeContext/*!*/ context, PythonType dt, SymbolId name) {
            PythonTypeSlot dts;
            if (dt.TryResolveSlot(context, name, out dts)) {
                throw PythonOps.AttributeErrorForReadonlyAttribute(dt.Name, name);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(dt.Name, name);
        }

        public static Exception AttributeErrorForMissingAttribute(object o, SymbolId name) {
            PythonType dt = o as PythonType;
            if (dt != null)
                return PythonOps.AttributeErrorForMissingAttribute(dt.Name, name);

            return AttributeErrorForReadonlyAttribute(PythonTypeOps.GetName(o), name);
        }


        public static Exception ValueError(string format, params object[] args) {
            return new ArgumentException(string.Format(format, args));
        }

        public static Exception KeyError(object key) {
            return PythonExceptions.CreateThrowable(PythonExceptions.KeyError, key);
        }

        public static Exception KeyError(string format, params object[] args) {
            return new KeyNotFoundException(string.Format(format, args));
        }

        public static Exception UnicodeEncodeError(string format, params object[] args) {
#if SILVERLIGHT // EncoderFallbackException and DecoderFallbackException
            throw new NotImplementedException();
#else
            return new System.Text.DecoderFallbackException(string.Format(format, args));
#endif
        }

        public static Exception UnicodeDecodeError(string format, params object[] args) {
#if SILVERLIGHT // EncoderFallbackException and DecoderFallbackException
            throw new NotImplementedException();
#else
            return new System.Text.EncoderFallbackException(string.Format(format, args));
#endif
        }

        public static Exception IOError(Exception inner) {
            return new System.IO.IOException(inner.Message, inner);
        }

        public static Exception IOError(string format, params object[] args) {
            return new System.IO.IOException(string.Format(format, args));
        }

        public static Exception EofError(string format, params object[] args) {
            return new System.IO.EndOfStreamException(string.Format(format, args));
        }

        public static Exception StandardError(string format, params object[] args) {
            return new SystemException(string.Format(format, args));
        }

        public static Exception ZeroDivisionError(string format, params object[] args) {
            return new DivideByZeroException(string.Format(format, args));
        }

        public static Exception SystemError(string format, params object[] args) {
            return new SystemException(string.Format(format, args));
        }

        public static Exception TypeError(string format, params object[] args) {
            return new ArgumentTypeException(string.Format(format, args));
        }

        public static Exception IndexError(string format, params object[] args) {
            return new System.IndexOutOfRangeException(string.Format(format, args));
        }

        public static Exception MemoryError(string format, params object[] args) {
            return new OutOfMemoryException(string.Format(format, args));
        }

        public static Exception ArithmeticError(string format, params object[] args) {
            return new ArithmeticException(string.Format(format, args));
        }

        public static Exception NotImplementedError(string format, params object[] args) {
            return new NotImplementedException(string.Format(format, args));
        }

        public static Exception AttributeError(string format, params object[] args) {
            return new MissingMemberException(string.Format(format, args));
        }

        public static Exception OverflowError(string format, params object[] args) {
            return new System.OverflowException(string.Format(format, args));
        }
        public static Exception WindowsError(string format, params object[] args) {
#if !SILVERLIGHT // System.ComponentModel.Win32Exception
            return new System.ComponentModel.Win32Exception(string.Format(format, args));
#else
            return new System.SystemException(string.Format(format, args));
#endif
        }

        public static Exception SystemExit() {
            return new SystemExitException();
        }

        public static Exception SyntaxWarning(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode) {
            int line = span.Start.Line;
            string fileName = sourceUnit.Path ?? "?";

            if (sourceUnit != null) {
                message = String.Format("{0} ({1}, line {2})", message, fileName, line);
            }

            return SyntaxWarning(message, fileName, span.Start.Line, span.Start.Column, sourceUnit.GetCodeLine(line), Severity.FatalError);
        }

        public static SyntaxErrorException SyntaxError(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode) {
            switch (errorCode & ErrorCodes.ErrorMask) {
                case ErrorCodes.IndentationError:
                    return new IndentationException(message, sourceUnit, span, errorCode, Severity.FatalError);

                case ErrorCodes.TabError:
                    return new TabException(message, sourceUnit, span, errorCode, Severity.FatalError);

                default:
                    return new SyntaxErrorException(message, sourceUnit, span, errorCode, Severity.FatalError);
            }
        }

        public static Exception StopIteration() {
            return StopIteration("");
        }

        public static Exception InvalidType(object o, RuntimeTypeHandle handle) {
            return PythonOps.TypeErrorForTypeMismatch(DynamicHelpers.GetPythonTypeFromType(Type.GetTypeFromHandle(handle)).Name, o);
        }

        public static Exception ZeroDivisionError() {
            return ZeroDivisionError("Attempted to divide by zero.");
        }

        // If you do "(a, b) = (1, 2, 3, 4)"
        public static Exception ValueErrorForUnpackMismatch(int left, int right) {
            System.Diagnostics.Debug.Assert(left != right);

            if (left > right)
                return ValueError("need more than {0} values to unpack", right);
            else
                return ValueError("too many values to unpack");
        }

        public static Exception NameError(SymbolId name) {
            return new UnboundNameException(string.Format("name '{0}' is not defined", SymbolTable.IdToString(name)));
        }


        // If an unbound method is called without a "self" argument, or a "self" argument of a bad type
        public static Exception TypeErrorForUnboundMethodCall(string methodName, Type methodType, object instance) {
            return TypeErrorForUnboundMethodCall(methodName, DynamicHelpers.GetPythonTypeFromType(methodType), instance);
        }

        public static Exception TypeErrorForUnboundMethodCall(string methodName, PythonType methodType, object instance) {
            string message = string.Format("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)",
                                           methodName, methodType.Name, DynamicHelpers.GetPythonType(instance).Name);
            return TypeError(message);
        }

        // When a generator first starts, before it gets to the first yield point, you can't call generator.Send(x) where x != null.
        // See Pep342 for details.
        public static Exception TypeErrorForIllegalSend() {
            string message = "can't send non-None value to a just-started generator";
            return TypeError(message);
        }

        // If a method is called with an incorrect number of arguments
        // You should use TypeErrorForUnboundMethodCall() for unbound methods called with 0 arguments
        public static Exception TypeErrorForArgumentCountMismatch(string methodName, int expectedArgCount, int actualArgCount) {
            return TypeError("{0}() takes exactly {1} argument{2} ({3} given)",
                             methodName, expectedArgCount, expectedArgCount == 1 ? "" : "s", actualArgCount);
        }

        public static Exception TypeErrorForTypeMismatch(string expectedTypeName, object instance) {
            return TypeError("expected {0}, got {1}", expectedTypeName, PythonOps.GetPythonTypeName(instance));
        }

        // If hash is called on an instance of an unhashable type
        public static Exception TypeErrorForUnhashableType(string typeName) {
            return TypeError(typeName + " objects are unhashable");
        }

        public static Exception TypeErrorForUnhashableObject(object obj) {
            return TypeErrorForUnhashableType(DynamicHelpers.GetPythonType(obj).Name);
        }

        internal static Exception TypeErrorForIncompatibleObjectLayout(string prefix, PythonType type, Type newType) {
            return TypeError("{0}: '{1}' object layout differs from '{2}'", prefix, type.Name, newType);
        }

        public static Exception TypeErrorForNonStringAttribute() {
            return TypeError("attribute name must be string");
        }

        internal static Exception TypeErrorForBadInstance(string template, object instance) {
            return TypeError(template, PythonOps.GetPythonTypeName(instance));
        }

        public static Exception TypeErrorForBinaryOp(string opSymbol, object x, object y) {
            throw PythonOps.TypeError("unsupported operand type(s) for {0}: '{1}' and '{2}'",
                                opSymbol, GetPythonTypeName(x), GetPythonTypeName(y));
        }

        public static Exception TypeErrorForUnaryOp(string opSymbol, object x) {
            throw PythonOps.TypeError("unsupported operand type for {0}: '{1}'",
                                opSymbol, GetPythonTypeName(x));
        }

        public static Exception TypeErrorForNonIterableObject(object o) {
            return PythonOps.TypeError(
                "argument of type '{0}' is not iterable",
                DynamicHelpers.GetPythonType(o).Name
            );
        }

        public static Exception TypeErrorForDefaultArgument(string message) {
            return PythonOps.TypeError(message);
        }

        public static Exception AttributeErrorForReadonlyAttribute(string typeName, SymbolId attributeName) {
            // CPython uses AttributeError for all attributes except "__class__"
            if (attributeName == Symbols.Class)
                return PythonOps.TypeError("can't delete __class__ attribute");

            return PythonOps.AttributeError("attribute '{0}' of '{1}' object is read-only", SymbolTable.IdToString(attributeName), typeName);
        }

        public static Exception AttributeErrorForBuiltinAttributeDeletion(string typeName, SymbolId attributeName) {
            return PythonOps.AttributeError("cannot delete attribute '{0}' of builtin type '{1}'", SymbolTable.IdToString(attributeName), typeName);
        }

        public static Exception MissingInvokeMethodException(object o, string name) {
            if (o is OldClass) {
                throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'",
                    ((OldClass)o).__name__, name);
            } else {
                throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", GetPythonTypeName(o), name);
            }
        }

        /// <summary>
        /// Create at TypeError exception for when Raise() can't create the exception requested.  
        /// </summary>
        /// <param name="type">original type of exception requested</param>
        /// <returns>a TypeEror exception</returns>
        internal static Exception MakeExceptionTypeError(object type) {
            return PythonOps.TypeError("exceptions must be classes, instances, or strings (deprecated), not {0}", DynamicHelpers.GetPythonType(type));
        }

        public static Exception AttributeErrorForMissingAttribute(string typeName, SymbolId attributeName) {
            return PythonOps.AttributeError("'{0}' object has no attribute '{1}'", typeName, SymbolTable.IdToString(attributeName));
        }

        public static Exception UncallableError(object func) {
            return PythonOps.TypeError("{0} is not callable", PythonTypeOps.GetName(func));
        }

        public static Exception TypeErrorForProtectedMember(Type/*!*/ type, string/*!*/ name) {
            return PythonOps.TypeError("cannot access protected member {0} without a python subclass of {1}", name, NameConverter.GetTypeName(type));
        }

        public static Exception TypeErrorForGenericMethod(Type/*!*/ type, string/*!*/ name) {
            return PythonOps.TypeError("{0}.{1} is a generic method and must be indexed with types before calling", NameConverter.GetTypeName(type), name);
        }

        public static Exception TypeErrorForUnIndexableObject(object o) {
            IPythonObject ipo;
            if (o == null) {
                return PythonOps.TypeError("'NoneType' object cannot be interpreted as an index");
            } else if ((ipo = o as IPythonObject) != null) {
                return TypeError("'{0}' object cannot be interpreted as an index", ipo.PythonType.Name);
            }

            return TypeError("object cannot be interpreted as an index");
        }

        [Obsolete("no longer used anywhere")]
        public static Exception/*!*/ TypeErrorForBadDictionaryArgument(PythonFunction/*!*/ f) {
            return PythonOps.TypeError("{0}() argument after ** must be a dictionary", f.__name__);
        }

        public static T TypeErrorForBadEnumConversion<T>(object value) {
            throw TypeError("Cannot convert numeric value {0} to {1}.  The value must be zero.", value, NameConverter.GetTypeName(typeof(T)));
        }

        public static Exception/*!*/ UnreadableProperty() {
            return PythonOps.AttributeError("unreadable attribute");
        }


        public static Exception/*!*/ UnsetableProperty() {
            return PythonOps.AttributeError("readonly attribute");
        }

        public static Exception/*!*/ UndeletableProperty() {
            return PythonOps.AttributeError("undeletable attribute");
        }

        #endregion        
    }
}
