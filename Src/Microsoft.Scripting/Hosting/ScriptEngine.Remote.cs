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
#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Scripting.Actions;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;

namespace Microsoft.Scripting.Hosting {

    internal sealed class RemoteScriptEngine : RemoteWrapper, IScriptEngine {
        private readonly ScriptEngine _engine;

        public override ILocalObject LocalObject {
            get { return _engine; }
        }

        public RemoteScriptEngine(ScriptEngine engine) {
            if (engine == null) throw new ArgumentNullException("engine");

            _engine = engine;
        }

        // TODO:
        public override object InitializeLifetimeService() {
            return null;
        }

        public ILanguageProvider LanguageProvider {
            get { return new RemoteLanguageProvider(_engine.LanguageProvider); }
        }
        
        public Guid LanguageGuid {
            get { return _engine.LanguageGuid; }
        }

        public Guid VendorGuid {
            get { return _engine.VendorGuid; }
        }

        public string VersionString {
            get { return _engine.VersionString; }
        }

        public EngineOptions Options {
            get { return _engine.Options; }
        }

        public void SetSourceUnitSearchPaths(string[] paths) {
            _engine.SetSourceUnitSearchPaths(paths);
        }

        // TODO: source unit
        public StreamReader GetSourceReader(Stream stream, ref Encoding encoding) {
            return _engine.GetSourceReader(stream, ref encoding);
        }

        // TODO: remove
        public ActionBinder DefaultBinder {
            get { return _engine.DefaultBinder; }
        }

        public string[] GetObjectCallSignatures(object obj) {
            return _engine.GetObjectCallSignatures(obj);
        }

        public string[] GetObjectMemberNames(object obj) {
            return _engine.GetObjectMemberNames(obj);
        }
        
        public string[] GetObjectMemberNames(object obj, IScriptModule module) {
            return _engine.GetObjectMemberNames(obj, module);
        }

        public string GetObjectDocumentation(object obj) {
            return _engine.GetObjectDocumentation(obj);
        }

        public bool IsObjectCallable(object obj) {
            return _engine.IsObjectCallable(obj);
        }

        public bool IsObjectCallable(object obj, IScriptModule module) {
            return _engine.IsObjectCallable(obj, module);
        }

        public object CallObject(object obj, params object[] args) {
            return _engine.CallObject(obj, args);
        }

        public object CallObject(object obj, IScriptModule module, params object[] args) {
            return _engine.CallObject(obj, module, args);
        }

        public bool TryGetObjectMemberValue(object obj, string name, out object value) {
            return _engine.TryGetObjectMemberValue(obj, name, out value);
        }

        public bool TryGetObjectMemberValue(object obj, string name, IScriptModule module, out object value) {
            return _engine.TryGetObjectMemberValue(obj, name, module, out value);
        }
        
        public string[] GetObjectCallSignatures(IObjectHandle obj) {
            return _engine.GetObjectCallSignatures(obj);
        }

        public string[] GetObjectMemberNames(IObjectHandle obj) {
            return _engine.GetObjectMemberNames(obj);
        }

        public string[] GetObjectMemberNames(IObjectHandle obj, IScriptModule module) {
            return _engine.GetObjectMemberNames(obj, module);
        }

        public bool TryGetObjectMemberValue(IObjectHandle obj, string name, out IObjectHandle value) {
            return _engine.TryGetObjectMemberValue(obj, name, out value);
        }

        public bool TryGetObjectMemberValue(IObjectHandle obj, string name, IScriptModule module, out IObjectHandle value) {
            return _engine.TryGetObjectMemberValue(obj, name, module, out value);
        }

        public string GetObjectDocumentation(IObjectHandle obj) {
            return _engine.GetObjectDocumentation(obj);
        }

        public bool IsObjectCallable(IObjectHandle obj) {
            return _engine.IsObjectCallable(obj);
        }

        public bool IsObjectCallable(IObjectHandle obj, IScriptModule module) {
            return _engine.IsObjectCallable(obj, module);
        }

        public IObjectHandle CallObject(IObjectHandle obj, params object[] args) {
            return _engine.CallObject(obj, args);
        }
        
        public IObjectHandle CallObject(IObjectHandle obj, IScriptModule module, params object[] args) {
            return _engine.CallObject(obj, module, args);
        }

        public void ExecuteFile(string code) {
            _engine.ExecuteFile(code);
        }

