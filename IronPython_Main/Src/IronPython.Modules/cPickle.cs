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
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Hosting;
using Microsoft.Scripting.Actions;

[assembly: PythonModule("cPickle", typeof(IronPython.Modules.PythonPickle))]
namespace IronPython.Modules {
    [Documentation("Fast object serialization/unserialization.\n\n"
        + "Differences from CPython:\n"
        + " - does not implement the undocumented fast mode\n"
        )]
    public static class PythonPickle {
        private static int highestProtocol = 2;
        public static int HIGHEST_PROTOCOL {
            get { return highestProtocol; }
        }

        private const string Newline = "\n";

        public static OldClass PickleError = ExceptionConverter.CreatePythonException("PickleError", "cPickle");
        public static OldClass PicklingError = ExceptionConverter.CreatePythonException("PicklingError", "cPickle", PickleError);
        public static OldClass UnpicklingError = ExceptionConverter.CreatePythonException("UnpicklingError", "cPickle", PickleError);
        public static OldClass BadPickleGet = ExceptionConverter.CreatePythonException("BadPickleGet", "cPickle", UnpicklingError);

        #region Public module-level functions

        [Documentation("dump(obj, file, protocol=0) -> None\n\n"
            + "Pickle obj and write the result to file.\n"
            + "\n"
            + "See documentation for Pickler() for a description the file, protocol, and\n"
            + "(deprecated) bin parameters."
            )]
        public static void dump(CodeContext context, object obj, object file, [DefaultParameterValue(null)] object protocol, [DefaultParameterValue(null)] object bin) {
            Pickler pickler = new Pickler(file, protocol, bin);
            pickler.dump(context, obj);
        }

        [Documentation("dumps(obj, protocol=0) -> pickle string\n\n"
            + "Pickle obj and return the result as a string.\n"
            + "\n"
            + "See the documentation for Pickler() for a description of the protocol and\n"
            + "(deprecated) bin parameters."
            )]
        public static string dumps(CodeContext context, object obj, [DefaultParameterValue(null)] object protocol, [DefaultParameterValue(null)] object bin) {
            //??? possible perf enhancement: use a C# TextWriter-backed IFileOutput and
            // thus avoid Python call overhead. Also do similar thing for LoadFromString.
            object stringIO = PythonOps.InvokeWithContext(context, DynamicHelpers.GetPythonTypeFromType(typeof(PythonStringIO)), SymbolTable.StringToId("StringIO"));
            Pickler pickler = new Pickler(stringIO, protocol, bin);
            pickler.dump(context, obj);
            return Converter.ConvertToString(PythonOps.Invoke(stringIO, SymbolTable.StringToId("getvalue")));
        }

        [Documentation("load(file) -> unpickled object\n\n"
            + "Read pickle data from the open file object and return the corresponding\n"
            + "unpickled object. Data after the first pickle found is ignored, but the file\n"
            + "cursor is not reset, so if a file objects contains multiple pickles, then\n"
            + "load() may be called multiple times to unpickle them.\n"
            + "\n"
            + "file: an object (such as an open file or a StringIO) with read(num_chars) and\n"
            + "    readline() methods that return strings\n"
            + "\n"
            + "load() automatically determines if the pickle data was written in binary or\n"
            + "text mode."
            )]
        public static object load(CodeContext context, object file) {
            return new Unpickler(file).load(context);
        }

        [Documentation("loads(string) -> unpickled object\n\n"
            + "Read a pickle object from a string, unpickle it, and return the resulting\n"
            + "reconstructed object. Characters in the string beyond the end of the first\n"
            + "pickle are ignored."
            )]
        public static object loads(CodeContext context, object @string) {
            object stringIO = PythonOps.Invoke(
                DynamicHelpers.GetPythonTypeFromType(typeof(PythonStringIO)),
                SymbolTable.StringToId("StringIO"),
                @string
            );
            return new Unpickler(stringIO).load(context);
        }

        #endregion

        #region File I/O wrappers

        /// <summary>
        /// Interface for "file-like objects" that implement the protocol needed by load() and friends.
        /// This enables the creation of thin wrappers that make fast .NET types and slow Python types look the same.
        /// </summary>
        public interface IFileInput {
            string Read(int size);
            string ReadLine();
        }

        /// <summary>
        /// Interface for "file-like objects" that implement the protocol needed by dump() and friends.
        /// This enables the creation of thin wrappers that make fast .NET types and slow Python types look the same.
        /// </summary>
        public interface IFileOutput {
            void Write(string data);
        }

        private class PythonFileInput : IFileInput {
            private object _readMethod;
            private object _readLineMethod;

            public PythonFileInput(object file) {
                if (!PythonOps.TryGetBoundAttr(file, SymbolTable.StringToId("read"), out _readMethod) ||
                    !PythonOps.IsCallable(_readMethod) ||
                    !PythonOps.TryGetBoundAttr(file, SymbolTable.StringToId("readline"), out _readLineMethod) ||
                    !PythonOps.IsCallable(_readLineMethod)
                ) {
                    throw PythonOps.TypeError("argument must have callable 'read' and 'readline' attributes");
                }
            }

            public string Read(int size) {
                return Converter.ConvertToString(PythonCalls.Call(_readMethod, size));
            }

            public string ReadLine() {
                return Converter.ConvertToString(PythonCalls.Call(_readLineMethod));
            }
        }

        private class PythonFileOutput : IFileOutput {
            private object _writeMethod;

            public PythonFileOutput(object file) {
                if (!PythonOps.TryGetBoundAttr(file, SymbolTable.StringToId("write"), out _writeMethod) ||
                    !PythonOps.IsCallable(this._writeMethod)
                ) {
                    throw PythonOps.TypeError("argument must have callable 'write' attribute");
                }
            }

            public void Write(string data) {
                PythonCalls.Call(_writeMethod, data);
            }
        }

        private class PythonReadableFileOutput : PythonFileOutput {
            private object _getValueMethod;

            public PythonReadableFileOutput(object file) : base(file) {
                if (!PythonOps.TryGetBoundAttr(file, SymbolTable.StringToId("getvalue"), out _getValueMethod) ||
                    !PythonOps.IsCallable(_getValueMethod)
                ) {
                    throw PythonOps.TypeError("argument must have callable 'getvalue' attribute");
                }
            }

            public object GetValue() {
                return PythonCalls.Call(_getValueMethod);
            }
        }

        #endregion

        #region Opcode constants

        public static class Opcode {
            public const string Append = "a";
            public const string Appends = "e";
            public const string BinFloat = "G";
            public const string BinGet = "h";
            public const string BinInt = "J";
            public const string BinInt1 = "K";
            public const string BinInt2 = "M";
            public const string BinPersid = "Q";
            public const string BinPut = "q";
            public const string BinString = "T";
            public const string BinUnicode = "X";
            public const string Build = "b";
            public const string Dict = "d";
            public const string Dup = "2";
            public const string EmptyDict = "}";
            public const string EmptyList = "]";
            public const string EmptyTuple = ")";
            public const string Ext1 = "\x82";
            public const string Ext2 = "\x83";
            public const string Ext4 = "\x84";
            public const string Float = "F";
            public const string Get = "g";
            public const string Global = "c";
            public const string Inst = "i";
            public const string Int = "I";
            public const string List = "l";
            public const string Long = "L";
            public const string Long1 = "\x8a";
            public const string Long4 = "\x8b";
            public const string LongBinGet = "j";
            public const string LongBinPut = "r";
            public const string Mark = "(";
            public const string NewFalse = "\x89";
            public const string NewObj = "\x81";
            public const string NewTrue = "\x88";
            public const string NoneValue = "N";
            public const string Obj = "o";
            public const string PersId = "P";
            public const string Pop = "0";
            public const string PopMark = "1";
            public const string Proto = "\x80";
            public const string Put = "p";
            public const string Reduce = "R";
            public const string SetItem = "s";
            public const string SetItems = "u";
            public const string ShortBinstring = "U";
            public const string Stop = ".";
            public const string String = "S";
            public const string Tuple = "t";
            public const string Tuple1 = "\x85";
            public const string Tuple2 = "\x86";
            public const string Tuple3 = "\x87";
            public const string Unicode = "V";
        }

