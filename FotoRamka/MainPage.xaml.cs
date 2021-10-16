// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
using System;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace FotoRamka
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private string _sAlbum = "public";
        private string _sUrl = "";
        private DispatcherTimer moTimer = null;
        private int _iTickTime = 60;

        private async void uiStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (moTimer is object)
            {
                TykanieStop();
                this.uiStartStop.Label = "Start";
            }
            else
            {
                await NextPic();
                TykanieStart();
                this.uiStartStop.Label = "Stop";
            }
        }

        private void uiDelay_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem oMI = (ToggleMenuFlyoutItem)sender;
            string sName = oMI.Name;
            if (!sName.StartsWith("uiDelay"))
                return;
            try
            {
                _iTickTime = Conversions.ToInteger(sName.ToLower().Replace("uidelay", ""));
            }
            catch (Exception ex)
            {
                _iTickTime = 60;
            }

            FotoRamka.pkar.SetSettingsInt("tickTime", _iTickTime);
            // SetCheckMarks()
        }

        private void uiAlbum_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem oMI = (ToggleMenuFlyoutItem)sender;
            string sName = oMI.Name;
            if (!sName.StartsWith("uiAlbum"))
                return;
            _sAlbum = sName.ToLower().Replace("uialbum", "");
            FotoRamka.pkar.SetSettingsString("albumName", _sAlbum);
            // SetCheckMarks()
        }

        private void SetTitlePicName(string sUrl)
        {
            if (FotoRamka.pkar.NetIsMobile())
                return;
            string sTitle = sUrl;
            sTitle = sTitle.Replace("http://beskid.geo.uj.edu.pl/p/store/", "");
            if (sTitle.ToLower().Contains("myproduction/models/"))
            {
                // http://beskid.geo.uj.edu.pl/p/store/Public/MyProduction/Models/A/Ad/AdrianaLima/Models.001/Adriana Lima Flood166.jpg
                // //Public/MyProduction/Models/A/Ad/AdrianaLima/Models.001/Adriana Lima Flood166.jpg
                sTitle = sTitle.Substring("Public/MyProduction/Models/".Length);
                // A/Ad/AdrianaLima/Models.001/Adriana Lima Flood166.jpg
                if (sTitle.Substring(1, 1) == "/")
                {
                    sTitle = "Models//" + sTitle.Substring(5);
                }
                else
                {
                    sTitle = "Models/" + sTitle;
                } // dla Covers, Misc, itp.
            }

            if (sTitle.ToLower().Contains("myproduction/ero/"))
            {
                sTitle = sTitle.Substring("Public/MyProduction/".Length);
                sTitle = "//" + sTitle.Substring(5);
            }

            // sWhereFullFoto = " ( ( path LIKE '%\FotoVideo\Analogowe\Zdjecia\%' OR path LIKE '%\FotoVideo\Cyfrowe\Zdjecia\%' ) AND path NOT LIKE '%\Ego\%' AND path NOT LIKE '%\Grlfrn\%' AND name LIKE '%jpg' ) "
            // sWherePublic = " path LIKE '%\Public\%' "
            // sWhereRodzina = " ( path LIKE '%\FotoVideo\Analogowe\Zdjecia\dMilek\%' OR  path LIKE '%\FotoVideo\Analogowe\Zdjecia\Tata\%' AND name LIKE '%jpg' ) "


            ApplicationView.GetForCurrentView().Title = sTitle;   // było "FotoRamka - " &..., ale on dopisuje Fotoramka na końcu (po nazwie)
        }

        private async Task NextPic()
        {
            string sUrl = "http://beskid.geo.uj.edu.pl/p/dysk/gerrandompic.asp?album=" + _sAlbum;
            sUrl = await FotoRamka.pkar.HttpPageAsync(sUrl, "random id", false);
            if (!sUrl.StartsWith("http"))
                return;
            _sUrl = sUrl;
            var oBmp = new BitmapImage(new Uri(sUrl));
            this.uiImage.Source = oBmp;
            SetTitlePicName(sUrl);
        }

        private void uiPic_Tapped(object sender, RoutedEventArgs e)
        {
            var oResize = this.uiImage.Stretch;
            switch (oResize)
            {
                case Stretch.Uniform:
                    {
                        this.uiImage.Stretch = Stretch.None;
                        break;
                    }

                case Stretch.None:
                    {
                        this.uiImage.Stretch = Stretch.Uniform;
                        break;
                    }
            }
        }

        private void uiGetInfo_Click(object sender, RoutedEventArgs e)
        {
        }

        private void uiGetUrl_Click(object sender, RoutedEventArgs e)
        {
            FotoRamka.pkar.ClipPut(_sUrl);
            FotoRamka.pkar.DialogBox(_sUrl);
        }

        private void TykanieStart()
        {
            try
            {
                moTimer = new DispatcherTimer();
                moTimer.Interval = TimeSpan.FromSeconds(_iTickTime);
                moTimer.Tick += Timer_TickUI;
                // mTimerMode = 0
                moTimer.Start();
                return;
            }
            catch (Exception ex)
            {
                FotoRamka.pkar.CrashMessageExit("@InitTykanie", ex.Message);
            }
        }

        private void TykanieStop()
        {
            if (moTimer is object)
            {
                try
                {
                    moTimer.Tick -= Timer_TickUI;
                    moTimer.Stop();
                    moTimer = null;
                }
                catch (Exception ex)
                {
                }
            }
        }

        private async void Timer_TickUI(object sender, object e)
        {
            // If mbInDebug Then Debug.WriteLine("BasicCamera:Timer_TickUI, " & Date.Now.ToString("HH:mm:ss"))

            moTimer?.Stop();
            await NextPic();
            moTimer?.Start();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // wczytanie zmiennych z SetSettings i ustawienie ToggleMFI
            _sAlbum = FotoRamka.pkar.GetSettingsString("albumName", "public");
            _iTickTime = FotoRamka.pkar.GetSettingsInt("tickTime", 60);
            SetCheckMarks();
        }

        private void SetCheckMarks()
        {
            string sDelay = _iTickTime.ToString();
            bool bSelected = false;
            foreach (ToggleMenuFlyoutItem oMFI in this.uiMenuDelay.Items)
            {
                if (oMFI.Name.EndsWith(sDelay))
                {
                    oMFI.IsChecked = true;
                    bSelected = true;
                }
                else
                {
                    oMFI.IsChecked = false;
                }
            }

            if (!bSelected)
                this.uiDelay60.IsChecked = true;
            foreach (ToggleMenuFlyoutItem oMFI in this.uiMenuAlbum.Items)
            {
                if (oMFI.Name.EndsWith(_sAlbum))
                {
                    oMFI.IsChecked = true;
                    bSelected = true;
                }
                else
                {
                    oMFI.IsChecked = false;
                }
            }

            if (!bSelected)
                this.uiAlbumPublic.IsChecked = true;
        }

        private void Page_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            if (!FotoRamka.pkar.NetIsMobile())
                return;
            moTimer?.Stop();
        }

        private void Page_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!FotoRamka.pkar.NetIsMobile())
                return;
            moTimer?.Start();
        }

        private void uiGoNext_Click(object sender, RoutedEventArgs e)
        {
            Timer_TickUI(null, null);
        }
    }
}