
Public Class winPswd

    Private Sub winPswd_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Dim tiU As New TabItem
        Dim fiU As New Frame
        Dim intTab As Integer
        Dim iTabCount As Integer = 1
        Dim ti As New TabItem

        If tcUser.Items.Count = 0 Then
            tiU.Header = "_User"
            tiU.Name = "User"
            fiU.NavigationService.Navigate(New pgUser)
            tiU.Content = fiU
            tcUser.Items.Add(tiU)
            intTab = tcUser.Items.Count - 1
            tcUser.SelectedItem = tcUser.Items(intTab)
            Exit Sub
        Else
            intTab = 0
            For Each tiU In tcUser.Items
                If tiU.Name = "User" Then
                    tcUser.SelectedItem = tcUser.Items(intTab)
                    Exit Sub
                End If
                intTab = intTab + 1
            Next


            ti.Header = "_User"
            ti.Name = "User"
            fiU.NavigationService.Navigate(New pgUser)
            ti.Content = fiU
            tcUser.Items.Add(ti)
            intTab = tcUser.Items.Count - 1
            tcUser.SelectedItem = tcUser.Items(intTab)
        End If

    End Sub
End Class
