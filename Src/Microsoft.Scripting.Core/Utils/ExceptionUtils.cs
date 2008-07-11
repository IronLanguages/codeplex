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

namespace System.Scripting.Utils {
    public static class ExceptionUtils {
        public static ArgumentOutOfRangeException MakeArgumentOutOfRangeException(string paramName, object actualValue, string message) {
#if SILVERLIGHT // ArgumentOutOfRangeException ctor overload
            throw new ArgumentOutOfRangeException(paramName, string.Format("{0} (actual value is '{1}')", message, actualValue));
#else
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
#endif
        }

        public static ArgumentNullException MakeArgumentItemNullException(int index, string arrayName) {
            return new ArgumentNullException(String.Format("{0}[{1}]", arrayName, index));
        }
    }


    //TODO: strings should come from localized resources
    //CONFORMING
    /// <summary>
    ///    Strongly-typed and parameterized exception factory.
    /// </summary>
    internal static class Error {
        /// <summary>
        /// ArgumentException with message like "Argument type cannot be System.Void."
        /// </summary>
        internal static Exception ArgumentCannotBeOfTypeVoid() {
            return new ArgumentException("Argument type cannot be System.Void.");
        }
        /// <summary>
        /// ArgumentException with message like "Argument must be of an integer type"
        /// </summary>
        internal static Exception ArgumentMustBeInteger() {
            return new ArgumentException("Argument must be of an integer type");
        }
        /// <summary>
        /// ArgumentException with message like "User-defined operator method '{0}' must be static."
        /// </summary>
        internal static Exception UserDefinedOperatorMustBeStatic(object p0) {
            return new ArgumentException(string.Format("User-defined operator method '{0}' must be static.", p0));
        }
        /// <summary>
        /// ArgumentException with message like "User-defined operator method '{0}' must not be void."
        /// </summary>
        internal static Exception UserDefinedOperatorMustNotBeVoid(object p0) {
            return new ArgumentException(string.Format("User-defined operator method '{0}' must not be void.", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Method {0} contains generic parameters"
        /// </summary>
        internal static Exception MethodContainsGenericParameters(object p0) {
            return new ArgumentException(string.Format("Method {0} contains generic parameters", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Method {0} is a generic method definition"
        /// </summary>
        internal static Exception MethodIsGeneric(object p0) {
            return new ArgumentException(string.Format("Method {0} is a generic method definition", p0));
        }
        /// <summary>
        /// InvalidOperationException with message like "The operands for operator '{0}' do not match the parameters of method '{1}'."
        /// </summary>
        internal static Exception OperandTypesDoNotMatchParameters(object p0, object p1) {
            return new InvalidOperationException(string.Format("The operands for operator '{0}' do not match the parameters of method '{1}'.", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Incorrect number of arguments supplied for call to method '{0}'"
        /// </summary>
        internal static Exception IncorrectNumberOfMethodCallArguments(object p0) {
            return new ArgumentException(string.Format("Incorrect number of arguments supplied for call to method '{0}'", p0));
        }
        /// <summary>
        /// InvalidOperationException with message like "The unary operator {0} is not defined for the type '{1}'."
        /// </summary>
        internal static Exception UnaryOperatorNotDefined(object p0, object p1) {
            return new InvalidOperationException(string.Format("The unary operator {0} is not defined for the type '{1}'.", p0, p1));
        }
        /// <summary>
        /// InvalidOperationException with message like "The binary operator {0} is not defined for the types '{1}' and '{2}'."
        /// </summary>
        internal static Exception BinaryOperatorNotDefined(object p0, object p1, object p2) {
            return new InvalidOperationException(string.Format("The binary operator {0} is not defined for the types '{1}' and '{2}'.", p0, p1, p2));
        }
        /// <summary>
        /// InvalidOperationException with message like "No method '{0}' exists on type '{1}'."
        /// </summary>
        internal static Exception MethodDoesNotExistOnType(object p0, object p1) {
            return new InvalidOperationException(string.Format("No method '{0}' exists on type '{1}'.", p0, p1));
        }
        /// <summary>
        /// InvalidOperationException with message like "No method '{0}' on type '{1}' is compatible with the supplied arguments."
        /// </summary>
        internal static Exception MethodWithArgsDoesNotExistOnType(object p0, object p1) {
            return new InvalidOperationException(string.Format("No method '{0}' on type '{1}' is compatible with the supplied arguments.", p0, p1));
        }
        /// <summary>
        /// InvalidOperationException with message like "More than one method '{0}' on type '{1}' is compatible with the supplied arguments."
        /// </summary>
        internal static Exception MethodWithMoreThanOneMatch(object p0, object p1) {
            return new InvalidOperationException(string.Format("More than one method '{0}' on type '{1}' is compatible with the supplied arguments.", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Argument must be array"
        /// </summary>
        internal static Exception ArgumentMustBeArray() {
            return new ArgumentException("Argument must be array");
        }
        /// <summary>
        /// ArgumentException with message like "Incorrect number of indexes"
        /// </summary>
        internal static Exception IncorrectNumberOfIndexes() {
            return new ArgumentException("Incorrect number of indexes");
        }
        /// <summary>
        /// ArgumentException with message like "Argument for array index must be of type Int32"
        /// </summary>
        internal static Exception ArgumentMustBeArrayIndexType() {
            return new ArgumentException("Argument for array index must be of type Int32");
        }
        /// <summary>
        /// ArgumentException with message like "The user-defined operator method '{1}' for operator '{0}' must have identical parameter and return types."
        /// </summary>
        internal static Exception LogicalOperatorMustHaveConsistentTypes(object p0, object p1) {
            return new ArgumentException(string.Format("The user-defined operator method '{1}' for operator '{0}' must have identical parameter and return types.", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "The user-defined operator method '{1}' for operator '{0}' must have associated boolean True and False operators."
        /// </summary>
        internal static Exception LogicalOperatorMustHaveBooleanOperators(object p0, object p1) {
            return new ArgumentException(string.Format("The user-defined operator method '{1}' for operator '{0}' must have associated boolean True and False operators.", p0, p1));
        }
        /// <summary>
        /// InvalidOperationException with message like "An expression of type '{0}' cannot be used to initialize an array of type '{1}'"
        /// </summary>
        internal static Exception ExpressionTypeCannotInitializeArrayType(object p0, object p1) {
            return new InvalidOperationException(string.Format("An expression of type '{0}' cannot be used to initialize an array of type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "The type used in TypeAs Expression must be of reference or nullable type, {0} is neither"
        /// </summary>
        internal static Exception IncorrectTypeForTypeAs(object p0) {
            return new ArgumentException(string.Format("The type used in TypeAs Expression must be of reference or nullable type, {0} is neither", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Unhandled unary: {0}"
        /// </summary>
        internal static Exception UnhandledUnary(object p0) {
            return new ArgumentException(string.Format("Unhandled unary: {0}", p0));
        }
        /// <summary>
        /// InvalidOperationException with message like "No coercion operator is defined between types '{0}' and '{1}'."
        /// </summary>
        internal static Exception CoercionOperatorNotDefined(object p0, object p1) {
            return new InvalidOperationException(string.Format("No coercion operator is defined between types '{0}' and '{1}'.", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Argument must be single dimensional array type"
        /// </summary>
        internal static Exception ArgumentMustBeSingleDimensionalArrayType() {
            return new ArgumentException("Argument must be single dimensional array type");
        }
        /// <summary>
        /// ArgumentException with message like "Unhandled binary: {0}"
        /// </summary>
        internal static Exception UnhandledBinary(object p0) {
            return new ArgumentException(string.Format("Unhandled binary: {0}", p0));
        }
        /// <summary>
        /// InvalidOperationException with message like "Coalesce used with type that cannot be null"
        /// </summary>
        internal static Exception CoalesceUsedOnNonNullType() {
            return new InvalidOperationException("Coalesce used with type that cannot be null");
        }
        /// <summary>
        /// ArgumentException with message like "Argument types do not match"
        /// </summary>
        internal static Exception ArgumentTypesMustMatch() {
            return new ArgumentException("Argument types do not match");
        }
        /// <summary>
        /// ArgumentException with message like "The method '{0}.{1}' is not a property accessor"
        /// </summary>
        internal static Exception MethodNotPropertyAccessor(object p0, object p1) {
            return new ArgumentException(string.Format("The method '{0}.{1}' is not a property accessor", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "'{0}' is not a member of type '{1}'"
        /// </summary>
        internal static Exception NotAMemberOfType(object p0, object p1) {
            return new ArgumentException(string.Format("'{0}' is not a member of type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Member '{0}' not field or property"
        /// </summary>
        internal static Exception MemberNotFieldOrProperty(object p0) {
            return new ArgumentException(string.Format("Member '{0}' not field or property", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Field '{0}' is not defined for type '{1}'"
        /// </summary>
        internal static Exception FieldNotDefinedForType(object p0, object p1) {
            return new ArgumentException(string.Format("Field '{0}' is not defined for type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "The property '{0}' has no 'get' accessor"
        /// </summary>
        internal static Exception PropertyDoesNotHaveGetter(object p0) {
            return new ArgumentException(string.Format("The property '{0}' has no 'get' accessor", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Property '{0}' is not defined for type '{1}'"
        /// </summary>
        internal static Exception PropertyNotDefinedForType(object p0, object p1) {
            return new ArgumentException(string.Format("Property '{0}' is not defined for type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Argument must be boolean"
        /// </summary>
        internal static Exception ArgumentMustBeBoolean() {
            return new ArgumentException("Argument must be boolean");
        }
        /// <summary>
        /// ArgumentException with message like "Method '{0}' is not defined for type '{1}'"
        /// </summary>
        internal static Exception MethodNotDefinedForType(object p0, object p1) {
            return new ArgumentException(string.Format("Method '{0}' is not defined for type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Expression of type '{0}' cannot be used for parameter of type '{1}' of method '{2}'"
        /// </summary>
        internal static Exception ExpressionTypeDoesNotMatchMethodParameter(object p0, object p1, object p2) {
            return new ArgumentException(string.Format("Expression of type '{0}' cannot be used for parameter of type '{1}' of method '{2}'", p0, p1, p2));
        }
        /// <summary>
        /// ArgumentException with message like "Expression of type '{0}' cannot be used for parameter of type '{1}'"
        /// </summary>
        internal static Exception ExpressionTypeDoesNotMatchParameter(object p0, object p1) {
            return new ArgumentException(string.Format("Expression of type '{0}' cannot be used for parameter of type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Type {0} is a generic type definition"
        /// </summary>
        internal static Exception TypeIsGeneric(object p0) {
            return new ArgumentException(string.Format("Type {0} is a generic type definition", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Type {0} contains generic parameters"
        /// </summary>
        internal static Exception TypeContainsGenericParameters(object p0) {
            return new ArgumentException(string.Format("Type {0} contains generic parameters", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Type '{0}' does not have a default constructor"
        /// </summary>
        internal static Exception TypeMissingDefaultConstructor(object p0) {
            return new ArgumentException(string.Format("Type '{0}' does not have a default constructor", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Incorrect number of arguments for constructor"
        /// </summary>
        internal static Exception IncorrectNumberOfConstructorArguments() {
            return new ArgumentException("Incorrect number of arguments for constructor");
        }
        /// <summary>
        /// ArgumentException with message like "Incorrect number of arguments for the given members "
        /// </summary>
        internal static Exception IncorrectNumberOfArgumentsForMembers() {
            return new ArgumentException("Incorrect number of arguments for the given members ");
        }
        /// <summary>
        /// ArgumentException with message like " The member '{0}' is not declared on type '{1}' being created"
        /// </summary>
        internal static Exception ArgumentMemberNotDeclOnType(object p0, object p1) {
            return new ArgumentException(string.Format(" The member '{0}' is not declared on type '{1}' being created", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like " Argument type '{0}' does not match the corresponding member type '{1}'"
        /// </summary>
        internal static Exception ArgumentTypeDoesNotMatchMember(object p0, object p1) {
            return new ArgumentException(string.Format(" Argument type '{0}' does not match the corresponding member type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Expression of type '{0}' cannot be used for constructor parameter of type '{1}'"
        /// </summary>
        internal static Exception ExpressionTypeDoesNotMatchConstructorParameter(object p0, object p1) {
            return new ArgumentException(string.Format("Expression of type '{0}' cannot be used for constructor parameter of type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like " Incorrect number of members for constructor"
        /// </summary>
        internal static Exception IncorrectNumberOfMembersForGivenConstructor() {
            return new ArgumentException(" Incorrect number of members for constructor");
        }
        /// <summary>
        /// ArgumentException with message like "Argument must be an instance member"
        /// </summary>
        internal static Exception ArgumentMustBeInstanceMember() {
            return new ArgumentException("Argument must be an instance member");
        }
        /// <summary>
        /// ArgumentException with message like "Argument must be either a FieldInfo, PropertyInfo or MethodInfo"
        /// </summary>
        internal static Exception ArgumentMustBeFieldInfoOrPropertInfoOrMethod() {
            return new ArgumentException("Argument must be either a FieldInfo, PropertyInfo or MethodInfo");
        }
        /// <summary>
        /// ArgumentException with message like "Expression of type '{0}' cannot be invoked"
        /// </summary>
        internal static Exception ExpressionTypeNotInvocable(object p0) {
            return new ArgumentException(string.Format("Expression of type '{0}' cannot be invoked", p0));
        }
        /// <summary>
        /// InvalidOperationException with message like "Incorrect number of arguments supplied for lambda invocation"
        /// </summary>
        internal static Exception IncorrectNumberOfLambdaArguments() {
            return new InvalidOperationException("Incorrect number of arguments supplied for lambda invocation");
        }
        /// <summary>
        /// ArgumentException with message like "Lambda type parameter must be derived from System.Delegate"
        /// </summary>
        internal static Exception LambdaTypeMustBeDerivedFromSystemDelegate() {
            return new ArgumentException("Lambda type parameter must be derived from System.Delegate");
        }
        /// <summary>
        /// ArgumentException with message like "Incorrect number of parameters supplied for lambda declaration"
        /// </summary>
        internal static Exception IncorrectNumberOfLambdaDeclarationParameters() {
            return new ArgumentException("Incorrect number of parameters supplied for lambda declaration");
        }
        /// <summary>
        /// ArgumentException with message like "A lambda expression cannot contain pass by reference parameters."
        /// </summary>
        internal static Exception ExpressionMayNotContainByrefParameters() {
            return new ArgumentException("A lambda expression cannot contain pass by reference parameters.");
        }
        /// <summary>
        /// ArgumentException with message like "ParameterExpression of type '{0}' cannot be used for delegate parameter of type '{1}'"
        /// </summary>
        internal static Exception ParameterExpressionNotValidAsDelegate(object p0, object p1) {
            return new ArgumentException(string.Format("ParameterExpression of type '{0}' cannot be used for delegate parameter of type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Expression of type '{0}' cannot be used for return type '{1}'"
        /// </summary>
        internal static Exception ExpressionTypeDoesNotMatchReturn(object p0, object p1) {
            return new ArgumentException(string.Format("Expression of type '{0}' cannot be used for return type '{1}'", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "List initializers must contain at least one initializer"
        /// </summary>
        internal static Exception ListInitializerWithZeroMembers() {
            return new ArgumentException("List initializers must contain at least one initializer");
        }
        /// <summary>
        /// ArgumentException with message like "Type '{0}' is not IEnumerable"
        /// </summary>
        internal static Exception TypeNotIEnumerable(object p0) {
            return new ArgumentException(string.Format("Type '{0}' is not IEnumerable", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Argument must be either a FieldInfo or PropertyInfo"
        /// </summary>
        internal static Exception ArgumentMustBeFieldInfoOrPropertInfo() {
            return new ArgumentException("Argument must be either a FieldInfo or PropertyInfo");
        }
        /// <summary>
        /// ArgumentException with message like "The property '{0}' has no 'set' accessor"
        /// </summary>
        internal static Exception PropertyDoesNotHaveSetter(object p0) {
            return new ArgumentException(string.Format("The property '{0}' has no 'set' accessor", p0));
        }
        /// <summary>
        /// ArgumentException with message like "Element initializer method must have at least 1 parameter"
        /// </summary>
        internal static Exception ElementInitializerMethodWithZeroArgs() {
            return new ArgumentException("Element initializer method must have at least 1 parameter");
        }
        /// <summary>
        /// ArgumentException with message like "Element initializer method must be named 'Add'"
        /// </summary>
        internal static Exception ElementInitializerMethodNotAdd() {
            return new ArgumentException("Element initializer method must be named 'Add'");
        }
        /// <summary>
        /// ArgumentException with message like "Element initializer method must be an instance method"
        /// </summary>
        internal static Exception ElementInitializerMethodStatic() {
            return new ArgumentException("Element initializer method must be an instance method");
        }
        /// <summary>
        /// ArgumentException with message like "Parameter '{0}' of element initializer method '{1}' must not be a pass by reference parameter"
        /// </summary>
        internal static Exception ElementInitializerMethodNoRefOutParam(object p0, object p1) {
            return new ArgumentException(string.Format("Parameter '{0}' of element initializer method '{1}' must not be a pass by reference parameter", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Unhandled Binding Type: {0}"
        /// </summary>
        internal static Exception UnhandledBindingType(object p0) {
            return new ArgumentException(string.Format("Unhandled Binding Type: {0}", p0));
        }
        /// <summary>
        /// InvalidOperationException with message like "Cannot cast from type '{0}' to type '{1}"
        /// </summary>
        internal static Exception InvalidCast(object p0, object p1) {
            return new InvalidOperationException(string.Format("Cannot cast from type '{0}' to type '{1}", p0, p1));
        }
        /// <summary>
        /// ArgumentException with message like "Unhandled convert: {0}"
        /// </summary>
        internal static Exception UnhandledConvert(object p0) {
            return new ArgumentException(string.Format("Unhandled convert: {0}", p0));
        }
        /// <summary>
        /// NotImplementedException with message like "The operator '{0}' is not implemented for type '{1}'"
        /// </summary>
        internal static Exception OperatorNotImplementedForType(object p0, object p1) {
            return new NotImplementedException(string.Format("The operator '{0}' is not implemented for type '{1}'", p0, p1));
        }
        /// <summary>
        /// InvalidOperationException with message like "Unexpected coalesce operator."
        /// </summary>
        internal static Exception UnexpectedCoalesceOperator() {
            return new InvalidOperationException("Unexpected coalesce operator.");
        }
        /// <summary>
        /// ArgumentException with message like "Unknown binding type"
        /// </summary>
        internal static Exception UnknownBindingType() {
            return new ArgumentException("Unknown binding type");
        }
        /// <summary>
        /// ArgumentException with message like "Unhandled binding"
        /// </summary>
        internal static Exception UnhandledBinding() {
            return new ArgumentException("Unhandled binding");
        }
        /// <summary>
        /// InvalidOperationException with message like "Cannot auto initialize elements of value type through property '{0}', use assignment instead"
        /// </summary>
        internal static Exception CannotAutoInitializeValueTypeElementThroughProperty(object p0) {
            return new InvalidOperationException(string.Format("Cannot auto initialize elements of value type through property '{0}', use assignment instead", p0));
        }
        /// <summary>
        /// InvalidOperationException with message like "Cannot auto initialize members of value type through property '{0}', use assignment instead"
        /// </summary>
        internal static Exception CannotAutoInitializeValueTypeMemberThroughProperty(object p0) {
            return new InvalidOperationException(string.Format("Cannot auto initialize members of value type through property '{0}', use assignment instead", p0));
        }
    }
}
