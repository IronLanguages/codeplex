#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################

from generate import CodeGenerator, CodeWriter

class Type:
    def __init__(self, lowercase, camelcase, priority, defaultval, IConvertible=True, localName=None, sealed=True):
        self.lowercase = lowercase
        self.camelcase = camelcase
        self.priority = priority
        self.defaultval = defaultval
        self.IConvertible = IConvertible
        self.localName = localName
        self.sealed = sealed

alltypes = {
    "none"      : Type("none", "none", 0, "null"),
    "object"    : Type("object", "Object", 150, "null", sealed=False),
    "bool"      : Type("bool", "Boolean", 37, "true"),
    "char"      : Type("char", "Char", 46, "'\\\\0'"),
    "sbyte"     : Type("sbyte", "SByte", 60, "0"),
    "byte"      : Type('byte', "Byte", 50, "0"),
    "short"     : Type('short', "Int16", 70, "0"),
    "ushort"    : Type('ushort', "UInt16", 100, "0"),
    "int"       : Type('int', "Int32", 10, "0"),
    "uint"      : Type('uint', "UInt32", 80, "0"),
    "long"      : Type('long', "Int64", 180, "0"),
    "ulong"     : Type('ulong', "UInt64", 90, "0"),
    "float"     : Type('float', "Single", 120, "0.0"),
    "double"    : Type('double', "Double", 30, "0.0"),
    "string"    : Type('string', "String", 20, "String.Empty", True, "str"),
    "decimal"   : Type('decimal', "Decimal", 150, "0", True),
    'BigInteger'   : Type('BigInteger', "BigInteger", 35, "BigInteger.Zero", False, "bi", False),
    'ExtensibleInt' : Type('ExtensibleInt', "ExtensibleInt", 23, "0", False, "ei", False),
    'ExtensibleComplex' : Type('ExtensibleComplex', "ExtensibleComplex", 128, "0", False, "ec", False),
    'ExtensibleString' : Type('ExtensibleString', "ExtensibleString", 50, "", False, "es", False),
    'ExtensibleFloat' : Type('ExtensibleFloat', "ExtensibleFloat", 125, "", False, "ef", False),
    "Complex64" : Type("Complex64", "Complex64", 40, "new Complex64(0.0)", False, sealed=False),
    "Delegate"  : Type("Delegate", "Delegate", 46, "null", False, "dlg", False),
    "IEnumerator" : Type("IEnumerator", "IEnumerator", 47, "null", False, "ie", False),
    "Type"      : Type("Type", "Type", 48, "null", False, "typ", False),
    "object[]" : Type("object[]", "ObjectArray", 160, "null", False, "objarr", False),
    "Tuple"     : Type("Tuple", "Tuple", 170, "null", False, "tup", False),
    "Enum"      : Type("Enum", "Enum", 110, "0", False, "e", False),
}

class To:
    def __init__(self, to, fromlist, gendefault=True):
        self.to = alltypes[to]
        self.fromlist = fromlist
        self.gendefault = gendefault

    def generate_if_statements(self, cw):
        total = len(self.fromlist)
        for i in xrange(total):
            f = self.fromlist[i]
            f.generate(cw, self.to, i == 0)
        cw.exit_block()

    def generate_locals(self, cw):
        total = len(self.fromlist)
        for i in xrange(total):
            f = self.fromlist[i]
            if f.fromtype.localName != None:
                cw.writeline("%s %s;" % (f.fromtype.lowercase, f.fromtype.localName))

    def generate_tryconvertto(self, cw):
        to_lc = self.to.lowercase
        to_cc = self.to.camelcase
        cw.enter_block("public static %s TryConvertTo%s(object value, out Conversion conversion)" % (to_lc, to_cc))

        self.generate_locals(cw)
        self.generate_if_statements(cw)

        self.generate_default(cw)
        cw.writeline("conversion = Conversion.None;")
        cw.writeline("return (%s)%s;" % (to_lc, self.to.defaultval))
        cw.exit_block()
        cw.writeline()

    def generate_if(self, cw, first):
        if first:
            eb = cw.enter_block
        else:
            eb = cw.else_block
        eb("if (to == %sType)" % self.to.camelcase)
        cw.writeline("return TryConvertTo%s(value, out conversion);" % self.to.camelcase)

    def generate_default(self, cw):
        pass

    def sort(self):
        self.fromlist.sort(compare_from_cls(self.to.lowercase))

    def generate_IConvertible(self, cw):
        if self.to.IConvertible:
            to_lc = self.to.lowercase
            to_cc = self.to.camelcase
            cw.enter_block("public static %s TryTo%s(object value, out Conversion conversion)" % (to_lc, to_cc))
            cw.enter_block("try")
            cw.writeline("%s %sVal = ((IConvertible)value).To%s(null);" % (to_lc,to_cc,to_cc))
            cw.writeline("conversion = Conversion.Eval;")
            cw.writeline("return %sVal;" % to_cc)
            cw.catch_block()
            cw.writeline("conversion = Conversion.None;")
            cw.writeline("return (%s)%s;" % (to_lc, self.to.defaultval))
            cw.exit_block()
            cw.exit_block()

    def generate_convertto(self, cw):
        to_lc = self.to.lowercase
        to_cc = self.to.camelcase
        cw.enter_block("public static %s ConvertTo%s(object value)" % (to_lc, to_cc))
        cw.writeline("Conversion conversion;")
        cw.writeline("%s val = TryConvertTo%s(value, out conversion);" % (to_lc, to_cc))
        cw.enter_block("if (conversion == Conversion.None)")
        cw.writeline("throw Ops.TypeError(\"expected %s, found {0}\", Ops.GetDynamicType(value).__name__);" % to_lc)
        cw.exit_block()
        cw.writeline("return val;")
        cw.exit_block()
        cw.writeline("")

