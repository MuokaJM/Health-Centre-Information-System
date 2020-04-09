Imports ADODB
Imports System.Text.RegularExpressions

Class pgANCPatientDetails

    Private lngCRec As Long 'current record
    Private CEdit As Boolean
    Private iAns As Integer
    Private MainWin As New MainWindow
    Private rsANC As New ADODB.Recordset()
    Private rsPatient As New ADODB.Recordset
    Private rsQueue As New ADODB.Recordset()
    Private lnPNo As Integer
    Private bnClearQueue As Boolean
    Private bnNew As Boolean = False 'check if new record procedure has been called
    Private lnQNO As Integer
    Public sUname As String
    Public strUser As String
    Private ANCNo As Long
    Private strPatNo As String
    Private strPatientName As String
    Private dtDoB As Date


    Private Sub pgANC_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        lblToday.Content = Format(Today, "dd-MMMM-yy")
        lblAge.Visibility = Windows.Visibility.Hidden
        btnCancel.IsEnabled = False
        btnSave.IsEnabled = False
        txtDiabetic.IsEnabled = False
        txtSurgicalOperation.IsEnabled = False
        txtTuberculosis.IsEnabled = False
        txtTwins.IsEnabled = False
        txtAllergy.IsEnabled = False
        txtFamilyDiabetes.IsEnabled = False
        txtHypertensive.IsEnabled = False
        txtFamilyTuberculosis.IsEnabled = False
        txtFamilyHypertension.IsEnabled = False


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
        EditReady()
        getPatientNumber()

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
                        .Open("SELECT * FROM tblPatient WHERE PNo=" & CInt(rsQueue.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            GetANCData()
                            lnPNo = .Fields("pno").Value
                            dtDoB = .Fields("DoB").Value
                            strPatNo = .Fields("PatNo").Value
                            GetAge(dtDoB)
                            lblDetails.Content = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value) & " " & .Fields("Sex").Value
                            strPatientName = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value)
                            lblDetails.Content = lnPNo & ": " & lblDetails.Content & " Age: " & lblAge.Content

                            txtSurgicalOperation.Focus()

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
                lblAge.Content = intAge & " Years"
            Else
                intAge = DateDiff(DateInterval.Month, DoB, Today())

                If intAge > 1 Then
                    lblAge.Content = intAge & " Months"
                Else
                    intAge = DateDiff(DateInterval.Day, DoB, Today())
                    lblAge.Content = intAge & " Days"
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
        ClearANCData()
        txtDiabetic.IsEnabled = True
        txtSurgicalOperation.IsEnabled = True
        txtTuberculosis.IsEnabled = True
        txtTwins.IsEnabled = True
        txtAllergy.IsEnabled = True
        txtFamilyDiabetes.IsEnabled = True
        txtHypertensive.IsEnabled = True
        txtFamilyTuberculosis.IsEnabled = True
        txtFamilyHypertension.IsEnabled = True
        txtBloodTransfusion.IsEnabled = True
        btnSave.IsEnabled = True
        btnCancel.IsEnabled = True
        btnEdit.IsEnabled = True
        btnFind.IsEnabled = False
        txtSurgicalOperation.Focus()

    End Sub


    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            If CEdit = True Then
                rsPatient.Close()
                CEdit = False
                rsPatient = New ADODB.Recordset()
                rsPatient.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsPatient.Open("SELECT * FROM tblPatient  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsPatient.Move(lngCRec)
            Else
                With rsPatient
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetANCData()
                    Else
                        MsgBox("Nothing to Cancel")
                        Me.txtSurgicalOperation.Focus()
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
            lngCRec = rsPatient.AbsolutePosition
            GetPatSNo(lblDetails.Content)
            With rsPatient
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub
                Else
                    EditReady()
                    .Close()
                    rsPatient = New ADODB.Recordset()
                    rsPatient.Open("SELECT * FROM tblPatient WHERE PNo=" & lnPNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
            With rsPatient
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
            With rsPatient
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
            With rsPatient
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
            With rsPatient
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
            rsPatient.Update()

            MsgBox(" Record Saved", MsgBoxStyle.Information, "Save")
            rsPatient.Close()

            CEdit = False

            rsPatient = New ADODB.Recordset()
            rsPatient.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsPatient.Open("SELECT * FROM tblPatient WHERE STATUS<>'ARCHIVED' ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            rsPatient.Move(lngCRec)
            btnSave.IsEnabled = False
        Else
            'check that all fields ave been filled

            If Trim(txtSurgicalOperation.Text) = "" Then
                MsgBox("Enter any surgical operation or no if no any done ", MsgBoxStyle.Information)
                txtSurgicalOperation.Focus()
            ElseIf Trim(txtDiabetic.Text) = "" Then
                MsgBox("State diabetes status of the patient (Yes/No)", MsgBoxStyle.Information)
                txtDiabetic.Focus()
            ElseIf Trim(txtHypertensive.Text) = "" Then
                MsgBox("State hypertension status of the patient (Yes/No)", MsgBoxStyle.Information)
                txtHypertensive.Focus()
            ElseIf Trim(txtTuberculosis.Text) = "" Then
                MsgBox("State tuberculosis status of the patient (Yes/No)", MsgBoxStyle.Information)
                txtTuberculosis.Focus()

            ElseIf Trim(txtTwins.Text) = "" Then
                MsgBox("Enter family's twins history", MsgBoxStyle.Information)
                txtTwins.Focus()
            ElseIf Trim(txtFamilyTuberculosis.Text) = "" Then
                MsgBox("Enter family's tuberculosis history", MsgBoxStyle.Information)
                txtFamilyTuberculosis.Focus()
            ElseIf (Trim(txtFamilyDiabetes.Text)) = "" Then
                MsgBox("Enter family's diabetes history", MsgBoxStyle.Information)
                txtFamilyDiabetes.Focus()
            ElseIf Trim(txtFamilyHypertension.Text) = "" Then
                MsgBox("Enter family's hypertension history", MsgBoxStyle.Information)
                txtFamilyHypertension.Focus()

            Else
                With rsPatient
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblPatient WHERE PNO=" & lnPNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    SetANCData()
                    .Update()
                    MsgBox("Record Saved!", MsgBoxStyle.Information)

                    btnSave.IsEnabled = False
                    btnCancel.IsEnabled = False
                    btnEdit.IsEnabled = True
                    txtDiabetic.IsEnabled = False
                    txtBloodTransfusion.IsEnabled = False
                    txtSurgicalOperation.IsEnabled = False
                    txtTuberculosis.IsEnabled = False
                    txtTwins.IsEnabled = False
                    txtAllergy.IsEnabled = False
                    txtFamilyDiabetes.IsEnabled = False
                    txtHypertensive.IsEnabled = False
                    txtFamilyTuberculosis.IsEnabled = False
                    txtFamilyHypertension.IsEnabled = False
                End With
            End If
        End If

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
            With rsPatient
                lnPNo = .Fields("PNo").Value
                If IsDBNull(.Fields("SurgicalOperation").Value) = False Then
                    txtSurgicalOperation.Text = .Fields("SurgicalOperation").Value
                End If

                If IsDBNull(.Fields("Diabetic").Value) = False Then
                    txtDiabetic.Text = .Fields("Diabetic").Value
                End If

                If IsDBNull(.Fields("Hypertensive").Value) = False Then
                    txtHypertensive.Text = .Fields("Hypertensive").Value
                End If

                If IsDBNull(.Fields("Tuberculosis").Value) = False Then
                    txtTuberculosis.Text = .Fields("Tuberculosis").Value
                End If

                If IsDBNull(.Fields("allergies").Value) = False Then
                    txtAllergy.Text = .Fields("allergies").Value
                End If

                If IsDBNull(.Fields("BloodTransfusion").Value) = False Then
                    txtBloodTransfusion.Text = .Fields("BloodTransfusion").Value
                End If

                If IsDBNull(.Fields("TwinsInFamily").Value) = False Then
                    txtTwins.Text = .Fields("TwinsInFamily").Value
                End If

                If IsDBNull(.Fields("TuberculosisInFamily").Value) = False Then
                    txtFamilyTuberculosis.Text = .Fields("TuberculosisInFamily").Value
                End If

                If IsDBNull(.Fields("DiabetesInFamily").Value) = False Then
                    txtFamilyDiabetes.Text = .Fields("DiabetesInFamily").Value
                End If
                If IsDBNull(.Fields("HypertensionInFamily").Value) = False Then
                    txtFamilyHypertension.Text = .Fields("HypertensionInFamily").Value
                End If
                lblRecNo.Content = "Record " & .AbsolutePosition & " Of " & .RecordCount & " Records"
            End With
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try

        Return (0)
    End Function

  

    Private Function SetANCData()
        Try
            With rsPatient
                .Fields("SurgicalOperation").Value = txtSurgicalOperation.Text
                .Fields("Diabetic").Value = txtDiabetic.Text
                .Fields("Hypertensive").Value = txtHypertensive.Text
                .Fields("Tuberculosis").Value = txtTuberculosis.Text
                .Fields("Allergies").Value = txtAllergy.Text
                .Fields("BloodTransfusion").Value = txtBloodTransfusion.Text
                .Fields("TwinsInFamily").Value = txtTwins.Text
                .Fields("TuberculosisInFamily").Value = txtFamilyTuberculosis.Text
                .Fields("DiabetesInFamily").Value = txtFamilyDiabetes.Text
                .Fields("HypertensionInFamily").Value = txtFamilyHypertension.Text
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
            txtDiabetic.Text = ""
            txtBloodTransfusion.Text = ""
            txtSurgicalOperation.Text = ""
            txtTuberculosis.Text = ""
            txtTwins.Text = ""
            txtAllergy.Text = ""
            txtFamilyDiabetes.Text = ""
            txtHypertensive.Text = ""
            txtFamilyTuberculosis.Text = ""
            txtFamilyHypertension.Text = ""
            lblRecNo.Content = ""
            lblAge.Content = ""
        Catch ex As Exception
            MsgBox("An error has occured while clearing patients' fields " & Err.Description)
        End Try
        Return (0)
    End Function



End Class


