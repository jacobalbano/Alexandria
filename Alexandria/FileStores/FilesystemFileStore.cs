using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alexandria.FileStores
{
    /// <summary>
    /// Simple filestore that allows direct access to the filesystem.
    /// Paths are sourced relative to the working directory.
    /// Disk-qualified paths as well as relative paths are all allowed.
    /// </summary>
    public sealed class FilesystemFileStore : Library.IReloadableFileStore
    {
        /// <summary>
        /// Return whether a directory exists with the given path.
        /// </summary>
        /// <param name="localFullPath">The full path relative to the working directory.</param>
        public bool DirectoryExists(string localFullPath)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            return Directory.Exists(localFullPath);
        }

        /// <summary>
        /// Return whether a file exists with the given path.
        /// </summary>
        /// <param name="localFullPath">The full path relative to the working directory.</param>
        public bool FileExists(string localFullPath)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            return File.Exists(localFullPath);
        }

        /// <summary>
        /// Open a stream containing file data.
        /// </summary>
        /// <param name="localFullPath">The full path relative to the root of the working directory.</param>
        /// <returns>The resulting stream.</returns>
        public Stream OpenFileEntryStream(string localFullPath)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            return File.OpenRead(localFullPath);
        }

        /// <summary>
        /// Return an enumeration over directories that exist as immediate descendents of the given directory
        /// If localFullRootPath is null, return the directories in the working directory
        /// Otherwise, return the full path of each directory in the supplied path
        /// 
        /// - Each returned string must begin with the value of localFullRootPath
        /// - Trailing spaces must be trimmed
        /// </summary>
        /// <param name="localFullRootPath">The (optional) path to search for directories in</param>
        public IEnumerable<string> EnumerateDirectories(string localFullRootPath = null)
        {
            if (localFullRootPath != null && !Directory.Exists(localFullRootPath))
                return Enumerable.Empty<string>();

            var results = Directory.EnumerateDirectories(localFullRootPath ?? ".")
                .Select(NormalizePath)
                .Select(s => localFullRootPath == null ? s.Substring(2) : s); // trim off './' from beginning (if not explicitly provided)

            return results;
        }

        /// <summary>
        /// Return an enumeration over files that exist as immediate descendents of the given directory
        /// If localFullRootPath is null, return the files in the working directory
        /// Otherwise, return the full path of each directory in the supplied path
        /// 
        /// - Each returned string must begin with the value of localFullRootPath
        /// - Trailing spaces must be trimmed
        /// </summary>
        /// <param name="localFullRootPath">The (optional) path to search for directories in</param>
        public IEnumerable<string> EnumerateFiles(string localFullRootPath = null)
        {
            if (localFullRootPath != null && !Directory.Exists(localFullRootPath))
                return Enumerable.Empty<string>();

            var results = Directory.EnumerateFiles(localFullRootPath ?? ".")
                .Select(NormalizePath)
                .Select(s => localFullRootPath == null ? s.Substring(2) : s); // trim off './' from beginning (if not explicitly provided)

            return results;
        }

        void Library.IReloadableFileStore.AddWatch(string localFullPath, Action<Stream> reloadAction)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            if (reloadAction == null)
                throw new ArgumentNullException(nameof(reloadAction));

            var finfo = new FileInfo(localFullPath);
            var watcher = new FileSystemWatcher
            {
                Path = finfo.Directory.FullName,
                Filter = Path.GetFileName(finfo.FullName),
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            watcher.Changed += (s, e) => {
                using (var stream = OpenFileEntryStream(finfo.FullName))
                    reloadAction(stream);
            };
        }

        private string NormalizePath(string path)
        {
            return path
                .TrimStart(trimChars)
                .Replace('\\', '/');
        }

        private readonly char[] trimChars = "\\/".ToCharArray();

        void IDisposable.Dispose()
        {
        }
    }
}