class ToInteger(To):
    def generate_default(self, cw):
        cw.writeline("int Int32Val = TryConvertToInt32(value, out conversion);")
        cw.enter_block("if (conversion != Conversion.None)")
        cw.writeline("return BigInteger.Create(Int32Val);")
        cw.exit_block()

class ToComplex(To):
    def generate_default(self, cw):
        cw.writeline("double DoubleVal = TryConvertToDouble(value, out conversion);")
        cw.enter_block("if (conversion != Conversion.None)")
        cw.writeline("return Complex64.MakeReal(DoubleVal);")
        cw.exit_block()

class ToDelegate(To):
    def generate_if(self, cw, first):
        if first:
            eb = cw.enter_block
        else:
            eb = cw.else_block
        eb("if (DelegateType.IsAssignableFrom(to))")
        cw.writeline("return TryConvertTo%s(value, to, out conversion);" % self.to.camelcase)

    def generate_tryconvertto(self, cw):
        pass
    def generate_convertto(self, cw):
        pass

class ToEnumerator(To):
    def generate_tryconvertto(self, cw):
        to_lc = self.to.lowercase
        to_cc = self.to.camelcase
        cw.enter_block("public static %s TryConvertTo%s(object value, out Conversion conversion)" % (to_lc, to_cc))

        cw.writeline("IEnumerator ie = value as IEnumerator;");
        cw.enter_block("if (ie != null || value == null)")
        cw.writeline("conversion = Conversion.Identity;")
        cw.writeline("return ie;")
        cw.exit_block()
        
        cw.writeline("conversion = Conversion.Eval;")
        cw.writeline("return Ops.GetEnumerator(value);")
        cw.exit_block()
        cw.writeline()

class ToType(To):
    def generate_if_statements(self, cw):
        cw.writeline("Type TypeVal = value as Type;")
        cw.enter_block("if (TypeVal != null || value == null)")
        cw.writeline("conversion = Conversion.Identity;")
        cw.writeline("return TypeVal;")
        cw.exit_block()
        cw.writeline("PythonType PythonTypeVal = value as PythonType;")
        cw.enter_block("if (PythonTypeVal != null)")
        cw.writeline("conversion = Conversion.Implicit;")
        cw.writeline("return PythonTypeVal.type;")
        cw.exit_block()
        
class ToBool(To):
    def generate_if_statements(self, cw):
        cw.writeline("BigInteger bi;")
        cw.writeline("Enum e;")
        cw.writeline("string str;")
        cw.writeline("ExtensibleInt ei;")
        cw.writeline("")
        cw.enter_block("if (value == null)")
        cw.writeline("conversion = Conversion.None;")
        cw.writeline("return false;")
        for f in self.fromlist:
            if f.fromtype.lowercase == "ExtensibleInt": continue 
            f.generate(cw, self.to, False)
        cw.else_block("if (value is IPythonContainer)")
        cw.writeline("conversion = Conversion.Eval;")
        cw.writeline("return ((IPythonContainer)value).GetLength() != 0;")
        cw.else_block("if (value is ICollection)")
        cw.writeline("conversion = Conversion.Eval;")
        cw.writeline("return ((ICollection)value).Count != 0;");
        cw.exit_block()

        cw.writeline("object ret;")
        cw.writeline("// try __nonzero__ first before __len__");
        cw.enter_block("if (Ops.TryToInvoke(value, SymbolTable.NonZero, out ret))")
        cw.writeline("conversion = Conversion.Eval;")
        cw.writeline("Type retType = ret.GetType();")
        cw.enter_block("if (retType == typeof(bool) || retType == typeof(int))")
        cw.writeline("Conversion dummy;")
        cw.writeline("return TryConvertToBoolean(ret, out dummy);")
        cw.exit_block()
        cw.writeline("else throw Ops.TypeError(\"__nonzero__ should return bool or int, returned {0}\", Ops.GetClassName(ret));");
        cw.else_block("if (Ops.TryToInvoke(value, SymbolTable.Length, out ret))")
        cw.writeline("conversion = Conversion.Eval;")
        cw.writeline("Type retType = ret.GetType();")
        cw.enter_block("if (retType == typeof(bool) || retType == typeof(int))")
        cw.writeline("Conversion dummy;")
        cw.writeline("return TryConvertToBoolean(ret, out dummy);")
        cw.exit_block()
        cw.writeline("else throw Ops.TypeError(\"an integer is required\");");
        for f in self.fromlist:
            if f.fromtype.lowercase == "ExtensibleInt":  
                f.generate(cw, self.to, False)                    
        cw.exit_block()

    def generate_tryconvertto(self, cw):
        to_lc = self.to.lowercase
        to_cc = self.to.camelcase
        cw.enter_block("public static %s TryConvertTo%s(object value, out Conversion conversion)" % (to_lc, to_cc))

        self.generate_if_statements(cw)

        self.generate_default(cw)
        cw.writeline("conversion = Conversion.NonStandard;")
        cw.writeline("return (%s)%s;" % (to_lc, self.to.defaultval))
        cw.exit_block()
        cw.writeline()

