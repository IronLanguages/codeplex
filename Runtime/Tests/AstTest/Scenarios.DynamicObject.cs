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
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;
using EU = ETUtils.ExpressionUtils;

namespace AstTest {
    public class TestDynamic : DynamicObject {
        public object[] Call;
        public string Info;

        #region things bound as .NET members

        // TestCreateInstance binder looks for this
        // (it's not an actual CLS operator)
        [SpecialName]
        public TestDynamic CreateInstance() {
            return new TestDynamic { Foo = this.Foo + 111 };
        }

        // Made up thing for TestInvoke to bind to
        public string Invoke(string x, string y) {
            return string.Format("Invoke({0}, {1})", x, y);
        }

        // This property is used by TestDeleteMember and TestDeleteIndex
        public int Deleted;

        public static TestDynamic operator +(TestDynamic x, TestDynamic y) {
            return new TestDynamic { Info = "+" };
        }

        public static TestDynamic operator ++(TestDynamic x) {
            return new TestDynamic { Info = "++" };
        }

        public static explicit operator string(TestDynamic x) {
            return "op_Explicit(TestDynamic):string";
        }

        public int Foo { get; set; }

        private int _itemValue;
        public int this[string s] {
            get {
                if (s == "hello") {
                    return _itemValue;
                } else {
                    throw new KeyNotFoundException();
                }
            }
            set {
                if (s == "hello") {
                    _itemValue = value;
                } else {
                    throw new KeyNotFoundException();
                }
            }
        }

        #endregion

        #region dynamic overloads

        private int _resultCounter;

        private bool NextResult() {
            object result;
            return NextResult(out result);
        }
        private bool NextResult(out object result) {
            result = _resultCounter / 2;
            return (_resultCounter++ % 2) == 1;
        }
        internal void SetNextResult(bool nextResult) {
            _resultCounter = nextResult ? 1 : 0;
        }

