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

MaxSiteArity = 14

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

def gargs_index(size):
    if size == 0: return ''
    return ", " + ", ".join(["args[%d]" % i for i in range(size)])

def gargs_indexwithcast(size):
    if size == 0: return ''
    return ", " + ", ".join(["(T%d)args[%d]" % (i, i) for i in range(size)])

def gonlyargs(size):
    if size == 0: return ''
    return ", ".join(["arg%d" % i for i in range(size)])

numbers = {
     0 : ( 'no',        ''            ),
     1 : ( 'one',       'first'       ),
     2 : ( 'two',       'second'      ),
     3 : ( 'three',     'third'       ),
     4 : ( 'four',      'fourth'      ),
     5 : ( 'five',      'fifth'       ),
     6 : ( 'six',       'sixth'       ),
     7 : ( 'seven',     'seventh'     ),
     8 : ( 'eight',     'eighth'      ),
     9 : ( 'nine',      'ninth'       ),
    10 : ( 'ten',       'tenth'       ),
    11 : ( 'eleven',    'eleventh'    ),
    12 : ( 'twelve',    'twelfth'     ),
    13 : ( 'thirteen',  'thirteenth'  ),
    14 : ( 'fourteen',  'fourteenth'  ),
    15 : ( 'fifteen',   'fifteenth'   ),
    16 : ( 'sixteen',   'sixteenth'   ),
}

def gsig_1(n):
    return ", ".join(["T%d" % i for i in range(1, n + 1)])
def gparams_1(n):
    return ", ".join(["T%d arg%d" % (i, i) for i in range(1, n + 1)])
def gsig_1_result(n):
    return ", ".join(["T%d" % i for i in range(1, n + 1)] + ['TResult'])

def generate_one_action_type(cw, n):
    cw.write("""
/// <summary>
/// Encapsulates a method that takes %(alpha)s parameters and does not return a value.
/// </summary>""", alpha = numbers[n][0])

    for i in range(1, n + 1):
        cw.write('/// <typeparam name="T%(number)d">The type of the %(alphath)s parameter of the method that this delegate encapsulates.</typeparam>', number = i, alphath = numbers[i][1])
    for i in range(1, n + 1):
        cw.write('/// <param name="arg%(number)d">The %(alphath)s parameter of the method that this delegate encapsulates.</param>', number = i, alphath = numbers[i][1])
    if n > 2:
        cw.write('[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]')
    cw.write("public delegate void Action<%(gsig)s>(%(gparms)s);", gsig = gsig_1(n), gparms = gparams_1(n))

def generate_one_func_type(cw, n):
    if n != 1: plural = "s"
    else: plural = ""
    cw.write("""
/// <summary>
/// Encapsulates a method that has %(alpha)s parameter%(plural)s and returns a value of the type specified by the TResult parameter.
/// </summary>""", alpha = numbers[n][0], plural = plural)

    for i in range(1, n + 1):
        cw.write('/// <typeparam name="T%(number)d">The type of the %(alphath)s parameter of the method that this delegate encapsulates.</typeparam>', number = i, alphath = numbers[i][1])
    cw.write('/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>')
    for i in range(1, n + 1):
        cw.write('/// <param name="arg%(number)d">The %(alphath)s parameter of the method that this delegate encapsulates.</param>', number = i, alphath = numbers[i][1])
    cw.write("""/// <returns>The return value of the method that this delegate encapsulates.</returns>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]""")
    cw.write('public delegate TResult Func<%(gsig)s>(%(gparms)s);', gsig = gsig_1_result(n), gparms = gparams_1(n))

def gen_func_types(cw):
    for i in range(2, 17):
        generate_one_func_type(cw, i)

def gen_action_types(cw):
    for i in range(2, 17):
        generate_one_action_type(cw, i)

