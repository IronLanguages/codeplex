using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using System.IO;
using Shapes;
using System.Reflection;

namespace ShapeScript {
    internal class Host {
        ScriptRuntime _runtime;
        ScriptEngine _engine;
        ScriptScope _theScope;

        MemoryStream _output;
        MemoryStream _error;

        public string ErrorFromLastExecution { get; set; }

        internal Host() {
            _output = new MemoryStream();
            _error = new MemoryStream();

            var configFile = Path.GetFullPath(Uri.UnescapeDataString(new Uri(typeof(Host).Assembly.CodeBase).AbsolutePath)) + ".config";
            
            _runtime = new ScriptRuntime( ScriptRuntimeSetup.ReadConfiguration(configFile));
            _engine = _runtime.GetEngine("py");
            _runtime.Globals.SetVariable("prompt", Form1.PromptString);

            _theScope = _engine.CreateScope();

            _runtime.IO.SetOutput(_output, new StreamWriter(_output));
            _runtime.IO.SetErrorOutput(_error, new StreamWriter(_error));

            _runtime.LoadAssembly(Assembly.GetAssembly(typeof(Circle)));
        }

        internal string ExecuteInCurrentScope(string snippet) {
            ScriptSource src = _engine.CreateScriptSourceFromString(
                        snippet,
                        SourceCodeKind.Statements);
            try {
                object o = src.Execute(_theScope);
            }
            catch (Exception ex) {
                ErrorFromLastExecution = ex.Message;
                return null;
            }
            return ReadOutput();
        }

        private string ReadOutput() {
            return ReadFromStream( _output);
        }

        private string ReadError() {
            return ReadFromStream(_error);
        }

        private string ReadFromStream(MemoryStream ms) {
            int length = (int)ms.Length;
            Byte[] bytes = new Byte[length];

            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(bytes, 0, (int)ms.Length);

            ms.SetLength(0);

            return Encoding.GetEncoding("utf-8").GetString(bytes, 0, (int)bytes.Length);
        }
    }
}
