


Imports ADODB
Imports System.Text.RegularExpressions
Class pgInjectables


    Private lngCRec As Long 'current record
    Private CEdit As Boolean
    Private iAns As Integer
    Private MainWin As New MainWindow
    Private rsFP As New ADODB.Recordset()
    Private rsQueue As New ADODB.Recordset()
    Private lnPNo As Integer

    Private bnClearQueue As Boolean
    Private bnNew As Boolean = False 'check if new record procedure has been called

    Private lnQNO As Integer
    Public sUname As String
    Public strUser As String
    Private FPCNo As Long
    Private strPatNo As String

    Private rsPatient As New ADODB.Recordset
    Private strPatientName As String
    Private dtDoB As Date


    Private Sub pgFP_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized


        lblToday.Content = Today
        btnSave.IsEnabled = False
        btnCancel.IsEnabled = False
        btnEdit.IsEnabled = True
        txtComments.IsEnabled = False
        'txtGravida.IsEnabled = False
        btnArchive.IsEnabled = True

    End Sub

    Private Sub btnArchive_Click(sender As Object, e As RoutedEventArgs) Handles btnArchive.Click
        Dim strPNo As String = GetPatSNo(lblDetails.Content)
        Try
            With rsFP
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblFPC WHERE FPNo=" & FPCNo, MainWin.cnHCare, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                lngCRec = .AbsolutePosition
                If .RecordCount > 0 Then
                    If MsgBox("Do you really want to archive this record? ", MsgBoxStyle.YesNo) = vbYes Then
                        .Fields("status").Value = "Archived"
                        .Update()
                        MsgBox("Record archived!", MsgBoxStyle.Exclamation)
                    End If
                End If
                .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblFPC  WHERE STATUS<>'ARCHIVED' ORDER BY FPNo", MainWin.cnHCare, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .Move(lngCRec)
                    GetFPData()
                Else
                    ClearFPData()
                End If
            End With
        Catch
            MsgBox("An error has occured while archiving a record " & Err.Description)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            If CEdit = True Then
                rsFP.Close()
                CEdit = False
                rsFP = New ADODB.Recordset()
                rsFP.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsFP.Open("SELECT *FROM tblFPC  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCare, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsFP.Move(lngCRec)
            Else
                With rsFP
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetFPData()
                    Else
                        MsgBox("Nothing to Cancel")
                        Me.txtDrug.Focus()
                    End If
                End With
            End If
        Catch
            MsgBox("An error has occured while canceling record " & Err.Description)
        End Try
        Try
            btnSave.IsEnabled = False
            'btnNew.IsEnabled = True
            btnCancel.IsEnabled = False
            'btnNew.Focus()
        Catch ex As Exception
            MsgBox("An error has occured while changing controls settings " & Err.Description)
        End Try



    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        Try
            lngCRec = rsFP.AbsolutePosition
            'getPatientNumber(lblPNo.Content)
            GetPatSNo(lblDetails.Content)
            With rsFP
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub
                Else
                    EditReady()
                    .Close()
                    rsFP = New ADODB.Recordset()
                    rsFP.Open("SELECT * FROM tblFPC WHERE FPNo=" & FPCNo, MainWin.cnHCare, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    CEdit = True
                    btnSave.IsEnabled = True
                    btnCancel.IsEnabled = True
                    '  btnSend.IsEnabled = True
                End If
            End With
        Catch
            MsgBox("An error has occured while preparing to edit record " & Err.Description)
        End Try



    End Sub

    Private Sub btnFind_Click(sender As Object, e As RoutedEventArgs) Handles btnFind.Click
        Try
            Dim nwWin As New Window1
            Dim fiS As New Frame
            Dim ti As New TabItem
            Dim pgPatSearch As New pgPatSearch

            ' pgPatSearch.dgBrush.Color = dgBrush.Color
            pgPatSearch.strUser = strUser
            fiS.NavigationService.Navigate(pgPatSearch)
            ti.Content = fiS
            nwWin.tcSearch.Items.Add(ti)
            nwWin.Show()
        Catch ex As Exception
            MsgBox("An error has occured while loading search window " & Err.Description)
        End Try


    End Sub

    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs) Handles btnFirst.Click
        Try
            With rsFP
                If .RecordCount <> 0 Then
                    If .BOF = True Or .EOF = True Then Exit Sub
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                ' btnNew.IsEnabled = True
                                GetFPData()
                            End If
                        Else
                            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveFirst()
                        btnPrevious.IsEnabled = False
                        btnNext.IsEnabled = True
                        GetFPData()

                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the first record " & Err.Description)
        End Try

    End Sub


    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click
        Try
            With rsFP
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnPrevious.IsEnabled = False
                                ' btnNew.IsEnabled = True
                                GetFPData()
                            End If
                        Else
                            MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveLast()
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = False
                        GetFPData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the last record " & Err.Description)
        End Try
    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        Try
            With rsFP
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
                                ' btnNew.IsEnabled = True
                                GetFPData()
                            Else
                                .CancelUpdate()
                                .MoveNext()
                                btnPrevious.IsEnabled = True
                                ' btnNew.IsEnabled = True
                                GetFPData()
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
                        GetFPData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the next record " & Err.Description)
        End Try
    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click
        Try
            With rsFP
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
                                'btnNew.IsEnabled = True
                                GetFPData()
                            Else
                                .CancelUpdate()
                                .MovePrevious()
                                btnNext.IsEnabled = True
                                'btnNew.IsEnabled = True
                                GetFPData()
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
                        GetFPData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the previous record " & Err.Description)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If CEdit = True Then
            SetFPData()
            rsFP.Update()

            MsgBox(" Record Saved", MsgBoxStyle.Information, "Save")
            rsFP.Close()

            CEdit = False

            rsFP = New ADODB.Recordset()
            rsFP.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsFP.Open("SELECT * FROM tblFPC WHERE STATUS<>'ARCHIVED' ", MainWin.cnHCare, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            rsFP.Move(lngCRec)
            'btnNew.IsEnabled = True
            btnSave.IsEnabled = False
            ' ElseIf rsFP.EditMode = ADODB.EditModeEnum.adEditNone Then
            '    MsgBox("No changes have been made add a new record or edit this record then save again", MsgBoxStyle.Information, "Save")
            ' btnNew.IsEnabled = True
            '   btnSave.IsEnabled = False
        Else
            'check that all fields ave been filledv

            If Trim(txtDrug.Text) = "" Then
                MsgBox("Enter drug name ", MsgBoxStyle.Information)
                txtDrug.Focus()
            ElseIf Trim(txtDosage.Text) = "" Then
                MsgBox("Please enter dosage", MsgBoxStyle.Information)
                txtDosage.Focus()
            ElseIf Trim(txtRoute.Text) = "" Then
                MsgBox("Please enter Route", MsgBoxStyle.Information)
                txtRoute.Focus()
            ElseIf (Trim(txtComments.Text)) = False Then
                MsgBox("Enter comments", MsgBoxStyle.Information)
                txtComments.Focus()
            Else
                With rsFP
                    .CancelUpdate()
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblFPC", MainWin.cnHCare, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    .AddNew()
                    SetFPData()
                    .Update()
                    MsgBox("Record Saved!", MsgBoxStyle.Information)
                    'btnNew.IsEnabled = True
                    'remove from queue
                    updateQueue()
                    btnSave.IsEnabled = False
                    btnCancel.IsEnabled = False
                    btnEdit.IsEnabled = True
                    txtComments.IsEnabled = False
                    ' txtGravida.IsEnabled = False
                    btnArchive.IsEnabled = True
                End With
            End If
        End If

    End Sub

    Private Sub updateQueue()
        Dim rsQueueUpdate As New ADODB.Recordset
        Dim rsPa As New ADODB.Recordset
        Dim rsU As New ADODB.Recordset
        Dim strP As String
        Dim Etime As Date
        Try
            With rsPa
                .Open("SELECT * FROM tblPatient WHERE PNO=" & lnPNo, MainWin.cnHCare, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                strP = .Fields("PatNo").Value
                .Close()
            End With


            With rsQueueUpdate
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblQueue WHERE PatNo='" & strPatNo & "' AND status='Waiting' AND DESTINATION='FP' ORDER BY qno Desc", MainWin.cnHCare, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF = True And .EOF = True Then
                    .Close()
                    Exit Sub
                End If
                .Fields("Status").Value = "Attended"
                .Fields("ADate").Value = Today
                .Fields("ATime").Value = Format(Now, "Long Time")
                rsU.Open("SELECT UName, Designation FROM tblUser WHERE UName='" & strUser & "'", MainWin.cnHCare, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
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

    Private Function GetPatSNo(strNo As String)
        Try
            Dim Mchar As String = ""
            ''Dim cboC As String
            Dim intLength As Integer
            Dim X As Integer
            Dim iMNo As String = ""
            Dim iSNo As String = ""

            'get Customer no only
            ' cboC = lblPNo.Content
            intLength = Len(strNo)
            For X = intLength To 0 Step -1
                Mchar = Mid(strNo, X, 1)
                If Mchar = "-" Then Exit For
                iMNo = iMNo + Mchar
            Next X

            FPCNo = Val(Right(strNo, Len(iMNo)))

        Catch ex As Exception
            MsgBox("An error has occured while getting patient serial number " & Err.Description)
        End Try

        Return FPCNo
    End Function


    Private Sub ClearFPData()

        txtComments.Text = ""
        ' txtGravida.Text = ""
        txtDosage.Text = ""
        txtRoute.Text = ""
        txtDrug.Text = ""

    End Sub

    Private Sub GetFPData()
        Try
            With rsFP
                ' txtComments.Text = .Fields("Comments").Value
                'txtGravida.Text = .Fields("gravida").Value
                txtDosage.Text = .Fields("Height").Value
                txtRoute.Text = .Fields("immunization").Value
                txtDrug.Text = .Fields("weight").Value
                lblRecNo.Content = "Record " & .AbsolutePosition & " Of " & .RecordCount & " Records"
            End With
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try

    End Sub


    Private Sub SetFPData()
        Try
            With rsFP
                '.Fields("Comments").Value = txtComments.Text
                '.Fields("gravida").Value = txtGravida.Text
                .Fields("Height").Value = txtDosage.Text
                .Fields("immunization").Value = txtRoute.Text
                .Fields("weight").Value = txtDrug.Text
                lblRecNo.Content = "Record " & .AbsolutePosition & " Of " & .RecordCount & " Records"
            End With
        Catch ex As Exception
            MsgBox("An error has occured: " & Err.Description)
        End Try

    End Sub

    Private Sub EditReady()
        txtDrug.IsEnabled = True
        txtComments.IsEnabled = True
        ' txtGravida.IsEnabled = True
        txtDosage.IsEnabled = True
        ' txtGravida.IsEnabled = True
        txtRoute.IsEnabled = True
        btnArchive.IsEnabled = False
        btnSave.IsEnabled = True
        btnCancel.IsEnabled = True
        btnEdit.IsEnabled = True
        btnFind.IsEnabled = False


    End Sub

End Class


