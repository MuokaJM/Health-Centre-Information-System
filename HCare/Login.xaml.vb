Imports System.Configuration
Imports System.Data.OleDb
Imports Microsoft.Win32
Imports Microsoft.SqlServer
Imports ADODB
Imports System.IO
Imports System.Linq
Imports MahApps.Metro
Imports MahApps.Metro.Controls
Imports System.Text
Imports System.Media
Imports System.Net
Imports System.Net.Sockets
Imports System.Data.SqlClient

Imports System.Security.Cryptography



Public Class Login
    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
      Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
      Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
      Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}

    'Declarations & Variables
    Public LoginSucceeded As Boolean
    Public sUser As String
    Public intGColor As Integer
    Public LogInTrue As Boolean
    Public UserNo As String
    Public strLogIn As String
    Public Uname As String
    Public sDesig As String
    Private scPwsd As String
    Private dcsPswd As String
    Private LogCount As Short 'User name login

    Public strPswd As String

    Public bnStart As Boolean '= True 'checking startupmode
    Private m_bUserLoggedIn As Boolean

    Private computer As String = Windows.Forms.SystemInformation.ComputerName
    Private Const m_sRegKey As String = "Software\MJM Solutions\HCIS\Login Information"
    Private Const m_sRegKeyDS As String = "Software\MJM Solutions\HCIS\Default Connection"
    Private Const m_sRegKeySI As String = "Software\MJM Solutions\HCIS\Server Information"
    Private Const m_sRegKeyST As String = "Software\Alpha Solutions\HCIS\Settings"

    Private intTheme As Integer = 0
    Private strTheme As String = ""
    Private strThemeColor As String = ""

    'get the requested registry value
    Private Reg As RegistryKey
    Private regDefault As RegistryKey

    Private mainWin As New MainWindow




    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        Dim rsPswd As New ADODB.Recordset()
        Dim cnPswd As New ADODB.Connection()
        Dim rsULog As New ADODB.Recordset()
        Dim strNow As String = CStr(TimeOfDay)
        Dim Upswd As String
        Dim strRegDef As String
        Dim SNO As Long

        scPwsd = ""
        If Trim(txtUName.Text) = "" Then
            MsgBox("Please enter your user name", MsgBoxStyle.Information)
            txtUName.Focus()
            Exit Sub
        ElseIf (txtPswd.Password) = "" Then
            MsgBox("Please enter your Password", MsgBoxStyle.Information)
            txtPswd.Focus()
            Exit Sub
        End If

        If bnStart = True Then 'use normal login string
            Try
                regDefault = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyDS, True)
                strRegDef = regDefault.GetValue("UseDefault")
                regDefault.Close()
            Catch
                strRegDef = ""
            End Try

            If strRegDef = "True" Then
                regDefault.Close()
                regDefault = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeySI, True)
                If (regDefault.GetValue("Login Details")) <> "" Then
                    strPswd = regDefault.GetValue("Login Details")
                    regDefault.Close()
                End If
            Else
                strPswd = "Provider=SQLOLEDB;Data Source=(LocalDB);Initial Catalog=HCISDB;User ID=sa;Password=******"
                ' strPswd = "Provider=SQLOLEDB;Data Source=(LocalDB);Initial Catalog=HCISDB;Integrated Security=SSPI;"

            End If
        Else
            strPswd = "Provider=SQLOLEDB;Data Source=(LocalDB);Initial Catalog=HCISDB;User ID=sa;Password=******"
            ' strPswd = "Provider=SQLOLEDB;Data Source=(LocalDB);Initial Catalog=HCISDB;Integrated Security=SSPI;"

            Try
                cnPswd.Open(strPswd)
            Catch
                MsgBox(Err.Description)
                Exit Sub
            End Try

            Try
                rsPswd.CursorLocation = ADODB.CursorLocationEnum.adUseClient
                rsPswd.Open("SELECT * FROM tblUser WHERE UName='" & Trim(txtUName.Text) & "'", cnPswd, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)

            Catch ex As Exception
                MsgBox("An error has occured " & Err.Description)
                Err.Clear()
                End '
            End Try

            If rsPswd.RecordCount > 0 Then
                Uname = rsPswd.Fields("UName").Value
                Upswd = rsPswd.Fields("UPswd").Value
                sUser = Trim(rsPswd.Fields("Status").Value)
                UserNo = rsPswd.Fields("UserNo").Value
                sDesig = rsPswd.Fields("Designation").Value
                sDesig = LCase(sDesig)

                If LCase(Trim(txtUName.Text)) = LCase(Uname) Then
                    CheckPwd(txtPswd.Password)

                    scPwsd = Hash512(txtPswd.Password, "")
                    Upswd = Hash512(Upswd, CreateRandomSalt)
                    scPwsd = Hash512(scPwsd, CreateRandomSalt)
                    If scPwsd = Upswd Then
                        m_bUserLoggedIn = True

                        With rsULog
                            If .State = 1 Then .Close()
                            .CursorLocation = CursorLocationEnum.adUseClient
                            .Open("SELECT * FROM tblULog", cnPswd, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                            If .BOF = True And .EOF = True Then
                                SNO = 0
                            Else
                                .MoveLast()
                                SNO = .Fields("SNO").Value
                            End If
                            SNO = SNO + 1
                            .AddNew()
                            .Fields("SNO").Value = SNO
                            .Fields("UserNo").Value = UserNo
                            .Fields("Uname").Value = Uname
                            .Fields("uDate").Value = Today
                            .Fields("TimeIn").Value = Format(Now, "Long Time")
                            mainWin.lblTimeIn.Content = Format(Now, "Long Time")
                            .Fields("TimeCheck").Value = Format(Now, "Long Time")
                            .Fields("ComputerUSed").Value = computer
                            .Update()
                            .Close()
                        End With

                        mainWin.intGColor = intGColor
                        mainWin.Show()
                        mainWin.GetUserName(txtUName.Text)
                        mainWin.lblUser.Content = txtUName.Text
                        mainWin.strDesign = sDesig
                        Me.Hide()

                        'This should be moved and managed from database (users and roles)
                        If sUser = "Admin" Then
                            'grant full privilages
                            mainWin.patRep.IsEnabled = True
                            mainWin.Reg.Visibility = Windows.Visibility.Visible
                            mainWin.lblUserStatus.Content = "System Administrator"
                            If sDesig = "co" Or sDesig = "c.o" Or sDesig = "rco" Or sDesig = "r.c.o" Or sDesig = "clinical officer" Then
                                mainWin.OpenConsultationFile()
                            ElseIf sDesig = "pharmacist" Then
                                mainWin.OpenPharmacyFile()
                            ElseIf sDesig = "lab technologist" Or sDesig = "lab tech" Then
                                mainWin.OpenLabFile()
                            ElseIf sDesig = "receptionist" Then
                                mainWin.OpenPatientFile()
                            ElseIf sDesig = "nurse" Then
                                mainWin.OpenNurseFile()
                            Else
                                'if user status is not known or specified then end the program
                                ''   MsgBox("Non designated user", MsgBoxStyle.Critical, "Login")
                                ''  End
                            End If

                        ElseIf sUser = "sUser" Then
                            mainWin.lblUserStatus.Content = "System User"
                            If sDesig = "co" Or sDesig = "c.o" Or sDesig = "rco" Or sDesig = "r.c.o" Or sDesig = "clinical officer" Then
                                mainWin.Accounts.IsEnabled = False
                                mainWin.Patient.IsEnabled = False
                                mainWin.LabTests.IsEnabled = False
                                mainWin.Drugs.IsEnabled = False
                                mainWin.Nurse.IsEnabled = True
                                mainWin.Payment.IsEnabled = False
                                mainWin.User.IsEnabled = False
                                mainWin.patRep.IsEnabled = False
                                mainWin.PrintRcpt.IsEnabled = False
                                mainWin.SwitchDept.IsEnabled = False
                                mainWin.OpenConsultationFile()
                            ElseIf sDesig = "pharmacist" Then
                                mainWin.Accounts.IsEnabled = False
                                mainWin.Patient.IsEnabled = False
                                mainWin.LabTests.IsEnabled = False
                                mainWin.Drugs.IsEnabled = True
                                mainWin.Nurse.IsEnabled = True
                                mainWin.Consultation.IsEnabled = False
                                mainWin.Payment.IsEnabled = False
                                mainWin.Lab.IsEnabled = False
                                mainWin.SwitchDept.IsEnabled = False
                                mainWin.User.IsEnabled = False
                                mainWin.patRep.IsEnabled = False
                                mainWin.PrintRcpt.IsEnabled = False
                                mainWin.OpenPharmacyFile()
                            ElseIf sDesig = "lab technologist" Or sDesig = "lab tech" Then
                                mainWin.Accounts.IsEnabled = False
                                mainWin.Patient.IsEnabled = False
                                mainWin.Nurse.IsEnabled = True
                                mainWin.Drugs.IsEnabled = False
                                mainWin.Payment.IsEnabled = False
                                mainWin.SwitchDept.IsEnabled = False
                                mainWin.User.IsEnabled = False
                                mainWin.patRep.IsEnabled = False
                                mainWin.PrintRcpt.IsEnabled = False
                                mainWin.OpenLabFile()
                            ElseIf sDesig = "receptionist" Then
                                mainWin.ConsultationReports.IsEnabled = False
                                mainWin.PharmacyReports.IsEnabled = False
                                mainWin.Clinics.Visibility = Windows.Visibility.Collapsed
                                mainWin.Nurse.IsEnabled = False
                                mainWin.Lab.IsEnabled = False
                                mainWin.Consultation.IsEnabled = False
                                mainWin.Pharmacy.IsEnabled = False
                                mainWin.SwitchDept.IsEnabled = False
                                mainWin.ConsultationReports.IsEnabled = False
                                mainWin.User.IsEnabled = False
                                mainWin.OpenPatientFile()
                            ElseIf sDesig = "nurse" Then
                                mainWin.Accounts.IsEnabled = False
                                mainWin.Main.IsEnabled = False
                                mainWin.Nurse.IsEnabled = True
                                mainWin.SwitchDept.IsEnabled = False
                                mainWin.Payment.IsEnabled = False
                                mainWin.Pharmacy.IsEnabled = True
                                mainWin.User.IsEnabled = False
                                mainWin.patRep.IsEnabled = False
                                mainWin.PrintRcpt.IsEnabled = False
                                mainWin.OpenNurseFile()
                            Else
                                'if user status is not known or specified then end the program
                                MsgBox("Non designated user", MsgBoxStyle.Critical, "Login")
                                End
                            End If
                        Else
                            'if user status is not known or specified then end the program
                            MsgBox("Unauthorized user", MsgBoxStyle.Critical, "Login")
                            End
                        End If

                        mainWin.tmrNotify_Tick()
                        Exit Sub 'dont execute the code below after correct log in
                    Else
                        MsgBox("Wrong Usename or Password entered, Try again", MsgBoxStyle.Exclamation, "Login")
                        txtPswd.Password = ""
                        txtUName.SelectionLength = Len(txtUName.Text)
                        LogCount = LogCount + 1
                    End If
                Else
                    MsgBox("Wrong User Name or Password, Try again", MsgBoxStyle.Exclamation, "Login")
                    txtPswd.Password = ""
                    txtUName.SelectionLength = Len(txtUName.Text)
                    LogCount = LogCount + 1
                End If
            Else
                MsgBox("Wrong User Name or Password, Try again", MsgBoxStyle.Exclamation, "Login")
                LogCount = LogCount + 1
                txtPswd.Password = ""
                txtUName.SelectionLength = Len(txtUName.Text)
            End If
        End If

        If LogCount = 2 Then
            MsgBox("You have one more chance to enter correct details", MsgBoxStyle.Exclamation, "Login")
        ElseIf LogCount = 3 Then
            MsgBox("Access denied! Contact the System Administrator ", MsgBoxStyle.Critical, "Login")
            End
        End If


    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        End
    End Sub

   

    Private Sub Login_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Dim myBrush As New SolidColorBrush
        Try
            Reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
            strTheme = Reg.GetValue("Theme")
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            strThemeColor = Reg.GetValue("Color")
            Reg.Close()
            setTheme(strThemeColor, intTheme)
        Catch ex As Exception
        End Try
        txtUName.Focus()
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

        scPwsd = sHpswd

    End Sub

    Private Sub setGlow(intGColor As Integer)
        On Error Resume Next

        Select Case intGColor
           
            Case Else
        End Select
    End Sub

    Private Sub Login_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        setGlow(intGColor)
    End Sub

    Private Sub setTheme(strTheme As String, intTheme As Integer)

        Select Case strTheme
            Case "Lime"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Lime"), ThemeManager.AppThemes(intTheme))
                intGColor = 0
            Case "Green"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Green"), ThemeManager.AppThemes(intTheme))
                intGColor = 1
            Case "Emerald"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Emerald"), ThemeManager.AppThemes(intTheme))
                intGColor = 2
            Case "Teal"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Teal"), ThemeManager.AppThemes(intTheme))
                intGColor = 3
            Case "Cyan"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Cyan"), ThemeManager.AppThemes(intTheme))
                intGColor = 4
            Case "Cobalt"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Cobalt"), ThemeManager.AppThemes(intTheme))
                intGColor = 5
            Case "Indigo"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Indigo"), ThemeManager.AppThemes(intTheme))
                intGColor = 6
            Case "Violet"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Violet"), ThemeManager.AppThemes(intTheme))
                intGColor = 7
            Case "Pink"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Pink"), ThemeManager.AppThemes(intTheme))
                intGColor = 8
            Case "Magenta"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Magenta"), ThemeManager.AppThemes(intTheme))
                intGColor = 9
            Case "Crimson"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Crimson"), ThemeManager.AppThemes(intTheme))
                intGColor = 10
            Case "Red"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Red"), ThemeManager.AppThemes(intTheme))
                intGColor = 11
            Case "Orange"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Orange"), ThemeManager.AppThemes(intTheme))
                intGColor = 12
            Case "Amber"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Amber"), ThemeManager.AppThemes(intTheme))
                intGColor = 13
            Case "Yellow"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Yellow"), ThemeManager.AppThemes(intTheme))
                intGColor = 14
            Case "Brown"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Brown"), ThemeManager.AppThemes(intTheme))
                intGColor = 15
            Case "Olive"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Olive"), ThemeManager.AppThemes(intTheme))
                intGColor = 16
            Case "Steel"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Steel"), ThemeManager.AppThemes(intTheme))
                intGColor = 17
            Case "Mauve"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Mauve"), ThemeManager.AppThemes(intTheme))
                intGColor = 18
            Case "Taupe"
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Taupe"), ThemeManager.AppThemes(intTheme))
                intGColor = 19
            Case Else
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Blue"), ThemeManager.AppThemes(0))
                intGColor = 4
        End Select

    End Sub

    Private Function CreateRandomSalt() As String
        Dim mix As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_=][}{<>"
        Dim salt As String = ""
        Dim rnd As New Random
        Dim sb As New StringBuilder
        Try
            For i As Integer = 1 To 100
                Dim x As Integer = rnd.Next(0, mix.Length - 1)
                salt &= (mix.Substring(x, 1))
            Next
        Catch ex As Exception

        End Try

        Return salt
    End Function

    Private Function Hash512(password As String, salt As String) As String
        Dim convertedToBytes As Byte() = Encoding.UTF8.GetBytes(password & salt)
        Dim hashType As HashAlgorithm = New SHA512Managed()
        Dim hashBytes As Byte() = hashType.ComputeHash(convertedToBytes)
        Dim hashedResult As String = Convert.ToBase64String(hashBytes)
        Return hashedResult
    End Function
End Class
