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
using IronPython.Hosting;

public class Eval {
    public static void Main(string[] args) {
        PythonEngine pe = new PythonEngine();
        if (args.Length > 0) {
            try {
                Console.WriteLine(pe.Evaluate(args[0]));
            } catch {
                Console.WriteLine("Error");
            }
        } else Console.WriteLine("eval <expression>");
    }
}