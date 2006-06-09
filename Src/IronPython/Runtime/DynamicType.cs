/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Reflection;

namespace IronPython.Runtime {
    // DynamicType represents a type which allows member-lookup by name.
    // Currently, the types deriving from DynamicType are:
    // 1. OldClass
    // 2. PythonType

    [PythonType("type")]
    public abstract partial class DynamicType {
        public object __name__;

        public virtual object Negate(object self) {
            return Ops.NotImplemented;
        }

        public virtual object Positive(object self) {
            return Ops.NotImplemented;
        }

        public virtual object OnesComplement(object self) {
            return Ops.NotImplemented;
        }

        public virtual string Repr(object self) {
            object ret;
            if (Ops.TryToInvoke(self, SymbolTable.Repr, out ret)) return (string)ret;
            return self.ToString();
        }

        public virtual object Call(object func, params object[] args) {
            object call;
            if (Ops.TryGetAttr(func, SymbolTable.Call, out call)) return Ops.Call(call, args);

            throw Ops.TypeError("{0} object is not callable", Ops.GetDynamicType(func).__name__);
        }

        public abstract Tuple BaseClasses {
            get;
            set;
        }

        [PythonName("__getitem__")]
        public virtual object GetIndex(object self, object index) {
            Slice slice = index as Slice;
            if (slice != null && slice.step == null) {
                object getSlice;
                if (Ops.TryGetAttr(DefaultContext.Default, self, SymbolTable.GetSlice, out getSlice)) {     
                    int start, stop;
                    slice.DeprecatedFixed(self, out start, out stop);
                    return Ops.Call(getSlice, start, stop);
                }
            }

            return Ops.Invoke(self, SymbolTable.GetItem, index);
        }

        [PythonName("__setitem__")]
        public virtual void SetIndex(object self, object index, object value) {
            Slice slice = index as Slice;
            if (slice != null && slice.step == null) {
                object setSlice;
                if (Ops.TryGetAttr(DefaultContext.Default, self, SymbolTable.SetSlice, out setSlice)) {
                    int start, stop;
                    slice.DeprecatedFixed(self, out start, out stop);
                    Ops.Call(setSlice, start, stop, value);
                    return;
                }
            }

            Ops.Invoke(self, SymbolTable.SetItem, index, value);
        }

        [PythonName("__delitem__")]
        public virtual void DelIndex(object self, object index) {
            Slice slice = index as Slice;
            if (slice != null && slice.step == null) {
                object delSlice;
                if (Ops.TryGetAttr(DefaultContext.Default, self, SymbolTable.DeleteSlice, out delSlice)) {
                    int start, stop;
                    slice.DeprecatedFixed(self, out start, out stop);

                    Ops.Call(delSlice, start, stop);
                    return;
                }
            } 

            Ops.Invoke(self, SymbolTable.DelItem, index);
        }

        public virtual bool TryGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            ret = null;
            return false;            
        }

        public virtual object GetAttr(ICallerContext context, object self, SymbolId name) {
            object ret;
            if (TryGetAttr(context, self, name, out ret)) return ret;
            throw Ops.AttributeError("'{0}' object has no attribute '{1}'", this.__name__, SymbolTable.IdToString(name));
        }

        public virtual void SetAttr(ICallerContext context, object self, SymbolId name, object value) {
            throw Ops.AttributeError("'{0}' object has no attribute '{1}'", this.__name__, SymbolTable.IdToString(name));
        }

        public virtual void DelAttr(ICallerContext context, object self, SymbolId name) {
            throw new NotImplementedException();
        }
       
        public virtual List GetAttrNames(ICallerContext context, object self) {
            return List.MakeEmptyList(0);
        }

        public virtual Dict GetAttrDict(ICallerContext context, object self) {
            return new Dict(0);
        }

        public virtual bool IsSubclassOf(object other) {
            throw new NotImplementedException();
        }

        internal static DynamicType GetDeclaringType(MemberInfo member) {
            Type declaringType = member.DeclaringType;
            if (OpsReflectedType.OpsTypeToType.ContainsKey(declaringType)) {
                // declaringType is an Ops type
                return OpsReflectedType.OpsTypeToType[declaringType];
            } else {
                return Ops.GetDynamicTypeFromType(declaringType);
            }
        }

        public virtual object CompareTo(object self, object other) {
            return Ops.NotImplemented;
        }

        public virtual object Coerce(object self, object other) {
            return Ops.NotImplemented;
        }

        public virtual object Invoke(object target, SymbolId name, params object[] args) {
            return Ops.Call(Ops.GetAttr(DefaultContext.Default, target, name), args);
        }

        public virtual bool TryInvoke(object target, SymbolId name, out object ret, params object []args) {
            object meth;
            if (Ops.TryGetAttr(target, name, out meth)) {
                ret = Ops.Call(meth, args);
                return true;
            } else {
                ret = null;
                return false;
            }
        }

