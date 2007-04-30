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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Compiler;
using IronPython.Compiler.Generation;

using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Operations {

    /// <summary>
    /// Contains functions that are called directly from
    /// generated code to perform low-level runtime functionality.
    /// </summary>
    public static partial class Ops {
        #region Shared static data
        //  
        // All shared data must be sharable between multiple Python engines!
        //
        private static TopReflectedPackage _topPackage;

        private static List<object> InfiniteRepr {
            get {
                return ThreadStatics.Ops_InfiniteRepr;
            }
            set {
                ThreadStatics.Ops_InfiniteRepr = value;
            }
        }

        /// <summary> Singleton NotImplemented object of NotImplementedType.  Initialized after type has been created in static constructor </summary>
        public static readonly object NotImplemented;
        // TODO: Remove after the Builtin reference is gone. Builtin cannot access PythonOps so Ellipsis must be here temporarily.
        public static readonly object Ellipsis;
        //TODO update references to go straight to RuntimeHelpers
        public static readonly object True = RuntimeHelpers.True;
        //TODO update references to go straight to RuntimeHelpers
        public static readonly object False = RuntimeHelpers.False;
        //TODO update references to go straight to RuntimeHelpers
        public static readonly object[] EmptyObjectArray = RuntimeHelpers.EmptyObjectArray;

        
        /// <summary> Table of dynamicly generated delegates which are shared based upon method signature. </summary>
        private static Publisher<DelegateSignatureInfo, MethodInfo> dynamicDelegates = new Publisher<DelegateSignatureInfo, MethodInfo>();

        /// <summary> Dictionary of error handlers for string codecs. </summary>
        private static Dictionary<string, object> errorHandlers = new Dictionary<string, object>();
        /// <summary> Table of functions used for looking for additional codecs. </summary>
        private static List<object> searchFunctions = new List<object>();
        /// <summary> stored for copy_reg module, used for reduce protocol </summary>
        internal static BuiltinFunction NewObject;
        /// <summary> stored for copy_reg module, used for reduce protocol </summary>
        internal static BuiltinFunction PythonReconstructor;

        internal static object ExceptionType = ExceptionConverter.GetPythonException("Exception");

        #endregion

        static Ops() {
            // HACK: remove this when GetDynamicTypeFromType moves down
            DynamicHelpers.GetDynamicTypeFromType = Ops.GetDynamicTypeFromType;

            DynamicTypeBuilder.TypeInitialized += new EventHandler<TypeCreatedEventArgs>(PythonTypeCustomizer.OnTypeInit);

            MakeDynamicTypesTable();
                        
            Debug.Assert(NotImplementedTypeOps.Instance != null);
            Debug.Assert(EllipsisTypeOps.Instance != null);
            NotImplemented = NotImplementedTypeOps.Instance;
            // TODO: Remove after the reference from Builtins is gone.
            Ellipsis = EllipsisTypeOps.Instance;
        }

        public static object[] MakeArray(object o1) { return new object[] { o1 }; }
        public static object[] MakeArray(object o1, object o2) { return new object[] { o1, o2 }; }

        public static Tuple MakeTuple(params object[] items) {
            return Tuple.MakeTuple(items);
        }

        public static Tuple MakeExpandableTuple(params object[] items) {
            return Tuple.MakeExpandableTuple(items);
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

            return Ops.HasAttr(context, o, Symbols.Call);
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
                throw Ops.LookupError("unknown error handler name '{0}'", name);
        }

        internal static void RegisterEncodingError(string name, object handler) {
            if(!Ops.IsCallable(handler))
                throw Ops.TypeError("handler must be callable");

            errorHandlers[name] = handler;
        }

#endif
        
        internal static Tuple LookupEncoding(string encoding) {
            for (int i = 0; i < searchFunctions.Count; i++) {
                object res = PythonCalls.Call(searchFunctions[i], encoding);
                if (res != null) return (Tuple)res;
            }

            throw Ops.LookupError("unknown encoding: {0}", encoding);
        }

        internal static void RegisterEncoding(object search_function) {
            if(!Ops.IsCallable(search_function))
                throw Ops.TypeError("search_function must be callable");

            searchFunctions.Add(search_function);
        }

        //!!! Temporarily left in so this checkin won't collide with Converter changes
        internal static string GetClassName(object obj) {
            return GetPythonTypeName(obj);
        }

        internal static string GetPythonTypeName(object obj) {
            OldInstance oi = obj as OldInstance;
            if (oi != null) return oi.__class__.__name__.ToString();
            else return DynamicTypeOps.GetName(Ops.GetDynamicType(obj));
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

            return GetDynamicType(o).InvokeUnaryOperator(context, Operators.CodeRepresentation, o) as string;
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
                Ops.TryGetBoundAttr(o, Symbols.Keys, out keys) ? "{...}" : // user dictionary
                "...";
        }

        public static string ToString(object o) {
            if (o == null) return "None";
            if (o is double) return DoubleOps.ToString((double)o);
            if (o is float) return DoubleOps.ToString((float)o);
            if (o is Array) return StringRepr(o);

            object res;
            if (!Ops.GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default,
                Operators.ConvertToString,
                o,
                out res))
                throw Ops.TypeError("cannot invoke __str__");

            return (string)res;
        }


        public static object Repr(object o) {
            return StringRepr(o);
        }

        //TODO update references to go straight to RuntimeHelpers
        public static object Bool2Object(bool value) {
            return RuntimeHelpers.BooleanToObject(value);
        }

        public static Delegate GetDelegate(object o, Type delegateType) {
            return GetDelegate(o, delegateType, null);
        }

        public static Delegate GetDelegate(object o, Type delegateType, Action<Exception> exceptionHandler) {
            Debug.Assert(typeof(Delegate).IsAssignableFrom(delegateType));

            Delegate handler = o as Delegate;

            if (handler != null) return handler;

            MethodInfo invoke = delegateType.GetMethod("Invoke");

            if (invoke == null) {
                Debug.Assert(delegateType == typeof(Delegate) || delegateType == typeof(MulticastDelegate));
                // We could try to convert to some implicit delegate type like CallTarget 0 (since it would
                // have to be a subtype of System.Delegate). However, we would be guessing, and it is better
                // to require the user to chose the explicit signature that is desired.
                throw Ops.TypeError("cannot implicitly convert {0} to {1}; please specify precise delegate type",
                                    Ops.GetPythonTypeName(o), delegateType);
            }

            ParameterInfo[] pis = invoke.GetParameters();
            int expArgCnt = pis.Length;

            int minArgCnt, maxArgCnt;
            if (!IsCallableCompatible(o, expArgCnt, out minArgCnt, out maxArgCnt))
                return null;

            DelegateSignatureInfo dsi = new DelegateSignatureInfo(invoke.ReturnType, pis, exceptionHandler);
            MethodInfo methodInfo = dynamicDelegates.GetOrCreateValue(dsi,
                delegate() {
                    // creation code
                    return dsi.CreateNewDelegate();
                });

            return CodeGen.CreateDelegate(methodInfo, delegateType, o);
        }
        
        internal static bool IsCallableCompatible(object o, int expArgCnt, out int minArgCnt, out int maxArgCnt) {
            // if we have a python function make sure it's compatible...
            PythonFunction fo = o as PythonFunction;

            Method m = o as Method;
            if (m != null) {
                fo = m.Function as PythonFunction;
            }

            minArgCnt = 0;
            maxArgCnt = 0;

            if (fo != null) {
                if (fo is FunctionN == false) {
                    maxArgCnt = fo.ArgCount;
                    minArgCnt = fo.ArgCount - fo.FunctionDefaults.Count;

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
            return true;
        }

        public static object ConvertTo(object o, Type toType) {
            return Converter.Convert(o, toType);
        }

        /// <summary>
        /// ToPython() wraps a CLI object with a PythonEngine object for cases where the PythonEngine does not want
        /// to deal with the CLI object directly. However, the general philosophy is to avoid using wrappers as
        /// that interferes with interoperability with the CLI world. Hence, there should be *very few* cases where
        /// wrappers are required. Try *really hard* to avoid wrappers.
        /// </summary>
        public static object ToPython(Type type, object o) {
            if (type == typeof(bool)) return Bool2Object((bool)o);  // preserve object identity o

            return o;
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
            else if (Ops.TryInvokeOperator(DefaultContext.Default,
                Operators.Positive,
                o,
                out ret))
                return ret;

            if (GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default, Operators.Positive, o, out ret) &&
                ret != Ops.NotImplemented)
                return ret;

            throw Ops.TypeError("bad operand type for unary +");
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
            if (GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default, Operators.Negate, o, out ret) &&
                ret != Ops.NotImplemented)
                return ret;

            throw Ops.TypeError("bad operand type for unary -");
        }

        public static bool IsSubClass(DynamicType c, object typeinfo) {
            if (c == null) throw Ops.TypeError("issubclass: arg 1 must be a class");
            if (typeinfo == null) throw Ops.TypeError("issubclass: arg 2 must be a class");

            Tuple pt = typeinfo as Tuple;
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
                if (!Ops.TryGetBoundAttr(typeinfo, Symbols.Bases, out bases)) {
                    //!!! deal with classes w/ just __bases__ defined.
                    throw Ops.TypeErrorForBadInstance("issubclass(): {0} is not a class nor a tuple of classes", typeinfo);
                }

                IEnumerator ie = Ops.GetEnumerator(bases);
                while (ie.MoveNext()) {
                    DynamicType baseType = ie.Current as DynamicType;

                    if (baseType == null) {
                        OldClass ocType = ie.Current as OldClass;
                        if (ocType == null) throw Ops.TypeError("expected type, got {0}", Ops.GetDynamicType(ie.Current));

                        baseType = ocType.TypeObject;
                    }

                    if (c.IsSubclassOf(baseType)) return true;
                }
                return false;
            }

            return c.IsSubclassOf(dt);
        }

        public static bool IsInstance(object o, object typeinfo) {
            if (typeinfo == null) throw Ops.TypeError("isinstance: arg 2 must be a class, type, or tuple of classes and types");

            Tuple tt = typeinfo as Tuple;
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

            DynamicType odt = Ops.GetDynamicType(o);
            if (IsSubClass(odt, typeinfo)) {
                return true;
            }

            object cls;
            if (Ops.TryGetBoundAttr(o, Symbols.Class, out cls) &&
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
            if (!Ops.TryGetBoundAttr(cls, Symbols.Bases, out bases)) {
                return false;   // no bases, cannot be subclass
            }
            Tuple tbases = bases as Tuple;
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
            if (GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default, Operators.OnesComplement, o, out ret) &&
                ret != Ops.NotImplemented)
                return ret;


            throw Ops.TypeError("bad operand type for unary ~");
        }

        public static object Not(object o) {
            return IsTrue(o) ? False : True;
        }

        public static object Is(object x, object y) {
            return x == y ? True : False;
        }

        public static bool IsRetBool(object x, object y) {
            return x == y;
        }

        public static object IsNot(object x, object y) {
            return x != y ? True : False;
        }

        public static bool IsNotRetBool(object x, object y) {
            return x != y;
        }

        public static object Increment(object o)
        {
            Debug.Assert(false, "Not yet implemented: Ops.Increment");
            return null;
        }

        public static object Decrement(object o)
        {
            Debug.Assert(false, "Not yet implemented: Ops.Decrement");
            return null;
        }

        public static object In(object x, object y) {
            if (y is IDictionary) {
                return Bool2Object(((IDictionary)y).Contains(x));
            }

            if (y is IList) {
                return Bool2Object(((IList)y).Contains(x));
            }

            string ys;
            if ((ys = y as string) != null) {
                string s = x as string;
                if (s == null) {
                    if (x is char) {
                        return (ys.IndexOf((char)x) != -1) ? True : False;
                    }
                    throw TypeError("'in <string>' requires string as left operand");
                }
                return ys.Contains(s) ? True : False;
            }

            if (y is char) {
                return In(x, y.ToString());
            }

            object contains;
            if (Ops.TryInvokeOperator(DefaultContext.Default,
                Operators.Contains,
                y,
                x,
                out contains)) {
                return Ops.IsTrue(contains) ? True : False;
            }

            IEnumerator e = GetEnumerator(y);
            while (e.MoveNext()) {
                if (Ops.EqualRetBool(e.Current, x)) return True;
            }

            return False;
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
            if (Ops.TryInvokeOperator(DefaultContext.Default,
                Operators.Contains,
                y,
                x,
                out contains)) {
                return Ops.IsTrue(contains);
            }

            IEnumerator e = GetEnumerator(y);
            while (e.MoveNext()) {
                if (Ops.EqualRetBool(e.Current, x)) return true;
            }

            return false;
        }

        public static object NotIn(object x, object y) {
            return Not(In(x, y));  //???
        }

        public static bool NotInRetBool(object x, object y) {
            return !InRetBool(x, y);  //???
        }

        //        public static object GetDynamicType1(object o) {
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
        //        public static object GetDynamicType2(object o) {
        //            Type ty = o.GetType();
        //            int hc = ty.GetHashCode();
        //
        //            return oas[hc%1];
        //        }

        // TODO: Remove this method, assemblies get registered as packages?
        private static void MakeDynamicTypesTable() {
            RegisterAssembly(Assembly.GetExecutingAssembly());

            DynamicType dt = Ops.GetDynamicTypeFromType(typeof(DynamicType)); 

            DynamicType.SetDynamicType(typeof(None), NoneTypeOps.MakeDynamicType());
            DynamicType.SetDynamicType(typeof(Ellipsis), EllipsisTypeOps.MakeDynamicType());
            DynamicType.SetDynamicType(typeof(NotImplemented), NotImplementedTypeOps.MakeDynamicType());

            // TODO: Remove impersonation
            DynamicTypeBuilder.GetBuilder(GetDynamicTypeFromType(typeof(SymbolDictionary))).SetImpersonationType(typeof(PythonDictionary));

            // TODO: Contest specific MRO?
            DynamicTypeBuilder.GetBuilder(GetDynamicTypeFromType(typeof(bool))).AddInitializer(delegate(DynamicMixinBuilder builder) {
                DynamicTypeBuilder dtb = (DynamicTypeBuilder)builder;
                builder.SetResolutionOrder(new DynamicType[]{
                    TypeCache.Boolean,
                    TypeCache.Int32,
                    TypeCache.Object});
                dtb.SetBases(new DynamicType[] { TypeCache.Int32 });
            });


            //!!! until this table moves to Microsoft.Scripting this needs to be initialized.
            DynamicTypeSlot dummy;
            dt.TryLookupSlot(DefaultContext.Default, Symbols.Name, out dummy);
            //!!! end region that needstogo
        }

        public static DynamicType GetDynamicTypeFromClsOnlyType(Type ty) {            
            DynamicType ret = DynamicType.GetDynamicType(ty);
            if (ret != null) return ret;

            ret = ReflectedTypeBuilder.Build(ty);

            return SaveDynamicType(ty, ret);
        }

        public static DynamicType SaveDynamicType(Type ty, DynamicType dt) {
            return DynamicType.SetDynamicType(ty, dt);
        }
        public static DynamicType GetDynamicTypeFromType(Type ty) {
            if (ty == null) throw Ops.TypeError("Expected type, got NoneType");

            PerfTrack.NoteEvent(PerfTrack.Categories.DictInvoke, "TypeLookup " + ty.FullName);

            DynamicType ret = DynamicType.GetDynamicType(ty);
            if (ret != null) return ret;

            ret = ReflectedTypeBuilder.Build(ty);

            return SaveDynamicType(ty, ret);
        }        

        public static DynamicType GetDynamicType(object o) {
            IDynamicObject dt = o as IDynamicObject;
            if (dt != null) return dt.DynamicType;

            if (o == null) return NoneTypeOps.TypeInstance;

            return GetDynamicTypeFromType(o.GetType());
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
                if (Ops.TryInvokeOperator(DefaultContext.Default, Operators.ReverseMultiply, count, sequence, out ret)) {
                    if (ret != NotImplemented) return ret;
                }
            }

            int icount = Converter.ConvertToInt32(count);
            if (icount < 0) icount = 0;
            return multiplier(sequence, icount);
        }

        private static FastDynamicSite<object, object, object> EqualSharedSite =
            new FastDynamicSite<object, object, object>(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Equal));

        public static object Equal(object x, object y) {
            return EqualSharedSite.Invoke(x, y);
        }
        private static FastDynamicSite<object, object, bool> EqualBooleanSharedSite =
            new FastDynamicSite<object, object, bool>(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Equal));

        public static bool EqualRetBool(object x, object y) {
            //TODO just can't seem to shake these fast paths
            if (x is int && y is int) { return ((int)x) == ((int)y); }
            if (x is double && y is double) { return ((double)x) == ((double)y); }
            if (x is string && y is string) { return ((string)x).Equals((string)y); }

            return EqualBooleanSharedSite.Invoke(x, y);
        }



        private static int ConvertToCompareInt(object x) {
            int res;
            if (Converter.TryConvertToInt32(x, out res)) {
                return res;
            }

            BigInteger bi;
            if (Converter.TryConvertToBigInteger(x, out bi)) {
                if (bi > BigInteger.Zero) return 1;
                else if (bi < BigInteger.Zero) return -1;
                return 0;
            }

            throw Ops.TypeErrorForBadInstance("Bad return type from comparison: {0}", x);
        }
        public static int Compare(object x, object y) {
            return Compare(DefaultContext.Default, x, y);
        }

        public static int Compare(CodeContext context, object x, object y) {
            if (x == y) return 0;

            // built-in types need to be special cased here (like for equals)
            // because they don't implement our interfaces.

            // it'd be nice to check for null ahead of time, but we can't...
            // the user could have defined __cmp__ on a class and have it
            // compare specially against null it's self.

            object ret = Ops.NotImplemented;
            if (x is int) {
                ret = Int32Ops.Compare(context, (int)x, y);
            } else if (x is ExtensibleInt) {
                ret = ((ExtensibleInt)x).Compare(context, y);
            } else if (x is double) {
                ret = DoubleOps.Compare(context, (double)x, y);
            } else if (x is BigInteger) {
                ret = BigIntegerOps.Compare(context, (BigInteger)x, y);
            } else if (x is string && y is string) {
                int temp = string.CompareOrdinal((string)x, (string)y);
                return (temp == 0) ? 0 : (temp > 0 ? 1 : -1);
            } else if (x is Complex64 || y is Complex64) {
                return ComplexOps.TrueCompare(context, x, y);
            } else if (x is bool) {
                ret = Int32Ops.Compare(context, ((bool)x) ? 1 : 0, y);
            } else if (x is long) {
                ret = BigIntegerOps.Compare(context, (long)x, y);
            } else if (x is ulong) {
                ret = BigIntegerOps.Compare(context, (ulong)x, y);
            } else if (x is short) {
                ret = Int32Ops.Compare(context, (int)(short)x, y);
            } else if (x is ushort) {
                ret = Int32Ops.Compare(context, (int)(ushort)x, y);
            } else if (x is byte) {
                ret = Int32Ops.Compare(context, (int)(byte)x, y);
            } else if (x is sbyte) {
                ret = Int32Ops.Compare(context, (int)(sbyte)x, y);
            } else if (x is decimal) {
                ret = DoubleOps.Compare(context, (double)(decimal)x, y);
            } else if (x is uint) {
                ret = BigIntegerOps.Compare(context, (long)(uint)x, y);
            } else if (x == null) {
                if (y.GetType().IsPrimitive || y is BigInteger) {
                    // built-in type that doesn't implement our comparable
                    // interfaces, being compared against null, go ahead
                    // and skip the rest of the checks.
                    return -1;
                }
            //} else if (x is Extensible<Complex64> || y is Extensible<Complex64>) {
            //    ret = Extensible<Complex64>.TrueCompare(x, y);
            } else if (x is char) {
                ret = CharOps.Compare((char)x, y);
            }

            if (ret != Ops.NotImplemented) return ConvertToCompareInt(ret);

            return SlowCompare(x, y);
        }

        private static int SlowCompare(object x, object y) {
            object ret;
            ret = TryRichCompare(DefaultContext.Default, x, y);
            if (ret != Ops.NotImplemented) return ConvertToCompareInt(ret);

            Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

            IComparable c = x as IComparable;
            if (c != null) {
                if (xType != null && xType != yType) {
                    object z;
                    try {
                        if (Converter.TryConvert(y, xType, out z)) {
                            int res = c.CompareTo(z);
                            return res < 0 ? -1 : res > 0 ? 1 : 0;
                        }
                    } catch {
                    }
                } else {
                    int res = c.CompareTo(y);
                    return res < 0 ? -1 : res > 0 ? 1 : 0;
                }
            }
            c = y as IComparable;
            if (c != null) {
                if (yType != null && xType != yType) {
                    try {
                        object z;
                        if (Converter.TryConvert(x, yType, out z)) {
                            int res = c.CompareTo(z);
                            return res < 0 ? 1 : res > 0 ? -1 : 0;
                        }
                    } catch {
                    }
                } else {
                    int res = c.CompareTo(x);
                    return res < 0 ? -1 : res > 0 ? 1 : 0;
                }
            }

            return CompareTypes(x, y);
        }

        public static object CompareEqual(int res) {
            return res == 0 ? Ops.True : Ops.False;
        }

        public static object CompareNotEqual(int res) {
            return res == 0 ? Ops.False : Ops.True;
        }

        public static object CompareGreaterThan(int res) {
            return res > 0 ? Ops.True : Ops.False;
        }

        public static object CompareGreaterThanOrEqual(int res) {
            return res >= 0 ? Ops.True : Ops.False;
        }

        public static object CompareLessThan(int res) {
            return res < 0 ? Ops.True : Ops.False;
        }

        public static object CompareLessThanOrEqual(int res) {
            return res <= 0 ? Ops.True : Ops.False;
        }

        public static bool CompareTypesEqual(object x, object y) {
            return Ops.CompareTypes(x, y) == 0;
        }

        public static bool CompareTypesNotEqual(object x, object y) {
            return Ops.CompareTypes(x, y) != 0;
        }

        public static bool CompareTypesGreaterThan(object x, object y) {
            return Ops.CompareTypes(x, y) > 0;
        }

        public static bool CompareTypesLessThan(object x, object y) {
            return Ops.CompareTypes(x, y) < 0;
        }

        public static bool CompareTypesGreaterThanOrEqual(object x, object y) {
            return Ops.CompareTypes(x, y) >= 0;
        }

        public static bool CompareTypesLessThanOrEqual(object x, object y) {
            return Ops.CompareTypes(x, y) <= 0;
        }

        public static int CompareTypes(object x, object y) {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            string name1, name2;
            int diff;

            if (Ops.GetDynamicType(x) != Ops.GetDynamicType(y)) {
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
                    name1 = Ops.GetDynamicType(x).Name;
                    name2 = Ops.GetDynamicType(y).Name;
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

        private static object InternalCompare(CodeContext context, Operators op, object self, object other) {
            object ret;
            if (Ops.GetDynamicType(self).TryInvokeBinaryOperator(context, op, self, other, out ret))
                return ret;

            return Ops.NotImplemented;
        }

        private static object RichEqualsHelper(object self, object other) {
            object res;

            if (Ops.GetDynamicType(self).TryInvokeBinaryOperator(DefaultContext.Default, Operators.Equal, self, other, out res))
                return res;

            return Ops.NotImplemented;
        }

        /// <summary>
        /// Attempts a Python rich comparison (see PEP 207)
        /// </summary>
        private static object TryRichCompare(CodeContext context, object x, object y) {
            object ret = Ops.NotImplemented;

            DynamicType xType = Ops.GetDynamicType(x);
            DynamicType yType = Ops.GetDynamicType(y);

            bool tryRich = true;
            if (xType == yType) {
                // use __cmp__ first if it's defined
                if (Ops.GetDynamicType(x).TryInvokeBinaryOperator(context, Operators.Compare, x, y, out ret)) {
                    if (ret != Ops.NotImplemented) {
                        return ret;
                    }

                    if (xType != TypeCache.OldInstance) {
                        // try __cmp__ backwards for new-style classes and don't fallback to
                        // rich comparisons if available
                        ret = InternalCompare(context, Operators.Compare, y, x);
                        if (ret != Ops.NotImplemented) return -1 * Converter.ConvertToInt32(ret);
                        tryRich = false;
                    }
                }
            }

            // next try equals, return 0 if we match.
            if (tryRich) {
                ret = RichEqualsHelper(x, y);
                if (ret != Ops.NotImplemented) {
                    if (Ops.IsTrue(ret)) return 0;
                } else if (y != null) {
                    // try the reverse
                    ret = RichEqualsHelper(y, x);
                    if (ret != Ops.NotImplemented && Ops.IsTrue(ret)) return 0;
                }

                // next try less than
                ret = LessThanHelper(context, x, y);
                if (ret != Ops.NotImplemented) {
                    if (Ops.IsTrue(ret)) return -1;
                } else if (y != null) {
                    // try the reverse
                    ret = GreaterThanHelper(context, y, x);
                    if (ret != Ops.NotImplemented && Ops.IsTrue(ret)) return -1;
                }

                // finally try greater than
                ret = GreaterThanHelper(context, x, y);
                if (ret != Ops.NotImplemented) {
                    if (Ops.IsTrue(ret)) return 1;
                } else if (y != null) {
                    //and the reverse
                    ret = LessThanHelper(context, y, x);
                    if (ret != Ops.NotImplemented && Ops.IsTrue(ret)) return 1;
                }

                if (xType != yType) {
                    // finally try __cmp__ if our types are different
                    ret = InternalCompare(context, Operators.Compare, x, y);
                    if (ret != Ops.NotImplemented) return Ops.CompareToZero(ret);

                    ret = InternalCompare(context, Operators.Compare, y, x);
                    if (ret != Ops.NotImplemented) return -1 * Ops.CompareToZero(ret);
                }
            }

            return Ops.NotImplemented;           
        }

        public static int CompareToZero(object value) {
            double val;
            if (Converter.TryConvertToDouble(value, out val)) {
                if (val > 0) return 1;
                if (val < 0) return -1;
                return 0;
            }
            throw Ops.TypeErrorForBadInstance("unable to compare type {0} with 0 ", value);
        }

        public static int CompareArrays(object[] data0, int size0, object[] data1, int size1) {
            int size = Math.Min(size0, size1);
            for (int i = 0; i < size; i++) {
                int c = Ops.Compare(data0[i], data1[i]);
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
                throw Ops.ValueError("complex modulo");
            }

            if (GetDynamicType(x).TryInvokeTernaryOperator(DefaultContext.Default, Operators.Power, x, y, z, out ret) && ret != Ops.NotImplemented)
                return ret;

            throw Ops.TypeErrorForBinaryOp("power with modulus", x, y);
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
                throw Ops.TypeError("{0} is not enumerable", StringRepr(o));
            }
            return ie;
        }

        public static IEnumerator GetEnumeratorForUnpack(object enumerable) {
            IEnumerator ie;
            if (!Converter.TryConvertToIEnumerator(enumerable, out ie)) {
                throw Ops.TypeError("unpack non-sequence of type {0}",
                    StringRepr(Ops.GetDynamicType(enumerable)));
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
                Ops.GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default,
                    Operators.ValueHash,
                    o,
                    out ret);
                if (ret != Ops.NotImplemented) {
                    return Converter.ConvertToInt32(ret);
                }
            }

            return o.GetHashCode();
        }

        public static object Hex(object o) {
            if (o is int) return Int32Ops.Hex((int)o);
            else if (o is BigInteger) return BigIntegerOps.Hex((BigInteger)o);

            object hex;
            if(Ops.TryInvokeOperator(DefaultContext.Default,
                Operators.ConvertToHex,
                o,
                out hex)) {            
                if (!(hex is string) && !(hex is ExtensibleString))
                    throw Ops.TypeError("hex expected string type as return, got {0}", Ops.StringRepr(Ops.GetDynamicType(hex)));

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
            if(Ops.TryInvokeOperator(DefaultContext.Default,
                Operators.ConvertToOctal,
                o,
                out octal)) {            
                if (!(octal is string) && !(octal is ExtensibleString))
                    throw Ops.TypeError("hex expected string type as return, got {0}", Ops.StringRepr(Ops.GetDynamicType(octal)));

                return octal;
            }
            throw TypeError("oct() argument cannot be converted to octal");
        }

        public static int Length(object o) {
            string s = o as String;
            if (s != null) return s.Length;

            ISequence seq = o as ISequence;
            if (seq != null) return seq.GetLength();

            ICollection ic = o as ICollection;
            if (ic != null) return ic.Count;

            object objres;
            if (!GetDynamicType(o).TryInvokeUnaryOperator(DefaultContext.Default, Operators.Length, o, out objres))
                throw Ops.TypeError("len() of unsized object");
            int res = (int)objres;
            if (res < 0) {
                throw Ops.ValueError("__len__ should return >= 0, got {0}", res);
            }
            return res;
        }

        public static object CallWithContext(CodeContext context, object func, params object[] args) {
            ICallableWithCodeContext icc = func as ICallableWithCodeContext;
            if (icc != null) return icc.Call(context, args);

            return SlowCallWithContext(context, func, args);
        }

        private static object SlowCallWithContext(CodeContext context, object func, object[] args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.OperatorInvoke, new KeyValuePair<Operators, DynamicType>(Operators.Call, Ops.GetDynamicType(func)));

            object res;
            if (!GetDynamicType(func).TryInvokeBinaryOperator(context, Operators.Call, func, args, out res))
                throw Ops.TypeError("{0} is not callable", Ops.GetDynamicType(func));

            return res;
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
            Tuple tp = argsTuple as Tuple;
            if (tp != null) {
                object[] nargs = new object[args.Length + tp.Count];
                for (int i = 0; i < args.Length; i++) nargs[i] = args[i];
                for (int i = 0; i < tp.Count; i++) nargs[i + args.Length] = tp[i];
                return CallWithContext(context, func, nargs);
            }

            List allArgs = List.MakeEmptyList(args.Length + 10);
            allArgs.AddRange(args);
            IEnumerator e = Ops.GetEnumerator(argsTuple);
            while (e.MoveNext()) allArgs.AddNoLock(e.Current);

            return CallWithContext(context, func, allArgs.GetObjectArray());
        }
       
        public static object CallWithArgsTupleAndKeywordDictAndContext(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            IDictionary kws = kwDict as IDictionary;
            if (kws == null && kwDict != null) throw Ops.TypeError("argument after ** must be a dictionary");

            if ((kws == null || kws.Count == 0) && names.Length == 0) {
                List<object> largs = new List<object>(args);
                if (argsTuple != null) {
                    foreach(object arg in Ops.GetCollection(argsTuple))
                        largs.Add(arg);
                }
                return CallWithContext(context, func, largs.ToArray());
            } else {
                List<object> largs;

                if (argsTuple != null && args.Length == names.Length) {
                    Tuple tuple = argsTuple as Tuple;
                    if (tuple == null) tuple = new Tuple(argsTuple);

                    largs = new List<object>(tuple);
                    largs.AddRange(args);
                } else {
                    largs = new List<object>(args);
                    if (argsTuple != null) {
                        largs.InsertRange(args.Length - names.Length, Tuple.Make(argsTuple));
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
                if (Ops.GetDynamicType(func).TryInvokeBinaryOperator(context, Operators.Call, func, new KwCallInfo(largs.ToArray(), lnames.ToArray()), out ret))
                    return ret;

                throw new Exception("this object is not callable with keyword parameters");                
            }
        }

        // TODO: Must stay here for now, because GetDynamicType fan-out is here.
        public static object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names) {
            IFancyCallable ic = func as IFancyCallable;
            if (ic != null) return ic.Call(context, args, names);

            object ret;
            if(GetDynamicType(func).TryInvokeBinaryOperator(context, Operators.Call, func, new KwCallInfo(args, names), out ret)) {
                return ret;
            }

            throw Ops.TypeError("{0} object is not callable", Ops.GetDynamicType(func));
        }

        // TODO: Must stay here for now, because of GetEnumerator
        public static object CallWithArgsTuple(object func, object[] args, object argsTuple) {
            Tuple tp = argsTuple as Tuple;
            if (tp != null) {
                object[] nargs = new object[args.Length + tp.Count];
                for (int i = 0; i < args.Length; i++) nargs[i] = args[i];
                for (int i = 0; i < tp.Count; i++) nargs[i + args.Length] = tp[i];
                return PythonCalls.Call(func, nargs);
            }

            List allArgs = List.MakeEmptyList(args.Length + 10);
            allArgs.AddRange(args);
            IEnumerator e = Ops.GetEnumerator(argsTuple);
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
                    if (types.Length != 1) throw Ops.TypeError("expected single type");

                    return Ops.GetDynamicTypeFromType(types[0].MakeArrayType());
                } else {
                    Tuple tindex = index as Tuple;
                    if (tindex == null) {
                        DynamicType dti = index as DynamicType;
                        if (dti != null) {
                            tindex = Tuple.MakeTuple(dti);
                        } else {
                            throw Ops.TypeError("__getitem__ expected tuple, got {0}", Ops.StringRepr(Ops.GetDynamicType(index)));
                        }
                    }

                    DynamicType[] dta = new DynamicType[tindex.Count];
                    for (int i = 0; i < tindex.Count; i++) {
                        dta[i] = (DynamicType)tindex[i];
                    }

                    return Ops.GetDynamicTypeFromType(dt.MakeGenericType(dta));
                }
            }

            BuiltinFunction bf = o as BuiltinFunction;
            if (bf != null) {
                return bf[index];
            }

            if (GetDynamicType(o).TryInvokeBinaryOperator(DefaultContext.Default, Operators.GetItem, o, index, out ret)) {
                return ret;
            }

            throw Ops.AttributeError("{0} object has no attribute '__getitem__'",
                Ops.StringRepr(Ops.GetDynamicType(o)));
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
                ArrayOps.SetIndex(array, index, value);
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
            if (!GetDynamicType(o).TryInvokeTernaryOperator(DefaultContext.Default, Operators.SetItem, o, index, value, out ret)) {
                throw Ops.AttributeError("{0} object has no attribute '__setitem__'",
                    Ops.StringRepr(Ops.GetDynamicType(o)));
            }
        }

        public static void DelIndex(object o, object index) {
            IMutableSequence seq = o as IMutableSequence;
            if (seq != null) {
                if (index == null) {
                    throw Ops.TypeError("index must be integer or slice");
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
            if (!GetDynamicType(o).TryInvokeBinaryOperator(DefaultContext.Default, Operators.DeleteItem, o, index, out ret)) {
                throw Ops.AttributeError("{0} object has no attribute '__delitem__'",
                    Ops.StringRepr(Ops.GetDynamicType(o)));
            }
        }

        public static object GetNamespace(CodeContext context, Assembly asm, string nameSpace) {
            object res;
            if (!Ops.GetDynamicTypeFromType(typeof(Assembly)).TryGetBoundMember(context, asm, SymbolTable.StringToId(nameSpace), out res)) {
                throw new InvalidOperationException("bad assembly");
            }
            return res;
        }

        public static bool TryGetAttr(object o, SymbolId name, out object ret) {
            return TryGetAttr(DefaultContext.Default, o, name, out ret);
        }

        public static bool TryGetAttr(CodeContext context, object o, SymbolId name, out object ret) {
            ICustomMembers ids = o as ICustomMembers;
            if (ids != null) {
                if (ids.TryGetCustomMember(context, name, out ret)) {
                    return true;
                }

                //!!! should go into DynamicType ICustomAttributes implementation, but we don't have
                // the type cache to do this... (see GetAttr for more info)
                if (o.GetType() != typeof(DynamicType) && !o.GetType().IsSubclassOf(typeof(ExtensibleType))) 
                    return false;
            }
            
            if (GetDynamicType(o).TryGetMember(context, o, name, out ret)) {
                //!!! needs to go (should be bulit-in to DynamicType)
                return true;
            }
            return false;
        }

        public static bool TryGetBoundAttr(object o, SymbolId name, out object ret) {
            return TryGetBoundAttr(DefaultContext.Default, o, name, out ret);
        }

        public static bool TryGetBoundAttr(CodeContext context, object o, SymbolId name, out object ret) {
            ICustomMembers icm = o as ICustomMembers;
            if (icm != null) {
                if (icm.TryGetBoundCustomMember(context, name, out ret)) {
                    return true;
                }

                //!!! should go into DynamicType ICustomAttributes implementation, but we don't have
                // the type cache to do this... (see GetAttr for more info)
                if (o.GetType() != typeof(DynamicType) && !o.GetType().IsSubclassOf(typeof(ExtensibleType)))
                    return false;
            }

            if (GetDynamicType(o).TryGetBoundMember(context, o, name, out ret)) {
                //!!! needs to go (should be bulit-in to DynamicType)
                return true;
            }
            return false;
        }

        public static bool HasAttr(CodeContext context, object o, SymbolId name) {
            object dummy;
            try {
                return TryGetAttr(context, o, name, out dummy);
            } catch {
                return false;
            }
        }

        public static object GetAttr(CodeContext context, object o, SymbolId name) {
            ICustomMembers ifca = o as ICustomMembers;
            object ret;

            if (ifca != null) {
                if (ifca.TryGetCustomMember(context, name, out ret)) {
                    //!!! needs to go (should be bulit-in to DynamicType)
                    return ret;                    
                }
                //!!! this DynamicType check can go away when the typecache
                // lives in Microsoft.Scripting & DynamicType's CustomAttrs
                // implementation can look in type
                if (o.GetType() == typeof(DynamicType) || o.GetType().IsSubclassOf(typeof(ExtensibleType))) {
                    // we have an instance of a class that is built w/
                    // a meta-class.  We need to check the metaclasses
                    // properties as well, which ICustomAttrs didn't do.
                    // we'll fall through to GetAttr (we should probably
                    // do special overrides in NewTypeMaker instead)
                } else if (o is OldClass) {
                    throw Ops.AttributeError("type object '{0}' has no attribute '{1}'",
                        ((OldClass)o).Name, SymbolTable.IdToString(name));
                } else {
                    throw Ops.AttributeError("'{0}' object has no attribute '{1}'", GetDynamicType(o).Name, SymbolTable.IdToString(name));
                }
            }

            // fall through to normal case...
            return GetDynamicType(o).GetMember(context, o, name);
        }

        public static object GetBoundAttr(CodeContext context, object o, SymbolId name) {
            ICustomMembers icm = o as ICustomMembers;
            if (icm != null) {
                object value;
                if (icm.TryGetBoundCustomMember(context, name, out value)) {
                    return value;
                }
                //!!! this DynamicType check can go away when the typecache
                // lives in Microsoft.Scripting & DynamicType's CustomAttrs
                // implementation can look in type
                if (o.GetType() == typeof(DynamicType) || o.GetType().IsSubclassOf(typeof(ExtensibleType))) {
                    // we have an instance of a class that is built w/
                    // a meta-class.  We need to check the metaclasses
                    // properties as well, which ICustomAttrs didn't do.
                    // we'll fall through to GetAttr (we should probably
                    // do special overrides in NewTypeMaker instead)
                } else if (o is OldClass) {
                    throw Ops.AttributeError("type object '{0}' has no attribute '{1}'",
                        ((OldClass)o).Name, SymbolTable.IdToString(name));
                } else {
                    throw Ops.AttributeError("'{0}' object has no attribute '{1}'", DynamicTypeOps.GetName(GetDynamicType(o)), SymbolTable.IdToString(name));
                }
            }

            return GetDynamicType(o).GetBoundMember(context, o, name);
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

            if (!GetDynamicType(o).TrySetNonCustomMember(context, o, name, value))
                throw AttributeErrorForMissingOrReadonly(context, Ops.GetDynamicType(o), name);
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

            if (!GetDynamicType(o).TryDeleteNonCustomMember(context, o, name)) {
                throw AttributeErrorForMissingOrReadonly(context, Ops.GetDynamicType(o), name);
            }
        }

        public static object ObjectGetAttribute(CodeContext context, object o, SymbolId name) {
            ICustomMembers ifca = o as ICustomMembers;
            if (ifca != null) {
                object ret;
                if (ifca.TryGetBoundCustomMember(context, name, out ret)) return ret;
                //!!! this DynamicType check can go away when the typecache
                // lives in Microsoft.Scripting & DynamicType's CustomAttrs
                // implementation can look in type
                if (o.GetType() == typeof(DynamicType)) {
                    // we have an instance of a class that is built w/
                    // a meta-class.  We need to check the metaclasses
                    // properties as well, which ICustomAttrs didn't do.
                    // we'll fall through to GetBoundAttr (we should probably
                    // do special overrides in NewTypeMaker instead)
                } else if (o is OldClass) {
                    throw Ops.AttributeError("type object '{0}' has no attribute '{1}'",
                        ((OldClass)o).Name, SymbolTable.IdToString(name));
                } else {
                    throw Ops.AttributeError("'{0}' object has no attribute '{1}'", GetDynamicType(o).Name, SymbolTable.IdToString(name));
                }
            }

            object value;
            if (Ops.GetDynamicType(o).TryGetNonCustomMember(context, o, name, out value)) {
                return value;
            }            

            throw Ops.AttributeErrorForMissingAttribute(Ops.GetDynamicType(o).Name, name);
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

            if (!GetDynamicType(o).TrySetMember(context, o, name, value))
                throw AttributeErrorForMissingOrReadonly(context, Ops.GetDynamicType(o), name);
        }

        public static Exception AttributeErrorForMissingOrReadonly(CodeContext context, DynamicType dt, SymbolId name) {
            DynamicTypeSlot dts;
            if (dt.TryResolveSlot(context, name, out dts)) {
                throw Ops.AttributeErrorForReadonlyAttribute(DynamicTypeOps.GetName(dt), name);
            }

            throw Ops.AttributeErrorForMissingAttribute(DynamicTypeOps.GetName(dt), name);
        }

        public static Exception AttributeErrorForMissingAttribute(object o, SymbolId name) {
            DynamicType dt = o as DynamicType;
            if (dt != null)
                return Ops.AttributeErrorForMissingAttribute(dt.Name, name);

            return AttributeErrorForReadonlyAttribute(DynamicTypeOps.GetName(Ops.GetDynamicType(o)), name);
        }


        public static IList<object> GetAttrNames(CodeContext context, object o) {
            ICustomMembers ids = o as ICustomMembers;

            if (ids != null) {
                return ids.GetCustomMemberNames(context);
            }

            List res = new List();
            foreach(SymbolId x in GetDynamicType(o).GetMemberNames(context, o))
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

            IAttributesCollection iac = GetDynamicType(o).GetMemberDictionary(context, o);
            if (iac != null) {
                return iac.AsObjectKeyedDictionary();
            }
            throw Ops.AttributeErrorForMissingAttribute(Ops.GetDynamicType(o).Name, Symbols.Dict);
        }

        public static void CheckInitializedAttribute(object o, object self, string name) {
            if (o == Uninitialized.Instance) {
                throw Ops.AttributeError("'{0}' object has no attribute '{1}'",
                    Ops.GetDynamicType(self),
                    name);
            }
        }
        
        /// <summary>
        /// Handles the descriptor protocol - checks for our internal implementation,
        /// then calls the version that works on user-types
        /// </summary>
        public static object GetDescriptor(object o, object instance, object type) {
            PythonFunction f = o as PythonFunction;
            if (f != null) return f.GetAttribute(instance, type);

            return GetUserDescriptor(o, instance, type);
        }

        /// <summary>
        /// Handles the descriptor protocol for user-defined objects that may implement __get__
        /// </summary>
        public static object GetUserDescriptor(object o, object instance, object context) {
            if (!(o is ISuperDynamicObject)) return o;   // only new-style classes can implement __get__

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

            return o;
        }

        public static object Invoke(object target, SymbolId name, params object[] args) {
            return PythonCalls.Call(Ops.GetBoundAttr(DefaultContext.Default, target, name), args);
        }

        public static object InvokeWithContext(CodeContext context, object target, SymbolId name, params object[] args) {
            return Ops.CallWithContext(context, Ops.GetBoundAttr(context, target, name), args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryInvokeOperator(CodeContext context, Operators name, object target, out object ret) {
            return Ops.GetDynamicType(target).TryInvokeUnaryOperator(context, name, target, out ret);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryInvokeOperator(CodeContext context, Operators name, object target, object other, out object ret) {
            return Ops.GetDynamicType(target).TryInvokeBinaryOperator(context, name, target, other, out ret);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryInvokeOperator(CodeContext context, Operators name, object target, object value1, object value2, out object ret) {
            return Ops.GetDynamicType(target).TryInvokeTernaryOperator(context, name, target, value1, value2, out ret);
        }

        public static void Write(object f, string text) {
            SystemState state = SystemState.Instance;

            if (f == null) f = state.stdout;
            if (f == null || f == Uninitialized.Instance) throw Ops.RuntimeError("lost sys.std_out");
            Ops.Invoke(f, Symbols.ConsoleWrite, text);
            if (HasAttr(DefaultContext.Default, f, SymbolTable.StringToId("flush"))) {
                Ops.Invoke(f, SymbolTable.StringToId("flush"));
            }
        }

        private static object ReadLine(object f) {
            if (f == null || f == Uninitialized.Instance) throw Ops.RuntimeError("lost sys.std_in");
            return Ops.Invoke(f, Symbols.ConsoleReadLine);
        }

        public static void WriteSoftspace(object f) {
            if (CheckSoftspace(f)) {
                SetSoftspace(f, False);
                Write(f, " ");
            }
        }

        // TODO: Move to Python Ops
        public static void SetSoftspace(object f, object value) {
            Ops.SetAttr(DefaultContext.Default, f, Symbols.Softspace, value);
        }

        // TODO: Move to Python Ops
        public static bool CheckSoftspace(object f) {
            object result;
            if (Ops.TryGetBoundAttr(f, Symbols.Softspace, out result)) return Ops.IsTrue(result);
            return false;
        }

        // Must stay here for now because libs depend on it.
        // TODO: Remove
        public static void Print(object o) {
            SystemState state = SystemState.Instance;

            PrintWithDest(state.stdout, o);
        }

        public static void PrintNoNewline(object o) {
            SystemState state = SystemState.Instance;

            PrintWithDestNoNewline(state.stdout, o);
        }

        private static void PrintWithDest(object dest, object o) {
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

        public static Delegate CreateDynamicDelegate(DynamicMethod meth, Type delegateType, object target) {
            // Always close delegate around its own instance of the frame
            return meth.CreateDelegate(delegateType, target);
        }

        public static double CheckMath(double v) {
            if (double.IsInfinity(v)) {
                throw Ops.OverflowError("math range error");
            } else if (double.IsNaN(v)) {
                throw Ops.ValueError("math domain error");
            } else {
                return v;
            }
        }

        public static object IsMappingType(CodeContext context, object o) {
            if (o is IMapping || o is PythonDictionary || o is IDictionary<object, object> || o is IAttributesCollection) {
                return Ops.True;
            }
            object getitem;
            if (Ops.TryGetBoundAttr(context, o, Symbols.GetItem, out getitem)) {
                if (!context.LanguageContext.ShowCls) {
                    // in standard Python methods aren't mapping types, therefore
                    // if the user hasn't broken out of that box yet don't treat 
                    // them as mapping types.
                    if (o is BuiltinFunction) return Ops.False;
                }
                return Ops.True;
            }
            return Ops.False;
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
                    throw Ops.ValueError("step cannot be zero");
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
                    throw Ops.IndexError("index out of range: {0}", v - len);
                }
            } else if (v >= len) {
                throw Ops.IndexError("index out of range: {0}", v);
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
            
            Ops.SetAttr(DefaultContext.Default,
                ExceptionConverter.ToPython(res),
                Symbols.Arguments,
                Tuple.MakeTuple(key));

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

        public static Exception SyntaxError(string msg, string filename, int line, int column, string lineText, int errorCode, Severity severity) {
            return new SyntaxErrorException(msg, filename, line, column, lineText, errorCode, severity);
        }

        public static Exception IndentationError(string msg, string filename, int line, int column, string lineText, int errorCode, Severity severity) {
            return new PythonIndentationError(msg, filename, line, column, lineText, errorCode, severity);
        }

        public static Exception TabError(string msg, string filename, int line, int column, string lineText, int errorCode, Severity severity) {
            return new PythonTabError(msg, filename, line, column, lineText, errorCode, severity);
        }


        #endregion


        public static Exception StopIteration() {
            return StopIteration("");
        }

        public static Exception InvalidType(object o, RuntimeTypeHandle handle) {
            System.Diagnostics.Debug.Assert(ScriptDomainManager.Options.GenerateSafeCasts);
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
            return new UnboundNameException(string.Format("name {0} is not defined", SymbolTable.IdToString(name)));
        }


        // If an unbound method is called without a "self" argument, or a "self" argument of a bad type
        public static Exception TypeErrorForUnboundMethodCall(string methodName, Type methodType, object instance) {
            return TypeErrorForUnboundMethodCall(methodName, GetDynamicTypeFromType(methodType), instance);
        }

        public static Exception TypeErrorForUnboundMethodCall(string methodName, DynamicType methodType, object instance) {
            string message = string.Format("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)",
                                           methodName, methodType.Name, GetDynamicType(instance).Name);
            return TypeError(message);
        }

        // If a method is called with an incorrect number of arguments
        // You should use TypeErrorForUnboundMethodCall() for unbound methods called with 0 arguments
        public static Exception TypeErrorForArgumentCountMismatch(string methodName, int expectedArgCount, int actualArgCount) {
            return TypeError("{0}() takes exactly {1} argument{2} ({3} given)",
                             methodName, expectedArgCount, expectedArgCount == 1 ? "" : "s", actualArgCount);
        }

        public static Exception TypeErrorForTypeMismatch(string expectedTypeName, object instance) {
            return TypeError("expected {0}, got {1}", expectedTypeName, Ops.GetPythonTypeName(instance));
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
            return TypeError(template, Ops.GetPythonTypeName(instance));
        }

        public static Exception TypeErrorForBinaryOp(string opSymbol, object x, object y) {
            throw Ops.TypeError("unsupported operand type(s) for {0}: '{1}' and '{2}'",
                                opSymbol, GetPythonTypeName(x), GetPythonTypeName(y));
        }

        public static Exception TypeErrorForUnaryOp(string opSymbol, object x) {
            throw Ops.TypeError("unsupported operand type for {0}: '{1}'",
                                opSymbol, GetPythonTypeName(x));
        }
        public static Exception TypeErrorForDefaultArgument(string message) {
            return Ops.TypeError(message);
        }

        public static Exception AttributeErrorForReadonlyAttribute(string typeName, SymbolId attributeName) {
            // CPython uses AttributeError for all attributes except "__class__"
            if (attributeName == Symbols.Class)
                return Ops.TypeError("can't delete __class__ attribute");

            return Ops.AttributeError("attribute '{0}' of '{1}' object is read-only", SymbolTable.IdToString(attributeName), typeName);
        }

        public static Exception AttributeErrorForBuiltinAttributeDeletion(string typeName, SymbolId attributeName) {
            return Ops.AttributeError("cannot delete attribute '{0}' of builtin type '{1}'", SymbolTable.IdToString(attributeName), typeName);
        }

        public static Exception MissingInvokeMethodException(object o, string name) {
            if (o is OldClass) {
                throw Ops.AttributeError("type object '{0}' has no attribute '{1}'",
                    ((OldClass)o).Name, name);
            } else {
                throw Ops.AttributeError("'{0}' object has no attribute '{1}'", GetPythonTypeName(o), name);
            }
        }

        public static Exception AttributeErrorForMissingAttribute(string typeName, SymbolId attributeName) {
            return Ops.AttributeError("'{0}' object has no attribute '{1}'", typeName, SymbolTable.IdToString(attributeName));
        }

        public static IAttributesCollection GetEnvironmentDictionary(object environment) {
            IAttributesCollection dict = null;
            NewTuple tuple = environment as NewTuple;

            if (tuple != null) dict = tuple.GetValue(0) as IAttributesCollection;

            return dict;
        }

        /// <summary>
        /// Registers a set of extension methods from the provided assemly.
        /// 
        /// TODO: Move to DynamicHelpers
        /// </summary>
        public static void RegisterAssembly(Assembly assembly) {
            object[] attrs = assembly.GetCustomAttributes(typeof(ExtensionTypeAttribute), false);
            foreach (ExtensionTypeAttribute et in attrs) {
                ExtendOneType(et, Ops.GetDynamicTypeFromType(et.Extends));
            }
        }

        internal static void ExtendOneType(ExtensionTypeAttribute et, DynamicType dt) {
            ExtensionTypeAttribute.RegisterType(et.Extends, et.Type, dt);

            DynamicTypeExtender.ExtendType(dt, et.Type, et.Transformer);

            if (et.EnableDerivation) {
                DynamicTypeBuilder.GetBuilder(Ops.GetDynamicTypeFromType(et.Extends)).SetIsExtensible();
            } else if (et.DerivationType != null) {
                DynamicTypeBuilder.GetBuilder(Ops.GetDynamicTypeFromType(et.Extends)).SetExtensionType(et.DerivationType);
            }
        }

        public static TopReflectedPackage TopPackage {
            get {
                if (_topPackage == null)
                    Interlocked.CompareExchange<TopReflectedPackage>(ref _topPackage, new TopReflectedPackage(), null);

                return _topPackage;
            }
        }


    }
}
