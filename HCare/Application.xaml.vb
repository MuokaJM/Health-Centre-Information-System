
Imports System.Data.OleDb
Imports Microsoft.Win32
Imports Microsoft.SqlServer
Imports ADODB
Imports System.IO
Imports System.Linq
Imports MahApps.Metro
Imports MahApps.Metro.Controls
Imports System.Text
Imports System.Media
Imports System.Windows.Threading ' DispatcherUnhandledExceptionEventArgs

Class Application

    Public Const m_sRegKeyST As String = "Software\Alpha Solutions\HCIS\Settings"
    Public intTheme As Integer = 0
    Public strTheme As String = ""
    Public strThemeColor As String = ""
    'get the requested registry value
    Private Reg As RegistryKey



    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.
    Private Sub App_DispatcherUnhandledException(ByVal sender As Object, ByVal e As DispatcherUnhandledExceptionEventArgs)
        ' Process unhandled exception

        MsgBox("Error has occured " & Err.Description)

        ' Prevent default unhandled exception processing
        e.Handled = True
    End Sub

    Public Sub getTheme()
        Try
            Reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
            strTheme = Reg.GetValue("Theme")
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            strThemeColor = Reg.GetValue("Color")
            Reg.Close()
        Catch ex As Exception
            ' MsgBox("An error has occured!, theme cannot be set now!")
        End Try
    End Sub


End Class
