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
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Shell;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {

    [Serializable]
    public class InvalidOptionException : Exception {
        public InvalidOptionException(string message) : base(message) {
        }

#if !SILVERLIGHT // SerializationInfo
        public InvalidOptionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    public abstract class OptionsParser {
       
        private List<string> _ignoredArgs = new List<string>();
        private ScriptDomainOptions _globalOptions;
        private string[] _args;
        private int _current = -1;

        protected OptionsParser() {
        }

        public ScriptDomainOptions GlobalOptions { 
            get { 
                return _globalOptions; 
            } set { 
                _globalOptions = value; 
            } 
        }
        
        public virtual ConsoleOptions ConsoleOptions { 
            get {
                throw new NotSupportedException(); 
            } 
            set {
                throw new NotSupportedException(); 
            } 
        }

        public virtual EngineOptions EngineOptions { 
            get { 
                throw new NotSupportedException(); 
            } set { 
                throw new NotSupportedException(); 
            } 
        }

        public virtual ConsoleOptions GetDefaultConsoleOptions() {
            throw new NotSupportedException();
        }

        public virtual EngineOptions GetDefaultEngineOptions() {
            throw new NotSupportedException();
        }

        public IList<string> IgnoredArgs { get { return _ignoredArgs; } }
        
        /// <exception cref="InvalidOptionException">On error.</exception>
        public virtual void Parse(string[] args) {
            if (args == null) throw new ArgumentNullException("args");

            if (_globalOptions == null) _globalOptions = new ScriptDomainOptions();

            _args = args;

            try {
                _current = 0;
                while (_current < args.Length) {
                    ParseArgument(args[_current++]);
                }
            } finally {
                _args = null;
                _current = -1;
            }
        }

        protected virtual void ParseArgument(string arg) {
            throw new NotImplementedException();
        }

        protected void IgnoreRemainingArgs() {
            while (_current < _args.Length) {
                _ignoredArgs.Add(_args[_current++]);
            }
        }

        protected string[] PopRemainingArgs() {
            string[] result = ArrayUtils.ShiftLeft(_args, _current);
            _current = _args.Length;
            return result;
        }

        protected string PeekNextArg() {
            if (_current < _args.Length)
                return _args[_current];
            else
                throw new InvalidOptionException(String.Format(CultureInfo.CurrentCulture, Resources.MissingOptionValue, _current > 0 ? _args[_current - 1] : ""));
        }

        protected string PopNextArg() {
            string result = PeekNextArg();
            _current++;
            return result;
        }

        protected void PushArgBack() {
            _current--;
        }

        protected static Exception InvalidOptionValue(string option, string value) {
            return new InvalidOptionException(String.Format(Resources.InvalidOptionValue, value, option));
        }

        public abstract void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments);

        public virtual void PrintHelp(TextWriter output) {
            if (output == null) throw new ArgumentNullException("output");

            string command_line, comments;
            string[,] options, environment_variables;

            GetHelp(out command_line, out options, out environment_variables, out comments);

            if (command_line != null) {
                output.WriteLine("{0}: {1}", Resources.Usage, command_line);
                output.WriteLine();
            }

            if (options != null) {
                output.WriteLine("{0}:", Resources.Options);
                ArrayUtils.PrintTable(output, options);
                output.WriteLine();
            }

            if (environment_variables != null) {
                output.WriteLine("{0}:", Resources.EnvironmentVariables);
                ArrayUtils.PrintTable(output, environment_variables);
                output.WriteLine();
            }

            if (comments != null) {
                output.Write(comments);
                output.WriteLine();
            }
        }
    }
}