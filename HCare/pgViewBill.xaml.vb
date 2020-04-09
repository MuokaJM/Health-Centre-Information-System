Imports ADODB
Imports System.Data
Imports Microsoft.SqlServer
Imports System.Data.OleDb
Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine

Class pgViewBill
    Private MainWin As New MainWindow
    Public rsBill As New ADODB.Recordset()
    Private rsBillDet As New ADODB.Recordset
    Private rsPatient As New ADODB.Recordset
    Private BNO As Integer
    Private lnPNO As Double
    Private BiNo As Integer
    Private dbTAmt As Double
    Private dbAmtP As Double
    Private dbBal As Double
    Private dbPrePaid As Double
    Private rServer As String
    Private rDatabase As String
    Public strUser As String

    Private Sub pgViewBill_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        LoadBills()
    End Sub

    Private Sub cboBillNo_GotFocus(sender As Object, e As RoutedEventArgs) Handles cboBillNo.GotFocus
        Try
            If chkAll.IsChecked = True Then
                LoadAllBills()
            ElseIf chkAll.IsChecked = False Then
                LoadBills()
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading bills")
        End Try

    End Sub


    Private Sub cboBillNo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboBillNo.SelectionChanged

        Dim dtBillDet As New DataTable
        Dim daBillDet As New OleDbDataAdapter

        getBillNumber()
        With rsBill
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblBill WHERE BNO=" & BNO & " ORDER BY BNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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

                If IsDBNull(.Fields("PBNo").Value) = True Then
                    lblPreviousBNo.Content = "-"
                Else
                    lblPreviousBNo.Content = .Fields("PBNo").Value
                End If

                If IsDBNull(.Fields("PBal").Value) = True Then
                    lblPreviousAmt.Content = "-"
                Else
                    lblPreviousAmt.Content = .Fields("PBal").Value
                End If


                lblBAmt.Content = dbTAmt
                lblBalance.Content = dbBal
                dbPrePaid = dbTAmt - dbBal
                If dbPrePaid > 0 Then
                    lblPrePaid.Content = dbTAmt - dbBal
                Else
                    lblPrePaid.Content = 0
                End If

                With rsBillDet
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT BNO, SNO as Serial, BiNo as Item_No, Service, SAMt as Amount, RefNo as Reference FROM tblBillDetails WHERE BNO=" & BNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)

                End With

                daBillDet.Fill(dtBillDet, rsBillDet)
                dgBillDet.ItemsSource = dtBillDet.DefaultView

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

        InvalidateArrange()
        InvalidateVisual()

    End Sub

    Public Function getBillNumber()

        Dim Mchar As String = ""
        Dim cboC As String
        Dim X As Integer
        Dim p As String = ""

        cboC = cboBillNo.SelectedItem
        For X = 1 To Len(cboC)
            Mchar = Mid(cboC, X, 1)
            If Mchar = " " Then Exit For
            p = p + Mchar
        Next X
        BNO = Val(p)

        Return (0)
    End Function

    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
        Dim rptB As New rptBilli
        Dim winRptR As New winRptI
        Dim myLogOnInfo As New TableLogOnInfo()
        Dim myTableLogOnInfos As New TableLogOnInfos
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptB.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
        Dim iPNo As String

        rptB.Refresh()
        If BNO <> 0 Then
            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE BNO=" & BNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    iPNo = .Fields("PNO").Value
                    BNO = .Fields("BNO").Value

                    GetServer()

                    myConnectionInfo.ServerName = rServer
                    myConnectionInfo.DatabaseName = rDatabase
                    myConnectionInfo.UserID = ""
                    myConnectionInfo.Password = ""
                    rptB.SetDatabaseLogon("sa", "********", rServer, rDatabase)
                    rptB.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "********")
                    rptB.DataSourceConnections.Item(0).SetLogon("sa", "********")
                    rptB.RecordSelectionFormula = "{tblBill.BNo} =" & BNO & " and {tblPatient.PNo} =" & iPNo '
                    rptB.Refresh()
                    winRptR.crvMain.ViewerCore.ReportSource = rptB
                    winRptR.Show()

                Else
                    MsgBox("Bill number does not exist", MsgBoxStyle.Exclamation)
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

    Private Sub LoadBills()
        Dim rsQ As New ADODB.Recordset
        Dim nQueue As Integer
        With rsQ
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblBill WHERE Bal>0", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            nQueue = .RecordCount
            .Close()
        End With

        If cboBillNo.Items.Count = nQueue Then Exit Sub

        cboBillNo.Items.Clear()
        Try
            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE Bal>0", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT PNo, Surname, ONames, Sex FROM tblPatient WHERE PNo=" & CInt(rsBill.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                cboBillNo.Items.Add(rsBill.Fields("BNO").Value & " " & .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value))
                            End If
                            .Close()
                        End With
                        .MoveNext()
                    End While
                End If
            End With
        Catch
            MsgBox("An error has occured while loading bills")
        End Try
    End Sub

    Private Sub LoadAllBills()
        Dim rsQ As New ADODB.Recordset
        Dim nQueue As Integer
        With rsQ
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblBill", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            nQueue = .RecordCount
            .Close()
        End With

        If cboBillNo.Items.Count = nQueue Then Exit Sub
        cboBillNo.Items.Clear()
        Try
            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT PNo, Surname, ONames, Sex FROM tblPatient WHERE PNo=" & CInt(rsBill.Fields("PNo").Value), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                cboBillNo.Items.Add(rsBill.Fields("BNO").Value & " " & .Fields("Surname").Value & " " & Trim(.Fields("Onames").Value))
                            End If
                            .Close()
                        End With
                        .MoveNext()
                    End While
                End If
            End With
        Catch
            MsgBox("An error has occured while loading bills")
        End Try
    End Sub

    Private Sub chkAll_Checked(sender As Object, e As RoutedEventArgs) Handles chkAll.Checked
        Try
            If chkAll.IsChecked = True Then
                LoadAllBills()
            ElseIf chkAll.IsChecked = False Then
                LoadBills()
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading bills")
        End Try
    End Sub
End Class
