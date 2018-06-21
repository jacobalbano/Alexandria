using Alexandria;
using Alexandria.FileStores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class LibraryWithZipTests
    {
        private Library lib;

        [TestInitialize]
        public void Init()
        {
            lib = new Library();

            lib.AddFileStore(new ZipFileStore(@"..\..\TestRoot\zip2.zip"));
            lib.AddFileStoreFactory(new ZipFileStore.Factory());
        }

        [TestMethod]
        public void TopLevelFilesMatchExpected()
        {
            CollectionAssert.AreEquivalent(
                new[] { "nested1.zip", "nested2.zip" },
                lib.EnumerateFiles(null).ToArray());
        }

        [TestMethod]
        public void FilesInFirstNestedZipMatchExpected()
        {
            CollectionAssert.AreEquivalent(
                new[] { "nested1.zip/file1.txt", "nested1.zip/file2.txt", "nested1.zip/file3.txt" },
                lib.EnumerateFiles("nested1.zip").ToArray());
        }

        [TestMethod]
        public void FilesInTwiceNestedZipMatchExpected()
        {
            CollectionAssert.AreEquivalent(
                new[] { "nested2.zip/nestedDeeper.zip/file1.txt" },
                lib.EnumerateFiles("nested2.zip/nestedDeeper.zip").ToArray());
        }

        [TestMethod]
        public void FilesInFirstNestedZipAndSameNamedFolderMatchExpected()
        {
            // ensure that a folder named 'nested1.zip' will be enumerated in addition to the archive 'nested1.zip'
            lib.AddFileStore(new RootDirectoryFileStore(@"..\..\TestRoot"));

            CollectionAssert.AreEquivalent(
                new[] { "nested1.zip/file1.txt", "nested1.zip/file2.txt", "nested1.zip/file3.txt", "nested1.zip/file4.txt" },
                lib.EnumerateFiles("nested1.zip").ToArray());
        }
    }
}
