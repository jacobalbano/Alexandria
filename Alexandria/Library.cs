using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Alexandria.Loaders;
using Alexandria.FileStores;

namespace Alexandria
{
    public sealed partial class Library : IDisposable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Library()
        {
        }

        /// <summary>
        /// Add a filestore to this library.
        /// </summary>
        public void AddFileStore(IFileStore fileStore)
        {
            if (fileStore == null)
                throw new ArgumentNullException(nameof(fileStore));

            fileStores.Add(fileStore);
        }

        /// <summary>
        /// Add a filestore factory to this library.
        /// </summary>
        public void AddFileStoreFactory(IFileStoreFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            fileStoreFactories.Add(factory);
        }

        /// <summary>
        /// Add a loader to this factory. Only one loader can be registered for each type.
        /// </summary>
        public void AddLoader<T>(Loader<T> loader)
        {
            // it's not safe to use the 'throw' expression here
            #pragma warning disable IDE0016
            if (loader == null)
                throw new ArgumentNullException(nameof(loader));
            #pragma warning restore IDE0016

            if (loaders.ContainsKey(typeof(T)))
                throw new Exception($"A content loader for type {typeof(T)} already exists!");

            loaders[loader.ResourceType] = loader;
        }

        /// <summary>
        /// Load a resource of type T from the library, given a path.
        /// </summary>
        /// <typeparam name="T">The type to load.</typeparam>
        /// <param name="localFullPath">Path to the desired resource, relative to the filestore it's contained in.</param>
        /// <returns>The loaded resource.</returns>
        public T Load<T>(string localFullPath)
        {
            if (localFullPath == null)
                throw new ArgumentNullException(nameof(localFullPath));

            var cache = cachePartition.EstablishCache<T>();
            if (cache.TryGetValue(localFullPath, out var resource))
                return (T) resource.Item;
            
            if (!loaders.TryGetValue(typeof(T), out var loader))
                throw new Exception($"No content loader found for type {typeof(T).Name}");

            using (var stream = FindStreamFromPath(localFullPath, out var storeLocalPath, out var fileStore))
            {
                var item = (T)loader.LoadFromStream(stream);
                cache[localFullPath] = resource = new Resource<T>(localFullPath, item);

                if (loader is ReloadableLoader<T> reloader && fileStore is IReloadableFileStore store)
                    store.AddWatch(localFullPath, s => reloader.UpdateFromStream(item, s));

                return item;
            }
        }

        /// <summary>
        /// Yields an enumerable containing the names of all subdirectories of the provided path
        /// across all FileStores that have been added.
        /// - If the path is null, the root of the FileStore will be used.
        /// - Results are distinct.
        /// </summary>
        /// <param name="localFullRootPath"></param>
        public IEnumerable<string> EnumerateDirectories(string localFullRootPath = null)
        {
            var paths = fileStores
                .SelectMany(s => s.EnumerateDirectories(localFullRootPath));

            if (localFullRootPath != null)
                paths = paths.Concat(EnumerateDirectoriesInNestedFilestores(localFullRootPath));

            foreach (var path in paths.Distinct())
                yield return path;
        }

        /// <summary>
        /// Yields an enumerable containing the names of all files found in provided path
        /// across all FileStores that have been added.
        /// - If the path is null, the root of the FileStore will be used.
        /// - Results are distinct.
        /// </summary>
        /// <param name="localFullRootPath"></param>
        public IEnumerable<string> EnumerateFiles(string localFullRootPath = null)
        {
            var paths = fileStores
                .SelectMany(s => s.EnumerateFiles(localFullRootPath));

            if (localFullRootPath != null)
                paths = paths.Concat(EnumerateFilesInNestedFilestores(localFullRootPath));

            foreach (var path in paths.Distinct())
                yield return path;
        }

        private IEnumerable<string> EnumerateDirectoriesInNestedFilestores(string path)
        {
            var pathEnumerator = new PathFragmentEnumerator(path);
            return EnumerateNestedFilestores(path, pathEnumerator)
                .SelectMany(s => s.EnumerateDirectories(null))
                .Select(s => FormatRootPath(path) + s);
        }

        private IEnumerable<string> EnumerateFilesInNestedFilestores(string path)
        {
            var pathEnumerator = new PathFragmentEnumerator(path);
            return EnumerateNestedFilestores(path, pathEnumerator)
                .SelectMany(s => s.EnumerateFiles(null))
                .Select(s => FormatRootPath(path) + s);
        }

