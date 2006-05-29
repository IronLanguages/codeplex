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

from Util.Debug import *
from collections import *



def nolocals():
    Assert(locals() == {})

def singleLocal():
    a = True
    Assert(locals() == {'a' : True})

def nolocalsWithArg(a):
    Assert(locals() == {'a' : 5})

def singleLocalWithArg(b):
    a = True
    Assert(locals() == {'a' : True, 'b': 5})

def delSimple():
    a = 5
    Assert(locals() == {'a' : 5})
    del(a)
    Assert(locals() == {})

inactive = """
def iteratorFunc():
    for i in range(1):
        Assert(locals() == {'i' : 0})
        yield i

def iteratorFuncLocals():
    a = 3
    for i in range(1):
        Assert(locals() == {'a':3, 'i' : 0})
        yield i

def iteratorFuncWithArg(b):
    for i in range(1):
        Assert(locals() == {'i' : 0, 'b':5})
        yield i

def iteratorFuncLocalsWithArg(b):
    a = 3
    for i in range(1):
        Assert(locals() == {'a':3, 'i' : 0, 'b':5})
        yield i

def delIter():
    a = 5
    yield 2
    Assert(locals() == {'a': 5})
    del(a)
    yield 3
    Assert(locals() == {})
"""

def execAdd():
    exec('a=2')
    Assert(locals() == {'a': 2})

def execAddExisting():
    b = 5
    exec('a=2')
    Assert(locals() == {'a': 2, 'b':5})

def execAddExistingArgs(c):
    b = 5
    exec('a=2')
    Assert(locals() == {'a': 2, 'b': 5, 'c':7})

def execDel():
    a = 5
    exec('del(a)')
    #AreEqual(locals(), {})

def unassigned():
    Assert(locals() == {})
    a = 5
    AreEqual(locals(), {'a': 5})


def reassignLocals():
    locals = 2
    Assert(locals == 2)

inactive = """
def unassignedIter():
    yield 1
    Assert(locals() == {})
    yield 2
    a = 5
    yield 3
    Assert(locals() == {'a': 5})
    yield 4

def reassignLocalsIter():
    yield 1
    locals = 2
    yield 2
    Assert(locals == 2)
 """

# we used to include _ which got defined during codegen.  Make sure
# we don't crash
def localsAfterExpr():
    exec "pass"
    10
    exec "pass"

nolocals()
singleLocal()
#for a in iteratorFunc(): pass
#for a in iteratorFuncLocals(): pass


nolocalsWithArg(5)
singleLocalWithArg(5)

def modifyingLocal(a):
    AreEqual(a, 10)
    a = 8
    AreEqual(a, 8)
    AreEqual(locals(), { 'a' : 8 })

modifyingLocal(10)


#for a in iteratorFuncWithArg(5): pass
#for a in iteratorFuncLocalsWithArg(5): pass

execAdd()
execAddExisting()
execAddExistingArgs(7)
execDel()

delSimple()
#for a in delIter(): pass

unassigned()
#for a in unassignedIter(): pass

reassignLocals()
#for a in reassignLocalsIter(): pass

Assert(locals().has_key('__builtins__'))
a = 5
Assert(locals().has_key('a'))

exec('a = a+1')

Assert(locals()['a'] == 6)

def my_locals():
    Fail("Calling wrong locals")

exec "pass"
locals = my_locals
exec "pass"
import __builtin__
save_locals = __builtin__.locals
try:
    __builtin__.locals = my_locals
    exec "pass"
finally:
    __builtin__.locals = save_locals

localsAfterExpr()