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
using System.Text;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Types;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    [PythonType(typeof(DynamicType))]
    public class ExtensibleType : DynamicType, ICustomMembers, ICallableWithCodeContext {
        
        [StaticExtensionMethod("__new__")]
        public static object Make(DynamicType cls, object o) {
            return DynamicHelpers.GetDynamicType(o);
        }
        
        [StaticExtensionMethod("__new__")]
        public static object Make(CodeContext context, DynamicType dt, string name, Tuple bases, IAttributesCollection dict) {
            return new ExtensibleType(context, name, bases, dict);
        }

        public ExtensibleType(CodeContext context)
            : base(Compiler.Generation.NewTypeMaker.GetNewType("type", Tuple.MakeTuple(TypeCache.DynamicType), new PythonDictionary())) {                        
        }

        public ExtensibleType(CodeContext context, string name, Tuple bases, IAttributesCollection dict)
            :
            base(Compiler.Generation.NewTypeMaker.GetNewType("type", bases, new PythonDictionary())) {
            UserTypeBuilder.Build(context, this, name, bases, dict);

        }

        #region ICustomMembers Members

        bool ICustomMembers.TryGetCustomMember(CodeContext context, SymbolId name, out object value) {
            if (name == Symbols.Class) {
                value = DynamicHelpers.GetDynamicType(this);
                return true;
            }

            // we search the slots instead of doing a TryGetAttr so we can pass in the
            // correct context (us instead of the DynamicType object)
            DynamicTypeSlot dts;
            if (Value.TryResolveSlot(context, name, out dts)) {
                if(dts.TryGetValue(context, null, this.Value, out value))
                    return true;
            }

            return DynamicHelpers.GetDynamicType(this).TryGetMember(context, this, name, out value);
        }

        bool ICustomMembers.TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            if (name == Symbols.Class) {
                value = DynamicHelpers.GetDynamicType(this);
                return true;
            }

            // we search the slots instead of doing a TryGetAttr so we can pass in the
            // correct context (us instead of the DynamicType object)
            DynamicTypeSlot dts;
            if (Value.TryResolveSlot(context, name, out dts)) {
                if (dts.TryGetBoundValue(context, null, this.Value, out value))
                    return true;
            }

            return DynamicHelpers.GetDynamicType(this).TryGetMember(context, this, name, out value);
        }

        void ICustomMembers.SetCustomMember(CodeContext context, SymbolId name, object value) {
            Value.SetCustomMember(context, name, value);
        }

        bool ICustomMembers.DeleteCustomMember(CodeContext context, SymbolId name) {
            return Value.DeleteCustomMember(context, name);
        }

        IList<object> IMembersList.GetCustomMemberNames(CodeContext context) {
            IList<object> res = Value.GetCustomMemberNames(context);
            foreach (object x in TypeCache.DynamicType.GetCustomMemberNames(context)){
                res.Add(x);
            }
            return res;
        }

        IDictionary<object, object> ICustomMembers.GetCustomMemberDictionary(CodeContext context) {            
            IDictionary<object, object> dict = Value.GetMemberDictionary(context).AsObjectKeyedDictionary();
            foreach (KeyValuePair<object, object> kvp in TypeCache.DynamicType.GetCustomMemberDictionary(context)) {
                if (!dict.ContainsKey(kvp.Key))
                    dict[kvp.Key] = kvp.Value;
            }
            return dict;
        }

        #endregion

        public DynamicType Value {
            get {
                return this;
            }
        }


        #region ICallableWithCodeContext Members

        [SpecialName, PythonName("__call__")]    // still need PythonName for the virtual override to work
        public virtual object Call(CodeContext context, params object[] args) {
            return DynamicTypeOps.CallParams(context, this, args);
        }

        #endregion
    }
}
