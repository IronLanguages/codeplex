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
using System.Diagnostics;
using System.Threading;
using Microsoft.Scripting.Shell;
using System.Reflection;

namespace Microsoft.Scripting {
    /// <summary>
    /// These are some generally useful helper methods.
    /// Currently the only methods are those to cached boxed representations of commonly
    /// used primitive types so that they can be shared.  This is useful to most dynamic
    /// languages that use object as a universal type.
    /// </summary>
    public static partial class RuntimeHelpers {
        private const int MIN_CACHE = -100;
        private const int MAX_CACHE = 1000;
        private static readonly object[] cache = new object[MAX_CACHE - MIN_CACHE];
        private static readonly string[] chars = new string[255];

        /// <summary> Singleton boxed instance of True.  We should never box additional instances. </summary>
        public static readonly object True = true;
        /// <summary> Singleton boxed instance of False  We should never box additional instances. </summary>
        public static readonly object False = false;
        public static readonly object[] EmptyObjectArray = new object[0];
        [ThreadStatic]
        internal static List<DynamicStackFrame> _stackFrames; 

        static RuntimeHelpers() {
            for (int i = 0; i < (MAX_CACHE - MIN_CACHE); i++) {
                cache[i] = (object)(i + MIN_CACHE);
            }

            for (char ch = (char)0; ch < 255; ch++) {
                chars[ch] = new string(ch, 1);
            }
        }


        public static string CharToString(char ch) {
            if (ch < 255) return chars[ch];
            return new string(ch, 1);
        }

        public static object BooleanToObject(bool value) {
            return value ? True : False;
        }

        public static object Int32ToObject(Int32 value) {
            // caches improves pystone by ~5-10% on MS .Net 1.1, this is a very integer intense app
            if (value < MAX_CACHE && value >= MIN_CACHE) {
                return cache[value - MIN_CACHE];
            }
            return (object)value;
        }

        /// <summary>
        /// Helper method for DynamicSite rules that check the version of their dynamic object
        /// TODO - Remove this method for more direct field accesses
        /// </summary>
        /// <param name="o"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool CheckTypeVersion(object o, int version) {
            return ((ISuperDynamicObject)o).DynamicType.Version == version;
        }

