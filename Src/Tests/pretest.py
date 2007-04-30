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

# pretest.py
# ----------
# Passed to SuperConsole script to redirect standard output to file, and populate
# the dictionary for tab-completion tests.

from System.IO import File
File.Delete('ip_session.log')
import sys
sys.stdout = open('ip_session.log', 'w')

# Test Case #1: ensure that an attribute with a prefix unique to the dictionary is properly completed.
######################################################################################################

# Only one attribute has 'z' has a prefix
zoltar = "zoltar"

# Two attributes have 'y' as a prefix, but only one has 'yo'
yorick = "yorick"
yak = "yak"

# Test Case #2: ensure that tabbing on a non-unique prefix cycles through the available options
######################################################################################################

# yorick and yak are used here also

# Test Case #3: ensure that tabbing after 'ident.' cycles through the available options
######################################################################################################

class C: 
    'Cdoc'
    pass

c = C()
