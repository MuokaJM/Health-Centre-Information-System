
Imports ADODB
Imports System.Data
Imports System.Data.OleDb
Imports System.Text.RegularExpressions
Class pgCWC



    Private lngCRec As Long 'current record
    Private CEdit As Boolean
    Private iAns As Integer
    Private MainWin As New MainWindow
    Private rsCWC As New ADODB.Recordset()
    Private rsQueue As New ADODB.Recordset()
    Private lnPNo As Integer
    Private strAge As String
    Private bnClearQueue As Boolean
    Private bnNew As Boolean = False '
    Private dtPatient As New DataTable
    Private daPatient As New OleDbDataAdapter
    Public dgBrush As New SolidColorBrush
    Private lnQNO As Integer
    Public sUname As String
    Public strUser As String
    Private CWCNo As Long
    Private strPatNo As String

    Private rsPatient As New ADODB.Recordset
    Private strPatientName As String
    Private dtDoB As Date





    Private Sub pgCWC_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized


        lblToday.Content = Format(Today, "dd-MMMM-yy")
        btnSave.IsEnabled = False
        btnCancel.IsEnabled = False
        btnEdit.IsEnabled = True
        txtComments.IsEnabled = False
        txtTreatment.IsEnabled = False
        btnArchive.IsEnabled = True

        Try
            getQueue()
        Catch ex As Exception
            MsgBox("An error has occured while loading queued patients " & Err.Description, MsgBoxStyle.Information)
        End Try


    End Sub


    Private Sub cboPatientNumber_GotFocus(sender As Object, e As RoutedEventArgs) Handles cboPatientNumber.GotFocus
        Dim nQueue As Integer
        Dim rsQ As New ADODB.Recordset
        With rsQ
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='CWC' AND Status='Waiting' AND PatNo NOT LIKE 'RF%' ", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
            nQueue = .RecordCount
            .Close()
        End With

        If cboPatientNumber.Items.Count = nQueue Then Exit Sub

        Try
            getQueue()
        Catch ex As Exception
            MsgBox("An error has occured while loading patients details " & Err.Description)
        End Try
    End Sub

    Private Sub cboPatientNumber_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboPatientNumber.SelectionChanged
        If bnClearQueue = True Then Exit Sub
        ClearCWCData()
        getPatientNumber()
        Dim rsL As New ADODB.Recordset
        If bnNew = True Then
        Else
            GenerateCWCNo()
            lblLSNo.Content = CWCNo
            EditReady()
            bnNew = True
        End If


        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT QNO, QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy, PNO, Remarks FROM tblQueue WHERE QNO= " & lnQNO & " AND destination='CWC' AND Status='Waiting' ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    strPatNo = .Fields("PatNo").Value

                    With rsPatient
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT PNo, Surname, ONames, Sex, DoB, address, subloc FROM tblPatient WHERE PNo=" & CInt(rsQueue.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            lnPNo = .Fields("pno").Value
                            dtDoB = .Fields("DoB").Value
                            GetAge(dtDoB)
                            lblDetails.Content = "Name: " & .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value) & " Sex: " & .Fields("Sex").Value & " Age: " & strAge & " Home: " & .Fields("Address").Value & " " & .Fields("SubLoc").Value
                            strPatientName = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value)
                            bnNew = True
                        End If
                        .Close()
                    End With


                End If
                .Close()
            End With

        Catch ex As Exception
            MsgBox("An error has occured while fetching patient's details " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub GenerateCWCNo()
        Dim rsANCone As New ADODB.Recordset
        Try
            With rsANCone
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblCWC ORDER BY CWCNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    CWCNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    CWCNo = .Fields("CWCNo").Value
                End If
                CWCNo = CWCNo + 1
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating lab service number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub GetAge(DoB As Date)
        Try
            Dim intAge As Integer
            intAge = DateDiff(DateInterval.Year, DoB, Today())

            If intAge > 1 Then
                strAge = intAge & " Years"
            Else
                intAge = DateDiff(DateInterval.Month, DoB, Today())

                If intAge > 1 Then
                    strAge = intAge & " Months"
                Else
                    intAge = DateDiff(DateInterval.Day, DoB, Today())
                    strAge = intAge & " Days"
                End If
            End If
        Catch
            MsgBox("An error has occured while getting patient's age " & Err.Description)
        End Try
    End Sub

    Private Sub getQueue()
        Try
            cboPatientNumber.Items.Clear()
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy, QNO FROM tblQueue WHERE destination='CWC' AND Status='Waiting' AND PatNo NOT LIKE 'RF%' ", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT PNO, Surname, Onames, Sex, PatNo FROM tblPatient WHERE PatNo ='" & rsQueue.Fields("PatNo").Value & "' ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                While .EOF = False
                                    cboPatientNumber.Items.Add(rsQueue.Fields("QNO").Value & " " & .Fields("PNO").Value & " " & .Fields("Surname").Value & " " & .Fields("Onames").Value)
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
            MsgBox("An error has occured while loading queued patients " & Err.Description, MsgBoxStyle.Information)
        End Try

    End Sub

    Public Function getPatientNumber()

        Dim Mchar As String = ""
        Dim cboC As String
        Dim X As Integer
        Dim p As String = ""

        'get queue number
        cboC = cboPatientNumber.SelectedItem
        For X = 1 To Len(cboC)
            Mchar = Mid(cboC, X, 1)
            If Mchar = " " Then Exit For
            p = p + Mchar
        Next X
        lnQNO = Val(p)

        Return (0)
    End Function


    Private Sub btnArchive_Click(sender As Object, e As RoutedEventArgs) Handles btnArchive.Click
        Dim strPNo As String = GetPatSNo(lblDetails.Content)
        Try
            With rsCWC
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblCWC WHERE CWCNo=" & CWCNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                lngCRec = .AbsolutePosition
                If .RecordCount > 0 Then
                    If MsgBox("Do you really want to archive this record? ", MsgBoxStyle.YesNo) = vbYes Then
                        .Fields("status").Value = "Archived"
                        .Update()
                        MsgBox("Record archived!", MsgBoxStyle.Exclamation)
                    End If
                End If
                .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblCWC  WHERE STATUS<>'ARCHIVED' ORDER BY CWCNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .Move(lngCRec)
                    GetCWCData()
                Else
                    ClearCWCData()
                End If
            End With
        Catch
            MsgBox("An error has occured while archiving a record " & Err.Description)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            If CEdit = True Then
                rsCWC.Close()
                CEdit = False
                rsCWC = New ADODB.Recordset()
                rsCWC.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsCWC.Open("SELECT *FROM tblCWC  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsCWC.Move(lngCRec)
            Else
                With rsCWC
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetCWCData()
                    Else
                        MsgBox("Nothing to Cancel")
                        Me.txtWeight.Focus()
                    End If
                End With
            End If
        Catch
            MsgBox("An error has occured while canceling record " & Err.Description)
        End Try
        Try
            btnSave.IsEnabled = False
            btnCancel.IsEnabled = False
        Catch ex As Exception
            MsgBox("An error has occured while changing controls settings " & Err.Description)
        End Try



    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        Try
            lngCRec = rsCWC.AbsolutePosition
            GetPatSNo(lblDetails.Content)
            With rsCWC
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub
                Else
                    EditReady()
                    .Close()
                    rsCWC = New ADODB.Recordset()
                    rsCWC.Open("SELECT * FROM tblCWC WHERE CWCNo=" & CWCNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    CEdit = True
                    btnSave.IsEnabled = True
                    btnCancel.IsEnabled = True
                End If
            End With
        Catch
            MsgBox("An error has occured while preparing to edit record " & Err.Description)
        End Try



    End Sub

    Private Sub btnFind_Click(sender As Object, e As RoutedEventArgs) Handles btnFind.Click
        Try
            Dim nwWin As New Window1
            Dim fiS As New Frame
            Dim ti As New TabItem
            Dim pgPatSearch As New pgPatSearch

            pgPatSearch.strUser = strUser
            fiS.NavigationService.Navigate(pgPatSearch)
            ti.Content = fiS
            nwWin.tcSearch.Items.Add(ti)
            nwWin.Show()
        Catch ex As Exception
            MsgBox("An error has occured while loading search window " & Err.Description)
        End Try


    End Sub

    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs) Handles btnFirst.Click
        Try
            With rsCWC
                If .RecordCount <> 0 Then
                    If .BOF = True Or .EOF = True Then Exit Sub
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                GetCWCData()
                            End If
                        Else
                            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveFirst()
                        btnPrevious.IsEnabled = False
                        btnNext.IsEnabled = True
                        GetCWCData()

                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the first record " & Err.Description)
        End Try

    End Sub


    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click
        Try
            With rsCWC
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnPrevious.IsEnabled = False
                                GetCWCData()
                            End If
                        Else
                            MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveLast()
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = False
                        GetCWCData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the last record " & Err.Description)
        End Try
    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        Try
            With rsCWC
                If .RecordCount <> 0 Then
                    If .EOF = True Or .BOF = True Then
                        .MoveLast()
                        MsgBox("This Is the Last Record", MsgBoxStyle.Information, "Navigation")
                        Exit Sub
                    End If
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnNext.IsEnabled = False
                                GetCWCData()
                            Else
                                .CancelUpdate()
                                .MoveNext()
                                btnPrevious.IsEnabled = True
                                GetCWCData()
                            End If
                        Else
                            MsgBox("Can't Go To Next Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        If .RecordCount = .AbsolutePosition Then
                            .MoveLast()
                            btnNext.IsEnabled = False
                            MsgBox("This Is the Last Record", MsgBoxStyle.Information, "Navigation")
                        ElseIf .EOF = True Then
                            .MoveLast()
                            btnNext.IsEnabled = False
                            MsgBox("This Is the Last Record", MsgBoxStyle.Information, "Navigation")
                        Else
                            .MoveNext()
                        End If
                        btnPrevious.IsEnabled = True
                        GetCWCData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the next record " & Err.Description)
        End Try
    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click
        Try
            With rsCWC
                If .RecordCount <> 0 Then
                    If .BOF = True Or .EOF = True Then
                        .MoveFirst()
                        MsgBox("This Is the first Record", MsgBoxStyle.Information, "Navigation")
                        Exit Sub
                    End If
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                GetCWCData()
                            Else
                                .CancelUpdate()
                                .MovePrevious()
                                btnNext.IsEnabled = True
                                GetCWCData()
                            End If
                        Else
                            MsgBox("Can't Go To previous Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        If .AbsolutePosition = 1 Then
                            .MoveFirst()
                            btnPrevious.IsEnabled = False
                            MsgBox("This Is the first Record", MsgBoxStyle.Information, "Navigation")
                        ElseIf .BOF = True Then
                            .MoveFirst()
                            btnPrevious.IsEnabled = False
                            MsgBox("This Is the first Record", MsgBoxStyle.Information, "Navigation")
                        Else
                            .MovePrevious()
                        End If
                        btnNext.IsEnabled = True
                        GetCWCData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the previous record " & Err.Description)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If CEdit = True Then
            SetCWCData()
            rsCWC.Update()

            MsgBox(" Record Saved", MsgBoxStyle.Information, "Save")
            rsCWC.Close()

            CEdit = False

            rsCWC = New ADODB.Recordset()
            rsCWC.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsCWC.Open("SELECT * FROM tblCWC WHERE STATUS<>'ARCHIVED' ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            rsCWC.Move(lngCRec)
            btnSave.IsEnabled = False
        Else

            If Trim(txtWeight.Text) = "" Then
                MsgBox("Enter Weight ", MsgBoxStyle.Information)
                txtWeight.Focus()
            ElseIf Trim(txtHeight.Text) = "" Then
                MsgBox("Please enter height", MsgBoxStyle.Information)
                txtHeight.Focus()
            ElseIf Trim(txtImmunization.Text) = "" Then
                MsgBox("Please enter any immunization done", MsgBoxStyle.Information)
                txtImmunization.Focus()
            ElseIf Trim(txtTreatment.Text) = "" Then
                MsgBox("Please enter any treatment given", MsgBoxStyle.Information)
                txtTreatment.Focus()
            ElseIf (Trim(txtComments.Text)) = "" Then
                MsgBox("Enter comments", MsgBoxStyle.Information)
                txtComments.Focus()
            Else
                With rsCWC
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblCWC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    .AddNew()
                    SetCWCData()
                    .Update()
                    MsgBox("Record Saved!", MsgBoxStyle.Information)
                    updateQueue()
                    btnSave.IsEnabled = False
                    btnCancel.IsEnabled = False
                    btnEdit.IsEnabled = True
                    txtComments.IsEnabled = False
                    txtTreatment.IsEnabled = False
                    btnArchive.IsEnabled = True
                End With
            End If
        End If

    End Sub

    Private Sub updateQueue()
        Dim rsQueueUpdate As New ADODB.Recordset
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


            With rsQueueUpdate
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblQueue WHERE PatNo='" & strPatNo & "' AND status='Waiting' AND DESTINATION='CWC' ORDER BY qno Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
                Etime = System.DateTime.FromOADate(CDate(.Fields("QTime").Value).ToOADate - CDate(.Fields("ATime").Value).ToOADate)
                .Fields("QTTime").Value = Etime
                .Update()
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while updating queue details " & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub


    Private Function GetPatSNo(strNo As String)
        Try
            Dim Mchar As String = ""
            Dim intLength As Integer
            Dim X As Integer
            Dim iMNo As String = ""
            Dim iSNo As String = ""

            intLength = Len(strNo)
            For X = intLength To 0 Step -1
                Mchar = Mid(strNo, X, 1)
                If Mchar = "-" Then Exit For
                iMNo = iMNo + Mchar
            Next X

            CWCNo = Val(Right(strNo, Len(iMNo)))

        Catch ex As Exception
            MsgBox("An error has occured while getting patient serial number " & Err.Description)
        End Try

        Return CWCNo
    End Function


    Private Sub ClearCWCData()

        txtComments.Text = ""
        txtTreatment.Text = ""
        txtHeight.Text = ""
        txtImmunization.Text = ""
        txtWeight.Text = ""

    End Sub

    Private Sub GetCWCData()
        Dim rsPreviousClinic As New ADODB.Recordset
        Try
            With rsCWC
                lblLSNo.Content = .Fields("CWCNO").Value
                txtComments.Text = .Fields("Comment").Value
                txtTreatment.Text = .Fields("treatment").Value
                txtHeight.Text = .Fields("Height").Value
                txtImmunization.Text = .Fields("immunization").Value
                txtWeight.Text = .Fields("weight").Value
                lblRecNo.Content = "Record " & .AbsolutePosition & " Of " & .RecordCount & " Records"
            End With
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try

        Try
            With rsPreviousClinic
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT PNO, VisitDate, Weight, Height, Immunization, Treatment, Comment, uname as Medic FROM tblCWC WHERE PNO=" & lnPNo & "  ORDER BY CWCNo DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then

                End If

            End With
            daPatient.Fill(dtPatient, rsPreviousClinic)

            dgHistory.ItemsSource = dtPatient.DefaultView
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try


    End Sub


    Private Sub SetCWCData()
        Try
            With rsCWC
                .Fields("CWCNO").Value = CWCNo
                .Fields("PNO").Value = lnPNo
                .Fields("Comment").Value = txtComments.Text
                .Fields("treatment").Value = txtTreatment.Text
                .Fields("Height").Value = txtHeight.Text
                .Fields("immunization").Value = txtImmunization.Text
                .Fields("weight").Value = txtWeight.Text
                .Fields("Uname").Value = strUser
                lblRecNo.Content = "Record " & .AbsolutePosition & " Of " & .RecordCount & " Records"
            End With
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try

    End Sub

    Private Sub EditReady()
        txtWeight.IsEnabled = True
        txtComments.IsEnabled = True
        txtTreatment.IsEnabled = True
        txtHeight.IsEnabled = True
        txtImmunization.IsEnabled = True
        btnArchive.IsEnabled = False
        btnSave.IsEnabled = True
        btnCancel.IsEnabled = True
        btnEdit.IsEnabled = True
        btnFind.IsEnabled = False
        txtWeight.Focus()
    End Sub

    Private Sub txtWeight_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtWeight.LostFocus
        If IsNumeric(txtWeight.Text) = False Then
            MsgBox("Please enter weight in numbers only e.g 5 instead of 5 kgs")
            txtWeight.SelectAll()
        End If
    End Sub
End Class
