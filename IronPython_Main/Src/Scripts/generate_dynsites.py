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
import clr
from System import *

#This is currently set as MAX_CALL_ARGS + 1 to include the target function as well
MaxSiteArity = 6

site_targets = """/// <summary>
/// Dynamic site delegate type with CodeContext passed in - arity %(arity)s
/// </summary>
[GeneratedCode("DLR", "2.0")]
public delegate TRet %(prefix)sDynamicSiteTarget<%(ts)s>(CallSite site, CodeContext context, %(tparams)s)%(constraints)s;"""


easy_sites="""/// <summary>
/// Dynamic site - arity %(arity)s
/// </summary>
[GeneratedCode("DLR", "2.0")]
public struct %(prefix)sDynamicSite<%(ts)s>%(constraints)s {
    private CallSite<%(prefix)sDynamicSiteTarget<%(ts)s>> _site;
    
    public %(prefix)sDynamicSite(DynamicAction action) {
        _site = CallSite<%(prefix)sDynamicSiteTarget<%(ts)s>>.Create(action);
    }

    public static %(prefix)sDynamicSite<%(ts)s> Create(DynamicAction action) {
        return new %(prefix)sDynamicSite<%(ts)s>(action);
    }

    public bool IsInitialized {
        get {
            return _site != null;
        }
    }
    
    public void EnsureInitialized(DynamicAction action) {
        if (_site == null) {
            Interlocked.CompareExchange(ref _site, CallSite<%(prefix)sDynamicSiteTarget<%(ts)s>>.Create(action), null);
        }
    }

    public TRet Invoke(CodeContext context, %(tparams)s) {
        return _site.Target(_site, context, %(targs)s);
    }
}"""

def gen_one_target(cw, size, arity, extra='', prefix='', constraints=''): 
    ts = ", ".join(["T%d" % i for i in range(size)] + ["TRet"])
    tparams = ", ".join(["T%d arg%d" % (i,i) for i in range(size)])
    targs = ", ".join(["arg%d" % i for i in range(size)])
    cw.write(site_targets, ts=ts, tparams=tparams, arity=arity, prefix=prefix, constraints=constraints)
    cw.writeline()

def gen_dynamic_site_targets(cw):
    for n in range(1, MaxSiteArity + 1):
        gen_one_target(cw, n, str(n))

    gen_one_target(cw, 1, 'variable based on Tuple size', prefix='Big', constraints = ' where T0 : Tuple')
    
def gen_one_easy_site(cw, size, arity, prefix='', constraints=''):
    ts = ", ".join(["T%d" % i for i in range(size)] + ["TRet"])
    tparams = ", ".join(["T%d arg%d" % (i,i) for i in range(size)])
    targs = ", ".join(["arg%d" % i for i in range(size)])
    cw.write(easy_sites, ts=ts, tparams=tparams, targs=targs, arity=arity, prefix=prefix, constraints=constraints)
    cw.writeline()
        
def gen_easy_sites(cw):
    for n in range(1, MaxSiteArity + 1):
        gen_one_easy_site(cw, n, str(n))
        
    gen_one_easy_site(cw, 1, 'variable based on Tuple size', prefix='Big', constraints = ' where T0 : Tuple')

maker = """\
public static Type MakeDynamicSiteType(params Type[] types) {
    Type genType;
    switch (types.Length) {
        %(cases)s
        default: return MakeBigDynamicSite(typeof(CallSite<>), typeof(BigDynamicSiteTarget<,>), types);
    }

    genType = genType.MakeGenericType(types);
    return typeof(CallSite<>).MakeGenericType(new Type[] { genType });
}
"""
MaxTypes = MaxSiteArity + 1
def gen_maker(cw):
    cases = []
    for i in range(1, MaxSiteArity + 1):
        ntypes = i+1
        targs = "," * (ntypes-1)
        line = "case %d: genType = typeof(DynamicSiteTarget<%s>); break;" % (ntypes, targs)
        cases.append(line)
    prefix = "\n        "
    cw.write(maker, maxtypes=MaxTypes, cases=prefix.join(cases))
    
def gen_execute(cw):
    cw.enter_block("internal static Type MakeDynamicSiteTargetType(Type/*!*/[] types)")
    cw.write('Type siteType;')
    cw.write('')
    cw.enter_block("switch (types.Length)")
    for i in range(1, MaxSiteArity + 1):
        cw.case_label("case %d: siteType = typeof(DynamicSiteTarget<%s>).MakeGenericType(types); break;" % (i+1, ','*(i)))
        cw.dedent()
    cw.case_label("default:")
    cw.write('Type tupleType = Tuple.MakeTupleType(ArrayUtils.RemoveLast(types));')
    cw.write('siteType = typeof(BigDynamicSiteTarget<,>).MakeGenericType(tupleType, types[types.Length - 1]);')
    cw.write('break;')
    cw.dedent()
    cw.exit_block()
    cw.write("return siteType;")
    cw.exit_block()

