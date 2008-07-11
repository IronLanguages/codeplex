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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Utils;

namespace System.Scripting.Actions {
    public class Restrictions {
        private class Restriction {
            internal enum RestrictionKind {
                Type,
                Instance,
                Custom
            };

            private readonly RestrictionKind _kind;

            // Simplification ... for now just one kind of restriction rather than hierarchy.
            private readonly Expression/*!*/ _expression;
            private readonly Type _type;
            private readonly object _instance;      // TODO: WeakRef ???

            internal RestrictionKind Kind {
                get { return _kind; }
            }

            internal Expression Expression {
                get { return _expression; }
            }

            internal Type Type {
                get { return _type; }
            }

            internal object Instance {
                get { return _instance; }
            }

            internal Restriction(Expression parameter, Type type) {
                _kind = RestrictionKind.Type;
                _expression = parameter;
                _type = type;
            }

            internal Restriction(Expression parameter, object instance) {
                _kind = RestrictionKind.Instance;
                _expression = parameter;
                _instance = instance;
            }

            internal Restriction(Expression expression) {
                _kind = RestrictionKind.Custom;
                _expression = expression;
            }

            public override bool Equals(object obj) {
                Restriction other = obj as Restriction;
                if (other == null) {
                    return false;
                }

                if (other.Kind != Kind ||
                    other.Expression != Expression) {
                    return false;
                }

                switch (other.Kind) {
                    case RestrictionKind.Instance:
                        return other.Instance == Instance;
                    case RestrictionKind.Type:
                        return other.Type == Type;
                    default:
                        return false;
                }
            }

            public override int GetHashCode() {
                // lots of collisions but we don't hash Restrictions ever
                return (int)Kind ^ Expression.GetHashCode();
            }
        }

        private readonly Restriction/*!*/[]/*!*/ _restrictions;

        private Restrictions(params Restriction[] restrictions) {
            _restrictions = restrictions;
        }

        private bool IsEmpty {
            get {
                return _restrictions.Length == 0;
            }
        }

        public Restrictions Merge(Restrictions restrictions) {
            if (restrictions.IsEmpty) {
                return this;
            } else if (IsEmpty) {
                return restrictions;
            } else {
                List<Restriction> res = new List<Restriction>(_restrictions.Length + restrictions._restrictions.Length);
                AddRestrictions(_restrictions, res);
                AddRestrictions(restrictions._restrictions, res);

                return new Restrictions(res.ToArray());
            }
        }

        /// <summary>
        /// Adds unique restrictions and doesn't add restrictions which are alerady present
        /// </summary>
        private static void AddRestrictions(Restriction[] list, List<Restriction>/*!*/ res) {
            foreach (Restriction r in list) {
                bool found = false;
                for (int j = 0; j < res.Count; j++) {
                    if (res[j] == r) {
                        found = true;
                    }
                }

                if (!found) {
                    res.Add(r);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static Restrictions Empty = new Restrictions();

        public static Restrictions TypeRestriction(Expression expression, Type type) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            
            if (expression.Type == type && CompilerHelpers.IsSealed(type)) {
                return Restrictions.Empty;
            }

            return new Restrictions(new Restriction(expression, type));
        }

        public static Restrictions InstanceRestriction(Expression expression, object instance) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(instance, "instance");
            return new Restrictions(new Restriction(expression, instance));
        }

        public static Restrictions ExpressionRestriction(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.Requires(expression.Type == typeof(bool), "expression");
            return new Restrictions(new Restriction(expression));
        }

        public static Restrictions/*!*/ Combine(IList<MetaObject> contributingObjects) {
            Restrictions res = Restrictions.Empty;
            if (contributingObjects != null) {                
                foreach (MetaObject mo in contributingObjects) {
                    if (mo != null) {
                        res = res.Merge(mo.Restrictions);
                    }
                }
            }
            return res;
        }

        internal Expression CreateTest() {
            // TODO: Currently totally unoptimized and unordered
            Expression test = null;
            foreach (Restriction r in _restrictions) {
                Expression one;
                switch (r.Kind) {
                    case Restriction.RestrictionKind.Type:
                        one = CreateTypeRestriction(r.Expression, r.Type);
                        break;
                    case Restriction.RestrictionKind.Instance:
                        one = CreateInstanceRestriction(r.Expression, r.Instance);
                        break;
                    case Restriction.RestrictionKind.Custom:
                        one = r.Expression;
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (one != null) {
                    if (test == null) {
                        test = one;
                    } else {
                        test = Expression.AndAlso(test, one);
                    }
                }
            }

            return test ?? Expression.True();
        }

        /// <summary>
        /// Creates one type identity test 
        /// </summary>
        private static Expression CreateTypeRestriction(Expression expression, Type rt) {
            Type ct = expression.Type;

            if (ct == rt) {
                if (ct.IsValueType) {
                    // No test necessary for value types
                    return null;
                }
                if (ct.IsSealed) {
                    // Sealed type is easy, just check for null
                    return Expression.NotEqual(expression, Expression.Null());
                }
            }

            if (rt == typeof(None)) {
                return Expression.Equal(expression, Expression.Null(expression.Type));
            }

            return Expression.AndAlso(
                Expression.NotEqual(expression, Expression.Null()),
                Expression.Equal(
                    Expression.Call(
                        Expression.ConvertHelper(expression, typeof(object)),
                        typeof(object).GetMethod("GetType")
                    ),
                    Expression.Constant(rt)
                )
            );
        }

        private static Expression CreateInstanceRestriction(Expression expression, object value) {
            return Expression.Equal(expression, Expression.Constant(value));
        }
    }
}
