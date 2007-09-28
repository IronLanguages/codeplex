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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.ComponentModel;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

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
                default:
                    return null;
            }
        }

        private StandardRule<T> MakeCallRule<T>(CallAction callAction, CodeContext context, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));
            
            // we could get better throughput w/ a more specific rule against our current custom old class but
            // this favors less code generation.
            Variable tmp = rule.GetTemporary(typeof(object), "callFunc");
            Expression [] callParams = (Expression[])rule.Parameters.Clone();
            callParams[0] = Ast.Read(tmp);
            rule.SetTarget(
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
                    rule.MakeError(context.LanguageContext.Binder,
                        Ast.Call(
                            null,
                            typeof(PythonOps).GetMethod("UncallableError"),
                            rule.Parameters[0]
                        )
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
                Ast.Constant(dict.KeyVersion));
            tryGetValue = Ast.Assign(tmp, tryGetValue);

            Expression test = Ast.AndAlso(
                Ast.NotEqual(
                    rule.Parameters[0],
                    Ast.Null()),
                Ast.Equal(
                    Ast.Call(
                        rule.Parameters[0], typeof(object).GetMethod("GetType")),
                        Ast.Constant(typeof(OldInstance))
                ));
            test = Ast.AndAlso(test,
                Ast.NotEqual(
                    tryGetValue, Ast.Null()));

            rule.SetTest(test);
            Expression target;

            switch (action.Kind) {
                case DynamicActionKind.GetMember:
                    target = Ast.Call(
                                Ast.ReadDefined(tmp),
                                typeof(CustomOldClassDictionary).GetMethod("GetValueHelper"),
                                Ast.Constant(key),
                                rule.Parameters[0]);
                    break;
                case DynamicActionKind.SetMember:
                    target = Ast.Call(
                                Ast.ReadDefined(tmp),
                                typeof(CustomOldClassDictionary).GetMethod("SetExtraValue"),
                                Ast.Constant(key),
                                rule.Parameters[1]);
                    break;
                case DynamicActionKind.DeleteMember:
                    target = Ast.Call(
                                rule.Parameters[0],
                                typeof(OldInstance).GetMethod("DeleteCustomMember"),
                                Ast.CodeContext(),
                                Ast.Constant(action.Name));
                    break;
                default:
                    throw new InvalidOperationException();
            }

            rule.SetTarget(rule.MakeReturn(context.LanguageContext.Binder, target));
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
                                    Ast.ReadField(null, typeof(OperationFailed).GetField("Value"))
                                );
                    } else {
                        target = Ast.Call(instance,
                            typeof(OldInstance).GetMethod("GetBoundMember"),
                                Ast.CodeContext(),
                                Ast.Constant(action.Name));
                    }
                    break;
                case DynamicActionKind.SetMember:
                    target = Ast.Call(instance,
                        typeof(OldInstance).GetMethod("SetCustomMember"),
                            Ast.CodeContext(),
                            Ast.Constant(action.Name),
                            rule.Parameters[1]);
                    break;
                 case DynamicActionKind.DeleteMember:
                    target = Ast.Call(instance,
                        typeof(OldInstance).GetMethod("DeleteCustomMember"),
                            Ast.CodeContext(),
                            Ast.Constant(action.Name));
                    break;
                default:
                    throw new InvalidOperationException();
            }

            rule.SetTarget(rule.MakeReturn(context.LanguageContext.Binder, target));
            return rule;
        }

        private static StandardRule<T> MakeOperationRule<T>(DoOperationAction action, CodeContext context, object[] args) {
            switch (action.Operation) {
                case Operators.GetItem:
                case Operators.SetItem:
                case Operators.DeleteItem:
                    return MakeIndexRule<T>(context, action, args);
                case Operators.GetSlice:
                case Operators.SetSlice:
                case Operators.DeleteSlice:
                    return MakeSliceRule<T>(context, action);
                default:
                    return null;
            }
        }

        private static StandardRule<T> MakeSliceRule<T>(CodeContext context, DoOperationAction action) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));

            Statement normalSlice = GetNormalSliceStatement<T>(context, action, rule);

            if (TryCallSliceMethod<T>(action, rule)) {
                // we need to check for the presence of __getslice__, __setslice__, or __delslice__
                // if(PythonOps.TryGetBoundAttr(this, Symbols.GetSlice, out res))
                //      res(adjustedSlice.Start, adjustedSlice.Stope)
                Variable var = rule.GetTemporary(typeof(object), "sliceFunc");
                Variable adjSlice = rule.GetTemporary(typeof(Slice), "adjustedSlice");
                                
                Expression []sliceArgs = (Expression[])rule.Parameters.Clone();
                sliceArgs[0] = Ast.Comma(
                    Ast.Assign(adjSlice, GetSliceObject<T>(action, rule)),
                    Ast.Read(var)
                );

                Expression sliceTest = Ast.Call(
                    null,
                    typeof(PythonOps).GetMethod("TryGetBoundAttr", new Type[] { 
                        typeof(CodeContext), 
                        typeof(object), 
                        typeof(SymbolId), 
                        typeof(object).MakeByRefType() 
                    }),
                    Ast.CodeContext(),
                    rule.Parameters[0],
                    Ast.Constant(GetDeprecatedSliceMethod(action)),
                    Ast.Read(var)
                );
                sliceTest = AddNumericTest<T>(rule, sliceTest, rule.Parameters[1]);
                sliceTest = AddNumericTest<T>(rule, sliceTest, rule.Parameters[2]);

                sliceArgs[1] = Ast.ReadProperty(Ast.Read(adjSlice), typeof(Slice).GetProperty("Start"));
                sliceArgs[2] = Ast.ReadProperty(Ast.Read(adjSlice), typeof(Slice).GetProperty("Stop"));
                
                rule.SetTarget(
                    Ast.IfThenElse(
                        sliceTest,
                        rule.MakeReturn(
                            context.LanguageContext.Binder,
                            Ast.Action.Call(
                                typeof(object),
                                sliceArgs
                            )
                        ),
                        normalSlice
                    )
                );
            } else {
                rule.SetTarget(normalSlice);
            }
            return rule;
        }

        private static Statement GetNormalSliceStatement<T>(CodeContext context, DoOperationAction action, StandardRule<T> rule) {
            Expression[] normalSliceArgs = new Expression[2 + (action.Operation == Operators.SetSlice ? 1 : 0)];
            normalSliceArgs[0] = Ast.CodeContext();
            normalSliceArgs[1] = GetSliceObject<T>(action, rule);
            if (normalSliceArgs.Length == 3) {
                normalSliceArgs[2] = rule.Parameters[rule.Parameters.Length - 1];
            }
            Statement normalSlice = rule.MakeReturn(
                context.LanguageContext.Binder,
                Ast.Call(
                    rule.Parameters[0],
                    typeof(OldInstance).GetMethod(GetIndexOrSliceMethod(action)),
                    normalSliceArgs
                )
            );
            return normalSlice;
        }

        private static Expression AddNumericTest<T>(StandardRule<T> rule, Expression sliceTest, Expression parameter) {
            if (!PythonOps.IsNumericType(parameter.Type) && parameter.Type != typeof(System.Reflection.Missing)) {
                sliceTest = Ast.AndAlso(sliceTest,
                    Ast.Call(null, typeof(PythonOps).GetMethod("IsNumericObject"),
                    parameter)
                );
            }
            return sliceTest;
        }

        private static bool TryCallSliceMethod<T>(DoOperationAction action, StandardRule<T> rule) {
            int sliceArgCount = GetSliceArgumentCount<T>(action, rule);
            if (sliceArgCount == 2) {
                for (int i = 1; i < sliceArgCount + 1; i++) {
                    if (!PythonOps.IsNumericType(rule.Parameters[i].Type) &&
                        rule.Parameters[i].Type != typeof(System.Reflection.Missing) &&
                        rule.Parameters[i].Type != typeof(object)) {
                        // strongly typed parameter which isn't an integer, we won't call __*slice__
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static SymbolId GetDeprecatedSliceMethod(DoOperationAction action) {
            SymbolId sliceMethod;
            switch (action.Operation) {
                case Operators.GetSlice: sliceMethod = Symbols.GetSlice; break;
                case Operators.SetSlice: sliceMethod = Symbols.SetSlice; break;
                case Operators.DeleteSlice: sliceMethod = Symbols.DeleteSlice; break;
                default: throw new InvalidOperationException();
            }
            return sliceMethod;
        }

        private static Expression GetSliceObject<T>(DoOperationAction action, StandardRule<T> rule) {
            Expression slice;
            int sliceArgCount = GetSliceArgumentCount<T>(action, rule);
            if (sliceArgCount <= 2) {
                // no step is provided, we need a __len__ if either of the arguments are negative                
                slice = Ast.Call(null,
                     typeof(PythonOps).GetMethod("MakeOldStyleSlice"),
                     rule.Parameters[0],
                     sliceArgCount >= 1 ? rule.Parameters[1] : Ast.Null(),
                     sliceArgCount >= 2 ? rule.Parameters[2] : Ast.Null()
                 );
            } else {
                slice = Ast.Call(null,
                    typeof(PythonOps).GetMethod("MakeSlice"),
                    CheckMissing(rule.Parameters[1]),
                    CheckMissing(rule.Parameters[2]),
                    CheckMissing(rule.Parameters[3]));
            }
            return slice;
        }

        internal static Expression CheckMissing(Expression toCheck) {
            if (toCheck.Type == typeof(System.Reflection.Missing)) return Ast.Null();
            if (toCheck.Type != typeof(object)) return toCheck;

            return Ast.Condition(Ast.TypeIs(toCheck, typeof(System.Reflection.Missing)), Ast.Null(), toCheck);
        }

        private static int GetSliceArgumentCount<T>(DoOperationAction action, StandardRule<T> rule) {
            int sliceArgCount = action.Operation == Operators.SetSlice ? rule.Parameters.Length - 2 : rule.Parameters.Length - 1;
            return sliceArgCount;
        }

        private static StandardRule<T> MakeIndexRule<T>(CodeContext context, DoOperationAction action, object[] args) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(typeof(OldInstance));

            rule.SetTarget(
                rule.MakeReturn(context.LanguageContext.Binder,
                    Ast.Call(
                        rule.Parameters[0],
                        typeof(OldInstance).GetMethod(GetIndexOrSliceMethod(action)),
                        PythonBinderHelper.GetCollapsedIndexArguments<T>(action, args, rule)
                    )
                )
            );

            return rule;
        }
        private static string GetIndexOrSliceMethod(DoOperationAction action) {
            string method;
            switch (action.Operation) {
                case Operators.GetItem:
                case Operators.GetSlice: method = "GetItem"; break;
                case Operators.SetItem:
                case Operators.SetSlice: method = "SetItem"; break;
                case Operators.DeleteItem:
                case Operators.DeleteSlice: method = "DeleteItem"; break;
                default: throw new InvalidOperationException();
            }

            return method;
        }

        #endregion

        #region Object overrides

        public override string ToString() {
            object ret;

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.String, out ret)) {
                ret = PythonCalls.Call(ret);
                string strRet;
                if (Converter.TryConvertToString(ret, out strRet) && strRet != null) {
                    return strRet;
                }
                throw PythonOps.TypeError("__str__ returned non-string type ({0})", DynamicTypeOps.GetName(ret));
            }

            return ToCodeString(DefaultContext.Default);
        }

        #endregion

        #region ICodeFormattable Members

        [SpecialName, PythonName("__repr__")]
        public string ToCodeString(CodeContext context) {
            object ret;

            if (TryGetBoundCustomMember(context, Symbols.Repr, out ret)) {
                ret = PythonOps.CallWithContext(context, ret);
                string strRet;
                if (Converter.TryConvertToString(ret, out strRet) && strRet != null) {
                    return strRet;
                }
                throw PythonOps.TypeError("__repr__ returned non-string type ({0})", DynamicTypeOps.GetName(ret));
            }

            return string.Format("<{0} instance at {1}>", __class__.FullName, PythonOps.HexId(this));
        }

        #endregion

        [SpecialName, PythonName("__divmod__")]
        public object DivMod(CodeContext context, object divmod) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.DivMod, out value)) {
                return PythonCalls.Call(value, divmod);
            }


            return PythonOps.NotImplemented;
        }
        [SpecialName, PythonName("__rdivmod__")]
        public object ReverseDivMod(CodeContext context, object divmod) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ReverseDivMod, out value)) {
                return PythonCalls.Call(value, divmod);
            }

            return PythonOps.NotImplemented;
        }


        [SpecialName, PythonName("__coerce__")]
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
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                KwCallInfo kwInfo;

                if (args is object[])
                    return PythonOps.CallWithContext(context, value, (object[])args);
                else if ((kwInfo = args as KwCallInfo) != null)
                    return PythonOps.CallWithKeywordArgs(context, value, kwInfo.Arguments, kwInfo.Names);

                return PythonOps.CallWithContext(context, value, args);
            }

            throw PythonOps.AttributeError("{0} instance has no __call__ method", __class__.Name);
        }

        [SpecialName]
        public object Call(CodeContext context, params object[] args) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                return PythonOps.CallWithContext(context, value, args);
            }

            throw PythonOps.AttributeError("{0} instance has no __call__ method", __class__.Name);
        }

        [SpecialName]
        public object Call(CodeContext context, [ParamDictionary]IAttributesCollection dict, params object[] args) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                return PythonOps.CallWithArgsTupleAndKeywordDictAndContext(context, value, args, ArrayUtils.EmptyStrings, null, dict);
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
                throw PythonOps.TypeError("an integer is required, got {0}", DynamicTypeOps.GetName(value));
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

            if (TryGetBoundCustomMember(context, Symbols.ConvertToInt, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return PythonOps.NotImplemented;
        }

        [SpecialName, PythonName("__long__")]
        public object ConvertToLong(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToLong, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return PythonOps.NotImplemented;
        }

        [SpecialName, PythonName("__float__")]
        public object ConvertToFloat(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToFloat, out value)) {
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
                DynamicTypeOps.GetName(this), SymbolTable.IdToString(name));
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

        public IList<object> GetCustomMemberNames(CodeContext context) {
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
            //    throw Ops.TypeError("instance.cmp(x,y) -> y must be an instance, got {0}", Ops.StringRepr(DynamicHelpers.GetDynamicType(other)));

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
            object meth;
            if (TryGetBoundCustomMember(DefaultContext.Default, cmp, out meth)) 
                return PythonOps.CallWithContext(DefaultContext.Default, meth, other);
            return PythonOps.NotImplemented;
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
            if (PythonOps.TryGetBoundAttr(DefaultContext.Default, this, Symbols.Hash, out func)) {
                object res = PythonCalls.Call(func);
                if (!(res is int))
                    throw PythonOps.TypeError("expected int from __hash__, got {0}", PythonOps.StringRepr(DynamicTypeOps.GetName(res)));

                return (int)res;
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
            object func;
            if (PythonOps.TryGetBoundAttr(DefaultContext.Default, this, Symbols.OperatorEquals, out func)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, func, other);
                if (res != PythonOps.NotImplemented) {
                    return res;
                }
            }

            if (PythonOps.TryGetBoundAttr(DefaultContext.Default, other, Symbols.OperatorEquals, out func)) {
                object res = PythonOps.CallWithContext(DefaultContext.Default, func, this);
                if (res != PythonOps.NotImplemented) {
                    return res;
                }
            }

            object coerce;
            if (TypeCache.OldInstance.TryInvokeBinaryOperator(DefaultContext.Default, Operators.Coerce, this, other, out coerce) &&
                coerce != PythonOps.NotImplemented &&
                !(coerce is OldInstance)) {
                return PythonOps.Equal(((PythonTuple)coerce)[0], ((PythonTuple)coerce)[1]);
            }

            return PythonOps.NotImplemented;
        }

        [return: MaybeNotImplemented]
        [SpecialName, PythonName("__ne__")]
        public object RichNotEquals(object other) {
            object func;
            if (PythonOps.TryGetBoundAttr(DefaultContext.Default, this, Symbols.OperatorNotEquals, out func)) {
                return PythonOps.CallWithContext(DefaultContext.Default, func, other);
            }

            object res = RichEquals(other);
            if (res != PythonOps.NotImplemented) return PythonOps.Not(res);

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
