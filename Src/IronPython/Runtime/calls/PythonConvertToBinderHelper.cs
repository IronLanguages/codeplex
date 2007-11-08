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
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Ast;

using IronPython.Runtime.Types;

namespace IronPython.Runtime.Calls {
    using Ast = Microsoft.Scripting.Ast.Ast;
    using IronPython.Runtime.Operations;

    class PythonConvertToBinderHelper<T> : BinderHelper<T, ConvertToAction> {
        private object _argument;

        public PythonConvertToBinderHelper(CodeContext/*!*/ context, ConvertToAction/*!*/ action, object[]/*!*/ args)
            : base(context, action) {
            Contract.RequiresNotNull(context, "context");
            Contract.RequiresNotNull(action, "action");
            Contract.RequiresNotNull(args, "args");
            Contract.Requires(args.Length == 1, "args", "must have single object to convert");

            _argument = args[0];
        }

        public StandardRule<T> MakeRule() {
            StandardRule<T> rule = null;
            if (Action.ToType == typeof(bool)) {
                rule = MakeBoolRule();
            } else if (Action.ToType == typeof(char)) {
                rule = MakeCharRule();
            }

            return rule;
        }

        private StandardRule<T> MakeCharRule() {
            StandardRule<T> rule = new StandardRule<T>();
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
                        Ast.ReadProperty(
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

                Expression getLen = Ast.ReadProperty(
                    Ast.ConvertHelper(
                        strExpr,
                        typeof(string)
                    ),
                    typeof(string).GetProperty("Length")
                );

                if (strVal.Length == 1) {
                    rule.AddTest(Ast.Equal(getLen, Ast.Constant(1)));
                    rule.SetTarget(
                        rule.MakeReturn(
                            Binder,
                            Ast.Call(
                                Ast.ConvertHelper(strExpr, typeof(string)),
                                typeof(string).GetMethod("get_Chars"),
                                Ast.Constant(0)
                            )
                        )
                    );
                } else {
                    rule.AddTest(Ast.NotEqual(getLen, Ast.Constant(1)));
                    rule.SetTarget(
                        rule.MakeError(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("TypeError"),
                                Ast.Constant("expected string of length 1 when converting to char, got '{0}'"),
                                Ast.NewArray(typeof(object[]), rule.Parameters[0])
                            )
                        )
                    );
                }
            } else {
                // let the default binder produce the rule
                rule = null;
            }

            return rule;
        }

        private StandardRule<T> MakeBoolRule() {
            Type fromType = CompilerHelpers.GetType(_argument);
            StandardRule<T> rule = new StandardRule<T>();

            if (fromType == typeof(None)) {
                // null is never true
                rule.SetTarget(rule.MakeReturn(Binder, Ast.Constant(false)));
            } else if (fromType == typeof(string)) {
                MakeNonZeroPropertyRule(rule, typeof(string), "Length");
            } else if (_argument is ICollection) {
                // collections are true if not empty
                MakeNonZeroPropertyRule(rule, typeof(ICollection), "Count");
            } else if (_argument is IPythonContainer) {
                // collections are true if not empty
                MakeIPythonContainerRule(rule);
            } else if (_argument is IStrongBox) {
                // Explictly block conversion of References to bool
                MakeStrongBoxRule(rule);
            } else if (fromType.IsEnum) {
                MakeEnumRule(rule);
            } else if (fromType.IsPrimitive) {
                MakePrimitiveRule(rule);
            } else if (fromType == typeof(Complex64)) {
                MakeComplexRule(rule);                
            } else if (fromType == typeof(BigInteger)) {
                MakeBigIntegerRule(rule);
            } else {
                // check for ICollection<T>
                foreach (Type t in fromType.GetInterfaces()) {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)) {
                        // collections are true if not empty
                        rule = new StandardRule<T>();
                        MakeNonZeroPropertyRule(rule, t, "Count");
                        break;
                    }
                }
            }

            rule.MakeTest(fromType);
            if (rule.Target == null) {
                // anything non-null that doesn't fall under one of the
                // above rules is true
                StandardRule<T> newrule = new ConvertToBinderHelper<T>(Context, Action, new object[] { _argument }).MakeRule();
                if (!newrule.IsError) {
                    rule = newrule;
                } else {
                    rule.SetTarget(rule.MakeReturn(Binder, Ast.Constant(true)));
                }
            }
            return rule;
        }

        private void MakeBigIntegerRule(StandardRule<T> rule) {
            rule.SetTarget(
                rule.MakeReturn(
                Binder,
                    Ast.Call(
                        typeof(BigInteger).GetMethod("op_Inequality", new Type[] { typeof(BigInteger), typeof(BigInteger) }),
                        Ast.ReadField(null, typeof(BigInteger).GetField("Zero")),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(BigInteger))
                    )
                )
            );
        }

        private void MakeComplexRule(StandardRule<T> rule) {            
            rule.SetTarget(
                rule.MakeReturn(
                Binder,
                    Ast.Call(
                        typeof(Complex64).GetMethod("op_Inequality", new Type[] { typeof(Complex64), typeof(Complex64) }),
                        Ast.Constant(new Complex64()),
                        Ast.ConvertHelper(rule.Parameters[0], typeof(Complex64))
                    )
                )
            );
        }

        private void MakePrimitiveRule(StandardRule<T> rule) {
            object zeroVal = Activator.CreateInstance(CompilerHelpers.GetType(_argument));
            rule.SetTarget(
                rule.MakeReturn(
                    Binder,
                    Ast.NotEqual(
                        Ast.Constant(zeroVal),
                        Ast.ConvertHelper(
                            rule.Parameters[0],
                            CompilerHelpers.GetType(_argument)
                        )
                    )
                )
            );
        }

        private void MakeEnumRule(StandardRule<T> rule) {
            Type enumStorageType = Enum.GetUnderlyingType(CompilerHelpers.GetType(_argument));
            object zeroVal = Activator.CreateInstance(enumStorageType);
            rule.SetTarget(
                rule.MakeReturn(
                    Binder,
                    Ast.Equal(
                        Ast.Convert(
                            rule.Parameters[0],
                            enumStorageType
                        ),
                        Ast.Constant(enumStorageType)
                    )
                )
            );
        }

        private void MakeStrongBoxRule(StandardRule<T> rule) {
            rule.SetTarget(
                rule.MakeError(
                    Ast.Call(
                        typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                        Ast.Constant("Can't convert a Reference<> instance to a bool")
                    )
                )
            );
        }

        private void MakeICollectionRule(StandardRule<T> rule, Type collectionType) {
            MakeNonZeroPropertyRule(rule, collectionType, "Count");
        }

        private void MakeNonZeroPropertyRule(StandardRule<T> rule, Type collectionType, string propertyName) {
            rule.SetTarget(
                rule.MakeReturn(
                    Binder,
                    Ast.NotEqual(
                        Ast.ReadProperty(
                            Ast.ConvertHelper(
                                rule.Parameters[0],
                                collectionType
                            ),
                            collectionType.GetProperty(propertyName)
                        ),
                        Ast.Constant(0)
                    )
                )
            );
        }

        private void MakeIPythonContainerRule(StandardRule<T> rule) {
            rule.SetTarget(
                rule.MakeReturn(
                    Binder,
                    Ast.NotEqual(
                        Ast.Call(
                            typeof(IPythonContainer).GetMethod("GetLength"),
                            Ast.ConvertHelper(
                                rule.Parameters[0],
                                typeof(ICollection)
                            )                            
                        ),
                        Ast.Constant(0)
                    )
                )
            );
        }
    }
}
