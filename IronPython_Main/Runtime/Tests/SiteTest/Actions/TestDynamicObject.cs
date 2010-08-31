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
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace SiteTest.Actions {
    /// <summary>
    /// Vacuous dynamic object that always defers to the CallSiteBinder
    /// </summary>
    class TestDynamicObject : DynamicObject {
    }

    [Serializable()]
    class MBRODynamicObject : MarshalByRefObject, IDynamicMetaObjectProvider {
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new MBROBinder(parameter, BindingRestrictions.Empty, this);
        }

        public object GetComObj() {
            return Activator.CreateInstance(Type.GetTypeFromProgID("DlrComLibrary.Properties"));
        }
    }

    class MBROBinder : DynamicMetaObject {
        public MBROBinder(Expression expression, BindingRestrictions restrictions, object value):
        base(expression, restrictions, value) {
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            return new DynamicMetaObject(
                Expression.Constant("MBRO_GetMember"),
                // MBRO metaobjects need to decide whether binding applies to remoted instances.
                BindingRestrictions.GetExpressionRestriction(
                    Expression.AndAlso(
                        Expression.TypeEqual(Expression, typeof(MBRODynamicObject)),
                        Expression.Not(
                            Expression.Call(
                                typeof(System.Runtime.Remoting.RemotingServices).GetMethod("IsObjectOutOfAppDomain"),
                                Expression
                            )
                        )
                    )
                )
            );
        }
    }

#if CLR45
    [Serializable]
    class SerializableDO : DynamicObject {
        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = "SerializableDO";
            return true;
        }
    }
#endif

    /// <summary>
    /// Dynamic object that implements every action and succeeds
    /// </summary>
    class TestDynamicObject2 : DynamicObject {
        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
            result = "BinaryOperation";
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            result = "InvokeMember";
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result) {
            result = "Convert";
            return true;
        }

        public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result) {
            result = "CreateInstance";
            return true;
        }

        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] args) {
            throw new BindingException("DeleteIndex");
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder) {
            throw new BindingException("DeleteMember");
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] args, out object result) {
            result = "GetIndex";
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = "GetMember";
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
            result = "Invoke";
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            throw new BindingException("SetIndex");
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            throw new BindingException("SetMember");
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
            result = "UnaryOperation";
            return true;
        }
    }

    /// <summary>
    /// Dynamic object that implements every action and fails
    /// </summary>
    class TestDynamicObject3 : DynamicObject {
        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
            result = null;
            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            result = null;
            return false;
        }

        public override bool TryConvert(ConvertBinder binder, out object result) {
            result = null;
            return false;
        }

        public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result) {
            result = null;
            return false;
        }

        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] args) {
            return false;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder) {
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] args, out object result) {
            result = null;
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = null;
            return false;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
            result = null;
            return false;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            return false;
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Dynamic object that overrides GetMetaObject
    /// </summary>
    class TestDynamicObject4 : DynamicObject {
        public override DynamicMetaObject GetMetaObject(Expression parameter) {
            //Do something more
            throw new NotImplementedException("Override exists solely to test the public virtual nature of GetMetaObject on DynamicObject");
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            //Do something more
            throw new NotImplementedException("Override exists solely to test the public virtual nature of GetDynamicMemberNames on DynamicObject");
        }
    }

    class ByRefDynamicObject : DynamicObject {
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            indexes[0] = 42;
            indexes[1] = 43;
            indexes[2] = 44;
            result = 45;
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            indexes[0] = 42;
            indexes[1] = 43;
            value = 44;
            return true;
        }

        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes) {
            indexes[0] = 42;
            indexes[1] = 43;
            indexes[2] = 44;
            return true;
        }
        
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
            args[0] = 42;
            args[1] = 43;
            args[2] = 44;
            result = 45;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            args[0] = 42;
            args[1] = 43;
            args[2] = 44;
            result = 45;
            return true;
        }
    }
}
