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
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Dynamic;
using EU = ETUtils.ExpressionUtils;

namespace AstTest
{
    public static partial class Scenarios
    {
        private class SimpleBinder : CallSiteBinder
        {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
            {
                EU.Equal(args.Length, parameters.Count);
                return Expression.Return(returnLabel, parameters[0]);
            }
        }

        public static void Positive_SimpleSite(EU.IValidator V)
        {
            var site = CallSite<Func<CallSite, int, int>>.Create(new SimpleBinder());
            int x = site.Target(site, 123);
            int y = site.Target(site, 456);

            EU.Equal(x, 123);
            EU.Equal(y, 456);
        }

        private class SimpleVoidBinder : CallSiteBinder
        {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
            {
                return Expression.Return(returnLabel);
            }
        }

        public static void Positive_SimpleVoidBinding(EU.IValidator V)
        {
            var site = CallSite<Action<CallSite, DateTime>>.Create(new SimpleVoidBinder());
            site.Target(site, DateTime.Now);
        }

        public static int CallByRef(ref int a, ref string b, out double c)
        {
            a = 17;
            b = "Called";
            c = Math.PI;
            return 7;
        }

        private class RefBinder : CallSiteBinder
        {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
            {
                return Expression.Return(
                    returnLabel,
                    Expression.Call(typeof(Scenarios).GetMethod("CallByRef"), parameters[0], parameters[1], parameters[2])
                );
            }
        }

        public delegate int CallByRefDelegate(CallSite site, ref int a, ref string b, out double c);

        public static void Positive_RefSites(EU.IValidator V)
        {
            CallSite<CallByRefDelegate> site = CallSite<CallByRefDelegate>.Create(new RefBinder());
            int a = 0;
            string b = null;
            double c = 0.0;

            int result = site.Target(site, ref a, ref b, out c);

            EU.Equal(a, 17);
            EU.Equal(b, "Called");
            EU.Equal(c, Math.PI);
            EU.Equal(result, 7);
        }

        private class RefBinderAll : CallSiteBinder
        {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
            {
                return Expression.Return(
                    returnLabel,
                    Expression.Call(typeof(Scenarios).GetMethod("CallByRefAll"), parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10])
                );
            }
        }

        public static int CallByRefAll(ref int a, ref uint b, ref short c, ref ushort d, ref long e, ref ulong f, ref char g, ref bool h, ref float i, ref double j, ref string k)
        {
            a = 5;
            b = 5;
            c = 5;
            d = 5;
            e = 5;
            f = 5;
            g = 's';
            h = true;
            i = (float)2.5;
            j = (double)2.5;
            k = "yes";
            return 7;
        }

        public delegate int CallByRefDelegateAll(CallSite site, ref int a, ref uint b, ref short c, ref ushort d, ref long e, ref ulong f, ref char g, ref bool h, ref float i, ref double j, ref string k);

        // Pass all value types to binder byref
        public static void Positive_BinderWithRefSiteAllTypes(EU.IValidator V)
        {
            var site = CallSite<CallByRefDelegateAll>.Create(new RefBinderAll());
            int a = 0;
            uint b = 1;
            short c = 1;
            ushort d = 1;
            long e = 1;
            ulong f = 1;
            char g = 'c';
            bool h = false;
            float i = (float)1;
            double j = (double)1;
            string k = "bl";

            int result = site.Target(site, ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref i, ref j, ref k);

            EU.Equal(a, 5);
            EU.Equal(b, (uint)5);
            EU.Equal(c, (short)5);
            EU.Equal(d, (ushort)5);
            EU.Equal(e, (long)5);
            EU.Equal(f, (ulong)5);
            EU.Equal(g, 's');
            EU.Equal(h, true);
            EU.Equal(i, (float)2.5);
            EU.Equal(j, (double)2.5);
            EU.Equal(k, "yes");
            EU.Equal(result, 7);
        }

        private class RefBinderAll2 : CallSiteBinder
        {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
            {
                return Expression.Return(
                    returnLabel,
                    Expression.Call(typeof(Scenarios).GetMethod("CallByRefAll2"))
                );
            }
        }

        public static int CallByRefAll2()
        {
            return 7;
        }

        private class RefBinderAll3 : CallSiteBinder
        {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
            {
                return Expression.Return(
                    returnLabel,
                    Expression.Call(typeof(Scenarios).GetMethod("CallByRefAll3"))
                );
            }
        }

        public static bool callflag;
        public static void CallByRefAll3()
        {
            callflag = true;
        }


