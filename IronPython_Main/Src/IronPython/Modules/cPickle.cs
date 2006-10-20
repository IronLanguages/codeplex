/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

using IronMath;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("cPickle", typeof(IronPython.Modules.PythonPickle))]
namespace IronPython.Modules {
    [PythonType("cPickle")]
    [Documentation("Fast object serialization/unserialization.\n\n"
        + "Differences from CPython:\n"
        + " - no persistent id support\n"
        + " - does not implement the undocumented fast mode\n"
        )]
    public static class PythonPickle {

        public static int HIGHEST_PROTOCOL = 2;

        public const string Newline = "\n";

        public static IPythonType PickleError = ExceptionConverter.CreatePythonException("PickleError", "cPickle");
        public static IPythonType PicklingError = ExceptionConverter.CreatePythonException("PicklingError", "cPickle", PickleError);
        public static IPythonType UnpicklingError = ExceptionConverter.CreatePythonException("UnpicklingError", "cPickle", PickleError);
        public static IPythonType BadPickleGet = ExceptionConverter.CreatePythonException("BadPickleGet", "cPickle", UnpicklingError);

        #region Public module-level functions

        [Documentation("dump(obj, file, protocol=0) -> None\n\n"
            + "Pickle obj and write the result to file.\n"
            + "\n"
            + "See documentation for Pickler() for a description the file, protocol, and\n"
            + "(deprecated) bin parameters."
            )]
        [PythonName("dump")]
        public static void DumpToFile(ICallerContext context, object obj, object file, [DefaultParameterValue(null)] object protocol, [DefaultParameterValue(null)] object bin) {
            Pickler pickler = new Pickler(file, protocol, bin);
            pickler.Dump(context, obj);
        }

