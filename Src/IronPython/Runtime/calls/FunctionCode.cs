/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using IronPython.Runtime.Operations;
using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Hosting;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using System.Runtime.InteropServices;

namespace IronPython.Runtime.Calls {
    /// <summary>
    /// Represents a piece of code.  This can reference either a CompiledCode
    /// object or a Function.   The user can explicitly call FunctionCode by
    /// passing it into exec or eval.
    /// </summary>
    [PythonType("code")]
    public class FunctionCode {
        #region Private member variables
        private Delegate _target;
        //private Statement _body;

        private object _varnames;
        private ScriptCode _code;
        private PythonFunction _func;
        private string _filename;
        private int _lineNo;
        private FunctionAttributes _flags;      // future division, generator
        #endregion
        private static DynamicSite<PythonFunction, object> _callSite = RuntimeHelpers.CreateSimpleCallSite<PythonFunction, object>();

        internal FunctionCode(Delegate target) {
            _target = target;            
        }

        internal FunctionCode(ScriptCode code, CompileFlags compilerFlags)
            : this(code) {

            if ((compilerFlags & CompileFlags.CO_FUTURE_DIVISION) != 0)
                _flags |= FunctionAttributes.FutureDivision;
        }

        internal FunctionCode(ScriptCode code) {
            _code = code;
        }

        internal FunctionCode(PythonFunction f) {
            _func = f;
        }

        #region Public constructors

        /*
        /// <summary>
        /// Standard python siganture
        /// </summary>
        /// <param name="argcount"></param>
        /// <param name="nlocals"></param>
        /// <param name="stacksize"></param>
        /// <param name="flags"></param>
        /// <param name="codestring"></param>
        /// <param name="constants"></param>
        /// <param name="names"></param>
        /// <param name="varnames"></param>
        /// <param name="filename"></param>
        /// <param name="name"></param>
        /// <param name="firstlineno"></param>
        /// <param name="nlotab"></param>
        /// <param name="freevars"></param>
        /// <param name="callvars"></param>
        public FunctionCode(int argcount, int nlocals, int stacksize, int flags, string codestring, object constants, Tuple names, Tuple varnames, string filename, string name, int firstlineno, object nlotab, [DefaultParameterValue(null)]object freevars, [DefaultParameterValue(null)]object callvars) {
        }*/

        #endregion

        #region Public Python API Surface

        public object VarNames {
            [PythonName("co_varnames")]
            get {
                if (_varnames == null) {
                    _varnames = GetArgNames();
                }
                return _varnames;
            }
        }

        public object ArgCount {
            [PythonName("co_argcount")]
            get {
                if (_code != null) return 0;
                return _func.ArgNames.Length;
            }
        }

        public object CallVars {
            [PythonName("co_cellvars")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object Code {
            [PythonName("co_code")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object Consts {
            [PythonName("co_consts")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object Filename {
            [PythonName("co_filename")]
            get {
                return _filename;
            }
        }

        public object FirstLineNumber {
            [PythonName("co_firstlineno")]
            get {
                return _lineNo;
            }
        }

        public object Flags {
            [PythonName("co_flags")]
            get {
                return (int)_flags;
            }
        }

        public object FreeVars {
            [PythonName("co_freevars")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object LineNumberTab {
            [PythonName("co_lnotab")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object Name {
            [PythonName("co_name")]
            get {
                if (_func != null) return _func.Name;
                if (_code != null) return _code.GetType().Name;

                throw PythonOps.NotImplementedError("");
            }
        }

        public object Names {
            [PythonName("co_names")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object NumberLocals {
            [PythonName("co_nlocals")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }

        public object StackSize {
            [PythonName("co_stacksize")]
            get {
                throw PythonOps.NotImplementedError("");
            }
        }
        #endregion

        #region Public setters called from PythonFunction factory method
        internal void SetFilename(string sourceFile) {
            _filename = sourceFile;
        }

        internal void SetLineNumber(int line) {
            _lineNo = line;
        }

        internal void SetFlags(FunctionAttributes flags) {
            _flags = flags;
        }

        #endregion

        #region Internal API Surface

        public object Call(CodeContext context, Microsoft.Scripting.Scope scope, bool tryEvaluate) {
            if (_code != null) {
                return _code.Run(scope, context.ModuleContext, tryEvaluate);
            } else if (_func != null) {
                return _callSite.Invoke(context, _func);
            }

            throw PythonOps.TypeError("bad code");
        }

        #endregion

        #region Private helper functions
        private PythonTuple GetArgNames() {
            if (_code != null) return PythonTuple.MakeTuple();

            List<string> names = new List<string>();
            List<PythonTuple> nested = new List<PythonTuple>();


            for (int i = 0; i < _func.ArgNames.Length; i++) {
                if (_func.ArgNames[i].IndexOf('#') != -1 && _func.ArgNames[i].IndexOf('!') != -1) {
                    names.Add("." + (i * 2));
                    // TODO: need to get local variable names here!!!
                    //nested.Add(FunctionDefinition.DecodeTupleParamName(func.ArgNames[i]));
                } else {
                    names.Add(_func.ArgNames[i]);
                }
            }

            for (int i = 0; i < nested.Count; i++) {
                ExpandArgsTuple(names, nested[i]);
            }
            return PythonTuple.Make(names);
        }

        private void ExpandArgsTuple(List<string> names, PythonTuple toExpand) {
            for (int i = 0; i < toExpand.Count; i++) {
                if (toExpand[i] is PythonTuple) {
                    ExpandArgsTuple(names, toExpand[i] as PythonTuple);
                } else {
                    names.Add(toExpand[i] as string);
                }
            }
        }
        #endregion

        public override bool Equals(object obj) {
            FunctionCode other = obj as FunctionCode;
            if (other == null) return false;

            if (_code != null) {
                return _code == other._code;
            } else if (_func != null) {
                return _func == other._func;
            }

            throw PythonOps.TypeError("bad code");
        }

        public override int GetHashCode() {
            if (_code != null) {
                return _code.GetHashCode();
            } else if (_func != null) {
                return _func.GetHashCode();
            }

            throw PythonOps.TypeError("bad code");
        }
    }
}
