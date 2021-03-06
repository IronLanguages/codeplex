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

from lib.assert_util import *
from time import clock

# GetTotalMemory() actually pulls in System
if is_cli:
    def evalLoop(N):
        for i in range(N):
            func = compile(code, '<>', 'exec')
            eval(func)
    
    def evalTest(N):
        startMem = GetTotalMemory()
        startTime = clock()
        evalLoop(N)
        endTime = clock()
        endMem = GetTotalMemory()
        return endMem-startMem
    
    list = [
        "if not 1 + 2 == 3: raise AssertionError('Assertion Failed')",
        "(a,b) = (0, 1)",
        "2+"*10 + "2",
    
        "import sys",
        "from time import clock",
    
        "eval('2+2')",
        "globals(), locals()",
    
        "try:\n    x = 10\nexcept:\n    pass",
    
        "def f(): pass",
        "def f(a): pass",
        "def f(a, b, c, d, e, f, g, h, i, j): pass",
    
        "def f(*args): pass",
        "def f(a, *args): pass",
        "def f(func, *args): func(args)",
        "def f(**args): pass",
    
        "def f(a, b=2, c=[2,3]): pass",
    
        "def f(x):\n    for i in range(x):\n        yield i",
        "def f(x):\n    print locals()",
        "def f(x):\n    print globals()",
    
        "lambda x: x + 2",
    
        "(lambda x: x + 2)(0)",
        "(lambda x, y, z, u, v, w: x + 2)(0, 0, 0, 0, 0, 0)",
    
        "class C:\n    pass",
    
        "class C:\n    class D:pass\n    pass",
        "def f(x):\n    def g(y):pass\n    pass",
        "def f(x):\n    def g(*y):pass\n    pass",
    
        "class C:\n    def f(self):\n        pass",
        "def f():\n    class C: pass\n    pass",
        "def f():pass\nclass C:pass\nf()",
    ]
    
    for code in list:
        baseMem = evalTest(10)
        usedMax = max(10000, 4*baseMem)
        for repetitions in [100, 500]:
            usedMem = evalTest(repetitions)
            Assert(usedMem < usedMax, "Allocated %i (max %i, base %i) running %s %d times" % (usedMem, usedMax, baseMem, code, repetitions))
    
    e = compile("def f(): return 42", "", "single")
    names = {}
    eval(e, names)
    AreEqual(names['f'](), 42)
    
    code = """
x=2
def f(y):
    return x+y
z = f(3)
    """
    e = compile(code, "", "exec")
    names = {}
    eval(e, names)
    AreEqual(names['z'], 5)

