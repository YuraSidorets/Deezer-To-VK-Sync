using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using CefSharp;
using Deezer_To_Vk_Sync.Api;

namespace Deezer_To_Vk_Sync
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        VkApi vkApi = new VkApi();
        DeezerApi deezerApi = new DeezerApi();

        public MainWindow()
        {
           InitializeComponent();
            Browser.Address = $"http://api.vkontakte.ru/oauth/authorize?client_id={vkApi.appId}&scope={vkApi.scope}&display=popup&response_type=token";
        }
      
        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            string[] vkMus = vkApi.ParseAudioList(vkApi.GetAudio());
            string[] deezMus = deezerApi.ParsePlaylist(deezerApi.GetUserPlaylist());

            Task.Factory.StartNew(() => DivideMusic(vkMus, deezMus));
            Bar.IsIndeterminate = true;
        }

        /// <summary>
        /// Get Except of two arrays and set new collection to VK Audio
        /// </summary>
        /// <param name="vk"> array of VK compositions </param>
        /// <param name="deezer">array of deezer compositions </param>
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

            Dispatcher.BeginInvoke(new MethodInvoker(delegate
            {
                Bar.IsIndeterminate = false;
                TextBlock.Visibility = Visibility.Visible;
            }));
           
        }

       
        private void Browser_OnFrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            if (e.Url.IndexOf("access_token", StringComparison.Ordinal) != -1)
            {
                Regex myReg = new Regex(@"(?<name>[\w\d\x5f]+)=(?<value>[^\x26\s]+)",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match m in myReg.Matches(e.Url))
                {
                    if (vkApi._accessToken == null)
                    {
                        if (m.Groups["name"].Value == "access_token")
                        {
                            vkApi._accessToken = m.Groups["value"].Value;
                        }

                    }
                    else if (m.Groups["name"].Value == "user_id")
                    {
                        vkApi.userId = Convert.ToInt32(m.Groups["value"].Value);

                    }
                    else if (vkApi._accessToken != null && deezerApi._accessToken == null)
                    {
                        Browser.Load(deezerApi.link);
                        if (m.Groups["name"].Value == "access_token")
                        {
                            deezerApi._accessToken = m.Groups["value"].Value;

                            Dispatcher.BeginInvoke(new MethodInvoker(delegate
                            {
                                Browser.Visibility = Visibility.Hidden;

                                Height = 122;
                                StartButton.Visibility = Visibility.Visible;
                                Bar.Visibility = Visibility.Visible;
                            }));
                        }
                    }

                }

            }
        }

    }
}
