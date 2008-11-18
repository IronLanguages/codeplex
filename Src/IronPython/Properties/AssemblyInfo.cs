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

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("IronPython")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("IronPython")]
[assembly: AssemblyCopyright("� Microsoft Corporation.  All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("68e40495-c34a-4539-b43e-9e4e6f11a9fb")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

#if !SILVERLIGHT
[assembly: AssemblyFileVersion("2.0.0.00")]
[assembly: AssemblyInformationalVersion("2.0")]
#endif
[assembly: SecurityTransparent]
[assembly: CLSCompliant(false)]

// hack in order to pass AssemblyInfoTask 
// AssemblyVersion attribute has been decorated in AssemblyVersion.cs
#if FALSE
[assembly: AssemblyVersion("2.0.0.00")]
#endif
