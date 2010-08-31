/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using TestAst.Runtime;

namespace TestAst {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using EU = ETUtils.ExpressionUtils;

    public partial class TestScenarios {

        #region AST Generation Helpers

        private static Expression GenThrow(string reason, int skipframes) {
            return TestSpan.GetDebugInfoForFrame(
                Ast.Throw(
                    Ast.New(
                        typeof(Exception).GetConstructor(new Type[] { typeof(string) }), 
                        Ast.Constant(reason)
                    )
                ),
                1 + skipframes
            );
        }

        private static Expression GenThrow(string reason) {
            return TestSpan.GetDebugInfoForFrame(
                Ast.Throw(
                    Ast.New(
                        typeof(Exception).GetConstructor(new Type[] { typeof(string) }), 
                        Ast.Constant(reason)
                    )
                )
            );
        }

        private static Expression GenThrow<T>(string reason) where T : Exception {
            return TestSpan.GetDebugInfoForFrame(
                Ast.Throw(
                    Ast.New(
                        typeof(T).GetConstructor(new Type[] { typeof(string) }), 
                        Ast.Constant(reason)
                    )
                )
            );
        }

        private static Expression GenPrint(string text) {
            return Ast.Call(
                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                Ast.Constant(text)
            );
        }

        private static Expression GenPrint(Expression exp) {
            return Ast.Call(
                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(object) }),
                Ast.Convert(exp, typeof(object))
            );
        }

        private static Expression GenAreEqual(Expression x, Expression y) {
            return GenAreEqual(x, y, String.Empty);
        }

        /// <summary>
        /// Generates AST nodes that assert two Expressions are equal
        /// </summary>
        private static Expression GenAreEqual(Expression x, Expression y, string note) {
            //if x==y:
            //  pass
            //else:
            //  print "values are not equal:"
            //  print x
            //  print y
            //  raise Exception("values are not equal")
            IfStatementTest test = AstUtils.IfCondition(
                Ast.Equal(x, y),
                Ast.Empty()
            );

            Expression ifstate = Utils.If(
                new IfStatementTest[] {
                    test
                },
                EU.BlockVoid(
                    GenPrint("values are not equal(" + note + "):"),
                    GenPrint(x),
                    GenPrint(y),
                    GenThrow("values are not equal",1)
                )
            );

            return ifstate;
        }

        private static Expression GenAreNotEqual(Expression x, Expression y) {
            return GenAreNotEqual(x, y, String.Empty);
        }

        /// <summary>
        /// Generates AST nodes that assert two Expressions are not equal
        /// </summary>
        private static Expression GenAreNotEqual(Expression x, Expression y, string note) {
            //if x!=y:
            //  pass
            //else:
            //  print "values are equal:"
            //  print x
            //  print y
            //  raise Exception("values are not ")
            IfStatementTest test = AstUtils.IfCondition(
                Ast.NotEqual(
                    x,
                    y
                ),
                Ast.Empty());

            Expression ifstate = Utils.If(new IfStatementTest[] { test },
                EU.BlockVoid(
                    GenPrint("values are equal (" + note + ":"),
                    GenPrint(x),
                    GenPrint(y),
                    GenThrow("values are equal", 1)
                )
            );

            return ifstate;
        }



        /// <summary>
        /// Generates AST nodes to set var = var + 1.  var should be an int32 or something that can be added to an int32
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        private Expression GenIncrement(ParameterExpression var) {
            return Ast.Assign(var, Ast.Add(var, Ast.Constant(1)));
        }

        private static TryExpression GenAssertExceptionThrown(Type t, Expression s) {
            return Expression.TryCatch(
                EU.BlockVoid(s, GenThrow(String.Format("Expected exception '" + t.ToString() + "'."), 1)),
                Expression.Catch(t, Ast.Empty())
            );
        }

        public static Expression GenFunctionDefinition(string name, Expression body) {
            LambdaExpression func = Ast.Lambda(typeof(TestCallTarget), Expression.Convert(body, typeof(object)), name, new ParameterExpression[0]);

            return Ast.Call(null, typeof(DefaultFunction).GetMethod("Create"),
                Ast.Constant(name),
                Ast.NewArrayInit(typeof(string), new Expression[0]),
                func
            );
        }

        #endregion

        #region General Testing Helpers

        private static void AssertExceptionThrown<T>(Action a) where T : Exception {
            try {
                a();
            } catch (T) {
                return;
            } catch (Exception e) {
                // For some reason, catch(T) doesn't work when running under
                // the debugger, so check for it down here
                if (e is T) {
                    return;
                }
                Assert(false, "Expecting exception '" + typeof(T) + "', instead got: " + e);
            }
            Assert(false, "Expecting exception '" + typeof(T) + "', but no exception was thrown");
        }

        /// <summary>
        /// Asserts two values are equal
        /// </summary>
        private static void AreEqual(object x, object y) {
            if (x == null && y == null) return;

            Assert(x != null && x.Equals(y), String.Format("values aren't equal: {0} and {1}", x, y));
        }

        /// <summary>
        /// Asserts an condition it true
        /// </summary>
        private static void Assert(bool condition, string msg) {
            if (!condition) throw new Exception(String.Format("Assertion failed: {0}", msg));
        }

        private static void Assert(bool condition) {
            if (!condition) throw new Exception("Assertion failed");
        }
        #endregion

        #region Targets for generated code to use
        /// <summary>
        /// Used as the target of a MethodCallExpression in many tests, this
        /// method simply throws with the given reason.
        /// </summary>
        public static bool Throw(string reason) {
            throw new Exception(reason);
        }

