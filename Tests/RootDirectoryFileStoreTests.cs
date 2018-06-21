using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alexandria;
using Alexandria.FileStores;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class RootDirectoryFileStoreTests
    {
        private RootDirectoryFileStore filestore;

        [TestInitialize]
        public void Init()
        {
            filestore = new RootDirectoryFileStore(@"..\..\TestRoot\FS");
        }

        [TestMethod]
        public void TopLevelFilesMatchExpected()
        {
            CollectionAssert.AreEquivalent(
                new[] { "file1.txt", "file2.txt", "file3.txt" },
                filestore.EnumerateFiles(null).ToArray());
        }

        [TestMethod]
        public void TopLevelDirectoriesMatchExpected()
        {
            CollectionAssert.AreEquivalent(
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
