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
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using System.Collections.Generic;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class GetMemberBinderHelper<T> : BinderHelper<T, GetMemberAction> {
        private Type _strongBoxType;

        public GetMemberBinderHelper(CodeContext context, GetMemberAction action)
            : base(context, action) {
            Utils.Assert.NotNull(context, action);
        }

        public StandardRule<T> MakeNewRule(object[] args) {
            Debug.Assert(args != null && args.Length == 1);

            object target = args[0];
            if (IsStrongBox(target)) {
                _strongBoxType = target.GetType();
                target = ((IStrongBox)target).Value;
            }

            DynamicType targetType = DynamicHelpers.GetDynamicType(target);

            // Disable caching for the dynamic cases
            if (!ShouldMakeDynamic(target, targetType)) {
                return MakeRule(targetType.UnderlyingSystemType);
            }

            return MakeDynamicRule(targetType);        
        }

        private StandardRule<T> MakeRule(Type type) {
            string finding = SymbolTable.IdToString(Action.Name);
            try {
                PropertyInfo pi = type.GetProperty(finding);
                if (pi != null) {
                    return MakeGetMemberRule(type, pi);
                }
            } catch (AmbiguousMatchException) {
                // could have a "new" property replacing the previous property.
                Type curType = type;                
                PropertyInfo pi;
                do {
                    pi = curType.GetProperty(finding, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    curType = curType.BaseType;
                } while (pi == null && curType != null);
                Debug.Assert(pi != null);

                return MakeGetMemberRule(type, pi);                
            }

            try {
                FieldInfo fi = type.GetField(finding);
                if (fi != null) {
                    return MakeGetMemberRule(type, fi);
                }
            } catch (AmbiguousMatchException) {
                // could have a "new" field replacing the previous property.
                Type curType = type;
                FieldInfo fi;
                do {
                    fi = curType.GetField(finding, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    curType = curType.BaseType;
                } while (fi == null && curType != null);
                Debug.Assert(fi != null);

                return MakeGetMemberRule(type, fi);
            }

            if (_strongBoxType != null) {
                // TODO: We should support this in more general cases to enable composable sites that avoid
                // production of the BoundBuiltinFunction.

                BuiltinFunction bf = ReflectionCache.GetMethodGroup(type, finding);
                if(bf != null) {
                    return MakeMethodRule(bf, _strongBoxType);
                }
            }

            return MakeDynamicRule(_strongBoxType ?? type);        
        }

        private StandardRule<T> MakeGetMemberRule(Type targetType, MemberInfo mi) {
            StandardRule<T> rule = new StandardRule<T>();
            if (TryMakeGetMemberRule(rule, mi, GetParametersForRule(rule))) {
                rule.MakeTest(_strongBoxType ?? targetType);
                return rule;
            }
            return MakeDynamicRule(_strongBoxType ?? targetType);
        }

        private Expression[] GetParametersForRule(StandardRule<T> rule) {
            if (_strongBoxType == null) return rule.Parameters;

            Expression[] prms = (Expression[])rule.Parameters.Clone();
            prms[0] = Ast.ReadField(Ast.Cast(prms[0], _strongBoxType), _strongBoxType.GetField("Value"));
            return prms;
        }

        /// <summary>
        /// Public helper function which makes a Get Member rule for the given type.
        /// 
        /// This does not check if the object is unsuitable for producing a GetMember rule.
        /// </summary>
        public bool TryMakeGetMemberRule(StandardRule<T> rule, DynamicTypeSlot slot, params Expression[] args) {
            ReflectedField rf;
            ReflectedProperty rp;

            if ((rf = slot as ReflectedField) != null) {
                return TryMakeGetMemberRule(rule, rf.info, args);
            } else if ((rp = slot as ReflectedProperty) != null) {
                return TryMakeGetMemberRule(rule, rp.Info, args);
            } else {
                // TODO handle builtin methods specially here
                return false;
            }
        }

        public bool TryMakeGetMemberRule(StandardRule<T> rule, MemberInfo mi, params Expression[] args) {
            switch(mi.MemberType) {
                case MemberTypes.Field:
                    return TryMakeFieldRule(rule, (FieldInfo)mi, args);
                case MemberTypes.Property:
                    return TryMakePropertyRule(rule, (PropertyInfo)mi, args);
                default: return false;
            }
        }

        private bool ShouldMakeDynamic(object target, DynamicType targetType) {
            if (target is ICustomMembers || IsNonSystemMutableType(target, targetType)) return true;

            if (!targetType.IsSystemType || targetType.IsExtended) {
                return targetType.Version == DynamicMixin.DynamicVersion || targetType.HasDynamicMembers(Context);
            }

            return false;
        }

        private bool IsNonSystemMutableType(object target, DynamicType targetType) {
            if (targetType.IsSystemType) return false;

            ISuperDynamicObject sdo = target as ISuperDynamicObject;
            if (sdo != null && !sdo.HasDictionary) {  
                // instance can't have new members...
                return false;
            }
            return true;
        }

        private bool TryMakePropertyRule(StandardRule<T> rule, PropertyInfo property, params Expression[] args) {
            MethodInfo getter = property.GetGetMethod();
            if (getter != null && CompilerHelpers.CanOptimizeMethod(getter)) {
                Statement call = MakeCallExpression(getter, args);
                if (call != null) {
                    rule.SetTarget(MakeCallExpression(getter, args));
                    return true;
                }
            } 
            return false;
        }

        private Statement MakeCallExpression(MethodInfo method, params Expression [] parameters) {
            ParameterInfo[] infos = method.GetParameters();
            Expression callInst = null;
            int parameter = 0;
            Expression[] callArgs = new Expression[infos.Length];
            
            if (!method.IsStatic) {
                callInst = parameters[0];
                parameter = 1;
            }
            for (int arg = 0; arg < infos.Length; arg++) {
                if (parameter < parameters.Length) {
                    callArgs[arg] = Binder.ConvertExpression(
                        parameters[parameter++],
                        infos[arg].ParameterType);
                } else {
                    return null;
                }
            }

            // check that we used all parameters
            if (parameter != parameters.Length) {
                return null;
            }
            return Ast.Return(Ast.Call(callInst, method, callArgs));
        }

        private static Statement InvalidArgumentCount(MethodInfo method, int expected, int provided) {
            return Ast.Statement(
                Ast.Throw(
                    Ast.Call(
                        null,
                        typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                        Ast.Constant(
                            String.Format("{0}() takes exactly {1} arguments ({1} given)", method.Name, expected, provided)
                        )
                    )
                )
            );
        }

        private bool TryMakeFieldRule(StandardRule<T> rule, FieldInfo field, params Expression [] args) {
            if (!field.IsStatic && CompilerHelpers.CanOptimizeField(field)) {
                rule.SetTarget(
                    rule.MakeReturn(
                        Binder,
                        Ast.ReadField(
                            field.IsStatic ?
                                null :
                                args[0],
                            field
                        )
                    )
                );
                return true;
            } else {
                return false;
            }
        }

        
        private StandardRule<T> MakeDynamicRule(Type targetType) {
            return MakeDynamicRule(DynamicHelpers.GetDynamicTypeFromType(targetType));
        }

        private StandardRule<T> MakeDynamicRule(DynamicType targetType) {
            StandardRule<T> rule = new StandardRule<T>();
            rule.MakeTest(new DynamicType[] { targetType });
            Expression expr = Ast.Call(null,
                    typeof(RuntimeHelpers).GetMethod("GetBoundMember"),
                    Ast.CodeContext(),
                    rule.Parameters[0],
                    Ast.Constant(Action.Name));
            rule.SetTarget(rule.MakeReturn(Binder, expr));
            return rule;
        }
    }
}
