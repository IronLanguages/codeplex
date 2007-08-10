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
using System.Threading;
using System.Globalization;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Shell;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Microsoft.Scripting {
    /// <summary>
    /// Provides language specific facilities which are typicalled called by the runtime.
    /// </summary>
    public abstract class LanguageContext : ICloneable {
        private static ModuleGlobalCache _noCache;
        [ThreadStatic]
        internal static List<Exception> _currentExceptions;

        /// <summary>
        /// Keeps track of exceptions being handled in interpreted mode (so we can support rethrow statements).
        /// </summary>
        [ThreadStatic]
        internal static List<Exception> _caughtExceptions;
        
        public virtual ActionBinder Binder {
            get { return Engine.DefaultBinder; }
        }

        public abstract ScriptEngine Engine { get; }

        /// <summary>
        /// Provides the ContextId which includes members that should only be shown for this LanguageContext.
        /// 
        /// ContextId's are used for filtering by DynamicType and Scope's.
        /// </summary>
        public virtual ContextId ContextId {
            get {
                return ContextId.Empty;
            }
        }

        protected LanguageContext() {
        }

        #region Module Context

        public ModuleContext GetModuleContext(ScriptModule module) {
            if (module == null) throw new ArgumentNullException("module");
            return module.GetModuleContext(ContextId);
        }
        
        public ModuleContext EnsureModuleContext(ScriptModule module) {
            if (module == null) throw new ArgumentNullException("module");
            ModuleContext context = module.GetModuleContext(ContextId);
            
            if (context == null) {
                context = CreateModuleContext(module);
                if (context == null) {
                    throw new InvalidImplementationException("CreateModuleContext must return a module context.");
                }
                return module.SetModuleContext(ContextId, context);
            }

            return context;
        }

        /// <summary>
        /// Notification sent when a ScriptCode is about to be executed within given ModuleContext.
        /// </summary>
        /// <param name="newContext"></param>
        public virtual void ModuleContextEntering(ModuleContext newContext) {
            // nop
        }

        /// <summary>
        /// Factory for ModuleContext creation. 
        /// It is guaranteed that this method is called once per each ScriptModule the langauge code is executed within.
        /// </summary>
        /// <param name="module">The module the context will be associated with.</param>
        /// <returns>Non-<c>null</c> module context instance.</returns>
        public virtual ModuleContext CreateModuleContext(ScriptModule module) {
            return new ModuleContext(module);
        }

        #endregion

        //TODO rename and comment
        public virtual ScriptCode CompileAst(CompilerContext context, CodeBlock body) {
            body.BindClosures();
#if DEBUG
            AstWriter.Dump(body, context);
#endif
            return new ScriptCode(body, this, context);
        }

        // TODO:
        public virtual ScriptCode Reload(ScriptCode original, ScriptModule module) {
            string path;
            SourceFileUnit sfu = original.SourceUnit as SourceFileUnit;
            if (sfu != null) {
                path = sfu.Path;
            } else {
                path = module.FileName;
            }

            ScriptEngine engine = this.Engine;
            if (engine == null) {
                engine = (ScriptEngine)original.SourceUnit.Engine;
            }

            SourceFileUnit su = new SourceFileUnit(engine, path, module.ModuleName, Encoding.Default);

            return ScriptCode.FromCompiledCode(su.Compile(engine.GetModuleCompilerOptions(module)));
        }

        /// <summary>
        /// Creates the language specific CompilerContext object for code compilation.  The 
        /// language should flow any relevant options from the LanguageContext to the 
        /// newly created CompilerContext.
        /// </summary>
        public virtual CompilerOptions GetCompilerOptions() {
            return Engine.GetDefaultCompilerOptions();
        }

        /// <summary>
        /// Looks up the name in the provided Scope using the current language's semantics.
        /// </summary>
        public virtual bool TryLookupName(CodeContext context, SymbolId name, out object value) {
            if (context.Scope.TryLookupName(this, name, out value)) {
                return true;
            }
            
            return TryLookupGlobal(context, name, out value);
        }

        /// <summary>
        /// Looks up the name in the provided scope using the current language's semantics.
        /// 
        /// If the name cannot be found throws the language appropriate exception or returns
        /// the language's appropriate default value.
        /// </summary>
        public virtual object LookupName(CodeContext context, SymbolId name) {
            object value;
            if (!TryLookupName(context, name, out value) || value == Uninitialized.Instance) {
                throw MissingName(name);
            }

            return value;
        }

        /// <summary>
        /// Attempts to set the name in the provided scope using the current language's semantics.
        /// </summary>
        public virtual void SetName(CodeContext context, SymbolId name, object value) {
            context.Scope.SetName(name, value);
        }

        /// <summary>
        /// Attempts to remove the name from the provided scope using the current language's semantics.
        /// </summary>
        public virtual bool RemoveName(CodeContext context, SymbolId name) {
            return context.Scope.RemoveName(this, name);
        }

        /// <summary>
        /// Attemps to lookup a global variable using the language's semantics called from
        /// the provided Scope.  The default implementation will attempt to lookup the variable
        /// at the host level.
        /// </summary>
        public virtual bool TryLookupGlobal(CodeContext context, SymbolId name, out object value) {
            return ScriptDomainManager.CurrentManager.Host.TryGetVariable(Engine, name, out value);
        }

        /// <summary>
        /// Called when a lookup has failed and an exception should be thrown.  Enables the 
        /// language context to throw the appropriate exception for their language when
        /// name lookup fails.
        /// </summary>
        protected internal virtual Exception MissingName(SymbolId name) {
            return new MissingMemberException(String.Format(CultureInfo.CurrentCulture, Resources.NameNotDefined, SymbolTable.IdToString(name)));
        }

        //TODO: - Review the design to see if these have to be made abstract
        #region Exception handling

        /// <summary>
        /// Called to get the exception to throw for the provided value.  Value depends upon the
        /// expression that is emitted in the ThrowStatement.  
        /// 
        /// For best results languages should map their exceptions as closely as possible to 
        /// .NET exceptions.  The created exception can have its original value stored in
        /// the Data property of the .NET exception.  The user can then be provided the original
        /// exception either via ExtractException or CheckException.
        /// 
        /// Returns the exception to be thrown.
        /// </summary>
        /// <param name="value">The language defined value to be thrown.</param>
        public virtual Exception ThrowException(object value) {
            return value as Exception ?? new Exception(value.ToString());   // TODO: Wrap value, can't throw RuntimeWrappedException            
        }
        
        /// <summary>
        /// Called once at the start of the catch block before evaluating the catch clauses.
        /// 
        /// The language can update any internal processing state here.
        /// 
        /// The return value of ExtractException will be passed to CheckException to
        /// perform the tests.  The return value of CheckException will be the value
        /// the ultimately sees.
        /// 
        /// The default implementation always returns the .NET Exception object.
        /// </summary>
        public virtual object PushExceptionHandler(CodeContext context, Exception exception) {
            return exception;
        }

        /// <summary>
        /// Called while processing an exception to see if the caught object is handled by
        /// the handler specified by the test object.  Return null if the exception should
        /// not be handled or the exception object expsoed to the user.
        /// 
        /// The default implementation checks to see if the test IsTrue and if so returns
        /// the exception object, otherwise returns null.
        /// </summary>        
        public virtual object CheckException(object exception, object test) {
            return IsTrue(test) ? exception : null;
        }

        /// <summary>
        /// Clears any exception handling state at the end of a catch block.  By default this
        /// function is a nop.
        /// </summary>
        public virtual void PopExceptionHandler() {
        }

        /// <summary>
        /// Gets the list of exceptions that are currently being handled by the user. 
        /// 
        /// These represent active catch blocks on the stack.
        /// </summary>
        protected List<Exception> CurrentExceptions {
            get {
                return _currentExceptions;
            }
        }

        #endregion

        /// <summary>
        /// Returns a ModuleGlobalCache for the given name.  
        /// 
        /// This cache enables fast access to global values when a SymbolId is not defined after searching the Scope's.  Usually
        /// a language implements lookup of the global value via TryLookupGlobal.  When GetModuleCache returns a ModuleGlobalCache
        /// a cached value can be used instead of calling TryLookupGlobal avoiding a possibly more expensive lookup from the 
        /// LanguageContext.  The ModuleGlobalCache can be held onto and have its value updated when the cache is invalidated.
        /// 
        /// By default this returns a cache which indicates no caching should occur and the LanguageContext will be 
        /// consulted when a module value is not available. If a LanguageContext only caches some values it can return 
        /// the value from the base method when the value should not be cached.
        /// </summary>
        protected internal virtual ModuleGlobalCache GetModuleCache(SymbolId name) {
            if (_noCache == null) {
                Interlocked.CompareExchange<ModuleGlobalCache>(ref _noCache, new ModuleGlobalCache(ModuleGlobalCache.NotCaching), null);
            }

            return _noCache;
        }

        #region ICloneable Members

        public virtual object Clone() {
            return MemberwiseClone();
        }

        #endregion

        public virtual bool IsTrue(object obj) {
            return false;
        }

        /// <summary>
        /// Delete the property 'name' from the object 'target'
        /// </summary>
        /// <param name="context"></param>
        /// <param name="target">Object on which it is to be deleted</param>
        /// <param name="name">property/member to be deleted</param>
        /// <returns>returns null by default.</returns>
        public virtual object DeleteMember(CodeContext context, object target, SymbolId name) {
            return null;
        }

        /// <summary>
        /// Get the value of the member with given name from the target
        /// </summary>
        /// <param name="context"></param>
        /// <param name="target">The target to get the member from</param>
        /// <param name="name">Name of the member</param>
        /// <returns></returns>
        public virtual object GetMember(CodeContext context, object target, SymbolId name) {
            return null;
        }

        /// <summary>
        /// Get the value of the bound member with given name from the target
        /// </summary>
        /// <param name="context"></param>
        /// <param name="target">The target to get the bound member from</param>
        /// <param name="name">Name of the member</param>
        /// <returns></returns>
        public virtual object GetBoundMember(CodeContext context, object target, SymbolId name) {
            return null;
        }

        /// <summary>
        /// Sets the value of the member with given name on the target.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="target">The target on which to set the member</param>
        /// <param name="name">Name of the member to set</param>
        /// <param name="value">The new value for the member</param>
        public virtual void SetMember(CodeContext context, object target, SymbolId name, object value) {
        }

        /// <summary>
        /// Calls the function with given arguments
        /// </summary>
        /// <param name="context"></param>
        /// <param name="function">The function to call</param>
        /// <param name="args">The argumetns with which to call the function.</param>
        /// <returns></returns>
        public virtual object Call(CodeContext context, object function, object[] args) {
            return null;
        }

        /// <summary>
        /// Calls the function with instance as the "this" value.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="function">The function to call</param>
        /// <param name="instance">The instance to pass as "this".</param>
        /// <param name="args">The rest of the arguments.</param>
        /// <returns></returns>
        public virtual object CallWithThis(CodeContext context, object function, object instance, object[] args) {
            return null;
        }

        /// <summary>
        /// Calls the function with arguments, extra arguments in tuple and dictionary of keyword arguments
        /// </summary>
        /// <param name="context"></param>
        /// <param name="func">The function to call</param>
        /// <param name="args">The arguments</param>
        /// <param name="names">Argument names</param>
        /// <param name="argsTuple">tuple of extra arguments</param>
        /// <param name="kwDict">keyword dictionary</param>
        /// <returns>The result of the function call.</returns>
        public virtual object CallWithArgsKeywordsTupleDict(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict) {
            return null;
        }

        /// <summary>
        /// Calls function with arguments and additional arguments in the tuple
        /// </summary>
        /// <param name="context"></param>
        /// <param name="func">The function to call</param>
        /// <param name="args">Argument array</param>
        /// <param name="argsTuple">Tuple with extra arguments</param>
        /// <returns>The result of calling the function "func"</returns>
        public virtual object CallWithArgsTuple(CodeContext context, object func, object[] args, object argsTuple) {
            return null;
        }

        /// <summary>
        /// Calls the function with arguments, some of which are keyword arguments.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="func">Function to call</param>
        /// <param name="args">Argument array</param>
        /// <param name="names">Names for some of the arguments</param>
        /// <returns>The result of calling the function "func"</returns>
        public virtual object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names) {
            return null;
        }

        // used only by ReflectedEvent.HandlerList
        public virtual bool EqualReturnBool(CodeContext context, object x, object y) {
            return false;
        }

        /// <summary>
        /// Gets the value or throws an exception when the provided MethodCandidate cannot be called.
        /// </summary>
        /// <returns></returns>
        public virtual object GetNotImplemented(params MethodCandidate []candidates) {
            throw new MissingMemberException("the specified operator is not implemented");
        }


        // used by DynamicHelpers.GetDelegate
        /// <summary>
        /// Checks whether the target is callable with given number of arguments.
        /// </summary>
        public void CheckCallable(object target, int argumentCount) {
            int min, max;
            if (!IsCallable(target, argumentCount, out min, out max)) {
                if (min == max) {
                    throw RuntimeHelpers.SimpleTypeError(String.Format("expected compatible function, but got parameter count mismatch (expected {0} args, target takes {1})", argumentCount, min));
                } else {
                    throw RuntimeHelpers.SimpleTypeError(String.Format("expected compatible function, but got parameter count mismatch (expected {0} args, target takes at least {1} and at most {2})", argumentCount, min, max));
                }
            }
        }

        public virtual bool IsCallable(object target, int argumentCount, out int min, out int max) {
            min = max = 0;
            return true;
        }

        public virtual Assembly LoadAssemblyFromFile(string file) {
#if SILVERLIGHT
            return null;
#else
            return Assembly.LoadFile(file);
#endif
        }
    }
}
