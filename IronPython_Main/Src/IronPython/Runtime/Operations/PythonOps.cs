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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

#if !SILVERLIGHT
using Microsoft.Scripting.Actions.ComDispatch;
#endif

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Hosting;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using IronPython.Compiler;

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

        /// <summary> Singleton NotImplemented object of NotImplementedType.  Initialized after type has been created in static constructor </summary>
        public static readonly object NotImplemented;
        public static readonly object Ellipsis;

        // start-up code path sites
        [MultiRuntimeAware]
        private static FastDynamicSite<object, string, object> _writeSite;
        private readonly static Dictionary<AttrKey, DynamicSite<object, object>> _tryGetMemSites = new Dictionary<AttrKey, DynamicSite<object, object>>();

        // non-startup call sites
        [MultiRuntimeAware]
        private static FastDynamicSite<object, object, int> _CompareSite;
        [MultiRuntimeAware]
        private static DynamicSite<object, string, PythonTuple, IAttributesCollection, object> MetaclassSite;
        [MultiRuntimeAware]
        private static Dictionary<AttrKey, DynamicSite<object, object>> _deleteAttrSites;        
        [MultiRuntimeAware]
        private static Dictionary<AttrKey, DynamicSite<object, object, object>> _setAttrSites;
        [MultiRuntimeAware]
        private static FastDynamicSite<object, object, object> _getIndexSite, EqualSharedSite;
        [MultiRuntimeAware]
        private static Dictionary<Type, FastDynamicSite<object, string>> _toStrSites;
        // Site for implementing callable() builtin function and Python.IsCallable.
        [MultiRuntimeAware]
        private static DynamicSite<object, bool> _isCallableSites;
        [MultiRuntimeAware]
        private static FastDynamicSite<object, object, bool> EqualBooleanSharedSite;
            
        // Site for implementing dir() builtin function
        private static DynamicSite<object, IList<object>> _memberNamesSite;

        #endregion

        static PythonOps() {
            PythonTypeBuilder.TypeInitialized += new EventHandler<TypeCreatedEventArgs>(PythonTypeCustomizer.OnTypeInit);

            MakePythonTypeTable();
                        
            NotImplemented = NotImplementedTypeOps.EnsureInstance();
            Ellipsis = EllipsisTypeOps.EnsureInstance();
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
       
        public static bool IsCallable(CodeContext/*!*/ context, object o) {
            // This tells if an object can be called, but does not make a claim about the parameter list.
            // In 1.x, we could check for certain interfaces like ICallable*, but those interfaces were deprecated
            // in favor of dynamic sites. 
            // This is difficult to infer because we'd need to simulate the entire callbinder, which can include
            // looking for [SpecialName] call methods and checking for a rule from IDynamicObject. But even that wouldn't
            // be complete since sites require the argument list of the call, and we only have the instance here. 
            // Thus check a dedicated IsCallable operator. This lets each object describe if it's callable.


            // Invoke Operator.IsCallable on the object. 
            if (!_isCallableSites.IsInitialized) {
                _isCallableSites.EnsureInitialized(DoOperationAction.Make(Operators.IsCallable));
            }
            return _isCallableSites.Invoke(context, o);
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
                if (!PythonOps.IsCallable(handler))
                    throw PythonOps.TypeError("handler must be callable");

                errorHandlers[name] = handler;
            }
        }