        private void AddCall(params object[] args) {
            Call = args;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
            AddCall(binder, arg);
            return NextResult(out result);
        }
        public override bool TryConvert(ConvertBinder binder, out object result) {
            AddCall(binder);
            return NextResult(out result);
        }
        public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result) {
            AddCall(binder, args);
            return NextResult(out result);
        }
        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes) {
            AddCall(binder, indexes);
            return NextResult();
        }
        public override bool TryDeleteMember(DeleteMemberBinder binder) {
            AddCall(binder);
            return NextResult();
        }
        public override bool TryGetIndex(GetIndexBinder binder, object[] args, out object result) {
            AddCall(binder, args);
            return NextResult(out result);
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            AddCall(binder);
            return NextResult(out result);
        }
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
            AddCall(binder, args);
            return NextResult(out result);
        }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            AddCall(binder, args);
            return NextResult(out result);
        }
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            AddCall(binder, indexes, value);
            return NextResult();
        }
        public override bool TrySetMember(SetMemberBinder binder, object value) {
            AddCall(binder, value);
            return NextResult();
        }
        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
            AddCall(binder);
            return NextResult(out result);
        }

        #endregion
    }

    public class TestDynamicDictionary : DynamicObject {
        private readonly Dictionary<string, object> _members = new Dictionary<string, object>();

        public IDictionary<string, object> Members {
            get { return _members; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            return _members.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            _members[binder.Name] = value;
            return true;
        }
    }

    public static class TestPrivateDynamic {
        class PrivateDynamicObject : DynamicObject {
            public override bool TryGetMember(GetMemberBinder binder, out object result) {
                if (binder.Name == "Secret") {
                    result = "SecretValue";
                    return true;
                }
                result = null;
                return false;
            }
        }

        public static object CreateTestObject() {
            return new PrivateDynamicObject();
        }
    }

    public static partial class Scenarios {
        public static void Positive_DynamicObject_Private(EU.IValidator V) {
            var d = TestPrivateDynamic.CreateTestObject();
            var site = CallSite<Func<CallSite, object, object>>.Create(new TestGetMember("Secret"));
            EU.Equal(site.Target(site, d), "SecretValue");
        }

        public static void Negative_DynamicObject_InvokeMemberFallbackInvoke(EU.IValidator V) {
            string result = null;
            var d = new TestDynamicDictionary();
            d.Members["hello"] = new Action(() => result = "world");

            var invokeMember = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeMember("hello"));
            invokeMember.Target(invokeMember, d);
            EU.Equal(result, "world");

            // assign something not callable
            result = null;
            d.Members["hello"] = new TestClass();
            EU.Throws<MissingMemberException>(() => invokeMember.Target(invokeMember, d));
            EU.Equal(result, null);

            // back to callable
            d.Members["hello"] = new Action(() => result = "again");
            invokeMember.Target(invokeMember, d);
            EU.Equal(result, "again");

            // Test that TryInvokeMember cooperates with TryGetMember
            var d2 = new TestDynamicInvokeGetMember();
            var foo = CallSite<Func<CallSite, object, object>>.Create(new TestInvokeMember("foo"));
            var bar = CallSite<Func<CallSite, object, object, object>>.Create(new TestInvokeMember("bar"));
            EU.Equal(foo.Target(foo, d2), "foo!");
            EU.Equal(bar.Target(bar, d2, "123"), "get_bar(123)");
            EU.Equal(bar.Target(bar, d2, "444"), "get_bar(444)");
        }

        public static void Negative_DynamicObject_BinaryOperation(EU.IValidator V) {
            var d1 = new TestDynamic();
            var d2 = new TestDynamic();

            var binaryOp1 = CallSite<Func<CallSite, object, object, object>>.Create(new TestBinaryOperation(ExpressionType.Add));
            EU.Equal(((TestDynamic)binaryOp1.Target(binaryOp1, d1, d2)).Info, "+");
            EU.Equal(d1.Call, null);

            var binder = new TestBinaryOperation(ExpressionType.Subtract);
            var binaryOp2 = CallSite<Func<CallSite, object, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d1, d2));
            EU.Equal(binaryOp2.Target(binaryOp2, d1, d2), 0);
            EU.ArrayEqual(d1.Call, binder, d2);
        }

        public static void Negative_DynamicObject_UnaryOperation(EU.IValidator V) {
            var d = new TestDynamic();

            var binaryOp1 = CallSite<Func<CallSite, object, object>>.Create(new TestUnaryOperation(ExpressionType.Increment));
            EU.Equal(((TestDynamic)binaryOp1.Target(binaryOp1, d)).Info, "++");
            EU.Equal(d.Call, null);

            var binder = new TestUnaryOperation(ExpressionType.Decrement);
            var binaryOp2 = CallSite<Func<CallSite, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d));
            EU.Equal(binaryOp2.Target(binaryOp2, d), 0);
            EU.ArrayEqual(d.Call, binder);
        }

        public static void Negative_DynamicObject_GetMember(EU.IValidator V) {
            var d = new TestDynamic { Foo = 123 };

            var binaryOp1 = CallSite<Func<CallSite, object, object>>.Create(new TestGetMember("Foo"));
            EU.Equal(binaryOp1.Target(binaryOp1, d), 123);
            EU.Equal(d.Call, null);

            var binder = new TestGetMember("Bar");
            var binaryOp2 = CallSite<Func<CallSite, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d));
            EU.Equal(binaryOp2.Target(binaryOp2, d), 0);
            EU.ArrayEqual(d.Call, binder);
        }

        public static void Negative_DynamicObject_SetMember(EU.IValidator V) {
            var d = new TestDynamic { Foo = 123 };

            var binaryOp1 = CallSite<Func<CallSite, object, object, object>>.Create(new TestSetMember("Foo"));
            EU.Equal(binaryOp1.Target(binaryOp1, d, 444), 444);
            EU.Equal(d.Foo, 444);
            EU.Equal(d.Call, null);

            var binder = new TestSetMember("Bar");
            var binaryOp2 = CallSite<Func<CallSite, object, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d, 444));
            EU.Equal(binaryOp2.Target(binaryOp2, d, 444), 444);
            EU.ArrayEqual(d.Call, binder, 444);
        }

        public static void Negative_DynamicObject_DeleteMember(EU.IValidator V) {
            var d = new TestDynamic();
            var binaryOp1 = CallSite<Action<CallSite, object>>.Create(new TestDeleteMember("Foo", true));
            binaryOp1.Target(binaryOp1, d);
            EU.Equal(d.Deleted, 1);
            EU.Equal(d.Call, null);

            var binder = new TestDeleteMember("Bar", false);
            var binaryOp2 = CallSite<Action<CallSite, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d));
            binaryOp2.Target(binaryOp2, d);
            EU.ArrayEqual(d.Call, binder);
        }

        public static void Negative_DynamicObject_Invoke(EU.IValidator V) {
            var d = new TestDynamic();
            var binaryOp1 = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestInvoke());
            EU.Equal(binaryOp1.Target(binaryOp1, d, "7", "8"), "Invoke(7, 8)");
            EU.Equal(d.Call, null);

            var binder = new TestInvoke();
            var binaryOp2 = CallSite<Func<CallSite, object, object, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d, 7, 8));
            EU.Equal(binaryOp2.Target(binaryOp2, d, 7, 8), 0);
            EU.ArrayEqual(d.Call, binder, new object[] { 7, 8 });
        }

        public static void Negative_DynamicObject_InvokeMember(EU.IValidator V) {
            var d = new TestDynamic();
            var binaryOp1 = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestInvokeMember("Invoke"));
            EU.Equal(binaryOp1.Target(binaryOp1, d, "7", "8"), "Invoke(7, 8)");
            EU.Equal(d.Call, null);

            var binder = new TestInvokeMember("Invoke");
            var binaryOp2 = CallSite<Func<CallSite, object, object, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d, 7, 8));
            d.SetNextResult(true);
            EU.Equal(binaryOp2.Target(binaryOp2, d, 7, 8), 0);
            EU.ArrayEqual(d.Call, binder, new object[] { 7, 8 });
        }

        public static void Negative_DynamicObject_CreateInstance(EU.IValidator V) {
            var d = new TestDynamic { Foo = 777 };
            var binaryOp1 = CallSite<Func<CallSite, object, object>>.Create(new TestCreateInstance());
            EU.Equal(((TestDynamic)binaryOp1.Target(binaryOp1, d)).Foo, 888);
            EU.Equal(d.Call, null);

            var binder = new TestCreateInstance();
            var binaryOp2 = CallSite<Func<CallSite, object, object, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d, 7, 8));
            EU.Equal(binaryOp2.Target(binaryOp2, d, 7, 8), 0);
            EU.ArrayEqual(d.Call, binder, new object[] { 7, 8 });
        }

        public static void Negative_DynamicObject_Convert(EU.IValidator V) {
            var d = new TestDynamic();
            var binaryOp1 = CallSite<Func<CallSite, object, string>>.Create(new TestConvert(typeof(string)));
            EU.Equal(binaryOp1.Target(binaryOp1, d), "op_Explicit(TestDynamic):string");
            EU.Equal(d.Call, null);

            var binder = new TestConvert(typeof(int));
            var binaryOp2 = CallSite<Func<CallSite, object, int>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d));
            EU.Equal(binaryOp2.Target(binaryOp2, d), 0);
            EU.ArrayEqual(d.Call, binder);
        }

        public static void Negative_DynamicObject_GetIndex(EU.IValidator V) {
            var d = new TestDynamic();
            d["hello"] = 123;

            var binder = new TestGetIndex();
            var binaryOp1 = CallSite<Func<CallSite, object, object, object>>.Create(binder);
            EU.Equal(binaryOp1.Target(binaryOp1, d, "hello"), 123);
            EU.Equal(d.Call, null);

            var binaryOp2 = CallSite<Func<CallSite, object, object, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d, "hello", "world"));
            EU.Equal(binaryOp2.Target(binaryOp2, d, "hello", "world"), 0);
            EU.ArrayEqual(d.Call, binder, new object[] { "hello", "world" });
        }

        public static void Negative_DynamicObject_SetIndex(EU.IValidator V) {
            var d = new TestDynamic();
            d["hello"] = 123;

            var binder = new TestSetIndex();
            var binaryOp1 = CallSite<Func<CallSite, object, object, object, object>>.Create(binder);
            EU.Equal(binaryOp1.Target(binaryOp1, d, "hello", 444), 444);
            EU.Equal(d["hello"], 444);
            EU.Equal(d.Call, null);

            var binaryOp2 = CallSite<Func<CallSite, object, object, object, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d, "hello", "world", 444));
            EU.Equal(binaryOp2.Target(binaryOp2, d, "hello", "world", 444), 444);
            EU.ArrayEqual(d.Call, binder, new object[] { "hello", "world" }, 444);
        }

        public static void Negative_DynamicObject_DeleteIndex(EU.IValidator V) {
            var d = new TestDynamic();
            var binaryOp1 = CallSite<Action<CallSite, object, object, object>>.Create(new TestDeleteIndex(true));
            binaryOp1.Target(binaryOp1, d, 7, 8);
            EU.Equal(d.Deleted, 1);
            EU.Equal(d.Call, null);

            var binder = new TestDeleteIndex(false);
            var binaryOp2 = CallSite<Action<CallSite, object, object>>.Create(binder);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d, 7));
            binaryOp2.Target(binaryOp2, d, 7);
            EU.ArrayEqual(d.Call, binder, new object[] { 7 });
        }

        public static void Negative_DynamicObject_Errors(EU.IValidator V) {
            var d1 = new TestDynamic();
            var d2 = new TestDynamic();

            // Null for exceptionArgs
            var binder1 = new TestBinaryOperationError_1(ExpressionType.Subtract);
            var binaryOp2 = CallSite<Func<CallSite, object, object, object>>.Create(binder1);
            EU.Throws<MissingMemberException>(() => binaryOp2.Target(binaryOp2, d1, d2));

            d1 = new TestDynamic();
            d2 = new TestDynamic();

            // exception doesn't have valid constructor
            var binder2 = new TestBinaryOperationError_2(ExpressionType.Subtract);
            binaryOp2 = CallSite<Func<CallSite, object, object, object>>.Create(binder2);
            EU.Throws<ArgumentException>(() => binaryOp2.Target(binaryOp2, d1, d2));

            // Verify trivial GetDynamicMemberNames case.
            DynamicMetaObject foo = new DynamicMetaObject(Expression.Empty(), System.Dynamic.BindingRestrictions.Empty);
            string[] s = (string[])foo.GetDynamicMemberNames();
            Assert.Equals(s.Length, 0);

            // Illegal operation
            EU.Throws<ArgumentException>(() => binder1 = new TestBinaryOperationError_1(ExpressionType.Goto));

        }

        public static void Negative_ExpandoObject_InvokeMember(EU.IValidator V) {
            var e = new ExpandoObject();
            IDictionary<string, object> dict = e;
            var site1 = CallSite<Func<CallSite, object, object, object, object>>.Create(new TestInvokeMember("Add"));

            site1.Target(site1, e, "hello", "world");
            EU.Equal(dict.Keys.Count, 1);
            foreach (var p in dict) {
                EU.Equal(p.Key, "hello");
                EU.Equal(p.Value, "world");
            }

            object key = null, value = null;
            dict.Clear();
            dict.Add("Add", new Action<object, object>(
                (k, v) => {
                    key = k;
                    value = v;
                }
            ));

            site1.Target(site1, e, "hi", "there");
            EU.Equal(dict.Count, 1);
            EU.Equal(key, "hi");
            EU.Equal(value, "there");

            dict["Add"] = new object();
            EU.Throws<MissingMemberException>(() => site1.Target(site1, e, "hi", "there"));
        }

        class TestDynamicInvokeGetMember : DynamicObject {
            public override bool TryGetMember(GetMemberBinder binder, out object result) {
                result = new Func<object, string>(x => "get_" + binder.Name + "(" + x + ")");
                return true;
            }
            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
                if (binder.Name == "foo") {
                    result = "foo!";
                    return true;
                }
                result = null;
                return false;
            }
        }

    }

    #region simple .NET binders

    internal static class MetaObjectExtensions {
        internal static DynamicMetaObject Limit(this DynamicMetaObject self) {
            if (self.HasValue && self.Value == null) {
                return LimitInstance(self);
            }
            return new DynamicMetaObject(
                Expression.Convert(self.Expression, self.LimitType),
                self.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(self.Expression, self.LimitType)),
                self.Value
            );
        }
        internal static DynamicMetaObject[] Limit(this DynamicMetaObject[] self) {
#if SILVERLIGHT
            List<DynamicMetaObject> newList = new List<DynamicMetaObject>();
            foreach (DynamicMetaObject mo in self)
                newList.Add(mo.Limit());
            return newList.ToArray();
#else
            return Array.ConvertAll(self, (x) => x.Limit());
#endif
        }
        internal static DynamicMetaObject LimitInstance(this DynamicMetaObject self) {
            return new DynamicMetaObject(
                Expression.Convert(self.Expression, self.LimitType),
                self.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(self.Expression, self.Value)),
                self.Value
            );
        }
    }

    internal class TestBinaryOperation : BinaryOperationBinder {
        internal TestBinaryOperation(ExpressionType operation) : base(operation) { }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
            try {
                target = target.Limit();
                arg = arg.Limit();
                return new DynamicMetaObject(
                    Expression.MakeBinary(Operation, target.Expression, arg.Expression),
                    target.Restrictions.Merge(arg.Restrictions)
                );
            } catch {
                return errorSuggestion ?? this.CreateThrow(target, new[] { arg }, typeof(MissingMemberException), "could not bind");
            }
        }
    }

    internal class TestBinaryOperationError_1 : BinaryOperationBinder {
        internal TestBinaryOperationError_1(ExpressionType operation) : base(operation) { }
        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
            // Trivial check of GetDynamicMemberNames()
            string[] s = (string[])target.GetDynamicMemberNames();
            Assert.Equals(s.Length, 0);

            return errorSuggestion ?? this.CreateThrow(target, new[] { arg }, typeof(MissingMemberException), (object[])null);
        }
    }

    internal class NoDefaultConstClass {
        private NoDefaultConstClass() { }
        public NoDefaultConstClass(int a) { }
    }

    internal class TestBinaryOperationError_2 : BinaryOperationBinder {
        internal TestBinaryOperationError_2(ExpressionType operation) : base(operation) { }
        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
            return errorSuggestion ?? this.CreateThrow(target, new[] { arg }, typeof(NoDefaultConstClass), (object[])null);
        }
    }

    internal class TestUnaryOperation : UnaryOperationBinder {
        internal TestUnaryOperation(ExpressionType operation) : base(operation) { }

        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            try {
                target = target.Limit();
                return new DynamicMetaObject(
                    Expression.MakeUnary(Operation, target.Expression, target.Expression.Type),
                    target.Restrictions
                );
            } catch {
                return errorSuggestion ?? this.CreateThrow(target, null, typeof(MissingMemberException), "could not bind");
            }
        }
    }

    internal class TestGetMember : GetMemberBinder {
        internal TestGetMember(string name) : base(name, true) { }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            try {
                target = target.Limit();
                return this.CreateDynamicMetaObject(
                    Expression.PropertyOrField(target.Expression, Name),
                    target.Restrictions
                );
            } catch {
                return errorSuggestion ?? this.CreateThrow(target, null, typeof(MissingMemberException), "could not bind");
            }
        }
    }

    internal class TestSetMember : SetMemberBinder {
        internal TestSetMember(string name) : base(name, true) { }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
            try {
                target = target.Limit();
                value = value.Limit();
                return this.CreateDynamicMetaObject(
                    Expression.Assign(Expression.PropertyOrField(target.Expression, Name), value.Expression),
                    target.Restrictions.Merge(value.Restrictions)
                );
            } catch {
                return errorSuggestion ?? this.CreateThrow(target, new[] { value }, typeof(MissingMemberException), "could not bind");
            }
        }
    }

    internal class TestDeleteMember : DeleteMemberBinder {
        private readonly bool _fake;

        internal TestDeleteMember(string name, bool fake)
            : base(name, true) {
            _fake = fake;
        }

        public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            if (_fake) {
                target = target.Limit();
                return this.CreateDynamicMetaObject(
                    Expression.PreIncrementAssign(Expression.Field(target.Expression, "Deleted")),
                    target.Restrictions
                );
            }
            return errorSuggestion ?? this.CreateThrow(target, null, typeof(MissingMemberException), "could not bind");
        }
    }

    internal class TestConvert : ConvertBinder {
        internal TestConvert(Type type) : base(type, true) { }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            try {
                target = target.Limit();
                return new DynamicMetaObject(Expression.Convert(target.Expression, Type), target.Restrictions);
            } catch {
                return errorSuggestion ?? this.CreateThrow(target, null, typeof(MissingMemberException), "could not bind");
            }
        }
    }

    internal class TestInvokeMember : InvokeMemberBinder {
        internal TestInvokeMember(string name) : base(name, true, new CallInfo(0)) { }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            return new TestInvoke().FallbackInvoke(target, args, errorSuggestion);
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            try {
                target = target.Limit();
                args = args.Limit();

                if (target.Value is ExpandoObject) {
                    target = new DynamicMetaObject(
                        Expression.Convert(target.Expression, typeof(IDictionary<string, object>)),
                        target.Restrictions
                    );
                }

                return this.CreateDynamicMetaObject(
                    Expression.Call(target.Expression, Name, null, DynamicUtils.GetExpressions(args)),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args))
                );
            } catch {
                return errorSuggestion ?? this.CreateThrow(target, args, typeof(MissingMemberException), "could not bind");
            }
        }
    }

    internal class TestInvoke : InvokeBinder {
        public TestInvoke() :
            base(new CallInfo(0)) {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            if (!target.HasValue) {
                return Defer(target, args);
            }
            try {
                target = target.Limit();
                args = args.Limit();

                return this.CreateDynamicMetaObject(
                    Expression.Call(target.Expression, "Invoke", null, DynamicUtils.GetExpressions(args)),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args))
                );
            } catch {
                return errorSuggestion ?? this.CreateThrow(target, args, typeof(MissingMemberException), "could not bind");
            }
        }
    }

    internal class TestCreateInstance : CreateInstanceBinder {
        public TestCreateInstance() :
            base(new CallInfo(0)) {
        }

        public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
            try {
                target = target.Limit();
                args = args.Limit();

                var call = Expression.Call(target.Expression, "CreateInstance", null, DynamicUtils.GetExpressions(args));
                if (call.Method.IsSpecialName) {
                    return new DynamicMetaObject(call, target.Restrictions.Merge(BindingRestrictions.Combine(args)));
                }
            } catch { }
            return errorSuggestion ?? this.CreateThrow(target, args, typeof(MissingMemberException), "could not bind");
        }
    }

    internal class TestGetIndex : GetIndexBinder {
        public TestGetIndex() :
            base(new CallInfo(0)) {
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion) {
            try {
                target = target.Limit();
                indexes = indexes.Limit();
                return this.CreateDynamicMetaObject(
                    Expression.Property(target.Expression, target.LimitType.GetProperty("Item"), DynamicUtils.GetExpressions(indexes)),
                    target.Restrictions.Merge(BindingRestrictions.Combine(indexes))
                );
            } catch {
                return errorSuggestion ?? this.CreateThrow(target, indexes, typeof(MissingMemberException), "could not bind");
            }
        }
    }

    internal class TestSetIndex : SetIndexBinder {
        public TestSetIndex() :
            base(new CallInfo(0)) {
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
            try {
                target = target.Limit();
                indexes = indexes.Limit();
                value = value.Limit();
                return this.CreateDynamicMetaObject(
                    Expression.Assign(Expression.Property(target.Expression, target.LimitType.GetProperty("Item"), DynamicUtils.GetExpressions(indexes)), value.Expression),
                    target.Restrictions.Merge(BindingRestrictions.Combine(indexes)).Merge(value.Restrictions)
                );
            } catch {
                var args = new DynamicMetaObject[indexes.Length + 1];
                indexes.CopyTo(args, 0);
                args[indexes.Length] = value;
                return errorSuggestion ?? this.CreateThrow(target, args, typeof(MissingMemberException), "could not bind");
            }
        }
    }

    internal class TestDeleteIndex : DeleteIndexBinder {
        private readonly bool _fake;

        internal TestDeleteIndex(bool fake) :
            base(new CallInfo(0)) {
            _fake = fake;
        }

        public override DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion) {
            if (_fake) {
                target = target.Limit();
                return this.CreateDynamicMetaObject(
                    Expression.PreIncrementAssign(
                        Expression.Field(target.Expression, "Deleted")
                    ),
                    target.Restrictions.Merge(BindingRestrictions.Combine(indexes))
                );
            }
            return errorSuggestion ?? this.CreateThrow(target, indexes, typeof(MissingMemberException), "could not bind");
        }
    }

    #endregion
}
#endif
