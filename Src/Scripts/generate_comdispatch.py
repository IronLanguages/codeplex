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

from generate import generate

class VariantType:
    def __init__(self, 
        variantType,
        managedType,
        emitAccessors=True,
        isPrimitiveType=True,
        unmanagedRepresentationType=None,
        includeInUnionTypes=True,
        getStatements=None,
        setStatements=None):
        
        self.emitAccessors = emitAccessors
        self.variantType = variantType
        self.managedType = managedType
        self.isPrimitiveType = isPrimitiveType
        if unmanagedRepresentationType == None: self.unmanagedRepresentationType = managedType
        else: self.unmanagedRepresentationType = unmanagedRepresentationType
        
        self.includeInUnionTypes = includeInUnionTypes
        
        self.getStatements = getStatements
        self.setStatements = setStatements
        
        self.managedFieldName = "_" + self.variantType.lower()
        firstChar = self.variantType[0]
        self.name = self.variantType.lower().replace(firstChar.lower(), firstChar, 1)
        self.accessorName = "As" + self.name
    
    def write_UnionTypes(self, cw):
        if not self.includeInUnionTypes: return
        if self.unmanagedRepresentationType == "IntPtr":
            cw.write('[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]')
        if self.managedFieldName == '_bstr':
            cw.write('[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]')
        cw.write("[FieldOffset(0)] internal %s %s;" % (self.unmanagedRepresentationType, self.managedFieldName))

    def write_ToObject(self, cw):
        cw.write("case VarEnum.VT_%s: return %s;" % (self.variantType, self.accessorName))

    def write_accessor(self, cw):
        if self.emitAccessors == False :
            return
            
        cw.write("// VT_%s" % self.variantType)
        cw.writeline()
        
        cw.enter_block('public %s %s' % (self.managedType, self.accessorName))

        # Getter
        cw.enter_block("get")
        cw.write("Debug.Assert(VariantType == VarEnum.VT_%s);" % self.variantType)
        if self.getStatements == None:
            cw.write("return _typeUnion._unionTypes.%s;" % self.managedFieldName)
        else:
            for s in self.getStatements: cw.write(s)
        cw.exit_block()

        # Setter
        cw.enter_block("set")
        cw.write("Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise")
        cw.write("VariantType = VarEnum.VT_%s;" % self.variantType)
        if self.setStatements == None:
            cw.write("_typeUnion._unionTypes.%s = value;" % self.managedFieldName)
        else:
            for s in self.setStatements: cw.write(s)
        cw.exit_block()

        cw.exit_block()

        # Byref Setter
        cw.writeline()
        cw.enter_block("public void SetAsByref%s(ref %s value)" % (self.name, self.unmanagedRepresentationType))
        cw.write("Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise")
        cw.write("VariantType = (VarEnum.VT_%s | VarEnum.VT_BYREF);" % self.variantType)
        cw.write("_typeUnion._unionTypes._byref = UnsafeMethods.Convert%sByrefToPtr(ref value);" % self.unmanagedRepresentationType)
        cw.exit_block()

        cw.writeline()
        
    def write_accessor_propertyinfo(self, cw):
        if self.emitAccessors == True :
            cw.write('case VarEnum.VT_%s: return typeof(Variant).GetProperty("%s");' % (self.variantType, self.accessorName))
        
    def write_byref_setters(self, cw):
        if self.emitAccessors == True :
            cw.write('case VarEnum.VT_%s: return typeof(Variant).GetMethod("SetAsByref%s");' % (self.variantType, self.name))

    def write_ComToManagedPrimitiveTypes(self, cw):
        wrapper_types = ["CY", "DISPATCH", "UNKNOWN", "ERROR"]           
        if not self.isPrimitiveType or (self.variantType in wrapper_types) : return
        cw.write("dict[VarEnum.VT_%s] = typeof(%s);" % (self.variantType, self.managedType))

    def write_IsPrimitiveType(self, cw):
        if not self.isPrimitiveType: return
        cw.write("case VarEnum.VT_%s:" % self.variantType)

    def write_ConvertByrefToPtr(self, cw):
        if self.isPrimitiveType and self.unmanagedRepresentationType == self.managedType and self.variantType != "ERROR":
            cw.write('[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]')
            if self.unmanagedRepresentationType == 'Int32':
                cw.enter_block("public static unsafe IntPtr Convert%sByrefToPtr(ref %s value)" % (self.unmanagedRepresentationType, self.unmanagedRepresentationType))
            else:
                cw.enter_block("internal static unsafe IntPtr Convert%sByrefToPtr(ref %s value)" % (self.unmanagedRepresentationType, self.unmanagedRepresentationType))
            cw.enter_block('fixed (%s *x = &value)' % self.unmanagedRepresentationType)
            cw.write('AssertByrefPointsToStack(new IntPtr(x));')
            cw.write('return new IntPtr(x);')
            cw.exit_block()        
            cw.exit_block()
            cw.write('')
    
