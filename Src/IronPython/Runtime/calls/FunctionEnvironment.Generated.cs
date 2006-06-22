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
using System.Diagnostics;

namespace IronPython.Runtime.Calls {
    #region Generated environment types

    // *** BEGIN GENERATED CODE ***

    [PythonType(typeof(Dict))]
    public sealed class FunctionEnvironment2Dictionary : FunctionEnvironmentDictionary {
        [EnvironmentIndex(0)] public object value0;
        [EnvironmentIndex(1)] public object value1;

        public FunctionEnvironment2Dictionary(FunctionEnvironmentDictionary parent, IModuleEnvironment moduleScope, SymbolId[] names, SymbolId[] outer)
            : base(parent, moduleScope, names, outer) {
        }
        public override bool TrySetExtraValue(SymbolId key, object value) {
            if (names.Length >= 1) {
                if (names[0] == key) {
                    value0 = value;
                    return true;
                }
                if (names.Length >= 2) {
                    if (names[1] == key) {
                        value1 = value;
                        return true;
                    }
                }
            }
            return false;
        }
        public override bool TryGetExtraValue(SymbolId key, out object value) {
            if (names.Length >= 1) {
                if (names[0] == key) {
                    value = value0;
                    return true;
                }
                if (names.Length >= 2) {
                    if (names[1] == key) {
                        value = value1;
                        return true;
                    }
                }
            }
            return TryGetOuterValue(key, out value);
        }
        protected override object GetValueAtIndex(int index) {
            switch (index) {
                case 0: return value0;
                case 1: return value1;
                default: throw OutOfRange(index);
            }
        }
    }

    [PythonType(typeof(Dict))]
    public sealed class FunctionEnvironment4Dictionary : FunctionEnvironmentDictionary {
        [EnvironmentIndex(0)] public object value0;
        [EnvironmentIndex(1)] public object value1;
        [EnvironmentIndex(2)] public object value2;
        [EnvironmentIndex(3)] public object value3;

