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
import sys
from System import Environment

enablePython25 = "-X:Python25" in Environment.GetCommandLineArgs()

try:
	c = compile("""

# Simple conditional true case
Assert(100 if 1 else 200 == 100)

# Simple conditional false case
Assert(100 if 0 else 200 == 200)

# Conditional multiple assignment 
x, y,z, w , u  = 1 if 0 else 2, 2 if 1 else 3, 3 if 10 else 4, 1 & 0 if 0 and 3 or 4 else 100, 1 and 0 if 0 and 3 or 4 & 0 else 100
assert(x,y,z,w,u == 2,2,3,0,100)

# combination of operators and conditional
Assert(100 + 1 & 3 if 1 else 2 == 1)
Assert(100 + (1 & 3 if 1 else 2) == 101)

# conditional in if-else
x,y,z = 0,1,2
if x if y else z:
	p = 100
else:
	p = 200
Assert(p == 200)

# nested conditionals 
if 0 if (0 if 100 else 1 ) else 10:
	x = 300
else:
	x = 400
Assert(x == 300)

# conditionals with test-list #test1
x,y,z = 1,2,3
if 20 if (x,y,z == 1,2,3 ) else 0 :
	x = 300
else:
	x = 400
Assert(x == 300)

# conditionals with test-list #test2
list = [[1 if 1 else 0,0 if 0 else 2,3],[4,5 if 1 and 1 else 0,8 if 0 and 1 else 6 & 7]]
if 20 if (list == [[1,2,3],[4,5,7]]) else 0 if 1 else 200 :
	x = 300
else:
	x = 400
Assert(x == 400)

#test for gen_for
Assert(sum(x*x for x in range(10) if not x%2 if not x%3) == sum([x*x for x in range(10) if not x%6]))

#test for gen_for gen_if combined
Assert(sum(x*x for x in range(10) for x in range(5) if not x %2) == 200)

#test for list_for
list = [10,20,30,40,50,60,70,80,90,100,110,120,130]
mysum = 0
for i in (0,1,2,3,4,5,6,7,8,9,10,11,12):
	mysum += list[i]
Assert(mysum == 910)

#test for null list
AssertError(SyntaxError, compile, "mysum = 0;for i in 10:pass", "Error", "exec")

#test for list_for list_if combined
list = [10,20,30,40,50,60,70,80,90,100,110,120,130]
Assert(sum(list[i] if not i%2 else 0 for i in (0,1,2,3,4,5,6,7,8,9,10,11,12) if not i %3 if not 0) == 210)

# test for lambda function
try:
	c = compile("[f for f in 1, lambda x: x if x >= 0 else -1]","","exec")
except SyntaxError,e:
	pass
except e:
	Assert(false,e.msg)

#AssertError(SyntaxError, compile, "[f for f in 1, lambda x: x if x >= 0 else -1]", "Error", "exec")
try:
	list = [f for f in (1, lambda x: x if x >= 0 else -1)]	
	list = [f for f in 1, lambda x: (x if x >= 0 else -1)] 
	list = [f for f in 1, (lambda x: x if x >= 0 else -1)]
except e:
	Assert(False, e.msg)
""","","exec")

	if not enablePython25:
		Assert(False,"Python 2.5 feature is enabled in lower version of Python")

except SyntaxError,e:
	if enablePython25:
		print e.msg
	else:
		pass
except e:
	print e.msg