        [Documentation("dumps(obj, protocol=0) -> pickle string\n\n"
            + "Pickle obj and return the result as a string.\n"
            + "\n"
            + "See the documentation for Pickler() for a description of the protocol and\n"
            + "(deprecated) bin parameters."
            )]
        [PythonName("dumps")]
        public static string DumpToString(ICallerContext context, object obj, [DefaultParameterValue(null)] object protocol, [DefaultParameterValue(null)] object bin) {
            //??? possible perf enhancement: use a C# TextWriter-backed IFileOutput and
            // thus avoid Python call overhead. Also do similar thing for LoadFromString.
            object stringIO = Ops.Invoke(Ops.GetDynamicTypeFromClsOnlyType(typeof(PythonStringIO)), SymbolTable.StringToId("StringIO"));
            Pickler pickler = new Pickler(stringIO, protocol, bin);
            pickler.Dump(context, obj);
            return Converter.ConvertToString(Ops.Invoke(stringIO, SymbolTable.StringToId("getvalue")));
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
        [PythonName("load")]
        public static object LoadFromFile(ICallerContext context, object file) {
            return new Unpickler(file).Load(context);
        }

        [Documentation("loads(string) -> unpickled object\n\n"
            + "Read a pickle object from a string, unpickle it, and return the resulting\n"
            + "reconstructed object. Characters in the string beyond the end of the first\n"
            + "pickle are ignored."
            )]
        [PythonName("loads")]
        public static object LoadFromString(ICallerContext context, object @string) {
            object stringIO = Ops.Invoke(
                Ops.GetDynamicTypeFromClsOnlyType(typeof(PythonStringIO)),
                SymbolTable.StringToId("StringIO"),
                @string
            );
            return new Unpickler(stringIO).Load(context);
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
            private object readMethod;
            private object readLineMethod;

            public PythonFileInput(object file) {
                if (!Ops.TryGetAttr(file, SymbolTable.StringToId("read"), out readMethod) ||
                    !Ops.IsCallable(readMethod) ||
                    !Ops.TryGetAttr(file, SymbolTable.StringToId("readline"), out readLineMethod) ||
                    !Ops.IsCallable(readLineMethod)
                ) {
                    throw Ops.TypeError("argument must have callable 'read' and 'readline' attributes");
                }
            }

            public string Read(int size) {
                return Converter.ConvertToString(Ops.Call(readMethod, size));
            }

            public string ReadLine() {
                return Converter.ConvertToString(Ops.Call(readLineMethod));
            }
        }

        private class PythonFileOutput : IFileOutput {
            private object writeMethod;

            public PythonFileOutput(object file) {
                if (!Ops.TryGetAttr(file, SymbolTable.StringToId("write"), out writeMethod) ||
                    !Ops.IsCallable(this.writeMethod)
                ) {
                    throw Ops.TypeError("argument must have callable 'write' attribute");
                }
            }

            public void Write(string data) {
                Ops.Call(writeMethod, data);
            }
        }

        private class PythonReadableFileOutput : PythonFileOutput {
            private object getValueMethod;

            public PythonReadableFileOutput(object file) : base(file) {
                if (!Ops.TryGetAttr(file, SymbolTable.StringToId("getvalue"), out getValueMethod) ||
                    !Ops.IsCallable(getValueMethod)
                ) {
                    throw Ops.TypeError("argument must have callable 'getvalue' attribute");
                }
            }

            public object GetValue() {
                return Ops.Call(getValueMethod);
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
        [PythonType("Pickler")]
        public class Pickler {

            private const char LowestPrintableChar = (char)32;
            private const char HighestPrintableChar = (char)126;
            // max elements that can be set/appended at a time using SETITEMS/APPENDS

            private delegate void PickleFunction(ICallerContext context, object value);
            private readonly Dictionary<DynamicType, PickleFunction> dispatchTable;

            private int batchSize = 1000;
            private IFileOutput file;
            private int protocol;
            private IDictionary memo;

            #region Public API

            public IDictionary Memo {
                [PythonName("memo")]
                get { return memo; }
                [PythonName("memo")]
                set { memo = value; }
            }

            public int Protocol {
                [PythonName("proto")]
                get { return protocol; }
                [PythonName("proto")]
                set { protocol = value; }
            }

            public int BatchSize {
                [PythonName("_BATCHSIZE")]
                get { return batchSize; }
                [PythonName("_BATCHSIZE")]
                set { batchSize = value; }
            }

            public int Binary {
                [PythonName("binary")]
                get { return protocol == 0 ? 1 : 0; }
            }

            public int Fast {
                // We don't implement fast, but we silently ignore it when it's set so that test_cpickle works.
                // For a description of fast, see http://mail.python.org/pipermail/python-bugs-list/2001-October/007695.html
                [PythonName("fast")]
                get { return 0; }
                [PythonName("fast")]
                set { /* ignore */ }
            }

            [PythonName("__new__")]
            public static Pickler Make(DynamicType cls,
                [DefaultParameterValue(null)] object file,
                [DefaultParameterValue(null)] object protocol,
                [DefaultParameterValue(null)] object bin
            ) {
                if (cls == Ops.GetDynamicTypeFromClsOnlyType(typeof(Pickler))) {
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
                    Pickler pickler = cls.ctor.Call(cls, file, protocol, bin) as Pickler;
                    if (pickler == null) throw Ops.TypeError("{0} is not a subclass of Pickler", cls);
                    return pickler;
                }
            }

            public Pickler(object file, object protocol, object bin)
                : this(new PythonFileOutput(file), protocol, bin) { }

            public Pickler(IFileOutput file, object protocol, object bin) {
                dispatchTable = new Dictionary<DynamicType, PickleFunction>();
                dispatchTable[TypeCache.Boolean] = SaveBoolean;
                dispatchTable[TypeCache.Int32] = SaveInteger;
                dispatchTable[TypeCache.None] = SaveNone;
                dispatchTable[TypeCache.Dict] = SaveDict;
                dispatchTable[TypeCache.BigInteger] = SaveLong;
                dispatchTable[TypeCache.Double] = SaveFloat;
                dispatchTable[TypeCache.String] = SaveUnicode;
                dispatchTable[TypeCache.Tuple] = SaveTuple;
                dispatchTable[TypeCache.List] = SaveList;
                dispatchTable[TypeCache.OldClass] = SaveGlobal;
                dispatchTable[TypeCache.Function] = SaveGlobal;
                dispatchTable[TypeCache.BuiltinFunction] = SaveGlobal;
                dispatchTable[TypeCache.DynamicType] = SaveGlobal;
                dispatchTable[TypeCache.OldInstance] = SaveInstance;

                this.file = file;
                this.memo = new Dict();

                if (protocol == null) protocol = Ops.IsTrue(bin) ? 1 : 0;

                int intProtocol = Converter.ConvertToInt32(protocol);
                if (intProtocol > HIGHEST_PROTOCOL) {
                    throw Ops.ValueError("pickle protocol {0} asked for; the highest available protocol is {1}", intProtocol, HIGHEST_PROTOCOL);
                } else if (intProtocol < 0) {
                    this.protocol = HIGHEST_PROTOCOL;
                } else {
                    this.protocol = intProtocol;
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
            [PythonName("dump")]
            public void Dump(ICallerContext context, object obj) {
                if (protocol >= 2) WriteProto();
                Save(context, obj);
                Write(Opcode.Stop);
            }

            [Documentation("clear_memo() -> None\n\n"
                + "Clear the memo, which is used internally by the pickler to keep track of which\n"
                + "objects have already been pickled (so that shared or recursive objects are\n"
                + "pickled only once)."
                )]
            [PythonName("clear_memo")]
            public void ClearMemo() {
                memo.Clear();
            }

            [Documentation("getvalue() -> string\n\n"
                + "Return the value of the internal string. Raises PicklingError if a file object\n"
                + "was passed to this pickler's constructor."
                )]
            [PythonName("getvalue")]
            public object GetValue() {
                if (file is PythonReadableFileOutput) {
                    return ((PythonReadableFileOutput)file).GetValue();
                }
                throw ExceptionConverter.CreateThrowable(PicklingError, "Attempt to getvalue() a non-list-based pickler");
            }

            #endregion

            #region Save functions

            private void Save(ICallerContext context, object obj) {
                if (memo.Contains(Ops.Id(obj))) {
                    WriteGet(obj);
                } else {
                    PickleFunction pickleFunction;
                    DynamicType objType = Ops.GetDynamicType(obj);
                    if (!dispatchTable.TryGetValue(objType, out pickleFunction)) {
                        if (objType.IsSubclassOf(TypeCache.DynamicType)) {
                            // treat classes with metaclasses like regular classes
                            pickleFunction = SaveGlobal;
                        } else {
                            pickleFunction = SaveObject;
                        }
                    }
                    pickleFunction(context, obj);
                }
            }

            private void SaveBoolean(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.Boolean), "arg must be bool");
                if (protocol < 2) {
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

            private void SaveDict(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.Dict), "arg must be dict");
                Debug.Assert(!memo.Contains(Ops.Id(obj)));
                Memoize(obj);

                if (protocol < 1) {
                    Write(Opcode.Mark);
                    Write(Opcode.Dict);
                } else {
                    Write(Opcode.EmptyDict);
                }

                WritePut(obj);
                BatchSetItems(context, (DictOps.IterItems((IDictionary<object, object>)obj)));
            }

            private void SaveFloat(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.Double), "arg must be float");

                if (protocol < 1) {
                    Write(Opcode.Float);
                    WriteFloatAsString(obj);
                } else {
                    Write(Opcode.BinFloat);
                    WriteFloat64(obj);
                }
            }

            private void SaveGlobal(ICallerContext context, object obj) {
                Debug.Assert(
                    Ops.GetDynamicType(obj).Equals(TypeCache.OldClass) ||
                    Ops.GetDynamicType(obj).Equals(TypeCache.Function) ||
                    Ops.GetDynamicType(obj).Equals(TypeCache.BuiltinFunction) ||
                    Ops.GetDynamicType(obj).Equals(TypeCache.DynamicType) ||
                    Ops.GetDynamicType(obj).IsSubclassOf(TypeCache.DynamicType),
                    "arg must be classic class, function, built-in function or method, or new-style type"
                );

                object name;
                if (Ops.TryGetAttr(obj, SymbolTable.Name, out name)) {
                    SaveGlobalByName(context, obj, name);
                } else {
                    throw CannotPickle(obj, "could not determine its __name__");
                }
            }

            private void SaveGlobalByName(ICallerContext context, object obj, object name) {
                Debug.Assert(!memo.Contains(Ops.Id(obj)));

                object moduleName = FindModuleForGlobal(context, obj, name);

                if (protocol >= 2) {
                    object code;
                    if (PythonCopyReg.ExtensionRegistry.TryGetValue(Tuple.MakeTuple(moduleName, name), out code)) {
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
                            throw Ops.RuntimeError("unrecognized integer format");
                        }
                        return;
                    }
                }

                Memoize(obj);

                Write(Opcode.Global);
                WriteStringPair(moduleName, name);
                WritePut(obj);
            }

            private void SaveInstance(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.OldInstance), "arg must be old-class instance");
                Debug.Assert(!memo.Contains(Ops.Id(obj)));

