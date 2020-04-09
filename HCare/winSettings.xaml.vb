Imports MahApps.Metro
Imports MahApps.Metro.Controls
Imports System.Windows.Threading
Imports System.Windows.Media

Imports System
Imports System.Security
Imports Microsoft.Win32


Public Class winSettings

    Private regTheme As RegistryKey
    Private Const m_sRegKeyST As String = "Software\Alpha Solutions\HCIS\Settings"
    Private myColors As Color() = New Color() {Color.FromRgb(&HA4, &HC4, &H0), Color.FromRgb(&H60, &HA9, &H17), Color.FromRgb(&H0, &H8A, &H0), Color.FromRgb(&H0, &HAB, &HA9), Color.FromRgb(&H1B, &HA1, &HE2), Color.FromRgb(&H0, &H50, &HEF), _
            Color.FromRgb(&H6A, &H0, &HFF), Color.FromRgb(&HAA, &H0, &HFF), Color.FromRgb(&HF4, &H72, &HD0), Color.FromRgb(&HD8, &H0, &H73), Color.FromRgb(&HA2, &H0, &H25), Color.FromRgb(&HE5, &H14, &H0), _
            Color.FromRgb(&HFA, &H68, &H0), Color.FromRgb(&HF0, &HA3, &HA), Color.FromRgb(&HE3, &HC8, &H0), Color.FromRgb(&H82, &H5A, &H2C), Color.FromRgb(&H6D, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
            Color.FromRgb(&H76, &H60, &H8A), Color.FromRgb(&H87, &H79, &H4E)}

    Private strTheme As String = ""
    Private intTheme As Integer = 0
    Private strColor As String

    Private Sub winSettings_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized


        Dim myBrush As New SolidColorBrush(myColors(0))
        cboTheme.Items.Add("Dark")
        cboTheme.Items.Add("Light")

        lime.Background = myBrush

        myBrush = New SolidColorBrush(myColors(1))
        Green.Background = myBrush

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
        Orange.Background = myBrush

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

    End Sub


    Private Sub Lime_Click(sender As Object, e As RoutedEventArgs) Handles lime.Click

        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If

        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Lime"), ThemeManager.AppThemes(intTheme))
        strColor = "Lime"

        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Lime")
        regTheme.Close()



    End Sub


    Private Sub Green_Click(sender As Object, e As RoutedEventArgs) Handles Green.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Green"), ThemeManager.AppThemes(intTheme))
        strColor = "Green"
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Green")
        regTheme.Close()
    End Sub

    Private Sub Emerald_Click(sender As Object, e As RoutedEventArgs) Handles emerald.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Emerald"), ThemeManager.AppThemes(intTheme))
        strColor = "Emerald"
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Emerald")
        regTheme.Close()
    End Sub

    Private Sub Teal_Click(sender As Object, e As RoutedEventArgs) Handles teal.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Teal"), ThemeManager.AppThemes(intTheme))
        strColor = "Teal"
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Teal")
        regTheme.Close()
    End Sub

    Private Sub Cyan_Click(sender As Object, e As RoutedEventArgs) Handles cyan.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Cyan"), ThemeManager.AppThemes(intTheme))
        strColor = "Cyan"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Cyan")
        'close the key
        regTheme.Close()

    End Sub

    Private Sub Cobalt_Click(sender As Object, e As RoutedEventArgs) Handles cobalt.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Cobalt"), ThemeManager.AppThemes(intTheme))
        strColor = "Cobalt"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Cobalt")
        'close the key
        regTheme.Close()
    End Sub

    Private Sub Indigo_Click(sender As Object, e As RoutedEventArgs) Handles indigo.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Indigo"), ThemeManager.AppThemes(intTheme))
        strColor = "Indigo"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Indigo")
        'close the key
        regTheme.Close()

    End Sub

    Private Sub Violet_Click(sender As Object, e As RoutedEventArgs) Handles violet.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Violet"), ThemeManager.AppThemes(intTheme))
        strColor = "Violet"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Violet")
        'close the key
        regTheme.Close()


    End Sub

    Private Sub Pink_Click(sender As Object, e As RoutedEventArgs) Handles pink.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Pink"), ThemeManager.AppThemes(intTheme))
        strColor = "Pink"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Pink")
        'close the key
        regTheme.Close()

    End Sub

    Private Sub Magenta_Click(sender As Object, e As RoutedEventArgs) Handles magenta.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Magenta"), ThemeManager.AppThemes(intTheme))
        strColor = "Magenta"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Magenta")
        'close the key
        regTheme.Close()

    End Sub

    Private Sub Crimson_Click(sender As Object, e As RoutedEventArgs) Handles crimson.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Crimson"), ThemeManager.AppThemes(intTheme))
        strColor = "Crimson"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Crimson")
        'close the key
        regTheme.Close()

    End Sub

    Private Sub Red_Click(sender As Object, e As RoutedEventArgs) Handles red.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Red"), ThemeManager.AppThemes(intTheme))
        strColor = "Red"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Red")
        'close the key
        regTheme.Close()

    End Sub

    Private Sub Orange_Click(sender As Object, e As RoutedEventArgs) Handles Orange.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Orange"), ThemeManager.AppThemes(intTheme))
        strColor = "Orange"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Orange")
        'close the key
        regTheme.Close()
    End Sub

    Private Sub Amber_Click(sender As Object, e As RoutedEventArgs) Handles amber.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Amber"), ThemeManager.AppThemes(intTheme))
        strColor = "Amber"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Amber")
        'close the key
        regTheme.Close()
    End Sub

    Private Sub Yellow_Click(sender As Object, e As RoutedEventArgs) Handles yellow.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Yellow"), ThemeManager.AppThemes(intTheme))
        strColor = "Yellow"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Yellow")
        'close the key
        regTheme.Close()
    End Sub


    Private Sub Brown_Click(sender As Object, e As RoutedEventArgs) Handles brown.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Brown"), ThemeManager.AppThemes(intTheme))
        strColor = "Brown"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Brown")
        'close the key
        regTheme.Close()
    End Sub

    Private Sub Olive_Click(sender As Object, e As RoutedEventArgs) Handles olive.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Olive"), ThemeManager.AppThemes(intTheme))
        strColor = "Olive"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Olive")
        'close the key
        regTheme.Close()
    End Sub

    Private Sub Steel_Click(sender As Object, e As RoutedEventArgs) Handles steel.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Steel"), ThemeManager.AppThemes(intTheme))
        strColor = "Steel"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Steel")
        'close the key
        regTheme.Close()

    End Sub

    Private Sub Mauve_Click(sender As Object, e As RoutedEventArgs) Handles mauve.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Mauve"), ThemeManager.AppThemes(intTheme))
        strColor = "Mauve"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Mauve")
        'close the key
        regTheme.Close()


    End Sub

    Private Sub Taupe_Click(sender As Object, e As RoutedEventArgs) Handles taupe.Click
        If strTheme = "Dark" Then
            intTheme = 1
        ElseIf strTheme = "Light" Then
            intTheme = 0
        End If
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(Function(a) a.Name = "Taupe"), ThemeManager.AppThemes(intTheme))
        strColor = "Taupe"
        'create the new registry key under HKEY_CURRENT_USER
        regTheme = Registry.CurrentUser.CreateSubKey(m_sRegKeyST)
        'open the key with write access
        regTheme = Registry.CurrentUser.OpenSubKey(m_sRegKeyST, True)
        'setup the value
        regTheme.SetValue("Theme", strTheme)
        regTheme.SetValue("Color", "Taupe")
        'close the key
        regTheme.Close()

    End Sub


    Private Sub cboTheme_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboTheme.SelectionChanged
        strTheme = cboTheme.SelectedItem
        If strTheme = "Dark" Then
            intTheme = 1
            ThemeManager.AddAppTheme("TestTheme", New Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseDark.xaml"))
            ThemeManager.ChangeAppTheme(Application.Current, "TestTheme")
        ElseIf strTheme = "Light" Then
            intTheme = 0
            ThemeManager.AddAppTheme("TestTheme", New Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml"))
            ThemeManager.ChangeAppTheme(Application.Current, "TestTheme")
        Else

        End If

    End Sub
End Class