class From:
    def __init__(self, fromtype, cond=None, cust=None, ct="Implicit"):
        self.fromtype = alltypes[fromtype]
        self.cond = cond
        self.cust = cust
        self.ct = ct
    def generate(self, cw, t, first):
        to_lc = t.lowercase
        to_cc = t.camelcase
        from_lc = self.fromtype.lowercase
        from_cc = self.fromtype.camelcase
        if self.fromtype.localName == None:
            if self.fromtype.lowercase == "object":
                if first: raise Exception, 'object cannot be first'
                cw.else_block("")
            elif first:
                cw.enter_block("if (value is %s)" % from_lc)
            else:
                cw.else_block("if (value is %s)" % from_lc)
        else:
            if first:
                cw.enter_block("if (!Object.Equals((%s = value as %s), null))" % (self.fromtype.localName, from_lc))
            else:
                cw.else_block("if (!Object.Equals((%s = value as %s), null))" % (self.fromtype.localName, from_lc))

        if self.cond:
            valName = ("%sVal" % from_cc)
            if self.fromtype.lowercase == 'object':
                cw.writeline("%s %sVal;" % (from_lc, from_cc))
            elif self.fromtype.localName == None:
                cw.writeline("%s %sVal = (%s)value;" % (from_lc, from_cc, from_lc))

            codetext = "if ("
            needand = False
            for cond in self.cond:
                if needand:
                    codetext += " &&\n                    "
                codetext += (cond % valName)
                needand = True
            codetext += ") {"
            cw.writeline(codetext)
            cw.writeline("    conversion = Conversion.%s;" % (self.ct))
            if self.cust:
                kwd = { "from_name":from_lc, "to_name":to_lc, "value_name": (valName)}
                for custline in self.cust:
                    cw.writeline(custline % kwd)
            else:
                cw.writeline("    return (%s)%s;" % (to_lc, valName))
            cw.writeline("}")
        else:
            if (self.cust):
                kwd = { "to_name":to_lc,
                        "to_cc":to_cc,
                        "from_name":from_lc,
                        "value_name":"value",
                        "conversion" : self.ct,
                        "cc_value": ("%sVal" % from_cc)}
                for custline in self.cust:
                    cw.writeline(custline % kwd)
            else:
                cw.writeline("conversion = Conversion.%s;" % (self.ct))
                if self.fromtype.localName == None:
                    if (to_lc != from_lc):
                        cw.writeline("return (%s)(%s)value;" % (to_lc, from_lc))
                    else:
                        cw.writeline("return (%s)value;" % to_lc)
                else:
                    if (to_lc != from_lc):
                        cw.writeline("return (%s)(%s)value;" % (to_lc, from_lc))
                    else:
                        cw.writeline("return (%s)value;" % to_lc)


class FromNull(From):
    def __init__(self):
        self.fromtype = alltypes["none"]
        self.cond = None
        self.cust = None
        self.ct = "Identity"
    def generate(self, cw, t, first):
        to_lc = t.lowercase
        to_cc = t.camelcase
        from_lc = self.fromtype.lowercase
        from_cc = self.fromtype.camelcase
        if first:
            cw.enter_block("if (value == null)")
        else:
            cw.else_block("if (value == null)")
        cw.writeline("conversion = Conversion.%s;" % (self.ct))
        cw.writeline("return null;")

class FromX(From):
    def __init__(self, fromtype, ct="Truncation", cust=None, cond=None):
        self.fromtype=alltypes[fromtype]
        self.cond=cond
        self.cust=cust
        self.ct = ct

class FromLimit(From):
    def __init__(self, fromtype, min=None, max=None, convType="Implicit"):
        self.fromtype = alltypes[fromtype]
        self.ct=convType
        self.cond=None
        self.cust = []

        self.cust.append("%(from_name)s %(cc_value)s = (%(from_name)s)%(value_name)s;")
        cust = "if ("
        if min:
            cust += "%(cc_value)s >= "
            cust += str(min)
            if max:
                cust += " && %(cc_value)s <= "
                cust += str(max)
        else:
            if max:
                cust += "%(cc_value)s <= "
                cust += str(max)
            else:
                raise ValueError("min or max must be defined")
        cust += ") {"
        self.cust.append(cust)
        self.cust.append("    conversion = Conversion.%(conversion)s;")
        self.cust.append("    return (%(to_name)s)%(cc_value)s;")
        self.cust.append("}")