        public virtual bool TryFancyInvoke(object target, SymbolId name, object[] args, string[] names, out object ret) {
            object meth;
            if (Ops.TryGetAttr(target, name, out meth)) {
                IFancyCallable ifc = meth as IFancyCallable;
                if (ifc != null) {
                    ret = ifc.Call(DefaultContext.Default, args, names);
                    return true;
                }

                ret = Ops.Call(meth, args, names);
                return true;
            } else {
                ret = null;
                return false;
            }
        }
    }

    [PythonType("NoneType")]
    public class NoneType : DynamicType, ICallable {
        #region Internal static members

        internal static readonly DynamicType InstanceOfNoneType = new NoneType();
        internal static int NoneHashCode = 0x1e1a1dd0;  // same as CPython.

        #endregion

        #region Private static members
        private static object strMethod, hashMethod, initMethod, newMethod;
        private static SymbolId[] noneAttrs = new SymbolId[] { 
            SymbolTable.Class, SymbolTable.Doc, SymbolTable.Hash, SymbolTable.Init,
            SymbolTable.NewInst, SymbolTable.Repr, SymbolTable.String 

            // !!! these below should also be present.  
            /* "__delattr__", "__getattribute__",  "__new__",     
             * "__reduce__", "__reduce_ex__",  "__setattr__", */        
        };
        #endregion

        #region Constructors
        private NoneType()
            : base() {
            __name__ = "NoneType";
        }
        #endregion

        #region Dynamic Type Overrides

        public override Tuple BaseClasses {
            get { return new Tuple(false, new object[] { TypeCache.Object }); }
            set { throw Ops.TypeError("can't set attributes of built-in/extension type 'NoneType'"); }
        }
        public override bool IsSubclassOf(object other) {
            if (other == this || other == TypeCache.Object) return true;
            return false;
        }

        public override List GetAttrNames(ICallerContext context, object self) {
            List names = base.GetAttrNames(context, self);
            for (int i = 0; i < noneAttrs.Length; i++) {
                names.AddNoLockNoDups(SymbolTable.IdToString(noneAttrs[i]));
            }
            return names;
        }

        public override bool TryGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            switch(name.Id){
                case SymbolTable.ClassId: ret = this;return true;
                case SymbolTable.DocId: ret = null; return true;
                case SymbolTable.ReprId:
                case SymbolTable.StringId: 
                    if (strMethod == null) strMethod = GenerateUnboundMethod("ReprMethod");
                    ret = strMethod;
                    return true;
                case SymbolTable.HashId:
                    if (hashMethod == null) hashMethod = GenerateUnboundMethod("HashMethod");
                    ret = hashMethod;
                    return true;
                case SymbolTable.InitId:
                    if (initMethod == null) initMethod = GenerateUnboundMethod("InitMethod");
                    ret = initMethod;
                    return true;
                case SymbolTable.NewInstId:
                    if (newMethod == null) newMethod = GenerateUnboundMethod("NewMethod");
                    ret = newMethod;
                    return true;
            }
            ret = null;
            return false;
        }
        
        /// <summary>
        /// You can't actually set anything on NoneType, we just override this to get the correct error.
        /// </summary>
        public override void SetAttr(ICallerContext context, object self, SymbolId name, object value) {
            for (int i = 0; i < noneAttrs.Length; i++) {
                if (noneAttrs[i] == name) throw Ops.AttributeError("'NoneType' object attribute '{0}' is read-only", SymbolTable.IdToString(name));
            }

            throw Ops.AttributeError("'NoneType' object has no attribute '{0}'",SymbolTable.IdToString(name));
        }

        #endregion

        #region Static method implementations for None instances

        public static string ReprMethod() {
            return "None";
        }

        public static int HashMethod() {
            return NoneHashCode;
        }

        public static void InitMethod(params object []prms){
            // nop
        }

        public static object NewMethod(object type, params object[] prms) {            
            if (type == InstanceOfNoneType) {
                throw Ops.TypeError("cannot create instances of 'NoneType'");
            }
            // someone is using  None.__new__ or type(None).__new__ to create
            // a new instance.  Call the type they want to create the instance for.
            return Ops.Call(type, prms);
        }

        private static object GenerateUnboundMethod(string name) {
            MethodInfo mi = typeof(NoneType).GetMethod(name);
            return new ReflectedUnboundMethod(name, mi, NameType.PythonMethod);
        }

        #endregion

        #region Object overrides

        public override string ToString() {
            return "<type 'NoneType'>";
        }

        #endregion

        #region ICallable Members

        object ICallable.Call(params object[] args) {
            throw Ops.TypeError("cannot create 'NoneType' instances");
        }

        #endregion

    }
}
