/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting.Shell {
    internal class DefaultOptionsParser : OptionsParser {
        private ConsoleOptions _consoleOptions;
        private EngineOptions _engineOptions;

        public override ConsoleOptions ConsoleOptions {
            get { return _consoleOptions; }
            set { _consoleOptions = value; }
        }

        public override EngineOptions EngineOptions {
            get { return _engineOptions; }
            set { _engineOptions = value; }
        }

        public DefaultOptionsParser() {
        }

        public override void Parse(string[] args) {
            if (_consoleOptions == null) _consoleOptions = GetDefaultConsoleOptions();
            if (_engineOptions == null) _engineOptions = GetDefaultEngineOptions();

            base.Parse(args);
        }

        /// <exception cref="Exception">On error.</exception>
        protected override void ParseArgument(string arg) {
            ContractUtils.RequiresNotNull(arg, "arg");

            base.ParseArgument(arg);
        }
    }
}
