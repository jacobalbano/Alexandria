using Alexandria.FileStores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class ZipFileStoreTests
    {
        private ZipFileStore.Factory factory;
        private ZipFileStore filestore;

        [TestInitialize]
        public void Init()
        {
            factory = new ZipFileStore.Factory();
            filestore = factory.Create(File.OpenRead(@"..\..\TestRoot\zip1.zip"));
        }

        [TestMethod]
        public void TopLevelFilesMatchExpected()
        {
            CollectionAssert.AreEqual(
                new[] { "file1.txt", "file2.txt", "file3.txt" },
                filestore.EnumerateFiles(null).ToArray());
        }

        [TestMethod]
        public void TopLevelDirectoriesMatchExpected()
        {
            CollectionAssert.AreEqual(
                new[] { "dir1", "dir2", "dir3" },
                filestore.EnumerateDirectories(null).ToArray());
        }

        [TestMethod]
        public void NoDirectoriesInSubDir()
        {
            Assert.AreEqual(0, filestore.EnumerateDirectories("dir1").Count());
        }
    }
}
