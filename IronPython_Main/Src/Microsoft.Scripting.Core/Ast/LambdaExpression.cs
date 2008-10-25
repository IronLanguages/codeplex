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
using System; using Microsoft;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Threading;

namespace Microsoft.Linq.Expressions {
    //CONFORMING
    /// <summary>
    /// This captures a block of code that should correspond to a .NET method
    /// body. It takes input through parameters and is expected to be fully
    /// bound. This code can then be generated in a variety of ways. The
    /// variables can be kept as .NET locals or hoisted into an object bound to
    /// the delegate. This is the primary unit used for passing around
    /// Expression Trees in LINQ and the DLR.
    /// </summary>
    public class LambdaExpression : Expression {
        private readonly string _name;
        private readonly Expression _body;
        private readonly ReadOnlyCollection<ParameterExpression> _parameters;
        private readonly Type _delegateType;

        internal LambdaExpression(
            Annotations annotations,
            Type delegateType,
            string name,
            Expression body,
            ReadOnlyCollection<ParameterExpression> parameters
        )
            : base(annotations) {

            Assert.NotNull(delegateType);

            _name = name;
            _body = body;
            _parameters = parameters;
            _delegateType = delegateType;
        }

        protected override Type GetExpressionType() {
            return _delegateType;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Lambda;
        }

        public ReadOnlyCollection<ParameterExpression> Parameters {
            get { return _parameters; }
        }

        public string Name {
            get { return _name; }
        }

        public Expression Body {
            get { return _body; }
        }

        public Type ReturnType {
            get { return Type.GetMethod("Invoke").ReturnType; }
        }

        internal override void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");

            if (Parameters.Count == 1) {
                ParameterExpression pe = Parameters[0];
                if (pe.IsByRef) {
                    builder.Append("ref ");
                }
                pe.BuildString(builder);
            } else {
                builder.Append("(");
                for (int i = 0, n = Parameters.Count; i < n; i++) {
                    if (i > 0) {
                        builder.Append(", ");
                    }
                    ParameterExpression pe = Parameters[i];
                    if (pe.IsByRef) {
                        builder.Append("ref ");
                    }
                    pe.BuildString(builder);
                }
                builder.Append(")");
            }
            builder.Append(" => ");
            _body.BuildString(builder);
        }

        public Delegate Compile() {
            return LambdaCompiler.CompileLambda(this, false);
        }

        public Delegate Compile(bool emitDebugSymbols) {
            return LambdaCompiler.CompileLambda(this, emitDebugSymbols);
        }

        // TODO: add an overload that returns a DynamicMethod?
        // TODO: this method doesn't expose the full power of TypeBuilder.DefineMethod
        public MethodBuilder CompileToMethod(TypeBuilder type, MethodAttributes attributes, bool emitDebugSymbols) {
            ContractUtils.RequiresNotNull(type, "type");
            if (emitDebugSymbols) {
                var module = type.Module as ModuleBuilder;
                ContractUtils.Requires(module != null, "method", Strings.InvalidTypeBuilder);
            }
            return LambdaCompiler.CompileLambda(this, type, attributes, emitDebugSymbols);
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitLambda(this);
        }

