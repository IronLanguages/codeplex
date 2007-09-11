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
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Microsoft.Scripting.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting;

namespace IronPython.Runtime.Types {        
    /// <summary>
    /// Type extender is used to extend an existing DynamicType based upon
    /// the static members of the provided extension type.
    /// 
    /// The static members can be decorated with attributes to control how the 
    /// dynamic type is extended.  Available attributes include:
    ///     OperatorMethodAttribute - the method is an operator and a operator is added to the
    ///         operator table.  The operator name must match the .NET standard names for operators.
    ///     PropertyMethodAttribute - currently not implemented
    ///     StaticExtensionMethodAttribute - the method should be added as a static method, not a instance method.
    /// 
    /// Additionally the type extender can be provided an ExtensionNameTransformer.  This is called for all methods
    /// and provides a name and context under which to provide the context-sensitive version of the name.
    /// </summary>
    public class DynamicTypeExtender : CoreReflectedTypeBuilder {
        private DynamicType _toExtend;
        private Type _extensionType;
        private ExtensionNameTransformer _transform;
        private Dictionary<OperatorMapping, OperatorInfo> _operImpl;

        private DynamicTypeExtender(DynamicType toExtend, Type extensionType, ExtensionNameTransformer nameTransform) {
            _toExtend = toExtend;
            _extensionType = extensionType;
            _transform = nameTransform;
            Builder = DynamicTypeBuilder.GetBuilder(toExtend);
        }

        #region Public API Surface

        /// <summary>
        /// Extends the provided dynamic type with the members of the extension type using
        /// the .NET names for the members.
        /// </summary>
        /// <param name="toExtend"></param>
        /// <param name="extensionType"></param>
        public static void ExtendType(DynamicType toExtend, Type extensionType) {
            ExtendType(toExtend, extensionType, null);
        }

        /// <summary>
        /// Extends the provided dynamic type with the members of the extension type using
        /// a custom name transform.
        /// </summary>
        /// <param name="toExtend"></param>
        /// <param name="extensionType"></param>
        /// <param name="nameTransform"></param>
        public static void ExtendType(DynamicType toExtend, Type extensionType, ExtensionNameTransformer nameTransform) {
            new DynamicTypeExtender(toExtend, extensionType, nameTransform ?? DefaultNameTransform).DoExtension();
        }
       
        #endregion

        #region Protected API Surface

        protected override void AddImplicitConversion(MethodInfo mi) {
            throw new NotImplementedException();
        }

        #endregion

        #region Internal implementation details

        /// <summary>
        /// Starting point for updating the DynamicType from the provided
        /// extension type.  
        /// 
        /// This walks over everything exposed from the OpsReflectedType
        /// and exposes it on the dynamic type.
        /// </summary>
        private void DoExtension() {
            Builder.AddInitializer(delegate(DynamicMixinBuilder building) {
                Builder = (DynamicTypeBuilder)building;

                Type curType = _extensionType;
                do {
                    AddMethods(curType);

                    AddFields(curType);

                    curType = curType.BaseType;
                } while (curType != typeof(object));

                PublishOperators();

                Builder.Finish();
            });
            Builder.UnfinishedType.IsExtended = true;
        }

        private void AddMethods(Type curType) {
            foreach (MethodInfo mi in curType.GetMethods()) {
                if (!mi.IsStatic) continue;

                if (mi.IsSpecialName) {
                    StoreOperator(mi);
                } else if (mi.IsDefined(typeof(PropertyMethodAttribute), false)) {
                    StoreProperty(mi);
                } else {
                    StoreMethod(mi);
                }
            }
        }
        
        private void AddFields(Type curType) {
            foreach (FieldInfo fi in curType.GetFields()) {
                if (!fi.IsStatic) continue;
                if (!typeof(DynamicTypeSlot).IsAssignableFrom(fi.FieldType)) continue;

                if (fi.IsDefined(typeof(OperatorSlotAttribute), false)) {
                    StoreOperator(fi);
                } else {
                    StoreSlot(fi);
                }
            }
        }

