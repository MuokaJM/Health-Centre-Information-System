Imports CrystalDecisions.Shared
Imports SAPBusinessObjects
Imports CrystalDecisions.CrystalReports.Engine

Public Class winRptI


    Private Sub winRptI_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        On Error Resume Next
        crvMain.ShowOpenFileButton = False


    End Sub
End Class
