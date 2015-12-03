using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Deezer_To_Vk_Sync.Deezer;
using Deezer_To_Vk_Sync.Vk;
using MahApps.Metro.Controls;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace Deezer_To_Vk_Sync
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Browser.Navigated += wbMain_Navigated;
        }

        void wbMain_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(Browser, true); // make it silent
        }
        #region Some magic to suppress browser script errors
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        #endregion

        VkApi vkApi = new VkApi();
        DeezerApi deezerApi = new DeezerApi();

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            Browser.Navigate(
                        $"http://api.vkontakte.ru/oauth/authorize?client_id={vkApi.appId}&scope={vkApi.scope}&display=popup&response_type=token");
        }

        private void Browser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Uri.ToString().IndexOf("access_token", StringComparison.Ordinal) != -1)
            {
                Regex myReg = new Regex(@"(?<name>[\w\d\x5f]+)=(?<value>[^\x26\s]+)",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match m in myReg.Matches(e.Uri.ToString()))
                {
                    if (vkApi._accessToken == null)
                    {
                        if (m.Groups["name"].Value == "access_token")
                        {
                            vkApi._accessToken = m.Groups["value"].Value;
                            Browser.Visibility = Visibility.Collapsed;
                            Height = 122;
                        }
                        else if (m.Groups["name"].Value == "user_id")
                        {
                            vkApi.userId = Convert.ToInt32(m.Groups["value"].Value);
                        }
                    }
                }

            }

        }


        private void Button_OnClick(object sender, RoutedEventArgs e)
        {

            if (textBox.Text != deezerApi.link || textBox.Text != null)
            {
                deezerApi._accessToken = textBox.Text;
            }

            string[] vkMus = vkApi.ParseAudioList(vkApi.GetAudio());
            string[] deezMus = deezerApi.ParsePlaylist(deezerApi.GetUserPlaylist());

            Task.Factory.StartNew(() => DivideMusic(vkMus, deezMus));
        }

        /// <summary>
        ///  Get Except of two arrays and set new collection to VK Audio
        /// </summary>
        /// <param name="vk"> array of VK compositions </param>
        /// <param name="deezer">array of deezer compositions param>
        private void DivideMusic(string[] vk, string[] deezer)
        {
            Array.Sort(vk);
            Array.Sort(deezer);

            IEnumerable<string> newMus = deezer.Except(vk);
           
            foreach (var item in newMus)
            {
                KeyValuePair<string, string> id = vkApi.ParseSearchList(vkApi.SearchAudio(item));
                if (Convert.ToInt32(id.Key) != 0)
                {
                    vkApi.SetAudio(id.Key, id.Value);
                }
            }
            MessageBox.Show("End");
        }

        private void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("Firefox", deezerApi.link);
        }
    }
}
