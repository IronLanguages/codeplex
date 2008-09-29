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

using System; using Microsoft;
using System.Collections.Generic;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;

#if SILVERLIGHT
[assembly: DynamicLanguageProvider(typeof(PythonContext), PythonContext.IronPythonDisplayName, PythonContext.IronPythonNames, PythonContext.IronPythonFileExtensions)]
#endif

namespace IronPython.Hosting {

    /// <summary>
    /// Provides helpers for interacting with IronPython.
    /// </summary>
    public static class Python {

        #region Public APIs

        /// <summary>
        /// Creates a new ScriptRuntime with the IronPython scipting engine pre-configured.
        /// </summary>
        /// <returns></returns>
        public static ScriptRuntime/*!*/ CreateRuntime() {
            return new ScriptRuntime(CreateRuntimeSetup(null));
        }

        /// <summary>
        /// Creates a new ScriptRuntime with the IronPython scipting engine pre-configured and
        /// additional options.
        /// </summary>
        public static ScriptRuntime/*!*/ CreateRuntime(IDictionary<string, object> options) {
            return new ScriptRuntime(CreateRuntimeSetup(options));
        }

#if !SILVERLIGHT
        /// <summary>
        /// Creates a new ScriptRuntime with the IronPython scripting engine pre-configured
        /// in the specified AppDomain.  The remote ScriptRuntime may  be manipulated from 
        /// the local domain but all code will run in the remote domain.
        /// </summary>
        public static ScriptRuntime/*!*/ CreateRuntime(AppDomain/*!*/ domain) {
            ContractUtils.RequiresNotNull(domain, "domain");

            return ScriptRuntime.CreateRemote(domain, CreateRuntimeSetup(null));
        }

        /// <summary>
        /// Creates a new ScriptRuntime with the IronPython scripting engine pre-configured
        /// in the specified AppDomain with additional options.  The remote ScriptRuntime may 
        /// be manipulated from the local domain but all code will run in the remote domain.
        /// </summary>
        public static ScriptRuntime/*!*/ CreateRuntime(AppDomain/*!*/ domain, IDictionary<string, object> options) {
            ContractUtils.RequiresNotNull(domain, "domain");

            return ScriptRuntime.CreateRemote(domain, CreateRuntimeSetup(options));
        }

#endif

        /// <summary>
        /// Creates a new ScriptRuntime and returns the ScriptEngine for IronPython. If
        /// the ScriptRuntime is requierd it can be acquired from the Runtime property
        /// on the engine.
        /// </summary>
        public static ScriptEngine/*!*/ CreateEngine() {
            return GetEngine(CreateRuntime());
        }

        /// <summary>
        /// Creates a new ScriptRuntime with the specified options and returns the 
        /// ScriptEngine for IronPython. If the ScriptRuntime is requierd it can be 
        /// acquired from the Runtime property on the engine.
        /// </summary>
        public static ScriptEngine/*!*/ CreateEngine(IDictionary<string, object> options) {
            return GetEngine(CreateRuntime(options));
        }

#if !SILVERLIGHT

        /// <summary>
        /// Creates a new ScriptRuntime and returns the ScriptEngine for IronPython. If
        /// the ScriptRuntime is requierd it can be acquired from the Runtime property
        /// on the engine.
        /// 
        /// The remote ScriptRuntime may be manipulated from the local domain but 
        /// all code will run in the remote domain.
        /// </summary>
        public static ScriptEngine/*!*/ CreateEngine(AppDomain/*!*/ domain) {
            return GetEngine(CreateRuntime(domain));
        }

        /// <summary>
        /// Creates a new ScriptRuntime with the specified options and returns the 
        /// ScriptEngine for IronPython. If the ScriptRuntime is requierd it can be 
        /// acquired from the Runtime property on the engine.
        /// 
        /// The remote ScriptRuntime may be manipulated from the local domain but 
        /// all code will run in the remote domain.
        /// </summary>
        public static ScriptEngine/*!*/ CreateEngine(AppDomain/*!*/ domain, IDictionary<string, object> options) {
            return GetEngine(CreateRuntime(domain, options));
        }

#endif

        /// <summary>
        /// Given a ScriptRuntime gets the ScriptEngine for IronPython.
        /// </summary>
        public static ScriptEngine/*!*/ GetEngine(ScriptRuntime/*!*/ runtime) {
            return runtime.GetEngineByTypeName(typeof(PythonContext).AssemblyQualifiedName);
        }

