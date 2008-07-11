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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Actions.ComDispatch {

    /// <summary>
    /// Creates rules for performing method calls.  Currently supports calling built-in functions, built-in method descriptors (w/o 
    /// a bound value) and bound built-in method descriptors (w/ a bound value), delegates, types defining a "Call" method marked
    /// with SpecialName.
    /// </summary>
    /// <typeparam name="T">The type of the dynamic site</typeparam>
    /// <typeparam name="TAction">The specific type of OldCallAction</typeparam>
    internal abstract class ComCallBinderHelper<T, TAction> : ComBinderHelper<T, TAction>
        where T : class
        where TAction : OldCallAction {

        // the instance or null if this is a non-instance call
        private Expression _instance;

        internal ComCallBinderHelper(CodeContext/*!*/ context, TAction/*!*/ action, object[]/*!*/ args)
            : base(context, action, args) {
            Test = Rule.MakeTypeTest(CompilerHelpers.GetType(Callable), 0);
        }

        /// <summary>
        /// The instance for the target method, or null if this is a non-instance call.
        /// 
        /// If it is set, it will typically be set to extract the instance from the Callable.
        /// </summary>
        protected Expression Instance {
            get {
                return _instance;
            }
            set {
                Debug.Assert(!Action.Signature.HasInstanceArgument());
                _instance = value;
            }
        }

        protected object Callable {
            get { return Arguments[0]; }
        }

        protected Expression[] FinishTestForCandidate(IList<Type> testTypes, Type[] explicitArgTypes) {
            Expression[] exprArgs = MakeArgumentExpressions();
            Debug.Assert(exprArgs.Length == (explicitArgTypes.Length + ((_instance == null) ? 0 : 1)));
            Debug.Assert(testTypes == null || exprArgs.Length == testTypes.Count);

            MakeSplatTests();

            if (explicitArgTypes.Length > 0 && testTypes != null) {
                // We've already tested the instance, no need to test it again. So remove it before adding 
                // rules for the arguments
                Expression[] exprArgsWithoutInstance = exprArgs;
                List<Type> testTypesWithoutInstance = new List<Type>(testTypes);
                for (int i = 0; i < exprArgs.Length; i++) {
                    if (exprArgs[i] == _instance) {
                        // We found the instance, so remove it
                        exprArgsWithoutInstance = ArrayUtils.RemoveAt(exprArgs, i);
                        testTypesWithoutInstance.RemoveAt(i);
                        break;
                    }
                }

                Test = Expression.AndAlso(Test, MakeNecessaryTests(testTypesWithoutInstance.ToArray(), exprArgsWithoutInstance));
            }

            return exprArgs;
        }

        /// <summary>
        /// Gets expressions to access all the arguments. This includes the instance argument. Splat arguments are
        /// unpacked in the output. The resulting array is similar to Rule.Parameters (but also different in some ways)
        /// </summary>
        protected Expression[] MakeArgumentExpressions() {
            List<Expression> exprargs = new List<Expression>();
            if (_instance != null) {
                exprargs.Add(_instance);
            }

            for (int i = 0; i < Action.Signature.ArgumentCount; i++) { // ArgumentCount(Action, Rule)
                switch (Action.Signature.GetArgumentKind(i)) {
                    case ArgumentKind.Simple:
                    case ArgumentKind.Named:
                        exprargs.Add(Rule.Parameters[i + 1]);
                        break;

                    case ArgumentKind.List:
                        IList<object> list = (IList<object>)Arguments[i + 1];
                        for (int j = 0; j < list.Count; j++) {
                            exprargs.Add(
                                Expression.Call(
                                    Expression.Convert(
                                        Rule.Parameters[i + 1],
                                        typeof(IList<object>)
                                    ),
                                    typeof(IList<object>).GetMethod("get_Item"),
                                    Expression.Constant(j)
                                )
                            );
                        }
                        break;

                    case ArgumentKind.Dictionary:
                        IDictionary dict = (IDictionary)Arguments[i + 1];

                        IDictionaryEnumerator dictEnum = dict.GetEnumerator();
                        while (dictEnum.MoveNext()) {
                            DictionaryEntry de = dictEnum.Entry;

                            string strKey = de.Key as string;
                            if (strKey == null) continue;

                            Expression dictExpr = Rule.Parameters[Rule.Parameters.Count - 1];
                            exprargs.Add(
                                Expression.Call(
                                    Expression.ConvertHelper(dictExpr, typeof(IDictionary)),
                                    typeof(IDictionary).GetMethod("get_Item"),
                                    Expression.Constant(strKey)
                                )
                            );
                        }
                        break;
                }
            }
            return exprargs.ToArray();
        }


        #region Test support

        /// <summary>
        /// Makes test for param arrays and param dictionary parameters.
        /// </summary>
        protected void MakeSplatTests() {
            if (Action.Signature.HasListArgument()) {
                MakeParamsArrayTest();
            }

            if (Action.Signature.HasDictionaryArgument()) {
                MakeParamsDictionaryTest();
            }
        }

        private void MakeParamsArrayTest() {
            int listIndex = Action.Signature.IndexOf(ArgumentKind.List);
            Debug.Assert(listIndex != -1);
            Test = Expression.AndAlso(Test, MakeParamsTest(Arguments[listIndex + 1], Rule.Parameters[listIndex + 1]));
        }

        private void MakeParamsDictionaryTest() {
            IDictionary dict = (IDictionary)Arguments[Arguments.Count - 1];
            IDictionaryEnumerator dictEnum = dict.GetEnumerator();

            // verify the dictionary has the same count and arguments.

            string[] names = new string[dict.Count];
            int index = 0;
            while (dictEnum.MoveNext()) {
                string name = dictEnum.Entry.Key as string;
                if (name == null) {
                    throw RuntimeHelpers.SimpleTypeError(String.Format("expected string for dictionary argument got {0}", dictEnum.Entry.Key));
                }
                names[index++] = name;
            }

            Test = Expression.AndAlso(
                Test,
                Expression.AndAlso(
                    Expression.TypeIs(Rule.Parameters[Rule.Parameters.Count - 1], typeof(IDictionary)),
                    Expression.Call(
                        typeof(RuntimeHelpers).GetMethod("CheckDictionaryMembers"),
                        Expression.Convert(Rule.Parameters[Rule.Parameters.Count - 1], typeof(IDictionary)),
                        Expression.Constant(names)
                    )
                )
            );
        }

        #endregion


        /// <summary>
        /// Gets all of the argument names and types. The instance argument is not included
        /// </summary>
        /// <param name="argNames">The names correspond to the end of argTypes.
        /// ArgumentKind.Dictionary is unpacked in the return value.
        /// This is set to an array of size 0 if there are no keyword arguments</param>
        /// <param name="argTypes">Non named arguments are returned at the beginning.
        /// ArgumentKind.List is unpacked in the return value. </param>
        protected void GetArgumentNamesAndTypes(out SymbolId[] argNames, out Type[] argTypes) {
            // Get names of named arguments
            argNames = Action.Signature.GetArgumentNames();
            argTypes = GetArgumentTypes();

            if (Action.Signature.HasDictionaryArgument()) {
                // need to get names from dictionary argument...
                GetDictionaryNamesAndTypes(ref argNames, ref argTypes);
            }
        }

        private Type[] GetArgumentTypes() {
            List<Type> res = new List<Type>();
            for (int i = 1; i < Arguments.Count; i++) {
                switch (Action.Signature.GetArgumentKind(i - 1)) {
                    case ArgumentKind.Simple:
                    case ArgumentKind.Instance:
                    case ArgumentKind.Named:
                        res.Add(CompilerHelpers.GetType(Arguments[i]));
                        continue;

                    case ArgumentKind.List:
                        IList<object> list = Arguments[i] as IList<object>;
                        if (list == null) return null;

                        for (int j = 0; j < list.Count; j++) {
                            res.Add(CompilerHelpers.GetType(list[j]));
                        }
                        break;

                    case ArgumentKind.Dictionary:
                        // caller needs to process these...
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            return res.ToArray();
        }

        private void GetDictionaryNamesAndTypes(ref SymbolId[] argNames, ref Type[] argTypes) {
            Debug.Assert(Action.Signature.GetArgumentKind(Action.Signature.ArgumentCount - 1) == ArgumentKind.Dictionary);

            List<SymbolId> names = new List<SymbolId>(argNames);
            List<Type> types = new List<Type>(argTypes);

            IDictionary dict = (IDictionary)Arguments[Arguments.Count - 1];
            IDictionaryEnumerator dictEnum = dict.GetEnumerator();
            while (dictEnum.MoveNext()) {
                DictionaryEntry de = dictEnum.Entry;

                if (de.Key is string) {
                    names.Add(SymbolTable.StringToId((string)de.Key));
                    types.Add(CompilerHelpers.GetType(de.Value));
                }
            }

            argNames = names.ToArray();
            argTypes = types.ToArray();
        }
    }
}

#endif
