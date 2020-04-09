Imports ADODB
Imports System.Text.RegularExpressions
Class pgPatient

    Private lngCRec As Long '
    Private CEdit As Boolean
    Private iAns As Integer
    Private bnfrmP As Boolean
    Private MainWin As New MainWindow
    Public rsPatient As New ADODB.Recordset()
    Private rsQueue As New ADODB.Recordset()
    Private BNO As Integer
    Private bDate As DateTime
    Private BDetNo As Integer
    Private BENo As Integer
    Private curRegAmt As Long
    Public sUname As String
    Public strUser As String
    Private PNo As Long
    Private strPatNo As String
    Private strPatientPrefix As String
    Public dgBrush As New SolidColorBrush

    Private planSno As Integer
    Private TNo As Integer
    Private rsLabTests As New ADODB.Recordset
    Private rsDrugs As New ADODB.Recordset
    Private strLabRequest As String
    Private strPharmRequest As String
    Private dbLabCost As Decimal
    Private totalCost As Decimal
    Private dbPharmCost As Decimal
    Private dbANCCost As Decimal
    Private dbCWCCost As Decimal
    Private dbFPCost As Decimal
    Private arrLabDet As New ArrayList
    Private arrPharmDet As New ArrayList
    Private bnNew As Boolean '
    Private iDQty As Integer '
    Private intPatientAge As Integer
    Private strSentTo As String
    Private strPi As String

    Private Sub pgPatient_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        Try
            lblToday.Content = Format(Today, "dd-MMMM-yy")
            btnCancel.IsEnabled = False
            btnSave.IsEnabled = False
            txtSName.IsEnabled = False
            txtONames.IsEnabled = False
            txtAge.IsEnabled = False
            txtSLocation.IsEnabled = False
            txtAddress.IsEnabled = False
            txtPhone.IsEnabled = False
            dtpDoB.IsEnabled = False
            cboSentTo.Items.Add("CO")
            cboSentTo.Items.Add("Lab")
            cboSentTo.Items.Add("Pharmacy")
            cboSentTo.Items.Add("ANC")
            cboSentTo.Items.Add("CWC")
            cboSentTo.Items.Add("FP")
            cboSentTo.Items.Add("Nurse")
            cboSentTo.IsEnabled = False

            cboNew.Items.Add("OP")
            cboNew.Items.Add("IP")
            cboNew.Items.Add("RF")

            stpDetails.Visibility = Windows.Visibility.Collapsed
        Catch ex As Exception
            MsgBox("An error has occured during form load " & Err.Description)
        End Try

        Try
            With rsPatient
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPatient  WHERE STATUS<>'ARCHIVED' ORDER BY PNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            End With
        Catch
            MsgBox("An error has occured during form load " & Err.Description)
        End Try



    End Sub


    Private Sub newRecord()
        Dim rsPatientNumber As New ADODB.Recordset
        Try
            If Trim(strPatientPrefix) = "" Then
                MsgBox("Please select the category of the patient", MsgBoxStyle.Information)
                cboNew.Focus()
                Exit Sub
            Else
                generatePatientNo()
                With rsPatientNumber
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblPatient ORDER BY PNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .BOF And .EOF Then
                        PNo = 0
                    Else
                        If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                        .MoveLast()
                        PNo = .Fields("PNo").Value
                    End If
                    PNo = PNo + 1
                    editReady()
                    .Close()
                End With
            End If
        Catch
            MsgBox("An error has occured while generating new record " & Err.Description)
        End Try

    End Sub

    Private Sub editReady()
        btnSave.IsEnabled = True
        btnCancel.IsEnabled = True
        bnNew = True
        strSentTo = ""
        txtSName.IsEnabled = True
        txtRemarks.IsEnabled = True
        txtONames.IsEnabled = True
        txtAge.IsEnabled = True
        txtSLocation.IsEnabled = True
        txtAddress.IsEnabled = True
        txtPhone.IsEnabled = True
        dtpDoB.IsEnabled = True
        btnArchive.IsEnabled = False
        cboSentTo.IsEnabled = True
        ClearPatientData()
        lblToday.Content = Today
        lblPNo.Content = strPatNo & " (SNo-" & PNo & ")"
    End Sub


    Private Sub generatePatientNo()
        Dim rsPatientNumber As New ADODB.Recordset

        Try
            With rsPatientNumber
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPatient  WHERE PatNo LIKE '" & strPatNo & "%' AND PatNo LIKE '%" & Today.Year & "' ORDER BY PNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .BOF And .EOF Then
                    strPatNo = strPatNo & "1-" & Today.Year
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    strPatNo = .Fields("PatNo").Value
                    getPatNo(strPatNo)
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating patient number " & Err.Description)
        End Try
    End Sub

    Private Sub getPatNo(strPNo As String)
        Try
            Dim X As Integer
            Dim strCn As String
            Dim N As Integer
            Dim iCode As String = ""
            Dim pCode As String = ""
            strCn = strPNo

            N = 0
            X = 4 '

            For X = 4 To Len(strCn) Step 1
                iCode = Mid(strCn, X, 1)
                If iCode Like "-" = True Then
                    If IsNumeric(pCode) = True Then
                        strPatNo = strPatientPrefix & Val(pCode) + 1 & "-" & Today.Year

                        Exit For
                    Else
                        pCode = ""
                        iCode = ""
                    End If
                End If
                N = N + 1
                pCode = pCode & iCode
            Next X

        Catch ex As Exception
            MsgBox("An error has occured while getting patient number " & Err.Description)
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

            PNo = Val(Right(strNo, Len(iMNo)))

        Catch ex As Exception
            MsgBox("An error has occured while getting patient serial number " & Err.Description)
        End Try

        Return PNo
    End Function

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click

        Try
            If (Trim(strSentTo) = "ANC") Then
                If optMale.IsChecked = False And optFemale.IsChecked = False Then
                    MsgBox("Select the patient's sex")
                    optFemale.Focus()
                    Exit Sub
                ElseIf optMale.IsChecked = True Then
                    MsgBox("Check the patient's sex")
                    optFemale.Focus()
                    Exit Sub
                End If

            ElseIf (Trim(strSentTo) = "FP") Then
                If optMale.IsChecked = False And optFemale.IsChecked = False Then
                    MsgBox("Select the patient's sex")
                    optFemale.Focus()
                    Exit Sub
                End If

            End If
        Catch ex As Exception
            MsgBox("An error has occured in pre save routine", MsgBoxStyle.Exclamation, "Save")
            Exit Sub
        End Try



        Try
            If CEdit = True Then
                SetPatientData()
                rsPatient.Update()

                MsgBox("Patient " & Me.txtONames.Text & " " & Me.txtSName.Text & " Record Saved", MsgBoxStyle.Information, "Save")
                rsPatient.Close()

                CEdit = False
                rsPatient = New ADODB.Recordset()
                rsPatient.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsPatient.Open("SELECT * FROM tblPatient  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsPatient.Move(lngCRec)
                btnSave.IsEnabled = False
            ElseIf bnNew = False Then
                MsgBox("No changes have been made add a new record or edit this record then save again", MsgBoxStyle.Information, "Save")
                bnNew = False
                btnSave.IsEnabled = False
            Else
                If Trim(strSentTo) = "" Then
                    MsgBox("please select the patients destination")
                    cboSentTo.Focus()
                    Exit Sub
                Else
                End If


                If Me.txtSName.Text = "" Then
                    MsgBox("Please enter the Patient's Surname", MsgBoxStyle.Information)
                    txtSName.Focus()
                ElseIf Me.txtONames.Text = "" Then
                    MsgBox("Please enter the Patient's Other Names", MsgBoxStyle.Information)
                    txtONames.Focus()
                ElseIf Me.dtpDoB.SelectedDate = Today Then
                    MsgBox("Please select the Patient's Date of birth", MsgBoxStyle.Information)
                    dtpDoB.Focus()
                ElseIf Me.dtpDoB.SelectedDate > Today Then
                    MsgBox("Please select earlier Patient's Date of birth", MsgBoxStyle.Information)
                    dtpDoB.Focus()
                ElseIf optFemale.IsChecked = False And optMale.IsChecked = False Then
                    MsgBox("Please select Patient's sex", MsgBoxStyle.Information)
                    optMale.Focus()
                ElseIf Me.txtAddress.Text = "" Then
                    MsgBox("Please enter the  Patient's Postal Address", MsgBoxStyle.Information)
                    Me.txtAddress.Focus()

                Else

                    If Trim(strSentTo) = "CO" Then
                        curRegAmt = Val(InputBox("Enter Consultation Amount", , 50))
                    ElseIf Trim(strSentTo) = "Lab" Then
                        curRegAmt = Val(InputBox("Confirm Lab Charges ", , dbLabCost))
                    ElseIf Trim(strSentTo) = "Pharmacy" Then
                        curRegAmt = Val(InputBox("Confirm Drug Charges", , dbPharmCost))
                    ElseIf Trim(strSentTo) = "ANC" Then
                        curRegAmt = Val(InputBox("Confirm ANC Charges", , dbANCCost))
                    ElseIf Trim(strSentTo) = "CWC" Then
                        curRegAmt = Val(InputBox("Confirm CWC Charges", , dbCWCCost))
                    ElseIf Trim(strSentTo) = "FP" Then
                        curRegAmt = Val(InputBox("Confirm FP Charges", , dbFPCost))
                    ElseIf Trim(strSentTo) = "Nurse" Then
                        curRegAmt = Val(InputBox("Enter Consultation Amount", , 50))

                    End If


                    With rsPatient
                        .CancelUpdate()
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblPatient  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        .AddNew()
                        SetPatientData()
                        .Update()
                        sendTo()

                        newPatientRegistration()

                        MsgBox("Record Saved!", MsgBoxStyle.Information)
                        ClearPatientData()
                        cboSentTo.SelectedItem = ""

                        strSentTo = ""
                        lblPNo.Content = ""
                        btnSave.IsEnabled = True
                        txtRemarks.IsEnabled = False
                        txtSName.IsEnabled = False
                        txtONames.IsEnabled = False
                        txtAge.IsEnabled = False
                        txtSLocation.IsEnabled = False
                        txtAddress.IsEnabled = False
                        txtPhone.IsEnabled = False
                        btnArchive.IsEnabled = True
                    End With
                End If
            End If
        Catch
            MsgBox("An error has occured while saving new record " & Err.Description)
        End Try
    End Sub



    Private Sub btnFind_Click(sender As Object, e As RoutedEventArgs) Handles btnFind.Click
        Try
            Dim nwWin As New Window1
            Dim fiS As New Frame
            Dim ti As New TabItem
            Dim pgPatSearch As New pgPatSearch

            pgPatSearch.dgBrush.Color = dgBrush.Color
            pgPatSearch.strUser = strUser
            fiS.NavigationService.Navigate(pgPatSearch)
            ti.Content = fiS
            nwWin.tcSearch.Items.Add(ti)
            nwWin.Show()
        Catch ex As Exception
            MsgBox("An error has occured while loading search window " & Err.Description)
        End Try


    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        Try
            lngCRec = rsPatient.AbsolutePosition
            GetPatSNo(lblPNo.Content)
            With rsPatient
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub
                Else
                    txtSName.IsEnabled = True
                    txtRemarks.IsEnabled = True
                    txtONames.IsEnabled = True
                    txtAge.IsEnabled = True
                    txtSLocation.IsEnabled = True
                    txtAddress.IsEnabled = True
                    txtPhone.IsEnabled = True
                    btnArchive.IsEnabled = False

                    .Close()
                    rsPatient = New ADODB.Recordset()
                    rsPatient.Open("SELECT * FROM tblPatient WHERE PNo=" & PNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    CEdit = True
                    btnSave.IsEnabled = True
                    btnCancel.IsEnabled = True
                End If
            End With
        Catch
            MsgBox("An error has occured while preparing to edit record " & Err.Description)
        End Try



    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click

        Try
            If CEdit = True Then
                rsPatient.Close()
                CEdit = False
                rsPatient = New ADODB.Recordset()
                rsPatient.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsPatient.Open("SELECT *FROM tblPatient  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsPatient.Move(lngCRec)
            Else
                With rsPatient
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetPatientData()
                        bnNew = False
                        editReady()
                    Else
                        btnSave.IsEnabled = True
                        bnNew = False
                        btnCancel.IsEnabled = False
                        strSentTo = ""
                        editReady()
                        cboNew.Focus()
                    End If
                End With
            End If
        Catch
            MsgBox("An error has occured while canceling record " & Err.Description)
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
                                bnNew = False
                                GetPatientData()
                            End If
                        Else
                            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveFirst()
                        btnPrevious.IsEnabled = False
                        btnNext.IsEnabled = True
                        GetPatientData()

                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the first record " & Err.Description)
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
                                bnNew = False
                                GetPatientData()
                            Else
                                .CancelUpdate()
                                .MovePrevious()
                                btnNext.IsEnabled = True
                                bnNew = False
                                GetPatientData()
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
                        GetPatientData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the previous record " & Err.Description)
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
                                bnNew = False
                                GetPatientData()
                            Else
                                .CancelUpdate()
                                .MoveNext()
                                btnPrevious.IsEnabled = True
                                bnNew = False
                                GetPatientData()
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
                        GetPatientData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the next record " & Err.Description)
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
                                bnNew = False
                                GetPatientData()
                            End If
                        Else
                            MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveLast()
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = False
                        GetPatientData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the last record " & Err.Description)
        End Try
    End Sub

    Private Function SetPatientData()
        Try
            With rsPatient
                .Fields("PNo").Value = PNo
                .Fields("Surname").Value = txtSName.Text
                .Fields("dob").Value = dtpDoB.SelectedDate
                .Fields("VDate").Value = Today
                .Fields("Onames").Value = txtONames.Text
                .Fields("surname").Value = txtSName.Text
                If optMale.IsChecked = True Then
                    .Fields("sex").Value = "Male"
                ElseIf optFemale.IsChecked = True Then
                    .Fields("sex").Value = "Female"
                Else
                    .Fields("sex").Value = "_"
                End If
                .Fields("PatNo").Value = strPatNo
                .Fields("Address").Value = txtAddress.Text
                .Fields("phone").Value = txtPhone.Text
                .Fields("SubLoc").Value = txtSLocation.Text
                .Fields("Org").Value = txtRemarks.Text
                .Fields("UName").Value = strUser
                .Fields("status").Value = "ON"
            End With
        Catch
            MsgBox("An error has occured while setting patient data for saving  " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Function ClearPatientData()
        Try
            lblPNo.Content = ""
            txtSName.Text = ""
            dtpDoB.SelectedDate = Today
            txtAge.Text = ""
            optFemale.IsChecked = False
            optMale.IsChecked = False
            txtONames.Text = ""
            txtSName.Text = ""
            txtAddress.Text = ""
            txtPhone.Text = ""
            txtSLocation.Text = ""
            txtRemarks.Text = ""
            lblRecNo.Content = ""
            lblAge.Content = ""
            cboNew.Text = ""
            cboSentTo.Text = ""
        Catch ex As Exception
            MsgBox("An error has occured while clearing patients' fields " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Function GetPatientData()
        Try
            Dim strPT As String = ""
            With Me
                .lblPNo.Content = .rsPatient.Fields("patno").Value & " SNO-" & .rsPatient.Fields("Pno").Value
                .txtSName.Text = .rsPatient.Fields("Surname").Value
                .dtpDoB.SelectedDate = CDate(.rsPatient.Fields("dob").Value)
                .lblToday.Content = .rsPatient.Fields("VDate").Value
                .txtONames.Text = .rsPatient.Fields("Onames").Value
                If Trim(.rsPatient.Fields("sex").Value) = "Male" = True Then
                    .optMale.IsChecked = True
                ElseIf Trim(.rsPatient.Fields("Sex").Value) = "Female" = True Then
                    .optFemale.IsChecked = True
                Else
                    optFemale.IsChecked = False
                    optMale.IsChecked = False
                End If

                .txtAddress.Text = .rsPatient.Fields("Address").Value
                .txtPhone.Text = .rsPatient.Fields("phone").Value
                .txtSLocation.Text = .rsPatient.Fields("SubLoc").Value
                .lblRecNo.Content = "Record " & .rsPatient.AbsolutePosition & " Of " & .rsPatient.RecordCount & " Records"
            End With
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try

        GetAge()
        Return (0)
    End Function


    Private Sub txtSName_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtSName.LostFocus
        Try
            Dim pattern As String = "^[a-zA-Z'-]*$"
            txtSName.Text = Trim(txtSName.Text)
            Dim SurnameMatch As Match = Regex.Match(txtSName.Text, pattern)

            If SurnameMatch.Success = True Then
                txtONames.Focus()
            Else
                MsgBox("Invalid Surname, name cannot contain numerals or special symbols", MsgBoxStyle.Exclamation)

                txtSName.Text = ""

            End If
        Catch ex As Exception
            MsgBox("An error has occured while validating surname " & Err.Description)
        End Try

    End Sub


    Private Sub dtpDoB_LostFocus(sender As Object, e As RoutedEventArgs) Handles dtpDoB.LostFocus
        GetAge()
    End Sub

    Private Sub GetAge()
        Try
            Dim intAge As Integer
            Dim dtDtp As Date
            dtDtp = dtpDoB.SelectedDate
            intAge = DateDiff(DateInterval.Year, dtDtp, Today())
            If intAge > 1 Then
                lblAge.Content = intAge & " Years"
                intPatientAge = intAge
                txtAge.Text = intAge
            Else
                intPatientAge = 0
                intAge = DateDiff(DateInterval.Month, dtDtp, Today())

                If intAge > 1 Then
                    lblAge.Content = intAge & " Months"
                    txtAge.Text = intAge & "m"
                Else
                    intAge = DateDiff(DateInterval.Day, dtDtp, Today())
                    lblAge.Content = intAge & " Days"
                    txtAge.Text = intAge & "d"
                End If
            End If
        Catch
            MsgBox("An error has occured while getting patient's age " & Err.Description)
        End Try
    End Sub


    Private Sub newPatientRegistration()
        Dim rsBill As New ADODB.Recordset
        Dim rsBillDet As New ADODB.Recordset

        GenerateBillNo()
        Try
            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill ORDER BY BNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                .AddNew()
                .Fields("BNo").Value = BNO
                .Fields("PNo").Value = PNo
                .Fields("BDate").Value = Today
                .Fields("uName").Value = strUser
                .Fields("BAmt").Value = curRegAmt
                .Fields("Bal").Value = curRegAmt
                .Fields("TAmt").Value = curRegAmt
                .Fields("PBNo").Value = 0
                .Fields("PBal").Value = 0
                .Update()
                .Close()
            End With
        Catch
            MsgBox("An error has occured while adding new bill " & Err.Description)
        End Try

        Try
            With rsBill
                If .State = 1 Then .Close()
                bDate = (Format(Now, "yyyy-MM-dd"))
                PNo = GetPatSNo(lblPNo.Content)

                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE PNo=" & PNo & " AND BDate='" & Format(Now, "yyyy-MM-dd").ToString & "' ORDER BY BDate Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .BOF = True And .EOF = True Then
                    BENo = 1
                Else
                    With rsBillDet
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM    tblBillDetails WHERE BNo=" & BNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            .MoveLast()
                            BENo = .Fields("BiNo").Value + 1
                        ElseIf .BOF And .EOF = True Then
                            BENo = 1
                        End If
                    End With
                End If
                .Close()
            End With
        Catch
            MsgBox("An error has occured while generating bill details number " & Err.Description)
        End Try

        Try
            With rsBillDet
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBillDetails ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                .AddNew()

                GenerateBillDetNo()

                .Fields("SNo").Value = BDetNo
                .Fields("PNo").Value = PNo
                .Fields("BNo").Value = BNO
                .Fields("BiNo").Value = BENo
                .Fields("SAmt").Value = curRegAmt

                If Trim(strSentTo) = "CO" Then
                    .Fields("Service").Value = "Consultation"
                    .Fields("RefNo").Value = "Patient Number " & lblPNo.Content & " Consultation"
                ElseIf Trim(strSentTo) = "Lab" Then
                    .Fields("Service").Value = "Lab Charges"
                    .Fields("RefNo").Value = "Patient Number " & lblPNo.Content & " Lab Charges"
                ElseIf Trim(strSentTo) = "Pharmacy" Then
                    .Fields("Service").Value = "Drug(s) Cost"
                    .Fields("RefNo").Value = "Patient Number " & lblPNo.Content & " Drug(s) Cost"
                ElseIf Trim(strSentTo) = "ANC" Then
                    .Fields("Service").Value = "ANC Clinic Cost"
                    .Fields("RefNo").Value = "Patient Number " & lblPNo.Content & " ANC Clinic Cost"

                ElseIf Trim(strSentTo) = "CWC" Then
                    .Fields("Service").Value = "CWC Cost"
                    .Fields("RefNo").Value = "Patient Number " & lblPNo.Content & " CWC Cost"
                ElseIf Trim(strSentTo) = "FP" Then
                    .Fields("Service").Value = "FP Cost"
                    .Fields("RefNo").Value = "Patient Number " & lblPNo.Content & " FP Cost"

                ElseIf Trim(strSentTo) = "Nurse" Then
                    .Fields("Service").Value = "Consultation"
                    .Fields("RefNo").Value = "Patient Number " & lblPNo.Content & " Consultation"
                End If

                .Update()
                .Close()
            End With
        Catch
            MsgBox("An error has occured while adding new bill details " & Err.Description)
        End Try

    End Sub

    Private Sub GenerateBillNo()
        Try
            Dim rsBill As New ADODB.Recordset
            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill ORDER BY BNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .BOF = True And .EOF = True Then
                    BNO = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    BNO = .Fields("BNo").Value
                End If
                BNO = BNO + 1
                .Close()
            End With
        Catch
            MsgBox("An error has occured while generating bill number " & Err.Description)
        End Try
    End Sub

    Private Sub GenerateBillDetNo()
        Try
            Dim rsBDet As New ADODB.Recordset
            With rsBDet
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBillDetails ORDER BY SNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .BOF = True And .EOF = True Then
                    BDetNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    BDetNo = .Fields("sNo").Value
                End If
                BDetNo = BDetNo + 1
                .Close()
            End With
        Catch
            MsgBox("An error has occured while generating bill details number " & Err.Description)
        End Try
    End Sub



    Private Sub billReturn()
        Dim rsBill As New ADODB.Recordset
        Dim rsBillDet As New ADODB.Recordset
        Dim strPNo As String = InputBox("Enter the patient number", , GetPatSNo(lblPNo.Content))
        Dim curRAmt As Double = Val(InputBox("Enter revisit amount", , 20))

        Dim dbBamt As Double '
        Dim dbBal As Double '
        Dim dbPBal As Double '
        Dim intPBNo As Integer '
        Dim dbTAmt As Double '
        Dim BiNo As Integer '

        Dim TrDate As DateTime = DateTime.Today ' 
        PNo = GetPatSNo(lblPNo.Content)

        Try
            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE PNO=" & PNo & " AND BDate='" & Format(Now, "yyyy-MM-dd").ToString & "' AND BAmt=Bal ORDER BY BDate DESC, BNO Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then '
                    dbBamt = .Fields("BAmt").Value
                    dbBal = .Fields("Bal").Value
                    dbPBal = .Fields("PBal").Value
                    dbTAmt = .Fields("TAmt").Value
                    intPBNo = .Fields("PBNo").Value
                    BNO = .Fields("BNo").Value
                    .Fields("uName").Value = strUser
                    .Fields("BAmt").Value = dbBamt + Val(curRAmt)
                    .Fields("TAmt").Value = dbTAmt + Val(curRAmt)
                    .Fields("Bal").Value = dbBal + Val(curRAmt)
                    .Update()
                    .Close()
                    GenerateBillDetNo()
                    Try
                        With rsBillDet
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblBillDetails WHERE BNo=" & BNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .RecordCount > 0 Then
                                .MoveLast()
                                BiNo = .Fields("BiNo").Value + 1
                            ElseIf .BOF And .EOF = True Then
                                BiNo = 1
                            End If
                            .AddNew()
                            .Fields("SNo").Value = BDetNo
                            .Fields("PNo").Value = Val(strPNo)
                            .Fields("BNo").Value = BNO
                            .Fields("BiNo").Value = BiNo
                            .Fields("SAmt").Value = Val(curRAmt)
                            .Fields("Service").Value = "Revist"
                            .Fields("RefNo").Value = "Patient Number " & strPNo & " Revisit"
                            .Update()
                            .Close()
                        End With
                    Catch
                        MsgBox("An error has occured saving bill details " & Err.Description)
                    End Try
                Else
                    Try
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblBill WHERE PNO=" & PNo & " ORDER BY BNO DESC, BDate DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then
                            GenerateBillNo()
                            dbBamt = Val(.Fields("BAmt").Value)
                            dbTAmt = Val(.Fields("TAmt").Value)
                            If IsDBNull(.Fields("Bal").Value) = True Then
                                dbPBal = 0
                            Else
                                dbPBal = Val(.Fields("Bal").Value)
                                .Fields("Bal").Value = 0
                                .Fields("Remarks").Value = "Balance Carried Forward to Bill No. " & BNO
                                .Update()
                            End If
                            intPBNo = CInt(.Fields("BNo").Value)

                            .AddNew()
                            .Fields("PNo").Value = PNo
                            .Fields("BNo").Value = BNO
                            .Fields("BDate").Value = Today
                            .Fields("BAmt").Value = Val(curRAmt)
                            .Fields("PBNO").Value = intPBNo
                            .Fields("PBal").Value = dbPBal
                            .Fields("TAmt").Value = Val(curRAmt) + dbPBal
                            .Fields("Bal").Value = Val(curRAmt) + dbPBal
                            .Fields("UName").Value = strUser
                            .Update()
                            .Close()

                            GenerateBillDetNo()
                            With rsBillDet
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblBillDetails WHERE BNo=" & BNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .RecordCount > 0 Then
                                    .MoveLast()
                                    BiNo = .Fields("BiNo").Value + 1
                                ElseIf .BOF And .EOF = True Then
                                    BiNo = 1
                                End If
                                .AddNew()
                                .Fields("SNo").Value = BDetNo
                                .Fields("PNo").Value = PNo
                                .Fields("BNo").Value = BNO
                                .Fields("BiNo").Value = BiNo
                                .Fields("SAmt").Value = Val(curRAmt)
                                .Fields("Service").Value = "Revisit"
                                .Fields("RefNo").Value = "Patient Number " & strPNo & " Revisit"
                                .Update()
                                .Close()
                            End With

                        Else
                            GenerateBillNo()
                            .AddNew()
                            .Fields("PNo").Value = PNo
                            .Fields("BNo").Value = BNO
                            .Fields("BDate").Value = Today
                            .Fields("BAmt").Value = Val(curRAmt)
                            .Fields("PBNO").Value = 0
                            .Fields("PBal").Value = 0
                            .Fields("TAmt").Value = curRAmt
                            .Fields("Bal").Value = curRAmt
                            .Fields("UName").Value = strUser
                            .Update()
                            .Close()

                            GenerateBillDetNo()
                            With rsBillDet
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblBillDetails WHERE BNo=" & BNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .RecordCount > 0 Then
                                    .MoveLast()
                                    BiNo = .Fields("BiNo").Value + 1
                                ElseIf .BOF And .EOF = True Then
                                    BiNo = 1
                                End If
                                .AddNew()
                                .Fields("SNo").Value = BDetNo
                                .Fields("PNo").Value = PNo
                                .Fields("BNo").Value = BNO
                                .Fields("BiNo").Value = BiNo
                                .Fields("SAmt").Value = curRAmt
                                .Fields("Service").Value = "Revisit"
                                .Fields("RefNo").Value = "Patient Number " & strPNo & " Revisit"
                                .Update()
                                .Close()
                            End With
                        End If
                    Catch
                        MsgBox("An error has occured while creating new bill " & Err.Description)
                    End Try
                End If
            End With
        Catch
            MsgBox("An error has occured while billing patient " & Err.Description)
        End Try
    End Sub



    Private Sub txtAge_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtAge.LostFocus

        Dim Mchar As String = ""
        Dim cboC As String = ""
        Dim intLength As Integer
        Dim X As Integer
        Dim iMNo As String = ""
        Dim iSNo As String = ""
        Dim intAge As Integer
        Dim strAgeFlag As String = ""

        If Val(txtAge.Text) <= 0 Then Exit Sub
        Try
            cboC = Trim(txtAge.Text)
            intLength = Len(cboC)
            For X = intLength To 0 Step -1
                Mchar = Mid(cboC, X, 1)
                If IsNumeric(Mchar) = True Then Exit For
                iMNo = iMNo + Mchar
            Next X
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

        Try
            If iMNo = "d" Or iMNo = "D" Or iMNo = "m" Or iMNo = "M" Or iMNo = "" Or IsNumeric(Trim(txtAge.Text)) = True Then
                If iMNo = "d" Or iMNo = "D" Then
                    strAgeFlag = "Days"
                    intAge = Left(cboC, (Len(cboC) - Len(iMNo)))
                    iMNo = ""
                    lblAge.Content = intAge & " Days"
                    If Today.Day - intAge < 0 Then
                        MsgBox("Enter this in terms of months", MsgBoxStyle.Information)
                        Exit Sub
                    Else
                        Dim dtCurrent As DateTime = New DateTime(Today.Year, Today.Month, Today.Day - intAge)
                        dtpDoB.SelectedDate = dtCurrent
                    End If
                    txtAge.IsEnabled = False
                ElseIf iMNo = "m" Or iMNo = "M" Then
                    strAgeFlag = "Months"
                    intAge = Left(cboC, (Len(cboC) - Len(iMNo)))
                    lblAge.Content = intAge & " Months"
                    iMNo = ""
                    If Today.Day - intAge < 0 Then
                        MsgBox("Enter this in terms of years", MsgBoxStyle.Information)
                        Exit Sub
                    Else
                        Dim dtCurrent As DateTime = New DateTime(Today.Year, Today.Month - intAge, Today.Day)
                        dtpDoB.SelectedDate = dtCurrent
                    End If
                    txtAge.IsEnabled = False
                Else
                    intAge = Left(cboC, (Len(cboC) - Len(iMNo)))
                    If intAge > 99 Then
                        MsgBox("Confirm if age details are right", MsgBoxStyle.Information)
                    End If
                    Dim dtCurrent As DateTime = New DateTime(Today.Year - intAge, Today.Month, Today.Day)
                    dtpDoB.SelectedDate = dtCurrent
                    lblAge.Content = intAge & " Years"
                    txtAge.IsEnabled = False
                End If
            Else
                MsgBox("Invalid entry, for days add 'd' or 'm' months after the figure, e.g. 2m", MsgBoxStyle.Information)
                txtAge.SelectAll()
            End If
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try


    End Sub



    Private Sub sendTo()
        Dim lnQNo As Long
        Dim rsU As New ADODB.Recordset()

        Try
            If Trim(strSentTo) = "" Then
                MsgBox("Please select the destination of the patient", MsgBoxStyle.Information)
                Exit Sub
            Else
                With rsQueue
                    If Trim(txtRemarks.Text) = "" Then '
                        With rsQueue
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblQueue ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .BOF And .EOF Then
                                lnQNo = 0
                            Else
                                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                                .MoveLast()
                                lnQNo = .Fields("QNo").Value
                            End If
                            .AddNew()
                            lnQNo = lnQNo + 1
                            .Fields("QNO").Value = lnQNo
                            .Fields("QDate").Value = Today
                            .Fields("QTime").Value = Format(Now, "Long Time")
                            .Fields("PatNo").Value = strPatNo
                            .Fields("PName").Value = txtSName.Text & " " & txtONames.Text
                            .Fields("PNo").Value = PNo
                            .Fields("Destination").Value = "Reception"
                            .Fields("Status").Value = "Waiting"
                            rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value
                            rsU.Close()
                            .Fields("Uname").Value = strUser
                            .Fields("Remarks").Value = "To pay: " & curRegAmt
                            .Update()
                            .Close()
                        End With

                    End If


                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblQueue ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .BOF And .EOF Then
                        lnQNo = 0
                    Else
                        If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                        .MoveLast()
                        lnQNo = .Fields("QNo").Value
                    End If
                    .AddNew()
                    lnQNo = lnQNo + 1
                    .Fields("QNO").Value = lnQNo
                    .Fields("QDate").Value = Today
                    .Fields("QTime").Value = Format(Now, "Long Time")
                    .Fields("PatNo").Value = strPatNo
                    .Fields("PName").Value = txtSName.Text & " " & txtONames.Text
                    .Fields("PNo").Value = PNo

                    If Trim(strSentTo) = "CO" Then
                        .Fields("Destination").Value = "Consultation"
                    ElseIf Trim(strSentTo) = "Lab" Then
                        .Fields("Destination").Value = "Lab"
                    ElseIf Trim(strSentTo) = "Pharmacy" Then
                        .Fields("Destination").Value = "Pharmacy"
                    ElseIf Trim(strSentTo) = "Nurse" Then
                        .Fields("Destination").Value = "Nurse"
                    ElseIf Trim(strSentTo) = "ANC" Then
                        .Fields("Destination").Value = "ANC"
                    ElseIf Trim(strSentTo) = "CWC" Then
                        .Fields("Destination").Value = "CWC"
                    ElseIf Trim(strSentTo) = "FP" Then
                        .Fields("Destination").Value = "FP"
                    End If

                    If Trim(txtRemarks.Text) = "" Then
                        .Fields("Status").Value = "Pending"
                    ElseIf Trim(txtRemarks.Text) <> "" Or curRegAmt = 0 Then '
                        .Fields("Status").Value = "Waiting" '
                    End If

                    rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                    .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value
                    rsU.Close()
                    .Fields("Uname").Value = strUser

                    If Trim(strSentTo) = "CO" Then
                        .Fields("Remarks").Value = "First Visit"
                    ElseIf Trim(strSentTo) = "Nurse" Then
                        .Fields("Remarks").Value = "First Visit"
                    ElseIf Trim(strSentTo) = "Pharmacy" Then
                        .Fields("Remarks").Value = "First Visit"
                    ElseIf Trim(strSentTo) = "Lab" Then
                        .Fields("Remarks").Value = txtTests.Text
                    ElseIf Trim(strSentTo) = "ANC" Then
                        .Fields("Remarks").Value = "First Visit"
                    ElseIf Trim(strSentTo) = "CWC" Then
                        .Fields("Remarks").Value = "First Visit"
                    ElseIf Trim(strSentTo) = "FP" Then
                        .Fields("Remarks").Value = "First Visit"
                    End If
                    .Update()
                    MsgBox("Patient scheduled successfully")
                    bnNew = False
                End With
            End If
        Catch
            MsgBox("An error has occured while scheduling a patient " & Err.Description)
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
            strPatNo = p
        Catch ex As Exception
            MsgBox("An error has occured while getting patient number " & Err.Description)
        End Try
        Return (0)
    End Function



    Private Sub btnArchive_Click(sender As Object, e As RoutedEventArgs) Handles btnArchive.Click
        Dim strPNo As String = GetPatSNo(lblPNo.Content)
        Try
            With rsPatient
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPatient WHERE PNo=" & PNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
                .Open("SELECT * FROM tblPatient  WHERE STATUS<>'ARCHIVED' ORDER BY PNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .Move(lngCRec)
                    GetPatientData()
                Else
                    ClearPatientData()
                End If
            End With
        Catch
            MsgBox("An error has occured while archiving a record " & Err.Description)
        End Try
    End Sub



    Private Sub btnClose_Click(sender As Object, e As RoutedEventArgs) Handles btnClose.Click
        stpDetails.Visibility = Windows.Visibility.Collapsed
        lblService.Content = "Service Cost"
    End Sub



    Private Sub lstDetails_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstDetails.SelectionChanged
        Try
            GetLastPlanNo(txtTests.Text)
            getTestNumber(lstDetails.SelectedItem)
            If lblHeader.Content = "Lab Tests" Then
                With rsLabTests
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblLabTests WHERE LTNO=" & TNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        planSno = planSno + 1
                        txtTests.Text = txtTests.Text & planSno & ". " & .Fields("TName").Value & " (@" & .Fields("cost").Value & ") " & vbCrLf
                        strLabRequest = strLabRequest & planSno & ". " & .Fields("TName").Value & " (@" & .Fields("cost").Value & ")" & vbCrLf
                        dbLabCost = dbLabCost + Val(.Fields("cost").Value)
                        lblCost.Content = dbLabCost
                    End If
                    .Close()
                End With
            ElseIf lblHeader.Content = "Drugs" Then
                With rsDrugs
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblDrugs WHERE DNO=" & TNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        planSno = planSno + 1
                        txtTests.Text = txtTests.Text & planSno & ". " & .Fields("DName").Value & " " & vbCrLf  '
                        strPharmRequest = strPharmRequest & planSno & ". " & .Fields("DName").Value & "(" & .Fields("cost").Value & ")"
                        dbPharmCost = dbPharmCost + Val(.Fields("cost").Value)
                        lblCost.Content = dbPharmCost
                    End If
                    .Close()
                End With
            End If
            txtTests.SelectionStart = Len(txtTests.Text)
        Catch ex As Exception
            MsgBox("An error has occured while getting lab test details " & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub GetLastPlanNo(str As String)
        Try
            Dim Mchar As String = ""
            Dim X As Integer
            Dim p As String = ""
            Dim pChar As String = ""
            Dim sTest As String = ""
            Dim rsLabDetails As New ADODB.Recordset
            Dim rsLabTest As New ADODB.Recordset
            totalCost = 0

            If str = "" Then
                planSno = 0
            Else
                X = Len(str)
                For X = Len(str) To X = 0 Step -1
                    Mchar = Mid(str, X, 1)
                    pChar = Mid(str, X - 1, 1)
                    If IsNumeric(pChar) = True And Mchar = "." Then
                        planSno = pChar '+ 1
                        Exit For
                    Else

                    End If

                Next

            End If
            str = "" '
        Catch ex As Exception
            MsgBox("An error has occured while numbering entries " & Err.Description)
        End Try
    End Sub

    Public Function getTestNumber(cboC As String)
        Dim Mchar As String = ""
        Dim X As Integer
        Dim p As String = ""

        Try
            For X = 1 To Len(cboC)
                Mchar = Mid(cboC, X, 1)
                If Mchar = "." Then Exit For
                p = p + Mchar
            Next X
            TNo = Val(p)
        Catch ex As Exception
            MsgBox("An error has occured while getting test number " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Sub LoadLabTests()
        Try
            lstDetails.Items.Clear()
            txtTests.Text = ""
            lblCost.Content = ""
            dbLabCost = 0
            dbPharmCost = 0
            With rsLabTests
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT LTNO, TName, Description, cost FROM tblLabTests ORDER BY LTNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        lstDetails.Items.Add(.Fields("LTNO").Value & ". " & .Fields("TNAME").Value & " " & .Fields("Description").Value & " " & .Fields("Cost").Value)
                        .MoveNext()
                    End While
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading lab tests " & Err.Description)
        End Try

    End Sub

    Private Sub LoadDrugs()
        Try
            lstDetails.Items.Clear()
            txtTests.Text = ""
            lblCost.Content = ""
            dbLabCost = 0
            dbPharmCost = 0
            With rsDrugs
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT DNO, DName, Description, Tradename, quantity FROM tblDrugs ORDER BY DNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        lstDetails.Items.Add(.Fields("DNO").Value & ". " & .Fields("DName").Value & " " & .Fields("Description").Value & " " & .Fields("TradeName").Value & " " & .Fields("Quantity").Value)
                        .MoveNext()
                    End While
                End If
                .close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading drugs " & Err.Description)
        End Try
    End Sub

    Private Sub repeatedGroups()
        Dim X1 As Integer = 1
        Dim S1 As String = ""
        Dim S2 As String = ""
        Dim arrRepeated As New ArrayList
        Dim str As String = ""

        Try
            arrLabDet.Sort()

            While X1 < arrLabDet.Count
                S1 = arrLabDet.Item(X1 - 1)
                S2 = arrLabDet.Item(X1)
                If LCase(S1) = LCase(S2) Then
                    arrRepeated.Add(S2)
                    str = str & "'" & S2 & "' "
                End If
                X1 = 1 + X1
            End While
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

        Try
            If arrRepeated.Count > 0 Then
                If arrRepeated.Count = 1 Then
                    MsgBox("Remove this repeated entry " & (str))
                Else
                    MsgBox("Remove these repeated entries " & (str))
                End If
                btnSave.IsEnabled = False
            Else
                btnSave.IsEnabled = True
            End If
            str = ""
            X1 = 1
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub


    Private Sub txtTests_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtTests.LostFocus
        GetLabTestsCost(txtTests.Text)
        repeatedGroups()
    End Sub

    Private Sub GetLabTestsCost(str As String)
        Try
            Dim trimChars As Char() = {" ", ChrW(13), ChrW(10), ChrW(13), "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ")", "(", ChrW(164), "/", "\", "*", "@", vbCrLf}
            Dim Mchar As String = ""
            Dim X As Integer
            Dim x1 As Integer = 0 '
            Dim strQ As String = ""
            Dim strC As String = ""
            Dim strC2 As String = ""
            Dim iCtr As Integer = 0
            Dim iNum As Integer = 0
            Dim p As String = ""
            Dim pChar As String = ""
            Dim sTest As String = ""
            Dim rsLabDetails As New ADODB.Recordset
            Dim rsLabTest As New ADODB.Recordset
            Dim strDsg As String = ""
            Dim strDrg As String = ""
            Dim iDrg As Integer = 0
            Dim DCount As Integer = 0
            Dim DChar As String = ""
            Dim iSerial As Integer = 1
            totalCost = 0
            arrLabDet.Clear()
            arrPharmDet.Clear()
            strPharmRequest = ""

            If str = "" Then Exit Sub '
            X = Len(str)
            For X = 1 To Len(str)
                Mchar = Mid(str, X, 1)
                If X > 1 Then pChar = Mid(str, X - 1, 1)
                If IsNumeric(pChar) = True And Mchar = "." Or X = Len(str) Then
                    If IsNumeric(sTest) <> True Then
                        If X = Len(str) Then
                            sTest = Trim(sTest) '
                        Else
                            sTest = Trim(Left(sTest, Len(sTest) - 2))
                        End If
                        If lblHeader.Content = "Lab Tests" Then
                            '

                        ElseIf lblHeader.Content = "Drugs" Then
                            strQ = ""
                            For x1 = 1 To Len(sTest)
                                strC = Mid(sTest, x1, 1)
                                If x1 > 2 Then
                                    strC2 = Mid(sTest, x1 - 1, 1)
                                    If IsNumeric(strC) = True And strC2 = "(" Then
                                        strQ = strQ & strC
                                        iCtr = iCtr + 1
                                    ElseIf IsNumeric(strC) = True And (IsNumeric(Mid(sTest, x1 + 1, 1)) = True Or (Mid(sTest, x1 + 1, 1)) = ")") And (Mid(sTest, x1 - 2, 1)) = "(" Then

                                        strQ = strQ & strC
                                        iCtr = iCtr + 1
                                        DChar = (Mid(sTest, x1 + 1, 1))
                                        DCount = x1 + 1
                                        If (Mid(sTest, x1 + 1, 1)) = ")" Then
                                            Do While (IsNumeric(DChar) = False)
                                                DChar = Mid(sTest, DCount, 1)
                                                DCount = DCount + 1
                                                strDsg = strDsg & DChar
                                            Loop
                                        End If
                                        strDsg = strDsg.Trim(trimChars)
                                    End If
                                End If
                            Next
                            x1 = 1
                            strDrg = iSerial & "."
                            strPi = ""
                            For x1 = 1 To Len(sTest)
                                DChar = Mid(sTest, x1, 1)
                                If (Mid(sTest, x1 + 1, 1)) = "*" Then
                                    If ((Mid(sTest, x1 + 1, 1)) = "*" And (IsNumeric(Mid(sTest, x1, 1))) = True And (IsNumeric(Mid(sTest, x1 + 2, 1))) = True) Then
                                        DChar = Mid(sTest, x1 + 1, 2)
                                        dsg(DChar)
                                        strDrg = strDrg & strPi
                                        x1 = x1 + 2
                                        iSerial = iSerial + 1
                                    End If
                                Else
                                    strDrg = strDrg & DChar
                                End If

                            Next
                            strDrg = strDrg '
                            iDQty = Val(strQ)
                            strPharmRequest = strPharmRequest & strDrg
                        End If

                        sTest = sTest.Trim(trimChars)
                        If lblHeader.Content = "Lab Tests" Then
                            With rsLabTest
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblLabTests WHERE TName='" & sTest & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .RecordCount > 0 Then
                                    totalCost = totalCost + rsLabTest.Fields("Cost").Value
                                    lblCost.Content = totalCost
                                    arrLabDet.Add(sTest)
                                End If
                                .Close()
                            End With
                        ElseIf lblHeader.Content = "Drugs" Then
                            With rsDrugs
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblDrugs WHERE DNAME='" & sTest & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                                If .RecordCount > 0 Then
                                    totalCost = totalCost + (Val(.Fields("Cost").Value) * iDQty)
                                    lblCost.Content = totalCost
                                    arrPharmDet.Add(sTest)
                                    iDQty = 0
                                    strQ = ""
                                    strC = ""
                                    strC2 = ""
                                    iCtr = 0
                                End If
                                .Close()
                            End With
                        End If
                    End If
                    sTest = ""
                Else
                    sTest = sTest & Mchar
                End If
            Next X
        Catch ex As Exception
            MsgBox("An error has occured while getting lab tests cost ")
        End Try

    End Sub

    Private Sub dsg(str As String)

        Try
            If str = "*1" Then
                Me.strPi = "OD"
            ElseIf str = "*2" Then
                Me.strPi = "BD"
            ElseIf str = "*3" Then
                Me.strPi = "TDS"
            ElseIf str = "*4" Then
                Me.strPi = "QID"
            ElseIf str = "*5" Then
            End If
        Catch

        End Try
    End Sub

    Private Sub txtONames_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtONames.LostFocus
        Try
            Dim rsPat As New ADODB.Recordset()
            Dim rsPatOne As New ADODB.Recordset()
            Dim pattern As String = "^[a-zA-Z\-'\s]*$"
            txtONames.Text = Trim(txtONames.Text)
            Dim SurnameMatch As Match = Regex.Match(txtONames.Text, pattern)

            If SurnameMatch.Success = True Then
                With rsPat
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblPatient WHERE surname='" & Trim(txtSName.Text) & "' and Onames Like '%" & Trim(txtONames.Text) & "%'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        MsgBox("Similar patient name exists! " & .Fields("Surname").Value & " " & .Fields("Onames").Value & " " & .Fields("PatNo").Value)
                        txtONames.Text = ""
                        txtSName.Text = ""
                    End If
                    .Close()
                End With

            Else
                MsgBox("Invalid name(s), name cannot contain numerals or special symbols", MsgBoxStyle.Exclamation)

                txtONames.Text = ""

            End If
        Catch ex As Exception
            MsgBox("An error has occured while validating other names " & Err.Description)
        End Try
    End Sub

   
    Private Sub cboSentTo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboSentTo.SelectionChanged
        Dim Mchar As String = ""
        strSentTo = cboSentTo.SelectedItem
        GetAge()
        Try
            If Trim(strSentTo) = "Lab" Then
                LoadLabTests()
                lblHeader.Content = "Lab Tests"
                stpDetails.Visibility = Windows.Visibility.Visible
                lblService.Content = "Lab Cost"
                Mchar = Mid(strPatNo, 1, 2)
                If Mchar <> "RF" Then
                    MsgBox("Only referred patient can be sent straight to Lab." & vbCrLf & "Choose 'R' Option ")
                    btnSave.IsEnabled = False
                    strSentTo = ""
                    Mchar = ""
                    stpDetails.Visibility = Windows.Visibility.Collapsed
                    lblService.Content = "Service Cost"
                Else

                End If

            ElseIf Trim(strSentTo) = "Pharmacy" Then
                LoadDrugs()
                lblHeader.Content = "Drugs"
                stpDetails.Visibility = Windows.Visibility.Visible
                lblService.Content = "Drugs Cost"
                Mchar = Mid(strPatNo, 1, 2)
                If Mchar <> "RF" Then
                    MsgBox("Only referred patient can be sent straight to pharmacy." & vbCrLf & "Cancel this record and choose 'R' Option ")
                    btnSave.IsEnabled = False
                    strSentTo = ""
                    Mchar = ""
                    stpDetails.Visibility = Windows.Visibility.Collapsed
                    lblService.Content = "Service Cost"
                Else

                End If

            ElseIf (Trim(strSentTo) <> "Lab") Or (Trim(strSentTo) <> "Pharmacy") Then
                stpDetails.Visibility = Windows.Visibility.Collapsed
                lblService.Content = "Service Cost"
            End If
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
        Mchar = ""

        If (Trim(strSentTo) = "ANC") Then
            If optMale.IsChecked = False And optFemale.IsChecked = False Then
                MsgBox("Select the patient's sex")
                optFemale.Focus()
            ElseIf optMale.IsChecked = True Then
                MsgBox("Check the patient's sex")
                optFemale.Focus()
            ElseIf intPatientAge < 15 Then
                MsgBox("Confirm the patient's Age")
                txtAge.Focus()
            End If
        ElseIf (Trim(strSentTo) = "CWC") Then
            If intPatientAge > 5 Then
                MsgBox("Confirm the patient's Age")
                txtAge.Focus()
            End If
        ElseIf (Trim(strSentTo) = "FP") Then
            If optMale.IsChecked = False And optFemale.IsChecked = False Then
                MsgBox("Select the patient's sex")
                optFemale.Focus()
            ElseIf optMale.IsChecked = True Then
                MsgBox("Check the patient's sex")
                optFemale.Focus()
            End If
        End If

    End Sub

    Private Sub cboNew_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboNew.SelectionChanged

        Try
            strPatNo = cboNew.SelectedItem & "-"
            strPatientPrefix = cboNew.SelectedItem & "-"
            newRecord()
            txtSName.Focus()
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub
End Class
