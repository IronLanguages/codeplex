/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System;

#if SILVERLIGHT
// InternalsVisibleTo requires the full public key--it won't work with the public key token.
[assembly: InternalsVisibleTo("IronPython.Modules")][assembly: InternalsVisibleTo("IronPythonTest, PublicKey=00240000048000009400000006020000002400005253413100040000010001000fc5993e0f511ad5e16e8b226553493e09067afc41039f70daeb94a968d664f40e69a46b617d15d3d5328be7dbedd059eb98495a3b03cb4ea4ba127444671c3c84cbc1fdc393d7e10b5ee3f31f5a29f005e5eed7e3c9c8af74f413f0004f0c2cabb22f9dd4f75a6f599784e1bab70985ef8174ca6c684278be82ce055a03ebaf")]
#elif SIGNED
[assembly: InternalsVisibleTo("IronPython.Modules")][assembly: InternalsVisibleTo("IronPythonTest, PublicKey=00240000048000009400000006020000002400005253413100040000010001000fc5993e0f511ad5e16e8b226553493e09067afc41039f70daeb94a968d664f40e69a46b617d15d3d5328be7dbedd059eb98495a3b03cb4ea4ba127444671c3c84cbc1fdc393d7e10b5ee3f31f5a29f005e5eed7e3c9c8af74f413f0004f0c2cabb22f9dd4f75a6f599784e1bab70985ef8174ca6c684278be82ce055a03ebaf")]
[assembly: AllowPartiallyTrustedCallers]
#else
[assembly: InternalsVisibleTo("IronPython.Modules")]
[assembly: InternalsVisibleTo("IronPythonTest")]
[assembly: AllowPartiallyTrustedCallers]
#endif
