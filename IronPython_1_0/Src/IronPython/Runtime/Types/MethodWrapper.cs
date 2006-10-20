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

using System.Collections;
using System.Collections.Generic;

using System.Diagnostics;
using System.Reflection;

using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Method wrappers provide quick access to commonly used methods that
    /// short-circuit walking the entire inheritance chain.  When a method wrapper
    /// is added to a type we calculate it's underlying value.  When the value (or
    /// base classes) changes we update the value.  That way we can always quickly
    /// get the correct value.
    /// </summary>
    public sealed class MethodWrapper : ICallable, IFancyCallable, IDataDescriptor {
        FieldInfo myField;
        private DynamicType pythonType;
        internal SymbolId name;

        private bool isObjectMethod, isBuiltinMethod, isSuperTypeMethod;
        private object func = null;
        private BuiltinFunction funcAsFunc = null;

        public static MethodWrapper Make(DynamicType pt, SymbolId name) {
            MethodWrapper ret = new MethodWrapper(pt, name);
            object meth;
            if (pt.dict.TryGetValue(name, out meth)) {
                object otherMeth;
                if (!pt.TryLookupSlotInBases(DefaultContext.Default, name, out otherMeth) || otherMeth != meth) {
                    ret.SetDeclaredMethod(meth);
                } else {
                    // they did __foo__ = myBase.__foo__, we'll just ignore it...
                    ret.UpdateFromBases(pt.MethodResolutionOrder);
                }
            } else {
                ret.UpdateFromBases(pt.MethodResolutionOrder);
            }

            //pt.dict[name] = ret; //???

            return ret;
        }

        public static MethodWrapper MakeUndefined(DynamicType pt, SymbolId name) {
            return new MethodWrapper(pt, name);
        }

        public static MethodWrapper MakeForObject(DynamicType pt, SymbolId name, Delegate func) {
            MethodWrapper ret = new MethodWrapper(pt, name);
            ret.isObjectMethod = true;
            ret.isBuiltinMethod = true;
            ret.isSuperTypeMethod = false;

            ret.func = BuiltinFunction.MakeMethod((string)SymbolTable.IdToString(name), func.Method, FunctionType.Function);
            ret.funcAsFunc = ret.func as BuiltinFunction;

            //pt.dict[name] = ret;

            return ret;
        }

        //public static MethodWrapper MakeDefault() { return new MethodWrapper(null, true, true); }

        public MethodWrapper(DynamicType pt, SymbolId name) {
            this.pythonType = pt;
            this.name = name;
            string fieldname = SymbolTable.IdToString(name) + "F";
            this.myField = typeof(DynamicType).GetField(fieldname);
            this.isObjectMethod = true;
            this.isBuiltinMethod = true;
            this.isSuperTypeMethod = true;
        }

        public override string ToString() {
            return String.Format("MethodWrapper for {0}.{1} => {2}>", pythonType, name, func);
        }

        public void SetDeclaredMethod(object m) {
            this.func = m;
            this.funcAsFunc = m as BuiltinFunction;
            this.isObjectMethod = pythonType.type == typeof(object);
            this.isBuiltinMethod = pythonType is ReflectedType;
            this.isSuperTypeMethod = false;

            pythonType.dict[this.name] = m;
        }

        public void UpdateFromBases(Tuple mro) {
            if (!isSuperTypeMethod) return;

            Debug.Assert(mro.Count > 0);
            MethodWrapper current = (MethodWrapper)myField.GetValue(mro[0]);

            for (int i = 1; i < mro.Count; i++) {
                if (current != null && !current.isSuperTypeMethod) {
                    break;
                }

                object baseTypeObj = mro[i];

                DynamicType baseType = baseTypeObj as DynamicType;
                if (baseType == null) {
                    System.Diagnostics.Debug.Assert(baseTypeObj is IPythonType);
                    continue;
                }
                baseType.Initialize();
                current = (MethodWrapper)myField.GetValue(baseType);
            }

            if (current != null) UpdateFromBase(current);
        }

        private void UpdateFromBase(MethodWrapper mw) {
            func = mw.func;
            funcAsFunc = mw.func as BuiltinFunction;
            isObjectMethod = mw.isObjectMethod;
            isBuiltinMethod = mw.isBuiltinMethod;
            isSuperTypeMethod = true;
        }

        public object Invoke(object self) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            if (funcAsFunc != null) return funcAsFunc.Call(self);
            return Ops.Call(Ops.GetDescriptor(func, self, pythonType));
        }
        public object Invoke(object self, object arg1) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            if (funcAsFunc != null) return funcAsFunc.Call(self, arg1);
            return Ops.Call(Ops.GetDescriptor(func, self, pythonType), arg1);
        }
        public object Invoke(object self, object arg1, object arg2) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            if (funcAsFunc != null) return funcAsFunc.Call(self, arg1, arg2);
            return Ops.Call(Ops.GetDescriptor(func, self, pythonType), arg1, arg2);
        }
        public object Invoke(object self, object arg1, object arg2, object arg3) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            if (funcAsFunc != null) return funcAsFunc.Call(self, arg1, arg2, arg3);
            return Ops.Call(Ops.GetDescriptor(func, self, pythonType), arg1, arg2, arg3);
        }

        public bool IsObjectMethod() {
            return isObjectMethod;
        }

        /// <summary>
        /// Called from generated code
        /// </summary>
        public bool IsBuiltinMethod() {
            return isBuiltinMethod;
        }

        public bool IsSuperTypeMethod() {
            return isSuperTypeMethod;
        }


        #region ICallable Members

        [PythonName("__call__")]
        public object Call(params object[] args) {
            if (func == null)
                throw Ops.AttributeErrorForMissingAttribute(pythonType.__name__.ToString(), name);
            return Ops.Call(func, args);
        }

        #endregion

        #region IFancyCallable Members

        [PythonName("__call__")]
        public object Call(ICallerContext context, object[] args, string[] names) {
            if (func == null) throw Ops.AttributeError("{0} not defined on instance of {1}", name, pythonType.__name__);
            return Ops.Call(context, func, args, names);
        }

        #endregion

        #region IDataDescriptor Members

        [PythonName("__get__")]
        public object GetAttribute(object instance, object owner) {
            if (func == null)
                throw Ops.AttributeErrorForMissingAttribute(pythonType.__name__.ToString(), name);
            if (instance != null) return new Method(func, instance, owner);
            else return func;
        }

        [PythonName("__set__")]
        public bool SetAttribute(object instance, object value) {
            SetDeclaredMethod(value);
            pythonType.UpdateSubclasses();
            return true;
        }
        [PythonName("__delete__")]
        public bool DeleteAttribute(object instance) {
            if (isSuperTypeMethod) {
                throw new NotImplementedException();
            }

            func = null;
            funcAsFunc = null;
            isSuperTypeMethod = true;
            UpdateFromBases(pythonType.MethodResolutionOrder);
            pythonType.UpdateSubclasses();
            return true;
        }

        #endregion
    }

}
