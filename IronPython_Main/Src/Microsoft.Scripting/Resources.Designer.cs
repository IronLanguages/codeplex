//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Scripting {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Scripting.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ambigious module resolution, could be {0} or {1}.
        /// </summary>
        internal static string AmbigiousModule {
            get {
                return ResourceManager.GetString("AmbigiousModule", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An expression cannot be produced because the method binding was unsuccessful..
        /// </summary>
        internal static string BindingTarget_BindingFailed {
            get {
                return ResourceManager.GetString("BindingTarget_BindingFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to can&apos;t add another casing for identifier {0}.
        /// </summary>
        internal static string CantAddCasing {
            get {
                return ResourceManager.GetString("CantAddCasing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to can&apos;t add new identifier {0}.
        /// </summary>
        internal static string CantAddIdentifier {
            get {
                return ResourceManager.GetString("CantAddIdentifier", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to can&apos;t perform deletion.
        /// </summary>
        internal static string CantDelete {
            get {
                return ResourceManager.GetString("CantDelete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to can&apos;t read from property.
        /// </summary>
        internal static string CantReadProperty {
            get {
                return ResourceManager.GetString("CantReadProperty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to can&apos;t write to property.
        /// </summary>
        internal static string CantWriteProperty {
            get {
                return ResourceManager.GetString("CantWriteProperty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ... .
        /// </summary>
        internal static string ConsoleContinuePrompt {
            get {
                return ResourceManager.GetString("ConsoleContinuePrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &gt;&gt;&gt; .
        /// </summary>
        internal static string ConsolePrompt {
            get {
                return ResourceManager.GetString("ConsolePrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to empty SymbolId, expected valid SymbolId.
        /// </summary>
        internal static string EmptySymbolId {
            get {
                return ResourceManager.GetString("EmptySymbolId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Expected SymbolId, got SymbolId.Empty.
        /// </summary>
        internal static string EmptySymbolId1 {
            get {
                return ResourceManager.GetString("EmptySymbolId1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Environment variables.
        /// </summary>
        internal static string EnvironmentVariables {
            get {
                return ResourceManager.GetString("EnvironmentVariables", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot create instance of {0} because it contains generic parameters.
        /// </summary>
        internal static string IllegalNew_GenericParams {
            get {
                return ResourceManager.GetString("IllegalNew_GenericParams", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to cannot modify immutable type.
        /// </summary>
        internal static string ImmutableType {
            get {
                return ResourceManager.GetString("ImmutableType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type &apos;{0}&apos; doesn&apos;t provide a suitable public constructor or its implementation is faulty..
        /// </summary>
        internal static string InvalidCtorImplementation {
            get {
                return ResourceManager.GetString("InvalidCtorImplementation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot access member {1} declared on type {0} because the type contains generic parameters..
        /// </summary>
        internal static string InvalidOperation_ContainsGenericParameters {
            get {
                return ResourceManager.GetString("InvalidOperation_ContainsGenericParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempt to define parameter on non-methodbuilder and non-dynamic method.
        /// </summary>
        internal static string InvalidOperation_DefineParameterBakedMethod {
            get {
                return ResourceManager.GetString("InvalidOperation_DefineParameterBakedMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to setting this.
        /// </summary>
        internal static string InvalidOperation_SetThis {
            get {
                return ResourceManager.GetString("InvalidOperation_SetThis", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to emitting this in static method.
        /// </summary>
        internal static string InvalidOperation_ThisInStaticMethod {
            get {
                return ResourceManager.GetString("InvalidOperation_ThisInStaticMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to too many arguments.
        /// </summary>
        internal static string InvalidOperation_TooManyArguments {
            get {
                return ResourceManager.GetString("InvalidOperation_TooManyArguments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to unexpected type, expected {0}, got {1}.
        /// </summary>
        internal static string InvalidOperation_UnexpectedType {
            get {
                return ResourceManager.GetString("InvalidOperation_UnexpectedType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; is not a valid value for option &apos;{1}&apos;..
        /// </summary>
        internal static string InvalidOptionValue {
            get {
                return ResourceManager.GetString("InvalidOptionValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Argument expected for the {0} option..
        /// </summary>
        internal static string MissingOptionValue {
            get {
                return ResourceManager.GetString("MissingOptionValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Language &apos;{0}&apos; doesn&apos;t provide service {1}..
        /// </summary>
        internal static string MissingService {
            get {
                return ResourceManager.GetString("MissingService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Language &apos;{0}&apos; doesn&apos;t provide a command line..
        /// </summary>
        internal static string MissingService_CommandLine {
            get {
                return ResourceManager.GetString("MissingService_CommandLine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Language &apos;{0}&apos; doesn&apos;t provide an engine..
        /// </summary>
        internal static string MissingService_Engine {
            get {
                return ResourceManager.GetString("MissingService_Engine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Language &apos;{0}&apos; doesn&apos;t provide a scanner..
        /// </summary>
        internal static string MissingService_GetScanner {
            get {
                return ResourceManager.GetString("MissingService_GetScanner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Language &apos;{0}&apos; doesn&apos;t provide an options parser..
        /// </summary>
        internal static string MissingService_OptionsParser {
            get {
                return ResourceManager.GetString("MissingService_OptionsParser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Language &apos;{0}&apos; doesn&apos;t provide a token categorizer..
        /// </summary>
        internal static string MissingService_TokenCategorizer {
            get {
                return ResourceManager.GetString("MissingService_TokenCategorizer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type &apos;{0}&apos; is missing or cannot be loaded..
        /// </summary>
        internal static string MissingType {
            get {
                return ResourceManager.GetString("MissingType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to name &apos;{0}&apos; not defined.
        /// </summary>
        internal static string NameNotDefined {
            get {
                return ResourceManager.GetString("NameNotDefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method or operation is not implemented..
        /// </summary>
        internal static string NotImplemented {
            get {
                return ResourceManager.GetString("NotImplemented", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Assign: {0}.
        /// </summary>
        internal static string NotImplemented_Assign {
            get {
                return ResourceManager.GetString("NotImplemented_Assign", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to cannot emit raw enum value for type {0} with value {1}.
        /// </summary>
        internal static string NotImplemented_EnumEmit {
            get {
                return ResourceManager.GetString("NotImplemented_EnumEmit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Evaluate: {0}.
        /// </summary>
        internal static string NotImplemented_Evaluate {
            get {
                return ResourceManager.GetString("NotImplemented_Evaluate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to execute: {0}.
        /// </summary>
        internal static string NotImplemented_Execute {
            get {
                return ResourceManager.GetString("NotImplemented_Execute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Options.
        /// </summary>
        internal static string Options {
            get {
                return ResourceManager.GetString("Options", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot compose module of remotely compiled code..
        /// </summary>
        internal static string RemoteCodeModuleComposition {
            get {
                return ResourceManager.GetString("RemoteCodeModuleComposition", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot use a remote instance..
        /// </summary>
        internal static string RemoteInstanceMisused {
            get {
                return ResourceManager.GetString("RemoteInstanceMisused", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to static property &quot;{0}&quot; of &quot;{1}&quot; can only be read through a type, not an instance.
        /// </summary>
        internal static string StaticAccessFromInstanceError {
            get {
                return ResourceManager.GetString("StaticAccessFromInstanceError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to static property &quot;{0}&quot; of &quot;{1}&quot; can only be assigned to through a type, not an instance.
        /// </summary>
        internal static string StaticAssignmentFromInstanceError {
            get {
                return ResourceManager.GetString("StaticAssignmentFromInstanceError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;type must be visible ({0})&quot;.
        /// </summary>
        internal static string TypeMustBeVisible {
            get {
                return ResourceManager.GetString("TypeMustBeVisible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unhandled exception.
        /// </summary>
        internal static string UnhandledException {
            get {
                return ResourceManager.GetString("UnhandledException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unit is not visible to a debugger.
        /// </summary>
        internal static string Unit_NotDebuggerVisible {
            get {
                return ResourceManager.GetString("Unit_NotDebuggerVisible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown language identifier..
        /// </summary>
        internal static string UnknownLanguageId {
            get {
                return ResourceManager.GetString("UnknownLanguageId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specified language provider type is not registered..
        /// </summary>
        internal static string UnknownLanguageProviderType {
            get {
                return ResourceManager.GetString("UnknownLanguageProviderType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to could not load module with base name {0}.
        /// </summary>
        internal static string UnknownModule {
            get {
                return ResourceManager.GetString("UnknownModule", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage.
        /// </summary>
        internal static string Usage {
            get {
                return ResourceManager.GetString("Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Non-verifiable assembly generated: {0}:\nAssembly preserved as {1}\nError text:\n{2}\n.
        /// </summary>
        internal static string VerificationException {
            get {
                return ResourceManager.GetString("VerificationException", resourceCulture);
            }
        }
    }
}
