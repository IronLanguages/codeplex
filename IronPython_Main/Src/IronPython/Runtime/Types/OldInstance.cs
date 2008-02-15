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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Types {
    using Ast = Microsoft.Scripting.Ast.Ast;

    [PythonType("instance")]
    [Serializable]
    public sealed partial class OldInstance :
        ICodeFormattable,
        IValueEquality,
#if !SILVERLIGHT // ICustomTypeDescriptor
        ICustomTypeDescriptor,
#endif
        ISerializable,
        IWeakReferenceable,
        ICustomMembers,
        IDynamicObject
    {

        private IAttributesCollection __dict__;
        internal OldClass __class__;
        private WeakRefTracker _weakRef;       // initialized if user defines finalizer on class or instance

        private IAttributesCollection MakeDictionary(OldClass oldClass) {
            //if (oldClass.OptimizedInstanceNames.Length == 0) {
            //    return new CustomOldClassDictionar();
            //}
            return new CustomOldClassDictionary(oldClass.OptimizedInstanceNames, oldClass.OptimizedInstanceNamesVersion);
        }


        public OldInstance(OldClass _class) {
            __class__ = _class;
            __dict__ = MakeDictionary(_class);
            if (__class__.HasFinalizer) {
                // class defines finalizer, we get it automatically.
                AddFinalizer();
            }
        }

#if !SILVERLIGHT // SerializationInfo
        private OldInstance(SerializationInfo info, StreamingContext context) {
            __class__ = (OldClass)info.GetValue("__class__", typeof(OldClass));
            __dict__ = MakeDictionary(__class__);

            List<object> keys = (List<object>)info.GetValue("keys", typeof(List<object>));
            List<object> values = (List<object>)info.GetValue("values", typeof(List<object>));
            for (int i = 0; i < keys.Count; i++) {
                __dict__.AddObjectKey(keys[i], values[i]);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            Contract.RequiresNotNull(info, "info");

            info.AddValue("__class__", __class__);
            List<object> keys = new List<object>();
            List<object> values = new List<object>();
            foreach (object o in __dict__.Keys) {
                keys.Add(o);
                object value;

                bool res = __dict__.TryGetObjectValue(o, out value);

                Debug.Assert(res);

                values.Add(value);
            }

            info.AddValue("keys", keys);
            info.AddValue("values", values);
        }
#endif

        /// <summary>
        /// Returns the dictionary used to store state for this object
        /// </summary>
        public IAttributesCollection Dictionary {
            get { return __dict__; }
        }

        public CustomOldClassDictionary GetOptimizedDictionary(int keyVersion) {
            CustomOldClassDictionary dict = __dict__ as CustomOldClassDictionary;
            if (dict == null || __class__.HasSetAttr || dict.KeyVersion != keyVersion) {
                return null;
            }
            return dict;
        }


        public static bool operator true(OldInstance self) {
            return (bool)self.IsNonZero(DefaultContext.Default);
        }

        public static bool operator false(OldInstance self) {
            return !(bool)self.IsNonZero(DefaultContext.Default);
        }


        public IEnumerator GetEnumerator() {
            return PythonOps.GetEnumeratorForIteration(this);
        }


        #region IDynamicObject Members

        LanguageContext IDynamicObject.LanguageContext {
            get { return DefaultContext.Default.LanguageContext; }
        }

        StandardRule<T> IDynamicObject.GetRule<T>(DynamicAction action, CodeContext context, object[] args)  {
            switch (action.Kind) {
                case DynamicActionKind.GetMember:
                case DynamicActionKind.SetMember:
                case DynamicActionKind.DeleteMember:
                    return MakeMemberRule<T>((MemberAction)action, context, args);
                case DynamicActionKind.DoOperation:
                    return MakeOperationRule<T>((DoOperationAction)action, context, args);
                case DynamicActionKind.Call:
                    return MakeCallRule<T>((CallAction)action, context, args);
                case DynamicActionKind.ConvertTo:
                    return MakeConvertToRule<T>((ConvertToAction)action, context, args);
                default:
                    return null;
            }
        }

        private StandardRule<T> MakeConvertToRule<T>(ConvertToAction convertToAction, CodeContext context, object[] args) {
            Type toType = convertToAction.ToType;
            if (toType == typeof(int)) {
                return MakeConvertRuleForCall<T>(context, convertToAction, Symbols.ConvertToInt, "ConvertToInt");
            } else if (toType == typeof(BigInteger)) {
                return MakeConvertRuleForCall<T>(context, convertToAction, Symbols.ConvertToLong, "ConvertToLong");
            } else if (toType == typeof(double)) {
                return MakeConvertRuleForCall<T>(context, convertToAction, Symbols.ConvertToFloat, "ConvertToFloat");
            } else if (toType == typeof(Complex64)) {
                return MakeConvertComplexRuleForCall<T>(context, convertToAction);
            } else if (toType == typeof(bool)) {
                return MakeBoolConvertRuleForCall<T>(context, convertToAction);
            } else if (toType == typeof(IEnumerable)) {
                return MakeConvertToIEnumerable<T>(context, convertToAction);
            } else if (toType == typeof(IEnumerator)) {
                return MakeConvertToIEnumerator<T>(context, convertToAction);
            } else if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                return MakeConvertToIEnumerator<T>(context, convertToAction);
            }

            return null;
        }

        private static StandardRule<T> MakeConvertToIEnumerator<T>(CodeContext context, ConvertToAction convertToAction) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));
            Variable tmp = rule.GetTemporary(typeof(object), "tmp");
            // build up:
            // if(hasattr(this, '__iter__')) { 
            //    return PythonEnumerator.Create(this)
            // } else { 
            //    if(hasattr(this, '__getitem__')) return ItemEnumerator.Create(this)
            // }
            // return or throw errorValue
            Expression failed = PythonBinderHelper.GetConversionFailedReturnValue<T>(context, convertToAction, rule);

            Expression call = Ast.Call(
                typeof(PythonOps).GetMethod("CreateItemEnumerable"),
                rule.Parameters[0]
            );

            call = WrapGenericEnumerator<T>(convertToAction, rule, call);

            Expression body2 = MakeIterRule<T>(context, rule, Symbols.GetItem, tmp, failed, call);
            call = Ast.Call(
                typeof(PythonOps).GetMethod("CreatePythonEnumerable"),
                rule.Parameters[0]
            );

            call = WrapGenericEnumerator<T>(convertToAction, rule, call);

            Expression body1 = MakeIterRule<T>(context, rule, Symbols.Iterator, tmp, body2, call);
            rule.Target = body1;
            return rule;
        }

        private static Expression WrapGenericEnumerator<T>(ConvertToAction convertToAction, StandardRule<T> rule, Expression call) {
            if (convertToAction.ToType != typeof(IEnumerator)) {
                // generic enumerator
                Debug.Assert(convertToAction.ToType.IsGenericType && 
                    convertToAction.ToType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                call = Ast.New(
                    typeof(IEnumerableOfTWrapper<>).MakeGenericType(convertToAction.ToType.GetGenericArguments()[0]).GetConstructor(new Type[] { typeof(IEnumerable) }),
                    Ast.Action.ConvertTo(typeof(IEnumerable), rule.Parameters[0])
                );
            }
            return call;
        }

        private static StandardRule<T> MakeConvertToIEnumerable<T>(CodeContext context, ConvertToAction convertToAction) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));
            Variable tmp = rule.GetTemporary(typeof(object), "tmp");
            // build up:
            // if(hasattr(this, '__iter__')) { 
            //    return PythonEnumerable.Create(this)
            // } else { 
            //    if(hasattr(this, '__getitem__')) return ItemEnumerable.Create(this)
            // }
            // return or throw errorValue
            Expression failed = PythonBinderHelper.GetConversionFailedReturnValue<T>(context, convertToAction, rule);

            Expression call = Ast.Call(
                typeof(PythonOps).GetMethod("CreateItemEnumerable"),
                rule.Parameters[0]
            );

            Expression body2 = MakeIterRule<T>(context, rule, Symbols.GetItem, tmp, failed, call);
            call = Ast.Call(
                typeof(PythonOps).GetMethod("CreatePythonEnumerable"),
                rule.Parameters[0]
            );

            rule.Target = MakeIterRule<T>(context, rule, Symbols.Iterator, tmp, body2, call);
            return rule;
        }

        private static Expression MakeIterRule<T>(CodeContext context, StandardRule<T> res, SymbolId symbolId, Variable tmp, Expression @else, Expression call) {
            return Ast.IfThenElse(
                Ast.Call(
                    Ast.Convert(res.Parameters[0], typeof(OldInstance)),
                    typeof(OldInstance).GetMethod("TryGetBoundCustomMember"),
                    Ast.CodeContext(),
                    Ast.Constant(symbolId),
                    Ast.Read(tmp)
                ),
                res.MakeReturn(context.LanguageContext.Binder, call),
                @else
            );
        }

        private static StandardRule<T> MakeConvertComplexRuleForCall<T>(CodeContext context, ConvertToAction convertToAction) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));

            // we could get better throughput w/ a more specific rule against our current custom old class but
            // this favors less code generation.
            Variable tmp = rule.GetTemporary(typeof(object), "callFunc"); // , Symbols.ConvertToComplex, "ConvertToComplex"

            Expression failed = PythonBinderHelper.GetConversionFailedReturnValue<T>(context, convertToAction, rule);
            Expression tryFloat = MakeConvertCallBody<T>(context, convertToAction, Symbols.ConvertToFloat, "ConvertToFloat", rule, tmp, failed);
            rule.Target = MakeConvertCallBody<T>(context, convertToAction, Symbols.ConvertToComplex, "ConvertToComplex", rule, tmp, tryFloat);
            

            return rule;
        }

        private static StandardRule<T> MakeConvertRuleForCall<T>(CodeContext context, ConvertToAction convertToAction, SymbolId symbolId, string returner) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));

            // we could get better throughput w/ a more specific rule against our current custom old class but
            // this favors less code generation.
            Variable tmp = rule.GetTemporary(typeof(object), "callFunc");

            Expression failed =  PythonBinderHelper.GetConversionFailedReturnValue<T>(context, convertToAction, rule);
            rule.Target = MakeConvertCallBody<T>(context, convertToAction, symbolId, returner, rule, tmp, failed);
            
            return rule;
        }

        private static Expression MakeConvertCallBody<T>(CodeContext context, ConvertToAction convertToAction, SymbolId symbolId, string returner, StandardRule<T> rule, Variable tmp, Expression @else) {
            return Ast.IfThenElse(
                Ast.Call(
                    Ast.Convert(rule.Parameters[0], typeof(OldInstance)),
                    typeof(OldInstance).GetMethod("TryGetBoundCustomMember"),
                    Ast.CodeContext(),
                    Ast.Constant(symbolId),
                    Ast.Read(tmp)
                ),
                rule.MakeReturn(context.LanguageContext.Binder,
                    Ast.Call(
                        PythonOps.GetConversionHelper(returner, convertToAction.ResultKind),
                        Ast.Action.Call(
                            typeof(object),
                            Ast.Read(tmp)
                        )
                    )
                ),
                @else
            );
        }

        private static StandardRule<T> MakeBoolConvertRuleForCall<T>(CodeContext context, ConvertToAction convertToAction) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));

            // we could get better throughput w/ a more specific rule against our current custom old class but
            // this favors less code generation.
            Variable tmp = rule.GetTemporary(typeof(object), "callFunc");

            // Python anything can be converted to a bool (by default if it's not null it's true).  If we don't
            // find the conversion methods, and the request comes from Python, the result is true.
            Expression error;
            if (context.LanguageContext.Binder is PythonBinder) {
                error = rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Constant(true)
                );
            } else {
                error = PythonBinderHelper.GetConversionFailedReturnValue<T>(context, convertToAction, rule);
            }

            rule.Target =
                Ast.IfThenElse(
                    Ast.Call(
                        Ast.Convert(rule.Parameters[0], typeof(OldInstance)),
                        typeof(OldInstance).GetMethod("TryGetBoundCustomMember"),
                        Ast.CodeContext(),
                        Ast.Constant(Symbols.NonZero),
                        Ast.Read(tmp)
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder,
                        Ast.Call(
                            PythonOps.GetConversionHelper("ConvertToNonZero", convertToAction.ResultKind),
                            Ast.Action.Call(
                                typeof(object),
                                Ast.Read(tmp)
                            )
                        )
                    ),
                    Ast.IfThenElse(
                        Ast.Call(
                            Ast.Convert(rule.Parameters[0], typeof(OldInstance)),
                            typeof(OldInstance).GetMethod("TryGetBoundCustomMember"),
                            Ast.CodeContext(),
                            Ast.Constant(Symbols.Length),
                            Ast.Read(tmp)
                        ),
                        rule.MakeReturn(context.LanguageContext.Binder,
                            PythonBinderHelper.GetConvertByLengthBody(tmp)
                        ),
                        error
                    )
                );

            return rule;
        }

        private StandardRule<T> MakeCallRule<T>(CallAction callAction, CodeContext context, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));
            
            // we could get better throughput w/ a more specific rule against our current custom old class but
            // this favors less code generation.
            Variable tmp = rule.GetTemporary(typeof(object), "callFunc");
            Expression[] callParams = ArrayUtils.MakeArray(rule.Parameters);
            callParams[0] = Ast.Read(tmp);
            rule.Target =
                Ast.IfThenElse(
                    Ast.Call(
                        Ast.Convert(rule.Parameters[0], typeof(OldInstance)),
                        typeof(OldInstance).GetMethod("TryGetBoundCustomMember"),
                        Ast.CodeContext(),
                        Ast.Constant(Symbols.Call),
                        Ast.Read(tmp)
                    ),
                    rule.MakeReturn(context.LanguageContext.Binder,
                        Ast.Action.Call(
                            callAction,
                            typeof(object),
                            callParams
                        )
                    ),
                    rule.MakeError(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("UncallableError"),
                            Ast.ConvertHelper(rule.Parameters[0], typeof(object))
                        )
                    )
                );

            return rule;
        }

        private StandardRule<T> MakeMemberRule<T>(MemberAction action, CodeContext context, object[] args) {
            CustomOldClassDictionary dict = this.Dictionary as CustomOldClassDictionary;
            if (dict == null || __class__.HasSetAttr) {
                return MakeDynamicOldInstanceRule<T>(action, context);
            }

            int key = dict.FindKey(action.Name);
            if (key == -1) {
                return MakeDynamicOldInstanceRule<T>(action, context);
            }

            StandardRule<T> rule = new StandardRule<T>();

            Variable tmp = rule.GetTemporary(typeof(CustomOldClassDictionary), "dict");
            Expression tryGetValue = Ast.Call(
                Ast.Convert(rule.Parameters[0], typeof(OldInstance)),
                typeof(OldInstance).GetMethod("GetOptimizedDictionary"),
                Ast.Constant(dict.KeyVersion)
            );
            tryGetValue = Ast.Assign(tmp, tryGetValue);

            Expression test = Ast.AndAlso(
                Ast.NotEqual(
                    rule.Parameters[0],
                    Ast.Null()),
                Ast.Equal(
                    Ast.Call(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object)),
                        typeof(object).GetMethod("GetType")
                    ),
                    Ast.Constant(typeof(OldInstance))
                )
            );
            test = Ast.AndAlso(test,
                Ast.NotEqual(
                    tryGetValue, Ast.Null()));

            rule.Test = test;
            Expression target;

            switch (action.Kind) {
                case DynamicActionKind.GetMember:
                    target = Ast.Call(
                        Ast.ReadDefined(tmp),
                        typeof(CustomOldClassDictionary).GetMethod("GetValueHelper"),
                        Ast.Constant(key),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(object))
                    );
                    break;
                case DynamicActionKind.SetMember:
                    target = Ast.Call(
                        Ast.ReadDefined(tmp),
                        typeof(CustomOldClassDictionary).GetMethod("SetExtraValue"),
                        Ast.Constant(key),
                        Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                    );
                    break;
                case DynamicActionKind.DeleteMember:
                    target = Ast.Call(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(OldInstance)),
                        typeof(OldInstance).GetMethod("DeleteCustomMember"),
                        Ast.CodeContext(),
                        Ast.Constant(action.Name)
                    );
                    break;
                default:
                    throw new InvalidOperationException();
            }

            rule.Target = rule.MakeReturn(context.LanguageContext.Binder, target);
            return rule;
        }

        private StandardRule<T> MakeDynamicOldInstanceRule<T>(MemberAction action, CodeContext context) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));
            Expression instance = Ast.Convert(
                    rule.Parameters[0], typeof(OldInstance));

            Expression target;
             switch (action.Kind) {
                case DynamicActionKind.GetMember:
                    if (((GetMemberAction)action).IsNoThrow) {
                        Variable tmp = rule.GetTemporary(typeof(object), "tmp");

                        target = Ast.Condition(
                                    Ast.Call(
                                        instance,
                                        typeof(OldInstance).GetMethod("TryGetBoundCustomMember"),
                                        Ast.CodeContext(),
                                        Ast.Constant(action.Name),
                                        Ast.Read(tmp)
                                    ),
                                    Ast.Read(tmp),
                                    Ast.Convert(
                                        Ast.ReadField(null, typeof(OperationFailed).GetField("Value")),
                                        typeof(object)
                                    )
                                );
                    } else {
                        target = Ast.Call(
                            instance,
                            typeof(OldInstance).GetMethod("GetBoundMember"),
                            Ast.CodeContext(),
                            Ast.Constant(action.Name)
                        );
                    }
                    break;
                case DynamicActionKind.SetMember:
                    target = Ast.Call(
                        instance,
                        typeof(OldInstance).GetMethod("SetCustomMember"),
                        Ast.CodeContext(),
                        Ast.Constant(action.Name),
                        Ast.ConvertHelper(rule.Parameters[1], typeof(object))
                    );
                    break;
                 case DynamicActionKind.DeleteMember:
                    target = Ast.Call(
                        instance,
                        typeof(OldInstance).GetMethod("DeleteCustomMember"),
                        Ast.CodeContext(),
                        Ast.Constant(action.Name)
                    );
                    break;
                default:
                    throw new InvalidOperationException();
            }

            rule.Target = rule.MakeReturn(context.LanguageContext.Binder, target);
            return rule;
        }

        private static StandardRule<T> MakeOperationRule<T>(DoOperationAction action, CodeContext context, object[] args) {
            switch (action.Operation) {
                case Operators.GetItem:
                case Operators.SetItem:
                case Operators.DeleteItem:
                case Operators.GetSlice:
                case Operators.SetSlice:
                case Operators.DeleteSlice:
                    // ask the default python DoOperationBinderHelper to produce the rule for the purpose
                    // of interop - it knows how to handle all things indexing.
                    return new PythonDoOperationBinderHelper<T>(context, action).MakeRule(args);
                case Operators.IsCallable:
                    return PythonBinderHelper.MakeIsCallableRule<T>(context, args[0]);
                default:
                    return null;
            }
        }

        #endregion

        #region Object overrides

        public override string ToString() {
            object ret = InvokeOne(this, Symbols.String);

            if (ret != PythonOps.NotImplemented) {
                string strRet;
                if (Converter.TryConvertToString(ret, out strRet) && strRet != null) {
                    return strRet;
                }
                throw PythonOps.TypeError("__str__ returned non-string type ({0})", PythonTypeOps.GetName(ret));
            }

            return ToCodeString(DefaultContext.Default);
        }

        #endregion

        #region ICodeFormattable Members

        [SpecialName, PythonName("__repr__")]
        public string ToCodeString(CodeContext context) {
            object ret = InvokeOne(this, Symbols.Repr);
            if(ret != PythonOps.NotImplemented) {
                string strRet;
                if (Converter.TryConvertToString(ret, out strRet) && strRet != null) {
                    return strRet;
                }
                throw PythonOps.TypeError("__repr__ returned non-string type ({0})", PythonTypeOps.GetName(ret));
            }

            return string.Format("<{0} instance at {1}>", __class__.FullName, PythonOps.HexId(this));
        }

        #endregion

        [SpecialName, PythonName("__divmod__")]
        [return: MaybeNotImplemented]
        public object DivMod(CodeContext context, object divmod) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.DivMod, out value)) {
                return PythonCalls.Call(value, divmod);
            }


            return PythonOps.NotImplemented;
        }

        [SpecialName, PythonName("__rdivmod__")]
        [return: MaybeNotImplemented]
        public static object ReverseDivMod(CodeContext context, object divmod, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(context, Symbols.ReverseDivMod, out value)) {
                return PythonCalls.Call(value, divmod);
            }

            return PythonOps.NotImplemented;
        }

        [PythonName("__coerce__")]
        public object Coerce(CodeContext context, object other) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Coerce, out value)) {
                return PythonCalls.Call(value, other);
            }

            return PythonOps.NotImplemented;
        }

        [SpecialName, PythonName("__len__")]
        public object GetLength(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Length, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(__class__.Name, Symbols.Length);
        }

        [SpecialName, PythonName("__pos__")]
        public object Positive(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Positive, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(__class__.Name, Symbols.Positive);
        }

        [SpecialName, PythonName("__getitem__")]
        public object GetItem(CodeContext context, object item) {
            return PythonOps.InvokeWithContext(context, this, Symbols.GetItem, item);
        }

        [SpecialName, PythonName("__setitem__")]
        public void SetItem(CodeContext context, object item, object value) {
            PythonOps.InvokeWithContext(context, this, Symbols.SetItem, item, value);
        }

        [SpecialName, PythonName("__delitem__")]
        public object DeleteItem(CodeContext context, object item) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.DelItem, out value)) {
                return PythonCalls.Call(value, item);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(__class__.Name, Symbols.DelItem);
        }

        [PythonName("__getslice__")]
        public object GetSlice(CodeContext context, int i, int j) {
            object callable;
            if (TryRawGetAttr(context, Symbols.GetSlice, out callable)) {
                return PythonCalls.Call(callable, i, j);
            } else if (TryRawGetAttr(context, Symbols.GetItem, out callable)) {
                return PythonCalls.Call(callable, new Slice(i, j));
            }

            throw PythonOps.TypeError("instance {0} does not have __getslice__ or __getitem__", __class__.Name);
        }
        
        [PythonName("__setslice__")]
        public void SetSlice(CodeContext context, int i, int j, object value) {
            object callable;
            if (TryRawGetAttr(context, Symbols.SetSlice, out callable)) {
                PythonCalls.Call(callable, i, j, value);
                return;
            } else if (TryRawGetAttr(context, Symbols.SetItem, out callable)) {
                PythonCalls.Call(callable, new Slice(i, j), value);
                return;
            }

            throw PythonOps.TypeError("instance {0} does not have __setslice__ or __setitem__", __class__.Name);
        }

        [PythonName("__delslice__")]
        public object DeleteSlice(CodeContext context, int i, int j) {
            object callable;
            if (TryRawGetAttr(context, Symbols.DeleteSlice, out callable)) {
                return PythonCalls.Call(callable, i, j);
            } else if (TryRawGetAttr(context, Symbols.DelItem, out callable)) {
                return PythonCalls.Call(callable, new Slice(i, j));
            }

            throw PythonOps.TypeError("instance {0} does not have __delslice__ or __delitem__", __class__.Name);
        }

        [PythonName("__index__")]
        public object GetIndexValue(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToInt, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.TypeError("object cannot be converted to an index");
        }

        [SpecialName, PythonName("__neg__")]
        public object Negate(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.OperatorNegate, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(__class__.Name, Symbols.OperatorNegate);
        }


        [SpecialName, PythonName("__abs__")]
        public object Absolute(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.AbsoluteValue, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(__class__.Name, Symbols.AbsoluteValue);
        }

        [SpecialName, PythonName("__invert__")]
        public object Invert(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.OperatorOnesComplement, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(__class__.Name, Symbols.OperatorOnesComplement);
        }

        [SpecialName, PythonName("__contains__")]
        public object Contains(CodeContext context, object index) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Contains, out value)) {
                return PythonCalls.Call(value, index);
            }

            IEnumerator ie = PythonOps.GetEnumerator(this);
            while (ie.MoveNext()) {
                if (PythonOps.EqualRetBool(ie.Current, index)) return RuntimeHelpers.True;
            }

            return RuntimeHelpers.False;
        }

        [SpecialName, PythonName("__pow__")]
        public object Power(CodeContext context, object exp, object mod) {
            object value;
            if (TryGetBoundCustomMember(context, Symbols.OperatorPower, out value)) {
                return PythonCalls.Call(value, exp, mod);
            }

            return PythonOps.NotImplemented;
        }

        [SpecialName]
        public object Call(CodeContext context) {
            return Call(context, ArrayUtils.EmptyObjects);
        }

        [SpecialName]
        public object Call(CodeContext context, object args) {
            PythonOps.FunctionPushFrame();
            try {
                object value;

                if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                    KwCallInfo kwInfo;

                    if (args is object[])
                        return PythonOps.CallWithContext(context, value, (object[])args);
                    else if ((kwInfo = args as KwCallInfo) != null)
                        return PythonOps.CallWithKeywordArgs(context, value, kwInfo.Arguments, kwInfo.Names);

                    return PythonOps.CallWithContext(context, value, args);
                }
            } finally {
                PythonOps.FunctionPopFrame();
            }

            throw PythonOps.AttributeError("{0} instance has no __call__ method", __class__.Name);
        }

        [SpecialName]
        public object Call(CodeContext context, params object[] args) {
            PythonOps.FunctionPushFrame();
            try {
                object value;

                if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                    return PythonOps.CallWithContext(context, value, args);
                }
            } finally {
                PythonOps.FunctionPopFrame();
            }

            throw PythonOps.AttributeError("{0} instance has no __call__ method", __class__.Name);
        }

        [SpecialName]
        public object Call(CodeContext context, [ParamDictionary]IAttributesCollection dict, params object[] args) {
            PythonOps.FunctionPushFrame();
            try {
                object value;

                if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                    return PythonOps.CallWithArgsTupleAndKeywordDictAndContext(context, value, args, ArrayUtils.EmptyStrings, null, dict);
                }
            } finally {
                PythonOps.FunctionPopFrame();
            }

            throw PythonOps.AttributeError("{0} instance has no __call__ method", __class__.Name);
        }

        [SpecialName, PythonName("__nonzero__")]
        public object IsNonZero(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.NonZero, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            if (TryGetBoundCustomMember(context, Symbols.Length, out value)) {
                value = PythonOps.CallWithContext(context, value);
                // Convert resulting object to the desired type
                if (value is Int32 || value is BigInteger) {
                    return RuntimeHelpers.BooleanToObject(Converter.ConvertToBoolean(value));
                }
                throw PythonOps.TypeError("an integer is required, got {0}", PythonTypeOps.GetName(value));
            }

            return RuntimeHelpers.True;
        }

        [SpecialName, PythonName("__hex__")]
        public object ConvertToHex(CodeContext context) {
            object value;
            if (TryGetBoundCustomMember(context, Symbols.ConvertToHex, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(__class__.Name, Symbols.ConvertToHex);
        }

        [SpecialName, PythonName("__oct__")]
        public object ConvertToOctal(CodeContext context) {
            object value;
            if (TryGetBoundCustomMember(context, Symbols.ConvertToOctal, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(__class__.Name, Symbols.ConvertToOctal);
        }

        [SpecialName, PythonName("__int__")]
        public object ConvertToInt(CodeContext context) {
            object value;

            if (PythonOps.TryGetBoundAttr(context, this, Symbols.ConvertToInt, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return PythonOps.NotImplemented;
        }

        [SpecialName, PythonName("__long__")]
        public object ConvertToLong(CodeContext context) {
            object value;

            if (PythonOps.TryGetBoundAttr(context, Symbols.ConvertToLong, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return PythonOps.NotImplemented;
        }

        [SpecialName, PythonName("__float__")]
        public object ConvertToFloat(CodeContext context) {
            object value;

            if (PythonOps.TryGetBoundAttr(context, this, Symbols.ConvertToFloat, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return PythonOps.NotImplemented;
        }

        [SpecialName, PythonName("__complex__")]
        public object ConvertToComplex(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToComplex, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return PythonOps.NotImplemented;
        }

        public object GetBoundMember(CodeContext context, SymbolId name) {
            object ret;
            if (TryGetBoundCustomMember(context, name, out ret)) {
                return ret;
            }
            throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'",
                PythonTypeOps.GetName(this), SymbolTable.IdToString(name));
        }

        #region ICustomMembers Members

        public bool TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            return TryGetBoundCustomMember(context, name, out value);
        }

        public bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            int nameId = name.Id;
            if (nameId == Symbols.Dict.Id) {
                //!!! user code can modify __del__ property of __dict__ behind our back
                value = __dict__;
                return true;
            } else if (nameId == Symbols.Class.Id) {
                value = __class__;
                return true;
            }

            if (TryRawGetAttr(context, name, out value)) return true;

            if (nameId != Symbols.GetBoundAttr.Id) {
                object getattr;
                if (TryRawGetAttr(context, Symbols.GetBoundAttr, out getattr)) {
                    try {
                        value = PythonCalls.Call(getattr, SymbolTable.IdToString(name));
                        return true;
                    } catch (MissingMemberException) {
                        // __getattr__ raised AttributeError, return false.
                    }
                }
            }

            return false;
        }

        public void SetCustomMember(CodeContext context, SymbolId name, object value) {
            object setFunc;
            int nameId = name.Id;
            if (nameId == Symbols.Class.Id) {
                SetClass(value);
            } else if (nameId == Symbols.Dict.Id) {
                SetDict(value);
            } else if (__class__.HasSetAttr && __class__.TryLookupSlot(Symbols.SetAttr, out setFunc)) {
                PythonCalls.Call(__class__.GetOldStyleDescriptor(context, setFunc, this, __class__), name.ToString(), value);
            } else if (nameId == Symbols.Unassign.Id) {
                SetFinalizer(name, value);
            } else {
                __dict__[name] = value;
            }
        }

        private void SetFinalizer(SymbolId name, object value) {
            if (!HasFinalizer()) {
                // user is defining __del__ late bound for the 1st time
                AddFinalizer();
            }

            __dict__[name] = value;
        }

        private void SetDict(object value) {
            IAttributesCollection dict = value as IAttributesCollection;
            if (dict == null) {
                throw PythonOps.TypeError("__dict__ must be set to a dictionary");
            }
            if (HasFinalizer() && !__class__.HasFinalizer) {
                if (!dict.ContainsKey(Symbols.Unassign)) {
                    ClearFinalizer();
                }
            } else if (dict.ContainsKey(Symbols.Unassign)) {
                AddFinalizer();
            }


            __dict__ = dict;
        }

        private void SetClass(object value) {
            OldClass oc = value as OldClass;
            if (oc == null) {
                throw PythonOps.TypeError("__class__ must be set to class");
            }
            __class__ = oc;
        }

        public bool DeleteCustomMember(CodeContext context, SymbolId name) {
            if (name == Symbols.Class) throw PythonOps.TypeError("__class__ must be set to class");
            if (name == Symbols.Dict) throw PythonOps.TypeError("__dict__ must be set to a dictionary");

            object delFunc;
            if (__class__.HasDelAttr && __class__.TryLookupSlot(Symbols.DelAttr, out delFunc)) {
                PythonCalls.Call(__class__.GetOldStyleDescriptor(context, delFunc, this, __class__), name.ToString());
                return true;
            }


            if (name == Symbols.Unassign) {
                // removing finalizer
                if (HasFinalizer() && !__class__.HasFinalizer) {
                    ClearFinalizer();
                }
            }

            if (!__dict__.Remove(name)) {
                throw PythonOps.AttributeError("{0} is not a valid attribute", SymbolTable.IdToString(name));
            }
            return true;
        }

        #endregion

        #region ICustomAttributes Members

        public IList<object> GetMemberNames(CodeContext context) {
            SymbolDictionary attrs = new SymbolDictionary(__dict__);
            OldClass.RecurseAttrHierarchy(this.__class__, attrs);
            return List.Make(attrs);
        }

        public IDictionary<object, object> GetCustomMemberDictionary(CodeContext context) {
            return (IDictionary<object, object>)__dict__;
        }

        #endregion

        [SpecialName, PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public object CompareTo(CodeContext context, object other) {
            OldInstance oiOther = other as OldInstance;
            // CPython raises this if called directly, but not via cmp(os,ns) which still calls the user __cmp__
            //if(!(oiOther is OldInstance)) 
            //    throw Ops.TypeError("instance.cmp(x,y) -> y must be an instance, got {0}", Ops.StringRepr(DynamicHelpers.GetPythonType(other)));

            object res = InternalCompare(Symbols.Cmp, other);
            if (res != PythonOps.NotImplemented) return res;
            if (oiOther != null) {
                res = oiOther.InternalCompare(Symbols.Cmp, this);
                if (res != PythonOps.NotImplemented) return ((int)res) * -1;
            }

            return PythonOps.NotImplemented;
        }

        private object CompareForwardReverse(object other, SymbolId forward, SymbolId reverse) {
            object res = InternalCompare(forward, other);
            if (res != PythonOps.NotImplemented) return res;

            OldInstance oi = other as OldInstance;
            if (oi != null) {
                // comparison operators are reflexive
                return oi.InternalCompare(reverse, this);
            }

            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        public static object operator >([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorGreaterThan, Symbols.OperatorLessThan);
        }

        [return: MaybeNotImplemented]
        public static object operator <([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorLessThan, Symbols.OperatorGreaterThan);
        }

        [return: MaybeNotImplemented]
        public static object operator >=([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorGreaterThanOrEqual, Symbols.OperatorLessThanOrEqual);
        }

        [return: MaybeNotImplemented]
        public static object operator <=([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorLessThanOrEqual, Symbols.OperatorGreaterThanOrEqual);
        }

        private object InternalCompare(SymbolId cmp, object other) {
            return InvokeOne(this, other, cmp);
        }

        #region ICustomTypeDescriptor Members
#if !SILVERLIGHT // ICustomTypeDescriptor

        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return CustomTypeDescHelpers.GetAttributes(this);
        }

        string ICustomTypeDescriptor.GetClassName() {
            return CustomTypeDescHelpers.GetClassName(this);
        }

        string ICustomTypeDescriptor.GetComponentName() {
            return CustomTypeDescHelpers.GetComponentName(this);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return CustomTypeDescHelpers.GetConverter(this);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return CustomTypeDescHelpers.GetDefaultEvent(this);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return CustomTypeDescHelpers.GetDefaultProperty(this);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return CustomTypeDescHelpers.GetEditor(this, editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetEvents(attributes);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return CustomTypeDescHelpers.GetEvents(this);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetProperties(attributes);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return CustomTypeDescHelpers.GetProperties(this);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return CustomTypeDescHelpers.GetPropertyOwner(this, pd);
        }

#endif
        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return _weakRef;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            _weakRef = value;
            return true;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            ((IWeakReferenceable)this).SetWeakRef(value);
        }

        #endregion

        #region Rich Equality
        // Specific rich equality support for when the user calls directly from oldinstance type.

        [SpecialName, PythonName("__hash__")]
        public object RichGetHashCode() {
            object func;
            object ret = InvokeOne(this, Symbols.Hash);
            if(ret != PythonOps.NotImplemented) {
                if (!(ret is int))
                    throw PythonOps.TypeError("expected int from __hash__, got {0}", PythonOps.StringRepr(PythonTypeOps.GetName(ret)));

                return (int)ret;
            }

            if (PythonOps.TryGetBoundAttr(DefaultContext.Default, this, Symbols.Cmp, out func) ||
                PythonOps.TryGetBoundAttr(DefaultContext.Default, this, Symbols.OperatorEquals, out func)) {
                throw PythonOps.TypeError("unhashable instance");
            }

            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__eq__")]
        public object RichEquals(object other) {
            object res = InvokeBoth(other, Symbols.OperatorEquals);
            if (res != PythonOps.NotImplemented) {
                return res;
            }


            return PythonOps.NotImplemented;
        }

        private object InvokeBoth(object other, SymbolId si) {
            object res = InvokeOne(this, other, si);
            if (res != PythonOps.NotImplemented) {
                return res;
            }
            OldInstance oi = other as OldInstance;
            if (oi != null) {
                res = InvokeOne(other, this, si);
                if (res != PythonOps.NotImplemented) {
                    return res;
                }
            }
            return PythonOps.NotImplemented;
        }

        private static object InvokeOne(object self, object other, SymbolId si) {
            object func;
            try {
                if (!PythonOps.TryGetBoundAttr(DefaultContext.Default, self, si, out func)) {
                    return PythonOps.NotImplemented;
                }
            } catch (MissingMemberException) {
                return PythonOps.NotImplemented;
            }

            return PythonOps.CallWithContext(DefaultContext.Default, func, other);
        }

        private static object InvokeOne(object self, SymbolId si) {
            object func;
            try {
                if (!PythonOps.TryGetBoundAttr(DefaultContext.Default, self, si, out func)) {
                    return PythonOps.NotImplemented;
                }
            } catch (MissingMemberException) {
                return PythonOps.NotImplemented;
            }

            return PythonOps.CallWithContext(DefaultContext.Default, func);
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ne__")]
        public object RichNotEquals(object other) {
            object res = InvokeBoth(other, Symbols.OperatorNotEquals);
            if (res != PythonOps.NotImplemented) {
                return res;
            }

            return PythonOps.NotImplemented;
        }

        #endregion

        #region ISerializable Members
#if !SILVERLIGHT // SerializationInfo

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("__class__", __class__);
            info.AddValue("__dict__", __dict__);
        }

#endif
        #endregion

        #region Private Implementation Details

        private void RecurseAttrHierarchyInt(OldClass oc, IDictionary<SymbolId, object> attrs) {
            foreach (SymbolId key in oc.__dict__.Keys) {
                if (!attrs.ContainsKey(key)) {
                    attrs.Add(key, key);
                }
            }
            //  recursively get attrs in parent hierarchy
            if (oc.BaseClasses.Count != 0) {
                foreach (OldClass parent in oc.BaseClasses) {
                    RecurseAttrHierarchyInt(parent, attrs);
                }
            }
        }

        private void AddFinalizer() {
            InstanceFinalizer oif = new InstanceFinalizer(this);
            _weakRef = new WeakRefTracker(oif, oif);
        }

        private void ClearFinalizer() {
            if (_weakRef == null) return;

            WeakRefTracker wrt = _weakRef;
            if (wrt != null) {
                // find our handler and remove it (other users could have created weak refs to us)
                for (int i = 0; i < wrt.HandlerCount; i++) {
                    if (wrt.GetHandlerCallback(i) is InstanceFinalizer) {
                        wrt.RemoveHandlerAt(i);
                        break;
                    }
                }

                // we removed the last handler
                if (wrt.HandlerCount == 0) {
                    GC.SuppressFinalize(wrt);
                    _weakRef = null;
                }
            }
        }

        private bool HasFinalizer() {
            if (_weakRef != null) {
                WeakRefTracker wrt = _weakRef;
                if (wrt != null) {
                    for (int i = 0; i < wrt.HandlerCount; i++) {
                        if (wrt.GetHandlerCallback(i) is InstanceFinalizer) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool TryRawGetAttr(CodeContext context, SymbolId name, out object ret) {
            if (__dict__.TryGetValue(name, out ret)) return true;

            if (__class__.TryLookupSlot(name, out ret)) {
                ret = __class__.GetOldStyleDescriptor(context, ret, this, __class__);
                return true;
            }

            return false;
        }

        #endregion

        #region IValueEquality Members

        public int GetValueHashCode() {
            object res = RichGetHashCode();
            if (res is int) {
                return (int)res;
            }
            return base.GetHashCode();
        }

        public bool ValueEquals(object other) {
            return Equals(other);
        }

        public bool ValueNotEquals(object other) {
            return !Equals(other);
        }

        #endregion
    }
}
