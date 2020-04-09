
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


Class pgImpression

    Inherits System.Windows.Controls.Page
    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
     Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
     Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
     Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}

    Private myBrush As New SolidColorBrush
    Public intTheme As Integer
    Private strColor As String
    Public dgBrush As New SolidColorBrush

    Private notifyIcon As New System.Windows.Forms.NotifyIcon()



    Private rsImpression As New ADODB.Recordset
    Private lnIMNO As Long
    Private CEdit As Boolean = False
    Private lngCRec As Long
    Private MainWin As New MainWindow
    Public strUser As String



    Private Sub pgImpression_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        lblToday.Content = Format(Today, "dd-MMMM-yy")
        btnSave.IsEnabled = False
        btnCancel.IsEnabled = False
        txtImpression.IsEnabled = False
        txtSymptoms.IsEnabled = False


        Try
            With rsImpression
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblImpression  WHERE STATUS<>'ARCHIVED' ORDER BY IMNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            End With
        Catch
            MsgBox("An error has occured during form load ", Err.Description)
        End Try
    End Sub



    Private Function SetTestData()
        Try
            With rsImpression
                .Fields("IMNO").Value = lnIMNO
                .Fields("Impression").Value = txtImpression.Text
                .Fields("Symptoms").Value = txtSymptoms.Text
                .Fields("status").Value = "ON"
                .Fields("UName").Value = strUser
            End With
        Catch
            MsgBox("An error has occured while setting data for saving ", Err.Description)
        End Try

        Return (0)
    End Function

    Private Function ClearTestData()

        txtImpression.Text = ""
        txtSymptoms.Text = ""
        lblRecNo.Content = ""
        lblNo.Content = ""

        Return (0)
    End Function

    Private Function GetTestData()
        Try
            With rsImpression
                lblNo.Content = .Fields("IMNO").Value
                txtImpression.Text = .Fields("Impression").Value
                txtSymptoms.Text = .Fields("Symptoms").Value
                lblRecNo.Content = "Record " & .AbsolutePosition & " of " & .RecordCount & " Records"
            End With
        Catch
            MsgBox("An error has occured while fetching data ", Err.Description)
        End Try
        Return (0)
    End Function

    Private Sub btnNew_Click(sender As Object, e As RoutedEventArgs) Handles btnNew.Click
        Try
            With rsImpression
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblImpression ORDER BY IMNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .BOF And .EOF Then
                    lnIMNO = 0
                Else
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                    .MoveLast()
                    lnIMNO = .Fields("IMNo").Value
                End If
                .AddNew()
                lnIMNO = lnIMNO + 1
                ClearTestData()
                lblNo.Content = lnIMNO

            End With
        Catch
            MsgBox("An error has occured while generating new record ", Err.Description)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Try
            If CEdit = True Then
                SetTestData()
                rsImpression.Update()

                MsgBox("Test " & txtImpression.Text & " Record Saved", MsgBoxStyle.Information, "Save")
                rsImpression.Close()

                CEdit = False

                rsImpression = New ADODB.Recordset()
                rsImpression.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsImpression.Open("SELECT * FROM tblImpression  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsImpression.Move(lngCRec)
                btnNew.IsEnabled = True
                btnSave.IsEnabled = False
                btnEdit.IsEnabled = True
            ElseIf rsImpression.EditMode = ADODB.EditModeEnum.adEditNone Then
                MsgBox("No changes have been made add a new record or edit this record then save again", MsgBoxStyle.Information, "Save")
                btnNew.IsEnabled = True
                btnSave.IsEnabled = False
            Else

                If Trim(txtImpression.Text) = "" Then
                    MsgBox("Please enter the name of the impression", MsgBoxStyle.Information)
                    txtImpression.Focus()
                ElseIf Trim(txtSymptoms.Text) = "" Then
                    MsgBox("Please enter the  impression symptoms", MsgBoxStyle.Information)
                    txtSymptoms.Focus()
                Else
                    With rsImpression
                        .CancelUpdate()
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblImpression  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        .AddNew()
                        SetTestData()
                        .Update()
                        MsgBox("Record Saved!", MsgBoxStyle.Information)
                        btnNew.IsEnabled = True
                        btnSave.IsEnabled = False
                        btnCancel.IsEnabled = False
                        btnEdit.IsEnabled = True
                        txtImpression.IsEnabled = False
                        txtSymptoms.IsEnabled = False
                        btnArchive.IsEnabled = True
                    End With
                End If
            End If
        Catch
            MsgBox("An error has occured while saving data ", Err.Description)
        End Try
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        Dim Value As String
        lngCRec = rsImpression.AbsolutePosition
        Value = lblNo.Content
        lnIMNO = Value
        Try
            With rsImpression
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub
                Else
                    .Close()
                    rsImpression = New ADODB.Recordset()
                    rsImpression.Open("SELECT * FROM tblImpression WHERE IMNO=" & Value, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
            btnSave.IsEnabled = True
            btnEdit.IsEnabled = False
            btnCancel.IsEnabled = True
            btnNew.IsEnabled = False
            txtImpression.IsEnabled = True
            txtSymptoms.IsEnabled = True
            txtImpression.Focus()
        Catch
            MsgBox("An error has occured while preparing to edit ", MsgBoxStyle.Exclamation)
        End Try

    End Sub


    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Try
            If CEdit = True Then
                rsImpression.Close()
                CEdit = False

                rsImpression = New ADODB.Recordset()
                rsImpression.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsImpression.Open("SELECT * FROM tblImpression  WHERE STATUS<>'ARCHIVED'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                rsImpression.Move(lngCRec)
            Else

                With rsImpression
                    If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                        .CancelUpdate()
                        .MoveLast()
                        GetTestData()
                    Else
                        MsgBox("Nothing to Cancel")
                        Me.txtImpression.Focus()
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
            With rsImpression
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
            With rsImpression
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
            With rsImpression
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
            With rsImpression
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







    Private Sub btnArchive_Click(sender As Object, e As RoutedEventArgs) Handles btnArchive.Click
        Try
            With rsImpression
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblImpression WHERE IMNO=" & Val(lblNo.Content), MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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
                .Open("SELECT * FROM tblImpression  WHERE STATUS<>'ARCHIVED' ORDER BY IMNO", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
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

