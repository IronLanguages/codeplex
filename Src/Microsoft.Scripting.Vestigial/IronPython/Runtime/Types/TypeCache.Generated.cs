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
using Microsoft.Scripting;
using Microsoft.Scripting.Math;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    public static class TypeCache {
        #region Generated TypeCache Storage

        // *** BEGIN GENERATED CODE ***

        private static DynamicType array, builtinfunction, pythondictionary, frozensetcollection, pythonfunction, builtin, generator, obj, setcollection, dynamictype, str, systemstate, tuple, weakreference, list, pythonfile, scriptmodule, method, enumerate, intType, doubleType, biginteger, complex64, super, oldclass, oldinstance, noneType, boolType;

        // *** END GENERATED CODE ***

        #endregion

        #region Generated TypeCache Entries

        // *** BEGIN GENERATED CODE ***

        public static DynamicType Array {
            get {
                if (array == null) array = Ops.GetDynamicTypeFromType(typeof(Array));
                return array;
            }
        }

        public static DynamicType BuiltinFunction {
            get {
                if (builtinfunction == null) builtinfunction = Ops.GetDynamicTypeFromType(typeof(BuiltinFunction));
                return builtinfunction;
            }
        }

        public static DynamicType Dict {
            get {
                if (pythondictionary == null) pythondictionary = Ops.GetDynamicTypeFromType(typeof(PythonDictionary));
                return pythondictionary;
            }
        }

        public static DynamicType FrozenSet {
            get {
                if (frozensetcollection == null) frozensetcollection = Ops.GetDynamicTypeFromType(typeof(FrozenSetCollection));
                return frozensetcollection;
            }
        }

        public static DynamicType Function {
            get {
                if (pythonfunction == null) pythonfunction = Ops.GetDynamicTypeFromType(typeof(PythonFunction));
                return pythonfunction;
            }
        }

        public static DynamicType Builtin {
            get {
                if (builtin == null) builtin = Ops.GetDynamicTypeFromType(typeof(Builtin));
                return builtin;
            }
        }

        public static DynamicType Generator {
            get {
                if (generator == null) generator = Ops.GetDynamicTypeFromType(typeof(Generator));
                return generator;
            }
        }

        public static DynamicType Object {
            get {
                if (obj == null) obj = Ops.GetDynamicTypeFromType(typeof(Object));
                return obj;
            }
        }

        public static DynamicType Set {
            get {
                if (setcollection == null) setcollection = Ops.GetDynamicTypeFromType(typeof(SetCollection));
                return setcollection;
            }
        }

        public static DynamicType DynamicType {
            get {
                if (dynamictype == null) dynamictype = Ops.GetDynamicTypeFromType(typeof(DynamicType));
                return dynamictype;
            }
        }

        public static DynamicType String {
            get {
                if (str == null) str = Ops.GetDynamicTypeFromType(typeof(String));
                return str;
            }
        }

        public static DynamicType SystemState {
            get {
                if (systemstate == null) systemstate = Ops.GetDynamicTypeFromType(typeof(SystemState));
                return systemstate;
            }
        }

        public static DynamicType Tuple {
            get {
                if (tuple == null) tuple = Ops.GetDynamicTypeFromType(typeof(Tuple));
                return tuple;
            }
        }

        public static DynamicType WeakReference {
            get {
                if (weakreference == null) weakreference = Ops.GetDynamicTypeFromType(typeof(WeakReference));
                return weakreference;
            }
        }

        public static DynamicType List {
            get {
                if (list == null) list = Ops.GetDynamicTypeFromType(typeof(List));
                return list;
            }
        }

        public static DynamicType PythonFile {
            get {
                if (pythonfile == null) pythonfile = Ops.GetDynamicTypeFromType(typeof(PythonFile));
                return pythonfile;
            }
        }

        public static DynamicType Module {
            get {
                if (scriptmodule == null) scriptmodule = Ops.GetDynamicTypeFromType(typeof(ScriptModule));
                return scriptmodule;
            }
        }

        public static DynamicType Method {
            get {
                if (method == null) method = Ops.GetDynamicTypeFromType(typeof(Method));
                return method;
            }
        }

        public static DynamicType Enumerate {
            get {
                if (enumerate == null) enumerate = Ops.GetDynamicTypeFromType(typeof(Enumerate));
                return enumerate;
            }
        }

        public static DynamicType Int32 {
            get {
                if (intType == null) intType = Ops.GetDynamicTypeFromType(typeof(Int32));
                return intType;
            }
        }

        public static DynamicType Double {
            get {
                if (doubleType == null) doubleType = Ops.GetDynamicTypeFromType(typeof(Double));
                return doubleType;
            }
        }

        public static DynamicType BigInteger {
            get {
                if (biginteger == null) biginteger = Ops.GetDynamicTypeFromType(typeof(BigInteger));
                return biginteger;
            }
        }

        public static DynamicType Complex64 {
            get {
                if (complex64 == null) complex64 = Ops.GetDynamicTypeFromType(typeof(Complex64));
                return complex64;
            }
        }

        public static DynamicType Super {
            get {
                if (super == null) super = Ops.GetDynamicTypeFromType(typeof(Super));
                return super;
            }
        }

        public static DynamicType OldClass {
            get {
                if (oldclass == null) oldclass = Ops.GetDynamicTypeFromType(typeof(OldClass));
                return oldclass;
            }
        }

        public static DynamicType OldInstance {
            get {
                if (oldinstance == null) oldinstance = Ops.GetDynamicTypeFromType(typeof(OldInstance));
                return oldinstance;
            }
        }

        public static DynamicType None {
            get {
                if (noneType == null) noneType = Ops.GetDynamicTypeFromType(typeof(None));
                return noneType;
            }
        }

        public static DynamicType Boolean {
            get {
                if (boolType == null) boolType = Ops.GetDynamicTypeFromType(typeof(Boolean));
                return boolType;
            }
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
