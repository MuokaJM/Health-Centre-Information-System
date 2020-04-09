Imports ADODB

Class pgDrugsReceived

    Private rsDrug As New ADODB.Recordset
    Private rsDReceived As New ADODB.Recordset
    Private lnDNO As Long
    Private lnDRNO As Long
    Private dbCQty As Long
    Private dbNQty As Long
    Private CEdit As Boolean = False
    Private lngCRec As Long
    Private MainWin As New MainWindow
    Public strUser As String



    Private Sub pgDrugsReceived_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        lblToday.Content = Format(Today, "dd-MMMM-yy")
        btnSave.IsEnabled = False
        btnCancel.IsEnabled = False
        btnNew.IsEnabled = False
        txtQuantity.IsEnabled = False
        txtUnit.IsEnabled = False

        With rsDrug
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblDrugs ORDER BY DNO", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
            If .RecordCount > 0 Then
                .MoveFirst()
                While .EOF = False
                    cboDNo.Items.Add(.Fields("DNO").Value & " " & .Fields("DName").Value & " " & .Fields("TradeName").Value)
                    .MoveNext()
                End While
            End If
            .Close()
        End With

    End Sub

    Public Function getDrugNumber()

        Dim Mchar As String = ""
        Dim cboC As String
        Dim X As Integer
        Dim p As String = ""

        cboC = cboDNo.SelectedItem
        For X = 1 To Len(cboC)
            Mchar = Mid(cboC, X, 1)
            If Mchar = " " Then Exit For
            p = p + Mchar
        Next X
        lnDNO = Val(p)

        Return (0)
    End Function

    Private Sub cboDNo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboDNo.SelectionChanged
        getDrugNumber()

        With rsDrug
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblDrugs WHERE DNO=" & lnDNO, MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
            If .RecordCount > 0 Then
                lblDetails.Content = .Fields("DNO").Value & " " & .Fields("DName").Value & " " & .Fields("TradeName").Value
                btnNew.IsEnabled = True
            End If
            .Close()
        End With
    End Sub

    Private Sub txtQuantity_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtQuantity.LostFocus

        If IsNumeric(Trim(txtQuantity.Text)) = False Then
            MsgBox("Quantity can only contain numbers (0...9)", MsgBoxStyle.Information)
        End If
    End Sub

    Private Function SetDReceivedData()

        With rsDReceived

            .Fields("DRSNO").Value = Val(lblNo.Content)
            .Fields("Quantity").Value = Val(txtQuantity.Text)
            .Fields("DNO").Value = lnDNO
            .Fields("RDate").Value = Today
            .Fields("Unit").Value = txtUnit.Text
            .Fields("CQTY").Value = dbCQty
            .Fields("NQTY").Value = dbNQty
            .Fields("Uname").Value = strUser
        End With


        Return (0)
    End Function

    Private Function ClearDReceivedData()

        lblNo.Content = ""
        lblDetails.Content = ""
        txtQuantity.Text = ""
        txtUnit.Text = ""

        Return (0)
    End Function

    Private Function GetDReceivedData()

        With rsDReceived
            lblNo.Content = .Fields("DRSNO").Value

            If IsDBNull(.Fields("Quantity").Value) = True Or Val(.Fields("Quantity").Value) = 0 Then
                txtQuantity.Text = ""
            Else
                txtQuantity.Text = .Fields("Quantity").Value
            End If
            txtUnit.Text = .Fields("Unit").Value

            lblToday.Content = Format(.Fields("DDate").Value, "Short Date")

        End With

        Return (0)
    End Function

    Private Sub btnNew_Click(sender As Object, e As RoutedEventArgs) Handles btnNew.Click
        GenerateDrugRecievedNo()
        lblNo.Content = lnDRNO
        btnSave.IsEnabled = True
        btnNew.IsEnabled = False
        cboDNo.IsEnabled = False
        txtQuantity.IsEnabled = True
        txtUnit.IsEnabled = True
    End Sub

    Private Sub GenerateDrugRecievedNo()
        Dim rsDR As New ADODB.Recordset
        With rsDR
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblDrugsReceived ORDER BY DRSNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
            If .BOF = True And .EOF = True Then
                lnDRNO = 0
            Else
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                .MoveLast()
                lnDRNO = .Fields("DRSNo").Value
            End If
            lnDRNO = lnDRNO + 1
            .Close()
        End With
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Dim rsD As New ADODB.Recordset

        With rsD
            .Open("SELECT DNO, Quantity FROM tblDrugs WHERE DNO=" & lnDNO, MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
            dbCQty = .Fields("Quantity").Value
            .Close()
        End With

        dbNQty = dbCQty + Val(Trim(txtQuantity.Text))

        If Trim(txtQuantity.Text) = "" Then
            MsgBox("Please enter the received quantity", MsgBoxStyle.Information)
            txtQuantity.Focus()
        ElseIf IsNumeric(Trim(txtQuantity.Text)) = False Then
            MsgBox("Quantity can only contain numbers (0...9)", MsgBoxStyle.Information)
            txtQuantity.Focus()
        ElseIf Trim(txtUnit.Text) = "" Then
            MsgBox("Please enter measuring unit of drugs received", MsgBoxStyle.Information)
            txtUnit.Focus()
        Else
            With rsDReceived
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblDrugsReceived ORDER BY DRSNO", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                .AddNew()
                SetDReceivedData()
                .Update()
                With rsD
                    .Open("SELECT DNO, Quantity FROM tblDrugs WHERE DNO=" & lnDNO, MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockPessimistic)
                    .Fields("Quantity").Value = dbNQty
                    .Update()
                    .Close()
                End With

                MsgBox("Record Saved")
                txtQuantity.IsEnabled = False
                txtUnit.IsEnabled = False
                btnSave.IsEnabled = False
                btnCancel.IsEnabled = False
                cboDNo.IsEnabled = True
            End With
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        If CEdit = True Then
            If rsDReceived.State = 1 Then rsDReceived.Close()
            CEdit = False

            rsDReceived = New ADODB.Recordset()
            rsDReceived.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsDReceived.Open("SELECT * FROM tblDrugsReceived", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            rsDReceived.Move(lngCRec)
        Else

            With rsDReceived
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    .CancelUpdate()
                    .MoveLast()
                    GetDReceivedData()
                Else
                    MsgBox("Nothing to Cancel")
                    txtQuantity.Focus()
                End If
            End With

        End If
        btnSave.IsEnabled = False
        btnCancel.IsEnabled = False

    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click

    End Sub
End Class
