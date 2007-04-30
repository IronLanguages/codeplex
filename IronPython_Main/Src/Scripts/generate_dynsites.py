#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Permissive License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Permissive License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Permissive License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

from generate import CodeGenerator
import clr
from System import *

#This is currently set as MAX_CALL_ARGS + 1 to include the target function as well
MaxSiteArity = 6

site = """\
/// <summary>
/// Dynamic site delegate type with CodeContext passed in - arity %(n)d
/// </summary>
public delegate Tret DynamicSiteTarget<%(ts)s>(DynamicSite<%(ts)s> site, CodeContext context, %(tparams)s);

/// <summary>
/// Dynamic site using CodeContext passed into the Invoke method - arity %(n)d
/// </summary>
public class DynamicSite<%(ts)s> : DynamicSite {
    private DynamicSiteTarget<%(ts)s> _target;
    private RuleSet<DynamicSiteTarget<%(ts)s>> _rules;

    public DynamicSite(Action action)
        : base(action) {
        this._rules = RuleSet<DynamicSiteTarget<%(ts)s>>.EmptyRules;
        this._target = this._rules.GetOrMakeTarget(null);
    }

    public Tret Invoke(CodeContext context, %(tparams)s) {
        Validate(context);
        return _target(this, context, %(targs)s);
    }

    public Tret UpdateBindingAndInvoke(CodeContext context, %(tparams)s) {
        StandardRule<DynamicSiteTarget<%(ts)s>> rule = 
          context.LanguageContext.Binder.GetRule<DynamicSiteTarget<%(ts)s>>(Action, new object[] { %(targs)s });

        RuleSet<DynamicSiteTarget<%(ts)s>> newRules = _rules.AddRule(rule);
        if (newRules != _rules) {
            DynamicSiteTarget<%(ts)s> newTarget = newRules.GetOrMakeTarget(context);
            lock (this) {
                _rules = newRules;
                _target = newTarget;
            }
        }

        return rule.MonomorphicRuleSet.GetOrMakeTarget(context)(this, context, %(targs)s);
    }
}

/// <summary>
/// Dynamic site delegate type using cached CodeContext - arity %(n)d
/// </summary>
public delegate Tret FastDynamicSiteTarget<%(ts)s>(FastDynamicSite<%(ts)s> site, %(tparams)s);

/// <summary>
/// Dynamic site using cached CodeContext - arity %(n)d
/// </summary>
public class FastDynamicSite<%(ts)s> : FastDynamicSite {
    private FastDynamicSiteTarget<%(ts)s> _target;
    private RuleSet<FastDynamicSiteTarget<%(ts)s>> _rules;

    public FastDynamicSite(CodeContext context, Action action)
        : base(context, action) {
        this._rules = RuleSet<FastDynamicSiteTarget<%(ts)s>>.EmptyRules;
        this._target = this._rules.GetOrMakeTarget(null);
    }

    public Tret Invoke(%(tparams)s) {
        return _target(this, %(targs)s);
    }

    public Tret UpdateBindingAndInvoke(%(tparams)s) {
        StandardRule<FastDynamicSiteTarget<%(ts)s>> rule = 
          Context.LanguageContext.Binder.GetRule<FastDynamicSiteTarget<%(ts)s>>(Action, new object[] { %(targs)s });

        RuleSet<FastDynamicSiteTarget<%(ts)s>> newRules = _rules.AddRule(rule);
        if (newRules != _rules) {
            FastDynamicSiteTarget<%(ts)s> newTarget = newRules.GetOrMakeTarget(Context);
            lock (this) {
                _rules = newRules;
                _target = newTarget;
            }
        }

        return rule.MonomorphicRuleSet.GetOrMakeTarget(Context)(this, %(targs)s);
    }
}
"""



def gen_all(cw):
    for n in range(1, MaxSiteArity + 1):
        ts = ", ".join(["T%d" % i for i in range(n)] + ["Tret"])
        tparams = ", ".join(["T%d arg%d" % (i,i) for i in range(n)])
        targs = ", ".join(["arg%d" % i for i in range(n)])
        cw.write(site, ts=ts, tparams=tparams, targs=targs, n=n)
        cw.writeline()

CodeGenerator("DynamicSites", gen_all).doit()

maker = """\
public static Type Make%(mod)sDynamicSiteType(params Type[] types) {
    Type genType;
    switch (types.Length) {
        %(cases)s
        default:
            throw new ArgumentException("require 2-%(maxtypes)d types");
    }

    return genType.MakeGenericType(types);
}
"""
MaxTypes = MaxSiteArity + 1
def gen_maker(cw, mod):
    cases = []
    for i in range(1, MaxSiteArity + 1):
        ntypes = i+1
        targs = "," * (ntypes-1)
        line = "case %d: genType = typeof(%sDynamicSite<%s>); break;" % (ntypes, mod, targs)
        cases.append(line)
    prefix = "\n        "
    cw.write(maker, mod=mod, maxtypes=MaxTypes, cases=prefix.join(cases))
    
def gen_method(cw, n, mod, xparams, xargs):
    ts = ", ".join(["T%d" % i for i in range(n)] + ["Tret"])
    plst = ["%sDynamicSite<%s> site" % (mod, ts)] + xparams + ["T%d arg%d" % (i,i) for i in range(n)]
    params = ", ".join(plst)
    args = ", ".join(xargs + ["arg%d" % i for i in range(n)])
    
    cw.enter_block("public Tret %(mod)sInvoke%(n)d(%(params)s)", mod=mod, n=n, params=params)
    cw.write("return site.UpdateBindingAndInvoke(%(args)s);", args=args)
    cw.exit_block()


def gen_uninitialized_type(cw):
    ts = ", ".join(["T%d" % i for i in range(MaxSiteArity)] + ["Tret"])
    cw.enter_block("private class UninitializedTargetHelper<%(ts)s>", ts=ts)
    
    for i in range(1, MaxSiteArity + 1):
        gen_method(cw, i, "", ["CodeContext context",], ["context"])
        gen_method(cw, i, "Fast", [], [])

    
    cw.exit_block()

uninitialized_helper = """\
public static Delegate MakeUninitialized%(mod)sTarget(Type targetType) {
    List<Type> types = new List<Type>(targetType.GetGenericArguments());
    int argCount = types.Count - 1;
    while (types.Count < %(maxtypes)d) types.Insert(argCount, typeof(object));
    Type dType = typeof(UninitializedTargetHelper<%(commas)s>).MakeGenericType(types.ToArray());
    return Delegate.CreateDelegate(targetType, Activator.CreateInstance(dType), "%(mod)sInvoke"+argCount);
}
"""

def gen_uninitialized_helper(cw, mod):
    commas = ','*(MaxTypes-1)
    cw.write(uninitialized_helper, mod=mod, maxtypes=MaxTypes, commas=commas)


def gen_helpers(cw):
    gen_maker(cw, "")
    gen_maker(cw, "Fast")
    
    gen_uninitialized_type(cw)
    
    gen_uninitialized_helper(cw, "")
    gen_uninitialized_helper(cw, "Fast")
    
    
    
CodeGenerator("DynamicSiteHelpers", gen_helpers).doit()

    
