Imports ADODB
Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine
Imports System.Text.RegularExpressions

Class pgPayment

    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
          Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
          Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
          Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}
    Private myBrush As New SolidColorBrush(myColors(0))



    Private MainWin As New MainWindow
    Private rsPayment As New ADODB.Recordset
    Private rsBill As New ADODB.Recordset
    Private rsBillDetails As New ADODB.Recordset
    Private rsPatient As New ADODB.Recordset
    Private rsConsultation As New ADODB.Recordset
    Private rServer As String
    Private rDatabase As String
    Private lnBNO As Long
    Private lnPYNO As Long
    Private lnPNO As Long
    Private dbTAmt As Double
    Private dbAmtP As Double
    Private dbBal As Double
    Private dbPrePaid As Double
    Public strUser As String
    Private bnNewRecord As Boolean = False '
    Private intReloadBills As Integer = 0 '
    Private intCompareBNo As Integer

    Private Sub pgPayment_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        lblToday.Content = Format(Today, "dd-MMMM-yy")
        txtCashGiven.IsEnabled = False
        txtRef.IsEnabled = False
        LoadBill()

    End Sub

    Private Sub LoadBill()
        cboBNo.Items.Clear()
        Try
            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE BAL > 0 ORDER BY BNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT PNo, Surname, ONames, Sex, Org FROM tblPatient WHERE PNo=" & CInt(rsBill.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                If (IsDBNull(.Fields("Org").Value) = True) Or (Trim(.Fields("Org").Value) = "") Then '
                                    cboBNo.Items.Add(rsBill.Fields("BNO").Value & " " & .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value))
                                End If
                            End If
                            .Close()
                        End With
                        .MoveNext()
                    End While
                End If
                intReloadBills = intReloadBills + 1
            End With
        Catch ex As Exception

        End Try

    End Sub

    Public Function getBillNumber(str As String)

        Dim Mchar As String = ""
        Dim X As Integer
        Dim p As String = ""

        For X = 1 To Len(str)
            Mchar = Mid(str, X, 1)
            If Mchar = " " Then Exit For
            p = p + Mchar
        Next X
        lnBNO = Val(p)

        Return (0)
    End Function

    Private Sub cboBNo_GotFocus(sender As Object, e As RoutedEventArgs) Handles cboBNo.GotFocus

        Dim nQueue As Integer
        Dim rsQ As New ADODB.Recordset
        Dim x As Integer = 1
        Try
            With rsQ
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE BAL > 0 ORDER BY BNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)

                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT PNo, Surname, ONames, Sex, Org FROM tblPatient WHERE PNo=" & CInt(rsQ.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                If (IsDBNull(.Fields("Org").Value) = True) Or (Trim(.Fields("Org").Value) = "") Then 'dont show corporate bills here! Or (Trim(.Fields("Org").Value) = "")
                                    nQueue = nQueue + 1
                                End If
                            End If
                            .Close()
                        End With
                        .MoveNext()
                    End While
                End If
                .Close()
            End With

        Catch ex As Exception
            '    MsgBox(Err.Description)
        End Try

        Try
            If cboBNo.Items.Count = nQueue Then ' 

                If intReloadBills = 1 Then
                    LoadBill()
                    Exit Sub
                Else
                    Exit Sub
                End If
            Else

                LoadBill()
                intReloadBills = 0 '
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading lab test details " & Err.Description)
        End Try

    End Sub


    Private Sub cboBNo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboBNo.SelectionChanged
        getBillNumber(cboBNo.SelectedItem)


        With rsBill
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblBill WHERE BNO=" & lnBNO & " ORDER BY BNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .RecordCount > 0 Then
                .MoveFirst()
                lnPNO = .Fields("PNo").Value
                If IsDBNull(.Fields("TAmt").Value) = True Then
                    dbAmtP = 0
                Else
                    dbTAmt = .Fields("TAmt").Value
                End If

                If IsDBNull(.Fields("Bal").Value) = True Then
                    dbBal = 0
                Else
                    dbBal = .Fields("Bal").Value
                End If
                lblBAmt.Content = dbTAmt
                lblBalance.Content = dbBal
                dbPrePaid = dbTAmt - dbBal
                If dbPrePaid > 0 Then
                    lblPrePaid.Content = dbTAmt - dbBal
                Else
                    lblPrePaid.Content = 0
                End If


                With rsPatient
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT PNo, Surname, ONames, Sex, SubLoc FROM tblPatient WHERE PNo=" & CInt(rsBill.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        .MoveFirst()
                        lblDetails.Content = .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value) & " " & .Fields("Sex").Value & " " & .Fields("SubLoc").Value
                    End If
                    .Close()
                End With
            End If
            .Close()
        End With
        If bnNewRecord = False Then
            CreateNewRecord()
        End If
        txtCashGiven.Focus()
        InvalidateArrange()
        InvalidateVisual()

    End Sub



    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click

        If optCheque.IsChecked = True Or optMpesa.IsChecked = True Or optOther.IsChecked = True Then
            If txtRef.Text = "" Then
                MsgBox("Please Enter the refrence number of the payment")
                txtRef.Focus()
                Exit Sub
            End If
        End If


        If txtCashGiven.Text = "" Or Val(txtCashGiven.Text) = 0 Then
            MsgBox("Please enter the amount given here", MsgBoxStyle.Information)
            txtCashGiven.Focus()

        Else
            rsPayment = New ADODB.Recordset()
            With rsPayment
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPayment ORDER BY PYNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF And .EOF Then
                    lnPYNO = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    lnPYNO = .Fields("PYNo").Value
                End If
                lnPYNO = lnPYNO + 1
                .AddNew()
                .Fields("PYNO").Value = lnPYNO
                .Fields("PNO").Value = lnPNO
                .Fields("BNO").Value = lnBNO
                .Fields("PDATE").Value = Format(Today, "yyyy-MM-d")
                .Fields("TAmt").Value = dbTAmt
                .Fields("AmtP").Value = dbTAmt '
                .Fields("CashGiven").Value = Val(txtCashGiven.Text)
                .Fields("BalGiven").Value = Val(lblCustBal.Content)
                .Fields("Balance").Value = dbBal
                .Fields("UName").Value = strUser

                If optCash.IsChecked = True Then
                    .Fields("PMode").Value = "Cash"
                ElseIf optCheque.IsChecked = True Then
                    .Fields("PMode").Value = "Cheque"
                ElseIf optMpesa.IsChecked = True Then
                    .Fields("PMode").Value = "Mpesa"
                ElseIf optOther.IsChecked = True Then
                    .Fields("PMode").Value = "Other"
                Else
                    .Fields("PMode").Value = "Cash"
                End If
                If txtRef.Text <> "" Then
                    .Fields("PModeRef").Value = txtRef.Text
                Else
                    .Fields("PModeRef").Value = ""
                End If
                .Update()

                UpdateBill()
                updateQueue()
                MsgBox("Record Saved!", MsgBoxStyle.Information)
                LoadBill()
                bnNewRecord = False
                cboBNo.SelectedItem = ""
                btnSave.IsEnabled = False
                btnCancel.IsEnabled = False
                txtCashGiven.IsEnabled = False
                txtRef.IsEnabled = False
            End With
        End If


    End Sub

    Private Sub UpdateBill()
        Dim rsQueue As New ADODB.Recordset
        Dim rsU As New ADODB.Recordset
        Dim Etime As Date
        Dim strP As String = ""
        Dim lnQNO As Integer = 0

        With rsBill
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNO & " ORDER BY BNO DESC, BDate DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .RecordCount > 0 Then
                .Fields("Bal").Value = dbBal
                .Fields("Remarks").Value = "Payment Number: " & lnPYNO
                With rsPatient
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT pno, PatNo, VDate FROM tblPatient WHERE PNO=" & lnPNO & " ORDER BY VDate DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        strP = .Fields("PatNo").Value
                    End If
                    .Close()
                End With
                .Update()
            End If
        End With
        With rsQueue
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblQueue WHERE PatNo='" & strP & "' AND status='Waiting' AND DESTINATION='Reception' ORDER BY qno Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF = True And .EOF = True Then
                .Close()
                Exit Sub
            End If
            If .RecordCount > 0 Then .MoveFirst()
            lnQNO = Val(.Fields("QNO").Value)
            While .EOF = False
                .Fields("Status").Value = "Attended"
                .Fields("ADate").Value = Today
                .Fields("Remarks").Value = "Paid"
                .Fields("ATime").Value = Format(Now, "Long Time")
                rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                .Fields("AttendedBy").Value = strUser & " " & rsU.Fields("Designation").Value
                rsU.Close()
                Etime = System.DateTime.FromOADate(CDate(rsQueue.Fields("QTime").Value).ToOADate - CDate(rsQueue.Fields("ATime").Value).ToOADate)
                .Fields("QTTime").Value = Etime
                .Update()
                .MoveNext()
            End While
            .Close()
        End With
        With rsQueue
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblQueue WHERE PatNo='" & strP & "' AND QNO >" & lnQNO & " AND status='Pending' ORDER BY qno Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF = True And .EOF = True Then
                .Close()
                Exit Sub
            End If
            .Fields("Status").Value = "Waiting"
            .Update()
            .Close()
        End With

    End Sub

    Private Sub txtCashGiven_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtCashGiven.LostFocus

        If txtCashGiven.Text = "" Or Val(txtCashGiven.Text) <= 0 Then
            Exit Sub
        ElseIf IsNumeric(txtCashGiven.Text) = False Then
            MsgBox("Please valid amount given")
            txtCashGiven.SelectAll()
        Else

            dbBal = 0
            lblBalance.Content = "" '
        End If
        If txtCashGiven.Text = "" Or Val(txtCashGiven.Text) = 0 Then Exit Sub

        If Val(txtCashGiven.Text) - Val(lblBAmt.Content) < 0 Then
            lblCustBal.Content = Val(txtCashGiven.Text) - Val(lblBAmt.Content)
            myBrush = New SolidColorBrush(myColors(11))
            lblCustBal.Foreground = myBrush
        Else
            myBrush = New SolidColorBrush(myColors(1))
            lblCustBal.Foreground = myBrush
            lblCustBal.Content = Val(txtCashGiven.Text) - Val(lblBAmt.Content)
        End If

    End Sub


    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
        Dim rptRcpt As New rptRcptNew '
        Dim winRptR As New winRptI

        Dim myLogOnInfo As New TableLogOnInfo()
        Dim myTableLogOnInfos As New TableLogOnInfos
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptRcpt.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
        Dim iPNo As String = CStr(lnPNO)
        Dim BNo As Integer = lnBNO
        Dim PNo As Integer = lnPYNO

        Dim myTables As Tables
        Dim myTable As Table
        Dim myTableLogOnInfo As New TableLogOnInfo

        GetServer()
        btnPrint.IsEnabled = False

        Try 'This piece of code is repeated very many times with minor variations it needs to be DRY'd
            myConnectionInfo.ServerName = rServer
            myConnectionInfo.DatabaseName = rDatabase
            myConnectionInfo.UserID = ""
            myConnectionInfo.Password = ""
            rptRcpt.SetDatabaseLogon("sa", "******", rServer, rDatabase)
            rptRcpt.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "******")
            rptRcpt.DataSourceConnections.Item(0).SetLogon("sa", "******")

            myTables = rptRcpt.Database.Tables
            For Each myTable In myTables
                myTableLogOnInfo = myTable.LogOnInfo
                myTableLogOnInfo.ConnectionInfo = myConnectionInfo
                myTable.ApplyLogOnInfo(myTableLogOnInfo)
            Next

            rptRcpt.RecordSelectionFormula = "{tblPatient.PNo} =" & iPNo & " and {tblBill.BNo} =" & lnBNO & " and {tblPayment.PYNo} =" & lnPYNO & ""
            rptRcpt.Refresh()
            winRptR.crvMain.ViewerCore.ReportSource = rptRcpt
            winRptR.Show()

        Catch ex As Exception
            MsgBox(Err.Description)
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

    Private Sub updateQueue()
        Dim rsQueue As New ADODB.Recordset
        Dim rsPa As New ADODB.Recordset
        Dim rsU As New ADODB.Recordset
        Dim strP As String = ""
        Dim Etime As Date
        Dim lnQNo As Integer = 0

        Try
            With rsPa
                .Open("SELECT * FROM tblPatient WHERE PNO=" & lnPNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                strP = .Fields("PatNo").Value
                .Close()
            End With


            With rsQueue
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblQueue WHERE PatNo=N'" & strP & "' AND status='Waiting' AND DESTINATION='Reception' ORDER BY qno Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    .Close()
                    Exit Sub
                End If
                lnQNo = Val(.Fields("QNO").Value)
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

        With rsQueue
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblQueue WHERE PatNo='" & strP & "' AND QNO>" & lnQNo & "status='Pending' ORDER BY qno Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF = True And .EOF = True Then
                .Close()
                Exit Sub
            End If
            .Fields("Status").Value = "Waiting"
            .Fields("ADate").Value = Today
            .Fields("ATime").Value = Format(Now, "Long Time")
            .Update()
            .Close()
        End With
    End Sub


    Private Sub CreateNewRecord()
        lnPYNO = 0
        rsPayment = New ADODB.Recordset()
        With rsPayment
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblPayment ORDER BY PYNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF And .EOF Then
                lnPYNO = 0
            Else
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                .MoveLast()
                lnPYNO = .Fields("PYNo").Value
            End If
            lnPYNO = lnPYNO + 1
            .Close()
        End With

        btnSave.IsEnabled = True
        btnCancel.IsEnabled = True
        txtCashGiven.Text = ""
        txtRef.Text = ""
        lblToday.Content = Today
        lblPYNo.Content = lnPYNO
        txtCashGiven.IsEnabled = True
        txtRef.IsEnabled = True
        txtCashGiven.Focus()
        bnNewRecord = True

    End Sub

End Class
