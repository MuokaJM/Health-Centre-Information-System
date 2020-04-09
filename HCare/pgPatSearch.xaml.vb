Imports System.Security
Imports Microsoft.Win32
Imports System.Reflection.Assembly
Imports System.Diagnostics.FileVersionInfo
Imports System.Data
Imports Microsoft.SqlServer
Imports ADODB
Imports System.Data.OleDb
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Windows.Documents
Imports System.Xaml
Imports System.ComponentModel
Imports System.Collections.ObjectModel



Class pgPatSearch
    Private rsPatient As New ADODB.Recordset
    Private dtPatient As New DataTable
    Private daPatient As New OleDbDataAdapter
    Private cvFilter As ICollectData
    Private MainWin As New MainWindow
    Public strUser As String
    Private strSentTo As String
    Public dgBrush As New SolidColorBrush
    Private BDetNo As Long
    Private BNo As Long
    Private PNo As Long
    Private curRAmt As Decimal

    Private Sub pgPatSearch_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        btnRevisit.IsEnabled = False
        cboSentTo.Items.Add("Consultation")
        cboSentTo.Items.Add("Lab")
        cboSentTo.Items.Add("Pharmacy")
        cboSentTo.Items.Add("ANC")
        cboSentTo.Items.Add("CWC")
        cboSentTo.Items.Add("FP")
        cboSentTo.Items.Add("Nurse")
        dgPat.BorderThickness = New Thickness(1)
        Try
            With rsPatient
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT Pno as Serial_NO, PatNo as Patient_No, Surname, Onames as Other_Names, Sex, DoB as Born, Phone,  Address, subloc as Sub_Location FROM tblPatient", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    lblRecNo.Content = .RecordCount & " Records found"
                Else
                    lblRecNo.Content = "No record to display"
                End If
            End With
            daPatient.Fill(dtPatient, rsPatient)
            dgPat.ItemsSource = dtPatient.DefaultView
        Catch ex As Exception
            MsgBox("An error has occured while getting patients data " & Err.Description, MsgBoxStyle.Exclamation)
        End Try


    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtSearch.TextChanged
        Try
            If Me.txtSearch.Text = "" Then
                dgPat.ItemsSource = ""
                dtPatient.Clear()
                With rsPatient
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT Pno as Serial_NO, PatNo as Patient_No, Surname, ONames as Other_Names, Sex, DoB as Born,  Phone, Address, subloc as Sub_Location FROM tblPatient", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        lblRecNo.Content = .RecordCount & " Records found"
                    Else
                        lblRecNo.Content = "No record to display"
                    End If
                End With
                daPatient.Fill(dtPatient, rsPatient)
                dgPat.ItemsSource = dtPatient.DefaultView
            Else
                SearchPatient()
            End If
        Catch ex As Exception
            MsgBox("An error has occured while searching records " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub


    Private Sub SearchPatient()
        dgPat.ItemsSource = ""
        dtPatient.Clear()
        Try
            If Me.txtSearch.Text <> "" Then
                If IsNumeric(txtSearch.Text) = True Then
                    With rsPatient
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT Pno as Serial_NO, PatNo as Patient_No, Surname, ONames as Other_Names, Sex, DoB as Born, Phone, Address, subloc as Sub_Location FROM tblPatient WHERE Surname LIKE '%" & txtSearch.Text & "%' OR Onames LIKE '%" & txtSearch.Text & "%' OR Subloc LIKE '%" & txtSearch.Text & "%' OR Address LIKE '%" & txtSearch.Text & "%' OR Sex LIKE '%" & txtSearch.Text & "%' OR PNo='" & txtSearch.Text & "' OR Phone LIKE '%" & txtSearch.Text & "%'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then
                            lblRecNo.Content = .RecordCount & " Records found"
                        Else
                            lblRecNo.Content = "No record to display"
                        End If

                    End With
                Else

                    With rsPatient
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT  Pno as Serial_NO, PatNo as Patient_No, Surname, ONames as Other_Names, Sex, DoB as Born, Phone, Address, subloc as Sub_Location  FROM tblPatient WHERE Surname LIKE '%" & txtSearch.Text & "%' OR Onames LIKE '%" & txtSearch.Text & "%' OR Subloc LIKE '%" & txtSearch.Text & "%' OR Address LIKE '%" & txtSearch.Text & "%' OR Sex LIKE '%" & txtSearch.Text & "%' OR Phone LIKE '%" & txtSearch.Text & "%'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then
                            lblRecNo.Content = .RecordCount & " Records found"
                        Else
                            lblRecNo.Content = "No record to display"
                        End If

                    End With
                End If
                daPatient.Fill(dtPatient, rsPatient)
                dgPat.ItemsSource = dtPatient.DefaultView
            Else
            End If
        Catch ex As Exception
            MsgBox("An error has occured while searching record " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub btnRevisit_Click(sender As Object, e As RoutedEventArgs) Handles btnRevisit.Click
        Dim rsQueue As New ADODB.Recordset
        Dim lnQNo As Long
        Dim rsU As New ADODB.Recordset()

        Try
            If Trim(strSentTo) = "" Then
                MsgBox("Please select the destination of the patient", MsgBoxStyle.Information)
                cboSentTo.Focus()
                Exit Sub
            Else


                With rsQueue
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblQueue WHERE DESTINATION='" & strSentTo & "' AND PatNo='" & Trim(txtPatNo.Text) & "' AND status='Waiting' ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        MsgBox("The patient has pending schedule " & .Fields("Destination").Value & " of " & .Fields("QDate").Value, MsgBoxStyle.Information)
                        .Close()
                        Exit Sub
                    End If
                End With

                billReturn()

                If curRAmt > 0 Then
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
                        .Fields("PatNo").Value = rsPatient.Fields("Patient_No").Value
                        .Fields("PName").Value = rsPatient.Fields("Surname").Value & " " & rsPatient.Fields("Other_Names").Value
                        .Fields("Destination").Value = "Reception"
                        .Fields("Status").Value = "Waiting"
                        rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                        .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value
                        rsU.Close()
                        .Fields("Uname").Value = strUser
                        .Fields("Remarks").Value = "To Pay: " & curRAmt
                        .Update()
                        .Close()
                    End With
                ElseIf curRAmt < 0 Then
                    Exit Sub
                End If



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

                    .Fields("PatNo").Value = rsPatient.Fields("Patient_No").Value
                    .Fields("PName").Value = rsPatient.Fields("Surname").Value & " " & rsPatient.Fields("Other_Names").Value
                    .Fields("Destination").Value = strSentTo
                    If curRAmt > 0 Then
                        .Fields("Status").Value = "Pending"
                    ElseIf curRAmt = 0 Then
                        .Fields("Status").Value = "Waiting"
                    End If
                    rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                    .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value
                    rsU.Close()
                    .Fields("Uname").Value = strUser
                    .Fields("Remarks").Value = "Re-Visit"
                    .Update()
                    MsgBox("Patient scheduled successfully")
                    curRAmt = 0
                    strSentTo = ""
                    btnRevisit.IsEnabled = False
                End With
            End If
        Catch ex As Exception
            MsgBox("An error has occured while scheduling a patient for revisit " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub txtPatNo_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtPatNo.LostFocus
        Dim rsQueue As New ADODB.Recordset
        dgPat.ItemsSource = ""
        dtPatient.Clear()
        Try
            If Trim(txtPatNo.Text) = "" Then
                MsgBox("Please enter the full patient number", MsgBoxStyle.Information)
                Exit Sub
            Else
                With rsPatient
                    If .State = 1 Then .Close()
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT Pno as Serial_NO, PatNo as Patient_No, Surname, Onames as Other_Names, Sex, DoB as Born, Phone,  Address, subloc as Sub_Location FROM tblPatient WHERE PatNo='" & Trim(txtPatNo.Text) & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        With rsQueue
                            .CursorLocation = CursorLocationEnum.adUseClient
                            .Open("SELECT PatNo, Status, Destination FROM tblQueue WHERE PatNo='" & Trim(txtPatNo.Text) & "' AND status='waiting'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                MsgBox("Patient is scheduled and is yet to be attended at " & .Fields("Destination").Value, MsgBoxStyle.Exclamation)
                                With rsPatient
                                    If .State = 1 Then .Close()
                                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                    .Open("SELECT Pno as Serial_NO, PatNo as Patient_No, Surname, Onames as Other_Names, Sex, DoB as Born, Phone,  Address, subloc as Sub_Location FROM tblPatient", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                    If .RecordCount > 0 Then
                                        lblRecNo.Content = .RecordCount & " Records found"
                                    Else
                                        lblRecNo.Content = "No record to display"
                                    End If
                                End With
                                daPatient.Fill(dtPatient, rsPatient)
                                dgPat.ItemsSource = dtPatient.DefaultView

                            Else
                                daPatient.Fill(dtPatient, rsPatient)
                                dgPat.ItemsSource = dtPatient.DefaultView
                                PNo = Val(rsPatient.Fields("Serial_NO").Value)
                                btnRevisit.IsEnabled = True
                                btnRevisit.Focus()
                            End If
                        End With
                    Else
                        MsgBox("The patient number does not exist", MsgBoxStyle.Information)

                        Exit Sub
                    End If
                End With
            End If
        Catch ex As Exception
            MsgBox("An error has occured while searching patient records " & Err.Description, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub billReturn()
        Dim rsBill As New ADODB.Recordset
        Dim rsBillDet As New ADODB.Recordset

        Dim dbBamt As Double '
        Dim dbBal As Double '
        Dim dbPBal As Double '
        Dim intPBNo As Integer '
        Dim dbTAmt As Double ' 
        Dim BiNo As Integer '

        curRAmt = Val(InputBox("Enter revisit amount", , 20))
        If curRAmt < 0 Then
            MsgBox("Amount cannot be less than zero (0). Try again and enter correct amount")
            Exit Sub
        End If

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
                    BNo = .Fields("BNo").Value
                    .Fields("uName").Value = strUser
                    .Fields("BAmt").Value = dbBamt + Val(curRAmt)
                    .Fields("TAmt").Value = dbTAmt + Val(curRAmt)
                    .Fields("Bal").Value = dbBal + Val(curRAmt)
                    .Update()
                    .Close()
                    GenerateBillDetNo()
                    With rsBillDet
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblBillDetails WHERE BNo=" & BNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then
                            .MoveLast()
                            BiNo = .Fields("BiNo").Value + 1
                        ElseIf .BOF And .EOF = True Then
                            BiNo = 1
                        End If
                        .AddNew()
                        .Fields("SNo").Value = BDetNo
                        .Fields("PNo").Value = PNo
                        .Fields("BNo").Value = BNo
                        .Fields("BiNo").Value = BiNo
                        .Fields("SAmt").Value = Val(curRAmt)
                        .Fields("Service").Value = "Revist"
                        .Fields("RefNo").Value = "Patient Number " & Trim(txtPatNo.Text) & " Revisit"
                        .Update()
                        .Close()
                    End With

                Else '  
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
                            .Fields("Remarks").Value = "Balance Carried Forward to Bill No. " & BNo
                            .Update()
                        End If
                        intPBNo = CInt(.Fields("BNo").Value)

                        .AddNew()
                        .Fields("PNo").Value = PNo
                        .Fields("BNo").Value = BNo
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
                            .Open("SELECT * FROM tblBillDetails WHERE BNo=" & BNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .RecordCount > 0 Then
                                .MoveLast()
                                BiNo = .Fields("BiNo").Value + 1
                            ElseIf .BOF And .EOF = True Then
                                BiNo = 1
                            End If
                            .AddNew()
                            .Fields("SNo").Value = BDetNo
                            .Fields("PNo").Value = PNo
                            .Fields("BNo").Value = BNo
                            .Fields("BiNo").Value = BiNo
                            .Fields("SAmt").Value = Val(curRAmt)
                            .Fields("Service").Value = "Revisit"
                            .Fields("RefNo").Value = "Patient Number " & LCase(Trim(txtPatNo.Text)) & " Revisit"
                            .Update()
                            .Close()
                        End With

                    Else
                        GenerateBillNo()
                        .AddNew()
                        .Fields("PNo").Value = PNo
                        .Fields("BNo").Value = BNo
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
                            .Open("SELECT * FROM tblBillDetails WHERE BNo=" & BNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .RecordCount > 0 Then
                                .MoveLast()
                                BiNo = .Fields("BiNo").Value + 1
                            ElseIf .BOF And .EOF = True Then
                                BiNo = 1
                            End If
                            .AddNew()
                            .Fields("SNo").Value = BDetNo
                            .Fields("PNo").Value = PNo
                            .Fields("BNo").Value = BNo
                            .Fields("BiNo").Value = BiNo
                            .Fields("SAmt").Value = curRAmt
                            .Fields("Service").Value = "Revisit"
                            .Fields("RefNo").Value = "Patient Number " & Trim(txtPatNo.Text) & " Revisit"
                            .Update()
                            .Close()
                        End With
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while billing a patient for revisit " & Err.Description, MsgBoxStyle.Exclamation)
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
                    BNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    BNo = .Fields("BNo").Value
                End If
                BNo = BNo + 1
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


    Private Sub dgPat_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles dgPat.SelectionChanged
        On Error Resume Next
        Dim myRow As DataRowView = dgPat.SelectedItem
        Dim intIdex As Integer = dgPat.CurrentCell.Column.DisplayIndex

        txtCEditing.Text = myRow.Row.ItemArray(2).ToString
        txtCEditing.Text = txtCEditing.Text & " " & myRow.Row.ItemArray(3).ToString
        txtCEditing.Text = txtCEditing.Text & " " & myRow.Row.ItemArray(1).ToString
        txtCEditing.Text = txtCEditing.Text & " " & myRow.Row.ItemArray(4).ToString
        txtCEditing.Text = txtCEditing.Text & " " & myRow.Row.ItemArray(7).ToString

        txtPatNo.Text = myRow.Row.ItemArray(1).ToString
        PNo = Val(myRow.Row.ItemArray(0).ToString)
        txtPatNo.Focus()

    End Sub


   
    

    Private Sub cboSentTo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboSentTo.SelectionChanged
        strSentTo = cboSentTo.SelectedItem
    End Sub
End Class