def gen_helpers(cw):
    cw.write('public static readonly int MaximumArity = %d;' % MaxTypes)
    cw.write('')
    gen_maker(cw)
    gen_execute(cw)

def gen_mismatch(cw, n, moda, strn, xparams, xargs, constraint):
    ts = ", ".join(["T%d" % i for i in range(n)] + ["TRet"])
    plst = ["CallSite site"] + xparams + ["T%d arg%d" % (i,i) for i in range(n)]
    params = ", ".join(plst)
    args = ", ".join(xargs + ["arg%d" % i for i in range(n)])

    cw.enter_block("public static TRet Mismatch%(mod)s%(strn)s<%(ts)s>(Matchmaker mm, %(params)s)%(constraint)s", ts = ts, mod=moda, strn=strn, params=params, constraint=constraint)
    cw.write("mm._match = false;")
    cw.write("return default(TRet);")
    cw.exit_block()

def gen_mismatch_group(cw, argprefix, paramprefix):
    cw.write("//\n// Mismatch routines for dynamic sites\n//\n")
    for i in range(1, MaxSiteArity + 1):
        cw.write("// Mismatch detection, arity %i" % i)
        gen_mismatch(cw, i, "", str(i), argprefix, paramprefix, "")
        
    cw.write("// Mismatch detection, big")
    gen_mismatch(cw, 1, "Big", "", argprefix, paramprefix, " where T0 : Tuple")

def gen_matchmaker(cw):
    gen_mismatch_group(cw, ["CodeContext context",], ["context"])

construction_helper="""[GeneratedCode("DLR", "2.0")]
public static DynamicSite<%(generic)s> CreateSimpleCallSite<%(generic)s>(ActionBinder binder) {
    return DynamicSite<%(generic)s>.Create(CallAction.Make(binder, %(args)d));
}
"""
ref_constr_helper="""[GeneratedCode("DLR", "2.0")]
public static void CreateSimpleCallSite<%(generic)s>(ActionBinder binder, ref DynamicSite<%(generic)s> site) {
    if (!site.IsInitialized) {
        site.EnsureInitialized(CallAction.Make(binder, %(args)d));
    }
}
"""
def gen_one_construction_helper(cw, helper, n):
    generic = ", ".join(["T"+str(i) for i in range(n + 1)] + ["R"])
    cw.write(helper, generic=generic, args=n)

def gen_construction_helpers(cw):
    for n in range(MaxSiteArity):
        gen_one_construction_helper(cw, construction_helper, n)
    for n in range(MaxSiteArity):
        gen_one_construction_helper(cw, ref_constr_helper, n)

update_target="""/// <summary>
/// Site update code
/// </summary>
public static TRet Update%(suffix)s<%(ts)s>(CallSite site, CodeContext context, %(tparams)s)%(constraints)s {
    CallSite<%(prefix)sDynamicSiteTarget<%(ts)s>> s = (CallSite<%(prefix)sDynamicSiteTarget<%(ts)s>>)site;
    return (TRet)site.Binder.UpdateSiteAndExecute<%(prefix)sDynamicSiteTarget<%(ts)s>>(context, s, %(getargsarray)s);
}
"""

def gen_one_update_target(cw, target, size, getargsarray, prefix, suffix, constraints):
    ts = ", ".join(["T%d" % i for i in range(size)] + ["TRet"])
    tparams = ", ".join(["T%d arg%d" % (i,i) for i in range(size)])
    targs = ", ".join(["arg%d" % i for i in range(size)])
    cw.write(target, ts=ts, tparams=tparams, targs=targs, getargsarray=getargsarray % targs, prefix=prefix, suffix=suffix, constraints=constraints)
        
def gen_update_targets(cw):
    for n in range(1, MaxSiteArity + 1):
        gen_one_update_target(cw, update_target, n, 'new object[] { %s }', '', str(n), '')

    gen_one_update_target(cw, update_target, 1, 'Tuple.GetTupleValues(%s)', 'Big', 'Big', ' where T0 : Tuple')

def main():
    return generate(
        ("Predefined Update Targets", gen_update_targets),
        ("Dynamic Site Targets", gen_dynamic_site_targets),
        ("Easy Dynamic Sites", gen_easy_sites),
        ("DynamicSiteHelpers", gen_helpers),
        ("Matchmaker", gen_matchmaker),
        ("Dynamic Sites Construction Helpers", gen_construction_helpers),
    )

if __name__ == "__main__":
    main()
