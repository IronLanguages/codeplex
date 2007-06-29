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

site = """/// <summary>
/// Dynamic site delegate type with CodeContext passed in - arity %(arity)s
/// </summary>
public delegate Tret %(prefix)sDynamicSiteTarget<%(ts)s>(%(prefix)sDynamicSite<%(ts)s> site, CodeContext context, %(tparams)s) %(constraints)s;

/// <summary>
/// Dynamic site using CodeContext passed into the Invoke method - arity %(arity)s
/// </summary>
public class %(prefix)sDynamicSite<%(ts)s> : DynamicSite %(constraints)s {
    private %(prefix)sDynamicSiteTarget<%(ts)s> _target;
    private RuleSet<%(prefix)sDynamicSiteTarget<%(ts)s>> _rules;

    public %(prefix)sDynamicSite(Action action)
        : base(action) {
        this._rules = RuleSet<%(prefix)sDynamicSiteTarget<%(ts)s>>.EmptyRules;
        this._target = this._rules.GetOrMakeTarget(null);
    }

    public Tret Invoke(CodeContext context, %(tparams)s) {
        Validate(context);
        return _target(this, context, %(targs)s);
    }

    public Tret UpdateBindingAndInvoke(CodeContext context, %(tparams)s) {
        StandardRule<%(prefix)sDynamicSiteTarget<%(ts)s>> rule = 
          context.LanguageContext.Binder.GetRule<%(prefix)sDynamicSiteTarget<%(ts)s>>(Action, %(getargsarray)s);

#if DEBUG
        if (context.LanguageContext.Engine.Options.FastEvaluation) {
            DynamicSiteHelpers.InsertArguments(context.Scope, %(targs)s);
            bool result = (bool)rule.Test.Evaluate(context);
            Debug.Assert(result);
            return (Tret)rule.Target.Execute(context);
        }
#endif
        RuleSet<%(prefix)sDynamicSiteTarget<%(ts)s>> newRules = _rules.AddRule(rule);
        if (newRules != _rules) {
            %(prefix)sDynamicSiteTarget<%(ts)s> newTarget = newRules.GetOrMakeTarget(context);
            lock (this) {
                _rules = newRules;
                _target = newTarget;
            }
            if (newRules != EmptyRuleSet<%(prefix)sDynamicSiteTarget<%(ts)s>>.FixedInstance) return newTarget(this, context, %(targs)s);
        }

        return rule.MonomorphicRuleSet.GetOrMakeTarget(context)(this, context, %(targs)s);
    }
}

/// <summary>
/// Dynamic site delegate type using cached CodeContext - arity %(arity)s
/// </summary>
public delegate Tret %(prefix)sFastDynamicSiteTarget<%(ts)s>(%(prefix)sFastDynamicSite<%(ts)s> site, %(tparams)s) %(constraints)s;

/// <summary>
/// Dynamic site using cached CodeContext - arity %(arity)s
/// </summary>
public class %(prefix)sFastDynamicSite<%(ts)s> : FastDynamicSite %(constraints)s {
    private %(prefix)sFastDynamicSiteTarget<%(ts)s> _target;
    private RuleSet<%(prefix)sFastDynamicSiteTarget<%(ts)s>> _rules;

    public %(prefix)sFastDynamicSite(CodeContext context, Action action)
        : base(context, action) {
        this._rules = RuleSet<%(prefix)sFastDynamicSiteTarget<%(ts)s>>.EmptyRules;
        this._target = this._rules.GetOrMakeTarget(null);
    }

    public Tret Invoke(%(tparams)s) {
        return _target(this, %(targs)s);
    }

    public Tret UpdateBindingAndInvoke(%(tparams)s) {
        StandardRule<%(prefix)sFastDynamicSiteTarget<%(ts)s>> rule = 
          Context.LanguageContext.Binder.GetRule<%(prefix)sFastDynamicSiteTarget<%(ts)s>>(Action, %(getargsarray)s);

#if DEBUG
        if (Context.LanguageContext.Engine.Options.FastEvaluation) {
            DynamicSiteHelpers.InsertArguments(Context.Scope, %(targs)s);
            bool result = (bool)rule.Test.Evaluate(Context);
            Debug.Assert(result);
            return (Tret)rule.Target.Execute(Context);
        }
#endif
        RuleSet<%(prefix)sFastDynamicSiteTarget<%(ts)s>> newRules = _rules.AddRule(rule);
        if (newRules != _rules) {
            %(prefix)sFastDynamicSiteTarget<%(ts)s> newTarget = newRules.GetOrMakeTarget(Context);
            lock (this) {
                _rules = newRules;
                _target = newTarget;
            }
            return newTarget(this, %(targs)s);
        }

        return rule.MonomorphicRuleSet.GetOrMakeTarget(Context)(this, %(targs)s);
    }
}"""

def gen_one(cw, size, arity, extra='', getargsarray='new object[] { %s }', prefix='', constraints=''): 
    ts = ", ".join(["T%d" % i for i in range(size)] + ["Tret"])
    tparams = ", ".join(["T%d arg%d" % (i,i) for i in range(size)])
    targs = ", ".join(["arg%d" % i for i in range(size)])
    cw.write(site, ts=ts, tparams=tparams, targs=targs, arity=arity, getargsarray=getargsarray % targs, prefix=prefix, extra=extra, constraints=constraints)
    cw.writeline()
        
def gen_all(cw):
    for n in range(1, MaxSiteArity + 1):
        gen_one(cw, n, str(n))
        
    gen_one(cw, 1, 'variable based on Tuple size', getargsarray='NewTuple.GetTupleValues(%s)', prefix='Big', constraints = 'where T0 : NewTuple')
   

CodeGenerator("DynamicSites", gen_all).doit()

maker = """\
public static Type Make%(mod)sDynamicSiteType(params Type[] types) {
    Type genType;
    switch (types.Length) {
        %(cases)s
        default:
            return MakeBig%(mod)sDynamicSiteType(types);
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

executor = """\
StandardRule<DynamicSiteTarget<%(typeargs)s>> rule%(k)d = 
    binder.GetRule<DynamicSiteTarget<%(typeargs)s>>(action, args);
result = (bool)rule%(k)d.Test.Evaluate(binder.Context);
Debug.Assert(result);
return rule%(k)d.Target.Execute(binder.Context);"""

def gen_execute(cw):
    cw.enter_block("public static object Execute(CodeContext context, ActionBinder binder, Action action, params object[] args)")
    cw.enter_block('for (int i = 0; i < args.Length; i++)')
    cw.write('binder.Context.Scope.SetName(SymbolTable.StringToId("$arg" + i.ToString()), args[i]);')
    cw.exit_block()
    cw.write('bool result;')
    cw.enter_block("switch (args.Length)")
    for i in range(MaxSiteArity):
        cw.case_label("case %d:" % (i+1))
        typeargs = ", ".join(['object'] * (i+2))
        cw.write(executor % dict(typeargs=typeargs,k=i+1))
        cw.dedent()
    cw.exit_block()
    cw.write('throw new ArgumentException("requires 1-6 arguments");')
    cw.exit_block()
    cw.write('')


def gen_helpers(cw):
    
    cw.write('public static readonly int MaximumArity = %d;' % MaxTypes)
    cw.write('')
    
    gen_maker(cw, "")
    gen_maker(cw, "Fast")
    
    gen_execute(cw)
    
    gen_uninitialized_type(cw)
    
    gen_uninitialized_helper(cw, "")
    gen_uninitialized_helper(cw, "Fast")
    
    
    
CodeGenerator("DynamicSiteHelpers", gen_helpers).doit()

    
