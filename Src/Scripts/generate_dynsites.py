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
MaxSiteArity = 8

def gsig(n):
    return ", ".join(["T%d" % i for i in range(n)] + ["TRet"])

def gsig_noret(n):
    return ", ".join(["T%d" % i for i in range(n)])

def gparms(size):
    if size == 0: return ''
    
    return ", " + ", ".join(["T%d arg%d" % (i,i) for i in range(size)])

def gargs(size):
    if size == 0: return ''
    
    return ", " + ", ".join(["arg%d" % i for i in range(size)])

def gargsindex(size):
    if size == 0: return ''
    
    return ", " + ", ".join(["args[%d]" % i for i in range(size)])

def gonlyargs(size):
    if size == 0: return ''
    
    return ", ".join(["arg%d" % i for i in range(size)])

easy_sites="""/// <summary>
/// Dynamic site - arity %(arity)d
/// </summary>
[GeneratedCode("DLR", "2.0")]
public struct DynamicSite<%(ts)s> {
    private CallSite<DynamicSiteTarget<CodeContext, %(ts)s>> _site;
    
    public DynamicSite(OldDynamicAction action) {
        _site = CallSite<DynamicSiteTarget<CodeContext, %(ts)s>>.Create(action);
    }

    public static DynamicSite<%(ts)s> Create(OldDynamicAction action) {
        return new DynamicSite<%(ts)s>(action);
    }

    public bool IsInitialized {
        get {
            return _site != null;
        }
    }
    
    public void EnsureInitialized(OldDynamicAction action) {
        if (_site == null) {
            Interlocked.CompareExchange(ref _site, CallSite<DynamicSiteTarget<CodeContext, %(ts)s>>.Create(action), null);
        }
    }

    public TRet Invoke(CodeContext context%(tparams)s) {
        return _site.Target(_site, context%(targs)s);
    }
}
"""

def gen_easy_sites(cw):
    for n in range(MaxSiteArity + 1):
        cw.write(easy_sites, ts = gsig(n), tparams = gparms(n), targs = gargs(n), arity = n)

site_targets = """/// <summary>
/// Dynamic site delegate type - arity %(arity)d
/// </summary>
[GeneratedCode("DLR", "2.0")]
public delegate TRet DynamicSiteTarget<%(ts)s>(CallSite site%(tparams)s);
"""

MaxTypes = MaxSiteArity + 2

def gen_dynamic_site_targets(cw):
    for n in range(MaxTypes):
        cw.write(site_targets, ts = gsig(n), tparams = gparms(n), arity = n)

def gen_site_target_maker(cw):
    for i in range(MaxTypes):
        cw.write("case %(length)d: return typeof(DynamicSiteTarget<%(targs)s>).MakeGenericType(types);", length = i + 1, targs = "," * i)

def gen_max_arity(cw):
    cw.write('internal const int MaximumArity = %d;' % MaxTypes)

mismatch = """// Mismatch detection - arity %(size)d
public static TRet Mismatch%(size)d<%(ts)s>(Matchmaker mm, %(params)s) {
    mm.Match = false;
    return default(TRet);
}
"""

def gen_matchmaker(cw):
    cw.write("//\n// Mismatch routines for dynamic sites\n//\n")
    for n in range(MaxSiteArity + 2):
        cw.write(mismatch, ts = gsig(n), size = n, params = "CallSite site" + gparms(n))

mismatch_void = """// Mismatch detection - arity %(size)d
public static void MismatchVoid%(size)d<%(ts)s>(Matchmaker mm, %(params)s) {
    mm.Match = false;
}
"""

def gen_void_matchmaker(cw):
    cw.write("//\n// Mismatch routines for dynamic sites with void return type\n//\n")
    for n in range(1, MaxSiteArity + 2):
        cw.write(mismatch_void, ts = gsig_noret(n), size = n, params = "CallSite site" + gparms(n))

splatcaller = """[Obsolete("used by generated code", true)]
public static object CallHelper%(size)d(CallSite<DynamicSiteTarget<object%(ts)s>> site, object[] args) {
    return site.Target(site%(args)s);
}
"""

def gen_splatsite(cw):
    cw.write("//\n// Splatting targets for dynamic sites\n//\n")
    for n in range(1, MaxSiteArity + 2):
        cw.write(splatcaller, size = n, ts = ", object" * n, args = gargsindex(n))

construction_helper="""[GeneratedCode("DLR", "2.0")]
public static DynamicSite<%(generic)s> CreateSimpleCallSite<%(generic)s>(ActionBinder binder) {
    return DynamicSite<%(generic)s>.Create(OldCallAction.Make(binder, %(args)d));
}
"""

ref_constr_helper="""[GeneratedCode("DLR", "2.0")]
public static void CreateSimpleCallSite<%(generic)s>(ActionBinder binder, ref DynamicSite<%(generic)s> site) {
    if (!site.IsInitialized) {
        site.EnsureInitialized(OldCallAction.Make(binder, %(args)d));
    }
}
"""

def gen_one_construction_helper(cw, helper, n):
    generic = gsig(n + 1)
    cw.write(helper, generic=generic, args=n)

def gen_construction_helpers(cw):
    for n in range(MaxSiteArity):
        gen_one_construction_helper(cw, construction_helper, n)
    for n in range(MaxSiteArity):
        gen_one_construction_helper(cw, ref_constr_helper, n)

update_target="""/// <summary>
/// Site update code - arity %(arity)d
/// </summary>
public static TRet Update%(arity)d<T, %(ts)s>(CallSite site%(tparams)s) where T : class {
    return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { %(targs)s });
}
"""

def gen_update_targets(cw):
    for n in range(1, MaxSiteArity + 2):
        cw.write(update_target, ts = gsig(n), tparams = gparms(n), targs = gonlyargs(n), arity = n)

update_target_void = """/// <summary>
/// Site update code - arity %(arity)d
/// </summary>
public static void UpdateVoid%(arity)d<T, %(ts)s>(CallSite site%(tparams)s) where T : class {
    ((CallSite<T>)site).UpdateAndExecute(new object[] { %(targs)s });
}
"""

def gen_void_update_targets(cw):
    for n in range(1, MaxSiteArity + 2):
        cw.write(update_target_void, ts = gsig_noret(n), tparams = gparms(n), targs = gonlyargs(n), arity = n)

def main():
    return generate(
        ("Predefined Update Targets", gen_update_targets),
        ("Predefined Void Update Targets", gen_void_update_targets),
        ("Dynamic Site Targets", gen_dynamic_site_targets),
        ("Easy Dynamic Sites", gen_easy_sites),
        ("Maximum Site Target Arity", gen_max_arity),
        ("Matchmaker", gen_matchmaker),
        ("Void Matchmaker", gen_void_matchmaker),
        ("SplatCallSite call helpers", gen_splatsite),
        ("Dynamic Sites Construction Helpers", gen_construction_helpers),
        ("Dynamic Site Target Type Maker", gen_site_target_maker)
    )

if __name__ == "__main__":
    main()
