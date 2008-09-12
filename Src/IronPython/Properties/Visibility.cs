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
using Microsoft.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System; using Microsoft;

#if SILVERLIGHT
// InternalsVisibleTo requires the full public key--it won't work with the public key token.
[assembly: InternalsVisibleTo("IronPython.Modules")][assembly: InternalsVisibleTo("IronPythonTest")]
#elif SIGNED
[assembly: InternalsVisibleTo("IronPython.Modules")][assembly: InternalsVisibleTo("IronPythonTest")][assembly: AllowPartiallyTrustedCallers]
#else
[assembly: InternalsVisibleTo("IronPython.Modules")]
[assembly: InternalsVisibleTo("IronPythonTest")]
[assembly: AllowPartiallyTrustedCallers]
#endif
