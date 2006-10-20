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

# Some utilities

def get_true():
    return True

def get_false():
    return False
    
def get_n(n):
    return n

def test_undefined(function, *args):
    try:
        function(*args)
    except NameError, n:
        Assert("'undefined'" in str(n))
    else:
        Fail("undefined not caught")

# straightforward undefined local
def test_simple_1():
    x = undefined
    undefined = 1           # create local binding

test_undefined(test_simple_1)

# longer assignment statement
def test_simple_2():
    x = y = z = undefined
    undefined = 1           # create local binding

test_undefined(test_simple_1)

# aug assignment
def test_simple_3():
    undefined += 1          # binds already

test_undefined(test_simple_3)

# assigned to self
def test_simple_4():
    undefined = undefined

test_undefined(test_simple_4)

# explicit deletion
def test_simple_5():
    del undefined

test_undefined(test_simple_5)

# if statement
def test_if_1():
    if get_false():
        undefined = 1       # unreachable
    x = undefined
    
test_undefined(test_if_1)

# if statement
def test_if_2():
    if get_false():
        undefined = 1
    else:
        x = 1
    x = undefined
    
test_undefined(test_if_2)

# if statement
def test_if_3():
    if get_true():
        pass
    else:
        undefined = 1
    x = undefined

test_undefined(test_if_3)

# nested if statements
def test_if_4():
    if get_false():
        if get_true():
            undefined = 1
        else:
            undefined = 1
    x = undefined

test_undefined(test_if_4)

# if elif elif elif
def test_if_5():
    n = get_n(10)
    if n == 1:
        undefined = n
    elif n == 2:
        undefined = n
    elif n == 3:
        undefined = n
    elif n == 4:
        undefined = n
    elif n == 5:
        undefined = n
    else:
        pass
    n = undefined

test_undefined(test_if_5)

# for
def test_for_1():
    for i in range(get_n(0)):
        undefined = i
    x = undefined

test_undefined(test_for_1)

# more for with else that doesn't always bind
def test_for_2():
    for i in range(get_n(0)):
        undefined = i
    else:
        if get_false():
            undefined = 1
        elif get_false():
            undefined = 1
        else:
            pass
    x = undefined

test_undefined(test_for_2)

# for with break
def test_for_3():
    for i in range(get_n(10)):
        break
        undefined = 10
    x = undefined

test_undefined(test_for_3)

# for with break and else
def test_for_4():
    for i in range(get_n(10)):
        break
        undefined = 10
    else:
        undefined = 20
    x = undefined

test_undefined(test_for_4)

# for with break and else
def test_for_5():
    for i in range(get_n(10)):
        if get_true():
            break
        undefined = 10
    else:
        undefined = 20
    x = undefined

test_undefined(test_for_5)

# for with break and else and conditional initialization
def test_for_6():
    for i in range(get_n(10)):
        if get_false():
            undefined = 10
        if get_true():
            break
        undefined = 10
    else:
        undefined = 20
    x = undefined

test_undefined(test_for_6)

# delete somewhere deep
def test_for_7():
    for i in range(get_n(10)):
        undefined = 10
        if get_false():
            del undefined
        if get_true():
            del undefined;
        if get_true():
            break
        undefined = 10
    else:
        undefined = 20
    x = undefined

test_undefined(test_for_7)


# bound by for
def test_for_7():
    for undefined in []:
        pass
    print undefined

test_undefined(test_for_7)

# more binding constructs
def test_try_1():
    try:
        1/0
        undefined = 1
    except:
        pass
    x = undefined

test_undefined(test_try_1)

def test_try_2():
    try:
        pass
    except Error, undefined:
        pass
    x = undefined
    
test_undefined(test_try_2)

# bound by import statement
def test_import_1():
    x = undefined
    import undefined

test_undefined(test_import_1)

# same here
def test_import_2():
    x = undefined
    import defined as undefined

test_undefined(test_import_2)

# del
def test_del_1():
    undefined = 1
    del undefined
    x = undefined

test_undefined(test_del_1)

# conditional del
def test_del_2():
    undefined = 10
    if get_true():
        del undefined
    else:
        undefined = 1
    x = undefined

test_undefined(test_del_2)

# del in the loop

def test_del_3():
    undefined = 10
    for i in [1]:
        if i == get_n(1):
            del undefined
    x = undefined
    
test_undefined(test_del_3)

# del in the loop in condition

def test_del_4():
    undefined = 10
    for i in [1,2,3]:
        if i == get_n(1):
            continue
        elif i == get_n(2):
            del undefined
        else:
            break
    x = undefined
    
test_undefined(test_del_4)

def test_params_1(undefined):
    AreEqual(undefined, 1)
    del undefined
    x = undefined

test_undefined(test_params_1, 1)

def test_params_2(undefined):
    AreEqual(undefined, 1)
    x = undefined
    AreEqual(x, 1)
    del undefined
    y = x
    AreEqual(y, 1)    
    x = undefined

test_undefined(test_params_2, 1)

def test_params_3(a):
    if get_false(): return
    x = a
    undefined = a
    del undefined
    x = undefined

test_undefined(test_params_3, 1)
