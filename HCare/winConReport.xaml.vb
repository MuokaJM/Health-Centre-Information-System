Imports ADODB
Imports System.Data
Imports Microsoft.SqlServer
Imports System.Data.OleDb
Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine


Public Class winConReport

    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
      Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
      Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
      Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}


    Private intTheme As Integer = 0
    Private strTheme As String = ""
    Private strThemeColor As String = ""
    Public intGColor As Integer


    Private MainWin As New MainWindow
    Private rServer As String
    Private rDatabase As String
    Private lnLSNo As Long

    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
       Dim rptConRep As New rptConsultation
        Dim winRptR As New winRptI
        Dim myLogOnInfo As New TableLogOnInfo()
        Dim myTableLogOnInfos As New TableLogOnInfos
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptConRep.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
        Dim iPNo As String
        Dim rsLabRep As New ADODB.Recordset
        Dim rsQ As New ADODB.Recordset

        rptConRep.Refresh()
        If Val(txtRNo.Text) <> 0 Then

            With rsLabRep
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblconsultation WHERE CSNO=" & Val(txtRNo.Text), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    iPNo = .Fields("PNO").Value

                    GetServer()

                    myConnectionInfo.ServerName = rServer
                    myConnectionInfo.DatabaseName = rDatabase
                    myConnectionInfo.UserID = ""
                    myConnectionInfo.Password = ""
                    rptConRep.SetDatabaseLogon("sa", "********", rServer, rDatabase)
                    rptConRep.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "********")
                    rptConRep.DataSourceConnections.Item(0).SetLogon("sa", "********")
                    rptConRep.RecordSelectionFormula = "{tblPatient.PNo} =" & iPNo & "" '
                    rptConRep.Refresh()
                    winRptR.crvMain.ViewerCore.ReportSource = rptConRep
                    winRptR.Show()
                    Me.Topmost = False
                Else
                    MsgBox("Report number does not exist", MsgBoxStyle.Exclamation)
                End If
            End With


        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Close()
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
        Topmost = True
    End Sub

    Private Sub winPrintRcpt_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        setGlow(intGColor)
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
End Class

