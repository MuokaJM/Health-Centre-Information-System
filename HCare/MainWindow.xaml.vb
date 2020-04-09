Imports MahApps.Metro
Imports MahApps.Metro.Controls
Imports CrystalDecisions.Shared
Imports CrystalDecisions.CrystalReports.Engine
Imports System.Windows.Threading
Imports System.Reflection.Assembly
Imports System.Diagnostics.FileVersionInfo
Imports System.Drawing.Icon
Imports ADODB
Imports Microsoft.Win32
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System
Imports System.IO
Imports Microsoft.VisualBasic
Imports System.Threading
Imports System.Windows.Navigation



Class MainWindow
    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
      Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
      Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
      Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}

    Private regTheme As RegistryKey
    Private Const m_sRegKeyST As String = "Software\Alpha Solutions\HCIS\Settings"
    Private regNotify As RegistryKey
    Private Const m_sRegKeyNT As String = "Software\Alpha Solutions\HCIS\Settings" 'for notification "on/off"
    Private strNotify As String
    Private Const m_sRegKeyNTT As String = "Software\Alpha Solutions\HCIS\Settings" 'for notification time in minutes
    Private strTime As String
    Private Const m_sRegKeyNTS As String = "Software\Alpha Solutions\HCIS\Settings" 'for notification Sound
    Private strSound As String

    ' Private regNotify 
    Private myBrush As New SolidColorBrush
    Private strTheme As String = ""
    Private intTheme As Integer = 0
    Private strColor As String
    Public intGColor As Integer



    Dim Listener As New TcpListener(65535)
    Dim Client As New TcpClient
    Dim Message As String = ""
    Dim Listener1 As New TcpListener(65534)
    Dim Client1 As New TcpClient
    Dim Message1 As String = ""
    Dim IPAdd As String
   


    ' Private _t As  Thread  ' Thread to perform the polling
    Private _notify As Boolean = False ' Boolean flag to determine when to stop
    Private _isRunning As Boolean = False  ' Set when the polling is taking place
    Private _interval As Integer = 10 ' Interval to poll at

    Private notifyIcon As New System.Windows.Forms.NotifyIcon()


    Public sUser As String
    Public UName As String
    Public UserNo As String
    Public lnFid As Integer
    Public strConn As String
    Public bnStart As Boolean '= True
    Public strPath As String 'get current path for the program
    Public sDatabase As String
    Public strRptPath As String
    Public strDBpath As String
    Public strPathP As String
    Public strPRpts As String
    Public sServer As String
    Public cnHCIS As New ADODB.Connection
    Private iAns As Integer
    Public rDatabase As String
    Public rServer As String
    Private tmrMain As New DispatcherTimer
    Private tmrNotify As New DispatcherTimer
    Private tmrKillIcon As New DispatcherTimer 'to kill the notify icon
    Private lnTime As Double
    Private strCompAdd As String
    Dim Etime As Date
    Public strDesign As String
    Private bnSwitchUser As Boolean
    Private nav As NavigationService



    Private Sub MainWindow_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Try
            If bnSwitchUser = True Then
                notifyIcon.Dispose()
                Me.Close()
                Dim frmL As New Login
                frmL.Show()
            Else
                notifyIcon.Dispose()
                End
            End If
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
        Try
            notifyIcon.Visible = False
            notifyIcon.Dispose()
            Dim rsULog As New ADODB.Recordset
            With rsULog
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblULog WHERE TimeIn='" & Format(lblTimeIn.Content, "Long Time") & "'", cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                .Fields("TTimeIn").Value = Format(lblTTimeIn.Content, "Long Time")
                .Fields("TimeOut").Value = Format(Now, "Long Time")
                .Update()
                .Close()
            End With
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub



    Private Sub MainWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        Dim rsInfo As New ADODB.Recordset
        notifyIcon.Icon = New System.Drawing.Icon("alpha.ico")
        notifyIcon.BalloonTipTitle = "Queue"

        tmrMain.Interval = TimeSpan.FromMilliseconds(600)
        AddHandler tmrMain.Tick, AddressOf tmrMain_Tick
        tmrMain.Start()

        'get current theme
        Try
            regTheme = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
            strTheme = regTheme.GetValue("Theme")
            If strTheme = "Dark" Then
                optDark.IsChecked = True
            ElseIf strTheme = "Light" Then
                optLight.IsChecked = True
            End If
            regTheme.Close()
        Catch ex As Exception
        End Try


        Try
            regNotify = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyNT, True)
            strNotify = regNotify.GetValue("State")
            If strNotify = "ON" Then
                tglSwtNot.IsChecked = True
            ElseIf strNotify = "OFF" Then
                tglSwtNot.IsChecked = False
            End If
            regNotify.Close()

        Catch ex As Exception
        End Try


        Try
            regNotify = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyNTT, True)
            strTime = regNotify.GetValue("Time")
            txtTime.Text = strTime
            regNotify.Close()
        Catch ex As Exception
            ' MsgBox("An error has occured!, theme cannot be set now!")
        End Try

        lnTime = Val(strTime) * 60 * 1000 'Change minutes to seconds then miliseconds
        If lnTime = 0 Then lnTime = 120000 'approx 2minutes

        tmrNotify.Interval = TimeSpan.FromMilliseconds(lnTime)
        AddHandler tmrNotify.Tick, AddressOf tmrNotify_Tick
        tmrNotify.Start()



        Try
            regNotify = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyNTS, True)
            strSound = regNotify.GetValue("Audio")
            regNotify.Close()
        Catch ex As Exception
            ' MsgBox("An error has occured!, theme cannot be set now!")
        End Try

        If strSound = "OFF" Then
            tglSwtSound.IsChecked = False
        ElseIf strSound = "ON" Then
            tglSwtSound.IsChecked = True
        End If

        Try
            strConn = "Provider=SQLOLEDB;Data Source=(LocalDB);Initial Catalog=HCISDB;User ID=sa;Password=*******" 'With password

            'strConn = "Provider=SQLOLEDB;Data Source=(LocalDB);Initial Catalog=HCISDB;Integrated Security=SSPI;" '

            cnHCIS.Open(strConn)
            With rsInfo
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblInfo", cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    Title = .Fields("Title").Value & " "
                End If
            End With
        Catch ex As Exception
            MsgBox("Server not found, the program will not start, contact system administrator", MsgBoxStyle.Information, "Alpha Health Care")
            End
        End Try

        themeButtons()

        Try

            Dim Mchar As String = ""

            strPath = GetExecutingAssembly.Location
            Do While Mchar <> "\"
                Mchar = Mid(strPath, Len(strPath), 1)
                strPath = strPath.Remove(Len(strPath) - 1, 1)
            Loop
            strPath = strPath & "\Resources"

        Catch ex As Exception

        End Try


    End Sub


    Private Sub tmrMain_Tick()
        Try
            lblTime.Content = Format(Now, "Long Time")
            Etime = System.DateTime.FromOADate(CDate(lblTimeIn.Content).ToOADate - CDate(lblTime.Content).ToOADate)
            lblTTimeIn.Content = Format(Etime, "HH:mm:ss")
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub


    Public Sub tmrNotify_Tick()

        Dim Sound As New System.Media.SoundPlayer()
        Dim strDetails As String

        btnCTab.BorderThickness = New Thickness(1)
        btnCTab.BorderBrush = myBrush


        If strNotify = "ON" Then
            tglSwtNot.IsChecked = True
        ElseIf strNotify = "OFF" Then
            Exit Sub
        End If

        If strDesign = "" Then Exit Sub

        Try
            strDesign = LCase(strDesign)
            Dim strQueue As String = ""
            Dim rsQueue As New ADODB.Recordset
            If strDesign = "co" Or strDesign = "c.o" Or strDesign = "rco" Or strDesign = "r.c.o" Or strDesign = "clinical officer" Then
                strQueue = "Consultation"
            ElseIf strDesign = "lab technician" Or strDesign = "lab tech" Or strDesign = "lab technologist" Then
                strQueue = "Lab"
            ElseIf strDesign = "pharmacist" Then
                strQueue = "Pharmacy"
            ElseIf strDesign = "nurse" Then
                strQueue = "Nurse"
            ElseIf strDesign = "receptionist" Then
                strQueue = "Reception"
            Else

            End If

            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT QDate as Date, QTime as Time, PatNo, PName, Destination, Status, SendBy, Remarks FROM tblQueue WHERE destination='" & strQueue & "' AND Status='Waiting'", cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    .MoveLast()
                    strDetails = .Fields("PName").Value & " " & .Fields("PatNo").Value & " Send by:" & .Fields("SendBy").Value & " " & .Fields("Remarks").Value

                    If strSound = "ON" Then
                        Sound.SoundLocation = (strPath & "\notify.wav") '"your path to the .wav file"  'ex.: c:\mysound.wav  
                        Sound.Load()
                        Sound.Play()
                    Else
                        'dont play sound
                    End If

                    notifyIcon.Visible = True
                    notifyIcon.ShowBalloonTip(5050, strQueue, "You have " & .RecordCount & " Patients in your queue, The latest is:" & vbLf + vbCr & strDetails, Forms.ToolTipIcon.Info)

                    tmrKillIcon.Interval = TimeSpan.FromMilliseconds(5000)
                    AddHandler tmrKillIcon.Tick, AddressOf tmrKillIcon_Tick
                    tmrKillIcon.Start()

                Else
                    notifyIcon.Visible = False
                End If
                .Close()
            End With

        Catch ex As Exception
            notifyIcon.Dispose()
        End Try
        '   


    End Sub

    Private Sub tmrKillIcon_Tick()
        Try
            notifyIcon.Visible = False
        Catch ex As Exception
            notifyIcon.Dispose()
        End Try
    End Sub

    Private Sub ChangeTheme()
        Try
            fo.Theme = FlyoutTheme.Inverse
            If fo.IsOpen = False Then
                fo.IsOpen = True
            Else
                fo.IsOpen = False
            End If
        Catch ex As Exception
            MsgBox("An error has occured while changing theme " & Err.Description)
        End Try
    End Sub
    Private Sub RegDet()
        Try
            Dim reg As New winReg
            Me.TimeCheck()
            reg.Show()
            reg.Owner = Me
            reg.Topmost = True
        Catch ex As Exception
            MsgBox("An error has occured while opening registration window " & Err.Description)
        End Try
    End Sub


    Public Function SearchPage()
        Try
            Dim fiS As New Frame
            Dim tiS As New TabItem
            Dim pgS As New pgPatSearch
            Dim i As Integer
            tiS.Header = "Search"
            fiS.NavigationService.Navigate(pgS)
            tiS.Content = fiS
            tcMain.Items.Add(tiS)
            i = tcMain.Items.Count - 1
            tcMain.SelectedItem = tcMain.Items(i)
        Catch ex As Exception
            MsgBox("An error has occured while opening search window " & Err.Description)
        End Try
        Return (0)
    End Function


    Private Sub btnCTab_Click(sender As Object, e As RoutedEventArgs) Handles btnCTab.Click
        Try
            If tcMain.Items.Count > 0 Then
                tcMain.Items.Remove(tcMain.SelectedItem)
            Else 'if no tabs open close the app
                If MsgBox("Do you wish to close the application?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    Close()
                Else
                End If
            End If
        Catch ex As Exception
            MsgBox("An error has occured while closing tab " & Err.Description)
        End Try
    End Sub
    Public Sub OpenPatientFile()
        Try
            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgPatient As New pgPatient

            pgPatient.strUser = UName
            pgPatient.dgBrush.Color = myColors(intGColor)
            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "_Patient"
                ti.Name = "Patient"
                fi.NavigationService.Navigate(pgPatient)

                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "Patient" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Patient"
                ti.Name = "Patient"
                fi.NavigationService.Navigate(pgPatient)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading patient form " & Err.Description)
        End Try
    End Sub

    Public Sub OpenConsultationFile()
        Try

            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgConsultation As New pgConsultation

            pgConsultation.strUser = UName
            pgConsultation.dgBrush.Color = myColors(intGColor)
            pgConsultation.intTheme = intGColor

            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "_Consultation"
                ti.Name = "Consultation"
                fi.NavigationService.Navigate(pgConsultation)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "Consultation" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Consultation"
                ti.Name = "Consultation"
                fi.NavigationService.Navigate(pgConsultation)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading consultation form " & Err.Description)
        End Try
    End Sub


    Public Sub OpenANCFile()
        Try

            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgANC As New pgANC

            pgANC.strUser = UName
            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "_Ante-Natal Care"
                ti.Name = "ANC"
                fi.NavigationService.Navigate(pgANC)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "ANC" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Ante-Natal Care"
                ti.Name = "ANC"
                fi.NavigationService.Navigate(pgANC)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading consultation form " & Err.Description)
        End Try
    End Sub


    Public Sub OpenANCHistoryFile()
        Try

            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgANCPatientDetails As New pgANCPatientDetails

            pgANCPatientDetails.strUser = UName

            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "ANC _History"
                ti.Name = "ANCHistory"
                fi.NavigationService.Navigate(pgANCPatientDetails)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "ANCHistory" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "ANC _History"
                ti.Name = "ANCHistory"
                fi.NavigationService.Navigate(pgANCPatientDetails)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading ANC patient history form " & Err.Description)
        End Try
    End Sub


    Public Sub OpenPreviousPregnancyFile()
        Try

            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgPreviousPregnancy As New pgPreviousPregnancy

            pgPreviousPregnancy.strUser = UName

            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "_Pregnancy History"
                ti.Name = "PregnancyHistory"
                fi.NavigationService.Navigate(pgPreviousPregnancy)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "PregnancyHistory" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Pregnancy History"
                ti.Name = "PregnancyHistory"
                fi.NavigationService.Navigate(pgPreviousPregnancy)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading ANC patient history form " & Err.Description)
        End Try
    End Sub

    Public Sub OpenCWCFile()
        Try

            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgCWC As New pgCWC

            pgCWC.strUser = UName

            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "_Child Welfare Clinic"
                ti.Name = "CWC"
                fi.NavigationService.Navigate(pgCWC)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "CWC" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Child Welfare Clinic"
                ti.Name = "CWC"
                fi.NavigationService.Navigate(pgCWC)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading Child Welfare form " & Err.Description)
        End Try
    End Sub

    Public Sub OpenFPCFile()
        Try

            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgFP As New pgFP

            pgFP.strUser = UName

            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "_Family Planning Clinic"
                ti.Name = "ANC"
                fi.NavigationService.Navigate(pgFP)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "FPC" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Family Planning Clinic"
                ti.Name = "CWC"
                fi.NavigationService.Navigate(pgFP)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading FP Clinic form " & Err.Description)
        End Try
    End Sub


    Public Sub OpenImpressionFile()
        Try

            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgImpression As New pgImpression

            pgImpression.strUser = UName
            pgImpression.dgBrush.Color = myColors(intGColor)
            pgImpression.intTheme = intGColor

            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "_Impression"
                ti.Name = "Impression"
                fi.NavigationService.Navigate(pgImpression)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "Impression" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Impression"
                ti.Name = "Impression"
                fi.NavigationService.Navigate(pgImpression)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading Impression form " & Err.Description)
        End Try
    End Sub

    Public Sub OpenLabFile()
        Try
            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgLab As New pgLab
            pgLab.strUser = UName
            TimeCheck()
            If tcMain.Items.Count = 0 Then
                ti.Header = "_Lab"
                ti.Name = "Lab"
                fi.NavigationService.Navigate(pgLab)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "Lab" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Lab"
                ti.Name = "Lab"
                fi.NavigationService.Navigate(pgLab)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while opening lab form " & Err.Description)
        End Try

    End Sub

    Public Sub OpenPharmacyFile()
        Try

            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgPharmacy As New pgPharmacy2
            pgPharmacy.strUser = UName
            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "_Pharmacy"
                ti.Name = "Pharmacy"
                fi.NavigationService.Navigate(pgPharmacy)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "Pharmacy" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Pharmacy"
                ti.Name = "Pharmacy"
                fi.NavigationService.Navigate(pgPharmacy)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while opening pharmacy form " & Err.Description)
        End Try
    End Sub

    Private Sub OpenUserFile()
        Try
            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgUser As New pgUser
            pgUser.strUser = UName
            TimeCheck()
            If tcMain.Items.Count = 0 Then
                tiU.Header = "_User"
                tiU.Name = "User"
                fiU.NavigationService.Navigate(pgUser)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items
                    If tiU.Name = "User" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_User"
                ti.Name = "User"
                fiU.NavigationService.Navigate(pgUser)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If

        Catch ex As Exception
            MsgBox("An error has occured while opening user form " & Err.Description)
        End Try


    End Sub

    Private Sub OpenPaymentFile()
        Try
            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgPayment As New pgPayment
            pgPayment.strUser = UName
            TimeCheck()
            If tcMain.Items.Count = 0 Then
                tiU.Header = "_Payment"
                tiU.Name = "Payment"
                fiU.NavigationService.Navigate(pgPayment)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items
                    If tiU.Name = "Payment" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next

                ti.Header = "_Payment"
                ti.Name = "Payment"
                fiU.NavigationService.Navigate(pgPayment)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while opening payment file " & Err.Description)
        End Try
    End Sub
    Public Sub OpenNurseFile()
        Try
            Dim ti As New TabItem
            Dim fi As New Frame
            Dim intTab As Integer
            Dim t As TabItem
            Dim iTabCount As Integer = 1
            Dim pgNurse As New pgNurse

            pgNurse.strUser = UName
            pgNurse.dgBrush.Color = myColors(intGColor)
            TimeCheck()

            If tcMain.Items.Count = 0 Then
                ti.Header = "_Nurse"
                ti.Name = "Nurse"
                fi.NavigationService.Navigate(pgNurse)

                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each t In tcMain.Items
                    If t.Name = "Nurse" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next
                ti.Header = "_Nurse"
                ti.Name = "Nurse"
                fi.NavigationService.Navigate(pgNurse)
                ti.Content = fi
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while loading nurse form " & Err.Description)
        End Try
    End Sub
    Private Sub OpenBillFile()
        Try

            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgViewBill As New pgViewBill
            pgViewBill.strUser = UName
            TimeCheck()
            If tcMain.Items.Count = 0 Then
                tiU.Header = "_Bill"
                tiU.Name = "Bill"
                fiU.NavigationService.Navigate(pgViewBill)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items

                    If tiU.Name = "Bill" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next

                ti.Header = "_Bill"
                ti.Name = "Bill"
                fiU.NavigationService.Navigate(pgViewBill)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while opening bill form " & Err.Description)
        End Try
    End Sub

    Private Sub OpenLabTestsFile()
        Try


            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgLabTests As New pgLabTests
            pgLabTests.strUser = UName
            TimeCheck()
            If tcMain.Items.Count = 0 Then
                tiU.Header = "_Lab Tests"
                tiU.Name = "LabTests"
                fiU.NavigationService.Navigate(pgLabTests)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items

                    If tiU.Name = "LabTests" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next

                ti.Header = "_Lab Tests"
                ti.Name = "LabTests"
                fiU.NavigationService.Navigate(pgLabTests)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while opening lab tests form " & Err.Description)

        End Try
    End Sub

    Private Sub OpenDrugsFile()
        Try


            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgDrugs As New pgDrugs
            pgDrugs.strUser = UName
            TimeCheck()
            If tcMain.Items.Count = 0 Then
                tiU.Header = "_Drugs"
                tiU.Name = "Drugs"
                fiU.NavigationService.Navigate(pgDrugs)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items

                    If tiU.Name = "Drugs" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next

                ti.Header = "_Drugs"
                ti.Name = "Drugs"
                fiU.NavigationService.Navigate(pgDrugs)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while opening drugs form " & Err.Description)
        End Try
    End Sub

    Private Sub OpenDrugsReceivedFile()
        Try

            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgDrugsReceived As New pgDrugsReceived
            pgDrugsReceived.strUser = UName
            TimeCheck()
            If tcMain.Items.Count = 0 Then
                tiU.Header = "_Drugs Received"
                tiU.Name = "DrugsReceived"
                fiU.NavigationService.Navigate(pgDrugsReceived)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items

                    If tiU.Name = "DrugsReceived" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next

                ti.Header = "_Drugs Received"
                ti.Name = "DrugsReceived"
                fiU.NavigationService.Navigate(pgDrugsReceived)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while opening drugs received form " & Err.Description)
        End Try
    End Sub

    Private Sub OpenMyQueueFile()
        Try


            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgMyQueue As New pgMyQueue
            pgMyQueue.strUser = UName
            pgMyQueue.strDesign = strDesign

            pgMyQueue.dgBrush.Color = myColors(intGColor)
            TimeCheck()

            If tcMain.Items.Count = 0 Then
                tiU.Header = "_My Queue"
                tiU.Name = "MyQueue"
                fiU.NavigationService.Navigate(pgMyQueue)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items

                    If tiU.Name = "MyQueue" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next

                ti.Header = "_My Queue"
                ti.Name = "MyQueue"
                fiU.NavigationService.Navigate(pgMyQueue)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If
        Catch ex As Exception
            MsgBox("An error has occured while opening my queue form " & Err.Description)
        End Try
    End Sub


    Public Sub printPatients()
        Try
            TimeCheck()
            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgReports As New pgReports

            If tcMain.Items.Count = 0 Then
                tiU.Header = "_Reports"
                tiU.Name = "Reports"
                fiU.NavigationService.Navigate(pgReports)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items

                    If tiU.Name = "pgReports" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next

                ti.Header = "_Reports"
                ti.Name = "pgReports"
                fiU.NavigationService.Navigate(pgReports)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If

        Catch ex As Exception
            MsgBox("An error has occured while printing patients' data " & Err.Description)
        End Try

        Dim nwWin As New winRptI


    End Sub

    Public Sub printMedReport()
        Try
            TimeCheck()
            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgMedReports As New pgMedReports

            If tcMain.Items.Count = 0 Then
                tiU.Header = "_Consultation Reports"
                tiU.Name = "ConsultationReports"
                fiU.NavigationService.Navigate(pgMedReports)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items

                    If tiU.Name = "pgMedReports" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next

                ti.Header = "_Consultation Reports"
                ti.Name = "pgMedReports"
                fiU.NavigationService.Navigate(pgMedReports)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If

        Catch ex As Exception
            MsgBox("An error has occured while printing diagnosis data " & Err.Description)
        End Try


    End Sub

    Public Sub printPharmacyReport()
        Try
            TimeCheck()
            Dim tiU As New TabItem
            Dim fiU As New Frame
            Dim intTab As Integer
            Dim iTabCount As Integer = 1
            Dim ti As New TabItem
            Dim pgPharmacyReports As New pgPharmacyReports

            If tcMain.Items.Count = 0 Then
                tiU.Header = "_Pharmacy Reports"
                tiU.Name = "PharmacyReports"
                fiU.NavigationService.Navigate(pgPharmacyReports)
                tiU.Content = fiU
                tcMain.Items.Add(tiU)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
                Exit Sub
            Else
                intTab = 0
                For Each tiU In tcMain.Items

                    If tiU.Name = "pgPharmacyReports" Then
                        tcMain.SelectedItem = tcMain.Items(intTab)
                        Exit Sub
                    End If
                    intTab = intTab + 1
                Next

                ti.Header = "_Pharmacy Reports"
                ti.Name = "pgPharmacyReports"
                fiU.NavigationService.Navigate(pgPharmacyReports)
                ti.Content = fiU
                tcMain.Items.Add(ti)
                intTab = tcMain.Items.Count - 1
                tcMain.SelectedItem = tcMain.Items(intTab)
            End If

        Catch ex As Exception
            MsgBox("An error has occured while printing pharmacy data " & Err.Description)
        End Try
    End Sub


    Public Sub printReceipt()
        Try
            TimeCheck()
            Dim winRcp As New winPrintRcpt
            winRcp.intGColor = intGColor
            winRcp.Show()
            winRcp.Owner = Me
        Catch ex As Exception
            MsgBox("An error has occured while opening form to print receipt " & Err.Description)
        End Try
    End Sub

    Public Sub printBill()
        Try
            Dim winRcp As New winBill
            TimeCheck()
            winRcp.Show()
            winRcp.Owner = Me
        Catch ex As Exception
            MsgBox("An error has occured while opening form to print bill " & Err.Description)
        End Try
    End Sub

    Private Sub printLabResult()
        Try
            TimeCheck()
            Dim winRcp As New winLabResult
            winRcp.intGColor = intGColor
            winRcp.Show()
            winRcp.Owner = Me
        Catch ex As Exception
            MsgBox("An error has occured while opening form to print lab result " & Err.Description)
        End Try
    End Sub

    Private Sub printConReport()
        Try
            TimeCheck()
            Dim winRcp As New winConReport
            winRcp.intGColor = intGColor
            winRcp.Show()
            winRcp.Owner = Me
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Public Sub gfServer()
        Try
            Dim X As Integer
            Dim strCn As String
            Dim N As Integer
            Dim iCode As String = ""
            Dim pCode As String = ""
            Dim ieCount As Integer
            Dim iCurLoc As Integer = 0

            strCn = strConn
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
            MsgBox("An error has occured while getting server details" & Err.Description)

        End Try
    End Sub

    Public Function GetUserName(ByVal un As String)
        UName = un
        Return (0)
    End Function

    Private Sub setGlow(intGColor As Integer)
        Try

            Select Case intGColor

                Case Else
            End Select
        Catch ex As Exception
            MsgBox("An error has occured while setting glow colour " & Err.Description)
        End Try
    End Sub

    Private Sub TimeCheck()
        Dim rsULog As New ADODB.Recordset
        Dim tmIn As Date
        If Trim(lblTimeIn.Content) <> "" Then
            tmIn = Format(lblTimeIn.Content, "Long Time").ToString
        Else
            tmIn = Format(Now, "Long Time")
        End If
        Try
            With rsULog
                If .State = 1 Then .Close()
                .CursorLocation = CursorLocationEnum.adUseClient
                .Open("SELECT * FROM tblULog WHERE TimeIn='" & tmIn & "'", cnHCIS, ADODB.CursorTypeEnum.adOpenDynamic, ADODB.LockTypeEnum.adLockPessimistic)
                If .RecordCount > 0 Then
                    .Fields("TimeCheck").Value = Format(Now, "Long Time")
                    .Fields("TTimeIn").Value = Format(lblTTimeIn.Content, "Long Time")
                    .Update()
                Else
                    'avoid this scenario
                End If

                .Close()
            End With
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub


    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        nav = NavigationService.GetNavigationService(Me)
        statusBarAccent()
    End Sub

    Private Sub statusBarAccent()
        Try
            myBrush = New SolidColorBrush(myColors(intGColor))
            setGlow(intGColor)
            btnCTab.BorderThickness = New Thickness(1)
            btnCTab.BorderBrush = myBrush
        Catch ex As Exception
            MsgBox("An error has occured while setting status bar colour " & Err.Description)
        End Try
    End Sub

    Private Sub themeButtons()
        Try


            myBrush = New SolidColorBrush(myColors(0))
            lime.Background = myBrush

            myBrush = New SolidColorBrush(myColors(1))
            green.Background = myBrush

            myBrush = New SolidColorBrush(myColors(2))
            emerald.Background = myBrush

            myBrush = New SolidColorBrush(myColors(3))
            teal.Background = myBrush

            myBrush = New SolidColorBrush(myColors(4))
            cyan.Background = myBrush

            myBrush = New SolidColorBrush(myColors(5))
            cobalt.Background = myBrush

            myBrush = New SolidColorBrush(myColors(6))
            indigo.Background = myBrush

            myBrush = New SolidColorBrush(myColors(7))
            violet.Background = myBrush

            myBrush = New SolidColorBrush(myColors(8))
            pink.Background = myBrush

            myBrush = New SolidColorBrush(myColors(9))
            magenta.Background = myBrush

            myBrush = New SolidColorBrush(myColors(10))
            crimson.Background = myBrush

            myBrush = New SolidColorBrush(myColors(11))
            red.Background = myBrush

            myBrush = New SolidColorBrush(myColors(12))
            orange.Background = myBrush

            myBrush = New SolidColorBrush(myColors(13))
            amber.Background = myBrush

            myBrush = New SolidColorBrush(myColors(14))
            yellow.Background = myBrush

            myBrush = New SolidColorBrush(myColors(15))
            brown.Background = myBrush

            myBrush = New SolidColorBrush(myColors(16))
            olive.Background = myBrush

            myBrush = New SolidColorBrush(myColors(17))
            steel.Background = myBrush

            myBrush = New SolidColorBrush(myColors(18))
            mauve.Background = myBrush

            myBrush = New SolidColorBrush(myColors(19))
            taupe.Background = myBrush
        Catch ex As Exception
            MsgBox("An error has occured while setting theme buttons " & Err.Description)

        End Try
    End Sub



    Private Sub Lime_Click(sender As Object, e As RoutedEventArgs) Handles lime.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Lime"), ThemeManager.AppThemes(intTheme))
            strColor = "Lime"
            intGColor = 0
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting lime colour " & Err.Description)
        End Try
    End Sub

    Private Sub Green_Click(sender As Object, e As RoutedEventArgs) Handles green.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Green"), ThemeManager.AppThemes(intTheme))
            strColor = "Green"
            intGColor = 1
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting green colour " & Err.Description)
        End Try
    End Sub

    Private Sub Emerald_Click(sender As Object, e As RoutedEventArgs) Handles emerald.Click
        Try

            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Emerald"), ThemeManager.AppThemes(intTheme))
            strColor = "Emerald"
            intGColor = 2
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting emerald colour " & Err.Description)
        End Try
    End Sub

    Private Sub Teal_Click(sender As Object, e As RoutedEventArgs) Handles teal.Click
        Try

            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Teal"), ThemeManager.AppThemes(intTheme))
            strColor = "Teal"
            intGColor = 3
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting teal colour " & Err.Description)
        End Try
    End Sub

    Private Sub Cyan_Click(sender As Object, e As RoutedEventArgs) Handles cyan.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Cyan"), ThemeManager.AppThemes(intTheme))
            strColor = "Cyan"
            intGColor = 4
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting teal colour " & Err.Description)
        End Try
    End Sub

    Private Sub Cobalt_Click(sender As Object, e As RoutedEventArgs) Handles cobalt.Click
        Try

            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Cobalt"), ThemeManager.AppThemes(intTheme))
            strColor = "Cobalt"
            intGColor = 5
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting cobalt colour " & Err.Description)
        End Try
    End Sub

    Private Sub Indigo_Click(sender As Object, e As RoutedEventArgs) Handles indigo.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Indigo"), ThemeManager.AppThemes(intTheme))
            strColor = "Indigo"
            intGColor = 6
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting indigo colour " & Err.Description)
        End Try
    End Sub

    Private Sub Violet_Click(sender As Object, e As RoutedEventArgs) Handles violet.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Violet"), ThemeManager.AppThemes(intTheme))
            strColor = "Violet"
            intGColor = 7
            statusBarAccent()

        Catch ex As Exception
            MsgBox("An error has occured while setting violet colour " & Err.Description)
        End Try
    End Sub

    Private Sub Pink_Click(sender As Object, e As RoutedEventArgs) Handles pink.Click
        Try

            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Pink"), ThemeManager.AppThemes(intTheme))
            strColor = "Pink"
            intGColor = 8
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting pink colour " & Err.Description)
        End Try
    End Sub

    Private Sub Magenta_Click(sender As Object, e As RoutedEventArgs) Handles magenta.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Magenta"), ThemeManager.AppThemes(intTheme))
            strColor = "Magenta"
            intGColor = 9
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting magenta colour " & Err.Description)
        End Try

    End Sub

    Private Sub Crimson_Click(sender As Object, e As RoutedEventArgs) Handles crimson.Click
        Try

            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Crimson"), ThemeManager.AppThemes(intTheme))
            strColor = "Crimson"
            intGColor = 10
            statusBarAccent()

        Catch ex As Exception
            MsgBox("An error has occured while setting crimson colour " & Err.Description)
        End Try
    End Sub

    Private Sub Red_Click(sender As Object, e As RoutedEventArgs) Handles red.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Red"), ThemeManager.AppThemes(intTheme))
            strColor = "Red"
            intGColor = 11
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting red colour " & Err.Description)
        End Try
    End Sub

    Private Sub Orange_Click(sender As Object, e As RoutedEventArgs) Handles orange.Click
        Try

            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Orange"), ThemeManager.AppThemes(intTheme))
            strColor = "Orange"
            intGColor = 12
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting orange colour " & Err.Description)
        End Try
    End Sub

    Private Sub Amber_Click(sender As Object, e As RoutedEventArgs) Handles amber.Click
        Try

            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Amber"), ThemeManager.AppThemes(intTheme))
            strColor = "Amber"
            intGColor = 13
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting amber colour " & Err.Description)
        End Try
    End Sub

    Private Sub Yellow_Click(sender As Object, e As RoutedEventArgs) Handles yellow.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Yellow"), ThemeManager.AppThemes(intTheme))
            strColor = "Yellow"
            intGColor = 14
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting yellow colour " & Err.Description)
        End Try

    End Sub
    Private Sub Brown_Click(sender As Object, e As RoutedEventArgs) Handles brown.Click
        Try

            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Brown"), ThemeManager.AppThemes(intTheme))
            strColor = "Brown"
            intGColor = 15
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting brown colour " & Err.Description)
        End Try
    End Sub

    Private Sub Olive_Click(sender As Object, e As RoutedEventArgs) Handles olive.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Olive"), ThemeManager.AppThemes(intTheme))
            strColor = "Olive"
            intGColor = 16
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting olive colour " & Err.Description)
        End Try

    End Sub

    Private Sub Steel_Click(sender As Object, e As RoutedEventArgs) Handles steel.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Steel"), ThemeManager.AppThemes(intTheme))
            strColor = "Steel"
            intGColor = 17
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting steel colour " & Err.Description)
        End Try
    End Sub

    Private Sub Mauve_Click(sender As Object, e As RoutedEventArgs) Handles mauve.Click
        Try

            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Mauve"), ThemeManager.AppThemes(intTheme))
            strColor = "Mauve"
            intGColor = 18
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting mauve colour " & Err.Description)
        End Try
    End Sub

    Private Sub Taupe_Click(sender As Object, e As RoutedEventArgs) Handles taupe.Click
        Try
            If strTheme = "Dark" Then
                intTheme = 1
            ElseIf strTheme = "Light" Then
                intTheme = 0
            End If
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Taupe"), ThemeManager.AppThemes(intTheme))
            strColor = "Taupe"
            intGColor = 19
            statusBarAccent()
        Catch ex As Exception
            MsgBox("An error has occured while setting taupe colour " & Err.Description)
        End Try

    End Sub


    Private Sub optDark_Click(sender As Object, e As RoutedEventArgs) Handles optDark.Click
        Try
            strTheme = "Dark"
            ThemeManager.AddAppTheme("BaseDark", New Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseDark.xaml"))
            ThemeManager.ChangeAppTheme(Application.Current, "BaseDark")
        Catch ex As Exception
            MsgBox("An error has occured while setting dark theme " & Err.Description)
        End Try
    End Sub

    Private Sub optLight_Click(sender As Object, e As RoutedEventArgs) Handles optLight.Click
        Try
            strTheme = "Light"
            ThemeManager.AddAppTheme("BaseLight", New Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml"))
            ThemeManager.ChangeAppTheme(Application.Current, "BaseLight")
        Catch ex As Exception
            MsgBox("An error has occured while setting light theme " & Err.Description)
        End Try
    End Sub


    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        If fo.IsOpen = True Then
            fo.IsOpen = False
        Else
            fo.IsOpen = False
        End If
    End Sub

    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        On Error Resume Next 'change this error handling

        If MsgBox("Do you wish to save this theme setting?", MsgBoxStyle.Question + MsgBoxStyle.YesNoCancel) = vbYes Then
            'create the new registry key under HKEY_CURRENT_USER
            regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
            'open the key with write access
            regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
            'setup the value
            regTheme.SetValue("Theme", strTheme)
            regTheme.SetValue("Color", strColor)
            'close the key
            regTheme.Close()
            'for notification purpose 
            'create the new registry key under HKEY_CURRENT_USER
            regNotify = Registry.CurrentUser.CreateSubKey(m_sRegKeyNT)
            'open the key with write access
            regNotify = Registry.CurrentUser.OpenSubKey(m_sRegKeyNT, True)
            'setup the value
            regNotify.SetValue("State", strNotify)
            'close the key
            regNotify.Close()

            'for notification  time
            'create the new registry key under HKEY_CURRENT_USER
            regNotify = Registry.CurrentUser.CreateSubKey(m_sRegKeyNTT)
            'open the key with write access
            regNotify = Registry.CurrentUser.OpenSubKey(m_sRegKeyNTT, True)
            'setup the value
            regNotify.SetValue("Time", strTime)
            'close the key
            regNotify.Close()

            lnTime = Val(strTime) * 60 * 1000

            'for notification  sound
            'create the new registry key under HKEY_CURRENT_USER
            regNotify = Registry.CurrentUser.CreateSubKey(m_sRegKeyNTS)
            'open the key with write access
            regNotify = Registry.CurrentUser.OpenSubKey(m_sRegKeyNTS, True)
            'setup the value
            regNotify.SetValue("Audio", strSound)
            'close the key
            regNotify.Close()
        Else
        End If

    End Sub

    Private Sub fo_Initialized(sender As Object, e As EventArgs) Handles fo.Initialized
        Try
            regNotify = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyNT, True)
            strNotify = regNotify.GetValue("State")
            If strNotify = "ON" Then
                tglSwtNot.IsChecked = True
            ElseIf strNotify = "OFF" Then
                tglSwtNot.IsChecked = False
            End If
            regNotify.Close()

        Catch ex As Exception
        End Try


        Try
            regNotify = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyNTT, True)
            strTime = regNotify.GetValue("Time")
            txtTime.Text = strTime
            regNotify.Close()
        Catch ex As Exception
        End Try

        If strNotify = "OFF" Then
            txtTime.IsEnabled = False
        ElseIf strNotify = "ON" Then
            txtTime.IsEnabled = True
        End If

        Try
            regNotify = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyNTS, True)
            strSound = regNotify.GetValue("Audio")
            regNotify.Close()
        Catch ex As Exception
            MsgBox("An error has occured!, settings cant be saved now!")
        End Try

        If strSound = "OFF" Then
            tglSwtSound.IsChecked = False
        ElseIf strSound = "ON" Then
            tglSwtSound.IsChecked = True
        End If

    End Sub

    Private Sub fo_MouseLeave(sender As Object, e As MouseEventArgs) Handles fo.MouseLeave
        fo.IsOpen = False
    End Sub

    Private Sub btnSetting_MouseEnter(sender As Object, e As MouseEventArgs) Handles btnSetting.MouseEnter
        Try
            If fo.IsOpen = True Then

            Else
                fo.Theme = FlyoutTheme.Inverse
                fo.IsOpen = True
                ' fo.IsOpen = False
            End If
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try

    End Sub

    Private Sub switchDepartment()
        Try

            If DeptFo.IsOpen = True Then

            Else
                DeptFo.Theme = FlyoutTheme.Inverse
                DeptFo.IsOpen = True
            End If
        Catch ex As Exception
            MsgBox("An error has occured while opening department flyout" & Err.Description)
        End Try
    End Sub

    Private Sub btnNoChange_Click(sender As Object, e As RoutedEventArgs) Handles btnNoChange.Click
        Try
            If DeptFo.IsOpen = True Then
                DeptFo.IsOpen = False
            Else
                DeptFo.IsOpen = False
            End If
        Catch ex As Exception
            MsgBox(Err.Description)
        End Try
    End Sub

    Private Sub btnChange_Click(sender As Object, e As RoutedEventArgs) Handles btnChange.Click
        Try

            If optConsultation.IsChecked = True Then
                Accounts.IsEnabled = False
                Patient.IsEnabled = False
                LabTests.IsEnabled = False
                Drugs.IsEnabled = False
                Pharmacy.IsEnabled = False
                Consultation.IsEnabled = True
                Payment.IsEnabled = False
                Lab.IsEnabled = False
                User.IsEnabled = False
                patRep.IsEnabled = False
                PrintRcpt.IsEnabled = False
                strDesign = "co"
            ElseIf optLab.IsChecked = True Then
                Accounts.IsEnabled = False
                Patient.IsEnabled = False
                LabTests.IsEnabled = True
                Lab.IsEnabled = True
                Drugs.IsEnabled = False
                Pharmacy.IsEnabled = False
                Consultation.IsEnabled = False
                Payment.IsEnabled = False
                User.IsEnabled = False
                patRep.IsEnabled = False
                PrintRcpt.IsEnabled = False
                strDesign = "lab technician"
            ElseIf optPharm.IsChecked = True Then
                Accounts.IsEnabled = False
                Patient.IsEnabled = False
                LabTests.IsEnabled = False
                Drugs.IsEnabled = False
                Pharmacy.IsEnabled = True
                Consultation.IsEnabled = False
                Payment.IsEnabled = False
                Lab.IsEnabled = False
                User.IsEnabled = False
                patRep.IsEnabled = False
                PrintRcpt.IsEnabled = False
                strDesign = "pharmacist"
            ElseIf optRecep.IsChecked = True Then
                Accounts.IsEnabled = True
                Main.IsEnabled = True
                Consultation.IsEnabled = False
                Payment.IsEnabled = True
                Pharmacy.IsEnabled = False
                User.IsEnabled = False
                strDesign = "receptionist"
            End If
            DeptFo.IsOpen = False
        Catch ex As Exception
            MsgBox("An error has occured while setting different department details " & Err.Description)
        End Try
    End Sub


    Private Sub switchUserM()
        bnSwitchUser = True
        Close()
    End Sub

    Private Sub exitSystem()
        Close()
    End Sub



    Private Sub txtTime_LostFocus(sender As Object, e As RoutedEventArgs) Handles txtTime.LostFocus
        Try
            If IsNumeric(txtTime.Text) = False Then
                MsgBox("Please enter time in numerals only (0...9)", MsgBoxStyle.Exclamation)
                txtTime.Text = ""
            ElseIf Val(txtTime.Text) <= 0 Then
                MsgBox("Minimum time cannot be less than 1 minute", MsgBoxStyle.Exclamation)
                txtTime.Text = 2
            ElseIf Val(txtTime.Text) > 10 Then
                MsgBox("Maximum time cannot be more than 10 minutes", MsgBoxStyle.Exclamation)
                txtTime.Text = 10
            Else
                strTime = txtTime.Text
            End If
        Catch ex As Exception
            MsgBox("An error has occured while trying to set reminder duration " & Err.Description)
        End Try
    End Sub

    Private Sub tglSwtSound_Click(sender As Object, e As RoutedEventArgs) Handles tglSwtSound.Click
      
    End Sub

    Public Sub PrintDailyCollections()
        Dim rptDCols As New rptDailyCollections
        Dim winRptR As New winRptI
        Dim myLogOnInfo As New TableLogOnInfo()
        Dim myTableLogOnInfos As New TableLogOnInfos
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptDCols.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)

        rptDCols.Refresh()

        GetServer()
        Try
            myConnectionInfo.ServerName = rServer
            myConnectionInfo.DatabaseName = rDatabase
            myConnectionInfo.UserID = ""
            myConnectionInfo.Password = ""
            rptDCols.SetDatabaseLogon("***", "******", rServer, rDatabase) 'change user name and password as they apply
            rptDCols.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "***", "******") 'change user name and password as they apply
            rptDCols.DataSourceConnections.Item(0).SetLogon("***", "******") 'change user name and password as they apply
            rptDCols.RecordSelectionFormula = "{tblPayment.PDate} ='" & Format(Today, "yyyy-MM-d") & "'"
            rptDCols.Refresh()
            winRptR.crvMain.ViewerCore.ReportSource = rptDCols
            winRptR.Show()
        Catch ex As Exception

        End Try

    End Sub

    Public Sub PrintDrugList()
        Dim rptDruglist As New rptDrugList
        Dim winRptR As New winRptI
        Dim myLogOnInfo As New TableLogOnInfo()
        Dim myTableLogOnInfos As New TableLogOnInfos
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptDruglist.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)

        rptDruglist.Refresh()

        GetServer()
        Try
            myConnectionInfo.ServerName = rServer
            myConnectionInfo.DatabaseName = rDatabase
            myConnectionInfo.UserID = ""
            myConnectionInfo.Password = ""
            rptDruglist.SetDatabaseLogon("***", "*****", rServer, rDatabase) 'change user name and password as they apply
            rptDruglist.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "***", "******") 'change user name and password as they apply
            rptDruglist.DataSourceConnections.Item(0).SetLogon("***", "******") 'change user name and password as they apply
            rptDruglist.Refresh()
            winRptR.crvMain.ViewerCore.ReportSource = rptDruglist
            winRptR.Show()
        Catch ex As Exception

        End Try

    End Sub

    Public Sub PrintLabTestList()
        Dim rptLabTestlist As New rptLabTestList
        Dim winRptR As New winRptI
        Dim myLogOnInfo As New TableLogOnInfo()
        Dim myTableLogOnInfos As New TableLogOnInfos
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptLabTestlist.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)

        rptLabTestlist.Refresh()

        GetServer()
        Try
            myConnectionInfo.ServerName = rServer
            myConnectionInfo.DatabaseName = rDatabase
            myConnectionInfo.UserID = ""
            myConnectionInfo.Password = ""
            rptLabTestlist.SetDatabaseLogon("***", "******", rServer, rDatabase) 'change user name and password as they apply
            rptLabTestlist.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "***", "******") 'change user name and password as they apply
            rptLabTestlist.DataSourceConnections.Item(0).SetLogon("***", "******") 'change user name and password as they apply
            rptLabTestlist.Refresh()
            winRptR.crvMain.ViewerCore.ReportSource = rptLabTestlist
            winRptR.Show()
        Catch ex As Exception

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

            strCn = strConn
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
            MsgBox("An error has occured while getting server details " & Err.Description)
        End Try
    End Sub

    Public Sub PrintDailyColSummary()
        Dim rptDCols As New rptDCollsDetails
        Dim winRptR As New winRptI
        Dim myLogOnInfo As New TableLogOnInfo()
        Dim myTableLogOnInfos As New TableLogOnInfos
        Dim myConnectionInfo As New ConnectionInfo()
        Dim myDataSourceConnections As DataSourceConnections = rptDCols.DataSourceConnections
        Dim myConnectInfo As IConnectionInfo = myDataSourceConnections(0)

        rptDCols.Refresh()

        GetServer()
        Try
            myConnectionInfo.ServerName = rServer
            myConnectionInfo.DatabaseName = rDatabase
            myConnectionInfo.UserID = ""
            myConnectionInfo.Password = ""
            rptDCols.SetDatabaseLogon("***", "******", rServer, rDatabase) 'change user name and password as they apply
            rptDCols.DataSourceConnections.Item(0).SetConnection(rServer, rDatabase, "***", "******") 'change user name and password as they apply
            rptDCols.DataSourceConnections.Item(0).SetLogon("***", "******") 'change user name and password as they apply
            rptDCols.RecordSelectionFormula = "{tblPayment.PDate} ='" & Format(Today, "yyyy-MM-d") & "'"
            rptDCols.Refresh()
            winRptR.crvMain.ViewerCore.ReportSource = rptDCols
            winRptR.Show()
        Catch ex As Exception
            MsgBox("An error has occured while printing " & Err.Description)
        End Try

    End Sub

    Private Sub tglSwtNot_IsCheckedChanged(sender As Object, e As EventArgs) Handles tglSwtNot.IsCheckedChanged
        If tglSwtNot.IsChecked = True Then
            strNotify = "ON"
            txtTime.IsEnabled = True
        ElseIf tglSwtNot.IsChecked = False Then
            strNotify = "OFF"
            txtTime.IsEnabled = False
        End If
    End Sub

    Private Sub tglSwtSound_IsCheckedChanged(sender As Object, e As EventArgs) Handles tglSwtSound.IsCheckedChanged
        If tglSwtSound.IsChecked = True Then
            strSound = "ON"
        ElseIf tglSwtSound.IsChecked = False Then
            strSound = "OFF"
        End If
    End Sub

End Class
