/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Ast;
using System.Diagnostics;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// A dynamic test that can invalidate a rule at runtime.  The definition of an
    /// invalid rule is one whose Test will always return false.  In theory a set of
    /// validators is not needed as this could be encoded in the test itself; however,
    /// in practice it is much simpler to include these helpers.
    /// </summary>
    /// <returns>Whether or not the rule should still be considered valid.</returns>
    public delegate bool Validator();

    /// <summary>
    /// A rule is the mechanism that LanguageBinders use to specify both what code to execute (the Target)
    /// for a particular action on a particular set of objects, but also a Test that guards the Target.
    /// Whenver the Test returns true, it is assumed that the Target will be the correct action to
    /// take on the arguments.
        /// 
    /// In the current design, a StandardRule is also used to provide a mini binding scope for the
    /// parameters and temporary variables that might be needed by the Test and Target.  This will
    /// probably change in the future as we unify around the notion of CodeBlocks.
    /// </summary>
    /// <typeparam name="T">The type of delegate for the DynamicSites this rule may apply to.</typeparam>
    public class StandardRule<T> {
        private List<Validator> _validators;
        private Expression _test;
        private Statement _target;

        private RuleSet<T> _ruleSet;

        // TODO revisit these fields and their uses when CodeBlock moves down
        private Variable[] _parameters;
        private List<Variable> _temps;
        private VariableReference[] _references;

        public StandardRule() {
            int firstParameter = DynamicSiteHelpers.IsFastTarget(typeof(T)) ? 1 : 2;
            
            ParameterInfo[] pis = typeof(T).GetMethod("Invoke").GetParameters();
            _parameters = new Variable[pis.Length - firstParameter];
            for (int i = firstParameter; i < pis.Length; i++) {
                _parameters[i - firstParameter] = MakeParameter(i, "$arg" + (i - firstParameter), pis[i].ParameterType);
            }
        }

        /// <summary>
        /// The code to execute if the Test is true.
        /// </summary>
        public Statement Target {
            get { return _target; }
        }

        /// <summary>
        /// An expression that should return true iff Target should be executed
        /// </summary>
        public Expression Test {
            get { return _test; }
        }

        public Variable[] Parameters {
            get { return _parameters; }
        }

        private Variable MakeParameter(int index, string name, Type type) {
            Variable ret = Variable.Parameter(null, SymbolTable.StringToId(name), type);
            ret.ParameterIndex = index;
            return ret;
        }

        public Expression GetParameterExpression(int index) {
            return BoundExpression.Defined(_parameters[index]);
        }

        public Expression[] GetParameterExpressions() {
            Expression[] parameters = new Expression[Parameters.Length];
            for (int i = 0; i < parameters.Length; i++) parameters[i] = GetParameterExpression(i);
            return parameters;
        }

        public Variable GetTemporary(Type type, string name) {
            if (_temps == null) {
                _temps = new List<Variable>();
            }
            Variable ret = Variable.Temporary(SymbolTable.StringToId(name), null, type);
            _temps.Add(ret);
            return ret;
        }

        public void SetTest(Expression test) {
            if (_test != null) throw new InvalidOperationException();
            _test = test;
        }

        public void SetTarget(Statement target) {
            if (_target != null) throw new InvalidOperationException();
            _target = target;
        }

        public void AddValidator(Validator validator) {
            if (_validators == null) _validators = new List<Validator>();
            _validators.Add(validator);
        }

        /// <summary>
        /// Each rule holds onto an immutable RuleSet that contains this rule only.
        /// This should heavily optimize monomorphic call sites.
        /// </summary>
        internal RuleSet<T> MonomorphicRuleSet {
            get {
                if (_ruleSet == null) {
                    _ruleSet = new SmallRuleSet<T>(new StandardRule<T>[] { this });
                }
                return _ruleSet;
            }
        }

        /// <summary>
        /// If not valid, this indicates that the given Test can never return true and therefore
        /// this rule should be removed from any RuleSets when convenient in order to 
        /// reduce memory usage and the number of active rules.
        /// </summary>
        public bool IsValid {
            get {
                if (_validators == null) return true;

                foreach (Validator v in _validators) {
                    if (!v()) return false;
                }
                return true;
            }
        }

        public void Emit(CodeGen cg, Label ifFalse) {
            // Need to make sure we aren't generating into two different CodeGens at the same time
            lock (this) {
                // First, finish binding my variable references
                if (_references == null) {
                    _references = RuleBinder.Bind(_test, _target, _parameters, _temps);
                }

                foreach (VariableReference vr in _references) {
                    vr.CreateSlot(cg);
                }

                if (_test != null) {
                    _test.EmitBranchFalse(cg, ifFalse);
                }

                // Now do the generation
                _target.Emit(cg);

                // free any temps now that we're done generating
                // TODO: Keep temp slots aside sot that they can be freed
                //if (_temps != null) {
                //    foreach (Variable vr in _temps) {
                //        cg.FreeLocalTmp(vr.Slot);
                //    }
                //}
            }
        }


        public override string ToString() {
            return string.Format("StandardRule({0})", _target);
        }

        public Statement MakeReturn(ActionBinder binder, Expression expr) {
            // we create a temporary here so that ConvertExpression doesn't need to (because it has no way to declare locals).
            if (expr.ExpressionType != typeof(void)) {
                Variable variable = GetTemporary(expr.ExpressionType, "$retVal");
                return new ReturnStatement(
                    new CommaExpression(
                        new Expression[] {
                            BoundAssignment.Assign(variable, expr),
                            binder.ConvertExpression(BoundExpression.Defined(variable), typeof(T).GetMethod("Invoke").ReturnType)
                        },
                        1
                    )
                );
            }
            return new ReturnStatement(binder.ConvertExpression(expr, typeof(T).GetMethod("Invoke").ReturnType));
        }

        public void MakeTest(params Type[] types) {
            DynamicType[] dts = new DynamicType[types.Length];
            for (int i = 0; i < types.Length; i++) {
                dts[i] = DynamicHelpers.GetDynamicTypeFromType(types[i]);
            }

            MakeTest(dts);
        }

        /// <summary>
        /// This helper will generate the appropriate tests for an exact match with
        /// all of the specified DynamicTypes.
        /// </summary>
        /// <param name="types"></param>
        public void MakeTest(params DynamicType[] types) {
            _test = MakeTestForTypes(types, 0);
        }

        public Expression MakeTestForTypes(DynamicType[] types, int index) {
            Expression test = MakeTypeTest(types[index], index);
            if (index+1 < types.Length) {
                Expression nextTests = MakeTestForTypes(types, index+1);
                if (test.IsConstant(true)) {
                    return nextTests;
                } else if (nextTests.IsConstant(true)) {
                    return test;
                } else {
                    return BinaryExpression.AndAlso(test, nextTests);
                }
            } else {
                return test;
            }
        }

        public Expression MakeTypeTest(DynamicType type, int index) {
            return MakeTypeTest(type, GetParameterExpression(index));
        }

        public Expression MakeTypeTest(DynamicType type, Expression tested) {
            if (type == null || type.IsNull) {
                return BinaryExpression.Equal(tested, ConstantExpression.Constant(null));
            } else {
                Type clrType = type.UnderlyingSystemType;
                bool isStaticType = !typeof(ISuperDynamicObject).IsAssignableFrom(clrType);
                
                // we must always check for non-sealed types explicitly - otherwise we end up
                // doing fast-path behavior on a subtype which overrides behavior that wasn't
                // present for the base type.
                //TODO there's a question about nulls here
                if (CompilerHelpers.IsSealed(clrType) && clrType == tested.ExpressionType) {
                    return ConstantExpression.Constant(true);
                }

                Expression test = MakeTypeTestExpression(type.UnderlyingSystemType, tested);

                if (!isStaticType) {
                    int version = type.Version;
                    test = BinaryExpression.AndAlso(test,
                        MethodCallExpression.Call(null, typeof(RuntimeHelpers).GetMethod("CheckTypeVersion"),
                            tested, ConstantExpression.Constant(version)));

                    AddValidator(new DynamicTypeValidator(new WeakReference(type), version).Validate);
                }
                return test;
            }
        }

        public Expression MakeTypeTestExpression(Type t, int param) {
            return MakeTypeTestExpression(t, GetParameterExpression(param));
        }

        public Expression MakeTypeTestExpression(Type t, Expression expr) {
            return BinaryExpression.AndAlso(
                BinaryExpression.NotEqual(
                    expr,
                    ConstantExpression.Constant(null)),
                BinaryExpression.Equal(
                    MethodCallExpression.Call(
                        expr, typeof(object).GetMethod("GetType")),
                        ConstantExpression.Constant(t)));
        }

        #region Factory Methods
        public static StandardRule<T> Simple(ActionBinder binder, MethodTarget target, params DynamicType[] types) {
            StandardRule<T> ret = new StandardRule<T>();
            ret.MakeTest(types);
            ret.SetTarget(ret.MakeReturn(binder, target.MakeExpression(binder, ret.Parameters)));
            return ret;
        }

        public static StandardRule<T> TypeError(string message, params DynamicType[] types) {
            StandardRule<T> ret = new StandardRule<T>();
            ret.MakeTest(types);
            ret.SetTarget(
                new ExpressionStatement(new ThrowExpression(
                    MethodCallExpression.Call(null, typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                        ConstantExpression.Constant(message)))));
            return ret;
        }

        public static StandardRule<T> AttributeError(string message, params DynamicType[] types) {
            StandardRule<T> ret = new StandardRule<T>();
            ret.MakeTest(types);
            ret.SetTarget(
                new ExpressionStatement(new ThrowExpression(
                    MethodCallExpression.Call(null, typeof(RuntimeHelpers).GetMethod("SimpleAttributeError"),
                        ConstantExpression.Constant(message)))));
            return ret;
        }

        private class DynamicTypeValidator {
            /// <summary>
            /// Weak reference to the dynamic type. Since they can be collected,
            /// we need to be able to let that happen and then disable the rule.
            /// </summary>
            private WeakReference _dynamicType;

            /// <summary>
            /// Expected version of the instance's dynamic type
            /// </summary>
            private int _version;

            public DynamicTypeValidator(WeakReference dynamicType, int version) {
                this._dynamicType = dynamicType;
                this._version = version;
            }

            public bool Validate() {
                DynamicType dt = _dynamicType.Target as DynamicType;
                return dt != null && dt.Version == _version;
            }
        }

        #endregion
    }
}
 
