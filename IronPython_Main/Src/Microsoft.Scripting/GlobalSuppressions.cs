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

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Scope = "member", Target = "Microsoft.Scripting.SymbolTable..cctor()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope="member", Target="Microsoft.Scripting.SymbolId.Id")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Scope = "member", Target = "Microsoft.Scripting.CallTarget5.Invoke(System.Object,System.Object,System.Object,System.Object,System.Object):System.Object")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Scope = "member", Target = "Microsoft.Scripting.CallTarget4.Invoke(System.Object,System.Object,System.Object,System.Object):System.Object")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Scope = "member", Target = "Microsoft.Scripting.IAttributesCollection.Item[Microsoft.Scripting.SymbolId]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Scope = "member", Target = "Microsoft.Scripting.DynamicTypeBuilder..ctor(System.Type)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "Microsoft.Scripting.Internal.ConsoleOptions.RemainingArgs")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily", Scope = "member", Target = "Microsoft.Scripting.Internal.SuperConsole..ctor(Microsoft.Scripting.Internal.Hosting.ScriptEngine,System.Boolean)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "Microsoft.Scripting.Internal.CommandLine.Arguments")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "Microsoft.Scripting.Internal.Generation.CodeGen.EmitArray(System.Int32,Microsoft.Scripting.Internal.Generation.EmitArrayHelper):System.Void")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "Microsoft.Scripting.Internal.Generation.CodeGen.EmitArray(System.Collections.Generic.IList`1<T>):System.Void")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Scope = "member", Target = "Microsoft.Scripting.Internal.Generation.CodeGen..ctor(Microsoft.Scripting.Internal.Generation.TypeGen,System.Reflection.MethodBase,System.Reflection.Emit.ILGenerator,System.Type[])")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Scope = "member", Target = "Microsoft.Scripting.Internal.Generation.AssemblyGen.CreateCodeGen(Microsoft.Scripting.Internal.Generation.TypeGen,System.Reflection.MethodBase,System.Reflection.Emit.ILGenerator,System.Type[]):Microsoft.Scripting.Internal.Generation.CodeGen")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Scripting.Internal.Ast")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Scripting.Internal.Hosting")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Scope = "member", Target = "Microsoft.Scripting.Internal.Generation.LexicalScope.Item[Microsoft.Scripting.SymbolId]")]