class FromCall(From):
    def __init__(self, callName, type, callId):
        self.callName = callName
        self.callId = callId
        self.fromtype = alltypes["object"]
        self.cond = [ "Ops.TryGetAttr(value, "+ self.callId + ", out %s)"]
        self.cust = ["    %(value_name)s = Ops.Call(%(value_name)s);",
                     "    if (%(value_name)s is "+type+")",
                     "        return ("+type+")%(value_name)s;",
                    ]
                    
        for x in ['int', 'double', 'long', 'BigInteger']:
            if x == type: continue
            
            if type == "BigInteger":
                self.cust.append("    if (%(value_name)s is "+x+")")
                self.cust.append("        return BigInteger.Create(("+x+")%(value_name)s);")
            else:
                self.cust.append("    if (%(value_name)s is "+x+")")
                self.cust.append("        return ("+type+")("+x+")%(value_name)s;")
        
        self.cust.append("    throw Ops.TypeError(\""+callName+" returned non-"+ callName[2:-2]+"\");")
        self.ct = "Eval"    
            

class FromEnum(From):
    def __init__(self):
        self.fromtype = alltypes["Enum"]
        self.cond=None
        self.cust=["return TryConvertEnumTo%(to_cc)s((Enum)value, out conversion);"]
        self.ct = "None"

class compare_from_cls:
    def __init__(self, mytype):
        self.mytype = mytype
    def __call__(self, a, b):
        fromtype = alltypes[a.fromtype.lowercase]
        totype   = alltypes[b.fromtype.lowercase]
        if (fromtype.lowercase ==  self.mytype):
            if (totype.lowercase == self.mytype):
                return 0
            else:
                return -1
        else:
            if (totype.lowercase ==  self.mytype):
                return 1
            else:
                frprio = fromtype.priority
                toprio = totype.priority
                if isinstance(a, FromX):
                    frprio += 1000
                if isinstance(b, FromX):
                    toprio += 1000
                if isinstance(a, FromCall):
                    frprio += 2000
                if isinstance(b, FromCall):
                    toprio += 2000
                return cmp(frprio,toprio)

def compare_from(a,b):
    fromtype = alltypes[a.fromtype.lowercase]
    totype   = alltypes[b.fromtype.lowercase]
    print "<" + fromtype.lowercase + ", " + totype.lowercase + ">"
    return cmp(fromtype.priority, totype.priority)

def compare_to(a,b):
    return cmp(alltypes[a.to.lowercase].priority, alltypes[b.to.lowercase].priority)

#############################################################

