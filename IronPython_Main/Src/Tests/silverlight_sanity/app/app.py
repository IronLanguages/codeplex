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

"""
Bare-bones application used to ensure the 'Silverlight Release' build configuration of
IronPython is OK.
"""

from System.Windows import Application
from System.Windows.Controls import TextBlock

tb = TextBlock()
tb.Text = "Hello World"

Application.Current.RootVisual = tb

