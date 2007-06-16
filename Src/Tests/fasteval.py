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

#
# Test what works with -X:FastEval
#
# This is not a direct test; it is a helper module invoked by test_ipye.py.

def do_fasteval_test():
    '''Test the sorts of statements that work under FastEval mode'''
    l = []
    for i in range(10):
        l.append(i)
    # we can't use AreEqual &co. until more stuff works.
    assert l == range(10), 'for loops/function calls'
    x = 0
    while x < 10:
        x = x + 1
    assert x == 10, 'while loops/basic arithmetic'

    def two_plus_two():
        return 2+2
    assert two_plus_two() == 4, 'function definition/execution'
    
if __name__=='__main__':
    do_fasteval_test()
