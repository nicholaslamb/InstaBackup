using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;

namespace InstaBackup.Impl
{
    public class BackupProcessor
    {
        public string InstagramUsername { get; set; }

        public BackupProcessor(string instagramUsername)
        {
            InstagramUsername = instagramUsername;
        }
        
        public void SaveZipToDisk(int numRecords, string fileName)
        {
            Console.WriteLine("Getting meta data from Instagram...");
            var data = new InstagramUtil().GetDataFromInstagram(InstagramUsername, numRecords);

            //var outputStream = new MemoryStream();
            var outputStream = File.Create(fileName);
            var zipOutputStream = new ZipOutputStream(outputStream);
            zipOutputStream.SetLevel(3); //0-9, 9 being the highest level of compression
            
            Console.WriteLine("About to download {0} media files...", data.Count);
            foreach (var d in data)
            {
                Console.WriteLine("Downloading media to {0}", d.GetFilenameForZip());
                var stream = d.DownloadStandardMedia();
                if(!d.MediaDownloadSuccessful)
                    continue;

                var entry = new ZipEntry(ZipEntry.CleanName(d.GetFilenameForZip()));
                entry.DateTime = d.Created;
                entry.Size = stream.Length;

                zipOutputStream.PutNextEntry(entry);
                StreamUtils.Copy(stream, zipOutputStream, new byte[4096]);
                zipOutputStream.CloseEntry();
                stream.Close();
            }
            zipOutputStream.IsStreamOwner = true;
            zipOutputStream.Close();

        }
    }
}
