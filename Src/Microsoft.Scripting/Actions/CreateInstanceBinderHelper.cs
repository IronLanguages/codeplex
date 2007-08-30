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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using System.Diagnostics;
    using System.Reflection;
    using Microsoft.Scripting.Utils;

    public class CreateInstanceBinderHelper<T> : CallBinderHelper<T, CreateInstanceAction> {
        public CreateInstanceBinderHelper(CodeContext context, CreateInstanceAction action, object []args)
            : base(context, action, args) {
        }

        public override StandardRule<T> MakeRule() {
            DynamicType dt = Arguments[0] as DynamicType;
            if (dt == null || !dt.IsSystemType) {   // base CreateInstance doesn't know how to create non-system types...
                Type t = CompilerHelpers.GetType(Arguments[0]);
                if (typeof(IConstructorWithCodeContext).IsAssignableFrom(t)) {
                    // TODO: This should go away when IConstructorWCC goes away.
                    Debug.Assert(!Action.HasKeywordArgument());

                    Expression call = Ast.Call(Rule.Parameters[0], typeof(IConstructorWithCodeContext).GetMethod("Construct"), GetICallableParameters(t, Rule));

                    Rule.SetTarget(Rule.MakeReturn(Binder, call));
                    Rule.MakeTest(t);

                    return Rule;
                }
            }
            return base.MakeRule();
        }

        protected override MethodBase[] GetTargetMethods() {
            object target = Arguments[0];
            Type t = GetTargetType(target);

            if (t != null) {
                Test = Ast.AndAlso(Test, Ast.Equal(Rule.Parameters[0], Ast.RuntimeConstant(target)));

                if (t.IsArray) {
                    // The JIT verifier doesn't like new int[](3) even though it appears as a ctor.
                    // We could do better and return newarr in the future.
                    return new MethodBase[] { GetArrayCtor(t) };
                }

                BindingFlags bf = BindingFlags.Instance | BindingFlags.Public;
                if (ScriptDomainManager.Options.PrivateBinding) {
                    bf |= BindingFlags.NonPublic;
                }

                ConstructorInfo[] ci = t.GetConstructors(bf);
                
                if (t.IsValueType) {
                    // structs don't define a parameterless ctor, add a generic method for that.
                    return ArrayUtils.Insert<MethodBase>(GetStructDefaultCtor(t), ci);
                }

                return ci;
            }

            return null;
        }
        
        private static Type GetTargetType(object target) {
            Type t = target as Type;
            if (t == null) {
                DynamicType dt = target as DynamicType;
                if (dt != null) {
                    t = dt.UnderlyingSystemType;
                }
            }
            return t;
        }

        private MethodBase GetStructDefaultCtor(Type t) {
            return typeof(RuntimeHelpers).GetMethod("CreateInstance").MakeGenericMethod(t);
        }

        private MethodBase GetArrayCtor(Type t) {
            return typeof(RuntimeHelpers).GetMethod("CreateArray").MakeGenericMethod(t.GetElementType());
        }

        protected override void MakeCannotCallRule(Type type) {
            string name = type.Name;
            DynamicType dt = Arguments[0] as DynamicType;
            if (dt != null) name = dt.Name;
            Type t = Arguments[0] as Type;
            if (t != null) name = t.Name;

            Rule.SetTarget(
                Rule.MakeError(Binder,
                    Ast.New(
                        typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                        Ast.Constant("Cannot create instances of " + name)
                    )
                )
            );
        }
    }
}
