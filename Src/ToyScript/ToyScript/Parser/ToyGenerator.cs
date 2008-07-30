/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using ToyScript.Binders;
using ToyScript.Parser.Ast;
using MSAst = System.Linq.Expressions;

namespace ToyScript.Parser {
    class ToyGenerator {
        private readonly ToyLanguageContext _tlc;
        private ToyScope _scope;

        private ToyGenerator(ToyLanguageContext tlc, SourceUnit sourceUnit) {
            _tlc = tlc;
            PushNewScope(sourceUnit.Path ?? "<toyblock>", MSAst.Expression.Annotate(sourceUnit.Information));
        }

        internal ToyLanguageContext Tlc {
            get { return _tlc; }
        }

        internal ToyScope Scope {
            get {
                return _scope;
            }
        }

        internal ToyScope PushNewScope(string name, MSAst.Annotations annotations) {
            return _scope = new ToyScope(_scope, name, annotations);
        }

        internal void PopScope() {
            _scope = _scope.Parent;
        }

        internal MSAst.Expression LookupName(string name) {
            return _scope.LookupName(name);
        }

        internal MSAst.Expression GetOrMakeLocal(string name) {
            return _scope.GetOrMakeLocal(name);
        }

        internal MSAst.Expression GetOrMakeGlobal(string name) {
            return _scope.TopScope.GetOrMakeLocal(name);
        }

        internal static MSAst.LambdaExpression Generate(ToyLanguageContext tlc, Statement statement, SourceUnit sourceUnit) {
            ToyGenerator tg = new ToyGenerator(tlc, sourceUnit);

            MSAst.Expression body = statement.Generate(tg);

            return tg.Scope.FinishScope(body);
        }

        internal static bool UseNewBinders = false;

        internal MSAst.Expression ConvertTo(Type type, MSAst.Expression expression) {
            if (UseNewBinders) {
                return MSAst.Expression.Convert(expression, type, Binder.Convert(type), MSAst.Annotations.Empty);
            } else {
                return Utils.ConvertTo(
                    _tlc.Binder,
                    typeof(bool),
                    ConversionResultKind.ExplicitCast,
                    Utils.CodeContext(),
                    expression
                );
            }
        }

        internal MSAst.Expression Call(MSAst.Expression target, MSAst.Expression[] arguments) {
            if (UseNewBinders) {
                return MSAst.Expression.Invoke(
                    MSAst.Annotations.Empty,
                    typeof(object),
                    target,
                    Binder.Call(),
                    arguments
                );
            } else {
                return Utils.Call(
                    _tlc.Binder,
                    typeof(object),
                    ArrayUtils.Insert(
                        Utils.CodeContext(),
                        target,
                        arguments
                   )
                );
            }
        }

        internal MSAst.Expression GetMember(string member, MSAst.Expression target) {
            if (UseNewBinders) {
                return MSAst.Expression.GetMember(
                    target,
                    typeof(object),
                    Binder.GetMember(member)
                );
            } else {
                return Utils.GetMember(
                    _tlc.Binder,
                    member,
                    typeof(object),
                    Utils.CodeContext(),
                    target
                );
            }
        }

        internal MSAst.Expression Operator(Operators op, MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                switch (op) {
                    case Operators.GetItem:
                        return MSAst.Expression.ArrayIndex(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(op));
                    default:
                        throw new NotImplementedException();
                }
            } else {
                return Utils.Operator(
                    _tlc.Binder,
                    op,
                    typeof(object),
                    Utils.CodeContext(),
                    left,
                    right
                );
            }
        }

        internal MSAst.Expression SetItem(MSAst.Expression target, MSAst.Expression index, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.AssignArrayIndex(target, index, right, typeof(object), Binder.Operation(Operators.SetItem), MSAst.Annotations.Empty);
            } else {
                return Utils.Operator(
                    _tlc.Binder,
                    Operators.SetItem,
                    typeof(object),
                    Utils.CodeContext(),
                    target,
                    index,
                    right
                );
            }
        }

        internal MSAst.Expression Add(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.Add(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.Add));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.Add, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression Subtract(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.Subtract(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.Subtract));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.Subtract, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression Multiply(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.Multiply(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.Multiply));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.Multiply, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression Divide(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.Divide(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.Divide));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.Divide, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression LessThan(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.LessThan(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.LessThan));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.LessThan, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression LessThanOrEqual(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.LessThanOrEqual(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.LessThanOrEqual));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.LessThanOrEqual, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression GreaterThan(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.GreaterThan(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.GreaterThan));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.GreaterThan, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression GreaterThanOrEqual(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.GreaterThanOrEqual(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.GreaterThanOrEqual));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.GreaterThanOrEqual, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression Equal(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.Equal(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.Equals));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.Equals, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression NotEqual(MSAst.Expression left, MSAst.Expression right) {
            if (UseNewBinders) {
                return MSAst.Expression.NotEqual(MSAst.Annotations.Empty, left, right, typeof(object), Binder.Operation(Operators.NotEquals));
            } else {
                return Utils.Operator(_tlc.Binder, Operators.NotEquals, typeof(object), Utils.CodeContext(), left, right);
            }
        }

        internal MSAst.Expression New(MSAst.Expression target, MSAst.Expression[] arguments) {
            if (UseNewBinders) {
                return MSAst.Expression.New(typeof(object), Binder.New(), ArrayUtils.Insert(target, arguments));
            } else {
                return Utils.Create(
                    _tlc.Binder,
                    typeof(object),
                    ArrayUtils.Insert(Utils.CodeContext(), target, arguments)
                );
            }
        }
    }
}
