Imports ADODB
Imports System.Text.RegularExpressions
Imports System.Data
Imports Microsoft.SqlServer
Imports System.Data.OleDb
Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine



Class pgPharmacy

    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
     Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
     Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
     Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}
    Public dgBrush As New SolidColorBrush
    Public intTheme As Integer

    Private rsPharmacy As New ADODB.Recordset
    Private rsConsultation As New ADODB.Recordset
    Private rsPatient As New ADODB.Recordset
    Private rsDrug As New ADODB.Recordset
    Private rsDrugDetails As New ADODB.Recordset

    Private dtDrugs As New DataTable
    Private daDrugs As New OleDbDataAdapter

    Private lnCSNO As Long
    Private lnQNO As Long
    Private lnPSNo As Long
    Private lnPNo As Long
    Private lnDNo As Long
    Private lnDDSNo As Long
    Private lnDDSiNo As Long
    Private CEdit As Boolean = False
    Private lngCRec As Long
    Private MainWin As New MainWindow
    Private BNO As Long
    Private BDetNo As Long
    Public strUser As String
    Private strPatNo As String
    Public strPName As String
    Private rsQueue As New ADODB.Recordset
    Private bnClearQueue As Boolean
    Private bnNew As Boolean '
    Private lnOldNo As Int64

    Private strOldSel As String
    Private strCurSel As String
    Private strDrugs As String
    Private dcDCost As Decimal
    Private dcTCost As Decimal
    Private rServer As String
    Private rDatabase As String
    Private strDispensed As String
    Private strDosage As String



    Private Sub pgPharmacy_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        btnCancel.IsEnabled = False
        btnEdit.IsEnabled = False
        btnNew.IsEnabled = False
        btnSave.IsEnabled = False

        dgBrush.Color = myColors(intTheme)
        dgDrugs.BorderThickness = New Thickness(1)
        dgDrugs.BorderBrush = dgBrush
        dgDrugs.AlternatingRowBackground = dgBrush


        Try
            LoadScheduledPatients()
        Catch ex As Exception
            MsgBox("An error has occured while loading the latest Phamarcy request " & Err.Description, MsgBoxStyle.Exclamation)
        End Try


        Try
            Me.cboTimes.Items.Add("OD")
            Me.cboTimes.Items.Add("BD")
            Me.cboTimes.Items.Add("TDS")
            Me.cboTimes.Items.Add("QID")
            Me.cboTimes.Items.Add("QSD")
            Me.cboTimes.Items.Add("M")
            Me.cboTimes.Items.Add("NOCTE")
            Me.cboTimes.Items.Add("PRN")

        Catch ex As Exception
        End Try

    End Sub

    Private Sub LoadDrugs()

        Try
            cboDNo.Items.Clear()
            With rsDrug
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDrugs ORDER BY DName, DNO ASC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        cboDNo.Items.Add(.Fields("DName").Value & " " & .Fields("TradeName").Value & " Alternative(s) " & .Fields("Alternatives").Value & " " & .Fields("DNO").Value)
                        .MoveNext()
                    End While
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading drugs details" & Err.Description)
        End Try


    End Sub

    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs) Handles btnFirst.Click
        Try
            With rsPharmacy
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                GetPharmData()
                            End If
                        Else
                            MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveFirst()
                        btnPrevious.IsEnabled = False
                        btnNext.IsEnabled = True
                        GetPharmData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while moving to the first record " & Err.Description, MsgBoxStyle.Exclamation)
        End Try


    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click
        Try
            With rsPharmacy
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
                                GetPharmData()
                            Else
                                .CancelUpdate()
                                .MovePrevious()
                                btnNext.IsEnabled = True
                                GetPharmData()
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

                        GetPharmData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while moving to the previous record " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        Try
            With rsPharmacy
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
                                GetPharmData()
                            Else
                                .CancelUpdate()
                                .MoveNext()
                                btnPrevious.IsEnabled = True
                                GetPharmData()
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
                        GetPharmData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while moving to the next record " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub


    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click
        Try
            With rsPharmacy
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnPrevious.IsEnabled = False
                                GetPharmData()
                            End If
                        Else
                            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveLast()
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = False
                        GetPharmData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while moving to the last record " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Function SetPharmData()
        Try
            With rsPharmacy
                .Fields("PSNo").Value = lnPSNo
                .Fields("QNO").Value = lnQNO
                .Fields("DDispensed").Value = strDispensed
                .Fields("Quantity").Value = Val(txtQuantity.Text)
                .Fields("Cost").Value = Val(txtCost.Text)
                .Fields("DDate").Value = Today
                .Fields("DRequested").Value = txtDRequested.Text
                .Fields("Remarks").Value = txtRemarks.Text
                .Fields("Uname").Value = strUser
            End With
        Catch ex As Exception
            MsgBox("An error has occured while setting pharmacy data for saving " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

        Return (0)
    End Function

    Private Function ClearPharmData()

        lblPSNo.Content = ""
        lblDetails.Content = ""
        txtDDispensed.Text = ""
        txtCost.Text = ""
        txtQuantity.Text = ""
        txtRemarks.Text = ""

        Return (0)
    End Function

    Private Function GetPharmData()
        Try
            With rsPharmacy
                lblPSNo.Content = .Fields("PSNO").Value

                If IsDBNull(.Fields("cost").Value) = True Or Val(.Fields("Cost").Value) = 0 Then
                    txtCost.Text = ""
                Else
                    txtCost.Text = .Fields("Cost").Value
                End If

                If IsDBNull(.Fields("Quantity").Value) = True Or Val(.Fields("Quantity").Value) = 0 Then
                    txtQuantity.Text = ""
                Else
                    txtQuantity.Text = .Fields("Quantity").Value
                End If

                txtDRequested.Text = .Fields("DRequested").Value
                txtDDispensed.Text = .Fields("DDispensed").Value
                txtRemarks.Text = .Fields("Remarks").Value
                lblToday.Content = Format(.Fields("DDate").Value, "Short Date")
            End With
        Catch ex As Exception
            MsgBox("An error has occured while getting pharmacy data " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
        Return (0)
    End Function


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
                    dbBamt = .Fields("TAmt").Value
                    dbBal = .Fields("Bal").Value
                    dbPBal = .Fields("PBal").Value
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
                        .Fields("Service").Value = "Pharmacy"
                        .Fields("RefNo").Value = "Pharmacy Service Number " & lblPSNo.Content
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
                            .Fields("Service").Value = "Pharmacy"
                            .Fields("RefNo").Value = "Pharmacy Service Number " & lblPSNo.Content
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
                            .Fields("Service").Value = "Pharmacy"
                            .Fields("RefNo").Value = "Pharmacy Service Number " & lblPSNo.Content
                            .Update()
                            .Close()
                        End With
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while billing pharmacy details " & Err.Description, MsgBoxStyle.Exclamation)
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




    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click

        Dim rsU As New ADODB.Recordset

        Try
            If CEdit = True Then
                SetPharmData()
                rsConsultation.Update()

                MsgBox("Pharmacy entry number " & Me.lblPSNo.Content & " record saved", MsgBoxStyle.Information, "Save")
                rsConsultation.Close()

                CEdit = False

                rsPharmacy = New ADODB.Recordset()
                rsPharmacy.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsPharmacy.Open("SELECT * FROM tblPharmacy", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsPharmacy.Move(lngCRec)
                btnNew.IsEnabled = True
                btnSave.IsEnabled = False
            Else
                If txtDDispensed.Text = "" Then
                    MsgBox("Please enter the dispensed drug", MsgBoxStyle.Information)
                    txtDDispensed.Focus()
                ElseIf txtQuantity.Text = "" Then
                    MsgBox("Please enter the dispensed drug quantity", MsgBoxStyle.Information)
                    txtQuantity.Focus()
                ElseIf txtCost.Text = "" Or Val(txtCost.Text) = 0 Then
                    MsgBox("Please enter the dispensed drug cost", MsgBoxStyle.Information)
                    txtCost.Focus()

                Else

                    With rsPharmacy

                        SetPharmData()
                        .Update()

                        updateQueue()
                        MsgBox("Record Saved!", MsgBoxStyle.Information)
                        btnSave.IsEnabled = False
                        btnEdit.IsEnabled = True
                        btnCancel.IsEnabled = False
                        cboCSNo.IsEnabled = True

                    End With
                End If
            End If
        Catch ex As Exception
            MsgBox("An error has occured while saving the record " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            If CEdit = True Then
                If rsPharmacy.State = 1 Then rsPharmacy.Close()
                CEdit = False

                rsPharmacy = New ADODB.Recordset()
                rsPharmacy.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsPharmacy.Open("SELECT * FROM tblPharmacy", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsPharmacy.Move(lngCRec)
            Else

                With rsPharmacy
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetPharmData()
                    Else
                        MsgBox("Nothing to Cancel")
                        txtDDispensed.Focus()
                    End If
                End With

            End If
            btnSave.IsEnabled = False
            btnCancel.IsEnabled = False
        Catch ex As Exception
            MsgBox("An error has occured while cancelling record " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        Dim Value As String
        lngCRec = rsPharmacy.AbsolutePosition
        Value = lblPSNo.Content

        Try
            With rsPharmacy
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub

                Else
                    .Close()
                    rsPharmacy = New ADODB.Recordset()
                    rsPharmacy.Open("SELECT * FROM tblPharmacy WHERE PSNO=" & Value, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    CEdit = True
                    btnSave.IsEnabled = True
                    btnCancel.IsEnabled = True
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while preparing to edit record " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub txtCost_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtCost.LostFocus

        Dim rsD As New ADODB.Recordset
        Dim rsDdDup As New ADODB.Recordset
        Dim rsDG As New ADODB.Recordset
        Dim dbNQty As Long
        Dim dbCQty As Long

        GenerateDisDrugNo()
        GenerateDisDrugINo()

        If txtCost.Text <> "" Or Val(txtCost.Text) <= 0 Then
            Try
                With rsDdDup
                    If .State = 1 Then .Close()
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblDisDrugs WHERE DNO=" & lnDNo & " AND PSNO=" & lnPSNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        MsgBox("That drug has already been added in the list", MsgBoxStyle.Exclamation)
                        .Close()
                        Exit Sub
                    End If
                    .Close()
                End With

            Catch ex As Exception

            End Try

            Try
                With rsDrugDetails
                    If .State = 1 Then .Close()
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblDisDrugs WHERE DDSNO=" & lnDDSNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .BOF = True And .EOF = True Then
                        .AddNew()
                    End If
                    .Fields("DDSNO").Value = lnDDSNo
                    .Fields("PSNO").Value = lnPSNo
                    .Fields("DNO").Value = lnDNo
                    .Fields("PSiNO").Value = lnDDSiNo
                    .Fields("Quantity").Value = Val(txtQuantity.Text)
                    .Fields("Cost").Value = Val(txtCost.Text) * Val(txtQuantity.Text)
                    .Fields("DDate").Value = Today
                    .Update()
                    .Close()

                    With rsDG
                        If .State = 1 Then .Close()
                        .CursorLocation = CursorLocationEnum.adUseClient
                        .Open("SELECT tblDisDrugs.DDSNO, tblDrugs.DName, tblDrugs.TradeName, tblDisDrugs.Quantity, tblDisDrugs.Cost FROM tblDisDrugs INNER JOIN tblDrugs ON tblDisDrugs.DNo = tblDrugs.DNo INNER JOIN tblPharmacy ON dbo.tblDisDrugs.PSNo = tblPharmacy.PSNo WHERE (dbo.tblDisDrugs.DDSNO =" & lnDDSNo & ")", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then
                            daDrugs.Fill(dtDrugs, rsDG)
                            dgDrugs.ItemsSource = dtDrugs.DefaultView
                        End If
                    End With

                End With
            Catch ex As Exception
                MsgBox("error", Err.Description)
            End Try
            Try
                With rsD
                    If .State = 1 Then .Close()
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT DNO, Quantity FROM tblDrugs WHERE DNO=" & lnDNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        dbCQty = .Fields("Quantity").Value
                    End If

                    dbNQty = dbCQty - Val(Trim(txtQuantity.Text))
                    .Fields("Quantity").Value = dbNQty
                    .Update()
                    .Close()
                End With
            Catch ex As Exception
                MsgBox("An error has occured while updating drugs quantity " & Err.Description, MsgBoxStyle.Exclamation)
            End Try

            btnSave.IsEnabled = True

            If Trim(txtDDispensed.Text) <> "" Then

                strDispensed = strDispensed & txtDDispensed.Text & " (" & txtQuantity.Text & ") (@" & (Val(txtCost.Text) * Val(txtQuantity.Text)) & ")" & vbCrLf
            End If
        Else
            MsgBox("Invalid cost, cost may not contain letters or special symbols", MsgBoxStyle.Exclamation)
            txtCost.SelectAll()
        End If


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
                .Open("SELECT * FROM tblQueue WHERE PatNo='" & strP & "' AND status='Waiting' AND Destination='pharmacy' ORDER BY qno Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
        cboCSNo.Items.Clear()
        cboCSNo.Items.Clear()
        Try
            With rsConsultation
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT PNO, CSNO, CDate FROM tblConsultation WHERE  decision LIKE '%Pharmacy%'  ORDER BY CSNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT PNo, Surname, ONames, Sex FROM tblPatient WHERE PNo=" & CInt(rsConsultation.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                cboCSNo.Items.Add(rsConsultation.Fields("CSNO").Value & " " & .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value))
                            End If
                            .Close()
                        End With
                        .MoveNext()
                    End While
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading all patients records " & Err.Description, MsgBoxStyle.Exclamation)
        End Try


    End Sub

    Private Sub LoadScheduledPatients()
        bnClearQueue = True
        cboCSNo.Items.Clear()
        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT QNO, QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Pharmacy' AND Status='Waiting'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT PNO, Surname, Onames, Sex, PatNo FROM tblPatient WHERE PatNo ='" & rsQueue.Fields("PatNo").Value & "' ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                While .EOF = False
                                    cboCSNo.Items.Add(rsQueue.Fields("QNO").Value & " " & .Fields("Surname").Value & " " & .Fields("Onames").Value)
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
            bnClearQueue = False
        Catch ex As Exception
            MsgBox("An error has occured while loading scheduled patients " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub


    Private Sub chkAll_Checked(sender As Object, e As RoutedEventArgs) Handles chkAll.Checked

    End Sub

    Private Sub btnNew_Click(sender As Object, e As RoutedEventArgs) Handles btnNew.Click
        GeneratePharmServiceNo()
        lblPSNo.Content = lnPSNo
        btnNew.IsEnabled = False
        cboCSNo.IsEnabled = False
        bnNew = True
    End Sub

    Private Sub GeneratePharmServiceNo()
        Try
            With rsPharmacy
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPharmacy ORDER BY PSNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    lnPSNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    lnPSNo = .Fields("PSNo").Value
                End If
                lnPSNo = lnPSNo + 1
                .AddNew()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating pharmacy service number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub GenerateDisDrugNo()
        Try
            With rsDrugDetails
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDisDrugs ORDER BY DDSNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    lnDDSNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    lnDDSNo = .Fields("DDSNo").Value
                End If
                lnDDSNo = lnDDSNo + 1
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating dispensed drug number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub GenerateDisDrugINo()
        Try
            With rsDrugDetails
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDisDrugs WHERE PSNO=" & lnPSNo & "ORDER BY DDSNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    lnDDSiNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    lnDDSiNo = .Fields("PSiNO").Value
                End If
                lnDDSiNo = lnDDSiNo + 1
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating dispensed drug number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub



    Private Sub cboCSNo_GotFocus(sender As Object, e As RoutedEventArgs) Handles cboCSNo.GotFocus
        Dim nQueue As Integer
        Dim rsQ As New ADODB.Recordset
        With rsQ
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Pharmacy' AND Status='Waiting'  AND PatNo NOT LIKE 'RF%' ", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
            nQueue = .RecordCount
            .Close()
        End With

        If cboCSNo.Items.Count = nQueue Then Exit Sub

        Try
            If chkAll.IsChecked = True Then
                LoadAllPatients()
            Else
                LoadScheduledPatients()
            End If

        Catch ex As Exception
            MsgBox("An error has occured while loading patients details R" & Err.Description)
        End Try
    End Sub



    Private Sub cboCSNo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboCSNo.SelectionChanged
        lnOldNo = lnCSNO
        strOldSel = strCurSel
        strCurSel = (Me.cboCSNo.SelectedItem)
        getConsultationNumber()

        If bnNew = True Then

        Else
            GeneratePharmServiceNo()
            lblPSNo.Content = lnPSNo
            btnNew.IsEnabled = False
            cboCSNo.IsEnabled = False
            btnNew.IsEnabled = False
            bnNew = True
            Me.strDrugs = ""

        End If


        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT QNO, QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy, PNO, Remarks FROM tblQueue WHERE QNO= " & lnQNO & " AND destination='Pharmacy' AND Status='Waiting' ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    strPatNo = .Fields("PatNo").Value
                    If IsDBNull(.Fields.Item("remarks").Value) = False Then txtDRequested.Text = .Fields.Item("remarks").Value

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
                        .Open("SELECT * FROM tblConsultation WHERE PNO=" & lnPNo & " ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            .MoveFirst()
                            If Trim(txtDRequested.Text) = "" Then
                                If IsDBNull(.Fields("prescription").Value) = False Then
                                    txtDRequested.Text = .Fields("prescription").Value
                                End If
                            End If
                            txtCost.Text = .Fields("PCost").Value
                        End If
                        .Close()
                    End With

                End If
                .Close()
            End With
            btnNew.IsEnabled = True
            bnNew = False
        Catch ex As Exception
            MsgBox("An error has occured while fetching patient's details " & Err.Description, MsgBoxStyle.Exclamation)
        End Try


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
        lnCSNO = Val(p)
        lnQNO = lnCSNO

        Return (0)
    End Function

    Private Sub chkAll_Click(sender As Object, e As RoutedEventArgs) Handles chkAll.Click
        If chkAll.IsChecked = True Then
            LoadScheduledPatients()
            chkAll.Content = "Load all patients"
        Else
            LoadAllPatients()
            chkAll.Content = "Load scheduled patients"

        End If
    End Sub

    Private Sub cboDNo_GotFocus(sender As Object, e As RoutedEventArgs) Handles cboDNo.GotFocus
        Dim nQueue As Integer
        Dim rsQ As New ADODB.Recordset

        With rsQ
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblDrugs ORDER BY DNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            nQueue = .RecordCount
            .Close()
        End With

        If cboDNo.Items.Count = nQueue Then Exit Sub

        Try
            LoadDrugs()
        Catch ex As Exception
            MsgBox("An error has occured while loading lab test details " & Err.Description)
        End Try
    End Sub

    Private Sub cboDNo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboDNo.SelectionChanged

        txtDDispensed.Text = ""
        txtQuantity.Text = ""
        txtCost.Text = ""
        txtDosage.Text = ""

        getDrugNumber()

        Try
            With rsDrug
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDrugs WHERE DNO=" & lnDNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    txtDDispensed.Text = .Fields("DName").Value
                    txtCost.Text = .Fields("Cost").Value
                    GetDrugName(txtDRequested.Text, txtDDispensed.Text)
                End If
                .Close()
            End With

            Try
                SetPharmData()

                rsPharmacy.Update()
            Catch ex As Exception

            End Try

        Catch ex As Exception
            MsgBox("An error has occured while fetching drugs data " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Public Function getDrugNumber()

        Dim Mchar As String = ""
        Dim cboC As String
        Dim X As Integer
        Dim p As String = ""

        cboC = cboDNo.SelectedItem
        For X = (Len(cboC) + 1) To 1 Step -1
            Mchar = Mid(cboC, X, 1)
            If Mchar = " " Then Exit For
            p = p + Mchar
        Next X
        p = Mid(cboC, ((Len(cboC) + 1) - (Len(p))), (Len(p)))
        lnDNo = Val(p)


        Return (0)
    End Function

    Private Sub txtQuantity_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtQuantity.LostFocus
        If IsNumeric(Trim(txtQuantity.Text)) = False Then
            MsgBox("Invalid entry! Enter a number '0...9'")
            txtQuantity.Text = ""
        Else
            txtCost.Focus()
        End If
    End Sub


    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
        Dim rptPH As New rptPatPresLabel
        Dim winRptR As New winRptI
        Dim myLogOnInfo As New TableLogOnInfo()
        Dim myTableLogOnInfos As New TableLogOnInfos
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptPH.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
        Dim iPNo As String
        Dim rsPharmRep As New ADODB.Recordset
        Dim rsQ As New ADODB.Recordset
        Dim rsPat As New ADODB.Recordset

        rptPH.Refresh()
        If lnPSNo <> 0 Then
            With rsPharmRep
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPharmacy WHERE PSNo=" & lnPSNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    iPNo = .Fields("QNO").Value
                    With rsQ
                        If .State = 1 Then .Close()
                        .CursorLocation = CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblQueue WHERE QNO=" & iPNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            iPNo = .Fields.Item("PNO").Value
                        End If
                    End With

                    GetServer()
                    Try
                        myConnectionInfo.ServerName = rServer
                        myConnectionInfo.DatabaseName = rDatabase
                        myConnectionInfo.UserID = "sa"
                        myConnectionInfo.Password = "*******"
                        rptPH.SetDatabaseLogon("sa", "*******", rServer, rDatabase)
                        rptPH.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "*******")
                        rptPH.DataSourceConnections.Item(0).SetLogon("sa", "******") '
                        rptPH.RecordSelectionFormula = "{tblPatient.PNo} =" & iPNo & " and {tblPharmacy.PSNo} =" & lnPSNo & ""
                        rptPH.Refresh()
                        winRptR.crvMain.ViewerCore.ReportSource = rptPH
                        winRptR.Show()
                    Catch ex As Exception
                        MsgBox(Err.Description)
                    End Try
                Else
                    MsgBox("Report number does not exist", MsgBoxStyle.Exclamation)
                End If
            End With


        End If
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

  

    Private Sub GetDrugName(SearchWithinString As String, SearchForString As String)
        ' Dim SearchWithinString As String ' = "ABCDEFGHIJKLMNOP"
        ' Dim SearchForString As String '= "DEF"

        ''   Dim FirstCharacter As Integer = SearchWithinString.IndexOf(SearchForString)

    End Sub

    Private Sub txtDays_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtDays.LostFocus



        Try
            Dim bnflag As Boolean = False '
            Dim x1 As Integer = 1 ' 
            Dim Mchar As String = ""
            Dim iCounter As Integer = 0
            If (Trim(Me.txtDays.Text) = "") Then
                Exit Sub
            Else

                Do While (x1 <= Len(txtDays.Text))
                    Mchar = Mid(txtDays.Text, x1, 1)
                    If Mchar = "/" Then
                        iCounter = iCounter + 1
                    End If
                    x1 = x1 + 1
                Loop

                If iCounter = 1 Then
                    bnflag = False
                    txtDosage.Text = cboTimes.SelectedItem & " " & txtDays.Text
                ElseIf (iCounter <> 1) Then
                    bnflag = True
                End If

                If bnflag = True Then '
                    MsgBox("Please enter details in the right format, e.g. 2/7")
                    Exit Sub
                End If
                Me.cboTimes.Text = ""
            End If

        Catch ex As Exception
            MsgBox("An error has occured while getting drug days " & Err.Description)
        End Try
    End Sub

    
End Class
