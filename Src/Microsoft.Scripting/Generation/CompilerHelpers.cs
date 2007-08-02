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
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection.Emit;

using Microsoft.Scripting.Types;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Generation {
    public static class CompilerHelpers {
        public static MethodAttributes PublicStatic = MethodAttributes.Public | MethodAttributes.Static;
       
        public static string[] GetArgumentNames(ParameterInfo[] parameterInfos) {
            string[] ret = new string[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++) ret[i] = parameterInfos[i].Name;
            return ret;
        }

        public static Type[] GetTypesWithThis(MethodBase mi) {
            Type[] types = Utils.Reflection.GetParameterTypes(mi.GetParameters());
            if(IsStatic(mi)) {
                return types;
            }

            return Utils.Array.Insert(mi.DeclaringType, types);
        }


        public static Type GetReturnType(MethodBase mi) {
            if (mi.IsConstructor) return mi.DeclaringType;
            else return ((MethodInfo)mi).ReturnType;
        }

        public static int GetStaticNumberOfArgs(MethodBase method) {
            if (IsStatic(method)) return method.GetParameters().Length;

            return method.GetParameters().Length + 1;
        }

        public static bool IsParamsMethod(MethodBase method) {
            return IsParamsMethod(method.GetParameters());
        }
        public static bool IsParamsMethod(ParameterInfo[] pis) {
            foreach (ParameterInfo pi in pis) {
                if (IsParamArray(pi)) return true;
            }
            return false;
        }

        public static bool IsParamArray(ParameterInfo parameter) {
            return parameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
        }

        public static bool IsOutParameter(ParameterInfo pi) {
            // not using IsIn/IsOut properties as they are not available in Silverlight:
            return (pi.Attributes & (ParameterAttributes.Out | ParameterAttributes.In)) == ParameterAttributes.Out;
        }

        public static int GetOutAndByRefParameterCount(MethodBase method) {
            int res = 0;
            ParameterInfo[] pis = method.GetParameters();
            for (int i = 0; i < pis.Length; i++) {
                if (IsByRefParameter(pis[i])) res++;
            }
            return res;
        }

        public static bool IsByRefParameter(ParameterInfo pi) {
            // not using IsIn/IsOut properties as they are not available in Silverlight:
            if (pi.ParameterType.IsByRef) return true;

            return (pi.Attributes & (ParameterAttributes.Out)) == ParameterAttributes.Out;
        }

        public static object GetMissingValue(Type type) {
            if (type.IsEnum) return Activator.CreateInstance(type);

            switch (Type.GetTypeCode(type)) {
                default:
                case TypeCode.Object:
                    // struct
                    if (type.IsSealed && type.IsValueType) {
                        return Activator.CreateInstance(type);
                    } else if (type == typeof(object)) {
                        // parameter of type object receives the actual Missing value
                        return Missing.Value;
                    } else if (!type.IsValueType) {
                        return null;
                    } else {
                        throw new ArgumentException(String.Format("Cannot create default value for type {0}", type));
                    }
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.String:
                    return null;
                case TypeCode.Boolean: return false;
                case TypeCode.Char: return '\0';
                case TypeCode.SByte: return (sbyte)0;
                case TypeCode.Byte: return (byte)0;
                case TypeCode.Int16: return (short)0;
                case TypeCode.UInt16: return (ushort)0;
                case TypeCode.Int32: return (int)0;
                case TypeCode.UInt32: return (uint)0;
                case TypeCode.Int64: return 0L;
                case TypeCode.UInt64: return 0UL;
                case TypeCode.Single: return 0.0f;
                case TypeCode.Double: return 0.0D;
                case TypeCode.Decimal: return (decimal)0;
                case TypeCode.DateTime: return DateTime.MinValue;
            }
        }

        public static bool IsStatic(MethodBase mi) {
            return mi.IsConstructor || mi.IsStatic;
        }

        public static T[] MakeRepeatedArray<T>(T item, int count) {
            T[] ret = new T[count];
            for (int i = 0; i < count; i++) ret[i] = item;
            return ret;
        }

        /// <summary>
        /// A helper routine to check if a type can be treated as sealed - i.e. there
        /// can never be a subtype of this given type.  This corresponds to a type
        /// that is either declared "Sealed" or is a ValueType and thus unable to be
        /// extended.
        /// </summary>
        public static bool IsSealed(Type type) {
            return type.IsSealed || type.IsValueType;
        }

        /// <summary>
        /// Will create storage allocator which allocates locals on the CLR stack (in the context of the codeGen).
        /// This doesn't set up allocator for globals. Further initialization needed.
        /// </summary>
        /// <param name="outer">Codegen of the lexically enclosing block.</param>
        /// <param name="codeGen">CodeGen object to use to allocate the locals on the CLR stack.</param>
        /// <returns>New ScopeAllocator</returns>
        public static ScopeAllocator CreateLocalStorageAllocator(CodeGen outer, CodeGen codeGen) {
            LocalStorageAllocator allocator = new LocalStorageAllocator(new LocalSlotFactory(codeGen));
            return new ScopeAllocator(outer.HasAllocator ? outer.Allocator : null, allocator);
        }

        /// <summary>
        /// allocates slots out of a FunctionEnvironment.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static ScopeAllocator CreateFrameAllocator(Slot frame) {
            // Globals
            ScopeAllocator global = new ScopeAllocator(
                null,
                new GlobalNamedAllocator()
            );

            // Locals
            ScopeAllocator ns = new ScopeAllocator(
                global,
                new LocalStorageAllocator(new LocalFrameSlotFactory(frame))
            );
            return ns;
        }


        public static Type[] MakeParamTypeArray(IList<Type> baseParamTypes, ConstantPool constantPool) {
            if (constantPool == null) return new List<Type>(baseParamTypes).ToArray();

            List<Type> ret = new List<Type>();
            ret.Add(constantPool.SlotType);
            ret.AddRange(baseParamTypes);
            return ret.ToArray();
        }

        public static void EmitStackTraceTryBlockStart(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            if (ScriptDomainManager.Options.DynamicStackTraceSupport) {
                // push a try for traceback support
                cg.PushTryBlock();
                cg.BeginExceptionBlock();
            }
        }

        public static void EmitStackTraceFaultBlock(CodeGen cg, string name, string displayName) {
            if (cg == null) throw new ArgumentNullException("cg");
            if (name == null) throw new ArgumentNullException("name");
            if (displayName == null) throw new ArgumentNullException("name");

            if (ScriptDomainManager.Options.DynamicStackTraceSupport) {
                // push a fault block (runs only if there's an exception, doesn't handle the exception)
                cg.PopTargets();
                if (cg.IsDynamicMethod) {
                    cg.BeginCatchBlock(typeof(Exception));
                } else {
                    cg.BeginFaultBlock();
                }

                cg.EmitCodeContext();
                if (cg.IsDynamicMethod) {
                    cg.ConstantPool.AddData(cg.MethodBase).EmitGet(cg);
                } else {
                    cg.Emit(OpCodes.Ldtoken, cg.MethodInfo);
                    cg.EmitCall(typeof(MethodBase), "GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) });
                }
                cg.EmitString(name);
                cg.EmitString(displayName);
                cg.EmitGetCurrentLine();
                cg.EmitCall(typeof(RuntimeHelpers), "UpdateStackTrace");

                // end the exception block
                if (cg.IsDynamicMethod) {
                    cg.Emit(OpCodes.Rethrow);
                }
                cg.EndExceptionBlock();
            }
        }

        #region CodeGen Creation Support

        internal static CodeGen CreateDebuggableDynamicCodeGenerator(CompilerContext context, string name, Type retType, IList<Type> paramTypes, IList<string> paramNames, ConstantPool constantPool) {
            TypeGen tg = ScriptDomainManager.CurrentManager.Snippets.DefineDebuggableType(name, context.SourceUnit);
            CodeGen cg = tg.DefineMethod("Initialize", retType, paramTypes, paramNames, constantPool);

            tg.AddCodeContextField();
            cg.DynamicMethod = true;

            return cg;
        }

        /// <summary>
        /// 
        /// </summary>
        internal static CodeGen CreateDynamicCodeGenerator(string name, Type retType, IList<Type> paramTypes, ConstantPool constantPool) {
            return ScriptDomainManager.CurrentManager.Snippets.Assembly.DefineMethod(name, retType, paramTypes, constantPool);
        }

        /// <summary>
        /// Creates a new CodeGenerator for emitting the given code.
        /// The CodeGenerator is usually tied to a dynamic method
        /// unless debugging has been enabled.
        /// </summary>
        public static CodeGen CreateDynamicCodeGenerator(CompilerContext context) {
            CodeGen cg;

            if (NeedDebuggableDynamicCodeGenerator(context)) {
                cg = CreateDebuggableDynamicCodeGenerator(
                    context,
                    context.SourceUnit.Name,
                    typeof(object),
                    new Type[] { typeof(CodeContext) },
                    null,
                    new ConstantPool()
                );
            } else {
                cg = CreateDynamicCodeGenerator(
                    context.SourceUnit.Name,
                    typeof(object),
                    new Type[] { typeof(CodeContext) },
                    new ConstantPool());

                cg.CacheConstants = false;
            }

            cg.ContextSlot = cg.GetArgumentSlot(0);
            cg.Context = context;

            // Caller wanted dynamic method, we should produce it.
            Debug.Assert(cg.DynamicMethod);

            return cg;
        }

        internal static bool NeedDebuggableDynamicCodeGenerator(CompilerContext context) {
            return context != null && context.SourceUnit.Engine.Options.ClrDebuggingEnabled && context.SourceUnit.IsVisibleToDebugger;
        }

        #endregion

        public static Operators OperatorToReverseOperator(Operators op) {
            switch (op) {
                case Operators.LessThan: return Operators.GreaterThan;
                case Operators.LessThanOrEqual: return Operators.GreaterThanOrEqual;
                case Operators.GreaterThan: return Operators.LessThan;
                case Operators.GreaterThanOrEqual: return Operators.LessThanOrEqual;
                case Operators.Equal: return Operators.Equal;
                case Operators.NotEqual: return Operators.NotEqual;
                default:
                    if (op >= Operators.Add && op <= Operators.Xor) {
                        return (Operators)((int)op + (int)Operators.ReverseAdd - (int)Operators.Add);
                    } 
                    return Operators.None;                    
            }
        }

        public static bool IsComparisonOperator(Operators op) {
            switch (op) {
                case Operators.LessThan: return true;
                case Operators.LessThanOrEqual: return true;
                case Operators.GreaterThan: return true;
                case Operators.GreaterThanOrEqual: return true;
                case Operators.Equal: return true;
                case Operators.NotEqual: return true;
                case Operators.Compare: return true;
            }
            return false;
        }

        // TODO remove this method as we move from DynamicType to Type
        public static DynamicType[] ObjectTypes(object[] args) {
            DynamicType[] types = new DynamicType[args.Length];
            for (int i = 0; i < args.Length; i++) {
                types[i] = DynamicHelpers.GetDynamicType(args[i]);
            }
            return types;
        }

        // TODO remove this method as we move from DynamicType to Type
        public static Type ConvertToType(DynamicType dynamicType) {
            if (dynamicType.IsNull) {
                return None.Type;
            } else {
                return dynamicType.UnderlyingSystemType;
            }
        }

        // TODO remove this method as we move from DynamicType to Type
        public static Type[] ConvertToTypes(DynamicType[] dynamicTypes) {
            Type[] types = new Type[dynamicTypes.Length];
            for (int i = 0; i < dynamicTypes.Length; i++) {
                types[i] = ConvertToType(dynamicTypes[i]);
            }
            return types;
        }

        /// <summary>
        /// Returns the System.Type for any object, including null.  The type of null
        /// is represented by None.Type and all other objects just return the 
        /// result of Object.GetType
        /// </summary>
        public static Type GetType(object obj) {
            return obj == null ? None.Type : obj.GetType();
        }

        /// <summary>
        /// Simply returns a Type[] from calling GetType on each element of args.
        /// </summary>
        public static Type[] GetTypes(object[] args) {
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++) {
                types[i] = GetType(args[i]);
            }
            return types;
        }

        public static bool CanOptimizeMethod(MethodBase method) {
            if (method.ContainsGenericParameters ||
                method.IsFamily ||
                method.IsPrivate ||
                method.IsFamilyOrAssembly ||
                !method.DeclaringType.IsVisible) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Given a MethodInfo which may be declared on a non-public type this attempts to
        /// return a MethodInfo which will dispatch to the original MethodInfo but is declared
        /// on a public type.
        /// </summary>
        public static MethodInfo GetCallableMethod(MethodInfo getter) {
            if (getter.DeclaringType.IsVisible) return getter;
            // first try and get it from the base type we're overriding...
            getter = getter.GetBaseDefinition();

            if (getter.DeclaringType.IsVisible) return getter;
            // maybe we can get it from an interface...
            Type[] interfaces = getter.DeclaringType.GetInterfaces();
            foreach (Type iface in interfaces) {
                InterfaceMapping mapping = getter.DeclaringType.GetInterfaceMap(iface);
                for (int i = 0; i < mapping.TargetMethods.Length; i++) {
                    if (mapping.TargetMethods[i] == getter) {
                        return mapping.InterfaceMethods[i];
                    }
                }
            }

            // well, we couldn't do any better.
            return getter;
        }

        public static bool CanOptimizeField(FieldInfo fi) {
            return fi.IsPublic && fi.DeclaringType.IsVisible;
        }

        internal static void CreateYieldLabels(CodeGen cg, List<YieldTarget> targets) {
            if (targets != null) {
                foreach (YieldTarget yt in targets) {
                    yt.EnsureLabel(cg);
                }
            }
        }

        public static Type GetVisibleType(object value) {
            return GetVisibleType(GetType(value));
        }

        public static Type GetVisibleType(Type t) {
            while (!t.IsVisible) {
                t = t.BaseType;
            }
            return t;
        }
    }
}
