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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Hosting;

using IronPython.Hosting;
using IronPython.Compiler;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    public static class PythonOps {

        /// <summary>
        /// Singleton Ellipsis object of EllipsisType.
        /// Initialized after type has been created in static constructor
        /// 
        /// TODO: Cannot access EllipsisTypeOps.Instance from here
        /// </summary>
        public static readonly object Ellipsis = Ops.Ellipsis;

        private static object FindMetaclass(CodeContext context, Tuple bases, IAttributesCollection dict) {
            // If dict['__metaclass__'] exists, it is used. 
            object ret;
            if (dict.TryGetValue(Symbols.MetaClass, out ret) && ret != null) return ret;

            //Otherwise, if there is at least one base class, its metaclass is used
            for (int i = 0; i < bases.Count; i++) {
                if (!(bases[i] is OldClass)) return Ops.GetDynamicType(bases[i]);
            }

            //Otherwise, if there's a global variable named __metaclass__, it is used.
            if (context.Scope.GlobalScope.TryLookupName(context.LanguageContext, Symbols.MetaClass, out ret) && ret != null) {
                return ret;
            }

            //Otherwise, the classic metaclass (types.ClassType) is used.
            return TypeCache.OldInstance;
        }

        public static object MakeClass(CodeContext context, string name, object[] bases, string selfNames, Delegate body) {
            CodeContext bodyContext;
            CallTarget0 target = body as CallTarget0;
            if (target != null) {
                bodyContext = (CodeContext)target();
            } else {
                bodyContext = (CodeContext)((CallTargetWithContext0)body)(context);
            }

            // TODO: Remove dependency on context.Locals
            IAttributesCollection vars = bodyContext.Locals;

            foreach (object dt in bases) {
                if (dt is TypeCollision) {
                    object[] newBases = new object[bases.Length];
                    for (int i = 0; i < bases.Length; i++) {
                        TypeCollision tc = bases[i] as TypeCollision;
                        if (tc != null) newBases[i] = Ops.GetDynamicTypeFromType(tc.DefaultType);
                        else newBases[i] = bases[i];
                    }
                    bases = newBases;
                    break;
                }
            }
            Tuple tupleBases = Tuple.MakeTuple(bases);

            object metaclass = FindMetaclass(context, tupleBases, vars);
            if (metaclass == TypeCache.OldInstance)
                return new OldClass(name, tupleBases, vars, selfNames);
            if (metaclass == TypeCache.DynamicType)
                return UserTypeBuilder.Build(context, name, tupleBases, vars);

            // eg:
            // def foo(*args): print args            
            // __metaclass__ = foo
            // class bar: pass
            // calls our function...
            return Ops.CallWithContext(context, metaclass, name, tupleBases, vars);
        }

        /// <summary>
        /// Python runtime helper for raising assertions. Used by AssertStatement.
        /// </summary>
        /// <param name="msg">Object representing the assertion message</param>
        public static void RaiseAssertionError(object msg) {
            string message = Ops.ToString(msg);
            if (message == null) {
                throw Ops.AssertionError(String.Empty, Ops.EmptyObjectArray);
            } else {
                throw Ops.AssertionError("{0}", new object[] { message });
            }
        }

        /// <summary>
        /// Support for with statement
        /// </summary>
        public static Tuple ExtractSysExcInfo(CodeContext context) {
            return SystemState.Instance.ExceptionInfo(context);
        }

        /// <summary>
        /// Python runtime helper which provides string representation of an object.
        /// </summary>
        /// <param name="o">Object to represent as a string</param>
        public static object Repr(object o) {
            return Ops.StringRepr(o);
        }

        /// <summary>
        /// Python runtime helper to create Python dictionary of an initial size.
        /// </summary>
        /// <param name="size">Initial capacity of the dictionary.</param>
        /// <returns>PythonDictionary</returns>
        public static PythonDictionary MakeDict(int size) {
            // TODO: This must call Ops helper because the Dictionary constructor
            // must stay internal (else dictionary constructors will get confused in
            // reflected type). Once PythonDictionary moves out of vestigial
            // to IronPython.dll, this can call new PythonDictionary(size) directly.
            return Ops.MakeDict(size);
        }

        /// <summary>
        /// Python runtime helper to create instance of Python List object.
        /// </summary>
        /// <returns>New instance of List</returns>
        public static List MakeList() {
            return List.MakeEmptyList(10);
        }

        /// <summary>
        /// Python runtime helper to create a populated instnace of Python List object.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List MakeList(params object[] items) {
            return List.MakeList(items);
        }

        /// <summary>
        /// Python runtime helper to create an instance of Tuple
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Tuple MakeTuple(params object[] items) {
            return Tuple.MakeTuple(items);
        }

        /// <summary>
        /// Python runtime helper to create an instance of an expandable tuple,
        /// tuple which can be expanded into individual elements for use in the
        /// context:    x[1, 2, 3]
        /// when calling .NET indexers
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Tuple MakeExpandableTuple(params object[] items) {
            return Ops.MakeExpandableTuple(items);
        }

        /// <summary>
        /// Python Runtime Helper for enumerator unpacking (tuple assignments, ...)
        /// Creates enumerator from the input parameter e, and then extracts 
        /// expected number of values, returning them as array
        /// </summary>
        /// <param name="e">object to enumerate</param>
        /// <param name="expected">expected number of objects to extract from the enumerator</param>
        /// <returns>
        /// array of objects (.Lengh == expected) if exactly expected objects are in the enumerator.
        /// Otherwise throws exception
        /// </returns>
        public static object[] GetEnumeratorValues(object e, int expected) {
            IEnumerator ie = Ops.GetEnumeratorForUnpack(e);

            int count = 0;
            object[] values = new object[expected];

            while (count < expected) {
                if (!ie.MoveNext()) {
                    throw Ops.ValueErrorForUnpackMismatch(expected, count);
                }
                values[count] = ie.Current;
                count++;
            }

            if (ie.MoveNext()) {
                throw Ops.ValueErrorForUnpackMismatch(expected, count + 1);
            }

            return values;
        }

        /// <summary>
        /// Python runtime helper to create instance of Slice object
        /// </summary>
        /// <param name="start">Start of the slice.</param>
        /// <param name="stop">End of the slice.</param>
        /// <param name="step">Step of the slice.</param>
        /// <returns>Slice</returns>
        public static object MakeSlice(object start, object stop, object step) {
            return new Slice(start, stop, step);
        }

        public static Exception MakeException(CodeContext context, object type, object value, object traceback) {
            PythonContext pc = (PythonContext)context.LanguageContext;
            return pc.MakeException(type, value, traceback);
        }

        #region Print support

        /// <summary>
        /// Prints value into default standard output
        /// </summary>
        /// <param name="o"></param>
        public static void Print(object o) {
            SystemState state = SystemState.Instance;

            PrintWithDest(state.stdout, o);
        }

        /// <summary>
        ///  Prints value into specified destination
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="o"></param>
        public static void PrintWithDest(object dest, object o) {
            PrintWithDestNoNewline(dest, o);
            Ops.Write(dest, "\n");
        }

        /// <summary>
        /// Prints newline into default standard output
        /// </summary>
        public static void PrintNewline() {
            SystemState state = SystemState.Instance;
            PrintNewlineWithDest(state.stdout);
        }

        /// <summary>
        /// Prints newline into specified destination. Sets softspace property to false.
        /// </summary>
        /// <param name="dest"></param>
        public static void PrintNewlineWithDest(object dest) {
            Ops.Write(dest, "\n");
            Ops.SetSoftspace(dest, Ops.False);
        }

        /// <summary>
        /// Prints value into default standard output with Python comma semantics.
        /// </summary>
        /// <param name="o"></param>
        public static void PrintComma(object o) {
            PrintCommaWithDest(SystemState.Instance.stdout, o);
        }

        /// <summary>
        /// Prints value into specified destination with Python comma semantics.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="o"></param>
        public static void PrintCommaWithDest(object dest, object o) {
            Ops.WriteSoftspace(dest);
            string s = o == null ? "None" : Ops.ToString(o);

            Ops.Write(dest, s);
            Ops.SetSoftspace(dest, !s.EndsWith("\n"));
        }

        public static void PrintWithDestNoNewline(object dest, object o) {
            Ops.WriteSoftspace(dest);
            Ops.Write(dest, o == null ? "None" : Ops.ToString(o));
        }

        /// <summary>
        /// Handles output of the expression statement.
        /// Prints the value and sets the __builtin__._
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        public static void PrintExpressionValue(CodeContext context, object value) {
            if (value != null) {
                Print(Ops.StringRepr(value));
                SystemState.Instance.BuiltinModuleInstance.SetCustomMember(context, Symbols.Underscore, value);
            }
        }

        #endregion

        #region Import support

        /// <summary>
        /// Called from generated code for:
        /// 
        /// import spam.eggs
        /// </summary>
        public static object ImportTop(CodeContext context, string fullName) {
            return SystemState.Instance.Importer.Import(context, fullName, null);
        }

        /// <summary>
        /// Python helper method called from generated code for:
        /// 
        /// import spam.eggs as ham
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static object ImportBottom(CodeContext context, string fullName) {
            object module = SystemState.Instance.Importer.Import(context, fullName, null);

            if (fullName.IndexOf('.') >= 0) {
                // Extract bottom from the imported module chain
                string[] parts = fullName.Split('.');

                for (int i = 1; i < parts.Length; i++) {
                    module = Ops.GetBoundAttr(context, module, SymbolTable.StringToId(parts[i]));
                }
            }
            return module;
        }

        /// <summary>
        /// Called from generated code for:
        /// 
        /// from spam import eggs1, eggs2 
        /// </summary>
        public static object ImportWithNames(CodeContext context, string fullName, string[] names) {
            return SystemState.Instance.Importer.Import(context, fullName, List.Make(names));
        }


        /// <summary>
        /// Imports one element from the module in the context of:
        /// 
        /// from module import a, b, c, d
        /// 
        /// Called repeatedly for all elements being imported (a, b, c, d above)
        /// </summary>
        public static object ImportFrom(CodeContext context, object module, string name) {
            return SystemState.Instance.Importer.ImportFrom(context, module, name);
        }

        /// <summary>
        /// Called from generated code for:
        /// 
        /// from spam import *
        /// </summary>
        public static void ImportStar(CodeContext context, string fullName) {
            object newmod = SystemState.Instance.Importer.Import(context, fullName, List.MakeList("*"));

            ScriptModule pnew = newmod as ScriptModule;
            if (pnew != null) {
                object all;
                if (pnew.TryGetBoundCustomMember(context, Symbols.All, out all)) {
                    IEnumerator exports = Ops.GetEnumerator(all);

                    while (exports.MoveNext()) {
                        string name = exports.Current as string;
                        if (name == null) continue;

                        SymbolId fieldId = SymbolTable.StringToId(name);
                        context.Scope.SetName(fieldId, Ops.GetBoundAttr(context, newmod, fieldId));
                    }
                    return;
                }
            }

            foreach (object o in Ops.GetAttrNames(context, newmod)) {
                if (o != null) {
                    if (!(o is string)) throw Ops.TypeErrorForNonStringAttribute();
                    string name = o as string;
                    if (name.Length == 0) continue;
                    if (name[0] == '_') continue;

                    SymbolId fieldId = SymbolTable.StringToId(name);

                    context.Scope.SetName(fieldId, Ops.GetBoundAttr(context, newmod, fieldId));
                }
            }
        }

        #endregion

        #region Exec

        /// <summary>
        /// Unqualified exec statement support.
        /// A Python helper which will be called for the statement:
        /// 
        /// exec code
        /// </summary>
        public static void UnqualifiedExec(CodeContext context, object code) {
            IAttributesCollection locals = null;
            IAttributesCollection globals = null;

            // if the user passes us a tuple we'll extract the 3 values out of it            
            Tuple codeTuple = code as Tuple;
            if (codeTuple != null && codeTuple.Count > 0 && codeTuple.Count <= 3) {
                code = codeTuple[0];

                if (codeTuple.Count > 1 && codeTuple[1] != null) {
                    globals = codeTuple[1] as IAttributesCollection;
                    if (globals == null) throw Ops.TypeError("globals must be dictionary or none");
                }

                if (codeTuple.Count > 2 && codeTuple[2] != null) {
                    locals = codeTuple[2] as IAttributesCollection;
                    if (locals == null) throw Ops.TypeError("locals must be dictionary or none");
                } else {
                    locals = globals;
                }
            }

            QualifiedExec(context, code, globals, locals);
        }

        /// <summary>
        /// Qualified exec statement support,
        /// Python helper which will be called for the statement:
        /// 
        /// exec code in globals [, locals ]
        /// </summary>
        public static void QualifiedExec(CodeContext context, object code, IAttributesCollection globals, object locals) {
            PythonFile pf;
            Stream cs;

            bool line_feed = true;

            // TODO: use SourceUnitReader when available
            if ((pf = code as PythonFile) != null) {
                List lines = pf.ReadLines();

                StringBuilder fullCode = new StringBuilder();
                for (int i = 0; i < lines.Count; i++) {
                    fullCode.Append(lines[i]);
                }

                code = fullCode.ToString();
            } else if ((cs = code as Stream) != null) {

                using (StreamReader reader = new StreamReader(cs)) { // TODO: encoding? 
                    code = reader.ReadToEnd();
                }

                line_feed = false;
            }

            string str_code = code as string;

            if (str_code != null) {
                ScriptEngine engine = SystemState.Instance.Engine;
                SourceCodeUnit code_unit = new SourceCodeUnit(engine, str_code);
                // in accordance to CPython semantics:
                code_unit.DisableLineFeedLineSeparator = line_feed;

                ScriptCode sc = PythonModuleOps.CompileFlowTrueDivision(code_unit, context.LanguageContext);
                
                code = new FunctionCode(sc);
            }

            FunctionCode fc = code as FunctionCode;
            if (fc == null) {
                throw Ops.TypeError("arg 1 must be a string, file, Stream, or code object");
            }

            if (locals == null) locals = globals;
            if (globals == null) globals = new GlobalsDictionary(context.Scope);

            if (locals != null && Ops.IsMappingType(context, locals) != Ops.True) {
                throw Ops.TypeError("exec: arg 3 must be mapping or None");
            }

            if (!globals.ContainsKey(Symbols.Builtins)) {
                globals[Symbols.Builtins] = SystemState.Instance.modules["__builtin__"];
            }

            IAttributesCollection attrLocals = Builtin.GetAttrLocals(context, locals);

            Microsoft.Scripting.Scope scope = new Microsoft.Scripting.Scope(new Microsoft.Scripting.Scope(globals), attrLocals);

            fc.Call(context, scope);
        }

        #endregion

        #region Binary Operators

        public static object In(object x, object y) {
            return Ops.In(x, y);
        }

        public static object NotIn(object x, object y) {
            return Ops.NotIn(x, y);
        }

        public static object IsNot(object x, object y) {
            return Ops.IsNot(x, y);
        }

        public static object Is(object x, object y) {
            return Ops.Is(x, y);
        }

        #endregion

        public static IEnumerator GetEnumeratorForIteration(object enumerable) {
            IEnumerator ie;
            if (!Converter.TryConvertToIEnumerator(enumerable, out ie)) {
                throw Ops.TypeError("iteration over non-sequence of type {0}",
                    Ops.StringRepr(Ops.GetDynamicType(enumerable)));
            }
            return ie;
        }
    }
}
