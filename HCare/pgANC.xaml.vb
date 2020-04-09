Imports ADODB
Imports System.Data
Imports System.Data.OleDb
Imports System.Text.RegularExpressions

Class pgANC

    Private lngCRec As Long 'current record
    Private CEdit As Boolean
    Private MainWin As New MainWindow
    Private rsANC As New ADODB.Recordset()
    Private rsQueue As New ADODB.Recordset()
    Private lnPNo As Integer
    Private bnClearQueue As Boolean
    Private bnNew As Boolean = False 'check if new record procedure has been called
    Private lnQNO As Integer
    Public strUser As String
    Private strAge As String
    Private ANCNo As Long
    Private strPatNo As String
    Private rsPatient As New ADODB.Recordset
    Private strPatientName As String
    Private dtDoB As Date
    Private dtPatient As New DataTable
    Private daPatient As New OleDbDataAdapter
    Public dgBrush As New SolidColorBrush


    Private Sub pgANC_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        lblToday.Content = Format(Today, "dd-MMMM-yy") 'Today
        dtpLMP.SelectedDate = Today
        btnCancel.IsEnabled = False
        btnSave.IsEnabled = False
        txtBPPulse.IsEnabled = False
        txtComments.IsEnabled = False
        txtFHR.IsEnabled = False
        txtGestation.IsEnabled = False
        txtGravida.IsEnabled = False
        txtLie.IsEnabled = False
        dtpLMP.IsEnabled = False
        txtMaturity.IsEnabled = False
        txtParity.IsEnabled = False
        txtPosition.IsEnabled = False
        txtTCA.IsEnabled = False
        btnArchive.IsEnabled = False

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
            .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='ANC' AND Status='Waiting' AND PatNo NOT LIKE 'RF%' ", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
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
        ClearANCData()
        getPatientNumber()
        Dim rsL As New ADODB.Recordset
        If bnNew = True Then
        Else
            GenerateANCNo()
            lblLSNo.Content = ANCNo
            EditReady()
            bnNew = True
        End If


        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT QNO, QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy, PNO, Remarks FROM tblQueue WHERE QNO= " & lnQNO & " AND destination='ANC' AND Status='Waiting' ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
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
                .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy, QNO FROM tblQueue WHERE destination='ANC' AND Status='Waiting' AND PatNo NOT LIKE 'RF%' ", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
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

    Private Sub EditReady()
        txtBPPulse.IsEnabled = True
        txtComments.IsEnabled = True
        txtFHR.IsEnabled = True
        txtGestation.IsEnabled = True
        txtGravida.IsEnabled = True
        txtLie.IsEnabled = True
        dtpLMP.IsEnabled = True
        txtMaturity.IsEnabled = True
        txtParity.IsEnabled = True
        txtPosition.IsEnabled = True
        txtTCA.IsEnabled = True
        btnArchive.IsEnabled = False

        btnSave.IsEnabled = True
        btnCancel.IsEnabled = True
        btnEdit.IsEnabled = True
        btnFind.IsEnabled = False
        txtGestation.Focus()

    End Sub

    Private Sub GenerateANCNo()
        Dim rsANCone As New ADODB.Recordset
        Try
            With rsANCone
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblANC ORDER BY ANCNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    ANCNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    ANCNo = .Fields("ANCNo").Value
                End If
                ANCNo = ANCNo + 1
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating lab service number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub


    Private Sub btnArchive_Click(sender As Object, e As RoutedEventArgs) Handles btnArchive.Click
        Dim strPNo As String = GetPatSNo(lblDetails.Content)
        Try
            With rsANC
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblANC WHERE ANCNo=" & ANCNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
                .Open("SELECT * FROM tblANC  WHERE STATUS<>'ARCHIVED' ORDER BY ANCNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .Move(lngCRec)
                    GetANCData()
                Else
                    ClearANCData()
                End If
            End With
        Catch
            MsgBox("An error has occured while archiving a record " & Err.Description)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            If CEdit = True Then
                rsANC.Close()
                CEdit = False
                rsANC = New ADODB.Recordset()
                rsANC.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsANC.Open("SELECT *FROM tblANC  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsANC.Move(lngCRec)
            Else
                With rsANC
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetANCData()
                    Else
                        MsgBox("Nothing to Cancel")
                        Me.txtGestation.Focus()
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
            lngCRec = rsANC.AbsolutePosition
            GetPatSNo(lblDetails.Content)
            With rsANC
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub
                Else
                    EditReady()
                    .Close()
                    rsANC = New ADODB.Recordset()
                    rsANC.Open("SELECT * FROM tblANC WHERE ANCNo=" & ANCNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
            With rsANC
                If .RecordCount <> 0 Then
                    If .BOF = True Or .EOF = True Then Exit Sub
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                GetANCData()
                            End If
                        Else
                            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveFirst()
                        btnPrevious.IsEnabled = False
                        btnNext.IsEnabled = True
                        GetANCData()

                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the first record " & Err.Description)
        End Try

    End Sub


    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click
        Try
            With rsANC
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnPrevious.IsEnabled = False
                                GetANCData()
                            End If
                        Else
                            MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveLast()
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = False
                        GetANCData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the last record " & Err.Description)
        End Try
    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        Try
            With rsANC
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
                                GetANCData()
                            Else
                                .CancelUpdate()
                                .MoveNext()
                                btnPrevious.IsEnabled = True
                                GetANCData()
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
                        GetANCData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the next record " & Err.Description)
        End Try
    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click
        Try
            With rsANC
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
                                GetANCData()
                            Else
                                .CancelUpdate()
                                .MovePrevious()
                                btnNext.IsEnabled = True
                                GetANCData()
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
                        GetANCData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the previous record " & Err.Description)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If CEdit = True Then
            SetANCData()
            rsANC.Update()

            MsgBox(" Record Saved", MsgBoxStyle.Information, "Save")
            rsANC.Close()

            CEdit = False

            rsANC = New ADODB.Recordset()
            rsANC.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsANC.Open("SELECT * FROM tblANC WHERE STATUS<>'ARCHIVED' ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            rsANC.Move(lngCRec)
            btnSave.IsEnabled = False
        Else
            'check that all fields ave been filled

            If Trim(txtGestation.Text) = "" Then
                MsgBox("Enter gestation ", MsgBoxStyle.Information)
                txtGestation.Focus()
            ElseIf Trim(txtBPPulse.Text) = "" Then
                MsgBox("Please BP & Pulse", MsgBoxStyle.Information)
                txtBPPulse.Focus()
            ElseIf Trim(txtParity.Text) = "" Then
                MsgBox("Please enter the parity", MsgBoxStyle.Information)
                txtParity.Focus()
            ElseIf Trim(txtGravida.Text) = "" Then
                MsgBox("Please gravida", MsgBoxStyle.Information)
                txtGravida.Focus()
            ElseIf dtpLMP.SelectedDate = Today Then
                MsgBox("Please select date for LMP", MsgBoxStyle.Information)
                dtpLMP.Focus()
            ElseIf Trim(txtFHR.Text) = "" Then
                MsgBox("Please FHR", MsgBoxStyle.Information)
                txtFHR.Focus()
            ElseIf Trim(txtLie.Text) = "" Then
                MsgBox("Please enter lie", MsgBoxStyle.Information)
                txtLie.Focus()
            ElseIf Trim(txtPosition.Text) = "" Then
                MsgBox("Please enter the position", MsgBoxStyle.Information)
                txtPosition.Focus()
            ElseIf (Trim(txtMaturity.Text)) = "" Then
                MsgBox("enter maturity", MsgBoxStyle.Information)
                txtMaturity.Focus()
            ElseIf Trim(txtTCA.Text) = "" Then
                MsgBox("Please enter the TCA", MsgBoxStyle.Information)
                txtTCA.Focus()
            ElseIf (Trim(txtComments.Text)) = "" Then
                MsgBox("Enter comments", MsgBoxStyle.Information)
                txtComments.Focus()
            Else
                With rsANC
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblANC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    .AddNew()
                    SetANCData()
                    .Update()
                    MsgBox("Record Saved!", MsgBoxStyle.Information)
                    updateQueue()

                    btnSave.IsEnabled = False
                    btnCancel.IsEnabled = False
                    btnEdit.IsEnabled = True
                    txtBPPulse.IsEnabled = False
                    txtComments.IsEnabled = False
                    txtFHR.IsEnabled = False
                    txtGestation.IsEnabled = False
                    txtGravida.IsEnabled = False
                    txtLie.IsEnabled = False
                    dtpLMP.IsEnabled = False
                    txtMaturity.IsEnabled = False
                    txtParity.IsEnabled = False
                    txtPosition.IsEnabled = False
                    txtTCA.IsEnabled = False
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
                .Open("SELECT * FROM tblQueue WHERE PatNo='" & strPatNo & "' AND status='Waiting' AND DESTINATION='ANC' ORDER BY qno Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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

            ANCNo = Val(Right(strNo, Len(iMNo)))

        Catch ex As Exception
            MsgBox("An error has occured while getting patient serial number " & Err.Description)
        End Try

        Return ANCNo
    End Function


    Private Function GetANCData()


        Try
            With rsANC
                lnPNo = .Fields("PNo").Value
                txtGestation.Text = .Fields("Gestation").Value
                txtBPPulse.Text = .Fields("BPPulse").Value
                Today = .Fields("VisitDate").Value
                txtParity.Text = .Fields("Parity").Value
                txtGravida.Text = .Fields("Gravidia").Value
                dtpLMP.SelectedDate = CDate(.Fields("LMP").Value)
                txtFHR.Text = .Fields("FHR").Value
                txtLie.Text = .Fields("Lie").Value
                txtPosition = .Fields("Position").Value
                txtMaturity.Text = .Fields("Maturity").Value
                txtTCA.Text = .Fields("TCA").Value
                txtComments.Text = .Fields("Comment").Value
                lblRecNo.Content = "Record " & .AbsolutePosition & " Of " & .RecordCount & " Records"
            End With
            previousclinic()
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Sub previousClinic()
        Dim rsPreviousClinic As New ADODB.Recordset

        Try
            With rsPreviousClinic
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT PNO, VisitDate, Gestation, BPPulse, Parity, Gravida, LMP, FHR, Lie, Position, Maturity, TCA, Comment, uname as Medic FROM tblANC WHERE PNO=" & lnPNo & "  ORDER BY ANCNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    daPatient.Fill(dtPatient, rsPreviousClinic)
                    dgHistory.ItemsSource = dtPatient.DefaultView
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try
    End Sub

    Private Sub familyHistory()
        Dim rsPreviousHistory As New ADODB.Recordset
        Try
            With rsPreviousHistory
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT PNO, Allergies, SurgicalOperation, Diabetic, Hypertensive, BloodTransfusion, Tuberculosis, TwinsInFamily, TuberculosisInFamily, DiabetesInFamily, HypertensionInFamily FROM tblPatient WHERE PNO=" & lnPNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    daPatient.Fill(dtPatient, rsPreviousHistory)
                    dgHistory.ItemsSource = dtPatient.DefaultView
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try
    End Sub



    Private Function SetANCData()
        Try
            With rsANC
                .Fields("ANCNO").Value = ANCNo
                .Fields("PNo").Value = lnPNo
                .Fields("Gestation").Value = txtGestation.Text
                .Fields("BPPulse").Value = txtBPPulse.Text
                .Fields("VisitDate").Value = Today
                .Fields("Parity").Value = txtParity.Text
                .Fields("Gravida").Value = txtGravida.Text
                .Fields("LMP").Value = dtpLMP.SelectedDate
                .Fields("FHR").Value = txtFHR.Text
                .Fields("Lie").Value = txtLie.Text
                .Fields("Position").Value = txtPosition.Text
                .Fields("Maturity").Value = txtMaturity.Text
                .Fields("TCA").Value = txtTCA.Text
                .Fields("Comment").Value = txtComments.Text
                .Fields("UName").Value = strUser
            End With
        Catch
            MsgBox("An error has occured while setting patient data for saving  " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Function ClearANCData()
        Try
            lblDetails.Content = ""
            txtBPPulse.Text = ""
            txtComments.Text = ""
            txtFHR.Text = ""
            txtGestation.Text = ""
            txtGravida.Text = ""
            txtLie.Text = ""
            dtpLMP.SelectedDate = Today
            txtMaturity.Text = ""
            txtParity.Text = ""
            txtPosition.Text = ""
            txtTCA.Text = ""
            lblRecNo.Content = ""

        Catch ex As Exception
            MsgBox("An error has occured while clearing patients' fields " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Sub dtpLMP_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpLMP.SelectedDateChanged
        Try
            If dtpLMP.SelectedDate > Today Then
                MsgBox("LMP cannot be in future")
                dtpLMP.SelectedDate = Today
            End If
        Catch ex As Exception
            MsgBox("An error has occured " & Err.Description)
        End Try

    End Sub

    Private Sub chkFamilyHistory_Click(sender As Object, e As RoutedEventArgs) Handles chkFamilyHistory.Click
        dgHistory.ItemsSource = ""
        dtPatient.Clear()
        If chkFamilyHistory.IsChecked = True Then
            familyHistory()
        ElseIf chkFamilyHistory.IsChecked = False Then
            previousClinic()
        End If
    End Sub
End Class
