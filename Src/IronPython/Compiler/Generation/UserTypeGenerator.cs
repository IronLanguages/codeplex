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
using System.Collections.Specialized;
using System.Text;
using System.Diagnostics;

using System.Reflection;
using System.Reflection.Emit;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Compiler.Ast;
using IronPython.CodeDom;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Generation {
    /// <summary>
    /// Create's static .NET types from Python types.  Python types must have __slots__
    /// definition to get this concrete form created.  These types then disallow arbitrary
    /// instance assignment.
    /// </summary>
    static class UserTypeGenerator {
        public static TypeGen DoStaticCompilation(GlobalSuite gs, CodeGen cg) {
            GeneratorWalker gw = new GeneratorWalker(cg);
            gs.Walk(gw);
            return gw.FinishedType;
        }

        private class GeneratorWalker : AstWalkerNonRecursive {
            CompilerContext compctx;
            Stack<StackInfo> stack = new Stack<StackInfo>();
            List<Dictionary<string, TypeGen>> declaredTypes = new List<Dictionary<string, TypeGen>>();
            CodeGen moduleInit;
            TypeGen outerScope;
            TypeGen finished;
            AssemblyGen assembly;
            List<string> namespaces;
            List<Assembly> assemblies;

            public GeneratorWalker(CodeGen codeGen) {
                Debug.Assert(codeGen.typeGen != null);

                assembly = codeGen.typeGen.myAssembly;

                compctx = codeGen.Context;
                moduleInit = codeGen;
                outerScope = codeGen.typeGen;
                namespaces = new List<string>();

                //!!! need to get type names already declared...
                PushNewType(codeGen.typeGen, codeGen, null, null);

                assemblies = new List<Assembly>();
                if (RemoteCompiler.Instance != null) {
                    StringCollection files = RemoteCompiler.Instance.References;
                    foreach (string file in files) {
                        Assembly asm = IronPython.Hosting.PythonCompiler.LoadAssembly(file);
                        assemblies.Add(asm);
                    }
                }
            }

            public TypeGen FinishedType {
                get {
                    return finished;
                }
            }

            #region IAstWalker Members

            /// <summary>
            /// Starts a class definition.  This will create the new type, 
            /// the static constructor & emit the initialization into it,
            /// and then push the type onto the stack.  
            /// 
            /// Later the walker will emit all of the statements in the class 
            /// definition into the static constructor.
            /// </summary>
            public override bool Walk(ClassDefinition cd) {
                // class definition - could either by a real type, or
                // a namespace.  If it only contains class defintions
                // and no slots then it's a namespace.
                if (IsNamespace(cd)) {
                    namespaces.Add(cd.Name.GetString());
                    return true;
                }

                // - emit all class initialization inside of
                // static ctor.                

                SlotFinder sf = new SlotFinder();
                cd.Walk(sf);

                TypeGen baseTg;
                List<Type> baseTypes = GetBaseType(cd, out baseTg);
                if (baseTypes == null) {
                    // we don't know the base types, just emit it the old way...
                    namespaces.Add(cd.Name.GetString());
                    return true;
                }

                TypeGen newType = DefineNewType(cd, baseTypes);
                CodeGen getCompiledType = AddIDynamicObject(newType, newType.AddField(typeof(IDictionary<SymbolId, object>), "__dict__"));


                CodeGen ctorBuilder = AddDefaultCtor(newType, sf.Slots);
                CodeGen classInit = DefineClassInit(cd, newType);

                PushNewType(newType, classInit, ctorBuilder, baseTypes);
                stack.Peek().GetCompiledType = getCompiledType;


                // continue the walk on just the body (returning
                // true would have us walk the bases & the body).
                cd.Body.Walk(this);
                return false;
            }

            /// <summary>
            /// Pops the last class defintion from the stack, updating
            /// the available type names so other classes can derive
            /// from them
            /// </summary>
            public override void PostWalk(ClassDefinition cd) {
                TypeGen baseTg;
                List<Type> baseTypes = GetBaseType(cd, out baseTg);

                if (baseTypes == null || IsNamespace(cd)) {
                    namespaces.RemoveAt(namespaces.Count - 1);
                    if (stack.Count == 1 && namespaces.Count == 0) {
                        // outer item is a namespace, we need to load it.
                        CodeGen cg = stack.Peek().StaticConstructor;

                        cg.EmitCallerContext();
                        cg.EmitCall(typeof(Assembly), "GetExecutingAssembly");
                        cg.EmitString(cd.Name.GetString());
                        cg.EmitCall(typeof(Ops), "GetNamespace");
                        cg.EmitSet(cd.Name);
                    }
                    return;
                }

                StackInfo info = stack.Pop();
                finished = info.Type;

                info.StaticConstructor.Emit(OpCodes.Ret);
                declaredTypes.RemoveAt(declaredTypes.Count - 1);
                declaredTypes[declaredTypes.Count - 1][cd.Name.GetString()] = finished;

                Type baseType = baseTypes[0];

                info.DefaultConstructor.Emit(OpCodes.Ldarg_0);
                ConstructorInfo baseCtor;
                if (baseTg != null) {
                    baseCtor = baseTg.DefaultConstructor;
                } else {
                    baseCtor = baseType.GetConstructor(new Type[0]);
                }
                Debug.Assert(baseCtor != null, "baseCtor is null", String.Format("type: {0} base: {1}", info.Type.myType.Name, baseType.FullName));

                info.DefaultConstructor.Emit(OpCodes.Call, baseCtor);

                info.DefaultConstructor.EmitReturn();

                if (stack.Count == 1) {
                    info.Type.FinishType();
                    CodeGen cg = stack.Peek().StaticConstructor;

                    if (namespaces.Count == 0) {
                        cg.EmitCall(info.GetCompiledType.MethodInfo);
                        cg.EmitSet(cd.Name);
                    }
                }
            }

            public override bool Walk(FunctionDefinition node) {
                CodeGen typeCctor = stack.Peek().StaticConstructor;
                Debug.Assert(typeCctor != null);

                if (stack.Count == 1) {
                    // global function, just emit the node...
                    node.Emit(typeCctor);
                    return false;
                }

                string strName = node.Name.GetString();

                SignatureInfo sigInfo = node.GetSignature(typeCctor);

                CodeGen icg = CreateNewMethod(node, typeCctor, sigInfo);

                icg.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(PythonNameAttribute).GetConstructor(new Type[] { typeof(string) }),
                    new object[] { strName }));

                string docStr = node.Body.Documentation;
                if (docStr != null) {
                    icg.SetCustomAttribute(new CustomAttributeBuilder(
                        typeof(DocumentationAttribute).GetConstructor(new Type[] { typeof(string) }),
                        new object[] { docStr }));
                }

                //  Generate the function implementation
                node.EmitFunctionImplementation(icg, typeCctor);
                icg.Finish();


                stack.Peek().Methods.Add(icg);

                return false;
            }

            public override bool Walk(SuiteStatement node) {
                return true;
            }

            public override bool Walk(GlobalSuite node) {
                node.CreateGlobalSlots(stack.Peek().StaticConstructor);
                return true;
            }

            // anything else gets emitted directly
            // into the current initialization function.  If this
            // is something like a conditional block that includes
            // a function definition it will not be visible at .NET
            // scope (but will still work).

            public override bool Walk(AssignStatement node) {
                if (stack.Count == 1) {
                    node.Emit(stack.Peek().StaticConstructor);
                    return false;
                }

                CallExpression rhsCall = node.Right as CallExpression;
                if (rhsCall != null) {
                    bool fCantEmit = false;
                    NameExpression ne = rhsCall.Target as NameExpression;
                    string get = null, set = null;
                    if (ne != null) {
                        if (ne.Name.GetString() == "property") {
                            // property definition...
                            fCantEmit = GetPropertyAccessors(rhsCall, ref ne, ref get, ref set);
                        }
                    }

                    if (!fCantEmit) {
                        PropertyBuilder pb = stack.Peek().Type.DefineProperty(((NameExpression)node.Left[0]).Name.GetString(), PropertyAttributes.None, typeof(object));

                        foreach (CodeGen cg in stack.Peek().Methods) {
                            if (get != null && cg.methodInfo.Name == get) {
                                pb.SetGetMethod((MethodBuilder)cg.methodInfo);
                            } else if (set != null && cg.methodInfo.Name == set) {
                                pb.SetSetMethod((MethodBuilder)cg.methodInfo);
                            }
                        }
                        return false;
                    }
                }

                ConstantExpression rhsConstant = node.Right as ConstantExpression;
                NameExpression rhsName;
                if (rhsConstant != null) {
                    if (rhsConstant.Value != null) {
                        stack.Peek().Type.AddStaticField(rhsConstant.Value.GetType(), ((NameExpression)node.Left[0]).Name.GetString());
                    } else {
                        stack.Peek().Type.AddStaticField(typeof(object), ((NameExpression)node.Left[0]).Name.GetString());
                    }
                } else if ((rhsName = node.Right as NameExpression) != null &&
                    (rhsName.Name.GetString() == "True" || rhsName.Name.GetString() == "False")) {
                    stack.Peek().Type.AddStaticField(typeof(bool), ((NameExpression)node.Left[0]).Name.GetString());
                } else {
                    Emit(node);
                }

                return false;
            }

            public override bool Walk(AssertStatement node) { Emit(node); return false; }
            public override bool Walk(AugAssignStatement node) { Emit(node); return false; }
            public override bool Walk(BreakStatement node) { Emit(node); return false; }
            public override bool Walk(ContinueStatement node) { Emit(node); return false; }
            public override bool Walk(DelStatement node) { Emit(node); return false; }
            public override bool Walk(ExecStatement node) { Emit(node); return false; }
            public override bool Walk(ExpressionStatement node) { Emit(node); return false; }
            public override bool Walk(ForStatement node) { Emit(node); return false; }
            public override bool Walk(FromImportStatement node) { Emit(node); return false; }
            public override bool Walk(GlobalStatement node) { Emit(node); return false; }
            public override bool Walk(IfStatement node) { Emit(node); return false; }
            public override bool Walk(ImportStatement node) { Emit(node); return false; }
            public override bool Walk(PassStatement node) { Emit(node); return false; }
            public override bool Walk(PrintStatement node) { Emit(node); return false; }
            public override bool Walk(RaiseStatement node) { Emit(node); return false; }
            public override bool Walk(ReturnStatement node) { Emit(node); return false; }
            public override bool Walk(TryFinallyStatement node) { Emit(node); return false; }
            public override bool Walk(TryStatement node) { Emit(node); return false; }
            public override bool Walk(WhileStatement node) { Emit(node); return false; }
            public override bool Walk(YieldStatement node) { Emit(node); return false; }

            #endregion

            #region Private implementation details
            private static bool GetPropertyAccessors(CallExpression rhsCall, ref NameExpression ne, ref string get, ref string set) {
                bool fCantEmit = false;
                for (int i = 0; i < rhsCall.Args.Count; i++) {
                    // fget, fset, fdel, doc
                    if (rhsCall.Args[i].Name != SymbolTable.Empty) {
                        switch (rhsCall.Args[i].Name.GetString()) {
                            case "fget":
                                ne = rhsCall.Args[i].Expression as NameExpression;
                                if (ne == null) { fCantEmit = true; break; }

                                get = ne.Name.GetString();
                                break;
                            case "fset":
                                ne = rhsCall.Args[i].Expression as NameExpression;
                                if (ne == null) { fCantEmit = true; break; }

                                set = ne.Name.GetString();
                                break;
                            default:
                                fCantEmit = true;
                                break;
                        }
                    } else {
                        switch (i) {
                            case 0:
                                ne = rhsCall.Args[i].Expression as NameExpression;
                                if (ne == null) { fCantEmit = true; break; }

                                get = ne.Name.GetString();
                                break;
                            case 1:
                                ne = rhsCall.Args[i].Expression as NameExpression;
                                if (ne == null) { fCantEmit = true; break; }

                                set = ne.Name.GetString();
                                break;
                            default:
                                fCantEmit = true;
                                break;
                        }
                    }
                }
                return fCantEmit;
            }


            private void Emit(Statement s) {
                s.Emit(stack.Peek().StaticConstructor);
            }


            private static bool IsNamespace(ClassDefinition cd) {
                SlotFinder sf = new SlotFinder();
                cd.Walk(sf);
                if (sf.HasSubTypes && !sf.HasFunctions && !sf.FoundSlots && cd.Bases.Count == 0) {
                    return true;
                }
                return false;
            }

            private CodeGen CreateNewMethod(FunctionDefinition node, CodeGen typeCctor, SignatureInfo sigInfo) {
                string strName = node.Name.GetString();
                MethodAttributes attrs = GetMethodAttributes(node, strName);

                int offset = (sigInfo.HasContext ? 2 : 1);
                if ((attrs & MethodAttributes.Static) != 0) offset--;
                
                if (node.Parameters.Count == 0 && (attrs & MethodAttributes.Static) == 0)
                    throw new CompilerException(String.Format("defining non-static method {0} with no parameters.  Add self or @staticmethod decorator", node.Name), node, this.compctx.SourceFile);

                Debug.Assert(sigInfo.ParamNames.Length >= offset,
                    "less param names then offset",
                    String.Format("Params: {0} Offset: {1}: Context: {2} Attrs: {3} Name: {4}",
                        sigInfo.ParamNames.Length, offset, sigInfo.HasContext, attrs, strName));

                string[] paramNames = new string[sigInfo.ParamNames.Length - offset];
                for (int i = 0; i < paramNames.Length; i++)
                    paramNames[i] = sigInfo.ParamNames[i + offset].GetString();

                Type[] typeArr;
                CustomAttributeBuilder[] cabs;
                GetTypesAndAttrs(node, sigInfo, offset, out typeArr, out cabs);

                object[] funcDefaults = GetStaticDefaults(node, paramNames.Length);
                MethodInfo baseMethod = GetMethodOverload(strName, attrs);

                Type retType;
                if (baseMethod == null) {
                    // Check whether the method has a return statement, to decide whether it should
                    // return void or object
                    ReturnStatementFinder finder = new ReturnStatementFinder(node);
                    node.Walk(finder);
                    retType = finder.FoundReturnStatement ? typeof(object) : typeof(void);
                } else {
                    // Get the return and param types from the base method
                    typeArr = CompilerHelpers.GetTypes(baseMethod.GetParameters());
                    retType = baseMethod.ReturnType;
                    attrs |= MethodAttributes.Virtual;
                }

                CodeGen icg = typeCctor.typeGen.DefineMethod(attrs,
                         strName,
                         retType,
                         typeArr,
                         paramNames,
                         funcDefaults,
                         cabs);

                icg.Context = compctx;

                if (baseMethod != null) icg.methodToOverride = baseMethod;

                icg.Names = CodeGen.CreateLocalNamespace(icg);
                for (int arg = offset; arg < sigInfo.ParamNames.Length; arg++) {
                    icg.Names.SetSlot(sigInfo.ParamNames[arg], icg.GetArgumentSlot(arg - offset));
                }

                if ((attrs & MethodAttributes.Static) == 0) {
                    icg.Names.SetSlot(sigInfo.ParamNames[offset - 1], new ArgSlot(0, typeCctor.typeGen.myType, icg));
                }

                icg.ContextSlot = stack.Peek().Type.moduleSlot;

                EmitArgsToTuple(node, icg, sigInfo, paramNames.Length);
                return icg;
            }

            private MethodInfo GetMethodOverload(string strName, MethodAttributes attrs) {
                MethodInfo baseMethod = null;
                if ((attrs & MethodAttributes.Static) == 0) {
                    // see if we're overriding anything in a base
                    List<Type> bases = stack.Peek().BaseTypes;
                    for (int i = 0; i < bases.Count; i++) {
                        MethodInfo[] mis = bases[i].GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        baseMethod = PickBestOverload(strName, mis);
                        if (baseMethod != null) {
                            break;
                        }
                    }
                }
                return baseMethod;
            }

            private static MethodAttributes GetMethodAttributes(FunctionDefinition node, string strName) {
                MethodAttributes attrs = MethodAttributes.Public;
                if (strName == "__new__") {
                    attrs |= MethodAttributes.Static;
                } else {
                    CallExpression decExpr = node.Decorators as CallExpression;
                    while (decExpr != null) {
                        NameExpression ne = decExpr.Target as NameExpression;
                        if (ne != null && ne.Name.GetString() == "staticmethod") {
                            attrs |= MethodAttributes.Static;
                        }

                        decExpr = decExpr.Args[0].Expression as CallExpression;
                    }
                }
                return attrs;
            }

            private static MethodInfo PickBestOverload(string name, MethodInfo[] mis) {
                for (int i = 0; i < mis.Length; i++) {
                    if (mis[i].Name == name) {
                        return mis[i];
                    }
                }
                return null;
            }

            private static void EmitArgsToTuple(FunctionDefinition fd, CodeGen icg, SignatureInfo sigInfo, int cnt) {
                if ((fd.Flags & FunctionAttributes.ArgumentList) != 0) {
                    // transform params object[] into tuple on call...

                    LocalBuilder lb = icg.DeclareLocal(typeof(object));
                    Slot argsSlot = new LocalSlot(lb, icg);
                    int index;

                    if ((fd.Flags & FunctionAttributes.KeywordDictionary) != 0) {
                        index = sigInfo.ParamNames.Length - 2;
                        icg.EmitArgGet(cnt - 2);
                    } else {
                        index = sigInfo.ParamNames.Length - 1;
                        icg.EmitArgGet(cnt - 1);
                    }

                    icg.EmitCall(typeof(Tuple), "MakeTuple");
                    argsSlot.EmitSet(icg);
                    lb.SetLocalSymInfo(sigInfo.ParamNames[index].GetString());
                    icg.Names.SetSlot(sigInfo.ParamNames[index], argsSlot);
                }
            }

            private static object[] GetStaticDefaults(FunctionDefinition fd, int count) {
                object[] funcDefaults = CompilerHelpers.MakeRepeatedArray<object>(DBNull.Value, count);
                if (fd.Defaults != null) {
                    for (int i = 0; i < fd.Defaults.Count; i++) {
                        ConstantExpression ce = fd.Defaults[i] as ConstantExpression;
                        if (ce != null) {
                            funcDefaults[i + (count - fd.Defaults.Count)] = ce.Value;
                        } else {
                            throw new InvalidOperationException(String.Format("can't handle default: {0}", funcDefaults[i]));
                        }
                    }
                }
                return funcDefaults;
            }

            private static void GetTypesAndAttrs(FunctionDefinition node, SignatureInfo sigInfo, int offset, out Type[] typeArr, out CustomAttributeBuilder[] cabs) {
                typeArr = CompilerHelpers.MakeRepeatedArray(typeof(object), sigInfo.ParamNames.Length - offset);
                cabs = null;
                int curIndex = typeArr.Length - 1;
                if ((node.Flags & FunctionAttributes.KeywordDictionary) != 0) {
                    typeArr[curIndex] = typeof(Dict);
                    cabs = new CustomAttributeBuilder[typeArr.Length];
                    cabs[curIndex] = new CustomAttributeBuilder(typeof(ParamDictAttribute).GetConstructor(new Type[0]), Ops.EMPTY);

                    curIndex--;
                }
                if ((node.Flags & FunctionAttributes.ArgumentList) != 0) {
                    if (cabs == null) cabs = new CustomAttributeBuilder[typeArr.Length];

                    typeArr[curIndex] = typeof(object[]);
                    cabs[curIndex] = new CustomAttributeBuilder(typeof(ParamArrayAttribute).GetConstructor(new Type[0]), Ops.EMPTY);

                    curIndex--;
                }
            }

            /// <summary>
            /// Pushes a new type onto the stack for cases where we have
            /// nested class definitions.
            /// </summary>
            private void PushNewType(TypeGen newType, CodeGen classInit, CodeGen init, List<Type> baseTypes) {
                stack.Push(new StackInfo(newType, classInit, init, baseTypes));
                declaredTypes.Add(new Dictionary<string, TypeGen>());
            }

            private CodeGen DefineClassInit(ClassDefinition cd, TypeGen newType) {
                CodeGen classInit = newType.GetOrMakeInitializer();

                EmitModuleInitialization(newType, classInit);

                classInit.Context = compctx;
                classInit.Names = CodeGen.CreateStaticFieldNamespace(newType);

                // prepare the class for compilation, this binds
                // all of our names into classInit's scope.  We'll then
                // recurse and add everything by hand.
                cd.PrepareForEmit(stack.Peek().StaticConstructor, classInit);

                return classInit;
            }

            /// <summary>
            /// Propagates the module field to the inner type, or runs the
            /// module initialization if it hasn't already been run.
            /// </summary>
            private void EmitModuleInitialization(TypeGen newType, CodeGen classInit) {
                Debug.Assert(classInit != null);

                Label noModule = classInit.DefineLabel();
                Label done = classInit.DefineLabel();

                outerScope.moduleSlot.EmitGet(classInit);
                classInit.Emit(OpCodes.Ldnull);
                classInit.Emit(OpCodes.Beq, noModule);

                outerScope.moduleSlot.EmitGet(classInit);
                newType.moduleSlot.EmitSet(classInit);
                classInit.Emit(OpCodes.Br, done);

                classInit.MarkLabel(noModule);
                // module slot is un-initialized, we must
                // have been reloaded by some .NET code.
                // We'll create a new module now.

                Slot instance = new LocalSlot(classInit.DeclareLocal(outerScope.myType), classInit);

                List<string> refs = null;
                if (RemoteCompiler.Instance != null) {
                    refs = new List<string>(RemoteCompiler.Instance.References.Count);
                    for (int i = 0; i < RemoteCompiler.Instance.References.Count; i++) {
                        string refName = RemoteCompiler.Instance.References[i];
                        if (refName.ToLower().EndsWith(".dll"))
                            refName = refName.Substring(0, refName.Length - 4);

                        if (refName.IndexOf('\\') != -1) {
                            refs.Add(refName.Substring(refName.LastIndexOf('\\') + 1));
                        } else
                            refs.Add(refName);
                    }
                }

                OutputGenerator.EmitModuleConstruction(outerScope, classInit, outerScope.myType.Name, instance, refs);
                // module ctor leaves PythonModule on the stack.
                classInit.Emit(OpCodes.Dup);

                // store the new module in both locations
                outerScope.moduleSlot.EmitSet(classInit);
                newType.moduleSlot.EmitSet(classInit);

                // and finally run the modules initialize method
                instance.EmitGet(classInit);
                classInit.EmitCall(moduleInit.MethodInfo);

                classInit.MarkLabel(done);
            }

            /// <summary>
            /// Creates the new type, sets PythonTypeAttribute on it, and sets the documentation string
            /// </summary>
            private TypeGen DefineNewType(ClassDefinition cd, List<Type> baseTypes) {
                Debug.Assert(baseTypes.Count > 0, "type has no bases", String.Format("for class {0}", cd.Name.ToString()));

                StringBuilder typeName = new StringBuilder();
                for (int i = 0; i < namespaces.Count; i++) {
                    typeName.Append(namespaces[i]);
                    typeName.Append('.');
                }
                typeName.Append(cd.Name.GetString());

                TypeGen newType = DefineTypeInParent(baseTypes, typeName);

                List<MethodInfo> overridden = new List<MethodInfo>();
                List<CodeGen> overrides = new List<CodeGen>();

                MethodInfo[] methods = baseTypes[0].GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                for (int i = 0; i < methods.Length; i++) {
                    if (methods[i].IsPrivate) continue;

                    if (methods[i].IsVirtual || methods[i].IsAbstract) {
                        overrides.Add(NewTypeMaker.CreateVirtualMethodHelper(newType, methods[i]));
                        overridden.Add(methods[i]);
                    }
                }

                for (int i = 1; i < baseTypes.Count; i++) {
                    Debug.Assert(baseTypes[i].IsInterface, "expected interface", String.Format("type is not an interface: {0}", baseTypes[i].FullName));
                    newType.myType.AddInterfaceImplementation(baseTypes[i]);
                }

                if (overridden.Count != 0) {
                    newType.myType.AddInterfaceImplementation(typeof(ICustomBaseAccess));

                    CodeGen baseAccess = newType.DefineMethodOverride(typeof(ICustomBaseAccess).GetMethod("TryGetBaseAttr"));
                    Label next = baseAccess.DefineLabel();

                    for (int i = 0; i < overridden.Count; i++) {
                        baseAccess.EmitSymbolIdInt(overridden[i].Name);
                        baseAccess.EmitArgAddr(1);
                        baseAccess.EmitFieldGet(typeof(SymbolId), "Id");
                        baseAccess.Emit(OpCodes.Bne_Un, next);

                        // return a ReflectedMethod for this method...
                        baseAccess.EmitArgGet(2);


                        baseAccess.EmitString(overridden[i].Name);
                        baseAccess.Emit(OpCodes.Ldtoken, overrides[i].MethodInfo);
                        baseAccess.EmitCall(typeof(MethodBase), "GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) });
                        baseAccess.Emit(OpCodes.Castclass, typeof(MethodInfo));
                        baseAccess.EmitInt((int)FunctionType.Method);

                        baseAccess.EmitCall(typeof(BuiltinFunction).GetMethod("MakeMethod",
                            new Type[] { typeof(string), typeof(MethodInfo), typeof(FunctionType) }));
                        baseAccess.EmitCall(typeof(BuiltinFunction), "GetDescriptor");

                        baseAccess.Emit(OpCodes.Stind_Ref);
                        baseAccess.EmitRawConstant(true);
                        baseAccess.EmitReturn();

                        baseAccess.MarkLabel(next);
                        next = baseAccess.DefineLabel();
                    }

                    baseAccess.EmitRawConstant(false);
                    baseAccess.EmitReturn();
                }

                newType.SetCustomAttribute(typeof(PythonTypeAttribute), new object[] { cd.Name.GetString() });
                string doc = cd.Documentation;
                if (doc != null) {
                    newType.SetCustomAttribute(typeof(DocumentationAttribute), new object[] { doc });
                }
                return newType;
            }

            private TypeGen DefineTypeInParent(List<Type> baseTypes, StringBuilder typeName) {
                TypeGen newType;
                if (stack.Count == 1) {
                    //!!! if typeName == moduleName we need to do something special (PythonNameAttribute ?)
                    newType = assembly.DefinePublicType(typeName.ToString(), baseTypes[0]);
                    newType.AddModuleField(typeof(PythonModule));
                } else {
                    newType = stack.Peek().Type.DefineNestedType(typeName.ToString(), baseTypes[0]);
                }
                return newType;
            }

            private static CodeGen AddIDynamicObject(TypeGen newType, Slot dictSlot) {
                Slot classSlot = newType.AddStaticField(typeof(DynamicType), FieldAttributes.Private, "$class");

                newType.myType.AddInterfaceImplementation(typeof(ISuperDynamicObject));
                /* GetDynamicType */

                CodeGen gdtCg = newType.DefineMethodOverride(typeof(IDynamicObject).GetMethod("GetDynamicType"));

                CodeGen cg = newType.DefineUserHiddenMethod(MethodAttributes.FamORAssem | MethodAttributes.Static,
                    "$$GetOrMakeDynamicType", typeof(DynamicType), new Type[0]);
                DoLazyInitCheck(cg, classSlot,
                    delegate() {
                        // if not null, just return the value.
                        classSlot.EmitGet(cg);
                        cg.EmitReturn();
                    },
                    delegate() {
                        // initialization code

                        // CompiledType GetTypeForType(Type t)
                        cg.EmitType(newType.myType);
                        cg.EmitCall(typeof(CompiledType), "GetTypeForType");
                        cg.Emit(OpCodes.Dup);
                        classSlot.EmitSet(cg);

                        cg.EmitReturn();
                    });

                cg.Finish();
                CodeGen ret = cg;

                // just a call to the helper method
                gdtCg.EmitThis();
                gdtCg.EmitCall(typeof(object), "GetType");
                gdtCg.EmitType(newType.myType);
                Label differ = gdtCg.DefineLabel();
                gdtCg.Emit(OpCodes.Bne_Un, differ);

                gdtCg.EmitCall(cg.MethodInfo);
                gdtCg.EmitReturn();

                gdtCg.MarkLabel(differ);

                gdtCg.EmitThis();
                gdtCg.EmitCall(typeof(object), "GetType");
                gdtCg.EmitCall(typeof(Ops), "GetDynamicTypeFromType");
                gdtCg.EmitReturn();

                gdtCg.Finish();



                /* GetDict */
                cg = newType.DefineMethodOverride(typeof(ISuperDynamicObject).GetMethod("GetDict"));
                DoLazyInitCheck(cg, dictSlot,
                    delegate() {
                        // if not null, just return the value.
                        dictSlot.EmitGet(cg);
                        cg.EmitReturn();
                    },
                    delegate() {
                        // initialization code - dictSlot = new CustomOldClassDict()
                        cg.EmitNew(typeof(CustomOldClassDict).GetConstructor(new Type[0]));
                        cg.Emit(OpCodes.Dup);
                        dictSlot.EmitSet(cg);

                        cg.EmitReturn();
                    });
                cg.Finish();

                /* SetDict */
                cg = newType.DefineMethodOverride(typeof(ISuperDynamicObject).GetMethod("SetDict"));
                cg.EmitString("SetDict");
                cg.EmitInt(0);
                cg.Emit(OpCodes.Newarr, typeof(object));
                cg.EmitCall(typeof(Ops), "NotImplementedError");
                cg.Emit(OpCodes.Throw);


                /* SetDynamicType */
                cg = newType.DefineMethodOverride(typeof(ISuperDynamicObject).GetMethod("SetDynamicType"));
                cg.EmitString("SetDynamicType");
                cg.EmitInt(0);
                cg.Emit(OpCodes.Newarr, typeof(object));
                cg.EmitCall(typeof(Ops), "NotImplementedError");
                cg.Emit(OpCodes.Throw);

                return ret;
            }

            delegate void LazyInitHelper();
            private static void DoLazyInitCheck(CodeGen cg, Slot val, LazyInitHelper ifNotNull, LazyInitHelper initCode) {
                Label initType = cg.DefineLabel();

                val.EmitGet(cg);
                cg.Emit(OpCodes.Ldnull);
                cg.Emit(OpCodes.Beq, initType);

                ifNotNull();

                cg.MarkLabel(initType);

                initCode();
            }

            private static CodeGen AddDefaultCtor(TypeGen newType, string[] slots) {
                CodeGen cg = newType.DefineConstructor(new Type[0]);
                if (slots != null) {

                    for (int i = 0; i < slots.Length; i++) {
                        Slot fld;
                        // getting other types would be nice here...
                        if (slots[i].StartsWith("__") && !slots[i].EndsWith("__")) {
                            fld = newType.AddField(typeof(object), "_" + newType.myType.Name + slots[i]);
                        } else {
                            fld = newType.AddField(typeof(object), slots[i]);
                        }

                        cg.EmitUninitialized();
                        fld.EmitSet(cg);
                    }

                    newType.DefaultConstructor = cg.methodInfo as ConstructorBuilder;
                    slots = null;
                }
                newType.DefaultConstructor = cg.methodInfo as ConstructorBuilder;
                return cg;
            }

            /// <summary>
            /// Gets the basetype for the class.  
            /// </summary>
            private List<Type> GetBaseType(ClassDefinition cd, out TypeGen tg) {
                //!!! need to handle types declared before us
                // but within our module's scope, and what do
                // we do if we don't know the base type?
                List<Type> types = new List<Type>(cd.Bases.Count);
                tg = null;
                if (cd.Bases.Count == 0) {
                    types.Add(typeof(object));
                    return types;
                }
                Type baseType = typeof(object);
                string baseName = null;
                for (int i = 0; i < cd.Bases.Count; i++) {
                    NameExpression ne = cd.Bases[i] as NameExpression;
                    FieldExpression fe;
                    if (ne != null) {
                        baseName = ne.Name.GetString();
                    } else if ((fe = cd.Bases[i] as FieldExpression) != null) {
                        baseName = CodeDom.CodeWalker.GetFieldString(fe);
                    } else {
                        throw new NotImplementedException(String.Format("non-name expr base type {0}", cd.Bases[i].GetType()));
                    }

                    if (baseName == null) throw new InvalidOperationException("couldn't find basetype");
                    if (baseName != "object" && baseName != "System.Object") {
                        for (int j = declaredTypes.Count - 1; j >= 0; j--) {
                            Dictionary<string, TypeGen> curDict = declaredTypes[j];

                            if (curDict.TryGetValue(baseName, out tg)) {
                                baseType = tg.myType;
                                break;
                            }
                        }

                        if (baseType == typeof(object) && RemoteCompiler.Instance != null) {
                            List<Assembly> assms = this.assemblies;

                            for (int j = 0; j < assms.Count; j++) {
                                Debug.Assert(assms[j] != null);
                                Type t = assms[j].GetType(baseName);
                                if (t != null) {
                                    baseType = t;
                                    break;
                                }
                            }
                        }

                        if (baseType == typeof(object) && Ops.compiledEngine != null) {
                            foreach (Assembly asm in Ops.compiledEngine.Sys.TopPackage.LoadedAssemblies.Keys) {
                                Type t = asm.GetType(baseName);
                                if (t != null) {
                                    baseType = t;
                                    break;
                                }
                            }
                        }

                        Debug.Assert(baseType != null, "Failed to find type for " + baseName);

                        if (baseType.IsInterface) {
                            types.Add(baseType);
                        } else if (baseType != typeof(object)) {
                            Debug.Assert(types.Count == 0 || types[0].IsInterface, "adding multiple classes",
                                String.Format("Had: {0}\r\nAdding{1} {2}", types.Count == 0 ? "" : types[0].FullName, baseType, baseName));

                            types.Insert(0, baseType);
                        } else {
                            return null;
                        }
                    } else {
                        types.Add(typeof(object));
                    }
                    baseType = typeof(object);
                }

                // if we only got interfaces object is our base
                if (types[0].IsInterface) {
                    types.Insert(0, typeof(object));
                }

                return types;
            }
            #endregion

            private class StackInfo {
                TypeGen type;
                CodeGen cctor, ctor, getCompiledType;
                List<Type> bases;
                List<CodeGen> methods;

                public StackInfo(TypeGen typeGen, CodeGen classInit, CodeGen defaultCtor, List<Type> baseTypes) {
                    type = typeGen;
                    cctor = classInit;
                    ctor = defaultCtor;
                    bases = baseTypes;
                    methods = new List<CodeGen>();
                }

                public TypeGen Type {
                    get {
                        return type;
                    }
                }

                public List<CodeGen> Methods {
                    get {
                        return methods;
                    }
                }

                public List<Type> BaseTypes {
                    get {
                        return bases;
                    }
                }

                public CodeGen StaticConstructor {
                    get {
                        return cctor;
                    }
                }

                public CodeGen DefaultConstructor {
                    get {
                        return ctor;
                    }
                }

                public CodeGen GetCompiledType {
                    get {
                        return getCompiledType;
                    }
                    set {
                        getCompiledType = value;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the class has a __slots__ field which would
        /// allow us to 
        /// </summary>
        private class SlotFinder : AstWalkerNonRecursive {
            public bool FoundSlots = false;
            public bool HasFunctions = false;
            public bool HasSubTypes = false;
            private int depth = 0;
            private string[] slots;

            #region AstWalkerNonRecursive Method Overrides

            public override bool Walk(AssignStatement node) {
                if (!FoundSlots && node.Left.Count == 1) {
                    NameExpression ne = node.Left[0] as NameExpression;
                    if (ne != null && ne.Name.GetString() == "__slots__") {
                        ListExpression le = node.Right as ListExpression;
                        if (le != null) {
                            string[] slotRes = new string[le.Items.Count];
                            for (int i = 0; i < le.Items.Count; i++) {
                                ConstantExpression ce = le.Items[i] as ConstantExpression;
                                if (ce == null) {
                                    slotRes = null;
                                    break;
                                }

                                string slotStr = ce.Value as string;
                                if (slotStr == null) {
                                    slotRes = null;
                                    break;
                                }
                                slotRes[i] = slotStr;
                            }
                            slots = slotRes;
                        }

                        FoundSlots = true;
                    }
                }
                return false;
            }

            public override bool Walk(ClassDefinition node) {
                depth++;
                if (depth > 1) HasSubTypes = true;
                if (depth == 1) {
                    return !FoundSlots;
                }
                return false;
            }

            public override void PostWalk(ClassDefinition node) {
                depth--;
            }

            public override bool Walk(FunctionDefinition node) {
                if (depth == 1) {
                    HasFunctions = true;
                }
                return false;
            }
            public override bool Walk(SuiteStatement node) {
                return !FoundSlots;
            }

            #endregion

            public string[] Slots {
                get {
                    return slots;
                }
            }
        }

        private class ReturnStatementFinder : AstWalkerNonRecursive {
            private FunctionDefinition _funcDef;
            public bool FoundReturnStatement = false;

            public ReturnStatementFinder(FunctionDefinition funcDef) { _funcDef = funcDef; }

            // Only recurse on constructs that can contain return statements

            public override bool Walk(SuiteStatement node) { return true; }
            public override bool Walk(ForStatement node) { return true; }
            public override bool Walk(IfStatement node) { return true; }
            public override bool Walk(WhileStatement node) { return true; }
            public override bool Walk(TryFinallyStatement node) { return true; }
            public override bool Walk(TryStatement node) { return true; }

            // Only recurse on the function itself, but not any nested ones
            public override bool Walk(FunctionDefinition node) { return (node == _funcDef); }

            public override bool Walk(ReturnStatement node) {
                FoundReturnStatement = true;
                return false;
            }
        }

    }
}
