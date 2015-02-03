using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InstaBackup.Impl
{
    public class MediaItem
    {
        public string Id { get; set; } // not used
        public string ThumbnailUrl { get; set; } // not used
        public string StandardUrl { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; } // image/video
        public DateTime Created { get; set; }
        public string Link { get; set; }

        public string GetFilenameForZip()
        {
            return string.Format("{0}-{1:D2}-{2:D2}-{3:D2}{4:D2}{5:D2}.{6}", Created.Year, Created.Month, Created.Day, Created.Hour, Created.Minute, Created.Second, (Type == "video") ? "mp4" : "jpg");
        }

        public bool MediaDownloadSuccessful { get; set; }

        public MemoryStream DownloadStandardMedia()
        {
            Debug.WriteLine("Getting media: {0}", Caption);
            var memoryStream = new MemoryStream();
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(StandardUrl);
                request.Proxy = null;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                request.Referer = "http://www.google.com";

                using (var response = request.GetResponse())
                {
                    using (var dataStream = response.GetResponseStream())
                    {
                        dataStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        MediaDownloadSuccessful = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error:", ex.ToString());
                memoryStream = null;
                MediaDownloadSuccessful = false;
            }

            return memoryStream;
        }
    }
}
