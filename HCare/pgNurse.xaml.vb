Imports System.Globalization
Imports System.Data
Imports System.Data.OleDb
Imports System.Text.RegularExpressions
Imports ADODB
Imports System.Windows.Forms
Imports System.Drawing.Icon
Imports System
Imports System.Text
Imports System.Collections
Imports Microsoft.SqlServer
Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine



Class pgNurse
    Inherits System.Windows.Controls.Page
    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
     Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
     Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
     Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}

    Private myBrush As New SolidColorBrush
    Public intTheme As Integer
    Private strColor As String

    Private notifyIcon As New System.Windows.Forms.NotifyIcon()

    Private lngCRec As Long '
    Private CEdit As Boolean
    Public rsNurse As New ADODB.Recordset
    Public rsPatient As New ADODB.Recordset
    Public rsQueue As New ADODB.Recordset
    Private MainWin As New MainWindow
    Private lnPNO As Integer '
    Private lnVNo As Integer
     Public strUser As String
    Private strPName As String
    Private lnQNo As Long
    Private rsU As New ADODB.Recordset()
    Public dgBrush As New SolidColorBrush
    Private bnEsc As Boolean '
    Private bnNew As Boolean = False '
    Private bnClearQueue As Boolean
    Private nQueue As Integer '
    Private strSendTo As String '



    Private Sub pgNurse_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        lblToday.Content = Format(Today, "dd-MMMM-yy")

    End Sub

    Private Sub cboPNo_GotFocus(sender As Object, e As RoutedEventArgs) Handles cboPNo.GotFocus
        Try
            Dim rsQ As New ADODB.Recordset
            With rsQ
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Nurse' AND Status='Waiting' ", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                nQueue = .RecordCount
                .Close()
            End With
            If cboPNo.Items.Count = nQueue Then Exit Sub
            LoadScheduledPatients()
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try


    End Sub

    Private Sub cboPNo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboPNo.SelectionChanged
        Try
            If bnClearQueue = True Then Exit Sub
            lblToday.Content = Today
            lblNo.Content = lnVNo
            strPName = ""
            strSendTo = ""
            lnPNO = 0
            getPatientNumber(cboPNo.SelectedItem)
        Catch ex As Exception
            MsgBox("An error has occured while fetching patient details " & Err.Description)
        End Try

        Try
            With rsPatient
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPatient WHERE PNO=" & lnPNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    strPName = .Fields("Surname").Value & " " & .Fields("ONames").Value
                    lblDetails.Content = .Fields("Surname").Value & " " & .Fields("ONames").Value
                End If

                .Close()
                If bnNew = False Then CreateNewRecord()

            End With

        Catch ex As Exception
            MsgBox("An error has occured while loading patients data " & Err.Description)
        End Try


    End Sub

    Private Sub LoadScheduledPatients()
        cboPNo.Items.Clear()
        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Nurse' AND Status='Waiting'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = CursorLocationEnum.adUseClient
                            .Open("SELECT PNO, Surname, Onames, Sex, PatNo FROM tblPatient WHERE PatNo ='" & rsQueue.Fields("PatNo").Value & "' ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                While .EOF = False
                                    cboPNo.Items.Add(.Fields("PNO").Value & " " & .Fields("Surname").Value & " " & .Fields("Onames").Value)
                                    .MoveNext()
                                End While
                            End If
                            .Close()
                        End With
                        .MoveNext()
                    End While
                End If
                .Close()
            End With
        Catch ex As Exception


            MsgBox("An error has occured while loading queued patients details " & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub

    Public Function getPatientNumber(cboC As String)
        Try
            Dim Mchar As String = ""
            Dim X As Integer
            Dim p As String = ""

            For X = 1 To Len(cboC)
                Mchar = Mid(cboC, X, 1)
                If Mchar = " " Then Exit For
                p = p + Mchar
            Next X
            lnPNO = Val(p)
        Catch ex As Exception
            MsgBox("An error has occured while getting patient number " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Dim rsU As New ADODB.Recordset
        Try
            If CEdit = True Then

            Else
                If txtService.Text = "" Then
                    MsgBox("Please enter the service done", MsgBoxStyle.Information, "Save")
                    txtService.Focus()

                Else

                    With rsNurse
                        If .State = 1 Then .Close()
                        .CursorLocation = CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblNurse", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .BOF And .EOF Then
                            lnVNo = 0
                        Else
                            If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                            .MoveLast()
                            lnVNo = .Fields("VisitingNo").Value
                        End If
                        lnVNo = lnVNo + 1
                        lblNo.Content = lnVNo
                        .AddNew()
                        .Fields("VisitingNo").Value = lnVNo
                        .Fields("PNo").Value = lnPNO
                        .Fields("Service").Value = txtService.Text
                        .Fields("Comment").Value = txtComment.Text
                        .Fields("Uname").Value = strUser
                        .Fields("Vdate").Value = Today
                        .Update()
                        .Close()
                        updateQueue()

                        MsgBox("Record Saved!", MsgBoxStyle.Information, "Save")
                        bnClearQueue = True
                        LoadScheduledPatients()
                        bnClearQueue = False
                        btnSave.IsEnabled = False
                        bnNew = False '

                    End With
                End If
            End If

        Catch ex As Exception
            MsgBox("An error has occured while saving data " & Err.Description, MsgBoxStyle.Critical)
        End Try

    End Sub


    Private Sub updateQueue()
        Dim rsQueue As New ADODB.Recordset
        Dim rsPa As New ADODB.Recordset
        Dim rsU As New ADODB.Recordset
        Dim strP As String
        Dim Etime As Date
        Try
            With rsPa
                .Open("SELECT * FROM tblPatient WHERE PNO=" & lnPNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                strP = .Fields("PatNo").Value
                .Close()
            End With


            With rsQueue
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblQueue WHERE PatNo='" & strP & "' AND status='Waiting' AND DESTINATION='Nurse' ORDER BY qno Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    .Close()
                    Exit Sub
                End If
                .Fields("Status").Value = "Attended"
                .Fields("ADate").Value = Today
                .Fields("ATime").Value = Format(Now, "Long Time")
                rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                .Fields("AttendedBy").Value = strUser & " " & rsU.Fields("Designation").Value
                rsU.Close()
                Etime = System.DateTime.FromOADate(CDate(rsQueue.Fields("QTime").Value).ToOADate - CDate(rsQueue.Fields("ATime").Value).ToOADate)
                .Fields("QTTime").Value = Etime
                .Update()
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while updating queue details " & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub CreateNewRecord()
        If bnNew = True Then Exit Sub '
        lnVNo = 0

        With rsNurse
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblNurse ORDER BY visitingno", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF And .EOF Then
                lnVNo = 0
            Else
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                .MoveLast()
                lnVNo = .Fields("visitingno").Value
            End If
            lnVNo = lnVNo + 1
            lblNo.Content = lnVNo
            .Close()
            bnNew = False
            lblDetails.Content = ""
            txtComment.Text = ""
            txtService.Text = ""

        End With


    End Sub
End Class
