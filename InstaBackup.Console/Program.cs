using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstaBackup.Impl;

namespace InstaBackup.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                var path = args[0];
                var username = args[1];

                System.Console.WriteLine("Starting Backup Process for {0} to {1}...", username, path);
                var processor = new BackupProcessor(username);
                processor.SaveZipToDisk(-1, path);
                System.Console.WriteLine("Finished With Backup.");
            }
            else
            {
                System.Console.WriteLine("Usage: InstaBackup.Console.exe <full-zip-path> <instagram-username>");
                System.Console.WriteLine("Example: InstaBackup.Console.exe C:\\Temp\\nicholaslamb.zip nicholaslamb");
            }
        }
    }
}
