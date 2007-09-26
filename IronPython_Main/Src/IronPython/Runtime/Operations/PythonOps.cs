/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Types;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Compiler;
using IronPython.Compiler.Generation;
using IronPython.Hosting;

namespace IronPython.Runtime.Operations {

    /// <summary>
    /// Contains functions that are called directly from
    /// generated code to perform low-level runtime functionality.
    /// </summary>
    public static partial class PythonOps {
        #region Shared static data

        [ThreadStatic]
        private static List<object> InfiniteRepr;

        [ThreadStatic]
        internal static Exception RawException;

        /// <summary> Singleton NotImplemented object of NotImplementedType.  Initialized after type has been created in static constructor </summary>
        public static readonly object NotImplemented;
        public static readonly object Ellipsis;

        /// <summary> Dictionary of error handlers for string codecs. </summary>
        private static Dictionary<string, object> errorHandlers = new Dictionary<string, object>();
        /// <summary> Table of functions used for looking for additional codecs. </summary>
        private static List<object> searchFunctions = new List<object>();
        /// <summary> stored for copy_reg module, used for reduce protocol </summary>
        public static BuiltinFunction NewObject;
        /// <summary> stored for copy_reg module, used for reduce protocol </summary>
        public static BuiltinFunction PythonReconstructor;

        private static FastDynamicSite<object, object, int> CompareSite = FastDynamicSite<object, object, int>.Create(DefaultContext.Default, DoOperationAction.Make(Operators.Compare));
        private static DynamicSite<object, string, PythonTuple, IAttributesCollection, object> MetaclassSite;
        private static FastDynamicSite<object, string, object> _writeSite = RuntimeHelpers.CreateSimpleCallSite<object, string, object>(DefaultContext.Default);


        #endregion

        static PythonOps() {
#if !SILVERLIGHT
            // register Python COM type builder until COM type builder can move down...
            ReflectedTypeBuilder.RegisterAlternateBuilder(delegate(Type t) {
                if (ComObject.Is__ComObject(t)) {
                    return ComTypeBuilder.ComType;
                }
                return null;
            });
#endif

            DynamicTypeBuilder.TypeInitialized += new EventHandler<TypeCreatedEventArgs>(PythonTypeCustomizer.OnTypeInit);

            MakeDynamicTypesTable();
                        
            NotImplemented = NotImplementedTypeOps.CreateInstance();
            Ellipsis = EllipsisTypeOps.CreateInstance();
            NoneTypeOps.InitInstance();
        }

        public static BigInteger MakeIntegerFromHex(string s) {
            return LiteralParser.ParseBigInteger(s, 16);
        }

        public static PythonDictionary MakeDict(int size) {
            return new PythonDictionary(size);
        }

        public static bool IsCallable(object o) {
            return IsCallable(DefaultContext.Default, o);
        }

        public static bool IsCallable(CodeContext context, object o) {
            if (o is ICallableWithCodeContext) {
                return true;
            }

            return PythonOps.HasAttr(context, o, Symbols.Call);
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
        
        internal static object LookupEncodingError(string name) {
            if (errorHandlers.ContainsKey(name))
                return errorHandlers[name];
            else
                throw PythonOps.LookupError("unknown error handler name '{0}'", name);
        }

        internal static void RegisterEncodingError(string name, object handler) {
            if(!PythonOps.IsCallable(handler))
                throw PythonOps.TypeError("handler must be callable");

            errorHandlers[name] = handler;
        }

#endif
        
        internal static PythonTuple LookupEncoding(string encoding) {
            for (int i = 0; i < searchFunctions.Count; i++) {
                object res = PythonCalls.Call(searchFunctions[i], encoding);
                if (res != null) return (PythonTuple)res;
            }

            throw PythonOps.LookupError("unknown encoding: {0}", encoding);
        }

        internal static void RegisterEncoding(object search_function) {
            if(!PythonOps.IsCallable(search_function))
                throw PythonOps.TypeError("search_function must be callable");

            searchFunctions.Add(search_function);
        }

        //!!! Temporarily left in so this checkin won't collide with Converter changes
        internal static string GetClassName(object obj) {
            return GetPythonTypeName(obj);
        }

        internal static string GetPythonTypeName(object obj) {
            OldInstance oi = obj as OldInstance;
            if (oi != null) return oi.__class__.__name__.ToString();
            else return DynamicTypeOps.GetName(DynamicHelpers.GetDynamicType(obj));
        }

        public static string StringRepr(object o) {
            return StringRepr(DefaultContext.Default, o);
        }

        public static string StringRepr(CodeContext context, object o) {
            if (o == null) return "None";

            string s = o as string;
            if (s != null) return StringOps.Quote(s);
            if (o is int) return o.ToString();
            if (o is long) return ((long)o).ToString() + "L";
            if (o is BigInteger) return ((BigInteger)o).ToString() + "L";
            if (o is double) return DoubleOps.ToString((double)o);
            if (o is float) return DoubleOps.ToString((float)o);

            PerfTrack.NoteEvent(PerfTrack.Categories.Temporary, "Repr " + o.GetType().FullName);

            // could be a container object, we need to detect recursion, but only
            // for our own built-in types that we're aware of.  The user can setup
            // infinite recursion in their own class if they want.
            ICodeFormattable f = o as ICodeFormattable;
            if (f != null) {
                List<object> infinite = GetAndCheckInfinite(o);
                if (infinite == null) return GetInfiniteRepr(o);
                int index = infinite.Count;
                infinite.Add(o);
                try {
                    return f.ToCodeString(context);
                } finally {
                    System.Diagnostics.Debug.Assert(index == infinite.Count - 1);
                    infinite.RemoveAt(index);
                }
            }

            Array a = o as Array;
            if (a != null) {
                List<object> infinite = GetAndCheckInfinite(o);
                if (infinite == null) return GetInfiniteRepr(o);
                int index = infinite.Count;
                infinite.Add(o);
                try {
                    return ArrayOps.CodeRepresentation(a);
                } finally {
                    System.Diagnostics.Debug.Assert(index == infinite.Count - 1);
                    infinite.RemoveAt(index);
                }
            }

            return DynamicHelpers.GetDynamicType(o).InvokeUnaryOperator(context, Operators.CodeRepresentation, o) as string;
        }

        private static List<object> GetAndCheckInfinite(object o) {
            List<object> infinite = GetReprInfinite();
            foreach (object o2 in infinite) {
                if (o == o2) {
                    return null;
                }
            }
            return infinite;
        }

        private static string GetInfiniteRepr(object o) {
            object keys;
            return o is List ? "[...]" :
                o is PythonDictionary ? "{...}" :
                PythonOps.TryGetBoundAttr(o, Symbols.Keys, out keys) ? "{...}" : // user dictionary
                "...";
        }

        private static Dictionary<Type, FastDynamicSite<object, string>> _toStrSites = new Dictionary<Type, FastDynamicSite<object, string>>();
        public static string ToString(object o) {
            string x = o as string;
            Array ax;
            DynamicType dt;
            OldClass oc;
            if (x != null) return x;
            if (o == null) return "None";
            if (o is double) return DoubleOps.ToString((double)o);
            if (o is float) return DoubleOps.ToString((float)o);
            if ((ax = o as Array) != null) return StringRepr(ax);
            if ((dt = o as DynamicType) != null) return DynamicTypeOps.Repr(DefaultContext.Default, dt);
            if ((oc = o as OldClass) != null) return oc.ToString();

            object tostr;
            if (TryGetBoundAttr(o, Symbols.String, out tostr)) {
                FastDynamicSite<object, string> callSite;
                lock (_toStrSites) {
                    if (!_toStrSites.TryGetValue(o.GetType(), out callSite)) {
                        _toStrSites[o.GetType()] = callSite = 
                            RuntimeHelpers.CreateSimpleCallSite<object, string>(DefaultContext.Default);
                    }
                }
                string ret = callSite.Invoke(tostr);
                if (ret == null) {
                    throw PythonOps.TypeError("expected str, got NoneType from __str__");
                }
                return ret;
            }
            return o.ToString();            
        }


        public static object Repr(object o) {
            return StringRepr(o);
        }

        internal static bool IsCallable(object o, int expArgCnt, out int minArgCnt, out int maxArgCnt) {
            // if we have a python function make sure it's compatible...
            PythonFunction fo = o as PythonFunction;

            Method m = o as Method;
            if (m != null) {
                fo = m.Function as PythonFunction;
            }

            minArgCnt = 0;
            maxArgCnt = 0;

            if (fo != null) {
                if ((fo.Flags & (FunctionAttributes.ArgumentList | FunctionAttributes.KeywordDictionary)) == 0) {
                    maxArgCnt = fo.NormalArgumentCount;
                    minArgCnt = fo.NormalArgumentCount - fo.Defaults.Length;

                    // take into account unbound methods / bound methods
                    if (m != null) {
                        if (m.Self != null) {
                            maxArgCnt--;
                            minArgCnt--;
                        }
                    }

                    // the target is no good for this delegate - we don't have enough
                    // parameters.
                    if (expArgCnt < minArgCnt || expArgCnt > maxArgCnt)
                        return false;
                }
            }

            return o != null;
        }

        public static object Plus(object o) {
            object ret;

            if (o is int) return o;
            else if (o is double) return o;
            else if (o is BigInteger) return o;
            else if (o is Complex64) return o;
            else if (o is long) return o;
            else if (o is float) return o;
            else if (o is bool) return RuntimeHelpers.Int32ToObject((bool)o ? 1 : 0);
            else if (PythonOps.TryInvokeOperator(DefaultContext.Default,
                Operators.Positive,
                o,
                out ret))
                return ret;

            if (DynamicHelpers.GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default, Operators.Positive, o, out ret) &&
                ret != PythonOps.NotImplemented)
                return ret;

            throw PythonOps.TypeError("bad operand type for unary +");
        }