        public void ExecuteFileContent(string code) {
            _engine.ExecuteFileContent(code);
        }

        public void ExecuteFileContent(string code, IScriptModule module) {
            _engine.ExecuteFileContent(code, module);
        }
        
        public void Execute(string code) {
            _engine.Execute(code);
        }
        
        public void Execute(string code, IScriptModule module) {
            _engine.Execute(code, module);
        }

        public void ExecuteInteractiveCode(string code) {
            _engine.ExecuteInteractiveCode(code);
        }

        public void ExecuteInteractiveCode(string code, IScriptModule module) {
            _engine.ExecuteInteractiveCode(code, module);
        }

        public ICompiledCode CompileInteractiveCode(string code) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.CompileInteractiveCode(code));
        }

        public ICompiledCode CompileInteractiveCode(string code, IScriptModule module) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.CompileInteractiveCode(code, module));
        }

        public InteractiveCodeProperties GetInteractiveCodeProperties(string code) {
            return _engine.GetInteractiveCodeProperties(code);
        }
        

        // throws SerializationException 
        public object Evaluate(string expression) {
            return _engine.Evaluate(expression);
        }

        // throws SerializationException 
        public object Evaluate(string expression, IScriptModule module) {
            return _engine.Evaluate(expression, module);
        }

        // throws SerializationException
        public bool TryGetVariable(string name, IScriptModule module, out object obj) {
            return _engine.TryGetVariable(name, module, out obj);
        }

        public bool TryGetVariableAndWrap(string name, IScriptModule module, out IObjectHandle obj) {
            return _engine.TryGetVariableAndWrap(name, module, out obj);
        }

        public IObjectHandle EvaluateAndWrap(string expression) {
            return _engine.EvaluateAndWrap(expression);
        }

        public IObjectHandle EvaluateAndWrap(string expression, IScriptModule module) {
            return _engine.EvaluateAndWrap(expression, module);
        }

        public IScriptModule CompileFile(string path) {
            return RemoteWrapper.WrapRemotable<IScriptModule>(_engine.CompileFile(path));
        }

        public ICompiledCode CompileFileContent(string path) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.CompileFileContent(path));
        }

        public ICompiledCode CompileFileContent(string path, IScriptModule module) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.CompileFileContent(path, module));
        }
        
        public ICompiledCode CompileCode(string code) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.CompileCode(code));
        }
        
        public ICompiledCode CompileCode(string code, IScriptModule module) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.CompileCode(code, module));
        }

        public ICompiledCode CompileExpression(string expression, IScriptModule module) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.CompileExpression(expression, module));
        }

        public ICompiledCode CompileStatement(string statement, IScriptModule module) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.CompileStatement(statement, module));
        }

        public ICompiledCode CompileCodeDom(System.CodeDom.CodeMemberMethod code, IScriptModule module) {
            return RemoteWrapper.WrapRemotable<ICompiledCode>(_engine.CompileCodeDom(code, module));
        }

        public int ExecuteProgram(SourceUnit sourceUnit) {
            return _engine.ExecuteProgram(sourceUnit);
        }

        public void ExecuteCommand(string code) {
            _engine.ExecuteCommand(code);
        }

        public void ExecuteCommand(string code, IScriptModule module) {
            _engine.ExecuteCommand(code, module);
        }

        public TextWriter GetOutputWriter(bool isErrorOutput) {
            throw new NotImplementedException("TODO");
        }

        // TODO: remove
        public SourceUnit CreateStandardInputSourceUnit(string code) {
            return _engine.CreateStandardInputSourceUnit(code);
        }

        public ErrorSink GetDefaultErrorSink() {
            return _engine.GetDefaultErrorSink();
        }

        public void Shutdown() {
            _engine.Shutdown();
        }

        public void AddAssembly(Assembly assembly) {
            _engine.AddAssembly(assembly);
        }

        public string FormatException(Exception exception) {
            return _engine.FormatException(exception);
        }

        public void PublishModule(IScriptModule module) {
            _engine.PublishModule(module);
        }

        public CompilerOptions GetDefaultCompilerOptions() {
            return _engine.GetDefaultCompilerOptions();
        }

        public CompilerOptions GetModuleCompilerOptions(ScriptModule module) {
            throw new NotSupportedException();
        }
    }
}

#endif
