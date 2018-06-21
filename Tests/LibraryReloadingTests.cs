using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alexandria;
using System.IO;
using Alexandria.FileStores;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class LibraryReloadingTests
    {
        public TestContext TestContext { get; set; }

        private Library lib;

        [TestInitialize]
        public void Init()
        {
            lib = new Library();
            lib.AddFileStore(new RootDirectoryFileStore(TestContext.DeploymentDirectory));
            lib.AddLoader(new Loader());

            WriteText("Initial");
        }

        [TestMethod]
        public void ObjectUpdatesAfterChangeOnDisk()
        {
            var str = lib.Load<ReloadableString>("file.txt");
            Assert.AreEqual(str.Value, "Initial");

            WriteText("New");
            Thread.Sleep(10);

            Assert.AreEqual(str.Value, "New");
        }

        private void WriteText(string contents)
        {
            File.WriteAllText(Path.Combine(TestContext.DeploymentDirectory, "file.txt"), contents);
        }

        private class ReloadableString
        {
            public string Value { get; set; }
        }

        private class Loader : Library.ReloadableLoader<ReloadableString>
        {
            public override ReloadableString LoadFromStream(Stream dataStream, Library library)
            {
                return new ReloadableString { Value = Read(dataStream) };
            }

            private string Read(Stream dataStream)
            {
                using (var sr = new StreamReader(dataStream))
                    return sr.ReadToEnd();
            }

            public override void UpdateFromStream(ReloadableString item, Stream dataStream)
            {
                item.Value = Read(dataStream);
            }
        }
    }
}
