Imports System.Globalization
Imports System.Data
Imports System.Data.OleDb
Imports System.Text.RegularExpressions
Imports ADODB
Imports System.Windows.Forms
Imports System.Drawing.Icon
Imports System
Imports System.Text
Imports System.Collections
Imports Microsoft.SqlServer
Imports SAPBusinessObjects
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine

Class pgConsultation
    Inherits System.Windows.Controls.Page
    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
     Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
     Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
     Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}

    Private myBrush As New SolidColorBrush
    Public intTheme As Integer
    Private strColor As String

    ' Create the NotifyIcon. 
    Private notifyIcon As New System.Windows.Forms.NotifyIcon()

    Private lngCRec As Long 'current record
    Private CEdit As Boolean
    Public rsConsultation As New ADODB.Recordset
    Public rsPatient As New ADODB.Recordset
    Private rsBill As New ADODB.Recordset
    Private rsBillDet As New ADODB.Recordset
    Private rsLab As New ADODB.Recordset
    Private rsPharm As New ADODB.Recordset
    Private rsDrugs As New ADODB.Recordset
    Private rsLabTests As New ADODB.Recordset
    Private rsImpression As New ADODB.Recordset
    Private dcDrugCost As Decimal
    Public rsQueue As New ADODB.Recordset
    Private curAmt As Double
    Private BNo As Integer
    Private MainWin As New MainWindow
    Private DNo As Integer
    Private TNo As Integer
    Private lnPNO As Integer 'patient number
    Private BDetNo As Long ' bill details number
    Private strLabRequest As String
    Private strPharmRequest As String
    Private CSNO As Long
    Private LSNo As Long
    Private planSno As Integer
    Private prescripno As Integer
    Private lnPSNO As Long
    Public strUser As String
    Private strPName As String
    Private strPatNo As String
    Private lnQNo As Long
    Private rsU As New ADODB.Recordset()
    Private dtPatient As New DataTable
    Private daPatient As New OleDbDataAdapter
    Public dgBrush As New SolidColorBrush
    Private bnEsc As Boolean 'escape flag for treatment formatting
    Private bnNew As Boolean = False 'new record flag
    Private strAge As String
    Private bnClearQueue As Boolean
    Private rsPreviousConsultation As New ADODB.Recordset
    Private dbLabCost As Decimal
    Private dbPharmCost As Decimal
    Private totalCost As Decimal
    Private arrLabDet As New ArrayList
    Private arrPharmDet As New ArrayList
    Private nQueue As Integer ' patients in queue
    Private rServer As String
    Private rDatabase As String
    Private iDQty As Integer 'drug quantity
    Private intDays As Integer
    Private intQNo As Integer '
    Private strPrecrip As String
    Private strPrescrip1 As String
    Private strP As String
    Private strPi As String
    Private strSelectedDrug As String
    Private strSendTo As String 'to check where data has been send to.
    Private strOrg As String 'see if the person is covered by an org

    

    Private Sub pgConsultation_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Try

            lblToday.Content = Format(Today, "dd-MMMM-yy")
            txtComplaint.IsEnabled = False
            txtDDecision.IsEnabled = False
            txtExamination.IsEnabled = False
            txtLabResults.IsEnabled = False
            txtPrescription.IsEnabled = False
            btnCancel.IsEnabled = False
            btnEdit.IsEnabled = False
            btnSave.IsEnabled = False
            chkLab.IsEnabled = False
            chkPharm.IsEnabled = False
            chkRefer.IsEnabled = False
            cboDrug.IsEnabled = False
            cboImpression.IsEnabled = False
            cboLTest.IsEnabled = False
            cboTimes.IsEnabled = False
            txtDays.IsEnabled = False
            txtQty.IsEnabled = False
            txtAllergies.IsEnabled = False
            txtImpression.IsEnabled = False
            dgBrush.Color = myColors(intTheme)
            dgHistory.BorderThickness = New Thickness(1)
            dgHistory.BorderBrush = dgBrush
            dgHistory.AlternatingRowBackground = dgBrush
            txtDetails.Foreground = dgBrush
            getQueue()
        Catch ex As Exception
            MsgBox("An error has occured while intializing consultation form " & Err.Description)
        End Try

        Try
            With rsConsultation
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblConsultation ORDER BY CSNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading consultation data " & Err.Description)
        End Try

        Try
            With rsLabTests
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT LTNO, TName, Description, cost FROM tblLabTests ORDER BY TNAME", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        cboLTest.Items.Add(.Fields("TNAME").Value & " " & .Fields("Description").Value & " Cost: " & .Fields("Cost").Value & " No: " & .Fields("LTNO").Value)
                        .MoveNext()
                    End While
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading lab tests " & Err.Description)
        End Try

        Try
            With rsImpression
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT Impression FROM tblImpression ORDER BY Impression", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        cboImpression.Items.Add(.Fields("Impression").Value)
                        .MoveNext()
                    End While
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading impressions " & Err.Description)
        End Try



        Try
            With rsDrugs
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT DNO, DName, Description, Tradename, quantity FROM tblDrugs ORDER BY DName, DNO ASC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        cboDrug.Items.Add(.Fields("DName").Value & " " & .Fields("Description").Value & " " & .Fields("TradeName").Value & " " & .Fields("Quantity").Value & " " & .Fields("DNO").Value)
                        .MoveNext()
                    End While
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading drugs " & Err.Description)
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

        Try
            stpDetails.Visibility = Windows.Visibility.Collapsed
            stpLResults.Visibility = Windows.Visibility.Collapsed
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub

    Private Sub cboPNo_GotFocus(sender As Object, e As RoutedEventArgs) Handles cboPNo.GotFocus
        Try
            stpDetails.Visibility = Windows.Visibility.Collapsed
            Dim rsQ As New ADODB.Recordset
            With rsQ
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Consultation' AND Status='Waiting'  AND PatNo NOT LIKE 'RF%' ", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                nQueue = .RecordCount
                .Close()
            End With

            If cboPNo.Items.Count = nQueue Then Exit Sub
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

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

    Private Sub cboPNo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboPNo.SelectionChanged
        Try
            stpLResults.Visibility = Windows.Visibility.Hidden
            If bnClearQueue = True Then Exit Sub
            ClearConsultationData()
            lblToday.Content = Today
            lblPNo.Content = CSNO
            strAge = ""
            dgHistory.ItemsSource = ""
            dtPatient.Clear()
            strPName = ""
            lblDrugCost.Content = ""
            lblLabCost.Content = ""
            strSendTo = ""
            strOrg = ""
            lnPNO = 0
            createNewRecord()
            getPatientNumber(cboPNo.SelectedItem)
        Catch ex As Exception
            MsgBox("An error has occured while fetching patient details " & Err.Description)
        End Try

        Try
            With rsPatient
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPatient WHERE PNO=" & lnPNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    GetAge(.Fields("DoB").Value)
                    strPName = .Fields("Surname").Value & " " & .Fields("ONames").Value
                    strPatNo = .Fields("PatNo").Value
                    If IsDBNull(.Fields("Org").Value) = False Then strOrg = .Fields("Org").Value

                    If IsDBNull(.Fields("Allergies").Value) = False Then
                        txtAllergies.Text = .Fields("Allergies").Value
                    Else

                    End If
                    txtDetails.Text = .Fields("Surname").Value & " " & .Fields("ONames").Value & " " & strAge
                End If
                .Close()
            End With

        Catch ex As Exception
            MsgBox("An error has occured while loading patients data " & Err.Description)
        End Try

        Try
            With rsPreviousConsultation
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT PNO, CDate as Date, Complaint, Examination, LabRequest, LabResults, Impression, Decision, DDetails as Details, prescription, uname as Medic FROM tblConsultation WHERE PNO=" & lnPNO & "  ORDER BY CSNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading patients data " & Err.Description)
        End Try

        Try
            daPatient.Fill(dtPatient, rsPreviousConsultation)
            dgHistory.ItemsSource = dtPatient.DefaultView
        Catch ex As Exception
            MsgBox("An error has occured while loading patients' data " & Err.Description)
        End Try

        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblQueue WHERE patNo='" & strPatNo & "' AND Status='Waiting' AND Destination='Consultation' AND SendBy LIKE '%Lab%' ORDER BY QNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    With rsConsultation
                        If .State = 1 Then .Close()
                        .CursorLocation = CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblConsultation WHERE PNo=" & rsQueue.Fields("PNO").Value & " AND Decision LIKE '%Lab%' ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then
                            GetConsultationData()
                            chkLab.IsChecked = False
                            stpLResults.Visibility = Windows.Visibility.Visible
                            txtLResults.Text = rsQueue.Fields("Remarks").Value
                            bnNew = False
                            myBrush = New SolidColorBrush(myColors(12))
                            stpEdit.Background = myBrush
                            stpEdit.Opacity = 50
                        End If
                    End With
                    btnEdit.IsEnabled = True
                    btnSave.IsEnabled = False 'disable save until edit button is clicked
                Else
                    txtComplaint.IsEnabled = True
                    txtComplaint.Focus()
                    myBrush = New SolidColorBrush(myColors(1))
                    stpEdit.Background = myBrush
                    stpEdit.Opacity = 0
                    If bnNew = False Then
                        createNewRecord()
                    Else
                        Exit Sub
                    End If
                End If
                .Close()

            End With

        Catch ex As Exception
            MsgBox("An error has occured while loading queue data " & Err.Description)
        End Try


    End Sub


    Private Sub GetAge(dt As Date)
        Try
            Dim intAge As Integer
            intAge = DateDiff(DateInterval.Year, dt, Today())
            If intAge > 1 Then
                strAge = intAge & " Years"
            Else
                intAge = DateDiff(DateInterval.Month, dt, Today())

                If intAge > 1 Then
                    strAge = intAge & " Months"
                Else
                    intAge = DateDiff(DateInterval.Day, dt, Today())
                    strAge = intAge & " Days"
                End If
            End If
        Catch ex As Exception
            MsgBox("An error has occured while getting patient age " & Err.Description)
        End Try
    End Sub



    Private Sub cboLTest_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboLTest.SelectionChanged
        Try
            TNo = 0
            getTestNumber(cboLTest.SelectedItem) 'use that code to get Test number
            GetLastPlanNo(txtDDecision.Text)
            With rsLabTests
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblLabTests WHERE LTNO=" & TNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    planSno = planSno + 1
                    txtDDecision.Text = txtDDecision.Text & planSno & ". " & .Fields("TName").Value & " (@" & .Fields("cost").Value & ") " & vbCrLf
                    strLabRequest = strLabRequest & planSno & ". " & .Fields("TName").Value & "(" & .Fields("cost").Value & ")"
                    dbLabCost = dbLabCost + Val(.Fields("cost").Value)
                    lblLabCost.Content = dbLabCost
                    chkLab.IsChecked = True
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading lab test data " & Err.Description)
        End Try
    End Sub

    Private Sub cboDrug_LostFocus(sender As Object, e As RoutedEventArgs) Handles cboDrug.LostFocus
        Try
            txtStrength.Text = ""
            txtQty.Text = ""
            txtDays.Text = ""
            chkPharm.IsChecked = True
            'cboDrug.IsEnabled = False
            cboTimes.Text = ""
            cboTimes.Focus()
        Catch ex As Exception
            MsgBox("An error has occured " & Err.Description)
        End Try

    End Sub



    Public Function getPatientNumber(cboC As String)
        Try
            Dim Mchar As String = ""
            Dim X As Integer
            Dim p As String = ""

            For X = 1 To Len(cboC)
                Mchar = Mid(cboC, X, 1)
                If Mchar = " " Then Exit For
                p = p + Mchar
            Next X
            lnPNO = Val(p)
        Catch ex As Exception
            MsgBox("An error has occured while getting patient number " & Err.Description)
        End Try
        Return (0)
    End Function

    Public Function getDrugNumber(cboC As String)
        Try
            Dim Mchar As String = ""
            Dim X As Integer
            Dim p As String = ""
            For X = (Len(cboC) + 1) To 1 Step -1
                Mchar = Mid(cboC, X, 1)
                If Mchar = " " Then Exit For
                p = p + Mchar
            Next X
            p = Mid(cboC, ((Len(cboC) + 1) - (Len(p))), (Len(p)))
            DNo = Val(p)
            With rsDrugs
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDrugs WHERE DNO=" & DNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    prescripno = prescripno + 1
                    txtPrescription.Text = txtPrescription.Text & prescripno & ". " & .Fields("DName").Value & " " '& vbCrLf
                    strPharmRequest = strPharmRequest & prescripno & "." & .Fields("DName").Value & "(" & Val(.Fields("cost").Value) & ")"
                    dcDrugCost = Val(.Fields("cost").Value)
                    lblDrugCost.Content = dbPharmCost

                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while getting drug number " & Err.Description)
        End Try
        Return (0)
    End Function

    Public Function getTestNumber(cboC As String)
        Try
            Dim Mchar As String = ""
            Dim X As Integer
            Dim p As String = ""
            For X = (Len(cboC) + 1) To 1 Step -1
                Mchar = Mid(cboC, X, 1)
                If Mchar = " " Then Exit For
                p = p + Mchar
            Next X
            p = Mid(cboC, ((Len(cboC) + 1) - (Len(p))), (Len(p)))
            TNo = Val(p)
            '   #

        Catch ex As Exception
            MsgBox("An error has occured while getting test number " & Err.Description)
        End Try



        Return (0)
    End Function

    Private Sub createNewRecord()
        Try
            If bnNew = True Then Exit Sub 'check if new record has been created
            CSNO = 0
            dbLabCost = 0
            dbPharmCost = 0

            With rsConsultation
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblconsultation ORDER BY CSNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF And .EOF Then
                    CSNO = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    CSNO = .Fields("CSNo").Value
                End If

                CSNO = CSNO + 1

                btnSave.IsEnabled = True
                btnCancel.IsEnabled = True
                btnEdit.IsEnabled = False

                btnNext.IsEnabled = False
                btnFirst.IsEnabled = False
                btnLast.IsEnabled = False
                btnPrevious.IsEnabled = False

                ClearConsultationData()
                lblToday.Content = Today
                lblPNo.Content = CSNO
                planSno = 0
                prescripno = 0
                txtDDecision.IsEnabled = True
                chkLab.IsEnabled = True
                chkPharm.IsEnabled = True
                chkRefer.IsEnabled = True
                txtExamination.IsEnabled = True
                txtLabResults.IsEnabled = True
                txtPrescription.IsEnabled = True
                txtComplaint.IsEnabled = True
                cboDrug.IsEnabled = True
                cboImpression.IsEnabled = True
                cboLTest.IsEnabled = True
                cboTimes.IsEnabled = True
                txtDays.IsEnabled = True
                txtQty.IsEnabled = True
                txtAllergies.IsEnabled = True
                txtImpression.IsEnabled = True
                bnNew = True
                Me.txtComplaint.Focus()
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating new number " & Err.Description)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Dim strD As String = ""
        Try
            If CEdit = True Then
                If Trim(txtAllergies.Text) <> "" Then
                    updateAllergies()
                End If

                If chkLab.IsChecked = True Then
                    If MsgBox("Do you wish to send the patient to Lab?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                        sendToLab()
                    Else
                        Exit Sub
                    End If
                End If

                If chkPharm.IsChecked = True Then
                    If MsgBox("Do you wish to send the patient to Pharmacy?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                        sendToPharmacy()
                    Else
                        Exit Sub
                    End If
                End If

                If chkNurse.IsChecked = True Then
                    If MsgBox("Do you wish to send the patient to the nurse?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                        sendToNurse()
                    Else
                        Exit Sub
                    End If
                End If

                lblHeader.Content = "Lab Tests" 'rename the header to workout for lab details
                GetLabTestsCost(txtDDecision.Text)
                dbLabCost = totalCost
                lblHeader.Content = "Drugs" 'rename the header to workout for drugs details
                GetLabTestsCost(txtPrescription.Text)
                Try
                    If Trim(strPharmRequest) = "" Then
                        GetLabTestsCost(txtPrescription.Text)
                    End If

                    rsConsultation = New ADODB.Recordset
                    With rsConsultation
                        If .State = 1 Then .Close()
                        .Open("SELECT *FROM tblConsultation WHERE CSNO=" & Val(lblPNo.Content), MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                        .Fields("Examination").Value = txtExamination.Text
                        If chkLab.IsChecked = True Then strD = strD & "Lab,"
                        If chkPharm.IsChecked = True Then strD = strD & "Pharmacy,"
                        If chkNurse.IsChecked = True Then strD = strD & "Nurse,"
                        If chkRefer.IsChecked = True Then strD = strD & "Refer,"
                        .Fields("Decision").Value = .Fields("Decision").Value & strD
                        .Fields("prescription").Value = strPharmRequest
                        .Fields("DRequest").Value = txtPrescription.Text
                        .Fields("Cost").Value = dbLabCost + dbPharmCost
                        .Fields("LCost").Value = dbLabCost
                        .Fields("PCost").Value = dbPharmCost
                        .Fields("QNO").Value = intQNo
                        .Update()
                    End With
                Catch ex As Exception
                    MsgBox("An error has occured while updating consultation data " & Err.Description)
                End Try

                updateQueue()

                MsgBox("Consultation Number " & Me.lblPNo.Content & " Record Saved", MsgBoxStyle.Information, "Save")
                rsConsultation.Close()

                CEdit = False
                intQNo = 0

                rsConsultation = New ADODB.Recordset()
                rsConsultation.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsConsultation.Open("SELECT * FROM tblConsultation", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsConsultation.Move(lngCRec)
                bnNew = False
                btnSave.IsEnabled = False
                cboPNo.IsEnabled = True
                myBrush = New SolidColorBrush(myColors(1))
                stpEdit.Background = myBrush
                stpEdit.Opacity = 100
            Else
                'check that all fields ave been filled

                If Me.txtComplaint.Text = "" Then
                    MsgBox("Please enter the patient's main complaint", MsgBoxStyle.Information)
                    txtComplaint.Focus()
                ElseIf Me.txtExamination.Text = "" Then
                    MsgBox("Please enter the patient's examination", MsgBoxStyle.Information)
                    txtExamination.Focus()
                ElseIf chkLab.IsChecked = False And chkPharm.IsChecked = False And chkRefer.IsChecked = False Then
                    MsgBox("Please select the decision made (lab, pharm or refer)", MsgBoxStyle.Information)
                    chkPharm.Focus()
                ElseIf Me.txtDDecision.Text = "" Then
                    MsgBox("Please select the details for the decision made", MsgBoxStyle.Information)
                    txtDDecision.Focus()
                ElseIf chkPharm.IsChecked = True And (Trim(txtImpression.Text) = "") Then
                    MsgBox("Please enter the impression you made", MsgBoxStyle.Information)
                    txtImpression.Focus()
                ElseIf chkPharm.IsChecked = True And txtPrescription.Text = "" Then
                    MsgBox("Please enter the prescription made or none if no any", MsgBoxStyle.Information)
                    Me.txtPrescription.Focus()

                Else
                    repeatedGroups()

                    With rsConsultation
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblConsultation ORDER BY CSNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .BOF And .EOF Then
                            CSNO = 0
                        Else
                            If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                            .MoveLast()
                            CSNO = .Fields("CSNo").Value
                        End If
                        CSNO = CSNO + 1
                        lblPNo.Content = CSNO
                        .AddNew()
                        SetConsultationData()
                        .Update()
                        PatientBilling()
                        updateQueue()
                        Try
                            If Trim(txtAllergies.Text) <> "" Then
                                updateAllergies()
                            End If
                        Catch ex As Exception
                            MsgBox("An error has occured while updating allergies " & Err.Description, MsgBoxStyle.Critical)
                        End Try
                        planSno = 0
                        prescripno = 0

                        If chkLab.IsChecked = True Then sendToLab()
                        If chkPharm.IsChecked = True Then sendToPharmacy()
                        If chkNurse.IsChecked = True Then sendToNurse()

                        MsgBox("Record Saved!", MsgBoxStyle.Information)

                        bnClearQueue = True
                        getQueue()
                        bnClearQueue = False
                        bnNew = False
                        btnSave.IsEnabled = False
                        btnEdit.IsEnabled = True
                        btnCancel.IsEnabled = False

                        btnNext.IsEnabled = True
                        btnFirst.IsEnabled = True
                        btnLast.IsEnabled = True
                        btnPrevious.IsEnabled = True
                        cboPNo.IsEnabled = True
                        txtComplaint.IsEnabled = False
                        txtDDecision.IsEnabled = False
                        chkLab.IsEnabled = False
                        chkPharm.IsEnabled = False
                        chkNurse.IsEnabled = False
                        chkRefer.IsEnabled = False

                        txtExamination.IsEnabled = False
                        txtLabResults.IsEnabled = False
                        txtPrescription.IsEnabled = False
                        txtPrescription.Text = ""
                        txtQty.Text = ""

                        cboDrug.IsEnabled = False
                        cboImpression.IsEnabled = False
                        cboLTest.IsEnabled = False
                        cboTimes.IsEnabled = False
                        txtDays.IsEnabled = False
                        txtQty.IsEnabled = False
                        txtAllergies.IsEnabled = False
                        txtImpression.IsEnabled = False
                        ClearConsultationData()

                        lblDrugCost.Content = ""
                        lblLabCost.Content = ""
                        cboDrug.Text = ""
                        cboLTest.Text = ""
                        strOrg = ""
                        stpDetails.Visibility = Windows.Visibility.Collapsed
                        stpLResults.Visibility = Windows.Visibility.Collapsed

                    End With
                End If
            End If

        Catch ex As Exception
            MsgBox("An error has occured while saving consultation details " & Err.Description, MsgBoxStyle.Critical)
        End Try

    End Sub

    Private Sub updateAllergies()
        Try
            Dim rsPat As New ADODB.Recordset
            With rsPat
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT pno, patno, allergies FROM tblPatient WHERE PatNo='" & strPatNo & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                .Fields("Allergies").Value = Trim(txtAllergies.Text)
                .Update()
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while updating allergies " & Err.Description, MsgBoxStyle.Critical)
        End Try

    End Sub


    Private Function SetConsultationData()
        Dim trimChars As Char() = {" ", ChrW(13), ChrW(10), ChrW(13), vbCrLf}
        txtImpression.Text = txtImpression.Text.Trim(trimChars)
        Dim strD As String = ""
        Try
            With rsConsultation
                .Fields("CSNo").Value = Val(lblPNo.Content)
                With rsPatient
                    If .State = 1 Then .Close()
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT pno, patno  FROM tblPatient WHERE PatNo='" & strPatNo & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                    rsConsultation.Fields("PNo").Value = .Fields("PNo").Value
                    lnPNO = .Fields("PNo").Value
                    .Close()
                End With
                .Fields("Complaint").Value = txtComplaint.Text
                .Fields("Examination").Value = txtExamination.Text
                .Fields("CDate").Value = Today
                If chkLab.IsChecked = True Then strD = strD & "Lab,"
                If chkPharm.IsChecked = True Then strD = strD & "Pharmacy,"
                If chkRefer.IsChecked = True Then strD = strD & "Refer,"
                .Fields("Decision").Value = strD
                .Fields("DDetails").Value = txtDDecision.Text
                .Fields("prescription").Value = strPharmRequest
                .Fields("DRequest").Value = txtPrescription.Text
                .Fields("Cost").Value = dbPharmCost + dbLabCost
                .Fields("LCost").Value = dbLabCost
                .Fields("PCost").Value = dbPharmCost
                .Fields("LabRequest").Value = txtDDecision.Text
                .Fields("LabResults").Value = txtLabResults.Text
                .Fields("Impression").Value = txtImpression.Text
                .Fields("UName").Value = strUser
            End With
            btnEdit.IsEnabled = False
        Catch ex As Exception
            MsgBox("An error has occured while setting consultation data for saving " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Function ClearConsultationData()
        Try
            lblPNo.Content = ""
            txtComplaint.Text = ""
            txtExamination.Text = ""
            txtImpression.Text = ""
            txtDDecision.Text = ""
            cboPNo.Text = ""
            cboImpression.Text = ""
            cboLTest.Text = ""
            cboTimes.Text = ""
            cboDrug.Text = ""

            txtDetails.Text = ""
            dgHistory.ItemsSource = ""
            txtComplaint.Text = ""
            txtDDecision.Text = ""
            txtExamination.Text = ""
            txtLResults.Text = ""
            chkLab.IsChecked = False
            chkPharm.IsChecked = False
            chkRefer.IsChecked = False
            chkNurse.IsChecked = False
            txtLabResults.Text = ""
            txtPrescription.Text = ""
            btnEdit.IsEnabled = False
            txtAllergies.Text = ""
            txtQty.Text = ""
            txtDrugQuantity.Text = ""
        Catch ex As Exception
            MsgBox("An error has occured while clearing consultation data " & Err.Description)
        End Try
        Return (0)
    End Function

    Private Function GetConsultationData()
        ClearConsultationData()
        dgHistory.ItemsSource = ""
        dtPatient.Clear()
        Try
            With rsConsultation
                CSNO = .Fields("CSNo").Value
                lblPNo.Content = .Fields("CSNo").Value
                txtComplaint.Text = .Fields("Complaint").Value
                txtExamination.Text = .Fields("Examination").Value
                getDecision(.Fields("Decision").Value)
                txtDDecision.Text = .Fields("DDetails").Value
                txtLabResults.Text = .Fields("LabResults").Value
                If IsDBNull(.Fields("DRequest").Value) = True Then
                Else
                    txtPrescription.Text = .Fields("DRequest").Value
                End If
                txtImpression.Text = .Fields("Impression").Value


                With rsPatient
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblPatient WHERE PNO=" & rsConsultation.Fields("PNo").Value, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        GetAge(.Fields("DoB").Value)
                        strPName = .Fields("Surname").Value & " " & .Fields("ONames").Value
                        strPatNo = .Fields("PatNo").Value
                        If IsDBNull(.Fields("Allergies").Value) = False Then
                            txtAllergies.Text = .Fields("Allergies").Value
                        Else
                        End If
                        txtDetails.Text = .Fields("Surname").Value & " " & .Fields("ONames").Value & " " & strAge
                    End If
                    .Close()
                End With

                With rsPreviousConsultation

                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT PNO, CDate as Date, Complaint, Examination, LabRequest, LabResults, Impression, Decision, DDetails as Details, prescription, uname as Medic FROM tblConsultation WHERE PNO=" & rsConsultation.Fields("PNo").Value & "  ORDER BY CSNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then

                    End If

                End With
                daPatient.Fill(dtPatient, rsPreviousConsultation)

                dgHistory.ItemsSource = dtPatient.DefaultView

            End With
            btnEdit.IsEnabled = True
        Catch ex As Exception

            MsgBox("An error has occured while getting consultation data " & Err.Description)



        End Try

        Return (0)
    End Function

    Private Sub PatientBilling()
        Dim rsBill As New ADODB.Recordset
        Dim rsBillDet As New ADODB.Recordset
        Dim dbBamt As Double 'bill amount
        Dim dbBal As Double 'current bill balance
        Dim dbPBal As Double 'previous balance
        Dim intPBNo As Integer 'previous bill number
        Dim dbTAmt As Double ' as total amoount
        Dim BiNo As Integer 'bill details entry
        Dim dbPCost As Decimal = 0 'drugs cost
        Dim dbLCost As Decimal = 0 'Lab cost
        Dim TrDate As DateTime = DateTime.Today ' .AddDays(1) 'to come to day`s midnight
        Dim iCnt As Integer

        Try
            With rsBill
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNO & " AND BDate='" & Format(Now, "yyyy-MM-dd").ToString & "' AND BAmt=Bal ORDER BY BDate DESC, BNO Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then '
                    dbBamt = .Fields("BAmt").Value
                    dbBal = .Fields("Bal").Value
                    dbPBal = .Fields("PBal").Value
                    dbTAmt = .Fields("TAmt").Value
                    intPBNo = .Fields("PBNo").Value
                    BNo = .Fields("BNo").Value
                    .Fields("uName").Value = strUser
                    .Fields("BAmt").Value = dbBamt + dbPharmCost + dbLabCost
                    .Fields("TAmt").Value = dbTAmt + dbPharmCost + dbLabCost
                    .Fields("Bal").Value = dbBal + dbPharmCost + dbLabCost
                    .Update()

                    .Close()
                    If chkLab.IsChecked = True Then
                        GenerateBillDetNo()
                        Try
                            While dbLCost = 0
                                If iCnt >= 2 Then Exit While
                                dbLCost = InputBox("Confirm the test(s) cost ", "Consultation", dbLabCost)
                                If IsNumeric(dbLCost) = False Then
                                    MsgBox("Invalid cost. Enter details again", MsgBoxStyle.Exclamation)
                                    dbLCost = 0
                                Else
                                    dbLabCost = dbLCost
                                    Exit While
                                End If
                                iCnt = iCnt + 1
                            End While
                        Catch ex As Exception
                            MsgBox(Err.Description)
                        End Try
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
                            .Fields("PNo").Value = lnPNO
                            .Fields("BNo").Value = BNo
                            .Fields("BiNo").Value = BiNo
                            .Fields("SAmt").Value = dbLabCost
                            .Fields("Service").Value = "Lab Charges"
                            .Fields("RefNo").Value = "Consultation Number " & lblPNo.Content
                            .Update()
                            .Close()
                        End With
                    End If

                    iCnt = 1
                    If chkPharm.IsChecked = True Then
                        GenerateBillDetNo()
                        Try
                            While dbPCost = 0
                                If iCnt >= 2 Then Exit While
                                dbPCost = InputBox("Confirm the drug(s) cost ", "Consultation", dbPharmCost)
                                If IsNumeric(dbPCost) = False Then
                                    MsgBox("Invalid cost. Enter details again", MsgBoxStyle.Exclamation)
                                    dbPCost = 0
                                Else
                                    dbPharmCost = dbPCost
                                    Exit While
                                End If
                                iCnt = iCnt + 1
                            End While
                        Catch ex As Exception
                            MsgBox(Err.Description)
                        End Try

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
                            .Fields("PNo").Value = lnPNO
                            .Fields("BNo").Value = BNo
                            .Fields("BiNo").Value = BiNo
                            .Fields("SAmt").Value = dbPharmCost
                            .Fields("Service").Value = "Drugs cost"
                            .Fields("RefNo").Value = "Consultation Number " & lblPNo.Content
                            .Update()
                            .Close()
                        End With
                    End If

                Else
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNO & " ORDER BY BNO DESC, BDate DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        GenerateBillNo()
                        dbBamt = Val(.Fields("Bal").Value)
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
                        .Fields("PNo").Value = lnPNO
                        .Fields("BNo").Value = BNo
                        .Fields("BDate").Value = Today
                        .Fields("BAmt").Value = dbPharmCost + dbLabCost
                        .Fields("PBNO").Value = intPBNo
                        .Fields("PBal").Value = dbPBal
                        .Fields("TAmt").Value = dbPharmCost + dbLabCost + dbPBal
                        .Fields("Bal").Value = dbPharmCost + dbLabCost + dbPBal
                        .Fields("UName").Value = strUser
                        .Fields("Remarks").Value = "Pending"
                        .Update()
                        .Close()
                        If chkLab.IsChecked = True Then
                            GenerateBillDetNo()
                            Try
                                While dbLCost = 0
                                    If iCnt >= 2 Then Exit While
                                    dbLCost = InputBox("Confirm the test(s) cost ", "Consultation", dbLabCost)
                                    If IsNumeric(dbLCost) = False Then
                                        MsgBox("Invalid cost. Enter details again", MsgBoxStyle.Exclamation)
                                        dbLCost = 0
                                    Else
                                        dbLabCost = dbLCost
                                        Exit While
                                    End If
                                    iCnt = iCnt + 1
                                End While
                            Catch ex As Exception
                                MsgBox(Err.Description)
                            End Try
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
                                .Fields("PNo").Value = lnPNO
                                .Fields("BNo").Value = BNo
                                .Fields("BiNo").Value = BiNo
                                .Fields("SAmt").Value = dbLabCost
                                .Fields("Service").Value = "Lab Charges"
                                .Fields("RefNo").Value = "Consultation Number " & lblPNo.Content
                                .Update()
                                .Close()
                            End With
                        End If

                        iCnt = 1
                        If chkPharm.IsChecked = True Then
                            GenerateBillDetNo()
                            Try
                                While dbPCost = 0
                                    If iCnt >= 2 Then Exit While
                                    dbPCost = InputBox("Confirm the drug(s) cost ", "Consultation", dbPharmCost)
                                    If IsNumeric(dbPCost) = False Then
                                        MsgBox("Invalid cost. Enter details again", MsgBoxStyle.Exclamation)
                                        dbPCost = 0
                                    Else
                                        dbPharmCost = dbPCost
                                        Exit While
                                    End If
                                    iCnt = iCnt + 1
                                End While
                            Catch ex As Exception
                                MsgBox(Err.Description)
                            End Try

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
                                .Fields("PNo").Value = lnPNO
                                .Fields("BNo").Value = BNo
                                .Fields("BiNo").Value = BiNo
                                .Fields("SAmt").Value = dbPharmCost
                                .Fields("Service").Value = "Drugs cost"
                                .Fields("RefNo").Value = "Consultation Number " & lblPNo.Content
                                .Update()
                                .Close()
                            End With
                        End If
                    Else
                        GenerateBillNo()
                        .AddNew()
                        .Fields("PNo").Value = lnPNO
                        .Fields("BNo").Value = BNo
                        .Fields("BDate").Value = Today
                        .Fields("BAmt").Value = dbPharmCost + dbLabCost
                        .Fields("PBNO").Value = 0
                        .Fields("PBal").Value = 0
                        .Fields("TAmt").Value = dbPharmCost + dbLabCost
                        .Fields("Bal").Value = dbPharmCost + dbLabCost
                        .Fields("UName").Value = strUser
                        .Fields("Remarks").Value = "Pending"
                        .Update()
                        .Close()
                        If chkLab.IsChecked = True Then
                            GenerateBillDetNo()
                            Try
                                While dbLCost = 0
                                    If iCnt >= 2 Then Exit While
                                    dbLCost = InputBox("Confirm the test(s) cost ", "Consultation", dbLabCost)
                                    If IsNumeric(dbLCost) = False Then
                                        MsgBox("Invalid cost. Enter details again", MsgBoxStyle.Exclamation)
                                        dbLCost = 0
                                    Else
                                        dbLabCost = dbLCost
                                        Exit While
                                    End If
                                    iCnt = iCnt + 1
                                End While
                            Catch ex As Exception
                                MsgBox(Err.Description)
                            End Try
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
                                .Fields("PNo").Value = lnPNO
                                .Fields("BNo").Value = BNo
                                .Fields("BiNo").Value = BiNo
                                .Fields("SAmt").Value = dbLabCost
                                .Fields("Service").Value = "Lab Charges"
                                .Fields("RefNo").Value = "Consultation Number " & lblPNo.Content
                                .Update()
                                .Close()
                            End With
                        End If

                        iCnt = 1
                        If chkPharm.IsChecked = True Then
                            GenerateBillDetNo()
                            Try
                                While dbPCost = 0
                                    If iCnt >= 2 Then Exit While
                                    dbPCost = InputBox("Confirm the drug(s) cost ", "Consultation", dbPharmCost)
                                    If IsNumeric(dbPCost) = False Then
                                        MsgBox("Invalid cost. Enter details again", MsgBoxStyle.Exclamation)
                                        dbPCost = 0
                                    Else
                                        dbPharmCost = dbPCost
                                        Exit While
                                    End If
                                    iCnt = iCnt + 1
                                End While
                            Catch ex As Exception
                                MsgBox(Err.Description)
                            End Try

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
                                .Fields("PNo").Value = lnPNO
                                .Fields("BNo").Value = BNo
                                .Fields("BiNo").Value = BiNo
                                .Fields("SAmt").Value = dbPharmCost
                                .Fields("Service").Value = "Drugs cost"
                                .Fields("RefNo").Value = "Consultation Number " & lblPNo.Content
                                .Update()
                                .Close()
                            End With
                        End If
                    End If
                End If


            End With
        Catch ex As Exception
            MsgBox("An error has occured during customer billing" & Err.Description)
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
            MsgBox("An error has occured while generating bill number " & Err.Description)
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
            MsgBox("An error has occured while generating bill details number " & Err.Description)
        End Try
    End Sub


    Private Sub GenerateLabServiceNo()
        Try
            With rsLab
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblLab ORDER BY LSNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .BOF = True And .EOF = True Then
                    LSNo = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    LSNo = .Fields("LSNo").Value
                End If
                LSNo = LSNo + 1
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating lab service number " & Err.Description)
        End Try
    End Sub

    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs) Handles btnFirst.Click
        Try
            With rsConsultation
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                bnNew = False
                                GetConsultationData()
                            End If
                        Else
                            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveFirst()
                        btnPrevious.IsEnabled = False
                        btnNext.IsEnabled = True
                        GetConsultationData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while moving to first record " & Err.Description)
        End Try
    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click
        Try
            With rsConsultation
                If .RecordCount <> 0 Then
                    If .BOF = True Or .EOF = True Then
                        .MoveFirst()
                        MsgBox("This Is the first Record", MsgBoxStyle.Information, "Navigation")
                        Exit Sub
                    End If
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                bnNew = False
                                GetConsultationData()
                            Else
                                .CancelUpdate()
                                .MovePrevious()
                                btnNext.IsEnabled = True
                                bnNew = False
                                GetConsultationData()
                            End If
                        Else
                            MsgBox("Can't Go To previous Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else

                        If .AbsolutePosition = 1 Then
                            .MoveFirst()
                            btnPrevious.IsEnabled = False
                            MsgBox("This Is the first Record", MsgBoxStyle.Information, "Navigation")
                        ElseIf .BOF = True Then
                            .MoveFirst()
                            btnPrevious.IsEnabled = False
                            MsgBox("This Is the first Record", MsgBoxStyle.Information, "Navigation")
                        Else
                            .MovePrevious()
                        End If
                        btnNext.IsEnabled = True
                        GetConsultationData()
                    End If
                End If
            End With

        Catch ex As Exception
            MsgBox("An error has occured while moving to previous record " & Err.Description)
        End Try

    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        Try
            With rsConsultation
                If .RecordCount <> 0 Then
                    If .EOF = True Or .BOF = True Then
                        .MoveLast()
                        MsgBox("This Is the Last Record", MsgBoxStyle.Information, "Navigation")
                        Exit Sub
                    End If
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnNext.IsEnabled = False
                                bnNew = False
                                GetConsultationData()
                            Else
                                .CancelUpdate()
                                .MoveNext()
                                btnPrevious.IsEnabled = True
                                '  btnNew.IsEnabled = True
                                bnNew = False
                                GetConsultationData()
                            End If
                        Else
                            MsgBox("Can't Go To Next Record!", MsgBoxStyle.Exclamation, "Navigation")

                        End If
                    Else
                        If .RecordCount = .AbsolutePosition Then
                            .MoveLast()
                            btnNext.IsEnabled = False
                            MsgBox("This Is the Last Record", MsgBoxStyle.Information, "Navigation")
                        ElseIf .EOF = True Then
                            .MoveLast()
                            btnNext.IsEnabled = False
                            MsgBox("This Is the Last Record", MsgBoxStyle.Information, "Navigation")
                        Else
                            .MoveNext()
                        End If
                        btnPrevious.IsEnabled = True
                        GetConsultationData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while moving to next record " & Err.Description)
        End Try
    End Sub

    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click
        Try
            With rsConsultation
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnPrevious.IsEnabled = False
                                bnNew = False
                                ' btnNew.IsEnabled = True
                                GetConsultationData()
                            End If
                        Else
                            MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")

                        End If
                    Else
                        .MoveLast()
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = False
                        GetConsultationData()
                    End If
                End If
            End With

        Catch ex As Exception
            MsgBox("An error has occured while moving to last record " & Err.Description)
        End Try
    End Sub


    Private Sub GeneratePharmServiceNo()
        Try
            With rsLab
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblPharmacy ORDER BY PSNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .BOF = True And .EOF = True Then
                    lnPSNO = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    lnPSNO = .Fields("PSNo").Value
                End If
                lnPSNO = lnPSNO + 1
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while generating pharmacy number " & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        Dim rsP As New ADODB.Recordset
        Dim rsB As New ADODB.Recordset
        Dim rsBill As New ADODB.Recordset
        Dim rsPayment As New ADODB.Recordset
        Dim rsConsult As New ADODB.Recordset
        Dim rsQueueEdit As New ADODB.Recordset
        Dim intBno As Integer
        Dim intPNo As Integer


        If CSNO = 0 Then
            MsgBox("No Consultation number selected to edit! click previous or next to locate the record to edit")
            Exit Sub
        End If

        With rsConsult
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblConsultation WHERE CSNo=" & CSNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .RecordCount > 0 Then
                intPNo = .Fields("PNo").Value
                With rsBill
                    If .State = 1 Then .Close()
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblBill WHERE PNo=" & intPNo & " ORDER BY BNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        .MoveFirst()
                        intBno = .Fields("Bno").Value
                        With rsPayment
                            If .State = 1 Then .Close()
                            .CursorLocation = CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblPayment WHERE BNo=" & intBno & " ORDER BY PyNo DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .RecordCount > 0 Then
                                With rsQueueEdit
                                    If .State = 1 Then .Close()
                                    .CursorLocation = CursorLocationEnum.adUseClient
                                    .Open("SELECT * FROM tblqueue WHERE PNo=" & intPNo & "AND Destination='Consultation' AND status='Waiting'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                    If .RecordCount > 0 Then

                                    Else
                                        MsgBox("This entry cannot be edited")
                                        .Close()
                                        rsPayment.Close()
                                        rsBill.Close()
                                        rsConsult.Close()
                                        Exit Sub
                                    End If
                                    .Close()
                                End With
                            End If
                            .Close()
                        End With
                    End If
                    .Close()
                End With
            End If
            .Close()
        End With


        Try
            If rsConsultation.State = 1 Then lngCRec = rsConsultation.AbsolutePosition
            With rsConsultation
                If .State = 1 Then .Close()
                rsConsultation.Open("SELECT * FROM tblConsultation WHERE CSNo=" & CSNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                lnPNO = rsConsultation.Fields("PNO").Value
                If IsDBNull(rsConsultation.Fields("QNO").Value) = False Then intQNo = rsConsultation.Fields("QNO").Value
                rsP.Open("SELECT PNO, Patno FROM tblPatient WHERE pno=" & lnPNO, MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                strPatNo = rsP.Fields("PatNo").Value
                rsP.Close()
                With rsB
                    If .State = 1 Then .Close()
                    .CursorLocation = CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNO & "  ORDER BY BDate DESC, BNO Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        BNo = .Fields("BNO").Value
                        totalCost = Val(.Fields("Bal").Value)
                    End If
                    .Close()
                End With

                CEdit = True
                btnSave.IsEnabled = True
                btnCancel.IsEnabled = True
                getPlanSno(txtDDecision.Text)
                getPrescSno(txtPrescription.Text)
                cboPNo.IsEnabled = False
                txtDDecision.IsEnabled = True
                chkLab.IsEnabled = True
                chkPharm.IsEnabled = True
                chkRefer.IsEnabled = True
                txtExamination.IsEnabled = True
                txtLabResults.IsEnabled = True
                txtPrescription.IsEnabled = True
                txtComplaint.IsEnabled = True
                Me.txtComplaint.Focus()
                '  End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while preparing for editing " & Err.Description, MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub getPlanSno(s As String)
        Try

            Dim Mchar As String = ""
            Dim pChar As String = ""
            Dim X As Integer
            Dim p As String = ""
            If Len(Trim(s)) = 0 Then Exit Sub
            For X = (Len(s) + 1) To 1 Step -1
                Mchar = Mid(s, X, 1)
                If X > 1 Then
                    pChar = Mid(s, X - 1, 2)
                Else
                    pChar = Mchar
                End If

                If pChar = ". " Then
                    Mchar = Mid(s, X - 2, 1)
                    If IsNumeric(Mchar) = True Then
                        planSno = Mchar
                        Exit Sub
                    End If
                End If
            Next X

        Catch ex As Exception
            MsgBox("An error has occured while numbering entries " & Err.Description)
        End Try
    End Sub

    Private Sub getPrescSno(s As String)
        Try
            Dim Mchar As String = ""
            Dim pChar As String = ""
            Dim X As Integer
            Dim p As String = ""

            If Len(Trim(s)) = 0 Then Exit Sub
            For X = (Len(s) + 1) To 1 Step -1
                Mchar = Mid(s, X, 1)
                If X > 1 Then
                    pChar = Mid(s, X - 1, 2)
                Else
                    pChar = Mchar
                End If

                If pChar = ". " Then
                    Mchar = Mid(s, X - 2, 1)
                    If IsNumeric(Mchar) = True Then
                        prescripno = Mchar
                        Exit Sub
                    End If
                End If
            Next X

        Catch ex As Exception
            MsgBox("An error has occured while getting prescription number " & Err.Description)
        End Try

    End Sub

    Private Sub txtExamination_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtExamination.GotFocus
        Try
            stpDetails.Visibility = Windows.Visibility.Collapsed
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub

    Private Sub txtExamination_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtExamination.TextChanged
        Try
            If bnEsc = False Then
                formatExamination()
            Else 'reset the flag to allow more testing
                bnEsc = False
            End If
            txtExamination.SelectionStart = Len(txtExamination.Text)
        Catch ex As Exception
            MsgBox("An error has occured while formatting examination details " & Err.Description)
        End Try
    End Sub

    Public Function formatExamination()

        Dim Mchar As String = ""
        Dim pChar As String = ""
        Dim cboC As String = ""
        Dim X As Integer
        Dim i As Integer
        Dim p As String = ""
        Try
            'avoid testing empty text box
            If Trim(txtExamination.Text) = "" Then
                Return 0
                Exit Function
            End If

            bnEsc = False
            cboC = txtExamination.Text
            X = Len(cboC)
            Mchar = Mid(cboC, X, 1)

            If Len(cboC) > 1 Then
                pChar = Mid(cboC, X - 1, 2)
            Else
                pChar = Mchar
            End If


            If Mchar = "+" Then
                Mchar = ChrW(&H207A)
                cboC = cboC.Remove(X - 1, 1)
                cboC = cboC & Mchar
            ElseIf Mchar = "0" Then
                Mchar = ChrW(&H2070)
                cboC = cboC.Remove(X - 1, 1)
                cboC = cboC & Mchar
            ElseIf Mchar = "2" Then
                Mchar = ChrW(&H2082)
                cboC = cboC.Remove(X - 1, 1)
                cboC = cboC & Mchar
            End If

            If pChar = ChrW(&H2070) & "0" Then
                Mchar = "0"
                i = X - 2
                cboC = cboC.Remove(i, 2)
                cboC = cboC & Mchar
                bnEsc = True
            End If

            If pChar = ChrW(&H2082) & "2" Then
                Mchar = "2"
                i = X - 2
                cboC = cboC.Remove(i, 2)
                cboC = cboC & Mchar
                bnEsc = True
            End If

            txtExamination.Text = cboC


            Return 0
            Exit Function
        Catch ex As Exception
            MsgBox("An error has occured while formatting examination details " & Err.Description, MsgBoxStyle.Critical)
        End Try

        Return (0)
    End Function

    Private Sub getDecision(strField As String)
        Dim X As Integer
        Dim strCn As String
        Dim N As Integer
        Dim iCode As String = ""
        Dim pCode As String = ""
        Dim ieCount As Integer
        Dim iCurLoc As Integer = 0
        Try
            strCn = strField
            N = 0
            X = 1
            While X < Len(strCn)
                ieCount = X
                For X = ieCount To Len(strCn) Step 1
                    'check for comma,
                    iCode = Mid(strCn, X, 1)
                    If iCode Like "," = True Then
                        If pCode = "Lab" Then
                            pCode = ""
                            iCode = ""
                            chkLab.IsChecked = True
                        ElseIf pCode = "Pharmacy" Then
                            pCode = ""
                            iCode = ""
                            chkPharm.IsChecked = True
                        ElseIf pCode = "Refer" Then
                            pCode = ""
                            iCode = ""
                            chkRefer.IsChecked = True
                        Else
                            ' X = X + 1
                            Exit For
                        End If
                    End If
                    N = N + 1
                    pCode = pCode & iCode
                Next X
            End While
        Catch ex As Exception
            MsgBox("An error has occured while getting decision details " & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub updateQueue()
        Dim rsQueueUpdate As New ADODB.Recordset
        Dim rsPa As New ADODB.Recordset
        Dim rsU As New ADODB.Recordset
        Dim strP As String
        Dim Etime As Date
        Try
            With rsPa
                .Open("SELECT * FROM tblPatient WHERE PNO=" & lnPNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                strP = .Fields("PatNo").Value
                .Close()
            End With


            With rsQueueUpdate
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblQueue WHERE PatNo='" & strP & "' AND status='Waiting' AND DESTINATION='Consultation' ORDER BY qno Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    .Close()
                    Exit Sub
                End If
                .Fields("Status").Value = "Attended"
                .Fields("ADate").Value = Today
                .Fields("ATime").Value = Format(Now, "Long Time")
                rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                .Fields("AttendedBy").Value = strUser & " " & rsU.Fields("Designation").Value
                rsU.Close()
                Etime = System.DateTime.FromOADate(CDate(.Fields("QTime").Value).ToOADate - CDate(.Fields("ATime").Value).ToOADate)
                .Fields("QTTime").Value = Etime
                .Update()
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while updating queue details " & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub


    Private Sub LoadAllPatients()

        Dim rsPatient1 As New ADODB.Recordset()

        'cboPNo.Items.Clear()
        Try
            cboPNo.Items.Clear()
            With rsPatient1
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT PNO, Surname, Onames, Sex, PatNo FROM tblPatient WHERE PatNo LIKE 'OP%' ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        cboPNo.Items.Add(.Fields("PNO").Value & " " & .Fields("Surname").Value & " " & .Fields("Onames").Value)
                        .MoveNext()
                    End While
                End If
                .Close()
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading patients data A" & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub LoadScheduledPatients()
        cboPNo.Items.Clear()
        Dim rsQueue1 As New ADODB.Recordset()
        Try
            With rsQueue1
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Consultation' AND Status='Waiting'  AND PatNo NOT LIKE 'RF%' ", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = CursorLocationEnum.adUseClient
                            .Open("SELECT PNO, Surname, Onames, Sex, PatNo FROM tblPatient WHERE PatNo ='" & rsQueue1.Fields("PatNo").Value & "' ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                While .EOF = False
                                    cboPNo.Items.Add(.Fields("PNO").Value & " " & .Fields("Surname").Value & " " & .Fields("Onames").Value)
                                    .MoveNext()
                                End While
                            End If
                            .Close()
                        End With
                        .MoveNext()
                    End While
                End If
                .Close()
                chkAll.Content = "Scheduled Patients"
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading queued patients details S" & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub


    Private Sub chkAll_Click(sender As Object, e As RoutedEventArgs) Handles chkAll.Click
        Try
            If chkAll.IsChecked = True Then
                'LoadAllPatients()
                chkAll.Content = "Load scheduled patients"
            Else

                LoadScheduledPatients()
                chkAll.Content = "Scheduled Patients"
            End If
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub


    Private Sub getQueue()
        Try
            cboPNo.Items.Clear()
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='Consultation' AND Status='Waiting' AND PatNo NOT LIKE 'RF%' ", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveFirst()
                    While .EOF = False
                        With rsPatient
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT PNO, Surname, Onames, Sex, PatNo FROM tblPatient WHERE PatNo ='" & rsQueue.Fields("PatNo").Value & "' ORDER BY PNO DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            If .RecordCount > 0 Then
                                .MoveFirst()
                                While .EOF = False
                                    cboPNo.Items.Add(.Fields("PNO").Value & " " & .Fields("Surname").Value & " " & .Fields("Onames").Value)
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
        Catch ex As Exception
            MsgBox("An error has occured while loading queued patients " & Err.Description, MsgBoxStyle.Information)
        End Try

    End Sub

    Private Sub sendToLab()
        Dim BiNo As Long

        Dim dbBamt As Double 'bill amount
        Dim dbBal As Double 'current bill balance
        Dim dbPBal As Double 'previous balance
        Dim intPBNo As Integer 'previous bill number
        Dim dbTAmt As Double ' as total amoount


        GetLabTestsCost(txtDDecision.Text)
        dbLabCost = totalCost
        If dbLabCost <= 0 Then
            dbLabCost = Val(lblLabCost.Content)
            If dbLabCost <= 0 Then
                dbLabCost = (InputBox("Please enter the lab test(s) cost", "Consultation", 0))
                If dbLabCost <= 0 Then
                    Exit Sub
                End If
            End If
        End If
        Try
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblQueue WHERE PNO=" & lnPNO & "AND DESTINATION='lab' AND status='Waiting'  ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)

                If .RecordCount > 0 Then
                    With rsConsultation
                        If .State = 1 Then .Close()
                        .CursorLocation = CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblConsultation WHERE qno=" & rsQueue.Fields("QNO").Value & " AND PNO=" & lnPNO, MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                        If .RecordCount > 0 Then
                            MsgBox("The patient has pending lab test request ", MsgBoxStyle.Information)

                            .Close()

                        End If
                    End With
                Else
                    With rsConsultation
                        .Fields("LabRequest").Value = txtDDecision.Text
                        .Fields("DDetails").Value = txtDDecision.Text
                        .Update()
                    End With

                    Try
                        If CEdit = True Then
                            With rsQueue
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblQueue WHERE PNO=" & lnPNO & "AND DESTINATION='lab' AND status='Pending' AND QNO=" & intQNo & " ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .RecordCount > 0 Then
                                    .Fields("Remarks").Value = "To be tested: " & txtDDecision.Text 'strLabRequest
                                    .Update()
                                End If
                                .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblQueue WHERE PNO=" & lnPNO & "AND DESTINATION='Reception' AND status='Waiting' AND QNO=" & (intQNo - 1) & " ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .RecordCount > 0 Then
                                    .Fields("Remarks").Value = "To pay: " & Val(lblLabCost.Content) 'strLabRequest
                                    .Update()
                                End If
                            End With

                        Else
                            If Trim(strOrg) = "" Then
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
                                    .Fields("PatNo").Value = strPatNo
                                    .Fields("PName").Value = strPName
                                    .Fields("PNo").Value = lnPNO
                                    .Fields("Destination").Value = "Reception"
                                    .Fields("Status").Value = "Waiting"
                                    rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                                    .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value
                                    rsU.Close()
                                    .Fields("Uname").Value = strUser
                                    .Fields("Remarks").Value = "To pay: " & Val(lblLabCost.Content) + dbPBal
                                    .Update()
                                    .Close()

                                End With

                            Else


                            End If


                            With rsQueue
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblQueue ORDER BY qNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .BOF And .EOF Then
                                    lnQNo = 0
                                Else
                                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                                    .MoveLast()
                                    lnQNo = .Fields("qNo").Value
                                End If
                                .AddNew()
                                lnQNo = lnQNo + 1
                                intQNo = lnQNo
                                .Fields("qNO").Value = lnQNo
                                .Fields("QDate").Value = Today
                                .Fields("QTime").Value = Format(Now, "Long Time")
                                .Fields("PatNo").Value = strPatNo
                                .Fields("PName").Value = strPName
                                .Fields("Destination").Value = "Lab"

                                If Trim(strOrg) = "" Then
                                    .Fields("Status").Value = "Pending"
                                Else
                                    .Fields("Status").Value = "Waiting" 'groups to pay later.
                                End If

                                With rsPatient
                                    If .State = 1 Then .Close()
                                    .CursorLocation = CursorLocationEnum.adUseClient
                                    .Open("SELECT pno, PatNo FROM tblPatient WHERE PatNo='" & strPatNo & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                                    If .RecordCount > 0 Then
                                        rsQueue.Fields("PNo").Value = .Fields("PNo").Value
                                    End If
                                    .Close()
                                End With
                                rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                                .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value
                                rsU.Close()
                                .Fields("Uname").Value = strUser
                                .Fields("Remarks").Value = "To be tested: " & txtDDecision.Text
                                .Update()
                                .Close()
                            End With
                        End If
                    Catch ex As Exception
                        MsgBox("An error has occured while adding to queue details " & Err.Description, MsgBoxStyle.Critical)
                    End Try

                    With rsConsultation
                        .Fields("qno").Value = intQNo
                        .Update()
                    End With

                    If CEdit = True Then
                        With rsBill
                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNO & " AND BDate='" & Format(Now, "yyyy-MM-dd").ToString & "' AND BAmt=Bal ORDER BY BDate DESC, BNO Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .RecordCount > 0 Then '
                                dbBamt = .Fields("BAmt").Value
                                dbBal = .Fields("Bal").Value
                                dbPBal = .Fields("PBal").Value
                                dbTAmt = .Fields("TAmt").Value
                                intPBNo = .Fields("PBNo").Value
                                BNo = .Fields("BNo").Value
                                .Fields("uName").Value = strUser
                                .Fields("BAmt").Value = dbLabCost '
                                .Fields("TAmt").Value = dbLabCost '
                                .Fields("Bal").Value = dbLabCost '
                                .Update()
                                .Close()
                                With rsBillDet
                                    If .State = 1 Then .Close()
                                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                    .Open("SELECT * FROM tblBillDetails WHERE BNo=" & BNo & " AND Service='Lab Charges'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                    If .RecordCount > 0 Then
                                        .Fields("SAmt").Value = dbLabCost
                                        .Update()
                                    End If
                                    .Close()
                                End With

                            Else

                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNO & " ORDER BY BNO DESC, BDate DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .RecordCount > 0 Then
                                    GenerateBillNo()
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
                                    .Fields("PNo").Value = lnPNO
                                    .Fields("BNo").Value = BNo
                                    .Fields("BDate").Value = Today
                                    .Fields("BAmt").Value = dbLabCost ' 
                                    .Fields("PBNO").Value = intPBNo
                                    .Fields("PBal").Value = dbPBal
                                    .Fields("TAmt").Value = dbLabCost + dbPBal
                                    .Fields("Bal").Value = dbLabCost + dbPBal
                                    .Fields("UName").Value = strUser
                                    .Fields("Remarks").Value = "Pending"
                                    .Update()
                                    .Close()
                                End If
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
                                    .Fields("PNo").Value = lnPNO
                                    .Fields("BNo").Value = BNo
                                    .Fields("BiNo").Value = BiNo
                                    .Fields("SAmt").Value = dbLabCost
                                    .Fields("Service").Value = "Lab Test(s) cost"
                                    .Fields("RefNo").Value = "Consultation Number " & lblPNo.Content
                                    .Update()
                                    .Close()
                                End With
                            End If
                        End With
                    End If '
                    MsgBox("Request made to lab successfully")
                    strSendTo = "Lab"
                End If
            End With

        Catch ex As Exception
            MsgBox("An error has occured while sending patient to lab " & Err.Description, MsgBoxStyle.Information)
        End Try


    End Sub

    Private Sub sendToPharmacy()
        Dim dbPCost As Decimal
        Dim iCnt As Integer
        Dim BiNo As Long
        Dim dbBamt As Double 'bill amount
        Dim dbBal As Double 'current bill balance
        Dim dbPBal As Double 'previous balance
        Dim intPBNo As Integer 'previous bill number
        Dim dbTAmt As Double ' as total amoount
        Dim rsCon As New ADODB.Recordset()


        Try
            strPharmRequest = InputBox("Enter pharmacy request here below", , txtPrescription.Text)

            If strPharmRequest = "" Then
                MsgBox("You have not entered anything click again to re-enter")
                Exit Sub
            Else
                GetLabTestsCost(txtPrescription.Text)

                If dbPharmCost <= 0 Then
                    dbPharmCost = Val(lblDrugCost.Content)
                    If dbPharmCost <= 0 Then
                        dbPharmCost = (InputBox("Please enter the drug(s) cost", "Consultation", 0))
                        If dbPharmCost <= 0 Then
                            Exit Sub
                        End If
                    End If
                End If


                Try
                    While dbPCost = 0
                        If iCnt >= 2 Then Exit While
                        dbPCost = InputBox("Confirm the drug(s) cost ", "Consultation", dbPharmCost)
                        If IsNumeric(dbPCost) = False Then
                            MsgBox("Invalid cost. Enter details again", MsgBoxStyle.Exclamation)
                            dbPCost = 0
                        Else
                            dbPharmCost = dbPCost
                            Exit While
                        End If
                        iCnt = iCnt + 1
                    End While
                Catch ex As Exception
                    MsgBox(Err.Description)
                End Try

                If CEdit = True Then
                    With rsQueue
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblQueue WHERE PNO=" & lnPNO & "AND DESTINATION='Pharmacy' AND status='Pending' AND QNO=" & intQNo & " ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then
                            .Fields("Remarks").Value = txtPrescription.Text '
                            .Update()
                        End If
                        .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblQueue WHERE PNO=" & lnPNO & "AND DESTINATION='Reception' AND status='Waiting' AND QNO=" & (intQNo - 1) & " ORDER BY QNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then
                            .Fields("Remarks").Value = "To pay: " & dbPharmCost '
                            .Update()
                        End If
                    End With
                    strSendTo = "Pharm"

                Else

                    If Trim(strOrg) = "" Then
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
                            .Fields("PatNo").Value = strPatNo
                            .Fields("PName").Value = strPName
                            .Fields("PNo").Value = lnPNO
                            .Fields("Destination").Value = "Reception"
                            .Fields("Status").Value = "Waiting"
                            rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                            .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value
                            rsU.Close()
                            .Fields("Uname").Value = strUser
                            .Fields("Remarks").Value = "To pay: " & dbPharmCost + dbPBal
                            .Update()
                            .Close()
                            strSendTo = "Pharm"
                        End With
                    Else
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
                        .Fields("PNO").Value = lnPNO
                        .Fields("QDate").Value = Today
                        .Fields("QTime").Value = Format(Now, "Long Time")
                        .Fields("PatNo").Value = strPatNo
                        .Fields("PName").Value = strPName
                        .Fields("Destination").Value = "Pharmacy"

                        If Trim(strOrg) = "" Then
                            .Fields("Status").Value = "Pending"
                        Else
                            .Fields("Status").Value = "Waiting" '
                        End If
                        rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                        .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value
                        rsU.Close()
                        If strPharmRequest = "" Then
                            txtPrescription.Focus()
                            btnSave.Focus()
                            .Fields("Remarks").Value = strPharmRequest
                        Else
                            .Fields("Remarks").Value = strPharmRequest '
                        End If

                        .Fields("Uname").Value = strUser
                        .Update()
                        .Close()
                        strSendTo = "Pharm"
                    End With
                End If

                If strPharmRequest = "" Then
                    GetLabTestsCost(txtPrescription.Text)
                End If
                With rsCon 'ation
                    If .State = 1 Then .Close()
                    .CursorLocation = CursorLocationEnum.adUseClient
                    If lnQNo = 0 Then lnQNo = intQNo
                    .Open("SELECT CSNO, DRequest, prescription, QNO FROM tblConsultation WHERE CSNO=" & Val(lblPNo.Content), MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                    If .RecordCount > 0 Then
                        .Fields("DRequest").Value = Trim(txtPrescription.Text) 'strPharmRequest
                        .Fields("Prescription").Value = strPharmRequest
                        .Fields("QNO").Value = lnQNo
                        .Update()
                    End If

                    .Close()
                End With

                If CEdit = True Then
                    With rsBill
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNO & " AND BDate='" & Format(Now, "yyyy-MM-dd").ToString & "' AND BAmt=Bal AND BAmt>0 ORDER BY BDate DESC, BNO Desc", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount > 0 Then '
                            dbBamt = .Fields("BAmt").Value
                            dbBal = .Fields("Bal").Value
                            dbPBal = .Fields("PBal").Value
                            dbTAmt = .Fields("TAmt").Value
                            intPBNo = .Fields("PBNo").Value
                            BNo = .Fields("BNo").Value
                            .Fields("uName").Value = strUser
                            .Fields("BAmt").Value = dbPharmCost '
                            .Fields("TAmt").Value = dbPharmCost '
                            .Fields("Bal").Value = dbPharmCost '
                            .Update()
                            .Close()

                            GenerateBillDetNo()
                            With rsBillDet
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblBillDetails WHERE BNo=" & BNo & " AND Service='Drugs cost'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .RecordCount > 0 Then
                                    .Fields("SAmt").Value = dbPharmCost
                                    .Update()
                                End If
                                .Close()
                            End With
                            strSendTo = "Pharm"
                        Else

                            If .State = 1 Then .Close()
                            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblBill WHERE PNO=" & lnPNO & " ORDER BY BNO DESC, BDate DESC", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .RecordCount > 0 Then
                                GenerateBillNo()
                                If IsDBNull(.Fields("Bal").Value) = True Then
                                    dbPBal = 0
                                Else
                                    dbPBal = Val(.Fields("Bal").Value)
                                    .Fields("Bal").Value = 0
                                    .Fields("Remarks").Value = "Balance Carried Forward to Bill No. " & BNo
                                    .Update()
                                End If
                                If dbPBal <= 0 And dbPharmCost <= 0 Then
                                    dbPharmCost = Val(InputBox("Drugs cost seems to be zero enter correct amount", , 0))
                                    If dbPharmCost <= 0 Then
                                        MsgBox("Drug cost still not correct!")
                                    End If
                                End If

                                intPBNo = CInt(.Fields("BNo").Value)
                                .AddNew()
                                .Fields("PNo").Value = lnPNO
                                .Fields("BNo").Value = BNo
                                .Fields("BDate").Value = Today
                                .Fields("BAmt").Value = dbPharmCost '
                                .Fields("PBNO").Value = intPBNo
                                .Fields("PBal").Value = dbPBal
                                .Fields("TAmt").Value = dbPharmCost + dbPBal
                                .Fields("Bal").Value = dbPharmCost + dbPBal
                                .Fields("UName").Value = strUser
                                .Fields("Remarks").Value = "Pending"
                                .Update()
                                .Close()
                                strSendTo = "Pharm"
                            End If

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
                                .Fields("PNo").Value = lnPNO
                                .Fields("BNo").Value = BNo
                                .Fields("BiNo").Value = BiNo
                                .Fields("SAmt").Value = dbPharmCost
                                .Fields("Service").Value = "Drugs cost"
                                .Fields("RefNo").Value = "Consultation Number " & lblPNo.Content
                                .Update()
                                .Close()
                                strSendTo = "Pharm"
                            End With
                        End If
                        strSendTo = "Pharm"
                    End With

                End If '

            End If
        Catch ex As Exception
            MsgBox("An error has occured while sending patient to pharmacy " & Err.Description, MsgBoxStyle.Information)
        End Try
    End Sub

    Private Sub sendToNurse()
        With rsQueue
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblQueue ORDER BY qNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF And .EOF Then
                lnQNo = 0
            Else
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                .MoveLast()
                lnQNo = .Fields("qNo").Value
            End If
            .AddNew()
            lnQNo = lnQNo + 1
            intQNo = lnQNo
            .Fields("qNO").Value = lnQNo
            .Fields("QDate").Value = Today
            .Fields("QTime").Value = Format(Now, "Long Time")
            .Fields("PatNo").Value = strPatNo
            .Fields("PName").Value = strPName
            .Fields("Destination").Value = "Nurse"

            .Fields("Status").Value = "Waiting" '

            With rsPatient
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT pno, PatNo FROM tblPatient WHERE PatNo='" & strPatNo & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    rsQueue.Fields("PNo").Value = .Fields("PNo").Value
                End If
                .Close()
            End With

            rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
            .Fields("SendBy").Value = strUser & " " & rsU.Fields("Designation").Value '
            rsU.Close()
            .Fields("Uname").Value = strUser
            .Fields("Remarks").Value = "For: " & txtDDecision.Text '
            .Update()
            .Close()
        End With

    End Sub


    Private Sub txtComplaint_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtComplaint.GotFocus
        stpDetails.Visibility = Windows.Visibility.Collapsed
    End Sub

    Private Sub txtDDecision_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtDDecision.GotFocus
        Dim intItems As Integer
        Dim i As Integer
        lstDetails.Items.Clear()
        lblHeader.Content = "Lab Tests"
        planSno = 0 '
        Try
            intItems = 1 '
            stpDetails.Visibility = Windows.Visibility.Visible
            While intItems <= cboLTest.Items.Count
                i = intItems - 1
                lstDetails.Items.Add(cboLTest.Items(i))
                intItems = intItems + 1
            End While
        Catch ex As Exception
            MsgBox(Err.Description) '
        End Try
    End Sub


    Private Sub txtDDecision_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles txtDDecision.KeyDown
        Try
            If (e.Key = Key.D1 Or e.Key = Key.NumPad1) Then
                One.Header = cboLTest.Items(0)
            ElseIf (e.Key = Key.D2 Or e.Key = Key.NumPad2) Then
                One.Header = cboLTest.Items(1)
            ElseIf (e.Key = Key.D3 Or e.Key = Key.NumPad3) Then
                One.Header = cboLTest.Items(2)
            ElseIf (e.Key = Key.D4 Or e.Key = Key.NumPad4) Then
                One.Header = cboLTest.Items(3)
            ElseIf (e.Key = Key.D5 Or e.Key = Key.NumPad5) Then
                One.Header = cboLTest.Items(4)
            ElseIf (e.Key = Key.D6 Or e.Key = Key.NumPad6) Then
                One.Header = cboLTest.Items(5)
            ElseIf (e.Key = Key.D7 Or e.Key = Key.NumPad7) Then
                One.Header = cboLTest.Items(6)
            ElseIf (e.Key = Key.D8 Or e.Key = Key.NumPad8) Then
                One.Header = cboLTest.Items(7)
            ElseIf (e.Key = Key.D9 Or e.Key = Key.NumPad9) Then
                One.Header = cboLTest.Items(8)
            Else
                txtDDecision.Text = txtDDecision.Text & e.Key
            End If
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try



        Try

        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
        mnuPlan.IsOpen = True
    End Sub


    Private Sub One_Click(sender As Object, e As RoutedEventArgs) Handles One.Click
        TNo = 0
        getTestNumber(One.Header) '
        Try
            With rsLabTests
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblLabTests WHERE LTNO=" & TNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    planSno = planSno + 1
                    txtDDecision.Text = txtDDecision.Text & planSno & ". " & .Fields("TName").Value & " " '
                    strLabRequest = strLabRequest & planSno & ". " & .Fields("TName").Value & "(" & .Fields("cost").Value & ")"
                    dbLabCost = dbLabCost + Val(.Fields("cost").Value)
                    lblLabCost.Content = dbLabCost
                End If
                .Close()
            End With
            txtDDecision.SelectionStart = Len(txtDDecision.Text)
        Catch ex As Exception
            MsgBox("An error has occured while getting lab test details " & Err.Description, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub txtDDecision_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtDDecision.LostFocus
        GetLabTestsCost(txtDDecision.Text)
        repeatedGroups()

    End Sub

    Private Sub txtDDecision_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtDDecision.TextChanged
        Try
            If Trim(txtDDecision.Text) = "" Then
                chkLab.IsChecked = False
            Else
                chkLab.IsChecked = True
            End If

        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub

    Private Sub txtPrescription_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtPrescription.GotFocus

        Dim intItems As Integer
        Dim i As Integer
        lstDetails.Items.Clear()
        lblHeader.Content = "Drugs"
        planSno = 0
        Try
            intItems = 1 '
            stpDetails.Visibility = Windows.Visibility.Visible
            While intItems <= cboDrug.Items.Count
                i = intItems - 1
                lstDetails.Items.Add(cboDrug.Items(i))
                intItems = intItems + 1
            End While
        Catch ex As Exception
            MsgBox(Err.Description) '
        End Try
    End Sub



    Private Sub txtPrescription_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtPrescription.LostFocus
        Try
            GetLabTestsCost(txtPrescription.Text)
            repeatedGroups()

            cboDrug.Focus()
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub txtPrescription_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtPrescription.TextChanged
        Try

            Dim trimChars As Char() = New Char() {" ", ChrW(13), ChrW(10), ChrW(13)}
            If (Me.txtPrescription.Text.Trim(trimChars) = "") Then
                Me.chkPharm.IsChecked = False
                Me.totalCost = Decimal.Subtract(Me.totalCost, Me.dbPharmCost)
                Me.dbPharmCost = New Decimal
                Me.strPrecrip = ""
                Me.strPrescrip1 = ""
                Me.strPharmRequest = ""
                Me.planSno = 1
                Me.prescripno = 0
            Else
                Me.chkPharm.IsChecked = True
                cboDrug.IsEnabled = True
            End If
            '
        Catch ex As Exception
            MsgBox("An error has occured while validating prescription " & Err.Description)
        End Try

    End Sub

    Private Sub btnClose_Click(sender As Object, e As RoutedEventArgs) Handles btnClose.Click
        Try
            stpDetails.Visibility = Windows.Visibility.Collapsed
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub

    Private Sub lstDetails_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstDetails.SelectionChanged
        TNo = 0
        getTestNumber(lstDetails.SelectedItem) '
        If lblHeader.Content = "Lab Tests" Then
            Try
                GetLastPlanNo(txtDDecision.Text)
                With rsLabTests
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblLabTests WHERE LTNO=" & TNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        planSno = planSno + 1
                        txtDDecision.Text = txtDDecision.Text & planSno & ". " & .Fields("TName").Value & " (@" & .Fields("cost").Value & ") " & vbCrLf
                        strLabRequest = strLabRequest & planSno & ". " & .Fields("TName").Value & " (@" & .Fields("cost").Value & ")"
                        dbLabCost = dbLabCost + Val(.Fields("cost").Value)
                        lblLabCost.Content = dbLabCost
                    End If
                    .Close()
                End With
                txtDDecision.SelectionStart = Len(txtDDecision.Text)
            Catch ex As Exception
                MsgBox("An error has occured while getting lab test details " & Err.Description, MsgBoxStyle.Critical)
            End Try
        ElseIf lblHeader.Content = "Drugs" Then
            Try
                getDrugNumber(lstDetails.SelectedItem)
                GetLastPlanNo(txtPrescription.Text)
                With rsDrugs
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblDrugs WHERE DNO=" & DNo, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        prescripno = prescripno + 1
                        txtPrescription.Text = txtPrescription.Text & prescripno & ". " & .Fields("DName").Value & " " '
                        strPharmRequest = strPharmRequest & planSno & ". " & .Fields("DName").Value & "(" & .Fields("cost").Value & ")"
                        dcDrugCost = Val(.Fields("cost").Value)
                        lblDrugCost.Content = dbPharmCost
                        txtStrength.Text = ""
                        txtQty.Text = ""
                        txtDays.Text = ""
                        chkPharm.IsChecked = True
                        cboDrug.IsEnabled = False
                        cboTimes.Text = ""
                        cboTimes.Focus()
                    End If
                    If .State = 1 Then .Close() '
                End With
                txtPrescription.SelectionStart = Len(txtPrescription.Text)
                txtDrugQuantity.Focus()
            Catch ex As Exception
                MsgBox("An error has occured while getting drugs details " & Err.Description, MsgBoxStyle.Critical)
            End Try
        ElseIf lblHeader.Content = "Impression" Then
        End If

    End Sub

    Private Sub btnCLResults_Click(sender As Object, e As RoutedEventArgs) Handles btnCLResults.Click
        Try
            stpLResults.Visibility = Windows.Visibility.Collapsed
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub

    Private Sub txtLabResults_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtLabResults.GotFocus
        Try
            If Trim(txtLabResults.Text) <> "" Then
                stpLResults.Visibility = Windows.Visibility.Visible
            Else
                stpLResults.Visibility = Windows.Visibility.Collapsed
            End If
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub


    Private Sub GetLabTestsCost(str As String)
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
            Dim rsLabDetails As New ADODB.Recordset
            Dim rsLabTest As New ADODB.Recordset
            Dim strDsg As String = ""
            Dim strDrg As String = ""
            Dim iDrg As Integer = 0
            Dim DCount As Integer = 0
            Dim DChar As String = ""
            Dim iSerial As Integer = 1

            totalCost = 0
            arrLabDet.Clear()
            arrPharmDet.Clear()
            strPharmRequest = ""

            If str = "" Then Exit Sub '
            X = Len(str)

            For X = 1 To Len(str)
                Mchar = Mid(str, X, 1)
                If X > 1 Then pChar = Mid(str, X - 1, 1)
                If IsNumeric(pChar) = True And Mchar = "." Or X = Len(str) Then
                    If IsNumeric(sTest) <> True Then
                        If X = Len(str) Then
                            sTest = Trim(sTest) '
                        Else
                            sTest = Trim(Left(sTest, Len(sTest) - 2))
                        End If
                        If lblHeader.Content = "Lab Tests" Then
                            '

                        ElseIf lblHeader.Content = "Drugs" Then
                            strQ = ""
                            For x1 = 1 To Len(sTest)
                                strC = Mid(sTest, x1, 1)
                                If x1 > 2 Then
                                    strC2 = Mid(sTest, x1 - 1, 1)
                                    If IsNumeric(strC) = True And strC2 = "(" Then
                                        strQ = strQ & strC
                                        iCtr = iCtr + 1
                                    ElseIf IsNumeric(strC) = True And (IsNumeric(Mid(sTest, x1 + 1, 1)) = True Or (Mid(sTest, x1 + 1, 1)) = ")") And (Mid(sTest, x1 - 2, 1)) = "(" Then

                                        strQ = strQ & strC
                                        iCtr = iCtr + 1
                                        DChar = (Mid(sTest, x1 + 1, 1))
                                        DCount = x1 + 1
                                        If (Mid(sTest, x1 + 1, 1)) = ")" Then
                                            Do While (IsNumeric(DChar) = False)
                                                DChar = Mid(sTest, DCount, 1)
                                                DCount = DCount + 1
                                                strDsg = strDsg & DChar

                                            Loop
                                        End If
                                        strDsg = strDsg.Trim(trimChars)
                                    End If
                                End If
                            Next
                            x1 = 1

                            strDrg = iSerial & "."
                            strPi = ""
                            For x1 = 1 To Len(sTest)
                                DChar = Mid(sTest, x1, 1)
                                If (Mid(sTest, x1 + 1, 1)) = "*" Then
                                    If ((Mid(sTest, x1 + 1, 1)) = "*" And (IsNumeric(Mid(sTest, x1, 1))) = True And (IsNumeric(Mid(sTest, x1 + 2, 1))) = True) Then
                                        DChar = Mid(sTest, x1 + 1, 2)
                                        dsg(DChar)
                                        strDrg = strDrg & strPi
                                        x1 = x1 + 2
                                        iSerial = iSerial + 1
                                    End If
                                Else
                                    strDrg = strDrg & DChar
                                End If

                            Next
                            strDrg = strDrg '
                            iDQty = Val(strQ)
                            strPharmRequest = strPharmRequest & strDrg
                        End If

                        sTest = sTest.Trim(trimChars)
                        If lblHeader.Content = "Lab Tests" Then
                            With rsLabTest
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblLabTests WHERE TName='" & sTest & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                                If .RecordCount > 0 Then
                                    totalCost = totalCost + rsLabTest.Fields("Cost").Value
                                    lblLabCost.Content = totalCost
                                    arrLabDet.Add(sTest)
                                    dbLabCost = totalCost
                                End If
                                .Close()
                            End With
                        ElseIf lblHeader.Content = "Drugs" Then
                            With rsDrugs
                                If .State = 1 Then .Close()
                                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                                .Open("SELECT * FROM tblDrugs WHERE DNAME='" & sTest & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                                If .RecordCount > 0 Then
                                    totalCost = totalCost + (Val(.Fields("Cost").Value) * iDQty)
                                    lblDrugCost.Content = totalCost
                                    dbPharmCost = totalCost
                                    arrPharmDet.Add(sTest)
                                    iDQty = 0
                                    strQ = ""
                                    strC = ""
                                    strC2 = ""
                                    iCtr = 0
                                End If
                                .Close()
                            End With
                        End If

                    End If

                    sTest = ""
                Else
                    sTest = sTest & Mchar
                End If
            Next X

        Catch ex As Exception
            MsgBox("An error has occured while getting lab tests cost ")
        End Try

    End Sub


    Private Sub GetLastPlanNo(str As String)
        Try
            Dim Mchar As String = ""
            Dim X As Integer
            Dim p As String = ""
            Dim pChar As String = ""
            Dim sTest As String = ""
            Dim rsLabDetails As New ADODB.Recordset
            Dim rsLabTest As New ADODB.Recordset
            totalCost = 0

            If str = "" Then
                planSno = 0
            Else
                X = Len(str)
                For X = Len(str) To X = 0 Step -1
                    Mchar = Mid(str, X, 1)
                    pChar = Mid(str, X - 1, 1)
                    If IsNumeric(pChar) = True And Mchar = "." Then
                        planSno = pChar '+ 1
                        Exit For
                    Else

                    End If
                Next

            End If
            str = ""
        Catch ex As Exception
            MsgBox("An error has occured while numbering entries ")
        End Try
    End Sub

    Private Sub EditBill()
        Try
            Dim rsBil As New ADODB.Recordset

            With rsBil
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblBill WHERE BNO=" & BNo, MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)

            End With
        Catch ex As Exception
            MsgBox("An error has occured while editing bill ")
        End Try

    End Sub

    Private Sub repeatedGroups()
        Dim X1 As Integer = 1
        Dim S1 As String = ""
        Dim S2 As String = ""
        Dim arrRepeated As New ArrayList
        Dim str As String = ""

        Try
            arrLabDet.Sort()

            While X1 < arrLabDet.Count
                S1 = arrLabDet.Item(X1 - 1)
                S2 = arrLabDet.Item(X1)
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

    Private Sub txtComplaint_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtComplaint.TextChanged

    End Sub

    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
        Try
            If strSendTo = "Lab" Then
                printLabRequest()
                strSendTo = ""
            ElseIf strSendTo = "Pharm" Then
                printPharmacyRequest()
                strSendTo = ""
            Else '

            End If

            Exit Sub '

        Catch ex As Exception
            MsgBox("An error has occured while trying to print ")
        End Try


    End Sub

    Public Sub GetServer()
        Try
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
        Catch ex As Exception
            MsgBox("An error has occured while getting server details ")
        End Try

    End Sub


    Private Sub printLabRequest()
        Try
            Dim rptLR As New rptLabRequest
            Dim winRptR As New winRptI
            Dim myLogOnInfo As New TableLogOnInfo()
            Dim myTableLogOnInfos As New TableLogOnInfos
            Dim myConnectionInfo As New ConnectionInfo()
            Dim myDataSourceConnections As DataSourceConnections = rptLR.DataSourceConnections
            Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
            Dim iPNo As String
            Dim rsLabRep As New ADODB.Recordset
            Dim rsQ As New ADODB.Recordset

            Dim myTables As Tables
            Dim myTable As Table
            Dim myTableLogOnInfo As New TableLogOnInfo

            rptLR.Refresh()
            If CSNO <> 0 Then
                With rsLabRep
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblconsultation WHERE CSNO=" & CSNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        iPNo = .Fields("PNO").Value
                        GetServer()
                        myConnectionInfo.ServerName = rServer
                        myConnectionInfo.DatabaseName = rDatabase
                        myConnectionInfo.UserID = ""
                        myConnectionInfo.Password = ""
                        rptLR.SetDatabaseLogon("sa", "pass", rServer, rDatabase)
                        rptLR.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "********")
                        rptLR.DataSourceConnections.Item(0).SetLogon("sa", "********")

                        myTables = rptLR.Database.Tables
                        For Each myTable In myTables
                            myTableLogOnInfo = myTable.LogOnInfo
                            myTableLogOnInfo.ConnectionInfo = myConnectionInfo
                            myTable.ApplyLogOnInfo(myTableLogOnInfo)
                        Next


                        myLogOnInfo.ConnectionInfo = myConnectionInfo

                        rptLR.RecordSelectionFormula = "{tblPatient.PNo} =" & iPNo & "" ' and {tblLab.LsNo} =" & lnLSNo & "" ' and {tblPayment.PYNo} =" & lnPYNO & ""
                        rptLR.Refresh()
                        winRptR.crvMain.ViewerCore.ReportSource = rptLR
                        winRptR.Show()

                    Else
                        MsgBox("Report number does not exist", MsgBoxStyle.Exclamation)
                    End If
                End With


            End If

        Catch ex As Exception
            MsgBox("An error has occured while printing lab tests request ")
        End Try
    End Sub

    Private Sub printPharmacyRequest()
        Try
            Dim rptPR As New rptPharmRequest
            Dim winRptR As New winRptI
            Dim myLogOnInfo As New TableLogOnInfo()
            Dim myTableLogOnInfos As New TableLogOnInfos
            Dim myConnectionInfo As New ConnectionInfo()
            Dim myDataSourceConnections As DataSourceConnections = rptPR.DataSourceConnections
            Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)
            Dim iPNo As String
            Dim rsLabRep As New ADODB.Recordset
            Dim rsQ As New ADODB.Recordset

            Dim myTables As Tables
            Dim myTable As Table
            Dim myTableLogOnInfo As New TableLogOnInfo

            rptPR.Refresh()
            If CSNO <> 0 Then

                With rsLabRep
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblconsultation WHERE CSNO=" & CSNO, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                    If .RecordCount > 0 Then
                        iPNo = .Fields("PNO").Value

                        GetServer()

                        myConnectionInfo.ServerName = rServer
                        myConnectionInfo.DatabaseName = rDatabase
                        myConnectionInfo.UserID = ""
                        myConnectionInfo.Password = ""
                        rptPR.SetDatabaseLogon("sa", "********", rServer, rDatabase)
                        rptPR.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "sa", "********")
                        rptPR.DataSourceConnections.Item(0).SetLogon("sa", "********")

                        myTables = rptPR.Database.Tables
                        For Each myTable In myTables
                            myTableLogOnInfo = myTable.LogOnInfo
                            myTableLogOnInfo.ConnectionInfo = myConnectionInfo
                            myTable.ApplyLogOnInfo(myTableLogOnInfo)
                        Next

                        myLogOnInfo.ConnectionInfo = myConnectionInfo

                        rptPR.RecordSelectionFormula = "{tblPatient.PNo} =" & iPNo & "" ' 
                        rptPR.Refresh()
                        winRptR.crvMain.ViewerCore.ReportSource = rptPR
                        winRptR.Show()

                    Else
                        MsgBox("Report number does not exist", MsgBoxStyle.Exclamation)
                    End If
                End With


            End If

        Catch ex As Exception
            MsgBox("An error has occured while printing pharmacy request ")
        End Try

    End Sub



    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        'have cancel code here
        ClearConsultationData()

        bnNew = False
        cboPNo.IsEnabled = True

        CEdit = False
        lngCRec = 0
        rsConsultation = New ADODB.Recordset
        Try
            With rsConsultation
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblConsultation ORDER BY CSNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            End With
        Catch ex As Exception
            MsgBox("An error has occured while loading consultation data " & Err.Description)
        End Try

        rsPatient = New ADODB.Recordset
        rsBill = New ADODB.Recordset
        rsBillDet = New ADODB.Recordset
        rsLab = New ADODB.Recordset
        rsPharm = New ADODB.Recordset
        rsDrugs = New ADODB.Recordset
        rsLabTests = New ADODB.Recordset
        dcDrugCost = 0
        curAmt = 0
        BNo = 0
        DNo = 0
        TNo = 0
        lnPNO = 0
        BDetNo = 0
        CSNO = 0
        LSNo = 0
        planSno = 0
        prescripno = 0
        lnPSNO = 0
        strPName = ""
        strPatNo = ""
        lnQNo = 0
        strAge = ""
        rsPreviousConsultation = New ADODB.Recordset
        dbLabCost = 0
        dbPharmCost = 0
        totalCost = 0
        nQueue = 0

        iDQty = 0
        intDays = 0
        strPrecrip = ""
        strPrescrip1 = ""
        strP = ""
        strPi = ""
        strSendTo = ""
        btnNext.IsEnabled = True
        btnFirst.IsEnabled = True
        btnLast.IsEnabled = True
        btnPrevious.IsEnabled = True


    End Sub


    Private Sub txtDrugQuantity_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtDrugQuantity.LostFocus


    End Sub

    Private Sub txtImpression_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtImpression.GotFocus
        Try
            stpDetails.Visibility = Windows.Visibility.Collapsed
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub

    Private Sub txtAllergies_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtAllergies.GotFocus
        Try
            stpDetails.Visibility = Windows.Visibility.Collapsed
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub


    Public Function getDrugDays(ByVal cboC As String)

        Dim str1 As String = ""
        Dim str2 As String = ""

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
            Me.intDays = (Val(str2))

        Catch exception1 As Exception
            MsgBox("An error has occured while getting drug days ")
        End Try

        Return CType(0, Integer)
    End Function

    Private Sub drugCalc()
        Dim str1 As String = ""
        Dim num1 As Integer
        Dim str2 As String = Strings.Trim(Me.txtQty.Text)
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

        getDrugNumber(cboDrug.SelectedItem)


        Try
            If cboTimes.SelectedItem = "OD" Then
                num1 = 1
                str1 = str2 & "*" & (num1)
                Me.strPi = "OD"
            ElseIf cboTimes.SelectedItem = "BD" Then
                num1 = 2
                str1 = str2 & "*" & (num1)
                Me.strPi = "BD"
            ElseIf cboTimes.SelectedItem = "TDS" Then
                num1 = 3
                str1 = str2 & "*" & (num1)
                Me.strPi = "TDS"
            ElseIf cboTimes.SelectedItem = "QID" Then
                num1 = 4
                str1 = str2 & "*" & (num1)
                Me.strPi = "QID"
            ElseIf cboTimes.SelectedItem = "QSD" Then
                num1 = 5
                str1 = str2 & "*" & (num1)
                Me.strPi = "QSD"
            ElseIf cboTimes.SelectedItem = "M" Then
                num1 = 1
                str1 = str2 & "*" & (num1)
                Me.strPi = "M"
            ElseIf cboTimes.SelectedItem = "NOCTE" Then
                num1 = 1
                str1 = str2 & "*" & (num1)
                Me.strPi = "NOCTE"
            ElseIf cboTimes.SelectedItem = "PRN" Then
                num1 = Val(InputBox("Enter number of times ", "Consultation"))
                str1 = str2 & "*" & (num1)
                Me.strPi = "PRN"
            End If

        Catch ex As Exception
            MsgBox("An error has occured while calculating drug cost " & Err.Description)
        End Try

        Try
            Me.iDQty = ((Me.intDays * (num1 * Val(Me.txtQty.Text))))
            Me.txtDrugQuantity.Text = (Me.iDQty)
            Me.strPrecrip = str1 & " " & Me.txtDays.Text
            Me.strPrescrip1 = Me.strPi & " " & txtStrength.Text & " " & Me.txtDays.Text & vbCrLf
            Me.lblDrugCost.Content = ""
            strPharmRequest = strPharmRequest & " (" & txtDrugQuantity.Text & ") " & strPrescrip1 '
            txtPrescription.Text = txtPrescription.Text & " " & strPrescrip1
            txtPrescription.ScrollToEnd()
            dbPharmCost = (dbPharmCost) + ((dcDrugCost) * Val(Me.txtDrugQuantity.Text))
            Me.lblDrugCost.Content = CType(Me.dbPharmCost, Decimal)
            Me.strPrecrip = ""
        Catch ex As Exception
            MsgBox("An error has occured while entering drug details " & Err.Description)
        End Try
    End Sub

    Private Sub txtQty_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtQty.LostFocus
        Try
            If (IsNumeric(Strings.Trim(Me.txtQty.Text)) = False) Then
                Interaction.MsgBox("Enter numeric value (0...9)", MsgBoxStyle.OkOnly, Nothing)
                Me.txtQty.Text = ""
            ElseIf (Strings.Trim(Me.txtQty.Text) = "") Then
                Interaction.MsgBox("Enter the dosage quantity", MsgBoxStyle.OkOnly, Nothing)
            Else
                ' Me.cboTimes.Text = ""
                txtPrescription.IsEnabled = True
                txtDays.Focus()
            End If
        Catch ex As Exception
            MsgBox("An error has occured while getting drugs quantity ")
        End Try

    End Sub

    Private Sub cboTimes_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboTimes.SelectionChanged
        Try
            Me.txtQty.Text = (1)
            txtStrength.Focus()

        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub txtDays_GotFocus(sender As Object, e As RoutedEventArgs) Handles txtDays.GotFocus
        cboDrug.IsEnabled = True
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
                ElseIf (iCounter <> 1) Then
                    bnflag = True
                End If

                If bnflag = True Then '
                    MsgBox("Please enter details in the right format, e.g. 2/7")
                    Exit Sub
                End If
                drugCalc()
                cboDrug.SelectedItem = ""

                txtPrescription.ScrollToEnd()
                txtPrescription.Focus()
            End If

        Catch ex As Exception
            MsgBox("An error has occured while getting drug days " & Err.Description)
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

    Private Sub txtStrength_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtStrength.LostFocus
        Dim x1 As Integer = 1 ' 
        Dim Mchar As String = ""
        If Trim(txtStrength.Text) = "" Then Exit Sub '
        Mchar = Right(Trim(txtStrength.Text), 2)
        If LCase(Mchar) = "mg" Then
        Else
            txtStrength.Text = txtStrength.Text & "mg"
        End If

        txtQty.Focus()
    End Sub



    Private Sub cboImpression_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboImpression.SelectionChanged

        txtImpression.Text = cboImpression.SelectedItem

    End Sub
End Class