conversions = [
    ToBool("bool", [
        From("bool", ct="Identity"),
        From("int", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("ExtensibleInt", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return ei.value != 0;",
                ]
            ),
        From("char", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("byte", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("sbyte", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("short", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("ushort", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("BigInteger", None, [
                "conversion = Conversion.%(conversion)s;",
                "return (%(from_name)s)%(value_name)s != BigInteger.Zero;",
                ]
             ),
        From("uint", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("long", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("ulong", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("float", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0.0;",
                ]
            ),
        From("double", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0.0;",
                ]
            ),
        From("decimal", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return (%(from_name)s)%(value_name)s != 0;",
                ]
            ),
        From("string", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return ((%(from_name)s)%(value_name)s).Length != 0;",
                ]
            ),
        From("Complex64", ct="NonStandard", cust=[
                    "conversion = Conversion.%(conversion)s;",
                    "return !((%(from_name)s)%(value_name)s).IsZero;",
                ]
            ),
        FromEnum(),
        ]
    ),
    To("int", [
        From("int", ct="Identity"),
        From("ExtensibleInt", None,
                ["conversion = Conversion.%(conversion)s;",
                 "return ei.value;"]),
        From("char"),
        From("byte"),
        From("sbyte"),
        From("short"),
        From("ushort"),
        From("BigInteger", None,
                ["int res;",
                 "if (bi.AsInt32(out res)) {",
                 "    conversion = Conversion.%(conversion)s;",
                 "    return res;",
                 "}"
                 ]
             ),
        From("bool", None,
                ["conversion = Conversion.%(conversion)s;",
                 "return ((%(from_name)s)%(value_name)s) ? 1 : 0;",
                 ],
                "NonStandard"),
        FromLimit("uint", None, "Int32.MaxValue"),
        FromLimit("long", "Int32.MinValue", "Int32.MaxValue"),
        FromLimit("ulong", None, "Int32.MaxValue"),
#        FromX("float"),
#        FromX("double"),
        FromLimit("decimal", None, "Int32.MaxValue"),
        FromEnum(),
        FromLimit("double", "Int32.MinValue", "Int32.MaxValue", "Truncation"),
        From("ExtensibleFloat", 
                ["/*%s*/ ef.value >= Int32.MinValue", "/*%s*/ef.value <= Int32.MaxValue"],
                ["    return (int)ef.value;"], 
                "Truncation"),
        FromCall("__int__", 'int', 'SymbolTable.ConvertToInt'),     
        ]
    ),
    To("long", [
        From("int"),
        From("ExtensibleInt", None,
                ["conversion = Conversion.%(conversion)s;",
                 "return (%(to_name)s)ei.value;"]
                ),
        From("long", ct="Identity"),
        From("char"),
        From("byte"),
        From("sbyte"),
        From("short"),
        From("uint"),
        From("ushort"),
        From("BigInteger", None,
                ["long res;",
                "if (bi.AsInt64(out res)) {",
                "    conversion = Conversion.%(conversion)s;",
                "    return res;",
                "}"]
                ),
        From("bool", None,
                ["conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? 1 : 0;"],
                "NonStandard"),
        FromLimit("ulong", None, "Int64.MaxValue"),
#        FromX("float"),
#        FromX("double"),
        FromLimit("decimal", None, "Int64.MaxValue"),
        FromEnum(),
        ]
    ),
    To("double", [
        From("int"),
        From("double", ct="Identity"),
        From("long"),
        From("char"),
        From("byte"),
        From("sbyte"),
        From("short"),
        From("uint"),
        From("ulong"),
        From("ushort"),
        From("float"),
        From("ExtensibleInt", None,
                ["conversion = Conversion.%(conversion)s;",
                "return (%(to_name)s)ei.value;"]
                ),
        From("ExtensibleFloat", None,
                ["conversion = Conversion.%(conversion)s;",
                "return (%(to_name)s)ef.value;"]
                ),
        From("BigInteger", None,
                ["conversion = Conversion.%(conversion)s;",
                "double res;",
                "if (bi.TryToFloat64(out res)) return res;"],
                "Implicit"),
        From("bool", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? 1.0 : 0.0;",
        ], ct="NonStandard"),
        FromX("decimal"),
        FromCall("__float__", 'double', 'SymbolTable.ConvertToFloat'),     
        ]
    ),
    To("char", [
        From("char", ct="Identity"),
        From("string", ['/*%s*/str.Length == 1'], ["    return (%(to_name)s)str[0];"]),
        FromLimit("int", "0", "0xFFFF"),
        FromX("byte"),
        FromLimit("sbyte", "0"),
        FromLimit("short", "0"),
        FromX("ushort"),
        FromLimit("uint", None, "0xFFFF"),
        FromLimit("long", "0", "0xFFFF"),
        FromLimit("ulong", None, "0xFFFF"),
#        FromX("float"),
#        FromX("double"),
        FromLimit("decimal", "0", "0xFFFF"),
        ]
    ),
    To("byte", [
        From("int", ['%s >= Byte.MinValue', '%s <= Byte.MaxValue']),
        From("byte", ct="Identity"),
        FromX("bool", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? (%(to_name)s)1 : (%(to_name)s)0;",
        ]),
        FromLimit("char", "Byte.MinValue"),
        FromLimit("sbyte", "Byte.MinValue"),
        FromLimit("short", "Byte.MinValue", "Byte.MaxValue"),
        FromLimit("ushort", None, "Byte.MaxValue"),
        FromLimit("uint", None, "Byte.MaxValue"),
        FromLimit("long", "Byte.MinValue", "Byte.MaxValue"),
        FromLimit("ulong", None, "Byte.MaxValue"),
        From("BigInteger", None,
                [
                "if (bi >= BigInteger.Create(Byte.MinValue) &&",
                "    bi <= BigInteger.Create(Byte.MaxValue)) {",
                "    conversion = Conversion.Implicit;",
                "    return (byte)(int)bi;",
                "}"]
                ),
#        FromX("float"),
#        FromX("double"),
        FromLimit("decimal", "Byte.MinValue", "Byte.MaxValue"),
        FromEnum(),
        ]
    ),
    To("sbyte", [
        From("int", ['%s >= SByte.MinValue', '%s <= SByte.MaxValue']),
        From("sbyte", ct="Identity"),
        FromX("bool", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? (%(to_name)s)1 : (%(to_name)s)0;",
        ]),
        FromLimit("char", None, "'\\xFF'"),
        FromLimit("byte", None, "SByte.MaxValue"),
        FromLimit("short", "SByte.MinValue", "SByte.MaxValue"),
        FromLimit("ushort", None, "SByte.MaxValue"),
        FromLimit("uint", None, "SByte.MaxValue"),
        FromLimit("long", "SByte.MinValue", "SByte.MaxValue"),
        FromLimit("ulong", None, "(ulong)SByte.MaxValue"),
        From("BigInteger", None,
                [
                "if (bi >= BigInteger.Create(SByte.MinValue) &&",
                "    bi <= BigInteger.Create(SByte.MaxValue)) {",
                "    conversion = Conversion.Implicit;",
                "    return (sbyte)(int)bi;",
                "}"]
                ),
#        FromX("float"),
#        FromX("double"),
        FromLimit("decimal", "SByte.MinValue", "SByte.MaxValue"),
        FromEnum(),
        ]
    ),
    To("short", [
        FromLimit("int", "Int16.MinValue", "Int16.MaxValue"),
        From("byte"),
        From("sbyte"),
        From("short", ct="Identity"),
        From("bool", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? (%(to_name)s)1 : (%(to_name)s)0;",
        ], ct="NonStandard"),
        FromLimit("char", None, "'\\x7FFF'"),
        FromLimit("ushort", None, "Int16.MaxValue"),
        FromLimit("uint", None, "Int16.MaxValue"),
        FromLimit("long", "Int16.MinValue", "Int16.MaxValue"),
        FromLimit("ulong", None, "(ulong)Int16.MaxValue"),
        From("BigInteger", None,
                [
                "if (bi >= BigInteger.Create(Int16.MinValue) &&",
                "    bi <= BigInteger.Create(Int16.MaxValue)) {",
                "    conversion = Conversion.Implicit;",
                "    return (short)(int)bi;",
                "}"]
                ),
#        FromX("float"),
#        FromX("double"),
        FromLimit("decimal", "Int16.MinValue", "Int16.MaxValue"),
        FromEnum(),
        ]
    ),
    To("uint", [
        From("int", ['%s >= UInt32.MinValue']),
        From("char"),
        From("byte"),
        From("uint", ct="Identity"),
        From("ushort"),
        From("BigInteger", None,
                ["uint res;",
                "if (bi.AsUInt32(out res)) {",
                "    conversion = Conversion.%(conversion)s;",
                "    return res;",
                "}"]
                ),
        From("ExtensibleInt", None,
                ["conversion = Conversion.%(conversion)s;",
                "return (%(to_name)s)ei.value;"]),
        From("bool", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? 1u : 0u;",
        ], ct="NonStandard"),
        FromLimit("sbyte", "0"),
        FromLimit("short", "0"),
        FromLimit("long", "UInt32.MinValue", "UInt32.MaxValue"),
        FromLimit("ulong", None, "UInt32.MaxValue"),
#        FromX("float"),
#        FromX("double"),
        FromLimit("decimal", "UInt32.MinValue", "UInt32.MaxValue"),
        FromEnum(),
        ]
    ),
    To("ulong", [
        From("int", ['%s >= 0']),
        From("long", ['%s >= 0']),
        From("char"),
        From("byte"),
        From("uint"),
        From("ulong", ct="Identity"),
        From("ushort"),
        From("ExtensibleInt", None,
                [
                "if (ei.value >= 0) {",
                "    conversion = Conversion.%(conversion)s;",
                "    return (%(to_name)s)ei.value;",
                "}"]),
        From("BigInteger", None,
                [
                "ulong res;",
                "if (bi.AsUInt64(out res)) {",
                "    conversion = Conversion.%(conversion)s;",
                "    return res;",
                "}"]),
        From("bool", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? 1ul : 0ul;",
        ], ct="NonStandard"),
        FromLimit("sbyte", "0"),
        FromLimit("short", "0"),
#        FromX("float"),
#        FromX("double"),
        FromLimit("decimal", "UInt64.MinValue", "UInt64.MaxValue"),
        FromEnum(),
        ]
    ),
    To("ushort", [
        FromLimit("int", "UInt16.MinValue", "UInt16.MaxValue"),
        From("char"),
        From("byte"),
        From("ushort", ct="Identity"),
        From("bool", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? (%(to_name)s)1 : (%(to_name)s)0;",
        ], ct="NonStandard"),
        FromLimit("sbyte", "0"),
        FromLimit("short", "0"),
        FromLimit("uint", None, "UInt16.MaxValue"),
        FromLimit("long", "UInt16.MinValue", "UInt16.MaxValue"),
        FromLimit("ulong", None, "UInt16.MaxValue"),
        From("BigInteger", None,
                [
                "if (bi >= BigInteger.Create(UInt16.MinValue) &&",
                "    bi <= BigInteger.Create(UInt16.MaxValue)) {",
                "    conversion = Conversion.Implicit;",
                "    return (ushort)(int)bi;",
                "}"]
                ),
#        FromX("float"),
#        FromX("double"),
        FromLimit("decimal", "UInt16.MinValue", "UInt16.MaxValue"),
        FromEnum(),
        ]
    ),
    To("float", [
        From("int"),
        From("long"),
        From("char"),
        From("byte"),
        From("sbyte"),
        From("short"),
        From("uint"),
        From("ulong"),
        From("ushort"),
        From("float", ct="Identity"),
        From("ExtensibleFloat", None,
                ["conversion = Conversion.%(conversion)s;",
                "return (%(to_name)s)ef.value;"]),
        From("ExtensibleInt", None,
                ["conversion = Conversion.%(conversion)s;",
                "return (%(to_name)s)ei.value;"]),
        From("BigInteger", None,
                [
                "conversion = Conversion.%(conversion)s;",
                "double res;",
                "if (bi.TryToFloat64(out res)) return (float)res;"],
                "Implicit"),
        From("bool", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? (%(to_name)s)1.0 : (%(to_name)s)0.0;",
        ], ct="NonStandard"),
        FromLimit("double", "Single.MinValue", "Single.MaxValue"),
        FromX("decimal"),
        ]
    ),
    To("decimal", [
        From("int"),
        From("long"),
        From("char"),
        From("byte"),
        From("sbyte"),
        From("short"),
        From("uint"),
        From("ulong"),
        From("ushort"),
        From("decimal", ct="Identity"),
        From("bool", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return ((%(from_name)s)%(value_name)s) ? (%(to_name)s)1 : (%(to_name)s)0;",
        ], ct="NonStandard"),
        #FromX("float"),
        #FromX("double"),
        ]
    ),
    ToInteger("BigInteger", [
        From("BigInteger", ct="Identity"),
        From("int", ct="Identity"),
        From("long", ct="Identity"),
        FromCall("__long__", "BigInteger", 'SymbolTable.ConvertToLong') 
        ],
        gendefault=False
    ),
    ToComplex("Complex64", [
        From("Complex64", ct="Identity"),
        From("ExtensibleComplex", None,
                ["conversion = Conversion.Implicit;",
                 "return ec.value;"]),
        From("double", None,
            ["conversion = Conversion.%(conversion)s;",
            "return (%(to_name)s)Complex64.MakeReal((%(from_name)s)%(value_name)s);"]
            ),
        FromCall("__complex__", 'Complex64', 'SymbolTable.ConvertToComplex'),     
        ],

        gendefault=False
    ),
    To("string", [
        FromNull(),
        From("string", ct="Identity"),
        From("char", ct="Identity", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return Ops.Char2String((%(from_name)s)%(value_name)s);"
            ]),
        From("ExtensibleString", cust=[
                "conversion = Conversion.%(conversion)s;",
                "return es.Value;"
            ])
        ]
    ),
    ToDelegate("Delegate", [], gendefault=False),
    ToEnumerator("IEnumerator", [
        From("IEnumerator", ct="Identity"),
        ],
        gendefault=False
    ),
    ToType("Type", [], gendefault=False),
]

