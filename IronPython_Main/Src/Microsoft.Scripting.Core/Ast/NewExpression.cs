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
using System.Collections.ObjectModel;
using System.Reflection;
using System.Scripting.Actions;
using System.Scripting.Utils;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class NewExpression : Expression {
        private readonly ConstructorInfo _constructor;
        private readonly ReadOnlyCollection<Expression> _arguments;
        private readonly ReadOnlyCollection<MemberInfo> _members;

        internal NewExpression(Annotations annotations, Type type, ConstructorInfo constructor, ReadOnlyCollection<Expression> arguments, CallSiteBinder bindingInfo)
            : base(ExpressionType.New, type, annotations, bindingInfo) {
            if (IsBound) {
                RequiresBoundItems(arguments, "arguments");
            }
            _constructor = constructor;
            _arguments = arguments;
        }

        internal NewExpression(Annotations annotations, Type type, ConstructorInfo constructor, ReadOnlyCollection<Expression> arguments, CallSiteBinder bindingInfo, ReadOnlyCollection<MemberInfo> members)
            : this(annotations, type, constructor, arguments, bindingInfo) {

            _members = members;
        }

        public ConstructorInfo Constructor {
            get { return _constructor; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return _arguments; }
        }

        public ReadOnlyCollection<MemberInfo> Members {
            get { return _members; }
        }


        private static PropertyInfo GetPropertyNoThrow(MethodInfo method) {
            if (method == null) {
                return null;
            }
            Type type = method.DeclaringType;
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic;
            flags |= (method.IsStatic) ? BindingFlags.Static : BindingFlags.Instance;
            PropertyInfo[] props = type.GetProperties(flags);
            foreach (PropertyInfo pi in props) {
                if (pi.CanRead && method == pi.GetGetMethod(true)) {
                    return pi;
                }
                if (pi.CanWrite && method == pi.GetSetMethod(true)) {
                    return pi;
                }
            }
            return null;
        }

        internal override void BuildString(StringBuilder builder) {
            Type type = (_constructor == null) ? type = this.Type : _constructor.DeclaringType;
            builder.Append("new ");
            int n = _arguments.Count;
            builder.Append(type.Name);
            builder.Append("(");
            if (n > 0) {
                for (int i = 0; i < n; i++) {
                    if (i > 0) {
                        builder.Append(", ");
                    }
                    if (_members != null) {
                        PropertyInfo pi = null;
                        if (_members[i].MemberType == MemberTypes.Method && (pi = GetPropertyNoThrow((MethodInfo)_members[i])) != null) {
                            builder.Append(pi.Name);
                        } else {
                            builder.Append(_members[i].Name);
                        }
                        builder.Append(" = ");
                    }
                    _arguments[i].BuildString(builder);
                }
            }
            builder.Append(")");
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor) {
            return New(constructor, (IEnumerable<Expression>)null);
        }

        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments) {
            return New(constructor, (IEnumerable<Expression>)arguments);
        }

        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments) {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ReadOnlyCollection<Expression> argList = arguments.ToReadOnly();
            ValidateNewArgs(constructor.DeclaringType, constructor, ref argList);
            return new NewExpression(Annotations.Empty, constructor.DeclaringType, constructor, argList, null);
        }

        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members) {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ReadOnlyCollection<MemberInfo> memberList = members.ToReadOnly();
            ReadOnlyCollection<Expression> argList = arguments.ToReadOnly();
            ValidateNewArgs(constructor, ref argList, memberList);
            return new NewExpression(Annotations.Empty, constructor.DeclaringType, constructor, argList, null, memberList);
        }

        //CONFORMING
        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo[] members) {
            return New(constructor, arguments, members.ToReadOnly());
        }

        //CONFORMING
        public static NewExpression New(Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            if (type == typeof(void)) {
                throw Error.ArgumentCannotBeOfTypeVoid();
            }
            ConstructorInfo ci = null;
            if (!type.IsValueType) {
                ci = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
                if (ci == null) {
                    throw Error.TypeMissingDefaultConstructor(type);
                }
                return New(ci);
            }
            return new NewExpression(Annotations.Empty, type, null, EmptyReadOnlyCollection<Expression>.Instance, null);
        }


        //CONFORMING
        private static void ValidateNewArgs(ConstructorInfo constructor, ref ReadOnlyCollection<Expression> arguments, ReadOnlyCollection<MemberInfo> members) {
            ParameterInfo[] pis;
            if ((pis = constructor.GetParameters()).Length > 0) {
                if (arguments.Count != pis.Length) {
                    throw Error.IncorrectNumberOfConstructorArguments();
                }
                if (arguments.Count != members.Count) {
                    throw Error.IncorrectNumberOfArgumentsForMembers();
                }
                Expression[] newArguments = null;
                for (int i = 0, n = arguments.Count; i < n; i++) {
                    Expression arg = arguments[i];
                    ContractUtils.RequiresNotNull(arg, "argument");
                    MemberInfo member = members[i];
                    ContractUtils.RequiresNotNull(member, "member");
                    if (member.DeclaringType != constructor.DeclaringType) {
                        throw Error.ArgumentMemberNotDeclOnType(member.Name, constructor.DeclaringType.Name);
                    }
                    Type memberType;
                    ValidateAnonymousTypeMember(member, out memberType);
                    if (!TypeUtils.AreReferenceAssignable(arg.Type, memberType)) {
                        if (TypeUtils.IsSameOrSubclass(typeof(Expression), memberType) && TypeUtils.AreAssignable(memberType, arg.GetType())) {
                            arg = Expression.Quote(arg);
                        } else {
                            throw Error.ArgumentTypeDoesNotMatchMember(arg.Type, memberType);
                        }
                    }
                    ParameterInfo pi = pis[i];
                    Type pType = pi.ParameterType;
                    if (pType.IsByRef) {
                        pType = pType.GetElementType();
                    }
                    if (!TypeUtils.AreReferenceAssignable(pType, arg.Type)) {
                        if (TypeUtils.IsSameOrSubclass(typeof(Expression), pType) && TypeUtils.AreAssignable(pType, arg.Type)) {
                            arg = Expression.Quote(arg);
                        } else {
                            throw Error.ExpressionTypeDoesNotMatchConstructorParameter(arg.Type, pType);
                        }
                    }
                    if (newArguments == null && arg != arguments[i]) {
                        newArguments = new Expression[arguments.Count];
                        for (int j = 0; j < i; j++) {
                            newArguments[j] = arguments[j];
                        }
                    }
                    if (newArguments != null) {
                        newArguments[i] = arg;
                    }
                }
                if (newArguments != null) {
                    arguments = new ReadOnlyCollection<Expression>(newArguments);
                }
            } else if (arguments != null && arguments.Count > 0) {
                throw Error.IncorrectNumberOfConstructorArguments();
            } else if (members != null && members.Count > 0) {
                throw Error.IncorrectNumberOfMembersForGivenConstructor();
            }
        }

        //CONFORMING
        private static void ValidateNewArgs(Type type, ConstructorInfo constructor, ref ReadOnlyCollection<Expression> arguments) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(constructor, "constructor");

            TypeUtils.ValidateType(type);

            ParameterInfo[] pis;
            if (constructor != null && (pis = constructor.GetParameters()).Length > 0) {
                if (arguments.Count != pis.Length) {
                    throw Error.IncorrectNumberOfConstructorArguments();
                }
                Expression[] newArgs = null;
                for (int i = 0, n = arguments.Count; i < n; i++) {
                    Expression arg = arguments[i];
                    ParameterInfo pi = pis[i];
                    RequiresCanRead(arg, "arguments");
                    Type pType = pi.ParameterType;
                    if (pType.IsByRef) {
                        pType = pType.GetElementType();
                    }
                    if (!TypeUtils.AreReferenceAssignable(pType, arg.Type)) {
                        if (TypeUtils.IsSameOrSubclass(typeof(Expression), pType) && TypeUtils.AreAssignable(pType, arg.GetType())) {
                            arg = Expression.Quote(arg);
                        } else {
                            throw Error.ExpressionTypeDoesNotMatchConstructorParameter(arg.Type, pType);
                        }
                    }
                    if (newArgs == null && arg != arguments[i]) {
                        newArgs = new Expression[arguments.Count];
                        for (int j = 0; j < i; j++) {
                            newArgs[j] = arguments[j];
                        }
                    }
                    if (newArgs != null) {
                        newArgs[i] = arg;
                    }
                }
                if (newArgs != null) {
                    arguments = new ReadOnlyCollection<Expression>(newArgs);
                }
            } else if (arguments != null && arguments.Count > 0) {
                throw Error.IncorrectNumberOfConstructorArguments();
            }
        }

        //CONFORMING
        private static void ValidateAnonymousTypeMember(MemberInfo member, out Type memberType) {
            switch (member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo field = member as FieldInfo;
                    if (field.IsStatic) {
                        throw Error.ArgumentMustBeInstanceMember();
                    }
                    memberType = field.FieldType;
                    break;
                case MemberTypes.Property:
                    PropertyInfo pi = member as PropertyInfo;
                    if (!pi.CanRead) {
                        throw Error.PropertyDoesNotHaveGetter(pi);
                    }
                    if (pi.GetGetMethod().IsStatic) {
                        throw Error.ArgumentMustBeInstanceMember();
                    }
                    memberType = pi.PropertyType;
                    break;
                case MemberTypes.Method:
                    MethodInfo method = member as MethodInfo;
                    if (method.IsStatic) {
                        throw Error.ArgumentMustBeInstanceMember();
                    }
                    memberType = method.ReturnType;
                    break;
                default:
                    throw Error.ArgumentMustBeFieldInfoOrPropertInfoOrMethod();
            }
        }



        public static NewExpression New(Type result, CallSiteBinder bindingInfo, params Expression[] arguments) {
            return New(result, bindingInfo, (IList<Expression>)arguments);
        }

        public static NewExpression New(Type result, CallSiteBinder bindingInfo, IList<Expression> arguments) {
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            RequiresCanRead(arguments, "arguments");
            return new NewExpression(Annotations.Empty, result, null, arguments.ToReadOnly(), bindingInfo);
        }

        public static NewExpression SimpleNewHelper(ConstructorInfo constructor, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");

            ParameterInfo[] parameters = constructor.GetParameters();
            ContractUtils.Requires(arguments.Length == parameters.Length, "arguments", Strings.IncorrectArgNumber);

            return New(constructor, ArgumentConvertHelper(arguments, parameters));
        }
    }
}
