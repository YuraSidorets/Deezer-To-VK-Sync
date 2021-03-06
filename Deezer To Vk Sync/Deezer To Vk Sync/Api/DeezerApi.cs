﻿using System.Net;
using System.Xml;

namespace Deezer_To_Vk_Sync.Api
{
    public class DeezerApi
    {
        public string _accessToken;

        public string link =
            @"http://connect.deezer.com/oauth/auth.php?app_id=168655&redirect_uri=http://deezer.com&perms=manage_library,listening_history&response_type=token";

        public XmlDocument GetUserPlaylist()
        {
            WebClient client = new WebClient();
            XmlDocument result = new XmlDocument();

            string output = client.DownloadString("http://api.deezer.com/playlist/1457009175&output=xml");
         
            result.LoadXml(output);
            return result;
        }


        public string[] ParsePlaylist(XmlDocument input)
        {
            XmlNodeList ArtistList = input.SelectNodes("root/tracks/data/track/artist/name");
            XmlNodeList TitleList = input.SelectNodes("root/tracks/data/track/title");

            string[] output = new string[ArtistList.Count];

            for (int i = 0; i < ArtistList.Count; i++)
            {
                output[i] = ArtistList[i].InnerText.ToLower() + " - " + TitleList[i].InnerText.ToLower();
            }

            return output;
        }

    }
}