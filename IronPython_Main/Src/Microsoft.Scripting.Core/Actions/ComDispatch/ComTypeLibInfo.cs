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
    using System;
    using Microsoft.Scripting.Generation;
    using Microsoft.Scripting.Runtime;
    using Ast = Microsoft.Scripting.Ast.Expression;

    public class ComTypeLibInfo : IDynamicObject  {
        private readonly ComTypeLibDesc _typeLibDesc;

        internal ComTypeLibInfo(ComTypeLibDesc typeLibDesc) {
            _typeLibDesc = typeLibDesc;
        }

        public string Name {
            get { return _typeLibDesc.Name; }
        }

        public Guid Guid {
            get { return new Guid(_typeLibDesc.Guid); }
        }

        public short VersionMajor {
            get { return _typeLibDesc.VersionMajor; }
        }

        public short VersionMinor {
            get { return _typeLibDesc.VersionMinor; }
        }

        public ComTypeLibDesc TypeLibDesc {
            get { return _typeLibDesc; }
        }

        public string[] GetMemberNames() {
            return new string[] { this.Name, "Guid", "Name", "VersionMajor", "VersionMinor" };
        }

        #region IDynamicObject Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public LanguageContext LanguageContext {
            get { throw new NotImplementedException(); }
        }

        public RuleBuilder<T> GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            switch (action.Kind) {
                case DynamicActionKind.GetMember: return MakeGetMemberRule<T>((GetMemberAction)action, context);
                case DynamicActionKind.DoOperation: return MakeDoOperationRule<T>((DoOperationAction)action, context);
            }
            return null;
        }

        private RuleBuilder<T> MakeGetMemberRule<T>(GetMemberAction action, CodeContext context) {

            string memberName = action.Name.ToString();
            RuleBuilder<T> rule = null;
            if (memberName == _typeLibDesc.Name) {
                memberName = "TypeLibDesc";
            } else if (memberName != "Guid" &&
                memberName != "Name" &&
                memberName != "VersionMajor" &&
                memberName != "VersionMinor")
            {
                return null;
            }
            
            ActionBinder binder = context.LanguageContext.Binder;

            rule = new RuleBuilder<T>();
            rule.MakeTest(CompilerHelpers.GetType(this));
            rule.Target = rule.MakeReturn(
                binder,
                Ast.ReadProperty(
                    Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeLibInfo)),
                    this.GetType().GetProperty(memberName)));

            return rule;
        }

        private RuleBuilder<T> MakeDoOperationRule<T>(DoOperationAction action, CodeContext context) {
            if (action.Operation == Operators.GetMemberNames || action.Operation == Operators.MemberNames) {
                RuleBuilder<T> rule = new RuleBuilder<T>();
                rule.MakeTest(CompilerHelpers.GetType(this));
                rule.Target = rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeLibInfo)),
                        this.GetType().GetMethod("GetMemberNames")));

                return rule;
            }

            return null;

        }

        #endregion
    }
}

#endif
