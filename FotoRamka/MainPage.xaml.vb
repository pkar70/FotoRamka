' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Partial Public NotInheritable Class MainPage
    Inherits Page

    Private _sAlbum As String = "public"
    Private _sUrl As String = ""
    Private moTimer As DispatcherTimer = Nothing
    Private _iTickTime As Integer = 60

    Private Async Sub uiStartStop_Click(sender As Object, e As RoutedEventArgs)
        If moTimer IsNot Nothing Then
            TykanieStop()
            uiStartStop.Label = "Start"
        Else
            Await NextPic()
            TykanieStart()
            uiStartStop.Label = "Stop"
        End If
    End Sub

    Private Sub uiDelay_Click(sender As Object, e As RoutedEventArgs)
        Dim oMI As ToggleMenuFlyoutItem = sender
        Dim sName As String = oMI.Name
        If Not sName.StartsWith("uiDelay") Then Return

        Try
            _iTickTime = sName.ToLower.Replace("uidelay", "")
        Catch ex As Exception
            _iTickTime = 60
        End Try

        SetSettingsInt("tickTime", _iTickTime)
        ' SetCheckMarks()
    End Sub

    Private Sub uiAlbum_Click(sender As Object, e As RoutedEventArgs)
        Dim oMI As ToggleMenuFlyoutItem = sender
        Dim sName As String = oMI.Name
        If Not sName.StartsWith("uiAlbum") Then Return

        _sAlbum = sName.ToLower.Replace("uialbum", "")

        SetSettingsString("albumName", _sAlbum)
        ' SetCheckMarks()
    End Sub

    Private Sub SetTitlePicName(sUrl As String)
        If pkar.NetIsCellInet Then Return
        Dim sTitle As String = sUrl
        sTitle = sTitle.Replace(BaseServiceUri & "/p/store/", "")

        If sTitle.ToLower.Contains("myproduction/models/") Then
            ' http://../p/store/Public/MyProduction/Models/A/Ad/AdrianaLima/Models.001/Adriana Lima Flood166.jpg
            ' //Public/MyProduction/Models/A/Ad/AdrianaLima/Models.001/Adriana Lima Flood166.jpg
            sTitle = sTitle.Substring("Public/MyProduction/Models/".Length)
            ' A/Ad/AdrianaLima/Models.001/Adriana Lima Flood166.jpg
            If sTitle.Substring(1, 1) = "/" Then
                sTitle = "Models//" & sTitle.Substring(5)
            Else
                sTitle = "Models/" & sTitle ' dla Covers, Misc, itp.
            End If
        End If

        If sTitle.ToLower.Contains("myproduction/ero/") Then
            sTitle = sTitle.Substring("Public/MyProduction/".Length)
            sTitle = "//" & sTitle.Substring(5)
        End If

        'sWhereFullFoto = " ( ( path LIKE '%\FotoVideo\Analogowe\Zdjecia\%' OR path LIKE '%\FotoVideo\Cyfrowe\Zdjecia\%' ) AND path NOT LIKE '%\Ego\%' AND path NOT LIKE '%\Grlfrn\%' AND name LIKE '%jpg' ) "
        'sWherePublic = " path LIKE '%\Public\%' "
        'sWhereRodzina = " ( path LIKE '%\FotoVideo\Analogowe\Zdjecia\dMilek\%' OR  path LIKE '%\FotoVideo\Analogowe\Zdjecia\Tata\%' AND name LIKE '%jpg' ) "


        ApplicationView.GetForCurrentView.Title = sTitle   ' było "FotoRamka - " &..., ale on dopisuje Fotoramka na końcu (po nazwie)
    End Sub

    Private Async Function NextPic() As Task

        Dim sUrl As String = BaseServiceUri & "/p/dysk/gerrandompic.asp?album=" & _sAlbum

        sUrl = Await HttpPageAsync(sUrl, "random id", False)
        If Not sUrl.StartsWith("http") Then Return

        _sUrl = sUrl

        Dim oBmp As BitmapImage = New BitmapImage(New Uri(sUrl))
        uiImage.Source = oBmp
        SetTitlePicName(sUrl)
    End Function



    Private Sub uiPic_Tapped(sender As Object, e As RoutedEventArgs)
        Dim oResize As Stretch = uiImage.Stretch
        Select Case oResize
            Case Stretch.Uniform
                uiImage.Stretch = Stretch.None
            Case Stretch.None
                uiImage.Stretch = Stretch.Uniform
        End Select

    End Sub
    Private Sub uiGetInfo_Click(sender As Object, e As RoutedEventArgs)

    End Sub
    Private Sub uiGetUrl_Click(sender As Object, e As RoutedEventArgs)
        ClipPut(_sUrl)
        DialogBox(_sUrl)
    End Sub

    Private Sub TykanieStart()
        Try
            moTimer = New DispatcherTimer
            moTimer.Interval = TimeSpan.FromSeconds(_iTickTime)

            AddHandler moTimer.Tick, AddressOf Timer_TickUI
            'mTimerMode = 0
            moTimer.Start()
            Return
        Catch ex As Exception
            CrashMessageExit("@InitTykanie", ex.Message)

        End Try
    End Sub

    Private Sub TykanieStop()
        If moTimer IsNot Nothing Then
            Try
                RemoveHandler moTimer.Tick, AddressOf Timer_TickUI
                moTimer.Stop()
                moTimer = Nothing
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Async Sub Timer_TickUI(sender As Object, e As Object)
        ' If mbInDebug Then Debug.WriteLine("BasicCamera:Timer_TickUI, " & Date.Now.ToString("HH:mm:ss"))

        moTimer?.Stop()

        Await NextPic()

        moTimer?.Start()

    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        ' wczytanie zmiennych z SetSettings i ustawienie ToggleMFI
        _sAlbum = GetSettingsString("albumName", "public")
        _iTickTime = GetSettingsInt("tickTime", 60)

        SetCheckMarks()
    End Sub

    Private Sub SetCheckMarks()

        Dim sDelay As String = _iTickTime.ToString
        Dim bSelected As Boolean = False
        For Each oMFI As ToggleMenuFlyoutItem In uiMenuDelay.Items
            If oMFI.Name.EndsWith(sDelay) Then
                oMFI.IsChecked = True
                bSelected = True
            Else
                oMFI.IsChecked = False
            End If
        Next
        If Not bSelected Then uiDelay60.IsChecked = True

        For Each oMFI As ToggleMenuFlyoutItem In uiMenuAlbum.Items
            If oMFI.Name.EndsWith(_sAlbum) Then
                oMFI.IsChecked = True
                bSelected = True
            Else
                oMFI.IsChecked = False
            End If
        Next
        If Not bSelected Then uiAlbumPublic.IsChecked = True


    End Sub

    Private Sub Page_LosingFocus(sender As UIElement, args As LosingFocusEventArgs)
        If Not pkar.NetIsCellInet Then Return
        moTimer?.Stop()
    End Sub

    Private Sub Page_GotFocus(sender As Object, e As RoutedEventArgs)
        If Not pkar.NetIsCellInet Then Return
        moTimer?.Start()
    End Sub

    Private Sub uiGoNext_Click(sender As Object, e As RoutedEventArgs)
        Timer_TickUI(Nothing, Nothing)
    End Sub
End Class
