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
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.ComDispatch {
    using Ast = System.Linq.Expressions.Expression;

    public class ComTypeLibInfo : IOldDynamicObject  {
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

        #region IOldDynamicObject Members

        public RuleBuilder<T> GetRule<T>(OldDynamicAction action, CodeContext context, object[] args) where T : class {
            switch (action.Kind) {
                case DynamicActionKind.GetMember: return MakeGetMemberRule<T>((OldGetMemberAction)action, context);
                case DynamicActionKind.DoOperation: return MakeDoOperationRule<T>((OldDoOperationAction)action, context);
            }
            return null;
        }

        private RuleBuilder<T> MakeGetMemberRule<T>(OldGetMemberAction action, CodeContext context) where T : class {

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
                Ast.Property(
                    Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeLibInfo)),
                    this.GetType().GetProperty(memberName)));

            return rule;
        }

        private RuleBuilder<T> MakeDoOperationRule<T>(OldDoOperationAction action, CodeContext context) where T : class {
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