                Write(Opcode.Mark);

                // Memoize() call isn't in the usual spot to allow class to be memoized before
                // instance (when using proto other than 0) to match CPython's bytecode output

                object objClass;
                if (!Ops.TryGetAttr(obj, SymbolTable.Class, out objClass)) {
                    throw CannotPickle(obj, "could not determine its __class__");
                }

                if (protocol < 1) {
                    object className, classModuleName;
                    if (!Ops.TryGetAttr(objClass, SymbolTable.Name, out className)) {
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
                if (Ops.TryGetAttr(obj, SymbolTable.GetState, out getStateCallable)) {
                    Save(context, Ops.Call(getStateCallable));
                } else {
                    Save(context, Ops.GetAttr(context, obj, SymbolTable.Dict));
                }

                Write(Opcode.Build);
            }

            private void SaveInteger(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.Int32), "arg must be int");
                if (protocol < 1) {
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
                        throw Ops.RuntimeError("unrecognized integer format");
                    }
                }
            }

            private void SaveList(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.List), "arg must be list");
                Debug.Assert(!memo.Contains(Ops.Id(obj)));
                Memoize(obj);
                if (protocol < 1) {
                    Write(Opcode.Mark);
                    Write(Opcode.List);
                } else {
                    Write(Opcode.EmptyList);
                }

                WritePut(obj);
                BatchAppends(context, ((IEnumerable)obj).GetEnumerator());
            }

            private void SaveLong(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.BigInteger), "arg must be long");

                if (protocol < 2) {
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

            private void SaveNone(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.None), "arg must be None");
                Write(Opcode.NoneValue);
            }

            /// <summary>
            /// Call the appropriate reduce method for obj and pickle the object using
            /// the resulting data. Use the first available of
            /// copy_reg.dispatch_table[type(obj)], obj.__reduce_ex__, and obj.__reduce__.
            /// </summary>
            private void SaveObject(ICallerContext context, object obj) {
                Debug.Assert(!memo.Contains(Ops.Id(obj)));
                Memoize(obj);

                object reduceCallable, result;
                DynamicType objType = Ops.GetDynamicType(obj);

                if (PythonCopyReg.DispatchTable.TryGetValue(objType, out reduceCallable)) {
                    result = Ops.Call(reduceCallable, obj);
                } else if (Ops.TryGetAttr(obj, SymbolTable.ReduceEx, out reduceCallable)) {
                    if (obj is DynamicType) {
                        result = Ops.Call(reduceCallable, obj, protocol);
                    } else {
                        result = Ops.Call(reduceCallable, protocol);
                    }
                } else if (Ops.TryGetAttr(obj, SymbolTable.Reduce, out reduceCallable)) {
                    if (obj is DynamicType) {
                        result = Ops.Call(reduceCallable, obj);
                    } else {
                        result = Ops.Call(reduceCallable);
                    }
                } else {
                    throw Ops.AttributeError("no reduce function found for {0}", obj);
                }

                if (objType.Equals(TypeCache.String)) {
                    if (memo.Contains(Ops.Id(obj))) {
                        WriteGet(obj);
                    } else {
                        SaveGlobalByName(context, obj, result);
                    }
                } else if (result is Tuple) {
                    Tuple rt = (Tuple)result;
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
            private void SaveReduce(ICallerContext context, object obj, object reduceCallable, object func, object args, object state, object listItems, object dictItems) {
                if (!Ops.IsCallable(func)) {
                    throw CannotPickle(obj, "func from reduce() should be callable");
                } else if (!(args is Tuple) && args != null) {
                    throw CannotPickle(obj, "args from reduce() should be a tuple");
                } else if (listItems != null && !(listItems is IEnumerator)) {
                    throw CannotPickle(obj, "listitems from reduce() should be a list iterator");
                } else if (dictItems != null && !(dictItems is IEnumerator)) {
                    throw CannotPickle(obj, "dictitems from reduce() should be a dict iterator");
                }

                object funcName;
                string funcNameString;
                if (!Ops.TryGetAttr(func, SymbolTable.Name, out funcName)) {
                    throw CannotPickle(obj, "func from reduce() ({0}) should have a __name__ attribute");
                } else if (!Converter.TryConvertToString(funcName, out funcNameString) || funcNameString == null) {
                    throw CannotPickle(obj, "__name__ of func from reduce() must be string");
                }

                if (protocol >= 2 && "__newobj__" == funcNameString) {
                    if (args == null) {
                        throw CannotPickle(obj, "__newobj__ arglist is None");
                    }
                    Tuple argsTuple = (Tuple)args;
                    if (argsTuple.Count == 0) {
                        throw CannotPickle(obj, "__newobj__ arglist is empty");
                    } else if (!Ops.GetDynamicType(obj).Equals(argsTuple[0])) {
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

            private void SaveTuple(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.Tuple), "arg must be tuple");
                Debug.Assert(!memo.Contains(Ops.Id(obj)));
                Tuple t = (Tuple)obj;
                string opcode;
                bool needMark = false;
                if (protocol > 0 && t.Count == 0) {
                    opcode = Opcode.EmptyTuple;
                } else if (protocol >= 2 && t.Count == 1) {
                    opcode = Opcode.Tuple1;
                } else if (protocol >= 2 && t.Count == 2) {
                    opcode = Opcode.Tuple2;
                } else if (protocol >= 2 && t.Count == 3) {
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

            private void SaveUnicode(ICallerContext context, object obj) {
                Debug.Assert(Ops.GetDynamicType(obj).Equals(TypeCache.String), "arg must be unicode");
                Debug.Assert(!memo.Contains(Ops.Id(obj)));
                Memoize(obj);
                if (protocol < 1) {
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
                Debug.Assert(Ops.GetDynamicType(value).Equals(TypeCache.Double));
                // 17 digits of precision are necessary for accurate roundtripping
                StringFormatter sf = new StringFormatter("%.17g", value);
                sf.TrailingZeroAfterWholeFloat = true;
                Write(sf.Format());
                Write(Newline);
            }

            /// <summary>
            /// Write value in pickle float8 format.
            /// </summary>
            private void WriteFloat64(object value) {
                Debug.Assert(Ops.GetDynamicType(value).Equals(TypeCache.Double));
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
                Write(Ops.StringRepr(value));
                Write(Newline);
            }

            /// <summary>
            /// Write value in pickle decimalnl_long format.
            /// </summary>
            private void WriteLongAsString(object value) {
                Debug.Assert(Ops.GetDynamicType(value).Equals(TypeCache.BigInteger));
                Write(Ops.StringRepr(value));
                Write(Newline);
            }

            /// <summary>
            /// Write value in pickle unicodestringnl format.
            /// </summary>
            private void WriteUnicodeStringRaw(object value) {
                Debug.Assert(Ops.GetDynamicType(value).Equals(TypeCache.String));
                // manually escape backslash and newline
                Write(StringOps.RawUnicodeEscapeEncode(((string)value).Replace("\\", "\\u005c").Replace("\n", "\\u000a")));
                Write(Newline);
            }

            /// <summary>
            /// Write value in pickle unicodestring4 format.
            /// </summary>
            private void WriteUnicodeStringUtf8(object value) {
                Debug.Assert(Ops.GetDynamicType(value).Equals(TypeCache.String));
                string encodedString = StringOps.FromByteArray(System.Text.Encoding.UTF8.GetBytes((string)value));
                WriteInt32(encodedString.Length);
                Write(encodedString);
            }

            /// <summary>
            /// Write value in pickle stringnl_noescape_pair format.
            /// </summary>
            private void WriteStringPair(object value1, object value2) {
                Debug.Assert(Ops.GetDynamicType(value1).Equals(TypeCache.String));
                Debug.Assert(Ops.GetDynamicType(value2).Equals(TypeCache.String));
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
                return Ops.LessThanOrEqualRetBool(0, value) && Ops.LessThanRetBool(value, 1 << 8);
            }

            /// <summary>
            /// Return true if value is appropriate for formatting in pickle uint2 format.
            /// </summary>
            private bool IsUInt16(object value) {
                return Ops.LessThanOrEqualRetBool(1 << 8, value) && Ops.LessThanRetBool(value, 1 << 16);
            }

            /// <summary>
            /// Return true if value is appropriate for formatting in pickle int4 format.
            /// </summary>
            private bool IsInt32(object value) {
                return Ops.LessThanOrEqualRetBool(Int32.MinValue, value) && Ops.LessThanOrEqualRetBool(value, Int32.MaxValue);
            }

            /// <summary>
            /// Return true if value is a string where each value is in the range of printable ASCII characters.
            /// </summary>
            private bool IsPrintableAscii(object value) {
                Debug.Assert(Ops.GetDynamicType(value).Equals(TypeCache.String));
                string strValue = (string)value;
                foreach (char c in strValue) {
                    if (!(LowestPrintableChar <= c && c <= HighestPrintableChar)) return false;
                }
                return true;
            }

            #endregion

            #region Output generation helpers

            private void Write(string data) {
                file.Write(data);
            }

            private void WriteGet(object obj) {
                Debug.Assert(memo.Contains(Ops.Id(obj)));
                // Memo entries are tuples, and the first element is the memo index
                IList<object> memoEntry = (IList<object>)memo[Ops.Id(obj)];

                object index = memoEntry[0];
                Debug.Assert(Ops.GreaterThanOrEqualRetBool(index, 0));
                if (protocol < 1) {
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

            private void WriteInitArgs(ICallerContext context, object obj) {
                object getInitArgsCallable;
                if (Ops.TryGetAttr(obj, SymbolTable.GetInitArgs, out getInitArgsCallable)) {
                    object initArgs = Ops.Call(getInitArgsCallable);
                    if (!(initArgs is Tuple)) {
                        throw CannotPickle(obj, "__getinitargs__() must return tuple");
                    }
                    foreach (object arg in (Tuple)initArgs) {
                        Save(context, arg);
                    }
                }
            }

            private void WritePut(object obj) {
                Debug.Assert(memo.Contains(Ops.Id(obj)));
                // Memo entries are tuples, and the first element is the memo index
                IList<object> memoEntry = (IList<object>)memo[Ops.Id(obj)];

                object index = memoEntry[0];
                Debug.Assert(Ops.GreaterThanOrEqualRetBool(index, 0));
                if (protocol < 1) {
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
                WriteUInt8(protocol);
            }

            /// <summary>
            /// Emit a series of opcodes that will set append all items indexed by iter
            /// to the object at the top of the stack. Use APPENDS if possible, but
            /// append no more than BatchSize items at a time.
            /// </summary>
            private void BatchAppends(ICallerContext context, IEnumerator enumerator) {
                if (protocol < 1) {
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

                        if (batchCompleted == BatchSize) {
                            Write(Opcode.Appends);
                            batchCompleted = 0;
                        }

                        if (batchCompleted == 0) {
                            Write(Opcode.Mark);
                        }

                        Save(context, current);
                        batchCompleted++;
                    }

                    if (batchCompleted == BatchSize) {
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
            private void BatchSetItems(ICallerContext context, IEnumerator enumerator) {
                Tuple kvTuple;
                if (protocol < 1) {
                    while (enumerator.MoveNext()) {
                        kvTuple = (Tuple)enumerator.Current;
                        Save(context, kvTuple[0]);
                        Save(context, kvTuple[1]);
                        Write(Opcode.SetItem);
                    }
                } else {
                    object nextKey, nextValue;
                    if (enumerator.MoveNext()) {
                        kvTuple = (Tuple)enumerator.Current;
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
                        kvTuple = (Tuple)enumerator.Current;
                        nextKey = kvTuple[0];
                        nextValue = kvTuple[1];

                        if (batchCompleted == BatchSize) {
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

                    if (batchCompleted == BatchSize) {
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
                Debug.Assert(!memo.Contains(Ops.Id(obj)));
                memo[Ops.Id(obj)] = Tuple.MakeTuple(memo.Count, obj);
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
            private object FindModuleForGlobal(ICallerContext context, object obj, object name) {
                object module;
                object moduleName;
                if (Ops.TryGetAttr(obj, SymbolTable.Module, out moduleName)) {
                    if (!Importer.TryGetExistingModule(context.SystemState, Converter.ConvertToString(moduleName), out module)) {
                        module = Builtin.__import__(context, Converter.ConvertToString(moduleName));
                    }

                    object foundObj;
                    if (Ops.TryGetAttr(module, SymbolTable.StringToId(Converter.ConvertToString(name)), out foundObj)) {
                        if (Ops.IsRetBool(foundObj, obj)) {
                            return moduleName;
                        } else {
                            throw CannotPickle(obj, "it's not the same object as {0}.{1}", moduleName, name);
                        }
                    } else {
                        throw CannotPickle(obj, "it's not found as {0}.{1}", moduleName, name);
                    }
                } else {
                    // No obj.__module__, so crawl through all loaded modules looking for obj
                    foreach (KeyValuePair<object, object> modulePair in context.SystemState.modules) {
                        moduleName = modulePair.Key;
                        module = modulePair.Value;
                        object foundObj;
                        if (Ops.TryGetAttr(module, SymbolTable.StringToId(Converter.ConvertToString(name)), out foundObj) &&
                            Ops.IsRetBool(foundObj, obj)
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
        [PythonType("Unpickler")]
        public class Unpickler {

            private readonly object mark = new object();

            private delegate void LoadFunction(ICallerContext context);
            private readonly Dictionary<string, LoadFunction> dispatch;

            private IFileInput file;
            private List stack;
            private IDictionary<object, object> memo;

            public Unpickler(object file)
                : this(new PythonFileInput(file)) { }

            public Unpickler(IFileInput file) {
                this.file = file;
                memo = new Dict();

                dispatch = new Dictionary<string, LoadFunction>();
                dispatch[""] = LoadEof;
                dispatch[Opcode.Append] = LoadAppend;
                dispatch[Opcode.Appends] = LoadAppends;
                dispatch[Opcode.BinFloat] = LoadBinFloat;
                dispatch[Opcode.BinGet] = LoadBinGet;
                dispatch[Opcode.BinInt] = LoadBinInt;
                dispatch[Opcode.BinInt1] = LoadBinInt1;
                dispatch[Opcode.BinInt2] = LoadBinInt2;
                dispatch[Opcode.BinPersid] = LoadBinPersid;
                dispatch[Opcode.BinPut] = LoadBinPut;
                dispatch[Opcode.BinString] = LoadBinString;
                dispatch[Opcode.BinUnicode] = LoadBinUnicode;
                dispatch[Opcode.Build] = LoadBuild;
                dispatch[Opcode.Dict] = LoadDict;
                dispatch[Opcode.Dup] = LoadDup;
                dispatch[Opcode.EmptyDict] = LoadEmptyDict;
                dispatch[Opcode.EmptyList] = LoadEmptyList;
                dispatch[Opcode.EmptyTuple] = LoadEmptyTuple;
                dispatch[Opcode.Ext1] = LoadExt1;
                dispatch[Opcode.Ext2] = LoadExt2;
                dispatch[Opcode.Ext4] = LoadExt4;
                dispatch[Opcode.Float] = LoadFloat;
                dispatch[Opcode.Get] = LoadGet;
                dispatch[Opcode.Global] = LoadGlobal;
                dispatch[Opcode.Inst] = LoadInst;
                dispatch[Opcode.Int] = LoadInt;
                dispatch[Opcode.List] = LoadList;
                dispatch[Opcode.Long] = LoadLong;
                dispatch[Opcode.Long1] = LoadLong1;
                dispatch[Opcode.Long4] = LoadLong4;
                dispatch[Opcode.LongBinGet] = LoadLongBinGet;
                dispatch[Opcode.LongBinPut] = LoadLongBinPut;
                dispatch[Opcode.Mark] = LoadMark;
                dispatch[Opcode.NewFalse] = LoadNewFalse;
                dispatch[Opcode.NewObj] = LoadNewObj;
                dispatch[Opcode.NewTrue] = LoadNewTrue;
                dispatch[Opcode.NoneValue] = LoadNoneValue;
                dispatch[Opcode.Obj] = LoadObj;
                dispatch[Opcode.PersId] = LoadPersId;
                dispatch[Opcode.Pop] = LoadPop;
                dispatch[Opcode.PopMark] = LoadPopMark;
                dispatch[Opcode.Proto] = LoadProto;
                dispatch[Opcode.Put] = LoadPut;
                dispatch[Opcode.Reduce] = LoadReduce;
                dispatch[Opcode.SetItem] = LoadSetItem;
                dispatch[Opcode.SetItems] = LoadSetItems;
                dispatch[Opcode.ShortBinstring] = LoadShortBinstring;
                dispatch[Opcode.String] = LoadString;
                dispatch[Opcode.Tuple] = LoadTuple;
                dispatch[Opcode.Tuple1] = LoadTuple1;
                dispatch[Opcode.Tuple2] = LoadTuple2;
                dispatch[Opcode.Tuple3] = LoadTuple3;
                dispatch[Opcode.Unicode] = LoadUnicode;
            }

            [Documentation("load() -> unpickled object\n\n"
                + "Read pickle data from the file object that was passed to the constructor and\n"
                + "return the corresponding unpickled objects."
               )]
            [PythonName("load")]
            public object Load(ICallerContext context) {
                stack = new List();

                string opcode = Read(1);

                while (opcode != Opcode.Stop) {
                    if (!dispatch.ContainsKey(opcode)) {
                        throw CannotUnpickle("invalid opcode: {0}", Ops.StringRepr(opcode));
                    }
                    dispatch[opcode](context);
                    opcode = Read(1);
                }

                return stack.Pop();
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
            [PythonName("noload")]
            public void NoLoad(ICallerContext context) {
                throw Ops.NotImplementedError("noload() is not implemented");
            }

            private Exception CannotUnpickle(string format, params object[] args) {
                return ExceptionConverter.CreateThrowable(PicklingError, String.Format(format, args));
            }

            public IDictionary<object, object> Memo {
                [PythonName("memo")]
                get { return memo; }
                [PythonName("memo")]
                set { memo = value; }
            }

            private object MemoGet(long key) {
                object value;
                if (memo.TryGetValue(key, out value)) return value;
                throw ExceptionConverter.CreateThrowable(BadPickleGet, String.Format("memo key {0} not found", key));
            }

            private void MemoPut(long key, object value) {
                memo[key] = value;
            }

            public int MarkIndex {
                get {
                    int i = stack.Count - 1;
                    while (i > 0 && stack[i] != mark) i -= 1;
                    if (i == -1) throw CannotUnpickle("mark not found");
                    return i;
                }
            }

            private string Read(int size) {
                return file.Read(size);
            }

            private string ReadLine() {
                return file.ReadLine();
            }

            private string ReadLineNoNewline() {
                string raw = file.ReadLine();
                return raw.Substring(0, raw.Length - 1);
            }

            private object ReadFloatString() {
                return FloatOps.Make(TypeCache.Double, ReadLineNoNewline());
            }

            private double ReadFloat64() {
                int index = 0;
                return PythonStruct.CreateDoubleValue(ref index, false, Read(8));
            }

            private object ReadIntFromString() {
                string raw = ReadLineNoNewline();
                if ("00" == raw) return Ops.FALSE;
                else if ("01" == raw) return Ops.TRUE;
                return IntOps.Make(TypeCache.Int32, raw);
            }

            private int ReadInt32() {
                int index = 0;
                return PythonStruct.CreateIntValue(ref index, true, Read(4));
            }

            private object ReadLongFromString() {
                return LongOps.Make(TypeCache.BigInteger, ReadLineNoNewline());
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

            [PythonName("find_global")]
            public object FindGlobal(ICallerContext context, object module, object attr) {
                object moduleObject;
                if (!Importer.TryGetExistingModule(context.SystemState, Converter.ConvertToString(module), out moduleObject)) {
                    moduleObject = Builtin.__import__(context, Converter.ConvertToString(module));
                }
                return Ops.GetAttr(context, moduleObject, SymbolTable.StringToId(Converter.ConvertToString(attr)));
            }

            private object MakeInstance(ICallerContext context, IPythonType cls, object[] args) {
                if (cls is OldClass) {
                    OldInstance inst = new OldInstance((OldClass)cls);
                    if (args.Length != 0 || Ops.HasAttr(context, cls, SymbolTable.GetInitArgs)) {
                        Ops.Call(Ops.GetAttr(context, inst, SymbolTable.Init), args);
                    }
                    return inst;
                }
                return Ops.Call(cls, args);
            }

            private void PopMark(int markIndex) {
                stack.DeleteSlice(markIndex, stack.Count);
            }

            /// <summary>
            /// Interpret everything from markIndex to the top of the stack as a sequence
            /// of key, value, key, value, etc. Set dict[key] = value for each. Pop
            /// everything from markIndex up when done.
            /// </summary>
            private void SetItems(Dict dict, int markIndex) {
                for (int i = markIndex + 1; i < stack.Count; i += 2) {
                    dict[stack[i]] = stack[i+1];
                }
                PopMark(markIndex);
            }

            private void LoadEof(ICallerContext context) {
                throw Ops.EofError("unexpected end of opcode stream");
            }

            private void LoadAppend(ICallerContext context) {
                object item = stack.Pop();
                object seq = stack[-1];
                if (seq is List) {
                    ((List)seq).Append(item);
                } else {
                    Ops.Call(Ops.GetAttr(context, seq, SymbolTable.Append), item);
                }
            }

            private void LoadAppends(ICallerContext context) {
                int markIndex = MarkIndex;
                object seq = stack[markIndex - 1];
                object stackSlice = stack.GetSlice(markIndex + 1, stack.Count);
                if (seq is List) {
                    ((List)seq).Extend(stackSlice);
                } else {
                    Ops.Call(Ops.GetAttr(context, seq, SymbolTable.Extend), stackSlice);
                }
                PopMark(markIndex);
            }

            private void LoadBinFloat(ICallerContext context) {
                stack.Append(ReadFloat64());
            }

            private void LoadBinGet(ICallerContext context) {
                stack.Append(MemoGet((long)ReadUInt8()));
            }

            private void LoadBinInt(ICallerContext context) {
                stack.Append(ReadInt32());
            }

            private void LoadBinInt1(ICallerContext context) {
                stack.Append((int)ReadUInt8());
            }

            private void LoadBinInt2(ICallerContext context) {
                stack.Append((int)ReadUInt16());
            }

            private void LoadBinPersid(ICallerContext context) {
                throw Ops.NotImplementedError("noload() is not implemented");
            }

            private void LoadBinPut(ICallerContext context) {
                MemoPut((long)ReadUInt8(), stack[-1]);
            }

            private void LoadBinString(ICallerContext context) {
                stack.Append(Read(ReadInt32()));
            }

            private void LoadBinUnicode(ICallerContext context) {
                stack.Append(StringOps.Decode(context, Read(ReadInt32()), "utf-8", "strict"));
            }

            private void LoadBuild(ICallerContext context) {
                object arg = stack.Pop();
                object inst = stack[-1];
                object setStateCallable;
                if (Ops.TryGetAttr(inst, SymbolTable.SetState, out setStateCallable)) {
                    Ops.Call(setStateCallable, arg);
                    return;
                }

                Dict dict;
                Dict slots;
                if (arg == null) {
                    dict = null;
                    slots = null;
                } else if (arg is Dict) {
                    dict = (Dict)arg;
                    slots = null;
                } else if (arg is Tuple) {
                    Tuple argsTuple = (Tuple)arg;
                    if (argsTuple.Count != 2) {
                        throw Ops.ValueError("state for object without __setstate__ must be None, dict, or 2-tuple");
                    }
                    dict = (Dict)argsTuple[0];
                    slots = (Dict)argsTuple[1];
                } else {
                    throw Ops.ValueError("state for object without __setstate__ must be None, dict, or 2-tuple");
                }

                if (dict != null) {
                    object instDict;
                    if (Ops.TryGetAttr(inst, SymbolTable.Dict, out instDict)) {
                        Dict realDict = instDict as Dict;
                        if (realDict != null) {
                            realDict.Update(arg);
                        } else {
                            object updateCallable;
                            if (Ops.TryGetAttr(instDict, SymbolTable.Update, out updateCallable)) {
                                Ops.Call(updateCallable, dict);
                            } else {
                                throw CannotUnpickle("could not update __dict__ {0} when building {1}", dict, inst);
                            }
                        }
                    }
                }

                if (slots != null) {
                    foreach(object key in slots) {
                        Ops.SetAttr(context, inst, SymbolTable.StringToId((string)key), slots[key]);
                    }
                }
            }

            private void LoadDict(ICallerContext context) {
                int markIndex = MarkIndex;
                Dict dict = new Dict((stack.Count - 1 - markIndex) / 2);
                SetItems(dict, markIndex);
                stack.Append(dict);
            }

            private void LoadDup(ICallerContext context) {
                stack.Append(stack[-1]);
            }

            private void LoadEmptyDict(ICallerContext context) {
                stack.Append(new Dict());
            }

            private void LoadEmptyList(ICallerContext context) {
                stack.Append(List.MakeList());
            }

            private void LoadEmptyTuple(ICallerContext context) {
                stack.Append(Tuple.MakeTuple());
            }

            private void LoadExt1(ICallerContext context) {
                Tuple global = (Tuple)PythonCopyReg.InvertedRegistry[(int)ReadUInt8()];
                stack.Append(FindGlobal(context, global[0], global[1]));
            }

            private void LoadExt2(ICallerContext context) {
                Tuple global = (Tuple)PythonCopyReg.InvertedRegistry[(int)ReadUInt16()];
                stack.Append(FindGlobal(context, global[0], global[1]));
            }

            private void LoadExt4(ICallerContext context) {
                Tuple global = (Tuple)PythonCopyReg.InvertedRegistry[ReadInt32()];
                stack.Append(FindGlobal(context, global[0], global[1]));
            }

            private void LoadFloat(ICallerContext context) {
                stack.Append(ReadFloatString());
            }

            private void LoadGet(ICallerContext context) {
                try {
                    stack.Append(MemoGet((long)(int)ReadIntFromString()));
                } catch (ArgumentException) {
                    throw ExceptionConverter.CreateThrowable(BadPickleGet, "while executing GET: invalid integer value");
                }
            }

            private void LoadGlobal(ICallerContext context) {
                string module = ReadLineNoNewline();
                string attr = ReadLineNoNewline();
                stack.Append(FindGlobal(context, module, attr));
            }

            private void LoadInst(ICallerContext context) {
                LoadGlobal(context);
                IPythonType cls = stack.Pop() as IPythonType;
                if (cls == null) {
                    throw Ops.TypeError("expected class or type after INST, got {0}", Ops.GetDynamicType(cls));
                }

                int markIndex = MarkIndex;
                object[] args = stack.GetSliceAsArray(markIndex + 1, stack.Count);
                PopMark(markIndex);

                stack.Append(MakeInstance(context, cls, args));
            }

            private void LoadInt(ICallerContext context) {
                stack.Append(ReadIntFromString());
            }

            private void LoadList(ICallerContext context) {
                int markIndex = MarkIndex;
                object list = stack.GetSlice(markIndex + 1, stack.Count);
                PopMark(markIndex);
                stack.Append(list);
            }

            private void LoadLong(ICallerContext context) {
                stack.Append(ReadLongFromString());
            }

            private void LoadLong1(ICallerContext context) {
                stack.Append(ReadLong(ReadUInt8()));
            }

            private void LoadLong4(ICallerContext context) {
                stack.Append(ReadLong(ReadInt32()));
            }

            private void LoadLongBinGet(ICallerContext context) {
                stack.Append(MemoGet((long)(int)ReadInt32()));
            }

            private void LoadLongBinPut(ICallerContext context) {
                MemoPut((long)(int)ReadInt32(), stack[-1]);
            }

            private void LoadMark(ICallerContext context) {
                stack.Append(mark);
            }

            private void LoadNewFalse(ICallerContext context) {
                stack.Append(Ops.FALSE);
            }

            private void LoadNewObj(ICallerContext context) {
                Tuple args = stack.Pop() as Tuple;
                if (args == null) {
                    throw Ops.TypeError("expected tuple as second argument to NEWOBJ, got {0}", Ops.GetDynamicType(args));
                }

                DynamicType cls = stack.Pop() as DynamicType;
                if (args == null) {
                    throw Ops.TypeError("expected new-style type as first argument to NEWOBJ, got {0}", Ops.GetDynamicType(args));
                }

                stack.Append(cls.CreateInstance(context, args.ToArray(), new string[0]));
            }

            private void LoadNewTrue(ICallerContext context) {
                stack.Append(Ops.TRUE);
            }

            private void LoadNoneValue(ICallerContext context) {
                stack.Append(NoneTypeOps.Instance);
            }

            private void LoadObj(ICallerContext context) {
                int markIndex = MarkIndex;
                IPythonType cls = stack[markIndex + 1] as IPythonType;
                if (cls == null) {
                    throw Ops.TypeError("expected class or type as first argument to INST, got {0}", Ops.GetDynamicType(cls));
                }
                object[] args = stack.GetSliceAsArray(markIndex + 2, stack.Count);
                PopMark(markIndex);
                stack.Append(MakeInstance(context, cls, args));
            }

            private void LoadPersId(ICallerContext context) {
                throw Ops.NotImplementedError("persistent id support is not implemented");
            }

            private void LoadPop(ICallerContext context) {
                stack.Pop();
            }

            private void LoadPopMark(ICallerContext context) {
                PopMark(MarkIndex);
            }

            private void LoadProto(ICallerContext context) {
                int proto = ReadUInt8();
                if (proto > 2) throw Ops.ValueError("unsupported pickle protocol: {0}", proto);
                // discard result
            }

            private void LoadPut(ICallerContext context) {
                MemoPut((long)(int)ReadIntFromString(), stack[-1]);
            }

            private void LoadReduce(ICallerContext context) {
                object args = stack.Pop();
                object callable = stack.Pop();
                if (args == null) {
                    stack.Append(Ops.Call(Ops.GetAttr(context, callable, SymbolTable.StringToId("__basicnew__"))));
                } else if (!Ops.GetDynamicType(args).Equals(TypeCache.Tuple)) {
                    throw Ops.TypeError(
                        "while executing REDUCE, expected tuple at the top of the stack, but got {0}",
                        Ops.GetDynamicType(args)
                    );
                }
                stack.Append(Ops.CallWithArgsTuple(callable, Ops.EMPTY, args));
            }

            private void LoadSetItem(ICallerContext context) {
                object value = stack.Pop();
                object key = stack.Pop();
                Dict dict = stack[-1] as Dict;
                if (dict == null) {
                    throw Ops.TypeError(
                        "while executing SETITEM, expected dict at stack[-3], but got {0}",
                        Ops.GetDynamicType(stack[-1])
                    );
                }
                dict[key] = value;
            }

            private void LoadSetItems(ICallerContext context) {
                int markIndex = MarkIndex;
                Dict dict = stack[markIndex - 1] as Dict;
                if (dict == null) {
                    throw Ops.TypeError(
                        "while executing SETITEMS, expected dict below last mark, but got {0}",
                        Ops.GetDynamicType(stack[markIndex - 1])
                    );
                }
                SetItems(dict, markIndex);
            }

            private void LoadShortBinstring(ICallerContext context) {
                stack.Append(Read(ReadUInt8()));
            }

            private void LoadString(ICallerContext context) {
                string repr = ReadLineNoNewline();
                if (repr.Length < 2 ||
                    !(
                    repr[0] == '"' && repr[repr.Length - 1] == '"' ||
                    repr[0] == '\'' && repr[repr.Length - 1] == '\''
                    )
                ) {
                    throw Ops.ValueError("while executing STRING, expected string that starts and ends with quotes");
                }
                stack.Append(StringOps.Decode(context, repr.Substring(1, repr.Length - 2), "string-escape", "strict"));
            }

            private void LoadTuple(ICallerContext context) {
                int markIndex = MarkIndex;
                Tuple tuple = Tuple.MakeTuple(stack.GetSliceAsArray(markIndex + 1, stack.Count));
                PopMark(markIndex);
                stack.Append(tuple);
            }

            private void LoadTuple1(ICallerContext context) {
                object item0 = stack.Pop();
                stack.Append(Tuple.MakeTuple(item0));
            }

            private void LoadTuple2(ICallerContext context) {
                object item1 = stack.Pop();
                object item0 = stack.Pop();
                stack.Append(Tuple.MakeTuple(item0, item1));
            }

            private void LoadTuple3(ICallerContext context) {
                object item2 = stack.Pop();
                object item1 = stack.Pop();
                object item0 = stack.Pop();
                stack.Append(Tuple.MakeTuple(item0, item1, item2));
            }

            private void LoadUnicode(ICallerContext context) {
                stack.Append(StringOps.Decode(context, ReadLineNoNewline(), "raw-unicode-escape", "strict"));
            }
        }

        #endregion

    }
}