#
# Pregenerated UpdateAndExecute methods for Func, Action delegate types
#
# Sample argument values:
#
# * methodDeclaration:
#       "TRet UpdateAndExecute1<T0, T1, TRet>(CallSite site, T0 arg0, T1 arg1)"
#       "void UpdateAndExecuteVoid1<T0, T1>(CallSite site, T0 arg0, T1 arg1)"
#
# * matchMakerDeclaration:
#       "TRet Fallback1<T0, T1, TRet>(CallSite site, T0 arg0, T1 arg1)"
#       "void FallbackVoid1<T0, T1>(CallSite site, T0 arg0, T1 arg1)"
#
# * funcType:
#       "Func<CallSite, T0, T1, TRet>"
#       "Action<CallSite, T0, T1>"
#
# * setResult
#       "result =" or ""
#
# * returnResult
#       "return result" or "return"
#
# * returnDefault
#       "return default(TRet)" or "return"
#
# * declareResult
#       Either "TRet result;\n" or ""
#
# * matchmakerArgs
#       "mm_arg0, mm_arg1"
#
# * args
#       "arg0, arg1"
#
# * argsTypes
#       "GetTypeForBinding(arg0), GetTypeForBinding(arg1)"
#
# * typeArgs
#       T0, T1, TRet
#       T0, T1
#
# * fallbackMethod
#       Fallback1
#       FallbackVoid1
#

def gen_update_targets(cw):
    maxArity = 11

    def argList(size):
        return ", ".join(["arg%d" % i for i in range(size)])
    
    def mmArgList(size):
        return ", ".join(["mm_arg%d" % i for i in range(size)])
    
    #def argTypeList(size):
    #    return ", ".join(["GetTypeForBinding(arg%d)" % i for i in range(size)])

    replace = {}
    for n in xrange(1, maxArity):
        replace['setResult'] = 'result ='
        replace['returnResult'] = 'return result'
        replace['returnDefault'] = 'return default(TRet)'
        replace['declareResult'] = 'TRet result;\n'
        replace['args'] = argList(n)
        #replace['argTypes'] = argTypeList(n)
        replace['matchmakerArgs'] = mmArgList(n)
        replace['funcType'] = 'Func<CallSite, %s>' % gsig(n)
        replace['methodDeclaration'] = 'TRet UpdateAndExecute%d<%s>(CallSite site%s)' % (n, gsig(n), gparms(n))
        replace['matchMakerDeclaration'] = 'TRet Fallback%d<%s>(CallSite site%s)' % (n, gsig(n), gparms(n))
        replace['fallbackMethod'] = 'Fallback%d' % (n, )
        replace['typeArgs'] = gsig(n)
        cw.write(updateAndExecute, **replace)

    for n in xrange(1, maxArity):
        replace['setResult'] = ''
        replace['returnResult'] = 'return'
        replace['returnDefault'] = 'return'
        replace['declareResult'] = ''
        replace['args'] = argList(n)
        #replace['argTypes'] = argTypeList(n)
        replace['matchmakerArgs'] = mmArgList(n)
        replace['funcType'] = 'Action<CallSite, %s>' % gsig_noret(n)
        replace['methodDeclaration'] = 'void UpdateAndExecuteVoid%d<%s>(CallSite site%s)' % (n, gsig_noret(n), gparms(n))
        replace['matchMakerDeclaration'] = 'void FallbackVoid%d<%s>(CallSite site%s)' % (n, gsig_noret(n), gparms(n))
        replace['fallbackMethod'] = 'FallbackVoid%d' % (n, )
        replace['typeArgs'] = gsig_noret(n)
        cw.write(updateAndExecute, **replace)

