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
using System.Reflection;
using System.Scripting;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Calls {
    using Ast = System.Linq.Expressions.Expression;

    class PythonConvertToBinderHelper<T> : BinderHelper<T, OldConvertToAction> where T : class {
        private object _argument;

        public PythonConvertToBinderHelper(CodeContext/*!*/ context, OldConvertToAction/*!*/ action, object[]/*!*/ args)
            : base(context, action) {
            ContractUtils.RequiresNotNull(context, "context");
            ContractUtils.RequiresNotNull(action, "action");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.Requires(args.Length == 1, "args", "must have single object to convert");

            _argument = args[0];
        }

        public RuleBuilder<T> MakeRule() {
            RuleBuilder<T> rule = null;

            if (Action.ToType == typeof(bool)) {
                rule = MakeBoolRule();
            } else if (Action.ToType == typeof(char)) {
                rule = MakeCharRule();
            } else if (Action.ToType.IsArray && _argument is PythonTuple && Action.ToType.GetArrayRank() == 1) {
                rule = MakeArrayRule();
            } else if (Action.ToType.IsGenericType && !Action.ToType.IsAssignableFrom(CompilerHelpers.GetType(_argument))) {
                Type genTo = Action.ToType.GetGenericTypeDefinition();

                // Interface conversion helpers...
                if (genTo == typeof(IList<>)) {
                    rule = MakeGenericWrapperRule(typeof(IList<object>), typeof(ListGenericWrapper<>));
                } else if (genTo == typeof(IDictionary<,>)) {
                    rule = MakeGenericWrapperRule(typeof(IDictionary<object, object>), typeof(DictionaryGenericWrapper<,>));
                } else if (genTo == typeof(IEnumerable<>)) {
                    rule = MakeGenericWrapperRule(typeof(IEnumerable), typeof(IEnumerableOfTWrapper<>));
                }
            }

            return rule;
        }

        private RuleBuilder<T> MakeGenericWrapperRule(Type fromType, Type wrapperType) {
            RuleBuilder<T> rule = null;

            if (fromType.IsAssignableFrom(CompilerHelpers.GetType(_argument))) {
                Type making = wrapperType.MakeGenericType(Action.ToType.GetGenericArguments());
                
                rule = new RuleBuilder<T>();
                rule.MakeTest(CompilerHelpers.GetType(_argument));
                rule.Target = rule.MakeReturn(
                    Binder,
                    Ast.New(
                        making.GetConstructor(new Type[] { fromType }),
                        Ast.ConvertHelper(
                            rule.Parameters[0],
                            fromType
                        )
                    )
                );
            }
            return rule;
        }

        private RuleBuilder<T> MakeArrayRule() {
            RuleBuilder<T> rule = new RuleBuilder<T>();
            PythonBinderHelper.MakeTest(rule, DynamicHelpers.GetPythonType(_argument));
            rule.Target = rule.MakeReturn(
                Binder,
                Ast.Call(
                    typeof(PythonOps).GetMethod("ConvertTupleToArray").MakeGenericMethod(Action.ToType.GetElementType()),
                    Ast.Convert(
                        rule.Parameters[0],
                        typeof(PythonTuple)
                    )
                )
            );
            return rule;
        }

        private RuleBuilder<T> MakeCharRule() {
            RuleBuilder<T> rule = new RuleBuilder<T>();
            // we have an implicit conversion to char if the
            // string length == 1, but we can only represent
            // this is implicit via a rule.
            string strVal = _argument as string;
            Expression strExpr = rule.Parameters[0];
            if (strVal == null) {
                Extensible<string> extstr = _argument as Extensible<string>;
                if (extstr != null) {
                    strVal = extstr.Value;
                    strExpr = 
                        Ast.Property(
                            Ast.ConvertHelper(
                                strExpr,
                                typeof(Extensible<string>)
                            ),
                            typeof(Extensible<string>).GetProperty("Value")
                        );
                }
            }

            if (strVal != null) {
                rule.MakeTest(CompilerHelpers.GetType(_argument));

                Expression getLen = Ast.Property(
                    Ast.ConvertHelper(
                        strExpr,
                        typeof(string)
                    ),
                    typeof(string).GetProperty("Length")
                );

                if (strVal.Length == 1) {
                    rule.AddTest(Ast.Equal(getLen, Ast.Constant(1)));
                    rule.Target =
                        rule.MakeReturn(
                            Binder,
                            Ast.Call(
                                Ast.ConvertHelper(strExpr, typeof(string)),
                                typeof(string).GetMethod("get_Chars"),
                                Ast.Constant(0)
                            )
                        );                    
                } else {
                    rule.AddTest(Ast.NotEqual(getLen, Ast.Constant(1)));
                    rule.Target = rule.MakeError(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("TypeError"),
                                Ast.Constant("expected string of length 1 when converting to char, got '{0}'"),
                                Ast.NewArrayInit(typeof(object), rule.Parameters[0])
                            )                        
                        );
                }
            } else {
                // let the default binder produce the rule
                rule = null;
            }

            return rule;
        }

        private RuleBuilder<T> MakeBoolRule() {
            Type fromType = CompilerHelpers.GetType(_argument);
            RuleBuilder<T> rule = new RuleBuilder<T>();

            if (fromType == typeof(None)) {
                // null is never true
                rule.Target = rule.MakeReturn(Binder, Ast.Constant(false));
            } else if (fromType == typeof(string)) {
                MakeNonZeroPropertyRule(rule, typeof(string), "Length");
            } else if (_argument is ICollection) {
                // collections are true if not empty
                MakeNonZeroPropertyRule(rule, typeof(ICollection), "Count");
            } else if (_argument is System.Runtime.CompilerServices.IStrongBox) {
                // Explictly block conversion of References to bool
                MakeStrongBoxRule(rule);
            } else if (fromType.IsEnum) {
                MakeEnumRule(rule);
            } else if (fromType.IsPrimitive) {
                MakePrimitiveRule(rule);
            } else if (fromType == typeof(Complex64)) {
                MakeComplexRule(rule, rule.Parameters[0]);                
            } else if (fromType == typeof(BigInteger)) {
                MakeBigIntegerRule(rule, rule.Parameters[0]);
            } else if (typeof(Extensible<BigInteger>).IsAssignableFrom(fromType)) {
                MakeBigIntegerRule(rule,
                    Ast.Property(Ast.ConvertHelper(rule.Parameters[0], fromType), fromType.GetProperty("Value"))
                );
            } else if (typeof(Extensible<Complex64>).IsAssignableFrom(fromType)) {
                MakeComplexRule(rule,
                    Ast.Property(Ast.ConvertHelper(rule.Parameters[0], fromType), fromType.GetProperty("Value"))
                );
            } else {
                // check for ICollection<T>
                foreach (Type t in fromType.GetInterfaces()) {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)) {
                        // collections are true if not empty
                        rule = new RuleBuilder<T>();
                        MakeNonZeroPropertyRule(rule, t, "Count");
                        break;
                    }
                }
            }

            rule.MakeTest(fromType);
            // TODO: We could just do this and eleminate all the above checks via the appropriate ops methods
            // look for __nonzero__
            if (rule.Target == null) {
                MemberGroup mg = Binder.GetMember(Action, fromType, "__nonzero__");
                if (mg.Count > 0) {
                    MethodBinder mb = MethodBinder.MakeBinder(Binder, "__nonzero__", GetTargets(mg));
                    BindingTarget bt = mb.MakeBindingTarget(CallTypes.ImplicitInstance, new Type[] { fromType });
                    if (bt.Success) {
                        rule.Target = rule.MakeReturn(Binder, bt.MakeExpression(rule, rule.Parameters));
                    }
                }                
            }

            // look for __len__
            if (rule.Target == null) {
                MemberGroup mg = Binder.GetMember(Action, fromType, "__len__");
                if (mg.Count > 0) {
                    MethodBinder mb = MethodBinder.MakeBinder(Binder, "__nonzero__", GetTargets(mg));
                    BindingTarget bt = mb.MakeBindingTarget(CallTypes.ImplicitInstance, new Type[] { fromType });
                    if (bt.Success) {
                        rule.Target = rule.MakeReturn(
                            Binder, 
                            Ast.NotEqual(
                                bt.MakeExpression(rule, rule.Parameters),
                                Ast.Constant(0)
                            )
                        );
                    }
                }
            }

            // fall back to DLR conversions or Python's default.
            if (rule.Target == null) {
                // anything non-null that doesn't fall under one of the
                // above rules is true
                RuleBuilder<T> newrule = new ConvertToBinderHelper<T>(Context, Action, new object[] { _argument }).MakeRule();
                if (!newrule.IsError) {
                    rule = newrule;
                } else {
                    rule.Target = rule.MakeReturn(Binder, Ast.Constant(true));
                }
            }
            return rule;
        }

        private static List<MethodBase> GetTargets(MemberGroup mg) {
            List<MethodBase> targets = new List<MethodBase>();
            foreach (MemberTracker mt in mg) {
                if (mt.MemberType == TrackerTypes.Method) {
                    targets.Add(((MethodTracker)mt).Method);
                }
            }
            return targets;
        }

        private void MakeBigIntegerRule(RuleBuilder<T> rule, Expression bigInt) {
            rule.Target = 
                rule.MakeReturn(
                    Binder,
                    Ast.Call(
                        typeof(BigInteger).GetMethod("op_Inequality", new Type[] { typeof(BigInteger), typeof(BigInteger) }),
                        Ast.Field(null, typeof(BigInteger).GetField("Zero")),
                        Ast.ConvertHelper(bigInt, typeof(BigInteger))
                    )
                );
        }
        
        private void MakeComplexRule(RuleBuilder<T> rule, Expression complex) {            
            rule.Target = 
                rule.MakeReturn(
                    Binder,
                    Ast.Call(
                        typeof(Complex64).GetMethod("op_Inequality", new Type[] { typeof(Complex64), typeof(Complex64) }),
                        Utils.Constant(new Complex64()),
                        Ast.ConvertHelper(complex, typeof(Complex64))
                    )
                );
        }

        private void MakePrimitiveRule(RuleBuilder<T> rule) {
            object zeroVal = Activator.CreateInstance(CompilerHelpers.GetType(_argument));
            rule.Target = rule.MakeReturn(
                Binder,
                Ast.NotEqual(
                    Ast.Constant(zeroVal),
                    Ast.ConvertHelper(
                        rule.Parameters[0],
                        CompilerHelpers.GetType(_argument)
                    )
                )
            );
        }

        private void MakeEnumRule(RuleBuilder<T> rule) {
            Type enumStorageType = Enum.GetUnderlyingType(CompilerHelpers.GetType(_argument));
            object zeroVal = Activator.CreateInstance(enumStorageType);
            rule.Target = rule.MakeReturn(
                Binder,
                Ast.NotEqual(
                    Ast.Convert(
                        rule.Parameters[0],
                        enumStorageType
                    ),
                    Ast.Constant(zeroVal)
                )
            );
        }

        private void MakeStrongBoxRule(RuleBuilder<T> rule) {
            rule.Target = rule.MakeError(
                Ast.Call(
                    typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                    Ast.Constant("Can't convert a Reference<> instance to a bool")
                )
            );
        }

        private void MakeICollectionRule(RuleBuilder<T> rule, Type collectionType) {
            MakeNonZeroPropertyRule(rule, collectionType, "Count");
        }

        private void MakeNonZeroPropertyRule(RuleBuilder<T> rule, Type collectionType, string propertyName) {
            rule.Target = rule.MakeReturn(
                Binder,
                Ast.NotEqual(
                    Ast.Property(
                        Ast.ConvertHelper(
                            rule.Parameters[0],
                            collectionType
                        ),
                        collectionType.GetProperty(propertyName)
                    ),
                    Ast.Constant(0)
                )            
            );
        }
    }
}
