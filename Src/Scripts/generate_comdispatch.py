#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

import generate
reload(generate)
from generate import CodeGenerator, CodeWriter

class VariantType:
    def __init__(self, 
        variantType,
        managedType,
        isPrimitiveType=True,
        unmanagedRepresentationType=None,
        clsCompliant=True,
        includeInUnionTypes=True,
        getStatements=None,
        setStatements=None):
        
        self.variantType = variantType
        self.managedType = managedType
        self.isPrimitiveType = isPrimitiveType
        if unmanagedRepresentationType == None: self.unmanagedRepresentationType = managedType
        else: self.unmanagedRepresentationType = unmanagedRepresentationType
        
        self.clsCompliant = clsCompliant
        self.includeInUnionTypes = includeInUnionTypes
        
        self.getStatements = getStatements
        self.setStatements = setStatements
        
        self.managedFieldName = "_" + self.variantType.lower()
        firstChar = self.variantType[0]
        self.accessorName = "As" + self.variantType.lower().replace(firstChar.lower(), firstChar, 1)
    
    def write_UnionTypes(self, cw):
        if not self.includeInUnionTypes: return
        if self.unmanagedRepresentationType == "IntPtr":
            cw.write('[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]')
        cw.write("[FieldOffset(0)] internal %s %s;" % (self.unmanagedRepresentationType, self.managedFieldName))

    def write_ToObject(self, cw):
        cw.write("case VarEnum.VT_%s: return %s;" % (self.variantType, self.accessorName))

    def write_accessor(self, cw):
        if not self.clsCompliant:
            cw.write("[CLSCompliant(false)]")
        
        cw.enter_block('public %s %s' % (self.managedType, self.accessorName))

        cw.enter_block("get")
        cw.write("Debug.Assert(VariantType == VarEnum.VT_%s);" % self.variantType)
        if self.getStatements == None:
            cw.write("return _typeUnion._unionTypes.%s;" % self.managedFieldName)
        else:
            for s in self.getStatements: cw.write(s)
        cw.exit_block()

        cw.enter_block("set")
        cw.write("Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise")
        cw.write("VariantType = VarEnum.VT_%s;" % self.variantType)
        if self.setStatements == None:
            cw.write("_typeUnion._unionTypes.%s = value;" % self.managedFieldName)
        else:
            for s in self.setStatements: cw.write(s)
        cw.exit_block()

        cw.exit_block()
        cw.writeline()
        
    def write_accessor_propertyinfo(self, cw):
        cw.write('case VarEnum.VT_%s: return typeof(Variant).GetProperty("%s");' % (self.variantType, self.accessorName))
        
    def write_ComToManagedPrimitiveTypes(self, cw):
        if not self.isPrimitiveType: return
        cw.write("dict[VarEnum.VT_%s] = typeof(%s);" % (self.variantType, self.managedType))

    def write_IsPrimitiveType(self, cw):
        if not self.isPrimitiveType: return
        cw.write("case VarEnum.VT_%s:" % self.variantType)

variantTypes = [
  # VariantType('varEnum', 'managed_type')
    VariantType('I1', "SByte", clsCompliant=False),
    VariantType('I2', "Int16"),
    VariantType('I4', "Int32"),
    VariantType('I8', "Int64"),

    VariantType('UI1', "Byte", clsCompliant=False),
    VariantType('UI2', "UInt16", clsCompliant=False),
    VariantType('UI4', "UInt32", clsCompliant=False),
    VariantType('UI8', "UInt64", clsCompliant=False),

    VariantType('INT', "IntPtr"),
    VariantType('UINT', "UIntPtr", clsCompliant=False),

    VariantType('BOOL', "bool", unmanagedRepresentationType="Int32",
                                getStatements=["return _typeUnion._unionTypes._bool != 0;"],
                                setStatements=["_typeUnion._unionTypes._bool = value ? -1 : 0;"]),

    VariantType('R4', "Single"),
    VariantType('R8', "Double"),
    VariantType('DECIMAL', "Decimal", includeInUnionTypes=False,
                                      getStatements=["// The first byte of Decimal is unused, but usually set to 0", 
                                                     "Variant v = this;", 
                                                     "v._typeUnion._vt = 0;", 
                                                     "return v._decimal;"],
                                       setStatements=["_decimal = value;", 
                                                      "// _vt overlaps with _decimal, and should be set after setting _decimal", 
                                                      "_typeUnion._vt = (ushort)VarEnum.VT_DECIMAL;"]),
    VariantType('DATE', "DateTime", unmanagedRepresentationType="double",
                                    getStatements=["return DateTime.FromOADate(_typeUnion._unionTypes._date);"],
                                    setStatements=["_typeUnion._unionTypes._date = value.ToOADate();"]),
    VariantType('BSTR', "String", unmanagedRepresentationType="IntPtr",
                                  getStatements=["return (string)Marshal.GetObjectForNativeVariant(ComRuntimeHelpers.UnsafeMethods.ConvertByrefToPtr(out this));"],
                                  setStatements=["Marshal.GetNativeVariantForObject(value, ComRuntimeHelpers.UnsafeMethods.ConvertByrefToPtr(out this));"]),
    VariantType("UNKNOWN", "Object", isPrimitiveType=False,
                                      unmanagedRepresentationType="IntPtr",
                                getStatements=["return Marshal.GetObjectForIUnknown(_typeUnion._unionTypes._unknown);"],
                                setStatements=["_typeUnion._unionTypes._unknown = Marshal.GetIUnknownForObject(value);"]),
    VariantType("DISPATCH", "Object", isPrimitiveType=False,
                                      unmanagedRepresentationType="IntPtr",
                                getStatements=["return Marshal.GetObjectForIUnknown(_typeUnion._unionTypes._dispatch);"],
                                setStatements=["_typeUnion._unionTypes._dispatch = Marshal.GetIDispatchForObject(value);"])
]

def gen_UnionTypes(cw):
    for variantType in variantTypes:
        variantType.write_UnionTypes(cw)

def gen_ToObject(cw):
    for variantType in variantTypes:
        variantType.write_ToObject(cw)

def gen_accessors(cw):
    for variantType in variantTypes:
        variantType.write_accessor(cw)

def gen_accessor_propertyinfo(cw):
    for variantType in variantTypes:
        variantType.write_accessor_propertyinfo(cw)

def gen_ComToManagedPrimitiveTypes(cw):
    for variantType in variantTypes:
        variantType.write_ComToManagedPrimitiveTypes(cw)

def gen_IsPrimitiveType(cw):
    for variantType in variantTypes:
        variantType.write_IsPrimitiveType(cw)

CodeGenerator("Variant union types", gen_UnionTypes).doit()
CodeGenerator("Variant ToObject", gen_ToObject).doit()
CodeGenerator("Variant accessors", gen_accessors).doit()
CodeGenerator("Variant accessors PropertyInfos", gen_accessor_propertyinfo).doit()
CodeGenerator("ComToManagedPrimitiveTypes", gen_ComToManagedPrimitiveTypes).doit()
CodeGenerator("Variant IsPrimitiveType", gen_IsPrimitiveType).doit()