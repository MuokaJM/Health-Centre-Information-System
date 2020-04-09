﻿

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


Class pgPharmacyReports

    Private MainWin As New MainWindow
    Private rServer As String
    Private rDatabase As String
    Private lnLSNo As Long
    Private strDrugName As String
    Private intLowerYear As Integer
    Private intUpperYear As Integer

    Private rsConsultation As New ADODB.Recordset


    Private Sub pgPharmacyReports_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Dim rsPatient As New ADODB.Recordset
        Try
            dtpEnd.SelectedDate = Today
            dtpStart.SelectedDate = Today
            optCDate.IsChecked = False
            optODate.IsChecked = False
        Catch ex As Exception
            MsgBox("An error has occured while loading reports page ")
        End Try


        Try
            cboDrugName.Items.Clear()
            With rsConsultation
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT DName FROM tblDrugs ORDER BY DName", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        cboDrugName.Items.Add(.Fields("DName").Value)
                        .MoveNext()
                    End While
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading drugs list" & Err.Description)
        End Try

    End Sub



    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click

        Try
            printReport()
        Catch ex As Exception
            MsgBox("An error has occured while printing report")
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


    Private Sub cboDept_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboDrugName.SelectionChanged
        strDrugName = cboDrugName.SelectedItem
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

    Private Sub printReport()
        Try


            Dim rpt As New DispensedDrugsSummary
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

            myLogOnInfo.ConnectionInfo = myConnectionInfo

            If optCDate.IsChecked = True Then
                If Trim(strDrugName) = "" Then

                    If optYears.IsChecked = True Then
                        rpt.RecordSelectionFormula = (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & ">=" & intLowerYear & " and " & (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & "<=" & intUpperYear & " and {tblConsultation.CDate}='" & Format(Today, "yyyy-MM-dd") & "'"
                    Else
                        rpt.RecordSelectionFormula = "{tblDisDrugs.DDate}='" & Format(Today, "yyyy-MM-dd") & "'"

                    End If
                Else
                    If optYears.IsChecked = True Then
                        rpt.RecordSelectionFormula = (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & ">=" & intLowerYear & " and " & (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & "<=" & intUpperYear & " and {tblConsultation.CDate}='" & Format(Today, "yyyy-MM-dd") & "' and {tblConsultation.Impression}='" & strDrugName & "'"
                    Else
                        rpt.RecordSelectionFormula = "{tblDisDrugs.DDate}='" & Format(Today, "yyyy-MM-dd") & "' and {tblDrugs.DName}='" & strDrugName & "'"
                    End If
                End If
            ElseIf optODate.IsChecked = True Then
                If Trim(strDrugName) = "" Then
                    If optYears.IsChecked = True Then
                        rpt.RecordSelectionFormula = (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & ">=" & intLowerYear & " and " & (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & "<=" & intUpperYear & " and {tblConsultation.CDate}>='" & Format(dtpStart.SelectedDate, "yyyy-MM-dd") & "' and {tblConsultation.CDate}<='" & Format(dtpEnd.SelectedDate, "yyyy-MM-dd") & "'"
                    Else
                        rpt.RecordSelectionFormula = "{tblDisDrugs.DDate}>='" & Format(dtpStart.SelectedDate, "yyyy-MM-dd") & "' and {tblDisDrugs.DDate}<='" & Format(dtpEnd.SelectedDate, "yyyy-MM-dd") & "'" ' and {tblConsultation.Impression}='" + strDrugName + "'"
                    End If
                Else
                    If optYears.IsChecked = True Then
                        rpt.RecordSelectionFormula = (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & ">=" & intLowerYear & " and " & (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & "<=" & intUpperYear & " and {tblConsultation.CDate}>='" & Format(dtpStart.SelectedDate, "yyyy-MM-dd") & "' and {tblConsultation.CDate}<='" & Format(dtpEnd.SelectedDate, "yyyy-MM-dd") & "' and {tblConsultation.Impression}='" + strDrugName + "'"
                    Else
                        rpt.RecordSelectionFormula = "{tblDisDrugs.DDate}>='" & Format(dtpStart.SelectedDate, "yyyy-MM-dd") & "' and {tblDisDrugs.DDate}<='" & Format(dtpEnd.SelectedDate, "yyyy-MM-dd") & "' and {tblDrugs.DName}='" + strDrugName + "'"
                    End If
                End If
            Else
                If Trim(strDrugName) = "" Then
                    If optYears.IsChecked = True Then
                        rpt.RecordSelectionFormula = (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & ">=" & intLowerYear & " and " & (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & "<=" & intUpperYear
                    Else

                    End If
                Else
                    If optYears.IsChecked = True Then
                        rpt.RecordSelectionFormula = (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & ">=" & intLowerYear & " and " & (CInt(Format(Today, "yyyy")) + 1 & "-(Year(DateValue({tblPatient.DoB})))") & "<=" & intUpperYear & " and {tblConsultation.Impression}='" + strDrugName + "'"
                    Else
                        rpt.RecordSelectionFormula = "{tblDrugs.Dname}='" + strDrugName + "'"
                    End If
                End If
            End If
            rpt.Refresh()

            winRptR.crvMain.ViewerCore.ReportSource = rpt
            winRptR.Show()
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try


    End Sub

    Private Sub cboSmall_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboSmall.SelectionChanged
        intLowerYear = CInt(cboSmall.SelectedItem)
    End Sub

    Private Sub cboLarge_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboLarge.SelectionChanged
        intUpperYear = CInt(cboLarge.SelectedItem)
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            intLowerYear = vbNull
            intUpperYear = vbNull
            cboDrugName.Text = ""
            cboLarge.Text = ""
            cboSmall.Text = ""
            strDrugName = ""
            optCDate.IsChecked = False
            optODate.IsChecked = False
            optYears.IsChecked = False
            dtpEnd.SelectedDate = Today
            dtpStart.SelectedDate = Today

        Catch ex As Exception

        End Try
    End Sub
End Class
