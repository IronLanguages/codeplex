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

import clr
import System

clr.AddReference("Microsoft.Scripting")
from Microsoft.Scripting import CallTarget0;
from Microsoft.Scripting.Generation import CompilerHelpers

paramTypes = System.Array[System.Type]([])
cg = CompilerHelpers.CreateDynamicMethod("test", System.Object, paramTypes)
cg.Emit(System.Reflection.Emit.OpCodes.Ret)
cg.CreateDelegate[CallTarget0](clr.Reference[System.Reflection.MethodInfo]())
