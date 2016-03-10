using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;

namespace Deezer_To_Vk_Sync.Api
{
  
    public class VkApi
    {  
        public int appId = 5172405;
        public int scope = (int)(VkontakteScopeList.audio | VkontakteScopeList.notify);
        public string _accessToken;
        public int userId = 0;
        private XmlDocument audio;
              
        private enum VkontakteScopeList
        {
            /// <summary>
            /// Пользователь разрешил отправлять ему уведомления. 
            /// </summary>
            notify = 1,
            /// <summary>
            /// Доступ к аудиозаписям. 
            /// </summary>
            audio = 8,
            
        }
        
        public XmlDocument ExecuteCommand(string name, NameValueCollection qs)
        {
            XmlDocument result = new XmlDocument();
            result.Load(
                $"https://api.vkontakte.ru/method/{name}.xml?&v=5.40&access_token={_accessToken}&{String.Join("&", from item in qs.AllKeys select item + "=" + qs[item])}");
            return result;
        }

        public string GetDataFromXmlNode(XmlNode input)
        {
            if (input == null || String.IsNullOrEmpty(input.InnerText))
            {
                return "нет данных";
            }
            else
            {
                return input.InnerText;
            }
        }

        public XmlDocument SetAudio(string id, string owner)
        {

            NameValueCollection qs = new NameValueCollection();
            qs["audio_id"] = id;
            qs["owner_id"] = owner;

            return ExecuteCommand("audio.add", qs);
        }

        public XmlDocument SearchAudio(string name)
        {

            NameValueCollection qs = new NameValueCollection();
            qs["q"] = name;
            qs["auto_complete"] = "1";
            
            return ExecuteCommand("audio.search", qs);
        }

        public XmlDocument GetAudio()
        {

            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = userId.ToString();
            qs["count"] = "6000";

            return ExecuteCommand("audio.get", qs);
        }

        public string[] ParseAudioList(XmlDocument input)
        {
            XmlNodeList ArtistList = input.SelectNodes("response/items/audio/artist");
            XmlNodeList TitleList = input.SelectNodes("response/items/audio/title");

            string[] output = new string[ArtistList.Count];

            for (int i = 0; i < ArtistList.Count; i++)
            {
                output[i] = ArtistList[i].InnerText.ToLower() + " - " + TitleList[i].InnerText.ToLower();
            }

            return output;
        }

        public KeyValuePair<string,string> ParseSearchList(XmlDocument input)
        {
           
                XmlNodeList idList = input.SelectNodes("response/items/audio/id");
                XmlNodeList ownerList = input.SelectNodes("response/items/audio/owner_id");

                if( idList[0] != null & ownerList[0] != null)
                return new KeyValuePair<string, string>(idList[0].InnerText, ownerList[0].InnerText);
            return new KeyValuePair<string, string>("0", "0");
        }
    }
    
}
