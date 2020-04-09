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
Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine

Class pgReports

    Private MainWin As New MainWindow
    Private rServer As String
    Private rDatabase As String
    Private lnLSNo As Long
    Private strDept As String


    Private Sub pgReports_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Try
            dtpEnd.SelectedDate = Today
            dtpStart.SelectedDate = Today
            cboDept.Items.Add("Consultation")
            cboDept.Items.Add("Lab")
            cboDept.Items.Add("Pharmacy")
            cboDept.Items.Add("All")
            optCDate.IsChecked = False
            optODate.IsChecked = False
        Catch ex As Exception
            MsgBox("An error has occured while loading reports page ")
        End Try



    End Sub

    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click

        Try
            If strDept = "" Then
                MsgBox("You have not selected department, please select")
                Exit Sub
            ElseIf strDept = "Consultation" Then
                printConsultation()
            ElseIf strDept = "Lab" Then
                printLab()
            ElseIf strDept = "Pharmacy" Then
                printPharm()

            ElseIf strDept = "All" Then
                printAll()
            End If

        Catch ex As Exception
            MsgBox("An error has occured")
        End Try

    End Sub

    Private Sub GetServer()
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

    Private Sub printConsultation()
        Try
            Dim rpt As New rptConsolt
            Dim winRptR As New winRptI
            Dim myLogOnInfo As New TableLogOnInfo()
            Dim myTableLogOnInfos As New TableLogOnInfos
            Dim myConnectionInfo As New ConnectionInfo()
            Dim myDataSourceConnections As DataSourceConnections = rpt.DataSourceConnections
            Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)

            rpt.Refresh()
            GetServer()
            myConnectionInfo.ServerName = rServer
            myConnectionInfo.DatabaseName = rDatabase
            myConnectionInfo.UserID = "sa"
            myConnectionInfo.Password = "********"
            rpt.SetDatabaseLogon("sa", "********", rServer, rDatabase)
            rpt.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "********")
            rpt.DataSourceConnections.Item(0).SetLogon("sa", "********")

            If optCDate.IsChecked = True Then
                rpt.RecordSelectionFormula = "{tblConsultation.CDate}='" & Format(Today, "yyyy-MM-dd") & "'"
            ElseIf optODate.IsChecked = True Then
                rpt.RecordSelectionFormula = "{tblConsultation.CDate}>='" & Format(dtpStart.SelectedDate, "yyyy-MM-dd") & "' and {tblConsultation.CDate}<='" & Format(dtpEnd.SelectedDate, "yyyy-MM-dd") & "'"
            End If
            rpt.Refresh()
            winRptR.crvMain.ViewerCore.ReportSource = rpt
            winRptR.Show()
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try


    End Sub

    Private Sub printLab()
        Try
            Dim rpt As New rptLabDetails
            Dim winRptR As New winRptI
            Dim myLogOnInfo As New TableLogOnInfo()
            Dim myTableLogOnInfos As New TableLogOnInfos
            Dim myConnectionInfo As New ConnectionInfo()
            Dim myDataSourceConnections As DataSourceConnections = rpt.DataSourceConnections
            Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
            Dim rsLabRep As New ADODB.Recordset
            Dim rsQ As New ADODB.Recordset

            rpt.Refresh()
            GetServer()
            myConnectionInfo.ServerName = rServer
            myConnectionInfo.DatabaseName = rDatabase
            myConnectionInfo.UserID = ""
            myConnectionInfo.Password = ""
            rpt.SetDatabaseLogon("sa", "********", rServer, rDatabase)
            rpt.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "********")
            rpt.DataSourceConnections.Item(0).SetLogon("sa", "********")
            If optCDate.IsChecked = True Then
                rpt.RecordSelectionFormula = "{tblLab.LDate}='" & Format(Today, "yyyy-MM-dd") & "'"
            ElseIf optODate.IsChecked = True Then
                rpt.RecordSelectionFormula = "{tblLab.LDate}>='" & Format(dtpStart.SelectedDate, "yyyy-MM-dd") & "' and {tblLab.LDate}<='" & Format(dtpEnd.SelectedDate, "yyyy-MM-dd") & "'"
            End If

            rpt.Refresh()
            winRptR.crvMain.ViewerCore.ReportSource = rpt
            winRptR.Show()
        Catch ex As Exception
            MsgBox("An error has occured while printing lab details ")
        End Try

    End Sub

    Private Sub printPharm()
        Try

            Dim rpt As New rptPharmDetails
            Dim winRptR As New winRptI
            Dim myLogOnInfo As New TableLogOnInfo()
            Dim myTableLogOnInfos As New TableLogOnInfos
            Dim myConnectionInfo As New ConnectionInfo()
            Dim myDataSourceConnections As DataSourceConnections = rpt.DataSourceConnections
            Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
            Dim rsLabRep As New ADODB.Recordset
            Dim rsQ As New ADODB.Recordset

            rpt.Refresh()
            GetServer()

            myConnectionInfo.ServerName = rServer
            myConnectionInfo.DatabaseName = rDatabase
            myConnectionInfo.UserID = ""
            myConnectionInfo.Password = ""
            rpt.SetDatabaseLogon("sa", "********", rServer, rDatabase)
            rpt.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "********")
            rpt.DataSourceConnections.Item(0).SetLogon("sa", "********")
            If optCDate.IsChecked = True Then
                rpt.RecordSelectionFormula = "{tblPharmacy.DDate}='" & Format(Today, "yyyy-MM-dd") & "'"
            ElseIf optODate.IsChecked = True Then
                rpt.RecordSelectionFormula = "{tblPharmacy.DDate}>='" & Format(dtpStart.SelectedDate, "yyyy-MM-dd") & "' and {tblPharmacy.DDate}<='" & Format(dtpEnd.SelectedDate, "yyyy-MM-dd") & "'"
            End If
            rpt.Refresh()
            winRptR.crvMain.ViewerCore.ReportSource = rpt
            winRptR.Show()
        Catch ex As Exception
            MsgBox("An error has occured while printing pharmacy details ")

        End Try
    End Sub

    Private Sub printAll()
        Try

            Dim rpt As New rptPatients
            Dim winRptR As New winRptI
            Dim myLogOnInfo As New TableLogOnInfo()
            Dim myTableLogOnInfos As New TableLogOnInfos
            Dim myConnectionInfo As New ConnectionInfo()
            Dim myDataSourceConnections As DataSourceConnections = rpt.DataSourceConnections
            Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
            Dim rsLabRep As New ADODB.Recordset
            Dim rsQ As New ADODB.Recordset

            rpt.Refresh()
            GetServer()
            myConnectionInfo.ServerName = rServer
            myConnectionInfo.DatabaseName = rDatabase
            myConnectionInfo.UserID = ""
            myConnectionInfo.Password = ""
            rpt.SetDatabaseLogon("sa", "********", rServer, rDatabase)
            rpt.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "********")
            rpt.DataSourceConnections.Item(0).SetLogon("sa", "********")

            If optCDate.IsChecked = True Then
                rpt.RecordSelectionFormula = "{tblPatient.VDate}='" & Format(Today, "yyyy-MM-dd") & "'"
            ElseIf optODate.IsChecked = True Then
                rpt.RecordSelectionFormula = "{tblPatient.VDate}>='" & Format(dtpStart.SelectedDate, "yyyy-MM-dd") & "' and {tblPatient.VDate}<='" & Format(dtpEnd.SelectedDate, "yyyy-MM-dd") & "'"
            End If

            rpt.Refresh()
            winRptR.crvMain.ViewerCore.ReportSource = rpt
            winRptR.Show()
        Catch ex As Exception
            MsgBox("An error has occured while printing details ")

        End Try
    End Sub

    Private Sub cboDept_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboDept.SelectionChanged
        strDept = cboDept.SelectedItem
    End Sub

    Private Sub dtpStart_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpStart.SelectedDateChanged
     
        If dtpStart.SelectedDate > dtpEnd.SelectedDate Then
            MsgBox("Please select earlier date for starting")
        Else
            optODate.IsChecked = True
        End If
    End Sub

    Private Sub dtpEnd_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpEnd.SelectedDateChanged

        If dtpEnd.SelectedDate < dtpStart.SelectedDate Then
            MsgBox("Please select later date for ending")
        ElseIf dtpEnd.SelectedDate > Today Then
            MsgBox("End date cannot be later than today")
            dtpEnd.SelectedDate = Today
        Else
            optODate.IsChecked = True
        End If
    End Sub

End Class