for c in conversions:
    c.sort()

conversions.sort(compare_to)

def converter_generator(cw):
    cw.writeline("//")
    cw.writeline("// \"Try\" conversion methods")
    cw.writeline("//")
    for t in conversions:
        t.generate_tryconvertto(cw)


    cw.writeline("//")
    cw.writeline("// Entry point into \"Try\" conversions")
    cw.writeline("//")

    cw.enter_block("public static object TryConvertWorker(object value, Type to, out Conversion conversion)")
    cw.writeline("")

    cw.writeline("Type from = value.GetType();")
    cw.enter_block("if (from == to)")
    cw.writeline("conversion = Conversion.Identity;")
    cw.writeline("return value;")
    cw.exit_block()

    cw.enter_block("if (to == ObjectType)")
    cw.writeline("conversion = Conversion.Implicit;")
    cw.writeline("return value;")
    cw.exit_block()

    total = len(conversions)
    for i in xrange(total):
        t = conversions[i]
        t.generate_if(cw, i == 0)


    cw.else_block("if (to == ArrayListType)")
    cw.writeline("return TryConvertToArrayList(value, out conversion);")

    cw.else_block("if (to == HashtableType)")
    cw.writeline("return TryConvertToHashtable(value, out conversion);")

    cw.else_block("if (to.IsArray)")
    cw.writeline("return TryConvertToArray(value, to, out conversion);")

    cw.else_block("if (to.IsGenericType)")
    cw.writeline("Type genTo = to.GetGenericTypeDefinition();")

    cw.enter_block("if (genTo == IListOfTType)")
    cw.writeline("object res = TryConvertToIListT(value, to.GetGenericArguments(), out conversion);")
    cw.writeline("if (conversion != Conversion.None) return res;")

    cw.else_block("if (genTo == ListOfTType)")
    cw.writeline("return TryConvertToListOfT(value, to.GetGenericArguments(), out conversion);")

    cw.else_block("if (genTo == IEnumeratorOfT)")
    cw.writeline("object res = TryConvertToIEnumeratorOfT(value, to.GetGenericArguments(), out conversion);")
    cw.writeline("if (conversion != Conversion.None) return res;")

    cw.else_block("if (genTo == IDictOfTType)")
    cw.writeline("object res = TryConvertToIDictOfT(value, to.GetGenericArguments(), out conversion);")
    cw.writeline("if (conversion != Conversion.None) return res;")
    cw.exit_block()

    cw.exit_block()

    cw.enter_block("if (from.IsValueType)")
    cw.enter_block("if (to.IsEnum)")
    cw.enter_block("if (value is int)")
    cw.writeline("int IntValue = (int)value;")
    cw.enter_block("if (IntValue == 0)")
    cw.writeline("conversion = Conversion.Implicit;")
    cw.writeline("return 0;")
    cw.exit_block()
    cw.exit_block()
    cw.exit_block()
    cw.enter_block("if (to == ValueTypeType)")
    cw.writeline("conversion = Conversion.Implicit;")
    cw.writeline("return (System.ValueType)value;")
    cw.exit_block()


    cw.exit_block()
    cw.enter_block("if (to.IsInstanceOfType(value))")
    cw.writeline("conversion = Conversion.Identity;")
    cw.writeline("return value;")
    cw.exit_block()

    cw.writeline("")
    cw.writeline("// check for implicit conversions ")
    cw.writeline("ReflectedType toType = Ops.GetDynamicTypeFromType(to) as ReflectedType;")
    cw.writeline("ReflectedType dt = Ops.GetDynamicType(value) as ReflectedType;")
    cw.writeline("")
    cw.enter_block("if (toType != null && dt != null)")
    cw.writeline("object res = dt.TryConvertTo(value, toType, out conversion);")
    cw.enter_block("if (conversion != Conversion.None)")
    cw.writeline("return res;")
    cw.exit_block()
    cw.writeline("")
    cw.writeline("res = toType.TryConvertFrom(value, out conversion);")
    cw.enter_block("if (conversion != Conversion.None)")
    cw.writeline("return res;")
    cw.exit_block()
    cw.exit_block()
    cw.writeline("")

    cw.writeline("conversion = Conversion.None;")
    cw.writeline("return null;")
    cw.exit_block()
    cw.writeline()

    cw.writeline("//")
    cw.writeline("// \"throw\" conversion methods")
    cw.writeline("//")

    for t in conversions:
        t.generate_convertto(cw)


    cw.writeline("//")
    cw.writeline("// Entry point into \"throw\" conversion")
    cw.writeline("//")

    cw.enter_block("public static object Convert(object value, Type to)")
    cw.writeline("Conversion conversion;")
    cw.writeline("object val = TryConvert(value, to, out conversion);")
    cw.enter_block("if (conversion == Conversion.None)")
    cw.writeline("throw Ops.TypeError(\"No conversion from {0} to {1}\", value, to);")
    cw.exit_block()
    cw.writeline("return val;")
    cw.exit_block()

