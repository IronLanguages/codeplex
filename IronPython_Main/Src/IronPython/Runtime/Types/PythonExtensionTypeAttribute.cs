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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

using IronPython.Compiler;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    // TODO: Make private
    internal partial class PythonExtensionTypeAttribute : ExtensionTypeAttribute {
        private PythonType _type;
        private bool _extension;
        private bool _enableDerivation;
        private Type _derivationType;
        internal static Dictionary<SymbolId, OperatorMapping> _pythonOperatorTable;
        private static Dictionary<OperatorMapping, SymbolId> _reverseOperatorTable;


        static PythonExtensionTypeAttribute() {
            InitializeOperatorTable();
        }

        public PythonExtensionTypeAttribute(Type extends, Type extensionType)
            : base(extends, extensionType) {

            Initialize();
            PythonBinder.RegisterType(extends, extensionType);
        }

        private void Initialize() {
            string name;
            if (!PythonTypeCustomizer.SystemTypes.TryGetValue(Extends, out name)) {
                NameConverter.TryGetName(Extends, out name);

                PythonTypeCustomizer.SystemTypes[Extends] = name;
            }

            _type = DynamicHelpers.GetPythonTypeFromType(base.Extends);
            _type.IsPythonType = true;
            _extension = true;
        }

        internal PythonExtensionTypeAttribute(PythonType pythonType)
            : base(pythonType.UnderlyingSystemType, null) {
            _type = pythonType;
            _type.IsPythonType = true;
        }

        internal ExtensionNameTransformer Transformer {
            get {
                return PythonNameTransformer;
            }
        }


        public bool EnableDerivation {
            get {
                return _enableDerivation;
            }
            set {
                _enableDerivation = value;
            }
        }

        /// <summary>
        ///  TODO: Remove me and need to have custom derivation types.
        /// </summary>
        public Type DerivationType {
            get {
                return _derivationType;
            }
            set {
                _derivationType = value;
            }
        }

        internal IEnumerable<TransformedName> PythonNameTransformer(MemberInfo mi, TransformReason reason) {
            switch (reason) {
                case TransformReason.Method:
                    Debug.Assert(mi.MemberType == MemberTypes.Method);
                    return MethodEnumerator((MethodInfo)mi);
                case TransformReason.Field: return FieldEnumerator((FieldInfo)mi);
                case TransformReason.Operator: return OperatorEnumerator(mi);
                case TransformReason.Property: return PropertyEnumerator(mi);
                case TransformReason.NestedType: return TypeEnumerator(mi);
                default:
                    Debug.Assert(false);
                    return EmptyEnumerator();
            }
        }

        private IEnumerable<TransformedName> TypeEnumerator(MemberInfo mi) {
            object[] attr = mi.GetCustomAttributes(typeof(PythonTypeAttribute), false);
            if (attr.Length > 0) {
                yield return new TransformedName(((PythonTypeAttribute)attr[0]).Name, ContextId.Empty);
            }
            yield return new TransformedName(mi.Name, ContextId.Empty);
        }

        private IEnumerable<TransformedName> FieldEnumerator(FieldInfo fi) {
            string name;
            NameType nt = NameConverter.TryGetName(_type, fi, out name);
            if (nt != NameType.None) {
                yield return new TransformedName(name, DefaultContext.Id);
            }
        }

        private IEnumerable<TransformedName> PropertyEnumerator(MemberInfo mi) {
           string name;
           NameType nt;
           switch (mi.MemberType) {
                // For extension properties we only get the MethodInfo
                case MemberTypes.Property:
                    PropertyInfo pi = (PropertyInfo)mi;
                    nt = NameConverter.TryGetName(_type, pi, pi.GetGetMethod(true) ?? pi.GetSetMethod(true), out name);
                    if ((nt & NameType.Python) != 0) {
                        yield return new TransformedName(name, DefaultContext.Id);
                    } else if (nt != NameType.None) {
                        yield return new TransformedName(name, ContextId.Empty);
                    }
                    break;
                case MemberTypes.Method:
                    MethodInfo methinfo = (MethodInfo)mi;
                    ExtensionPropertyInfo epi = new ExtensionPropertyInfo(_type, methinfo);

                    nt = NameConverter.TryGetName(_type, epi, methinfo, out name);
                    if ((nt & NameType.Python) != 0) {
                        // no PythonName, remove Get or Set from name.
                        if (name == methinfo.Name) name = ExtensionPropertyInfo.GetName(methinfo);

                        yield return new TransformedName(name, DefaultContext.Id);
                    }
                    break;
            }
        }

        private IEnumerable<TransformedName> MethodEnumerator(MethodInfo mi) {
            switch (mi.MemberType) {
                case MemberTypes.Method:
                    if (mi.IsSpecialName) {
                        bool regular, reverse;
                        OperatorMapping om = GetRegularReverse(_type.UnderlyingSystemType, mi, out regular, out reverse);
   
                        // added during AddOperator
                        if (regular || reverse) yield break;
                    }

                    string name;
                    NameType nt = NameConverter.TryGetName(_type, (MethodInfo)mi, out name);
                    if ((nt & NameType.Python) != 0) {
                        if (nt == NameType.ClassMethod) {
                            yield return new TransformedName(name, CreateClassMethod, DefaultContext.Id);
                        } else {
                            yield return new TransformedName(name, DefaultContext.Id);
                        }

                        // we do this weird thing where we publish our extension methods in the CLR context under their CLR names.
                        // do we want to keep doing this???
                        if (mi.Name != name && _extension) {
                            yield return new TransformedName(mi.Name, ContextId.Empty);
                        }
                    } else if (nt != NameType.None && _extension) {
                        yield return new TransformedName(name, ContextId.Empty);
                    }
                    break;
                default:
                    break;
            }

            yield break;
        }

        private IEnumerable<TransformedName> OperatorEnumerator(MemberInfo mi) {
            MethodInfo method = mi as MethodInfo;

            OperatorMapping opmap;

            if (ReflectedTypeBuilder.OperatorTable.TryGetValue(mi.Name, out opmap)) {
                Debug.Assert(opmap != null);

                bool regular, reverse;
                if (method != null && opmap.IsReversible) {
                    GetRegularReverse(_type.UnderlyingSystemType, method, out regular, out reverse);
                } else {
                    // TODO: Support more arbitrary slot types, currently we're mainly just using call
                    regular = true;
                    reverse = false;
                }

                SymbolId sym;
                if (regular) {
                    if (_reverseOperatorTable.TryGetValue(opmap, out sym)) {
                        // Python supports the operator and exposes it under a name
                        yield return new TransformedName(SymbolTable.IdToString(sym), opmap, DefaultContext.Id);
                    } else {
                        // we support the operator but don't have a name for it
                        yield return new TransformedName(opmap, DefaultContext.Id);
                    }
                }

                if (reverse) {
                    opmap = opmap.GetReversed();
                    if (_reverseOperatorTable.TryGetValue(opmap, out sym)) {
                        yield return new TransformedName(SymbolTable.IdToString(sym), opmap, DefaultContext.Id);
                    } else {
                        yield return new TransformedName(opmap, DefaultContext.Id);
                    }
                }
            } else {
                string name;
                NameType nt;
                switch(mi.MemberType) {
                    case MemberTypes.Method: nt = NameConverter.TryGetName(_type, (MethodInfo)mi, out name); break;
                    case MemberTypes.Field: nt = NameConverter.TryGetName(_type, (FieldInfo)mi, out name); break;
                    default: throw new InvalidOperationException();
                }
                                
                if ((nt & NameType.Python) != 0) {
                    if (_pythonOperatorTable.TryGetValue(SymbolTable.StringToId(name), out opmap)) {
                        Debug.Assert(opmap != null);

                        yield return new TransformedName(name, opmap, DefaultContext.Id);
                    }

                    // we do this weird thing where we publish our extension methods in the CLR context under their CLR names.
                    // do we want to keep doing this???
                    if (_extension && mi.Name != name) {
                        yield return new TransformedName(mi.Name, ContextId.Empty);
                    }
                } else if (_extension) {
                    yield return new TransformedName(mi.Name, ContextId.Empty);
                }
            }


            yield break;
        }

        private static IEnumerable<TransformedName> EmptyEnumerator() {
            yield break;
        }

        internal static OperatorMapping GetRegularReverse(Type declaringType, MethodInfo mi, out bool regular, out bool reverse) {
            OperatorMapping om;
            regular = reverse = false;

            if (ReflectedTypeBuilder.OperatorTable.TryGetValue(mi.Name, out om)) {
                ParameterInfo[] parms = mi.GetParameters();
                int nparams = parms.Length;
                int ctxOffset = 0;
                if (parms.Length > 0 && parms[0].ParameterType == typeof(CodeContext)) {
                    ctxOffset++;
                    nparams--;
                }

                if (mi.IsStatic) {
                    if (nparams < om.MinArgs || nparams > om.MaxArgs) return null;

                    regular = parms.Length > ctxOffset && IsThis(declaringType, parms[ctxOffset]);
                    
                    if (om.IsReversible && !CompilerHelpers.IsComparisonOperator(om.Operator)) {
                        Operators revOp = CompilerHelpers.OperatorToReverseOperator(om.Operator);
                        reverse = (!regular || revOp != om.Operator) && parms.Length > ctxOffset+1 && IsThis(declaringType, parms[ctxOffset+1]);
                    }
                    if (!regular && !reverse) {
                        // Treat fully object signatures as regular:
                        // eg: EnumOps.BitwiseOr(object x, object y)
                        regular = true;
                        for (int i = ctxOffset; i < parms.Length; i++) {
                            if (parms[i].ParameterType != typeof(object)) {
                                regular = false;
                                break;
                            }
                        }
                    }
                } else {
                    nparams++;
                    if (nparams < om.MinArgs || nparams > om.MaxArgs) return null;

                    regular = true;
                    if (om.IsBinary && !CompilerHelpers.IsComparisonOperator(om.Operator)) {
                        Operators revOp = CompilerHelpers.OperatorToReverseOperator(om.Operator);
                        reverse = (revOp != om.Operator) && parms.Length > ctxOffset && parms[ctxOffset].ParameterType == declaringType;
                    }
                }
            } 

            return om;
        }

        private static bool IsThis(Type declaringType, ParameterInfo param) {
            return  param.ParameterType == declaringType || param.IsDefined(typeof(StaticThisAttribute), false);
        }

        internal PythonTypeSlot CreateClassMethod(MemberInfo mi, PythonTypeSlot existing) {
            MethodInfo method = mi as MethodInfo;
            if (existing != null) {
                ClassMethodDescriptor cm = existing as ClassMethodDescriptor;

                Debug.Assert(cm != null,
                    String.Format("Replacing existing method {0} on {1}\nExisting Method: {2}", mi.Name, mi.DeclaringType, existing));

                cm._func.AddMethod(method);
                return existing;
            } else {
                object [] attrs = mi.GetCustomAttributes(typeof(PythonClassMethodAttribute), false);
                Debug.Assert(attrs.Length == 1);

                BuiltinFunction classFunc = BuiltinFunction.MakeMethod(((PythonClassMethodAttribute)attrs[0]).Name,
                    method, 
                    FunctionType.Function | FunctionType.AlwaysVisible);
                return new ClassMethodDescriptor(classFunc);
            }
        }

        internal static Dictionary<OperatorMapping, SymbolId> ReverseOperatorTable {
            get { return PythonExtensionTypeAttribute._reverseOperatorTable; }
            set { PythonExtensionTypeAttribute._reverseOperatorTable = value; }
        }


    }
}
