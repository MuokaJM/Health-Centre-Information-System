Imports Microsoft.SqlServer
Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports ADODB
Imports System.Windows.Forms.Application
Imports Microsoft.VisualBasic
Imports System
Imports System.IO
Imports System.Reflection.Assembly



Public Class winReg
    Public cnHecom As New ADODB.Connection
    Public rsInfo As New ADODB.Recordset
    Public strConn As String
    Public bnSplash As Boolean
    Private CusNo As Long
    Private CEdit As Boolean



   
    Dim cn As ADODB.Connection
    Dim rs As ADODB.Recordset
    Dim strSQL As String
    Dim FileLength As Integer '
    Dim Numblocks As Short
    Dim LeftOver As Integer
    Dim i As Short
    Const BlockSize As Integer = 100000 'This size can be experimented with for
    Public PictBmp As String
    Dim ByteData() As Byte  'Byte array for Blob data.
    Dim SourceFile As Short ' Open the BlobTable table.
    Private PicFile As String '
    Private strPath As String ' 
    Private iAns As Short
    Private errNo As Integer
    Private oPicDlg As New Microsoft.Win32.OpenFileDialog
    Private DiskFile As String
    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Close()
    End Sub

    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        If MsgBox("The change you are about to make cannot be reversed any previous data will be deleted!", vbExclamation + vbYesNo, "Close") = vbYes Then
            If txtName.Text = "" Then
                MsgBox("Please enter the business name ", vbInformation)
                txtName.Focus()
            ElseIf txtPAdd.Text = "" Then
                MsgBox("Please enter the Address of the Business", vbInformation)
                txtPAdd.Focus()
            ElseIf txtTel.Text = "" Then
                MsgBox("Please enter the business telephone contact", vbInformation)
                txtTel.Focus()
            ElseIf txtBLine.Text = "" Then
                MsgBox("Please enter the business statement", vbInformation)
                txtBLine.Focus()
            ElseIf txtLicensed.Text = "" Then
                MsgBox("Please enter the name of the licensee ", vbInformation)
                txtLicensed.Focus()
            End If
            Try
                If cnHecom.State <> 1 Then cnHecom.Open(strConn)
                With rsInfo
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblInfo", cnHecom, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .EOF = True And .BOF = True Then

                    Else
                        .Delete()
                    End If

                    .AddNew()
                    SetRegData()
                    SavePic()
                    .Update()
                    .Close()
                End With
                cnHecom.Close()
            Catch ex As Exception
                MsgBox("error while updating information")
            End Try


            Try
                'Save Settings to Registry
                SaveSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="UB", Setting:=txtName.Text)
                SaveSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="Title", Setting:=txtLicensed.Text)
                SaveSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="Add", Setting:=txtPAdd.Text)
                SaveSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="qt", Setting:=txtBLine.Text)
                SaveSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="TM", Setting:=txtTel.Text)
            Catch ex As Exception

            End Try
            Close()
        Else

        End If


    End Sub

    Private Sub winReg_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        Dim str As String = ""
        Try
            Me.strPath = GetExecutingAssembly.Location
            Do While (str <> "\")
                str = Strings.Mid(Me.strPath, Strings.Len(Me.strPath), 1)
                Me.strPath = Me.strPath.Remove((Strings.Len(Me.strPath) - 1), 1)
            Loop
            Me.PicFile = Me.strPath & "\TempFile.bmp"
            Me.DiskFile = Me.strPath & "\logo.bmp"
        Catch ex As Exception

        End Try


        Try

            Me.txtName.Text = GetSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="UB", Default:="")
            Me.txtLicensed.Text = GetSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="Title", Default:="")
            Me.txtPAdd.Text = GetSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="Add", Default:="")
            Me.txtBLine.Text = GetSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="qt", Default:="")
            Me.txtTel.Text = GetSetting(AppName:="Alpha Solutions\HCIS", Section:="Details", Key:="TM", Default:="")
        Catch ex As Exception

        End Try


        Try
            strConn = "Provider=SQLOLEDB;Data Source=MASII8-PC\SQLEXPRESS;Initial Catalog=HCISDB;Integrated Security=SSPI;"
            ''  strConn = "Provider=SQLOLEDB;Data Source=(LocalDB);Initial Catalog=HCISDB;User ID=sa;Password=******"
            If cnHecom.State <> 1 Then cnHecom.Open(strConn)
            rsInfo.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsInfo.Open("SELECT * FROM tblInfo", cnHecom, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            GetRegData()
            GetPic()
            rsInfo.Close()
            cnHecom.Close()
        Catch ex As Exception

        End Try

    End Sub



    Public Function SavePic()
        Dim i As Integer
        Try
            SourceFile = FreeFile()
            If Len(Dir(PicFile)) > 0 Then
                Kill(PicFile)
            End If
            File.Copy(PictBmp, PicFile, True)
            FileOpen(SourceFile, PicFile, OpenMode.Binary, OpenAccess.Read, , -1)  '
            FileLength = LOF(SourceFile) ' 
            If FileLength = 0 Then
                FileClose(SourceFile)
                MsgBox(PictBmp & " Empty or not found.")
                Return 0
                Exit Function
            Else
                Numblocks = FileLength / BlockSize
                LeftOver = FileLength Mod BlockSize
                ReDim ByteData(LeftOver - 1)
                FileGet(SourceFile, ByteData) '
                rsInfo.Fields("logo").AppendChunk(ByteData)
                ReDim ByteData(BlockSize - 1)
                For i = 1 To Numblocks
                    FileGet(SourceFile, ByteData)
                    rsInfo.Fields("logo").AppendChunk(ByteData)
                Next i
            End If

        Catch ex As Exception

        End Try

        Return 0

    End Function


    Public Function GetPic() As Object
        Dim DestFileNum As Short
        Dim i As Integer
        Me.FileLength = 0
        Try
            FileSystem.Kill(Me.DiskFile)
            If (Strings.Len(FileSystem.Dir(Me.DiskFile, FileAttribute.Normal)) > 0) Then
                FileSystem.Kill(Me.DiskFile)
            End If

        Catch exception2 As Exception
            Me.DiskFile = Me.strPath & "\logo.bmp"
            Try
                If (Strings.Len(FileSystem.Dir(Me.DiskFile, FileAttribute.Normal)) > 0) Then
                    FileSystem.Kill(Me.DiskFile)
                End If

            Catch exception1 As Exception
                Me.DiskFile = Me.strPath & "\logo.bmp"
                If (Strings.Len(FileSystem.Dir(Me.DiskFile, FileAttribute.Normal)) > 0) Then
                    FileSystem.Kill(Me.DiskFile)
                End If
                Throw
                Interaction.MsgBox("File cannot be deleted!", MsgBoxStyle.Exclamation, Nothing)
            End Try
        End Try
        FileLength = rsInfo.Fields.Item("logo").ActualSize

        If (FileLength = 0) Then
            Return CType(0, Integer)
            Exit Function 'dont load empty
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

    Private Sub btnBrowse_Click(sender As Object, e As RoutedEventArgs) Handles btnBrowse.Click

        oPicDlg.Filter = "Image Files(*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG)| *.BMP;*.JPG;*.JPEG;*.GIF;*.PNG|All Files(*.*)|*.*"
        oPicDlg.ShowDialog()
        If (oPicDlg.FileName <> "") Then
            PictBmp = oPicDlg.FileName
            Me.imgLogo.Source = New System.Windows.Media.Imaging.BitmapImage(New Uri(oPicDlg.FileName, UriKind.Absolute))
            Me.txtLogo.Text = oPicDlg.FileName
            FileSystem.FileClose(New Integer(-1) {})
        End If

    End Sub

    Private Sub SetRegData()
        Try
            With rsInfo
                .Fields("Title").Value = Me.txtLicensed.Text
                .Fields("Title").Value = Me.txtName.Text
                .Fields("PAddress").Value = Me.txtPAddress.Text
                .Fields("PhysicalAddress").Value = Me.txtPAdd.Text
                .Fields("Telephone").Value = Trim(Me.txtTel.Text)
                .Fields("cphone").Value = Trim(Me.txtCPhone.Text)
                .Fields("BusinessLine").Value = Me.txtBLine.Text
            End With

        Catch ex As Exception

        End Try
    End Sub





    Private Sub GetRegData()
        Try
            If rsInfo.EOF = True And rsInfo.BOF = True Then

            Else
                Me.txtLicensed.Text = (rsInfo.Fields.Item("Title").Value)
                Me.txtName.Text = (rsInfo.Fields.Item("Title").Value)
                Me.txtPAddress.Text = (rsInfo.Fields.Item("PAddress").Value)
                Me.txtPAdd.Text = (rsInfo.Fields.Item("PhysicalAddress").Value)
                If Not Information.IsDBNull((rsInfo.Fields.Item("Telephone").Value)) Then
                    Me.txtTel.Text = (rsInfo.Fields.Item("Telephone").Value)
                End If
                If Not Information.IsDBNull((rsInfo.Fields.Item("cphone").Value)) Then
                    Me.txtCPhone.Text = (rsInfo.Fields.Item("cphone").Value)
                End If
                If IsDBNull(rsInfo.Fields.Item("BusinessLine").Value) = False Then
                    Me.txtBLine.Text = (rsInfo.Fields.Item("BusinessLine").Value)
                End If
            End If
        Catch ex As Exception
            Interaction.MsgBox((Information.Err.Description), MsgBoxStyle.ApplicationModal, Nothing)

        End Try
    End Sub




End Class
