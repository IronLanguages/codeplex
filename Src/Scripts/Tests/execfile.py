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

def main():
    root = iron_python_root + iron_python_tests

    execfile(root + "/Inc/toexec.py")
    execfile(root + "/Inc/toexec.py")
    execfile(root + "/doc.py")
    execfile(root + "/Inc/toexec.py")

if __name__ == "__main__":
    __name__ = "execfile"
    main()

AssertError(TypeError, execfile, None) # arg must be string
AssertError(TypeError, execfile, [])
AssertError(TypeError, execfile, 1)


##
## to test how exec related to globals/locals
##

def Contains(large, small):
    for (key, value) in small.items():
        Assert(large[key] == value)

def NotContain(dict, *keylist):
    for key in keylist:
        Assert(not dict.has_key(key))

## exec without in something
x = 1
y = "hello"
Contains(globals(), {"x":1, "y":"hello"})
Contains(locals(),  {"x":1, "y":"hello"})

exec "x, y"
Contains(globals(), {"x":1, "y":"hello"})
Contains(locals(),  {"x":1, "y":"hello"})

## exec with custom globals
# -- use global x, y; assign
g1 = {'x':2, 'y':'world'}
exec "global x; x, y; x = 4" in g1
Contains(globals(), {"x":1, "y":"hello"})
Contains(locals(),  {"x":1, "y":"hello"})
Contains(g1, {"x":4, "y":"world"})

exec "global x; x, y; x = x + 4" in g1
Contains(g1, {"x":8})

# -- declare global
exec "global z" in g1
NotContain(globals(), 'z')
NotContain(locals(), 'z')
NotContain(g1, 'z')

# -- new global
exec "global z; z = -1" in g1
NotContain(globals(), 'z')
NotContain(locals(), 'z')
Contains(g1, {'z':-1})

# y is missing in g2
g2 = {'x':3}
try:
    exec "x, y" in g2
    Assert(False, "should throw NameError exception")
except NameError:
    pass

exec "y = 'ironpython'" in g2
Contains(g2, {"x":3, "y":"ironpython"})
Contains(globals(), {"y":"hello"})
Contains(locals(),  {"y":"hello"})

## exec with custom globals, locals
g = {'x': -1, 'y': 'python' }
l = {}

# use global
exec "if x != -1: throw" in g, l
exec "if y != 'python': throw" in g, l
NotContain(l, 'x', 'y')

# new local
exec "x = 20; z = 2" in g, l
Contains(g, {"x":-1, "y":"python"})
Contains(l, {"x":20, "z":2})

# changes
exec "global y; y = y.upper(); z = -2" in g, l
Contains(g, {'x': -1, 'y': 'PYTHON'})
Contains(l, {'x': 20, 'z': -2})

# new global
exec "global w; w = -2" in g, l
Contains(g, {'x': -1, 'y': 'PYTHON', 'w': -2})
Contains(l, {'x': 20, 'z': -2})

# x in both g and l; use it
exec "global x; x = x - 1" in g, l
Contains(g, {'x': -2, 'y': 'PYTHON', 'w': -2})
Contains(l, {'x': 20, 'z': -2})

exec "x = x + 1" in g, l
Contains(g, {'x': -2, 'y': 'PYTHON', 'w': -2})
Contains(l, {'x': 21, 'z': -2})


## Inside Function: same as last part of previous checks
def InsideFunc():
    g = {'x': -1, 'y': 'python' }
    l = {}

    # use global
    exec "if x != -1: throw" in g, l
    exec "if y != 'python': throw" in g, l
    NotContain(l, 'x', 'y')

    # new local
    exec "x = 20; z = 2" in g, l
    Contains(g, {"x":-1, "y":"python"})
    Contains(l, {"x":20, "z":2})

    # changes
    exec "global y; y = y.upper(); z = -2" in g, l
    Contains(g, {'x': -1, 'y': 'PYTHON'})
    Contains(l, {'x': 20, 'z': -2})

    # new global
    exec "global w; w = -2" in g, l
    Contains(g, {'x': -1, 'y': 'PYTHON', 'w': -2})
    Contains(l, {'x': 20, 'z': -2})

    # x in both g and l; use it
    exec "global x; x = x - 1" in g, l
    Contains(g, {'x': -2, 'y': 'PYTHON', 'w': -2})
    Contains(l, {'x': 20, 'z': -2})

    exec "x = x + 1" in g, l
    Contains(g, {'x': -2, 'y': 'PYTHON', 'w': -2})
    Contains(l, {'x': 21, 'z': -2})


InsideFunc()


unique_global_name = 987654321
class C:
    exec 'a = unique_global_name'
    exec "if unique_global_name != 987654321: raise AssertionError('cannott see unique_global_name')"

AreEqual(C.a, 987654321)

def f():
    exec "if unique_global_name != 987654321: raise AssertionError('cannot see unique_global_name')"
    
    def g():
         exec "if unique_global_name != 987654321: raise AssertionError('cannot see unique_global_name')"
    g()

f()
