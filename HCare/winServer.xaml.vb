Imports System.Data
Imports System.Data.Sql
Imports Microsoft.SqlServer.Management.Smo


Imports System.Security
Imports Microsoft.Win32
Imports MahApps.Metro
Imports MahApps.Metro.Controls

Public Class winServer


    Sub retrieveServers()
        Dim instance As SqlDataSourceEnumerator = SqlDataSourceEnumerator.Instance
        Dim table As System.Data.DataTable = instance.GetDataSources()

        DisplayData(table)



    End Sub

    Private Sub DisplayData(ByVal table As DataTable)
        cboServer.Items.Clear()
        For Each row As DataRow In table.Rows
            For Each col As DataColumn In table.Columns
                cboServer.Items.Add("{0} = {1}" & ", " & col.ColumnName & " , " & row(col))
            Next
        Next
    End Sub

    Private Sub winServer_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized

        Dim dt As DataTable = Nothing, dr As DataRow = Nothing

        Try

            dt = SmoApplication.EnumAvailableSqlServers()


            For Each dr In dt.Rows
                cboServer.Items.Add(dr.Item(0).ToString)
            Next

        Catch ex As System.Data.SqlClient.SqlException
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Error!")

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "Error!")

        Finally
            dr = Nothing
            dt = Nothing
        End Try
        LoadSQLServers(cboServer)
    End Sub


    Private Sub LoadSQLServers(ByVal sqlCombo As ComboBox)
        Dim sqlServerList As List(Of String) = New List(Of String)

        cboServer.Items.Clear()

        Try
            Dim dt As DataTable = SmoApplication.EnumAvailableSqlServers(False)
            If dt.Rows.Count > 0 Then
                For Each dr As DataRow In dt.Rows
                    If Not sqlServerList.Contains(dr("Name").ToString) Then
                        sqlServerList.Add(dr("Name").ToString)
                        cboServer.Items.Add(dr("Name").ToString)
                    End If
                Next
            End If
        Catch ex As Exception
        End Try

        Try
            Dim rk As RegistryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\Microsoft\Microsoft SQL Server")
            Dim instances As String() = CType(rk.GetValue("InstalledInstances"), String())
            If (instances.Length > 0) Then
                For Each element As String In instances
                    If element = "MSSQLSERVER" Then
                        If Not sqlServerList.Contains(System.Environment.MachineName) Then
                            sqlServerList.Add(System.Environment.MachineName)
                        End If
                    Else
                        If Not sqlServerList.Contains(System.Environment.MachineName + "\" + element) Then
                            sqlServerList.Add(System.Environment.MachineName + "\" + element)
                        End If
                    End If
                Next
            End If
        Catch ex As Exception
        Finally
        End Try

    End Sub

End Class
