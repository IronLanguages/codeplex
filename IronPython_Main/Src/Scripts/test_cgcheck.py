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


def test_main(level='full'):
    import sys
    old_args = sys.argv
    sys.argv = ['checkonly']

    # !!! Instead of a whitelist, we should have a blacklist so that any newly added
    # generators automatically get included in this tests
    generators = [
        'generate_AssemblyTypeNames',
        'generate_alltypes',
        'generate_calls', 
        'generate_casts',
        'generate_dynsites',
        'generate_exceptions', 
        'generate_math', 
        'generate_ops',
        'generate_reflected_calls',
        'generate_walker',
        'generate_typecache',
        ]

    for gen in generators:
            print "Running", gen
            __import__(gen)
            
    sys.argv = old_args
    
if __name__=="__main__":
    test_main()    
