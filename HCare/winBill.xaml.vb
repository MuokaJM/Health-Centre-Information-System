Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine
Imports System.Text.RegularExpressions
Public Class winBill
    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
      Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
      Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
      Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}


    Private intTheme As Integer = 0
    Private strTheme As String = ""
    Private strThemeColor As String = ""
    Private intGColor As Integer

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

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Close()
    End Sub

    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click

        Dim rptB As New rptBill '
        Dim winRptR As New winRptI
        Dim myLogOnInfo As New TableLogOnInfo()
        Dim myTableLogOnInfos As New TableLogOnInfos
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptB.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
        Dim iPNo As String

        If txtRNo.Text <> "" Then

            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE BNO=" & Val(txtRNo.Text), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    iPNo = .Fields("PNO").Value
                    lnBNO = .Fields("BNO").Value
                    GetServer()
                    myConnectionInfo.ServerName = rServer
                    myConnectionInfo.DatabaseName = rDatabase
                    myConnectionInfo.UserID = ""
                    myConnectionInfo.Password = ""
                    rptB.SetDatabaseLogon("sa", "********", rServer, rDatabase)
                    rptB.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "********")
                    rptB.DataSourceConnections.Item(0).SetLogon("sa", "********")
                    rptB.RecordSelectionFormula = "{tblPatient.PNo} =" & iPNo & " and {tblBill.BNo} =" & lnBNO & "" ' 
                    rptB.Refresh()
                    winRptR.crvMain.ViewerCore.ReportSource = rptB
                    Topmost = False
                    winRptR.Show()
                    winRptR.Owner = Me


                Else
                    MsgBox("Bill number does not exist", MsgBoxStyle.Exclamation)
                    txtRNo.Focus()
                End If
            End With

        ElseIf txtRNo.Text = "" Then
            MsgBox("Please enter the bill number to print")
            txtRNo.Focus()
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


    Private Sub winPrintRcpt_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        Dim myBrush As New SolidColorBrush
        txtRNo.Focus()

    End Sub



    Private Sub setGlow(intGColor As Integer)
        On Error Resume Next

        Select Case intGColor
            Case 0
            Case 1
            Case 2
            Case 3
            Case 4
            Case 5
            Case 6
            Case 7
            Case 8
            Case 9
            Case 10
            Case 11
            Case 12
            Case 13
            Case 14
            Case 15
            Case 16
            Case 17
            Case 18
            Case 19
            Case Else
        End Select
    End Sub

    Private Sub winBill_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        setGlow(intGColor)
    End Sub
End Class
