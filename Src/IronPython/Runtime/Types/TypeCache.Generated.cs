/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using Microsoft.Scripting.Types;

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    public static class TypeCache {
        #region Generated TypeCache Storage

        // *** BEGIN GENERATED CODE ***

        private static DynamicType array, builtinfunction, pythondictionary, frozensetcollection, pythonfunction, builtin, generator, obj, setcollection, dynamictype, str, systemstate, pythontuple, weakreference, list, pythonfile, scriptmodule, method, enumerate, intType, doubleType, biginteger, complex64, super, oldclass, oldinstance, noneType, boolType;

        // *** END GENERATED CODE ***

        #endregion

        #region Generated TypeCache Entries

        // *** BEGIN GENERATED CODE ***

        public static DynamicType Array {
            get {
                if (array == null) array = DynamicHelpers.GetDynamicTypeFromType(typeof(Array));
                return array;
            }
        }

        public static DynamicType BuiltinFunction {
            get {
                if (builtinfunction == null) builtinfunction = DynamicHelpers.GetDynamicTypeFromType(typeof(BuiltinFunction));
                return builtinfunction;
            }
        }

        public static DynamicType Dict {
            get {
                if (pythondictionary == null) pythondictionary = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonDictionary));
                return pythondictionary;
            }
        }

        public static DynamicType FrozenSet {
            get {
                if (frozensetcollection == null) frozensetcollection = DynamicHelpers.GetDynamicTypeFromType(typeof(FrozenSetCollection));
                return frozensetcollection;
            }
        }

        public static DynamicType Function {
            get {
                if (pythonfunction == null) pythonfunction = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonFunction));
                return pythonfunction;
            }
        }

        public static DynamicType Builtin {
            get {
                if (builtin == null) builtin = DynamicHelpers.GetDynamicTypeFromType(typeof(Builtin));
                return builtin;
            }
        }

        public static DynamicType Generator {
            get {
                if (generator == null) generator = DynamicHelpers.GetDynamicTypeFromType(typeof(Generator));
                return generator;
            }
        }

        public static DynamicType Object {
            get {
                if (obj == null) obj = DynamicHelpers.GetDynamicTypeFromType(typeof(Object));
                return obj;
            }
        }

        public static DynamicType Set {
            get {
                if (setcollection == null) setcollection = DynamicHelpers.GetDynamicTypeFromType(typeof(SetCollection));
                return setcollection;
            }
        }

        public static DynamicType DynamicType {
            get {
                if (dynamictype == null) dynamictype = DynamicHelpers.GetDynamicTypeFromType(typeof(DynamicType));
                return dynamictype;
            }
        }

        public static DynamicType String {
            get {
                if (str == null) str = DynamicHelpers.GetDynamicTypeFromType(typeof(String));
                return str;
            }
        }

        public static DynamicType SystemState {
            get {
                if (systemstate == null) systemstate = DynamicHelpers.GetDynamicTypeFromType(typeof(SystemState));
                return systemstate;
            }
        }

        public static DynamicType PythonTuple {
            get {
                if (pythontuple == null) pythontuple = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonTuple));
                return pythontuple;
            }
        }

        public static DynamicType WeakReference {
            get {
                if (weakreference == null) weakreference = DynamicHelpers.GetDynamicTypeFromType(typeof(WeakReference));
                return weakreference;
            }
        }

        public static DynamicType List {
            get {
                if (list == null) list = DynamicHelpers.GetDynamicTypeFromType(typeof(List));
                return list;
            }
        }

        public static DynamicType PythonFile {
            get {
                if (pythonfile == null) pythonfile = DynamicHelpers.GetDynamicTypeFromType(typeof(PythonFile));
                return pythonfile;
            }
        }

        public static DynamicType Module {
            get {
                if (scriptmodule == null) scriptmodule = DynamicHelpers.GetDynamicTypeFromType(typeof(ScriptModule));
                return scriptmodule;
            }
        }

        public static DynamicType Method {
            get {
                if (method == null) method = DynamicHelpers.GetDynamicTypeFromType(typeof(Method));
                return method;
            }
        }

        public static DynamicType Enumerate {
            get {
                if (enumerate == null) enumerate = DynamicHelpers.GetDynamicTypeFromType(typeof(Enumerate));
                return enumerate;
            }
        }

        public static DynamicType Int32 {
            get {
                if (intType == null) intType = DynamicHelpers.GetDynamicTypeFromType(typeof(Int32));
                return intType;
            }
        }

        public static DynamicType Double {
            get {
                if (doubleType == null) doubleType = DynamicHelpers.GetDynamicTypeFromType(typeof(Double));
                return doubleType;
            }
        }

        public static DynamicType BigInteger {
            get {
                if (biginteger == null) biginteger = DynamicHelpers.GetDynamicTypeFromType(typeof(BigInteger));
                return biginteger;
            }
        }

        public static DynamicType Complex64 {
            get {
                if (complex64 == null) complex64 = DynamicHelpers.GetDynamicTypeFromType(typeof(Complex64));
                return complex64;
            }
        }

        public static DynamicType Super {
            get {
                if (super == null) super = DynamicHelpers.GetDynamicTypeFromType(typeof(Super));
                return super;
            }
        }

        public static DynamicType OldClass {
            get {
                if (oldclass == null) oldclass = DynamicHelpers.GetDynamicTypeFromType(typeof(OldClass));
                return oldclass;
            }
        }

        public static DynamicType OldInstance {
            get {
                if (oldinstance == null) oldinstance = DynamicHelpers.GetDynamicTypeFromType(typeof(OldInstance));
                return oldinstance;
            }
        }

        public static DynamicType None {
            get {
                if (noneType == null) noneType = DynamicHelpers.GetDynamicTypeFromType(typeof(None));
                return noneType;
            }
        }

        public static DynamicType Boolean {
            get {
                if (boolType == null) boolType = DynamicHelpers.GetDynamicTypeFromType(typeof(Boolean));
                return boolType;
            }
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
