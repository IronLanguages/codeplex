/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;

namespace IronPython.Runtime.Operations {
    /// <summary>
    /// Helper class that all custom type descriptor implementations call for
    /// the bulk of their implementation.
    /// </summary>
    public static class CustomTypeDescHelpers {
        #region ICustomTypeDescriptor helper functions

        public static AttributeCollection GetAttributes(object self) {
            return AttributeCollection.Empty;
        }

        public static string GetClassName(object self) {
            object cls;
            if (Ops.TryGetAttr(DefaultContext.DefaultCLS, self, SymbolTable.Class, out cls)) {
                return Ops.GetAttr(DefaultContext.DefaultCLS, cls, SymbolTable.Name).ToString();
            }
            return null;
        }

        public static string GetComponentName(object self) {
            return null;
        }

        public static TypeConverter GetConverter(object self) {
            return new TypeConv(self);
        }

        public static EventDescriptor GetDefaultEvent(object self) {
            return null;
        }

        public static PropertyDescriptor GetDefaultProperty(object self) {
            return null;
        }

        public static object GetEditor(object self, Type editorBaseType) {
            return null;
        }

        public static EventDescriptorCollection GetEvents(object self, Attribute[] attributes) {
            if (attributes == null || attributes.Length == 0) return GetEvents(self);
            //!!! update when we support attributes on python types

            // you want things w/ attributes?  we don't have attributes!
            return EventDescriptorCollection.Empty;
        }

        public static EventDescriptorCollection GetEvents(object self) {
            return EventDescriptorCollection.Empty;
        }

        public static PropertyDescriptorCollection GetProperties(object self) {
            List<PropertyDescriptor> props = new List<PropertyDescriptor>(GetPropertiesImpl(self));
            return new PropertyDescriptorCollection(props.ToArray());
        }

        public static PropertyDescriptorCollection GetProperties(object self, Attribute[] attributes) {
            if (attributes == null || attributes.Length == 0) return GetProperties(self);
            //!!! update when we support attributes on python types

            // you want things w/ attributes?  we don't have attributes!
            return PropertyDescriptorCollection.Empty;
        }

        static IEnumerable<PropertyDescriptor> GetPropertiesImpl(object self) {
            List attrNames = Ops.GetAttrNames(DefaultContext.DefaultCLS, self);
            if (attrNames != null) {
                foreach (object o in attrNames) {
                    string s = o as string;
                    if (s == null) continue;
                    object attrVal = Ops.GetAttr(DefaultContext.DefaultCLS, self, SymbolTable.StringToId(s));
                    Type attrType = (attrVal == null) ? typeof(NoneTypeOps) : attrVal.GetType();
                    yield return new SuperDynamicObjectPropertyDescriptor(
                        s,
                        attrType,
                        self.GetType());
                }
            }
        }

        public static object GetPropertyOwner(object self, PropertyDescriptor pd) {
            return self;
        }

        #endregion

        class SuperDynamicObjectPropertyDescriptor : PropertyDescriptor {
            string _name;
            Type _propertyType;
            Type _componentType;
            internal SuperDynamicObjectPropertyDescriptor(
                string name,
                Type propertyType,
                Type componentType)
                : base(name, null) {
                _name = name;
                _propertyType = propertyType;
                _componentType = componentType;
            }

            public override object GetValue(object component) {
                return Ops.GetAttr(DefaultContext.DefaultCLS, component, SymbolTable.StringToId(_name));
            }
            public override void SetValue(object component, object value) {
                Ops.SetAttr(DefaultContext.DefaultCLS, component, SymbolTable.StringToId(_name), value);
            }

            public override bool CanResetValue(object component) {
                return true;
            }

            public override Type ComponentType {
                get { return _componentType; }
            }

            public override bool IsReadOnly {
                get { return false; }
            }

            public override Type PropertyType {
                get { return _propertyType; }
            }

            public override void ResetValue(object component) {
                Ops.DelAttr(DefaultContext.DefaultCLS, component, SymbolTable.StringToId(_name));
            }

            public override bool ShouldSerializeValue(object component) {
                object o;
                return Ops.TryGetAttr(component, SymbolTable.StringToId(_name), out o);
            }
        }

        public class TypeConv : TypeConverter {
            object convObj;

            public TypeConv(object self) {
                convObj = self;
            }

            #region TypeConverter overrides
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
                object result;
                return Converter.TryConvert(convObj, destinationType, out result);
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
                // we need an instance...
                ConstructorInfo ci = sourceType.GetConstructor(Type.EmptyTypes);
                if (ci != null) {
                    object value = ci.Invoke(new object[0]);
                    object result;
                    return Converter.TryConvert(value, convObj.GetType(), out result);
                } else {
                    throw new NotImplementedException("cannot determine conversion without instance");
                }
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
                return Converter.Convert(value, convObj.GetType());
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
                return Converter.Convert(convObj, destinationType);
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
                return false;
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
                return false;
            }

            public override bool IsValid(ITypeDescriptorContext context, object value) {
                object result;
                return Converter.TryConvert(value, convObj.GetType(), out result);
            }
            #endregion
        }
    }
}