        public static object Negate(object o) {
            if (o is int) return Int32Ops.Negate((int)o);
            else if (o is double) return DoubleOps.Negate((double)o);
            else if (o is long) return Int64Ops.Negate((long)o);
            else if (o is BigInteger) return BigIntegerOps.Negate((BigInteger)o);
            else if (o is Complex64) return -(Complex64)o;
            else if (o is float) return DoubleOps.Negate((float)o);
            else if (o is bool) return RuntimeHelpers.Int32ToObject((bool)o ? -1 : 0);

            object ret;
            if (DynamicHelpers.GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default, Operators.Negate, o, out ret) &&
                ret != PythonOps.NotImplemented)
                return ret;

            throw PythonOps.TypeError("bad operand type for unary -");
        }

        public static bool IsSubClass(DynamicType c, object typeinfo) {
            if (c == null) throw PythonOps.TypeError("issubclass: arg 1 must be a class");
            if (typeinfo == null) throw PythonOps.TypeError("issubclass: arg 2 must be a class");

            PythonTuple pt = typeinfo as PythonTuple;
            if (pt != null) {
                // Recursively inspect nested tuple(s)
                foreach (object o in pt) {
                    if (IsSubClass(c, o)) return true;
                }
                return false;
            }

            OldClass oc = typeinfo as OldClass;
            if (oc != null) {
                return c.IsSubclassOf(oc.TypeObject);
            }

            object bases;
            DynamicType dt = typeinfo as DynamicType;
            if (dt == null) {
                if (!PythonOps.TryGetBoundAttr(typeinfo, Symbols.Bases, out bases)) {
                    //!!! deal with classes w/ just __bases__ defined.
                    throw PythonOps.TypeErrorForBadInstance("issubclass(): {0} is not a class nor a tuple of classes", typeinfo);
                }

                IEnumerator ie = PythonOps.GetEnumerator(bases);
                while (ie.MoveNext()) {
                    DynamicType baseType = ie.Current as DynamicType;

                    if (baseType == null) {
                        OldClass ocType = ie.Current as OldClass;
                        if (ocType == null) throw PythonOps.TypeError("expected type, got {0}", DynamicHelpers.GetDynamicType(ie.Current));

                        baseType = ocType.TypeObject;
                    }

                    if (c.IsSubclassOf(baseType)) return true;
                }
                return false;
            }

            if (dt.UnderlyingSystemType.IsInterface) {
                // interfaces aren't in bases, and therefore IsSubclassOf doesn't do this check.
                if (dt.UnderlyingSystemType.IsAssignableFrom(c.UnderlyingSystemType)) {
                    return true;
                }
            } 

            return c.IsSubclassOf(dt);
        }

        public static bool IsInstance(object o, object typeinfo) {
            if (typeinfo == null) throw PythonOps.TypeError("isinstance: arg 2 must be a class, type, or tuple of classes and types");

            PythonTuple tt = typeinfo as PythonTuple;
            if (tt != null) {
                foreach (object type in tt) {
                    if (IsInstance(o, type)) return true;
                }
                return false;
            }

            if (typeinfo is OldClass) {
                // old instances are strange - they all share a common type
                // of instance but they can "be subclasses" of other
                // OldClass's.  To check their types we need the actual
                // instance.
                OldInstance oi = o as OldInstance;
                if (oi != null) return oi.__class__.IsSubclassOf(typeinfo);
            }

            DynamicType odt = DynamicHelpers.GetDynamicType(o);
            if (IsSubClass(odt, typeinfo)) {
                return true;
            }

            object cls;
            if (PythonOps.TryGetBoundAttr(o, Symbols.Class, out cls) &&
                (!object.ReferenceEquals(odt, cls))) {
                return IsSubclassSlow(cls, typeinfo);
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
            if (o is bool) return RuntimeHelpers.Int32ToObject((bool)o ? -2 : -1);

            object ret;
            if (DynamicHelpers.GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default, Operators.OnesComplement, o, out ret) &&
                ret != PythonOps.NotImplemented)
                return ret;


            throw PythonOps.TypeError("bad operand type for unary ~");
        }

        public static object Not(object o) {
            return IsTrue(o) ? RuntimeHelpers.False : RuntimeHelpers.True;
        }

        public static object Is(object x, object y) {
            return x == y ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        public static bool IsRetBool(object x, object y) {
            return x == y;
        }

        public static object IsNot(object x, object y) {
            return x != y ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        public static bool IsNotRetBool(object x, object y) {
            return x != y;
        }

        public static object In(object x, object y) {
            if (y is IDictionary) {
                return RuntimeHelpers.BooleanToObject(((IDictionary)y).Contains(x));
            }

            if (y is IList) {
                return RuntimeHelpers.BooleanToObject(((IList)y).Contains(x));
            }

            if (y is IList<object>) {
                return RuntimeHelpers.BooleanToObject(((IList<object>)y).Contains(x));
            }
           
            string ys;
            if ((ys = y as string) != null) {
                string s = x as string;
                if (s == null) {
                    if (x is char) {
                        return (ys.IndexOf((char)x) != -1) ? RuntimeHelpers.True : RuntimeHelpers.False;
                    }
                    throw TypeError("'in <string>' requires string as left operand");
                }
                return ys.Contains(s) ? RuntimeHelpers.True : RuntimeHelpers.False;
            }

            if (y is char) {
                return In(x, y.ToString());
            }

            object contains;
            if (PythonOps.TryInvokeOperator(DefaultContext.Default,
                Operators.Contains,
                y,
                x,
                out contains)) {
                return PythonOps.IsTrue(contains) ? RuntimeHelpers.True : RuntimeHelpers.False;
            }

            IEnumerator e = GetEnumerator(y);
            while (e.MoveNext()) {
                if (PythonOps.EqualRetBool(e.Current, x)) return RuntimeHelpers.True;
            }

            return RuntimeHelpers.False;
        }

        public static bool InRetBool(object x, object y) {
            if (y is IDictionary) {
                return ((IDictionary)y).Contains(x);
            }

            if (y is IList) {
                return ((IList)y).Contains(x);
            }

            if (y is string) {
                string s = x as string;
                if (s == null) {
                    throw TypeError("'in <string>' requires string as left operand");
                }
                return ((string)y).Contains(s);
            }

            object contains;
            if (PythonOps.TryInvokeOperator(DefaultContext.Default,
                Operators.Contains,
                y,
                x,
                out contains)) {
                return PythonOps.IsTrue(contains);
            }

            IEnumerator e = GetEnumerator(y);
            while (e.MoveNext()) {
                if (PythonOps.EqualRetBool(e.Current, x)) return true;
            }

            return false;
        }

        public static object NotIn(object x, object y) {
            return Not(In(x, y));  //???
        }

        public static bool NotInRetBool(object x, object y) {
            return !InRetBool(x, y);  //???
        }

        //        public static object DynamicHelpers.GetDynamicType1(object o) {
        //            IConvertible ic = o as IConvertible;
        //            if (ic != null) {
        //                switch (ic.GetTypeCode()) {
        //                    case TypeCode.Int32: return "int";
        //                    case TypeCode.Double: return "double";
        //                    default: throw new NotImplementedException();
        //                }
        //            } else {
        //                throw new NotImplementedException();
        //            }
        //        }
        //
        //        private static object[] oas = new object[] { "int", "double" };
        //
        //        public static object DynamicHelpers.GetDynamicType2(object o) {
        //            Type ty = o.GetType();
        //            int hc = ty.GetHashCode();
        //
        //            return oas[hc%1];
        //        }

        // TODO: Remove this method, assemblies get registered as packages?
        private static void MakeDynamicTypesTable() {
            RegisterLanguageAssembly(Assembly.GetExecutingAssembly());
            DynamicHelpers.TypeExtended += new EventHandler<DynamicHelpers.TypeExtendedEventArgs>(DynamicHelpers_TypeExtended);

            // TODO: Remove impersonation
            DynamicTypeBuilder.GetBuilder(DynamicHelpers.GetDynamicTypeFromType(typeof(SymbolDictionary))).SetImpersonationType(typeof(PythonDictionary));

            // TODO: Contest specific MRO?
            DynamicTypeBuilder.GetBuilder(DynamicHelpers.GetDynamicTypeFromType(typeof(bool))).AddInitializer(delegate(DynamicMixinBuilder builder) {
                DynamicTypeBuilder dtb = (DynamicTypeBuilder)builder;
                builder.SetResolutionOrder(new DynamicType[]{
                    TypeCache.Boolean,
                    TypeCache.Int32,
                    TypeCache.Object});
                dtb.SetBases(new DynamicType[] { TypeCache.Int32 });
            });
        }

        private static void DynamicHelpers_TypeExtended(object sender, DynamicHelpers.TypeExtendedEventArgs e) {
            DynamicTypeExtender.ExtendType(DynamicHelpers.GetDynamicTypeFromType(e.Extending), e.Extension);
        }
                
        public static bool EqualIsTrue(object x, int y) {
            if (x is int) return ((int)x) == y;

            return EqualRetBool(x, y);
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
        internal static object MultiplySequence<T>(MultiplySequenceWorker<T> multiplier, T sequence, object count, bool isForward) {
            if (isForward && count != null) {
                object ret;
                if (PythonOps.TryInvokeOperator(DefaultContext.Default, Operators.ReverseMultiply, count, sequence, out ret)) {
                    if (ret != NotImplemented) return ret;
                }
            }

            int icount = Converter.ConvertToInt32(count);
            if (icount < 0) icount = 0;
            return multiplier(sequence, icount);
        }

        private static FastDynamicSite<object, object, object> EqualSharedSite =
            FastDynamicSite<object, object, object>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Equals));