        internal virtual LambdaExpression CloneWith(string name, Expression body, Annotations annotations, ReadOnlyCollection<ParameterExpression> parameters) {
            return Expression.Lambda(NodeType, Type, name, body, annotations, parameters);
        }
    }

    //CONFORMING
    public sealed class Expression<TDelegate> : LambdaExpression {
        internal Expression(
            Annotations annotations,
            string name,
            Expression body,
            ReadOnlyCollection<ParameterExpression> parameters
        )
            : base(annotations, typeof(TDelegate), name, body, parameters) {
        }

        public new TDelegate Compile() {
            return LambdaCompiler.CompileLambda<TDelegate>(this, false);
        }

        public new TDelegate Compile(bool emitDebugSymbols) {
            return LambdaCompiler.CompileLambda<TDelegate>(this, emitDebugSymbols);
        }

        internal override LambdaExpression CloneWith(string name, Expression body, Annotations annotations, ReadOnlyCollection<ParameterExpression> parameters) {
            return Lambda<TDelegate>(body, name, annotations, parameters);
        }
    }


    public partial class Expression {
        private static CacheDict<Type, Func<Expression, string, Annotations, IEnumerable<ParameterExpression>, LambdaExpression>> _exprCtors = new CacheDict<Type, Func<Expression, string, Annotations, IEnumerable<ParameterExpression>, LambdaExpression>>(200);
        private static MethodInfo _lambdaCtorMethod;

        //internal lambda factory that creates an instance of Expression<delegateType>
        internal static LambdaExpression Lambda(
                ExpressionType nodeType,
                Type delegateType,
                string name,
                Expression body,
                Annotations annotations,
                ReadOnlyCollection<ParameterExpression> parameters
        ) {
            if (nodeType == ExpressionType.Lambda) {
                // got or create a delegate to the public Expression.Lambda<T> method and call that will be used for
                // creating instances of this delegate type
                Func<Expression, string, Annotations, IEnumerable<ParameterExpression>, LambdaExpression> func;
                lock (_exprCtors) {
                    if (_lambdaCtorMethod == null) {
                        EnsureLambdCtor();
                    }

                    if (!_exprCtors.TryGetValue(delegateType, out func)) {
                        _exprCtors[delegateType] = func = (Func<Expression, string, Annotations, IEnumerable<ParameterExpression>, LambdaExpression>)
                            Delegate.CreateDelegate(
                                typeof(Func<Expression, string, Annotations, IEnumerable<ParameterExpression>, LambdaExpression>),
                                _lambdaCtorMethod.MakeGenericMethod(delegateType)
                            );
                    }
                }

                return func(body, name, annotations, parameters);
            }

            return SlowMakeLambda(annotations, nodeType, delegateType, name, body, parameters);
        }

        private static void EnsureLambdCtor() {
            MethodInfo[] methods = (MethodInfo[])typeof(Expression).GetMember("Lambda", MemberTypes.Method, BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo mi in methods) {
                if (!mi.IsGenericMethod) {
                    continue;
                }

                ParameterInfo[] pis = mi.GetParameters();
                if (pis.Length == 4) {
                    if (pis[0].ParameterType == typeof(Expression) &&
                        pis[1].ParameterType == typeof(string) &&
                        pis[2].ParameterType == typeof(Annotations) &&
                        pis[3].ParameterType == typeof(IEnumerable<ParameterExpression>)) {
                        _lambdaCtorMethod = mi;
                        break;
                    }
                }
            }
            Debug.Assert(_lambdaCtorMethod != null);
        }

        private static LambdaExpression SlowMakeLambda(Annotations annotations, ExpressionType nodeType, Type delegateType, string name, Expression body, ReadOnlyCollection<ParameterExpression> parameters) {
            Type ot = typeof(Expression<>);
            Type ct = ot.MakeGenericType(new Type[] { delegateType });
            ConstructorInfo ctor = ct.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, ctorTypes, null);
            return (LambdaExpression)ctor.Invoke(new object[] { annotations, nodeType, name, body, parameters });
        }

        //CONFORMING
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters) {
            return Lambda<TDelegate>(body, (IEnumerable<ParameterExpression>)parameters);
        }

        //CONFORMING
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, IEnumerable<ParameterExpression> parameters) {
            return Lambda<TDelegate>(body, "lambda_method", Annotations.Empty, parameters);
        }

        //CONFORMING
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, String name, Annotations annotations, IEnumerable<ParameterExpression> parameters) {
            ReadOnlyCollection<ParameterExpression> parameterList = parameters.ToReadOnly();
            ValidateLambdaArgs(typeof(TDelegate), ref body, parameterList);
            return new Expression<TDelegate>(annotations, name, body, parameterList);
        }


        public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters) {
            return Lambda(body, (IEnumerable<ParameterExpression>)parameters);
        }

        public static LambdaExpression Lambda(Expression body, IEnumerable<ParameterExpression> parameters) {
            return Lambda(body, "lambda_method", parameters);
        }

        //CONFORMING
        public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters) {
            return Lambda(delegateType, body, "lambda_method", Annotations.Empty, parameters);
        }

        //CONFORMING
        public static LambdaExpression Lambda(Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters) {
            return Lambda(delegateType, body, "lambda_method", Annotations.Empty, parameters);
        }

        public static LambdaExpression Lambda(Expression body, string name, IEnumerable<ParameterExpression> parameters) {
            return Lambda(body, name, Annotations.Empty, parameters);
        }

        public static LambdaExpression Lambda(Type delegateType, Expression body, string name, IEnumerable<ParameterExpression> parameters) {
            return Lambda(delegateType, body, name, Annotations.Empty, parameters);
        }


        //CONFORMING
        public static LambdaExpression Lambda(Expression body, string name, Annotations annotations, IEnumerable<ParameterExpression> parameters) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(body, "body");

            ReadOnlyCollection<ParameterExpression> parameterList = parameters.ToReadOnly();

            bool action = body.Type == typeof(void);

            int paramCount = parameterList.Count;
            Type[] typeArgs = new Type[paramCount + (action ? 0 : 1)];
            for (int i = 0; i < paramCount; i++) {
                ContractUtils.RequiresNotNull(parameterList[i], "parameter");
                typeArgs[i] = parameterList[i].Type;
            }

            Type delegateType;
            if (action)
                delegateType = GetActionType(typeArgs);
            else {
                typeArgs[paramCount] = body.Type;
                delegateType = GetFuncType(typeArgs);
            }

            return Lambda(ExpressionType.Lambda, delegateType, name, body, annotations, parameterList);
        }


        private static Type[] ctorTypes = new Type[] {
            typeof(Annotations),        // annotations,
            typeof(ExpressionType),     // nodeType,
            typeof(string),             // name,
            typeof(Expression),         // body,
            typeof(ReadOnlyCollection<ParameterExpression>) // parameters) 
        };

        //CONFORMING
        public static LambdaExpression Lambda(Type delegateType, Expression body, string name, Annotations annotations, IEnumerable<ParameterExpression> parameters) {
            ReadOnlyCollection<ParameterExpression> paramList = parameters.ToReadOnly();
            ValidateLambdaArgs(delegateType, ref body, paramList);

            return Lambda(ExpressionType.Lambda, delegateType, name, body, annotations, paramList);
        }

        private static CacheDict<Type, MethodInfo> _LambdaDelegateCache = new CacheDict<Type, MethodInfo>(40);

        //CONFORMING
        private static void ValidateLambdaArgs(Type delegateType, ref Expression body, ReadOnlyCollection<ParameterExpression> parameters) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            RequiresCanRead(body, "body");

            if (!TypeUtils.AreAssignable(typeof(Delegate), delegateType) || delegateType == typeof(Delegate)) {
                throw Error.LambdaTypeMustBeDerivedFromSystemDelegate();
            }

            MethodInfo mi;
            lock (_LambdaDelegateCache) {
                if (!_LambdaDelegateCache.TryGetValue(delegateType, out mi)) {
                    _LambdaDelegateCache[delegateType] = mi = delegateType.GetMethod("Invoke");
                }
            }

            ParameterInfo[] pis = mi.GetParametersCached();

            if (pis.Length > 0) {
                if (pis.Length != parameters.Count) {
                    throw Error.IncorrectNumberOfLambdaDeclarationParameters();
                }
                for (int i = 0, n = pis.Length; i < n; i++) {
                    ParameterExpression pex = parameters[i];
                    ParameterInfo pi = pis[i];
                    RequiresCanRead(pex, "parameters");
                    Type pType = pi.ParameterType;
                    if (pex.IsByRef) {
                        pType = pType.GetElementType();
                    }
                    if (!TypeUtils.AreReferenceAssignable(pex.Type, pType)) {
                        throw Error.ParameterExpressionNotValidAsDelegate(pex.Type, pType);
                    }
                }
            } else if (parameters.Count > 0) {
                throw Error.IncorrectNumberOfLambdaDeclarationParameters();
            }
            if (mi.ReturnType != typeof(void) && !TypeUtils.AreReferenceAssignable(mi.ReturnType, body.Type)) {
                //TODO: statement lambda may be autoquoted to Expression<void>. 
                // cosider whether this is desirable.
                if (TypeUtils.IsSameOrSubclass(typeof(Expression), mi.ReturnType) && TypeUtils.AreAssignable(mi.ReturnType, body.GetType())) {
                    body = Expression.Quote(body);
                } else {
                    // We allow the body type to be void with non-void return statements, so
                    // we can’t completely verify this here without walking the tree. Instead we validate
                    // this condition in VariableBinder.
                    // TODO: can the return statement design be improved to allow the check here?
                    if (body.Type != typeof(void)) {
                        throw Error.ExpressionTypeDoesNotMatchReturn(body.Type, mi.ReturnType);
                    }
                }
            }
        }

        //CONFORMING
        public static Type GetFuncType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");
            return DelegateHelpers.GetFuncType(typeArgs);
        }

        //CONFORMING
        public static Type GetActionType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");
            return DelegateHelpers.GetActionType(typeArgs);
        }
    }
}
