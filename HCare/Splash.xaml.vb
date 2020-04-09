Imports System.Data.Sql
Imports ADODB
Imports System.Windows.Threading
Imports System.Security
Imports Microsoft.Win32
Imports System.Reflection.Assembly
Imports System.Diagnostics.FileVersionInfo
Imports MahApps.Metro
Imports MahApps.Metro.Controls
Imports System.Windows.Media

Imports Microsoft.SqlServer

Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports System.Windows.Forms.Application
Imports Microsoft.VisualBasic
Imports System
Imports System.IO

Public Class Splash
    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
        Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
        Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
        Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}


    Private regOpen As RegistryKey
    Private Const m_sRegKeyST As String = "Software\Alpha Solutions\HCIS\Settings"

    Private intTheme As Integer = 0
    Private strTheme As String = ""
    Private strThemeColor As String = ""
    Private iGColor As Integer

    Private dtLogin As New DispatcherTimer
    Private frmL As New Login
    ' Private User As String = Windows.Forms.SystemInformation.
    Private computer As String = Windows.Forms.SystemInformation.ComputerName

    Public cnHecom As New ADODB.Connection
    Public rsInfo As New ADODB.Recordset
    Public strConn As String
    Private cn As ADODB.Connection
    Private rs As ADODB.Recordset
    Private strSQL As String
    Private FileLength As Integer
    Private Numblocks As Integer
    Private LeftOver As Integer
    Private i As Integer
    Private Const BlockSize As Integer = 100000
    Public PictBmp As String
    Private ByteData As Byte()
    Private SourceFile As Integer
    Private PicFile As String
    Private strPath As String
    Private DiskFile As String



    Private Sub Splash_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        Dim myBrush As New SolidColorBrush

        Try
            regOpen = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
            strTheme = regOpen.GetValue("Theme")
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            strThemeColor = regOpen.GetValue("Color")
            regOpen.Close()

            setTheme(strThemeColor, intTheme)
        Catch ex As Exception

        End Try

        myBrush.Color = myColors(iGColor)

        lblCName.Foreground = myBrush
        lblVersion.Foreground = myBrush
        lblCAdd.Foreground = myBrush
        lblCon.Foreground = myBrush
        lblCTel.Foreground = myBrush
        lblPlatform.Foreground = myBrush
        pRing.Foreground = myBrush
        Me.lblBline.Foreground = myBrush
        Me.lblTitle.Foreground = myBrush

        Try
            lblCName.Content = "Running on: " & computer
            lblVersion.Content = GetVersionInfo(GetExecutingAssembly.Location).ProductVersion
            Me.lblTitle.Content = Interaction.GetSetting("Alpha Solutions\HCIS", "Details", "UB", "")
            Me.lblCAdd.Content = Interaction.GetSetting("Alpha Solutions\HCIS", "Details", "Add", "")
            Me.lblBline.Content = Interaction.GetSetting("Alpha Solutions\HCIS", "Details", "qt", "")
            Me.lblCTel.Content = Interaction.GetSetting("Alpha Solutions\HCIS", "Details", "TM", "")
            Me.lblCon.Content = ""
            Me.lblPlatform.Content = ""
        Catch ex As Exception
            MsgBox("An error has occured during load 3")
        End Try

        Try
            Dim Mchar As String = ""
            strPath = GetExecutingAssembly.Location
            Do While Mchar <> "\"
                Mchar = Mid(strPath, Len(strPath), 1)
                strPath = strPath.Remove(Len(strPath) - 1, 1)
            Loop

        Catch ex As Exception

        End Try

        strConn = "Provider=SQLOLEDB;Data Source=(LocalDB);Initial Catalog=HCISDB;User ID=sa;Password=******"
        ' strConn = "Provider=SQLOLEDB;Data Source=(LocalDB);Initial Catalog=HCISDB;Integrated Security=SSPI;"


        cnHecom.Open(strConn)
        rsInfo.CursorLocation = ADODB.CursorLocationEnum.adUseClient
        rsInfo.Open("SELECT * FROM tblInfo", cnHecom, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)

        GetRegData()

        Try
            dtLogin.Interval = TimeSpan.FromMilliseconds(25)
            AddHandler dtLogin.Tick, AddressOf Login_Tick
        Catch ex As Exception
            MsgBox("An error has occured during load")
        End Try

        Try
            pRing.IsActive = True
            pbLoad.Maximum = 100
            dtLogin.Start()
        Catch ex As Exception
            MsgBox("An error has occured during load ")
        End Try

    End Sub

    Public Sub CheckForExistingInstance()
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then
            MsgBox("Another Instance of this process is already running Multiple Instances Forbidden", MsgBoxStyle.Exclamation)
            End
        End If
    End Sub

    Private Sub Login_Tick()
        pbLoad.Value = pbLoad.Value + 1
    End Sub


    Private Sub pbLoad_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If pbLoad.Value = pbLoad.Maximum Then
            dtLogin.Stop()
            pRing.IsActive = False
            frmL.intGColor = iGColor
            frmL.Show()
            Me.Hide()
            ' Close()
        End If
    End Sub

    Private Sub setTheme(strTheme As String, intTheme As Integer)

        Select Case strTheme
            Case "Lime"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Lime"), ThemeManager.AppThemes(intTheme))
                iGColor = 0
            Case "Green"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Green"), ThemeManager.AppThemes(intTheme))
                iGColor = 1
            Case "Emerald"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Emerald"), ThemeManager.AppThemes(intTheme))
                iGColor = 2
            Case "Teal"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Teal"), ThemeManager.AppThemes(intTheme))
                iGColor = 3
            Case "Cyan"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Cyan"), ThemeManager.AppThemes(intTheme))
                iGColor = 4
            Case "Cobalt"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Cobalt"), ThemeManager.AppThemes(intTheme))
                iGColor = 5
            Case "Indigo"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Indigo"), ThemeManager.AppThemes(intTheme))
                iGColor = 6
            Case "Violet"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Violet"), ThemeManager.AppThemes(intTheme))
                iGColor = 7
            Case "Pink"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Pink"), ThemeManager.AppThemes(intTheme))
                iGColor = 8
            Case "Magenta"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Magenta"), ThemeManager.AppThemes(intTheme))
                iGColor = 9
            Case "Crimson"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Crimson"), ThemeManager.AppThemes(intTheme))
                iGColor = 10
            Case "Red"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Red"), ThemeManager.AppThemes(intTheme))
                iGColor = 11
            Case "Orange"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Orange"), ThemeManager.AppThemes(intTheme))
                iGColor = 12
            Case "Amber"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Amber"), ThemeManager.AppThemes(intTheme))
                iGColor = 13
            Case "Yellow"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Yellow"), ThemeManager.AppThemes(intTheme))
                iGColor = 14
            Case "Brown"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Brown"), ThemeManager.AppThemes(intTheme))
                iGColor = 15
            Case "Olive"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Olive"), ThemeManager.AppThemes(intTheme))
                iGColor = 16
            Case "Steel"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Steel"), ThemeManager.AppThemes(intTheme))
                iGColor = 17
            Case "Mauve"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Mauve"), ThemeManager.AppThemes(intTheme))
                iGColor = 18
            Case "Taupe"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Taupe"), ThemeManager.AppThemes(intTheme))
                iGColor = 19
            Case Else
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Blue"), ThemeManager.AppThemes(0))
                iGColor = 4
        End Select

    End Sub

    Sub retrieveServers()
        Dim instance As SqlDataSourceEnumerator = SqlDataSourceEnumerator.Instance
        Dim table As System.Data.DataTable = instance.GetDataSources()

       




    End Sub

 
    Private Sub GetRegData()
        Try
            Dim rsInfo As New ADODB.Recordset
            With rsInfo
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblInfo", cnHecom, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .EOF = True And .BOF = True Then
                Else
                    If IsDBNull(.Fields("Title").Value) = False Then
                        Me.lblTitle.Content = (.Fields("Title").Value)
                    End If
                    If IsDBNull(.Fields("PAddress").Value) = False Then
                        Me.lblCAdd.Content = (.Fields("PAddress").Value)
                    End If
                    If IsDBNull(.Fields("Telephone").Value) = False Then
                        Me.lblCTel.Content = (.Fields("Telephone").Value)
                    End If
                    If IsDBNull(.Fields("BusinessLine").Value) = False Then
                        Me.lblBline.Content = (.Fields("BusinessLine").Value)
                    End If
                    Me.GetPic()
                End If

            End With


        Catch ex As Exception
            MsgBox("An error has occured")
        End Try
    End Sub


    Public Function GetPic() As Object
        Dim DestFileNum As Short
        Dim i As Integer
        Me.FileLength = 0
       
        Try
            Me.DiskFile = Me.strPath & "\logo.bmp"
            If (Strings.Len(FileSystem.Dir(Me.DiskFile, FileAttribute.Normal)) > 0) Then
                FileSystem.Kill(Me.DiskFile)
            End If

        Catch exception As Exception
            Me.DiskFile = Me.strPath & "\logo.bmp"
            If (Strings.Len(FileSystem.Dir(Me.DiskFile, FileAttribute.Normal)) > 0) Then
                FileSystem.Kill(Me.DiskFile)
            End If
            Throw
            Interaction.MsgBox("File cannot be deleted!", MsgBoxStyle.Exclamation, Nothing)
        End Try

        FileLength = rsInfo.Fields.Item("logo").ActualSize

        If (FileLength = 0) Then
            Return CType(0, Integer)
            Exit Function '
        Else
            DestFileNum = FreeFile()
            FileOpen(DestFileNum, DiskFile, OpenMode.Binary, , OpenShare.Shared)
            Numblocks = FileLength / BlockSize
            LeftOver = FileLength Mod BlockSize
            ByteData = rsInfo.Fields("logo").GetChunk(LeftOver)
            FilePut(DestFileNum, ByteData)
            For i = 1 To (Numblocks - 1)
                ByteData = rsInfo.Fields("logo").GetChunk(BlockSize)
                FilePut(DestFileNum, ByteData)
            Next i
            FileClose(DestFileNum)
            Me.imgLogo.Source = New System.Windows.Media.Imaging.BitmapImage(New Uri(Me.DiskFile, UriKind.Absolute))
            Dim object1 As Object = CType(0, Integer)
        End If
        Return 0
    End Function


 

End Class
