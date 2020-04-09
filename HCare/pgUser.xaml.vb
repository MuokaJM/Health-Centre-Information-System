Imports System.Security.Cryptography
Imports System.Text

Class pgUser
    Private lngCRec As Long 'current record
    Private CEdit As Boolean
    Private iAns As Integer
    Private bnfrmP As Boolean
    Private MainWin As New MainWindow
    Public rsUser As New ADODB.Recordset()
    Private BNO As Integer
    Private bDate As String
    Private BDetNo As Integer
    Private SMNo As Integer
    Private scPswd As String
    Private cPswd As String
    Private sL As New Login
    Public strUser As String


    Private Sub pgUser_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        lblToday.Content = Format(Today, "dd-MMMM-yy")
        Try
            With rsUser
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblUser WHERE USTATUS<>'ARCHIVED'  ORDER BY UserNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then GetUserData()
            End With
        Catch
            MsgBox("An error has occured during form load " & Err.Description)
        End Try


        Try
            'This should not be hard-coded
            cboDesign.Items.Add("Clinical Officer")
            cboDesign.Items.Add("Lab Technologist")
            cboDesign.Items.Add("Nurse")
            cboDesign.Items.Add("Pharmacist")
            cboDesign.Items.Add("Receptionist")
            txtAdmin.IsEnabled = False
            txtCPswd.IsEnabled = False
            txtFName.IsEnabled = False
            txtPswd.IsEnabled = False
            txtStatus.IsEnabled = False
            txtUName.IsEnabled = False

        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub btnNew_Click(sender As Object, e As RoutedEventArgs) Handles btnNew.Click
        Dim UserNo As Integer

        With rsUser
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblUser ORDER BY UserNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            If .BOF And .EOF Then
                UserNo = 0
            Else
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then .CancelUpdate()
                .MoveLast()

                UserNo = .Fields("UserNo").Value
            End If
            .AddNew()

            UserNo = UserNo + 1

            btnSave.IsEnabled = True
            btnCancel.IsEnabled = True
            btnNew.IsEnabled = False
            ClearUserData()
            lblToday.Content = Today
            lblUserNo.Content = UserNo
            txtCPswd.IsEnabled = True
            txtFName.IsEnabled = True
            txtPswd.IsEnabled = True
            txtUName.IsEnabled = True
            txtStatus.Text = "sUser"
            Me.txtAdmin.Text = strUser
            Me.txtFName.Focus()
        End With

    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If CEdit = True Then
            SetUserData()
            rsUser.Update()

            MsgBox("User " & cboDesign.Text & " " & Me.txtFName.Text & " Record Saved", MsgBoxStyle.Information, "Save")
            rsUser.Close()

            CEdit = False

            rsUser = New ADODB.Recordset()
            rsUser.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsUser.Open("SELECT * FROM tblUser WHERE USTATUS<>'ARCHIVED' ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            rsUser.Move(lngCRec)
            btnNew.IsEnabled = True
            btnSave.IsEnabled = False
        ElseIf rsUser.EditMode = ADODB.EditModeEnum.adEditNone Then
            MsgBox("No changes have been made add a new record or edit this record then save again", MsgBoxStyle.Information, "Save")
            btnNew.IsEnabled = True
            btnSave.IsEnabled = False
        Else

            If Me.txtFName.Text = "" Then
                MsgBox("Please enter the User's Fullnames", MsgBoxStyle.Information)
                txtFName.Focus()
            ElseIf cboDesign.Text = "" Then
                MsgBox("Please select the user's designation (DR, CO, Nurse etc)", MsgBoxStyle.Information)
                cboDesign.Focus()


            ElseIf Me.txtUName.Text = "" Then
                MsgBox("Please enter the  User Name", MsgBoxStyle.Information)
                Me.txtUName.Focus()


            Else
                With rsUser
                    .CancelUpdate()
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT * FROM tblUser WHERE USTATUS<>'ARCHIVED' ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    .AddNew()
                    SetUserData()
                    .Update()

                    MsgBox("Record Saved!", MsgBoxStyle.Information)
                    btnNew.IsEnabled = True
                    btnSave.IsEnabled = False
                End With
            End If
        End If

    End Sub



    Private Sub btnFind_Click(sender As Object, e As RoutedEventArgs) Handles btnFind.Click
        Dim nwWin As New Window1
        Dim fiS As New Frame
        Dim ti As New TabItem

        fiS.NavigationService.Navigate(New pgPatSearch)
        ti.Content = fiS

        nwWin.tcSearch.Items.Add(ti)

        nwWin.Show()

    End Sub

    Private Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click

        Dim Value As String

        lngCRec = rsUser.AbsolutePosition
        Value = lblUserNo.Content
        Try
            With rsUser
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    MsgBox("Editing is not possible now")
                    Exit Sub

                Else
                    .Close()
                    rsUser = New ADODB.Recordset()
                    rsUser.Open("SELECT *FROM tblUser WHERE UserNo=" & Value, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                    CEdit = True
                    btnSave.IsEnabled = True
                    btnCancel.IsEnabled = True
                    txtFName.IsEnabled = False
                    txtUName.IsEnabled = False
                    txtPswd.IsEnabled = False
                    txtCPswd.IsEnabled = False
                End If
            End With
        Catch ex As Exception
            MsgBox("An error has occured during edit " & Err.Description)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        If CEdit = True Then
            rsUser.Close()

            CEdit = False

            rsUser = New ADODB.Recordset()
            rsUser.CursorLocation = ADODB.CursorLocationEnum.adUseClient
            rsUser.Open("SELECT * FROM tblUser WHERE USTATUS<>'ARCHIVED' ", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            rsUser.Move(lngCRec)
        Else

            With rsUser
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    .CancelUpdate()
                    .MoveLast()
                    GetUserData()
                Else
                    MsgBox("Nothing to Cancel")
                    Me.txtFName.Focus()
                End If
            End With

        End If
        btnSave.IsEnabled = False
        btnNew.IsEnabled = True
        btnCancel.IsEnabled = False
        btnNew.Focus()


    End Sub

    Private Sub btnArchive_Click(sender As Object, e As RoutedEventArgs) Handles btnArchive.Click
        Dim rsCheckUser As New ADODB.Recordset
        Try
            With rsUser
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblUser WHERE UserNo=" & lblUserNo.Content, MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                lngCRec = .AbsolutePosition
                If .RecordCount > 0 Then
                    If .Fields("Uname").Value = strUser Then
                        MsgBox("You cannot archive your own account login using different account to archive '" & strUser & "' user name", MsgBoxStyle.Information)
                        Exit Sub
                    End If
                    With rsCheckUser
                        If .State = 1 Then .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblUser WHERE Status='Admin'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        If .RecordCount = 1 Then
                            MsgBox("Only one user is remaining in the database, add another account to archive this user", MsgBoxStyle.Exclamation)
                            Exit Sub
                        End If
                        rsCheckUser.Close()
                    End With
                Else
                    If MsgBox("Do you really want to archive this record? ", MsgBoxStyle.YesNo) = vbYes Then
                        .Fields("ustatus").Value = "Archived"
                        .Update()
                        MsgBox("Record archived!", MsgBoxStyle.Exclamation)
                        .Close()
                        .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                        .Open("SELECT * FROM tblUser WHERE USTATUS<>'ARCHIVED'  ORDER BY UserNo", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                        .Move(lngCRec)
                        GetUserData()
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub

    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs) Handles btnFirst.Click
        With rsUser
            If .RecordCount <> 0 Then
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                        If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                            .CancelUpdate()
                            .MoveFirst()
                            btnNext.IsEnabled = True
                            btnNew.IsEnabled = True
                            GetUserData()

                        End If
                    Else
                        MsgBox("Can't Go To first Record!", MsgBoxStyle.Exclamation, "Navigation")
                    End If
                Else
                    .MoveFirst()
                    btnPrevious.IsEnabled = False
                    btnNext.IsEnabled = True
                    GetUserData()

                End If
            End If
        End With

    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As RoutedEventArgs) Handles btnPrevious.Click
        With rsUser
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
                            GetUserData()

                        Else
                            .CancelUpdate()
                            .MovePrevious()
                            btnNext.IsEnabled = True
                            btnNew.IsEnabled = True
                            GetUserData()

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
                    GetUserData()

                End If
            End If
        End With

    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs) Handles btnNext.Click
        With rsUser
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
                            GetUserData()

                        Else
                            .CancelUpdate()
                            .MoveNext()
                            btnPrevious.IsEnabled = True
                            btnNew.IsEnabled = True
                            GetUserData()

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
                    GetUserData()

                End If
            End If
        End With

    End Sub

    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs) Handles btnLast.Click
        With rsUser
            If .RecordCount <> 0 Then
                If .EditMode <> ADODB.EditModeEnum.adEditNone Then
                    If MsgBox("Add New Or Edit in Progress! " & Chr(10) & Chr(13) & "Do You Wan't To Cancel It?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Navigation") = MsgBoxResult.Yes Then
                        If .EditMode = ADODB.EditModeEnum.adEditAdd Then
                            .CancelUpdate()
                            .MoveLast()
                            btnPrevious.IsEnabled = False
                            btnNew.IsEnabled = True
                            GetUserData()
                        End If
                    Else
                        MsgBox("Can't Go To last Record!", MsgBoxStyle.Exclamation, "Navigation")

                    End If
                Else
                    .MoveLast()
                    btnPrevious.IsEnabled = True
                    btnNext.IsEnabled = False
                    GetUserData()

                End If
            End If
        End With

    End Sub

    Private Function SetUserData()

        With rsUser

            .Fields("UserNo").Value = Val(lblUserNo.Content)
            .Fields("FNames").Value = txtFName.Text
            .Fields("JDate").Value = Today
            .Fields("Designation").Value = cboDesign.SelectedValue
            .Fields("UName").Value = txtUName.Text
            .Fields("UPswd").Value = Hash512(txtPswd.Password, "")
            'scPswd
            .Fields("Status").Value = txtStatus.Text
            .Fields("Admin").Value = strUser
            .Fields("uStatus").Value = "On"
        End With


        Return (0)
    End Function

    Private Function ClearUserData()

        lblUserNo.Content = ""
        txtFName.Text = ""
        cboDesign.Text = ""
        txtFName.Text = ""
        txtUName.Text = ""
        txtPswd.Password = ""
        txtCPswd.Password = ""
        txtStatus.Text = ""
        txtAdmin.Text = ""

        Return (0)
    End Function

    Private Function GetUserData()
        Try


            With Me
                .lblUserNo.Content = .rsUser.Fields("UserNo").Value
                .txtFName.Text = .rsUser.Fields("FNames").Value
                .lblToday.Content = .rsUser.Fields("JDate").Value
                .cboDesign.Text = .rsUser.Fields("Designation").Value
                .txtUName.Text = .rsUser.Fields("UName").Value
                .txtStatus.Text = .rsUser.Fields("Status").Value
                .txtAdmin.Text = .rsUser.Fields("Admin").Value
            End With
        Catch ex As Exception
            MsgBox("An error has occured while fetching data " & Err.Description)
        End Try
        Return (0)
    End Function




    Private Sub txtCPswd_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtCPswd.LostFocus

        If Trim(txtPswd.Password) <> "" Then
            cPswd = scPswd
            scPswd = ""
            CheckPwd(txtCPswd.Password)
            If cPswd <> scPswd Then
                MsgBox("The password does not match")
                txtCPswd.Password = ""
                txtPswd.Password = ""
                txtPswd.Focus()
            End If
        End If
    End Sub

    Private Sub txtUName_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtUName.LostFocus
        Dim rsCPswd As New ADODB.Recordset
        With rsCPswd
            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT UName FROM tblUser  WHERE UName='" & txtUName.Text & "'", MainWin.cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
            If .RecordCount > 0 Then
                MsgBox("That User Name is already in use, pick different user name")
                txtUName.Text = ""
            End If
            .Close()
        End With
    End Sub

   
    Private Sub txtPswd_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtPswd.LostFocus
        If Trim(txtPswd.Password) <> "" Then
            CheckPwd(txtPswd.Password)
        End If

    End Sub

    Private Sub CheckPwd(sPw As String)
        Dim sPswd As String = ""
        Dim X As Integer
        Dim Xe As Integer
        Dim Xo As Integer
        Dim sHpswd As String = ""
        Dim Mchar As String = ""
        Dim strName As String
        Dim strO As String = ""
        Dim strE As String = ""


        strName = sPw
        For X = 1 To Len(strName)
            Mchar = Mid(strName, X, 1)
            If X Mod 2 = 0 Then
                strE = strE & Asc(Mchar)
            ElseIf X Mod 2 <> 0 Then
                strO = strO & Asc(Mchar)
            End If
        Next X

        Xe = 1
        Xo = 1
        X = 1
        While Len(sPswd) <> Len(strE & strO)

            If X Mod 2 = 0 Then
                Mchar = Mid(strO, Xo, 1)
                Xo = Xo + 1
            ElseIf X Mod 2 <> 0 Then
                Mchar = Mid(strE, Xe, 1)
                Xe = Xe + 1
            End If
            sPswd = sPswd & Mchar
            X = X + 1
        End While

        For X = 1 To Len(sPswd)
            Mchar = Mid(sPswd, X, 4)
            sHpswd = sHpswd & Hex(Mchar)
            X = X + 3
        Next X

        scPswd = sHpswd



    End Sub

    Private Function Hash512(password As String, salt As String) As String
        Dim convertedToBytes As Byte() = Encoding.UTF8.GetBytes(password & salt)
        Dim hashType As HashAlgorithm = New SHA512Managed()
        Dim hashBytes As Byte() = hashType.ComputeHash(convertedToBytes)
        Dim hashedResult As String = Convert.ToBase64String(hashBytes)
        Return hashedResult
    End Function

End Class