        // Verify the update delegates we generate for 0 - 11 args
        public static void Positive_BinderWithManyArgs(EU.IValidator V)
        {

            // do funcs
            var site0 = CallSite<Func<CallSite, int>>.Create(new RefBinderAll2());
            int result = site0.Target(site0);
            EU.Equal(result, 7);

            var site1 = CallSite<Func<CallSite, int, int>>.Create(new RefBinderAll2());
            result = site1.Target(site1, 1);
            EU.Equal(result, 7);

            var site2 = CallSite<Func<CallSite, int, int, int>>.Create(new RefBinderAll2());
            result = site2.Target(site2, 1, 2);
            EU.Equal(result, 7);

            var site3 = CallSite<Func<CallSite, int, int, int, int>>.Create(new RefBinderAll2());
            result = site3.Target(site3, 1, 2, 3);
            EU.Equal(result, 7);

            var site4 = CallSite<Func<CallSite, int, int, int, int, int>>.Create(new RefBinderAll2());
            result = site4.Target(site4, 1, 2, 3, 4);
            EU.Equal(result, 7);

            var site5 = CallSite<Func<CallSite, int, int, int, int, int, int>>.Create(new RefBinderAll2());
            result = site5.Target(site5, 1, 2, 3, 4, 5);
            EU.Equal(result, 7);

            var site6 = CallSite<Func<CallSite, int, int, int, int, int, int, int>>.Create(new RefBinderAll2());
            result = site6.Target(site6, 1, 2, 3, 4, 5, 6);
            EU.Equal(result, 7);

            var site7 = CallSite<Func<CallSite, int, int, int, int, int, int, int, int>>.Create(new RefBinderAll2());
            result = site7.Target(site7, 1, 2, 3, 4, 5, 6, 7);
            EU.Equal(result, 7);

            var site8 = CallSite<Func<CallSite, int, int, int, int, int, int, int, int, int>>.Create(new RefBinderAll2());
            result = site8.Target(site8, 1, 2, 3, 4, 5, 6, 7, 8);
            EU.Equal(result, 7);

            var site9 = CallSite<Func<CallSite, int, int, int, int, int, int, int, int, int, int>>.Create(new RefBinderAll2());
            result = site9.Target(site9, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            EU.Equal(result, 7);

            var site10 = CallSite<Func<CallSite, int, int, int, int, int, int, int, int, int, int, int>>.Create(new RefBinderAll2());
            result = site10.Target(site10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            EU.Equal(result, 7);

            var site11 = CallSite<Func<CallSite, int, int, int, int, int, int, int, int, int, int, int, int>>.Create(new RefBinderAll2());
            result = site11.Target(site11, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
            EU.Equal(result, 7);

            // now do actions
            callflag = false;
            var sub0 = CallSite<Action<CallSite>>.Create(new RefBinderAll3());
            sub0.Target(sub0);
            EU.Equal(callflag, true);

            callflag = false;
            var sub1 = CallSite<Action<CallSite, int>>.Create(new RefBinderAll3());
            sub1.Target(sub1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub2 = CallSite<Action<CallSite, int, int>>.Create(new RefBinderAll3());
            sub2.Target(sub2, 1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub3 = CallSite<Action<CallSite, int, int, int>>.Create(new RefBinderAll3());
            sub3.Target(sub3, 1, 1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub4 = CallSite<Action<CallSite, int, int, int, int>>.Create(new RefBinderAll3());
            sub4.Target(sub4, 1, 1, 1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub5 = CallSite<Action<CallSite, int, int, int, int, int>>.Create(new RefBinderAll3());
            sub5.Target(sub5, 1, 1, 1, 1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub6 = CallSite<Action<CallSite, int, int, int, int, int, int>>.Create(new RefBinderAll3());
            sub6.Target(sub6, 1, 1, 1, 1, 1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub7 = CallSite<Action<CallSite, int, int, int, int, int, int, int>>.Create(new RefBinderAll3());
            sub7.Target(sub7, 1, 1, 1, 1, 1, 1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub8 = CallSite<Action<CallSite, int, int, int, int, int, int, int, int>>.Create(new RefBinderAll3());
            sub8.Target(sub8, 1, 1, 1, 1, 1, 1, 1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub9 = CallSite<Action<CallSite, int, int, int, int, int, int, int, int, int>>.Create(new RefBinderAll3());
            sub9.Target(sub9, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub10 = CallSite<Action<CallSite, int, int, int, int, int, int, int, int, int, int>>.Create(new RefBinderAll3());
            sub10.Target(sub10, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            EU.Equal(callflag, true);

            callflag = false;
            var sub11 = CallSite<Action<CallSite, int, int, int, int, int, int, int, int, int, int, int>>.Create(new RefBinderAll3());
            sub11.Target(sub11, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            EU.Equal(callflag, true);

        }


        public class VoidCaller
        {
            public string Result;
            private int Count;

            public void CallVoid()
            {
                Result = "Called" + Count++;
            }
        }

        private class VoidBinder : CallSiteBinder
        {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
            {
                return Expression.Return(
                    returnLabel,
                    Expression.Call(
                        Expression.Convert(parameters[0], typeof(VoidCaller)),
                        typeof(VoidCaller).GetMethod("CallVoid")
                    )
                );
            }
        }

        public delegate void CallVoidDelegate3(CallSite site, object a0, object a1, object a2, object a3);
        public delegate void CallVoidDelegate4(CallSite site, object a0, object a1, object a2, object a3, object a4);
        public delegate void CallVoidDelegate5(CallSite site, object a0, object a1, object a2, object a3, object a4, object a5);
        public delegate void CallVoidDelegate6(CallSite site, object a0, object a1, object a2, object a3, object a4, object a5, object a6);
        public delegate void CallVoidDelegate7(CallSite site, object a0, object a1, object a2, object a3, object a4, object a5, object a6, object a7);
        public delegate void CallVoidDelegate8(CallSite site, object a0, object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8);
        public delegate void CallVoidDelegate9(CallSite site, object a0, object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9);
        public delegate void CallVoidDelegate10(CallSite site, object a0, object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9, object a10);

        public static void Positive_VoidSites(EU.IValidator V)
        {
            CallSite<CallVoidDelegate3> site3 = CallSite<CallVoidDelegate3>.Create(new VoidBinder());
            CallSite<CallVoidDelegate4> site4 = CallSite<CallVoidDelegate4>.Create(new VoidBinder());
            CallSite<CallVoidDelegate5> site5 = CallSite<CallVoidDelegate5>.Create(new VoidBinder());
            CallSite<CallVoidDelegate6> site6 = CallSite<CallVoidDelegate6>.Create(new VoidBinder());
            CallSite<CallVoidDelegate7> site7 = CallSite<CallVoidDelegate7>.Create(new VoidBinder());
            CallSite<CallVoidDelegate8> site8 = CallSite<CallVoidDelegate8>.Create(new VoidBinder());
            CallSite<CallVoidDelegate9> site9 = CallSite<CallVoidDelegate9>.Create(new VoidBinder());
            CallSite<CallVoidDelegate10> site10 = CallSite<CallVoidDelegate10>.Create(new VoidBinder());

            VoidCaller vc = new VoidCaller();
            site3.Target(site3, vc, null, null, null);
            EU.Equal(vc.Result, "Called0");
            site4.Target(site4, vc, null, null, null, null);
            EU.Equal(vc.Result, "Called1");
            site5.Target(site5, vc, null, null, null, null, null);
            EU.Equal(vc.Result, "Called2");
            site6.Target(site6, vc, null, null, null, null, null, null);
            EU.Equal(vc.Result, "Called3");
            site7.Target(site7, vc, null, null, null, null, null, null, null);
            EU.Equal(vc.Result, "Called4");
            site8.Target(site8, vc, null, null, null, null, null, null, null, null);
            EU.Equal(vc.Result, "Called5");
            site9.Target(site9, vc, null, null, null, null, null, null, null, null, null);
            EU.Equal(vc.Result, "Called6");
            site10.Target(site10, vc, null, null, null, null, null, null, null, null, null, null);
            EU.Equal(vc.Result, "Called7");
        }

        private class BadIDynamicMetaObjectProviderBinder : GetMemberBinder
        {
            internal BadIDynamicMetaObjectProviderBinder()
                : base("Hello", false)
            {
            }
            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }

        private class BadIDynamicMetaObjectProvider : IDynamicMetaObjectProvider
        {
            private int _behavior;

            internal BadIDynamicMetaObjectProvider(int behavior)
            {
                _behavior = behavior;
            }

            public DynamicMetaObject GetMetaObject(Expression parameter)
            {
                switch (_behavior)
                {
                    case 0: // null
                        return null;
                    case 1: // no value
                        return new DynamicMetaObject(parameter, BindingRestrictions.Empty);
                    case 2: // wrong parameter instance
                        return new DynamicMetaObject(Expression.Parameter(parameter.Type), BindingRestrictions.Empty, this);
                    case 3: // null value
                        return new DynamicMetaObject(parameter, BindingRestrictions.Empty, null);
                    case 4: // no value, bad parameter instance
                        return new DynamicMetaObject(Expression.Parameter(parameter.Type), BindingRestrictions.Empty);
                    default:
                        return new BadDynamicMetaObject(parameter, this);
                }
            }

            private class BadDynamicMetaObject : DynamicMetaObject
            {
                internal BadDynamicMetaObject(Expression expression, BadIDynamicMetaObjectProvider value)
                    : base(expression, BindingRestrictions.Empty, value)
                {
                }

                public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
                {
                    return new DynamicMetaObject(
                        Expression,
                        BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                    );
                }
            }
        }

        public static void Negative_BadIDynamicMetaObjectProvider(EU.IValidator V)
        {
            CallSite<Func<CallSite, object, object>> site = CallSite<Func<CallSite, object, object>>.Create(new BadIDynamicMetaObjectProviderBinder());
            for (int behavior = 0; behavior < 5; behavior++)
            {
                EU.Throws<InvalidOperationException>(
                    delegate
                    {
                        site.Target(site, new BadIDynamicMetaObjectProvider(behavior));
                    }
                );
            }

            var bad5 = new BadIDynamicMetaObjectProvider(5);
            var result = site.Target(site, bad5);
            EU.Equal((object)bad5 == (object)result, true);
        }
    }
}
#endif
