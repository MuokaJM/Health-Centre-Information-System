Imports ADODB
Imports System.Text.RegularExpressions
Imports System.Data
Imports Microsoft.SqlServer
Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine



Class pgPharmacy2

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
    Private bnResetPrescription As Boolean = False '
    Private lnOldNo As Int64

    Private strOldSel As String
    Private strCurSel As String
    Private strDrugs As String
    Private dcSingleDrugCost As Decimal '
    Private dcTotalDrugCost As Decimal '
    Private intSingleDrugQuantity As Integer
    Private intTotalDrugQuantity As Integer
    Private rServer As String
    Private rDatabase As String
    Private strDispensed As String
    Private strDosage As String
    Private strStatus As String
    Private strDrugName As String
    Private dbCost As Double
    Private arrPharmacyDetails As New ArrayList
    Private strPharmacyRequest As String
    Private strP As String
    Private strPi As String
    Private iDQty As String
    Private intDrugDays As Integer
    Private strPrescription As String
    Private strAllPrescription As String
    Private dbPharmacyCost As Double


    Private Sub pgPharmacy_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        btnCancel.IsEnabled = False
        btnEdit.IsEnabled = False
        btnSave.IsEnabled = False
        txtCost.IsEnabled = False
        txtDays.IsEnabled = False
        txtDDispensed.IsEnabled = False

        lstDrugRequested.IsEnabled = False

        txtQuantity.IsEnabled = False
        txtPrescription.IsEnabled = False
        txtRemarks.IsEnabled = False
        txtStrength.IsEnabled = False
        cboDNo.IsEnabled = False
        strStatus = "Waiting"

        lblToday.Content = Format(Today, "dd-MMMM-yy")
        dgBrush.Color = myColors(intTheme)


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
            MsgBox("An error has occured " & Err.Description)

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

    Private Sub editReady()
        btnCancel.IsEnabled = True
        btnEdit.IsEnabled = False
        btnSave.IsEnabled = True
        txtCost.IsEnabled = True
        txtDays.IsEnabled = True
        txtDDispensed.IsEnabled = True
        lstDrugRequested.IsEnabled = True
        txtQuantity.IsEnabled = True
        txtPrescription.IsEnabled = True
        txtRemarks.IsEnabled = True
        txtStrength.IsEnabled = True
        cboDNo.IsEnabled = True

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
                .Fields("DDispensed").Value = strAllPrescription
                .Fields("Quantity").Value = Val(txtQuantity.Text)
                .Fields("Cost").Value = dbPharmacyCost '
                .Fields("DDate").Value = Today
                .Fields("DRequested").Value = strAllPrescription '
                .Fields("Remarks").Value = txtRemarks.Text '
                .Fields("Uname").Value = strUser
            End With
        Catch ex As Exception
            MsgBox("An error has occured while setting pharmacy data for saving " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

        Return (0)
    End Function


    Private Sub saveDetails()
        '
        Dim strSelected As String = ""
        Dim strDrug As String = ""
        Dim i As Integer = 0
        If lstDrugRequested.SelectedItem = "" Then

        Else
            strAllPrescription = "" '

        End If
        For Each Item In lstDrugRequested.Items
            If lstDrugRequested.SelectedItems.Contains(Item) Then
                strDrug = lstDrugRequested.Items(i)
                strSelected = lstDrugRequested.Items(i) & vbCrLf
                GetDrugsCost(strDrug)
                strAllPrescription = strAllPrescription & strSelected
                saveDispensingDetails()
            End If
            i = i + 1
        Next
    End Sub


    Private Function ClearPharmData()
        cboDNo.Text = ""
        lblPSNo.Content = ""
        lblDetails.Content = ""
        txtDDispensed.Text = ""
        txtCost.Text = ""
        txtQuantity.Text = ""
        txtRemarks.Text = ""
        lstDrugRequested.Items.Clear()
        cboTimes.Text = ""
        txtStrength.Text = ""
        txtDays.Text = ""
        txtPrescription.Text = ""
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

                addItemToPrescriptionList(.Fields("DRequested").Value)

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
        If Trim(strAllPrescription) = "" Then
            MsgBox("No presecription selected!", MsgBoxStyle.Exclamation, "Save")
            Exit Sub
        End If
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
                btnSave.IsEnabled = False
            Else

                If (lstDrugRequested.SelectedItem = "") And (txtDDispensed.Text = "") Then
                    MsgBox("Please enter the dispensed drug", MsgBoxStyle.Information)
                    txtDDispensed.Focus()
                ElseIf (lstDrugRequested.SelectedItem = "") And (txtQuantity.Text = "") Then
                    MsgBox("Please enter the dispensed drug quantity", MsgBoxStyle.Information)
                    txtQuantity.Focus()
                ElseIf (lstDrugRequested.SelectedItem = "") And (txtCost.Text = "" Or Val(txtCost.Text) = 0) Then
                    MsgBox("Please enter the dispensed drug cost", MsgBoxStyle.Information)
                    txtCost.Focus()

                Else

                    With rsPharmacy
                        SetPharmData()
                        .Update()
                        .Close()

                        ' 
                        saveDetails()
                        .Open("SELECT * FROM tblPharmacy WHERE PSNo=" & lnPSNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        .Fields("DDispensed").Value = strAllPrescription
                        .Fields("Quantity").Value = Val(txtQuantity.Text)
                        .Fields("Cost").Value = Val(txtCost.Text)
                        .Fields("DRequested").Value = strAllPrescription '
                        .Update()


                        updateQueue()
                        lstPatients.Items.Clear()
                        ClearPharmData()
                        MsgBox("Record Saved!", MsgBoxStyle.Information)
                        strAllPrescription = ""
                        dbPharmacyCost = 0
                        dcTotalDrugCost = 0

                        btnSave.IsEnabled = False
                        btnEdit.IsEnabled = True
                        btnCancel.IsEnabled = False
                        LoadScheduledPatients()

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

        If txtCost.Text <> "" Or Val(txtCost.Text) <= 0 Then
            saveDispensingDetails()
        Else
            MsgBox("Invalid cost, cost may not contain letters or special symbols", MsgBoxStyle.Exclamation)
            txtCost.SelectAll()
        End If

    End Sub


    Private Sub saveDispensingDetails()
        Dim rsD As New ADODB.Recordset
        Dim rsDdDup As New ADODB.Recordset
        Dim rsPharmacyCheck As New ADODB.Recordset

        Dim dbNQty As Long
        Dim dbCQty As Long

        GenerateDisDrugNo()
        GenerateDisDrugINo()

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
            MsgBox("Error has occured ", MsgBoxStyle.Exclamation)

        End Try

        Try
            With rsPharmacyCheck
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPharmacy WHERE PSNO=" & lnPSNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                Else
                    SetPharmData()
                    rsPharmacy.Update()
                End If
                .Close()
            End With

        Catch ex As Exception
            MsgBox("Error has occured ", MsgBoxStyle.Exclamation)
        End Try
        Try
            With rsDrugDetails
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDisDrugs WHERE DDSNO=" & lnDDSNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    .AddNew()
                End If
                txtQuantity.Text = intSingleDrugQuantity
                txtCost.Text = dcSingleDrugCost

                .Fields("DDSNO").Value = lnDDSNo
                .Fields("PSNO").Value = lnPSNo
                .Fields("DNO").Value = lnDNo
                .Fields("PSiNO").Value = lnDDSiNo
                .Fields("Quantity").Value = Val(txtQuantity.Text)
                .Fields("Cost").Value = Val(txtCost.Text) '
                .Fields("DDate").Value = Today
                .Update()
                .Close()
            End With
        Catch ex As Exception
            MsgBox("Error", Err.Description)
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
        lstPatients.Items.Clear()
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
        lstPatients.Items.Clear()
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
                                    lstPatients.Items.Add(rsQueue.Fields("QNO").Value & " " & .Fields("Surname").Value & " " & .Fields("Onames").Value)
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

    Private Sub LoadPendingPatients()
        bnClearQueue = True
        lstPatients.Items.Clear()
        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT QNO, QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Pharmacy' AND Status='Pending'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
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
                                    lstPatients.Items.Add(rsQueue.Fields("QNO").Value & " " & .Fields("Surname").Value & " " & .Fields("Onames").Value)
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



    Private Sub GeneratePharmServiceNo()
        Try
            With rsPharmacy
                If .State = 1 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .Close()
                    Else
                        .Close()
                    End If

                End If

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
                '  .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating pharmacy service number " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub GenerateDisDrugNo()
        Dim rsDrugDetails3 As New ADODB.Recordset
        Try
            With rsDrugDetails3
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
        Dim rsDrugDetails2 As New ADODB.Recordset
        Try
            With rsDrugDetails2
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDisDrugs WHERE PSNO=" & lnPSNo & " ORDER BY DDSNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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




    Public Sub patientSelection()
        lnOldNo = lnCSNO
        strOldSel = strCurSel

        getConsultationNumber(strCurSel)
        If bnNew = True Then
        Else
            GeneratePharmServiceNo()
            lblPSNo.Content = lnPSNo
            editReady()
            bnNew = True
            Me.strDrugs = ""
        End If

        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT QNO, QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy, PNO, Remarks FROM tblQueue WHERE QNO= " & lnQNO & " AND destination='Pharmacy' AND Status='" & strStatus & "' ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    strPatNo = .Fields("PatNo").Value

                    With rsPatient
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT PNo, Surname, ONames, Sex FROM tblPatient WHERE PNo=" & CInt(rsQueue.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            lnPNo = .Fields("pno").Value
                            lblDetails.Content = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value) & " " & .Fields("Sex").Value
                            strPName = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value)
                        End If
                        .Close()
                    End With

                    With rsConsultation
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblConsultation WHERE PNO=" & lnPNo & " ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            .MoveFirst()
                            If IsDBNull(.Fields("DRequest").Value) = False Then
                                addItemToPrescriptionList(.Fields("DRequest").Value)
                                arrPharmacyDetails.Add(.Fields("DRequest").Value)
                            End If
                            lblCost.Content = "Total Cost: " & .Fields("PCost").Value
                            txtCost.Text = .Fields("PCost").Value
                        End If
                        .Close()
                    End With

                End If
                .Close()
            End With
            repeatedDrug()
            bnNew = False
        Catch ex As Exception
            MsgBox("An error has occured while fetching patient's details " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub


    Private Sub repeatedDrug()
        Dim X1 As Integer = 1
        Dim S1 As String = ""
        Dim S2 As String = ""
        Dim arrRepeated As New ArrayList
        Dim str As String = ""

        Try
            arrPharmacyDetails.Sort()

            While X1 < arrPharmacyDetails.Count
                S1 = arrPharmacyDetails.Item(X1 - 1)
                S2 = arrPharmacyDetails.Item(X1)
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

    Private Sub addItemToPrescriptionList(strField As String)
        Dim trimChars As Char() = {" ", ChrW(13), ChrW(10), vbCrLf}

        Dim MChar As String
        Dim intX As Integer
        Dim strEntry As String = ""

        For intX = 1 To Len(strField)
            MChar = Mid(strField, intX, 1)
            If MChar = ChrW(13) Then
                Trim(strEntry)
                lstDrugRequested.Items.Add(strEntry)
                GetDrugsCost(strEntry)
                strAllPrescription = strAllPrescription & strEntry
                strEntry = ""
                MChar = ""
                bnResetPrescription = True
            Else

            End If
            strEntry = strEntry & MChar
        Next

    End Sub

    Private Sub getSelectedDrugs()

        With lstDrugRequested
            For x = 0 To .Items.Count - 1
                If .SelectedItem(x) = True Then
                End If
            Next
        End With
    End Sub


    Public Function getConsultationNumber(strNumber As String)

        Dim Mchar As String = ""
        Dim X As Integer
        Dim p As String = ""

        For X = 1 To Len(strNumber)
            Mchar = Mid(strNumber, X, 1)
            If Mchar = " " Then Exit For
            p = p + Mchar
        Next X
        lnCSNO = Val(p)
        lnQNO = lnCSNO

        Return (0)
    End Function

    Private Sub chkAll_Click(sender As Object, e As RoutedEventArgs) Handles chkAll.Click
        If chkAll.IsChecked = True Then
            strStatus = "Pending"
            LoadPendingPatients()
            chkAll.Content = "Load scheduled patients"
        Else
            strStatus = "Waiting"
            LoadScheduledPatients()
            chkAll.Content = "Load pending  patients"
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

    Private Sub cboDNo_LostFocus(sender As Object, e As RoutedEventArgs) Handles cboDNo.LostFocus
        If bnResetPrescription = True Then
            strAllPrescription = ""
            bnResetPrescription = False
        Else

        End If
    End Sub

    Private Sub cboDNo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboDNo.SelectionChanged
        txtDDispensed.Text = ""
        txtQuantity.Text = ""
        txtCost.Text = ""
        txtDays.Text = ""
        txtStrength.Text = ""
        cboTimes.Text = ""

    End Sub

    Private Sub updatePharmacyDetails()

        Try
            SetPharmData()
            rsPharmacy.Update()
            saveDispensingDetails()

        Catch ex As Exception
            MsgBox("An error has occured while saving pharmacy data " & Err.Description, MsgBoxStyle.Exclamation)

        End Try


    End Sub

    Public Function getDrugNumber(strCode As String)

        Dim Mchar As String = ""
        Dim X As Integer
        Dim p As String = ""

        For X = (Len(strCode) + 1) To 1 Step -1
            Mchar = Mid(strCode, X, 1)
            If Mchar = " " Then Exit For
            p = p + Mchar
        Next X
        p = Mid(strCode, ((Len(strCode) + 1) - (Len(p))), (Len(p)))
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
        Dim myTableLogOnInfo As New TableLogOnInfo
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptPH.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
        Dim myTables As Tables
        Dim myTable As Table
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
                        rptPH.DataSourceConnections.Item(0).SetLogon("sa", "*******")

                        myTables = rptPH.Database.Tables
                        For Each myTable In myTables
                            myTableLogOnInfo = myTable.LogOnInfo
                            myTableLogOnInfo.ConnectionInfo = myConnectionInfo
                            myTable.ApplyLogOnInfo(myTableLogOnInfo)
                        Next

                        rptPH.RecordSelectionFormula = "{tblPatient.PNo} =" & iPNo & " and {tblPharmacy.PSNo} =" & lnPSNo & "" '
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

    Public Sub GetServer() 'This piece of code has been repeated much it needs to be DRY'd
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





    Private Sub txtDays_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtDays.LostFocus

        getDrugNumber(cboDNo.SelectedItem)

        Try
            With rsDrug
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDrugs WHERE DNO=" & lnDNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    txtDDispensed.Text = .Fields("DName").Value
                    txtCost.Text = .Fields("Cost").Value
                    strDrugName = .Fields("DName").Value
                    dcSingleDrugCost = CInt(.Fields("Cost").Value)
                    dcTotalDrugCost = dcTotalDrugCost + dcSingleDrugCost
                    dbPharmacyCost = dcTotalDrugCost
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while fetching drugs data " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
        '

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
                ElseIf (iCounter <> 1) Then
                    bnflag = True
                End If

                If bnflag = True Then '
                    MsgBox("Please enter details in the right format, e.g. 2/7")
                    Exit Sub
                End If

                drugCalculator()
                updatePharmacyDetails()
                cboDNo.SelectedItem = ""
                txtPrescription.ScrollToEnd()
                txtPrescription.Focus()
            End If

        Catch ex As Exception
            MsgBox("An error has occured while getting drug days " & Err.Description)
        End Try
    End Sub



    Private Sub lstPatients_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstPatients.SelectionChanged

        strCurSel = lstPatients.SelectedItem
        ClearPharmData()
        editReady()
        If lstPatients.SelectedItem = "" Then
        Else
            patientSelection()
        End If

    End Sub



    Private Sub GetDrugsCost(str As String)
        dcSingleDrugCost = 0
        intSingleDrugQuantity = 0
        Try
            Dim trimChars As Char() = {" ", ChrW(13), ChrW(10), ChrW(13), "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ")", "(", ChrW(164), "/", "\", "*", "@", vbCrLf}
            Dim Mchar As String = ""
            Dim X As Integer
            Dim x1 As Integer = 0 'for qty
            Dim strQ As String = ""
            Dim strC As String = ""
            Dim strC2 As String = ""
            Dim iCtr As Integer = 0
            Dim iNum As Integer = 0
            Dim p As String = ""
            Dim pChar As String = ""
            Dim sTest As String = ""
            Dim rsDrugs As New ADODB.Recordset
            Dim strDsg As String = ""
            Dim strDrg As String = ""
            Dim iDrg As Integer = 0
            Dim DCount As Integer = 0
            Dim DChar As String = ""
            Dim iSerial As Integer = 1

            Dim ix As Integer = 0 '
            Dim iChar As String
            Dim iDrugLen As Integer = 0
            Dim iSpace As Integer = 0
            Dim strDrug As String = ""
            Dim iMchar As String = ""
            Dim iDTimes As Integer = 0
            Dim drugQty As Integer = 0
            Dim iLensTest As Integer = 0
            Dim iLenDrug As Integer = 0


            dbCost = 0
            arrPharmacyDetails.Clear()
            strPharmacyRequest = ""


            If str = "" Then Exit Sub '
            getDrugDays(str)
            X = Len(str)

            For X = 1 To Len(str)
                Mchar = Mid(str, X, 1)
                If X > 1 Then pChar = Mid(str, X - 1, 1)
                If IsNumeric(pChar) = True And Mchar = "." Or X = Len(str) Then
                    If IsNumeric(sTest) <> True Then
                        If X = Len(str) Then
                            sTest = sTest.Trim(trimChars) '
                        Else
                            sTest = Trim(Left(sTest, Len(sTest) - 2))
                            sTest = sTest.Trim(trimChars)
                        End If



                        ix = 0
                        iChar = ""
                        For ix = 0 To (Len(sTest) - 1)
                            iChar = Mid(sTest, (Len(sTest) - ix), 1)
                            iMchar = iMchar & iChar
                            If iChar = " " Then
                                iSpace = iSpace + 1
                                If iSpace = 1 Then
                                    iMchar = ""
                                End If
                                iLensTest = Len(sTest)
                                If iSpace = 2 Then
                                    iLenDrug = (iLensTest - (ix + 1))
                                    strDrug = Mid(sTest, 1, iLenDrug)
                                    Exit For
                                End If
                            End If
                        Next
                        iDrugLen = 0
                        iMchar = iMchar.Trim(trimChars)

                        If iMchar = "DO" Then
                            iDTimes = 1
                        ElseIf iMchar = "DB" Then
                            iDTimes = 2
                        ElseIf iMchar = "SDT" Then
                            iDTimes = 3
                        ElseIf iMchar = "DIQ" Then
                            iDTimes = 4
                        ElseIf iMchar = "DSQ" Then
                            iDTimes = 5
                        ElseIf iMchar = "ETCON" Then
                            iDTimes = 1
                        ElseIf iMchar = "NRP" Then
                            iDTimes = Val(InputBox("Enter the number of times", "Prescription", 1))
                        End If

                        intSingleDrugQuantity = iDTimes * intDrugDays
                        txtQuantity.Text = iDQty
                        strPharmacyRequest = strPharmacyRequest & strDrug

                        With rsDrugs
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblDrugs WHERE DNAME='" & strDrug & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                lnDNo = .Fields("DNO").Value
                                dcSingleDrugCost = (Val(.Fields("Cost").Value) * intSingleDrugQuantity)
                                dbCost = dbCost + (Val(.Fields("Cost").Value) * intSingleDrugQuantity)
                                dcTotalDrugCost = dcTotalDrugCost + dcSingleDrugCost
                                dbPharmacyCost = dcTotalDrugCost
                                intTotalDrugQuantity = intTotalDrugQuantity + intSingleDrugQuantity
                                txtCost.Text = dcTotalDrugCost
                                txtQuantity.Text = intTotalDrugQuantity

                                arrPharmacyDetails.Add(sTest)
                                repeatedDrug()
                                iDQty = 0
                                strQ = ""
                                strC = ""
                                strC2 = ""
                                iCtr = 0
                            End If
                            .Close()
                        End With
                        sTest = sTest.Trim(trimChars)
                    End If

                    sTest = ""
                Else
                    sTest = sTest & Mchar
                End If
            Next X

        Catch ex As Exception
            MsgBox("An error has occured while getting drug cost ")
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

    Private Sub drugCalculator()
        Dim str1 As String = ""
        Dim num1 As Integer
        Try
            If (Strings.Trim(Me.txtDays.Text) = "") Then
                Interaction.MsgBox("Enter the number of days the drug should be taken", MsgBoxStyle.Information, Nothing)
                txtDays.Focus()
            ElseIf cboTimes.SelectedItem <> "" Then

            End If

        Catch ex As Exception
        End Try
        Try
            Me.getDrugDays(Trim(Me.txtDays.Text))

        Catch ex As Exception

        End Try

        Try
            If strDosage = "OD" Then
                num1 = 1
                Me.strPi = "OD"
            ElseIf strDosage = "BD" Then
                num1 = 2
                Me.strPi = "BD"
            ElseIf strDosage = "TDS" Then
                num1 = 3
                Me.strPi = "TDS"
            ElseIf strDosage = "QID" Then
                num1 = 4
                Me.strPi = "QID"
            ElseIf strDosage = "QSD" Then
                num1 = 5
                Me.strPi = "QSD"
            ElseIf strDosage = "M" Then
                num1 = 1
                Me.strPi = "M"
            ElseIf strDosage = "NOCTE" Then
                num1 = 1
                Me.strPi = "NOCTE"
            ElseIf strDosage = "PRN" Then
                num1 = Val(InputBox("Enter number of times ", "Consultation"))
                Me.strPi = "PRN"
            End If

        Catch ex As Exception
            MsgBox("An error has occured while calculating drug cost " & Err.Description)
        End Try

        Try
            Me.iDQty = ((intDrugDays * (num1 * 1)))


            Me.txtQuantity.Text = (Me.iDQty)
            strPrescription = strDrugName & " " & Me.strPi & " " & txtStrength.Text & " " & Me.txtDays.Text & vbCrLf

            strAllPrescription = strAllPrescription & strPrescription
            strPharmacyRequest = strPharmacyRequest & " (" & txtQuantity.Text & ") " & strPrescription & " " & vbCrLf
            txtPrescription.Text = txtPrescription.Text & " " & strPrescription
            txtPrescription.ScrollToEnd()
            dbPharmacyCost = (dbPharmacyCost) + ((dcSingleDrugCost) * Val(Me.txtQuantity.Text))
            dcTotalDrugCost = dbPharmacyCost
            lblTotalCost.Content = "Total drug(s) cost is: " & dbPharmacyCost
        Catch ex As Exception
            MsgBox("An error has occured while entering drug details " & Err.Description)
        End Try
    End Sub

    Public Function getDrugDays(ByVal cboC As String)

        Dim str1 As String = ""
        Dim str2 As String = ""
        Dim strDay As String = ""
        Try
            Dim num2 As Integer = Len(cboC)
            Dim num1 As Integer = 1
            Do While (num1 <= num2)
                str1 = Mid(cboC, num1, 1)
                If (str1 = "/") Then
                    Exit Do
                End If
                str2 = str2 & str1
                num1 = (num1 + 1)
            Loop
            strDay = Mid(str2, (Len(str2)), 1)
            intDrugDays = (Val(strDay))

        Catch exception1 As Exception
            MsgBox("An error has occured while getting drug days ")
        End Try

        Return CType(0, Integer)
    End Function

    Private Sub cboTimes_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboTimes.SelectionChanged
        strDosage = cboTimes.SelectedItem
        Try
            txtStrength.Focus()
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub txtStrength_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtStrength.LostFocus
        Dim x1 As Integer = 1 ' 
        Dim Mchar As String = ""
        If Trim(txtStrength.Text) = "" Then Exit Sub '
        Mchar = Right(Trim(txtStrength.Text), 2)
        If LCase(Mchar) = "mg" Then
        Else
            txtStrength.Text = txtStrength.Text & "mg"
        End If
        txtDays.Focus()
    End Sub

    
   
End Class

