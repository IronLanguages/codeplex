#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace SiteTest.Actions {
    /// <summary>
    /// DelegateBinder tests binding to delegates, aka pre-compiled rules.
    /// It is intended to work with callsites matching: Func(CallSite, string, string)
    /// Where the string argument is the method to bind to.
    /// </summary>
    class DelegateBinder : CallSiteBinder {
        public override Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            return Expression.Condition(
                Expression.Field(Expression.Constant(this), typeof(DelegateBinder).GetField("BindTest")),
                Expression.Return(
                    returnLabel,
                    Expression.Constant("Bind")
                ),
                Expression.Empty()
            );
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args) {
            string arg0 = (string)args[0];
            switch (arg0) {
                case "Bind":
                    //Trivial case of fallback to Bind
                    return base.BindDelegate<T>(site, args);
                case "Target1":
                    //Bind to a delegate that will work once and then fall back
                    return (T)(object)new Func<CallSite, string, string>(Target1);
                default:
                    throw new Exception("Bad argument to test binder");
            }
        }

        private string Target1(CallSite site, string binding) {
            if (Target1Test)
                return "Target1";
            else
                return ((CallSite<Func<CallSite, string, string>>)site).Update(site, binding);
        }

        public bool BindTest = true;
        public bool Target1Test = true;
    }
}