        private IEnumerable<IFileStore> EnumerateNestedFilestores(string fullPath, PathFragmentEnumerator fragmentEnumerator, List<IFileStore> fileStoresToSearch = null)
        {
            fileStoresToSearch = fileStoresToSearch ?? fileStores;
            var search = new List<IFileStore>(fileStoresToSearch.Count);
            search.AddRange(fileStoresToSearch);

            var localPath = string.Empty;
            while (fragmentEnumerator.MoveNext())
            {
                localPath += fragmentEnumerator.Current;

                var fileFrontier = search
                    .Where(s => s.FileExists(localPath))
                    .ToList();

                if (fileFrontier.Count == 0)
                {
                    var dirFrontier = search
                        .Where(s => s.DirectoryExists(localPath))
                        .ToList();

                    //  no FileStore found that includes this path as a file, but it might be a directory
                    if (dirFrontier.Count == 0)
                        return Enumerable.Empty<IFileStore>();
                }
                else
                {
                    var newFileStoreFrontier = fileFrontier
                        .Select(s => s.OpenFileEntryStream(localPath))
                        .Select(s => EstablishFileStore(localPath, s))
                        .ToList();

                    if (fragmentEnumerator.HasNext)
                    {
                        //  there's still more to the path
                        //  recurse and try to find it inside the file we've found
                        return EnumerateNestedFilestores(fullPath, fragmentEnumerator, newFileStoreFrontier);
                    }
                    else
                    {
                        return newFileStoreFrontier;
                    }
                }

                localPath += "/";
            }

            return Enumerable.Empty<IFileStore>();
        }

        private Stream FindStreamFromPath(string path, out string storeLocalPath, out IFileStore fileStore)
        {
            var pathEnumerator = new PathFragmentEnumerator(path);

            try { return FindStream(path, pathEnumerator, out storeLocalPath, out fileStore); }
            catch (Exception) { throw new Exception($"The file '{path}' could not be found"); }
        }

        private Stream FindStream(string fullPath, PathFragmentEnumerator fragmentEnumerator, out string storeLocalPath, out IFileStore fileStore, List<IFileStore> fileStoresToSearch = null)
        {
            fileStoresToSearch = fileStoresToSearch ?? fileStores;

            var search = new List<IFileStore>(fileStoresToSearch.Count);
            search.AddRange(fileStoresToSearch);

            var localPath = string.Empty;
            while (fragmentEnumerator.MoveNext())
            {
                localPath += fragmentEnumerator.Current;

                var dirFrontier = search
                    .Where(s => s.DirectoryExists(localPath))
                    .ToList();

                if (dirFrontier.Count == 0)
                {
                    //  no FileStore found that includes this path as a directory, but it might be a file
                    var fileFrontier = search
                        .Where(s => s.FileExists(localPath))
                        .ToList();

                    //  none of our FileStores have this file
                    if (fileFrontier.Count == 0)
                        throw new Exception("Couldn't find the file");

                    if (fragmentEnumerator.HasNext)
                    {
                        //  there's still more to the path
                        //  recurse and try to find it inside the file we've found
                        var newFileStoreFrontier = fileFrontier
                            .Select(s => s.OpenFileEntryStream(localPath))
                            .Select(s => EstablishFileStore(localPath, s))
                            .ToList();

                        return FindStream(fullPath, fragmentEnumerator, out storeLocalPath, out fileStore, newFileStoreFrontier);
                    }
                    else
                    {
                        storeLocalPath = localPath;
                        fileStore = fileFrontier.Last();
                        return fileStore.OpenFileEntryStream(localPath);
                    }
                }

                localPath += "/";
            }

            throw new Exception("Couldn't find the file");
        }
        
        private IFileStore EstablishFileStore(string fullPath, Stream stream)
        {
            if (!subFileStoreCache.TryGetValue(fullPath, out var result))
            {
                try
                {
                    foreach (var factory in fileStoreFactories)
                    {
                        if (factory.IsCandidate(fullPath))
                        {
                            result = factory.Create(stream);
                            break;
                        }
                    }

                    streams.Add(stream);
                    subFileStoreCache[fullPath] = result;
                    return result;
                }
                catch (Exception e)
                {
                    stream.Dispose();
                    throw new Exception("Failed to open FileStore", e);
                }
            }

            return result;
        }
        
        private string FormatRootPath(string localFullPath)
        {
            if (localFullPath == null) return string.Empty;

            if (!localFullPath.EndsWith("/")) localFullPath += "/";
            return localFullPath;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var factory in fileStoreFactories)
                        factory.Dispose();

                    foreach (var store in fileStores)
                        store.Dispose();

                    foreach (var partition in cachePartition.Values)
                        partition.Dispose();

                    foreach (var loader in loaders.Values)
                        loader.Dispose();

                    foreach (var stream in streams)
                        stream.Dispose();
                }
                
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        private List<IFileStore> fileStores = new List<IFileStore>();
        private List<IFileStoreFactory> fileStoreFactories = new List<IFileStoreFactory>();
        private Dictionary<string, IFileStore> subFileStoreCache = new Dictionary<string, IFileStore>();

        private Dictionary<Type, ILoader> loaders = new Dictionary<Type, ILoader>();
        private ResourceCachePartition cachePartition = new ResourceCachePartition();
        private List<Stream> streams = new List<Stream>();
    }
}
