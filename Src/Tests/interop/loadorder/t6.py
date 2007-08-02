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
import clr
clr.AddReferenceToFileAndPath(r"D:\merlin2\Main\Test\ClrAssembly\folder1\loadorder_6.dll") # TODO

import Namespace_ToBeRemoved

for x in clr.References: 
    print x, x.Location

clr.AddReferenceToFileAndPath(r"D:\merlin2\Main\Test\ClrAssembly\folder2\loadorder_6.dll") # TODO

for x in clr.References: 
    print x, x.Location

print Namespace_ToBeRemoved.C.Flag

import Namespace_JustAdded

print Namespace_JustAdded.C.Flag

del Namespace_ToBeRemoved
print Namespace_ToBeRemoved.C.Flag

import Namespace_ToBeRemoved
