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

        private object varnames;
        private ScriptCode _code;
        private PythonFunction func;
        private string filename;
        private int lineNo;
        private FunctionAttributes flags;      // future division, generator
        #endregion

        internal FunctionCode(Delegate target) {
            _target = target;            
        }

        internal FunctionCode(ScriptCode code, CompileFlags compilerFlags)
            : this(code) {

            if ((compilerFlags & CompileFlags.CO_FUTURE_DIVISION) != 0)
                flags |= FunctionAttributes.FutureDivision;
        }

        internal FunctionCode(ScriptCode code) {
            this._code = code;
        }

        internal FunctionCode(PythonFunction f) {
            this.func = f;
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
                if (varnames == null) {
                    varnames = GetArgNames();
                }
                return varnames;
            }
        }

        public object ArgCount {
            [PythonName("co_argcount")]
            get {
                if (_code != null) return 0;
                return func.ArgNames.Length;
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
                return filename;
            }
        }

        public object FirstLineNumber {
            [PythonName("co_firstlineno")]
            get {
                return lineNo;
            }
        }

        public object Flags {
            [PythonName("co_flags")]
            get {
                FunctionAttributes res = flags;
                FunctionN funcN = func as FunctionN;
                FunctionX funcX = func as FunctionX;
                if (funcX != null) {
                    res |= funcX.Flags;
                } else if (funcN != null) res |= FunctionAttributes.ArgumentList;
                
                return (int)res;
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
                if (func != null) return func.Name;
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
            filename = sourceFile;
        }

        internal void SetLineNumber(int line) {
            lineNo = line;
        }

        // This is only used to set the value of FutureDivision and Generator flags
        internal void SetFlags(CodeContext context, int value) {
            this.flags = (FunctionAttributes)value & (FunctionAttributes.FutureDivision | FunctionAttributes.Generator);
            if (((PythonContext)context.LanguageContext).TrueDivision) this.flags |= FunctionAttributes.FutureDivision;
        }

        #endregion

        #region Internal API Surface

        public object Call(CodeContext context, Microsoft.Scripting.Scope scope) {
            if (_code != null) {
                return _code.Run(scope, context.ModuleContext);
            } else if (func != null) {
                return func.Call(context);
            }

            throw PythonOps.TypeError("bad code");
        }

        #endregion

        #region Private helper functions
        private Tuple GetArgNames() {
            if (_code != null) return Tuple.MakeTuple();

            List<string> names = new List<string>();
            List<Tuple> nested = new List<Tuple>();


            for (int i = 0; i < func.ArgNames.Length; i++) {
                if (func.ArgNames[i].IndexOf('#') != -1 && func.ArgNames[i].IndexOf('!') != -1) {
                    names.Add("." + (i * 2));
                    // TODO: need to get local variable names here!!!
                    //nested.Add(FunctionDefinition.DecodeTupleParamName(func.ArgNames[i]));
                } else {
                    names.Add(func.ArgNames[i]);
                }
            }

            for (int i = 0; i < nested.Count; i++) {
                ExpandArgsTuple(names, nested[i]);
            }
            return Tuple.Make(names);
        }

        private void ExpandArgsTuple(List<string> names, Tuple toExpand) {
            for (int i = 0; i < toExpand.Count; i++) {
                if (toExpand[i] is Tuple) {
                    ExpandArgsTuple(names, toExpand[i] as Tuple);
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
            } else if (func != null) {
                return func == other.func;
            }

            throw PythonOps.TypeError("bad code");
        }

        public override int GetHashCode() {
            if (_code != null) {
                return _code.GetHashCode();
            } else if (func != null) {
                return func.GetHashCode();
            }

            throw PythonOps.TypeError("bad code");
        }
    }
}
