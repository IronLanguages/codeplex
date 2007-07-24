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
using System.Reflection;

[assembly: AssemblyVersion("2.0.0.300")]

namespace IronPython {
    static class VersionInfo {
        public static string GetIronPythonAssembly(string baseName) {
#if SIGNED
            return baseName + ", Version=" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35";
#else
            return baseName;
#endif
        }
    }
}