        #endregion

        #region Pickler object

        [Documentation("Pickler(file, protocol=0) -> Pickler object\n\n"
            + "A Pickler object serializes Python objects to a pickle bytecode stream, which\n"
            + "can then be converted back into equivalent objects using an Unpickler.\n"
            + "\n"
            + "file: an object (such as an open file) that has a write(string) method.\n"
            + "protocol: if omitted, protocol 0 is used. If HIGHEST_PROTOCOL or a negative\n"
            + "    number, the highest available protocol is used.\n"
            + "bin: (deprecated; use protocol instead) for backwards compability, a 'bin'\n"
            + "    keyword parameter is supported. When protocol is specified it is ignored.\n"
            + "    If protocol is not specified, then protocol 0 is used if bin is false, and\n"
            + "    protocol 1 is used if bin is true."
            )]
        [PythonSystemType]
        public class Pickler {

            private const char LowestPrintableChar = (char)32;
            private const char HighestPrintableChar = (char)126;
            // max elements that can be set/appended at a time using SETITEMS/APPENDS

            private delegate void PickleFunction(CodeContext context, object value);
            private readonly Dictionary<PythonType, PickleFunction> dispatchTable;

            private int _batchSize = 1000;
            private IFileOutput _file;
            private int _protocol;
            private IDictionary _memo;
            private object _persist_id;
            private DynamicSite<object, object, string> _persist_site;

            #region Public API

            public IDictionary memo {
                get { return _memo; }
                set { _memo = value; }
            }

            public int proto {
                get { return _protocol; }
                set { _protocol = value; }
            }

            public int _BATCHSIZE {
                get { return _batchSize; }
                set { _batchSize = value; }
            }

            public object persistent_id {
                get {
                    return _persist_id;
                }
                set {
                    _persist_id = value;
                }
            }

            public int binary {
                get { return _protocol == 0 ? 1 : 0; }
                set { _protocol = value; }
            }

            public int fast {
                // We don't implement fast, but we silently ignore it when it's set so that test_cpickle works.
                // For a description of fast, see http://mail.python.org/pipermail/python-bugs-list/2001-October/007695.html
                get { return 0; }
                set { /* ignore */ }
            }

            public static Pickler __new__(CodeContext context, 
                PythonType cls,
                [DefaultParameterValue(null)] object file,
                [DefaultParameterValue(null)] object protocol,
                [DefaultParameterValue(null)] object bin
            ) {
                if (cls == DynamicHelpers.GetPythonTypeFromType(typeof(Pickler))) {
                    // For undocumented (yet tested in official CPython tests) list-based pickler, the
                    // user could do something like Pickler(1), which would create a protocol-1 pickler
                    // with an internal string output buffer (retrievable using GetValue()). For a little
                    // more info, see
                    // https://sourceforge.net/tracker/?func=detail&atid=105470&aid=939395&group_id=5470
                    int intProtocol;
                    if (file == null) {
                        file = new PythonReadableFileOutput(new PythonStringIO.StringO());
                    } else if (Converter.TryConvertToInt32(file, out intProtocol)) {
                        return new Pickler((IFileOutput) new PythonReadableFileOutput(new PythonStringIO.StringO()), intProtocol, bin);
                    }
                    return new Pickler(file, protocol, bin);
                } else {
                    Pickler pickler = cls.CreateInstance(context, file, protocol, bin) as Pickler;
                    if (pickler == null) throw PythonOps.TypeError("{0} is not a subclass of Pickler", cls);
                    return pickler;
                }
            }

            public Pickler(object file, object protocol, object bin)
                : this(new PythonFileOutput(file), protocol, bin) { }

            public Pickler(IFileOutput file, object protocol, object bin) {
                dispatchTable = new Dictionary<PythonType, PickleFunction>();
                dispatchTable[TypeCache.Boolean] = SaveBoolean;
                dispatchTable[TypeCache.Int32] = SaveInteger;
                dispatchTable[TypeCache.None] = SaveNone;
                dispatchTable[TypeCache.Dict] = SaveDict;
                dispatchTable[TypeCache.BigInteger] = SaveLong;
                dispatchTable[TypeCache.Double] = SaveFloat;
                dispatchTable[TypeCache.String] = SaveUnicode;
                dispatchTable[TypeCache.PythonTuple] = SaveTuple;
                dispatchTable[TypeCache.List] = SaveList;
                dispatchTable[TypeCache.OldClass] = SaveGlobal;
                dispatchTable[TypeCache.Function] = SaveGlobal;
                dispatchTable[TypeCache.BuiltinFunction] = SaveGlobal;
                dispatchTable[TypeCache.PythonType] = SaveGlobal;
                dispatchTable[TypeCache.OldInstance] = SaveInstance;

                this._file = file;
                this._memo = new PythonDictionary();

                if (protocol == null) protocol = PythonOps.IsTrue(bin) ? 1 : 0;

                int intProtocol = Converter.ConvertToInt32(protocol);
                if (intProtocol > highestProtocol) {
                    throw PythonOps.ValueError("pickle protocol {0} asked for; the highest available protocol is {1}", intProtocol, highestProtocol);
                } else if (intProtocol < 0) {
                    this._protocol = highestProtocol;
                } else {
                    this._protocol = intProtocol;
                }
            }

            [Documentation("dump(obj) -> None\n\n"
                + "Pickle obj and write the result to the file object that was passed to the\n"
                + "constructor\n."
                + "\n"
                + "Note that you may call dump() multiple times to pickle multiple objects. To\n"
                + "unpickle the stream, you will need to call Unpickler's load() method a\n"
                + "corresponding number of times.\n"
                + "\n"
                + "The first time a particular object is encountered, it will be pickled normally.\n"
                + "If the object is encountered again (in the same or a later dump() call), a\n"
                + "reference to the previously generated value will be pickled. Unpickling will\n"
                + "then create multiple references to a single object."
                )]
            public void dump(CodeContext context, object obj) {
                if (_protocol >= 2) WriteProto();
                Save(context, obj);
                Write(Opcode.Stop);
            }

            [Documentation("clear_memo() -> None\n\n"
                + "Clear the memo, which is used internally by the pickler to keep track of which\n"
                + "objects have already been pickled (so that shared or recursive objects are\n"
                + "pickled only once)."
                )]
            public void clear_memo() {
                _memo.Clear();
            }

            [Documentation("getvalue() -> string\n\n"
                + "Return the value of the internal string. Raises PicklingError if a file object\n"
                + "was passed to this pickler's constructor."
                )]
            public object getvalue() {
                if (_file is PythonReadableFileOutput) {
                    return ((PythonReadableFileOutput)_file).GetValue();
                }
                throw ExceptionConverter.CreateThrowable(PicklingError, "Attempt to getvalue() a non-list-based pickler");
            }

            #endregion

            #region Save functions

            private void Save(CodeContext context, object obj) {
                if (_persist_id != null) {
                    if (_persist_site == null) {
                        _persist_site = RuntimeHelpers.CreateSimpleCallSite<object, object, string>();
                    }
                    string res = _persist_site.Invoke(context, _persist_id, obj);
                    if (res != null) {
                        SavePersId(context, res);
                        return;
                    }
                }

                if (_memo.Contains(PythonOps.Id(obj))) {
                    WriteGet(obj);
                } else {                    
                    PickleFunction pickleFunction;
                    PythonType objType = DynamicHelpers.GetPythonType(obj);
                    if (!dispatchTable.TryGetValue(objType.CanonicalPythonType, out pickleFunction)) {
                        if (objType.IsSubclassOf(TypeCache.PythonType)) {
                            // treat classes with metaclasses like regular classes
                            pickleFunction = SaveGlobal;
                        } else {
                            pickleFunction = SaveObject;
                        }
                    }
                    pickleFunction(context, obj);
                }
            }

            private void SavePersId(CodeContext context, string res) {
                if (this.binary != 0) {
                    Save(context, res);
                    Write(Opcode.BinPersid);
                } else {
                    Write(Opcode.PersId);
                    Write(res);
                    Write("\n");
                }
            }

