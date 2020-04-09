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




Class pgBill

    Private lngCRec As Long 'current record
    Private CEdit As Boolean
    Private bnBSaved As Boolean
    Private iAns As Integer
    Private bnfrmP As Boolean
    Private MainWin As New MainWindow
    Public rsBill As New ADODB.Recordset()
    Private rsBillDet As New ADODB.Recordset
    Private rsPatient As New ADODB.Recordset
    Private BNO As Integer
    Private BiNo As Integer
    Private BsNo As Integer
    Private PNo As Integer
    Private bDate As String
    Private BDetNo As Integer
    Private SMNo As Integer
    Private BAmt As Double 'bill amount
    Public strUser As String


    Private Sub pgBill_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Try
            lblToday.Content = Today

            With rsPatient
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPatient pno", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    Do While .EOF = False
                        Me.cboPatientName.Items.Add(.Fields("PNo").Value & " " & .Fields("Surname").Value & " " & .Fields("Onames").Value)
                        .MoveNext()
                    Loop
                End If
                .Close()
            End With
        Catch
            MsgBox("An error has occured during form load")
        End Try

    End Sub

    Private Sub btnNew_Click(sender As Object, e As RoutedEventArgs) Handles btnNew.Click

        With rsBill
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblBill ORDER BY BNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF And .EOF Then
                BNo = 0
            Else
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                .MoveLast()
                BNo = .Fields("BNo").Value
            End If

            BNo = BNo + 1
            BAmt = 0
            PNo = 0
            txtAmount.Text = ""
            txtService.Text = ""
            dgBillDet.ItemsSource = ""
            lblBal.Content = ""
            lblPatientDetails.Content = ""
            lblPreviousAmt.Content = ""
            lblPreviousBNo.Content = ""
            lblTotal.Content = ""
            btnSave.IsEnabled = True
            btnCancel.IsEnabled = True
            btnNew.IsEnabled = False
            bnBSaved = False
            lblToday.Content = Today
            lblBillNo.Content = BNo
            cboPatientName.Focus()
        End With

    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click

        With rsBill

            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblBill WHERE BNO=" & BNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            .Fields("PNo").Value = PNo
            .Fields("BNo").Value = Val(lblBillNo.Content)
            .Fields("BDate").Value = Today
            .Fields("BAmt").Value = BAmt
            .Fields("PBNO").Value = Val(lblPreviousBNo.Content)
            .Fields("PBal").Value = Val(lblBal.Content)
            .Fields("TAmt").Value = Val(lblTotal.Content)
            .Fields("UName").Value = strUser
            .Update()

            MsgBox("Record Saved!", MsgBoxStyle.Information)
            btnNew.IsEnabled = True
            btnSave.IsEnabled = False
        End With

    End Sub

    Private Sub btnNewBItem_Click(sender As Object, e As RoutedEventArgs) Handles btnNewBItem.Click


        With rsBillDet
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblBillDetails ORDER BY SNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF And .EOF Then
                BsNo = 0
            Else
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                .MoveLast()
                BsNo = .Fields("SNo").Value
            End If
            .Close()
        End With

        BsNo = BsNo + 1

        With rsBillDet
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblBillDetails WHERE BNO=" & BNO & " ORDER BY BiNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF And .EOF Then
                BiNo = 0
            Else
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                .MoveLast()
                BiNo = .Fields("BiNo").Value
            End If

            BiNo = BiNo + 1
            txtAmount.Text = ""
            txtService.Text = ""
            txtService.Focus()
        End With

    End Sub

    Private Sub txtAmount_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtAmount.LostFocus

        Dim dtBillDet As New DataTable
        Dim daBillDet As New OleDbDataAdapter
        getPatientNumber()
        With rsBill
            If bnBSaved = False Then
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                .AddNew()
                bnBSaved = True
            Else
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE BNo=" & BNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            End If

            .Fields("PNo").Value = PNo
            .Fields("BNo").Value = lblBillNo.Content
            .Fields("BDate").Value = Today
            .Fields("BAmt").Value = BAmt
            .Fields("PBNO").Value = Val(lblPreviousBNo.Content)
            .Fields("PBal").Value = Val(lblBal.Content)
            .Fields("TAmt").Value = Val(lblTotal.Content)
            .Fields("UName").Value = strUser
            .Update()
        End With

        With rsBillDet
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT BNO as Bill_Number, sNo as Serial_Number, BiNo as Item_Number, Service, SAMT as Amount FROM tblBillDetails WHERE BNO=" & Val(lblBillNo.Content), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            .AddNew()
            .Fields("Serial_Number").Value = BsNo
            .Fields("Bill_Number").Value = Val(lblBillNo.Content)
            .Fields("Item_Number").Value = BiNo
            .Fields("Amount").Value = txtAmount.Text
            .Fields("Service").Value = txtService.Text
            .Update()

        End With

        BAmt = BAmt + Val(txtAmount.Text)
        lblTotal.Content = Val(lblBal.Content) + BAmt


        daBillDet.Fill(dtBillDet, rsBillDet)
        dgBillDet.ItemsSource = dtBillDet.DefaultView

        btnNewBItem.IsEnabled = True
        btnNewBItem.Focus()

    End Sub


    Private Sub cboPatientName_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboPatientName.SelectionChanged

        getPatientNumber()

        With rsPatient
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblPatient WHERE PNO=" & PNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
            If .RecordCount > 0 Then
                lblPatientDetails.Content = .Fields("Sex").Value & " " & .Fields("SubLoc").Value
            End If
            .Close()
        End With

    End Sub

    Public Function getPatientNumber()

        Dim Mchar As String = ""
        Dim cboC As String
        Dim X As Integer
        Dim p As String = ""
       
        cboC = cboPatientName.SelectedItem
        For X = 1 To Len(cboC)
            Mchar = Mid(cboC, X, 1)
            If Mchar = " " Then Exit For
            p = p + Mchar
        Next X
        PNo = Val(p)
            
        Return (0)
    End Function

    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
        Dim win2 As New winRpt
        Dim doc As New FixedDocument
        Dim pg As New PageContent
        Dim strPath As String = GetExecutingAssembly.FullName
        Dim strOutPath As String = GetExecutingAssembly.FullName

        If (Thread.CurrentThread.GetApartmentState() <> ApartmentState.STA) Then


        End If
       
        win2.Show()

    End Sub
End Class
