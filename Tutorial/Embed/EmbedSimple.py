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

import clr
clr.AddReferenceByPartialName("PresentationCore")
clr.AddReferenceByPartialName("PresentationFramework")
clr.AddReferenceByPartialName("WindowsBase")
clr.AddReferenceByPartialName("IronPython")

from System.Windows.Controls import *
from System.Windows import *

def add_text(window):
    txt = TextBlock();
    txt.FontSize = 50;
    txt.Text = "IronPython!";
    Canvas.SetTop(txt, 200);
    Canvas.SetLeft(txt, 100);
    window.Content.Children.Add(txt);

if globals().has_key('window'):
    add_text(window)
else:
    MessageBox.Show("Please set 'window' variable'")
