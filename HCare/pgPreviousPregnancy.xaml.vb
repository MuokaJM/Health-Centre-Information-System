Imports ADODB
Imports System.Text.RegularExpressions

Class pgPreviousPregnancy

    Private lngCRec As Long '
    Private CEdit As Boolean
    Private iAns As Integer
    Private MainWin As New MainWindow
    Private rsPreviousPregnancy As New ADODB.Recordset()
    Private rsQueue As New ADODB.Recordset()
    Private lnPNo As Integer

    Private bnClearQueue As Boolean
    Private bnNew As Boolean = False '


    Private lnQNO As Integer
    Public sUname As String
    Public strUser As String
    Private strAge As String
    Private PPNO As Long
    Private strPatNo As String
    Private rsPatient As New ADODB.Recordset
   
    Private strPatientName As String
    Private dtDoB As Date
    

    Private Sub pgPreviousPregnancy_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        lblToday.Content = Format(Today, "dd-MMMM-yy")
        btnCancel.IsEnabled = False
        btnSave.IsEnabled = False
        txtPregnancyYear.IsEnabled = False
        txtPuerperium.IsEnabled = False
        txtLabourDuration.IsEnabled = False
        txtPregnancyNumber.IsEnabled = False
        txtPlaceOfDelivery.IsEnabled = False
        txtTypeOfDelivery.IsEnabled = False
        txtMaturity.IsEnabled = False
        txtSex.IsEnabled = False
        txtTimesAttendedANC.IsEnabled = False
        txtBirthWeight.IsEnabled = False
        txtOutcome.IsEnabled = False
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
        ClearPPData()
        getPatientNumber()
        Dim rsL As New ADODB.Recordset

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
                        .Open("SELECT PNo, Surname, ONames, Sex, DoB FROM tblPatient WHERE PNo=" & CInt(rsQueue.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            lnPNo = .Fields("pno").Value
                            dtDoB = .Fields("DoB").Value
                            GetAge(dtDoB)
                            lblDetails.Content = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value) & " " & .Fields("Sex").Value
                            strPatientName = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value)
                            lblDetails.Content = lnPNo & ": " & lblDetails.Content & " Age: " & strAge
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
            Dim dtDtp As Date
            intAge = DateDiff(DateInterval.Year, DoB, Today())
            If intAge > 1 Then
                strAge = intAge & " Years"
            Else
                intAge = DateDiff(DateInterval.Month, dtDtp, Today())

                If intAge > 1 Then
                    strAge = intAge & " Months"
                Else
                    intAge = DateDiff(DateInterval.Day, dtDtp, Today())
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
        txtPregnancyYear.IsEnabled = True
        txtPuerperium.IsEnabled = True
        txtLabourDuration.IsEnabled = True
        txtPregnancyNumber.IsEnabled = True
        txtPlaceOfDelivery.IsEnabled = True
        txtTypeOfDelivery.IsEnabled = True
        txtMaturity.IsEnabled = True
        txtSex.IsEnabled = True
        txtTimesAttendedANC.IsEnabled = True
        txtBirthWeight.IsEnabled = True
        txtOutcome.IsEnabled = True
        btnArchive.IsEnabled = False

        btnSave.IsEnabled = True
        btnCancel.IsEnabled = True
        btnEdit.IsEnabled = False
        btnFind.IsEnabled = False


    End Sub

    Private Sub GeneratePPNO()
        Dim rsPP As New ADODB.Recordset
        Try
            With rsPP
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPPregnancy ORDER BY PPNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    PPNO = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    PPNO = .Fields("PPNO").Value
                End If
                PPNO = PPNO + 1
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating lab service number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub


    Private Sub btnArchive_Click(sender As Object, e As RoutedEventArgs) Handles btnArchive.Click
        Dim strPNo As String = GetPatSNo(lblDetails.Content)
        Try
            With rsPreviousPregnancy
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPPregnancy WHERE PPNO=" & PPNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
                .Open("SELECT * FROM tblPPregnancy ORDER BY PPNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .Move(lngCRec)
                    GetPPData()
                Else
                    ClearPPData()
                End If
            End With
        Catch
            MsgBox("An error has occured while archiving a record " & Err.Description)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            If CEdit = True Then
                rsPreviousPregnancy.Close()
                CEdit = False
                rsPreviousPregnancy = New ADODB.Recordset()
                rsPreviousPregnancy.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsPreviousPregnancy.Open("SELECT *FROM tblPPregnancy ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsPreviousPregnancy.Move(lngCRec)
            Else
                With rsPreviousPregnancy
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetPPData()
                    Else
                        MsgBox("Nothing to Cancel")
                        Me.txtPregnancyNumber.Focus()
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
            lngCRec = rsPreviousPregnancy.AbsolutePosition
            GetPatSNo(lblDetails.Content)
            With rsPreviousPregnancy
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub
                Else
                    EditReady()
                    .Close()
                    rsPreviousPregnancy = New ADODB.Recordset()
                    rsPreviousPregnancy.Open("SELECT * FROM tblPPregnancy WHERE PPNO=" & PPNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
            With rsPreviousPregnancy
                If .State = 1 Then

                Else
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblPPregnancy ORDER BY PPNO", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                End If
            End With
        Catch ex As Exception
            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
        End Try


        Try
            With rsPreviousPregnancy
                If .RecordCount <> 0 Then
                    If .BOF = True Or .EOF = True Then Exit Sub
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                GetPPData()
                            End If
                        Else
                            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveFirst()
                        btnPrevious.IsEnabled = False
                        btnNext.IsEnabled = True
                        GetPPData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the first record " & Err.Description)
        End Try

    End Sub


    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click

        Try
            With rsPreviousPregnancy
                If .State = 1 Then

                Else
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblPPregnancy ORDER BY PPNO", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                End If
            End With
        Catch ex As Exception
            MsgBox("Can't Go To last record!", MsgBoxStyle.Exclamation, "Navigation")
        End Try
        Try
            With rsPreviousPregnancy
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnPrevious.IsEnabled = False
                                GetPPData()
                            End If
                        Else
                            MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveLast()
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = False
                        GetPPData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the last record " & Err.Description)
        End Try
    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click

        Try
            With rsPreviousPregnancy
                If .State = 1 Then

                Else
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblPPregnancy ORDER BY PPNO", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                End If
            End With
        Catch ex As Exception
            MsgBox("Can't Go To next record!", MsgBoxStyle.Exclamation, "Navigation")
        End Try

        Try
            With rsPreviousPregnancy
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
                                GetPPData()
                            Else
                                .CancelUpdate()
                                .MoveNext()
                                btnPrevious.IsEnabled = True
                                GetPPData()
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
                        GetPPData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the next record " & Err.Description)
        End Try
    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click

        Try
            With rsPreviousPregnancy
                If .State = 1 Then

                Else
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblPPregnancy ORDER BY PPNO", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                End If
            End With
        Catch ex As Exception
            MsgBox("Can't Go To previous record!", MsgBoxStyle.Exclamation, "Navigation")
        End Try

        Try
            With rsPreviousPregnancy
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
                                GetPPData()
                            Else
                                .CancelUpdate()
                                .MovePrevious()
                                btnNext.IsEnabled = True
                                GetPPData()
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
                        GetPPData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the previous record " & Err.Description)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If CEdit = True Then
            SetPPData()
            rsPreviousPregnancy.Update()

            MsgBox(" Record Saved", MsgBoxStyle.Information, "Save")
            rsPreviousPregnancy.Close()

            CEdit = False

            rsPreviousPregnancy = New ADODB.Recordset()
            rsPreviousPregnancy.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsPreviousPregnancy.Move(lngCRec)
            btnSave.IsEnabled = False
        Else

            If Trim(txtPregnancyNumber.Text) = "" Then
                MsgBox("Enter pregnancy number ", MsgBoxStyle.Information)
                txtPregnancyNumber.Focus()
            ElseIf Trim(txtPregnancyYear.Text) = "" Then
                MsgBox("Please enter pregnancy year", MsgBoxStyle.Information)
                txtPregnancyYear.Focus()
            ElseIf Trim(txtTimesAttendedANC.Text) = "" Then
                MsgBox("Please enter number of times ANC attended", MsgBoxStyle.Information)
                txtTimesAttendedANC.Focus()
            ElseIf Trim(txtPlaceOfDelivery.Text) = "" Then
                MsgBox("Please enter place of delivery", MsgBoxStyle.Information)
                txtPlaceOfDelivery.Focus()
            ElseIf Trim(txtMaturity.Text) = "" Then
                MsgBox("Please enter maturity", MsgBoxStyle.Information)
                txtMaturity.Focus()
            ElseIf Trim(txtLabourDuration.Text) = "" Then
                MsgBox("Please enter labour duration", MsgBoxStyle.Information)
                txtLabourDuration.Focus()
            ElseIf Trim(txtTypeOfDelivery.Text) = "" Then
                MsgBox("Please enter type of delivery", MsgBoxStyle.Information)
                txtTypeOfDelivery.Focus()
            ElseIf Trim(txtBirthWeight.Text) = "" Then
                MsgBox("Please enter the child's birth weight in Kgs", MsgBoxStyle.Information)
                txtBirthWeight.Focus()
            ElseIf (Trim(txtSex.Text)) = "" Then
                MsgBox("enter child's sex", MsgBoxStyle.Information)
                txtSex.Focus()
            ElseIf Trim(txtOutcome.Text) = "" Then
                MsgBox("Please enter the outcome", MsgBoxStyle.Information)
                txtOutcome.Focus()
            ElseIf (Trim(txtPuerperium.Text)) = "" Then
                MsgBox("Enter puerperium", MsgBoxStyle.Information)
                txtPuerperium.Focus()
            Else
                With rsPreviousPregnancy
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblPPregnancy", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    .AddNew()
                    SetPPData()
                    .Update()
                    MsgBox("Record Saved!", MsgBoxStyle.Information)
                    bnNew = False
                    btnSave.IsEnabled = False
                    btnCancel.IsEnabled = False
                    btnEdit.IsEnabled = True
                    txtPregnancyYear.IsEnabled = False
                    txtPuerperium.IsEnabled = False
                    txtLabourDuration.IsEnabled = False
                    txtPregnancyNumber.IsEnabled = False
                    txtPlaceOfDelivery.IsEnabled = False
                    txtTypeOfDelivery.IsEnabled = False
                    txtMaturity.IsEnabled = False
                    txtSex.IsEnabled = False
                    txtTimesAttendedANC.IsEnabled = False
                    txtBirthWeight.IsEnabled = False
                    txtOutcome.IsEnabled = False
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
                .Open("SELECT * FROM tblPatient WHERE PNO=" & lnPNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
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

            PPNO = Val(Right(strNo, Len(iMNo)))

        Catch ex As Exception
            MsgBox("An error has occured while getting patient serial number " & Err.Description)
        End Try

        Return PPNO
    End Function


    Private Function GetPPData()
        Try
            With rsPreviousPregnancy
                lnPNo = .Fields("PNo").Value
                txtPregnancyNumber.Text = .Fields("Gestation").Value
                txtPregnancyYear.Text = .Fields("BPPulse").Value
                Today = .Fields("VisitDate").Value
                txtTimesAttendedANC.Text = .Fields("Parity").Value
                txtPlaceOfDelivery.Text = .Fields("Gravidia").Value
                txtMaturity.Text = .Fields("LMP").Value
                txtLabourDuration.Text = .Fields("FHR").Value
                txtTypeOfDelivery.Text = .Fields("Lie").Value
                txtBirthWeight = .Fields("Position").Value
                txtSex.Text = .Fields("Maturity").Value
                txtOutcome.Text = .Fields("TCA").Value
                txtPuerperium.Text = .Fields("Comment").Value
                lblRecNo.Content = "Record " & .AbsolutePosition & " Of " & .RecordCount & " Records"
            End With
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try

        Return (0)
    End Function

 
    Private Function SetPPData()
        Try
            With rsPreviousPregnancy
                .Fields("PNo").Value = lnPNo
                .Fields("PPNO").Value = PPNO
                .Fields("PregnancyNumber").Value = txtPregnancyNumber.Text
                .Fields("PregnancyYear").Value = txtPregnancyYear.Text
                .Fields("TimesAttendedANC").Value = txtTimesAttendedANC.Text
                .Fields("PlaceOfDelivery").Value = txtPlaceOfDelivery.Text
                .Fields("Maturity").Value = txtMaturity.Text
                .Fields("LabourDuration").Value = txtLabourDuration.Text
                .Fields("TypeOfDelivery").Value = txtTypeOfDelivery.Text
                .Fields("BirthWeight").Value = txtBirthWeight.Text
                .Fields("Sex").Value = txtSex.Text
                .Fields("Outcome").Value = txtOutcome.Text
                .Fields("Puerperium").Value = txtPuerperium.Text
                .Fields("UName").Value = strUser
            End With
        Catch
            MsgBox("An error has occured while setting patient data for saving  " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Function ClearPPData()
        Try
            lblDetails.Content = ""
            txtPregnancyYear.Text = ""
            txtPuerperium.Text = ""
            txtLabourDuration.Text = ""
            txtPregnancyNumber.Text = ""
            txtPlaceOfDelivery.Text = ""
            txtTypeOfDelivery.Text = ""
            txtMaturity.Text = ""
            txtSex.Text = ""
            txtTimesAttendedANC.Text = ""
            txtBirthWeight.Text = ""
            txtOutcome.Text = ""
            lblRecNo.Content = ""

        Catch ex As Exception
            MsgBox("An error has occured while clearing patients' fields " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Sub btnNew_Click(sender As Object, e As RoutedEventArgs) Handles btnNew.Click
        If bnNew = True Then
        Else
            ClearPPData()
            GeneratePPNO()
            lblLSNo.Content = PPNO
            EditReady()
            bnNew = True
            txtPregnancyNumber.Focus()
        End If
    End Sub
End Class