        private void PublishOperators() {
            if (_operImpl != null) {
                foreach (KeyValuePair<OperatorMapping, OperatorInfo> op in _operImpl) {
                    DynamicTypeSlot callable = (DynamicTypeSlot)op.Value.Callable;
                    DynamicType dt = _toExtend;

                    if (op.Key.Operator == Operators.Call) {
                        // call's special....
                        Builder.AddOperator(op.Value.Context,
                                op.Key.Operator,
                                delegate(CodeContext context, object self, object other, out object ret) {
                                    object value;
                                    if (callable.TryGetValue(context, self, dt, out value)) {
                                        if (other.GetType() == typeof(KwCallInfo)) {
                                            KwCallInfo kwinfo = (KwCallInfo)other;
                                            ret = context.LanguageContext.CallWithKeywordArgs(context,
                                                value,
                                                kwinfo.Arguments,
                                                kwinfo.Names);
                                            return true;
                                        } else {
                                            object[] arroth = (object[])other;
                                            ret = context.LanguageContext.Call(context,
                                                value,
                                                arroth);
                                            return true;
                                        }
                                    }
                                    ret = null;
                                    return false;
                                });
                        return;
                    }

                    if (op.Key.IsUnary) {
                        Builder.AddOperator(op.Value.Context,
                            op.Key.Operator,
                            UnarySite.Make(dt, callable, op.Value.Context).InvokeNotImpl
                        );
                    }

                    if (op.Key.IsBinary) {
                        Builder.AddOperator(op.Value.Context,
                            op.Key.Operator,
                            BinarySite.Make(dt, callable, op.Value.Context).InvokeNotImpl
                        );
                    }

                    if (op.Key.IsTernary) {
                        Builder.AddOperator(op.Value.Context, 
                            op.Key.Operator,
                            TernarySite.Make(dt, callable, op.Value.Context).InvokeNotImpl
                        );
                    }
                }
            }
        }

        private void StoreMethod(MethodInfo mi) {
            // Store under any language specific name
            foreach (TransformedName name in _transform(mi, TransformReason.Method)) {
                StoreMethod(mi, name, GetFunctionType(mi, name));
            }
        }

        /// <summary>
        /// Stores a method with the specified transformed name.
        /// </summary>
        private void StoreMethod(MethodInfo mi, TransformedName name, FunctionType funcType) {
            CodeContext ctx = SimpleContext.Create(name.Context);
            RemoveNonOps(ctx, SymbolTable.StringToId(name.Name));

            // store language specific version
            StoreMethod(SymbolTable.StringToId(name.Name), name.Name, name.Context, mi, funcType | FunctionType.OpsFunction); 
        }

        /// <summary>
        /// Stores the method for the operator.  We build up a table of operators and then publish it into the type at the end.
        /// </summary>
        /// <param name="mi"></param>
        private void StoreOperator(MethodInfo mi) {
            FunctionType baseFunctionType = FunctionType.Method | FunctionType.OpsFunction;
            foreach(TransformedName name in _transform(mi, TransformReason.Operator)) {
                OperatorInfo info;
                OperatorMapping op = name.Operator;
                FunctionType ft = baseFunctionType;

                if (op != null) {
                    if (_operImpl == null) _operImpl = new Dictionary<OperatorMapping, OperatorInfo>();
                    
                    if (op.IsBinary && op.Operator != Operators.GetItem) ft |= FunctionType.BinaryOperator;                    
                    if (op.IsReversed)  ft |= FunctionType.ReversedOperator;
                    if (name.Context != ContextId.Empty) ft |= FunctionType.AlwaysVisible;
                    
                    if (!_operImpl.TryGetValue(op, out info)) {
                        object bf = BuiltinFunction.MakeMethod(name.Name ?? mi.Name, mi, ft).GetDescriptor();

                        _operImpl[op] = info = new OperatorInfo(bf, name.Context);
                    } else {
                        info.Callable = ExtendMethod(mi, ft, info.Callable);
                    }

                    if (name.Name != null) {
                        SetValue(SymbolTable.StringToId(name.Name), name.Context, info.Callable);
                    }
                } else if (name.Name != null) {
                    StoreMethod(mi, name, GetFunctionType(mi, name));
                }
            }
        }

