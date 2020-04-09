
Imports ADODB

Class pgDrugs

    Private rsDrug As New ADODB.Recordset
    Private lnDNO As Long
    Private CEdit As Boolean = False
    Private lngCRec As Long
    Private MainWin As New MainWindow
    Public strUser As String


    Private Sub pgDrugs_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        lblToday.Content = Format(Today, "dd-MMMM-yy")
        btnSave.IsEnabled = False
        btnCancel.IsEnabled = False
        txtName.IsEnabled = False
        txtDescription.IsEnabled = False
        txtCost.IsEnabled = False
        txtAlt.IsEnabled = False
        txtQty.IsEnabled = False
        txtTradeName.IsEnabled = False
        txtNotes.IsEnabled = False
        txtUnit.IsEnabled = False
        txtPackage.IsEnabled = False

        Try
            With rsDrug
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDrugs WHERE STATUS<>'ARCHIVED' ORDER BY DNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then GetDrugData()
            End With
        Catch
            MsgBox("An error has occured during form load " & Err.Description)
        End Try
    End Sub


    Private Function SetDrugData()
        Try

            With rsDrug
                .Fields("DNO").Value = lnDNO
                .Fields("DName").Value = txtName.Text
                .Fields("Description").Value = txtDescription.Text
                .Fields("Quantity").Value = txtQty.Text
                .Fields("TradeName").Value = txtTradeName.Text
                .Fields("Alternatives").Value = txtAlt.Text
                .Fields("Notes").Value = txtNotes.Text
                .Fields("Unit").Value = txtUnit.Text
                .Fields("Package").Value = txtPackage.Text
                .Fields("Cost").Value = Val(txtCost.Text)
                .Fields("Status").Value = "On"

            End With
        Catch
            MsgBox("An error has occured while setting data for saving " & Err.Description)
        End Try

        Return (0)
    End Function

    Private Function ClearDrugData()
        lblRecNo.Content = ""
        lblNo.Content = ""
        txtName.Text = ""
        txtDescription.Text = ""
        txtCost.Text = ""
        txtAlt.Text = ""
        txtQty.Text = ""
        txtTradeName.Text = ""
        txtNotes.Text = ""
        txtUnit.Text = ""
        txtPackage.Text = ""

        Return (0)
    End Function

    Private Function GetDrugData()
        Try


            With rsDrug
                lblNo.Content = .Fields("DNO").Value
                txtName.Text = .Fields("DName").Value
                txtDescription.Text = .Fields("Description").Value
                txtCost.Text = .Fields("Cost").Value
                txtAlt.Text = .Fields("Alternatives").Value
                txtQty.Text = .Fields("Quantity").Value
                txtTradeName.Text = .Fields("TradeName").Value
                txtNotes.Text = .Fields("Notes").Value
                txtUnit.Text = .Fields("Unit").Value
                txtPackage.Text = .Fields("Package").Value
                lblRecNo.Content = "Record " & .AbsolutePosition & " of " & .RecordCount & " Records"
            End With
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
        Return (0)
    End Function

    Private Sub btnNew_Click(sender As Object, e As RoutedEventArgs) Handles btnNew.Click
        With rsDrug
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblDrugs ORDER BY DNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF And .EOF Then
                lnDNO = 0
            Else
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                .MoveLast()
                lnDNO = .Fields("DNO").Value
            End If
            .AddNew()
            lnDNO = lnDNO + 1
            lblNo.Content = lnDNO
            ClearDrugData()
            EditReady()

        End With
    End Sub
    Private Sub EditReady()
        Try
            btnSave.IsEnabled = True
            btnEdit.IsEnabled = False
            btnCancel.IsEnabled = True
            btnNew.IsEnabled = False
            txtName.IsEnabled = True
            txtDescription.IsEnabled = True
            txtCost.IsEnabled = True
            txtAlt.IsEnabled = True
            txtQty.IsEnabled = True
            txtTradeName.IsEnabled = True
            txtNotes.IsEnabled = True
            txtUnit.IsEnabled = True
            txtPackage.IsEnabled = True
            txtName.Focus()
        Catch ex As Exception
            MsgBox("An error has occured while preparing fields for editing", MsgBoxStyle.Exclamation)
        End Try

    End Sub
    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If CEdit = True Then
            SetDrugData()
            rsDrug.Update()

            MsgBox("Drug " & txtName.Text & " Record Saved", MsgBoxStyle.Information, "Save")
            rsDrug.Close()

            CEdit = False

            rsDrug = New ADODB.Recordset()
            rsDrug.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsDrug.Open("SELECT * FROM tblDrugs WHERE STATUS<>'ARCHIVED' ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            rsDrug.Move(lngCRec)
            btnNew.IsEnabled = True
            btnSave.IsEnabled = False
        ElseIf rsDrug.EditMode = ADODB.EditModeEnum.adEditNone Then
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
            ElseIf Trim(txtTradeName.Text) = "" Then
                MsgBox("Please enter the trade name of the drug", MsgBoxStyle.Information)
                txtTradeName.Focus()
            ElseIf Trim(txtAlt.Text) = "" Then
                MsgBox("Please any alternative drug or 'No Alternative'", MsgBoxStyle.Information)
                txtAlt.Focus()
            ElseIf Trim(txtPackage.Text) = "" Then
                MsgBox("Please enter the drugs are packaged", MsgBoxStyle.Information)
                txtPackage.Focus()
            ElseIf Trim(txtNotes.Text) = "" Then
                MsgBox("Please enter more info on the drug or 'No Info'", MsgBoxStyle.Information)
                txtNotes.Focus()
            ElseIf Trim(txtUnit.Text) = "" Then
                MsgBox("Please enter the drug's unit", MsgBoxStyle.Information)
                txtUnit.Focus()
            ElseIf Trim(txtQty.Text) = "" Then
                MsgBox("Please enter the quantity available", MsgBoxStyle.Information)
                txtQty.Focus()
            ElseIf IsNumeric(Trim(txtQty.Text)) = False Then
                MsgBox("Quantity can contain numbers only (0...9)", MsgBoxStyle.Information)
                txtQty.Focus()
            ElseIf Trim(txtCost.Text) = "" Then
                MsgBox("Please enter the cost of the test", MsgBoxStyle.Information)
                txtCost.Focus()
            ElseIf IsNumeric(Trim(txtCost.Text)) = False Then
                MsgBox("Cost can contain numbers only (0...9)", MsgBoxStyle.Information)
                txtCost.Focus()
            Else
                With rsDrug
                    .CancelUpdate()
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblDrugs", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    .AddNew()
                    SetDrugData()
                    .Update()
                    MsgBox("Record Saved!", MsgBoxStyle.Information)
                    btnNew.IsEnabled = True
                    btnSave.IsEnabled = False
                    btnCancel.IsEnabled = False
                    btnEdit.IsEnabled = True
                    txtName.IsEnabled = False
                    txtDescription.IsEnabled = False
                    txtCost.IsEnabled = False
                    txtAlt.IsEnabled = False
                    txtQty.IsEnabled = False
                    txtTradeName.IsEnabled = False
                    txtNotes.IsEnabled = False
                    txtUnit.IsEnabled = False
                    txtPackage.IsEnabled = False
                    btnArchive.IsEnabled = True
                End With
            End If
        End If
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        Dim Value As String
        Try
            lngCRec = rsDrug.AbsolutePosition
            Value = lblNo.Content

            With rsDrug
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub
                Else
                    .Close()
                    rsDrug = New ADODB.Recordset()
                    rsDrug.Open("SELECT * FROM tblDrugs WHERE DNO=" & Value, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    CEdit = True
                    btnSave.IsEnabled = True
                    btnCancel.IsEnabled = True
                    EditReady()
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured while preparing to edit", MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        On Error Resume Next 'change this error handling 

        If CEdit = True Then
            rsDrug.Close()

            CEdit = False

            rsDrug = New ADODB.Recordset()
            rsDrug.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsDrug.Open("SELECT *FROM tblDrugs WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            rsDrug.Move(lngCRec)
        Else

            With rsDrug
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    .CancelUpdate()
                    .MoveLast()
                    GetDrugData()
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

    End Sub


    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs) Handles btnFirst.Click
        With rsDrug
            If .RecordCount <> 0 Then
                If .BOF = True Or .EOF = True Then Exit Sub
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                        If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                            .CancelUpdate()
                            .MoveFirst()
                            btnNext.IsEnabled = True
                            btnNew.IsEnabled = True
                            GetDrugData()
                        End If
                    Else
                        MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                    End If
                Else
                    .MoveFirst()
                    btnPrevious.IsEnabled = False
                    btnNext.IsEnabled = True
                    GetDrugData()

                End If
            End If
        End With

    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click
        With rsDrug
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
                            GetDrugData()
                        Else
                            .CancelUpdate()
                            .MovePrevious()
                            btnNext.IsEnabled = True
                            btnNew.IsEnabled = True
                            GetDrugData()

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
                    GetDrugData()
                End If
            End If
        End With

    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        With rsDrug
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
                            GetDrugData()

                        Else
                            .CancelUpdate()
                            .MoveNext()
                            btnPrevious.IsEnabled = True
                            btnNew.IsEnabled = True
                            GetDrugData()

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
                    GetDrugData()

                End If
            End If
        End With

    End Sub

    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click
        With rsDrug
            If .RecordCount <> 0 Then
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                        If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                            .CancelUpdate()
                            .MoveLast()
                            btnPrevious.IsEnabled = False
                            btnNew.IsEnabled = True
                            GetDrugData()
                        End If
                    Else
                        MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")

                    End If
                Else
                    .MoveLast()
                    btnPrevious.IsEnabled = True
                    btnNext.IsEnabled = False
                    GetDrugData()
                End If
            End If
        End With

    End Sub


    Private Sub txtCost_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtCost.LostFocus

        If IsNumeric(Trim(txtCost.Text)) = False Then
            MsgBox("Cost can only have numbers (0...9)", MsgBoxStyle.Information)

        End If

    End Sub



    Private Sub btnArchive_Click(sender As Object, e As RoutedEventArgs) Handles btnArchive.Click
        With rsDrug
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblDrugs WHERE DNO=" & Val(lblNo.Content), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
            .Open("SELECT * FROM tblDrugs WHERE STATUS<>'ARCHIVED' ORDER BY DNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .RecordCount > 0 Then
                .Move(lngCRec)
                GetDrugData()
            Else
                ClearDrugData()
            End If
        End With
    End Sub
End Class
