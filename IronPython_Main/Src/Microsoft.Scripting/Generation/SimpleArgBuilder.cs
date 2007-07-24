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

using System;
using System.Collections.Generic;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Generation {
    public class SimpleArgBuilder : ArgBuilder {
        private int _index;
        private Type _parameterType;
        private bool _isParams;

        public SimpleArgBuilder(int index, Type parameterType) {
            _index = index;
            _parameterType = parameterType;
        }

        public SimpleArgBuilder(int index, Type parameterType, bool isParams) {
            if (index < 0) throw new ArgumentOutOfRangeException("index");
            if (parameterType == null) throw new ArgumentNullException("parameterType");

            _index = index;
            _parameterType = parameterType;
            _isParams = isParams;
        }

        public override int Priority {
            get { return 0; }
        }

        public bool IsParams {
            get {
                return _isParams;
            }
        }

        public override object Build(CodeContext context, object[] args) {            
            return context.LanguageContext.Binder.Convert(args[_index], _parameterType);
        }

        public override Expression ToExpression(ActionBinder binder, Expression[] parameters) {
            return binder.ConvertExpression(parameters[_index], _parameterType);
        }

        public override AbstractValue AbstractBuild(AbstractContext context, IList<AbstractValue> parameters) {
            AbstractValue value = parameters[_index];
            return context.Binder.AbstractExecute(ConvertToAction.Make(_parameterType), new AbstractValue[] { value });
        }

        public int Index {
            get {
                return _index;
            }
        }

        public Type Type {
            get {
                return _parameterType;
            }
        }
    }
}
