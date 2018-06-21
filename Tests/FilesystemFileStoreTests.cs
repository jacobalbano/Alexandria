using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alexandria;
using Alexandria.FileStores;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class FilesystemFileStoreTests
    {
        private FilesystemFileStore filestore;

        [TestInitialize]
        public void Init()
        {
            filestore = new FilesystemFileStore();
        }

        [TestMethod]
        public void TopLevelFilesMatchExpected()
        {
            CollectionAssert.AreEquivalent(
                new[] { "../../TestRoot/FS/file1.txt", "../../TestRoot/FS/file2.txt", "../../TestRoot/FS/file3.txt" },
                filestore.EnumerateFiles("../../TestRoot/FS").ToArray());
        }

        [TestMethod]
        public void TopLevelDirectoriesMatchExpected()
        {
            CollectionAssert.AreEquivalent(
                new[] { "../../TestRoot/FS/dir1", "../../TestRoot/FS/dir2", "../../TestRoot/FS/dir3" },
                filestore.EnumerateDirectories("../../TestRoot/FS").ToArray());
        }

        [TestMethod]
        public void NoDirectoriesInSubDir()
        {
            Assert.AreEqual(0, filestore.EnumerateDirectories("../../TestRoot/FS/dir1").Count());
        }
    }
}