        public static object Equal(object x, object y) {
            return EqualSharedSite.Invoke(x, y);
        }
        private static FastDynamicSite<object, object, bool> EqualBooleanSharedSite =
            FastDynamicSite<object, object, bool>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Equals));

        public static bool EqualRetBool(object x, object y) {
            //TODO just can't seem to shake these fast paths
            if (x is int && y is int) { return ((int)x) == ((int)y); }
            if (x is double && y is double) { return ((double)x) == ((double)y); }
            if (x is string && y is string) { return ((string)x).Equals((string)y); }

            return EqualBooleanSharedSite.Invoke(x, y);
        }

        public static int Compare(object x, object y) {
            return Compare(DefaultContext.Default, x, y);
        }

        public static int Compare(CodeContext context, object x, object y) {
            if (x == y) return 0;

            return CompareSite.Invoke(x, y);
        }

        public static object CompareEqual(int res) {
            return res == 0 ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        public static object CompareNotEqual(int res) {
            return res == 0 ? RuntimeHelpers.False : RuntimeHelpers.True;
        }

        public static object CompareGreaterThan(int res) {
            return res > 0 ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        public static object CompareGreaterThanOrEqual(int res) {
            return res >= 0 ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        public static object CompareLessThan(int res) {
            return res < 0 ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        public static object CompareLessThanOrEqual(int res) {
            return res <= 0 ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        public static bool CompareTypesEqual(object x, object y) {
            return PythonOps.CompareTypes(x, y) == 0;
        }

        public static bool CompareTypesNotEqual(object x, object y) {
            return PythonOps.CompareTypes(x, y) != 0;
        }

        public static bool CompareTypesGreaterThan(object x, object y) {
            return PythonOps.CompareTypes(x, y) > 0;
        }

        public static bool CompareTypesLessThan(object x, object y) {
            return PythonOps.CompareTypes(x, y) < 0;
        }

        public static bool CompareTypesGreaterThanOrEqual(object x, object y) {
            return PythonOps.CompareTypes(x, y) >= 0;
        }

        public static bool CompareTypesLessThanOrEqual(object x, object y) {
            return PythonOps.CompareTypes(x, y) <= 0;
        }

        public static int CompareTypes(object x, object y) {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            string name1, name2;
            int diff;

            if (DynamicHelpers.GetDynamicType(x) != DynamicHelpers.GetDynamicType(y)) {
                if (x.GetType() == typeof(OldInstance)) {
                    name1 = ((OldInstance)x).__class__.Name;
                    if (y.GetType() == typeof(OldInstance)) {
                        name2 = ((OldInstance)y).__class__.Name;
                    } else {
                        // old instances are always less than new-style classes
                        return -1;
                    }
                } else if (y.GetType() == typeof(OldInstance)) {
                    // old instances are always less than new-style classes
                    return 1;
                } else {
                    name1 = DynamicTypeOps.GetName(x);
                    name2 = DynamicTypeOps.GetName(y);
                }
                diff = String.CompareOrdinal(name1, name2);
            } else {
                diff = (int)(IdDispenser.GetId(x) - IdDispenser.GetId(y));
            }

            if (diff < 0) return -1;
            if (diff == 0) return 0;
            return 1;
        }

        public static object GreaterThanHelper(CodeContext context, object self, object other) {
            return InternalCompare(context, Operators.GreaterThan, self, other);
        }

        public static object LessThanHelper(CodeContext context, object self, object other) {
            return InternalCompare(context, Operators.LessThan, self, other);
        }

        public static object GreaterThanOrEqualHelper(CodeContext context, object self, object other) {
            return InternalCompare(context, Operators.GreaterThanOrEqual, self, other);
        }

        public static object LessThanOrEqualHelper(CodeContext context, object self, object other) {
            return InternalCompare(context, Operators.LessThanOrEqual, self, other);
        }

        public static object InternalCompare(CodeContext context, Operators op, object self, object other) {
            object ret;
            if (DynamicHelpers.GetDynamicType(self).TryInvokeBinaryOperator(context, op, self, other, out ret))
                return ret;

            return PythonOps.NotImplemented;
        }

        public static object RichEqualsHelper(object self, object other) {
            object res;

            if (DynamicHelpers.GetDynamicType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.Equals, self, other, out res))
                return res;

            return PythonOps.NotImplemented;
        }

        public static int CompareToZero(object value) {
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                if (val > 0) return 1;
                if (val < 0) return -1;
                return 0;
            }
            throw PythonOps.TypeErrorForBadInstance("unable to compare type {0} with 0 ", value);
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

        public static object PowerMod(object x, object y, object z) {
            object ret;
            if (z == null) return PythonSites.Power(x, y);
            if (x is int && y is int && z is int) {
                ret = Int32Ops.Power((int)x, (int)y, (int)z);
                if (ret != NotImplemented) return ret;
            } else if (x is BigInteger) {
                ret = BigIntegerOps.Power((BigInteger)x, y, z);
                if (ret != NotImplemented) return ret;
            }

            if (x is Complex64 || y is Complex64 || z is Complex64) {
                throw PythonOps.ValueError("complex modulo");
            }

            if (DynamicHelpers.GetDynamicType(x).TryInvokeTernaryOperator(DefaultContext.Default, Operators.Power, x, y, z, out ret) && ret != PythonOps.NotImplemented)
                return ret;

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
            if (!Converter.TryConvertToIEnumerator(o, out ie)) {
                throw PythonOps.TypeError("{0} is not enumerable", StringRepr(o));
            }
            return ie;
        }

        public static IEnumerator GetEnumeratorForUnpack(object enumerable) {
            IEnumerator ie;
            if (!Converter.TryConvertToIEnumerator(enumerable, out ie)) {
                throw PythonOps.TypeError("unpack non-sequence of type {0}",
                    StringRepr(DynamicTypeOps.GetName(enumerable)));
            }
            return ie;
        }

        public static long Id(object o) {
            return IdDispenser.GetId(o);
        }

        public static string HexId(object o) {
            return string.Format("0x{0:X16}", Id(o));
        }

        public static int SimpleHash(object o) {
            // must stay in sync w/ Ops.Hash!  This is just the version w/o RichEquality checks
            if (o is int) return (int)o;
            if (o is string) return o.GetHashCode();    // avoid lookups on strings - A) We can stack overflow w/ Dict B) they don't define __hash__
            if (o is double) return (int)(double)o;
            if (o == null) return NoneTypeOps.HashCode;
            if (o is char) return new String((char)o, 1).GetHashCode();

            return o.GetHashCode();
        }

        public static int Hash(object o) {
            // must stay in sync w/ Ops.SimpleHash!  This is just the version w/ RichEquality checks
            if (o is int) return (int)o;
            if (o is string) return o.GetHashCode();    // avoid lookups on strings - A) We can stack overflow w/ Dict B) they don't define __hash__
            if (o is double) return (int)(double)o;
            if (o == null) return NoneTypeOps.HashCode;
            if (o is char) return new String((char)o, 1).GetHashCode();

            IValueEquality ipe = o as IValueEquality;
            if (ipe != null) {
                // invoke operator dynamically to go through protocol wrapper override, if defined.
                object ret;
                DynamicHelpers.GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default,
                    Operators.ValueHash,
                    o,
                    out ret);
                if (ret != PythonOps.NotImplemented) {
                    return Converter.ConvertToInt32(ret);
                }
            }

            return o.GetHashCode();
        }

        public static object Hex(object o) {
            if (o is int) return Int32Ops.Hex((int)o);
            else if (o is BigInteger) return BigIntegerOps.Hex((BigInteger)o);

            object hex;
            if(PythonOps.TryInvokeOperator(DefaultContext.Default,
                Operators.ConvertToHex,
                o,
                out hex)) {            
                if (!(hex is string) && !(hex is ExtensibleString))
                    throw PythonOps.TypeError("hex expected string type as return, got {0}", PythonOps.StringRepr(DynamicTypeOps.GetName(hex)));

                return hex;
            }
            throw TypeError("hex() argument cannot be converted to hex");
        }

        public static object Oct(object o) {
            if (o is int) {
                return Int32Ops.Oct((int)o);
            } else if (o is BigInteger) {
                return BigIntegerOps.Oct((BigInteger)o);
            }

            object octal;
            if(PythonOps.TryInvokeOperator(DefaultContext.Default,
                Operators.ConvertToOctal,
                o,
                out octal)) {            
                if (!(octal is string) && !(octal is ExtensibleString))
                    throw PythonOps.TypeError("hex expected string type as return, got {0}", PythonOps.StringRepr(DynamicTypeOps.GetName(octal)));

                return octal;
            }
            throw TypeError("oct() argument cannot be converted to octal");
        }

        public static int Length(object o) {
            string s = o as String;
            if (s != null) return s.Length;

            IPythonContainer seq = o as IPythonContainer;
            if (seq != null) return seq.GetLength();

            ICollection ic = o as ICollection;
            if (ic != null) return ic.Count;

            object objres;
            if (!DynamicHelpers.GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default, Operators.Length, o, out objres))
                throw PythonOps.TypeError("len() of unsized object");
            int res = (int)objres;
            if (res < 0) {
                throw PythonOps.ValueError("__len__ should return >= 0, got {0}", res);
            }
            return res;
        }

        public static object CallWithContext(CodeContext context, object func, params object[] args) {
            ICallableWithCodeContext icc = func as ICallableWithCodeContext;
            if (icc != null) return icc.Call(context, args);

            return SlowCallWithContext(context, func, args);
        }

        private static object SlowCallWithContext(CodeContext context, object func, object[] args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.OperatorInvoke, new KeyValuePair<Operators, DynamicType>(Operators.Call, DynamicHelpers.GetDynamicType(func)));

            object res;
            if (!DynamicHelpers.GetDynamicType(func).TryInvokeBinaryOperator(context, Operators.Call, func, args, out res))
                throw UncallableError(func);

            return res;
        }

        public static Exception UncallableError(object func) {
            return PythonOps.TypeError("{0} is not callable", DynamicTypeOps.GetName(func));
        }

        /// <summary>
        /// Supports calling of functions that require an explicit 'this'
        /// Currently, we check if the function object implements the interface 
        /// that supports calling with 'this'. If not, the 'this' object is dropped
        /// and a normal call is made.
        /// </summary>
        public static object CallWithContextAndThis(CodeContext context, object func, object instance, params object[] args) {

            ICallableWithThis icc = func as ICallableWithThis;
            if (icc != null) {
                return icc.Call(context, instance, args);
            } else {
                // drop the 'this' and make the call
                return CallWithContext(context, func, args);
            }
        }

        public static object ToPythonType(DynamicMixin dt) {
            if (dt != null && dt != TypeCache.Object) {
                DynamicTypeSlot ret;
                if (dt.TryLookupSlot(DefaultContext.Default, Symbols.Class, out ret) &&
                    ret.GetType() == typeof(DynamicTypeValueSlot)) {
                    object tmp;
                    if (ret.TryGetValue(DefaultContext.Default, null, dt, out tmp)) {
                        return tmp;
                    }
                }
            }
            return dt;
        }

        public static object CallWithArgsTupleAndContext(CodeContext context, object func, object[] args, object argsTuple) {
            PythonTuple tp = argsTuple as PythonTuple;
            if (tp != null) {
                object[] nargs = new object[args.Length + tp.Count];
                for (int i = 0; i < args.Length; i++) nargs[i] = args[i];
                for (int i = 0; i < tp.Count; i++) nargs[i + args.Length] = tp[i];
                return CallWithContext(context, func, nargs);
            }

            List allArgs = List.MakeEmptyList(args.Length + 10);
            allArgs.AddRange(args);
            IEnumerator e = PythonOps.GetEnumerator(argsTuple);
            while (e.MoveNext()) allArgs.AddNoLock(e.Current);

            return CallWithContext(context, func, allArgs.GetObjectArray());
        }
       
        public static object CallWithArgsTupleAndKeywordDictAndContext(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
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

                // fast path
                IFancyCallable ic = func as IFancyCallable;
                if (ic != null) {
                    return ic.Call(context, largs.ToArray(), lnames.ToArray());
                } 

                // slow path
                object ret;
                if (DynamicHelpers.GetDynamicType(func).TryInvokeBinaryOperator(context, Operators.Call, func, new KwCallInfo(largs.ToArray(), lnames.ToArray()), out ret))
                    return ret;

                throw new Exception("this object is not callable with keyword parameters");                
            }
        }

        public static object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names) {
            IFancyCallable ic = func as IFancyCallable;
            if (ic != null) return ic.Call(context, args, names);

            object ret;
            if(DynamicHelpers.GetDynamicType(func).TryInvokeBinaryOperator(context, Operators.Call, func, new KwCallInfo(args, names), out ret)) {
                return ret;
            }

            throw PythonOps.TypeError("{0} object is not callable", DynamicHelpers.GetDynamicType(func));
        }

        public static object CallWithArgsTuple(object func, object[] args, object argsTuple) {
            PythonTuple tp = argsTuple as PythonTuple;
            if (tp != null) {
                object[] nargs = new object[args.Length + tp.Count];
                for (int i = 0; i < args.Length; i++) nargs[i] = args[i];
                for (int i = 0; i < tp.Count; i++) nargs[i + args.Length] = tp[i];
                return PythonCalls.Call(func, nargs);
            }

            List allArgs = List.MakeEmptyList(args.Length + 10);
            allArgs.AddRange(args);
            IEnumerator e = PythonOps.GetEnumerator(argsTuple);
            while (e.MoveNext()) allArgs.AddNoLock(e.Current);

            return PythonCalls.Call(func, allArgs.GetObjectArray());
        }

        public static object GetIndex(object o, object index) {
            ISequence seq = o as ISequence;
            if (seq != null) {
                Slice slice;
                if (index is int) return seq[(int)index];
                else if ((slice = index as Slice) != null) {
                    if (slice.Step == null) {
                        int start, stop;
                        slice.DeprecatedFixed(o, out start, out stop);

                        return seq.GetSlice(start, stop);
                    }

                    return seq[slice];
                }
                //???
            }

            string s = o as string;
            if (s != null) {
                if (index is int) return StringOps.GetItem(s, (int)index);
                else if (index is Slice) return StringOps.GetItem(s, (Slice)index);
                //???
            }

            IMapping map = o as IMapping;
            if (map != null) {
                return map[index];
            }
            return SlowGetIndex(o, index);
        }

        private static object SlowGetIndex(object o, object index) {
            object ret;
            IDictionary<object, object> dict = o as IDictionary<object, object>;
            if (dict != null) {
                if (dict.TryGetValue(index, out ret)) return ret;
            } 
            
            Array array = o as Array;
            if (array != null) {
                return ArrayOps.GetIndex(array, index);
            }

            IList list = o as IList;
            if (list != null) {
                int val;
                if (Converter.TryConvertToInt32(index, out val)) {
                    return list[val];
                }
            }

            DynamicType dt = o as DynamicType;
            if (dt != null) {
                if (dt == TypeCache.Array) {
                    Type[] types = TypeHelpers.GetTypesFromTuple(index);
                    if (types.Length != 1) throw PythonOps.TypeError("expected single type");

                    return DynamicHelpers.GetDynamicTypeFromType(types[0].MakeArrayType());
                } else {
                    PythonTuple tindex = index as PythonTuple;
                    if (tindex == null) {
                        DynamicType dti = index as DynamicType;
                        if (dti != null) {
                            tindex = PythonTuple.MakeTuple(dti);
                        } else {
                            throw PythonOps.TypeError("__getitem__ expected tuple, got {0}", PythonOps.StringRepr(DynamicTypeOps.GetName(index)));
                        }
                    }

                    DynamicType[] dta = new DynamicType[tindex.Count];
                    for (int i = 0; i < tindex.Count; i++) {
                        dta[i] = (DynamicType)tindex[i];
                    }

                    return DynamicHelpers.GetDynamicTypeFromType(dt.MakeGenericType(dta));
                }
            }

            BuiltinFunction bf = o as BuiltinFunction;
            if (bf != null) {
                return PythonBuiltinFunctionOps.GetItem(bf, index);
            }

            if (DynamicHelpers.GetDynamicType(o).TryInvokeBinaryOperator(DefaultContext.Default, Operators.GetItem, o, index, out ret)) {
                return ret;
            }

            throw PythonOps.AttributeError("{0} object has no attribute '__getitem__'",
                PythonOps.StringRepr(DynamicTypeOps.GetName(o)));
        }

        public static void SetIndexId(object o, SymbolId index, object value) {
            IAttributesCollection ad;
            if ((ad = o as IAttributesCollection) != null) {
                ad[index] = value;
            } else {
                SetIndex(o, SymbolTable.IdToString(index), value);
            }
        }

        public static void SetIndex(object o, object index, object value) {
            IMutableSequence seq = o as IMutableSequence;
            if (seq != null) {
                Slice slice;

                if (index is int) {
                    seq[(int)index] = value;
                    return;
                } else if ((slice = index as Slice) != null) {
                    if (slice.Step == null) {
                        SetDeprecatedSlice(o, value, seq, slice);
                    } else {
                        seq[slice] = value;
                    }

                    return;
                }
                //???
            }

            IMapping map = o as IMapping;
            if (map != null) {
                map[index] = value;
                return;
            }

            SlowSetIndex(o, index, value);
        }

        private static void SetDeprecatedSlice(object o, object value, IMutableSequence seq, Slice slice) {
            int start, stop;
            slice.DeprecatedFixed(o, out start, out stop);

            seq.SetSlice(start, stop, value);
        }

        private static void SlowSetIndex(object o, object index, object value) {
            Array array = o as Array;
            if (array != null) {
                ArrayOps.SetItem(array, index, value);
                return;
            }

            IList list = o as IList;
            if (list != null) {
                int val;
                if (Converter.TryConvertToInt32(index, out val)) {
                    list[val] = value;
                    return;
                }
            }

            object ret;
            if (!DynamicHelpers.GetDynamicType(o).TryInvokeTernaryOperator(DefaultContext.Default, Operators.SetItem, o, index, value, out ret)) {
                throw PythonOps.AttributeError("{0} object has no attribute '__setitem__'",
                    PythonOps.StringRepr(DynamicTypeOps.GetName(o)));
            }
        }

        public static void DelIndex(object o, object index) {
            IMutableSequence seq = o as IMutableSequence;
            if (seq != null) {
                if (index == null) {
                    throw PythonOps.TypeError("index must be integer or slice");
                }

                Slice slice;
                if (index is int) {
                    seq.DeleteItem((int)index);
                    return;
                } else if ((slice = index as Slice) != null) {
                    if (slice.Step == null) {
                        int start, stop;
                        slice.DeprecatedFixed(o, out start, out stop);

                        seq.DeleteSlice(start, stop);
                    } else
                        seq.DeleteItem((Slice)index);

                    return;
                }
            }

            IMapping map = o as IMapping;
            if (map != null) {
                map.DeleteItem(index);
                return;
            }

            object ret;
            if (!DynamicHelpers.GetDynamicType(o).TryInvokeBinaryOperator(DefaultContext.Default, Operators.DeleteItem, o, index, out ret)) {
                throw PythonOps.AttributeError("{0} object has no attribute '__delitem__'",
                    PythonOps.StringRepr(DynamicTypeOps.GetName(o)));
            }
        }

        public static bool TryGetBoundAttr(object o, SymbolId name, out object ret) {
            return TryGetBoundAttr(DefaultContext.Default, o, name, out ret);
        }

        private static Dictionary<AttrKey, DynamicSite<object, object>> _tryGetMemSites = new Dictionary<AttrKey, DynamicSite<object, object>>();
        
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
        
        public static bool TryGetBoundAttr(CodeContext context, object o, SymbolId name, out object ret) {
            DynamicSite<object, object> site;
            lock (_tryGetMemSites) {
                AttrKey key = new AttrKey(CompilerHelpers.GetType(o), name);
                if (!_tryGetMemSites.TryGetValue(key, out site)) {
                    _tryGetMemSites[key] = site = DynamicSite<object, object>.Create(GetMemberAction.Make(name, GetMemberBindingFlags.Bound | GetMemberBindingFlags.NoThrow));
                }
            }
            
            ret = site.Invoke(context, o);
            return ret != OperationFailed.Value;
        }

        public static bool HasAttr(CodeContext context, object o, SymbolId name) {
            object dummy;
            try {
                return TryGetBoundAttr(context, o, name, out dummy);
            } catch {
                return false;
            }
        }
        
        public static object GetBoundAttr(CodeContext context, object o, SymbolId name) {
            object ret;
            if (!TryGetBoundAttr(context, o, name, out ret)) {
                if (o is OldClass) {
                    throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'",
                        ((OldClass)o).Name, SymbolTable.IdToString(name));
                } else {
                    throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", DynamicTypeOps.GetName(DynamicHelpers.GetDynamicType(o)), SymbolTable.IdToString(name));
                }
            }
            return ret;           
        }

        public static void ObjectSetAttribute(CodeContext context, object o, SymbolId name, object value) {
            ICustomMembers ids = o as ICustomMembers;

            if (ids != null) {
                try {
                    ids.SetCustomMember(context, name, value);
                } catch (InvalidOperationException) {
                    throw AttributeErrorForMissingAttribute(o, name);
                }
                return;
            }

            if (!DynamicHelpers.GetDynamicType(o).TrySetNonCustomMember(context, o, name, value))
                throw AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetDynamicType(o), name);
        }

        public static void ObjectDeleteAttribute(CodeContext context, object o, SymbolId name) {
            ICustomMembers ifca = o as ICustomMembers;
            if (ifca != null) {
                try {
                    ifca.DeleteCustomMember(context, name);
                } catch (InvalidOperationException) {
                    throw AttributeErrorForMissingAttribute(o, name);
                }
                return;
            }

            object dummy;
            if (!DynamicHelpers.GetDynamicType(o).TryInvokeBinaryOperator(context, Operators.DeleteDescriptor, o, name, out dummy)) {                
                throw AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetDynamicType(o), name);
            }
        }

        public static object ObjectGetAttribute(CodeContext context, object o, SymbolId name) {
            ICustomMembers ifca = o as ICustomMembers;
            if (ifca != null) {
                return GetCustomMembers(context, ifca, name);
            }

            object value;
            if (DynamicHelpers.GetDynamicType(o).TryGetNonCustomMember(context, o, name, out value)) {
                return value;
            }            

            throw PythonOps.AttributeErrorForMissingAttribute(DynamicHelpers.GetDynamicType(o).Name, name);
        }

        private static object GetCustomMembers(CodeContext context, ICustomMembers ifca, SymbolId name) {
            object ret;
            if (ifca.TryGetBoundCustomMember(context, name, out ret)) return ret;

            if (ifca is OldClass) {
                throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'", ((OldClass)ifca).Name, SymbolTable.IdToString(name));
            } else {
                throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", DynamicHelpers.GetDynamicType(ifca).Name, SymbolTable.IdToString(name));
            }
        }

        public static void SetAttr(CodeContext context, object o, SymbolId name, object value) {
            ICustomMembers ids = o as ICustomMembers;

            if (ids != null) {
                try {
                    ids.SetCustomMember(context, name, value);
                } catch (InvalidOperationException) {
                    throw AttributeErrorForMissingAttribute(o, name);
                }
                return;
            }

            if (!DynamicHelpers.GetDynamicType(o).TrySetMember(context, o, name, value))
                throw AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetDynamicType(o), name);
        }

        public static Exception AttributeErrorForMissingOrReadonly(CodeContext context, DynamicType dt, SymbolId name) {
            DynamicTypeSlot dts;
            if (dt.TryResolveSlot(context, name, out dts)) {
                throw PythonOps.AttributeErrorForReadonlyAttribute(DynamicTypeOps.GetName(dt), name);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(DynamicTypeOps.GetName(dt), name);
        }

        public static Exception AttributeErrorForMissingAttribute(object o, SymbolId name) {
            DynamicType dt = o as DynamicType;
            if (dt != null)
                return PythonOps.AttributeErrorForMissingAttribute(dt.Name, name);

            return AttributeErrorForReadonlyAttribute(DynamicTypeOps.GetName(o), name);
        }


        public static IList<object> GetAttrNames(CodeContext context, object o) {
            IMembersList memList = o as IMembersList;

            if (memList != null) {

                return memList.GetCustomMemberNames(context);
            }

            List res = new List();
            foreach(SymbolId x in DynamicHelpers.GetDynamicType(o).GetMemberNames(context, o))
                res.AddNoLock(SymbolTable.IdToString(x));

            //!!! ugly, we need to check fro non-SymbolID keys
            ISuperDynamicObject dyno = o as ISuperDynamicObject;
            if (dyno != null) {
                IAttributesCollection iac = dyno.Dict;
                if (iac != null) {
                    foreach (object id in iac.Keys) {
                        if (!res.Contains(id)) res.Add(id);
                    }
                }
            }

            return res;
        }

        public static IDictionary<object, object> GetAttrDict(CodeContext context, object o) {
            ICustomMembers ids = o as ICustomMembers;
            if (ids != null) {
                return ids.GetCustomMemberDictionary(context);
            }

            IAttributesCollection iac = DynamicHelpers.GetDynamicType(o).GetMemberDictionary(context, o);
            if (iac != null) {
                return iac.AsObjectKeyedDictionary();
            }
            throw PythonOps.AttributeErrorForMissingAttribute(DynamicTypeOps.GetName(o), Symbols.Dict);
        }

        /// <summary>
        /// Called from generated code emitted by NewTypeMaker.
        /// </summary>
        public static void CheckInitializedAttribute(object o, object self, string name) {
            if (o == Uninitialized.Instance) {
                throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'",
                    DynamicHelpers.GetDynamicType(self),
                    name);
            }
        }               

        /// <summary>
        /// Handles the descriptor protocol for user-defined objects that may implement __get__
        /// </summary>
        public static object GetUserDescriptor(object o, object instance, object context) {
            if (o != null && o.GetType() == typeof(OldInstance)) return o;   // only new-style classes can have descriptors
            if (o is ISuperDynamicObject) {
                // slow, but only encountred for user defined descriptors.
                PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "__get__");
                object ret;
                if (TryInvokeOperator(DefaultContext.Default,
                    Operators.GetDescriptor,
                    o,
                    instance,
                    context,
                    out ret))
                    return ret;
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
            return TryInvokeOperator(DefaultContext.Default,
                Operators.SetDescriptor,
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
            return TryInvokeOperator(DefaultContext.Default,
                Operators.DeleteDescriptor,
                o,
                instance,
                out dummy);
        }

        public static object Invoke(object target, SymbolId name, params object[] args) {
            return PythonCalls.Call(PythonOps.GetBoundAttr(DefaultContext.Default, target, name), args);
        }

        public static object InvokeWithContext(CodeContext context, object target, SymbolId name, params object[] args) {
            return PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, target, name), args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryInvokeOperator(CodeContext context, Operators name, object target, out object ret) {
            return DynamicHelpers.GetDynamicType(target).TryInvokeUnaryOperator(context, name, target, out ret);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryInvokeOperator(CodeContext context, Operators name, object target, object other, out object ret) {
            return DynamicHelpers.GetDynamicType(target).TryInvokeBinaryOperator(context, name, target, other, out ret);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryInvokeOperator(CodeContext context, Operators name, object target, object value1, object value2, out object ret) {
            return DynamicHelpers.GetDynamicType(target).TryInvokeTernaryOperator(context, name, target, value1, value2, out ret);
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

        public static object IsMappingType(CodeContext context, object o) {
            if (o is IMapping || o is PythonDictionary || o is IDictionary<object, object> || o is IAttributesCollection) {
                return RuntimeHelpers.True;
            }
            object getitem;
            if (PythonOps.TryGetBoundAttr(context, o, Symbols.GetItem, out getitem)) {
                if (!context.ModuleContext.ShowCls) {
                    // in standard Python methods aren't mapping types, therefore
                    // if the user hasn't broken out of that box yet don't treat 
                    // them as mapping types.
                    if (o is BuiltinFunction) return RuntimeHelpers.False;
                }
                return RuntimeHelpers.True;
            }
            return RuntimeHelpers.False;
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
                ostep = Converter.ConvertToSliceIndex(step);
                if (ostep == 0) {
                    throw PythonOps.ValueError("step cannot be zero");
                }
            }

            if (start == null) {
                ostart = ostep > 0 ? 0 : length - 1;
            } else {
                ostart = Converter.ConvertToSliceIndex(start);
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
                ostop = Converter.ConvertToSliceIndex(stop);
                if (ostop < 0) {
                    ostop += length;
                    if (ostop < 0) {
                        ostop = Math.Min(length, -1);
                    }
                } else if (ostop > length) {
                    ostop = length;
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

        #region CLS Compatible exception factories

        public static Exception ValueError(string format, params object[] args) {
            return new ArgumentException(string.Format(format, args));
        }

        public static Exception KeyError(object key) {
            // create the .NET & Python exception, setting the Arguments on the
            // python exception to the invalid key
            Exception res = new KeyNotFoundException(string.Format("{0}", key));
            
            PythonOps.SetAttr(DefaultContext.Default,
                ExceptionConverter.ToPython(res),
                Symbols.Arguments,
                PythonTuple.MakeTuple(key));

            return res;
        }

        public static Exception KeyError(string format, params object[] args) {
            return new KeyNotFoundException(string.Format(format, args));
        }

        public static Exception StopIteration(string format, params object[] args) {
            return new StopIterationException(string.Format(format, args));
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
            return new PythonSystemExitException();
        }

        public static Exception SyntaxWarning(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode) {
            int line = span.Start.Line;
            string fileName = sourceUnit.GetSymbolDocument(line) ?? "?";

            if (sourceUnit != null) {
                message = String.Format("{0} ({1}, line {2})", message, fileName, line);
            }

            return SyntaxWarning(message, fileName, span.Start.Line, span.Start.Column, sourceUnit.GetCodeLine(line), Severity.FatalError);
        }

        public static SyntaxErrorException SyntaxError(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode) {
            switch (errorCode & ErrorCodes.ErrorMask) {
                case ErrorCodes.IndentationError:
                    return new PythonIndentationError(message, sourceUnit, span, errorCode, Severity.FatalError);

                case ErrorCodes.TabError:
                    return new PythonTabError(message, sourceUnit, span, errorCode, Severity.FatalError);

                default:
                    return new SyntaxErrorException(message, sourceUnit, span, errorCode, Severity.FatalError);
            }
        }

        #endregion


        public static Exception StopIteration() {
            return StopIteration("");
        }

        public static Exception InvalidType(object o, RuntimeTypeHandle handle) {
            Type type = Type.GetTypeFromHandle(handle);
            return TypeError("Object {0} is not of type {1}", o == null ? "None" : o, type);
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
            return TypeErrorForUnboundMethodCall(methodName, DynamicHelpers.GetDynamicTypeFromType(methodType), instance);
        }

        public static Exception TypeErrorForUnboundMethodCall(string methodName, DynamicType methodType, object instance) {
            string message = string.Format("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)",
                                           methodName, methodType.Name, DynamicHelpers.GetDynamicType(instance).Name);
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

        internal static Exception TypeErrorForIncompatibleObjectLayout(string prefix, DynamicType type, Type newType) {
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
                    ((OldClass)o).Name, name);
            } else {
                throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", GetPythonTypeName(o), name);
            }
        }

        public static Exception AttributeErrorForMissingAttribute(string typeName, SymbolId attributeName) {
            return PythonOps.AttributeError("'{0}' object has no attribute '{1}'", typeName, SymbolTable.IdToString(attributeName));
        }

        public static void InitializeForFinalization(object newObject) {
            IWeakReferenceable iwr = newObject as IWeakReferenceable;
            Debug.Assert(iwr != null);

            InstanceFinalizer nif = new InstanceFinalizer(newObject);
            iwr.SetFinalizer(new WeakRefTracker(nif, nif));
        }

        private static object FindMetaclass(CodeContext context, PythonTuple bases, IAttributesCollection dict) {
            // If dict['__metaclass__'] exists, it is used. 
            object ret;
            if (dict.TryGetValue(Symbols.MetaClass, out ret) && ret != null) return ret;

            //Otherwise, if there is at least one base class, its metaclass is used
            for (int i = 0; i < bases.Count; i++) {
                if (!(bases[i] is OldClass)) return DynamicHelpers.GetDynamicType(bases[i]);
            }

            //Otherwise, if there's a global variable named __metaclass__, it is used.
            if (context.Scope.ModuleScope.TryLookupName(context.LanguageContext, Symbols.MetaClass, out ret) && ret != null) {
                return ret;
            }

            //Otherwise, the classic metaclass (types.ClassType) is used.
            return TypeCache.OldInstance;
        }

        public static object MakeClass(CodeContext context, string name, object[] bases, string selfNames, Delegate body) {
            CodeContext bodyContext;
            CallTarget0 target;
            CallTargetWithContext0 targetWithContext;
            if ((target = body as CallTarget0) != null) {
                bodyContext = (CodeContext)target();
            } else if ((targetWithContext = body as CallTargetWithContext0) != null) {
                bodyContext = (CodeContext)((CallTargetWithContext0)body)(context);
            } else {
                bodyContext = (CodeContext)((CallTargetWithContextN)body)(context);
            }

            IAttributesCollection vars = bodyContext.Scope.Dict;

            foreach (object dt in bases) {
                if (dt is TypeGroup) {
                    object[] newBases = new object[bases.Length];
                    for (int i = 0; i < bases.Length; i++) {
                        TypeGroup tc = bases[i] as TypeGroup;
                        if (tc != null) {
                            Type nonGenericType;
                            if (!tc.TryGetNonGenericType(out nonGenericType)) {
                                throw PythonOps.TypeError("cannot derive from open generic types " + Builtin.repr(tc).ToString());
                            }
                            newBases[i] = DynamicHelpers.GetDynamicTypeFromType(nonGenericType);
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
            if (metaclass == TypeCache.OldInstance)
                return new OldClass(name, tupleBases, vars, selfNames);
            if (metaclass == TypeCache.DynamicType)
                return UserTypeBuilder.Build(context, name, tupleBases, vars);

            // eg:
            // def foo(*args): print args            
            // __metaclass__ = foo
            // class bar: pass
            // calls our function...
            EnsureMetaclassSite();

            return MetaclassSite.Invoke(context, metaclass, name, tupleBases, vars);
        }

        private static void EnsureMetaclassSite() {
            if (MetaclassSite == null) {
                Interlocked.CompareExchange<DynamicSite<object, string, PythonTuple, IAttributesCollection, object>>(
                    ref MetaclassSite,
                    RuntimeHelpers.CreateSimpleCallSite<object, string, PythonTuple, IAttributesCollection, object>(),
                    null
                );
            }
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
            return List.MakeEmptyList(10);
        }

        /// <summary>
        /// Python runtime helper to create a populated instnace of Python List object.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List MakeList(params object[] items) {
            return List.MakeList(items);
        }

        /// <summary>
        /// Python runtime helper to create an instance of Tuple
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static PythonTuple MakeTuple(params object[] items) {
            return PythonTuple.MakeTuple(items);
        }

        /// <summary>
        /// Python runtime helper to create an instance of an expandable tuple,
        /// tuple which can be expanded into individual elements for use in the
        /// context:    x[1, 2, 3]
        /// when calling .NET indexers
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static PythonTuple MakeExpandableTuple(params object[] items) {
            return PythonTuple.MakeExpandableTuple(items);
        }

        /// <summary>
        /// Python Runtime Helper for enumerator unpacking (tuple assignments, ...)
        /// Creates enumerator from the input parameter e, and then extracts 
        /// expected number of values, returning them as array
        /// </summary>
        /// <param name="e">object to enumerate</param>
        /// <param name="expected">expected number of objects to extract from the enumerator</param>
        /// <returns>
        /// array of objects (.Lengh == expected) if exactly expected objects are in the enumerator.
        /// Otherwise throws exception
        /// </returns>
        public static object[] GetEnumeratorValues(object e, int expected) {
            IEnumerator ie = PythonOps.GetEnumeratorForUnpack(e);

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
        public static object MakeSlice(object start, object stop, object step) {
            return new Slice(start, stop, step);
        }

        #region Standard I/O support

        public static void Write(object f, string text) {
            SystemState state = SystemState.Instance;

            if (f == null) f = state.stdout;
            if (f == null || f == Uninitialized.Instance) throw PythonOps.RuntimeError("lost sys.std_out");

            _writeSite.Invoke(PythonOps.GetBoundAttr(DefaultContext.Default, f, Symbols.ConsoleWrite), text);
        }

        private static object ReadLine(object f) {
            if (f == null || f == Uninitialized.Instance) throw PythonOps.RuntimeError("lost sys.std_in");
            return PythonOps.Invoke(f, Symbols.ConsoleReadLine);
        }

        public static void WriteSoftspace(object f) {
            if (CheckSoftspace(f)) {
                SetSoftspace(f, RuntimeHelpers.False);
                Write(f, " ");
            }
        }

        public static void SetSoftspace(object f, object value) {
            PythonOps.SetAttr(DefaultContext.Default, f, Symbols.Softspace, value);
        }

        public static bool CheckSoftspace(object f) {
            object result;
            if (PythonOps.TryGetBoundAttr(f, Symbols.Softspace, out result)) return PythonOps.IsTrue(result);
            return false;
        }

        // Must stay here for now because libs depend on it.
        public static void Print(object o) {
            SystemState state = SystemState.Instance;

            PrintWithDest(state.stdout, o);
        }

        public static void PrintNoNewline(object o) {
            SystemState state = SystemState.Instance;

            PrintWithDestNoNewline(state.stdout, o);
        }

        public static void PrintWithDest(object dest, object o) {
            PrintWithDestNoNewline(dest, o);
            Write(dest, "\n");
        }

        public static void PrintWithDestNoNewline(object dest, object o) {
            WriteSoftspace(dest);
            Write(dest, o == null ? "None" : ToString(o));
        }

        public static object ReadLineFromSrc(object src) {
            return ReadLine(src);
        }

        /// <summary>
        /// Prints newline into default standard output
        /// </summary>
        public static void PrintNewline() {
            SystemState state = SystemState.Instance;
            PrintNewlineWithDest(state.stdout);
        }

        /// <summary>
        /// Prints newline into specified destination. Sets softspace property to false.
        /// </summary>
        /// <param name="dest"></param>
        public static void PrintNewlineWithDest(object dest) {
            PythonOps.Write(dest, "\n");
            PythonOps.SetSoftspace(dest, RuntimeHelpers.False);
        }

        /// <summary>
        /// Prints value into default standard output with Python comma semantics.
        /// </summary>
        /// <param name="o"></param>
        public static void PrintComma(object o) {
            PrintCommaWithDest(SystemState.Instance.stdout, o);
        }

        /// <summary>
        /// Prints value into specified destination with Python comma semantics.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="o"></param>
        public static void PrintCommaWithDest(object dest, object o) {
            PythonOps.WriteSoftspace(dest);
            string s = o == null ? "None" : PythonOps.ToString(o);

            PythonOps.Write(dest, s);
            PythonOps.SetSoftspace(dest, !s.EndsWith("\n"));
        }        

        /// <summary>
        /// Handles output of the expression statement.
        /// Prints the value and sets the __builtin__._
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        public static void PrintExpressionValue(CodeContext context, object value) {
            if (value != null) {
                Print(PythonOps.StringRepr(value));
                SystemState.Instance.BuiltinModuleInstance.SetMemberAfter(context, "_", value);
            }
        }

        #endregion

        #region Import support

        /// <summary>
        /// Called from generated code for:
        /// 
        /// import spam.eggs
        /// </summary>
        public static object ImportTop(CodeContext context, string fullName) {
            return PythonEngine.CurrentEngine.Importer.Import(context, fullName, null);
        }

        /// <summary>
        /// Python helper method called from generated code for:
        /// 
        /// import spam.eggs as ham
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static object ImportBottom(CodeContext context, string fullName) {
            object module = PythonEngine.CurrentEngine.Importer.Import(context, fullName, null);

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
        public static object ImportWithNames(CodeContext context, string fullName, string[] names) {
            return PythonEngine.CurrentEngine.Importer.Import(context, fullName, PythonTuple.MakeTuple(names));
        }


        /// <summary>
        /// Imports one element from the module in the context of:
        /// 
        /// from module import a, b, c, d
        /// 
        /// Called repeatedly for all elements being imported (a, b, c, d above)
        /// </summary>
        public static object ImportFrom(CodeContext context, object module, string name) {
            return PythonEngine.CurrentEngine.Importer.ImportFrom(context, module, name);
        }

        /// <summary>
        /// Called from generated code for:
        /// 
        /// from spam import *
        /// </summary>
        public static void ImportStar(CodeContext context, string fullName) {
            object newmod = PythonEngine.CurrentEngine.Importer.Import(context, fullName, PythonTuple.MakeTuple("*"));

            ScriptModule pnew = newmod as ScriptModule;
            if (pnew != null) {
                object all;
                if (pnew.Scope.TryGetName(context.LanguageContext, Symbols.All, out all)) {
                    IEnumerator exports = PythonOps.GetEnumerator(all);

                    while (exports.MoveNext()) {
                        string name = exports.Current as string;
                        if (name == null) continue;

                        SymbolId fieldId = SymbolTable.StringToId(name);
                        context.Scope.SetName(fieldId, PythonOps.GetBoundAttr(context, newmod, fieldId));
                    }
                    return;
                }
            }

            foreach (object o in PythonOps.GetAttrNames(context, newmod)) {
                if (o != null) {
                    if (!(o is string)) throw PythonOps.TypeErrorForNonStringAttribute();
                    string name = o as string;
                    if (name.Length == 0) continue;
                    if (name[0] == '_') continue;

                    SymbolId fieldId = SymbolTable.StringToId(name);

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
        public static void UnqualifiedExec(CodeContext context, object code) {
            IAttributesCollection locals = null;
            IAttributesCollection globals = null;

            // if the user passes us a tuple we'll extract the 3 values out of it            
            PythonTuple codeTuple = code as PythonTuple;
            if (codeTuple != null && codeTuple.Count > 0 && codeTuple.Count <= 3) {
                code = codeTuple[0];

                if (codeTuple.Count > 1 && codeTuple[1] != null) {
                    globals = codeTuple[1] as IAttributesCollection;
                    if (globals == null) throw PythonOps.TypeError("globals must be dictionary or none");
                }

                if (codeTuple.Count > 2 && codeTuple[2] != null) {
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
        public static void QualifiedExec(CodeContext context, object code, IAttributesCollection globals, object locals) {
            PythonFile pf;
            Stream cs;

            bool line_feed = true;
            bool tryEvaluate = false;

            // TODO: use SourceUnitReader when available
            if ((pf = code as PythonFile) != null) {
                List lines = pf.ReadLines();

                StringBuilder fullCode = new StringBuilder();
                for (int i = 0; i < lines.Count; i++) {
                    fullCode.Append(lines[i]);
                }

                code = fullCode.ToString();
            } else if ((cs = code as Stream) != null) {

                using (StreamReader reader = new StreamReader(cs)) { // TODO: encoding? 
                    code = reader.ReadToEnd();
                }

                line_feed = false;
            }

            string str_code = code as string;

            if (str_code != null) {
                ScriptEngine engine = PythonEngine.CurrentEngine;
                SourceUnit code_unit = SourceUnit.CreateSnippet(engine, str_code);
                // in accordance to CPython semantics:
                code_unit.DisableLineFeedLineSeparator = line_feed;

                ScriptCode sc = PythonModuleOps.CompileFlowTrueDivision(code_unit, context.LanguageContext);
                code = new FunctionCode(sc);
                tryEvaluate = true; // do interpretation only on strings -- not on files, streams, or code objects
            }

            FunctionCode fc = code as FunctionCode;
            if (fc == null) {
                throw PythonOps.TypeError("arg 1 must be a string, file, Stream, or code object");
            }

            if (locals == null) locals = globals;
            if (globals == null) globals = new GlobalsDictionary(context.Scope);

            if (locals != null && PythonOps.IsMappingType(context, locals) != RuntimeHelpers.True) {
                throw PythonOps.TypeError("exec: arg 3 must be mapping or None");
            }

            if (!globals.ContainsKey(Symbols.Builtins)) {
                globals[Symbols.Builtins] = SystemState.Instance.modules["__builtin__"];
            }

            IAttributesCollection attrLocals = Builtin.GetAttrLocals(context, locals);

            Microsoft.Scripting.Scope scope = new Microsoft.Scripting.Scope(new Microsoft.Scripting.Scope(globals), attrLocals);

            fc.Call(context, scope, tryEvaluate);
        }

        #endregion        

        public static IEnumerator GetEnumeratorForIteration(object enumerable) {
            IEnumerator ie;
            if (!Converter.TryConvertToIEnumerator(enumerable, out ie)) {
                throw PythonOps.TypeError("iteration over non-sequence of type {0}",
                    PythonOps.StringRepr(DynamicHelpers.GetDynamicType(enumerable)));
            }
            return ie;
        }

        public static LanguageContext GetLanguageContext() {
            return DefaultContext.Default.LanguageContext;
        }

        #region Exception handling

        public static object PushExceptionHandler(Exception clrException) {
            ExceptionHelpers.PushExceptionHandler(clrException);

            RawException = clrException;
            GetExceptionInfo(); // force update of non-thread static exception info...
            return ExceptionConverter.ToPython(clrException);
        }

        public static void PopExceptionHandler() {
            ExceptionHelpers.PopExceptionHandler();

            // only clear after the last exception is out, we
            // leave the last thrown exception as our current
            // exception
            List<Exception> exceptions = ExceptionHelpers.CurrentExceptions;
            if (exceptions == null || exceptions.Count == 0) {
                PythonOps.RawException = null;
            }
        }

        public static object CheckException(object exception, object test) {
            Debug.Assert(exception != null);

            StringException strex;
            if (test is PythonTuple) {
                // we handle multiple exceptions, we'll check them one at a time.
                PythonTuple tt = test as PythonTuple;
                for (int i = 0; i < tt.Count; i++) {
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
            } else if (test is OldClass) {
                if (PythonOps.IsInstance(exception, test)) {
                    // catching a Python type.
                    return exception;
                }
            } else if (test is DynamicType) {
                if (PythonOps.IsSubClass(test as DynamicType, DynamicHelpers.GetDynamicTypeFromType(typeof(Exception)))) {
                    // catching a CLR exception type explicitly.
                    Exception clrEx = ExceptionConverter.ToClr(exception);
                    if (PythonOps.IsInstance(clrEx, test)) return clrEx;
                }
            }

            return null;
        }

        private static TraceBack CreateTraceBack(Exception e) {
            // user provided trace back
            object result;
            if (ExceptionUtils.TryGetData(e, typeof(TraceBack), out result)) {
                return (TraceBack)result;
            }

            DynamicStackFrame[] frames = DynamicHelpers.GetDynamicStackFrames(e, false);
            TraceBack tb = null;
            for (int i = frames.Length - 1; i >= 0; i--) {
                DynamicStackFrame frame = frames[i];

                PythonFunction fx = new Function0(frame.CodeContext, frame.GetMethodName(), null, ArrayUtils.EmptyStrings, ArrayUtils.EmptyObjects);

                TraceBackFrame tbf = new TraceBackFrame(
                    new GlobalsDictionary(frame.CodeContext.Scope),
                    new LocalsDictionary(frame.CodeContext.Scope),
                    fx.FunctionCode);

                fx.FunctionCode.SetFilename(frame.GetFileName());
                fx.FunctionCode.SetLineNumber(frame.GetFileLineNumber());
                tb = new TraceBack(tb, tbf);
                tb.SetLine(frame.GetFileLineNumber());
            }

            return tb;
        }

        /// <summary>
        /// Support for exception handling (called directly by with statement)
        /// </summary>
        public static PythonTuple GetExceptionInfo() {
            if (RawException == null) {
                return PythonTuple.MakeTuple(null, null, null);
            }

            object pyExcep = ExceptionConverter.ToPython(RawException);

            TraceBack tb = CreateTraceBack(RawException);
            SystemState.Instance.ExceptionTraceBack = tb;

            StringException se = pyExcep as StringException;
            if (se == null) {
                object excType = PythonOps.GetBoundAttr(DefaultContext.Default, pyExcep, Symbols.Class);
                SystemState.Instance.ExceptionType = excType;
                SystemState.Instance.ExceptionValue = pyExcep;

                return PythonTuple.MakeTuple(
                    excType,
                    pyExcep,
                    tb);
            }

            // string exceptions are special...  there tuple looks
            // like string, argument, traceback instead of
            //      type,   instance, traceback
            SystemState.Instance.ExceptionType = pyExcep;
            SystemState.Instance.ExceptionValue = se.Value;

            return PythonTuple.MakeTuple(
                pyExcep,
                se.Value,
                tb);
        }

        /// <summary>
        /// helper function for non-re-raise exceptions.
        /// 
        /// type is the type of exception to throwo or an instance.  If it 
        /// is an instance then value should be null.  
        /// 
        /// If type is a type then value can either be an instance of type,
        /// a Tuple, or a single value.  This case is handled by EC.CreateThrowable.
        /// </summary>
        public static Exception MakeException(object type, object value, object traceback) {
            Exception throwable;

            if (type == null && value == null && traceback == null) {
                // rethrow
                PythonTuple t = GetExceptionInfo();
                type = t[0];
                value = t[1];
                traceback = t[2];
            }

            if (type is Exception) {
                throwable = type as Exception;
            } else if (PythonOps.IsInstance(type, PythonEngine.CurrentEngine._exceptionType)) {
                throwable = ExceptionConverter.ToClr(type);
            } else if (type is string) {
                throwable = new StringException(type.ToString(), value);
            } else if (type is OldClass) {
                if (value == null) {
                    throwable = ExceptionConverter.CreateThrowable(type);
                } else {
                    throwable = ExceptionConverter.CreateThrowable(type, value);
                }
            } else if (type is OldInstance) {
                throwable = ExceptionConverter.ToClr(type);
            } else {
                throwable = PythonOps.TypeError("exceptions must be classes, instances, or strings (deprecated), not {0}", DynamicHelpers.GetDynamicType(type));
            }

            IDictionary dict = ExceptionUtils.GetDataDictionary(throwable);

            if (traceback != null) {
                TraceBack tb = traceback as TraceBack;
                if (tb == null) throw PythonOps.TypeError("traceback argument must be a traceback object");

                dict[typeof(TraceBack)] = tb;
            } else if (dict.Contains(typeof(TraceBack))) {
                dict.Remove(typeof(TraceBack));
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.Exceptions, throwable);

            return throwable;
        }

        public static void ClearDynamicStackFrames() {
            ExceptionHelpers.ClearDynamicStackFrames();
        }

        #endregion

        public static IAttributesCollection CopyAndVerifyDictionary(PythonFunction function, IDictionary dict) {
            foreach (object o in dict.Keys) {
                if (!(o is string)) {
                    throw TypeError("{0}() keywords most be strings", function.Name);
                }
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
                function.Name, 
                function.NormalArgumentCount,
                argCnt);
        }

        public static void AddDictionaryArgument(PythonFunction function, string name, object value, IAttributesCollection dict) {
            if (dict.ContainsObjectKey(name)) {
                throw TypeError("{0}() got multiple values for keyword argument '{1}'", function.Name, name);
            }

            dict.AddObjectKey(name, value);
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
            if (list.Count != 0) {
                return list.Pop(0);
            }

            throw function.BadArgumentError(argCnt);
        }

        public static void AddParamsArguments(List list, params object[] args) {
            for (int i = 0; i < args.Length; i++) {
                list.Insert(i, args[i]);
            }
        }

        /// <summary>
        /// Extracts an argument from either the dictionary or params
        /// </summary>
        public static object ExtractAnyArgument(PythonFunction function, string name, int argCnt, List list, IDictionary dict) {
            object val;
            if (dict.Contains(name)) {                
                val = dict[name];
                dict.Remove(name);
                return val;
            }

            if (list.Count != 0) {
                return list.Pop(0);
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

            throw RuntimeHelpers.TypeErrorForIncorrectArgumentCount(
                function.Name,
                function.NormalArgumentCount,
                function.Defaults.Length,
                argCnt,
                function.ExpandListPosition != -1,
                dict.Count > 0);
        }

        public static object GetParamsValueOrDefault(PythonFunction function, int index, List extraArgs) {
            if (extraArgs.Count > 0) {
                return extraArgs.Pop(0);
            }

            return function.GetDefaultValue(index);
        }

        public static object GetFunctionParameterValue(PythonFunction function, int index, string name, List extraArgs, IAttributesCollection dict) {
            if (extraArgs != null && extraArgs.Count > 0) {
                return extraArgs.Pop(0);
            }

            object val;
            if (dict != null && dict.TryGetObjectValue(name, out val)) {
                dict.RemoveObjectKey(name);
                return val;
            }

            return function.GetDefaultValue(index);
        }

        public static void CheckParamsZero(PythonFunction function, List extraArgs) {
            if (extraArgs.Count != 0) {
                throw function.BadArgumentError(extraArgs.Count + function.NormalArgumentCount);
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

        public static Exception UnexpectedKeywordArgumentError(PythonFunction function, string name) {
            return TypeError("{0}() got an unexpected keyword argument '{1}'", function.Name, name);
        }

        public static object InitializeUserTypeSlots(Type type) {
            return Tuple.MakeTuple(type, 
                CompilerHelpers.MakeRepeatedArray<object>(Uninitialized.Instance, Tuple.GetSize(type)));
        }

        public static bool IsClsVisible(CodeContext context) {
            PythonModuleContext pmc = context.ModuleContext as PythonModuleContext;
            return pmc == null || pmc.ShowCls;
        }

        public static object GetInitMember(CodeContext context, DynamicType type, object instance) {
            object value;
            bool res = type.TryGetNonCustomBoundMember(context, instance, Symbols.Init, out value);
            Debug.Assert(res);

            return value;
        }

        public static object GetMixedMember(CodeContext context, DynamicType type, object instance, SymbolId name) {
            foreach (DynamicType t in type.ResolutionOrder) {
                if (Mro.IsOldStyle(t)) {
                    OldClass oc = (OldClass)ToPythonType(t);
                    object ret;
                    if (oc.__dict__.TryGetValue(name, out ret)) {
                        if (instance != null) return oc.GetOldStyleDescriptor(context, ret, instance, oc);
                        return ret;
                    }
                } else {
                    DynamicTypeSlot dts;
                    if (t.TryLookupSlot(context, name, out dts)) {
                        if (instance != null) {
                            object ret;
                            if (dts.TryGetBoundValue(context, instance, type, out ret)) {
                                return ret;
                            }
                        }
                        return dts;
                    }
                }
            }

            throw AttributeErrorForMissingAttribute(type, name);
        }

        /// <summary>
        /// Registers a set of extension methods from the provided assemly.
        /// </summary>
        private static void RegisterLanguageAssembly(Assembly assembly) {
            object[] attrs = assembly.GetCustomAttributes(typeof(ExtensionTypeAttribute), false);
            foreach (ExtensionTypeAttribute et in attrs) {
                ExtendOneType(et, DynamicHelpers.GetDynamicTypeFromType(et.Extends));
            }
        }

        internal static void ExtendOneType(ExtensionTypeAttribute et, DynamicType dt) {
            // new-style extensions:
            ExtensionTypeAttribute.RegisterType(et.Extends, et.Type, dt);

            DynamicTypeExtender.ExtendType(dt, et.Type, et.Transformer);

            if (et.EnableDerivation) {
                DynamicTypeBuilder.GetBuilder(DynamicHelpers.GetDynamicTypeFromType(et.Extends)).SetIsExtensible();
            } else if (et.DerivationType != null) {
                DynamicTypeBuilder.GetBuilder(DynamicHelpers.GetDynamicTypeFromType(et.Extends)).SetExtensionType(et.DerivationType);
            }
        }

        #region OldInstance slicing

        /// <summary>
        /// Helper to return a Slice object for OldInstance slicing when only a start & stop are provided.
        /// </summary>
        public static Slice MakeOldStyleSlice(OldInstance self, object start, object stop) {
            Nullable<int> length = null;

            if (start == Type.Missing && stop == Type.Missing) {
                return new Slice(0, Int32.MaxValue);
            }

            object newStart = FixSliceIndex(self, start, ref length);
            if (newStart == Type.Missing) {
                if (IsNumericObject(stop)) {
                    newStart = 0;
                } else {
                    newStart = null;
                }
            }

            object newStop = FixSliceIndex(self, stop, ref length);
            if (newStop == Type.Missing) {
                if (IsNumericObject(start)) {
                    newStop = Int32.MaxValue;
                } else {
                    newStop = null;
                }
            }

            return new Slice(newStart, newStop);
        }

        /// <summary>
        /// Helper to determine if the value is a simple numeric type (int or big int or bool) - used for OldInstance
        /// deprecated form of slicing.
        /// </summary>
        public static bool IsNumericObject(object value) {
            return value is int || value is ExtensibleInt || value is BigInteger || value is Extensible<BigInteger> || value is bool;
        }

        /// <summary>
        /// Helper to determine if the type is a simple numeric type (int or big int or bool) - used for OldInstance
        /// deprecated form of slicing.
        /// </summary>
        internal static bool IsNumericType(Type t) {
            return t == typeof(int) ||
                t == typeof(bool) ||
                t == typeof(BigInteger) ||
                t.IsSubclassOf(typeof(ExtensibleInt)) ||
                t.IsSubclassOf(typeof(Extensible<BigInteger>));
        }

        /// <summary>
        /// Fixes a single value to be used for slicing.
        /// </summary>
        private static object FixSliceIndex(OldInstance self, object index, ref Nullable<int> length) {
            if (index != null) {
                BigInteger bi;
                ExtensibleInt ei;
                Extensible<BigInteger> el;

                if (index is int) {
                    index = NormalizeInt(self, (int)index, ref length);
                } else if (!Object.ReferenceEquals((bi = index as BigInteger), null)) {
                    index = NormalizeBigInteger(self, bi, ref length);
                } else if ((ei = index as ExtensibleInt) != null) {
                    index = NormalizeInt(self, ei.Value, ref length);
                } else if ((el = index as Extensible<BigInteger>) != null) {
                    index = NormalizeBigInteger(self, el.Value, ref length);
                } else if (index is bool) {
                    return ((bool)index) ? 1 : 0;
                }
            }
            return index;
        }

        /// <summary>
        /// For OldInstance slicing.  Fixes up a BigInteger and returns an integer w/ the length of the
        /// OldInstance added if the value is negative.
        /// </summary>
        private static object NormalizeBigInteger(OldInstance self, BigInteger bi, ref Nullable<int> length) {
            int val;
            if (bi < BigInteger.Zero) {
                GetLength(self, ref length);

                if (bi.AsInt32(out val)) {
                    return val + length;
                } else {
                    return Int32.MaxValue;
                }
            } else if (bi.AsInt32(out val)) {
                return val;
            }

            return Int32.MaxValue;
        }

        /// <summary>
        /// For OldInstance slicing.  Returns a normalized integer w/ the length of the
        /// OldInstance added if the value is negative.
        /// </summary>
        private static object NormalizeInt(OldInstance self, int index, ref Nullable<int> length) {
            if (index < 0) {
                GetLength(self, ref length);
                return index + length;
            }
            return index;
        }

        /// <summary>
        /// For OldInstance slicing.  Gets the length of the OldInstance, only gets the length
        /// once.
        /// </summary>
        private static void GetLength(OldInstance self, ref Nullable<int> length) {
            if (length != null) return;

            length = PythonOps.Length(self);
        }

        #endregion
    }
}
