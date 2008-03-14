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
using System.Runtime.InteropServices;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {

    using Ast = Microsoft.Scripting.Ast.Ast;

    /// <summary>
    /// This is a helper class for runtime-callable-wrappers of COM instances. We create one instance of this type
    /// for every generic RCW instance.
    /// </summary>
    public abstract class ComObject : IDynamicObject {
        #region Data Members
        
        private readonly object _comObject; // the runtime-callable wrapper

        private static readonly object _ComObjectInfoKey = (object)1; // use an int as the key since hashing an Int32.GetHashCode is cheap
        private const string comObjectTypeName = "System.__ComObject";
        private readonly static Type comObjectType = Type.GetType(comObjectTypeName);
        
        #endregion

        #region Constructor(s)/Initialization

        protected ComObject(object rcw) {
            Debug.Assert(Is__ComObject(rcw.GetType()));
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
            get { return comObjectType; }
        }

        /// <summary>
        /// This is the factory method to get the ComObject corresponding to an RCW
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public static ComObject ObjectToComObject(object rcw) {
            Debug.Assert(Is__ComObject(rcw.GetType()));

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

        static ComObject CreateComObject(object rcw) {
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

        static public bool Is__ComObject(Type type) {
            return type == comObjectType;
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
        /// <param name="binder">The binder that is provided by the language for conversions.</param>
        /// <param name="action">The get member that is to be performed.</param>
        /// <returns>
        /// This function returns an AST statement that implements the relevant method/property lookup/invoke.
        /// </returns>
        public static Expression GetTargetForGetMember(StandardRule rule, ActionBinder binder, MemberAction action) {
            VariableExpression comObject = rule.GetTemporary(typeof(ComObject), "comObject");
            List<Expression> expressions = new List<Expression>();

            expressions.Add(
                Ast.Write(
                    comObject,
                    Ast.SimpleCallHelper(
                        typeof(ComObject).GetMethod("ObjectToComObject"),
                        rule.Parameters[0])));

            expressions.Add(
                rule.MakeReturn(
                    binder,
                    Ast.Action.GetMember(
                        action.Name,
                        rule.ReturnType,
                        Ast.Read(comObject))));

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
        /// <param name="binder">The binder that is provided by the language for conversions.</param>
        /// <param name="action">The set member that is to be performed.</param>
        /// <returns>This function returns an AST statement that implements the required property set invocation.</returns>
        public static Expression GetTargetForSetMember(StandardRule rule, ActionBinder binder, MemberAction action) {
            VariableExpression comObject = rule.GetTemporary(typeof(ComObject), "comObject");
            List<Expression> expressions = new List<Expression>();

            expressions.Add(
                Ast.Write(
                    comObject,
                    Ast.SimpleCallHelper(
                        typeof(ComObject).GetMethod("ObjectToComObject"),
                        rule.Parameters[0])));

            expressions.Add(
                rule.MakeReturn(
                    binder,
                    Ast.Action.SetMember(
                        action.Name,
                        rule.ReturnType,
                        Ast.Read(comObject),
                        rule.Parameters[1])));

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
        /// <param name="binder">The binder that is provided by the language for conversions.</param>
        /// <param name="action">The do operation that is to be performed.</param>
        /// <returns>This function returns an AST statement that implements the indexed operation "propertyget".</returns>
        public static Expression GetTargetForDoOperation(StandardRule rule, ActionBinder binder, DoOperationAction action) {
            VariableExpression comObject = rule.GetTemporary(typeof(ComObject), "comObject");
            List<Expression> expressions = new List<Expression>();

            expressions.Add(
                Ast.Write(
                    comObject,
                    Ast.SimpleCallHelper(
                        typeof(ComObject).GetMethod("ObjectToComObject"),
                        rule.Parameters[0])));

            expressions.Add(
                rule.MakeReturn(
                    binder,
                    Ast.Action.Operator(
                        action.Operation,
                        rule.ReturnType,
                        ArrayUtils.InsertAt<Expression>((Expression[])ArrayUtils.RemoveFirst(rule.Parameters), 0, Ast.Read(comObject)))));

            return Ast.Block(expressions);
        }

        #endregion

        #region IMembersList Members

        public abstract IList<SymbolId> GetMemberNames(CodeContext context);

        #endregion

        #region IDynamicObject Members

        // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public abstract LanguageContext LanguageContext {
            get;
        }

        public abstract StandardRule<T> GetRule<T>(DynamicAction action, CodeContext context, object[] args);

        #endregion

        #region Abstract Members

        public abstract string Documentation {
            get;
        }

        #endregion
    }
}

#endif
