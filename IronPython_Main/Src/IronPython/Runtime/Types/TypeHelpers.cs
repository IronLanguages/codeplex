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
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Types {
    class TypeHelpers {
        public static IDictionary<object, object> GetAttrDictWithCustomDict(PythonType dt, CodeContext context, ICustomMembers self, IAttributesCollection selfDict) {
            Debug.Assert(dt.IsInstanceOfType(self));

            // Get the attributes from the instance
            PythonDictionary res = new PythonDictionary(selfDict);

            // Add the attributes from the type
            IAttributesCollection iac = dt.GetMemberDictionary(context);
            foreach (KeyValuePair<object, object> pair in iac.AsObjectKeyedDictionary()) {
                res.Add(pair);
            } 

            return res;
        }

        public static void SetAttrWithCustomDict(PythonType dt, CodeContext context, ICustomMembers self, IAttributesCollection selfDict, SymbolId name, object value) {
            Debug.Assert(dt.IsInstanceOfType(self), String.Format("{0} not instance of {1}", self, dt.Name));

            if (name == Symbols.Dict)
                throw PythonOps.AttributeErrorForReadonlyAttribute(dt.Name, name);

            PythonTypeSlot dts;
            if (dt.TryLookupSlot(context, name, out dts)) {
                dts.TrySetValue(context, self, dt, value);
            } else {
                selfDict[name] = value;
            }
        }

        public static bool DeleteAttrWithCustomDict(PythonType dt, CodeContext context, ICustomMembers self, IAttributesCollection selfDict, SymbolId name) {
            Debug.Assert(dt.IsInstanceOfType(self));

            if (name == Symbols.Dict)
                throw PythonOps.AttributeErrorForReadonlyAttribute(dt.Name.ToString(), name);

            object value;
            if (selfDict.TryGetValue(name, out value)) {
                if (value == Uninitialized.Instance) return false;

                selfDict.Remove(name);
                return true;
            }

            PythonTypeSlot dummy;
            if (dt.TryLookupSlot(context, name, out dummy)) {
                selfDict[name] = Uninitialized.Instance;
                return true;
            } 

            return false;            
        }

        public static PythonType GetDeclaringType(MemberInfo member) {
            return GetDeclaringType(member.DeclaringType);
        }

        public static PythonType GetDeclaringType(Type declaringType) {
            if(ExtensionTypeAttribute.IsExtensionType(declaringType)) {
                // declaringType is an Ops type
                return DynamicHelpers.GetPythonTypeFromType(ExtensionTypeAttribute.GetExtendedTypeFromExtension(declaringType));
            } else {
                return DynamicHelpers.GetPythonTypeFromType(declaringType);
            }
        }

        public static void GetKeywordArgs(IAttributesCollection dict, object[] args, out object[] finalArgs, out string[] names) {
            finalArgs = new object[args.Length + dict.Count];
            Array.Copy(args, finalArgs, args.Length);
            names = new string[dict.Count];
            int i = 0;
            foreach (KeyValuePair<object, object> kvp in (IDictionary<object, object>)dict) {
                names[i] = (string)kvp.Key;
                finalArgs[i + args.Length] = kvp.Value;
                i++;
            }
        }

        public static string ReprMethod(object self) {
            return string.Format("<{0} object at {1}>", PythonTypeOps.GetName(self), PythonOps.HexId(self));
        }

        public static Type[] GetTypesFromTuple(object index) {
            PythonTuple typesTuple = index as PythonTuple;
            Type[] types;
            if (typesTuple != null) {
                types = new Type[typesTuple.Count];
                int i = 0;
                foreach (object t in typesTuple) {
                    types[i++] = Converter.ConvertToType(t);
                }
            } else {
                types = new Type[1];
                types[0] = Converter.ConvertToType(index);
            }

            return types;
        }


    }
}
