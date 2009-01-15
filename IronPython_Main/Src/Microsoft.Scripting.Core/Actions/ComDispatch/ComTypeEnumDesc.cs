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

namespace Microsoft.Scripting.Actions.ComDispatch {
    using System; using Microsoft;
    using System.Runtime.InteropServices;
    using Microsoft.Scripting.Generation;
    using Microsoft.Scripting.Runtime;
    using Ast = Microsoft.Scripting.Ast.Expression;
    using ComTypes = System.Runtime.InteropServices.ComTypes;

    public class ComTypeEnumDesc : ComTypeDesc, IDynamicObject {

        readonly string[] _memberNames;
        readonly object[] _memberValues;

        public override string ToString() {
            return String.Format("<enum '{0}'>", TypeName);
        }

        internal ComTypeEnumDesc(ComTypes.ITypeInfo typeInfo, ComTypeLibDesc typeLibDesc) :
            base(typeInfo, ComType.Enum, typeLibDesc) {
            ComTypes.TYPEATTR typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);
            string[] memberNames = new string[typeAttr.cVars];
            object[] memberValues = new object[typeAttr.cVars];

            IntPtr p = IntPtr.Zero;

            // For each enum member get name and value.
            for (int i = 0; i < typeAttr.cVars; i++) {
                typeInfo.GetVarDesc(i, out p);

                // Get the enum member value (as object).
                ComTypes.VARDESC varDesc;

                try {
                    varDesc = (ComTypes.VARDESC)Marshal.PtrToStructure(p, typeof(ComTypes.VARDESC));

                    if (varDesc.varkind == ComTypes.VARKIND.VAR_CONST) {
                        memberValues[i] = Marshal.GetObjectForNativeVariant(varDesc.desc.lpvarValue);
                    }

                } finally {
                    typeInfo.ReleaseVarDesc(p);
                }

                // Get the enum member name
                memberNames[i] = ComRuntimeHelpers.GetNameOfMethod(typeInfo, varDesc.memid);
            }

            _memberNames = memberNames;
            _memberValues = memberValues;
        }

        #region IDynamicObject Members

        // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public LanguageContext LanguageContext {
            get { throw new NotImplementedException(); }
        }

        public object GetValue(string enumValueName) {
            for (int i = 0; i < _memberNames.Length; i++) {
                if (_memberNames[i] == enumValueName)
                    return _memberValues[i];
            }

            throw new MissingMemberException(enumValueName);
        }

        private bool HasMember(string name) {
            for (int i = 0; i < _memberNames.Length; i++) {
                if (_memberNames[i] == name)
                    return true;
            }

            return false;
        }

        public string[] GetMemberNames() {
            return (string[])this._memberNames.Clone();
        }

        public RuleBuilder<T> GetRule<T>(DynamicAction action, CodeContext context, object[] args) where T : class {
            switch (action.Kind) {
                case DynamicActionKind.GetMember: return MakeGetMemberRule<T>((GetMemberAction)action, context);
                case DynamicActionKind.DoOperation: return MakeDoOperationRule<T>((DoOperationAction)action, context);
            }
            return null;
        }

        private void MakeInstanceTestHelper<T>(RuleBuilder<T> rule) where T : class {
            rule.MakeTest(CompilerHelpers.GetType(this));
            rule.AddTest(
                Ast.Equal(
                    Ast.ReadProperty(
                        Ast.ReadProperty(
                            Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeEnumDesc)),
                            typeof(ComTypeDesc).GetProperty("TypeLib")),
                        typeof(ComTypeLibDesc).GetProperty("Guid")),
                    Ast.Constant(this.TypeLib.Guid)));
            rule.AddTest(
                Ast.Equal(
                    Ast.ReadProperty(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeEnumDesc)),
                        this.GetType().GetProperty("TypeName")),
                    Ast.Constant(this.TypeName)));
        }


        private RuleBuilder<T> MakeGetMemberRule<T>(GetMemberAction action, CodeContext context) where T : class {

            RuleBuilder<T> rule = null;

            if (this.HasMember(((GetMemberAction)action).Name.ToString())) {
                rule = new RuleBuilder<T>();

                // Sample test:
                // (.bound $arg0).GetType() == ComTypeLibEnumDesc && 
                // (.bound $arg0).TypeLib.Guid == "00020813-0000-0000-c000-000000000046" && 
                // (.bound $arg0).TypeName == "XlFileFormat"
                MakeInstanceTestHelper(rule);

                ActionBinder binder = context.LanguageContext.Binder;

                // Sample target:
                // return (.bound $arg0).GetValue("xlAddIn")
                rule.Target = rule.MakeReturn(
                    binder,
                    Ast.Call(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeEnumDesc)),
                        this.GetType().GetMethod("GetValue"),
                        Ast.Constant(((GetMemberAction)action).Name.ToString())));
            }

            return rule;
        }

        private RuleBuilder<T> MakeDoOperationRule<T>(DoOperationAction action, CodeContext context) where T : class {
            if (action.Operation == Operators.GetMemberNames || action.Operation == Operators.MemberNames) {
                // Sample test:
                // (.bound $arg0).GetType() == ComTypeLibEnumDesc && 
                // (.bound $arg0).TypeLib.Guid == "00020813-0000-0000-c000-000000000046" && 
                // (.bound $arg0).GetEnumTypeName() == "XlFileFormat"

                RuleBuilder<T> rule = new RuleBuilder<T>();

                MakeInstanceTestHelper(rule);

                // return (.bound $arg0).GetMemberNames()
                rule.Target = rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeEnumDesc)),
                        this.GetType().GetMethod("GetMemberNames")));

                return rule;
            }

            return null;
        }
        #endregion
    }
}

#endif
