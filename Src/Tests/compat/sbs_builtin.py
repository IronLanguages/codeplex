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

from common import *
import testdata

import sys

def complex_case_repr(*args):    
    ret = "complex with " 
    for x in args:
        ret += "'%s (%s)'" % (str(x), type(x))
    return ret

class test_builtin: 
    ''' test built-in type, etc '''
    
    def test_slice(self):
        ''' currently mainly test 
                del list[slice]
        '''
        test_str = testdata.long_string
        str_len  = len(test_str)
        
        choices = ['', 0]
        numbers = [1, 2, 3, str_len/2-1, str_len/2, str_len/2+1, str_len-3, str_len-2, str_len-1, str_len, str_len+1, str_len+2, str_len+3, str_len*2]
        numbers = numbers[::3] # Temporary approach to speed things up...
        choices.extend(numbers)
        choices.extend([-1 * x for x in numbers])
        
        for x in choices:
            for y in choices:
                for z in choices:
                    if z == 0:  continue

                    line = "l = list(test_str); del l[%s:%s:%s]" % (str(x), str(y), str(z))
                    exec line                
                    printwith("case", "del l[%s:%s:%s]" % (str(x), str(y), str(z)))
                    printwith("same", eval("l"), eval("len(l)"))

    def test_xrange(self):
        ''' test xrange with corner cases'''
        import sys
        maxint = sys.maxint
        numbers = [1, 2, maxint/2, maxint-1, maxint, maxint+1, maxint+2]
        choices = [0]
        choices.extend(numbers)
        choices.extend([-1 * x for x in numbers])
        
        for x in choices:
            for y in choices:
                for z in choices:
                    line = "xrange(%s, %s, %s)" % (str(x), str(y), str(z))
                    printwith("case", line)
                    try: 
                        xr = eval(line)
                        xl = len(xr)
                        cnt = 0
                        first = last = first2 = last2 = "n/a"
                        # testing XRangeIterator
                        if xl < 10:
                            for x in xr: 
                                if cnt == 0: first = x
                                if cnt == xl -1 : last = x
                                cnt += 1
                        # testing this[index]
                        if xl == 0: first2 = xr[0]
                        if xl > 1 : first2, last2 = xr[0], xr[xl - 1]
                        
                        printwith("same", xr, xl, first, last, first2, last2)
                    except: 
                        printwith("same", sys.exc_type)

    def test_complex_ctor_str(self):
        l = [ "", " ", "-1", "0", "1", "+1", "+1.1", "-1.01", "-.101", ".234", "-1.3e3", "1.09e-3", "33.2e+10"]
        
        for s in l:
            try: 
                printwith("case", complex_case_repr(s))
                c = complex(s)
                printwithtype(c)
            except: 
                printwith("same", sys.exc_type, sys.exc_value)
            
            s += "j"
            try: 
                printwith("case", complex_case_repr(s))
                c = complex(s)
                printwithtype(c)
            except: 
                printwith("same", sys.exc_type, sys.exc_value)
        
        for s1 in l:
            for s2 in l:
                try:
                    if s2.startswith("+") or s2.startswith("-"):
                        s = "%s%sJ" % (s1, s2)
                    else:
                        s = "%s+%sj" % (s1, s2)
                    
                    printwith("case", complex_case_repr(s))
                    c = complex(s)
                    printwithtype(c)
                except: 
                    printwith("same", sys.exc_type, sys.exc_value)
                    
    def test_complex_ctor(self):
        # None is not included due to defaultvalue issue
        ln = [-1, 1L, 1.5, 1.5e+5, 1+2j, -1-9.3j ]
        ls = ["1", "1L", "-1.5", "1.5e+5", "-34-2j"]
        
        la = []
        la.extend(ln)
        la.extend(ls)
        
        for s in la:
            try:                
                printwith("case", complex_case_repr(s))
                c = complex(s)
                printwithtype(c)
            except:
                printwith("same", sys.exc_type, sys.exc_value)
        
        bug = '''
        for s in la:
            try:                
                printwith("case", "real only", complex_case_repr(s))
                c = complex(real=s)
                printwithtype(c)
            except:
                printwith("same", sys.exc_type, sys.exc_value)

        for s in la:
            try:                
                printwith("case", "imag only", complex_case_repr(s))
                c = complex(imag=s)
                printwithtype(c)
            except:
                printwith("same", sys.exc_type, sys.exc_value)
        '''
                     
        for s1 in la:
            for s2 in ln:
                try:                
                    printwith("case", complex_case_repr(s1, s2))
                    c = complex(s1, s2)
                    printwithtype(c)
                except:
                    printwith("same", sys.exc_type, sys.exc_value)
                    
runtests(test_builtin)