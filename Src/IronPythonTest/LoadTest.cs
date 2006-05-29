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
using System.Collections.Generic;
using System.Text;

namespace IronPythonTest.LoadTest {

    public class Values {
        public static int GlobalName1 = 10;
        public static int GlobalName2 = 20;
        public static int NestedName1 = 30;
        public static int NestedName2 = 40;
    }

    public class Name1 {
        public static int Value = Values.GlobalName1;
    }

    public class Nested {
        public class Name1 {
            public static int Value = Values.NestedName1;
        }

        public class Name2 {
            public static int Value = Values.NestedName2;
        }
    }

    public class Name2 {
        public static int Value = Values.GlobalName2;
    }
}

public class NoNamespaceLoadTest {
    public string HelloWorld() {
        return "Hello World";
    }
}