        /// <summary>
        /// Gets a ScriptScope which is the Python sys module for the provided ScriptRuntime.
        /// </summary>
        public static ScriptScope/*!*/ GetSysModule(this ScriptRuntime/*!*/ runtime) {
            ContractUtils.RequiresNotNull(runtime, "runtime");

            return GetSysModule(GetEngine(runtime));
        }

        /// <summary>
        /// Gets a ScriptScope which is the Python sys module for the provided ScriptEngine.
        /// </summary>
        public static ScriptScope/*!*/ GetSysModule(this ScriptEngine/*!*/ engine) {
            ContractUtils.RequiresNotNull(engine, "engine");

            return GetPythonService(engine).GetSystemState();
        }

        /// <summary>
        /// Gets a ScriptScope which is the Python __builtin__ module for the provided ScriptRuntime.
        /// </summary>
        public static ScriptScope/*!*/ GetBuiltinModule(this ScriptRuntime/*!*/ runtime) {
            ContractUtils.RequiresNotNull(runtime, "runtime");

            return GetBuiltinModule(GetEngine(runtime));
        }

        /// <summary>
        /// Gets a ScriptScope which is the Python __builtin__ module for the provided ScriptEngine.
        /// </summary>
        public static ScriptScope/*!*/ GetBuiltinModule(this ScriptEngine/*!*/ engine) {
            ContractUtils.RequiresNotNull(engine, "engine");

            return GetPythonService(engine).GetBuiltins();
        }

        /// <summary>
        /// Gets a ScriptScope which is the Python clr module for the provided ScriptRuntime.
        /// </summary>
        public static ScriptScope/*!*/ GetClrModule(this ScriptRuntime/*!*/ runtime) {
            ContractUtils.RequiresNotNull(runtime, "runtime");

            return GetClrModule(GetEngine(runtime));
        }

        /// <summary>
        /// Gets a ScriptScope which is the Python clr module for the provided ScriptEngine.
        /// </summary>
        public static ScriptScope/*!*/ GetClrModule(this ScriptEngine/*!*/ engine) {
            ContractUtils.RequiresNotNull(engine, "engine");

            return GetPythonService(engine).GetClr();
        }

        /// <summary>
        /// Imports the Python module by the given name and returns its ScriptSCope.  If the 
        /// module does not exist an exception is raised.
        /// </summary>
        public static ScriptScope/*!*/ ImportModule(this ScriptRuntime/*!*/ runtime, string/*!*/ moduleName) {
            ContractUtils.RequiresNotNull(runtime, "runtime");
            ContractUtils.RequiresNotNull(moduleName, "moduleName");

            return ImportModule(GetEngine(runtime), moduleName);
        }

        /// <summary>
        /// Imports the Python module by the given name and returns its ScriptSCope.  If the 
        /// module does not exist an exception is raised.
        /// </summary>
        public static ScriptScope/*!*/ ImportModule(this ScriptEngine/*!*/ engine, string/*!*/ moduleName) {
            ContractUtils.RequiresNotNull(engine, "engine");
            ContractUtils.RequiresNotNull(moduleName, "moduleName");

            return GetPythonService(engine).ImportModule(engine, moduleName);
        }

        #endregion

        #region Private helpers

        public static ScriptRuntimeSetup/*!*/ CreateRuntimeSetup(IDictionary<string, object> options) {
            ScriptRuntimeSetup setup = new ScriptRuntimeSetup();
            setup.LanguageSetups.Add(CreateLanguageSetup(options));

            if (options != null) {
                object value;
                if (options.TryGetValue("Debug", out value) &&
                    value is bool &&
                    (bool)value) {
                    setup.DebugMode = true;
                }

                if (options.TryGetValue("PrivateBinding", out value) &&
                    value is bool &&
                    (bool)value) {
                    setup.PrivateBinding = true;
                }
            }

            return setup;
        }

        public static LanguageSetup/*!*/ CreateLanguageSetup(IDictionary<string, object> options) {
            var setup = new LanguageSetup(
                typeof(PythonContext).AssemblyQualifiedName,
                PythonContext.IronPythonDisplayName,
                PythonContext.IronPythonNames.Split(';'),
                PythonContext.IronPythonFileExtensions.Split(';')
            );

            if (options != null) {
                foreach (var entry in options) {
                    setup.Options.Add(entry.Key, entry.Value);
                }
            }

            return setup;
        }

        private static PythonService/*!*/ GetPythonService(ScriptEngine/*!*/ engine) {
            return (PythonService)HostingHelpers.CallEngine<ScriptEngine, object>(
                engine,
                (context, arg) => ((PythonContext)context).GetPythonService(arg),
                engine
            );
        }

        #endregion
    }
}