            private void SaveBoolean(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.Boolean), "arg must be bool");
                if (_protocol < 2) {
                    Write(Opcode.Int);
                    Write(String.Format("0{0}", ((bool)obj) ? 1 : 0));
                    Write(Newline);
                } else {
                    if ((bool)obj) {
                        Write(Opcode.NewTrue);
                    } else {
                        Write(Opcode.NewFalse);
                    }
                }
            }

            private void SaveDict(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).CanonicalPythonType.Equals(TypeCache.Dict), "arg must be dict");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                Memoize(obj);

                if (_protocol < 1) {
                    Write(Opcode.Mark);
                    Write(Opcode.Dict);
                } else {
                    Write(Opcode.EmptyDict);
                }

                WritePut(obj);
                BatchSetItems(context, (DictionaryOps.iteritems((IDictionary<object, object>)obj)));
            }

            private void SaveFloat(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.Double), "arg must be float");

                if (_protocol < 1) {
                    Write(Opcode.Float);
                    WriteFloatAsString(obj);
                } else {
                    Write(Opcode.BinFloat);
                    WriteFloat64(obj);
                }
            }

            private void SaveGlobal(CodeContext context, object obj) {
                Debug.Assert(
                    DynamicHelpers.GetPythonType(obj).CanonicalPythonType.Equals(TypeCache.OldClass) ||
                    DynamicHelpers.GetPythonType(obj).CanonicalPythonType.Equals(TypeCache.Function) ||
                    DynamicHelpers.GetPythonType(obj).CanonicalPythonType.Equals(TypeCache.BuiltinFunction) ||
                    DynamicHelpers.GetPythonType(obj).CanonicalPythonType.Equals(TypeCache.PythonType) ||
                    DynamicHelpers.GetPythonType(obj).CanonicalPythonType.IsSubclassOf(TypeCache.PythonType),
                    "arg must be classic class, function, built-in function or method, or new-style type"
                );

                object name;
                if (PythonOps.TryGetBoundAttr(context, obj, Symbols.Name, out name)) {
                    SaveGlobalByName(context, obj, name);
                } else {
                    throw CannotPickle(obj, "could not determine its __name__");
                }
            }

            private void SaveGlobalByName(CodeContext context, object obj, object name) {
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));

                object moduleName = FindModuleForGlobal(context, obj, name);

                if (_protocol >= 2) {
                    object code;
                    if (PythonCopyReg.ExtensionRegistry.TryGetValue(PythonTuple.MakeTuple(moduleName, name), out code)) {
                        int intCode = (int)code;
                        if (IsUInt8(code)) {
                            Write(Opcode.Ext1);
                            WriteUInt8(code);
                        } else if (IsUInt16(code)) {
                            Write(Opcode.Ext2);
                            WriteUInt16(code);
                        } else if (IsInt32(code)) {
                            Write(Opcode.Ext4);
                            WriteInt32(code);
                        } else {
                            throw PythonOps.RuntimeError("unrecognized integer format");
                        }
                        return;
                    }
                }

                Memoize(obj);

                Write(Opcode.Global);
                WriteStringPair(moduleName, name);
                WritePut(obj);
            }

            private void SaveInstance(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.OldInstance), "arg must be old-class instance");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));

                Write(Opcode.Mark);

                // Memoize() call isn't in the usual spot to allow class to be memoized before
                // instance (when using proto other than 0) to match CPython's bytecode output

                object objClass;
                if (!PythonOps.TryGetBoundAttr(context, obj, Symbols.Class, out objClass)) {
                    throw CannotPickle(obj, "could not determine its __class__");
                }

                if (_protocol < 1) {
                    object className, classModuleName;
                    if (!PythonOps.TryGetBoundAttr(context, objClass, Symbols.Name, out className)) {
                        throw CannotPickle(obj, "its __class__ has no __name__");
                    }
                    classModuleName = FindModuleForGlobal(context, objClass, className);

                    Memoize(obj);
                    WriteInitArgs(context, obj);
                    Write(Opcode.Inst);
                    WriteStringPair(classModuleName, className);
                } else {
                    Save(context, objClass);
                    Memoize(obj);
                    WriteInitArgs(context, obj);
                    Write(Opcode.Obj);
                }

                WritePut(obj);

                object getStateCallable;
                if (PythonOps.TryGetBoundAttr(context, obj, Symbols.GetState, out getStateCallable)) {
                    Save(context, PythonCalls.Call(getStateCallable));
                } else {
                    Save(context, PythonOps.GetBoundAttr(context, obj, Symbols.Dict));
                }

                Write(Opcode.Build);
            }

            private void SaveInteger(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.Int32), "arg must be int");
                if (_protocol < 1) {
                    Write(Opcode.Int);
                    WriteIntAsString(obj);
                } else {
                    if (IsUInt8(obj)) {
                        Write(Opcode.BinInt1);
                        WriteUInt8(obj);
                    } else if (IsUInt16(obj)) {
                        Write(Opcode.BinInt2);
                        WriteUInt16(obj);
                    } else if (IsInt32(obj)) {
                        Write(Opcode.BinInt);
                        WriteInt32(obj);
                    } else {
                        throw PythonOps.RuntimeError("unrecognized integer format");
                    }
                }
            }

            private void SaveList(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.List), "arg must be list");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                Memoize(obj);
                if (_protocol < 1) {
                    Write(Opcode.Mark);
                    Write(Opcode.List);
                } else {
                    Write(Opcode.EmptyList);
                }

                WritePut(obj);
                BatchAppends(context, ((IEnumerable)obj).GetEnumerator());
            }

            private void SaveLong(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.BigInteger), "arg must be long");

                if (_protocol < 2) {
                    Write(Opcode.Long);
                    WriteLongAsString(obj);
                } else {
                    if (((BigInteger)obj).IsZero()) {
                        Write(Opcode.Long1);
                        WriteUInt8(0);
                    } else {
                        byte[] dataBytes = ((BigInteger)obj).ToByteArray();
                        if (dataBytes.Length < 256) {
                            Write(Opcode.Long1);
                            WriteUInt8(dataBytes.Length);
                        } else {
                            Write(Opcode.Long4);
                            WriteInt32(dataBytes.Length);
                        }

                        foreach (byte b in dataBytes) {
                            WriteUInt8(b);
                        }
                    }
                }
            }

            private void SaveNone(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.None), "arg must be None");
                Write(Opcode.NoneValue);
            }

            /// <summary>
            /// Call the appropriate reduce method for obj and pickle the object using
            /// the resulting data. Use the first available of
            /// copy_reg.dispatch_table[type(obj)], obj.__reduce_ex__, and obj.__reduce__.
            /// </summary>
            private void SaveObject(CodeContext context, object obj) {
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                Memoize(obj);

                object reduceCallable, result;
                PythonType objType = DynamicHelpers.GetPythonType(obj);

                if (PythonCopyReg.DispatchTable.TryGetValue(objType, out reduceCallable)) {
                    result = PythonCalls.Call(reduceCallable, obj);
                } else if (PythonOps.TryGetBoundAttr(context, obj, Symbols.ReduceExtended, out reduceCallable)) {
                    if (obj is PythonType) {
                        result = PythonOps.CallWithContext(context, reduceCallable, obj, _protocol);
                    } else {
                        result = PythonOps.CallWithContext(context, reduceCallable, _protocol);
                    }
                } else if (PythonOps.TryGetBoundAttr(context, obj, Symbols.Reduce, out reduceCallable)) {
                    if (obj is PythonType) {
                        result = PythonOps.CallWithContext(context, reduceCallable, obj);
                    } else {
                        result = PythonOps.CallWithContext(context, reduceCallable);
                    }
                } else {
                    throw PythonOps.AttributeError("no reduce function found for {0}", obj);
                }

                if (objType.Equals(TypeCache.String)) {
                    if (_memo.Contains(PythonOps.Id(obj))) {
                        WriteGet(obj);
                    } else {
                        SaveGlobalByName(context, obj, result);
                    }
                } else if (result is PythonTuple) {
                    PythonTuple rt = (PythonTuple)result;
                    switch (rt.Count) {
                        case 2:
                            SaveReduce(context, obj, reduceCallable, rt[0], rt[1], null, null, null);
                            break;
                        case 3:
                            SaveReduce(context, obj, reduceCallable, rt[0], rt[1], rt[2], null, null);
                            break;
                        case 4:
                            SaveReduce(context, obj, reduceCallable, rt[0], rt[1], rt[2], rt[3], null);
                            break;
                        case 5:
                            SaveReduce(context, obj, reduceCallable, rt[0], rt[1], rt[2], rt[3], rt[4]);
                            break;
                        default:
                            throw CannotPickle(obj, "tuple returned by {0} must have to to five elements", reduceCallable);
                    }
                } else {
                    throw CannotPickle(obj, "{0} must return string or tuple", reduceCallable);
                }
            }

            /// <summary>
            /// Pickle the result of a reduce function.
            /// 
            /// Only context, obj, func, and reduceCallable are required; all other arguments may be null.
            /// </summary>
            private void SaveReduce(CodeContext context, object obj, object reduceCallable, object func, object args, object state, object listItems, object dictItems) {
                if (!PythonOps.IsCallable(func)) {
                    throw CannotPickle(obj, "func from reduce() should be callable");
                } else if (!(args is PythonTuple) && args != null) {
                    throw CannotPickle(obj, "args from reduce() should be a tuple");
                } else if (listItems != null && !(listItems is IEnumerator)) {
                    throw CannotPickle(obj, "listitems from reduce() should be a list iterator");
                } else if (dictItems != null && !(dictItems is IEnumerator)) {
                    throw CannotPickle(obj, "dictitems from reduce() should be a dict iterator");
                }

                object funcName;
                string funcNameString;
                if (!PythonOps.TryGetBoundAttr(context, func, Symbols.Name, out funcName)) {
                    throw CannotPickle(obj, "func from reduce() ({0}) should have a __name__ attribute");
                } else if (!Converter.TryConvertToString(funcName, out funcNameString) || funcNameString == null) {
                    throw CannotPickle(obj, "__name__ of func from reduce() must be string");
                }

                if (_protocol >= 2 && "__newobj__" == funcNameString) {
                    if (args == null) {
                        throw CannotPickle(obj, "__newobj__ arglist is None");
                    }
                    PythonTuple argsTuple = (PythonTuple)args;
                    if (argsTuple.Count == 0) {
                        throw CannotPickle(obj, "__newobj__ arglist is empty");
                    } else if (!DynamicHelpers.GetPythonType(obj).Equals(argsTuple[0])) {
                        throw CannotPickle(obj, "args[0] from __newobj__ args has the wrong class");
                    }
                    Save(context, argsTuple[0]);
                    Save(context, argsTuple[new Slice(1, null)]);
                    Write(Opcode.NewObj);
                } else {
                    Save(context, func);
                    Save(context, args);
                    Write(Opcode.Reduce);
                }

                WritePut(obj);

                if (state != null) {
                    Save(context, state);
                    Write(Opcode.Build);
                }

                if (listItems != null) {
                    BatchAppends(context, (IEnumerator)listItems);
                }

                if (dictItems != null) {
                    BatchSetItems(context, (IEnumerator)dictItems);
                }
            }

            private void SaveTuple(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.PythonTuple), "arg must be tuple");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                PythonTuple t = (PythonTuple)obj;
                string opcode;
                bool needMark = false;
                if (_protocol > 0 && t.Count == 0) {
                    opcode = Opcode.EmptyTuple;
                } else if (_protocol >= 2 && t.Count == 1) {
                    opcode = Opcode.Tuple1;
                } else if (_protocol >= 2 && t.Count == 2) {
                    opcode = Opcode.Tuple2;
                } else if (_protocol >= 2 && t.Count == 3) {
                    opcode = Opcode.Tuple3;
                } else {
                    opcode = Opcode.Tuple;
                    needMark = true;
                }

                if (needMark) Write(Opcode.Mark);
                foreach (object o in t) Save(context, o);
                Write(opcode);

                if (t.Count > 0) {
                    Memoize(t);
                    WritePut(t);
                }
            }

            private void SaveUnicode(CodeContext context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.String), "arg must be unicode");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                Memoize(obj);
                if (_protocol < 1) {
                    Write(Opcode.Unicode);
                    WriteUnicodeStringRaw(obj);
                } else {
                    Write(Opcode.BinUnicode);
                    WriteUnicodeStringUtf8(obj);
                }

                WritePut(obj);
            }

            #endregion

            #region Output encoding

            /// <summary>
            /// Write value in pickle decimalnl_short format.
            /// </summary>
            private void WriteFloatAsString(object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.Double));
                // 17 digits of precision are necessary for accurate roundtripping
                StringFormatter sf = new StringFormatter("%.17g", value);
                sf._TrailingZeroAfterWholeFloat = true;
                Write(sf.Format());
                Write(Newline);
            }

            /// <summary>
            /// Write value in pickle float8 format.
            /// </summary>
            private void WriteFloat64(object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.Double));
                Write(PythonStruct.Pack(">d", value));
            }

            /// <summary>
            /// Write value in pickle uint1 format.
            /// </summary>
            private void WriteUInt8(object value) {
                Debug.Assert(IsUInt8(value));
                Write(PythonStruct.Pack("B", value));
            }

            /// <summary>
            /// Write value in pickle uint2 format.
            /// </summary>
            private void WriteUInt16(object value) {
                Debug.Assert(IsUInt16(value));
                Write(PythonStruct.Pack("<H", value));
            }

            /// <summary>
            /// Write value in pickle int4 format.
            /// </summary>
            private void WriteInt32(object value) {
                Debug.Assert(IsInt32(value));
                Write(PythonStruct.Pack("<i", value));
            }

            /// <summary>
            /// Write value in pickle decimalnl_short format.
            /// </summary>
            private void WriteIntAsString(object value) {
                Debug.Assert(IsInt32(value));
                Write(PythonOps.StringRepr(value));
                Write(Newline);
            }

            /// <summary>
            /// Write value in pickle decimalnl_long format.
            /// </summary>
            private void WriteLongAsString(object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.BigInteger));
                Write(PythonOps.StringRepr(value));
                Write(Newline);
            }

            /// <summary>
            /// Write value in pickle unicodestringnl format.
            /// </summary>
            private void WriteUnicodeStringRaw(object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.String));
                // manually escape backslash and newline
                Write(StringOps.RawUnicodeEscapeEncode(((string)value).Replace("\\", "\\u005c").Replace("\n", "\\u000a")));
                Write(Newline);
            }

            /// <summary>
            /// Write value in pickle unicodestring4 format.
            /// </summary>
            private void WriteUnicodeStringUtf8(object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.String));
                string encodedString = StringOps.FromByteArray(System.Text.Encoding.UTF8.GetBytes((string)value));
                WriteInt32(encodedString.Length);
                Write(encodedString);
            }

            /// <summary>
            /// Write value in pickle stringnl_noescape_pair format.
            /// </summary>
            private void WriteStringPair(object value1, object value2) {
                Debug.Assert(DynamicHelpers.GetPythonType(value1).Equals(TypeCache.String));
                Debug.Assert(DynamicHelpers.GetPythonType(value2).Equals(TypeCache.String));
                Debug.Assert(IsPrintableAscii(value1));
                Debug.Assert(IsPrintableAscii(value2));
                Write((string)value1);
                Write(Newline);
                Write((string)value2);
                Write(Newline);
            }

            #endregion

            #region Type checking

            /// <summary>
            /// Return true if value is appropriate for formatting in pickle uint1 format.
            /// </summary>
            private bool IsUInt8(object value) {
                return PythonSites.LessThanOrEqualRetBool(0, value) && PythonSites.LessThanRetBool(value, 1 << 8);
            }

            /// <summary>
            /// Return true if value is appropriate for formatting in pickle uint2 format.
            /// </summary>
            private bool IsUInt16(object value) {
                return PythonSites.LessThanOrEqualRetBool(1 << 8, value) && PythonSites.LessThanRetBool(value, 1 << 16);
            }

            /// <summary>
            /// Return true if value is appropriate for formatting in pickle int4 format.
            /// </summary>
            private bool IsInt32(object value) {
                return PythonSites.LessThanOrEqualRetBool(Int32.MinValue, value) && PythonSites.LessThanOrEqualRetBool(value, Int32.MaxValue);
            }

            /// <summary>
            /// Return true if value is a string where each value is in the range of printable ASCII characters.
            /// </summary>
            private bool IsPrintableAscii(object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.String));
                string strValue = (string)value;
                foreach (char c in strValue) {
                    if (!(LowestPrintableChar <= c && c <= HighestPrintableChar)) return false;
                }
                return true;
            }

            #endregion

            #region Output generation helpers

            private void Write(string data) {
                _file.Write(data);
            }

            private void WriteGet(object obj) {
                Debug.Assert(_memo.Contains(PythonOps.Id(obj)));
                // Memo entries are tuples, and the first element is the memo index
                IList<object> memoEntry = (IList<object>)_memo[PythonOps.Id(obj)];

                object index = memoEntry[0];
                Debug.Assert(PythonSites.GreaterThanOrEqualRetBool(index, 0));
                if (_protocol < 1) {
                    Write(Opcode.Get);
                    WriteIntAsString(index);
                } else {
                    if (IsUInt8(index)) {
                        Write(Opcode.BinGet);
                        WriteUInt8(index);
                    } else {
                        Write(Opcode.LongBinGet);
                        WriteInt32(index);
                    }
                }
            }

            private void WriteInitArgs(CodeContext context, object obj) {
                object getInitArgsCallable;
                if (PythonOps.TryGetBoundAttr(context, obj, Symbols.GetInitArgs, out getInitArgsCallable)) {
                    object initArgs = PythonCalls.Call(getInitArgsCallable);
                    if (!(initArgs is PythonTuple)) {
                        throw CannotPickle(obj, "__getinitargs__() must return tuple");
                    }
                    foreach (object arg in (PythonTuple)initArgs) {
                        Save(context, arg);
                    }
                }
            }

            private void WritePut(object obj) {
                Debug.Assert(_memo.Contains(PythonOps.Id(obj)));
                // Memo entries are tuples, and the first element is the memo index
                IList<object> memoEntry = (IList<object>)_memo[PythonOps.Id(obj)];

                object index = memoEntry[0];
                Debug.Assert(PythonSites.GreaterThanOrEqualRetBool(index, 0));
                if (_protocol < 1) {
                    Write(Opcode.Put);
                    WriteIntAsString(index);
                } else {
                    if (IsUInt8(index)) {
                        Write(Opcode.BinPut);
                        WriteUInt8(index);
                    } else {
                        Write(Opcode.LongBinPut);
                        WriteInt32(index);
                    }
                }
            }

            private void WriteProto() {
                Write(Opcode.Proto);
                WriteUInt8(_protocol);
            }

            /// <summary>
            /// Emit a series of opcodes that will set append all items indexed by iter
            /// to the object at the top of the stack. Use APPENDS if possible, but
            /// append no more than BatchSize items at a time.
            /// </summary>
            private void BatchAppends(CodeContext context, IEnumerator enumerator) {
                if (_protocol < 1) {
                    while (enumerator.MoveNext()) {
                        Save(context, enumerator.Current);
                        Write(Opcode.Append);
                    }
                } else {
                    object next;
                    if (enumerator.MoveNext()) {
                        next = enumerator.Current;
                    } else {
                        return;
                    }

                    int batchCompleted = 0;
                    object current;

                    // We do a one-item lookahead to avoid emitting an APPENDS for a
                    // single remaining item.
                    while (enumerator.MoveNext()) {
                        current = next;
                        next = enumerator.Current;

                        if (batchCompleted == _BATCHSIZE) {
                            Write(Opcode.Appends);
                            batchCompleted = 0;
                        }

                        if (batchCompleted == 0) {
                            Write(Opcode.Mark);
                        }

                        Save(context, current);
                        batchCompleted++;
                    }

                    if (batchCompleted == _BATCHSIZE) {
                        Write(Opcode.Appends);
                        batchCompleted = 0;
                    }
                    Save(context, next);
                    batchCompleted++;

                    if (batchCompleted > 1) {
                        Write(Opcode.Appends);
                    } else {
                        Write(Opcode.Append);
                    }
                }
            }

            /// <summary>
            /// Emit a series of opcodes that will set all (key, value) pairs indexed by
            /// iter in the object at the top of the stack. Use SETITEMS if possible,
            /// but append no more than BatchSize items at a time.
            /// </summary>
            private void BatchSetItems(CodeContext context, IEnumerator enumerator) {
                PythonTuple kvTuple;
                if (_protocol < 1) {
                    while (enumerator.MoveNext()) {
                        kvTuple = (PythonTuple)enumerator.Current;
                        Save(context, kvTuple[0]);
                        Save(context, kvTuple[1]);
                        Write(Opcode.SetItem);
                    }
                } else {
                    object nextKey, nextValue;
                    if (enumerator.MoveNext()) {
                        kvTuple = (PythonTuple)enumerator.Current;
                        nextKey = kvTuple[0];
                        nextValue = kvTuple[1];
                    } else {
                        return;
                    }

                    int batchCompleted = 0;
                    object curKey, curValue;

                    // We do a one-item lookahead to avoid emitting a SETITEMS for a
                    // single remaining item.
                    while (enumerator.MoveNext()) {
                        curKey = nextKey;
                        curValue = nextValue;
                        kvTuple = (PythonTuple)enumerator.Current;
                        nextKey = kvTuple[0];
                        nextValue = kvTuple[1];

                        if (batchCompleted == _BATCHSIZE) {
                            Write(Opcode.SetItems);
                            batchCompleted = 0;
                        }

                        if (batchCompleted == 0) {
                            Write(Opcode.Mark);
                        }

                        Save(context, curKey);
                        Save(context, curValue);
                        batchCompleted++;
                    }

                    if (batchCompleted == _BATCHSIZE) {
                        Write(Opcode.SetItems);
                        batchCompleted = 0;
                    }
                    Save(context, nextKey);
                    Save(context, nextValue);
                    batchCompleted++;

                    if (batchCompleted > 1) {
                        Write(Opcode.SetItems);
                    } else {
                        Write(Opcode.SetItem);
                    }
                }
            }

            #endregion

            #region Other private helper methods

            private Exception CannotPickle(object obj, string format, params object[] args) {
                StringBuilder msgBuilder = new StringBuilder();
                msgBuilder.Append("Can't pickle ");
                msgBuilder.Append(obj);
                if (format != null) {
                    msgBuilder.Append(": ");
                    msgBuilder.Append(String.Format(format, args));
                }
                return ExceptionConverter.CreateThrowable(PicklingError, msgBuilder.ToString());
            }

            private void Memoize(object obj) {
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                _memo[PythonOps.Id(obj)] = PythonTuple.MakeTuple(_memo.Count, obj);
            }

            /// <summary>
            /// Find the module for obj and ensure that obj is reachable in that module by the given name.
            /// 
            /// Throw PicklingError if any of the following are true:
            ///  - The module couldn't be determined.
            ///  - The module couldn't be loaded.
            ///  - The given name doesn't exist in the module.
            ///  - The given name is a different object than obj.
            /// 
            /// Otherwise, return the name of the module.
            /// 
            /// To determine which module obj lives in, obj.__module__ is used if available. The
            /// module named by obj.__module__ is loaded if needed. If obj has no __module__
            /// attribute, then each loaded module is searched. If a loaded module has an
            /// attribute with the given name, and that attribute is the same object as obj,
            /// then that module is used.
            /// </summary>
            private object FindModuleForGlobal(CodeContext context, object obj, object name) {
                object module;
                object moduleName;
                if (PythonOps.TryGetBoundAttr(context, obj, Symbols.Module, out moduleName)) {
                    // TODO: Global SystemState
                    if (!PythonEngine.CurrentEngine.Importer.TryGetExistingModule(Converter.ConvertToString(moduleName), out module)) {
                        module = Builtin.__import__(context, Converter.ConvertToString(moduleName));
                    }

                    object foundObj;
                    if (PythonOps.TryGetBoundAttr(context, module, SymbolTable.StringToId(Converter.ConvertToString(name)), out foundObj)) {
                        if (PythonOps.IsRetBool(foundObj, obj)) {
                            return moduleName;
                        } else {
                            throw CannotPickle(obj, "it's not the same object as {0}.{1}", moduleName, name);
                        }
                    } else {
                        throw CannotPickle(obj, "it's not found as {0}.{1}", moduleName, name);
                    }
                } else {
                    // No obj.__module__, so crawl through all loaded modules looking for obj
                    foreach (KeyValuePair<object, object> modulePair in SystemState.Instance.modules) {
                        moduleName = modulePair.Key;
                        module = modulePair.Value;
                        object foundObj;
                        if (PythonOps.TryGetBoundAttr(context, module, SymbolTable.StringToId(Converter.ConvertToString(name)), out foundObj) &&
                            PythonOps.IsRetBool(foundObj, obj)
                        ) {
                            return moduleName;
                        }
                    }
                    throw CannotPickle(obj, "could not determine its module");
                }

            }

            #endregion

        }

        #endregion

        #region Unpickler object

        [Documentation("Unpickler(file) -> Unpickler object\n\n"
            + "An Unpickler object reads a pickle bytecode stream and creates corresponding\n"
            + "objects."
            + "\n"
            + "file: an object (such as an open file or a StringIO) with read(num_chars) and\n"
            + "    readline() methods that return strings"
            )]
        [PythonSystemType]
        public class Unpickler {

            private readonly object _mark = new object();

            private delegate void LoadFunction(CodeContext context);
            private readonly Dictionary<string, LoadFunction> _dispatch;

            private IFileInput _file;
            private List _stack;
            private IDictionary<object, object> _memo;
            private object _pers_loader;
            private DynamicSite<object, object, object> _pers_site;

            public Unpickler(object file)
                : this(new PythonFileInput(file)) { }

            public Unpickler(IFileInput file) {
                this._file = file;
                _memo = new PythonDictionary();

                _dispatch = new Dictionary<string, LoadFunction>();
                _dispatch[""] = LoadEof;
                _dispatch[Opcode.Append] = LoadAppend;
                _dispatch[Opcode.Appends] = LoadAppends;
                _dispatch[Opcode.BinFloat] = LoadBinFloat;
                _dispatch[Opcode.BinGet] = LoadBinGet;
                _dispatch[Opcode.BinInt] = LoadBinInt;
                _dispatch[Opcode.BinInt1] = LoadBinInt1;
                _dispatch[Opcode.BinInt2] = LoadBinInt2;
                _dispatch[Opcode.BinPersid] = LoadBinPersid;
                _dispatch[Opcode.BinPut] = LoadBinPut;
                _dispatch[Opcode.BinString] = LoadBinString;
                _dispatch[Opcode.BinUnicode] = LoadBinUnicode;
                _dispatch[Opcode.Build] = LoadBuild;
                _dispatch[Opcode.Dict] = LoadDict;
                _dispatch[Opcode.Dup] = LoadDup;
                _dispatch[Opcode.EmptyDict] = LoadEmptyDict;
                _dispatch[Opcode.EmptyList] = LoadEmptyList;
                _dispatch[Opcode.EmptyTuple] = LoadEmptyTuple;
                _dispatch[Opcode.Ext1] = LoadExt1;
                _dispatch[Opcode.Ext2] = LoadExt2;
                _dispatch[Opcode.Ext4] = LoadExt4;
                _dispatch[Opcode.Float] = LoadFloat;
                _dispatch[Opcode.Get] = LoadGet;
                _dispatch[Opcode.Global] = LoadGlobal;
                _dispatch[Opcode.Inst] = LoadInst;
                _dispatch[Opcode.Int] = LoadInt;
                _dispatch[Opcode.List] = LoadList;
                _dispatch[Opcode.Long] = LoadLong;
                _dispatch[Opcode.Long1] = LoadLong1;
                _dispatch[Opcode.Long4] = LoadLong4;
                _dispatch[Opcode.LongBinGet] = LoadLongBinGet;
                _dispatch[Opcode.LongBinPut] = LoadLongBinPut;
                _dispatch[Opcode.Mark] = LoadMark;
                _dispatch[Opcode.NewFalse] = LoadNewFalse;
                _dispatch[Opcode.NewObj] = LoadNewObj;
                _dispatch[Opcode.NewTrue] = LoadNewTrue;
                _dispatch[Opcode.NoneValue] = LoadNoneValue;
                _dispatch[Opcode.Obj] = LoadObj;
                _dispatch[Opcode.PersId] = LoadPersId;
                _dispatch[Opcode.Pop] = LoadPop;
                _dispatch[Opcode.PopMark] = LoadPopMark;
                _dispatch[Opcode.Proto] = LoadProto;
                _dispatch[Opcode.Put] = LoadPut;
                _dispatch[Opcode.Reduce] = LoadReduce;
                _dispatch[Opcode.SetItem] = LoadSetItem;
                _dispatch[Opcode.SetItems] = LoadSetItems;
                _dispatch[Opcode.ShortBinstring] = LoadShortBinstring;
                _dispatch[Opcode.String] = LoadString;
                _dispatch[Opcode.Tuple] = LoadTuple;
                _dispatch[Opcode.Tuple1] = LoadTuple1;
                _dispatch[Opcode.Tuple2] = LoadTuple2;
                _dispatch[Opcode.Tuple3] = LoadTuple3;
                _dispatch[Opcode.Unicode] = LoadUnicode;
            }

            [Documentation("load() -> unpickled object\n\n"
                + "Read pickle data from the file object that was passed to the constructor and\n"
                + "return the corresponding unpickled objects."
               )]
            public object load(CodeContext context) {
                _stack = new List();

                string opcode = Read(1);

                while (opcode != Opcode.Stop) {
                    if (!_dispatch.ContainsKey(opcode)) {
                        throw CannotUnpickle("invalid opcode: {0}", PythonOps.StringRepr(opcode));
                    }
                    _dispatch[opcode](context);
                    opcode = Read(1);
                }

                return _stack.Pop();
            }

            [Documentation("noload() -> unpickled object\n\n"
                // 1234567890123456789012345678901234567890123456789012345678901234567890123456789
                + "Like load(), but don't import any modules or create create any instances of\n"
                + "user-defined types. (Builtin objects such as ints, tuples, etc. are created as\n"
                + "with load().)\n"
                + "\n"
                + "This is primarily useful for scanning a pickle for persistent ids without\n"
                + "incurring the overhead of completely unpickling an object. See the pickle\n"
                + "module documentation for more information about persistent ids."
               )]
            public void noload(CodeContext context) {
                throw PythonOps.NotImplementedError("noload() is not implemented");
            }

            private Exception CannotUnpickle(string format, params object[] args) {
                return ExceptionConverter.CreateThrowable(PicklingError, String.Format(format, args));
            }

            public IDictionary<object, object> memo {
                get { return _memo; }
                set { _memo = value; }
            }

            public object persistent_load {
                get {
                    return _pers_loader;
                }
                set {
                    _pers_loader = value;
                }
            }

            private object MemoGet(long key) {
                object value;
                if (_memo.TryGetValue(key, out value)) return value;
                throw ExceptionConverter.CreateThrowable(BadPickleGet, String.Format("memo key {0} not found", key));
            }

            private void MemoPut(long key, object value) {
                _memo[key] = value;
            }

            public int MarkIndex {
                get {
                    int i = _stack.Count - 1;
                    while (i > 0 && _stack[i] != _mark) i -= 1;
                    if (i == -1) throw CannotUnpickle("mark not found");
                    return i;
                }
            }

            private string Read(int size) {
                return _file.Read(size);
            }

            private string ReadLine() {
                return _file.ReadLine();
            }

            private string ReadLineNoNewline() {
                string raw = _file.ReadLine();
                return raw.Substring(0, raw.Length - 1);
            }

            private object ReadFloatString(CodeContext context) {
                return DoubleOps.Make(context, TypeCache.Double, ReadLineNoNewline());
            }

            private double ReadFloat64() {
                int index = 0;
                return PythonStruct.CreateDoubleValue(ref index, false, Read(8));
            }

            private object ReadIntFromString(CodeContext context) {
                string raw = ReadLineNoNewline();
                if ("00" == raw) return RuntimeHelpers.False;
                else if ("01" == raw) return RuntimeHelpers.True;
                return Int32Ops.Make(context, TypeCache.Int32, raw);
            }

            private int ReadInt32() {
                int index = 0;
                return PythonStruct.CreateIntValue(ref index, true, Read(4));
            }

            private object ReadLongFromString(CodeContext context) {
                return BigIntegerOps.Make(context, TypeCache.BigInteger, ReadLineNoNewline());
            }

            private object ReadLong(int size) {
                return BigInteger.Create(StringOps.ToByteArray(Read(size)));
            }

            private char ReadUInt8() {
                int index = 0;
                return PythonStruct.CreateCharValue(ref index, Read(1));
            }

            private ushort ReadUInt16() {
                int index = 0;
                return PythonStruct.CreateUShortValue(ref index, true, Read(2));
            }

            public object find_global(CodeContext context, object module, object attr) {
                object moduleObject;
                if (!PythonEngine.CurrentEngine.Importer.TryGetExistingModule(Converter.ConvertToString(module), out moduleObject)) {
                    moduleObject = Builtin.__import__(context, Converter.ConvertToString(module));
                }
                return PythonOps.GetBoundAttr(context, moduleObject, SymbolTable.StringToId(Converter.ConvertToString(attr)));
            }

            private object MakeInstance(CodeContext context, object cls, object[] args) {
                OldClass oc = cls as OldClass;
                if (oc != null) {
                    OldInstance inst = new OldInstance(oc);
                    if (args.Length != 0 || PythonOps.HasAttr(context, cls, Symbols.GetInitArgs)) {
                        PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, inst, Symbols.Init), args);
                    }
                    return inst;
                }
                return PythonOps.CallWithContext(context, cls, args);
            }

            private void PopMark(int markIndex) {
                _stack.DeleteSlice(markIndex, _stack.Count);
            }

            /// <summary>
            /// Interpret everything from markIndex to the top of the stack as a sequence
            /// of key, value, key, value, etc. Set dict[key] = value for each. Pop
            /// everything from markIndex up when done.
            /// </summary>
            private void SetItems(PythonDictionary dict, int markIndex) {
                for (int i = markIndex + 1; i < _stack.Count; i += 2) {
                    dict[_stack[i]] = _stack[i+1];
                }
                PopMark(markIndex);
            }

            private void LoadEof(CodeContext context) {
                throw PythonOps.EofError("unexpected end of opcode stream");
            }

            private void LoadAppend(CodeContext context) {
                object item = _stack.Pop();
                object seq = _stack[-1];
                if (seq is List) {
                    ((List)seq).Append(item);
                } else {
                    PythonCalls.Call(PythonOps.GetBoundAttr(context, seq, Symbols.Append), item);
                }
            }

            private void LoadAppends(CodeContext context) {
                int markIndex = MarkIndex;
                object seq = _stack[markIndex - 1];
                object stackSlice = _stack.GetSlice(markIndex + 1, _stack.Count);
                if (seq is List) {
                    ((List)seq).Extend(stackSlice);
                } else {
                    PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, seq, Symbols.Extend), stackSlice);
                }
                PopMark(markIndex);
            }

            private void LoadBinFloat(CodeContext context) {
                _stack.Append(ReadFloat64());
            }

            private void LoadBinGet(CodeContext context) {
                _stack.Append(MemoGet((long)ReadUInt8()));
            }

            private void LoadBinInt(CodeContext context) {
                _stack.Append(ReadInt32());
            }

            private void LoadBinInt1(CodeContext context) {
                _stack.Append((int)ReadUInt8());
            }

            private void LoadBinInt2(CodeContext context) {
                _stack.Append((int)ReadUInt16());
            }

            private void LoadBinPersid(CodeContext context) {
                if (_pers_loader == null) throw CannotUnpickle("cannot unpickle binary persistent ID w/o persistent_load");

                if (_pers_site == null) {
                    _pers_site = RuntimeHelpers.CreateSimpleCallSite<object, object, object>();
                }
                _stack.Append(_pers_site.Invoke(context, _pers_loader, _stack.Pop()));
            }

            private void LoadBinPut(CodeContext context) {
                MemoPut((long)ReadUInt8(), _stack[-1]);
            }

            private void LoadBinString(CodeContext context) {
                _stack.Append(Read(ReadInt32()));
            }

            private void LoadBinUnicode(CodeContext context) {
#if !SILVERLIGHT    // Encoding
                _stack.Append(StringOps.Decode(context, Read(ReadInt32()), "utf-8", "strict"));
#else
                throw new NotImplementedException("SILVERLIGHT - not supported - encoding");
#endif
            }

            private void LoadBuild(CodeContext context) {
                object arg = _stack.Pop();
                object inst = _stack[-1];
                object setStateCallable;
                if (PythonOps.TryGetBoundAttr(context, inst, Symbols.SetState, out setStateCallable)) {
                    PythonOps.CallWithContext(context, setStateCallable, arg);
                    return;
                }

                PythonDictionary dict;
                PythonDictionary slots;
                if (arg == null) {
                    dict = null;
                    slots = null;
                } else if (arg is PythonDictionary) {
                    dict = (PythonDictionary)arg;
                    slots = null;
                } else if (arg is PythonTuple) {
                    PythonTuple argsTuple = (PythonTuple)arg;
                    if (argsTuple.Count != 2) {
                        throw PythonOps.ValueError("state for object without __setstate__ must be None, dict, or 2-tuple");
                    }
                    dict = (PythonDictionary)argsTuple[0];
                    slots = (PythonDictionary)argsTuple[1];
                } else {
                    throw PythonOps.ValueError("state for object without __setstate__ must be None, dict, or 2-tuple");
                }

                if (dict != null) {
                    object instDict;
                    if (PythonOps.TryGetBoundAttr(context, inst, Symbols.Dict, out instDict)) {
                        PythonDictionary realDict = instDict as PythonDictionary;
                        if (realDict != null) {
                            realDict.Update(arg);
                        } else {
                            object updateCallable;
                            if (PythonOps.TryGetBoundAttr(context, instDict, Symbols.Update, out updateCallable)) {
                                PythonOps.CallWithContext(context, updateCallable, dict);
                            } else {
                                throw CannotUnpickle("could not update __dict__ {0} when building {1}", dict, inst);
                            }
                        }
                    }
                }

                if (slots != null) {
                    foreach(object key in slots) {
                        PythonOps.SetAttr(context, inst, SymbolTable.StringToId((string)key), slots[key]);
                    }
                }
            }

            private void LoadDict(CodeContext context) {
                int markIndex = MarkIndex;
                PythonDictionary dict = new PythonDictionary((_stack.Count - 1 - markIndex) / 2);
                SetItems(dict, markIndex);
                _stack.Append(dict);
            }

            private void LoadDup(CodeContext context) {
                _stack.Append(_stack[-1]);
            }

            private void LoadEmptyDict(CodeContext context) {
                _stack.Append(new PythonDictionary());
            }

            private void LoadEmptyList(CodeContext context) {
                _stack.Append(List.MakeList());
            }

            private void LoadEmptyTuple(CodeContext context) {
                _stack.Append(PythonTuple.MakeTuple());
            }

            private void LoadExt1(CodeContext context) {
                PythonTuple global = (PythonTuple)PythonCopyReg.InvertedRegistry[(int)ReadUInt8()];
                _stack.Append(find_global(context, global[0], global[1]));
            }

            private void LoadExt2(CodeContext context) {
                PythonTuple global = (PythonTuple)PythonCopyReg.InvertedRegistry[(int)ReadUInt16()];
                _stack.Append(find_global(context, global[0], global[1]));
            }

            private void LoadExt4(CodeContext context) {
                PythonTuple global = (PythonTuple)PythonCopyReg.InvertedRegistry[ReadInt32()];
                _stack.Append(find_global(context, global[0], global[1]));
            }

            private void LoadFloat(CodeContext context) {
                _stack.Append(ReadFloatString(context));
            }

            private void LoadGet(CodeContext context) {
                try {
                    _stack.Append(MemoGet((long)(int)ReadIntFromString(context)));
                } catch (ArgumentException) {
                    throw ExceptionConverter.CreateThrowable(BadPickleGet, "while executing GET: invalid integer value");
                }
            }

            private void LoadGlobal(CodeContext context) {
                string module = ReadLineNoNewline();
                string attr = ReadLineNoNewline();
                _stack.Append(find_global(context, module, attr));
            }

            private void LoadInst(CodeContext context) {
                LoadGlobal(context);
                object cls = _stack.Pop();
                if (cls is OldClass || cls is PythonType) {
                    int markIndex = MarkIndex;
                    object[] args = _stack.GetSliceAsArray(markIndex + 1, _stack.Count);
                    PopMark(markIndex);

                    _stack.Append(MakeInstance(context, cls, args));
                } else {
                    throw PythonOps.TypeError("expected class or type after INST, got {0}", DynamicHelpers.GetPythonType(cls));
                }
            }

            private void LoadInt(CodeContext context) {
                _stack.Append(ReadIntFromString(context));
            }

            private void LoadList(CodeContext context) {
                int markIndex = MarkIndex;
                object list = _stack.GetSlice(markIndex + 1, _stack.Count);
                PopMark(markIndex);
                _stack.Append(list);
            }

            private void LoadLong(CodeContext context) {
                _stack.Append(ReadLongFromString(context));
            }

            private void LoadLong1(CodeContext context) {
                _stack.Append(ReadLong(ReadUInt8()));
            }

            private void LoadLong4(CodeContext context) {
                _stack.Append(ReadLong(ReadInt32()));
            }

            private void LoadLongBinGet(CodeContext context) {
                _stack.Append(MemoGet((long)(int)ReadInt32()));
            }

            private void LoadLongBinPut(CodeContext context) {
                MemoPut((long)(int)ReadInt32(), _stack[-1]);
            }

            private void LoadMark(CodeContext context) {
                _stack.Append(_mark);
            }

            private void LoadNewFalse(CodeContext context) {
                _stack.Append(RuntimeHelpers.False);
            }

            private void LoadNewObj(CodeContext context) {
                PythonTuple args = _stack.Pop() as PythonTuple;
                if (args == null) {
                    throw PythonOps.TypeError("expected tuple as second argument to NEWOBJ, got {0}", DynamicHelpers.GetPythonType(args));
                }

                PythonType cls = _stack.Pop() as PythonType;
                if (args == null) {
                    throw PythonOps.TypeError("expected new-style type as first argument to NEWOBJ, got {0}", DynamicHelpers.GetPythonType(args));
                }

                PythonTypeSlot dts;
                object value;
                if (cls.TryResolveSlot(context, Symbols.NewInst, out dts) &&
                    dts.TryGetValue(context, null, cls, out value)) {
                    object[] newargs = new object[args.Count + 1];
                    args.CopyTo(newargs, 1);
                    newargs[0] = cls;

                    _stack.Append(PythonOps.CallWithContext(context, value, newargs));
                    return;
                }
                
                throw PythonOps.TypeError("didn't find __new__");
            }

            private void LoadNewTrue(CodeContext context) {
                _stack.Append(RuntimeHelpers.True);
            }

            private void LoadNoneValue(CodeContext context) {
                _stack.Append(NoneTypeOps.Instance);
            }

            private void LoadObj(CodeContext context) {
                int markIndex = MarkIndex;
                object cls = _stack[markIndex + 1];
                if (cls is OldClass || cls is PythonType) {
                    object[] args = _stack.GetSliceAsArray(markIndex + 2, _stack.Count);
                    PopMark(markIndex);
                    _stack.Append(MakeInstance(context, cls, args));
                } else {
                    throw PythonOps.TypeError("expected class or type as first argument to INST, got {0}", DynamicHelpers.GetPythonType(cls));
                }
            }

            private void LoadPersId(CodeContext context) {
                if (_pers_loader == null) {
                    throw CannotUnpickle("A load persistent ID instruction is present but no persistent_load function is available");
                }

                if (_pers_site == null) _pers_site = RuntimeHelpers.CreateSimpleCallSite<object, object, object>();

                _stack.Append(_pers_site.Invoke(context, _pers_loader, ReadLineNoNewline()));
            }

            private void LoadPop(CodeContext context) {
                _stack.Pop();
            }

            private void LoadPopMark(CodeContext context) {
                PopMark(MarkIndex);
            }

            private void LoadProto(CodeContext context) {
                int proto = ReadUInt8();
                if (proto > 2) throw PythonOps.ValueError("unsupported pickle protocol: {0}", proto);
                // discard result
            }

            private void LoadPut(CodeContext context) {
                MemoPut((long)(int)ReadIntFromString(context), _stack[-1]);
            }

            private void LoadReduce(CodeContext context) {
                object args = _stack.Pop();
                object callable = _stack.Pop();
                if (args == null) {
                    _stack.Append(PythonCalls.Call(PythonOps.GetBoundAttr(context, callable, SymbolTable.StringToId("__basicnew__"))));
                } else if (!DynamicHelpers.GetPythonType(args).Equals(TypeCache.PythonTuple)) {
                    throw PythonOps.TypeError(
                        "while executing REDUCE, expected tuple at the top of the stack, but got {0}",
                        DynamicHelpers.GetPythonType(args)
                    );
                }
                _stack.Append(PythonOps.CallWithArgsTupleAndContext(context, callable, ArrayUtils.EmptyObjects, args));
            }

            private void LoadSetItem(CodeContext context) {
                object value = _stack.Pop();
                object key = _stack.Pop();
                PythonDictionary dict = _stack[-1] as PythonDictionary;
                if (dict == null) {
                    throw PythonOps.TypeError(
                        "while executing SETITEM, expected dict at stack[-3], but got {0}",
                        DynamicHelpers.GetPythonType(_stack[-1])
                    );
                }
                dict[key] = value;
            }

            private void LoadSetItems(CodeContext context) {
                int markIndex = MarkIndex;
                PythonDictionary dict = _stack[markIndex - 1] as PythonDictionary;
                if (dict == null) {
                    throw PythonOps.TypeError(
                        "while executing SETITEMS, expected dict below last mark, but got {0}",
                        DynamicHelpers.GetPythonType(_stack[markIndex - 1])
                    );
                }
                SetItems(dict, markIndex);
            }

            private void LoadShortBinstring(CodeContext context) {
                _stack.Append(Read(ReadUInt8()));
            }

            private void LoadString(CodeContext context) {
                string repr = ReadLineNoNewline();
                if (repr.Length < 2 ||
                    !(
                    repr[0] == '"' && repr[repr.Length - 1] == '"' ||
                    repr[0] == '\'' && repr[repr.Length - 1] == '\''
                    )
                ) {
                    throw PythonOps.ValueError("while executing STRING, expected string that starts and ends with quotes");
                }
#if !SILVERLIGHT // Encoding
                _stack.Append(StringOps.Decode(context, repr.Substring(1, repr.Length - 2), "string-escape", "strict"));
#else
                throw new NotImplementedException("SILVERLIGHT - not supported - encoding");
#endif
            }

            private void LoadTuple(CodeContext context) {
                int markIndex = MarkIndex;
                PythonTuple tuple = PythonTuple.MakeTuple(_stack.GetSliceAsArray(markIndex + 1, _stack.Count));
                PopMark(markIndex);
                _stack.Append(tuple);
            }

            private void LoadTuple1(CodeContext context) {
                object item0 = _stack.Pop();
                _stack.Append(PythonTuple.MakeTuple(item0));
            }

            private void LoadTuple2(CodeContext context) {
                object item1 = _stack.Pop();
                object item0 = _stack.Pop();
                _stack.Append(PythonTuple.MakeTuple(item0, item1));
            }

            private void LoadTuple3(CodeContext context) {
                object item2 = _stack.Pop();
                object item1 = _stack.Pop();
                object item0 = _stack.Pop();
                _stack.Append(PythonTuple.MakeTuple(item0, item1, item2));
            }

            private void LoadUnicode(CodeContext context) {
#if !SILVERLIGHT    // Encoding
                _stack.Append(StringOps.Decode(context, ReadLineNoNewline(), "raw-unicode-escape", "strict"));
#else
                throw new NotImplementedException("SILVERLIGHT - not supported - encoding");
#endif
            }
        }

        #endregion

    }
}