        /// <summary>
        /// Helper method for DynamicSite rules that check the version of their dynamic object
        /// TODO - Remove this method for more direct field accesses
        /// </summary>
        /// <param name="o"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool CheckAlternateTypeVersion(object o, int version) {
            return ((ISuperDynamicObject)o).DynamicType.AlternateVersion == version;
        }

        // formalNormalArgumentCount - does not include FuncDefFlags.ArgList and FuncDefFlags.KwDict
        // defaultArgumentCount - How many arguments in the method declaration have a default value?
        // providedArgumentCount - How many arguments are passed in at the call site?
        // hasArgList - Is the method declaration of the form "foo(*argList)"?
        // keywordArgumentsProvided - Does the call site specify keyword arguments?
        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(
            string methodName,
            int formalNormalArgumentCount,
            int defaultArgumentCount,
            int providedArgumentCount,
            bool hasArgList,
            bool keywordArgumentsProvided) {

            int formalCount;
            string formalCountQualifier;
            string nonKeyword = keywordArgumentsProvided ? "non-keyword " : "";

            if (defaultArgumentCount > 0 || hasArgList) {
                if (providedArgumentCount < formalNormalArgumentCount) {
                    formalCountQualifier = "at least";
                    formalCount = formalNormalArgumentCount - defaultArgumentCount;
                } else {
                    formalCountQualifier = "at most";
                    formalCount = formalNormalArgumentCount;
                }
            } else {
                formalCountQualifier = "exactly";
                formalCount = formalNormalArgumentCount;
            }

            return RuntimeHelpers.SimpleTypeError(string.Format(
                "{0}() takes {1} {2} {3}argument{4} ({5} given)",
                                methodName, // 0
                                formalCountQualifier, // 1
                                formalCount, // 2
                                nonKeyword, // 3
                                formalCount == 1 ? "" : "s", // 4
                                providedArgumentCount)); // 5
        }

        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(string name, int formalNormalArgumentCount, int defaultArgumentCount, int providedArgumentCount) {
            return TypeErrorForIncorrectArgumentCount(name, formalNormalArgumentCount, defaultArgumentCount, providedArgumentCount, false, false);
        }

        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(string name, int expected, int received) {
            return TypeErrorForIncorrectArgumentCount(name, expected, 0, received);
        }

        public static ArgumentTypeException SimpleTypeError(string message) {
            return new ArgumentTypeException(message);
        }

        public static Exception SimpleAttributeError(string message) {
            return new MissingMemberException(message);
        }

        public static void ThrowUnboundLocalError(SymbolId name) {
            throw new UnboundLocalException(string.Format("local variable '{0}' referenced before assignment", SymbolTable.IdToString(name)));
        }

        /// <summary>
        /// Called from generated code, helper to do name lookup
        /// </summary>
        public static object LookupName(CodeContext context, SymbolId name) {
            return context.LanguageContext.LookupName(context, name);
        }

        /// <summary>
        /// Called from generated code, helper to do name assignment.
        /// Order of parameters matches the codegen flow.
        /// </summary>
        public static object SetNameReorder(object value, CodeContext context, SymbolId name) {
            context.LanguageContext.SetName(context, name, value);
            return value;
        }

        /// <summary>
        /// Called from generated code, helper to do name assignment
        /// </summary>
        public static void SetName(CodeContext context, SymbolId name, object value) {
            context.LanguageContext.SetName(context, name, value);
        }

        /// <summary>
        /// Called from generated code, helper to remove a name
        /// </summary>
        public static object RemoveName(CodeContext context, SymbolId name) {
            context.LanguageContext.RemoveName(context, name);
            return null;
        }
        /// <summary>
        /// Called from generated code, helper to do a global name lookup
        /// </summary>
        public static object LookupGlobalName(CodeContext context, SymbolId name) {
            // TODO: could we get rid of new context creation:
            CodeContext moduleScopedContext = new CodeContext(context.Scope.ModuleScope, context.LanguageContext, context.ModuleContext);
            return context.LanguageContext.LookupName(moduleScopedContext, name);
        }

        /// <summary>
        /// Called from generated code, helper to do global name assignment
        /// </summary>
        public static void SetGlobalName(CodeContext context, SymbolId name, object value) {
            // TODO: could we get rid of new context creation:
            CodeContext moduleScopedContext = new CodeContext(context.Scope.ModuleScope, context.LanguageContext, context.ModuleContext);
            context.LanguageContext.SetName(moduleScopedContext, name, value);
        }

        /// <summary>
        /// Called from generated code, helper to remove a global name
        /// </summary>
        public static void RemoveGlobalName(CodeContext context, SymbolId name) {
            // TODO: could we get rid of new context creation:
            CodeContext moduleScopedContext = new CodeContext(context.Scope.ModuleScope, context.LanguageContext, context.ModuleContext);
            context.LanguageContext.RemoveName(moduleScopedContext, name);
        }

        /// <summary>
        /// Called from the generated code. a helper to get a member from the target with a given name.
        /// </summary>
        public static object GetMember(CodeContext context, object target, SymbolId name) {
            return context.LanguageContext.GetMember(context, target, name);
        }

        /// <summary>
        /// Called from the generated code. a helper to get a bound member from the target with a given name.
        /// </summary>
        public static object GetBoundMember(CodeContext context, object target, SymbolId name) {
            return context.LanguageContext.GetBoundMember(context, target, name);
        }

        /// <summary>
        /// Called from the generated code, a helper to set a member on the target with a given name.
        /// The unusual order of parameters makes it easier to do a codegen for this call.
        /// </summary>
        public static object SetMember(object value, object target, SymbolId name, CodeContext context) {
            context.LanguageContext.SetMember(context, target, name, value);
            return value;
        }

        /// <summary>
        /// Called from the generated code, a helper to call a function with "this" and array of arguments.
        /// </summary>
        public static object CallWithThis(CodeContext context, object function, object instance, object[] args) {
            return context.LanguageContext.CallWithThis(context, function, instance, args);
        }

        public static void InitializeModuleField(CodeContext context, SymbolId name, ref ModuleGlobalWrapper wrapper) {
            ModuleGlobalCache mgc = context.LanguageContext.GetModuleCache(name);

            wrapper = new ModuleGlobalWrapper(context, mgc, name);
        }

        public static TupleType GetTupleDictionaryData<TupleType>(Scope scope) where TupleType : NewTuple {
            return ((TupleDictionary<TupleType>)scope.Dict).Tuple;
        }

        public static CodeContext CreateNestedCodeContext(CodeContext context, IAttributesCollection locals, bool visible) {
            return new CodeContext(new Scope(context.Scope, locals, visible), context.LanguageContext, context.ModuleContext);
        }

        public static void AddFunctionEnvironmentToArray(FunctionEnvironmentNDictionary funcEnv) {
            funcEnv.EnvironmentValues[0] = funcEnv;
        }       

        #region Exception handling

        public static void UpdateStackTrace(CodeContext context, MethodBase method, string funcName, string filename, int line) {
            if (_stackFrames == null) _stackFrames = new List<DynamicStackFrame>();

            if (line == SourceLocation.None.Line) {
                line = 0;
            }
            _stackFrames.Add(new DynamicStackFrame(context, method, funcName, filename, line));
        }

        public static List<DynamicStackFrame> AssociateDynamicStackFrames(Exception clrException) {
            if (_stackFrames != null) {
                Utils.GetDataDictionary(clrException)[typeof(DynamicStackFrame)] = _stackFrames;
            }
            return _stackFrames;
        }
        public static void ClearDynamicStackFrames() {
            _stackFrames = null;
        }

        public static object PushExceptionHandler(CodeContext context, Exception clrException) {
            // _currentExceptions is thread static
            if (LanguageContext._currentExceptions == null) LanguageContext._currentExceptions = new List<Exception>();
            LanguageContext._currentExceptions.Add(clrException);

            AssociateDynamicStackFrames(clrException);

            return context.LanguageContext.PushExceptionHandler(context, clrException);
        }

        public static void PopExceptionHandler(CodeContext context) {
            // _currentExceptions is thread static
            Debug.Assert(LanguageContext._currentExceptions != null);
            Debug.Assert(LanguageContext._currentExceptions.Count != 0);

#if !SILVERLIGHT
            ThreadAbortException tae = LanguageContext._currentExceptions[LanguageContext._currentExceptions.Count - 1] as ThreadAbortException;
            if (tae != null && tae.ExceptionState is KeyboardInterruptException) {
                Thread.ResetAbort();
            }
#endif
            context.LanguageContext.PopExceptionHandler();

            _stackFrames = null;

            LanguageContext._currentExceptions.RemoveAt(LanguageContext._currentExceptions.Count - 1);
        }

        public static object CheckException(CodeContext context, object exception, object test) {
            return context.LanguageContext.CheckException(exception, test);
        }

        #endregion

        #region Calls

        public static object CallWithContext(CodeContext context, object func, params object[] args) {
            FastCallable fc = func as FastCallable;
            if (fc != null) return fc.Call(context, args);

            return context.LanguageContext.Call(context, func, args);
        }

        /// <summary>
        /// Called from generate code for the calls with arguments, tuple of extra arguments and keyword dictionary
        /// </summary>
        public static object CallWithArgsKeywordsTupleDict(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            return context.LanguageContext.CallWithArgsKeywordsTupleDict(context, func, args, names, argsTuple, kwDict);
        }

        /// <summary>
        /// Called from generated code for the calls with arguments and argument tuple
        /// </summary>
        public static object CallWithArgsTuple(CodeContext context, object func, object[] args, object argsTuple) {
            return context.LanguageContext.CallWithArgsTuple(context, func, args, argsTuple);
        }

        /// <summary>
        /// Called from the generated code to perform the call with keyword args
        /// </summary>
        public static object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names) {
            return context.LanguageContext.CallWithKeywordArgs(context, func, args, names);
        }

        /// <summary>
        /// Called from the generated code to construct a new instance of the class,
        /// or to call function with the new object instance
        /// </summary>
        public static object Construct(CodeContext context, object func, params object[] args) {
            IConstructorWithCodeContext iConstructor = func as IConstructorWithCodeContext;
            if (iConstructor != null) {
                return iConstructor.Construct(context, args);
            } else {
                throw SimpleTypeError(String.Format("{0} object cannot be constructed", DynamicHelpers.GetDynamicType(func)));
            }
        }

        #endregion

        /// <summary>
        /// Helper method to create an instance.  Work around for Silverlight where Activator.CreateInstance
        /// is SecuritySafeCritical.
        /// </summary>
        public static T CreateInstance<T>() {
            return default(T);
        }

        public static IAttributesCollection GetLocalDictionary(CodeContext context) {
            return context.Scope.Dict;
        }

        /// <summary>
        /// Initializes all but the 1st member of a environement tuple to Uninitialized.Instance
        /// 
        /// Called from generated code for environment initialization.
        /// </summary>
        public static void UninitializeEnvironmentTuple(NewTuple tuple) {
            for (int i = 1; i < tuple.Capacity; i++) {
                tuple.SetValue(i, Uninitialized.Instance);
            }
        }

        /// <summary>
        /// Initializes all but the 1st member of an environment array to Uninitialized.Instance.
        /// 
        /// Called from generated code for environment initialization.
        /// </summary>
        public static void UninitializeEnvironmentArray(object[] array) {
            for (int i = 1; i < array.Length; i++) {
                array[i] = Uninitialized.Instance;
            }
        }

        public static ReflectedEvent.BoundEvent MakeBoundEvent(ReflectedEvent eventObj, object instance, Type type) {
            return new ReflectedEvent.BoundEvent(eventObj, instance, DynamicHelpers.GetDynamicTypeFromType(type));
        }
    }
}
