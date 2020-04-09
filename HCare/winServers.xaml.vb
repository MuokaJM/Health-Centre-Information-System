Imports Microsoft.SqlServer.Management
Imports Microsoft.SqlServer.Server
Imports Microsoft.Win32
Imports System


Public Class winServers

    Private iAns As Integer
    Private cnServer As New ADODB.Connection()
    Private oSQLServerDMOApp As New SmoApplication()
    Private myServer As New Server
    ''Private oRegisteredServer As New SQLDMO.RegisteredServer()
    Private oRegisteredServer As New Smo.RegisteredServers.RegisteredServer

    'Private oServerGroup As New SQLDMO.ServerGroup()
    Private oServerGroup As Smo.RegisteredServers.ServerGroupBase

    'Private oDatabase As New SQLDMO.Database()
    Private oDatabase As New Smo.Database

    'Private oSvr As New SQLDMO.SQLServer()
    Private oSvr As New Smo.Server()

    Private WithEvents oSQLServer2 As Smo.Server ' .Server2

    Public ServerName As String, DatabaseName As String, _
    UserName As String, Password As String, i As Integer, namX As String, _
    AvailableServers As String



    Private Sub winServers_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Dim iServers As Integer
        Dim iDbCount As Integer
        Dim iserver As String
        Dim oMyServer As New Smo.Server()
        ' Dim dtServer As DataTable
        Dim dt As DataTable = SmoApplication.EnumAvailableSqlServers()

        'Dim iCurrentServer As Integer
       
        '/search for available servers
        oRegisteredServer = New Smo.RegisteredServers.RegisteredServer
        ''oMyServer.LoginSecure = True
        'dtServer = oSQLServerDMOApp.EnumAvailableSqlServers()

        ' Me.pnlManual.Visible = False
        iServers = 0
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows 'oSQLServerDMOApp.EnumAvailableSqlServers()
                iServers = iServers + 1
                lstServers.Items.Add(dr("Name"))
                ''add servers in a group
                '' oServerGroup = oSQLServerDMOApp.ServerGroups.Item(iServers)
                tvwServers.Nodes.Add(New TreeNode("Name"))

                'list databases in the server
                ''oMyServer.Connect(oRegisteredServer.Name, "", "") '("dansonic\solns", "", "")
                ''iDbCount = 0
                ''For Each oDatabase In oMyServer.Databases
                ''    lstServers.Items.Add("               " & oDatabase.Name)
                ''    tvwServers.Nodes(iServers - 1).Nodes(0).Nodes.Add(New TreeNode(oDatabase.Name))
                ''    iDbCount = iDbCount + 1
                ''Next
            Next
        Else

            'manually enter details
            ''  Me.pnlManual.Visible = True
        End If
        'add local server
        'lstServers.Items.Add("(local)")


    End Sub

    Private Sub btnLogin_Click(sender As Object, e As RoutedEventArgs) Handles btnLogin.Click
        ''  On Error GoTo Err
        ' Dim oServer As new SQLDMO.SQLServer
        ' Specify the OLE DB provider.
        Dim strKey As String

        cnServer.Provider = "sqloledb"
        UserName = Trim(Me.txtUName.Text)
        Password = Trim(txtPswd.Text)


        If Me.pnlManual.Visible = True Then
            ServerName = Trim(Me.txtServer.Text)
            DatabaseName = Trim(Me.txtDB.Text)
        Else
            ServerName = Trim(Me.tvwServers.SelectedNode.Parent.Text)
            DatabaseName = Trim(Me.tvwServers.SelectedNode.Text)
        End If

        'myServer.LoginSecure = True
        ' Set SQLOLEDB connection properties.
        cnServer.Properties("Data Source").Value = ServerName
        cnServer.Properties("Initial Catalog").Value = DatabaseName


        'Check on server status
        'myServer.Name = ServerName
        ' If myServer.Status = SQLDMO_SVCSTATUS_TYPE.SQLDMOSvc_Stopped Then
        ' ''Try
        ' ''    ' start the server
        ' ''    myServer.Start(False, ServerName)
        ' ''    'ElseIf myServer.Status = SQLDMO_SVCSTATUS_TYPE.SQLDMOSvc_Paused Then
        ' ''Catch
        ' ''    'If paused Then continue()
        ' ''    myServer.[Continue]()
        ' ''    '  End If.
        ' ''    ' Catch
        ' ''End Try
        '' '' Decision code for login authorization type: 
        ' Windows NT or SQL Server authentication.
        If chkAuthentication.Checked = True Then
            cnServer.Properties("Integrated Security").Value = "SSPI"
            strLogin = "Provider=SQLOLEDB;Data Source=" & ServerName & ";Initial Catalog=" & DatabaseName & ";Integrated Security=SSPI;"
        Else
            cnServer.Properties("User ID").Value = UserName
            cnServer.Properties("Password").Value = Password
            strLogin = "Provider=SQLOLEDB;Data Source=" & ServerName & ";Initial Catalog=" & DatabaseName & ";User ID=" & UserName & ";Password=" & Password & ";"
        End If
        Try
            ' Open the server.
            cnServer.Open()
            If cnServer.State <> 1 Then
                'strLogin ' = "Provider=SQLOLEDB;Data Source=" & ServerName & ";Initial Catalog=" & DatabaseName & ";Integrated Security=SSPI;"
                cnServer.Open(strLogin)
            End If
        Catch

        End Try
        '' strCon = "Provider=SQLOLEDB;Data Source=(local);Initial Catalog=PSFSys;Integrated Security=SSPI;"


        If cnServer.State = 1 Then 'prompt to save that as default logi string
            If UserName = "" Then UserName = "sa"
            '/strLogin = "Provider=SQLOLEDB;Data Source=" & ServerName & ";Initial Catalog=" & DatabaseName & ";Integrated Security=SSPI;"

            '      **      strLogin = "Provider=SQLOLEDB;Data Source=" & ServerName & ";Initial Catalog=" & DatabaseName & ";User ID=" & UserName & ";Password=" & Password & ";"

            'strLogin = "Provider =SQLOLEDB;" & "Persist Security Info=False;User ID=" & UserName & ";Password=" & Password & ";Initial Catalog=" & DatabaseName & ";Data Source=" & ServerName


            ''strLogin = "Provider =" & "SQLOLEDB.1;" & "Persist Security Info=False;User ID=" & UserName & ";Initial Catalog " & DatabaseName & ";Data Source=" & ServerName

            ''check the existing data
            Try
                regCheck = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKey, True)
                strKey = (regCheck.GetValue("Login Details"))
            Catch
                strKey = ""
            End Try
            If strKey = "" Then   ', & " User Name")  ', "User Name", bHKeyCurrentUser:=True)

                If MsgBox("Do you want to use this as the default connection?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, ) = MsgBoxResult.Yes Then
                    'create the new registry key under HKEY_CURRENT_USER
                    regOpen = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(m_sRegKey)
                    'open the key with write access
                    regOpen = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKey, True)
                    'setup the value
                    regOpen.SetValue("Login Details", strLogin)


                    Me.Visible = False
                    'close the key
                    regOpen.Close()
                    'set the defult instance//key
                    regOpen = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(m_sRegKeyDS)
                    regOpen = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(m_sRegKeyDS, True)
                    regOpen.SetValue("UseDefault", "True")

                End If
                Try
                    regCheck.Close()
                Catch
                    ' //
                End Try
            End If 'null check
            Me.Hide()
            frmLogin.Show()
            frmLogin.bnStart = False
            frmLogin.strLogIn = strLogin
            frmLogin.strPswd = strLogin
        Else
            MsgBox("No server found please contact the Network or Server Administrator" & vbCrLf _
            & "The program will close", MsgBoxStyle.Critical)
            End
        End If

    End Sub
End Class
