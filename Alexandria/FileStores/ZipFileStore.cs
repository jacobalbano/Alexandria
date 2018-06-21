using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Alexandria.FileStores
{
    /// <summary>
    /// Provides access to files inside a zip archive
    /// Paths are compared case-insensitively
    /// Does NOT support archive passwords
    /// </summary>
    public sealed class ZipFileStore : Library.IFileStore
    {
        /// <summary>
        /// Factory for creating new zip filestores on the fly when iterating a path.
        /// </summary>
        public class Factory : Library.FileStoreFactory<ZipFileStore>
        {
            public override bool IsCandidate(string fileName)
            {
                return fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
            }

            public override ZipFileStore Create(Stream fileStoreStream)
            {
                return new ZipFileStore(fileStoreStream);
            }
        }

        /// <summary>
        /// Constructor. Loads a zip file from a physical path on disk.
        /// </summary>
        /// <param name="path">Full path to the zip. Must be on the filesystem directly; cannot exist inside of an archive.</param>
        public ZipFileStore(string path) : this(File.OpenRead(path))
        {
        }

        /// <summary>
        /// Constructor. Loads a zip file from a stream.
        /// </summary>
        /// <param name="zipStream">The stream to read from. Does NOT dispose the stream. The stream must NOT be disposed while this object is alive.</param>
        public ZipFileStore(Stream zipStream)
        {
            zip = new System.IO.Compression.ZipArchive(zipStream);
            var entries = zip.Entries.Select(e => e.FullName);
            var files = zip.Entries
                .Select(e => e.FullName)
                .Where(s => !s.EndsWith("/"));

            FileEntries = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);

            var dirs = FileEntries
                .Select(s => new { Str = s, Slash = s.LastIndexOf("/") })
                .Where(x => x.Slash >= 0)
                .Select(x => x.Str.Substring(0, x.Slash));

            Directories = new HashSet<string>(dirs, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Return whether a directory exists with the given path.
        /// </summary>
        /// <param name="localFullPath">The full path relative to the root of this zip archive.</param>
        public bool DirectoryExists(string localFullPath)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            return Directories.Any(s => s.StartsWith(FormatRootPath(localFullPath)));
        }

        /// <summary>
        /// Return whether a file exists with the given path.
        /// </summary>
        /// <param name="localFullPath">The full path relative to the root of this zip archive.</param>
        public bool FileExists(string localFullPath)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            return FileEntries.Contains(localFullPath);
        }

        /// <summary>
        /// Open a stream containing file data.
        /// </summary>
        /// <param name="localFullPath">The full path relative to the root of this zip archive.</param>
        /// <returns>The resulting stream.</returns>
        public Stream OpenFileEntryStream(string localFullPath)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            return zip.GetEntry(localFullPath).Open();
        }

        /// <summary>
        /// Return an enumeration over directories that exist as immediate descendents of the given directory
        /// If localFullRootPath is null, return the directories in the root of the archive
        /// Otherwise, return the full path of each directory in the supplied path
        /// 
        /// - Each returned string must begin with the value of localFullRootPath
        /// - Trailing spaces must be trimmed
        /// </summary>
        /// <param name="localFullRootPath">The (optional) path to search for directories in</param>
        public IEnumerable<string> EnumerateDirectories(string localFullRootPath)
        {
            var distinct = new HashSet<string>();

            localFullRootPath = FormatRootPath(localFullRootPath);
            var dirs = Directories
                .Where(s => s.StartsWith(localFullRootPath, StringComparison.OrdinalIgnoreCase));

            foreach (var dir in dirs)
            {
                var item = dir;

                //  if there's more than one slash, this is a subdirectory
                var remainder = item.Substring(localFullRootPath.Length);
                var slash = remainder.IndexOf("/");
                if (slash >= 0)
                    item = item.Substring(0, slash);

                if (distinct.Add(item))
                    yield return item;
            }
        }

        /// <summary>
        /// Return an enumeration over files that exist as immediate descendents of the given directory
        /// If localFullRootPath is null, return the files in the root of the archive
        /// Otherwise, return the full path of each directory in the supplied path
        /// 
        /// - Each returned string must begin with the value of localFullRootPath
        /// - Trailing spaces must be trimmed
        /// </summary>
        /// <param name="localFullRootPath">The (optional) path to search for files in</param>
        public IEnumerable<string> EnumerateFiles(string localFullRootPath)
        {
            localFullRootPath = FormatRootPath(localFullRootPath);
            var files = FileEntries
                .Where(s => s.StartsWith(localFullRootPath, StringComparison.OrdinalIgnoreCase));

            foreach (var file in files)
            {
                var remainder = file.Substring(localFullRootPath.Length);
                if (remainder.Any(c => c == '/')) continue;
                yield return file;
            }
        }

        private string FormatRootPath(string localFullPath)
        {
            if (localFullPath == null) return string.Empty;

            if (!localFullPath.EndsWith("/")) localFullPath += "/";
            return localFullPath;
        }

        private HashSet<string> FileEntries { get; }
        private HashSet<string> Directories { get; }
        private System.IO.Compression.ZipArchive zip;

        #region IDisposable Support
        private bool disposedValue = false;
        
        public void Dispose()
        {
            if (!disposedValue)
            {
                zip.Dispose();
                disposedValue = true;
            }
        }
        #endregion
    }
}
