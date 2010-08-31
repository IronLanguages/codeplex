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

#if !SILVERLIGHT3
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Scripting.Generation;
using System.Diagnostics;
using System.Reflection.Emit;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    public static partial class Scenarios {
        public class TailCallTest {
            private int _x;

            public TailCallTest(int x) {
                _x = x;
            }

            public int InstanceInc1() {
                CheckCallerNameInV4OrHigher();
                return ++_x;
            }

            // Throw when the stack frame has a method with the specified name. 
            private static void ThrowWhenFrameHasSpecifiedName(StackFrame frame, string name) {
                if (frame.GetMethod().Name == name) {
                    throw new InvalidOperationException("Wrong stack frame is detected.");
                }
            }

            // The method is used to verify that tail call works in .NET 4.0 or higher but not 3.5.
            // It checks if the name of its caller's caller is "lambda_method". 
            // If yes, throws in .Net 4.0 or higher.
            // If not, throws in .Net 3.5 or lower.
            public static void CheckCallerNameInV4OrHigher() {
                // Taill call effects can be observed only in .Net 4.0 or higher
                // when the caller is a security transparent dynamic method and the
                // callee is a security transparent non-dynamic method.
                // Get the frame of the dynamic caller, which is two frames above 
                // the current stack frame.
                var stackFrame = new StackFrame(2);
                if (System.Environment.Version.Major >= 4) {
                    // If it is a tail call, we should see a different name than "lambda_method".
                    ThrowWhenFrameHasSpecifiedName(stackFrame, "lambda_method");
                } else {
                    // For .Net 3.5 or lower, no tail call should be observed.
                    EU.Equal(stackFrame.GetMethod().Name, "lambda_method");
                }
            }

            // The method is used to verify that tail call works.
            // It checks if the name of its caller's caller is "lambda_method". 
            public static void CheckCallerName() {
                // This method should be called in the method that is supposed to be a tail call in
                // a lambda expression. So the lambda method is two stack frames above the
                // current stack frame.
                var stackFrame = new StackFrame(2);
                // If tail call is observed, the stack frame for the lambda expression should
                // not be present, so the name for the method on that frame needs to be different
                // than "lambda_method".
                ThrowWhenFrameHasSpecifiedName(stackFrame, "lambda_method");
            }

        }

        public class TestOperatorOverloading {
            public static long ConvertFromIntToLong(int x) {
                return (long)x;
            }

            public int _int;
            public bool _bool;

            public TestOperatorOverloading(int i) {
                _int = i;
                _bool = false;
            }

            public TestOperatorOverloading(bool b) {
                _int = 0;
                _bool = b;
            }

            // The operator overloading methods below should only be called in lambda expressions without a custom name.
            public static TestOperatorOverloading operator +(TestOperatorOverloading x, TestOperatorOverloading y) {
                TailCallTest.CheckCallerNameInV4OrHigher();
                return new TestOperatorOverloading(x._int + y._int);
            }

            public static TestOperatorOverloading operator ++(TestOperatorOverloading x) {
                TailCallTest.CheckCallerNameInV4OrHigher();
                return new TestOperatorOverloading(x._int + 1);
            }

            public static TestOperatorOverloading operator &(TestOperatorOverloading x, TestOperatorOverloading y) {
                TailCallTest.CheckCallerNameInV4OrHigher();
                return new TestOperatorOverloading(x._bool & y._bool);
            }

            public static TestOperatorOverloading operator |(TestOperatorOverloading x, TestOperatorOverloading y) {
                TailCallTest.CheckCallerNameInV4OrHigher();
                return new TestOperatorOverloading(x._bool | y._bool);
            }

            public static bool operator true(TestOperatorOverloading a) {
                return a._bool;
            }

            public static bool operator false(TestOperatorOverloading a) {
                return !a._bool;
            }

        }

        public struct StructTailCallTest {
            public int X;

            public StructTailCallTest(int x) {
                X = x;
            }

            public int InstanceInc1() {
                // No tail call should be observed for the method since
                // it is defined in a value type.
                var stackFrame = new StackFrame(1);
                // Verify that the caller frame is for the lambda expression.
                EU.Equal(stackFrame.GetMethod().Name, "lambda_method");

                return ++X;
            }
        }

        // The method returns a dynamic method that throws exception when its caller
        // has the name "lambda_method", which is the method name for a lambda expression
        // when no custom name is provided.
        // This method is used to verify that tail call to this method does destroy the
        // stack frame for the caller. So if the call to the result MethodInfo is a tail call,
        // there should be no exception.
        private static MethodInfo GetDynamicMethodForTailCallTest() {
            return GetDynamicMethodForTailCallTest(false);
        }

        // When hasReturn is true, the result dynamic method returns a string value "hello".
        // The behavior is purely for testing purposes.
        private static MethodInfo GetDynamicMethodForTailCallTest(bool hasReturn) {
            var returnType = hasReturn ? typeof(string) : typeof(void);
            var dynamicMethod = new DynamicMethod("DynamicMethod", returnType, new Type[] { });
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Call, typeof(TailCallTest).GetMethod("CheckCallerName"));
            if (hasReturn) {
                il.Emit(OpCodes.Ldstr, "hello"); //return a string
            }
            il.Emit(OpCodes.Ret);

            return (MethodInfo)dynamicMethod;
        }

        public static void Positive_TestTailMethodCallInLambda(EU.IValidator V) {
            //\ -> Foo(x)
            //
            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi); ;
            var le = Expression.Lambda<Action>(
                call,
                true //tail call
            );
            var f = le.Compile();
            f();
        }

        public static void Positive_TestTailCallBlock(EU.IValidator V) {
            //produces \ -> 1; Foo()
            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi); ;
            var body = Expression.Block(
                Expression.Constant(1),
                call
            );

            var le = Expression.Lambda<Action>(
                body,
                true
            );
            var f = le.Compile();
            f();
        }

        public static void Positive_TestTailCallConditional(EU.IValidator V) {
            //produces \x -> if x == 1 ? Foo() : Foo()
            //conditional tail calls
            //tail prefix is emitted for both calls
            var x = Expression.Parameter(typeof(int), "x");
            var testX = Expression.Equal(x, Expression.Constant(1, typeof(int)));
            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi); ;

            var body = Expression.Condition(testX, call, call);
            var le = Expression.Lambda<Action<int>>(
                body, 
                true,
                x
            );
            V.Validate(le, f =>
            {
                f(1);
                f(100);
            });
        }

        public static void Positive_TestTailCallConditional2(EU.IValidator V) {
            //produces \x -> if x == 1 ? (if x == 1 ? Foo() : Foo()) : (if x == 1 ? Foo() : Foo())
            var x = Expression.Parameter(typeof(int), "x");
            var testX = Expression.Equal(x, Expression.Constant(1, typeof(int)));
            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi); ;

            var cond = Expression.Condition(testX, call, call);
            var body = Expression.Condition(testX, cond, cond);
            var le = Expression.Lambda<Action<int>>(
                body, 
                true,
                x
            );

            V.Validate(le, f =>
            {
                f(1);
                f(100);
            });
        }

        public static void Positive_TestTailCallReturn(EU.IValidator V) {
            //\ -> return Foo()
            //
            var mi = GetDynamicMethodForTailCallTest(true);
            var call = Expression.Call(mi); ;
            var lt = Expression.Label(typeof(string));
            var ret = Expression.Return(lt, call, typeof(string));

            var le = Expression.Lambda<Func<string>>(
                Expression.Label(lt, ret),
                true
            );

            var f = le.Compile();
            f();
        }

        public static void Positive_TestTailCallGotoInBlock(EU.IValidator V) {
            //Tail call is in a goto which is not at the end of lambda

            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi); ;

            var endLabel = Expression.Label("End");

            var block = Expression.Block(
                Expression.Constant(1),
                Expression.Goto(endLabel, call),
                Expression.Label(endLabel)
            );

            var le = Expression.Lambda<Action>(
                block,
                true
            );

            var f = le.Compile();
            f();
        }

        public static void Positive_TestTailCallGotoEndLabelNotTheLast(EU.IValidator V) {
            //The end label syntactically is not the last expression,
            //but followed by empty and debug info only.
            //tail call should be emitted.

            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi); ;

            var endLabel = Expression.Label("End");

            var document = Expression.SymbolDocument("Foo.cs");

            var block = Expression.Block(
                Expression.Goto(endLabel, call),
                Expression.DebugInfo(document, 22, 1, 23, 100),
                Expression.Label(endLabel),
                Expression.Block(
                    new[] { Expression.Parameter(typeof(int), "p") },
                    Expression.Empty(),
                    Expression.Empty()
                ),
                Expression.ClearDebugInfo(document)
            );

            var le = Expression.Lambda<Action>(
                block,
                true
            );
            var f = le.Compile();
            f();
        }

        public static void Positive_TestTailCallGotoEndLabelAfterCall(EU.IValidator V) {
            // \ -> Foo(); Goto_end; ...
            // A goto to end label follows a call, the call is emitted as a tail call.
            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi); ;

            var endLabel = Expression.Label("End");

            var document = Expression.SymbolDocument("Foo.cs");

            var block = Expression.Block(
                call,
                Expression.Goto(endLabel),
                Expression.DebugInfo(document, 22, 1, 23, 100),
                Expression.Label(endLabel),
                Expression.ClearDebugInfo(document)
            );

            var le = Expression.Lambda<Action>(
                block,
                true
            );

            var f = le.Compile();
            f();
        }


        public static void Positive_TestTailCallGotoEndLabelWithEmptyValueAfterCall(EU.IValidator V) {
            // \ -> Foo; Goto_end; ...
            // The goto has an empty value, tail call is emitted.
            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi); ;

            var endLabel = Expression.Label("End");

            var block = Expression.Block(
                call,
                Expression.Goto(endLabel, Expression.Empty()),
                Expression.Label(endLabel)
            );

            var le = Expression.Lambda<Action>(
                block,
                true
            );

            var f = le.Compile();
            f();
        }

        public static void Negative_TestTailCallGotoEndLabelWithNonEmptyValueAfterCall(EU.IValidator V) {
            // 707968: Fails on 64-bit
            if (IntPtr.Size == 8) {
                return;
            }

            // \ -> Foo(); return ""; ...
            // The goto has an non-empty value, no tail call is emitted.
            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi);

            var endLabel = Expression.Label(typeof(string), "End");

            var block = Expression.Block(
                call,
                Expression.Label(
                    endLabel, 
                    Expression.Return(endLabel, Expression.Constant(""), typeof(string))
                )
            );

            var le = Expression.Lambda<Func<string>>(
                block,
                true
            );

            var f = le.Compile();
            EU.Throws<InvalidOperationException>(() => f());
        }

        public static void Negative_TestTailCallGotoNonEndLabel(EU.IValidator V) {
            // 707968: Fails on 64-bit
            if (IntPtr.Size == 8) {
                return;
            }

            // The method call followed by a goto to a non-ending label is not
            // emitted as tail call, even the label is unconditionally jumping
            // to the end label.
            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi); ;

            var endLabel = Expression.Label("End");
            var label = Expression.Label("Jump_To_End");

            var block = Expression.Block(
                call,
                Expression.Goto(label),
                Expression.Label(label, Expression.Goto(endLabel)),
                Expression.Label(endLabel)
            );

            var le = Expression.Lambda<Action>(
                block,
                true
            );

            var f = le.Compile();
            EU.Throws<InvalidOperationException>(() => f());
        }

        public static void Positive_TestTailCallGotoInSwitch(EU.IValidator V) {
            //Tail call is in a goto which is not at the end of lambda
            var switchValue = Expression.Parameter(typeof(int), "switchValue");
            var mi = GetDynamicMethodForTailCallTest(true);
            var call = Expression.Call(mi);

            var le = Expression.Lambda<Func<int, string>>(
                Expression.Switch(
                    switchValue,
                    Expression.Constant(""),
                    Expression.SwitchCase(call, Expression.Constant(1)),
                    Expression.SwitchCase(call, Expression.Constant(2))
                ),
                true,
                switchValue
            );

            var f = le.Compile();
            f(0);
            f(1);
            f(2);
            f(3);
        }

        public static void Negative_TestTailCallConvertingResult(EU.IValidator V) {
            //\ -> (object)Foo()
            //A castclass is emitted before ret, no tail prefix is emitted
            var mi = GetDynamicMethodForTailCallTest(true);
            var call = Expression.Call(mi);
            var convert = Expression.Convert(call, typeof(object));
            var le = Expression.Lambda<Func<object>>(
                convert,
                true
            );

            var f = le.Compile();
            EU.Throws<InvalidOperationException>(() => f());
        }


        public static void Positive_TestTailCallConvertingResultsToSameType(EU.IValidator V) {
            //produces \ -> (string)Foo()
            //Converting the result of a call to the same type won't emit additional IL
            //so tail prefix is emitted
            var mi = GetDynamicMethodForTailCallTest(true);
            var call = Expression.Call(mi);
            var convert = Expression.Convert(call, typeof(string));
            var le = Expression.Lambda<Func<string>>(
                convert,
                true
            );

            var f = le.Compile();
            f();
        }


        // Create a dynamic method for converting an int to long.
        // If the caller to this method is not a lambda method, it throws an exception.
        private static MethodInfo GetDynamicMethodForTailCallConvertFromIntToLong() {
            var dynamicMethod = new DynamicMethod("DynamicConversion", typeof(long), new Type[] { typeof(int) });
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Call, typeof(TailCallTest).GetMethod("CheckCallerName"));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Ret);

            return (MethodInfo)dynamicMethod;
        }

        public static void Positive_TestTailCallConversionOverload(EU.IValidator V) {
            //Converting operation with a method 
            //A tail prefix is emitted before the converting method is called.
            var x = Expression.Parameter(typeof(int), "x");
            var mi = GetDynamicMethodForTailCallConvertFromIntToLong();
            var convert = Expression.Convert(
                x,
                typeof(long), 
                mi
            );
            var le = Expression.Lambda<Func<int, long>>(
                convert,
                true,
                x
            );

            V.Validate(le, f =>
            {
                EU.Equal(f(100), (long)100);
            });
        }

        public static void Positive_TestTailCallBinaryOverloading(EU.IValidator V) {
            //produces \(x, y) => Plus_op(x, y) 
            //
            var x = Expression.Parameter(typeof(TestOperatorOverloading), "x");
            var y = Expression.Parameter(typeof(TestOperatorOverloading), "y");

            var le = Expression.Lambda<Func<TestOperatorOverloading, TestOperatorOverloading, TestOperatorOverloading>>(
                Expression.Add(x, y),
                true,
                x, y
            );

            V.Validate(le, f =>
            {
                EU.Equal(f(new TestOperatorOverloading(100), new TestOperatorOverloading(200))._int, 300);
            });
        }

        public static void Positive_TestTailCallUnaryOverloading(EU.IValidator V) {
            //produces \(x) => Inc_op(x) 
            //
            var x = Expression.Parameter(typeof(TestOperatorOverloading), "x");

            var le = Expression.Lambda<Func<TestOperatorOverloading, TestOperatorOverloading>>(
                Expression.Increment(x),
                true,
                x
            );

            V.Validate(le, f =>
            {
                EU.Equal(f(new TestOperatorOverloading(100))._int, 101);
            });
        }

        public static void Positive_TestTailCall1AndAlsoOverloading(EU.IValidator V) {
            //produces \(x, y) => AndAlso_op(x, y) 
            //
            var mi = typeof(TestOperatorOverloading).GetMethod("AndAlso");
            var x = Expression.Parameter(typeof(TestOperatorOverloading), "x");
            var y = Expression.Parameter(typeof(TestOperatorOverloading), "y");

            var le = Expression.Lambda<Func<TestOperatorOverloading, TestOperatorOverloading, TestOperatorOverloading>>(
                Expression.AndAlso(x, y),
                true,
                x, y
            );

            V.Validate(le, f =>
            {
                var @true = new TestOperatorOverloading(true);
                var @false = new TestOperatorOverloading(false);
                EU.Equal(f(@true, @true)._bool, true);
                EU.Equal(f(@true, @false)._bool, false);
                EU.Equal(f(@false, @true)._bool, false);
                EU.Equal(f(@false, @false)._bool, false);
            });
        }


        public static void Positive_TestTailCallOrElseOverloading(EU.IValidator V) {
            //produces \(x, y) => OrElse_op(x, y) 
            //
            var mi = typeof(TestOperatorOverloading).GetMethod("OrElse");
            var x = Expression.Parameter(typeof(TestOperatorOverloading), "x");
            var y = Expression.Parameter(typeof(TestOperatorOverloading), "y");

            var le = Expression.Lambda<Func<TestOperatorOverloading, TestOperatorOverloading, TestOperatorOverloading>>(
                Expression.OrElse(x, y, mi),
                true,
                x, y
            );

            V.Validate(le, f =>
            {
                var @true = new TestOperatorOverloading(true);
                var @false = new TestOperatorOverloading(false);
                EU.Equal(f(@true, @true)._bool, true);
                EU.Equal(f(@true, @false)._bool, true);
                EU.Equal(f(@false, @true)._bool, true);
                EU.Equal(f(@false, @false)._bool, false);
            });
        }

        public static void Positive_TestTailCall1InvocationInlineLambda(EU.IValidator V) {
            //produces \-> Invoke(\-> Foo())
            //
            //The lambda to invoke is compled inline with tail call emitted.
            var mi = GetDynamicMethodForTailCallTest();
            var call = Expression.Call(mi);
            var inlineLambda = Expression.Lambda<Action>(
                call,
                true
            );

            var le = Expression.Lambda<Action>(
                Expression.Invoke(inlineLambda),
                true
            );

            var f = le.Compile();
            f();
        }

        // Create a dynamic method that takes a byref parameter.
        private static MethodInfo GetDynamicMethodByRefTailCall() {
            var dynamicMethod = new DynamicMethod("DynamicConversion", typeof(int), new Type[] { typeof(int).MakeByRefType() });
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Call, typeof(TailCallTest).GetMethod("CheckCallerName"));
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            return (MethodInfo)dynamicMethod;
        }

        public static void Negative_TestTailCallByRefCall(EU.IValidator V) {
            //Tail prefix cannot be emitted for method calls with by ref parameters

            var x = Expression.Parameter(typeof(int), "x");
            var mi = GetDynamicMethodByRefTailCall();
            var call = Expression.Call(mi, x);

            var le = Expression.Lambda<Func<int, int>>(
                call,
                true,
                x
            );

            var f = le.Compile();
            EU.Throws<InvalidOperationException>(() => f(100));
        }

        public static void Positive_TestTailCall1ForClassInstanceMethodCall(EU.IValidator V) {
            var x = Expression.Parameter(typeof(TailCallTest), "x");
            var call = Expression.Call(x, typeof(TailCallTest).GetMethod("InstanceInc1"));

            var le = Expression.Lambda<Func<TailCallTest, int>>(
                call,
                true,
                x
            );

            V.Validate(le, f =>
            {
                EU.Equal(f(new TailCallTest(100)), 101);
            });
        }

        public static void Negative_TestTailCall1ForStructInstanceMethodCall(EU.IValidator V) {
            //If the instance has a value type, no tail call can be emitted since we load
            //the instance's address so the stack cannot be destroyed.
            var x = Expression.Parameter(typeof(StructTailCallTest), "x");
            var call = Expression.Call(x, typeof(StructTailCallTest).GetMethod("InstanceInc1"));

            var le = Expression.Lambda<Func<StructTailCallTest, int>>(
                call,
                true,
                x
            );

            V.Validate(le, f =>
            {
                EU.Equal(f(new StructTailCallTest(100)), 101);
            });
        }

        public static void Negative_TestTailCallInTry(EU.IValidator V) {
            // 707968: Fails on 64-bit
            if (IntPtr.Size == 8) {
                return;
            }

            //The tail. call (or calli or callvirt) instruction cannot be used to transfer control out of a try
            //catch, filter, or finally. No tail call will be emitted for this test.

            var call = Expression.Call(GetDynamicMethodForTailCallTest());

            var tryExpr = Expression.TryCatchFinally(
                call, //try body
                call, //finally body
                Expression.Catch(Expression.Parameter(typeof(Exception), "e"), call)
            );
            
            var le = Expression.Lambda<Action>(
                tryExpr,
                true
            );

            var f = le.Compile();
            EU.Throws<InvalidOperationException>(() => f());
        }
    }
}
#endif