#
# WARNING: If you're changing the generated C# code, make sure you update the
# expression tree representation as well, which lives in UpdateDelegate.cs
# The two implementations *must* be kept in sync!
#
updateAndExecute = '''
[Obsolete("pregenerated CallSite<T>.Update delegate", true)]
internal static %(methodDeclaration)s {
    //
    // Declare the locals here upfront. It actually saves JIT stack space.
    //
    var @this = (CallSite<%(funcType)s>)site;
    CallSiteRule<%(funcType)s>[] applicable;
    CallSiteRule<%(funcType)s> rule;
    %(funcType)s ruleTarget, startingTarget = @this.Target;
    %(declareResult)s
    int count, index;
    CallSiteRule<%(funcType)s> originalRule = null;

    // get the matchmaker & its delegate
    Matchmaker mm = Interlocked.Exchange(ref MatchmakerCache<%(funcType)s>.Info, null);
    if (mm == null) {
        mm = new Matchmaker();
        mm.Delegete = ruleTarget = mm.%(fallbackMethod)s<%(typeArgs)s>;
    } else {
        ruleTarget = (%(funcType)s)mm.Delegete;
    }

    try {    
        //
        // Create matchmaker and its site. We'll need them regardless.
        //
        mm.Match = true;
        site = CallSiteOps.CreateMatchmaker(
            @this,
            ruleTarget
        );
    
        //
        // Level 1 cache lookup
        //
        if ((applicable = CallSiteOps.GetRules(@this)) != null) {
            for (index = 0, count = applicable.Length; index < count; index++) {
                rule = applicable[index];
    
                //
                // Execute the rule
                //
                ruleTarget = CallSiteOps.SetTarget(@this, rule);
    
                try {
                    %(setResult)s ruleTarget(site, %(args)s);
                    if (mm.Match) {
                        %(returnResult)s;
                    }
                } finally {
                    if (mm.Match) {
                        //
                        // Match in Level 1 cache. We saw the arguments that match the rule before and now we
                        // see them again. The site is polymorphic. Update the delegate and keep running
                        //
                        CallSiteOps.SetPolymorphicTarget(@this);
                    }
                }
    
                if ((object)startingTarget == (object)ruleTarget) {
                    // our rule was previously monomorphic, if we produce another monomorphic
                    // rule we should try and share code between the two.
                    originalRule = rule;
                }
                
                // Rule didn't match, try the next one
                mm.Match = true;            
            }
        }
    
        //
        // Level 2 cache lookup
        //
        var args = new object[] { %(args)s };
    
        //
        // Any applicable rules in level 2 cache?
        //
        if ((applicable = CallSiteOps.FindApplicableRules(@this, args)) != null) {
            for (index = 0, count = applicable.Length; index < count; index++) {
                rule = applicable[index];
    
                //
                // Execute the rule
                //
                ruleTarget = CallSiteOps.SetTarget(@this, rule);
    
                try {
                    %(setResult)s ruleTarget(site, %(args)s);
                    if (mm.Match) {
                        %(returnResult)s;
                    }
                } finally {
                    if (mm.Match) {
                        //
                        // Rule worked. Add it to level 1 cache
                        //
                        CallSiteOps.AddRule(@this, rule);
                        // and then move it to the front of the L2 cache
                        @this.RuleCache.MoveRule(rule, args);
                    }
                }
    
                if ((object)startingTarget == (object)ruleTarget) {
                    // If we've gone megamorphic we can still template off the L2 cache
                    originalRule = rule;
                }
                
                // Rule didn't match, try the next one
                mm.Match = true;
            }
        }
    
    
        //
        // Miss on Level 0, 1 and 2 caches. Create new rule
        //
    
        rule = null;
        
        for (; ; ) {
            rule = CallSiteOps.CreateNewRule(@this, rule, originalRule, args);
    
            //
            // Execute the rule on the matchmaker site
            //
    
            ruleTarget = CallSiteOps.SetTarget(@this, rule);
    
            try {
                %(setResult)s ruleTarget(site, %(args)s);
                if (mm.Match) {
                    %(returnResult)s;
                }
            } finally {
                if (mm.Match) {
                    //
                    // The rule worked. Add it to level 1 cache.
                    //
                    CallSiteOps.AddRule(@this, rule);
                }
            }
    
            // Rule we got back didn't work, try another one
            mm.Match = true;
        }
    } finally {
        Interlocked.Exchange(ref MatchmakerCache<%(funcType)s>.Info, mm);
    }
}

private partial class Matchmaker {
    internal %(matchMakerDeclaration)s {
        Match = false;
        %(returnDefault)s;
    }    
}
'''

