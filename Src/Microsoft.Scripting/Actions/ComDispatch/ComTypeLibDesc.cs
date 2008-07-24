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

#if !SILVERLIGHT // ComObject

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.Actions.ComDispatch {
    using Ast = System.Linq.Expressions.Expression;

    public class ComTypeLibDesc : IOldDynamicObject {

        // typically typelibs contain very small number of coclasses
        // so we will just use the linked list as it performs better
        // on small number of entities
        private readonly LinkedList<ComTypeClassDesc> _classes;
        private readonly Dictionary<string, ComTypeEnumDesc> _enums;
        private string _typeLibName;
        private ComTypes.TYPELIBATTR _typeLibAttributes;

        private static readonly Dictionary<Guid, ComTypeLibDesc> _CachedTypeLibDesc = new Dictionary<Guid, ComTypeLibDesc>();

        private ComTypeLibDesc() {
            _enums = new Dictionary<string, ComTypeEnumDesc>();
            _classes = new LinkedList<ComTypeClassDesc>();
        }

        public override string ToString() {
            return String.Format("<type library {0}>", _typeLibName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string Documentation {
            get { return String.Empty; }
        }

        /// <summary>
        /// Reads the latest registered type library for the corresponding GUID,
        /// reads definitions of CoClass'es and Enum's from this library
        /// and creates a IOldDynamicObject that allows to instantiate coclasses
        /// and get actual values for the enums.
        /// </summary>
        /// <param name="typeLibGuid">Type Library Guid</param>
        /// <returns>ComTypeLibDesc object</returns>
        public static ComTypeLibInfo CreateFromGuid(Guid typeLibGuid) {
            // passing majorVersion = -1, minorVersion = -1 will always
            // load the latest typelib
            ComTypes.ITypeLib typeLib = ComRuntimeHelpers.UnsafeNativeMethods.LoadRegTypeLib(ref typeLibGuid, -1, -1, 0);

            return new ComTypeLibInfo(GetFromTypeLib(typeLib));
        }

        /// <summary>
        /// Gets an ITypeLib object from OLE Automation compatible RCW ,
        /// reads definitions of CoClass'es and Enum's from this library
        /// and creates a IOldDynamicObject that allows to instantiate coclasses
        /// and get actual values for the enums.
        /// </summary>
        /// <param name="rcw">OLE automation compatible RCW</param>
        /// <returns>ComTypeLibDesc object</returns>
        public static ComTypeLibInfo CreateFromObject(object rcw) {
            if (Marshal.IsComObject(rcw) == false) {
                throw new ArgumentException("COM object is expected");
            }

            ComTypes.ITypeInfo typeInfo = ComRuntimeHelpers.GetITypeInfoFromIDispatch(rcw as IDispatch, true);

            ComTypes.ITypeLib typeLib;
            int typeInfoIndex;
            typeInfo.GetContainingTypeLib(out typeLib, out typeInfoIndex);

            return new ComTypeLibInfo(GetFromTypeLib(typeLib));
        }

        internal static ComTypeLibDesc GetFromTypeLib(ComTypes.ITypeLib typeLib) {

            // check whether we have already loaded this type library
            ComTypes.TYPELIBATTR typeLibAttr = ComRuntimeHelpers.GetTypeAttrForTypeLib(typeLib);
            ComTypeLibDesc typeLibDesc;
            lock (_CachedTypeLibDesc) {
                if (_CachedTypeLibDesc.TryGetValue(typeLibAttr.guid, out typeLibDesc)) {
                    return typeLibDesc;
                }
            }

            typeLibDesc = new ComTypeLibDesc();

            typeLibDesc._typeLibName = ComRuntimeHelpers.GetNameOfLib(typeLib);
            typeLibDesc._typeLibAttributes = typeLibAttr;

            int countTypes = typeLib.GetTypeInfoCount();
            for (int i = 0; i < countTypes; i++) {
                ComTypes.TYPEKIND typeKind;
                typeLib.GetTypeInfoType(i, out typeKind);

                ComTypes.ITypeInfo typeInfo;
                if (typeKind == ComTypes.TYPEKIND.TKIND_COCLASS) {
                    typeLib.GetTypeInfo(i, out typeInfo);
                    ComTypeClassDesc classDesc = new ComTypeClassDesc(typeInfo, typeLibDesc);
                    typeLibDesc._classes.AddLast(classDesc);
                }
                else if (typeKind == ComTypes.TYPEKIND.TKIND_ENUM) {
                    typeLib.GetTypeInfo(i, out typeInfo);
                    ComTypeEnumDesc enumDesc = new ComTypeEnumDesc(typeInfo, typeLibDesc);
                    typeLibDesc._enums.Add(enumDesc.TypeName, enumDesc);
                }
            }

            // cached the typelib using the guid as the dictionary key
            lock (_CachedTypeLibDesc) {
                _CachedTypeLibDesc.Add(typeLibAttr.guid, typeLibDesc);
            }

            return typeLibDesc;
        }

        #region IOldDynamicObject Members

        RuleBuilder<T> IOldDynamicObject.GetRule<T>(OldDynamicAction action, CodeContext context, object[] args) {
            switch (action.Kind) {
                case DynamicActionKind.GetMember: return MakeGetMemberRule<T>((OldGetMemberAction)action, context);
                case DynamicActionKind.DoOperation: return MakeDoOperationRule<T>((OldDoOperationAction)action, context);
            }
            return null;
        }

        public object GetTypeLibObjectDesc(string member) {
            foreach( ComTypeClassDesc coclass in _classes) {
                if (member == coclass.TypeName) {
                    return coclass;
                }
            }

            ComTypeEnumDesc enumDesc;
            if (_enums != null && _enums.TryGetValue(member, out enumDesc) == true)
                return enumDesc;

            return null;
        }

        public string [] GetMemberNames() {

            string [] retval = new string[_enums.Count + _classes.Count];
            int i = 0;

            foreach (ComTypeClassDesc coclass in _classes) {
                retval[i++] = coclass.TypeName;
            }

            foreach (KeyValuePair<string, ComTypeEnumDesc> enumDesc in _enums) {
                retval[i++] = enumDesc.Key;
            }

            return retval;
        }

        private bool HasMember(string member) {
            foreach (ComTypeClassDesc coclass in _classes) {
                if (member == coclass.TypeName) {
                    return true;
                }
            }

            if (_enums.ContainsKey(member) == true)
                return true;

            return false;
        }

        public string Guid {
            get { return _typeLibAttributes.guid.ToString(); }
        }

        public short VersionMajor {
            get { return _typeLibAttributes.wMajorVerNum; }
        }

        public short VersionMinor {
            get { return _typeLibAttributes.wMinorVerNum; }
        }

        public string Name {
            get { return _typeLibName; }
        }

        private RuleBuilder<T> MakeGetMemberRule<T>(OldGetMemberAction action, CodeContext context) where T : class {

            string memberName = action.Name.ToString();
            RuleBuilder<T> rule = null;
            if (this.HasMember(memberName)) {
                ActionBinder binder = context.LanguageContext.Binder;

                rule = new RuleBuilder<T>();
                rule.MakeTest(CompilerHelpers.GetType(this));
                rule.AddTest(
                    Ast.Equal(
                        Ast.Property(
                            Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeLibDesc)),
                            this.GetType().GetProperty("Guid")),
                        Ast.Constant(this.Guid.ToString())));

                rule.Target = rule.MakeReturn(
                    binder,
                    Ast.Call(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeLibDesc)),
                        this.GetType().GetMethod("GetTypeLibObjectDesc"), Ast.Constant(action.Name.ToString())));
            }

            return rule;
        }

        private RuleBuilder<T> MakeDoOperationRule<T>(OldDoOperationAction action, CodeContext context) where T : class {
            if (action.Operation == Operators.GetMemberNames || action.Operation == Operators.MemberNames) {
                RuleBuilder<T> rule = new RuleBuilder<T>();
                rule.MakeTest(CompilerHelpers.GetType(this));
                rule.Target = rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeLibDesc)),
                        this.GetType().GetMethod("GetMemberNames")));

                return rule;
            }

            return null;

        }

        #endregion

        internal ComTypeClassDesc GetCoClassForInterface(string itfName) {
            foreach (ComTypeClassDesc coclass in _classes) {
                if (coclass.Implements(itfName, false)) {
                    return coclass;
                }
            }

            return null;
        }
    }
}

#endif
