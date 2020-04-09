
Imports System.Data
Imports ADODB
Imports System.Data.OleDb

Class pgMyQueue
    Private rsQueue As New ADODB.Recordset
    Private dtQueue As New DataTable
    Private daQueue As New OleDbDataAdapter
    Private MainWin As New MainWindow
    Public strUser As String
    Private strQueue As String = ""
    Public strDesign As String = ""
    Public dgBrush As New SolidColorBrush

    Private Sub pgMyQueue_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        txtSearch.IsEnabled = False

    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtSearch.TextChanged

        If Me.txtSearch.Text = "" Then
            dgQueue.ItemsSource = ""
            dtQueue.Clear()
            With rsQueue
                If .State = 1 Then .Close()
                .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                .Open("SELECT QDate as Date, QTime as Time, PatNo, Pname, Destination, Status, SendBy FROM tblQueue WHERE destination='" & strQueue & "' AND Status='Waiting'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
                If .RecordCount > 0 Then
                    lblRecNo.Content = .RecordCount & " Person(s) in queue"
                Else
                    lblRecNo.Content = "Nobody in your queue"
                End If
            End With
            daQueue.Fill(dtQueue, rsQueue)
            dgQueue.ItemsSource = dtQueue.DefaultView
        Else
            SearchPatient()
        End If


    End Sub


    Private Sub SearchPatient()
        dgQueue.ItemsSource = ""
        dtQueue.Clear()
        If Me.txtSearch.Text <> "" Then
            If IsNumeric(txtSearch.Text) = True Then
            Else

                With rsQueue
                    If .State = 1 Then .Close()
                    .CursorLocation = ADODB.CursorLocationEnum.adUseClient
                    .Open("SELECT QDate as Date, QTime as Time, PatNo, Destination, Status, SendBy FROM tblQueue WHERE destination='" & strQueue & "' OR PatNo LIKE '%" & txtSearch.Text & "%' OR PName LIKE '%" & txtSearch.Text & "%'  OR sendBy LIKE '%" & txtSearch.Text & "%' HAVING Status='Waiting'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)

                    If .RecordCount > 0 Then
                        lblRecNo.Content = .RecordCount & " Records found"
                    Else
                        lblRecNo.Content = "No record to display"
                    End If

                End With
            End If
            daQueue.Fill(dtQueue, rsQueue)
            dgQueue.ItemsSource = dtQueue.DefaultView
        Else
        End If


    End Sub

    Private Sub pgMyQueue_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        dgQueue.ItemsSource = ""
        dtQueue.Clear()
        strDesign = LCase(strDesign)
        dgQueue.BorderThickness = New Thickness(1)
        dgQueue.BorderBrush = dgBrush
        dgQueue.AlternatingRowBackground = dgBrush

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
        End If

        With rsQueue
            If .State = 1 Then .Close()
            .CursorLocation = CursorLocationEnum.adUseClient
            .Open("SELECT QDate as Date, QTime as Time, PatNo, PName, Destination, Status, SendBy, Remarks FROM tblQueue WHERE destination='" & strQueue & "' AND Status='Waiting'", MainWin.cnHCIS, CursorTypeEnum.adOpenDynamic, LockTypeEnum.adLockReadOnly)
            If .RecordCount > 0 Then
                lblRecNo.Content = .RecordCount & " Person(s) in queue"
            Else
                lblRecNo.Content = "Nobody in your queue"
            End If
        End With

        daQueue.Fill(dtQueue, rsQueue)
        dgQueue.ItemsSource = dtQueue.DefaultView
    End Sub
End Class
