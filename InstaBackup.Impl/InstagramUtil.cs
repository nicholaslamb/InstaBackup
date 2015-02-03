using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using Newtonsoft.Json.Linq;

namespace InstaBackup.Impl
{
    public class InstagramUtil
    {
        private int maxPerRequest = 20;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numRecords">-1 for all</param>
        public IList<MediaItem> GetDataFromInstagram(string instagramUsername, int numRecords)
        {
            var mediaItems = new List<MediaItem>();
            var maxId = "";

            var go = true;
            do
            {
                Debug.WriteLine("Getting Items maxId " + maxId);
                var json = GetJsonFromInstagram(instagramUsername, maxId);
                var items = ParseMediaItemsFromJson(json);
                mediaItems.AddRange(items);

                maxId = items.Last().Id;

                Debug.WriteLine("Got Items " + items.Count);

                if (numRecords != -1 && mediaItems.Count >= numRecords)
                    go = false;

                if (json["more_available"].ToString().ToLower() == "false")
                    go = false;
            } while (go);

            if (numRecords != -1)
                return mediaItems.Take(numRecords).ToList();

            return mediaItems;
        }

        public IList<MediaItem> ParseMediaItemsFromJson(JObject json)
        {
            var mediaItems = new List<MediaItem>();

            foreach (var i in json["items"])
            {
                var item = new MediaItem();

                item.Id = i["id"].ToString();
                if (i["caption"].HasValues)
                    item.Caption = i["caption"]["text"].ToString();
                item.Created = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(i["created_time"]));
                item.Link = i["link"].ToString();
                item.Type = i["type"].ToString();

                if (item.Type == "video")
                {
                    item.StandardUrl = i["videos"]["standard_resolution"]["url"].ToString();
                }
                else
                {
                    item.StandardUrl = i["images"]["standard_resolution"]["url"].ToString();
                }

                mediaItems.Add(item);
            }

            return mediaItems;
        }

        public JObject GetJsonFromInstagram(string instagramUsername, string maxId)
        {
            var url = string.Format("http://instagram.com/{0}/media", instagramUsername);
            if (!string.IsNullOrEmpty(maxId))
                url = string.Format("{0}?max_id={1}", url, maxId);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = null;
            AddSpoofHeaders(request, instagramUsername);

            using (var response = request.GetResponse())
            {
                // Get the stream containing content returned by the server.
                using (var dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    using (var reader = new StreamReader(dataStream))
                    {
                        // Read the content.
                        var responseFromServer = reader.ReadToEnd();

                        return JObject.Parse(responseFromServer);
                    }
                }
            }
        }

        public MemoryStream GetStreamFromUrl(string url)
        {
            var memoryStream = new MemoryStream();
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = null;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            request.Referer = "http://www.google.com";

            using (var response = request.GetResponse())
            {
                using (var dataStream = response.GetResponseStream())
                {
                    dataStream.CopyTo(memoryStream);
                }
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        private void AddSpoofHeaders(HttpWebRequest request, string instagramUsername)
        {
            //Accept:application/json, text/javascript, *\/*; q=0.01
            //Accept-Encoding:gzip, deflate, sdch
            //Accept-Language:en-US,en;q=0.8
            //Connection:keep-alive
            //Cookie:fbm_124024574287414=base_domain=.instagram.com; mid=VGUllAAEAAF3c0A_tS_j7j6aWaUi; ccode=US; __utma=227057989.1150533732.1420673194.1420673194.1420673194.1; __utmc=227057989; __utmz=227057989.1420673194.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); __utmt=1; __utma=1.1243575795.1415914899.1420674391.1420748586.14; __utmb=1.1.10.1420748586; __utmc=1; __utmz=1.1420674391.13.9.utmcsr=partner|utmccn=photo|utmcmd=embed; fbsr_124024574287414=1H7JKLnqaexyyhgGeu8cHMg3vqUDGijyNbp-DPvZw78.eyJhbGdvcml0aG0iOiJITUFDLVNIQTI1NiIsImNvZGUiOiJBUUJ0N1Y3R3hNb2RxV0lPMUFubElHWFFPNFdfREcyYVQ0VV93SG9Cdm9UTGYwN2JvMFdMRnp6ajNOb0wwcnc0N1ZjdEN2NWl5MThrUUlvTnZLdlNzUTlxbW9iRWVIY2J0cC13UXZXV2w4RUZBLTRuNG95NnpJbVB5MWk0SnhDa0Z2WVEyXzVOakp6cEVSeEd0cENGNEhZVENlODNYUU9YSjl2eXdkZ2tEeDR0XzRYQjhvYjNhSW1WdzFuM1hXRkE4dFZ5LTRQQnFudUxEZy1kWkdISnhVWFY5cjNEd0Rtc2h0a1V1a2R1eGlpemxaS0lOVzB5UjdrLXdlMlpyOEtjRWRvUEdvbERULXVrQVgtTDZudGFfcnZqMkE0bHhYY19valBvZkJzUDFJSEl4d2ZyTTk3enMtaVI3cFNyX1pPYXItcEdaVEpybWtzQk1fWGZWN2VVdkNvUSIsImlzc3VlZF9hdCI6MTQyMDc0ODU4NywidXNlcl9pZCI6IjEwMjA0MTA5In0; csrftoken=0fb4efe3ed2a49e6d00998f82160d7d7
            //Host:instagram.com
            //Referer:http://instagram.com/nicholaslamb/
            //User-Agent:Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36
            //X-Requested-With:XMLHttpRequest
            
            request.Accept = "Accept:application/json, text/javascript, */*; q=0.01";
            request.KeepAlive = true;
            request.Host = "instagram.com";
            request.Referer = string.Format("http://instagram.com/{0}/", instagramUsername);
            request.UserAgent= "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        }

        #region Old Stuff from HTML
        public string GetRawHTMLResponseFromInstagram(string instagramUsername)
        {
            var request = (HttpWebRequest)WebRequest.Create(string.Format("http://instagram.com/{0}", instagramUsername));
            request.Proxy = null;
            AddSpoofHeaders(request, instagramUsername);

            using (var response = request.GetResponse())
            {
                // Get the stream containing content returned by the server.
                using (var dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    using (var reader = new StreamReader(dataStream))
                    {
                        // Read the content.
                        var responseFromServer = reader.ReadToEnd();

                        return responseFromServer;
                    }
                }
            }
        }

        /// <summary>
        /// Use CsQuery to parse out the script tag where all the instagram data lives
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string GetRawDataStringFromFullHTML(string response)
        {
            CQ dom = response;

            var scriptTags = dom.Select("script").ToList();
            var rawData = "";

            foreach (var scriptTag in scriptTags)
            {
                if (scriptTag.InnerHTML.Contains("window._sharedData"))
                    rawData = scriptTag.InnerHTML;
            }

            rawData = rawData.Replace("window._sharedData =", "");
            rawData = rawData.Substring(0, rawData.LastIndexOf(";"));

            return rawData;
        }
        #endregion Old Stuff from HTML
    }
}
