using System;
using InstaBackup.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstaBackup.Test
{
    [TestClass]
    public class TestBackupProcessor
    {
        [TestMethod]
        public void TestGetWebRequest()
        {
            var processor = new InstagramUtil();
            var response = processor.GetRawHTMLResponseFromInstagram("nicholaslamb");

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public void TestGetData()
        {
            var processor = new InstagramUtil();
            var response = processor.GetDataFromInstagram("nicholaslamb", -1);

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public void TestSaveZip()
        {
            var processor = new BackupProcessor("nicholaslamb");
            processor.SaveZipToDisk(-1, @"C:\temp\test.zip");
        }
    }
}