#endif
        
        internal static PythonTuple LookupEncoding(CodeContext/*!*/ context, string encoding) {
            List<object> searchFunctions = PythonContext.GetContext(context).SearchFunctions;
            lock (searchFunctions) {
                for (int i = 0; i < searchFunctions.Count; i++) {
                    object res = PythonCalls.Call(searchFunctions[i], encoding);
                    if (res != null) return (PythonTuple)res;
                }
            }

            throw PythonOps.LookupError("unknown encoding: {0}", encoding);
        }

        internal static void RegisterEncoding(CodeContext/*!*/ context, object search_function) {
            if(!PythonOps.IsCallable(search_function))
                throw PythonOps.TypeError("search_function must be callable");

            List<object> searchFunctions = PythonContext.GetContext(context).SearchFunctions;

            lock (searchFunctions) {
                searchFunctions.Add(search_function);
            }
        }

        //!!! Temporarily left in so this checkin won't collide with Converter changes
        internal static string GetClassName(object obj) {
            return GetPythonTypeName(obj);
        }

        internal static string GetPythonTypeName(object obj) {
            OldInstance oi = obj as OldInstance;
            if (oi != null) return oi.__class__.__name__.ToString();
            else return PythonTypeOps.GetName(DynamicHelpers.GetPythonType(obj));
        }

        public static string StringRepr(object o) {
            return StringRepr(DefaultContext.Default, o);
        }

        public static string StringRepr(CodeContext/*!*/ context, object o) {
            if (o == null) return "None";

            string s = o as string;
            if (s != null) return StringOps.Quote(s);
            if (o is int) return ((int)o).ToString(CultureInfo.InvariantCulture);
            if (o is long) return ((long)o).ToString() + "L";
            if (o is BigInteger) return ((BigInteger)o).ToString() + "L";
            if (o is double) return DoubleOps.__str__((double)o);
            if (o is float) return DoubleOps.__str__((float)o);

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
                    return f.__repr__(context);
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
                    return ArrayOps.__repr__(a);
                } finally {
                    System.Diagnostics.Debug.Assert(index == infinite.Count - 1);
                    infinite.RemoveAt(index);
                }
            }

            object ret;
            PythonTypeOps.TryInvokeUnaryOperator(context, o, Symbols.Repr, out ret);    // repr's always defined somewhere
            return ret as string;
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

        public static string ToString(object o) {
            string x = o as string;
            Array ax;
            PythonType dt;
            OldClass oc;
            if (x != null) return x;
            if (o == null) return "None";
            if (o is double) return DoubleOps.__str__((double)o);
            if (o is float) return DoubleOps.__str__((float)o);
            if ((ax = o as Array) != null) return StringRepr(ax);
            if ((dt = o as PythonType) != null) return PythonTypeOps.__repr__(DefaultContext.Default, dt);
            if ((oc = o as OldClass) != null) return oc.ToString();

            object tostr;
            if (TryGetBoundAttr(o, Symbols.String, out tostr)) {
                FastDynamicSite<object, string> callSite;
                if (_toStrSites == null) {
                    Interlocked.CompareExchange(ref _toStrSites,
                        new Dictionary<Type, FastDynamicSite<object, string>>(),
                        null);
                }
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
                fo = m.im_func as PythonFunction;
            }

            minArgCnt = 0;
            maxArgCnt = 0;

            if (fo != null) {
                if ((fo.Flags & (FunctionAttributes.ArgumentList | FunctionAttributes.KeywordDictionary)) == 0) {
                    maxArgCnt = fo.NormalArgumentCount;
                    minArgCnt = fo.NormalArgumentCount - fo.Defaults.Length;

                    // take into account unbound methods / bound methods
                    if (m != null) {
                        if (m.im_self != null) {
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

            if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, Symbols.Positive, out ret) && 
                ret != PythonOps.NotImplemented) {
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
            else if (o is bool) return RuntimeHelpers.Int32ToObject((bool)o ? -1 : 0);

            object ret;
            if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, Symbols.OperatorNegate, out ret) &&
                ret != PythonOps.NotImplemented) {
                return ret;
            }

            throw PythonOps.TypeError("bad operand type for unary -");
        }

        public static bool IsSubClass(PythonType c, object typeinfo) {
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

            PythonType odt = DynamicHelpers.GetPythonType(o);
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
            if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, Symbols.OperatorOnesComplement, out ret) &&
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
            if (PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default,
                y,
                x,
                Symbols.Contains,
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
            if (PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default,
                y,
                x,
                Symbols.Contains,
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

        // TODO: Remove this method, assemblies get registered as packages?
        private static void MakePythonTypeTable() {
            RegisterLanguageAssembly(Assembly.GetExecutingAssembly());
            RuntimeHelpers.TypeExtended += new EventHandler<RuntimeHelpers.TypeExtendedEventArgs>(DynamicHelpers_TypeExtended);

            // TODO: Contest specific MRO?
            PythonTypeBuilder.GetBuilder(DynamicHelpers.GetPythonTypeFromType(typeof(bool))).AddInitializer(delegate(PythonTypeBuilder builder) {
                PythonTypeBuilder dtb = (PythonTypeBuilder)builder;
                builder.SetResolutionOrder(new PythonType[]{
                    TypeCache.Boolean,
                    TypeCache.Int32,
                    TypeCache.Object});
                dtb.SetBases(new PythonType[] { TypeCache.Int32 });
            });
        }

        private static void DynamicHelpers_TypeExtended(object sender, RuntimeHelpers.TypeExtendedEventArgs e) {
            PythonTypeExtender.ExtendType(DynamicHelpers.GetPythonTypeFromType(e.Extending), e.Extension);
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
                if (PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default, count, sequence, Symbols.OperatorReverseMultiply, out ret)) {
                    if (ret != NotImplemented) return ret;
                }
            }

            int icount = GetSequenceMultiplier(sequence, count);

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

        public static object Equal(object x, object y) {
            if (!EqualSharedSite.IsInitialized) {
                EqualSharedSite.EnsureInitialized(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Equals));
            }
            return EqualSharedSite.Invoke(x, y);
        }

        public static bool EqualRetBool(object x, object y) {
            //TODO just can't seem to shake these fast paths
            if (x is int && y is int) { return ((int)x) == ((int)y); }
            if (x is double && y is double) { return ((double)x) == ((double)y); }
            if (x is string && y is string) { return ((string)x).Equals((string)y); }

            if (!EqualBooleanSharedSite.IsInitialized) {
                EqualBooleanSharedSite.EnsureInitialized(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Equals));
            }

            return EqualBooleanSharedSite.Invoke(x, y);
        }

        public static int Compare(object x, object y) {
            return Compare(DefaultContext.Default, x, y);
        }

        public static int Compare(CodeContext/*!*/ context, object x, object y) {
            if (x == y) return 0;

            if (!_CompareSite.IsInitialized) {
                _CompareSite.EnsureInitialized(DefaultContext.Default, DoOperationAction.Make(Operators.Compare));
            }

            return _CompareSite.Invoke(x, y);
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

            if (DynamicHelpers.GetPythonType(x) != DynamicHelpers.GetPythonType(y)) {
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
                    name1 = PythonTypeOps.GetName(x);
                    name2 = PythonTypeOps.GetName(y);
                }
                diff = String.CompareOrdinal(name1, name2);
            } else {
                diff = (int)(IdDispenser.GetId(x) - IdDispenser.GetId(y));
            }

            if (diff < 0) return -1;
            if (diff == 0) return 0;
            return 1;
        }

        public static object GreaterThanHelper(CodeContext/*!*/ context, object self, object other) {
            return InternalCompare(context, Operators.GreaterThan, self, other);
        }

        public static object LessThanHelper(CodeContext/*!*/ context, object self, object other) {
            return InternalCompare(context, Operators.LessThan, self, other);
        }

        public static object GreaterThanOrEqualHelper(CodeContext/*!*/ context, object self, object other) {
            return InternalCompare(context, Operators.GreaterThanOrEqual, self, other);
        }

        public static object LessThanOrEqualHelper(CodeContext/*!*/ context, object self, object other) {
            return InternalCompare(context, Operators.LessThanOrEqual, self, other);
        }

        public static object InternalCompare(CodeContext/*!*/ context, Operators op, object self, object other) {
            object ret;
            if (PythonTypeOps.TryInvokeBinaryOperator(context, self, other, Symbols.OperatorToSymbol(op), out ret))
                return ret;

            return PythonOps.NotImplemented;
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

            if (PythonTypeOps.TryInvokeTernaryOperator(DefaultContext.Default, x, y, z, Symbols.OperatorPower, out ret)) {
                if(ret != PythonOps.NotImplemented) {
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
            IEnumerator ie = Converter.ConvertToIEnumerator(o);
            if (ie == null) {
                throw PythonOps.TypeError("{0} is not enumerable", StringRepr(o));
            }
            return ie;
        }

        public static IEnumerator GetEnumeratorForUnpack(object enumerable) {
            IEnumerator ie;
            if (!Converter.TryConvertToIEnumerator(enumerable, out ie)) {
                throw PythonOps.TypeError("unpack non-sequence of type {0}",
                    StringRepr(PythonTypeOps.GetName(enumerable)));
            }
            return ie;
        }

        public static long Id(object o) {
            return IdDispenser.GetId(o);
        }

        public static string HexId(object o) {
            return string.Format("0x{0:X16}", Id(o));
        }
        
        #region Hash operations
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
        // So we have hashing in the following layers:
        //   CommonHash(object) - initial entry for arbitrary System.Object
        //   Hash* - entry for a given type, checks for equality relationships. 
        //           Within this layer, calls flow from: Complex --> Double --> BigInt --> Int.
        //   DisjointHash* - hashes a given type that is disjoint from other types and so does not
        //                   need to deal with equality relationships..
        //   Object.GetHashCode() - DLR / CLR general hashing functions. These do not respect language-
        //       specific equality conventions.
        //
        // Layers should not call back up to a higher layer.
        

        static int HashInt(int i) {
            // Doubles and BigInts may overlap with ints. Those other hash functions will check
            // their domain and map to ours.
            return DisjointHashInt(i);
        }

        static int HashDouble(double d) {
            // Python allows equality between floats, ints, and big ints.
            if ((d % 1) == 0) {
                // This double represents an integer number, so it must hash like an integer number.
                if (Int32.MinValue <= d && d <= Int32.MaxValue) {
                    return HashInt((int) d);
                }
                // Big integer
                BigInteger b = BigInteger.Create(d);
                return HashBigInt(b);
            }
            return DisjointHashDouble(d);
        }

        static int HashBigInt(BigInteger b) {
            // Call the DLR's BigInteger hash function, which will return an int32 representation of
            // b if b is within the int32 range. We use that as an optimization for hashing, and 
            // assert the assumption below.
            int hash = b.GetHashCode();
#if DEBUG
            int i;
            if (b.AsInt32(out i)) {
                Debug.Assert(DisjointHashInt(i) == hash, "input:" + i);
            }
#endif
            return hash;
        }

        static int HashComplex(Complex64 c) {
            if (c.Imag == 0) {
                return HashDouble(c.Real);
            }
            return DisjointHashComplex(c);
        }

        #region Raw Hash functions for disjoint regions   
     
        // Bottom level of hashing. 
        // Callers have already done equality comparisons. At this point, we can use any hash technique we want,
        // including calling into Framework or DLR implementations of Object.GetHashCode.
        // Beware that those hash functions can change between releases of thoses libraries.               

        static int DisjointHashInt(int i) {
            return i;
        }

        static int DisjointHashDouble(double d) {
            Debug.Assert((d % 1) != 0); // If this was an int, caller should have delegated to DisjointHashInt()
            return d.GetHashCode();
        }

        static int DisjointHashComplex(Complex64 c) {
            Debug.Assert(c.Imag != 0); // if this was float, caller should have delegated to DisjointHashDouble()
            return c.GetHashCode();
        }

        #endregion // Raw Hash functions for disjoint regions

        /// <summary>
        /// Attempt to hash common known object values. This is shared by both SimpleHash() and Hash(). 
        /// </summary>
        /// <param name="o">the object to hash </param>
        /// <param name="hashvalue">the hash value, only valid if this function returns true.</param>
        /// <returns>true if this can hash the object, else false</returns>
        static bool CommonHash(object o, out int hashvalue) {
            if (o is int) {
                hashvalue = HashInt((int)o);
                return true;
            }
            if (o is string) {
                // avoid lookups on strings - A) We can stack overflow w/ Dict B) they don't define __hash__
                hashvalue = o.GetHashCode(); 
                return true;
            }
            if (o is double) {
                hashvalue = HashDouble((double)o);
                return true;
            }
            if (o == null) {
                hashvalue = NoneTypeOps.HashCode;
                return true;
            }
            if (o is char) {
                hashvalue = new String((char)o, 1).GetHashCode();
                return true;
            }
            if (o is BigInteger) {
                hashvalue = HashBigInt((BigInteger)o);
                return true;
            }
            if (o is Complex64) {
                hashvalue = HashComplex((Complex64)o);
                return true;
            }

            // Need to use another technique to hash.
            hashvalue = 0;
            return false;
        }

        public static int SimpleHash(object o) {
            int hash;
            if (CommonHash(o, out hash)) {
                return hash;
            }

            return o.GetHashCode();
        }

        public static int Hash(object o) {
            int hash;
            if (CommonHash(o, out hash)) {
                return hash;
            }

            IValueEquality ipe = o as IValueEquality;
            if (ipe != null) {
                // invoke operator dynamically to go through protocol wrapper override, if defined.
                
                object ret;
                if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, Symbols.Hash, out ret) &&
                    ret != PythonOps.NotImplemented) {
                    BigInteger bi = ret as BigInteger;
                    if (!Object.ReferenceEquals(bi, null)) {
                        // Python 2.5 defines the result of returning a long as hashing the long
                        return HashBigInt(bi);
                    }
                    return Converter.ConvertToInt32(ret);
                }
            }

            return o.GetHashCode();
        }

        #endregion // Hash operations

        public static object Hex(object o) {
            if (o is int) return Int32Ops.__hex__((int)o);
            else if (o is BigInteger) return BigIntegerOps.__hex__((BigInteger)o);

            object hex;
            if(PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default,
                o,
                Symbols.ConvertToHex,
                out hex)) {            
                if (!(hex is string) && !(hex is ExtensibleString))
                    throw PythonOps.TypeError("hex expected string type as return, got {0}", PythonOps.StringRepr(PythonTypeOps.GetName(hex)));

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
                    throw PythonOps.TypeError("hex expected string type as return, got {0}", PythonOps.StringRepr(PythonTypeOps.GetName(octal)));

                return octal;
            }
            throw TypeError("oct() argument cannot be converted to octal");
        }

        public static int Length(object o) {
            string s = o as String;
            if (s != null) return s.Length;

            ICollection ic = o as ICollection;
            if (ic != null) return ic.Count;

            object objres;
            if (!PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, Symbols.Length, out objres)) {
                throw PythonOps.TypeError("len() of unsized object");
            }

            int res = (int)objres;
            if (res < 0) {
                throw PythonOps.ValueError("__len__ should return >= 0, got {0}", res);
            }
            return res;
        }

        public static object CallWithContext(CodeContext/*!*/ context, object func, params object[] args) {
            return PythonCalls.Call(func, args);
        }

        public static Exception UncallableError(object func) {
            return PythonOps.TypeError("{0} is not callable", PythonTypeOps.GetName(func));
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
            if (dt != null && dt != TypeCache.Object) {
                PythonTypeSlot ret;
                if (dt.TryLookupSlot(DefaultContext.Default, Symbols.Class, out ret) &&
                    ret.GetType() == typeof(PythonTypeValueSlot)) {
                    object tmp;
                    if (ret.TryGetValue(DefaultContext.Default, null, dt, out tmp)) {
                        return tmp;
                    }
                }
            }
            return dt;
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

                return PythonCalls.CallWithKeywordArgs(func, largs.ToArray(), lnames.ToArray());
            }
        }

        public static object CallWithKeywordArgs(CodeContext/*!*/ context, object func, object[] args, string[] names) {
            return PythonCalls.CallWithKeywordArgs(func, args, names);
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

        public static object GetIndex(object o, object index) {
            if (!_getIndexSite.IsInitialized) {
                _getIndexSite.EnsureInitialized(DefaultContext.Default, DoOperationAction.Make(Operators.GetItem));
            }

            return _getIndexSite.Invoke(o, index);
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
                    if (slice.step == null) {
                        SetDeprecatedSlice(o, value, seq, slice);
                    } else {
                        seq[slice] = value;
                    }

                    return;
                }
                //???
            }

            SlowSetIndex(o, index, value);
        }

        private static void SetDeprecatedSlice(object o, object value, IMutableSequence seq, Slice slice) {
            int start, stop;
            slice.DeprecatedFixed(o, out start, out stop);

            seq.__setslice__(start, stop, value);
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
            if (!PythonTypeOps.TryInvokeTernaryOperator(DefaultContext.Default, o, index, value, Symbols.SetItem, out ret)) {
                throw PythonOps.AttributeError("{0} object has no attribute '__setitem__'",
                    PythonOps.StringRepr(PythonTypeOps.GetName(o)));
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
                    seq.__delitem__((int)index);
                    return;
                } else if ((slice = index as Slice) != null) {
                    if (slice.step == null) {
                        int start, stop;
                        slice.DeprecatedFixed(o, out start, out stop);

                        seq.__delslice__(start, stop);
                    } else
                        seq.__delitem__((Slice)index);

                    return;
                }
            }

            IDictionary<object, object> dict = o as IDictionary<object, object>;
            if (dict != null) {
                if (!dict.Remove(index)) {
                    throw PythonOps.KeyError(index);
                }                
                return;
            }

            object ret;
            if (!PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default, o, index, Symbols.DelItem, out ret)) {
                throw PythonOps.AttributeError("{0} object has no attribute '__delitem__'",
                    PythonOps.StringRepr(PythonTypeOps.GetName(o)));
            }
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
            DynamicSite<object, object, object> site;
            if (_setAttrSites == null) {
                Interlocked.CompareExchange(ref _setAttrSites, new Dictionary<AttrKey, DynamicSite<object, object, object>>(), null);
            }

            lock (_setAttrSites) {
                AttrKey key = new AttrKey(CompilerHelpers.GetType(o), name);
                if (!_setAttrSites.TryGetValue(key, out site)) {
                    _setAttrSites[key] = site = DynamicSite<object, object, object>.Create(SetMemberAction.Make(name));
                }
            }

            site.Invoke(context, o, value);
        }

        public static bool TryGetBoundAttr(CodeContext/*!*/ context, object o, SymbolId name, out object ret) {
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

        public static void DeleteAttr(CodeContext/*!*/ context, object o, SymbolId name) {
            DynamicSite<object, object> site;
            if (_deleteAttrSites == null) {
                Interlocked.CompareExchange(ref _deleteAttrSites, new Dictionary<AttrKey, DynamicSite<object, object>>(), null);
            }

            lock (_deleteAttrSites) {
                AttrKey key = new AttrKey(CompilerHelpers.GetType(o), name);
                if (!_deleteAttrSites.TryGetValue(key, out site)) {
                    _deleteAttrSites[key] = site = DynamicSite<object, object>.Create(DeleteMemberAction.Make(name));
                }
            }

            site.Invoke(context, o);            
        }

        public static bool HasAttr(CodeContext/*!*/ context, object o, SymbolId name) {
            object dummy;
            try {
                return TryGetBoundAttr(context, o, name, out dummy);
            } catch {
                return false;
            }
        }
        
        public static object GetBoundAttr(CodeContext/*!*/ context, object o, SymbolId name) {
            object ret;
            if (!TryGetBoundAttr(context, o, name, out ret)) {
                if (o is OldClass) {
                    throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'",
                        ((OldClass)o).Name, SymbolTable.IdToString(name));
                } else {
                    throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", PythonTypeOps.GetName(DynamicHelpers.GetPythonType(o)), SymbolTable.IdToString(name));
                }
            }
            return ret;           
        }

        public static void ObjectSetAttribute(CodeContext/*!*/ context, object o, SymbolId name, object value) {
            ICustomMembers ids = o as ICustomMembers;

            if (ids != null) {
                try {
                    ids.SetCustomMember(context, name, value);
                } catch (InvalidOperationException) {
                    throw AttributeErrorForMissingAttribute(o, name);
                }
                return;
            }

            if (!DynamicHelpers.GetPythonType(o).TrySetNonCustomMember(context, o, name, value))
                throw AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetPythonType(o), name);
        }

        public static void ObjectDeleteAttribute(CodeContext/*!*/ context, object o, SymbolId name) {
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
            if (!PythonTypeOps.TryInvokeBinaryOperator(context, o, name, Symbols.DeleteDescriptor, out dummy)) {                
                throw AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetPythonType(o), name);
            }
        }

        public static object ObjectGetAttribute(CodeContext/*!*/ context, object o, SymbolId name) {
            ICustomMembers ifca = o as ICustomMembers;
            if (ifca != null) {
                return GetCustomMembers(context, ifca, name);
            }

            object value;
            if (DynamicHelpers.GetPythonType(o).TryGetNonCustomMember(context, o, name, out value)) {
                return value;
            }            

            throw PythonOps.AttributeErrorForMissingAttribute(DynamicHelpers.GetPythonType(o).Name, name);
        }

        private static object GetCustomMembers(CodeContext/*!*/ context, ICustomMembers ifca, SymbolId name) {
            object ret;
            if (ifca.TryGetBoundCustomMember(context, name, out ret)) return ret;

            if (ifca is OldClass) {
                throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'", ((OldClass)ifca).Name, SymbolTable.IdToString(name));
            } else {
                throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", DynamicHelpers.GetPythonType(ifca).Name, SymbolTable.IdToString(name));
            }
        }

        public static Exception AttributeErrorForMissingOrReadonly(CodeContext/*!*/ context, PythonType dt, SymbolId name) {
            PythonTypeSlot dts;
            if (dt.TryResolveSlot(context, name, out dts)) {
                throw PythonOps.AttributeErrorForReadonlyAttribute(PythonTypeOps.GetName(dt), name);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(PythonTypeOps.GetName(dt), name);
        }

        public static Exception AttributeErrorForMissingAttribute(object o, SymbolId name) {
            PythonType dt = o as PythonType;
            if (dt != null)
                return PythonOps.AttributeErrorForMissingAttribute(dt.Name, name);

            return AttributeErrorForReadonlyAttribute(PythonTypeOps.GetName(o), name);
        }


        public static IList<object> GetAttrNames(CodeContext/*!*/ context, object o) {
            IMembersList memList = o as IMembersList;

            if (memList != null) {
                return memList.GetMemberNames(context);
            }

            List res = new List();

            if (o is IDynamicObject) {
                if (!_memberNamesSite.IsInitialized) {
                    _memberNamesSite.EnsureInitialized(DoOperationAction.Make(Operators.MemberNames));
                }
                foreach (object x in _memberNamesSite.Invoke(context, o)) {
                    res.AddNoLock(x);
                }
            }
            else {
                foreach (SymbolId x in DynamicHelpers.GetPythonType(o).GetMemberNames(context, o)) {
                    res.AddNoLock(SymbolTable.IdToString(x));
                }

#if !SILVERLIGHT
                if (o != null && ComObject.IsGenericComObject(o)) {
                    foreach (SymbolId symbol in ComObject.ObjectToComObject(o).GetMemberNames(context)) {
                        res.AddNoLock(SymbolTable.IdToString(symbol));
                    }
                }
#endif
            }

            //!!! ugly, we need to check fro non-SymbolID keys
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
            ICustomMembers ids = o as ICustomMembers;
            if (ids != null) {
                return ids.GetCustomMemberDictionary(context);
            }

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
                if (PythonTypeOps.TryInvokeTernaryOperator(DefaultContext.Default,
                    o,
                    instance,
                    context,
                    Symbols.GetDescriptor,
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
            return PythonTypeOps.TryInvokeTernaryOperator(DefaultContext.Default,
                o,
                instance,
                value,
                Symbols.SetDescriptor,
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

        public static object Invoke(object target, SymbolId name, params object[] args) {
            return PythonCalls.Call(PythonOps.GetBoundAttr(DefaultContext.Default, target, name), args);
        }

        public static object InvokeWithContext(CodeContext/*!*/ context, object target, SymbolId name, params object[] args) {
            return PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, target, name), args);
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
                return RuntimeHelpers.True;
            }
            object getitem;
            if ((o is IPythonObject || o is OldInstance) && PythonOps.TryGetBoundAttr(context, o, Symbols.GetItem, out getitem)) {
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
            string fileName = sourceUnit.GetSymbolDocument(line) ?? "?";

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

        #endregion


        public static Exception StopIteration() {
            return StopIteration("");
        }

        public static Exception InvalidType(object o, RuntimeTypeHandle handle) {
            return PythonOps.TypeErrorForTypeMismatch(PythonTypeOps.GetName(Type.GetTypeFromHandle(handle)), o);
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

        private static object FindMetaclass(CodeContext/*!*/ context, PythonTuple bases, IAttributesCollection dict) {
            // If dict['__metaclass__'] exists, it is used. 
            object ret;
            if (dict.TryGetValue(Symbols.MetaClass, out ret) && ret != null) return ret;

            //Otherwise, if there is at least one base class, its metaclass is used
            for (int i = 0; i < bases.__len__(); i++) {
                if (!(bases[i] is OldClass)) return DynamicHelpers.GetPythonType(bases[i]);
            }

            //Otherwise, if there's a global variable named __metaclass__, it is used.
            if (context.Scope.ModuleScope.TryLookupName(Symbols.MetaClass, out ret) && ret != null) {
                return ret;
            }

            //Otherwise, the classic metaclass (types.ClassType) is used.
            return TypeCache.OldInstance;
        }

        public static object MakeClass(CodeContext/*!*/ context, string name, object[] bases, string selfNames, CallTarget0 body) {
            CodeContext bodyContext = (CodeContext)body();

            IAttributesCollection vars = bodyContext.Scope.Dict;

            return MakeClass(context, name, bases, selfNames, vars);
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
                                throw PythonOps.TypeError("cannot derive from open generic types " + Builtin.repr(tc).ToString());
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
            if (metaclass == TypeCache.OldInstance)
                return new OldClass(name, tupleBases, vars, selfNames);
            if (metaclass == TypeCache.PythonType)
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
            if (!MetaclassSite.IsInitialized) {
                MetaclassSite.EnsureInitialized(CallAction.Make(3));
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
            return new List();
        }

        /// <summary>
        /// Python runtime helper to create a populated instance of Python List object.
        /// </summary>
        public static List MakeList(params object[] items) {
            return new List(items);
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
        public static List MakeEmptyList(int capacity) {
            return new List(capacity);
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
        /// Python runtime helper to create an instance of Tuple
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static PythonTuple MakeTupleFromSequence(object items) {
            return PythonTuple.Make(items);
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
        public static Slice MakeSlice(object start, object stop, object step) {
            return new Slice(start, stop, step);
        }

        #region Standard I/O support

        public static void Write(CodeContext/*!*/ context, object f, string text) {
            if (f == null) {
                f = PythonContext.GetContext(context).SystemStandardOut;
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

            if (!_writeSite.IsInitialized) {
                _writeSite.EnsureInitialized(DefaultContext.Default, CallAction.Make(1));
            }

            _writeSite.Invoke(PythonOps.GetBoundAttr(DefaultContext.Default, f, Symbols.ConsoleWrite), text);
        }

        private static object ReadLine(object f) {
            if (f == null || f == Uninitialized.Instance) throw PythonOps.RuntimeError("lost sys.std_in");
            return PythonOps.Invoke(f, Symbols.ConsoleReadLine);
        }

        public static void WriteSoftspace(CodeContext/*!*/ context, object f) {
            if (CheckSoftspace(f)) {
                SetSoftspace(f, RuntimeHelpers.False);
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

        public static object ReadLineFromSrc(object src) {
            return ReadLine(src);
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
        /// <param name="dest"></param>
        public static void PrintNewlineWithDest(CodeContext/*!*/ context, object dest) {
            PythonOps.Write(context, dest, "\n");
            PythonOps.SetSoftspace(dest, RuntimeHelpers.False);
        }

        /// <summary>
        /// Prints value into default standard output with Python comma semantics.
        /// </summary>
        /// <param name="o"></param>
        public static void PrintComma(CodeContext/*!*/ context, object o) {
            PrintCommaWithDest(context, PythonContext.GetContext(context).SystemStandardOut, o);
        }

        /// <summary>
        /// Prints value into specified destination with Python comma semantics.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="o"></param>
        public static void PrintCommaWithDest(CodeContext/*!*/ context, object dest, object o) {
            PythonOps.WriteSoftspace(context, dest);
            string s = o == null ? "None" : PythonOps.ToString(o);

            PythonOps.Write(context, dest, s);
            PythonOps.SetSoftspace(dest, !s.EndsWith("\n"));
        }        

        /// <summary>
        /// Handles output of the expression statement.
        /// Prints the value and sets the __builtin__._
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        public static void PrintExpressionValue(CodeContext/*!*/ context, object value) {
            if (value != null) {
                Print(context, PythonOps.StringRepr(value));
                PythonContext.GetContext(context).BuiltinModuleInstance.SetMemberAfter("_", value);
            }
        }

        #endregion

        #region Import support

        /// <summary>
        /// Called from generated code for:
        /// 
        /// import spam.eggs
        /// </summary>
        public static object ImportTop(CodeContext/*!*/ context, string fullName, int level) {
            return Importer.Import(context, fullName, null, level);
        }

        /// <summary>
        /// Python helper method called from generated code for:
        /// 
        /// import spam.eggs as ham
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
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
        public static void ImportStar(CodeContext/*!*/ context, string fullName, int level) {
            object newmod = Importer.Import(context, fullName, PythonTuple.MakeTuple("*"), level);

            Scope scope = newmod as Scope;
            if (scope != null) {
                object all;
                if (scope.TryGetName(Symbols.All, out all)) {
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
        public static void QualifiedExec(CodeContext/*!*/ context, object code, IAttributesCollection globals, object locals) {
            PythonFile pf;
            Stream cs;

            bool lineFeed = true;
            bool tryEvaluate = false;

            // TODO: use SourceUnitReader when available
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

                lineFeed = false;
            }

            string strCode = code as string;

            if (strCode != null) {
                SourceUnit source = context.LanguageContext.CreateSnippet(strCode, SourceCodeKind.Statements);
                // in accordance to CPython semantics:
                source.DisableLineFeedLineSeparator = lineFeed;


                ScriptCode compiledCode = source.Compile(Builtin.GetDefaultCompilerOptions(context, true, 0), ThrowingErrorSink.Default);
                code = new FunctionCode(compiledCode);
                tryEvaluate = true; // do interpretation only on strings -- not on files, streams, or code objects
            }

            FunctionCode fc = code as FunctionCode;
            if (fc == null) {
                throw PythonOps.TypeError("arg 1 must be a string, file, Stream, or code object");
            }

            if (locals == null) locals = globals;
            if (globals == null) globals = new PythonDictionary(new GlobalScopeDictionaryStorage(context.Scope));

            if (locals != null && PythonOps.IsMappingType(context, locals) != RuntimeHelpers.True) {
                throw PythonOps.TypeError("exec: arg 3 must be mapping or None");
            }

            if (!globals.ContainsKey(Symbols.Builtins)) {
                globals[Symbols.Builtins] = PythonContext.GetContext(context).SystemStateModules["__builtin__"];
            }

            IAttributesCollection attrLocals = Builtin.GetAttrLocals(context, locals);

            Scope scope = new Scope(new Scope(globals), attrLocals);
            scope.SetExtension(context.LanguageContext.ContextId, ((PythonModule)context.ModuleContext).Clone());

            fc.Call(new CodeContext(scope, context.LanguageContext), scope, tryEvaluate);
        }

        #endregion        

        public static IEnumerator GetEnumeratorForIteration(object enumerable) {
            IEnumerator ie;
            if (!Converter.TryConvertToIEnumerator(enumerable, out ie)) {
                throw PythonOps.TypeError("iteration over non-sequence of type {0}",
                    PythonOps.StringRepr(DynamicHelpers.GetPythonType(enumerable)));
            }
            return ie;
        }

        public static LanguageContext GetLanguageContext() {
            return DefaultContext.Default.LanguageContext;
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
        //      <function body>
        //
        //      // except:
        //        SetCurrentException($exception)
        //        <except body>
        //   
        //   finally {
        //      RestoreCurrentException($save)
        //   }

        // Called at the start of the except handlers to set the current exception. 
        public static object SetCurrentException(CodeContext/*!*/ context, Exception clrException) {
            Assert.NotNull(clrException);

            ExceptionHelpers.AssociateDynamicStackFrames(clrException);
            RawException = clrException;
            GetExceptionInfo(context); // force update of non-thread static exception info...
            return PythonExceptions.ToPython(clrException);
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

        // Check for thread abort exceptions.
        // This is necessary to be able to catch python's KeyboardInterrupt exceptions.
        // CLR restrictions require that this must be called from within a catch block. 
        public static void CheckThreadAbort() {
#if !SILVERLIGHT
            ThreadAbortException tae = RawException as ThreadAbortException;
            if (tae != null && tae.ExceptionState is Microsoft.Scripting.Shell.KeyboardInterruptException) {
                // ThreadAbort can only be reset within a catch block. Codegen must have emitted this call 
                // from a catch region. Else the abort is rethrown at the end of the catch.
                Thread.ResetAbort();
            }
#endif
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
                if (PythonOps.IsSubClass(test as PythonType, DynamicHelpers.GetPythonTypeFromType(typeof(PythonExceptions.BaseException)))) {
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

            DynamicStackFrame[] frames = RuntimeHelpers.GetDynamicStackFrames(e, false);
            TraceBack tb = null;
            for (int i = frames.Length - 1; i >= 0; i--) {
                DynamicStackFrame frame = frames[i];

                PythonFunction fx = new PythonFunction(frame.CodeContext, frame.GetMethodName(), null, ArrayUtils.EmptyStrings, ArrayUtils.EmptyObjects, FunctionAttributes.None);

                TraceBackFrame tbf = new TraceBackFrame(
                    new PythonDictionary(new GlobalScopeDictionaryStorage(frame.CodeContext.Scope)),
                    LocalScopeDictionaryStorage.GetDictionaryFromScope(frame.CodeContext.Scope),
                    fx.func_code);

                fx.func_code.SetFilename(frame.GetFileName());
                fx.func_code.SetLineNumber(frame.GetFileLineNumber());
                tb = new TraceBack(tb, tbf);
                tb.SetLine(frame.GetFileLineNumber());
            }

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
        /// <param name="ex">the exception to create a tuple for.</param>
        /// <returns>a tuple of (type, value, traceback)</returns>
        /// <remarks>This is called directly by the With statement so that it can get an exception tuple
        /// in its own private except handler without disturbing the thread-wide sys.exc_info(). </remarks>
        public static PythonTuple GetExceptionInfoLocal(CodeContext/*!*/ context, Exception ex) {
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
        /// Create at TypeError exception for when Raise() can't create the exception requested.  
        /// </summary>
        /// <param name="type">original type of exception requested</param>
        /// <returns>a TypeEror exception</returns>
        internal static Exception MakeExceptionTypeError(object type) {
            Exception throwable = PythonOps.TypeError("exceptions must be classes, instances, or strings (deprecated), not {0}", DynamicHelpers.GetPythonType(type));
            return throwable;
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
            Exception throwable;

            if (type == null && value == null && traceback == null) {
                // rethrow
                PythonTuple t = GetExceptionInfo(context);
                type = t[0];
                value = t[1];
                traceback = t[2];
            }

            PythonType pt;

            if (type is Exception) {
                throwable = type as Exception;
            } else if (type is PythonExceptions.BaseException) {
                throwable = PythonExceptions.ToClr(type);
            } else if ((pt = type as PythonType) != null && typeof(PythonExceptions.BaseException).IsAssignableFrom(pt.UnderlyingSystemType)) {
                throwable = PythonExceptions.CreateThrowableForRaise(pt, value);
            } else if (type is string) {
                throwable = new StringException(type.ToString(), value);
            } else if (type is OldClass) {
                if (value == null) {
                    throwable = new OldInstanceException((OldInstance)PythonCalls.Call(type));
                } else {
                    throwable = PythonExceptions.CreateThrowableForRaise((OldClass)type, value);
                }
            } else if (type is OldInstance) {
                throwable = new OldInstanceException((OldInstance)type);
            } else {
                throwable = MakeExceptionTypeError(type);
            }

            IDictionary dict = throwable.Data;

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
                throw TypeError("{0}() got multiple values for keyword argument '{1}'", function.__name__, name);
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

            throw RuntimeHelpers.TypeErrorForIncorrectArgumentCount(
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

        public static object GetFunctionParameterValue(PythonFunction function, int index, string name, List extraArgs, IAttributesCollection dict) {
            if (extraArgs != null && extraArgs.__len__() > 0) {
                return extraArgs.pop(0);
            }

            object val;
            if (dict != null && dict.TryGetObjectValue(name, out val)) {
                dict.RemoveObjectKey(name);
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

        public static Exception UnexpectedKeywordArgumentError(PythonFunction function, string name) {
            return TypeError("{0}() got an unexpected keyword argument '{1}'", function.__name__, name);
        }

        public static object InitializeUserTypeSlots(Type type) {
            return Tuple.MakeTuple(type, 
                CompilerHelpers.MakeRepeatedArray<object>(Uninitialized.Instance, Tuple.GetSize(type)));
        }

        public static bool IsClsVisible(CodeContext/*!*/ context) {
            PythonModule pmc = context.ModuleContext as PythonModule;
            return pmc == null || pmc.ShowCls;
        }

        public static object GetInitMember(CodeContext/*!*/ context, PythonType type, object instance) {
            object value;
            bool res = type.TryGetNonCustomBoundMember(context, instance, Symbols.Init, out value);
            Debug.Assert(res);

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
            foreach (PythonExtensionTypeAttribute et in attrs) {
                ExtendOneType(et, DynamicHelpers.GetPythonTypeFromType(et.Extends));
            }

#if !SILVERLIGHT
            PythonExtensionTypeAttribute cotet = new PythonExtensionTypeAttribute(ComObject.ComObjectType, typeof(ComOps));
            ExtendOneType(cotet, DynamicHelpers.GetPythonTypeFromType(cotet.Extends));
#endif
        }

        internal static void ExtendOneType(PythonExtensionTypeAttribute et, PythonType dt) {
            // new-style extensions:
            ExtensionTypeAttribute.RegisterType(et.Extends, et.ExtensionType);
            PythonBinder.RegisterType(et.Extends, et.ExtensionType);

            string name;
            if (!PythonTypeCustomizer.SystemTypes.TryGetValue(et.Extends, out name)) {
                NameConverter.TryGetName(et.Extends, out name);

                PythonTypeCustomizer.SystemTypes[et.Extends] = name;
            }

            PythonTypeExtender.ExtendType(dt, et.ExtensionType, et.Transformer);

            if (et.EnableDerivation) {
                PythonTypeBuilder.GetBuilder(DynamicHelpers.GetPythonTypeFromType(et.Extends)).SetIsExtensible();
            } else if (et.DerivationType != null) {
                PythonTypeBuilder.GetBuilder(DynamicHelpers.GetPythonTypeFromType(et.Extends)).SetExtensionType(et.DerivationType);
            }
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
            return t == typeof(int) ||
                t == typeof(bool) ||
                t == typeof(BigInteger) ||
                t.IsSubclassOf(typeof(Extensible<int>)) ||
                t.IsSubclassOf(typeof(Extensible<BigInteger>));
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
            return ((IPythonObject)o).PythonType.Version == version;
        }

        /// <summary>
        /// Helper method for DynamicSite rules that check the version of their dynamic object
        /// TODO - Remove this method for more direct field accesses
        /// </summary>
        /// <param name="o"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool CheckAlternateTypeVersion(object o, int version) {
            return ((IPythonObject)o).PythonType.AlternateVersion == version;
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

        internal static bool CheckingConvertToInt(object value) {
            return value is int || value is BigInteger || value is Extensible<int> || value is Extensible<BigInteger>;
        }

        internal static bool CheckingConvertToLong(object value) {
            return CheckingConvertToInt(value);
        }

        internal static bool CheckingConvertToFloat(object value) {
            return value is double || value is Extensible<double>;
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
            if (!CheckingConvertToInt(value)) return 0;
            return value;
        }

        public static object NonThrowingConvertToLong(object value) {
            if (!CheckingConvertToInt(value)) return BigInteger.Zero;
            return value;
        }

        public static object NonThrowingConvertToFloat(object value) {
            if (!CheckingConvertToFloat(value)) return 0D;
            return value;
        }

        public static object NonThrowingConvertToComplex(object value) {
            if (!CheckingConvertToComplex(value)) return new Complex64();
            return value;                            
        }

        public static object NonThrowingConvertToString(object value) {
            if (!CheckingConvertToString(value)) return null;
            return value;
        }

        public static object NonThrowingConvertToNonZero(object value) {
            if (!CheckingConvertToNonZero(value)) return RuntimeHelpers.False;
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

        public static object ThrowingConvertToNonZero(object value) {
            if (!CheckingConvertToNonZero(value)) throw TypeError("__nonzero__ should return bool or int, returned {0}", PythonTypeOps.GetName(value));
            return value;
        }
                
        #endregion

        public static bool SlotTryGetBoundValue(CodeContext/*!*/ context, PythonTypeSlot slot, object instance, PythonType owner, out object value) {
            return slot.TryGetBoundValue(context, instance, owner, out value);
        }

        public static bool SlotTryGetValue(CodeContext/*!*/ context, PythonTypeSlot slot, object instance, PythonType owner, out object value) {
            return slot.TryGetValue(context, instance, owner, out value);
        }

        public static bool SlotTrySetValue(CodeContext/*!*/ context, PythonTypeSlot slot, object instance, PythonType owner, object value) {
            return slot.TrySetValue(context, instance, owner, value);
        }

        public static bool SlotTryDeleteValue(CodeContext/*!*/ context, PythonTypeSlot slot, object instance, PythonType owner) {
            return slot.TryDeleteValue(context, instance, owner);
        }

        public static BoundBuiltinFunction/*!*/ MakeBoundBuiltinFunction(BuiltinFunction/*!*/ function, object/*!*/ target) {
            return new BoundBuiltinFunction(function, target);
        }

        public static BuiltinFunction/*!*/ GetBoundBuiltinFunctionTarget(BoundBuiltinFunction/*!*/ self) {
            return self.Target;
        }

        public static BuiltinFunction/*!*/ GetBuiltinMethodDescriptorTemplate(BuiltinMethodDescriptor/*!*/ descriptor) {
            return descriptor.Template;
        }

        public static int GetTypeVersion(PythonType type) {
            return type.Version;
        }

        public static int GetAlternateTypeVersion(PythonType type) {
            return type.AlternateVersion;
        }

        public static bool TypeHasGetAttribute(PythonType type) {
            return type.HasGetAttribute;
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

        public static Exception StaticAssignmentFromInstanceError(PropertyTracker tracker, bool isAssignment) {
            return new MissingMemberException(string.Format(isAssignment ? Resources.StaticAssignmentFromInstanceError : Resources.StaticAccessFromInstanceError, tracker.Name, tracker.DeclaringType.Name));
        }

        #region Function helpers

        public static PythonFunction MakeFunction(CodeContext/*!*/ context, string name, Delegate target, string[] argNames, object[] defaults,
            FunctionAttributes attributes, string docString, int lineNumber, string fileName) {
            PythonFunction ret = new PythonFunction(context, name, target, argNames, defaults, attributes);
            if (docString != null) ret.__doc__ = docString;
            ret.func_code.SetLineNumber(lineNumber);
            ret.func_code.SetFilename(fileName);
            ret.func_code.SetFlags(attributes);
            return ret;
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

        public static Exception FunctionBadArgumentError(PythonFunction func, int count) {
            return func.BadArgumentError(count);
        }

        public static Exception BadKeywordArgumentError(PythonFunction func, int count) {
            return func.BadKeywordArgumentError(count);
        }

        public static void FunctionPushFrame() {
            //HACK ALERT:
            // In interpreted mode, cap the recursion limit at 200, since our stack grows 30x faster than normal.
            //TODO: remove this when we switch to a non-recursive interpretation strategy
            if (PythonContext.GetPythonOptions(null).InterpretedMode) {
                if (PythonFunction.Depth > 200) throw PythonOps.RuntimeError("maximum recursion depth exceeded");
            }

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
            return NotImplemented;
        }

        public static CallAction MakeListCallAction(int count) {
            ArgumentInfo[] infos = CompilerHelpers.MakeRepeatedArray(ArgumentInfo.Simple, count);
            infos[count - 1] = new ArgumentInfo(Microsoft.Scripting.Ast.ArgumentKind.List);
            return CallAction.Make(new CallSignature(infos));
        }

        public static CallAction MakeSimpleCallAction(int count) {
            return CallAction.Make(count);
        }

        public static PythonTuple ValidateCoerceResult(object coerceResult) {
            if (coerceResult == null || coerceResult == PythonOps.NotImplemented) {
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

        public static object GeneratorCheckThrowableAndReturnSendValue(PythonGenerator self) {
            return self.CheckThrowableAndReturnSendValue();
        }

        public static ItemEnumerable CreateItemEnumerable(object baseObject) {
            return ItemEnumerable.Create(baseObject);
        }

        public static IEnumerable CreatePythonEnumerable(object baseObject) {
            return PythonEnumerable.Create(baseObject);
        }

        public static IEnumerator CreateItemEnumerator(object baseObject) {
            return ItemEnumerator.Create(baseObject);
        }

        public static IEnumerator CreatePythonEnumerator(object baseObject) {
            return PythonEnumerator.Create(baseObject);
        }

        public static object CheckUninitialized(object value, SymbolId name) {
            if (value == Uninitialized.Instance) {
                RuntimeHelpers.ThrowUnboundLocalError(name);
            }
            return value;
        }
    }
}
