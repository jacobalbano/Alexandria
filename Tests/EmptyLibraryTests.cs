using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alexandria;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class EmptyLibraryTests
    {
        private Library lib;

        [TestInitialize]
        public void Init()
        {
            lib = new Library();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]
        public void ErrorOnNullFilestore()
        {
            lib.AddFileStore(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]
        public void ErrorOnNullFilestoreFactory()
        {
            lib.AddFileStoreFactory(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]
        public void ErrorOnNullLoader()
        {
            lib.AddLoader<string>(null);
        }

        [TestMethod]
        public void NoResultsFromEnumerateFiles()
        {
            Assert.AreEqual(0, lib.EnumerateFiles().Count());
        }

        [TestMethod]
        public void NoResultsFromEnumerateDirectories()
        {
            Assert.AreEqual(0, lib.EnumerateDirectories().Count());
        }
    }
}
