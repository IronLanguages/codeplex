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

#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// This is a helper class for runtime-callable-wrappers of COM instances. We create one instance of this type
    /// for every generic RCW instance.
    /// </summary>
    public abstract class ComObject : IOldDynamicObject {
        #region Data Members

        private const string _comObjectTypeName = "System.__ComObject";
        private readonly object _comObject; // the runtime-callable wrapper
        private readonly static object _ComObjectInfoKey = (object)1; // use an int as the key since hashing an Int32.GetHashCode is cheap
        private readonly static Type _comObjectType = Type.GetType(_comObjectTypeName);
        
        #endregion

        #region Constructor(s)/Initialization

        protected ComObject(object rcw) {
            Debug.Assert(IsGenericComObject(rcw));
            _comObject = rcw;
        }
        
        #endregion

        #region Public Members

        public object Obj {
            get {
                return _comObject;
            }
        }

        #endregion

        #region Static Members

        public static Type ComObjectType {
            get { return _comObjectType; }
        }

        /// <summary>
        /// This is the factory method to get the ComObject corresponding to an RCW
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public static ComObject ObjectToComObject(object rcw) {
            Debug.Assert(IsGenericComObject(rcw));

            // Marshal.Get/SetComObjectData has a LinkDemand for UnmanagedCode which will turn into
            // a full demand. We could avoid this by making this method SecurityCritical
            object data = Marshal.GetComObjectData(rcw, _ComObjectInfoKey);
            if (data != null) {
                return (ComObject)data;
            }

            lock (_ComObjectInfoKey) {
                data = Marshal.GetComObjectData(rcw, _ComObjectInfoKey);
                if (data != null) {
                    return (ComObject)data;
                }

                ComObject comObjectInfo = CreateComObject(rcw);
                if (!Marshal.SetComObjectData(rcw, _ComObjectInfoKey, comObjectInfo)) {
                    throw new COMException("Marshal.SetComObjectData failed");
                }

                return comObjectInfo;
            }
        }

        private static ComObject CreateComObject(object rcw) {
            IDispatch dispatchObject = rcw as IDispatch;
            if (ScriptDomainManager.Options.PreferComDispatchOverTypeInfo && (dispatchObject != null)) {
                // We can do method invocations on IDispatch objects
                return new IDispatchComObject(dispatchObject);
            }

            // First check if we can associate metadata with the COM object
            ComObject comObject = ComObjectWithTypeInfo.CheckClassInfo(rcw);
            if (comObject != null) {
                return comObject;
            }

            if (dispatchObject != null) {
                // We can do method invocations on IDispatch objects
                return new IDispatchComObject(dispatchObject);
            }

            // There is not much we can do in this case
            return new GenericComObject(rcw);
        }

        public static bool IsGenericComObjectType(Type type) {
            return type == _comObjectType;
        }

        public static bool IsGenericComObject(object obj) {
            return obj != null && IsGenericComObjectType(obj.GetType());
        }

        #endregion

        #region Ast Generation

        /// <summary>
        /// Gets the target rule body for the GetMember action. This handles statements of the kind:
        /// comObject.Method(parameter1, parameter2, ...,  parameterN)
        /// comObject.Property
        /// The return value of the resulting AST is the result of the Method lookup or Property invocation as:
        /// comObject.LookUp("Method")
        /// comObject.Invoke("get_Property")
        /// In the case of comObject.Method, by returning a DispMethod instance, it is assumed that a subsequent call will be performed
        /// to resolve the method invocation.  The DispMethod instance is a callable object in keeping with the potential treatment
        /// of functions as first class objects.
        /// </summary>
        /// <param name="rule">The rule that the binder helper is attempting to construct, supplying temporary variables and parameters to the AST block.</param>
        /// <param name="action">The get member that is to be performed.</param>
        /// <returns>
        /// This function returns an AST statement that implements the relevant method/property lookup/invoke.
        /// </returns>
        public static Expression GetTargetForGetMember(RuleBuilder rule, OldMemberAction action) {
            VariableExpression comObject = rule.GetTemporary(typeof(ComObject), "comObject");
            List<Expression> expressions = new List<Expression>();

            expressions.Add(
                Ast.Assign(
                    comObject,
                    Ast.SimpleCallHelper(
                        typeof(ComObject).GetMethod("ObjectToComObject"),
                        rule.Parameters[0]
                    )
                )
            );

            expressions.Add(
                rule.MakeReturn(
                    action.Binder,
                    Ast.ActionExpression(
                        action,
                        rule.ReturnType,
                        rule.Context,
                        comObject
                    )
                )
            );

            return Ast.Block(expressions);
        }

        /// <summary>
        /// Gets the target rule body for the SetMember action. This handles statements of the kind:
        /// 
        /// comObject.Property = value
        /// 
        /// The return value of the resulting AST is the result of the Property invocation as:
        /// 
        /// comObject.Invoke("put_Property", value)
        /// </summary>
        /// <param name="rule">The rule that the binder helper is attempting to construct, supplying temporary variables and parameters to the AST block.</param>
        /// <param name="action">The set member that is to be performed.</param>
        /// <returns>This function returns an AST statement that implements the required property set invocation.</returns>
        public static Expression GetTargetForSetMember(RuleBuilder rule, OldMemberAction action) {
            VariableExpression comObject = rule.GetTemporary(typeof(ComObject), "comObject");
            List<Expression> expressions = new List<Expression>();

            expressions.Add(
                Ast.Assign(
                    comObject,
                    Ast.SimpleCallHelper(
                        typeof(ComObject).GetMethod("ObjectToComObject"),
                        rule.Parameters[0])));

            expressions.Add(
                rule.MakeReturn(
                    action.Binder,
                    Ast.ActionExpression(
                        action,
                        rule.ReturnType,
                        rule.Context,
                        comObject,
                        rule.Parameters[1]
                    )
                )
            );

            return Ast.Block(expressions);
        }

        /// <summary>
        /// Gets the target rule body for the GetItem operation. This handles statements of the kind:
        /// 
        /// comObject[index1, index2, ..., indexN]
        /// 
        /// The return value of the resulting AST is the result of the IndexerMethod or IndexerProperty invocation as:
        /// 
        /// comObject.Invoke("__propertyget__", index1, index2, ..., indexN)
        /// 
        /// This implies that comObject implements a "default" method or property (DispId = 0) adorned with the PropertyPut attribute
        /// and most usually associated with the method/property name "Item".
        /// </summary>
        /// <param name="rule">The rule that the binder helper is attempting to construct, supplying temporary variables and parameters to the AST block.</param>
        /// <param name="action">The do operation that is to be performed.</param>
        /// <returns>This function returns an AST statement that implements the indexed operation "propertyget".</returns>
        public static Expression GetTargetForDoOperation(RuleBuilder rule, OldDoOperationAction action) {
            VariableExpression comObject = rule.GetTemporary(typeof(ComObject), "comObject");
            List<Expression> expressions = new List<Expression>();

            expressions.Add(
                Ast.Assign(
                    comObject,
                    Ast.SimpleCallHelper(
                        typeof(ComObject).GetMethod("ObjectToComObject"),
                        rule.Parameters[0]
                    )
                )
            );

            expressions.Add(
                rule.MakeReturn(
                    action.Binder,
                    Ast.ActionExpression(
                        action,
                        rule.ReturnType,
                        ArrayUtils.InsertAt<Expression>(ArrayUtils.RemoveFirst(rule.Parameters), 0, rule.Context, comObject)
                    )
                )
            );

            return Ast.Block(expressions);
        }

        // The rule test now checks to ensure that the wrapper is of the correct type so that any cast against on the RCW will succeed. 
        // Note that the test must NOT test the wrapper itself since the wrapper is a surrogate for the RCW instance and would cause a 
        // memory leak when a wrapped RCW goes out of scope.  So, the test asks the argument (which is an RCW wrapper) to identify its 
        // RCW’s type.  On the rule creation side, the type is encoded in the test so that when the rule cache is searched the test will 
        // succeed only if the wrapper’s returned RCW type matches that expected by the test. 
        public static Expression MakeComObjectTest(Type type, PropertyInfo testProperty, object targetObject, RuleBuilder rule) {
            Expression t0 = rule.MakeTypeTest(type, 0);
            Expression testObject = 
                Ast.SimpleCallHelper(
                    Ast.Convert(
                        rule.Parameters[0], 
                        type), 
                    testProperty.GetGetMethod());
            Expression t1 =
                Ast.Equal(
                    testObject,
                    Ast.Constant(targetObject));
            return Ast.AndAlso(t0, t1);
        }

        #endregion

        public abstract IList<string> GetMemberNames(CodeContext context);

        #region IOldDynamicObject Members

        public abstract RuleBuilder<T> GetRule<T>(OldDynamicAction action, CodeContext context, object[] args) where T : class;

        #endregion

        #region Abstract Members

        public abstract string Documentation {
            get;
        }

        #endregion
    }
}

#endif
