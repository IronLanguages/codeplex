﻿'------------------------------------------------------------------------------
'  <auto-generated>
'      This code was generated by coded UI test builder.
'      Version: 10.0.0.0
'
'      Changes to this file may cause incorrect behavior and will be lost if
'      the code is regenerated.
'  </auto-generated>
' ------------------------------------------------------------------------------

Imports System
Imports System.CodeDom.Compiler
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Windows.Input
Imports Microsoft.VisualStudio.TestTools.UITest.Extension
Imports Microsoft.VisualStudio.TestTools.UITesting
Imports Microsoft.VisualStudio.TestTools.UITesting.WinControls
Imports Microsoft.VisualStudio.TestTools.UITesting.WpfControls
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard
Imports Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse
Imports MouseButtons = System.Windows.Forms.MouseButtons

Namespace DiskUse
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Partial Public Class UIMap
        
        '''<summary>
        '''OpenPowerShell - Use 'OpenPowerShellParams' to pass parameters into this method.
        '''</summary>
        Public Sub OpenPowerShell()
            Dim uIWindowsPowerShellTreeItem As WinTreeItem = Me.UIBrowseForFolderWindow.UITreeViewTree.UIDesktopTreeItem.UIComputerTreeItem.UIWindowsCTreeItem.UIWindowsTreeItem.UISystem32TreeItem.UIWindowsPowerShellTreeItem
            Dim uIOKButton As WinButton = Me.UIBrowseForFolderWindow.UIOKWindow.UIOKButton

            'Expand 'Desktop' -> 'Computer' -> 'windows (C:)' -> 'Windows' -> 'System32' -> 'WindowsPowerShell' tree item
            uIWindowsPowerShellTreeItem.Expanded = Me.OpenPowerShellParams.UIWindowsPowerShellTreeItemExpanded

            'Click 'OK' button
            Mouse.Click(uIOKButton, New Point(47, 8))
        End Sub
        
        '''<summary>
        '''AssertSanity - Use 'AssertSanityExpectedValues' to pass parameters into this method.
        '''</summary>
        Public Sub AssertSanity()
            Dim uIIrondiskusageTitleBar As WpfTitleBar = Me.UIIrondiskusageWindow.UIIrondiskusageTitleBar

            'Verify that 'iron disk usage' title bar's property 'DisplayText' equals 'iron disk usage'
            Assert.AreEqual(Me.AssertSanityExpectedValues.UIIrondiskusageTitleBarDisplayText, uIIrondiskusageTitleBar.DisplayText)
        End Sub
        
        '''<summary>
        '''CloseDiskUse
        '''</summary>
        Public Sub CloseDiskUse()
            Dim uIIrondiskusageTitleBar As WpfTitleBar = Me.UIIrondiskusageWindow.UIIrondiskusageTitleBar
            Dim uICloseButton As WpfButton = Me.UIIrondiskusageWindow.UIIrondiskusageTitleBar.UICloseButton

            'Click 'iron disk usage' title bar
            Mouse.Click(uIIrondiskusageTitleBar, New Point(298, 11))

            'Click 'Close' button
            Mouse.Click(uICloseButton, New Point(7, 8))
        End Sub
        
        #Region "Properties"
        Public Overridable ReadOnly Property OpenPowerShellParams() As OpenPowerShellParams
            Get
                If (Me.mOpenPowerShellParams Is Nothing) Then
                    Me.mOpenPowerShellParams = New OpenPowerShellParams()
                End If
                Return Me.mOpenPowerShellParams
            End Get
        End Property
        
        Public Overridable ReadOnly Property AssertSanityExpectedValues() As AssertSanityExpectedValues
            Get
                If (Me.mAssertSanityExpectedValues Is Nothing) Then
                    Me.mAssertSanityExpectedValues = New AssertSanityExpectedValues()
                End If
                Return Me.mAssertSanityExpectedValues
            End Get
        End Property
        
        Public ReadOnly Property UIBrowseForFolderWindow() As UIBrowseForFolderWindow
            Get
                If (Me.mUIBrowseForFolderWindow Is Nothing) Then
                    Me.mUIBrowseForFolderWindow = New UIBrowseForFolderWindow()
                End If
                Return Me.mUIBrowseForFolderWindow
            End Get
        End Property
        
        Public ReadOnly Property UIIrondiskusageWindow() As UIIrondiskusageWindow
            Get
                If (Me.mUIIrondiskusageWindow Is Nothing) Then
                    Me.mUIIrondiskusageWindow = New UIIrondiskusageWindow()
                End If
                Return Me.mUIIrondiskusageWindow
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mOpenPowerShellParams As OpenPowerShellParams
        
        Private mAssertSanityExpectedValues As AssertSanityExpectedValues
        
        Private mUIBrowseForFolderWindow As UIBrowseForFolderWindow
        
        Private mUIIrondiskusageWindow As UIIrondiskusageWindow
        #End Region
    End Class
    
    '''<summary>
    '''Parameters to be passed into 'OpenPowerShell'
    '''</summary>
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class OpenPowerShellParams
        
        #Region "Fields"
        '''<summary>
        '''Expand 'Desktop' -> 'Computer' -> 'windows (C:)' -> 'Windows' -> 'System32' -> 'WindowsPowerShell' tree item
        '''</summary>
        Public UIWindowsPowerShellTreeItemExpanded As Boolean = true
        #End Region
    End Class
    
    '''<summary>
    '''Parameters to be passed into 'AssertSanity'
    '''</summary>
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class AssertSanityExpectedValues
        
        #Region "Fields"
        '''<summary>
        '''Verify that 'iron disk usage' title bar's property 'DisplayText' equals 'iron disk usage'
        '''</summary>
        Public UIIrondiskusageTitleBarDisplayText As String = "iron disk usage"
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIBrowseForFolderWindow
        Inherits WinWindow
        
        Public Sub New()
            MyBase.New
            Me.SearchProperties(WinWindow.PropertyNames.Name) = "Browse For Folder"
            Me.SearchProperties(WinWindow.PropertyNames.ClassName) = "#32770"
            Me.WindowTitles.Add("Browse For Folder")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UITreeViewTree() As UITreeViewTree
            Get
                If (Me.mUITreeViewTree Is Nothing) Then
                    Me.mUITreeViewTree = New UITreeViewTree(Me)
                End If
                Return Me.mUITreeViewTree
            End Get
        End Property
        
        Public ReadOnly Property UIOKWindow() As UIOKWindow
            Get
                If (Me.mUIOKWindow Is Nothing) Then
                    Me.mUIOKWindow = New UIOKWindow(Me)
                End If
                Return Me.mUIOKWindow
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUITreeViewTree As UITreeViewTree
        
        Private mUIOKWindow As UIOKWindow
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UITreeViewTree
        Inherits WinTree
        
        Public Sub New(ByVal searchLimitContainer As UITestControl)
            MyBase.New(searchLimitContainer)
            Me.SearchProperties(WinTree.PropertyNames.Name) = "Please select the folder to analyse"
            Me.WindowTitles.Add("Browse For Folder")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UIDesktopTreeItem() As UIDesktopTreeItem
            Get
                If (Me.mUIDesktopTreeItem Is Nothing) Then
                    Me.mUIDesktopTreeItem = New UIDesktopTreeItem(Me)
                End If
                Return Me.mUIDesktopTreeItem
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUIDesktopTreeItem As UIDesktopTreeItem
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIDesktopTreeItem
        Inherits WinTreeItem
        
        Public Sub New(ByVal searchLimitContainer As UITestControl)
            MyBase.New(searchLimitContainer)
            Me.SearchProperties(WinTreeItem.PropertyNames.Name) = "Desktop"
            Me.SearchProperties("Value") = "0"
            Me.WindowTitles.Add("Browse For Folder")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UIComputerTreeItem() As UIComputerTreeItem
            Get
                If (Me.mUIComputerTreeItem Is Nothing) Then
                    Me.mUIComputerTreeItem = New UIComputerTreeItem(Me)
                End If
                Return Me.mUIComputerTreeItem
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUIComputerTreeItem As UIComputerTreeItem
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIComputerTreeItem
        Inherits WinTreeItem
        
        Public Sub New(ByVal searchLimitContainer As UITestControl)
            MyBase.New(searchLimitContainer)
            Me.SearchProperties(WinTreeItem.PropertyNames.Name) = "Computer"
            Me.SearchProperties("Value") = "1"
            Me.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching)
            Me.SearchConfigurations.Add(SearchConfiguration.NextSibling)
            Me.WindowTitles.Add("Browse For Folder")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UIWindowsCTreeItem() As UIWindowsCTreeItem
            Get
                If (Me.mUIWindowsCTreeItem Is Nothing) Then
                    Me.mUIWindowsCTreeItem = New UIWindowsCTreeItem(Me)
                End If
                Return Me.mUIWindowsCTreeItem
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUIWindowsCTreeItem As UIWindowsCTreeItem
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIWindowsCTreeItem
        Inherits WinTreeItem
        
        Public Sub New(ByVal searchLimitContainer As UITestControl)
            MyBase.New(searchLimitContainer)
            Me.SearchProperties(WinTreeItem.PropertyNames.Name) = "windows (C:)"
            Me.SearchProperties("Value") = "2"
            Me.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching)
            Me.SearchConfigurations.Add(SearchConfiguration.NextSibling)
            Me.WindowTitles.Add("Browse For Folder")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UIWindowsTreeItem() As UIWindowsTreeItem
            Get
                If (Me.mUIWindowsTreeItem Is Nothing) Then
                    Me.mUIWindowsTreeItem = New UIWindowsTreeItem(Me)
                End If
                Return Me.mUIWindowsTreeItem
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUIWindowsTreeItem As UIWindowsTreeItem
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIWindowsTreeItem
        Inherits WinTreeItem
        
        Public Sub New(ByVal searchLimitContainer As UITestControl)
            MyBase.New(searchLimitContainer)
            Me.SearchProperties(WinTreeItem.PropertyNames.Name) = "Windows"
            Me.SearchProperties("Value") = "3"
            Me.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching)
            Me.SearchConfigurations.Add(SearchConfiguration.NextSibling)
            Me.WindowTitles.Add("Browse For Folder")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UISystem32TreeItem() As UISystem32TreeItem
            Get
                If (Me.mUISystem32TreeItem Is Nothing) Then
                    Me.mUISystem32TreeItem = New UISystem32TreeItem(Me)
                End If
                Return Me.mUISystem32TreeItem
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUISystem32TreeItem As UISystem32TreeItem
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UISystem32TreeItem
        Inherits WinTreeItem
        
        Public Sub New(ByVal searchLimitContainer As UITestControl)
            MyBase.New(searchLimitContainer)
            Me.SearchProperties(WinTreeItem.PropertyNames.Name) = "System32"
            Me.SearchProperties("Value") = "4"
            Me.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching)
            Me.SearchConfigurations.Add(SearchConfiguration.NextSibling)
            Me.WindowTitles.Add("Browse For Folder")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UIWindowsPowerShellTreeItem() As WinTreeItem
            Get
                If (Me.mUIWindowsPowerShellTreeItem Is Nothing) Then
                    Me.mUIWindowsPowerShellTreeItem = New WinTreeItem(Me)
                    Me.mUIWindowsPowerShellTreeItem.SearchProperties(WinTreeItem.PropertyNames.Name) = "WindowsPowerShell"
                    Me.mUIWindowsPowerShellTreeItem.SearchProperties("Value") = "5"
                    Me.mUIWindowsPowerShellTreeItem.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching)
                    Me.mUIWindowsPowerShellTreeItem.SearchConfigurations.Add(SearchConfiguration.NextSibling)
                    Me.mUIWindowsPowerShellTreeItem.WindowTitles.Add("Browse For Folder")
                End If
                Return Me.mUIWindowsPowerShellTreeItem
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUIWindowsPowerShellTreeItem As WinTreeItem
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIOKWindow
        Inherits WinWindow
        
        Public Sub New(ByVal searchLimitContainer As UITestControl)
            MyBase.New(searchLimitContainer)
            Me.SearchProperties(WinWindow.PropertyNames.ControlId) = "1"
            Me.WindowTitles.Add("Browse For Folder")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UIOKButton() As WinButton
            Get
                If (Me.mUIOKButton Is Nothing) Then
                    Me.mUIOKButton = New WinButton(Me)
                    Me.mUIOKButton.SearchProperties(WinButton.PropertyNames.Name) = "OK"
                    Me.mUIOKButton.WindowTitles.Add("Browse For Folder")
                End If
                Return Me.mUIOKButton
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUIOKButton As WinButton
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIIrondiskusageWindow
        Inherits WpfWindow
        
        Public Sub New()
            MyBase.New
            Me.SearchProperties(WpfWindow.PropertyNames.Name) = "iron disk usage"
            Me.SearchProperties.Add(New PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains))
            Me.WindowTitles.Add("iron disk usage")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UIIrondiskusageTitleBar() As UIIrondiskusageTitleBar
            Get
                If (Me.mUIIrondiskusageTitleBar Is Nothing) Then
                    Me.mUIIrondiskusageTitleBar = New UIIrondiskusageTitleBar(Me)
                End If
                Return Me.mUIIrondiskusageTitleBar
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUIIrondiskusageTitleBar As UIIrondiskusageTitleBar
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIIrondiskusageTitleBar
        Inherits WpfTitleBar
        
        Public Sub New(ByVal searchLimitContainer As UITestControl)
            MyBase.New(searchLimitContainer)
            Me.SearchProperties(WpfTitleBar.PropertyNames.AutomationId) = "TitleBar"
            Me.WindowTitles.Add("iron disk usage")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UICloseButton() As WpfButton
            Get
                If (Me.mUICloseButton Is Nothing) Then
                    Me.mUICloseButton = New WpfButton(Me)
                    Me.mUICloseButton.SearchProperties(WpfButton.PropertyNames.AutomationId) = "Close"
                    Me.mUICloseButton.WindowTitles.Add("iron disk usage")
                End If
                Return Me.mUICloseButton
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUICloseButton As WpfButton
        #End Region
    End Class
End Namespace
