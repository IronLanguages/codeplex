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

using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    public static class TypeCache {
        #region Generated TypeCache Storage

        // *** BEGIN GENERATED CODE ***

        private static PythonType array, builtinfunction, pythondictionary, frozensetcollection, pythonfunction, builtin, generator, obj, setcollection, pythontype, str, systemstate, pythontuple, weakreference, list, pythonfile, scriptmodule, method, enumerate, intType, doubleType, biginteger, complex64, super, oldclass, oldinstance, noneType, boolType;

        // *** END GENERATED CODE ***

        #endregion

        #region Generated TypeCache Entries

        // *** BEGIN GENERATED CODE ***

        public static PythonType Array {
            get {
                if (array == null) array = DynamicHelpers.GetPythonTypeFromType(typeof(Array));
                return array;
            }
        }

        public static PythonType BuiltinFunction {
            get {
                if (builtinfunction == null) builtinfunction = DynamicHelpers.GetPythonTypeFromType(typeof(BuiltinFunction));
                return builtinfunction;
            }
        }

        public static PythonType Dict {
            get {
                if (pythondictionary == null) pythondictionary = DynamicHelpers.GetPythonTypeFromType(typeof(PythonDictionary));
                return pythondictionary;
            }
        }

        public static PythonType FrozenSet {
            get {
                if (frozensetcollection == null) frozensetcollection = DynamicHelpers.GetPythonTypeFromType(typeof(FrozenSetCollection));
                return frozensetcollection;
            }
        }

        public static PythonType Function {
            get {
                if (pythonfunction == null) pythonfunction = DynamicHelpers.GetPythonTypeFromType(typeof(PythonFunction));
                return pythonfunction;
            }
        }

        public static PythonType Builtin {
            get {
                if (builtin == null) builtin = DynamicHelpers.GetPythonTypeFromType(typeof(Builtin));
                return builtin;
            }
        }

        public static PythonType Generator {
            get {
                if (generator == null) generator = DynamicHelpers.GetPythonTypeFromType(typeof(Generator));
                return generator;
            }
        }

        public static PythonType Object {
            get {
                if (obj == null) obj = DynamicHelpers.GetPythonTypeFromType(typeof(Object));
                return obj;
            }
        }

        public static PythonType Set {
            get {
                if (setcollection == null) setcollection = DynamicHelpers.GetPythonTypeFromType(typeof(SetCollection));
                return setcollection;
            }
        }

        public static PythonType PythonType {
            get {
                if (pythontype == null) pythontype = DynamicHelpers.GetPythonTypeFromType(typeof(PythonType));
                return pythontype;
            }
        }

        public static PythonType String {
            get {
                if (str == null) str = DynamicHelpers.GetPythonTypeFromType(typeof(String));
                return str;
            }
        }

        public static PythonType SystemState {
            get {
                if (systemstate == null) systemstate = DynamicHelpers.GetPythonTypeFromType(typeof(SystemState));
                return systemstate;
            }
        }

        public static PythonType PythonTuple {
            get {
                if (pythontuple == null) pythontuple = DynamicHelpers.GetPythonTypeFromType(typeof(PythonTuple));
                return pythontuple;
            }
        }

        public static PythonType WeakReference {
            get {
                if (weakreference == null) weakreference = DynamicHelpers.GetPythonTypeFromType(typeof(WeakReference));
                return weakreference;
            }
        }

        public static PythonType List {
            get {
                if (list == null) list = DynamicHelpers.GetPythonTypeFromType(typeof(List));
                return list;
            }
        }

        public static PythonType PythonFile {
            get {
                if (pythonfile == null) pythonfile = DynamicHelpers.GetPythonTypeFromType(typeof(PythonFile));
                return pythonfile;
            }
        }

        public static PythonType Module {
            get {
                if (scriptmodule == null) scriptmodule = DynamicHelpers.GetPythonTypeFromType(typeof(ScriptModule));
                return scriptmodule;
            }
        }

        public static PythonType Method {
            get {
                if (method == null) method = DynamicHelpers.GetPythonTypeFromType(typeof(Method));
                return method;
            }
        }

        public static PythonType Enumerate {
            get {
                if (enumerate == null) enumerate = DynamicHelpers.GetPythonTypeFromType(typeof(Enumerate));
                return enumerate;
            }
        }

        public static PythonType Int32 {
            get {
                if (intType == null) intType = DynamicHelpers.GetPythonTypeFromType(typeof(Int32));
                return intType;
            }
        }

        public static PythonType Double {
            get {
                if (doubleType == null) doubleType = DynamicHelpers.GetPythonTypeFromType(typeof(Double));
                return doubleType;
            }
        }

        public static PythonType BigInteger {
            get {
                if (biginteger == null) biginteger = DynamicHelpers.GetPythonTypeFromType(typeof(BigInteger));
                return biginteger;
            }
        }

        public static PythonType Complex64 {
            get {
                if (complex64 == null) complex64 = DynamicHelpers.GetPythonTypeFromType(typeof(Complex64));
                return complex64;
            }
        }

        public static PythonType Super {
            get {
                if (super == null) super = DynamicHelpers.GetPythonTypeFromType(typeof(Super));
                return super;
            }
        }

        public static PythonType OldClass {
            get {
                if (oldclass == null) oldclass = DynamicHelpers.GetPythonTypeFromType(typeof(OldClass));
                return oldclass;
            }
        }

        public static PythonType OldInstance {
            get {
                if (oldinstance == null) oldinstance = DynamicHelpers.GetPythonTypeFromType(typeof(OldInstance));
                return oldinstance;
            }
        }

        public static PythonType None {
            get {
                if (noneType == null) noneType = DynamicHelpers.GetPythonTypeFromType(typeof(None));
                return noneType;
            }
        }

        public static PythonType Boolean {
            get {
                if (boolType == null) boolType = DynamicHelpers.GetPythonTypeFromType(typeof(Boolean));
                return boolType;
            }
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}
