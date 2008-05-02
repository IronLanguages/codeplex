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

using System;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using System.Collections.Generic;
using Microsoft.Scripting;

namespace IronPython.Runtime.Types {
    using Ast = Microsoft.Scripting.Ast.Expression;
    
    public abstract class PythonCustomTracker : CustomTracker {
        public abstract PythonTypeSlot/*!*/ GetSlot();

        public override Expression GetValue(ActionBinder binder, Type type) {
            return Ast.RuntimeConstant(GetSlot());
        }

        public override MemberTracker BindToInstance(Expression instance) {
            return new BoundMemberTracker(this, instance);
        }

        public override Expression SetValue(ActionBinder binder, Type type, Expression value) {
            return SetBoundValue(binder, type, value, Ast.Null());
        }

        protected override Expression GetBoundValue(ActionBinder binder, Type type, Expression instance) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("SlotGetValue"),
                Ast.CodeContext(),
                Ast.RuntimeConstant(GetSlot()),
                Ast.ConvertHelper(
                    instance,
                    typeof(object)
                ),
                Ast.RuntimeConstant(DynamicHelpers.GetPythonTypeFromType(type))
            );
        }
        
        protected override Expression SetBoundValue(ActionBinder binder, Type type, Expression value, Expression instance) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("SlotSetValue"),
                Ast.CodeContext(),
                Ast.RuntimeConstant(GetSlot()),
                Ast.ConvertHelper(
                    instance,
                    typeof(object)
                ),
                Ast.RuntimeConstant(DynamicHelpers.GetPythonTypeFromType(type)),
                value
            );
        }
    }

    /// <summary>
    /// Provides a CustomTracker which handles special fields which have custom
    /// behavior on get/set.
    /// </summary>
    class CustomAttributeTracker : PythonCustomTracker {
        private readonly PythonTypeSlot/*!*/ _slot;
        private readonly Type/*!*/ _declType;
        private readonly string/*!*/ _name;

        public CustomAttributeTracker(Type/*!*/ declaringType, string/*!*/ name, PythonTypeSlot/*!*/ slot) {
            Debug.Assert(slot != null);
            Debug.Assert(declaringType != null);
            Debug.Assert(name != null);

            _declType = declaringType;
            _name = name;
            _slot = slot;
        }

        public override Expression GetValue(ActionBinder binder, Type type) {
            return GetBoundValue(binder, type, Ast.Constant(null));
        }

        public override string Name {
            get { return _name; }
        }

        public override Type DeclaringType {
            get {
                return _declType;
            }
        }

        public override PythonTypeSlot/*!*/ GetSlot() {
            return _slot;
        }
    }

    class ClassMethodTracker : PythonCustomTracker {
        private MethodTracker/*!*/[]/*!*/ _trackers;
        
        public ClassMethodTracker(MemberGroup/*!*/ group) {
            List<MethodTracker> trackers = new List<MethodTracker>(group.Count);
            
            foreach (MethodTracker mt in group) {
                trackers.Add(mt);
            }

            _trackers = trackers.ToArray();
        }

        public override PythonTypeSlot GetSlot() {
            List<MethodBase> meths = new List<MethodBase>();
            foreach (MethodTracker mt in _trackers) {
                meths.Add(mt.Method);
            }

            return PythonTypeOps.GetFinalSlotForFunction(
                PythonTypeOps.GetBuiltinFunction(DeclaringType,
                        Name,
                        meths.ToArray()
                )
            );
        }

        public override Expression GetValue(ActionBinder binder, Type type) {
            return GetBoundValue(binder, type, Ast.Null());
        }

        public Expression GetBoundPythonValue(ActionBinder binder, PythonType accessing) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("SlotGetValue"),
                Ast.CodeContext(),
                Ast.RuntimeConstant(GetSlot()),
                Ast.Null(),
                Ast.RuntimeConstant(accessing)
            );
        }

        public override Type DeclaringType {
            get { return _trackers[0].DeclaringType; }
        }

        public override string Name {
            get { return _trackers[0].Name; }
        }
    }

    class OperatorTracker : PythonCustomTracker {
        private MethodTracker/*!*/[]/*!*/ _trackers;
        private bool _reversed;
        private string/*!*/ _name;
        private Type/*!*/ _declType;

        public OperatorTracker(Type/*!*/ declaringType, string/*!*/ name, bool reversed, params MethodTracker/*!*/[]/*!*/ members) {
            Debug.Assert(declaringType != null);
            Debug.Assert(members != null);
            Debug.Assert(name != null);
            Debug.Assert(members.Length > 0);

            _declType = declaringType;
            _reversed = reversed;
            _trackers = members;
            _name = name;
        }        
        
        public override PythonTypeSlot/*!*/ GetSlot() {
            List<MethodBase> meths = new List<MethodBase>();
            foreach (MethodTracker mt in _trackers) {
                meths.Add(mt.Method);
            }

            MethodBase[] methods = meths.ToArray();
            FunctionType ft = (PythonTypeOps.GetMethodFunctionType(DeclaringType, methods) | FunctionType.Method) & (~FunctionType.Function);
            if (_reversed) {
                ft |= FunctionType.ReversedOperator;
            } else {
                ft &= ~FunctionType.ReversedOperator;
            }

            // operators are always visible as they get mapped into Python names.
            ft |= FunctionType.AlwaysVisible;

            return PythonTypeOps.GetFinalSlotForFunction(PythonTypeOps.GetBuiltinFunction(DeclaringType, 
                        Name,
                        ft,
                        meths.ToArray()
                    ));
        }

        public override Type DeclaringType {
            get { return _declType; }
        }

        public override string Name {
            get { return _name; }
        }
    }
}