variantTypes = [
  # VariantType('varEnum', 'managed_type')
    VariantType('I1', "SByte"),
    VariantType('I2', "Int16"),
    VariantType('I4', "Int32"),
    VariantType('I8', "Int64"),

    VariantType('UI1', "Byte"),
    VariantType('UI2', "UInt16"),
    VariantType('UI4', "UInt32"),
    VariantType('UI8', "UInt64"),

    VariantType('INT', "IntPtr"),
    VariantType('UINT', "UIntPtr"),

    VariantType('BOOL', "bool", 
        unmanagedRepresentationType="Int16",
        getStatements=["return _typeUnion._unionTypes._bool != 0;"],
        setStatements=["_typeUnion._unionTypes._bool = value ? (Int16)(-1) : (Int16)0;"]),
    
    VariantType("ERROR", "Int32"),

    VariantType('R4', "Single"),
    VariantType('R8', "Double"),
    VariantType('DECIMAL', "Decimal", 
        includeInUnionTypes=False,
        getStatements=["// The first byte of Decimal is unused, but usually set to 0", 
                       "Variant v = this;", 
                       "v._typeUnion._vt = 0;", 
                       "return v._decimal;"],
        setStatements=["_decimal = value;", 
                        "// _vt overlaps with _decimal, and should be set after setting _decimal", 
                        "_typeUnion._vt = (ushort)VarEnum.VT_DECIMAL;"]),
    VariantType("CY", "Decimal", 
        unmanagedRepresentationType="Int64",
        getStatements=["return Decimal.FromOACurrency(_typeUnion._unionTypes._cy);"],
        setStatements=["_typeUnion._unionTypes._cy = Decimal.ToOACurrency(value);"]),

    VariantType('DATE', "DateTime", 
        unmanagedRepresentationType="Double",
        getStatements=["return DateTime.FromOADate(_typeUnion._unionTypes._date);"],
        setStatements=["_typeUnion._unionTypes._date = value.ToOADate();"]),
    VariantType('BSTR', "String", 
        unmanagedRepresentationType="IntPtr",
        getStatements=[
                "if (_typeUnion._unionTypes._bstr != IntPtr.Zero) {",
                "    return Marshal.PtrToStringBSTR(_typeUnion._unionTypes._bstr);",
                "} else {",
                "    return null;",
                "}"
        ],
        setStatements=[
                "if (value != null) {",
                "    Marshal.GetNativeVariantForObject(value, UnsafeMethods.ConvertVariantByrefToPtr(ref this));",
                "}"
        ]),
    VariantType("UNKNOWN", "Object", 
        isPrimitiveType=False,
        unmanagedRepresentationType="IntPtr",
        getStatements=[
                "if (_typeUnion._unionTypes._dispatch != IntPtr.Zero) {",
                "    return Marshal.GetObjectForIUnknown(_typeUnion._unionTypes._unknown);",
                "} else {",
                "    return null;",
                "}"
        ],
        setStatements=[
                "if (value != null) {",
                "    _typeUnion._unionTypes._unknown = Marshal.GetIUnknownForObject(value);",
                "}"
        ]),
    VariantType("DISPATCH", "Object", 
        isPrimitiveType=False,
        unmanagedRepresentationType="IntPtr",
        getStatements=[
                "if (_typeUnion._unionTypes._dispatch != IntPtr.Zero) {",
                "    return Marshal.GetObjectForIUnknown(_typeUnion._unionTypes._dispatch);",
                "} else {",
                "    return null;",
                "}"
        ],
        setStatements=[
                "if (value != null) {",
                "    _typeUnion._unionTypes._unknown = Marshal.GetIDispatchForObject(value);",
                "}"
        ]),
    VariantType("VARIANT", "Object", 
        emitAccessors=False,
        isPrimitiveType=False,
        unmanagedRepresentationType="Variant",
        includeInUnionTypes=False,              # will use "this" 
        getStatements=["return Marshal.GetObjectForNativeVariant(UnsafeMethods.ConvertVariantByrefToPtr(ref this));"],
        setStatements=["UnsafeMethods.InitVariantForObject(value, ref this);"])

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

def gen_byref_setters(cw):
    for variantType in variantTypes:
        variantType.write_byref_setters(cw)

def gen_ComToManagedPrimitiveTypes(cw):
    for variantType in variantTypes:
        variantType.write_ComToManagedPrimitiveTypes(cw)

def gen_IsPrimitiveType(cw):
    for variantType in variantTypes:
        variantType.write_IsPrimitiveType(cw)

def gen_ConvertByrefToPtr(cw):
    for variantType in variantTypes:
        variantType.write_ConvertByrefToPtr(cw)

def main():
    return generate(
        ("Variant union types", gen_UnionTypes),
        ("Variant ToObject", gen_ToObject),
        ("Variant accessors", gen_accessors),
        ("Variant accessors PropertyInfos", gen_accessor_propertyinfo),
        ("Variant byref setter", gen_byref_setters),
        ("ComToManagedPrimitiveTypes", gen_ComToManagedPrimitiveTypes),
        ("Variant IsPrimitiveType", gen_IsPrimitiveType),
        ("ConvertByrefToPtr", gen_ConvertByrefToPtr),
    )

if __name__ == "__main__":
    main()