MaxTypes = MaxSiteArity + 2

def gen_delegate_func(cw):
    for i in range(MaxTypes + 1):
        cw.write("case %(length)d: return typeof(Func<%(targs)s>).MakeGenericType(types);", length = i + 1, targs = "," * i)

def gen_delegate_action(cw):
    for i in range(MaxTypes):
        cw.write("case %(length)d: return typeof(Action<%(targs)s>).MakeGenericType(types);", length = i + 1, targs = "," * i)

def gen_max_delegate_arity(cw):
    cw.write('private const int MaximumArity = %d;' % (MaxTypes + 1))

mismatch = """// Mismatch detection - arity %(size)d
internal static TRet Mismatch%(size)d<%(ts)s>(StrongBox<bool> box, %(params)s) {
    box.Value = false;
    return default(TRet);
}
"""

def gen_matchmaker(cw):
    cw.write("//\n// Mismatch routines for dynamic sites\n//\n")
    for n in range(MaxSiteArity + 2):
        cw.write(mismatch, ts = gsig(n), size = n, params = "CallSite site" + gparms(n))

mismatch_void = """// Mismatch detection - arity %(size)d
internal static void MismatchVoid%(size)d<%(ts)s>(StrongBox<bool> box, %(params)s) {
    box.Value = false;
}
"""

def gen_void_matchmaker(cw):
    cw.write("//\n// Mismatch routines for dynamic sites with void return type\n//\n")
    for n in range(1, MaxSiteArity + 2):
        cw.write(mismatch_void, ts = gsig_noret(n), size = n, params = "CallSite site" + gparms(n))

splatcaller = """[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
[Obsolete("used by generated code", true)]
public static object CallHelper%(size)d(CallSite<Func<CallSite, object%(ts)s>> site, object[] args) {
    return site.Target(site%(args)s);
}
"""

def gen_splatsite(cw):
    cw.write("//\n// Splatting targets for dynamic sites\n//\n")
    for n in range(1, MaxSiteArity + 2):
        cw.write(splatcaller, size = n, ts = ", object" * n, args = gargs_index(n))

update_target="""/// <summary>
/// Site update code - arity %(arity)d
/// </summary>
internal static TRet Update%(arity)d<T, %(ts)s>(CallSite site%(tparams)s) where T : class {
    return (TRet)((CallSite<T>)site).UpdateAndExecute(new object[] { %(targs)s });
}
"""

matchcaller_target="""/// Matchcaller - arity %(arity)d
internal static object Call%(arity)d<%(ts)s>(Func<CallSite, %(ts)s> target, CallSite site, object[] args) {
    return (object)target(site%(targs)s);
}
"""

def gen_matchcaller_targets(cw):
    for n in range(1, MaxSiteArity):
        cw.write(matchcaller_target, ts = gsig(n), targs = gargs_indexwithcast(n), arity = n)

matchcaller_target_void = """// Matchcaller - arity %(arity)d
internal static object CallVoid%(arity)d<%(ts)s>(Action<CallSite, %(ts)s> target, CallSite site, object[] args) {
    target(site%(targs)s);
    return null;
}
"""

def gen_void_matchcaller_targets(cw):
    for n in range(1, MaxSiteArity):
        cw.write(matchcaller_target_void, ts = gsig_noret(n), targs = gargs_indexwithcast(n), arity = n)

def main():
    return generate(
        ("SplatCallSite call helpers", gen_splatsite),
        ("Func Types", gen_func_types),
        ("Action Types", gen_action_types),
        ("UpdateAndExecute Methods", gen_update_targets),
        ("Delegate Action Types", gen_delegate_action),
        ("Delegate Func Types", gen_delegate_func),
        ("Maximum Delegate Arity", gen_max_delegate_arity),
# outer ring generators
        ("Delegate Microsoft Scripting Action Types", gen_delegate_action),
        ("Delegate Microsoft Scripting Scripting Func Types", gen_delegate_func),
        
    )

if __name__ == "__main__":
    main()
