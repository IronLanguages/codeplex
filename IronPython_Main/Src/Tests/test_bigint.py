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

from iptest.assert_util import *

def test_axiom(a,b):
    Assert((a / b) * b + (a % b) == a, "(" + str(a) + " / " + str(b) + ") * " + str(b) + " + ( " + str(a) + " % " + str(b) + ") != " + str(a))


a = -209681412991024529003047811046079621104607962110459585190118809030105845255159325119855216402270708
b = 37128952704582304957243524

test_axiom(a,b)

a = 209681412991024529003047811046079621104607962110459585190118809030105845255159325119855216402270708
b = 37128952704582304957243524

test_axiom(a,b)

def test(i, j, k):
    u = i * j + k
    test_axiom(u, j)

i = -5647382910564738291056473829105647382910564738291023857209485209457092435
j = 37128952704582304957243524
k = 37128952704582304957243524
k = k - j

while j > k:
   test(i, j, k)
   k = k * 2 + 312870870232

i = 5647382910564738291056473829105647382910564738291023857209485209457092435

while j > k:
   test(i, j, k)
   k = k * 2 + 312870870232

Assert(12297829382473034410)

# Test hex conversions. CPython 2.5 uses capital L, lowercase letters a...f)
s = hex(27L)  # 0x1b
Assert(s == "0x1bL", "27L: Expect lowercase digits. Received: %s." % (s));

s = hex(-27L)
Assert(s == "-0x1bL", "-27L: Expect lowercase digits. Received: %s." % (s));
