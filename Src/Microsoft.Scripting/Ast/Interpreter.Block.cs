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
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Interpreter partial class. This part contains interpretation code for code blocks.
    /// </summary>
    public static partial class Interpreter {
        private static WeakHash<CodeBlock, InterpreterData> _Hashtable = new WeakHash<CodeBlock, InterpreterData>();

        private static InterpreterData GetBlockInterpreterData(CodeBlock block) {
            InterpreterData data;
            lock (_Hashtable) {
                if (!_Hashtable.TryGetValue(block, out data)) {
                    _Hashtable[block] = data = new InterpreterData();
                }
            }
            Debug.Assert(data != null);
            return data;
        }

        private static object DoExecute(CodeContext context, CodeBlock block) {
            object ret;

            // Make sure that locals owned by this block mask any identically-named variables in outer scopes
            if (!block.IsGlobal) {
                foreach (Variable v in block.Variables) {
                    if (v.Kind == Variable.VariableKind.Local && v.Uninitialized && v.Block == block) {
                        Interpreter.EvaluateAssignVariable(context, v, Uninitialized.Instance);
                    }
                }
            }

            context.Scope.SourceLocation = block.Start;
            ret = Interpreter.EvaluateExpression(context, block.Body);

            ControlFlow cf = ret as ControlFlow;
            if (cf != null) {
                return cf.Value;
            } else {
                return null;
            }
        }

        private static object Execute(CodeContext context, CodeBlock block) {
            try {
                return DoExecute(context, block);
            } catch (Exception e) {
#if !SILVERLIGHT
                MethodBase method = MethodBase.GetCurrentMethod();
#else
                MethodBase method = null;
#endif
                SourceUnit sourceUnit = context.ModuleContext.CompilerContext.SourceUnit;
                int line = context.Scope.SourceLocation.Line;

                ExceptionHelpers.UpdateStackTrace(context, method, block.Name, sourceUnit.GetSymbolDocument(line), sourceUnit.MapLine(line));
                ExceptionHelpers.AssociateDynamicStackFrames(e);
                throw ExceptionHelpers.UpdateForRethrow(e);
            }
        }

        // TODO: Rename
        internal static object TopLevelExecute(CodeBlock block, CodeContext context) {
            FlowChecker.Check(block);
            return Interpreter.Execute(context, block);
        }

        private static bool ShouldCompile(InterpreterData id) {
            return id.CallCount++ > InterpreterData.MaxInterpretedCalls;
        }

        internal static object ExecuteWithChildContext(CodeContext parent, CodeBlock block, params object[] args) {
            InterpreterData id = GetBlockInterpreterData(block);

            // Fast path for if we have emitted this code block as a delegate
            if (id.Delegate != null) {
                return ReflectionUtils.InvokeDelegate(id.Delegate, args);
            }

            if (parent.LanguageContext.Options.ProfileDrivenCompilation) {
                lock (id) {
                    // Check _delegate again -- maybe it appeared between our first check and taking the lock
                    if (id.Delegate == null && ShouldCompile(id)) {
                        id.Delegate = GetCompiledDelegate(block, id.DeclaringContext, null, id.ForceWrapperMethod);
                    }
                }
                if (id.Delegate != null) {
                    return ReflectionUtils.InvokeDelegate(id.Delegate, args);
                }
            }

            CodeContext child = RuntimeHelpers.CreateNestedCodeContext(new SymbolDictionary(), parent, block.IsVisible);
            for (int i = 0; i < block.Parameters.Count; i++) {
                RuntimeHelpers.SetName(child, block.Parameters[i].Name, args[i]);
            }
            return Execute(child, block);
        }

        internal static object ExecuteWithChildContextAndThis(CodeContext parent, CodeBlock block, object @this, params object[] args) {
            InterpreterData id = GetBlockInterpreterData(block);

            if (id.Delegate != null) {
                return parent.LanguageContext.CallWithThis(parent, id.Delegate, @this, args);
            }

            if (parent.LanguageContext.Options.ProfileDrivenCompilation) {
                lock (id) {
                    if (id.Delegate == null && ShouldCompile(id)) {
                        id.Delegate = GetCompiledDelegate(block, id.DeclaringContext, null, id.ForceWrapperMethod);
                    }
                }
                if (id.Delegate != null) {
                    return parent.LanguageContext.CallWithThis(parent, id.Delegate, @this, args);
                }
            }

            CodeContext child = RuntimeHelpers.CreateNestedCodeContext(new SymbolDictionary(), parent, block.IsVisible);
            RuntimeHelpers.SetName(child, block.Parameters[0].Name, @this);
            for (int i = 1; i < block.Parameters.Count; i++) {
                RuntimeHelpers.SetName(child, block.Parameters[i].Name, args[i - 1]);
            }
            return Execute(child, block);
        }

        private static Delegate GetDelegateForInterpreter(CodeBlock block, CodeContext context, Type delegateType, bool forceWrapperMethod) {
            GeneratorCodeBlock gcb = block as GeneratorCodeBlock;
            if (gcb != null) {
                return GetGeneratorDelegateForInterpreter(gcb, context, delegateType, forceWrapperMethod);
            } else {
                return GetCodeBlockDelegateForInterpreter(block, context, delegateType, forceWrapperMethod);
            }
        }

        // Return a delegate to execute this block in interpreted mode.
        private static Delegate GetCodeBlockDelegateForInterpreter(CodeBlock block, CodeContext context, Type delegateType, bool forceWrapperMethod) {
            FlowChecker.Check(block);

            bool delayedEmit = context.LanguageContext.Options.ProfileDrivenCompilation;
            InterpreterData id = GetBlockInterpreterData(block);

            // Walk the tree to determine whether to emit this CodeBlock or interpret it
            if (InterpretChecker.CanEvaluate(block, delayedEmit)) {
                // Hold onto our declaring context in case we decide to emit ourselves later
                id.DeclaringContext = context.ModuleContext.CompilerContext;
                id.ForceWrapperMethod = forceWrapperMethod;

                if (delegateType == null) {
                    if (block.HasThis()) {
                        return (CallTargetWithContextAndThisN)(new CodeBlockInvoker(block, null).ExecuteWithChildContextAndThis);
                    } else {
                        return (CallTargetWithContextN)(new CodeBlockInvoker(block, null).ExecuteWithChildContext);
                    }
                }

                return MakeStronglyTypedDelegate(block, context, delegateType);
            } else {
                lock (id) {
                    if (id.Delegate == null) {
                        id.Delegate = GetCompiledDelegate(block, context.ModuleContext.CompilerContext, delegateType, forceWrapperMethod);
                    }
                    return id.Delegate;
                }
            }
        }

        private static Delegate MakeStronglyTypedDelegate(CodeBlock block, CodeContext context, Type delegateType) {
            MethodInfo target = delegateType.GetMethod("Invoke");
            ParameterInfo[] pis = target.GetParameters();
            CodeBlockInvoker cbi = new CodeBlockInvoker(block, context);
            MemberInfo[] targets;

            Type[] paramTypes;
            if (target.ReturnType == typeof(void)) {
                paramTypes = ReflectionUtils.GetParameterTypes(pis);
                targets = typeof(CodeBlockInvoker).GetMember("ExecuteWithChildContext");
            } else {
                paramTypes = ArrayUtils.Append(ReflectionUtils.GetParameterTypes(pis), target.ReturnType);
                targets = typeof(CodeBlockInvoker).GetMember("InvokeWithChildContext");
            }

            foreach (MethodInfo mi in targets) {
                if (mi.GetParameters().Length == pis.Length) {
                    MethodInfo genMethod = mi.MakeGenericMethod(paramTypes);
                    return Delegate.CreateDelegate(delegateType, cbi, genMethod);
                }
            }
            throw new InvalidOperationException(String.Format("failed to make delegate for type {0}", delegateType.FullName));
        }

        private static Delegate GetCompiledDelegate(CodeBlock block, CompilerContext context, Type delegateType, bool forceWrapperMethod) {
            bool createWrapperMethod = block.ParameterArray ? false : forceWrapperMethod || Compiler.NeedsWrapperMethod(block, true, false);
            bool hasThis = block.HasThis();

            Compiler cg = CreateInterprettedMethod(block, context, delegateType, hasThis);
            cg.EmitFunctionImplementation(block);

            cg.Finish();

            if (delegateType == null) {
                if (createWrapperMethod) {
                    Compiler wrapper = Compiler.MakeWrapperMethodN(null, cg, block, hasThis);
                    wrapper.Finish();
                    delegateType = hasThis ? typeof(CallTargetWithContextAndThisN) : typeof(CallTargetWithContextN);
                    return wrapper.CreateDelegate(delegateType);
                } else if (block.ParameterArray) {
                    delegateType = hasThis ? typeof(CallTargetWithContextAndThisN) : typeof(CallTargetWithContextN);
                    return cg.CreateDelegate(delegateType);
                } else {
                    delegateType = CallTargets.GetTargetType(true, block.Parameters.Count - (block.HasThis() ? 1 : 0), block.HasThis());
                    return cg.CreateDelegate(delegateType);
                }
            } else {
                return cg.CreateDelegate(delegateType);
            }
        }

        private static Compiler CreateInterprettedMethod(CodeBlock block, CompilerContext context, Type delegateType, bool hasThis) {
            List<Type> paramTypes;
            List<SymbolId> paramNames;
            Compiler impl;
            string implName;


            int lastParamIndex;

            if (delegateType == null) {
                lastParamIndex = Compiler.ComputeSignature(block, true, hasThis, out paramTypes, out paramNames, out implName);
            } else {
                Debug.Assert(!block.ParameterArray);
                lastParamIndex = Compiler.ComputeDelegateSignature(block, delegateType, out paramTypes, out paramNames, out implName);
            }

            impl = CompilerHelpers.CreateDynamicCodeGenerator(
                    implName,
                    typeof(object),
                    paramTypes.ToArray(),
                    new ConstantPool());
            impl.InterpretedMode = true;
            impl.ContextSlot = impl.ArgumentSlots[0];
            impl.Context = context;
            impl.EnvironmentSlot = new EnvironmentSlot(
                new PropertySlot(
                    new PropertySlot(impl.ContextSlot,
                        typeof(CodeContext).GetProperty("Scope")),
                    typeof(Scope).GetProperty("Dict"))
                );

            if (block.ParameterArray) {
                impl.ParamsSlot = impl.GetArgumentSlot(lastParamIndex);
            }

            impl.Allocator = CompilerHelpers.CreateLocalStorageAllocator(null, impl);

            return impl;
        }

        private static Delegate GetGeneratorDelegateForInterpreter(GeneratorCodeBlock block, CodeContext context, Type delegateType, bool forceWrapperMethod) {
            // For now, always return a compiled delegate (since yield is not implemented)
            InterpreterData id = GetBlockInterpreterData(block);
            lock (id) {
                if (id.Delegate == null) {
                    FlowChecker.Check(block);
                    id.Delegate = GetCompiledDelegate(block, context.ModuleContext.CompilerContext, delegateType, forceWrapperMethod);
                }
                return id.Delegate;
            }
        }
    }

}
