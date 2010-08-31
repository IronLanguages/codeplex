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
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SiteTest {

    public class OnesComplementTemplateTestType {
        public readonly int Version;

        public OnesComplementTemplateTestType(int version) {
            Version = version;
        }

        public static OnesComplementTemplateTestType OnesComplement1(OnesComplementTemplateTestType mt) {
            return new OnesComplementTemplateTestType(100);
        }

        public static OnesComplementTemplateTestType OnesComplement2(OnesComplementTemplateTestType mt) {
            return new OnesComplementTemplateTestType(200);
        }

        public static OnesComplementTemplateTestType OnesComplement3(OnesComplementTemplateTestType mt) {
            return new OnesComplementTemplateTestType(300);
        }
    }

    partial class SiteTestScenarios {

        class OnesComplementTemplateTestBinder : CallSiteBinder {
            public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
                OnesComplementTemplateTestType my = args[0] as OnesComplementTemplateTestType;
                if (my == null) {
                    throw new InvalidOperationException("Bad input");
                }

                switch (my.Version) {
                    case 1:
                    case 2:
                    case 3:
                        break;
                    default:
                        throw new InvalidOperationException("Bad input");
                }

                return Binding(my, parameters[0], returnLabel);
            }

            private Expression Binding(OnesComplementTemplateTestType my, ParameterExpression parameter, LabelTarget returnLabel) {
                // if (arg.GetType() == typeof(OnesComplementTemplateTestType) {
                //     OnesComplementTemplateTestType mt = (OnesComplementTemplateTestType)arg;
                //     if (mt.Version == <version>) {
                //         return OnesComplement<version>(mt);
                //     }
                // }

                ParameterExpression mt = Expression.Variable(typeof(OnesComplementTemplateTestType));
                return Expression.IfThen(
                    Expression.TypeEqual(parameter, typeof(OnesComplementTemplateTestType)),
                    Expression.Block(
                        new[] { mt },
                        Expression.Assign(mt, Expression.Convert(parameter, typeof(OnesComplementTemplateTestType))),
                        Expression.IfThen(
                            Expression.Equal(
                                Expression.Field(mt, typeof(OnesComplementTemplateTestType), "Version"),
                                Expression.Constant(my.Version)
                            ),
                            Expression.Return(
                                returnLabel,
                                Expression.OnesComplement(mt, typeof(OnesComplementTemplateTestType).GetMethod("OnesComplement" + my.Version))
                            )
                        )
                    )
                );
            }
        }

        [Test("Test checking for sanity of templating of ones complement expression.")]
        private void Scenario_TemplateOnesComplement() {
            var binder = new OnesComplementTemplateTestBinder();
            CallSite<Func<CallSite, object, object>> site = CallSite<Func<CallSite, object, object>>.Create(binder);

            OnesComplementTemplateTestType one = new OnesComplementTemplateTestType(1);
            OnesComplementTemplateTestType two = new OnesComplementTemplateTestType(2);
            OnesComplementTemplateTestType thr = new OnesComplementTemplateTestType(3);

            var r_one = site.Target(site, one) as OnesComplementTemplateTestType;
            var r_two = site.Target(site, two) as OnesComplementTemplateTestType;
            var r_thr = site.Target(site, thr) as OnesComplementTemplateTestType;

            Assert.AreEqual(100, r_one.Version);
            Assert.AreEqual(200, r_two.Version);
            Assert.AreEqual(300, r_thr.Version);
        }
    }
}