class EnumType:
    def __init__(self, name, priority, typecode, min, max):
        self.name = name
        self.priority = priority
        self.typecode = typecode
        self.min = min
        self.max = max
    def __repr__(self):
        return "EnumType %s (%d)" % (self.name, self.priority)

enum_types = [
    EnumType("short", 30, "Int16", -32768, 32767),
    EnumType("sbyte", 60, "SByte", -128, 127),
    EnumType("ulong", 50, "UInt64", 0, 18446744073709551615),
    EnumType("int", 10, "Int32", -2147483648, 2147483647),
    EnumType("ushort", 60, "UInt16", 0, 65535),
    EnumType("long", 20, "Int64", -9223372036854775808, 9223372036854775807),
    EnumType("uint", 40, "UInt32", 0, 4294967295),
    EnumType("byte", 70, "Byte", 0, 255),
]

class compare_enum_types:
    def __init__(self, base):
        self.mytype = base

    def __call__(self, a, b):
        fromtype = a
        totype   = b
        if (fromtype.name == self.mytype):
            if (totype.name == self.mytype):
                return 0
            else:
                return -1
        else:
            if (totype.name == self.mytype):
                return 1
            else:
                frprio = fromtype.priority
                toprio = totype.priority
                return cmp(frprio,toprio)

