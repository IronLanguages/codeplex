/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace TestAst {
    public class TestScope {
        private TestScope _parent;
        private LambdaBuilder _block;
        private Dictionary<string, ParameterExpression> _variables = new Dictionary<string, ParameterExpression>();

        //////////////////////////////////////////////////////////
        public static TestScope Current = null;
        public static void PushNewScope(string name) {
            Current = new TestScope(name, Current);
        }
        public static void PopScope() {
            Current = Current.Parent;
        }
        //////////////////////////////////////////////////////////

        public TestScope(string name, TestScope parent) {
            _block = Utils.Lambda(typeof(object), name);
            _parent = parent;
            Current = this;
        }

        public TestScope Parent {
            get {
                return _parent;
            }
        }

        public TestScope TopScope {
            get {
                if (_parent == null) {
                    return this;
                } else {
                    return _parent.TopScope;
                }
            }
        }

        public LambdaBuilder Block {
            get {
                return _block;
            }
        }

        public ParameterExpression GetOrMakeLocal(string name) {
            return GetOrMakeLocal(name, typeof(object));
        }

        public ParameterExpression GetOrMakeLocal(string name, Type type) {
            ParameterExpression ret;
            if (_variables.TryGetValue(name, out ret)) {
                return ret;
            }
            ret = (ParameterExpression)_block.Variable(type, name);
            _variables[name] = ret;
            return ret;
        }

        public ParameterExpression LookupName(string name) {
            ParameterExpression var;
            if (_variables.TryGetValue(name, out var)) {
                return var;
            }

            if (_parent != null) {
                return _parent.LookupName(name);
            } else {
                return null;
            }
        }

        public ParameterExpression HiddenVariable(Type type, string name) {
            return _block.HiddenVariable(type, name);
        }

        public LambdaExpression FinishScope(Expression body) {
            _block.Body = body;
            _block.AddParameters(Expression.Parameter(typeof(Scope), "#scope"), Expression.Parameter(typeof(LanguageContext), "#context"));
            
            return _block.MakeLambda();
        }
    }
}

