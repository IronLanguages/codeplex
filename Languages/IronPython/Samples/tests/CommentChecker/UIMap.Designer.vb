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
Imports Microsoft.VisualStudio.TestTools.UITesting.WpfControls
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard
Imports Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse
Imports MouseButtons = System.Windows.Forms.MouseButtons

Namespace CommentChecker
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Partial Public Class UIMap
        
        '''<summary>
        '''ClickSampleWindow
        '''</summary>
        Public Sub ClickSampleWindow()
            Dim uIIronPythonCommentCheTitleBar As WpfTitleBar = Me.UIIronPythonCommentCheWindow.UIIronPythonCommentCheTitleBar

            'Click 'IronPython Comment Checker' title bar
            Mouse.Click(uIIronPythonCommentCheTitleBar, New Point(586, 5))
        End Sub
        
        '''<summary>
        '''AssertWindowName - Use 'AssertWindowNameExpectedValues' to pass parameters into this method.
        '''</summary>
        Public Sub AssertWindowName()
            Dim uIIronPythonCommentCheTitleBar As WpfTitleBar = Me.UIIronPythonCommentCheWindow.UIIronPythonCommentCheTitleBar

            'Verify that 'IronPython Comment Checker' title bar's property 'Name' equals 'IronPython Comment Checker'
            Assert.AreEqual(Me.AssertWindowNameExpectedValues.UIIronPythonCommentCheTitleBarName, uIIronPythonCommentCheTitleBar.Name)
        End Sub
        
        '''<summary>
        '''CloseSample
        '''</summary>
        Public Sub CloseSample()
            Dim uIIronPythonCommentCheTitleBar As WpfTitleBar = Me.UIIronPythonCommentCheWindow.UIIronPythonCommentCheTitleBar
            Dim uICloseButton As WpfButton = Me.UIIronPythonCommentCheWindow.UIIronPythonCommentCheTitleBar.UICloseButton

            'Click 'IronPython Comment Checker' title bar
            Mouse.Click(uIIronPythonCommentCheTitleBar, New Point(713, 9))

            'Click 'Close' button
            Mouse.Click(uICloseButton, New Point(8, 9))
        End Sub
        
        #Region "Properties"
        Public Overridable ReadOnly Property AssertWindowNameExpectedValues() As AssertWindowNameExpectedValues
            Get
                If (Me.mAssertWindowNameExpectedValues Is Nothing) Then
                    Me.mAssertWindowNameExpectedValues = New AssertWindowNameExpectedValues()
                End If
                Return Me.mAssertWindowNameExpectedValues
            End Get
        End Property
        
        Public ReadOnly Property UIIronPythonCommentCheWindow() As UIIronPythonCommentCheWindow
            Get
                If (Me.mUIIronPythonCommentCheWindow Is Nothing) Then
                    Me.mUIIronPythonCommentCheWindow = New UIIronPythonCommentCheWindow()
                End If
                Return Me.mUIIronPythonCommentCheWindow
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mAssertWindowNameExpectedValues As AssertWindowNameExpectedValues
        
        Private mUIIronPythonCommentCheWindow As UIIronPythonCommentCheWindow
        #End Region
    End Class
    
    '''<summary>
    '''Parameters to be passed into 'AssertWindowName'
    '''</summary>
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class AssertWindowNameExpectedValues
        
        #Region "Fields"
        '''<summary>
        '''Verify that 'IronPython Comment Checker' title bar's property 'Name' equals 'IronPython Comment Checker'
        '''</summary>
        Public UIIronPythonCommentCheTitleBarName As String = "IronPython Comment Checker"
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIIronPythonCommentCheWindow
        Inherits WpfWindow
        
        Public Sub New()
            MyBase.New
            Me.SearchProperties(WpfWindow.PropertyNames.Name) = "IronPython Comment Checker"
            Me.SearchProperties.Add(New PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains))
            Me.WindowTitles.Add("IronPython Comment Checker")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UIIronPythonCommentCheTitleBar() As UIIronPythonCommentCheTitleBar
            Get
                If (Me.mUIIronPythonCommentCheTitleBar Is Nothing) Then
                    Me.mUIIronPythonCommentCheTitleBar = New UIIronPythonCommentCheTitleBar(Me)
                End If
                Return Me.mUIIronPythonCommentCheTitleBar
            End Get
        End Property
        #End Region
        
        #Region "Fields"
        Private mUIIronPythonCommentCheTitleBar As UIIronPythonCommentCheTitleBar
        #End Region
    End Class
    
    <GeneratedCode("Coded UITest Builder", "10.0.30319.1")>  _
    Public Class UIIronPythonCommentCheTitleBar
        Inherits WpfTitleBar
        
        Public Sub New(ByVal searchLimitContainer As UITestControl)
            MyBase.New(searchLimitContainer)
            Me.SearchProperties(WpfTitleBar.PropertyNames.AutomationId) = "TitleBar"
            Me.WindowTitles.Add("IronPython Comment Checker")
        End Sub
        
        #Region "Properties"
        Public ReadOnly Property UICloseButton() As WpfButton
            Get
                If (Me.mUICloseButton Is Nothing) Then
                    Me.mUICloseButton = New WpfButton(Me)
                    Me.mUICloseButton.SearchProperties(WpfButton.PropertyNames.AutomationId) = "Close"
                    Me.mUICloseButton.WindowTitles.Add("IronPython Comment Checker")
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
