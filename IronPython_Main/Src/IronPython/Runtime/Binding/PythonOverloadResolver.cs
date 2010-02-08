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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;
using System.Collections;

namespace IronPython.Runtime.Binding {
    internal sealed class PythonOverloadResolverFactory : OverloadResolverFactory {
        private readonly PythonBinder/*!*/ _binder;
        internal readonly Expression/*!*/ _codeContext;

        public PythonOverloadResolverFactory(PythonBinder/*!*/ binder, Expression/*!*/ codeContext) {
            Assert.NotNull(binder, codeContext);
            _binder = binder;
            _codeContext = codeContext;
        }

        public override DefaultOverloadResolver CreateOverloadResolver(IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType) {
            return new PythonOverloadResolver(_binder, args, signature, callType, _codeContext);
        }
    }

    public sealed class PythonOverloadResolver : DefaultOverloadResolver {
        private readonly Expression _context;

        public Expression ContextExpression {
            get { return _context; }
        }

        // instance method call:
        public PythonOverloadResolver(PythonBinder binder, DynamicMetaObject instance, IList<DynamicMetaObject> args, CallSignature signature,
            Expression codeContext)
            : base(binder, instance, args, signature) {
            Assert.NotNull(codeContext);
            _context = codeContext;
        }

        // method call:
        public PythonOverloadResolver(PythonBinder binder, IList<DynamicMetaObject> args, CallSignature signature, Expression codeContext)
            : this(binder, args, signature, CallTypes.None, codeContext) {
        }

        // method call:
        public PythonOverloadResolver(PythonBinder binder, IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType, Expression codeContext)
            : base(binder, args, signature, callType) {
            Assert.NotNull(codeContext);
            _context = codeContext;
        }

        private new PythonBinder Binder {
            get {
                return (PythonBinder)base.Binder;
            }
        }

        public override bool CanConvertFrom(Type fromType, DynamicMetaObject fromArg, ParameterWrapper toParameter, NarrowingLevel level) {
            if ((fromType == typeof(List) || fromType.IsSubclassOf(typeof(List)))) {
                if (toParameter.Type.IsGenericType &&
                    toParameter.Type.GetGenericTypeDefinition() == typeof(IList<>) &&
                    (toParameter.ParameterInfo.IsDefined(typeof(BytesConversionAttribute), false) ||
                     toParameter.ParameterInfo.IsDefined(typeof(BytesConversionNoStringAttribute), false))) {
                    return false;
                }
            } else if (fromType == typeof(string)) {
                if (toParameter.Type == typeof(IList<byte>) &&
                    !Binder.Context.PythonOptions.Python30 &&
                    toParameter.ParameterInfo.IsDefined(typeof(BytesConversionAttribute), false)) {
                    // string -> byte array, we allow this in Python 2.6
                    return true;
                }
            } else if (fromType == typeof(Bytes)) {
                if (toParameter.Type == typeof(string) &&
                    !Binder.Context.PythonOptions.Python30 &&
                    toParameter.ParameterInfo.IsDefined(typeof(BytesConversionAttribute), false)) {
                    return true;
                }
            }

            return base.CanConvertFrom(fromType, fromArg, toParameter, level);
        }

        protected override BitArray MapSpecialParameters(ParameterMapping/*!*/ mapping) {
            var infos = mapping.Overload.Parameters;
            BitArray special = base.MapSpecialParameters(mapping);

            if (infos.Count > 0) {
                bool normalSeen = false;
                for (int i = 0; i < infos.Count; i++) {
                    bool isSpecial = false;
                    if (infos[i].ParameterType.IsSubclassOf(typeof(SiteLocalStorage))) {
                        mapping.AddBuilder(new SiteLocalStorageBuilder(infos[i]));
                        isSpecial = true;
                    } else if (infos[i].ParameterType == typeof(CodeContext) && !normalSeen) {
                        mapping.AddBuilder(new ContextArgBuilder(infos[i]));
                        isSpecial = true;
                    } else {
                        normalSeen = true;
                    }

                    if (isSpecial) {
                        (special = special ?? new BitArray(infos.Count))[i] = true;
                    }
                }
            }

            return special;
        }

        protected override Expression GetByRefArrayExpression(Expression argumentArrayExpression) {
            return Expression.Call(typeof(PythonOps).GetMethod("MakeTuple"), argumentArrayExpression);
        }

        protected override bool AllowMemberInitialization(OverloadInfo method) {
            return method.IsInstanceFactory && !method.DeclaringType.IsDefined(typeof(PythonTypeAttribute), true);
        }

        public override Expression Convert(DynamicMetaObject metaObject, Type restrictedType, ParameterInfo info, Type toType) {
            return Binder.ConvertExpression(metaObject.Expression, toType, ConversionResultKind.ExplicitCast, new PythonOverloadResolverFactory(Binder, _context));
        }

        public override Expression GetDynamicConversion(Expression value, Type type) {
            return Expression.Dynamic(
                Binder.Context.Convert(type, ConversionResultKind.ExplicitCast), 
                type, 
                value);
        }

        public override Type GetGenericInferenceType(DynamicMetaObject dynamicObject) {            
            Type res = PythonTypeOps.GetFinalSystemType(dynamicObject.LimitType);
            if (res == typeof(ExtensibleString) ||
                res == typeof(ExtensibleComplex) || 
                (res.IsGenericType && res.GetGenericTypeDefinition() == typeof(Extensible<>))) {
                return typeof(object);
            }

            return res;
        }

        public override Func<object[], object> GetConvertor(int index, DynamicMetaObject metaObject, ParameterInfo info, Type toType) {
            return Binder.ConvertObject(index, metaObject, toType, ConversionResultKind.ExplicitCast);
        }
    }
}
