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
using Microsoft.Linq.Expressions.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Diagnostics;

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

        internal LambdaExpression(
            Annotations annotations,
            ExpressionType nodeType,
            Type delegateType,
            string name,
            Expression body,
            ReadOnlyCollection<ParameterExpression> parameters
        )
            : base(nodeType, delegateType, annotations) {

            Assert.NotNull(delegateType);

            _name = name;
            _body = body;
            _parameters = parameters;
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
                Parameters[0].BuildString(builder);
            } else {
                builder.Append("(");
                for (int i = 0, n = Parameters.Count; i < n; i++) {
                    if (i > 0)
                        builder.Append(", ");
                    Parameters[i].BuildString(builder);
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

        // TODO: Remove the Compile<T> overloads
        //
        // They allow compiling an Expression<T> with a different
        // (but compatible) delegate signature.
        //
        // Instead, Expression<T> should be created with the right
        // delegate type.

        public T Compile<T>() {
            return LambdaCompiler.CompileLambda<T>(this, false);
        }

        public T Compile<T>(bool emitDebugSymbols) {
            return LambdaCompiler.CompileLambda<T>(this, emitDebugSymbols);
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitLambda(this);
        }
    }

    //CONFORMING
    public sealed class Expression<TDelegate> : LambdaExpression {
        internal Expression(
            Annotations annotations,
            ExpressionType nodeType,
            string name,
            Expression body,
            ReadOnlyCollection<ParameterExpression> parameters
        )
            : base(annotations, nodeType, typeof(TDelegate), name, body, parameters) {
        }

        public new TDelegate Compile() {
            return Compile<TDelegate>();
        }

        public new TDelegate Compile(bool emitDebugSymbols) {
            return Compile<TDelegate>(emitDebugSymbols);
        }
    }


    public partial class Expression {
        //internal lambda factory that creates an instance of Expression<delegateType>
        internal static LambdaExpression Lambda(ExpressionType nodeType, Type delegateType, string name, Expression body, Annotations annotations, ReadOnlyCollection<ParameterExpression> parameters) {
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
            return new Expression<TDelegate>(annotations, ExpressionType.Lambda, name, body, parameterList);
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

        //CONFORMING
        private static void ValidateLambdaArgs(Type delegateType, ref Expression body, ReadOnlyCollection<ParameterExpression> parameters) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            RequiresCanRead(body, "body");

            if (!TypeUtils.AreAssignable(typeof(Delegate), delegateType) || delegateType == typeof(Delegate)) {
                throw Error.LambdaTypeMustBeDerivedFromSystemDelegate();
            }
            MethodInfo mi = delegateType.GetMethod("Invoke");
            ParameterInfo[] pis = mi.GetParameters();
            if (pis.Length > 0) {
                if (pis.Length != parameters.Count) {
                    throw Error.IncorrectNumberOfLambdaDeclarationParameters();
                }
                for (int i = 0, n = pis.Length; i < n; i++) {
                    Expression pex = parameters[i];
                    ParameterInfo pi = pis[i];
                    RequiresCanRead(pex, "parameters");
                    Type pType = pi.ParameterType;
                    if (pType.IsByRef || pex.Type.IsByRef) {
                        throw Error.ExpressionMayNotContainByrefParameters();
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

        private static void ValidateGeneratorReturnType(Type delegateType) {
            Debug.Assert(delegateType != null && delegateType.IsSubclassOf(typeof(Delegate)));
            MethodInfo invoke = delegateType.GetMethod("Invoke");

            // Currently only IEnumerator or IEnumerator<object> are supported
            Type rt = invoke.ReturnType;
            if (rt != typeof(System.Collections.IEnumerator) &&
                rt != typeof(IEnumerator<object>)) {
                throw Error.WrongIteratorReturnType();
            }
        }

        // TODO: review factories
        #region Generator

        public static Expression<TDelegate> Generator<TDelegate>(Expression body, params ParameterExpression[] parameters) {
            return Generator<TDelegate>(body, null, Annotations.Empty, parameters);
        }

        public static Expression<TDelegate> Generator<TDelegate>(Expression body, string name, params ParameterExpression[] parameters) {
            return Generator<TDelegate>(body, name, Annotations.Empty, parameters);
        }

        public static Expression<TDelegate> Generator<TDelegate>(Expression body, string name, Annotations annotations, IEnumerable<ParameterExpression> parameters) {
            ReadOnlyCollection<ParameterExpression> parameterList = parameters.ToReadOnly();
            ValidateLambdaArgs(typeof(TDelegate), ref body, parameterList);
            ValidateGeneratorReturnType(typeof(TDelegate));
            return new Expression<TDelegate>(annotations, ExpressionType.Generator, name, body, parameterList);
        }

        public static LambdaExpression Generator(Type delegateType, Expression body, params ParameterExpression[] parameters) {
            return Generator(delegateType, body, null, Annotations.Empty, parameters);
        }

        public static LambdaExpression Generator(Type delegateType, Expression body, string name, params ParameterExpression[] parameters) {
            return Generator(delegateType, body, name, Annotations.Empty, parameters);
        }

        public static LambdaExpression Generator(Type delegateType, Expression body, string name, Annotations annotations, IEnumerable<ParameterExpression> parameters) {
            ReadOnlyCollection<ParameterExpression> paramList = parameters.ToReadOnly();
            ValidateLambdaArgs(delegateType, ref body, paramList);
            ValidateGeneratorReturnType(delegateType);
            return Lambda(ExpressionType.Generator, delegateType, name, body, annotations, paramList);
        }

        #endregion


        //CONFORMING
        public static Type GetFuncType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");

            Type funcType;

            switch (typeArgs.Length) {
                case 1:
                    funcType = typeof(Func<>).MakeGenericType(typeArgs);
                    break;
                case 2:
                    funcType = typeof(Func<,>).MakeGenericType(typeArgs);
                    break;
                case 3:
                    funcType = typeof(Func<,,>).MakeGenericType(typeArgs);
                    break;
                case 4:
                    funcType = typeof(Func<,,,>).MakeGenericType(typeArgs);
                    break;
                case 5:
                    funcType = typeof(Func<,,,,>).MakeGenericType(typeArgs);
                    break;
                case 6:
                    funcType = typeof(Func<,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 7:
                    funcType = typeof(Func<,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 8:
                    funcType = typeof(Func<,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 9:
                    funcType = typeof(Func<,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 10:
                    funcType = typeof(Func<,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 11:
                    funcType = typeof(Func<,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 12:
                    funcType = typeof(Func<,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 13:
                    funcType = typeof(Func<,,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 14:
                    funcType = typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 15:
                    funcType = typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 16:
                    funcType = typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 17:
                    funcType = typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                default:
                    throw Error.IncorrectNumberOfTypeArgsForFunc();
            }
            return funcType;
        }
        //CONFORMING
        public static Type GetActionType(params Type[] typeArgs) {
            ContractUtils.RequiresNotNull(typeArgs, "typeArgs");

            Type actionType;

            switch (typeArgs.Length) {
                case 0:
                    actionType = typeof(Action);
                    break;
                case 1:
                    actionType = typeof(Action<>).MakeGenericType(typeArgs);
                    break;
                case 2:
                    actionType = typeof(Action<,>).MakeGenericType(typeArgs);
                    break;
                case 3:
                    actionType = typeof(Action<,,>).MakeGenericType(typeArgs);
                    break;
                case 4:
                    actionType = typeof(Action<,,,>).MakeGenericType(typeArgs);
                    break;
                case 5:
                    actionType = typeof(Action<,,,,>).MakeGenericType(typeArgs);
                    break;
                case 6:
                    actionType = typeof(Action<,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 7:
                    actionType = typeof(Action<,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 8:
                    actionType = typeof(Action<,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 9:
                    actionType = typeof(Action<,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 10:
                    actionType = typeof(Action<,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 11:
                    actionType = typeof(Action<,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 12:
                    actionType = typeof(Action<,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 13:
                    actionType = typeof(Action<,,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 14:
                    actionType = typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 15:
                    actionType = typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                case 16:
                    actionType = typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(typeArgs);
                    break;
                default:
                    throw Error.IncorrectNumberOfTypeArgsForAction();
            }
            return actionType;
        }
    }
}
