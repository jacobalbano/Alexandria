using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alexandria.FileStores
{
    /// <summary>
    /// Provides access to files existing within a specified directory or its subfolders (recursively)
    /// Does NOT allow access to relative or absolute paths
    /// </summary>
    public sealed class RootDirectoryFileStore : Library.IReloadableFileStore
    {
        /// <summary>
        /// Root directory of this filestore. Immutable.
        /// </summary>
        public string RootDirectory { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rootDirectory">The directory to search for paths relative to</param>
        public RootDirectoryFileStore(string rootDirectory)
        {
            RootDirectory = rootDirectory;
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
        public IEnumerable<string> EnumerateDirectories(string localFullRootPath)
        {
            localFullRootPath = localFullRootPath ?? ".";
            if (!ValidatePath(localFullRootPath))
                yield break;

            localFullRootPath = Resolve(localFullRootPath);
            if (!Directory.Exists(localFullRootPath))
                yield break;

            foreach (var d in Directory.EnumerateDirectories(localFullRootPath))
                yield return NormalizePath(d);
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
        public IEnumerable<string> EnumerateFiles(string localFullRootPath)
        {
            localFullRootPath = localFullRootPath ?? ".";
            if (!ValidatePath(localFullRootPath))
                yield break;

            localFullRootPath = Resolve(localFullRootPath);
            if (!Directory.Exists(localFullRootPath))
                yield break;

            foreach (var f in Directory.EnumerateFiles(localFullRootPath))
                yield return NormalizePath(f);
        }

        /// <summary>
        /// Return whether a directory exists with the given path.
        /// </summary>
        /// <param name="localFullPath">The full path relative to the root directory.</param>
        public bool DirectoryExists(string localFullPath)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            return ValidatePath(localFullPath) && Directory.Exists(Resolve(localFullPath));
        }

        /// <summary>
        /// Return whether a file exists with the given path.
        /// </summary>
        /// <param name="localFullPath">The full path relative to the root directory.</param>
        public bool FileExists(string localFullPath)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            return ValidatePath(localFullPath) && File.Exists(Resolve(localFullPath));
        }

        /// <summary>
        /// Open a stream containing file data.
        /// </summary>
        /// <param name="localFullPath">The full path relative to the root directory.</param>
        /// <returns>The resulting stream.</returns>
        public Stream OpenFileEntryStream(string locaFullPath)
        {
            return File.OpenRead(Path.Combine(RootDirectory, locaFullPath));
        }
        
        void Library.IReloadableFileStore.AddWatch(string localFullPath, Action<Stream> reloadAction)
        {
            var finfo = new FileInfo(Resolve(localFullPath));
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

        private string Resolve(string localFullPath)
        {
            return Path.Combine(RootDirectory, localFullPath);
        }

        private bool ValidatePath(string path)
        {
            return !(Path.IsPathRooted(path) || path.Contains(".."));
        }

        private string NormalizePath(string path)
        {
            return path
                .Substring(RootDirectory.Length)
                .TrimStart(trimChars)
                .Replace('\\', '/');
        }

        private readonly char[] trimChars = "\\/.".ToCharArray();
        
        void IDisposable.Dispose()
        {
        }
    }
}
