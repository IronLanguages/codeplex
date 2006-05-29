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
using System.Collections.Generic;
using System.Text;
using IronPython.Modules;

namespace IronPython.Runtime {
    public static class TypeCache {
        #region Generated TypeCache Storage

        // *** BEGIN GENERATED CODE ***

        private static ReflectedType builtinfunction, dict, frozensetcollection, pythonfunction, codecs, builtin, generator, setcollection, reflectedtype, str, systemstate, tuple, usertype, weakreference, list, pythonfile, method, enumerate;
        private static PythonType obj, intType;

        // *** END GENERATED CODE ***

        #endregion
        
        #region Generated TypeCache Entries

        // *** BEGIN GENERATED CODE ***

        public static ReflectedType BuiltinFunction {
            get {
                if (builtinfunction == null) builtinfunction = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(BuiltinFunction));
                return builtinfunction;
            }
        }

        public static ReflectedType Dict {
            get {
                if (dict == null) dict = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(Dict));
                return dict;
            }
        }

        public static ReflectedType FrozenSet {
            get {
                if (frozensetcollection == null) frozensetcollection = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(FrozenSetCollection));
                return frozensetcollection;
            }
        }

        public static ReflectedType Function {
            get {
                if (pythonfunction == null) pythonfunction = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(PythonFunction));
                return pythonfunction;
            }
        }

        public static ReflectedType Codecs {
            get {
                if (codecs == null) codecs = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(PythonCodecs));
                return codecs;
            }
        }

        public static ReflectedType Builtin {
            get {
                if (builtin == null) builtin = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(Builtin));
                return builtin;
            }
        }

        public static ReflectedType Generator {
            get {
                if (generator == null) generator = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(Generator));
                return generator;
            }
        }

        public static PythonType Object {
            get {
                if (obj == null) obj = (PythonType)Ops.GetDynamicTypeFromType(typeof(Object));
                return obj;
            }
        }

        public static ReflectedType Set {
            get {
                if (setcollection == null) setcollection = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(SetCollection));
                return setcollection;
            }
        }

        public static ReflectedType ReflectedType {
            get {
                if (reflectedtype == null) reflectedtype = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(ReflectedType));
                return reflectedtype;
            }
        }

        public static ReflectedType String {
            get {
                if (str == null) str = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(String));
                return str;
            }
        }

        public static ReflectedType SystemState {
            get {
                if (systemstate == null) systemstate = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(SystemState));
                return systemstate;
            }
        }

        public static ReflectedType Tuple {
            get {
                if (tuple == null) tuple = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(Tuple));
                return tuple;
            }
        }

        public static ReflectedType UserType {
            get {
                if (usertype == null) usertype = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(UserType));
                return usertype;
            }
        }

        public static ReflectedType WeakReference {
            get {
                if (weakreference == null) weakreference = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(WeakReference));
                return weakreference;
            }
        }

        public static ReflectedType List {
            get {
                if (list == null) list = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(List));
                return list;
            }
        }

        public static ReflectedType PythonFile {
            get {
                if (pythonfile == null) pythonfile = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(PythonFile));
                return pythonfile;
            }
        }

        public static ReflectedType Method {
            get {
                if (method == null) method = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(Method));
                return method;
            }
        }

        public static ReflectedType Enumerate {
            get {
                if (enumerate == null) enumerate = (ReflectedType)Ops.GetDynamicTypeFromType(typeof(Enumerate));
                return enumerate;
            }
        }

        public static PythonType Int32 {
            get {
                if (intType == null) intType = (PythonType)Ops.GetDynamicTypeFromType(typeof(Int32));
                return intType;
            }
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