        private bool StoreProperty(MethodInfo mi) {
            if (!mi.Name.StartsWith("Get") && !mi.Name.StartsWith("Set")) return false;

            ExtensionPropertyInfo info = new ExtensionPropertyInfo(_toExtend, mi);

            foreach (TransformedName name in _transform(mi, TransformReason.Property)) {
                SymbolId propName = SymbolTable.StringToId(name.Name);
                object tmp;
                if(!TryGetValue(propName, name.Context, out tmp)) {
                    SetValue(propName, name.Context, new ReflectedExtensionProperty(info, NameType.PythonProperty));
                }
            }
            return false;
        }

        private void StoreOperator(FieldInfo fi) {
            foreach (TransformedName name in _transform(fi, TransformReason.Operator)) {
                OperatorInfo info;
                OperatorMapping op = name.Operator;

                if (op != null) {
                    if (_operImpl == null) _operImpl = new Dictionary<OperatorMapping, OperatorInfo>();

                    if (!_operImpl.TryGetValue(op, out info)) {
                        _operImpl[op] = new OperatorInfo(fi.GetValue(null), name.Context);
                    } else {
                        throw new InvalidOperationException(String.Format("operators name collision with DynamicTypeSlot.  Field: {0}, Transformed Name: {1}", fi.Name, name.Name));
                    }
                }

                if (name.Name != null) {
                    SetValue(SymbolTable.StringToId(name.Name), name.Context, fi.GetValue(null));
                }

            }
        }

        private void StoreSlot(FieldInfo fi) {
            foreach (TransformedName name in _transform(fi, TransformReason.Field)) {
                SetValue(SymbolTable.StringToId(name.Name), name.Context, fi.GetValue(null));
            }
        }

        /// <summary>
        /// Calculates the function time for the given method.
        /// </summary>
        private static FunctionType GetFunctionType(MethodInfo mi, TransformedName name) {
            FunctionType funcType = FunctionType.Method;
            if (mi.IsDefined(typeof(StaticExtensionMethodAttribute), false))
                funcType = FunctionType.Function;

            /*if (mi.DeclaringType == typeof(ArrayOps)) 
                funcType |= FunctionType.SkipThisCheck;*/

            if (name.Context != ContextId.Empty)
                funcType |= FunctionType.AlwaysVisible;
            return funcType;
        }

        /// <summary>
        /// Helper function to remove any methods that aren't marked w/
        /// Function.OpsFunction from the type we're updating.
        /// </summary>
        private void RemoveNonOps(CodeContext context, SymbolId id) {
            if (ContainsNonOps(context, id)) {
                Builder.RemoveSlot(context.LanguageContext.ContextId, id);
            }
        }

        /// <summary>
        /// Helper function to check if the dictionary contains a function
        /// which isn't marked with a FunctionType.OpsFunction tag.
        /// </summary>
        private bool ContainsNonOps(CodeContext context, SymbolId id) {
            DynamicTypeSlot value;
            if (_toExtend.TryLookupSlot(context,
                id,
                out value)) {

                BuiltinFunction rum = value as BuiltinFunction;
                BuiltinMethodDescriptor bimd;
                if (rum != null) {
                    if ((rum.FunctionType & FunctionType.OpsFunction) != 0) 
                        return false;
                } 
                
                if ((bimd = value as BuiltinMethodDescriptor) != null) {
                    if ((bimd.Template.FunctionType & FunctionType.OpsFunction) != 0) 
                        return false;
                }

                return true;
            }
            return false;
        }

        #endregion

        /// <summary>
        /// The default name transform just returns the member's .NET name within
        /// the default context.
        /// </summary>
        private static IEnumerable<TransformedName> DefaultNameTransform(MemberInfo member, TransformReason reason) {
            switch(reason) {
                case TransformReason.Method: 
                    yield return new TransformedName(member.Name, ContextId.Empty); 
                    break;
                case TransformReason.Operator:
                    OperatorMapping opmap;
                    if (ReflectedTypeBuilder.OperatorTable.TryGetValue(member.Name, out opmap)) {
                        yield return new TransformedName(opmap, ContextId.Empty);
                    }
                    break;
                case TransformReason.Property:
                    break;
            }
        }

        class OperatorInfo {
            public ContextId Context;
            public object Callable;

            public OperatorInfo(object callable, ContextId context) {
                Callable = callable;
                Context = context;
            }
        }
    }
}
