using System;
using System.Collections.Generic;
using System.IO;

namespace Alexandria
{
    public partial class Library
    {
        public interface IReloadableFileStore : IFileStore
        {
            /// <summary>
            /// Add a watcher for this file. 
            /// This method is permitted to do nothing.
            /// </summary>
            /// <param name="localFullPath">The path of the file relative to this FileStore</param>
            /// <param name="reloadAction">The action that will be called with the new stream on reload</param>
            void AddWatch(string localFullPath, Action<Stream> reloadAction);
        }

        /// <summary>
        /// Describes a class that provides information about files and directories in a given context
        /// Examples include folders directly on the filesystem, entries inside a zip archive, or data from a remote server
        /// Paths are expected to use the forward slash '/' (char code 47) as the directory separator -- otherwise path distinction may not work
        /// </summary>
        public interface IFileStore : IDisposable
        {
            Stream OpenFileEntryStream(string localFullPath);

            /// <summary>
            /// Return an enumeration over directories that exist as immediate descendents of the given directory
            /// If localFullRootPath is null, return the directories in the root of the FileStore
            /// Otherwise, return the full path of each directory in the supplied path
            /// 
            /// - Each returned string must begin with the value of localFullRootPath
            /// - Trailing spaces must be trimmed
            /// </summary>
            /// <param name="localFullRootPath">The (optional) path to search for directories in</param>
            IEnumerable<string> EnumerateDirectories(string localFullRootPath = null);

            /// <summary>
            /// Return an enumeration over directories that exist as immediate descendents of the given directory
            /// If localFullRootPath is null, return the directories in the root of the FileStore
            /// Otherwise, return the full path of each directory in the supplied path
            /// 
            /// - Each returned string must begin with the value of localFullRootPath
            /// - Trailing spaces must be trimmed
            /// </summary>
            /// <param name="localFullRootPath">The (optional) path to search for directories in</param>
            IEnumerable<string> EnumerateFiles(string localFullRootPath = null);

            /// <summary>
            /// Given a full path from the root of this FileStore, determine whether a file exists at that path.
            /// </summary>
            bool FileExists(string localFullPath);

            /// <summary>
            /// Given a full path from the root of this FileStore, determine whether a directory exists at that path.
            /// </summary>
            bool DirectoryExists(string localFullPath);

        }

        /// <summary>
        /// Describes a class that can be used to create instances of a FileStore
        /// Can be used to descend into nested archives.
        /// It is recommended to use the abstract class Library.FileStoreFactory instead of this interface
        /// </summary>
        public interface IFileStoreFactory : IDisposable
        {
            /// <summary>
            /// Given a path from the root of the containing FileStore,
            /// determine whether this factory can be used to create a FileStore from the given file
            /// </summary>
            bool IsCandidate(string localFullPath);

            /// <summary>
            /// Create an instance of the FileStore given a stream
            /// </summary>
            /// <param name="fileStoreStream">
            /// The stream containing data to create the FileStore from.
            /// Do NOT dispose this stream!
            /// </param>
            /// <returns>An instance of the FileStore</returns>
            IFileStore Create(Stream fileStoreStream);
        }
        
        /// <summary>
        /// Base class to cut down on boilerplate for implementing IFileStoreFactory
        /// </summary>
        /// <typeparam name="T">FileStore type for this factory to create</typeparam>
        public abstract class FileStoreFactory<T> : IFileStoreFactory where T : IFileStore
        {
            public abstract bool IsCandidate(string fileName);
            public abstract T Create(Stream fileStoreStream);

            IFileStore IFileStoreFactory.Create(Stream fileStoreStream) => Create(fileStoreStream);

            #region IDisposable Support
            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }
            #endregion
        }
    }
}
