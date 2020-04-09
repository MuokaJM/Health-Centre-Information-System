Imports ADODB
Imports System.Text.RegularExpressions

Class pgLabTests

    Private rsTest As New ADODB.Recordset
    Private lnTNO As Long
    Private CEdit As Boolean = False
    Private lngCRec As Long
    Private MainWin As New MainWindow
    Public strUser As String



    Private Sub pgLabTests_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        lblToday.Content = Format(Today, "dd-MMMM-yy")
        btnSave.IsEnabled = False
        btnCancel.IsEnabled = False
        txtName.IsEnabled = False
        txtDescription.IsEnabled = False
        txtDuration.IsEnabled = False
        txtCost.IsEnabled = False


        Try
            With rsTest
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblLabTests  WHERE STATUS<>'ARCHIVED' ORDER BY LTNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then GetTestData()
            End With
        Catch
            MsgBox("An error has occured during form load ", Err.Description)
        End Try
    End Sub



    Private Function SetTestData()
        Try
            With rsTest
                .Fields("LTNO").Value = lnTNO
                .Fields("TName").Value = txtName.Text
                .Fields("Description").Value = txtDescription.Text
                .Fields("Duration").Value = txtDuration.Text
                .Fields("Cost").Value = Val(txtCost.Text)
                .Fields("status").Value = "ON"
            End With
        Catch
            MsgBox("An error has occured while setting data for saving ", Err.Description)
        End Try

        Return (0)
    End Function

    Private Function ClearTestData()

        txtName.Text = ""
        txtDescription.Text = ""
        txtDuration.Text = ""
        txtCost.Text = ""
        lblRecNo.Content = ""
        lblNo.Content = ""

        Return (0)
    End Function

    Private Function GetTestData()
        Try
            With rsTest
                lblNo.Content = .Fields("LTNO").Value
                txtName.Text = .Fields("TName").Value
                txtDescription.Text = .Fields("Description").Value
                txtDuration.Text = .Fields("Duration").Value
                txtCost.Text = .Fields("Cost").Value
                lblRecNo.Content = "Record " & .AbsolutePosition & " of " & .RecordCount & " Records"
            End With
        Catch
            MsgBox("An error has occured while fetching data ", Err.Description)
        End Try
        Return (0)
    End Function

    Private Sub btnNew_Click(sender As Object, e As RoutedEventArgs) Handles btnNew.Click
        Try
            With rsTest
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblLabTests ORDER BY LTNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF And .EOF Then
                    lnTNO = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    lnTNO = .Fields("LTNo").Value
                End If
                .AddNew()
                lnTNO = lnTNO + 1
                ClearTestData()
                lblNo.Content = lnTNO
                EditReady()

            End With
        Catch
            MsgBox("An error has occured while generating new record ", Err.Description)
        End Try
    End Sub



    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Try
            If CEdit = True Then
                SetTestData()
                rsTest.Update()

                MsgBox("Test " & txtName.Text & " Record Saved", MsgBoxStyle.Information, "Save")
                rsTest.Close()

                CEdit = False

                rsTest = New ADODB.Recordset()
                rsTest.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsTest.Open("SELECT * FROM tblLabTests  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsTest.Move(lngCRec)
                btnNew.IsEnabled = True
                btnSave.IsEnabled = False
                btnEdit.IsEnabled = True
            ElseIf rsTest.EditMode = ADODB.EditModeEnum.adEditNone Then
                MsgBox("No changes have been made add a new record or edit this record then save again", MsgBoxStyle.Information, "Save")
                btnNew.IsEnabled = True
                btnSave.IsEnabled = False
            Else

                If Trim(txtName.Text) = "" Then
                    MsgBox("Please enter the name of the test", MsgBoxStyle.Information)
                    txtName.Focus()
                ElseIf Trim(txtDescription.Text) = "" Then
                    MsgBox("Please describe the test", MsgBoxStyle.Information)
                    txtDescription.Focus()
                ElseIf Trim(txtDuration.Text) = "" Then
                    MsgBox("Please enter the  duration needed to carry out the test", MsgBoxStyle.Information)
                    txtDuration.Focus()
                ElseIf Trim(txtCost.Text) = "" Then
                    MsgBox("Please enter the cost of the test", MsgBoxStyle.Information)
                    txtCost.Focus()
                ElseIf IsNumeric(Trim(txtCost.Text)) = False Then
                    MsgBox("Cost can contain numbers only (0...9)", MsgBoxStyle.Information)
                    txtCost.Focus()
                Else
                    With rsTest
                        .CancelUpdate()
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblLabTests  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .BOF And .EOF Then
                            lnTNO = 0
                        Else
                            If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                            .MoveLast()
                            lnTNO = .Fields("LTNo").Value
                        End If
                        lnTNO = lnTNO + 1
                        .AddNew()
                        SetTestData()
                        .Update()
                        MsgBox("Record Saved!", MsgBoxStyle.Information)
                        btnNew.IsEnabled = True
                        btnSave.IsEnabled = False
                        btnCancel.IsEnabled = False
                        btnEdit.IsEnabled = True
                        txtName.IsEnabled = False
                        txtDescription.IsEnabled = False
                        txtDuration.IsEnabled = False
                        txtCost.IsEnabled = False
                        btnArchive.IsEnabled = True
                        btnFirst.IsEnabled = True
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = True
                        btnLast.IsEnabled = True
                    End With
                End If
            End If
        Catch
            MsgBox("An error has occured while saving data ", Err.Description)
        End Try
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        Dim Value As String
        lngCRec = rsTest.AbsolutePosition
        Value = lblNo.Content
        lnTNO = Value
        btnFirst.IsEnabled = False
        btnPrevious.IsEnabled = False
        btnNext.IsEnabled = False
        btnLast.IsEnabled = False
        Try
            With rsTest
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub
                Else
                    .Close()
                    rsTest = New ADODB.Recordset()
                    rsTest.Open("SELECT * FROM tblLabTests WHERE LTNO=" & Value, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    CEdit = True
                    btnSave.IsEnabled = True
                    btnCancel.IsEnabled = True
                    EditReady()

                End If
            End With
        Catch
            MsgBox("An error has occured while editing record ", Err.Description)
        End Try
    End Sub


    Private Sub EditReady()
        Try
            btnFirst.IsEnabled = False
            btnPrevious.IsEnabled = False
            btnNext.IsEnabled = False
            btnLast.IsEnabled = False
            btnSave.IsEnabled = True
            btnEdit.IsEnabled = False
            btnCancel.IsEnabled = True
            btnNew.IsEnabled = False
            txtName.IsEnabled = True
            txtDescription.IsEnabled = True
            txtDuration.IsEnabled = True
            txtCost.IsEnabled = True
            txtName.Focus()
        Catch
            MsgBox("An error has occured while preparing to edit ", MsgBoxStyle.Exclamation)
        End Try

    End Sub


    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            If CEdit = True Then
                rsTest.Close()
                CEdit = False

                rsTest = New ADODB.Recordset()
                rsTest.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsTest.Open("SELECT * FROM tblLabTests  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsTest.Move(lngCRec)
            Else

                With rsTest
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetTestData()
                    Else
                        MsgBox("Nothing to Cancel")
                        Me.txtName.Focus()
                    End If
                End With

            End If
            btnSave.IsEnabled = False
            btnNew.IsEnabled = True
            btnCancel.IsEnabled = False
            btnNew.Focus()
        Catch
            MsgBox("An error has occured while cancelling record ", Err.Description)
        End Try
    End Sub



    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs) Handles btnFirst.Click
        Try
            With rsTest
                If .RecordCount <> 0 Then
                    If .BOF = True Or .EOF = True Then Exit Sub
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveFirst()
                                btnNext.IsEnabled = True
                                btnNew.IsEnabled = True
                                GetTestData()
                            End If
                        Else
                            MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                        End If
                    Else
                        .MoveFirst()
                        btnPrevious.IsEnabled = False
                        btnNext.IsEnabled = True
                        GetTestData()

                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the first record ", Err.Description)
        End Try

    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click
        Try
            With rsTest
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
                                btnNew.IsEnabled = True
                                GetTestData()
                            Else
                                .CancelUpdate()
                                .MovePrevious()
                                btnNext.IsEnabled = True
                                btnNew.IsEnabled = True
                                GetTestData()

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
                        GetTestData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the previous record ", Err.Description)
        End Try
    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        Try
            With rsTest
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
                                btnNew.IsEnabled = True
                                GetTestData()

                            Else
                                .CancelUpdate()
                                .MoveNext()
                                btnPrevious.IsEnabled = True
                                btnNew.IsEnabled = True
                                GetTestData()

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
                        GetTestData()

                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the next record ", Err.Description)
        End Try
    End Sub

    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click
        Try
            With rsTest
                If .RecordCount <> 0 Then
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                            If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                                .CancelUpdate()
                                .MoveLast()
                                btnPrevious.IsEnabled = False
                                btnNew.IsEnabled = True
                                GetTestData()
                            End If
                        Else
                            MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")

                        End If
                    Else
                        .MoveLast()
                        btnPrevious.IsEnabled = True
                        btnNext.IsEnabled = False
                        GetTestData()
                    End If
                End If
            End With
        Catch
            MsgBox("An error has occured while moving to the last record ", Err.Description)
        End Try
    End Sub



    Private Sub txtCost_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtCost.LostFocus

        If IsNumeric(Trim(txtCost.Text)) = False Then
            MsgBox("Cost can only have numbers (0...9)", MsgBoxStyle.Information)

        End If

    End Sub



    Private Sub btnArchive_Click(sender As Object, e As RoutedEventArgs) Handles btnArchive.Click
        Try
            With rsTest
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblLabTests WHERE LTNo=" & Val(lblNo.Content), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
                .Open("SELECT * FROM tblLabTests  WHERE STATUS<>'ARCHIVED' ORDER BY LTNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .Move(lngCRec)
                    GetTestData()
                Else
                    ClearTestData()
                End If
            End With
        Catch
            MsgBox("An error has occured while archiving a record ", Err.Description)
        End Try
    End Sub
End Class
