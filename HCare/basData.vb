'Option Strict Off
Option Explicit On 
Imports ADODB
'Imports System.Windows.Forms.Application
Imports Microsoft.VisualBasic

'Imports System.

Module basData
    Public strConn As String
    Public stNo As String
    Public PNo As String
    Public SNo As String
    Public lnFPNo As Long
    Public lnFileSz As Long
    Public imgPic As Object
    Public byPic As Byte
    Public mStream As New ADODB.Stream()
    Public strPath As String
    Public bnGetPic As Boolean

    Public pgPat As New pgPatient
    Public MainW As New MainWindow
    '#Private sUsers As 

    Public Function GetPatientData() As Object
        'On Error Resume Next

        With pgPat
            .lblPNo.Content = .rsPatient.Fields("Pno").Value
            .txtSName.Text = .rsPatient.Fields("Surname").Value
            .dtpDoB.SelectedDate = .rsPatient.Fields("dob").Value
            .lblToday.Content = .rsPatient.Fields("VDate").Value
            .txtONames.Text = .rsPatient.Fields("Onames").Value
            If .rsPatient.Fields("sex").Value = "Male" Then
                .optMale.IsChecked = True
            Else
                .optFemale.IsChecked = True
            End If
            .txtAddress.Text = .rsPatient.Fields("Address").Value
            .txtPhone.Text = .rsPatient.Fields("phone").Value
            .txtSLocation.Text = .rsPatient.Fields("SubLoc").Value
            .lblRecNo.Content = "Record " & .rsPatient.AbsolutePosition & " Of " & .rsPatient.RecordCount & " Records"
        End With
        Return (0)
    End Function

    Public Function ClearPatientData() As Object
        On Error Resume Next
        With pgPat
            .lblPNo.Content = ""
            .txtSName.Text = ""
            .dtpDoB.SelectedDate = ""
            .optFemale.IsChecked = False
            .optMale.IsChecked = False
            .txtONames.Text = ""
            .txtSName.Text = ""
            .txtAddress.Text = ""
            .txtPhone.Text = ""
            .txtSLocation.Text = ""
            .lblRecNo.Content = ""
        End With
        Return (0)
    End Function

    Public Function SetPatientData() As Object
        '  On Error Resume Next
        Dim cn As New ADODB.Connection()
        Dim rstMember As New ADODB.Recordset()
        Dim rsP As New ADODB.Recordset
        With rsP

            If .State = 1 Then .Close()
            .CursorLocation = ADODB.CursorLocationEnum.adUseClient
            .Open("SELECT * FROM tblPatient", MainW.cnHCare, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
            .AddNew()

            .Fields("PNo").Value = Val(pgPat.lblPNo.Content)
            .Fields("Surname").Value = pgPat.txtSName.Text
            .Fields("dob").Value = pgPat.dtpDoB.SelectedDate
            .Fields("VDate").Value = Today
            .Fields("Onames").Value = pgPat.txtONames.Text
            .Fields("surname").Value = pgPat.txtSName.Text
            If pgPat.optMale.IsChecked = True Then
                .Fields("sex").Value = "Male"
            Else
                .Fields("sex").Value = "Female"
            End If

            .Fields("Address").Value = pgPat.txtAddress.Text
            .Fields("phone").Value = pgPat.txtPhone.Text
            .Fields("SubLoc").Value = pgPat.txtSLocation.Text
            .Fields("UName").Value = MainW.UserNo
            .Update()
            'basData.SetPatientData()
            '.Update()
        End With

        With pgPat
            .rsPatient.Fields("PNo").Value = Val(.lblPNo.Content)
            .rsPatient.Fields("Surname").Value = .txtSName.Text
            .rsPatient.Fields("dob").Value = .dtpDoB.SelectedDate
            .rsPatient.Fields("VDate").Value = Today
            .rsPatient.Fields("Onames").Value = .txtONames.Text
            .rsPatient.Fields("surname").Value = .txtSName.Text
            If .optMale.IsChecked = True Then
                .rsPatient.Fields("sex").Value = "Male"
            Else
                .rsPatient.Fields("sex").Value = "Female"
            End If

            .rsPatient.Fields("Address").Value = .txtAddress.Text
            .rsPatient.Fields("phone").Value = .txtPhone.Text
            .rsPatient.Fields("SubLoc").Value = .txtSLocation.Text
            .rsPatient.Fields("UName").Value = MainW.UserNo
            .rsPatient.Update()

        End With
        Return (0)
    End Function


    'Public Function GetLogData() As Object
    '    On Error Resume Next

    '    On Error Resume Next
    '    With frmLog.DefInstance
    '        .txtDate.Text = .rsLog.Fields("Date").Value
    '        .txtLogIn.Text = .rsLog.Fields("LogIn").Value
    '        .txtLogOut.Text = .rsLog.Fields("logOut").Value
    '        .txtUser.Text = .rsLog.Fields("User").Value
    '    End With
    '    Return (0)
    'End Function
    'Public Function GetUsersData() As Object
    '    On Error Resume Next

    '    With frmUsers
    '        .txtAdmin.Text = .rspswd.Fields("Admin").Value
    '        .txtDOJ.Text = .rspswd.Fields("DoJ").Value
    '        .txtPswd.Text = .rspswd.Fields("Pswd").Value
    '        .txtUAR.Text = .rspswd.Fields("uType").Value
    '        .txtUName.Text = .rspswd.Fields("UName").Value
    '        .txtUserNo.Text = .rspswd.Fields("UNo").Value
    '        .txtSName.Text = .rspswd.Fields("Surname").Value
    '        .txtONames.Text = .rspswd.Fields("Onames").Value
    '    End With
    '    Return (0)
    'End Function

    'Public Function SetUsersData() As Object
    '    With frmUsers
    '        .rspswd.Fields("Admin").Value = .txtAdmin.Text
    '        .rspswd.Fields("DoJ").Value = .txtDOJ.Text
    '        .rspswd.Fields("Pswd").Value = .txtPswd.Text
    '        .rspswd.Fields("uType").Value = .txtUAR.Text
    '        .rspswd.Fields("Uname").Value = .txtUName.Text
    '        .rspswd.Fields("UNo").Value = .txtUserNo.Text
    '        .rspswd.Fields("Surname").Value = .txtSName.Text
    '        .rspswd.Fields("Onames").Value = .txtONames.Text
    '    End With
    '    Return (0)
    'End Function




    Public Function GetServiceData() As Object
        'On Error Resume Next

        'With frmServices
        '.txtSNo.Text = .rsServices.Fields("sno").Value
        '.txtSName.Text = .rsServices.Fields("Sname").Value
        '.txtDescrip.Text = .rsServices.Fields("Description").Value
        '.txtCost.Text = .rsServices.Fields("Cost").Value
        '.lblRecNo.Visible = True
        '.lblRecNo.Text = "Record " & .rsServices.AbsolutePosition & " Of " & .rsServices.RecordCount & " Records"
        '.Cursor = System.Windows.Forms.Cursors.Arrow
        'E() 'nd With
        Return (0)
    End Function

    Public Function ClearServiceData() As Object
        On Error Resume Next
        'With frmServices
        '.txtSNo.Text = ""
        '.txtSName.Text = ""
        '' .txtDescrip.Text = ""
        '.txtSName.Text = ""
        '.txtCost.Text = ""
        '.lblRecNo.Text = ""
        'End With
        Return (0)
    End Function

    Public Function SetMemberServiceData() As Object
        '  On Error Resume Next
        Dim cn As New ADODB.Connection()
        Dim rsService As New ADODB.Recordset()
        Dim rsMS As New ADODB.Recordset()
        Dim Mchar As String = ""
        ' Dim cboC As String
        'Dim X As Integer

        'With frmMembersService
        '.rsPatientService.Fields("msno").Value = .txtMSNo.Text
        'get member no only
        'cboC = .cboMNo.Text
        'For X = 1 To Len(cboC)
        'Mchar = Mid(cboC, X, 1)
        'If Mchar = " " Then Exit For
        'mNo = mNo + Mchar
        'Next X
        '.rsPatientService.Fields("MNo").Value = MNo
        'get service no only
        'Mchar = ""
        'mNo = ""
        'cboC = .cboService.Text
        'For X = 1 To Len(cboC)
        'Mchar = Mid(cboC, X, 1)
        'If Mchar = " " Then Exit For
        'SNo = SNo + Mchar
        'Next X
        '.rsPatientService.Fields("SNO").Value = SNo
        '.rsPatientService.Fields("SDate").Value = Today '.dtpSD.Value
        '.rsPatientService.Fields("Cost").Value = .txtCost.Text 'cCost
        '.rsPatientService.Fields("Persons").Value = .txtPersons.Text
        '.rsPatientService.Fields("uNo").Value = mdi.UserNo
        '.rsPatientService.Update()
        'SNo = ""
        'End With
        Return (0)
    End Function

    Public Function GetMemberServiceData() As Object
        'On Error Resume Next

        'With frmMembersService
        '.txtMSNo.Text = .rsPatientService.Fields("msno").Value
        '.cboMNo.Text = .rsPatientService.Fields("mno").Value
        '.cboService.Text = .rsPatientService.Fields("sno").Value
        '.txtCost.Text = .rsPatientService.Fields("Cost").Value
        '.dtpSD.Value = .rsPatientService.Fields("SDate").Value
        '.txtPersons.Text = .rsPatientService.Fields("Persons").Value
        ' .lblRecNo.Visible = True
        '  .lblRecNo.Text = "Record " & .rsPatientService.AbsolutePosition & " Of " & .rsPatientService.RecordCount & " Records"
        '   .Cursor = System.Windows.Forms.Cursors.Arrow
        '    End With
        Return (0)
    End Function

    Public Function ClearMemberServiceData() As Object
        On Error Resume Next
        'With frmMembersService
        '.txtMSNo.Text = ""
        '.cboMNo.Text = "Select Member No"
        '.cboService.Text = "Select Service No"
        '.txtCost.Text = "0"
        '.lblRecNo.Text = ""
        '.txtPersons.Text = "1"
        'End With
        Return (0)
    End Function

    Public Function SetServiceData() As Object
        '  On Error Resume Next
        Dim cn As New ADODB.Connection()
        Dim rsService As New ADODB.Recordset()

        'With frmServices
        '.rsServices.Fields("sno").Value = .txtSNo.Text
        '.rsServices.Fields("Sname").Value = .txtSName.Text
        '.rsServices.Fields("Description").Value = .txtDescrip.Text
        '.rsServices.Fields("Cost").Value = .txtCost.Text

        '.rsServices.Update()
        '     End With
        Return (0)
    End Function
    Public Function GetUsersData() As Object
        On Error Resume Next

        'With frmUsers
        '.txtAdmin.Text = .rspswd.Fields("Admin").Value
        '.txtDOJ.Text = .rspswd.Fields("DoJ").Value
        '.txtPswd.Text = .rspswd.Fields("Pswd").Value
        '.txtUAR.Text = .rspswd.Fields("uType").Value
        '.txtUName.Text = .rspswd.Fields("UName").Value
        '.txtUserNo.Text = .rspswd.Fields("UNo").Value
        '.txtSName.Text = .rspswd.Fields("Surname").Value
        '.txtONames.Text = .rspswd.Fields("Onames").Value
        'End With
        Return (0)
    End Function

    Public Function SetUsersData() As Object
        'With frmUsers
        '.rspswd.Fields("Admin").Value = .txtAdmin.Text
        '.rspswd.Fields("DoJ").Value = .txtDOJ.Text
        '.rspswd.Fields("Pswd").Value = .txtPswd.Text
        '.rspswd.Fields("uType").Value = .txtUAR.Text
        '.rspswd.Fields("Uname").Value = .txtUName.Text
        '.rspswd.Fields("UNo").Value = .txtUserNo.Text
        '.rspswd.Fields("Surname").Value = .txtSName.Text
        '.rspswd.Fields("Onames").Value = .txtONames.Text
        'End With
        Return (0)
    End Function


End Module