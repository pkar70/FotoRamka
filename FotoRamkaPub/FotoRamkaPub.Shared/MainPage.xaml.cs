using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FotoRamka
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }


        // The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


            private string _sAlbum = "public";
            private string _sUrl = "";
            private DispatcherTimer moTimer = null;
            private int _iTickTime = 60;

            private async void uiStartStop_Click(object sender, RoutedEventArgs e)
            {
                if (moTimer is object)
                {
                    TykanieStop();
                    uiStartStop.Label = "Start";
                }
                else
                {
                    await NextPic();
                    TykanieStart();
                    uiStartStop.Label = "Stop";
                }
            }

            private void uiDelay_Click(object sender, RoutedEventArgs e)
            {
                ToggleMenuFlyoutItem oMI = (ToggleMenuFlyoutItem)sender;
                string sName = oMI.Name;
                if (!sName.StartsWith("uiDelay"))
                    return;

                int iValue;
                sName = sName.ToLower().Replace("uidelay", "");
                if(!int.TryParse(sName, out iValue))
                    _iTickTime = 60;

                p.k.SetSettingsInt("tickTime", _iTickTime);
                // SetCheckMarks()
            }

            private void uiAlbum_Click(object sender, RoutedEventArgs e)
            {
                ToggleMenuFlyoutItem oMI = (ToggleMenuFlyoutItem)sender;
                string sName = oMI.Name;
                if (!sName.StartsWith("uiAlbum"))
                    return;
                _sAlbum = sName.ToLower().Replace("uialbum", "");
            p.k.SetSettingsString("albumName", _sAlbum);
                // SetCheckMarks()
            }

            private void SetTitlePicName(string sUrl)
            {
                if (p.k.NetIsCellInet())
                    return;

#if NETFX_CORE

                string sTitle = sUrl;
                sTitle = sTitle.Replace(BaseServiceUri + "/p/store/", "");
                if (sTitle.ToLower().Contains("myproduction/models/"))
                {
                    // http://../p/store/Public/MyProduction/Models/A/Ad/AdrianaLima/Models.001/Adriana Lima Flood166.jpg
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


            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = sTitle;   // było "FotoRamka - " &..., ale on dopisuje Fotoramka na końcu (po nazwie)
#endif

        }

        private void AndroPanel(bool bVisible)
        {
#if __ANDROID__
            if(bVisible)
                uiAndroPanel.Visibility = Visibility.Visible;
            else
                uiAndroPanel.Visibility = Visibility.Collapsed;

#endif 
        }
        private async System.Threading.Tasks.Task NextPic()
            {
                string sUrl = BaseServiceUri + "/p/dysk/gerrandompic.asp?album=" + _sAlbum;
                sUrl = await p.k.HttpPageAsync(sUrl, "random id", false);
                if (!sUrl.StartsWith("http"))
                    return;
                _sUrl = sUrl;
                var oBmp = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(sUrl));
                this.uiImage.Source = oBmp;
                SetTitlePicName(sUrl);
            AndroPanel(false);
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

            AndroPanel(true);

        }

        private void uiGetInfo_Click(object sender, RoutedEventArgs e)
            {
            }

            private void uiGetUrl_Click(object sender, RoutedEventArgs e)
            {
            p.k.ClipPut(_sUrl);
            p.k.DialogBox(_sUrl);
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
                p.k.CrashMessageExit("@InitTykanie", ex.Message);
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
                _sAlbum = p.k.GetSettingsString("albumName", "public");
                _iTickTime = p.k.GetSettingsInt("tickTime", 60);
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
                if (!p.k.NetIsCellInet())
                    return;
                moTimer?.Stop();
            }

            private void Page_GotFocus(object sender, RoutedEventArgs e)
            {
                if (!p.k.NetIsCellInet())
                    return;
                moTimer?.Start();
            }

            private void uiGoNext_Click(object sender, RoutedEventArgs e)
            {
                Timer_TickUI(null, null);
            }
        }
    }


