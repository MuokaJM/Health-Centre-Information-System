Imports ADODB
Imports System.Windows.Threading
Imports System.Text.RegularExpressions
Imports System.Data
Imports Microsoft.SqlServer
Imports System.Data.OleDb
Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine


Class pgLab
    Private dtTimer As New DispatcherTimer
    Private rsConsultation As New ADODB.Recordset
    Private rsLab As New ADODB.Recordset
    Private rsPatient As New ADODB.Recordset
    Private lnQNO As Long
    Private lnLSNo As Long
    Private lnLDSNo As Long
    Private strPatNo As String
    Private strPName As String
    Private lnPNo As Long
    Private CEdit As Boolean = False
    Private lngCRec As Long
    Private MainWin As New MainWindow
    Private BNO As Long
    Private BDetNo As Long
    Public strUser As String
    Private strLabRequest As String
    Private rsQueue As New ADODB.Recordset
    Private bnClearQueue As Boolean
    Private bnCSNO As Boolean
    Private bnNew As Boolean = False '
    Private rServer As String
    Private rDatabase As String
    Private tNo As Long '
    Private rsLabTests As New ADODB.Recordset
    Private dbLabCost As Decimal
    Private planSno As Integer
    Private arrLabDet As New ArrayList
    Private totalCost As Decimal


    Private Sub pgLab_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        dtTimer.Interval = TimeSpan.FromMilliseconds(100)
        AddHandler dtTimer.Tick, AddressOf Timer_Tick
        dtTimer.Start()
        lblToday.Content = Format(Today, "dd-MMMM-yy")
        btnCancel.IsEnabled = False
        btnEdit.IsEnabled = False
        btnSave.IsEnabled = False
        txtCost.IsEnabled = False
        txtDoneBy.IsEnabled = False
        txtLRequest.IsEnabled = False
        txtLResults.IsEnabled = False

        Try
            LoadScheduledPatients()
        Catch ex As Exception
            MsgBox("An error has occured while loading the latest lab request " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

        LoadLabTests()
        
    End Sub

    Private Sub Timer_Tick()
        lblNow.Content = Format(Now, "Long Time")
    End Sub


    Public Function getConsultationNumber()

        Dim Mchar As String = ""
        Dim cboC As String
        Dim X As Integer
        Dim p As String = ""

        cboC = cboCSNo.SelectedItem
        For X = 1 To Len(cboC)
            Mchar = Mid(cboC, X, 1)
            If Mchar = " " Then Exit For
            p = p + Mchar
        Next X
        lnQNO = Val(p)

        Return (0)
    End Function

    Private Sub cboCSNo_GotFocus(sender As Object, e As RoutedEventArgs) Handles cboCSNo.GotFocus
        Dim nQueue As Integer

        Dim rsQ As New ADODB.Recordset
        With rsQ
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Lab' AND Status='Waiting'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)

            nQueue = .RecordCount
            .Close()
        End With

        If cboCSNo.Items.Count = nQueue Then Exit Sub

        Try
            LoadScheduledPatients()
        Catch ex As Exception
            MsgBox("An error has occured while loading patients details " & Err.Description)
        End Try
    End Sub



    Private Sub cboCSNo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboCSNo.SelectionChanged
        If bnClearQueue = True Then Exit Sub
        ClearLabData()
        getConsultationNumber()
        Dim rsL As New ADODB.Recordset
        If bnNew = True Then
        Else
            GenerateLabServiceNo()
            lblLSNo.Content = lnLSNo
            EditReady()
            bnNew = True
        End If


        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT QNO, QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy, PNO, Remarks FROM tblQueue WHERE QNO= " & lnQNO & " AND destination='Lab' AND Status='Waiting' ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    strPatNo = .Fields("PatNo").Value
                    lblTimeRequestMade.Content = .Fields("Time").Value
                    txtLRequest.Text = .Fields("Remarks").Value
                    getTestsCost(txtLRequest.Text)
                    With rsPatient
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT PNo, Surname, ONames, Sex FROM tblPatient WHERE PNo=" & CInt(rsQueue.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            lnPNo = .Fields("pno").Value
                            lblDetails.Content = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value) & " " & .Fields("Sex").Value
                            strPName = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value)
                            bnNew = True
                        End If
                        .Close()
                    End With

                    With rsConsultation
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblConsultation WHERE QNO=" & lnQNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            txtLRequest.Text = .Fields("LabRequest").Value
                            txtCost.Text = .Fields("LCost").Value
                        End If
                        .Close()
                    End With

                End If
                .Close()
            End With

            If Trim(txtCost.Text) = "" Then
                getTestsCost(txtLRequest.Text)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while fetching patient's details " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub


    Private Function SetLabData()
        Try
            With rsLab
                .Fields("LSNo").Value = lnLSNo
                .Fields("QNO").Value = lnQNO
                .Fields("RTest").Value = txtLRequest.Text
                .Fields("UName").Value = strUser
                .Fields("TimeIn").Value = lblTimeRequestMade.Content
                .Fields("LDate").Value = Today
                .Fields("LResults").Value = txtLResults.Text
                .Fields("Cost").Value = Val(txtCost.Text)
                .Fields("TimeOut").Value = Format(Now, "Long Time")
                .Fields("DoneBy").Value = txtDoneBy.Text
            End With
        Catch ex As Exception
            MsgBox("An error has occured while setting lab data for saving " & Err.Description, MsgBoxStyle.Exclamation)
        End Try


        Return (0)
    End Function

    Private Function ClearLabData()

        lblLSNo.Content = ""
        lblDetails.Content = ""
        lblTimeRequestMade.Content = ""
        txtCost.Text = ""
        txtDoneBy.Text = ""
        txtLRequest.Text = ""
        txtLResults.Text = ""

        Return (0)
    End Function

    Private Function GetLabData()
        Try
            With rsLab
                lblLSNo.Content = .Fields("LSNO").Value
                lblTimeRequestMade.Content = Format(.Fields("TimeIn").Value, "Long Time")
                If IsDBNull(.Fields("TimeOut").Value) = False Then
                    lblTimeOut.Content = Format(.Fields("TimeOut").Value, "Long Time")
                Else
                    lblTimeOut.Content = ""
                End If

                If Val(.Fields("Cost").Value) = 0 Then
                    txtCost.Text = ""
                Else
                    txtCost.Text = .Fields("Cost").Value
                End If
                txtDoneBy.Text = .Fields("DoneBy").Value
                txtLRequest.Text = .Fields("RTest").Value
                txtLResults.Text = .Fields("Lresults").Value
                lblToday.Content = Format(.Fields("LDate").Value, "Short Date")
            End With
        Catch ex As Exception
            MsgBox("An error has occured while getting lab data " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

        Return (0)
    End Function


    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Dim rsU As New ADODB.Recordset
        Dim MChar As String = "" '
        Try
            If CEdit = True Then
                SetLabData()
                rsLab.Update()

                MsgBox("Lab request for " & txtLRequest.Text & " Record Saved", MsgBoxStyle.Information, "Save")
                rsLab.Close()

                CEdit = False

                rsLab = New ADODB.Recordset()
                rsLab.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsLab.Open("SELECT * FROM tblLab", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsLab.Move(lngCRec)
                btnSave.IsEnabled = False
            Else
                If txtCost.Text = "" Or Val(txtCost.Text) = 0 Then
                    MsgBox("Please enter the test cost", MsgBoxStyle.Information, "Save")
                    txtCost.Focus()
                ElseIf txtDoneBy.Text = "" Then
                    MsgBox("Please enter you name", MsgBoxStyle.Information, "Save")
                    txtDoneBy.Focus()
                ElseIf txtLRequest.Text = "" Then
                    MsgBox("Please enter the test that had been requested here", MsgBoxStyle.Information, "Save")
                    txtLRequest.Focus()
                ElseIf txtLResults.Text = "" Then
                    MsgBox("Please enter the findings of the test here", MsgBoxStyle.Information, "Save")

                Else

                    With rsLab
                        SetLabData()
                        .Update()
                        PickRequestedTest(txtLRequest.Text)

                        MChar = Mid(strPatNo, 1, 2)
                        If MChar = "RF" Then
                        Else
                            With rsConsultation
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT QNO, PNO, LabResults FROM tblConsultation WHERE qNO=" & lnQNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .RecordCount > 0 Then
                                    .Fields("LabResults").Value = txtLResults.Text
                                    .Update()
                                End If
                                .Close()
                            End With
                        End If

                        updateQueue()


                        If MChar = "RF" Then
                        Else
                            Try
                                With rsQueue
                                    If .State = 1 Then .Close()
                                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                    .Open("SELECT * FROM tblQueue ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                    If .BOF And .EOF Then
                                        lnQNO = 0
                                    Else
                                        If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                                        .MoveLast()
                                        lnQNO = .Fields("QNo").Value
                                    End If
                                    .AddNew()
                                    lnQNO = lnQNO + 1
                                    .Fields("QNO").Value = lnQNO
                                    .Fields("QDate").Value = Today
                                    .Fields("QTime").Value = Format(Now, "Long Time")
                                    .Fields("PatNo").Value = strPatNo
                                    .Fields("PName").Value = strPName
                                    .Fields("PNo").Value = lnPNo
                                    .Fields("Destination").Value = "Consultation"
                                    .Fields("Status").Value = "Waiting"

                                    rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                                    .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value & " From Lab"
                                    rsU.Close()
                                    .Fields("Uname").Value = strUser
                                    .Fields("Remarks").Value = "Lab test " & (txtLRequest.Text) & " results: " & (txtLResults.Text)
                                    .Update()
                                    .Close()
                                End With
                            Catch ex As Exception
                                MsgBox("An error has occured while adding details to the queue " & Err.Description, MsgBoxStyle.Exclamation)
                            End Try
                        End If

                        MsgBox("Record Saved!", MsgBoxStyle.Information, "Save")
                        bnClearQueue = True
                        LoadScheduledPatients()
                        bnClearQueue = False
                        btnSave.IsEnabled = False
                        bnNew = False '
                        btnEdit.IsEnabled = True
                        cboCSNo.IsEnabled = True
                        txtCost.IsEnabled = False
                        txtDoneBy.IsEnabled = False
                        txtLRequest.IsEnabled = False
                        txtLResults.IsEnabled = False
                    End With
                End If
            End If

        Catch ex As Exception
            MsgBox("An error has occured while saving data " & Err.Description, MsgBoxStyle.Critical)
        End Try

    End Sub

    Private Sub txtDoneBy_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtDoneBy.LostFocus
        Dim rs As New ADODB.Recordset
        Try
            Dim pattern As String = "^[a-zA-Z' ]*$" ' 
            Dim dbMatch As Match = Regex.Match(Trim(txtDoneBy.Text), pattern)
            Dim rsU As New ADODB.Recordset
            If txtDoneBy.Text <> "" Then
                If dbMatch.Success = True Then
                    With rsU
                        .CursorLocation = CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblUser WHERE UName like'%" & Trim(txtDoneBy.Text) & "%' AND DESIGNATION <>'Receptionist' AND DESIGNATION <>'ACCOUNTANT'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then

                        Else
                            With rs
                                .CursorLocation = CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblUser WHERE FName like'%" & Trim(txtDoneBy.Text) & "%' AND DESIGNATION <>'Receptionist' AND DESIGNATION <>'ACCOUNTANT'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                                If .RecordCount > 0 Then

                                Else
                                    MsgBox("That person is not allowed to carry out tests", MsgBoxStyle.Critical)
                                    .Close()
                                    rsU.Close()
                                    Exit Sub
                                End If

                            End With

                        End If
                    End With
                    btnSave.IsEnabled = True

                Else
                    MsgBox("Invalid name, name may contain characters and spaces only", MsgBoxStyle.Exclamation)
                    txtDoneBy.SelectAll()
                End If
            Else
                MsgBox("Enter name here", MsgBoxStyle.Information)
            End If

        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub


    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        Try
            Dim Value As String
            lngCRec = rsLab.AbsolutePosition
            Value = lblLSNo.Content

            With rsLab
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub

                Else
                    .Close()
                    rsLab = New ADODB.Recordset()
                    rsLab.Open("SELECT *FROM tblLab WHERE LsNo=" & Value, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    CEdit = True
                    btnSave.IsEnabled = True
                    btnCancel.IsEnabled = True

                End If
            End With
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub EditReady()
        txtCost.IsEnabled = True
        txtDoneBy.IsEnabled = True
        txtLRequest.IsEnabled = True
        txtLResults.IsEnabled = True
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            If CEdit = True Then
                If rsLab.State = 1 Then rsLab.Close()
                CEdit = False

                rsLab = New ADODB.Recordset()
                rsLab.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsLab.Open("SELECT * FROM tbllab", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsLab.Move(lngCRec)
            Else

                With rsLab
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetLabData()
                    Else
                        MsgBox("Nothing to Cancel")
                        txtLResults.Focus()
                    End If
                End With

            End If

            btnSave.IsEnabled = False
            btnCancel.IsEnabled = False
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs) Handles btnFirst.Click
        Try
            With rsLab
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                GetLabData()
                            End If
                        Else
                            MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveFirst()
                        btnPrevious.IsEnabled = False
                        btnNext.IsEnabled = True
                        GetLabData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click
        Try
            With rsLab
                If .RecordCount <> 0 Then
                    If .BOF = True Or .EOF = True Then
                        .MoveFirst()
                        MsgBox("This Is the last Record", MsgBoxStyle.Information, "Navigation")
                        Exit Sub
                    End If
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                GetLabData()
                            Else
                                .CancelUpdate()
                                .MovePrevious()
                                btnNext.IsEnabled = True
                                GetLabData()
                            End If
                        Else
                            MsgBox("Can't Go To next Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        If .AbsolutePosition = 1 Then
                            .MoveFirst()
                            btnPrevious.IsEnabled = False
                            MsgBox("This Is the last Record", MsgBoxStyle.Information, "Navigation")
                        ElseIf .BOF = True Then
                            .MoveFirst()
                            btnPrevious.IsEnabled = False
                            MsgBox("This Is the last Record", MsgBoxStyle.Information, "Navigation")
                        Else
                            .MovePrevious()
                        End If
                        btnNext.IsEnabled = True

                        GetLabData()
                    End If
                End If
            End With

        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        Try
            With rsLab
                If .RecordCount <> 0 Then
                    If .EOF = True Or .BOF = True Then
                        .MoveLast()
                        MsgBox("This Is the first Record", MsgBoxStyle.Information, "Navigation")
                        Exit Sub
                    End If
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnNext.IsEnabled = False
                                GetLabData()
                            Else
                                .CancelUpdate()
                                .MoveNext()
                                btnPrevious.IsEnabled = True
                                GetLabData()
                            End If
                        Else
                            MsgBox("Can't Go To previous Record!", MsgBoxStyle.Exclamation, "Navigation")

                        End If
                    Else
                        If .RecordCount = .AbsolutePosition Then
                            .MoveLast()
                            btnNext.IsEnabled = False
                            MsgBox("This Is the first Record", MsgBoxStyle.Information, "Navigation")
                        ElseIf .EOF = True Then
                            .MoveLast()
                            btnNext.IsEnabled = False
                            MsgBox("This Is the first Record", MsgBoxStyle.Information, "Navigation")
                        Else
                            .MoveNext()
                        End If
                        btnPrevious.IsEnabled = True
                        GetLabData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click
        Try
            With rsLab
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnPrevious.IsEnabled = False
                                GetLabData()
                            End If
                        Else
                            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveLast()
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = False
                        GetLabData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub PatientBilling()
        Dim rsBill As New ADODB.Recordset
        Dim rsBillDet As New ADODB.Recordset
        Dim dbBamt As Double '
        Dim dbBal As Double '
        Dim dbTAmt As Double '
        Dim dbPBal As Double '
        Dim intPBNo As Integer '
        Dim BiNo As Integer '

        Dim TrDate As DateTime = DateTime.Today ' 


        Try
            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNo & " AND BDate='" & DateTime.Today & "' AND BAmt=Bal ORDER BY BDate DESC, BNO Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then '
                    dbBamt = .Fields("BAmt").Value
                    dbBal = .Fields("Bal").Value
                    dbPBal = .Fields("PBal").Value
                    dbTAmt = .Fields("TAmt").Value
                    intPBNo = .Fields("PBNo").Value
                    BNO = .Fields("BNo").Value
                    .Fields("uName").Value = strUser
                    .Fields("BAmt").Value = dbBamt + Val(txtCost.Text)
                    .Fields("TAmt").Value = dbTAmt + Val(txtCost.Text)
                    .Fields("Bal").Value = dbBal + Val(txtCost.Text)
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
                        .Fields("PNo").Value = lnPNo
                        .Fields("BNo").Value = BNO
                        .Fields("BiNo").Value = BiNo
                        .Fields("SAmt").Value = Val(Me.txtCost.Text)
                        .Fields("Service").Value = "Lab"
                        .Fields("RefNo").Value = "Lab Service Number " & lblLSNo.Content
                        .Update()
                        .Close()
                    End With

                Else '  
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNo & " ORDER BY BNO DESC, BDate DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
                        .Fields("PNo").Value = lnPNo
                        .Fields("BNo").Value = BNO
                        .Fields("BDate").Value = Today
                        .Fields("BAmt").Value = Val(txtCost.Text)
                        .Fields("PBNO").Value = intPBNo
                        .Fields("PBal").Value = dbPBal
                        .Fields("TAmt").Value = Val(txtCost.Text) + dbPBal
                        .Fields("Bal").Value = Val(txtCost.Text) + dbPBal
                        .Fields("UName").Value = strUser
                        .Update()
                        .Close()

                        'add entry to bill details
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
                            .Fields("PNo").Value = lnPNo
                            .Fields("BNo").Value = BNO
                            .Fields("BiNo").Value = BiNo
                            .Fields("SAmt").Value = Val(Me.txtCost.Text)
                            .Fields("Service").Value = "Lab "
                            .Fields("RefNo").Value = "Lab Service Number " & lblLSNo.Content
                            .Update()
                            .Close()
                        End With

                    Else
                        GenerateBillNo()
                        .AddNew()
                        .Fields("PNo").Value = lnPNo
                        .Fields("BNo").Value = BNO
                        .Fields("BDate").Value = Today
                        .Fields("BAmt").Value = Val(txtCost.Text)
                        .Fields("PBNO").Value = 0
                        .Fields("PBal").Value = 0
                        .Fields("TAmt").Value = Val(txtCost.Text)
                        .Fields("Bal").Value = Val(txtCost.Text)
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
                            .Fields("PNo").Value = lnPNo
                            .Fields("BNo").Value = BNO
                            .Fields("BiNo").Value = BiNo
                            .Fields("SAmt").Value = Val(Me.txtCost.Text)
                            .Fields("Service").Value = "Lab "
                            .Fields("RefNo").Value = "Lab Service Number " & lblLSNo.Content
                            .Update()
                            .Close()
                        End With
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while billing the customer " & Err.Description, MsgBoxStyle.Critical)
        End Try

    End Sub

    Private Sub GenerateBillNo()
        Dim rsBill As New ADODB.Recordset
        Try
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
        Catch ex As Exception
            MsgBox("An error has occured while generating bill number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub GenerateBillDetNo()
        Dim rsBDet As New ADODB.Recordset
        Try
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
        Catch ex As Exception
            MsgBox("An error has occured while generating bill details number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub updateQueue()

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

            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblQueue WHERE PatNo='" & strP & "' AND QNO=" & lnQNO & " and status='Waiting' AND Destination='Lab' ORDER BY QNO Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                .Fields("Status").Value = "Attended"
                .Fields("ADate").Value = Today
                .Fields("ATime").Value = Format(Now, "Long Time")
                rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
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


    Private Sub LoadAllPatients()


    End Sub

    Private Sub LoadScheduledPatients()
        bnClearQueue = True
        cboCSNo.Items.Clear()
        Dim rsCn As New ADODB.Recordset
        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT QNO, QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Lab' AND Status='Waiting' ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT PNO, Surname, Onames, Sex, PatNo FROM tblPatient WHERE PatNo ='" & rsQueue.Fields("PatNo").Value & "' ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                cboCSNo.Items.Add(rsQueue.Fields("QNO").Value & " " & rsPatient.Fields("Surname").Value & " " & rsPatient.Fields("Onames").Value)
                            End If
                            .Close()
                        End With
                        .MoveNext()
                    End While
                End If
                .Close()
            End With
            bnClearQueue = False
        Catch ex As Exception
            MsgBox("An error has occured while loading scheduled patients " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub





    Private Sub GenerateLabServiceNo()

        Try
            With rsLab
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblLab ORDER BY LSNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    lnLSNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    lnLSNo = .Fields("LSNo").Value
                End If
                lnLSNo = lnLSNo + 1
                .AddNew()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating lab service number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub




    Private Sub txtDoneBy_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtDoneBy.TextChanged
        If Trim(txtDoneBy.Text) <> "" Then
            btnSave.IsEnabled = True
        Else
            btnSave.IsEnabled = False
        End If
    End Sub

    Private Sub PickRequestedTest(str As String)
        Dim trimChars As Char() = {" ", ChrW(13), ChrW(10), "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ")", "(", ChrW(164), "/", "\", "*", ""}
        Dim Mchar As String = ""
        Dim X As Integer
        Dim p As String = ""
        Dim pChar As String = ""
        Dim sTest As String = ""
        Dim rsLabDetails As New ADODB.Recordset
        Dim rsLabTest As New ADODB.Recordset
        Dim lnLDNo As Long

        Try
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
                        sTest = sTest.Trim(trimChars)

                        With rsLabTest
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblLabTests WHERE TName='" & sTest & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .RecordCount > 0 Then
                                With rsLabDetails
                                    If .State = 1 Then .Close()
                                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                    .Open("SELECT * FROM tblLabDetails WHERE LSNo=" & lnLSNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                    If .BOF = True And .EOF = True Then
                                        lnLDNo = 0
                                    Else
                                        lnLDNo = .Fields("LSiNO").Value
                                    End If
                                    lnLDNo = lnLDNo + 1
                                    .AddNew()
                                    .Fields("TDSNO").Value = lnLDSNo
                                    .Fields("lSNO").Value = lnLSNo
                                    .Fields("lTNO").Value = rsLabTest.Fields("LTNo").Value
                                    .Fields("TResults").Value = ""
                                    .Fields("Amt").Value = rsLabTest.Fields("Cost").Value
                                    .Fields("lSiNO").Value = lnLDNo
                                    .Update()
                                    .Close()
                                End With
                            End If
                            .Close()
                        End With
                    End If
                    sTest = ""
                Else
                    sTest = sTest & Mchar
                End If
            Next X
        Catch ex As Exception
            MsgBox("An error has ocurred while getting lab tests cost " & Err.Description)
        End Try

    End Sub


    Private Sub GenerateLabDetailsNo()
        lnLDSNo = 0
        Try
            Dim rsLabDets As New ADODB.Recordset
            With rsLabDets
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblLabDetails ORDER BY TDSNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    lnLDSNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    lnLDSNo = .Fields("TDSNo").Value
                End If
                lnLDSNo = lnLDSNo + 1
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating lab details number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub


    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
        Try
            Dim rptLabRep As New rptLabResults
            Dim winRptR As New winRptI
            Dim myLogOnInfo As New TableLogOnInfo()
            Dim myTableLogOnInfos As New TableLogOnInfos
            Dim myConnectionInfo As New ConnectionInfo()
            Dim myDataSourceConnections As DataSourceConnections = rptLabRep.DataSourceConnections
            Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
            Dim iPNo As String
            Dim rsLabRep As New ADODB.Recordset
            Dim rsQ As New ADODB.Recordset

            Dim myTables As Tables
            Dim myTable As Table
            Dim myTableLogOnInfo As New TableLogOnInfo

            rptLabRep.Refresh()
            If lnLSNo <> 0 Then

                With rsLabRep
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblLab WHERE LsNo=" & lnLSNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        iPNo = .Fields("QNO").Value
                        With rsQ
                            If .State = 1 Then .Close()
                            .CursorLocation = CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblQueue WHERE QNO=" & iPNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                iPNo = .Fields("PNO").Value
                            End If
                            .Close()
                        End With

                        GetServer()
                        myConnectionInfo.ServerName = rServer
                        myConnectionInfo.DatabaseName = rDatabase
                        myConnectionInfo.UserID = ""
                        myConnectionInfo.Password = ""
                        rptLabRep.SetDatabaseLogon("sa", "*******", rServer, rDatabase)
                        rptLabRep.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "*******")
                        rptLabRep.DataSourceConnections.Item(0).SetLogon("sa", "*******")

                        myTables = rptLabRep.Database.Tables
                        For Each myTable In myTables
                            myTableLogOnInfo = myTable.LogOnInfo
                            myTableLogOnInfo.ConnectionInfo = myConnectionInfo
                            myTable.ApplyLogOnInfo(myTableLogOnInfo)
                        Next

                        rptLabRep.RecordSelectionFormula = "{tblPatient.PNo} =" & iPNo & " and {tblLab.LsNo} =" & lnLSNo & "" ' "
                        rptLabRep.Refresh()
                        winRptR.crvMain.ViewerCore.ReportSource = rptLabRep
                        winRptR.Show()
                    Else
                        MsgBox("Report number does not exist", MsgBoxStyle.Exclamation)
                    End If
                End With


            End If
        Catch ex As Exception
            MsgBox("An error has occured while preparing to print " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Public Sub GetServer()
        Dim X As Integer
        Dim strCn As String
        Dim N As Integer
        Dim iCode As String = ""
        Dim pCode As String = ""
        Dim ieCount As Integer
        Dim iCurLoc As Integer = 0

        strCn = MainWin.strConn
        N = 0
        X = 0

        For X = 1 To Len(strCn) Step 1
            iCode = Mid(strCn, X, 1)
            If iCode Like "=" = True Or iCode Like ";" = True Then
                If pCode = "Provider" Or pCode = "Data Source" Or pCode = "SQLOLEDB" Then
                    pCode = ""
                    iCode = ""
                Else
                    Exit For
                End If
            Else
            End If
            N = N + 1
            pCode = pCode & iCode
        Next X


        rServer = pCode

        pCode = ""
        iCode = ""
        N = 0
        X = 0
        ieCount = 0
        For X = 1 To Len(strCn) Step 1
            iCode = Mid(strCn, X, 1)
            If iCode Like "=" = True Then ieCount = ieCount + 1
            If iCode Like "=" = True Or iCode Like ";" = True Then
                If pCode = "Provider" Or pCode = "Data Source" Or pCode = "SQLOLEDB" Or pCode = "Initial Catalog" Then pCode = "" : iCode = ""
                If ieCount > 2 And Len(pCode) > 0 Then
                    Exit For
                Else
                    pCode = ""
                    iCode = ""
                End If
            Else
            End If
            N = N + 1
            pCode = pCode & iCode
        Next X
        rDatabase = pCode
    End Sub

    Private Sub cboLTest_GotFocus(sender As Object, e As RoutedEventArgs) Handles cboLTest.GotFocus

        Dim nQueue As Integer

        Dim rsQ As New ADODB.Recordset

        With rsQ
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT LTNO, TName, Description, cost FROM tblLabTests ORDER BY LTNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
            nQueue = .RecordCount
            .Close()
        End With

        If cboLTest.Items.Count = nQueue Then Exit Sub

        Try
            LoadLabTests()
        Catch ex As Exception
            MsgBox("An error has occured while loading lab test details " & Err.Description)
        End Try


    End Sub

    Private Sub cboLTest_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboLTest.SelectionChanged
        tNo = 0
        getTestNumber(cboLTest.SelectedItem) '
        dbLabCost = Val(txtCost.Text)
        Try
            GetLastPlanNo(txtLRequest.Text)

            With rsLabTests
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblLabTests WHERE LTNO=" & tNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    planSno = planSno + 1
                    txtLRequest.Text = txtLRequest.Text & planSno & ". " & .Fields("TName").Value & " " '
                    strLabRequest = strLabRequest & planSno & ". " & .Fields("TName").Value & "(" & .Fields("cost").Value & ")"
                    dbLabCost = dbLabCost + Val(.Fields("cost").Value)
                    txtCost.Text = dbLabCost
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading lab test data " & Err.Description)
        End Try
    End Sub

    Public Function getTestNumber(cboC As String)
        Dim Mchar As String = ""
        Dim X As Integer
        Dim p As String = ""

        Try
            For X = 1 To Len(cboC)
                Mchar = Mid(cboC, X, 1)
                If Mchar = " " Then Exit For
                p = p + Mchar
            Next X
            tNo = Val(p)
        Catch ex As Exception
            MsgBox("An error has occured while getting test number " & Err.Description)
        End Try
        Return (0)
    End Function

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

    Private Sub txtLRequest_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtLRequest.LostFocus
        getTestsCost(txtLRequest.Text)
        repeatedGroups()
    End Sub

    Private Sub GetLabTestsCost(str As String)
        Dim Mchar As String = ""
        Dim X As Integer
        Dim p As String = ""
        Dim pChar As String = ""
        Dim sTest As String = ""
        Dim rsLabDetails As New ADODB.Recordset
        Dim rsLabTest As New ADODB.Recordset
        totalCost = 0
        arrLabDet.Clear()

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


                    With rsLabTest
                        sTest = Trim(Left(sTest, Len(sTest) - 1))
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblLabTests WHERE TName='" & sTest & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then
                            totalCost = totalCost + rsLabTest.Fields("Cost").Value
                            txtCost.Text = totalCost
                            arrLabDet.Add(sTest)
                        End If
                        .Close()
                    End With


                End If

                sTest = ""
            Else
                sTest = sTest & Mchar
            End If
        Next X

    End Sub

    Private Sub GetLastPlanNo(str As String)
        Dim Mchar As String = ""
        Dim X As Integer
        Dim p As String = ""
        Dim pChar As String = ""
        Dim sTest As String = ""
        Dim rsLabDetails As New ADODB.Recordset
        Dim rsLabTest As New ADODB.Recordset
        totalCost = 0
        Try
            If str = "" Then
                planSno = 0
            Else

                X = Len(str)
                For X = Len(str) To X = 0 Step -1
                    Mchar = Mid(str, X, 1)
                    pChar = Mid(str, X - 1, 1)
                    If IsNumeric(pChar) = True And Mchar = "." Then
                        planSno = pChar '
                        Exit For
                    Else

                    End If
                Next

            End If
        Catch ex As Exception
            MsgBox("An error has occured while getting test number " & Err.Description)
            planSno = 0
        End Try
        str = "" '
    End Sub

    Private Sub LoadLabTests()
        cboLTest.Items.Clear()
        Try
            With rsLabTests
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT LTNO, TName, Description, cost FROM tblLabTests ORDER BY LTNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        cboLTest.Items.Add(.Fields("LTNO").Value & " " & .Fields("TNAME").Value & " " & .Fields("Description").Value & " " & .Fields("Cost").Value)
                        .MoveNext()
                    End While
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading lab tests " & Err.Description)
        End Try
    End Sub

    Private Sub getTestsCost(str As String)
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
                        sTest = sTest.Trim(trimChars)
                        With rsLabTest
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblLabTests WHERE TName='" & sTest & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .RecordCount > 0 Then
                                totalCost = totalCost + rsLabTest.Fields("Cost").Value
                                txtCost.Text = totalCost
                                arrLabDet.Add(sTest)
                            End If
                            .Close()
                        End With

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
End Class