#pragma warning disable 649, 169
        /// <summary>
        /// Used as a target for expressions like MemberAssignment
        /// </summary>
        public class Reference {
            public int publicIntField;
            public static int publicStaticIntField;
            private int privateIntField;
            public string publicStringField;
            private string privateStringField;

            private string _publicStringProperty;
            public string publicStringProperty {
                get {
                    return _publicStringProperty;
                }
                set {
                    _publicStringProperty = value;
                }
            }

            private int _publicIntProperty;
            public int publicIntProperty {
                get {
                    return _publicIntProperty;
                }
                set {
                    _publicIntProperty = value;
                }
            }

            private static int _publicStaticIntProperty;
            public static int publicStaticIntProperty {
                get {
                    return _publicStaticIntProperty;
                }
                set {
                    _publicStaticIntProperty = value;
                }
            }

            public void Method() { }
        }

        /// <summary>
        /// Used as a target for expressions like MemberAssignment
        /// </summary>
        public struct Value {
            public int publicIntField;
            public static int publicStaticIntField;
            private int privateIntField;
            public string publicStringField;
            private string privateStringField;

            private string _publicStringProperty;
            public string publicStringProperty {
                get {
                    return _publicStringProperty;
                }
                set {
                    _publicStringProperty = value;
                }
            }

            private int _publicIntProperty;
            public int publicIntProperty {
                get {
                    return _publicIntProperty;
                }
                set {
                    _publicIntProperty = value;
                }
            }

            private static int _publicStaticIntProperty;
            public static int publicStaticIntProperty {
                get {
                    return _publicStaticIntProperty;
                }
                set {
                    _publicStaticIntProperty = value;
                }
            }

            public void Method() { }
        }
#pragma warning restore 649, 169

        public enum Int32Enum : int {
            One,
            Two,
            Three
        };

        public enum Int64Enum : long {
            One,
            Two,
            Three
        };

        public enum Int16Enum : short {
            One,
            Two,
            Three
        };

        public enum UInt32Enum : uint {
            One,
            Two,
            Three
        };

        public enum UInt64Enum : ulong {
            One,
            Two,
            Three
        };

        public enum SByteEnum : sbyte {
            One,
            Two,
            Three
        };

        public enum UInt16Enum : ushort {
            One,
            Two,
            Three
        };

        public enum ByteEnum : byte {
            One,
            Two,
            Three
        };

        public static Value GetValueClass() {
            return new Value();
        }

        #endregion
    }
}