def genenum(cw, et, v):
    cw.enter_block("private static %s TryConvertEnumTo%s(object value, out Conversion conversion)" % (et.name, et.typecode) )
    cw.writeline("conversion = Conversion.NonStandard;")
    cw.enter_block("switch (((Enum)value).GetTypeCode())")

    def zero_or_minval(x):
        if x.min == 0: return "0"
        return "%s.MinValue" % x.typecode

    def opt_cast(f, t):
        if f.name == "ulong" and t.name in ["short", "sbyte"]:
            return "(ulong)"
        return ""

    for i in v:
        cw.writeline("case TypeCode.%s:" % i.typecode)
        cw.indent += 1
        if et.name == i.name:
            cw.writeline("return (%s)(value);" % et.name)
        else:
            if i.min < et.min or i.max > et.max:
                cw.writeline("%s %sVal = (%s)value;" % (i.name, i.typecode, i.name))
                condition = "if ("
                space = False
                if i.min < et.min:
                    condition += "%sVal >= %s" % (i.typecode, zero_or_minval(et))
                    space = True
                if i.max > et.max:
                    if space: condition += " && "
                    condition += "%sVal <= %s%s.MaxValue" % (i.typecode, opt_cast(i, et), et.typecode)
                condition += ")"
                cw.enter_block(condition)
                cw.writeline("return (%s)%sVal;" % (et.name, i.typecode))
                cw.exit_block()
                cw.writeline("break;")
            else:
                cw.writeline("return (%s)(%s)(value);" % (et.name, i.name))
        cw.indent -= 1

    cw.exit_block()
    cw.writeline("conversion = Conversion.None;")
    cw.writeline("return 0;")
    cw.exit_block()
    cw.writeline()

def enum_converter_generator(cw):
    for et in enum_types:
        v = list(enum_types)
        v.sort(compare_enum_types(et.name))

        genenum(cw, et, v)

    v = list(enum_types)
    v.sort(compare_enum_types("int"))

    cw.enter_block("private static bool TryConvertEnumToBoolean(object value, out Conversion conversion)")
    cw.enter_block("switch (((Enum)value).GetTypeCode())")

    for i in v:
        cw.writeline("case TypeCode.%s:" % i.typecode)
        cw.writeline("    conversion = Conversion.NonStandard;")
        cw.writeline("    return (%s)value != 0;" % i.name)

    cw.writeline("default:")
    cw.writeline("    conversion = Conversion.None;")
    cw.writeline("    return false;")
    cw.exit_block()
    cw.exit_block()


def conversion_helper_generator(cw):
    cw.enter_block("public static bool HasConversion(Type t)")
    cw.writeline("if (t.IsArray) return true;")
    cw.writeline("if (t == typeof(ArrayList) || t == typeof(Hashtable) || t.IsGenericType) return true;")
    
    for type in alltypes.values():
        if type.lowercase == "none" or type.lowercase == "object" or type.lowercase=="object[]": continue
        
        if type.sealed:
            cw.writeline("if (t == typeof(%s)) return true;" %type.lowercase)
        else:
            cw.writeline("if (t == typeof(%s) || t.IsSubclassOf(typeof(%s))) return true;" % (type.lowercase,type.lowercase))
    cw.writeline("if (t.IsSubclassOf(typeof(ArrayList)) || t.IsSubclassOf(typeof(Hashtable))) return true;")
    cw.writeline("return false;")
    cw.exit_block()
    
CodeGenerator("conversion routines", converter_generator).doit()
CodeGenerator("enum conversions", enum_converter_generator).doit()
CodeGenerator("Conversion Helpers", conversion_helper_generator).doit()
