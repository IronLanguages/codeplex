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

using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Base class for properties backed by methods.  These include our slot properties,
    /// indexers, and normal properties.  This class provides the storage of these as well
    /// as the storage of our optimized getter/setter methods, documentation for the property,
    /// etc...
    /// </summary>
    public abstract class ReflectedGetterSetter : PythonTypeSlot {
        private readonly MethodInfo[]/*!*/ _getter, _setter;
        private readonly NameType _nameType;

        protected ReflectedGetterSetter(MethodInfo[]/*!*/ getter, MethodInfo[]/*!*/ setter, NameType nt) {
            Debug.Assert(getter != null);
            Debug.Assert(setter != null);

            _getter = RemoveNullEntries(getter);
            _setter = RemoveNullEntries(setter);
            _nameType = nt;
        }

        protected ReflectedGetterSetter(ReflectedGetterSetter from) {
            _getter = from._getter;
            _setter = from._setter;
            _nameType = from._nameType;
        }

        public abstract string Name {
            get;                
        }

        public abstract Type DeclaringType {
            get;
        }

        public MethodInfo[]/*!*/ Getter {
            get {
                return _getter;
            }
        }

        public MethodInfo[]/*!*/ Setter {
            get {
                return _setter;
            }
        }

        protected NameType NameType {
            get {
                return _nameType;
            }
        }

        public object CallGetter(CodeContext context, object instance, object[] args) {
            if (NeedToReturnProperty(instance, Getter)) {
                return this;
            }

            if (Getter.Length == 0) {
                throw new MissingMemberException("unreadable property");
            }

            MethodBinder binder = MethodBinder.MakeBinder(context.LanguageContext.Binder, Name, Getter);

            if (instance != null) {                
                return binder.CallInstanceReflected(context, instance, args);
            }

            return binder.CallReflected(context, CallTypes.None, args);
        }

        private static bool NeedToReturnProperty(object instance, MethodInfo[] mis) {
            if (instance == null) {
                if (mis.Length == 0) {
                    return true;
                }

                foreach (MethodInfo mi in mis) {
                    if (!mi.IsStatic) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CallSetter(CodeContext context, object instance, object[] args, object value) {
            if (NeedToReturnProperty(instance, Setter)) {
                return false;
            }

            MethodBinder binder = MethodBinder.MakeBinder(context.LanguageContext.Binder, Name, Setter);

            if (args.Length == 0) {
                if (instance != null) {
                    binder.CallInstanceReflected(context, instance, value);
                } else {
                    binder.CallReflected(context, CallTypes.None, value);
                }
            } else {
                args = ArrayUtils.Append(args, value); 

                if (instance != null) {
                    binder.CallInstanceReflected(context, instance, args);
                } else {
                    binder.CallReflected(context, CallTypes.None, args);
                }
            }

            return true;
        }

        internal override bool IsVisible(CodeContext context, PythonType owner) {
            return _nameType == NameType.PythonProperty || PythonOps.IsClsVisible(context);
        }

        internal override bool IsAlwaysVisible {
            get {
                return _nameType == NameType.PythonProperty;
            }
        }

        private MethodInfo[] RemoveNullEntries(MethodInfo[] mis) {
            List<MethodInfo> res = null;
            for(int i = 0; i<mis.Length; i++) {
                if (mis[i] == null) {
                    if (res == null) {
                        res = new List<MethodInfo>();
                        for (int j = 0; j < i; j++) {
                            res.Add(mis[j]);
                        }
                    }
                } else if(res != null) {
                    res.Add(mis[i]);
                }
            }

            if (res != null) {
                return res.ToArray();
            }
            return mis;
        }
    }
}