        public FunctionEnvironment4Dictionary(FunctionEnvironmentDictionary parent, IModuleEnvironment moduleScope, SymbolId[] names, SymbolId[] outer)
            : base(parent, moduleScope, names, outer) {
        }
        public override bool TrySetExtraValue(SymbolId key, object value) {
            if (names.Length >= 1) {
                if (names[0] == key) {
                    value0 = value;
                    return true;
                }
                if (names.Length >= 2) {
                    if (names[1] == key) {
                        value1 = value;
                        return true;
                    }
                    if (names.Length >= 3) {
                        if (names[2] == key) {
                            value2 = value;
                            return true;
                        }
                        if (names.Length >= 4) {
                            if (names[3] == key) {
                                value3 = value;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public override bool TryGetExtraValue(SymbolId key, out object value) {
            if (names.Length >= 1) {
                if (names[0] == key) {
                    value = value0;
                    return true;
                }
                if (names.Length >= 2) {
                    if (names[1] == key) {
                        value = value1;
                        return true;
                    }
                    if (names.Length >= 3) {
                        if (names[2] == key) {
                            value = value2;
                            return true;
                        }
                        if (names.Length >= 4) {
                            if (names[3] == key) {
                                value = value3;
                                return true;
                            }
                        }
                    }
                }
            }
            return TryGetOuterValue(key, out value);
        }
        protected override object GetValueAtIndex(int index) {
            switch (index) {
                case 0: return value0;
                case 1: return value1;
                case 2: return value2;
                case 3: return value3;
                default: throw OutOfRange(index);
            }
        }
    }

    [PythonType(typeof(Dict))]
    public sealed class FunctionEnvironment8Dictionary : FunctionEnvironmentDictionary {
        [EnvironmentIndex(0)] public object value0;
        [EnvironmentIndex(1)] public object value1;
        [EnvironmentIndex(2)] public object value2;
        [EnvironmentIndex(3)] public object value3;
        [EnvironmentIndex(4)] public object value4;
        [EnvironmentIndex(5)] public object value5;
        [EnvironmentIndex(6)] public object value6;
        [EnvironmentIndex(7)] public object value7;

        public FunctionEnvironment8Dictionary(FunctionEnvironmentDictionary parent, IModuleEnvironment moduleScope, SymbolId[] names, SymbolId[] outer)
            : base(parent, moduleScope, names, outer) {
        }
        public override bool TrySetExtraValue(SymbolId key, object value) {
            for (int i = 0; i < names.Length; i++) {
                if (names[i]== key) {
                    SetAtIndex(i, value);
                    return true;
                }
            }
            return false;
        }
        public override bool TryGetExtraValue(SymbolId key, out object value) {
            for (int index = 0; index < names.Length; index++) {
                if (names[index] == key) {
                    value = GetAtIndex(index);
                    return true;
                }
            }
            return TryGetOuterValue(key, out value);
        }
        protected override object GetValueAtIndex(int index) {
            return GetAtIndex(index);
        }
        private object GetAtIndex(int index) {
            switch (index) {
                case 0: return value0;
                case 1: return value1;
                case 2: return value2;
                case 3: return value3;
                case 4: return value4;
                case 5: return value5;
                case 6: return value6;
                case 7: return value7;
                default: throw OutOfRange(index);
            }
        }
        private void SetAtIndex(int index, object value) {
            switch (index) {
                case 0: value0 = value; break;
                case 1: value1 = value; break;
                case 2: value2 = value; break;
                case 3: value3 = value; break;
                case 4: value4 = value; break;
                case 5: value5 = value; break;
                case 6: value6 = value; break;
                case 7: value7 = value; break;
                default: throw OutOfRange(index);
            }
        }
    }

    [PythonType(typeof(Dict))]
    public sealed class FunctionEnvironment16Dictionary : FunctionEnvironmentDictionary {
        [EnvironmentIndex(0)] public object value0;
        [EnvironmentIndex(1)] public object value1;
        [EnvironmentIndex(2)] public object value2;
        [EnvironmentIndex(3)] public object value3;
        [EnvironmentIndex(4)] public object value4;
        [EnvironmentIndex(5)] public object value5;
        [EnvironmentIndex(6)] public object value6;
        [EnvironmentIndex(7)] public object value7;
        [EnvironmentIndex(8)] public object value8;
        [EnvironmentIndex(9)] public object value9;
        [EnvironmentIndex(10)] public object value10;
        [EnvironmentIndex(11)] public object value11;
        [EnvironmentIndex(12)] public object value12;
        [EnvironmentIndex(13)] public object value13;
        [EnvironmentIndex(14)] public object value14;
        [EnvironmentIndex(15)] public object value15;

        public FunctionEnvironment16Dictionary(FunctionEnvironmentDictionary parent, IModuleEnvironment moduleScope, SymbolId[] names, SymbolId[] outer)
            : base(parent, moduleScope, names, outer) {
        }
        public override bool TrySetExtraValue(SymbolId key, object value) {
            for (int i = 0; i < names.Length; i++) {
                if (names[i]== key) {
                    SetAtIndex(i, value);
                    return true;
                }
            }
            return false;
        }
        public override bool TryGetExtraValue(SymbolId key, out object value) {
            for (int index = 0; index < names.Length; index++) {
                if (names[index] == key) {
                    value = GetAtIndex(index);
                    return true;
                }
            }
            return TryGetOuterValue(key, out value);
        }
        protected override object GetValueAtIndex(int index) {
            return GetAtIndex(index);
        }
        private object GetAtIndex(int index) {
            switch (index) {
                case 0: return value0;
                case 1: return value1;
                case 2: return value2;
                case 3: return value3;
                case 4: return value4;
                case 5: return value5;
                case 6: return value6;
                case 7: return value7;
                case 8: return value8;
                case 9: return value9;
                case 10: return value10;
                case 11: return value11;
                case 12: return value12;
                case 13: return value13;
                case 14: return value14;
                case 15: return value15;
                default: throw OutOfRange(index);
            }
        }
        private void SetAtIndex(int index, object value) {
            switch (index) {
                case 0: value0 = value; break;
                case 1: value1 = value; break;
                case 2: value2 = value; break;
                case 3: value3 = value; break;
                case 4: value4 = value; break;
                case 5: value5 = value; break;
                case 6: value6 = value; break;
                case 7: value7 = value; break;
                case 8: value8 = value; break;
                case 9: value9 = value; break;
                case 10: value10 = value; break;
                case 11: value11 = value; break;
                case 12: value12 = value; break;
                case 13: value13 = value; break;
                case 14: value14 = value; break;
                case 15: value15 = value; break;
                default: throw OutOfRange(index);
            }
        }
    }

    [PythonType(typeof(Dict))]
    public sealed class FunctionEnvironment32Dictionary : FunctionEnvironmentDictionary {
        [EnvironmentIndex(0)] public object value0;
        [EnvironmentIndex(1)] public object value1;
        [EnvironmentIndex(2)] public object value2;
        [EnvironmentIndex(3)] public object value3;
        [EnvironmentIndex(4)] public object value4;
        [EnvironmentIndex(5)] public object value5;
        [EnvironmentIndex(6)] public object value6;
        [EnvironmentIndex(7)] public object value7;
        [EnvironmentIndex(8)] public object value8;
        [EnvironmentIndex(9)] public object value9;
        [EnvironmentIndex(10)] public object value10;
        [EnvironmentIndex(11)] public object value11;
        [EnvironmentIndex(12)] public object value12;
        [EnvironmentIndex(13)] public object value13;
        [EnvironmentIndex(14)] public object value14;
        [EnvironmentIndex(15)] public object value15;
        [EnvironmentIndex(16)] public object value16;
        [EnvironmentIndex(17)] public object value17;
        [EnvironmentIndex(18)] public object value18;
        [EnvironmentIndex(19)] public object value19;
        [EnvironmentIndex(20)] public object value20;
        [EnvironmentIndex(21)] public object value21;
        [EnvironmentIndex(22)] public object value22;
        [EnvironmentIndex(23)] public object value23;
        [EnvironmentIndex(24)] public object value24;
        [EnvironmentIndex(25)] public object value25;
        [EnvironmentIndex(26)] public object value26;
        [EnvironmentIndex(27)] public object value27;
        [EnvironmentIndex(28)] public object value28;
        [EnvironmentIndex(29)] public object value29;
        [EnvironmentIndex(30)] public object value30;
        [EnvironmentIndex(31)] public object value31;

        public FunctionEnvironment32Dictionary(FunctionEnvironmentDictionary parent, IModuleEnvironment moduleScope, SymbolId[] names, SymbolId[] outer)
            : base(parent, moduleScope, names, outer) {
        }
        public override bool TrySetExtraValue(SymbolId key, object value) {
            for (int i = 0; i < names.Length; i++) {
                if (names[i]== key) {
                    SetAtIndex(i, value);
                    return true;
                }
            }
            return false;
        }
        public override bool TryGetExtraValue(SymbolId key, out object value) {
            for (int index = 0; index < names.Length; index++) {
                if (names[index] == key) {
                    value = GetAtIndex(index);
                    return true;
                }
            }
            return TryGetOuterValue(key, out value);
        }
        protected override object GetValueAtIndex(int index) {
            return GetAtIndex(index);
        }
        private object GetAtIndex(int index) {
            switch (index) {
                case 0: return value0;
                case 1: return value1;
                case 2: return value2;
                case 3: return value3;
                case 4: return value4;
                case 5: return value5;
                case 6: return value6;
                case 7: return value7;
                case 8: return value8;
                case 9: return value9;
                case 10: return value10;
                case 11: return value11;
                case 12: return value12;
                case 13: return value13;
                case 14: return value14;
                case 15: return value15;
                case 16: return value16;
                case 17: return value17;
                case 18: return value18;
                case 19: return value19;
                case 20: return value20;
                case 21: return value21;
                case 22: return value22;
                case 23: return value23;
                case 24: return value24;
                case 25: return value25;
                case 26: return value26;
                case 27: return value27;
                case 28: return value28;
                case 29: return value29;
                case 30: return value30;
                case 31: return value31;
                default: throw OutOfRange(index);
            }
        }
        private void SetAtIndex(int index, object value) {
            switch (index) {
                case 0: value0 = value; break;
                case 1: value1 = value; break;
                case 2: value2 = value; break;
                case 3: value3 = value; break;
                case 4: value4 = value; break;
                case 5: value5 = value; break;
                case 6: value6 = value; break;
                case 7: value7 = value; break;
                case 8: value8 = value; break;
                case 9: value9 = value; break;
                case 10: value10 = value; break;
                case 11: value11 = value; break;
                case 12: value12 = value; break;
                case 13: value13 = value; break;
                case 14: value14 = value; break;
                case 15: value15 = value; break;
                case 16: value16 = value; break;
                case 17: value17 = value; break;
                case 18: value18 = value; break;
                case 19: value19 = value; break;
                case 20: value20 = value; break;
                case 21: value21 = value; break;
                case 22: value22 = value; break;
                case 23: value23 = value; break;
                case 24: value24 = value; break;
                case 25: value25 = value; break;
                case 26: value26 = value; break;
                case 27: value27 = value; break;
                case 28: value28 = value; break;
                case 29: value29 = value; break;
                case 30: value30 = value; break;
                case 31: value31 = value; break;
                default: throw OutOfRange(index);
            }
        }
    }

    // *** END GENERATED CODE ***

    #endregion
}
