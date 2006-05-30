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


# types derived from built-in types

class myint(int): pass
class mylong(long): pass
class myfloat(float): pass
class mycomplex(complex): pass

class mystr(str): pass

class mytuple(tuple): pass
class mylist(list): pass
class mydict(dict): pass

class myset(set): pass
class myfrozenset(frozenset): pass

class myfile(file): pass


# to define type constant

def _func(): pass
class _class:
    def method(self): pass
    
class types:
    functionType        = type(_func)
    instancemethodType  = type(_class().method)
    classType           = type(_class)

    
if __name__ == '__main__':
    # for eye check
    for x in dir(types):
        print "%-25s : %r" % (x, getattr(types, x))